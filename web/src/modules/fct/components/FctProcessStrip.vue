<template>
  <!-- FINOVA_FINANCEING "Application Process": where this step sits in the factoring cycle -->
  <section class="fx-card fc-step-card fc-no-print">
    <header class="fc-sec-head">
      <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-sitemap"></span></span>
      <div><h2>{{ tr('Application Process') }}</h2></div>
    </header>
    <ol class="fc-process">
      <li v-for="(s, i) in steps" :key="s.label" :class="i + 1 < current ? 'is-done' : i + 1 === current ? 'is-current' : ''">
        <span class="fc-process-no"><span v-if="i + 1 < current" class="fa fa-check" aria-hidden="true"></span><template v-else>{{ i + 1 }}</template></span>
        <div><b>{{ tr(s.label) }}</b><small>{{ tr(s.owner) }}</small></div>
      </li>
    </ol>
  </section>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { tr } from '@/shared/layout/language'

const props = defineProps<{ current: number; withLegalReview?: boolean }>()

const BASE = [
  { label: 'Application', owner: 'BD Officer' },
  { label: 'Inquiry', owner: 'Inquiry Officer' },
  { label: 'Credit', owner: 'Credit Officer' },
  { label: 'Risk', owner: 'Risk Officer' },
  { label: 'Committee', owner: 'Committee Sec.' },
  { label: 'Client Acceptance', owner: 'BD + Operations' },
  { label: 'Contract', owner: 'Legal Officer' }
]

// the Legal Review page shows the cycle with its own step before Activation
const steps = computed(() => [
  ...BASE,
  ...(props.withLegalReview ? [{ label: 'Legal Review', owner: 'Legal Officer' }] : []),
  { label: 'Activation', owner: 'Operations' }
])
</script>
