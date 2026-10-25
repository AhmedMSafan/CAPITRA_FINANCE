<script setup lang="ts">
// Workflow step "Portfolio Cheque Coverage Review" (APEX App 105 page 213): enter the cheques of every investor kept
// in the deal, check they cover each investor's and the whole portfolio's outstanding balance, then accept or reject.
import { computed, reactive, ref, shallowRef, watch } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount, formatShort } from '@/shared/format/money'
import { useFeedback } from '@/shared/feedback/useFeedback'
import StepLayout from '../StepLayout.vue'
import { messageOf } from '../stepUtils'
import { chequeCoverageApi, type Cheque, type ChequeCoverage } from './chequeCoverage.api'

type Filter = 'ALL' | 'SHORT' | 'COVERED'

const props = defineProps<{ portfolioId: string }>()
const canAct = hasPermission('MTG.STEP.CHEQUE_COVERAGE')

const data = shallowRef<ChequeCoverage | null>(null)
const error = ref('')
const notice = ref('')
const busy = ref(false)
const filter = ref<Filter>('ALL')
const search = ref('')
const selectedId = ref<number | null>(null)

const id = computed(() => Number(props.portfolioId))
const editable = computed(() => canAct && !!data.value?.isOpen)

async function load() {
  try {
    data.value = await chequeCoverageApi.get(id.value)
    if (selectedId.value === null || !data.value.investors.some(i => i.investorId === selectedId.value))
      selectedId.value = data.value.investors.find(i => !i.isCovered)?.investorId ?? data.value.investors[0]?.investorId ?? null
  } catch (e) {
    error.value = messageOf(e, 'Unable to load the cheque coverage.')
  }
}

watch(id, () => {
  data.value = null
  error.value = ''
  selectedId.value = null
  load()
}, { immediate: true })

const date = (d: string | null) => (d ? formatDate(d + 'T00:00:00Z') : '-')
const pct = (part: number, whole: number) => (whole > 0 ? Math.min(100, (part / whole) * 100) : 100)

const investors = computed(() => {
  const q = search.value.trim().toLowerCase()
  return (data.value?.investors ?? []).filter(i => {
    if (filter.value === 'SHORT' && i.isCovered) return false
    if (filter.value === 'COVERED' && !i.isCovered) return false
    return !q || [i.investorName, i.investorCode].some(v => v.toLowerCase().includes(q))
  })
})

const totals = computed(() => {
  const all = data.value?.investors ?? []
  const outstanding = all.reduce((t, i) => t + i.outstandingAmount, 0)
  const cheques = all.reduce((t, i) => t + i.chequeAmount, 0)
  const covered = all.filter(i => i.isCovered).length
  return { count: all.length, covered, short: all.length - covered, outstanding, cheques, remaining: Math.max(0, outstanding - cheques) }
})

const rules = computed(() => [
  {
    label: 'Cover every investor',
    hint: 'The cheques of each investor must cover their outstanding balance.',
    ok: totals.value.count > 0 && totals.value.short === 0,
    detail: `${totals.value.covered} of ${totals.value.count} investors covered`
  },
  {
    label: 'Cover the portfolio',
    hint: 'The cheques of all investors together must cover the outstanding balance of the whole portfolio.',
    ok: totals.value.count > 0 && totals.value.cheques >= totals.value.outstanding - 0.005,
    detail: totals.value.remaining > 0 ? `${formatAmount(totals.value.remaining)} to reach full coverage` : 'Fully covered'
  }
])

const selected = computed(() => data.value?.investors.find(i => i.investorId === selectedId.value) ?? null)
const selectedCheques = computed(() => (data.value?.cheques ?? []).filter(c => c.investorId === selectedId.value))
const selectedAccounts = computed(() => (data.value?.accounts ?? []).filter(a => a.investorId === selectedId.value))

