<template>
  <!-- CF_FINANCE (Ahmed, 26 Nov 2026): the step a consumer finance from a supplier is waiting at; the task holder works on it here.
       Credit approval decides each charge (upfront or added to the schedule); Charges and claim executes that decision (claim, payment);
       Schedule and contract builds the schedule. -->
  <section v-if="step" class="fx-card cfs">
    <header class="cfs-head">
      <span class="cfs-ic" aria-hidden="true"><span class="fa fa-random"></span></span>
      <h3>{{ tr(step.stepName) }}</h3>
      <p v-if="step.isMine">— {{ tr(HINT[step.stepCode] ?? 'Check the finance, then accept or reject the step.') }}</p>
      <p v-else>— {{ tr('Waiting for') }} {{ tr(ROLE[step.stepCode] ?? 'the step holder') }}. {{ tr('Open it from the inbox to work on it.') }}</p>
    </header>

    <!-- the deal at a glance, for the step holder -->
    <div v-if="step.isMine && loan.goods" class="cfs-deal">
      <span>{{ tr('Customer') }} <b>{{ loan.clientName }}</b></span>
      <span>{{ tr('Supplier') }} <b>{{ loan.goods.merchantName }}</b></span>
      <span>{{ tr('Item') }} <b>{{ loan.goods.description }}<template v-if="loan.goods.brandModel"> · {{ loan.goods.brandModel }}</template></b></span>
      <span>{{ tr('Price') }} <b>{{ money(loan.goods.priceAmount) }}</b></span>
      <template v-if="!deciding">
        <span>{{ tr('Down payment') }} <b>{{ money(loan.goods.downPaymentAmount) }}</b> ({{ tr(loan.goods.downPaymentCollectorCode === 'COMPANY' ? 'us' : 'supplier') }})</span>
        <span>{{ tr('Financed') }} <b>{{ money(loan.goods.priceAmount - loan.goods.downPaymentAmount) }}</b></span>
        <span>{{ tr('Term') }} <b>{{ loan.termMonths }} {{ tr('months') }}</b></span>
      </template>
      <span>{{ tr('Commission') }} <b>{{ loan.goods.supplierCommissionPct ?? 0 }}%</b></span>
    </div>

    <!-- Credit approval (Ahmed, 27 Nov 2026): every financial value is this finance's own copy and Credit can change any of them;
         what was entered shows under each, a changed one is highlighted -->
    <div v-if="deciding && t && o" class="cfs-terms">
      <h4 class="cfs-sub">{{ tr('Amount and term') }}</h4>
      <div class="cfs-grid">
        <label class="fc-field"><span>{{ tr('Down payment') }} ({{ tr(loan.goods?.downPaymentCollectorCode === 'COMPANY' ? 'us' : 'supplier') }})</span>
          <input v-model.number="t.downPaymentAmount" type="number" min="0" step="any" class="fx-search" :class="{ chg: ch('downPaymentAmount') }" @input="downChanged">
          <small>{{ tr('entered') }} {{ money(o.downPaymentAmount) }}<template v-if="o.minDownPaymentAmount"> · {{ tr('at least') }} {{ money(o.minDownPaymentAmount) }}</template></small></label>
        <label class="fc-field"><span>{{ tr('Financed') }}</span><input :value="money(o.priceAmount - (Number(t.downPaymentAmount) || 0))" class="fx-search" disabled>
          <small>{{ tr('price − down payment') }}</small></label>
        <label class="fc-field"><span>{{ tr('Term (months)') }}</span><input v-model.number="t.termMonths" type="number" min="1" max="120" class="fx-search" :class="{ chg: ch('termMonths') }">
          <small>{{ tr('entered') }} {{ o.termMonths }}</small></label>
        <label class="fc-field"><span>{{ tr('Repayment') }}</span>
          <select v-model="t.repaymentFrequencyCode" class="fx-select" :class="{ chg: ch('repaymentFrequencyCode') }"><option v-for="x in o.options.frequencies" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select>
          <small>{{ tr('entered') }} {{ tr(lbl(o.options.frequencies, o.repaymentFrequencyCode)) }}</small></label>
      </div>
      <h4 class="cfs-sub">{{ tr('Interest') }}</h4>
      <div class="cfs-grid">
        <label class="fc-field"><span>{{ tr('Yearly rate') }} (%)</span><input v-model.number="t.annualRatePct" type="number" min="0" max="100" step="any" class="fx-search" :class="{ chg: ch('annualRatePct') }">
          <small>{{ tr('product') }} {{ step.productRatePct }}%<template v-if="marginNow"> · {{ tr('margin') }} {{ marginNow > 0 ? '+' : '' }}{{ marginNow }}</template></small></label>
        <label class="fc-field"><span>{{ tr('Interest method') }}</span>
          <select v-model="t.interestMethodCode" class="fx-select" :class="{ chg: ch('interestMethodCode') }"><option v-for="x in o.options.interestMethods" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select>
          <small>{{ tr('entered') }} {{ tr(lbl(o.options.interestMethods, o.interestMethodCode)) }}</small></label>
        <label class="fc-field"><span>{{ tr('Day count') }}</span>
          <select v-model="t.dayCountCode" class="fx-select" :class="{ chg: ch('dayCountCode') }"><option v-for="x in o.options.dayCounts" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select>
          <small>{{ tr('entered') }} {{ tr(lbl(o.options.dayCounts, o.dayCountCode)) }}</small></label>
      </div>
      <h4 class="cfs-sub">{{ tr('Late fee and other') }}</h4>
      <div class="cfs-grid">
        <label class="fc-field"><span>{{ tr('Late fee type') }}</span>
          <select v-model="t.lateFeeTypeCode" class="fx-select" :class="{ chg: ch('lateFeeTypeCode') }"><option v-for="x in o.options.lateFeeTypes" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select>
          <small>{{ tr('entered') }} {{ tr(lbl(o.options.lateFeeTypes, o.lateFeeTypeCode)) }}</small></label>
        <label v-if="t.lateFeeTypeCode === 'FLAT'" class="fc-field"><span>{{ tr('Fee per late instalment') }}</span>
          <input v-model.number="t.lateFeeFlatAmount" type="number" min="0" step="any" class="fx-search" :class="{ chg: ch('lateFeeFlatAmount') }"><small>{{ tr('entered') }} {{ o.lateFeeFlatAmount ?? '—' }}</small></label>
        <template v-if="t.lateFeeTypeCode === 'PCT_ONCE' || t.lateFeeTypeCode === 'PCT_DAILY'">
          <label class="fc-field"><span>{{ tr('Late fee rate') }} (%)</span>
            <input v-model.number="t.lateFeeRatePct" type="number" min="0" max="100" step="any" class="fx-search" :class="{ chg: ch('lateFeeRatePct') }"><small>{{ tr('entered') }} {{ o.lateFeeRatePct ?? '—' }}</small></label>
          <label class="fc-field"><span>{{ tr('Calculated on') }}</span>
            <select v-model="t.lateFeeBaseCode" class="fx-select" :class="{ chg: ch('lateFeeBaseCode') }"><option v-for="x in o.options.lateFeeBases" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select>
            <small>{{ tr('entered') }} {{ tr(lbl(o.options.lateFeeBases, o.lateFeeBaseCode)) }}</small></label>
        </template>
        <label v-if="t.lateFeeTypeCode !== 'NONE'" class="fc-field"><span>{{ tr('Grace days') }}</span>
          <input v-model.number="t.lateFeeGraceDays" type="number" min="0" max="90" class="fx-search" :class="{ chg: ch('lateFeeGraceDays') }"><small>{{ tr('entered') }} {{ o.lateFeeTypeCode === 'NONE' ? '—' : o.lateFeeGraceDays }}</small></label>
        <label class="fc-field"><span>{{ tr('Insurance required') }}</span>
          <select v-model="t.isInsuranceRequired" class="fx-select" :class="{ chg: ch('isInsuranceRequired') }"><option :value="false">{{ tr('No') }}</option><option :value="true">{{ tr('Yes') }}</option></select>
          <small>{{ tr('entered') }} {{ tr(o.isInsuranceRequired ? 'Yes' : 'No') }}</small></label>
        <label class="fc-field"><span>{{ tr('Paying early') }}</span>
          <select v-model="t.isPrepaymentAllowed" class="fx-select" :class="{ chg: ch('isPrepaymentAllowed') }"><option :value="true">{{ tr('Allowed') }}</option><option :value="false">{{ tr('Not allowed') }}</option></select>
          <small>{{ tr('entered') }} {{ tr(o.isPrepaymentAllowed ? 'Allowed' : 'Not allowed') }}</small></label>
        <template v-if="t.isPrepaymentAllowed">
          <label class="fc-field"><span>{{ tr('After a part prepayment') }}</span>
            <select v-model="t.prepaymentRecalcCode" class="fx-select" :class="{ chg: ch('prepaymentRecalcCode') }"><option v-for="x in o.options.prepaymentRecalcs" :key="x.value" :value="x.value">{{ tr(x.label) }}</option></select>
            <small>{{ tr('entered') }} {{ tr(lbl(o.options.prepaymentRecalcs, o.prepaymentRecalcCode)) }}</small></label>
          <label class="fc-field"><span>{{ tr('Part prepayment fee') }} (%)</span>
            <input v-model.number="t.prepaymentFeePct" type="number" min="0" max="100" step="any" class="fx-search" :class="{ chg: ch('prepaymentFeePct') }"><small>{{ tr('entered') }} {{ o.prepaymentFeePct }}%</small></label>
          <label class="fc-field"><span>{{ tr('Early settlement fee') }} (%)</span>
            <input v-model.number="t.earlySettlementFeePct" type="number" min="0" max="100" step="any" class="fx-search" :class="{ chg: ch('earlySettlementFeePct') }"><small>{{ tr('entered') }} {{ o.earlySettlementFeePct }}%</small></label>
        </template>
      </div>
      <div class="cfs-order">
        <span class="c3-muted">{{ tr('Payment order') }}:</span>
        <span v-for="(part, i) in t.paymentOrder" :key="part" class="cfs-part" :class="{ chg: ch('paymentOrder') }">
          <button type="button" :disabled="i === 0" :aria-label="tr('Move up')" @click="moveOrder(i)"><span class="fa fa-angle-left" aria-hidden="true"></span></button>{{ tr(lbl(o.options.paymentParts, part)) }}
        </span>
      </div>
    </div>

    <!-- the charges: Credit decides (the rule's default is marked), later steps show the decision -->
    <div v-if="charges && (step.isMine || charges.decided)" class="cfs-box">
      <h4>{{ tr('Charges') }} <small v-if="charges.decided" class="c3-muted">· {{ tr('decided by Credit') }}</small></h4>
      <p v-if="charges.problem" class="cfs-bad">{{ charges.problem }}</p>
      <table v-else-if="charges.charges.length" class="fx-table">
        <thead><tr><th>{{ tr('Charge') }}</th><th>{{ tr('Basis') }}</th><th class="cfs-c">{{ tr('Amount') }}</th><th class="cfs-c">{{ tr('How paid') }}</th></tr></thead>
        <tbody>
          <tr v-for="c in charges.charges" :key="c.chargeCode">
            <td>{{ tr(c.chargeName) }}</td><td>{{ tr(c.basisText) || '—' }}</td>
            <td v-if="deciding" class="cfs-c">
              <input v-model.number="amounts[c.chargeCode]" type="number" min="0" step="any" class="fx-search cfs-amt" :class="{ chg: amountOf(c) !== c.ruleAmount }" @input="touched.add(c.chargeCode)">
              <small class="c3-muted cfs-rule">{{ tr('rule') }}: {{ money(c.ruleAmount) }}</small>
            </td>
            <td v-else class="cfs-c">{{ money(c.amount + c.taxAmount) }}<small v-if="c.taxAmount" class="c3-muted"> ({{ tr('incl. tax') }} {{ money(c.taxAmount) }})</small></td>
            <td class="cfs-c">
              <span v-if="deciding" class="cfs-seg" role="group" :aria-label="tr('How paid')">
                <button type="button" :class="{ on: timing(c) === UPFRONT }" @click="setTiming(c, UPFRONT)">{{ tr('Upfront') }}</button>
                <button type="button" :class="{ on: timing(c) === FINANCED }" @click="setTiming(c, FINANCED)">{{ tr('Add to schedule') }}</button>
              </span>
              <span v-else class="fx-chip" :class="c.timingCode === UPFRONT ? 'fx-chip--warn' : 'fx-chip--teal'">{{ tr(c.timingCode === UPFRONT ? 'Upfront' : 'Added to schedule') }}</span>
              <small v-if="deciding ? timing(c) !== c.defaultTimingCode || amountOf(c) !== c.ruleAmount : c.isOverridden" class="c3-muted cfs-ovr">{{ tr('changed from the rule') }}</small>
            </td>
          </tr>
        </tbody>
      </table>
      <p v-else class="c3-muted">{{ tr('The rule has no charges.') }}</p>
      <p v-if="!charges.problem" class="cfs-sum">
        {{ tr('Financed') }} <b>{{ money(totals.financed) }}</b> · {{ tr('Added to schedule') }} <b>{{ money(totals.added) }}</b>
        · {{ tr('Finance amount') }} <b>{{ money(totals.financed + totals.added) }}</b> · {{ tr('Upfront') }} <b>{{ money(totals.upfront) }}</b>
        <template v-if="deciding && changedCount"> · <span class="c3-muted">{{ changedCount }} {{ tr('values changed from what was entered') }}</span></template>
      </p>
    </div>

    <!-- Charges and claim: the claim of the upfront charges (and the down payment we take), then its payment -->
    <div v-if="step.stepCode === 'CF_CHARGES' && step.isMine" class="cfs-box">
      <h4>{{ tr('Upfront claim') }}</h4>
      <template v-if="!loan.claim">
        <p class="c3-muted">
          {{ tr('Upfront charges') }} {{ money(claimCharges) }} · {{ tr('Down payment we take') }} {{ money(claimDown) }} = <b>{{ money(claimCharges + claimDown) }}</b>
        </p>
        <button type="button" class="fx-btn fx-btn--teal fx-btn--sm" :disabled="busy" @click="createClaim">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-file-text-o'" aria-hidden="true"></span>{{ tr(claimCharges + claimDown ? 'Create claim' : 'Confirm nothing upfront') }}
        </button>
      </template>
      <p v-else-if="loan.claim.statusCode === 'NONE'" class="c3-muted">{{ tr('Nothing is paid upfront. Accept to go on to the schedule.') }}</p>
      <template v-else>
        <p>{{ tr('Claim') }} <b>{{ money(loan.claim.totalAmount) }}</b>
          <span class="fx-chip" :class="loan.claim.statusCode === 'PAID' ? 'fx-chip--ok' : 'fx-chip--warn'">{{ tr(loan.claim.statusCode === 'PAID' ? 'Paid' : 'Waiting for payment') }}</span>
          <small v-if="loan.claim.glTranSetId" class="c3-muted"> GL {{ loan.claim.glTranSetId }}</small></p>
        <div v-if="loan.claim.statusCode === 'OPEN'" class="cfs-pay">
          <label class="fc-field"><span>{{ tr('Received into') }}<b> *</b></span>
            <select v-model="pay.bankAccountId" class="fx-select">
              <option :value="null" disabled>{{ tr('Choose the bank account') }}</option>
              <option v-for="a in step.accounts ?? []" :key="a.accountId" :value="a.accountId">{{ a.bankName }} · {{ a.accountRef }}</option>
            </select></label>
          <label class="fc-field"><span>{{ tr('Payment method') }}<b> *</b></span>
            <select v-model="pay.paymentMethodCode" class="fx-select">
              <option v-for="m in METHODS" :key="m[0]" :value="m[0]">{{ tr(m[1]) }}</option>
            </select></label>
          <label class="fc-field"><span>{{ tr('Reference') }}<b v-if="pay.paymentMethodCode !== 'CASH'"> *</b></span>
            <input v-model="pay.paymentReference" class="fx-search" maxlength="100"></label>
          <button type="button" class="fx-btn fx-btn--teal fx-btn--sm" :disabled="busy" @click="payClaim">
            <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-money'" aria-hidden="true"></span>{{ tr('Record payment') }}
          </button>
        </div>
        <p v-else class="c3-muted">{{ tr('Paid') }} {{ loan.claim.paidAt ? formatDateTime(loan.claim.paidAt) : '' }} · {{ loan.claim.paidBy }}
          <template v-if="loan.claim.paymentGlTranSetId"> · GL {{ loan.claim.paymentGlTranSetId }}</template></p>
      </template>
    </div>

    <!-- Delivery and customer receipt (27 Nov 2026): the delivery and the customer's confirmation; accepting books the finance -->
    <div v-if="step.stepCode === 'CF_DELIVERY' && step.isMine" class="cfs-box">
      <h4 class="cfs-sub">{{ tr('Delivery') }}</h4>
      <div class="cfs-grid">
        <label class="fc-field"><span>{{ tr('Delivered on') }}<b> *</b></span><input v-model="delivery.deliveryDate" type="date" class="fx-search" :max="todayIso">
          <small>{{ tr('not before the schedule step, not in the future') }}</small></label>
        <label class="fc-field"><span>{{ tr('Delivery note no.') }}<b> *</b></span><input v-model="delivery.deliveryNoteNo" class="fx-search" maxlength="50"></label>
        <label class="fc-field"><span>{{ tr('Serial / IMEI') }}</span><input v-model="delivery.serialNo" class="fx-search" maxlength="100"></label>
        <label class="fc-field"><span>{{ tr('Delivery note (file)') }}</span><small class="cfs-doc">{{ tr('Add the signed delivery note under Documents below.') }}</small></label>
      </div>
      <label class="cfs-confirm"><input v-model="delivery.customerConfirmed" type="checkbox">{{ tr('The customer received the goods and confirms they match the contract') }}</label>
      <h4 class="cfs-sub">{{ tr('On accept') }}</h4>
      <ul class="cfs-list">
        <li>{{ tr('The finance is booked (rule CF_DELIVERY)') }}: {{ tr('Dr Finance') }} <b>{{ money(loan.principalAmount) }}</b> · {{ tr('Cr Payable to the supplier') }}
          <b>{{ money(financedNow) }}</b><template v-if="loan.principalAmount - financedNow > 0"> · {{ tr('Cr Charges not earned yet') }} <b>{{ money(loan.principalAmount - financedNow) }}</b></template>
          <template v-if="heldDown"> · {{ tr('Down payment held, now due to the supplier') }} <b>{{ money(heldDown) }}</b></template></li>
        <li>{{ tr('Status') }} {{ tr('Approved') }} → <b>{{ tr('Active') }}</b>; {{ tr('the schedule dates move to the delivery day (amounts unchanged)') }}</li>
        <li>{{ tr('The supplier is paid by') }} <b>{{ supplierDue }}</b> ({{ tr('delivery') }} + {{ loan.goods?.supplierPaymentDays ?? 0 }} {{ tr('days') }}) → {{ tr('Supplier payment step (Treasury)') }}</li>
      </ul>
    </div>

    <!-- Supplier payment (27 Nov 2026): what the supplier is owed less its commission, paid from our bank account; accepting posts it -->
    <div v-if="step.stepCode === 'CF_SUPPLIER_PAYMENT' && step.isMine && step.supplierDue" class="cfs-box">
      <h4 class="cfs-sub">{{ tr('Supplier payment') }}</h4>
      <p class="cfs-sum">
        {{ tr('Owed to the supplier') }} <b>{{ money(step.supplierDue.payableAmount) }}</b>
        · {{ tr('Commission') }} <b>{{ money(commissionNow) }}</b> ({{ tr('agreed') }} {{ step.supplierDue.commissionPct }}% = {{ money(step.supplierDue.commissionAmount) }})
        · {{ tr('Net paid') }} <b>{{ money(step.supplierDue.payableAmount - commissionNow) }}</b>
        <template v-if="step.supplierDue.dueDate"> · {{ tr('pay by') }} <b>{{ step.supplierDue.dueDate }}</b></template>
      </p>
      <div class="cfs-grid">
        <label class="fc-field"><span>{{ tr('Paid from') }}<b> *</b></span>
          <select v-model="sp.bankAccountId" class="fx-select">
            <option :value="null" disabled>{{ tr('Choose the bank account') }}</option>
            <option v-for="a in step.accounts ?? []" :key="a.accountId" :value="a.accountId">{{ a.bankName }} · {{ a.accountRef }}</option>
          </select></label>
        <label class="fc-field"><span>{{ tr('Transfer reference') }}<b> *</b></span><input v-model="sp.reference" class="fx-search" maxlength="100"></label>
        <label class="fc-field"><span>{{ tr('Commission') }}</span>
          <input v-model.number="sp.commissionAmount" type="number" min="0" step="any" class="fx-search" :class="{ chg: commissionNow !== step.supplierDue.commissionAmount }">
          <small>{{ tr('agreed') }} {{ money(step.supplierDue.commissionAmount) }}</small></label>
        <label v-if="commissionNow !== step.supplierDue.commissionAmount" class="fc-field"><span>{{ tr('Why the commission changed') }}<b> *</b></span>
          <input v-model="sp.commissionReason" class="fx-search" maxlength="500"></label>
      </div>
      <h4 class="cfs-sub">{{ tr('On accept') }}</h4>
      <ul class="cfs-list">
        <li>{{ tr('Rule CF_SUPPLIER_PAYMENT') }}: {{ tr('Dr Payable to the supplier') }} <b>{{ money(step.supplierDue.payableAmount) }}</b> · {{ tr('Cr Commission income') }}
          <b>{{ money(commissionNow) }}</b> · {{ tr('Cr Bank') }} <b>{{ money(step.supplierDue.payableAmount - commissionNow) }}</b></li>
        <li>{{ tr('The workflow ends; the finance stays active and is collected by its schedule.') }}</li>
      </ul>
    </div>

    <p v-if="step.stepCode === 'CF_SCHEDULE' && step.isMine" class="cfs-note">
      <span class="fa fa-info-circle" aria-hidden="true"></span>
      {{ tr('Accept builds the schedule on the finance amount') }} {{ money(loan.principalAmount) }} {{ tr('over') }} {{ loan.termMonths }} {{ tr('months, from today.') }}
    </p>

    <div v-if="step.isMine" class="cfs-actions">
      <input v-model="comment" class="fx-search cfs-comment" maxlength="2000" :placeholder="tr('Comment (required to reject)')" :aria-label="tr('Comment')">
        <button type="button" class="fx-btn fc-btn-reject" :disabled="busy" @click="complete('REJECT')"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Reject') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="complete('ACCEPT')">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr(ACCEPT[step.stepCode] ?? 'Accept') }}
        </button>
    </div>
  </section>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { loansApi, type CfCharge, type CfCreditDecision, type CfStep, type CfTerms, type Loan } from '../api/loans.api'

const props = defineProps<{ loan: Loan; version: number }>()
const emit = defineEmits<{ changed: [] }>()
const route = useRoute()
const router = useRouter()

const UPFRONT = 'PAID_UPFRONT'
const FINANCED = 'FINANCE_WITH_PRINCIPAL'
const HINT: Record<string, string> = {
  CF_CREDIT: "Check the deal; every value below is this finance's own copy, change any of them. Approving needs the customer's compliance to be clear.",
  CF_CHARGES: 'Create the claim of the upfront charges, record its payment, then accept. Charges added to the schedule are part of the principal.',
  CF_SCHEDULE: 'Check the finance amount and term, then accept to build the schedule.',
  CF_DELIVERY: "Record the delivery and the customer's receipt; accepting books the finance.",
  CF_SUPPLIER_PAYMENT: 'Check what the supplier is owed and its commission, choose the bank account, then pay.',
}
const ACCEPT: Record<string, string> = { CF_CREDIT: 'Approve', CF_SCHEDULE: 'Build schedule and accept', CF_DELIVERY: 'Accept and book', CF_SUPPLIER_PAYMENT: 'Pay the supplier' }
const ROLE: Record<string, string> = {
  CF_CREDIT: 'Credit (CREDIT_OFFICER)', CF_CHARGES: 'Operations (OPERATIONS_OFFICER)', CF_SCHEDULE: 'Operations (OPERATIONS_OFFICER)',
  CF_DELIVERY: 'Operations (OPERATIONS_OFFICER)', CF_SUPPLIER_PAYMENT: 'Treasury (TREASURY_OFFICER)',
}
const METHODS: [string, string][] = [['CASH', 'Cash'], ['BANK_TRANSFER', 'Bank transfer'], ['PAYMENT_NETWORK', 'Payment network'], ['WALLET', 'Wallet']]

const step = ref<CfStep | null>(null)
const comment = ref('')
const busy = ref(false)
const timings = ref<Record<string, string>>({})
const amounts = ref<Record<string, number | string>>({})
/** Credit's values (t) and what was entered (o). */
const t = ref<CfTerms | null>(null)
const o = computed(() => step.value?.terms ?? null)
const touched = new Set<string>()
const todayIso = new Date().toISOString().slice(0, 10)
const delivery = reactive({ deliveryDate: todayIso as string | null, deliveryNoteNo: '', serialNo: '', customerConfirmed: false })
const financedNow = computed(() => (props.loan.goods ? props.loan.goods.priceAmount - props.loan.goods.downPaymentAmount : props.loan.principalAmount))
const heldDown = computed(() => (props.loan.claim?.statusCode === 'PAID' ? props.loan.claim.downPaymentAmount : 0))
const supplierDue = computed(() => {
  if (!delivery.deliveryDate) return '—'
  const d = new Date(`${delivery.deliveryDate}T00:00:00Z`)
  d.setUTCDate(d.getUTCDate() + (props.loan.goods?.supplierPaymentDays ?? 0))
  return d.toISOString().slice(0, 10)
})
const sp = reactive({ bankAccountId: null as number | null, reference: '', commissionAmount: null as number | string | null, commissionReason: '' })
const commissionNow = computed(() => {
  const v = sp.commissionAmount
  return v === null || v === '' || Number.isNaN(Number(v)) ? step.value?.supplierDue?.commissionAmount ?? 0 : Math.round(Number(v) * 100) / 100
})
const pay = reactive({ bankAccountId: null as number | null, paymentMethodCode: 'CASH', paymentReference: '' })
const inboxId = () => (route.query.inboxId ? Number(route.query.inboxId) : null)
const money = (n: number) => new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n)

