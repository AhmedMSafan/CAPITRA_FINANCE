<template>
  <!-- Process monitor (K2's View Flow and process reports): where every request is, one request on its diagram, and time per step. -->
  <div class="fx" id="fxMonitor">
    <div class="fx-head">
      <div>
        <h1>{{ tr('Process monitor') }}</h1>
        <p>{{ tr('Where every request is, who has it and for how long. Open one to see it on its diagram, step by step.') }}</p>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openWorkflowManual(13)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
        <RouterLink class="fx-btn" to="/workflowengine/registry"><span class="fa fa-list" aria-hidden="true"></span>{{ tr('Workflow registry') }}</RouterLink>
      </div>
    </div>

    <div class="pm-tabs" role="tablist">
      <button type="button" role="tab" :aria-selected="tab === 'requests'" @click="tab = 'requests'"><span class="fa fa-sitemap" aria-hidden="true"></span>{{ tr('Requests') }}</button>
      <button type="button" role="tab" :aria-selected="tab === 'times'" @click="tab = 'times'; loadTimes()"><span class="fa fa-clock-o" aria-hidden="true"></span>{{ tr('Time per step') }}</button>
    </div>

    <section v-if="error" class="wf-msg is-bad" role="alert"><span class="fa fa-exclamation-circle" aria-hidden="true"></span><span>{{ error }}</span></section>

    <!-- REQUESTS -->
    <template v-if="tab === 'requests'">
      <div class="fx-card fx-card-pad">
        <div class="pm-filters">
          <select v-model="status" class="fx-select" :aria-label="tr('Status')" @change="loadList">
            <option value="running">{{ tr('Running') }}</option>
            <option value="finished">{{ tr('Finished') }}</option>
            <option value="all">{{ tr('All') }}</option>
          </select>
          <select v-model="workflowId" class="fx-select" :aria-label="tr('Workflow')" @change="loadList(); loadTimes(true)">
            <option :value="null">{{ tr('All workflows') }}</option>
            <option v-for="d in defs" :key="d.id" :value="d.id">{{ d.name }}</option>
          </select>
          <input v-model="search" class="fx-search" :placeholder="tr('Reference or started by')" @keyup.enter="loadList">
          <span class="pm-count">{{ rows.length }} {{ tr('requests') }}<template v-if="overdueCount"> · <b class="pm-late">{{ overdueCount }} {{ tr('overdue') }}</b></template></span>
        </div>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead><tr>
              <th>{{ tr('Request') }}</th><th>{{ tr('Workflow') }}</th><th>{{ tr('Now at') }}</th><th>{{ tr('With') }}</th>
              <th class="fx-r">{{ tr('Waiting') }}</th><th>{{ tr('Started') }}</th><th class="fx-r">{{ tr('Total time') }}</th><th>{{ tr('Status') }}</th>
            </tr></thead>
            <tbody>
              <tr v-for="r in rows" :key="r.instanceId" class="pm-row" :class="{ 'is-overdue': r.overdue, on: view?.summary.instanceId === r.instanceId }" tabindex="0"
                  @click="open(r.instanceId)" @keyup.enter="open(r.instanceId)">
                <td><a href="#" @click.prevent>{{ r.businessReference || '#' + r.instanceId }}</a><span class="fx-cell-sub">#{{ r.instanceId }}</span></td>
                <td>{{ r.workflowName }}</td>
                <td>{{ r.currentStep || '-' }}</td>
                <td>{{ r.waitingFor || '-' }}</td>
                <td class="fx-r"><span :class="{ 'pm-late': r.overdue }">{{ duration(r.waitingHours) }}</span></td>
                <td>{{ when(r.startedAt) }}<span class="fx-cell-sub">{{ r.startedBy }}</span></td>
                <td class="fx-r">{{ duration(r.totalHours) }}</td>
                <td><span class="fx-chip" :class="chip(r)">{{ statusText(r) }}</span></td>
              </tr>
              <tr v-if="!rows.length"><td colspan="8" class="pm-empty">{{ loading ? tr('Loading...') : tr('No requests match these filters.') }}</td></tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- the process view of the chosen request -->
      <div v-if="view" ref="viewCard" class="fx-card fx-card-pad pm-view">
        <div class="pm-view-head">
          <div>
            <h2>{{ view.summary.businessReference || '#' + view.summary.instanceId }} <span class="fx-chip" :class="chip(view.summary)">{{ statusText(view.summary) }}</span></h2>
            <p>{{ view.summary.workflowName }} · {{ tr('started') }} {{ when(view.summary.startedAt) }} {{ tr('by') }} {{ view.summary.startedBy }} · {{ tr('total') }} {{ duration(view.summary.totalHours) }}</p>
          </div>
          <button type="button" class="fx-btn fx-btn--sm" @click="view = null"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
        </div>
        <div ref="diagram" class="pm-diagram" :aria-label="tr('Process diagram')"></div>
        <div class="pm-legend">
          <span class="lg lg-done">{{ tr('Done') }}</span><span class="lg lg-cur">{{ tr('Now here') }}</span><span class="lg lg-rej">{{ tr('Rejected') }}</span>
          <span class="lg lg-todo">{{ tr('Not reached') }}</span><span class="lg lg-flow">{{ tr('Path taken') }}</span>
        </div>

        <div class="pm-split">
          <div>
            <h3>{{ tr('Steps') }}</h3>
            <div class="fx-table-wrap">
              <table class="fx-table">
                <thead><tr><th>{{ tr('Step') }}</th><th>{{ tr('Arrived') }}</th><th>{{ tr('Finished') }}</th><th>{{ tr('By / with') }}</th><th class="fx-r">{{ tr('Time') }}</th><th>{{ tr('Outcome') }}</th></tr></thead>
                <tbody>
                  <tr v-for="(s, i) in humanSteps" :key="i" :class="{ 'pm-cur': !s.completedAt }">
                    <td><b>{{ s.stepName || s.elementId }}</b></td>
                    <td>{{ when(s.enteredAt) }}</td>
                    <td>{{ s.completedAt ? when(s.completedAt) : tr('Waiting') }}</td>
                    <td>{{ s.completedBy || s.tasks.filter(t => isOpen(t.status)).map(t => t.claimedBy || t.assignedTo).join(', ') || '-' }}
                      <span v-if="s.tasks.length > 1" class="fx-cell-sub">{{ s.tasks.map(t => `${t.assignedTo}: ${taskText(t)}`).join(' · ') }}</span></td>
                    <td class="fx-r"><span :class="{ 'pm-late': lateStep(s) }">{{ duration(s.hours) }}</span></td>
                    <td>{{ s.outcome || '-' }}</td>
                  </tr>
                  <tr v-if="!humanSteps.length"><td colspan="6" class="pm-empty">{{ tr('No human step yet.') }}</td></tr>
                </tbody>
              </table>
            </div>
            <template v-if="view.data.length">
              <h3>{{ tr('Request data') }}</h3>
              <dl class="pm-data"><template v-for="d in view.data" :key="d.name"><dt>{{ d.label }}</dt><dd>{{ d.value || '-' }}</dd></template></dl>
            </template>
          </div>
          <div>
            <h3>{{ tr('History') }}</h3>
            <ol class="pm-hist">
              <li v-for="(h, i) in view.history" :key="i" :class="'ev-' + evKind(h.eventType)">
                <time>{{ when(h.at) }}</time>
                <b>{{ eventText(h.eventType) }}<template v-if="h.action && !['CLAIM', 'START'].includes(h.action)"> · {{ h.action }}</template></b>
                <span>{{ [h.by, h.details].filter(Boolean).join(' · ') }}</span>
              </li>
            </ol>
          </div>
        </div>
      </div>
    </template>

    <!-- TIME PER STEP -->
    <template v-else>
      <div class="fx-card fx-card-pad">
        <div class="pm-filters">
          <label class="pm-date">{{ tr('From') }} <input v-model="from" type="date" class="fx-search" @change="loadTimes(true)"></label>
          <label class="pm-date">{{ tr('To') }} <input v-model="to" type="date" class="fx-search" @change="loadTimes(true)"></label>
          <select v-model="workflowId" class="fx-select" :aria-label="tr('Workflow')" @change="loadTimes(true); loadList()">
            <option :value="null">{{ tr('All workflows') }}</option>
            <option v-for="d in defs" :key="d.id" :value="d.id">{{ d.name }}</option>
          </select>
          <span class="pm-count">{{ tr('Steps finished in the period; waiting and overdue are as of now.') }}</span>
        </div>
        <h3>{{ tr('Time per step') }}</h3>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead><tr>
              <th>{{ tr('Workflow') }}</th><th>{{ tr('Step') }}</th><th class="fx-r">{{ tr('Finished') }}</th><th class="pm-barcol">{{ tr('Average time') }}</th>
              <th class="fx-r">{{ tr('Median') }}</th><th class="fx-r">{{ tr('Longest') }}</th><th class="fx-r">{{ tr('Waiting now') }}</th><th class="fx-r">{{ tr('Overdue now') }}</th><th class="fx-r">{{ tr('Finished late') }}</th>
            </tr></thead>
            <tbody>
              <tr v-for="s in times?.steps ?? []" :key="s.workflowDefinitionId + s.elementId">
                <td>{{ s.workflowName }}</td>
                <td><b>{{ s.stepName }}</b></td>
                <td class="fx-r">{{ s.completed }}</td>
                <td class="pm-barcol"><span class="pm-bar"><i :style="{ width: barWidth(s.averageHours) }"></i></span><span class="pm-barv">{{ s.completed ? duration(s.averageHours) : '-' }}</span></td>
                <td class="fx-r">{{ s.completed ? duration(s.medianHours) : '-' }}</td>
                <td class="fx-r">{{ s.completed ? duration(s.longestHours) : '-' }}</td>
                <td class="fx-r">{{ s.waiting || '-' }}</td>
                <td class="fx-r"><span :class="{ 'pm-late': s.overdue }">{{ s.overdue || '-' }}</span></td>
                <td class="fx-r"><span :class="{ 'pm-late': s.late }">{{ s.late || '-' }}</span></td>
              </tr>
              <tr v-if="!times?.steps.length"><td colspan="9" class="pm-empty">{{ loading ? tr('Loading...') : tr('No step was finished or is waiting in this period.') }}</td></tr>
            </tbody>
          </table>
        </div>
        <h3>{{ tr('Time per person') }}</h3>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead><tr><th>{{ tr('Person') }}</th><th class="fx-r">{{ tr('Tasks finished') }}</th><th class="pm-barcol">{{ tr('Average time') }}</th><th class="fx-r">{{ tr('Longest') }}</th><th class="fx-r">{{ tr('Finished late') }}</th><th class="fx-r">{{ tr('Open now') }}</th></tr></thead>
            <tbody>
              <tr v-for="p in times?.people ?? []" :key="p.userName">
                <td><b>{{ p.userName }}</b></td>
                <td class="fx-r">{{ p.completed }}</td>
                <td class="pm-barcol"><span class="pm-bar"><i :style="{ width: barWidth(p.averageHours) }"></i></span><span class="pm-barv">{{ p.completed ? duration(p.averageHours) : '-' }}</span></td>
                <td class="fx-r">{{ p.completed ? duration(p.longestHours) : '-' }}</td>
                <td class="fx-r"><span :class="{ 'pm-late': p.late }">{{ p.late || '-' }}</span></td>
                <td class="fx-r">{{ p.open || '-' }}</td>
              </tr>
              <tr v-if="!times?.people.length"><td colspan="6" class="pm-empty">{{ tr('Nobody finished a task in this period.') }}</td></tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import 'bpmn-js/dist/assets/diagram-js.css'
import 'bpmn-js/dist/assets/bpmn-font/css/bpmn.css'
import NavigatedViewer from 'bpmn-js/lib/NavigatedViewer'
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { tr } from '@/shared/layout/language'
import { openWorkflowManual } from '../utils/manual'
import { getWorkflowDefinitions } from '../services/workflowDefinitionService'
import { duration, monitorApi, type MonitorInstance, type MonitorRow, type MonitorStep, type MonitorTask, type TimeReport } from '../services/monitorService'

const route = useRoute()
const router = useRouter()
const tab = ref<'requests' | 'times'>(route.query.tab === 'times' ? 'times' : 'requests')
const status = ref('running')
const workflowId = ref<number | null>(null)
const search = ref('')
const rows = ref<MonitorRow[]>([])
const view = ref<MonitorInstance | null>(null)
const times = ref<TimeReport | null>(null)
const defs = ref<{ id: number; name: string }[]>([])
const loading = ref(false)
const error = ref('')
const diagram = ref<HTMLElement | null>(null)
const viewCard = ref<HTMLElement | null>(null)
const today = new Date()
const iso = (d: Date) => d.toISOString().slice(0, 10)
const to = ref(iso(today))
const from = ref(iso(new Date(today.getTime() - 90 * 86400000)))
let viewer: InstanceType<typeof NavigatedViewer> | null = null

// the parts of the bpmn-js viewer this page uses
type DiagramElement = { id: string; type: string; businessObject?: { sourceRef?: { id: string }; targetRef?: { id: string } } }
type Canvas = { zoom: (to: string, center: string) => void; addMarker: (id: string, cls: string) => void }
type Registry = { get: (id: string) => DiagramElement | undefined; filter: (fn: (e: DiagramElement) => boolean) => DiagramElement[] }
type Overlays = { add: (id: string, o: { position: { bottom: number; left: number }; html: HTMLElement }) => void }

const overdueCount = computed(() => rows.value.filter((r) => r.overdue).length)
const humanSteps = computed(() => (view.value?.steps ?? []).filter((s) => s.tasks.length > 0))

function fail(e: unknown) { error.value = e instanceof Error ? e.message : String(e) }
function when(v: string | null) {
  if (!v) return '-'
  return new Date(v).toLocaleString('en-GB', { day: '2-digit', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit' })
}
const isOpen = (s: string) => ['OPEN', 'CLAIMED', 'IN_PROGRESS'].includes(s)
function taskText(t: MonitorTask) { return t.status === 'COMPLETED' ? (t.outcome || tr('done')) : t.status === 'CANCELLED' ? tr('not needed') : tr('waiting') }
function lateStep(s: MonitorStep) { return s.tasks.some((t) => t.dueAt && new Date(s.completedAt ?? Date.now()) > new Date(t.dueAt)) }
function statusText(r: MonitorRow) { return r.overdue ? tr('Overdue') : ['COMPLETED', 'REJECTED', 'CANCELLED'].includes(r.status) ? tr('Finished') : tr('Running') }
function chip(r: MonitorRow) { return r.overdue ? 'fx-chip--bad' : ['COMPLETED', 'REJECTED', 'CANCELLED'].includes(r.status) ? 'fx-chip--ok' : 'fx-chip--brand' }
const eventNames: Record<string, string> = {
  INSTANCE_STARTED: 'Request started', INSTANCE_COMPLETED: 'Request finished', STEP_ENTERED: 'Arrived at step', TASK_CREATED: 'Task sent',
  TASK_CLAIMED: 'Task claimed', TASK_STARTED: 'Work started', TASK_COMPLETED: 'Task done', TASK_DELEGATED: 'Out of office: delegated',
  TASK_TAKEN_OVER: 'Taken over', TASK_REMINDER: 'Reminder', TASK_OVERDUE: 'Overdue', TASK_ESCALATED: 'Escalated', PATH_CHOSEN: 'Rule chose the path',
  PARALLEL_STARTED: 'Parallel approval sent', PARALLEL_WAITING: 'Parallel approval: waiting', PARALLEL_DECIDED: 'Parallel approval decided',
}
function eventText(t: string) { return tr(eventNames[t] ?? t.replace(/_/g, ' ').toLowerCase()) }
function evKind(t: string) { return /OVERDUE|ESCALATED/.test(t) ? 'bad' : /COMPLETED|DECIDED/.test(t) ? 'ok' : /PATH|PARALLEL/.test(t) ? 'rule' : 'plain' }
const maxHours = computed(() => Math.max(1, ...(times.value?.steps.map((s) => s.averageHours) ?? []), ...(times.value?.people.map((p) => p.averageHours) ?? [])))
function barWidth(h: number) { return `${Math.max(2, Math.round((100 * h) / maxHours.value))}%` }

async function loadList() {
  loading.value = true
  try { rows.value = await monitorApi.list(status.value, workflowId.value, search.value.trim()) } catch (e) { fail(e) } finally { loading.value = false }
}
let timesLoaded = false
async function loadTimes(force = false) {
  if (timesLoaded && !force) return
  loading.value = true
  try { times.value = await monitorApi.times(from.value, to.value, workflowId.value); timesLoaded = true } catch (e) { fail(e) } finally { loading.value = false }
}

async function open(id: number) {
  try {
    view.value = await monitorApi.instance(id)
    void router.replace({ query: { ...route.query, id: String(id) } })
    await nextTick()
    await draw()
    viewCard.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
  } catch (e) { fail(e) }
}

/** The request on its diagram: finished steps teal, the current one orange, a rejected one red, the arrows it took highlighted. */
async function draw() {
  if (!view.value || !diagram.value) return
  viewer?.destroy()
  viewer = new NavigatedViewer({ container: diagram.value })
  await viewer.importXML(view.value.bpmnXml)
  const canvas = viewer.get('canvas') as Canvas
  const registry = viewer.get('elementRegistry') as Registry
  const overlays = viewer.get('overlays') as Overlays
  canvas.zoom('fit-viewport', 'auto')
  const steps = view.value.steps
  const byElement = new Map<string, MonitorStep[]>()
  for (const s of steps) byElement.set(s.elementId, [...(byElement.get(s.elementId) ?? []), s])
  for (const [id, visits] of byElement) {
    if (!registry.get(id)) continue
    const last = visits[visits.length - 1]!
    const cls = !last.completedAt && last.status !== 'COMPLETED' ? 'pv-cur' : last.outcome === 'REJECT' ? 'pv-rej' : 'pv-done'
    canvas.addMarker(id, cls)
    const human = visits.some((v) => v.tasks.length)
    if (human) {
      const hours = visits.reduce((a, v) => a + v.hours, 0)
      const badge = document.createElement('div')
      badge.className = 'pv-badge ' + cls
      badge.textContent = duration(hours) + (visits.length > 1 ? ` · ${visits.length}×` : '')
      overlays.add(id, { position: { bottom: 12, left: 0 }, html: badge })
    }
  }
  // the arrows taken: from each element to the next one the request entered
  for (let i = 1; i < steps.length; i++) {
    const a = steps[i - 1]!.elementId, b = steps[i]!.elementId
    const flow = registry.filter((e) => e.type === 'bpmn:SequenceFlow' && e.businessObject?.sourceRef?.id === a && e.businessObject?.targetRef?.id === b)[0]
    if (flow) canvas.addMarker(flow.id, 'pv-flow')
  }
  // steps never reached are dimmed
  registry.filter((e) => /Task|Gateway|Event/.test(e.type) && !byElement.has(e.id) && e.type !== 'label').forEach((e) => canvas.addMarker(e.id, 'pv-todo'))
}

onMounted(async () => {
  try {
    defs.value = (await getWorkflowDefinitions('')).map((d) => ({ id: d.dgrmId, name: d.dgrmName }))
  } catch { defs.value = [] }
  await loadList()
  if (tab.value === 'times') await loadTimes()
  const id = Number(route.query.id)
  if (id) await open(id)
})
onBeforeUnmount(() => viewer?.destroy())
</script>

<style scoped>
.pm-tabs { display: flex; gap: 6px; margin-bottom: 14px; }
.pm-tabs button { display: inline-flex; align-items: center; gap: 8px; height: 38px; padding: 0 16px; border: 1px solid var(--fin-line, #dfe8ea); border-radius: 10px; background: #fff; color: #3f5b6b; font: inherit; font-weight: 700; cursor: pointer; }
.pm-tabs button[aria-selected=true] { background: #00636D; border-color: #00636D; color: #fff; }
.pm-filters { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; margin-bottom: 14px; }
.pm-filters .fx-select { height: 36px; min-width: 160px; }
.pm-count { margin-left: auto; color: #5f7787; font-size: 12.5px; }
.pm-date { display: inline-flex; align-items: center; gap: 6px; font-size: 12.5px; font-weight: 700; color: #3f5b6b; }
.pm-date .fx-search { flex: 0 0 auto; min-width: 0; width: 150px; }
.pm-row { cursor: pointer; }
.pm-row.on td { background: #eef6f7 !important; }
.pm-late { color: #b42318; font-weight: 700; }
.pm-empty { text-align: center; color: #5f7787; padding: 22px !important; }
.pm-view { margin-top: 16px; }
.pm-view-head { display: flex; align-items: flex-start; justify-content: space-between; gap: 12px; }
.pm-view-head h2 { margin: 0; display: flex; align-items: center; gap: 10px; font-size: 20px; }
.pm-view-head p { margin: 4px 0 0; color: #5f7787; font-size: 13px; }
.pm-diagram { height: 380px; margin-top: 14px; border: 1px solid #dfe8ea; border-radius: 12px; background: #fbfdfd radial-gradient(#dfe8ea 1px, transparent 1px) 0 0 / 18px 18px; overflow: hidden; }
.pm-legend { display: flex; flex-wrap: wrap; gap: 14px; margin: 10px 2px 4px; font-size: 12px; color: #3f5b6b; }
.lg::before { content: ''; display: inline-block; width: 12px; height: 12px; border-radius: 3px; margin-right: 6px; vertical-align: -1px; border: 2px solid; }
.lg-done::before { background: #e3f1f2; border-color: #00636D; }
.lg-cur::before { background: #fdeee2; border-color: #E87722; }
.lg-rej::before { background: #fdecec; border-color: #b42318; }
.lg-todo::before { background: #fff; border-color: #c9d3d8; }
.lg-flow::before { height: 0; border-width: 2px 0 0; width: 18px; border-color: #00636D; border-radius: 0; vertical-align: 3px; }
.pm-split { display: grid; grid-template-columns: minmax(0, 1.6fr) minmax(0, 1fr); gap: 22px; margin-top: 10px; }
.pm-split h3, .fx-card h3 { margin: 18px 0 8px; font-size: 14px; color: #12324a; }
.pm-cur td { background: #fff8f2; }
.pm-hist { list-style: none; margin: 0; padding: 0 0 0 14px; border-left: 2px solid #dfe8ea; max-height: 520px; overflow: auto; }
.pm-hist li { position: relative; display: grid; gap: 1px; padding: 0 0 12px 12px; font-size: 12.5px; }
.pm-hist li::before { content: ''; position: absolute; left: -21px; top: 4px; width: 10px; height: 10px; border-radius: 50%; background: #fff; border: 2px solid #9fb3bd; }
.pm-hist li.ev-ok::before { border-color: #00636D; background: #e3f1f2; }
.pm-hist li.ev-bad::before { border-color: #b42318; background: #fdecec; }
.pm-hist li.ev-rule::before { border-color: #E87722; background: #fdeee2; }
.pm-hist time { color: #8aa0ad; font-size: 11.5px; }
.pm-hist b { color: #12324a; }
.pm-hist span { color: #5f7787; }
.pm-data { display: grid; grid-template-columns: max-content 1fr; gap: 6px 14px; margin: 0; font-size: 13px; }
.pm-data dt { color: #5f7787; }
.pm-data dd { margin: 0; font-weight: 600; color: #12324a; }
.pm-barcol { min-width: 200px; }
.pm-bar { display: inline-block; width: calc(100% - 70px); height: 8px; border-radius: 99px; background: #eef3f4; vertical-align: middle; overflow: hidden; }
.pm-bar i { display: block; height: 100%; border-radius: 99px; background: linear-gradient(90deg, #00636D, #2a8c95); }
.pm-barv { display: inline-block; width: 64px; text-align: right; font-variant-numeric: tabular-nums; font-size: 12.5px; }
:deep(.pv-done:not(.djs-connection) .djs-visual > :first-child) { fill: #e3f1f2 !important; stroke: #00636D !important; stroke-width: 2.5px !important; }
:deep(.pv-cur:not(.djs-connection) .djs-visual > :first-child) { fill: #fdeee2 !important; stroke: #E87722 !important; stroke-width: 3px !important; }
:deep(.pv-rej:not(.djs-connection) .djs-visual > :first-child) { fill: #fdecec !important; stroke: #b42318 !important; stroke-width: 2.5px !important; }
:deep(.pv-todo .djs-visual) { opacity: .45; }
:deep(.pv-flow .djs-visual > path) { stroke: #00636D !important; stroke-width: 3px !important; }
:deep(.pv-badge) { white-space: nowrap; padding: 1px 8px; border-radius: 999px; font: 700 11px/1.5 Inter, Arial, sans-serif; background: #00636D; color: #fff; transform: translateY(100%); }
:deep(.pv-badge.pv-cur) { background: #E87722; }
:deep(.pv-badge.pv-rej) { background: #b42318; }
@media (max-width: 1000px) { .pm-split { grid-template-columns: minmax(0, 1fr); } }
</style>
