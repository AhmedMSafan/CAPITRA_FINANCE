<script setup lang="ts">
// Journal entry (APEX App 120 page 50): enter a manual journal line by line, then submit it, approve it (another user),
// post it, cancel it or reverse it. Every account is entered segment by segment, whatever the account structure.
import { computed, reactive, ref, shallowRef, watch } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { showSuccess } from '@/shared/feedback/feedback'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { formatDate, formatDateTime, todayInCairo } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import { ApiError } from '@/shared/api/http'
import GlDialog from '../components/GlDialog.vue'
import GlPageHeader from '../components/GlPageHeader.vue'
import { codeLabel } from '../api/glSetup.api'
import { periodStatusClass } from '../api/glPeriods.api'
import {
  drCr, journalsApi, journalStatusClass, STAGE_LABELS,
  type Journal, type JournalAction, type JournalEntrySetup, type JournalSegment, type SaveJournal
} from '../api/glJournals.api'

const props = defineProps<{ id?: string }>()
const router = useRouter()

const canEdit = hasPermission('GL.JOURNAL.EDIT')
const canApprove = hasPermission('GL.JOURNAL.APPROVE')
const canPost = hasPermission('GL.JOURNAL.POST')

const setup = shallowRef<JournalEntrySetup | null>(null)
const journal = shallowRef<Journal | null>(null)
const loadError = ref('')
useFeedback({ errors: [loadError] })

// Vue turns the value of a number input into a number, so an amount is a number or, when empty, ''.
type EditLine = { key: number; segments: Record<number, string>; dr: string | number; cr: string | number; description: string }

const form = reactive({
  ledgerId: null as number | null,
  categoryCode: 'STANDARD',
  accountingDate: todayInCairo(),
  journalDate: '',
  journalName: '',
  description: '',
  referenceNo: ''
})
const lines = ref<EditLine[]>([])
const fieldErrors = ref<Record<string, string>>({})
const lineErrors = ref<Record<number, string>>({})
const formError = ref('')
const saving = ref(false)
let nextKey = 1

const ledger = computed(() => setup.value?.ledgers.find(l => l.ledgerId === form.ledgerId) ?? null)
const structure = computed(() => setup.value?.structures.find(s => s.coaId === ledger.value?.coaId) ?? null)
/** Segments with a FIXED default are filled by the server and not typed. */
const segments = computed(() => structure.value?.segments.filter(s => !(s.defaultMode === 'FIXED' && s.defaultValueCode)) ?? [])
const fixedSegments = computed(() => structure.value?.segments.filter(s => s.defaultMode === 'FIXED' && s.defaultValueCode) ?? [])
const editable = computed(() => canEdit && (!journal.value || journal.value.allowed.edit))

/** The ledger period of the accounting date: the regular period, not the adjustment one. */
const period = computed(() => {
  const date = form.accountingDate
  if (!date || !form.ledgerId) return null
  return setup.value?.periods
    .filter(p => p.ledgerId === form.ledgerId && p.startDate <= date && p.endDate >= date)
    .sort((a, b) => Number(a.isAdjustmentPeriod) - Number(b.isAdjustmentPeriod))[0] ?? null
})

function text(value: string | number | null | undefined): string {
  return value === null || value === undefined ? '' : String(value).trim()
}

function amount(value: string | number): number {
  const n = Number(text(value))
  return text(value) !== '' && Number.isFinite(n) ? n : 0
}

const totals = computed(() => {
  if (!editable.value && journal.value) return { dr: journal.value.totalDr, cr: journal.value.totalCr }
  return {
    dr: lines.value.reduce((sum, l) => sum + amount(l.dr), 0),
    cr: lines.value.reduce((sum, l) => sum + amount(l.cr), 0)
  }
})
const difference = computed(() => Math.round((totals.value.dr - totals.value.cr) * 100) / 100)

function newLine(copyFrom?: EditLine): EditLine {
  const values: Record<number, string> = {}
  for (const s of segments.value) {
    if (s.isAccount) values[s.segmentNum] = ''
    else values[s.segmentNum] = copyFrom?.segments[s.segmentNum] ?? s.defaultValueCode ?? ''
  }
  return { key: nextKey++, segments: values, dr: '', cr: '', description: '' }
}

function addLine() {
  lines.value.push(newLine(lines.value[lines.value.length - 1]))
}

function removeLine(index: number) {
  lines.value.splice(index, 1)
  lineErrors.value = {}
}

