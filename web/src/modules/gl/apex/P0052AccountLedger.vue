<script setup lang="ts">
// App 120 page 52 "Account Ledger", copied from APEX: region HTML, page CSS and page script (acc-journal52.js) unchanged.
import ApexPage from '@/modules/apex/ApexPage.vue'
import html from './p0052/region.html?raw'
import css from './p0052/page.css?raw'
import script from './p0052/page.js?raw'
</script>

<template>
  <ApexPage :app="120" :page="52" :html="html" :css="css" :scripts="[script]" />
</template>
