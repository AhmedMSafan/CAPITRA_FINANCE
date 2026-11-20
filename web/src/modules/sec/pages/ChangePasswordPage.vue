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

<style scoped>
.cp-page {
  min-height: 100vh;
  display: grid;
  place-items: center;
  padding: 24px;
  background: radial-gradient(circle at 85% 10%, rgb(232 119 34 / 0.18), transparent 40%), linear-gradient(135deg, #023a42, #014b55 60%, #02343b);
  font-family: Inter, 'Segoe UI', Tahoma, Arial, sans-serif;
}
.cp-card {
  width: min(460px, 100%);
  background: #fff;
  border-radius: 20px;
  padding: 28px 30px 22px;
  box-shadow: 0 24px 60px rgb(0 0 0 / 0.35);
  display: flex;
  flex-direction: column;
  gap: 14px;
}
.cp-head { display: flex; align-items: center; gap: 14px; }
.cp-head h1 { margin: 0; font-size: 21px; color: #0b2e35; }
.cp-head p { margin: 2px 0 0; color: #5d7a80; font-size: 13px; }
.cp-ic { flex: 0 0 48px; height: 48px; border-radius: 14px; display: grid; place-items: center; font-size: 20px; background: #fdf0e5; color: #e87722; }
.cp-lang { margin-inline-start: auto; align-self: flex-start; min-width: 34px; height: 30px; border: 1px solid #d7e3e5; border-radius: 9px; background: #fff; color: #014b55; font-weight: 700; cursor: pointer; }
.cp-note { display: flex; gap: 10px; padding: 10px 12px; border-radius: 12px; background: #e9f6f4; color: #0b4f55; font-size: 13px; line-height: 1.5; }
.cp-note .fa { margin-top: 3px; color: #0f766e; }
.cp-field { display: flex; flex-direction: column; gap: 6px; font-size: 13px; font-weight: 600; color: #24464d; }
.cp-field input { height: 44px; padding: 0 12px; border: 1px solid #d7e3e5; border-radius: 11px; font-size: 15px; background: #f7fafa; outline: none; transition: border-color 0.15s, box-shadow 0.15s; }
.cp-field input:focus { border-color: #0f766e; box-shadow: 0 0 0 3px rgb(15 118 110 / 0.15); background: #fff; }
.cp-show { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #5d7a80; }
.cp-rules { list-style: none; margin: 0; padding: 10px 12px; border-radius: 12px; background: #f7fafa; display: grid; gap: 4px; font-size: 13px; color: #6b8489; }
.cp-rules li { display: flex; align-items: center; gap: 8px; }
.cp-rules li.ok { color: #0f766e; }
.cp-err { color: #c8321f; font-size: 12px; font-weight: 600; }
.cp-msg { margin: 0; }
.cp-submit { height: 48px; border: 0; border-radius: 12px; background: linear-gradient(135deg, #0f766e, #014b55); color: #fff; font-size: 15px; font-weight: 700; display: flex; align-items: center; justify-content: center; gap: 8px; cursor: pointer; }
.cp-submit:disabled { opacity: 0.55; cursor: not-allowed; }
.cp-foot { display: flex; justify-content: center; gap: 18px; }
.cp-link { border: 0; background: none; color: #0f766e; font-weight: 600; cursor: pointer; font-size: 13px; }
.cp-link:hover { text-decoration: underline; }
</style>
