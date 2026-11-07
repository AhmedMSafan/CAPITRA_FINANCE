<script setup lang="ts">
import AppShellHeader from './AppShellHeader.vue'
import ServerStatus from './ServerStatus.vue'
import { language } from './language'
</script>

<template>
  <div class="capitra-app-layout">
    <AppShellHeader />
    <ServerStatus />
    <main class="capitra-app-content">
      <!-- keyed by the language: a switch draws the page again in place -->
      <router-view v-slot="{ Component, route }">
        <component :is="Component" :key="`${route.fullPath}#${language}`" />
      </router-view>
    </main>
  </div>
</template>
