<template>
  <!-- A consumer finance's upfront claim (Ahmed, 27 Nov 2026), the same standard as a factoring claim: its lines, what is paid and
       outstanding, each payment with its GL, and Record payment (all or part) for COLLECTION.EDIT. Opened from the workspace Claims tab
       and from the finance page. -->
  <Crm360Dialog v-model:visible="shown" :title="`${tr('Claim')} · ${claim?.loanNumber ?? ''}`" :subtitle="claim?.clientName ?? ''" icon="fa-flag-o" :width="980">
    <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
    <div v-else-if="!claim" class="fx-empty"><span class="fa fa-flag-o" aria-hidden="true"></span>{{ tr('This finance has no claim.') }}</div>
    <template v-else>
      <div class="ccd-chips">
        <span class="fx-chip" :class="claimTone(claim.statusCode)">{{ tr(CLAIM_LABELS[claim.statusCode] ?? claim.statusCode) }}</span>
        <span>{{ tr('Created') }} <b>{{ claim.claimDate ? formatDate(claim.claimDate) : '—' }}</b><template v-if="claim.createdBy"> · {{ claim.createdBy }}</template></span>
        <span>{{ tr('Currency') }} <b>{{ claim.currencyCode }}</b></span>
        <span v-if="claim.glTranSetId">GL <b>{{ claim.glTranSetId }}</b></span>
      </div>

      <table class="fx-table">
        <thead><tr><th>{{ tr('Line') }}</th><th class="fx-r">{{ tr('Amount') }}</th><th class="fx-r">{{ tr('Tax') }}</th><th class="fx-r">{{ tr('Total') }}</th></tr></thead>
        <tbody>
          <tr v-for="l in claim.lines" :key="l.code"><td>{{ tr(l.name) }}</td><td class="fx-r">{{ money(l.amount) }}</td><td class="fx-r">{{ money(l.taxAmount) }}</td><td class="fx-r">{{ money(l.totalAmount) }}</td></tr>
        </tbody>
        <tfoot>
          <tr><th>{{ tr('Claim Total') }}</th><th></th><th></th><th class="fx-r">{{ money(claim.totalAmount) }}</th></tr>
          <tr><td>{{ tr('Paid') }}</td><td></td><td></td><td class="fx-r">{{ money(claim.paidAmount) }}</td></tr>
          <tr><th>{{ tr('Outstanding') }}</th><th></th><th></th><th class="fx-r">{{ money(claim.outstandingAmount) }}</th></tr>
        </tfoot>
      </table>

      <h4 class="ccd-sub">{{ tr('Payments') }}</h4>
      <table v-if="claim.payments.length" class="fx-table">
        <thead><tr><th>{{ tr('Date') }}</th><th class="fx-r">{{ tr('Amount') }}</th><th class="fx-r">{{ tr('Charges') }}</th><th class="fx-r">{{ tr('Down payment') }}</th>
          <th>{{ tr('Received into') }}</th><th>{{ tr('Payment method') }}</th><th>{{ tr('Reference') }}</th><th>GL</th><th>{{ tr('Recorded by') }}</th></tr></thead>
        <tbody>
          <tr v-for="p in claim.payments" :key="p.claimPaymentId">
            <td>{{ formatDate(p.paymentDate) }}</td><td class="fx-r">{{ money(p.paymentAmount) }}</td><td class="fx-r">{{ money(p.chargesAmount) }}</td>
            <td class="fx-r">{{ money(p.downPaymentAmount) }}</td><td>{{ p.bankAccount ?? '—' }}</td><td>{{ tr(METHOD[p.paymentMethodCode] ?? p.paymentMethodCode) }}</td>
            <td>{{ p.paymentReference ?? '—' }}</td><td>{{ p.glTranSetId ?? '—' }}</td><td>{{ p.recordedBy }}</td>
          </tr>
        </tbody>
      </table>
      <p v-else class="c3-muted ccd-p">{{ tr('No payment yet.') }}</p>

      <template v-if="canPay && open">
        <h4 class="ccd-sub">{{ tr('Record payment') }}</h4>
        <div class="ccd-pay">
          <label class="fc-field"><span>{{ tr('Amount') }}<b> *</b></span>
            <input v-model.number="pay.amount" type="number" min="0" step="any" class="fx-search"><small class="c3-muted">{{ tr('outstanding') }} {{ money(claim.outstandingAmount) }}</small></label>
          <label class="fc-field"><span>{{ tr('Received into') }}<b> *</b></span>
            <select v-model="pay.bankAccountId" class="fx-select">
              <option :value="null" disabled>{{ tr('Choose the bank account') }}</option>
              <option v-for="a in claim.accounts" :key="a.accountId" :value="a.accountId">{{ a.bankName }} · {{ a.accountRef }}</option>
            </select></label>
          <label class="fc-field"><span>{{ tr('Payment method') }}<b> *</b></span>
            <select v-model="pay.paymentMethodCode" class="fx-select"><option v-for="(label, code) in METHOD" :key="code" :value="code">{{ tr(label) }}</option></select></label>
          <label class="fc-field"><span>{{ tr('Reference') }}<b v-if="pay.paymentMethodCode !== 'CASH'"> *</b></span>
            <input v-model="pay.paymentReference" class="fx-search" maxlength="100"></label>
        </div>
        <p class="c3-muted ccd-p">{{ tr('A payment pays the charges first, then the down payment. The schedule step waits until the claim is paid.') }}</p>
      </template>
      <p v-else-if="open" class="c3-muted ccd-p">{{ tr('Recording a payment needs the Collection edit permission.') }}</p>
    </template>
    <template #footer>
      <RouterLink v-if="claim && showFinance" class="fx-btn" :to="{ name: 'lnd-loan', params: { loanId: claim.loanId } }">
        <span class="fa fa-external-link" aria-hidden="true"></span>{{ tr('Open finance') }}</RouterLink>
      <button type="button" class="fx-btn" @click="shown = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
      <button v-if="claim && canPay && open" type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="record">
        <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-money'" aria-hidden="true"></span>{{ tr('Record payment') }}</button>
    </template>
  </Crm360Dialog>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { CLAIM_LABELS, claimTone, loansApi, type CfClaimDetail } from '../api/loans.api'

