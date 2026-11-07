<template>
  <!-- Invoice summary of a disbursement (Ahmed, 7 Nov 2026): every invoice received, what is not eligible, the eligible bills by supplier
       (debtor in factoring) with a total for each, and the value we fund after the advance rate. A button that opens a read-only popup. -->
  <button type="button" class="fx-btn" @click="open"><span class="fa fa-list-alt" aria-hidden="true"></span>{{ tr('Invoice summary') }}</button>
  <Crm360Dialog v-model:visible="visible" :title="tr('Invoice summary')" :subtitle="s ? `${s.disbursementNumber} · ${s.clientName}` : ''"
                icon="fa-list-alt" :width="980">
    <p v-if="loading" class="sm-muted"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span> {{ tr('Loading…') }}</p>
    <template v-else-if="s">
      <!-- the money in four steps: received, not eligible, eligible, what we fund -->
      <div class="sm-flow">
        <div class="sm-box"><span>{{ tr('Invoices received') }}</span><b>{{ formatAmount(s.receivedGross) }}</b><small>{{ count(s.receivedCount, 'invoice') }}</small></div>
        <div class="sm-box sm-box--bad"><span>{{ tr('Not eligible') }}</span><b>− {{ formatAmount(s.notEligibleGross) }}</b><small>{{ count(s.notEligibleCount, 'invoice') }}</small></div>
        <div class="sm-box"><span>{{ tr('Eligible') }}</span><b>{{ formatAmount(s.eligibleGross) }}</b><small>{{ count(s.eligibleCount, 'invoice') }}</small></div>
        <div class="sm-box sm-box--main"><span>{{ tr('Value we finance') }}</span><b>{{ formatAmount(s.eligibleFinanced) }}</b>
          <small>{{ rateText }}</small></div>
      </div>
      <p class="sm-facts">
        <span>{{ tr('Business line') }}: <b>{{ tr(s.isReverse ? 'Reverse Factoring' : 'Factoring') }}</b></span>
        <span>{{ tr('Currency') }}: <b>{{ s.currencyCode }}</b></span>
        <span v-if="s.netAmount !== null">{{ tr('Disbursement net amount') }}: <b>{{ formatAmount(s.netAmount) }}</b>
          <span v-if="Math.abs(s.netAmount - s.eligibleFinanced) > 0.01" class="sm-diff">({{ tr('differs by') }} {{ formatAmount(s.netAmount - s.eligibleFinanced) }})</span></span>
      </p>

      <h3 class="sm-h"><span class="fa fa-users" aria-hidden="true"></span>{{ s.isReverse ? tr('Eligible bills by supplier') : tr('Eligible bills by debtor') }}</h3>
      <div class="fx-table-wrap">
        <table class="fx-table sm-table">
          <thead><tr>
            <th>{{ s.isReverse ? tr('Supplier / Invoice No.') : tr('Debtor / Invoice No.') }}</th><th>{{ tr('Invoice Date') }}</th><th>{{ tr('Due Date') }}</th>
            <th class="num">{{ tr('Invoice Amount') }}</th><th class="num">{{ tr('Amount Financed') }}</th>
          </tr></thead>
          <tbody v-for="p in s.parties" :key="p.taxNumber ?? p.name">
            <tr class="sm-party" @click="toggle(p)">
              <td colspan="3"><span class="fa" :class="isOpen(p) ? 'fa-caret-down' : 'fa-caret-right'" aria-hidden="true"></span>
                <b>{{ p.name }}</b> <span class="sm-muted">{{ p.taxNumber ? `· ${tr('Tax No.')} ${p.taxNumber}` : '' }} · {{ count(p.count, 'bill') }}</span></td>
              <td class="num"><b>{{ formatAmount(p.grossAmount) }}</b></td>
              <td class="num"><b>{{ formatAmount(p.financed) }}</b></td>
            </tr>
            <template v-if="isOpen(p)">
              <tr v-for="i in p.invoices" :key="i.invoiceId" class="sm-inv">
                <td>{{ i.invoiceNumber }}</td><td>{{ formatDate(i.invoiceAt) }}</td><td>{{ formatDate(i.dueAt) }}</td>
                <td class="num">{{ formatAmount(i.grossAmount) }}</td><td class="num">{{ formatAmount(i.financed) }}</td>
              </tr>
            </template>
          </tbody>
          <tbody v-if="s.parties.length === 0"><tr><td colspan="5" class="sm-muted">{{ tr('No eligible invoices.') }}</td></tr></tbody>
          <tfoot>
            <tr class="sm-total"><th colspan="3">{{ tr('Total eligible') }} · {{ count(s.eligibleCount, 'bill') }}</th>
              <th class="num">{{ formatAmount(s.eligibleGross) }}</th><th class="num">{{ formatAmount(s.eligibleFinanced) }}</th></tr>
          </tfoot>
        </table>
      </div>

      <template v-if="s.notEligible.length">
        <h3 class="sm-h"><span class="fa fa-ban" aria-hidden="true"></span>{{ tr('Not eligible') }}</h3>
        <div class="fx-table-wrap">
          <table class="fx-table sm-table">
            <thead><tr><th>{{ tr('Invoice No.') }}</th><th>{{ tr('Due Date') }}</th><th class="num">{{ tr('Invoice Amount') }}</th><th>{{ tr('Reason') }}</th></tr></thead>
            <tbody>
              <tr v-for="i in s.notEligible" :key="i.invoiceId" class="sm-off">
                <td>{{ i.invoiceNumber }}</td><td>{{ formatDate(i.dueAt) }}</td><td class="num">{{ formatAmount(i.grossAmount) }}</td><td>{{ i.ineligibleReason ?? '—' }}</td>
              </tr>
            </tbody>
            <tfoot><tr class="sm-total"><th colspan="2">{{ tr('Total not eligible') }}</th><th class="num">{{ formatAmount(s.notEligibleGross) }}</th><th></th></tr></tfoot>
          </table>
        </div>
      </template>
    </template>
    <template #footer>
      <button type="button" class="fx-btn" @click="visible = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
    </template>
  </Crm360Dialog>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { disbursementSummaryApi, type DisbursementSummary, type SummaryParty } from '../api/disbursementSummary.api'

