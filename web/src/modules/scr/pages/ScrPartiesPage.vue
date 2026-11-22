<template>
  <!-- Parties: the people and companies decisions are about, with their other names (screened like the name). -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Parties') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-users"></span></span>
          <div>
            <h1>{{ tr('Parties') }}</h1>
            <p>{{ tr('The people and companies decisions are about, with their other names.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openScreeningManual(6)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
        <button v-if="canEdit" type="button" class="fx-btn fx-btn--primary" @click="edit(null)"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New party') }}</button>
      </div>
    </div>

    <div class="fx-card fx-card-pad fc-filters">
      <label class="fc-field"><span>{{ tr('Search') }}</span>
        <input v-model="search" class="fx-search" :placeholder="tr('Search by name or identity number')" @keyup.enter="load">
      </label>
      <ScrUserFilter v-model="who" :me="me" :users="users" />
      <div class="fc-filter-actions">
        <button type="button" class="fx-btn fx-btn--teal" @click="load"><span class="fa fa-search" aria-hidden="true"></span>{{ tr('Search') }}</button>
        <button type="button" class="fx-btn" @click="search = ''; load()"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Clear') }}</button>
      </div>
    </div>

    <div class="fx-card">
      <div class="fc-card-head"><h2>{{ tr('Parties') }} ({{ scoped.length }})</h2></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th class="fc-no">#</th><th>{{ tr('Name') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('Identity number') }}</th>
              <th>{{ tr('Nationality') }}</th><th>{{ tr('Birth date') }}</th><th>{{ tr('Entered by') }}</th><th class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(p, i) in scoped" :key="p.partyId">
              <td class="fc-no">{{ i + 1 }}</td>
              <td><b>{{ p.nameAr }}</b><span v-if="p.nameEn" class="fx-cell-sub">{{ p.nameEn }}</span><span v-for="a in p.aliases" :key="a" class="scr-tag">{{ a }}</span></td>
              <td><span class="fx-chip fx-chip--plain">{{ tr(LABELS[p.partyTypeCode] ?? p.partyTypeCode) }}</span></td>
              <td dir="ltr">{{ p.identityNo }}<span class="fx-cell-sub">{{ tr(LABELS[p.identityTypeCode] ?? p.identityTypeCode) }}</span></td>
              <td>{{ p.nationalityCode }}</td>
              <td>{{ p.birthDate ? formatDate(p.birthDate) : '-' }}</td>
              <td>{{ p.createdBy || '-' }}</td>
              <td class="fx-r"><button v-if="canEdit" type="button" class="fx-btn fx-btn--sm" @click="edit(p)"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}</button></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
      <div v-else-if="!scoped.length" class="fx-empty"><span class="fa fa-users" aria-hidden="true"></span>{{ parties.length ? tr('Nothing here for the chosen user. Choose All users to see everything.') : tr('No parties yet.') }}</div>
    </div>

    <ScrDialog v-if="editing" :title="editId ? tr('Edit party') : tr('New party')" :subtitle="tr('Other names are screened like the name.')" icon="fa-user-o" @close="editing = false">
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Type') }} <b>*</b></span>
          <select v-model="form.partyTypeCode" class="fx-select" @change="typeChanged"><option value="PERSON">{{ tr('Person') }}</option><option value="COMPANY">{{ tr('Company') }}</option></select>
        </label>
        <label class="fc-field"><span>{{ tr('Name (Arabic)') }} <b>*</b></span><input v-model="form.nameAr" class="fx-search" dir="rtl">
          <small v-if="errors.nameAr" class="scr-err">{{ errors.nameAr }}</small></label>
        <label class="fc-field"><span>{{ tr('Name (English)') }}</span><input v-model="form.nameEn" class="fx-search" dir="ltr"></label>
        <label class="fc-field"><span>{{ tr('Identity type') }} <b>*</b></span>
          <select v-model="form.identityTypeCode" class="fx-select"><option v-for="t in idTypes" :key="t" :value="t">{{ tr(LABELS[t] ?? t) }}</option></select>
          <small v-if="errors.identityTypeCode" class="scr-err">{{ errors.identityTypeCode }}</small></label>
        <label class="fc-field"><span>{{ tr('Identity number') }} <b>*</b></span><input v-model="form.identityNo" class="fx-search" dir="ltr">
          <small v-if="errors.identityNo" class="scr-err">{{ errors.identityNo }}</small></label>
        <label class="fc-field"><span>{{ tr('Nationality') }}</span><input v-model="form.nationalityCode" class="fx-search" dir="ltr" maxlength="2" placeholder="EG">
          <small v-if="errors.nationalityCode" class="scr-err">{{ errors.nationalityCode }}</small></label>
        <label v-if="form.partyTypeCode === 'PERSON'" class="fc-field"><span>{{ tr('Birth date') }}</span><input v-model="form.birthDate" class="fx-search" type="date">
          <small v-if="errors.birthDate" class="scr-err">{{ errors.birthDate }}</small></label>
        <label class="fc-field fc-full"><span>{{ tr('Aliases') }} <small class="scr-hint">({{ tr('one per line') }})</small></span>
          <textarea v-model="aliasText" class="fx-search fc-textarea"></textarea></label>
        <label class="fc-field fc-full"><span>{{ tr('Notes') }}</span><textarea v-model="form.notes" class="fx-search fc-textarea"></textarea></label>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" @click="editing = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="saving" @click="save"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Save') }}</button>
      </template>
    </ScrDialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import { useUserFilter } from '../utils/userFilter'
import ScrUserFilter from '../components/ScrUserFilter.vue'
import ScrDialog from '../components/ScrDialog.vue'
import { LABELS, errorText, fieldErrors, scrApi, type ScrParty, type ScrPartyRequest } from '../api/scr.api'
import { openScreeningManual } from '@/modules/scr/utils/manual'

const canEdit = hasPermission('SCR.DECISION.EDIT')
const parties = ref<ScrParty[]>([])
/** Entered by me by default (Admin: all users). */
const { who, me, users, scoped } = useUserFilter(parties, (p) => p.createdBy)
const loading = ref(false)
const saving = ref(false)
const editing = ref(false)
const editId = ref<number | null>(null)
const search = ref('')
const aliasText = ref('')
const errors = ref<Record<string, string>>({})
const blank = (): ScrPartyRequest => ({
  partyTypeCode: 'PERSON', nameAr: '', nameEn: null, identityTypeCode: 'NATIONAL_ID', identityNo: '', nationalityCode: 'EG', birthDate: null, notes: null, aliases: []
})
const form = reactive<ScrPartyRequest>(blank())
const idTypes = computed(() => (form.partyTypeCode === 'PERSON' ? ['NATIONAL_ID', 'PASSPORT', 'OTHER'] : ['COMMERCIAL_REGISTER', 'OTHER']))

function typeChanged() {
  form.identityTypeCode = form.partyTypeCode === 'PERSON' ? 'NATIONAL_ID' : 'COMMERCIAL_REGISTER'
  if (form.partyTypeCode === 'COMPANY') form.birthDate = null
}

function edit(p: ScrParty | null) {
  errors.value = {}
  editId.value = p?.partyId ?? null
  Object.assign(form, p
    ? { partyTypeCode: p.partyTypeCode, nameAr: p.nameAr, nameEn: p.nameEn, identityTypeCode: p.identityTypeCode, identityNo: p.identityNo,
        nationalityCode: p.nationalityCode, birthDate: p.birthDate, notes: p.notes, aliases: p.aliases }
    : blank())
  aliasText.value = (p?.aliases ?? []).join('\n')
  editing.value = true
}

async function load() {
  loading.value = true
  try {
    parties.value = await scrApi.parties(search.value)
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  errors.value = {}
  try {
    const aliases = aliasText.value.split('\n').map((a) => a.trim()).filter(Boolean)
    await scrApi.saveParty(editId.value, { ...form, aliases })
    editing.value = false
    showSuccess(tr('Party saved.'))
    await load()
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) showError(errorText(err))
  } finally {
    saving.value = false
  }
}

onMounted(load)
</script>
