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

    <!-- one compact row (Ahmed, 25 Nov 2026): who the customer is on the left, the three answers on the right -->
    <section class="fx-card c3-head">
      <div class="c3-who">
        <span class="c3-ic" aria-hidden="true"><span class="fa fa-building-o"></span></span>
        <div class="c3-who-txt">
          <div class="c3-line">
            <span class="c3-no" dir="ltr">{{ p?.clientNumber || '—' }}</span>
            <b class="c3-name">{{ p?.clientName || (loading ? tr('Loading...') : '—') }}</b>
            <span v-if="p?.clientNameAr" class="c3-ar" dir="auto">{{ p.clientNameAr }}</span>
          </div>
          <div class="c3-line c3-sub">
            <span v-if="p?.taxNumber">{{ tr('Tax No.') }} <b>{{ p.taxNumber }}</b></span>
            <span v-if="p?.registrationNumber">{{ tr('Reg. No.') }} <b>{{ p.registrationNumber }}</b></span>
            <span v-if="p?.countryName">{{ p.countryName }}</span>
            <span v-for="r in data?.roles ?? []" :key="r.clientRoleId" class="c3-role" :title="tr(label(r.statusCode))">
              <span class="c3-dot" :class="`c3-dot--${roleTone(r.statusCode)}`" aria-hidden="true"></span>{{ tr(r.roleLabel) }}
            </span>
            <span v-if="data && !data.roles.length" class="c3-muted">{{ tr('No roles yet') }}</span>
          </div>
        </div>
      </div>
      <div class="c3-badges">
        <button v-for="b in badges" :key="b.key" type="button" class="c3-badge" :class="`c3-badge--${tone(b.badge.code)}`"
                :title="b.badge.reasons.map((r) => tr(r)).join('\n')" @click="tab = b.tab">
          <span class="c3-badge-k"><span class="fa" :class="b.icon" aria-hidden="true"></span>{{ tr(b.title) }}</span>
          <b>{{ tr(b.badge.label) }}</b>
          <small v-if="b.badge.reasons.length">{{ tr(b.badge.reasons[0] ?? '') }}<template v-if="b.badge.reasons.length > 1"> (+{{ b.badge.reasons.length - 1 }})</template></small>
        </button>
      </div>
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
          <Crm360Overview v-if="tab === 'overview'" :client-id="clientId" :version="version" @open-tab="tab = $event" />

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
                            :history="data.complianceHistory" :maker-checker="data.complianceMakerChecker" :can-edit="canEdit" :can-verify="canVerify" @changed="load" />

          <Crm360BankAccounts v-else-if="tab === 'bank'" :client-id="clientId" :accounts="data.bankAccounts" :history="data.bankAccountHistory"
                              :badge="data.bankBadge" :currencies="currencies" :default-currency-id="baseCurrencyId" :holder-name="data.profile.clientName"
                              :waiting-days="data.bankAccountWaitingDays" :maker-checker="data.bankAccountMakerChecker" :can-edit="canEdit" :can-verify="canVerify" @changed="load" />

          <Crm360Roles v-else-if="tab === 'roles'" :client-id="clientId" :roles="data.roles" :programs="data.programs" :can-edit="canEdit" @changed="load"
                       @open-tab="(t) => (tab = t)" />

          <CfSupplierPanel v-else-if="tab === 'cf-supplier'" :client-id="clientId" :inbox-id="inboxId" @changed="load" />
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
import { useRoute } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { crm360Api, crmApi, type CrmBadge, type CrmCustomer360, type CrmOption } from '../api/crm.api'
import Crm360BankAccounts from '../components/Crm360BankAccounts.vue'
import Crm360Compliance from '../components/Crm360Compliance.vue'
import Crm360Overview from '../components/Crm360Overview.vue'
import Crm360Roles from '../components/Crm360Roles.vue'
import CfSupplierPanel from '../components/CfSupplierPanel.vue'
import CrmClientDialog from '../components/CrmClientDialog.vue'
import { label } from '../components/crm360'

const props = defineProps<{ clientId: number }>()

const data = ref<CrmCustomer360 | null>(null)
const currencies = ref<CrmOption[]>([])
const baseCurrencyId = ref(0)
const loading = ref(false)
const editing = ref(false)
// changes after every load, so the Overview reads its data again
const version = ref(0)
const route = useRoute()
// ?tab= opens a tab (cf-supplier from the supplier list or a workflow task); ?inboxId= is the user's task on the supplier application
const tab = ref(typeof route.query.tab === 'string' ? route.query.tab : 'overview')
const inboxId = computed(() => Number(route.query.inboxId) || null)
const canEdit = hasPermission('CRM.EDIT')
const canVerify = hasPermission('CRM.VERIFY')
const p = computed(() => data.value?.profile)

