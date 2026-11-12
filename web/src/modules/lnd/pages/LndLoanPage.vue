<template>
  <!-- One retail loan: its terms (copied from the product version at creation), its schedule and its status history. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'lnd-loans' }">{{ tr('Loans') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ loan?.loanNumber ?? '' }}</b>
        </nav>
        <h1>{{ loan?.loanNumber ?? tr('Loan') }} <span v-if="loan" class="fx-chip" :class="CHIP[loan.statusCode] ?? 'fx-chip--teal'">{{ tr(STATUS[loan.statusCode] ?? loan.statusCode) }}</span></h1>
      </div>
      <div class="fx-actions">
        <button v-if="loan && canEdit && loan.statusCode === 'APPROVED'" type="button" class="fx-btn" @click="cancelling = true"><span class="fa fa-ban" aria-hidden="true"></span>{{ tr('Cancel loan') }}</button>
        <RouterLink :to="{ name: 'lnd-loans' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Loans') }}</RouterLink>
      </div>
    </div>

    <div v-if="!loan" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading loan...') }}</div>
    <template v-else>
      <section class="fx-card ln-facts">
        <div v-for="f in facts" :key="f[0]"><small>{{ tr(f[0]) }}</small><b>{{ f[1] }}</b></div>
      </section>

      <section v-if="payouts" class="fx-card">
        <header class="c3-panel-head lp-head">
          <h3><span class="fa fa-money" aria-hidden="true"></span>{{ tr('Payout') }}</h3>
          <button v-if="payouts.canRequest && loan.statusCode === 'APPROVED' && !waiting" type="button" class="fx-btn fx-btn--teal fx-btn--sm" @click="openRequest">
            <span class="fa fa-paper-plane" aria-hidden="true"></span>{{ tr('Ask for payout') }}
          </button>
        </header>
        <p v-if="loan.statusCode === 'APPROVED' && payouts.preview.problem" class="c3-note lp-bad"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>{{ payouts.preview.problem }}</p>
        <p v-else-if="loan.statusCode === 'APPROVED'" class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
          {{ tr('Paid out today:') }} {{ money(payouts.preview.principalAmount) }} − {{ tr('charges') }} {{ money(payouts.preview.chargesAmount) }} = <b>{{ money(payouts.preview.netPayoutAmount) }}</b>
          <template v-for="c in payouts.preview.charges" :key="c.chargeCode"> · {{ c.chargeName }} {{ money(c.amount) }}</template>
        </p>
        <div v-if="payouts.disbursements.length" class="fx-table-wrap">
          <table class="fx-table">
            <thead><tr><th>{{ tr('Asked') }}</th><th>{{ tr('Paid from') }}</th><th>{{ tr('Paid to') }}</th><th class="ln-num">{{ tr('Net paid') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('GL') }}</th><th></th></tr></thead>
            <tbody>
              <tr v-for="d in payouts.disbursements" :key="d.disbursementId">
                <td>{{ d.requestedBy }}<br><small class="c3-muted">{{ formatDateTime(d.requestedAt) }}</small></td>
                <td>{{ d.paidFromAccount ?? d.paidFromAccountId }}</td>
                <td>{{ d.payeeName }}<br><small class="c3-muted">{{ tr(METHODS[d.payoutMethodCode] ?? d.payoutMethodCode) }}<template v-if="d.payeeAccount"> · {{ d.payeeAccount }}</template></small></td>
                <td class="ln-num">{{ money(d.netPayoutAmount) }}<br><small class="c3-muted">{{ tr('charges') }} {{ money(d.chargesAmount) }}</small></td>
                <td><span class="fx-chip" :class="PCHIP[d.statusCode]">{{ tr(PSTATUS[d.statusCode]) }}</span><template v-if="d.decidedBy"><br><small class="c3-muted">{{ d.decidedBy }} · {{ formatDateTime(d.decidedAt) }}</small></template></td>
                <td>{{ d.glTranSetId ?? '' }}</td>
                <td class="lp-actions">
                  <template v-if="d.statusCode === 'REQUESTED'">
                    <button v-if="d.canPay" type="button" class="fx-btn fx-btn--sm fx-btn--teal" :disabled="busy" @click="openDecision(d.disbursementId, 'pay')"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Pay') }}</button>
                    <button v-if="payouts.canPay && !mine(d)" type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="openDecision(d.disbursementId, 'reject')"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Reject') }}</button>
                    <button v-if="mine(d)" type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="withdraw(d.disbursementId)"><span class="fa fa-undo" aria-hidden="true"></span>{{ tr('Withdraw') }}</button>
                    <small v-if="d.payBlockedReason" class="c3-muted">{{ tr(d.payBlockedReason) }}</small>
                  </template>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section v-if="receipts && loan.statusCode !== 'APPROVED' && loan.statusCode !== 'CANCELLED'" class="fx-card">
        <header class="c3-panel-head lp-head">
          <h3><span class="fa fa-download" aria-hidden="true"></span>{{ tr('Payments') }}</h3>
          <button v-if="receipts.canReceive" type="button" class="fx-btn fx-btn--teal fx-btn--sm" @click="openReceive"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Receive payment') }}</button>
        </header>
        <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
          {{ tr('Due today:') }} <b>{{ money(receipts.overdueToday) }}</b> · {{ tr('Customer credit balance:') }} <b>{{ money(receipts.creditBalance) }}</b>
        </p>
        <div v-if="receipts.receipts.length" class="fx-table-wrap">
          <table class="fx-table">
            <thead><tr><th>{{ tr('Receipt') }}</th><th>{{ tr('Value date') }}</th><th>{{ tr('From') }}</th><th class="ln-num">{{ tr('Received') }}</th><th class="ln-num">{{ tr('Applied') }}</th><th>{{ tr('GL') }}</th><th>{{ tr('By') }}</th></tr></thead>
            <tbody>
              <tr v-for="r in receipts.receipts" :key="r.repaymentTxnId">
                <td><b>{{ r.repaymentNumber }}</b></td><td>{{ formatDate(r.valueDate) }}</td><td>{{ r.sources }}</td>
                <td class="ln-num">{{ money(r.receiptAmount) }}</td><td class="ln-num">{{ money(r.allocatedAmount) }}</td><td>{{ r.glTranSetId ?? '' }}</td><td>{{ r.createdBy }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="fx-card">
        <header class="c3-panel-head"><h3><span class="fa fa-calendar" aria-hidden="true"></span>{{ tr('Schedule') }}</h3></header>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead>
              <tr>
                <th>#</th><th>{{ tr('Due date') }}</th><th class="ln-num">{{ tr('Principal') }}</th><th class="ln-num">{{ tr('Interest') }}</th>
                <th class="ln-num">{{ tr('Charges') }}</th><th class="ln-num">{{ tr('Insurance') }}</th><th class="ln-num">{{ tr('Late fee') }}</th>
                <th class="ln-num">{{ tr('Due') }}</th><th class="ln-num">{{ tr('Paid') }}</th><th class="ln-num">{{ tr('Balance after') }}</th><th>{{ tr('Status') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="i in loan.installments" :key="i.installmentNo">
                <td>{{ i.installmentNo }}</td><td>{{ formatDate(i.dueDate) }}</td>
                <td class="ln-num">{{ money(i.principalDue) }}</td><td class="ln-num">{{ money(i.interestDue) }}</td><td class="ln-num">{{ money(i.chargesDue) }}</td>
                <td class="ln-num">{{ money(i.insuranceDue) }}</td><td class="ln-num">{{ money(i.lateFeeDue) }}</td><td class="ln-num"><b>{{ money(i.totalDue) }}</b></td>
                <td class="ln-num">{{ money(i.totalPaid) }}</td><td class="ln-num">{{ money(i.closingPrincipal) }}</td>
                <td><span class="fx-chip" :class="i.statusCode === 'PAID' ? 'fx-chip--ok' : i.statusCode === 'PART_PAID' ? 'fx-chip--warn' : 'fx-chip--teal'">{{ tr(INST[i.statusCode] ?? i.statusCode) }}</span></td>
              </tr>
            </tbody>
            <tfoot>
              <tr><td colspan="2"><b>{{ tr('Total') }}</b></td><td class="ln-num"><b>{{ money(sum('principalDue')) }}</b></td><td class="ln-num"><b>{{ money(sum('interestDue')) }}</b></td>
                <td class="ln-num">{{ money(sum('chargesDue')) }}</td><td class="ln-num">{{ money(sum('insuranceDue')) }}</td><td class="ln-num">{{ money(sum('lateFeeDue')) }}</td>
                <td class="ln-num"><b>{{ money(sum('totalDue')) }}</b></td><td class="ln-num">{{ money(sum('totalPaid')) }}</td><td></td><td></td></tr>
            </tfoot>
          </table>
        </div>
      </section>

      <section v-if="loan.events.length" class="fx-card">
        <header class="c3-panel-head"><h3><span class="fa fa-book" aria-hidden="true"></span>{{ tr('Loan events') }}</h3></header>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead><tr><th>{{ tr('Date') }}</th><th>{{ tr('Event') }}</th><th class="ln-num">{{ tr('Amount') }}</th><th>{{ tr('GL') }}</th><th>{{ tr('Notes') }}</th></tr></thead>
            <tbody>
              <tr v-for="(e, n) in loan.events" :key="n">
                <td>{{ formatDate(e.eventDate) }}</td><td>{{ tr(EVENTS[e.eventCode] ?? e.eventCode) }}</td><td class="ln-num">{{ money(e.eventAmount) }}</td>
                <td>{{ e.glTranSetId ?? '' }}</td><td>{{ e.notes ?? '' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section class="fx-card">
        <header class="c3-panel-head"><h3><span class="fa fa-history" aria-hidden="true"></span>{{ tr('Status history') }}</h3></header>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead><tr><th>{{ tr('When') }}</th><th>{{ tr('By') }}</th><th>{{ tr('From') }}</th><th>{{ tr('To') }}</th><th>{{ tr('Reason') }}</th></tr></thead>
            <tbody>
              <tr v-for="(h, n) in loan.history" :key="n">
                <td>{{ formatDateTime(h.at) }}</td><td>{{ h.by }}</td><td>{{ h.fromStatusCode ? tr(STATUS[h.fromStatusCode] ?? h.fromStatusCode) : '' }}</td>
                <td>{{ tr(STATUS[h.toStatusCode] ?? h.toStatusCode) }}</td><td>{{ h.reason ?? '' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>

    <Crm360Dialog v-model:visible="requesting" :title="tr('Ask for Payout')" :subtitle="loan?.loanNumber ?? ''" icon="fa-paper-plane" :busy="busy" :width="640">
      <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Another user pays it. Paying posts the GL and makes the loan active.') }}</p>
      <div class="c3-grid c3-grid--2">
        <label class="fc-field c3-wide" :class="pbad('paidFromAccountId')"><span>{{ tr('Paid from') }}<b> *</b></span>
          <select v-model="payout.paidFromAccountId" class="fx-select">
            <option v-for="a in payouts?.accounts ?? []" :key="a.accountId" :value="a.accountId" :disabled="!a.glAccountCode">{{ a.bankName }} · {{ a.accountRef }} · {{ a.accountNo }}{{ a.glAccountCode ? '' : ' (' + tr('no GL account') + ')' }}</option>
          </select>
        </label>
        <label class="fc-field" :class="pbad('payoutMethodCode')"><span>{{ tr('Paid by') }}<b> *</b></span>
          <select v-model="payout.payoutMethodCode" class="fx-select"><option v-for="(l, k) in METHODS" :key="k" :value="k">{{ tr(l) }}</option></select>
        </label>
        <label class="fc-field" :class="pbad('payeeName')"><span>{{ tr('Paid to') }}<b> *</b></span><input v-model="payout.payeeName" class="fx-search" maxlength="200"></label>
        <label class="fc-field c3-wide" :class="pbad('payeeAccount')"><span>{{ tr('Account, IBAN or wallet') }}</span><input v-model="payout.payeeAccount" class="fx-search" maxlength="100"></label>
      </div>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="requesting = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="requestPayout"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-paper-plane'" aria-hidden="true"></span>{{ tr('Ask for payout') }}</button>
      </template>
    </Crm360Dialog>

    <Crm360Dialog v-model:visible="deciding" :title="tr(decision === 'pay' ? 'Pay Loan' : 'Reject Payout')" :subtitle="loan?.loanNumber ?? ''" :icon="decision === 'pay' ? 'fa-check-circle' : 'fa-times-circle'" :busy="busy" :width="520">
      <p v-if="decision === 'pay'" class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Paying posts the GL today and makes the loan active. A payout on another day than planned rebuilds the schedule from today.') }}</p>
      <label class="fc-field" :class="pbad('notes')"><span>{{ tr(decision === 'pay' ? 'Notes' : 'Reason') }}<b v-if="decision === 'reject'"> *</b></span><input v-model="notes" class="fx-search" maxlength="1000"></label>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="deciding = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
        <button type="button" class="fx-btn" :class="decision === 'pay' ? 'fx-btn--teal' : 'fx-btn--primary'" :disabled="busy" @click="decide">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : decision === 'pay' ? 'fa-check' : 'fa-times'" aria-hidden="true"></span>{{ tr(decision === 'pay' ? 'Pay' : 'Reject') }}
        </button>
      </template>
    </Crm360Dialog>

    <Crm360Dialog v-model:visible="receiving" :title="tr('Receive Payment')" :subtitle="loan?.loanNumber ?? ''" icon="fa-download" :busy="busy" :width="900">
      <div class="c3-grid c3-grid--2">
        <label class="fc-field" :class="rbad('valueDate')"><span>{{ tr('Value date') }}<b> *</b></span><input v-model="rform.valueDate" type="date" class="fx-search" :max="receipts?.today"></label>
        <label class="fc-field"><span>{{ tr('Notes') }}</span><input v-model="rform.notes" class="fx-search" maxlength="1000"></label>
      </div>
      <div class="lp-sources" :class="rbad('sources')">
        <div v-for="(s, n) in rform.sources" :key="n" class="lp-source">
          <select v-model="s.sourceType" class="fx-select lp-type">
            <option value="BANK_DEPOSIT">{{ tr('Bank deposit') }}</option><option value="CHEQUE">{{ tr('Collected cheque') }}</option><option value="CREDIT_BALANCE">{{ tr('Credit balance') }}</option>
          </select>
          <template v-if="s.sourceType === 'BANK_DEPOSIT'">
            <select v-model="s.depositAccountId" class="fx-select"><option v-for="a in receipts?.depositAccounts ?? []" :key="a.accountId" :value="a.accountId" :disabled="!a.hasGlAccount">{{ a.label }}</option></select>
            <input v-model="s.reference" class="fx-search" :placeholder="tr('Bank reference')" maxlength="100">
            <input v-model.number="s.amount" type="number" min="0" step="any" class="fx-search lp-amount" :placeholder="tr('Amount')">
          </template>
          <select v-else-if="s.sourceType === 'CHEQUE'" v-model="s.chequeId" class="fx-select">
            <option v-for="c in receipts?.cheques ?? []" :key="c.chequeId" :value="c.chequeId">{{ c.chequeRef }} · {{ c.chequeNo }} · {{ formatDate(c.dueDate) }} · {{ money(c.chequeAmount) }}</option>
          </select>
          <input v-else v-model.number="s.amount" type="number" min="0" step="any" class="fx-search lp-amount" :placeholder="tr('Amount')">
          <button type="button" class="fx-btn fx-btn--sm" :aria-label="tr('Remove')" :disabled="rform.sources.length === 1" @click="rform.sources.splice(n, 1)"><span class="fa fa-trash" aria-hidden="true"></span></button>
        </div>
        <button type="button" class="fx-btn fx-btn--sm" @click="addSource"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Add source') }}</button>
      </div>
      <div v-if="rpreview" class="lp-preview">
        <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
          {{ tr('Received') }} <b>{{ money(rpreview.receiptAmount) }}</b> · {{ tr('applied') }} <b>{{ money(rpreview.allocatedAmount) }}</b> · {{ tr('kept as credit') }} <b>{{ money(rpreview.creditKeptAmount) }}</b>
        </p>
        <table class="fx-table">
          <thead><tr><th>#</th><th>{{ tr('Part') }}</th><th class="ln-num">{{ tr('Amount') }}</th></tr></thead>
          <tbody><tr v-for="(l, n) in rpreview.lines" :key="n"><td>{{ l.installmentNo }}</td><td>{{ tr(PARTS[l.partCode] ?? l.partCode) }}</td><td class="ln-num">{{ money(l.amount) }}</td></tr></tbody>
        </table>
      </div>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="receiving = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
        <button type="button" class="fx-btn" :disabled="busy" @click="previewReceipt"><span class="fa fa-table" aria-hidden="true"></span>{{ tr('Show allocation') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !rpreview" @click="receive"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr('Receive') }}</button>
      </template>
    </Crm360Dialog>

    <Crm360Dialog v-model:visible="cancelling" :title="tr('Cancel Loan')" :subtitle="loan?.loanNumber ?? ''" icon="fa-ban" :busy="busy" :width="520">
      <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('A loan can be cancelled until it is paid out. Its schedule is kept for the record.') }}</p>
      <label class="fc-field" :class="{ 'c3-bad': !!errors.reason }"><span>{{ tr('Reason') }}<b> *</b></span><input v-model="reason" class="fx-search" maxlength="1000"></label>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="cancelling = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="cancel"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-ban'" aria-hidden="true"></span>{{ tr('Cancel loan') }}</button>
      </template>
    </Crm360Dialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { ApiError } from '@/shared/api/http'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { getUser } from '@/shared/auth/session'
import { loansApi, type Disbursement, type DisbursementPage, type Installment, type Loan, type PayoutRequest, type ReceiptPreview, type ReceiptSource, type ReceiptsPage } from '../api/loans.api'

const props = defineProps<{ loanId: number }>()

const STATUS: Record<string, string> = { APPROVED: 'Approved', ACTIVE: 'Active', CLOSED: 'Closed', CANCELLED: 'Cancelled' }
const CHIP: Record<string, string> = { APPROVED: 'fx-chip--warn', ACTIVE: 'fx-chip--ok', CLOSED: 'fx-chip--teal', CANCELLED: 'fx-chip--bad' }
const INST: Record<string, string> = { DUE: 'Due', PART_PAID: 'Part paid', PAID: 'Paid' }
const FREQ: Record<string, string> = { WEEKLY: 'Weekly', FORTNIGHTLY: 'Every two weeks', MONTHLY: 'Monthly' }
const egp = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const money = (n: number) => egp.format(n)

const canEdit = hasPermission('LND.LOAN.EDIT')
const loan = ref<Loan | null>(null)

const facts = computed<[string, string][]>(() => {
  const l = loan.value
  if (!l) return []
  const fee = l.lateFeeTypeCode === 'NONE' ? tr('None')
    : `${l.lateFeeTypeCode === 'FLAT' ? `${tr('EGP')} ${money(l.lateFeeFlatAmount ?? 0)}` : `${l.lateFeeRatePct}%${l.lateFeeTypeCode === 'PCT_DAILY' ? ' ' + tr('a day') : ''}`} · ${l.lateFeeGraceDays} ${tr('grace days')}`
  return [
    ['Customer', l.clientName],
    ['Product', `${l.productName} · ${tr('Version')} ${l.versionNo}`],
    ['Licence', tr(l.licenceCode === 'MF' ? 'Microfinance' : 'Consumer Finance')],
    ['Amount', `${tr('EGP')} ${money(l.principalAmount)}`],
    ['Term', `${l.termMonths} ${tr('months')} · ${l.installmentCount} × ${money(l.installmentAmount)}`],
    ['Repayment', tr(FREQ[l.repaymentFrequencyCode] ?? l.repaymentFrequencyCode)],
    ['Yearly rate', `${l.annualRatePct}% · ${tr(l.interestMethodCode === 'FLAT' ? 'Flat' : 'Declining balance')}`],
    ['Total interest', money(l.totalInterestAmount)],
    ['Disbursement', formatDate(l.disbursementDate)],
    ['First instalment', formatDate(l.firstDueDate)],
    ['Maturity', formatDate(l.maturityDate)],
    ['Late fee', fee],
    ...(l.lastEodDate ? [
      ['Days late', `${l.daysLate}${l.isNonAccrual ? ' · ' + tr('In non-accrual') : ''}${l.isWriteOffProposed ? ' · ' + tr('Proposed for write-off') : ''}`],
      ['Interest suspense', `${money(l.interestSuspenseAmount)} · ${tr('Late fee suspense')} ${money(l.lateFeeSuspenseAmount)}`],
      ['Last end of day', formatDate(l.lastEodDate)]
    ] as [string, string][] : [])
  ]
})

// ---- payout
const METHODS: Record<string, string> = { BANK_TRANSFER: 'Bank transfer', CASH: 'Cash', MERCHANT: 'Paid to merchant', WALLET: 'E-wallet' }
const PSTATUS = { REQUESTED: 'Waiting', PAID: 'Paid', REJECTED: 'Rejected', WITHDRAWN: 'Withdrawn' } as const
const PCHIP = { REQUESTED: 'fx-chip--warn', PAID: 'fx-chip--ok', REJECTED: 'fx-chip--bad', WITHDRAWN: 'fx-chip--teal' } as const
const EVENTS: Record<string, string> = {
  DISBURSED: 'Paid out', RECEIPT: 'Payment received', ACCRUED: 'Interest accrued', LATE_FEE: 'Late fee', NON_ACCRUAL: 'Non-accrual',
  CURED: 'Cured', SUSPENSE_RELEASED: 'Suspense released', WRITE_OFF_PROPOSED: 'Proposed for write-off'
}
const me = (getUser()?.userName ?? '').toLowerCase()
const payouts = ref<DisbursementPage | null>(null)
const waiting = computed(() => payouts.value?.disbursements.some((d) => d.statusCode === 'REQUESTED') ?? false)
const mine = (d: Disbursement) => !!me && d.requestedBy.toLowerCase() === me
const requesting = ref(false)
const deciding = ref(false)
const decision = ref<'pay' | 'reject'>('pay')
const decidingId = ref(0)
const notes = ref('')
const payout = reactive<PayoutRequest>({ paidFromAccountId: null, payoutMethodCode: 'BANK_TRANSFER', payeeName: '', payeeAccount: null })
const perrors = ref<Record<string, string[]>>({})
const pbad = (f: string) => ({ 'c3-bad': !!perrors.value[f] })

function openRequest() {
  Object.assign(payout, { paidFromAccountId: payouts.value?.accounts.find((a) => a.glAccountCode)?.accountId ?? null, payoutMethodCode: 'BANK_TRANSFER',
    payeeName: loan.value?.clientName ?? '', payeeAccount: null })
  perrors.value = {}
  requesting.value = true
}
function openDecision(id: number, d: 'pay' | 'reject') {
  decidingId.value = id
  decision.value = d
  notes.value = ''
  perrors.value = {}
  deciding.value = true
}
async function payoutCall(action: () => Promise<DisbursementPage>, done: string, close: () => void) {
  busy.value = true
  perrors.value = {}
  try {
    payouts.value = await action()
    close()
    showSuccess(tr(done))
    loan.value = await loansApi.get(props.loanId)
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) {
      perrors.value = err.problem.errors
      showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    } else showError(err instanceof Error ? err.message : String(err))
  } finally {
    busy.value = false
  }
}
const requestPayout = () => payoutCall(() => loansApi.requestPayout(props.loanId, { ...payout, payeeAccount: payout.payeeAccount || null }),
  'Payout sent. Another user must pay it.', () => (requesting.value = false))
const decide = () => decision.value === 'pay'
  ? payoutCall(() => loansApi.pay(props.loanId, decidingId.value, notes.value || null), 'Loan paid out. It is now active.', () => (deciding.value = false))
  : payoutCall(() => loansApi.rejectPayout(props.loanId, decidingId.value, notes.value), 'Payout rejected.', () => (deciding.value = false))
const withdraw = (id: number) => payoutCall(() => loansApi.withdrawPayout(props.loanId, id), 'Payout withdrawn.', () => {})

// ---- receipts
const PARTS: Record<string, string> = { LATE_FEE: 'Late fee', CHARGES: 'Charges', INSURANCE: 'Insurance', INTEREST: 'Interest', PRINCIPAL: 'Principal' }
const receipts = ref<ReceiptsPage | null>(null)
const receiving = ref(false)
const rpreview = ref<ReceiptPreview | null>(null)
const rerrors = ref<Record<string, string[]>>({})
const rbad = (f: string) => ({ 'c3-bad': !!rerrors.value[f] })
const blankSource = (): ReceiptSource => ({ sourceType: 'BANK_DEPOSIT', depositAccountId: receipts.value?.depositAccounts.find((a) => a.hasGlAccount)?.accountId ?? null, chequeId: null, amount: null, reference: null })
const rform = reactive<{ valueDate: string; notes: string; sources: ReceiptSource[] }>({ valueDate: '', notes: '', sources: [] })
watch(() => JSON.stringify([rform.valueDate, rform.sources]), () => (rpreview.value = null))

function openReceive() {
  rform.valueDate = receipts.value?.today ?? ''
  rform.notes = ''
  rform.sources = [blankSource()]
  rpreview.value = null
  rerrors.value = {}
  receiving.value = true
}
const addSource = () => rform.sources.push(blankSource())
/** The money of the sources: a cheque counts its amount, the others what is entered. */
const sourcesTotal = () => rform.sources.reduce((a, s) => a + (s.sourceType === 'CHEQUE'
  ? receipts.value?.cheques.find((c) => c.chequeId === s.chequeId)?.chequeAmount ?? 0
  : Number(s.amount ?? 0)), 0)

async function previewReceipt() {
  try {
    rpreview.value = await loansApi.receiptPreview(props.loanId, rform.valueDate, sourcesTotal())
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  }
}
async function receive() {
  busy.value = true
  rerrors.value = {}
  try {
    receipts.value = await loansApi.receive(props.loanId, { valueDate: rform.valueDate || null, sources: rform.sources, notes: rform.notes || null })
    receiving.value = false
    showSuccess(tr('Payment received.'))
    loan.value = await loansApi.get(props.loanId)
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) {
      rerrors.value = err.problem.errors
      showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    } else showError(err instanceof Error ? err.message : String(err))
  } finally {
    busy.value = false
  }
}

const sum = (k: keyof Installment) => (loan.value?.installments ?? []).reduce((a, i) => a + Number(i[k]), 0)

const cancelling = ref(false)
const busy = ref(false)
const reason = ref('')
const errors = ref<Record<string, string[]>>({})

async function cancel() {
  if (!loan.value) return
  busy.value = true
  errors.value = {}
  try {
    loan.value = await loansApi.cancel(loan.value.loanId, reason.value, loan.value.rowVersion)
    cancelling.value = false
    showSuccess(tr('Loan cancelled.'))
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) {
      errors.value = err.problem.errors
      showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    } else showError(err instanceof Error ? err.message : String(err))
  } finally {
    busy.value = false
  }
}

onMounted(async () => {
  try {
    loan.value = await loansApi.get(props.loanId)
    payouts.value = await loansApi.payouts(props.loanId)
    receipts.value = await loansApi.receipts(props.loanId)
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  }
})
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
.ln-facts { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px; padding: 12px; }
.ln-facts > div { display: flex; flex-direction: column; gap: 2px; padding: 8px 10px; border-radius: var(--fin-radius-sm); background: var(--fin-teal-50); }
.ln-facts small { color: var(--fin-muted); }
.ln-num { text-align: end; white-space: nowrap; }
h1 .fx-chip { vertical-align: middle; font-size: 12px; }
.lp-head { display: flex; justify-content: space-between; align-items: center; }
.lp-bad { background: var(--fin-bad-50); color: var(--fin-ink); }
.lp-actions { display: flex; gap: 6px; flex-wrap: wrap; align-items: center; }
.lp-sources { display: flex; flex-direction: column; gap: 8px; margin-top: 12px; }
.lp-source { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
.lp-source > .fx-select, .lp-source > .fx-search { flex: 1; min-width: 160px; }
.lp-source > .lp-type { flex: 0 0 180px; }
.lp-source > .lp-amount { flex: 0 0 140px; }
.lp-preview { margin-top: 12px; }
.c3-grid--2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
@media (max-width: 900px) { .ln-facts { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
