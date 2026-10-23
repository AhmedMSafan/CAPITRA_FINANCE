<script setup lang="ts">
// Workflow step "AML Review" (APEX App 105 page 209): screen the developer and every investor, decide which contracts
// stay in the deal, then accept or reject the review.
import { computed, reactive, ref, shallowRef, watch } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { ApiError } from '@/shared/api/http'
import { formatDate, formatDateTime, todayInCairo } from '@/shared/format/datetime'
import { formatAmount, formatShort } from '@/shared/format/money'
import StepLayout from '../StepLayout.vue'
import { amlReviewApi, isFlagged, niceCode, type AmlReview, type DealDecision } from './amlReview.api'

type Subject = { type: 'DEVELOPER' | 'INVESTOR'; id: number; name: string }
type AccountFilter = 'ALL' | 'FLAGGED' | 'UNDECIDED' | 'EXCLUDED'

const props = defineProps<{ portfolioId: string }>()
const canAct = hasPermission('MTG.STEP.AML_REVIEW')

const data = shallowRef<AmlReview | null>(null)
const error = ref('')
const notice = ref('')
const busy = ref(false)
const filter = ref<AccountFilter>('ALL')

const id = computed(() => Number(props.portfolioId))
const editable = computed(() => canAct && !!data.value?.isOpen)

async function load() {
  try {
    data.value = await amlReviewApi.get(id.value)
  } catch (e) {
    error.value = messageOf(e, 'Unable to load the AML review.')
  }
}

watch(id, () => {
  data.value = null
  error.value = ''
  load()
}, { immediate: true })

function messageOf(e: unknown, fallback: string): string {
  if (e instanceof ApiError) {
    const fieldErrors = Object.values(e.problem?.errors ?? {}).flat()
    if (fieldErrors.length) return fieldErrors.join(' ')
  }
  return e instanceof Error ? e.message : fallback
}

// ----- investors (one row per contract) -----
const investors = computed(() => {
  const all = data.value?.accounts ?? []
  return all.filter(a =>
    filter.value === 'ALL' ||
    (filter.value === 'FLAGGED' && isFlagged(a.amlStatusCode)) ||
    (filter.value === 'UNDECIDED' && !a.dealDecisionCode) ||
    (filter.value === 'EXCLUDED' && a.dealDecisionCode === 'EXCLUDED'))
})

const counts = computed(() => {
  const all = data.value?.accounts ?? []
  return {
    all: all.length,
    flagged: all.filter(a => isFlagged(a.amlStatusCode)).length,
    flaggedKept: all.filter(a => isFlagged(a.amlStatusCode) && a.dealDecisionCode !== 'EXCLUDED').length,
    undecided: all.filter(a => !a.dealDecisionCode).length,
    excluded: all.filter(a => a.dealDecisionCode === 'EXCLUDED').length,
    notScreened: new Set(all.filter(a => a.amlStatusCode === 'NOT_SCREENED').map(a => a.investorId)).size
  }
})

/** What stays in the deal with the current decisions. */
const impact = computed(() => {
  const all = data.value?.accounts ?? []
  const kept = all.filter(a => a.dealDecisionCode === 'SELECTED')
  const sum = (rows: typeof all, f: (a: (typeof all)[number]) => number) => rows.reduce((t, a) => t + f(a), 0)
  return {
    keptCount: kept.length,
    face: sum(all, a => a.contractAmount),
    keptFace: sum(kept, a => a.contractAmount),
    outstanding: sum(all, a => a.outstandingAmount),
    keptOutstanding: sum(kept, a => a.outstandingAmount)
  }
})

const readiness = computed(() => {
  const d = data.value
  if (!d) return []
  return [
    { ok: d.developer.amlStatusCode !== 'NOT_SCREENED', text: 'Developer screened' },
    { ok: counts.value.notScreened === 0, text: counts.value.notScreened ? `${counts.value.notScreened} investors not screened` : 'Every investor screened' },
    { ok: counts.value.undecided === 0, text: counts.value.undecided ? `${counts.value.undecided} contracts undecided` : 'Every contract included or excluded' }
  ]
})

