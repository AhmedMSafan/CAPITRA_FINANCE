<template>
  <!-- App 118 (Workflow Engine) page 1 "Home": regions "Home", "Live Operations" and "Home Catalog" with the page's markup and CSS.
       Left out until this app has them: the Task actions count, the Catalogs & setup cards (entity types, functions, task actions,
       committees) and the links to the SLA monitor and the instance tracker. -->
  <div class="wx wh">
    <section class="wx-hero wh-hero">
      <div>
        <span class="wx-eyebrow"><i></i>CAPITRA Workflow Engine</span>
        <h1><span>{{ greeting }}</span>, <em>{{ userLabel }}</em></h1>
        <p>Design, version and govern every approval workflow that runs CAPITRA, from the first step to the final decision.</p>
        <div class="wh-cta">
          <RouterLink class="wx-btn wx-btn--hot" to="/workflowengine/designer/new"><span v-html="svg(PLUS, 17, 2.4)"></span><span>New workflow</span></RouterLink>
          <RouterLink class="wx-btn wx-btn--ghost" to="/workflowengine/registry"><span v-html="svg(LIST, 17)"></span><span>Workflow registry</span></RouterLink>
          <RouterLink class="wx-btn wx-btn--ghost" to="/workflow/inbox"><span v-html="svg(INBOX, 17)"></span><span>Task inbox</span></RouterLink>
        </div>
      </div>
      <div class="wh-kpis" role="list">
        <div class="wh-kpi" role="listitem"><b>{{ fmt(defs?.length) }}</b><span>Workflows &middot; <small>{{ fmt(activeDefs) }} active</small></span></div>
        <div class="wh-kpi" role="listitem"><b>{{ fmt(totals?.steps) }}</b><span>Designed steps</span></div>
        <div class="wh-kpi" role="listitem"><b>{{ fmt(totals?.flows) }}</b><span>Transitions</span></div>
      </div>
    </section>

    <div class="wh-live">
      <div class="wx-sec"><h2>Live operations</h2></div>
      <div class="wh-ops">
        <!-- 1. inbox card -->
        <section class="wx-card wh-op" style="--c:#00636D;--cl:#E6F3F4" aria-label="Task inbox">
          <div class="wh-top">
            <div class="wh-oph">
              <span class="wh-ico" v-html="svg(INBOX, 21)"></span>
              <div><h3>Task inbox</h3><p>Open tasks, the step they wait on and the role or user they are assigned to.</p></div>
            </div>
            <RouterLink class="wh-more" to="/workflow/inbox">View all</RouterLink>
          </div>
          <div class="wh-okpi">
            <div><b>{{ fmt(mineCount) }}</b><span>My inbox</span></div>
            <div><b>{{ fmt(open.length) }}</b><span>Open tasks</span></div>
            <div><b>{{ fmt(open.filter((t) => !t.claimedBy).length) }}</b><span>Not claimed</span></div>
            <div :class="{ 'is-hot': pastDue > 0 }"><b>{{ fmt(pastDue) }}</b><span>Past due</span></div>
          </div>
          <div class="wr-chips wh-seg" role="group" aria-label="Show tasks">
            <button type="button" class="wx-chip" :aria-pressed="mode === 'mine'" @click="mode = 'mine'">My tasks <em>{{ mineCount }}</em></button>
            <button type="button" class="wx-chip" :aria-pressed="mode === 'all'" @click="mode = 'all'">All tasks <em>{{ open.length }}</em></button>
          </div>
          <ul class="wh-tl">
            <li v-for="t in timeline" :key="t.inboxId">
              <RouterLink :to="{ name: 'workflow-task', params: { inboxId: t.inboxId } }">
                <span><b>{{ t.subject || t.stepName }}</b><small>{{ t.workflowName }} &middot; {{ t.stepName }}</small></span>
                <span>
                  <span class="wo-asg"><span v-if="t.assignmentType" class="wx-tag">{{ nice(t.assignmentType) }}</span><b>{{ assignee(t) }}</b></span>
                  <small>{{ t.claimedBy ? 'Claimed by ' + (t.claimedByName || t.claimedBy) : 'Not claimed' }}</small>
                </span>
                <span class="wh-r">
                  <span class="wo-pill" :class="'wo-pill--' + tsc(t.status)">{{ nice(t.status) }}</span>
                  <small class="wh-due" :class="'wh-due--' + (t.dueDate ? (new Date(t.dueDate).getTime() < now ? 'bad' : 'ok') : 'na')">{{ t.dueDate ? 'Due ' + dt(t.dueDate) : 'No due date' }}</small>
                </span>
              </RouterLink>
            </li>
            <li v-if="!timeline.length" class="wh-tl-empty">No tasks in this view.</li>
          </ul>
        </section>

        <!-- 2. SLA card -->
        <section class="wx-card wh-op" style="--c:#E87722;--cl:#FFF1E6" aria-label="SLA of claimed tasks">
          <div class="wh-top">
            <div class="wh-oph">
              <span class="wh-ico" v-html="svg(CLOCK, 21)"></span>
              <div><h3>SLA of claimed tasks</h3><p>Target, time used and time left, calculated by the SLA engine.</p></div>
            </div>
          </div>
          <div class="wh-okpi">
            <div><b>{{ fmt(sla.length) }}</b><span>Claimed</span></div>
            <div><b>{{ fmt(slaCount('ok')) }}</b><span>On track</span></div>
            <div :class="{ 'is-warm': slaCount('warn') > 0 }"><b>{{ fmt(slaCount('warn')) }}</b><span>Warning</span></div>
            <div :class="{ 'is-hot': slaCount('bad') > 0 }"><b>{{ fmt(slaCount('bad')) }}</b><span>Breached</span></div>
          </div>
          <template v-if="sla.length">
            <div class="wh-dist" role="img" :aria-label="`${slaCount('ok')} on track, ${slaCount('warn')} warning, ${slaCount('bad')} breached`">
              <i v-for="s in dist" :key="s.c" :style="{ width: s.w + '%', background: s.col }"></i>
            </div>
            <ul class="wh-sl">
              <li v-for="s in sla.slice(0, 5)" :key="s.t.inboxId">
                <RouterLink :to="{ name: 'workflow-task', params: { inboxId: s.t.inboxId } }">
                  <span class="r1"><b>{{ s.t.subject || s.t.stepName }}</b><span class="wo-pill" :class="'wo-pill--' + s.c">{{ SLA_LABEL[s.c] }}</span></span>
                  <span class="wo-meter" :class="'wo-meter--' + s.c"><i :style="{ width: Math.min(Math.max(s.pct ?? 0, 0), 100) + '%' }"></i></span>
                  <span class="r2">
                    <small>{{ s.t.claimedByName || s.t.claimedBy }} &middot; {{ s.t.stepName }}</small>
                    <small>
                      <template v-if="s.remain === null">No due date</template>
                      <b v-else-if="s.remain < 0" class="wo-neg">{{ dur(s.remain) }} over</b>
                      <template v-else>{{ dur(s.remain) }} left</template>
                      <template v-if="s.pct !== null"> &middot; {{ s.pct }}% used</template>
                    </small>
                  </span>
                </RouterLink>
              </li>
            </ul>
          </template>
          <div v-else class="wh-tl-empty">No claimed tasks right now.</div>
        </section>
      </div>

      <!-- 3. running workflow diagram -->
      <section class="wx-card wh-op wh-diag" style="--c:#5B5BD6;--cl:#EEEEFC" aria-label="Running workflow">
        <div class="wh-dtop">
          <div class="wh-oph">
            <span class="wh-ico" v-html="svg(DIAGRAM, 21)"></span>
            <div><h3>Running workflows</h3><p>Defined steps in sequence, coloured by progress, with the current step highlighted.</p></div>
          </div>
          <div v-if="runs.length" class="wh-dsel">
            <select v-model="instId" aria-label="Running workflow instance">
              <optgroup v-for="g in runGroups" :key="g.id" :label="`${g.name} #${g.id} v${g.version}`">
                <option v-for="r in g.items" :key="r.instanceId" :value="r.instanceId">{{ (r.businessReference || '#' + r.instanceId) + ' - ' + currentName(r) }}</option>
              </optgroup>
            </select>
          </div>
        </div>
        <div v-if="!runs.length" class="wh-tl-empty">No workflow instance is running right now.</div>
        <template v-else>
          <div v-if="inst" class="wh-pane">
            <div class="wh-pmeta">
              <code>{{ inst.businessReference || '#' + inst.instanceId }}</code>
              <span><b>{{ inst.workflowName }}</b> &middot; #{{ inst.workflowDefinitionId }}</span>
              <span>Started {{ dt(inst.startedAt) }} by {{ inst.startedBy.toLowerCase() }}</span>
              <span class="wo-pill" :class="'wo-pill--' + isc(inst.instanceStatus)">{{ nice(inst.instanceStatus) }}</span>
            </div>
            <ol class="wd-flow wd-flow--sm" aria-label="Workflow steps">
              <li class="wd-ev wd-ev--start"><span>Start</span></li>
              <li v-for="(n, i) in diagram" :key="n.id" class="wd-n" :class="['wd-n--' + n.state, { 'wd-n--gw': n.gateway, 'wd-p': n.state !== 'todo' && n.state !== 'skip' }]"
                  style="--sc:#00636D" :title="`${n.name} - ${n.label}`" :aria-current="n.state === 'cur' ? 'step' : undefined">
                <span class="wd-no">{{ i + 1 }}</span>
                <span class="wd-tx">
                  <b>{{ n.name }}</b>
                  <small class="wd-st">{{ n.label }}<template v-if="n.visits > 1"> &middot; {{ n.visits }} visits</template></small>
                  <small v-if="n.who && n.state === 'cur'" class="wd-who">{{ n.who }}</small>
                </span>
              </li>
              <li class="wd-ev wd-ev--end"><span>End</span></li>
            </ol>
            <div v-if="!diagram.length" class="wh-empty">This workflow definition has no designed steps.</div>
            <div v-else class="wd-prog">
              <div class="wd-bar"><i :style="{ width: Math.round((100 * doneSteps) / diagram.length) + '%' }"></i></div>
              <span><b>{{ doneSteps }}</b> of <b>{{ diagram.length }}</b> steps completed</span>
            </div>
          </div>
          <div class="wh-dfoot">
            <div class="wd-legend" aria-label="Legend">
              <span class="wd-lg wd-lg--done">Completed</span><span class="wd-lg wd-lg--cur">Current step</span><span class="wd-lg wd-lg--act">Active</span>
              <span class="wd-lg wd-lg--ret">Returned</span><span class="wd-lg wd-lg--rej">Rejected</span><span class="wd-lg wd-lg--todo">Pending</span><span class="wd-lg wd-lg--skip">Not reached</span>
            </div>
            <span>{{ runs.length > 12 ? `Showing the 12 most recently active of ${fmt(runs.length)} running instances` : `${fmt(runs.length)} running instance(s)` }}</span>
          </div>
        </template>
      </section>
    </div>

    <div class="wx-sec"><h2>Design &amp; build</h2></div>
    <div class="wh-grid wh-grid--2">
      <RouterLink class="wx-card wh-card wh-feat" to="/workflowengine/registry" style="--c:#00636D;--cl:#E6F3F4">
        <div class="wh-body">
          <div class="wh-top"><span class="wh-ico" v-html="svg(REGISTRY, 21)"></span><span class="wh-go" v-html="svg(ARROW, 16, 2.2)"></span></div>
          <div><h3>Workflow registry</h3><p>Browse every workflow definition, check its version and status, and open it in the visual designer.</p></div>
          <div class="wh-stats">
            <div><b>{{ fmt(defs?.length) }}</b><span>Definitions</span></div><div><b>{{ fmt(activeDefs) }}</b><span>Active</span></div><div><b>{{ fmt(totals?.steps) }}</b><span>Steps</span></div>
          </div>
        </div>
        <div class="wh-mini" v-html="MINI"></div>
      </RouterLink>
      <RouterLink class="wx-card wh-card wh-new" to="/workflowengine/designer/new" style="--c:#E87722;--cl:#FFF1E6">
        <div class="wh-top"><span class="wh-ico" v-html="svg(PLUS, 22, 2.4)"></span><span class="wh-go" v-html="svg(ARROW, 16, 2.2)"></span></div>
        <div><h3>Start a new workflow</h3><p>Open a blank canvas, drag steps and decisions from the palette, connect them and save.</p></div>
        <div class="wh-pal" aria-hidden="true"><span><i class="t"></i>Approval</span><span><i class="t r"></i>Review</span><span><i class="d"></i>Decision</span><span><i class="c"></i>System check</span><span><i class="t s"></i>Service</span></div>
        <div class="wh-foot"><span>Visual designer &middot; drag &amp; drop</span></div>
      </RouterLink>
    </div>

    <div class="wx-sec"><h2>Recently changed workflows</h2></div>
    <div v-if="recent.length" class="wh-recent">
      <RouterLink v-for="w in recent" :key="w.dgrmId" class="wh-rc" :to="`/workflowengine/designer/${w.dgrmId}`">
        <span class="r1"><code>{{ w.workflowCode }}</code><span class="wx-pill" :class="w.isActive === 'Y' ? 'wx-pill--ok' : 'wx-pill--off'">{{ w.isActive === 'Y' ? 'Active' : 'Inactive' }}</span></span>
        <b>{{ w.dgrmName }}</b>
        <small>v{{ w.dgrmVersion || '1' }} &middot; {{ day(w.dgrmLastUpdateOn || w.dgrmCreatedOn) }}<template v-if="w.dgrmLastUpdateBy || w.dgrmCreatedBy"> &middot; {{ (w.dgrmLastUpdateBy || w.dgrmCreatedBy).toLowerCase() }}</template></small>
      </RouterLink>
    </div>
    <div v-else-if="defs" class="wh-empty">No workflows yet. Start one with <b>New workflow</b>.</div>
  </div>
