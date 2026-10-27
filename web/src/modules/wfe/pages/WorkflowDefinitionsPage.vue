<template>
  <!-- App 118 page 904 "Workflow Definition Registry": the hero with its counts, a search bar and a gallery of workflow cards. -->
  <div class="wx wr wx-page">
    <section class="wx-hero">
      <div class="wr-hero">
        <div>
          <span class="wx-eyebrow"><i></i>Workflow registry</span>
          <h1>Workflow definitions</h1>
          <p>Every workflow the engine can run. Open one to change its steps and routing in the visual designer, or start a new one.</p>
        </div>
        <div class="wr-cta">
          <router-link class="wx-btn wx-btn--ghost" to="/workflow/inbox"><i class="pi pi-inbox"></i><span>My inbox</span></router-link>
          <button class="wx-btn wx-btn--hot" type="button" @click="createNew"><i class="pi pi-plus"></i><span>New workflow</span></button>
        </div>
      </div>
      <div class="wr-kpis">
        <div class="wr-kpi"><b>{{ rows.length }}</b><span>Workflow definitions</span></div>
        <div class="wr-kpi"><b>{{ activeCount }}</b><span>Active</span></div>
        <div class="wr-kpi"><b>{{ totals.steps }}</b><span>Designed steps</span></div>
        <div class="wr-kpi"><b>{{ totals.flows }}</b><span>Transitions</span></div>
      </div>
    </section>

    <div class="wr-bar" role="search">
      <label class="wr-search">
        <i class="pi pi-search"></i>
        <input v-model="search" type="search" placeholder="Search by name, code or description" aria-label="Search workflows" />
      </label>
      <div class="wr-chips" role="group" aria-label="Status">
        <button type="button" class="wx-chip" :aria-pressed="status === 'ALL'" @click="status = 'ALL'">All <em>{{ rows.length }}</em></button>
        <button type="button" class="wx-chip" :aria-pressed="status === 'Y'" @click="status = 'Y'">Active <em>{{ activeCount }}</em></button>
        <button type="button" class="wx-chip" :aria-pressed="status === 'N'" @click="status = 'N'">Inactive <em>{{ rows.length - activeCount }}</em></button>
      </div>
      <select v-model="sort" class="wr-sel" aria-label="Sort">
        <option value="recent">Recently changed</option>
        <option value="name">Name A–Z</option>
        <option value="steps">Most steps</option>
      </select>
    </div>

    <div v-if="errorMessage" class="wr-error">{{ errorMessage }}</div>

    <div v-if="loading" class="wr-empty on"><b>Loading workflows…</b></div>
    <div v-else class="wr-grid">
      <article v-for="w in shown" :key="w.dgrmId" class="wr-card" :class="{ 'is-off': w.isActive !== 'Y' }">
        <div class="wr-head">
          <div class="wr-r1">
            <span class="wr-code" :title="w.workflowCode">{{ w.workflowCode }}</span>
            <span v-if="w.isActive === 'Y'" class="wx-pill wx-pill--ok">Active</span>
            <span v-else class="wx-pill wx-pill--off">Inactive</span>
          </div>
          <h3><a href="#" @click.prevent="edit(w.dgrmId)">{{ w.dgrmName }}</a></h3>
          <p class="wr-desc">{{ w.description || 'No description.' }}</p>
          <div class="wr-tags">
            <span class="wx-tag">{{ formatStatus(w.dgrmStatus) }}</span>
          </div>
        </div>
        <div class="wr-flow" aria-label="Flow preview">
          <span v-if="!w.flow.steps.length" class="wr-noflow">No steps designed yet</span>
          <template v-else>
            <span class="wr-ev" title="Start"></span>
            <template v-for="(s, i) in w.flow.steps.slice(0, 3)" :key="i">
              <span class="wr-ln"></span>
              <span v-if="s.gateway" class="wr-st dec" :title="s.name"></span>
              <span v-else class="wr-st" :title="s.name"><i></i><span>{{ s.name }}</span></span>
            </template>
            <template v-if="w.flow.steps.length > 3">
              <span class="wr-ln"></span><span class="wr-more">+{{ w.flow.steps.length - 3 }}</span>
            </template>
            <span class="wr-ln"></span><span class="wr-ev end" title="End"></span>
          </template>
        </div>
        <div class="wr-meta">
          <div><b>v{{ w.dgrmVersion }}</b><span>Version</span></div>
          <div><b>{{ w.flow.steps.length }}</b><span>Steps</span></div>
          <div><b>{{ w.flow.flows }}</b><span>Transitions</span></div>
          <div><b>{{ w.flow.gateways }}</b><span>Decisions</span></div>
        </div>
        <div class="wr-foot">
          <small>Updated {{ formatDate(w.dgrmLastUpdateOn || w.dgrmCreatedOn) }} · {{ (w.dgrmLastUpdateBy || w.dgrmCreatedBy || '').toLowerCase() }}</small>
          <button class="wr-open" type="button" @click="edit(w.dgrmId)"><i class="pi pi-pencil"></i><span>Open designer</span></button>
        </div>
      </article>
    </div>
    <div v-if="!loading && !shown.length" class="wr-empty on">
      <b>{{ rows.length ? 'No workflows match' : 'No workflows yet' }}</b>
      {{ rows.length ? 'Try another search or clear the filters.' : 'Create your first one with New workflow.' }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { getWorkflowDefinitions, type WorkflowDefinition } from '../services/workflowDefinitionService'

type Flow = { steps: { name: string; gateway: boolean }[]; flows: number; gateways: number }

const router = useRouter()

const rows = ref<(WorkflowDefinition & { flow: Flow })[]>([])
const search = ref('')
const status = ref<'ALL' | 'Y' | 'N'>('ALL')
const sort = ref<'recent' | 'name' | 'steps'>('recent')
const loading = ref(false)
const errorMessage = ref('')

const TASKS = ['task', 'userTask', 'serviceTask', 'scriptTask', 'manualTask', 'businessRuleTask', 'sendTask', 'receiveTask', 'callActivity', 'subProcess']
const GATEWAYS = ['exclusiveGateway', 'parallelGateway', 'inclusiveGateway', 'eventBasedGateway', 'complexGateway']

/** The steps of a BPMN diagram in the order the flow reaches them from its start event. */
function readFlow(xml: string): Flow {
  const flow: Flow = { steps: [], flows: 0, gateways: 0 }
  if (!xml) return flow
  const doc = new DOMParser().parseFromString(xml, 'application/xml')
  const process = [...doc.getElementsByTagName('*')].find((e) => e.localName === 'process')
  if (!process) return flow
  const nodes = new Map<string, Element>()
  const next = new Map<string, string[]>()
  for (const e of process.children) {
    const id = e.getAttribute('id') || ''
    if (e.localName === 'sequenceFlow') {
      flow.flows++
      const src = e.getAttribute('sourceRef') || ''
      next.set(src, [...(next.get(src) ?? []), e.getAttribute('targetRef') || ''])
    } else nodes.set(id, e)
  }
  const seen = new Set<string>()
  const queue = [...nodes.values()].filter((e) => e.localName === 'startEvent').map((e) => e.getAttribute('id') || '')
  const ordered: Element[] = []
  while (queue.length) {
    const id = queue.shift()!
    if (seen.has(id)) continue
    seen.add(id)
    const e = nodes.get(id)
    if (e) ordered.push(e)
    queue.push(...(next.get(id) ?? []))
  }
  // steps the flow does not reach still count
  for (const e of nodes.values()) if (!seen.has(e.getAttribute('id') || '')) ordered.push(e)
  for (const e of ordered) {
    const gateway = GATEWAYS.includes(e.localName)
    if (!gateway && !TASKS.includes(e.localName)) continue
    if (gateway) flow.gateways++
    flow.steps.push({ name: e.getAttribute('name') || e.getAttribute('id') || '', gateway })
  }
  return flow
}

const activeCount = computed(() => rows.value.filter((x) => x.isActive === 'Y').length)

const totals = computed(() => ({
  steps: rows.value.reduce((n, x) => n + x.flow.steps.length, 0),
  flows: rows.value.reduce((n, x) => n + x.flow.flows, 0)
}))

const changed = (w: WorkflowDefinition) => w.dgrmLastUpdateOn || w.dgrmCreatedOn || ''

const shown = computed(() => {
  const q = search.value.trim().toLowerCase()
  const list = rows.value.filter(
    (w) =>
      (status.value === 'ALL' || w.isActive === status.value) &&
      (!q || [w.workflowCode, w.dgrmName, w.description].join(' ').toLowerCase().includes(q))
  )
  if (sort.value === 'name') return [...list].sort((a, b) => a.dgrmName.localeCompare(b.dgrmName))
  if (sort.value === 'steps') return [...list].sort((a, b) => b.flow.steps.length - a.flow.steps.length)
  return [...list].sort((a, b) => changed(b).localeCompare(changed(a)))
})

async function load() {
  loading.value = true
  errorMessage.value = ''
  try {
    rows.value = (await getWorkflowDefinitions('')).map((w) => ({ ...w, flow: readFlow(w.dgrmContent) }))
  } catch (e) {
    errorMessage.value = e instanceof Error ? e.message : 'Unable to load workflows.'
  } finally {
    loading.value = false
  }
}

function createNew() {
  router.push('/workflowengine/designer/new')
}

function edit(id: number) {
  router.push(`/workflowengine/designer/${id}`)
}

function formatDate(v?: string | null) {
  if (!v) return '—'
  const d = new Date(v)
  if (Number.isNaN(d.getTime())) return v
  return new Intl.DateTimeFormat('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }).format(d)
}

function formatStatus(value?: string | null) {
  if (!value) return '—'
  return value.charAt(0).toUpperCase() + value.slice(1).toLowerCase()
}

onMounted(load)
</script>

<style scoped>
.wr-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(440px, 1fr)); gap: 16px; }
.wr-card {
  position: relative; display: flex; flex-direction: column; border-radius: 18px; background: #fff; border: 1px solid var(--wx-line); overflow: hidden;
  box-shadow: 0 1px 2px rgba(11, 37, 51, .04); transition: transform .18s, box-shadow .18s, border-color .18s; animation: wrIn .4s both;
}
@keyframes wrIn { from { opacity: 0; transform: translateY(10px); } to { opacity: 1; transform: none; } }
.wr-card:hover { transform: translateY(-3px); border-color: #B9CBD1; box-shadow: 0 22px 40px -22px rgba(11, 37, 51, .35); }
.wr-head { padding: 18px 18px 0; display: flex; flex-direction: column; gap: 10px; }
.wr-r1 { display: flex; align-items: center; justify-content: space-between; gap: 10px; }
.wr-code {
  font: 600 12px/1.2 ui-monospace, SFMono-Regular, Menlo, Consolas, monospace; color: var(--wx-teal); background: var(--wx-teal-l); padding: 4px 8px;
  border-radius: 7px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; max-width: 65%;
}
.wr-card h3 { margin: 0; font-size: 17px; font-weight: 700; letter-spacing: -.01em; line-height: 1.3; color: var(--wx-ink); }
.wr-card h3 a { color: inherit; }
.wr-card h3 a::after { content: ""; position: absolute; inset: 0; z-index: 1; }
.wr-desc {
  margin: 0; font-size: 13px; line-height: 1.5; color: var(--wx-muted); display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical;
  overflow: hidden; min-height: 39px;
}
.wr-tags { display: flex; flex-wrap: wrap; gap: 6px; }
.wr-flow {
  margin: 14px 18px 0; padding: 14px 12px; border-radius: 12px; background: var(--wx-soft); border: 1px solid var(--wx-line);
  background-image: radial-gradient(#D6E3E7 1px, transparent 1.1px); background-size: 12px 12px; display: flex; align-items: center;
  overflow: hidden; min-height: 58px;
}
.wr-ev { flex: 0 0 auto; width: 14px; height: 14px; border-radius: 50%; border: 2.5px solid var(--wx-teal); background: #fff; }
.wr-ev.end { border-width: 4.5px; border-color: var(--wx-orange); }
.wr-ln { flex: 0 0 14px; min-width: 10px; height: 2px; background: #9DB9C0; position: relative; }
.wr-ln::after { content: ""; position: absolute; inset-inline-end: -1px; top: -3px; border: 4px solid transparent; border-inline-start: 5px solid #9DB9C0; border-inline-end: 0; }
.wr-st {
  flex: 1 1 0; min-width: 0; max-width: 150px; display: flex; align-items: center; gap: 6px; height: 28px; padding: 0 9px; border-radius: 8px;
  background: #fff; border: 1px solid #CFDDE1; font-size: 11.5px; font-weight: 600; color: var(--wx-ink2); white-space: nowrap;
  box-shadow: 0 1px 2px rgba(11, 37, 51, .05);
}
.wr-st i { flex: 0 0 auto; width: 7px; height: 7px; border-radius: 50%; background: var(--wx-teal); }
.wr-st span { overflow: hidden; text-overflow: ellipsis; }
.wr-st.dec { width: 22px; height: 22px; padding: 0; flex: 0 0 22px; transform: rotate(45deg); border-radius: 4px; border-color: #F3B47F; background: #FFF1E6; }
.wr-more { flex: 0 0 auto; font-size: 11.5px; font-weight: 700; color: var(--wx-muted); padding: 0 6px; }
.wr-noflow { font-size: 12.5px; color: var(--wx-muted); font-style: italic; padding: 0 4px; }
.wr-meta { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px; margin: 14px 18px 0; padding: 12px 0; border-top: 1px dashed var(--wx-line); }
.wr-meta div b { display: block; font-size: 16px; font-weight: 800; color: var(--wx-ink); line-height: 1.2; }
.wr-meta div span { font-size: 11.5px; color: var(--wx-muted); }
.wr-foot {
  display: flex; align-items: center; justify-content: space-between; gap: 10px; margin-top: auto; padding: 12px 18px; background: #FAFCFC;
  border-top: 1px solid var(--wx-line);
}
.wr-foot small { font-size: 12px; color: var(--wx-muted); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.wr-open {
  position: relative; z-index: 2; display: inline-flex; align-items: center; gap: 7px; height: 34px; padding: 0 13px; border: 0; border-radius: 10px;
  background: var(--wx-teal-d); color: #fff; font: 600 13px Inter, Arial, sans-serif; white-space: nowrap; cursor: pointer; transition: background .15s, transform .15s;
}
.wr-open:hover { background: var(--wx-teal); transform: translateX(2px); }
.wr-card.is-off { background: #FCFDFD; }
.wr-card.is-off .wr-flow { filter: grayscale(.7); opacity: .75; }
.wr-empty { display: none; padding: 40px 20px; text-align: center; border-radius: 16px; border: 1px dashed var(--wx-line); background: #fff; color: var(--wx-muted); }
.wr-empty.on { display: block; }
.wr-empty b { display: block; font-size: 16px; color: var(--wx-ink); margin-bottom: 4px; }
@media (max-width: 640px) { .wr-grid { grid-template-columns: 1fr; } .wr-meta { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
