<template>
  <!-- Treasury > Treasury Disburse (agreed with Ahmed, 5 Nov 2026): the last step of a disbursement. Treasury chooses which money pays
       the net amount: own funds, one bank sublimit or a split. Each bank line shows what it can still take and a live badge for every
       bank rule that covers it (green passes, orange warns or needs an override, red blocks). Save reserves the room; Execute records
       the drawdowns; then the step is completed. -->
  <div class="fx fc trs">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'trs-bank-limits' }">{{ tr('Treasury') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ tr('Treasury Disburse') }}</b>
        </nav>
        <h1>{{ tr('Treasury Disburse') }} <span v-if="data" class="trs-muted">{{ data.header.disbursementNumber }}</span></h1>
        <p class="trs-lead">{{ tr('Choose which money pays this disbursement. Every choice is checked against the limits and the bank rules.') }}</p>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openManual(6)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
        <button type="button" class="fx-btn" :disabled="loading || busy" @click="load"><span class="fa" :class="loading ? 'fa-spinner fa-spin' : 'fa-refresh'" aria-hidden="true"></span>{{ tr('Refresh') }}</button>
        <FctStepActions :save="saveForStep" :disabled="busy || loading" :complete-note="completeNote" />
      </div>
    </div>

    <div v-if="data" class="trs-kpis">
      <div class="fx-card trs-kpi"><span class="trs-k">{{ tr('Net amount to fund') }}</span><b>{{ formatAmount(net) }}</b><small>{{ data.header.currencyCode }}</small></div>
      <div class="fx-card trs-kpi"><span class="trs-k">{{ tr('Covered') }}</span><b>{{ formatAmount(total) }}</b>
        <span class="trs-bar"><span class="o" :style="{ width: pct(total, net) + '%' }"></span></span></div>
      <div class="fx-card trs-kpi"><span class="trs-k">{{ tr('Remaining') }}</span><b :class="{ 'is-bad': remaining < -0.001 }">{{ formatAmount(remaining) }}</b>
        <small>{{ remaining > 0.001 ? tr('still to choose') : remaining < -0.001 ? tr('above the net amount') : tr('fully covered') }}</small></div>
      <div class="fx-card trs-kpi"><span class="trs-k">{{ tr('Status') }}</span>
        <b><span class="fx-chip" :class="statusClass">{{ tr(statusText) }}</span></b>
        <small v-if="data.header.executedBy">{{ data.header.executedBy }} · {{ formatDate(data.header.executedAt) }}</small>
        <small v-else-if="dirty">{{ tr('Changes not saved yet') }}</small></div>
    </div>

    <div v-if="data" class="trs-body">
      <section class="fx-card trs-src">
        <header class="trs-tree-head">
          <b>{{ tr('Sources of funds') }}</b>
          <span class="trs-src-tools">
            <button v-if="editable" type="button" class="fx-btn" @click="clearAll"><span class="fa fa-eraser" aria-hidden="true"></span>{{ tr('Clear') }}</button>
            <button v-if="editable" type="button" class="fx-btn fx-btn--teal" @click="suggest"><span class="fa fa-magic" aria-hidden="true"></span>{{ tr('Suggest') }}</button>
          </span>
        </header>
        <div class="trs-line trs-line--head" aria-hidden="true">
          <span class="trs-k">{{ tr('Source') }}</span><span class="trs-k num">{{ tr('Available') }}</span>
          <span class="trs-k">{{ tr('Bank rules') }}</span><span class="trs-k num">{{ tr('Amount') }}</span><span></span>
        </div>
        <template v-for="g in groups" :key="g.key">
          <div v-if="g.bankName" class="trs-group"><span class="fa fa-university" aria-hidden="true"></span>{{ g.bankName }}</div>
          <div v-for="s in g.sources" :key="s.sourceKey" class="trs-line" :class="'lvl-' + lineLevel(s)">
            <span class="trs-name">
              <b>{{ s.sublimitId ? s.sublimitName : tr('Own funds') }}</b>
              <small v-if="s.sublimitId">{{ s.facilityRef }} › {{ s.trancheName }}</small>
              <small v-else>{{ tr('Our own money; no bank rules apply') }}</small>
            </span>
            <span class="num">{{ s.available === null ? '—' : formatAmount(s.available) }}</span>
            <span class="trs-badges">
              <template v-if="rulesOf(s).length">
                <span class="fx-chip" :class="chip(lineRules(s))" :title="rulesOf(s).map((r) => tr(badge(r)) + ': ' + r.label).join('\n')">{{ lineBadge(s) }}</span>
                <small v-if="worst(s)" class="trs-muted">{{ worst(s)!.label }}</small>
              </template>
              <span v-else-if="s.sublimitId" class="trs-muted">{{ tr('No rules') }}</span>
            </span>
            <span class="num">
              <input v-model.number="amounts[s.sourceKey]" type="number" min="0" step="0.001" class="fx-search trs-amt" :disabled="!editable"
                     :aria-label="tr('Amount') + ' ' + (s.sublimitName ?? tr('Own funds'))" @input="dirty = true" />
            </span>
            <button v-if="editable" type="button" class="fx-btn trs-max" :title="tr('Fill with the most this line allows')" @click="fillMax(s)">{{ tr('Max') }}</button>
            <span v-else></span>
          </div>
        </template>
        <div v-if="sources.length === 1" class="fx-empty">{{ tr('No bank sublimit can fund this business line and currency. Use own funds, or add a sublimit in Bank Limits.') }}</div>
      </section>

      <aside class="fx-card trs-side">
        <div>
          <span class="trs-k">{{ tr('Disbursement') }}</span>
          <h2>{{ data.header.clientName }}</h2>
          <dl class="trs-facts">
            <div><dt>{{ tr('Agreement') }}</dt><dd>{{ data.header.agreementNumber ?? '—' }}</dd></div>
            <div><dt>{{ tr('Business line') }}</dt><dd>{{ tr(label(data.header.businessLine)) }}</dd></div>
            <div><dt>{{ tr('Invoices') }}</dt><dd>{{ data.header.invoiceCount }}</dd></div>
            <div><dt>{{ tr('Longest tenor') }}</dt><dd>{{ data.header.maxTenorDays === null ? '—' : data.header.maxTenorDays + ' ' + tr('days') }}</dd></div>
          </dl>
        </div>
        <div>
          <span class="trs-k">{{ data.header.isReverse ? tr('Buyer and suppliers') : tr('Debtors and seller') }}</span>
          <ul class="trs-parties">
            <li v-for="p in data.parties" :key="p.role + (p.clientId ?? p.name)">
              <span class="fx-chip fx-chip--plain">{{ tr(p.role === 'DEBTOR' ? 'Debtor' : 'Seller') }}</span>
              <span>{{ p.name }}</span><span class="trs-muted num">{{ (p.weight * 100).toFixed(0) }}%</span>
            </li>
          </ul>
        </div>
        <div>
          <span class="trs-k">{{ tr('Bank rule checks') }}</span>
          <div v-for="r in data.rules" :key="r.ruleId" class="trs-rule">
            <div class="trs-rule-top"><span class="trs-rule-subject">{{ r.bankName }}</span>
              <span class="fx-chip" :class="chip(level(r))">{{ tr(badge(r)) }}</span></div>
            <span>{{ r.label }}</span>
            <small class="trs-muted">{{ r.detail }}</small>
            <small v-if="r.limitAbove !== null" class="trs-muted">{{ tr('This disbursement may take up to') }} {{ formatAmount(r.limitAbove) }}
              · {{ tr('chosen') }} {{ formatAmount(covered(r)) }}</small>
          </div>
          <div v-if="!data.rules.length" class="trs-muted">{{ tr('No active bank rule applies to these sources.') }}</div>
        </div>

        <label v-if="needsOverride && editable" class="fc-field"><span>{{ tr('Override reason (a second user approves it)') }}</span>
          <textarea v-model="overrideReason" class="fx-search fc-textarea" rows="2" @input="dirty = true"></textarea></label>
        <div v-if="data.overrideRequestedBy && !dirty" class="trs-note">
          <span class="fa fa-shield" aria-hidden="true"></span>
          <span v-if="data.overrideApprovedBy">{{ tr('Override approved by') }} {{ data.overrideApprovedBy }}.</span>
          <span v-else>{{ tr('Override waiting for approval. Asked by') }} {{ data.overrideRequestedBy }}: {{ data.overrideReason }}</span>
        </div>

        <div class="trs-side-actions">
          <button v-if="editable" type="button" class="fx-btn" :disabled="busy" @click="saveLines()">
            <span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save and reserve') }}</button>
          <button v-if="canVerify && data.canApproveOverride && !dirty" type="button" class="fx-btn" :disabled="busy" @click="approve">
            <span class="fa fa-check" aria-hidden="true"></span>{{ tr('Approve override') }}</button>
          <button v-if="editable" type="button" class="fx-btn fx-btn--teal" :disabled="busy || !canExecute" :title="executeHint" @click="execute">
            <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-bolt'" aria-hidden="true"></span>{{ tr('Execute') }}</button>
        </div>
        <small v-if="editable && !canExecute" class="trs-muted">{{ tr(executeHint) }}</small>
        <small v-if="data.approveBlockedReason" class="trs-muted">{{ tr(data.approveBlockedReason) }}</small>
      </aside>
    </div>
    <div v-else-if="!loading" class="fx-empty">{{ tr('Open this page from a disbursement task.') }}</div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import { tr } from '@/shared/layout/language'