/** Puts on the last line whatever makes debits equal credits. */
function fillDifference() {
  const last = lines.value[lines.value.length - 1]
  if (!last) return
  const others = lines.value.slice(0, -1)
  const net = others.reduce((sum, l) => sum + amount(l.dr) - amount(l.cr), 0)
  const value = Math.round(Math.abs(net) * 100) / 100
  last.dr = net < 0 ? String(value) : ''
  last.cr = net > 0 ? String(value) : ''
}

function onAmount(line: EditLine, side: 'dr' | 'cr') {
  if (side === 'dr' && text(line.dr) !== '') line.cr = ''
  if (side === 'cr' && text(line.cr) !== '') line.dr = ''
}

function valueName(segment: JournalSegment, code: string | undefined): string {
  if (!code) return ''
  return segment.values.find(v => v.code === code.trim().toUpperCase())?.name ?? ''
}

function onLedgerChange() {
  lines.value = [newLine(), newLine()]
}

// ---------------------------------------------------------------- load

function fillForm(j: Journal | null) {
  const s = setup.value
  if (!j) {
    Object.assign(form, {
      ledgerId: s?.defaultLedgerId ?? null, categoryCode: s?.defaultCategory ?? 'STANDARD', accountingDate: s?.today ?? todayInCairo(),
      journalDate: '', journalName: '', description: '', referenceNo: ''
    })
    lines.value = [newLine(), newLine()]
  } else {
    Object.assign(form, {
      ledgerId: j.ledgerId, categoryCode: j.categoryCode, accountingDate: j.accountingDate, journalDate: j.journalDate,
      journalName: j.journalName, description: j.description ?? '', referenceNo: j.referenceNo ?? ''
    })
    lines.value = j.lines.map(l => ({
      key: nextKey++,
      segments: Object.fromEntries(segments.value.map(s => [s.segmentNum, l.segments[s.segmentNum] ?? ''])),
      dr: l.dr ? String(l.dr) : '',
      cr: l.cr ? String(l.cr) : '',
      description: l.description ?? ''
    }))
  }
  fieldErrors.value = {}
  lineErrors.value = {}
  formError.value = ''
}

async function load() {
  try {
    if (!setup.value) setup.value = await journalsApi.setup()
    journal.value = props.id ? await journalsApi.get(Number(props.id)) : null
    fillForm(journal.value)
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'The journal could not be loaded.'
  }
}

watch(() => props.id, id => {
  if (journal.value && String(journal.value.journalHeaderId) === id) return
  journal.value = null
  load()
})

// ---------------------------------------------------------------- save

function readErrors(e: unknown) {
  fieldErrors.value = {}
  lineErrors.value = {}
  formError.value = ''
  if (e instanceof ApiError && e.problem?.errors) {
    for (const [key, messages] of Object.entries(e.problem.errors)) {
      const m = /^lines\[(\d+)\]$/i.exec(key)
      if (m) lineErrors.value[Number(m[1])] = messages.join(' ')
      else if (['journalName', 'accountingDate', 'ledgerId', 'categoryCode', 'description', 'referenceNo'].includes(key)) fieldErrors.value[key] = messages.join(' ')
      else formError.value = [formError.value, ...messages].filter(Boolean).join(' ')
    }
    if (!formError.value) formError.value = 'Correct the fields marked in red.'
  } else {
    formError.value = e instanceof Error ? e.message : 'The journal could not be saved.'
  }
}

async function save(submit: boolean) {
  saving.value = true
  try {
    const body: SaveJournal = {
      ledgerId: form.ledgerId,
      categoryCode: form.categoryCode,
      accountingDate: form.accountingDate || null,
      journalDate: form.journalDate || null,
      journalName: form.journalName,
      description: form.description || null,
      referenceNo: form.referenceNo || null,
      lines: lines.value.map(l => ({
        segments: Object.fromEntries(Object.entries(l.segments).map(([k, v]) => [Number(k), v.trim() ? v.trim().toUpperCase() : null])),
        dr: text(l.dr) ? Number(text(l.dr)) : null,
        cr: text(l.cr) ? Number(text(l.cr)) : null,
        description: l.description.trim() || null
      })),
      submit,
      rowVersion: journal.value?.rowVersion ?? null
    }
    const result = journal.value ? await journalsApi.update(journal.value.journalHeaderId, body) : await journalsApi.create(body)
    const wasNew = !journal.value
    journal.value = result.journal
    fillForm(result.journal)
    if (wasNew) await router.replace(`/gl/journals/${result.journal.journalHeaderId}`)
    if (result.submitError) formError.value = `Saved as a draft, but not submitted: ${result.submitError}`
    else showSuccess(submit ? `Journal ${result.journal.journalNo} saved and submitted for approval.` : `Journal ${result.journal.journalNo} saved as a draft.`)
  } catch (e) {
    readErrors(e)
  } finally {
    saving.value = false
  }
}

