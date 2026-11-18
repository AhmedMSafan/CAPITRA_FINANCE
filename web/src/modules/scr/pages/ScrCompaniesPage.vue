<template>
  <!-- Member companies (federation): each company, its status, and its user accounts. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Member companies') }}</b>
        </nav>
        <h1>{{ tr('Member companies') }}</h1>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn fx-btn--teal" @click="edit(null)"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New company') }}</button>
      </div>
    </div>

    <section v-if="editing" class="fx-card">
      <div class="scr-grid scr-grid--3">
        <label class="scr-field"><span>{{ tr('Code') }} *</span><input v-model="form.companyCode" dir="ltr"><small v-if="errors.companyCode" class="err">{{ errors.companyCode }}</small></label>
        <label class="scr-field"><span>{{ tr('Name (English)') }} *</span><input v-model="form.nameEn" dir="ltr"><small v-if="errors.nameEn" class="err">{{ errors.nameEn }}</small></label>
        <label class="scr-field"><span>{{ tr('Name (Arabic)') }} *</span><input v-model="form.nameAr" dir="rtl"><small v-if="errors.nameAr" class="err">{{ errors.nameAr }}</small></label>
        <label class="scr-field"><span>{{ tr('Licence number') }}</span><input v-model="form.licenceNo" dir="ltr"></label>
        <label class="scr-field"><span>{{ tr('Email') }}</span><input v-model="form.contactEmail" type="email" dir="ltr"></label>
        <label class="scr-field"><span>{{ tr('Status') }}</span>
          <select v-model="form.statusCode"><option value="ACTIVE">{{ tr('Active') }}</option><option value="SUSPENDED">{{ tr('Suspended') }}</option></select>
        </label>
        <label class="scr-field scr-wide"><span>{{ tr('Notes') }}</span><textarea v-model="form.notes" rows="2"></textarea></label>
      </div>
      <div class="scr-actions">
        <button type="button" class="fx-btn" @click="editing = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save">{{ tr('Save') }}</button>
      </div>
    </section>

    <section class="fx-card">
      <table class="fx-table">
        <thead><tr><th>{{ tr('Code') }}</th><th>{{ tr('Name') }}</th><th>{{ tr('Licence number') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Users') }}</th><th>{{ tr('Inquiries') }}</th><th></th></tr></thead>
        <tbody>
          <tr v-for="c in companies" :key="c.memberCompanyId" :class="{ 'fx-row--on': selected === c.memberCompanyId }">
            <td dir="ltr"><b>{{ c.companyCode }}</b></td>
            <td>{{ c.nameAr }}<br><small class="scr-muted">{{ c.nameEn }}</small></td>
            <td dir="ltr">{{ c.licenceNo }}</td>
            <td><span class="fx-chip" :class="c.statusCode === 'ACTIVE' ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(LABELS[c.statusCode] ?? c.statusCode) }}</span></td>
            <td>{{ c.userCount }}</td>
            <td>{{ c.inquiryCount }}</td>
            <td>
              <button type="button" class="fx-btn fx-btn--sm" @click="edit(c)">{{ tr('Edit') }}</button>
              <button type="button" class="fx-btn fx-btn--sm" @click="selected = c.memberCompanyId">{{ tr('Users') }}</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="!companies.length" class="fx-empty">{{ tr('No member companies yet.') }}</div>
    </section>

    <ScrCompanyUsers v-if="selected !== null" :company-id="selected" />
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
import { LABELS, errorText, fieldErrors, scrApi, type ScrCompany, type ScrCompanyRequest } from '../api/scr.api'

const companies = ref<ScrCompany[]>([])
const selected = ref<number | null>(null)
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
