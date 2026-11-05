<template>
  <!-- Treasury > Bank Limits (agreed with Ahmed, 5 Nov 2026): each bank's facilities as a tree of facility, released tranches and the
       sublimits that fund our business lines, with bars for outstanding (teal), reserved (orange) and available. What can be drawn at a
       node is the smallest of what is free there and above it. The side panel shows the selected node and the bank's rules. -->
  <div class="fx fc trs">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <span>{{ tr('Treasury') }}</span><span aria-hidden="true">›</span>
          <b>{{ tr('Bank Limits') }}</b>
        </nav>
        <h1>{{ tr('Bank Limits') }}</h1>
        <p class="trs-lead">{{ tr("Each bank's facilities as a tree: facility, released tranches, and the sublimits that fund our business lines.") }}</p>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" :disabled="loading" @click="load"><span class="fa" :class="loading ? 'fa-spinner fa-spin' : 'fa-refresh'" aria-hidden="true"></span>{{ tr('Refresh') }}</button>
        <button v-if="canEdit" type="button" class="fx-btn fx-btn--teal" :disabled="!banks.length" @click="openFacility(null)">
          <span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New Facility') }}</button>
      </div>
    </div>

    <div class="trs-kpis">
      <div v-for="k in kpis" :key="k.label" class="fx-card trs-kpi">
        <span class="trs-k">{{ tr(k.label) }}</span>
        <b>{{ k.value }}</b>
        <small>{{ k.note }}</small>
      </div>
    </div>

    <div class="trs-body">
      <aside class="trs-banks" :aria-label="tr('Banks')">
        <span class="trs-k">{{ tr('Banks') }}</span>
        <button v-for="b in banks" :key="b.bankId" type="button" class="trs-bank" :class="{ on: b.bankId === bankId }" @click="pickBank(b)">
          <b>{{ b.bankName }}</b>
          <span class="trs-bar"><span class="o" :style="{ width: pct(b.outstanding, b.approvedLimit) + '%' }"></span><span class="r" :style="{ width: pct(b.reserved, b.approvedLimit) + '%' }"></span></span>
          <small>{{ money(b.available) }} {{ tr('available of') }} {{ money(b.approvedLimit) }}</small>
        </button>
        <div v-if="!banks.length && !loading" class="fx-empty">{{ tr('No bank facilities yet.') }}</div>
      </aside>

      <section class="fx-card trs-tree">
        <header class="trs-tree-head">
          <b>{{ bank?.bankName ?? '—' }}</b>
          <span class="trs-legend"><span class="sw o"></span>{{ tr('Outstanding') }}<span class="sw r"></span>{{ tr('Reserved') }}<span class="sw a"></span>{{ tr('Available') }}</span>
        </header>
        <div class="trs-row trs-row--head" aria-hidden="true">
          <span></span>
          <span class="trs-cols"><span class="trs-k">{{ tr('Limit') }}</span><span class="trs-k trs-hide">{{ tr('Use') }}</span>
            <span class="trs-k num trs-hide">{{ tr('Approved') }}</span><span class="trs-k num">{{ tr('Available') }}</span></span>
        </div>
        <div v-for="r in rows" :key="r.key" class="trs-row" :class="{ on: r.key === selKey }">
          <button v-if="r.hasKids" type="button" class="trs-caret" :aria-label="(r.open ? tr('Collapse') : tr('Expand')) + ' ' + r.name" @click="toggle(r.key)">
            <span class="fa" :class="r.open ? 'fa-chevron-down' : 'fa-chevron-right'" aria-hidden="true"></span></button>
          <span v-else></span>
          <button type="button" class="trs-cols trs-pick" @click="selKey = r.key">
            <span class="trs-name" :style="{ paddingInlineStart: r.level * 26 + 'px' }">
              <span class="trs-title"><span class="fa" :class="r.icon" aria-hidden="true"></span><b>{{ r.name }}</b>
                <span class="fx-chip" :class="statusChip(r.status)">{{ tr(label(r.status)) }}</span>
                <span v-if="r.rules" class="fx-chip fx-chip--teal fx-chip--plain">{{ r.rules }} {{ tr('bank rules') }}</span></span>
              <small>{{ r.sub }}</small>
            </span>
            <span class="trs-use trs-hide">
              <span class="trs-bar"><span class="o" :style="{ width: pct(r.out, r.approved) + '%' }"></span><span class="r" :style="{ width: pct(r.res, r.approved) + '%' }"></span></span>
              <small>{{ money(r.out) }} {{ tr('used') }} · {{ money(r.res) }} {{ tr('reserved') }}</small>
            </span>
            <span class="num trs-hide">{{ money(r.approved) }}</span>
            <span class="num trs-avail" :class="r.availCls">{{ r.status === 'DRAFT' ? tr('Not released') : money(r.avail) }}</span>
          </button>
        </div>
        <div v-if="bank && !bank.facilities.length" class="fx-empty">{{ tr('This bank has no open facility.') }}</div>
      </section>

      <aside v-if="sel" class="fx-card trs-side">
        <div>
          <span class="trs-k">{{ tr(sel.kind) }}</span>
          <h2>{{ sel.name }}</h2>
          <small class="trs-muted">{{ sel.sub }}</small>
        </div>
        <span class="trs-bar trs-bar--lg"><span class="o" :style="{ width: pct(sel.out, sel.approved) + '%' }"></span><span class="r" :style="{ width: pct(sel.res, sel.approved) + '%' }"></span></span>
        <dl class="trs-facts">
          <div v-for="f in sel.facts" :key="f.k"><dt>{{ tr(f.k) }}</dt><dd>{{ f.v }}</dd></div>
        </dl>

        <div v-if="sel.facility && !sel.tranche" class="trs-rules">
          <div class="trs-rules-head"><b>{{ tr('Bank rules') }}</b><small class="trs-muted">{{ tr('checked at every drawdown') }}</small></div>
          <div v-for="u in sel.rules" :key="u.ruleId" class="trs-rule">
            <div class="trs-rule-top">
              <span class="trs-rule-subject">{{ u.subjectLabel }}</span>
              <span class="fx-chip fx-chip--plain" :class="ruleChip(u.ruleTypeCode)">{{ tr(ruleName(u.ruleTypeCode)) }}</span>
            </div>
            <small class="trs-muted">{{ ruleLimit(u) }} · {{ tr('on breach:') }} {{ tr(actionName(u.breachActionCode)) }}</small>
            <small v-if="u.highest" :class="'trs-lvl-' + u.highest.level">
              {{ tr('Highest now:') }} {{ u.highest.subjectName }} {{ u.highest.pct !== null ? u.highest.pct + '%' : money(u.highest.exposure) }}</small>
            <small v-else-if="u.ruleTypeCode === 'CONCENTRATION'" class="trs-muted">{{ tr('No debtor or seller exposure recorded yet.') }}</small>
            <div class="trs-rule-foot">
              <span v-if="u.statusCode === 'PENDING'" class="fx-chip fx-chip--warn">{{ tr('Waiting for verification') }}</span>
              <span v-else class="trs-muted">{{ tr('Verified by') }} {{ u.verifiedBy }}</span>
              <span class="trs-rule-btns">
                <button v-if="u.statusCode === 'PENDING' && canVerify" type="button" class="fx-btn fx-btn--sm fx-btn--teal" :disabled="busy || !u.canVerify"
                        :title="u.verifyBlockedReason ? tr(u.verifyBlockedReason) : ''" @click="verify(u)"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Verify') }}</button>
                <button v-if="canEdit" type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="openRule(u)"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}</button>
                <button v-if="canEdit" type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="openDeactivate(u)"><span class="fa fa-ban" aria-hidden="true"></span>{{ tr('Stop') }}</button>
              </span>
            </div>
          </div>
          <div v-if="!sel.rules.length" class="trs-muted">{{ tr('No rules for this facility yet.') }}</div>
        </div>

        <div v-if="canEdit" class="trs-side-btns">
          <button type="button" class="fx-btn" @click="editSelected"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}</button>
          <button v-if="sel.facility && !sel.tranche" type="button" class="fx-btn" @click="openTranche(sel.facility, null)"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Tranche') }}</button>
          <button v-if="sel.tranche && !sel.sublimit" type="button" class="fx-btn" @click="openSublimit(sel.tranche, null)"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Sublimit') }}</button>
          <button v-if="sel.facility && !sel.tranche" type="button" class="fx-btn" @click="openRule(null)"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Rule') }}</button>
        </div>
      </aside>
    </div>

    <TrsFacilityDialog v-model:visible="dlg.facility" :facility="editFacility" :bank-id="bankId" :banks="banks" :types="options?.facilityTypes ?? []" @saved="load" />
    <TrsTrancheDialog v-model:visible="dlg.tranche" :facility="ctxFacility" :tranche="editTranche" @saved="load" />
    <TrsSublimitDialog v-model:visible="dlg.sublimit" :tranche="ctxTranche" :sublimit="editSublimit" :lines="options?.businessLines ?? []" @saved="load" />
    <TrsRuleDialog v-if="options" v-model:visible="dlg.rule" :bank="bank" :rule="editRule" :options="options" :facility-id="sel?.facility?.facilityId ?? null" @saved="load" />

    <Crm360Dialog v-model:visible="dlg.stop" :title="tr('Stop Bank Rule')" :subtitle="editRule?.subjectLabel ?? ''" icon="fa-ban" :busy="busy" :width="520">
      <label class="fc-field"><span>{{ tr('Reason') }}<b> *</b></span><input v-model="stopReason" class="fx-search" maxlength="1000"></label>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="dlg.stop = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn" :disabled="busy || !stopReason.trim()" @click="deactivate"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-ban'" aria-hidden="true"></span>{{ tr('Stop Rule') }}</button>
      </template>
    </Crm360Dialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import {
  trsLimitsApi, type TrsBankNode, type TrsBankRule, type TrsFacilityNode, type TrsLimitsOptions, type TrsSublimitNode, type TrsTrancheNode
} from '../api/trsLimits.api'
import TrsFacilityDialog from '../components/TrsFacilityDialog.vue'
import TrsRuleDialog from '../components/TrsRuleDialog.vue'
import TrsSublimitDialog from '../components/TrsSublimitDialog.vue'
import TrsTrancheDialog from '../components/TrsTrancheDialog.vue'
import { label, money, pct, save, statusChip } from '../components/trs'

