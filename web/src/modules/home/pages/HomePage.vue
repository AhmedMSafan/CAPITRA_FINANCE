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
  tone: 'slate' | 'gold'
  // Inbox MODULE_CODE values that belong to this module.
  inboxCodes: string[]
}

type Shortcut = { title: string; hint: string; icon: string; to: RouteLocationRaw; tone?: 'gold' }

const router = useRouter()
const user = getUser()
const displayName = user?.fullName?.trim() || user?.userName?.trim() || 'CAPITRA User'
const firstName = displayName.split(/\s+/)[0] ?? 'User'

const longDate = new Intl.DateTimeFormat('en-GB', {
  timeZone: 'Africa/Cairo', weekday: 'long', day: 'numeric', month: 'long', year: 'numeric'
}).format(new Date()).replace(',', ' ·')

// The APEX applications being rebuilt.
const modules: BusinessModule[] = [
  {
    code: 'MORTGAGE',
    title: 'Portfolio Acquisition',
    description: 'Data load, AML, inquiry, legal, valuation, coverage, board review and disbursement.',
    route: '/mortgage',
    icon: 'pi pi-home',
    tone: 'slate',
    inboxCodes: ['MORTGAGE', 'MTG']
  },
  {
    code: 'MORTGAGE FINANCE',
    title: 'Mortgage Finance',
    description: 'Fund file intake, credit, subsidy, offer, contract, servicing and collections.',
    route: '/mortgage-finance',
    icon: 'pi pi-building',
    tone: 'slate',
    inboxCodes: ['MORTGAGE_FINANCE', 'MFF']
  },
  {
    code: 'ACCOUNTING',
    title: 'Accounting',
    description: 'Chart of accounts, periods, posting rules and general ledger transactions.',
    route: '/gl',
    icon: 'pi pi-book',
    tone: 'slate',
    inboxCodes: ['ACCOUNTING', 'GL']
  },
  {
    code: 'CHEQUES',
    title: 'Cheques Wallet',
    description: 'Cheque batches, cheque movements and collection status.',
    route: '/cheque-wallet',
    icon: 'pi pi-wallet',
    tone: 'gold',
    inboxCodes: ['CHEQUES', 'CHQ']
  }
]

const createShortcuts: Shortcut[] = [
  { title: 'Portfolio cases', hint: 'Open the mortgage portfolio cases', icon: 'pi pi-folder-open', to: { name: 'mtg-201' } },
  { title: 'Mortgage Finance', hint: 'Fund intake and applications', icon: 'pi pi-file-plus', to: '/mortgage-finance' },
  { title: 'Journal entry', hint: 'Enter a manual GL journal', icon: 'pi pi-pencil', to: '/gl/journal-entry' },
  { title: 'Cheque batch upload', hint: 'Add a new cheque batch', icon: 'pi pi-wallet', to: { name: 'chq-120' }, tone: 'gold' }
]

