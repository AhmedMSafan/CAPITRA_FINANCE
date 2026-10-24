<script setup lang="ts">
// Workflow step "Portfolio Legal Review" (APEX App 105 page 211): work through the six legal checks, record the
// findings and sign-off, review which investors stay in the deal, then accept or reject the step.
import { computed, reactive, ref, shallowRef, watch } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { formatDateTime, todayInCairo } from '@/shared/format/datetime'
import { formatAmount, formatShort } from '@/shared/format/money'
import StepLayout from '../StepLayout.vue'
import { messageOf } from '../stepUtils'
import { LEGAL_CHECKS, legalReviewApi, type LegalReview, type LegalReviewForm } from './legalReview.api'

type DealFilter = 'ALL' | 'IN' | 'OUT'

const props = defineProps<{ portfolioId: string }>()
const canAct = hasPermission('MTG.STEP.LEGAL_REVIEW')

const data = shallowRef<LegalReview | null>(null)
const error = ref('')
const notice = ref('')
const busy = ref(false)
const dealFilter = ref<DealFilter>('ALL')
const search = ref('')

const id = computed(() => Number(props.portfolioId))
const editable = computed(() => canAct && !!data.value?.isOpen)

// ----- checklist form -----
function emptyForm(): LegalReviewForm {
  return {
    isOwnershipVerified: false, isBuildingLicenseVerified: false, isRegistrationPossible: false,
    isMortgagePossible: false, isEncumbranceFree: false, isTitleDeedVerified: false,
    legalFindings: '', legalRecommendation: '', reviewedBy: '', reviewDate: '', approvedBy: '', approvedDate: ''
  }
}

const form = reactive<LegalReviewForm>(emptyForm())
const savedForm = ref('')
const formError = ref('')

function snapshot(): string {
  return JSON.stringify(form)
}

function fillForm(d: LegalReview) {
  const r = d.review
  Object.assign(form, r
    ? {
        isOwnershipVerified: r.isOwnershipVerified, isBuildingLicenseVerified: r.isBuildingLicenseVerified,
        isRegistrationPossible: r.isRegistrationPossible, isMortgagePossible: r.isMortgagePossible,
        isEncumbranceFree: r.isEncumbranceFree, isTitleDeedVerified: r.isTitleDeedVerified,
        legalFindings: r.legalFindings ?? '', legalRecommendation: r.legalRecommendation ?? '',
        reviewedBy: r.reviewedBy ?? '', reviewDate: r.reviewDate ?? '', approvedBy: r.approvedBy ?? '', approvedDate: r.approvedDate ?? ''
      }
    : { ...emptyForm(), reviewDate: todayInCairo() })
  savedForm.value = d.review ? snapshot() : JSON.stringify({ ...emptyForm(), reviewDate: todayInCairo() })
}

const dirty = computed(() => snapshot() !== savedForm.value || (!data.value?.review && editable.value))

async function load() {
  try {
    data.value = await legalReviewApi.get(id.value)
    fillForm(data.value)
  } catch (e) {
    error.value = messageOf(e, 'Unable to load the legal review.')
  }
}

watch(id, () => {
  data.value = null
  error.value = ''
  load()
}, { immediate: true })

const checksDone = computed(() => LEGAL_CHECKS.filter(c => form[c.key]).length)

function blankToNull(v: string | null): string | null {
  return v && v.trim() ? v.trim() : null
}

async function saveForm() {
  busy.value = true
  formError.value = ''
  notice.value = ''
  try {
    data.value = await legalReviewApi.save(id.value, {
      ...form,
      legalFindings: blankToNull(form.legalFindings),
      legalRecommendation: blankToNull(form.legalRecommendation),
      reviewedBy: blankToNull(form.reviewedBy),
      reviewDate: form.reviewDate || null,
      approvedBy: blankToNull(form.approvedBy),
      approvedDate: form.approvedDate || null,
      rowVersion: data.value?.review?.rowVersion ?? null
    })
    fillForm(data.value)
    notice.value = 'Legal review saved.'
  } catch (e) {
    formError.value = messageOf(e, 'Unable to save the legal review.')
  } finally {
    busy.value = false
  }
}

