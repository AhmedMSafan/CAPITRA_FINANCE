<script setup lang="ts">
// Frame shared by every portfolio workflow step page: breadcrumb, title, the case facts and the step's own progress.
import { computed } from 'vue'

const props = defineProps<{
  portfolioId: number
  title: string
  lead: string
  portfolioNo: string
  projectName: string
  projectCode: string
  statusName: string
  /** Progress inside the step, e.g. upload, validate, import. */
  stages?: { label: string; state: 'done' | 'now' | 'todo' }[]
  /** APEX page this step replaces. */
  apexPage: string
}>()

const RING = 2 * Math.PI * 26

/** Circular counter: the stage the user is on (1/3, 2/3, 3/3), filled by the stages already done. */
const counter = computed(() => {
  const list = props.stages ?? []
  const total = list.length
  const done = list.filter(s => s.state === 'done').length
  const now = list.findIndex(s => s.state === 'now')
  const position = done === total ? total : now >= 0 ? now + 1 : Math.min(done + 1, total)
  return { total, done, position, complete: total > 0 && done === total, offset: RING - (RING * done) / Math.max(total, 1) }
})
</script>

<template>
  <div class="cp-page">
    <nav class="crumbs" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'mtg-portfolio', params: { portfolioId } }"><i class="pi pi-arrow-left"></i> Back to portfolio</RouterLink>
      <span>/</span>
      <span>{{ title }}</span>
    </nav>

    <header class="cp-card hero">
      <div class="hero__text">
        <div class="hero__eyebrow">Purchased portfolio workflow</div>
        <h1>{{ title }}</h1>
        <p>{{ lead }}</p>
      </div>
      <dl class="facts">
        <div><dt>Portfolio</dt><dd>{{ portfolioNo }}</dd></div>
        <div><dt>Project</dt><dd>{{ projectName }}</dd></div>
        <div><dt>Project code</dt><dd>{{ projectCode }}</dd></div>
        <div><dt>Status</dt><dd><span class="cp-pill cp-pill--info">{{ statusName }}</span></dd></div>
      </dl>
      <div v-if="counter.total" class="counter" :class="{ 'counter--complete': counter.complete }" role="img"
           :aria-label="counter.complete ? 'All steps complete' : `Step ${counter.position} of ${counter.total}`">
        <svg viewBox="0 0 64 64" aria-hidden="true">
          <circle class="counter__track" cx="32" cy="32" r="26" />
          <circle class="counter__bar" cx="32" cy="32" r="26" :style="{ strokeDasharray: RING, strokeDashoffset: counter.offset }" />
        </svg>
        <div class="counter__value">
          <i v-if="counter.complete" class="pi pi-check"></i>
          <template v-else><b>{{ counter.position }}</b><span>/{{ counter.total }}</span></template>
        </div>
        <div class="counter__text">
          <strong>{{ counter.complete ? 'Step complete' : `Step ${counter.position} of ${counter.total}` }}</strong>
          <small>{{ counter.complete ? 'Nothing left to do here' : `${counter.total - counter.done} left to finish` }}</small>
        </div>
      </div>
      <ol v-if="stages?.length" class="stages">
        <li v-for="(s, i) in stages" :key="s.label" :class="`stage--${s.state}`">
          <b><i v-if="s.state === 'done'" class="pi pi-check"></i><template v-else>{{ i + 1 }}</template></b>
          <span>{{ s.label }}</span>
        </li>
      </ol>
    </header>

    <div class="body">
      <slot />
    </div>

    <p class="apex-ref">Replaces APEX App 105 page {{ apexPage }}</p>
  </div>
</template>

<style scoped>
.crumbs { display: flex; gap: 8px; margin-bottom: 14px; color: var(--cap-muted); font-size: 12px; }
.crumbs a { color: var(--cap-primary-dark); text-decoration: none; }
.crumbs a:hover { text-decoration: underline; }
.hero { display: flex; flex-wrap: wrap; align-items: flex-start; gap: 18px 28px; padding: 22px 28px; }
.hero__text { flex: 1 1 380px; }
.hero__eyebrow { color: var(--cap-warning); font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: .08em; }
.hero h1 { margin: 4px 0 6px; color: var(--cap-heading); font-size: 26px; font-weight: 700; }
.hero p { margin: 0; max-width: 620px; color: var(--cap-muted); font-size: 13px; line-height: 1.5; }
.facts { display: grid; grid-template-columns: repeat(2, minmax(120px, auto)); gap: 10px 26px; margin: 0; }
.facts dt { color: var(--cap-muted); font-size: 11px; text-transform: uppercase; letter-spacing: .04em; }
.facts dd { margin: 3px 0 0; color: var(--cap-ink); font-size: 13px; font-weight: 600; }
.counter { display: flex; align-items: center; gap: 12px; order: -1; margin-inline-start: auto; }
.hero__text { order: -2; }
.counter svg { width: 64px; height: 64px; transform: rotate(-90deg); }
.counter { position: relative; }
.counter__track { fill: none; stroke: var(--cap-border-soft); stroke-width: 6; }
.counter__bar { fill: none; stroke: var(--cap-orange); stroke-width: 6; stroke-linecap: round; transition: stroke-dashoffset .8s cubic-bezier(.2, .8, .2, 1), stroke .3s; animation: counterIn 1s cubic-bezier(.2, .8, .2, 1); }
.counter--complete .counter__bar { stroke: var(--cap-success-strong); }
.counter__value { position: absolute; left: 0; top: 0; width: 64px; height: 64px; display: grid; place-items: center; grid-auto-flow: column; justify-content: center; align-items: baseline; padding-top: 20px; color: var(--cap-heading); }
.counter__value b { font-size: 20px; font-weight: 800; line-height: 1; }
.counter__value span { font-size: 12px; font-weight: 700; color: var(--cap-muted); }
.counter--complete .counter__value { padding-top: 0; align-items: center; color: var(--cap-success-strong); font-size: 20px; }
.counter__text { display: grid; gap: 2px; }
.counter__text strong { color: var(--cap-ink); font-size: 13px; }
.counter__text small { color: var(--cap-muted); font-size: 12px; }
@keyframes counterIn { from { stroke-dashoffset: 163.4; } }
.stages { flex: 1 1 100%; display: flex; flex-wrap: wrap; gap: 10px; margin: 4px 0 0; padding: 14px 0 0; border-top: 1px solid var(--cap-border-soft); list-style: none; }
.stages li { display: flex; align-items: center; gap: 9px; padding: 6px 14px 6px 6px; border-radius: 999px; background: var(--cap-bg); color: var(--cap-muted); font-size: 12px; }
.stages b { width: 24px; height: 24px; display: grid; place-items: center; border-radius: 50%; background: var(--cap-border-soft); color: var(--cap-muted); font-size: 11px; }
.stage--now { background: var(--cap-selected-tint) !important; color: var(--cap-selected) !important; font-weight: 600; }
.stage--now b { background: var(--cap-selected) !important; color: #fff !important; }
.stage--done { color: var(--cap-success) !important; }
.stage--done b { background: var(--cap-success-strong) !important; color: #fff !important; }
.body { display: grid; grid-template-columns: minmax(0, 1fr); gap: 16px; margin-top: 16px; }
.apex-ref { margin: 14px 2px 0; color: var(--cap-faint); font-size: 10px; }
</style>
