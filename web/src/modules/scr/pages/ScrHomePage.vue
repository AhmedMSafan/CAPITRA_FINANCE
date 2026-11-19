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

    <div v-if="registry" class="fc-indicators scr-indicators">
      <div class="fx-card fc-indicator"><b>V{{ registry.versionNo }}</b><span>{{ tr('Final List version') }}</span>
        <small>{{ registry.publishedAt ? `${tr('Last published')} ${formatDateTime(registry.publishedAt)}` : tr('Nothing published yet') }}</small></div>
      <div class="fx-card fc-indicator fc-indicator--orange"><b>{{ registry.activeCount }}</b><span>{{ tr('Active entries') }}</span>
        <small>{{ registry.suspendedCount }} {{ tr('suspended') }} · {{ registry.liftedCount }} {{ tr('lifted') }}</small></div>
      <RouterLink :to="{ name: 'scr-decisions', query: { view: 'STARTED' } }" class="fx-card fc-indicator scr-indicator-link"><b>{{ registry.pendingCount }}</b><span>{{ tr('Decisions in the workflow') }}</span>
        <small>{{ tr('In review or awaiting approval') }}</small></RouterLink>
    </div>

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
