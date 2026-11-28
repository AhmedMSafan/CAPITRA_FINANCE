<template>
  <!-- All finances (Ahmed, 28 Nov 2026): opened from the Lending Workspace only; searched by finance no., customer, amount and status.
       A new finance starts from a customer's workspace. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'lnd-workspace' }">{{ tr('Lending Workspace') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ tr('All finances') }}</b>
        </nav>
        <h1>{{ tr('All finances') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'lnd-workspace' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Lending Workspace') }}</RouterLink>
      </div>
    </div>

    <section class="fx-card">
      <div class="ln-filter">
        <label class="ln-f"><span>{{ tr('Finance No') }}</span><input v-model="f.number" class="fx-search"></label>
        <label class="ln-f"><span>{{ tr('Customer Name') }}</span><input v-model="f.name" class="fx-search"></label>
        <label class="ln-f"><span>{{ tr('Customer Number') }}</span><input v-model="f.clientNumber" class="fx-search"></label>
        <label class="ln-f"><span>{{ tr('Amount from') }}</span><input v-model.number="f.from" type="number" min="0" step="any" class="fx-search"></label>
        <label class="ln-f"><span>{{ tr('Amount to') }}</span><input v-model.number="f.to" type="number" min="0" step="any" class="fx-search"></label>
        <label class="ln-f"><span>{{ tr('Status') }}</span><select v-model="f.status" class="fx-select"><option value="">{{ tr('All statuses') }}</option><option v-for="s in Object.keys(STATUS)" :key="s" :value="s">{{ tr(STATUS[s]!) }}</option></select></label>
        <button type="button" class="fx-btn ln-clear" @click="clear"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Clear') }}</button>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr><th>{{ tr('Finance') }}</th><th>{{ tr('Customer') }}</th><th>{{ tr('Product') }}</th><th>{{ tr('Status') }}</th><th class="ln-num">{{ tr('Amount') }}</th><th>{{ tr('Term') }}</th><th class="ln-num">{{ tr('Instalment') }}</th><th>{{ tr('Disbursement') }}</th><th>{{ tr('Maturity') }}</th></tr>
          </thead>
          <tbody>
            <tr v-for="l in rows" :key="l.loanId" class="ln-row" @click="router.push({ name: 'lnd-loan', params: { loanId: l.loanId } })">
              <td><b>{{ l.loanNumber }}</b></td>
              <td>{{ l.clientName }} <small class="c3-muted">{{ l.clientNumber }}</small></td>
              <td>{{ l.productName }} <small class="c3-muted">{{ l.licenceCode }}</small></td>
              <td><span class="fx-chip" :class="CHIP[l.statusCode] ?? 'fx-chip--teal'">{{ tr(STATUS[l.statusCode] ?? l.statusCode) }}</span></td>
              <td class="ln-num">{{ money(l.principalAmount) }}</td>
              <td>{{ l.termMonths }} {{ tr('months') }}</td>
              <td class="ln-num">{{ money(l.installmentAmount) }}</td>
              <td>{{ formatDate(l.disbursementDate) }}</td>
              <td>{{ formatDate(l.maturityDate) }}</td>
            </tr>
          </tbody>
        </table>
        <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading finances...') }}</div>
        <div v-else-if="!rows.length" class="fx-empty"><span class="fa fa-folder-open-o" aria-hidden="true"></span>{{ tr('No finances yet.') }}</div>
      </div>
    </section>

  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { loansApi, type LoanListItem } from '../api/loans.api'

const STATUS: Record<string, string> = { IN_APPROVAL: 'In approval', REJECTED: 'Rejected', APPROVED: 'Approved', ACTIVE: 'Active', CLOSED: 'Closed', CANCELLED: 'Cancelled', WRITTEN_OFF: 'Written off' }
const CHIP: Record<string, string> = { IN_APPROVAL: 'fx-chip--warn', REJECTED: 'fx-chip--bad', APPROVED: 'fx-chip--warn', ACTIVE: 'fx-chip--ok', CLOSED: 'fx-chip--teal', CANCELLED: 'fx-chip--bad', WRITTEN_OFF: 'fx-chip--bad' }
const egp = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const money = (n: number) => egp.format(n)

const router = useRouter()
const loans = ref<LoanListItem[]>([])
const loading = ref(false)
const f = reactive({ number: '', name: '', clientNumber: '', from: '' as number | '', to: '' as number | '', status: '' })
const has = (text: string, q: string) => !q.trim() || text.toLowerCase().includes(q.trim().toLowerCase())
const rows = computed(() => loans.value.filter((l) => has(l.loanNumber, f.number) && has(l.clientName, f.name) && has(l.clientNumber ?? '', f.clientNumber)
  && (f.from === '' || l.principalAmount >= Number(f.from)) && (f.to === '' || l.principalAmount <= Number(f.to))
  && (!f.status || l.statusCode === f.status)))
function clear() {
  Object.assign(f, { number: '', name: '', clientNumber: '', from: '', to: '', status: '' })
}

async function load() {
  loading.value = true
  try {
    loans.value = await loansApi.list()
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  } finally {
    loading.value = false
  }
}
onMounted(load)
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
.ln-filter { display: grid; grid-template-columns: repeat(6, minmax(0, 1fr)) auto; gap: 10px; padding: 12px 4px; align-items: end; }
.ln-f { display: flex; flex-direction: column; gap: 4px; font-size: 12.5px; font-weight: 600; color: #34495a; }
.ln-f .fx-search, .ln-f .fx-select { width: 100%; }
@media (max-width: 1100px) { .ln-filter { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
.ln-row { cursor: pointer; }
.ln-num { text-align: end; white-space: nowrap; }
.c3-grid--3 { grid-template-columns: repeat(3, minmax(0, 1fr)); }
.ln-preview { margin-top: 14px; display: flex; flex-direction: column; gap: 10px; }
.ln-facts { display: grid; grid-template-columns: repeat(6, minmax(0, 1fr)); gap: 8px; }
.ln-facts > div { display: flex; flex-direction: column; gap: 2px; padding: 8px 10px; border-radius: var(--fin-radius-sm); background: var(--fin-teal-50); }
.ln-facts small { color: var(--fin-muted); }
.ln-schedule { max-height: 320px; overflow: auto; }
@media (max-width: 900px) {
  .c3-grid--3 { grid-template-columns: 1fr; }
  .ln-facts { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
</style>
