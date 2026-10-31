<template>
  <!-- APEX App 150722 page 750 "Disbursement Requests": the requests over FCT_DISBURSEMENTS, newest first; Create and the row link
       open page 751. Page 750 hides started requests (IS_ACTIVE Y); here they stay listed as WF Started and open read-only. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-money"></span></span>
        <div>
          <h1>{{ tr('Disbursement Requests') }}</h1>
          <p>{{ tr('Customer Disbursement Request') }}</p>
        </div>
      </div>
      <div class="col-actions">
        <RouterLink class="fx-btn" :to="{ name: 'crm-customers' }"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Customers') }}</RouterLink>
        <RouterLink v-if="canEdit" class="fx-btn fx-btn--teal" :to="{ name: 'crm-disbursement-request-new' }">
          <span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Create') }}
        </RouterLink>
      </div>
    </div>

    <div class="fx-card">
      <div class="fc-card-head">
        <h2>{{ tr('Disbursement Requests') }} ({{ filtered.length }})</h2>
        <div class="dr-tools">
          <select v-model="status" class="fx-select" :aria-label="tr('Status')" @change="page = 1">
            <option value="">{{ tr('All statuses') }}</option>
            <option value="new">{{ tr('Not Started') }}</option>
            <option value="started">{{ tr('WF Started') }}</option>
          </select>
          <input v-model="query" class="fx-search dr-search" type="search" :placeholder="tr('Search...')" :aria-label="tr('Search')" @input="page = 1">
        </div>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th class="dr-link"><span class="fx-sr">{{ tr('Open') }}</span></th>
              <th>{{ tr('Disbursement No') }}</th>
              <th>{{ tr('Date') }}</th>
              <th>{{ tr('Client Name') }}</th>
              <th class="fx-r">{{ tr('Gross Amount') }}</th>
              <th class="fx-r">{{ tr('Net Amount') }}</th>
              <th>{{ tr('Repayment Start Date') }}</th>
              <th class="fx-r">{{ tr('Tenor Month') }}</th>
              <th>{{ tr('Reference No') }}</th>
              <th>{{ tr('Disbursement Type') }}</th>
              <th>{{ tr('Status') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in paged" :key="r.disbursementId">
              <td class="dr-link">
                <RouterLink class="fx-btn fx-btn--sm" :to="{ name: 'crm-disbursement-request', params: { disbursementId: r.disbursementId } }"
                            :title="tr(r.workflowStarted ? 'View' : 'Edit')" :aria-label="tr(r.workflowStarted ? 'View' : 'Edit') + ' ' + r.disbursementNumber">
                  <span class="fa" :class="r.workflowStarted ? 'fa-eye' : 'fa-pencil'" aria-hidden="true"></span>
                </RouterLink>
              </td>
              <td class="dr-nowrap">{{ r.disbursementNumber }}</td>
              <td class="dr-nowrap">{{ formatDate(r.disbursementDate) }}</td>
              <td>{{ r.clientName || '-' }}</td>
              <td class="fx-r">{{ formatAmount(r.grossAmount) }}</td>
              <td class="fx-r">{{ r.netAmount == null ? '' : formatAmount(r.netAmount) }}</td>
              <td class="dr-nowrap">{{ r.repaymentStartDate ? formatDate(r.repaymentStartDate) : '' }}</td>
              <td class="fx-r">{{ r.tenorMonth ?? '' }}</td>
              <td>{{ r.disbursementRef ?? '' }}</td>
              <td>{{ r.disbursementType ?? '' }}</td>
              <td><span class="fx-chip" :class="r.workflowStarted ? 'fx-chip--teal' : 'fx-chip--brand'">{{ tr(r.workflowStarted ? 'WF Started' : 'Not Started') }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
      <div v-else-if="!paged.length" class="fx-empty"><span class="fa fa-money" aria-hidden="true"></span>{{ tr('No data found') }}</div>
      <div class="fc-foot">
        <span class="fc-muted">{{ tr('Showing') }} {{ first }} {{ tr('to') }} {{ last }} {{ tr('of') }} {{ filtered.length }} {{ tr('entries') }}</span>
        <div class="fc-pager">
          <button type="button" class="fx-btn fx-btn--sm" :disabled="page === 1" :aria-label="tr('Previous')" @click="page--"><span class="fa fa-chevron-left" aria-hidden="true"></span></button>
          <button v-for="p in pages" :key="p" type="button" class="fx-btn fx-btn--sm" :class="{ 'fx-btn--teal': p === page }" @click="page = p">{{ p }}</button>
          <button type="button" class="fx-btn fx-btn--sm" :disabled="page === pageCount" :aria-label="tr('Next')" @click="page++"><span class="fa fa-chevron-right" aria-hidden="true"></span></button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '@/modules/collection/pages/collection.css'
import { computed, onMounted, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import { messageOf } from '@/modules/collection/pages/collection'
import { disbursementApi, type DisbursementRequestRow } from '../api/disbursement.api'

const PAGE_SIZE = 15

const rows = ref<DisbursementRequestRow[]>([])
const canEdit = hasPermission('CRM.EDIT')
const loading = ref(false)

// the report's search box (any column) and a status filter
const query = ref('')
const status = ref<'' | 'new' | 'started'>('')
const page = ref(1)
const filtered = computed(() => {
  const q = query.value.trim().toLowerCase()
  return rows.value.filter((r) => {
    if (status.value === 'new' && r.workflowStarted) return false
    if (status.value === 'started' && !r.workflowStarted) return false
    if (!q) return true
    return [r.disbursementNumber, r.disbursementDate, r.clientName, String(r.grossAmount), r.disbursementRef, r.disbursementType]
      .some((v) => (v ?? '').toLowerCase().includes(q))
  })
})
const pageCount = computed(() => Math.max(1, Math.ceil(filtered.value.length / PAGE_SIZE)))
const pages = computed(() => Array.from({ length: pageCount.value }, (_, i) => i + 1))
const paged = computed(() => filtered.value.slice((page.value - 1) * PAGE_SIZE, page.value * PAGE_SIZE))
const first = computed(() => (filtered.value.length ? (page.value - 1) * PAGE_SIZE + 1 : 0))
const last = computed(() => Math.min(page.value * PAGE_SIZE, filtered.value.length))

onMounted(async () => {
  loading.value = true
  try {
    rows.value = await disbursementApi.list()
  } catch (err) {
    showError(messageOf(err))
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.dr-tools { display: flex; flex-wrap: wrap; gap: 8px; }
.dr-tools .fx-select { width: auto; min-width: 150px; }
.dr-search { width: min(260px, 100%); }
.dr-link { width: 44px; }
.dr-nowrap { white-space: nowrap; }
.fx-sr { position: absolute; width: 1px; height: 1px; overflow: hidden; clip: rect(0 0 0 0); }
</style>
