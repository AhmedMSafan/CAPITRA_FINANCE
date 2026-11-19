<template>
  <!--
    Decisions awaiting approval: every entered decision not yet published or cancelled. A decision waits here as "Workflow not
    started" until Run starts its SCR_DECISION_APPROVAL workflow; then the Workflow step column shows the step it is at.
  -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Awaiting approval') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-hourglass-half"></span></span>
          <div>
            <h1>{{ tr('Decisions awaiting approval') }}</h1>
            <p>{{ tr('Run a decision to start its approval workflow; the workflow step shows where each one is.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <RouterLink v-if="canEdit" :to="{ name: 'scr-decision-new' }" class="fx-btn fx-btn--primary"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New decision') }}</RouterLink>
      </div>
    </div>

    <div class="fc-indicators scr-indicators">
      <button v-for="k in KPIS" :key="k.key" type="button" class="fx-card fc-indicator" :class="{ 'fc-indicator--orange': k.orange, 'scr-kpi--on': view === k.key }" @click="view = view === k.key ? '' : k.key">
        <b>{{ count(k.key) }}</b><span>{{ tr(k.title) }}</span><small>{{ tr(k.descr) }}</small>
      </button>
    </div>

    <div class="fx-card fx-card-pad fc-filters">
      <label class="fc-field"><span>{{ tr('Search') }}</span>
        <input v-model="search" class="fx-search" :placeholder="tr('Search by decision, official number or party')">
      </label>
      <label class="fc-field"><span>{{ tr('Workflow') }}</span>
        <select v-model="view" class="fx-select">
          <option value="">{{ tr('All') }}</option>
          <option v-for="k in KPIS" :key="k.key" :value="k.key">{{ tr(k.title) }}</option>
        </select>
      </label>
      <div class="fc-filter-actions">
        <button type="button" class="fx-btn" :disabled="loading" @click="load"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Refresh') }}</button>
      </div>
    </div>

    <div class="fx-card">
      <div class="fc-card-head"><h2>{{ tr('Decisions awaiting approval') }} ({{ rows.length }})</h2></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th class="fc-no">#</th><th>{{ tr('Decision') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('Parties') }}</th><th>{{ tr('Entered by') }}</th>
              <th>{{ tr('Workflow status') }}</th><th>{{ tr('Workflow Step') }}</th><th class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(d, i) in rows" :key="d.decisionId">
              <td class="fc-no">{{ i + 1 }}</td>
              <td><RouterLink :to="open(d)" dir="ltr">{{ d.decisionRef }}</RouterLink><span class="fx-cell-sub" dir="ltr">{{ d.officialNo }}</span></td>
              <td><span class="fx-chip fx-chip--plain">{{ tr(LABELS[d.decisionTypeCode] ?? d.decisionTypeCode) }}</span>
                <span v-if="d.parentDecisionRef" class="fx-cell-sub" dir="ltr">{{ d.parentDecisionRef }}</span></td>
              <td>{{ d.parties.map((p) => p.nameAr).join('، ') || '-' }}</td>
              <td>{{ d.enteredBy }}<span class="fx-cell-sub">{{ formatDate(d.createdAt) }}</span></td>
              <td>
                <span v-if="started(d)" class="fx-chip fx-chip--ok"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Workflow started') }}</span>
                <span v-else class="fx-chip fx-chip--plain"><span class="fa fa-minus" aria-hidden="true"></span>{{ tr('Workflow not started') }}</span>
              </td>
              <td>
                <template v-if="started(d)"><b class="scr-step">{{ d.workflowStepName ? tr(d.workflowStepName) : '-' }}</b></template>
                <template v-else-if="d.statusCode === 'RETURNED'">
                  <span class="fx-chip fx-chip--warn">{{ tr('Returned to data entry') }}</span>
                  <span v-if="d.returnReason" class="fx-cell-sub scr-reason" :title="d.returnReason">{{ d.returnReason }}</span>
                </template>
                <span v-else>-</span>
              </td>
              <td class="fx-r">
                <div class="fc-row-actions">
                  <button v-if="canEdit && !started(d)" type="button" class="fx-btn fx-btn--primary fx-btn--sm" :disabled="busy === d.decisionId" @click="runWorkflow(d)">
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
      <div v-else-if="!rows.length" class="fx-empty"><span class="fa fa-check-circle-o" aria-hidden="true"></span>{{ tr('No decisions are awaiting approval.') }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import { LABELS, errorText, scrApi, type ScrDecision } from '../api/scr.api'

type View = '' | 'NOT_STARTED' | 'STARTED' | 'RETURNED'
const KPIS: { key: Exclude<View, ''>; title: string; descr: string; orange?: boolean }[] = [
  { key: 'NOT_STARTED', title: 'Workflow not started', descr: 'Entered, waiting for Run', orange: true },
  { key: 'STARTED', title: 'Workflow started', descr: 'In review or awaiting approval' },
  { key: 'RETURNED', title: 'Returned to data entry', descr: 'Fix the decision, then run it again' }
]

const canEdit = hasPermission('SCR.DECISION.EDIT')
const decisions = ref<ScrDecision[]>([])
const loading = ref(false)
const busy = ref<number | null>(null)
const search = ref('')
const view = ref<View>('')
const open = (d: ScrDecision) => ({ name: 'scr-decision', params: { decisionId: d.decisionId } })
const started = (d: ScrDecision) => d.statusCode === 'IN_REVIEW' || d.statusCode === 'AWAITING_APPROVAL'
const viewOf = (d: ScrDecision): View => (started(d) ? 'STARTED' : d.statusCode === 'RETURNED' ? 'RETURNED' : 'NOT_STARTED')
const count = (k: View) => decisions.value.filter((d) => viewOf(d) === k).length

const rows = computed(() => {
  const q = search.value.trim().toLowerCase()
  return decisions.value.filter((d) => (!view.value || viewOf(d) === view.value)
    && (!q || `${d.decisionRef} ${d.officialNo} ${d.parties.map((p) => `${p.nameAr} ${p.nameEn ?? ''} ${p.identityNo}`).join(' ')}`.toLowerCase().includes(q)))
})

async function load() {
  loading.value = true
  try {
    decisions.value = await scrApi.decisions('PENDING')
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
    showSuccess(`${d.decisionRef}: ${tr('Workflow started')} · ${r.workflowStepName ? tr(r.workflowStepName) : ''}`)
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = null
  }
}

onMounted(load)
</script>
