<template>
  <!--
    Member companies (federation). A company is created in one step with its licence (administrator + X users, period, quota) and
    its administrator, who receives an activation e-mail; the administrator then adds the company's users within the licence.
  -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Member companies') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-building-o"></span></span>
          <div>
            <h1>{{ tr('Member companies') }}</h1>
            <p>{{ tr('Each company with its licence, its administrator and its users.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openScreeningManual(7)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" @click="startNew"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New company') }}</button>
      </div>
    </div>

    <div class="fx-card fx-card-pad fc-filters">
      <label class="fc-field"><span>{{ tr('Search') }}</span>
        <input v-model="search" class="fx-search" :placeholder="tr('Search by code or name')">
      </label>
      <ScrUserFilter v-model="who" :me="me" :users="users" />
    </div>

    <div class="fx-card">
      <div class="fc-card-head"><h2>{{ tr('Member companies') }} ({{ rows.length }})</h2></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Code') }}</th><th>{{ tr('Name') }}</th><th>{{ tr('Administrator') }}</th><th>{{ tr('Licence') }}</th>
              <th>{{ tr('Price plan') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Inquiries') }}</th><th class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in rows" :key="c.memberCompanyId">
              <td dir="ltr"><b class="fc-ref">{{ c.companyCode }}</b></td>
              <td><b>{{ c.nameAr }}</b><span class="fx-cell-sub">{{ c.nameEn }}</span></td>
              <td>
                <template v-if="c.seats?.adminUserName">
                  <span dir="ltr">{{ c.seats.adminUserName }}</span>
                  <span class="fx-cell-sub"><span class="fx-chip" :class="STATUS_CHIP[c.seats.adminStatusCode ?? '']">{{ tr(LABELS[c.seats.adminStatusCode ?? ''] ?? c.seats.adminStatusCode ?? '') }}</span></span>
                </template>
                <span v-else class="fx-chip fx-chip--bad"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>{{ tr('No administrator') }}</span>
              </td>
              <td>
                <template v-if="c.licence">
                  <div class="scr-seats" :title="tr('Users besides the administrator')">
                    <b>{{ tr('Administrator') }} + {{ c.licence.maxUsers }}</b>
                    <span class="scr-seat-bar"><i :style="{ width: seatPct(c) }"></i></span>
                    <small>{{ c.seats?.usersInUse ?? 0 }} / {{ c.licence.maxUsers }} {{ tr('in use') }}</small>
                  </div>
                  <span class="fx-cell-sub"><bdi dir="ltr">{{ formatDate(c.licence.validFrom) }}</bdi> – <bdi>{{ c.licence.validTo ? formatDate(c.licence.validTo) : tr('open-ended') }}</bdi></span>
                  <span v-if="c.licenceStateCode && c.licenceStateCode !== 'ACTIVE'" class="fx-chip fx-chip--bad">{{ tr(LABELS[c.licenceStateCode] ?? c.licenceStateCode) }}</span>
                </template>
                <span v-else class="fx-chip fx-chip--bad">{{ tr('No licence') }}</span>
              </td>
              <td>
                <template v-if="c.pricePlanCode"><b dir="ltr">{{ c.pricePlanCode }}</b>
                  <span class="fx-cell-sub">{{ tr(LABELS[c.billingCycleCode ?? 'MONTHLY'] ?? '') }}<template v-if="planOf(c.pricePlanId)"> · <bdi dir="ltr">{{ egp(planOf(c.pricePlanId)!.userMonthlyFee) }}</bdi> / {{ tr('user') }}</template></span></template>
                <span v-else class="fx-chip fx-chip--warn">{{ tr('Not billed yet') }}</span>
              </td>
              <td><span class="fx-chip" :class="c.statusCode === 'ACTIVE' ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(LABELS[c.statusCode] ?? c.statusCode) }}</span></td>
              <td>{{ c.inquiryCount }}<span v-if="c.licence?.yearlyInquiryQuota" class="fx-cell-sub">{{ tr('Quota') }} {{ c.licence.yearlyInquiryQuota.toLocaleString() }} / {{ tr('year') }}</span></td>
              <td class="fx-r">
                <div class="fc-row-actions">
                  <button type="button" class="fx-btn fx-btn--sm" @click="edit(c)"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}</button>
                  <button type="button" class="fx-btn fx-btn--sm" @click="openLicence(c)"><span class="fa fa-id-card-o" aria-hidden="true"></span>{{ tr('Licence') }}</button>
                  <button type="button" class="fx-btn fx-btn--sm" @click="apiOf = c"><span class="fa fa-plug" aria-hidden="true"></span>{{ tr('API') }}</button>
                  <button type="button" class="fx-btn fx-btn--teal fx-btn--sm" @click="selected = c"><span class="fa fa-users" aria-hidden="true"></span>{{ tr('Users') }}</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="!rows.length" class="fx-empty"><span class="fa fa-building-o" aria-hidden="true"></span>{{ companies.length ? tr('Nothing here for the chosen user. Choose All users to see everything.') : tr('No member companies yet.') }}</div>
    </div>

    <ScrCompanyUsers v-if="selected" :key="selected.memberCompanyId" :company-id="selected.memberCompanyId" :company-name="`${selected.companyCode} · ${selected.nameAr}`"
                     :max-users="selected.licence?.maxUsers ?? 0" @changed="load" />

    <ScrApiClients v-if="apiOf" :company-id="apiOf.memberCompanyId" :company-name="`${apiOf.companyCode} · ${apiOf.nameAr}`" @close="apiOf = null" />

    <!-- new company: company + licence + administrator in one step -->
    <ScrDialog v-if="creating" :title="tr('New company')" :subtitle="tr('The company, its licence and its administrator, in one step. The administrator receives an activation e-mail.')" icon="fa-building-o" @close="creating = false">
      <h3 class="scr-form-sec"><span class="fa fa-building-o" aria-hidden="true"></span>{{ tr('Company') }}</h3>
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Code') }} <b>*</b></span><input v-model="company.companyCode" class="fx-search" dir="ltr">
          <small v-if="err('companyCode')" class="scr-err">{{ err('companyCode') }}</small></label>
        <label class="fc-field"><span>{{ tr('Name (Arabic)') }} <b>*</b></span><input v-model="company.nameAr" class="fx-search" dir="rtl">
          <small v-if="err('nameAr')" class="scr-err">{{ err('nameAr') }}</small></label>
        <label class="fc-field"><span>{{ tr('Name (English)') }} <b>*</b></span><input v-model="company.nameEn" class="fx-search" dir="ltr">
          <small v-if="err('nameEn')" class="scr-err">{{ err('nameEn') }}</small></label>
        <label class="fc-field"><span>{{ tr('Licence number') }}</span><input v-model="company.licenceNo" class="fx-search" dir="ltr"></label>
        <label class="fc-field"><span>{{ tr('Company e-mail') }}</span><input v-model="company.contactEmail" class="fx-search" type="email" dir="ltr"></label>
      </div>

      <h3 class="scr-form-sec"><span class="fa fa-id-card-o" aria-hidden="true"></span>{{ tr('Licence') }}</h3>
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Users besides the administrator') }} <b>*</b></span>
          <input v-model.number="licence.maxUsers" class="fx-search" type="number" min="0" dir="ltr">
          <small class="scr-hint">{{ tr('Administrator') }} + {{ licence.maxUsers ?? 0 }} = {{ (licence.maxUsers ?? 0) + 1 }} {{ tr('accounts') }}</small>
          <small v-if="err('maxUsers')" class="scr-err">{{ err('maxUsers') }}</small></label>
        <label class="fc-field"><span>{{ tr('Valid from') }} <b>*</b></span><input v-model="licence.validFrom" class="fx-search" type="date">
          <small v-if="err('validFrom')" class="scr-err">{{ err('validFrom') }}</small></label>
        <label class="fc-field"><span>{{ tr('Valid to') }}</span><input v-model="licence.validTo" class="fx-search" type="date">
          <small class="scr-hint">{{ tr('Empty = open-ended') }}</small>
          <small v-if="err('validTo')" class="scr-err">{{ err('validTo') }}</small></label>
        <label class="fc-field"><span>{{ tr('Yearly inquiry quota') }}</span><input v-model.number="licence.yearlyInquiryQuota" class="fx-search" type="number" min="1" dir="ltr">
          <small class="scr-hint">{{ tr('Empty = no limit') }}</small>
          <small v-if="err('yearlyInquiryQuota')" class="scr-err">{{ err('yearlyInquiryQuota') }}</small></label>
      </div>

      <h3 class="scr-form-sec"><span class="fa fa-money" aria-hidden="true"></span>{{ tr('Billing') }}</h3>
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Price plan') }}</span>
          <select v-model="company.pricePlanId" class="fx-select"><option :value="null">{{ tr('Not billed yet') }}</option>
            <option v-for="p in plans.filter((x) => x.isActive || x.pricePlanId === company.pricePlanId)" :key="p.pricePlanId" :value="p.pricePlanId">{{ p.planCode }} · {{ egp(p.userMonthlyFee) }} / {{ tr('user') }} / {{ tr('month') }} · {{ egp(p.inquiryFee) }} / {{ tr('inquiry') }}</option></select>
          <small v-if="canPlans && !plans.length" class="scr-hint"><RouterLink :to="{ name: 'scr-price-plans' }">{{ tr('Add a price plan first') }}</RouterLink></small>
          <small v-if="err('pricePlanId')" class="scr-err">{{ err('pricePlanId') }}</small></label>
        <label class="fc-field"><span>{{ tr('Invoicing') }}</span>
          <select v-model="company.billingCycleCode" class="fx-select">
            <option value="MONTHLY">{{ tr('Monthly') }}</option><option value="QUARTERLY">{{ tr('Quarterly') }}</option><option value="YEARLY">{{ tr('Yearly') }}</option></select>
          <small class="scr-hint">{{ tr('Calendar months, quarters or years.') }}</small></label>
      </div>

      <h3 class="scr-form-sec"><span class="fa fa-user-circle-o" aria-hidden="true"></span>{{ tr('Company administrator') }}</h3>
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Full name') }} <b>*</b></span><input v-model="admin.fullName" class="fx-search">
          <small v-if="err('admin.fullName')" class="scr-err">{{ err('admin.fullName') }}</small></label>
        <label class="fc-field"><span>{{ tr('E-mail') }} <b>*</b></span><input v-model="admin.email" class="fx-search" type="email" dir="ltr">
          <small v-if="err('admin.email')" class="scr-err">{{ err('admin.email') }}</small></label>
        <label class="fc-field"><span>{{ tr('User name') }}</span><input v-model="admin.userName" class="fx-search" dir="ltr" :placeholder="admin.email || ''">
          <small class="scr-hint">{{ tr('Empty = the e-mail address') }}</small>
          <small v-if="err('admin.userName')" class="scr-err">{{ err('admin.userName') }}</small></label>
      </div>
      <div class="fc-note"><span class="fa fa-envelope-o" aria-hidden="true"></span>
        <span>{{ tr('No password is set here. The administrator receives an e-mail with a link to choose their own password, then adds the company\'s users.') }}</span></div>
      <template #actions>
        <button type="button" class="fx-btn" @click="creating = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="create"><span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-check']" aria-hidden="true"></span>{{ tr('Create company and send the invitation') }}</button>
      </template>
    </ScrDialog>

    <!-- what happened to the administrator's invitation -->
    <ScrDialog v-if="created" :title="tr('Company created')" :subtitle="`${created.company.companyCode} · ${created.admin.userName}`" icon="fa-check" @close="created = null">
      <div v-if="created.email.sent" class="fc-note"><span class="fa fa-envelope-o" aria-hidden="true"></span>
        <span>{{ tr('The activation e-mail was sent to') }} <b dir="ltr">{{ created.admin.email }}</b>. {{ tr('The link works once and expires in 48 hours.') }}</span></div>
      <template v-else>
        <div class="fc-note fc-note--warn"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>
          <span>{{ tr('The activation e-mail could not be sent') }} ({{ tr(LABELS[created.email.statusCode] ?? created.email.statusCode) }}). {{ tr('Copy the link and send it to the administrator another way.') }}</span></div>
        <ScrLinkBox :company-id="created.company.memberCompanyId" :user-id="created.admin.userId" />
      </template>
      <template #actions>
        <button type="button" class="fx-btn fx-btn--primary" @click="created = null">{{ tr('Done') }}</button>
      </template>
    </ScrDialog>

    <!-- company details -->
    <ScrDialog v-if="editing" :title="tr('Edit company')" :subtitle="tr('A suspended company\'s users cannot sign in or screen.')" icon="fa-building-o" @close="editing = false">
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Code') }} <b>*</b></span><input v-model="company.companyCode" class="fx-search" dir="ltr">
          <small v-if="err('companyCode')" class="scr-err">{{ err('companyCode') }}</small></label>
        <label class="fc-field"><span>{{ tr('Name (Arabic)') }} <b>*</b></span><input v-model="company.nameAr" class="fx-search" dir="rtl">
          <small v-if="err('nameAr')" class="scr-err">{{ err('nameAr') }}</small></label>
        <label class="fc-field"><span>{{ tr('Name (English)') }} <b>*</b></span><input v-model="company.nameEn" class="fx-search" dir="ltr">
          <small v-if="err('nameEn')" class="scr-err">{{ err('nameEn') }}</small></label>
        <label class="fc-field"><span>{{ tr('Licence number') }}</span><input v-model="company.licenceNo" class="fx-search" dir="ltr"></label>
        <label class="fc-field"><span>{{ tr('Company e-mail') }}</span><input v-model="company.contactEmail" class="fx-search" type="email" dir="ltr"></label>
        <label class="fc-field"><span>{{ tr('Status') }}</span>
          <select v-model="company.statusCode" class="fx-select"><option value="ACTIVE">{{ tr('Active') }}</option><option value="SUSPENDED">{{ tr('Suspended') }}</option></select>
        </label>
        <label class="fc-field fc-full"><span>{{ tr('Notes') }}</span><textarea v-model="company.notes" class="fx-search fc-textarea"></textarea></label>
        <!-- the number of users and the dates belong to the licence: every change is a new licence, kept for billing -->
        <div v-if="editOf" class="fc-note fc-full scr-edit-licence">
          <span class="fa fa-id-card-o" aria-hidden="true"></span>
          <span><b>{{ tr('Licence') }}:</b>&nbsp;
            <template v-if="editOf.licence"> {{ tr('Administrator') }} + {{ editOf.licence.maxUsers }} · <bdi dir="ltr">{{ formatDate(editOf.licence.validFrom) }}</bdi> – <bdi>{{ editOf.licence.validTo ? formatDate(editOf.licence.validTo) : tr('open-ended') }}</bdi></template>
            <template v-else> {{ tr('No licence') }}</template>
            <br><small>{{ tr('The number of users and the dates are changed as a new licence; earlier licences are kept.') }}</small></span>
          <button type="button" class="fx-btn fx-btn--sm fx-btn--teal" @click="licenceFromEdit"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Change users or dates') }}</button>
        </div>
      </div>
      <h3 class="scr-form-sec"><span class="fa fa-money" aria-hidden="true"></span>{{ tr('Billing') }}</h3>
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Price plan') }}</span>
          <select v-model="company.pricePlanId" class="fx-select"><option :value="null">{{ tr('Not billed yet') }}</option>
            <option v-for="p in plans.filter((x) => x.isActive || x.pricePlanId === company.pricePlanId)" :key="p.pricePlanId" :value="p.pricePlanId">{{ p.planCode }} · {{ egp(p.userMonthlyFee) }} / {{ tr('user') }} / {{ tr('month') }} · {{ egp(p.inquiryFee) }} / {{ tr('inquiry') }}</option></select>
          <small v-if="canPlans && !plans.length" class="scr-hint"><RouterLink :to="{ name: 'scr-price-plans' }">{{ tr('Add a price plan first') }}</RouterLink></small>
          <small v-if="err('pricePlanId')" class="scr-err">{{ err('pricePlanId') }}</small></label>
        <label class="fc-field"><span>{{ tr('Invoicing') }}</span>
          <select v-model="company.billingCycleCode" class="fx-select">
            <option value="MONTHLY">{{ tr('Monthly') }}</option><option value="QUARTERLY">{{ tr('Quarterly') }}</option><option value="YEARLY">{{ tr('Yearly') }}</option></select>
          <small class="scr-hint">{{ tr('Calendar months, quarters or years.') }}</small></label>
      </div>

      <template #actions>
        <button type="button" class="fx-btn" @click="editing = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="saveCompany"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Save') }}</button>
      </template>
    </ScrDialog>

    <!-- licence: the one in force, its history, and a new one -->
    <ScrDialog v-if="licenceOf" :title="tr('Licence')" :subtitle="`${licenceOf.companyCode} · ${licenceOf.nameAr}`" icon="fa-id-card-o" @close="licenceOf = null">
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Users') }}</th><th>{{ tr('Valid from') }}</th><th>{{ tr('Valid to') }}</th><th>{{ tr('Yearly inquiry quota') }}</th><th>{{ tr('Entered by') }}</th></tr></thead>
          <tbody>
            <tr v-for="(l, i) in history" :key="l.licenceId" :class="{ 'scr-current': i === 0 }">
              <td><b>{{ tr('Administrator') }} + {{ l.maxUsers }}</b><span v-if="i === 0" class="fx-chip fx-chip--ok">{{ tr('In force') }}</span></td>
              <td>{{ formatDate(l.validFrom) }}</td><td>{{ l.validTo ? formatDate(l.validTo) : tr('open-ended') }}</td>
              <td>{{ l.yearlyInquiryQuota?.toLocaleString() ?? tr('No limit') }}</td>
              <td>{{ l.createdBy }}<span class="fx-cell-sub">{{ formatDateTime(l.createdAt) }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
      <h3 class="scr-form-sec"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('New licence (renewal or change)') }}</h3>
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Users besides the administrator') }} <b>*</b></span><input v-model.number="licence.maxUsers" class="fx-search" type="number" min="0" dir="ltr">
          <small class="scr-hint">{{ tr('In use now') }}: {{ licenceOf.seats?.usersInUse ?? 0 }}</small>
          <small v-if="err('maxUsers')" class="scr-err">{{ err('maxUsers') }}</small></label>
        <label class="fc-field"><span>{{ tr('Valid from') }} <b>*</b></span><input v-model="licence.validFrom" class="fx-search" type="date">
          <small v-if="err('validFrom')" class="scr-err">{{ err('validFrom') }}</small></label>
        <label class="fc-field"><span>{{ tr('Valid to') }}</span><input v-model="licence.validTo" class="fx-search" type="date">
          <small v-if="err('validTo')" class="scr-err">{{ err('validTo') }}</small></label>
        <label class="fc-field"><span>{{ tr('Yearly inquiry quota') }}</span><input v-model.number="licence.yearlyInquiryQuota" class="fx-search" type="number" min="1" dir="ltr">
          <small v-if="err('yearlyInquiryQuota')" class="scr-err">{{ err('yearlyInquiryQuota') }}</small></label>
        <label class="fc-field fc-full"><span>{{ tr('Notes') }}</span><input v-model="licence.notes" class="fx-search"></label>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" @click="licenceOf = null">{{ tr('Close') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="saveLicence"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Save new licence') }}</button>
      </template>
    </ScrDialog>
  </div>
