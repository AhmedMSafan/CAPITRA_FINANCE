<template>
  <!-- The workflow actions of a factoring step, inside the working page as on the portfolio steps: each button saves the
       page, asks for an optional comment, then completes the task with that action and goes back to the inbox. -->
  <template v-if="actions.length">
    <button v-for="a in actions" :key="a" type="button" class="fx-btn" :class="buttonClass(a)" :disabled="disabled || busy"
            @click="ask(a)">
      <span class="fa" :class="icon(a)" aria-hidden="true"></span>{{ tr(`Save and ${label(a)}`) }}
    </button>
  </template>

  <Teleport to="body">
    <div v-if="pending" class="fx fc fc-modal-back" role="presentation" @click.self="cancel">
      <section class="fc-modal fc-act-modal" role="dialog" aria-modal="true" aria-labelledby="fc-act-title">
        <header class="fc-act-head">
          <span class="fc-act-ic" :class="{ 'is-bad': pending === 'REJECT' }" aria-hidden="true"><span class="fa" :class="icon(pending)"></span></span>
          <div>
            <h2 id="fc-act-title">{{ tr(`Save and ${label(pending)}`) }}</h2>
            <p>{{ tr(prompt(pending)) }}</p>
          </div>
        </header>
        <label class="fc-field"><span>{{ tr('Comment (optional)') }}</span>
          <textarea v-model="comment" class="fx-search fc-textarea" rows="3" :placeholder="tr('Add a comment for the workflow history')"></textarea></label>
        <div class="fc-form-actions">
          <button type="button" class="fx-btn" :disabled="busy" @click="cancel">{{ tr('Cancel') }}</button>
          <button type="button" class="fx-btn" :class="buttonClass(pending)" :disabled="busy" @click="run">
            <span class="fa" :class="busy ? 'fa-spinner fa-spin' : icon(pending)" aria-hidden="true"></span>{{ tr(busy ? 'Working...' : `Save and ${label(pending)}`) }}</button>
        </div>
      </section>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { tr } from '@/shared/layout/language'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { workflowService } from '@/modules/wfe/services/workflowService'
import type { WorkflowInboxTask } from '@/modules/wfe/models/workflow'
import { errorText } from './stepFormat'

const props = defineProps<{
  /** Saves the page; false when the save failed (the page has shown why). */
  save: () => Promise<boolean>
  disabled?: boolean
}>()

const route = useRoute()
const router = useRouter()
const task = ref<WorkflowInboxTask | null>(null)
const pending = ref<string | null>(null)
const comment = ref('')
const busy = ref(false)

// CLAIM and START are the inbox's Claim and Start work buttons
const actions = computed(() => {
  if (task.value?.status !== 'IN_PROGRESS') return []
  return (task.value.allowedActions || 'COMPLETE').split(',').map((a) => a.trim().toUpperCase())
    .filter((a) => a && a !== 'CLAIM' && a !== 'START')
})

const LABELS: Record<string, string> = { COMPLETE: 'Complete', ACCEPT: 'Accept', APPROVE: 'Approve', REJECT: 'Reject', RETURN: 'Return' }
const label = (a: string) => LABELS[a] ?? a.charAt(0) + a.slice(1).toLowerCase()

function icon(a: string) {
  if (a === 'REJECT') return 'fa-times-circle'
  if (a === 'RETURN') return 'fa-undo'
  if (a === 'APPROVE') return 'fa-thumbs-o-up'
  return 'fa-check-circle'
}

function buttonClass(a: string) {
  if (a === 'REJECT') return 'fc-btn-reject'
  if (a === 'RETURN') return ''
  return 'fx-btn--teal'
}

function prompt(a: string) {
  if (a === 'REJECT') return 'Your changes are saved, then this step is rejected.'
  if (a === 'RETURN') return 'Your changes are saved, then this step is returned.'
  return 'Your changes are saved, then this step is completed and the workflow moves to the next step.'
}

const DONE: Record<string, string> = {
  COMPLETE: 'Step completed.', ACCEPT: 'Step accepted.', APPROVE: 'Step approved.', REJECT: 'Step rejected.', RETURN: 'Step returned.'
}

function ask(a: string) {
  comment.value = ''
  pending.value = a
}

function cancel() {
  if (!busy.value) pending.value = null
}

async function run() {
  const t = task.value
  const action = pending.value
  if (!t || !action) return
  busy.value = true
  try {
    if (!(await props.save())) {
      pending.value = null
      return
    }
    await workflowService.complete(t.inboxId, { action, comment: comment.value.trim() || null })
    pending.value = null
    showSuccess(tr(DONE[action] ?? 'Step completed.'))
    await router.push({ name: 'workflow-inbox' })
  } catch (e) {
    showError(errorText(e, 'Unable to complete workflow task.'))
  } finally {
    busy.value = false
  }
}

onMounted(async () => {
  const inboxId = Number(route.query.inboxId)
  if (!Number.isFinite(inboxId) || inboxId <= 0) return
  try {
    task.value = await workflowService.getTask(inboxId)
  } catch {
    // without the task there is nothing to complete; the page still saves
    task.value = null
  }
})
</script>
