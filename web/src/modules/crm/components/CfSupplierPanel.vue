<template>
  <!--
    Customer 360 > Consumer Finance Supplier tab: the customer's supplier application (Ahmed, 25 Nov 2026). Sales enters the supplier, its agreement terms, owners and
    signatories and documents, then submits it to CF_SUPPLIER_APPROVAL. Opened from the inbox (?inboxId=), it is the page of the step:
    compliance checks the customer's compliance in Customer 360 (one place for KYC), the site visit its date and report, the contract step the signed agreement
    and the bank account. Accepting the contract step activates the supplier. No step is done by the person who entered it.
  -->
  <div class="cf-panel">
    <header class="c3-panel-head cf-head">
      <h3>
        <span class="fa fa-handshake-o" aria-hidden="true"></span>
        <span v-if="task" class="fc-eyebrow">{{ tr('WORKFLOW TASK') }} · </span>
        <span dir="ltr">{{ app ? app.applicationRef : tr('New supplier application') }}</span>
        <span v-if="app" class="fx-chip" :class="CF_CHIP[app.statusCode]">{{ tr(CF_STATUS[app.statusCode] ?? app.statusCode) }}</span>
        <span v-if="app?.openStepName" class="fx-chip fx-chip--plain">{{ tr('At') }}: {{ tr(app.openStepName) }}</span>
      </h3>
      <!-- the workflow steps and the audit trail open as popups (Ahmed, 27 Nov 2026); plain buttons, green is kept for a step's main action -->
      <div class="cf-head-btns">
        <button v-if="app" type="button" class="fx-btn" @click="showSteps = true"><span class="fa fa-sitemap" aria-hidden="true"></span>{{ tr('Workflow steps') }}</button>
        <button v-if="app" type="button" class="fx-btn" @click="showAudit = true">
          <span class="fa fa-history" aria-hidden="true"></span>{{ tr('Audit trail') }} <span class="cf-cnt">{{ app.termChanges.length }}</span>
        </button>
        <button v-if="canAmend && !amending && !app?.pendingChange" type="button" class="fx-btn" @click="amending = true"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Change terms') }}</button>
        <button v-if="task" type="button" class="fx-btn" @click="router.push({ name: 'workflow-inbox' })"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Task') }}</button>
      </div>
    </header>
    <p v-if="!app" class="c3-muted cf-intro">{{ tr('Enter the agreement terms and save; then add the owners, signatories and documents, and submit for approval.') }}</p>


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
        <div><h2>{{ tr('Agreement terms') }}</h2><p>{{ termsOpen ? tr('Fields marked * are required.') : tr('Read only: the application is in its workflow or closed.') }}</p></div>
      </header>
      <!-- a change of the terms waiting for another admin (Ahmed, 27 Nov 2026: the same standard as the lending terms) -->
      <div v-if="app?.pendingChange" class="cf-ban">
        <span class="fx-chip fx-chip--warn">{{ tr('Change waiting for approval') }}</span>
        <span class="cf-ban-txt">
          <template v-for="(c, i) in app.pendingChange.changes" :key="c.fieldName"><template v-if="i"> · </template>{{ tr(c.fieldName) }} <b>{{ c.oldValue ?? '—' }} → {{ c.newValue ?? '—' }}</b></template>
          · {{ tr('asked by') }} {{ app.pendingChange.requestedBy }} {{ formatDateTime(app.pendingChange.requestedAt) }} · <i>{{ app.pendingChange.reason }}</i>
        </span>
        <template v-if="hasPermission('CRM.CF_SUPPLIER.AMEND')">
          <button v-if="app.pendingChange.isMine" type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="takeBack"><span class="fa fa-undo" aria-hidden="true"></span>{{ tr('Take back') }}</button>
          <template v-if="app.pendingChange.canDecide">
            <button type="button" class="fx-btn fx-btn--sm fc-btn-reject" :disabled="busy" @click="rejectChange"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Reject') }}</button>
            <button type="button" class="fx-btn fx-btn--teal fx-btn--sm" :disabled="busy" @click="approveChange"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Approve') }}</button>
          </template>
        </template>
      </div>
      <div class="fc-grid fc-grid--3">
        <div class="fc-field fc-full"><span>{{ tr('Goods the supplier sells on finance') }} <b>*</b></span>
          <div class="cf-cats">
            <label v-for="g in options?.goodsCategories ?? []" :key="g.code" class="cf-cat" :class="{ 'is-on': form.goodsCategoryCodes.includes(g.code) }">
              <input v-model="form.goodsCategoryCodes" type="checkbox" :value="g.code" :disabled="!termsOpen">{{ tr(g.name) }}
            </label>
          </div>
          <small v-if="errors.goodsCategoryCodes" class="scr-err">{{ errors.goodsCategoryCodes }}</small>
        </div>
        <label class="fc-field"><span>{{ tr('Minimum down payment %') }} <b>*</b></span>
          <input v-model.number="form.minDownPaymentPct" class="fx-search" type="number" min="0" max="99.99" step="0.01" :disabled="!termsOpen">
          <small v-if="errors.minDownPaymentPct" class="scr-err">{{ errors.minDownPaymentPct }}</small></label>
        <label class="fc-field"><span>{{ tr('Down payment taken by') }} <b>*</b></span>
          <select v-model="form.downPaymentCollectorCode" class="fx-select" :disabled="!termsOpen">
            <option v-for="o in options?.collectors ?? []" :key="o.code" :value="o.code">{{ tr(o.name) }}</option>
          </select></label>
        <label class="fc-field"><span>{{ tr('Commission %') }} <b>*</b></span>
          <input v-model.number="form.commissionPct" class="fx-search" type="number" min="0" max="99.99" step="0.01" :disabled="!termsOpen">
          <small v-if="errors.commissionPct" class="scr-err">{{ errors.commissionPct }}</small></label>
        <label class="fc-field"><span>{{ tr('Payment days after delivery') }} <b>*</b></span>
          <input v-model.number="form.paymentDays" class="fx-search" type="number" min="0" max="365" :disabled="!termsOpen">
          <small v-if="errors.paymentDays" class="scr-err">{{ errors.paymentDays }}</small></label>
        <label class="fc-field"><span>{{ tr('Quotation validity days') }} <b>*</b></span>
          <input v-model.number="form.quotationValidityDays" class="fx-search" type="number" min="1" max="90" :disabled="!termsOpen">
          <small v-if="errors.quotationValidityDays" class="scr-err">{{ errors.quotationValidityDays }}</small></label>
        <label class="fc-field"><span>{{ tr('Delivery days') }} <b>*</b></span>
          <input v-model.number="form.deliveryDays" class="fx-search" type="number" min="1" max="90" :disabled="!termsOpen">
          <small v-if="errors.deliveryDays" class="scr-err">{{ errors.deliveryDays }}</small></label>
        <label class="fc-field"><span>{{ tr('Valid from') }} <b>*</b></span>
          <input v-model="form.validFromDate" class="fx-search" type="date" :disabled="!termsOpen"></label>
        <label class="fc-field"><span>{{ tr('Valid to') }}</span>
          <input v-model="form.validToDate" class="fx-search" type="date" :disabled="!termsOpen">
          <small v-if="errors.validToDate" class="scr-err">{{ errors.validToDate }}</small></label>
        <label class="fc-field"><span>{{ tr('Bank account to pay') }} <b v-if="atStep('CFS_CONTRACT')">*</b></span>
          <select v-model="form.clientBankAccountId" class="fx-select" :disabled="!termsOpen && !atStep('CFS_CONTRACT')" @change="bankChanged">
            <option :value="null">—</option>
            <option v-for="b in app?.bankAccounts ?? []" :key="b.bankAccountId" :value="b.bankAccountId">
              {{ b.bankName }} · {{ b.iban || b.accountNumber }} · {{ tr(BANK_STATUS[b.statusCode] ?? b.statusCode) }}
            </option>
          </select>
          <small class="scr-hint">{{ tr('It must be verified in Customer 360 before the contract step is accepted.') }}</small></label>
        <label class="fc-field fc-full"><span>{{ tr('Notes') }}</span><textarea v-model="form.notes" class="fx-search fc-textarea" maxlength="2000" :disabled="!termsOpen"></textarea></label>
      </div>
      <div v-if="amending" class="cf-amend">
        <p class="c3-muted"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('The change applies after another admin approves it, to new finances only; finances already created keep their terms.') }}</p>
        <label class="fc-field"><span>{{ tr('Reason for the change') }} <b>*</b></span><input v-model="amendReason" class="fx-search" maxlength="1000"></label>
        <div class="cf-amend-actions">
          <button type="button" class="fx-btn" :disabled="busy" @click="cancelAmend"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
          <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="saveAmend"><span class="fa fa-paper-plane" aria-hidden="true"></span>{{ tr('Send for approval') }}</button>
        </div>
      </div>
    </section>

    <!-- owners, managers and signatories -->
    <section v-if="app" class="fx-card fc-step-card">
      <header class="fc-sec-head">
        <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-users"></span></span>
        <div><h2>{{ tr('Owners and signatories') }} ({{ app.parties.length }})</h2>
          <p>{{ tr('At least one owner and one authorised signatory.') }}</p></div>
      </header>
      <div class="cf-from360">
        <span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Kept with the customer in Customer 360 > Compliance, with their screening.') }}
        <button type="button" class="cf-linkbtn" @click="emit('open-tab', 'compliance')">{{ tr('Open Compliance') }} <span class="fa fa-angle-right" aria-hidden="true"></span></button>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Type') }}</th><th>{{ tr('Name') }}</th><th>{{ tr('National ID / passport') }}</th><th>{{ tr('Nationality') }}</th><th class="num">{{ tr('Share %') }}</th><th>{{ tr('Screening') }}</th></tr></thead>
          <tbody>
            <tr v-for="p in app.parties" :key="p.partyId">
              <td>{{ tr(PARTY[p.partyTypeCode] ?? p.partyTypeCode) }}</td>
              <td><b>{{ p.fullName }}</b></td>
              <td dir="ltr">{{ p.nationalIdNo || '—' }}</td>
              <td>{{ p.nationality || '—' }}</td>
              <td class="num">{{ p.ownershipPct ?? '' }}</td>
              <td>
                <span v-if="p.screeningResultCode" class="fx-chip" :class="SCREEN_CHIP[p.screeningResultCode]">{{ tr(SCREEN_NAME[p.screeningResultCode] ?? p.screeningResultCode) }}</span>
                <span v-else class="c3-muted">{{ tr('Not screened') }}</span>
                <small v-if="p.screenedBy" class="fx-cell-sub">{{ p.screenedBy }}</small>
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

      <!-- compliance enters nothing here (Ahmed, 26 Nov 2026): it checks the customer's compliance in Customer 360 and accepts or returns -->
      <template v-if="atStep('CFS_COMPLIANCE')">
        <ul class="cf-checks">
          <li :class="['CLEAR', 'DUE'].includes(app.compliance.code) ? 'is-ok' : 'is-bad'">
            <span class="fa" :class="['CLEAR', 'DUE'].includes(app.compliance.code) ? 'fa-check-circle' : 'fa-times-circle'" aria-hidden="true"></span>
            {{ tr('Customer compliance') }}: <b>{{ tr(app.compliance.label) }}</b>
            <small v-if="app.compliance.reasons.length"> · {{ app.compliance.reasons.map((r) => tr(r)).join(' ') }}</small>
          </li>
          <li :class="app.customerRiskGrade ? 'is-ok' : 'is-bad'">
            <span class="fa" :class="app.customerRiskGrade ? 'fa-check-circle' : 'fa-times-circle'" aria-hidden="true"></span>
            {{ tr('Risk grade') }}: <b>{{ app.customerRiskGrade ? tr(riskName(app.customerRiskGrade)) : tr('not set') }}</b>
            <small v-if="app.customerRiskGrade === 'HIGH'"> · {{ tr('goes to the committee') }}</small>
          </li>
          <li :class="screenedAll ? 'is-ok' : 'is-bad'">
            <span class="fa" :class="screenedAll ? 'fa-check-circle' : 'fa-times-circle'" aria-hidden="true"></span>
            {{ tr('Owners and signatories screened, no sanctions match') }}: <b>{{ app.parties.filter((p) => p.screeningResultCode && p.screeningResultCode !== 'MATCH').length }} / {{ app.parties.length }}</b>
          </li>
        </ul>
        <div class="cf-step-row">
          <button type="button" class="fx-btn" @click="emit('open-tab', 'compliance')"><span class="fa fa-shield" aria-hidden="true"></span>{{ tr('Open Compliance') }}</button>
          <span class="c3-muted">{{ tr('Fix anything here in Customer 360 > Compliance (verified by a second person), then accept the step.') }}</span>
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

    <!-- popups: where the application is and every step with its decision; the audit trail of the terms -->
    <Crm360Dialog v-if="app" v-model:visible="showSteps" :title="tr('Workflow steps')" :subtitle="app.applicationRef" icon="fa-sitemap" :width="1100">
      <ol class="fc-process">
        <li v-for="(s, i) in process" :key="s.code" :class="{ 'is-done': processIndex > i, 'is-current': processIndex === i }">
          <span class="fc-process-no">{{ processIndex > i ? '✓' : i + 1 }}</span>
          <div><b>{{ tr(s.title) }}</b><small>{{ tr(s.who) }}</small></div>
        </li>
      </ol>
      <CfSupplierSteps :application-id="app.applicationId" :focus-step="null" :version="version" />
      <template #footer>
        <button type="button" class="fx-btn" @click="showSteps = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
      </template>
    </Crm360Dialog>
    <Crm360Dialog v-if="app" v-model:visible="showAudit" :title="tr('Audit trail')" :subtitle="app.applicationRef" icon="fa-history" :width="1000">
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead><tr><th>{{ tr('When') }}</th><th>{{ tr('Field') }}</th><th>{{ tr('Old') }} → {{ tr('New') }}</th><th>{{ tr('Why') }}</th><th>{{ tr('Asked by') }}</th><th>{{ tr('Approved by') }}</th></tr></thead>
            <tbody>
              <tr v-for="(c, i) in app.termChanges" :key="i">
                <td>{{ formatDateTime(c.at) }}</td><td>{{ tr(c.fieldName) }}</td>
                <td>{{ c.oldValue ?? '—' }} → <b>{{ c.newValue ?? '—' }}</b></td><td>{{ c.reason }}</td><td>{{ c.by }}</td>
                <td>{{ c.approvedBy ?? tr('— (applied before approval was required)') }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      <div v-if="!app.termChanges.length" class="fx-empty"><span class="fa fa-history" aria-hidden="true"></span>{{ tr('No changes yet.') }}</div>
      <template #footer>
        <button type="button" class="fx-btn" @click="showAudit = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
      </template>
    </Crm360Dialog>

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
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { workflowService } from '@/modules/wfe/services/workflowService'
import type { WorkflowInboxTask } from '@/modules/wfe/models/workflow'
import { errorText, fieldErrors } from '@/modules/scr/api/scr.api'
import CfSupplierDocuments from './CfSupplierDocuments.vue'
import CfSupplierSteps from './CfSupplierSteps.vue'
import Crm360Dialog from './Crm360Dialog.vue'
import { CF_CHIP, CF_STATUS, cfSuppliersApi, type CfOptions, type CfSupplier, type CfSupplierRequest } from '../api/cfSuppliers.api'

const props = defineProps<{ clientId: number; inboxId: number | null }>()
const emit = defineEmits<{ changed: []; 'open-tab': [tab: string] }>()
const router = useRouter()
const canEdit = hasPermission('CRM.CF_SUPPLIER.EDIT')
const amending = ref(false)
const showSteps = ref(false)
const showAudit = ref(false)
const amendReason = ref('')
const canAmend = computed(() => hasPermission('CRM.CF_SUPPLIER.AMEND') && app.value?.statusCode === 'ACTIVE')
// the terms can be typed in: data entry, or an active supplier's terms being changed
const termsOpen = computed(() => editable.value || amending.value)

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
const siteVisitDate = ref<string | null>(null)

const blank = (): CfSupplierRequest => ({
  goodsCategoryCodes: [], minDownPaymentPct: 10, downPaymentCollectorCode: 'SUPPLIER', commissionPct: 0, paymentDays: 7, quotationValidityDays: 7,
  deliveryDays: 5, validFromDate: null, validToDate: null, clientBankAccountId: null, notes: null
})
const form = reactive<CfSupplierRequest>(blank())
const screenedAll = computed(() => !!app.value?.parties.length && app.value.parties.every((p) => !!p.screeningResultCode && p.screeningResultCode !== 'MATCH'))
const SCREEN_NAME: Record<string, string> = { CLEAR: 'Clear', PEP: 'PEP', MATCH: 'Sanctions match' }
const SCREEN_CHIP: Record<string, string> = { CLEAR: 'fx-chip--ok', PEP: 'fx-chip--warn', MATCH: 'fx-chip--bad' }

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
  if ((app.value?.riskRatingCode ?? app.value?.customerRiskGrade) === 'HIGH') list.push({ code: 'CFS_COMMITTEE_APPROVAL', title: 'Committee approval', who: 'Committee' })
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
  siteVisitDate.value = a.siteVisitDate
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

const cancelAmend = () => {
  amending.value = false
  amendReason.value = ''
  if (app.value) fill(app.value)
}

const saveAmend = () => run(async () => {
  if (!amendReason.value.trim()) {
    showError(tr('Write why the terms are changed.'))
    return
  }
  fill(await cfSuppliersApi.amend(app.value!.applicationId, { ...form }, amendReason.value.trim()))
  amending.value = false
  amendReason.value = ''
  showSuccess(tr('Change sent for approval. Another admin must approve it.'))
})

const approveChange = () => run(async () => {
  fill(await cfSuppliersApi.approveChange(app.value!.pendingChange!.changeRequestId, null))
  showSuccess(tr('Change approved: the new terms apply to new finances.'))
})

const rejectChange = () => run(async () => {
  const notes = window.prompt(tr('Why is the change rejected?'))?.trim()
  if (!notes) return
  fill(await cfSuppliersApi.rejectChange(app.value!.pendingChange!.changeRequestId, notes))
  showSuccess(tr('Change rejected.'))
})

const takeBack = () => run(async () => {
  fill(await cfSuppliersApi.takeBackChange(app.value!.pendingChange!.changeRequestId))
  showSuccess(tr('Change taken back.'))
})

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

const submit = () => run(async () => {
  await cfSuppliersApi.update(app.value!.applicationId, { ...form })
  fill(await cfSuppliersApi.submit(app.value!.applicationId))
  showSuccess(tr('Submitted for approval.'))
  emit('changed')
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
.cf-from360 { display: flex; align-items: center; flex-wrap: wrap; gap: 6px; margin: 0 16px 10px; font-size: 12.5px; color: var(--fin-muted); }
.cf-linkbtn { padding: 0; border: 0; background: none; color: var(--fin-teal-700); font: 600 12.5px/1 inherit; cursor: pointer; }
.cf-checks { margin: 4px 16px 0; padding: 0; list-style: none; display: flex; flex-direction: column; gap: 6px; font-size: 13.5px; }
.cf-checks li { display: flex; align-items: baseline; flex-wrap: wrap; gap: 6px; }
.cf-checks small { color: var(--fin-muted); }
.cf-checks .is-ok .fa { color: #2e9e6a; }
.cf-checks .is-bad .fa { color: #c0392b; }
.cf-share { max-width: 110px; }
.cf-facts { display: flex; flex-wrap: wrap; gap: 6px 20px; margin: 0 16px 4px; font-size: 13px; color: var(--fin-muted); }
.cf-facts b { color: var(--fin-ink); }
.cf-step-row { display: flex; align-items: flex-end; gap: 12px; padding: 12px 16px 16px; }
.cf-step-row .fc-field { width: 260px; }
.cf-hint { margin: 0 16px 14px; font-size: 12px; }
.num { text-align: end; }
.cf-amend-btn { margin-inline-start: auto; }
.cf-amend { display: flex; flex-direction: column; gap: 8px; margin: 4px 16px 16px; padding: 12px; border: 1px dashed var(--fin-line); border-radius: 10px; }
.cf-amend-actions { display: flex; justify-content: flex-end; gap: 8px; }
.cf-changes { margin: 0 16px 16px; }
.cf-changes h3 { margin: 6px 0; font-size: 13px; color: var(--fin-muted); }
.cf-head-btns { display: flex; flex-wrap: wrap; gap: 8px; align-items: center; }
.cf-cnt { padding: 0 7px; border-radius: 999px; background: #eef1f3; font-size: 11.5px; }
.cf-ban { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; margin: 0 16px 12px; padding: 10px 14px; border: 1px solid #f1d58a;
  border-radius: 10px; background: #fff8e6; }
.cf-ban-txt { flex: 1; min-width: 0; font-size: 13px; }
</style>
