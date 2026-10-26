<template>
  <main class="inbox-page">
    <section class="inbox-shell">

      <!-- HEADER / FILTER BAR -->
      <header class="inbox-header">
        <div class="title-wrap">
          <div class="title-icon"><i class="pi pi-inbox"></i></div>
          <div>
            <h1>Workflow Inbox</h1>
            <p>Manage and action your workflow tasks</p>
          </div>
        </div>

        <div class="header-tools">
          <label class="search-box">
            <i class="pi pi-search"></i>
            <input v-model="search"
                   placeholder="Search by reference, client name or number..." />
          </label>

          <select v-model="workflowFilter" class="filter-select">
            <option value="">All Workflows</option>
            <option v-for="item in workflowOptions" :key="item" :value="item">
              {{ item }}
            </option>
          </select>

          <select v-model="status" class="filter-select">
            <option value="">All Statuses</option>
            <option value="OPEN">OPEN</option>
            <option value="CLAIMED">CLAIMED</option>
            <option value="IN_PROGRESS">IN PROGRESS</option>
          </select>

          <button class="icon-button" type="button" :disabled="loading" @click="load">
            <i :class="loading ? 'pi pi-spin pi-spinner' : 'pi pi-refresh'"></i>
          </button>
        </div>
      </header>

      <!-- INBOX COUNTERS -->
      <section class="summary-cards">
        <div class="summary-card summary-total">
          <span class="summary-icon"><i class="pi pi-list"></i></span>
          <span><strong>{{ tasks.length }}</strong><small>Total Tasks</small></span>
        </div>
        <div class="summary-card summary-open">
          <span class="summary-icon"><i class="pi pi-inbox"></i></span>
          <span><strong>{{ openCount }}</strong><small>Open</small></span>
        </div>
        <div class="summary-card summary-progress">
          <span class="summary-icon"><i class="pi pi-clock"></i></span>
          <span><strong>{{ inProgressCount }}</strong><small>In Progress</small></span>
        </div>
        <div class="summary-card summary-claimed">
          <span class="summary-icon"><i class="pi pi-user-check"></i></span>
          <span><strong>{{ claimedByMeCount }}</strong><small>Claimed By Me</small></span>
        </div>
        <div class="summary-card summary-overdue">
          <span class="summary-icon"><i class="pi pi-exclamation-circle"></i></span>
          <span><strong>{{ overdueCount }}</strong><small>Overdue</small></span>
        </div>
      </section>

      <!-- STATES -->
      <div v-if="loading" class="state-box">
        <i class="pi pi-spin pi-spinner"></i> Loading inbox...
      </div>

      <div v-else-if="error" class="state-box error-box">
        <i class="pi pi-exclamation-circle"></i> {{ error }}
      </div>

      <div v-else-if="filtered.length === 0" class="state-box empty-box">
        <i class="pi pi-inbox"></i>
        <strong>Your inbox is empty</strong>
        <span>No workflow tasks match the selected filters.</span>
      </div>

      <!-- COMPACT TASK CARDS -->
      <section v-else class="task-list">
        <article v-for="(t, index) in pagedTasks"
                 :key="t.inboxId"
                 class="task-card"
                 :class="visualTheme(index)">

          <div class="task-left">
            <div class="document-icon" :class="visualTheme(index)">
              <i class="pi pi-file"></i>
            </div>

            <div class="task-content">
              <div class="identity-row">
                <strong class="workflow-name">
                  {{ t.workflowName || t.workflowCode || 'Workflow' }}
                </strong>

                <span v-if="t.entityType" :class="['entity-badge', 'entity-type', entityTypeClass(t.entityType)]">
                  {{ pretty(t.entityType) }}
                </span>

                <span v-if="t.entityId != null" class="entity-badge entity-id">
                  {{ t.entityId }}
                </span>
              </div>

              <h2 class="task-title">
                {{ t.stepName }}
                <template v-if="t.entityRef">
                  - {{ t.entityRef }}
                </template>
              </h2>

              <div class="client-line">
                <span v-if="t.clientName">{{ t.clientName }}</span>
                <span v-if="t.clientName && clientNumber(t)" class="separator">|</span>
                <span v-if="clientNumber(t)">{{ clientNumber(t) }}</span>
                <span v-if="!t.clientName && !clientNumber(t) && t.entityRef">
                  {{ t.entityRef }}
                </span>
              </div>

              <div class="meta-row">
                <span>
                  <i class="pi pi-user"></i>
                  Assigned to:
                  <strong>{{ assignmentLabel(t) }}</strong>
                </span>

                <template v-if="t.claimedBy">
                  <span class="meta-divider"></span>
                  <span>
                    <i class="pi pi-user-check"></i>
                    Claimed by:
                    <strong>{{ t.claimedByName || t.claimedBy }}</strong>
                  </span>
                </template>

                <span class="meta-divider"></span>

                <span>
                  <i class="pi pi-calendar"></i>
                  Created on:
                  <strong>{{ formatCreated(t.createdOn || t.createdDate) }}</strong>
                </span>

                <span class="meta-divider"></span>

                <span>
                  <i class="pi pi-clock"></i>
                  Due date:
                  <strong :class="{ overdue: slaStatus(t) === 'OVERDUE' }">
                    {{ formatDue(t.dueDate) }}
                  </strong>
                </span>
              </div>
            </div>
          </div>

          <div class="status-area">
            <div class="status-line">
              <span class="status-dot" :class="taskStatusDotClass(t.status)"></span>
              <strong>{{ pretty(t.status) }}</strong>
            </div>

            <span class="sla-pill" :class="slaClass(slaStatus(t))">
              {{ slaLabel(slaStatus(t)) }}
            </span>
          </div>

          <div class="action-area">
            <button type="button" class="open-button" @click="openTask(t.inboxId)">
              <i class="pi pi-arrow-right"></i>
              Open
            </button>
          </div>

          <div class="more-area">
            <button type="button" class="more-button" aria-label="More actions">
              <i class="pi pi-ellipsis-v"></i>
            </button>
          </div>
        </article>
      </section>

      <!-- PAGINATION -->
      <footer v-if="filtered.length > 0" class="pagination">
        <div>
          Showing {{ pageStart }} to {{ pageEnd }} of {{ filtered.length }} items
        </div>

        <div class="pagination-right">
          <span>Rows per page</span>

          <select v-model.number="pageSize" @change="currentPage = 1">
            <option :value="3">3</option>
            <option :value="5">5</option>
            <option :value="10">10</option>
          </select>

          <button type="button" class="page-button"
                  :disabled="currentPage === 1"
                  @click="currentPage = 1">
            <i class="pi pi-angle-double-left"></i>
          </button>

          <button type="button" class="page-button"
                  :disabled="currentPage === 1"
                  @click="currentPage--">
            <i class="pi pi-angle-left"></i>
          </button>

          <button v-for="page in visiblePages"
                  :key="page"
                  type="button"
                  class="page-button"
                  :class="{ active: page === currentPage }"
                  @click="currentPage = page">
            {{ page }}
          </button>

          <button type="button" class="page-button"
                  :disabled="currentPage === totalPages"
                  @click="currentPage++">
            <i class="pi pi-angle-right"></i>
          </button>

          <button type="button" class="page-button"
                  :disabled="currentPage === totalPages"
                  @click="currentPage = totalPages">
            <i class="pi pi-angle-double-right"></i>
          </button>
        </div>
      </footer>

    </section>
  </main>
