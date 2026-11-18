<template>
  <!-- Screening Portal (member companies): screen a person or company, see the result, and the company's earlier inquiries. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <h1>{{ tr('Screening Portal') }}</h1>
        <small v-if="company" class="scr-muted">{{ company.nameAr }} · {{ company.nameEn }}</small>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" :class="{ 'fx-btn--teal': tab === 'new' }" @click="tab = 'new'">{{ tr('New inquiry') }}</button>
        <button type="button" class="fx-btn" :class="{ 'fx-btn--teal': tab === 'mine' }" @click="openHistory">{{ tr('My inquiries') }}</button>
        <button v-if="canUsers && company" type="button" class="fx-btn" :class="{ 'fx-btn--teal': tab === 'users' }" @click="tab = 'users'">{{ tr('Company users') }}</button>
      </div>
    </div>

    <template v-if="tab === 'new'">
      <section class="fx-card">
        <div class="scr-grid scr-grid--3">
          <label class="scr-field"><span>{{ tr('Type') }} *</span>
            <select v-model="form.subjectTypeCode" @change="typeChanged"><option value="PERSON">{{ tr('Person') }}</option><option value="COMPANY">{{ tr('Company') }}</option></select>
          </label>
          <label class="scr-field"><span>{{ tr('Full name') }} *</span><input v-model="form.subjectName"><small v-if="errors.subjectName" class="err">{{ errors.subjectName }}</small></label>
          <label class="scr-field"><span>{{ tr('Identity type') }} *</span>
            <select v-model="form.identityTypeCode"><option v-for="t in idTypes" :key="t" :value="t">{{ tr(LABELS[t] ?? t) }}</option></select>
            <small v-if="errors.identityTypeCode" class="err">{{ errors.identityTypeCode }}</small>
          </label>
          <label class="scr-field"><span>{{ tr('Identity number') }} *</span><input v-model="form.identityNo" dir="ltr"><small v-if="errors.identityNo" class="err">{{ errors.identityNo }}</small></label>
          <label v-if="form.subjectTypeCode === 'PERSON'" class="scr-field"><span>{{ tr('Birth date') }}</span><input v-model="form.birthDate" type="date"></label>
          <label class="scr-field"><span>{{ tr('Nationality') }}</span><input v-model="form.nationalityCode" dir="ltr" maxlength="2"><small v-if="errors.nationalityCode" class="err">{{ errors.nationalityCode }}</small></label>
          <label class="scr-field"><span>{{ tr('Purpose') }} *</span>
            <select v-model="form.purposeCode"><option v-for="p in ['ONBOARDING', 'TRANSACTION', 'PERIODIC_REVIEW', 'OTHER']" :key="p" :value="p">{{ tr(LABELS[p] ?? p) }}</option></select>
          </label>
          <label v-if="form.purposeCode === 'OTHER'" class="scr-field"><span>{{ tr('Describe the purpose') }} *</span><input v-model="form.purposeText"><small v-if="errors.purposeText" class="err">{{ errors.purposeText }}</small></label>
          <label class="scr-field"><span>{{ tr('Case reference') }}</span><input v-model="form.caseReference" dir="ltr"></label>
        </div>
        <div class="scr-actions">
          <button type="button" class="fx-btn" @click="clear">{{ tr('Clear') }}</button>
          <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="screen"><span class="fa fa-search" aria-hidden="true"></span>{{ tr('Screen') }}</button>
        </div>
      </section>
      <ScrInquiryResult v-if="result" :inquiry="result" />
    </template>

    <section v-else-if="tab === 'mine'" class="fx-card">
      <table class="fx-table">
        <thead><tr><th>{{ tr('Inquiry') }}</th><th>{{ tr('Name') }}</th><th>{{ tr('Identity number') }}</th><th>{{ tr('Purpose') }}</th><th>{{ tr('Result') }}</th><th>{{ tr('Date') }}</th><th>{{ tr('By') }}</th></tr></thead>
        <tbody>
          <tr v-for="i in history" :key="i.inquiryId" class="scr-row" @click="result = i; tab = 'new'">
            <td dir="ltr">{{ i.inquiryRef }}</td>
            <td>{{ i.subjectName }}</td>
            <td dir="ltr">{{ i.identityNo }}</td>
            <td>{{ tr(LABELS[i.purposeCode] ?? i.purposeCode) }}</td>
            <td><span class="fx-chip" :class="CHIP[i.resultCode]">{{ tr(LABELS[i.resultCode] ?? i.resultCode) }}</span></td>
            <td>{{ formatDateTime(i.screenedAt) }}</td>
            <td>{{ i.requestedBy }}</td>
          </tr>
        </tbody>
      </table>
      <div v-if="!history.length" class="fx-empty">{{ tr('No inquiries yet.') }}</div>
    </section>

    <ScrCompanyUsers v-else-if="tab === 'users' && company" :company-id="company.memberCompanyId" />
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
import { formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import ScrCompanyUsers from '../components/ScrCompanyUsers.vue'
import ScrInquiryResult from '../components/ScrInquiryResult.vue'
import { CHIP, LABELS, errorText, fieldErrors, scrApi, type ScrInquiry, type ScrInquiryRequest, type ScrMyCompany } from '../api/scr.api'

const canUsers = hasPermission('SCR.COMPANY.USERS')
const tab = ref<'new' | 'mine' | 'users'>('new')
const company = ref<ScrMyCompany | null>(null)
const result = ref<ScrInquiry | null>(null)
const history = ref<ScrInquiry[]>([])
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
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function openHistory() {
  tab.value = 'mine'
  try {
    history.value = await scrApi.myInquiries()
  } catch (err) {
    showError(errorText(err))
  }
}

onMounted(async () => {
  try {
    company.value = await scrApi.myCompany()
  } catch (err) {
    showError(errorText(err))
  }
})
</script>
