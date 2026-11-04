<template>
  <!-- Customer 360 (Ahmed, 4 Nov 2026): one page for everything we know about a customer, whatever role it plays. The header shows the
       three answers that matter first (is compliance clear, is there a bank account we can pay, can we pay today); the tabs hold the
       business-line workspaces, the profile, compliance, bank accounts, and roles and programs. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'crm-customers' }">{{ tr('Customers') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ tr('Customer 360') }}</b>
        </nav>
        <h1>{{ tr('Customer 360') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'crm-customers' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Customers') }}</RouterLink>
      </div>
    </div>

    <section class="fx-card c3-head">
      <div class="fc-customer-main">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-building-o"></span></span>
        <div>
          <span class="fc-customer-no">{{ p?.clientNumber || '—' }}</span>
          <b class="fc-customer-name">{{ p?.clientName || (loading ? tr('Loading...') : '—') }}</b>
          <span class="fc-customer-ar">{{ p?.clientNameAr || '' }}</span>
          <div class="c3-ids">
            <span v-if="p?.taxNumber">{{ tr('Tax No.') }} <b>{{ p.taxNumber }}</b></span>
            <span v-if="p?.registrationNumber">{{ tr('Reg. No.') }} <b>{{ p.registrationNumber }}</b></span>
            <span v-if="p?.countryName">{{ p.countryName }}</span>
          </div>
          <div class="c3-roles">
            <span v-for="r in data?.roles ?? []" :key="r.clientRoleId" class="fx-chip" :class="chip(r.statusCode)" :title="tr(label(r.statusCode))">{{ tr(r.roleLabel) }}</span>
            <span v-if="data && !data.roles.length" class="c3-muted">{{ tr('No roles yet') }}</span>
          </div>
        </div>
      </div>
      <button v-for="b in badges" :key="b.key" type="button" class="c3-badge" :class="`c3-badge--${tone(b.badge.code)}`" @click="tab = b.tab">
        <span class="c3-badge-k"><span class="fa" :class="b.icon" aria-hidden="true"></span>{{ tr(b.title) }}</span>
        <b>{{ tr(b.badge.label) }}</b>
        <small v-if="b.badge.reasons.length">{{ tr(b.badge.reasons[0] ?? '') }}<template v-if="b.badge.reasons.length > 1"> (+{{ b.badge.reasons.length - 1 }})</template></small>
      </button>
    </section>

    <section class="fx-card">
      <div class="fc-tabs" role="tablist">
        <button v-for="t in tabs" :key="t.key" type="button" role="tab" class="fc-tab" :aria-selected="tab === t.key" @click="tab = t.key">
          <span class="fa" :class="t.icon" aria-hidden="true"></span>{{ tr(t.label) }}<em v-if="t.count !== undefined">{{ t.count }}</em>
        </button>
      </div>

      <div class="c3-body">
        <div v-if="loading && !data" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading customer...') }}</div>

        <template v-else-if="data">
          <div v-if="tab === 'overview'" class="c3-tab">
            <div v-if="data.readyToPay.reasons.length" class="c3-panel">
              <header class="c3-panel-head">
                <h3><span class="fa fa-money" aria-hidden="true"></span>{{ tr('Why this customer cannot be paid yet') }}</h3>
              </header>
              <ul class="c3-reasons"><li v-for="r in data.readyToPay.reasons" :key="r">{{ tr(r) }}</li></ul>
            </div>
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

          <div v-else-if="tab === 'profile'" class="c3-tab">
            <section class="c3-panel">
              <header class="c3-panel-head">
                <h3><span class="fa fa-id-card-o" aria-hidden="true"></span>{{ tr('Profile') }}</h3>
                <span class="fx-chip" :class="data.profile.isActive ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(data.profile.isActive ? 'Active' : 'Inactive') }}</span>
                <div class="c3-actions">
                  <button v-if="canEdit" type="button" class="fx-btn" @click="editing = true"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}</button>
                </div>
              </header>
              <dl class="c3-facts">
                <div v-for="f in profileFacts" :key="f.k"><dt>{{ tr(f.k) }}</dt><dd :dir="f.rtl ? 'rtl' : undefined">{{ f.v || '—' }}</dd></div>
              </dl>
            </section>
          </div>

          <Crm360Compliance v-else-if="tab === 'compliance'" :client-id="clientId" :state="data.compliance" :badge="data.complianceBadge"
                            :history="data.complianceHistory" :can-edit="canEdit" :can-verify="canVerify" @changed="load" />

          <Crm360BankAccounts v-else-if="tab === 'bank'" :client-id="clientId" :accounts="data.bankAccounts" :history="data.bankAccountHistory"
                              :badge="data.bankBadge" :currencies="currencies" :default-currency-id="baseCurrencyId" :holder-name="data.profile.clientName"
                              :waiting-days="data.bankAccountWaitingDays" :can-edit="canEdit" :can-verify="canVerify" @changed="load" />

          <Crm360Roles v-else-if="tab === 'roles'" :client-id="clientId" :roles="data.roles" :programs="data.programs" :can-edit="canEdit" @changed="load" />
        </template>
      </div>
    </section>

    <CrmClientDialog v-model:visible="editing" :client-id="clientId" @saved="load" />
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { crm360Api, crmApi, type CrmBadge, type CrmCustomer360, type CrmOption } from '../api/crm.api'
import Crm360BankAccounts from '../components/Crm360BankAccounts.vue'
import Crm360Compliance from '../components/Crm360Compliance.vue'
import Crm360Roles from '../components/Crm360Roles.vue'
import CrmClientDialog from '../components/CrmClientDialog.vue'
import { chip, label } from '../components/crm360'

const props = defineProps<{ clientId: number }>()

const data = ref<CrmCustomer360 | null>(null)
const currencies = ref<CrmOption[]>([])
const baseCurrencyId = ref(0)
const loading = ref(false)
const editing = ref(false)
const tab = ref('overview')
const canEdit = hasPermission('CRM.EDIT')
const canVerify = hasPermission('CRM.VERIFY')
const p = computed(() => data.value?.profile)

const tone = (code: string) => ({ CLEAR: 'ok', VERIFIED: 'ok', YES: 'ok', DUE: 'warn', PENDING: 'warn', WAITING: 'warn' } as Record<string, string>)[code] ?? 'bad'
const badges = computed(() => data.value ? [
  { key: 'c', title: 'Compliance', icon: 'fa-shield', badge: data.value.complianceBadge, tab: 'compliance' },
  { key: 'b', title: 'Bank Account', icon: 'fa-university', badge: data.value.bankBadge, tab: 'bank' },
  { key: 'r', title: 'Ready to Pay', icon: 'fa-money', badge: data.value.readyToPay, tab: 'overview' }
] as { key: string; title: string; icon: string; badge: CrmBadge; tab: string }[] : [])

const tabs = computed(() => [
  { key: 'overview', label: 'Overview', icon: 'fa-th-large' },
  { key: 'profile', label: 'Profile', icon: 'fa-id-card-o' },
  { key: 'compliance', label: 'Compliance', icon: 'fa-shield' },
  { key: 'bank', label: 'Bank Accounts', icon: 'fa-university', count: data.value?.bankAccounts.length },
  { key: 'roles', label: 'Roles and Programs', icon: 'fa-tags', count: data.value?.roles.length }
])

const profileFacts = computed(() => {
  const x = data.value?.profile
  if (!x) return []
  return [
    { k: 'Client Number', v: x.clientNumber }, { k: 'Client Name', v: x.clientName }, { k: 'Arabic Client Name', v: x.clientNameAr, rtl: true },
    { k: 'Short Name', v: x.shortName }, { k: 'Client Type', v: x.clientType }, { k: 'Legal Structure', v: x.legalStructure },
    { k: 'Tax Number', v: x.taxNumber }, { k: 'Commercial Registration', v: x.registrationNumber }, { k: 'VAT Number', v: x.vatNumber },
    { k: 'National ID', v: x.nationalId }, { k: 'Country', v: x.countryName }, { k: 'Base Currency', v: x.currencyCode },
    { k: 'Status', v: x.statusName }, { k: 'Group', v: x.groupName }, { k: 'Related Party', v: tr(x.isRelatedParty ? 'Yes' : 'No') },
    { k: 'Email', v: x.email }, { k: 'Phone', v: x.phoneNumber }, { k: 'Mobile', v: x.mobileNumber }, { k: 'Website', v: x.website }
  ]
})

// Factoring opens the Factoring Customer Workspace; Mortgage is shown disabled until the mortgage customer workspace is built
const workspaces = computed(() => [
  { name: 'Factoring Workspace', short: 'Factoring', descr: 'Factoring applications, agreements, limits, disbursements and claims of this customer.',
    to: { name: 'FactoringCustomerWorkspace', params: { clientId: props.clientId }, query: { from: 'customer' } }, color: '#00636D', tint: '#E6F3F4',
    icon: 'fa-file-text-o' },
  { name: 'Mortgage Workspace', short: 'Mortgage', descr: 'Mortgage applications and finance of this customer.', to: null, color: '#E87722', tint: '#FFF1E6',
    icon: 'fa-home' }
])

async function load() {
  loading.value = true
  try {
    const [d, opts, client] = await Promise.all([
      crm360Api.get(props.clientId),
      currencies.value.length ? Promise.resolve(null) : crmApi.options(),
      crmApi.client(props.clientId)
    ])
    data.value = d
    if (opts) currencies.value = opts.currencies
    baseCurrencyId.value = client.baseCurrencyId
  } catch (e) {
    showError(e instanceof Error ? e.message : String(e))
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<style scoped>
@import '../components/crm360.css';
.c3-head { display: grid; grid-template-columns: minmax(280px, 1.6fr) repeat(3, minmax(150px, 1fr)); gap: 14px; align-items: stretch; padding: 16px 18px; }
.c3-ids { display: flex; flex-wrap: wrap; gap: 4px 14px; margin-block-start: 6px; font-size: var(--fs-xs); color: var(--fin-muted); }
.c3-ids b { color: var(--fin-ink); }
.c3-roles { display: flex; flex-wrap: wrap; gap: 6px; margin-block-start: 8px; }
.c3-badge { appearance: none; display: flex; flex-direction: column; gap: 4px; text-align: start; font: inherit; padding: 12px 14px; cursor: pointer;
  border: 1px solid var(--fin-line); border-inline-start: 4px solid var(--c3-c); border-radius: var(--fin-radius); background: var(--c3-bg); }
.c3-badge:hover { box-shadow: var(--fin-shadow); }
.c3-badge-k { display: flex; align-items: center; gap: 6px; font-size: var(--fs-xs); font-weight: 700; color: var(--fin-muted); }
.c3-badge b { font-size: var(--fs-lg); color: var(--c3-c); }
.c3-badge small { color: var(--fin-ink); font-size: var(--fs-xs); line-height: 1.35; }
.c3-badge--ok { --c3-c: var(--fin-ok); --c3-bg: var(--fin-ok-50); }
.c3-badge--warn { --c3-c: var(--fin-warn); --c3-bg: var(--fin-warn-50); }
.c3-badge--bad { --c3-c: var(--fin-bad); --c3-bg: var(--fin-bad-50); }
.c3-body { padding: 16px; }
.cw-grid { grid-template-columns: repeat(4, minmax(0, 1fr)); }
.cw-grid a, .cw-grid a * { text-decoration: none; }
.fm-ico .fa { font-size: 22px; }
.fm-go .fa { margin-inline-start: 6px; }
@media (max-width: 1100px) { .c3-head { grid-template-columns: repeat(3, minmax(0, 1fr)); } .c3-head > .fc-customer-main { grid-column: 1 / -1; }
  .cw-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
@media (max-width: 640px) { .c3-head, .cw-grid { grid-template-columns: 1fr; } }
</style>
