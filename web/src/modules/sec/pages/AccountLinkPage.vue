<template>
  <!--
    Opened from an e-mailed link: /activate (a new account chooses its first password) or /reset-password. The link works once,
    for 48 hours; a newer link voids it. No password is ever sent by e-mail.
  -->
  <main class="cp-page" :dir="rtl ? 'rtl' : 'ltr'" :lang="rtl ? 'ar' : 'en'">
    <form class="cp-card" novalidate @submit.prevent="submit">
      <header class="cp-head">
        <span class="cp-ic" aria-hidden="true"><span :class="['fa', activating ? 'fa-user-plus' : 'fa-key']"></span></span>
        <div>
          <h1>{{ tr(activating ? 'Activate your account' : 'Choose a new password') }}</h1>
          <p v-if="info" dir="auto">{{ info.fullName || info.userName }}</p>
        </div>
        <button type="button" class="cp-lang" @click="switchLanguage(rtl ? 'EN' : 'AR')">{{ rtl ? 'EN' : 'ع' }}</button>
      </header>

      <div v-if="state === 'checking'" class="cp-note"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span><span>{{ tr('Checking the link…') }}</span></div>

      <template v-else-if="state === 'invalid'">
        <div class="cp-note cp-note--bad" role="alert"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span><span>{{ message }}</span></div>
        <RouterLink class="cp-submit" :to="{ name: 'forgot-password' }"><span class="fa fa-envelope-o" aria-hidden="true"></span>{{ tr('Send me a new link') }}</RouterLink>
        <div class="cp-foot"><RouterLink class="cp-link" :to="{ name: 'login' }">{{ tr('Back to sign-in') }}</RouterLink></div>
      </template>

      <template v-else-if="state === 'done'">
        <div class="cp-note" role="status"><span class="fa fa-check-circle" aria-hidden="true"></span>
          <span>{{ tr(activating ? 'Your account is active. Sign in with your user name and the password you chose.' : 'Your password has been changed. Sign in with the new password.') }}</span></div>
        <div v-if="info" class="cp-username"><span>{{ tr('Your user name, to sign in') }}</span><b dir="ltr">{{ info.userName }}</b></div>
        <RouterLink class="cp-submit" :to="{ name: 'login', query: info ? { user: info.userName } : {} }"><span class="fa fa-sign-in" aria-hidden="true"></span>{{ tr('Sign in') }}</RouterLink>
      </template>

      <template v-else>
        <div v-if="info" class="cp-username"><span>{{ tr('Your user name, to sign in') }}</span><b dir="ltr">{{ info.userName }}</b></div>
        <div class="cp-note"><span class="fa fa-shield" aria-hidden="true"></span>
          <span>{{ tr('Choose your own password. Nobody else, the administrators included, will know it.') }}</span></div>
        <label class="cp-field">
          <span>{{ tr('New password') }}</span>
          <input v-model="next" :type="show ? 'text' : 'password'" autocomplete="new-password" dir="ltr" required>
          <small v-if="fieldError" class="cp-err">{{ fieldError }}</small>
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
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-check']" aria-hidden="true"></span>{{ tr(activating ? 'Activate my account' : 'Save my password') }}
        </button>
        <div class="cp-foot"><RouterLink class="cp-link" :to="{ name: 'login' }">{{ tr('Back to sign-in') }}</RouterLink></div>
      </template>
    </form>
  </main>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import './account-card.css'
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { ApiError } from '@/shared/api/http'
import { language, switchLanguage, tr } from '@/shared/layout/language'
import { checkAccountLink, setPasswordFromLink, type AccountLinkInfo } from '../api/auth.api'

const route = useRoute()
const rtl = computed(() => language.value === 'AR')
const userId = typeof route.query.user === 'string' ? route.query.user : ''
const token = typeof route.query.token === 'string' ? route.query.token : ''

const state = ref<'checking' | 'invalid' | 'ready' | 'done'>('checking')
const info = ref<AccountLinkInfo | null>(null)
/** The server knows: an account without a password is being activated, whichever address the link used. */
const activating = computed(() => info.value?.activating ?? route.name === 'activate')
const next = ref('')
const confirm = ref('')
const show = ref(false)
const busy = ref(false)
const message = ref('')
const fieldError = ref('')

const rules = computed(() => [
  { text: 'At least 8 characters', ok: next.value.length >= 8 },
  { text: 'An upper-case and a lower-case letter', ok: /[A-Z]/.test(next.value) && /[a-z]/.test(next.value) },
  { text: 'A digit', ok: /\d/.test(next.value) },
  { text: 'A symbol such as ! @ # $', ok: /[^A-Za-z0-9]/.test(next.value) },
  { text: 'Both new passwords match', ok: !!next.value && next.value === confirm.value }
])
const allOk = computed(() => rules.value.every((r) => r.ok))

function problemOf(err: unknown) {
  const problem = err instanceof ApiError ? err.problem as { errors?: Record<string, string[]> } | null : null
  return problem?.errors ?? {}
}

onMounted(async () => {
  if (!userId || !token) {
    state.value = 'invalid'
    message.value = tr('This link is incomplete. Open it from the e-mail again, or ask for a new one.')
    return
  }
  try {
    info.value = await checkAccountLink(userId, token)
    state.value = 'ready'
  } catch (err) {
    state.value = 'invalid'
    message.value = tr(problemOf(err).token?.[0] ?? (err instanceof Error ? err.message : 'This link is not valid any more.'))
  }
})

async function submit() {
  if (!allOk.value) return
  busy.value = true
  message.value = ''
  fieldError.value = ''
  try {
    await setPasswordFromLink(userId, token, next.value)
    state.value = 'done'
  } catch (err) {
    const errors = problemOf(err)
    if (errors.token) {
      state.value = 'invalid'
      message.value = tr(errors.token[0] ?? '')
    } else if (errors.newPassword) {
      fieldError.value = tr(errors.newPassword[0] ?? '')
    } else {
      message.value = err instanceof Error ? err.message : tr('The password could not be saved.')
    }
  } finally {
    busy.value = false
  }
}
</script>
