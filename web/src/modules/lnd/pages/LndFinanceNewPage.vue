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
        <!-- the supplier the goods are bought from (Ahmed, 26 Nov 2026): active suppliers only; its agreement decides the goods and the down payment -->
        <label class="fc-field" :class="bad('supplierApplicationId')"><span>{{ tr('Supplier') }}<b> *</b></span>
          <select v-model="form.supplierApplicationId" class="fx-select">
            <option :value="null" disabled>{{ suppliers.length ? tr('Choose the supplier') : tr('No active supplier') }}</option>
            <option v-for="s in suppliers" :key="s.applicationId" :value="s.applicationId">{{ s.supplierName }} · {{ s.clientNumber }}</option>
          </select>
        </label>
        <label class="fc-field" :class="bad('goodsCategoryCode')"><span>{{ tr('Product') }}<b> *</b></span>
          <select v-model="form.goodsCategoryCode" class="fx-select" :disabled="!supplier">
            <option :value="null" disabled>{{ supplier ? tr('Choose the product') : tr('Choose the supplier first') }}</option>
            <option v-for="g in supplierGoods" :key="g.code" :value="g.code">{{ tr(g.name) }}</option>
          </select>
        </label>
        <label class="fc-field"><span>{{ tr('Notes') }}</span><input v-model="form.notes" class="fx-search" maxlength="2000"></label>
      </div>
      <div v-if="supplier" class="nf-terms">
        <span class="nf-term">{{ tr('Agreement') }} <b dir="ltr">{{ supplier.applicationRef }}</b></span>
        <span class="nf-term">{{ tr('Min down payment') }} <b>{{ supplier.minDownPaymentPct }}%</b></span>
        <span class="nf-term">{{ tr('Down payment collected by') }} <b>{{ tr(supplier.downPaymentCollectorCode === 'COMPANY' ? 'Us' : 'Supplier') }}</b></span>
        <span class="nf-term">{{ tr('Commission') }} <b>{{ supplier.commissionPct }}%</b></span>
        <span class="nf-term">{{ tr('We pay after') }} <b>{{ supplier.paymentDays }} {{ tr('days') }}</b></span>
        <span class="nf-term">{{ tr('Delivers within') }} <b>{{ supplier.deliveryDays }} {{ tr('days') }}</b></span>
      </div>
    </section>

    <section class="fx-card nf-card">
      <h2 class="nf-title"><span class="fa fa-shopping-cart" aria-hidden="true"></span>{{ tr('Financed item') }}</h2>
      <div class="nf-grid">
        <label class="fc-field" :class="bad('goodsDescription')"><span>{{ tr('Item description') }}<b> *</b></span><input v-model="form.goodsDescription" class="fx-search" maxlength="300" :placeholder="tr('e.g. Samsung 55 inch TV')"></label>
        <label class="fc-field" :class="bad('goodsBrandModel')"><span>{{ tr('Brand / model') }}</span><input v-model="form.goodsBrandModel" class="fx-search" maxlength="200"></label>
        <label class="fc-field" :class="bad('goodsPriceAmount')"><span>{{ tr('Item price') }} ({{ tr('EGP') }})<b> *</b></span><input v-model.number="form.goodsPriceAmount" type="number" min="0" step="any" class="fx-search"></label>
        <label class="fc-field" :class="bad('downPaymentAmount')"><span>{{ tr('Down payment') }} ({{ tr('EGP') }})<b v-if="minDown"> *</b></span>
          <input v-model.number="form.downPaymentAmount" type="number" min="0" step="any" class="fx-search">
          <small v-if="minDown !== null" class="c3-muted">{{ tr('At least') }} {{ supplier!.minDownPaymentPct }}% = {{ money(minDown) }}</small>
        </label>
        <label class="fc-field" :class="bad('principalAmount')"><span>{{ tr('Finance amount') }} ({{ tr('EGP') }})</span>
          <input :value="financeAmount === null ? '' : money(financeAmount)" class="fx-search" readonly>
          <small class="c3-muted">{{ tr('Price less down payment. The scenario must have this amount.') }}</small>
        </label>
      </div>
    </section>

    <section class="fx-card nf-card">
      <h2 class="nf-title"><span class="fa fa-calculator" aria-hidden="true"></span>{{ tr('Schedule') }}</h2>
      <div class="nf-grid">
        <label class="fc-field" :class="bad('scenarioId')"><span>{{ tr(calculation ? 'Schedule from the calculator' : 'Saved scenario') }}<b> *</b></span>
          <div v-if="calculation" class="nf-calc-chip">
            <span class="fa fa-calculator" aria-hidden="true"></span>
            <span>{{ tr('Unsaved schedule from the calculator') }} · {{ money(calculation.principalAmount) }} · {{ calculation.interestRatePct }}%</span>
            <button type="button" class="nf-chip-x" :title="tr('Remove')" :aria-label="tr('Remove')" @click="dropCalculation"><span class="fa fa-times" aria-hidden="true"></span></button>
          </div>
          <select v-else v-model="form.scenarioId" class="fx-select" @change="onScenario">
            <option :value="null" disabled>{{ tr('Choose one of your saved scenarios') }}</option>
            <option v-for="s in scenarios" :key="s.id" :value="s.id">{{ s.name }} · {{ money(s.principalAmount) }} · {{ s.ratePct }}%</option>
          </select>
          <small v-if="!calculation && !scenarios.length" class="c3-muted">{{ tr('You have no saved scenarios yet.') }}</small>
        </label>
        <label class="fc-field" :class="bad('ratePct')"><span>{{ tr('Rate %') }}</span>
          <input v-model.number="form.ratePct" type="number" min="0" max="100" step="0.0001" class="fx-search" :disabled="baseRate === null">
          <small v-if="rateChanged" class="nf-warn">{{ tr('Changed from the scenario rate') }} {{ baseRate }}%. {{ tr('The schedule is worked out again for this finance; the saved scenario is not changed.') }}</small>
          <small v-else-if="baseRate !== null" class="c3-muted">{{ tr('From the scenario. Change it for this finance if needed.') }}</small>
        </label>
        <div class="nf-calc">
          <button type="button" class="fx-btn" @click="goCalculator">
            <span class="fa fa-calculator" aria-hidden="true"></span>{{ tr('Make a scenario in the Finance Calculator') }}
          </button>
          <button type="button" class="fx-btn" :disabled="busy" @click="loadScenarios"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Refresh list') }}</button>
        </div>
      </div>
      <p class="c3-muted nf-hint">{{ tr('The scenario gives the amount, contract date, payment dates, rate and schedule. Each payment must have principal and interest together, with one rate for the whole term.') }}</p>

      <div v-if="shown" ref="scheduleBox" class="nf-preview">
        <p v-if="!preview" class="nf-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('The schedule from the calculator. Click Check and show schedule to check it with this finance before creating it.') }}</p>
        <p v-else class="nf-ok"><span class="fa fa-check-circle" aria-hidden="true"></span>{{ tr('Checked for this finance. You can create it.') }}</p>
        <p class="nf-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Indicative only: Credit decides the charges, and the schedule is built at the Schedule and contract step.') }}</p>
        <div class="nf-facts">
          <div><small>{{ tr('Instalments') }}</small><b>{{ shown.installmentCount }}</b></div>
          <div><small>{{ tr('Yearly rate') }}</small><b>{{ shown.annualRatePct }}%</b></div>
          <div><small>{{ tr('Total interest') }}</small><b>{{ money(shown.totalInterestAmount) }}</b></div>
          <div><small>{{ tr('Total to repay') }}</small><b>{{ money(shown.totalRepayableAmount) }}</b></div>
          <div><small>{{ tr('First payment') }}</small><b>{{ formatDate(shown.firstDueDate) }}</b></div>
          <div><small>{{ tr('Maturity') }}</small><b>{{ formatDate(shown.maturityDate) }}</b></div>
        </div>
        <div class="fx-table-wrap nf-schedule">
          <table class="fx-table">
            <thead><tr><th>#</th><th>{{ tr('Due date') }}</th><th class="nf-num">{{ tr('Principal') }}</th><th class="nf-num">{{ tr('Interest') }}</th><th class="nf-num">{{ tr('Instalment') }}</th><th class="nf-num">{{ tr('Balance after') }}</th></tr></thead>
            <tbody>
              <tr v-for="s in shown.schedule" :key="s.installmentNo">
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
import { computed, nextTick, onMounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ApiError } from '@/shared/api/http'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import type { LoanScenarioResult } from '@/modules/tls/api/loanScenarios.api'
import { loansApi, type CfSupplierOption, type LoanOption, type LoanPreview, type LoanRequest, type ScenarioOption } from '../api/loans.api'
import { lendingWsApi } from '../api/workspace.api'
import { FINANCE_CALCULATION_KEY, FINANCE_CALCULATION_RESULT_KEY, FINANCE_DRAFT_KEY, keep, take, type FinanceCalculation } from '../financeDraft'

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
const suppliers = ref<CfSupplierOption[]>([])
const supplier = computed(() => suppliers.value.find((s) => s.applicationId === form.supplierApplicationId) ?? null)
// only the goods the supplier may sell
const supplierGoods = computed(() => (supplier.value ? goods.value.filter((g) => supplier.value!.goodsCategoryCodes.includes(g.code)) : []))
// the supplier's least down payment for this price
const minDown = computed(() => {
  const price = Number(form.goodsPriceAmount) || 0
  return supplier.value && price > 0 ? Math.round(price * supplier.value.minDownPaymentPct) / 100 : null
})
const scenarios = ref<ScenarioOption[]>([])
const preview = ref<LoanPreview | null>(null)
// the schedule the calculator showed, shown at once on return until it is checked for this finance
const handed = ref<LoanPreview | null>(null)
const scheduleBox = ref<HTMLElement | null>(null)
const errors = ref<Record<string, string[]>>({})
const bad = (f: string) => ({ 'c3-bad': !!errors.value[f] })

