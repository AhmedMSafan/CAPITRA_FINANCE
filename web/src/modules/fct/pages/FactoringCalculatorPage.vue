<template>
  <!-- FINOVA_FINANCEING FactoringCalculatorView: the four input sections, Calculate, Use In Offer and the calculation summary,
       in this app's fx- theme. FINOVA's "Save draft" and "Submit Pricing" buttons did nothing and are left out. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-calculator"></span></span>
        <div>
          <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
            <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
            <RouterLink :to="{ name: 'factoring-home' }">{{ tr('Factoring') }}</RouterLink><span aria-hidden="true">›</span>
            <RouterLink :to="workspace">{{ tr('Customer') }}</RouterLink><span aria-hidden="true">›</span>
            <b>{{ tr('Factoring Calculator') }}</b>
          </nav>
          <h1>{{ tr('Factoring Calculator') }}</h1>
          <p>{{ tr('Calculate pricing, review the results and create an offer') }}</p>
        </div>
      </div>
      <div class="fx-card fc-summary-customer">
        <span class="fc-quick-ic" aria-hidden="true"><span class="fa fa-building-o"></span></span>
        <div>
          <span class="fc-customer-no">{{ client?.clientNumber || '—' }}</span>
          <b>{{ client?.clientName || (loading ? tr('Loading...') : '—') }}</b>
          <small>{{ client?.clientNameAr || '' }}</small>
        </div>
        <div class="fc-info"><span>{{ tr('Currency') }}</span><span class="fx-chip fx-chip--brand fx-chip--plain">{{ client?.currencyCode || '—' }}</span></div>
      </div>
    </div>

    <div v-if="error" class="fc-alert" role="alert"><span class="fa fa-exclamation-circle" aria-hidden="true"></span>{{ error }}</div>

    <div class="fc-calc">
      <main class="fc-calc-main">
        <section class="fx-card">
          <header class="fc-sec-head"><span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-file-text-o"></span></span>
            <div><h2>{{ tr('Deal Basics') }}</h2><p>{{ tr('Define the core details of the factoring deal') }}</p></div></header>
          <div class="fc-grid fc-grid--3">
            <label class="fc-field"><span>{{ tr('Factoring Product') }} <b>*</b></span>
              <select v-model="f.factoringProduct" class="fx-select"><option>CLASSIC - Supplier</option><option>CLASSIC - Buyer</option></select></label>
            <label class="fc-field"><span>{{ tr('Recourse Type') }} <b>*</b></span>
              <select v-model="f.recourseType" class="fx-select"><option>RECOURSE</option><option>NON-RECOURSE</option></select></label>
            <label class="fc-field"><span>{{ tr('Requested Limit (EGP)') }} <b>*</b></span><input v-model.number="f.requestedLimit" class="fx-search" type="number" min="0"></label>
            <label class="fc-field"><span>{{ tr('Expected Annual Volume') }} <b>*</b></span><input v-model.number="f.expectedAnnualVolume" class="fx-search" type="number" min="0"></label>
            <label class="fc-field"><span>{{ tr('Average Invoice Tenor (days)') }} <b>*</b></span><input v-model.number="f.averageInvoiceTenor" class="fx-search" type="number" min="0"></label>
            <label class="fc-field"><span>{{ tr('Number of Debtors') }}</span><input v-model.number="f.numDebtors" class="fx-search" type="number" min="1"></label>
            <label class="fc-field"><span>{{ tr('Est. Transactions per Year') }}</span><input v-model.number="f.estimatedTransactions" class="fx-search" type="number" min="0"></label>
          </div>
        </section>

        <section class="fx-card">
          <header class="fc-sec-head"><span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-shield"></span></span>
            <div><h2>{{ tr('Risk Buffer - Input') }}</h2><p>{{ tr('Set the risk parameters for the calculation') }}</p></div></header>
          <div class="fc-grid fc-grid--3">
            <label class="fc-field"><span>{{ tr('Risk Band') }} <b>*</b></span>
              <select v-model="f.riskBand" class="fx-select"><option>LOW</option><option>MEDIUM</option><option>HIGH</option></select></label>
            <label class="fc-field"><span>{{ tr('Largest Concentration') }} <b>*</b></span><input v-model.number="f.largestConcentration" class="fx-search" type="number" step="0.01" min="0"></label>
            <label class="fc-field"><span>{{ tr('Dilution Rate') }} <b>*</b></span><input v-model.number="f.dilutionRate" class="fx-search" type="number" step="0.01" min="0"></label>
          </div>
        </section>

        <section class="fx-card">
          <header class="fc-sec-head"><span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-bar-chart"></span></span>
            <div><h2>{{ tr('Market & Cost') }}</h2><p>{{ tr('Define market conditions and cost assumptions') }}</p></div></header>
          <div class="fc-grid fc-grid--4">
            <label class="fc-field"><span>{{ tr('Cost Of Funds Rate') }} <b>*</b></span><input v-model.number="f.costOfFundsRate" class="fx-search" type="number" step="0.01"></label>
            <label class="fc-field"><span>{{ tr('Operating Spread') }} <b>*</b></span><input v-model.number="f.operatingSpread" class="fx-search" type="number" step="0.01"></label>
            <label class="fc-field"><span>{{ tr('Base Advance Ratio') }} <b>*</b></span><input v-model.number="f.baseAdvanceRatio" class="fx-search" type="number" step="0.01"></label>
            <label class="fc-field"><span>{{ tr('Risk Buffer Override') }}</span><input v-model.number="f.riskBufferOverride" class="fx-search" type="number" step="0.01" :placeholder="tr('Enter percentage')"></label>
          </div>
        </section>

        <section class="fx-card">
          <header class="fc-sec-head"><span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-database"></span></span>
            <div><h2>{{ tr('Fees & Economics') }}</h2><p>{{ tr('Configure fees and additional charges') }}</p></div></header>
          <div class="fc-grid fc-grid--3">
            <label class="fc-field"><span>{{ tr('Setup Fee — Fixed (EGP)') }}</span><input v-model.number="f.setupFeeFixed" class="fx-search" type="number" min="0"></label>
            <label class="fc-field"><span>{{ tr('Setup Fee — % of Limit') }}</span><input v-model.number="f.setupFeePct" class="fx-search" type="number" step="0.01"></label>
            <label class="fc-field"><span>{{ tr('Commission — % of Gross Invoices') }}</span><input v-model.number="f.commissionPct" class="fx-search" type="number" step="0.01"></label>
            <label class="fc-field"><span>{{ tr('Handling Fee per Transaction (EGP)') }}</span><input v-model.number="f.handlingFeePerTransaction" class="fx-search" type="number" min="0"></label>
            <label class="fc-field"><span>{{ tr('Debtor Analysis Fee per Debtor (EGP)') }}</span><input v-model.number="f.debtorAnalysisFee" class="fx-search" type="number" min="0"></label>
            <label class="fc-field"><span>{{ tr('Debtor Analysis — Minimum (EGP)') }}</span><input v-model.number="f.debtorAnalysisMinimum" class="fx-search" type="number" min="0"></label>
          </div>
        </section>
      </main>

      <aside class="fc-calc-side">
        <section class="fx-card">
          <header class="fc-sec-head"><span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-cog"></span></span>
            <div><h2>{{ tr('Calculate & Actions') }}</h2><p>{{ tr('Run the calculation and manage the results') }}</p></div></header>
          <div class="fc-actions">
            <button type="button" class="fx-btn fx-btn--teal fx-btn--block" :disabled="calculating" @click="calculate">
              <span class="fa" :class="calculating ? 'fa-spinner fa-spin' : 'fa-calculator'" aria-hidden="true"></span>{{ tr(calculating ? 'Calculating...' : 'Calculate') }}
            </button>
            <button v-if="canEdit" type="button" class="fx-btn fx-btn--primary fx-btn--block" :disabled="creating" @click="useInOffer">
              <span class="fa" :class="creating ? 'fa-spinner fa-spin' : 'fa-arrow-circle-right'" aria-hidden="true"></span>{{ tr(creating ? 'Creating Offer...' : 'Use In Offer') }}
            </button>
            <div v-if="offerError" class="fc-alert fc-alert--sm" role="alert"><span class="fa fa-exclamation-circle" aria-hidden="true"></span>{{ offerError }}</div>
          </div>
        </section>

        <section class="fx-card">
          <header class="fc-sec-head"><span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-bar-chart"></span></span>
            <div><h2>{{ tr('Calculation Summary') }}</h2><p>{{ tr('Key pricing results updated after calculation') }}</p></div>
            <span class="fx-chip fx-chip--warn">DRAFT</span></header>
          <div v-if="!result" class="fx-empty">
            <span class="fa fa-info-circle" aria-hidden="true"></span>
            <b>{{ tr('Click Calculate to see the pricing results') }}</b><br>
            <small>{{ tr('All amounts are indicative and subject to credit approval.') }}</small>
          </div>
          <div v-else class="fc-summary">
            <div class="fc-hero">
              <div><span>{{ tr('Advance Rate') }}</span><b>{{ pct(result.advancePct) }}</b></div>
              <small>EGP {{ money(ofLimit(result.advancePct)) }}</small>
            </div>
            <dl class="fc-lines">
              <div><dt>{{ tr('Discount Rate') }}</dt><dd>{{ pct(result.discountRatePct) }}</dd></div>
              <div><dt>{{ tr('Spread over LCR') }}</dt><dd>{{ pct(result.spreadOverLcrPct) }}</dd></div>
              <div><dt>{{ tr('Total Fees') }}</dt><dd>EGP {{ money(result.totalAnnualIncome) }}</dd></div>
              <div><dt>{{ tr('Interest') }}</dt><dd>EGP {{ money2(result.interestIncome) }}</dd></div>
            </dl>
            <dl class="fc-mini">
              <h3>{{ tr('Advance Breakdown') }}</h3>
              <div><dt>{{ tr('Base') }}</dt><dd>{{ pct(result.advanceBasePct) }}</dd></div>
              <div><dt>{{ tr('Dilution') }}</dt><dd>{{ pct(result.advanceDilutionPct) }}</dd></div>
              <div><dt>{{ tr('Risk Buffer') }}</dt><dd>{{ pct(result.advanceRiskBufferPct) }}</dd></div>
              <div class="is-total"><dt>{{ tr('Final') }}</dt><dd>{{ pct(result.advancePct) }}</dd></div>
            </dl>
            <dl class="fc-mini">
              <h3>{{ tr('Discount Breakdown') }}</h3>
              <div><dt>{{ tr('Cost of Funds') }}</dt><dd>{{ pct(result.discountCostOfFundsPct) }}</dd></div>
              <div><dt>{{ tr('Spread') }}</dt><dd>{{ pct(result.discountOperatingSpreadPct) }}</dd></div>
              <div><dt>{{ tr('Risk') }}</dt><dd>{{ pct(result.discountRiskMarginPct) }}</dd></div>
              <div><dt>{{ tr('Tenor') }}</dt><dd>{{ pct(result.discountTenorAdjustmentPct) }}</dd></div>
              <div><dt>{{ tr('Concentration') }}</dt><dd>{{ pct(result.discountConcentrationPct) }}</dd></div>
              <div class="is-total"><dt>{{ tr('All-in') }}</dt><dd>{{ pct(result.discountRatePct) }}</dd></div>
            </dl>
            <dl class="fc-mini">
              <h3>{{ tr('Fees & Economics') }}</h3>
              <div><dt>{{ tr('Setup Fee') }}</dt><dd>EGP {{ money(result.setupFeeTotal) }}</dd></div>
              <div><dt>{{ tr('Commission') }}</dt><dd>EGP {{ money(result.commissionAnnual) }}</dd></div>
              <div><dt>{{ tr('Handling') }}</dt><dd>EGP {{ money(result.handlingAnnual) }}</dd></div>
              <div><dt>{{ tr('Debtor Fees') }}</dt><dd>EGP {{ money(result.debtorFeesTotal) }}</dd></div>
              <div><dt>{{ tr('Interest') }}</dt><dd>EGP {{ money2(result.interestIncome) }}</dd></div>
              <div class="is-total"><dt>{{ tr('Total Annual Income') }}</dt><dd>EGP {{ money(result.totalAnnualIncome) }}</dd></div>
            </dl>
          </div>
        </section>
      </aside>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import './factoring.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import { fctApi, fctApplicationsApi, type FctCalculationResult, type FctCustomerDetails } from '../api/fct.api'

