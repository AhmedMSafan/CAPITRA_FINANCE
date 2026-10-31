<script setup lang="ts">
// App 120 page 30 "Accounting Periods", copied from APEX: region HTML, page CSS and page script unchanged.
import ApexPage from '@/modules/apex/ApexPage.vue'
import html from './p0030/region.html?raw'
import css from './p0030/page.css?raw'
import script from './p0030/page.js?raw'
</script>

<template>
  <ApexPage :app="120" :page="30" :html="html" :css="css" :scripts="[script]" />
</template>