</template>

<script setup lang="ts">
import '@/shared/styles/apex-wh.css'
import { computed, onMounted, ref } from 'vue'
import { getUser } from '@/shared/auth/session'
import type { WorkflowInboxTask } from '../models/workflow'
import { workflowService } from '../services/workflowService'
import { getRunningInstances, getWorkflowDefinitions, type RunningInstance, type WorkflowDefinition } from '../services/workflowDefinitionService'
import { readFlow } from '../utils/bpmnFlow'

// icons of the APEX page
const PLUS = '<path d="M12 5v14M5 12h14"/>'
const LIST = '<path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01"/>'
const INBOX = '<path d="M22 12h-6l-2 3h-4l-2-3H2"/><path d="M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z"/>'
const CLOCK = '<circle cx="12" cy="13" r="8"/><path d="M12 9v4l2.5 2.5"/><path d="M9 2h6"/>'
const DIAGRAM = '<rect x="2" y="9" width="6" height="6" rx="1.5"/><rect x="16" y="3" width="6" height="6" rx="1.5"/><rect x="16" y="15" width="6" height="6" rx="1.5"/><path d="M8 12h4M12 6v12M12 6h4M12 18h4"/>'
const REGISTRY = '<rect x="3" y="3" width="7" height="6" rx="1.5"/><rect x="14" y="15" width="7" height="6" rx="1.5"/><path d="M6.5 9v3a3 3 0 0 0 3 3h4.5"/><circle cx="18" cy="6" r="3"/>'
const ARROW = '<path d="M5 12h14M13 5l7 7-7 7"/>'
const MINI = '<svg viewBox="0 0 300 120" aria-hidden="true"><defs><marker id="whA" markerWidth="6" markerHeight="6" refX="5" refY="3" orient="auto" markerUnits="userSpaceOnUse"><path d="M0,0 L6,3 L0,6 z" fill="#00636D"/></marker></defs><g fill="none" stroke="#00636D" stroke-width="1.6" marker-end="url(#whA)"><path d="M30 60 H52"/><path d="M118 60 H140"/><path d="M180 44 V24 H206"/><path d="M180 76 V96 H206"/></g><circle cx="20" cy="60" r="9" fill="#fff" stroke="#00636D" stroke-width="2"/><rect x="54" y="44" width="64" height="32" rx="8" fill="#fff" stroke="#B9CBD1"/><rect x="62" y="56" width="40" height="7" rx="3.5" fill="#CFE3E6"/><rect x="166" y="46" width="28" height="28" rx="3" transform="rotate(45 180 60)" fill="#FFF1E6" stroke="#E87722" stroke-width="1.6"/><rect x="208" y="8" width="72" height="32" rx="8" fill="#fff" stroke="#B9CBD1"/><rect x="216" y="20" width="44" height="7" rx="3.5" fill="#CDEBDD"/><rect x="208" y="80" width="72" height="32" rx="8" fill="#fff" stroke="#B9CBD1"/><rect x="216" y="92" width="36" height="7" rx="3.5" fill="#FAD9CF"/></svg>'
const svg = (paths: string, size: number, width = 2) =>
  `<svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="${width}" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${paths}</svg>`

