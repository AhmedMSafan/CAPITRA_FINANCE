<template>
  <!-- Tools > Loan Calculator: the FINOVA_FINANCEING Loan Calculator, as it is (Ahmed, 15 Nov 2026). The rate starts as the one
       Administration > Rate Maintenance has valid on the contract date, and the user may change it. Generate calculates on screen
       only; Save keeps the scenario, which only its creator sees. -->
  <div class="fx fc loan-page">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <span>{{ tr('Tools') }}</span><span aria-hidden="true">›</span>
          <b>{{ tr('Finance Calculator') }}</b>
        </nav>
        <h1>{{ tr('Finance Calculator') }}</h1>
        <p class="loan-sub">{{ tr('Generate, review and save finance repayment scenarios.') }}</p>
      </div>

      <div class="scenario-selector">
        <label for="saved-scenario">{{ tr('Saved Scenario') }}</label>
        <select id="saved-scenario" v-model="selectedSavedScenarioId" :disabled="loading" @change="onSavedScenarioChanged">
          <option :value="null">-- {{ tr('New Scenario') }} --</option>
          <option v-for="scenario in savedScenarios" :key="scenario.scenarioId" :value="scenario.scenarioId">{{ scenario.scenarioName }}</option>
        </select>
        <span class="mode-badge" :class="{ update: isUpdateMode }">{{ tr(isUpdateMode ? 'Update Mode' : 'New Scenario') }}</span>
      </div>
    </div>

    <div v-if="successMessage" class="loan-message success"><span class="fa fa-check-circle" aria-hidden="true"></span>{{ successMessage }}</div>
    <div v-if="errorMessage" class="loan-message error"><span class="fa fa-exclamation-circle" aria-hidden="true"></span>{{ errorMessage }}</div>

    <section class="loan-card">
      <div class="loan-section-title"><span class="fa fa-pencil-square-o" aria-hidden="true"></span>{{ tr('Main Information') }}</div>

      <div class="loan-grid">
        <div class="field">
          <label for="lc-principal">{{ tr('Principal Amount') }}</label>
          <input id="lc-principal" v-model.number="form.principalAmount" type="number" min="0" step="0.01">
        </div>

        <div class="field">
          <label for="lc-contract">{{ tr('Contract Date') }}</label>
          <input id="lc-contract" v-model="form.contractDate" type="date">
        </div>

        <div class="field">
          <label for="lc-tenor">{{ tr('Finance Tenor') }}</label>
          <div class="inline-field">
            <input id="lc-tenor" v-model.number="form.tenorValue" type="number" min="1">
            <select v-model="form.tenorUnit" :aria-label="tr('Tenor unit')">
              <option v-for="item in ContractingFrequencyUnits" :key="item.value" :value="item.value">{{ tr(item.label) }}</option>
            </select>
          </div>
        </div>

        <div class="field">
          <label for="lc-maturity">{{ tr('Maturity Date') }}</label>
          <input id="lc-maturity" :value="formatDisplayDate(maturityDate)" type="text" readonly class="readonly">
        </div>

        <div class="field">
          <label for="lc-index">{{ tr('Rate Index') }}</label>
          <select id="lc-index" v-model="form.rateIndexId" @change="applyIndexRate">
            <option :value="null">-- {{ tr('Entered by hand') }} --</option>
            <option v-for="r in rateOptions" :key="r.rateIndexId" :value="r.rateIndexId">
              {{ r.rateIndexName }} ({{ r.ratePct === null ? tr('no rate on this date') : `${r.ratePct}%` }})
            </option>
          </select>
        </div>

        <div class="field">
          <label for="lc-rate">{{ tr('Interest Rate %') }}</label>
          <input id="lc-rate" v-model.number="form.interestRatePct" type="number" min="0" step="0.0001">
          <small v-if="indexRateNote" class="field-note">{{ indexRateNote }}</small>
        </div>

        <div class="field">
          <label for="lc-daycount">{{ tr('Day Count Basis') }}</label>
          <select id="lc-daycount" v-model="form.dayCountMethod">
            <option v-for="item in dayCountMethods" :key="item.value" :value="item.value">{{ tr(item.label) }}</option>
          </select>
        </div>

        <div class="field">
          <label for="lc-mode">{{ tr('Schedule Type') }}</label>
          <select id="lc-mode" v-model="form.scheduleMode">
            <option value="SEPARATE">{{ tr('Separate Principal and Interest') }}</option>
            <option value="COMBINED">{{ tr('Combined Principal + Interest') }}</option>
          </select>
        </div>
      </div>
    </section>

    <section class="loan-card principal-interest-card">
      <div class="loan-section-title"><span class="fa fa-calendar" aria-hidden="true"></span>{{ tr('Principal and Interest Rules') }}</div>

      <!-- Separate P&I: the calculation stays on the server -->
      <template v-if="!isCombined">
        <div class="schedule-subtitle">
          <strong>{{ tr('Regular Principal Schedule') }}</strong>
          <span>{{ tr('Principal is divided equally across all principal installments.') }}</span>
        </div>

        <div class="loan-grid">
          <div class="field">
            <label for="lc-pstart">{{ tr('Principal Start Date') }}</label>
            <input id="lc-pstart" v-model="form.principalPaymentStartDate" type="date">
          </div>

          <div class="field">
            <label for="lc-pfreq">{{ tr('Principal Repayment Frequency') }}</label>
            <div class="inline-field">
              <input id="lc-pfreq" v-model.number="form.principalFrequencyValue" type="number" min="1">
              <select v-model="form.principalFrequencyUnit" :aria-label="tr('Principal frequency unit')">
                <option v-for="item in SeparateFrequencyUnits" :key="item.value" :value="item.value">{{ tr(item.label) }}</option>
              </select>
            </div>
          </div>

          <div class="field">
            <label for="lc-pend">{{ tr('Principal End Date') }}</label>
            <input id="lc-pend" :value="formatDisplayDate(principalScheduleEndDate)" type="text" readonly class="readonly">
          </div>
        </div>

        <div class="schedule-divider"></div>

        <div class="schedule-subtitle">
          <strong>{{ tr('Regular Interest Schedule') }}</strong>
          <span>{{ tr('Interest is calculated on the outstanding principal balance.') }}</span>
        </div>

        <div class="loan-grid">
          <div class="field">
            <label for="lc-istart">{{ tr('Interest Start Date') }}</label>
            <input id="lc-istart" v-model="form.interestPaymentStartDate" type="date">
          </div>

          <div class="field">
            <label for="lc-ifreq">{{ tr('Interest Payment Frequency') }}</label>
            <div class="inline-field">
              <input id="lc-ifreq" v-model.number="form.interestFrequencyValue" type="number" min="1">
              <select v-model="form.interestFrequencyUnit" :aria-label="tr('Interest frequency unit')">
                <option v-for="item in SeparateFrequencyUnits" :key="item.value" :value="item.value">{{ tr(item.label) }}</option>
              </select>
            </div>
          </div>

          <div class="field">
            <label for="lc-iend">{{ tr('Interest End Date') }}</label>
            <input id="lc-iend" :value="formatDisplayDate(interestScheduleEndDate)" type="text" readonly class="readonly">
          </div>
        </div>
      </template>

      <!-- Combined -->
      <template v-else>
        <div class="loan-grid">
          <div class="field">
            <label for="lc-ptype">{{ tr('Payment Type') }}</label>
            <select id="lc-ptype" v-model="form.principalMethod">
              <option value="EQUAL_PRINCIPAL">{{ tr('Equal Principal') }}</option>
              <option value="EQUAL_INSTALLMENT">{{ tr('Equal Total Installment') }}</option>
            </select>
          </div>

          <div v-if="!combinedAtMaturity" class="field">
            <label for="lc-first">{{ tr('First Payment Date') }}</label>
            <input id="lc-first" v-model="form.principalPaymentStartDate" type="date">
          </div>
        </div>

        <div class="loan-grid subsection">
          <div class="field">
            <label for="lc-cfreq">{{ tr('Repayment Frequency') }}</label>
            <div class="inline-field">
              <input v-if="!combinedAtMaturity" id="lc-cfreq" v-model.number="form.combinedFrequencyValue" type="number" min="1">
              <div v-else class="frequency-value-placeholder">—</div>
              <select v-model="form.combinedFrequencyUnit" :aria-label="tr('Repayment frequency unit')">
                <option v-for="item in RepaymentFrequencyUnits" :key="item.value" :value="item.value">{{ tr(item.label) }}</option>
              </select>
            </div>
          </div>

          <div v-if="combinedAtMaturity" class="maturity-note">
            <span class="fa fa-calendar maturity-note-icon" aria-hidden="true"></span>
            <span>{{ tr('Principal and interest will both be paid on') }} {{ formatDisplayDate(maturityDate) }}.</span>
          </div>
        </div>
      </template>

      <div class="actions">
        <button type="button" class="fx-btn fx-btn--teal" :disabled="loading" @click="generateSchedule">
          <span class="fa" :class="loading ? 'fa-spinner fa-spin' : 'fa-calculator'" aria-hidden="true"></span>{{ tr('Generate Schedule') }}
        </button>
      </div>
    </section>

    <template v-if="workingResult">
      <section class="loan-card">
        <div class="loan-section-title"><span class="fa fa-percent" aria-hidden="true"></span>{{ tr('Interest Rate History') }}</div>

        <div class="rate-layout">
          <div class="table-wrap">
            <table class="loan-table">
              <thead>
                <tr><th>#</th><th>{{ tr('Effective Date') }}</th><th>{{ tr('Interest Rate %') }}</th></tr>
              </thead>
              <tbody>
                <tr v-for="rate in workingResult.rates" :key="`${rate.sequenceNo}-${rate.effectiveDate}`">
                  <td>{{ rate.sequenceNo }}</td>
                  <td>{{ formatDisplayDate(rate.effectiveDate) }}</td>
                  <td class="num">{{ rate.interestRatePct }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <div class="rate-change-box">
            <div class="rate-change-title">{{ tr('Working Rate Change') }}</div>
            <div class="field">
              <label for="lc-reff">{{ tr('Effective Date') }}</label>
              <input id="lc-reff" v-model="rateEffectiveDate" type="date">
            </div>
            <div class="field">
              <label for="lc-rnew">{{ tr('New Interest Rate %') }}</label>
              <input id="lc-rnew" v-model.number="newRatePct" type="number" min="0" step="0.0001">
            </div>
            <button type="button" class="fx-btn" :disabled="loading" @click="regenerateForRateChange">
              <span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Regenerate Schedule') }}
            </button>
            <div class="working-note">{{ tr('This changes the schedule on screen only. Nothing is saved until Save/Update Scenario is clicked.') }}</div>
          </div>
        </div>
      </section>

      <section class="loan-card">
        <div class="save-scenario-bar">
          <div class="save-scenario-heading">
            <span class="fa fa-floppy-o" aria-hidden="true"></span>
            <div>
              <strong>{{ tr(isUpdateMode ? 'Update Scenario' : 'Save New Scenario') }}</strong>
              <span>{{ tr(isUpdateMode ? 'The existing scenario will be replaced with the current screen values.'
                : 'Assign a unique name to save the current working scenario.') }}</span>
            </div>
          </div>
          <div class="save-scenario-controls">
            <input v-model="saveScenarioName" type="text" maxlength="200" :placeholder="tr('Unique scenario name')" :aria-label="tr('Scenario name')">
            <button type="button" class="fx-btn fx-btn--teal" :disabled="loading" @click="saveScenario">
              <span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr(isUpdateMode ? 'Update Scenario' : 'Save Scenario') }}
            </button>
          </div>
        </div>

        <div class="loan-section-title schedule-title"><span class="fa fa-table" aria-hidden="true"></span>{{ tr('Repayment Schedule') }}</div>

        <div class="table-wrap schedule-table-wrap">
          <table class="loan-table schedule-table">
            <thead>
              <tr>
                <th>#</th><th>{{ tr('Payment Date') }}</th><th>{{ tr('Accrual From') }}</th><th>{{ tr('Accrual To') }}</th><th>{{ tr('Accrual Days') }}</th>
                <th>{{ tr('Opening Principal') }}</th><th>{{ tr('Principal') }}</th><th>{{ tr('Interest') }}</th><th>{{ tr('Total Payment') }}</th>
                <th>{{ tr('Closing Principal') }}</th><th>{{ tr('Rate %') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="line in workingResult.schedule" :key="line.installmentNo">
                <td>{{ line.installmentNo }}</td>
                <td>{{ formatDisplayDate(line.paymentDate) }}</td>
                <td>{{ formatDisplayDate(line.accrualStartDate) }}</td>
                <td>{{ formatDisplayDate(line.accrualEndDate) }}</td>
                <td class="accrual-days-cell">{{ line.accrualDays }}</td>
                <td class="num">{{ amount(line.openingPrincipal) }}</td>
                <td class="num">{{ amount(line.principalAmount) }}</td>
                <td class="num">{{ amount(line.interestAmount) }}</td>
                <td class="num strong-num">{{ amount(line.totalPaymentAmount) }}</td>
                <td class="num strong-num">{{ amount(line.closingPrincipal) }}</td>
                <td class="num">{{ line.interestRatePct }}</td>
              </tr>
            </tbody>
            <tfoot>
              <tr>
                <td colspan="6">{{ tr('Total') }}</td>
                <td class="num strong-num">{{ amount(total('principalAmount')) }}</td>
                <td class="num strong-num">{{ amount(total('interestAmount')) }}</td>
                <td class="num strong-num">{{ amount(total('totalPaymentAmount')) }}</td>
                <td colspan="2"></td>
              </tr>
            </tfoot>
          </table>
        </div>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, nextTick, onMounted, reactive, ref, watch } from 'vue'
import { RouterLink } from 'vue-router'
import { tr } from '@/shared/layout/language'
import {
  loanScenariosApi, type LoanScenarioListItem, type LoanScenarioResult, type LoanScenarioWorkspaceRate,
  type LoanScenarioWorkspaceRequest, type LoanScheduleLine, type RateOption
} from '../api/loanScenarios.api'

// dates are yyyy-MM-dd strings ('' when empty), as the date inputs give them
type LoanScenarioForm = {
  principalAmount: number
  contractDate: string
  tenorValue: number
  tenorUnit: string
  rateIndexId: number | null
  interestRatePct: number
  dayCountMethod: string
  scheduleMode: string
  principalMethod: string
  interestPaymentMethod: string
  principalPaymentStartDate: string
  interestPaymentStartDate: string
  principalFrequencyValue: number | null
  principalFrequencyUnit: string | null
  principalDateRule: string | null
  interestFrequencyValue: number | null
  interestFrequencyUnit: string | null
  interestDateRule: string | null
  combinedFrequencyValue: number | null
  combinedFrequencyUnit: string | null
  combinedDateRule: string | null
}

const pad = (n: number) => String(n).padStart(2, '0')
const ymd = (d: Date) => `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`
const today = () => ymd(new Date())

function parseApiDate(value: string | null | undefined): Date | null {
  if (!value) return null
  const [year, month, day] = value.substring(0, 10).split('-').map(Number)
  if (!year || !month || !day) return null
  return new Date(year, month - 1, day)
}

const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec']

function formatDisplayDate(value: string | null | undefined): string {
  const d = parseApiDate(value)
  return d ? `${pad(d.getDate())}-${MONTHS[d.getMonth()] ?? ''}-${d.getFullYear()}` : (value ?? '')
}

function amount(value: number) {
  return new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value ?? 0)
}

