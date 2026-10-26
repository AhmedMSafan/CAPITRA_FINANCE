<script setup lang="ts">
// A page of App 105 (Mortgage), rendered from its definition generated from the APEX export. The page opens again
// for other page items (another URL), as APEX loads a page again.
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ApexWizardPage from '@/modules/apex/ApexWizardPage.vue'
import MtgHeader from './MtgHeader.vue'
import { useBilingual } from './finovaBilingual'
import { wizardPages } from './wizard/wizardPages'

defineProps<{ page: number }>()
const route = useRoute()
const router = useRouter()
// finova-portfolio-bilingual.js, one of the app's JavaScript File URLs
onMounted(() => useBilingual(router))
const opened = ref(0)
// the app's CSS File URLs (User Interface attributes of App 105): #APP_FILES#finova-v14.css, finova-v15.css
const appCss = ['/apex/105/finova-v14.css', '/apex/105/finova-v15.css']
</script>

<template>
  <component :is="'link'" v-for="href in appCss" :key="href" rel="stylesheet" :href="href" />
  <MtgHeader :page="page" />
  <ApexWizardPage :key="`${route.fullPath}#${opened}`" :app="105" :pages="wizardPages" :page="page" @reload="opened++" />
</template>
