<template>
  <main class="wf-task-page">

    <button type="button"
            class="wf-task-back"
            @click="router.push({ name: 'workflow-inbox' })">
      <i class="pi pi-arrow-left"></i>
      Back to inbox
    </button>

    <div v-if="loading" class="wf-task-loading">
      <i class="pi pi-spin pi-spinner"></i>
      Loading task...
    </div>

    <div v-else-if="error" class="wf-task-error">
      <i class="pi pi-exclamation-circle"></i>
      {{ error }}
    </div>

    <template v-else-if="task">

      <section class="wf-task-hero">
        <div class="wf-task-hero-main">
          <div class="wf-task-hero-icon">
            <i class="pi pi-file-edit"></i>
          </div>

          <div class="wf-task-hero-copy">
            <div class="wf-task-tags">
              <span class="wf-task-tag">
                {{ task.moduleCode || 'WORKFLOW' }}
              </span>

              <span class="wf-task-tag wf-task-tag-status">
                {{ pretty(task.status) }}
              </span>
            </div>

            <h1>{{ task.stepName }}</h1>

            <p class="wf-task-hero-ref">
              {{ task.entityRef || `Task #${task.inboxId}` }}
            </p>
          </div>
        </div>

        <div class="wf-task-hero-actions">
          <button v-if="task.status === 'OPEN'"
                  type="button"
                  class="wf-action-button wf-action-claim"
                  :disabled="actionLoading"
                  @click="claimTask">
            <i class="pi pi-user-plus"></i>
            Claim
          </button>

          <button v-if="task.status === 'CLAIMED' || task.status === 'IN_PROGRESS'"
                  type="button"
                  class="wf-action-button wf-action-start"
                  :disabled="actionLoading || !task.route"
                  @click="runTask">
            <i class="pi pi-play"></i>
            Run Task
          </button>

          <button v-if="task.status === 'CLAIMED' || task.status === 'IN_PROGRESS'"
                  type="button"
                  class="wf-action-button wf-action-open"
                  :disabled="actionLoading || !task.route"
                  @click="openBusinessScreen">
            <i class="pi pi-external-link"></i>
            {{ displayEntityButtonText }}
          </button>
        </div>
      </section>

      <section class="wf-context-card">
        <div class="wf-context-main">
          <div class="wf-context-entity">
            <div class="wf-context-entity-copy">
              <div class="wf-context-badges">
                <span class="wf-context-type">{{ entityLabel }}</span>
                <span class="wf-context-id">#{{ task.entityId ?? '-' }}</span>
              </div>
              <h3>{{ task.entityRef || '-' }}</h3>
              <div class="wf-context-app-ref">
                <label>Application Reference</label>
                <strong>{{ applicationReferenceText }}</strong>
              </div>
            </div>
          </div>

          <div class="wf-context-info wf-context-info-middle">
            <div class="wf-context-field">
              <span class="wf-context-field-icon"><i class="pi pi-sitemap"></i></span>
              <div>
                <label>Workflow Name</label>
                <strong>{{ workflowNameText }}</strong>
              </div>
            </div>

            <div class="wf-context-field">
              <span class="wf-context-field-icon"><i class="pi pi-building"></i></span>
              <div>
                <label>Customer Name</label>
                <strong>{{ task.clientName || '-' }}</strong>
              </div>
            </div>

            <div class="wf-context-field">
              <span class="wf-context-field-icon"><i class="pi pi-user"></i></span>
              <div>
                <label>Customer Number</label>
                <strong>{{ customerNumber }}</strong>
              </div>
            </div>
          </div>

          <div class="wf-context-info wf-context-info-right">
            <div class="wf-context-field">
              <span class="wf-context-field-icon"><i class="pi pi-tag"></i></span>
              <div>
                <label>Amount</label>
                <strong>{{ amountText }}</strong>
              </div>
            </div>

            <div class="wf-context-field">
              <span class="wf-context-field-icon"><i class="pi pi-wallet"></i></span>
              <div>
                <label>Currency</label>
                <strong>{{ task.currencyCode || '-' }}</strong>
              </div>
            </div>

            <div class="wf-context-field">
              <span class="wf-context-field-icon"><i class="pi pi-file-edit"></i></span>
              <div>
                <label>Business Reference</label>
                <strong>{{ businessReferenceText }}</strong>
              </div>
            </div>
          </div>
        </div>

        <div class="wf-context-summary">
          <div class="wf-context-summary-item">
            <span class="wf-context-summary-icon wf-priority-icon"><i class="pi pi-flag"></i></span>
            <div>
              <label>Priority</label>
              <span class="wf-context-priority">{{ task.priority || 'NORMAL' }}</span>
            </div>
          </div>

          <div class="wf-context-summary-item">
            <span class="wf-context-summary-icon"><i class="pi pi-user"></i></span>
            <div>
              <label>Assigned To</label>
              <strong>{{ assignmentText }}</strong>
            </div>
          </div>

          <div class="wf-context-summary-item">
            <span class="wf-context-summary-icon"><i class="pi pi-user-check"></i></span>
            <div>
              <label>Claimed By</label>
              <strong>{{ task.claimedByName || task.claimedBy || '-' }}</strong>
            </div>
          </div>

          <div class="wf-context-summary-item">
            <span class="wf-context-summary-icon"><i class="pi pi-calendar"></i></span>
            <div>
              <label>Creation Date</label>
              <strong>{{ formatDateTime(task.createdOn) }}</strong>
            </div>
          </div>

          <div class="wf-context-summary-item">
            <span class="wf-context-summary-icon"><i class="pi pi-share-alt"></i></span>
            <div>
              <label>Current Step</label>
              <strong>{{ task.stepName || '-' }}</strong>
            </div>
          </div>

          <div class="wf-context-summary-item">
            <span class="wf-context-summary-icon"><i class="pi pi-clock"></i></span>
            <div>
              <label>Due Date</label>
              <strong>{{ formatDateTime(task.dueDate) }}</strong>
            </div>
          </div>
        </div>

        <div v-if="task.instructions" class="wf-context-instructions">
          <i class="pi pi-info-circle"></i>
          <span>{{ task.instructions }}</span>
        </div>
      </section>

      <section class="wf-business-screen-card">
        <div class="wf-business-screen-header wf-task-card-header-green">
          <i class="pi pi-desktop"></i>
          Business Screen
        </div>

        <div class="wf-business-screen-body">
          <template v-if="task.status === 'OPEN'">
            Claim this task first. The Run Task button will then become available.
          </template>

          <template v-else-if="task.status === 'CLAIMED'">
            Select Run Task to start processing. You can use Run Task again later to return to the working screen.
          </template>

          <template v-else-if="task.status === 'IN_PROGRESS'">
            This task is in progress. Use Run Task whenever you need to return to the working screen.
          </template>

          <template v-else>
            This task is currently {{ pretty(task.status).toLowerCase() }}.
          </template>
        </div>
      </section>

      <section v-if="task.status === 'IN_PROGRESS'"
               class="wf-completion-panel">
        <label for="completionComment">
          Completion Comment
        </label>

        <textarea id="completionComment"
                  v-model="completionComment"
                  placeholder="Optional completion comment"></textarea>

        <div class="wf-completion-actions">
          <button
            v-for="action in completionActions"
            :key="action"
            type="button"
            class="wf-action-button"
            :class="actionButtonClass(action)"
            :disabled="actionLoading"
            @click="executeTaskAction(action)"
          >
            <i :class="actionIcon(action)"></i>
            {{ actionLabel(action) }}
          </button>
        </div>
      </section>

    </template>
  </main>
