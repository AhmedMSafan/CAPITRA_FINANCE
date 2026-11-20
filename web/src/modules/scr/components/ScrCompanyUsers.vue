<template>
  <!--
    The accounts of one member company: its administrator (made with the company) and its users, within the licence. Every account
    is activated by an e-mailed link; no password is ever set or shown here.
  -->
  <section class="fx-card fc-step-card">
    <header class="fc-sec-head">
      <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-user-circle-o"></span></span>
      <div>
        <h2>{{ tr('Company users') }} <span v-if="companyName" class="fc-muted">· {{ companyName }}</span></h2>
        <p>{{ tr('Each account is activated by an e-mailed link where the user chooses their own password.') }}</p>
      </div>
      <div class="scr-seats scr-seats--head" :title="tr('Users besides the administrator')">
        <b>{{ inUse }}<template v-if="maxUsers !== undefined"> / {{ maxUsers }}</template></b>
        <small>{{ tr('users in use') }}</small>
      </div>
      <button type="button" class="fx-btn fx-btn--teal fx-btn--sm" :disabled="full" :title="full ? tr('All the licence\'s users are in use.') : ''" @click="edit(null)">
        <span class="fa fa-user-plus" aria-hidden="true"></span>{{ tr('New user') }}
      </button>
    </header>
    <div v-if="noAdmin" class="fc-note fc-note--warn scr-full-note"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>
      <span>{{ tr('This company has no administrator yet. The next user you add becomes its administrator.') }}</span></div>
    <div v-if="full" class="fc-note fc-note--warn scr-full-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
      <span>{{ maxUsers === 0 ? tr('The licence is for the administrator only. Ask the Federation to add users to the licence.') : tr('All the licence\'s users are in use. Deactivate a user or ask the Federation for more.') }}</span></div>
    <div class="fx-table-wrap">
      <table class="fx-table">
        <thead>
          <tr><th>{{ tr('User name') }}</th><th>{{ tr('Full name') }}</th><th>{{ tr('Role') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Last sign-in') }}</th><th class="fx-r">{{ tr('Actions') }}</th></tr>
        </thead>
        <tbody>
          <tr v-for="u in users" :key="u.userId">
            <td dir="ltr"><b>{{ u.userName }}</b></td>
            <td>{{ u.fullName || '-' }}<span v-if="u.email" class="fx-cell-sub" dir="ltr">{{ u.email }}</span></td>
            <td><span class="fx-chip fx-chip--plain" :class="u.roleCode === ADMIN ? 'fx-chip--brand' : ''">{{ tr(LABELS[u.roleCode] ?? u.roleCode) }}</span></td>
            <td><span class="fx-chip" :class="STATUS_CHIP[u.statusCode]">{{ tr(LABELS[u.statusCode] ?? u.statusCode) }}</span></td>
            <td>{{ u.lastLoginAt ? formatDateTime(u.lastLoginAt) : '-' }}</td>
            <td class="fx-r">
              <div class="fc-row-actions">
                <button type="button" class="fx-btn fx-btn--sm" @click="edit(u)"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}</button>
                <button v-if="u.isActive" type="button" class="fx-btn fx-btn--sm" @click="linkFor = u">
                  <span class="fa fa-envelope-o" aria-hidden="true"></span>{{ tr(u.statusCode === 'PENDING_ACTIVATION' ? 'Resend invitation' : 'Send password reset') }}
                </button>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
    <div v-if="!users.length" class="fx-empty"><span class="fa fa-user-circle-o" aria-hidden="true"></span>{{ tr('No users yet.') }}</div>

    <ScrDialog v-if="editing" :title="editId ? tr('Edit user') : noAdmin ? tr('New company administrator') : tr('New user')" :subtitle="companyName" icon="fa-user-o" @close="editing = false">
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Full name') }} <b>*</b></span><input v-model="form.fullName" class="fx-search">
          <small v-if="errors.fullName" class="scr-err">{{ errors.fullName }}</small></label>
        <label class="fc-field"><span>{{ tr('E-mail') }} <b>*</b></span><input v-model="form.email" class="fx-search" type="email" dir="ltr">
          <small v-if="errors.email" class="scr-err">{{ errors.email }}</small></label>
        <label class="fc-field"><span>{{ tr('User name') }} <b>*</b></span><input v-model="form.userName" class="fx-search" dir="ltr" :disabled="editId !== null" autocomplete="off" :placeholder="form.email || ''">
          <small v-if="editId === null" class="scr-hint">{{ tr('Empty = the e-mail address') }}</small>
          <small v-if="errors.userName" class="scr-err">{{ errors.userName }}</small></label>
        <label v-if="editId !== null && !editingAdmin" class="fc-check" style="align-self:end;padding-bottom:8px"><input v-model="form.isActive" type="checkbox"><span>{{ tr('Active') }}</span></label>
        <div v-if="editId === null" class="fc-note fc-full"><span class="fa fa-envelope-o" aria-hidden="true"></span>
          <span>{{ noAdmin ? tr('This user becomes the company administrator and receives an e-mail with a link to choose their own password.')
            : tr('The user receives an e-mail with a link to choose their own password. The account holds one of the licence\'s users from now on.') }}</span></div>
        <div v-if="editingAdmin" class="fc-note fc-full"><span class="fa fa-info-circle" aria-hidden="true"></span>
          <span>{{ tr('The company administrator cannot be deactivated: a company always has one.') }}</span></div>
        <div v-for="(m, k) in otherErrors" :key="k" class="fc-note fc-note--warn fc-full"><span class="fa fa-exclamation-circle" aria-hidden="true"></span><span>{{ m }}</span></div>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" @click="editing = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-check']" aria-hidden="true"></span>{{ editId ? tr('Save') : tr('Create and send the invitation') }}
        </button>
      </template>
    </ScrDialog>

    <!-- e-mail a link, or copy it when the e-mail does not arrive -->
    <ScrDialog v-if="linkFor" :title="tr(linkFor.statusCode === 'PENDING_ACTIVATION' ? 'Resend invitation' : 'Send password reset')" :subtitle="`${linkFor.userName} · ${linkFor.email ?? ''}`" icon="fa-envelope-o" @close="closeLink">
      <p class="scr-confirm">{{ tr(linkFor.statusCode === 'PENDING_ACTIVATION'
        ? 'A new activation link is e-mailed; the earlier one stops working.'
        : 'A link to choose a new password is e-mailed. Their current password keeps working until they use it.') }}</p>
      <div v-if="outcome" class="fc-note" :class="outcome.sent ? '' : 'fc-note--warn'">
        <span :class="['fa', outcome.sent ? 'fa-check' : 'fa-exclamation-triangle']" aria-hidden="true"></span>
        <span>{{ outcome.sent ? tr('The e-mail was sent.') : `${tr('The e-mail could not be sent')} (${tr(LABELS[outcome.statusCode] ?? outcome.statusCode)}). ${tr('Copy the link and send it another way.')}` }}</span>
      </div>
      <ScrLinkBox v-if="outcome && !outcome.sent" :company-id="companyId" :user-id="linkFor.userId" />
      <template #actions>
        <button type="button" class="fx-btn" @click="closeLink">{{ tr('Close') }}</button>
        <button v-if="!outcome" type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="sendLink">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-paper-plane']" aria-hidden="true"></span>{{ tr('Send the e-mail') }}
        </button>
      </template>
    </ScrDialog>

    <!-- what happened to a new user's invitation -->
    <ScrDialog v-if="invited" :title="tr('User created')" :subtitle="invited.user.userName" icon="fa-check" @close="invited = null">
      <div v-if="invited.email.sent" class="fc-note"><span class="fa fa-envelope-o" aria-hidden="true"></span>
        <span>{{ tr('The activation e-mail was sent to') }} <b dir="ltr">{{ invited.user.email }}</b>. {{ tr('The link works once and expires in 48 hours.') }}</span></div>
      <template v-else>
        <div class="fc-note fc-note--warn"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>
          <span>{{ tr('The activation e-mail could not be sent') }} ({{ tr(LABELS[invited.email.statusCode] ?? invited.email.statusCode) }}). {{ tr('Copy the link and send it another way.') }}</span></div>
        <ScrLinkBox :company-id="companyId" :user-id="invited.user.userId" />
      </template>
      <template #actions>
        <button type="button" class="fx-btn fx-btn--primary" @click="invited = null">{{ tr('Done') }}</button>
      </template>
    </ScrDialog>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDateTime } from '../utils/dates'
