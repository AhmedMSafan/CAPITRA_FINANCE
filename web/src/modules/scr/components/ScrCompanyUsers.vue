<template>
  <!-- The user accounts of one member company: they sign in with the CAPITRA login and can only screen. -->
  <section class="fx-card fc-step-card">
    <header class="fc-sec-head">
      <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-user-circle-o"></span></span>
      <div>
        <h2>{{ tr('Company users') }} <span v-if="companyName" class="fc-muted">· {{ companyName }}</span></h2>
        <p>{{ tr('They sign in with the CAPITRA login and can only use the Screening Portal.') }}</p>
      </div>
      <button type="button" class="fx-btn fx-btn--teal fx-btn--sm" @click="edit(null)"><span class="fa fa-user-plus" aria-hidden="true"></span>{{ tr('New user') }}</button>
    </header>
    <div class="fx-table-wrap">
      <table class="fx-table">
        <thead>
          <tr><th>{{ tr('User name') }}</th><th>{{ tr('Full name') }}</th><th>{{ tr('Role') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Last sign-in') }}</th><th class="fx-r">{{ tr('Actions') }}</th></tr>
        </thead>
        <tbody>
          <tr v-for="u in users" :key="u.userId">
            <td dir="ltr"><b>{{ u.userName }}</b></td>
            <td>{{ u.fullName || '-' }}<span v-if="u.email" class="fx-cell-sub" dir="ltr">{{ u.email }}</span></td>
            <td><span class="fx-chip fx-chip--plain" :class="u.roleCode === 'SCR_COMPANY_ADMIN' ? 'fx-chip--brand' : ''">{{ tr(LABELS[u.roleCode] ?? u.roleCode) }}</span></td>
            <td><span class="fx-chip" :class="u.isActive ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(u.isActive ? 'Active' : 'Inactive') }}</span></td>
            <td>{{ u.lastLoginAt ? formatDateTime(u.lastLoginAt) : '-' }}</td>
            <td class="fx-r">
              <div class="fc-row-actions">
                <button type="button" class="fx-btn fx-btn--sm" @click="edit(u)"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}</button>
                <button type="button" class="fx-btn fx-btn--sm" @click="startReset(u)"><span class="fa fa-key" aria-hidden="true"></span>{{ tr('Reset password') }}</button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div v-if="!users.length" class="fx-empty"><span class="fa fa-user-circle-o" aria-hidden="true"></span>{{ tr('No users yet.') }}</div>

    <ScrDialog v-if="editing" :title="editId ? tr('Edit user') : tr('New user')" :subtitle="companyName" icon="fa-user-o" @close="editing = false">
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('User name') }} <b>*</b></span><input v-model="form.userName" class="fx-search" dir="ltr" :disabled="editId !== null" autocomplete="off"></label>
        <label class="fc-field"><span>{{ tr('Full name') }}</span><input v-model="form.fullName" class="fx-search"></label>
        <label class="fc-field"><span>{{ tr('Email') }}</span><input v-model="form.email" class="fx-search" type="email" dir="ltr"></label>
        <label class="fc-field"><span>{{ tr('Role') }}</span>
          <select v-model="form.roleCode" class="fx-select"><option v-for="r in ['SCR_COMPANY_USER', 'SCR_COMPANY_ADMIN']" :key="r" :value="r">{{ tr(LABELS[r] ?? r) }}</option></select>
        </label>
        <label v-if="editId === null" class="fc-field"><span>{{ tr('First password') }} <b>*</b></span><input v-model="form.password" class="fx-search" type="password" autocomplete="new-password"></label>
        <label class="fc-check" style="align-self:end;padding-bottom:8px"><input v-model="form.isActive" type="checkbox"><span>{{ tr('Active') }}</span></label>
        <div v-for="(m, k) in errors" :key="k" class="fc-note fc-note--warn fc-full"><span class="fa fa-exclamation-circle" aria-hidden="true"></span><span>{{ m }}</span></div>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" @click="editing = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Save') }}</button>
      </template>
    </ScrDialog>

    <ScrDialog v-if="resetting" :title="tr('Reset password')" :subtitle="resetting.userName" icon="fa-key" orange @close="resetting = null">
      <div class="fc-grid">
        <label class="fc-field"><span>{{ tr('New password') }} <b>*</b></span><input v-model="newPassword" class="fx-search" type="password" autocomplete="new-password">
          <small v-if="resetError" class="scr-err">{{ resetError }}</small></label>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" @click="resetting = null">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="busy || !newPassword" @click="reset"><span class="fa fa-key" aria-hidden="true"></span>{{ tr('Reset password') }}</button>
      </template>
    </ScrDialog>
  </section>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDateTime } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import ScrDialog from './ScrDialog.vue'
import { LABELS, errorText, fieldErrors, scrApi, type ScrCompanyUser, type ScrCompanyUserRequest } from '../api/scr.api'

const props = defineProps<{ companyId: number; companyName?: string }>()
const users = ref<ScrCompanyUser[]>([])
const editing = ref(false)
const editId = ref<string | null>(null)
const busy = ref(false)
const errors = ref<Record<string, string>>({})
const resetting = ref<ScrCompanyUser | null>(null)
const newPassword = ref('')
const resetError = ref('')
const blank = (): ScrCompanyUserRequest => ({ userName: '', password: null, email: null, fullName: null, roleCode: 'SCR_COMPANY_USER', isActive: true })
const form = reactive<ScrCompanyUserRequest>(blank())

async function load() {
  try {
    users.value = await scrApi.companyUsers(props.companyId)
  } catch (err) {
    showError(errorText(err))
  }
}

function edit(u: ScrCompanyUser | null) {
  errors.value = {}
  editId.value = u?.userId ?? null
  Object.assign(form, u ? { userName: u.userName, password: null, email: u.email, fullName: u.fullName, roleCode: u.roleCode, isActive: u.isActive } : blank())
  editing.value = true
}

async function save() {
  busy.value = true
  errors.value = {}
  try {
    await scrApi.saveCompanyUser(props.companyId, editId.value, { ...form })
    editing.value = false
    showSuccess(tr('User saved.'))
    await load()
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) showError(errorText(err))
  } finally {
    busy.value = false
  }
}

function startReset(u: ScrCompanyUser) {
  resetting.value = u
  newPassword.value = ''
  resetError.value = ''
}

async function reset() {
  if (!resetting.value) return
  busy.value = true
  resetError.value = ''
  try {
    await scrApi.resetCompanyUserPassword(props.companyId, resetting.value.userId, newPassword.value)
    resetting.value = null
    showSuccess(tr('Password reset.'))
  } catch (err) {
    resetError.value = Object.values(fieldErrors(err))[0] ?? errorText(err)
  } finally {
    busy.value = false
  }
}

watch(() => props.companyId, load)
onMounted(load)
</script>
