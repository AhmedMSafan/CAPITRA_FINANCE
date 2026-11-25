<template>
  <!--
    Customer 360 > Consumer Finance Supplier tab: the customer's supplier application (Ahmed, 25 Nov 2026). Sales enters the supplier, its agreement terms, owners and
    signatories and documents, then submits it to CF_SUPPLIER_APPROVAL. Opened from the inbox (?inboxId=), it is the page of the step:
    compliance records the screening and the risk rating, the site visit its date and report, the contract step the signed agreement
    and the bank account. Accepting the contract step activates the supplier. No step is done by the person who entered it.
  -->
  <div class="cf-panel">
    <header class="c3-panel-head cf-head">
      <h3>
        <span class="fa fa-handshake-o" aria-hidden="true"></span>
        <span v-if="task" class="fc-eyebrow">{{ tr('WORKFLOW TASK') }} · </span>
        <span dir="ltr">{{ app ? app.applicationRef : tr('New supplier application') }}</span>
        <span v-if="app" class="fx-chip" :class="CF_CHIP[app.statusCode]">{{ tr(CF_STATUS[app.statusCode] ?? app.statusCode) }}</span>
      </h3>
      <button v-if="task" type="button" class="fx-btn" @click="router.push({ name: 'workflow-inbox' })"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Task') }}</button>
    </header>
    <p v-if="!app" class="c3-muted cf-intro">{{ tr('Enter the agreement terms and save; then add the owners, signatories and documents, and submit for approval.') }}</p>

    <!-- where the application is -->
    <section v-if="app" class="fx-card">
      <ol class="fc-process">
        <li v-for="(s, i) in process" :key="s.code" :class="{ 'is-done': processIndex > i, 'is-current': processIndex === i }">
          <span class="fc-process-no">{{ processIndex > i ? '✓' : i + 1 }}</span>
          <div><b>{{ tr(s.title) }}</b><small>{{ tr(s.who) }}</small></div>
        </li>
      </ol>
    </section>

    <div v-if="app?.statusCode === 'RETURNED' && app.returnReason" class="fc-note fc-note--warn fc-step-card">
      <span class="fa fa-undo" aria-hidden="true"></span><span><b>{{ tr('Returned') }}:</b> {{ app.returnReason }}</span>
    </div>
    <div v-if="app?.statusCode === 'ACTIVE'" class="fc-note fc-step-card">
      <span class="fa fa-check-circle" aria-hidden="true"></span><span>{{ tr('The supplier is active and its agreement is in force from') }} {{ formatDate(app.validFromDate) }}.</span>
    </div>

    <!-- agreement terms -->
    <section class="fx-card fc-step-card">
      <header class="fc-sec-head">
        <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-file-text"></span></span>
        <div><h2>{{ tr('Agreement terms') }}</h2><p>{{ editable ? tr('Fields marked * are required.') : tr('Read only: the application is in its workflow or closed.') }}</p></div>
      </header>
      <div class="fc-grid fc-grid--3">
        <div class="fc-field fc-full"><span>{{ tr('Goods the supplier sells on finance') }} <b>*</b></span>
          <div class="cf-cats">
            <label v-for="g in options?.goodsCategories ?? []" :key="g.code" class="cf-cat" :class="{ 'is-on': form.goodsCategoryCodes.includes(g.code) }">
              <input v-model="form.goodsCategoryCodes" type="checkbox" :value="g.code" :disabled="!editable">{{ tr(g.name) }}
            </label>
          </div>
          <small v-if="errors.goodsCategoryCodes" class="scr-err">{{ errors.goodsCategoryCodes }}</small>
        </div>
        <label class="fc-field"><span>{{ tr('Minimum down payment %') }} <b>*</b></span>
          <input v-model.number="form.minDownPaymentPct" class="fx-search" type="number" min="0" max="99.99" step="0.01" :disabled="!editable">
          <small v-if="errors.minDownPaymentPct" class="scr-err">{{ errors.minDownPaymentPct }}</small></label>
        <label class="fc-field"><span>{{ tr('Down payment taken by') }} <b>*</b></span>
          <select v-model="form.downPaymentCollectorCode" class="fx-select" :disabled="!editable">
            <option v-for="o in options?.collectors ?? []" :key="o.code" :value="o.code">{{ tr(o.name) }}</option>
          </select></label>
        <label class="fc-field"><span>{{ tr('Commission %') }} <b>*</b></span>
          <input v-model.number="form.commissionPct" class="fx-search" type="number" min="0" max="99.99" step="0.01" :disabled="!editable">
          <small v-if="errors.commissionPct" class="scr-err">{{ errors.commissionPct }}</small></label>
        <label class="fc-field"><span>{{ tr('Payment days after delivery') }} <b>*</b></span>
          <input v-model.number="form.paymentDays" class="fx-search" type="number" min="0" max="365" :disabled="!editable">
          <small v-if="errors.paymentDays" class="scr-err">{{ errors.paymentDays }}</small></label>
        <label class="fc-field"><span>{{ tr('Quotation validity days') }} <b>*</b></span>
          <input v-model.number="form.quotationValidityDays" class="fx-search" type="number" min="1" max="90" :disabled="!editable">
          <small v-if="errors.quotationValidityDays" class="scr-err">{{ errors.quotationValidityDays }}</small></label>
        <label class="fc-field"><span>{{ tr('Delivery days') }} <b>*</b></span>
          <input v-model.number="form.deliveryDays" class="fx-search" type="number" min="1" max="90" :disabled="!editable">
          <small v-if="errors.deliveryDays" class="scr-err">{{ errors.deliveryDays }}</small></label>
        <label class="fc-field"><span>{{ tr('Valid from') }} <b>*</b></span>
          <input v-model="form.validFromDate" class="fx-search" type="date" :disabled="!editable"></label>
        <label class="fc-field"><span>{{ tr('Valid to') }}</span>
          <input v-model="form.validToDate" class="fx-search" type="date" :disabled="!editable">
          <small v-if="errors.validToDate" class="scr-err">{{ errors.validToDate }}</small></label>
        <label class="fc-field"><span>{{ tr('Bank account to pay') }} <b v-if="atStep('CFS_CONTRACT')">*</b></span>
          <select v-model="form.clientBankAccountId" class="fx-select" :disabled="!editable && !atStep('CFS_CONTRACT')" @change="bankChanged">
            <option :value="null">—</option>
            <option v-for="b in app?.bankAccounts ?? []" :key="b.bankAccountId" :value="b.bankAccountId">
              {{ b.bankName }} · {{ b.iban || b.accountNumber }} · {{ tr(BANK_STATUS[b.statusCode] ?? b.statusCode) }}
            </option>
          </select>
          <small class="scr-hint">{{ tr('It must be verified in Customer 360 before the contract step is accepted.') }}</small></label>
        <label class="fc-field fc-full"><span>{{ tr('Notes') }}</span><textarea v-model="form.notes" class="fx-search fc-textarea" maxlength="2000" :disabled="!editable"></textarea></label>
      </div>
    </section>

    <!-- owners, managers and signatories -->
    <section v-if="app" class="fx-card fc-step-card">
      <header class="fc-sec-head">
        <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-users"></span></span>
        <div><h2>{{ tr('Owners and signatories') }} ({{ app.parties.length }})</h2>
          <p>{{ tr('At least one owner and one authorised signatory.') }}</p></div>
      </header>
      <div v-if="editable" class="scr-toolbar cf-party-add">
        <select v-model="party.partyTypeCode" class="fx-select" :aria-label="tr('Type')"><option v-for="o in options?.partyTypes ?? []" :key="o.code" :value="o.code">{{ tr(o.name) }}</option></select>
        <input v-model="party.fullName" class="fx-search" :placeholder="tr('Full name')" maxlength="300">
        <input v-model="party.nationalIdNo" class="fx-search" :placeholder="tr('National ID / passport')" maxlength="50" dir="ltr">
        <input v-model="party.nationality" class="fx-search" :placeholder="tr('Nationality')" maxlength="100">
        <input v-if="party.partyTypeCode === 'OWNER'" v-model.number="party.ownershipPct" class="fx-search cf-share" type="number" min="0" max="100" step="0.01" :placeholder="tr('Share %')">
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !party.fullName.trim()" @click="addParty"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Add') }}</button>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Type') }}</th><th>{{ tr('Name') }}</th><th>{{ tr('National ID / passport') }}</th><th>{{ tr('Nationality') }}</th><th class="num">{{ tr('Share %') }}</th><th>{{ tr('Screening') }}</th><th v-if="editable"></th></tr></thead>
          <tbody>
            <tr v-for="p in app.parties" :key="p.partyId">
              <td>{{ tr(PARTY[p.partyTypeCode] ?? p.partyTypeCode) }}</td>
              <td><b>{{ p.fullName }}</b></td>
              <td dir="ltr">{{ p.nationalIdNo || '—' }}</td>
              <td>{{ p.nationality || '—' }}</td>
              <td class="num">{{ p.ownershipPct ?? '' }}</td>
              <td>
                <span v-if="p.screeningResultCode" class="fx-chip" :class="p.screeningResultCode === 'CLEAR' ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(p.screeningResultCode === 'CLEAR' ? 'Clear' : 'Match') }}</span>
                <span v-else class="c3-muted">{{ tr('Not screened') }}</span>
                <small v-if="p.screenedBy" class="fx-cell-sub">{{ p.screenedBy }}</small>
              </td>
              <td v-if="editable" class="fx-r">
                <button type="button" class="fx-btn fx-btn--sm fc-btn-reject" :disabled="busy" @click="removeParty(p.partyId)"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Remove') }}</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="!app.parties.length" class="fx-empty"><span class="fa fa-users" aria-hidden="true"></span>{{ tr('No owners or signatories yet.') }}</div>
    </section>

    <!-- the step box (Ahmed, 25 Nov 2026): only what the open step enters, with what earlier steps decided as plain text;
         later steps' fields are not shown -->
    <section v-if="app && (atStep('CFS_COMPLIANCE') || atStep('CFS_SITE_VISIT') || atStep('CFS_CONTRACT') || app.riskRatingCode || app.siteVisitDate)" class="fx-card fc-step-card">
      <header class="fc-sec-head">
        <span class="fc-sec-ic" aria-hidden="true"><span class="fa" :class="atStep('CFS_SITE_VISIT') ? 'fa-map-marker' : 'fa-shield'"></span></span>
        <div><h2>{{ tr(atStep('CFS_COMPLIANCE') ? 'Compliance' : atStep('CFS_SITE_VISIT') ? 'Site visit' : atStep('CFS_CONTRACT') ? 'Contract signing' : 'Compliance and site visit') }}</h2>
          <p>{{ tr(atStep('CFS_COMPLIANCE') ? 'Screen each person against sanctions lists, blacklists and PEP lists, then set the risk rating. A HIGH risk supplier also needs every committee member.'
            : atStep('CFS_SITE_VISIT') ? 'Enter the visit date and add the visit report to the documents.' : 'What the earlier steps decided.') }}</p></div>
      </header>

      <!-- what earlier steps decided -->
      <div v-if="(app.riskRatingCode && !atStep('CFS_COMPLIANCE')) || (app.siteVisitDate && !atStep('CFS_SITE_VISIT'))" class="cf-facts">
        <span v-if="app.riskRatingCode && !atStep('CFS_COMPLIANCE')">{{ tr('Risk rating') }}: <b>{{ tr(riskName(app.riskRatingCode)) }}</b></span>
        <span v-if="app.siteVisitDate && !atStep('CFS_SITE_VISIT')">{{ tr('Site visit date') }}: <b>{{ formatDate(app.siteVisitDate) }}</b></span>
      </div>

      <!-- compliance: the screening of every owner and signatory, and the risk rating -->
      <template v-if="atStep('CFS_COMPLIANCE')">
        <div class="fx-table-wrap cf-screen">
          <table class="fx-table">
            <thead><tr><th>{{ tr('Name') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('National ID / passport') }}</th><th>{{ tr('Screening') }} <b>*</b></th></tr></thead>
            <tbody>
              <tr v-for="p in app.parties" :key="p.partyId">
                <td><b>{{ p.fullName }}</b></td>
                <td>{{ tr(PARTY[p.partyTypeCode] ?? p.partyTypeCode) }}</td>
                <td dir="ltr">{{ p.nationalIdNo || '—' }}</td>
                <td>
                  <select v-model="screening[p.partyId]" class="fx-select fx-select--sm" :aria-label="`${tr('Screening')}: ${p.fullName}`">
                    <option :value="null">{{ tr('Not screened') }}</option><option value="CLEAR">{{ tr('Clear') }}</option><option value="MATCH">{{ tr('Match') }}</option>
                  </select>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div class="cf-step-row">
          <label class="fc-field"><span>{{ tr('Risk rating') }} <b>*</b></span>
            <select v-model="riskRating" class="fx-select">
              <option :value="null">—</option><option v-for="o in options?.riskRatings ?? []" :key="o.code" :value="o.code">{{ tr(o.name) }}</option>
            </select></label>
          <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="saveCompliance"><span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save screening and rating') }}</button>
        </div>
      </template>

      <!-- site visit: the date; the report goes to the documents -->
      <template v-if="atStep('CFS_SITE_VISIT')">
        <div class="cf-step-row">
          <label class="fc-field"><span>{{ tr('Site visit date') }} <b>*</b></span>
            <input v-model="siteVisitDate" class="fx-search" type="date"></label>
          <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="saveSiteVisit"><span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save visit date') }}</button>
        </div>
        <p class="c3-muted cf-hint">{{ tr('Add the visit report (type: Site visit report) and photos to the documents below.') }}</p>
      </template>
      <p v-if="atStep('CFS_CONTRACT')" class="c3-muted cf-hint">{{ tr('Add the signed agreement (type: Signed agreement) to the documents below, and choose the verified bank account above.') }}</p>
    </section>

    <!-- the application's documents: one list, added at any time -->
    <CfSupplierDocuments v-if="app" :application-id="app.applicationId" :status-code="app.statusCode" :types="options?.documentTypes ?? []"
                         :read-only="false" :version="version" />

    <!-- every step with its decision -->
    <CfSupplierSteps v-if="app" :application-id="app.applicationId" :focus-step="null" :version="version" />

    <!-- data entry actions -->
    <section v-if="editable" class="fx-card fc-step-card">
      <div class="fc-form-actions" style="padding-top:16px">
        <div class="fc-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
          <span>{{ app ? tr('Add the owners and signatories, then submit for approval. Documents can be added now or at any step.') : tr('Save first; then add the owners, signatories and documents.') }}</span></div>
        <button type="button" class="fx-btn" :disabled="busy" @click="save"><span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save') }}</button>
        <button v-if="app" type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="submit"><span class="fa fa-paper-plane" aria-hidden="true"></span>{{ tr('Submit for approval') }}</button>
      </div>
    </section>

    <!-- the workflow step -->
    <section v-if="task && task.status !== 'COMPLETED'" class="fx-card fc-complete">
      <label class="fc-field"><span>{{ tr(app?.openStepName ?? 'Step') }} · {{ tr('Comment') }} <small class="scr-hint">{{ tr('(required to return)') }}</small></span>
        <textarea v-model="comment" class="fx-search fc-textarea" :placeholder="tr(STEP_HINT[task.stepCode ?? ''] ?? '')"></textarea>
        <small v-if="!taskOpen" class="scr-hint">{{ tr('Claim and start the task from the inbox first.') }}</small>
      </label>
      <button type="button" class="fx-btn fc-btn-reject" :disabled="busy || !taskOpen" @click="complete('REJECT')"><span class="fa fa-undo" aria-hidden="true"></span>{{ tr('Return to Sales') }}</button>
      <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !taskOpen" @click="complete('ACCEPT')">
        <span class="fa fa-check" aria-hidden="true"></span>{{ task.stepCode === 'CFS_CONTRACT' ? tr('Accept and activate the supplier') : tr('Accept') }}
      </button>
    </section>
  </div>
