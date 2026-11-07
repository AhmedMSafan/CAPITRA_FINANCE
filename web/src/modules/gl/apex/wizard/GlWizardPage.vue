<script setup lang="ts">
// A wizard-built page of App 120 (interactive report and form pages), rendered from its generated definition.
import ApexWizardPage from '@/modules/apex/ApexWizardPage.vue'
import { wizardPages } from './wizardPages'

defineProps<{ page: number }>()
</script>

<template>
  <ApexWizardPage :key="page" :app="120" :pages="wizardPages" :page="page" />
</template>