// ----- cheque form -----
const editing = shallowRef<Cheque | null>(null)
const formOpen = ref(false)
const formError = ref('')
const confirmDelete = ref(false)
const form = reactive({
  portfolioAccountId: '' as number | '',
  chequeNo: '',
  bankName: '',
  chequeDate: '',
  dueDate: '',
  chequeAmount: '' as number | '',
  isEndorsedToCompany: true,
  chequeStatusCode: '',
  clearingDate: '',
  returnReason: ''
})

function openForm(row: Cheque | null) {
  editing.value = row
  formError.value = ''
  confirmDelete.value = false
  const remaining = selected.value ? Math.max(0, selected.value.outstandingAmount - selected.value.chequeAmount) : 0
  Object.assign(form, row
    ? {
        portfolioAccountId: row.portfolioAccountId, chequeNo: row.chequeNo, bankName: row.bankName ?? '', chequeDate: row.chequeDate ?? '',
        dueDate: row.dueDate, chequeAmount: row.chequeAmount, isEndorsedToCompany: row.isEndorsedToCompany,
        chequeStatusCode: row.chequeStatusCode, clearingDate: row.clearingDate ?? '', returnReason: row.returnReason ?? ''
      }
    : {
        portfolioAccountId: selectedAccounts.value.length === 1 ? selectedAccounts.value[0]!.portfolioAccountId : '',
        chequeNo: '', bankName: '', chequeDate: '', dueDate: '', chequeAmount: remaining > 0 ? Math.round(remaining * 100) / 100 : '',
        isEndorsedToCompany: true, chequeStatusCode: data.value?.chequeStatuses.find(s => s.code === 'PENDING')?.code ?? data.value?.chequeStatuses[0]?.code ?? '',
        clearingDate: '', returnReason: ''
      })
  formOpen.value = true
}

async function saveForm() {
  busy.value = true
  formError.value = ''
  const body = {
    portfolioAccountId: Number(form.portfolioAccountId || 0),
    chequeNo: form.chequeNo,
    bankName: form.bankName || null,
    chequeDate: form.chequeDate || null,
    dueDate: form.dueDate || null,
    chequeAmount: form.chequeAmount === '' ? null : Number(form.chequeAmount),
    isEndorsedToCompany: form.isEndorsedToCompany,
    chequeStatusCode: form.chequeStatusCode || null,
    clearingDate: form.clearingDate || null,
    returnReason: form.returnReason || null
  }
  try {
    if (editing.value) await chequeCoverageApi.update(id.value, editing.value.chequeId, { ...body, rowVersion: editing.value.rowVersion })
    else await chequeCoverageApi.create(id.value, body)
    formOpen.value = false
    error.value = ''
    await load()
  } catch (e) {
    formError.value = messageOf(e, 'Unable to save the cheque.')
  } finally {
    busy.value = false
  }
}

async function removeCheque() {
  if (!editing.value) return
  if (!confirmDelete.value) {
    confirmDelete.value = true
    return
  }
  confirmDelete.value = false
  busy.value = true
  try {
    await chequeCoverageApi.remove(id.value, editing.value)
    formOpen.value = false
    error.value = ''
    await load()
  } catch (e) {
    formError.value = messageOf(e, 'Unable to delete the cheque.')
  } finally {
    busy.value = false
  }
}

// ----- accept / reject -----
const comment = ref('')

async function finish(action: 'ACCEPT' | 'REJECT') {
  busy.value = true
  error.value = ''
  notice.value = ''
  try {
    data.value = await chequeCoverageApi.decide(id.value, action, comment.value)
    notice.value = action === 'ACCEPT' ? 'Cheque coverage accepted.' : 'Cheque coverage rejected.'
    comment.value = ''
  } catch (e) {
    error.value = messageOf(e, 'Unable to record the decision.')
  } finally {
    busy.value = false
  }
}

// Messages open the app-wide pop-up; saving shows the progress screen.
useFeedback({
  errors: [error, formError],
  notices: [notice],
  busy,
  busyAs: () => ({ title: 'Saving the cheque coverage', kind: 'save', steps: ['Checking the cheques', 'Saving', 'Updating the portfolio'] })
})
</script>

