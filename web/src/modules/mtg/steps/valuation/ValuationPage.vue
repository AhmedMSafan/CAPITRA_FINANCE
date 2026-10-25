<script setup lang="ts">
// Workflow step "Portfolio Valuation" (APEX App 105 page 401): record the valuations of the project and its units,
// compare the recommended values with the outstanding balance, then complete the step.
import { computed, reactive, ref, shallowRef, watch } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { formatDate, formatDateTime, todayInCairo } from '@/shared/format/datetime'
import { formatAmount, formatPct, formatShort } from '@/shared/format/money'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { buildStages, stepPhase } from '../stepPhase'
import StepLayout from '../StepLayout.vue'
import { messageOf } from '../stepUtils'
import { valuationApi, type Valuation, type Valuations } from './valuation.api'

const props = defineProps<{ portfolioId: string }>()
const canAct = hasPermission('MTG.STEP.VALUATION')

const data = shallowRef<Valuations | null>(null)
const error = ref('')
const notice = ref('')
const busy = ref(false)

const id = computed(() => Number(props.portfolioId))
const editable = computed(() => canAct && !!data.value?.isOpen)

async function load() {
  try {
    data.value = await valuationApi.get(id.value)
  } catch (e) {
    error.value = messageOf(e, 'Unable to load the valuations.')
  }
}

watch(id, () => {
  data.value = null
  error.value = ''
  load()
}, { immediate: true })

// ----- totals (as APEX: every recorded valuation counts) -----
const totals = computed(() => {
  const vs = data.value?.valuations ?? []
  const withRecommended = vs.filter(v => v.recommendedValueAmount !== null)
  const market = vs.reduce((t, v) => t + v.marketValueAmount, 0)
  const recommended = withRecommended.reduce((t, v) => t + (v.recommendedValueAmount ?? 0), 0)
  const outstanding = data.value?.outstandingAmount ?? 0
  const coverage = outstanding > 0 && withRecommended.length ? Math.round((recommended / outstanding) * 100) : 0
  return { count: vs.length, withRecommended: withRecommended.length, market, recommended, outstanding, coverage }
})

const checks = computed(() => [
  { label: 'At least one valuation recorded', ok: totals.value.count > 0 },
  { label: `Every valuation has a recommended value (${totals.value.withRecommended} of ${totals.value.count})`, ok: totals.value.count > 0 && totals.value.withRecommended === totals.value.count }
])

// ----- valuation form -----
const editing = shallowRef<Valuation | null>(null)
const formOpen = ref(false)
const formError = ref('')
const form = reactive({
  valuationTypeCode: '',
  projectUnitId: '' as number | '',
  valuerName: '',
  valuerRegistrationNo: '',
  valuationDate: '',
  marketValueAmount: '' as number | '',
  recommendedValueAmount: '' as number | '',
  projectCompletionPct: '' as number | '',
  facilitiesStatus: '',
  approvedBy: '',
  approvedDate: ''
})

function openForm(row: Valuation | null) {
  editing.value = row
  formError.value = ''
  confirmDelete.value = false
  Object.assign(form, row
    ? {
        valuationTypeCode: row.valuationTypeCode, projectUnitId: row.projectUnitId ?? '', valuerName: row.valuerName,
        valuerRegistrationNo: row.valuerRegistrationNo ?? '', valuationDate: row.valuationDate,
        marketValueAmount: row.marketValueAmount, recommendedValueAmount: row.recommendedValueAmount ?? '',
        projectCompletionPct: row.projectCompletionPct ?? '', facilitiesStatus: row.facilitiesStatus ?? '',
        approvedBy: row.approvedBy ?? '', approvedDate: row.approvedDate ?? ''
      }
    : {
        valuationTypeCode: data.value?.valuationTypes[0]?.code ?? '', projectUnitId: '', valuerName: '', valuerRegistrationNo: '',
        valuationDate: todayInCairo(), marketValueAmount: '', recommendedValueAmount: '', projectCompletionPct: '',
        facilitiesStatus: '', approvedBy: '', approvedDate: ''
      })
  formOpen.value = true
}

function num(v: number | ''): number | null {
  return v === '' ? null : Number(v)
}

