<script setup lang="ts">
// Frame shared by every portfolio workflow step page: breadcrumb, title, the case facts and the step's own progress.
defineProps<{
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
.crumbs { display: flex; gap: 8px; margin-bottom: 14px; color: #718579; font-size: 12px; }
.crumbs a { color: #0a3573; text-decoration: none; }
.crumbs a:hover { text-decoration: underline; }
.hero { display: flex; flex-wrap: wrap; align-items: flex-start; gap: 18px 28px; padding: 22px 28px; }
.hero__text { flex: 1 1 380px; }
.hero__eyebrow { color: #c47b2c; font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: .08em; }
.hero h1 { margin: 4px 0 6px; color: #0a3573; font-size: 26px; font-weight: 700; }
.hero p { margin: 0; max-width: 620px; color: #556b7a; font-size: 13px; line-height: 1.5; }
.facts { display: grid; grid-template-columns: repeat(2, minmax(120px, auto)); gap: 10px 26px; margin: 0; }
.facts dt { color: #718579; font-size: 11px; text-transform: uppercase; letter-spacing: .04em; }
.facts dd { margin: 3px 0 0; color: #183550; font-size: 13px; font-weight: 600; }
.stages { flex: 1 1 100%; display: flex; flex-wrap: wrap; gap: 10px; margin: 4px 0 0; padding: 14px 0 0; border-top: 1px solid #e5ecee; list-style: none; }
.stages li { display: flex; align-items: center; gap: 9px; padding: 6px 14px 6px 6px; border-radius: 999px; background: #f4f7fb; color: #718579; font-size: 12px; }
.stages b { width: 24px; height: 24px; display: grid; place-items: center; border-radius: 50%; background: #dfe7ee; color: #556b7a; font-size: 11px; }
.stage--now { background: #e7f0fa !important; color: #0a3573 !important; font-weight: 600; }
.stage--now b { background: #0a3573 !important; color: #fff !important; }
.stage--done { color: #197447 !important; }
.stage--done b { background: #15945b !important; color: #fff !important; }
.body { display: grid; grid-template-columns: minmax(0, 1fr); gap: 16px; margin-top: 16px; }
.apex-ref { margin: 14px 2px 0; color: #9aa9b4; font-size: 10px; }
</style>
