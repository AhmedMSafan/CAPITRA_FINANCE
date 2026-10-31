<template>
  <!-- APEX App 150722 page 751 "Disbursement Request Detail": the FCT_DISBURSEMENTS form, redesigned (Ahmed, 31 Oct 2026). A new
       request is made from a limit of the Factoring Customer Workspace, so the customer, the limit and its currency come with it and
       only show; the limit's agreement fills the Application Summary. The workflow is started from the workspace's Disbursements
       tab, after which the request only shows. Opened from a workflow task, the step's Complete button is here too. -->
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
        <RouterLink class="fx-btn" :to="back"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Customer Workspace') }}</RouterLink>
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
          <div class="fc-field">
            <span>{{ tr('Customer') }}</span>
            <div class="col-ro">{{ customer ? `${customer.clientName} -- ${customer.clientNumber}` : '-' }}</div>
          </div>
          <div class="fc-field" :class="{ 'col-bad': bad('limitId') }">
            <span>{{ tr('Limit') }}</span>
            <div class="col-ro">{{ limit?.limitNumber ?? '-' }}</div>
          </div>
          <div class="fc-field">
            <span>{{ tr('Available Limit') }}</span>
            <div class="col-ro">{{ limit?.availableAmount == null ? '-' : formatAmount(limit.availableAmount) }}</div>
          </div>
          <div class="fc-field">
            <span>{{ tr('Currency') }}</span>
            <div class="col-ro">{{ limit?.currencyCode ?? '-' }}</div>
          </div>
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
          <RouterLink class="fx-btn" :to="back">{{ tr('Cancel') }}</RouterLink>
          <template v-if="isSaved">
            <button type="button" class="fx-btn fc-btn-reject" :disabled="busy" @click="remove"><span class="fa fa-trash-o" aria-hidden="true"></span>{{ tr('Delete') }}</button>
            <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Apply Changes') }}</button>
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
import { formatAmount } from '@/shared/format/money'
import { fieldErrors, messageOf } from '@/modules/collection/pages/collection'
import FctStepActions from '@/modules/fct/components/FctStepActions.vue'
import { fctApi, type FctCreditLimit, type FctCustomerDetails } from '@/modules/fct/api/fct.api'
import { disbursementApi, type AgreementSummary, type DisbursementRequestSave } from '../api/disbursement.api'

// a saved request comes by its id; a new one by the customer and the limit it is made from
const props = defineProps<{ disbursementId?: number; clientId?: number; limitId?: number }>()

const router = useRouter()
const canEdit = hasPermission('CRM.EDIT')
const customer = ref<FctCustomerDetails | null>(null)
const limit = ref<FctCreditLimit | null>(null)
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

// Disbursement Date defaults to today; the customer, the limit and its currency are set by load()
const blank = (): DisbursementRequestSave => ({
  disbursementDate: todayInCairo(), clientId: null, limitId: null, currencyId: null, grossAmount: null, remarks: null
})
const form = reactive<DisbursementRequestSave>(blank())

// back to the customer's Factoring Customer Workspace, on its Disbursements tab
const back = computed(() => form.clientId == null ? { name: 'crm-customers' }
  : { name: 'FactoringCustomerWorkspace', params: { clientId: form.clientId }, query: { tab: 'disbursements', from: 'customer' } })

// the summary's numbers: TO_CHAR(FACILITY_LIMIT, 'FM999,999,999,999') and the application date as MM/DD/YYYY
const wholeAmount = (n: number) => Math.round(n).toLocaleString('en-US')
function submitted(s: AgreementSummary): string {
  const d = s.applicationAt ? new Date(s.applicationAt) : null
  const date = d ? `${String(d.getMonth() + 1).padStart(2, '0')}/${String(d.getDate()).padStart(2, '0')}/${d.getFullYear()}` : ''
  return `${date} · ${s.relationshipManager ?? ''}`
}

// the customer card and the limit row of the workspace the request belongs to
async function loadParty(clientId: number, limitId: number | null) {
  const [c, limits] = await Promise.all([fctApi.customer(clientId), fctApi.creditLimits(clientId)])
  customer.value = c
  limit.value = limits.find((l) => l.creditLimitId === limitId) ?? null
}

async function load() {
  ready.value = false
  errors.value = {}
  try {
    if (props.disbursementId == null) {
      Object.assign(form, blank(), { clientId: props.clientId ?? null, limitId: props.limitId ?? null })
      number.value = tmpNumber()
      started.value = false
      if (form.clientId != null) await loadParty(form.clientId, form.limitId)
      form.currencyId = limit.value?.currencyId ?? null
      summary.value = form.limitId == null ? null : await disbursementApi.limitSummary(form.limitId)
    } else {
      const d = await disbursementApi.get(props.disbursementId)
      Object.assign(form, {
        disbursementDate: d.disbursementDate, clientId: d.clientId, limitId: d.limitId, currencyId: d.currencyId,
        grossAmount: d.grossAmount, remarks: d.remarks
      })
      number.value = d.disbursementNumber
      started.value = d.workflowStarted
      await loadParty(d.clientId, d.limitId)
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
  if (form.clientId == null || form.limitId == null || form.currencyId == null)
    e.limitId = ['Open a new disbursement request from a limit of the Factoring Customer Workspace.']
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

// Create: the request is saved and listed in the Disbursements tab with Workflow Not Started
const create = () => check() && run(async () => {
  const saved = await disbursementApi.create(payload())
  showSuccess(tr(saved.message))
  await router.push(back.value)
})

const save = () => check() && run(async () => {
  const d = await disbursementApi.update(props.disbursementId!, payload())
  number.value = d.disbursementNumber
  showSuccess(tr('Action Processed.'))
})

const remove = () => window.confirm(tr('Would you like to perform this delete action?')) && run(async () => {
  await disbursementApi.remove(props.disbursementId!)
  showSuccess(tr('Action Processed.'))
  await router.push(back.value)
})

// a workflow step has nothing of its own to save: the request only shows
const keep = async () => true

watch(() => [props.disbursementId, props.clientId, props.limitId], load, { immediate: true })
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