const canEdit = hasPermission('TRS.EDIT')
const canVerify = hasPermission('TRS.VERIFY')

const banks = ref<TrsBankNode[]>([])
const options = ref<TrsLimitsOptions | null>(null)
const loading = ref(false)
const busy = ref(false)
const bankId = ref<number | null>(null)
const selKey = ref('')
const closed = ref<Record<string, boolean>>({})

async function load() {
  loading.value = true
  try {
    const [limits, opts] = await Promise.all([trsLimitsApi.get(), options.value ? Promise.resolve(options.value) : trsLimitsApi.options()])
    banks.value = limits.banks
    options.value = opts
    if (!banks.value.some((b) => b.bankId === bankId.value)) bankId.value = banks.value[0]?.bankId ?? null
    if (!rows.value.some((r) => r.key === selKey.value)) selKey.value = rows.value[0]?.key ?? ''
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  } finally {
    loading.value = false
  }
}
onMounted(load)

const bank = computed(() => banks.value.find((b) => b.bankId === bankId.value) ?? null)

function pickBank(b: TrsBankNode) {
  bankId.value = b.bankId
  selKey.value = b.facilities[0] ? `f${b.facilities[0].facilityId}` : ''
}

// ------------------------------------------------------------------ KPIs

const kpis = computed(() => {
  const fs = banks.value.flatMap((b) => b.facilities)
  const sum = (f: (x: TrsFacilityNode) => number) => fs.reduce((a, x) => a + f(x), 0)
  const approved = sum((x) => x.approvedLimit)
  const out = sum((x) => x.outstanding)
  const notReleased = sum((x) => x.notReleased)
  return [
    { label: 'Approved Limits', value: money(approved), note: `${banks.value.length} ${tr('banks')} · ${fs.length} ${tr('facilities')}` },
    { label: 'Outstanding', value: money(out), note: `${Math.round(pct(out, approved) * 10) / 10}% ${tr('of approved')}` },
    { label: 'Reserved', value: money(sum((x) => x.reserved)), note: tr('held for approved disbursements') },
    { label: 'Available To Draw', value: money(sum((x) => x.available)),
      note: notReleased > 0 ? `${tr('excludes')} ${money(notReleased)} ${tr('not yet released')}` : tr('all limits released') }
  ]
})

