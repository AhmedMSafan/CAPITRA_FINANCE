<template>
  <!-- Communication Center home: which mail server the system uses now, and the module's pages. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-paper-plane"></span></span>
        <div>
          <h1>{{ tr('Communication Center') }}</h1>
          <p>{{ tr('The mail servers every e-mail of CAPITRA goes through, and the log of every e-mail sent.') }}</p>
        </div>
      </div>
    </div>

    <section v-if="status" class="scr-band" :aria-label="tr('Mail server in use')">
      <div class="scr-band-head">
        <span class="scr-band-pulse" :class="{ 'cmc-pulse-off': status.sourceCode === 'NONE' }" aria-hidden="true"></span>
        <b>{{ tr('Mail server in use') }}</b>
        <small v-if="status.lastTestAt">{{ tr('Last test') }} {{ formatDateTime(status.lastTestAt) }} · {{ tr(LABELS[status.lastTestStatusCode ?? ''] ?? '') }}</small>
      </div>
      <div class="scr-band-stats">
        <RouterLink :to="{ name: 'cmc-mail-servers' }" class="scr-stat">
          <span class="scr-stat-ic" :class="{ 'scr-stat-ic--orange': status.sourceCode !== 'COMMUNICATION_CENTER' }" aria-hidden="true"><span class="fa fa-server"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Server') }}</em>
            <b class="cmc-band-text">{{ status.sourceCode === 'NONE' ? tr('None') : status.serverName ?? tr('Application settings') }}</b>
            <small dir="ltr">{{ status.hostName ?? tr('E-mails are not sent: add a mail server.') }}</small></span>
        </RouterLink>
        <RouterLink :to="{ name: 'cmc-mail-servers' }" class="scr-stat">
          <span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-at"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Sender') }}</em><b class="cmc-band-text" dir="ltr">{{ status.fromAddress ?? '-' }}</b>
            <small>{{ tr(LABELS[status.sourceCode] ?? status.sourceCode) }}</small></span>
        </RouterLink>
        <RouterLink v-if="canLog" :to="{ name: 'cmc-email-log', query: { statusCode: 'FAILED' } }" class="scr-stat">
          <span class="scr-stat-ic scr-stat-ic--orange" aria-hidden="true"><span class="fa fa-exclamation-triangle"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Failed in the last 7 days') }}</em><b>{{ failed ?? '-' }}</b>
            <small>{{ sent ?? '-' }} {{ tr('sent') }}</small></span>
        </RouterLink>
      </div>
    </section>

    <div v-if="!cards.length" class="fx-card scr-no-access">
      <span class="fa fa-lock" aria-hidden="true"></span>
      <div>
        <b>{{ tr('You have no access to the Communication Center') }}</b>
        <p>{{ tr('Ask your administrator for the permission CMC.MAIL_SERVER.EDIT or CMC.EMAIL_LOG.VIEW, then sign out and sign in again.') }}</p>
      </div>
    </div>

    <h2 v-if="cards.length" class="scr-section-title">{{ tr('Communication services') }}</h2>
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
import '@/modules/scr/components/scr.css'
import './cmc.css'
import { onMounted, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDateTime } from '@/modules/scr/utils/dates'
import { LABELS, cmcApi, errorText, type CmcMailStatus } from '../api/cmc.api'

const ALL = [
  { name: 'cmc-mail-servers', icon: 'fa-server', title: 'Mail servers', descr: 'Add the outgoing mail server, send a test e-mail and choose the one in use.', perm: 'CMC.MAIL_SERVER.EDIT', orange: true },
  { name: 'cmc-email-log', icon: 'fa-list-ul', title: 'Sent e-mails', descr: 'Every e-mail the system sent or tried to send, with the reason when it failed.', perm: 'CMC.EMAIL_LOG.VIEW', orange: false }
]
const cards = ALL.filter((c) => hasPermission(c.perm))
const canLog = hasPermission('CMC.EMAIL_LOG.VIEW')
const status = ref<CmcMailStatus | null>(null)
const failed = ref<number | null>(null)
const sent = ref<number | null>(null)

onMounted(async () => {
  try {
    if (hasPermission('CMC.MAIL_SERVER.EDIT')) status.value = await cmcApi.status()
    if (canLog) {
      const from = new Date(Date.now() - 7 * 86_400_000).toISOString().slice(0, 10)
      const rows = await cmcApi.emailLog({ fromDate: from, take: 2000 })
      failed.value = rows.filter((r) => r.statusCode === 'FAILED' || r.statusCode === 'NOT_CONFIGURED').length
      sent.value = rows.filter((r) => r.statusCode === 'SENT').length
    }
  } catch (err) {
    showError(errorText(err))
  }
})
</script>