async function decide(rows: { portfolioAccountId: number }[], decision: DealDecision) {
  if (!rows.length) return
  busy.value = true
  error.value = ''
  try {
    await amlReviewApi.saveDecisions(id.value, rows.map(r => ({ portfolioAccountId: r.portfolioAccountId, dealDecisionCode: decision })))
    await load()
  } catch (e) {
    error.value = messageOf(e, 'Unable to save the deal decision.')
  } finally {
    busy.value = false
  }
}

function excludeFlagged() {
  decide((data.value?.accounts ?? []).filter(a => isFlagged(a.amlStatusCode) && a.dealDecisionCode !== 'EXCLUDED'), 'EXCLUDED')
}

// ----- screening form -----
const screening = ref<Subject | null>(null)
const form = reactive({ amlTypeCode: 'AML_SCREENING', screeningDate: '', referenceNo: '', amlResultCode: 'CLEAR', riskLevelCode: 'LOW', matchScorePct: '', resultDetails: '' })
const formError = ref('')

function openScreening(subject: Subject) {
  Object.assign(form, { amlTypeCode: 'AML_SCREENING', screeningDate: todayInCairo(), referenceNo: '', amlResultCode: 'CLEAR', riskLevelCode: 'LOW', matchScorePct: '', resultDetails: '' })
  formError.value = ''
  screening.value = subject
}

async function saveScreening() {
  if (!screening.value) return
  busy.value = true
  formError.value = ''
  try {
    await amlReviewApi.addResult(id.value, {
      subjectTypeCode: screening.value.type,
      subjectId: screening.value.id,
      amlTypeCode: form.amlTypeCode,
      screeningDate: form.screeningDate,
      referenceNo: form.referenceNo || null,
      amlResultCode: form.amlResultCode,
      riskLevelCode: form.riskLevelCode,
      matchScorePct: form.matchScorePct === '' ? null : Number(form.matchScorePct),
      resultDetails: form.resultDetails || null
    })
    screening.value = null
    await load()
  } catch (e) {
    formError.value = messageOf(e, 'Unable to save the screening.')
  } finally {
    busy.value = false
  }
}

// ----- accept / reject -----
const comment = ref('')

async function finish(action: 'ACCEPT' | 'REJECT') {
  busy.value = true
  error.value = ''
  notice.value = ''
  try {
    data.value = await amlReviewApi.decide(id.value, action, comment.value)
    notice.value = action === 'ACCEPT' ? 'AML accepted successfully.' : 'AML rejected successfully.'
    comment.value = ''
  } catch (e) {
    error.value = messageOf(e, 'Unable to record the decision.')
  } finally {
    busy.value = false
  }
}

function tone(code: string): string {
  if (code === 'CLEAR') return 'cp-pill--ok'
  if (code === 'CONFIRMED_MATCH') return 'cp-pill--bad'
  if (isFlagged(code)) return 'cp-pill--warn'
  return ''
}
</script>

