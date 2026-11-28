<template>
  <!-- One party of the disbursement (Ahmed, 7 Nov 2026): who it is, the bills (invoices) in this disbursement, the amount it receives
       (the seller: the supplier in reverse factoring) and the bank account the money goes to. Read only. -->
  <Crm360Dialog v-model:visible="visible" :title="detail?.name ?? tr('Loading…')" :subtitle="subtitle" icon="fa-user" :width="860">
    <p v-if="loading" class="trs-muted"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span> {{ tr('Loading…') }}</p>
    <template v-else-if="detail">
      <dl class="c3-facts">
        <div><dt>{{ tr('Role') }}</dt><dd>{{ tr(roleLabel) }}</dd></div>
        <div><dt>{{ tr('Tax No.') }}</dt><dd>{{ detail.taxNumber ?? '—' }}</dd></div>
        <div><dt>{{ tr('Customer No.') }}</dt><dd>{{ detail.clientCode ?? tr('Not a customer yet') }}</dd></div>
        <div><dt>{{ tr('Share of the disbursement') }}</dt><dd>{{ (detail.weight * 100).toFixed(0) }}%</dd></div>
      </dl>

      <!-- the money: what this party receives and where it goes -->
      <div v-if="detail.amountToReceive !== null" class="trs-pay" :class="{ 'trs-pay--bad': !detail.payTo }">
        <div>
          <span class="trs-pay-k">{{ tr('Amount to receive') }}</span>
          <strong>{{ formatAmount(detail.amountToReceive) }} {{ detail.currencyCode }}</strong>
          <small class="trs-muted">{{ (detail.weight * 100).toFixed(0) }}% {{ tr('of the net amount') }}</small>
        </div>
        <div>
          <span class="trs-pay-k">{{ tr('Paid into') }}</span>
          <strong v-if="payAccount">{{ payAccount.bankName }} · {{ payAccount.iban ?? payAccount.accountNumber }}</strong>
          <strong v-else>{{ tr('No payable bank account') }}</strong>
          <small v-if="!payAccount">{{ tr('Add and verify a bank account in Customer 360 before Execute.') }}</small>
        </div>
      </div>

      <section class="trs-sec">
        <h3><span class="fa fa-file-text-o" aria-hidden="true"></span>{{ detail.role === 'SELLER' && reverse ? tr('Bills to be paid') : tr('Invoices') }}
          <span class="trs-muted">({{ detail.invoices.length }})</span></h3>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead><tr>
              <th>{{ tr('Invoice No.') }}</th><th>{{ tr('Invoice Date') }}</th><th>{{ tr('Due Date') }}</th>
              <th class="num">{{ tr('Invoice Amount') }}</th><th class="num">{{ tr('Amount Financed') }}</th><th>{{ tr('Status') }}</th>
            </tr></thead>
            <tbody>
              <tr v-for="i in detail.invoices" :key="i.invoiceId" :class="{ 'trs-off': notEligible(i.statusCode) }">
                <td>{{ i.clientInvoiceNumber ?? i.invoiceNumber }}</td>
                <td>{{ formatDate(i.invoiceAt) }}</td>
                <td>{{ formatDate(i.dueAt) }}</td>
                <td class="num">{{ formatAmount(i.grossAmount) }}</td>
                <td class="num">{{ formatAmount(i.amount) }}</td>
                <td><span class="fx-chip" :class="notEligible(i.statusCode) ? 'fx-chip--bad' : 'fx-chip--ok'">{{ tr(statusLabel(i.statusCode)) }}</span>
                  <small v-if="i.ineligibleReason" class="trs-muted trs-reason">{{ i.ineligibleReason }}</small></td>
              </tr>
              <tr v-if="detail.invoices.length === 0"><td colspan="6" class="trs-muted">{{ tr('No invoices for this party in the disbursement.') }}</td></tr>
            </tbody>
            <tfoot v-if="detail.invoices.length">
              <tr><th colspan="4">{{ tr('Eligible total') }}</th><th class="num">{{ formatAmount(detail.eligibleTotal) }}</th><th></th></tr>
            </tfoot>
          </table>
        </div>
      </section>

      <section class="trs-sec">
        <h3><span class="fa fa-university" aria-hidden="true"></span>{{ tr('Bank accounts') }}</h3>
        <p v-if="detail.clientId === null" class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
          {{ tr('This tax number is not a customer yet. Add it in Customer 360 with its bank account.') }}</p>
        <div v-else class="fx-table-wrap">
          <table class="fx-table">
            <thead><tr><th>{{ tr('Bank') }}</th><th>{{ tr('Account No. / IBAN') }}</th><th>{{ tr('Holder') }}</th><th>{{ tr('Currency') }}</th><th>{{ tr('Status') }}</th></tr></thead>
            <tbody>
              <tr v-for="a in detail.bankAccounts" :key="a.bankAccountId">
                <td>{{ a.bankName }}<small v-if="a.branchName" class="trs-muted"> · {{ a.branchName }}</small>
                  <span v-if="a.isPrimary" class="fx-chip fx-chip--plain fx-chip--teal trs-primary">{{ tr('Primary') }}</span></td>
                <td>{{ a.accountNumber ?? '—' }}<small v-if="a.iban" class="trs-muted trs-reason">{{ a.iban }}</small></td>
                <td>{{ a.accountHolderName }}</td>
                <td>{{ a.currencyCode }}</td>
                <td><span class="fx-chip" :class="accountChip(a)">{{ tr(accountStatus(a)) }}</span></td>
              </tr>
              <tr v-if="detail.bankAccounts.length === 0"><td colspan="5" class="trs-muted">{{ tr('No bank account yet.') }}</td></tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>
    <template #footer>
      <button type="button" class="fx-btn" @click="visible = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
    </template>
  </Crm360Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { tr } from '@/shared/layout/language'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { trsDisburseApi, type TrsDisburseParty, type TrsDisbursePartyDetail } from '../api/trsDisburse.api'

