<script setup lang="ts">
// Journals (APEX App 120 page 51): find any journal by number, TRAN_SET_ID, account or segment values,
// open it, or submit, approve, post, reject or cancel several at once.
import { computed, reactive, ref, shallowRef } from 'vue'
import { RouterLink } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import { ApiError } from '@/shared/api/http'
import GlDialog from '../components/GlDialog.vue'
import GlPageHeader from '../components/GlPageHeader.vue'
import { codeLabel } from '../api/glSetup.api'
import {
  journalsApi, journalStatusClass, STAGE_LABELS,
  type BulkAction, type BulkResult, type JournalFilters, type JournalSearchResult
} from '../api/glJournals.api'

const canEdit = hasPermission('GL.JOURNAL.EDIT')
const canApprove = hasPermission('GL.JOURNAL.APPROVE')
const canPost = hasPermission('GL.JOURNAL.POST')

const result = shallowRef<JournalSearchResult | null>(null)
const loadError = ref('')
const searching = ref(false)
useFeedback({ errors: [loadError] })

function emptyFilters(): JournalFilters {
  return { stage: '', q: '', tran: '', ledgerId: '', source: '', category: '', period: '', from: '', to: '', minAmount: '', maxAmount: '', createdBy: '', segments: {} }
}

const filters = reactive<JournalFilters>(emptyFilters())
const moreFilters = ref(false)
const selected = ref<Set<number>>(new Set())

async function search() {
  searching.value = true
  try {
    result.value = await journalsApi.search(filters)
    const ids = new Set(result.value.rows.map(r => r.journalHeaderId))
    selected.value = new Set([...selected.value].filter(id => ids.has(id)))
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'The journals could not be loaded.'
  } finally {
    searching.value = false
  }
}

function setStage(stage: string) {
  filters.stage = stage
  search()
}

function clearFilters() {
  Object.assign(filters, emptyFilters(), { stage: filters.stage })
  search()
}

const activeFilters = computed(() =>
  [filters.source, filters.category, filters.tran, filters.minAmount, filters.maxAmount, filters.createdBy, ...Object.values(filters.segments)]
    .filter(v => v && String(v).trim()).length)

/** The chips: what needs someone first, then every status. */
const STAGES: { code: string; label: string }[] = [
  { code: '', label: 'All' },
  { code: 'MINE', label: 'Waiting my approval' },
  { code: 'DRAFT', label: STAGE_LABELS.DRAFT! },
  { code: 'SUBMITTED', label: STAGE_LABELS.SUBMITTED! },
  { code: 'APPROVED', label: STAGE_LABELS.APPROVED! },
  { code: 'REJECTED', label: STAGE_LABELS.REJECTED! },
  { code: 'POSTED', label: STAGE_LABELS.POSTED! },
  { code: 'REVERSED', label: STAGE_LABELS.REVERSED! },
  { code: 'CANCELLED', label: STAGE_LABELS.CANCELLED! }
]

function count(code: string): number {
  const counts = result.value?.counts ?? {}
  if (code === '') return Object.entries(counts).filter(([k]) => k !== 'MINE').reduce((sum, [, n]) => sum + n, 0)
  return counts[code] ?? 0
}

// ---------------------------------------------------------------- selection and bulk actions

const rows = computed(() => result.value?.rows ?? [])
const allSelected = computed(() => rows.value.length > 0 && rows.value.every(r => selected.value.has(r.journalHeaderId)))

function toggle(id: number) {
  const next = new Set(selected.value)
  if (next.has(id)) next.delete(id)
  else next.add(id)
  selected.value = next
}

function toggleAll() {
  selected.value = allSelected.value ? new Set() : new Set(rows.value.map(r => r.journalHeaderId))
}

type BulkSpec = { action: BulkAction; label: string; needs: string[]; reason: boolean; danger?: boolean; allowed: boolean }

