<script setup lang="ts">
// Renders the app-wide pop-up (success, warning, error) and the animated progress screen. Mounted once in App.vue.
import { computed, nextTick, onUnmounted, ref, watch } from 'vue'
import { closePopup, feedback, type BusySpec } from './feedback'

// ---------- pop-up ----------
const SUCCESS_MS = 4000
const popup = computed(() => feedback.popups[0] ?? null)
const okButton = ref<HTMLButtonElement | null>(null)
let autoClose: number | undefined

watch(popup, async p => {
  window.clearTimeout(autoClose)
  if (!p) return
  await nextTick()
  okButton.value?.focus()
  if (p.kind === 'success') autoClose = window.setTimeout(() => closePopup(p.id), SUCCESS_MS)
})

function close(): void {
  if (popup.value) closePopup(popup.value.id)
}

function onKey(event: KeyboardEvent): void {
  if (event.key === 'Escape' || event.key === 'Enter') {
    event.preventDefault()
    close()
  }
}

// ---------- progress screen ----------
const SHOW_AFTER_MS = 250 // quick calls never flash the screen
const MIN_VISIBLE_MS = 900 // once shown, long enough to read
const STEP_MS = 700

const current = ref<BusySpec | null>(null)
const visible = ref(false)
const finishing = ref(false)
const stepIndex = ref(0)
const elapsed = ref(0)
let shownAt = 0
let showTimer: number | undefined
let stepTimer: number | undefined
let clockTimer: number | undefined
let hideTimer: number | undefined

const steps = computed(() => current.value?.steps?.length ? current.value.steps : ['Working on it'])
const percent = computed(() => {
  if (finishing.value) return 100
  const n = steps.value.length
  return Math.min(92, Math.round(((stepIndex.value + 0.5) / n) * 100))
})

function clearTimers(): void {
  window.clearTimeout(showTimer)
  window.clearInterval(stepTimer)
  window.clearInterval(clockTimer)
  window.clearTimeout(hideTimer)
}

function begin(spec: BusySpec): void {
  clearTimers()
  current.value = spec
  finishing.value = false
  stepIndex.value = 0
  elapsed.value = 0
  showTimer = window.setTimeout(() => {
    visible.value = true
    shownAt = Date.now()
    stepTimer = window.setInterval(() => {
      if (stepIndex.value < steps.value.length - 1) stepIndex.value++
    }, STEP_MS)
    clockTimer = window.setInterval(() => (elapsed.value = Math.floor((Date.now() - shownAt) / 1000)), 500)
  }, SHOW_AFTER_MS)
}

function end(): void {
  window.clearTimeout(showTimer)
  window.clearInterval(stepTimer)
  if (!visible.value) {
    clearTimers()
    current.value = null
    return
  }
  finishing.value = true
  stepIndex.value = steps.value.length
  const wait = Math.max(450, MIN_VISIBLE_MS - (Date.now() - shownAt))
  hideTimer = window.setTimeout(() => {
    clearTimers()
    visible.value = false
    current.value = null
  }, wait)
}

const active = computed(() => feedback.busy[0] ?? null)
watch(active, (now, before) => {
  if (now && now.token !== before?.token) begin(now)
  else if (!now && before) end()
})

onUnmounted(() => {
  clearTimers()
  window.clearTimeout(autoClose)
})
</script>

