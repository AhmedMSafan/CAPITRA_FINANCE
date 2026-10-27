<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter, type RouteLocationRaw } from 'vue-router'
import { getUser } from '@/shared/auth/session'
import { formatDate, formatDateTime, todayInCairo } from '@/shared/format/datetime'
import { workflowService } from '@/modules/wfe/services/workflowService'
import type { WorkflowInboxTask } from '@/modules/wfe/models/workflow'

type BusinessModule = {
  code: string
  title: string
  description: string
  route: string
  icon: string
  tone: string
  subtitle: string
  // Inbox MODULE_CODE values that belong to this module.
  inboxCodes: string[]
}

type Shortcut = { title: string; hint: string; icon: string; to: RouteLocationRaw; tone: string }

const router = useRouter()
const user = getUser()
const displayName = user?.fullName?.trim() || user?.userName?.trim() || 'CAPITRA User'
const firstName = displayName.split(/\s+/)[0] ?? 'User'

const longDate = new Intl.DateTimeFormat('en-GB', {
  timeZone: 'Africa/Cairo', weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
}).format(new Date())
const shortcuts = computed(() => [...createShortcuts, ...workShortcuts])

// The APEX applications being rebuilt.
const modules: BusinessModule[] = [
  {
    code: 'MORTGAGE',
    title: 'Portfolio Acquisition',
    description: 'Data load, AML, inquiry, legal, valuation, coverage, board review and disbursement.',
    route: '/mortgage',
    icon: 'pi pi-home',
    tone: 'c2',
    subtitle: 'Mortgage',
    inboxCodes: ['MORTGAGE', 'MTG']
  },
  {
    code: 'MORTGAGE FINANCE',
    title: 'Mortgage Finance',
    description: 'Fund file intake, credit, subsidy, offer, contract, servicing and collections.',
    route: '/mortgage-finance',
    icon: 'pi pi-building',
    tone: 'c3',
    subtitle: 'Mortgage Finance',
    inboxCodes: ['MORTGAGE_FINANCE', 'MFF']
  },
  {
    code: 'ACCOUNTING',
    title: 'Accounting',
    description: 'Chart of accounts, periods, posting rules and general ledger transactions.',
    route: '/gl',
    icon: 'pi pi-book',
    tone: 'c1',
    subtitle: 'General Ledger',
    inboxCodes: ['ACCOUNTING', 'GL']
  },
  {
    code: 'CHEQUES',
    title: 'Cheques Wallet',
    description: 'Cheque batches, cheque movements and collection status.',
    route: '/cheque-wallet',
    icon: 'pi pi-wallet',
    tone: 'c7',
    subtitle: 'Cheques',
    inboxCodes: ['CHEQUES', 'CHQ']
  }
]

const createShortcuts: Shortcut[] = [
  { title: 'Portfolio cases', hint: 'Open the mortgage portfolio cases', icon: 'pi pi-folder-open', to: { name: 'mtg-201' }, tone: 'c2' },
  { title: 'Mortgage Finance', hint: 'Fund intake and applications', icon: 'pi pi-file-plus', to: '/mortgage-finance', tone: 'c3' },
  { title: 'Journal entry', hint: 'Enter a manual GL journal', icon: 'pi pi-pencil', to: '/gl/journal-entry', tone: 'c1' },
  { title: 'Cheque batch upload', hint: 'Add a new cheque batch', icon: 'pi pi-upload', to: { name: 'chq-120' }, tone: 'c7' }
]

const workShortcuts: Shortcut[] = [
  { title: 'My inbox', hint: 'All tasks assigned to you', icon: 'pi pi-inbox', to: '/workflow/inbox', tone: 'c5' },
  { title: 'Workflow engine', hint: 'Processes, steps and roles', icon: 'pi pi-sitemap', to: '/workflowengine', tone: 'c6' },
  { title: 'Financial statements', hint: 'Trial balance and reports', icon: 'pi pi-chart-bar', to: '/gl/financial-statements', tone: 'c8' },
  { title: 'Administration', hint: 'Users, roles and setup', icon: 'pi pi-cog', to: '/admin', tone: 'c0' }
]

