<script setup lang="ts">
// A page of App 110 (Mortgage Finance), rendered from its definition generated from the APEX export. The header and
// the module's pages menu are the shared Capitra header (AppShellHeader); the language is the header's switch.
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import ApexWizardPage from '@/modules/apex/ApexWizardPage.vue'
import { useMffUi } from './mffUi'
import { wizardPages } from './wizard/wizardPages'

const props = defineProps<{ page: number }>()
const route = useRoute()
const router = useRouter()
const opened = ref(0)
// the app's CSS File URLs (#APP_FILES#finova-v14.css) and the global page's styles
const appCss = ['/apex/110/finova-v14.css', '/apex/110/mff-ui.css']

onMounted(() => useMffUi(router, () => props.page))
</script>

<template>
  <component :is="'link'" v-for="href in appCss" :key="href" rel="stylesheet" :href="href" />
  <div class="mff-app t-PageBody">
    <main class="t-Body-main mff-main">
      <ApexWizardPage :key="`${route.fullPath}#${opened}`" :app="110" :pages="wizardPages" :page="page" @reload="opened++" />
    </main>
  </div>
</template>

<style scoped>
.mff-app { min-height: calc(100vh - 120px); background: var(--ds-bg); }
.mff-main { min-width: 0; padding: 0 0 24px; }
</style>
