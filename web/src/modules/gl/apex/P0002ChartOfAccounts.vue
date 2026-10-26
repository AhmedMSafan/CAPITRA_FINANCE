<script setup lang="ts">
// App 120 page 2 "Chart of Accounts", copied from APEX: region HTML, page CSS and page script unchanged.
import ApexPage from '@/modules/apex/ApexPage.vue'
import html from './p0002/region.html?raw'
import css from './p0002/page.css?raw'
import script from './p0002/page.js?raw'
</script>

<template>
  <ApexPage :app="120" :page="2" :html="html" :css="css" :scripts="[script]" />
</template>