// ---------------------------------------------------------------- actions

type ActionSpec = { action: JournalAction; title: string; text: string; button: string; reason: 'required' | 'optional' | 'none'; danger?: boolean }

const ACTIONS: Record<JournalAction, ActionSpec> = {
  submit: { action: 'submit', title: 'Submit for approval', text: 'The lines are locked and another user approves the journal.', button: 'Submit', reason: 'none' },
  approve: { action: 'approve', title: 'Approve journal', text: 'After approval the journal can be posted to the ledger.', button: 'Approve', reason: 'optional' },
  reject: { action: 'reject', title: 'Reject journal', text: 'The journal goes back to its owner to be corrected and submitted again.', button: 'Reject', reason: 'required', danger: true },
  post: { action: 'post', title: 'Post to ledger', text: 'The account balances are updated. A posted journal cannot be changed; it is corrected by a reversal.', button: 'Post', reason: 'none' },
  cancel: { action: 'cancel', title: 'Cancel journal', text: 'A cancelled journal is kept for the record but never posts.', button: 'Cancel journal', reason: 'required', danger: true },
  reverse: { action: 'reverse', title: 'Reverse journal', text: 'A draft journal with debit and credit switched is created. It is submitted, approved and posted like any journal.', button: 'Create reversal', reason: 'required' }
}

const pending = ref<ActionSpec | null>(null)
const actionReason = ref('')
const reversalDate = ref('')
const actionError = ref('')
const acting = ref(false)

const buttons = computed(() => {
  const j = journal.value
  if (!j || editable.value) return []
  const a = j.allowed
  const list: ActionSpec[] = []
  if (a.submit && canEdit) list.push(ACTIONS.submit)
  if (a.approve && canApprove) list.push(ACTIONS.approve)
  if (a.post && canPost) list.push(ACTIONS.post)
  if (a.reverse && canEdit) list.push(ACTIONS.reverse)
  if (a.reject && canApprove) list.push(ACTIONS.reject)
  if (a.cancel && canEdit) list.push(ACTIONS.cancel)
  return list
})

function openAction(spec: ActionSpec) {
  pending.value = spec
  actionReason.value = ''
  reversalDate.value = setup.value?.today ?? todayInCairo()
  actionError.value = ''
}

async function runAction() {
  const j = journal.value
  const spec = pending.value
  if (!j || !spec) return
  if (spec.reason === 'required' && !actionReason.value.trim()) {
    actionError.value = 'Enter the reason.'
    return
  }
  acting.value = true
  actionError.value = ''
  try {
    const result = await journalsApi.act(j.journalHeaderId, spec.action, {
      reason: actionReason.value.trim() || null,
      reversalDate: spec.action === 'reverse' ? reversalDate.value || null : null,
      rowVersion: j.rowVersion
    })
    pending.value = null
    if (result.created) {
      showSuccess(result.message)
      await router.push(`/gl/journals/${result.created.journalHeaderId}`)
      return
    }
    journal.value = result.journal
    fillForm(result.journal)
    showSuccess(result.message)
  } catch (e) {
    const problem = e instanceof ApiError ? e.problem?.errors : undefined
    actionError.value = problem ? Object.values(problem).flat().join(' ') : e instanceof Error ? e.message : 'The action failed.'
    // A failed post is recorded on the journal: show it.
    if (spec.action === 'post') journal.value = await journalsApi.get(j.journalHeaderId).catch(() => j)
  } finally {
    acting.value = false
  }
}

function closeAction() {
  if (!acting.value) pending.value = null
}

const ACTION_LABELS: Record<string, string> = {
  CREATE: 'Created', UPDATE: 'Changed', SUBMIT: 'Submitted', APPROVE: 'Approved', REJECT: 'Rejected', POST: 'Posted',
  POST_FAILED: 'Posting failed', CANCEL: 'Cancelled', REVERSE: 'Reversal created', REVERSED: 'Reversed'
}

const statusLabel = computed(() => (journal.value ? STAGE_LABELS[journal.value.journalStatus] ?? codeLabel(journal.value.journalStatus) : 'New'))

load()
</script>

