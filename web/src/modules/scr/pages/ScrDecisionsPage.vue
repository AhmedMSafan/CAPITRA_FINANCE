<template>
  <!-- Decisions: entered from letters, reviewed and approved through the SCR_DECISION_APPROVAL workflow. -->
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
            <p>{{ tr('Enter decisions from letters and send them through review and approval.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <RouterLink v-if="canEdit" :to="{ name: 'scr-decision-new' }" class="fx-btn fx-btn--primary"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New decision') }}</RouterLink>
      </div>
    </div>

    <div class="fx-card fx-card-pad fc-filters">
      <label class="fc-field"><span>{{ tr('Search') }}</span>
        <input v-model="search" class="fx-search" :placeholder="tr('Search by decision, official number or party')">
      </label>
      <label class="fc-field"><span>{{ tr('Type') }}</span>
        <select v-model="type" class="fx-select">
          <option value="">{{ tr('All') }}</option>
          <option v-for="t in TYPES" :key="t" :value="t">{{ tr(LABELS[t] ?? t) }}</option>
        </select>
      </label>
      <label class="fc-field"><span>{{ tr('Status') }}</span>
        <select v-model="status" class="fx-select" @change="load">
          <option value="">{{ tr('All statuses') }}</option>
          <option v-for="s in STATUSES" :key="s" :value="s">{{ tr(LABELS[s] ?? s) }}</option>
        </select>
      </label>
    </div>

    <div class="fx-card">
      <div class="fc-card-head"><h2>{{ tr('Decisions') }} ({{ rows.length }})</h2></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th class="fc-no">#</th><th>{{ tr('Decision') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('Parties') }}</th><th>{{ tr('Effective date') }}</th>
              <th>{{ tr('Status') }}</th><th>{{ tr('Entered by') }}</th><th>{{ tr('Version') }}</th><th class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(d, i) in rows" :key="d.decisionId">
              <td class="fc-no">{{ i + 1 }}</td>
              <td><RouterLink :to="open(d)" dir="ltr">{{ d.decisionRef }}</RouterLink><span class="fx-cell-sub" dir="ltr">{{ d.officialNo }}</span></td>
              <td><span class="fx-chip fx-chip--plain">{{ tr(LABELS[d.decisionTypeCode] ?? d.decisionTypeCode) }}</span>
                <span v-if="d.parentDecisionRef" class="fx-cell-sub" dir="ltr">{{ d.parentDecisionRef }}</span></td>
              <td>{{ d.parties.map((p) => p.nameAr).join('، ') }}</td>
              <td>{{ formatDate(d.effectiveDate) }}</td>
              <td><span class="fx-chip" :class="CHIP[d.statusCode]">{{ tr(LABELS[d.statusCode] ?? d.statusCode) }}</span></td>
              <td>{{ d.enteredBy }}<span class="fx-cell-sub">{{ formatDate(d.createdAt) }}</span></td>
              <td>{{ d.registryVersionNo ? `V${d.registryVersionNo}` : '-' }}</td>
              <td class="fx-r"><RouterLink :to="open(d)" class="fx-btn fx-btn--teal fx-btn--sm"><span class="fa fa-folder-open-o" aria-hidden="true"></span>{{ tr('Open') }}</RouterLink></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
      <div v-else-if="!rows.length" class="fx-empty"><span class="fa fa-gavel" aria-hidden="true"></span>{{ tr('No decisions.') }}</div>
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
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import { CHIP, LABELS, errorText, scrApi, type ScrDecision } from '../api/scr.api'

const TYPES = ['LISTING', 'AMENDMENT', 'SUSPENSION', 'LIFTING']
const STATUSES = ['DRAFT', 'IN_REVIEW', 'AWAITING_APPROVAL', 'RETURNED', 'PUBLISHED', 'CANCELLED']
const canEdit = hasPermission('SCR.DECISION.EDIT')
const decisions = ref<ScrDecision[]>([])
const loading = ref(false)
const search = ref('')
const type = ref('')
const status = ref('')
const open = (d: ScrDecision) => ({ name: 'scr-decision', params: { decisionId: d.decisionId } })
const rows = computed(() => {
  const q = search.value.trim().toLowerCase()
  return decisions.value.filter((d) => (!type.value || d.decisionTypeCode === type.value)
    && (!q || `${d.decisionRef} ${d.officialNo} ${d.parties.map((p) => `${p.nameAr} ${p.nameEn ?? ''} ${p.identityNo}`).join(' ')}`.toLowerCase().includes(q)))
})

async function load() {
  loading.value = true
  try {
    decisions.value = await scrApi.decisions(status.value)
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>
