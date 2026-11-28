<template>
  <!-- FINOVA_FINANCEING FactoringContractGenerationPage (APEX page 650): the Generate Contract step. The contract data is kept
       on the agreement; Generate Contract saves and prints the contract summary. -->
  <FctStepShell :agreement-id="Number(agreementId)" title="Contract Generation" icon="fa-file-text-o"
                subtitle="Finalize agreement dates and terms, generate the contract and upload the signed or draft document."
                summary-note="Factoring agreement at contract preparation stage" loading-text="Loading contract generation..."
                :summary="page?.summary ?? null" :loading="loading">
    <template v-if="page">
      <FctProcessStrip :current="7" />

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-file-o"></span></span>
          <div><h2>{{ tr('Contract Data Summary') }}</h2><p>{{ tr('Approved terms carried forward from the committee decision.') }}</p></div>
        </header>
        <div class="fc-facts">
          <div class="fc-fact"><span class="fa fa-money" aria-hidden="true"></span><div><span>{{ tr('Approved Amount') }}</span><b>{{ formatAmount(page.approvedTerms.approvedLimit, page.summary.currencyCode) }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-percent" aria-hidden="true"></span><div><span>{{ tr('Advance Rate') }}</span><b>{{ formatRate(page.approvedTerms.approvedAdvRate) }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-line-chart" aria-hidden="true"></span><div><span>{{ tr('Effective Rate') }}</span><b>{{ formatRate(page.approvedTerms.effectiveRatePct) }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-clock-o" aria-hidden="true"></span><div><span>{{ tr('Approved Tenor') }}</span><b>{{ formatMonths(page.approvedTerms.approvedTenorMth) }}</b></div></div>
        </div>
      </section>

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-pencil-square-o"></span></span>
          <div><h2>{{ tr('Contract Generation') }}</h2><p>{{ tr('Maintain the contract dates, tenor and additional legal terms.') }}</p></div>
        </header>
        <div class="fc-grid fc-grid--3">
          <label class="fc-field fc-span2"><span>{{ tr('Agreement Number') }} <b>*</b></span>
            <input v-model="c.agreementNumber" class="fx-search" type="text" maxlength="120" :placeholder="tr('Agreement number')"></label>
          <label class="fc-field"><span>{{ tr('Effective Date') }}</span>
            <input :value="dayInput(c.effectiveDate)" class="fx-search" type="date" @input="c.effectiveDate = toUtcDay(value($event))"></label>
          <label class="fc-field"><span>{{ tr('Tenor Months') }}</span><input v-model.number="c.tenorMonths" class="fx-search" type="number" min="0" placeholder="0"></label>
          <label class="fc-field"><span>{{ tr('Contract Start Date') }}</span>
            <input :value="dayInput(c.contractStartDate)" class="fx-search" type="date" @input="c.contractStartDate = toDay(value($event))"></label>
          <label class="fc-field"><span>{{ tr('Contract End Date') }}</span>
            <input :value="dayInput(c.contractEndDate)" class="fx-search" type="date" @input="c.contractEndDate = toDay(value($event))"></label>
          <label class="fc-field"><span>{{ tr('Contract Signoff Date') }}</span>
            <input :value="dayInput(c.constractSignoffDate)" class="fx-search" type="date" @input="c.constractSignoffDate = toDay(value($event))"></label>
          <label class="fc-field fc-full"><span>{{ tr('Additional Terms & Conditions') }}</span>
            <textarea v-model="c.additionalTermsConditions" class="fx-search fc-textarea" rows="6" maxlength="800" :placeholder="tr('Enter additional contract terms and conditions...')"></textarea></label>
        </div>
        <div class="fc-form-actions">
          <p class="fc-note"><span class="fa fa-upload" aria-hidden="true"></span><span><b>{{ tr('Upload Contract') }}</b> · {{ tr('Upload the draft, final or signed contract to the agreement documents.') }}</span></p>
          <FctAgreementDocuments :agreement-id="Number(agreementId)" label="Upload Contract" icon="fa-upload" />
          <button type="button" class="fx-btn" :disabled="saving" @click="generateContract">
            <span class="fa fa-print" aria-hidden="true"></span>{{ tr('Generate Contract') }}</button>
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="save">
            <span class="fa" :class="saving ? 'fa-spinner fa-spin' : 'fa-check-circle'" aria-hidden="true"></span>{{ tr('Save Step') }}</button>
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
import FctAgreementDocuments from '../components/FctAgreementDocuments.vue'
import { dayInput, errorText, formatAmount, formatDate, formatMonths, formatRate, num, printLetter, toDay, toUtcDay } from '../components/stepFormat'
import { fctStepsApi, type FctContract, type FctContractGeneration } from '../api/fct.api'

const props = defineProps<{ agreementId: string }>()

const page = ref<FctContractGeneration | null>(null)
const c = reactive<FctContract>({
  agreementNumber: null, effectiveDate: null, tenorMonths: null, contractStartDate: null, contractEndDate: null, constractSignoffDate: null,
  additionalTermsConditions: null
})
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const success = ref('')
useFeedback({ errors: [error], notices: [success] })

const value = (e: Event) => (e.target as HTMLInputElement).value

function assign(p: FctContractGeneration) {
  page.value = p
  Object.assign(c, p.contract)
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    assign(await fctStepsApi.contractGeneration(Number(props.agreementId)))
  } catch (e) {
    error.value = errorText(e, 'Unable to load contract generation details.')
  } finally {
    loading.value = false
  }
}

async function persist(): Promise<boolean> {
  if (!c.agreementNumber?.trim()) {
    error.value = tr('Agreement number is required.')
    return false
  }
  saving.value = true
  error.value = ''
  success.value = ''
  try {
    assign(await fctStepsApi.saveContractGeneration(Number(props.agreementId), { ...c, agreementNumber: c.agreementNumber.trim(), tenorMonths: num(c.tenorMonths) }))
    return true
  } catch (e) {
    error.value = errorText(e, 'Unable to save contract generation details.')
    return false
  } finally {
    saving.value = false
  }
}

async function save() {
  if (await persist()) success.value = tr('Contract generation details saved successfully.')
}

async function generateContract() {
  if (!(await persist()) || !page.value) return
  const s = page.value.summary
  printLetter(tr('Capitra Factoring Contract'), `${tr('Agreement')} ${c.agreementNumber || '-'}`, [
    [tr('Client'), s.clientName || '-'],
    [tr('Approved Amount'), formatAmount(page.value.approvedTerms.approvedLimit, s.currencyCode)],
    [tr('Effective Date'), formatDate(c.effectiveDate)],
    [tr('Contract Period'), `${formatDate(c.contractStartDate)} - ${formatDate(c.contractEndDate)}`],
    [tr('Tenor'), formatMonths(c.tenorMonths)],
    [tr('Signoff Date'), formatDate(c.constractSignoffDate)]
  ], [[tr('Additional Terms & Conditions'), c.additionalTermsConditions || '-']])
}

onMounted(load)
</script>
