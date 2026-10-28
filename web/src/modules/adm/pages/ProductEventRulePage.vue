<script setup lang="ts">
// One product event rule, its charges (ADM_PRODUCT_EVENT_RULE_CHARGES) and GL entries (ADM_PRODUCT_EVENT_RULE_GL_LINES):
// FINOVA_FINANCEING ProductEventRulesPage, Components and Accounting tabs.
import { computed, onMounted, ref } from 'vue'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FieldOption, FormValues } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { formatCell } from '@/shared/crud/format'
import { tr } from '@/shared/layout/language'
import { todayInCairo } from '@/shared/format/datetime'
import '@/shared/styles/apex-fx-inbox.css'
import { CALCULATION_METHODS, CHARGE_TIMINGS, chargesApi, labelOf, productEventRulesApi, type ProductEventRule, type RuleCharge, type RuleGlLine } from '../api/products.api'

const props = defineProps<{ ruleId: string }>()
const id = computed(() => Number(props.ruleId))

type Row = RuleCharge & { methodLabel: string; timingLabel: string; valueLabel: string }

const rule = ref<ProductEventRule | null>(null)
const charges = ref<FieldOption[]>([])
const DAY_COUNTS: FieldOption[] = [360, 365, 366].map(v => ({ value: v, label: String(v) }))

const columns: CrudColumn<Row>[] = [
  { field: 'sequenceNo', label: 'Seq', type: 'number' },
  { field: 'chargeCode', label: 'Charge', type: 'code' },
  { field: 'chargeNameEn', label: 'Name' },
  { field: 'methodLabel', label: 'Method' },
  { field: 'valueLabel', label: 'Amount / Rate' },
  { field: 'timingLabel', label: 'Timing' },
  { field: 'taxRate', label: 'Tax', type: 'pct' }
]

const fields = ref<CrudField[]>([])
const buildFields = () => {
  fields.value = [
    { name: 'chargeId', label: 'Charge', type: 'select', required: true, options: charges.value },
    { name: 'sequenceNo', label: 'Sequence', type: 'number', required: true, min: 0 },
    { name: 'calculationMethodCode', label: 'Calculation Method', type: 'select', required: true, options: CALCULATION_METHODS },
    { name: 'chargeTimingCode', label: 'Charge Timing', type: 'select', required: true, options: CHARGE_TIMINGS },
    { name: 'fixedAmount', label: 'Fixed Amount', type: 'amount', min: 0 },
    { name: 'chargeRate', label: 'Rate %', type: 'pct', min: 0, max: 100 },
    { name: 'perUnitAmount', label: 'Per Unit Amount', type: 'amount', min: 0 },
    { name: 'spreadRate', label: 'Spread %', type: 'pct', min: 0, max: 100 },
    { name: 'dayCountBasis', label: 'Day Count Basis', type: 'select', options: DAY_COUNTS },
    { name: 'roundingScale', label: 'Rounding Scale', type: 'number', min: 0, max: 6 },
    { name: 'minimumAmount', label: 'Minimum Amount', type: 'amount', min: 0 },
    { name: 'maximumAmount', label: 'Maximum Amount', type: 'amount', min: 0 },
    { name: 'dueDays', label: 'Due Days', type: 'number', min: 0 },
    { name: 'taxRate', label: 'Tax Rate %', type: 'pct', min: 0, max: 100 },
    { name: 'effectiveFromDate', label: 'Effective From', type: 'date', required: true },
    { name: 'effectiveToDate', label: 'Effective To', type: 'date' }
  ]
}
buildFields()

const facts = computed(() => {
  const r = rule.value
  if (!r) return []
  return [
    ['Product', `${r.productCode} - ${r.productNameEn}`],
    ['Accounting Event', `${r.eventTypeCode} - ${r.eventTypeNameEn}`],
    ['Priority', String(r.priorityNo)],
    ['Effective', `${formatCell(r.effectiveFromDate, 'date')}${r.effectiveToDate ? ' – ' + formatCell(r.effectiveToDate, 'date') : ''}`]
  ] as [string, string][]
})

