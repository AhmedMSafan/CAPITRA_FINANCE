<template>
  <!-- APEX App 150722 page 751 "Disbursement Request Detail": the FCT_DISBURSEMENTS form. Choosing a limit shows its agreement in
       the Application Summary; Initialize Workflow Cycle saves the request and starts DISBURSEMENT_REQUEST_WF, after which the
       request only shows. Opened from a workflow task, the step's Complete button is here too. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-file-text-o"></span></span>
        <div>
          <h1>{{ tr('Disbursement Request Detail') }}</h1>
          <p>{{ number }}</p>
        </div>
      </div>
      <div class="col-actions">
        <RouterLink class="fx-btn" :to="{ name: 'crm-disbursement-requests' }"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to list') }}</RouterLink>
        <FctStepActions v-if="isSaved" :save="keep" :disabled="busy" />
      </div>
    </div>

    <div v-if="ready" class="col-stack">
      <!-- the Application Summary region: hidden while there is no agreement -->
      <section v-if="summary" class="dr-sum" :aria-label="tr('Application Summary')">
        <div class="dr-sum-title">
          <span class="dr-sum-ic" aria-hidden="true">&#x2702;</span>{{ tr('Application Summary') }}
          <span class="dr-badge" :class="{ 'is-on': summary.statusCode === 'ACTIVE' }">{{ summary.statusCode }}</span>
        </div>
        <div class="dr-sum-grid">
          <div><div class="dr-sum-label">{{ tr('Agreement No.') }}</div><div class="dr-sum-value is-link">{{ summary.agreementNumber }}</div></div>
          <div><div class="dr-sum-label">{{ tr('Client') }}</div><div class="dr-sum-value">{{ summary.clientName }}</div></div>
          <div><div class="dr-sum-label">{{ tr('Requested Amount') }}</div><div class="dr-sum-value">{{ summary.currencyCode }} {{ wholeAmount(summary.facilityLimit) }}</div></div>
          <div><div class="dr-sum-label">{{ tr('Submitted') }}</div><div class="dr-sum-value">{{ submitted(summary) }}</div></div>
        </div>
      </section>

      <section class="col-sec">
        <h2>
          <span class="fa fa-money" aria-hidden="true"></span>{{ tr('Disbursement Request Detail') }}
          <span v-if="started" class="fx-chip fx-chip--teal dr-chip">{{ tr('WF Started') }}</span>
        </h2>
        <div class="col-grid">
          <div class="fc-field">
            <span>{{ tr('Disbursement Number') }}</span>
            <div class="col-ro">{{ number }}</div>
          </div>
          <label class="fc-field" :class="{ 'col-bad': bad('disbursementDate') }">
            <span>{{ tr('Disbursement Date') }} <b v-if="!readOnly">*</b></span>
            <input v-model="form.disbursementDate" class="fx-search" type="date" :disabled="readOnly">
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('clientId') }">
            <span>{{ tr('Client Id') }} <b v-if="!readOnly">*</b></span>
            <select v-model="form.clientId" class="fx-select" :disabled="readOnly" @change="clientChanged">
              <option :value="null">{{ tr('- Select -') }}</option>
              <option v-for="c in clientOptions" :key="c.value" :value="Number(c.value)">{{ c.label }}</option>
            </select>
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('limitId') }">
            <span>{{ tr('Available Limit') }}</span>
            <select v-model="form.limitId" class="fx-select" :disabled="readOnly" @change="limitChanged">
              <option :value="null">{{ tr('Select Limit') }}</option>
              <option v-for="l in limits" :key="l.value" :value="Number(l.value)">{{ l.label }}</option>
            </select>
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('currencyId') }">
            <span>{{ tr('Currency Id') }} <b v-if="!readOnly">*</b></span>
            <select v-model="form.currencyId" class="fx-select" :disabled="readOnly">
              <option v-for="c in currencies" :key="c.value" :value="Number(c.value)">{{ c.label }}</option>
            </select>
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('grossAmount') }">
            <span>{{ tr('Invoices Gross Amount') }} <b v-if="!readOnly">*</b></span>
            <input v-model.number="form.grossAmount" class="fx-search" type="number" min="0" step="0.01" inputmode="decimal" :readonly="readOnly">
          </label>
          <label class="fc-field dr-wide">
            <span>{{ tr('Remarks') }}</span>
            <input v-model="form.remarks" class="fx-search" type="text" maxlength="2000" :readonly="readOnly">
          </label>
        </div>
        <div v-if="!readOnly" class="col-actions dr-buttons">
          <RouterLink class="fx-btn" :to="{ name: 'crm-disbursement-requests' }">{{ tr('Cancel') }}</RouterLink>
          <template v-if="isSaved">
            <button type="button" class="fx-btn fc-btn-reject" :disabled="busy" @click="remove"><span class="fa fa-trash-o" aria-hidden="true"></span>{{ tr('Delete') }}</button>
            <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Apply Changes') }}</button>
            <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="startWorkflow"><span class="fa fa-play-circle" aria-hidden="true"></span>{{ tr('Initialize Workflow Cycle') }}</button>
          </template>
          <button v-else type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="create"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Create') }}</button>
        </div>
      </section>
    </div>
    <div v-else class="fx-card fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '@/modules/collection/pages/collection.css'
