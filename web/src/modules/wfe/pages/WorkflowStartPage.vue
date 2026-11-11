<template>
  <!-- Start a request (K2-style): the user-defined workflows, each on its document; fill it in and send it for approval. -->
  <div class="fx" id="fxStart">
    <div class="fx-head">
      <div>
        <h1>{{ tr('Start a request') }}</h1>
        <p>{{ tr('Choose a request, fill in its document and send it. It goes to the first approver the workflow names; you see the answer below, in My requests.') }}</p>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openWorkflowManual(8)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
        <RouterLink class="fx-btn" :to="{ name: 'workflow-inbox' }"><span class="fa fa-inbox" aria-hidden="true"></span>{{ tr('My Inbox') }}</RouterLink>
      </div>
    </div>

    <section v-if="message" class="wf-msg" :class="{ 'is-bad': message.bad }" role="status">
      <span class="fa" :class="message.bad ? 'fa-exclamation-circle' : 'fa-check-circle'" aria-hidden="true"></span>
      <span>{{ message.text }}</span>
      <RouterLink v-if="started?.inboxId" class="fx-btn fx-btn--sm" :to="{ name: 'workflow-task', params: { inboxId: started.inboxId } }">{{ tr('Open the task') }}</RouterLink>
      <button type="button" class="wf-x" :aria-label="tr('Close')" @click="message = null"><span class="fa fa-times" aria-hidden="true"></span></button>
    </section>

    <div class="st-grid">
      <div class="fx-card st-list">
        <button v-for="w in items" :key="w.workflowCode" type="button" class="st-row" :class="{ on: chosen?.workflowCode === w.workflowCode }" @click="choose(w)">
          <span class="st-icon fa fa-play-circle-o" aria-hidden="true"></span>
          <span class="st-text">
            <b>{{ w.workflowName }}</b>
            <small>{{ w.description || w.workflowCode }}</small>
          </span>
        </button>
        <div v-if="!items.length && !loading" class="fx-empty"><span class="fa fa-play-circle-o" aria-hidden="true"></span>{{ tr('No request can be started yet. In the workflow designer, make a workflow user-defined, link its document and release it.') }}</div>
        <div v-if="issues.length" class="st-issues">
          <h3><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>{{ tr('Not ready to start') }}</h3>
          <RouterLink v-for="i in issues" :key="i.workflowDefinitionId" class="st-issue" :to="{ name: 'workflow-engine-designer', params: { id: i.workflowDefinitionId } }">
            <b>{{ i.workflowName }} <small>{{ i.workflowCode }}</small></b>
            <span>{{ tr(i.reason) }}</span>
          </RouterLink>
        </div>
      </div>

      <div v-if="view" class="fx-card fx-card-pad st-form">
        <h2>{{ view.formName }}</h2>
        <p class="st-sub">{{ view.description || chosen?.workflowName }}</p>

        <label class="st-ref">
          <span>{{ tr('Business reference (optional)') }}</span>
          <input v-model="reference" class="fx-search" maxlength="100" :placeholder="tr('e.g. a contract or request number')">
        </label>

        <WorkflowFormView :view="view" v-model="values" :errors="errors" id-prefix="start" />

        <div class="st-actions">
          <button type="button" class="fx-btn" :disabled="busy" @click="reset">{{ tr('Clear') }}</button>
          <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="start">
            <span class="fa fa-play" aria-hidden="true"></span>{{ tr('Send request') }}
          </button>
        </div>
      </div>
      <div v-else class="fx-card fx-card-pad st-blank">
        <span class="fa fa-hand-o-left" aria-hidden="true"></span>
        <p>{{ tr('Choose a request to see its document.') }}</p>
      </div>
    </div>

    <!-- the requests I sent: where each one is now, and the decision on it -->
    <div class="fx-card st-mine">
      <div class="st-mine-head"><h2>{{ tr('My requests') }}</h2><span class="fx-cell-sub">{{ mine.length }}</span></div>
      <div class="st-table-wrap">
        <table v-if="mine.length" class="st-table">
          <thead>
            <tr><th>#</th><th>{{ tr('Request') }}</th><th>{{ tr('Sent') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Now / decided by') }}</th><th>{{ tr('Comment') }}</th></tr>
          </thead>
          <tbody>
            <tr v-for="r in mine" :key="r.instanceId">
              <td class="st-num">{{ r.instanceId }}</td>
              <td><b>{{ r.workflowName }}</b><small v-if="r.businessReference">{{ r.businessReference }}</small></td>
              <td>{{ fmt(r.startedAt) }}</td>
              <td><span class="st-chip" :class="`st-chip--${r.status.toLowerCase()}`">{{ tr(statusText[r.status] ?? r.status) }}</span></td>
              <td>
                <template v-if="r.status === 'IN_PROGRESS'">{{ tr('Waiting at') }} {{ r.currentStep ?? '-' }}</template>
                <template v-else-if="r.lastActionBy">{{ r.lastActionBy }}<small>{{ fmt(r.lastActionAt) }}</small></template>
                <template v-else>-</template>
              </td>
              <td class="st-comment">{{ r.comment || '-' }}</td>
            </tr>
          </tbody>
        </table>
        <div v-else class="fx-empty"><span class="fa fa-paper-plane-o" aria-hidden="true"></span>{{ tr('You have not sent a request yet.') }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import { openWorkflowManual } from '../utils/manual'
import { onMounted, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import WorkflowFormView from '../components/WorkflowFormView.vue'
import { apiErrorText, apiFieldErrors, formApi, type FormView, type MyRequest, type StartFormItem, type StartIssue, type Started } from '../services/formService'

const mine = ref<MyRequest[]>([])
const statusText: Record<string, string> = { IN_PROGRESS: 'In progress', APPROVED: 'Approved', REJECTED: 'Rejected', COMPLETED: 'Completed', FAULTED: 'Stopped by an error' }
function fmt(d: string | null) {
  return d ? new Date(d).toLocaleString(undefined, { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' }) : ''
}
async function loadMine() {
  try { mine.value = await formApi.myRequests() } catch { mine.value = [] }
}

const items = ref<StartFormItem[]>([])
// designers also see the user-defined workflows that are not offered yet, and why
const issues = ref<StartIssue[]>([])
async function loadIssues() {
  if (!hasPermission('WFE.DEFINITION.VIEW')) return
  try { issues.value = await formApi.startIssues() } catch { issues.value = [] }
}
const chosen = ref<StartFormItem | null>(null)
const view = ref<FormView | null>(null)
const values = ref<Record<string, string | null>>({})
const errors = ref<Record<string, string>>({})
const reference = ref('')
const busy = ref(false)
const loading = ref(true)
const started = ref<Started | null>(null)
const message = ref<{ text: string; bad: boolean } | null>(null)

function initialValues(v: FormView) {
  return Object.fromEntries(v.controls.filter((c) => c.field).map((c) => [c.field as string, c.value]))
}

async function choose(w: StartFormItem) {
  chosen.value = w
  errors.value = {}
  try {
    view.value = await formApi.startForm(w.workflowCode)
    values.value = initialValues(view.value)
  } catch (e) {
    view.value = null
    message.value = { bad: true, text: apiErrorText(e) }
  }
}

function reset() {
  if (view.value) values.value = initialValues(view.value)
  reference.value = ''
  errors.value = {}
}

async function start() {
  if (!chosen.value) return
  busy.value = true
  errors.value = {}
  started.value = null
  try {
    const r = await formApi.start(chosen.value.workflowCode, reference.value.trim() || null, values.value)
    started.value = r
    message.value = {
      bad: false,
      text: `${tr('Sent')} ${chosen.value.workflowName} (#${r.instanceId}).` +
        (r.nextStep ? ` ${tr('The next task,')} ${r.nextStep}, ${tr('went to')} ${r.nextAssignee ?? tr('the workflow\'s users')}.` : ` ${tr('It has no task waiting.')}`)
    }
    reset()
    await loadMine()
  } catch (e) {
    errors.value = apiFieldErrors(e)
    message.value = { bad: true, text: apiErrorText(e) }
  } finally {
    busy.value = false
  }
}

onMounted(async () => {
  void loadMine()
  void loadIssues()
  try {
    items.value = await formApi.startList()
    const only = items.value.length === 1 ? items.value[0] : undefined
    if (only) await choose(only)
  } catch (e) {
    message.value = { bad: true, text: apiErrorText(e) }
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.st-issues { border-top: 1px solid #e6eef1; padding: 12px 16px 14px; display: grid; gap: 8px; }
.st-issues h3 { display: flex; align-items: center; gap: 8px; margin: 0; font-size: 13px; color: #b45a12; text-transform: uppercase; letter-spacing: .04em; }
.st-issue { display: grid; gap: 2px; padding: 9px 11px; border: 1px solid #f6d7bd; border-left: 3px solid #E87722; border-radius: 8px; background: #fff8f2; color: #33475b; text-decoration: none; font-size: 13px; }
.st-issue:hover { background: #fff1e4; }
.st-issue b { color: #00636D; }
.st-issue small { color: #5f7787; font-weight: 400; }
.wf-msg { display: flex; align-items: center; gap: 10px; margin: 0 0 14px; padding: 11px 14px; border-radius: 10px; font-size: 13.5px; background: #e4f6ee; border: 1px solid #bfe5d2; color: #0e5a3c; }
.wf-msg.is-bad { background: #fdecea; border-color: #f3c3bc; color: #8a2416; }
.wf-msg > span:nth-child(2) { flex: 1; }
.wf-x { border: 0; background: none; color: inherit; cursor: pointer; }
.st-grid { display: grid; grid-template-columns: minmax(240px, 320px) minmax(0, 720px); gap: 16px; align-items: start; }
.st-row { display: flex; align-items: center; gap: 12px; width: 100%; padding: 12px 16px; border: 0; border-top: 1px solid #e6eef1; background: none; text-align: left; cursor: pointer; font: inherit; }
.st-row:first-child { border-top: 0; }
.st-row:hover { background: #f5f9fa; }
.st-row.on { background: #e7f3f2; box-shadow: inset 3px 0 0 #00636D; }
.st-icon { font-size: 22px; color: #00636D; }
.st-row.on .st-icon { color: #E87722; }
.st-text { display: grid; gap: 2px; min-width: 0; }
.st-text b { color: #12324a; font-size: 13.5px; }
.st-text small { color: #5f7787; font-size: 12px; }
.st-form { min-width: 0; }
.st-sub { margin: 0 0 16px; color: #5f7787; font-size: 12.5px; }
.st-ref { display: grid; gap: 5px; margin-bottom: 16px; padding-bottom: 16px; border-bottom: 1px solid #e6eef1; }
.st-ref > span { color: #12324a; font-size: 12px; font-weight: 700; }
.st-ref .fx-search { width: 100%; min-width: 0; max-width: 100%; flex: none; }
.st-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 18px; }
.st-blank { display: grid; justify-items: center; gap: 6px; padding: 48px 18px; color: #5f7787; text-align: center; }
.st-blank .fa { font-size: 26px; color: #9fbfc4; }
.st-mine { margin-top: 16px; max-width: 1056px; }
.st-mine-head { display: flex; align-items: baseline; justify-content: space-between; padding: 14px 16px 8px; }
.st-mine-head h2 { margin: 0; }
.st-table-wrap { overflow-x: auto; }
.st-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.st-table th { text-align: left; padding: 8px 16px; color: #5f7787; font-size: 11.5px; font-weight: 700; text-transform: uppercase; letter-spacing: .03em; border-top: 1px solid #e6eef1; white-space: nowrap; }
.st-table td { padding: 10px 16px; border-top: 1px solid #e6eef1; color: #12324a; vertical-align: top; }
.st-table td small { display: block; color: #5f7787; font-size: 11.5px; }
.st-num { color: #5f7787 !important; font-variant-numeric: tabular-nums; }
.st-comment { max-width: 280px; color: #3d5666 !important; }
.st-chip { display: inline-block; padding: 3px 10px; border-radius: 999px; font-size: 11.5px; font-weight: 700; white-space: nowrap; }
.st-chip--in_progress { background: #fff1e4; color: #a24f0c; }
.st-chip--approved, .st-chip--completed { background: #e4f6ee; color: #0e5a3c; }
.st-chip--rejected, .st-chip--faulted { background: #fdecea; color: #8a2416; }
@media (max-width: 900px) { .st-grid { grid-template-columns: minmax(0, 1fr); } }
</style>
