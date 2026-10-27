<template>
  <!-- The main page in the app 118 home design: hero with the greeting and inbox counts, the modules as cards, the inbox and shortcuts. -->
  <div class="wx hm wx-page">
    <section class="wx-hero hm-hero">
      <div>
        <span class="wx-eyebrow"><i></i>Capitra Finance &middot; {{ longDate }}</span>
        <h1>{{ greeting }}, <em>{{ firstName }}</em></h1>
        <p>Acquire and finance mortgage portfolios, keep the ledger and follow every cheque, with every task that waits for you in one inbox.</p>
        <div class="hm-cta">
          <router-link class="wx-btn wx-btn--hot" to="/workflow/inbox"><i class="pi pi-inbox"></i><span>My inbox</span></router-link>
          <router-link class="wx-btn wx-btn--ghost" :to="{ name: 'mtg-201' }"><i class="pi pi-folder-open"></i><span>Portfolio cases</span></router-link>
          <router-link class="wx-btn wx-btn--ghost" to="/gl/journal-entry"><i class="pi pi-pencil"></i><span>Journal entry</span></router-link>
        </div>
      </div>
      <div class="hm-kpis" role="list">
        <div class="hm-kpi" role="listitem"><b>{{ loading ? '–' : counts.all }}</b><span>Tasks in my inbox &middot; <small>{{ counts.new }} new</small></span></div>
        <div class="hm-kpi" :class="{ 'is-hot': counts.overdue > 0 }" role="listitem"><b>{{ loading ? '–' : counts.overdue }}</b><span>Overdue</span></div>
        <div class="hm-kpi" :class="{ 'is-warm': counts.today > 0 }" role="listitem"><b>{{ loading ? '–' : counts.today }}</b><span>Due today</span></div>
        <div class="hm-kpi" role="listitem"><b>{{ modules.length }}</b><span>Business modules</span></div>
      </div>
    </section>

    <div class="wx-sec"><h2>Your modules</h2></div>
    <section class="hm-mods">
      <router-link v-for="m in modules" :key="m.code" :to="m.route" class="hm-card">
        <div class="hm-top">
          <span class="hm-ico"><i :class="m.icon"></i></span>
          <span class="hm-go"><i class="pi pi-arrow-right"></i></span>
        </div>
        <div>
          <h3>{{ m.title }}</h3>
          <p>{{ m.description }}</p>
        </div>
        <div class="hm-foot">
          <b>{{ moduleStats(m).tasks }}</b><span>{{ moduleStats(m).tasks === 1 ? 'task' : 'tasks' }} in your inbox</span>
          <span v-if="moduleStats(m).overdue" class="wo-pill wo-pill--bad">{{ moduleStats(m).overdue }} overdue</span>
        </div>
      </router-link>
    </section>

    <div class="wx-sec"><h2>Work</h2></div>
    <section class="hm-work">
      <article class="hm-panel">
        <div class="hm-ph">
          <span class="hm-ico"><i class="pi pi-inbox"></i></span>
          <div><h3>My inbox</h3><p>The tasks waiting for you or your roles, the oldest due first.</p></div>
          <router-link class="hm-more" to="/workflow/inbox">View all</router-link>
        </div>
        <div class="wr-chips" role="group" aria-label="Show">
          <button type="button" class="wx-chip" :aria-pressed="filter === 'all'" @click="filter = 'all'">All <em>{{ counts.all }}</em></button>
          <button type="button" class="wx-chip" :aria-pressed="filter === 'new'" @click="filter = 'new'">New <em>{{ counts.new }}</em></button>
          <button type="button" class="wx-chip" :aria-pressed="filter === 'today'" @click="filter = 'today'">Due today <em>{{ counts.today }}</em></button>
          <button type="button" class="wx-chip" :aria-pressed="filter === 'overdue'" @click="filter = 'overdue'">Overdue <em>{{ counts.overdue }}</em></button>
        </div>
        <ul class="hm-tl">
          <li v-if="loading" class="hm-empty">Loading your inbox…</li>
          <li v-else-if="loadError" class="hm-empty hm-err">{{ loadError }}</li>
          <li v-else-if="!shown.length" class="hm-empty">{{ counts.all ? 'No tasks match this filter.' : 'No tasks here. You are all caught up.' }}</li>
          <li v-for="t in shown" :key="t.inboxId">
            <a href="#" @click.prevent="openTask(t)">
              <span><b>{{ t.subject || t.stepName }}</b><small>{{ [t.entityRef, t.workflowName].filter(Boolean).join(' · ') }}</small></span>
              <span><b class="hm-sm">{{ moduleOf(t)?.title || t.moduleCode }}</b><small>{{ t.stepName }}</small></span>
              <span class="hm-r">
                <span class="wo-pill" :class="pillOf(t)">{{ statusOf(t).label }}</span>
                <small>{{ t.dueDate ? 'Due ' + formatDate(t.dueDate) : 'Received ' + formatDateTime(t.createdOn) }}</small>
              </span>
            </a>
          </li>
        </ul>
      </article>

      <article class="hm-panel">
        <div class="hm-ph">
          <span class="hm-ico"><i class="pi pi-bolt"></i></span>
          <div><h3>Shortcuts</h3><p>Start the work you do most.</p></div>
        </div>
        <ul class="hm-sc">
          <li v-for="s in shortcuts" :key="s.title">
            <router-link :to="s.to">
              <span class="hm-li"><i :class="s.icon"></i></span>
              <span><b>{{ s.title }}</b><small>{{ s.hint }}</small></span>
              <i class="pi pi-arrow-right hm-arr"></i>
            </router-link>
          </li>
        </ul>
      </article>
    </section>
  </div>