<template>
  <!-- progress screen -->
  <Transition name="fb-fade">
    <div v-if="visible && current" class="fb-veil fb-veil--busy" role="status" aria-live="polite">
      <div class="fb-progress" :class="{ 'is-done': finishing }">
        <div class="fb-art" :class="`fb-art--${current.kind ?? 'save'}`" aria-hidden="true">
          <svg class="fb-ring" viewBox="0 0 120 120">
            <circle class="fb-ring__track" cx="60" cy="60" r="52" />
            <circle class="fb-ring__bar" cx="60" cy="60" r="52" :style="{ strokeDashoffset: 327 - (327 * percent) / 100 }" />
          </svg>
          <div v-if="(current.kind ?? 'save') === 'excel'" class="fb-sheet">
            <i v-for="n in 5" :key="n" :style="{ animationDelay: `${n * 0.14}s` }"></i>
            <b class="fb-scan"></b>
          </div>
          <div v-else-if="current.kind === 'calc'" class="fb-calc">
            <span v-for="n in 3" :key="n" :style="{ animationDelay: `${n * 0.18}s` }"></span>
          </div>
          <div v-else class="fb-orbit"><span></span><span></span><span></span></div>
          <div class="fb-check"><i class="pi pi-check"></i></div>
        </div>

        <div class="fb-body">
          <h2>{{ finishing ? 'Completed' : current.title }}</h2>
          <ol class="fb-steps">
            <li v-for="(s, i) in steps" :key="s" :class="{ done: i < stepIndex, now: i === stepIndex && !finishing }">
              <span class="fb-dot"><i v-if="i < stepIndex" class="pi pi-check"></i><i v-else-if="i === stepIndex" class="fb-spin"></i></span>
              <span>{{ s }}</span>
            </li>
          </ol>
          <div class="fb-bar"><i :style="{ width: `${percent}%` }"></i></div>
          <div class="fb-meta"><span>{{ percent }}%</span><span>{{ elapsed }}s</span></div>
        </div>
      </div>
    </div>
  </Transition>

  <!-- pop-up -->
  <Transition name="fb-pop">
    <div v-if="popup" :key="popup.id" class="fb-veil" @click.self="close" @keydown="onKey">
      <div class="fb-popup" :class="`fb-popup--${popup.kind}`" role="alertdialog" aria-modal="true" :aria-labelledby="`fb-t-${popup.id}`" :aria-describedby="`fb-m-${popup.id}`">
        <div class="fb-icon" aria-hidden="true">
          <svg viewBox="0 0 80 80">
            <circle class="fb-icon__circle" cx="40" cy="40" r="34" />
            <path v-if="popup.kind === 'success'" class="fb-icon__mark" d="M25 41 l10 10 l21 -22" />
            <g v-else-if="popup.kind === 'error'" class="fb-icon__mark"><path d="M29 29 l22 22" /><path d="M51 29 l-22 22" /></g>
            <g v-else class="fb-icon__mark"><path d="M40 24 v20" /><path d="M40 55 v1" /></g>
          </svg>
          <svg v-if="popup.kind === 'success'" class="fb-timer" viewBox="0 0 80 80"><circle cx="40" cy="40" r="38" :style="{ animationDuration: `${SUCCESS_MS}ms` }" /></svg>
        </div>
        <h2 :id="`fb-t-${popup.id}`">{{ popup.title }}</h2>
        <p :id="`fb-m-${popup.id}`">{{ popup.message }}</p>
        <button ref="okButton" type="button" class="fb-ok" @click="close">OK</button>
        <small v-if="feedback.popups.length > 1" class="fb-more">{{ feedback.popups.length - 1 }} more message{{ feedback.popups.length > 2 ? 's' : '' }}</small>
      </div>
    </div>
  </Transition>
</template>

<style scoped>
.fb-veil {
  position: fixed;
  inset: 0;
  z-index: 9000;
  display: grid;
  place-items: center;
  padding: 16px;
  background: rgba(6, 16, 31, 0.45);
  backdrop-filter: blur(4px);
}
.fb-veil--busy { z-index: 8900; background: rgba(6, 16, 31, 0.55); }