async function saveForm() {
  busy.value = true
  formError.value = ''
  const body = {
    valuationTypeCode: form.valuationTypeCode,
    projectUnitId: num(form.projectUnitId),
    valuerName: form.valuerName,
    valuerRegistrationNo: form.valuerRegistrationNo || null,
    valuationDate: form.valuationDate || null,
    marketValueAmount: num(form.marketValueAmount),
    recommendedValueAmount: num(form.recommendedValueAmount),
    projectCompletionPct: num(form.projectCompletionPct),
    facilitiesStatus: form.facilitiesStatus || null,
    approvedBy: form.approvedBy || null,
    approvedDate: form.approvedDate || null
  }
  try {
    if (editing.value) await valuationApi.update(id.value, editing.value.valuationId, { ...body, rowVersion: editing.value.rowVersion })
    else await valuationApi.create(id.value, body)
    formOpen.value = false
    await load()
  } catch (e) {
    formError.value = messageOf(e, 'Unable to save the valuation.')
  } finally {
    busy.value = false
  }
}

const confirmDelete = ref(false)

async function removeValuation() {
  if (!editing.value) return
  if (!confirmDelete.value) {
    confirmDelete.value = true
    return
  }
  confirmDelete.value = false
  busy.value = true
  try {
    await valuationApi.remove(id.value, editing.value)
    formOpen.value = false
    await load()
  } catch (e) {
    formError.value = messageOf(e, 'Unable to delete the valuation.')
  } finally {
    busy.value = false
  }
}

// ----- complete -----
const comment = ref('')

async function complete() {
  busy.value = true
  error.value = ''
  notice.value = ''
  try {
    data.value = await valuationApi.complete(id.value, comment.value)
    notice.value = 'Valuation step completed.'
    comment.value = ''
  } catch (e) {
    error.value = messageOf(e, 'Unable to complete the step.')
  } finally {
    busy.value = false
  }
}


// Progress inside the step. Nothing is ticked before the portfolio reaches this step, everything once it has passed it.
const stages = computed(() => {
  const d = data.value
  if (!d) return []
  return buildStages(stepPhase('VALUATION_COMPLETED', d.portfolioStatusCode), [
    { label: 'Record valuations', done: totals.value.count > 0 },
    { label: 'Set recommended values', done: checks.value[1]!.ok },
    { label: 'Complete step', done: false }
  ])
})

// Messages open the app-wide pop-up; saving shows the progress screen.
useFeedback({
  errors: [error, formError],
  notices: [notice],
  busy,
  busyAs: () => ({ title: 'Saving the valuation', kind: 'save', steps: ['Checking the valuation', 'Saving the figures', 'Updating the portfolio'] })
})
</script>

