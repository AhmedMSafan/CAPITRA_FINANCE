<template>
  <!-- The strip on a completed step's page opened view-only from the Disbursements tab, with the GL transaction sets the step posted
       (Ahmed, 7 Nov 2026). -->
  <p class="fc-viewonly" role="status"><span class="fa fa-eye" aria-hidden="true"></span>
    <span>{{ tr('View only: this step is completed. Nothing can be changed here.') }}</span>
    <span v-if="sets.length" class="fc-viewonly-gl">{{ tr('GL Tran Set ID') }}: <b v-for="s in sets" :key="s" class="fc-ref">{{ s }}</b></span>
    <span v-else-if="loaded && step" class="fc-viewonly-gl fc-viewonly-none">{{ tr('No GL entries posted by this step') }}</span></p>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { http } from '@/shared/api/http'
import { tr } from '@/shared/layout/language'

const props = defineProps<{ disbursementId?: number; step?: string }>()
const sets = ref<string[]>([])
const loaded = ref(false)

watch(() => [props.disbursementId, props.step], async () => {
  sets.value = []
  loaded.value = false
  if (!props.disbursementId || !props.step) return
  try {
    sets.value = await http.get<string[]>(`/api/crm/disbursement-requests/${props.disbursementId}/steps/${props.step}/gl-tran-sets`)
  } catch {
    sets.value = []
  } finally {
    loaded.value = true
  }
}, { immediate: true })
</script>

<style scoped>
.fc-viewonly { display: flex; align-items: center; flex-wrap: wrap; gap: 10px; margin: 0 0 14px; padding: 10px 14px; border-radius: 8px; background: var(--fin-teal-50);
  border-inline-start: 4px solid var(--fin-teal-600); color: var(--fin-ink); font-weight: 600; }
.fc-viewonly .fa { color: var(--fin-teal-600); }
.fc-viewonly-gl { display: inline-flex; flex-wrap: wrap; align-items: center; gap: 6px; margin-inline-start: auto; }
.fc-viewonly-none { color: var(--fin-muted); font-weight: 400; }
</style>
