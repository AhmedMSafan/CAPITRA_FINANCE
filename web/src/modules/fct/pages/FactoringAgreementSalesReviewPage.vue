<template>
  <!-- FINOVA_FINANCEING FactoringAgreementSalesReviewPage: the Sales Review step. The sales officer corrects the application
       header; the task is completed on the workflow task. -->
  <FctStepShell ref="shell" :agreement-id="Number(agreementId)" title="Sales Review" icon="fa-pencil-square-o"
                subtitle="Review and complete the application before the investigation starts."
                summary-note="Factoring application under sales review" loading-text="Loading application..."
                :summary="summary" :submitted-by="form?.createdBy" :loading="loading" :error="error" :success="success">
    <template v-if="form">
      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-file-text-o"></span></span>
          <div><h2>{{ tr('Application Header Form') }}</h2></div>
        </header>
        <div class="fc-offer"><span>{{ tr('Offer Number') }}</span><b>{{ form.agreementRef || '-' }}</b></div>
        <div class="fc-grid fc-grid--3">
          <label class="fc-field"><span>{{ tr('Application Number') }}</span><input :value="form.agreementNumber" class="fx-search fc-ro" type="text" readonly></label>
          <label class="fc-field"><span>{{ tr('Application Date') }}</span>
            <input :value="dayInput(form.applicationDate)" class="fx-search" type="date" @input="form.applicationDate = toUtcDay(value($event))"></label>
          <label class="fc-field"><span>{{ tr('Client') }}</span>
            <select v-model.number="form.clientId" class="fx-select"><option v-for="c in form.clients" :key="c.value" :value="c.value">{{ c.label }}</option></select></label>
          <label class="fc-field"><span>{{ tr('Factoring Type') }}</span>
            <select v-model="form.factoringType" class="fx-select">
              <option value="RECEIVABLES - SUPPLIER FINANCE">RECEIVABLES - SUPPLIER FINANCE</option>
              <option value="REVERSE - BUYER FINANCE">REVERSE - BUYER FINANCE</option>
            </select></label>
          <label class="fc-field"><span>{{ tr('Recourse Type') }}</span>
            <select v-model="form.recourseType" class="fx-select">
              <option value="RECOURSE">RECOURSE</option>
              <option value="NON-RECOURSE">NON-RECOURSE</option>
            </select></label>
          <label class="fc-field"><span>{{ tr('Facility Limit') }}</span><input v-model.number="form.facilityLimit" class="fx-search" type="number" min="0" step="0.01"></label>
          <label class="fc-field"><span>{{ tr('Currency') }}</span>
            <select v-model.number="form.currencyId" class="fx-select"><option v-for="c in form.currencies" :key="c.value" :value="c.value">{{ c.label }}</option></select></label>
          <label class="fc-field"><span>{{ tr('Advance Rate Pct') }}</span><input v-model.number="form.advanceRatePct" class="fx-search" type="number" min="0" step="0.01"></label>
          <label class="fc-field"><span>{{ tr('Tenor Months') }}</span><input v-model.number="form.tenorMonths" class="fx-search" type="number" min="0"></label>
          <label class="fc-field fc-full"><span>{{ tr('Notes') }}</span><textarea v-model="form.notes" class="fx-search fc-textarea" rows="3"></textarea></label>
        </div>
        <div class="fc-form-actions">
          <button type="button" class="fx-btn" :disabled="saving" @click="save(true)">
            <span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr(saving ? 'Saving...' : 'Save and return') }}</button>
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="save(false)">
            <span class="fa" :class="saving ? 'fa-spinner fa-spin' : 'fa-floppy-o'" aria-hidden="true"></span>{{ tr(saving ? 'Saving...' : 'Save Application') }}</button>
        </div>
      </section>

      <!-- FINOVA shows the debtors region for display only, with no rows yet -->
      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-users"></span></span>
          <div><h2>{{ tr('Debtors') }}</h2></div>
        </header>
        <div class="fc-table-wrap">
          <table class="fx-table">
            <thead><tr>
              <th>{{ tr('Agreement Id') }}</th><th>{{ tr('Tax Id') }}</th><th>{{ tr('Debtor Name') }}</th><th class="fx-r">{{ tr('Debtor Limit') }}</th>
              <th>{{ tr('Currency Id') }}</th><th class="fx-r">{{ tr('Concentration Limit Pct') }}</th><th>{{ tr('Remarks') }}</th>
            </tr></thead>
            <tbody><tr><td colspan="7" class="fx-empty">{{ tr('No debtor records.') }}</td></tr></tbody>
          </table>
        </div>
      </section>
    </template>
  </FctStepShell>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import FctStepShell from '../components/FctStepShell.vue'
import { dayInput, errorText, num, toUtcDay } from '../components/stepFormat'
import { fctStepsApi, type FctAgreementForm } from '../api/fct.api'

const props = defineProps<{ agreementId: string }>()

const shell = ref<InstanceType<typeof FctStepShell> | null>(null)
const form = ref<FctAgreementForm | null>(null)
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const success = ref('')

const value = (e: Event) => (e.target as HTMLInputElement).value

const summary = computed(() => {
  const f = form.value
  if (!f) return null
  return {
    agreementId: f.agreementId, agreementNumber: f.agreementNumber, clientId: f.clientId,
    clientName: f.clients.find((c) => c.value === f.clientId)?.label ?? null, facilityLimit: f.facilityLimit,
    currencyCode: f.currencies.find((c) => c.value === f.currencyId)?.label ?? null, applicationDate: f.applicationDate,
    relationshipManager: null, statusCode: f.statusCode
  }
})

async function load() {
  loading.value = true
  error.value = ''
  try {
    form.value = await fctStepsApi.agreement(Number(props.agreementId))
  } catch (e) {
    error.value = errorText(e, 'Unable to load factoring application.')
  } finally {
    loading.value = false
  }
}

async function save(andReturn: boolean) {
  const f = form.value
  if (!f) return
  saving.value = true
  error.value = ''
  success.value = ''
  try {
    form.value = await fctStepsApi.updateAgreement(f.agreementId, {
      applicationDate: f.applicationDate, clientId: f.clientId, factoringType: f.factoringType, recourseType: f.recourseType,
      currencyId: f.currencyId, facilityLimit: num(f.facilityLimit) ?? 0, advanceRatePct: num(f.advanceRatePct) ?? 0, tenorMonths: num(f.tenorMonths),
      notes: f.notes
    })
    if (andReturn) shell.value?.backToTask()
    else success.value = tr('Application saved.')
  } catch (e) {
    error.value = errorText(e, 'Unable to save factoring application.')
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>