const endOfMonth = (year: number, month: number) => new Date(year, month, 0)

function day30OrMonthEnd(year: number, month: number): Date {
  const lastDay = new Date(year, month, 0).getDate()
  return new Date(year, month - 1, Math.min(30, lastDay))
}

function addMonthsSafe(date: Date, months: number): Date {
  const first = new Date(date.getFullYear(), date.getMonth() + months, 1)
  const lastDay = new Date(first.getFullYear(), first.getMonth() + 1, 0).getDate()
  return new Date(first.getFullYear(), first.getMonth(), Math.min(date.getDate(), lastDay))
}

/** The first payment date the page proposes for a frequency, as FINOVA does. */
function calculateDefaultPaymentDate(contractValue: string, frequencyValue: number | null | undefined,
  frequencyUnit: string | null | undefined, maturityValue?: string): string {
  const contractDate = parseApiDate(contractValue)
  if (!contractDate || !frequencyUnit) return ''
  const unit = frequencyUnit.toUpperCase()
  const value = Number(frequencyValue ?? 1)
  const y = contractDate.getFullYear()
  const m = contractDate.getMonth()
  switch (unit) {
    case 'MATURITY': return maturityValue ?? ''
    case 'DAY': return ymd(new Date(y, m, contractDate.getDate() + value))
    case 'WEEK': return ymd(new Date(y, m, contractDate.getDate() + value * 7))
    case 'MONTH': return ymd(addMonthsSafe(contractDate, value))
    case 'YEAR': return ymd(addMonthsSafe(contractDate, value * 12))
    case 'EOM': return ymd(endOfMonth(y, m + 1))
    case 'NEXT_EOM':
    case 'EOMS': return ymd(endOfMonth(y, m + 2))
    case 'EOM_30': return ymd(day30OrMonthEnd(y, m + 1))
    case 'NEXT_EOM_30': return ymd(day30OrMonthEnd(y, m + 2))
    default: return ''
  }
}

