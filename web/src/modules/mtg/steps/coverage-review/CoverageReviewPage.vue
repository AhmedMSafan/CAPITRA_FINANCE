<script setup lang="ts">
// Workflow step "Portfolio Coverage Review" (APEX App 105 page 212): compare what each investor has paid with the
// portfolio's required coverage, keep or exclude investors, then accept or reject the step.
import { computed, ref, shallowRef, watch } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { formatAmount, formatPct, formatShort } from '@/shared/format/money'
import StepLayout from '../StepLayout.vue'
import { messageOf } from '../stepUtils'
import { coverageReviewApi, type CoverageReview } from './coverageReview.api'

type Filter = 'ALL' | 'BELOW' | 'IN' | 'OUT'

const props = defineProps<{ portfolioId: string }>()
const canAct = hasPermission('MTG.STEP.COVERAGE_REVIEW')

const data = shallowRef<CoverageReview | null>(null)
const error = ref('')
const notice = ref('')
const busy = ref(false)
const filter = ref<Filter>('ALL')
const search = ref('')

const id = computed(() => Number(props.portfolioId))
const editable = computed(() => canAct && !!data.value?.isOpen)

async function load() {
  try {
    data.value = await coverageReviewApi.get(id.value)
  } catch (e) {
    error.value = messageOf(e, 'Unable to load the coverage review.')
  }
}

watch(id, () => {
  data.value = null
  error.value = ''
  load()
}, { immediate: true })

const kept = (code: string | null) => code !== 'EXCLUDED'

const investors = computed(() => {
  const q = search.value.trim().toLowerCase()
  return (data.value?.investors ?? []).filter(i => {
    if (filter.value === 'BELOW' && i.meetsCoverage !== false) return false
    if (filter.value === 'IN' && !kept(i.dealDecisionCode)) return false
    if (filter.value === 'OUT' && kept(i.dealDecisionCode)) return false
    return !q || [i.investorName, i.investorCode].some(v => v.toLowerCase().includes(q))
  })
})

const counts = computed(() => {
  const all = data.value?.investors ?? []
  const keep = all.filter(i => kept(i.dealDecisionCode))
  const below = all.filter(i => i.meetsCoverage === false)
  return {
    all: all.length,
    kept: keep.length,
    out: all.length - keep.length,
    below: below.length,
    belowKept: below.filter(i => kept(i.dealDecisionCode)).length,
    face: all.reduce((t, i) => t + i.contractAmount, 0),
    keptFace: keep.reduce((t, i) => t + i.contractAmount, 0),
    outstanding: all.reduce((t, i) => t + i.outstandingAmount, 0),
    keptOutstanding: keep.reduce((t, i) => t + i.outstandingAmount, 0),
    keptPaid: keep.reduce((t, i) => t + i.paidAmount, 0)
  }
})

const keptPaidPct = computed(() => (counts.value.keptFace > 0 ? (counts.value.keptPaid / counts.value.keptFace) * 100 : null))

async function run(work: () => Promise<void>, fallback: string) {
  busy.value = true
  error.value = ''
  notice.value = ''
  try {
    await work()
  } catch (e) {
    error.value = messageOf(e, fallback)
  } finally {
    busy.value = false
  }
}

function applyRule() {
  return run(async () => {
    const result = await coverageReviewApi.applyRule(id.value)
    data.value = result.review
    notice.value = result.changedInvestorCount
      ? `Coverage rule applied: ${result.changedInvestorCount} investor${result.changedInvestorCount === 1 ? '' : 's'} changed.`
      : 'Coverage rule applied: every investor already matched the rule.'
  }, 'Unable to apply the coverage rule.')
}

function setDecision(investorId: number, decision: 'SELECTED' | 'EXCLUDED') {
  return run(async () => {
    data.value = await coverageReviewApi.setDecision(id.value, investorId, decision)
  }, 'Unable to change the deal decision.')
}

const comment = ref('')

