<script setup lang="ts">
// Shown when the API does not answer, so a page never just stays empty.
import { apiBaseUrl, serverDown } from '@/shared/api/http'
const refresh = () => location.reload()
</script>

<template>
  <div v-if="serverDown" class="srv" role="alert">
    <i class="pi pi-exclamation-triangle"></i>
    <div>
      <b>The Capitra server is not reachable.</b>
      <span>The page could not load its data from {{ apiBaseUrl }}. Check that the API is running, then refresh the page.</span>
    </div>
    <button type="button" @click="refresh">Refresh</button>
  </div>
</template>

<style scoped>
.srv { display: flex; align-items: center; gap: 14px; margin: 16px var(--ds-gutter) 0; padding: 14px 18px; border-radius: 14px; background: var(--ds-danger-soft); color: var(--ds-danger); border: 1px solid var(--ds-danger-line); font-family: var(--ds-font); }
.srv i { font-size: 20px; }
.srv b { display: block; font-size: 14px; }
.srv span { font-size: 13px; color: var(--ds-ink); }
.srv button { margin-inline-start: auto; border: 1px solid var(--ds-danger); background: transparent; color: var(--ds-danger); border-radius: 999px; padding: 7px 16px; font: 600 13px var(--ds-font); cursor: pointer; }
</style>