const props = withDefaults(defineProps<{ loanId: number | null; showFinance?: boolean }>(), { showFinance: true })
const shown = defineModel<boolean>('visible', { default: false })
const emit = defineEmits<{ changed: [] }>()

const METHOD: Record<string, string> = { CASH: 'Cash', BANK_TRANSFER: 'Bank transfer', PAYMENT_NETWORK: 'Payment network', WALLET: 'Wallet' }
const canPay = hasPermission('COLLECTION.EDIT')
const claim = ref<CfClaimDetail | null>(null)
const loading = ref(false)
const busy = ref(false)
const pay = reactive({ amount: null as number | string | null, bankAccountId: null as number | null, paymentMethodCode: 'CASH', paymentReference: '' })
const open = computed(() => claim.value?.statusCode === 'OPEN' || claim.value?.statusCode === 'PARTIALLY_PAID')
const money = (n: number) => new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n)
const message = (err: unknown) => (err instanceof Error ? err.message : String(err))

async function load() {
  if (!props.loanId) return
  loading.value = true
  try {
    claim.value = await loansApi.cfClaim(props.loanId)
    pay.amount = claim.value?.outstandingAmount ?? null
    pay.paymentReference = ''
  } catch (err) {
    showError(message(err))
  } finally {
    loading.value = false
  }
}

async function record() {
  if (!props.loanId) return
  busy.value = true
  try {
    const amount = pay.amount === null || pay.amount === '' ? null : Number(pay.amount)
    claim.value = await loansApi.payCfClaimPart(props.loanId, {
      amount, bankAccountId: pay.bankAccountId, paymentMethodCode: pay.paymentMethodCode, paymentReference: pay.paymentReference.trim() || null,
    })
    pay.amount = claim.value?.outstandingAmount ?? null
    pay.paymentReference = ''
    showSuccess(tr('Payment recorded.'))
    emit('changed')
  } catch (err) {
    showError(message(err))
  } finally {
    busy.value = false
  }
}

watch(() => [shown.value, props.loanId], () => { if (shown.value) void load() }, { immediate: true })
</script>

<style scoped>
.ccd-chips { display: flex; flex-wrap: wrap; gap: 6px; align-items: center; margin-bottom: 10px; }
.ccd-chips > span:not(.fx-chip) { padding: 3px 10px; border-radius: 999px; background: #f2f4f6; color: #34495a; font-size: 12.5px; }
.ccd-sub { margin: 14px 0 6px; font-size: 11.5px; font-weight: 700; letter-spacing: .4px; text-transform: uppercase; color: var(--fin-muted); }
.ccd-p { margin: 6px 0; font-size: 13px; }
.ccd-pay { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px 14px; }
@media (max-width: 900px) { .ccd-pay { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