import FctStepActions from '@/modules/fct/components/FctStepActions.vue'
import { trsDisburseApi, type TrsDisburse, type TrsDisburseRule, type TrsDisburseSource } from '../api/trsDisburse.api'
import { label, openManual, pct, save } from '../components/trs'

const route = useRoute()
const id = computed(() => Number(route.params.disbursementId))
const canEdit = hasPermission('TRS.EDIT')
const canVerify = hasPermission('TRS.VERIFY')
const data = ref<TrsDisburse | null>(null)
const amounts = reactive<Record<string, number | ''>>({})
const overrideReason = ref('')
const loading = ref(false)
const busy = ref(false)
const dirty = ref(false)
const completeNote = 'Your choices are saved, then this step is completed. The source of funds must be executed first.'

const sources = computed(() => data.value?.sources ?? [])
const net = computed(() => data.value?.header.netAmount ?? 0)
const amt = (key: string) => Number(amounts[key]) || 0
const total = computed(() => sources.value.reduce((t, s) => t + amt(s.sourceKey), 0))
const remaining = computed(() => net.value - total.value)
const executed = computed(() => data.value?.header.fundingStatus === 'EXECUTED')
const editable = computed(() => canEdit && !!data.value?.canEdit)

const groups = computed(() => {
  const out: { key: string; bankName: string | null; sources: TrsDisburseSource[] }[] = []
  for (const s of sources.value) {
    const key = s.bankId ? `B${s.bankId}` : 'OWN'
    let g = out.find((x) => x.key === key)
    if (!g) out.push((g = { key, bankName: s.bankName, sources: [] }))
    g.sources.push(s)
  }
  return out
})

