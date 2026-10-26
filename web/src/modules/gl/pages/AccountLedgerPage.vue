<script setup lang="ts">
// Account ledger (APEX App 120 page 52): every posted movement of an account, in date order, with the balance
// before and after each one. Search by account or segment values, period, journal or TRAN_SET_ID.
import { reactive, ref, shallowRef } from 'vue'
import { RouterLink } from 'vue-router'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import GlPageHeader from '../components/GlPageHeader.vue'
import { drCr, journalsApi, type AccountLedgerResult } from '../api/glJournals.api'

const result = shallowRef<AccountLedgerResult | null>(null)
const loadError = ref('')
const searching = ref(false)
useFeedback({ errors: [loadError] })

const filters = reactive({ ledgerId: null as number | null, from: '', to: '', q: '', segments: {} as Record<number, string> })

async function search() {
  searching.value = true
  try {
    const r = await journalsApi.accountLedger(filters)
    result.value = r
    filters.ledgerId = r.ledgerId
    filters.from = r.fromPeriod ?? ''
    filters.to = r.toPeriod ?? ''
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'The account ledger could not be loaded.'
  } finally {
    searching.value = false
  }
}

function onLedgerChange() {
  filters.from = ''
  filters.to = ''
  filters.segments = {}
  search()
}

search()
</script>

<template>
  <div class="cp-page">
    <GlPageHeader
      title="Account ledger"
      description="Every posted movement of an account, in date order, with the balance before and after each one. Search by account or segment values, period, journal or TRAN_SET_ID."
      icon="pi pi-chart-line"
    >
      <RouterLink to="/gl/journals" class="cp-btn"><i class="pi pi-list"></i><span>Journals</span></RouterLink>
      <RouterLink to="/gl/account-balances" class="cp-btn"><i class="pi pi-table"></i><span>Account balances</span></RouterLink>
    </GlPageHeader>

    <form class="cp-card al-filters" @submit.prevent="search">
      <label class="al-f"><span>Ledger</span>
        <select v-model="filters.ledgerId" @change="onLedgerChange"><option v-for="l in result?.ledgers" :key="l.ledgerId" :value="l.ledgerId">{{ l.ledgerCode }}</option></select>
      </label>
      <label class="al-f"><span>From period</span>
        <select v-model="filters.from"><option v-for="p in result?.periods" :key="p" :value="p">{{ p }}</option></select>
      </label>
      <label class="al-f"><span>To period</span>
        <select v-model="filters.to"><option v-for="p in result?.periods" :key="p" :value="p">{{ p }}</option></select>
      </label>
      <label v-for="s in result?.segments" :key="s.segmentNum" class="al-f">
        <span>{{ s.segmentNameEn }}{{ s.isAccount ? ' (starts with)' : '' }}</span>
        <input v-model="filters.segments[s.segmentNum]" class="al-code" :list="`al-dl-${s.segmentNum}`" autocomplete="off" />
        <datalist :id="`al-dl-${s.segmentNum}`"><option v-for="v in s.values" :key="v.code" :value="v.code">{{ v.name }}</option></datalist>
      </label>
      <label class="al-f al-f--q"><span>Journal, name or TRAN_SET_ID</span><input v-model="filters.q" /></label>
      <button type="submit" class="cp-btn cp-btn--primary" :disabled="searching"><i :class="searching ? 'pi pi-spin pi-spinner' : 'pi pi-search'"></i><span>Search</span></button>
    </form>

    <div v-if="!result" class="cp-card cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div>
    <div v-else-if="!result.ledgers.length" class="cp-card cp-empty">No active ledger yet.</div>
    <div v-else-if="!result.searched" class="cp-card cp-empty al-start">
      <i class="pi pi-filter"></i>
      <span>Enter an account (or the start of one), another segment value or a journal number, then search.</span>
    </div>
    <template v-else>
      <section class="cp-card cp-figures al-sum">
        <div class="cp-figure"><div class="cp-figure__label">Brought forward</div><div class="cp-figure__value">{{ drCr(result.broughtForward) }}</div><div class="cp-figure__sub">Before {{ result.fromPeriod }}</div></div>
        <div class="cp-figure"><div class="cp-figure__label">Debits</div><div class="cp-figure__value">{{ formatAmount(result.totalDr) }}</div></div>
        <div class="cp-figure"><div class="cp-figure__label">Credits</div><div class="cp-figure__value">{{ formatAmount(result.totalCr) }}</div></div>
        <div class="cp-figure"><div class="cp-figure__label">Closing balance</div><div class="cp-figure__value">{{ drCr(result.closing) }}</div><div class="cp-figure__sub">End of {{ result.toPeriod }}</div></div>
      </section>

      <section class="cp-card">
        <div class="cp-card__head"><i class="pi pi-list"></i><h3>Movements</h3><span class="cp-spacer"></span><span class="cp-muted al-n">{{ result.rows.length }}</span></div>
        <div v-if="!result.rows.length" class="cp-empty">No posted movement in these periods.</div>
        <div v-else class="cp-table-wrap">
          <table class="cp-table">
            <thead><tr><th>Date</th><th>Journal</th><th>TRAN_SET_ID</th><th>Account</th><th>Description</th><th class="cp-num">Debit</th><th class="cp-num">Credit</th><th class="cp-num">Before</th><th class="cp-num">After</th></tr></thead>
            <tbody>
              <tr v-for="(r, i) in result.rows" :key="i">
                <td>{{ formatDate(r.accountingDate) }}</td>
                <td><RouterLink :to="`/gl/journals/${r.journalHeaderId}`" class="al-no">{{ r.journalNo }}</RouterLink><small class="al-sub">{{ r.journalName }}</small></td>
                <td>{{ r.tranSetId ?? '' }}</td>
                <td><code class="al-combo">{{ r.concatenatedAccount }}</code></td>
                <td class="al-wrap">{{ r.description ?? '' }}</td>
                <td class="cp-num">{{ r.dr ? formatAmount(r.dr) : '' }}</td>
                <td class="cp-num">{{ r.cr ? formatAmount(r.cr) : '' }}</td>
                <td class="cp-num cp-muted">{{ drCr(r.balanceBefore) }}</td>
                <td class="cp-num"><b>{{ drCr(r.balanceAfter) }}</b></td>
              </tr>
            </tbody>
          </table>
        </div>
        <p v-if="result.more" class="al-more">Showing the first 2,000 movements; the totals cover all of them.</p>
      </section>
    </template>
    <p class="apex-ref">APEX App 120 page 52</p>
  </div>