// the figure that applies to the method: a fixed amount, an amount per unit, or a rate (plus spread over the index)
const valueOf = (c: RuleCharge) => {
  if (c.calculationMethodCode === 'FIXED') return formatCell(c.fixedAmount, 'amount')
  if (c.calculationMethodCode === 'PER_UNIT') return c.perUnitAmount == null ? '' : `${formatCell(c.perUnitAmount, 'amount')} / unit`
  if (c.calculationMethodCode === 'INDEX_PLUS_SPREAD') return `${tr('Index')} + ${formatCell(c.spreadRate, 'pct')}`
  return formatCell(c.chargeRate, 'pct')
}

const toRow = (c: RuleCharge): Row => ({
  ...c,
  methodLabel: labelOf(CALCULATION_METHODS, c.calculationMethodCode),
  timingLabel: labelOf(CHARGE_TIMINGS, c.chargeTimingCode),
  valueLabel: valueOf(c)
})

const toBody = (v: FormValues, rowVersion: number | null) => ({
  ...v,
  chargeId: v.chargeId == null ? null : Number(v.chargeId),
  dayCountBasis: v.dayCountBasis == null ? null : Number(v.dayCountBasis),
  rowVersion
})

// the GL list shows the rule's charges by code, and loses a deleted charge's lines, so it reloads after every charge list load
const glPage = ref<{ reload: () => Promise<void> } | null>(null)
const ruleCharges = ref<RuleCharge[]>([])

const api = computed<CrudApi<Row>>(() => ({
  list: async () => {
    ruleCharges.value = await productEventRulesApi.charges(id.value)
    buildGlFields()
    void glPage.value?.reload()
    return ruleCharges.value.map(toRow)
  },
  create: async v => toRow(await productEventRulesApi.createCharge(id.value, toBody(v, null))),
  update: async (row, v) => toRow(await productEventRulesApi.updateCharge(id.value, row.ruleChargeId, toBody(v, row.rowVersion))),
  remove: row => productEventRulesApi.removeCharge(id.value, row.ruleChargeId, row.rowVersion)
}))

// GL entries: lines of the rule itself (section "Rule") and lines that post one of its charges
type GlRow = RuleGlLine & { sectionLabel: string; dcLabel: string; accountLabel: string }

const RULE_SECTION = 'RULE'
const DEBIT_CREDIT: FieldOption[] = [{ value: 'D', label: 'Debit' }, { value: 'C', label: 'Credit' }]
const glAccounts = ref<FieldOption[]>([])

const glColumns: CrudColumn<GlRow>[] = [
  { field: 'sectionLabel', label: 'Section', type: 'code' },
  { field: 'lineNo', label: 'Line', type: 'number' },
  { field: 'dcLabel', label: 'Dr / Cr' },
  { field: 'accountLabel', label: 'Account' },
  { field: 'descriptionTemplate', label: 'Description' }
]

const glFields = ref<CrudField[]>([])
function buildGlFields() {
  glFields.value = [
    { name: 'section', label: 'Section', type: 'select', required: true,
      options: [{ value: RULE_SECTION, label: 'Rule (main entry)' }, ...ruleCharges.value.map(c => ({ value: c.ruleChargeId, label: `Charge: ${c.chargeCode} - ${c.chargeNameEn}` }))] },
    { name: 'lineNo', label: 'Line No', type: 'number', required: true, min: 1 },
    { name: 'debitCreditCode', label: 'Debit / Credit', type: 'select', required: true, options: DEBIT_CREDIT },
    { name: 'accountId', label: 'Account', type: 'select', required: true, options: glAccounts.value },
    { name: 'descriptionTemplate', label: 'Description', type: 'textarea', maxLength: 500 }
  ]
}
buildGlFields()

const toGlRow = (l: RuleGlLine): GlRow => ({
  ...l,
  sectionLabel: l.chargeCode ?? tr('Rule'),
  dcLabel: labelOf(DEBIT_CREDIT as { value: string; label: string }[], l.debitCreditCode),
  accountLabel: `${l.accountCode} - ${l.accountNameEn}`
})

const toGlBody = (v: FormValues, rowVersion: number | null) => {
  const { section, ...rest } = v
  return {
    ...rest,
    ruleChargeId: section == null || section === RULE_SECTION ? null : Number(section),
    accountId: v.accountId == null ? null : Number(v.accountId),
    rowVersion
  }
}

