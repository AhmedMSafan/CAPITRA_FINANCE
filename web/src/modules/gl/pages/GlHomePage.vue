<script setup lang="ts">
// General ledger hub (APEX App 120 page 1): the GL setup screens built so far.
import HubPage from '@/shared/hub/HubPage.vue'
import type { HubTile } from '@/shared/hub/types'

const tiles: HubTile[] = [
  { title: 'Account Structure & Segments', description: 'The account structure, its segments (company, branch, account ...) and the values each segment accepts.', icon: 'pi pi-sitemap', to: '/gl/account-structure', apexPages: '40' },
  { title: 'Chart of Accounts', description: 'The natural account tree: types, normal balance, statements and posting controls.', icon: 'pi pi-book', to: '/gl/chart-of-accounts', apexPages: '2/3' },
  { title: 'Reference Data', description: 'Legal entities and business lines used by segment values and ledgers.', icon: 'pi pi-database', to: '/gl/reference-data', apexPages: '' }
]
</script>

<template>
  <HubPage
    title="General Ledger"
    description="Chart of accounts setup first; accounting periods, journals, posting and statements follow."
    icon="pi pi-book"
    :tiles="tiles"
    apex-page="1"
  />
</template>
