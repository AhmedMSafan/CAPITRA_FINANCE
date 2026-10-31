<template>
  <!-- APEX App 150722 page 751 "Disbursement Request Detail": the FCT_DISBURSEMENTS form, redesigned (Ahmed, 31 Oct 2026). A new
       request is made from a limit of the Factoring Customer Workspace, so the customer, the limit and its currency come with it and
       only show, with the limit's agreement, in one strip. The workflow is started from the workspace's Disbursements
       tab, after which the request only shows. Opened from a workflow task, the step's Complete button is here too. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'crm-customers' }">{{ tr('Customers') }}</RouterLink><span aria-hidden="true">›</span>
          <template v-if="form.clientId != null">
            <RouterLink :to="{ name: 'crm-customer-workspace', params: { clientId: form.clientId } }">{{ tr('Customer Workspace') }}</RouterLink><span aria-hidden="true">›</span>
            <RouterLink :to="back">{{ tr('Factoring Customer Workspace') }}</RouterLink><span aria-hidden="true">›</span>
          </template>
          <b>{{ tr(isSaved ? 'Disbursement Request' : 'New Disbursement Request') }}</b>
        </nav>
        <div class="dr-title">
          <h1>{{ tr(isSaved ? 'Disbursement Request' : 'New Disbursement Request') }}</h1>
          <span class="fx-chip" :class="started ? 'fx-chip--teal' : 'fx-chip--warn'">{{ tr(started ? 'WF Started' : 'Workflow Not Started') }}</span>
        </div>
        <p class="dr-sub">{{ isSaved ? number : `${tr('From limit')} ${limit?.limitNumber ?? ''}` }}</p>
      </div>
      <div class="fx-actions">
        <RouterLink class="fx-btn" :to="back"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Factoring Workspace') }}</RouterLink>
        <FctStepActions v-if="isSaved" :save="keep" :disabled="busy" />
      </div>
    </div>

    <template v-if="ready">
      <!-- the customer, the limit and its agreement (page 751's Application Summary) in one strip -->
      <section class="fx-card dr-strip">
        <div class="fc-customer-main">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-building-o"></span></span>
          <div>
            <span class="fc-customer-no">{{ customer?.clientNumber || '—' }}</span>
            <b class="fc-customer-name">{{ customer?.clientName || '—' }}</b>
            <span class="fc-customer-ar">{{ summary ? `${tr('Agreement')} ${summary.agreementNumber}` : '' }}</span>
          </div>
        </div>
        <div class="fc-info"><span>{{ tr('Limit') }}</span><b>{{ limit?.limitNumber ?? '—' }}</b></div>
        <div class="fc-info"><span>{{ tr('Currency') }}</span><span class="fx-chip fx-chip--brand fx-chip--plain">{{ limit?.currencyCode ?? '—' }}</span></div>
        <div class="fc-info"><span>{{ tr('Facility Limit') }}</span><b>{{ summary ? wholeAmount(summary.facilityLimit) : '—' }}</b></div>
        <div class="fc-info"><span>{{ tr('Available Limit') }}</span><b>{{ limit?.availableAmount == null ? '—' : wholeAmount(limit.availableAmount) }}</b></div>
        <div class="fc-info"><span>{{ tr('Agreement Status') }}</span>
          <span v-if="summary?.statusCode" class="fx-chip fx-chip--plain" :class="summary.statusCode === 'ACTIVE' ? 'fx-chip--ok' : 'fx-chip--warn'">{{ summary.statusCode }}</span>
          <b v-else>—</b>
        </div>
      </section>

      <section class="fx-card dr-sec">
        <div class="dr-sec-head">
          <h2><span class="fa fa-money" aria-hidden="true"></span>{{ tr('Request Details') }}</h2>
          <div v-if="started" class="dr-step"><span>{{ tr('Workflow Step') }}</span><span class="fx-chip fx-chip--teal fx-chip--plain">{{ step || '-' }}</span></div>
        </div>

        <!-- once the workflow has started (or without CRM.EDIT) the request only shows -->
        <div v-if="readOnly" class="dr-kv">
          <div><span>{{ tr('Disbursement Number') }}</span><b>{{ number }}</b></div>
          <div><span>{{ tr('Disbursement Date') }}</span><b>{{ formatDate(form.disbursementDate) }}</b></div>
          <div><span>{{ tr('Invoices Gross Amount') }}</span><b class="dr-amt">{{ formatAmount(Number(form.grossAmount ?? 0)) }} <small>{{ limit?.currencyCode ?? '' }}</small></b></div>
          <div><span>{{ tr('Disbursement Type') }}</span><b>{{ type || '-' }}</b></div>
          <div class="dr-wide"><span>{{ tr('Remarks') }}</span><b class="dr-text">{{ form.remarks || '-' }}</b></div>
        </div>

        <template v-else>
          <div class="dr-form">
            <label class="fc-field" :class="{ 'col-bad': bad('disbursementDate') }">
              <span>{{ tr('Disbursement Date') }} <b class="dr-req">*</b></span>
              <input v-model="form.disbursementDate" class="fx-search" type="date">
            </label>
            <label class="fc-field" :class="{ 'col-bad': bad('grossAmount') }">
              <span>{{ tr('Invoices Gross Amount') }} <b class="dr-req">*</b></span>
              <span class="dr-amount">
                <input v-model.number="form.grossAmount" class="fx-search" type="number" min="0" step="0.01" inputmode="decimal" placeholder="0.00">
                <em>{{ limit?.currencyCode ?? '' }}</em>
              </span>
            </label>
            <div class="fc-field">
              <span>{{ tr('Disbursement Number') }}</span>
              <div class="col-ro" :class="{ 'dr-muted': !isSaved }">{{ isSaved ? number : tr('Given on save') }}</div>
            </div>
            <label class="fc-field dr-wide">
              <span>{{ tr('Remarks') }}</span>
              <textarea v-model="form.remarks" class="fx-search dr-textarea" maxlength="2000" :placeholder="tr('Optional')"></textarea>
            </label>
          </div>
          <div class="dr-foot">
            <span class="dr-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('After saving, start the workflow from the Disbursements tab.') }}</span>
            <div class="dr-buttons">
              <template v-if="isSaved">
                <button type="button" class="fx-btn fc-btn-reject" :disabled="busy" @click="remove"><span class="fa fa-trash-o" aria-hidden="true"></span>{{ tr('Delete') }}</button>
                <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Apply Changes') }}</button>
              </template>
              <button v-else type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="create"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Create') }}</button>
            </div>
          </div>
        </template>
      </section>
    </template>
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
import { formatDate, todayInCairo } from '@/shared/format/datetime'
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
// from the customer's list: the type and the open workflow step of a started request
const type = ref<string | null>(null)
const step = ref<string | null>(null)
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

// the strip's limits: TO_CHAR(FACILITY_LIMIT, 'FM999,999,999,999')
const wholeAmount = (n: number) => Math.round(n).toLocaleString('en-US')

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
      const row = (await disbursementApi.list(d.clientId)).find((x) => x.disbursementId === d.disbursementId)
      type.value = row?.disbursementType ?? null
      step.value = row?.workflowStepName ?? null
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
.dr-title { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }
.dr-title h1 { margin: 0; }
.dr-sub { margin: 4px 0 0; color: var(--fin-muted); font-size: var(--fs-sm); }
/* the customer strip, as the Factoring Customer Workspace's customer card */
.dr-strip { display: grid; grid-template-columns: minmax(240px, 1.5fr) repeat(5, minmax(110px, 1fr)); gap: 14px 18px; align-items: center; padding: 16px 18px; }
.dr-sec { margin-block-start: 14px; padding: 18px 20px; }
.dr-sec-head { display: flex; align-items: center; gap: 10px; margin: 0 0 16px; padding-bottom: 12px; border-bottom: 1px solid var(--fin-line-soft); }
.dr-sec-head h2 { display: flex; align-items: center; gap: 8px; margin: 0; font-size: 15px; color: var(--fin-teal-700); }
.dr-sec-head h2 .fa { color: var(--fin-orange); }
.dr-step { margin-inline-start: auto; display: flex; align-items: center; gap: 8px; font-size: 12px; color: var(--fin-muted); }
.dr-kv { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 18px 22px; }
.dr-kv > div > span { display: block; margin-bottom: 6px; font-size: var(--fs-xs); font-weight: 600; color: var(--fin-muted); }
.dr-kv b { font-size: 15px; color: var(--fin-ink); overflow-wrap: anywhere; }
.dr-kv .dr-amt { font-size: 18px; color: var(--fin-teal-700); }
.dr-kv .dr-amt small { font-size: 12px; color: var(--fin-muted); }
.dr-kv .dr-text { font-weight: 500; white-space: pre-wrap; }
.dr-form { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 14px 16px; }
.dr-wide { grid-column: 1 / -1; }
.dr-req { color: var(--fin-orange); }
.dr-amount { position: relative; display: block; }
.dr-amount input { width: 100%; padding-inline-end: 56px !important; text-align: end; }
.dr-amount em { position: absolute; inset-inline-end: 12px; top: 50%; transform: translateY(-50%); font-style: normal; font-size: 12px; font-weight: 700; color: var(--fin-muted); }
.dr-textarea { min-height: 70px; padding-top: 8px; resize: vertical; }
.dr-muted { color: var(--fin-muted); font-weight: 500; }
.dr-foot { display: flex; align-items: center; gap: 10px; margin-top: 18px; padding-top: 14px; border-top: 1px solid var(--fin-line-soft); }
.dr-note { display: flex; align-items: center; gap: 6px; font-size: var(--fs-xs); color: var(--fin-muted); }
.dr-buttons { margin-inline-start: auto; display: flex; gap: 8px; }
@media (max-width: 1100px) { .dr-strip { grid-template-columns: repeat(3, minmax(0, 1fr)); } .dr-strip .fc-customer-main { grid-column: 1 / -1; } }
@media (max-width: 760px) {
  .dr-strip, .dr-kv, .dr-form { grid-template-columns: 1fr; }
  .dr-strip .fc-info { border-inline-start: 0; padding-inline-start: 0; }
  .dr-foot { flex-direction: column; align-items: stretch; }
}
</style>
