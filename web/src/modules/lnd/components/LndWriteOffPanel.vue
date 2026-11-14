<template>
  <!-- Write-off of a retail loan: one user asks with a reason, another approves or rejects (four eyes). Approving posts the product's
       rule LND_WRITE_OFF and makes the loan WRITTEN_OFF. -->
  <section v-if="page && (page.loanStatusCode === 'ACTIVE' || page.requests.length)" class="fx-card">
    <header class="c3-panel-head wo-head">
      <h3><span class="fa fa-ban" aria-hidden="true"></span>{{ tr('Write-off') }}</h3>
      <button v-if="page.canRequest" type="button" class="fx-btn fx-btn--sm" @click="openAsk"><span class="fa fa-paper-plane" aria-hidden="true"></span>{{ tr('Ask for write-off') }}</button>
    </header>
    <p v-if="page.loanStatusCode === 'ACTIVE'" class="c3-note">
      <span class="fa fa-info-circle" aria-hidden="true"></span>
      <template v-if="page.isWriteOffProposed">{{ tr('End of day proposed this loan for write-off.') }} </template>
      {{ tr('Written off today:') }} <b>{{ money(page.figures.totalAmount) }}</b>
      ({{ tr('principal') }} {{ money(page.figures.principalAmount) }}, {{ tr('interest') }} {{ money(page.figures.interestAmount + page.figures.interestSuspenseAmount) }},
      {{ tr('late fees') }} {{ money(page.figures.lateFeeAmount + page.figures.lateFeeSuspenseAmount) }}, {{ tr('charges and insurance') }}
      {{ money(page.figures.chargesAmount + page.figures.insuranceAmount) }}) · {{ page.figures.daysLate }} {{ tr('days late') }}
    </p>
    <div v-if="page.requests.length" class="fx-table-wrap">
      <table class="fx-table">
        <thead>
          <tr>
            <th>{{ tr('Asked') }}</th><th>{{ tr('Reason') }}</th><th class="wo-num">{{ tr('Amount') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Decision') }}</th><th>{{ tr('GL') }}</th><th></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="w in page.requests" :key="w.writeOffId">
            <td>{{ w.requestedBy }}<br><small>{{ formatDateTime(w.requestedAt) }}</small></td>
            <td>{{ w.reason }}</td>
            <td class="wo-num">{{ money(w.figures.totalAmount) }}</td>
            <td><span class="fx-chip" :class="CHIP[w.statusCode]">{{ tr(STATUS[w.statusCode] ?? w.statusCode) }}</span></td>
            <td><template v-if="w.decidedBy">{{ w.decidedBy }}<br><small>{{ w.decidedAt ? formatDateTime(w.decidedAt) : '' }}{{ w.decisionNotes ? ' · ' + w.decisionNotes : '' }}</small></template></td>
            <td>{{ w.glTranSetId ?? '' }}</td>
            <td class="wo-actions">
              <template v-if="w.canApprove">
                <button type="button" class="fx-btn fx-btn--teal fx-btn--sm" @click="openDecision(w.writeOffId, 'approve')"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Approve') }}</button>
                <button type="button" class="fx-btn fx-btn--sm" @click="openDecision(w.writeOffId, 'reject')"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Reject') }}</button>
              </template>
              <button v-if="w.canWithdraw" type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="call(() => writeOffsApi.withdraw(loanId, w.writeOffId), 'Write-off request withdrawn.')">
                <span class="fa fa-undo" aria-hidden="true"></span>{{ tr('Withdraw') }}
              </button>
              <small v-else-if="w.statusCode === 'PENDING' && w.approveBlockedReason" class="c3-muted">{{ tr(w.approveBlockedReason) }}</small>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <Crm360Dialog v-model:visible="asking" :title="tr('Ask for Write-off')" icon="fa-ban" :busy="busy" :width="560">
      <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Another user approves it. Approving posts the GL and the loan becomes written off; the amounts are worked out again on that day.') }}</p>
      <label class="fc-field" :class="{ 'c3-bad': !!errors.reason }"><span>{{ tr('Reason') }}<b> *</b></span><textarea v-model="reason" class="fx-search" rows="3" maxlength="1000"></textarea></label>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="asking = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="ask"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-paper-plane'" aria-hidden="true"></span>{{ tr('Send') }}</button>
      </template>
    </Crm360Dialog>

    <Crm360Dialog v-model:visible="deciding" :title="tr(decision === 'approve' ? 'Approve Write-off' : 'Reject Write-off')" :icon="decision === 'approve' ? 'fa-check-circle' : 'fa-times-circle'" :busy="busy" :width="520">
      <p v-if="decision === 'approve'" class="c3-note"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>{{ tr('Approving posts the GL today and the loan becomes written off. This cannot be undone here.') }}</p>
      <label class="fc-field" :class="{ 'c3-bad': !!errors.notes }"><span>{{ tr(decision === 'approve' ? 'Notes' : 'Reason') }}<b v-if="decision === 'reject'"> *</b></span><input v-model="notes" class="fx-search" maxlength="1000"></label>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="deciding = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
        <button type="button" class="fx-btn" :class="decision === 'approve' ? 'fx-btn--teal' : 'fx-btn--primary'" :disabled="busy" @click="decide">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : decision === 'approve' ? 'fa-check' : 'fa-times'" aria-hidden="true"></span>{{ tr(decision === 'approve' ? 'Approve' : 'Reject') }}
        </button>
      </template>
    </Crm360Dialog>
  </section>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { ApiError } from '@/shared/api/http'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { writeOffsApi, type WriteOffsPage } from '../api/writeOffs.api'

const props = defineProps<{ loanId: number }>()
const emit = defineEmits<{ changed: [] }>()

const STATUS: Record<string, string> = { PENDING: 'Waiting', APPROVED: 'Written off', REJECTED: 'Rejected', WITHDRAWN: 'Withdrawn' }
const CHIP: Record<string, string> = { PENDING: 'fx-chip--warn', APPROVED: 'fx-chip--bad', REJECTED: 'fx-chip--teal', WITHDRAWN: 'fx-chip--teal' }
const egp = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const money = (n: number) => egp.format(n)

const page = ref<WriteOffsPage | null>(null)
const busy = ref(false)
const asking = ref(false)
const deciding = ref(false)
const decision = ref<'approve' | 'reject'>('approve')
const decidingId = ref(0)
const reason = ref('')
const notes = ref('')
const errors = ref<Record<string, string[]>>({})

function openAsk() {
  reason.value = ''
  errors.value = {}
  asking.value = true
}
function openDecision(id: number, d: 'approve' | 'reject') {
  decidingId.value = id
  decision.value = d
  notes.value = ''
  errors.value = {}
  deciding.value = true
}

async function call(action: () => Promise<WriteOffsPage>, done: string, close?: () => void) {
  busy.value = true
  errors.value = {}
  try {
    page.value = await action()
    close?.()
    showSuccess(tr(done))
    emit('changed')
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) {
      errors.value = err.problem.errors
      showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    } else showError(err instanceof Error ? err.message : String(err))
  } finally {
    busy.value = false
  }
}
const ask = () => call(() => writeOffsApi.request(props.loanId, reason.value), 'Write-off sent. Another user must approve it.', () => (asking.value = false))
const decide = () => decision.value === 'approve'
  ? call(() => writeOffsApi.approve(props.loanId, decidingId.value, notes.value || null), 'Loan written off.', () => (deciding.value = false))
  : call(() => writeOffsApi.reject(props.loanId, decidingId.value, notes.value || null), 'Write-off rejected.', () => (deciding.value = false))

async function load() {
  try {
    page.value = await writeOffsApi.get(props.loanId)
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  }
}
onMounted(load)
defineExpose({ load })
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
.wo-head { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
.wo-num { text-align: end; white-space: nowrap; }
.wo-actions { display: flex; gap: 6px; flex-wrap: wrap; align-items: center; }
</style>
