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
import { computed, onMounted, ref } from 'vue'
import { ApiError } from '@/shared/api/http'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { loansApi, type Installment, type Loan } from '../api/loans.api'

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
    ['Late fee', fee]
  ]
})

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
@media (max-width: 900px) { .ln-facts { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