</template>

<script setup lang="ts">
import '@/modules/scr/components/scr.css'
import { computed, reactive, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { workflowService } from '@/modules/wfe/services/workflowService'
import type { WorkflowInboxTask } from '@/modules/wfe/models/workflow'
import { errorText, fieldErrors } from '@/modules/scr/api/scr.api'
import CfSupplierDocuments from './CfSupplierDocuments.vue'
import CfSupplierSteps from './CfSupplierSteps.vue'
import { CF_CHIP, CF_STATUS, cfSuppliersApi, type CfOptions, type CfPartyRequest, type CfSupplier, type CfSupplierRequest } from '../api/cfSuppliers.api'

const props = defineProps<{ clientId: number; inboxId: number | null }>()
const emit = defineEmits<{ changed: [] }>()
const router = useRouter()
const canEdit = hasPermission('CRM.CF_SUPPLIER.EDIT')

const PARTY: Record<string, string> = { OWNER: 'Owner', MANAGER: 'Manager', SIGNATORY: 'Authorised signatory' }
const riskName = (code: string) => options.value?.riskRatings.find((o) => o.code === code)?.name ?? code
const BANK_STATUS: Record<string, string> = { PENDING: 'Pending verification', VERIFIED: 'Verified', SUSPENDED: 'Suspended' }
const STEP_HINT: Record<string, string> = {
  CFS_DOCUMENT_CHECK: 'Check the commercial register, tax card, activity licence and the bank letter (account in the supplier\'s own name).',
  CFS_COMPLIANCE: 'Screen every owner, manager and signatory, set the risk rating and save them before accepting.',
  CFS_SITE_VISIT: 'Enter the visit date and add the visit report (type: Site visit report) to the documents before accepting.',
  CFS_RISK_REVIEW: 'Review fraud indicators, the sector and the agreement terms; write your recommendation.',
  CFS_MANAGER_APPROVAL: 'Approve the supplier and its agreement terms. A HIGH risk supplier then goes to the committee.',
  CFS_COMMITTEE_APPROVAL: 'Every committee member approves a HIGH risk supplier.',
  CFS_CONTRACT: 'Add the signed agreement (type: Signed agreement) to the documents and choose the verified bank account. Accepting activates the supplier.'
}

const app = ref<CfSupplier | null>(null)
const options = ref<CfOptions | null>(null)
const task = ref<WorkflowInboxTask | null>(null)
const version = ref(0)
const busy = ref(false)
const comment = ref('')
const errors = ref<Record<string, string>>({})
const riskRating = ref<string | null>(null)
const siteVisitDate = ref<string | null>(null)
const screening = reactive<Record<number, string | null>>({})

const blank = (): CfSupplierRequest => ({
  goodsCategoryCodes: [], minDownPaymentPct: 10, downPaymentCollectorCode: 'SUPPLIER', commissionPct: 0, paymentDays: 7, quotationValidityDays: 7,
  deliveryDays: 5, validFromDate: null, validToDate: null, clientBankAccountId: null, notes: null
})
const form = reactive<CfSupplierRequest>(blank())
const party = reactive<CfPartyRequest>({ partyTypeCode: 'OWNER', fullName: '', nationalIdNo: null, nationality: null, ownershipPct: null })

const editable = computed(() => canEdit && !task.value && (!app.value || app.value.canEdit))
const taskOpen = computed(() => !!task.value && (task.value.status === 'IN_PROGRESS' || task.value.status === 'CLAIMED') && task.value.instanceId === app.value?.workflowInstanceId)
const atStep = (code: string) => taskOpen.value && task.value?.stepCode === code

const process = computed(() => {
  const list = [
    { code: 'DATA_ENTRY', title: 'Data entry', who: 'Sales' },
    { code: 'CFS_DOCUMENT_CHECK', title: 'Document check', who: 'Operations' },
    { code: 'CFS_COMPLIANCE', title: 'Compliance', who: 'Compliance' },
    { code: 'CFS_SITE_VISIT', title: 'Site visit', who: 'Sales or Risk' },
    { code: 'CFS_RISK_REVIEW', title: 'Risk review', who: 'Risk' },
    { code: 'CFS_MANAGER_APPROVAL', title: 'Manager approval', who: 'Manager' }
  ]
  if (app.value?.riskRatingCode === 'HIGH') list.push({ code: 'CFS_COMMITTEE_APPROVAL', title: 'Committee approval', who: 'Committee' })
  list.push({ code: 'CFS_CONTRACT', title: 'Contract signing', who: 'Legal' }, { code: 'ACTIVE', title: 'Active', who: '' })
  return list
})
const processIndex = computed(() => {
  const a = app.value
  if (!a || a.statusCode === 'DRAFT' || a.statusCode === 'RETURNED' || a.statusCode === 'WITHDRAWN') return 0
  if (a.statusCode === 'ACTIVE') return process.value.length - 1
  return Math.max(0, process.value.findIndex((s) => s.code === a.openStepCode))
})

function fill(a: CfSupplier) {
  app.value = a
  version.value++
  Object.assign(form, {
    goodsCategoryCodes: [...a.goodsCategoryCodes], minDownPaymentPct: a.minDownPaymentPct, downPaymentCollectorCode: a.downPaymentCollectorCode,
    commissionPct: a.commissionPct, paymentDays: a.paymentDays, quotationValidityDays: a.quotationValidityDays, deliveryDays: a.deliveryDays,
    validFromDate: a.validFromDate, validToDate: a.validToDate, clientBankAccountId: a.clientBankAccountId, notes: a.notes
  })
  riskRating.value = a.riskRatingCode
  siteVisitDate.value = a.siteVisitDate
  for (const p of a.parties) screening[p.partyId] = p.screeningResultCode
}

async function run(work: () => Promise<void>) {
  busy.value = true
  errors.value = {}
  try {
    await work()
  } catch (err) {
    errors.value = fieldErrors(err)
    showError(Object.keys(errors.value).length ? Object.values(errors.value).join('\n') : errorText(err))
  } finally {
    busy.value = false
  }
}

const save = () => run(async () => {
  if (app.value) {
    fill(await cfSuppliersApi.update(app.value.applicationId, { ...form }))
    showSuccess(tr('Saved.'))
    return
  }
  const created = await cfSuppliersApi.create({ ...form, clientId: props.clientId })
  showSuccess(`${tr('Supplier application created')}: ${created.applicationRef}`)
  fill(created)
  emit('changed')
})

const addParty = () => run(async () => {
  fill(await cfSuppliersApi.addParty(app.value!.applicationId, { ...party, nationalIdNo: party.nationalIdNo || null, nationality: party.nationality || null }))
  Object.assign(party, { fullName: '', nationalIdNo: null, nationality: null, ownershipPct: null })
})

const removeParty = (partyId: number) => run(async () => { fill(await cfSuppliersApi.removeParty(app.value!.applicationId, partyId)) })

const submit = () => run(async () => {
  await cfSuppliersApi.update(app.value!.applicationId, { ...form })
  fill(await cfSuppliersApi.submit(app.value!.applicationId))
  showSuccess(tr('Submitted for approval.'))
  emit('changed')
})

const saveCompliance = () => run(async () => {
  fill(await cfSuppliersApi.saveCompliance(app.value!.applicationId, task.value!.inboxId, riskRating.value,
    app.value!.parties.map((p) => ({ partyId: p.partyId, resultCode: screening[p.partyId] ?? null }))))
  showSuccess(tr('Saved.'))
})

const saveSiteVisit = () => run(async () => {
  fill(await cfSuppliersApi.saveSiteVisit(app.value!.applicationId, task.value!.inboxId, siteVisitDate.value || null))
  showSuccess(tr('Saved.'))
})

/** At the contract step the bank account is chosen on its own (the terms are read only there). */
function bankChanged() {
  if (!atStep('CFS_CONTRACT')) return
  void run(async () => { fill(await cfSuppliersApi.saveBankAccount(app.value!.applicationId, task.value!.inboxId, form.clientBankAccountId)) })
}

const complete = (action: 'ACCEPT' | 'REJECT') => run(async () => {
  if (action === 'REJECT' && !comment.value.trim()) {
    errors.value = { comment: tr('Write why the application is returned.') }
    showError(tr('Write why the application is returned.'))
    return
  }
  // what was entered at the step is kept before the step is accepted
  if (action === 'ACCEPT' && atStep('CFS_COMPLIANCE'))
    await cfSuppliersApi.saveCompliance(app.value!.applicationId, task.value!.inboxId, riskRating.value,
      app.value!.parties.map((p) => ({ partyId: p.partyId, resultCode: screening[p.partyId] ?? null })))
  if (action === 'ACCEPT' && atStep('CFS_SITE_VISIT'))
    await cfSuppliersApi.saveSiteVisit(app.value!.applicationId, task.value!.inboxId, siteVisitDate.value || null)
  await cfSuppliersApi.completeStep(app.value!.applicationId, task.value!.inboxId, action, comment.value.trim() || null)
  showSuccess(action === 'REJECT' ? tr('Returned to Sales.') : task.value!.stepCode === 'CFS_CONTRACT' ? tr('The supplier is active.') : tr('Step accepted.'))
  await router.push({ name: 'workflow-inbox' })
})

/** The customer's application (none yet: a new one, saved for this customer) and, from the inbox, the user's task on it. */
async function load() {
  try {
    const jobs: Promise<unknown>[] = [cfSuppliersApi.byClient(props.clientId).then(async (r) => {
      if (r.applicationId) fill(await cfSuppliersApi.get(r.applicationId))
      else app.value = null
    })]
    if (!options.value) jobs.push(cfSuppliersApi.options().then((o) => { options.value = o }))
    task.value = null
    if (props.inboxId) jobs.push(workflowService.getTask(props.inboxId).then((t) => { task.value = t }))
    await Promise.all(jobs)
  } catch (err) {
    showError(errorText(err))
  }
}

watch(() => [props.clientId, props.inboxId], load, { immediate: true })
</script>

<style scoped>
.cf-panel { display: flex; flex-direction: column; gap: 14px; }
.cf-head { display: flex; align-items: center; justify-content: space-between; gap: 10px; }
.cf-head h3 { display: flex; align-items: center; gap: 8px; margin: 0; }
.cf-intro { margin: 0; }
.cf-cats { display: flex; flex-wrap: wrap; gap: 8px; }
.cf-cat { display: inline-flex; align-items: center; gap: 6px; padding: 6px 10px; border: 1px solid var(--fin-border, #dbe3ea); border-radius: 999px; cursor: pointer; font-size: 13px; }
.cf-cat.is-on { border-color: var(--fin-teal, #00636d); background: var(--fin-teal-50, #eef9f8); }
.cf-party-add { flex-wrap: wrap; }
.cf-party-add .fx-search { min-width: 160px; flex: 1; }
.cf-share { max-width: 110px; }
.cf-facts { display: flex; flex-wrap: wrap; gap: 6px 20px; margin: 0 16px 4px; font-size: 13px; color: var(--fin-muted); }
.cf-facts b { color: var(--fin-ink); }
.cf-screen { margin: 0 16px; }
.cf-step-row { display: flex; align-items: flex-end; gap: 12px; padding: 12px 16px 16px; }
.cf-step-row .fc-field { width: 260px; }
.cf-hint { margin: 0 16px 14px; font-size: 12px; }
.num { text-align: end; }
</style>
