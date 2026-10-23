<script setup lang="ts">
// Portfolio acquisition hub (APEX App 105 page 4): portfolio cases and master data.
import HubPage from '@/shared/hub/HubPage.vue'
import type { HubTile } from '@/shared/hub/types'

const tiles: HubTile[] = [
  { title: 'Portfolio Cases', description: 'Purchased portfolios: accounts, pricing terms, investors and payment history.', icon: 'pi pi-folder-open', to: '/mortgage/portfolios', apexPages: '201-207' },
  { title: 'Developers', description: 'Real-estate developers, their registration details and risk level.', icon: 'pi pi-briefcase', to: '/mortgage/developers', apexPages: '101/102' },
  { title: 'Developer Signatories', description: 'People authorised to sign for each developer.', icon: 'pi pi-id-card', to: '/mortgage/developer-signatories', apexPages: '103/104' },
  { title: 'Projects', description: 'Developer projects: location, title deed, licence and completion.', icon: 'pi pi-map', to: '/mortgage/projects', apexPages: '105/106' },
  { title: 'Project Units', description: 'Units in each project with area, contract and market value.', icon: 'pi pi-building', to: '/mortgage/project-units', apexPages: '107/108' },
  { title: 'Investors', description: 'Unit buyers: identity, contact details, employer and income.', icon: 'pi pi-users', to: '/mortgage/investors', apexPages: '109/110' },
  { title: 'Reference Data', description: 'Code lists: statuses, document types, check types, currencies and more.', icon: 'pi pi-database', to: '/mortgage/reference-data', apexPages: '3' }
]
</script>

<template>
  <HubPage
    title="Portfolio Acquisition"
    description="Portfolio cases, and the developers, projects, units and investors used when buying mortgage portfolios."
    icon="pi pi-th-large"
    :tiles="tiles"
    apex-page="4"
  />
</template>
