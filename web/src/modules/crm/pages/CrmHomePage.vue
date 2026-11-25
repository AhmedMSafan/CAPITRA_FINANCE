<template>
  <!-- FINOVA_FINANCEING CrmHomePage "Customers": the same filters, sortable columns and paging, in this app's fx- theme.
       Workspaces opens the customer's workspaces; New Customer and Edit open the client dialog. -->
  <div class="fx fc">
    <!-- one line (Ahmed, 25 Nov 2026): the title, the customers this user opened last in Customer 360, and New Customer -->
    <div class="fx-head crm-head">
      <div class="fc-title crm-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-users"></span></span>
        <h1>{{ tr('Customers') }}</h1>
      </div>
      <nav v-if="recent.length" class="crm-recent" :aria-label="tr('Recently opened')">
        <span class="crm-recent-lb"><span class="fa fa-clock-o" aria-hidden="true"></span>{{ tr('Recently opened') }}:</span>
        <RouterLink v-for="c in recent" :key="c.clientId" :to="workspace(c.clientId)" class="crm-recent-chip" :title="`${c.clientName} · ${tr('Customer 360')}`">
          <b dir="ltr">{{ c.clientNumber }}</b><span>{{ c.clientName }}</span>
        </RouterLink>
      </nav>
      <button v-if="canEdit" type="button" class="fx-btn fx-btn--teal" @click="openClient(null)">
        <span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New Customer') }}
      </button>
    </div>

    <!-- the search on one short line: labels beside their boxes -->
    <div class="fx-card crm-filters">
      <label class="crm-f">
        <span>{{ tr('Name') }}</span>
        <input v-model="customerName" class="fx-search" type="text" :placeholder="tr('Customer name...')" @keyup.enter="search">
      </label>
      <label class="crm-f">
        <span>{{ tr('Number') }}</span>
        <input v-model="customerNumber" class="fx-search" type="text" :placeholder="tr('Customer number...')" @keyup.enter="search">
      </label>
      <label class="crm-f crm-f--type">
        <span>{{ tr('Type') }}</span>
        <select v-model="clientType" class="fx-select">
          <option v-for="t in clientTypes" :key="t" :value="t">{{ t === ALL ? tr(ALL) : t }}</option>
        </select>
      </label>
      <div class="crm-f-actions">
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
              <td><RouterLink class="crm-num" :to="workspace(c.clientId)">{{ c.clientNumber }}</RouterLink></td>
              <td>{{ c.clientName }}</td>
              <td><span class="fx-chip fx-chip--plain">{{ c.clientType || '-' }}</span></td>
              <td>{{ c.phoneNumber || '-' }}</td>
              <td>{{ c.email || '-' }}</td>
              <td><span class="crm-status"><span class="crm-dot" :class="{ 'crm-dot--off': !c.isActive }" aria-hidden="true"></span>{{ tr(c.isActive ? 'Active' : 'Inactive') }}</span></td>
              <td>
                <span v-if="c.lastActivityAt" class="crm-last"><b>{{ formatDate(c.lastActivityAt) }}</b><small>{{ tr(c.lastActivityType!) }} {{ c.lastActivityRef }}</small></span>
                <span v-else class="fc-muted">-</span>
              </td>
              <td class="fx-r crm-acts">
                <!-- quiet buttons (Ahmed, 25 Nov 2026): neutral, teal only on hover; 360 opens Customer 360, the pencil edits the customer -->
                <RouterLink class="crm-act" :to="workspace(c.clientId)" :title="tr('Customer 360')" :aria-label="tr('Customer 360')"><svg class="crm-ic" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><circle cx="8" cy="8" r="6"/><path d="M8 2c2 2 2 10 0 12M8 2c-2 2-2 10 0 12M2 8h12"/></svg>360</RouterLink>
                <button v-if="canEdit" type="button" class="crm-act crm-act--icon" :title="tr('Edit')" :aria-label="tr('Edit')" @click="openClient(c.clientId)">
                  <span class="fa fa-pencil" aria-hidden="true"></span>
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
import { crmApi, type CrmClientRow, type CrmRecentClient } from '../api/crm.api'
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

