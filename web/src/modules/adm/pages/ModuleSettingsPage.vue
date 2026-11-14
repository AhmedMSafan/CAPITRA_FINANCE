<template>
  <!-- Administration > Module Settings (Ahmed, 7 Nov 2026): the rules each module used to have fixed in code, one tab per module.
       A change is a request; it takes effect only when another user approves it. The History tab keeps every request and decision. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'adm-home' }">{{ tr('Administration') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ tr('Module Settings') }}</b>
        </nav>
        <h1>{{ tr('Module Settings') }}</h1>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openManual"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
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
        <div v-if="loading && !data" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading settings...') }}</div>

        <div v-else-if="module" class="c3-tab">
          <p class="c3-note"><span class="fa fa-shield" aria-hidden="true"></span>{{ tr('A change takes effect only after another user approves it. Until then the current value stays in force.') }}</p>

          <section v-for="g in groups" :key="g.name" class="c3-panel">
            <header class="c3-panel-head"><h3><span class="fa" :class="module.icon" aria-hidden="true"></span>{{ tr(g.name) }}</h3></header>
            <div class="ms-list">
              <article v-for="s in g.settings" :key="s.code" class="ms-row" :class="{ 'ms-row--child': s.shownWhen, 'ms-row--pending': s.pendingValue !== null }">
                <div class="ms-text">
                  <b>{{ tr(s.label) }}</b>
                  <p v-if="s.help">{{ tr(s.help) }}</p>
                  <small class="c3-muted">
                    <template v-if="s.isDefault && !s.approvedBy">{{ tr('Default value') }}</template>
                    <template v-else-if="s.approvedBy">{{ tr('Changed by') }} {{ s.changedBy }} · {{ tr('approved by') }} {{ s.approvedBy }} · {{ formatDateTime(s.approvedAt) }}</template>
                  </small>
                </div>
                <div class="ms-value">
                  <span class="fx-chip" :class="valueChip(s)">{{ show(s, s.value) }}</span>
                  <button v-if="canEdit && s.pendingValue === null" type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="openChange(s)">
                    <span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Change') }}
                  </button>
                </div>
                <div v-if="s.pendingValue !== null" class="ms-pending">
                  <span class="fa fa-clock-o" aria-hidden="true"></span>
                  <div class="ms-pending-text">
                    <b>{{ tr('Waiting for approval:') }} {{ show(s, s.value) }} → {{ show(s, s.pendingValue) }}</b>
                    <small>{{ s.requestedBy }} · {{ formatDateTime(s.requestedAt) }}<template v-if="s.pendingReason"> · {{ s.pendingReason }}</template></small>
                  </div>
                  <div v-if="canEdit" class="c3-row-actions">
                    <template v-if="s.canApprove">
                      <button type="button" class="fx-btn fx-btn--sm fx-btn--teal" :disabled="busy" @click="openDecision(s, 'approve')"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Approve') }}</button>
                      <button type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="openDecision(s, 'reject')"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Reject') }}</button>
                    </template>
                    <template v-else-if="mine(s)">
                      <span class="c3-muted">{{ tr(s.approveBlockedReason ?? '') }}</span>
                      <button type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="withdraw(s)"><span class="fa fa-undo" aria-hidden="true"></span>{{ tr('Withdraw') }}</button>
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
              <thead><tr><th>{{ tr('When') }}</th><th>{{ tr('By') }}</th><th>{{ tr('Setting') }}</th><th>{{ tr('Action') }}</th><th>{{ tr('From') }}</th><th>{{ tr('To') }}</th><th>{{ tr('Notes') }}</th></tr></thead>
              <tbody>
                <tr v-for="h in data.history" :key="h.historyId">
                  <td>{{ formatDateTime(h.at) }}</td><td>{{ h.by }}</td><td>{{ tr(h.label) }}</td>
                  <td><span class="fx-chip" :class="ACTION_CHIP[h.actionCode]">{{ tr(ACTIONS[h.actionCode]) }}</span></td>
                  <td>{{ tr(h.oldValue ?? '') }}</td><td>{{ tr(h.newValue ?? '') }}</td><td>{{ h.notes ?? '' }}</td>
                </tr>
              </tbody>
            </table>
            <div v-if="!data.history.length" class="fx-empty"><span class="fa fa-history" aria-hidden="true"></span>{{ tr('No changes recorded yet.') }}</div>
          </div>
        </div>
      </div>
    </section>

    <Crm360Dialog v-model:visible="changing" :title="tr('Change Setting')" :subtitle="current ? tr(current.label) : ''" icon="fa-sliders" :busy="busy" :width="560">
      <template v-if="current">
        <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Now:') }} {{ show(current, current.value) }}. {{ tr('Another user must approve the change before it takes effect.') }}</p>
        <div class="c3-grid c3-grid--2">
          <label class="fc-field" :class="bad('value')"><span>{{ tr('New value') }}<template v-if="current.unit && current.type !== 'BOOL'"> ({{ tr(current.unit) }})</template><b> *</b></span>
            <select v-if="current.type === 'BOOL'" v-model="value" class="fx-select"><option value="true">{{ tr('Yes') }}</option><option value="false">{{ tr('No') }}</option></select>
            <select v-else-if="current.type === 'CHOICE'" v-model="value" class="fx-select"><option v-for="c in current.choices" :key="c.value" :value="c.value">{{ tr(c.label) }}</option></select>
            <input v-else v-model="value" class="fx-search" inputmode="decimal">
          </label>
          <div class="fc-field"><span>{{ tr('Default') }}</span><div class="ms-default">{{ defaultLabel }}</div></div>
          <label class="fc-field c3-wide" :class="bad('reason')"><span>{{ tr('Reason') }}<b> *</b></span><input v-model="reason" class="fx-search" maxlength="1000"></label>
        </div>
        <p v-if="current.min !== null && current.type !== 'BOOL' && current.type !== 'CHOICE'" class="c3-muted">{{ tr('From') }} {{ current.min }} {{ tr('to') }} {{ current.max }}{{ current.unit ? ' ' + tr(current.unit) : '' }}.</p>
      </template>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="changing = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="request"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-paper-plane'" aria-hidden="true"></span>{{ tr('Ask for approval') }}</button>
      </template>
    </Crm360Dialog>

    <Crm360Dialog v-model:visible="deciding" :title="tr(decision === 'approve' ? 'Approve Change' : 'Reject Change')" :subtitle="current ? tr(current.label) : ''"
                  :icon="decision === 'approve' ? 'fa-check-circle' : 'fa-times-circle'" :busy="busy" :width="520">
      <p v-if="current" class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ show(current, current.value) }} → {{ show(current, current.pendingValue) }} · {{ current.requestedBy }}<template v-if="current.pendingReason">: {{ current.pendingReason }}</template></p>
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
import { computed, onMounted, ref } from 'vue'
import { useRoute } from 'vue-router'
import { ApiError } from '@/shared/api/http'
import { getUser, hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDateTime } from '@/shared/format/datetime'
import { currentLang, tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { moduleSettingsApi, type ModuleSetting, type ModuleSettingsPage } from '../api/moduleSettings.api'

const canEdit = hasPermission('ADM.SETTINGS.EDIT')
const me = (getUser()?.userName ?? '').toLowerCase()

const ACTIONS = { REQUEST: 'Requested', APPROVE: 'Approved', REJECT: 'Rejected', CANCEL: 'Withdrawn' } as const
const ACTION_CHIP = { REQUEST: 'fx-chip--warn', APPROVE: 'fx-chip--ok', REJECT: 'fx-chip--bad', CANCEL: 'fx-chip--teal' } as const

const data = ref<ModuleSettingsPage | null>(null)
const loading = ref(false)
const busy = ref(false)
// the Administration cards open a module's tab (?tab=TRS)
const asked = String(useRoute().query.tab ?? 'CRM').toUpperCase()
const tab = ref(asked === 'HISTORY' ? 'history' : asked)

const tabs = computed(() => [
  ...(data.value?.modules ?? []).map((m) => ({ key: m.code, label: m.title, icon: m.icon, count: m.pendingCount })),
  { key: 'history', label: 'History', icon: 'fa-history', count: 0 }
])
const module = computed(() => data.value?.modules.find((m) => m.code === tab.value) ?? null)

/** The module's settings by group, in the catalog's order; a setting that depends on a yes/no one is shown only while that one is Yes. */
const groups = computed(() => {
  const all = module.value?.settings ?? []
  const on = (code: string) => all.find((x) => x.code === code)?.value === 'true'
  const out: { name: string; settings: ModuleSetting[] }[] = []
  for (const s of all) {
    if (s.shownWhen && !on(s.shownWhen)) continue
    let g = out.find((x) => x.name === s.group)
    if (!g) out.push((g = { name: s.group, settings: [] }))
    g.settings.push(s)
  }
  return out
})

const valueChip = (s: ModuleSetting) => (s.type === 'BOOL' ? (s.value === 'true' ? 'fx-chip--ok' : 'fx-chip--bad') : 'fx-chip--teal')
const mine = (s: ModuleSetting) => !!me && (s.requestedBy ?? '').toLowerCase() === me

// ---- dialogs
const changing = ref(false)
const deciding = ref(false)
const decision = ref<'approve' | 'reject'>('approve')
const current = ref<ModuleSetting | null>(null)
const value = ref('')
const reason = ref('')
const notes = ref('')
const errors = ref<Record<string, string[]>>({})
const bad = (f: string) => ({ 'c3-bad': !!errors.value[f] })
/** A value in words, in the page language: Yes / No, the choice's label, or the number with its unit. */
function show(s: ModuleSetting, v: string | null): string {
  if (v === null) return ''
  if (s.type === 'BOOL') return tr(v === 'true' ? 'Yes' : 'No')
  if (s.type === 'CHOICE') return tr(s.choices.find((c) => c.value === v)?.label ?? v)
  return `${v}${s.unit === '%' ? '%' : s.unit ? ' ' + tr(s.unit) : ''}`
}
const defaultLabel = computed(() => (current.value ? show(current.value, current.value.defaultValue) : ''))

function openChange(s: ModuleSetting) {
  current.value = s
  // a yes/no setting proposes the other answer; the others start from the value in force
  value.value = s.type === 'BOOL' ? (s.value === 'true' ? 'false' : 'true') : s.value
  reason.value = ''
  errors.value = {}
  changing.value = true
}
function openDecision(s: ModuleSetting, d: 'approve' | 'reject') {
  current.value = s
  decision.value = d
  notes.value = ''
  errors.value = {}
  deciding.value = true
}

async function run(action: () => Promise<ModuleSettingsPage>, done: string, close: () => void) {
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

const request = () => run(() => moduleSettingsApi.request(current.value!.code, String(value.value).trim(), reason.value),
  'Change sent for approval. Another user must approve it.', () => (changing.value = false))
const decide = () => decision.value === 'approve'
  ? run(() => moduleSettingsApi.approve(current.value!.code, notes.value || null), 'Change approved. It is now in force.', () => (deciding.value = false))
  : run(() => moduleSettingsApi.reject(current.value!.code, notes.value), 'Change rejected.', () => (deciding.value = false))
const withdraw = (s: ModuleSetting) => run(() => moduleSettingsApi.cancel(s.code), 'Change withdrawn.', () => {})

/** The Module Settings manual (public/manuals/administration), in the page language, at the open tab's section. */
function openManual() {
  const lang = currentLang() === 'AR' ? 'ar' : 'en'
  const section = ({ CRM: '2', TRS: '3', FCT: '4', history: '5' } as Record<string, string>)[tab.value] ?? '1'
  window.open(`${import.meta.env.BASE_URL}manuals/administration/module-settings-manual.html#${lang}-${section}`, '_blank', 'noopener')
}

async function load() {
  loading.value = true
  try {
    data.value = await moduleSettingsApi.get()
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
.c3-body { padding: 16px 4px 4px; }
.fc-tab em.ms-count { font-style: normal; opacity: 1; margin-inline-start: 6px; padding: 0 7px; border-radius: 999px; background: var(--fin-orange); color: #fff; font-size: 11px; line-height: 18px; }
.ms-list { display: flex; flex-direction: column; }
.ms-row { display: grid; grid-template-columns: minmax(0, 1fr) auto; gap: 8px 16px; align-items: center; padding: 12px 4px; border-top: 1px solid var(--fin-line); }
.ms-row:first-child { border-top: 0; padding-top: 2px; }
.ms-row--child { padding-inline-start: 24px; }
.ms-text b { display: block; color: var(--fin-ink); font-size: var(--fs-md, 14px); }
.ms-text p { margin: 3px 0 4px; color: var(--fin-muted); font-size: var(--fs-sm, 13px); line-height: 1.45; max-width: 760px; }
.ms-value { display: flex; align-items: center; gap: 10px; justify-content: flex-end; }
.ms-value .fx-chip { min-width: 64px; justify-content: center; }
.ms-pending { grid-column: 1 / -1; display: flex; align-items: center; gap: 10px; flex-wrap: wrap; padding: 10px 12px; border-radius: var(--fin-radius-sm);
  background: var(--fin-warn-50); color: var(--fin-ink); font-size: var(--fs-sm, 13px); }
.ms-pending > .fa { color: var(--fin-orange); font-size: 16px; }
.ms-pending-text { flex: 1; min-width: 220px; display: flex; flex-direction: column; gap: 2px; }
.ms-pending-text small { color: var(--fin-muted); }
.ms-default { padding-top: 8px; color: var(--fin-ink); }
.c3-grid--2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
@media (max-width: 700px) {
  .ms-row { grid-template-columns: 1fr; }
  .ms-value { justify-content: flex-start; }
  .c3-grid--2 { grid-template-columns: 1fr; }
}
</style>