const glApi = computed<CrudApi<GlRow>>(() => ({
  list: async () => (await productEventRulesApi.glLines(id.value)).map(toGlRow),
  create: async v => toGlRow(await productEventRulesApi.createGlLine(id.value, toGlBody(v, null))),
  update: async (row, v) => toGlRow(await productEventRulesApi.updateGlLine(id.value, row.ruleGlLineId, toGlBody(v, row.rowVersion))),
  remove: row => productEventRulesApi.removeGlLine(id.value, row.ruleGlLineId, row.rowVersion)
}))

onMounted(async () => {
  const [r, c, a] = await Promise.all([productEventRulesApi.get(id.value), chargesApi.list(), productEventRulesApi.glAccounts()])
  rule.value = r
  charges.value = c.filter(x => x.isActive).map(x => ({ value: x.chargeId, label: `${x.chargeCode} - ${x.chargeNameEn}` }))
  glAccounts.value = a.map(x => ({ value: x.accountId, label: `${x.accountCode} - ${x.accountNameEn}` }))
  buildFields()
  buildGlFields()
})
</script>

<template>
  <div class="fx prc">
    <div class="fx-head">
      <div>
        <h1>{{ rule ? `${rule.ruleCode} · ${rule.ruleNameEn}` : tr('Product Event Rule') }}</h1>
        <p>{{ tr('The charges this product applies on this accounting event, and their GL entries.') }}</p>
      </div>
      <div class="fx-actions">
        <span v-if="rule" class="fx-chip" :class="{ 'fx-chip--ok': rule.isActive }">{{ tr(rule.isActive ? 'Active' : 'Inactive') }}</span>
        <RouterLink to="/admin/product-event-rules" class="fx-btn fx-btn--ghost"><i class="pi pi-arrow-left"></i><span>{{ tr('Product Event Rules') }}</span></RouterLink>
      </div>
    </div>

    <section v-if="rule" class="fx-card fx-card-pad">
      <div class="prc-facts">
        <div v-for="[label, value] in facts" :key="label"><span>{{ tr(label) }}</span><b>{{ value }}</b></div>
      </div>
    </section>

    <CrudPage
      embedded
      title="Rule Charges"
      description="Charges, pricing method and timing."
      icon="pi pi-wallet"
      apex-pages="FINOVA_FINANCEING"
      entity-label="Rule Charge"
      :columns="columns"
      :fields="fields"
      :api="api"
      :row-key="row => row.ruleChargeId"
      :new-values="() => ({ isActive: true, sequenceNo: 10, calculationMethodCode: 'FIXED', chargeTimingCode: 'DUE_SEPARATELY', dayCountBasis: 365, roundingScale: 2, spreadRate: 0, dueDays: 0, taxRate: 0, effectiveFromDate: rule?.effectiveFromDate ?? todayInCairo() })"
      :to-values="row => ({ ...row })"
      :can-edit="hasPermission('ADM.PRODUCT.EDIT')"
    />

    <CrudPage
      ref="glPage"
      embedded
      title="GL Entries"
      description="Debit and credit lines of the rule's main entry and of each charge."
      icon="pi pi-book"
      apex-pages="FINOVA_FINANCEING"
      entity-label="GL Line"
      :columns="glColumns"
      :fields="glFields"
      :api="glApi"
      :row-key="row => row.ruleGlLineId"
      :new-values="() => ({ isActive: true, section: RULE_SECTION, lineNo: 10 })"
      :to-values="row => ({ ...row, section: row.ruleChargeId ?? RULE_SECTION })"
      :can-edit="hasPermission('ADM.PRODUCT.EDIT')"
    />
  </div>
</template>

<style scoped>
.prc { display: flex; flex-direction: column; gap: 16px; }
.prc-facts { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 14px 18px; }
.prc-facts span { display: block; color: var(--cap-muted); font-size: 11px; }
.prc-facts b { display: block; margin-top: 3px; color: var(--cap-heading); font-size: 14px; font-weight: 600; }
@media (max-width: 900px) { .prc-facts { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
