<script setup lang="ts">
// Investors (APEX App 105 pages 109/110).
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { investorsApi, type Investor } from '../api/masterData.api'

const api: CrudApi<Investor> = { ...investorsApi, list: () => investorsApi.list() }

const columns: CrudColumn<Investor>[] = [
  { field: 'investorCode', label: 'Code', type: 'code' },
  { field: 'fullNameEn', label: 'Full Name (EN)' },
  { field: 'fullNameAr', label: 'Full Name (AR)', rtl: true },
  { field: 'nationalIdNo', label: 'National ID' },
  { field: 'birthDate', label: 'Birth Date', type: 'date' },
  { field: 'phoneNo', label: 'Phone' },
  { field: 'emailAddress', label: 'Email' },
  { field: 'employerName', label: 'Employer' },
  { field: 'declaredIncomeAmount', label: 'Declared Income', type: 'amount' }
]

const fields: CrudField[] = [
  { name: 'investorCode', label: 'Investor Code', type: 'text', required: true, maxLength: 30, uppercase: true },
  { name: 'nationalIdNo', label: 'National ID', type: 'text', maxLength: 50 },
  { name: 'fullNameEn', label: 'Full Name (EN)', type: 'text', maxLength: 300 },
  { name: 'fullNameAr', label: 'Full Name (AR)', type: 'text', required: true, maxLength: 300, rtl: true },
  { name: 'birthDate', label: 'Birth Date', type: 'date' },
  { name: 'phoneNo', label: 'Phone', type: 'text', maxLength: 100 },
  { name: 'emailAddress', label: 'Email', type: 'text', maxLength: 320 },
  { name: 'employerName', label: 'Employer', type: 'text', maxLength: 300 },
  { name: 'declaredIncomeAmount', label: 'Declared Income', type: 'amount', min: 0 },
  { name: 'addressText', label: 'Address', type: 'textarea', maxLength: 1000 }
]
</script>

<template>
  <CrudPage
    title="Investors"
    description="Unit buyers: identity, contact details, employer and declared income."
    icon="pi pi-users"
    apex-pages="109/110"
    entity-label="Investor"
    :columns="columns"
    :fields="fields"
    :api="api"
    :row-key="row => row.investorId"
    :new-values="() => ({ isActive: true })"
    :to-values="row => ({ ...row })"
    :can-edit="hasPermission('MTG.MASTER_DATA.EDIT')"
  />
</template>