</template>

<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import { useRoute, useRouter } from 'vue-router'
  import type { WorkflowInboxTask } from '../models/workflow'
  import { workflowService } from '../services/workflowService'
  import '../styles/workflow.css'

  const route = useRoute()
  const router = useRouter()

  const task = ref<WorkflowInboxTask | null>(null)
  const loading = ref(false)
  const actionLoading = ref(false)
  const error = ref('')
  const completionComment = ref('')

  const inboxId = computed(() =>
    Number(route.params.inboxId)
  )

  const entityLabel = computed(() =>
    task.value?.entityType?.trim() || 'Entity'
  )

  const displayEntityButtonText = computed(() =>
    `Display ${entityLabel.value}`
  )

  const workflowNameText = computed(() =>
    (task.value?.workflowName || task.value?.workflowCode || '-').replaceAll('_', ' ')
  )

  const customerNumber = computed(() => {
    if (!task.value) return '-'
    const extended = task.value as WorkflowInboxTask & {
      clientNumber?: string | null
      customerNumber?: string | null
    }
    return extended.clientNumber || extended.customerNumber ||
      (task.value.clientId != null ? String(task.value.clientId) : '-')
  })

  const businessReferenceText = computed(() => {
    if (!task.value) return '-'
    const extended = task.value as WorkflowInboxTask & {
      businessReference?: string | null
      applicationReference?: string | null
      applicationRef?: string | null
    }
    return extended.businessReference ||
      task.value.entityRef ||
      extended.applicationReference ||
      extended.applicationRef ||
      '-'
  })

  const applicationReferenceText = computed(() => {
    if (!task.value) return '-'
    const extended = task.value as WorkflowInboxTask & {
      applicationReference?: string | null
      applicationRef?: string | null
      businessReference?: string | null
    }
    return extended.applicationReference ||
      extended.applicationRef ||
      extended.businessReference ||
      task.value.entityRef ||
      '-'
  })

  const assignmentText = computed(() => {
    if (!task.value) return '-'
    return task.value.assignedUserName ||
      task.value.assignedRole ||
      task.value.assignedGroup ||
      task.value.assignedTo ||
      '-'
  })

  const amountText = computed(() => {
    if (!task.value || task.value.amount == null)
      return '-'

    const currency = task.value.currencyCode || 'EGP'

    try {
      return new Intl.NumberFormat('en-US', {
        style: 'currency',
        currency,
        maximumFractionDigits: 2
      }).format(task.value.amount)
    } catch {
      return `${currency} ${task.value.amount.toLocaleString()}`
    }
  })

  // Render task-completion actions from the workflow definition.
  // CLAIM remains handled by the existing Claim button while the task is OPEN,
  // so it is intentionally excluded from the IN_PROGRESS completion panel.
  const completionActions = computed(() => {
    if (!task.value || task.value.status !== 'IN_PROGRESS')
      return []

    const configuredActions = (task.value.allowedActions || '')
      .split(',')
      .map(action => action.trim().toUpperCase())
      .filter(Boolean)
      .filter(action => action !== 'CLAIM')

    // Preserve the existing behavior for older workflow tasks that do not
    // contain AllowedActions.
    return configuredActions.length > 0
      ? configuredActions
      : ['COMPLETE']
  })

  function actionLabel(action: string) {
    return pretty(action)
  }

  function actionIcon(action: string) {
    switch (action.toUpperCase()) {
      case 'ACCEPT':
        return 'pi pi-check'
      case 'APPROVE':
        return 'pi pi-thumbs-up'
      case 'REJECT':
        return 'pi pi-times-circle'
      case 'RETURN':
        return 'pi pi-replay'
      case 'COMPLETE':
        return 'pi pi-check-circle'
      default:
        return 'pi pi-check'
    }
  }

  function actionButtonClass(action: string) {
    switch (action.toUpperCase()) {
      case 'ACCEPT':
      case 'APPROVE':
        return 'wf-action-accept'
      case 'REJECT':
        return 'wf-action-reject'
      case 'RETURN':
        return 'wf-action-return'
      case 'COMPLETE':
        return 'wf-action-complete'
      default:
        return 'wf-action-complete'
    }
  }

  function pretty(value?: string | null) {
    if (!value) return ''
    return value.replaceAll('_', ' ')
  }

  function formatDateTime(value?: string | null) {
    if (!value) return '-'

    const date = new Date(value)

    if (Number.isNaN(date.getTime()))
      return value

    return new Intl.DateTimeFormat('en-US', {
      year: 'numeric',
      month: 'numeric',
      day: 'numeric',
      hour: 'numeric',
      minute: '2-digit',
      second: '2-digit'
    }).format(date)
  }

  async function load() {
    if (!Number.isFinite(inboxId.value)) {
      error.value = 'Invalid workflow inbox task ID.'
      return
    }

    loading.value = true
    error.value = ''

    try {
      task.value = await workflowService.getTask(inboxId.value)
    } catch (e) {
      error.value =
        e instanceof Error
          ? e.message
          : 'Unable to load workflow task.'
    } finally {
      loading.value = false
    }
  }

  async function claimTask() {
    if (!task.value) return

    actionLoading.value = true
    error.value = ''

    try {
      task.value = await workflowService.claim(task.value.inboxId)
    } catch (e) {
      error.value =
        e instanceof Error
          ? e.message
          : 'Unable to claim workflow task.'
    } finally {
      actionLoading.value = false
    }
  }

  async function runTask() {
    if (!task.value) return

    actionLoading.value = true
    error.value = ''

    try {
      // First visit after claiming:
      // move the task from CLAIMED -> IN_PROGRESS.
      if (task.value.status === 'CLAIMED') {
        task.value = await workflowService.start(task.value.inboxId)
      }

      // Subsequent visits while IN_PROGRESS:
      // do NOT call Start again; simply reopen the business screen.
      if (task.value.route) {
        openBusinessScreen()
      }
    } catch (e) {
      error.value =
        e instanceof Error
          ? e.message
          : 'Unable to run workflow task.'
    } finally {
      actionLoading.value = false
    }
  }

  function openBusinessScreen() {
    if (!task.value?.route)
      return

    const params: Record<string, string> = {}

    if (task.value.routeParameters) {
      try {
        const routeParams = JSON.parse(task.value.routeParameters) as Array<{
          Name?: string
          Value?: string | number | null
          name?: string
          value?: string | number | null
        }>

        for (const parameter of routeParams) {
          const name = (parameter.name ?? parameter.Name)?.trim()
          const value = parameter.value ?? parameter.Value

          if (
            name &&
            value !== null &&
            value !== undefined &&
            String(value).trim() !== ''
          ) {
            params[name] = String(value)
          }
        }
      } catch {
        // Keep navigation available even when optional route metadata is malformed.
      }
    }

    // Parameters named as a page item (P209_PORTFOLIO_ID) travel in the query as copied APEX pages read them
    // (?p209_portfolio_id=); any other parameter fills the route's own path parameter.
    const items = Object.fromEntries(
      Object.entries(params).filter(([name]) => /^P\d+_/i.test(name)).map(([name, value]) => [name.toLowerCase(), value])
    )
    const pathParams = Object.fromEntries(Object.entries(params).filter(([name]) => !/^P\d+_/i.test(name)))

    router.push({
      name: task.value.route,
      params: Object.keys(pathParams).length > 0 ? pathParams : undefined,
      query: {
        ...items,
        inboxId: String(task.value.inboxId)
      }
    })
  }

  async function executeTaskAction(action: string) {
    if (!task.value) return

    actionLoading.value = true
    error.value = ''

    try {
      task.value = await workflowService.complete(
        task.value.inboxId,
        {
          action,
          comment: completionComment.value || null
        }
      )

      await router.push({
        name: 'workflow-inbox'
      })
    } catch (e) {
      error.value =
        e instanceof Error
          ? e.message
          : `Unable to ${actionLabel(action).toLowerCase()} workflow task.`
    } finally {
      actionLoading.value = false
    }
  }

  onMounted(load)
