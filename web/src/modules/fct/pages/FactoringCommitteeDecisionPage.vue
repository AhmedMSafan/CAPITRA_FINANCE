<template>
  <!-- FINOVA_FINANCEING FactoringCommitteeDecisionPage (APEX page 640): the Committee Approval step. The committee-approved
       terms are saved on the agreement; ACCEPT or REJECT (which ends the workflow) is on the workflow task. -->
  <FctStepShell :agreement-id="Number(agreementId)" title="Committee Decision" icon="fa-gavel"
                subtitle="Record the committee-approved facility terms before client acceptance."
                summary-note="Factoring application under committee review" loading-text="Loading committee decision..."
                :summary="page?.summary ?? null" :loading="loading">
    <template #actions>
      <button type="button" class="fx-btn" :disabled="!page" @click="printDecision">
        <span class="fa fa-print" aria-hidden="true"></span>{{ tr('Print Decision Letter') }}</button>
    </template>

    <template v-if="page">
      <FctProcessStrip :current="5" />

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-check-square-o"></span></span>
          <div><h2>{{ tr('Committee Decision') }}</h2><p>{{ tr('Enter the approved terms agreed by the committee.') }}</p></div>
        </header>
        <div class="fc-grid fc-grid--4">
          <label class="fc-field"><span>{{ tr('Approval Date') }}</span>
            <input :value="dayInput(form.approvalDate)" class="fx-search" type="date" @input="form.approvalDate = toDay(value($event))"></label>
          <label class="fc-field"><span>{{ tr('Approved Limit') }} <small>{{ page.summary.currencyCode }}</small></span>
            <input v-model.number="form.approvedLimit" class="fx-search" type="number" min="0" step="0.01" placeholder="0.00"></label>
          <label class="fc-field"><span>{{ tr('Approved Advance Rate') }}</span>
            <input v-model.number="form.approvedAdvRate" class="fx-search" type="number" min="0" step="0.01" placeholder="0.00"></label>
          <label class="fc-field"><span>{{ tr('Approved Tenor') }} <small>{{ tr('Months') }}</small></span>
            <input v-model.number="form.approvedTenorMth" class="fx-search" type="number" min="0" placeholder="0"></label>
        </div>
        <div class="fc-form-actions">
          <button type="button" class="fx-btn fx-btn--primary" :disabled="saving" @click="save">
            <span class="fa" :class="saving ? 'fa-spinner fa-spin' : 'fa-floppy-o'" aria-hidden="true"></span>{{ tr(saving ? 'Saving...' : 'Save Committee Decision') }}</button>
          <FctStepActions :save="persist" :disabled="saving" />
        </div>
      </section>

      <!-- shown as on the original committee page; FINOVA does not store these -->
      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-list-ul"></span></span>
          <div><h2>{{ tr('Pre-Disbursement Conditions') }}</h2><p>{{ tr('Conditions shown on the original committee page.') }}</p></div>
        </header>
        <div class="fc-table-wrap">
          <table class="fx-table fc-conditions">
            <thead><tr><th class="fc-c">{{ tr('Done') }}</th><th>{{ tr('Condition') }}</th><th>{{ tr('Due') }}</th><th>{{ tr('Responsible') }}</th></tr></thead>
            <tbody>
              <tr v-for="c in conditions" :key="c.text">
                <td class="fc-c"><input v-model="c.done" type="checkbox" :aria-label="c.text"></td>
                <td :class="{ 'fc-muted': c.done }">{{ c.text }}</td><td>{{ tr(c.due) }}</td><td>{{ tr(c.responsible) }}</td>
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
import { useFeedback } from '@/shared/feedback/useFeedback'
import FctStepShell from '../components/FctStepShell.vue'
import FctStepActions from '../components/FctStepActions.vue'
import FctProcessStrip from '../components/FctProcessStrip.vue'
import { dayInput, errorText, formatAmount, formatDate, formatMonths, formatRate, num, printLetter, toDay } from '../components/stepFormat'
import { fctStepsApi, type FctCommitteeDecision, type FctCommitteeDecisionTerms } from '../api/fct.api'

const props = defineProps<{ agreementId: string }>()

const conditions = reactive([
  { text: 'Monthly financial statements required by 10th of each month', due: 'Ongoing', responsible: 'BD Officer', done: false },
  { text: 'Client must notify AAIL of any changes to Carrefour payment terms within 5 days', due: 'Ongoing', responsible: 'BD Officer', done: false },
  { text: 'Signed final contract copy to be deposited before disbursement', due: 'Before first disbursement', responsible: 'Operations', done: false }
])

const page = ref<FctCommitteeDecision | null>(null)
const form = reactive<FctCommitteeDecisionTerms>({ approvalDate: null, approvedLimit: null, approvedAdvRate: null, approvedTenorMth: null })
const loading = ref(false)
const saving = ref(false)
const error = ref('')
const success = ref('')
useFeedback({ errors: [error], notices: [success] })

const value = (e: Event) => (e.target as HTMLInputElement).value

function assign(p: FctCommitteeDecision) {
  page.value = p
  Object.assign(form, p.decision)
}

async function load() {
  loading.value = true
  error.value = ''
  try {
    assign(await fctStepsApi.committeeDecision(Number(props.agreementId)))
  } catch (e) {
    error.value = errorText(e, 'Unable to load committee decision.')
  } finally {
    loading.value = false
  }
}

async function persist(): Promise<boolean> {
  saving.value = true
  error.value = ''
  success.value = ''
  try {
    assign(await fctStepsApi.saveCommitteeDecision(Number(props.agreementId), {
      approvalDate: form.approvalDate, approvedLimit: num(form.approvedLimit), approvedAdvRate: num(form.approvedAdvRate), approvedTenorMth: num(form.approvedTenorMth)
    }))
    return true
  } catch (e) {
    error.value = errorText(e, 'Unable to save committee decision.')
    return false
  } finally {
    saving.value = false
  }
}

async function save() {
  if (await persist()) success.value = tr('Committee decision saved.')
}

function printDecision() {
  const s = page.value?.summary
  if (!s) return
  printLetter(tr('Committee Decision Letter'), `${tr('Agreement')} ${s.agreementNumber}`, [
    [tr('Client'), s.clientName || '-'],
    [tr('Requested Amount'), formatAmount(s.facilityLimit, s.currencyCode)],
    [tr('Approval Date'), formatDate(form.approvalDate)],
    [tr('Approved Limit'), formatAmount(num(form.approvedLimit), s.currencyCode)],
    [tr('Approved Advance Rate'), formatRate(num(form.approvedAdvRate))],
    [tr('Approved Tenor'), formatMonths(num(form.approvedTenorMth))]
  ], [[tr('Pre-Disbursement Conditions'), conditions.map((c) => `- ${c.text} (${c.due}, ${c.responsible})`).join('\n')]])
}

onMounted(load)
</script>