const rulesOf = (s: TrsDisburseSource) => (data.value?.rules ?? []).filter((r) => s.ruleIds.includes(r.ruleId))
const covered = (r: TrsDisburseRule) => sources.value.filter((s) => s.ruleIds.includes(r.ruleId)).reduce((t, s) => t + amt(s.sourceKey), 0)

/** OK, WARN (above the warning level, or a WARN rule breached), OVERRIDE (needs a second user), BLOCK. */
function level(r: TrsDisburseRule): 'OK' | 'WARN' | 'OVERRIDE' | 'BLOCK' {
  const sum = covered(r)
  if (r.limitAbove !== null && sum > r.limitAbove + 0.001) return r.breachActionCode === 'WARN' ? 'WARN' : r.breachActionCode
  if (r.warnAbove !== null && sum > r.warnAbove + 0.001) return 'WARN'
  return 'OK'
}
const rank = { OK: 0, WARN: 1, OVERRIDE: 2, BLOCK: 3 } as const
function lineLevel(s: TrsDisburseSource) {
  if (!amt(s.sourceKey)) return 'OK'
  if (s.available !== null && amt(s.sourceKey) > s.available + 0.001) return 'BLOCK'
  return rulesOf(s).map(level).reduce<keyof typeof rank>((a, b) => (rank[b] > rank[a] ? b : a), 'OK')
}
const lineRules = (s: TrsDisburseSource) => rulesOf(s).map(level).reduce<keyof typeof rank>((a, b) => (rank[b] > rank[a] ? b : a), 'OK')
/** The rule that limits the line most (only when it does not pass). */
const worst = (s: TrsDisburseSource) => rulesOf(s).filter((r) => level(r) !== 'OK').sort((a, b) => rank[level(b)] - rank[level(a)])[0]
function lineBadge(s: TrsDisburseSource) {
  const n = rulesOf(s).length
  const w = worst(s)
  return w ? tr(badge(w)) : `${n} ${tr(n === 1 ? 'rule passes' : 'rules pass')}`
}
const chip = (l: string) => (l === 'OK' ? 'fx-chip--ok' : l === 'BLOCK' ? 'fx-chip--bad' : 'fx-chip--warn')
function badge(r: TrsDisburseRule) {
  const l = level(r)
  if (l === 'BLOCK') return 'Blocked'
  if (l === 'OVERRIDE') return 'Needs override'
  if (l === 'WARN') return 'Warning'
  return 'Passes'
}