const BULK: BulkSpec[] = [
  { action: 'submit', label: 'Submit', needs: ['DRAFT', 'REJECTED'], reason: false, allowed: canEdit },
  { action: 'approve', label: 'Approve', needs: ['SUBMITTED'], reason: false, allowed: canApprove },
  { action: 'post', label: 'Post', needs: ['APPROVED'], reason: false, allowed: canPost },
  { action: 'reject', label: 'Reject', needs: ['SUBMITTED'], reason: true, danger: true, allowed: canApprove },
  { action: 'cancel', label: 'Cancel', needs: ['DRAFT', 'REJECTED', 'SUBMITTED', 'APPROVED'], reason: true, danger: true, allowed: canEdit }
]

const selectedRows = computed(() => rows.value.filter(r => selected.value.has(r.journalHeaderId)))
const bulkButtons = computed(() => BULK.filter(b => b.allowed && selectedRows.value.some(r => b.needs.includes(r.stage))))

const pending = ref<BulkSpec | null>(null)
const reason = ref('')
const bulkError = ref('')
const running = ref(false)
const bulkResult = ref<BulkResult | null>(null)

function openBulk(spec: BulkSpec) {
  pending.value = spec
  reason.value = ''
  bulkError.value = ''
}

/** Journals of the selection the action applies to; the others are left alone. */
const targets = computed(() => (pending.value ? selectedRows.value.filter(r => pending.value!.needs.includes(r.stage)) : []))

async function runBulk() {
  const spec = pending.value
  if (!spec) return
  if (spec.reason && !reason.value.trim()) {
    bulkError.value = 'Enter the reason.'
    return
  }
  running.value = true
  try {
    bulkResult.value = await journalsApi.bulk(spec.action, targets.value.map(r => r.journalHeaderId), reason.value.trim() || null)
    pending.value = null
    selected.value = new Set()
    await search()
  } catch (e) {
    const problem = e instanceof ApiError ? e.problem?.errors : undefined
    bulkError.value = problem ? Object.values(problem).flat().join(' ') : e instanceof Error ? e.message : 'The action failed.'
  } finally {
    running.value = false
  }
}

const helpOpen = ref(false)

search()
</script>

