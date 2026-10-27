<template>
  <!-- My workflow inbox in the app 118 "Task inbox monitor" design (page 920): hero with the counts, search bar, status chips and the task table. -->
  <div class="wx wo wx-page">
    <section class="wx-hero">
      <div class="wr-hero">
        <div>
          <span class="wx-eyebrow"><i></i>Live operations &middot; My inbox</span>
          <h1>My workflow inbox</h1>
          <p>Every task waiting for you or for your roles: the step it is on, who it is assigned to and when it is due. Open a task to act on it.</p>
        </div>
        <div class="wr-cta">
          <router-link class="wx-btn wx-btn--ghost" to="/workflowengine"><i class="pi pi-sitemap"></i><span>Workflow registry</span></router-link>
          <button class="wx-btn wx-btn--hot" type="button" :disabled="loading" @click="load">
            <i :class="loading ? 'pi pi-spin pi-spinner' : 'pi pi-refresh'"></i><span>Refresh</span>
          </button>
        </div>
      </div>
      <div class="wr-kpis wr-kpis--5">
        <div class="wr-kpi"><b>{{ tasks.length }}</b><span>Tasks in my inbox</span></div>
        <div class="wr-kpi is-warm"><b>{{ openCount }}</b><span>Not claimed yet</span></div>
        <div class="wr-kpi is-ok"><b>{{ claimedByMeCount }}</b><span>Claimed by me</span></div>
        <div class="wr-kpi"><b>{{ inProgressCount }}</b><span>In progress</span></div>
        <div class="wr-kpi" :class="{ 'is-hot': overdueCount > 0 }"><b>{{ overdueCount }}</b><span>Past due date</span></div>
      </div>
    </section>

    <div class="wr-bar" role="search">
      <label class="wr-search">
        <i class="pi pi-search"></i>
        <input v-model="search" type="search" placeholder="Search task, reference, client, step or workflow" aria-label="Search tasks" />
      </label>
      <select v-model="workflowFilter" class="wr-sel" aria-label="Workflow">
        <option value="">All workflows</option>
        <option v-for="item in workflowOptions" :key="item" :value="item">{{ item }}</option>
      </select>
    </div>

    <div class="wo-sub">
      <div class="wr-chips" role="group" aria-label="Task status">
        <button type="button" class="wx-chip" :aria-pressed="status === ''" @click="status = ''">All <em>{{ tasks.length }}</em></button>
        <button type="button" class="wx-chip" :aria-pressed="status === 'OPEN'" @click="status = 'OPEN'">Open <em>{{ openCount }}</em></button>
        <button type="button" class="wx-chip" :aria-pressed="status === 'CLAIMED'" @click="status = 'CLAIMED'">Claimed <em>{{ countOf('CLAIMED') }}</em></button>
        <button type="button" class="wx-chip" :aria-pressed="status === 'IN_PROGRESS'" @click="status = 'IN_PROGRESS'">In progress <em>{{ countOf('IN_PROGRESS') }}</em></button>
      </div>
      <span class="wo-count"><b>{{ filtered.length }}</b> of <b>{{ tasks.length }}</b> tasks</span>
    </div>

    <div v-if="error" class="wr-error">{{ error }}</div>

    <div class="wo-tw">
      <table class="wo-table">
        <thead>
          <tr>
            <th>Task</th><th>Workflow</th><th>Step</th><th>Assigned to</th><th>Claimed by</th><th>Status</th><th>SLA / due</th><th>Received</th>
            <th><span class="sr-only">Open</span></th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="t in filtered" :key="t.inboxId" class="wo-row" @click="openTask(t.inboxId)">
            <td>
              <div class="wo-t">
                <code>{{ t.entityRef || '#' + t.taskId }}</code>
                <b>{{ t.subject || t.stepName }}</b>
                <small>{{ [t.clientName, pretty(t.entityType), t.amount != null ? t.amount.toLocaleString('en-US') + ' ' + (t.currencyCode || '') : ''].filter(Boolean).join(' · ') }}</small>
              </div>
            </td>
            <td><b>{{ t.workflowName || t.workflowCode }}</b><small>{{ t.workflowCode }}</small></td>
            <td><b>{{ t.stepName }}</b><small>{{ t.stepCode }}</small></td>
            <td><b>{{ assignmentLabel(t) }}</b><small>{{ pretty(t.assignmentType) }}</small></td>
            <td>
              <template v-if="t.claimedBy"><b>{{ t.claimedByName || t.claimedBy }}<span v-if="t.isClaimedByMe" class="wx-tag wo-you">You</span></b><small>{{ formatCreated(t.claimedOn) }}</small></template>
              <span v-else class="wo-dim">Not claimed</span>
            </td>
            <td>
              <span class="wo-pill" :class="statusPill(t.status)">{{ pretty(t.status) }}</span>
              <small v-if="t.priority">{{ pretty(t.priority).toLowerCase() }} priority</small>
            </td>
            <td>
              <span class="wo-pill" :class="slaPill(slaStatus(t))">{{ slaLabel(slaStatus(t)) }}</span>
              <small>{{ t.dueDate ? 'Due ' + formatDue(t.dueDate) : 'No due date' }}</small>
            </td>
            <td><b>{{ formatCreated(t.createdOn) }}</b></td>
            <td class="wo-act">
              <button class="wo-go" type="button" :aria-label="'Open task ' + (t.subject || t.taskId)" @click.stop="openTask(t.inboxId)"><i class="pi pi-arrow-right"></i></button>
            </td>
          </tr>
          <tr v-if="loading && !tasks.length" class="wo-none"><td colspan="9"><b>Loading your inbox…</b></td></tr>
          <tr v-else-if="!filtered.length" class="wo-none">
            <td colspan="9">
              <b>{{ tasks.length ? 'No tasks match these filters' : 'Your inbox is empty' }}</b>
              {{ tasks.length ? 'Try another search or clear the filters.' : 'You are all caught up.' }}
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import { useRouter } from 'vue-router'
  import type { WorkflowInboxTask } from '../models/workflow'
  import { workflowService } from '../services/workflowService'

  const router = useRouter()

  const tasks = ref<WorkflowInboxTask[]>([])

  const openCount = computed(() =>
    tasks.value.filter(t => (t.status || '').toUpperCase() === 'OPEN').length
  )

  const inProgressCount = computed(() =>
    tasks.value.filter(t => ['IN_PROGRESS', 'CLAIMED'].includes((t.status || '').toUpperCase())).length
  )

  const claimedByMeCount = computed(() =>
    tasks.value.filter(t => t.isClaimedByMe === true).length
  )

  const overdueCount = computed(() =>
    tasks.value.filter(t => slaStatus(t) === 'OVERDUE').length
  )
  const loading = ref(false)
  const error = ref('')

  const search = ref('')
  const status = ref('')
  const workflowFilter = ref('')

  const SLA_WARNING_DAYS = 2

  type SlaStatus = 'ON_TRACK' | 'WARNING' | 'OVERDUE' | 'CLOSED'

  function pretty(value?: string | null) {
    if (!value) return ''
    return value.replaceAll('_', ' ')
  }

  function clientNumber(task: WorkflowInboxTask) {
    return (task as WorkflowInboxTask & { clientNumber?: string | null }).clientNumber || ''
  }

  function assignmentLabel(task: WorkflowInboxTask) {
    return task.assignedUserName ||
      task.assignedRole ||
      task.assignedTo ||
      task.assignmentType ||
      'Unassigned'
  }

  function formatCreated(value?: string | null) {
    if (!value) return '-'
    const d = new Date(value)
    if (Number.isNaN(d.getTime())) return value

    return new Intl.DateTimeFormat('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(d)
  }

  function formatDue(value?: string | null) {
    if (!value) return '-'
    const d = new Date(value)
    if (Number.isNaN(d.getTime())) return value

    return new Intl.DateTimeFormat('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    }).format(d)
  }

  function slaStatus(task: WorkflowInboxTask): SlaStatus {
    if (['COMPLETED', 'REJECTED', 'CANCELLED'].includes(task.status))
      return 'CLOSED'

    if (!task.dueDate)
      return 'ON_TRACK'

    const due = new Date(task.dueDate)
    const now = new Date()

    if (due < now)
      return 'OVERDUE'

    const diffDays = (due.getTime() - now.getTime()) / 86400000
    if (diffDays <= SLA_WARNING_DAYS)
      return 'WARNING'

    return 'ON_TRACK'
  }

  function slaLabel(value: SlaStatus) {
    if (value === 'ON_TRACK') return 'On Track'
    if (value === 'WARNING') return 'Due Soon'
    if (value === 'OVERDUE') return 'Overdue'
    return 'Closed'
  }

  const workflowOptions = computed(() => {
    const values = tasks.value
      .map(t => t.workflowName || t.workflowCode)
      .filter((x): x is string => !!x && x.trim().length > 0)

    return [...new Set(values)].sort()
  })

  const filtered = computed(() =>
    tasks.value.filter(t => {
      const q = search.value.trim().toLowerCase()

      const searchable = [
        t.stepName,
        t.entityRef,
        t.entityType,
        t.entityId?.toString(),
        t.clientName,
        clientNumber(t),
        t.workflowName,
        t.workflowCode
      ]

      const searchMatch =
        !q || searchable.some(x => x?.toLowerCase().includes(q))

      const workflowValue = t.workflowName || t.workflowCode || ''

      return searchMatch &&
        (!status.value || t.status === status.value) &&
        (!workflowFilter.value || workflowValue === workflowFilter.value)
    })
  )

  function countOf(value: string) {
    return tasks.value.filter(t => (t.status || '').toUpperCase() === value).length
  }

  function statusPill(value?: string | null) {
    const v = (value || '').toUpperCase()
    if (v === 'OPEN') return 'wo-pill--open'
    if (v === 'CLAIMED') return 'wo-pill--claim'
    if (v === 'IN_PROGRESS') return 'wo-pill--prog'
    if (v === 'COMPLETED') return 'wo-pill--ok'
    if (v === 'REJECTED' || v === 'ERROR') return 'wo-pill--bad'
    return ''
  }

  function slaPill(value: SlaStatus) {
    if (value === 'OVERDUE') return 'wo-pill--bad'
    if (value === 'WARNING') return 'wo-pill--warn'
    if (value === 'ON_TRACK') return 'wo-pill--ok'
    return ''
  }

  function openTask(inboxId: number) {
    router.push({
      name: 'workflow-task',
      params: { inboxId }
    })
  }

  async function load() {
    loading.value = true
    error.value = ''

    try {
      tasks.value = await workflowService.getMyInbox()
    } catch (e) {
      error.value = e instanceof Error ? e.message : 'Unable to load inbox'
    } finally {
      loading.value = false
    }
  }

  onMounted(load)
</script>

<style scoped>
.wo-row { cursor: pointer; }
.wo-you { margin-inline-start: 6px; vertical-align: 1px; }
.sr-only { position: absolute; width: 1px; height: 1px; overflow: hidden; clip: rect(0 0 0 0); }
</style>