// ------------------------------------------------------------------ tree

type Row = {
  key: string; level: number; name: string; sub: string; status: string; icon: string; approved: number; out: number; res: number; avail: number
  availCls: string; hasKids: boolean; open: boolean; rules: number
}

const rulesFor = (f: TrsFacilityNode) => (bank.value?.rules ?? []).filter((r) => r.facilityId === null || r.facilityId === f.facilityId)
const lineNames = (s: TrsSublimitNode) =>
  s.businessLines.length ? s.businessLines.map((v) => tr(options.value?.businessLines.find((o) => o.value === v)?.label ?? label(v))).join(', ') : tr('all business lines')
const facilitySub = (f: TrsFacilityNode) => {
  const rate = f.interestRateType === 'FIXED' ? `${tr('fixed')} ${f.allInRatePct ?? '—'}%`
    : `${f.baseRateCode ?? tr('base')} + ${((f.marginBps ?? 0) / 100).toFixed(2)}%${f.allInRatePct ? ` (${f.allInRatePct}%)` : ''}`
  const type = options.value?.facilityTypes.find((o) => o.value === f.facilityTypeCode)?.label ?? label(f.facilityTypeCode)
  return `${tr(type)} · ${f.currencyCode} · ${rate} · ${formatDate(f.startDate)} ${tr('to')} ${formatDate(f.maturityDate)}`
}
const trancheSub = (t: TrsTrancheNode) =>
  t.statusCode === 'DRAFT' ? tr('Released by the bank on request')
    : `${t.releasedDate ? tr('Released') + ' ' + formatDate(t.releasedDate) : tr('Released')}${t.maturityDate ? ' · ' + tr('matures') + ' ' + formatDate(t.maturityDate) : ''}`