function finish(action: 'ACCEPT' | 'REJECT') {
  return run(async () => {
    data.value = await coverageReviewApi.decide(id.value, action, comment.value)
    notice.value = action === 'ACCEPT' ? 'Coverage review accepted.' : 'Coverage review rejected.'
    comment.value = ''
  }, 'Unable to record the decision.')
}
</script>

<template>
  <div v-if="error && !data" class="cp-page"><div class="cp-error">{{ error }}</div></div>
  <div v-else-if="!data" class="cp-page"><div class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div></div>

  <StepLayout
    v-else
    :portfolio-id="data.portfolioId"
    title="Portfolio coverage review"
    lead="Compare what each investor has paid with the portfolio's required coverage, decide who stays in the deal, then accept or reject this workflow step."
    :portfolio-no="data.portfolioNo"
    :project-name="data.projectName"
    :project-code="data.projectCode"
    :status-name="data.portfolioStatusName"
    :stages="[
      { label: 'Required coverage', state: data.requiredCoveragePct !== null ? 'done' : 'now' },
      { label: 'Review investors', state: !data.isOpen ? 'done' : data.requiredCoveragePct !== null ? 'now' : 'todo' },
      { label: 'Accept step', state: data.isOpen ? 'todo' : 'done' }
    ]"
    apex-page="212"
  >
    <div v-if="error" class="cp-error">{{ error }}</div>
    <div v-if="notice" class="notice">{{ notice }}</div>
    <div v-if="data.rejectionReason && data.isOpen" class="rejected"><i class="pi pi-replay"></i> Last rejected: {{ data.rejectionReason }}</div>
    <div v-if="!data.isOpen" class="readonly"><i class="pi pi-eye"></i> Read-only view. The coverage review is not open for a portfolio in status {{ data.portfolioStatusName }}.</div>
    <div v-else-if="!canAct" class="readonly"><i class="pi pi-eye"></i> Read-only view. Recording the coverage review needs the coverage review permission.</div>

    <div class="grid">
      <div class="stack">
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-percentage"></i><h3>Coverage rule</h3></div>
          <div class="cp-card__body rule">
            <div class="rule__value">
              <b>{{ data.requiredCoveragePct !== null ? formatPct(data.requiredCoveragePct) : 'Not set' }}</b>
              <small>Required paid share of contract value</small>
            </div>
            <p v-if="data.requiredCoveragePct === null" class="rule__text">
              Set the coverage percentage on the portfolio's financial terms first.
              <RouterLink :to="{ name: 'mtg-portfolio', params: { portfolioId: data.portfolioId } }">Open the portfolio</RouterLink>
            </p>
            <p v-else class="rule__text">
              {{ counts.below }} of {{ counts.all }} investors are below the required coverage<template v-if="counts.belowKept">, and {{ counts.belowKept }} of them {{ counts.belowKept === 1 ? 'is' : 'are' }} still in the deal</template>.
              Applying the rule keeps every investor at or above it and excludes the rest.
            </p>
            <button v-if="editable" type="button" class="cp-btn cp-btn--primary" :disabled="busy || data.requiredCoveragePct === null" @click="applyRule">
              <i class="pi pi-bolt"></i><span>Apply coverage rule</span>
            </button>
          </div>
        </section>

        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-users"></i><h3>Investors</h3><span class="cp-spacer"></span><span class="cp-muted">{{ investors.length }} of {{ counts.all }}</span></div>
          <div class="tools">
            <label class="search"><i class="pi pi-search"></i><input v-model="search" type="search" placeholder="Search investor or code" aria-label="Search investors" /></label>
            <div class="seg" role="group" aria-label="Filter">
              <button type="button" :aria-pressed="filter === 'ALL'" @click="filter = 'ALL'">All <em>{{ counts.all }}</em></button>
              <button type="button" :aria-pressed="filter === 'BELOW'" @click="filter = 'BELOW'">Below coverage <em>{{ counts.below }}</em></button>
              <button type="button" :aria-pressed="filter === 'IN'" @click="filter = 'IN'">Included <em>{{ counts.kept }}</em></button>
              <button type="button" :aria-pressed="filter === 'OUT'" @click="filter = 'OUT'">Excluded <em>{{ counts.out }}</em></button>
            </div>
          </div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Investor</th><th class="cp-num">Contracts</th><th class="cp-num">Contract value</th><th class="cp-num">Paid</th><th>Paid share</th><th>Deal</th></tr></thead>
              <tbody>
                <tr v-for="i in investors" :key="i.investorId" :class="{ out: !kept(i.dealDecisionCode) }">
                  <td><strong>{{ i.investorName }}</strong><br /><small class="cp-muted">{{ i.investorCode }}</small></td>
                  <td class="cp-num">{{ i.contractCount }}</td>
                  <td class="cp-num">{{ formatAmount(i.contractAmount) }}</td>
                  <td class="cp-num">{{ formatAmount(i.paidAmount) }}</td>
                  <td>
                    <div class="share">
                      <div class="bar" :class="{ low: i.meetsCoverage === false }">
                        <i :style="{ width: Math.min(100, i.paidPct ?? 0) + '%' }"></i>
                        <span v-if="data.requiredCoveragePct !== null" class="mark" :style="{ left: Math.min(100, data.requiredCoveragePct) + '%' }"></span>
                      </div>
                      <span>{{ formatPct(i.paidPct) }}</span>
                      <span v-if="i.meetsCoverage === false" class="cp-pill cp-pill--bad">Below</span>
                    </div>
                  </td>
                  <td>
                    <div v-if="editable" class="toggle" role="group" :aria-label="`Deal decision for ${i.investorName}`">
                      <button type="button" class="in" :aria-pressed="i.dealDecisionCode === 'SELECTED'" :disabled="busy" @click="setDecision(i.investorId, 'SELECTED')">Include</button>
                      <button type="button" class="ex" :aria-pressed="i.dealDecisionCode === 'EXCLUDED'" :disabled="busy" @click="setDecision(i.investorId, 'EXCLUDED')">Exclude</button>
                    </div>
                    <span v-else-if="i.dealDecisionCode === 'EXCLUDED'" class="cp-pill cp-pill--bad">Excluded</span>
                    <span v-else class="cp-pill cp-pill--ok">Included</span>
                    <small v-if="i.dealDecisionCode === 'MIXED'" class="cp-muted mixed">Some contracts excluded</small>
                  </td>
                </tr>
                <tr v-if="investors.length === 0"><td colspan="6" class="cp-empty">{{ data.investors.length ? 'No investors match this filter.' : 'This portfolio has no investor contracts yet.' }}</td></tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>

      <aside class="stack">
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-chart-bar"></i><h3>Deal impact</h3></div>
          <dl class="cp-kv cp-card__body">
            <dt>Investors kept</dt><dd>{{ counts.kept }} of {{ counts.all }}</dd>
            <dt>Face value</dt><dd :title="formatAmount(counts.keptFace)">{{ formatShort(counts.keptFace) }} <small class="cp-muted">of {{ formatShort(counts.face) }}</small></dd>
            <dt>Outstanding</dt><dd :title="formatAmount(counts.keptOutstanding)">{{ formatShort(counts.keptOutstanding) }} <small class="cp-muted">of {{ formatShort(counts.outstanding) }}</small></dd>
            <dt>Paid share kept</dt><dd>{{ formatPct(keptPaidPct === null ? null : Math.round(keptPaidPct * 100) / 100) }}</dd>
          </dl>
        </section>

        <section v-if="editable" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-check-circle"></i><h3>Decide</h3></div>
          <div class="cp-card__body">
            <div class="cp-field">
              <label for="cov-comment">Comment <span class="req">*</span></label>
              <textarea id="cov-comment" v-model="comment" rows="4" maxlength="4000" placeholder="Required for Accept and Reject"></textarea>
            </div>
            <div class="decide">
              <button type="button" class="cp-btn cp-btn--danger" :disabled="busy" @click="finish('REJECT')"><i class="pi pi-times"></i><span>Reject</span></button>
              <button type="button" class="cp-btn cp-btn--primary" :disabled="busy" @click="finish('ACCEPT')"><i class="pi pi-check"></i><span>Accept coverage</span></button>
            </div>
          </div>
        </section>
      </aside>
    </div>
  </StepLayout>
