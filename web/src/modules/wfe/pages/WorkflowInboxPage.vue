<template>
  <!-- App 100 (FINOVA launcher) page 920 "My Inbox", region "FINOVA Workflow Inbox": the same markup, CSS and script behaviour,
       fed from this app's inbox API. -->
  <div class="fx" id="fxInbox">
    <div class="fx-head">
      <div>
        <h1>{{ tr('My Inbox') }}</h1>
        <p>{{ tr('Tasks for your roles. Overdue tasks are listed first.') }}</p>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openWorkflowManual(7)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
        <RouterLink class="fx-btn" :to="{ name: 'workflow-out-of-office' }"><span class="fa fa-plane" aria-hidden="true"></span>{{ tr('Out of office') }}</RouterLink>
        <button type="button" class="fx-btn" @click="load"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Refresh') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="!selected.length || busy" @click="claim(selected)">
          <span class="fa fa-hand-grab-o" aria-hidden="true"></span>{{ tr('Claim selected') }} (<span>{{ selected.length }}</span>)
        </button>
      </div>
    </div>

    <!-- the result of a claim, as a notice region: what happened, which tasks, and the next step -->
    <section v-if="notice" class="ib-note" :class="{ 'is-bad': notice.bad }" role="status" aria-live="polite">
      <span class="ib-note-ic" aria-hidden="true"><span class="fa" :class="notice.bad ? 'fa-exclamation' : 'fa-hand-grab-o'"></span></span>
      <div class="ib-note-body">
        <b>{{ notice.title }}</b>
        <span>{{ notice.detail }}</span>
        <ul v-if="notice.failed.length" class="ib-note-list"><li v-for="f in notice.failed" :key="f">{{ f }}</li></ul>
      </div>
      <RouterLink v-if="notice.open" class="fx-btn fx-btn--teal fx-btn--sm" :to="taskLink(notice.open)">
        <span class="fa fa-play" aria-hidden="true"></span>{{ tr('Open task') }}
      </RouterLink>
      <button type="button" class="ib-note-x" :aria-label="tr('Close')" @click="notice = null"><span class="fa fa-times" aria-hidden="true"></span></button>
    </section>

    <!-- whom the user covers today: their tasks are listed here too -->
    <section v-if="covering.length" class="ib-cover" role="status">
      <span class="fa fa-plane" aria-hidden="true"></span>
      <span>
        {{ tr('You are covering for') }}
        <template v-for="(c, i) in covering" :key="c.userName + (c.workflowCode || '')">
          <b>{{ c.fullName || c.userName }}</b> ({{ c.workflowCode || tr('all workflows') }}, {{ tr('until') }} {{ day(c.toDate) }})<template v-if="i < covering.length - 1">, </template>
        </template>.
        {{ tr('Their tasks are listed here with a "For" tag. Claim one to work it; a task they had claimed is taken over.') }}
      </span>
    </section>

    <!-- the inbox at a glance (Ahmed, 28 Nov 2026): count tiles filter the list; one search; workflow chips -->
    <div class="ib-tiles">
      <button v-for="v in views" :key="v.key" type="button" class="ib-tile" :class="{ on: view === v.key, bad: v.key === 'overdue' }" :aria-pressed="view === v.key" @click="view = v.key">
        <small>{{ tr(v.label) }}</small><b>{{ v.count }}</b>
      </button>
    </div>
    <div class="ib-bar">
      <input v-model="search" class="fx-search" type="search" :placeholder="tr('Search by reference, customer or step')" :aria-label="tr('Search tasks')">
      <div class="ib-wf" role="group" :aria-label="tr('Workflow')">
        <button type="button" :class="{ on: !workflow }" @click="workflow = ''">{{ tr('All') }} {{ rows.length }}</button>
        <button v-for="w in workflows" :key="w.code" type="button" :class="{ on: workflow === w.code }" @click="workflow = w.code">
          {{ w.name }} {{ rows.filter((r) => r.t.workflowCode === w.code).length }}</button>
      </div>
    </div>
    <div class="fx-card">
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th style="width:34px"><input type="checkbox" class="fx-check" aria-label="Select all unclaimed tasks" :checked="allChecked" @change="selectAll(($event.target as HTMLInputElement).checked)"></th>
              <th>{{ tr('Reference') }}</th><th>{{ tr('Customer') }}</th><th>{{ tr('Step') }}</th><th class="fx-r">{{ tr('Amount') }}</th>
              <th>{{ tr('Received') }}</th><th>{{ tr('Due') }}</th><th>{{ tr('Claimed by') }}</th><th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in shown" :key="r.t.inboxId" :class="{ 'ib-od': r.rank === 0 }">
              <td><input v-if="r.claimable" v-model="selected" type="checkbox" class="fx-check" :value="r.t.inboxId" :aria-label="`Select ${r.ref}`"></td>
              <td>
                <RouterLink :to="taskLink(r.t)"><b>{{ r.t.entityRef || r.t.subject || r.ref }}</b></RouterLink>
                <span v-if="r.t.workflowName" class="ib-wtag">{{ r.t.workflowName }}</span>
                <span v-if="r.t.onBehalfOf" class="ib-for">{{ tr('For') }} {{ r.t.onBehalfOf }}</span>
              </td>
              <td>{{ r.t.clientName || '—' }}<span v-if="r.t.clientNumber" class="fx-cell-sub">{{ r.t.clientNumber }}</span></td>
              <td><span class="ib-step">{{ r.t.stepName || r.t.stepCode || '-' }}</span><span class="fx-cell-sub">{{ r.t.assignedRole || r.t.assignedUserName || r.t.assignedGroup }}</span></td>
              <td class="fx-r">{{ full(r.t.amount, r.t.currencyCode) }}</td>
              <td>{{ received(r.t.createdOn) }}<span class="fx-cell-sub">{{ age(r.t.createdOn) }}</span></td>
              <td>
                <span class="fx-chip" :class="r.slaClass" :title="r.t.dueDate ? tr('Due') + ' ' + dueLabel(new Date(r.t.dueDate)) : ''">{{ r.sla }}</span>
                <span v-if="r.t.escalatedFrom" class="fx-chip fx-chip--esc" :title="tr('Escalated from') + ' ' + r.t.escalatedFrom">{{ tr('Escalated') }}</span>
              </td>
              <td><b v-if="r.t.isClaimedByMe" class="ib-me">{{ tr('You') }}</b><template v-else>{{ r.t.claimedByName || r.t.claimedBy || '—' }}</template></td>
              <td class="fx-r">
                <button v-if="r.claimable" type="button" class="fx-btn fx-btn--teal fx-btn--sm" :disabled="busy" @click="claim([r.t.inboxId])">{{ r.t.claimedBy ? tr('Take over') : tr('Claim') }}</button>
                <RouterLink v-else class="fx-btn fx-btn--teal fx-btn--sm" :to="taskLink(r.t)">{{ tr('Open') }} ›</RouterLink>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="!shown.length" class="fx-empty"><span class="fa fa-inbox" aria-hidden="true"></span>{{ tr('No tasks match these filters.') }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import { openWorkflowManual } from '../utils/manual'
import { computed, onMounted, ref, watch } from 'vue'
import type { WorkflowInboxTask } from '../models/workflow'
import { workflowService } from '../services/workflowService'
import { outOfOfficeApi, type CoverInfo } from '../services/outOfOfficeService'
import { tr } from '@/shared/layout/language'


type View = 'all' | 'mine' | 'unclaimed' | 'overdue'

const tasks = ref<WorkflowInboxTask[]>([])
const covering = ref<CoverInfo[]>([])
function day(d: string) { return new Date(d + 'T00:00:00').toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) }
const view = ref<View>('all')
const search = ref('')
const workflow = ref('')
const selected = ref<number[]>([])
const busy = ref(false)
type Notice = { bad: boolean; title: string; detail: string; failed: string[]; open?: WorkflowInboxTask }
const notice = ref<Notice | null>(null)

