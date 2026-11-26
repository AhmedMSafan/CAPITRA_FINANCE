<template>
  <!-- Retail Lending > New Finance (consumer finance): step 1 of CF_FINANCE, the main data only (Ahmed, 26 Nov 2026): customer, supplier,
       product, item, price, down payment and term. The instalment shown is indicative, at the product rate: Credit decides the charges,
       and the schedule is built at the Schedule and contract step. From a customer, the customer comes filled in and fixed. -->
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
          <small class="c3-muted">{{ tr('Price less down payment.') }}</small>
        </label>
        <label class="fc-field" :class="bad('termMonths')"><span>{{ tr('Term (months)') }}<b> *</b></span>
          <input v-model.number="form.termMonths" type="number" min="1" step="1" class="fx-search">
        </label>
      </div>
      <p v-if="indicative" class="nf-ind"><span class="fa fa-info-circle" aria-hidden="true"></span>
        {{ tr('About') }} <b>{{ money(indicative.installmentAmount) }}</b> × {{ indicative.installmentCount }} {{ tr('at the product rate') }} {{ indicative.annualRatePct }}%.
        {{ tr('Indicative only: Credit decides the charges, and the schedule is built at the Schedule and contract step.') }}</p>
      <div class="nf-actions">
        <RouterLink :to="back" class="fx-btn"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</RouterLink>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="create">
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
import { tr } from '@/shared/layout/language'
import { loansApi, type CfSupplierOption, type LoanOption, type LoanPreview, type LoanRequest } from '../api/loans.api'
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
const suppliers = ref<CfSupplierOption[]>([])
const supplier = computed(() => suppliers.value.find((s) => s.applicationId === form.supplierApplicationId) ?? null)
// only the goods the supplier may sell
const supplierGoods = computed(() => (supplier.value ? goods.value.filter((g) => supplier.value!.goodsCategoryCodes.includes(g.code)) : []))
// the supplier's least down payment for this price
const minDown = computed(() => {
  const price = Number(form.goodsPriceAmount) || 0
  return supplier.value && price > 0 ? Math.round(price * supplier.value.minDownPaymentPct) / 100 : null
})
const indicative = ref<LoanPreview | null>(null)
const errors = ref<Record<string, string[]>>({})
const bad = (f: string) => ({ 'c3-bad': !!errors.value[f] })

const form = reactive<LoanRequest>({
  clientId: fixedClientId, productId: null, principalAmount: null, termMonths: null, disbursementDate: null, firstDueDate: null, notes: null,
  goodsCategoryCode: null, goodsDescription: null, goodsBrandModel: null, merchantName: null, goodsPriceAmount: null, downPaymentAmount: null,
  supplierApplicationId: null
})

const financeAmount = computed(() => {
  const price = Number(form.goodsPriceAmount) || 0
  if (price <= 0) return null
  return Math.round((price - (Number(form.downPaymentAmount) || 0)) * 100) / 100
})

// a product the new supplier does not sell is cleared
watch(() => form.supplierApplicationId, () => {
  if (form.goodsCategoryCode && !supplierGoods.value.some((g) => g.code === form.goodsCategoryCode)) form.goodsCategoryCode = null
})

// the indicative instalment, worked out (not saved) once the main data is in
let previewTimer: ReturnType<typeof setTimeout> | undefined
watch(() => ({ ...form, notes: null }), () => {
  clearTimeout(previewTimer)
  indicative.value = null
  if (!form.supplierApplicationId || !form.goodsCategoryCode || !form.goodsDescription || !financeAmount.value || !(Number(form.termMonths) > 0)) return
  previewTimer = setTimeout(async () => {
    try {
      indicative.value = await loansApi.preview(body())
    } catch {
      indicative.value = null // the checks show on Create
    }
  }, 400)
}, { deep: true })

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
  ...form, productId: null, principalAmount: financeAmount.value, termMonths: Number(form.termMonths) || null, notes: form.notes || null,
  goodsDescription: form.goodsDescription || null, goodsBrandModel: form.goodsBrandModel || null, merchantName: null,
  downPaymentAmount: form.downPaymentAmount || 0, scenarioId: null, calculation: null, ratePct: null
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

async function create() {
  const loan = await call(() => loansApi.create(body()))
  if (!loan) return
  showSuccess(loan.statusCode === 'IN_APPROVAL' ? `${tr('Finance submitted for credit approval:')} ${loan.loanNumber}` : `${tr('Finance created:')} ${loan.loanNumber}`)
  router.push({ name: 'lnd-loan', params: { loanId: loan.loanId } })
}

onMounted(async () => {
  try {
    const [g, sup] = await Promise.all([loansApi.goodsCategories(), loansApi.cfSuppliers()])
    goods.value = g
    suppliers.value = sup
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
.nf-ind { display: flex; align-items: center; gap: 6px; flex-wrap: wrap; margin: 12px 0 0; padding: 8px 10px; border-radius: var(--fin-radius-sm, 8px); background: var(--fin-teal-50, #eef9f8); font-size: 13px; color: #34495a; }
.nf-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 16px; }
@media (max-width: 900px) {
  .nf-grid { grid-template-columns: 1fr; }
}
</style>
