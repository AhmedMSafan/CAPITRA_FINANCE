<script setup lang="ts">
// Workflow step "Portfolio Inquiries" (APEX App 105 page 210): record every inquiry and its result, review which
// investors stay in the deal, then complete the step.
import { computed, reactive, ref, shallowRef, watch } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { formatDate, formatDateTime, todayInCairo } from '@/shared/format/datetime'
import { formatAmount, formatShort } from '@/shared/format/money'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { buildStages, stepPhase } from '../stepPhase'
import StepLayout from '../StepLayout.vue'
import { messageOf, niceCode } from '../stepUtils'
import { SUBJECT_TYPES, inquiriesApi, type Inquiries, type Inquiry, type SubjectType } from './inquiries.api'

type DealFilter = 'ALL' | 'IN' | 'OUT'

const props = defineProps<{ portfolioId: string }>()
const canAct = hasPermission('MTG.STEP.INQUIRIES')

const data = shallowRef<Inquiries | null>(null)
const error = ref('')
const notice = ref('')
const busy = ref(false)
const dealFilter = ref<DealFilter>('ALL')
const search = ref('')

const id = computed(() => Number(props.portfolioId))
const editable = computed(() => canAct && !!data.value?.isOpen)

async function load() {
  try {
    data.value = await inquiriesApi.get(id.value)
  } catch (e) {
    error.value = messageOf(e, 'Unable to load the inquiries.')
  }
}

watch(id, () => {
  data.value = null
  error.value = ''
  load()
}, { immediate: true })

// ----- investors -----
const accounts = computed(() => {
  const q = search.value.trim().toLowerCase()
  return (data.value?.accounts ?? []).filter(a => {
    const out = a.dealDecisionCode === 'EXCLUDED'
    if (dealFilter.value === 'IN' && out) return false
    if (dealFilter.value === 'OUT' && !out) return false
    return !q || [a.investorName, a.investorCode, a.sellerContractNo ?? '', a.unitCode ?? ''].some(v => v.toLowerCase().includes(q))
  })
})

const counts = computed(() => {
  const all = data.value?.accounts ?? []
  const kept = all.filter(a => a.dealDecisionCode !== 'EXCLUDED')
  return {
    all: all.length,
    kept: kept.length,
    out: all.length - kept.length,
    face: all.reduce((t, a) => t + a.contractAmount, 0),
    keptFace: kept.reduce((t, a) => t + a.contractAmount, 0),
    inquiries: data.value?.inquiries.length ?? 0,
    awaiting: (data.value?.inquiries ?? []).filter(q => !q.resultText).length
  }
})

/** Include and exclude can be switched freely until the step is completed. */
async function decide(portfolioAccountId: number, decision: 'SELECTED' | 'EXCLUDED') {
  busy.value = true
  error.value = ''
  try {
    await inquiriesApi.saveDecisions(id.value, [portfolioAccountId], decision)
    await load()
  } catch (e) {
    error.value = messageOf(e, 'Unable to save the deal decision.')
  } finally {
    busy.value = false
  }
}

// ----- inquiry form -----
const editing = shallowRef<Inquiry | null>(null)
const formOpen = ref(false)
const formError = ref('')
const form = reactive({
  inquiryTypeCode: '',
  subjectTypeCode: 'PORTFOLIO' as SubjectType,
  subjectId: '' as number | '',
  inquiryDate: '',
  referenceNo: '',
  resultText: '',
  riskLevelCode: '',
  resultDetails: ''
})

const subjectChoices = computed(() => (data.value?.subjects ?? []).filter(s => s.subjectTypeCode === form.subjectTypeCode))

function pickSubjectType(code: SubjectType) {
  form.subjectTypeCode = code
  // Portfolio, developer and project have exactly one choice; pick it.
  const choices = subjectChoices.value
  form.subjectId = choices.length === 1 ? choices[0]!.subjectId : ''
}

