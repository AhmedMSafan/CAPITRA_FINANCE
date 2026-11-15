<template>
  <!-- Retail Lending > New Finance (consumer finance; Ahmed, 15 Nov 2026): a full page. From a customer, the customer comes filled in
       and fixed. The Product is what is financed (the goods categories). The amount, dates, rate and schedule come from one of the
       user's saved Finance Calculator scenarios, which must pay principal and interest together on each date. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'lnd-workspace' }">{{ tr('Lending Workspace') }}</RouterLink><span aria-hidden="true">›</span>
          <template v-if="fixedClientId">
            <RouterLink :to="{ name: 'lnd-customer', params: { clientId: fixedClientId } }">{{ clientLabel || tr('Customer') }}</RouterLink><span aria-hidden="true">›</span>
          </template>
          <b>{{ tr('New Finance') }}</b>
        </nav>
        <h1>{{ tr('New Finance') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink :to="back" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back') }}</RouterLink>
      </div>
    </div>

    <section class="fx-card nf-card">
      <h2 class="nf-title"><span class="fa fa-user" aria-hidden="true"></span>{{ tr('Customer and product') }}</h2>
      <div class="nf-grid">
        <label class="fc-field" :class="bad('clientId')"><span>{{ tr('Customer') }}<b> *</b></span>
          <input v-if="fixedClientId" :value="clientLabel" class="fx-search" readonly>
          <template v-else>
            <input v-model="clientSearch" class="fx-search" :placeholder="tr('Type a name, number or national ID')" @input="findClients">
            <select v-model="form.clientId" class="fx-select" :size="clients.length > 1 ? Math.min(clients.length, 5) : undefined">
              <option v-for="c in clients" :key="c.id" :value="c.id">{{ c.name }} · {{ c.code }}</option>
            </select>
          </template>
        </label>
        <label class="fc-field" :class="bad('goodsCategoryCode')"><span>{{ tr('Product') }}<b> *</b></span>
          <select v-model="form.goodsCategoryCode" class="fx-select">
            <option :value="null" disabled>{{ tr('Choose the product') }}</option>
            <option v-for="g in goods" :key="g.code" :value="g.code">{{ tr(g.name) }}</option>
          </select>
        </label>
        <label class="fc-field"><span>{{ tr('Notes') }}</span><input v-model="form.notes" class="fx-search" maxlength="2000"></label>
      </div>
    </section>

    <section class="fx-card nf-card">
      <h2 class="nf-title"><span class="fa fa-shopping-cart" aria-hidden="true"></span>{{ tr('Financed item') }}</h2>
      <div class="nf-grid">
        <label class="fc-field" :class="bad('goodsDescription')"><span>{{ tr('Item description') }}<b> *</b></span><input v-model="form.goodsDescription" class="fx-search" maxlength="300" :placeholder="tr('e.g. Samsung 55 inch TV')"></label>
        <label class="fc-field" :class="bad('goodsBrandModel')"><span>{{ tr('Brand / model') }}</span><input v-model="form.goodsBrandModel" class="fx-search" maxlength="200"></label>
        <label class="fc-field" :class="bad('merchantName')"><span>{{ tr('Merchant') }}</span><input v-model="form.merchantName" class="fx-search" maxlength="200"></label>
        <label class="fc-field" :class="bad('goodsPriceAmount')"><span>{{ tr('Item price') }} ({{ tr('EGP') }})<b> *</b></span><input v-model.number="form.goodsPriceAmount" type="number" min="0" step="any" class="fx-search"></label>
        <label class="fc-field" :class="bad('downPaymentAmount')"><span>{{ tr('Down payment') }} ({{ tr('EGP') }})</span><input v-model.number="form.downPaymentAmount" type="number" min="0" step="any" class="fx-search"></label>
        <label class="fc-field" :class="bad('principalAmount')"><span>{{ tr('Finance amount') }} ({{ tr('EGP') }})</span>
          <input :value="financeAmount === null ? '' : money(financeAmount)" class="fx-search" readonly>
          <small class="c3-muted">{{ tr('Price less down payment. The scenario must have this amount.') }}</small>
        </label>
      </div>
    </section>

    <section class="fx-card nf-card">
      <h2 class="nf-title"><span class="fa fa-calculator" aria-hidden="true"></span>{{ tr('Schedule') }}</h2>
      <div class="nf-grid">
        <label class="fc-field" :class="bad('scenarioId')"><span>{{ tr('Saved scenario') }}<b> *</b></span>
          <select v-model="form.scenarioId" class="fx-select">
            <option :value="null" disabled>{{ tr('Choose one of your saved scenarios') }}</option>
            <option v-for="s in scenarios" :key="s.id" :value="s.id">{{ s.name }}</option>
          </select>
          <small v-if="!scenarios.length" class="c3-muted">{{ tr('You have no saved scenarios yet.') }}</small>
        </label>
        <div class="nf-calc">
          <RouterLink :to="{ name: 'tls-loan-calculator', query: fixedClientId ? { from: 'finance', clientId: String(fixedClientId) } : { from: 'finance' } }" class="fx-btn">
            <span class="fa fa-external-link" aria-hidden="true"></span>{{ tr('Make a scenario in the Finance Calculator') }}
          </RouterLink>
          <button type="button" class="fx-btn" :disabled="busy" @click="loadScenarios"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Refresh list') }}</button>
        </div>
      </div>
      <p class="c3-muted nf-hint">{{ tr('The scenario gives the amount, contract date, payment dates, rate and schedule. Each payment must have principal and interest together, with one rate for the whole term.') }}</p>

      <div v-if="preview" class="nf-preview">
        <div class="nf-facts">
          <div><small>{{ tr('Instalments') }}</small><b>{{ preview.installmentCount }}</b></div>
          <div><small>{{ tr('Yearly rate') }}</small><b>{{ preview.annualRatePct }}%</b></div>
          <div><small>{{ tr('Total interest') }}</small><b>{{ money(preview.totalInterestAmount) }}</b></div>
          <div><small>{{ tr('Total to repay') }}</small><b>{{ money(preview.totalRepayableAmount) }}</b></div>
          <div><small>{{ tr('First payment') }}</small><b>{{ formatDate(preview.firstDueDate) }}</b></div>
          <div><small>{{ tr('Maturity') }}</small><b>{{ formatDate(preview.maturityDate) }}</b></div>
        </div>
        <div class="fx-table-wrap nf-schedule">
          <table class="fx-table">
            <thead><tr><th>#</th><th>{{ tr('Due date') }}</th><th class="nf-num">{{ tr('Principal') }}</th><th class="nf-num">{{ tr('Interest') }}</th><th class="nf-num">{{ tr('Instalment') }}</th><th class="nf-num">{{ tr('Balance after') }}</th></tr></thead>
            <tbody>
              <tr v-for="s in preview.schedule" :key="s.installmentNo">
                <td>{{ s.installmentNo }}</td><td>{{ formatDate(s.dueDate) }}</td><td class="nf-num">{{ money(s.principal) }}</td>
                <td class="nf-num">{{ money(s.interest) }}</td><td class="nf-num"><b>{{ money(s.total) }}</b></td><td class="nf-num">{{ money(s.closingPrincipal) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div class="nf-actions">
        <RouterLink :to="back" class="fx-btn"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</RouterLink>
        <button type="button" class="fx-btn" :disabled="busy" @click="showPreview"><span class="fa fa-check-square-o" aria-hidden="true"></span>{{ tr('Check and show schedule') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !preview" @click="create">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr('Create Finance') }}
        </button>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ApiError } from '@/shared/api/http'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { loansApi, type LoanOption, type LoanPreview, type LoanRequest } from '../api/loans.api'
import { lendingWsApi } from '../api/workspace.api'

const egp = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const money = (n: number) => egp.format(n)

const route = useRoute()
const router = useRouter()

// from a customer (?clientId=): the customer is filled in and cannot be changed
const fixedClientId = Number(route.query.clientId) || null
const clientLabel = ref('')
const back = computed(() => (fixedClientId ? { name: 'lnd-customer', params: { clientId: fixedClientId } } : { name: 'lnd-loans' }))

const busy = ref(false)
const clients = ref<LoanOption[]>([])
const clientSearch = ref('')
const goods = ref<LoanOption[]>([])
const scenarios = ref<LoanOption[]>([])
const preview = ref<LoanPreview | null>(null)
const errors = ref<Record<string, string[]>>({})
const bad = (f: string) => ({ 'c3-bad': !!errors.value[f] })

const form = reactive<LoanRequest>({
  clientId: fixedClientId, productId: null, principalAmount: null, termMonths: null, disbursementDate: null, firstDueDate: null, notes: null,
  goodsCategoryCode: null, goodsDescription: null, goodsBrandModel: null, merchantName: null, goodsPriceAmount: null, downPaymentAmount: null,
  scenarioId: null
})

const financeAmount = computed(() => {
  const price = Number(form.goodsPriceAmount) || 0
  return price > 0 ? Math.round((price - (Number(form.downPaymentAmount) || 0)) * 100) / 100 : null
})

// any change of the request makes the shown schedule stale
watch(() => ({ ...form, notes: null }), () => (preview.value = null), { deep: true })

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

const body = (): LoanRequest => ({
  ...form, principalAmount: financeAmount.value, notes: form.notes || null, goodsDescription: form.goodsDescription || null,
  goodsBrandModel: form.goodsBrandModel || null, merchantName: form.merchantName || null, downPaymentAmount: form.downPaymentAmount || 0
})

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
  if (!form.scenarioId) {
    errors.value = { scenarioId: ['Choose one of your saved scenarios.'] }
    showError(tr('Choose one of your saved scenarios.'))
    return
  }
  const p = await call(() => loansApi.preview(body()))
  if (p) preview.value = p
}

async function create() {
  const loan = await call(() => loansApi.create(body()))
  if (!loan) return
  showSuccess(`${tr('Finance created:')} ${loan.loanNumber}`)
  router.push({ name: 'lnd-loan', params: { loanId: loan.loanId } })
}

async function loadScenarios() {
  try {
    scenarios.value = await loansApi.scenarios()
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  }
}

onMounted(async () => {
  try {
    const [g] = await Promise.all([loansApi.goodsCategories(), loadScenarios()])
    goods.value = g
    if (fixedClientId) {
      const ws = await lendingWsApi.customer(fixedClientId)
      clientLabel.value = `${ws.clientName} · ${ws.clientNumber}`
    }
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  }
})
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
.nf-card { padding: 16px 20px; }
.nf-title { display: flex; align-items: center; gap: 8px; margin: 0 0 12px; font-size: 15px; font-weight: 800; }
.nf-title .fa { color: var(--fin-teal, #00636d); }
.nf-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 12px 18px; }
.nf-calc { display: flex; gap: 8px; align-items: flex-end; flex-wrap: wrap; }
.nf-hint { margin: 10px 0 0; font-size: 12px; }
.nf-preview { margin-top: 14px; display: flex; flex-direction: column; gap: 10px; }
.nf-facts { display: grid; grid-template-columns: repeat(6, minmax(0, 1fr)); gap: 8px; }
.nf-facts > div { display: flex; flex-direction: column; gap: 2px; padding: 8px 10px; border-radius: var(--fin-radius-sm); background: var(--fin-teal-50); }
.nf-facts small { color: var(--fin-muted); }
.nf-schedule { max-height: 420px; overflow: auto; }
.nf-num { text-align: end; white-space: nowrap; }
.nf-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 16px; }
@media (max-width: 900px) {
  .nf-grid { grid-template-columns: 1fr; }
  .nf-facts { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
</style>
