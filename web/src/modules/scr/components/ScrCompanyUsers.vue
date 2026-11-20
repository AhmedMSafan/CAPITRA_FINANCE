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
            <td><span class="fx-chip fx-chip--plain" :class="u.roleCode === 'MEMBER_COMPANY_ADMIN' ? 'fx-chip--brand' : ''">{{ tr(LABELS[u.roleCode] ?? u.roleCode) }}</span></td>
            <td><span class="fx-chip" :class="u.isActive ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(u.isActive ? 'Active' : 'Inactive') }}</span>
              <span v-if="u.mustChangePassword" class="fx-cell-sub scr-temp-flag"><span class="fa fa-clock-o" aria-hidden="true"></span> {{ tr('Has not chosen a password yet') }}</span></td>
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
          <select v-model="form.roleCode" class="fx-select"><option v-for="r in ['MEMBER_COMPANY_USER', 'MEMBER_COMPANY_ADMIN']" :key="r" :value="r">{{ tr(LABELS[r] ?? r) }}</option></select>
        </label>
        <label class="fc-check" style="align-self:end;padding-bottom:8px"><input v-model="form.isActive" type="checkbox"><span>{{ tr('Active') }}</span></label>
        <div v-if="editId === null" class="fc-note fc-full"><span class="fa fa-info-circle" aria-hidden="true"></span>
          <span>{{ tr('The system makes a temporary password, shown to you once after saving. The user must replace it with their own at first sign-in.') }}</span></div>
        <div v-for="(m, k) in errors" :key="k" class="fc-note fc-note--warn fc-full"><span class="fa fa-exclamation-circle" aria-hidden="true"></span><span>{{ m }}</span></div>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" @click="editing = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Save') }}</button>
      </template>
    </ScrDialog>

    <ScrDialog v-if="resetting" :title="tr('Reset password')" :subtitle="resetting.userName" icon="fa-key" orange @close="resetting = null">
      <p class="scr-confirm">{{ tr('A new temporary password will be made and shown to you once. The user\'s current password and open sessions stop working, and they must choose their own password at the next sign-in.') }}</p>
      <small v-if="resetError" class="scr-err">{{ resetError }}</small>
      <template #actions>
        <button type="button" class="fx-btn" @click="resetting = null">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="reset"><span class="fa fa-key" aria-hidden="true"></span>{{ tr('Make a new temporary password') }}</button>
      </template>
    </ScrDialog>

    <!-- the temporary password, shown once -->
    <ScrDialog v-if="issued" :title="tr('Temporary password')" :subtitle="issued.userName" icon="fa-key" @close="closeIssued">
      <div class="scr-temp">
        <code dir="ltr">{{ issued.password }}</code>
        <button type="button" class="fx-btn fx-btn--teal fx-btn--sm" @click="copy"><span :class="['fa', copied ? 'fa-check' : 'fa-copy']" aria-hidden="true"></span>{{ tr(copied ? 'Copied' : 'Copy') }}</button>
      </div>
      <div class="fc-note fc-note--warn"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>
        <span>{{ tr('This is the only time it is shown. Give it to the user securely (not by the same e-mail as the user name). At first sign-in they must replace it with their own password.') }}</span></div>
      <template #actions>
        <button type="button" class="fx-btn fx-btn--primary" @click="closeIssued"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('I have given it to the user') }}</button>
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
const resetError = ref('')
/** A temporary password just made: shown once, then forgotten. */
const issued = ref<{ userName: string; password: string } | null>(null)
const copied = ref(false)
const blank = (): ScrCompanyUserRequest => ({ userName: '', email: null, fullName: null, roleCode: 'MEMBER_COMPANY_USER', isActive: true })
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
  Object.assign(form, u ? { userName: u.userName, email: u.email, fullName: u.fullName, roleCode: u.roleCode, isActive: u.isActive } : blank())
  editing.value = true
}

async function save() {
  busy.value = true
  errors.value = {}
  try {
    const saved = await scrApi.saveCompanyUser(props.companyId, editId.value, { ...form })
    editing.value = false
    showSuccess(tr('User saved.'))
    if (saved.temporaryPassword) show(saved.userName, saved.temporaryPassword)
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
  resetError.value = ''
}

function show(userName: string, password: string) {
  copied.value = false
  issued.value = { userName, password }
}

async function copy() {
  if (!issued.value) return
  try {
    await navigator.clipboard.writeText(issued.value.password)
    copied.value = true
  } catch {
    copied.value = false
  }
}

function closeIssued() {
  issued.value = null
  copied.value = false
}

async function reset() {
  if (!resetting.value) return
  busy.value = true
  resetError.value = ''
  try {
    const r = await scrApi.resetCompanyUserPassword(props.companyId, resetting.value.userId)
    resetting.value = null
    if (r.temporaryPassword) show(r.userName, r.temporaryPassword)
    await load()
  } catch (err) {
    resetError.value = Object.values(fieldErrors(err))[0] ?? errorText(err)
  } finally {
    busy.value = false
  }
}

watch(() => props.companyId, load)
onMounted(load)
</script>
