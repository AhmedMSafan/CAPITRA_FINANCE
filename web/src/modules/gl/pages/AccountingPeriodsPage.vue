<script setup lang="ts">
// Accounting periods (APEX App 120 page 30): calendars, fiscal years, their periods, and opening and closing each period per ledger.
import { computed, reactive, ref, shallowRef } from 'vue'
import { RouterLink } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { showSuccess } from '@/shared/feedback/feedback'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import GlDialog from '../components/GlDialog.vue'
import GlPageHeader from '../components/GlPageHeader.vue'
import { blank, emptyErrors, numberOrNull, toFormErrors, type FormErrors } from '../components/formErrors'
import { codeLabel } from '../api/glSetup.api'
import {
  periodsApi,
  periodStatusClass,
  type AccountingPeriodsPage,
  type GlAccountingPeriod,
  type GlCalendar,
  type LedgerPeriodAction,
  type LedgerPeriodInfo,
  type PeriodLedgerStatus
} from '../api/glPeriods.api'

const canEdit = hasPermission('GL.SETUP.EDIT')
const canControl = hasPermission('GL.PERIOD.CONTROL')

const page = shallowRef<AccountingPeriodsPage | null>(null)
const selectedYearId = ref<number | null>(null)
const loadError = ref('')
useFeedback({ errors: [loadError] })

const calendar = computed(() => page.value?.calendars.find(c => c.calendarId === page.value?.calendarId) ?? null)
const today = computed(() => page.value?.today ?? '')
const year = computed(() => page.value?.years.find(y => y.fiscalYearId === selectedYearId.value) ?? null)
const yearPeriods = computed(() => page.value?.periods.filter(p => p.fiscalYearId === selectedYearId.value) ?? [])

async function load(calendarId?: number | null, keepYear = true) {
  try {
    const data = await periodsApi.get(calendarId)
    page.value = data
    if (!keepYear || !data.years.some(y => y.fiscalYearId === selectedYearId.value)) {
      const current = data.years.find(y => y.startDate <= data.today && y.endDate >= data.today)
      selectedYearId.value = current?.fiscalYearId ?? data.years[0]?.fiscalYearId ?? null
    }
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'The accounting periods could not be loaded.'
  }
}

function onCalendarChange(event: Event) {
  load(Number((event.target as HTMLSelectElement).value), false)
}

/** The regular period that covers today, if any. */
const todayPeriod = computed(() =>
  page.value?.periods.find(p => !p.isAdjustmentPeriod && p.startDate <= today.value && p.endDate >= today.value) ?? null)
const notOpenToday = computed(() => todayPeriod.value?.ledgers.filter(l => l.periodStatus !== 'OPEN') ?? [])

function addDays(iso: string, days: number): string {
  const [y, m, d] = iso.split('-').map(Number)
  return new Date(Date.UTC(y!, m! - 1, d! + days)).toISOString().slice(0, 10)
}

/** Data checks of a fiscal year's periods: dates outside the year, overlaps and gaps (page 30 "to check" list). */
function issuesOf(yearId: number): Map<number, string[]> {
  const out = new Map<number, string[]>()
  const y = page.value?.years.find(x => x.fiscalYearId === yearId)
  if (!y || !page.value) return out
  const add = (id: number, message: string) => out.set(id, [...(out.get(id) ?? []), message])
  const rows = page.value.periods.filter(p => p.fiscalYearId === yearId)
  const regularAll = page.value.periods.filter(p => !p.isAdjustmentPeriod)

  for (const p of rows) {
    if (p.startDate < y.startDate || p.endDate > y.endDate)
      add(p.periodId, `Dates are outside fiscal year ${y.yearName ?? y.fiscalYear} (${formatDate(y.startDate)} – ${formatDate(y.endDate)}).`)
    if (!p.isAdjustmentPeriod)
      for (const o of regularAll)
        if (o.periodId !== p.periodId && o.startDate <= p.endDate && o.endDate >= p.startDate)
          add(p.periodId, `Overlaps period ${o.periodCode} (${formatDate(o.startDate)} – ${formatDate(o.endDate)}).`)
  }

  const regular = rows
    .filter(p => !p.isAdjustmentPeriod && p.startDate >= y.startDate && p.endDate <= y.endDate)
    .sort((a, b) => a.startDate.localeCompare(b.startDate))
  for (let i = 1; i < regular.length; i++) {
    const gapStart = addDays(regular[i - 1]!.endDate, 1)
    if (regular[i]!.startDate > gapStart)
      add(regular[i]!.periodId, `Gap: no period covers ${formatDate(gapStart)} – ${formatDate(addDays(regular[i]!.startDate, -1))}.`)
  }
  const first = regular[0]
  const last = regular[regular.length - 1]
  if (first && first.startDate > y.startDate)
    add(first.periodId, `Gap: the year starts on ${formatDate(y.startDate)} but the first period starts on ${formatDate(first.startDate)}.`)
  if (last && last.endDate < y.endDate)
    add(last.periodId, `Gap: the year ends on ${formatDate(y.endDate)} but the last period ends on ${formatDate(last.endDate)}.`)
  return out
}

const issues = computed(() => (selectedYearId.value ? issuesOf(selectedYearId.value) : new Map<number, string[]>()))

function isToday(p: GlAccountingPeriod) {
  return !p.isAdjustmentPeriod && p.startDate <= today.value && p.endDate >= today.value
}

// ---------------------------------------------------------------- setup dialogs
type DialogKind = 'calendar' | 'generate' | 'year' | 'period'
const dialog = ref<DialogKind | null>(null)
const editing = ref(false)
const saving = ref(false)
const errors = ref<FormErrors>(emptyErrors())
const form = reactive<Record<string, unknown>>({})
let editingId = 0
let editingVersion = 0

function open(kind: DialogKind, values: Record<string, unknown>, isEdit: boolean, id = 0, version = 0) {
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, values)
  dialog.value = kind
  editing.value = isEdit
  editingId = id
  editingVersion = version
  errors.value = emptyErrors()
}

function openCalendar(c: GlCalendar | null) {
  open('calendar', c
    ? { ...c, calendarNameAr: c.calendarNameAr ?? '' }
    : { calendarCode: '', calendarNameEn: '', calendarNameAr: '', calendarType: 'MONTHLY', periodFrequency: 'MONTHLY', isAdjustmentPeriodUsed: true, isActive: true },
  !!c, c?.calendarId, c?.rowVersion)
}

