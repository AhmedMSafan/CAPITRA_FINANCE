<template>
  <!-- End of Day: the night batch of every application. An administrator runs it for today, or for several days up to a date; days run in
       order and each job's result is kept. When a job fails the others still run and the day stays open until it is run again. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-moon-o"></span></span>
        <div>
          <h1>{{ tr('End of Day') }}</h1>
          <p>{{ tr('The night batch of every application: each business day, in order, every job switched on runs and its result is kept.') }}</p>
        </div>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'eod-jobs' }" class="fx-btn"><span class="fa fa-sliders" aria-hidden="true"></span>{{ tr('Jobs') }}</RouterLink>
        <button v-if="page?.canRun" type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="openRun">
          <span class="fa fa-play" aria-hidden="true"></span>{{ tr(page.isNextDayOpen ? 'Run again' : 'Run end of day') }}
        </button>
      </div>
    </div>

    <section v-if="page" class="scr-band" :aria-label="tr('End of Day')">
      <div class="scr-band-head">
        <span class="scr-band-pulse" :class="{ 'eod-pulse-bad': page.isNextDayOpen }" aria-hidden="true"></span>
        <b>{{ tr(page.isNextDayOpen ? 'A day is open: a job failed' : page.nextDate ? 'Ready to run' : 'Done up to today') }}</b>
      </div>
      <div class="scr-band-stats">
        <div class="scr-stat">
          <span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-calendar"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Today') }}</em><b class="eod-band-text">{{ formatDate(page.today) }}</b></span>
        </div>
        <div class="scr-stat">
          <span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-check"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Last day done') }}</em><b class="eod-band-text">{{ page.lastDoneDate ? formatDate(page.lastDoneDate) : tr('None yet') }}</b></span>
        </div>
        <div class="scr-stat">
          <span class="scr-stat-ic" :class="{ 'scr-stat-ic--orange': page.isNextDayOpen }" aria-hidden="true"><span class="fa fa-step-forward"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Next day to run') }}</em><b class="eod-band-text">{{ page.nextDate ? formatDate(page.nextDate) : '-' }}</b>
            <small v-if="page.isNextDayOpen">{{ tr('Open: only the jobs not done will run') }}</small></span>
        </div>
        <RouterLink :to="{ name: 'eod-jobs' }" class="scr-stat">
          <span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-tasks"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Jobs switched on') }}</em><b class="eod-band-text">{{ enabledJobs }} / {{ page.jobs.length }}</b></span>
        </RouterLink>
      </div>
    </section>

    <div v-if="openDay && failedJobs.length" class="fx-card fx-card-pad eod-alert" role="alert">
      <span class="fa fa-exclamation-triangle" aria-hidden="true"></span>
      <div>
        <b>{{ formatDate(openDay.businessDate) }}: {{ tr('these jobs failed. Fix the cause, then run again: only the jobs not done will run.') }}</b>
        <ul>
          <li v-for="j in failedJobs" :key="j.jobCode"><b>{{ jobName(j) }}</b> — <span dir="auto">{{ tr(j.errorMessage ?? '') }}</span></li>
        </ul>
      </div>
    </div>

    <div class="fx-card">
      <div class="fc-card-head"><h2>{{ tr('Business days') }} ({{ page?.days.length ?? 0 }})</h2>
        <button type="button" class="fx-btn fx-btn--sm" :disabled="loading" @click="load"><span :class="['fa', loading ? 'fa-spinner fa-spin' : 'fa-refresh']" aria-hidden="true"></span>{{ tr('Refresh') }}</button></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th class="eod-toggle-col"></th><th>{{ tr('Business day') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Jobs') }}</th><th>{{ tr('Started') }}</th><th>{{ tr('Finished') }}</th><th>{{ tr('By') }}</th></tr></thead>
          <tbody>
            <template v-for="d in page?.days ?? []" :key="d.dayId">
              <tr class="eod-day" @click="toggle(d.dayId)">
                <td class="eod-toggle-col"><span :class="['fa', open.has(d.dayId) ? 'fa-chevron-down' : 'fa-chevron-right']" aria-hidden="true"></span></td>
                <td><b>{{ formatDate(d.businessDate) }}</b></td>
                <td><span class="fx-chip" :class="STATUS_CHIP[d.statusCode]">{{ tr(STATUS[d.statusCode] ?? d.statusCode) }}</span></td>
                <td><span class="eod-dots">
                  <span v-for="j in d.jobs" :key="j.jobCode" class="eod-dot" :class="'eod-dot--' + j.statusCode.toLowerCase()" :title="jobName(j) + ': ' + tr(STATUS[j.statusCode] ?? j.statusCode)"></span>
                </span> <small class="c3-muted">{{ countDone(d) }} / {{ d.jobs.length }} {{ tr('done') }}</small></td>
                <td>{{ formatDateTime(d.startedAt) }}</td>
                <td>{{ d.finishedAt ? formatDateTime(d.finishedAt) : '-' }}</td>
                <td dir="ltr">{{ d.runBy }}</td>
              </tr>
              <tr v-if="open.has(d.dayId)" class="eod-jobs-row">
                <td colspan="7">
                  <table class="eod-jobs">
                    <tbody>
                      <tr v-for="j in d.jobs" :key="j.jobCode">
                        <td class="eod-job-name"><b>{{ jobName(j) }}</b><small>{{ tr(MODULES[j.moduleCode] ?? j.moduleCode) }}</small></td>
                        <td><span class="fx-chip" :class="STATUS_CHIP[j.statusCode]">{{ tr(STATUS[j.statusCode] ?? j.statusCode) }}</span>
                          <small v-if="j.attempts > 1" class="eod-attempts">{{ j.attempts }} {{ tr('attempts') }}</small></td>
                        <td class="eod-job-text">
                          <span v-if="j.summary" dir="ltr">{{ j.summary }}</span>
                          <span v-if="j.errorMessage" class="eod-error" dir="ltr">{{ j.errorMessage }}</span>
                        </td>
                        <td class="eod-job-time">{{ j.finishedAt ? formatDateTime(j.finishedAt) : '' }}</td>
                      </tr>
                    </tbody>
                  </table>
                  <RouterLink v-if="d.jobs.some((j) => j.jobCode === 'LND_DAILY')" :to="{ name: 'lnd-eod' }" class="eod-detail-link">
                    <span class="fa fa-external-link" aria-hidden="true"></span>{{ tr('Retail Lending detail: finances in non-accrual and proposed for write-off') }}</RouterLink>
                </td>
              </tr>
            </template>
          </tbody>
        </table>
      </div>
      <div v-if="loading && !page" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
      <div v-else-if="!page?.days.length" class="fx-empty"><span class="fa fa-calendar-o" aria-hidden="true"></span>{{ tr('End of day has not run yet.') }}</div>
    </div>

    <ScrDialog v-if="running && page" :title="tr(page.isNextDayOpen ? 'Run again' : 'Run end of day')" :subtitle="tr('Administrators only. Days run in order and never ahead of today.')" icon="fa-play" @close="running = false">
      <div class="eod-run">
        <div class="eod-choice">
          <label class="eod-radio"><input v-model="mode" type="radio" value="next">
            <span><b>{{ tr('One day') }}: {{ formatDate(page.nextDate!) }}</b><small>{{ tr(page.isNextDayOpen ? 'The open day: only its jobs not done run.' : 'The next business day not done yet.') }}</small></span></label>
          <label class="eod-radio"><input v-model="mode" type="radio" value="upto" :disabled="page.nextDate === page.today">
            <span><b>{{ tr('Several days, up to') }}</b><small>{{ tr('Every day from the next one to the day chosen, in order. It stops at a day where a job fails.') }}</small></span></label>
          <label v-if="mode === 'upto'" class="fc-field eod-date"><span>{{ tr('Up to') }}</span>
            <input v-model="toDate" class="fx-search" type="date" :min="page.nextDate ?? undefined" :max="page.today"></label>
        </div>
        <p class="eod-days-count"><span class="fa fa-calendar-check-o" aria-hidden="true"></span>
          {{ dayCount }} {{ tr('day(s) will run') }}: {{ formatDate(page.nextDate!) + (dayCount > 1 ? ` ${tr('to')} ${formatDate(target)}` : '') }}</p>
        <h3>{{ tr('Jobs, in this order') }}</h3>
        <ol class="eod-run-jobs">
          <li v-for="j in page.jobs" :key="j.jobCode" :class="{ 'eod-off': !j.isEnabled }">
            <span>{{ language === 'AR' ? j.nameAr : j.nameEn }}</span>
            <span class="fx-chip" :class="j.isEnabled ? 'fx-chip--ok' : 'fx-chip--plain'">{{ tr(j.isEnabled ? 'On' : 'Off') }}</span>
          </li>
        </ol>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" :disabled="busy" @click="running = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || (mode === 'upto' && !toDate)" @click="run">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-play']" aria-hidden="true"></span>{{ tr('Run') }}</button>
      </template>
    </ScrDialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '@/modules/scr/components/scr.css'
import './eod.css'
import { computed, onMounted, ref } from 'vue'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { language, tr } from '@/shared/layout/language'
import { formatDate, formatDateTime } from '@/modules/scr/utils/dates'
import ScrDialog from '@/modules/scr/components/ScrDialog.vue'
import { MODULES, STATUS, STATUS_CHIP, eodApi, errorText, type EodDay, type EodDayJob, type EodPage } from '../api/eod.api'

const page = ref<EodPage | null>(null)
const loading = ref(false)
const busy = ref(false)
const running = ref(false)
const mode = ref<'next' | 'upto'>('next')
const toDate = ref('')
const open = ref(new Set<number>())

const enabledJobs = computed(() => page.value?.jobs.filter((j) => j.isEnabled).length ?? 0)
const openDay = computed(() => page.value?.days.find((d) => d.statusCode === 'OPEN') ?? null)
const failedJobs = computed(() => openDay.value?.jobs.filter((j) => j.statusCode === 'FAILED') ?? [])
const target = computed(() => (mode.value === 'upto' && toDate.value ? toDate.value : page.value?.nextDate ?? ''))
const dayCount = computed(() => {
  const from = page.value?.nextDate
  if (!from || !target.value) return 0
  return Math.max(1, Math.round((Date.parse(target.value) - Date.parse(from)) / 86_400_000) + 1)
})

const jobName = (j: EodDayJob) => (language.value === 'AR' ? j.nameAr : j.nameEn)
const countDone = (d: EodDay) => d.jobs.filter((j) => j.statusCode === 'DONE' || j.statusCode === 'SKIPPED').length

function toggle(id: number) {
  const s = new Set(open.value)
  if (s.has(id)) s.delete(id)
  else s.add(id)
  open.value = s
}

function openRun() {
  mode.value = 'next'
  toDate.value = page.value?.today ?? ''
  running.value = true
}

async function run() {
  busy.value = true
  try {
    const r = await eodApi.run(mode.value === 'upto' ? toDate.value : page.value?.nextDate ?? null)
    page.value = r.page
    running.value = false
    if (r.result.openDate) {
      showError(`${r.result.daysDone ? `${r.result.daysDone} ${tr('day(s) done.')} ` : ''}${formatDate(r.result.openDate)}: ${tr('a job failed. Fix the cause and run again; only the jobs not done run.')}`)
      open.value = new Set(r.page.days.filter((d) => d.businessDate === r.result.openDate).map((d) => d.dayId))
    } else {
      showSuccess(`${tr('End of day done up to')} ${formatDate(r.page.lastDoneDate)} (${r.result.daysDone} ${tr('day(s)')})`)
      if (r.page.days[0]) open.value = new Set([r.page.days[0].dayId])
    }
  } catch (err) {
    showError(errorText(err))
    await load()
  } finally {
    busy.value = false
  }
}

async function load() {
  loading.value = true
  try {
    page.value = await eodApi.page()
    if (open.value.size === 0 && page.value.days[0]) open.value = new Set([page.value.days[0].dayId])
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>
