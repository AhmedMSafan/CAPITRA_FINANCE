<script setup lang="ts">
// Account balances (APEX App 120 page 20, GL_ACCOUNT_BALANCE_SUMMARY_V): each account combination's opening,
// period debits and credits and closing balance in one ledger period.
import { computed, reactive, ref, shallowRef } from 'vue'
import { RouterLink } from 'vue-router'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { formatDateTime } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import GlPageHeader from '../components/GlPageHeader.vue'
import { codeLabel } from '../api/glSetup.api'
import { drCr, journalsApi, type AccountBalancesResult } from '../api/glJournals.api'

const result = shallowRef<AccountBalancesResult | null>(null)
const loadError = ref('')
const searching = ref(false)
useFeedback({ errors: [loadError] })

const filters = reactive({ ledgerId: null as number | null, period: '', q: '' })

async function search() {
  searching.value = true
  try {
    const r = await journalsApi.balances(filters)
    result.value = r
    filters.ledgerId = r.ledgerId
    filters.period = r.period ?? ''
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'The balances could not be loaded.'
  } finally {
    searching.value = false
  }
}

function onLedgerChange() {
  filters.period = ''
  search()
}

const balanced = computed(() => Math.abs(result.value?.totalClosing ?? 0) < 0.005)

search()
</script>

<template>
  <div class="cp-page">
    <GlPageHeader
      title="Account balances"
      description="Opening balance, the period's debits and credits, and the closing balance of every account combination in one period."
      icon="pi pi-table"
    >
      <RouterLink to="/gl/account-ledger" class="cp-btn"><i class="pi pi-chart-line"></i><span>Account ledger</span></RouterLink>
    </GlPageHeader>

    <form class="cp-card ab-filters" @submit.prevent="search">
      <label class="ab-f"><span>Ledger</span>
        <select v-model="filters.ledgerId" @change="onLedgerChange"><option v-for="l in result?.ledgers" :key="l.ledgerId" :value="l.ledgerId">{{ l.ledgerCode }}</option></select>
      </label>
      <label class="ab-f"><span>Period</span>
        <select v-model="filters.period" @change="search"><option v-for="p in result?.periods" :key="p" :value="p">{{ p }}</option></select>
      </label>
      <label class="ab-f ab-f--q"><span>Account or name</span><input v-model="filters.q" /></label>
      <button type="submit" class="cp-btn cp-btn--primary" :disabled="searching"><i :class="searching ? 'pi pi-spin pi-spinner' : 'pi pi-search'"></i><span>Search</span></button>
    </form>

    <div v-if="!result" class="cp-card cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div>
    <section v-else class="cp-card">
      <div class="cp-card__head">
        <i class="pi pi-table"></i><h3>{{ result.period ?? 'No period' }}</h3><span class="cp-spacer"></span>
        <span v-if="result.rows.length && !filters.q" class="cp-pill" :class="balanced ? 'cp-pill--ok' : 'cp-pill--bad'">{{ balanced ? 'Debits equal credits' : 'Out of balance' }}</span>
      </div>
      <div v-if="!result.rows.length" class="cp-empty">No balance in this period yet. Balances appear when journals are posted.</div>
      <div v-else class="cp-table-wrap">
        <table class="cp-table">
          <thead>
            <tr><th>Account</th><th>Name</th><th>Type</th><th class="cp-num">Opening</th><th class="cp-num">Debits</th><th class="cp-num">Credits</th><th class="cp-num">Closing</th><th>Last posted</th></tr>
          </thead>
          <tbody>
            <tr v-for="r in result.rows" :key="`${r.accountCombinationId}-${r.currencyCode}`">
              <td><code class="ab-combo">{{ r.concatenatedAccount }}</code></td>
              <td>{{ r.accountNameEn }}</td>
              <td>{{ codeLabel(r.accountType) }}</td>
              <td class="cp-num">{{ drCr(r.opening) }}</td>
              <td class="cp-num">{{ r.periodDr ? formatAmount(r.periodDr) : '' }}</td>
              <td class="cp-num">{{ r.periodCr ? formatAmount(r.periodCr) : '' }}</td>
              <td class="cp-num"><b>{{ drCr(r.closing) }}</b></td>
              <td><span v-if="r.lastJournalNo">{{ r.lastJournalNo }}<small class="ab-sub">{{ formatDateTime(r.lastPostedAt) }}</small></span></td>
            </tr>
          </tbody>
          <tfoot>
            <tr><td colspan="3"><b>Total</b></td><td class="cp-num">{{ drCr(result.totalOpening) }}</td><td class="cp-num">{{ formatAmount(result.totalDr) }}</td><td class="cp-num">{{ formatAmount(result.totalCr) }}</td><td class="cp-num"><b>{{ drCr(result.totalClosing) }}</b></td><td></td></tr>
          </tfoot>
        </table>
      </div>
    </section>
    <p class="apex-ref">APEX App 120 page 20</p>
  </div>
</template>

<style scoped>
.ab-filters { display: flex; flex-wrap: wrap; align-items: flex-end; gap: 12px; margin-bottom: 14px; padding: 14px 18px; }
.ab-f { display: flex; flex-direction: column; gap: 5px; width: 160px; }
.ab-f--q { flex: 1 1 220px; width: auto; }
.ab-f span { color: var(--cap-ink-soft); font-size: 11px; font-weight: 500; }
.ab-f input, .ab-f select { height: 38px; padding: 0 10px; border: 1px solid var(--cap-border); border-radius: 7px; background: #fff; color: var(--cap-ink); font: inherit; font-size: 13px; }
.ab-combo { font: 600 12px ui-monospace, Menlo, Consolas, monospace; color: var(--cap-primary-dark); }
.ab-sub { display: block; color: var(--cap-muted); font-size: 11px; }
tfoot td { padding: 10px 14px; border-top: 2px solid var(--cap-border); background: var(--cap-bg); color: var(--cap-ink); font-size: 12px; white-space: nowrap; }
.apex-ref { margin: 12px 0 0; color: var(--cap-faint); font-size: 11px; text-align: end; }
@media (max-width: 760px) { .ab-f { width: calc(50% - 6px); } .ab-f--q { width: 100%; } }
</style>
