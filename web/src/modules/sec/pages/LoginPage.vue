<script setup lang="ts">
// Sign-in page, rebuilt from APEX App 100 page 9999 (LOGIN): each application's sign-in scene
// plays in turn behind the card, with a rail to pick one, show-password, a Caps Lock warning
// and "Remember username".
import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { login } from '@/modules/sec/api/auth.api'

const SCENES = [
  { key: 'portfolio', label: 'Portfolio', title: 'Portfolio Acquisition' },
  { key: 'mortgage', label: 'Mortgage', title: 'Personal Mortgage' },
  { key: 'workflow', label: 'Workflow', title: 'Workflow Engine' },
  { key: 'ledger', label: 'Ledger', title: 'General Ledger' },
  { key: 'wallet', label: 'Wallet', title: 'Cheques Wallet' }
] as const
const REMEMBER_KEY = 'capitra.login.userName'

const router = useRouter()
const route = useRoute()

const userName = ref(readRemembered())
const password = ref('')
const remember = ref(userName.value !== '')
const showPassword = ref(false)
const capsLock = ref(false)
const errorMessage = ref('')
const loading = ref(false)

// ---------- rotating application scenes ----------
const reduceMotion = window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ?? false
const narrow = window.matchMedia?.('(max-width: 900px)').matches ?? false
const sceneMs = reduceMotion ? 6000 : 3000
const current = ref(-1)
const loaded = ref<boolean[]>(SCENES.map(() => false))
const sources = ref<string[]>(SCENES.map(() => ''))
let swapTimer: number | undefined
let preloadTimer: number | undefined
let halted = false

const stageStyle = computed(() => ({ '--fm-ms': `${sceneMs}ms` }))

function sceneUrl(k: number): string {
  return `${import.meta.env.BASE_URL}login-scenes/login-scene-${SCENES[k]!.key}.html`
}

function prepare(k: number): void {
  if (!sources.value[k]) sources.value[k] = sceneUrl(k)
}

function onSceneLoad(k: number): void {
  loaded.value[k] = true
  if (pending === k) show(k)
}

let pending = -1
function go(k: number): void {
  if (halted && current.value >= 0) return
  window.clearTimeout(swapTimer)
  window.clearTimeout(preloadTimer)
  if (loaded.value[k]) show(k)
  else {
    pending = k
    prepare(k)
  }
}

function show(k: number): void {
  pending = -1
  const previous = current.value
  current.value = k
  const next = (k + 1) % SCENES.length
  if (previous >= 0 && previous !== k && previous !== next) {
    // Stop the hidden scene once it has faded out.
    window.setTimeout(() => {
      if (current.value !== previous) {
        sources.value[previous] = ''
        loaded.value[previous] = false
      }
    }, 900)
  }
  if (halted) return
  preloadTimer = window.setTimeout(() => prepare(next), Math.max(0, sceneMs - 1200))
  swapTimer = window.setTimeout(() => go(next), sceneMs)
}

function pick(k: number): void {
  halted = false
  go(k)
}

/** Sign-in started (typing, autofill, Windows Hello): the scene stays still so the browser can fill the fields. */
function halt(): void {
  halted = true
  window.clearTimeout(swapTimer)
  window.clearTimeout(preloadTimer)
}

function onVisibility(): void {
  if (document.hidden) {
    window.clearTimeout(swapTimer)
    window.clearTimeout(preloadTimer)
  } else if (current.value >= 0 && !halted) {
    go((current.value + 1) % SCENES.length)
  }
}

onMounted(() => {
  if (narrow) return
  document.addEventListener('visibilitychange', onVisibility)
  window.addEventListener('blur', halt)
  go(0)
})

onUnmounted(() => {
  window.clearTimeout(swapTimer)
  window.clearTimeout(preloadTimer)
  document.removeEventListener('visibilitychange', onVisibility)
  window.removeEventListener('blur', halt)
})

// ---------- sign-in ----------
function readRemembered(): string {
  try {
    return localStorage.getItem(REMEMBER_KEY) ?? ''
  } catch {
    return ''
  }
}

function storeRemembered(): void {
  try {
    if (remember.value) localStorage.setItem(REMEMBER_KEY, userName.value.trim())
    else localStorage.removeItem(REMEMBER_KEY)
  } catch {
    // Storage blocked: the username simply is not remembered.
  }
}

function checkCaps(event: KeyboardEvent): void {
  if (typeof event.getModifierState === 'function') capsLock.value = event.getModifierState('CapsLock')
}