const DAY = 86400000
// the page's warning window: a task is "Due soon" within two days of its due date
const WARNING_DAYS = 2

/** The amount in full with its currency (EGP 90,000.00). */
function full(n?: number | null, ccy?: string | null) {
  if (n == null) return '—'
  return `${ccy ? ccy + ' ' : ''}${n.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`
}
function received(v?: string | null) {
  if (!v) return '—'
  return new Date(v).toLocaleString('en-GB', { day: '2-digit', month: 'short', hour: '2-digit', minute: '2-digit' })
}
function age(v?: string | null) {
  if (!v) return ''
  const ms = Date.now() - new Date(v).getTime()
  return ms >= DAY ? `${Math.floor(ms / DAY)} d ${tr('ago')}` : `${Math.max(1, Math.floor(ms / 3600000))} h ${tr('ago')}`
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
    // "due soon" starts at the step's reminder time when it has one, else two days before the due date
    const soonFrom = t.remindAt ? new Date(t.remindAt).getTime() : due ? due.getTime() - WARNING_DAYS * DAY : Infinity
    const rank = overdue ? 0 : due && now >= soonFrom ? 1 : 2
    let sla = tr('No due date')
    let slaClass = ''
    if (overdue) {
      const late = now - due.getTime()
      sla = `${tr('Overdue')} ${late >= DAY ? Math.floor(late / DAY) + 'd' : Math.max(1, Math.floor(late / 3600000)) + 'h'}`
      slaClass = 'fx-chip--bad'
    }
    else if (rank === 1) { sla = tr('Due soon'); slaClass = 'fx-chip--warn' }
    else if (due) sla = dueLabel(due)
    const actions = (t.allowedActions || 'CLAIM').toUpperCase()
    return {
      t, rank, sla, slaClass,
      ref: `#${t.taskId}`,
      unclaimed: !t.claimedBy && open,
      // a task the away user had claimed can be taken over by their delegate
      claimable: ((!t.claimedBy && open) || (!!t.onBehalfOf && !!t.claimedBy && !t.isClaimedByMe)) && actions.includes('CLAIM'),
      due: due ? due.getTime() : Number.MAX_SAFE_INTEGER,
      search: [`#${t.taskId}`, t.moduleCode, t.subject, t.entityRef, t.clientName, t.stepName].join(' ').toUpperCase()
    }
  }).sort((a, b) => a.rank - b.rank || a.due - b.due || b.t.createdOn.localeCompare(a.t.createdOn) || b.t.taskId - a.t.taskId)
})

