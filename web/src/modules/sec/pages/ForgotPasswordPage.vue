<template>
  <!--
    Forgot password: e-mails a link to choose a new one. The answer never says whether the account exists, so the page cannot be
    used to find user names.
  -->
  <main class="cp-page" :dir="rtl ? 'rtl' : 'ltr'" :lang="rtl ? 'ar' : 'en'">
    <form class="cp-card" novalidate @submit.prevent="submit">
      <header class="cp-head">
        <span class="cp-ic" aria-hidden="true"><span class="fa fa-envelope-o"></span></span>
        <div>
          <h1>{{ tr('Forgot password') }}</h1>
          <p>{{ tr('We e-mail you a link to choose a new password.') }}</p>
        </div>
        <button type="button" class="cp-lang" @click="switchLanguage(rtl ? 'EN' : 'AR')">{{ rtl ? 'EN' : 'ع' }}</button>
      </header>

      <template v-if="sent">
        <div class="cp-note" role="status"><span class="fa fa-check-circle" aria-hidden="true"></span>
          <span>{{ tr('If the account exists and has an e-mail address, a link has been sent to it. It works once and expires in 48 hours. Check the spam folder too.') }}</span></div>
        <RouterLink class="cp-submit" :to="{ name: 'login' }"><span class="fa fa-sign-in" aria-hidden="true"></span>{{ tr('Back to sign-in') }}</RouterLink>
      </template>
      <template v-else>
        <label class="cp-field">
          <span>{{ tr('User name or e-mail') }}</span>
          <input v-model="key" autocomplete="username" dir="ltr" required>
        </label>
        <p v-if="message" class="cp-err cp-msg" role="alert">{{ message }}</p>
        <button type="submit" class="cp-submit" :disabled="busy || !key.trim()">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-paper-plane']" aria-hidden="true"></span>{{ tr('Send me the link') }}
        </button>
        <div class="cp-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
          <span>{{ tr('No e-mail on your account? Ask your company administrator to send you the link.') }}</span></div>
        <div class="cp-foot"><RouterLink class="cp-link" :to="{ name: 'login' }">{{ tr('Back to sign-in') }}</RouterLink></div>
      </template>
    </form>
  </main>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import './account-card.css'
import { computed, ref } from 'vue'
import { useRoute } from 'vue-router'
import { language, switchLanguage, tr } from '@/shared/layout/language'
import { forgotPassword } from '../api/auth.api'

const route = useRoute()
const rtl = computed(() => language.value === 'AR')
const key = ref(typeof route.query.user === 'string' ? route.query.user : '')
const busy = ref(false)
const sent = ref(false)
const message = ref('')

async function submit() {
  if (!key.value.trim()) return
  busy.value = true
  message.value = ''
  try {
    await forgotPassword(key.value.trim())
    sent.value = true
  } catch (err) {
    message.value = err instanceof Error ? err.message : tr('The link could not be sent. Try again later.')
  } finally {
    busy.value = false
  }
}
</script>