const charges = computed(() => step.value?.charges ?? null)
/** Credit decides on its own task; later steps only show the decision. */
const deciding = computed(() => step.value?.stepCode === 'CF_CREDIT' && step.value.isMine)
const timing = (c: CfCharge) => timings.value[c.chargeCode] ?? c.timingCode
/** The amount Credit typed (an empty box counts as the rule's amount); the tax follows it. */
const amountOf = (c: CfCharge) => {
  const v = amounts.value[c.chargeCode]
  return v === undefined || v === '' || Number.isNaN(Number(v)) ? c.amount : Math.round(Number(v) * 100) / 100
}
const taxOf = (c: CfCharge) => (c.amount ? Math.round((amountOf(c) * c.taxAmount) / c.amount * 100) / 100 : 0)
const lbl = (list: { value: string; label: string }[], v: string) => list.find((x) => x.value === v)?.label ?? v
const TERM_KEYS = ['downPaymentAmount', 'termMonths', 'repaymentFrequencyCode', 'interestMethodCode', 'annualRatePct', 'dayCountCode', 'lateFeeTypeCode',
  'lateFeeRatePct', 'lateFeeFlatAmount', 'lateFeeBaseCode', 'lateFeeGraceDays', 'paymentOrder', 'isInsuranceRequired', 'isPrepaymentAllowed',
  'prepaymentRecalcCode', 'prepaymentFeePct', 'earlySettlementFeePct'] as const