/* ---------- pop-up ---------- */
.fb-popup {
  --tone: var(--cap-primary);
  --tone-bg: var(--cap-primary-tint);
  width: min(440px, 100%);
  padding: 30px 30px 24px;
  border-radius: 22px;
  background: var(--cap-surface);
  text-align: center;
  box-shadow: 0 30px 80px -20px rgba(6, 16, 31, 0.6), 0 0 0 1px rgba(255, 255, 255, 0.6) inset;
}
.fb-popup--success { --tone: var(--cap-success-strong); --tone-bg: var(--cap-success-bg); }
.fb-popup--warning { --tone: var(--cap-accent-dark); --tone-bg: var(--cap-warning-bg); }
.fb-popup--error { --tone: var(--cap-danger); --tone-bg: var(--cap-danger-bg); }
.fb-popup--error .fb-icon { animation: fbShake 0.5s 0.35s ease-in-out; }
.fb-icon { position: relative; width: 84px; height: 84px; margin: 0 auto 14px; border-radius: 50%; background: var(--tone-bg); }
.fb-icon svg { position: absolute; inset: 2px; width: 80px; height: 80px; }
.fb-icon__circle { fill: none; stroke: var(--tone); stroke-width: 4; stroke-dasharray: 214; stroke-dashoffset: 214; animation: fbDraw 0.55s ease-out forwards; transform: rotate(-90deg); transform-origin: center; }
.fb-icon__mark, .fb-icon__mark path { fill: none; stroke: var(--tone); stroke-width: 6; stroke-linecap: round; stroke-linejoin: round; stroke-dasharray: 60; stroke-dashoffset: 60; animation: fbDraw 0.4s 0.45s ease-out forwards; }
.fb-timer circle { fill: none; stroke: var(--tone); stroke-width: 2; opacity: 0.35; stroke-dasharray: 239; stroke-dashoffset: 0; transform: rotate(-90deg); transform-origin: center; animation: fbCountdown linear forwards; }
.fb-popup h2 { margin: 0 0 8px; color: var(--cap-ink); font-size: 20px; font-weight: 800; letter-spacing: -0.01em; }
.fb-popup p { margin: 0 0 22px; color: var(--cap-ink-soft); font-size: 14px; line-height: 1.55; white-space: pre-line; }
.fb-ok {
  min-width: 140px;
  height: 42px;
  border: 0;
  border-radius: 12px;
  background: var(--tone);
  color: #fff;
  font: inherit;
  font-size: 14px;
  font-weight: 700;
  cursor: pointer;
  box-shadow: 0 10px 22px -10px var(--tone);
  transition: transform 0.15s, filter 0.15s;
}
.fb-ok:hover { transform: translateY(-1px); filter: brightness(1.06); }
.fb-ok:focus-visible { outline: 3px solid var(--tone-bg); outline-offset: 2px; }
.fb-more { display: block; margin-top: 10px; color: var(--cap-faint); font-size: 12px; }

/* ---------- progress screen ---------- */
.fb-progress {
  display: flex;
  align-items: center;
  gap: 26px;
  width: min(560px, 100%);
  padding: 28px 30px;
  border-radius: 22px;
  background: var(--cap-surface);
  box-shadow: 0 30px 80px -20px rgba(6, 16, 31, 0.65);
  animation: fbRise 0.35s cubic-bezier(0.2, 0.8, 0.2, 1);
}
.fb-art { position: relative; flex: 0 0 120px; width: 120px; height: 120px; }
.fb-ring { position: absolute; inset: 0; width: 120px; height: 120px; transform: rotate(-90deg); }
.fb-ring__track { fill: none; stroke: var(--cap-border-soft); stroke-width: 8; }
.fb-ring__bar { fill: none; stroke: var(--cap-primary); stroke-width: 8; stroke-linecap: round; stroke-dasharray: 327; transition: stroke-dashoffset 0.6s ease, stroke 0.3s; }
.is-done .fb-ring__bar { stroke: var(--cap-success-strong); }