const needsOverride = computed(() => (data.value?.rules ?? []).some((r) => level(r) === 'OVERRIDE'))
const anyBlocked = computed(() => sources.value.some((s) => lineLevel(s) === 'BLOCK'))
const pendingOverride = computed(() => !!data.value?.overrideRequestedBy && !data.value?.overrideApprovedBy)
const executeHint = computed(() => {
  if (Math.abs(remaining.value) > 0.001) return 'The sources must add up to the net amount.'
  if (anyBlocked.value) return 'A line is blocked by a limit or a bank rule.'
  if (needsOverride.value && !dirty.value && pendingOverride.value) return 'A second user must approve the override.'
  if (needsOverride.value && (dirty.value || !data.value?.overrideApprovedBy)) return 'Save, then a second user approves the override.'
  return 'Records the drawdowns on the chosen sublimits.'
})
const canExecute = computed(() => executeHint.value === 'Records the drawdowns on the chosen sublimits.')

const statusText = computed(() => (executed.value ? 'Executed' : data.value?.header.fundingStatus === 'RESERVED' ? 'Reserved' : 'Not chosen'))
const statusClass = computed(() => (executed.value ? 'fx-chip--ok' : data.value?.header.fundingStatus === 'RESERVED' ? 'fx-chip--warn' : 'fx-chip--plain'))

/** The most a line may take: its availability, the room every blocking or override rule leaves, and what is still to cover. */
function maxFor(s: TrsDisburseSource) {
  const others = total.value - amt(s.sourceKey)
  let m = Math.max(0, net.value - others)
  if (s.available !== null) m = Math.min(m, s.available)
  for (const r of rulesOf(s)) {
    if (r.limitAbove === null || r.breachActionCode === 'WARN') continue
    m = Math.min(m, Math.max(0, r.limitAbove - (covered(r) - amt(s.sourceKey))))
  }
  return Math.max(0, Math.floor(m * 1000 + 1e-6) / 1000)
}
function fillMax(s: TrsDisburseSource) {
  amounts[s.sourceKey] = maxFor(s)
  dirty.value = true
}
function clearAll() {
  for (const s of sources.value) amounts[s.sourceKey] = 0
  dirty.value = true
}
/** Fills the bank lines first (most available first), each within every rule, then own funds with the rest. */
function suggest() {
  clearAll()
  const banks = sources.value.filter((s) => s.sublimitId).sort((a, b) => (b.available ?? 0) - (a.available ?? 0))
  for (const s of banks) if (remaining.value > 0.001) amounts[s.sourceKey] = maxFor(s)
  const own = sources.value.find((s) => !s.sublimitId)
  if (own && remaining.value > 0.001) amounts[own.sourceKey] = Math.round(remaining.value * 1000) / 1000
}

function apply(d: TrsDisburse) {
  data.value = d
  for (const k of Object.keys(amounts)) delete amounts[k]
  for (const s of d.sources) amounts[s.sourceKey] = s.amount
  overrideReason.value = d.overrideReason ?? ''
  dirty.value = false
}

async function load() {
  if (!Number.isFinite(id.value) || id.value <= 0) return
  loading.value = true
  try {
    apply(await trsDisburseApi.get(id.value))
  } catch (e) {
    showError(e instanceof Error ? e.message : String(e))
  } finally {
    loading.value = false
  }
}

const form = () => ({
  lines: sources.value.filter((s) => amt(s.sourceKey) > 0).map((s) => ({ sublimitId: s.sublimitId, amount: amt(s.sourceKey) })),
  overrideReason: overrideReason.value.trim() || null
})

async function saveLines(message = 'Sources of funds saved and reserved.') {
  busy.value = true
  try {
    return await save(async () => apply(await trsDisburseApi.save(id.value, form())), message)
  } finally {
    busy.value = false
  }
}

async function approve() {
  busy.value = true
  try {
    await save(async () => apply(await trsDisburseApi.approveOverride(id.value)), 'Override approved.')
  } finally {
    busy.value = false
  }
}

async function execute() {
  if (dirty.value && !(await saveLines('Sources of funds saved.'))) return
  busy.value = true
  try {
    await save(async () => apply(await trsDisburseApi.execute(id.value)), 'Executed: the drawdowns are recorded.')
  } finally {
    busy.value = false
  }
}

/** The step's Complete: once executed nothing is left to save; before that, the step cannot be completed. */
async function saveForStep() {
  if (executed.value) return true
  if (dirty.value) await saveLines()
  showError(tr('Execute the source of funds before completing this step.'))
  return false
}

