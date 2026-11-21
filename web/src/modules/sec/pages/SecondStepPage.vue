<template>
  <!--
    The second step of signing in, with any authenticator app (Google or Microsoft Authenticator, Authy, 1Password…):
    - SETUP (first time): scan the QR code (or type the key), confirm with the first code, keep the 10 backup codes;
    - CODE (every time after): the app's 6-digit code, or one backup code if the phone is not at hand.
  -->
  <main class="cp-page" :dir="rtl ? 'rtl' : 'ltr'" :lang="rtl ? 'ar' : 'en'">
    <form class="cp-card cp-card--wide" novalidate @submit.prevent="submit">
      <header class="cp-head">
        <span class="cp-ic" aria-hidden="true"><span :class="['fa', step === 'codes' ? 'fa-key' : 'fa-mobile']"></span></span>
        <div>
          <h1>{{ tr(step === 'setup' ? 'Set up your authenticator app' : step === 'codes' ? 'Keep your backup codes' : 'Enter the code from your app') }}</h1>
          <p v-if="pending" dir="auto">{{ pending.fullName || pending.userName }}<span v-if="pending.fullName"> · <bdi dir="ltr">{{ pending.userName }}</bdi></span></p>
        </div>
        <button type="button" class="cp-lang" @click="switchLanguage(rtl ? 'EN' : 'AR')">{{ rtl ? 'EN' : 'ع' }}</button>
      </header>

      <template v-if="step === 'expired'">
        <div class="cp-note cp-note--bad" role="alert"><span class="fa fa-clock-o" aria-hidden="true"></span><span>{{ message || tr('The sign-in took too long or was already used. Sign in again.') }}</span></div>
        <RouterLink class="cp-submit" :to="{ name: 'login' }"><span class="fa fa-sign-in" aria-hidden="true"></span>{{ tr('Sign in again') }}</RouterLink>
      </template>

      <!-- first time: add CAPITRA to the app -->
      <template v-else-if="step === 'setup'">
        <div class="cp-note"><span class="fa fa-shield" aria-hidden="true"></span>
          <span>{{ tr('Company accounts sign in with a password and a code from an authenticator app on your phone. This is set up once.') }}</span></div>
        <ol class="cp-steps">
          <li>{{ tr('Install any authenticator app on your phone: Google Authenticator, Microsoft Authenticator, Authy…') }}</li>
          <li>{{ tr('In the app, add an account and scan this QR code.') }}</li>
          <li>{{ tr('Type the 6-digit code the app shows for CAPITRA.') }}</li>
        </ol>
        <div class="cp-qr">
          <img v-if="setup" :src="setup.qrCode" :alt="tr('QR code to scan with the authenticator app')" width="196" height="196">
          <span v-else class="fa fa-spinner fa-spin" aria-hidden="true"></span>
          <div v-if="setup" class="cp-qr-key">
            <small>{{ tr('Cannot scan? Enter this key in the app (time-based):') }}</small>
            <code dir="ltr">{{ setup.sharedKey }}</code>
            <button type="button" class="cp-link" @click="copy(setup.sharedKey.replace(/ /g, ''))"><span :class="['fa', copied ? 'fa-check' : 'fa-copy']" aria-hidden="true"></span> {{ tr(copied ? 'Copied' : 'Copy the key') }}</button>
          </div>
        </div>
        <label class="cp-field">
          <span>{{ tr('Code from the app') }}</span>
          <input ref="codeBox" v-model="code" class="cp-code" inputmode="numeric" autocomplete="one-time-code" maxlength="7" dir="ltr" placeholder="000000">
        </label>
        <p v-if="message" class="cp-err cp-msg" role="alert">{{ message }}</p>
        <button type="submit" class="cp-submit" :disabled="busy || digits.length !== 6">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-check']" aria-hidden="true"></span>{{ tr('Confirm and sign in') }}
        </button>
      </template>

      <!-- after set-up: the backup codes, shown once -->
      <template v-else-if="step === 'codes'">
        <div class="cp-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
          <span>{{ tr('If you lose your phone, sign in with one of these codes. Each works once. Print them or keep them somewhere safe: they are not shown again.') }}</span></div>
        <ul class="cp-backup" dir="ltr">
          <li v-for="c in recoveryCodes" :key="c"><code>{{ c }}</code></li>
        </ul>
        <div class="cp-row">
          <button type="button" class="cp-link" @click="copy(recoveryCodes.join('\n'))"><span :class="['fa', copied ? 'fa-check' : 'fa-copy']" aria-hidden="true"></span> {{ tr(copied ? 'Copied' : 'Copy the codes') }}</button>
          <button type="button" class="cp-link" @click="printCodes"><span class="fa fa-print" aria-hidden="true"></span> {{ tr('Print') }}</button>
        </div>
        <label class="cp-show"><input v-model="saved" type="checkbox"> {{ tr('I have kept my backup codes') }}</label>
        <button type="button" class="cp-submit" :disabled="!saved" @click="goOn"><span class="fa fa-arrow-right" aria-hidden="true"></span>{{ tr('Continue') }}</button>
      </template>

      <!-- every sign-in: the code, or a backup code -->
      <template v-else>
        <label v-if="!useBackup" class="cp-field">
          <span>{{ tr('6-digit code') }}</span>
          <input ref="codeBox" v-model="code" class="cp-code" inputmode="numeric" autocomplete="one-time-code" maxlength="7" dir="ltr" placeholder="000000">
          <small class="cp-hint">{{ tr('Open your authenticator app and type the code shown for CAPITRA.') }}</small>
        </label>
        <label v-else class="cp-field">
          <span>{{ tr('Backup code') }}</span>
          <input ref="codeBox" v-model="backup" class="cp-code cp-code--backup" autocomplete="off" dir="ltr" placeholder="xxxxx-xxxxx">
          <small class="cp-hint">{{ tr('One of the codes you kept when you set up the app. Each works once.') }}</small>
        </label>
        <p v-if="message" class="cp-err cp-msg" role="alert">{{ message }}</p>
        <button type="submit" class="cp-submit" :disabled="busy || (useBackup ? !backup.trim() : digits.length !== 6)">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-sign-in']" aria-hidden="true"></span>{{ tr('Sign in') }}
        </button>
        <div class="cp-foot cp-alt">
          <button type="button" class="cp-link" @click="toggleBackup">{{ tr(useBackup ? 'Use the code from the app' : 'No phone? Use a backup code') }}</button>
          <small class="cp-hint">{{ tr('Lost your phone and your backup codes? Ask your company administrator to reset your two-factor.') }}</small>
        </div>
      </template>

      <div v-if="step !== 'codes' && step !== 'expired'" class="cp-foot"><RouterLink class="cp-link" :to="{ name: 'login' }">{{ tr('Back to sign-in') }}</RouterLink></div>
    </form>
  </main>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import './account-card.css'
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { language, switchLanguage, tr } from '@/shared/layout/language'
import { authenticatorSetup, clearSecondStep, enableAuthenticator, pendingSecondStep, verifySecondStep, type AuthenticatorSetup } from '../api/auth.api'

