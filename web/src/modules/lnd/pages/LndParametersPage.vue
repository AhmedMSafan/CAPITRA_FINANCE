<template>
  <!-- Retail Lending > Parameters (Ahmed, 4 Nov 2026: every legal figure is an admin setting). One tab per licence. A new value is a
       request with an effective date and, for a legal figure, its FRA decision; it is used only once another user approves it.
       The go-live check at the top of each tab lists what still stops the licence from lending. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'adm-home' }">{{ tr('Administration') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ tr('Lending Parameters') }}</b>
        </nav>
        <h1>{{ tr('Lending Parameters') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'adm-home' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Administration') }}</RouterLink>
      </div>
    </div>

    <section class="fx-card">
      <div class="fc-tabs" role="tablist">
        <button v-for="t in tabs" :key="t.key" type="button" role="tab" class="fc-tab" :aria-selected="tab === t.key" @click="tab = t.key">
          <span class="fa" :class="t.icon" aria-hidden="true"></span>{{ tr(t.label) }}<em v-if="t.count" class="ms-count">{{ t.count }}</em>
        </button>
      </div>

      <div class="c3-body">
        <div v-if="loading && !data" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading parameters...') }}</div>

        <div v-else-if="scope && data" class="c3-tab">
          <div v-if="scope.problems.length" class="lp-check lp-check--bad">
            <span class="fa fa-ban" aria-hidden="true"></span>
            <div>
              <b>{{ tr('This licence cannot lend yet.') }} {{ scope.problems.length }} {{ tr(scope.problems.length === 1 ? 'item to resolve' : 'items to resolve') }}</b>
              <ul><li v-for="p in scope.problems" :key="p">{{ problemText(p) }}</li></ul>
            </div>
          </div>
          <p v-else class="c3-note"><span class="fa fa-check-circle" aria-hidden="true"></span>{{ tr('Every value of this licence is approved and in force. It passes the go-live check.') }}</p>

          <section v-for="g in groups" :key="g.name" class="c3-panel">
            <header class="c3-panel-head"><h3><span class="fa" :class="scope.icon" aria-hidden="true"></span>{{ tr(g.name) }}</h3></header>
            <div class="ms-list">
              <article v-for="p in g.parameters" :key="p.code" class="ms-row" :class="{ 'ms-row--pending': p.pending !== null }">
                <div class="ms-text">
                  <b>{{ tr(p.label) }}<span v-if="p.isLegal" class="fx-chip fx-chip--teal lp-legal">{{ tr('FRA rule') }}</span></b>
                  <p v-if="p.help">{{ tr(p.help) }}</p>
                  <small v-if="p.value !== null" class="c3-muted">
                    {{ tr('In force from') }} {{ formatDate(p.effectiveFromDate) }}<template v-if="p.decisionReference"> · {{ p.decisionReference }}</template>
                    · {{ tr('approved by') }} {{ p.approvedBy }} · {{ formatDateTime(p.approvedAt) }}
                  </small>
                  <small v-if="p.nextValueLabel" class="c3-muted lp-next">
                    <span class="fa fa-calendar" aria-hidden="true"></span>{{ tr('Changes to') }} {{ p.nextValueLabel }} {{ tr('on') }} {{ formatDate(p.nextEffectiveFromDate) }}
                  </small>
                  <small v-if="p.value === null && p.source" class="c3-muted lp-source">
                    <template v-if="p.suggestedLabel">{{ tr('Suggested:') }} {{ fmt(p, p.suggestedValue) }} · </template>{{ tr(p.source) }}
                  </small>
                </div>
                <div class="ms-value">
                  <span v-if="p.value !== null" class="fx-chip" :class="valueChip(p)">{{ fmt(p, p.value) }}</span>
                  <span v-else class="fx-chip fx-chip--bad">{{ tr('No value') }}</span>
                  <button v-if="data.canEdit && p.pending === null" type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="openRequest(p)">
                    <span class="fa fa-pencil" aria-hidden="true"></span>{{ tr(p.value === null ? 'Enter value' : 'New value') }}
                  </button>
                </div>
                <div v-if="p.pending" class="ms-pending">
                  <span class="fa fa-clock-o" aria-hidden="true"></span>
                  <div class="ms-pending-text">
                    <b>{{ tr('Waiting for approval:') }} {{ fmt(p, p.pending.value) }} {{ tr('from') }} {{ formatDate(p.pending.effectiveFromDate) }}</b>
                    <small>
                      {{ p.pending.requestedBy }} · {{ formatDateTime(p.pending.requestedAt) }}<template v-if="p.pending.decisionReference"> · {{ p.pending.decisionReference }}</template> · {{ p.pending.reason }}
                    </small>
                  </div>
                  <div class="c3-row-actions">
                    <template v-if="p.pending.canApprove">
                      <button type="button" class="fx-btn fx-btn--sm fx-btn--teal" :disabled="busy" @click="openDecision(p, 'approve')"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Approve') }}</button>
                      <button type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="openDecision(p, 'reject')"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Reject') }}</button>
                    </template>
                    <template v-else-if="mine(p) && data.canEdit">
                      <span class="c3-muted">{{ tr(p.pending.approveBlockedReason ?? '') }}</span>
                      <button type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="withdraw(p)"><span class="fa fa-undo" aria-hidden="true"></span>{{ tr('Withdraw') }}</button>
                    </template>
                  </div>
                </div>
              </article>
            </div>
          </section>
        </div>

        <div v-else-if="tab === 'history' && data" class="c3-tab">
          <div class="fx-table-wrap">
            <table class="fx-table">
              <thead>
                <tr>
                  <th>{{ tr('Asked') }}</th><th>{{ tr('Licence') }}</th><th>{{ tr('Parameter') }}</th><th>{{ tr('Value') }}</th><th>{{ tr('From') }}</th>
                  <th>{{ tr('FRA decision') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Decided') }}</th><th>{{ tr('Notes') }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="h in data.history" :key="h.parameterValueId">
                  <td>{{ h.requestedBy }}<br><small class="c3-muted">{{ formatDateTime(h.requestedAt) }}</small></td>
                  <td>{{ tr(scopeTitle(h.scopeCode)) }}</td>
                  <td>{{ tr(h.label) }}</td>
                  <td>{{ h.valueLabel }}</td>
                  <td>{{ formatDate(h.effectiveFromDate) }}</td>
                  <td>{{ h.decisionReference ?? '' }}</td>
                  <td><span class="fx-chip" :class="STATUS_CHIP[h.statusCode]">{{ tr(STATUS[h.statusCode]) }}</span></td>
                  <td><template v-if="h.decidedBy">{{ h.decidedBy }}<br><small class="c3-muted">{{ formatDateTime(h.decidedAt) }}</small></template></td>
                  <td>{{ h.reason }}<template v-if="h.decisionNotes"><br><small class="c3-muted">{{ h.decisionNotes }}</small></template></td>
                </tr>
              </tbody>
            </table>
            <div v-if="!data.history.length" class="fx-empty"><span class="fa fa-history" aria-hidden="true"></span>{{ tr('No values recorded yet.') }}</div>
          </div>
        </div>
      </div>
    </section>

    <Crm360Dialog v-model:visible="requesting" :title="tr('New Value')" :subtitle="current ? `${tr(current.label)} · ${tr(scope?.title ?? '')}` : ''" icon="fa-sliders" :busy="busy" :width="620">
      <template v-if="current">
        <p class="c3-note">
          <span class="fa fa-info-circle" aria-hidden="true"></span>
          <span>
            <template v-if="current.value !== null">{{ tr('Now:') }} {{ fmt(current, current.value) }}. </template>
            {{ tr('Another user must approve the value before it is used. It is used from its start date; earlier days keep the value they had.') }}
          </span>
        </p>
        <div class="c3-grid c3-grid--2">
          <label class="fc-field" :class="bad('value')"><span>{{ tr('Value') }}<template v-if="current.unit && current.type !== 'BOOL'"> ({{ tr(current.unit) }})</template><b> *</b></span>
            <select v-if="current.type === 'BOOL'" v-model="form.value" class="fx-select"><option value="true">{{ tr('Yes') }}</option><option value="false">{{ tr('No') }}</option></select>
            <input v-else v-model="form.value" class="fx-search" inputmode="decimal">
          </label>
          <label class="fc-field" :class="bad('effectiveFromDate')"><span>{{ tr('In force from') }}<b> *</b></span>
            <input v-model="form.effectiveFromDate" type="date" class="fx-search" :min="data?.today">
          </label>
          <label class="fc-field c3-wide" :class="bad('decisionReference')"><span>{{ tr('FRA decision or policy') }}<b v-if="current.isLegal"> *</b></span>
            <input v-model="form.decisionReference" class="fx-search" maxlength="200" :placeholder="tr('e.g. FRA Decision 1 of 2026')">
          </label>
          <label class="fc-field c3-wide" :class="bad('reason')"><span>{{ tr('Reason') }}<b> *</b></span><input v-model="form.reason" class="fx-search" maxlength="1000"></label>
        </div>
        <p v-if="current.suggestedLabel" class="lp-suggest">
          <span class="c3-muted">{{ tr('Suggested:') }} {{ fmt(current, current.suggestedValue) }} · {{ tr(current.source ?? '') }}</span>
          <button type="button" class="fx-btn fx-btn--sm" @click="useSuggestion"><span class="fa fa-magic" aria-hidden="true"></span>{{ tr('Use suggestion') }}</button>
        </p>
        <p v-if="current.min !== null && current.type !== 'BOOL'" class="c3-muted">{{ tr('From') }} {{ current.min }} {{ tr('to') }} {{ current.max }}{{ current.unit ? ' ' + tr(current.unit) : '' }}.</p>
      </template>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="requesting = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="request"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-paper-plane'" aria-hidden="true"></span>{{ tr('Ask for approval') }}</button>
      </template>
    </Crm360Dialog>

    <Crm360Dialog v-model:visible="deciding" :title="tr(decision === 'approve' ? 'Approve Value' : 'Reject Value')" :subtitle="current ? tr(current.label) : ''"
                  :icon="decision === 'approve' ? 'fa-check-circle' : 'fa-times-circle'" :busy="busy" :width="560">
      <p v-if="current?.pending" class="c3-note">
        <span class="fa fa-info-circle" aria-hidden="true"></span>
        <span>{{ fmt(current, current.pending.value) }} {{ tr('from') }} {{ formatDate(current.pending.effectiveFromDate) }}<template v-if="current.pending.decisionReference"> · {{ current.pending.decisionReference }}</template> · {{ current.pending.requestedBy }}: {{ current.pending.reason }}</span>
      </p>
      <p v-if="decision === 'approve' && current?.isLegal" class="c3-muted">{{ tr('Check the value against the FRA decision itself before approving.') }}</p>
      <label class="fc-field" :class="bad('notes')"><span>{{ tr(decision === 'approve' ? 'Notes' : 'Reason') }}<b v-if="decision === 'reject'"> *</b></span><input v-model="notes" class="fx-search" maxlength="1000"></label>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="deciding = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn" :class="decision === 'approve' ? 'fx-btn--teal' : 'fx-btn--primary'" :disabled="busy" @click="decide">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : decision === 'approve' ? 'fa-check' : 'fa-times'" aria-hidden="true"></span>{{ tr(decision === 'approve' ? 'Approve' : 'Reject') }}
        </button>
      </template>
    </Crm360Dialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute } from 'vue-router'
import { ApiError } from '@/shared/api/http'
import { getUser } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { lndParametersApi, type LndParameter, type LndParametersPage } from '../api/lndParameters.api'

const me = (getUser()?.userName ?? '').toLowerCase()

const STATUS = { PENDING: 'Waiting', APPROVED: 'Approved', REJECTED: 'Rejected', WITHDRAWN: 'Withdrawn' } as const
const STATUS_CHIP = { PENDING: 'fx-chip--warn', APPROVED: 'fx-chip--ok', REJECTED: 'fx-chip--bad', WITHDRAWN: 'fx-chip--teal' } as const

const data = ref<LndParametersPage | null>(null)
const loading = ref(false)
const busy = ref(false)
// the Administration card opens a licence's tab (?tab=CF)
const asked = String(useRoute().query.tab ?? 'MF').toUpperCase()
const tab = ref(asked === 'HISTORY' ? 'history' : asked)

const tabs = computed(() => [
  ...(data.value?.scopes ?? []).map((s) => ({ key: s.code as string, label: s.title, icon: s.icon, count: s.pendingCount })),
  { key: 'history', label: 'History', icon: 'fa-history', count: 0 }
])
const scope = computed(() => data.value?.scopes.find((s) => s.code === tab.value) ?? null)
const scopeTitle = (code: string) => data.value?.scopes.find((s) => s.code === code)?.title ?? code

/** The licence's parameters by group, in the catalog's order. */
const groups = computed(() => {
  const out: { name: string; parameters: LndParameter[] }[] = []
  for (const p of scope.value?.parameters ?? []) {
    let g = out.find((x) => x.name === p.group)
    if (!g) out.push((g = { name: p.group, parameters: [] }))
    g.parameters.push(p)
  }
  return out
})

const valueChip = (p: LndParameter) => (p.type === 'BOOL' ? (p.value === 'true' ? 'fx-chip--ok' : 'fx-chip--bad') : 'fx-chip--teal')
const mine = (p: LndParameter) => !!me && (p.pending?.requestedBy ?? '').toLowerCase() === me
const egp = new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 })
/** A stored value in words, in the page language: Yes / No, "50%", "EGP 292,000" or the number with its unit. */
function fmt(p: LndParameter, v: string | null): string {
  if (v === null) return ''
  if (p.type === 'BOOL') return tr(v === 'true' ? 'Yes' : 'No')
  if (p.unit === '%') return `${v}%`
  if (p.unit === 'EGP') return `${tr('EGP')} ${egp.format(Number(v))}`
  return p.unit ? `${v} ${tr(p.unit)}` : v
}
/** A go-live problem is "<parameter>: no approved value." or a sentence; the parameter name is shown in the page language. */
function problemText(p: string) {
  const i = p.indexOf(': ')
  return i < 0 ? tr(p) : `${tr(p.slice(0, i))}: ${tr(p.slice(i + 2))}`
}

// ---- dialogs
const requesting = ref(false)
const deciding = ref(false)
const decision = ref<'approve' | 'reject'>('approve')
const current = ref<LndParameter | null>(null)
const form = reactive({ value: '', effectiveFromDate: '', decisionReference: '', reason: '' })
const notes = ref('')
const errors = ref<Record<string, string[]>>({})
const bad = (f: string) => ({ 'c3-bad': !!errors.value[f] })

function openRequest(p: LndParameter) {
  current.value = p
  form.value = p.value ?? (p.type === 'BOOL' ? 'true' : '')
  form.effectiveFromDate = data.value?.today ?? ''
  form.decisionReference = ''
  form.reason = ''
  errors.value = {}
  requesting.value = true
}
function useSuggestion() {
  if (!current.value?.suggestedValue) return
  form.value = current.value.suggestedValue
  if (!form.decisionReference && current.value.source) form.decisionReference = current.value.source.split('.')[0] ?? ''
}
function openDecision(p: LndParameter, d: 'approve' | 'reject') {
  current.value = p
  decision.value = d
  notes.value = ''
  errors.value = {}
  deciding.value = true
}

async function run(action: () => Promise<LndParametersPage>, done: string, close: () => void) {
  busy.value = true
  try {
    data.value = await action()
    close()
    showSuccess(tr(done))
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) {
      errors.value = err.problem.errors
      showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    } else showError(err instanceof Error ? err.message : String(err))
  } finally {
    busy.value = false
  }
}

