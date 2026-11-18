<template>
  <!-- Decisions: entered from letters, reviewed and approved through the SCR_DECISION_APPROVAL workflow. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Decisions') }}</b>
        </nav>
        <h1>{{ tr('Decisions') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink v-if="canEdit" :to="{ name: 'scr-decision-new' }" class="fx-btn fx-btn--teal"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New decision') }}</RouterLink>
      </div>
    </div>
    <section class="fx-card">
      <div class="scr-filter">
        <input v-model="search" class="fx-search" :placeholder="tr('Search by decision, official number or party')">
        <select v-model="status" class="fx-select" @change="load">
          <option value="">{{ tr('All statuses') }}</option>
          <option v-for="s in STATUSES" :key="s" :value="s">{{ tr(LABELS[s] ?? s) }}</option>
        </select>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Decision') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('Parties') }}</th><th>{{ tr('Effective date') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Entered by') }}</th><th>{{ tr('Version') }}</th></tr></thead>
          <tbody>
            <tr v-for="d in rows" :key="d.decisionId" class="scr-row" @click="router.push({ name: 'scr-decision', params: { decisionId: d.decisionId } })">
              <td dir="ltr"><b>{{ d.decisionRef }}</b><br><small class="scr-muted">{{ d.officialNo }}</small></td>
              <td>{{ tr(LABELS[d.decisionTypeCode] ?? d.decisionTypeCode) }}<small v-if="d.parentDecisionRef" class="scr-muted" dir="ltr"> · {{ d.parentDecisionRef }}</small></td>
              <td>{{ d.parties.map((p) => p.nameAr).join('، ') }}</td>
              <td>{{ formatDate(d.effectiveDate) }}</td>
              <td><span class="fx-chip" :class="CHIP[d.statusCode]">{{ tr(LABELS[d.statusCode] ?? d.statusCode) }}</span></td>
              <td>{{ d.enteredBy }}</td>
              <td>{{ d.registryVersionNo ? `V${d.registryVersionNo}` : '' }}</td>
            </tr>
          </tbody>
        </table>
        <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
        <div v-else-if="!rows.length" class="fx-empty">{{ tr('No decisions.') }}</div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { CHIP, LABELS, errorText, scrApi, type ScrDecision } from '../api/scr.api'

const STATUSES = ['DRAFT', 'IN_REVIEW', 'AWAITING_APPROVAL', 'RETURNED', 'PUBLISHED', 'CANCELLED']
const router = useRouter()
const canEdit = hasPermission('SCR.DECISION.EDIT')
const decisions = ref<ScrDecision[]>([])
const loading = ref(false)
const search = ref('')
const status = ref('')
const rows = computed(() => {
  const q = search.value.trim().toLowerCase()
  return decisions.value.filter((d) => !q || `${d.decisionRef} ${d.officialNo} ${d.parties.map((p) => `${p.nameAr} ${p.nameEn ?? ''} ${p.identityNo}`).join(' ')}`.toLowerCase().includes(q))
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
