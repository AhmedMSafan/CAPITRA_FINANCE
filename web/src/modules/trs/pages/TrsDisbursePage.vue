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
        <FctDisbursementSummary v-if="id > 0" :disbursement-id="id" from="trs" />
        <button type="button" class="fx-btn" @click="openManual(6)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
        <button type="button" class="fx-btn" :disabled="loading || busy" @click="load"><span class="fa" :class="loading ? 'fa-spinner fa-spin' : 'fa-refresh'" aria-hidden="true"></span>{{ tr('Refresh') }}</button>
        <FctStepActions v-if="!viewOnly" :save="saveForStep" :disabled="busy || loading" :complete-note="completeNote" />
      </div>
    </div>

    <FctViewOnlyNote v-if="viewOnly" :disbursement-id="id" step="TREASURY_DISBURSE" />
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
        <!-- the limits as a tree: bank › facility › tranche › sublimit; an amount is entered on a sublimit (or own funds) -->
        <template v-for="row in tree" :key="row.key">
          <div v-if="row.kind !== 'LINE'" class="trs-node" :class="'trs-node--' + row.kind">
            <span class="fa" :class="row.kind === 'BANK' ? 'fa-university' : row.kind === 'FACILITY' ? 'fa-file-text-o' : 'fa-folder-open-o'" aria-hidden="true"></span>
            <span class="trs-node-name">{{ row.label }}</span>
            <small class="trs-muted">{{ row.usable }} / {{ row.count }} {{ tr('usable') }}</small>
            <span class="num trs-node-sum">{{ row.chosen ? formatAmount(row.chosen) : '' }}</span>
          </div>
          <div v-else class="trs-line" :class="[row.source.unavailable ? 'is-off' : 'lvl-' + lineLevel(row.source), row.source.sublimitId ? 'trs-leaf' : '', row.deep ? 'trs-leaf--deep' : '']">
            <template v-for="s in [row.source]" :key="s.sourceKey">
            <span class="trs-name">
              <b>{{ s.sublimitId ? s.sublimitName : tr('Own funds') }}</b>
              <small v-if="!s.sublimitId">{{ tr('Our own money; no bank rules apply') }}</small>
              <small v-if="s.unavailable" class="trs-why trs-why--no"><span class="fa fa-ban" aria-hidden="true"></span>{{ tr('Cannot be used') }}: {{ tr(s.unavailable) }}</small>
              <small v-else-if="s.fit" class="trs-why"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Can be used') }}: {{ tr(s.fit) }}</small>
            </span>
            <span class="num">{{ s.available === null ? '—' : formatAmount(s.available) }}</span>
            <span v-if="s.unavailable && !amt(s.sourceKey)" class="trs-off-cells"></span>
            <span v-else class="trs-badges">
              <template v-if="rulesOf(s).length">
                <span class="fx-chip" :class="chip(lineRules(s))" :title="rulesOf(s).map((r) => tr(badge(r)) + ': ' + r.label).join('\n')">{{ lineBadge(s) }}</span>
                <small v-if="worst(s)" class="trs-muted">{{ worst(s)!.label }}</small>
              </template>
              <span v-else-if="s.sublimitId" class="trs-muted">{{ tr('No rules') }}</span>
            </span>
            <span v-if="!s.unavailable || amt(s.sourceKey)" class="num">
              <input v-model.number="amounts[s.sourceKey]" type="number" min="0" step="0.001" class="fx-search trs-amt" :disabled="!editable"
                     :aria-label="tr('Amount') + ' ' + (s.sublimitName ?? tr('Own funds'))" @input="dirty = true" />
            </span>
            <button v-if="editable && !s.unavailable" type="button" class="fx-btn trs-max" :title="tr('Fill with the most this line allows')" @click="fillMax(s)">{{ tr('Max') }}</button>
            <span v-else></span>
            </template>
          </div>
        </template>
        <div v-if="!sources.some((s) => s.sublimitId && !s.unavailable)" class="fx-empty">{{ tr('No bank sublimit can fund this disbursement now. Use own funds, or see the reasons above.') }}</div>
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
              <span class="fx-chip fx-chip--plain">{{ tr(partyRole(p.role)) }}</span>
              <button type="button" class="trs-party-link" :title="tr('Show invoices, bank account and amount')" @click="openParty(p)">
                {{ p.name }}<span class="fa fa-external-link" aria-hidden="true"></span></button>
              <span class="trs-muted num">{{ (p.weight * 100).toFixed(0) }}%</span>
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

        <div v-if="data.payments.length && !dirty">
          <span class="trs-k">{{ executed ? tr('Payment') : tr('Payment on Execute') }}</span>
          <div v-for="p in data.payments" :key="p.sourceKey" class="trs-rule">
            <div class="trs-rule-top"><span class="trs-rule-subject">{{ p.bankName ?? tr('Own funds') }}</span>
              <span class="num">{{ formatAmount(p.amount) }}</span></div>
            <small v-if="p.accountRef" class="trs-muted">{{ p.sourceTypeCode === 'BANK' ? tr('Drawn into and paid from') : tr('Paid from') }}
              {{ p.accountBankName }} · {{ p.accountRef }}<template v-if="p.glAccountCode"> · GL {{ p.glAccountCode }}</template></small>
            <small v-if="p.problem" class="is-bad">{{ tr(p.problem) }}</small>
          </div>
          <div v-if="executed && glTranSet" class="trs-note trs-note--ok">
            <span class="fa fa-check-circle" aria-hidden="true"></span>
            <span>{{ tr('Paid') }} {{ formatDate(data.executionDate) }} · {{ tr('GL posted under') }} <b>{{ glTranSet }}</b></span>
          </div>
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
          <button v-if="canVerify && data.canApproveOverride && !dirty && !viewOnly" type="button" class="fx-btn" :disabled="busy" @click="approve">
            <span class="fa fa-check" aria-hidden="true"></span>{{ tr('Approve override') }}</button>
          <button v-if="editable" type="button" class="fx-btn fx-btn--teal" :disabled="busy || !canExecute" :title="executeHint" @click="execute">
            <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-bolt'" aria-hidden="true"></span>{{ tr('Execute') }}</button>
        </div>
        <small v-if="editable && !canExecute" class="trs-muted">{{ tr(executeHint) }}</small>
        <small v-if="data.approveBlockedReason" class="trs-muted">{{ tr(data.approveBlockedReason) }}</small>
      </aside>
    </div>
    <!-- reverse factoring (Ahmed, 7 Nov 2026): Execute sends one transfer per supplier; each is ticked Confirmed when the bank confirms it,
         which posts DR reverse factoring receivable / CR our bank. The step completes once every supplier is confirmed. -->
    <section v-if="data?.header.isReverse && data.transfers?.length" class="fx-card trs-tr">
      <header class="trs-tr-head">
        <div><h2>{{ tr('Supplier transfers') }}</h2>
          <small class="trs-muted">{{ executed ? tr('Tick Confirmed when the bank confirms a transfer. Each tick posts its GL entry. The step completes when every supplier is confirmed.')
            : tr('What Execute will send to each supplier. Execute posts only the bank drawdown.') }}</small></div>
        <span v-if="executed" class="fx-chip" :class="openTransfers.length ? 'fx-chip--warn' : 'fx-chip--teal'">
          {{ openTransfers.length ? `${data.transfers.length - openTransfers.length} / ${data.transfers.length} ${tr('confirmed')}` : tr('All confirmed') }}</span>
      </header>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr>
            <th v-if="executed" class="trs-tr-check">
              <input v-if="canConfirm && openTransfers.length" type="checkbox" :checked="allTicked" :aria-label="tr('Confirm all')" :title="tr('Confirm all')"
                     @change="tickAll(($event.target as HTMLInputElement).checked)" /></th>
            <th>{{ tr('Supplier') }}</th><th>{{ tr('Bank / IBAN') }}</th><th>{{ tr('Paid from') }}</th>
            <th class="num">{{ tr('Amount') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('GL Tran Set ID') }}</th>
          </tr></thead>
          <tbody>
            <tr v-for="t in data.transfers" :key="t.transferId || t.supplierName">
              <td v-if="executed" class="trs-tr-check">
                <input type="checkbox" :checked="t.statusCode === 'CONFIRMED' || ticked.has(t.transferId)"
                       :disabled="t.statusCode === 'CONFIRMED' || !canConfirm" :aria-label="`${tr('Confirmed')} ${t.supplierName}`"
                       @change="tick(t.transferId, ($event.target as HTMLInputElement).checked)" /></td>
              <td><b>{{ t.supplierName }}</b><small v-if="t.taxNumber" class="trs-muted"> · {{ tr('Tax No.') }} {{ t.taxNumber }}</small></td>
              <td><template v-if="t.supplierIban || t.supplierAccountNo">{{ t.supplierBankName }}<br /><small class="trs-muted">{{ t.supplierIban ?? t.supplierAccountNo }}</small></template>
                <small v-else-if="t.problem" class="is-bad">{{ tr(t.problem) }}</small><span v-else>—</span></td>
              <td>{{ t.paidFromBankName ?? '—' }}<br v-if="t.paidFromAccountRef" /><small class="trs-muted">{{ t.paidFromAccountRef }}</small></td>
              <td class="num"><b>{{ formatAmount(t.amount) }}</b></td>
              <td><span class="fx-chip" :class="t.statusCode === 'CONFIRMED' ? 'fx-chip--teal' : t.statusCode === 'SENT' ? 'fx-chip--warn' : 'fx-chip--plain'">{{ tr(statusLabel(t.statusCode)) }}</span>
                <br v-if="t.confirmedBy || t.sentBy" /><small class="trs-muted">{{ t.statusCode === 'CONFIRMED' ? `${t.confirmedBy} · ${formatDate(t.confirmedAt)}${t.bankReference ? ' · ' + t.bankReference : ''}`
                  : t.sentBy ? `${t.sentBy} · ${formatDate(t.sentAt)}` : '' }}</small></td>
              <td><b v-if="t.glTranSetId" class="fc-ref">{{ t.glTranSetId }}</b><span v-else>—</span></td>
            </tr>
          </tbody>
          <tfoot><tr><th :colspan="executed ? 4 : 3">{{ tr('Total') }}</th><th class="num">{{ formatAmount(data.transfers.reduce((a, t) => a + t.amount, 0)) }}</th><th colspan="2"></th></tr></tfoot>
        </table>
      </div>
      <div v-if="canConfirm && ticked.size" class="trs-tr-confirm">
        <label class="fc-field"><span>{{ tr('Bank confirmation reference (optional)') }}</span>
          <input v-model="bankReference" class="fx-search" maxlength="100" /></label>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="confirmTicked">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check-square-o'" aria-hidden="true"></span>{{ tr('Confirm') }} {{ ticked.size }} {{ tr(ticked.size === 1 ? 'transfer' : 'transfers') }}</button>
      </div>
    </section>

    <div v-else-if="!loading && !data" class="fx-empty">{{ tr('Open this page from a disbursement task.') }}</div>
    <TrsPartyDialog v-if="data" v-model:visible="partyOpen" :disbursement-id="data.header.disbursementId" :party="party"
                    :reverse="data.header.isReverse" />
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
import FctDisbursementSummary from '@/modules/fct/components/FctDisbursementSummary.vue'
import FctViewOnlyNote from '@/modules/fct/components/FctViewOnlyNote.vue'
import { useViewOnly } from '@/modules/fct/components/viewOnly'
import TrsPartyDialog from '../components/TrsPartyDialog.vue'
import { trsDisburseApi, type TrsDisburse, type TrsDisburseParty, type TrsDisburseRule, type TrsDisburseSource } from '../api/trsDisburse.api'
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
const completeNote = computed(() => data.value?.header.isReverse
  ? 'Your choices are saved, then this step is completed. The source of funds must be executed and every supplier transfer confirmed first.'
  : 'Your choices are saved, then this step is completed. The source of funds must be executed first.')