function openGenerate() {
  const c = calendar.value
  if (!c || !page.value) return
  const last = [...page.value.years].sort((a, b) => b.fiscalYear - a.fiscalYear)[0]
  const fiscalYear = last ? last.fiscalYear + 1 : Number(today.value.slice(0, 4))
  const startDate = last ? addDays(last.endDate, 1) : `${fiscalYear}-01-01`
  open('generate', {
    fiscalYear, startDate, yearStatus: 'OPEN', includeAdjustmentPeriod: c.isAdjustmentPeriodUsed,
    createLedgerPeriods: page.value.ledgers.length > 0
  }, false)
}

const MONTHS = ['JAN', 'FEB', 'MAR', 'APR', 'MAY', 'JUN', 'JUL', 'AUG', 'SEP', 'OCT', 'NOV', 'DEC']

/** The periods "Generate" will create, shown before saving. */
const generatePreview = computed(() => {
  if (dialog.value !== 'generate') return []
  const start = String(form.startDate ?? '')
  const fy = Number(form.fiscalYear) || '????'
  if (!/^\d{4}-\d{2}-\d{2}$/.test(start)) return []
  const [y, m] = start.split('-').map(Number)
  const out: { code: string; range: string }[] = []
  for (let i = 0; i < 12; i++) {
    const s = new Date(Date.UTC(y!, m! - 1 + i, 1))
    const e = new Date(Date.UTC(y!, m! + i, 0))
    out.push({ code: `${MONTHS[s.getUTCMonth()]}-${fy}`, range: `${s.getUTCDate()}/${s.getUTCMonth() + 1} – ${e.getUTCDate()}/${e.getUTCMonth() + 1}` })
  }
  if (form.includeAdjustmentPeriod) out.push({ code: `ADJ-${fy}`, range: 'Adjustment' })
  return out
})

function openYear() {
  const y = year.value
  if (!y) return
  open('year', { fiscalYear: y.fiscalYear, yearName: y.yearName ?? '', startDate: y.startDate, endDate: y.endDate, yearStatus: y.yearStatus },
    true, y.fiscalYearId, y.rowVersion)
}

function openPeriod(p: GlAccountingPeriod | null) {
  const y = year.value
  if (!y) return
  if (!p) {
    const no = yearPeriods.value.reduce((n, r) => Math.max(n, r.periodNo), 0) + 1
    open('period', {
      periodCode: '', periodNo: no, periodNameEn: '', periodNameAr: '', startDate: y.startDate, endDate: y.endDate, quarterNo: '',
      isAdjustmentPeriod: false, defaultPeriodStatus: 'NEVER_OPENED'
    }, false)
    return
  }
  open('period', { ...p, periodNameAr: p.periodNameAr ?? '', quarterNo: p.quarterNo ?? '' }, true, p.periodId, p.rowVersion)
}

const editingPeriod = computed(() => (dialog.value === 'period' && editing.value ? page.value?.periods.find(p => p.periodId === editingId) ?? null : null))

function closeDialog() {
  if (!saving.value) dialog.value = null
}

async function save() {
  saving.value = true
  errors.value = emptyErrors()
  const names = Object.keys(form)
  try {
    if (dialog.value === 'calendar') {
      const body = { ...form, calendarCode: String(form.calendarCode ?? '').trim().toUpperCase(), calendarNameAr: blank(form.calendarNameAr), rowVersion: editingVersion }
      const saved = editing.value ? await periodsApi.updateCalendar(editingId, body) : await periodsApi.createCalendar(body)
      dialog.value = null
      showSuccess(`Calendar ${saved.calendarCode} saved.`)
      await load(saved.calendarId, editing.value)
    } else if (dialog.value === 'generate' && calendar.value) {
      const result = await periodsApi.generateYear(calendar.value.calendarId, {
        ...form, fiscalYear: numberOrNull(form.fiscalYear), startDate: blank(form.startDate)
      })
      dialog.value = null
      selectedYearId.value = result.fiscalYearId
      showSuccess(result.message)
      await load(calendar.value.calendarId)
    } else if (dialog.value === 'year') {
      await periodsApi.updateYear(editingId, {
        yearName: blank(form.yearName), startDate: blank(form.startDate), endDate: blank(form.endDate), yearStatus: form.yearStatus, rowVersion: editingVersion
      })
      dialog.value = null
      showSuccess('Fiscal year saved.')
      await load(calendar.value?.calendarId)
    } else if (dialog.value === 'period' && year.value) {
      const body = {
        ...form, periodCode: String(form.periodCode ?? '').trim().toUpperCase(), periodNo: numberOrNull(form.periodNo),
        periodNameAr: blank(form.periodNameAr), startDate: blank(form.startDate), endDate: blank(form.endDate),
        quarterNo: numberOrNull(form.quarterNo), rowVersion: editingVersion
      }
      const saved = editing.value ? await periodsApi.updatePeriod(editingId, body) : await periodsApi.createPeriod(year.value.fiscalYearId, body)
      dialog.value = null
      showSuccess(`Period ${saved.periodCode} saved.`)
      await load(calendar.value?.calendarId)
    }
  } catch (e) {
    errors.value = toFormErrors(e, names)
  } finally {
    saving.value = false
  }
}

async function syncLedgerPeriods() {
  const y = year.value
  if (!y) return
  try {
    const result = await periodsApi.syncLedgerPeriods(y.fiscalYearId)
    showSuccess(result.message)
    await load(calendar.value?.calendarId)
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'The ledger periods could not be created.'
  }
}

const missingLedgerPeriods = computed(() => {
  const ledgers = page.value?.ledgers.length ?? 0
  return yearPeriods.value.reduce((n, p) => n + Math.max(0, ledgers - p.ledgers.length), 0)
})

// ---------------------------------------------------------------- ledger period details and actions
const info = shallowRef<LedgerPeriodInfo | null>(null)
const infoError = ref('')

async function openInfo(ledgerPeriodId: number) {
  infoError.value = ''
  try {
    info.value = await periodsApi.ledgerPeriod(ledgerPeriodId)
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'The ledger period could not be loaded.'
  }
}