const workShortcuts: Shortcut[] = [
  { title: 'My inbox', hint: 'All tasks assigned to you', icon: 'pi pi-inbox', to: '/workflow/inbox' },
  { title: 'Workflow engine', hint: 'Processes, steps and roles', icon: 'pi pi-sitemap', to: '/workflowengine', tone: 'gold' },
  { title: 'Financial statements', hint: 'Trial balance and reports', icon: 'pi pi-chart-bar', to: '/gl/financial-statements' },
  { title: 'Administration', hint: 'Users, roles and setup', icon: 'pi pi-cog', to: '/admin' }
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
  return list.slice(0, 8)
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
    <section class="welcome">
      <div>
        <small>{{ longDate }}</small>
        <h1>Welcome back, {{ firstName }}</h1>
        <p>Integrated Financing Solutions for a Smarter Tomorrow</p>
      </div>
      <div class="kpis">
        <RouterLink to="/workflow/inbox" class="kpi"><strong>{{ counts.all }}</strong><span>My open tasks</span></RouterLink>
        <button type="button" class="kpi" @click="filter = 'overdue'"><strong class="warn">{{ counts.overdue }}</strong><span>Overdue</span></button>
        <button type="button" class="kpi" @click="filter = 'today'"><strong>{{ counts.today }}</strong><span>Due today</span></button>
      </div>
    </section>

    <section class="mods">
      <RouterLink v-for="m in modules" :key="m.code" :to="m.route" class="mod" :class="`mod--${m.tone}`">
        <div class="mod__top">
          <span class="mod__ic"><i :class="m.icon"></i></span>
          <span class="mod__code">{{ m.code }}</span>
        </div>
        <h3>{{ m.title }}</h3>
        <p>{{ m.description }}</p>
        <div class="mod__meta">
          <div><strong>{{ moduleStats(m).tasks }}</strong><span>My tasks</span></div>
          <div><strong :class="{ warn: moduleStats(m).overdue }">{{ moduleStats(m).overdue }}</strong><span>Overdue</span></div>
        </div>
        <div class="mod__go">Open module <i class="pi pi-arrow-right"></i></div>
      </RouterLink>
    </section>

    <section class="sec">
      <div class="sec__head">
        <div><h2>My inbox</h2><p>Tasks assigned to you or your roles</p></div>
        <div class="tabs">
          <button type="button" :class="{ on: filter === 'all' }" @click="filter = 'all'">All {{ counts.all }}</button>
          <button type="button" :class="{ on: filter === 'overdue' }" @click="filter = 'overdue'">Overdue {{ counts.overdue }}</button>
          <button type="button" :class="{ on: filter === 'today' }" @click="filter = 'today'">Due today {{ counts.today }}</button>
          <button type="button" :class="{ on: filter === 'new' }" @click="filter = 'new'">New {{ counts.new }}</button>
        </div>
      </div>

      <div class="panel">
        <table>
          <thead>
            <tr><th>Task</th><th>Module</th><th>Received</th><th>Due</th><th>Status</th><th></th></tr>
          </thead>
          <tbody>
            <tr v-if="loading"><td colspan="6" class="empty">Loading your tasks…</td></tr>
            <tr v-else-if="loadError"><td colspan="6" class="empty">{{ loadError }}</td></tr>
            <tr v-else-if="!shown.length"><td colspan="6" class="empty">No tasks here. You are all caught up.</td></tr>
            <tr v-for="t in shown" v-else :key="t.inboxId">
              <td>
                <div class="tk">
                  <span class="tk__ic"><i :class="moduleOf(t)?.icon ?? 'pi pi-check-square'"></i></span>
                  <div><b>{{ t.stepName || t.subject || 'Task' }}</b><small>{{ t.entityRef || t.workflowName || '' }}</small></div>
                </div>
              </td>
              <td><span class="mtag">{{ moduleOf(t)?.title ?? (t.moduleCode || '—') }}</span></td>
              <td>{{ formatDateTime(t.createdOn) }}</td>
              <td>{{ t.dueDate ? formatDate(t.dueDate) : '—' }}</td>
              <td><span class="st" :class="statusOf(t).cls">{{ statusOf(t).label }}</span></td>
              <td class="r"><button type="button" class="open" @click="openTask(t)">Open <i class="pi pi-arrow-right"></i></button></td>
            </tr>
          </tbody>
        </table>
        <div class="panel__foot">
          <span>Showing {{ shown.length }} of {{ counts.all }} tasks</span>
          <RouterLink to="/workflow/inbox">Open full inbox <i class="pi pi-arrow-right"></i></RouterLink>
        </div>
      </div>
    </section>

    <section class="sec">
      <div class="sec__head"><div><h2>Shortcuts</h2><p>Start the most common work in one click</p></div></div>
      <div class="grp">Create and process</div>
      <div class="qa">
        <RouterLink v-for="s in createShortcuts" :key="s.title" :to="s.to" class="q" :class="{ 'q--gold': s.tone === 'gold' }">
          <span class="q__ic"><i :class="s.icon"></i></span>
          <div><b>{{ s.title }}</b><small>{{ s.hint }}</small></div>
        </RouterLink>
      </div>
      <div class="grp">Work and review</div>
      <div class="qa">
        <RouterLink v-for="s in workShortcuts" :key="s.title" :to="s.to" class="q" :class="{ 'q--gold': s.tone === 'gold' }">
          <span class="q__ic"><i :class="s.icon"></i></span>
          <div><b>{{ s.title }}</b><small>{{ s.hint }}</small></div>
        </RouterLink>
      </div>
    </section>
  </div>
</template>

<style scoped>
.home {
  max-width: 1320px;
  margin: 0 auto;
  padding: 0 24px 48px;
  font-family: Inter, "Segoe UI", Arial, sans-serif;
  color: #14232b;
}

a { text-decoration: none; color: inherit; }

.welcome {
  position: relative;
  overflow: hidden;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 24px;
  padding: 28px 32px 32px;
  border-radius: 0 0 20px 20px;
  color: #fff;
  background:
    radial-gradient(600px 260px at 100% 0%, rgba(224, 176, 74, .22), transparent 60%),
    linear-gradient(120deg, #24394a 0%, #2c4557 55%, #355266 100%);
}

.welcome small { font-size: 12px; font-weight: 700; letter-spacing: .14em; text-transform: uppercase; color: #f3d58c; }
.welcome h1 { margin: 8px 0 6px; font-size: 30px; font-weight: 800; }
.welcome p { margin: 0; font-size: 14.5px; color: #c3ced6; }

.kpis { display: flex; gap: 12px; }

.kpi {
  min-width: 190px;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 9px 20px;
  border: 1px solid rgba(255, 255, 255, .16);
  border-radius: 14px;
  background: rgba(255, 255, 255, .10);
  color: #fff;
  font-family: inherit;
  text-align: left;
  cursor: pointer;
}

.kpi:hover { background: rgba(255, 255, 255, .16); }
.kpi strong { display: block; font-size: 24px; font-weight: 700; line-height: 1.1; }
.kpi strong.warn { color: #fcd34d; }
.kpi span { font-size: 13.5px; color: #dbe4ea; }

.mods { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 20px; margin-top: 24px; }

.mod {
  display: flex;
  flex-direction: column;
  padding: 22px;
  border-top: 3px solid #e0b04a;
  border-radius: 14px;
  background: #fff;
  box-shadow: 0 2px 4px rgba(11, 37, 48, .05), 0 18px 40px rgba(11, 37, 48, .08);
  transition: transform .16s ease, box-shadow .16s ease;
}

.mod:hover { transform: translateY(-3px); box-shadow: 0 4px 8px rgba(11, 37, 48, .06), 0 24px 48px rgba(11, 37, 48, .12); }

.mod__top { display: flex; justify-content: space-between; align-items: center; }

.mod__ic {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: grid;
  place-items: center;
  background: #f1f5f9;
  color: #334155;
  font-size: 20px;
}

.mod__ic i { font-size: 20px !important; }
.tk__ic i { font-size: 14px !important; }
.q__ic i { font-size: 18px !important; }
.mod--gold .mod__ic { background: #fbf4e3; color: #a87a14; }
.mod__code { font-size: 10.5px; font-weight: 700; letter-spacing: .12em; color: #94a3b8; }
.mod h3 { margin: 16px 0 0; font-size: 18px; font-weight: 700; color: #0b2530; }
.mod p { flex: 1; margin: 8px 0 16px; font-size: 13px; line-height: 1.55; color: #64748b; }

.mod__meta { display: grid; grid-template-columns: 1fr 1fr; padding: 14px 0; margin-bottom: 14px; border-top: 1px solid #e6ebef; }
.mod__meta div + div { padding-left: 16px; border-left: 1px solid #e6ebef; }
.mod__meta strong { display: block; font-size: 20px; font-weight: 700; color: #0b2530; }
.mod__meta strong.warn { color: #b42318; }
.mod__meta span { font-size: 11.5px; color: #64748b; }

.mod__go { display: flex; justify-content: space-between; align-items: center; font-size: 13.5px; font-weight: 700; color: #0b2530; }

.mod__go i {
  width: 30px;
  height: 30px;
  border-radius: 9px;
  display: grid;
  place-items: center;
  background: #f1f5f9;
  font-size: 12px;
}

.sec { margin-top: 38px; }
.sec__head { display: flex; justify-content: space-between; align-items: flex-end; margin-bottom: 14px; }
.sec__head h2 { margin: 0; font-size: 20px; font-weight: 800; }
.sec__head p { margin: 4px 0 0; font-size: 13px; color: #64748b; }

.tabs { display: flex; gap: 6px; }

.tabs button {
  padding: 7px 14px;
  border: 1px solid #e6ebef;
  border-radius: 9px;
  background: #fff;
  font-family: inherit;
  font-size: 13px;
  font-weight: 600;
  color: #64748b;
  cursor: pointer;
}

.tabs button.on { background: #0b2530; border-color: #0b2530; color: #fff; }

.panel { overflow: hidden; border-radius: 16px; background: #fff; box-shadow: 0 1px 3px rgba(11, 37, 48, .06), 0 8px 24px rgba(11, 37, 48, .05); }

table { width: 100%; border-collapse: collapse; }

th {
  padding: 14px 20px;
  border-bottom: 1px solid #e6ebef;
  background: #f8fafb;
  text-align: left;
  font-size: 11.5px;
  font-weight: 700;
  letter-spacing: .06em;
  text-transform: uppercase;
  color: #94a3b8;
}

td { padding: 14px 20px; border-bottom: 1px solid #eef2f4; font-size: 14px; vertical-align: middle; }
tbody tr:last-child td { border-bottom: 0; }
tbody tr:hover td { background: #fbfcfd; }
td.empty { padding: 28px 20px; text-align: center; color: #64748b; }
td.r { text-align: right; }

.tk { display: flex; align-items: center; gap: 12px; }

.tk__ic {
  width: 36px;
  height: 36px;
  flex: none;
  border-radius: 10px;
  display: grid;
  place-items: center;
  background: #f1f5f9;
  color: #475569;
  font-size: 14px;
}

.tk b { display: block; font-size: 14px; }
.tk small { font-size: 12px; color: #64748b; }

.mtag { padding: 4px 10px; border-radius: 20px; background: #f1f5f9; color: #334155; font-size: 12px; font-weight: 600; white-space: nowrap; }

.st { padding: 4px 10px; border-radius: 6px; font-size: 12px; font-weight: 700; white-space: nowrap; }
.st.new { background: #e0f2fe; color: #0369a1; }
.st.prog { background: #fbf4e3; color: #a87a14; }
.st.late { background: #ffe4e6; color: #be123c; }
.st.soon { background: #fef3c7; color: #b45309; }

.open {
  display: inline-flex;
  gap: 6px;
  align-items: center;
  padding: 7px 14px;
  border: 0;
  border-radius: 9px;
  background: #0b2530;
  color: #fff;
  font-family: inherit;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
}

.open i { font-size: 11px; }
.open:hover { background: #16323f; }

.panel__foot { display: flex; justify-content: space-between; padding: 14px 20px; border-top: 1px solid #e6ebef; background: #f8fafb; font-size: 13px; color: #64748b; }
.panel__foot a { font-weight: 700; color: #0b2530; }
.panel__foot a i { font-size: 11px; }

.grp { margin: 20px 0 10px; font-size: 11.5px; font-weight: 700; letter-spacing: .08em; text-transform: uppercase; color: #94a3b8; }

.qa { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 16px; }

.q {
  display: flex;
  gap: 14px;
  align-items: flex-start;
  padding: 20px;
  border: 1px solid transparent;
  border-radius: 16px;
  background: #fff;
  box-shadow: 0 1px 3px rgba(11, 37, 48, .06), 0 8px 24px rgba(11, 37, 48, .05);
}

.q:hover { border-color: #ecd9a8; }

.q__ic {
  width: 46px;
  height: 46px;
  flex: none;
  border-radius: 13px;
  display: grid;
  place-items: center;
  background: #f1f5f9;
  color: #334155;
  font-size: 18px;
}

.q--gold .q__ic { background: #fbf4e3; color: #a87a14; }
.q b { display: block; margin-top: 2px; font-size: 15px; }
.q small { display: block; margin-top: 4px; font-size: 12.5px; line-height: 1.45; color: #64748b; }

@media (max-width: 1100px) {
  .mods, .qa { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .welcome { flex-direction: column; align-items: flex-start; }
}

@media (max-width: 640px) {
  .mods, .qa { grid-template-columns: 1fr; }
  .sec__head { flex-direction: column; align-items: flex-start; gap: 10px; }
}
</style>
