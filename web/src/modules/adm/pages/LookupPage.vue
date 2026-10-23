<script setup lang="ts">
// Every reference-data list and form (APEX App 105 pages 11-40), drawn from the lookup's definition.
import { computed, ref, shallowRef, watch } from 'vue'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FieldOption, FormValues } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import {
  createLookupValue,
  deleteLookupValue,
  getLookupDefinitions,
  getLookupOptions,
  getLookupValues,
  updateLookupValue,
  type LookupDefinition,
  type LookupValue,
  type SaveLookupValue
} from '../api/lookups.api'

/** A lookup value with its extra columns flattened next to the common ones, so the grid can show them. */
type LookupRow = LookupValue & Record<string, unknown>

const props = defineProps<{ lookupName: string }>()

const definition = shallowRef<LookupDefinition | null>(null)
const options = shallowRef<Record<string, FieldOption[]>>({})
const loadError = ref('')
const canEdit = hasPermission('ADM.LOOKUP.EDIT')

const LABEL_SUFFIX = '__label'

watch(
  () => props.lookupName,
  async name => {
    definition.value = null
    loadError.value = ''
    try {
      const found = (await getLookupDefinitions()).find(d => d.name === name)
      if (!found) {
        loadError.value = `Unknown reference data list "${name}".`
        return
      }
      const lookups = found.fields.filter(f => f.type === 'LOOKUP' && f.lookupName)
      const loaded = await Promise.all(lookups.map(f => getLookupOptions(f.lookupName as string)))
      options.value = Object.fromEntries(lookups.map((f, i) => [f.name, loaded[i] ?? []]))
      definition.value = found
    } catch (error) {
      loadError.value = error instanceof Error ? error.message : 'Unable to load the reference data list.'
    }
  },
  { immediate: true }
)

const columns = computed<CrudColumn<LookupRow>[]>(() => {
  const d = definition.value
  if (!d) return []
  const cols: CrudColumn<LookupRow>[] = [
    { field: 'code', label: 'Code', type: 'code' },
    { field: 'nameEn', label: 'Name (EN)' },
    { field: 'nameAr', label: 'Name (AR)', rtl: true }
  ]
  for (const f of d.fields) {
    if (f.type === 'LOOKUP') cols.push({ field: f.name + LABEL_SUFFIX, label: f.label })
    else cols.push({ field: f.name, label: f.label, type: f.type === 'NUMBER' ? 'number' : f.type === 'BOOLEAN' ? 'bool' : 'text' })
  }
  if (d.showDescription) cols.push({ field: 'description', label: 'Description' })
  if (d.showDisplayOrder) cols.push({ field: 'displayOrder', label: d.displayOrderLabel, type: 'number' })
  return cols
})

const fields = computed<CrudField[]>(() => {
  const d = definition.value
  if (!d) return []
  const list: CrudField[] = [
    { name: 'code', label: 'Code', type: 'text', required: true, maxLength: d.codeMaxLength, lockedOnEdit: true, uppercase: true },
    { name: 'nameEn', label: 'Name (EN)', type: 'text', required: true, maxLength: d.nameMaxLength },
    { name: 'nameAr', label: 'Name (AR)', type: 'text', required: true, maxLength: d.nameMaxLength, rtl: true }
  ]
  if (d.showDisplayOrder)
    list.push({ name: 'displayOrder', label: d.displayOrderLabel, type: 'number', required: d.displayOrderRequired, min: 0 })
  for (const f of d.fields) {
    const base = { name: f.name, label: f.label, required: f.required }
    if (f.type === 'LOOKUP') list.push({ ...base, type: 'select', options: options.value[f.name] ?? [] })
    else if (f.type === 'BOOLEAN') list.push({ ...base, type: 'bool' })
    else if (f.type === 'NUMBER') list.push({ ...base, type: 'number', min: f.min ?? undefined, max: f.max ?? undefined })
    else list.push({ ...base, type: 'text', maxLength: f.maxLength ?? undefined })
  }
  if (d.showDescription) list.push({ name: 'description', label: 'Description', type: 'textarea', maxLength: 1000 })
  return list
})

function labelOf(fieldName: string, code: unknown): string {
  if (code === null || code === undefined || code === '') return ''
  return options.value[fieldName]?.find(o => o.value === code)?.label ?? String(code)
}

function toRow(value: LookupValue): LookupRow {
  const row: LookupRow = { ...value, ...value.extras }
  for (const f of definition.value?.fields ?? [])
    if (f.type === 'LOOKUP') row[f.name + LABEL_SUFFIX] = labelOf(f.name, value.extras[f.name])
  return row
}

function toRequest(values: FormValues, rowVersion: number | null): SaveLookupValue {
  const extras: Record<string, unknown> = {}
  for (const f of definition.value?.fields ?? []) extras[f.name] = values[f.name] ?? null
  return {
    code: String(values.code ?? ''),
    nameAr: String(values.nameAr ?? ''),
    nameEn: String(values.nameEn ?? ''),
    description: (values.description as string | null) ?? null,
    displayOrder: (values.displayOrder as number | null) ?? 0,
    isActive: Boolean(values.isActive),
    rowVersion,
    extras
  }
}

const api = computed<CrudApi<LookupRow>>(() => {
  const name = props.lookupName
  return {
    list: async () => (await getLookupValues(name, true)).map(toRow),
    create: async values => toRow(await createLookupValue(name, toRequest(values, null))),
    update: async (row, values) => toRow(await updateLookupValue(name, row.code, toRequest(values, row.rowVersion))),
    remove: row => deleteLookupValue(name, row.code, row.rowVersion)
  }
})

/** "Currencies" -> "Currency", "Payment Statuses" -> "Payment Status", "Portfolio Status" stays. */
function singular(title: string): string {
  if (title.endsWith('ies')) return title.slice(0, -3) + 'y'
  if (title.endsWith('uses')) return title.slice(0, -2)
  if (title.endsWith('s') && !title.endsWith('us') && !title.endsWith('ss')) return title.slice(0, -1)
  return title
}

function newValues(): FormValues {
  const values: FormValues = { isActive: true }
  for (const f of definition.value?.fields ?? []) if (f.type === 'BOOLEAN') values[f.name] = false
  return values
}

function toValues(row: LookupRow): FormValues {
  return { ...row, displayOrder: definition.value?.showDisplayOrder ? row.displayOrder : '' }
}
</script>

<template>
  <div v-if="loadError" class="lookup-error">{{ loadError }}</div>
  <CrudPage
    v-else-if="definition"
    :key="definition.name"
    :title="definition.title"
    :description="`Maintain the ${definition.title.toLowerCase()} used across CAPITRA.`"
    icon="pi pi-list"
    :apex-pages="definition.apexPages"
    :entity-label="singular(definition.title)"
    :columns="columns"
    :fields="fields"
    :api="api"
    :row-key="row => row.code"
    :new-values="newValues"
    :to-values="toValues"
    :can-edit="canEdit"
  >
    <template #actions>
      <RouterLink to="/mortgage/reference-data" class="back-link"><i class="pi pi-arrow-left"></i><span>Reference Data</span></RouterLink>
    </template>
  </CrudPage>
</template>

<style scoped>
.lookup-error { margin: 24px 30px; padding: 12px 16px; border: 1px solid #f2c4c4; border-radius: 8px; background: #fff3f3; color: #b42318; font-size: 13px; }
.back-link { height: 40px; display: inline-flex; align-items: center; gap: 8px; padding: 0 12px; color: #405b4d; font-size: 13px; text-decoration: none; }
.back-link:hover { color: #0a3573; }
</style>
