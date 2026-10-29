<template>
  <!-- FINOVA_FINANCEING FactoringAgreementAssignSalesPage: the Operation Received step. It shows the agreement and lets the
       officer send the next task (Sales Review) to the workflow role, or to one user for this instance only. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-pencil-square-o"></span></span>
        <div>
          <span class="fc-eyebrow">{{ tr('WORKFLOW TASK') }}</span>
          <h1>{{ currentTaskName }}</h1>
          <p>{{ tr('Review the agreement and confirm how the next workflow task should be assigned.') }}</p>
        </div>
      </div>
      <div class="fx-actions">
        <FctAgreementDocuments :agreement-id="Number(agreementId)" />
        <button type="button" class="fx-btn" @click="backToTask"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Task') }}</button>
      </div>
    </div>

    <div v-if="error" class="fc-alert" role="alert"><span class="fa fa-exclamation-circle" aria-hidden="true"></span>{{ error }}</div>
    <div v-if="loading" class="fx-card fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading assignment workbench...') }}</div>

    <template v-else>
      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-file-text-o"></span></span>
          <div><h2>{{ tr('Agreement Summary') }}</h2></div>
        </header>
        <div class="fc-facts">
          <div class="fc-fact"><span class="fa fa-bookmark-o" aria-hidden="true"></span><div><span>{{ tr('Agreement Number') }}</span><b>{{ agreementNumber }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-hashtag" aria-hidden="true"></span><div><span>{{ tr('Agreement ID') }}</span><b>{{ agreementId }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-user-o" aria-hidden="true"></span><div><span>{{ tr('Client') }}</span><b>{{ clientName }}</b>
            <small v-if="clientId != null">{{ tr('Client ID') }} {{ clientId }}</small></div></div>
          <div class="fc-fact"><span class="fa fa-money" aria-hidden="true"></span><div><span>{{ tr('Agreement Amount') }}</span><b>{{ formatAmount(agreementAmount) }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-calendar" aria-hidden="true"></span><div><span>{{ tr('Application Date') }}</span><b>{{ formatDate(agreement?.applicationDate) }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-calendar-check-o" aria-hidden="true"></span><div><span>{{ tr('Effective Date') }}</span><b>{{ formatDate(agreement?.effectiveDate) }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-check-circle-o" aria-hidden="true"></span><div><span>{{ tr('Status') }}</span><b>{{ agreement?.statusCode || '-' }}</b></div></div>
        </div>
      </section>

      <section class="fx-card fc-step-card">
        <header class="fc-sec-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-share-alt"></span></span>
          <div><h2>{{ tr('Next Task Assignment') }}</h2></div>
        </header>
        <div class="fc-routing">
          <div class="fc-routing-default">
            <div><span>{{ tr('Next Task') }}</span><b>{{ tr(nextTaskName) }}</b></div>
            <div><span>{{ tr('Workflow Default') }}</span><span class="fx-chip fx-chip--teal"><span class="fa fa-users" aria-hidden="true"></span>{{ nextDefaultRole }}</span></div>
          </div>
          <label class="fc-check">
            <input v-model="assignSpecificUser" type="checkbox">
            <span>{{ tr('Assign the next task to a specific user for this workflow instance only') }}</span>
          </label>
          <div v-if="assignSpecificUser" class="fc-routing-user">
            <label class="fc-field"><span>{{ tr('Assigned User') }}</span>
              <select v-model="selectedUserId" class="fx-select">
                <option value="">{{ tr('Select user') }}</option>
                <!-- the task goes to the user name, so the inbox shows who holds it -->
                <option v-for="u in users" :key="u.id" :value="u.userName || u.id">{{ userLabel(u) }}</option>
              </select>
            </label>
            <div v-if="selectedUser" class="fc-note fc-note--warn">
              <span class="fa fa-info-circle" aria-hidden="true"></span>
              <span>{{ tr('The next task will be assigned directly to') }} <b>{{ userLabel(selectedUser) }}</b>
                {{ tr('instead of the workflow default role') }} <b>{{ nextDefaultRole }}</b>. {{ tr('This affects only this workflow instance.') }}</span>
            </div>
          </div>
          <div v-else class="fc-note">
            <span class="fa fa-check-circle" aria-hidden="true"></span>
            <span>{{ tr('No instance override selected. The next task will use the workflow definition:') }} <b>{{ nextDefaultRole }}</b>.</span>
          </div>
        </div>
      </section>

      <section class="fx-card fc-complete">
        <label class="fc-field"><span>{{ tr('Completion Comment') }}</span>
          <textarea v-model="completionComment" class="fx-search fc-textarea" :placeholder="tr('Optional completion comment')"></textarea>
        </label>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="completing || !task" @click="completeCurrentTask">
          <span class="fa" :class="completing ? 'fa-spinner fa-spin' : 'fa-check-circle'" aria-hidden="true"></span>{{ tr(completing ? 'Completing...' : 'Complete Task') }}
        </button>
      </section>
    </template>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import './factoring.css'
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { dateLocale, tr } from '@/shared/layout/language'
import { DISPLAY_TIME_ZONE } from '@/shared/format/datetime'
import { workflowService } from '@/modules/wfe/services/workflowService'
import type { WorkflowInboxTask } from '@/modules/wfe/models/workflow'
import FctAgreementDocuments from '../components/FctAgreementDocuments.vue'
import { fctReviewApi, type FctAgreementReviewSummary, type FctUser } from '../api/fct.api'

const props = defineProps<{ agreementId: string }>()
const route = useRoute()
const router = useRouter()

const inboxId = computed(() => Number(route.query.inboxId))

const task = ref<WorkflowInboxTask | null>(null)
const agreement = ref<FctAgreementReviewSummary | null>(null)
const users = ref<FctUser[]>([])
const loading = ref(false)
const completing = ref(false)
const error = ref('')
const completionComment = ref('')
const assignSpecificUser = ref(false)
const selectedUserId = ref('')

// as in FINOVA: the step after Operation Received, and its role in FACTORING_LIMIT_APPROVAL
const nextTaskName = 'Sales Review'
const nextDefaultRole = 'SALES_OFFICER'

const currentTaskName = computed(() => task.value?.stepName?.trim() || tr('Workflow Task'))
const agreementNumber = computed(() => agreement.value?.agreementNumber || task.value?.entityRef || `Agreement #${props.agreementId}`)
const clientId = computed(() => agreement.value?.clientId ?? task.value?.clientId ?? null)
const clientName = computed(() => agreement.value?.clientName || task.value?.clientName || '-')
const agreementAmount = computed(() => agreement.value?.approvedLimit ?? agreement.value?.facilityLimit ?? task.value?.amount ?? null)
const selectedUser = computed(() => users.value.find((x) => (x.userName || x.id) === selectedUserId.value) ?? null)

function formatAmount(value?: number | null) {
  if (value == null) return '-'
  const currency = agreement.value?.currencyCode || task.value?.currencyCode || 'EGP'
  try {
    return new Intl.NumberFormat('en-US', { style: 'currency', currency, maximumFractionDigits: 2 }).format(value)
  } catch {
    return `${currency} ${value.toLocaleString()}`
  }
}

const formatDate = (v?: string | null) =>
  v ? new Intl.DateTimeFormat(dateLocale(), { year: 'numeric', month: 'short', day: '2-digit', timeZone: DISPLAY_TIME_ZONE }).format(new Date(v)) : '-'

function userLabel(user: FctUser) {
  const name = user.fullName?.trim() || user.userName?.trim() || 'User'
  const employeeNo = user.employeeNo?.trim()
  return employeeNo ? `${name} (${employeeNo})` : name
}

const errorText = (e: unknown, fallback: string) => (e instanceof Error ? e.message : tr(fallback))

async function loadPage() {
  loading.value = true
  error.value = ''
  try {
    const jobs: Promise<unknown>[] = []
    if (Number.isFinite(inboxId.value)) jobs.push(workflowService.getTask(inboxId.value).then((v) => { task.value = v }))
    jobs.push(fctReviewApi.reviewSummary(Number(props.agreementId)).then((v) => { agreement.value = v }))
    jobs.push(fctReviewApi.users().then((v) => { users.value = [...v].sort((a, b) => userLabel(a).localeCompare(userLabel(b))) }))
    await Promise.all(jobs)
  } catch (e) {
    error.value = errorText(e, 'Unable to load agreement review.')
  } finally {
    loading.value = false
  }
}

async function completeCurrentTask() {
  if (!task.value) {
    error.value = tr('Current workflow task was not supplied to this page.')
    return
  }
  if (assignSpecificUser.value && !selectedUserId.value) {
    error.value = tr('Select a user or turn off Assign to a specific user.')
    return
  }
  completing.value = true
  error.value = ''
  try {
    await workflowService.complete(task.value.inboxId, {
      comment: completionComment.value || null,
      routingMode: assignSpecificUser.value ? 'USER' : 'DEFAULT',
      assignedUserId: assignSpecificUser.value ? selectedUserId.value : null
    })
    await router.push({ name: 'workflow-inbox' })
  } catch (e) {
    error.value = errorText(e, 'Unable to complete workflow task.')
  } finally {
    completing.value = false
  }
}

function backToTask() {
  if (Number.isFinite(inboxId.value)) {
    router.push({ name: 'workflow-task', params: { inboxId: inboxId.value } })
    return
  }
  router.back()
}

onMounted(loadPage)
</script>