const ContractingFrequencyUnits = [
  { value: 'DAY', label: 'Day(s)' },
  { value: 'WEEK', label: 'Week(s)' },
  { value: 'MONTH', label: 'Month(s)' },
  { value: 'YEAR', label: 'Year(s)' }
]

const SeparateFrequencyUnits = [
  ...ContractingFrequencyUnits,
  { value: 'EOM', label: 'EOM - Current Month' },
  { value: 'NEXT_EOM', label: 'EOM - Next Month' },
  { value: 'EOM_30', label: '30th - Start Current Month' },
  { value: 'NEXT_EOM_30', label: '30th - Start Next Month' }
]

const RepaymentFrequencyUnits = [...SeparateFrequencyUnits, { value: 'MATURITY', label: 'Maturity' }]

const dayCountMethods = [
  { value: 'ACT_ACT', label: 'Actual / Actual' },
  { value: 'ACT_360', label: 'Actual / 360' },
  { value: 'ACT_365', label: 'Actual / 365' },
  { value: '30_ACT', label: '30 / Actual' },
  { value: '30_360', label: '30 / 360' },
  { value: '30_365', label: '30 / 365' }
]

const loading = ref(false)
const successMessage = ref('')
const errorMessage = ref('')

const savedScenarios = ref<LoanScenarioListItem[]>([])
const selectedSavedScenarioId = ref<number | null>(null)
const loadedScenarioId = ref<number | null>(null)

