<script setup lang="ts">
// App 120 page 40 "Account Structure and Segments", copied from APEX: region HTML, page CSS and page script unchanged.
import ApexPage from '@/modules/apex/ApexPage.vue'
import html from './p0040/region.html?raw'
import css from './p0040/page.css?raw'
import script from './p0040/page.js?raw'
</script>

<template>
  <ApexPage :app="120" :page="40" :html="html" :css="css" :scripts="[script]" />
</template>
