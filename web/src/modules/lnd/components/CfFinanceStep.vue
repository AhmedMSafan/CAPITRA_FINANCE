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
      <span>{{ tr('Down payment') }} <b>{{ money(loan.goods.downPaymentAmount) }}</b> ({{ tr(loan.goods.downPaymentCollectorCode === 'COMPANY' ? 'us' : 'supplier') }})</span>
      <span>{{ tr('Financed') }} <b>{{ money(loan.goods.priceAmount - loan.goods.downPaymentAmount) }}</b></span>
      <span>{{ tr('Term') }} <b>{{ loan.termMonths }} {{ tr('months') }}</b></span>
      <span>{{ tr('Commission') }} <b>{{ loan.goods.supplierCommissionPct ?? 0 }}%</b></span>
    </div>

    <!-- the rate: the product rate stays fixed, Credit adds or takes off a margin -->
    <div v-if="deciding && step.productRatePct !== null" class="cfs-rate">
      <span class="c3-muted">{{ tr('Product rate') }} <b>{{ step.productRatePct }}%</b></span>
      <label>{{ tr('Margin ±%') }}<input v-model.number="margin" type="number" step="0.25" class="fx-search" :class="{ chg: Number(margin) !== 0 }"></label>
      <span class="cfs-eff">{{ tr('Finance rate') }} <b>{{ financeRate }}%</b></span>
      <small class="c3-muted">{{ tr('The product rate stays fixed; Credit adds or takes off a margin for this finance.') }}</small>
    </div>

    <!-- the charges: Credit decides (the rule's default is marked), later steps show the decision -->
    <div v-if="charges && (step.isMine || charges.decided)" class="cfs-box">
      <h4>{{ tr('Charges') }} <small v-if="charges.decided" class="c3-muted">· {{ tr('decided by Credit') }}</small></h4>
      <p v-if="charges.problem" class="cfs-bad">{{ charges.problem }}</p>
      <table v-else-if="charges.charges.length" class="fx-table">
        <thead><tr><th>{{ tr('Charge') }}</th><th>{{ tr('Basis') }}</th><th class="fx-r">{{ tr('Amount') }}</th><th>{{ tr('How paid') }}</th></tr></thead>
        <tbody>
          <tr v-for="c in charges.charges" :key="c.chargeCode">
            <td>{{ tr(c.chargeName) }}</td><td>{{ tr(c.basisText) || '—' }}</td>
            <td v-if="deciding" class="fx-r">
              <input v-model.number="amounts[c.chargeCode]" type="number" min="0" step="any" class="fx-search cfs-amt" :class="{ chg: amountOf(c) !== c.ruleAmount }">
              <small class="c3-muted cfs-rule">{{ tr('rule') }}: {{ money(c.ruleAmount) }}</small>
            </td>
            <td v-else class="fx-r">{{ money(c.amount + c.taxAmount) }}<small v-if="c.taxAmount" class="c3-muted"> ({{ tr('incl. tax') }} {{ money(c.taxAmount) }})</small></td>
            <td>
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
import { loansApi, type CfCharge, type CfStep, type Loan } from '../api/loans.api'

const props = defineProps<{ loan: Loan; version: number }>()
const emit = defineEmits<{ changed: [] }>()
const route = useRoute()
const router = useRouter()

const UPFRONT = 'PAID_UPFRONT'
const FINANCED = 'FINANCE_WITH_PRINCIPAL'
const HINT: Record<string, string> = {
  CF_CREDIT: "Check the customer, the supplier, the amount and term, and decide each charge. Approving needs the customer's compliance to be clear.",
  CF_CHARGES: 'Create the claim of the upfront charges, record its payment, then accept. Charges added to the schedule are part of the principal.',
  CF_SCHEDULE: 'Check the finance amount and term, then accept to build the schedule.',
}
const ACCEPT: Record<string, string> = { CF_CREDIT: 'Approve', CF_SCHEDULE: 'Build schedule and accept' }
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
const margin = ref<number | string>(0)
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
const financeRate = computed(() => Math.round(((step.value?.productRatePct ?? 0) + (Number(margin.value) || 0)) * 10000) / 10000)
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
    amounts.value = Object.fromEntries((step.value?.charges?.charges ?? []).map((c) => [c.chargeCode, c.amount]))
    margin.value = step.value?.rateMarginPct ?? 0
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
      rateMarginPct: Number(margin.value) || 0
    } : undefined
    await loansApi.completeCfStep(props.loan.loanId, step.value.inboxId, action, comment.value.trim() || null, decision)
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
.cfs-rate { display: flex; flex-wrap: wrap; align-items: center; gap: 12px; margin-top: 10px; font-size: 13px; }
.cfs-rate label { display: inline-flex; align-items: center; gap: 6px; font-weight: 700; color: #5b6b78; font-size: 12.5px; }
.cfs-rate input { width: 90px; height: 30px; text-align: end; }
.cfs-eff { padding: 3px 12px; border-radius: 999px; background: var(--fin-teal-50); color: var(--fin-teal-700); }
.cfs-amt { width: 110px; height: 30px; text-align: end; }
.cfs-rule { display: block; margin-top: 2px; }
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
