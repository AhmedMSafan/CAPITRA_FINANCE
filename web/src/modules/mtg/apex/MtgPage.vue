<script setup lang="ts">
// A page of App 105 (Mortgage): a wizard-built page from its generated definition, or the page written for it.
import { computed } from 'vue'
import ApexWizardPage from '@/modules/apex/ApexWizardPage.vue'
import { appPages } from './pages'
import { wizardPages } from './wizard/wizardPages'

const props = defineProps<{ page: number }>()
const info = computed(() => appPages.find((p) => p.page === props.page))
</script>

<template>
  <ApexWizardPage v-if="info?.wizard" :key="page" :app="105" :pages="wizardPages" :page="page" />
  <div v-else class="ut-page">
    <div class="t-Region">
      <div class="t-Region-header"><h2 class="t-Region-title">{{ info?.title ?? `Page ${page}` }}</h2></div>
      <div class="t-Region-body">This page is not copied yet.</div>
    </div>
  </div>
</template>