const request = () => run(() => lndParametersApi.request(current.value!.code, tab.value, {
  value: String(form.value).trim(),
  effectiveFromDate: form.effectiveFromDate || null,
  decisionReference: form.decisionReference.trim() || null,
  reason: form.reason
}), 'Value sent for approval. Another user must approve it.', () => (requesting.value = false))
const decide = () => decision.value === 'approve'
  ? run(() => lndParametersApi.approve(current.value!.pending!.parameterValueId, notes.value || null), 'Value approved.', () => (deciding.value = false))
  : run(() => lndParametersApi.reject(current.value!.pending!.parameterValueId, notes.value), 'Value rejected.', () => (deciding.value = false))
const withdraw = (p: LndParameter) => run(() => lndParametersApi.withdraw(p.pending!.parameterValueId), 'Request withdrawn.', () => {})

async function load() {
  loading.value = true
  try {
    data.value = await lndParametersApi.get()
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  } finally {
    loading.value = false
  }
}
onMounted(load)
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
.c3-body { padding: 16px 4px 4px; display: flex; flex-direction: column; gap: 14px; }
.c3-tab { display: flex; flex-direction: column; gap: 14px; }
.fc-tab em.ms-count { font-style: normal; opacity: 1; margin-inline-start: 6px; padding: 0 7px; border-radius: 999px; background: var(--fin-orange); color: #fff; font-size: 11px; line-height: 18px; }
.lp-check { display: flex; gap: 10px; align-items: flex-start; padding: 12px 14px; border-radius: var(--fin-radius-sm); font-size: var(--fs-sm, 13px); }
.lp-check--bad { background: var(--fin-bad-50); color: var(--fin-ink); }
.lp-check--bad > .fa { color: var(--fin-bad); font-size: 18px; margin-top: 1px; }
.lp-check ul { margin: 6px 0 0; padding-inline-start: 18px; color: var(--fin-muted); }
.lp-legal { margin-inline-start: 8px; font-size: 11px; vertical-align: 2px; }
.lp-next, .lp-source { display: block; margin-top: 2px; }
.lp-next .fa { margin-inline-end: 5px; }
.lp-suggest { display: flex; gap: 10px; align-items: center; justify-content: space-between; flex-wrap: wrap; margin: 10px 0 0; }
.ms-list { display: flex; flex-direction: column; }
.ms-row { display: grid; grid-template-columns: minmax(0, 1fr) auto; gap: 8px 16px; align-items: center; padding: 12px 4px; border-top: 1px solid var(--fin-line); }
.ms-row:first-child { border-top: 0; padding-top: 2px; }
.ms-text b { display: block; color: var(--fin-ink); font-size: var(--fs-md, 14px); }
.ms-text p { margin: 3px 0 4px; color: var(--fin-muted); font-size: var(--fs-sm, 13px); line-height: 1.45; max-width: 760px; }
.ms-value { display: flex; align-items: center; gap: 10px; justify-content: flex-end; }
.ms-value .fx-chip { min-width: 72px; justify-content: center; }
.ms-pending { grid-column: 1 / -1; display: flex; align-items: center; gap: 10px; flex-wrap: wrap; padding: 10px 12px; border-radius: var(--fin-radius-sm);
  background: var(--fin-warn-50); color: var(--fin-ink); font-size: var(--fs-sm, 13px); }
.ms-pending > .fa { color: var(--fin-orange); font-size: 16px; }
.ms-pending-text { flex: 1; min-width: 220px; display: flex; flex-direction: column; gap: 2px; }
.ms-pending-text small { color: var(--fin-muted); }
.c3-grid--2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
@media (max-width: 700px) {
  .ms-row { grid-template-columns: 1fr; }
  .ms-value { justify-content: flex-start; }
  .c3-grid--2 { grid-template-columns: 1fr; }
}
</style>
