<template>
  <!-- Sent e-mails (Communication Center): every e-mail the system sent or tried to send, newest first. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'cmc-home' }">{{ tr('Communication Center') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Sent e-mails') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-list-ul"></span></span>
          <div>
            <h1>{{ tr('Sent e-mails') }}</h1>
            <p>{{ tr('Every e-mail the system sent or tried to send, with the reason when it failed. No e-mail ever contains a password.') }}</p>
          </div>
        </div>
      </div>
    </div>

    <div class="fx-card fx-card-pad fc-filters">
      <label class="fc-field"><span>{{ tr('Search') }}</span><input v-model="q.search" class="fx-search" :placeholder="tr('Address or subject')" @keyup.enter="load"></label>
      <label class="fc-field"><span>{{ tr('Status') }}</span>
        <select v-model="q.statusCode" class="fx-select" @change="load"><option value="">{{ tr('All') }}</option>
          <option v-for="s in ['SENT', 'FAILED', 'NOT_CONFIGURED', 'NO_ADDRESS', 'LINK_COPIED']" :key="s" :value="s">{{ tr(LABELS[s] ?? s) }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Kind') }}</span>
        <select v-model="q.kindCode" class="fx-select" @change="load"><option value="">{{ tr('All') }}</option>
          <option v-for="k in ['ACTIVATION', 'PASSWORD_RESET', 'TEST']" :key="k" :value="k">{{ tr(LABELS[k] ?? k) }}</option></select></label>
      <label class="fc-field"><span>{{ tr('From') }}</span><input v-model="q.fromDate" class="fx-search" type="date" @change="load"></label>
      <label class="fc-field"><span>{{ tr('To') }}</span><input v-model="q.toDate" class="fx-search" type="date" @change="load"></label>
    </div>

    <div class="fx-card">
      <div class="fc-card-head"><h2>{{ tr('Sent e-mails') }} ({{ rows.length }})</h2>
        <button type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="load"><span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-refresh']" aria-hidden="true"></span>{{ tr('Refresh') }}</button></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('When') }}</th><th>{{ tr('To') }}</th><th>{{ tr('Kind') }}</th><th>{{ tr('Subject') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Server') }}</th><th>{{ tr('By') }}</th></tr></thead>
          <tbody>
            <tr v-for="r in rows" :key="r.emailLogId">
              <td>{{ formatDateTime(r.attemptedAt) }}</td>
              <td dir="ltr">{{ r.toAddress || '-' }}<span v-if="r.userName" class="fx-cell-sub">{{ r.userName }}</span></td>
              <td>{{ tr(LABELS[r.kindCode] ?? r.kindCode) }}</td>
              <td dir="auto">{{ r.subject }}</td>
              <td><span class="fx-chip" :class="STATUS_CHIP[r.statusCode]">{{ tr(LABELS[r.statusCode] ?? r.statusCode) }}</span>
                <small v-if="r.errorText" class="cmc-log-error" dir="ltr">{{ r.errorText }}</small></td>
              <td>{{ r.serverName ?? (r.statusCode === 'SENT' || r.statusCode === 'FAILED' ? tr('Application settings') : '-') }}</td>
              <td dir="ltr">{{ r.createdBy }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="!rows.length && !busy" class="fx-empty"><span class="fa fa-envelope-o" aria-hidden="true"></span>{{ tr('No e-mails for this search.') }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '@/modules/scr/components/scr.css'
import './cmc.css'
import { onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDateTime } from '@/modules/scr/utils/dates'
import { LABELS, STATUS_CHIP, cmcApi, errorText, type CmcEmailLog } from '../api/cmc.api'

const route = useRoute()
const q = reactive({ search: '', statusCode: typeof route.query.statusCode === 'string' ? route.query.statusCode : '', kindCode: '', fromDate: '', toDate: '' })
const rows = ref<CmcEmailLog[]>([])
const busy = ref(false)

async function load() {
  busy.value = true
  try {
    rows.value = await cmcApi.emailLog({ ...q, search: q.search.trim() })
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

onMounted(load)
</script>
