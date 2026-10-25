<script setup lang="ts">
// Workflow step "Portfolio Board Review" (APEX App 105 page 215): one report of what every earlier step found and the
// price the financial review worked out, then the board's resolution (number, date, comment) as approval or rejection.
import { computed, ref, shallowRef, watch } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { formatDate, formatDateTime, todayInCairo } from '@/shared/format/datetime'
import { formatAmount, formatPct } from '@/shared/format/money'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { buildStages, stepPhase } from '../stepPhase'
import StepLayout from '../StepLayout.vue'
import { messageOf, niceCode } from '../stepUtils'
import { boardReviewApi, type BoardAction, type BoardReview } from './boardReview.api'

const props = defineProps<{ portfolioId: string }>()
const canAct = hasPermission('MTG.STEP.BOARD_REVIEW')

const data = shallowRef<BoardReview | null>(null)
const error = ref('')
const notice = ref('')
const busy = ref(false)

const id = computed(() => Number(props.portfolioId))
const editable = computed(() => canAct && !!data.value?.isOpen)

async function load() {
  try {
    data.value = await boardReviewApi.get(id.value)
  } catch (e) {
    error.value = messageOf(e, 'Unable to load the board report.')
  }
}

watch(id, () => {
  data.value = null
  error.value = ''
  load()
}, { immediate: true })

const date = (d: string | null) => (d ? formatDate(d + 'T00:00:00Z') : '–')
const pct = (v: number | null) => (v === null ? '–' : formatPct(v))
const amount = (v: number | null) => (v === null ? '–' : formatAmount(v))
const plural = (n: number, word: string) => `${n} ${word}${n === 1 ? '' : 's'}`

// ----- report -----
const legalChecks = computed(() => {
  const l = data.value?.dueDiligence.legal
  if (!l) return []
  return [
    { label: 'Ownership verified', ok: l.isOwnershipVerified },
    { label: 'Building license verified', ok: l.isBuildingLicenseVerified },
    { label: 'Title deed verified', ok: l.isTitleDeedVerified },
    { label: 'Free of encumbrance', ok: l.isEncumbranceFree },
    { label: 'Registration possible', ok: l.isRegistrationPossible },
    { label: 'Mortgage possible', ok: l.isMortgagePossible }
  ]
})

const deductions = computed(() => {
  const p = data.value?.pricing
  if (!p) return []
  return [
    { label: `Margin (${formatPct(p.marginPct)})`, value: p.marginAmount },
    { label: 'Admin expense', value: p.adminDeductedAmount },
    { label: 'Legal fees', value: p.legalDeductedAmount },
    { label: 'Valuation fees', value: p.valuationDeductedAmount }
  ].filter(x => x.value !== 0)
})

const terms = computed(() => {
  const t = data.value?.terms
  if (!t) return []
  return [
    { label: 'Coverage', value: pct(t.coveragePct) },
    { label: 'Margin', value: pct(t.marginPct) },
    { label: 'PV rate', value: pct(t.pvRate) },
    { label: 'Reserved amount', value: pct(t.reservedAmountPct) },
    { label: 'Late fee rate', value: pct(t.lateFeeRate) },
    { label: 'Admin expense', value: pct(t.adminExpensePct) },
    { label: 'Legal fee / unit', value: amount(t.legalFeePerUnitAmount) },
    { label: 'Valuation fee / unit', value: amount(t.valuationFeePerUnitAmount) },
    { label: 'Term', value: t.termMonths === null ? '–' : `${t.termMonths} months` }
  ]
})

// ----- decision -----
const decisionNo = ref('')
const decisionDate = ref(todayInCairo())
const comment = ref('')

async function decide(action: BoardAction) {
  busy.value = true
  error.value = ''
  notice.value = ''
  try {
    data.value = await boardReviewApi.decide(id.value, {
      action, decisionNo: decisionNo.value, decisionDate: decisionDate.value || null, comment: comment.value
    })
    notice.value = action === 'ACCEPT' ? 'Board approval recorded.' : 'Board rejection recorded.'
    decisionNo.value = ''
    comment.value = ''
  } catch (e) {
    error.value = messageOf(e, 'Unable to record the board decision.')
  } finally {
    busy.value = false
  }
}


