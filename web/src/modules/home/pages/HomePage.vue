<template>
  <!-- App 100 (FINOVA launcher) page 1 "Home", region "Applications": the same markup and CSS (the page's inline CSS),
       with each card opening this app's module instead of an APEX application. -->
  <div class="fm">
    <section class="fm-hero">
      <div class="fm-hero-l">
        <span class="fm-eyebrow"><i></i>{{ tr('CAPITRA · Digital Finance Platform') }}</span>
        <h1><span>{{ tr(greeting) }}</span>{{ tr(',') }} <em>{{ displayName }}</em></h1>
        <p>{{ tr('Choose an application to continue. You are signed in once for all CAPITRA applications.') }}</p>
        <div class="fm-cta">
          <router-link class="fm-btn fm-btn--inbox" to="/workflow/inbox">
            <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M22 12h-6l-2 3h-4l-2-3H2" /><path d="M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z" /></svg>
            <span>{{ tr('Open my inbox') }}</span><em>{{ tasks === null ? 0 : tasks }}</em>
          </router-link>
        </div>
      </div>
      <div class="fm-kpis">
        <div><b>{{ live }}</b><span>{{ tr('Live applications') }}</span></div>
        <router-link class="fm-kpi-link" to="/workflow/inbox"><b>{{ tasks === null ? '–' : tasks }}</b><span>{{ tr('Workflow tasks for you') }}</span></router-link>
        <div><b>{{ today }}</b><span>{{ tr('Today') }}</span></div>
      </div>
    </section>

    <div class="fm-sec">
      <h2>{{ tr(group === 'mortgage' ? 'Mortgage applications' : 'Business applications') }}</h2>
      <router-link v-if="group" to="/" class="fm-back"><span v-html="icon(BACK, 15)"></span>{{ tr('All applications') }}</router-link>
    </div>
    <div class="fm-grid fm-grid--1">
      <component :is="app.to ? 'router-link' : 'div'" v-for="app in business" :key="app.name" :to="app.to" class="fm-card fm-card--t1"
                 :class="{ 'is-off': !app.to }" :aria-disabled="app.to ? undefined : 'true'" :style="{ '--c': app.color, '--cl': app.tint }">
        <div class="fm-top"><span class="fm-ico" v-html="icon(app.icon)"></span><span class="fm-pill" :class="app.to ? 'fm-pill--live' : 'fm-pill--soon'">{{ tr(app.to ? 'Live' : 'Coming soon') }}</span></div>
        <div class="fm-body">
          <small>{{ tr(app.short) }}</small><h3>{{ tr(app.name) }}</h3><p>{{ tr(app.descr) }}</p>
          <ul v-if="app.feats.length" class="fm-feats"><li v-for="f in app.feats" :key="f">{{ tr(f) }}</li></ul>
        </div>
        <div class="fm-foot">
          <span v-if="app.to" class="fm-go">{{ tr('Open ' + app.short) }}<span v-html="icon(ARROW, 16)"></span></span>
          <span v-else class="fm-muted">{{ tr('In preparation · available soon') }}</span>
        </div>
      </component>
    </div>

    <div v-if="!group" class="fm-sec"><h2>{{ tr('Shared services') }}</h2></div>
    <div v-if="!group" class="fm-grid fm-grid--2">
      <router-link v-for="app in tier(2)" :key="app.name" :to="app.to!" class="fm-card fm-card--t2" :style="{ '--c': app.color, '--cl': app.tint }">
        <div class="fm-top"><span class="fm-ico" v-html="icon(app.icon)"></span><span class="fm-pill fm-pill--live">{{ tr('Live') }}</span></div>
        <div class="fm-body"><small>{{ tr(app.short) }}</small><h3>{{ tr(app.name) }}</h3><p>{{ tr(app.descr) }}</p></div>
        <div v-if="app.inbox && tasks !== null" class="fm-note"><b>{{ tasks }}</b> {{ tr(tasks === 1 ? 'task waiting in your inbox' : 'tasks waiting in your inbox') }}</div>
        <div class="fm-foot"><span class="fm-go">{{ tr('Open ' + app.short) }}<span v-html="icon(ARROW, 16)"></span></span></div>
      </router-link>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { getUser } from '@/shared/auth/session'
import { language, tr } from '@/shared/layout/language'
import { workflowService } from '@/modules/wfe/services/workflowService'

type App = { tier: 1 | 2 | 'mortgage'; name: string; short: string; descr: string; to: string | null; color: string; tint: string; icon: string; feats: string[]; inbox?: boolean }

// group 'mortgage' (route /mortgage-applications) shows the three mortgage applications behind the Mortgage card
const props = defineProps<{ group?: 'mortgage' }>()

const user = getUser()
const displayName = user?.fullName?.trim() || user?.userName?.trim() || 'CAPITRA User'
const hour = new Date().getHours()
const greeting = hour < 12 ? 'Good morning' : hour < 18 ? 'Good afternoon' : 'Good evening'
const today = new Date().toLocaleDateString(language.value === 'AR' ? 'ar-EG-u-nu-latn' : undefined, { day: 'numeric', month: 'short' })

const ARROW = '<path d="M5 12h14M13 5l7 7-7 7"/>'
const BACK = '<path d="M19 12H5M11 19l-7-7 7-7"/>'
// the card icons of the APEX page (its svg() function)
const icon = (paths: string, size = 24) =>
  `<svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${paths}</svg>`

// Home shows two business cards, Mortgage and Factoring, with the FINOVA_FINANCEING home texts; Mortgage opens the APEX page's
// three mortgage cards (reg calls), each pointed at this app's module. Personal Mortgage is not built here yet.
const apps: App[] = [
  { tier: 1, name: 'Mortgage', short: 'Mortgage', descr: 'Mortgage portfolio purchase, approvals, disbursement, repayment schedules and monitoring.',
    to: '/mortgage-applications', color: '#E87722', tint: '#FFF1E6', icon: '<path d="M3 11l9-7 9 7"/><path d="M5 10v10h14V10"/><path d="M10 20v-5h4v5"/>',
    feats: ['Portfolio Acquisition', 'Subsidized Mortgage Finance', 'Personal Mortgage'] },
  { tier: 1, name: 'Factoring', short: 'Factoring', descr: 'Manage factoring applications, agreements, limits, funding and operational processing.',
    to: '/factoring', color: '#00636D', tint: '#E6F3F4',
    icon: '<path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6"/><path d="M12 18v-1M12 11v1"/><path d="M10 16.2c.4.5 1.1.8 2 .8 1.1 0 2-.6 2-1.5s-.9-1.3-2-1.5-2-.6-2-1.5.9-1.5 2-1.5c.9 0 1.6.3 2 .8"/>',
    feats: [] },
  { tier: 'mortgage', name: 'Portfolio Acquisition', short: 'Portfolio', descr: 'Evaluate, screen and acquire real-estate financing portfolios through one controlled, fully audited workflow.',
    to: '/mortgage', color: '#E87722', tint: '#FFF1E6', icon: '<path d="M3 21h18"/><path d="M5 21V7l7-4 7 4v14"/><path d="M9 21v-6h6v6"/><path d="M9 10h.01M15 10h.01"/>',
    feats: ['Due diligence & AML screening', 'Committee approval', 'Acquisition tracking'] },
  { tier: 'mortgage', name: 'Subsidized Mortgage Finance', short: 'Subsidized', descr: 'Government-subsidised home finance: eligibility, subsidy calculation and disbursement under programme rules.',
    to: '/mortgage-finance', color: '#14734F', tint: '#E7F6EF',
    icon: '<path d="M3 11l9-7 9 7"/><path d="M5 10v10h14V10"/><path d="M10 15.5c0-1 .9-1.5 2-1.5s2 .5 2 1.5-1 1.3-2 1.5-2 .5-2 1.5.9 1.5 2 1.5 2-.5 2-1.5"/><path d="M12 13v1M12 19v1"/>',
    feats: ['Eligibility checks', 'Subsidy calculation', 'Programme reporting'] },
  { tier: 'mortgage', name: 'Personal Mortgage', short: 'Mortgage', descr: 'Standard mortgage finance: calculation, offer, origination, credit, booking, servicing and collections.',
    to: null, color: '#5B5BD6', tint: '#EEEEFC', icon: '<path d="M3 11l9-7 9 7"/><path d="M5 10v10h14V10"/><path d="M9 17l2-2 2 1.5 3-3.5"/>',
    feats: ['Calculator & customer offers', 'Origination, credit & booking', 'Servicing & collections'] },
  { tier: 2, name: 'General Ledger', short: 'GL', descr: 'Segment-based chart of accounts, balanced journals and controlled period close.', to: '/gl', color: '#0E7490', tint: '#E0F2F7',
    icon: '<path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5z"/><path d="M4 19.5A2.5 2.5 0 0 0 6.5 22H20v-5"/><path d="M9 7h7M9 11h5"/>', feats: [] },
  { tier: 2, name: 'Workflow Engine', short: 'Workflow', descr: 'Design approval workflows and follow every task, SLA and decision.', to: '/workflowengine', color: '#00636D', tint: '#E6F3F4',
    icon: '<rect x="3" y="3" width="7" height="6" rx="1.5"/><rect x="14" y="15" width="7" height="6" rx="1.5"/><path d="M6.5 9v3a3 3 0 0 0 3 3h4.5"/><circle cx="18" cy="6" r="3"/>', feats: [], inbox: true },
  { tier: 2, name: 'Cheques Wallet', short: 'Wallet', descr: 'Client cheque batches from upload to custody, deposit and collection.', to: '/cheque-wallet', color: '#B4570E', tint: '#FFF1E6',
    icon: '<path d="M20 7H5a2 2 0 0 1 0-4h13v4"/><path d="M3 5v14a2 2 0 0 0 2 2h15V7H5"/><path d="M16 14h.01"/>', feats: [] },
  { tier: 2, name: 'Admin Setup', short: 'Admin', descr: 'Users, roles, modules and platform-wide settings in one place.', to: '/admin', color: '#475569', tint: '#EEF2F6',
    icon: '<circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.8l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-1.8-.3 1.7 1.7 0 0 0-1 1.5V21a2 2 0 1 1-4 0v-.1a1.7 1.7 0 0 0-1.1-1.5 1.7 1.7 0 0 0-1.8.3l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0 .3-1.8 1.7 1.7 0 0 0-1.5-1H3a2 2 0 1 1 0-4h.1a1.7 1.7 0 0 0 1.5-1.1 1.7 1.7 0 0 0-.3-1.8l-.1-.1a2 2 0 1 1 2.8-2.8l.1.1a1.7 1.7 0 0 0 1.8.3H9a1.7 1.7 0 0 0 1-1.5V3a2 2 0 1 1 4 0v.1a1.7 1.7 0 0 0 1 1.5 1.7 1.7 0 0 0 1.8-.3l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.8V9a1.7 1.7 0 0 0 1.5 1H21a2 2 0 1 1 0 4h-.1a1.7 1.7 0 0 0-1.5 1z"/>', feats: [] }
]
const tier = (n: App['tier']) => apps.filter((a) => a.tier === n)
const business = computed(() => tier(props.group ?? 1))
// the live applications themselves: the Mortgage card only opens its three cards
const live = computed(() => apps.filter((a) => a.to && a.name !== 'Mortgage').length)

const tasks = ref<number | null>(null)
onMounted(async () => {
  try {
    tasks.value = (await workflowService.getMyInbox()).length
  } catch {
    tasks.value = null
  }
})
</script>

<style scoped>
.fm-back { order: 2; display: inline-flex; align-items: center; gap: 6px; margin: 0; line-height: 1; font-size: 13px; font-weight: 600; color: #00636D; text-decoration: none; white-space: nowrap; }
.fm-back span { display: inline-flex; }
/* the same smaller greeting as the Workflow Engine home: it carries the full name */
.fm-hero h1 { font-size: clamp(20px, 1.9vw, 26px); }
</style>