const workingResult = ref<LoanScenarioResult | null>(null)
const workingRates = ref<LoanScenarioWorkspaceRate[]>([])

const saveScenarioName = ref('')
const rateEffectiveDate = ref('')
const newRatePct = ref(0)

const suspendDefaults = ref(false)

// the rate indexes of Administration > Rate Maintenance, with their rate valid on the contract date
const rateOptions = ref<RateOption[]>([])

const form = reactive<LoanScenarioForm>({
  principalAmount: 0,
  contractDate: today(),
  tenorValue: 12,
  tenorUnit: 'MONTH',
  rateIndexId: null,
  interestRatePct: 0,
  dayCountMethod: 'ACT_365',
  scheduleMode: 'SEPARATE',
  principalMethod: 'EQUAL_PRINCIPAL',
  interestPaymentMethod: 'REGULAR',
  principalPaymentStartDate: '',
  interestPaymentStartDate: '',
  principalFrequencyValue: 1,
  principalFrequencyUnit: 'MONTH',
  principalDateRule: 'ACTUAL',
  interestFrequencyValue: 1,
  interestFrequencyUnit: 'MONTH',
  interestDateRule: 'ACTUAL',
  combinedFrequencyValue: 1,
  combinedFrequencyUnit: 'MONTH',
  combinedDateRule: 'ACTUAL'
})

const isCombined = computed(() => form.scheduleMode === 'COMBINED')
const isUpdateMode = computed(() => (loadedScenarioId.value ?? 0) > 0)
const combinedAtMaturity = computed(() => isCombined.value && form.combinedFrequencyUnit === 'MATURITY')

const selectedIndex = computed(() => rateOptions.value.find((r) => r.rateIndexId === form.rateIndexId) ?? null)

/** Says when the rate differs from the index's rate on the contract date, so a changed rate is never a surprise. */
const indexRateNote = computed(() => {
  const r = selectedIndex.value
  if (!r) return ''
  if (r.ratePct === null) return `${r.rateIndexName}: ${tr('no rate on this date')}`
  return Number(form.interestRatePct) === r.ratePct ? '' : `${tr('Changed from')} ${r.ratePct}% (${r.rateIndexName})`
})

const maturityDate = computed(() => {
  const contractDate = parseApiDate(form.contractDate)
  if (!contractDate || !form.tenorValue || !form.tenorUnit) return ''
  const tenor = Number(form.tenorValue)
  switch (form.tenorUnit) {
    case 'DAY': return ymd(new Date(contractDate.getFullYear(), contractDate.getMonth(), contractDate.getDate() + tenor))
    case 'WEEK': return ymd(new Date(contractDate.getFullYear(), contractDate.getMonth(), contractDate.getDate() + tenor * 7))
    case 'MONTH': return ymd(addMonthsSafe(contractDate, tenor))
    case 'YEAR': return ymd(addMonthsSafe(contractDate, tenor * 12))
    default: return ''
  }
})