function openForm(row: Inquiry | null) {
  editing.value = row
  formError.value = ''
  Object.assign(form, row
    ? {
        inquiryTypeCode: row.inquiryTypeCode, subjectTypeCode: row.subjectTypeCode, subjectId: row.subjectId,
        inquiryDate: row.inquiryDate ?? '', referenceNo: row.referenceNo ?? '', resultText: row.resultText ?? '',
        riskLevelCode: row.riskLevelCode ?? '', resultDetails: row.resultDetails ?? ''
      }
    : {
        inquiryTypeCode: data.value?.inquiryTypes[0]?.code ?? '', subjectTypeCode: 'PORTFOLIO', subjectId: data.value?.portfolioId ?? '',
        inquiryDate: todayInCairo(), referenceNo: '', resultText: '', riskLevelCode: '', resultDetails: ''
      })
  formOpen.value = true
}

async function saveForm() {
  busy.value = true
  formError.value = ''
  const body = {
    subjectTypeCode: form.subjectTypeCode,
    subjectId: form.subjectId === '' ? null : Number(form.subjectId),
    inquiryTypeCode: form.inquiryTypeCode,
    inquiryDate: form.inquiryDate || null,
    referenceNo: form.referenceNo || null,
    resultText: form.resultText || null,
    riskLevelCode: form.riskLevelCode || null,
    resultDetails: form.resultDetails || null
  }
  try {
    if (editing.value) await inquiriesApi.update(id.value, editing.value.inquiryResultId, { ...body, rowVersion: editing.value.rowVersion })
    else await inquiriesApi.create(id.value, body)
    formOpen.value = false
    await load()
  } catch (e) {
    formError.value = messageOf(e, 'Unable to save the inquiry.')
  } finally {
    busy.value = false
  }
}

const confirmDelete = ref(false)

async function removeInquiry() {
  if (!editing.value) return
  if (!confirmDelete.value) {
    confirmDelete.value = true
    return
  }
  confirmDelete.value = false
  busy.value = true
  try {
    await inquiriesApi.remove(id.value, editing.value)
    formOpen.value = false
    await load()
  } catch (e) {
    formError.value = messageOf(e, 'Unable to delete the inquiry.')
  } finally {
    busy.value = false
  }
}

// ----- complete / reject -----
const comment = ref('')

async function finish(action: 'APPROVE' | 'REJECT') {
  busy.value = true
  error.value = ''
  notice.value = ''
  try {
    data.value = await inquiriesApi.decide(id.value, action, comment.value)
    notice.value = action === 'APPROVE' ? 'Inquiry step completed.' : 'Inquiry step rejected.'
    comment.value = ''
  } catch (e) {
    error.value = messageOf(e, 'Unable to record the decision.')
  } finally {
    busy.value = false
  }
}

function riskTone(code: string | null): string {
  if (code === 'LOW') return 'cp-pill--ok'
  if (code === 'MEDIUM') return 'cp-pill--warn'
  if (code === 'HIGH' || code === 'CRITICAL') return 'cp-pill--bad'
  return ''
}


// Progress inside the step. Nothing is ticked before the portfolio reaches this step, everything once it has passed it.
const stages = computed(() => {
  const d = data.value
  if (!d) return []
  return buildStages(stepPhase('INQUIRY_ACCEPTED', d.portfolioStatusCode), [
    { label: 'Record inquiries', done: counts.value.inquiries > 0 },
    { label: 'Capture results', done: counts.value.inquiries > 0 && counts.value.awaiting === 0 },
    { label: 'Review investors', done: false },
    { label: 'Complete step', done: false }
  ])
})

// Messages open the app-wide pop-up; saving shows the progress screen.
useFeedback({
  errors: [error, formError],
  notices: [notice],
  busy,
  busyAs: () => ({ title: 'Saving the inquiries', kind: 'save', steps: ['Checking the inquiry results', 'Saving', 'Updating the portfolio'] })
})
</script>