async function submitLogin(): Promise<void> {
  halt()
  errorMessage.value = ''
  if (!userName.value.trim() || !password.value) {
    errorMessage.value = 'Enter your username and password.'
    return
  }

  try {
    loading.value = true
    await login(userName.value.trim(), password.value)
    storeRemembered()
    const redirect = typeof route.query.redirect === 'string' && route.query.redirect.startsWith('/') && !route.query.redirect.startsWith('//')
      ? route.query.redirect
      : '/'
    await router.push(redirect)
  } catch (error) {
    errorMessage.value = error instanceof Error ? error.message : 'Sign-in failed.'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <main class="cl-page" :style="stageStyle">
    <div class="fm-stage" aria-hidden="true">
      <iframe
        v-for="(scene, k) in SCENES"
        :key="scene.key"
        tabindex="-1"
        :title="scene.title"
        :src="sources[k] || undefined"
        :class="{ on: current === k }"
        @load="sources[k] && onSceneLoad(k)"
      ></iframe>
    </div>

    <nav class="fm-rail" aria-label="Capitra applications">
      <button
        v-for="(scene, k) in SCENES"
        :key="scene.key"
        type="button"
        :title="scene.title"
        :class="{ on: current === k, done: current > k }"
        :aria-current="current === k ? 'true' : 'false'"
        @click="pick(k)"
      >
        <span>{{ scene.label }}</span><i><b :key="current === k ? `on-${current}` : 'off'"></b></i>
      </button>
    </nav>

    <div class="cl-side">
      <form class="cl-card" novalidate @submit.prevent="submitLogin" @focusin="halt" @pointerdown="halt">
        <header class="cl-head">
          <span class="cl-mob" aria-hidden="true"></span>
          <span class="cl-mark" aria-hidden="true"></span>
          <h1 class="cl-hello">Welcome to Capitra</h1>
          <p class="cl-sub">Sign in once to reach every Capitra application.</p>
        </header>

        <div v-if="errorMessage" class="cl-error" role="alert">
          <i class="pi pi-exclamation-circle"></i><span>{{ errorMessage }}</span>
        </div>

        <div class="cl-field">
          <label for="cl-user">Username</label>
          <div class="cl-input cl-input--user">
            <input
              id="cl-user"
              v-model="userName"
              type="text"
              autocomplete="username"
              placeholder="Enter your username"
              autocapitalize="off"
              spellcheck="false"
            />
          </div>
        </div>

        <div class="cl-field">
          <label for="cl-password">Password</label>
          <div class="cl-input cl-input--password">
            <input
              id="cl-password"
              v-model="password"
              :type="showPassword ? 'text' : 'password'"
              autocomplete="current-password"
              placeholder="Enter your password"
              @keydown="checkCaps"
              @keyup="checkCaps"
              @blur="capsLock = false"
            />
            <button
              type="button"
              class="cl-eye"
              :aria-label="showPassword ? 'Hide password' : 'Show password'"
              :aria-pressed="showPassword"
              @click="showPassword = !showPassword"
            >
              <svg v-if="!showPassword" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z" /><circle cx="12" cy="12" r="3" /></svg>
              <svg v-else width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17.9 17.9A10.4 10.4 0 0 1 12 19c-6.5 0-10-7-10-7a18.5 18.5 0 0 1 5.1-5.9" /><path d="M9.9 4.2A9.6 9.6 0 0 1 12 4c6.5 0 10 7 10 7a18.6 18.6 0 0 1-2.2 3.2" /><path d="m2 2 20 20" /></svg>
            </button>
          </div>
        </div>
        <div v-if="capsLock" class="cl-caps" role="status">Caps Lock is on</div>

        <label class="cl-remember" for="cl-remember">
          <input id="cl-remember" v-model="remember" type="checkbox" />
          <span>Remember username</span>
        </label>

        <button type="submit" class="cl-button" :disabled="loading">
          <span>{{ loading ? 'Signing in…' : 'Sign in' }}</span>
          <i :class="loading ? 'pi pi-spin pi-spinner' : 'pi pi-arrow-right'"></i>
        </button>

        <div class="cl-note">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 3l8 3v6c0 5-3.5 8-8 9-4.5-1-8-4-8-9V6z" /></svg>
          <span>Authorised users only. One sign-in opens every Capitra application you are allowed to use.</span>
        </div>
      </form>
    </div>

    <div class="cl-foot"><span>Need access? Contact your system administrator.</span><span>Release 1.0</span></div>
  </main>
</template>

<style scoped>
/* Colours: Capitra Navy (theme option A). */
.cl-page {
  --cl-scene: #06101f;
  --cl-ink: #14213a;
  --cl-muted: #5b6880;
  --cl-label: #2a3b57;
  --cl-line: #d3dbe6;
  --cl-field: #f5f7fb;
  --cl-primary: #1f5fd1;
  --cl-primary-dark: #0e3a86;
  --cl-accent: #f5a524;
  position: relative;
  min-height: 100vh;
  overflow: hidden;
  background: var(--cl-scene);
  color: var(--cl-ink);
  font-family: Inter, 'Segoe UI', Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
}

/* ---------- application scenes ---------- */
.fm-stage {
  position: fixed;
  inset: 0;
  z-index: 0;
  overflow: hidden;
  pointer-events: none;
  background: radial-gradient(120% 120% at 0% 0%, #16427a 0%, #0b1e3a 50%, #06101f 100%);
}
.fm-stage iframe {
  position: absolute;
  inset: 0;
  width: 100%;
  height: 100%;
  border: 0;
  opacity: 0;
  transition: opacity 0.8s ease;
  pointer-events: none;
  background: transparent;
}
.fm-stage iframe.on { opacity: 1; }

.fm-rail {
  position: fixed;
  top: 22px;
  inset-inline-end: 32px;
  z-index: 3;
  display: flex;
  gap: 4px;
  padding: 5px;
  border-radius: 14px;
  background: rgba(6, 16, 31, 0.55);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.14);
  box-shadow: 0 10px 30px -18px rgba(0, 0, 0, 0.6);
  animation: fmRise 0.7s cubic-bezier(0.2, 0.8, 0.2, 1) 0.2s both;
}
.fm-rail button {
  appearance: none;
  border: 0;
  margin: 0;
  background: transparent;
  color: rgba(255, 255, 255, 0.66);
  font: 600 12px/1.2 Inter, 'Segoe UI', Arial, sans-serif;
  padding: 7px 10px 8px;
  border-radius: 10px;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  align-items: stretch;
  gap: 6px;
  min-width: 84px;
  text-align: start;
  transition: background 0.2s, color 0.2s;
}
.fm-rail button:hover { color: #fff; background: rgba(255, 255, 255, 0.07); }
.fm-rail button.on { color: #fff; background: rgba(255, 255, 255, 0.11); }
.fm-rail button:focus-visible { outline: 2px solid var(--cl-accent); outline-offset: 1px; }
.fm-rail i { display: block; height: 3px; border-radius: 3px; background: rgba(255, 255, 255, 0.18); overflow: hidden; }
.fm-rail b { display: block; height: 100%; width: 0; border-radius: inherit; background: linear-gradient(90deg, #f5a524, #e8950f); }
.fm-rail button.on b { animation: fmFill var(--fm-ms, 3000ms) linear forwards; }
.fm-rail button.done b { width: 100%; background: rgba(255, 255, 255, 0.45); }
@keyframes fmFill { from { width: 0; } to { width: 100%; } }

/* ---------- sign-in side ---------- */
.cl-side {
  position: relative;
  z-index: 2;
  min-height: 100vh;
  margin-inline-start: 58vw;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 72px clamp(20px, 3vw, 48px) 52px;
}
.cl-card {
  position: relative;
  width: 100%;
  max-width: 420px;
  overflow: hidden;
  background: rgba(255, 255, 255, 0.96);
  border: 1px solid rgba(255, 255, 255, 0.7);
  border-radius: 24px;
  padding: 30px 34px 26px;
  backdrop-filter: blur(20px);
  box-shadow: 0 0 0 1px rgba(255, 255, 255, 0.08), 0 40px 90px -20px rgba(0, 0, 0, 0.6), 0 12px 30px -10px rgba(0, 0, 0, 0.35);
  animation: clCard 0.7s 0.15s cubic-bezier(0.2, 0.8, 0.2, 1) both;
}
@keyframes clCard { from { opacity: 0; transform: translateY(24px) scale(0.98); } to { opacity: 1; transform: none; } }

.cl-head { margin: 0 0 22px; }
.cl-head > * { animation: fmRise 0.6s cubic-bezier(0.2, 0.8, 0.2, 1) both; }
.cl-head > :nth-child(3) { animation-delay: 0.3s; }
.cl-head > :nth-child(4) { animation-delay: 0.38s; }
.cl-mob { display: none; }
.cl-mark {
  display: grid;
  place-items: center;
  width: 46px;
  height: 46px;
  border-radius: 14px;
  margin-bottom: 18px;
  position: relative;
  background: linear-gradient(135deg, #2a6fe0, #0e3a86);
  box-shadow: 0 12px 24px -6px rgba(31, 95, 209, 0.55);
  animation: fmMark 3.2s ease-in-out 1.2s infinite !important;
}
.cl-mark::before {
  content: '';
  width: 21px;
  height: 21px;
  background: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='white' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Crect x='3' y='3' width='7' height='7' rx='1.5'/%3E%3Crect x='14' y='3' width='7' height='7' rx='1.5'/%3E%3Crect x='3' y='14' width='7' height='7' rx='1.5'/%3E%3Crect x='14' y='14' width='7' height='7' rx='1.5'/%3E%3C/svg%3E") center/contain no-repeat;
}
.cl-mark::after { content: ''; position: absolute; inset: -5px; border-radius: 18px; border: 1px solid rgba(31, 95, 209, 0.18); }
.cl-hello { display: block; margin: 0; font-size: 27px; font-weight: 800; letter-spacing: -0.02em; color: var(--cl-ink); line-height: 1.2; }
.cl-sub { display: block; margin: 6px 0 0; font-size: 14.5px; color: var(--cl-muted); line-height: 1.45; }

.cl-error {
  display: flex;
  align-items: center;
  gap: 8px;
  margin: 0 0 14px;
  padding: 10px 12px;
  border-radius: 12px;
  border: 1px solid #f2c4c4;
  background: #fff3f3;
  color: #b42318;
  font-size: 13px;
}

.cl-field { margin: 0 0 14px; animation: fmRise 0.6s cubic-bezier(0.2, 0.8, 0.2, 1) 0.55s both; }
.cl-field + .cl-field { animation-delay: 0.63s; }
.cl-field label { display: block; padding: 0 0 6px; font-size: 13px; font-weight: 600; color: var(--cl-label); line-height: 1.2; }
.cl-input {
  display: flex;
  align-items: center;
  height: 48px;
  border: 1px solid var(--cl-line);
  border-radius: 13px;
  background: var(--cl-field);
  overflow: hidden;
  transition: border-color 0.15s, box-shadow 0.15s, background 0.15s;
}
.cl-input:hover { border-color: #b5c3d8; }
.cl-input:focus-within { border-color: var(--cl-primary); background: #fff; box-shadow: 0 0 0 4px rgba(31, 95, 209, 0.13); }
.cl-input::before {
  content: '';
  flex: 0 0 18px;
  width: 18px;
  height: 18px;
  margin-inline-start: 15px;
  background: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%235B6880' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Ccircle cx='12' cy='8' r='4'/%3E%3Cpath d='M4 21a8 8 0 0 1 16 0'/%3E%3C/svg%3E") center/18px 18px no-repeat;
}
.cl-input--password::before {
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 24 24' fill='none' stroke='%235B6880' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Crect x='4' y='11' width='16' height='10' rx='2'/%3E%3Cpath d='M8 11V7a4 4 0 0 1 8 0v4'/%3E%3C/svg%3E");
}
.cl-input input {
  flex: 1 1 auto;
  min-width: 0;
  height: 46px;
  margin: 0;
  border: 0;
  outline: 0;
  font: inherit;
  font-size: 15px;
  color: var(--cl-ink);
  padding: 0 12px 0 11px;
  background: transparent;
}
.cl-input input::placeholder { color: #8e9ab0; }
.cl-input input:-webkit-autofill,
.cl-input input:-webkit-autofill:focus {
  -webkit-box-shadow: 0 0 0 100px var(--cl-field) inset;
  -webkit-text-fill-color: var(--cl-ink);
  caret-color: var(--cl-ink);
}
.cl-eye {
  flex: 0 0 auto;
  width: 44px;
  height: 46px;
  display: grid;
  place-items: center;
  border: 0;
  background: transparent;
  color: var(--cl-muted);
  cursor: pointer;
}
.cl-eye:hover { color: var(--cl-primary); }
.cl-eye:focus-visible { outline: 2px solid rgba(31, 95, 209, 0.4); outline-offset: -4px; border-radius: 10px; }
.cl-caps { margin: -6px 0 12px; font-size: 12.5px; font-weight: 600; color: #a5650a; }

.cl-remember {
  display: flex;
  align-items: center;
  gap: 10px;
  margin: 2px 0 4px;
  font-size: 14px;
  font-weight: 500;
  color: var(--cl-label);
  cursor: pointer;
  animation: fmRise 0.6s cubic-bezier(0.2, 0.8, 0.2, 1) 0.71s both;
}
.cl-remember input {
  appearance: none;
  width: 38px;
  height: 22px;
  flex: 0 0 38px;
  margin: 0;
  border-radius: 99px;
  cursor: pointer;
  background: radial-gradient(circle at 11px 50%, #fff 0 7.5px, transparent 8.5px), #c9d2e0;
  transition: background 0.15s;
}
.cl-remember input:checked { background: radial-gradient(circle at 27px 50%, #fff 0 7.5px, transparent 8.5px), var(--cl-primary); }
.cl-remember input:focus-visible { outline: 3px solid rgba(31, 95, 209, 0.25); outline-offset: 2px; }

.cl-button {
  position: relative;
  overflow: hidden;
  width: 100%;
  height: 46px;
  margin: 20px 0 0;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  border: 0;
  border-radius: 13px;
  font: inherit;
  font-size: 15.5px;
  font-weight: 700;
  color: #fff;
  letter-spacing: 0.01em;
  cursor: pointer;
  background: linear-gradient(135deg, #2a6fe0 0%, #1f5fd1 55%, #174db0 100%);
  box-shadow: 0 14px 28px -8px rgba(31, 95, 209, 0.6), inset 0 1px 0 rgba(255, 255, 255, 0.25);
  transition: transform 0.15s, box-shadow 0.15s, filter 0.15s;
  animation: fmRise 0.6s cubic-bezier(0.2, 0.8, 0.2, 1) 0.8s both;
}
.cl-button::after {
  content: '';
  position: absolute;
  top: 0;
  bottom: 0;
  width: 60%;
  left: -80%;
  background: linear-gradient(100deg, transparent, rgba(255, 255, 255, 0.35), transparent);
  transform: skewX(-20deg);
  animation: clShine 5.5s 1.2s ease-in-out infinite;
}
@keyframes clShine { 0%, 70% { left: -80%; } 100% { left: 130%; } }
.cl-button:hover:not(:disabled) { transform: translateY(-1px); filter: brightness(1.05); box-shadow: 0 18px 34px -8px rgba(31, 95, 209, 0.7); }
.cl-button:focus-visible { outline: 3px solid rgba(31, 95, 209, 0.35); outline-offset: 2px; }
.cl-button:disabled { cursor: progress; filter: saturate(0.8); }
.cl-button i { transition: transform 0.2s; }
.cl-button:hover i.pi-arrow-right { transform: translateX(3px); }

.cl-note {
  display: flex;
  align-items: center;
  gap: 10px;
  margin: 20px -34px -26px;
  padding: 13px 34px;
  border-top: 1px solid #e9eef5;
  background: #f6f8fc;
  color: var(--cl-muted);
  font-size: 12.5px;
  line-height: 1.4;
}
.cl-note svg { color: var(--cl-primary); flex: 0 0 auto; }

.cl-foot {
  position: fixed;
  bottom: 20px;
  inset-inline-start: 58vw;
  inset-inline-end: 32px;
  z-index: 2;
  display: flex;
  justify-content: center;
  gap: 16px;
  font-size: 12.5px;
  color: rgba(255, 255, 255, 0.6);
}
.cl-foot span + span::before { content: '\00B7'; margin-inline-end: 16px; }

@keyframes fmRise { from { opacity: 0; transform: translateY(12px); } to { opacity: 1; transform: none; } }
@keyframes fmMark { 0%, 100% { transform: none; } 8% { transform: rotate(-8deg) scale(1.06); } 16% { transform: rotate(6deg) scale(1.06); } 24% { transform: none; } }

@media (max-height: 700px) { .cl-foot, .cl-mark { display: none; } }
@media (max-height: 620px) { .cl-note { display: none; } .cl-side { padding-top: 60px; } }
@media (max-width: 1100px) { .fm-rail button { min-width: 64px; padding: 6px 8px 7px; } .fm-rail span { font-size: 11px; } }
@media (max-width: 900px) {
  .fm-stage iframe, .fm-rail, .cl-foot, .cl-mark { display: none; }
  .fm-stage { background: linear-gradient(180deg, #0b1e3a, #06101f); }
  .cl-side { margin-inline-start: 0; justify-content: flex-start; padding: 56px 16px 40px; }
  .cl-card { padding: 24px 22px 22px; }
  .cl-note { margin: 18px -22px -22px; padding: 12px 22px; }
  .cl-mob {
    display: block;
    height: 52px;
    border-radius: 12px;
    margin-bottom: 20px;
    background: url('/login-scenes/capitra-logo.svg') 14px center/auto 28px no-repeat, linear-gradient(135deg, #0a1a33, #0e3a86);
  }
}
@media (prefers-reduced-motion: reduce) {
  .fm-stage iframe { transition: none; }
  .fm-rail button.on b { animation: none; width: 100%; }
  .cl-card, .cl-head > *, .cl-field, .cl-remember, .cl-button, .cl-button::after, .cl-mark, .fm-rail { animation: none !important; }
}
</style>