import { computed, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { todayInCairo } from '@/shared/format/datetime'
import { fieldErrors, messageOf } from '@/modules/collection/pages/collection'
import FctStepActions from '@/modules/fct/components/FctStepActions.vue'
import type { CrmOption } from '../api/crm.api'
import { disbursementApi, type AgreementSummary, type DisbursementRequestSave } from '../api/disbursement.api'

const props = defineProps<{ disbursementId?: number }>()

const router = useRouter()
const canEdit = hasPermission('CRM.EDIT')
const clientOptions = ref<CrmOption[]>([])
const limits = ref<CrmOption[]>([])
const currencies = ref<CrmOption[]>([])
const summary = ref<AgreementSummary | null>(null)
const ready = ref(false)
const busy = ref(false)
const started = ref(false)
const errors = ref<Record<string, string[]>>({})
const bad = (field: string) => !!errors.value[field]

const isSaved = computed(() => props.disbursementId != null)
const readOnly = computed(() => started.value || !canEdit)

// P751_DISBURSEMENT_NUMBER defaults to 'TMP-' || TO_CHAR(SYSDATE, 'HHMMSS') until the request is saved
const tmpNumber = () => 'TMP-' + new Date().toTimeString().slice(0, 8).replace(/:/g, '')
const number = ref(tmpNumber())

// Disbursement Date defaults to today, Currency Id to 1
const blank = (): DisbursementRequestSave => ({
  disbursementDate: todayInCairo(), clientId: null, limitId: null, currencyId: 1, grossAmount: null, remarks: null
})
const form = reactive<DisbursementRequestSave>(blank())

// the summary's numbers: TO_CHAR(FACILITY_LIMIT, 'FM999,999,999,999') and the application date as MM/DD/YYYY
const wholeAmount = (n: number) => Math.round(n).toLocaleString('en-US')
function submitted(s: AgreementSummary): string {
  const d = s.applicationAt ? new Date(s.applicationAt) : null
  const date = d ? `${String(d.getMonth() + 1).padStart(2, '0')}/${String(d.getDate()).padStart(2, '0')}/${d.getFullYear()}` : ''
  return `${date} · ${s.relationshipManager ?? ''}`
}

async function loadLimits() {
  limits.value = form.clientId == null ? [] : await disbursementApi.limits(form.clientId)
}

// the limit list cascades from the client
async function clientChanged() {
  form.limitId = null
  summary.value = null
  try {
    await loadLimits()
  } catch (err) {
    showError(messageOf(err))
  }
}

// FetchDetails: the chosen limit's agreement fills the Application Summary
async function limitChanged() {
  try {
    summary.value = form.limitId == null ? null : await disbursementApi.limitSummary(form.limitId)
  } catch (err) {
    summary.value = null
    showError(messageOf(err))
  }
}

async function load() {
  ready.value = false
  errors.value = {}
  try {
    ;[clientOptions.value, currencies.value] = await Promise.all([disbursementApi.clients(), disbursementApi.currencies()])
    if (props.disbursementId == null) {
      Object.assign(form, blank())
      number.value = tmpNumber()
      started.value = false
      limits.value = []
      summary.value = null
    } else {
      const d = await disbursementApi.get(props.disbursementId)
      Object.assign(form, {
        disbursementDate: d.disbursementDate, clientId: d.clientId, limitId: d.limitId, currencyId: d.currencyId,
        grossAmount: d.grossAmount, remarks: d.remarks
      })
      // a client outside the list (a prospect) still shows by name
      if (!clientOptions.value.some((c) => Number(c.value) === d.clientId))
        clientOptions.value = [...clientOptions.value, { value: String(d.clientId), label: `${d.clientName ?? ''} -- ${d.clientId}` }]
      number.value = d.disbursementNumber
      started.value = d.workflowStarted
      await loadLimits()
      if (d.limitId != null && !limits.value.some((l) => Number(l.value) === d.limitId))
        limits.value = [...limits.value, { value: String(d.limitId), label: String(d.limitId) }]
      summary.value = d.agreementId == null ? null : await disbursementApi.agreementSummary(d.agreementId)
    }
  } catch (err) {
    showError(messageOf(err))
  } finally {
    ready.value = true
  }
}

// the page's required items, checked before the submit as APEX does
function check(): boolean {
  const e: Record<string, string[]> = {}
  if (!form.disbursementDate) e.disbursementDate = ['Disbursement Date must have some value.']
  if (form.clientId == null) e.clientId = ['Client Id must have some value.']
  if (form.currencyId == null) e.currencyId = ['Currency Id must have some value.']
  if (form.grossAmount == null || (form.grossAmount as unknown) === '') e.grossAmount = ['Invoices Gross Amount must have some value.']
  errors.value = e
  if (Object.keys(e).length) showError(Object.values(e).flat().map((m) => tr(m)).join('\n'))
  return !Object.keys(e).length
}

const payload = (): DisbursementRequestSave => ({ ...form, grossAmount: Number(form.grossAmount), remarks: form.remarks?.trim() || null })

async function run(work: () => Promise<void>) {
  busy.value = true
  try {
    await work()
  } catch (err) {
    errors.value = fieldErrors(err) ?? {}
    showError(messageOf(err))
  } finally {
    busy.value = false
  }
}

const create = () => check() && run(async () => {
  const saved = await disbursementApi.create(payload())
  showSuccess(tr(saved.message))
  await router.replace({ name: 'crm-disbursement-request', params: { disbursementId: saved.disbursementId } })
})

const save = () => check() && run(async () => {
  const d = await disbursementApi.update(props.disbursementId!, payload())
  number.value = d.disbursementNumber
  showSuccess(tr('Action Processed.'))
})

const remove = () => window.confirm(tr('Would you like to perform this delete action?')) && run(async () => {
  await disbursementApi.remove(props.disbursementId!)
  showSuccess(tr('Action Processed.'))
  await router.push({ name: 'crm-disbursement-requests' })
})

// START: saves the request, starts the workflow, then goes back to the list with the message (branch BackToList)
const startWorkflow = () => check() && run(async () => {
  const saved = await disbursementApi.startWorkflow(props.disbursementId!, payload())
  showSuccess(tr(saved.message))
  await router.push({ name: 'crm-disbursement-requests' })
})

// a workflow step has nothing of its own to save: the request only shows
const keep = async () => true

watch(() => props.disbursementId, load, { immediate: true })
</script>

<style scoped>
/* the Application Summary card of page 751 */
.dr-sum { background: var(--fin-surface); border: 1px solid var(--fin-line); border-radius: 12px; padding: 18px 22px; }
.dr-sum-title { display: flex; align-items: center; gap: 10px; margin-bottom: 14px; font-weight: 700; font-size: 16px; color: var(--fin-ink); }
.dr-sum-ic { width: 30px; height: 30px; border-radius: 8px; background: #E6F3F4; color: #00636D; display: flex; align-items: center; justify-content: center; font-size: 15px; }
.dr-badge { margin-inline-start: auto; padding: 4px 10px; border-radius: 999px; background: #9bb0af; color: #fff; font-size: 11px; font-weight: 700; letter-spacing: .04em; }
.dr-badge.is-on { background: #2f9e64; }
.dr-sum-grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 18px; }
.dr-sum-label { margin-bottom: 5px; font-size: 11px; letter-spacing: .04em; text-transform: uppercase; color: #9bb0af; }
.dr-sum-value { font-weight: 700; font-size: 15px; color: var(--fin-ink); overflow-wrap: anywhere; }
.dr-sum-value.is-link { color: #00636D; }
.dr-chip { margin-inline-start: 6px; }
.dr-wide { grid-column: span 2; }
.dr-buttons { justify-content: flex-end; margin-top: 16px; }
@media (max-width: 760px) {
  .dr-sum-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .dr-wide { grid-column: auto; }
}
</style>
