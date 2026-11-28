<template>
  <!--
    The steps of a Consumer Finance supplier application, in order: data entry, then each workflow step of each run (a returned
    application starts a new round).
    Each step shows its decision: who and when, the outcome and the comment. The documents are one list (CfSupplierDocuments).
  -->
  <section class="fx-card fc-step-card">
    <header class="fc-sec-head">
      <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-sitemap"></span></span>
      <div>
        <h2>{{ tr('Workflow steps') }}</h2>
        <p>{{ tr('What was decided at each step, by whom and when.') }}</p>
      </div>
    </header>

    <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
    <ol v-else class="scr-steps">
      <li v-for="s in steps" :id="`step-${s.key}`" :key="s.key" class="scr-stepc"
          :class="{ 'scr-stepc--done': s.status === 'DONE', 'scr-stepc--open': s.status === 'OPEN', 'scr-stepc--focus': s.key === focused, 'scr-stepc--back': s.action === 'REJECT' }">
        <span class="scr-stepc-dot" aria-hidden="true"><span class="fa" :class="s.status === 'OPEN' ? 'fa-circle-o' : s.action === 'REJECT' ? 'fa-undo' : 'fa-check'"></span></span>
        <div class="scr-stepc-body">
          <div class="scr-stepc-head">
            <b>{{ tr(s.stepName) }}</b>
            <span v-if="rounds > 1" class="fx-chip fx-chip--plain">{{ tr('Round') }} {{ s.round }}</span>
            <span v-if="s.status === 'OPEN'" class="fx-chip fx-chip--warn">{{ tr('Open step') }}</span>
            <span v-else class="fx-chip" :class="s.action === 'REJECT' ? 'fx-chip--warn' : 'fx-chip--ok'">{{ tr(ACTIONS[s.action ?? ''] ?? 'Completed') }}</span>
            <span v-if="s.status === 'DONE'" class="fx-chip fx-chip--plain"><span class="fa fa-eye" aria-hidden="true"></span>{{ tr('View only') }}</span>
            <small v-if="s.doneBy" class="scr-stepc-who"><span class="fa fa-user-o" aria-hidden="true"></span> {{ s.doneBy }}<template v-if="s.doneAt"> · {{ formatDateTime(s.doneAt) }}</template></small>
          </div>

          <p v-if="s.comment" class="scr-stepc-comment" dir="auto"><span class="fa fa-comment-o" aria-hidden="true"></span> {{ s.comment }}</p>
        </div>
      </li>
    </ol>
  </section>
</template>

<script setup lang="ts">
import '@/modules/scr/components/scr.css'
import { computed, nextTick, ref, watch } from 'vue'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDateTime } from '@/modules/scr/utils/dates'
import { errorText } from '@/modules/scr/api/scr.api'
import { cfSuppliersApi, type CfStepView as ScrStepView } from '../api/cfSuppliers.api'

const props = defineProps<{
  applicationId: number
  /** Step code to open on (?step=): its last completion is highlighted. */
  focusStep: string | null
  /** Changes when the application changes, to read the steps again. */
  version: number
}>()

const ACTIONS: Record<string, string> = { ACCEPT: 'Accepted', REJECT: 'Returned', SUBMIT: 'Submitted' }

const steps = ref<ScrStepView[]>([])
const loading = ref(false)
const rounds = computed(() => Math.max(1, ...steps.value.map((s) => s.round)))
const focused = computed(() => {
  if (!props.focusStep) return null
  const done = steps.value.filter((s) => s.stepCode === props.focusStep && s.status === 'DONE')
  return done.length ? done[done.length - 1]!.key : null
})

async function load() {
  loading.value = true
  try {
    steps.value = await cfSuppliersApi.steps(props.applicationId)
    if (focused.value) {
      await nextTick()
      document.getElementById(`step-${focused.value}`)?.scrollIntoView({ behavior: 'smooth', block: 'center' })
    }
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
}

watch(() => [props.applicationId, props.version], load, { immediate: true })
</script>