const ACTIONS: { action: LedgerPeriodAction; label: string; cls: string; done: string }[] = [
  { action: 'OPEN', label: 'Open period', cls: 'cp-btn--primary', done: 'opened' },
  { action: 'SET_FUTURE', label: 'Allow future entry', cls: '', done: 'set to future entry' },
  { action: 'CLOSE', label: 'Close period', cls: '', done: 'closed' },
  { action: 'REOPEN', label: 'Reopen', cls: '', done: 'reopened' },
  { action: 'PERMANENT', label: 'Close permanently', cls: 'cp-btn--danger', done: 'closed permanently' }
]

/** Which actions the status allows (GL_PERIOD_CONTROL_PKG). */
function allowed(status: string, action: LedgerPeriodAction): boolean {
  switch (action) {
    case 'OPEN': return status === 'NEVER_OPENED' || status === 'FUTURE_ENTRY'
    case 'SET_FUTURE': return status === 'NEVER_OPENED'
    case 'CLOSE': return status === 'OPEN'
    case 'REOPEN': case 'PERMANENT': return status === 'CLOSED'
  }
}

type PendingAction = {
  ledgerPeriodId: number
  rowVersion: number
  ledgerCode: string
  periodCode: string
  action: LedgerPeriodAction
  intro?: string
}
const pending = ref<PendingAction | null>(null)
const reason = ref('')
const acting = ref(false)
const actionError = ref('')
const pendingMeta = computed(() => ACTIONS.find(a => a.action === pending.value?.action) ?? ACTIONS[0]!)
const reasonRequired = computed(() => pending.value?.action === 'REOPEN' || pending.value?.action === 'PERMANENT')

function ask(p: PendingAction) {
  pending.value = p
  reason.value = ''
  actionError.value = ''
}

function askFromTable(p: GlAccountingPeriod, l: PeriodLedgerStatus, action: LedgerPeriodAction) {
  ask({ ledgerPeriodId: l.ledgerPeriodId, rowVersion: l.rowVersion, ledgerCode: l.ledgerCode, periodCode: p.periodCode, action })
}

function askFromInfo(action: LedgerPeriodAction) {
  const i = info.value
  if (!i) return
  info.value = null
  ask({ ledgerPeriodId: i.ledgerPeriodId, rowVersion: i.rowVersion, ledgerCode: i.ledgerCode, periodCode: i.periodCode, action })
}

async function runAction() {
  const p = pending.value
  if (!p) return
  if (reasonRequired.value && !reason.value.trim()) {
    actionError.value = 'Please enter a reason.'
    return
  }
  acting.value = true
  actionError.value = ''
  try {
    const result = await periodsApi.action(p.ledgerPeriodId, p.action, reason.value.trim() || null, p.rowVersion)
    pending.value = null
    showSuccess(`${p.ledgerCode} ${p.periodCode} is now ${codeLabel(result.periodStatus).toLowerCase()}.`)
    await load(calendar.value?.calendarId)
    // After a month-end close, offer to open the next period straight away.
    const next = result.next
    if (p.action === 'CLOSE' && next && allowed(next.periodStatus, 'OPEN'))
      ask({
        ledgerPeriodId: next.ledgerPeriodId, rowVersion: next.rowVersion, ledgerCode: p.ledgerCode, periodCode: next.periodCode, action: 'OPEN',
        intro: `${p.periodCode} is closed. Open the next period ${next.periodCode} (${codeLabel(next.periodStatus).toLowerCase()}) for ${p.ledgerCode} now?`
      })
  } catch (e) {
    actionError.value = toFormErrors(e, []).form
  } finally {
    acting.value = false
  }
}

function closeAction() {
  if (!acting.value) pending.value = null
}

load()
</script>

