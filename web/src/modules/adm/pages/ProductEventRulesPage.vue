<script setup lang="ts">
// Product Event Rules (ADM_PRODUCT_EVENT_RULES): FINOVA_FINANCEING ProductEventRulesPage. The rule list, and under it the
// configuration of the selected rule in three tabs as there: Rule GL Entries (the posting of the business event itself),
// Charge Components, and Component GL Entries (the posting of one charge).
import { computed, nextTick, ref } from 'vue'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FieldOption, FormValues } from '@/shared/crud/types'
import { formatCell } from '@/shared/crud/format'
import { hasPermission } from '@/shared/auth/session'
import { tr } from '@/shared/layout/language'
import { todayInCairo } from '@/shared/format/datetime'
import {
  CALCULATION_METHODS, CHARGE_TIMINGS, chargesApi, labelOf, productEventRulesApi, productsApi,
  type ProductEventRule, type RuleCharge, type RuleGlLine
} from '../api/products.api'

const canEdit = hasPermission('ADM.PRODUCT.EDIT')

// ---- lists for the drop-downs
const products = ref<FieldOption[]>([])
const eventTypes = ref<FieldOption[]>([])
const charges = ref<FieldOption[]>([])
const glAccounts = ref<FieldOption[]>([])

// ---- rule list
type RuleRow = ProductEventRule

const productFilter = ref('')
const eventFilter = ref('')

const ruleColumns: CrudColumn<RuleRow>[] = [
  { field: 'productCode', label: 'Product', type: 'code' },
  { field: 'eventTypeCode', label: 'Business Event', type: 'code' },
  { field: 'ruleCode', label: 'Rule Code', type: 'code' },
  { field: 'ruleNameEn', label: 'Rule Name' },
  { field: 'priorityNo', label: 'Priority', type: 'number' },
  { field: 'effectiveFromDate', label: 'Effective From', type: 'date' },
  { field: 'effectiveToDate', label: 'Effective To', type: 'date' }
]

const ruleFields = computed<CrudField[]>(() => [
  { name: 'productId', label: 'Product', type: 'select', required: true, options: products.value },
  { name: 'eventTypeId', label: 'Business Event', type: 'select', required: true, options: eventTypes.value },
  { name: 'ruleCode', label: 'Rule Code', type: 'text', required: true, maxLength: 50, uppercase: true },
  { name: 'priorityNo', label: 'Priority No', type: 'number', required: true, min: 0 },
  { name: 'ruleNameEn', label: 'Rule Name (EN)', type: 'text', required: true, maxLength: 200 },
  { name: 'ruleNameAr', label: 'Rule Name (AR)', type: 'text', maxLength: 200, rtl: true },
  { name: 'effectiveFromDate', label: 'Effective From', type: 'date', required: true },
  { name: 'effectiveToDate', label: 'Effective To', type: 'date' }
])

const toRuleBody = (v: FormValues, rowVersion: number | null) => ({
  ...v,
  productId: v.productId == null ? null : Number(v.productId),
  eventTypeId: v.eventTypeId == null ? null : Number(v.eventTypeId),
  rowVersion
})

const ruleApi = computed<CrudApi<RuleRow>>(() => {
  const product = productFilter.value
  const event = eventFilter.value
  return {
    list: async () => {
      const rows = (await productEventRulesApi.list())
      // keep the open configuration in step with the saved rule
      if (selectedRule.value) selectedRule.value = rows.find(x => x.productEventRuleId === selectedRule.value!.productEventRuleId) ?? null
      return rows.filter(r => (!product || String(r.productId) === product) && (!event || String(r.eventTypeId) === event))
    },
    create: v => productEventRulesApi.create(toRuleBody(v, null)),
    update: (row, v) => productEventRulesApi.update(row.productEventRuleId, toRuleBody(v, row.rowVersion)),
    remove: async row => {
      await productEventRulesApi.remove(row.productEventRuleId, row.rowVersion)
      if (selectedRule.value?.productEventRuleId === row.productEventRuleId) closeConfiguration()
    }
  }
})

