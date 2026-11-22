<template>
  <!--
    Sign-ins and activity: every sign-in attempt (who, when, from where, the result or why it was refused), and for each session the
    inquiries made in it: when received, when the result was sent back, and the result.
  -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Sign-ins and activity') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-history"></span></span>
          <div>
            <h1>{{ tr('Sign-ins and activity') }}</h1>
            <p>{{ tr('Every sign-in attempt, and in each session the inquiries received and the results sent back.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openScreeningManual(12)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
      </div>
    </div>

    <div class="fx-card fx-card-pad fc-filters scr-log-filters">
      <label class="fc-field"><span>{{ tr('From') }}</span><input v-model="q.fromDate" class="fx-search" type="date" @change="load"></label>
      <label class="fc-field"><span>{{ tr('To') }}</span><input v-model="q.toDate" class="fx-search" type="date" @change="load"></label>
      <label class="fc-field"><span>{{ tr('Company') }}</span>
        <select v-model="q.memberCompanyId" class="fx-select" @change="load"><option :value="undefined">{{ tr('Everyone') }}</option><option :value="0">{{ tr('Federation users') }}</option>
          <option v-for="c in companies" :key="c.memberCompanyId" :value="c.memberCompanyId">{{ c.companyCode }} · {{ c.nameAr }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Result') }}</span>
        <select v-model="q.resultCode" class="fx-select" @change="load"><option value="">{{ tr('All') }}</option><option value="SUCCESS">{{ tr('Signed in') }}</option><option value="REFUSED">{{ tr('Refused') }}</option>
          <option v-for="r in REFUSALS" :key="r" :value="r">— {{ tr(LABELS[r] ?? r) }}</option>
          <option value="CODE_TIMEOUT">{{ tr('Code not entered') }}</option><option value="AWAITING_CODE">{{ tr('Waiting for the code') }}</option><option value="TWO_FACTOR_RESET">{{ tr('Two-factor reset') }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Search') }}</span><input v-model="q.search" class="fx-search" :placeholder="tr('User name or IP address')" @keyup.enter="load"></label>
    </div>

    <section v-if="page" class="scr-band" :aria-label="tr('Summary')">
      <div class="scr-band-stats scr-band-stats--5">
        <div class="scr-stat"><span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-sign-in"></span></span><span class="scr-stat-copy"><em>{{ tr('Attempts') }}</em><b>{{ page.summary.attempts }}</b></span></div>
        <div class="scr-stat"><span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-check"></span></span><span class="scr-stat-copy"><em>{{ tr('Signed in') }}</em><b>{{ page.summary.successful }}</b></span></div>
        <div class="scr-stat"><span class="scr-stat-ic scr-stat-ic--orange" aria-hidden="true"><span class="fa fa-ban"></span></span><span class="scr-stat-copy"><em>{{ tr('Refused') }}</em><b>{{ page.summary.refused }}</b></span></div>
        <div class="scr-stat"><span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-users"></span></span><span class="scr-stat-copy"><em>{{ tr('Users') }}</em><b>{{ page.summary.users }}</b></span></div>
        <div class="scr-stat"><span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-search"></span></span><span class="scr-stat-copy"><em>{{ tr('Inquiries') }}</em><b>{{ page.summary.inquiries }}</b></span></div>
      </div>
    </section>

    <div class="fx-card">
      <div class="fc-card-head"><h2>{{ tr('Sign-ins') }} ({{ page?.rows.length ?? 0 }})</h2>
        <button type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="load"><span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-refresh']" aria-hidden="true"></span>{{ tr('Refresh') }}</button></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr><th>{{ tr('When') }}</th><th>{{ tr('User') }}</th><th>{{ tr('Company') }}</th><th>{{ tr('Result') }}</th><th>{{ tr('From') }}</th>
              <th>{{ tr('Session') }}</th><th class="fx-r">{{ tr('Inquiries') }}</th><th class="fx-r">{{ tr('Actions') }}</th></tr>
          </thead>
          <tbody>
            <tr v-for="s in page?.rows ?? []" :key="s.signInId" :class="{ 'scr-log-refused': s.resultCode !== 'SUCCESS' }">
              <td>{{ formatDateTime(s.attemptedAt) }}</td>
              <td><b dir="ltr">{{ s.userName }}</b><span v-if="s.fullName" class="fx-cell-sub">{{ s.fullName }}</span></td>
              <td><template v-if="s.companyCode"><b dir="ltr">{{ s.companyCode }}</b><span class="fx-cell-sub">{{ s.companyNameAr }}</span></template>
                <span v-else class="fc-muted">{{ s.userName && s.resultCode !== 'UNKNOWN_USER' ? tr('Federation') : '-' }}</span></td>
              <td><span class="fx-chip" :class="resultChip(s.resultCode)">{{ tr(LABELS[s.resultCode] ?? s.resultCode) }}</span>
                <span v-if="s.secondStepCode && s.resultCode === 'SUCCESS'" class="fx-cell-sub"><span class="fa fa-shield" aria-hidden="true"></span> {{ tr(LABELS[s.secondStepCode] ?? s.secondStepCode) }}</span>
                <span v-else-if="s.failureText && (s.resultCode === 'COMPANY_BLOCKED' || s.resultCode === 'TWO_FACTOR_RESET')" class="fx-cell-sub" dir="auto">{{ tr(s.failureText) }}</span></td>
              <td><span dir="ltr">{{ s.ipAddress ?? '-' }}</span><span class="fx-cell-sub" :title="s.userAgent ?? ''">{{ s.device ?? '' }}</span></td>
              <td><template v-if="s.sessionStateCode"><span class="fx-chip" :class="SESSION_CHIP[s.sessionStateCode]">{{ tr(LABELS[s.sessionStateCode] ?? s.sessionStateCode) }}</span>
                <span class="fx-cell-sub">{{ s.signedOutAt ? `${tr('Left')} ${formatTime(s.signedOutAt)}` : s.lastActivityAt ? `${tr('Last inquiry')} ${formatTime(s.lastActivityAt)}` : '' }}</span></template></td>
              <td class="fx-r"><b>{{ s.sessionId ? s.inquiryCount : '' }}</b></td>
              <td class="fx-r"><button v-if="s.sessionId" type="button" class="fx-btn fx-btn--sm" @click="open(s)"><span class="fa fa-list-alt" aria-hidden="true"></span>{{ tr('Activity') }}</button></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="page && !page.rows.length" class="fx-empty"><span class="fa fa-history" aria-hidden="true"></span>{{ tr('No sign-ins for this search.') }}</div>
    </div>

    <!-- one session: who, from where, and every inquiry received and answered in it -->
    <ScrDialog v-if="session" :title="tr('Session activity')" :subtitle="`${session.signIn.userName} · ${formatDateTime(session.signIn.attemptedAt)}`" icon="fa-list-alt" @close="session = null">
      <div class="fc-grid fc-grid--3 scr-session-facts">
        <div><span>{{ tr('User') }}</span><b dir="ltr">{{ session.signIn.userName }}</b><small>{{ session.signIn.fullName }}</small></div>
        <div><span>{{ tr('Company') }}</span><b>{{ session.signIn.companyNameAr ?? tr('Federation') }}</b><small dir="ltr">{{ session.signIn.companyCode }}</small></div>
        <div><span>{{ tr('From') }}</span><b dir="ltr">{{ session.signIn.ipAddress ?? '-' }}</b><small>{{ session.signIn.device }}</small></div>
        <div><span>{{ tr('Signed in') }}</span><b>{{ formatDateTime(session.signIn.attemptedAt) }}</b></div>
        <div><span>{{ tr('Session') }}</span><b>{{ tr(LABELS[session.signIn.sessionStateCode ?? ''] ?? '-') }}</b>
          <small>{{ session.signIn.signedOutAt ? `${tr('Left')} ${formatDateTime(session.signIn.signedOutAt)}` : session.signIn.expiresAt ? `${tr('Ends')} ${formatDateTime(session.signIn.expiresAt)}` : '' }}</small></div>
        <div><span>{{ tr('Inquiries') }}</span><b>{{ session.inquiries.length }}</b><small v-if="avgMs !== null">{{ tr('average answer') }} <bdi dir="ltr">{{ avgMs }} ms</bdi></small></div>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Inquiry') }}</th><th>{{ tr('Received') }}</th><th>{{ tr('Result sent back') }}</th><th class="fx-r">{{ tr('Took') }}</th><th>{{ tr('Subject') }}</th><th>{{ tr('Purpose') }}</th><th>{{ tr('Result') }}</th></tr></thead>
          <tbody>
            <tr v-for="i in session.inquiries" :key="i.inquiryId">
              <td dir="ltr"><b>{{ i.inquiryRef }}</b></td>
              <td dir="ltr">{{ i.receivedAt ? formatPrecise(i.receivedAt) : '-' }}</td>
              <td dir="ltr">{{ i.respondedAt ? formatPrecise(i.respondedAt) : '-' }}</td>
              <td class="fx-r" dir="ltr">{{ i.durationMs !== null ? `${i.durationMs} ms` : '-' }}</td>
              <td><b>{{ i.subjectName }}</b><span class="fx-cell-sub">{{ tr(LABELS[i.identityTypeCode] ?? i.identityTypeCode) }} · <bdi dir="ltr">{{ i.identityNo }}</bdi></span></td>
              <td>{{ tr(LABELS[i.purposeCode] ?? i.purposeCode) }}</td>
              <td><span class="fx-chip" :class="RESULT_CHIP[i.resultCode]">{{ tr(LABELS[i.resultCode] ?? i.resultCode) }}</span><span v-if="i.hitCount" class="fx-cell-sub">{{ i.hitCount }} {{ tr('matches') }}</span></td>
            </tr>
            <tr v-if="!session.inquiries.length"><td colspan="7" class="fx-empty-cell">{{ tr('No inquiries in this session.') }}</td></tr>
          </tbody>
        </table>
      </div>
      <template #actions>
        <button type="button" class="fx-btn fx-btn--primary" @click="session = null">{{ tr('Close') }}</button>
      </template>
    </ScrDialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import ScrDialog from '../components/ScrDialog.vue'
import { formatDateTime } from '../utils/dates'
import { LABELS, errorText, scrApi, type ScrCompany, type ScrSessionInquiry, type ScrSignIn, type ScrSignInPage } from '../api/scr.api'
import { openScreeningManual } from '@/modules/scr/utils/manual'

const REFUSALS = ['BAD_PASSWORD', 'BAD_CODE', 'UNKNOWN_USER', 'INACTIVE', 'LOCKED', 'COMPANY_BLOCKED', 'IP_BLOCKED']
/** Success, refusal, or the in-between ones: password right but no code (warn), two-factor reset by an administrator (plain). */
const resultChip = (code: string) => code === 'SUCCESS' ? 'fx-chip--ok' : code === 'AWAITING_CODE' || code === 'CODE_TIMEOUT' ? 'fx-chip--warn' : code === 'TWO_FACTOR_RESET' ? 'fx-chip--plain' : 'fx-chip--bad'
const SESSION_CHIP: Record<string, string> = { OPEN: 'fx-chip--ok', SIGNED_OUT: 'fx-chip--plain', EXPIRED: 'fx-chip--plain' }
const RESULT_CHIP: Record<string, string> = { FOUND: 'fx-chip--warn', NOT_FOUND: 'fx-chip--teal', NO_MATCH: 'fx-chip--teal', POSSIBLE_MATCH: 'fx-chip--warn', MATCH: 'fx-chip--warn', PREVIOUSLY_LISTED: 'fx-chip--warn' }

const iso = (d: Date) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
const q = reactive<{ fromDate: string; toDate: string; memberCompanyId?: number; resultCode: string; search: string }>({
  fromDate: iso(new Date(Date.now() - 6 * 86_400_000)), toDate: iso(new Date()), memberCompanyId: undefined, resultCode: '', search: ''
})
const page = ref<ScrSignInPage | null>(null)
const companies = ref<ScrCompany[]>([])
const busy = ref(false)
const session = ref<{ signIn: ScrSignIn; inquiries: ScrSessionInquiry[] } | null>(null)
const avgMs = computed(() => {
  const d = (session.value?.inquiries ?? []).map((i) => i.durationMs).filter((x): x is number => x !== null)
  return d.length ? Math.round(d.reduce((a, b) => a + b, 0) / d.length) : null
})

const formatTime = (v: string) => new Date(v).toLocaleTimeString('en-GB', { hour: '2-digit', minute: '2-digit' })
/** Date, time to the second and milliseconds: the evidence of when a request came and its answer left. */
const formatPrecise = (v: string) => {
  const d = new Date(v)
  return `${d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short' })} ${d.toLocaleTimeString('en-GB', { hour12: false })}.${String(d.getMilliseconds()).padStart(3, '0')}`
}

async function load() {
  busy.value = true
  try {
    page.value = await scrApi.accessLog({ ...q, search: q.search.trim() })
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function open(s: ScrSignIn) {
  try {
    session.value = await scrApi.session(s.signInId)
  } catch (err) {
    showError(errorText(err))
  }
}

onMounted(async () => {
  try {
    companies.value = await scrApi.companies()
  } catch {
    // the company filter is optional
  }
  await load()
})
</script>