</template>

<script setup lang="ts">
import { openScreeningManual } from '@/modules/scr/utils/manual'
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDate, formatDateTime } from '../utils/dates'
import { useUserFilter } from '../utils/userFilter'
import ScrUserFilter from '../components/ScrUserFilter.vue'
import ScrCompanyUsers from '../components/ScrCompanyUsers.vue'
import ScrApiClients from '../components/ScrApiClients.vue'
import ScrDialog from '../components/ScrDialog.vue'
import ScrLinkBox from '../components/ScrLinkBox.vue'
import {
  LABELS, egp, errorText, fieldErrors, scrApi,
  type ScrCompany, type ScrPricePlan, type ScrCompanyAdminRequest, type ScrCompanyRequest, type ScrCompanyUser, type ScrEmailOutcome, type ScrLicence, type ScrLicenceRequest
} from '../api/scr.api'

const STATUS_CHIP: Record<string, string> = { ACTIVE: 'fx-chip--ok', PENDING_ACTIVATION: 'fx-chip--warn', INACTIVE: 'fx-chip--bad' }

const companies = ref<ScrCompany[]>([])
/** Entered by me by default (Admin: all users). */
const { who, me, users, scoped } = useUserFilter(companies, (c) => c.createdBy)
const search = ref('')
const rows = computed(() => {
  const q = search.value.trim().toLowerCase()
  return scoped.value.filter((c) => !q || `${c.companyCode} ${c.nameEn} ${c.nameAr}`.toLowerCase().includes(q))
})
const seatPct = (c: ScrCompany) => `${c.licence?.maxUsers ? Math.min(100, Math.round(((c.seats?.usersInUse ?? 0) / c.licence.maxUsers) * 100)) : 0}%`

