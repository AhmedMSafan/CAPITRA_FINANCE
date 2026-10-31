<template>
  <!-- FINOVA_FINANCEING FactoringLegalReviewPage: the Legal Review and Limit Creation step. The legal opinion is saved on the
       agreement; ACCEPT on the workflow task creates the customer's credit limit. -->
  <FctStepShell :agreement-id="Number(agreementId)" title="Legal Review" icon="fa-balance-scale"
                subtitle="Review the final contract, record the legal opinion and complete the legal decision."
                summary-note="Factoring agreement currently under legal review" loading-text="Loading legal review..."
                :summary="page?.summary ?? null" :loading="loading" @uploaded="loadDocuments">
    <template v-if="page">
      <FctProcessStrip :current="8" with-legal-review />

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-file-pdf-o"></span></span>
          <div><h2>{{ tr('Contract Draft Review') }}</h2><p>{{ tr('Review the latest uploaded contract before completing the legal decision.') }}</p></div>
        </header>
        <ul v-if="latest" class="fc-ref-docs">
          <li>
            <span class="fc-docs-file" aria-hidden="true"><span class="fa fa-file-o"></span></span>
            <div><b>{{ latest.fileName }}</b><small>{{ tr('Uploaded') }} {{ formatDate(latest.creationDate) }} · {{ latest.createdBy }}</small></div>
            <button type="button" class="fx-btn fx-btn--sm" @click="view(latest.documentId)"><span class="fa fa-eye" aria-hidden="true"></span>{{ tr('View') }}</button>
          </li>
        </ul>
        <div v-else class="fc-form-actions">
          <p class="fc-note"><span class="fa fa-info-circle" aria-hidden="true"></span><span>{{ tr('No contract has been uploaded yet.') }}</span></p>
          <FctAgreementDocuments :agreement-id="Number(agreementId)" label="Upload Contract" icon="fa-upload" @uploaded="loadDocuments" />
        </div>
      </section>

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-gavel"></span></span>
          <div><h2>{{ tr('Legal Review Details') }}</h2><p>{{ tr('Record the legal review date and the legal opinion, then accept or reject the step.') }}</p></div>
        </header>
        <div class="fc-grid fc-grid--3">
          <label class="fc-field"><span>{{ tr('Legal Review Date') }}</span>
            <input :value="dayInput(form.legalReviewDate)" class="fx-search" type="date" @input="form.legalReviewDate = toUtcDay(value($event))"></label>
          <p class="fc-note fc-span2"><span class="fa fa-random" aria-hidden="true"></span>
            <span><b>{{ tr('Workflow Decision') }}</b> · {{ tr('Save and Accept creates the customer credit limit.') }}</span></p>
          <label class="fc-field fc-full"><span>{{ tr('Legal Opinion Notes') }}</span>
            <textarea v-model="form.legalOpinionNotes" class="fx-search fc-textarea" rows="7"
                      :placeholder="tr('Enter the legal opinion, observations, required amendments or approval notes...')"></textarea></label>
        </div>
        <div class="fc-form-actions">
          <FctAgreementDocuments :agreement-id="Number(agreementId)" label="Upload Final Contract" icon="fa-upload" @uploaded="loadDocuments" />
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="save">
            <span class="fa" :class="saving ? 'fa-spinner fa-spin' : 'fa-floppy-o'" aria-hidden="true"></span>{{ tr(saving ? 'Saving...' : 'Save Legal Review') }}</button>
          <FctStepActions :save="persist" :disabled="saving" />
        </div>
      </section>
    </template>
  </FctStepShell>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import { useFeedback } from '@/shared/feedback/useFeedback'
import FctStepShell from '../components/FctStepShell.vue'
import FctStepActions from '../components/FctStepActions.vue'
import FctProcessStrip from '../components/FctProcessStrip.vue'
import FctAgreementDocuments from '../components/FctAgreementDocuments.vue'
import { dayInput, errorText, formatDate, toUtcDay, viewDocument } from '../components/stepFormat'
import { fctDocumentsApi, fctStepsApi, type FctAgreementDocument, type FctLegalReview, type FctLegalReviewDetails } from '../api/fct.api'

const props = defineProps<{ agreementId: string }>()

const page = ref<FctLegalReview | null>(null)
const documents = ref<FctAgreementDocument[]>([])
const form = reactive<FctLegalReviewDetails>({ legalReviewDate: null, legalOpinionNotes: null })
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const success = ref('')
useFeedback({ errors: [error], notices: [success] })

// the list comes newest first
const latest = computed(() => documents.value[0] ?? null)
const value = (e: Event) => (e.target as HTMLInputElement).value

function assign(p: FctLegalReview) {
  page.value = p
  Object.assign(form, p.legalReview)
}

async function loadDocuments() {
  documents.value = await fctDocumentsApi.list(Number(props.agreementId))
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    const [p] = await Promise.all([fctStepsApi.legalReview(Number(props.agreementId)), loadDocuments()])
    assign(p)
  } catch (e) {
    error.value = errorText(e, 'Unable to load legal review.')
  } finally {
    loading.value = false
  }
}

async function view(documentId: number) {
  try {
    await viewDocument(documentId)
  } catch (e) {
    error.value = errorText(e, 'Unable to open document.')
  }
}

async function persist(): Promise<boolean> {
  saving.value = true
  error.value = ''
  success.value = ''
  try {
    assign(await fctStepsApi.saveLegalReview(Number(props.agreementId), { ...form }))
    return true
  } catch (e) {
    error.value = errorText(e, 'Unable to save legal review.')
    return false
  } finally {
    saving.value = false
  }
}

async function save() {
  if (await persist()) success.value = tr('Legal review saved.')
}

onMounted(load)
</script>
