<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'

import AppHeader from './AppHeader.vue'
import HomeHeader from './HomeHeader.vue'

const route = useRoute()

const moduleName = computed(() => {
  return (route.meta.moduleName as string) || ''
})

const functionName = computed(() => {
  return (route.meta.functionName as string) || ''
})
</script>

<template>
  <div class="capitra-app-layout">
    <HomeHeader
      v-if="route.name === 'home'"
      :moduleName="moduleName"
      :functionName="functionName"
    />
    <AppHeader
      v-else
      :moduleName="moduleName"
      :functionName="functionName"
    />

    <main class="capitra-app-content">
      <router-view />
    </main>
  </div>
</template>
