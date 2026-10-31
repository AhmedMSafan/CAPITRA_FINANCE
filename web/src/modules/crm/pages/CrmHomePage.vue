<template>
  <!-- FINOVA_FINANCEING CrmHomePage "Customers": the same filters, sortable columns and paging, in this app's fx- theme.
       Workspaces opens the customer's workspaces; New Customer and Edit open the client dialog. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-users"></span></span>
        <div>
          <h1>{{ tr('Customers') }}</h1>
          <p>{{ tr('View and manage customer information.') }}</p>
        </div>
      </div>
      <button v-if="canEdit" type="button" class="fx-btn fx-btn--teal" @click="openClient(null)">
        <span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New Customer') }}
      </button>
    </div>

    <!-- the customers with the latest transactions, one click from their workspaces (Ahmed, 31 Oct 2026) -->
    <section v-if="recent.length" class="fx-card crm-recent" :aria-label="tr('Recent Customers')">
      <div class="crm-recent-head">
        <h2><span class="fa fa-clock-o" aria-hidden="true"></span>{{ tr('Recent Customers') }}</h2>
        <small>· {{ tr('latest application, agreement or disbursement') }}</small>
      </div>
      <div class="crm-recent-grid">
        <RouterLink v-for="c in recent" :key="c.clientId" :to="workspace(c.clientId)" class="crm-recent-card" :title="tr('Workspaces')">
          <span class="crm-recent-ic" aria-hidden="true"><span class="fa" :class="ACTIVITY_ICON[c.lastActivityType!]"></span></span>
          <span class="crm-recent-copy">
            <span class="crm-recent-no">{{ c.clientNumber }}</span>
            <b>{{ c.clientName }}</b>
            <small>{{ tr(c.lastActivityType!) }} {{ c.lastActivityRef }} · {{ formatDate(c.lastActivityAt) }}</small>
          </span>
          <span class="fa fa-arrow-right" aria-hidden="true"></span>
        </RouterLink>
      </div>
    </section>

    <div class="fx-card fx-card-pad fc-filters">
      <label class="fc-field">
        <span>{{ tr('Customer Name') }}</span>
        <input v-model="customerName" class="fx-search" type="text" :placeholder="tr('Enter customer name...')" @keyup.enter="search">
      </label>
      <label class="fc-field">
        <span>{{ tr('Customer Number') }}</span>
        <input v-model="customerNumber" class="fx-search" type="text" :placeholder="tr('Enter customer number...')" @keyup.enter="search">
      </label>
      <label class="fc-field">
        <span>{{ tr('Client Type') }}</span>
        <select v-model="clientType" class="fx-select">
          <option v-for="t in clientTypes" :key="t" :value="t">{{ t === ALL ? tr(ALL) : t }}</option>
        </select>
      </label>
      <div class="fc-filter-actions">
        <button type="button" class="fx-btn fx-btn--teal" @click="search"><span class="fa fa-search" aria-hidden="true"></span>{{ tr('Search') }}</button>
        <button type="button" class="fx-btn" @click="clearFilters"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Clear') }}</button>
      </div>
    </div>

    <div class="fx-card">
      <div class="fc-card-head">
        <h2>{{ tr('Customers') }} ({{ filtered.length }})</h2>
        <span class="fc-muted">{{ tr('Show') }} {{ PAGE_SIZE }} {{ tr('entries per page') }}</span>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th v-for="col in COLUMNS" :key="col.field" :aria-sort="ariaSort(col.field)">
                <button type="button" class="crm-sort" :class="{ 'is-on': sortField === col.field }" @click="changeSort(col.field)">
                  {{ tr(col.label) }}<span class="fa" :class="sortIcon(col.field)" aria-hidden="true"></span>
                </button>
              </th>
              <th class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in paged" :key="c.clientId">
              <td class="fc-muted">{{ c.clientId }}</td>
              <td><RouterLink :to="workspace(c.clientId)">{{ c.clientNumber }}</RouterLink></td>
              <td>{{ c.clientName }}</td>
              <td><span class="fx-chip fx-chip--plain">{{ c.clientType || '-' }}</span></td>
              <td>{{ c.phoneNumber || '-' }}</td>
              <td>{{ c.email || '-' }}</td>
              <td><span class="fx-chip" :class="c.isActive ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(c.isActive ? 'Active' : 'Inactive') }}</span></td>
              <td>
                <span v-if="c.lastActivityAt" class="crm-last"><b>{{ formatDate(c.lastActivityAt) }}</b><small>{{ tr(c.lastActivityType!) }} {{ c.lastActivityRef }}</small></span>
                <span v-else class="fc-muted">-</span>
              </td>
              <td class="fx-r">
                <RouterLink class="fx-btn fx-btn--teal fx-btn--sm" :to="workspace(c.clientId)" :title="tr('Workspaces')">
                  <span class="fa fa-th-large" aria-hidden="true"></span>{{ tr('Workspaces') }}
                </RouterLink>
                <button v-if="canEdit" type="button" class="fx-btn fx-btn--sm crm-edit" :title="tr('Edit')" @click="openClient(c.clientId)">
                  <span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading customers...') }}</div>
      <div v-else-if="!paged.length" class="fx-empty"><span class="fa fa-users" aria-hidden="true"></span>{{ tr('No customers match the current filters.') }}</div>
      <div class="fc-foot">
        <span class="fc-muted">{{ tr('Showing') }} {{ from }} {{ tr('to') }} {{ to }} {{ tr('of') }} {{ filtered.length }} {{ tr('entries') }}</span>
        <div class="fc-pager">
          <button type="button" class="fx-btn fx-btn--sm" :disabled="page === 1" :aria-label="tr('Previous')" @click="page--"><span class="fa fa-chevron-left" aria-hidden="true"></span></button>
          <button v-for="p in pages" :key="p" type="button" class="fx-btn fx-btn--sm" :class="{ 'fx-btn--teal': p === page }" @click="page = p">{{ p }}</button>
          <button type="button" class="fx-btn fx-btn--sm" :disabled="page === pageCount" :aria-label="tr('Next')" @click="page++"><span class="fa fa-chevron-right" aria-hidden="true"></span></button>
        </div>
      </div>
    </div>

    <CrmClientDialog v-model:visible="dialogOpen" :client-id="dialogClientId" @saved="load" />
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, ref } from 'vue'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import { formatDate } from '@/shared/format/datetime'
import { crmApi, type CrmClientRow } from '../api/crm.api'
import CrmClientDialog from '../components/CrmClientDialog.vue'

