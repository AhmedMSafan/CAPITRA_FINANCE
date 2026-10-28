<script setup lang="ts">
// Product Event Rules (ADM_PRODUCT_EVENT_RULES): FINOVA_FINANCEING ProductEventRulesPage, the rule list.
// Each rule's charges and GL entries open on the rule's own page.
import { onMounted, ref } from 'vue'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FieldOption, FormValues } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { tr } from '@/shared/layout/language'
import { todayInCairo } from '@/shared/format/datetime'
import { productEventRulesApi, productsApi, type ProductEventRule } from '../api/products.api'

const products = ref<FieldOption[]>([])
const eventTypes = ref<FieldOption[]>([])

const columns: CrudColumn<ProductEventRule>[] = [
  { field: 'ruleCode', label: 'Rule Code', type: 'code' },
  { field: 'ruleNameEn', label: 'Rule Name' },
  { field: 'productCode', label: 'Product', type: 'code' },
  { field: 'eventTypeCode', label: 'Accounting Event', type: 'code' },
  { field: 'priorityNo', label: 'Priority', type: 'number' },
  { field: 'chargeCount', label: 'Charges', type: 'number' },
  { field: 'glLineCount', label: 'GL Lines', type: 'number' },
  { field: 'effectiveFromDate', label: 'Effective From', type: 'date' },
  { field: 'effectiveToDate', label: 'Effective To', type: 'date' }
]

const fields = ref<CrudField[]>([])
const buildFields = () => {
  fields.value = [
    { name: 'productId', label: 'Product', type: 'select', required: true, options: products.value },
    { name: 'eventTypeId', label: 'Accounting Event', type: 'select', required: true, options: eventTypes.value },
    { name: 'ruleCode', label: 'Rule Code', type: 'text', required: true, maxLength: 50, uppercase: true },
    { name: 'priorityNo', label: 'Priority', type: 'number', required: true, min: 0 },
    { name: 'ruleNameEn', label: 'Rule Name (EN)', type: 'text', required: true, maxLength: 200 },
    { name: 'ruleNameAr', label: 'Rule Name (AR)', type: 'text', maxLength: 200, rtl: true },
    { name: 'effectiveFromDate', label: 'Effective From', type: 'date', required: true },
    { name: 'effectiveToDate', label: 'Effective To', type: 'date' }
  ]
}
buildFields()

// select values come back as strings; the API takes ids as numbers
const toBody = (v: FormValues, rowVersion: number | null) => ({
  ...v,
  productId: v.productId == null ? null : Number(v.productId),
  eventTypeId: v.eventTypeId == null ? null : Number(v.eventTypeId),
  rowVersion
})

const api: CrudApi<ProductEventRule> = {
  list: productEventRulesApi.list,
  create: v => productEventRulesApi.create(toBody(v, null)),
  update: (row, v) => productEventRulesApi.update(row.productEventRuleId, toBody(v, row.rowVersion)),
  remove: row => productEventRulesApi.remove(row.productEventRuleId, row.rowVersion)
}

onMounted(async () => {
  const [p, e] = await Promise.all([productsApi.list(), productEventRulesApi.eventTypes()])
  products.value = p.filter(x => x.isActive).map(x => ({ value: x.productId, label: `${x.productCode} - ${x.productNameEn}` }))
  eventTypes.value = e.map(x => ({ value: x.eventTypeId, label: `${x.eventTypeCode} - ${x.eventTypeNameEn}` }))
  buildFields()
})
</script>

<template>
  <CrudPage
    title="Product Event Rules"
    description="Define product events and processing rules."
    icon="pi pi-sitemap"
    apex-pages="FINOVA_FINANCEING"
    entity-label="Product Event Rule"
    :columns="columns"
    :fields="fields"
    :api="api"
    :row-key="row => row.productEventRuleId"
    :new-values="() => ({ isActive: true, priorityNo: 10, effectiveFromDate: todayInCairo() })"
    :to-values="row => ({ ...row })"
    :can-edit="hasPermission('ADM.PRODUCT.EDIT')"
  >
    <template #actions>
      <RouterLink to="/admin" class="fx-btn fx-btn--ghost"><i class="pi pi-arrow-left"></i><span>{{ tr('Admin Setup') }}</span></RouterLink>
    </template>
    <template #row-actions="{ row }">
      <RouterLink :to="`/admin/product-event-rules/${row.productEventRuleId}`" class="fx-btn fx-btn--sm pr-charges">
        <i class="pi pi-wallet"></i>{{ tr('Charges & GL') }}
      </RouterLink>
    </template>
  </CrudPage>
</template>

<style scoped>
.pr-charges { margin-inline-end: 6px; }
</style>