type TermKey = typeof TERM_KEYS[number]
/** A value Credit changed from what was entered. */
const ch = (k: TermKey) => !!t.value && !!o.value && JSON.stringify(t.value[k] ?? null) !== JSON.stringify(o.value[k] ?? null)
const changedCount = computed(() => TERM_KEYS.filter((k) => ch(k)).length
  + (charges.value?.charges ?? []).filter((c) => timing(c) !== c.defaultTimingCode || amountOf(c) !== c.ruleAmount).length)
const marginNow = computed(() => (t.value && step.value?.productRatePct != null ? Math.round(((Number(t.value.annualRatePct) || 0) - step.value.productRatePct) * 10000) / 10000 : 0))
function moveOrder(i: number) {
  if (!t.value || i < 1) return
  const list = [...t.value.paymentOrder]
  const up = list[i]!
  list[i] = list[i - 1]!
  list[i - 1] = up
  t.value.paymentOrder = list
}
// a new down payment changes the financed amount, so the charges are worked out again (amounts Credit typed stay)
let downTimer: ReturnType<typeof setTimeout> | undefined
function downChanged() {
  clearTimeout(downTimer)
  downTimer = setTimeout(async () => {
    if (!t.value) return
    try {
      const fresh = await loansApi.cfStep(props.loan.loanId, inboxId(), Number(t.value.downPaymentAmount) || 0)
      if (step.value && fresh) step.value = { ...step.value, charges: fresh.charges }
      for (const c of fresh?.charges?.charges ?? []) if (!touched.has(c.chargeCode)) amounts.value[c.chargeCode] = c.amount
    } catch {
      // the check on Approve says what is wrong
    }
  }, 400)
}
function setTiming(c: CfCharge, t: string) {
  timings.value = { ...timings.value, [c.chargeCode]: t }
}
const totals = computed(() => {
  const list = charges.value?.charges ?? []
  const of = (t: string) => list.filter((c) => (deciding.value ? timing(c) : c.timingCode) === t)
    .reduce((s, c) => s + (deciding.value ? amountOf(c) + taxOf(c) : c.amount + c.taxAmount), 0)
  return { financed: charges.value?.financedAmount ?? 0, added: of(FINANCED), upfront: of(UPFRONT) }
})
const claimCharges = computed(() => (charges.value?.charges ?? []).filter((c) => c.timingCode === UPFRONT).reduce((s, c) => s + c.amount + c.taxAmount, 0))
const claimDown = computed(() => (props.loan.goods?.downPaymentCollectorCode === 'COMPANY' ? props.loan.goods.downPaymentAmount : 0))