const tasks = ref<WorkflowInboxTask[]>([])
const loading = ref(true)
const loadError = ref('')
const filter = ref<'all' | 'overdue' | 'today' | 'new'>('all')
const today = todayInCairo()

const dueDay = (t: WorkflowInboxTask) => (t.dueDate ? t.dueDate.slice(0, 10) : '')
const isOverdue = (t: WorkflowInboxTask) => !!dueDay(t) && dueDay(t) < today
const isDueToday = (t: WorkflowInboxTask) => dueDay(t) === today
const isNew = (t: WorkflowInboxTask) => (t.status || '').toUpperCase() === 'OPEN'

const counts = computed(() => ({
  all: tasks.value.length,
  overdue: tasks.value.filter(isOverdue).length,
  today: tasks.value.filter(isDueToday).length,
  new: tasks.value.filter(isNew).length
}))

const shown = computed(() => {
  const list = tasks.value.filter((t) =>
    filter.value === 'overdue' ? isOverdue(t) :
    filter.value === 'today' ? isDueToday(t) :
    filter.value === 'new' ? isNew(t) : true)
  return list.slice(0, 6)
})

function moduleOf(t: WorkflowInboxTask) {
  const code = (t.moduleCode || '').toUpperCase()
  return modules.find((m) => m.inboxCodes.includes(code))
}

function moduleStats(m: BusinessModule) {
  const mine = tasks.value.filter((t) => moduleOf(t) === m)
  return { tasks: mine.length, overdue: mine.filter(isOverdue).length }
}

function statusOf(t: WorkflowInboxTask): { label: string; cls: string } {
  if (isOverdue(t)) return { label: 'Overdue', cls: 'late' }
  if (isDueToday(t)) return { label: 'Due today', cls: 'soon' }
  const s = (t.status || '').toUpperCase()
  if (s === 'OPEN') return { label: 'New', cls: 'new' }
  if (s === 'CLAIMED' || s === 'IN_PROGRESS') return { label: 'In progress', cls: 'prog' }
  return { label: s.replace(/_/g, ' ').toLowerCase() || '—', cls: 'prog' }
}

function openTask(t: WorkflowInboxTask) {
  router.push({ name: 'workflow-task', params: { inboxId: t.inboxId } })
}

onMounted(async () => {
  try {
    tasks.value = await workflowService.getMyInbox()
  } catch {
    loadError.value = 'Your inbox could not be loaded.'
  } finally {
    loading.value = false
  }
})
</script>

