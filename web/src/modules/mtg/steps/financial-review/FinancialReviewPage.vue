<script setup lang="ts">
// Workflow step "Portfolio Financial Review" (APEX App 105 page 214): price the portfolio from the cheques of the
// investors kept in the deal, save the figures, then accept (send to the board) or reject. Every figure on this page
// comes from the API's calculator; the page only collects the choices.
import { computed, reactive, ref, shallowRef, watch } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import { formatAmount, formatPct, formatShort } from '@/shared/format/money'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { buildStages, stepPhase } from '../stepPhase'
import StepLayout from '../StepLayout.vue'
import { messageOf } from '../stepUtils'
import {
  ADMIN_BASES, MARGIN_BASES, PV_METHODS, financialReviewApi,
  type FinancialCalculation, type FinancialInputs, type FinancialReview
} from './financialReview.api'

type Charge = 'admin' | 'legal' | 'valuation'

const props = defineProps<{ portfolioId: string }>()
const canAct = hasPermission('MTG.STEP.FINANCIAL_REVIEW')

const data = shallowRef<FinancialReview | null>(null)
const calc = shallowRef<FinancialCalculation | null>(null)
const error = ref('')
const calcError = ref('')
const notice = ref('')
const busy = ref(false)
const calculating = ref(false)

const id = computed(() => Number(props.portfolioId))
const editable = computed(() => canAct && !!data.value?.isOpen)

const inputs = reactive<FinancialInputs>({
  pvMethodCode: 'ANNUAL', marginBaseCode: 'SELECTED_OUTSTANDING', adminBaseCode: 'SELECTED_OUTSTANDING',
  adminFinalAmount: null, isAdminIncluded: true, legalFinalAmount: null, isLegalIncluded: true,
  valuationFinalAmount: null, isValuationIncluded: true
})
const savedInputs = ref('')

function adopt(review: FinancialReview) {
  data.value = review
  calc.value = review.calculation
  Object.assign(inputs, review.inputs)
  savedInputs.value = JSON.stringify(review.inputs)
}

async function load() {
  try {
    adopt(await financialReviewApi.get(id.value))
  } catch (e) {
    error.value = messageOf(e, 'Unable to load the financial review.')
  }
}

watch(id, () => {
  data.value = null
  error.value = ''
  load()
}, { immediate: true })

const dirty = computed(() => !data.value?.saved || JSON.stringify(inputs) !== savedInputs.value)

// Recalculate on the server shortly after a choice changes.
let timer: ReturnType<typeof setTimeout> | undefined
watch(inputs, () => {
  if (!data.value || data.value.terms.pvRate === null) return
  clearTimeout(timer)
  timer = setTimeout(async () => {
    calculating.value = true
    try {
      calc.value = await financialReviewApi.calculate(id.value, { ...inputs })
      calcError.value = ''
    } catch (e) {
      calcError.value = messageOf(e, 'Unable to calculate.')
    } finally {
      calculating.value = false
    }
  }, 300)
})

// ----- charges -----
const charges = computed(() => {
  const c = calc.value
  if (!c) return []
  return [
    { key: 'admin' as Charge, label: 'Admin expense', basis: `${formatPct(c.adminExpensePct)} of ${formatAmount(c.adminBaseAmount)}`, calc: c.adminCalcAmount, final: c.adminFinalAmount, included: c.isAdminIncluded },
    { key: 'legal' as Charge, label: 'Legal fees', basis: `${formatAmount(c.legalFeePerUnitAmount)} × ${c.unitCount} units`, calc: c.legalCalcAmount, final: c.legalFinalAmount, included: c.isLegalIncluded },
    { key: 'valuation' as Charge, label: 'Valuation fees', basis: `${formatAmount(c.valuationFeePerUnitAmount)} × ${c.unitCount} units`, calc: c.valuationCalcAmount, final: c.valuationFinalAmount, included: c.isValuationIncluded }
  ]
})

const overrideField = { admin: 'adminFinalAmount', legal: 'legalFinalAmount', valuation: 'valuationFinalAmount' } as const
const includedField = { admin: 'isAdminIncluded', legal: 'isLegalIncluded', valuation: 'isValuationIncluded' } as const

function setOverride(key: Charge, value: string) {
  inputs[overrideField[key]] = value.trim() === '' ? null : Number(value)
}

// ----- save / decide -----
async function save() {
  busy.value = true
  error.value = ''
  notice.value = ''
  try {
    adopt(await financialReviewApi.save(id.value, { ...inputs }, data.value?.saved?.rowVersion ?? null))
    notice.value = 'Financial review saved. The portfolio now carries this purchase price.'
  } catch (e) {
    error.value = messageOf(e, 'Unable to save the financial review.')
  } finally {
    busy.value = false
  }
}