</script>


<style scoped>
  .wf-context-card {
    margin-top: 18px;
    background: #fff;
    border: 1px solid #dbe5ec;
    border-radius: 14px;
    box-shadow: 0 4px 16px rgba(28,55,82,.05);
    padding: 22px 26px 20px;
    color: #0b2345;
  }

  .wf-context-main {
    display: grid;
    grid-template-columns: 1.2fr .95fr .9fr;
    border: 1px solid #dce6ed;
    border-radius: 12px;
    padding: 22px 24px;
    min-height: 230px
  }

  .wf-context-entity {
    display: block;
    padding: 12px 28px 12px 6px
  }

  .wf-context-badges {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 16px
  }

  .wf-context-type, .wf-context-id {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    height: 34px;
    border-radius: 7px;
    padding: 0 18px;
    font-size: 14px;
    font-weight: 750
  }

  .wf-context-type {
    background: #078b6c;
    color: #fff
  }

  .wf-context-id {
    background: #e7f1ff;
    color: #0863b9
  }

  .wf-context-entity h3 {
    margin: 0 0 12px;
    font-size: 22px;
    font-weight: 750;
    letter-spacing: .01em
  }

  .wf-context-entity p {
    margin: 0;
    color: #657995;
    font-size: 15px
  }

  .wf-context-separator {
    margin: 0 12px;
    color: #c3cdd6
  }

  .wf-context-info {
    display: flex;
    flex-direction: column;
    justify-content: center;
    gap: 22px;
    padding: 8px 26px
  }

  .wf-context-info-middle, .wf-context-info-right {
    border-left: 1px solid #d5e0e8
  }

  .wf-context-field {
    display: grid;
    grid-template-columns: 42px 1fr;
    align-items: center;
    gap: 12px
  }

  .wf-context-field-icon, .wf-context-summary-icon {
    display: grid;
    place-items: center;
    background: #edf5fc;
    color: #426b98;
    border-radius: 10px
  }

  .wf-context-field-icon {
    width: 42px;
    height: 42px;
    font-size: 17px
  }

  .wf-context-field label, .wf-context-summary-item label {
    display: block;
    color: #657995;
    font-size: 12px;
    margin-bottom: 4px;
    font-weight: 500
  }

  .wf-context-field strong {
    font-size: 15px;
    font-weight: 650;
    color: #0b2345
  }

  .wf-context-summary {
    display: grid;
    grid-template-columns: .75fr 1.05fr 1.05fr .95fr 1fr .9fr;
    border-top: 1px solid #dce6ed;
    margin-top: 18px;
    padding-top: 18px
  }

  .wf-context-summary-item {
    display: grid;
    grid-template-columns: 42px 1fr;
    align-items: center;
    gap: 11px;
    padding: 5px 20px;
    border-right: 1px solid #dce6ed;
    min-height: 62px
  }

    .wf-context-summary-item:first-child {
      padding-left: 6px
    }

    .wf-context-summary-item:last-child {
      border-right: 0
    }

  .wf-context-summary-icon {
    width: 42px;
    height: 42px;
    font-size: 17px
  }

  .wf-priority-icon {
    background: #ffeded;
    color: #e52f37
  }

  .wf-context-summary-item strong {
    font-size: 13px;
    font-weight: 650;
    color: #0b2345
  }

  .wf-context-priority {
    display: inline-flex;
    padding: 5px 14px;
    border-radius: 7px;
    background: #ffe4e4;
    color: #df3037;
    font-size: 12px;
    font-weight: 750;
    text-transform: uppercase
  }

  .wf-context-instructions {
    display: flex;
    align-items: flex-start;
    gap: 9px;
    margin-top: 16px;
    padding: 11px 13px;
    background: #f5f9fc;
    border-radius: 8px;
    color: #516a84;
    font-size: 12px
  }

  @media(max-width:1050px) {
    .wf-context-main {
      grid-template-columns: 1fr
    }

    .wf-context-info-middle, .wf-context-info-right {
      border-left: 0;
      border-top: 1px solid #d5e0e8
    }

    .wf-context-info {
      padding: 18px 6px
    }

    .wf-context-summary {
      grid-template-columns: repeat(2,1fr)
    }

    .wf-context-summary-item {
      border-right: 0;
      border-bottom: 1px solid #e2e9ee
    }
  }

  .wf-context-app-ref {
    margin-top: 18px
  }

    .wf-context-app-ref label {
      display: block;
      color: #657995;
      font-size: 12px;
      margin-bottom: 5px;
      font-weight: 500
    }

    .wf-context-app-ref strong {
      display: block;
      color: #0b2345;
      font-size: 15px;
      font-weight: 650
    }

  /* Clearer task header icon; frame size is unchanged */
  .wf-task-hero-icon i,
  .wf-task-header-icon i,
  .task-icon i,
  .hero-icon i {
    font-size: 30px !important;
    font-weight: 700;
    color: #e87500;
  }

  /* Dynamic workflow outcome buttons.
     Existing Complete styling is preserved; these variants only add
     clear visual distinction for configured actions. */
  .wf-completion-actions {
    display: flex;
    justify-content: flex-end;
    gap: 10px;
    flex-wrap: wrap;
  }

  .wf-action-accept {
    background: #0b8f6a !important;
    border-color: #0b8f6a !important;
    color: #fff !important;
  }

  .wf-action-reject {
    background: #c8454d !important;
    border-color: #c8454d !important;
    color: #fff !important;
  }

  .wf-action-return {
    background: #e18424 !important;
    border-color: #e18424 !important;
    color: #fff !important;
  }

</style>