const principalScheduleEndDate = computed(() => (!isCombined.value && form.principalPaymentStartDate ? maturityDate.value : ''))
const interestScheduleEndDate = computed(() => (!isCombined.value && form.interestPaymentStartDate ? maturityDate.value : ''))

function total(key: 'principalAmount' | 'interestAmount' | 'totalPaymentAmount') {
  return (workingResult.value?.schedule ?? []).reduce((s: number, l: LoanScheduleLine) => s + l[key], 0)
}

function clearMessages() {
  successMessage.value = ''
  errorMessage.value = ''
}

const message = (error: unknown, fallback: string) => (error instanceof Error ? error.message : tr(fallback))

function applyDefaultDates() {
  if (isCombined.value) {
    form.principalPaymentStartDate = calculateDefaultPaymentDate(form.contractDate, form.combinedFrequencyValue, form.combinedFrequencyUnit, maturityDate.value)
    form.interestPaymentStartDate = form.principalPaymentStartDate
    return
  }
  form.principalPaymentStartDate = calculateDefaultPaymentDate(form.contractDate, form.principalFrequencyValue, form.principalFrequencyUnit, maturityDate.value)
  form.interestPaymentStartDate = calculateDefaultPaymentDate(form.contractDate, form.interestFrequencyValue, form.interestFrequencyUnit, maturityDate.value)
}

watch(
  [() => form.contractDate, () => form.scheduleMode, () => form.combinedFrequencyValue, () => form.combinedFrequencyUnit, () => maturityDate.value],
  () => {
    if (suspendDefaults.value || !isCombined.value) return
    form.principalPaymentStartDate = calculateDefaultPaymentDate(form.contractDate, form.combinedFrequencyValue, form.combinedFrequencyUnit, maturityDate.value)
    form.interestPaymentStartDate = form.principalPaymentStartDate
  }
)

watch(
  [() => form.contractDate, () => form.scheduleMode, () => form.principalFrequencyValue, () => form.principalFrequencyUnit, () => maturityDate.value],
  () => {
    if (suspendDefaults.value || isCombined.value) return
    form.principalPaymentStartDate = calculateDefaultPaymentDate(form.contractDate, form.principalFrequencyValue, form.principalFrequencyUnit, maturityDate.value)
  }
)

watch(
  [() => form.contractDate, () => form.scheduleMode, () => form.interestFrequencyValue, () => form.interestFrequencyUnit, () => maturityDate.value],
  () => {
    if (suspendDefaults.value || isCombined.value) return
    form.interestPaymentStartDate = calculateDefaultPaymentDate(form.contractDate, form.interestFrequencyValue, form.interestFrequencyUnit, maturityDate.value)
  }
)

/** Puts the selected index's rate valid on the contract date into Interest Rate % (the user may change it afterwards). */
function applyIndexRate() {
  const r = selectedIndex.value
  if (r && r.ratePct !== null) form.interestRatePct = r.ratePct
}

/**
 * Reads the indexes' rates valid on the contract date. 'pick' (a new scenario) takes the first active index with a rate on the
 * date; 'follow' (the contract date changed) keeps the chosen index and takes its rate on the new date; 'keep' (a saved scenario)
 * leaves the rate as saved.
 */
async function loadRates(mode: 'pick' | 'follow' | 'keep') {
  if (!form.contractDate) return
  try {
    rateOptions.value = await loanScenariosApi.rates(form.contractDate)
  } catch (error) {
    errorMessage.value = message(error, 'Unable to read the rates of Rate Maintenance.')
    return
  }
  if (mode === 'keep') return
  if (mode === 'pick') form.rateIndexId = rateOptions.value.find((r) => r.ratePct !== null)?.rateIndexId ?? null
  applyIndexRate()
}

watch(() => form.contractDate, () => {
  if (!suspendDefaults.value) void loadRates('follow')
})

function normalizedRatesForRequest(): LoanScenarioWorkspaceRate[] {
  const contractDate = form.contractDate
  const futureRates = workingRates.value
    .filter((x) => x.effectiveDate.substring(0, 10) > contractDate)
    .map((x) => ({ effectiveDate: x.effectiveDate.substring(0, 10), interestRatePct: Number(x.interestRatePct) }))
  return [{ effectiveDate: contractDate, interestRatePct: Number(form.interestRatePct) }, ...futureRates]
}

function buildWorkspaceRequest(scenarioName: string | null = null): LoanScenarioWorkspaceRequest {
  const request: LoanScenarioWorkspaceRequest = {
    scenarioId: loadedScenarioId.value,
    scenarioName,
    rateIndexId: form.rateIndexId,
    principalAmount: Number(form.principalAmount),
    contractDate: form.contractDate,
    tenorValue: Number(form.tenorValue),
    tenorUnit: form.tenorUnit,
    interestRatePct: Number(form.interestRatePct),
    dayCountMethod: form.dayCountMethod,
    scheduleMode: form.scheduleMode,
    principalMethod: form.principalMethod,
    interestPaymentMethod: 'REGULAR',
    principalPaymentStartDate: form.principalPaymentStartDate,
    interestPaymentStartDate: form.interestPaymentStartDate,
    principalFrequencyValue: form.principalFrequencyValue,
    principalFrequencyUnit: form.principalFrequencyUnit,
    principalDateRule: form.principalDateRule,
    interestFrequencyValue: form.interestFrequencyValue,
    interestFrequencyUnit: form.interestFrequencyUnit,
    interestDateRule: form.interestDateRule,
    combinedFrequencyValue: form.combinedFrequencyValue,
    combinedFrequencyUnit: form.combinedFrequencyUnit,
    combinedDateRule: form.combinedDateRule,
    rates: normalizedRatesForRequest()
  }

  if (isCombined.value) {
    request.principalFrequencyValue = null
    request.principalFrequencyUnit = null
    request.principalDateRule = null
    request.interestFrequencyValue = null
    request.interestFrequencyUnit = null
    request.interestDateRule = null
    request.combinedDateRule = null
    if (combinedAtMaturity.value) {
      request.combinedFrequencyValue = null
      request.principalPaymentStartDate = maturityDate.value
      request.interestPaymentStartDate = maturityDate.value
    } else {
      request.interestPaymentStartDate = request.principalPaymentStartDate
    }
  } else {
    request.principalMethod = 'EQUAL_PRINCIPAL'
    request.principalDateRule = 'ACTUAL'
    request.interestDateRule = 'ACTUAL'
    request.combinedFrequencyValue = null
    request.combinedFrequencyUnit = null
    request.combinedDateRule = null
  }
  return request
}