const sources = computed(() => data.value?.sources ?? [])
const net = computed(() => data.value?.header.netAmount ?? 0)
const amt = (key: string) => Number(amounts[key]) || 0
const total = computed(() => sources.value.reduce((t, s) => t + amt(s.sourceKey), 0))
const remaining = computed(() => net.value - total.value)
const executed = computed(() => data.value?.header.fundingStatus === 'EXECUTED')
const viewOnly = useViewOnly()
const editable = computed(() => canEdit && !!data.value?.canEdit && !viewOnly.value)

type TreeRow =
  | { kind: 'BANK' | 'FACILITY' | 'TRANCHE'; key: string; label: string; count: number; usable: number; chosen: number }
  | { kind: 'LINE'; key: string; source: TrsDisburseSource; deep?: boolean }
/** Own funds, then each bank › facility › tranche with its sublimits (usable ones first), as flat rows for the grid. */
const tree = computed<TreeRow[]>(() => {
  const rows: TreeRow[] = []
  const node = (kind: 'BANK' | 'FACILITY' | 'TRANCHE', key: string, label: string, xs: TrsDisburseSource[]) =>
    rows.push({ kind, key, label, count: xs.length, usable: xs.filter((x) => !x.unavailable).length, chosen: xs.reduce((t, x) => t + amt(x.sourceKey), 0) })
  const by = <T,>(xs: TrsDisburseSource[], f: (s: TrsDisburseSource) => T) => [...new Set(xs.map(f))].map((k) => [k, xs.filter((x) => f(x) === k)] as const)
  for (const s of sources.value.filter((x) => !x.sublimitId)) rows.push({ kind: 'LINE', key: s.sourceKey, source: s })
  for (const [bankId, bs] of by(sources.value.filter((x) => x.sublimitId), (x) => x.bankId)) {
    node('BANK', `B${bankId}`, bs[0]?.bankName ?? '', bs)
    for (const [fid, fs] of by(bs, (x) => x.facilityId)) {
      const tranches = by(fs, (x) => x.trancheName)
      // one tranche: shown on the facility row, to keep the tree short
      node('FACILITY', `F${fid}`, (fs[0]?.facilityRef ?? '') + (tranches.length === 1 ? ` › ${tranches[0]?.[0] ?? ''}` : ''), fs)
      for (const [tn, ts] of tranches) {
        if (tranches.length > 1) node('TRANCHE', `F${fid}T${tn}`, tn ?? '', ts)
        for (const s of [...ts].sort((a, b) => Number(!!a.unavailable) - Number(!!b.unavailable))) rows.push({ kind: 'LINE', key: s.sourceKey, source: s, deep: tranches.length > 1 })
      }
    }
  }
  return rows
})