<template>
  <div v-if="error && !data" class="cp-page"><div class="cp-error">{{ error }}</div></div>
  <div v-else-if="!data" class="cp-page"><div class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div></div>

  <StepLayout
    v-else
    :portfolio-id="data.portfolioId"
    title="Portfolio cheque coverage"
    lead="Enter the cheques of every investor kept in the deal, check they cover each investor's and the whole portfolio's outstanding balance, then accept or reject this workflow step."
    :portfolio-no="data.portfolioNo"
    :project-name="data.projectName"
    :project-code="data.projectCode"
    :status-name="data.portfolioStatusName"
    :stages="[
      { label: 'Enter cheques', state: data.cheques.length ? 'done' : 'now' },
      { label: 'Cover every investor', state: rules[0]!.ok ? 'done' : data.cheques.length ? 'now' : 'todo' },
      { label: 'Accept or reject', state: data.isOpen ? (rules[0]!.ok && rules[1]!.ok ? 'now' : 'todo') : 'done' }
    ]"
    apex-page="213"
  >
    <div v-if="data.rejectionReason && data.isOpen" class="rejected"><i class="pi pi-replay"></i> Last rejected: {{ data.rejectionReason }}</div>
    <div v-if="!data.isOpen" class="readonly"><i class="pi pi-eye"></i> Read-only view. Cheque coverage is not open for a portfolio in status {{ data.portfolioStatusName }}.</div>
    <div v-else-if="!canAct" class="readonly"><i class="pi pi-eye"></i> Read-only view. Entering cheques needs the cheque coverage permission.</div>

    <div class="grid">
      <div class="stack">
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-users"></i><h3>Investors</h3><span class="cp-spacer"></span><small class="cp-muted">Only investors included in the deal are counted.</small></div>
          <div class="tools">
            <label class="search"><i class="pi pi-search"></i><input v-model="search" type="search" placeholder="Search investor or code" aria-label="Search investors" /></label>
            <div class="seg" role="group" aria-label="Filter">
              <button type="button" :aria-pressed="filter === 'ALL'" @click="filter = 'ALL'">All <em>{{ totals.count }}</em></button>
              <button type="button" :aria-pressed="filter === 'SHORT'" @click="filter = 'SHORT'">Not covered <em>{{ totals.short }}</em></button>
              <button type="button" :aria-pressed="filter === 'COVERED'" @click="filter = 'COVERED'">Covered <em>{{ totals.covered }}</em></button>
            </div>
          </div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Investor</th><th class="cp-num">Contracts</th><th class="cp-num">Outstanding</th><th class="cp-num">Cheques</th><th>Coverage</th></tr></thead>
              <tbody>
                <tr
                  v-for="i in investors"
                  :key="i.investorId"
                  class="pick"
                  :class="{ picked: i.investorId === selectedId }"
                  tabindex="0"
                  @click="selectedId = i.investorId"
                  @keydown.enter="selectedId = i.investorId"
                >
                  <td><strong>{{ i.investorName }}</strong><br /><small class="cp-muted">{{ i.investorCode }}</small></td>
                  <td class="cp-num">{{ i.contractCount }}</td>
                  <td class="cp-num">{{ formatAmount(i.outstandingAmount) }}</td>
                  <td class="cp-num">{{ formatAmount(i.chequeAmount) }}<br /><small class="cp-muted">{{ i.chequeCount }} cheques</small></td>
                  <td>
                    <div class="cov">
                      <div class="bar" :class="{ full: i.isCovered }"><i :style="{ width: pct(i.chequeAmount, i.outstandingAmount) + '%' }"></i></div>
                      <span class="cp-pill" :class="i.isCovered ? 'cp-pill--ok' : 'cp-pill--warn'">{{ i.isCovered ? 'Covered' : 'Not covered' }}</span>
                    </div>
                  </td>
                </tr>
                <tr v-if="investors.length === 0"><td colspan="5" class="cp-empty">{{ data.investors.length ? 'No investors match this filter.' : 'No investors are included in this deal.' }}</td></tr>
              </tbody>
            </table>
          </div>
        </section>

        <section class="cp-card">
          <div class="cp-card__head">
            <i class="pi pi-wallet"></i>
            <h3>{{ selected ? `Cheques of ${selected.investorName}` : 'Cheques' }}</h3>
            <span class="cp-spacer"></span>
            <span v-if="selected" class="cp-muted">Remaining {{ formatAmount(Math.max(0, selected.outstandingAmount - selected.chequeAmount)) }}</span>
            <button v-if="editable && selected" type="button" class="cp-btn cp-btn--small cp-btn--primary" @click="openForm(null)"><i class="pi pi-plus"></i><span>Add cheque</span></button>
          </div>
          <div v-if="!selected" class="cp-card__body cp-muted">Select an investor above to see their outstanding balance and enter their cheques.</div>
          <div v-else class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Cheque no.</th><th>Contract</th><th>Bank</th><th>Due date</th><th class="cp-num">Amount</th><th>Endorsed</th><th>Status</th><th></th></tr></thead>
              <tbody>
                <tr v-for="c in selectedCheques" :key="c.chequeId" :title="c.returnReason ?? ''">
                  <td><strong>{{ c.chequeNo }}</strong><br /><small v-if="c.chequeDate" class="cp-muted">Dated {{ date(c.chequeDate) }}</small></td>
                  <td><span class="code">{{ c.sellerContractNo ?? '-' }}</span><br /><small class="cp-muted">{{ c.unitCode ?? '' }}</small></td>
                  <td>{{ c.bankName ?? '-' }}</td>
                  <td>{{ date(c.dueDate) }}</td>
                  <td class="cp-num">{{ formatAmount(c.chequeAmount) }}</td>
                  <td>{{ c.isEndorsedToCompany ? 'Yes' : 'No' }}</td>
                  <td><span class="cp-pill" :class="{ 'cp-pill--ok': c.chequeStatusCode === 'CLEARED', 'cp-pill--bad': c.chequeStatusCode === 'RETURNED' }">{{ c.chequeStatusName }}</span></td>
                  <td><button v-if="editable" type="button" class="cp-btn cp-btn--small" :aria-label="`Edit cheque ${c.chequeNo}`" @click="openForm(c)"><i class="pi pi-pencil"></i></button></td>
                </tr>
                <tr v-if="selectedCheques.length === 0"><td colspan="8" class="cp-empty">No cheques yet for this investor. Use Add cheque.</td></tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>

      <aside class="stack">
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-list-check"></i><h3>Coverage rules</h3></div>
          <ul class="rules cp-card__body">
            <li v-for="r in rules" :key="r.label" :class="{ ok: r.ok }">
              <i :class="r.ok ? 'pi pi-check-circle' : 'pi pi-circle'"></i>
              <div><strong>{{ r.label }}</strong><small>{{ r.hint }}</small><em>{{ r.detail }}</em></div>
            </li>
          </ul>
        </section>

        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-chart-bar"></i><h3>Totals</h3></div>
          <dl class="cp-kv cp-card__body">
            <dt>Outstanding to cover</dt><dd :title="formatAmount(totals.outstanding)">{{ formatShort(totals.outstanding) }} <small class="cp-muted">{{ data.currencyCode }}</small></dd>
            <dt>Cheques entered</dt><dd :title="formatAmount(totals.cheques)">{{ formatShort(totals.cheques) }}</dd>
            <dt>Remaining</dt><dd :title="formatAmount(totals.remaining)">{{ formatShort(totals.remaining) }}</dd>
            <dt>Investors covered</dt><dd>{{ totals.covered }} of {{ totals.count }}</dd>
          </dl>
        </section>

        <section v-if="editable" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-check-circle"></i><h3>Decide</h3></div>
          <div class="cp-card__body">
            <div class="cp-field">
              <label for="chq-comment">Comment <span class="req">*</span></label>
              <textarea id="chq-comment" v-model="comment" rows="4" maxlength="4000" placeholder="Required for Accept and Reject"></textarea>
            </div>
            <div class="decide">
              <button type="button" class="cp-btn cp-btn--danger" :disabled="busy" @click="finish('REJECT')"><i class="pi pi-times"></i><span>Reject</span></button>
              <button type="button" class="cp-btn cp-btn--primary" :disabled="busy" @click="finish('ACCEPT')"><i class="pi pi-check"></i><span>Accept & continue</span></button>
            </div>
          </div>
        </section>
      </aside>
    </div>

    <div v-if="formOpen" class="cp-modal-overlay" @click.self="formOpen = false">
      <section class="cp-modal" role="dialog" aria-modal="true" aria-labelledby="chq-title">
        <header class="cp-modal__header">
          <div><h3 id="chq-title">{{ editing ? 'Edit cheque' : 'Add cheque' }}</h3><p>{{ selected?.investorName }}</p></div>
          <button type="button" class="cp-modal__close" aria-label="Close" @click="formOpen = false"><i class="pi pi-times"></i></button>
        </header>
        <div class="cp-modal__body">
          <div class="cp-form">
            <div class="cp-field cp-field--wide">
              <label for="chq-account">Contract <span class="req">*</span></label>
              <select id="chq-account" v-model="form.portfolioAccountId">
                <option value="">- Select -</option>
                <option v-for="a in selectedAccounts" :key="a.portfolioAccountId" :value="a.portfolioAccountId">
                  {{ a.sellerContractNo ?? `Contract ${a.portfolioAccountId}` }}{{ a.unitCode ? ` · ${a.unitCode}` : '' }} · outstanding {{ formatAmount(a.outstandingAmount) }}
                </option>
              </select>
            </div>
            <div class="cp-field"><label for="chq-no">Cheque no. <span class="req">*</span></label><input id="chq-no" v-model="form.chequeNo" maxlength="100" /></div>
            <div class="cp-field"><label for="chq-bank">Bank name</label><input id="chq-bank" v-model="form.bankName" maxlength="300" /></div>
            <div class="cp-field"><label for="chq-amount">Amount <span class="req">*</span></label><input id="chq-amount" v-model="form.chequeAmount" type="number" min="0" step="0.01" /></div>
            <div class="cp-field"><label for="chq-due">Due date <span class="req">*</span></label><input id="chq-due" v-model="form.dueDate" type="date" /></div>
            <div class="cp-field"><label for="chq-date">Cheque date</label><input id="chq-date" v-model="form.chequeDate" type="date" /></div>
            <div class="cp-field">
              <label for="chq-status">Cheque status</label>
              <select id="chq-status" v-model="form.chequeStatusCode">
                <option v-for="s in data.chequeStatuses" :key="s.code" :value="s.code">{{ s.name }}</option>
              </select>
            </div>
            <div class="cp-field">
              <label>Endorsed to company</label>
              <div class="yn" role="group" aria-label="Endorsed to company">
                <button type="button" :aria-pressed="form.isEndorsedToCompany" @click="form.isEndorsedToCompany = true">Yes</button>
                <button type="button" :aria-pressed="!form.isEndorsedToCompany" @click="form.isEndorsedToCompany = false">No</button>
              </div>
            </div>
            <div class="cp-field"><label for="chq-clearing">Clearing date</label><input id="chq-clearing" v-model="form.clearingDate" type="date" /></div>
            <div class="cp-field cp-field--wide"><label for="chq-return">Return reason</label><input id="chq-return" v-model="form.returnReason" maxlength="1000" /></div>
          </div>
        </div>
        <footer class="cp-modal__footer">
          <button v-if="editing" type="button" class="cp-btn cp-btn--danger" :disabled="busy" @click="removeCheque">
            <i class="pi pi-trash"></i><span>{{ confirmDelete ? 'Click again to delete' : 'Delete' }}</span>
          </button>
          <span class="cp-spacer"></span>
          <button type="button" class="cp-btn" :disabled="busy" @click="formOpen = false">Cancel</button>
          <button type="button" class="cp-btn cp-btn--primary" :disabled="busy" @click="saveForm"><i class="pi pi-check"></i><span>Save cheque</span></button>
        </footer>
      </section>
    </div>
  </StepLayout>