// ----- investors -----
const accounts = computed(() => {
  const q = search.value.trim().toLowerCase()
  return (data.value?.accounts ?? []).filter(a => {
    const out = a.dealDecisionCode === 'EXCLUDED'
    if (dealFilter.value === 'IN' && out) return false
    if (dealFilter.value === 'OUT' && !out) return false
    return !q || [a.investorName, a.investorCode, a.sellerContractNo ?? '', a.unitCode ?? ''].some(v => v.toLowerCase().includes(q))
  })
})

const counts = computed(() => {
  const all = data.value?.accounts ?? []
  const kept = all.filter(a => a.dealDecisionCode !== 'EXCLUDED')
  return {
    all: all.length,
    kept: kept.length,
    out: all.length - kept.length,
    undecided: all.filter(a => a.dealDecisionCode !== 'SELECTED' && a.dealDecisionCode !== 'EXCLUDED').length,
    face: all.reduce((t, a) => t + a.contractAmount, 0),
    keptFace: kept.reduce((t, a) => t + a.contractAmount, 0),
    outstanding: all.reduce((t, a) => t + a.outstandingAmount, 0),
    keptOutstanding: kept.reduce((t, a) => t + a.outstandingAmount, 0)
  }
})

async function setDecision(portfolioAccountId: number, decision: 'SELECTED' | 'EXCLUDED') {
  busy.value = true
  error.value = ''
  try {
    await legalReviewApi.setDecision(id.value, portfolioAccountId, decision)
    // The form is not refilled, so unsaved checklist edits survive.
    data.value = await legalReviewApi.get(id.value)
  } catch (e) {
    error.value = messageOf(e, 'Unable to change the deal decision.')
  } finally {
    busy.value = false
  }
}

// ----- accept / reject -----
const comment = ref('')

const readiness = computed(() => {
  const r = data.value?.review
  return [
    { label: 'Checklist saved', ok: !!r && !dirty.value },
    { label: `All six checks are Yes (${checksDone.value} of 6)`, ok: checksDone.value === 6 },
    { label: 'Review date and approval date filled', ok: !!form.reviewDate && !!form.approvedDate },
    { label: 'Every contract is included or excluded', ok: counts.value.undecided === 0 }
  ]
})
const ready = computed(() => readiness.value.every(x => x.ok))

async function finish(action: 'ACCEPT' | 'REJECT') {
  busy.value = true
  error.value = ''
  notice.value = ''
  try {
    data.value = await legalReviewApi.decide(id.value, action, comment.value)
    fillForm(data.value)
    notice.value = action === 'ACCEPT' ? 'Legal review accepted.' : 'Legal review rejected.'
    comment.value = ''
  } catch (e) {
    error.value = messageOf(e, 'Unable to record the decision.')
  } finally {
    busy.value = false
  }
}
</script>