</template>

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
  subtitle: string
  // Inbox MODULE_CODE values that belong to this module.
  inboxCodes: string[]
}

type Shortcut = { title: string; hint: string; icon: string; to: RouteLocationRaw }

const router = useRouter()
const user = getUser()
const displayName = user?.fullName?.trim() || user?.userName?.trim() || 'CAPITRA User'
const firstName = displayName.split(/\s+/)[0] ?? 'User'

const cairoHour = Number(new Intl.DateTimeFormat('en-GB', { timeZone: 'Africa/Cairo', hour: 'numeric', hour12: false }).format(new Date()))
const greeting = cairoHour < 12 ? 'Good morning' : cairoHour < 17 ? 'Good afternoon' : 'Good evening'

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
    subtitle: 'Mortgage',
    inboxCodes: ['MORTGAGE', 'MTG']
  },
  {
    code: 'MORTGAGE FINANCE',
    title: 'Mortgage Finance',
    description: 'Fund file intake, credit, subsidy, offer, contract, servicing and collections.',
    route: '/mortgage-finance',
    icon: 'pi pi-building',
    subtitle: 'Mortgage Finance',
    inboxCodes: ['MORTGAGE_FINANCE', 'MFF']
  },
  {
    code: 'ACCOUNTING',
    title: 'Accounting',
    description: 'Chart of accounts, periods, posting rules and general ledger transactions.',
    route: '/gl',
    icon: 'pi pi-book',
    subtitle: 'General Ledger',
    inboxCodes: ['ACCOUNTING', 'GL']
  },
  {
    code: 'CHEQUES',
    title: 'Cheques Wallet',
    description: 'Cheque batches, cheque movements and collection status.',
    route: '/cheque-wallet',
    icon: 'pi pi-wallet',
    subtitle: 'Cheques',
    inboxCodes: ['CHEQUES', 'CHQ']
  }
]

