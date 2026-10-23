<script setup lang="ts">
// Developers (APEX App 105 pages 101/102).
import { computed, shallowRef } from 'vue'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudColumn, CrudField, FieldOption } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { getLookupOptions } from '@/modules/adm/api/lookups.api'
import { developersApi, type Developer } from '../api/masterData.api'

const riskLevels = shallowRef<FieldOption[]>([])
getLookupOptions('risk-level').then(o => (riskLevels.value = o)).catch(() => undefined)

const columns: CrudColumn<Developer>[] = [
  { field: 'developerCode', label: 'Code', type: 'code' },
  { field: 'legalNameEn', label: 'Legal Name (EN)' },
  { field: 'legalNameAr', label: 'Legal Name (AR)', rtl: true },
  { field: 'commercialRegNo', label: 'Commercial Reg. No' },
  { field: 'taxRegNo', label: 'Tax Reg. No' },
  { field: 'incorporationDate', label: 'Incorporation Date', type: 'date' },
  { field: 'phoneNo', label: 'Phone' },
  { field: 'emailAddress', label: 'Email' },
  { field: 'riskLevelName', label: 'Risk Level' }
]

const fields = computed<CrudField[]>(() => [
  { name: 'developerCode', label: 'Developer Code', type: 'text', required: true, maxLength: 30, uppercase: true },
  { name: 'riskLevelCode', label: 'Risk Level', type: 'select', options: riskLevels.value },
  { name: 'legalNameEn', label: 'Legal Name (EN)', type: 'text', maxLength: 300 },
  { name: 'legalNameAr', label: 'Legal Name (AR)', type: 'text', required: true, maxLength: 300, rtl: true },
  { name: 'commercialRegNo', label: 'Commercial Reg. No', type: 'text', maxLength: 100 },
  { name: 'taxRegNo', label: 'Tax Reg. No', type: 'text', maxLength: 100 },
  { name: 'incorporationDate', label: 'Incorporation Date', type: 'date' },
  { name: 'phoneNo', label: 'Phone', type: 'text', maxLength: 100 },
  { name: 'emailAddress', label: 'Email', type: 'text', maxLength: 320, wide: true },
  { name: 'addressText', label: 'Address', type: 'textarea', maxLength: 1000 }
])
</script>

<template>
  <CrudPage
    title="Developers"
    description="Real-estate developers whose units are financed in purchased portfolios."
    icon="pi pi-briefcase"
    apex-pages="101/102"
    entity-label="Developer"
    :columns="columns"
    :fields="fields"
    :api="developersApi"
    :row-key="row => row.developerId"
    :new-values="() => ({ isActive: true })"
    :to-values="row => ({ ...row })"
    :can-edit="hasPermission('MTG.MASTER_DATA.EDIT')"
  />
</template>