// ---- configuration of the selected rule
type Tab = 'RULE_GL' | 'COMPONENTS' | 'COMPONENT_GL'

const selectedRule = ref<RuleRow | null>(null)
const selectedComponent = ref<RuleCharge | null>(null)
const activeTab = ref<Tab>('RULE_GL')
const ruleCharges = ref<RuleCharge[]>([])
const glLines = ref<RuleGlLine[]>([])
const configEl = ref<HTMLElement | null>(null)

const ruleId = computed(() => selectedRule.value?.productEventRuleId ?? 0)
const ruleLines = computed(() => glLines.value.filter(l => l.ruleChargeId == null))
const componentLines = computed(() => glLines.value.filter(l => l.ruleChargeId === selectedComponent.value?.ruleChargeId))

async function configureRule(row: RuleRow) {
  selectedRule.value = row
  selectedComponent.value = null
  activeTab.value = 'RULE_GL'
  ruleCharges.value = []
  glLines.value = []
  void productEventRulesApi.charges(row.productEventRuleId).then(x => { ruleCharges.value = x })
  await nextTick()
  configEl.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

function closeConfiguration() {
  selectedRule.value = null
  selectedComponent.value = null
}

function openComponentGl(component: RuleCharge) {
  selectedComponent.value = component
  activeTab.value = 'COMPONENT_GL'
}

// ---- charge components
type ComponentRow = RuleCharge & { methodLabel: string; timingLabel: string; valueLabel: string; glCount: number }

const DAY_COUNTS: FieldOption[] = [360, 365, 366].map(v => ({ value: v, label: String(v) }))

const componentColumns: CrudColumn<ComponentRow>[] = [
  { field: 'sequenceNo', label: 'Seq.', type: 'number' },
  { field: 'chargeCode', label: 'Component', type: 'code' },
  { field: 'methodLabel', label: 'Calculation' },
  { field: 'valueLabel', label: 'Rate / Amount' },
  { field: 'timingLabel', label: 'Charge Timing' },
  { field: 'glCount', label: 'GL Lines', type: 'number' }
]

const componentFields = computed<CrudField[]>(() => [
  { name: 'chargeId', label: 'Charge Component', type: 'select', required: true, options: charges.value },
  { name: 'sequenceNo', label: 'Sequence No', type: 'number', required: true, min: 0 },
  { name: 'calculationMethodCode', label: 'Calculation Method', type: 'select', required: true, options: CALCULATION_METHODS },
  { name: 'chargeTimingCode', label: 'Charge Timing', type: 'select', required: true, options: CHARGE_TIMINGS },
  { name: 'fixedAmount', label: 'Fixed Amount', type: 'amount', min: 0 },
  { name: 'chargeRate', label: 'Rate Percent', type: 'pct', min: 0, max: 100 },
  { name: 'perUnitAmount', label: 'Per Unit Amount', type: 'amount', min: 0 },
  { name: 'spreadRate', label: 'Spread Percent', type: 'pct', min: 0, max: 100 },
  { name: 'dayCountBasis', label: 'Day Count Basis', type: 'select', options: DAY_COUNTS },
  { name: 'roundingScale', label: 'Rounding Scale', type: 'number', required: true, min: 0, max: 6 },
  { name: 'minimumAmount', label: 'Minimum Amount', type: 'amount', min: 0 },
  { name: 'maximumAmount', label: 'Maximum Amount', type: 'amount', min: 0 },
  { name: 'dueDays', label: 'Due Days', type: 'number', required: true, min: 0 },
  { name: 'taxRate', label: 'Tax Rate Percent', type: 'pct', min: 0, max: 100 },
  { name: 'effectiveFromDate', label: 'Effective From', type: 'date', required: true },
  { name: 'effectiveToDate', label: 'Effective To', type: 'date' }
])

// the figure that applies to the method: a fixed amount, an amount per unit, or a rate (plus spread over the index)
const valueOf = (c: RuleCharge) => {
  if (c.calculationMethodCode === 'FIXED') return formatCell(c.fixedAmount, 'amount')
  if (c.calculationMethodCode === 'PER_UNIT') return c.perUnitAmount == null ? '' : `${formatCell(c.perUnitAmount, 'amount')} / unit`
  if (c.calculationMethodCode === 'INDEX_PLUS_SPREAD') return `${tr('Index')} + ${formatCell(c.spreadRate, 'pct')}`
  return formatCell(c.chargeRate, 'pct')
}

const toComponentRow = (c: RuleCharge, lines: RuleGlLine[]): ComponentRow => ({
  ...c,
  methodLabel: labelOf(CALCULATION_METHODS, c.calculationMethodCode),
  timingLabel: labelOf(CHARGE_TIMINGS, c.chargeTimingCode),
  valueLabel: valueOf(c),
  glCount: lines.filter(l => l.ruleChargeId === c.ruleChargeId).length
})

const toComponentBody = (v: FormValues, rowVersion: number | null) => ({
  ...v,
  chargeId: v.chargeId == null ? null : Number(v.chargeId),
  dayCountBasis: v.dayCountBasis == null ? null : Number(v.dayCountBasis),
  rowVersion
})

const componentApi = computed<CrudApi<ComponentRow>>(() => {
  const id = ruleId.value
  return {
    list: async () => {
      const [list, lines] = await Promise.all([productEventRulesApi.charges(id), productEventRulesApi.glLines(id)])
      ruleCharges.value = list
      glLines.value = lines
      return list.map(c => toComponentRow(c, lines))
    },
    create: async v => toComponentRow(await productEventRulesApi.createCharge(id, toComponentBody(v, null)), glLines.value),
    update: async (row, v) => toComponentRow(await productEventRulesApi.updateCharge(id, row.ruleChargeId, toComponentBody(v, row.rowVersion)), glLines.value),
    remove: async row => {
      await productEventRulesApi.removeCharge(id, row.ruleChargeId, row.rowVersion)
      if (selectedComponent.value?.ruleChargeId === row.ruleChargeId) selectedComponent.value = null
    }
  }
})

// ---- GL entries: one form for rule-level and component lines
type GlRow = RuleGlLine & { dcLabel: string; accountLabel: string }

const DEBIT_CREDIT = [{ value: 'D', label: 'Debit' }, { value: 'C', label: 'Credit' }]

const glColumns: CrudColumn<GlRow>[] = [
  { field: 'lineNo', label: 'Line No', type: 'number' },
  { field: 'dcLabel', label: 'D/C' },
  { field: 'accountLabel', label: 'Account' },
  { field: 'descriptionTemplate', label: 'Description Template' }
]

const glFields = computed<CrudField[]>(() => [
  { name: 'lineNo', label: 'Line No', type: 'number', required: true, min: 1 },
  { name: 'debitCreditCode', label: 'Debit / Credit', type: 'select', required: true, options: DEBIT_CREDIT },
  { name: 'accountId', label: 'Account', type: 'select', required: true, options: glAccounts.value, wide: true },
  { name: 'descriptionTemplate', label: 'Description Template', type: 'textarea', maxLength: 500 }
])

const toGlRow = (l: RuleGlLine): GlRow => ({ ...l, dcLabel: labelOf(DEBIT_CREDIT, l.debitCreditCode), accountLabel: `${l.accountCode} - ${l.accountNameEn}` })

/** The GL lines of one section: the rule itself (ruleChargeId null) or one charge component. */
function glApiFor(ruleChargeId: number | null): CrudApi<GlRow> {
  const id = ruleId.value
  const body = (v: FormValues, rowVersion: number | null) => ({ ...v, ruleChargeId, accountId: v.accountId == null ? null : Number(v.accountId), rowVersion })
  return {
    list: async () => {
      glLines.value = await productEventRulesApi.glLines(id)
      return glLines.value.filter(l => l.ruleChargeId === ruleChargeId).map(toGlRow)
    },
    create: async v => toGlRow(await productEventRulesApi.createGlLine(id, body(v, null))),
    update: async (row, v) => toGlRow(await productEventRulesApi.updateGlLine(id, row.ruleGlLineId, body(v, row.rowVersion))),
    remove: row => productEventRulesApi.removeGlLine(id, row.ruleGlLineId, row.rowVersion)
  }
}

const ruleGlApi = computed(() => glApiFor(ruleId.value ? null : null))
const componentGlApi = computed(() => glApiFor(selectedComponent.value?.ruleChargeId ?? null))

// ---- start
void (async () => {
  const [p, e, c, a] = await Promise.all([
    productsApi.list(), productEventRulesApi.eventTypes(), chargesApi.list(), productEventRulesApi.glAccounts()
  ])
  products.value = p.filter(x => x.isActive).map(x => ({ value: x.productId, label: `${x.productCode} - ${x.productNameEn}` }))
  eventTypes.value = e.map(x => ({ value: x.eventTypeId, label: `${x.eventTypeCode} - ${x.eventTypeNameEn}` }))
  charges.value = c.filter(x => x.isActive).map(x => ({ value: x.chargeId, label: `${x.chargeCode} - ${x.chargeNameEn}` }))
  glAccounts.value = a.map(x => ({ value: x.accountId, label: `${x.accountCode} - ${x.accountNameEn}` }))
})()
</script>

<template>
  <div class="per">
    <CrudPage
      title="Product Event Rules"
      description="Define business-event rules, charges and their accounting entries."
      icon="pi pi-sitemap"
      apex-pages="FINOVA_FINANCEING"
      entity-label="Rule"
      :columns="ruleColumns"
      :fields="ruleFields"
      :api="ruleApi"
      :row-key="row => row.productEventRuleId"
      :new-values="() => ({ isActive: true, priorityNo: 10, effectiveFromDate: todayInCairo() })"
      :to-values="row => ({ ...row })"
      :can-edit="canEdit"
    >
      <template #actions>
        <RouterLink to="/admin" class="fx-btn fx-btn--ghost"><i class="pi pi-arrow-left"></i><span>{{ tr('Admin Setup') }}</span></RouterLink>
      </template>
      <template #filters>
        <select v-model="productFilter" class="per-filter" :aria-label="tr('Product')">
          <option value="">{{ tr('All Products') }}</option>
          <option v-for="x in products" :key="x.value" :value="String(x.value)">{{ x.label }}</option>
        </select>
        <select v-model="eventFilter" class="per-filter" :aria-label="tr('Business Event')">
          <option value="">{{ tr('All Business Events') }}</option>
          <option v-for="x in eventTypes" :key="x.value" :value="String(x.value)">{{ x.label }}</option>
        </select>
      </template>
      <template #row-actions="{ row }">
        <button type="button" class="fx-btn fx-btn--sm per-configure" :class="{ 'is-on': selectedRule?.productEventRuleId === row.productEventRuleId }"
                :title="tr('Configure rule GL entries, charge components and component GL entries')" @click="configureRule(row)">
          <i class="pi pi-cog"></i>{{ tr('Configure') }}
        </button>
      </template>
    </CrudPage>

    <section v-if="selectedRule" ref="configEl" class="fx per-config">
      <div class="fx-card fx-card-pad">
        <div class="per-head">
          <div class="per-title">
            <span class="per-icon"><i class="pi pi-sliders-h"></i></span>
            <div>
              <div class="per-kicker">{{ tr('RULE CONFIGURATION') }}</div>
              <h2>{{ selectedRule.ruleCode }} — {{ selectedRule.ruleNameEn }}</h2>
            </div>
          </div>
          <div class="per-meta">
            <span class="fx-chip" :class="{ 'fx-chip--ok': selectedRule.isActive }">{{ tr(selectedRule.isActive ? 'Active' : 'Inactive') }}</span>
            <span><b>{{ tr('Product') }}:</b> {{ selectedRule.productCode }}</span>
            <span><b>{{ tr('Event') }}:</b> {{ selectedRule.eventTypeCode }}</span>
            <span><b>{{ tr('Priority') }}:</b> {{ selectedRule.priorityNo }}</span>
            <span><b>{{ tr('Effective') }}:</b> {{ formatCell(selectedRule.effectiveFromDate, 'date') }}</span>
            <button type="button" class="per-close" :title="tr('Close configuration')" @click="closeConfiguration"><i class="pi pi-times"></i></button>
          </div>
        </div>

        <div class="per-tabs" role="tablist">
          <button type="button" role="tab" class="per-tab" :class="{ active: activeTab === 'RULE_GL' }" @click="activeTab = 'RULE_GL'">
            <i class="pi pi-book"></i>{{ tr('Rule GL Entries') }}<em>{{ ruleLines.length }}</em>
          </button>
          <button type="button" role="tab" class="per-tab" :class="{ active: activeTab === 'COMPONENTS' }" @click="activeTab = 'COMPONENTS'">
            <i class="pi pi-box"></i>{{ tr('Charge Components') }}<em>{{ ruleCharges.length }}</em>
          </button>
          <button type="button" role="tab" class="per-tab" :class="{ active: activeTab === 'COMPONENT_GL' }" :disabled="!selectedComponent"
                  @click="selectedComponent && (activeTab = 'COMPONENT_GL')">
            <i class="pi pi-sitemap"></i>{{ tr('Component GL Entries') }}<em v-if="selectedComponent">{{ componentLines.length }}</em>
          </button>
        </div>
      </div>

      <CrudPage
        v-if="activeTab === 'RULE_GL'"
        :key="`rule-gl-${ruleId}`"
        embedded
        title="Rule-Level Accounting Entries"
        description="Debit and credit entries for the business event itself, independent of charge components."
        icon="pi pi-book"
        apex-pages="FINOVA_FINANCEING"
        entity-label="Entry"
        :columns="glColumns"
        :fields="glFields"
        :api="ruleGlApi"
        :row-key="row => row.ruleGlLineId"
        :new-values="() => ({ isActive: true, lineNo: (ruleLines.length + 1) * 10, debitCreditCode: 'D' })"
        :to-values="row => ({ ...row })"
        :can-edit="canEdit"
      />

      <CrudPage
        v-else-if="activeTab === 'COMPONENTS'"
        :key="`components-${ruleId}`"
        embedded
        title="Charge Components"
        description="Pricing and charge components applicable to this Product + Business Event rule."
        icon="pi pi-box"
        apex-pages="FINOVA_FINANCEING"
        entity-label="Charge Component"
        :columns="componentColumns"
        :fields="componentFields"
        :api="componentApi"
        :row-key="row => row.ruleChargeId"
        :new-values="() => ({ isActive: true, sequenceNo: (ruleCharges.length + 1) * 10, calculationMethodCode: 'FIXED', chargeTimingCode: 'DUE_SEPARATELY', dayCountBasis: 365, roundingScale: 2, spreadRate: 0, dueDays: 0, taxRate: 0, effectiveFromDate: selectedRule?.effectiveFromDate ?? todayInCairo() })"
        :to-values="row => ({ ...row })"
        :can-edit="canEdit"
      >
        <template #row-actions="{ row }">
          <button type="button" class="fx-btn fx-btn--sm per-configure" :title="tr('Open component GL entries')" @click="openComponentGl(row)">
            <i class="pi pi-book"></i>{{ tr('GL Entries') }}
          </button>
        </template>
      </CrudPage>

      <template v-else-if="selectedComponent">
        <div class="fx-card fx-card-pad per-component">
          <div>
            <div class="per-kicker">{{ tr('SELECTED COMPONENT') }}</div>
            <h3>{{ selectedComponent.chargeCode }} — {{ selectedComponent.chargeNameEn }}</h3>
            <p>{{ tr('Accounting entries generated specifically for this charge component.') }}</p>
          </div>
          <button type="button" class="fx-btn" @click="activeTab = 'COMPONENTS'"><i class="pi pi-arrow-left"></i>{{ tr('Components') }}</button>
        </div>
        <CrudPage
          :key="`component-gl-${selectedComponent.ruleChargeId}`"
          embedded
          title="Component GL Entries"
          description="Debit and credit entries of this charge component."
          icon="pi pi-sitemap"
          apex-pages="FINOVA_FINANCEING"
          entity-label="Entry"
          :columns="glColumns"
          :fields="glFields"
          :api="componentGlApi"
          :row-key="row => row.ruleGlLineId"
          :new-values="() => ({ isActive: true, lineNo: (componentLines.length + 1) * 10, debitCreditCode: 'C' })"
          :to-values="row => ({ ...row })"
          :can-edit="canEdit"
        />
      </template>
    </section>
  </div>
</template>

<style scoped>
.per { display: flex; flex-direction: column; }
.per-filter { height: 38px; padding: 0 10px; border: 1px solid var(--cap-border); border-radius: 8px; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 13px; }
.per-configure { margin-inline-end: 6px; }
.per-configure.is-on { border-color: var(--cap-primary); color: var(--cap-primary); background: var(--cap-primary-tint); }
.per-config { display: flex; flex-direction: column; gap: 14px; margin-top: 18px; padding: 0 0 24px; }
.per-head { display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: 14px; }
.per-title { display: flex; align-items: center; gap: 12px; }
.per-title h2 { margin: 2px 0 0; color: var(--cap-heading); font-size: 18px; font-weight: 700; }
.per-icon { width: 40px; height: 40px; display: grid; place-items: center; border-radius: 12px; background: var(--cap-orange-tint); color: var(--cap-orange); font-size: 17px; }
.per-kicker { color: var(--cap-muted); font-size: 10.5px; font-weight: 700; letter-spacing: .08em; }
.per-meta { display: flex; flex-wrap: wrap; align-items: center; gap: 8px 16px; color: var(--cap-ink-soft); font-size: 12.5px; }
.per-meta b { color: var(--cap-heading); font-weight: 600; }
.per-close { width: 32px; height: 32px; display: grid; place-items: center; border: 1px solid var(--cap-border); border-radius: 50%; background: #fff; color: var(--cap-muted); cursor: pointer; }
.per-tabs { display: flex; flex-wrap: wrap; gap: 8px; margin-top: 16px; padding-top: 14px; border-top: 1px solid var(--cap-border-soft); }
.per-tab { display: inline-flex; align-items: center; gap: 8px; height: 38px; padding: 0 14px; border: 1px solid var(--cap-border); border-radius: 10px; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 13px; font-weight: 600; cursor: pointer; }
.per-tab em { min-width: 20px; padding: 1px 6px; border-radius: 99px; background: var(--cap-bg); color: var(--cap-muted); font-size: 11px; font-style: normal; text-align: center; }
.per-tab.active { border-color: var(--cap-selected); background: var(--cap-selected-tint); color: var(--cap-selected); }
.per-tab.active em { background: var(--cap-selected); color: #fff; }
.per-tab:disabled { opacity: .5; cursor: default; }
.per-component { display: flex; align-items: center; justify-content: space-between; gap: 14px; }
.per-component h3 { margin: 2px 0 4px; color: var(--cap-heading); font-size: 16px; font-weight: 700; }
.per-component p { margin: 0; color: var(--cap-muted); font-size: 12.5px; }
</style>