import ScrDialog from './ScrDialog.vue'
import ScrLinkBox from './ScrLinkBox.vue'
import { LABELS, errorText, fieldErrors, scrApi, type ScrCompanyUser, type ScrCompanyUserRequest, type ScrEmailOutcome } from '../api/scr.api'

const props = defineProps<{ companyId: number; companyName?: string; /** Users allowed besides the administrator; unknown in the portal. */ maxUsers?: number }>()
const emit = defineEmits<{ changed: [] }>()

const ADMIN = 'MEMBER_COMPANY_ADMIN'
const STATUS_CHIP: Record<string, string> = { ACTIVE: 'fx-chip--ok', PENDING_ACTIVATION: 'fx-chip--warn', INACTIVE: 'fx-chip--bad' }

const users = ref<ScrCompanyUser[]>([])
const loaded = ref(false)
/** Seats held: the company's active users besides the administrator, pending activation included. */
const inUse = computed(() => users.value.filter((u) => u.roleCode !== ADMIN && u.isActive).length)
/** A company made before administrators came with it: the Federation's next user for it becomes its administrator. */
const noAdmin = computed(() => loaded.value && !users.value.some((u) => u.roleCode === ADMIN))
const full = computed(() => !noAdmin.value && props.maxUsers !== undefined && inUse.value >= props.maxUsers)

