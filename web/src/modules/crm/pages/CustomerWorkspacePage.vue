<template>
  <!-- Customer Workspace (Ahmed, 31 Oct 2026): the layer between Customers and a business line. Workspaces on Customers opens it;
       it shows the customer once and a workspace card per business line. Mortgage stays disabled until it is built. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'crm-customers' }">{{ tr('Customers') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ tr('Customer Workspace') }}</b>
        </nav>
        <h1>{{ tr('Customer Workspace') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'crm-customers' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Customers') }}</RouterLink>
      </div>
    </div>

    <section class="fx-card cw-customer">
      <div class="fc-customer-main">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-building-o"></span></span>
        <div>
          <span class="fc-customer-no">{{ client?.clientNumber || '—' }}</span>
          <b class="fc-customer-name">{{ client?.clientName || (loading ? tr('Loading...') : '—') }}</b>
          <span class="fc-customer-ar">{{ client?.clientNameAr || '' }}</span>
        </div>
      </div>
      <div class="fc-info"><span>{{ tr('Client Type') }}</span><span class="fx-chip fx-chip--teal fx-chip--plain">{{ client?.clientType || '—' }}</span></div>
      <div class="fc-info"><span>{{ tr('Phone') }}</span><b><span class="fa fa-phone" aria-hidden="true"></span> {{ client?.phoneNumber || '—' }}</b></div>
      <div class="fc-info"><span>{{ tr('Email') }}</span><b class="fc-email"><span class="fa fa-envelope-o" aria-hidden="true"></span> {{ client?.email || '—' }}</b></div>
    </section>

    <div class="fm-grid fm-grid--1 cw-grid">
      <component :is="w.to ? 'router-link' : 'div'" v-for="w in workspaces" :key="w.name" :to="w.to" class="fm-card fm-card--t2"
                 :class="{ 'is-off': !w.to }" :aria-disabled="w.to ? undefined : 'true'" :style="{ '--c': w.color, '--cl': w.tint }">
        <div class="fm-top">
          <span class="fm-ico"><span class="fa" :class="w.icon" aria-hidden="true"></span></span>
          <span class="fm-pill" :class="w.to ? 'fm-pill--live' : 'fm-pill--soon'">{{ tr(w.to ? 'Live' : 'Coming soon') }}</span>
        </div>
        <div class="fm-body"><small>{{ tr(w.short) }}</small><h3>{{ tr(w.name) }}</h3><p>{{ tr(w.descr) }}</p></div>
        <div class="fm-foot">
          <span v-if="w.to" class="fm-go">{{ tr('Open ' + w.name) }}<span class="fa fa-arrow-right" aria-hidden="true"></span></span>
          <span v-else class="fm-muted">{{ tr('In preparation · available soon') }}</span>
        </div>
      </component>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { crmApi, type CrmClientForm } from '../api/crm.api'

const props = defineProps<{ clientId: number }>()

const client = ref<CrmClientForm | null>(null)
const loading = ref(false)
const error = ref('')
useFeedback({ errors: [error] })

// Factoring opens the Factoring Customer Workspace; Mortgage is shown disabled until the mortgage customer workspace is built
const workspaces = computed(() => [
  { name: 'Factoring Workspace', short: 'Factoring', descr: 'Factoring applications, agreements, limits, disbursements and claims of this customer.',
    to: { name: 'FactoringCustomerWorkspace', params: { clientId: props.clientId }, query: { from: 'customer' } }, color: '#00636D', tint: '#E6F3F4',
    icon: 'fa-file-text-o' },
  { name: 'Mortgage Workspace', short: 'Mortgage', descr: 'Mortgage applications and finance of this customer.', to: null, color: '#E87722', tint: '#FFF1E6',
    icon: 'fa-home' }
])

onMounted(async () => {
  loading.value = true
  try {
    client.value = await crmApi.client(props.clientId)
  } catch (e) {
    error.value = e instanceof Error ? e.message : String(e)
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.cw-customer { display: grid; grid-template-columns: minmax(260px, 1.6fr) repeat(3, minmax(110px, 1fr)); gap: 14px 18px; align-items: center; padding: 16px 18px; }
.cw-grid { margin-block-start: 16px; grid-template-columns: repeat(4, minmax(0, 1fr)); }
.cw-grid a, .cw-grid a * { text-decoration: none; }
.fm-ico .fa { font-size: 22px; }
.fm-go .fa { margin-inline-start: 6px; }
@media (max-width: 1100px) { .cw-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
@media (max-width: 640px) { .cw-customer, .cw-grid { grid-template-columns: 1fr; } }
</style>
