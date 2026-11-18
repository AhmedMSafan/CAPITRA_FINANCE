<template>
  <!-- Parties: the people and companies decisions are about, with their other names (screened like the name). -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Parties') }}</b>
        </nav>
        <h1>{{ tr('Parties') }}</h1>
      </div>
      <div class="fx-actions">
        <button v-if="canEdit" type="button" class="fx-btn fx-btn--teal" @click="edit(null)"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New party') }}</button>
      </div>
    </div>

    <section v-if="editing" class="fx-card">
      <div class="scr-grid scr-grid--3">
        <label class="scr-field"><span>{{ tr('Type') }} *</span>
          <select v-model="form.partyTypeCode" @change="typeChanged"><option value="PERSON">{{ tr('Person') }}</option><option value="COMPANY">{{ tr('Company') }}</option></select>
        </label>
        <label class="scr-field"><span>{{ tr('Name (Arabic)') }} *</span><input v-model="form.nameAr" dir="rtl"><small v-if="errors.nameAr" class="err">{{ errors.nameAr }}</small></label>
        <label class="scr-field"><span>{{ tr('Name (English)') }}</span><input v-model="form.nameEn" dir="ltr"></label>
        <label class="scr-field"><span>{{ tr('Identity type') }} *</span>
          <select v-model="form.identityTypeCode"><option v-for="t in idTypes" :key="t" :value="t">{{ tr(LABELS[t] ?? t) }}</option></select>
          <small v-if="errors.identityTypeCode" class="err">{{ errors.identityTypeCode }}</small>
        </label>
        <label class="scr-field"><span>{{ tr('Identity number') }} *</span><input v-model="form.identityNo" dir="ltr"><small v-if="errors.identityNo" class="err">{{ errors.identityNo }}</small></label>
        <label class="scr-field"><span>{{ tr('Nationality') }}</span><input v-model="form.nationalityCode" dir="ltr" maxlength="2"><small v-if="errors.nationalityCode" class="err">{{ errors.nationalityCode }}</small></label>
        <label v-if="form.partyTypeCode === 'PERSON'" class="scr-field"><span>{{ tr('Birth date') }}</span><input v-model="form.birthDate" type="date"><small v-if="errors.birthDate" class="err">{{ errors.birthDate }}</small></label>
        <label class="scr-field scr-wide"><span>{{ tr('Aliases') }} <small>({{ tr('one per line') }})</small></span><textarea v-model="aliasText" rows="2"></textarea></label>
        <label class="scr-field scr-wide"><span>{{ tr('Notes') }}</span><textarea v-model="form.notes" rows="2"></textarea></label>
      </div>
      <div class="scr-actions">
        <button type="button" class="fx-btn" @click="editing = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="saving" @click="save">{{ tr('Save') }}</button>
      </div>
    </section>

    <section class="fx-card">
      <div class="scr-filter"><input v-model="search" class="fx-search" :placeholder="tr('Search by name or identity number')" @keyup.enter="load"></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Name') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('Identity number') }}</th><th>{{ tr('Nationality') }}</th><th>{{ tr('Birth date') }}</th><th></th></tr></thead>
          <tbody>
            <tr v-for="p in parties" :key="p.partyId">
              <td><b>{{ p.nameAr }}</b><br><small class="scr-muted">{{ p.nameEn }}</small><span v-for="a in p.aliases" :key="a" class="scr-tag">{{ a }}</span></td>
              <td>{{ tr(LABELS[p.partyTypeCode] ?? p.partyTypeCode) }}</td>
              <td dir="ltr">{{ p.identityNo }} <small class="scr-muted">{{ tr(LABELS[p.identityTypeCode] ?? p.identityTypeCode) }}</small></td>
              <td>{{ p.nationalityCode }}</td>
              <td>{{ p.birthDate ? formatDate(p.birthDate) : '' }}</td>
              <td><button v-if="canEdit" type="button" class="fx-btn fx-btn--sm" @click="edit(p)">{{ tr('Edit') }}</button></td>
            </tr>
          </tbody>
        </table>
        <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
        <div v-else-if="!parties.length" class="fx-empty">{{ tr('No parties yet.') }}</div>
      </div>
    </section>
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
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { LABELS, errorText, fieldErrors, scrApi, type ScrParty, type ScrPartyRequest } from '../api/scr.api'

const canEdit = hasPermission('SCR.DECISION.EDIT')
const parties = ref<ScrParty[]>([])
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
  Object.assign(form, p ? { ...p } : blank())
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
