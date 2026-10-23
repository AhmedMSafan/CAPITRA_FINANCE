<script setup lang="ts">
// Developer signatories (APEX App 105 pages 103/104), filtered by developer.
import { computed, ref, shallowRef } from 'vue'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FieldOption } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { developerSignatoriesApi, getDeveloperOptions, type DeveloperSignatory } from '../api/masterData.api'

const developers = shallowRef<FieldOption[]>([])
getDeveloperOptions().then(o => (developers.value = o)).catch(() => undefined)

const developerFilter = ref<number | ''>('')

const api = computed<CrudApi<DeveloperSignatory>>(() => {
  const id = developerFilter.value
  return { ...developerSignatoriesApi, list: () => developerSignatoriesApi.list(id === '' ? '' : `?developerId=${id}`) }
})

const columns: CrudColumn<DeveloperSignatory>[] = [
  { field: 'developerName', label: 'Developer' },
  { field: 'signatoryName', label: 'Signatory Name' },
  { field: 'nationalIdNo', label: 'National ID' },
  { field: 'jobTitle', label: 'Job Title' },
  { field: 'authorizationReference', label: 'Authorization Ref.' },
  { field: 'validFromDate', label: 'Valid From', type: 'date' },
  { field: 'validToDate', label: 'Valid To', type: 'date' }
]

const fields = computed<CrudField[]>(() => [
  { name: 'developerId', label: 'Developer', type: 'select', required: true, options: developers.value, wide: true },
  { name: 'signatoryName', label: 'Signatory Name', type: 'text', required: true, maxLength: 300 },
  { name: 'nationalIdNo', label: 'National ID', type: 'text', maxLength: 50 },
  { name: 'jobTitle', label: 'Job Title', type: 'text', maxLength: 200 },
  { name: 'authorizationReference', label: 'Authorization Reference', type: 'text', maxLength: 300 },
  { name: 'validFromDate', label: 'Valid From', type: 'date' },
  { name: 'validToDate', label: 'Valid To', type: 'date' }
])
</script>

<template>
  <CrudPage
    title="Developer Signatories"
    description="People authorised to sign contracts on behalf of each developer."
    icon="pi pi-id-card"
    apex-pages="103/104"
    entity-label="Signatory"
    :columns="columns"
    :fields="fields"
    :api="api"
    :row-key="row => row.developerSignatoryId"
    :new-values="() => ({ isActive: true, developerId: developerFilter })"
    :to-values="row => ({ ...row })"
    :can-edit="hasPermission('MTG.MASTER_DATA.EDIT')"
  >
    <template #filters>
      <select v-model="developerFilter" class="grid-select">
        <option value="">All Developers</option>
        <option v-for="o in developers" :key="o.value" :value="o.value">{{ o.label }}</option>
      </select>
    </template>
  </CrudPage>
</template>
