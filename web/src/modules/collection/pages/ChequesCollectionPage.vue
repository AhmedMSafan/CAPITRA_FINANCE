<template>
  <!-- APEX App 136 page 712 "Cheques Collection": the due factoring cheques (FNV_DUE_CHEQUES_V) for a deposit account and a
       due-date range; each needs a Bank Result before Apply Bank Results runs apply_cheque_status. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true">
          <svg viewBox="0 0 24 24" width="22" height="22" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
            <rect x="2" y="5" width="20" height="14" rx="2" /><line x1="6" y1="9" x2="16" y2="9" /><line x1="6" y1="13" x2="14" y2="13" /><path d="M16 14l2 2 4-4" />
          </svg>
        </span>
        <div>
          <h1>{{ tr('Cheques Collection') }}</h1>
          <p>{{ tr('Reconcile due factoring cheques against bank statements and record collected, bounced, or rejected results.') }}</p>
        </div>
      </div>
    </div>

    <div class="col-stack">
      <section class="fx-card fx-card-pad fc-filters" :aria-label="tr('Bank Statement Filters')">
        <label class="fc-field">
          <span>{{ tr('Deposited Bank Account') }}</span>
          <select v-model="filter.depositBankAccountId" class="fx-select">
            <option value="">{{ tr('- Select -') }}</option>
            <option v-for="a in accounts" :key="a.value" :value="a.value">{{ a.label }}</option>
          </select>
        </label>
        <label class="fc-field">
          <span>{{ tr('Due From') }}</span>
          <input v-model="filter.dueFrom" class="fx-search" type="date">
        </label>
        <label class="fc-field">
          <span>{{ tr('To') }}</span>
          <input v-model="filter.dueTo" class="fx-search" type="date">
        </label>
        <div class="fc-filter-actions">
          <button type="button" class="fx-btn fx-btn--teal" :disabled="loading" @click="load"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Refresh') }}</button>
        </div>
      </section>

      <section class="fx-card">
        <div class="fc-card-head">
          <h2>{{ tr('Due Cheques') }} ({{ rows.length }})</h2>
          <button v-if="canEdit" type="button" class="fx-btn fx-btn--primary" :disabled="applying || !rows.length" @click="apply">
            <span class="fa" :class="applying ? 'fa-spinner fa-spin' : 'fa-check-square-o'" aria-hidden="true"></span>{{ tr('Apply Bank Results') }}
          </button>
        </div>
        <div class="fx-table-wrap">
          <table class="fx-table col-due">
            <thead>
              <tr>
                <th>{{ tr('Cheque Reference') }}</th>
                <th>{{ tr('Cheque Number') }}</th>
                <th>{{ tr('Due Date') }}</th>
                <th>{{ tr('Issuer Bank') }}</th>
                <th>{{ tr('Drawer') }}</th>
                <th>{{ tr('Customer Id') }}</th>
                <th>{{ tr('Client Name') }}</th>
                <th>{{ tr('Bank Name En') }}</th>
                <th class="fx-r">{{ tr('Cheque Amount') }}</th>
                <th>{{ tr('Currency') }}</th>
                <th>{{ tr('Current Status') }}</th>
                <th>{{ tr('Bank Result') }}</th>
                <th>{{ tr('Statement Date') }}</th>
                <th>{{ tr('Statement Reference') }}</th>
                <th>{{ tr('New Reason Code') }}</th>
                <th>{{ tr('Notes') }}</th>
                <th>{{ tr('Rejection Reason Code') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in rows" :key="row.chequeId">
                <td class="col-nowrap">{{ row.chequeRef }}</td>
                <td class="col-nowrap">{{ row.chequeNo }}</td>
                <td class="col-nowrap">{{ formatDate(row.dueDate) }}</td>
                <td>{{ row.issuerBankName }}</td>
                <td>{{ row.drawerNameEn }}</td>
                <td>{{ row.customerId }}</td>
                <td>{{ row.clientName }}</td>
                <td>{{ row.bankNameEn }}</td>
                <td class="fx-r col-nowrap">{{ formatAmount(row.chequeAmount) }}</td>
                <td>{{ row.currencyCode }}</td>
                <td><span class="fx-chip" :class="statusChip(row.statusCode)">{{ row.statusCode }}</span></td>
                <td>
                  <select v-model="row.edit.newStatusCode" class="fx-select col-cell" :class="{ 'col-missing': missing && !row.edit.newStatusCode }" :aria-label="tr('Bank Result') + ' ' + row.chequeNo" :disabled="!canEdit">
                    <option value="">{{ tr('- Select -') }}</option>
                    <option v-for="o in BANK_RESULTS" :key="o.value" :value="o.value">{{ tr(o.label) }}</option>
                  </select>
                </td>
                <td><input v-model="row.edit.statusDate" class="fx-search col-cell" type="date" :aria-label="tr('Statement Date') + ' ' + row.chequeNo" :disabled="!canEdit"></td>
                <td><input v-model="row.edit.bankStatementReference" class="fx-search col-cell" type="text" maxlength="100" :aria-label="tr('Statement Reference') + ' ' + row.chequeNo" :disabled="!canEdit"></td>
                <td><input v-model="row.edit.reasonCode" class="fx-search col-cell" type="text" maxlength="50" :aria-label="tr('New Reason Code') + ' ' + row.chequeNo" :disabled="!canEdit"></td>
                <td><textarea v-model="row.edit.notes" class="fx-search col-cell col-notes" rows="1" maxlength="2000" :aria-label="tr('Notes') + ' ' + row.chequeNo" :disabled="!canEdit" /></td>
                <td>{{ row.rejectionReasonCode ?? '' }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
        <div v-else-if="!rows.length" class="fx-empty"><span class="fa fa-inbox" aria-hidden="true"></span>{{ tr('No data found') }}</div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import './collection.css'
import { onMounted, reactive, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate, todayInCairo } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import { collectionApi, type ChequeResult, type DueCheque, type Option } from '../api/collection.api'
import { messageOf, statusChip } from './collection'

// the page's static Bank Result list
const BANK_RESULTS = [
  { value: 'COLLECTED', label: 'Collected' },
  { value: 'BOUNCED', label: 'Bounced' },
  { value: 'REJECTED', label: 'Rejected' }
]

type Edit = { newStatusCode: string; statusDate: string; bankStatementReference: string; reasonCode: string; notes: string }

const canEdit = hasPermission('COLLECTION.EDIT')
const accounts = ref<Option[]>([])
// each due cheque with the five Bank Result cells the user fills (empty in FNV_DUE_CHEQUES_V)
const rows = ref<(DueCheque & { edit: Edit })[]>([])
const loading = ref(false)
const applying = ref(false)
const missing = ref(false)

// Due From defaults to a week ago, To to today (P712_DUE_FROM = trunc(sysdate)-7, P712_DUE_TO = trunc(sysdate))
const today = todayInCairo()
const weekAgo = (() => {
  const d = new Date(`${today}T00:00:00Z`)
  d.setUTCDate(d.getUTCDate() - 7)
  return d.toISOString().slice(0, 10)
})()
const filter = reactive({ depositBankAccountId: '', dueFrom: weekAgo, dueTo: today })

async function load() {
  loading.value = true
  missing.value = false
  try {
    rows.value = (await collectionApi.dueCheques(filter))
      .map((c) => ({ ...c, edit: { newStatusCode: '', statusDate: '', bankStatementReference: '', reasonCode: '', notes: '' } }))
  } catch (err) {
    showError(messageOf(err))
  } finally {
    loading.value = false
  }
}

const blank = (v: string) => (v.trim() === '' ? null : v.trim())

// applyBankResults(): every cheque on the grid needs a result; customer and amount must be present
async function apply() {
  const results: ChequeResult[] = []
  for (const row of rows.value) {
    const e = row.edit
    if (!e.newStatusCode) {
      missing.value = true
      showError(tr('Please select a bank result for every cheque.'))
      return
    }
    if (!row.customerId?.trim()) {
      showError(`${tr('Customer ID is missing for cheque ID')} ${row.chequeNo}`)
      return
    }
    results.push({
      chequeId: row.chequeId,
      customerId: row.customerId.trim(),
      chequeAmount: row.chequeAmount,
      newStatusCode: e.newStatusCode,
      statusDate: e.statusDate || null,
      bankStatementReference: blank(e.bankStatementReference),
      reasonCode: blank(e.reasonCode),
      notes: blank(e.notes)
    })
  }
  missing.value = false
  applying.value = true
  try {
    const done = await collectionApi.applyResults(results)
    showSuccess(tr(done.message))
    await load()
  } catch (err) {
    showError(messageOf(err))
  } finally {
    applying.value = false
  }
}

onMounted(async () => {
  try {
    accounts.value = await collectionApi.depositAccounts()
  } catch (err) {
    showError(messageOf(err))
  }
  await load()
})
</script>

<style scoped>
.col-due td { vertical-align: middle; }
.col-nowrap { white-space: nowrap; }
.col-cell { min-width: 140px; height: 32px; }
.col-notes { min-width: 180px; height: auto; min-height: 32px; padding-block: 6px; resize: vertical; }
.col-missing { border-color: var(--fin-bad); }
</style>
