<template>
  <!-- FINOVA_FINANCEING FactoringInquiryFieldReportPage: the Investigation step. The client inquiry, AML screening and legal
       opinion are saved together; the debtor rows come from FCT_AGREEMENT_DEBTORS. -->
  <FctStepShell ref="shell" :agreement-id="Number(agreementId)" title="Investigation" icon="fa-search"
                subtitle="Record the client inquiry, AML screening and legal opinion for this application."
                summary-note="Factoring application under investigation" loading-text="Loading inquiry field report..."
                :summary="page?.summary ?? null" :loading="loading" :error="error" :success="success">
    <template v-if="page">
      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-user-circle-o"></span></span>
          <div><h2>{{ tr('Client Inquiry Form') }}</h2></div>
        </header>
        <div class="fc-grid fc-grid--3">
          <label class="fc-field"><span>{{ tr('Investigation Report Date') }}</span>
            <input :value="dayInput(f.inquiry.investigationReportDate)" class="fx-search" type="date" @input="f.inquiry.investigationReportDate = toUtcDay(value($event))"></label>
          <label class="fc-field"><span>{{ tr('Credit Bureau Score') }}</span>
            <input v-model.number="f.inquiry.creditBureauScore" class="fx-search" type="number" min="0" :placeholder="tr('Credit Bureau Score')"></label>
          <label class="fc-field fc-full"><span>{{ tr('Investigation Notes') }}</span>
            <textarea v-model="f.inquiry.investigationNotes" class="fx-search fc-textarea" rows="4" maxlength="2000" :placeholder="tr('Investigation Notes')"></textarea></label>
        </div>
        <div class="fc-form-actions">
          <button type="button" class="fx-btn" :disabled="saving" @click="save(true)">
            <span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr(saving ? 'Saving...' : 'Save and return') }}</button>
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="save(false)">
            <span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr(saving ? 'Saving...' : 'Save Application') }}</button>
        </div>
      </section>

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-shield"></span></span>
          <div><h2>{{ tr('Client AML Form') }}</h2></div>
        </header>
        <div class="fc-grid fc-grid--3">
          <label class="fc-field"><span>{{ tr('AML Screening Report Date') }}</span>
            <input :value="dayInput(f.aml.amlScreeningReportDate)" class="fx-search" type="date" @input="f.aml.amlScreeningReportDate = toUtcDay(value($event))"></label>
          <label class="fc-field"><span>{{ tr('AML Risk Band') }}</span>
            <select v-model="f.aml.amlRiskBand" class="fx-select">
              <option :value="null">{{ tr('Select risk band') }}</option>
              <option v-for="b in ['LOW', 'MEDIUM', 'HIGH']" :key="b" :value="b">{{ b }}</option>
            </select></label>
          <label class="fc-field"><span>{{ tr('AML Screening Result') }}</span>
            <input v-model="f.aml.amlScreeningResult" class="fx-search" type="text" maxlength="80" :placeholder="tr('AML Screening Result')"></label>
          <label class="fc-field fc-full"><span>{{ tr('AML Screening Notes') }}</span>
            <textarea v-model="f.aml.amlScreeningNotes" class="fx-search fc-textarea" rows="3" :placeholder="tr('AML Screening Notes')"></textarea></label>
        </div>
        <div class="fc-form-actions">
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="save(false)">
            <span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr(saving ? 'Saving...' : 'Save AML Changes') }}</button>
        </div>
      </section>

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-balance-scale"></span></span>
          <div><h2>{{ tr('Legal Opinion Form') }}</h2></div>
        </header>
        <div class="fc-grid fc-grid--3">
          <label class="fc-field"><span>{{ tr('Legal Opinion Report Date') }}</span>
            <input :value="dayInput(f.legal.legalOpenionReportDate)" class="fx-search" type="date" @input="f.legal.legalOpenionReportDate = toUtcDay(value($event))"></label>
          <label class="fc-field fc-full"><span>{{ tr('Legal Opinion Notes') }}</span>
            <textarea v-model="f.legal.legalOpenionNotes" class="fx-search fc-textarea" rows="3" :placeholder="tr('Legal Opinion Notes')"></textarea></label>
        </div>
        <div class="fc-form-actions">
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="save(false)">
            <span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr(saving ? 'Saving...' : 'Save Legal Changes') }}</button>
        </div>
      </section>

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-users"></span></span>
          <div><h2>{{ tr('Debtor Inquiry Results') }}</h2></div>
        </header>
        <div class="fc-table-wrap">
          <table class="fx-table">
            <thead><tr><th>{{ tr('Debtor') }}</th><th>{{ tr('Visit Date') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Payment History') }}</th><th>{{ tr('Assessment') }}</th></tr></thead>
            <tbody>
              <tr v-if="!page.debtors.length"><td colspan="5" class="fx-empty">{{ tr('No debtor inquiry rows found.') }}</td></tr>
              <tr v-for="d in page.debtors" :key="d.agrmtDebtorId">
                <td>{{ d.debtorName || '-' }}</td><td>{{ formatDate(d.visitDate) }}</td><td>{{ d.operationalStatus || '-' }}</td>
                <td>{{ d.paymentHistory || '-' }}</td><td>{{ d.assessment || '-' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </template>
  </FctStepShell>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import FctStepShell from '../components/FctStepShell.vue'
import { dayInput, errorText, formatDate, num, toUtcDay } from '../components/stepFormat'
import { fctStepsApi, type FctAmlDetails, type FctInquiryDetails, type FctInquiryFieldReport, type FctLegalDetails } from '../api/fct.api'

const props = defineProps<{ agreementId: string }>()

const shell = ref<InstanceType<typeof FctStepShell> | null>(null)
const page = ref<FctInquiryFieldReport | null>(null)
const f = reactive<{ inquiry: FctInquiryDetails; aml: FctAmlDetails; legal: FctLegalDetails }>({
  inquiry: { investigationReportDate: null, creditBureauScore: null, investigationNotes: null },
  aml: { amlScreeningReportDate: null, amlRiskBand: null, amlScreeningResult: null, amlScreeningNotes: null },
  legal: { legalOpenionReportDate: null, legalOpenionNotes: null }
})
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const success = ref('')

const value = (e: Event) => (e.target as HTMLInputElement).value

function assign(p: FctInquiryFieldReport) {
  page.value = p
  Object.assign(f.inquiry, p.inquiry)
  Object.assign(f.aml, p.aml)
  Object.assign(f.legal, p.legal)
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    assign(await fctStepsApi.inquiry(Number(props.agreementId)))
  } catch (e) {
    error.value = errorText(e, 'Unable to load inquiry field report.')
  } finally {
    loading.value = false
  }
}

// as in FINOVA, every save button saves the three forms together
async function save(andReturn: boolean) {
  if ((f.inquiry.creditBureauScore ?? 0) < 0) {
    error.value = tr('Credit Bureau Score cannot be negative.')
    return
  }
  saving.value = true
  error.value = ''
  success.value = ''
  try {
    assign(await fctStepsApi.saveInquiry(Number(props.agreementId), {
      inquiry: { ...f.inquiry, creditBureauScore: num(f.inquiry.creditBureauScore) }, aml: f.aml, legal: f.legal
    }))
    if (andReturn) shell.value?.backToTask()
    else success.value = tr('Inquiry field report saved.')
  } catch (e) {
    error.value = errorText(e, 'Unable to save inquiry field report.')
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>
