<template>
  <!--
    CRM > Consumer Finance suppliers > one application (Ahmed, 25 Nov 2026). Sales enters the supplier, its agreement terms, owners and
    signatories and documents, then submits it to CF_SUPPLIER_APPROVAL. Opened from the inbox (?inboxId=), it is the page of the step:
    compliance records the screening and the risk rating, the site visit its date and report, the contract step the signed agreement
    and the bank account. Accepting the contract step activates the supplier. No step is done by the person who entered it.
  -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'crm-cf-suppliers' }">{{ tr('Consumer Finance Suppliers') }}</RouterLink><span aria-hidden="true">›</span>
          <b dir="ltr">{{ app?.applicationRef ?? tr('New supplier application') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-handshake-o"></span></span>
          <div>
            <span v-if="task" class="fc-eyebrow">{{ tr('WORKFLOW TASK') }}</span>
            <h1>
              <span dir="ltr">{{ app ? app.applicationRef : tr('New supplier application') }}</span>
              <span v-if="app" class="fx-chip" :class="CF_CHIP[app.statusCode]">{{ tr(CF_STATUS[app.statusCode] ?? app.statusCode) }}</span>
            </h1>
            <p>{{ app ? `${app.clientName} · ${app.clientNumber}` : tr('A merchant that sells goods on consumer finance, with its agreement terms.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <button v-if="task" type="button" class="fx-btn" @click="router.push({ name: 'workflow-inbox' })"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Task') }}</button>
        <RouterLink v-else :to="{ name: 'crm-cf-suppliers' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Consumer Finance Suppliers') }}</RouterLink>
      </div>
    </div>

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

    <!-- the supplier -->
    <section class="fx-card fc-step-card">
      <header class="fc-sec-head">
        <span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-building-o"></span></span>
        <div><h2>{{ tr('Supplier') }}</h2><p>{{ tr('The customer record of the merchant. Its compliance profile and bank accounts are kept in Customer 360.') }}</p></div>
      </header>
      <div v-if="!app" class="fc-grid fc-grid--3">
        <label class="fc-field fc-span2"><span>{{ tr('Customer') }} <b>*</b></span>
          <input v-model="clientSearch" class="fx-search" :placeholder="tr('Type a name, number or register number')" @input="findClients">
          <select v-model="clientId" class="fx-select" :size="clients.length > 1 ? Math.min(clients.length, 5) : undefined">
            <option v-for="c in clients" :key="c.clientId" :value="c.clientId">{{ c.clientName }} · {{ c.clientNumber }}</option>
          </select>
          <small v-if="errors.clientId" class="scr-err">{{ errors.clientId }}</small>
        </label>
      </div>
      <div v-else class="fc-facts">
        <div class="fc-fact"><span class="fa fa-building" aria-hidden="true"></span><div><span>{{ tr('Customer') }}</span>
          <RouterLink :to="{ name: 'crm-customer-workspace', params: { clientId: app.clientId } }"><b>{{ app.clientName }}</b></RouterLink><small>{{ app.clientNumber }}</small></div></div>
        <div class="fc-fact"><span class="fa fa-id-card-o" aria-hidden="true"></span><div><span>{{ tr('Commercial register') }}</span><b dir="ltr">{{ app.registrationNumber || '—' }}</b></div></div>
        <div class="fc-fact"><span class="fa fa-file-text-o" aria-hidden="true"></span><div><span>{{ tr('Tax number') }}</span><b dir="ltr">{{ app.taxNumber || '—' }}</b></div></div>
        <div class="fc-fact"><span class="fa fa-tag" aria-hidden="true"></span><div><span>{{ tr('Supplier role') }}</span><b>{{ tr(ROLE_STATUS[app.roleStatusCode] ?? app.roleStatusCode) }}</b></div></div>
      </div>
    </section>

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
          <p>{{ atStep('CFS_COMPLIANCE') ? tr('Screen each person against sanctions lists, blacklists and PEP lists, and record the result.') : tr('At least one owner and one authorised signatory.') }}</p></div>
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
                <select v-if="atStep('CFS_COMPLIANCE')" v-model="screening[p.partyId]" class="fx-select fx-select--sm" :aria-label="tr('Screening')">
                  <option :value="null">{{ tr('Not screened') }}</option><option value="CLEAR">{{ tr('Clear') }}</option><option value="MATCH">{{ tr('Match') }}</option>
                </select>
                <template v-else>
                  <span v-if="p.screeningResultCode" class="fx-chip" :class="p.screeningResultCode === 'CLEAR' ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(p.screeningResultCode === 'CLEAR' ? 'Clear' : 'Match') }}</span>
                  <span v-else class="c3-muted">{{ tr('Not screened') }}</span>
                  <small v-if="p.screenedBy" class="fx-cell-sub">{{ p.screenedBy }}</small>
                </template>
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

    <!-- compliance and site visit, filled at their steps -->
    <section v-if="app && app.statusCode !== 'DRAFT'" class="fx-card fc-step-card">
      <header class="fc-sec-head">
        <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-shield"></span></span>
        <div><h2>{{ tr('Compliance and site visit') }}</h2><p>{{ tr('A HIGH risk supplier also needs the approval of every committee member.') }}</p></div>
      </header>
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Risk rating') }} <b v-if="atStep('CFS_COMPLIANCE')">*</b></span>
          <select v-model="riskRating" class="fx-select" :disabled="!atStep('CFS_COMPLIANCE')">
            <option :value="null">—</option><option v-for="o in options?.riskRatings ?? []" :key="o.code" :value="o.code">{{ tr(o.name) }}</option>
          </select></label>
        <label class="fc-field"><span>{{ tr('Site visit date') }} <b v-if="atStep('CFS_SITE_VISIT')">*</b></span>
          <input v-model="siteVisitDate" class="fx-search" type="date" :disabled="!atStep('CFS_SITE_VISIT')"></label>
        <div class="fc-field cf-step-save">
          <button v-if="atStep('CFS_COMPLIANCE')" type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="saveCompliance"><span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save screening and rating') }}</button>
          <button v-if="atStep('CFS_SITE_VISIT')" type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="saveSiteVisit"><span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save visit date') }}</button>
        </div>
      </div>
      <p v-if="atStep('CFS_SITE_VISIT')" class="c3-muted cf-hint">{{ tr('Attach the visit report and photos at the Site visit step below.') }}</p>
      <p v-if="atStep('CFS_CONTRACT')" class="c3-muted cf-hint">{{ tr('Attach the signed supplier agreement at the Contract signing step below, and choose the verified bank account above.') }}</p>
    </section>

    <!-- every step, completed ones view-only, with its documents -->
    <CfSupplierSteps v-if="app" :application-id="app.applicationId" :data-entry-open="editable" :inbox-id="taskOpen ? task!.inboxId : null"
                     :task-step-code="task?.stepCode ?? null" :read-only="false" :focus-step="null" :version="version" />

    <!-- data entry actions -->
    <section v-if="editable" class="fx-card fc-step-card">
      <div class="fc-form-actions" style="padding-top:16px">
        <div class="fc-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
          <span>{{ app ? tr('Attach the commercial register, tax card and bank letter at Data entry, then submit for approval.') : tr('Save first; then add the owners, signatories and documents.') }}</span></div>
        <button v-if="app" type="button" class="fx-btn fc-btn-reject" :disabled="busy" @click="withdraw"><span class="fa fa-ban" aria-hidden="true"></span>{{ tr('Withdraw') }}</button>
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
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '@/modules/scr/components/scr.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { workflowService } from '@/modules/wfe/services/workflowService'
import type { WorkflowInboxTask } from '@/modules/wfe/models/workflow'
import { errorText, fieldErrors } from '@/modules/scr/api/scr.api'
import CfSupplierSteps from '../components/CfSupplierSteps.vue'
import { CF_CHIP, CF_STATUS, cfSuppliersApi, type CfClient, type CfOptions, type CfPartyRequest, type CfSupplier, type CfSupplierRequest } from '../api/cfSuppliers.api'

const props = defineProps<{ applicationId?: number }>()
const route = useRoute()
const router = useRouter()
const canEdit = hasPermission('CRM.CF_SUPPLIER.EDIT')

const PARTY: Record<string, string> = { OWNER: 'Owner', MANAGER: 'Manager', SIGNATORY: 'Authorised signatory' }
const ROLE_STATUS: Record<string, string> = { ONBOARDING: 'Onboarding', ACTIVE: 'Active', SUSPENDED: 'Suspended' }
const BANK_STATUS: Record<string, string> = { PENDING: 'Pending verification', VERIFIED: 'Verified', SUSPENDED: 'Suspended' }
const STEP_HINT: Record<string, string> = {
  CFS_DOCUMENT_CHECK: 'Check the commercial register, tax card, activity licence and the bank letter (account in the supplier\'s own name).',
  CFS_COMPLIANCE: 'Screen every owner, manager and signatory, set the risk rating and save them before accepting.',
  CFS_SITE_VISIT: 'Enter the visit date and attach the visit report and photos before accepting.',
  CFS_RISK_REVIEW: 'Review fraud indicators, the sector and the agreement terms; write your recommendation.',
  CFS_MANAGER_APPROVAL: 'Approve the supplier and its agreement terms. A HIGH risk supplier then goes to the committee.',
  CFS_COMMITTEE_APPROVAL: 'Every committee member approves a HIGH risk supplier.',
  CFS_CONTRACT: 'Attach the signed agreement and choose the verified bank account. Accepting activates the supplier.'
}

const app = ref<CfSupplier | null>(null)
const options = ref<CfOptions | null>(null)
const task = ref<WorkflowInboxTask | null>(null)
const version = ref(0)
const busy = ref(false)
const comment = ref('')
const errors = ref<Record<string, string>>({})
const clientSearch = ref('')
const clients = ref<CfClient[]>([])
const clientId = ref<number | null>(Number(route.query.clientId) || null)
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

let timer: ReturnType<typeof setTimeout> | undefined
function findClients() {
  clearTimeout(timer)
  timer = setTimeout(async () => {
    try {
      clients.value = await cfSuppliersApi.clients(clientSearch.value)
      if (clients.value.length === 1) clientId.value = clients.value[0]!.clientId
    } catch (err) {
      showError(errorText(err))
    }
  }, 300)
}

const save = () => run(async () => {
  if (app.value) {
    fill(await cfSuppliersApi.update(app.value.applicationId, { ...form }))
    showSuccess(tr('Saved.'))
    return
  }
  const created = await cfSuppliersApi.create({ ...form, clientId: clientId.value })
  showSuccess(`${tr('Supplier application created')}: ${created.applicationRef}`)
  await router.replace({ name: 'crm-cf-supplier', params: { applicationId: created.applicationId } })
  fill(created)
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
})

const withdraw = () => run(async () => {
  if (!window.confirm(tr('Withdraw this supplier application?'))) return
  fill(await cfSuppliersApi.withdraw(app.value!.applicationId))
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

onMounted(async () => {
  try {
    const inboxId = Number(route.query.inboxId)
    const jobs: Promise<unknown>[] = [cfSuppliersApi.options().then((o) => { options.value = o })]
    if (props.applicationId) jobs.push(cfSuppliersApi.get(props.applicationId).then(fill))
    if (Number.isFinite(inboxId) && inboxId > 0) jobs.push(workflowService.getTask(inboxId).then((t) => { task.value = t }))
    await Promise.all(jobs)
  } catch (err) {
    showError(errorText(err))
  }
})
</script>

<style scoped>
.cf-cats { display: flex; flex-wrap: wrap; gap: 8px; }
.cf-cat { display: inline-flex; align-items: center; gap: 6px; padding: 6px 10px; border: 1px solid var(--fin-border, #dbe3ea); border-radius: 999px; cursor: pointer; font-size: 13px; }
.cf-cat.is-on { border-color: var(--fin-teal, #00636d); background: var(--fin-teal-50, #eef9f8); }
.cf-party-add { flex-wrap: wrap; }
.cf-party-add .fx-search { min-width: 160px; flex: 1; }
.cf-share { max-width: 110px; }
.cf-step-save { justify-content: flex-end; flex-direction: row; align-items: flex-end; gap: 8px; }
.cf-hint { margin: 8px 16px 0; font-size: 12px; }
.num { text-align: end; }
</style>
