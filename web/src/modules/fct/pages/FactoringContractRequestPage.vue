<template>
  <!-- FINOVA_FINANCEING FactoringContractRequestPage (APEX page 645): the Client Acceptance step ("Customer Approval"). The
       signed acceptance goes to the agreement documents; the notes to Operations are kept on the agreement. -->
  <FctStepShell :agreement-id="Number(agreementId)" title="Customer Approval" icon="fa-handshake-o"
                subtitle="Capture the client response, signed acceptance and notes required for contract preparation."
                summary-note="Factoring application at client acceptance stage" loading-text="Loading contract request..."
                :summary="page?.summary ?? null" :loading="loading" :error="error" :success="success">
    <template v-if="page">
      <FctProcessStrip :current="6" />

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-check-circle-o"></span></span>
          <div><h2>{{ tr('Approved Terms') }}</h2><p>{{ tr('Committee-approved terms recorded on the agreement.') }}</p></div>
        </header>
        <div class="fc-facts">
          <div class="fc-fact"><span class="fa fa-money" aria-hidden="true"></span><div><span>{{ tr('Approved Amount') }}</span><b>{{ formatAmount(page.approvedTerms.approvedLimit, page.summary.currencyCode) }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-percent" aria-hidden="true"></span><div><span>{{ tr('Advance Rate') }}</span><b>{{ formatRate(page.approvedTerms.approvedAdvRate) }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-line-chart" aria-hidden="true"></span><div><span>{{ tr('Effective Rate') }}</span><b>{{ formatRate(page.approvedTerms.effectiveRatePct) }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-clock-o" aria-hidden="true"></span><div><span>{{ tr('Tenor') }}</span><b>{{ formatMonths(page.approvedTerms.approvedTenorMth) }}</b></div></div>
        </div>
      </section>

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-pencil"></span></span>
          <div><h2>{{ tr('Client Signature & Contract Approval') }}</h2><p>{{ tr('Upload the signed acceptance and add instructions for Operations.') }}</p></div>
        </header>
        <div class="fc-grid">
          <div class="fc-offer fc-upload-row">
            <span class="fa fa-cloud-upload" aria-hidden="true"></span>
            <div><b>{{ tr('Upload signed committee terms acceptance') }}</b><br><small>{{ tr('Opens the agreement documents, with every document uploaded before.') }}</small></div>
            <FctAgreementDocuments :agreement-id="Number(agreementId)" label="Upload Signed Acceptance" icon="fa-upload" />
          </div>
          <label class="fc-field"><span>{{ tr('Notes To Operations') }}</span>
            <textarea v-model="notes" class="fx-search fc-textarea" rows="7" :placeholder="tr('Enter notes and instructions for Operations...')"></textarea></label>
        </div>
        <div class="fc-form-actions">
          <button type="button" class="fx-btn" :disabled="saving" @click="saveResponse">
            <span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save Client Response') }}</button>
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="sendContractRequest">
            <span class="fa" :class="saving ? 'fa-spinner fa-spin' : 'fa-paper-plane'" aria-hidden="true"></span>{{ tr('Send Contract Request') }}</button>
        </div>
      </section>
    </template>
  </FctStepShell>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import FctStepShell from '../components/FctStepShell.vue'
import FctProcessStrip from '../components/FctProcessStrip.vue'
import FctAgreementDocuments from '../components/FctAgreementDocuments.vue'
import { errorText, formatAmount, formatMonths, formatRate } from '../components/stepFormat'
import { fctStepsApi, type FctContractRequest } from '../api/fct.api'

const props = defineProps<{ agreementId: string }>()

const page = ref<FctContractRequest | null>(null)
const notes = ref<string | null>(null)
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const success = ref('')

function assign(p: FctContractRequest) {
  page.value = p
  notes.value = p.notesToOperations
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    assign(await fctStepsApi.contractRequest(Number(props.agreementId)))
  } catch (e) {
    error.value = errorText(e, 'Unable to load contract request.')
  } finally {
    loading.value = false
  }
}

async function save(done: string) {
  saving.value = true
  error.value = ''
  success.value = ''
  try {
    assign(await fctStepsApi.saveContractRequest(Number(props.agreementId), { notesToOperations: notes.value }))
    success.value = tr(done)
  } catch (e) {
    error.value = errorText(e, 'Unable to save client response.')
  } finally {
    saving.value = false
  }
}

const saveResponse = () => save('Client response saved successfully.')
// as in FINOVA, sending the request saves the response; the step is accepted or rejected on the task
const sendContractRequest = () => save('Client response saved. Return to the workflow task to Accept or Reject this step.')

onMounted(load)
</script>