onMounted(load)
</script>

<style scoped>
.trs { display: flex; flex-direction: column; gap: 16px; }
.trs-lead { margin: 4px 0 0; color: var(--fin-muted); font-size: var(--fs-sm); }
.trs-k { font-size: var(--fs-xs); font-weight: 700; color: var(--fin-muted); text-transform: uppercase; letter-spacing: .03em; }
.trs-muted { color: var(--fin-muted); font-size: var(--fs-xs); font-weight: 400; }
.num { text-align: end; font-variant-numeric: tabular-nums; white-space: nowrap; }
.is-bad { color: var(--fin-bad, #B42318); }

.trs-kpis { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; }
.trs-kpi { display: flex; flex-direction: column; gap: 6px; padding: 14px 16px; }
.trs-kpi b { font-size: 22px; font-variant-numeric: tabular-nums; color: var(--fin-ink); }
.trs-kpi small { color: var(--fin-muted); font-size: var(--fs-xs); }
.trs-bar { display: flex; height: 10px; width: 100%; border-radius: 999px; overflow: hidden; background: var(--fin-teal-50); }
.trs-bar .o { background: var(--fin-teal-600); }

.trs-body { display: flex; flex-wrap: wrap; gap: 16px; align-items: flex-start; }
.trs-src { flex: 999 1 620px; min-width: 0; padding: 0; overflow: hidden; }
.trs-tree-head { display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: 10px; padding: 12px 16px; border-block-end: 1px solid var(--fin-line); }
.trs-src-tools { display: flex; gap: 8px; }
.trs-group { display: flex; align-items: center; gap: 8px; padding: 10px 16px 6px; font-weight: 700; color: var(--fin-teal-600); background: #FAFCFC;
  border-block-end: 1px solid var(--fin-line-soft); }
.trs-line { display: grid; grid-template-columns: minmax(0, 1.5fr) 150px minmax(0, 1.3fr) 170px 58px; gap: 12px; align-items: center;
  padding: 10px 16px; border-block-end: 1px solid var(--fin-line-soft); border-inline-start: 4px solid transparent; }
.trs-line--head { background: #FAFCFC; padding-block: 8px; }
.trs-line.lvl-BLOCK { border-inline-start-color: #B42318; background: #FEF6F5; }
.trs-line.lvl-OVERRIDE, .trs-line.lvl-WARN { border-inline-start-color: var(--fin-orange); }
.trs-name { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
.trs-name small { color: var(--fin-muted); font-size: var(--fs-xs); }
.trs-badges { display: flex; flex-direction: column; align-items: flex-start; gap: 3px; min-width: 0; }
.trs-amt { width: 100%; min-width: 0; padding-inline: 10px; text-align: end; font-variant-numeric: tabular-nums; }
.trs-max { padding-inline: 10px; }

.trs-side { flex: 1 1 320px; max-width: 400px; align-self: flex-start; display: flex; flex-direction: column; gap: 16px; padding: 16px 18px; }
.trs-side h2 { margin: 4px 0 8px; font-size: var(--fs-lg); color: var(--fin-ink); }
.trs-facts { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 10px 16px; margin: 0; }
.trs-facts dt { font-size: var(--fs-xs); font-weight: 700; color: var(--fin-muted); }
.trs-facts dd { margin: 3px 0 0; }
.trs-parties { list-style: none; margin: 6px 0 0; padding: 0; display: flex; flex-direction: column; gap: 6px; }
.trs-parties li { display: grid; grid-template-columns: auto minmax(0, 1fr) auto; gap: 8px; align-items: center; }
.trs-rule { display: flex; flex-direction: column; gap: 3px; padding: 10px 0; border-block-start: 1px solid var(--fin-line-soft); }
.trs-rule-top { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
.trs-rule-subject { font-weight: 600; }
.trs-note { display: flex; gap: 8px; padding: 10px 12px; border-radius: 8px; background: #FDF0E5; color: var(--fin-ink); font-size: var(--fs-sm); }
.trs-note .fa { color: var(--fin-orange); margin-top: 3px; }
.trs-side-actions { display: flex; flex-wrap: wrap; gap: 8px; }
.fc .trs-side .fc-field { flex: none; }

@media (max-width: 900px) {
  .trs-kpis { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .trs-line { grid-template-columns: minmax(0, 1fr) 120px; }
  .trs-line--head { display: none; }
  .trs-badges { grid-column: 1 / -1; }
  .trs-side { max-width: none; }
}
</style>