<template>
  <div class="cp-page">
    <GlPageHeader
      title="Journals"
      description="Find any journal by number, TRAN_SET_ID, account or segment values, then open it, or approve, post or cancel several at once."
      icon="pi pi-book"
    >
      <button type="button" class="cp-btn" @click="helpOpen = true"><i class="pi pi-question-circle"></i><span>How a journal moves</span></button>
      <RouterLink to="/gl/account-ledger" class="cp-btn"><i class="pi pi-chart-line"></i><span>Account ledger</span></RouterLink>
      <RouterLink v-if="canEdit" to="/gl/journals/new" class="cp-btn cp-btn--create"><i class="pi pi-plus"></i><span>New journal</span></RouterLink>
    </GlPageHeader>

    <nav class="js-stages" aria-label="Status">
      <button
        v-for="s in STAGES" :key="s.code" type="button" class="js-stage"
        :class="{ 'is-on': filters.stage === s.code, 'is-mine': s.code === 'MINE' && count('MINE') > 0 }" @click="setStage(s.code)"
      >
        <span>{{ s.label }}</span><b>{{ count(s.code) }}</b>
      </button>
    </nav>

    <form class="cp-card js-filters" @submit.prevent="search">
      <div class="js-row">
        <label class="js-f js-f--q"><span>Search</span><input v-model="filters.q" placeholder="Journal no, name, batch, reference, TRAN_SET_ID" /></label>
        <label class="js-f"><span>Ledger</span>
          <select v-model="filters.ledgerId"><option value="">All</option><option v-for="l in result?.lists.ledgers" :key="l.ledgerId" :value="String(l.ledgerId)">{{ l.ledgerCode }}</option></select>
        </label>
        <label class="js-f"><span>Period</span>
          <select v-model="filters.period"><option value="">All</option><option v-for="p in result?.lists.periods" :key="p" :value="p">{{ p }}</option></select>
        </label>
        <label class="js-f"><span>From</span><input v-model="filters.from" type="date" /></label>
        <label class="js-f"><span>To</span><input v-model="filters.to" type="date" /></label>
        <div class="js-go">
          <button type="button" class="cp-btn" :aria-expanded="moreFilters" @click="moreFilters = !moreFilters">
            <i class="pi pi-sliders-h"></i><span>More{{ activeFilters ? ` (${activeFilters})` : '' }}</span>
          </button>
          <button type="submit" class="cp-btn cp-btn--primary" :disabled="searching"><i :class="searching ? 'pi pi-spin pi-spinner' : 'pi pi-search'"></i><span>Search</span></button>
        </div>
      </div>
      <div v-if="moreFilters" class="js-row js-more">
        <label class="js-f"><span>Source</span>
          <select v-model="filters.source"><option value="">All</option><option v-for="o in result?.lists.sources" :key="o.code" :value="o.code">{{ o.name }}</option></select>
        </label>
        <label class="js-f"><span>Category</span>
          <select v-model="filters.category"><option value="">All</option><option v-for="o in result?.lists.categories" :key="o.code" :value="o.code">{{ o.name }}</option></select>
        </label>
        <label class="js-f"><span>TRAN_SET_ID starts with</span><input v-model="filters.tran" /></label>
        <label class="js-f"><span>Amount from</span><input v-model="filters.minAmount" type="number" min="0" step="0.01" /></label>
        <label class="js-f"><span>Amount to</span><input v-model="filters.maxAmount" type="number" min="0" step="0.01" /></label>
        <label class="js-f"><span>Created by</span><input v-model="filters.createdBy" /></label>
        <label v-for="s in result?.lists.segments" :key="s.segmentNum" class="js-f">
          <span>{{ s.segmentNameEn }}{{ s.isAccount ? ' (starts with)' : '' }}</span>
          <input v-model="filters.segments[s.segmentNum]" class="js-code" :list="`js-dl-${s.segmentNum}`" autocomplete="off" />
          <datalist :id="`js-dl-${s.segmentNum}`"><option v-for="v in s.values" :key="v.code" :value="v.code">{{ v.name }}</option></datalist>
        </label>
        <div class="js-go"><button type="button" class="cp-btn" @click="clearFilters"><i class="pi pi-filter-slash"></i><span>Clear</span></button></div>
      </div>
    </form>

    <div v-if="selected.size" class="cp-card js-bulk">
      <b>{{ selected.size }} selected</b>
      <button v-for="b in bulkButtons" :key="b.action" type="button" class="cp-btn cp-btn--small" :class="{ 'cp-btn--danger': b.danger }" @click="openBulk(b)">{{ b.label }}</button>
      <span v-if="!bulkButtons.length" class="cp-muted">No action fits every selected journal's status.</span>
      <span class="cp-spacer"></span>
      <button type="button" class="cp-btn cp-btn--small" @click="selected = new Set()">Clear selection</button>
    </div>

    <section class="cp-card">
      <div v-if="!result" class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div>
      <div v-else-if="!rows.length" class="cp-empty">No journal matches.<template v-if="canEdit"> <RouterLink to="/gl/journals/new">Enter a journal</RouterLink>.</template></div>
      <div v-else class="cp-table-wrap">
        <table class="cp-table js-table">
          <thead>
            <tr>
              <th class="js-check"><input type="checkbox" :checked="allSelected" aria-label="Select all" @change="toggleAll" /></th>
              <th>Journal</th><th>Name</th><th>Date</th><th>Period</th><th>Source</th><th class="cp-num">Amount</th><th class="cp-num">Lines</th><th>Status</th><th>Entered by</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in rows" :key="r.journalHeaderId" :class="{ 'is-sel': selected.has(r.journalHeaderId) }">
              <td class="js-check"><input type="checkbox" :checked="selected.has(r.journalHeaderId)" :aria-label="`Select ${r.journalNo}`" @change="toggle(r.journalHeaderId)" /></td>
              <td>
                <RouterLink :to="`/gl/journals/${r.journalHeaderId}`" class="js-no">{{ r.journalNo }}</RouterLink>
                <small v-if="r.tranSetId" class="js-sub">TRAN {{ r.tranSetId }}</small>
              </td>
              <td class="js-name">{{ r.journalName }}<small v-if="r.description" class="js-sub">{{ r.description }}</small></td>
              <td>{{ formatDate(r.accountingDate) }}</td>
              <td>{{ r.periodCode }}</td>
              <td>{{ codeLabel(r.sourceCode) }}<small class="js-sub">{{ codeLabel(r.categoryCode) }}</small></td>
              <td class="cp-num">{{ formatAmount(r.totalDr) }} <span class="cp-muted">{{ r.currencyCode }}</span></td>
              <td class="cp-num">{{ r.lineCount }}</td>
              <td>
                <span class="cp-pill" :class="journalStatusClass(r.stage)">{{ STAGE_LABELS[r.stage] ?? codeLabel(r.stage) }}</span>
                <small v-if="r.postError" class="js-err" :title="r.postError"><i class="pi pi-exclamation-triangle"></i> Posting failed</small>
              </td>
              <td>{{ r.createdBy }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <p v-if="result?.more" class="js-more-note">Showing the latest 500 journals. Narrow the search to see the rest.</p>
    </section>
    <p class="apex-ref">APEX App 120 page 51</p>

    <GlDialog
      v-if="pending"
      :title="`${pending.label} ${targets.length} journal${targets.length === 1 ? '' : 's'}`"
      :subtitle="selected.size > targets.length ? `${selected.size - targets.length} selected journal(s) are in another status and are left alone.` : undefined"
      :saving="running"
      :error="bulkError"
      :save-label="pending.label"
      @close="!running && (pending = null)"
      @save="runBulk"
    >
      <ul class="cp-field--wide js-list">
        <li v-for="r in targets" :key="r.journalHeaderId"><b>{{ r.journalNo }}</b> {{ r.journalName }} <span class="cp-muted">{{ formatAmount(r.totalDr) }}</span></li>
      </ul>
      <div v-if="pending.reason" class="cp-field cp-field--wide">
        <label for="js-reason">Reason *</label>
        <textarea id="js-reason" v-model="reason" rows="3" maxlength="1000"></textarea>
      </div>
    </GlDialog>

    <GlDialog
      v-if="bulkResult"
      :title="`${bulkResult.succeeded} done, ${bulkResult.failed} not done`"
      no-save
      @close="bulkResult = null"
    >
      <ul class="cp-field--wide js-list">
        <li v-for="r in bulkResult.results" :key="r.journalHeaderId" :class="r.ok ? 'is-ok' : 'is-bad'">
          <i :class="r.ok ? 'pi pi-check-circle' : 'pi pi-times-circle'"></i> <b>{{ r.journalNo }}</b> {{ r.message }}
        </li>
      </ul>
    </GlDialog>

    <GlDialog v-if="helpOpen" title="How a journal moves" subtitle="From the first line to the account balances, in four steps." no-save wide @close="helpOpen = false">
      <ol class="cp-field--wide js-flow">
        <li><b>1 · Draft</b><span>You enter the lines and save a draft at any time.</span></li>
        <li><b>2 · Submitted</b><span>Debits equal credits; the lines are locked.</span></li>
        <li><b>3 · Approved</b><span>Another user checks the journal and approves it. The user who submits cannot approve.</span></li>
        <li><b>4 · Posted</b><span>Posting updates the account balances.</span></li>
      </ol>
      <ul class="cp-field--wide js-rules">
        <li><b>Rejected</b> journals go back to their owner with a reason, to be corrected and submitted again.</li>
        <li><b>Cancel</b> a journal any time before it is posted, with a reason.</li>
        <li>If posting fails (for example, the period is closed), the journal stays approved with the reason; fix it and post again.</li>
        <li>A <b>posted</b> journal cannot change. To correct it, create a reversal from the journal: it follows the same four steps.</li>
      </ul>
    </GlDialog>
  </div>
</template>

<style scoped>
.js-stages { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 14px; }
.js-stage { display: inline-flex; align-items: center; gap: 8px; height: 36px; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 999px; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.js-stage b { min-width: 22px; padding: 1px 7px; border-radius: 999px; background: var(--cap-bg); color: var(--cap-ink); font-size: 11px; font-variant-numeric: tabular-nums; }
.js-stage.is-mine b { background: var(--cap-accent); color: var(--cap-navy); }
.js-stage.is-on { border-color: var(--cap-primary); background: var(--cap-primary); color: #fff; }
.js-stage.is-on b { background: rgba(255, 255, 255, .22); color: #fff; }
.js-filters { margin-bottom: 14px; padding: 14px 18px; }
.js-row { display: flex; flex-wrap: wrap; align-items: flex-end; gap: 12px; }
.js-more { margin-top: 12px; padding-top: 12px; border-top: 1px solid var(--cap-border-soft); }
.js-f { display: flex; flex-direction: column; gap: 5px; width: 150px; }
.js-f--q { flex: 1 1 260px; width: auto; }
.js-f span { color: var(--cap-ink-soft); font-size: 11px; font-weight: 500; }
.js-f input, .js-f select { height: 38px; padding: 0 10px; border: 1px solid var(--cap-border); border-radius: 7px; background: #fff; color: var(--cap-ink); font: inherit; font-size: 13px; }
.js-code { text-transform: uppercase; }
.js-go { display: flex; gap: 8px; margin-left: auto; }
.js-bulk { display: flex; flex-wrap: wrap; align-items: center; gap: 8px; margin-bottom: 14px; padding: 10px 16px; border-color: var(--cap-primary); background: var(--cap-primary-tint); }
.js-bulk b { margin-right: 6px; color: var(--cap-primary-dark); font-size: 13px; }
.js-table td { vertical-align: top; }
.js-check { width: 36px; }
.js-check input { width: 16px; height: 16px; accent-color: var(--cap-primary); }
.js-no { color: var(--cap-primary); font: 600 12px ui-monospace, Menlo, Consolas, monospace; text-decoration: none; }
.js-no:hover { text-decoration: underline; }
.js-sub { display: block; margin-top: 2px; color: var(--cap-muted); font-size: 11px; max-width: 320px; overflow: hidden; text-overflow: ellipsis; }
.js-name { white-space: normal; min-width: 180px; max-width: 340px; }
.js-err { display: block; margin-top: 4px; color: var(--cap-danger); font-size: 11px; }
tr.is-sel td { background: var(--cap-primary-tint); }
.js-more-note { margin: 0; padding: 10px 16px; border-top: 1px solid var(--cap-border-soft); color: var(--cap-muted); font-size: 12px; }
.js-list { margin: 0; padding: 0; list-style: none; max-height: 300px; overflow-y: auto; }
.js-list li { padding: 7px 0; border-bottom: 1px solid var(--cap-border-soft); font-size: 13px; }
.js-list li.is-ok i { color: var(--cap-success); }
.js-list li.is-bad { color: var(--cap-danger); }
.js-flow { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 10px; margin: 0; padding: 0; list-style: none; }
.js-flow li { padding: 12px; border: 1px solid var(--cap-border-soft); border-top: 4px solid var(--cap-primary); border-radius: 10px; }
.js-flow li:nth-child(3) { border-top-color: var(--cap-accent); }
.js-flow li:nth-child(4) { border-top-color: var(--cap-success-strong); }
.js-flow b { display: block; color: var(--cap-heading); font-size: 13px; }
.js-flow span { display: block; margin-top: 4px; color: var(--cap-muted); font-size: 12px; }
.js-rules { margin: 4px 0 0; padding-left: 18px; color: var(--cap-ink-soft); font-size: 13px; line-height: 1.7; }
.apex-ref { margin: 12px 0 0; color: var(--cap-faint); font-size: 11px; text-align: end; }
@media (max-width: 760px) {
  .js-f { width: calc(50% - 6px); }
  .js-f--q { width: 100%; flex-basis: 100%; }
  .js-go { margin-left: 0; }
  .js-flow { grid-template-columns: 1fr 1fr; }
}
</style>