const props = defineProps<{ clientId: string }>()
const router = useRouter()

const id = computed(() => Number(props.clientId))
const workspace = computed(() => ({ name: 'FactoringCustomerWorkspace', params: { clientId: props.clientId } }))
const canEdit = hasPermission('FCT.EDIT')

const client = ref<FctCustomerDetails | null>(null)
const loading = ref(false)
const error = ref('')
const result = ref<FctCalculationResult | null>(null)
const calculating = ref(false)
const creating = ref(false)
const offerError = ref('')

// FINOVA's starting values
const f = reactive({
  factoringProduct: 'CLASSIC - Supplier',
  recourseType: 'RECOURSE',
  requestedLimit: null as number | null,
  expectedAnnualVolume: null as number | null,
  averageInvoiceTenor: 90 as number | null,
  numDebtors: 1 as number | null,
  estimatedTransactions: null as number | null,
  riskBand: 'LOW',
  largestConcentration: 0.25 as number | null,
  dilutionRate: 0 as number | null,
  costOfFundsRate: 12 as number | null,
  operatingSpread: 3 as number | null,
  baseAdvanceRatio: 90 as number | null,
  riskBufferOverride: null as number | null,
  setupFeeFixed: 10000 as number | null,
  setupFeePct: 0 as number | null,
  commissionPct: 0 as number | null,
  handlingFeePerTransaction: 0 as number | null,
  debtorAnalysisFee: 8000 as number | null,
  debtorAnalysisMinimum: 0 as number | null
})

