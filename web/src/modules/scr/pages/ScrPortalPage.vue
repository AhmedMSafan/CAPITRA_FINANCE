<template>
  <!-- Screening Portal (member companies): screen a person or company, see the result, and the company's earlier inquiries. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-shield"></span></span>
        <div>
          <h1>{{ tr('Screening Portal') }}</h1>
          <p>{{ tr('Screen a person or company against the Federation Final List. Every inquiry is recorded.') }}</p>
        </div>
      </div>
      <div v-if="company" class="fx-card fc-summary-customer">
        <span class="fc-quick-ic" aria-hidden="true"><span class="fa fa-building-o"></span></span>
        <div><b>{{ company.nameAr }}</b><small>{{ company.nameEn }} · {{ company.companyCode }}</small>
          <small v-if="company.quota" class="scr-quota" :class="{ 'scr-quota--low': company.quota.left !== null && company.quota.left <= Math.max(1, (company.quota.quota ?? 0) / 10) }">
            {{ tr('Inquiries this licence year') }}: <bdi dir="ltr">{{ company.quota.used.toLocaleString() }}<template v-if="company.quota.quota !== null"> / {{ company.quota.quota.toLocaleString() }}</template></bdi>
            <template v-if="company.quota.left !== null"> · {{ company.quota.left.toLocaleString() }} {{ tr('left') }}</template></small></div>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openScreeningManual(8)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
      </div>
    </div>

    <div class="fx-card">
      <div class="fc-tabs" role="tablist">
        <button type="button" class="fc-tab" role="tab" :aria-selected="tab === 'new'" @click="tab = 'new'"><span class="fa fa-search" aria-hidden="true"></span>{{ tr('New inquiry') }}</button>
        <button type="button" class="fc-tab" role="tab" :aria-selected="tab === 'mine'" @click="openHistory"><span class="fa fa-history" aria-hidden="true"></span>{{ tr('My inquiries') }}<em v-if="history.length">{{ history.length }}</em></button>
        <button v-if="canUsers && company" type="button" class="fc-tab" role="tab" :aria-selected="tab === 'users'" @click="tab = 'users'"><span class="fa fa-users" aria-hidden="true"></span>{{ tr('Company users') }}</button>
        <button v-if="canUsers && company" type="button" class="fc-tab" role="tab" :aria-selected="tab === 'invoices'" @click="tab = 'invoices'"><span class="fa fa-file-text-o" aria-hidden="true"></span>{{ tr('Invoices') }}</button>
        <button v-if="canUsers && company" type="button" class="fc-tab" role="tab" :aria-selected="tab === 'signins'" @click="tab = 'signins'"><span class="fa fa-sign-in" aria-hidden="true"></span>{{ tr('Sign-ins') }}</button>
      </div>

      <template v-if="tab === 'new'">
        <div class="fc-grid fc-grid--3">
          <label class="fc-field"><span>{{ tr('Type') }} <b>*</b></span>
            <select v-model="form.subjectTypeCode" class="fx-select" @change="typeChanged"><option value="PERSON">{{ tr('Person') }}</option><option value="COMPANY">{{ tr('Company') }}</option></select>
          </label>
          <label class="fc-field fc-span2"><span>{{ tr('Full name') }} <b>*</b></span><input v-model="form.subjectName" class="fx-search" :placeholder="tr('As on the identity document, Arabic or English')">
            <small v-if="errors.subjectName" class="scr-err">{{ errors.subjectName }}</small></label>
          <label class="fc-field"><span>{{ tr('Identity type') }} <b>*</b></span>
            <select v-model="form.identityTypeCode" class="fx-select"><option v-for="t in idTypes" :key="t" :value="t">{{ tr(LABELS[t] ?? t) }}</option></select>
            <small v-if="errors.identityTypeCode" class="scr-err">{{ errors.identityTypeCode }}</small></label>
          <label class="fc-field"><span>{{ tr('Identity number') }} <b>*</b></span><input v-model="form.identityNo" class="fx-search" dir="ltr">
            <small v-if="errors.identityNo" class="scr-err">{{ errors.identityNo }}</small></label>
          <label class="fc-field"><span>{{ tr('Nationality') }}</span><input v-model="form.nationalityCode" class="fx-search" dir="ltr" maxlength="2" placeholder="EG">
            <small v-if="errors.nationalityCode" class="scr-err">{{ errors.nationalityCode }}</small></label>
          <label v-if="form.subjectTypeCode === 'PERSON'" class="fc-field"><span>{{ tr('Birth date') }}</span><input v-model="form.birthDate" class="fx-search" type="date"></label>
          <label class="fc-field"><span>{{ tr('Purpose') }} <b>*</b></span>
            <select v-model="form.purposeCode" class="fx-select"><option v-for="p in ['ONBOARDING', 'TRANSACTION', 'PERIODIC_REVIEW', 'OTHER']" :key="p" :value="p">{{ tr(LABELS[p] ?? p) }}</option></select>
          </label>
          <label v-if="form.purposeCode === 'OTHER'" class="fc-field"><span>{{ tr('Describe the purpose') }} <b>*</b></span><input v-model="form.purposeText" class="fx-search">
            <small v-if="errors.purposeText" class="scr-err">{{ errors.purposeText }}</small></label>
          <label class="fc-field"><span>{{ tr('Case reference') }}</span><input v-model="form.caseReference" class="fx-search" dir="ltr"></label>
        </div>
        <div class="fc-form-actions">
          <div v-if="!result" class="fc-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
            <span><b>{{ tr('How screening works:') }}</b> {{ tr('we look for the identity number exactly, and for similar names in Arabic and English, other names included.') }}</span></div>
          <span v-else class="scr-grow"></span>
          <button type="button" class="fx-btn" @click="clear"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Clear') }}</button>
          <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="screen">
            <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-search']" aria-hidden="true"></span>{{ tr('Screen') }}
          </button>
        </div>
      </template>

      <template v-else-if="tab === 'mine'">
        <div class="fc-filters scr-portal-filter">
          <ScrUserFilter v-model="who" :me="me" :users="users" />
        </div>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead>
              <tr>
                <th>{{ tr('Inquiry') }}</th><th>{{ tr('Name') }}</th><th>{{ tr('Identity number') }}</th><th>{{ tr('Purpose') }}</th>
                <th>{{ tr('Result') }}</th><th>{{ tr('Date') }}</th><th>{{ tr('By') }}</th><th class="fx-r">{{ tr('Actions') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="i in scoped" :key="i.inquiryId" :class="{ 'is-overdue': i.resultCode === 'MATCH' }">
                <td dir="ltr"><b class="fc-ref">{{ i.inquiryRef }}</b></td>
                <td>{{ i.subjectName }}<span class="fx-cell-sub">{{ tr(LABELS[i.subjectTypeCode] ?? i.subjectTypeCode) }}</span></td>
                <td dir="ltr">{{ i.identityNo }}</td>
                <td>{{ tr(LABELS[i.purposeCode] ?? i.purposeCode) }}</td>
                <td><span class="fx-chip" :class="CHIP[i.resultCode]">{{ tr(LABELS[i.resultCode] ?? i.resultCode) }}</span>
                  <span v-if="i.hits.length" class="scr-basis">
                    <span :class="i.hits[0].matchBasisCode === 'NAME_ONLY' ? 'scr-basis--no' : 'scr-basis--yes'"><span :class="['fa', i.hits[0].matchBasisCode === 'NAME_ONLY' ? 'fa-times' : 'fa-check']" aria-hidden="true"></span>{{ tr('ID') }}</span>
                    <span :class="i.hits[0].matchBasisCode === 'ID_ONLY' ? 'scr-basis--no' : 'scr-basis--yes'"><span :class="['fa', i.hits[0].matchBasisCode === 'ID_ONLY' ? 'fa-times' : 'fa-check']" aria-hidden="true"></span>{{ tr('Name') }}</span>
                  </span></td>
                <td>{{ formatDateTime(i.screenedAt) }}</td>
                <td>{{ i.requestedBy }}</td>
                <td class="fx-r"><button type="button" class="fx-btn fx-btn--sm" @click="result = i; tab = 'new'"><span class="fa fa-eye" aria-hidden="true"></span>{{ tr('View') }}</button></td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="!scoped.length" class="fx-empty"><span class="fa fa-history" aria-hidden="true"></span>{{ history.length ? tr('Nothing here for the chosen user. Choose All users to see everything.') : tr('No inquiries yet.') }}</div>
      </template>
    </div>

    <ScrInquiryResult v-if="tab === 'new' && result" :inquiry="result" />
    <ScrPortalInvoices v-if="tab === 'invoices' && company" />
    <ScrPortalSignIns v-if="tab === 'signins' && company" />
    <ScrCompanyUsers v-if="tab === 'users' && company" :company-id="company.memberCompanyId" :company-name="company.nameAr" :max-users="company.maxUsers ?? undefined" />
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { formatDateTime } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import { useUserFilter } from '../utils/userFilter'
import ScrUserFilter from '../components/ScrUserFilter.vue'
import ScrCompanyUsers from '../components/ScrCompanyUsers.vue'
import ScrInquiryResult from '../components/ScrInquiryResult.vue'
import ScrPortalInvoices from '../components/ScrPortalInvoices.vue'
import ScrPortalSignIns from '../components/ScrPortalSignIns.vue'
import { useRoute } from 'vue-router'
import { CHIP, LABELS, errorText, fieldErrors, scrApi, type ScrInquiry, type ScrInquiryRequest, type ScrMyCompany } from '../api/scr.api'
import { openScreeningManual } from '@/modules/scr/utils/manual'

const canUsers = hasPermission('SCR.COMPANY.USERS')
const route = useRoute()
const tab = ref<'new' | 'mine' | 'users' | 'invoices' | 'signins'>(route.query.tab === 'invoices' && canUsers ? 'invoices' : 'new')
const company = ref<ScrMyCompany | null>(null)
const result = ref<ScrInquiry | null>(null)
const history = ref<ScrInquiry[]>([])
/** My own inquiries by default; the company's other users can be chosen (Admin: all). */
const { who, me, users, scoped } = useUserFilter(history, (i) => i.requestedBy)
const busy = ref(false)
const errors = ref<Record<string, string>>({})
const blank = (): ScrInquiryRequest => ({
  subjectTypeCode: 'PERSON', subjectName: '', identityTypeCode: 'NATIONAL_ID', identityNo: '', birthDate: null, nationalityCode: 'EG',
  purposeCode: 'ONBOARDING', purposeText: null, caseReference: null
})
const form = reactive<ScrInquiryRequest>(blank())
const idTypes = computed(() => (form.subjectTypeCode === 'PERSON' ? ['NATIONAL_ID', 'PASSPORT', 'OTHER'] : ['COMMERCIAL_REGISTER', 'OTHER']))

function typeChanged() {
  form.identityTypeCode = form.subjectTypeCode === 'PERSON' ? 'NATIONAL_ID' : 'COMMERCIAL_REGISTER'
  if (form.subjectTypeCode === 'COMPANY') form.birthDate = null
}

function clear() {
  Object.assign(form, blank())
  errors.value = {}
  result.value = null
}

async function screen() {
  busy.value = true
  errors.value = {}
  try {
    result.value = await scrApi.screen({ ...form })
    history.value = [result.value, ...history.value.filter((i) => i.inquiryId !== result.value?.inquiryId)]
    scrApi.myCompany().then((c) => (company.value = c)).catch(() => undefined)
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function loadHistory() {
  try {
    history.value = await scrApi.myInquiries()
  } catch (err) {
    showError(errorText(err))
  }
}

async function openHistory() {
  tab.value = 'mine'
  await loadHistory()
}

onMounted(async () => {
  try {
    company.value = await scrApi.myCompany()
    await loadHistory()
  } catch (err) {
    showError(errorText(err))
  }
})
</script>
