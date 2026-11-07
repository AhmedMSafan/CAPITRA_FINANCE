<script setup lang="ts">
// Charges (ADM_CHARGES): FINOVA_FINANCEING ChargeComponentsPage in the shared list layout.
import { onMounted, ref } from 'vue'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FieldOption, FormValues } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { tr } from '@/shared/layout/language'
import { getLookupOptions } from '../api/lookups.api'
import { getCurrencies } from '../api/currencies.api'
import { CALCULATION_TYPES, FINANCIAL_NATURES, chargesApi, labelOf, type Charge } from '../api/products.api'

type ChargeRow = Charge & { natureLabel: string; calculationLabel: string; defaultValue: string }

const chargeTypes = ref<FieldOption[]>([])
const currencies = ref<FieldOption[]>([])

const columns: CrudColumn<ChargeRow>[] = [
  { field: 'chargeCode', label: 'Code', type: 'code' },
  { field: 'chargeNameEn', label: 'Name' },
  { field: 'chargeTypeCode', label: 'Type', type: 'code' },
  { field: 'natureLabel', label: 'Financial Nature' },
  { field: 'calculationLabel', label: 'Calculation' },
  { field: 'defaultValue', label: 'Default' },
  { field: 'isRecurring', label: 'Recurring', type: 'bool' }
]

const fields = ref<CrudField[]>([])
const buildFields = () => {
  fields.value = [
    { name: 'chargeCode', label: 'Charge Code', type: 'text', required: true, maxLength: 50, uppercase: true },
    { name: 'chargeTypeCode', label: 'Charge Type', type: 'select', required: true, options: chargeTypes.value },
    { name: 'chargeNameEn', label: 'Name (EN)', type: 'text', required: true, maxLength: 150 },
    { name: 'chargeNameAr', label: 'Name (AR)', type: 'text', maxLength: 150, rtl: true },
    { name: 'financialNatureCode', label: 'Financial Nature', type: 'select', required: true, options: FINANCIAL_NATURES },
    { name: 'calculationTypeCode', label: 'Calculation Type', type: 'select', required: true, options: CALCULATION_TYPES },
    { name: 'defaultRate', label: 'Default Rate % (percentage)', type: 'pct', min: 0, max: 100 },
    { name: 'defaultAmount', label: 'Default Amount (fixed)', type: 'amount', min: 0 },
    { name: 'currencyCode', label: 'Currency (fixed)', type: 'select', options: currencies.value },
    { name: 'isRecurring', label: 'Recurring', type: 'bool' },
    { name: 'effectiveFromDate', label: 'Effective From', type: 'date' },
    { name: 'effectiveToDate', label: 'Effective To', type: 'date' },
    { name: 'description', label: 'Description', type: 'textarea', maxLength: 500 }
  ]
}
buildFields()

const toRow = (c: Charge): ChargeRow => ({
  ...c,
  natureLabel: labelOf(FINANCIAL_NATURES, c.financialNatureCode),
  calculationLabel: labelOf(CALCULATION_TYPES, c.calculationTypeCode),
  defaultValue:
    c.calculationTypeCode === 'PERCENTAGE' && c.defaultRate != null ? `${c.defaultRate}%`
      : c.calculationTypeCode === 'FIXED' && c.defaultAmount != null ? `${c.defaultAmount.toLocaleString('en-US', { minimumFractionDigits: 2 })} ${c.currencyCode ?? ''}`.trim()
        : ''
})

const toBody = (v: FormValues, rowVersion: number | null) => ({ ...v, rowVersion })

const api: CrudApi<ChargeRow> = {
  list: async () => (await chargesApi.list()).map(toRow),
  create: async v => toRow(await chargesApi.create(toBody(v, null))),
  update: async (row, v) => toRow(await chargesApi.update(row.chargeId, toBody(v, row.rowVersion))),
  remove: row => chargesApi.remove(row.chargeId, row.rowVersion)
}

onMounted(async () => {
  const [types, curr] = await Promise.all([getLookupOptions('charge-type'), getCurrencies()])
  chargeTypes.value = types
  currencies.value = curr.filter(c => c.isActive).map(c => ({ value: c.currencyCode, label: `${c.currencyCode} - ${c.currencyNameEn}` }))
  buildFields()
})
</script>

<template>
  <CrudPage
    title="Charges"
    description="Maintain reusable charge components."
    icon="pi pi-wallet"
    apex-pages="FINOVA_FINANCEING"
    entity-label="Charge"
    :columns="columns"
    :fields="fields"
    :api="api"
    :row-key="row => row.chargeId"
    :new-values="() => ({ isActive: true, isRecurring: false, calculationTypeCode: 'FIXED', financialNatureCode: 'REVENUE' })"
    :to-values="row => ({ ...row })"
    :can-edit="hasPermission('ADM.PRODUCT.EDIT')"
  >
    <template #actions>
      <RouterLink to="/admin/reference-data/charge-type" class="fx-btn"><i class="pi pi-tags"></i><span>{{ tr('Charge Types') }}</span></RouterLink>
      <RouterLink to="/admin" class="fx-btn fx-btn--ghost"><i class="pi pi-arrow-left"></i><span>{{ tr('Admin Setup') }}</span></RouterLink>
    </template>
  </CrudPage>
</template>