<template>
  <div v-if="error && !data" class="cp-page"><div class="cp-error">{{ error }}</div></div>
  <div v-else-if="!data" class="cp-page"><div class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div></div>

  <StepLayout
    v-else
    :portfolio-id="data.portfolioId"
    title="Portfolio valuation"
    lead="Record the valuations of the project and its units, compare the recommended values with the outstanding balance, then complete this workflow step."
    :portfolio-no="data.portfolioNo"
    :project-name="data.projectName"
    :project-code="data.projectCode"
    :status-name="data.portfolioStatusName"
    :stages="stages"
    apex-page="401"
  >
    <div v-if="!data.isOpen" class="readonly"><i class="pi pi-eye"></i> Read-only view. Valuation is not open for a portfolio in status {{ data.portfolioStatusName }}.</div>
    <div v-else-if="!canAct" class="readonly"><i class="pi pi-eye"></i> Read-only view. Recording valuations needs the valuation permission.</div>

    <div class="grid">
      <div class="stack">
        <section class="cp-card">
          <div class="cp-card__head">
            <i class="pi pi-calculator"></i><h3>Valuations</h3><span class="cp-spacer"></span>
            <span class="cp-muted">{{ totals.count }} recorded</span>
            <button v-if="editable" type="button" class="cp-btn cp-btn--small cp-btn--create" @click="openForm(null)"><i class="pi pi-plus"></i><span>Add valuation</span></button>
          </div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Valuation</th><th>Scope</th><th>Valuer</th><th>Date</th><th class="cp-num">Market value</th><th class="cp-num">Recommended</th><th class="cp-num">Completion</th><th>Approved</th><th></th></tr></thead>
              <tbody>
                <tr v-for="v in data.valuations" :key="v.valuationId" :title="v.facilitiesStatus ?? ''">
                  <td><strong>{{ v.valuationTypeName }}</strong><br /><small class="cp-muted">{{ v.createdBy }}, {{ formatDateTime(v.createdAt) }}</small></td>
                  <td><span v-if="v.unitCode" class="code">Unit {{ v.unitCode }}</span><span v-else>Whole project</span></td>
                  <td>{{ v.valuerName }}<br /><small v-if="v.valuerRegistrationNo" class="cp-muted">Reg. {{ v.valuerRegistrationNo }}</small></td>
                  <td>{{ formatDate(v.valuationDate + 'T00:00:00Z') }}</td>
                  <td class="cp-num">{{ formatAmount(v.marketValueAmount) }}</td>
                  <td class="cp-num"><template v-if="v.recommendedValueAmount !== null">{{ formatAmount(v.recommendedValueAmount) }}</template><span v-else class="cp-pill cp-pill--warn">Missing</span></td>
                  <td class="cp-num">{{ formatPct(v.projectCompletionPct) }}</td>
                  <td><template v-if="v.approvedBy || v.approvedDate"><small>{{ v.approvedBy ?? '-' }}<br /><span class="cp-muted">{{ v.approvedDate ? formatDate(v.approvedDate + 'T00:00:00Z') : '' }}</span></small></template><span v-else class="cp-muted">-</span></td>
                  <td><button v-if="editable" type="button" class="cp-btn cp-btn--small" :aria-label="`Edit ${v.valuationTypeName}`" @click="openForm(v)"><i class="pi pi-pencil"></i></button></td>
                </tr>
                <tr v-if="data.valuations.length === 0"><td colspan="9" class="cp-empty">No valuations recorded yet. Start with Add valuation.</td></tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>

      <aside class="stack">
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-chart-bar"></i><h3>Value check</h3></div>
          <div class="cp-card__body">
            <div class="kpis">
              <div><b :title="formatAmount(totals.market)">{{ totals.count ? formatShort(totals.market) : '-' }}</b><small>Total market value</small></div>
              <div><b :title="formatAmount(totals.recommended)">{{ totals.withRecommended ? formatShort(totals.recommended) : '-' }}</b><small>Total recommended</small></div>
            </div>
            <dl class="cp-kv">
              <dt>Face value</dt><dd :title="formatAmount(data.faceValueAmount)">{{ formatShort(data.faceValueAmount) }}</dd>
              <dt>Outstanding</dt><dd :title="formatAmount(data.outstandingAmount)">{{ formatShort(data.outstandingAmount) }}</dd>
            </dl>
            <div class="cover">
              <span class="cp-muted">Recommended value covers outstanding</span>
              <div class="bar" :class="{ full: totals.coverage >= 100 }"><i :style="{ width: Math.min(100, totals.coverage) + '%' }"></i></div>
              <small v-if="!totals.withRecommended" class="cp-muted">Add recommended values to see how they compare with the outstanding balance.</small>
              <small v-else-if="totals.outstanding <= 0" class="cp-muted">The outstanding balance of the kept contracts is zero.</small>
              <small v-else>
                {{ totals.coverage }}% of the outstanding balance<template v-if="totals.coverage < 100"> (shortfall {{ formatAmount(totals.outstanding - totals.recommended) }})</template>
              </small>
            </div>
            <p class="cp-muted note">Face value and outstanding are for the contracts kept in the deal.</p>
          </div>
        </section>

        <section v-if="data.isOpen" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-list-check"></i><h3>Progress</h3></div>
          <ul class="ready cp-card__body">
            <li v-for="c in checks" :key="c.label" :class="{ ok: c.ok }"><i :class="c.ok ? 'pi pi-check-circle' : 'pi pi-circle'"></i>{{ c.label }}</li>
          </ul>
        </section>

        <section v-if="editable" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-check-circle"></i><h3>Complete step</h3></div>
          <div class="cp-card__body">
            <div class="cp-field">
              <label for="val-comment">Comment</label>
              <textarea id="val-comment" v-model="comment" rows="4" maxlength="4000" placeholder="Optional"></textarea>
            </div>
            <p v-if="totals.count && !checks[1]!.ok" class="hint">You can still complete the step, but some valuations have no recommended value.</p>
            <div class="decide">
              <button type="button" class="cp-btn cp-btn--primary" :disabled="busy || !totals.count" @click="complete"><i class="pi pi-check"></i><span>Complete valuation step</span></button>
            </div>
          </div>
        </section>
      </aside>
    </div>

    <div v-if="formOpen" class="cp-modal-overlay" @click.self="formOpen = false">
      <section class="cp-modal" role="dialog" aria-modal="true" aria-labelledby="val-title">
        <header class="cp-modal__header">
          <div><h3 id="val-title">{{ editing ? 'Edit valuation' : 'Add valuation' }}</h3><p>Who valued what, when, and for how much.</p></div>
          <button type="button" class="cp-modal__close" aria-label="Close" @click="formOpen = false"><i class="pi pi-times"></i></button>
        </header>
        <div class="cp-modal__body">
          <div class="cp-form">
            <div class="cp-field cp-field--wide">
              <label>Valuation type <span class="req">*</span></label>
              <div class="chips">
                <button v-for="t in data.valuationTypes" :key="t.code" type="button" :aria-pressed="form.valuationTypeCode === t.code" @click="form.valuationTypeCode = t.code">{{ t.name }}</button>
                <span v-if="data.valuationTypes.length === 0" class="cp-muted">No active valuation types. Add them in Reference Data.</span>
              </div>
            </div>
            <div class="cp-field">
              <label for="val-unit">Scope</label>
              <select id="val-unit" v-model="form.projectUnitId">
                <option value="">Whole project</option>
                <option v-for="u in data.units" :key="u.projectUnitId" :value="u.projectUnitId">Unit {{ u.unitCode }}{{ u.unitTypeCode ? ` (${u.unitTypeCode})` : '' }}</option>
              </select>
            </div>
            <div class="cp-field"><label for="val-date">Valuation date <span class="req">*</span></label><input id="val-date" v-model="form.valuationDate" type="date" /></div>
            <div class="cp-field"><label for="val-valuer">Valuer name <span class="req">*</span></label><input id="val-valuer" v-model="form.valuerName" maxlength="200" /></div>
            <div class="cp-field"><label for="val-reg">Valuer registration no.</label><input id="val-reg" v-model="form.valuerRegistrationNo" maxlength="100" /></div>
            <div class="cp-field"><label for="val-market">Market value <span class="req">*</span></label><input id="val-market" v-model="form.marketValueAmount" type="number" min="0" step="0.01" /></div>
            <div class="cp-field"><label for="val-recommended">Recommended value</label><input id="val-recommended" v-model="form.recommendedValueAmount" type="number" min="0" step="0.01" /></div>
            <div class="cp-field"><label for="val-completion">Project completion %</label><input id="val-completion" v-model="form.projectCompletionPct" type="number" min="0" max="100" step="0.01" /></div>
            <div class="cp-field"><label for="val-approved-by">Approved by</label><input id="val-approved-by" v-model="form.approvedBy" maxlength="255" /></div>
            <div class="cp-field"><label for="val-approved-date">Approved date</label><input id="val-approved-date" v-model="form.approvedDate" type="date" /></div>
            <div class="cp-field cp-field--wide"><label for="val-facilities">Facilities status</label><textarea id="val-facilities" v-model="form.facilitiesStatus" rows="3" maxlength="4000" placeholder="Roads, utilities and shared facilities"></textarea></div>
          </div>
        </div>
        <footer class="cp-modal__footer">
          <button v-if="editing" type="button" class="cp-btn cp-btn--danger" :disabled="busy" @click="removeValuation">
            <i class="pi pi-trash"></i><span>{{ confirmDelete ? 'Click again to delete' : 'Delete' }}</span>
          </button>
          <span class="cp-spacer"></span>
          <button type="button" class="cp-btn" :disabled="busy" @click="formOpen = false">Cancel</button>
          <button type="button" class="cp-btn cp-btn--primary" :disabled="busy" @click="saveForm"><i class="pi pi-check"></i><span>Save valuation</span></button>
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
.code { color: var(--cap-heading); font-weight: 600; font-size: 11px; }
.kpis { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; margin-bottom: 14px; }
.kpis div { display: grid; gap: 2px; padding: 12px; border-radius: 10px; background: var(--cap-bg); }
.kpis b { font-size: 20px; color: var(--cap-heading); }
.kpis small { font-size: 11px; color: var(--cap-muted); }
.cover { display: grid; gap: 6px; margin-top: 14px; font-size: 12px; }
.bar { height: 8px; border-radius: 999px; background: var(--cap-border-soft); overflow: hidden; }
.bar i { display: block; height: 100%; border-radius: inherit; background: var(--cap-warning); }
.bar.full i { background: var(--cap-success); }
.note { margin: 12px 0 0; font-size: 11px; }
.ready { list-style: none; margin: 0; display: grid; gap: 8px; font-size: 13px; color: var(--cap-muted); }
.ready li { display: flex; align-items: center; gap: 8px; }
.ready li.ok { color: var(--cap-success); }
.hint { margin: 10px 0 0; font-size: 12px; color: var(--cap-warning); }
.decide { display: flex; justify-content: flex-end; gap: 8px; margin-top: 12px; }
.req { color: var(--cap-danger); }
.chips { display: flex; flex-wrap: wrap; gap: 6px; }
.chips button { height: 32px; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 999px; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.chips button[aria-pressed="true"] { border-color: var(--cap-selected); background: var(--cap-selected); color: #fff; }
@media (max-width: 1100px) { .grid { grid-template-columns: minmax(0, 1fr); } }
</style>