const props = defineProps<{ disbursementId: number; from: 'crm' | 'trs' }>()

const visible = ref(false)
const loading = ref(false)
const s = ref<DisbursementSummary | null>(null)
const opened = ref(new Set<string>())

const count = (n: number, word: string) => `${n} ${tr(n === 1 ? word : word + 's')}`
const key = (p: SummaryParty) => p.taxNumber ?? p.name
const isOpen = (p: SummaryParty) => opened.value.has(key(p))
function toggle(p: SummaryParty) {
  const next = new Set(opened.value)
  if (next.has(key(p))) next.delete(key(p))
  else next.add(key(p))
  opened.value = next
}

// "88% of the eligible amount"; when the invoices carry their own advance amounts, the share they come to
const rateText = computed(() => {
  const v = s.value
  if (!v) return ''
  const pct = v.advanceRatePct ?? (v.eligibleGross > 0 ? (v.eligibleFinanced / v.eligibleGross) * 100 : null)
  return pct === null ? '' : `${Number(pct.toFixed(2))}% ${tr('of the eligible amount')}`
})

async function open() {
  visible.value = true
  loading.value = true
  try {
    s.value = await disbursementSummaryApi.get(props.disbursementId, props.from)
    // small batches open with every bill shown; large ones show the totals per party first
    opened.value = new Set(s.value.eligibleCount <= 30 ? s.value.parties.map(key) : [])
  } catch (e) {
    showError(e instanceof Error ? e.message : String(e))
    visible.value = false
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
.sm-muted { color: var(--fin-muted); font-size: var(--fs-xs, 12px); font-weight: 400; }
.sm-flow { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 10px; }
.sm-box { display: flex; flex-direction: column; gap: 3px; padding: 12px 14px; border: 1px solid var(--fin-line); border-radius: 8px; min-width: 0; }
.sm-box span { font-size: var(--fs-xs, 12px); font-weight: 700; color: var(--fin-muted); }
.sm-box b { font-size: 18px; color: var(--fin-ink); font-variant-numeric: tabular-nums; overflow-wrap: anywhere; }
.sm-box small { color: var(--fin-muted); }
.sm-box--bad b { color: var(--fin-bad); }
.sm-box--main { background: var(--fin-teal-50); border-color: var(--fin-teal-600); }
.sm-box--main b { color: var(--fin-teal-700); }
.sm-facts { display: flex; flex-wrap: wrap; gap: 6px 22px; margin: 12px 0 0; color: var(--fin-ink); }
.sm-diff { color: var(--fin-orange-700, #b85a12); margin-inline-start: 4px; }
.sm-h { display: flex; align-items: center; gap: 8px; margin: 18px 0 8px; font-size: var(--fs-md, 14px); color: var(--fin-teal-700); }
.sm-h .fa { color: var(--fin-orange); }
.sm-party { cursor: pointer; background: var(--fin-neutral-50, #f4f7f8); }
.sm-party .fa { width: 12px; color: var(--fin-teal-600); }
.sm-inv td:first-child { padding-inline-start: 30px; }
.sm-total th { background: var(--fin-teal-50); color: var(--fin-teal-800, var(--fin-ink)); }
.sm-off td { color: var(--fin-muted); }
.num { text-align: end; font-variant-numeric: tabular-nums; }
@media (max-width: 760px) { .sm-flow { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