const message = (err: unknown) => (err instanceof Error ? err.message : String(err))

async function load() {
  try {
    step.value = await loansApi.cfStep(props.loan.loanId, inboxId())
    timings.value = {}
    touched.clear()
    amounts.value = Object.fromEntries((step.value?.charges?.charges ?? []).map((c) => [c.chargeCode, c.amount]))
    t.value = step.value?.terms ? { ...step.value.terms, paymentOrder: [...step.value.terms.paymentOrder] } : null
  } catch (err) {
    showError(message(err))
  }
}

async function createClaim() {
  if (!step.value?.inboxId) return
  busy.value = true
  try {
    await loansApi.createCfClaim(props.loan.loanId, step.value.inboxId)
    showSuccess(tr(claimCharges.value + claimDown.value ? 'Claim created.' : 'Nothing is paid upfront.'))
    emit('changed')
  } catch (err) {
    showError(message(err))
  } finally {
    busy.value = false
  }
}

async function payClaim() {
  if (!step.value?.inboxId) return
  busy.value = true
  try {
    await loansApi.payCfClaim(props.loan.loanId, step.value.inboxId, {
      bankAccountId: pay.bankAccountId, paymentMethodCode: pay.paymentMethodCode, paymentReference: pay.paymentReference.trim() || null,
    })
    showSuccess(tr('Payment recorded.'))
    emit('changed')
  } catch (err) {
    showError(message(err))
  } finally {
    busy.value = false
  }
}