</template>

<style scoped>
.al-filters { display: flex; flex-wrap: wrap; align-items: flex-end; gap: 12px; margin-bottom: 14px; padding: 14px 18px; }
.al-f { display: flex; flex-direction: column; gap: 5px; width: 140px; }
.al-f--q { flex: 1 1 200px; width: auto; }
.al-f span { color: var(--cap-ink-soft); font-size: 11px; font-weight: 500; }
.al-f input, .al-f select { height: 38px; padding: 0 10px; border: 1px solid var(--cap-border); border-radius: 7px; background: #fff; color: var(--cap-ink); font: inherit; font-size: 13px; }
.al-code { text-transform: uppercase; }
.al-start { display: flex; align-items: center; justify-content: center; gap: 10px; }
.al-sum { margin-bottom: 14px; }
.al-n { font-size: 12px; }
.al-no { color: var(--cap-primary); font: 600 12px ui-monospace, Menlo, Consolas, monospace; text-decoration: none; }
.al-sub { display: block; margin-top: 2px; color: var(--cap-muted); font-size: 11px; }
.al-combo { font: 600 12px ui-monospace, Menlo, Consolas, monospace; color: var(--cap-primary-dark); }
.al-wrap { white-space: normal; min-width: 140px; }
.al-more { margin: 0; padding: 10px 16px; border-top: 1px solid var(--cap-border-soft); color: var(--cap-muted); font-size: 12px; }
.apex-ref { margin: 12px 0 0; color: var(--cap-faint); font-size: 11px; text-align: end; }
@media (max-width: 760px) { .al-f { width: calc(50% - 6px); } .al-f--q { width: 100%; } }
</style>
