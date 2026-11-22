<template>
  <!-- A member company administrator's view of the company's sign-ins: every attempt by its users, and per session the inquiries made. -->
  <section class="fx-card">
    <div class="fc-card-head"><h2>{{ tr('Our sign-ins') }}</h2>
      <span v-if="page" class="fc-muted">{{ page.summary.successful }} {{ tr('successful') }} · {{ page.summary.refused }} {{ tr('refused') }} · {{ page.summary.inquiries }} {{ tr('inquiries') }}</span></div>
    <div class="fc-grid fc-grid--3 scr-portal-filters">
      <label class="fc-field"><span>{{ tr('From') }}</span><input v-model="q.fromDate" class="fx-search" type="date" @change="load"></label>
      <label class="fc-field"><span>{{ tr('To') }}</span><input v-model="q.toDate" class="fx-search" type="date" @change="load"></label>
      <label class="fc-field"><span>{{ tr('Result') }}</span>
        <select v-model="q.resultCode" class="fx-select" @change="load"><option value="">{{ tr('All') }}</option><option value="SUCCESS">{{ tr(LABELS.SUCCESS ?? 'Signed in') }}</option><option value="REFUSED">{{ tr('Refused') }}</option></select></label>
    </div>
    <div class="fx-table-wrap">
      <table class="fx-table">
        <thead><tr><th>{{ tr('When') }}</th><th>{{ tr('User') }}</th><th>{{ tr('Result') }}</th><th>{{ tr('Device') }}</th><th>{{ tr('Inquiries') }}</th></tr></thead>
        <tbody>
          <template v-for="s in page?.rows ?? []" :key="s.signInId">
            <tr ":class="{ 'scr-portal-click': s.inquiryCount > 0 }" @click="s.inquiryCount > 0 && toggle(s.signInId)">
              <td>{{ formatDateTime(s.attemptedAt) }}</td>
              <td><b dir="ltr">{{ s.userName }}</b><span v-if="s.fullName" class="fx-cell-sub">{{ s.fullName }}</span></td>
              <td><span class="fx-chip" :class="s.resultCode === 'SUCCESS' ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(LABELS[s.resultCode] ?? s.resultCode) }}</span>
                <span v-if="s.failureText && s.resultCode !== 'SUCCESS'" class="fx-cell-sub">{{ tr(s.failureText) }}</span></td>
              <td>{{ s.device ?? '-' }}<span v-if="s.ipAddress" class="fx-cell-sub" dir="ltr">{{ s.ipAddress }}</span></td>
              <td>{{ s.inquiryCount }}<span v-if="s.inquiryCount" class="fa fa-chevron-down scr-portal-more" aria-hidden="true"></span></td>
            </tr>
            <tr v-if="open[s.signInId]"><td colspan="5">
              <ul class="scr-portal-session">
                <li v-for="i in open[s.signInId]" :key="i.inquiryId"><b dir="ltr">{{ i.inquiryRef }}</b> · {{ i.subjectName }} ·
                  <span class="fx-chip" :class="CHIP[i.resultCode]">{{ tr(LABELS[i.resultCode] ?? i.resultCode) }}</span>
                  <small v-if="i.durationMs !== null"> · {{ i.durationMs }} ms</small></li>
              </ul>
            </td></tr>
          </template>
        </tbody>
      </table>
    </div>
    <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
    <div v-else-if="!page?.rows.length" class="fx-empty"><span class="fa fa-sign-in" aria-hidden="true"></span>{{ tr('No sign-ins in these days.') }}</div>
  </section>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDateTime } from '../utils/dates'
import { CHIP, LABELS, errorText, scrApi, type ScrSessionInquiry, type ScrSignInPage } from '../api/scr.api'

const today = new Intl.DateTimeFormat('en-CA', { timeZone: 'Africa/Cairo' }).format(new Date())
const monthAgo = new Intl.DateTimeFormat('en-CA', { timeZone: 'Africa/Cairo' }).format(new Date(Date.now() - 30 * 86_400_000))
const q = reactive({ fromDate: monthAgo, toDate: today, resultCode: '' })
const page = ref<ScrSignInPage | null>(null)
const loading = ref(false)
const open = ref<Record<number, ScrSessionInquiry[]>>({})

async function toggle(id: number) {
  if (open.value[id]) {
    const rest = { ...open.value }
    delete rest[id]
    open.value = rest
    return
  }
  try {
    open.value = { ...open.value, [id]: (await scrApi.portalSession(id)).inquiries }
  } catch (err) {
    showError(errorText(err))
  }
}

async function load() {
  loading.value = true
  try {
    page.value = await scrApi.portalSignIns({ ...q })
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>
