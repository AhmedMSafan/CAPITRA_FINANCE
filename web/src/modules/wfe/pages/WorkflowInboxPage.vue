<template>
  <!-- App 100 (FINOVA launcher) page 920 "My Inbox", region "FINOVA Workflow Inbox": the same markup, CSS and script behaviour,
       fed from this app's inbox API. -->
  <div class="fx" id="fxInbox">
    <div class="fx-head">
      <div>
        <h1>My Inbox</h1>
        <p>Overdue tasks are listed first. Select unclaimed tasks to claim several at once.</p>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="load"><span class="fa fa-refresh" aria-hidden="true"></span>Refresh</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="!selected.length || busy" @click="claim(selected)">
          <span class="fa fa-hand-grab-o" aria-hidden="true"></span>Claim selected (<span>{{ selected.length }}</span>)
        </button>
      </div>
    </div>

    <div v-if="message" class="fx-msg" :class="messageBad ? 'fx-chip--bad' : 'fx-chip--ok'">{{ message }}</div>

    <div class="fx-toolbar">
      <div class="fx-seg" role="group" aria-label="Filter tasks">
        <button v-for="v in views" :key="v.key" type="button" :class="{ 'is-bad': v.key === 'overdue' }" :aria-pressed="view === v.key" @click="view = v.key">
          {{ v.label }}<em>{{ v.count }}</em>
        </button>
      </div>
      <input v-model="search" class="fx-search" type="search" placeholder="Search task, portfolio or developer" aria-label="Search tasks">
      <select v-model="workflow" class="fx-select" aria-label="Workflow">
        <option value="">All workflows</option>
        <option v-for="w in workflows" :key="w.code" :value="w.code">{{ w.name }}</option>
      </select>
    </div>

    <div class="fx-card">
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th style="width:34px"><input type="checkbox" class="fx-check" aria-label="Select all unclaimed tasks" :checked="allChecked" @change="selectAll(($event.target as HTMLInputElement).checked)"></th>
              <th>Task</th><th>Portfolio</th><th>Step</th><th class="fx-r">Value</th><th>SLA</th><th>Assignee</th><th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in shown" :key="r.t.inboxId" :class="{ 'is-overdue': r.rank === 0 }">
              <td><input v-if="r.claimable" v-model="selected" type="checkbox" class="fx-check" :value="r.t.inboxId" :aria-label="`Select ${r.ref}`"></td>
              <td>
                <RouterLink :to="taskLink(r.t)">{{ r.t.subject || r.t.stepName || '-' }}</RouterLink>
                <span class="fx-cell-sub">{{ r.ref }}<template v-if="r.t.moduleCode"> &middot; {{ r.t.moduleCode }}</template></span>
              </td>
              <td>{{ r.t.entityRef || '-' }}<span class="fx-cell-sub">{{ r.t.clientName }}</span></td>
              <td><span class="fx-chip fx-chip--teal">{{ r.t.stepName || r.t.stepCode || '-' }}</span></td>
              <td class="fx-r">{{ money(r.t.amount) }}</td>
              <td><span class="fx-chip" :class="r.slaClass">{{ r.sla }}</span></td>
              <td>
                {{ r.t.claimedByName || r.t.claimedBy || r.t.assignedRole || r.t.assignedUserName || r.t.assignedGroup }}
                <span v-if="!r.t.claimedBy" class="fx-cell-sub">Unclaimed</span>
              </td>
              <td class="fx-r">
                <button v-if="r.claimable" type="button" class="fx-btn fx-btn--teal fx-btn--sm" :disabled="busy" @click="claim([r.t.inboxId])">Claim</button>
                <RouterLink v-else class="fx-btn fx-btn--sm" :to="taskLink(r.t)">Open</RouterLink>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="!shown.length" class="fx-empty"><span class="fa fa-inbox" aria-hidden="true"></span>No tasks match these filters.</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import { computed, onMounted, ref, watch } from 'vue'
import type { WorkflowInboxTask } from '../models/workflow'
import { workflowService } from '../services/workflowService'

type View = 'all' | 'mine' | 'unclaimed' | 'overdue'

const tasks = ref<WorkflowInboxTask[]>([])
const view = ref<View>('all')
const search = ref('')
const workflow = ref('')
const selected = ref<number[]>([])
const busy = ref(false)
const message = ref('')
const messageBad = ref(false)

const DAY = 86400000
// the page's warning window: a task is "Due soon" within two days of its due date
const WARNING_DAYS = 2

// the APEX page's money() function
function money(n?: number | null) {
  if (n == null) return '-'
  const f = (v: number) => v.toLocaleString('en-US', { minimumFractionDigits: 1, maximumFractionDigits: 1 })
  if (Math.abs(n) >= 1e9) return f(n / 1e9) + 'B'
  if (Math.abs(n) >= 1e6) return f(n / 1e6) + 'M'
  if (Math.abs(n) >= 1e3) return f(n / 1e3) + 'K'
  return Math.round(n).toLocaleString('en-US')
}

