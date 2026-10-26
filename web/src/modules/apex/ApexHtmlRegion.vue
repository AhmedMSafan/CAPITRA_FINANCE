<script setup lang="ts">
// A static content region (its HTML) or a PL/SQL dynamic content region: the server process REGION_<key>
// writes the same HTML the PL/SQL printed, for the page items.
import { computed, ref, watch } from 'vue'
import { http } from '@/shared/api/http'
import { substitute, type WizardRegion } from './wizard'

const props = defineProps<{ app: number; page: number; region: WizardRegion; items: Record<string, string> }>()

const dynamic = ref('')
const html = computed(() => (props.region.kind === 'plsql' ? dynamic.value : substitute(props.region.html ?? '', props.items)))

async function load() {
  if (props.region.kind !== 'plsql') return
  try {
    const res = await http.post<{ ok: boolean; html?: string }>(`/api/apex/${props.app}/${props.page}/REGION_${props.region.key}`, {
      x01: JSON.stringify(props.items)
    })
    dynamic.value = res.ok ? (res.html ?? '') : ''
  } catch {
    dynamic.value = ''
  }
}

watch(() => JSON.stringify(props.items), load, { immediate: true })
</script>

<template>
  <div v-if="html" class="t-Region" :class="{ 't-Region--plain': region.hideHeader }">
    <div v-if="!region.hideHeader" class="t-Region-header"><h2 class="t-Region-title">{{ region.title }}</h2></div>
    <!-- eslint-disable-next-line vue/no-v-html -- the region's own HTML, built on the server from escaped values as APEX did -->
    <div class="t-Region-body" v-html="html"></div>
  </div>
</template>
