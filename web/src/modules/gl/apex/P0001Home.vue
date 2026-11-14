<script setup lang="ts">
// App 120 page 1 "Home", copied from APEX: the PL/SQL region is rendered by the ported REGION_HOME process.
import ApexPage from '@/modules/apex/ApexPage.vue'
import css from './p0001/page.css?raw'

const html = '<div data-apex-region="REGION_HOME"></div>'
</script>

<template>
  <ApexPage :app="120" :page="1" :html="html" :css="css" :scripts="[]" />
</template>