/** Reverse factoring (agreed with Ahmed, 7 Nov 2026): our client is the buyer, who owes the invoices; the suppliers sold them to us. */
const partyOpen = ref(false)
const party = ref<TrsDisburseParty | null>(null)
function openParty(p: TrsDisburseParty) {
  party.value = p
  partyOpen.value = true
}
const partyRole = (role: string) => (data.value?.header.isReverse ? (role === 'DEBTOR' ? 'Buyer (debtor)' : 'Supplier') : role === 'DEBTOR' ? 'Debtor' : 'Seller')
const rulesOf = (s: TrsDisburseSource) => (data.value?.rules ?? []).filter((r) => s.ruleIds.includes(r.ruleId))
const covered = (r: TrsDisburseRule) => sources.value.filter((s) => s.ruleIds.includes(r.ruleId)).reduce((t, s) => t + amt(s.sourceKey), 0)

/** OK, WARN (above the warning level, or a WARN rule breached), OVERRIDE (needs a second user), BLOCK. */
function level(r: TrsDisburseRule): 'OK' | 'WARN' | 'OVERRIDE' | 'BLOCK' {
  const sum = covered(r)
  if (r.limitAbove !== null && r.limitAbove <= 0 && r.breachActionCode === 'BLOCK') return 'BLOCK'
  if (r.limitAbove !== null && sum > r.limitAbove + 0.001) return r.breachActionCode === 'WARN' ? 'WARN' : r.breachActionCode
  if (r.warnAbove !== null && sum > r.warnAbove + 0.001) return 'WARN'
  return 'OK'
}
const rank = { OK: 0, WARN: 1, OVERRIDE: 2, BLOCK: 3 } as const
function lineLevel(s: TrsDisburseSource) {
  if (!amt(s.sourceKey)) return 'OK'
  if (s.unavailable) return 'BLOCK'
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
  const problem = dirty.value ? null : data.value?.payments.find((p) => p.problem)?.problem
  if (problem) return problem
  return 'Records the drawdowns on the chosen sublimits.'
})
const canExecute = computed(() => executeHint.value === 'Records the drawdowns on the chosen sublimits.')