<template>
  <div class="cp-page">
    <GlPageHeader
      title="Accounting periods"
      description="Calendars, fiscal years and their periods. Each ledger opens and closes its own periods: posting is allowed only into an open period."
      icon="pi pi-calendar"
    >
      <select v-if="page?.calendars.length" :value="page.calendarId ?? ''" aria-label="Calendar" @change="onCalendarChange">
        <option v-for="c in page.calendars" :key="c.calendarId" :value="c.calendarId">{{ c.calendarCode }} - {{ c.calendarNameEn }}</option>
      </select>
      <button v-if="canEdit && calendar" type="button" class="cp-btn" @click="openCalendar(calendar)">
        <i class="pi pi-pencil"></i><span>Edit calendar</span>
      </button>
      <button v-if="canEdit" type="button" class="cp-btn" @click="openCalendar(null)">
        <i class="pi pi-plus"></i><span>New calendar</span>
      </button>
      <button v-if="canEdit && calendar" type="button" class="cp-btn cp-btn--create" @click="openGenerate">
        <i class="pi pi-sync"></i><span>Generate fiscal year</span>
      </button>
    </GlPageHeader>

    <div v-if="!page" class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div>

    <div v-else-if="!calendar" class="cp-card cp-empty ap-none">
      <b>No calendar yet</b>
      <span>Create one with <b>New calendar</b>, then generate its first fiscal year.</span>
    </div>

    <template v-else>
      <section class="cp-card ap-info">
        <div><small>Calendar</small><b>{{ calendar.calendarNameEn }}<span v-if="calendar.calendarNameAr" class="ar"> · {{ calendar.calendarNameAr }}</span></b></div>
        <div><small>Type</small><b>{{ codeLabel(calendar.calendarType) }}</b></div>
        <div><small>Frequency</small><b>{{ codeLabel(calendar.periodFrequency) }}</b></div>
        <div><small>Adjustment period</small><b>{{ calendar.isAdjustmentPeriodUsed ? 'Yes' : 'No' }}</b></div>
        <div>
          <small>Status</small>
          <b><span class="cp-pill" :class="calendar.isActive ? 'cp-pill--ok' : 'cp-pill--bad'">{{ calendar.isActive ? 'Active' : 'Inactive' }}</span></b>
        </div>
        <div class="ap-info__ledgers">
          <small>Ledgers on this calendar</small>
          <div>
            <span v-for="l in page.ledgers" :key="l.ledgerId" class="ap-tag" :title="l.ledgerNameEn">{{ l.ledgerCode }}</span>
            <span v-if="!page.ledgers.length" class="cp-muted">None yet.</span>
            <RouterLink to="/gl/ledgers" class="ap-link">Manage ledgers</RouterLink>
          </div>
        </div>
      </section>

      <div v-if="todayPeriod && notOpenToday.length" class="ap-alert" role="status">
        <i class="pi pi-exclamation-triangle"></i>
        <div>
          <b>Today's period {{ todayPeriod.periodCode }} is not open</b>
          <span>{{ notOpenToday.map(l => `${l.ledgerCode}: ${codeLabel(l.periodStatus).toLowerCase()}`).join(' · ') }}</span>
        </div>
        <template v-if="canControl">
          <template v-for="l in notOpenToday" :key="l.ledgerPeriodId">
            <button v-if="allowed(l.periodStatus, 'OPEN')" type="button" class="cp-btn cp-btn--small cp-btn--primary" @click="askFromTable(todayPeriod, l, 'OPEN')">
              Open {{ l.ledgerCode }}
            </button>
            <button v-else type="button" class="cp-btn cp-btn--small" @click="openInfo(l.ledgerPeriodId)">{{ l.ledgerCode }} details</button>
          </template>
        </template>
      </div>
      <div v-else-if="!todayPeriod && page.periods.length" class="ap-alert" role="status">
        <i class="pi pi-exclamation-triangle"></i>
        <div><b>No period covers today ({{ formatDate(today) }})</b><span>Generate the fiscal year or add a period.</span></div>
      </div>

      <div class="ap-layout">
        <aside class="ap-years" aria-label="Fiscal years">
          <button
            v-for="y in page.years"
            :key="y.fiscalYearId"
            type="button"
            class="ap-year"
            :class="{ on: y.fiscalYearId === selectedYearId }"
            @click="selectedYearId = y.fiscalYearId"
          >
            <span class="ap-year__top"><b>{{ y.yearName ?? y.fiscalYear }}</b><span class="cp-pill" :class="periodStatusClass(y.yearStatus)">{{ codeLabel(y.yearStatus) }}</span></span>
            <small>{{ formatDate(y.startDate) }} – {{ formatDate(y.endDate) }}</small>
            <small>
              {{ y.periodCount }} periods
              <span v-if="issuesOf(y.fiscalYearId).size" class="ap-bad"> · {{ issuesOf(y.fiscalYearId).size }} to check</span>
            </small>
          </button>
          <div v-if="!page.years.length" class="cp-card cp-empty">No fiscal years yet.</div>
        </aside>

        <section class="cp-card ap-panel">
          <div v-if="!year" class="cp-empty">
            <b class="ap-empty-title">No fiscal year selected</b>
            Use <b>Generate fiscal year</b> to create twelve monthly periods (and an adjustment period) in one step.
          </div>
          <template v-else>
            <div class="cp-card__head ap-head">
              <h3>Fiscal year {{ year.yearName ?? year.fiscalYear }} <small>{{ yearPeriods.length }} periods · {{ formatDate(year.startDate) }} – {{ formatDate(year.endDate) }}</small></h3>
              <span class="cp-spacer"></span>
              <button v-if="canEdit && page.ledgers.length && missingLedgerPeriods > 0" type="button" class="cp-btn cp-btn--small" @click="syncLedgerPeriods">
                <i class="pi pi-sync"></i><span>Create ledger periods ({{ missingLedgerPeriods }})</span>
              </button>
              <button v-if="canEdit" type="button" class="cp-btn cp-btn--small" @click="openYear">
                <i class="pi pi-pencil"></i><span>Edit year</span>
              </button>
              <button v-if="canEdit" type="button" class="cp-btn cp-btn--small cp-btn--create" @click="openPeriod(null)">
                <i class="pi pi-plus"></i><span>Add period</span>
              </button>
            </div>

            <div v-if="issues.size" class="ap-warn">
              <b>{{ issues.size }} period{{ issues.size > 1 ? 's need' : ' needs' }} attention.</b>
              The periods below do not fit the fiscal year; the rows are marked. Fix their dates with the edit button.
            </div>

            <div v-if="!yearPeriods.length" class="cp-empty">This fiscal year has no periods.</div>
            <div v-else class="cp-table-wrap">
              <table class="cp-table ap-table">
                <thead>
                  <tr><th>#</th><th>Code</th><th>Name</th><th>Dates</th><th>Qtr</th><th>Default status</th><th>Ledgers</th><th></th></tr>
                </thead>
                <tbody>
                  <tr
                    v-for="p in yearPeriods"
                    :key="p.periodId"
                    :class="{ adj: p.isAdjustmentPeriod, now: isToday(p), bad: issues.has(p.periodId) }"
                  >
                    <td class="num">{{ p.periodNo }}</td>
                    <td class="nowrap"><code class="ap-code">{{ p.periodCode }}</code><span v-if="isToday(p)" class="ap-today">Today</span></td>
                    <td>
                      {{ p.periodNameEn }}
                      <span v-if="p.isAdjustmentPeriod" class="ap-tag">Adjustment</span>
                      <span v-if="p.periodNameAr" class="ar">{{ p.periodNameAr }}</span>
                      <ul v-if="issues.has(p.periodId)" class="ap-issues">
                        <li v-for="m in issues.get(p.periodId)" :key="m">{{ m }}</li>
                      </ul>
                    </td>
                    <td class="num">{{ formatDate(p.startDate) }}<template v-if="p.endDate !== p.startDate"> – {{ formatDate(p.endDate) }}</template></td>
                    <td class="num">{{ p.quarterNo ? `Q${p.quarterNo}` : '–' }}</td>
                    <td><span class="cp-pill" :class="periodStatusClass(p.defaultPeriodStatus)">{{ codeLabel(p.defaultPeriodStatus) }}</span></td>
                    <td>
                      <span v-if="!p.ledgers.length" class="cp-muted ap-small">{{ page.ledgers.length ? 'Not created' : 'No ledger' }}</span>
                      <div v-for="l in p.ledgers" :key="l.ledgerPeriodId" class="ap-lrow">
                        <button type="button" class="ap-led" :class="periodStatusClass(l.periodStatus)" title="Details, history and all actions" @click="openInfo(l.ledgerPeriodId)">
                          <i></i>{{ l.ledgerCode }} · {{ codeLabel(l.periodStatus) }}
                        </button>
                        <template v-if="canControl">
                          <button v-if="l.periodStatus === 'OPEN'" type="button" class="cp-btn cp-btn--small ap-mini" @click="askFromTable(p, l, 'CLOSE')">Close</button>
                          <button
                            v-else-if="allowed(l.periodStatus, 'OPEN')"
                            type="button"
                            class="cp-btn cp-btn--small ap-mini"
                            :class="isToday(p) ? 'cp-btn--primary' : 'ap-mini--open'"
                            @click="askFromTable(p, l, 'OPEN')"
                          >
                            Open
                          </button>
                        </template>
                      </div>
                    </td>
                    <td>
                      <button type="button" class="cp-btn cp-btn--small" :aria-label="`${canEdit ? 'Edit' : 'View'} ${p.periodCode}`" @click="openPeriod(p)">
                        <i :class="canEdit ? 'pi pi-pencil' : 'pi pi-eye'"></i>
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
            <p class="ap-note">
              "Today" is {{ formatDate(today) }} (Cairo). A period's status is not set by the date: the default status is copied to a ledger period when it is
              created, and after that each ledger keeps its own status. Click a ledger to see its history and every action.
            </p>
          </template>
        </section>
      </div>
      <p class="apex-ref">Replaces APEX App 120 page 30</p>
    </template>

    <!-- Calendar -->
    <GlDialog
      v-if="dialog === 'calendar'"
      :title="editing ? `Edit calendar ${form.calendarCode}` : 'New calendar'"
      :subtitle="editing ? 'The code identifies the calendar and cannot be changed.' : 'A calendar holds the fiscal years and periods its ledgers post into.'"
      :saving="saving"
      :error="errors.form"
      save-label="Save calendar"
      @close="closeDialog"
      @save="save"
    >
      <div class="cp-field">
        <label for="c-code">Calendar code *</label>
        <input id="c-code" v-model="form.calendarCode as string" class="upper" maxlength="30" :disabled="editing" />
        <small v-if="errors.fields.calendarCode" class="cp-field__error">{{ errors.fields.calendarCode }}</small>
      </div>
      <div class="cp-field">
        <label for="c-name">Name (English) *</label>
        <input id="c-name" v-model="form.calendarNameEn as string" maxlength="150" />
        <small v-if="errors.fields.calendarNameEn" class="cp-field__error">{{ errors.fields.calendarNameEn }}</small>
      </div>
      <div class="cp-field cp-field--wide">
        <label for="c-name-ar">Name (Arabic)</label>
        <input id="c-name-ar" v-model="form.calendarNameAr as string" maxlength="150" dir="rtl" />
      </div>
      <div class="cp-field">
        <label for="c-type">Calendar type</label>
        <select id="c-type" v-model="form.calendarType">
          <option v-for="t in page?.lists.calendarTypes" :key="t" :value="t">{{ codeLabel(t) }}</option>
        </select>
      </div>
      <div class="cp-field">
        <label for="c-freq">Period frequency</label>
        <select id="c-freq" v-model="form.periodFrequency">
          <option v-for="t in page?.lists.periodFrequencies" :key="t" :value="t">{{ codeLabel(t) }}</option>
        </select>
      </div>
      <div class="gl-checks">
        <label class="gl-check"><input v-model="form.isAdjustmentPeriodUsed" type="checkbox" /> Uses an adjustment period</label>
        <label class="gl-check"><input v-model="form.isActive" type="checkbox" /> Active</label>
      </div>
    </GlDialog>

    <!-- Generate fiscal year -->
    <GlDialog
      v-if="dialog === 'generate'"
      :title="`Generate fiscal year · ${calendar?.calendarCode}`"
      subtitle="Twelve monthly periods are created from the first day; each ends on the last day of its month."
      :saving="saving"
      :error="errors.form"
      save-label="Generate"
      @close="closeDialog"
      @save="save"
    >
      <div class="cp-field">
        <label for="g-year">Fiscal year *</label>
        <input id="g-year" v-model="form.fiscalYear" type="number" min="1990" max="2200" />
        <small v-if="errors.fields.fiscalYear" class="cp-field__error">{{ errors.fields.fiscalYear }}</small>
      </div>
      <div class="cp-field">
        <label for="g-start">First day of the year *</label>
        <input id="g-start" v-model="form.startDate as string" type="date" />
        <small v-if="errors.fields.startDate" class="cp-field__error">{{ errors.fields.startDate }}</small>
        <small v-else class="cp-field__hint">The first day of a month.</small>
      </div>
      <div class="cp-field">
        <label for="g-status">Fiscal year status</label>
        <select id="g-status" v-model="form.yearStatus">
          <option v-for="s in page?.lists.yearStatuses" :key="s" :value="s">{{ codeLabel(s) }}</option>
        </select>
      </div>
      <div></div>
      <div class="gl-checks">
        <label class="gl-check"><input v-model="form.includeAdjustmentPeriod" type="checkbox" /> Add an adjustment period (13)</label>
        <label class="gl-check" :class="{ 'ap-off': !page?.ledgers.length }">
          <input v-model="form.createLedgerPeriods" type="checkbox" :disabled="!page?.ledgers.length" />
          {{ page?.ledgers.length ? `Create ledger periods for ${page.ledgers.length} ledger(s)` : 'Create ledger periods (no ledger on this calendar)' }}
        </label>
      </div>
      <div class="cp-field cp-field--wide">
        <small class="cp-field__hint">New periods start as <b>Never opened</b>. Open the first one for each ledger when you are ready to post.</small>
        <div class="ap-preview">
          <span v-for="g in generatePreview" :key="g.code"><b>{{ g.code }}</b>{{ g.range }}</span>
        </div>
      </div>
    </GlDialog>

    <!-- Fiscal year -->
    <GlDialog
      v-if="dialog === 'year'"
      :title="`Edit fiscal year ${form.fiscalYear}`"
      subtitle="The status is a label for the year; posting is controlled by each ledger period."
      :saving="saving"
      :error="errors.form"
      save-label="Save year"
      @close="closeDialog"
      @save="save"
    >
      <div class="cp-field">
        <label for="y-no">Fiscal year</label>
        <input id="y-no" :value="form.fiscalYear" disabled />
      </div>
      <div class="cp-field">
        <label for="y-name">Name</label>
        <input id="y-name" v-model="form.yearName as string" maxlength="50" />
        <small v-if="errors.fields.yearName" class="cp-field__error">{{ errors.fields.yearName }}</small>
      </div>
      <div class="cp-field">
        <label for="y-start">Start date *</label>
        <input id="y-start" v-model="form.startDate as string" type="date" />
        <small v-if="errors.fields.startDate" class="cp-field__error">{{ errors.fields.startDate }}</small>
      </div>
      <div class="cp-field">
        <label for="y-end">End date *</label>
        <input id="y-end" v-model="form.endDate as string" type="date" />
        <small v-if="errors.fields.endDate" class="cp-field__error">{{ errors.fields.endDate }}</small>
      </div>
      <div class="cp-field">
        <label for="y-status">Status *</label>
        <select id="y-status" v-model="form.yearStatus">
          <option v-for="s in page?.lists.yearStatuses" :key="s" :value="s">{{ codeLabel(s) }}</option>
        </select>
      </div>
      <div class="cp-field">
        <small class="cp-field__hint ap-hint-top">The dates must still contain all {{ year?.periodCount }} periods of the year.</small>
      </div>
    </GlDialog>

    <!-- Period -->
    <GlDialog
      v-if="dialog === 'period'"
      :title="editing ? `Edit period ${form.periodCode}` : `Add period to ${year?.yearName ?? year?.fiscalYear}`"
      :subtitle="`Dates must fall inside fiscal year ${year?.yearName ?? year?.fiscalYear} (${formatDate(year?.startDate)} – ${formatDate(year?.endDate)}) and must not overlap another regular period.`"
      :saving="saving"
      :error="errors.form"
      :readonly="!canEdit"
      save-label="Save period"
      @close="closeDialog"
      @save="save"
    >
      <div class="cp-field">
        <label for="p-code">Period code *</label>
        <input id="p-code" v-model="form.periodCode as string" class="upper" maxlength="30" :disabled="editing" />
        <small v-if="errors.fields.periodCode" class="cp-field__error">{{ errors.fields.periodCode }}</small>
        <small v-else-if="editing" class="cp-field__hint">The code identifies the period and cannot be changed.</small>
      </div>
      <div class="cp-field">
        <label for="p-no">Period number *</label>
        <input id="p-no" v-model="form.periodNo" type="number" min="1" max="99" />
        <small v-if="errors.fields.periodNo" class="cp-field__error">{{ errors.fields.periodNo }}</small>
      </div>
      <div class="cp-field">
        <label for="p-name">Name (English) *</label>
        <input id="p-name" v-model="form.periodNameEn as string" maxlength="80" />
        <small v-if="errors.fields.periodNameEn" class="cp-field__error">{{ errors.fields.periodNameEn }}</small>
      </div>
      <div class="cp-field">
        <label for="p-name-ar">Name (Arabic)</label>
        <input id="p-name-ar" v-model="form.periodNameAr as string" maxlength="80" dir="rtl" />
      </div>
      <div class="cp-field">
        <label for="p-start">Start date *</label>
        <input id="p-start" v-model="form.startDate as string" type="date" />
        <small v-if="errors.fields.startDate" class="cp-field__error">{{ errors.fields.startDate }}</small>
      </div>
      <div class="cp-field">
        <label for="p-end">End date *</label>
        <input id="p-end" v-model="form.endDate as string" type="date" />
        <small v-if="errors.fields.endDate" class="cp-field__error">{{ errors.fields.endDate }}</small>
      </div>
      <div class="cp-field">
        <label for="p-q">Quarter</label>
        <select id="p-q" v-model="form.quarterNo">
          <option value="">–</option>
          <option v-for="q in 4" :key="q" :value="q">Q{{ q }}</option>
        </select>
        <small v-if="errors.fields.quarterNo" class="cp-field__error">{{ errors.fields.quarterNo }}</small>
      </div>
      <div class="cp-field">
        <label for="p-status">Default status for new ledger periods</label>
        <select id="p-status" v-model="form.defaultPeriodStatus">
          <option v-for="s in page?.lists.defaultPeriodStatuses" :key="s" :value="s">{{ codeLabel(s) }}</option>
        </select>
        <small class="cp-field__hint">Used only when a ledger period is first created. To open or close the period now, use the ledger buttons.</small>
      </div>
      <div class="gl-checks">
        <label class="gl-check"><input v-model="form.isAdjustmentPeriod" type="checkbox" /> Adjustment period</label>
      </div>
      <div v-if="editingPeriod?.ledgers.length" class="cp-field cp-field--wide">
        <label>Ledger periods</label>
        <div class="ap-lrows-inline">
          <button
            v-for="l in editingPeriod.ledgers"
            :key="l.ledgerPeriodId"
            type="button"
            class="ap-led"
            :class="periodStatusClass(l.periodStatus)"
            @click="dialog = null; openInfo(l.ledgerPeriodId)"
          >
            <i></i>{{ l.ledgerCode }} · {{ codeLabel(l.periodStatus) }}
          </button>
        </div>
      </div>
    </GlDialog>

    <!-- Ledger period details -->
    <GlDialog
      v-if="info"
      :title="`${info.ledgerCode} · ${info.periodCode}`"
      :subtitle="`Posting: ${codeLabel(info.postingMode).toLowerCase()}`"
      no-save
      :error="infoError"
      @close="info = null"
    >
      <div class="ap-kv cp-field--wide">
        <div><small>Status</small><b><span class="cp-pill" :class="periodStatusClass(info.periodStatus)">{{ codeLabel(info.periodStatus) }}</span></b></div>
        <div><small>Opened</small><b>{{ info.openedBy ?? '–' }}</b><small>{{ formatDateTime(info.openedAt) }}</small></div>
        <div>
          <small>{{ info.permanentlyClosedBy ? 'Closed permanently' : info.periodStatus === 'CLOSED' || !info.closedBy ? 'Closed' : 'Last closed' }}</small>
          <b>{{ info.permanentlyClosedBy ?? info.closedBy ?? '–' }}</b>
          <small>{{ formatDateTime(info.permanentlyClosedAt ?? info.closedAt) }}</small>
        </div>
      </div>
      <div class="ap-allow cp-field--wide">
        <span :class="{ on: info.isManualJournalAllowed }"><i class="pi" :class="info.isManualJournalAllowed ? 'pi-check' : 'pi-times'"></i>Manual journals</span>
        <span :class="{ on: info.isSubledgerAllowed }"><i class="pi" :class="info.isSubledgerAllowed ? 'pi-check' : 'pi-times'"></i>Subledger postings</span>
        <span :class="{ on: info.isReversalAllowed }"><i class="pi" :class="info.isReversalAllowed ? 'pi-check' : 'pi-times'"></i>Reversals</span>
        <span :class="{ on: info.isAdjustmentAllowed }"><i class="pi" :class="info.isAdjustmentAllowed ? 'pi-check' : 'pi-times'"></i>Adjustments</span>
      </div>
      <div v-if="info.canClose !== null" class="ap-ready cp-field--wide" :class="info.canClose ? 'ok' : 'no'">
        <b>{{ info.canClose ? 'Ready to close.' : 'Not ready to close.' }}</b> {{ info.canCloseMessage }}
      </div>
      <div v-if="info.periodStatus === 'CLOSED' && info.next && allowed(info.next.periodStatus, 'OPEN')" class="ap-ready ok cp-field--wide">
        <b>Next period:</b> {{ info.next.periodCode }} is {{ codeLabel(info.next.periodStatus).toLowerCase() }}.
        <button
          v-if="canControl"
          type="button"
          class="cp-btn cp-btn--small cp-btn--primary"
          @click="ask({ ledgerPeriodId: info.next.ledgerPeriodId, rowVersion: info.next.rowVersion, ledgerCode: info.ledgerCode, periodCode: info.next.periodCode, action: 'OPEN' }); info = null"
        >
          Open {{ info.next.periodCode }}
        </button>
      </div>
      <div v-if="info.closeNotes" class="ap-notes cp-field--wide"><small>Notes</small>{{ info.closeNotes }}</div>
      <div v-if="canControl" class="ap-acts cp-field--wide">
        <button
          v-for="a in ACTIONS.filter(x => allowed(info!.periodStatus, x.action))"
          :key="a.action"
          type="button"
          class="cp-btn"
          :class="a.cls"
          @click="askFromInfo(a.action)"
        >
          {{ a.label }}
        </button>
        <span v-if="info.periodStatus === 'PERMANENTLY_CLOSED'" class="cp-muted">This period is closed permanently: nothing more can be done with it.</span>
      </div>
      <div v-if="info.history.length" class="ap-hist cp-field--wide">
        <h4>History</h4>
        <div v-for="(h, i) in info.history" :key="i">
          <span>
            <b>{{ codeLabel(h.actionCode) }}</b>
            {{ h.oldPeriodStatus ? codeLabel(h.oldPeriodStatus) : '–' }} → {{ codeLabel(h.newPeriodStatus) }}<template v-if="h.actionReason"> · {{ h.actionReason }}</template>
          </span>
          <small>{{ h.actionBy }} · {{ formatDateTime(h.actionAt) }}</small>
        </div>
      </div>
    </GlDialog>

    <!-- Confirm an action -->
    <GlDialog
      v-if="pending"
      :title="`${pendingMeta.label} · ${pending.ledgerCode} ${pending.periodCode}`"
      :subtitle="pending.action === 'PERMANENT' ? 'This cannot be undone.' : undefined"
      :saving="acting"
      :error="actionError"
      :save-label="pendingMeta.label"
      @close="closeAction"
      @save="runAction"
    >
      <p class="ap-confirm cp-field--wide">
        {{ pending.intro ?? `${pendingMeta.label}: ${pending.periodCode} for ledger ${pending.ledgerCode}?` }}
      </p>
      <div class="cp-field cp-field--wide">
        <label for="a-reason">Reason / notes{{ reasonRequired ? ' *' : '' }}</label>
        <textarea id="a-reason" v-model="reason" rows="3" maxlength="1000" :placeholder="reasonRequired ? 'Required' : 'Optional'"></textarea>
      </div>
    </GlDialog>
  </div>
