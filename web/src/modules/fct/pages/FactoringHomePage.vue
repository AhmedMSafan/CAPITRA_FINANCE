<template>
  <!-- FINOVA_FINANCEING FactoringHomePage "Customers": the same filters, columns and paging, in this app's fx- theme.
       A customer opens the Factoring Customer Workspace. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-users"></span></span>
        <div>
          <h1>{{ tr('Customers') }}</h1>
          <p>{{ tr('View and manage customer information.') }}</p>
        </div>
      </div>
      <RouterLink to="/workflow/inbox" class="fc-inbox">
        <span class="fc-inbox-ic" aria-hidden="true"><span class="fa fa-envelope-o"></span></span>
        <span class="fc-inbox-copy"><b>{{ tr('Workflow Inbox') }}</b><small>{{ tr('View and process your assigned workflow tasks') }}</small></span>
        <em>{{ inboxCount }}</em>
        <span class="fa fa-arrow-right" aria-hidden="true"></span>
      </RouterLink>
    </div>

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
              <th class="fc-no">#</th>
              <th>{{ tr('Customer Number') }}</th>
              <th>{{ tr('Customer Name') }}</th>
              <th>{{ tr('Client Type') }}</th>
              <th>{{ tr('Phone') }}</th>
              <th>{{ tr('Email') }}</th>
              <th>{{ tr('Status') }}</th>
              <th class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(c, i) in paged" :key="c.clientId">
              <td class="fc-no">{{ from + i }}</td>
              <td><RouterLink :to="workspace(c.clientId)">{{ c.clientNumber }}</RouterLink></td>
              <td>{{ c.clientName }}</td>
              <td><span class="fx-chip fx-chip--plain">{{ c.clientType || '-' }}</span></td>
              <td>{{ c.phoneNumber || '-' }}</td>
              <td>{{ c.email || '-' }}</td>
              <td><span class="fx-chip" :class="c.isActive ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(c.isActive ? 'Active' : 'Inactive') }}</span></td>
              <td class="fx-r">
                <RouterLink class="fx-btn fx-btn--teal fx-btn--sm" :to="workspace(c.clientId)" :title="tr('Open Factoring Workspace')">
                  <span class="fa fa-play-circle-o" aria-hidden="true"></span>{{ tr('Process') }}
                </RouterLink>
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
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import './factoring.css'
import { computed, onMounted, ref } from 'vue'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { tr } from '@/shared/layout/language'
import { workflowService } from '@/modules/wfe/services/workflowService'
import { fctApi, type FctCustomer } from '../api/fct.api'

const ALL = 'All Types'
const PAGE_SIZE = 10

const customers = ref<FctCustomer[]>([])
const loading = ref(false)
const error = ref('')
useFeedback({ errors: [error] })
const inboxCount = ref(0)

const customerName = ref('')
const customerNumber = ref('')
const clientType = ref(ALL)
// the filters apply when Search is pressed (or Enter), as on the FINOVA page
const applied = ref({ name: '', number: '', type: ALL })
const page = ref(1)

const clientTypes = computed(() => [ALL, ...[...new Set(customers.value.map((c) => c.clientType).filter(Boolean))].sort()])

const filtered = computed(() => {
  const name = applied.value.name.trim().toLowerCase()
  const number = applied.value.number.trim().toLowerCase()
  return customers.value.filter((c) =>
    (!name || c.clientName.toLowerCase().includes(name)) &&
    (!number || c.clientNumber.toLowerCase().includes(number)) &&
    (applied.value.type === ALL || c.clientType === applied.value.type))
})
const pageCount = computed(() => Math.max(1, Math.ceil(filtered.value.length / PAGE_SIZE)))
const pages = computed(() => Array.from({ length: pageCount.value }, (_, i) => i + 1))
const paged = computed(() => filtered.value.slice((page.value - 1) * PAGE_SIZE, page.value * PAGE_SIZE))
const from = computed(() => (filtered.value.length ? (page.value - 1) * PAGE_SIZE + 1 : 0))
const to = computed(() => Math.min(page.value * PAGE_SIZE, filtered.value.length))

const workspace = (clientId: number) => ({ name: 'FactoringCustomerWorkspace', params: { clientId } })

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

onMounted(async () => {
  loading.value = true
  try {
    customers.value = await fctApi.customers()
  } catch (e) {
    error.value = e instanceof Error ? e.message : String(e)
  } finally {
    loading.value = false
  }
  try {
    inboxCount.value = (await workflowService.getMyInbox()).length
  } catch {
    inboxCount.value = 0
  }
})
</script>
