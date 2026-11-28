<template>
  <!-- Rules and GL (Ahmed, 27 Nov 2026): a dark orange button on New Finance and every CF_FINANCE step; its popup shows, read only, the role
       of the page, the product rules its buttons post, their charges and the GL lines with this finance's amounts. -->
  <button type="button" class="cfr-btn" @click="open"><span class="fa fa-cogs" aria-hidden="true"></span>{{ tr('Rules and GL') }} ›</button>
  <Crm360Dialog v-model:visible="shown" :title="tr('Rules and GL')" :subtitle="subtitle" icon="fa-cogs" :width="1150">
    <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
    <template v-else-if="data">
      <section v-for="pg in data.pages" :key="pg.stepCode" class="cfr-page">
        <h4>{{ tr(pg.stepName) }}</h4>
        <div v-if="!pg.rules.length" class="cfr-warn" role="note"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>
          <div><b>{{ tr('No rule on this step') }}</b><br>{{ tr('It posts nothing.') }}</div></div>
        <div v-for="r in pg.rules" :key="r.button + r.ruleCode" class="cfr-rule">
          <div class="cfr-rhead">
            <span class="cfr-tag">{{ r.ruleCode }}</span><b>{{ r.ruleName ?? '' }}</b>
          </div>
          <p v-if="r.problem" class="cfr-bad">{{ r.problem }}</p>
          <div v-if="r.notApplied" class="cfr-warn" role="note"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>
            <div><b>{{ tr('Rule not applied on this finance') }}</b><br>{{ tr(r.notApplied) }}</div></div>
          <table v-if="r.charges.length" class="fx-table">
            <thead><tr><th>{{ tr('Charge') }}</th><th>{{ tr('Calculation') }}</th><th>{{ tr('Default timing') }}</th><th>{{ tr('Income') }}</th><th class="fx-r">{{ tr('On this finance') }}</th></tr></thead>
            <tbody><tr v-for="c in r.charges" :key="c.chargeCode"><td>{{ tr(c.chargeName) }}</td><td>{{ tr(c.calculation) }}</td><td>{{ tr(c.defaultTiming) }}</td><td>{{ tr(c.income) }}</td><td class="fx-r">{{ c.onThisFinance ?? '—' }}</td></tr></tbody>
          </table>
          <table v-if="r.lines.length" class="fx-table">
            <thead><tr><th>J</th><th>{{ tr('Dr/Cr') }}</th><th>{{ tr('Account') }}</th><th>{{ tr('Amount code') }}</th><th class="fx-r">{{ tr('Amount') }}</th></tr></thead>
            <tbody>
              <tr v-for="(l, i) in r.lines" :key="i">
                <td>{{ l.journalNo }}</td><td>{{ l.debitCredit === 'D' ? tr('Dr') : tr('Cr') }}</td><td>{{ l.account }}</td>
                <td>{{ l.amountCode }}<template v-if="l.chargeCode"> · {{ l.chargeCode }}</template></td>
                <td class="fx-r">{{ l.amount === null ? '—' : money(l.amount) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
      <p class="cfr-p">{{ tr('Read only. Change the rules in Admin Setup › Product Catalog › the product › Product Event Rules.') }}</p>
    </template>
    <template #footer>
      <button type="button" class="fx-btn" @click="shown = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Close') }}</button>
    </template>
  </Crm360Dialog>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { loansApi, type CfRules } from '../api/loans.api'

const props = defineProps<{ loanId: number | null; step: string | null; title?: string
  /** Credit's choices still on the screen (not saved until Approve). */
  choices?: { charges: { code: string; timing: string; amount: number }[]; down: number | null } | null }>()
const shown = ref(false)
const loading = ref(false)
const data = ref<CfRules | null>(null)
const subtitle = computed(() => props.title ?? '')
const money = (n: number) => new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n)

async function open() {
  shown.value = true
  loading.value = true
  try {
    data.value = await loansApi.cfRules(props.loanId, props.step, props.choices ?? null)
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.cfr-btn { display: inline-flex; align-items: center; gap: 6px; padding: 4px 10px; border: 1px solid #E87722; border-radius: 7px; background: #fff; color: #c45f12;
  font: 600 12.5px/1.2 inherit; text-transform: none; letter-spacing: 0; cursor: pointer; white-space: nowrap; }
.cfr-btn:hover { background: #fff4ea; }
.cfr-warn { display: flex; gap: 10px; align-items: flex-start; margin: 6px 0 10px; padding: 10px 14px; border: 1px solid #f0c36d; border-radius: 8px; background: #fff8e6; color: #7a4b00; font-size: 13px; }
.cfr-warn .fa { margin-top: 2px; color: #d08a00; }
.cfr-p { margin: 0 0 8px; font-size: 12.5px; }
.cfr-page { padding: 10px 0; border-top: 1px solid #eef1f3; }
.cfr-page h4 { margin: 0 0 4px; font-size: 14px; color: var(--fin-teal-700); }
.cfr-rule { margin: 8px 0 12px; }
.cfr-rhead { display: flex; flex-wrap: wrap; align-items: center; gap: 8px; margin-bottom: 6px; font-size: 13px; }
.cfr-tag { padding: 1px 7px; border-radius: 6px; background: #f2f4f6; font: 600 12px/1.6 ui-monospace, monospace; }
.cfr-bad { margin: 4px 0; color: var(--fin-danger, #b42318); font-size: 13px; }
.cfr-rule .fx-table { margin-bottom: 6px; }
</style>