.fb-sheet { position: absolute; inset: 30px 32px; display: grid; gap: 4px; padding: 7px; border-radius: 8px; background: var(--cap-primary-tint); overflow: hidden; }
.fb-sheet i { display: block; height: 5px; border-radius: 3px; background: var(--cap-primary); opacity: 0.25; animation: fbRow 1.4s ease-in-out infinite; }
.fb-sheet i:nth-child(odd) { width: 80%; }
.fb-scan { position: absolute; left: 0; right: 0; height: 14px; background: linear-gradient(180deg, transparent, rgba(245, 165, 36, 0.45), transparent); animation: fbScan 1.6s ease-in-out infinite; }
.fb-calc { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; gap: 7px; }
.fb-calc span { width: 10px; height: 26px; border-radius: 4px; background: var(--cap-primary); animation: fbBars 0.9s ease-in-out infinite; }
.fb-orbit { position: absolute; inset: 32px; animation: fbSpin 1.6s linear infinite; }
.fb-orbit span { position: absolute; width: 14px; height: 14px; border-radius: 50%; background: var(--cap-primary); }
.fb-orbit span:nth-child(1) { top: 0; left: 21px; }
.fb-orbit span:nth-child(2) { bottom: 4px; left: 2px; background: var(--cap-accent); }
.fb-orbit span:nth-child(3) { bottom: 4px; right: 2px; background: var(--cap-primary-dark); }
.fb-check { position: absolute; inset: 30px; display: grid; place-items: center; border-radius: 50%; background: var(--cap-success-strong); color: #fff; font-size: 26px; transform: scale(0); transition: transform 0.35s cubic-bezier(0.3, 1.6, 0.5, 1); }
.is-done .fb-check { transform: scale(1); }
.is-done .fb-sheet, .is-done .fb-calc, .is-done .fb-orbit { opacity: 0; transition: opacity 0.2s; }

.fb-body { flex: 1 1 auto; min-width: 0; }
.fb-body h2 { margin: 0 0 12px; color: var(--cap-primary-dark); font-size: 18px; font-weight: 800; }
.fb-steps { display: grid; gap: 7px; margin: 0 0 14px; padding: 0; list-style: none; }
.fb-steps li { display: flex; align-items: center; gap: 9px; color: var(--cap-faint); font-size: 13px; transition: color 0.25s; }
.fb-steps li.now { color: var(--cap-ink); font-weight: 600; }
.fb-steps li.done { color: var(--cap-ink-soft); }
.fb-dot { width: 20px; height: 20px; flex: 0 0 20px; display: grid; place-items: center; border-radius: 50%; border: 2px solid var(--cap-border); font-size: 10px; transition: all 0.25s; }
.fb-steps li.done .fb-dot { border-color: var(--cap-success-strong); background: var(--cap-success-strong); color: #fff; animation: fbPopIn 0.3s; }
.fb-steps li.now .fb-dot { border-color: var(--cap-primary); }
.fb-spin { width: 10px; height: 10px; border-radius: 50%; border: 2px solid var(--cap-primary); border-top-color: transparent; animation: fbSpin 0.7s linear infinite; }
.fb-bar { position: relative; height: 6px; border-radius: 6px; background: var(--cap-border-soft); overflow: hidden; }
.fb-bar i { position: absolute; inset: 0 auto 0 0; border-radius: inherit; background: linear-gradient(90deg, var(--cap-primary), var(--cap-primary-hover)); transition: width 0.6s ease; overflow: hidden; }
.fb-bar i::after { content: ''; position: absolute; inset: 0; background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.55), transparent); animation: fbShimmer 1.2s linear infinite; }
.is-done .fb-bar i { background: var(--cap-success-strong); }
.fb-meta { display: flex; justify-content: space-between; margin-top: 6px; color: var(--cap-faint); font-size: 11px; font-variant-numeric: tabular-nums; }

@keyframes fbDraw { to { stroke-dashoffset: 0; } }
@keyframes fbCountdown { to { stroke-dashoffset: 239; } }
@keyframes fbShake { 0%, 100% { transform: none; } 20% { transform: translateX(-6px); } 40% { transform: translateX(6px); } 60% { transform: translateX(-4px); } 80% { transform: translateX(3px); } }
@keyframes fbRise { from { opacity: 0; transform: translateY(14px) scale(0.98); } to { opacity: 1; transform: none; } }
@keyframes fbRow { 0%, 100% { opacity: 0.2; } 50% { opacity: 0.85; } }
@keyframes fbScan { 0% { top: -14px; } 100% { top: 100%; } }
@keyframes fbBars { 0%, 100% { transform: scaleY(0.4); } 50% { transform: scaleY(1); } }
@keyframes fbSpin { to { transform: rotate(360deg); } }
@keyframes fbPopIn { from { transform: scale(0.4); } to { transform: scale(1); } }
@keyframes fbShimmer { from { transform: translateX(-100%); } to { transform: translateX(100%); } }

.fb-fade-enter-active, .fb-fade-leave-active { transition: opacity 0.25s; }
.fb-fade-enter-from, .fb-fade-leave-to { opacity: 0; }
.fb-pop-enter-active { transition: opacity 0.2s; }
.fb-pop-enter-active .fb-popup { animation: fbRise 0.3s cubic-bezier(0.2, 0.8, 0.2, 1); }
.fb-pop-leave-active { transition: opacity 0.15s; }
.fb-pop-enter-from, .fb-pop-leave-to { opacity: 0; }

@media (max-width: 520px) {
  .fb-progress { flex-direction: column; text-align: center; }
  .fb-steps li { justify-content: flex-start; text-align: start; }
}
@media (prefers-reduced-motion: reduce) {
  .fb-sheet i, .fb-scan, .fb-calc span, .fb-orbit, .fb-spin, .fb-bar i::after, .fb-popup--error .fb-icon { animation: none; }
  .fb-icon__circle, .fb-icon__mark, .fb-icon__mark path { animation-duration: 0.01s; animation-delay: 0s; }
}
</style>
