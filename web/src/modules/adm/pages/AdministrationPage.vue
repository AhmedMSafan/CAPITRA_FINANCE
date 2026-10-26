<script setup lang="ts">
// Administration hub: global settings shared by every Capitra application.
import HubPage from '@/shared/hub/HubPage.vue'
import type { HubTile } from '@/shared/hub/types'

const tiles: HubTile[] = [
  { title: 'Reference Data', description: 'Global code lists: currencies, countries, cheque and payment statuses, risk levels, document and decision types.', icon: 'pi pi-database', to: '/admin/reference-data', apexPages: '' },
  { title: 'Users', description: 'Create users, set their roles, activate or deactivate them and reset passwords.', icon: 'pi pi-users', to: '/admin/users', apexPages: '' },
  { title: 'Roles', description: 'Business roles used for security and workflow routing, and the permissions each role grants.', icon: 'pi pi-shield', to: '/admin/roles', apexPages: '' },
  { title: 'Workflow Definitions', description: 'Design and release the BPMN workflows that route tasks to users and roles.', icon: 'pi pi-sitemap', to: '/workflowengine', apexPages: '' }
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