const selected = ref<ScrCompany | null>(null)
const apiOf = ref<ScrCompany | null>(null)
const busy = ref(false)
const errors = ref<Record<string, string>>({})
const err = (k: string) => errors.value[k] ?? errors.value[k.charAt(0).toLowerCase() + k.slice(1)]

const today = () => new Date().toISOString().slice(0, 10)
const blankCompany = (): ScrCompanyRequest => ({ companyCode: '', nameEn: '', nameAr: '', licenceNo: null, contactEmail: null, statusCode: 'ACTIVE', notes: null, pricePlanId: null, billingCycleCode: 'MONTHLY' })
const blankLicence = (): ScrLicenceRequest => ({ maxUsers: 0, validFrom: today(), validTo: null, yearlyInquiryQuota: null, notes: null })
const company = reactive<ScrCompanyRequest>(blankCompany())
const licence = reactive<ScrLicenceRequest>(blankLicence())
const admin = reactive<ScrCompanyAdminRequest>({ userName: null, email: null, fullName: null })

const creating = ref(false)
const created = ref<{ company: ScrCompany; admin: ScrCompanyUser; email: ScrEmailOutcome } | null>(null)
const editing = ref(false)
/** The company being edited, for its licence summary. */
const editOf = ref<ScrCompany | null>(null)
const editId = ref<number | null>(null)
const licenceOf = ref<ScrCompany | null>(null)
const history = ref<ScrLicence[]>([])

