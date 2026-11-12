<template>
  <!-- Retail Lending > End of Day: one run per business day, in order. It applies customer credit to what fell due, accrues the day's
       interest, charges late fees after the grace days (from day 1), moves late loans into non-accrual and suspense, releases it when
       they cure, and proposes loans for write-off. GL posts each product's rule LND_EOD. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'lnd-loans' }">{{ tr('Retail Lending') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ tr('End of Day') }}</b>
        </nav>
        <h1>{{ tr('End of Day') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'lnd-loans' }" class="fx-btn"><span class="fa fa-list" aria-hidden="true"></span>{{ tr('Loans') }}</RouterLink>
        <button v-if="page?.canRun" type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="confirming = true">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-play'" aria-hidden="true"></span>{{ tr('Run') }} {{ formatDate(page.nextDate!) }}
        </button>
      </div>
    </div>

    <section v-if="page" class="fx-card">
      <div class="ln-facts">
        <div><small>{{ tr('Today') }}</small><b>{{ formatDate(page.today) }}</b></div>
        <div><small>{{ tr('Last day done') }}</small><b>{{ page.lastDoneDate ? formatDate(page.lastDoneDate) : tr('None yet') }}</b></div>
        <div><small>{{ tr('Next day to run') }}</small><b>{{ page.nextDate ? formatDate(page.nextDate) : '—' }}</b></div>
        <div><small>{{ tr('In non-accrual') }}</small><b>{{ page.nonAccrual.length }}</b></div>
        <div><small>{{ tr('Proposed for write-off') }}</small><b>{{ page.writeOffProposed.length }}</b></div>
      </div>
      <p v-if="page.blocked" class="c3-muted ln-note"><span class="fa fa-check-circle" aria-hidden="true"></span> {{ tr(page.blocked) }}</p>
    </section>

    <section class="fx-card">
      <h2 class="ln-h2">{{ tr('Runs') }}</h2>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Business day') }}</th><th>{{ tr('Status') }}</th><th class="ln-num">{{ tr('Loans') }}</th><th class="ln-num">{{ tr('Credit applied') }}</th>
              <th class="ln-num">{{ tr('Interest accrued') }}</th><th class="ln-num">{{ tr('To suspense') }}</th><th class="ln-num">{{ tr('Late fees') }}</th>
              <th>{{ tr('Non-accrual / cured / write-off') }}</th><th>{{ tr('GL') }}</th><th>{{ tr('By') }}</th>
            </tr>
          </thead>
          <tbody>
            <template v-for="r in page?.runs ?? []" :key="r.eodRunId">
              <tr>
                <td><b>{{ formatDate(r.runDate) }}</b></td>
                <td><span class="fx-chip" :class="CHIP[r.statusCode]">{{ tr(STATUS[r.statusCode] ?? r.statusCode) }}</span></td>
                <td class="ln-num">{{ r.loansCount }}</td>
                <td class="ln-num">{{ money(r.creditAppliedAmount) }}</td>
                <td class="ln-num">{{ money(r.interestAccruedAmount) }}</td>
                <td class="ln-num">{{ money(r.interestSuspendedAmount) }}</td>
                <td class="ln-num">{{ money(r.lateFeeAmount) }}</td>
                <td>{{ r.nonAccrualCount }} / {{ r.curedCount }} / {{ r.writeOffProposedCount }}</td>
                <td><small>{{ r.glTranSetIds ?? '—' }}</small></td>
                <td>{{ r.createdBy }}</td>
              </tr>
              <tr v-if="r.errorMessage" class="ln-error"><td colspan="10"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span> {{ r.errorMessage }}</td></tr>
            </template>
          </tbody>
        </table>
        <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
        <div v-else-if="!page?.runs.length" class="fx-empty"><span class="fa fa-calendar-o" aria-hidden="true"></span>{{ tr('End of day has not run yet.') }}</div>
      </div>
    </section>

    <section v-for="list in lists" :key="list.title" class="fx-card">
      <h2 class="ln-h2">{{ tr(list.title) }} <small class="c3-muted">{{ list.rows.length }}</small></h2>
      <p class="c3-muted ln-note">{{ tr(list.help) }}</p>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Loan') }}</th><th>{{ tr('Customer') }}</th><th>{{ tr('Product') }}</th><th class="ln-num">{{ tr('Days late') }}</th>
              <th class="ln-num">{{ tr('Principal outstanding') }}</th><th class="ln-num">{{ tr('Interest suspense') }}</th><th class="ln-num">{{ tr('Late fee suspense') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="l in list.rows" :key="l.loanId" class="ln-row" @click="router.push({ name: 'lnd-loan', params: { loanId: l.loanId } })">
              <td><b>{{ l.loanNumber }}</b></td><td>{{ l.clientName }}</td><td>{{ l.productCode }}</td><td class="ln-num">{{ l.daysLate }}</td>
              <td class="ln-num">{{ money(l.principalOutstanding) }}</td><td class="ln-num">{{ money(l.interestSuspenseAmount) }}</td>
              <td class="ln-num">{{ money(l.lateFeeSuspenseAmount) }}</td>
            </tr>
          </tbody>
        </table>
        <div v-if="!list.rows.length" class="fx-empty"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('None.') }}</div>
      </div>
    </section>

    <Crm360Dialog v-model:visible="confirming" :title="tr('Run end of day')" icon="fa-play" :busy="busy" :width="560">
      <p>{{ tr('End of day will run for') }} <b>{{ page?.nextDate ? formatDate(page.nextDate) : '' }}</b>.</p>
      <ol class="ln-steps">
        <li>{{ tr('Customer credit balances pay what fell due, as credit balance receipts.') }}</li>
        <li>{{ tr('Interest accrues, late fees are charged after the grace days, and late loans move into or out of non-accrual.') }}</li>
        <li>{{ tr('GL posts each product\'s rule LND_EOD.') }}</li>
      </ol>
      <p class="c3-muted">{{ tr('Days run in order and a day runs once. A day that failed can be run again after the cause is fixed.') }}</p>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="confirming = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="run"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-play'" aria-hidden="true"></span>{{ tr('Run') }}</button>
      </template>
    </Crm360Dialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { eodApi, type EodPage } from '../api/eod.api'

const STATUS: Record<string, string> = { RUNNING: 'Running', DONE: 'Done', FAILED: 'Failed' }
const CHIP: Record<string, string> = { RUNNING: 'fx-chip--warn', DONE: 'fx-chip--ok', FAILED: 'fx-chip--bad' }
const egp = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const money = (n: number) => egp.format(n)

const router = useRouter()
const page = ref<EodPage | null>(null)
const loading = ref(false)
const busy = ref(false)
const confirming = ref(false)
const lists = computed(() => [
  { title: 'Loans in non-accrual', help: 'Late at or beyond the non-accrual days: their interest and late fees go to suspense until they are no longer late.', rows: page.value?.nonAccrual ?? [] },
  { title: 'Proposed for write-off', help: 'Late at or beyond the write-off days: they wait for a write-off decision.', rows: page.value?.writeOffProposed ?? [] }
])

async function run() {
  busy.value = true
  const day = page.value?.nextDate ?? null
  try {
    page.value = await eodApi.run(day)
    confirming.value = false
    showSuccess(`${tr('End of day done:')} ${day ? formatDate(day) : ''}`)
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
    await load()
  } finally {
    busy.value = false
  }
}

async function load() {
  loading.value = true
  try {
    page.value = await eodApi.get()
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
.ln-row { cursor: pointer; }
.ln-num { text-align: end; white-space: nowrap; }
.ln-h2 { font-size: 16px; margin: 4px 4px 8px; }
.ln-note { margin: 4px 4px 10px; }
.ln-facts { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: 8px; }
.ln-facts > div { display: flex; flex-direction: column; gap: 2px; padding: 8px 10px; border-radius: var(--fin-radius-sm); background: var(--fin-teal-50); }
.ln-facts small { color: var(--fin-muted); }
.ln-error td { color: var(--fin-danger, #b42318); background: var(--fin-danger-50, #fef3f2); }
.ln-steps { margin: 10px 0; padding-inline-start: 20px; display: flex; flex-direction: column; gap: 6px; }
@media (max-width: 900px) {
  .ln-facts { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
</style>