// the role's status as a small dot: Active green, Onboarding amber, Suspended red
const roleTone = (code: string) => ({ ACTIVE: 'ok', ONBOARDING: 'warn', SUSPENDED: 'bad' } as Record<string, string>)[code] ?? 'none'
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
  { key: 'roles', label: 'Roles and Programs', icon: 'fa-tags', count: data.value?.roles.length },
  // a Consumer Finance Supplier: its application, terms and approval workflow
  ...(data.value?.roles.some((r) => r.byWorkflow) || tab.value === 'cf-supplier'
    ? [{ key: 'cf-supplier', label: 'Consumer Finance Supplier', icon: 'fa-handshake-o', count: undefined }] : [])
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

async function load() {
  loading.value = true
  try {
    const [d, opts, client] = await Promise.all([
      crm360Api.get(props.clientId),
      currencies.value.length ? Promise.resolve(null) : crmApi.options(),
      crmApi.client(props.clientId)
    ])
    data.value = d
    version.value++
    if (opts) currencies.value = opts.currencies
    baseCurrencyId.value = client.baseCurrencyId
  } catch (e) {
    showError(e instanceof Error ? e.message : String(e))
  } finally {
    loading.value = false
  }
}

onMounted(() => {
  void load()
  // for "Recently opened" on the Customers page; a failure here never stops the page
  crmApi.opened(props.clientId).catch(() => undefined)
})
</script>

<style scoped>
@import '../components/crm360.css';
.c3-head { display: flex; align-items: center; gap: 16px; padding: 12px 16px; }
.c3-who { flex: 1; min-width: 0; display: flex; align-items: center; gap: 12px; }
.c3-ic { flex: none; display: grid; place-items: center; width: 40px; height: 40px; border-radius: 10px; background: #fdf1e6; color: #c46a1c; font-size: 18px; }
.c3-who-txt { min-width: 0; }
.c3-line { display: flex; flex-wrap: wrap; align-items: baseline; gap: 4px 12px; }
.c3-no { font-size: 12px; font-weight: 700; color: var(--fin-orange-700, #b5651d); }
.c3-name { font-size: 18px; color: var(--fin-ink); }
.c3-ar { font-size: 13px; color: var(--fin-muted); }
.c3-sub { align-items: center; margin-block-start: 3px; font-size: 12.5px; color: var(--fin-muted); }
.c3-sub b { color: var(--fin-ink); }
.c3-role { display: inline-flex; align-items: center; gap: 5px; padding: 2px 9px; border-radius: 999px; background: #f2f4f6; color: #34495a; font-size: 11.5px; font-weight: 600; }
.c3-dot { width: 6px; height: 6px; border-radius: 50%; background: #8a98a3; }
.c3-dot--ok { background: #2e9e6a; } .c3-dot--warn { background: #d39b2a; } .c3-dot--bad { background: #c0392b; }
.c3-badges { flex: none; display: flex; gap: 8px; }
.c3-badge { appearance: none; display: flex; flex-direction: column; gap: 1px; min-width: 150px; max-width: 210px; text-align: start; font: inherit; padding: 7px 12px;
  cursor: pointer; border: 1px solid var(--fin-line); border-inline-start: 3px solid var(--c3-c); border-radius: 10px; background: #fff; }
.c3-badge:hover { box-shadow: var(--fin-shadow); }
.c3-badge-k { display: flex; align-items: center; gap: 5px; font-size: 11.5px; font-weight: 600; color: var(--fin-muted); }
.c3-badge b { font-size: 13.5px; color: var(--c3-c); }
.c3-badge small { overflow: hidden; white-space: nowrap; text-overflow: ellipsis; color: var(--fin-muted); font-size: 11px; }
.c3-badge--ok { --c3-c: var(--fin-ok); }
.c3-badge--warn { --c3-c: var(--fin-warn); }
.c3-badge--bad { --c3-c: var(--fin-bad); }
.c3-body { padding: 16px; }
@media (max-width: 1100px) { .c3-head { flex-wrap: wrap; } .c3-badges { width: 100%; } .c3-badge { flex: 1; max-width: none; } }
@media (max-width: 640px) { .c3-badges { flex-direction: column; } }
</style>
