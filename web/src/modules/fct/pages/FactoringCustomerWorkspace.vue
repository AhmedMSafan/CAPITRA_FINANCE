<template>
  <!-- FINOVA_FINANCEING FactoringCustomerWorkspace: the customer card, the calculator and inbox cards and the five tabs,
       in this app's fx- theme. Everything for a customer is done from here. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <template v-if="fromCustomer">
            <RouterLink :to="{ name: 'crm-customers' }">{{ tr('Customers') }}</RouterLink><span aria-hidden="true">›</span>
            <RouterLink :to="customerWorkspace">{{ tr('Customer Workspace') }}</RouterLink><span aria-hidden="true">›</span>
          </template>
          <template v-else><RouterLink :to="{ name: 'factoring-home' }">{{ tr('Factoring') }}</RouterLink><span aria-hidden="true">›</span></template>
          <b>{{ tr('Process Customer') }}</b>
        </nav>
        <h1>{{ tr('Factoring Customer Workspace') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink v-if="fromCustomer" :to="customerWorkspace" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Customer Workspace') }}</RouterLink>
        <RouterLink v-else :to="{ name: 'factoring-home' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Factoring') }}</RouterLink>
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

    <section class="fc-quick fc-quick--3">
      <!-- the Factoring Calculator creates the application (FCT_OFFERS) -->
      <RouterLink :to="{ name: 'FactoringCalculator', params: { clientId } }" class="fx-card fc-quick-card fc-quick-card--calc">
        <span class="fc-quick-ic" aria-hidden="true"><span class="fa fa-calculator"></span></span>
        <span class="fc-quick-copy"><b>{{ tr('Factoring Calculator') }}</b><small>{{ tr('Create a new application using the factoring calculator') }}</small></span>
        <span class="fa fa-arrow-right" aria-hidden="true"></span>
      </RouterLink>
      <RouterLink to="/workflow/inbox" class="fx-card fc-quick-card">
        <span class="fc-quick-ic" aria-hidden="true"><span class="fa fa-envelope-o"></span></span>
        <span class="fc-quick-copy"><b>{{ tr('Workflow Inbox') }}</b><small>{{ tr('View and process your assigned workflow tasks') }}</small></span>
        <em class="fc-count">{{ inboxCount }}</em>
        <span class="fa fa-arrow-right" aria-hidden="true"></span>
      </RouterLink>
      <!-- Customer Disbursement Request (APEX App 150722 page 750), moved here from its CRM card -->
      <RouterLink :to="{ name: 'crm-disbursement-requests' }" class="fx-card fc-quick-card">
        <span class="fc-quick-ic" aria-hidden="true"><span class="fa fa-money"></span></span>
        <span class="fc-quick-copy"><b>{{ tr('Disbursement Requests') }}</b><small>{{ tr('Request a disbursement against an active credit limit and start the disbursement workflow.') }}</small></span>
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

      <div v-else-if="tab === 'applications'" class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Application No.') }}</th><th>{{ tr('Application Date') }}</th><th class="fx-r">{{ tr('Requested Limit') }}</th>
              <th>{{ tr('Currency') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Workflow Step') }}</th>
              <th class="fc-c">{{ tr('WF Started') }}</th>
              <th v-if="canEdit" class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="a in applications" :key="a.offerId">
              <td><b class="fc-ref">{{ a.offerRef || `Offer #${a.offerId}` }}</b></td>
              <td>{{ date(a.offerAt) }}</td>
              <td class="fx-r">{{ amount(a.requestedLimit) }}</td>
              <td>{{ a.currencyCode || '-' }}</td>
              <td>{{ a.factoringType || '-' }}</td>
              <td>
                <select v-if="canEdit && editable(a)" v-model="selected[a.offerId]" class="fx-select fc-status-select" :aria-label="`${tr('Status')} ${a.offerRef}`"
                        :disabled="busy[a.offerId] || !!a.agreementNo">
                  <option v-for="st in STATUS_OPTIONS" :key="st" :value="st">{{ st }}</option>
                </select>
                <span v-else class="fx-chip" :class="tone(a.statusCode)">{{ a.statusCode || '-' }}</span>
              </td>
              <td>{{ a.workflowStepName || '-' }}</td>
              <td class="fc-c">
                <span v-if="a.workflowStepName || a.agreementNo" class="fc-wf fc-wf--yes" :title="tr('Workflow Started')"><span class="fa fa-check" aria-hidden="true"></span></span>
                <span v-else class="fc-wf" :title="tr('Workflow Not Started')"><span class="fa fa-minus" aria-hidden="true"></span></span>
              </td>
              <td v-if="canEdit" class="fx-r">
                <div class="fc-row-actions">
                  <button type="button" class="fx-btn fx-btn--sm" :title="tr(busy[a.offerId] ? 'Saving...' : 'Save Status')" :aria-label="`${tr('Save Status')} ${a.offerRef}`"
                          :disabled="!dirty(a) || busy[a.offerId] || !!a.agreementNo" @click="saveStatus(a)">
                    <span class="fa fa-floppy-o" aria-hidden="true"></span>
                  </button>
                  <button type="button" class="fx-btn fx-btn--primary fx-btn--sm" :title="tr(startHint(a))" :aria-label="`${tr('Start Agreement Workflow')} ${a.offerRef}`"
                          :disabled="!canStart(a)" @click="startWorkflow(a)">
                    <span class="fa fa-play" aria-hidden="true"></span>
                  </button>
                </div>
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

      <div v-else-if="tab === 'limits'" class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Limit No.') }}</th><th>{{ tr('Level') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('Currency') }}</th>
              <th class="fx-r">{{ tr('Approved Amount') }}</th><th class="fx-r">{{ tr('Utilized') }}</th><th class="fx-r">{{ tr('Available') }}</th>
              <th>{{ tr('Effective Date') }}</th><th>{{ tr('Expiry Date') }}</th><th>{{ tr('Status') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="l in limits" :key="l.creditLimitId">
              <td><b class="fc-ref">{{ l.limitNumber }}</b></td>
              <td>{{ l.limitLevel }}</td>
              <td>{{ l.limitType }}</td>
              <td>{{ l.currencyCode || '-' }}</td>
              <td class="fx-r">{{ amount(l.approvedAmount) }}</td>
              <td class="fx-r">{{ amount(l.utilizedAmount) }}</td>
              <td class="fx-r">{{ amount(l.availableAmount) }}</td>
              <td>{{ date(l.effectiveAt) }}</td>
              <td>{{ date(l.expiryAt) }}</td>
              <td><span class="fx-chip" :class="l.isActive ? 'fx-chip--ok' : 'fx-chip--warn'">{{ tr(l.isActive ? 'Active' : 'Inactive') }}</span></td>
            </tr>
          </tbody>
        </table>
        <div v-if="!limits.length" class="fx-empty"><span class="fa fa-database" aria-hidden="true"></span>{{ tr('Approved limits will be available here.') }}</div>
      </div>

      <!-- FINOVA shows these two tabs as placeholders -->
      <div v-else class="fx-empty">
        <span class="fa" :class="tabs.find((t) => t.key === tab)?.icon" aria-hidden="true"></span>
        {{ tr(tab === 'disbursements' ? 'Disbursement data will be available here.' : 'Claims data will be available here.') }}
      </div>

      <div class="fc-foot">
        <span class="fc-muted">{{ tr('Showing') }} {{ rows ? 1 : 0 }} {{ tr('to') }} {{ rows }} {{ tr('of') }} {{ rows }} {{ tr('entries') }}</span>
      </div>
    </section>

    <!-- FINOVA's "Starting Workflow" modal. Capitra creates the agreement and starts the workflow in one call,
         so both steps complete together when it returns. -->
    <div v-if="progress" class="fc-modal-back" role="presentation">
      <section class="fc-modal" role="dialog" aria-modal="true" aria-labelledby="fc-wf-title">
        <h2 id="fc-wf-title">{{ tr('Starting Workflow') }}</h2>
        <ol>
          <li :class="progress === 'working' ? 'is-active' : 'is-done'">
            <span class="fa" :class="progress === 'working' ? 'fa-spinner fa-spin' : 'fa-check-circle'" aria-hidden="true"></span>{{ tr('Creating Application') }}
          </li>
          <li :class="progress === 'working' ? 'is-active' : 'is-done'">
            <span class="fa" :class="progress === 'working' ? 'fa-spinner fa-spin' : 'fa-check-circle'" aria-hidden="true"></span>{{ tr('Starting Workflow') }}
          </li>
        </ol>
        <p class="fc-muted"><span class="fa fa-info-circle" aria-hidden="true"></span> {{ tr('Do not close or refresh this page until the process is complete.') }}</p>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import './factoring.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { dateLocale, tr } from '@/shared/layout/language'
import { DISPLAY_TIME_ZONE } from '@/shared/format/datetime'
import { workflowService } from '@/modules/wfe/services/workflowService'
import { showError } from '@/shared/feedback/feedback'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { hasPermission } from '@/shared/auth/session'
import { fctApi, fctApplicationsApi, type FctAgreement, type FctApplication, type FctCreditLimit, type FctCustomerDetails } from '../api/fct.api'

const props = defineProps<{ clientId: string }>()
const route = useRoute()
// opened from the Customer Workspace (Customers > Process): the way back goes there, not to the Factoring home
const fromCustomer = computed(() => route.query.from === 'customer')
const customerWorkspace = computed(() => ({ name: 'crm-customer-workspace', params: { clientId: props.clientId } }))

type Tab = 'applications' | 'agreements' | 'limits' | 'disbursements' | 'claims'

const client = ref<FctCustomerDetails | null>(null)
const applications = ref<FctApplication[]>([])
const agreements = ref<FctAgreement[]>([])
const limits = ref<FctCreditLimit[]>([])
const inboxCount = ref(0)
const loading = ref(false)
const error = ref('')
useFeedback({ errors: [error] })
const tab = ref<Tab>('applications')
const canEdit = computed(() => hasPermission('FCT.EDIT'))

// FINOVA's application actions: the status can be changed while it is one of these, and the agreement workflow starts
// from a saved ACCEPTED application that has no agreement yet.
const STATUS_OPTIONS = ['DRAFT', 'ACCEPTED', 'REJECTED', 'SUBMITTED']
const selected = reactive<Record<number, string>>({})
const busy = reactive<Record<number, boolean>>({})
const progress = ref<'' | 'working' | 'done'>('')

const editable = (a: FctApplication) => STATUS_OPTIONS.includes(a.statusCode || '')
const dirty = (a: FctApplication) => editable(a) && selected[a.offerId] !== a.statusCode
const canStart = (a: FctApplication) => a.statusCode === 'ACCEPTED' && !dirty(a) && !busy[a.offerId] && !a.agreementNo
function startHint(a: FctApplication) {
  if (a.statusCode !== 'ACCEPTED') return 'Save ACCEPTED status first'
  if (dirty(a)) return 'Save Status before starting workflow'
  if (a.agreementNo) return 'Agreement already generated'
  return 'Start Agreement Workflow'
}

const tabs = computed(() => [
  { key: 'applications' as const, label: 'Factoring Applications', count: applications.value.length, icon: 'fa-file-text-o' },
  { key: 'agreements' as const, label: 'Agreements', count: agreements.value.length, icon: 'fa-handshake-o' },
  { key: 'limits' as const, label: 'Limits', count: limits.value.length, icon: 'fa-database' },
  { key: 'disbursements' as const, label: 'Disbursements', count: 0, icon: 'fa-exchange' },
  { key: 'claims' as const, label: 'Claims', count: 0, icon: 'fa-flag-o' }
])
const rows = computed(() =>
  tab.value === 'applications' ? applications.value.length : tab.value === 'agreements' ? agreements.value.length : tab.value === 'limits' ? limits.value.length : 0)

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

const errorText = (e: unknown) => (e instanceof Error ? e.message : String(e))

async function loadData(id: number) {
  ;[client.value, applications.value, agreements.value, limits.value] = await Promise.all([
    fctApi.customer(id), fctApi.applications(id), fctApi.agreements(id), fctApi.creditLimits(id)
  ])
  for (const a of applications.value) selected[a.offerId] = a.statusCode || ''
}

async function loadInbox() {
  try {
    inboxCount.value = (await workflowService.getMyInbox()).length
  } catch {
    inboxCount.value = 0
  }
}

async function saveStatus(a: FctApplication) {
  const status = selected[a.offerId] ?? ''
  if (!dirty(a) || !STATUS_OPTIONS.includes(status)) return
  busy[a.offerId] = true
  try {
    await fctApplicationsApi.setStatus(a.offerId, status)
    a.statusCode = status
  } catch (e) {
    showError(errorText(e))
  } finally {
    busy[a.offerId] = false
  }
}

async function startWorkflow(a: FctApplication) {
  if (!canStart(a)) return
  busy[a.offerId] = true
  progress.value = 'working'
  try {
    await fctApplicationsApi.startWorkflow(a.offerId)
    progress.value = 'done'
    await Promise.all([loadData(Number(props.clientId)), loadInbox()])
    await new Promise((r) => setTimeout(r, 700))
  } catch (e) {
    showError(errorText(e))
  } finally {
    progress.value = ''
    busy[a.offerId] = false
  }
}

onMounted(async () => {
  const id = Number(props.clientId)
  if (!Number.isFinite(id) || id <= 0) {
    error.value = tr('Customer ID is missing from the route.')
    return
  }
  loading.value = true
  try {
    await loadData(id)
  } catch (e) {
    error.value = errorText(e)
  } finally {
    loading.value = false
  }
  await loadInbox()
})
</script>