// an emptied number box gives '' with v-model.number: send it as no value
const n = (v: number | string | null) => (v === '' || v == null ? null : Number(v))

const message = (e: unknown, fallback: string) => (e instanceof Error ? e.message : fallback)

async function calculate() {
  error.value = ''
  calculating.value = true
  try {
    result.value = await fctApplicationsApi.calculate({
      requestedLimit: n(f.requestedLimit), expectedVolume: n(f.expectedAnnualVolume), averageTenorDays: n(f.averageInvoiceTenor),
      numDebtors: n(f.numDebtors), estTransactionsPerYear: n(f.estimatedTransactions), riskBand: f.riskBand,
      baseAdvanceRatio: n(f.baseAdvanceRatio), dilutionRate: n(f.dilutionRate), riskBufferOverride: n(f.riskBufferOverride),
      largestConcentration: n(f.largestConcentration), costOfFundsRate: n(f.costOfFundsRate), operatingSpread: n(f.operatingSpread),
      setupFeeFixed: n(f.setupFeeFixed), setupFeePct: n(f.setupFeePct), commissionPct: n(f.commissionPct),
      handlingFee: n(f.handlingFeePerTransaction), debtorFee: n(f.debtorAnalysisFee), debtorMinimum: n(f.debtorAnalysisMinimum)
    })
  } catch (e) {
    error.value = message(e, tr('Unable to calculate factoring pricing.'))
  } finally {
    calculating.value = false
  }
}

