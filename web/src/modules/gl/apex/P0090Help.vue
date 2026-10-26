<script setup lang="ts">
// App 120 page 90 "General Ledger Help", copied from APEX: both help regions, page CSS and the page load script unchanged.
import ApexPage from '@/modules/apex/ApexPage.vue'
import html from './p0090/region.html?raw'
import css from './p0090/page.css?raw'
import script from './p0090/page.js?raw'
</script>

<template>
  <ApexPage :app="120" :page="90" :html="html" :css="css" :scripts="[script]" />
</template>