async function complete(action: 'ACCEPT' | 'REJECT') {
  if (!step.value?.inboxId) return
  if (action === 'REJECT' && !comment.value.trim()) {
    showError(tr('Write why the finance is rejected.'))
    return
  }
  busy.value = true
  try {
    const list = charges.value?.charges ?? []
    const decision = deciding.value && action === 'ACCEPT' ? {
      chargeTimings: Object.fromEntries(list.map((c) => [c.chargeCode, timing(c)])),
      chargeAmounts: Object.fromEntries(list.map((c) => [c.chargeCode, amountOf(c)])),
      rateMarginPct: null,
      terms: t.value ? Object.fromEntries(TERM_KEYS.filter((k) => ch(k)).map((k) => [k, t.value![k]])) as CfCreditDecision['terms'] : null
    } : undefined
    const deliveryBody = step.value.stepCode === 'CF_DELIVERY' && action === 'ACCEPT'
      ? { deliveryDate: delivery.deliveryDate, deliveryNoteNo: delivery.deliveryNoteNo.trim() || null, serialNo: delivery.serialNo.trim() || null, customerConfirmed: delivery.customerConfirmed }
      : undefined
    const paymentBody = step.value.stepCode === 'CF_SUPPLIER_PAYMENT' && action === 'ACCEPT'
      ? { bankAccountId: sp.bankAccountId, reference: sp.reference.trim() || null, commissionAmount: commissionNow.value, commissionReason: sp.commissionReason.trim() || null }
      : undefined
    await loansApi.completeCfStep(props.loan.loanId, step.value.inboxId, action, comment.value.trim() || null, decision, deliveryBody, paymentBody)
    showSuccess(tr(action === 'ACCEPT' ? 'Step accepted.' : 'Finance rejected.'))
    comment.value = ''
    // the task is done: drop it from the address so the page shows the next step as someone else's
    const query = { ...route.query }
    delete query.inboxId
    await router.replace({ query })
    emit('changed')
  } catch (err) {
    showError(message(err))
  } finally {
    busy.value = false
  }
}

