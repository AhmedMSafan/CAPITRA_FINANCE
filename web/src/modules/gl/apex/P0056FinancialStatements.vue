<script setup lang="ts">
// App 120 page 56 "Financial Statements", copied from APEX: region HTML, page CSS and page script (acc-fs56.js) unchanged.
import ApexPage from '@/modules/apex/ApexPage.vue'
import html from './p0056/region.html?raw'
import css from './p0056/page.css?raw'
import script from './p0056/page.js?raw'
</script>

<template>
  <ApexPage :app="120" :page="56" :html="html" :css="css" :scripts="[script]" />
</template>
