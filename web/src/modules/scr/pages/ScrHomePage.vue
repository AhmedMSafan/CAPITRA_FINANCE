<template>
  <!-- Screening home (federation): the Final List at a glance and the module's pages. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Screening') }}</b>
        </nav>
        <h1>{{ tr('Federation Final List') }}</h1>
      </div>
    </div>

    <div v-if="registry" class="scr-tiles">
      <div><small>{{ tr('Version') }}</small><b>V{{ registry.versionNo }}</b></div>
      <div><small>{{ tr('Active') }}</small><b>{{ registry.activeCount }}</b></div>
      <div><small>{{ tr('Suspended') }}</small><b>{{ registry.suspendedCount }}</b></div>
      <div><small>{{ tr('Lifted') }}</small><b>{{ registry.liftedCount }}</b></div>
      <div><small>{{ tr('Pending approval') }}</small><b>{{ registry.pendingCount }}</b></div>
    </div>

    <div class="scr-cards">
      <RouterLink v-for="c in cards" :key="c.name" :to="{ name: c.name }" class="scr-card">
        <b><span :class="['fa', c.icon]" aria-hidden="true"></span> {{ tr(c.title) }}</b>
        <small>{{ tr(c.descr) }}</small>
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
import { tr } from '@/shared/layout/language'
import { errorText, scrApi, type ScrRegistry } from '../api/scr.api'

const ALL = [
  { name: 'scr-registry', icon: 'fa-list', title: 'Federation Final List', descr: 'Every listed person and company, active, suspended and lifted.', perm: 'SCR.DECISION.VIEW' },
  { name: 'scr-decisions', icon: 'fa-gavel', title: 'Decisions', descr: 'Enter decisions from letters and send them through review and approval.', perm: 'SCR.DECISION.VIEW' },
  { name: 'scr-letters', icon: 'fa-envelope-o', title: 'Incoming letters', descr: 'Register letters and attach their scans.', perm: 'SCR.DECISION.VIEW' },
  { name: 'scr-parties', icon: 'fa-users', title: 'Parties', descr: 'The people and companies decisions are about, with their other names.', perm: 'SCR.DECISION.VIEW' },
  { name: 'scr-companies', icon: 'fa-building', title: 'Member companies', descr: 'Member companies and their user accounts.', perm: 'SCR.COMPANY.ADMIN' },
  { name: 'scr-inquiries', icon: 'fa-search', title: 'Inquiry audit', descr: 'Every screening by every member company, with its result.', perm: 'SCR.INQUIRY.AUDIT' },
  { name: 'scr-portal', icon: 'fa-shield', title: 'Screening Portal', descr: 'Screen a person or company against the Final List.', perm: 'SCR.INQUIRY.CREATE' }
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
