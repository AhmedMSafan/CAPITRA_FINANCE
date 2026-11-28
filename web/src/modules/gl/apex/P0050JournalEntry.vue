<script setup lang="ts">
// App 120 page 50 "Journal Entry", copied from APEX: region HTML, page CSS and page script (acc-journal50.js) unchanged.
import ApexPage from '@/modules/apex/ApexPage.vue'
import html from './p0050/region.html?raw'
import css from './p0050/page.css?raw'
import script from './p0050/page.js?raw'
</script>

<template>
  <ApexPage :app="120" :page="50" :html="html" :css="css" :scripts="[script]" />
</template>