<template>
  <div class="cp-page">
    <GlPageHeader
      :title="journal ? `Journal ${journal.journalNo}` : 'New journal'"
      :description="journal ? journal.journalName : 'Enter the lines, save a draft at any time, then submit it: another user approves it before it is posted.'"
      icon="pi pi-file-edit"
    >
      <RouterLink to="/gl/journals" class="cp-btn"><i class="pi pi-list"></i><span>Journals</span></RouterLink>
      <RouterLink v-if="journal && canEdit" to="/gl/journals/new" class="cp-btn cp-btn--create"><i class="pi pi-plus"></i><span>New journal</span></RouterLink>
    </GlPageHeader>

    <div v-if="!setup" class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div>

    <template v-else>
      <div v-if="!setup.ledgers.length" class="cp-card cp-empty">
        No active ledger with an account structure. Set one up on the <RouterLink to="/gl/ledgers">Ledgers</RouterLink> page.
      </div>

      <template v-else>
        <!-- Status and what the journal needs next -->
        <section v-if="journal" class="cp-card je-status">
          <div class="je-status__main">
            <span class="cp-pill" :class="journalStatusClass(journal.journalStatus)">{{ statusLabel }}</span>
            <span class="je-meta">{{ journal.ledgerCode }} · {{ journal.periodCode }} · {{ journal.sourceCode }} · Batch {{ journal.batchNo }}</span>
            <span v-if="journal.tranSetId" class="je-meta">TRAN_SET_ID {{ journal.tranSetId }}</span>
          </div>
          <ol class="je-steps" aria-label="Progress">
            <li :class="{ done: !!journal.createdAt }"><b>Entered</b><span>{{ journal.createdBy }} · {{ formatDateTime(journal.createdAt) }}</span></li>
            <li :class="{ done: !!journal.submittedAt }"><b>Submitted</b><span>{{ journal.submittedBy ? `${journal.submittedBy} · ${formatDateTime(journal.submittedAt)}` : '–' }}</span></li>
            <li :class="{ done: !!journal.approvedAt }"><b>Approved</b><span>{{ journal.approvedBy ? `${journal.approvedBy} · ${formatDateTime(journal.approvedAt)}` : 'by another user' }}</span></li>
            <li :class="{ done: !!journal.postedAt }"><b>Posted</b><span>{{ journal.postedBy ? `${journal.postedBy} · ${formatDateTime(journal.postedAt)}` : '–' }}</span></li>
          </ol>
        </section>

        <div v-if="journal?.rejectReason" class="je-note je-note--bad"><i class="pi pi-undo"></i><span><b>Rejected:</b> {{ journal.rejectReason }} Correct the journal and submit it again.</span></div>
        <div v-if="journal?.lastPostError" class="je-note je-note--bad"><i class="pi pi-exclamation-triangle"></i><span><b>Last posting failed:</b> {{ journal.lastPostError }}</span></div>
        <div v-if="journal?.cancelReason && journal.journalStatus === 'CANCELLED'" class="je-note"><i class="pi pi-ban"></i><span><b>Cancelled</b> by {{ journal.cancelledBy }} on {{ formatDateTime(journal.cancelledAt) }}: {{ journal.cancelReason }}</span></div>
        <div v-if="journal?.allowed.approveBlockedReason && canApprove" class="je-note je-note--info"><i class="pi pi-users"></i><span>{{ journal.allowed.approveBlockedReason }}</span></div>
        <div v-if="journal?.reversalOf" class="je-note je-note--info"><i class="pi pi-replay"></i><span>This journal reverses <RouterLink :to="`/gl/journals/${journal.reversalOf.journalHeaderId}`">{{ journal.reversalOf.journalNo }}</RouterLink>. Its lines are the original lines with debit and credit switched.</span></div>
        <div v-if="journal?.reversedBy" class="je-note je-note--info"><i class="pi pi-replay"></i><span>Reversal <RouterLink :to="`/gl/journals/${journal.reversedBy.journalHeaderId}`">{{ journal.reversedBy.journalNo }}</RouterLink> is {{ (STAGE_LABELS[journal.reversedBy.journalStatus] ?? journal.reversedBy.journalStatus).toLowerCase() }}.</span></div>
        <div v-if="formError" class="cp-error je-error" role="alert">{{ formError }}</div>

        <!-- Header -->
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-file"></i><h3>Journal</h3></div>
          <div class="cp-card__body">
            <fieldset v-if="editable" class="cp-form je-head" :disabled="saving">
              <div class="cp-field">
                <label for="je-ledger">Ledger *</label>
                <select id="je-ledger" v-model="form.ledgerId" :disabled="!!journal" @change="onLedgerChange">
                  <option v-for="l in setup.ledgers" :key="l.ledgerId" :value="l.ledgerId">{{ l.ledgerCode }} - {{ l.ledgerNameEn }}</option>
                </select>
                <small v-if="fieldErrors.ledgerId" class="cp-field__error">{{ fieldErrors.ledgerId }}</small>
              </div>
              <div class="cp-field">
                <label for="je-date">Accounting date *</label>
                <input id="je-date" v-model="form.accountingDate" type="date" />
                <small v-if="fieldErrors.accountingDate" class="cp-field__error">{{ fieldErrors.accountingDate }}</small>
                <small v-else-if="period" class="cp-field__hint">Period {{ period.periodCode }} <span class="cp-pill je-mini" :class="periodStatusClass(period.periodStatus)">{{ codeLabel(period.periodStatus) }}</span></small>
                <small v-else-if="form.accountingDate" class="cp-field__error">No period of this ledger covers the date.</small>
              </div>
              <div class="cp-field">
                <label for="je-cat">Category</label>
                <select id="je-cat" v-model="form.categoryCode">
                  <option v-for="c in setup.categories" :key="c.code" :value="c.code">{{ c.name }}</option>
                </select>
              </div>
              <div class="cp-field">
                <label for="je-name">Journal name *</label>
                <input id="je-name" v-model="form.journalName" maxlength="240" placeholder="e.g. October rent accrual" />
                <small v-if="fieldErrors.journalName" class="cp-field__error">{{ fieldErrors.journalName }}</small>
              </div>
              <div class="cp-field">
                <label for="je-ref">Reference</label>
                <input id="je-ref" v-model="form.referenceNo" maxlength="120" placeholder="Invoice or document number" />
              </div>
              <div class="cp-field">
                <label for="je-jdate">Journal date</label>
                <input id="je-jdate" v-model="form.journalDate" type="date" />
                <small class="cp-field__hint">Defaults to the accounting date.</small>
              </div>
              <div class="cp-field cp-field--wide">
                <label for="je-desc">Description</label>
                <textarea id="je-desc" v-model="form.description" rows="2" maxlength="1000"></textarea>
              </div>
            </fieldset>
            <dl v-else-if="journal" class="je-view">
              <div><dt>Ledger</dt><dd>{{ journal.ledgerCode }}</dd></div>
              <div><dt>Accounting date</dt><dd>{{ formatDate(journal.accountingDate) }} <span class="cp-muted">({{ journal.periodCode }})</span></dd></div>
              <div><dt>Category</dt><dd>{{ setup.categories.find(c => c.code === journal!.categoryCode)?.name ?? codeLabel(journal.categoryCode) }}</dd></div>
              <div><dt>Currency</dt><dd>{{ journal.currencyCode }}</dd></div>
              <div><dt>Reference</dt><dd>{{ journal.referenceNo || '–' }}</dd></div>
              <div><dt>Journal date</dt><dd>{{ formatDate(journal.journalDate) }}</dd></div>
              <div v-if="journal.sourceDocumentNo"><dt>Source document</dt><dd>{{ journal.sourceDocumentType }} {{ journal.sourceDocumentNo }}</dd></div>
              <div class="je-view__wide"><dt>Description</dt><dd>{{ journal.description || '–' }}</dd></div>
            </dl>
          </div>
        </section>

        <!-- Lines -->
        <section class="cp-card je-lines">
          <div class="cp-card__head">
            <i class="pi pi-list"></i><h3>Lines</h3>
            <span class="cp-spacer"></span>
            <span v-if="ledger" class="cp-muted je-cur">{{ ledger.currencyCode }}</span>
          </div>
          <div class="cp-table-wrap">
            <table v-if="editable" class="cp-table je-table">
              <thead>
                <tr>
                  <th class="je-no">#</th>
                  <th v-for="s in segments" :key="s.segmentNum" :class="s.isAccount ? 'je-acc' : 'je-seg'">{{ s.segmentNameEn }}{{ s.isRequired ? ' *' : '' }}</th>
                  <th class="cp-num je-amt">Debit</th>
                  <th class="cp-num je-amt">Credit</th>
                  <th>Line description</th>
                  <th></th>
                </tr>
              </thead>
              <tbody>
                <template v-for="(l, i) in lines" :key="l.key">
                  <tr :class="{ 'je-bad': lineErrors[i] }">
                    <td class="je-no">{{ i + 1 }}</td>
                    <td v-for="s in segments" :key="s.segmentNum" :class="s.isAccount ? 'je-acc' : 'je-seg'">
                      <input v-model="l.segments[s.segmentNum]" class="je-in je-code" :list="`je-dl-${s.segmentNum}`" :aria-label="`Line ${i + 1} ${s.segmentNameEn}`" autocomplete="off" />
                      <small v-if="s.isAccount" class="je-name">{{ valueName(s, l.segments[s.segmentNum]) || ' ' }}</small>
                    </td>
                    <td class="je-amt"><input v-model="l.dr" class="je-in cp-num" type="number" min="0" step="0.01" :aria-label="`Line ${i + 1} debit`" @input="onAmount(l, 'dr')" /></td>
                    <td class="je-amt"><input v-model="l.cr" class="je-in cp-num" type="number" min="0" step="0.01" :aria-label="`Line ${i + 1} credit`" @input="onAmount(l, 'cr')" /></td>
                    <td><input v-model="l.description" class="je-in" maxlength="1000" :aria-label="`Line ${i + 1} description`" /></td>
                    <td><button type="button" class="je-x" :aria-label="`Remove line ${i + 1}`" @click="removeLine(i)"><i class="pi pi-trash"></i></button></td>
                  </tr>
                  <tr v-if="lineErrors[i]" class="je-err-row"><td></td><td :colspan="segments.length + 4">{{ lineErrors[i] }}</td></tr>
                </template>
              </tbody>
            </table>
            <table v-else-if="journal" class="cp-table je-table">
              <thead>
                <tr><th class="je-no">#</th><th>Account</th><th>Name</th><th class="cp-num">Debit</th><th class="cp-num">Credit</th><th>Line description</th></tr>
              </thead>
              <tbody>
                <tr v-for="l in journal.lines" :key="l.lineNo">
                  <td class="je-no">{{ l.lineNo }}</td>
                  <td><code class="je-combo">{{ l.concatenatedAccount }}</code></td>
                  <td>{{ l.accountNameEn }}</td>
                  <td class="cp-num">{{ l.dr ? formatAmount(l.dr) : '' }}</td>
                  <td class="cp-num">{{ l.cr ? formatAmount(l.cr) : '' }}</td>
                  <td class="je-wrap">{{ l.description }}</td>
                </tr>
                <tr v-if="!journal.lines.length"><td colspan="6" class="cp-muted">No lines.</td></tr>
              </tbody>
            </table>
            <datalist v-for="s in segments" :id="`je-dl-${s.segmentNum}`" :key="s.segmentNum">
              <option v-for="v in s.values" :key="v.code" :value="v.code">{{ v.name }}</option>
            </datalist>
          </div>
          <div v-if="editable" class="je-tools">
            <button type="button" class="cp-btn cp-btn--small" @click="addLine"><i class="pi pi-plus"></i><span>Add line</span></button>
            <button type="button" class="cp-btn cp-btn--small" :disabled="lines.length < 2" @click="fillDifference"><i class="pi pi-equals"></i><span>Put the difference on the last line</span></button>
            <span class="je-hint">A new line copies the segments of the line above, except the account.<template v-if="fixedSegments.length">&nbsp;{{ fixedSegments.map(s => `${s.segmentNameEn} is always ${s.defaultValueCode}`).join('; ') }}.</template></span>
          </div>
        </section>

        <!-- Totals and actions -->
        <section class="cp-card je-bar">
          <div class="je-totals">
            <div><span>Debits</span><b>{{ formatAmount(totals.dr) }}</b></div>
            <div><span>Credits</span><b>{{ formatAmount(totals.cr) }}</b></div>
            <div><span>Difference</span><b v-if="!totals.dr && !totals.cr" class="cp-muted">–</b>
              <b v-else :class="difference === 0 ? 'je-ok' : 'je-off'">{{ difference === 0 ? 'Balanced' : formatAmount(Math.abs(difference)) }}</b></div>
          </div>
          <span class="cp-spacer"></span>
          <template v-if="editable">
            <button type="button" class="cp-btn" :disabled="saving" @click="save(false)"><i class="pi pi-save"></i><span>Save draft</span></button>
            <button type="button" class="cp-btn cp-btn--primary" :disabled="saving" @click="save(true)">
              <i :class="saving ? 'pi pi-spin pi-spinner' : 'pi pi-send'"></i><span>Save and submit</span>
            </button>
            <button v-if="journal?.allowed.cancel" type="button" class="cp-btn cp-btn--danger" :disabled="saving" @click="openAction(ACTIONS.cancel)"><i class="pi pi-ban"></i><span>Cancel journal</span></button>
          </template>
          <template v-else>
            <button
              v-for="b in buttons" :key="b.action" type="button" class="cp-btn"
              :class="b.danger ? 'cp-btn--danger' : b === buttons[0] ? 'cp-btn--primary' : ''" @click="openAction(b)"
            >{{ b.button }}</button>
          </template>
        </section>

        <!-- Balance impact -->
        <section v-if="journal?.impacts.length" class="cp-card">
          <div class="cp-card__head">
            <i class="pi pi-chart-bar"></i><h3>Balance impact</h3><span class="cp-spacer"></span>
            <RouterLink to="/gl/account-ledger" class="cp-btn cp-btn--small"><i class="pi pi-arrow-right"></i><span>Account ledger</span></RouterLink>
          </div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>#</th><th>Account</th><th>Name</th><th>Period</th><th class="cp-num">Debit</th><th class="cp-num">Credit</th><th class="cp-num">Balance before</th><th class="cp-num">Balance after</th></tr></thead>
              <tbody>
                <tr v-for="i in journal.impacts" :key="i.lineNo">
                  <td>{{ i.lineNo }}</td><td><code class="je-combo">{{ i.concatenatedAccount }}</code></td><td>{{ i.accountNameEn }}</td><td>{{ i.periodCode }}</td>
                  <td class="cp-num">{{ i.dr ? formatAmount(i.dr) : '' }}</td><td class="cp-num">{{ i.cr ? formatAmount(i.cr) : '' }}</td>
                  <td class="cp-num">{{ drCr(i.balanceBefore) }}</td><td class="cp-num"><b>{{ drCr(i.balanceAfter) }}</b></td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <!-- History -->
        <section v-if="journal?.history.length" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-history"></i><h3>History</h3></div>
          <ul class="je-history">
            <li v-for="(h, i) in journal.history" :key="i">
              <b>{{ ACTION_LABELS[h.actionCode] ?? codeLabel(h.actionCode) }}</b>
              <span class="cp-muted">{{ h.by }} · {{ formatDateTime(h.at) }}</span>
              <span v-if="h.reason" class="je-history__reason">{{ h.reason }}</span>
            </li>
          </ul>
        </section>
      </template>
    </template>

    <p class="apex-ref">APEX App 120 page 50</p>

    <GlDialog
      v-if="pending && journal"
      :title="`${pending.title} ${journal.journalNo}`"
      :subtitle="pending.text"
      :saving="acting"
      :error="actionError"
      :save-label="pending.button"
      @close="closeAction"
      @save="runAction"
    >
      <dl class="cp-kv cp-field--wide je-kv">
        <dt>Journal</dt><dd>{{ journal.journalName }}</dd>
        <dt>Amount</dt><dd>{{ formatAmount(journal.totalDr) }} {{ journal.currencyCode }}</dd>
        <dt>Accounting date</dt><dd>{{ formatDate(journal.accountingDate) }}</dd>
      </dl>
      <div v-if="pending.action === 'reverse'" class="cp-field">
        <label for="je-rdate">Reversal date *</label>
        <input id="je-rdate" v-model="reversalDate" type="date" />
        <small class="cp-field__hint">Its period must be open.</small>
      </div>
      <div v-if="pending.reason !== 'none'" class="cp-field cp-field--wide">
        <label for="je-reason">{{ pending.reason === 'required' ? 'Reason *' : 'Notes' }}</label>
        <textarea id="je-reason" v-model="actionReason" rows="3" maxlength="1000"></textarea>
      </div>
    </GlDialog>
  </div>
