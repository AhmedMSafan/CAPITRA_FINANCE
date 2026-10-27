<script setup lang="ts">
import AppShellHeader from './AppShellHeader.vue'
import ServerStatus from './ServerStatus.vue'
</script>

<template>
  <div class="capitra-app-layout">
    <AppShellHeader />
    <ServerStatus />
    <main class="capitra-app-content">
      <router-view />
    </main>
  </div>
</template>
