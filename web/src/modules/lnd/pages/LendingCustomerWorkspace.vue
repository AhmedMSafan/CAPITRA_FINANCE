<template>
  <!-- Retail Lending > Lending Workspace > a customer (as the Factoring Customer Workspace): the customer card, the summary, a new
       loan with the customer filled in, and the Loans and Payments tabs. A loan opens its own page (schedule, payout, payments). -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'lnd-workspace' }">{{ tr('Lending Workspace') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ tr('Process Customer') }}</b>
        </nav>
        <h1>{{ tr('Lending Customer Workspace') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'lnd-workspace' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Lending Workspace') }}</RouterLink>
      </div>
    </div>

    <section class="fx-card fc-customer">
      <div class="fc-customer-main">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-user-o"></span></span>
        <div>
          <span class="fc-customer-no">{{ ws?.clientNumber || '—' }}</span>
          <b class="fc-customer-name">{{ ws?.clientName || (loading ? tr('Loading...') : '—') }}</b>
          <span class="fc-customer-ar">{{ ws?.clientNameAr || '' }}</span>
        </div>
      </div>
      <div class="fc-info"><span>{{ tr('Client Type') }}</span><span class="fx-chip fx-chip--teal fx-chip--plain">{{ ws?.clientType || '—' }}</span></div>
      <div class="fc-info"><span>{{ tr('National ID') }}</span><b>{{ ws?.nationalId || '—' }}</b></div>
      <div class="fc-info"><span>{{ tr('Relationship Manager') }}</span><b>{{ ws?.relationshipManager || '—' }}</b></div>
      <div class="fc-info"><span>{{ tr('Mobile') }}</span><b><span class="fa fa-phone" aria-hidden="true"></span> {{ ws?.mobileNumber || ws?.phoneNumber || '—' }}</b></div>
      <div class="fc-info"><span>{{ tr('Email') }}</span><b class="fc-email"><span class="fa fa-envelope-o" aria-hidden="true"></span> {{ ws?.email || '—' }}</b></div>
    </section>

    <section v-if="ws" class="lw-kpis">
      <div class="fx-card lw-kpi"><small>{{ tr('Active loans') }}</small><b>{{ ws.summary.activeLoans }}</b></div>
      <div class="fx-card lw-kpi"><small>{{ tr('Principal outstanding') }}</small><b>{{ money(ws.summary.outstandingPrincipal) }}</b></div>
      <div class="fx-card lw-kpi" :class="{ 'lw-kpi--bad': ws.summary.overdueAmount > 0 }"><small>{{ tr('Overdue today') }}</small><b>{{ money(ws.summary.overdueAmount) }}</b></div>
      <div class="fx-card lw-kpi" :class="{ 'lw-kpi--bad': ws.summary.maxDaysLate > 0 }"><small>{{ tr('Days late') }}</small><b>{{ ws.summary.maxDaysLate }}</b></div>
      <div class="fx-card lw-kpi"><small>{{ tr('Customer credit balance') }}</small><b>{{ money(ws.summary.creditBalance) }}</b></div>
    </section>

    <section v-if="canEdit" class="fc-quick fc-quick--3">
      <RouterLink :to="{ name: 'lnd-loans', query: { new: '1', clientId: String(clientId), client: ws ? `${ws.clientNumber} · ${ws.clientName}` : '' } }" class="fx-card fc-quick-card fc-quick-card--calc">
        <span class="fc-quick-ic" aria-hidden="true"><span class="fa fa-plus-circle"></span></span>
        <span class="fc-quick-copy"><b>{{ tr('New Loan') }}</b><small>{{ tr('A microfinance or consumer finance loan for this customer, with its schedule') }}</small></span>
        <span class="fa fa-arrow-right" aria-hidden="true"></span>
      </RouterLink>
    </section>

    <section class="fx-card">
      <div class="fc-tabs" role="tablist">
        <button v-for="t in tabs" :key="t.key" type="button" role="tab" class="fc-tab" :aria-selected="tab === t.key" @click="tab = t.key">
          <span class="fa" :class="t.icon" aria-hidden="true"></span>{{ tr(t.label) }}<em>{{ t.count }}</em>
        </button>
      </div>

      <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>

      <div v-else-if="tab === 'loans'" class="fx-table-wrap">
        <table v-if="ws?.loans.length" class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Loan') }}</th><th>{{ tr('Product') }}</th><th>{{ tr('Status') }}</th><th class="fx-r">{{ tr('Principal') }}</th>
              <th class="fx-r">{{ tr('Outstanding') }}</th><th class="fx-r">{{ tr('Overdue') }}</th><th class="fx-r">{{ tr('Days late') }}</th>
              <th>{{ tr('Next due') }}</th><th>{{ tr('Paid out') }}</th><th>{{ tr('Maturity') }}</th><th class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="l in ws.loans" :key="l.loanId">
              <td><RouterLink :to="{ name: 'lnd-loan', params: { loanId: l.loanId } }"><b>{{ l.loanNumber }}</b></RouterLink></td>
              <td>{{ l.productName }}<br><small class="fc-muted">{{ l.productCode }} · {{ l.licenceCode }}</small></td>
              <td>
                <span class="fx-chip" :class="CHIP[l.statusCode] ?? 'fx-chip--plain'">{{ tr(STATUS[l.statusCode] ?? l.statusCode) }}</span>
                <span v-if="l.isNonAccrual" class="fx-chip fx-chip--bad">{{ tr('Non-accrual') }}</span>
              </td>
              <td class="fx-r">{{ money(l.principalAmount) }}</td>
              <td class="fx-r">{{ money(l.principalOutstanding) }}</td>
              <td class="fx-r" :class="{ 'lw-bad': l.overdueAmount > 0 }">{{ money(l.overdueAmount) }}</td>
              <td class="fx-r" :class="{ 'lw-bad': l.daysLate > 0 }">{{ l.daysLate }}</td>
              <td>{{ l.nextDueDate ? `${formatDate(l.nextDueDate)} · ${money(l.nextDueAmount)}` : '—' }}</td>
              <td>{{ formatDate(l.disbursementDate) }}</td>
              <td>{{ formatDate(l.maturityDate) }}</td>
              <td class="fx-r">
                <RouterLink class="fx-btn fx-btn--teal fx-btn--sm" :to="{ name: 'lnd-loan', params: { loanId: l.loanId } }">
                  <span class="fa fa-play-circle-o" aria-hidden="true"></span>{{ tr('Open') }}
                </RouterLink>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-else class="fx-empty"><span class="fa fa-folder-open-o" aria-hidden="true"></span>{{ tr('No loans for this customer yet.') }}</div>
      </div>

      <div v-else class="fx-table-wrap">
        <table v-if="ws?.payments.length" class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Receipt') }}</th><th>{{ tr('Loan') }}</th><th>{{ tr('Kind') }}</th><th>{{ tr('Value date') }}</th><th>{{ tr('Method') }}</th>
              <th class="fx-r">{{ tr('Received') }}</th><th class="fx-r">{{ tr('Applied') }}</th><th>{{ tr('Status') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="p in ws.payments" :key="p.repaymentTxnId">
              <td><b>{{ p.repaymentNumber }}</b></td>
              <td>{{ p.loanNumber }}</td>
              <td>{{ tr(KIND[p.kindCode] ?? p.kindCode) }}</td>
              <td>{{ formatDate(p.valueDate) }}</td>
              <td>{{ tr(METHOD[p.method] ?? p.method) }}</td>
              <td class="fx-r">{{ money(p.receiptAmount) }}</td>
              <td class="fx-r">{{ money(p.allocatedAmount) }}</td>
              <td><span class="fx-chip" :class="p.statusCode === 'REVERSED' ? 'fx-chip--bad' : 'fx-chip--ok'">{{ tr(p.statusCode === 'REVERSED' ? 'Reversed' : 'Posted') }}</span></td>
            </tr>
          </tbody>
        </table>
        <div v-else class="fx-empty"><span class="fa fa-money" aria-hidden="true"></span>{{ tr('No payments yet.') }}</div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { lendingWsApi, type LwCustomerWorkspace } from '../api/workspace.api'

const props = defineProps<{ clientId: number }>()

const STATUS: Record<string, string> = { APPROVED: 'Approved', ACTIVE: 'Active', CLOSED: 'Closed', WRITTEN_OFF: 'Written off' }
const CHIP: Record<string, string> = { APPROVED: 'fx-chip--warn', ACTIVE: 'fx-chip--ok', CLOSED: 'fx-chip--teal', WRITTEN_OFF: 'fx-chip--bad' }
const KIND: Record<string, string> = { RECEIPT: 'Payment', RECOVERY: 'Recovery', PREPAYMENT: 'Part prepayment', SETTLEMENT: 'Early settlement' }
const METHOD: Record<string, string> = { BANK_DEPOSIT: 'Bank deposit', CHEQUE: 'Cheque', CREDIT_BALANCE: 'Credit balance', MIXED: 'Mixed' }
const egp = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const money = (n: number) => egp.format(n)

const canEdit = hasPermission('LND.LOAN.EDIT')
const ws = ref<LwCustomerWorkspace | null>(null)
const loading = ref(false)
const tab = ref<'loans' | 'payments'>('loans')
const tabs = computed(() => [
  { key: 'loans' as const, label: 'Loans', count: ws.value?.loans.length ?? 0, icon: 'fa-list' },
  { key: 'payments' as const, label: 'Payments', count: ws.value?.payments.length ?? 0, icon: 'fa-money' }
])

onMounted(async () => {
  loading.value = true
  try {
    ws.value = await lendingWsApi.customer(props.clientId)
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.lw-kpis { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: 12px; margin-bottom: 14px; }
.lw-kpi { display: flex; flex-direction: column; gap: 4px; padding: 12px 14px; }
.lw-kpi small { color: var(--fin-muted); font-size: 12px; }
.lw-kpi b { font-size: 18px; }
.lw-kpi--bad b, .lw-bad { color: var(--fin-danger, #b42318); }
@media (max-width: 900px) { .lw-kpis { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
