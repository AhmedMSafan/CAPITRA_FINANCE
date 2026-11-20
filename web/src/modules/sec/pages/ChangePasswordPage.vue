<template>
  <!--
    Change password. Forced (?forced=1) after signing in with a temporary password made by an administrator: until the user
    chooses their own, the API allows nothing else. Also open to any signed-in user who wants to change theirs.
  -->
  <main class="cp-page" :dir="rtl ? 'rtl' : 'ltr'" :lang="rtl ? 'ar' : 'en'">
    <form class="cp-card" novalidate @submit.prevent="submit">
      <header class="cp-head">
        <span class="cp-ic" aria-hidden="true"><span class="fa fa-key"></span></span>
        <div>
          <h1>{{ forced ? tr('Choose your own password') : tr('Change password') }}</h1>
          <p>{{ user?.fullName || user?.userName }}</p>
        </div>
        <button type="button" class="cp-lang" @click="switchLanguage(rtl ? 'EN' : 'AR')">{{ rtl ? 'EN' : 'ع' }}</button>
      </header>

      <div v-if="forced" class="cp-note">
        <span class="fa fa-shield" aria-hidden="true"></span>
        <span>{{ tr('You signed in with a temporary password given by your administrator. Choose your own password to continue; nobody else will know it.') }}</span>
      </div>

      <label class="cp-field">
        <span>{{ forced ? tr('Temporary password') : tr('Current password') }}</span>
        <input v-model="current" :type="show ? 'text' : 'password'" autocomplete="current-password" dir="ltr" required>
        <small v-if="errors.currentPassword" class="cp-err">{{ errors.currentPassword }}</small>
      </label>
      <label class="cp-field">
        <span>{{ tr('New password') }}</span>
        <input v-model="next" :type="show ? 'text' : 'password'" autocomplete="new-password" dir="ltr" required>
        <small v-if="errors.newPassword" class="cp-err">{{ errors.newPassword }}</small>
      </label>
      <label class="cp-field">
        <span>{{ tr('Confirm new password') }}</span>
        <input v-model="confirm" :type="show ? 'text' : 'password'" autocomplete="new-password" dir="ltr" required>
      </label>
      <label class="cp-show"><input v-model="show" type="checkbox"> {{ tr('Show passwords') }}</label>

      <ul class="cp-rules" :aria-label="tr('Password rules')">
        <li v-for="r in rules" :key="r.text" :class="{ ok: r.ok }">
          <span class="fa" :class="r.ok ? 'fa-check-circle' : 'fa-circle-o'" aria-hidden="true"></span>{{ tr(r.text) }}
        </li>
      </ul>

      <p v-if="message" class="cp-err cp-msg" role="alert">{{ message }}</p>

      <button type="submit" class="cp-submit" :disabled="busy || !allOk">
        <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-check']" aria-hidden="true"></span>{{ tr('Save my password') }}
      </button>
      <div class="cp-foot">
        <button v-if="!forced" type="button" class="cp-link" @click="router.back()">{{ tr('Cancel') }}</button>
        <button type="button" class="cp-link" @click="signOut">{{ tr('Sign out') }}</button>
      </div>
    </form>
  </main>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import './account-card.css'
import { computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ApiError } from '@/shared/api/http'
import { getUser, isCompanyUser } from '@/shared/auth/session'
import { showSuccess } from '@/shared/feedback/feedback'
import { language, switchLanguage, tr } from '@/shared/layout/language'
import { changePassword, logout } from '../api/auth.api'

const route = useRoute()
const router = useRouter()
const rtl = computed(() => language.value === 'AR')
const forced = computed(() => route.query.forced === '1')
const user = getUser()

const current = ref('')
const next = ref('')
const confirm = ref('')
const show = ref(false)
const busy = ref(false)
const message = ref('')
const errors = ref<Record<string, string>>({})

/** The same rules the server applies (Identity password options), shown as the user types. */
const rules = computed(() => [
  { text: 'At least 8 characters', ok: next.value.length >= 8 },
  { text: 'An upper-case and a lower-case letter', ok: /[A-Z]/.test(next.value) && /[a-z]/.test(next.value) },
  { text: 'A digit', ok: /\d/.test(next.value) },
  { text: 'A symbol such as ! @ # $', ok: /[^A-Za-z0-9]/.test(next.value) },
  { text: 'Different from the current password', ok: !!next.value && next.value !== current.value },
  { text: 'Both new passwords match', ok: !!next.value && next.value === confirm.value }
])
const allOk = computed(() => !!current.value && rules.value.every((r) => r.ok))

async function submit() {
  if (!allOk.value) return
  busy.value = true
  message.value = ''
  errors.value = {}
  try {
    await changePassword(current.value, next.value)
    showSuccess(tr('Your password has been changed.'))
    const redirect = typeof route.query.redirect === 'string' && route.query.redirect.startsWith('/') && !route.query.redirect.startsWith('//')
      ? route.query.redirect : null
    await router.replace(redirect ?? { name: isCompanyUser() ? 'scr-portal' : 'home' })
  } catch (err) {
    const problem = err instanceof ApiError ? err.problem as { errors?: Record<string, string[]> } | null : null
    const fieldErrors = Object.fromEntries(Object.entries(problem?.errors ?? {}).map(([k, v]) => [k.charAt(0).toLowerCase() + k.slice(1), tr(v[0] ?? '')]))
    errors.value = fieldErrors
    if (!Object.keys(fieldErrors).length) message.value = err instanceof Error ? err.message : tr('The password could not be changed.')
  } finally {
    busy.value = false
  }
}

async function signOut() {
  logout()
  await router.replace({ name: 'login' })
}
</script>