<template>
  <div v-if="error && !data" class="cp-page"><div class="cp-error">{{ error }}</div></div>
  <div v-else-if="!data" class="cp-page"><div class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div></div>

  <StepLayout
    v-else
    :portfolio-id="data.portfolioId"
    title="Portfolio legal review"
    lead="Work through the legal checklist, record the findings and sign-off, review which investors stay in the deal, then accept or reject this workflow step."
    :portfolio-no="data.portfolioNo"
    :project-name="data.projectName"
    :project-code="data.projectCode"
    :status-name="data.portfolioStatusName"
    :stages="[
      { label: 'Legal checklist', state: checksDone === 6 ? 'done' : 'now' },
      { label: 'Findings & sign-off', state: form.reviewDate && form.approvedDate ? 'done' : checksDone === 6 ? 'now' : 'todo' },
      { label: 'Review investors', state: data.isOpen ? (ready ? 'done' : 'todo') : 'done' },
      { label: 'Accept step', state: data.isOpen ? (ready ? 'now' : 'todo') : 'done' }
    ]"
    apex-page="211"
  >
    <div v-if="error" class="cp-error">{{ error }}</div>
    <div v-if="notice" class="notice">{{ notice }}</div>
    <div v-if="!data.isOpen" class="readonly"><i class="pi pi-eye"></i> Read-only view. The legal review is not open for a portfolio in status {{ data.portfolioStatusName }}.</div>
    <div v-else-if="!canAct" class="readonly"><i class="pi pi-eye"></i> Read-only view. Recording the legal review needs the legal review permission.</div>

    <div class="grid">
      <div class="stack">
        <section class="cp-card">
          <div class="cp-card__head">
            <i class="pi pi-briefcase"></i><h3>Legal checklist</h3><span class="cp-spacer"></span>
            <span class="cp-pill" :class="checksDone === 6 ? 'cp-pill--ok' : 'cp-pill--warn'">{{ checksDone }} of 6 verified</span>
          </div>
          <div class="cp-card__body">
            <div v-if="formError" class="cp-error">{{ formError }}</div>
            <div class="checks">
              <div v-for="c in LEGAL_CHECKS" :key="c.key" class="check" :class="{ yes: form[c.key] }">
                <i :class="c.icon" class="check__icon"></i>
                <div class="check__text"><strong>{{ c.label }}</strong><small>{{ c.hint }}</small></div>
                <div class="yn" role="group" :aria-label="c.label">
                  <button type="button" :aria-pressed="form[c.key]" :disabled="!editable" @click="form[c.key] = true">Yes</button>
                  <button type="button" :aria-pressed="!form[c.key]" :disabled="!editable" @click="form[c.key] = false">No</button>
                </div>
              </div>
            </div>

            <div class="cp-form findings">
              <div class="cp-field">
                <label for="lr-findings">Legal findings</label>
                <textarea id="lr-findings" v-model="form.legalFindings" rows="4" maxlength="4000" :readonly="!editable" placeholder="What did the legal review find?"></textarea>
              </div>
              <div class="cp-field">
                <label for="lr-recommendation">Recommendation</label>
                <textarea id="lr-recommendation" v-model="form.legalRecommendation" rows="4" maxlength="4000" :readonly="!editable" placeholder="Recommendation to the committee"></textarea>
              </div>
            </div>

            <h4 class="sub"><i class="pi pi-pencil"></i> Sign-off</h4>
            <div class="cp-form signoff">
              <div class="cp-field"><label for="lr-reviewed-by">Reviewed by</label><input id="lr-reviewed-by" v-model="form.reviewedBy" maxlength="255" :readonly="!editable" placeholder="Lawyer's name" /></div>
              <div class="cp-field"><label for="lr-review-date">Review date <span class="req">*</span></label><input id="lr-review-date" v-model="form.reviewDate" type="date" :readonly="!editable" /></div>
              <div class="cp-field"><label for="lr-approved-by">Approved by</label><input id="lr-approved-by" v-model="form.approvedBy" maxlength="255" :readonly="!editable" placeholder="Legal manager's name" /></div>
              <div class="cp-field"><label for="lr-approved-date">Approval date <span class="req">*</span></label><input id="lr-approved-date" v-model="form.approvedDate" type="date" :readonly="!editable" /></div>
            </div>

            <div v-if="editable" class="save">
              <small class="cp-muted">
                <template v-if="dirty">Unsaved changes.</template>
                <template v-else-if="data.review">Saved by {{ data.review.savedBy }}, {{ formatDateTime(data.review.savedAt) }}.</template>
              </small>
              <span class="cp-spacer"></span>
              <button type="button" class="cp-btn cp-btn--primary" :disabled="busy || !dirty" @click="saveForm"><i class="pi pi-save"></i><span>Save legal review</span></button>
            </div>
          </div>
        </section>

        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-users"></i><h3>Portfolio investors</h3><span class="cp-spacer"></span><span class="cp-muted">{{ accounts.length }} of {{ counts.all }}</span></div>
          <div class="tools">
            <label class="search"><i class="pi pi-search"></i><input v-model="search" type="search" placeholder="Search investor, code, contract or unit" aria-label="Search investors" /></label>
            <div class="seg" role="group" aria-label="Filter">
              <button type="button" :aria-pressed="dealFilter === 'ALL'" @click="dealFilter = 'ALL'">All <em>{{ counts.all }}</em></button>
              <button type="button" :aria-pressed="dealFilter === 'IN'" @click="dealFilter = 'IN'">Included <em>{{ counts.kept }}</em></button>
              <button type="button" :aria-pressed="dealFilter === 'OUT'" @click="dealFilter = 'OUT'">Excluded <em>{{ counts.out }}</em></button>
            </div>
          </div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Investor</th><th>Unit</th><th>Contract</th><th class="cp-num">Face value</th><th class="cp-num">Paid</th><th class="cp-num">Outstanding</th><th>Deal</th></tr></thead>
              <tbody>
                <tr v-for="a in accounts" :key="a.portfolioAccountId" :class="{ out: a.dealDecisionCode === 'EXCLUDED' }">
                  <td><strong>{{ a.investorName }}</strong><br /><small class="cp-muted">{{ a.investorCode }}</small></td>
                  <td>{{ a.unitCode ?? '-' }}</td>
                  <td><span class="code">{{ a.sellerContractNo ?? '-' }}</span></td>
                  <td class="cp-num">{{ formatAmount(a.contractAmount) }}</td>
                  <td class="cp-num">{{ formatAmount(a.paidAmount) }}</td>
                  <td class="cp-num">{{ formatAmount(a.outstandingAmount) }}</td>
                  <td>
                    <div v-if="editable" class="toggle" role="group" :aria-label="`Deal decision for ${a.investorName}`">
                      <button type="button" class="in" :aria-pressed="a.dealDecisionCode === 'SELECTED'" :disabled="busy" @click="setDecision(a.portfolioAccountId, 'SELECTED')">Include</button>
                      <button type="button" class="ex" :aria-pressed="a.dealDecisionCode === 'EXCLUDED'" :disabled="busy" @click="setDecision(a.portfolioAccountId, 'EXCLUDED')">Exclude</button>
                    </div>
                    <span v-else-if="a.dealDecisionCode === 'EXCLUDED'" class="cp-pill cp-pill--bad">Excluded</span>
                    <span v-else-if="a.dealDecisionCode === 'SELECTED'" class="cp-pill cp-pill--ok">Included</span>
                    <span v-else class="cp-pill cp-pill--warn">Not decided</span>
                  </td>
                </tr>
                <tr v-if="accounts.length === 0"><td colspan="7" class="cp-empty">{{ data.accounts.length ? 'No contracts match this filter.' : 'This portfolio has no investor contracts yet.' }}</td></tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>

      <aside class="stack">
        <section v-if="data.isOpen" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-list-check"></i><h3>Ready to accept?</h3></div>
          <ul class="ready cp-card__body">
            <li v-for="r in readiness" :key="r.label" :class="{ ok: r.ok }"><i :class="r.ok ? 'pi pi-check-circle' : 'pi pi-circle'"></i>{{ r.label }}</li>
          </ul>
        </section>

        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-chart-bar"></i><h3>Deal impact</h3></div>
          <dl class="cp-kv cp-card__body">
            <dt>Contracts kept</dt><dd>{{ counts.kept }} of {{ counts.all }}</dd>
            <dt>Face value</dt><dd :title="formatAmount(counts.keptFace)">{{ formatShort(counts.keptFace) }} <small class="cp-muted">of {{ formatShort(counts.face) }}</small></dd>
            <dt>Outstanding</dt><dd :title="formatAmount(counts.keptOutstanding)">{{ formatShort(counts.keptOutstanding) }} <small class="cp-muted">of {{ formatShort(counts.outstanding) }}</small></dd>
          </dl>
        </section>

        <section v-if="editable" class="cp-card">
          <div class="cp-card__head"><i class="pi pi-check-circle"></i><h3>Decide</h3></div>
          <div class="cp-card__body">
            <div class="cp-field">
              <label for="lr-comment">Comment</label>
              <textarea id="lr-comment" v-model="comment" rows="4" maxlength="4000" placeholder="Optional"></textarea>
            </div>
            <p v-if="dirty" class="hint">Save the legal review before accepting.</p>
            <div class="decide">
              <button type="button" class="cp-btn cp-btn--danger" :disabled="busy" @click="finish('REJECT')"><i class="pi pi-times"></i><span>Reject</span></button>
              <button type="button" class="cp-btn cp-btn--primary" :disabled="busy || !ready" @click="finish('ACCEPT')"><i class="pi pi-check"></i><span>Accept legal review</span></button>
            </div>
          </div>
        </section>
      </aside>
    </div>
  </StepLayout>
