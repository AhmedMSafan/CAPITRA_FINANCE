<script setup lang="ts">
// Title band of a GL page, with the page's main actions on the right.
defineProps<{ title: string; description: string; icon: string }>()
</script>

<template>
  <header class="gl-head">
    <div class="gl-head__icon"><i :class="icon"></i></div>
    <div class="gl-head__text">
      <span class="gl-head__eyebrow">General ledger</span>
      <h2>{{ title }}</h2>
      <p>{{ description }}</p>
    </div>
    <div class="gl-head__actions"><slot></slot></div>
  </header>
</template>

<style scoped>
.gl-head { display: flex; align-items: center; gap: 16px; margin-bottom: 16px; padding: 18px 24px; border: 1px solid var(--cap-border-soft); border-radius: 16px; background: #fff; box-shadow: 0 8px 24px rgba(13, 42, 46, .06); }
.gl-head__icon { width: 50px; height: 50px; flex: 0 0 50px; display: grid; place-items: center; border-radius: 14px; background: var(--cap-primary-tint); color: var(--cap-primary); font-size: 21px; }
.gl-head__text { flex: 1; min-width: 0; }
.gl-head__eyebrow { color: var(--cap-primary); font-size: 11px; font-weight: 600; letter-spacing: .06em; text-transform: uppercase; }
.gl-head h2 { margin: 2px 0 0; color: var(--cap-heading); font-size: 20px; font-weight: 600; }
.gl-head p { margin: 4px 0 0; color: var(--cap-muted); font-size: 13px; max-width: 760px; }
.gl-head__actions { display: flex; flex-wrap: wrap; align-items: center; justify-content: flex-end; gap: 8px; }
.gl-head__actions :deep(select) { height: 40px; min-width: 240px; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 7px; background: #fff; color: var(--cap-ink); font: inherit; font-size: 13px; }
@media (max-width: 900px) { .gl-head { flex-wrap: wrap; } .gl-head__actions { width: 100%; justify-content: flex-start; } }
</style>
