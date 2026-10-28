<script setup lang="ts">
// Product Catalog (ADM_PRODUCTS): FINOVA_FINANCEING ProductsPage in the shared list layout.
import { onMounted, ref } from 'vue'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FieldOption, FormValues } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { tr } from '@/shared/layout/language'
import { todayInCairo } from '@/shared/format/datetime'
import { getLookupOptions } from '../api/lookups.api'
import { getCurrencies } from '../api/currencies.api'
import { productsApi, type Product } from '../api/products.api'

type ProductRow = Product & { recourseLabel: string }

const RECOURSE: FieldOption[] = [
  { value: 'Y', label: 'With recourse' },
  { value: 'N', label: 'Without recourse' }
]

const businessLines = ref<FieldOption[]>([])
const currencies = ref<FieldOption[]>([])

const columns: CrudColumn<ProductRow>[] = [
  { field: 'productCode', label: 'Code', type: 'code' },
  { field: 'productNameEn', label: 'Name' },
  { field: 'businessLineCode', label: 'Business Line', type: 'code' },
  { field: 'productTypeCode', label: 'Product Type', type: 'code' },
  { field: 'defaultCurrencyCode', label: 'Currency', type: 'code' },
  { field: 'effectiveFromDate', label: 'Effective From', type: 'date' },
  { field: 'effectiveToDate', label: 'Effective To', type: 'date' }
]

const fields = ref<CrudField[]>([])
const buildFields = () => {
  fields.value = [
    { name: 'productCode', label: 'Product Code', type: 'text', required: true, maxLength: 50, uppercase: true },
    { name: 'businessLineCode', label: 'Business Line', type: 'select', required: true, options: businessLines.value },
    { name: 'productNameEn', label: 'Name (EN)', type: 'text', required: true, maxLength: 200 },
    { name: 'productNameAr', label: 'Name (AR)', type: 'text', maxLength: 200, rtl: true },
    { name: 'productTypeCode', label: 'Product Type Code', type: 'text', required: true, maxLength: 30, uppercase: true },
    { name: 'defaultCurrencyCode', label: 'Default Currency', type: 'select', options: currencies.value },
    { name: 'factoringModeCode', label: 'Factoring Mode Code', type: 'text', maxLength: 20, uppercase: true },
    { name: 'recourse', label: 'Recourse', type: 'select', options: RECOURSE },
    { name: 'effectiveFromDate', label: 'Effective From', type: 'date', required: true },
    { name: 'effectiveToDate', label: 'Effective To', type: 'date' }
  ]
}
buildFields()

const toRow = (p: Product): ProductRow => ({ ...p, recourseLabel: p.isRecourse == null ? '' : p.isRecourse ? 'Y' : 'N' })

const toBody = (v: FormValues, rowVersion: number | null) => {
  const { recourse, ...rest } = v
  return { ...rest, isRecourse: recourse === 'Y' ? true : recourse === 'N' ? false : null, rowVersion }
}

const api: CrudApi<ProductRow> = {
  list: async () => (await productsApi.list()).map(toRow),
  create: async v => toRow(await productsApi.create(toBody(v, null))),
  update: async (row, v) => toRow(await productsApi.update(row.productId, toBody(v, row.rowVersion))),
  remove: row => productsApi.remove(row.productId, row.rowVersion)
}

onMounted(async () => {
  const [lines, curr] = await Promise.all([getLookupOptions('business-line'), getCurrencies()])
  businessLines.value = lines
  currencies.value = curr.filter(c => c.isActive).map(c => ({ value: c.currencyCode, label: `${c.currencyCode} - ${c.currencyNameEn}` }))
  buildFields()
})
</script>

<template>
  <CrudPage
    title="Product Catalog"
    description="Maintain product catalog and product definitions."
    icon="pi pi-box"
    apex-pages="FINOVA_FINANCEING"
    entity-label="Product"
    :columns="columns"
    :fields="fields"
    :api="api"
    :row-key="row => row.productId"
    :new-values="() => ({ isActive: true, effectiveFromDate: todayInCairo() })"
    :to-values="row => ({ ...row, recourse: row.recourseLabel })"
    :can-edit="hasPermission('ADM.PRODUCT.EDIT')"
  >
    <template #actions>
      <RouterLink to="/admin/products/mortgage-portfolio-purchasing" class="fx-btn"><i class="pi pi-sliders-h"></i><span>{{ tr('Portfolio Purchasing Parameters') }}</span></RouterLink>
      <RouterLink to="/admin" class="fx-btn fx-btn--ghost"><i class="pi pi-arrow-left"></i><span>{{ tr('Admin Setup') }}</span></RouterLink>
    </template>
  </CrudPage>
</template>