type SortField = 'clientId' | 'clientNumber' | 'clientName' | 'clientType' | 'phoneNumber' | 'email' | 'isActive' | 'lastActivityAt'

const ALL = 'All Types'
const PAGE_SIZE = 10
// FINOVA's columns, each sortable, and Last Activity: the list opens newest activity first
const COLUMNS: { field: SortField; label: string }[] = [
  { field: 'clientId', label: 'Client ID' },
  { field: 'clientNumber', label: 'Customer Number' },
  { field: 'clientName', label: 'Customer Name' },
  { field: 'clientType', label: 'Client Type' },
  { field: 'phoneNumber', label: 'Phone' },
  { field: 'email', label: 'Email' },
  { field: 'isActive', label: 'Status' },
  { field: 'lastActivityAt', label: 'Last Activity' }
]
const RECENT_COUNT = 8
const ACTIVITY_ICON = { Application: 'fa-file-text-o', Agreement: 'fa-handshake-o', Disbursement: 'fa-money' } as const

const customers = ref<CrmClientRow[]>([])
const canEdit = hasPermission('CRM.EDIT')
const dialogOpen = ref(false)
const dialogClientId = ref<number | null>(null)
const loading = ref(false)
const error = ref('')
useFeedback({ errors: [error] })

const customerName = ref('')
const customerNumber = ref('')
const clientType = ref(ALL)
// the filters apply when Search is pressed (or Enter), as on the FINOVA page
const applied = ref({ name: '', number: '', type: ALL })
const page = ref(1)
const sortField = ref<SortField>('lastActivityAt')
const sortAsc = ref(false)

const clientTypes = computed(() => [ALL, ...[...new Set(customers.value.map((c) => c.clientType).filter(Boolean))].sort()])