<template>
  <div v-if="error && !data" class="cp-page"><div class="cp-error">{{ error }}</div></div>
  <div v-else-if="!data" class="cp-page"><div class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div></div>

  <StepLayout
    v-else
    :portfolio-id="data.portfolioId"
    title="Portfolio inquiries"
    lead="Record every inquiry and its result, review which investors stay in the deal, then complete this workflow step."
    :portfolio-no="data.portfolioNo"
    :project-name="data.projectName"
    :project-code="data.projectCode"
    :status-name="data.portfolioStatusName"
    :stages="stages"
    apex-page="210"
  >
    <div v-if="!data.isOpen" class="readonly"><i class="pi pi-eye"></i> Read-only view. Inquiries are not open for a portfolio in status {{ data.portfolioStatusName }}.</div>
    <div v-else-if="!canAct" class="readonly"><i class="pi pi-eye"></i> Read-only view. Recording inquiries needs the inquiries permission.</div>

    <div class="grid">
      <div class="stack">
        <section class="cp-card">
          <div class="cp-card__head">
            <i class="pi pi-comments"></i><h3>Inquiries</h3><span class="cp-spacer"></span>
            <span class="cp-muted">{{ counts.inquiries }} inquiries<template v-if="counts.awaiting"> · {{ counts.awaiting }} awaiting result</template></span>
            <button v-if="editable" type="button" class="cp-btn cp-btn--small cp-btn--create" @click="openForm(null)"><i class="pi pi-plus"></i><span>Add inquiry</span></button>
          </div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Inquiry</th><th>Subject</th><th>Date</th><th>Reference</th><th>Result</th><th>Risk</th><th>By</th><th></th></tr></thead>
              <tbody>
                <tr v-for="q in data.inquiries" :key="q.inquiryResultId" :title="q.resultDetails ?? ''">
                  <td><strong>{{ q.inquiryTypeName }}</strong></td>
                  <td>{{ q.subjectName }}<br /><small class="cp-muted">{{ niceCode(q.subjectTypeCode) }}</small></td>
                  <td>{{ q.inquiryDate ? formatDate(q.inquiryDate + 'T00:00:00Z') : '-' }}</td>
                  <td>{{ q.referenceNo ?? '-' }}</td>
                  <td><span v-if="q.resultText">{{ q.resultText }}</span><span v-else class="cp-pill cp-pill--warn">Awaiting result</span></td>
                  <td><span v-if="q.riskLevelCode" class="cp-pill" :class="riskTone(q.riskLevelCode)">{{ niceCode(q.riskLevelCode) }}</span><span v-else class="cp-muted">-</span></td>
                  <td><small>{{ q.createdBy }}<br /><span class="cp-muted">{{ formatDateTime(q.createdAt) }}</span></small></td>
                  <td><button v-if="editable" type="button" class="cp-btn cp-btn--small" :aria-label="`Edit ${q.inquiryTypeName}`" @click="openForm(q)"><i class="pi pi-pencil"></i></button></td>
                </tr>
                <tr v-if="data.inquiries.length === 0"><td colspan="8" class="cp-empty">No inquiries recorded yet. Start with Add inquiry.</td></tr>
              </tbody>
            </table>
          </div>
        </section>

        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-users"></i><h3>Portfolio investors</h3><span class="cp-spacer"></span><span class="cp-muted">{{ accounts.length }} of {{ counts.all }}</span></div>
          <div class="tools">
            <label class="search"><i class="pi pi-search"></i><input v-model="search" type="search" placeholder="Search investor, code, contract or unit" aria-label="Search investors" /></label>
            <div class="seg" role="group" aria-label="Filter">
              <button type="button" :aria-pressed="dealFilter === 'ALL'" @click="dealFilter = 'ALL'">All <em>{{ counts.all }}</em></button>
              <button type="button" :aria-pressed="dealFilter === 'IN'" @click="dealFilter = 'IN'">Included <em>{{ counts.kept }}</em></button>
              <button type="button" :aria-pressed="dealFilter === 'OUT'" @click="dealFilter = 'OUT'">Excluded <em>{{ counts.out }}</em></button>
            </div>
          </div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Investor</th><th>Unit</th><th>Contract</th><th class="cp-num">Face value</th><th class="cp-num">Outstanding</th><th>Inquiries</th><th>Deal</th></tr></thead>
              <tbody>
                <tr v-for="a in accounts" :key="a.portfolioAccountId" :class="{ out: a.dealDecisionCode === 'EXCLUDED' }">
                  <td><strong>{{ a.investorName }}</strong><br /><small class="cp-muted">{{ a.investorCode }}</small></td>
                  <td>{{ a.unitCode ?? '-' }}</td>
                  <td><span class="code">{{ a.sellerContractNo ?? '-' }}</span></td>
                  <td class="cp-num">{{ formatAmount(a.contractAmount) }}</td>
                  <td class="cp-num">{{ formatAmount(a.outstandingAmount) }}</td>
                  <td>
                    {{ a.inquiryCount }}
                    <span v-if="a.latestRiskLevelCode" class="cp-pill" :class="riskTone(a.latestRiskLevelCode)">{{ niceCode(a.latestRiskLevelCode) }}</span>
                  </td>
                  <td>
                    <div class="toggle" role="group" :aria-label="`Deal decision for ${a.investorName}`">
                      <button type="button" class="in" :aria-pressed="a.dealDecisionCode !== 'EXCLUDED'" :disabled="!editable || busy" @click="decide(a.portfolioAccountId, 'SELECTED')">Included</button>
                      <button type="button" class="ex" :aria-pressed="a.dealDecisionCode === 'EXCLUDED'" :disabled="!editable || busy" @click="decide(a.portfolioAccountId, 'EXCLUDED')">Excluded</button>
                    </div>
                  </td>
                </tr>
                <tr v-if="accounts.length === 0"><td colspan="7" class="cp-empty">{{ data.accounts.length ? 'No contracts match this filter.' : 'This portfolio has no investor contracts yet.' }}</td></tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>

      <aside class="stack">
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-chart-bar"></i><h3>Deal impact</h3></div>
          <dl class="cp-kv cp-card__body">
            <dt>Contracts kept</dt><dd>{{ counts.kept }} of {{ counts.all }}</dd>
            <dt>Face value</dt><dd :title="formatAmount(counts.keptFace)">{{ formatShort(counts.keptFace) }} <small class="cp-muted">of {{ formatShort(counts.face) }}</small></dd>
          </dl>
        </section>

        <section v-if="editable" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-check-circle"></i><h3>Complete step</h3></div>
          <div class="cp-card__body">
            <div class="cp-field">
              <label for="inq-comment">Comment</label>
              <textarea id="inq-comment" v-model="comment" rows="4" maxlength="4000"></textarea>
            </div>
            <div class="decide">
              <button type="button" class="cp-btn cp-btn--danger" :disabled="busy" @click="finish('REJECT')"><i class="pi pi-times"></i><span>Reject</span></button>
              <button type="button" class="cp-btn cp-btn--primary" :disabled="busy" @click="finish('APPROVE')"><i class="pi pi-check"></i><span>Approve inquiry step</span></button>
            </div>
          </div>
        </section>
      </aside>
    </div>

    <div v-if="formOpen" class="cp-modal-overlay" @click.self="formOpen = false">
      <section class="cp-modal" role="dialog" aria-modal="true" aria-labelledby="inq-title">
        <header class="cp-modal__header">
          <div><h3 id="inq-title">{{ editing ? 'Edit inquiry' : 'Add inquiry' }}</h3><p>What was asked, about whom, and what came back.</p></div>
          <button type="button" class="cp-modal__close" aria-label="Close" @click="formOpen = false"><i class="pi pi-times"></i></button>
        </header>
        <div class="cp-modal__body">
          <div class="cp-form">
            <div class="cp-field cp-field--wide">
              <label>Inquiry type <span class="req">*</span></label>
              <div class="chips">
                <button v-for="t in data.inquiryTypes" :key="t.code" type="button" :aria-pressed="form.inquiryTypeCode === t.code" @click="form.inquiryTypeCode = t.code">{{ t.name }}</button>
                <span v-if="data.inquiryTypes.length === 0" class="cp-muted">No active inquiry types. Add them in Reference Data.</span>
              </div>
            </div>
            <div class="cp-field cp-field--wide">
              <label>Subject <span class="req">*</span></label>
              <div class="chips">
                <button v-for="s in SUBJECT_TYPES" :key="s.code" type="button" :aria-pressed="form.subjectTypeCode === s.code" @click="pickSubjectType(s.code)">{{ s.label }}</button>
              </div>
            </div>
            <div v-if="form.subjectTypeCode === 'GUARANTEE'" class="cp-field">
              <label for="inq-subject-id">Guarantee ID <span class="req">*</span></label>
              <input id="inq-subject-id" v-model="form.subjectId" type="number" min="1" />
            </div>
            <div v-else class="cp-field">
              <label for="inq-subject">Choose the subject <span class="req">*</span></label>
              <select id="inq-subject" v-model="form.subjectId">
                <option value="">- Select -</option>
                <option v-for="s in subjectChoices" :key="s.subjectId" :value="s.subjectId">{{ s.name }}</option>
              </select>
            </div>
            <div class="cp-field"><label for="inq-date">Inquiry date</label><input id="inq-date" v-model="form.inquiryDate" type="date" /></div>
            <div class="cp-field"><label for="inq-ref">Reference no.</label><input id="inq-ref" v-model="form.referenceNo" maxlength="100" /></div>
            <div class="cp-field"><label for="inq-result">Result</label><input id="inq-result" v-model="form.resultText" maxlength="100" placeholder="e.g. No adverse record" /></div>
            <div class="cp-field cp-field--wide">
              <label>Risk level</label>
              <div class="chips">
                <button type="button" :aria-pressed="form.riskLevelCode === ''" @click="form.riskLevelCode = ''">Not set</button>
                <button v-for="r in data.riskLevels" :key="r" type="button" :aria-pressed="form.riskLevelCode === r" @click="form.riskLevelCode = r">{{ niceCode(r) }}</button>
              </div>
            </div>
            <div class="cp-field cp-field--wide"><label for="inq-details">Result details</label><textarea id="inq-details" v-model="form.resultDetails" rows="4" maxlength="4000"></textarea></div>
          </div>
        </div>
        <footer class="cp-modal__footer">
          <button v-if="editing" type="button" class="cp-btn cp-btn--danger" :disabled="busy" @click="removeInquiry">
            <i class="pi pi-trash"></i><span>{{ confirmDelete ? 'Click again to delete' : 'Delete' }}</span>
          </button>
          <span class="cp-spacer"></span>
          <button type="button" class="cp-btn" :disabled="busy" @click="formOpen = false">Cancel</button>
          <button type="button" class="cp-btn cp-btn--primary" :disabled="busy" @click="saveForm"><i class="pi pi-check"></i><span>Save inquiry</span></button>
        </footer>
      </section>
    </div>
  </StepLayout>
