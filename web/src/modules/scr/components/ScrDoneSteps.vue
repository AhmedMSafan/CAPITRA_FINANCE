<template>
  <!-- The completed steps of a decision: each opens the decision with that step view-only, as in Factoring. -->
  <div v-if="steps?.length" class="scr-done-steps">
    <RouterLink v-for="s in steps" :key="s.key" class="scr-done-step" :class="{ 'scr-done-step--back': s.action === 'REJECT' }"
                :to="{ name: 'scr-decision', params: { decisionId }, query: { view: '1', step: s.stepCode } }"
                :title="`${tr('Open the completed step, view only')}${s.doneBy ? ' · ' + s.doneBy : ''}${s.doneAt ? ' · ' + formatDateTime(s.doneAt) : ''}`">
      <span class="fa" :class="s.action === 'REJECT' ? 'fa-undo' : 'fa-check'" aria-hidden="true"></span>{{ tr(s.stepName) }}
    </RouterLink>
  </div>
</template>

<script setup lang="ts">
import { tr } from '@/shared/layout/language'
import { formatDateTime } from '../utils/dates'
import type { ScrCompletedStep } from '../api/scr.api'

defineProps<{ decisionId: number; steps?: ScrCompletedStep[] | null }>()
</script>
