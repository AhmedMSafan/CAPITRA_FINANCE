<template>
  <!-- Retail Lending > Loans: every loan, and New Loan: a customer, a lending product with terms in force, amount and term. The
       schedule preview shows the instalments before the loan is created. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ tr('Retail Lending') }}</b>
        </nav>
        <h1>{{ tr('Loans') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'lnd-parameters' }" class="fx-btn"><span class="fa fa-sliders" aria-hidden="true"></span>{{ tr('Lending Parameters') }}</RouterLink>
        <button v-if="canEdit" type="button" class="fx-btn fx-btn--teal" @click="openNew"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New loan') }}</button>
      </div>
    </div>

    <section class="fx-card">
      <div class="ln-filter">
        <input v-model="search" class="fx-search" :placeholder="tr('Search by loan number, customer or product')">
        <select v-model="status" class="fx-select"><option value="">{{ tr('All statuses') }}</option><option v-for="s in Object.keys(STATUS)" :key="s" :value="s">{{ tr(STATUS[s]!) }}</option></select>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr><th>{{ tr('Loan') }}</th><th>{{ tr('Customer') }}</th><th>{{ tr('Product') }}</th><th>{{ tr('Status') }}</th><th class="ln-num">{{ tr('Amount') }}</th><th>{{ tr('Term') }}</th><th class="ln-num">{{ tr('Instalment') }}</th><th>{{ tr('Disbursement') }}</th><th>{{ tr('Maturity') }}</th></tr>
          </thead>
          <tbody>
            <tr v-for="l in rows" :key="l.loanId" class="ln-row" @click="router.push({ name: 'lnd-loan', params: { loanId: l.loanId } })">
              <td><b>{{ l.loanNumber }}</b></td>
              <td>{{ l.clientName }}</td>
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
        <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading loans...') }}</div>
        <div v-else-if="!rows.length" class="fx-empty"><span class="fa fa-folder-open-o" aria-hidden="true"></span>{{ tr('No loans yet.') }}</div>
      </div>
    </section>

    <Crm360Dialog v-model:visible="creating" :title="tr('New Loan')" icon="fa-plus-circle" :busy="busy" :width="980">
      <div class="c3-grid c3-grid--3">
        <label class="fc-field" :class="bad('clientId')"><span>{{ tr('Customer') }}<b> *</b></span>
          <input v-model="clientSearch" class="fx-search" :placeholder="tr('Type a name, number or national ID')" @input="findClients">
          <select v-model="form.clientId" class="fx-select" :size="clients.length > 1 ? Math.min(clients.length, 5) : undefined">
            <option v-for="c in clients" :key="c.id" :value="c.id">{{ c.name }} · {{ c.code }}</option>
          </select>
        </label>
        <label class="fc-field" :class="bad('productId')"><span>{{ tr('Product') }}<b> *</b></span>
          <select v-model="form.productId" class="fx-select"><option v-for="p in products" :key="p.id" :value="p.id">{{ p.name }} · {{ p.code }}</option></select>
          <small v-if="!products.length" class="c3-muted">{{ tr('No lending product has approved terms in force.') }}</small>
        </label>
        <label class="fc-field"><span>{{ tr('Notes') }}</span><input v-model="form.notes" class="fx-search" maxlength="2000"></label>
        <label class="fc-field" :class="bad('principalAmount')"><span>{{ tr('Amount') }} ({{ tr('EGP') }})<b> *</b></span><input v-model.number="form.principalAmount" type="number" min="0" step="any" class="fx-search"></label>
        <label class="fc-field" :class="bad('termMonths')"><span>{{ tr('Term') }} ({{ tr('months') }})<b> *</b></span><input v-model.number="form.termMonths" type="number" min="1" class="fx-search"></label>
        <label class="fc-field" :class="bad('disbursementDate')"><span>{{ tr('Disbursement date') }}</span><input v-model="form.disbursementDate" type="date" class="fx-search"></label>
        <label class="fc-field" :class="bad('firstDueDate')"><span>{{ tr('First instalment') }}</span><input v-model="form.firstDueDate" type="date" class="fx-search"><small class="c3-muted">{{ tr('Blank: one period after the disbursement.') }}</small></label>
      </div>

      <div v-if="preview" class="ln-preview">
        <div class="ln-facts">
          <div><small>{{ tr('Instalments') }}</small><b>{{ preview.installmentCount }} × {{ money(preview.installmentAmount) }}</b></div>
          <div><small>{{ tr('Yearly rate') }}</small><b>{{ preview.annualRatePct }}% · {{ tr(preview.interestMethodCode === 'FLAT' ? 'Flat' : 'Declining balance') }}</b></div>
          <div><small>{{ tr('Total interest') }}</small><b>{{ money(preview.totalInterestAmount) }}</b></div>
          <div><small>{{ tr('Total to repay') }}</small><b>{{ money(preview.totalRepayableAmount) }}</b></div>
          <div><small>{{ tr('Maturity') }}</small><b>{{ formatDate(preview.maturityDate) }}</b></div>
          <div><small>{{ tr('Product version') }}</small><b>{{ preview.versionNo }}</b></div>
        </div>
        <div class="fx-table-wrap ln-schedule">
          <table class="fx-table">
            <thead><tr><th>#</th><th>{{ tr('Due date') }}</th><th class="ln-num">{{ tr('Principal') }}</th><th class="ln-num">{{ tr('Interest') }}</th><th class="ln-num">{{ tr('Instalment') }}</th><th class="ln-num">{{ tr('Balance after') }}</th></tr></thead>
            <tbody>
              <tr v-for="s in preview.schedule" :key="s.installmentNo">
                <td>{{ s.installmentNo }}</td><td>{{ formatDate(s.dueDate) }}</td><td class="ln-num">{{ money(s.principal) }}</td>
                <td class="ln-num">{{ money(s.interest) }}</td><td class="ln-num"><b>{{ money(s.total) }}</b></td><td class="ln-num">{{ money(s.closingPrincipal) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="creating = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn" :disabled="busy" @click="showPreview"><span class="fa fa-table" aria-hidden="true"></span>{{ tr('Show schedule') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !preview" @click="create"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr('Create loan') }}</button>
      </template>
    </Crm360Dialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { ApiError } from '@/shared/api/http'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate, todayInCairo } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { loansApi, type LoanListItem, type LoanOption, type LoanPreview, type LoanRequest } from '../api/loans.api'

const STATUS: Record<string, string> = { APPROVED: 'Approved', ACTIVE: 'Active', CLOSED: 'Closed', CANCELLED: 'Cancelled' }
const CHIP: Record<string, string> = { APPROVED: 'fx-chip--warn', ACTIVE: 'fx-chip--ok', CLOSED: 'fx-chip--teal', CANCELLED: 'fx-chip--bad' }
const egp = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const money = (n: number) => egp.format(n)

const router = useRouter()
const canEdit = hasPermission('LND.LOAN.EDIT')
const loans = ref<LoanListItem[]>([])
const loading = ref(false)
const search = ref('')
const status = ref('')
const rows = computed(() => {
  const q = search.value.trim().toLowerCase()
  return loans.value.filter((l) => (!status.value || l.statusCode === status.value)
    && (!q || `${l.loanNumber} ${l.clientName} ${l.productName} ${l.productCode}`.toLowerCase().includes(q)))
})

// ---- New Loan
const creating = ref(false)
const busy = ref(false)
const products = ref<LoanOption[]>([])
const clients = ref<LoanOption[]>([])
const clientSearch = ref('')
const preview = ref<LoanPreview | null>(null)
const errors = ref<Record<string, string[]>>({})
const bad = (f: string) => ({ 'c3-bad': !!errors.value[f] })
const blank = (): LoanRequest => ({ clientId: null, productId: null, principalAmount: null, termMonths: null, disbursementDate: todayInCairo(), firstDueDate: null, notes: null })
const form = reactive<LoanRequest>(blank())
// any change of the request makes the shown schedule stale
watch(() => ({ ...form, notes: null }), () => (preview.value = null), { deep: true })

async function openNew() {
  Object.assign(form, blank())
  preview.value = null
  errors.value = {}
  clientSearch.value = ''
  clients.value = []
  creating.value = true
  try {
    products.value = await loansApi.products()
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  }
}

let timer: ReturnType<typeof setTimeout> | undefined
function findClients() {
  clearTimeout(timer)
  timer = setTimeout(async () => {
    try {
      clients.value = await loansApi.clients(clientSearch.value)
      if (clients.value.length === 1) form.clientId = clients.value[0]!.id
    } catch (err) {
      showError(err instanceof Error ? err.message : String(err))
    }
  }, 300)
}

const body = (): LoanRequest => ({ ...form, firstDueDate: form.firstDueDate || null, disbursementDate: form.disbursementDate || null, notes: form.notes || null })

async function call<T>(action: () => Promise<T>): Promise<T | null> {
  busy.value = true
  errors.value = {}
  try {
    return await action()
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) {
      errors.value = err.problem.errors
      showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    } else showError(err instanceof Error ? err.message : String(err))
    return null
  } finally {
    busy.value = false
  }
}

async function showPreview() {
  const p = await call(() => loansApi.preview(body()))
  if (p) preview.value = p
}

async function create() {
  const loan = await call(() => loansApi.create(body()))
  if (!loan) return
  creating.value = false
  showSuccess(`${tr('Loan created:')} ${loan.loanNumber}`)
  router.push({ name: 'lnd-loan', params: { loanId: loan.loanId } })
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
.ln-filter { display: flex; gap: 10px; padding: 12px 4px; flex-wrap: wrap; }
.ln-filter .fx-search { flex: 1; min-width: 220px; }
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