const sublimitSub = (s: TrsSublimitNode) => `${tr('Scope')}: ${lineNames(s)} · ${s.poolTypeCode === 'DEDICATED' ? tr('dedicated pool') : tr('shared cap')}`
const availCls = (avail: number, own: number) => (avail < own ? 'is-capped' : 'is-free')

const rows = computed<Row[]>(() => {
  const list: Row[] = []
  for (const f of bank.value?.facilities ?? []) {
    const fk = `f${f.facilityId}`
    const fOpen = !closed.value[fk]
    list.push({ key: fk, level: 0, name: f.facilityRef, sub: facilitySub(f), status: f.facilityStatus, icon: 'fa-university', approved: f.approvedLimit,
      out: f.outstanding, res: f.reserved, avail: f.available, availCls: availCls(f.available, f.approvedLimit - f.outstanding - f.reserved),
      hasKids: f.tranches.length > 0, open: fOpen, rules: rulesFor(f).length })
    if (!fOpen) continue
    for (const t of f.tranches) {
      const tk = `t${t.trancheId}`
      const tOpen = !closed.value[tk]
      list.push({ key: tk, level: 1, name: t.trancheName, sub: trancheSub(t), status: t.statusCode, icon: 'fa-th-large', approved: t.trancheAmount,
        out: t.outstanding, res: t.reserved, avail: t.available, availCls: availCls(t.available, t.trancheAmount - t.outstanding - t.reserved),
        hasKids: t.sublimits.length > 0, open: tOpen, rules: 0 })
      if (!tOpen) continue
      for (const s of t.sublimits) {
        list.push({ key: `s${s.sublimitId}`, level: 2, name: s.sublimitName, sub: sublimitSub(s), status: s.statusCode, icon: 'fa-sitemap',
          approved: s.limitAmount, out: s.outstanding, res: s.reserved, avail: s.available,
          availCls: availCls(s.available, s.limitAmount - s.outstanding - s.reserved), hasKids: false, open: false, rules: 0 })
      }
    }
  }
  return list
})

