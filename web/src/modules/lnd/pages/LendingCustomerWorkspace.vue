<template>
  <!-- Retail Lending > Lending Workspace > a customer: one compact card (the customer and its figures), New Finance on the title
       line, and the Finances, Claims and Payments tabs (the same tabs as the Factoring Customer Workspace). A finance opens its own page. -->
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
        <RouterLink v-if="canEdit" :to="{ name: 'lnd-finance-new', query: { clientId: String(clientId) } }" class="fx-btn fx-btn--teal">
          <span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New Finance') }}
        </RouterLink>
        <RouterLink :to="{ name: 'lnd-workspace' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Lending Workspace') }}</RouterLink>
      </div>
    </div>

    <!-- one compact card (Ahmed, 26 Nov 2026): who the customer is on one line, its figures on the next; New Finance is on the title line -->
    <section class="fx-card lw-card">
      <div class="lw-who">
        <span class="lw-ic" aria-hidden="true"><span class="fa fa-user-o"></span></span>
        <span class="lw-no" dir="ltr">{{ ws?.clientNumber || '—' }}</span>
        <b class="lw-name">{{ ws?.clientName || (loading ? tr('Loading...') : '—') }}</b>
        <span v-if="ws?.clientNameAr" class="lw-ar" dir="auto">{{ ws.clientNameAr }}</span>
        <span v-if="ws?.clientType" class="lw-chip">{{ ws.clientType }}</span>
        <span class="lw-facts">
          <span>{{ tr('National ID') }} <b>{{ ws?.nationalId || '—' }}</b></span>
          <span>{{ tr('Relationship Manager') }} <b>{{ ws?.relationshipManager || '—' }}</b></span>
          <span><span class="fa fa-phone" aria-hidden="true"></span> <b dir="ltr">{{ ws?.mobileNumber || ws?.phoneNumber || '—' }}</b></span>
          <span><span class="fa fa-envelope-o" aria-hidden="true"></span> <b>{{ ws?.email || '—' }}</b></span>
        </span>
      </div>
      <div v-if="ws" class="lw-kpis">
        <div class="lw-kpi"><small>{{ tr('Active finances') }}</small><b>{{ ws.summary.activeLoans }}</b></div>
        <div class="lw-kpi"><small>{{ tr('Principal outstanding') }}</small><b>{{ money(ws.summary.outstandingPrincipal) }}</b></div>
        <div class="lw-kpi" :class="{ 'lw-kpi--bad': ws.summary.overdueAmount > 0 }"><small>{{ tr('Overdue today') }}</small><b>{{ money(ws.summary.overdueAmount) }}</b></div>
        <div class="lw-kpi" :class="{ 'lw-kpi--bad': ws.summary.maxDaysLate > 0 }"><small>{{ tr('Days late') }}</small><b>{{ ws.summary.maxDaysLate }}</b></div>
        <div class="lw-kpi"><small>{{ tr('Customer credit balance') }}</small><b>{{ money(ws.summary.creditBalance) }}</b></div>
      </div>
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
              <th>{{ tr('Finance') }}</th><th>{{ tr('Product') }}</th><th>{{ tr('Status') }}</th><th class="fx-r">{{ tr('Principal') }}</th>
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
                <small v-if="l.stepName" class="fc-muted"> · {{ tr(l.stepName) }}</small>
                <span v-if="l.isNonAccrual" class="fx-chip fx-chip--bad">{{ tr('Non-accrual') }}</span>
              </td>
              <td class="fx-r">{{ money(l.principalAmount) }}</td>
              <td class="fx-r">{{ money(l.principalOutstanding) }}</td>
              <td class="fx-r" :class="{ 'lw-bad': l.overdueAmount > 0 }">{{ money(l.overdueAmount) }}</td>
              <td class="fx-r" :class="{ 'lw-bad': l.daysLate > 0 }">{{ l.daysLate }}</td>
              <td>{{ l.nextDueDate ? `${formatDate(l.nextDueDate)} · ${money(l.nextDueAmount)}` : '—' }}</td>
              <!-- not paid out yet before the finance is booked -->
              <td>{{ booked(l.statusCode) ? formatDate(l.disbursementDate) : '—' }}</td>
              <td>{{ booked(l.statusCode) ? formatDate(l.maturityDate) : '—' }}</td>
              <td class="fx-r">
                <RouterLink class="fx-btn fx-btn--teal fx-btn--sm" :to="{ name: 'lnd-loan', params: { loanId: l.loanId } }">
                  <span class="fa fa-play-circle-o" aria-hidden="true"></span>{{ tr('Open') }}
                </RouterLink>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-else class="fx-empty"><span class="fa fa-folder-open-o" aria-hidden="true"></span>{{ tr('No finances for this customer yet.') }}</div>
      </div>

      <!-- the upfront claims of consumer finances (created when Credit approves), shown like the factoring claims; Open shows the claim -->
      <div v-else-if="tab === 'claims'" class="fx-table-wrap">
        <table v-if="ws?.claims.length" class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Finance') }}</th><th>{{ tr('Date') }}</th><th>{{ tr('Currency') }}</th><th class="fx-r">{{ tr('Upfront charges') }}</th>
              <th class="fx-r">{{ tr('Down payment') }}</th><th class="fx-r">{{ tr('Claim Total') }}</th><th class="fx-r">{{ tr('Paid') }}</th>
              <th class="fx-r">{{ tr('Outstanding') }}</th><th>{{ tr('Status') }}</th><th class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in ws.claims" :key="c.loanId">
              <td><RouterLink :to="{ name: 'lnd-loan', params: { loanId: c.loanId } }"><b>{{ c.loanNumber }}</b></RouterLink></td>
              <td>{{ c.claimDate ? formatDate(c.claimDate) : '—' }}</td>
              <td>{{ c.currencyCode }}</td>
              <td class="fx-r">{{ money(c.chargesAmount) }}</td>
              <td class="fx-r">{{ money(c.downPaymentAmount) }}</td>
              <td class="fx-r">{{ money(c.totalAmount) }}</td>
              <td class="fx-r">{{ money(c.paidAmount) }}</td>
              <td class="fx-r"><b>{{ money(c.outstandingAmount) }}</b></td>
              <td><span class="fx-chip" :class="claimTone(c.statusCode)">{{ tr(CLAIM_LABELS[c.statusCode] ?? c.statusCode) }}</span></td>
              <td class="fx-r">
                <button type="button" class="fx-btn fx-btn--sm" @click="openClaim(c.loanId)">
                  <span class="fa fa-flag-o" aria-hidden="true"></span>{{ tr('Open') }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-else class="fx-empty"><span class="fa fa-flag-o" aria-hidden="true"></span>{{ tr('Claims data will be available here.') }}</div>
      </div>

      <div v-else class="fx-table-wrap">
        <table v-if="ws?.payments.length" class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Receipt') }}</th><th>{{ tr('Finance') }}</th><th>{{ tr('Kind') }}</th><th>{{ tr('Value date') }}</th><th>{{ tr('Method') }}</th>
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
    <CfClaimDialog v-model:visible="claimShown" :loan-id="claimLoanId" />
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
import CfClaimDialog from '../components/CfClaimDialog.vue'
import { CLAIM_LABELS, claimTone } from '../api/loans.api'
import { lendingWsApi, type LwCustomerWorkspace } from '../api/workspace.api'

const props = defineProps<{ clientId: number }>()

const STATUS: Record<string, string> = { IN_APPROVAL: 'In approval', REJECTED: 'Rejected', APPROVED: 'Approved', ACTIVE: 'Active', CLOSED: 'Closed', WRITTEN_OFF: 'Written off' }
const CHIP: Record<string, string> = { IN_APPROVAL: 'fx-chip--warn', REJECTED: 'fx-chip--bad', APPROVED: 'fx-chip--warn', ACTIVE: 'fx-chip--ok', CLOSED: 'fx-chip--teal', WRITTEN_OFF: 'fx-chip--bad' }
const KIND: Record<string, string> = { RECEIPT: 'Payment', RECOVERY: 'Recovery', PREPAYMENT: 'Part prepayment', SETTLEMENT: 'Early settlement' }
const METHOD: Record<string, string> = { BANK_DEPOSIT: 'Bank deposit', CHEQUE: 'Cheque', CREDIT_BALANCE: 'Credit balance', MIXED: 'Mixed' }
const egp = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const money = (n: number) => egp.format(n)

const canEdit = hasPermission('LND.LOAN.EDIT')
const ws = ref<LwCustomerWorkspace | null>(null)
const loading = ref(false)
const claimShown = ref(false)
const claimLoanId = ref<number | null>(null)
function openClaim(loanId: number) {
  claimLoanId.value = loanId
  claimShown.value = true
}
const booked = (status: string) => ['ACTIVE', 'CLOSED', 'WRITTEN_OFF'].includes(status)
const tab = ref<'loans' | 'claims' | 'payments'>('loans')
const tabs = computed(() => [
  { key: 'loans' as const, label: 'Finances', count: ws.value?.loans.length ?? 0, icon: 'fa-list' },
  { key: 'claims' as const, label: 'Claims', count: ws.value?.claims.length ?? 0, icon: 'fa-flag-o' },
  { key: 'payments' as const, label: 'Payments', count: ws.value?.payments.length ?? 0, icon: 'fa-money' }
])

async function reload() {
  loading.value = true
  try {
    ws.value = await lendingWsApi.customer(props.clientId)
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  } finally {
    loading.value = false
  }
}
onMounted(reload)
</script>

<style scoped>
.lw-card { padding: 0; margin-bottom: 14px; }
.lw-who { display: flex; flex-wrap: wrap; align-items: center; gap: 6px 12px; padding: 12px 16px; border-bottom: 1px solid var(--fin-line); }
.lw-ic { display: grid; place-items: center; width: 36px; height: 36px; border-radius: 10px; background: #fdf1e6; color: #c46a1c; }
.lw-no { font-size: 12px; font-weight: 700; color: var(--fin-orange-700, #b5651d); }
.lw-name { font-size: 18px; }
.lw-ar { color: var(--fin-muted); font-size: 13px; }
.lw-chip { padding: 3px 9px; border-radius: 999px; background: #f2f4f6; color: #5b6b78; font-size: 11px; font-weight: 700; }
.lw-facts { display: flex; flex-wrap: wrap; gap: 4px 18px; margin-inline-start: auto; font-size: 12.5px; color: var(--fin-muted); }
.lw-facts b { color: var(--fin-ink); font-weight: 600; }
.lw-kpis { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); }
.lw-kpi { display: flex; flex-direction: column; gap: 2px; padding: 10px 16px; border-inline-end: 1px solid var(--fin-line); }
.lw-kpi:last-child { border-inline-end: 0; }
.lw-kpi small { color: var(--fin-muted); font-size: 12px; }
.lw-kpi b { font-size: 17px; }
.lw-kpi--bad b, .lw-bad { color: var(--fin-danger, #b42318); }
@media (max-width: 900px) {
  .lw-kpis { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .lw-kpi { border-inline-end: 0; border-bottom: 1px solid var(--fin-line); }
  .lw-facts { margin-inline-start: 0; }
}
</style>
