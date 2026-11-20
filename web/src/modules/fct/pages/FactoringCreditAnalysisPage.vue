<template>
  <!-- FINOVA_FINANCEING FactoringCreditAnalysisPage: the Credit Analysis step, in three tabs (financial analysis, credit scoring,
       credit memo). Each tab saves on its own; Save Progress saves all three together. -->
  <FctStepShell :agreement-id="Number(agreementId)" title="Credit Analysis" icon="fa-line-chart"
                subtitle="Review financial analysis, credit scoring and the final credit recommendation."
                summary-note="Factoring application under credit review" loading-text="Loading credit analysis..."
                :summary="page?.summary ?? null" :loading="loading">
    <template #actions>
      <button type="button" class="fx-btn fx-btn--primary" :disabled="saving || !page" @click="saveProgress">
        <span class="fa" :class="saving ? 'fa-spinner fa-spin' : 'fa-floppy-o'" aria-hidden="true"></span>{{ tr('Save Progress') }}</button>
      <FctStepActions :save="persistProgress" :disabled="saving || !page" />
    </template>

    <section v-if="page" class="fx-card fc-step-card">
      <div class="fc-tabs" role="tablist">
        <button v-for="t in tabs" :key="t.key" type="button" role="tab" class="fc-tab" :aria-selected="tab === t.key" @click="tab = t.key">
          <span class="fa" :class="t.icon" aria-hidden="true"></span>{{ tr(t.label) }}
        </button>
      </div>

      <template v-if="tab === 'financial'">
        <header class="fc-sec-head">
          <div><h2>{{ tr('Financial Analysis') }}</h2><p>{{ tr('Record the financial statement assessment and supporting analysis notes.') }}</p></div>
        </header>
        <div class="fc-grid fc-grid--3">
          <label class="fc-field"><span>{{ tr('FS Assessment Date') }}</span>
            <input :value="dayInput(financial.fsAssessmentDate)" class="fx-search" type="date" @input="financial.fsAssessmentDate = toDay(value($event))"></label>
          <label class="fc-field"><span>{{ tr('FS Assessment Result') }}</span>
            <select v-model="financial.fsAssessmentResult" class="fx-select">
              <option :value="null">{{ tr('Select result') }}</option>
              <option v-for="r in results" :key="r.value" :value="r.value">{{ tr(r.label) }}</option>
            </select></label>
          <label class="fc-field fc-full"><span>{{ tr('FS Notes') }}</span>
            <textarea v-model="financial.fsNotes" class="fx-search fc-textarea" rows="4" :placeholder="tr('Enter financial analysis notes...')"></textarea></label>
        </div>
        <div class="fc-form-actions">
          <p class="fc-note"><span class="fa fa-file-pdf-o" aria-hidden="true"></span><span><b>{{ tr('Financial Analysis Report') }}</b> · {{ tr('Use the agreement documents for the supporting report.') }}</span></p>
          <FctAgreementDocuments :agreement-id="Number(agreementId)" label="Upload Report" icon="fa-upload" />
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="saveFinancial">
            <span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save Analysis') }}</button>
        </div>
      </template>

      <template v-else-if="tab === 'scoring'">
        <header class="fc-sec-head">
          <div><h2>{{ tr('Credit Scoring') }}</h2><p>{{ tr('Maintain the risk-rating assessment recorded on the factoring agreement.') }}</p></div>
        </header>
        <div class="fc-grid fc-grid--3">
          <label class="fc-field"><span>{{ tr('Risk Rating Date') }}</span>
            <input :value="dayInput(risk.rrAssessmentDate)" class="fx-search" type="date" @input="risk.rrAssessmentDate = toDay(value($event))"></label>
          <label class="fc-field fc-span2"><span>{{ tr('Risk Rating Result') }}</span>
            <input v-model="risk.rrAssessmentResult" class="fx-search" type="text" maxlength="100" :placeholder="tr('Enter risk rating result')"></label>
          <label class="fc-field fc-full"><span>{{ tr('Risk Rating Notes') }}</span>
            <textarea v-model="risk.rrNotes" class="fx-search fc-textarea" rows="4" :placeholder="tr('Enter risk rating notes...')"></textarea></label>
        </div>
        <div class="fc-form-actions">
          <p class="fc-note"><span class="fa fa-file-pdf-o" aria-hidden="true"></span><span><b>{{ tr('Risk Rating Report') }}</b> · {{ tr('Use the agreement documents for the supporting report.') }}</span></p>
          <FctAgreementDocuments :agreement-id="Number(agreementId)" label="Upload Report" icon="fa-upload" />
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="saveRisk">
            <span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save Risk Rating') }}</button>
        </div>
      </template>

      <template v-else>
        <header class="fc-sec-head">
          <div><h2>{{ tr('Credit Memo') }}</h2><p>{{ tr('Record the recommended facility terms and final credit recommendation.') }}</p></div>
          <button type="button" class="fx-btn fx-btn--sm" @click="printMemo"><span class="fa fa-print" aria-hidden="true"></span>{{ tr('Print Credit Memo') }}</button>
        </header>
        <div class="fc-grid fc-grid--4">
          <label class="fc-field"><span>{{ tr('Credit Memo Date') }}</span>
            <input :value="dayInput(memo.creditMemoDate)" class="fx-search" type="date" @input="memo.creditMemoDate = toDay(value($event))"></label>
          <label class="fc-field"><span>{{ tr('Recommended Limit') }}</span><input v-model.number="memo.recommendedLimt" class="fx-search" type="number" min="0" step="0.01"></label>
          <label class="fc-field"><span>{{ tr('Recommended Advance Rate (%)') }}</span><input v-model.number="memo.recommendedAdvRate" class="fx-search" type="number" min="0" step="0.01"></label>
          <label class="fc-field"><span>{{ tr('Recommended Tenor (Months)') }}</span><input v-model.number="memo.recommendedTenorMth" class="fx-search" type="number" min="0"></label>
          <label class="fc-field fc-full"><span>{{ tr('Credit Notes') }}</span>
            <textarea v-model="memo.creditNotes" class="fx-search fc-textarea" rows="4" :placeholder="tr('Enter credit recommendation and conditions...')"></textarea></label>
        </div>
        <div class="fc-form-actions">
          <p class="fc-note"><span class="fa fa-file-pdf-o" aria-hidden="true"></span><span><b>{{ tr('Credit Approval Memo') }}</b> · {{ tr('Use the agreement documents for the supporting CAM.') }}</span></p>
          <FctAgreementDocuments :agreement-id="Number(agreementId)" label="Upload CAM" icon="fa-upload" />
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="saveMemo">
            <span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save Credit Memo') }}</button>
        </div>
      </template>
    </section>
  </FctStepShell>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import { useFeedback } from '@/shared/feedback/useFeedback'
