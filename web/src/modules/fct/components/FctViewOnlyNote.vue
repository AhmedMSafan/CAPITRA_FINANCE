<template>
  <!-- The strip on a completed step's page opened view-only from the Disbursements tab. -->
  <p class="fc-viewonly" role="status"><span class="fa fa-eye" aria-hidden="true"></span>
    <span>{{ tr('View only: this step is completed. Nothing can be changed here.') }}</span></p>
</template>

<script setup lang="ts">
import { tr } from '@/shared/layout/language'
</script>

<style scoped>
.fc-viewonly { display: flex; align-items: center; gap: 10px; margin: 0 0 14px; padding: 10px 14px; border-radius: 8px; background: var(--fin-teal-50);
  border-inline-start: 4px solid var(--fin-teal-600); color: var(--fin-ink); font-weight: 600; }
.fc-viewonly .fa { color: var(--fin-teal-600); }
</style>