const route = useRoute()
const router = useRouter()
const rtl = computed(() => language.value === 'AR')
const pending = pendingSecondStep()
const step = ref<'setup' | 'code' | 'codes' | 'expired'>(pending ? (pending.kind === 'SETUP' ? 'setup' : 'code') : 'expired')
const setup = ref<AuthenticatorSetup | null>(null)
const code = ref('')
const backup = ref('')
const useBackup = ref(false)
const busy = ref(false)
const message = ref('')
const copied = ref(false)
const saved = ref(false)
const recoveryCodes = ref<string[]>([])
const codeBox = ref<HTMLInputElement | null>(null)
const digits = computed(() => code.value.replace(/\D/g, ''))
let timer: ReturnType<typeof setTimeout> | undefined

const redirect = () => {
  const r = route.query.redirect
  return typeof r === 'string' && r.startsWith('/') && !r.startsWith('//') ? r : '/'
}

function expire(text = '') {
  clearSecondStep()
  step.value = 'expired'
  message.value = text
}

onMounted(async () => {
  if (!pending) return
  // the challenge lasts a few minutes: after that, sign in again
  timer = setTimeout(() => { if (step.value === 'setup' || step.value === 'code') expire() }, pending.expiresInSeconds * 1000)
  if (step.value === 'setup') {
    try {
      setup.value = await authenticatorSetup(pending.challenge)
    } catch (err) {
      expire(err instanceof Error ? tr(err.message) : '')
      return
    }
  }
  await nextTick()
  codeBox.value?.focus()
})
onBeforeUnmount(() => clearTimeout(timer))

async function submit() {
  if (!pending || busy.value) return
  busy.value = true
  message.value = ''
  try {
    if (step.value === 'setup') {
      const r = await enableAuthenticator(pending.challenge, digits.value)
      recoveryCodes.value = r.recoveryCodes ?? []
      step.value = 'codes'
    } else if (step.value === 'code') {
      const r = await verifySecondStep(pending.challenge, useBackup.value ? null : digits.value, useBackup.value ? backup.value.trim() : null)
      if (useBackup.value) sessionStorage.setItem('capitra.backupCodesLeft', String(r.recoveryCodesLeft))
      await router.replace(redirect())
    }
  } catch (err) {
    const text = err instanceof Error ? tr(err.message) : tr('The code could not be checked.')
    if (/too long|already used|locked/i.test(err instanceof Error ? err.message : '')) expire(text)
    else {
      message.value = text
      code.value = ''
      backup.value = ''
      codeBox.value?.focus()
    }
  } finally {
    busy.value = false
  }
}

function toggleBackup() {
  useBackup.value = !useBackup.value
  message.value = ''
  void nextTick(() => codeBox.value?.focus())
}

async function copy(text: string) {
  try {
    await navigator.clipboard.writeText(text)
    copied.value = true
    setTimeout(() => (copied.value = false), 2000)
  } catch {
    copied.value = false
  }
}

function printCodes() {
  const w = window.open('', '_blank', 'width=480,height=640')
  if (!w) return
  const list = recoveryCodes.value.map((c) => `<li>${c}</li>`).join('')
  w.document.write(`<title>CAPITRA backup codes</title><body style="font-family:Consolas,monospace;padding:24px"><h2>CAPITRA · ${pending?.userName ?? ''}</h2><p>Backup codes: each works once.</p><ol style="font-size:18px;line-height:2">${list}</ol></body>`)
  w.document.close()
  w.print()
}

async function goOn() {
  await router.replace(redirect())
}
</script>