import FctStepShell from '../components/FctStepShell.vue'
import FctStepActions from '../components/FctStepActions.vue'
import FctAgreementDocuments from '../components/FctAgreementDocuments.vue'
import { dayInput, errorText, formatAmount, formatDate, formatMonths, formatRate, num, printLetter, toDay } from '../components/stepFormat'
import { fctStepsApi, type FctCreditAnalysis, type FctCreditMemo, type FctFinancialAnalysis, type FctRiskRating } from '../api/fct.api'

const props = defineProps<{ agreementId: string }>()

const tabs = [
  { key: 'financial', label: 'Financial Analysis', icon: 'fa-area-chart' },
  { key: 'scoring', label: 'Credit Scoring', icon: 'fa-bar-chart' },
  { key: 'memo', label: 'Credit Memo', icon: 'fa-pencil-square-o' }
] as const
const results = [{ value: 'WEAK', label: 'Weak' }, { value: 'AVERAGE', label: 'Average' }, { value: 'STRONG', label: 'Strong' }]

const tab = ref<(typeof tabs)[number]['key']>('financial')
const page = ref<FctCreditAnalysis | null>(null)
const financial = reactive<FctFinancialAnalysis>({ fsAssessmentId: null, fsAssessmentDate: null, fsAssessmentResult: null, fsNotes: null })
const risk = reactive<FctRiskRating>({ rrAssessmentId: null, rrAssessmentDate: null, rrAssessmentResult: null, rrNotes: null })
const memo = reactive<FctCreditMemo>({ creditMemoDate: null, recommendedLimt: null, recommendedAdvRate: null, recommendedTenorMth: null, creditNotes: null })
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const success = ref('')
useFeedback({ errors: [error], notices: [success] })

const id = () => Number(props.agreementId)
const value = (e: Event) => (e.target as HTMLInputElement).value
const memoBody = (): FctCreditMemo => ({
  ...memo, recommendedLimt: num(memo.recommendedLimt), recommendedAdvRate: num(memo.recommendedAdvRate), recommendedTenorMth: num(memo.recommendedTenorMth)
})

function assign(p: FctCreditAnalysis) {
  page.value = p
  Object.assign(financial, p.financialAnalysis)
  Object.assign(risk, p.riskRating)
  Object.assign(memo, p.creditMemo)
}

// done is the pop-up after a save; the workflow actions save without one
async function run(job: () => Promise<void>, done: string | null, fallback: string): Promise<boolean> {
  saving.value = true
  error.value = ''
  success.value = ''
  try {
    await job()
    if (done) success.value = tr(done)
    return true
  } catch (e) {
    error.value = errorText(e, fallback)
    return false
  } finally {
    saving.value = false
  }
}

const saveFinancial = () => run(async () => { Object.assign(financial, await fctStepsApi.saveFinancialAnalysis(id(), financial)) },
  'Financial analysis saved.', 'Unable to save financial analysis.')
const saveRisk = () => run(async () => { Object.assign(risk, await fctStepsApi.saveRiskRating(id(), risk)) },
  'Risk rating saved.', 'Unable to save risk rating.')
const saveMemo = () => run(async () => { Object.assign(memo, await fctStepsApi.saveCreditMemo(id(), memoBody())) },
  'Credit memo saved.', 'Unable to save credit memo.')
const progress = (done: string | null) => run(async () => {
  assign(await fctStepsApi.saveCreditAnalysisProgress(id(), { financialAnalysis: financial, riskRating: risk, creditMemo: memoBody() }))
}, done, 'Unable to save credit analysis progress.')
const saveProgress = () => progress('Credit analysis progress saved.')
const persistProgress = () => progress(null)

function printMemo() {
  const s = page.value?.summary
  if (!s) return
  printLetter(tr('Credit Approval Memo'), `${tr('Agreement')} ${s.agreementNumber}`, [
    [tr('Client'), s.clientName || '-'],
    [tr('Requested Amount'), formatAmount(s.facilityLimit, s.currencyCode)],
    [tr('Credit Memo Date'), formatDate(memo.creditMemoDate)],
    [tr('Recommended Limit'), formatAmount(num(memo.recommendedLimt), s.currencyCode)],
    [tr('Recommended Advance Rate (%)'), formatRate(num(memo.recommendedAdvRate))],
    [tr('Recommended Tenor (Months)'), formatMonths(num(memo.recommendedTenorMth))]
  ], [[tr('Credit Notes'), memo.creditNotes || '-']])
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    assign(await fctStepsApi.creditAnalysis(id()))
  } catch (e) {
    error.value = errorText(e, 'Unable to load credit analysis.')
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>
