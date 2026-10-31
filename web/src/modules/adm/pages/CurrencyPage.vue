<script setup lang="ts">
// Currencies (ADM_LKP_CURRENCIES, APEX App 105 pages 37/38) in the shared list layout.
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FormValues } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { tr } from '@/shared/layout/language'
import { createCurrency, deleteCurrency, getCurrencies, updateCurrency, type Currency, type SaveCurrency } from '../api/currencies.api'

const columns: CrudColumn<Currency>[] = [
  { field: 'currencyCode', label: 'Code', type: 'code' },
  { field: 'currencyNameEn', label: 'Name (EN)' },
  { field: 'currencyNameAr', label: 'Name (AR)', rtl: true },
  { field: 'decimalPlaces', label: 'Decimal Places', type: 'number' }
]

const fields: CrudField[] = [
  { name: 'currencyCode', label: 'Code', type: 'text', required: true, maxLength: 3, lockedOnEdit: true, uppercase: true },
  { name: 'decimalPlaces', label: 'Decimal Places', type: 'number', required: true, min: 0, max: 6 },
  { name: 'currencyNameEn', label: 'Name (EN)', type: 'text', required: true, maxLength: 100 },
  { name: 'currencyNameAr', label: 'Name (AR)', type: 'text', required: true, maxLength: 100, rtl: true }
]

const toRequest = (v: FormValues, rowVersion: number | null): SaveCurrency => ({
  currencyCode: String(v.currencyCode ?? ''),
  currencyNameEn: String(v.currencyNameEn ?? ''),
  currencyNameAr: String(v.currencyNameAr ?? ''),
  decimalPlaces: Number(v.decimalPlaces ?? 2),
  isActive: Boolean(v.isActive),
  rowVersion
})

const api: CrudApi<Currency> = {
  list: getCurrencies,
  create: v => createCurrency(toRequest(v, null)),
  update: (row, v) => updateCurrency(row.currencyCode, toRequest(v, row.rowVersion)),
  remove: row => deleteCurrency(row.currencyCode, row.rowVersion)
}
</script>

<template>
  <CrudPage
    title="Currencies"
    description="Maintain supported currencies and currency information."
    icon="pi pi-money-bill"
    apex-pages="37/38"
    entity-label="Currency"
    :columns="columns"
    :fields="fields"
    :api="api"
    :row-key="row => row.currencyCode"
    :new-values="() => ({ isActive: true, decimalPlaces: 2 })"
    :to-values="row => ({ ...row })"
    :can-edit="hasPermission('ADM.LOOKUP.EDIT')"
  >
    <template #actions>
      <RouterLink to="/admin/reference-data" class="fx-btn fx-btn--ghost"><i class="pi pi-arrow-left"></i><span>{{ tr('Reference Data') }}</span></RouterLink>
    </template>
  </CrudPage>
</template>
