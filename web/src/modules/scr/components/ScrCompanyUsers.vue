<template>
  <!-- The user accounts of one member company: they sign in with the CAPITRA login and can only screen. -->
  <section class="fx-card">
    <div class="fx-head">
      <h3>{{ tr('Company users') }}</h3>
      <button type="button" class="fx-btn fx-btn--teal" @click="edit(null)"><span class="fa fa-user-plus" aria-hidden="true"></span>{{ tr('New user') }}</button>
    </div>
    <div v-if="editing" class="scr-grid scr-grid--3">
      <label class="scr-field"><span>{{ tr('User name') }} *</span><input v-model="form.userName" dir="ltr" :disabled="editId !== null"></label>
      <label class="scr-field"><span>{{ tr('Full name') }}</span><input v-model="form.fullName"></label>
      <label class="scr-field"><span>{{ tr('Email') }}</span><input v-model="form.email" type="email" dir="ltr"></label>
      <label class="scr-field"><span>{{ tr('Role') }}</span>
        <select v-model="form.roleCode"><option v-for="r in ['SCR_COMPANY_USER', 'SCR_COMPANY_ADMIN']" :key="r" :value="r">{{ tr(LABELS[r] ?? r) }}</option></select>
      </label>
      <label v-if="editId === null" class="scr-field"><span>{{ tr('First password') }} *</span><input v-model="form.password" type="password" autocomplete="new-password"></label>
      <label class="scr-field"><span>{{ tr('Active') }}</span><input v-model="form.isActive" type="checkbox"></label>
      <small v-for="(m, k) in errors" :key="k" class="err scr-wide">{{ m }}</small>
      <div class="scr-actions scr-wide">
        <button type="button" class="fx-btn" @click="editing = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save">{{ tr('Save') }}</button>
      </div>
    </div>
    <table class="fx-table">
      <thead><tr><th>{{ tr('User name') }}</th><th>{{ tr('Full name') }}</th><th>{{ tr('Role') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Last sign-in') }}</th><th></th></tr></thead>
      <tbody>
        <tr v-for="u in users" :key="u.userId">
          <td dir="ltr">{{ u.userName }}</td>
          <td>{{ u.fullName }}<br><small class="scr-muted" dir="ltr">{{ u.email }}</small></td>
          <td>{{ tr(LABELS[u.roleCode] ?? u.roleCode) }}</td>
          <td><span class="fx-chip" :class="u.isActive ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(u.isActive ? 'Active' : 'Inactive') }}</span></td>
          <td>{{ u.lastLoginAt ? formatDateTime(u.lastLoginAt) : '—' }}</td>
          <td>
            <button type="button" class="fx-btn fx-btn--sm" @click="edit(u)">{{ tr('Edit') }}</button>
            <button type="button" class="fx-btn fx-btn--sm" @click="reset(u)">{{ tr('Reset password') }}</button>
          </td>
        </tr>
      </tbody>
    </table>
    <div v-if="!users.length" class="fx-empty">{{ tr('No users yet.') }}</div>
  </section>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref, watch } from 'vue'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { LABELS, errorText, fieldErrors, scrApi, type ScrCompanyUser, type ScrCompanyUserRequest } from '../api/scr.api'

const props = defineProps<{ companyId: number }>()
const users = ref<ScrCompanyUser[]>([])
const editing = ref(false)
const editId = ref<string | null>(null)
const busy = ref(false)
const errors = ref<Record<string, string>>({})
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

async function reset(u: ScrCompanyUser) {
  const password = window.prompt(`${tr('New password for')} ${u.userName}`)
  if (!password) return
  try {
    await scrApi.resetCompanyUserPassword(props.companyId, u.userId, password)
    showSuccess(tr('Password reset.'))
  } catch (err) {
    showError(Object.values(fieldErrors(err))[0] ?? errorText(err))
  }
}

watch(() => props.companyId, load)
onMounted(load)
</script>