watch(() => [props.loan.loanId, props.version, route.query.inboxId], load, { immediate: true })
</script>

<style scoped>
.cfs { border-inline-start: 3px solid var(--fin-teal-600); }
.cfs { padding: 14px 18px; }
.cfs-head { display: flex; gap: 10px; align-items: center; flex-wrap: wrap; }
.cfs-head h3 { margin: 0; font-size: 15px; }
.cfs-head p { margin: 0; color: var(--fin-muted); font-size: 13px; }
.cfs-deal { display: flex; flex-wrap: wrap; gap: 6px; margin-top: 10px; }
.cfs-deal > span { padding: 3px 10px; border-radius: 999px; background: #f2f4f6; color: #34495a; font-size: 12.5px; }
.cfs-deal b { color: var(--fin-ink); }
.cfs-terms { margin-top: 10px; }
.cfs-confirm { display: flex; align-items: center; gap: 8px; margin-top: 10px; font-size: 13.5px; }
.cfs-confirm input { width: 16px; height: 16px; }
.cfs-list { margin: 0; padding-inline-start: 18px; font-size: 13px; line-height: 1.7; }
.cfs-doc { display: block; padding-top: 8px; color: var(--fin-muted); }
.cfs-sub { margin: 10px 0 6px; font-size: 11.5px; font-weight: 700; letter-spacing: .4px; text-transform: uppercase; color: var(--fin-muted); }
.cfs-grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px 14px; }
.cfs-grid .fc-field small { color: var(--fin-muted); font-size: 11.5px; }
.cfs-order { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; margin-top: 10px; font-size: 12.5px; }
.cfs-part { display: inline-flex; align-items: center; gap: 4px; padding: 3px 10px 3px 4px; border: 1px solid var(--fin-line); border-radius: 999px; }
.cfs-part button { border: 0; background: none; padding: 0 4px; cursor: pointer; color: var(--fin-teal-700); }
.cfs-part button:disabled { color: #c5ccd3; cursor: default; }
@media (max-width: 900px) { .cfs-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
.cfs-rate { display: flex; flex-wrap: wrap; align-items: center; gap: 12px; margin-top: 10px; font-size: 13px; }
.cfs-rate label { display: inline-flex; align-items: center; gap: 6px; font-weight: 700; color: #5b6b78; font-size: 12.5px; }
.cfs-rate input { width: 90px; height: 30px; text-align: end; }
.cfs-eff { padding: 3px 12px; border-radius: 999px; background: var(--fin-teal-50); color: var(--fin-teal-700); }
.cfs-amt { width: 110px; height: 30px; text-align: end; }
.cfs-rule { margin-inline-start: 6px; white-space: nowrap; }
/* every row on one centred line; amount and timing centred in their columns */
.cfs :deep(.fx-table) th, .cfs :deep(.fx-table) td { vertical-align: middle; }
.cfs .cfs-c { text-align: center; white-space: nowrap; }
.cfs .cfs-c > * { vertical-align: middle; }
.cfs-deal { align-items: center; }
.chg { border-color: #d39b2a !important; background: #fffaf0 !important; }
.cfs-ic { display: grid; place-items: center; width: 34px; height: 34px; border-radius: 9px; background: var(--fin-teal-50); color: var(--fin-teal-700); flex: none; }
.cfs-box { margin-top: 12px; padding-top: 10px; border-top: 1px solid #f0f2f4; }
.cfs-box h4 { margin: 0 0 6px; font-size: 13px; color: var(--fin-muted); }
.cfs-box p { margin: 6px 0; font-size: 13px; }
.cfs-sum { color: #34495a; }
.cfs-bad { color: var(--fin-danger, #b42318); }
.cfs-ovr { margin-inline-start: 6px; }
.cfs-seg { display: inline-flex; border: 1px solid var(--fin-line); border-radius: 8px; overflow: hidden; }
.cfs-seg button { padding: 5px 10px; border: 0; background: #fff; color: #34495a; font: 600 12px/1 inherit; cursor: pointer; }
.cfs-seg button.on { background: var(--fin-teal-600); color: #fff; }
.cfs-pay { display: flex; flex-wrap: wrap; align-items: flex-end; gap: 10px; }
.cfs-pay .fc-field { min-width: 180px; }
.cfs-note { margin: 12px 0 0; font-size: 13px; color: #34495a; }
.cfs-note .fa { color: var(--fin-teal-700); margin-inline-end: 6px; }
.cfs-actions { display: flex; gap: 8px; align-items: center; margin-top: 12px; }
.cfs-comment { flex: 1; min-width: 0; height: 36px; }
</style>