<template>
  <div class="home">
    <section class="card hello">
      <span class="ic c1 ic--lg"><i class="pi pi-home"></i></span>
      <div>
        <div class="muted">{{ longDate }}</div>
        <h1>Welcome back, {{ firstName }}</h1>
      </div>
      <div class="kpis">
        <RouterLink to="/workflow/inbox"><small>My open tasks</small><b>{{ counts.all }}</b></RouterLink>
        <button type="button" @click="filter = 'overdue'"><small>Overdue</small><b :class="{ red: counts.overdue }">{{ counts.overdue }}</b></button>
        <button type="button" @click="filter = 'today'"><small>Due today</small><b>{{ counts.today }}</b></button>
      </div>
    </section>

    <section class="mods">
      <article v-for="m in modules" :key="m.code" class="card mod">
        <div class="mod__top">
          <span class="ic" :class="m.tone"><i :class="m.icon"></i></span>
          <div><h3>{{ m.title }}</h3><small>{{ m.subtitle }}</small></div>
        </div>
        <p>{{ m.description }}</p>
        <div class="mod__stats">
          <div><b>{{ moduleStats(m).tasks }}</b><span>My tasks</span></div>
          <div><b :class="{ red: moduleStats(m).overdue }">{{ moduleStats(m).overdue }}</b><span>Overdue</span></div>
          <RouterLink :to="m.route" class="btn">Open</RouterLink>
        </div>
      </article>
    </section>

    <section class="row">
      <div class="card">
        <header class="card__h">
          <span class="ic c5"><i class="pi pi-inbox"></i></span>
          <h2>My Inbox ({{ counts.all }})</h2>
          <div class="seg">
            <button type="button" :class="{ on: filter === 'all' }" @click="filter = 'all'">All {{ counts.all }}</button>
            <button type="button" :class="{ on: filter === 'overdue' }" @click="filter = 'overdue'">Overdue {{ counts.overdue }}</button>
            <button type="button" :class="{ on: filter === 'today' }" @click="filter = 'today'">Due today {{ counts.today }}</button>
            <button type="button" :class="{ on: filter === 'new' }" @click="filter = 'new'">New {{ counts.new }}</button>
          </div>
        </header>
        <table>
          <thead><tr><th>Task</th><th>Module</th><th>Received</th><th>Due</th><th>Status</th><th></th></tr></thead>
          <tbody>
            <tr v-if="loading"><td colspan="6" class="empty">Loading your tasks…</td></tr>
            <tr v-else-if="loadError"><td colspan="6" class="empty red">{{ loadError }}</td></tr>
            <tr v-else-if="!shown.length"><td colspan="6" class="empty">No tasks here. You are all caught up.</td></tr>
            <tr v-for="t in shown" v-else :key="t.inboxId">
              <td><a class="lnk" @click="openTask(t)">{{ t.stepName || t.subject || 'Task' }}</a><div class="muted">{{ t.entityRef || t.workflowName || '' }}</div></td>
              <td>{{ moduleOf(t)?.title ?? (t.moduleCode || '—') }}</td>
              <td>{{ formatDateTime(t.createdOn) }}</td>
              <td>{{ t.dueDate ? formatDate(t.dueDate) : '—' }}</td>
              <td><span :class="{ red: statusOf(t).cls === 'late' }">{{ statusOf(t).label }}</span></td>
              <td class="r"><button type="button" class="btn btn--sm" @click="openTask(t)">Open</button></td>
            </tr>
          </tbody>
        </table>
        <footer class="card__f"><RouterLink to="/workflow/inbox">View All</RouterLink></footer>
      </div>

      <div class="card">
        <header class="card__h"><span class="ic c3"><i class="pi pi-bolt"></i></span><h2>Shortcuts</h2></header>
        <div class="sc">
          <RouterLink v-for="s in shortcuts" :key="s.title" :to="s.to">
            <span class="ic ic--sm" :class="s.tone"><i :class="s.icon"></i></span>
            <span><b>{{ s.title }}</b><small>{{ s.hint }}</small></span>
          </RouterLink>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.home { max-width: 1500px; margin: 0 auto; padding: 12px; display: flex; flex-direction: column; gap: 12px; font-family: var(--ds-font); color: var(--ds-text); font-size: 13px; }
