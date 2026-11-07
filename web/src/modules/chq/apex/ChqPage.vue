<script setup lang="ts">
// A page of App 122 (Cheques Wallet), rendered from its definition generated from the APEX export. The header and
// the module's pages menu are the shared Capitra header (AppShellHeader).
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import ApexWizardPage from '@/modules/apex/ApexWizardPage.vue'
import { wizardPages } from './wizard/wizardPages'

const props = defineProps<{ page: number }>()
const route = useRoute()
const opened = ref(0)
// the app's CSS File URLs (#APP_FILES#finova-theme.css)
const appCss = ['/apex/122/finova-theme.css']
</script>

<template>
  <component :is="'link'" v-for="href in appCss" :key="href" rel="stylesheet" :href="href" />
  <div class="chq-app t-PageBody">
    <main class="t-Body-main chq-main">
      <ApexWizardPage :key="`${route.fullPath}#${opened}`" :app="122" :pages="wizardPages" :page="props.page" @reload="opened++" />
    </main>
  </div>
</template>

<style scoped>
.chq-app { min-height: calc(100vh - 120px); background: var(--ds-bg); }
.chq-main { padding: 0 0 24px; }
</style>
