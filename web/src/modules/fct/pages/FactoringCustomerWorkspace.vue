<template>
  <!-- FINOVA_FINANCEING FactoringCustomerWorkspace: the customer card, the calculator and inbox cards and the five tabs,
       in this app's fx- theme. Everything for a customer is done from here. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'factoring-home' }">{{ tr('Factoring') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ tr('Process Customer') }}</b>
        </nav>
        <h1>{{ tr('Factoring Customer Workspace') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'factoring-home' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Factoring') }}</RouterLink>
      </div>
    </div>

    <section class="fx-card fc-customer">
      <div class="fc-customer-main">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-building-o"></span></span>
        <div>
          <span class="fc-customer-no">{{ client?.clientNumber || '—' }}</span>
          <b class="fc-customer-name">{{ client?.clientName || (loading ? tr('Loading...') : '—') }}</b>
          <span class="fc-customer-ar">{{ client?.clientNameAr || '' }}</span>
        </div>
      </div>
      <div class="fc-info"><span>{{ tr('Client Type') }}</span><span class="fx-chip fx-chip--teal fx-chip--plain">{{ client?.clientType || '—' }}</span></div>
      <div class="fc-info"><span>{{ tr('Country') }}</span><b>{{ client?.countryName || '—' }}</b></div>
      <div class="fc-info"><span>{{ tr('Currency') }}</span><span class="fx-chip fx-chip--brand fx-chip--plain">{{ client?.currencyCode || '—' }}</span></div>
      <div class="fc-info"><span>{{ tr('Relationship Manager') }}</span><b>{{ client?.relationshipManager || '—' }}</b></div>
      <div class="fc-info"><span>{{ tr('Phone') }}</span><b><span class="fa fa-phone" aria-hidden="true"></span> {{ client?.phoneNumber || '—' }}</b></div>
      <div class="fc-info"><span>{{ tr('Email') }}</span><b class="fc-email"><span class="fa fa-envelope-o" aria-hidden="true"></span> {{ client?.email || '—' }}</b></div>
    </section>

    <section class="fc-quick">
      <!-- the Factoring Calculator creates the application (FCT_OFFERS); it is the next page to be ported -->
      <button type="button" class="fx-card fc-quick-card fc-quick-card--calc" disabled :title="tr('Coming soon')">
        <span class="fc-quick-ic" aria-hidden="true"><span class="fa fa-calculator"></span></span>
        <span class="fc-quick-copy"><b>{{ tr('Factoring Calculator') }}</b><small>{{ tr('Create a new application using the factoring calculator') }}</small></span>
        <span class="fa fa-arrow-right" aria-hidden="true"></span>
      </button>
      <RouterLink to="/workflow/inbox" class="fx-card fc-quick-card">
        <span class="fc-quick-ic" aria-hidden="true"><span class="fa fa-envelope-o"></span></span>
        <span class="fc-quick-copy"><b>{{ tr('Workflow Inbox') }}</b><small>{{ tr('View and process your assigned workflow tasks') }}</small></span>
        <em class="fc-count">{{ inboxCount }}</em>
        <span class="fa fa-arrow-right" aria-hidden="true"></span>
      </RouterLink>
    </section>

    <section class="fx-card">
      <div class="fc-tabs" role="tablist">
        <button v-for="t in tabs" :key="t.key" type="button" role="tab" class="fc-tab" :aria-selected="tab === t.key" @click="tab = t.key">
          <span class="fa" :class="t.icon" aria-hidden="true"></span>{{ tr(t.label) }}<em>{{ t.count }}</em>
        </button>
      </div>

      <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading customer factoring data...') }}</div>
      <div v-else-if="error" class="fx-empty fc-error"><span class="fa fa-exclamation-circle" aria-hidden="true"></span>{{ error }}</div>

      <div v-else-if="tab === 'applications'" class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Application No.') }}</th><th>{{ tr('Application Date') }}</th><th class="fx-r">{{ tr('Requested Limit') }}</th>
              <th>{{ tr('Currency') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Workflow Step') }}</th>
              <th class="fc-c">{{ tr('WF Started') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="a in applications" :key="a.offerId">
              <td><b class="fc-ref">{{ a.offerRef || `Offer #${a.offerId}` }}</b></td>
              <td>{{ date(a.offerAt) }}</td>
              <td class="fx-r">{{ amount(a.requestedLimit) }}</td>
              <td>{{ a.currencyCode || '-' }}</td>
              <td>{{ a.factoringType || '-' }}</td>
              <td><span class="fx-chip" :class="tone(a.statusCode)">{{ a.statusCode || '-' }}</span></td>
              <td>{{ a.workflowStepName || '-' }}</td>
              <td class="fc-c">
                <span v-if="a.workflowStepName || a.agreementNo" class="fc-wf fc-wf--yes" :title="tr('Workflow Started')"><span class="fa fa-check" aria-hidden="true"></span></span>
                <span v-else class="fc-wf" :title="tr('Workflow Not Started')"><span class="fa fa-minus" aria-hidden="true"></span></span>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="!applications.length" class="fx-empty"><span class="fa fa-file-text-o" aria-hidden="true"></span>{{ tr('No applications found for this customer.') }}</div>
      </div>

      <div v-else-if="tab === 'agreements'" class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Agreement No.') }}</th><th>{{ tr('Application Date') }}</th><th class="fx-r">{{ tr('Facility Limit') }}</th>
              <th>{{ tr('Currency') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Workflow Step') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="g in agreements" :key="g.agreementId">
              <td><b class="fc-ref">{{ g.agreementNumber || `Agreement #${g.agreementId}` }}</b></td>
              <td>{{ date(g.applicationAt) }}</td>
              <td class="fx-r">{{ amount(g.facilityLimit) }}</td>
              <td>{{ g.currencyCode || '-' }}</td>
              <td>{{ g.factoringType || '-' }}</td>
              <td><span class="fx-chip" :class="tone(g.statusCode)">{{ g.statusCode || '-' }}</span></td>
              <td>{{ g.workflowStepName || '-' }}</td>
            </tr>
          </tbody>
        </table>
        <div v-if="!agreements.length" class="fx-empty"><span class="fa fa-handshake-o" aria-hidden="true"></span>{{ tr('No agreements found for this customer.') }}</div>
      </div>

      <!-- FINOVA shows these three tabs as placeholders -->
      <div v-else class="fx-empty">
        <span class="fa" :class="tabs.find((t) => t.key === tab)?.icon" aria-hidden="true"></span>
        {{ tr(tab === 'limits' ? 'Approved limits will be available here.' : tab === 'disbursements' ? 'Disbursement data will be available here.' : 'Claims data will be available here.') }}
      </div>

      <div class="fc-foot">
        <span class="fc-muted">{{ tr('Showing') }} {{ rows ? 1 : 0 }} {{ tr('to') }} {{ rows }} {{ tr('of') }} {{ rows }} {{ tr('entries') }}</span>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import './factoring.css'
import { computed, onMounted, ref } from 'vue'
import { dateLocale, tr } from '@/shared/layout/language'
import { DISPLAY_TIME_ZONE } from '@/shared/format/datetime'
import { workflowService } from '@/modules/wfe/services/workflowService'
import { fctApi, type FctAgreement, type FctApplication, type FctCustomerDetails } from '../api/fct.api'

const props = defineProps<{ clientId: string }>()

type Tab = 'applications' | 'agreements' | 'limits' | 'disbursements' | 'claims'

const client = ref<FctCustomerDetails | null>(null)
const applications = ref<FctApplication[]>([])
const agreements = ref<FctAgreement[]>([])
const inboxCount = ref(0)
const loading = ref(false)
const error = ref('')
const tab = ref<Tab>('applications')

const tabs = computed(() => [
  { key: 'applications' as const, label: 'Factoring Applications', count: applications.value.length, icon: 'fa-file-text-o' },
  { key: 'agreements' as const, label: 'Agreements', count: agreements.value.length, icon: 'fa-handshake-o' },
  { key: 'limits' as const, label: 'Limits', count: 0, icon: 'fa-database' },
  { key: 'disbursements' as const, label: 'Disbursements', count: 0, icon: 'fa-exchange' },
  { key: 'claims' as const, label: 'Claims', count: 0, icon: 'fa-flag-o' }
])
const rows = computed(() => (tab.value === 'applications' ? applications.value.length : tab.value === 'agreements' ? agreements.value.length : 0))

const amount = (v: number | null | undefined) =>
  v == null ? '-' : new Intl.NumberFormat('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 2 }).format(v)

const date = (v: string | null | undefined) =>
  v ? new Intl.DateTimeFormat(dateLocale(), { year: 'numeric', month: 'short', day: '2-digit', timeZone: DISPLAY_TIME_ZONE }).format(new Date(v)) : '-'

// FINOVA's statusClass(): approved / accepted green, rejected / cancelled red, in review violet-teal, anything else amber
function tone(status: string | null | undefined) {
  const s = (status || '').toUpperCase()
  if (s.includes('APPROV') || s === 'ACCEPTED') return 'fx-chip--ok'
  if (s.includes('REJECT') || s.includes('CANCEL')) return 'fx-chip--bad'
  if (s.includes('REVIEW') || s.includes('PROCESS')) return 'fx-chip--teal'
  return 'fx-chip--warn'
}

onMounted(async () => {
  const id = Number(props.clientId)
  if (!Number.isFinite(id) || id <= 0) {
    error.value = tr('Customer ID is missing from the route.')
    return
  }
  loading.value = true
  try {
    ;[client.value, applications.value, agreements.value] = await Promise.all([fctApi.customer(id), fctApi.applications(id), fctApi.agreements(id)])
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