function checkDates() {
  if (!form.contractDate) throw new Error(tr('Enter the contract date.'))
  if (!form.principalPaymentStartDate || !form.interestPaymentStartDate) throw new Error(tr('Enter the payment start dates.'))
}

async function refreshScenarioList(selectId?: number | null) {
  savedScenarios.value = await loanScenariosApi.list()
  if (selectId) selectedSavedScenarioId.value = selectId
}

async function generateSchedule() {
  clearMessages()
  try {
    checkDates()
    loading.value = true
    workingResult.value = await loanScenariosApi.preview(buildWorkspaceRequest(saveScenarioName.value || null))
    successMessage.value = tr('Schedule generated on screen. Nothing has been saved.')
  } catch (error) {
    errorMessage.value = message(error, 'Unable to generate schedule.')
  } finally {
    loading.value = false
  }
}

function upsertWorkingRate() {
  const effectiveDate = rateEffectiveDate.value
  if (!effectiveDate) throw new Error(tr('Enter the rate effective date.'))
  if (effectiveDate <= form.contractDate) throw new Error(tr('Rate change effective date must be after the contract date.'))
  if (maturityDate.value && effectiveDate > maturityDate.value) throw new Error(tr('Rate change effective date cannot be after maturity.'))
  const rate = Number(newRatePct.value)
  if (rate < 0) throw new Error(tr('Interest rate cannot be negative.'))
  const item: LoanScenarioWorkspaceRate = { effectiveDate, interestRatePct: rate }
  const existingIndex = workingRates.value.findIndex((x) => x.effectiveDate.substring(0, 10) === effectiveDate)
  if (existingIndex >= 0) workingRates.value.splice(existingIndex, 1, item)
  else workingRates.value.push(item)
  workingRates.value.sort((a, b) => a.effectiveDate.localeCompare(b.effectiveDate))
}

async function regenerateForRateChange() {
  clearMessages()
  try {
    upsertWorkingRate()
    await generateSchedule()
    rateEffectiveDate.value = ''
    newRatePct.value = 0
  } catch (error) {
    errorMessage.value = message(error, 'Unable to regenerate schedule.')
  }
}

async function saveScenario() {
  clearMessages()
  const name = saveScenarioName.value.trim()
  if (!name) {
    errorMessage.value = tr('Enter a unique scenario name before saving.')
    return
  }
  try {
    checkDates()
    loading.value = true
    const wasUpdate = isUpdateMode.value
    const result = await loanScenariosApi.save(buildWorkspaceRequest(name))
    workingResult.value = result
    loadedScenarioId.value = result.scenarioId
    saveScenarioName.value = result.scenarioName
    await refreshScenarioList(result.scenarioId)
    successMessage.value = `${tr(wasUpdate ? 'Scenario updated:' : 'Scenario saved:')} ${result.scenarioName}`
  } catch (error) {
    errorMessage.value = message(error, 'Unable to save scenario.')
  } finally {
    loading.value = false
  }
}

async function applyLoadedScenario(scenario: LoanScenarioResult) {
  suspendDefaults.value = true
  loadedScenarioId.value = scenario.scenarioId
  saveScenarioName.value = scenario.scenarioName
  form.principalAmount = scenario.principalAmount
  form.contractDate = scenario.contractDate.substring(0, 10)
  form.tenorValue = scenario.tenorValue
  form.tenorUnit = scenario.tenorUnit
  form.rateIndexId = scenario.rateIndexId
  form.interestRatePct = scenario.rates[0]?.interestRatePct ?? 0
  form.dayCountMethod = scenario.dayCountMethod
  form.scheduleMode = scenario.scheduleMode
  form.principalMethod = scenario.principalMethod
  form.interestPaymentMethod = 'REGULAR'
  form.principalPaymentStartDate = scenario.principalPaymentStartDate.substring(0, 10)
  form.interestPaymentStartDate = scenario.interestPaymentStartDate.substring(0, 10)
  form.principalFrequencyValue = scenario.principalFrequencyValue
  form.principalFrequencyUnit = scenario.principalFrequencyUnit
  form.principalDateRule = scenario.principalDateRule
  form.interestFrequencyValue = scenario.interestFrequencyValue
  form.interestFrequencyUnit = scenario.interestFrequencyUnit
  form.interestDateRule = scenario.interestDateRule
  form.combinedFrequencyValue = scenario.combinedFrequencyValue
  form.combinedFrequencyUnit = scenario.combinedFrequencyUnit
  form.combinedDateRule = scenario.combinedDateRule
  workingRates.value = scenario.rates.map((x) => ({ effectiveDate: x.effectiveDate.substring(0, 10), interestRatePct: x.interestRatePct }))
  workingResult.value = scenario
  // the saved rate stays as saved; the list only shows the indexes' rates on that date
  await loadRates('keep')
  await nextTick()
  suspendDefaults.value = false
}

