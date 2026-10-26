<script setup lang="ts">
// Reference data hub (APEX App 105 page 3): one tile per code table.
// ADM lists are global and live under Administration; MTG lists stay under Portfolio Acquisition.
import { computed, ref, shallowRef } from 'vue'
import { useFeedback } from '@/shared/feedback/useFeedback'
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
  country: 'pi pi-globe',
  'unit-type': 'pi pi-building',
  'legal-entity': 'pi pi-building-columns',
  'business-line': 'pi pi-briefcase'
}

const props = defineProps<{ module: 'ADM' | 'MTG' | 'GL' }>()

const AREAS = {
  ADM: {
    base: '/admin/reference-data',
    description: 'Global code lists shared by every Capitra application: currencies, countries, statuses, document types and more.',
    back: { to: '/admin', label: 'Administration' }
  },
  MTG: {
    base: '/mortgage/reference-data',
    description: 'Code lists used only by portfolio acquisition: portfolio statuses, inquiry, check and valuation types, approval steps and unit types.',
    back: { to: '/mortgage', label: 'Master Data' }
  },
  GL: {
    base: '/gl/reference-data',
    description: 'Organisation lists used by the general ledger: legal entities and business lines.',
    back: { to: '/gl', label: 'General Ledger' }
  }
} as const

const area = computed(() => AREAS[props.module])

const definitions = shallowRef<LookupDefinition[]>([])
const error = ref('')

getLookupDefinitions()
  .then(d => (definitions.value = d))
  .catch(e => (error.value = e instanceof Error ? e.message : 'Unable to load the reference data lists.'))

const tiles = computed<HubTile[]>(() =>
  definitions.value.filter(d => d.module === props.module).map(d => ({
    title: d.title,
    description: `${d.module === 'MTG' ? 'Portfolio acquisition' : d.module === 'GL' ? 'General ledger' : 'Global'} code list.`,
    icon: ICONS[d.name] ?? 'pi pi-list',
    to: `${area.value.base}/${d.name}`,
    apexPages: d.apexPages
  }))
)

// Errors open the app-wide pop-up.
useFeedback({ errors: [error] })
</script>

<template>
  <HubPage
    title="Reference Data"
    :description="area.description"
    icon="pi pi-database"
    :tiles="tiles"
    apex-page="3"
  >
    <template #actions>
      <RouterLink :to="area.back.to" class="hub-link"><i class="pi pi-arrow-left"></i><span>{{ area.back.label }}</span></RouterLink>
    </template>
  </HubPage>
</template>

<style scoped>
.hub-error { margin: 24px 30px 0; padding: 12px 16px; border: 1px solid var(--cap-danger-border); border-radius: 8px; background: var(--cap-danger-bg); color: var(--cap-danger); font-size: 13px; }
.hub-link { display: inline-flex; align-items: center; gap: 8px; color: var(--cap-ink-soft); font-size: 13px; text-decoration: none; }
.hub-link:hover { color: var(--cap-primary-dark); }
</style>