const views = computed(() => [
  { key: 'all' as View, label: 'All tasks', count: rows.value.length },
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
  const done: WorkflowInboxTask[] = []
  for (const id of ids) {
    const t = tasks.value.find((x) => x.inboxId === id)
    try { await workflowService.claim(id); if (t) done.push(t) } catch (e) {
      failed.push(`#${t?.taskId ?? id} ${t?.subject || t?.stepName || ''}: ${e instanceof Error ? e.message : 'error'}`)
    }
  }
  busy.value = false
  await load()
  // a successful claim shows in the Status column; only failures need a notice
  if (!failed.length) return
  const name = (t: WorkflowInboxTask) => `${t.subject || t.stepName || ''} #${t.taskId}${t.entityRef ? ' · ' + t.entityRef : ''}`
  notice.value = {
    bad: done.length === 0,
    title: done.length === 0 ? tr('The task could not be claimed') : done.length === 1 ? tr('Task claimed') : `${done.length} ${tr('tasks claimed')}`,
    detail: done.length === 1 ? `${name(done[0]!)} ${tr('is now assigned to you. Open it to start work.')}`
      : done.length > 1 ? tr('They are now assigned to you and listed under Claimed by me.')
      : tr('Nothing was claimed.'),
    failed,
    open: done.length === 1 ? done[0] : undefined
  }
}

async function load() {
  selected.value = []
  notice.value = null
  try {
    tasks.value = await workflowService.getMyInbox()
    outOfOfficeApi.covering().then((x) => (covering.value = x)).catch(() => (covering.value = []))
  } catch (e) {
    notice.value = { bad: true, title: tr('Unable to load the inbox'), detail: e instanceof Error ? e.message : '', failed: [] }
  }
}

onMounted(load)
</script>

<style scoped>
.ib-cover { display: flex; gap: 10px; align-items: flex-start; margin: 0 0 12px; padding: 10px 14px; border: 1px solid #f5c79f; border-radius: 10px; background: #fff4ea; color: #7a3d0a; font-size: 13px; line-height: 1.5; }
.ib-cover > .fa { color: #E87722; margin-top: 3px; }
.ib-for { display: inline-block; margin-top: 3px; padding: 1px 8px; border-radius: 999px; background: #fff0e3; color: #b5530d; font-size: 11px; font-weight: 700; }
.ib-claim { display: inline-flex; align-items: center; gap: 6px; padding: 3px 10px; border-radius: 999px; font-size: 12px; font-weight: 700; }
.ib-claim.is-claimed { background: #FDEFE3; color: #C25E12; }
.ib-claim.is-open { background: #EEF2F4; color: #5F7787; }
.fx-chip--ok { background: var(--fin-ok-50); color: var(--fin-ok); }
.fx-chip--bad { background: var(--fin-bad-50); color: var(--fin-bad); }
/* the claim notice: a white region with a teal accent (red when nothing was claimed) */
.ib-note { display: flex; align-items: center; gap: 14px; margin-block-end: 14px; padding: 14px 16px; border-radius: 14px; background: #FFF8F1;
  border: 1px solid #F6DCC4; border-inline-start: 4px solid #E87722; box-shadow: 0 8px 22px -16px rgba(11, 37, 51, .35); animation: ib-in .2s ease-out; }
.ib-note.is-bad { border-color: #F3C9C4; border-inline-start-color: #C43325; background: #FFF6F5; }
.ib-note-ic { flex: 0 0 auto; width: 38px; height: 38px; border-radius: 50%; display: grid; place-items: center; background: #E87722; color: #fff; font-size: 15px; }
.ib-note.is-bad .ib-note-ic { background: #FDECEA; color: #C43325; }
.ib-note-body { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 2px; }
.ib-note-body b { font-size: 14.5px; font-weight: 700; color: #0B2533; }
.ib-note-body > span { font-size: 13px; color: #5F7787; }
.ib-note-list { margin: 6px 0 0; padding-inline-start: 18px; font-size: 12.5px; color: #C43325; }
.ib-note .fx-btn .fa { margin-inline-end: 6px; }
.ib-note-x { flex: 0 0 auto; width: 30px; height: 30px; border: 0; border-radius: 8px; background: transparent; color: #8AA0AB; cursor: pointer; }
.ib-note-x:hover { background: #F2F6F7; color: #0B2533; }
@keyframes ib-in { from { opacity: 0; transform: translateY(-4px); } to { opacity: 1; transform: none; } }
.fx-chip--esc { margin-inline-start: 4px; background: #E6F2F2; color: #00636D; }
.ib-tiles { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; margin-bottom: 12px; }
.ib-tile { text-align: start; padding: 12px 16px; border: 1px solid #e3e8ec; border-radius: 10px; background: #fff; cursor: pointer; }
.ib-tile small { display: block; font-size: 12.5px; color: #34495a; }
.ib-tile b { font-size: 24px; color: #1d2b36; }
.ib-tile.on { border: 2px solid var(--fin-teal-600, #0f7c86); background: #f2fafa; }
.ib-tile.bad b { color: #c0392b; }
.ib-bar { display: flex; flex-wrap: wrap; gap: 10px; align-items: center; margin-bottom: 12px; }
.ib-bar .fx-search { flex: 1; min-width: 260px; }
.ib-wf { display: flex; flex-wrap: wrap; gap: 6px; }
.ib-wf button { padding: 6px 12px; border: 1px solid #cfd8de; border-radius: 999px; background: #fff; color: #1d2b36; font-size: 13px; cursor: pointer; }
.ib-wf button.on { background: var(--fin-teal-600, #0f7c86); border-color: var(--fin-teal-600, #0f7c86); color: #fff; }
.ib-wtag { display: inline-block; margin-inline-start: 6px; padding: 1px 8px; border-radius: 999px; background: #e6f3f4; color: #0b5c64; font-size: 11px; font-weight: 700; }
.ib-step { display: inline-block; padding: 2px 10px; border-radius: 999px; background: #eef6f7; color: #0b5c64; font-weight: 600; font-size: 13px; }
.ib-me { color: #0b5c64; }
tr.ib-od td:first-child { box-shadow: inset 3px 0 #c0392b; }
@media (max-width: 900px) { .ib-tiles { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