a { text-decoration: none; }
.card { background: #fff; border: 1px solid var(--ds-line); border-radius: 4px; box-shadow: var(--ds-card-shadow); }
.muted { color: var(--ds-muted); font-size: 12px; }
.red { color: var(--ds-danger) !important; }
.ic { width: 32px; height: 32px; border-radius: 4px; display: grid; place-items: center; color: #fff; font-size: 16px; flex: none; }
.ic--lg { width: 40px; height: 40px; font-size: 20px; }
.ic--sm { width: 28px; height: 28px; font-size: 13px; }
.c0 { background: #706e6b; } .c1 { background: #0b5e5a; } .c2 { background: #f2a33a; } .c3 { background: #5867e8; }
.c5 { background: #3ba755; } .c6 { background: #7f8de1; } .c7 { background: #06a59a; } .c8 { background: #9050e9; }
.hello { display: flex; align-items: center; gap: 14px; padding: 14px 16px; }
.hello h1 { margin: 2px 0 0; font-size: 18px; font-weight: 700; color: var(--ds-ink); }
.kpis { margin-inline-start: auto; display: grid; grid-template-columns: repeat(3, 180px); border: 1px solid var(--ds-line); border-radius: 4px; }
.kpis > * { padding: 9px 14px; border: 0; border-inline-end: 1px solid var(--ds-line); background: transparent; text-align: start; cursor: pointer; font-family: inherit; color: inherit; }
.kpis > *:last-child { border-inline-end: 0; }
.kpis > *:hover { background: var(--ds-soft); }
.kpis small { display: block; color: var(--ds-muted); font-size: 12px; }
.kpis b { display: block; font-size: 22px; font-weight: 300; color: var(--ds-ink); }
.mods { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; }
.mod { padding: 14px 16px; display: flex; flex-direction: column; }
.mod__top { display: flex; gap: 12px; align-items: center; }
.mod__top .ic { width: 36px; height: 36px; font-size: 18px; }
.mod h3 { margin: 0; font-size: 15px; font-weight: 700; color: var(--ds-ink); }
.mod small { color: var(--ds-muted); font-size: 12px; }
.mod p { color: var(--ds-muted); font-size: 12.5px; line-height: 1.5; margin: 10px 0 12px; flex: 1; }
.mod__stats { display: flex; gap: 24px; align-items: flex-end; padding-top: 10px; border-top: 1px solid var(--ds-line-soft); }
.mod__stats b { display: block; font-size: 20px; font-weight: 300; color: var(--ds-ink); }
.mod__stats span { font-size: 12px; color: var(--ds-muted); }
.mod__stats .btn { margin-inline-start: auto; }
.btn { height: 32px; padding: 0 16px; border: 1px solid #c9c9c9; border-radius: 4px; background: #fff; color: var(--ds-link); display: inline-flex; align-items: center; font: 13px var(--ds-font); cursor: pointer; }
.btn:hover { background: var(--ds-soft); }
.btn--sm { height: 28px; }
.row { display: grid; grid-template-columns: 1fr 420px; gap: 12px; align-items: start; }
.card__h { display: flex; align-items: center; gap: 10px; padding: 12px 16px; }
.card__h h2 { margin: 0; font-size: 14px; font-weight: 700; color: var(--ds-ink); }
.card__f { border-top: 1px solid var(--ds-line); text-align: center; padding: 10px; }
.card__f a { color: var(--ds-link); }
.seg { margin-inline-start: auto; display: flex; }
.seg button { padding: 5px 12px; border: 1px solid #c9c9c9; margin-inline-start: -1px; background: #fff; color: var(--ds-link); font: 12.5px var(--ds-font); cursor: pointer; }
.seg button:first-child { border-radius: 4px 0 0 4px; }
.seg button:last-child { border-radius: 0 4px 4px 0; }
.seg button.on { background: var(--ds-brand); border-color: var(--ds-brand); color: #fff; }
table { width: 100%; border-collapse: collapse; }
th { text-align: start; font-size: 12px; font-weight: 600; color: var(--ds-muted); background: #fafaf9; padding: 8px 12px; border-block: 1px solid var(--ds-line); }
td { padding: 9px 12px; border-bottom: 1px solid var(--ds-line-soft); font-size: 13px; }
td.r { text-align: end; }
td.empty { text-align: center; color: var(--ds-muted); padding: 28px; }
.lnk { color: var(--ds-link); cursor: pointer; }
.lnk:hover { text-decoration: underline; }
.sc { display: grid; grid-template-columns: 1fr 1fr; padding: 0 16px 12px; column-gap: 12px; }
.sc a { display: flex; gap: 10px; align-items: center; padding: 10px 4px; border-bottom: 1px solid var(--ds-line-soft); }
.sc b { display: block; font-weight: 400; color: var(--ds-link); font-size: 13px; }
.sc small { display: block; color: var(--ds-muted); font-size: 12px; }
.sc a:hover b { text-decoration: underline; }
@media (max-width: 1200px) { .mods { grid-template-columns: repeat(2, 1fr); } .row { grid-template-columns: 1fr; } .kpis { grid-template-columns: repeat(3, 1fr); } }
</style>