function toggle(key: string) {
  closed.value = { ...closed.value, [key]: !closed.value[key] }
}

// ------------------------------------------------------------------ selected node

type Sel = {
  kind: string; name: string; sub: string; approved: number; out: number; res: number; facts: { k: string; v: string }[]
  facility: TrsFacilityNode | null; tranche: TrsTrancheNode | null; sublimit: TrsSublimitNode | null; rules: TrsBankRule[]
}

const sel = computed<Sel | null>(() => {
  const key = selKey.value
  for (const f of bank.value?.facilities ?? []) {
    const base = (approved: number, out: number, res: number, avail: number, status: string) => [
      { k: 'Approved', v: formatAmount(approved) }, { k: 'Outstanding', v: formatAmount(out) }, { k: 'Reserved', v: formatAmount(res) },
      { k: 'Available', v: status === 'DRAFT' ? tr('Not released') : formatAmount(avail) }, { k: 'Status', v: tr(label(status)) },
      { k: 'Bank', v: bank.value?.bankName ?? '' }]
    if (key === `f${f.facilityId}`)
      return { kind: 'Facility', name: f.facilityRef, sub: facilitySub(f), approved: f.approvedLimit, out: f.outstanding, res: f.reserved,
        facts: [...base(f.approvedLimit, f.outstanding, f.reserved, f.available, f.facilityStatus),
          { k: 'Not Released', v: formatAmount(f.notReleased) }, { k: 'Revolving', v: f.isRevolving ? tr('Yes') : tr('No') }],
        facility: f, tranche: null, sublimit: null, rules: rulesFor(f) }
    for (const t of f.tranches) {
      if (key === `t${t.trancheId}`)
        return { kind: 'Tranche', name: t.trancheName, sub: `${f.facilityRef} · ${trancheSub(t)}`, approved: t.trancheAmount, out: t.outstanding, res: t.reserved,
          facts: base(t.trancheAmount, t.outstanding, t.reserved, t.available, t.statusCode), facility: f, tranche: t, sublimit: null, rules: [] }
      for (const s of t.sublimits)
        if (key === `s${s.sublimitId}`)
          return { kind: 'Sublimit', name: s.sublimitName, sub: `${f.facilityRef} › ${t.trancheName} · ${sublimitSub(s)}`, approved: s.limitAmount,
            out: s.outstanding, res: s.reserved, facts: base(s.limitAmount, s.outstanding, s.reserved, s.available, s.statusCode),
            facility: f, tranche: t, sublimit: s, rules: [] }
    }
  }
  return null
})

const ruleName = (c: string) => options.value?.ruleTypes.find((o) => o.value === c)?.label ?? label(c)
const actionName = (c: string) => options.value?.breachActions.find((o) => o.value === c)?.label ?? label(c)
const ruleChip = (c: string) => (c === 'CONCENTRATION' ? 'fx-chip--teal' : c === 'EXCLUDE' ? 'fx-chip--bad' : '')
function ruleLimit(u: TrsBankRule): string {
  const scope = u.facilityRef ? ` · ${tr('only')} ${u.facilityRef}` : ''
  if (u.ruleTypeCode === 'EXCLUDE') return tr('Not financed by this bank') + scope
  if (u.ruleTypeCode === 'MAX_TENOR') return `${tr('At most')} ${u.maxTenorDays} ${tr('days')}${scope}`
  const basis = u.basisCode === 'FACILITY_LIMIT' ? tr('of the facility limit') : tr('of the funded pool')
  const lim = u.limitPct !== null ? `${u.limitPct}% ${basis}` : money(u.limitAmount)
  const warn = u.warningPct !== null ? ` · ${tr('warn at')} ${u.warningPct}%` : ''
  return `${tr('At most')} ${lim}${warn}${scope}`
}

// ------------------------------------------------------------------ dialogs

