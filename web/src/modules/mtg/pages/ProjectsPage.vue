<script setup lang="ts">
// Projects (APEX App 105 pages 105/106).
import { computed, ref, shallowRef } from 'vue'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FieldOption } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { getDeveloperOptions, projectsApi, type Project } from '../api/masterData.api'

const developers = shallowRef<FieldOption[]>([])
getDeveloperOptions().then(o => (developers.value = o)).catch(() => undefined)

const developerFilter = ref<number | ''>('')

const api = computed<CrudApi<Project>>(() => {
  const id = developerFilter.value
  return { ...projectsApi, list: () => projectsApi.list(id === '' ? '' : `?developerId=${id}`) }
})

const columns: CrudColumn<Project>[] = [
  { field: 'projectCode', label: 'Code', type: 'code' },
  { field: 'projectNameEn', label: 'Project Name (EN)' },
  { field: 'projectNameAr', label: 'Project Name (AR)', rtl: true },
  { field: 'developerName', label: 'Developer' },
  { field: 'locationText', label: 'Location' },
  { field: 'landArea', label: 'Land Area', type: 'number' },
  { field: 'titleDeedNo', label: 'Title Deed No' },
  { field: 'buildingLicenseNo', label: 'Building License No' },
  { field: 'buildingLicenseDate', label: 'License Date', type: 'date' },
  { field: 'projectCompletionPct', label: 'Completion', type: 'pct' },
  { field: 'projectStatus', label: 'Project Status' }
]

const fields = computed<CrudField[]>(() => [
  { name: 'developerId', label: 'Developer', type: 'select', required: true, options: developers.value, wide: true },
  { name: 'projectCode', label: 'Project Code', type: 'text', required: true, maxLength: 30, uppercase: true },
  { name: 'projectStatus', label: 'Project Status', type: 'text', maxLength: 50 },
  { name: 'projectNameEn', label: 'Project Name (EN)', type: 'text', maxLength: 300 },
  { name: 'projectNameAr', label: 'Project Name (AR)', type: 'text', required: true, maxLength: 300, rtl: true },
  { name: 'landArea', label: 'Land Area', type: 'amount', min: 0 },
  { name: 'projectCompletionPct', label: 'Completion %', type: 'pct', min: 0, max: 100 },
  { name: 'titleDeedNo', label: 'Title Deed No', type: 'text', maxLength: 100 },
  { name: 'buildingLicenseNo', label: 'Building License No', type: 'text', maxLength: 100 },
  { name: 'buildingLicenseDate', label: 'Building License Date', type: 'date' },
  { name: 'locationText', label: 'Location', type: 'textarea', maxLength: 1000 }
])
</script>

<template>
  <CrudPage
    title="Projects"
    description="Developer projects: location, title deed, building licence and completion."
    icon="pi pi-map"
    apex-pages="105/106"
    entity-label="Project"
    :columns="columns"
    :fields="fields"
    :api="api"
    :row-key="row => row.projectId"
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
