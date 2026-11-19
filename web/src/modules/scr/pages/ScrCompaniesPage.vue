<template>
  <!-- Member companies (federation): each company, its status, and its user accounts. -->
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
            <p>{{ tr('Member companies and their user accounts.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn fx-btn--primary" @click="edit(null)"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New company') }}</button>
      </div>
    </div>

    <div class="fx-card">
      <div class="fc-card-head"><h2>{{ tr('Member companies') }} ({{ companies.length }})</h2></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Code') }}</th><th>{{ tr('Name') }}</th><th>{{ tr('Licence number') }}</th><th>{{ tr('Status') }}</th>
              <th>{{ tr('Users') }}</th><th>{{ tr('Inquiries') }}</th><th class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in companies" :key="c.memberCompanyId">
              <td dir="ltr"><b class="fc-ref">{{ c.companyCode }}</b></td>
              <td><b>{{ c.nameAr }}</b><span class="fx-cell-sub">{{ c.nameEn }}</span></td>
              <td dir="ltr">{{ c.licenceNo || '-' }}<span v-if="c.contactEmail" class="fx-cell-sub">{{ c.contactEmail }}</span></td>
              <td><span class="fx-chip" :class="c.statusCode === 'ACTIVE' ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(LABELS[c.statusCode] ?? c.statusCode) }}</span></td>
              <td>{{ c.userCount }}</td>
              <td>{{ c.inquiryCount }}</td>
              <td class="fx-r">
                <div class="fc-row-actions">
                  <button type="button" class="fx-btn fx-btn--sm" @click="edit(c)"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}</button>
                  <button type="button" class="fx-btn fx-btn--teal fx-btn--sm" @click="selected = c"><span class="fa fa-users" aria-hidden="true"></span>{{ tr('Users') }}</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="!companies.length" class="fx-empty"><span class="fa fa-building-o" aria-hidden="true"></span>{{ tr('No member companies yet.') }}</div>
    </div>

    <ScrCompanyUsers v-if="selected" :company-id="selected.memberCompanyId" :company-name="`${selected.companyCode} · ${selected.nameAr}`" />

    <ScrDialog v-if="editing" :title="editId ? tr('Edit company') : tr('New company')" :subtitle="tr('A suspended company\'s users cannot screen.')" icon="fa-building-o" @close="editing = false">
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Code') }} <b>*</b></span><input v-model="form.companyCode" class="fx-search" dir="ltr">
          <small v-if="errors.companyCode" class="scr-err">{{ errors.companyCode }}</small></label>
        <label class="fc-field"><span>{{ tr('Name (Arabic)') }} <b>*</b></span><input v-model="form.nameAr" class="fx-search" dir="rtl">
          <small v-if="errors.nameAr" class="scr-err">{{ errors.nameAr }}</small></label>
        <label class="fc-field"><span>{{ tr('Name (English)') }} <b>*</b></span><input v-model="form.nameEn" class="fx-search" dir="ltr">
          <small v-if="errors.nameEn" class="scr-err">{{ errors.nameEn }}</small></label>
        <label class="fc-field"><span>{{ tr('Licence number') }}</span><input v-model="form.licenceNo" class="fx-search" dir="ltr"></label>
        <label class="fc-field"><span>{{ tr('Email') }}</span><input v-model="form.contactEmail" class="fx-search" type="email" dir="ltr"></label>
        <label class="fc-field"><span>{{ tr('Status') }}</span>
          <select v-model="form.statusCode" class="fx-select"><option value="ACTIVE">{{ tr('Active') }}</option><option value="SUSPENDED">{{ tr('Suspended') }}</option></select>
        </label>
        <label class="fc-field fc-full"><span>{{ tr('Notes') }}</span><textarea v-model="form.notes" class="fx-search fc-textarea"></textarea></label>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" @click="editing = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Save') }}</button>
      </template>
    </ScrDialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { onMounted, reactive, ref } from 'vue'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import ScrCompanyUsers from '../components/ScrCompanyUsers.vue'
import ScrDialog from '../components/ScrDialog.vue'
import { LABELS, errorText, fieldErrors, scrApi, type ScrCompany, type ScrCompanyRequest } from '../api/scr.api'

const companies = ref<ScrCompany[]>([])
const selected = ref<ScrCompany | null>(null)
const editing = ref(false)
const editId = ref<number | null>(null)
const busy = ref(false)
const errors = ref<Record<string, string>>({})
const blank = (): ScrCompanyRequest => ({ companyCode: '', nameEn: '', nameAr: '', licenceNo: null, contactEmail: null, statusCode: 'ACTIVE', notes: null })
const form = reactive<ScrCompanyRequest>(blank())

async function load() {
  try {
    companies.value = await scrApi.companies()
  } catch (err) {
    showError(errorText(err))
  }
}

function edit(c: ScrCompany | null) {
  errors.value = {}
  editId.value = c?.memberCompanyId ?? null
  Object.assign(form, c
    ? { companyCode: c.companyCode, nameEn: c.nameEn, nameAr: c.nameAr, licenceNo: c.licenceNo, contactEmail: c.contactEmail, statusCode: c.statusCode, notes: c.notes }
    : blank())
  editing.value = true
}

async function save() {
  busy.value = true
  errors.value = {}
  try {
    await scrApi.saveCompany(editId.value, { ...form })
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

onMounted(load)
</script>