const dlg = reactive({ facility: false, tranche: false, sublimit: false, rule: false, stop: false })
const editFacility = ref<TrsFacilityNode | null>(null)
const ctxFacility = ref<TrsFacilityNode | null>(null)
const editTranche = ref<TrsTrancheNode | null>(null)
const ctxTranche = ref<TrsTrancheNode | null>(null)
const editSublimit = ref<TrsSublimitNode | null>(null)
const editRule = ref<TrsBankRule | null>(null)
const stopReason = ref('')

function openFacility(f: TrsFacilityNode | null) {
  editFacility.value = f
  dlg.facility = true
}
function openTranche(f: TrsFacilityNode, t: TrsTrancheNode | null) {
  ctxFacility.value = f
  editTranche.value = t
  dlg.tranche = true
}
function openSublimit(t: TrsTrancheNode, s: TrsSublimitNode | null) {
  ctxTranche.value = t
  editSublimit.value = s
  dlg.sublimit = true
}
function openRule(r: TrsBankRule | null) {
  editRule.value = r
  dlg.rule = true
}
function openDeactivate(r: TrsBankRule) {
  editRule.value = r
  stopReason.value = ''
  dlg.stop = true
}
function editSelected() {
  const s = sel.value
  if (!s?.facility) return
  if (s.sublimit && s.tranche) openSublimit(s.tranche, s.sublimit)
  else if (s.tranche) openTranche(s.facility, s.tranche)
  else openFacility(s.facility)
}

async function verify(r: TrsBankRule) {
  busy.value = true
  await save(() => trsLimitsApi.verifyRule(r.ruleId), 'Rule verified. It is checked from now on.', load)
  busy.value = false
}
async function deactivate() {
  if (!editRule.value) return
  busy.value = true
  await save(() => trsLimitsApi.deactivateRule(editRule.value!.ruleId, stopReason.value.trim()), 'Rule stopped.', () => {
    dlg.stop = false
    void load()
  })
  busy.value = false
}
</script>

<style scoped>
.trs { display: flex; flex-direction: column; gap: 16px; }
.trs-lead { margin: 4px 0 0; color: var(--fin-muted); font-size: var(--fs-sm); }
.trs-k { font-size: var(--fs-xs); font-weight: 700; color: var(--fin-muted); text-transform: uppercase; letter-spacing: .03em; }
.trs-muted { color: var(--fin-muted); font-size: var(--fs-xs); }
.num { text-align: end; font-variant-numeric: tabular-nums; white-space: nowrap; }

.trs-kpis { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 12px; }
.trs-kpi { display: flex; flex-direction: column; gap: 4px; padding: 14px 16px; }
.trs-kpi b { font-size: 22px; font-variant-numeric: tabular-nums; color: var(--fin-ink); }
.trs-kpi small { color: var(--fin-muted); font-size: var(--fs-xs); }

.trs-body { display: flex; flex-wrap: wrap; gap: 16px; align-items: flex-start; }
.trs-banks { flex: 1 1 220px; max-width: 270px; display: flex; flex-direction: column; gap: 8px; }
.trs-bank { appearance: none; display: flex; flex-direction: column; gap: 6px; width: 100%; text-align: start; font: inherit; padding: 12px 14px; cursor: pointer;
  border: 1px solid var(--fin-line); border-radius: var(--fin-radius); background: var(--fin-surface); color: var(--fin-ink); }
.trs-bank:hover { border-color: var(--fin-teal-600); }
.trs-bank.on { border-color: var(--fin-teal-600); box-shadow: inset 4px 0 0 var(--fin-teal-600); background: var(--fin-teal-50); }
.trs-bank small { color: var(--fin-muted); font-size: var(--fs-xs); }

.trs-bar { display: flex; height: 10px; width: 100%; border-radius: 999px; overflow: hidden; background: var(--fin-teal-50); }
.trs-bar--lg { height: 12px; }
.trs-bar .o { background: var(--fin-teal-600); }
.trs-bar .r { background: var(--fin-orange); }