<template>
  <div v-if="error && !data" class="cp-page"><div class="cp-error">{{ error }}</div></div>
  <div v-else-if="!data" class="cp-page"><div class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div></div>

  <StepLayout
    v-else
    :portfolio-id="data.portfolioId"
    title="AML review"
    lead="Screen the developer and every investor, decide who stays in the deal, then accept or reject the review."
    :portfolio-no="data.portfolioNo"
    :project-name="data.projectName"
    :project-code="data.projectCode"
    :status-name="data.portfolioStatusName"
    apex-page="209"
  >
    <div v-if="error" class="cp-error">{{ error }}</div>
    <div v-if="notice" class="notice">{{ notice }}</div>
    <div v-if="!data.isOpen" class="readonly"><i class="pi pi-eye"></i> Read-only view. The AML review is not open for a portfolio in status {{ data.portfolioStatusName }}.</div>
    <div v-else-if="!canAct" class="readonly"><i class="pi pi-eye"></i> Read-only view. Recording the review needs the AML review permission.</div>
    <div v-if="data.rejectionReason" class="rejected"><b>Last rejection</b>{{ data.rejectionReason }}</div>

    <div class="grid">
      <div class="stack">
        <!-- developer -->
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-briefcase"></i><h3>Screen developer</h3></div>
          <div class="subject">
            <div><strong>{{ data.developer.name }}</strong><small class="cp-muted">{{ data.developer.code }}</small></div>
            <span class="cp-pill" :class="tone(data.developer.amlStatusCode)">{{ niceCode(data.developer.amlStatusCode) }}</span>
            <button v-if="editable" type="button" class="cp-btn cp-btn--small" @click="openScreening({ type: 'DEVELOPER', id: data.developer.subjectId, name: data.developer.name })">
              <i class="pi pi-shield"></i><span>Screen now</span>
            </button>
          </div>
        </section>

        <!-- investors and deal decisions -->
        <section class="cp-card">
          <div class="cp-card__head">
            <i class="pi pi-users"></i><h3>Screen investors and decide deal inclusion</h3><span class="cp-spacer"></span>
            <button v-if="editable && counts.flaggedKept" type="button" class="cp-btn cp-btn--small cp-btn--danger" :disabled="busy" @click="excludeFlagged">
              <i class="pi pi-ban"></i><span>Exclude flagged ({{ counts.flaggedKept }})</span>
            </button>
          </div>
          <div class="seg-row">
            <div class="seg" role="group" aria-label="Contracts">
              <button type="button" :aria-pressed="filter === 'ALL'" @click="filter = 'ALL'">All <em>{{ counts.all }}</em></button>
              <button type="button" :aria-pressed="filter === 'FLAGGED'" @click="filter = 'FLAGGED'">Flagged <em>{{ counts.flagged }}</em></button>
              <button type="button" :aria-pressed="filter === 'UNDECIDED'" @click="filter = 'UNDECIDED'">Undecided <em>{{ counts.undecided }}</em></button>
              <button type="button" :aria-pressed="filter === 'EXCLUDED'" @click="filter = 'EXCLUDED'">Excluded <em>{{ counts.excluded }}</em></button>
            </div>
          </div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Investor</th><th>Unit</th><th>Contract</th><th class="cp-num">Face value</th><th class="cp-num">Outstanding</th><th>AML</th><th>Deal</th><th></th></tr></thead>
              <tbody>
                <tr v-for="a in investors" :key="a.portfolioAccountId" :class="{ out: a.dealDecisionCode === 'EXCLUDED' }">
                  <td><strong>{{ a.investorName }}</strong><br /><small class="cp-muted">{{ a.investorCode }}</small></td>
                  <td>{{ a.unitCode ?? '-' }}</td>
                  <td><span class="code">{{ a.sellerContractNo ?? '-' }}</span></td>
                  <td class="cp-num">{{ formatAmount(a.contractAmount) }}</td>
                  <td class="cp-num">{{ formatAmount(a.outstandingAmount) }}</td>
                  <td><span class="cp-pill" :class="tone(a.amlStatusCode)">{{ niceCode(a.amlStatusCode) }}</span></td>
                  <td>
                    <div class="toggle" role="group" :aria-label="`Deal decision for ${a.investorName}`">
                      <button type="button" :aria-pressed="a.dealDecisionCode === 'SELECTED'" :disabled="!editable || busy" @click="decide([a], 'SELECTED')">Include</button>
                      <button type="button" :aria-pressed="a.dealDecisionCode === 'EXCLUDED'" :disabled="!editable || busy" @click="decide([a], 'EXCLUDED')">Exclude</button>
                    </div>
                  </td>
                  <td>
                    <button v-if="editable" type="button" class="cp-btn cp-btn--small" @click="openScreening({ type: 'INVESTOR', id: a.investorId, name: a.investorName })">
                      <i class="pi pi-shield"></i><span>Screen</span>
                    </button>
                  </td>
                </tr>
                <tr v-if="investors.length === 0">
                  <td colspan="8" class="cp-empty">{{ data.accounts.length ? 'No contracts match this filter.' : 'This portfolio has no investor contracts yet.' }}</td>
                </tr>
              </tbody>
            </table>
          </div>
        </section>

        <!-- screening log -->
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-history"></i><h3>Screening log</h3><span class="cp-spacer"></span><span class="cp-muted">{{ data.results.length }} checks</span></div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Subject</th><th>Check</th><th>Date</th><th>Result</th><th>Risk</th><th class="cp-num">Match</th><th>Reference</th><th>By</th></tr></thead>
              <tbody>
                <tr v-for="r in data.results" :key="r.amlResultId" :title="r.resultDetails ?? ''">
                  <td><strong>{{ r.subjectName }}</strong><br /><small class="cp-muted">{{ niceCode(r.subjectTypeCode) }}</small></td>
                  <td>{{ niceCode(r.amlTypeCode) }}</td>
                  <td>{{ formatDate(r.screeningDate + 'T00:00:00Z') }}</td>
                  <td><span class="cp-pill" :class="tone(r.amlResultCode)">{{ niceCode(r.amlResultCode) }}</span></td>
                  <td>{{ niceCode(r.riskLevelCode) }}</td>
                  <td class="cp-num">{{ r.matchScorePct === null ? '-' : `${r.matchScorePct}%` }}</td>
                  <td>{{ r.referenceNo ?? '-' }}</td>
                  <td><small>{{ r.createdBy }}<br /><span class="cp-muted">{{ formatDateTime(r.createdAt) }}</span></small></td>
                </tr>
                <tr v-if="data.results.length === 0"><td colspan="8" class="cp-empty">No checks recorded yet.</td></tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>

      <aside class="stack">
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-chart-bar"></i><h3>Deal impact</h3></div>
          <dl class="cp-kv cp-card__body">
            <dt>Contracts kept</dt><dd>{{ impact.keptCount }} of {{ counts.all }}</dd>
            <dt>Face value</dt><dd :title="formatAmount(impact.keptFace)">{{ formatShort(impact.keptFace) }} <small class="cp-muted">of {{ formatShort(impact.face) }}</small></dd>
            <dt>Outstanding</dt><dd :title="formatAmount(impact.keptOutstanding)">{{ formatShort(impact.keptOutstanding) }} <small class="cp-muted">of {{ formatShort(impact.outstanding) }}</small></dd>
          </dl>
        </section>

        <section v-if="editable" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-check-circle"></i><h3>Accept or reject</h3></div>
          <div class="cp-card__body">
            <ul class="ready">
              <li v-for="r in readiness" :key="r.text" :class="{ ok: r.ok }"><i :class="r.ok ? 'pi pi-check' : 'pi pi-circle'"></i>{{ r.text }}</li>
            </ul>
            <div class="cp-field">
              <label for="aml-comment">Comment <span class="req">*</span></label>
              <textarea id="aml-comment" v-model="comment" rows="4" maxlength="4000" placeholder="Required for both accept and reject"></textarea>
            </div>
            <div class="decide">
              <button type="button" class="cp-btn cp-btn--danger" :disabled="busy" @click="finish('REJECT')"><i class="pi pi-times"></i><span>Reject review</span></button>
              <button type="button" class="cp-btn cp-btn--primary" :disabled="busy" @click="finish('ACCEPT')"><i class="pi pi-check"></i><span>Accept & continue</span></button>
            </div>
          </div>
        </section>
      </aside>
    </div>

    <div v-if="screening" class="cp-modal-overlay" @click.self="screening = null">
      <section class="cp-modal" role="dialog" aria-modal="true" aria-labelledby="aml-title">
        <header class="cp-modal__header">
          <div><h3 id="aml-title">Screen {{ screening.name }}</h3><p>{{ niceCode(screening.type) }}</p></div>
          <button type="button" class="cp-modal__close" aria-label="Close" @click="screening = null"><i class="pi pi-times"></i></button>
        </header>
        <div class="cp-modal__body">
          <div v-if="formError" class="cp-error">{{ formError }}</div>
          <div class="cp-form">
            <div class="cp-field cp-field--wide">
              <label>Check type</label>
              <div class="chips">
                <button v-for="t in data.options.amlTypes" :key="t" type="button" :aria-pressed="form.amlTypeCode === t" @click="form.amlTypeCode = t">{{ niceCode(t) }}</button>
              </div>
            </div>
            <div class="cp-field"><label for="aml-date">Screening date <span class="req">*</span></label><input id="aml-date" v-model="form.screeningDate" type="date" /></div>
            <div class="cp-field"><label for="aml-ref">Reference no.</label><input id="aml-ref" v-model="form.referenceNo" maxlength="100" /></div>
            <div class="cp-field">
              <label for="aml-result">Result <span class="req">*</span></label>
              <select id="aml-result" v-model="form.amlResultCode"><option v-for="r in data.options.results" :key="r" :value="r">{{ niceCode(r) }}</option></select>
            </div>
            <div class="cp-field">
              <label for="aml-risk">Risk level <span class="req">*</span></label>
              <select id="aml-risk" v-model="form.riskLevelCode"><option v-for="r in data.options.riskLevels" :key="r" :value="r">{{ niceCode(r) }}</option></select>
            </div>
            <div class="cp-field"><label for="aml-score">Match score (%)</label><input id="aml-score" v-model="form.matchScorePct" type="number" min="0" max="100" step="0.01" /></div>
            <div class="cp-field cp-field--wide"><label for="aml-details">Result details</label><textarea id="aml-details" v-model="form.resultDetails" rows="3" maxlength="4000"></textarea></div>
          </div>
        </div>
        <footer class="cp-modal__footer">
          <span class="cp-spacer"></span>
          <button type="button" class="cp-btn" :disabled="busy" @click="screening = null">Cancel</button>
          <button type="button" class="cp-btn cp-btn--primary" :disabled="busy" @click="saveScreening"><i class="pi pi-check"></i><span>Save screening</span></button>
        </footer>
      </section>
    </div>
  </StepLayout>