// Progress inside the step. Nothing is ticked before the portfolio reaches this step, everything once it has passed it.
const stages = computed(() => {
  const d = data.value
  if (!d) return []
  return buildStages(stepPhase('BOARD_REVIEW_ACCEPTED', d.portfolioStatusCode), [
    { label: 'Read the report', done: !!d.pricing },
    { label: 'Record the resolution', done: !!d.decision }
  ])
})

// Messages open the app-wide pop-up; saving shows the progress screen.
useFeedback({
  errors: [error],
  notices: [notice],
  busy,
  busyAs: () => ({ title: 'Recording the board decision', kind: 'save', steps: ['Checking the resolution', 'Saving the decision', 'Updating the portfolio'] })
})
</script>

<template>
  <div v-if="error && !data" class="cp-page"><div class="cp-error">{{ error }}</div></div>
  <div v-else-if="!data" class="cp-page"><div class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div></div>

  <StepLayout
    v-else
    :portfolio-id="data.portfolioId"
    title="Portfolio board review"
    lead="Everything the earlier steps found and the price the financial review worked out, in one report for the board. Record the board's resolution to approve or reject the purchase."
    :portfolio-no="data.portfolioNo"
    :project-name="data.projectName"
    :project-code="data.projectCode"
    :status-name="data.portfolioStatusName"
    :stages="stages"
    apex-page="215"
  >
    <div v-if="data.rejectionReason && data.isOpen" class="rejected"><i class="pi pi-replay"></i> Last rejected: {{ data.rejectionReason }}</div>
    <div v-if="!data.isOpen" class="readonly"><i class="pi pi-eye"></i> Read-only view. The board review is not open for a portfolio in status {{ data.portfolioStatusName }}.</div>
    <div v-else-if="!canAct" class="readonly"><i class="pi pi-eye"></i> Read-only view. Recording the board decision needs the board review permission.</div>

    <div class="grid">
      <div class="stack">
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-flag"></i><h3>Recommendation</h3></div>
          <div class="cp-card__body">
            <div class="kpis">
              <div class="kpi main"><small>Proposed purchase value</small><b>{{ data.pricing ? formatAmount(data.pricing.proposedPurchaseAmount) : '–' }}</b><small>{{ data.currencyCode }}</small></div>
              <div class="kpi"><small>Expected return</small><b>{{ data.pricing ? formatAmount(data.pricing.expectedReturnAmount) : '–' }}</b></div>
              <div class="kpi"><small>Kept outstanding</small><b>{{ formatAmount(data.totals.selectedOutstandingAmount) }}</b></div>
              <div class="kpi"><small>Kept in the deal</small><b>{{ data.totals.selectedContractCount }} / {{ data.totals.contractCount }}</b><small>contracts, {{ plural(data.totals.selectedInvestorCount, 'investor') }}, {{ plural(data.totals.selectedUnitCount, 'unit') }}</small></div>
            </div>
            <p class="meta">Developer {{ data.developerName }} · requested {{ date(data.requestDate) }} · sent to the board {{ date(data.submittedDate) }}</p>
          </div>
        </section>

        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-table"></i><h3>The portfolio</h3></div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th></th><th class="cp-num">Contracts</th><th class="cp-num">Investors</th><th class="cp-num">Face value</th><th class="cp-num">Paid</th><th class="cp-num">Outstanding</th></tr></thead>
              <tbody>
                <tr><td><strong>All loaded</strong></td><td class="cp-num">{{ data.totals.contractCount }}</td><td class="cp-num">{{ data.totals.investorCount }}</td><td class="cp-num">{{ formatAmount(data.totals.faceValueAmount) }}</td><td class="cp-num">{{ formatAmount(data.totals.paidAmount) }}</td><td class="cp-num">{{ formatAmount(data.totals.outstandingAmount) }}</td></tr>
                <tr><td><strong>Kept in the deal</strong></td><td class="cp-num">{{ data.totals.selectedContractCount }}</td><td class="cp-num">{{ data.totals.selectedInvestorCount }}</td><td class="cp-num">{{ formatAmount(data.totals.selectedFaceValueAmount) }}</td><td class="cp-num">{{ formatAmount(data.totals.selectedPaidAmount) }}</td><td class="cp-num">{{ formatAmount(data.totals.selectedOutstandingAmount) }}</td></tr>
              </tbody>
            </table>
          </div>
          <div class="cp-card__body foot"><small class="cp-muted">{{ plural(data.totals.chequeCount, 'cheque') }} from kept investors, {{ formatAmount(data.totals.chequeAmount) }} in total.</small></div>
        </section>

        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-calculator"></i><h3>Pricing</h3></div>
          <div class="cp-card__body two">
            <div>
              <template v-if="data.pricing">
                <dl class="fall">
                  <dt>Cheques at face <small>{{ data.pricing.chequeCount }} cheques</small></dt><dd>{{ formatAmount(data.pricing.chequesFaceAmount) }}</dd>
                  <dt>Present value <small>{{ niceCode(data.pricing.pvMethodCode) }} at {{ formatPct(data.pricing.pvRate) }}, to {{ date(data.pricing.pvBaseDate) }}</small></dt><dd>{{ formatAmount(data.pricing.chequesPvAmount) }}</dd>
                  <template v-for="d in deductions" :key="d.label"><dt>{{ d.label }}</dt><dd class="minus">− {{ formatAmount(d.value) }}</dd></template>
                  <dt class="total">Proposed purchase value</dt><dd class="total">{{ formatAmount(data.pricing.proposedPurchaseAmount) }}</dd>
                </dl>
              </template>
              <p v-else class="cp-muted none">The financial review has not been saved.</p>
            </div>
            <dl class="cp-kv terms">
              <template v-for="t in terms" :key="t.label"><dt>{{ t.label }}</dt><dd>{{ t.value }}</dd></template>
            </dl>
          </div>
        </section>

        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-shield"></i><h3>Due diligence</h3></div>
          <div class="cp-card__body dd">
            <div class="dd-item">
              <h4>AML screening</h4>
              <span class="cp-pill" :class="data.dueDiligence.amlFlaggedCount ? 'cp-pill--bad' : 'cp-pill--ok'">{{ data.dueDiligence.amlFlaggedCount ? `${data.dueDiligence.amlFlaggedCount} flagged` : 'All clear' }}</span>
              <small class="cp-muted">{{ data.dueDiligence.amlCount }} screening results</small>
            </div>
            <div class="dd-item">
              <h4>Inquiries</h4>
              <span class="cp-pill" :class="data.dueDiligence.inquiryHighRiskCount ? 'cp-pill--warn' : 'cp-pill--ok'">{{ data.dueDiligence.inquiryHighRiskCount ? `${data.dueDiligence.inquiryHighRiskCount} high risk` : 'No high risk' }}</span>
              <small class="cp-muted">{{ data.dueDiligence.inquiryCount }} inquiry results</small>
            </div>
            <div class="dd-item">
              <h4>Legal review</h4>
              <template v-if="data.dueDiligence.legal">
                <ul class="checks">
                  <li v-for="c in legalChecks" :key="c.label" :class="c.ok ? 'yes' : 'no'"><i :class="c.ok ? 'pi pi-check' : 'pi pi-times'"></i> {{ c.label }}</li>
                </ul>
                <small class="cp-muted">Reviewed {{ date(data.dueDiligence.legal.reviewDate) }}</small>
                <p v-if="data.dueDiligence.legal.legalRecommendation" class="quote">{{ data.dueDiligence.legal.legalRecommendation }}</p>
              </template>
              <small v-else class="cp-muted">No legal review was recorded.</small>
            </div>
            <div class="dd-item">
              <h4>Valuation</h4>
              <template v-if="data.dueDiligence.valuation">
                <dl class="cp-kv">
                  <dt>Market value</dt><dd>{{ formatAmount(data.dueDiligence.valuation.marketValueAmount) }}</dd>
                  <dt>Recommended</dt><dd>{{ amount(data.dueDiligence.valuation.recommendedValueAmount) }}</dd>
                  <dt>Completion</dt><dd>{{ pct(data.dueDiligence.valuation.projectCompletionPct) }}</dd>
                </dl>
                <small class="cp-muted">{{ data.dueDiligence.valuation.valuerName }}, {{ date(data.dueDiligence.valuation.valuationDate) }}</small>
              </template>
              <small v-else class="cp-muted">No valuation was recorded.</small>
            </div>
          </div>
        </section>

        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-users"></i><h3>Investors</h3><span class="cp-spacer"></span><span class="cp-muted">{{ data.investors.length }}</span></div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Investor</th><th class="cp-num">Contracts</th><th class="cp-num">Contract value</th><th class="cp-num">Paid</th><th class="cp-num">Outstanding</th><th class="cp-num">Cheques</th><th>In the deal</th></tr></thead>
              <tbody>
                <tr v-for="i in data.investors" :key="i.investorId" :class="{ out: !i.isIncluded }">
                  <td><strong>{{ i.investorName }}</strong><small class="cp-muted code">{{ i.investorCode }}</small></td>
                  <td class="cp-num">{{ i.selectedContractCount }} / {{ i.contractCount }}</td>
                  <td class="cp-num">{{ formatAmount(i.contractAmount) }}</td>
                  <td class="cp-num">{{ formatAmount(i.paidAmount) }}</td>
                  <td class="cp-num">{{ formatAmount(i.outstandingAmount) }}</td>
                  <td class="cp-num">{{ formatAmount(i.chequeAmount) }}</td>
                  <td><span class="cp-pill" :class="i.isIncluded ? 'cp-pill--ok' : 'cp-pill--bad'">{{ i.isIncluded ? 'Included' : 'Excluded' }}</span></td>
                </tr>
                <tr v-if="data.investors.length === 0"><td colspan="7" class="cp-empty">No contracts were loaded.</td></tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>

      <aside class="stack">
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-briefcase"></i><h3>Board decision</h3></div>
          <div class="cp-card__body">
            <div v-if="data.decision" class="recorded" :class="data.decision.boardDecisionCode === 'ACCEPTED' ? 'ok' : 'bad'">
              <b><i :class="data.decision.boardDecisionCode === 'ACCEPTED' ? 'pi pi-check-circle' : 'pi pi-times-circle'"></i> {{ data.decision.boardDecisionCode === 'ACCEPTED' ? 'Approved' : 'Rejected' }}</b>
              <small>Resolution {{ data.decision.boardDecisionNo }} of {{ date(data.decision.boardDecisionDate) }}</small>
              <small v-if="data.decision.approvedDate">Approved on {{ date(data.decision.approvedDate) }}</small>
            </div>
            <p v-else-if="!editable" class="cp-muted none">No board decision has been recorded.</p>

            <template v-if="editable">
              <div class="form">
                <div class="cp-field">
                  <label for="br-no">Resolution number <span class="req">*</span></label>
                  <input id="br-no" v-model="decisionNo" type="text" maxlength="200" autocomplete="off" />
                </div>
                <div class="cp-field">
                  <label for="br-date">Resolution date <span class="req">*</span></label>
                  <input id="br-date" v-model="decisionDate" type="date" :max="todayInCairo()" />
                </div>
                <div class="cp-field">
                  <label for="br-comment">Comment <span class="req">*</span></label>
                  <textarea id="br-comment" v-model="comment" rows="4" maxlength="4000" placeholder="The board's resolution or conditions"></textarea>
                </div>
              </div>
              <div class="decide">
                <button type="button" class="cp-btn cp-btn--danger" :disabled="busy" @click="decide('REJECT')"><i class="pi pi-times"></i><span>Reject</span></button>
                <button type="button" class="cp-btn cp-btn--primary" :disabled="busy || !data.pricing" @click="decide('ACCEPT')"><i class="pi pi-check"></i><span>Approve purchase</span></button>
              </div>
            </template>
          </div>
        </section>

        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-history"></i><h3>Workflow trail</h3></div>
          <div class="cp-card__body">
            <ol class="trail">
              <li v-for="(h, n) in data.history" :key="n">
                <b>{{ h.toStatusName }}</b>
                <small>{{ h.changedBy }} · {{ formatDateTime(h.changedAt) }}</small>
                <p v-if="h.comments">{{ h.comments }}</p>
              </li>
              <li v-if="data.history.length === 0" class="cp-muted">No workflow history was found.</li>
            </ol>
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
.grid { display: grid; grid-template-columns: minmax(0, 1fr) 340px; gap: 16px; align-items: start; }
.stack { display: grid; grid-template-columns: minmax(0, 1fr); gap: 16px; }
.kpis { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; }
.kpi { display: grid; gap: 2px; padding: 12px 14px; border-radius: 10px; background: var(--cap-bg); }
.kpi small { font-size: 11px; color: var(--cap-muted); }
.kpi b { font-size: 17px; color: var(--cap-ink); font-variant-numeric: tabular-nums; overflow-wrap: anywhere; }
.kpi.main b { font-size: 20px; color: var(--cap-heading); }
.meta { margin: 12px 0 0; font-size: 12px; color: var(--cap-muted); }
.foot { padding-top: 10px; padding-bottom: 14px; }
.two { display: grid; grid-template-columns: minmax(0, 1.2fr) minmax(0, 1fr); gap: 24px; align-items: start; }
.fall { display: grid; grid-template-columns: 1fr auto; gap: 8px 12px; margin: 0; font-size: 13px; }
.fall dt { color: var(--cap-ink-soft); }
.fall dt small { display: block; font-size: 11px; color: var(--cap-muted); }
.fall dd { margin: 0; text-align: end; font-variant-numeric: tabular-nums; }
.fall dd.minus { color: var(--cap-danger-dark); }
.fall .total { padding-top: 8px; border-top: 1px solid var(--cap-border-soft); font-weight: 600; color: var(--cap-heading); }
.terms { font-size: 13px; }
.dd { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
.dd-item { display: grid; gap: 6px; align-content: start; padding: 12px 14px; border: 1px solid var(--cap-border-soft); border-radius: 10px; }
.dd-item h4 { margin: 0; font-size: 13px; }
.dd-item .cp-pill { justify-self: start; }
.checks { display: grid; gap: 4px; margin: 0; padding: 0; list-style: none; font-size: 12px; }
.checks .yes { color: var(--cap-success); }
.checks .no { color: var(--cap-danger); }
.quote { margin: 0; padding: 8px 10px; border-inline-start: 3px solid var(--cap-border); background: var(--cap-bg); font-size: 12px; white-space: pre-wrap; }
.code { display: block; font-size: 11px; }
tr.out td { opacity: .55; }
tr.out td:last-child { opacity: 1; }
.recorded { display: grid; gap: 2px; margin-bottom: 12px; padding: 12px 14px; border-radius: 10px; }
.recorded.ok { background: var(--cap-success-bg); color: var(--cap-success); }
.recorded.bad { background: var(--cap-danger-bg); color: var(--cap-danger-dark); }
.recorded small { font-size: 12px; }
.form { display: grid; gap: 12px; }
.none { margin: 0; font-size: 13px; }
.decide { display: flex; flex-wrap: wrap; justify-content: flex-end; gap: 8px; margin-top: 12px; }
.req { color: var(--cap-danger); }
.trail { display: grid; gap: 12px; margin: 0; padding: 0 0 0 14px; border-inline-start: 2px solid var(--cap-border-soft); list-style: none; font-size: 13px; }
.trail li { display: grid; gap: 2px; }
.trail small { font-size: 11px; color: var(--cap-muted); }
.trail p { margin: 2px 0 0; font-size: 12px; color: var(--cap-ink-soft); white-space: pre-wrap; overflow-wrap: anywhere; }
@media (max-width: 1100px) { .grid { grid-template-columns: minmax(0, 1fr); } }
@media (max-width: 760px) { .kpis, .two, .dd { grid-template-columns: minmax(0, 1fr); } }
</style>