const glTranSet = computed(() => data.value?.payments.find((p) => p.glTranSetId)?.glTranSetId ?? null)

const statusText = computed(() => (executed.value ? 'Executed' : data.value?.header.fundingStatus === 'RESERVED' ? 'Reserved' : 'Not chosen'))
const statusClass = computed(() => (executed.value ? 'fx-chip--ok' : data.value?.header.fundingStatus === 'RESERVED' ? 'fx-chip--warn' : 'fx-chip--plain'))

/** The most a line may take: its availability, the room every blocking or override rule leaves, and what is still to cover. */
function maxFor(s: TrsDisburseSource) {
  if (s.unavailable) return 0
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
/**
 * Shares the net amount (module setting TRS.SUGGEST_ORDER): bank lines first (most available first, each within every rule) and own
 * funds with the rest, or own funds for all of it.
 */
function suggest() {
  clearAll()
  const own = sources.value.find((s) => !s.sublimitId)
  const fillOwn = () => {
    if (own && remaining.value > 0.001) amounts[own.sourceKey] = Math.round(remaining.value * 1000) / 1000
  }
  if (data.value?.suggestOrder === 'OWN_FIRST') fillOwn()
  const banks = sources.value.filter((s) => s.sublimitId && !s.unavailable).sort((a, b) => (b.available ?? 0) - (a.available ?? 0))
  for (const s of banks) if (remaining.value > 0.001) amounts[s.sourceKey] = maxFor(s)
  fillOwn()
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
    await save(async () => apply(await trsDisburseApi.execute(id.value)), data.value?.header.isReverse
      ? 'Executed: the bank drawdown is posted and the supplier transfers are sent. Tick Confirmed for each one when the bank confirms it.'
      : 'Executed: the drawdowns are recorded, the client is paid and the GL is posted.')
  } finally {
    busy.value = false
  }
}