</template>

<style scoped>
.notice { padding: 10px 14px; border: 1px solid var(--cap-success-border); border-radius: 8px; background: var(--cap-success-bg); color: var(--cap-success); font-size: 13px; }
.readonly { padding: 10px 14px; border-radius: 8px; background: var(--cap-bg); color: var(--cap-ink-soft); font-size: 13px; }
.grid { display: grid; grid-template-columns: minmax(0, 1fr) 320px; gap: 16px; align-items: start; }
.stack { display: grid; grid-template-columns: minmax(0, 1fr); gap: 16px; }
.tools { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; padding: 12px 20px; }
.search { flex: 1; max-width: 420px; height: 36px; display: flex; align-items: center; gap: 9px; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 8px; }
.search i { color: var(--cap-muted); font-size: 12px; }
.search input { width: 100%; border: 0; outline: 0; background: transparent; font: inherit; font-size: 12px; }
.seg { display: inline-flex; border: 1px solid var(--cap-border); border-radius: 8px; overflow: hidden; }
.seg button { height: 34px; padding: 0 12px; border: 0; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.seg button + button { border-inline-start: 1px solid var(--cap-border); }
.seg button em { margin-inline-start: 4px; color: var(--cap-muted); font-style: normal; }
.seg button[aria-pressed="true"] { background: var(--cap-selected); color: #fff; }
.seg button[aria-pressed="true"] em { color: var(--cap-primary-tint-2); }
tr.out td { opacity: .6; }
.toggle { display: inline-flex; border: 1px solid var(--cap-border); border-radius: 8px; overflow: hidden; }
.toggle button { height: 30px; padding: 0 12px; border: 0; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.toggle button + button { border-inline-start: 1px solid var(--cap-border); }
.toggle button:disabled { cursor: default; }
.toggle .in[aria-pressed="true"] { background: var(--cap-success); color: #fff; }
.toggle .ex[aria-pressed="true"] { background: var(--cap-danger); color: #fff; }
.code { color: var(--cap-heading); font-weight: 600; font-size: 11px; }
.decide { display: flex; justify-content: flex-end; gap: 8px; margin-top: 12px; }
.req { color: var(--cap-danger); }
.chips { display: flex; flex-wrap: wrap; gap: 6px; }
.chips button { height: 32px; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 999px; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.chips button[aria-pressed="true"] { border-color: var(--cap-selected); background: var(--cap-selected); color: #fff; }
@media (max-width: 1100px) { .grid { grid-template-columns: minmax(0, 1fr); } }
</style>
