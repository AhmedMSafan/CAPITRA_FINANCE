<template>
  <!--
    Decisions: the one list of every decision and its approval workflow; it opens on the decisions the signed-in user entered (Admin: all). A saved decision shows "Workflow not started" with a Run
    button; Run starts its SCR_DECISION_APPROVAL workflow. From then on the Workflow step column shows the step it is at and who
    holds it, and each completed step opens the decision view-only on that step.
  -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Decisions') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-gavel"></span></span>
          <div>
            <h1>{{ tr('Decisions') }}</h1>
            <p>{{ tr('Enter a decision, then Run starts its approval workflow; the workflow step shows where each one is.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openScreeningManual(5)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
        <RouterLink v-if="canEdit" :to="{ name: 'scr-decision-new' }" class="fx-btn fx-btn--primary"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New decision') }}</RouterLink>
      </div>
    </div>

    <div class="fc-indicators scr-indicators scr-indicators--4">
      <button v-for="k in KPIS" :key="k.key" type="button" class="fx-card fc-indicator" :class="{ 'fc-indicator--orange': k.orange, 'scr-kpi--on': view === k.key }" @click="view = view === k.key ? '' : k.key">
        <b>{{ count(k.key) }}</b><span>{{ tr(k.title) }}</span><small>{{ tr(k.descr) }}</small>
      </button>
    </div>

    <div class="fx-card fx-card-pad fc-filters">
      <label class="fc-field"><span>{{ tr('Search') }}</span>
        <input v-model="search" class="fx-search" :placeholder="tr('Search by decision, official number or party')">
      </label>
      <ScrUserFilter v-model="who" :me="me" :users="users" with-waiting :waiting-count="waitingCount" />
      <label class="fc-field"><span>{{ tr('Type') }}</span>
        <select v-model="type" class="fx-select">
          <option value="">{{ tr('All') }}</option>
          <option v-for="t in TYPES" :key="t" :value="t">{{ tr(LABELS[t] ?? t) }}</option>
        </select>
      </label>
      <label class="fc-field"><span>{{ tr('Workflow') }}</span>
        <select v-model="view" class="fx-select">
          <option value="">{{ tr('All') }}</option>
          <option v-for="k in KPIS" :key="k.key" :value="k.key">{{ tr(k.title) }}</option>
          <option value="CANCELLED">{{ tr('Cancelled') }}</option>
        </select>
      </label>
    </div>

    <div class="fx-card">
      <div class="fc-card-head scr-card-head">
        <h2>{{ who === MINE ? tr('Entered by me') : who === WAITING ? tr('Waiting for me') : tr('Decisions') }} ({{ rows.length }})</h2>
        <button type="button" class="fx-btn fx-btn--sm" :disabled="loading" @click="load"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Refresh') }}</button>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th class="fc-no">#</th><th>{{ tr('Decision') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('Parties') }}</th><th>{{ tr('Entered by') }}</th>
              <th>{{ tr('Workflow status') }}</th><th>{{ tr('Workflow Step') }}</th><th>{{ tr('Version') }}</th><th class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(d, i) in rows" :key="d.decisionId">
              <td class="fc-no">{{ i + 1 }}</td>
              <td><RouterLink :to="open(d)" dir="ltr">{{ d.decisionRef }}</RouterLink><span class="fx-cell-sub" dir="ltr">{{ d.officialNo }}</span>
                <span class="fx-cell-sub">{{ tr('Effective') }} {{ formatDate(d.effectiveDate) }}</span></td>
              <td><span class="fx-chip fx-chip--plain">{{ tr(LABELS[d.decisionTypeCode] ?? d.decisionTypeCode) }}</span>
                <span v-if="d.parentDecisionRef" class="fx-cell-sub" dir="ltr">{{ d.parentDecisionRef }}</span></td>
              <td>{{ d.parties.map((p) => p.nameAr).join('، ') || '-' }}</td>
              <td>{{ d.enteredBy }}<span class="fx-cell-sub">{{ formatDate(d.createdAt) }}</span></td>
              <td>
                <span v-if="viewOf(d) === 'STARTED'" class="fx-chip fx-chip--ok">{{ tr('Workflow started') }}</span>
                <span v-else-if="viewOf(d) === 'PUBLISHED'" class="fx-chip fx-chip--ok">{{ tr('Published') }}</span>
                <span v-else-if="viewOf(d) === 'CANCELLED'" class="fx-chip fx-chip--plain">{{ tr('Cancelled') }}</span>
                <span v-else-if="viewOf(d) === 'RETURNED'" class="fx-chip fx-chip--warn">{{ tr('Returned to data entry') }}</span>
                <span v-else class="fx-chip fx-chip--plain">{{ tr('Workflow not started') }}</span>
              </td>
              <td>
                <template v-if="viewOf(d) === 'STARTED'">
                  <b class="scr-step">{{ d.workflowStepName ? tr(d.workflowStepName) : '-' }}</b>
                  <span v-if="d.workflowStepHolder && isMine(d.workflowStepHolder)" class="fx-chip fx-chip--warn scr-with-me"><span class="fa fa-hand-o-right" aria-hidden="true"></span>{{ tr('Waiting for you') }}</span>
                  <span v-else-if="d.workflowStepHolder" class="fx-cell-sub"><span class="fa fa-user-o" aria-hidden="true"></span> {{ tr('With') }} {{ holder(d.workflowStepHolder) }}</span>
                </template>
                <span v-else-if="viewOf(d) === 'RETURNED' && d.returnReason" class="fx-cell-sub scr-reason" :title="d.returnReason">{{ d.returnReason }}</span>
                <span v-else-if="viewOf(d) === 'NOT_STARTED'" class="fx-cell-sub">{{ tr('Press Run to start the workflow') }}</span>
                <ScrDoneSteps :decision-id="d.decisionId" :steps="d.completedSteps" />
              </td>
              <td>{{ d.registryVersionNo ? `V${d.registryVersionNo}` : '-' }}</td>
              <td class="fx-r">
                <div class="fc-row-actions">
                  <button v-if="canEdit && (viewOf(d) === 'NOT_STARTED' || viewOf(d) === 'RETURNED')" type="button" class="fx-btn fx-btn--primary fx-btn--sm"
                          :disabled="busy === d.decisionId" @click="runWorkflow(d)">
                    <span :class="['fa', busy === d.decisionId ? 'fa-spinner fa-spin' : 'fa-play']" aria-hidden="true"></span>{{ tr('Run') }}
                  </button>
                  <RouterLink :to="open(d)" class="fx-btn fx-btn--teal fx-btn--sm"><span class="fa fa-folder-open-o" aria-hidden="true"></span>{{ tr('Open') }}</RouterLink>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
      <div v-else-if="!rows.length" class="fx-empty"><span class="fa fa-gavel" aria-hidden="true"></span>
        {{ who === MINE ? tr('You have not entered any decisions. Choose Waiting for me or All users.') : who === WAITING ? tr('Nothing is waiting for you.') : tr('No decisions.') }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { MINE, WAITING, useUserFilter } from '../utils/userFilter'
import ScrUserFilter from '../components/ScrUserFilter.vue'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate } from '../utils/dates'
import ScrDoneSteps from '../components/ScrDoneSteps.vue'
import { tr } from '@/shared/layout/language'
import { LABELS, errorText, scrApi, type ScrDecision } from '../api/scr.api'
import { openScreeningManual } from '@/modules/scr/utils/manual'

type View = '' | 'NOT_STARTED' | 'STARTED' | 'RETURNED' | 'PUBLISHED' | 'CANCELLED'
const KPIS: { key: Exclude<View, '' | 'CANCELLED'>; title: string; descr: string; orange?: boolean }[] = [
  { key: 'NOT_STARTED', title: 'Workflow not started', descr: 'Entered, waiting for Run', orange: true },
  { key: 'STARTED', title: 'Workflow started', descr: 'In review or awaiting approval' },
  { key: 'RETURNED', title: 'Returned to data entry', descr: 'Fix the decision, then run it again', orange: true },
  { key: 'PUBLISHED', title: 'Published', descr: 'On the Federation Final List' }
]
/** Role codes shown by name when a step waits for a role. */
const ROLES: Record<string, string> = { LEGAL_OFFICER: 'Legal officer', RISK_OFFICER: 'Risk officer', OPERATIONS_OFFICER: 'Operations officer' }
const TYPES = ['LISTING', 'AMENDMENT', 'SUSPENSION', 'LIFTING']

const route = useRoute()
const canEdit = hasPermission('SCR.DECISION.EDIT')
const decisions = ref<ScrDecision[]>([])
const loading = ref(false)
const busy = ref<number | null>(null)
const search = ref('')
const type = ref('')
const view = ref<View>((typeof route.query.view === 'string' ? route.query.view : '') as View)
/** Entered by me by default (Admin: all users); Waiting for me = the current step is with me or my role. */
const { who, me, users, scoped, waitingCount, isMine } = useUserFilter(decisions, (d) => d.enteredBy,
  (d) => (viewOf(d) === 'STARTED' ? d.workflowStepHolder : null))
const open = (d: ScrDecision) => ({ name: 'scr-decision', params: { decisionId: d.decisionId } })
const holder = (h: string) => tr(ROLES[h] ?? h)

function viewOf(d: ScrDecision): View {
  switch (d.statusCode) {
    case 'IN_REVIEW':
    case 'AWAITING_APPROVAL': return 'STARTED'
    case 'RETURNED': return 'RETURNED'
    case 'PUBLISHED': return 'PUBLISHED'
    case 'CANCELLED': return 'CANCELLED'
    default: return 'NOT_STARTED'
  }
}
const count = (k: View) => scoped.value.filter((d) => viewOf(d) === k).length

const rows = computed(() => {
  const q = search.value.trim().toLowerCase()
  return scoped.value.filter((d) => (!view.value || viewOf(d) === view.value) && (!type.value || d.decisionTypeCode === type.value)
    && (!q || `${d.decisionRef} ${d.officialNo} ${d.parties.map((p) => `${p.nameAr} ${p.nameEn ?? ''} ${p.identityNo}`).join(' ')}`.toLowerCase().includes(q)))
})

async function load() {
  loading.value = true
  try {
    decisions.value = await scrApi.decisions('')
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
}

async function runWorkflow(d: ScrDecision) {
  busy.value = d.decisionId
  try {
    const r = await scrApi.run(d.decisionId)
    decisions.value = decisions.value.map((x) => (x.decisionId === r.decisionId ? r : x))
    showSuccess(`${d.decisionRef}: ${tr('Workflow started')}${r.workflowStepName ? ' · ' + tr(r.workflowStepName) : ''}`)
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = null
  }
}

onMounted(load)
</script>