const form = reactive<LoanRequest>({
  clientId: fixedClientId, productId: null, principalAmount: null, termMonths: null, disbursementDate: null, firstDueDate: null, notes: null,
  goodsCategoryCode: null, goodsDescription: null, goodsBrandModel: null, merchantName: null, goodsPriceAmount: null, downPaymentAmount: null,
  supplierApplicationId: null,
  scenarioId: null, ratePct: null
})

// or a schedule handed back by the calculator without saving it
const calculation = ref<FinanceCalculation | null>(null)

const chosen = computed(() => scenarios.value.find((s) => s.id === form.scenarioId) ?? null)
const shown = computed(() => preview.value ?? (calculation.value && !rateChanged.value ? handed.value : null))

function asPreview(r: LoanScenarioResult): LoanPreview | null {
  const first = r.schedule[0]
  const last = r.schedule[r.schedule.length - 1]
  if (!first || !last) return null
  const interest = r.schedule.reduce((t, x) => t + x.interestAmount, 0)
  return {
    versionNo: 0, repaymentFrequencyCode: '', interestMethodCode: '', annualRatePct: r.rates[0]?.interestRatePct ?? 0,
    installmentCount: r.schedule.length, installmentAmount: first.totalPaymentAmount, totalInterestAmount: interest,
    totalRepayableAmount: r.principalAmount + interest, firstDueDate: first.paymentDate, maturityDate: last.paymentDate,
    schedule: r.schedule.map((x) => ({
      installmentNo: x.installmentNo, dueDate: x.paymentDate, openingPrincipal: x.openingPrincipal, principal: x.principalAmount,
      interest: x.interestAmount, total: x.totalPaymentAmount, closingPrincipal: x.closingPrincipal
    }))
  }
}

