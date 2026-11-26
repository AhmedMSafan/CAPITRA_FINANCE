<template>
  <!-- CF_FINANCE (Ahmed, 26 Nov 2026): the step a consumer finance from a supplier is waiting at; the task holder accepts or rejects it here. -->
  <section v-if="step" class="fx-card cfs">
    <header class="cfs-head">
      <span class="cfs-ic" aria-hidden="true"><span class="fa fa-random"></span></span>
      <div>
        <h3>{{ tr('Workflow step') }}: {{ tr(step.stepName) }}</h3>
        <p v-if="step.isMine">{{ tr(HINT[step.stepCode] ?? 'Check the finance, then accept or reject the step.') }}</p>
        <p v-else>{{ tr('Waiting for') }} {{ tr(ROLE[step.stepCode] ?? 'the step holder') }}. {{ tr('Open it from the inbox to work on it.') }}</p>
      </div>
    </header>
    <div v-if="step.isMine" class="cfs-body">
      <label class="fc-field"><span>{{ tr('Comment') }} <small class="c3-muted">({{ tr('required to reject') }})</small></span>
        <textarea v-model="comment" class="fx-search cfs-comment" rows="2" maxlength="2000"></textarea></label>
      <div class="cfs-actions">
        <button type="button" class="fx-btn fc-btn-reject" :disabled="busy" @click="complete('REJECT')"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Reject') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="complete('ACCEPT')">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr(ACCEPT[step.stepCode] ?? 'Accept') }}
        </button>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { loansApi, type CfStep } from '../api/loans.api'

const props = defineProps<{ loanId: number; version: number }>()
const emit = defineEmits<{ changed: [] }>()
const route = useRoute()
const router = useRouter()

const HINT: Record<string, string> = {
  CF_CREDIT: "Check the customer, the supplier and the schedule. Approving needs the customer's compliance to be clear.",
}
const ACCEPT: Record<string, string> = { CF_CREDIT: 'Approve' }
const ROLE: Record<string, string> = {
  CF_CREDIT: 'Credit (CREDIT_OFFICER)', CF_AGREEMENT: 'Operations (OPERATIONS_OFFICER)', CF_DELIVERY: 'Operations (OPERATIONS_OFFICER)',
  CF_SUPPLIER_PAYMENT: 'Treasury (TREASURY_OFFICER)',
}

const step = ref<CfStep | null>(null)
const comment = ref('')
const busy = ref(false)
const inboxId = () => (route.query.inboxId ? Number(route.query.inboxId) : null)

async function load() {
  try {
    step.value = await loansApi.cfStep(props.loanId, inboxId())
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  }
}

async function complete(action: 'ACCEPT' | 'REJECT') {
  if (!step.value?.inboxId) return
  if (action === 'REJECT' && !comment.value.trim()) {
    showError(tr('Write why the finance is rejected.'))
    return
  }
  busy.value = true
  try {
    await loansApi.completeCfStep(props.loanId, step.value.inboxId, action, comment.value.trim() || null)
    showSuccess(tr(action === 'ACCEPT' ? 'Step accepted.' : 'Finance rejected.'))
    comment.value = ''
    // the task is done: drop it from the address so the page shows the next step as someone else's
    const query = { ...route.query }
    delete query.inboxId
    await router.replace({ query })
    emit('changed')
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  } finally {
    busy.value = false
  }
}

watch(() => [props.loanId, props.version, route.query.inboxId], load, { immediate: true })
</script>

<style scoped>
.cfs { border-inline-start: 3px solid var(--fin-teal-600); }
.cfs-head { display: flex; gap: 12px; align-items: flex-start; }
.cfs-head h3 { margin: 0; font-size: 15px; }
.cfs-head p { margin: 3px 0 0; color: var(--fin-muted); font-size: 13px; }
.cfs-ic { display: grid; place-items: center; width: 34px; height: 34px; border-radius: 9px; background: var(--fin-teal-50); color: var(--fin-teal-700); flex: none; }
.cfs-body { margin-top: 12px; display: flex; flex-direction: column; gap: 10px; }
.cfs-comment { min-height: 56px; resize: vertical; }
.cfs-actions { display: flex; gap: 8px; justify-content: flex-end; }
</style>