const customers = ref<CrmClientRow[]>([])
// the customers this user opened last in Customer 360 (kept on the server)
const recent = ref<CrmRecentClient[]>([])
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
    const [list, opened] = await Promise.all([crmApi.clients(), crmApi.recent().catch(() => [] as CrmRecentClient[])])
    customers.value = list
    recent.value = opened
  } catch (e) {
    error.value = e instanceof Error ? e.message : String(e)
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.crm-head { gap: 12px; flex-wrap: wrap; }
.crm-title { align-items: center; }
.crm-title h1 { margin: 0; }
.crm-recent { flex: 1; min-width: 0; display: flex; align-items: center; gap: 6px; overflow: hidden; }
.crm-recent-lb { display: inline-flex; align-items: center; gap: 5px; flex: none; font-size: 12px; color: var(--fin-muted); white-space: nowrap; }
.crm-recent-chip { flex: none; display: inline-flex; align-items: center; gap: 6px; height: 28px; max-width: 220px; padding: 0 10px; border: 1px solid var(--fin-line);
  border-radius: 999px; background: #fff; color: var(--fin-ink); font-size: 12.5px; text-decoration: none; }
.crm-recent-chip span { overflow: hidden; white-space: nowrap; text-overflow: ellipsis; }
.crm-recent-chip b { flex: none; font-size: 11px; color: var(--fin-orange-700, #b5651d); }
.crm-recent-chip:hover { border-color: var(--fin-teal-600); }
.crm-filters { display: flex; align-items: center; gap: 16px; margin-bottom: 14px; padding: 10px 16px; }
.crm-f { flex: 1; min-width: 0; display: flex; align-items: center; gap: 8px; }
.crm-f > span { flex: none; font-size: 12.5px; font-weight: 600; color: var(--fin-muted); }
.crm-f .fx-search, .crm-f .fx-select { flex: 1; min-width: 0; height: 34px; }
.crm-f--type { flex: .7; }
.crm-f-actions { flex: none; display: flex; gap: 8px; }
@media (max-width: 900px) {
  .crm-recent { order: 3; flex-basis: 100%; flex-wrap: wrap; overflow: visible; }
  .crm-filters { flex-wrap: wrap; }
  .crm-f { flex-basis: 100%; flex-direction: column; align-items: stretch; gap: 4px; }
}
.crm-last { display: flex; flex-direction: column; gap: 1px; line-height: 1.25; }
.crm-last small { font-size: 11.5px; color: var(--fin-muted); }
.crm-sort { display: inline-flex; align-items: center; gap: 6px; padding: 0; border: 0; background: none; font: inherit; color: inherit; text-transform: inherit; letter-spacing: inherit; cursor: pointer; white-space: nowrap; }
.crm-sort .fa { font-size: 11px; opacity: .45; }
.crm-sort.is-on { margin-inline-start: 6px; }
.crm-acts { white-space: nowrap; }
.crm-act { display: inline-flex; align-items: center; justify-content: center; gap: 5px; height: 28px; margin-inline-start: 6px; padding: 0 9px;
  border: 1px solid #d5dce2; border-radius: 8px; background: #fff; color: #34495a; font: 600 12px/1 inherit; text-decoration: none; cursor: pointer;
  vertical-align: middle; transition: background .15s, border-color .15s, color .15s; }
.crm-act--icon { width: 28px; padding: 0; }
.crm-act:hover, .crm-act:focus-visible { border-color: var(--fin-teal-600); background: var(--fin-teal-50); color: var(--fin-teal-700); }
.crm-ic { width: 14px; height: 14px; }
.crm-num { font-weight: 600; color: #1f3b4d; text-decoration: none; }
.crm-num:hover { color: var(--fin-teal-700); text-decoration: underline; }
.crm-status { display: inline-flex; align-items: center; gap: 6px; color: #3d4b56; }
.crm-dot { width: 7px; height: 7px; border-radius: 50%; background: #2e9e6a; }
.crm-dot--off { background: #c0392b; }
.crm-sort.is-on .fa { color: var(--fin-teal-600); opacity: 1; }
</style>
