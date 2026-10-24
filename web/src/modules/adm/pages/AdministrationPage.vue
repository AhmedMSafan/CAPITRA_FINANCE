<script setup lang="ts">
// Administration hub: global settings shared by every Capitra application.
import HubPage from '@/shared/hub/HubPage.vue'
import type { HubTile } from '@/shared/hub/types'

const tiles: HubTile[] = [
  { title: 'Reference Data', description: 'Global code lists: currencies, countries, cheque and payment statuses, risk levels, document and decision types.', icon: 'pi pi-database', to: '/admin/reference-data', apexPages: '' }
]
</script>

<template>
  <HubPage
    title="Administration"
    description="Settings and code lists shared by every Capitra application."
    icon="pi pi-cog"
    :tiles="tiles"
  />
</template>