const createShortcuts: Shortcut[] = [
  { title: 'Portfolio cases', hint: 'Open the mortgage portfolio cases', icon: 'pi pi-folder-open', to: { name: 'mtg-201' } },
  { title: 'Mortgage Finance', hint: 'Fund intake and applications', icon: 'pi pi-file-plus', to: '/mortgage-finance' },
  { title: 'Journal entry', hint: 'Enter a manual GL journal', icon: 'pi pi-pencil', to: '/gl/journal-entry' },
  { title: 'Cheque batch upload', hint: 'Add a new cheque batch', icon: 'pi pi-upload', to: { name: 'chq-120' } }
]

const workShortcuts: Shortcut[] = [
  { title: 'My inbox', hint: 'All tasks assigned to you', icon: 'pi pi-inbox', to: '/workflow/inbox' },
  { title: 'Workflow engine', hint: 'Processes, steps and roles', icon: 'pi pi-sitemap', to: '/workflowengine' },
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
  return list.slice(0, 7)
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

function pillOf(t: WorkflowInboxTask) {
  const c = statusOf(t).cls
  return c === 'late' ? 'wo-pill--bad' : c === 'soon' ? 'wo-pill--warn' : c === 'new' ? 'wo-pill--open' : 'wo-pill--prog'
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

<style scoped>
.hm-hero { display: grid; grid-template-columns: minmax(0, 1.25fr) minmax(0, 1fr); gap: 28px; align-items: center; }
.hm-cta { display: flex; flex-wrap: wrap; gap: 10px; margin-top: 20px; }
.hm-kpis { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 10px; }
.hm-kpi { padding: 14px 16px; border-radius: 14px; background: rgba(255, 255, 255, .07); border: 1px solid rgba(255, 255, 255, .12); backdrop-filter: blur(8px); }
.hm-kpi b { display: block; font-size: 26px; font-weight: 800; letter-spacing: -.02em; line-height: 1.1; color: #fff; }
.hm-kpi span { display: block; margin-top: 3px; font-size: 12.5px; color: rgba(255, 255, 255, .66); }
.hm-kpi small { color: #7FE0C9; font-weight: 600; font-size: 12.5px; }
.hm-kpi.is-hot b { color: #FFB4A8; }
.hm-kpi.is-warm b { color: #FFC98F; }
.hm-mods { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 16px; }
.hm-card {
  position: relative; display: flex; flex-direction: column; gap: 14px; padding: 20px; border-radius: 16px; background: #fff; border: 1px solid var(--wx-line);
  color: var(--wx-ink); box-shadow: 0 1px 2px rgba(11, 37, 51, .04); transition: transform .18s, box-shadow .18s, border-color .18s; overflow: hidden;
}
.hm-card::before { content: ""; position: absolute; inset: 0 0 auto 0; height: 3px; background: var(--wx-teal); opacity: 0; transition: opacity .18s; }
.hm-card:hover { transform: translateY(-3px); border-color: #A9CDD1; box-shadow: 0 18px 36px -18px rgba(11, 37, 51, .25); }
.hm-card:hover::before { opacity: 1; }
.hm-top { display: flex; align-items: center; justify-content: space-between; }
.hm-ico { flex: 0 0 44px; width: 44px; height: 44px; border-radius: 12px; display: grid; place-items: center; color: var(--wx-teal); background: var(--wx-teal-l); font-size: 18px; }
.hm-go {
  width: 32px; height: 32px; border-radius: 50%; display: grid; place-items: center; font-size: 12px; color: var(--wx-muted); background: var(--wx-soft);
  border: 1px solid var(--wx-line); transition: all .18s;
}
.hm-card:hover .hm-go { background: var(--wx-teal); border-color: var(--wx-teal); color: #fff; transform: translateX(2px); }
.hm-card h3, .hm-panel h3 { margin: 0; font-size: 16px; font-weight: 700; letter-spacing: -.01em; color: var(--wx-ink); }
.hm-card p, .hm-panel p { margin: 4px 0 0; font-size: 13px; line-height: 1.5; color: var(--wx-muted); }
.hm-foot { display: flex; align-items: baseline; gap: 6px; margin-top: auto; padding-top: 12px; border-top: 1px dashed var(--wx-line); }
.hm-foot b { font-size: 22px; font-weight: 800; letter-spacing: -.02em; }
.hm-foot > span { font-size: 12.5px; color: var(--wx-muted); }
.hm-foot .wo-pill { margin-inline-start: auto; align-self: center; }
.hm-work { display: grid; grid-template-columns: minmax(0, 1.6fr) minmax(0, 1fr); gap: 16px; }
.hm-panel { display: flex; flex-direction: column; gap: 14px; padding: 20px; border-radius: 16px; background: #fff; border: 1px solid var(--wx-line); box-shadow: 0 1px 2px rgba(11, 37, 51, .04); }
.hm-ph { display: flex; align-items: center; gap: 12px; }
.hm-ph > div { flex: 1; min-width: 0; }
.hm-ph p { font-size: 12.5px; }
.hm-more {
  flex: 0 0 auto; display: inline-flex; align-items: center; height: 32px; padding: 0 12px; border-radius: 9px; border: 1px solid var(--wx-line); background: var(--wx-soft);
  font-size: 12.5px; font-weight: 600; color: var(--wx-ink2); transition: all .15s;
}
.hm-more:hover { background: var(--wx-teal-d); border-color: var(--wx-teal-d); color: #fff; }
.hm-tl, .hm-sc { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 6px; }
.hm-tl a {
  display: grid; grid-template-columns: minmax(0, 1.4fr) minmax(0, 1fr) auto; gap: 12px; align-items: center; padding: 10px 12px; border-radius: 12px;
  border: 1px solid var(--wx-line); background: #fff; color: var(--wx-ink); transition: border-color .15s, box-shadow .15s;
}
.hm-tl a:hover, .hm-sc a:hover { border-color: #B9CBD1; box-shadow: 0 8px 20px -14px rgba(11, 37, 51, .35); }
.hm-tl a > span { min-width: 0; }
.hm-tl b { display: block; font-size: 13.5px; font-weight: 700; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.hm-tl b.hm-sm { font-size: 12.5px; font-weight: 600; }
.hm-tl small, .hm-sc small { display: block; margin-top: 2px; font-size: 11.5px; color: var(--wx-muted); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.hm-r { text-align: end; }
.hm-empty { padding: 18px; border-radius: 12px; border: 1px dashed var(--wx-line); font-size: 13px; color: var(--wx-muted); text-align: center; }
.hm-err { color: var(--wx-red); border-color: #F6CFCB; background: var(--wx-red-l); }
.hm-sc a {
  display: flex; align-items: center; gap: 12px; padding: 10px 12px; border-radius: 12px; border: 1px solid var(--wx-line); color: var(--wx-ink);
  transition: border-color .15s, box-shadow .15s;
}
.hm-sc a > span:not(.hm-li) { flex: 1; min-width: 0; }
.hm-sc b { display: block; font-size: 13.5px; font-weight: 700; }
.hm-li { flex: 0 0 30px; width: 30px; height: 30px; border-radius: 9px; display: grid; place-items: center; background: var(--wx-soft); color: var(--wx-teal); font-size: 13px; }
.hm-arr { font-size: 11px; color: var(--wx-muted); }
.hm-ico .pi { font-size: 18px; }
.hm-go .pi { font-size: 12px; }
.hm-sc { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); }
@media (max-width: 1200px) { .hm-mods { grid-template-columns: repeat(2, minmax(0, 1fr)); } .hm-work { grid-template-columns: 1fr; } }
@media (max-width: 980px) { .hm-hero { grid-template-columns: 1fr; } }
@media (max-width: 640px) { .hm-mods { grid-template-columns: 1fr; } .hm-tl a { grid-template-columns: 1fr; gap: 6px; } .hm-r { text-align: start; } }
</style>