const editing = ref(false)
const editId = ref<string | null>(null)
const editingAdmin = ref(false)
const busy = ref(false)
const errors = ref<Record<string, string>>({})
const otherErrors = computed(() => Object.fromEntries(Object.entries(errors.value).filter(([k]) => !['fullName', 'email', 'userName'].includes(k))))
const blank = (): ScrCompanyUserRequest => ({ userName: '', email: null, fullName: null, isActive: true })
const form = reactive<ScrCompanyUserRequest>(blank())

const linkFor = ref<ScrCompanyUser | null>(null)
const outcome = ref<ScrEmailOutcome | null>(null)
const invited = ref<{ user: ScrCompanyUser; email: ScrEmailOutcome } | null>(null)

async function load() {
  try {
    users.value = await scrApi.companyUsers(props.companyId)
    loaded.value = true
  } catch (err) {
    showError(errorText(err))
  }
}

function edit(u: ScrCompanyUser | null) {
  errors.value = {}
  editId.value = u?.userId ?? null
  editingAdmin.value = u?.roleCode === ADMIN
  Object.assign(form, u ? { userName: u.userName, email: u.email, fullName: u.fullName, isActive: u.isActive } : blank())
  editing.value = true
}

async function save() {
  busy.value = true
  errors.value = {}
  const request = { ...form, userName: form.userName?.trim() || form.email?.trim() || '' }
  try {
    if (editId.value) {
      await scrApi.updateCompanyUser(props.companyId, editId.value, request)
      showSuccess(tr('User saved.'))
    } else {
      invited.value = await scrApi.createCompanyUser(props.companyId, request)
    }
    editing.value = false
    await load()
    emit('changed')
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) errors.value = { general: errorText(err) }
  } finally {
    busy.value = false
  }
}

async function sendLink() {
  if (!linkFor.value) return
  busy.value = true
  try {
    outcome.value = await scrApi.sendUserLink(props.companyId, linkFor.value.userId)
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

function closeLink() {
  linkFor.value = null
  outcome.value = null
}

onMounted(load)
</script>