async function useInOffer() {
  offerError.value = ''
  if (!result.value) {
    offerError.value = tr('Please calculate pricing before creating an offer.')
    return
  }
  creating.value = true
  try {
    await fctApplicationsApi.createFromCalculator({
      clientId: id.value, notes: '', factoringType: f.factoringProduct, recourseType: f.recourseType,
      requestedLimit: n(f.requestedLimit) ?? 0, averageTenorDays: n(f.averageInvoiceTenor) ?? 0,
      costOfFundsRate: n(f.costOfFundsRate) ?? 0, operatingSpread: n(f.operatingSpread) ?? 0, riskBand: f.riskBand,
      advancePct: result.value.advancePct, advanceDilutionPct: result.value.advanceDilutionPct,
      discountRatePct: result.value.discountRatePct, spreadOverLcrPct: result.value.spreadOverLcrPct
    })
    await router.push(workspace.value)
  } catch (e) {
    offerError.value = message(e, tr('Unable to create offer.'))
  } finally {
    creating.value = false
  }
}

const money = (v: number | null | undefined) => new Intl.NumberFormat('en-US', { maximumFractionDigits: 0 }).format(v ?? 0)
const money2 = (v: number | null | undefined) => new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(v ?? 0)
const pct = (v: number | null | undefined) => `${Number(v ?? 0).toFixed(2)}%`
const ofLimit = (percentage: number) => (n(f.requestedLimit) ?? 0) * (percentage / 100)

onMounted(async () => {
  if (!Number.isFinite(id.value) || id.value <= 0) {
    error.value = tr('Customer ID is missing from the calculator route.')
    return
  }
  loading.value = true
  try {
    client.value = await fctApi.customer(id.value)
  } catch (e) {
    error.value = message(e, tr('Unable to load customer information.'))
  } finally {
    loading.value = false
  }
})
</script>