</template>

<script setup lang="ts">
  import { computed, onMounted, ref, watch } from 'vue'
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

  const currentPage = ref(1)
  const pageSize = ref(3)

  const SLA_WARNING_DAYS = 2

  type SlaStatus = 'ON_TRACK' | 'WARNING' | 'OVERDUE' | 'CLOSED'

  function pretty(value?: string | null) {
    if (!value) return ''
    return value.replaceAll('_', ' ')
  }

  function clientNumber(task: WorkflowInboxTask) {
    return (task as WorkflowInboxTask & { clientNumber?: string | null }).clientNumber || ''
  }

  function entityTypeClass(value?: string | null) {
    const v = (value || '').trim().toUpperCase()
    if (v === 'APPLICATION') return 'entity-application'
    if (v === 'CONTRACT') return 'entity-contract'
    if (v === 'AGREEMENT') return 'entity-agreement'
    return 'entity-generic'
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

  function slaClass(value: SlaStatus) {
    return {
      'sla-on-track': value === 'ON_TRACK',
      'sla-warning': value === 'WARNING',
      'sla-overdue': value === 'OVERDUE',
      'sla-closed': value === 'CLOSED'
    }
  }

  function taskStatusDotClass(status?: string | null) {
    const value = (status || '').toUpperCase()
    return {
      'dot-green': value === 'OPEN',
      'dot-blue': value === 'IN_PROGRESS',
      'dot-yellow': value === 'CLAIMED' || value === 'WAITING',
      'dot-red': value === 'REJECTED' || value === 'ERROR',
      'dot-gray': value === 'COMPLETED' || value === 'CANCELLED'
    }
  }

  function visualTheme(index: number) {
    // Decorative palette only. It does NOT change workflow/SLA meaning.
    // Matches the approved mockup: green, amber, red.
    const themes = ['theme-green', 'theme-amber', 'theme-red']
    return themes[index % themes.length]
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

  const totalPages = computed(() =>
    Math.max(1, Math.ceil(filtered.value.length / pageSize.value))
  )

  const pagedTasks = computed(() => {
    const start = (currentPage.value - 1) * pageSize.value
    return filtered.value.slice(start, start + pageSize.value)
  })

  const pageStart = computed(() =>
    filtered.value.length === 0 ? 0 : (currentPage.value - 1) * pageSize.value + 1
  )

  const pageEnd = computed(() =>
    Math.min(currentPage.value * pageSize.value, filtered.value.length)
  )

  const visiblePages = computed(() => {
    const total = totalPages.value
    const current = currentPage.value
    let start = Math.max(1, current - 1)
    const end = Math.min(total, start + 2)
    start = Math.max(1, end - 2)

    const pages: number[] = []
    for (let p = start; p <= end; p++) pages.push(p)
    return pages
  })

  watch([search, status, workflowFilter], () => {
    currentPage.value = 1
  })

  watch(totalPages, value => {
    if (currentPage.value > value)
      currentPage.value = value
  })

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
  /* Reference-targeted Workflow Inbox */
  .inbox-page {
    padding: 18px 22px 20px;
    min-height: 100%;
    background: #f8fbfc;
    color: #142945;
    font-family: inherit;
  }

  .inbox-shell {
    max-width: 1700px;
    margin: 0 auto;
  }

  /* Header proportions from the approved design */
  .inbox-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 22px;
    margin-bottom: 16px;
  }

  .title-wrap {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .title-icon {
    width: 42px;
    height: 42px;
    display: grid;
    place-items: center;
    border: 2px solid #12866f;
    border-radius: 8px;
    background: #fff;
    color: #12866f;
  }

    .title-icon i {
      font-size: 20px;
    }

  .title-wrap h1 {
    margin: 0;
    color: #142945;
    font-size: 21px;
    line-height: 1.1;
    font-weight: 700;
  }

  .title-wrap p {
    margin: 4px 0 0;
    color: #667990;
    font-size: 12px;
    font-weight: 400;
  }

  .header-tools {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .search-box {
    width: 340px;
    height: 38px;
    display: flex;
    align-items: center;
    gap: 9px;
    padding: 0 12px;
    border: 1px solid #d6e0e7;
    border-radius: 6px;
    background: #fff;
  }

    .search-box i {
      color: #294c70;
      font-size: 13px;
    }

    .search-box input {
      width: 100%;
      border: 0;
      outline: 0;
      background: transparent;
      color: #455a70;
      font-size: 12px;
      font-weight: 400;
    }

  .filter-select,
  .pagination select {
    height: 38px;
    border: 1px solid #d6e0e7;
    border-radius: 6px;
    background: #fff;
    color: #233b57;
    padding: 0 32px 0 12px;
    outline: none;
    font-size: 12px;
    font-weight: 600;
  }

  .icon-button {
    width: 38px;
    height: 38px;
    border: 1px solid #d6e0e7;
    border-radius: 6px;
    background: #fff;
    color: #294864;
    cursor: pointer;
  }

  /* Summary counters */
  .summary-cards {
    display: grid;
    grid-template-columns: repeat(5, minmax(140px, 1fr));
    gap: 12px;
    margin: 12px 0;
  }

  .summary-card {
    min-height: 64px;
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px 14px;
    background: #fff;
    border: 1px solid #dfe6eb;
    border-radius: 8px;
    box-shadow: 0 2px 8px rgba(24, 50, 75, .04);
  }

    .summary-card .summary-icon {
      width: 36px;
      height: 36px;
      display: grid;
      place-items: center;
      border-radius: 8px;
      font-size: 15px;
    }

    .summary-card > span:last-child {
      display: flex;
      flex-direction: column;
      gap: 1px;
    }

    .summary-card strong {
      color: #142945;
      font-size: 17px;
      line-height: 1;
      font-weight: 700;
    }

    .summary-card small {
      color: #64778b;
      font-size: 10px;
      font-weight: 600;
    }

  .summary-total {
    border-top: 3px solid #315b7d;
  }

    .summary-total .summary-icon {
      background: #edf3f8;
      color: #315b7d;
    }

  .summary-open {
    border-top: 3px solid #27b97f;
  }

    .summary-open .summary-icon {
      background: #e8f7f1;
      color: #138b67;
    }

  .summary-progress {
    border-top: 3px solid #3b82f6;
  }

    .summary-progress .summary-icon {
      background: #eaf2ff;
      color: #2563c9;
    }

  .summary-claimed {
    border-top: 3px solid #7c5ce5;
  }

    .summary-claimed .summary-icon {
      background: #f0edff;
      color: #6848d6;
    }

  .summary-overdue {
    border-top: 3px solid #ef4444;
  }

    .summary-overdue .summary-icon {
      background: #ffebeb;
      color: #dc3030;
    }

  /* Cards */
  .task-list {
    display: grid;
    gap: 12px;
  }

  .task-card {
    position: relative;
    display: grid;
    grid-template-columns: minmax(0, 1fr) 180px 165px 44px;
    min-height: 116px;
    background: #fff;
    border: 1px solid #dfe6eb;
    border-radius: 8px;
    overflow: hidden;
    box-shadow: 0 3px 10px rgba(24, 50, 75, .05);
  }

    .task-card::before {
      content: "";
      position: absolute;
      inset: 0 auto 0 0;
      width: 5px;
    }

  .theme-green::before {
    background: #22bf91;
  }

  .theme-amber::before {
    background: #e5a11a;
  }

  .theme-red::before {
    background: #ef3d43;
  }

  .task-left {
    display: flex;
    align-items: center;
    gap: 14px;
    min-width: 0;
    padding: 14px 20px 13px 24px;
  }

  .document-icon {
    width: 44px;
    height: 44px;
    flex: 0 0 44px;
    display: grid;
    place-items: center;
    border-radius: 10px;
    background: #eef3f6;
    color: #526b80;
  }

    .document-icon.theme-green {
      background: #e8f7f2;
      color: #109679;
    }

    .document-icon.theme-amber {
      background: #fff3d9;
      color: #c27a00;
    }

    .document-icon.theme-red {
      background: #ffe5e7;
      color: #ef3038;
    }

    .document-icon i {
      font-size: 18px;
    }

  .task-content {
    width: 100%;
    min-width: 0;
  }

  .identity-row {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
    margin-bottom: 5px;
  }

  .workflow-name {
    color: #142945;
    font-size: 13px;
    font-weight: 700;
  }

  .entity-badge {
    height: 26px;
    min-width: 62px;
    padding: 0 13px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    border-radius: 5px;
    color: #fff;
    font-size: 10px;
    line-height: 1;
    font-weight: 700;
    letter-spacing: .01em;
  }

  .entity-agreement {
    background: #13765f;
  }

  .entity-application {
    background: #98631f;
  }

  .entity-contract {
    background: #684073;
  }

  .entity-generic {
    background: #536779;
  }

  .entity-id {
    min-width: 58px;
    background: #24496d;
  }

  .task-title {
    margin: 0 0 4px;
    color: #142945;
    font-size: 12px;
    line-height: 1.3;
    font-weight: 650;
  }

  .client-line {
    min-height: 16px;
    margin-bottom: 8px;
    color: #5d7189;
    font-size: 11px;
    line-height: 1.3;
    font-weight: 400;
  }

  .separator {
    margin: 0 7px;
    color: #a4b0bb;
  }

  .meta-row {
    display: flex;
    align-items: center;
    flex-wrap: wrap;
    gap: 8px;
    color: #63768b;
    font-size: 10px;
    font-weight: 400;
  }

    .meta-row > span:not(.meta-divider) {
      display: inline-flex;
      align-items: center;
      gap: 5px;
    }

    .meta-row i {
      color: #345b7c;
      font-size: 10px;
    }

    .meta-row strong {
      color: #2b425a;
      font-weight: 600;
    }

    .meta-row .overdue {
      color: #dc2929;
    }

  .meta-divider {
    width: 1px;
    height: 13px;
    background: #ccd6dd;
  }

  .status-area,
  .action-area,
  .more-area {
    border-left: 1px solid #e1e8ed;
  }

  .status-area {
    display: flex;
    flex-direction: column;
    justify-content: center;
    align-items: flex-start;
    gap: 8px;
    padding: 15px 22px;
  }

  .status-line {
    display: flex;
    align-items: center;
    gap: 9px;
    color: #1d344f;
    font-size: 11px;
    font-weight: 650;
  }

  .status-dot {
    width: 10px;
    height: 10px;
    border-radius: 50%;
  }

  .dot-green {
    background: #43c56b;
  }

  .dot-blue {
    background: #3b82f6;
  }

  .dot-yellow {
    background: #e7aa1e;
  }

  .dot-red {
    background: #f02f2f;
  }

  .dot-gray {
    background: #9ba8b3;
  }

  .sla-pill {
    min-width: 82px;
    padding: 4px 12px;
    border-radius: 8px;
    text-align: center;
    font-size: 10px;
    line-height: 1.2;
    font-weight: 650;
  }

  .sla-on-track {
    background: #e2f2d2;
    color: #26721c;
  }

  .sla-warning {
    background: #fff0c8;
    color: #96640a;
  }

  .sla-overdue {
    background: #ffe0e0;
    color: #df2929;
  }

  .sla-closed {
    background: #edf0f3;
    color: #667481;
  }

  .action-area {
    display: flex;
    align-items: center;
    justify-content: center;
    padding: 14px;
  }

  .open-button {
    height: 32px;
    min-width: 108px;
    padding: 0 17px;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 10px;
    border: 1px solid #158e78;
    border-radius: 6px;
    background: #fff;
    color: #167b69;
    font-size: 12px;
    font-weight: 650;
    cursor: pointer;
  }

    .open-button i {
      font-size: 11px;
    }

    .open-button:hover {
      background: #f4faf8;
    }

  .more-area {
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .more-button {
    width: 30px;
    height: 34px;
    border: 0;
    background: transparent;
    color: #17324f;
    cursor: pointer;
    border-radius: 5px;
  }

    .more-button:hover {
      background: #f2f5f7;
    }

  /* Pagination */
  .pagination {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 18px;
    padding: 14px 2px 0;
    color: #617389;
    font-size: 11px;
    font-weight: 400;
  }

  .pagination-right {
    display: flex;
    align-items: center;
    gap: 7px;
  }

  .pagination select {
    height: 34px;
    min-width: 58px;
    padding-right: 22px;
  }

  .page-button {
    width: 34px;
    height: 34px;
    border: 1px solid #d6e0e6;
    border-radius: 6px;
    background: #fff;
    color: #304860;
    font-size: 11px;
    font-weight: 600;
    cursor: pointer;
  }

    .page-button.active {
      border-color: #147861;
      background: #147861;
      color: #fff;
    }

    .page-button:disabled {
      opacity: .38;
      cursor: default;
    }

  .state-box {
    min-height: 140px;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    border: 1px solid #e0e6eb;
    border-radius: 8px;
    background: #fff;
    color: #66788c;
    font-size: 12px;
  }

  .empty-box {
    flex-direction: column;
  }

    .empty-box > i {
      color: #178d78;
      font-size: 24px;
    }

  .error-box {
    color: #b42318;
  }

  @media (max-width: 900px) {
    .summary-cards {
      grid-template-columns: repeat(2, minmax(140px, 1fr));
    }
  }

  @media (max-width: 1100px) {
    .inbox-header {
      align-items: flex-start;
      flex-direction: column;
    }

    .header-tools {
      width: 100%;
      flex-wrap: wrap;
    }

    .search-box {
      flex: 1 1 320px;
    }

    .task-card {
      grid-template-columns: minmax(0, 1fr) 150px 135px 40px;
    }
  }

  @media (max-width: 760px) {
    .task-card {
      display: block;
    }

    .status-area, .action-area, .more-area {
      border-left: 0;
      border-top: 1px solid #e1e8ed;
    }

    .status-area {
      flex-direction: row;
      align-items: center;
    }

    .action-area {
      justify-content: flex-end;
    }

    .more-area {
      display: none;
    }

    .pagination {
      align-items: flex-start;
      flex-direction: column;
    }

    .pagination-right {
      flex-wrap: wrap;
    }
  }
</style>