</template>

<style scoped>
.notice { padding: 10px 14px; border: 1px solid var(--cap-success-border); border-radius: 8px; background: var(--cap-success-bg); color: var(--cap-success); font-size: 13px; }
.readonly { padding: 10px 14px; border-radius: 8px; background: var(--cap-bg); color: var(--cap-ink-soft); font-size: 13px; }
.grid { display: grid; grid-template-columns: minmax(0, 1fr) 320px; gap: 16px; align-items: start; }
.stack { display: grid; grid-template-columns: minmax(0, 1fr); gap: 16px; }
.checks { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 10px; }
.check { display: flex; align-items: center; gap: 12px; padding: 12px 14px; border: 1px solid var(--cap-border); border-radius: 10px; background: #fff; }
.check.yes { border-color: var(--cap-success-border); background: var(--cap-success-bg); }
.check__icon { width: 32px; height: 32px; display: grid; place-items: center; border-radius: 8px; background: var(--cap-bg); color: var(--cap-primary-dark); font-size: 14px; flex: none; }
.check.yes .check__icon { background: var(--cap-success-bg); color: var(--cap-success); }
.check__text { display: grid; gap: 2px; min-width: 0; flex: 1; }
.check__text strong { font-size: 13px; color: var(--cap-ink); }
.check__text small { font-size: 11px; color: var(--cap-muted); }
.yn, .toggle { display: inline-flex; border: 1px solid var(--cap-border); border-radius: 8px; overflow: hidden; flex: none; }
.yn button, .toggle button { height: 30px; padding: 0 12px; border: 0; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.yn button + button, .toggle button + button { border-inline-start: 1px solid var(--cap-border); }
.yn button:disabled, .toggle button:disabled { cursor: default; }
.yn button[aria-pressed="true"]:first-child, .toggle .in[aria-pressed="true"] { background: var(--cap-success); color: #fff; }
.yn button[aria-pressed="true"]:last-child, .toggle .ex[aria-pressed="true"] { background: var(--cap-danger); color: #fff; }
.findings { margin-top: 16px; grid-template-columns: repeat(2, minmax(0, 1fr)); }
.signoff { grid-template-columns: repeat(4, minmax(0, 1fr)); }
.sub { display: flex; align-items: center; gap: 8px; margin: 18px 0 10px; font-size: 13px; color: var(--cap-primary-dark); }
.save { display: flex; align-items: center; gap: 10px; margin-top: 16px; padding-top: 14px; border-top: 1px solid var(--cap-border-soft); }
.tools { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; padding: 12px 20px; }
.search { flex: 1; max-width: 420px; height: 36px; display: flex; align-items: center; gap: 9px; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 8px; }
.search i { color: var(--cap-muted); font-size: 12px; }
.search input { width: 100%; border: 0; outline: 0; background: transparent; font: inherit; font-size: 12px; }
.seg { display: inline-flex; border: 1px solid var(--cap-border); border-radius: 8px; overflow: hidden; }
.seg button { height: 34px; padding: 0 12px; border: 0; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.seg button + button { border-inline-start: 1px solid var(--cap-border); }
.seg button em { margin-inline-start: 4px; color: var(--cap-muted); font-style: normal; }
.seg button[aria-pressed="true"] { background: var(--cap-primary-dark); color: #fff; }
.seg button[aria-pressed="true"] em { color: var(--cap-primary-tint-2); }
tr.out td { opacity: .6; }
tr.out td:last-child { opacity: 1; }
.code { color: var(--cap-primary-dark); font-weight: 600; font-size: 11px; }
.ready { list-style: none; margin: 0; display: grid; gap: 8px; font-size: 13px; color: var(--cap-muted); }
.ready li { display: flex; align-items: center; gap: 8px; }
.ready li.ok { color: var(--cap-success); }
.hint { margin: 10px 0 0; font-size: 12px; color: var(--cap-warning); }
.decide { display: flex; justify-content: flex-end; gap: 8px; margin-top: 12px; }
.req { color: var(--cap-danger); }
@media (max-width: 1100px) { .grid { grid-template-columns: minmax(0, 1fr); } .signoff { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
@media (max-width: 760px) { .checks, .findings, .signoff { grid-template-columns: 1fr; } }
</style>