</template>

<style scoped>
.notice { padding: 10px 14px; border: 1px solid #bfe3cc; border-radius: 8px; background: #effaf3; color: #197447; font-size: 13px; }
.readonly { padding: 10px 14px; border-radius: 8px; background: #f4f7fb; color: #405b4d; font-size: 13px; }
.rejected { padding: 10px 14px; border: 1px solid #f3d0cc; border-radius: 8px; background: #fdf3f2; color: #8a1c12; font-size: 13px; }
.grid { display: grid; grid-template-columns: minmax(0, 1fr) 320px; gap: 16px; align-items: start; }
.stack { display: grid; grid-template-columns: minmax(0, 1fr); gap: 16px; }
.rule { display: flex; flex-wrap: wrap; align-items: center; gap: 16px; }
.rule__value { display: grid; gap: 2px; padding: 12px 16px; border-radius: 10px; background: #f4f7fb; }
.rule__value b { font-size: 22px; color: #0a3573; }
.rule__value small { font-size: 11px; color: #6f8990; }
.rule__text { flex: 1; min-width: 220px; margin: 0; font-size: 13px; color: #405b4d; }
.tools { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; padding: 12px 20px; }
.search { flex: 1; max-width: 360px; height: 36px; display: flex; align-items: center; gap: 9px; padding: 0 12px; border: 1px solid #d5e1e8; border-radius: 8px; }
.search i { color: #6f8990; font-size: 12px; }
.search input { width: 100%; border: 0; outline: 0; background: transparent; font: inherit; font-size: 12px; }
.seg { display: inline-flex; flex-wrap: wrap; border: 1px solid #d5e1e8; border-radius: 8px; overflow: hidden; }
.seg button { height: 34px; padding: 0 12px; border: 0; background: #fff; color: #405b4d; font: inherit; font-size: 12px; cursor: pointer; }
.seg button + button { border-inline-start: 1px solid #d5e1e8; }
.seg button em { margin-inline-start: 4px; color: #718579; font-style: normal; }
.seg button[aria-pressed="true"] { background: #0a3573; color: #fff; }
.seg button[aria-pressed="true"] em { color: #d6e2f3; }
.share { display: flex; align-items: center; gap: 8px; min-width: 190px; font-size: 12px; }
.bar { position: relative; width: 90px; height: 8px; border-radius: 999px; background: #e6edf2; flex: none; }
.bar i { display: block; height: 100%; border-radius: inherit; background: #197447; }
.bar.low i { background: #c27c0e; }
.bar .mark { position: absolute; top: -3px; width: 2px; height: 14px; background: #0a3573; }
.toggle { display: inline-flex; border: 1px solid #d5e1e8; border-radius: 8px; overflow: hidden; }
.toggle button { height: 30px; padding: 0 12px; border: 0; background: #fff; color: #405b4d; font: inherit; font-size: 12px; cursor: pointer; }
.toggle button + button { border-inline-start: 1px solid #d5e1e8; }
.toggle .in[aria-pressed="true"] { background: #197447; color: #fff; }
.toggle .ex[aria-pressed="true"] { background: #b42318; color: #fff; }
.mixed { display: block; margin-top: 4px; font-size: 11px; }
tr.out td { opacity: .6; }
tr.out td:last-child { opacity: 1; }
.decide { display: flex; justify-content: flex-end; gap: 8px; margin-top: 12px; }
.req { color: #b42318; }
@media (max-width: 1100px) { .grid { grid-template-columns: minmax(0, 1fr); } }
</style>
