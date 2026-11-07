<script setup lang="ts">
// App 120 page 51 "Journals", copied from APEX: region HTML, page CSS and page script (acc-journal51.js) unchanged.
import ApexPage from '@/modules/apex/ApexPage.vue'
import html from './p0051/region.html?raw'
import css from './p0051/page.css?raw'
import script from './p0051/page.js?raw'
</script>

<template>
  <ApexPage :app="120" :page="51" :html="html" :css="css" :scripts="[script]" />
</template>