const baseRate = computed(() => chosen.value?.ratePct ?? calculation.value?.interestRatePct ?? null)
const rateChanged = computed(() => baseRate.value !== null && form.ratePct !== null && (form.ratePct as unknown) !== ''
  && Number(form.ratePct) !== baseRate.value)

// a scenario brings its own rate; the user may change it for this finance
function onScenario() {
  form.ratePct = chosen.value?.ratePct ?? null
}

function dropCalculation() {
  calculation.value = null
  handed.value = null
  form.ratePct = null
}

/** Keeps what was typed here, then opens the calculator with the finance amount. */
function goCalculator() {
  keep(FINANCE_DRAFT_KEY, { clientId: fixedClientId, form: { ...form }, clientSearch: clientSearch.value, clients: clients.value })
  const query: Record<string, string> = { from: 'finance' }
  if (fixedClientId) query.clientId = String(fixedClientId)
  if (financeAmount.value && financeAmount.value > 0) query.amount = String(financeAmount.value)
  router.push({ name: 'tls-loan-calculator', query })
}

const financeAmount = computed(() => {
  const price = Number(form.goodsPriceAmount) || 0
  if (price <= 0) return null
  return Math.round((price - (Number(form.downPaymentAmount) || 0)) * 100) / 100
})

// a product the new supplier does not sell is cleared
watch(() => form.supplierApplicationId, () => {
  if (form.goodsCategoryCode && !supplierGoods.value.some((g) => g.code === form.goodsCategoryCode)) form.goodsCategoryCode = null
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
  goodsBrandModel: form.goodsBrandModel || null, merchantName: null, downPaymentAmount: form.downPaymentAmount || 0,
  ratePct: form.ratePct === null || (form.ratePct as unknown) === '' ? null : Number(form.ratePct),
  scenarioId: calculation.value ? null : form.scenarioId, calculation: calculation.value
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
  if (!form.scenarioId && !calculation.value) {
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
  showSuccess(loan.statusCode === 'IN_APPROVAL' ? `${tr('Finance submitted for credit approval:')} ${loan.loanNumber}` : `${tr('Finance created:')} ${loan.loanNumber}`)
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
    const [g, sup] = await Promise.all([loansApi.goodsCategories(), loansApi.cfSuppliers(), loadScenarios()])
    goods.value = g
    suppliers.value = sup
    // back from the calculator: what was typed before, and the schedule it handed back (if any)
    const draft = take<{ clientId: number | null; form: LoanRequest; clientSearch: string; clients: LoanOption[] }>(FINANCE_DRAFT_KEY)
    if (draft && draft.clientId === fixedClientId) {
      Object.assign(form, draft.form, { clientId: fixedClientId ?? draft.form.clientId })
      clientSearch.value = draft.clientSearch
      clients.value = draft.clients
    }
    const terms = take<FinanceCalculation>(FINANCE_CALCULATION_KEY)
    const result = take<LoanScenarioResult>(FINANCE_CALCULATION_RESULT_KEY)
    if (terms) {
      calculation.value = terms
      form.scenarioId = null
      form.ratePct = terms.interestRatePct
      handed.value = result ? asPreview(result) : null
      // the schedule is shown at once; when the finance is filled in, it is checked straight away too
      await nextTick()
      scheduleBox.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
      if (form.clientId && form.goodsCategoryCode && form.goodsDescription && form.goodsPriceAmount) await showPreview()
    }
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
.nf-terms { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 12px; }
.nf-term { padding: 4px 10px; border-radius: 999px; background: #f2f4f6; color: #34495a; font-size: 12px; }
.nf-term b { color: var(--fin-ink); }
.nf-card { padding: 16px 20px; }
.nf-title { display: flex; align-items: center; gap: 8px; margin: 0 0 12px; font-size: 15px; font-weight: 800; }
.nf-title .fa { color: var(--fin-teal, #00636d); }
.nf-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 12px 18px; }
.nf-warn { color: #b45309; }
.nf-note, .nf-ok { display: flex; align-items: center; gap: 8px; margin: 0; padding: 8px 10px; border-radius: var(--fin-radius-sm, 8px); font-size: 13px; }
.nf-note { background: #fff7ed; color: #9a3412; }
.nf-ok { background: #f0fdf4; color: #166534; }
.nf-calc-chip { display: flex; align-items: center; gap: 8px; min-height: 38px; padding: 6px 10px; border: 1px solid var(--fin-teal, #00636d); border-radius: var(--fin-radius-sm, 8px); background: var(--fin-teal-50, #eef9f8); font-size: 13px; }
.nf-calc-chip > span:nth-child(2) { flex: 1; }
.nf-chip-x { border: 0; background: transparent; color: var(--fin-muted); cursor: pointer; }
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