const comment = ref('')

async function finish(action: 'ACCEPT' | 'REJECT') {
  busy.value = true
  error.value = ''
  notice.value = ''
  try {
    adopt(await financialReviewApi.decide(id.value, action, comment.value))
    notice.value = action === 'ACCEPT' ? 'Financial review completed. The portfolio goes to the board.' : 'Financial review rejected.'
    comment.value = ''
  } catch (e) {
    error.value = messageOf(e, 'Unable to record the decision.')
  } finally {
    busy.value = false
  }
}

const date = (d: string) => formatDate(d + 'T00:00:00Z')


// Progress inside the step. Nothing is ticked before the portfolio reaches this step, everything once it has passed it.
const stages = computed(() => {
  const d = data.value
  if (!d) return []
  return buildStages(stepPhase('FINANCIAL_REVIEW_ACCEPTED', d.portfolioStatusCode), [
    { label: 'Choose the method', done: !!d.saved },
    { label: 'Save the figures', done: !!d.saved && !dirty.value },
    { label: 'Complete step', done: false }
  ])
})

// Messages open the app-wide pop-up; saving shows the progress screen.
useFeedback({
  errors: [error, calcError],
  notices: [notice],
  busy,
  busyAs: () => ({ title: 'Working on the financial review', kind: 'calc', steps: ['Reading cheques and terms', 'Calculating present values and charges', 'Saving the results'] })
})
</script>

