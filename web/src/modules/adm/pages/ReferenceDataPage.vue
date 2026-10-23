<script setup lang="ts">
// Reference data hub (APEX App 105 page 3): one tile per code table.
import { computed, ref, shallowRef } from 'vue'
import HubPage from '@/shared/hub/HubPage.vue'
import type { HubTile } from '@/shared/hub/types'
import { getLookupDefinitions, type LookupDefinition } from '../api/lookups.api'

const ICONS: Record<string, string> = {
  'portfolio-status': 'pi pi-flag',
  'document-group': 'pi pi-folder',
  'document-type': 'pi pi-file',
  'inquiry-type': 'pi pi-question-circle',
  'dd-check-type': 'pi pi-check-square',
  'valuation-type': 'pi pi-chart-bar',
  'approval-step': 'pi pi-sitemap',
  'decision-type': 'pi pi-directions',
  'guarantee-type': 'pi pi-shield',
  'insurance-type': 'pi pi-verified',
  'risk-level': 'pi pi-chart-line',
  'payment-status': 'pi pi-credit-card',
  'cheque-status': 'pi pi-wallet',
  currency: 'pi pi-dollar',
  'unit-type': 'pi pi-building'
}

const definitions = shallowRef<LookupDefinition[]>([])
const error = ref('')

getLookupDefinitions()
  .then(d => (definitions.value = d))
  .catch(e => (error.value = e instanceof Error ? e.message : 'Unable to load the reference data lists.'))

const tiles = computed<HubTile[]>(() =>
  definitions.value.map(d => ({
    title: d.title,
    description: `${d.module === 'MTG' ? 'Portfolio acquisition' : 'Shared'} code list.`,
    icon: ICONS[d.name] ?? 'pi pi-list',
    to: `/mortgage/reference-data/${d.name}`,
    apexPages: d.apexPages
  }))
)
</script>

<template>
  <div v-if="error" class="hub-error">{{ error }}</div>
  <HubPage
    title="Reference Data"
    description="Code lists used by portfolio acquisition: statuses, document types, check types, currencies and more."
    icon="pi pi-database"
    :tiles="tiles"
    apex-page="3"
  >
    <template #actions>
      <RouterLink to="/mortgage" class="hub-link"><i class="pi pi-arrow-left"></i><span>Master Data</span></RouterLink>
    </template>
  </HubPage>
</template>

<style scoped>
.hub-error { margin: 24px 30px 0; padding: 12px 16px; border: 1px solid #f2c4c4; border-radius: 8px; background: #fff3f3; color: #b42318; font-size: 13px; }
.hub-link { display: inline-flex; align-items: center; gap: 8px; color: #405b4d; font-size: 13px; text-decoration: none; }
.hub-link:hover { color: #0a3573; }
</style>