</template>

<style scoped>
.notice { padding: 10px 14px; border: 1px solid var(--cap-success-border); border-radius: 8px; background: var(--cap-success-bg); color: var(--cap-success); font-size: 13px; }
.readonly { padding: 10px 14px; border-radius: 8px; background: var(--cap-bg); color: var(--cap-ink-soft); font-size: 13px; }
.rejected { padding: 10px 14px; border: 1px solid var(--cap-danger-border); border-radius: 8px; background: var(--cap-danger-bg); color: var(--cap-danger-dark); font-size: 13px; }
.grid { display: grid; grid-template-columns: minmax(0, 1fr) 320px; gap: 16px; align-items: start; }
.stack { display: grid; grid-template-columns: minmax(0, 1fr); gap: 16px; }
.tools { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; padding: 12px 20px; }
.search { flex: 1; max-width: 360px; height: 36px; display: flex; align-items: center; gap: 9px; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 8px; }
.search i { color: var(--cap-muted); font-size: 12px; }
.search input { width: 100%; border: 0; outline: 0; background: transparent; font: inherit; font-size: 12px; }
.seg { display: inline-flex; flex-wrap: wrap; border: 1px solid var(--cap-border); border-radius: 8px; overflow: hidden; }
.seg button { height: 34px; padding: 0 12px; border: 0; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.seg button + button { border-inline-start: 1px solid var(--cap-border); }
.seg button em { margin-inline-start: 4px; color: var(--cap-muted); font-style: normal; }
.seg button[aria-pressed="true"] { background: var(--cap-primary-dark); color: #fff; }
.seg button[aria-pressed="true"] em { color: var(--cap-primary-tint-2); }
tr.pick { cursor: pointer; }
tr.pick:hover td { background: var(--cap-bg); }
tr.picked td { background: var(--cap-primary-tint); }
tr.picked td:first-child { box-shadow: inset 3px 0 0 var(--cap-primary-dark); }
.cov { display: flex; align-items: center; gap: 8px; min-width: 170px; }
.bar { width: 80px; height: 8px; border-radius: 999px; background: var(--cap-border-soft); overflow: hidden; flex: none; }
.bar i { display: block; height: 100%; border-radius: inherit; background: var(--cap-warning); }
.bar.full i { background: var(--cap-success); }
.code { color: var(--cap-primary-dark); font-weight: 600; font-size: 11px; }
.rules { list-style: none; margin: 0; display: grid; gap: 14px; }
.rules li { display: flex; gap: 10px; color: var(--cap-muted); }
.rules li > i { margin-top: 2px; }
.rules li.ok > i { color: var(--cap-success); }
.rules div { display: grid; gap: 2px; }
.rules strong { font-size: 13px; color: var(--cap-ink); }
.rules small { font-size: 11px; }
.rules em { font-style: normal; font-size: 12px; color: var(--cap-warning); }
.rules li.ok em { color: var(--cap-success); }
.yn { display: inline-flex; width: fit-content; border: 1px solid var(--cap-border); border-radius: 8px; overflow: hidden; }
.yn button { height: 34px; padding: 0 14px; border: 0; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.yn button + button { border-inline-start: 1px solid var(--cap-border); }
.yn button[aria-pressed="true"] { background: var(--cap-primary-dark); color: #fff; }
.decide { display: flex; justify-content: flex-end; gap: 8px; margin-top: 12px; }
.req { color: var(--cap-danger); }
@media (max-width: 1100px) { .grid { grid-template-columns: minmax(0, 1fr); } }
</style>