async function resetNewScenario() {
  suspendDefaults.value = true
  loadedScenarioId.value = null
  saveScenarioName.value = ''
  workingResult.value = null
  workingRates.value = []
  form.principalAmount = 0
  form.contractDate = today()
  form.tenorValue = 12
  form.tenorUnit = 'MONTH'
  form.rateIndexId = null
  form.interestRatePct = 0
  form.dayCountMethod = 'ACT_365'
  form.scheduleMode = 'SEPARATE'
  form.principalMethod = 'EQUAL_PRINCIPAL'
  form.interestPaymentMethod = 'REGULAR'
  form.principalFrequencyValue = 1
  form.principalFrequencyUnit = 'MONTH'
  form.principalDateRule = 'ACTUAL'
  form.interestFrequencyValue = 1
  form.interestFrequencyUnit = 'MONTH'
  form.interestDateRule = 'ACTUAL'
  form.combinedFrequencyValue = 1
  form.combinedFrequencyUnit = 'MONTH'
  form.combinedDateRule = 'ACTUAL'
  await nextTick()
  suspendDefaults.value = false
  applyDefaultDates()
  // a new scenario opens with today's rate of Rate Maintenance
  await loadRates('pick')
}

async function onSavedScenarioChanged() {
  clearMessages()
  if (!selectedSavedScenarioId.value) {
    await resetNewScenario()
    return
  }
  try {
    loading.value = true
    const scenario = await loanScenariosApi.get(selectedSavedScenarioId.value)
    await applyLoadedScenario(scenario)
    successMessage.value = `${tr('Scenario loaded in update mode:')} ${scenario.scenarioName}`
  } catch (error) {
    errorMessage.value = message(error, 'Unable to load scenario.')
  } finally {
    loading.value = false
  }
}

