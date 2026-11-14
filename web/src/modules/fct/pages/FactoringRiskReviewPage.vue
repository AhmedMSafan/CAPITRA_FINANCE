<template>
  <!-- FINOVA_FINANCEING FactoringRiskReviewPage (APEX page 635): the Risk Review step. The three indicator cards come from the
       agreement; the risk officer records the memo date and notes. -->
  <FctStepShell :agreement-id="Number(agreementId)" title="Risk Review" icon="fa-shield"
                subtitle="Review the credit recommendation and record the risk officer assessment."
                summary-note="Factoring application under risk review" loading-text="Loading risk review..."
                :summary="page?.summary ?? null" :loading="loading">
    <template v-if="page">
      <FctProcessStrip :current="4" />

      <div class="fc-indicators">
        <article class="fx-card fc-indicator">
          <b>{{ page.indicators.creditBureauScore ?? '-' }}</b><span>{{ tr('Credit Score') }}</span>
          <small>{{ tr('Credit bureau score recorded on the agreement') }}</small>
        </article>
        <article class="fx-card fc-indicator fc-indicator--orange">
          <b>{{ formatAmount(page.summary.facilityLimit, page.summary.currencyCode) }}</b><span>{{ tr('Requested Facility') }}</span>
          <small>{{ page.indicators.recourseType || '-' }} · {{ formatMonths(page.indicators.tenorMonths) }}</small>
        </article>
        <article class="fx-card fc-indicator">
          <b>{{ page.indicators.rrAssessmentResult || '-' }}</b><span>{{ tr('Debtor Risk Grade') }}</span>
          <small>{{ tr('Risk-rating result recorded during credit review') }}</small>
        </article>
      </div>

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-clipboard"></span></span>
          <div><h2>{{ tr('Risk Review Results') }}</h2><p>{{ tr('Record the risk officer review notes and memo date.') }}</p></div>
        </header>
        <div class="fc-grid fc-grid--3">
          <label class="fc-field"><span>{{ tr('Risk Memo Date') }}</span>
            <input :value="dayInput(form.riskMemoDate)" class="fx-search" type="date" @input="form.riskMemoDate = toDay(value($event))"></label>
          <label class="fc-field fc-full"><span>{{ tr('Risk Notes') }}</span>
            <textarea v-model="form.riskNotes" class="fx-search fc-textarea" rows="5" :placeholder="tr('Enter risk review notes...')"></textarea></label>
        </div>
        <div class="fc-form-actions">
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="save">
            <span class="fa" :class="saving ? 'fa-spinner fa-spin' : 'fa-floppy-o'" aria-hidden="true"></span>{{ tr(saving ? 'Saving...' : 'Save Risk Review') }}</button>
          <FctStepActions :save="persist" :disabled="saving" />
        </div>
      </section>
    </template>
  </FctStepShell>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import { useFeedback } from '@/shared/feedback/useFeedback'
import FctStepShell from '../components/FctStepShell.vue'
import FctStepActions from '../components/FctStepActions.vue'
import FctProcessStrip from '../components/FctProcessStrip.vue'
import { dayInput, errorText, formatAmount, formatMonths, toDay } from '../components/stepFormat'
import { fctStepsApi, type FctRiskReview } from '../api/fct.api'

const props = defineProps<{ agreementId: string }>()

const page = ref<FctRiskReview | null>(null)
const form = reactive<{ riskMemoDate: string | null; riskNotes: string | null }>({ riskMemoDate: null, riskNotes: null })
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const success = ref('')
useFeedback({ errors: [error], notices: [success] })

const value = (e: Event) => (e.target as HTMLInputElement).value

function assign(p: FctRiskReview) {
  page.value = p
  form.riskMemoDate = p.riskReview.riskMemoDate
  form.riskNotes = p.riskReview.riskNotes
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    assign(await fctStepsApi.riskReview(Number(props.agreementId)))
  } catch (e) {
    error.value = errorText(e, 'Unable to load risk review.')
  } finally {
    loading.value = false
  }
}

async function persist(): Promise<boolean> {
  saving.value = true
  error.value = ''
  success.value = ''
  try {
    assign(await fctStepsApi.saveRiskReview(Number(props.agreementId), { ...form }))
    return true
  } catch (e) {
    error.value = errorText(e, 'Unable to save risk review.')
    return false
  } finally {
    saving.value = false
  }
}

async function save() {
  if (await persist()) success.value = tr('Risk review saved.')
}

onMounted(load)
</script>