/** Price plans for the Billing section (needs a billing permission; without it the company keeps its plan as it is). */
const canPlans = hasPermission('SCR.BILLING.VIEW')
const plans = ref<ScrPricePlan[]>([])
const planOf = (id: number | null | undefined) => plans.value.find((p) => p.pricePlanId === id)

async function load() {
  try {
    if (canPlans && !plans.value.length) plans.value = await scrApi.pricePlans()
    companies.value = await scrApi.companies()
    if (selected.value) selected.value = companies.value.find((c) => c.memberCompanyId === selected.value!.memberCompanyId) ?? null
  } catch (err) {
    showError(errorText(err))
  }
}

const clean = (v: string | null) => (v && v.trim() ? v.trim() : null)
const numberOrNull = (v: unknown) => (typeof v === 'number' && !Number.isNaN(v) ? v : null)

function startNew() {
  errors.value = {}
  Object.assign(company, blankCompany())
  Object.assign(licence, blankLicence())
  Object.assign(admin, { userName: null, email: null, fullName: null })
  creating.value = true
}

async function create() {
  busy.value = true
  errors.value = {}
  try {
    const r = await scrApi.onboardCompany({ ...company },
      { ...licence, maxUsers: numberOrNull(licence.maxUsers), yearlyInquiryQuota: numberOrNull(licence.yearlyInquiryQuota), validTo: clean(licence.validTo) },
      { userName: clean(admin.userName), email: clean(admin.email), fullName: clean(admin.fullName) })
    creating.value = false
    created.value = r
    await load()
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) showError(errorText(err))
  } finally {
    busy.value = false
  }
}

