<template>
  <!-- Customer Payments (Ahmed, 2 Nov 2026): pay a customer's open claim of disbursement charges from one of the customer's payment
       sources, fully or in part. Each payment posts its own GL Tran Set (DR customer liability / CR each charge's income). Pay on the
       Claims tab of the Factoring Workspace opens this page with the customer and the claim already chosen. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-money"></span></span>
        <div>
          <h1>{{ tr('Customer Payments') }}</h1>
          <p>{{ tr('Pay a customer\'s open claim from the customer\'s payment sources.') }}</p>
        </div>
      </div>
      <div class="col-actions">
        <RouterLink v-if="fromWorkspace" class="fx-btn" :to="{ name: 'FactoringCustomerWorkspace', params: { clientId: clientId }, query: { tab: 'claims', from: 'customer' } }">
          <span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Claims') }}</RouterLink>
        <RouterLink v-else class="fx-btn" to="/collection-repayments"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Collection and Repayments') }}</RouterLink>
      </div>
    </div>

    <section class="col-sec">
      <h2><span class="fa fa-user" aria-hidden="true"></span>{{ tr('Customer and Claim') }}</h2>
      <div class="col-grid">
        <label class="fc-field">
          <span>{{ tr('Customer') }} <b>*</b></span>
          <select v-model="clientId" class="fx-select" :disabled="busy" @change="customerChanged">
            <option :value="null">{{ tr('- Select -') }}</option>
            <option v-for="c in customers" :key="c.value" :value="Number(c.value)">{{ c.label }}</option>
          </select>
        </label>
        <label class="fc-field">
          <span>{{ tr('Open Claim') }} <b>*</b></span>
          <select v-model="claimId" class="fx-select" :disabled="busy || clientId == null" @change="claimChanged">
            <option :value="null">{{ tr('- Select -') }}</option>
            <option v-for="c in claims" :key="c.claimId" :value="c.claimId">
              {{ c.claimNumber }} · {{ c.disbursementNumber }} · {{ formatAmount(c.outstandingAmount) }}</option>
          </select>
        </label>
      </div>
      <p v-if="clientId != null && !claims.length && !loading" class="ie-hint">
        <span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('This customer has no open claim.') }}</p>
    </section>

    <section v-if="claim" class="col-sec">
      <h2>
        <span class="fa fa-flag-o" aria-hidden="true"></span>{{ tr('Claim') }} {{ claim.claim.claimNumber }}
        <span class="fx-chip" :class="claimTone(claim.claim.statusCode)">{{ tr(claimLabel(claim.claim.statusCode)) }}</span>
      </h2>
      <div class="cp-totals">
        <div><small>{{ tr('Disbursement No') }}</small><b>{{ claim.claim.disbursementNumber }}</b></div>
        <div><small>{{ tr('Claim Total') }}</small><b>{{ formatAmount(claim.claim.totalAmount) }}</b></div>
        <div><small>{{ tr('Paid') }}</small><b class="cp-ok">{{ formatAmount(claim.claim.paidAmount) }}</b></div>
        <div><small>{{ tr('Outstanding') }}</small><b :class="{ 'cp-due': claim.claim.outstandingAmount > 0 }">{{ formatAmount(claim.claim.outstandingAmount) }}</b></div>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Charge') }}</th><th class="fx-r">{{ tr('Amount') }}</th><th class="fx-r">{{ tr('Tax') }}</th><th class="fx-r">{{ tr('Total') }}</th>
              <th class="fx-r">{{ tr('Paid') }}</th><th class="fx-r">{{ tr('Outstanding') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="l in claim.lines" :key="l.claimLineId">
              <td><b>{{ l.chargeName || l.chargeCode }}</b> <small class="cp-code">{{ l.chargeCode }}</small></td>
              <td class="fx-r">{{ formatAmount(l.chargeAmount) }}</td>
              <td class="fx-r">{{ formatAmount(l.taxAmount) }}</td>
              <td class="fx-r"><b>{{ formatAmount(l.totalAmount) }}</b></td>
              <td class="fx-r">{{ formatAmount(l.paidAmount) }}</td>
              <td class="fx-r"><b>{{ formatAmount(l.outstandingAmount) }}</b></td>
            </tr>
          </tbody>
        </table>
      </div>

      <template v-if="claim.payments.length">
        <h3 class="cp-sub">{{ tr('Payments') }}</h3>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead>
              <tr>
                <th>{{ tr('Date') }}</th><th>{{ tr('Repayment No.') }}</th><th>{{ tr('Source') }}</th><th>{{ tr('Reference') }}</th>
                <th class="fx-r">{{ tr('Amount') }}</th><th>{{ tr('GL Tran Set ID') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in claim.payments" :key="p.claimPaymentId">
                <td>{{ formatDate(p.paymentDate) }}</td>
                <td>{{ p.repaymentNumber }}</td>
                <td>{{ tr(sourceLabel(p.sourceType)) }}</td>
                <td>{{ p.reference || '-' }}</td>
                <td class="fx-r"><b>{{ formatAmount(p.paymentAmount) }}</b></td>
                <td><b>{{ p.glTranSetId || '-' }}</b></td>
              </tr>
            </tbody>
          </table>
        </div>
      </template>
    </section>

    <section v-if="claim && isOpen" class="col-sec">
      <h2><span class="fa fa-university" aria-hidden="true"></span>{{ tr('Payment Sources') }}</h2>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th class="cp-pick"><span class="fx-sr">{{ tr('Use') }}</span></th>
              <th>{{ tr('Repayment No.') }}</th><th>{{ tr('Source') }}</th><th>{{ tr('Reference') }}</th><th>{{ tr('Value Date') }}</th>
              <th class="fx-r">{{ tr('Amount') }}</th><th class="fx-r">{{ tr('Used') }}</th><th class="fx-r">{{ tr('Available') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in sources" :key="s.repaymentSourceId" :class="{ 'is-chosen': sourceId === s.repaymentSourceId }">
              <td class="cp-pick"><input :id="`src-${s.repaymentSourceId}`" v-model="sourceId" type="radio" name="source" :value="s.repaymentSourceId"
                                         :disabled="!canEdit || busy" @change="amount = suggested"></td>
              <td><label :for="`src-${s.repaymentSourceId}`"><b>{{ s.repaymentNumber }}</b></label></td>
              <td>{{ tr(sourceLabel(s.sourceType)) }}</td>
              <td>{{ s.reference || '-' }}</td>
              <td>{{ formatDate(s.valueDate) }}</td>
              <td class="fx-r">{{ formatAmount(s.sourceAmount) }}</td>
              <td class="fx-r">{{ formatAmount(s.usedAmount) }}</td>
              <td class="fx-r"><b>{{ formatAmount(s.availableAmount) }}</b></td>
            </tr>
            <tr v-if="!sources.length">
              <td colspan="8" class="fx-empty">{{ tr('This customer has no payment source with balance. Record the customer\'s payment (collected cheque or bank transfer) first.') }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="canEdit" class="col-actions cp-pay">
        <label class="fc-field">
          <span>{{ tr('Amount to Pay') }}</span>
          <input v-model.number="amount" class="fx-search" type="number" min="0" step="0.01" inputmode="decimal" :disabled="sourceId == null || busy">
        </label>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="sourceId == null || busy || !(Number(amount) > 0)" @click="pay">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr('Pay Claim') }}
        </button>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import './collection.css'
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import type { Claim, ClaimRow } from '@/modules/crm/api/invoiceEntry.api'
import { claimPaymentApi, type ClaimSource, type Option } from '../api/collection.api'
import { messageOf } from './collection'

const route = useRoute()
const canEdit = hasPermission('COLLECTION.EDIT')
// Pay on the Factoring Workspace's Claims tab passes the customer and the claim
const fromWorkspace = route.query.clientId != null

const customers = ref<Option[]>([])
const clientId = ref<number | null>(route.query.clientId ? Number(route.query.clientId) : null)
const claims = ref<ClaimRow[]>([])
const claimId = ref<number | null>(route.query.claimId ? Number(route.query.claimId) : null)
const claim = ref<Claim | null>(null)
const sources = ref<ClaimSource[]>([])
const sourceId = ref<number | null>(null)
const amount = ref<number | null>(null)
const loading = ref(false)
const busy = ref(false)

const isOpen = computed(() => claim.value?.claim.statusCode === 'OPEN' || claim.value?.claim.statusCode === 'PARTIALLY_PAID')
// the most the chosen source can pay of the claim
const suggested = computed(() => {
  const s = sources.value.find((x) => x.repaymentSourceId === sourceId.value)
  return s && claim.value ? Math.round(Math.min(s.availableAmount, claim.value.claim.outstandingAmount) * 100) / 100 : null
})

const SOURCE_LABELS: Record<string, string> = { CHEQUE: 'Collected Cheque', BANK_TRANSFER: 'Bank Transfer', CREDIT_BALANCE: 'Credit Balance' }
const sourceLabel = (t: string) => SOURCE_LABELS[t] ?? t
const CLAIM_LABELS: Record<string, string> = { OPEN: 'Unpaid', PARTIALLY_PAID: 'Partially Paid', PAID: 'Paid', CANCELLED: 'Cancelled' }
const claimLabel = (s: string) => CLAIM_LABELS[s] ?? s
const claimTone = (s: string) => (s === 'PAID' ? 'fx-chip--ok' : s === 'CANCELLED' ? 'fx-chip--bad' : 'fx-chip--warn')

async function guard(work: () => Promise<void>) {
  loading.value = true
  try {
    await work()
  } catch (e) {
    showError(messageOf(e))
  } finally {
    loading.value = false
  }
}

async function loadClaim() {
  sourceId.value = null
  amount.value = null
  if (claimId.value == null || clientId.value == null) {
    claim.value = null
    return
  }
  ;[claim.value, sources.value] = await Promise.all([claimPaymentApi.claim(claimId.value), claimPaymentApi.sources(clientId.value)])
}

const customerChanged = () => guard(async () => {
  claimId.value = null
  claim.value = null
  claims.value = clientId.value == null ? [] : await claimPaymentApi.claims(clientId.value)
  // a customer with one open claim has it chosen
  if (claims.value.length === 1) {
    claimId.value = claims.value[0]!.claimId
    await loadClaim()
  }
})

const claimChanged = () => guard(loadClaim)

async function pay() {
  if (claimId.value == null || sourceId.value == null) return
  busy.value = true
  try {
    const r = await claimPaymentApi.pay(claimId.value, sourceId.value, Number(amount.value) || null)
    showSuccess(tr(r.message))
    claims.value = clientId.value == null ? [] : await claimPaymentApi.claims(clientId.value)
    claim.value = r.claim
    sources.value = await claimPaymentApi.sources(r.claim.claim.clientId)
    sourceId.value = null
    amount.value = null
  } catch (e) {
    showError(messageOf(e))
  } finally {
    busy.value = false
  }
}

onMounted(() => guard(async () => {
  customers.value = await claimPaymentApi.customers()
  if (clientId.value == null) return
  // the customer passed in is listed even when its claims are all paid
  if (!customers.value.some((c) => Number(c.value) === clientId.value))
    customers.value = [...customers.value, { value: String(clientId.value), label: String(clientId.value) }]
  claims.value = await claimPaymentApi.claims(clientId.value)
  await loadClaim()
}))
</script>

<style scoped>
.cp-totals { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; margin-bottom: 14px; }
.cp-totals > div { padding: 10px 14px; border: 1px solid var(--fin-line); border-radius: 10px; background: var(--fin-surface); }
.cp-totals small { display: block; font-size: 11px; color: var(--fin-muted); }
.cp-totals b { font-size: 16px; }
.cp-ok { color: #00636D; }
.cp-due { color: #c25a0a; }
.cp-code { color: var(--fin-muted); }
.cp-sub { margin: 16px 0 8px; font-size: 14px; }
.cp-pick { width: 44px; }
.cp-pay { display: flex; flex-direction: row; flex-wrap: wrap; margin-top: 14px; align-items: flex-end; justify-content: flex-end; gap: 12px; }
.fc .cp-pay .fc-field { flex: 0 0 220px; }
tr.is-chosen td { background: #E6F3F4; }
.ie-hint { margin-top: 10px; font-size: 13px; color: var(--fin-muted); }
.ie-hint .fa { margin-inline-end: 6px; }
.fx-sr { position: absolute; width: 1px; height: 1px; overflow: hidden; clip: rect(0 0 0 0); }
@media (max-width: 760px) { .cp-totals { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