const props = defineProps<{ disbursementId: number; party: TrsDisburseParty | null; reverse: boolean }>()
const visible = defineModel<boolean>('visible', { required: true })

type Account = TrsDisbursePartyDetail['bankAccounts'][number]
const detail = ref<TrsDisbursePartyDetail | null>(null)
const loading = ref(false)

const roleLabel = computed(() => {
  const r = detail.value?.role
  return props.reverse ? (r === 'DEBTOR' ? 'Buyer (debtor)' : 'Supplier') : r === 'DEBTOR' ? 'Debtor' : 'Seller'
})
const subtitle = computed(() => (detail.value ? tr(roleLabel.value) : ''))
const payAccount = computed(() => detail.value?.bankAccounts.find((a) => (a.iban ?? a.accountNumber) === detail.value?.payTo) ?? null)

const notEligible = (s: string | null) => (s ?? '').toUpperCase() === 'NOT_ELIGIBLE'
const statusLabel = (s: string | null) => (notEligible(s) ? 'Not eligible' : (s ?? '').toUpperCase() === 'APPROVED' ? 'Approved' : 'Eligible')
const accountStatus = (a: Account) =>
  a.statusCode !== 'VERIFIED' ? (a.statusCode === 'SUSPENDED' ? 'Suspended' : 'Not verified') : a.isPayable ? 'Can be paid' : 'Waiting'
const accountChip = (a: Account) => (a.isPayable ? 'fx-chip--ok' : a.statusCode === 'VERIFIED' ? 'fx-chip--warn' : 'fx-chip--bad')

watch(visible, async (open) => {
  if (!open || !props.party) return
  detail.value = null
  loading.value = true
  try {
    detail.value = await trsDisburseApi.party(props.disbursementId, { clientId: props.party.clientId, taxNumber: props.party.taxNumber })
  } catch (e) {
    showError(e instanceof Error ? e.message : String(e))
    visible.value = false
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
.trs-muted { color: var(--fin-muted); font-size: var(--fs-xs, 12px); }
.trs-reason { display: block; margin-block-start: 2px; }
.trs-pay { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 2fr); gap: 16px; margin: 16px 0 0; padding: 12px 14px; border-radius: 8px;
  background: var(--fin-teal-50); border-inline-start: 4px solid var(--fin-teal-600); }
.trs-pay > div { display: flex; flex-direction: column; gap: 3px; min-width: 0; }
.trs-pay strong { font-size: var(--fs-md, 14px); color: var(--fin-ink); overflow-wrap: anywhere; }
.trs-pay-k { font-size: var(--fs-xs, 12px); font-weight: 700; color: var(--fin-muted); }
.trs-pay--bad { background: #FDF0E5; border-inline-start-color: var(--fin-orange); }
.trs-sec { margin-block-start: 18px; }
.trs-sec h3 { display: flex; align-items: center; gap: 8px; margin: 0 0 8px; font-size: var(--fs-md, 14px); color: var(--fin-teal-700); }
.trs-sec h3 .fa { color: var(--fin-orange); }
.trs-off td { color: var(--fin-muted); }
.trs-primary { margin-inline-start: 6px; }
.num { text-align: end; font-variant-numeric: tabular-nums; }
@media (max-width: 600px) { .trs-pay { grid-template-columns: 1fr; } }
</style>