</template>

<style scoped>
.notice { padding: 10px 14px; border: 1px solid #bfe3cc; border-radius: 8px; background: #effaf3; color: #197447; font-size: 13px; }
.readonly { padding: 10px 14px; border-radius: 8px; background: #f4f7fb; color: #405b4d; font-size: 13px; }
.rejected { padding: 10px 14px; border: 1px solid #f2c4c4; border-radius: 8px; background: #fff7f7; color: #7a2e2e; font-size: 13px; }
.rejected b { display: block; margin-bottom: 3px; color: #a53d3d; }
.grid { display: grid; grid-template-columns: minmax(0, 1fr) 320px; gap: 16px; align-items: start; }
.stack { display: grid; gap: 16px; }
.subject { display: flex; align-items: center; gap: 14px; padding: 14px 20px; }
.subject > div { flex: 1; }
.subject strong { display: block; color: #183550; }
.seg-row { padding: 12px 20px; }
.seg { display: inline-flex; border: 1px solid #d5e1e8; border-radius: 8px; overflow: hidden; }
.seg button { height: 34px; padding: 0 12px; border: 0; background: #fff; color: #405b4d; font: inherit; font-size: 12px; cursor: pointer; }
.seg button + button { border-inline-start: 1px solid #d5e1e8; }
.seg button em { margin-inline-start: 4px; color: #718579; font-style: normal; }
.seg button[aria-pressed="true"] { background: #0a3573; color: #fff; }
.seg button[aria-pressed="true"] em { color: #d6e2f3; }
.toggle { display: inline-flex; border: 1px solid #d5e1e8; border-radius: 7px; overflow: hidden; }
.toggle button { height: 30px; padding: 0 10px; border: 0; background: #fff; color: #405b4d; font: inherit; font-size: 12px; cursor: pointer; }
.toggle button + button { border-inline-start: 1px solid #d5e1e8; }
.toggle button[aria-pressed="true"]:first-child { background: #15945b; color: #fff; }
.toggle button[aria-pressed="true"]:last-child { background: #a53d3d; color: #fff; }
.toggle button:disabled { cursor: default; }
tr.out td { opacity: .6; }
.code { color: #0a3573; font-weight: 600; font-size: 11px; }
.ready { margin: 0 0 14px; padding: 0; list-style: none; font-size: 12px; }
.ready li { display: flex; align-items: center; gap: 8px; padding: 4px 0; color: #9a5b0c; }
.ready li.ok { color: #197447; }
.ready i { font-size: 11px; }
.decide { display: flex; justify-content: flex-end; gap: 8px; margin-top: 12px; }
.req { color: #b42318; }
.chips { display: flex; flex-wrap: wrap; gap: 6px; }
.chips button { height: 32px; padding: 0 12px; border: 1px solid #d5e1e8; border-radius: 999px; background: #fff; color: #405b4d; font: inherit; font-size: 12px; cursor: pointer; }
.chips button[aria-pressed="true"] { border-color: #0a3573; background: #0a3573; color: #fff; }
@media (max-width: 1100px) { .grid { grid-template-columns: 1fr; } }
</style>