// FINOVA sorts the id as a number, the rest as text (numeric-aware, case-insensitive); Status sorts Active before Inactive
const sortValue = (c: CrmClientRow, f: SortField) => (f === 'isActive' ? (c.isActive ? 'Active' : 'Inactive') : String(c[f] ?? '').trim())
const filtered = computed(() => {
  const name = applied.value.name.trim().toLowerCase()
  const number = applied.value.number.trim().toLowerCase()
  const f = sortField.value
  const dir = sortAsc.value ? 1 : -1
  return customers.value
    .filter((c) =>
      (!name || c.clientName.toLowerCase().includes(name)) &&
      (!number || c.clientNumber.toLowerCase().includes(number)) &&
      (applied.value.type === ALL || c.clientType === applied.value.type))
    .sort((a, b) => f === 'lastActivityAt' ? byActivity(a, b, dir) : dir * (f === 'clientId'
      ? a.clientId - b.clientId
      : sortValue(a, f).localeCompare(sortValue(b, f), undefined, { numeric: true, sensitivity: 'base' })))
})
// by last activity time; customers without any stay last either way, then by Client ID
const activityTime = (c: CrmClientRow) => (c.lastActivityAt ? Date.parse(c.lastActivityAt) : null)
function byActivity(a: CrmClientRow, b: CrmClientRow, dir: number) {
  const x = activityTime(a)
  const y = activityTime(b)
  if (x == null || y == null) return x == null && y == null ? a.clientId - b.clientId : x == null ? 1 : -1
  return dir * (x - y) || a.clientId - b.clientId
}
const recent = computed(() => customers.value.filter((c) => c.lastActivityAt).sort((a, b) => byActivity(a, b, -1)).slice(0, RECENT_COUNT))
const pageCount = computed(() => Math.max(1, Math.ceil(filtered.value.length / PAGE_SIZE)))
const pages = computed(() => Array.from({ length: pageCount.value }, (_, i) => i + 1))
const paged = computed(() => filtered.value.slice((page.value - 1) * PAGE_SIZE, page.value * PAGE_SIZE))
const from = computed(() => (filtered.value.length ? (page.value - 1) * PAGE_SIZE + 1 : 0))
const to = computed(() => Math.min(page.value * PAGE_SIZE, filtered.value.length))

// Workspaces opens the Customer Workspace, which leads to each business line's workspace
const workspace = (clientId: number) => ({ name: 'crm-customer-workspace', params: { clientId } })

function changeSort(f: SortField) {
  // Last Activity starts newest first, the other columns ascending
  sortAsc.value = sortField.value === f ? !sortAsc.value : f !== 'lastActivityAt'
  sortField.value = f
  page.value = 1
}
const sortIcon = (f: SortField) => (sortField.value !== f ? 'fa-sort' : sortAsc.value ? 'fa-sort-asc' : 'fa-sort-desc')
const ariaSort = (f: SortField) => (sortField.value !== f ? undefined : sortAsc.value ? 'ascending' : 'descending')

function openClient(clientId: number | null) {
  dialogClientId.value = clientId
  dialogOpen.value = true
}

function search() {
  applied.value = { name: customerName.value, number: customerNumber.value, type: clientType.value }
  page.value = 1
}

function clearFilters() {
  customerName.value = ''
  customerNumber.value = ''
  clientType.value = ALL
  search()
}

async function load() {
  loading.value = true
  try {
    customers.value = await crmApi.clients()
  } catch (e) {
    error.value = e instanceof Error ? e.message : String(e)
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.crm-recent { margin-bottom: 14px; padding: 14px 16px; }
.crm-recent-head { display: flex; align-items: baseline; gap: 8px; margin-bottom: 12px; }
.crm-recent-head h2 { display: flex; align-items: center; gap: 8px; margin: 0; font-size: 15px; color: var(--fin-teal-700); }
.crm-recent-head h2 .fa { color: var(--fin-orange); }
.crm-recent-head small { font-size: 12px; color: var(--fin-muted); }
.crm-recent-grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 10px; }
.crm-recent-card { display: flex; align-items: center; gap: 10px; padding: 10px 12px; border: 1px solid var(--fin-line); border-radius: 12px; background: #fff; color: var(--fin-ink); text-decoration: none; }
.crm-recent-card:hover { border-color: var(--fin-teal-600); }
.crm-recent-card > .fa { color: var(--fin-teal-600); }
.crm-recent-ic { flex: none; display: grid; place-items: center; width: 36px; height: 36px; border-radius: 10px; background: var(--fin-teal-50); color: var(--fin-teal-600); }
.crm-recent-copy { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 1px; }
.crm-recent-copy b, .crm-recent-copy small { overflow: hidden; white-space: nowrap; text-overflow: ellipsis; }
.crm-recent-copy b { font-size: 14px; }
.crm-recent-copy small { font-size: 11.5px; color: var(--fin-muted); }
.crm-recent-no { font-size: 11px; font-weight: 700; color: var(--fin-orange-700); }
.crm-last { display: flex; flex-direction: column; gap: 1px; line-height: 1.25; }
.crm-last small { font-size: 11.5px; color: var(--fin-muted); }
@media (max-width: 1100px) { .crm-recent-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
@media (max-width: 640px) { .crm-recent-grid { grid-template-columns: 1fr; } }
.crm-sort { display: inline-flex; align-items: center; gap: 6px; padding: 0; border: 0; background: none; font: inherit; color: inherit; text-transform: inherit; letter-spacing: inherit; cursor: pointer; white-space: nowrap; }
.crm-sort .fa { font-size: 11px; opacity: .45; }
.crm-edit { margin-inline-start: 6px; }
.crm-sort.is-on, .crm-edit { margin-inline-start: 6px; }
.crm-sort.is-on .fa { color: var(--fin-teal-600); opacity: 1; }
</style>