.trs-tree { flex: 999 1 520px; min-width: 0; padding: 0; overflow: hidden; }
.trs-tree-head { display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: 10px; padding: 12px 16px; border-block-end: 1px solid var(--fin-line); }
.trs-legend { display: inline-flex; align-items: center; gap: 6px; font-size: var(--fs-xs); color: var(--fin-muted); }
.trs-legend .sw { width: 10px; height: 10px; border-radius: 3px; margin-inline-start: 8px; }
.trs-legend .sw.o { background: var(--fin-teal-600); }
.trs-legend .sw.r { background: var(--fin-orange); }
.trs-legend .sw.a { background: var(--fin-teal-50); border: 1px solid var(--fin-line); }
.trs-row { display: grid; grid-template-columns: 36px minmax(0, 1fr); align-items: center; border-block-end: 1px solid var(--fin-line-soft); }
.trs-row.on { background: var(--fin-teal-50); }
.trs-row--head { background: #FAFCFC; border-block-end: 1px solid var(--fin-line); }
.trs-row--head .trs-cols { min-height: 32px; cursor: default; }
.trs-caret { width: 32px; height: 32px; border: 0; background: none; cursor: pointer; color: var(--fin-muted); border-radius: 6px; }
.trs-caret:hover { background: var(--fin-teal-50); color: var(--fin-teal-600); }
.trs-cols { display: grid; grid-template-columns: minmax(0, 1.7fr) minmax(0, 1.1fr) 90px 90px; gap: 14px; align-items: center; width: 100%;
  min-height: 56px; padding: 8px 16px 8px 0; }
.trs-pick { appearance: none; border: 0; background: none; text-align: start; font: inherit; color: var(--fin-ink); cursor: pointer; }
.trs-name { display: flex; flex-direction: column; gap: 3px; min-width: 0; }
.trs-name small { color: var(--fin-muted); font-size: var(--fs-xs); }
.trs-title { display: flex; align-items: center; flex-wrap: wrap; gap: 8px; }
.trs-title .fa { color: var(--fin-orange); width: 14px; text-align: center; }
.trs-use { display: flex; flex-direction: column; gap: 5px; }
.trs-use small { color: var(--fin-muted); font-size: var(--fs-xs); }
.trs-avail { font-weight: 700; }
.trs-avail.is-free { color: var(--fin-ok); }
.trs-avail.is-capped { color: var(--fin-warn); }

.trs-side { flex: 1 1 340px; max-width: 400px; display: flex; flex-direction: column; gap: 14px; padding: 16px 18px; }
.trs-side h2 { margin: 4px 0 2px; font-size: var(--fs-lg); color: var(--fin-ink); }
.trs-facts { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 10px 16px; margin: 0; }
.trs-facts dt { font-size: var(--fs-xs); font-weight: 700; color: var(--fin-muted); }
.trs-facts dd { margin: 3px 0 0; font-variant-numeric: tabular-nums; }
.trs-rules-head { display: flex; align-items: center; justify-content: space-between; margin-block-end: 4px; }
.trs-rule { display: flex; flex-direction: column; gap: 4px; padding: 10px 0; border-block-start: 1px solid var(--fin-line-soft); }
.trs-rule-top { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
.trs-rule-subject { font-weight: 600; }
.trs-lvl-OK { color: var(--fin-ok); font-size: var(--fs-xs); }
.trs-lvl-WARN { color: var(--fin-warn); font-size: var(--fs-xs); font-weight: 700; }
.trs-lvl-BREACH { color: var(--fin-bad); font-size: var(--fs-xs); font-weight: 700; }
.trs-rule-foot { display: flex; align-items: center; justify-content: space-between; gap: 8px; flex-wrap: wrap; font-size: var(--fs-xs); }
.trs-rule-btns { display: flex; gap: 6px; }
.trs-side-btns { display: flex; flex-wrap: wrap; gap: 8px; }

@media (max-width: 1100px) { .trs-kpis { grid-template-columns: repeat(2, minmax(0, 1fr)); } .trs-banks, .trs-side { max-width: none; } }
@media (max-width: 760px) { .trs-cols { grid-template-columns: minmax(0, 1fr) 100px; } .trs-hide { display: none; } .trs-kpis { grid-template-columns: 1fr; } }
</style>