<template>
  <div v-if="error && !data" class="cp-page"><div class="cp-error">{{ error }}</div></div>
  <div v-else-if="!data" class="cp-page"><div class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div></div>

  <StepLayout
    v-else
    :portfolio-id="data.portfolioId"
    title="Portfolio financial review"
    lead="Price the portfolio from the cheques of the investors kept in the deal: present value, less margin, less charges. Save the figures, then complete or reject this workflow step."
    :portfolio-no="data.portfolioNo"
    :project-name="data.projectName"
    :project-code="data.projectCode"
    :status-name="data.portfolioStatusName"
    :status-code="data.portfolioStatusCode"
    :stages="stages"
    apex-page="214"
  >
    <div v-if="data.rejectionReason && data.isOpen" class="rejected"><i class="pi pi-replay"></i> Last rejected: {{ data.rejectionReason }}</div>
    <div v-if="!data.isOpen" class="readonly"><i class="pi pi-eye"></i> Read-only view. The financial review is not open for a portfolio in status {{ data.portfolioStatusName }}.</div>
    <div v-else-if="!canAct" class="readonly"><i class="pi pi-eye"></i> Read-only view. Saving the financial review needs the financial review permission.</div>

    <div v-if="data.terms.pvRate === null" class="cp-card cp-card__body missing">
      <i class="pi pi-exclamation-triangle"></i>
      <span>Set the Present Value Calculation Rate on the portfolio's financial terms first.</span>
      <RouterLink :to="{ name: 'mtg-portfolio', params: { portfolioId: data.portfolioId } }">Open the portfolio</RouterLink>
    </div>

    <div v-else class="grid">
      <div class="stack">
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-sliders-h"></i><h3>Method</h3><span class="cp-spacer"></span><small v-if="calculating" class="cp-muted"><i class="pi pi-spin pi-spinner"></i> Calculating</small></div>
          <div class="cp-card__body choices">
            <div class="cp-field">
              <label>Present value</label>
              <div class="seg" role="group" aria-label="Present value method">
                <button v-for="m in PV_METHODS" :key="m.code" type="button" :aria-pressed="inputs.pvMethodCode === m.code" :disabled="!editable" @click="inputs.pvMethodCode = m.code">{{ m.label }}</button>
              </div>
              <small class="cp-muted">{{ PV_METHODS.find(m => m.code === inputs.pvMethodCode)?.formula }}, at {{ formatPct(data.terms.pvRate) }} a year</small>
            </div>
            <div class="cp-field">
              <label for="fr-margin-base">Margin on</label>
              <select id="fr-margin-base" v-model="inputs.marginBaseCode" :disabled="!editable">
                <option v-for="b in MARGIN_BASES" :key="b.code" :value="b.code">{{ b.label }}</option>
              </select>
            </div>
            <div class="cp-field">
              <label for="fr-admin-base">Admin expense on</label>
              <select id="fr-admin-base" v-model="inputs.adminBaseCode" :disabled="!editable">
                <option v-for="b in ADMIN_BASES" :key="b.code" :value="b.code">{{ b.label }}</option>
              </select>
            </div>
          </div>
        </section>

        <section v-if="calc" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-receipt"></i><h3>Charges</h3><span class="cp-spacer"></span><small class="cp-muted">Leave the amount empty to use the calculated one.</small></div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Charge</th><th>Basis</th><th class="cp-num">Calculated</th><th class="cp-num">Amount used</th><th>Deduct</th></tr></thead>
              <tbody>
                <tr v-for="c in charges" :key="c.key" :class="{ out: !c.included }">
                  <td><strong>{{ c.label }}</strong></td>
                  <td><small class="cp-muted">{{ c.basis }}</small></td>
                  <td class="cp-num">{{ formatAmount(c.calc) }}</td>
                  <td class="cp-num">
                    <input
                      class="amount"
                      type="number"
                      min="0"
                      step="0.01"
                      :aria-label="`${c.label} amount`"
                      :placeholder="String(c.calc)"
                      :value="inputs[overrideField[c.key]] ?? ''"
                      :disabled="!editable"
                      @change="setOverride(c.key, ($event.target as HTMLInputElement).value)"
                    />
                  </td>
                  <td>
                    <div class="yn" role="group" :aria-label="`Deduct ${c.label}`">
                      <button type="button" :aria-pressed="inputs[includedField[c.key]]" :disabled="!editable" @click="inputs[includedField[c.key]] = true">Yes</button>
                      <button type="button" :aria-pressed="!inputs[includedField[c.key]]" :disabled="!editable" @click="inputs[includedField[c.key]] = false">No</button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <section v-if="calc" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-wallet"></i><h3>Cheques valued</h3><span class="cp-spacer"></span><span class="cp-muted">{{ calc.chequeCount }} cheques, discounted to {{ date(calc.pvBaseDate) }}</span></div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Cheque</th><th>Investor</th><th>Due date</th><th class="cp-num">Days</th><th class="cp-num">Months</th><th class="cp-num">Amount</th><th class="cp-num">Present value</th></tr></thead>
              <tbody>
                <tr v-for="c in calc.cheques" :key="c.chequeId">
                  <td><strong>{{ c.chequeNo }}</strong></td>
                  <td>{{ c.investorName }}</td>
                  <td>{{ date(c.dueDate) }}</td>
                  <td class="cp-num">{{ c.days }}</td>
                  <td class="cp-num">{{ c.months.toFixed(2) }}</td>
                  <td class="cp-num">{{ formatAmount(c.chequeAmount) }}</td>
                  <td class="cp-num">{{ formatAmount(c.pvAmount) }}</td>
                </tr>
                <tr v-if="calc.cheques.length === 0"><td colspan="7" class="cp-empty">There are no cheques to value for the included investors.</td></tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>

      <aside class="stack">
        <section v-if="calc" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-calculator"></i><h3>Purchase price</h3></div>
          <div class="cp-card__body">
            <dl class="fall">
              <dt>Cheques at face</dt><dd>{{ formatAmount(calc.chequesFaceAmount) }}</dd>
              <dt>Present value</dt><dd>{{ formatAmount(calc.chequesPvAmount) }}</dd>
              <dt>Margin <small>{{ formatPct(calc.marginPct) }} of {{ formatShort(calc.marginBaseAmount) }}</small></dt><dd class="minus">− {{ formatAmount(calc.marginAmount) }}</dd>
              <dt>Charges deducted</dt><dd class="minus">− {{ formatAmount(calc.totalChargesAmount) }}</dd>
            </dl>
            <div class="price">
              <small>Proposed purchase value</small>
              <b :class="{ bad: calc.proposedPurchaseAmount <= 0 }">{{ formatAmount(calc.proposedPurchaseAmount) }}</b>
              <small>{{ data.currencyCode }} · expected return {{ formatAmount(calc.expectedReturnAmount) }}</small>
            </div>
          </div>
        </section>

        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-save"></i><h3>Saved figures</h3></div>
          <div class="cp-card__body">
            <template v-if="data.saved">
              <dl class="cp-kv">
                <dt>Purchase value</dt><dd>{{ formatAmount(data.saved.proposedPurchaseAmount) }}</dd>
                <dt>Expected return</dt><dd>{{ formatAmount(data.saved.expectedReturnAmount) }}</dd>
                <dt>Valued on</dt><dd>{{ date(data.saved.pvBaseDate) }}</dd>
              </dl>
              <small class="cp-muted">Saved by {{ data.saved.savedBy }}, {{ formatDateTime(data.saved.savedAt) }}.</small>
            </template>
            <p v-else class="cp-muted none">Nothing saved yet.</p>
            <p v-if="editable && dirty && data.saved" class="hint">The choices above differ from the saved figures.</p>
            <div v-if="editable" class="decide">
              <button type="button" class="cp-btn cp-btn--primary" :disabled="busy || calculating" @click="save"><i class="pi pi-save"></i><span>Save financial review</span></button>
            </div>
          </div>
        </section>

        <section v-if="editable" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-check-circle"></i><h3>Decide</h3></div>
          <div class="cp-card__body">
            <div class="cp-field">
              <label for="fr-comment">Comment <span class="req">*</span></label>
              <textarea id="fr-comment" v-model="comment" rows="4" maxlength="4000" placeholder="Required for the decision"></textarea>
            </div>
            <p v-if="dirty" class="hint">Save the financial review before completing the step.</p>
            <div class="decide">
              <button type="button" class="cp-btn cp-btn--danger" :disabled="busy" @click="finish('REJECT')"><i class="pi pi-times"></i><span>Reject</span></button>
              <button type="button" class="cp-btn cp-btn--primary" :disabled="busy || dirty" @click="finish('ACCEPT')"><i class="pi pi-check"></i><span>Complete & send to board</span></button>
            </div>
          </div>
        </section>
      </aside>
    </div>
  </StepLayout>