</template>

<style scoped>
.ap-none { display: flex; flex-direction: column; gap: 6px; }
.ap-none b { color: var(--cap-heading); }
.ap-info { display: flex; flex-wrap: wrap; gap: 14px 34px; padding: 16px 22px; margin-bottom: 14px; }
.ap-info small, .ap-kv small, .ap-notes small { display: block; color: var(--cap-muted); font-size: 11px; font-weight: 500; text-transform: uppercase; letter-spacing: .04em; }
.ap-info b { display: block; margin-top: 4px; color: var(--cap-heading); font-size: 14px; font-weight: 600; }
.ap-info .ar { color: var(--cap-muted); font-weight: 500; }
.ap-info__ledgers { margin-left: auto; }
.ap-info__ledgers > div { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; margin-top: 4px; font-size: 13px; }
.ap-tag { display: inline-flex; align-items: center; height: 22px; padding: 0 8px; border: 1px solid var(--cap-border); border-radius: 6px; background: var(--cap-bg); color: var(--cap-ink-soft); font-size: 11px; font-weight: 600; white-space: nowrap; }
.ap-link { margin-left: 4px; color: var(--cap-primary); font-size: 12px; font-weight: 600; }
.ap-alert { display: flex; flex-wrap: wrap; align-items: center; gap: 8px 12px; margin-bottom: 14px; padding: 12px 16px; border: 1px solid var(--cap-warning-border); border-radius: 12px; background: var(--cap-warning-bg); color: var(--cap-warning); font-size: 13px; }
.ap-alert > div { flex: 1; min-width: 200px; }
.ap-alert b { display: block; color: var(--cap-heading); }
.ap-layout { display: grid; grid-template-columns: 250px minmax(0, 1fr); gap: 16px; align-items: start; }
.ap-years { display: flex; flex-direction: column; gap: 10px; }
.ap-year { display: flex; flex-direction: column; gap: 4px; padding: 14px 16px; border: 1px solid var(--cap-border-soft); border-radius: 14px; background: #fff; text-align: start; font: inherit; cursor: pointer; box-shadow: 0 4px 14px rgba(13, 42, 46, .04); }
.ap-year:hover { border-color: var(--cap-primary-tint-2); }
.ap-year.on { border-color: var(--cap-primary); box-shadow: 0 0 0 2px var(--cap-primary-tint-2); }
.ap-year__top { display: flex; justify-content: space-between; align-items: center; gap: 8px; }
.ap-year b { color: var(--cap-heading); font-size: 19px; font-weight: 700; }
.ap-year small { color: var(--cap-muted); font-size: 12px; }
.ap-bad { color: var(--cap-danger); font-weight: 600; }
.ap-panel { min-width: 0; }
.ap-empty-title { display: block; margin-bottom: 6px; color: var(--cap-heading); font-size: 15px; }
.ap-head { flex-wrap: wrap; }
.ap-head h3 small { margin-left: 8px; color: var(--cap-muted); font-size: 12px; font-weight: 500; }
.ap-warn { margin: 12px 16px 4px; padding: 10px 12px; border: 1px solid var(--cap-danger-border); border-radius: 10px; background: var(--cap-danger-bg); color: var(--cap-danger-dark); font-size: 12px; }
.ap-table td { vertical-align: middle; }
.ap-table tr.adj td { background: var(--cap-bg); }
.ap-table tr.now td { background: var(--cap-orange-tint); }
.ap-table tr.bad td { background: var(--cap-danger-bg); }
.ap-table tr.bad td:first-child { box-shadow: inset 3px 0 0 var(--cap-danger); }
.ap-table .num { font-variant-numeric: tabular-nums; white-space: nowrap; }
.ap-table .nowrap { white-space: nowrap; }
.ap-table .ar { display: block; color: var(--cap-muted); font-size: 11px; direction: rtl; text-align: start; }
.ap-code { font: 600 12px ui-monospace, Menlo, Consolas, monospace; color: var(--cap-primary-dark); }
.ap-today { display: inline-block; margin-left: 6px; padding: 2px 7px; border-radius: 999px; background: var(--cap-orange); color: #fff; font-size: 10px; font-weight: 700; vertical-align: middle; }
.ap-issues { margin: 6px 0 0; padding-left: 16px; color: var(--cap-danger); font-size: 11px; line-height: 1.45; }
.ap-small { font-size: 12px; }
.ap-lrow { display: flex; align-items: center; gap: 6px; margin: 2px 0; }
.ap-lrows-inline { display: flex; flex-wrap: wrap; gap: 6px; }
.ap-led { display: inline-flex; align-items: center; gap: 6px; height: 24px; padding: 0 9px; border: 1px solid var(--cap-border); border-radius: 7px; background: var(--cap-bg); color: var(--cap-ink-soft); font: inherit; font-size: 11px; font-weight: 600; white-space: nowrap; cursor: pointer; }
.ap-led:hover { border-color: var(--cap-primary); }
.ap-led i { width: 7px; height: 7px; border-radius: 50%; background: var(--cap-faint); }
.ap-led.cp-pill--ok i { background: var(--cap-success-strong); }
.ap-led.cp-pill--info i { background: var(--cap-primary); }
.ap-led.cp-pill--warn i { background: var(--cap-warning); }
.ap-led.cp-pill--bad i { background: var(--cap-danger); }
.ap-mini { height: 24px; padding: 0 9px; font-size: 11px; }
.ap-mini--open { color: var(--cap-primary); font-weight: 600; }
.ap-note { margin: 0; padding: 12px 20px 16px; color: var(--cap-muted); font-size: 12px; line-height: 1.5; border-top: 1px solid var(--cap-border-soft); }
.ap-off { opacity: .6; }
.ap-preview { display: grid; grid-template-columns: repeat(auto-fill, minmax(96px, 1fr)); gap: 6px; margin-top: 10px; }
.ap-preview span { display: flex; flex-direction: column; gap: 2px; padding: 7px 9px; border: 1px solid var(--cap-border-soft); border-radius: 8px; background: var(--cap-bg); color: var(--cap-muted); font-size: 11px; }
.ap-preview b { font: 600 11px ui-monospace, Menlo, Consolas, monospace; color: var(--cap-primary-dark); }
.ap-hint-top { margin-top: 24px; }
.ap-kv { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 10px; }
.ap-kv > div { min-width: 0; padding: 10px 12px; border: 1px solid var(--cap-border-soft); border-radius: 10px; background: var(--cap-bg); }
.ap-kv b { display: block; margin: 3px 0; color: var(--cap-heading); font-size: 13px; overflow-wrap: anywhere; }
.ap-kv small + b + small { text-transform: none; letter-spacing: 0; }
.ap-allow { display: flex; flex-wrap: wrap; gap: 6px 16px; color: var(--cap-muted); font-size: 12px; }
.ap-allow span { display: inline-flex; align-items: center; gap: 6px; }
.ap-allow span.on { color: var(--cap-success); font-weight: 600; }
.ap-allow i { font-size: 11px; }
.ap-ready { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; padding: 10px 12px; border: 1px solid var(--cap-border-soft); border-radius: 10px; font-size: 12px; }
.ap-ready.ok { border-color: var(--cap-success-border); background: var(--cap-success-bg); color: var(--cap-success); }
.ap-ready.no { border-color: var(--cap-warning-border); background: var(--cap-warning-bg); color: var(--cap-warning); }
.ap-notes { padding: 10px 12px; border-radius: 10px; background: var(--cap-bg); color: var(--cap-ink-soft); font-size: 13px; }
.ap-acts { display: flex; flex-wrap: wrap; gap: 8px; }
.ap-hist { padding-top: 12px; border-top: 1px solid var(--cap-border-soft); }
.ap-hist h4 { margin: 0 0 6px; color: var(--cap-muted); font-size: 11px; font-weight: 600; letter-spacing: .06em; text-transform: uppercase; }
.ap-hist > div { display: flex; justify-content: space-between; gap: 10px; padding: 6px 0; border-bottom: 1px dashed var(--cap-border-soft); font-size: 12px; color: var(--cap-ink-soft); }
.ap-hist > div:last-child { border-bottom: 0; }
.ap-hist b { color: var(--cap-heading); }
.ap-hist small { color: var(--cap-muted); white-space: nowrap; }
.ap-confirm { margin: 0; color: var(--cap-ink); font-size: 14px; }
.apex-ref { margin: 12px 0 0; color: var(--cap-faint); font-size: 11px; text-align: end; }
@media (max-width: 980px) {
  .ap-layout { grid-template-columns: 1fr; }
  .ap-years { flex-direction: row; overflow-x: auto; }
  .ap-year { min-width: 200px; }
  .ap-info__ledgers { margin-left: 0; }
}
@media (max-width: 640px) {
  .ap-kv { grid-template-columns: 1fr; }
  .ap-hist > div { flex-direction: column; gap: 2px; }
}
</style>