</template>

<style scoped>
.je-status { display: flex; flex-wrap: wrap; align-items: center; gap: 14px 24px; margin-bottom: 14px; padding: 14px 20px; }
.je-status__main { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; }
.je-meta { color: var(--cap-muted); font-size: 12px; }
.je-steps { flex: 1; display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 8px; margin: 0; padding: 0; list-style: none; min-width: 420px; }
.je-steps li { padding: 6px 10px; border-top: 3px solid var(--cap-border); }
.je-steps li.done { border-top-color: var(--cap-primary); }
.je-steps b { display: block; color: var(--cap-heading); font-size: 12px; font-weight: 600; }
.je-steps span { display: block; color: var(--cap-muted); font-size: 11px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.je-note { display: flex; align-items: flex-start; gap: 10px; margin-bottom: 12px; padding: 10px 14px; border: 1px solid var(--cap-border); border-radius: 10px; background: var(--cap-bg); color: var(--cap-ink-soft); font-size: 13px; }
.je-note i { margin-top: 2px; }
.je-note a { color: var(--cap-primary); font-weight: 600; }
.je-note--bad { border-color: var(--cap-danger-border); background: var(--cap-danger-bg); color: var(--cap-danger); }
.je-note--info { border-color: var(--cap-border); background: var(--cap-primary-tint); color: var(--cap-primary-dark); }
.je-error { margin-bottom: 12px; }
.cp-card + .cp-card, .je-lines, .je-bar { margin-top: 14px; }
.je-head { grid-template-columns: repeat(3, minmax(0, 1fr)); }
.je-mini { margin-left: 4px; padding: 1px 7px; font-size: 10px; }
.je-view { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 12px 24px; margin: 0; }
.je-view dt { color: var(--cap-muted); font-size: 11px; }
.je-view dd { margin: 2px 0 0; color: var(--cap-ink); font-size: 13px; font-weight: 500; }
.je-view__wide { grid-column: 1 / -1; }
.je-cur { font-size: 12px; font-weight: 600; }
.je-table td { vertical-align: top; }
.je-table .je-no { width: 34px; color: var(--cap-muted); padding-top: 18px; }
.je-acc { min-width: 190px; }
.je-seg { min-width: 96px; }
.je-amt { min-width: 130px; }
.je-in { width: 100%; height: 36px; padding: 0 10px; border: 1px solid var(--cap-border); border-radius: 7px; background: #fff; color: var(--cap-ink); font: inherit; font-size: 13px; }
.je-in:focus { outline: none; border-color: var(--cap-primary); box-shadow: 0 0 0 2px rgba(15, 118, 110, .1); }
.je-code { text-transform: uppercase; font-family: ui-monospace, Menlo, Consolas, monospace; }
.je-name { display: block; min-height: 15px; margin-top: 3px; color: var(--cap-muted); font-size: 11px; white-space: normal; }
.je-bad .je-in { border-color: var(--cap-danger-border); }
.je-err-row td { padding-top: 0; border-bottom: 1px solid var(--cap-border-soft); color: var(--cap-danger); font-size: 12px; white-space: normal; }
.je-bad td { border-bottom: 0; }
.je-x { width: 34px; height: 36px; border: 0; border-radius: 7px; background: transparent; color: var(--cap-faint); cursor: pointer; }
.je-x:hover { background: var(--cap-danger-bg); color: var(--cap-danger); }
.je-combo { font: 600 12px ui-monospace, Menlo, Consolas, monospace; color: var(--cap-primary-dark); }
.je-wrap { white-space: normal; }
.je-tools { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; padding: 12px 20px 16px; border-top: 1px solid var(--cap-border-soft); }
.je-hint { color: var(--cap-muted); font-size: 12px; }
.je-bar { position: sticky; bottom: 12px; z-index: 5; display: flex; flex-wrap: wrap; align-items: center; gap: 10px; padding: 12px 20px; }
.je-totals { display: flex; gap: 28px; }
.je-totals span { display: block; color: var(--cap-muted); font-size: 11px; text-transform: uppercase; letter-spacing: .04em; }
.je-totals b { color: var(--cap-heading); font-size: 16px; font-variant-numeric: tabular-nums; }
.je-totals .je-ok { color: var(--cap-success); }
.je-totals .je-off { color: var(--cap-danger); }
.je-history { margin: 0; padding: 6px 20px 14px; list-style: none; }
.je-history li { display: flex; flex-wrap: wrap; gap: 4px 12px; padding: 9px 0; border-bottom: 1px solid var(--cap-border-soft); font-size: 13px; }
.je-history li:last-child { border-bottom: 0; }
.je-history b { min-width: 130px; color: var(--cap-heading); font-weight: 600; }
.je-history__reason { flex-basis: 100%; padding-left: 142px; color: var(--cap-ink-soft); font-size: 12px; }
.je-kv { margin-bottom: 4px; }
.apex-ref { margin: 12px 0 0; color: var(--cap-faint); font-size: 11px; text-align: end; }
@media (max-width: 900px) {
  .je-head, .je-view { grid-template-columns: 1fr 1fr; }
  .je-steps { min-width: 0; grid-template-columns: 1fr 1fr; }
  .je-history__reason { padding-left: 0; }
}
@media (max-width: 600px) {
  .je-head, .je-view { grid-template-columns: 1fr; }
  .je-totals { gap: 16px; }
  .je-bar { position: static; }
}
</style>