</template>

<style scoped>
.notice { padding: 10px 14px; border: 1px solid var(--cap-success-border); border-radius: 8px; background: var(--cap-success-bg); color: var(--cap-success); font-size: 13px; }
.readonly { padding: 10px 14px; border-radius: 8px; background: var(--cap-bg); color: var(--cap-ink-soft); font-size: 13px; }
.rejected { padding: 10px 14px; border: 1px solid var(--cap-danger-border); border-radius: 8px; background: var(--cap-danger-bg); color: var(--cap-danger-dark); font-size: 13px; }
.missing { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; font-size: 13px; color: var(--cap-warning); }
.grid { display: grid; grid-template-columns: minmax(0, 1fr) 340px; gap: 16px; align-items: start; }
.stack { display: grid; grid-template-columns: minmax(0, 1fr); gap: 16px; }
.choices { display: grid; grid-template-columns: minmax(0, 1.3fr) minmax(0, 1fr) minmax(0, 1fr); gap: 16px; align-items: start; }
.choices .cp-field small { margin-top: 4px; font-size: 11px; }
.inline { margin: 0 20px 16px; }
.seg { display: inline-flex; width: fit-content; border: 1px solid var(--cap-border); border-radius: 8px; overflow: hidden; }
.seg button { height: 34px; padding: 0 14px; border: 0; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.seg button + button { border-inline-start: 1px solid var(--cap-border); }
.seg button[aria-pressed="true"] { background: var(--cap-selected); color: #fff; }
.seg button:disabled { cursor: default; }
.amount { width: 130px; height: 32px; padding: 0 8px; border: 1px solid var(--cap-border); border-radius: 6px; font: inherit; font-size: 12px; text-align: end; }
.yn { display: inline-flex; border: 1px solid var(--cap-border); border-radius: 8px; overflow: hidden; }
.yn button { height: 30px; padding: 0 12px; border: 0; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.yn button + button { border-inline-start: 1px solid var(--cap-border); }
.yn button[aria-pressed="true"]:first-child { background: var(--cap-success); color: #fff; }
.yn button[aria-pressed="true"]:last-child { background: var(--cap-muted); color: #fff; }
tr.out td { opacity: .55; }
tr.out td:last-child { opacity: 1; }
.fall { display: grid; grid-template-columns: 1fr auto; gap: 8px 12px; margin: 0; font-size: 13px; }
.fall dt { color: var(--cap-ink-soft); }
.fall dt small { display: block; font-size: 11px; color: var(--cap-muted); }
.fall dd { margin: 0; text-align: end; font-variant-numeric: tabular-nums; }
.fall dd.minus { color: var(--cap-danger-dark); }
.price { display: grid; gap: 2px; margin-top: 14px; padding: 14px; border-radius: 10px; background: var(--cap-bg); }
.price small { font-size: 11px; color: var(--cap-muted); }
.price b { font-size: 22px; color: var(--cap-heading); font-variant-numeric: tabular-nums; }
.price b.bad { color: var(--cap-danger); }
.none { margin: 0; font-size: 13px; }
.hint { margin: 10px 0 0; font-size: 12px; color: var(--cap-warning); }
.decide { display: flex; flex-wrap: wrap; justify-content: flex-end; gap: 8px; margin-top: 12px; }
.req { color: var(--cap-danger); }
@media (max-width: 1100px) { .grid { grid-template-columns: minmax(0, 1fr); } }
@media (max-width: 760px) { .choices { grid-template-columns: minmax(0, 1fr); } }
</style>