onMounted(async () => {
  try {
    loading.value = true
    await refreshScenarioList()
    await resetNewScenario()
  } catch (error) {
    errorMessage.value = message(error, 'Unable to load saved scenarios.')
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.loan-page {
  --lc-text: #1f2d33;
  --lc-muted: #64748b;
  --lc-border: #dbe4e8;
  --lc-primary: #00636d;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.loan-sub {
  margin: 4px 0 0;
  color: var(--lc-muted);
  font-size: 13px;
}

.scenario-selector {
  min-width: 430px;
  display: grid;
  grid-template-columns: minmax(220px, 1fr) auto;
  column-gap: 10px;
  row-gap: 5px;
  align-items: center;
}

.scenario-selector label {
  grid-column: 1 / -1;
  font-size: 12px;
  font-weight: 800;
  color: var(--lc-text);
}

.scenario-selector select {
  height: 40px;
  width: 100%;
  padding: 0 10px;
  border: 1px solid #cbd9de;
  border-radius: 8px;
  background: #fff;
  color: var(--lc-text);
  font-size: 12px;
}

.mode-badge {
  display: inline-flex;
  align-items: center;
  height: 30px;
  padding: 0 11px;
  border-radius: 999px;
  background: #eff6ff;
  color: #1d4ed8;
  font-size: 11px;
  font-weight: 800;
  white-space: nowrap;
}

.mode-badge.update {
  background: #fff7ed;
  color: #c2410c;
}

.loan-card {
  background: #fff;
  border: 1px solid var(--lc-border);
  border-radius: 12px;
  box-shadow: 0 1px 3px rgba(15, 23, 42, 0.06);
  padding: 18px 24px;
}

.loan-section-title {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 15px;
  color: var(--lc-text);
  font-size: 14px;
  font-weight: 800;
}

.loan-section-title .fa {
  color: var(--lc-primary);
}

.principal-interest-card > .loan-section-title {
  margin-bottom: 28px;
}

.loan-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 14px 20px;
}

.subsection {
  margin-top: 14px;
  padding-top: 14px;
  border-top: 1px solid #edf2f4;
}

.schedule-subtitle {
  display: flex;
  align-items: baseline;
  gap: 14px;
  margin: 2px 0 12px;
  color: var(--lc-text);
}

.schedule-subtitle strong {
  font-size: 13px;
}

.schedule-subtitle span {
  color: var(--lc-muted);
  font-size: 11.5px;
}

.schedule-divider {
  height: 1px;
  margin: 26px 0;
  background: #e7eef1;
}

.field label {
  display: block;
  margin-bottom: 5px;
  font-size: 12px;
  font-weight: 700;
  color: var(--lc-text);
}

.field input,
.field select {
  width: 100%;
  height: 38px;
  box-sizing: border-box;
  border: 1px solid #d7e1e5;
  border-radius: 8px;
  padding: 0 10px;
  background: #fff;
  color: var(--lc-text);
  font-size: 12px;
}

.field input.readonly {
  background: #f5f7f8;
  color: var(--lc-muted);
}

.field input:focus,
.field select:focus,
.scenario-selector select:focus {
  outline: none;
  border-color: var(--lc-primary);
  box-shadow: 0 0 0 3px rgba(0, 99, 109, 0.1);
}

.field-note {
  display: block;
  margin-top: 4px;
  color: #b45309;
  font-size: 11px;
}

.inline-field {
  display: grid;
  grid-template-columns: 0.7fr 1.3fr;
  gap: 7px;
}

.frequency-value-placeholder {
  height: 38px;
  display: flex;
  align-items: center;
  justify-content: center;
  border: 1px solid #d7e1e5;
  border-radius: 8px;
  background: #f5f7f8;
  color: var(--lc-muted);
}

.maturity-note {
  display: flex;
  align-items: center;
  align-self: end;
  min-height: 38px;
  color: #15803d;
  font-size: 12px;
  font-style: italic;
}

.maturity-note-icon {
  margin-inline-end: 8px;
  color: #16a34a;
  font-size: 17px;
  font-style: normal;
}

.actions {
  display: flex;
  justify-content: flex-end;
  margin-top: 22px;
}

.loan-message {
  display: flex;
  gap: 8px;
  align-items: center;
  padding: 10px 12px;
  border-radius: 8px;
  font-size: 12px;
}

.loan-message.success {
  color: #166534;
  background: #f0fdf4;
  border: 1px solid #bbf7d0;
}

.loan-message.error {
  color: #b91c1c;
  background: #fef2f2;
  border: 1px solid #fecaca;
}

.table-wrap {
  width: 100%;
  overflow-x: auto;
}

.loan-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 12px;
}

.loan-table th {
  background: #f8fafc;
  color: #475569;
  text-align: start;
  padding: 9px 10px;
  border-bottom: 1px solid var(--lc-border);
  white-space: nowrap;
}

.loan-table td {
  color: var(--lc-text);
  padding: 8px 10px;
  border-bottom: 1px solid #edf2f4;
  white-space: nowrap;
}

.loan-table .num {
  text-align: end;
  font-variant-numeric: tabular-nums;
}

.rate-layout {
  display: grid;
  grid-template-columns: minmax(0, 1.5fr) minmax(300px, 0.5fr);
  gap: 18px;
}

.rate-change-box {
  border: 1px solid #dce8eb;
  border-radius: 10px;
  background: #fafcfc;
  padding: 14px;
}

.rate-change-title {
  margin-bottom: 12px;
  font-size: 13px;
  font-weight: 800;
  color: var(--lc-text);
}

.rate-change-box .field {
  margin-bottom: 10px;
}

.working-note {
  margin-top: 10px;
  color: var(--lc-muted);
  font-size: 11px;
  line-height: 1.45;
}

.save-scenario-bar {
  display: flex;
  justify-content: space-between;
  gap: 24px;
  align-items: center;
  margin-bottom: 20px;
  padding: 14px 16px;
  border: 1px solid #d9e5e8;
  border-radius: 10px;
  background: #f8fbfb;
}

.save-scenario-heading {
  display: flex;
  gap: 10px;
  align-items: flex-start;
}

.save-scenario-heading > .fa {
  margin-top: 2px;
  color: var(--lc-primary);
  font-size: 17px;
}

.save-scenario-heading strong,
.save-scenario-heading span {
  display: block;
}

.save-scenario-heading strong {
  margin-bottom: 3px;
  font-size: 13px;
  color: var(--lc-text);
}

.save-scenario-heading span {
  max-width: 620px;
  color: var(--lc-muted);
  font-size: 11.5px;
}

.save-scenario-controls {
  display: flex;
  gap: 8px;
  min-width: 480px;
}

.save-scenario-controls input {
  flex: 1;
  height: 38px;
  border: 1px solid #cbd9de;
  border-radius: 8px;
  padding: 0 11px;
  background: #fff;
  color: var(--lc-text);
  font-size: 12px;
}

.schedule-title {
  margin-top: 4px;
}

.schedule-table-wrap {
  border-radius: 10px;
  overflow-x: auto;
  padding-bottom: 2px;
}

.schedule-table {
  min-width: 1260px;
  border-collapse: separate;
  border-spacing: 0;
  border: 1px solid var(--lc-border);
  border-radius: 10px;
  overflow: hidden;
  background: #fff;
}

.schedule-table thead th {
  position: sticky;
  top: 0;
  z-index: 2;
  padding: 11px 12px;
  background: #eef5f6;
  color: #24404b;
  font-size: 11.5px;
  font-weight: 800;
  border-bottom: 1px solid #cfdde2;
  border-inline-end: 1px solid #dde7ea;
}

.schedule-table tbody td {
  padding: 10px 12px;
  border-bottom: 1px solid #e7eef1;
  border-inline-end: 1px solid #edf2f4;
  background: #fff;
}

.schedule-table tbody tr:nth-child(even) td {
  background: #f9fbfc;
}

.schedule-table tbody tr:hover td {
  background: #eef9f8;
}

.schedule-table tfoot td {
  padding: 10px 12px;
  background: #eef5f6;
  font-weight: 800;
}

.schedule-table .accrual-days-cell {
  text-align: center;
  font-weight: 700;
  color: #0f766e;
}

.schedule-table .strong-num {
  font-weight: 800;
}

@media (max-width: 1100px) {
  .loan-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .rate-layout {
    grid-template-columns: 1fr;
  }

  .save-scenario-bar {
    align-items: stretch;
    flex-direction: column;
  }

  .scenario-selector,
  .save-scenario-controls {
    min-width: 0;
    width: 100%;
  }
}

@media (max-width: 650px) {
  .loan-grid {
    grid-template-columns: 1fr;
  }

  .scenario-selector {
    grid-template-columns: 1fr;
  }

  .scenario-selector label {
    grid-column: 1;
  }

  .save-scenario-controls {
    flex-direction: column;
  }
}
</style>