function dueLabel(d: Date) {
  const mon = d.toLocaleString('en-US', { month: 'short' }).toUpperCase()
  const p = (v: number) => String(v).padStart(2, '0')
  return `${p(d.getDate())}-${mon} ${p(d.getHours())}:${p(d.getMinutes())}`
}

const rows = computed(() => {
  const now = Date.now()
  return tasks.value.map((t) => {
    const due = t.dueDate ? new Date(t.dueDate) : null
    const open = (t.status || '').toUpperCase() === 'OPEN'
    const overdue = !!due && due.getTime() < now
    const rank = overdue ? 0 : due && due.getTime() - now <= WARNING_DAYS * DAY ? 1 : 2
    let sla = 'No due date'
    let slaClass = ''
    if (overdue) { sla = `Overdue ${Math.floor((now - due.getTime()) / DAY)}d`; slaClass = 'fx-chip--bad' }
    else if (rank === 1) { sla = 'Due soon'; slaClass = 'fx-chip--warn' }
    else if (due) sla = dueLabel(due)
    const actions = (t.allowedActions || 'CLAIM').toUpperCase()
    return {
      t, rank, sla, slaClass,
      ref: `#${t.taskId}`,
      unclaimed: !t.claimedBy && open,
      claimable: !t.claimedBy && open && actions.includes('CLAIM'),
      due: due ? due.getTime() : Number.MAX_SAFE_INTEGER,
      search: [`#${t.taskId}`, t.moduleCode, t.subject, t.entityRef, t.clientName, t.stepName].join(' ').toUpperCase()
    }
  }).sort((a, b) => a.rank - b.rank || a.due - b.due || b.t.createdOn.localeCompare(a.t.createdOn) || b.t.taskId - a.t.taskId)
})

const views = computed(() => [
  { key: 'all' as View, label: 'All', count: rows.value.length },
  { key: 'mine' as View, label: 'Claimed by me', count: rows.value.filter((r) => r.t.isClaimedByMe).length },
  { key: 'unclaimed' as View, label: 'Unclaimed', count: rows.value.filter((r) => r.unclaimed).length },
  { key: 'overdue' as View, label: 'Overdue', count: rows.value.filter((r) => r.rank === 0).length }
])

const workflows = computed(() => {
  const m = new Map<string, string>()
  for (const t of tasks.value) if (t.workflowCode) m.set(t.workflowCode, t.workflowName || t.workflowCode)
  return [...m].map(([code, name]) => ({ code, name })).sort((a, b) => a.name.localeCompare(b.name))
})

const shown = computed(() => {
  const q = search.value.trim().toUpperCase()
  return rows.value.filter((r) =>
    (!q || r.search.includes(q)) &&
    (!workflow.value || r.t.workflowCode === workflow.value) &&
    (view.value === 'all' || (view.value === 'mine' && r.t.isClaimedByMe) || (view.value === 'unclaimed' && r.unclaimed) || (view.value === 'overdue' && r.rank === 0)))
})

// a filter change unticks the rows it hides, as the APEX script does
watch(shown, (s) => { const ids = new Set(s.map((r) => r.t.inboxId)); selected.value = selected.value.filter((id) => ids.has(id)) })

const claimableShown = computed(() => shown.value.filter((r) => r.claimable).map((r) => r.t.inboxId))
const allChecked = computed(() => claimableShown.value.length > 0 && claimableShown.value.every((id) => selected.value.includes(id)))
function selectAll(on: boolean) { selected.value = on ? [...claimableShown.value] : [] }

function taskLink(t: WorkflowInboxTask) {
  return { name: 'workflow-task', params: { inboxId: t.inboxId } }
}

async function claim(ids: number[]) {
  if (!ids.length) return
  busy.value = true
  const failed: string[] = []
  let claimed = 0
  for (const id of ids) {
    try { await workflowService.claim(id); claimed++ } catch (e) {
      const t = tasks.value.find((x) => x.inboxId === id)
      failed.push(`#${t?.taskId ?? id} (${e instanceof Error ? e.message : 'error'})`)
    }
  }
  message.value = `${claimed} task(s) claimed.` + (failed.length ? ` ${failed.length} could not be claimed: ${failed.join('; ')}` : '')
  messageBad.value = claimed === 0
  busy.value = false
  await load()
}

async function load() {
  selected.value = []
  try {
    tasks.value = await workflowService.getMyInbox()
  } catch (e) {
    message.value = 'Unable to load the inbox: ' + (e instanceof Error ? e.message : '')
    messageBad.value = true
  }
}

onMounted(load)
</script>

<style scoped>
/* the APEX page's apex.message success / error line */
.fx-msg { margin-block-end: 12px; padding: 10px 14px; border-radius: var(--fin-radius-sm); font-size: var(--fs-sm); font-weight: 600; }
.fx-chip--ok { background: var(--fin-ok-50); color: var(--fin-ok); }
.fx-chip--bad { background: var(--fin-bad-50); color: var(--fin-bad); }
</style>
