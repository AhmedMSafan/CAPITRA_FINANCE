<template>
  <!-- The steps of a consumer finance in order (Ahmed, 28 Nov 2026), the factoring standard: a completed step opens view only with the
       GL Tran Set IDs it posted; the step waiting now is worked on from the Inbox by its role; later steps are not reached yet. -->
  <section v-if="steps.length" class="fx-card cst">
    <h3 class="cst-h"><span class="fa fa-sitemap" aria-hidden="true"></span>{{ tr('Workflow steps') }}</h3>
    <ol class="cst-list">
      <li v-for="s in steps" :key="s.stepCode" class="cst-step" :class="[`cst-step--${s.status.toLowerCase()}`, { 'cst-step--here': s.stepCode === current }]">
        <span class="cst-dot" aria-hidden="true"><span class="fa" :class="s.status === 'DONE' ? (s.outcome === 'REJECT' ? 'fa-times' : 'fa-check') : s.status === 'OPEN' ? 'fa-circle' : 'fa-circle-o'"></span></span>
        <div class="cst-body">
          <b>{{ tr(s.stepName) }}</b>
          <small v-if="s.role">{{ s.role }}</small>
          <template v-if="s.status === 'DONE'">
            <small>{{ s.doneBy ?? '' }}<template v-if="s.doneAt"> · {{ formatDateTime(s.doneAt) }}</template><template v-if="s.outcome === 'REJECT'"> · {{ tr('Rejected') }}</template></small>
            <small v-if="s.glTranSetIds.length" class="cst-gl">GL <b v-for="g in s.glTranSetIds" :key="g" class="fc-ref">{{ g }}</b></small>
            <RouterLink v-if="s.stepCode !== 'CF_NEW'" class="cst-view" :to="{ name: 'lnd-loan', params: { loanId }, query: { step: s.stepCode, view: '1' } }">
              <span class="fa fa-eye" aria-hidden="true"></span>{{ tr('View') }}</RouterLink>
          </template>
          <small v-else-if="s.status === 'OPEN'" class="cst-wait">{{ tr('Waiting: open it from the Inbox') }}</small>
          <small v-else>{{ tr('Not reached yet') }}</small>
        </div>
      </li>
    </ol>
  </section>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { showError } from '@/shared/feedback/feedback'
import { formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { loansApi, type CfStepView } from '../api/loans.api'

const props = defineProps<{ loanId: number; version: number; current?: string | null }>()
const steps = ref<CfStepView[]>([])

watch(() => [props.loanId, props.version], async () => {
  try {
    steps.value = await loansApi.cfSteps(props.loanId)
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  }
}, { immediate: true })
</script>

<style scoped>
.cst { padding: 14px 18px; margin-bottom: 16px; }
.cst-h { display: flex; align-items: center; gap: 8px; margin: 0 0 12px; font-size: 15px; color: var(--fin-teal-700, #0b5c64); }
.cst-list { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 10px; margin: 0; padding: 0; list-style: none; }
.cst-step { display: flex; gap: 10px; padding: 10px 12px; border: 1px solid #e3e8ec; border-radius: 10px; background: #fff; }
.cst-step--done { background: #f3faf6; border-color: #cfe9da; }
.cst-step--open { background: #fff8e6; border-color: #f0c36d; }
.cst-step--later { background: #f6f8fa; color: #6b7b88; }
.cst-step--here { outline: 2px solid var(--fin-teal-600, #0f7c86); }
.cst-dot { margin-top: 2px; color: #1f8a4c; }
.cst-step--open .cst-dot { color: #d08a00; }
.cst-step--later .cst-dot { color: #9aa7b1; }
.cst-body { display: flex; flex-direction: column; gap: 2px; min-width: 0; font-size: 13.5px; }
.cst-body small { color: #34495a; font-size: 12.5px; }
.cst-step--later .cst-body small { color: #6b7b88; }
.cst-gl b { margin-inline-start: 4px; }
.cst-wait { color: #7a4b00 !important; font-weight: 600; }
.cst-view { display: inline-flex; align-items: center; gap: 5px; margin-top: 4px; font-size: 12.5px; font-weight: 600; color: var(--fin-teal-700, #0b5c64); }
</style>
