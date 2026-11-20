<template>
  <!-- Screening home (federation): the Federation Final List at a glance and the module's pages. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-shield"></span></span>
        <div>
          <h1>{{ tr('Screening') }}</h1>
          <p>{{ tr('The Federation Final List: letters, decisions approved through the workflow, and member company screening.') }}</p>
        </div>
      </div>
    </div>

    <!-- the Final List at a glance: one status band, set apart from the pages below -->
    <section v-if="registry" class="scr-band" :aria-label="tr('Final List status')">
      <div class="scr-band-head">
        <span class="scr-band-pulse" aria-hidden="true"></span>
        <b>{{ tr('Final List status') }}</b>
        <small v-if="registry.publishedAt">{{ tr('Last published') }} {{ formatDateTime(registry.publishedAt) }}</small>
        <small v-else>{{ tr('Nothing published yet') }}</small>
      </div>
      <div class="scr-band-stats">
        <RouterLink :to="{ name: 'scr-registry' }" class="scr-stat">
          <span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-tag"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Final List version') }}</em><b>V{{ registry.versionNo }}</b></span>
        </RouterLink>
        <RouterLink :to="{ name: 'scr-registry' }" class="scr-stat">
          <span class="scr-stat-ic scr-stat-ic--orange" aria-hidden="true"><span class="fa fa-user-times"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Active entries') }}</em><b>{{ registry.activeCount }}</b>
            <small>{{ registry.suspendedCount }} {{ tr('suspended') }} · {{ registry.liftedCount }} {{ tr('lifted') }}</small></span>
        </RouterLink>
        <RouterLink :to="{ name: 'scr-decisions', query: { view: 'STARTED', user: '' } }" class="scr-stat">
          <span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-hourglass-half"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Decisions in the workflow') }}</em><b>{{ registry.pendingCount }}</b>
            <small>{{ tr('In review or awaiting approval') }}</small></span>
        </RouterLink>
      </div>
    </section>

    <!-- a user with no Screening permission sees why, not an empty page -->
    <div v-if="!cards.length" class="fx-card scr-no-access">
      <span class="fa fa-lock" aria-hidden="true"></span>
      <div>
        <b>{{ tr('You have no access to Screening yet') }}</b>
        <p>{{ tr('Your roles do not include any Screening permission. Ask your administrator to give you the role for your work:') }}</p>
        <ul>
          <li><b>OPERATIONS_OFFICER</b> · {{ tr('letters, parties, decisions and Run') }}</li>
          <li><b>LEGAL_OFFICER</b> · {{ tr('reviewing decisions') }}</li>
          <li><b>RISK_OFFICER</b> · {{ tr('approving and publishing decisions') }}</li>
          <li><b>MEMBER_COMPANY_ADMIN / MEMBER_COMPANY_USER</b> · {{ tr('member company screening') }}</li>
        </ul>
        <small>{{ tr('After the role is given, sign out and sign in again.') }}</small>
      </div>
    </div>

    <h2 v-if="cards.length" class="scr-section-title">{{ tr('Screening services') }}</h2>
    <div class="fc-quick fc-quick--3">
      <RouterLink v-for="c in cards" :key="c.name" :to="{ name: c.name }" class="fx-card fc-quick-card" :class="{ 'fc-quick-card--calc': c.orange }">
        <span class="fc-quick-ic" aria-hidden="true"><span :class="['fa', c.icon]"></span></span>
        <span class="fc-quick-copy"><b>{{ tr(c.title) }}</b><small>{{ tr(c.descr) }}</small></span>
        <span class="fa fa-angle-right" aria-hidden="true"></span>
      </RouterLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { onMounted, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { formatDateTime } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import { errorText, scrApi, type ScrRegistry } from '../api/scr.api'

const ALL = [
  { name: 'scr-registry', icon: 'fa-list-alt', title: 'Federation Final List', descr: 'Every listed person and company, active, suspended and lifted.', perm: 'SCR.DECISION.VIEW', orange: true },
  { name: 'scr-decisions', icon: 'fa-gavel', title: 'Decisions', descr: 'Enter decisions, run their approval workflow and follow each one\'s step.', perm: 'SCR.DECISION.VIEW', orange: false },
  { name: 'scr-letters', icon: 'fa-envelope-o', title: 'Incoming letters', descr: 'Register letters and attach their scans.', perm: 'SCR.DECISION.VIEW', orange: false },
  { name: 'scr-parties', icon: 'fa-users', title: 'Parties', descr: 'The people and companies decisions are about, with their other names.', perm: 'SCR.DECISION.VIEW', orange: false },
  { name: 'scr-companies', icon: 'fa-building-o', title: 'Member companies', descr: 'Member companies and their user accounts.', perm: 'SCR.COMPANY.ADMIN', orange: false },
  { name: 'scr-inquiries', icon: 'fa-search', title: 'Inquiry audit', descr: 'Every screening by every member company, with its result.', perm: 'SCR.INQUIRY.AUDIT', orange: false },
  { name: 'scr-portal', icon: 'fa-shield', title: 'Screening Portal', descr: 'Screen a person or company against the Final List.', perm: 'SCR.INQUIRY.CREATE', orange: true }
]
const cards = ALL.filter((c) => hasPermission(c.perm))
const registry = ref<ScrRegistry | null>(null)

onMounted(async () => {
  if (!hasPermission('SCR.DECISION.VIEW')) return
  try {
    registry.value = await scrApi.registry()
  } catch (err) {
    showError(errorText(err))
  }
})
</script>