// reverse factoring: the supplier transfers ticked as confirmed, posted together with Confirm
const ticked = ref(new Set<number>())
const bankReference = ref('')
const openTransfers = computed(() => (data.value?.transfers ?? []).filter((t) => t.statusCode === 'SENT'))
const canConfirm = computed(() => canEdit && !viewOnly.value && executed.value)
const allTicked = computed(() => openTransfers.value.length > 0 && openTransfers.value.every((t) => ticked.value.has(t.transferId)))
const statusLabel = (s: string) => (s === 'CONFIRMED' ? 'Confirmed' : s === 'SENT' ? 'Sent, waiting for the bank' : 'Planned')
function tick(transferId: number, on: boolean) {
  const next = new Set(ticked.value)
  if (on) next.add(transferId)
  else next.delete(transferId)
  ticked.value = next
}
const tickAll = (on: boolean) => { ticked.value = new Set(on ? openTransfers.value.map((t) => t.transferId) : []) }

async function confirmTicked() {
  busy.value = true
  try {
    const ok = await save(async () => apply(await trsDisburseApi.confirmTransfers(id.value, [...ticked.value], bankReference.value.trim() || null)),
      'Transfers confirmed and the GL is posted.')
    if (ok) { ticked.value = new Set(); bankReference.value = '' }
  } finally {
    busy.value = false
  }
}

