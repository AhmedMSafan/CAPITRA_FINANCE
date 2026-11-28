<template>
  <!-- A consumer finance's upfront claim (Ahmed, 27 Nov 2026), the same standard as a factoring claim, read only: its lines, what is
       paid and outstanding, each payment with its GL. Pay opens Collection › Customer Payments for it (COLLECTION.EDIT), like the
       factoring Claims tab. Opened from the workspace Claims tab and from the finance page. -->
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
          <th>{{ tr('Repayment No.') }}</th><th>{{ tr('Source') }}</th><th>{{ tr('Reference') }}</th><th>GL</th><th>{{ tr('Recorded by') }}</th></tr></thead>
        <tbody>
          <tr v-for="p in claim.payments" :key="p.claimPaymentId">
            <td>{{ formatDate(p.paymentDate) }}</td><td class="fx-r">{{ money(p.paymentAmount) }}</td><td class="fx-r">{{ money(p.chargesAmount) }}</td>
            <td class="fx-r">{{ money(p.downPaymentAmount) }}</td><td>{{ p.repaymentNumber ?? '—' }}</td><td>{{ tr(METHOD[p.paymentMethodCode] ?? p.paymentMethodCode) }}</td>
            <td>{{ p.paymentReference ?? '—' }}</td><td>{{ p.glTranSetId ?? '—' }}</td><td>{{ p.recordedBy }}</td>
          </tr>
        </tbody>
      </table>
      <p v-else class="c3-muted ccd-p">{{ tr('No payment yet.') }}</p>

      <p v-if="open" class="c3-muted ccd-p">{{ tr('The claim is paid in Collection › Customer Payments, from the customer\'s payment sources. The schedule step waits until it is paid.') }}</p>
    </template>
    <template #footer>
      <RouterLink v-if="claim && showFinance" class="fx-btn" :to="{ name: 'lnd-loan', params: { loanId: claim.loanId } }">
        <span class="fa fa-external-link" aria-hidden="true"></span>{{ tr('Open finance') }}</RouterLink>
      <button type="button" class="fx-btn" @click="shown = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
      <RouterLink v-if="claim && canPay && open" class="fx-btn fx-btn--teal"
                  :to="{ name: 'collection-customer-payments', query: { clientId: claim.clientId, cfLoanId: claim.loanId } }">
        <span class="fa fa-money" aria-hidden="true"></span>{{ tr('Pay') }}</RouterLink>
    </template>
  </Crm360Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { CLAIM_LABELS, claimTone, loansApi, type CfClaimDetail } from '../api/loans.api'

const props = withDefaults(defineProps<{ loanId: number | null; showFinance?: boolean }>(), { showFinance: true })
const shown = defineModel<boolean>('visible', { default: false })

const METHOD: Record<string, string> = {
  CASH: 'Cash', BANK_TRANSFER: 'Bank Transfer', PAYMENT_NETWORK: 'Payment network', WALLET: 'Wallet', CHEQUE: 'Collected Cheque', CREDIT_BALANCE: 'Credit Balance',
}
const canPay = hasPermission('COLLECTION.EDIT')
const claim = ref<CfClaimDetail | null>(null)
const loading = ref(false)
const open = computed(() => claim.value?.statusCode === 'OPEN' || claim.value?.statusCode === 'PARTIALLY_PAID')
const money = (n: number) => new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n)
const message = (err: unknown) => (err instanceof Error ? err.message : String(err))

async function load() {
  if (!props.loanId) return
  loading.value = true
  try {
    claim.value = await loansApi.cfClaim(props.loanId)
  } catch (err) {
    showError(message(err))
  } finally {
    loading.value = false
  }
}

watch(() => [shown.value, props.loanId], () => { if (shown.value) void load() }, { immediate: true })
</script>

<style scoped>
.ccd-chips { display: flex; flex-wrap: wrap; gap: 6px; align-items: center; margin-bottom: 10px; }
.ccd-chips > span:not(.fx-chip) { padding: 3px 10px; border-radius: 999px; background: #f2f4f6; color: #34495a; font-size: 12.5px; }
.ccd-sub { margin: 14px 0 6px; font-size: 11.5px; font-weight: 700; letter-spacing: .4px; text-transform: uppercase; color: var(--fin-muted); }
.ccd-p { margin: 6px 0; font-size: 13px; }
</style>