function licenceFromEdit() {
  const c = editOf.value
  editing.value = false
  if (c) void openLicence(c)
}

function edit(c: ScrCompany) {
  editOf.value = c
  errors.value = {}
  editId.value = c.memberCompanyId
  Object.assign(company, { companyCode: c.companyCode, nameEn: c.nameEn, nameAr: c.nameAr, licenceNo: c.licenceNo, contactEmail: c.contactEmail, statusCode: c.statusCode, notes: c.notes, pricePlanId: c.pricePlanId ?? null, billingCycleCode: c.billingCycleCode ?? 'MONTHLY' })
  editing.value = true
}

async function saveCompany() {
  busy.value = true
  errors.value = {}
  try {
    await scrApi.updateCompany(editId.value!, { ...company })
    editing.value = false
    showSuccess(tr('Company saved.'))
    await load()
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function openLicence(c: ScrCompany) {
  errors.value = {}
  licenceOf.value = c
  const l = c.licence
  Object.assign(licence, l
    ? { maxUsers: l.maxUsers, validFrom: today(), validTo: l.validTo, yearlyInquiryQuota: l.yearlyInquiryQuota, notes: null }
    : blankLicence())
  try {
    history.value = await scrApi.licences(c.memberCompanyId)
  } catch (err) {
    showError(errorText(err))
  }
}

async function saveLicence() {
  busy.value = true
  errors.value = {}
  try {
    await scrApi.addLicence(licenceOf.value!.memberCompanyId,
      { ...licence, maxUsers: numberOrNull(licence.maxUsers), yearlyInquiryQuota: numberOrNull(licence.yearlyInquiryQuota), validTo: clean(licence.validTo), notes: clean(licence.notes) })
    showSuccess(tr('The new licence is in force.'))
    licenceOf.value = null
    await load()
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) showError(errorText(err))
  } finally {
    busy.value = false
  }
}

onMounted(load)
</script>