/** The step's Complete: once executed nothing is left to save; before that, the step cannot be completed. */
async function saveForStep() {
  if (executed.value && openTransfers.value.length) {
    showError(`${tr('Tick Confirmed for every supplier transfer before completing this step. Not confirmed yet:')} ${openTransfers.value.map((t) => t.supplierName).join(', ')}`)
    return false
  }
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

.trs-tr { display: flex; flex-direction: column; gap: 12px; padding: 16px; }
.trs-tr-head { display: flex; align-items: flex-start; justify-content: space-between; gap: 12px; flex-wrap: wrap; }
.trs-tr-head h2 { margin: 0 0 4px; font-size: var(--fs-md, 14px); color: var(--fin-teal-700); }
.trs-tr-check { width: 34px; text-align: center; }
.trs-tr-check input { width: 16px; height: 16px; accent-color: var(--fin-teal-600); cursor: pointer; }
.trs-tr-confirm { display: flex; align-items: flex-end; gap: 12px; flex-wrap: wrap; }
.trs-tr-confirm .fc-field { min-width: 260px; }
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
.trs-why { display: flex; gap: 6px; align-items: baseline; color: var(--fin-teal-600) !important; }
.trs-why--no { color: var(--fin-ink) !important; }
.trs-why--no .fa { color: #B42318; }
.trs-line.is-off { background: #F7F8F8; }
.trs-node { display: flex; align-items: center; gap: 8px; padding: 8px 16px; border-block-end: 1px solid var(--fin-line-soft); }
.trs-node .fa { color: var(--fin-teal-600); width: 16px; }
.trs-node-name { font-weight: 700; }
.trs-node-sum { margin-inline-start: auto; font-weight: 700; }
.trs-node--BANK { background: #FAFCFC; color: var(--fin-teal-600); }
.trs-node--FACILITY { padding-inline-start: 36px; }
.trs-node--TRANCHE { padding-inline-start: 56px; }
.trs-node--TRANCHE .trs-node-name { font-weight: 600; }
.trs-leaf { padding-inline-start: 56px; }
.trs-leaf--deep { padding-inline-start: 76px; }
.trs-line.is-off .trs-name b, .trs-line.is-off .num { color: var(--fin-muted); }
.trs-off-cells { grid-column: span 2; }

.trs-side { flex: 1 1 320px; max-width: 400px; align-self: flex-start; display: flex; flex-direction: column; gap: 16px; padding: 16px 18px; }
.trs-side h2 { margin: 4px 0 8px; font-size: var(--fs-lg); color: var(--fin-ink); }
.trs-facts { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 10px 16px; margin: 0; }
.trs-facts dt { font-size: var(--fs-xs); font-weight: 700; color: var(--fin-muted); }
.trs-facts dd { margin: 3px 0 0; }
.trs-parties { list-style: none; margin: 6px 0 0; padding: 0; display: flex; flex-direction: column; gap: 6px; }
.trs-parties li { display: grid; grid-template-columns: auto minmax(0, 1fr) auto; gap: 8px; align-items: center; }
.trs-party-link { justify-self: start; display: inline-flex; align-items: center; gap: 6px; padding: 0; border: 0; background: none; font: inherit;
  color: var(--fin-teal-700); font-weight: 600; text-align: start; cursor: pointer; text-decoration: underline; text-underline-offset: 3px; }
.trs-party-link .fa { font-size: 11px; color: var(--fin-orange); }
.trs-party-link:hover { color: var(--fin-teal-600); }
.trs-rule { display: flex; flex-direction: column; gap: 3px; padding: 10px 0; border-block-start: 1px solid var(--fin-line-soft); }
.trs-rule-top { display: flex; align-items: center; justify-content: space-between; gap: 8px; }
.trs-rule-subject { font-weight: 600; }
.trs-note { display: flex; gap: 8px; padding: 10px 12px; border-radius: 8px; background: #FDF0E5; color: var(--fin-ink); font-size: var(--fs-sm); }
.trs-note .fa { color: var(--fin-orange); margin-top: 3px; }
.trs-note--ok { background: var(--fin-teal-50); }
.trs-note--ok .fa { color: var(--fin-teal-600); }
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