// the page's helper functions (fmt, nice, dt, dur, tsc, isc)
const fmt = (n?: number | null) => (n == null ? '–' : n.toLocaleString('en-US'))
const nice = (v?: string | null) => (v || '').toLowerCase().replaceAll('_', ' ').replace(/\b\w/g, (c) => c.toUpperCase())
const dt = (v?: string | null) => {
  if (!v) return '–'
  const d = new Date(v)
  return `${String(d.getDate()).padStart(2, '0')} ${d.toLocaleString('en-US', { month: 'short' })} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}
const day = (v?: string | null) => (v ? new Date(v).toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) : '–')
function dur(h: number) {
  const m = Math.round(Math.abs(h) * 60)
  return m >= 1440 ? `${Math.trunc(m / 1440)}d ${Math.trunc((m % 1440) / 60)}h` : m >= 60 ? `${Math.trunc(m / 60)}h ${m % 60}m` : `${m}m`
}
const tsc = (s: string) => ({ OPEN: 'open', CLAIMED: 'claim', IN_PROGRESS: 'prog', COMPLETED: 'ok', REASSIGNED: 'ret' })[s.toUpperCase()] ?? 'na'
const isc = (s: string) => ({ IN_PROGRESS: 'prog', COMPLETED: 'ok', REJECTED: 'bad', CANCELLED: 'na', RETURNED: 'ret', SUSPENDED: 'warn' })[s.toUpperCase()] ?? 'open'

const user = getUser()
const userLabel = nice(user?.userName || '')
const hour = new Date().getHours()
const greeting = hour < 12 ? 'Good morning' : hour < 18 ? 'Good afternoon' : 'Good evening'
const now = Date.now()
const HOUR = 3600000

const defs = ref<WorkflowDefinition[] | null>(null)
const open = ref<WorkflowInboxTask[]>([])
const myIds = ref<Set<number>>(new Set())
const runs = ref<RunningInstance[]>([])
const instId = ref<number | null>(null)
const mode = ref<'mine' | 'all'>('all')

const flows = computed(() => new Map((defs.value ?? []).map((d) => [d.dgrmId, readFlow(d.dgrmContent)])))
const activeDefs = computed(() => defs.value && defs.value.filter((d) => d.isActive === 'Y').length)
const totals = computed(() => defs.value && {
  steps: [...flows.value.values()].reduce((n, f) => n + f.steps.length, 0),
  flows: [...flows.value.values()].reduce((n, f) => n + f.flows, 0)
})
const recent = computed(() =>
  [...(defs.value ?? [])].sort((a, b) => (b.dgrmLastUpdateOn || b.dgrmCreatedOn).localeCompare(a.dgrmLastUpdateOn || a.dgrmCreatedOn) || b.dgrmId - a.dgrmId).slice(0, 4))

// inbox card: tasks by due date (no due date last), at most 7 per view
const byDue = (a: WorkflowInboxTask, b: WorkflowInboxTask) =>
  (a.dueDate ? new Date(a.dueDate).getTime() : Infinity) - (b.dueDate ? new Date(b.dueDate).getTime() : Infinity) || a.taskId - b.taskId
const mineCount = computed(() => open.value.filter((t) => myIds.value.has(t.inboxId)).length)
const pastDue = computed(() => open.value.filter((t) => t.dueDate && new Date(t.dueDate).getTime() < now).length)
const timeline = computed(() => open.value.filter((t) => mode.value === 'all' || myIds.value.has(t.inboxId)).sort(byDue).slice(0, 7))
function assignee(t: WorkflowInboxTask) {
  return (t.assignmentType || '').toUpperCase() === 'ROLE' ? t.assignedRole || t.assignedTo || '–' : t.assignedTo || t.assignedRole || '–'
}

// SLA card: claimed tasks, from the task due date (start = task created)
type SlaClass = 'ok' | 'warn' | 'bad' | 'na'
const SLA_LABEL: Record<SlaClass, string> = { ok: 'On track', warn: 'Warning', bad: 'Breached', na: 'No SLA' }
const sla = computed(() => open.value
  .filter((t) => t.claimedBy && ['CLAIMED', 'IN_PROGRESS'].includes(t.status.toUpperCase()))
  .map((t) => {
    const start = new Date(t.createdOn).getTime()
    const due = t.dueDate ? new Date(t.dueDate).getTime() : null
    const target = due === null ? null : (due - start) / HOUR
    const c: SlaClass = due === null ? 'na' : now > due ? 'bad' : 'ok'
    return { t, c, remain: due === null ? null : (due - now) / HOUR, pct: target && target > 0 ? Math.round((100 * (now - start)) / HOUR / target) : null }
  })
  .sort((a, b) => ['bad', 'warn', 'ok', 'na'].indexOf(a.c) - ['bad', 'warn', 'ok', 'na'].indexOf(b.c) || (a.remain ?? Infinity) - (b.remain ?? Infinity) || a.t.taskId - b.t.taskId))
const slaCount = (c: SlaClass) => sla.value.filter((s) => s.c === c).length
const dist = computed(() => ([['ok', '#2BA877'], ['warn', '#F59A3C'], ['bad', '#E5484D'], ['na', '#9DB0B8']] as const)
  .map(([c, col]) => ({ c, col, w: (100 * slaCount(c)) / (sla.value.length || 1) }))
  .filter((s) => s.w > 0))

// running workflows: the 12 most recently active, grouped by workflow
const shownRuns = computed(() => runs.value.slice(0, 12))
const runGroups = computed(() => {
  const groups = new Map<number, { id: number; name: string; version: string; items: RunningInstance[] }>()
  for (const r of [...shownRuns.value].sort((a, b) => a.workflowName.toUpperCase().localeCompare(b.workflowName.toUpperCase()) || a.workflowDefinitionId - b.workflowDefinitionId)) {
    if (!groups.has(r.workflowDefinitionId)) groups.set(r.workflowDefinitionId, { id: r.workflowDefinitionId, name: r.workflowName, version: r.versionNo, items: [] })
    groups.get(r.workflowDefinitionId)!.items.push(r)
  }
  return [...groups.values()]
})
const inst = computed(() => runs.value.find((r) => r.instanceId === instId.value) ?? null)
function currentName(r: RunningInstance) {
  return r.steps.find((s) => s.elementId === r.currentElementId)?.stepName || nice(r.instanceStatus)
}

// the page's wf_diagram: the designed steps in order, overlaid with the instance's step history
const diagram = computed(() => {
  const r = inst.value
  if (!r) return []
  const flow = flows.value.get(r.workflowDefinitionId)
  return (flow?.steps ?? []).map((s) => {
    const visits = r.steps.filter((x) => x.elementId === s.id)
    const last = visits[visits.length - 1]
    const u = `${last?.stepStatus ?? ''} ${last?.outcome ?? ''}`.toUpperCase()
    let state: string
    let label: string
    if (s.id === r.currentElementId) { state = 'cur'; label = 'Current step' }
    else if (!last) { state = 'todo'; label = 'Pending' }
    else if (u.includes('REJECT')) { state = 'rej'; label = 'Rejected' }
    else if (u.includes('RETURN')) { state = 'ret'; label = 'Returned' }
    else if (u.includes('CANCEL') || u.includes('SKIP')) { state = 'skip'; label = nice(last.stepStatus) }
    else if (last.completedAt || u.includes('COMPLET') || u.includes('APPROV')) { state = 'done'; label = 'Completed' }
    else { state = 'act'; label = nice(last.stepStatus) || 'Active' }
    let who = ''
    if (state === 'cur' || state === 'act') {
      const task = open.value.find((t) => t.instanceId === r.instanceId && t.stepCode && s.id.endsWith(t.stepCode))
        ?? open.value.find((t) => t.instanceId === r.instanceId)
      if (task) who = task.claimedBy ? 'Claimed by ' + (task.claimedByName || task.claimedBy) : 'Assigned to ' + assignee(task)
    }
    return { id: s.id, name: s.name, gateway: s.gateway, state, label, visits: visits.length, who }
  })
})
const doneSteps = computed(() => diagram.value.filter((n) => n.state === 'done').length)

onMounted(async () => {
  const [d, all, mine, running] = await Promise.allSettled([getWorkflowDefinitions(''), workflowService.getAllOpen(), workflowService.getMyInbox(), getRunningInstances()])
  if (d.status === 'fulfilled') defs.value = d.value
  if (all.status === 'fulfilled') open.value = all.value
  if (mine.status === 'fulfilled') {
    myIds.value = new Set(mine.value.map((t) => t.inboxId))
    if (all.status !== 'fulfilled') open.value = mine.value
  }
  if (running.status === 'fulfilled') {
    runs.value = running.value
    instId.value = running.value[0]?.instanceId ?? null
  }
  mode.value = mineCount.value > 0 ? 'mine' : 'all'
})
</script>
