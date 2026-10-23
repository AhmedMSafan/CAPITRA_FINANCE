<script setup lang="ts">
// Project units (APEX App 105 pages 107/108), filtered by project.
import { computed, ref, shallowRef } from 'vue'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FieldOption } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { getLookupOptions } from '@/modules/adm/api/lookups.api'
import { getProjectOptions, projectUnitsApi, type ProjectUnit } from '../api/masterData.api'

const projects = shallowRef<FieldOption[]>([])
const unitTypes = shallowRef<FieldOption[]>([])
getProjectOptions().then(o => (projects.value = o)).catch(() => undefined)
getLookupOptions('unit-type').then(o => (unitTypes.value = o)).catch(() => undefined)

const projectFilter = ref<number | ''>('')

const api = computed<CrudApi<ProjectUnit>>(() => {
  const id = projectFilter.value
  return { ...projectUnitsApi, list: () => projectUnitsApi.list(id === '' ? '' : `?projectId=${id}`) }
})

const columns: CrudColumn<ProjectUnit>[] = [
  { field: 'unitCode', label: 'Unit Code', type: 'code' },
  { field: 'projectName', label: 'Project' },
  { field: 'unitTypeName', label: 'Unit Type' },
  { field: 'buildingNo', label: 'Building' },
  { field: 'floorNo', label: 'Floor' },
  { field: 'unitNo', label: 'Unit No' },
  { field: 'areaSqm', label: 'Area (m²)', type: 'number' },
  { field: 'contractAmount', label: 'Contract Amount', type: 'amount' },
  { field: 'marketValueAmount', label: 'Market Value', type: 'amount' },
  { field: 'licenseStatus', label: 'License Status' }
]

const fields = computed<CrudField[]>(() => [
  { name: 'projectId', label: 'Project', type: 'select', required: true, options: projects.value, wide: true },
  { name: 'unitCode', label: 'Unit Code', type: 'text', required: true, maxLength: 50, uppercase: true },
  { name: 'unitTypeCode', label: 'Unit Type', type: 'select', options: unitTypes.value },
  { name: 'buildingNo', label: 'Building No', type: 'text', maxLength: 50 },
  { name: 'floorNo', label: 'Floor No', type: 'text', maxLength: 50 },
  { name: 'unitNo', label: 'Unit No', type: 'text', maxLength: 50 },
  { name: 'areaSqm', label: 'Area (m²)', type: 'amount', min: 0 },
  { name: 'contractAmount', label: 'Contract Amount', type: 'amount', min: 0 },
  { name: 'marketValueAmount', label: 'Market Value', type: 'amount', min: 0 },
  { name: 'licenseStatus', label: 'License Status', type: 'text', maxLength: 50 }
])
</script>

<template>
  <CrudPage
    title="Project Units"
    description="Units in each project with their area, contract amount and market value."
    icon="pi pi-building"
    apex-pages="107/108"
    entity-label="Unit"
    :columns="columns"
    :fields="fields"
    :api="api"
    :row-key="row => row.projectUnitId"
    :new-values="() => ({ isActive: true, projectId: projectFilter })"
    :to-values="row => ({ ...row })"
    :can-edit="hasPermission('MTG.MASTER_DATA.EDIT')"
  >
    <template #filters>
      <select v-model="projectFilter" class="grid-select">
        <option value="">All Projects</option>
        <option v-for="o in projects" :key="o.value" :value="o.value">{{ o.label }}</option>
      </select>
    </template>
  </CrudPage>
</template>
