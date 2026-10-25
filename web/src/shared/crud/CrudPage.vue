<script setup lang="ts" generic="T extends { rowVersion: number; isActive?: boolean }">
// One list + one modal form: the Vue version of an APEX interactive report page and its modal form page.
import { computed, reactive, ref, shallowRef, watch } from 'vue'
import { ApiError } from '@/shared/api/http'
import { showSuccess, showWarning } from '@/shared/feedback/feedback'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { downloadCsv, formatCell, toCsv } from './format'
import type { CrudApi, CrudColumn, CrudField, FormValues } from './types'

const props = withDefaults(
  defineProps<{
    title: string
    description: string
    icon: string
    /** APEX pages this screen replaces, e.g. "101/102". */
    apexPages: string
    entityLabel: string
    columns: CrudColumn<T>[]
    fields: CrudField[]
    api: CrudApi<T>
    rowKey: (row: T) => string | number
    newValues: () => FormValues
    toValues: (row: T) => FormValues
    canEdit?: boolean
    /** False for tables without an IS_ACTIVE column (transactions): no status column, filter or checkbox. */
    hasActive?: boolean
    /** Drawn inside another page (a tab): no outer padding. */
    embedded?: boolean
    /** APEX application of the replaced pages. */
    apexApp?: number
  }>(),
  { canEdit: false, hasActive: true, embedded: false, apexApp: 105 }
)

const rows = shallowRef<T[]>([])
const loading = ref(false)
const saving = ref(false)
const pageError = ref('')
const formError = ref('')
const fieldErrors = reactive<Record<string, string>>({})
const searchText = ref('')
const statusFilter = ref<'' | 'Y' | 'N'>('')

const dialogVisible = ref(false)
const editingRow = shallowRef<T | null>(null)
const confirmDelete = ref(false)
const form = reactive<FormValues>({})

const filteredRows = computed(() => {
  const q = searchText.value.trim().toLowerCase()

  return rows.value.filter(row => {
    if (props.hasActive && statusFilter.value === 'Y' && !row.isActive) return false
    if (props.hasActive && statusFilter.value === 'N' && row.isActive) return false
    if (!q) return true
    return props.columns.some(c => formatCell(row[c.field], c.type).toLowerCase().includes(q))
  })
})

async function load() {
  loading.value = true
  pageError.value = ''
  try {
    rows.value = await props.api.list()
  } catch (error) {
    pageError.value = messageOf(error, `Unable to load ${props.title.toLowerCase()}.`)
  } finally {
    loading.value = false
  }
}

function clearErrors() {
  formError.value = ''
  Object.keys(fieldErrors).forEach(k => delete fieldErrors[k])
}

function fill(values: FormValues) {
  Object.keys(form).forEach(k => delete form[k])
  for (const field of props.fields) form[field.name] = values[field.name] ?? (field.type === 'bool' ? false : '')
  if (props.hasActive) form.isActive = values.isActive ?? true
}

function openCreate() {
  editingRow.value = null
  confirmDelete.value = false
  clearErrors()
  fill(props.newValues())
  dialogVisible.value = true
}

function openEdit(row: T) {
  editingRow.value = row
  confirmDelete.value = false
  clearErrors()
  fill(props.toValues(row))
  dialogVisible.value = true
}

function closeDialog() {
  if (!saving.value) dialogVisible.value = false
}

/** Converts the form to the request body: blanks become null, numbers become numbers. */
function body(): FormValues {
  const values: FormValues = props.hasActive ? { isActive: form.isActive } : {}
  for (const field of props.fields) {
    const raw = form[field.name]
    if (field.type === 'bool') values[field.name] = Boolean(raw)
    else if (raw === '' || raw === null || raw === undefined) values[field.name] = null
    else if (['number', 'amount', 'pct'].includes(field.type)) values[field.name] = Number(raw)
    else if (typeof raw === 'string') values[field.name] = field.uppercase ? raw.trim().toUpperCase() : raw.trim()
    else values[field.name] = raw
  }
  return values
}

function checkRequired(): boolean {
  for (const field of props.fields) {
    const raw = form[field.name]
    if (field.required && field.type !== 'bool' && (raw === '' || raw === null || raw === undefined))
      fieldErrors[field.name] = `${field.label} is required.`
  }
  return Object.keys(fieldErrors).length === 0
}

async function save() {
  clearErrors()
  if (!checkRequired()) {
    showWarning('Fill in the required fields marked in red.')
    return
  }

  saving.value = true
  try {
    const created = !editingRow.value
    if (editingRow.value) await props.api.update(editingRow.value, body())
    else await props.api.create(body())
    dialogVisible.value = false
    showSuccess(`${props.entityLabel} ${created ? 'added' : 'saved'}.`)
    await load()
  } catch (error) {
    showError(error)
  } finally {
    saving.value = false
  }
}

async function remove() {
  if (!editingRow.value) return
  if (!confirmDelete.value) {
    confirmDelete.value = true
    return
  }

  clearErrors()
  saving.value = true
  try {
    await props.api.remove(editingRow.value)
    dialogVisible.value = false
    showSuccess(`${props.entityLabel} deleted.`)
    await load()
  } catch (error) {
    confirmDelete.value = false
    showError(error)
  } finally {
    saving.value = false
  }
}

function showError(error: unknown) {
  if (error instanceof ApiError && error.problem?.errors) {
    const names = props.fields.map(f => f.name)
    for (const [key, messages] of Object.entries(error.problem.errors)) {
      const name = names.find(n => n.toLowerCase() === key.toLowerCase())
      if (name) fieldErrors[name] = messages.join(' ')
      else formError.value = [formError.value, ...messages].filter(Boolean).join(' ')
    }
    if (!formError.value && Object.keys(fieldErrors).length === 0) formError.value = error.message
    return
  }
  formError.value = messageOf(error, `Unable to save the ${props.entityLabel.toLowerCase()}.`)
}

/** A saved value that is no longer offered (made inactive) stays visible instead of turning blank. */
function isMissingOption(field: CrudField): boolean {
  const value = form[field.name]
  return value !== '' && value !== null && value !== undefined && !(field.options ?? []).some(o => o.value === value)
}

function messageOf(error: unknown, fallback: string) {
  return error instanceof Error ? error.message : fallback
}

function exportCsv() {
  const headers = [...props.columns.map(c => c.label), ...(props.hasActive ? ['Status'] : [])]
  const data = filteredRows.value.map(row => [
    ...props.columns.map(c => formatCell(row[c.field], c.type)),
    ...(props.hasActive ? [row.isActive ? 'Active' : 'Inactive'] : [])
  ])
  downloadCsv(`${props.title.replace(/\s+/g, '-').toLowerCase()}.csv`, toCsv(headers, data))
}

defineExpose({ reload: load })

watch(() => props.api, load, { immediate: true })

// Errors and confirmations open the app-wide pop-up; field errors stay beside their fields.
useFeedback({ errors: [pageError, formError] })
</script>

<template>
  <div class="crud-page" :class="{ 'crud-page--embedded': embedded }">
    <div class="crud-card">
      <div class="crud-toolbar">
        <div class="crud-header">
          <div class="crud-header-icon"><i :class="icon"></i></div>
          <div>
            <h2>{{ title }}</h2>
            <p>{{ description }}</p>
          </div>
        </div>

        <div class="crud-actions">
          <slot name="actions"></slot>
          <button type="button" class="secondary-button" title="Download as CSV" @click="exportCsv">
            <i class="pi pi-download"></i><span>Export</span>
          </button>
          <button v-if="canEdit" type="button" class="add-button" @click="openCreate">
            <i class="pi pi-plus"></i><span>Add</span>
          </button>
        </div>
      </div>


      <div class="grid-filters">
        <div class="grid-search">
          <i class="pi pi-search"></i>
          <input v-model="searchText" type="text" placeholder="Search..." />
        </div>
        <slot name="filters"></slot>
        <select v-if="hasActive" v-model="statusFilter" class="grid-select">
          <option value="">All Statuses</option>
          <option value="Y">Active</option>
          <option value="N">Inactive</option>
        </select>
        <div class="grid-meta">{{ filteredRows.length }} record(s)</div>
      </div>

      <div class="grid-panel">
        <div class="grid-panel__title">
          <i :class="icon"></i><span>{{ title }}</span>
        </div>

        <div v-if="loading" class="grid-loading">
          <i class="pi pi-spin pi-spinner"></i><span>Loading...</span>
        </div>

        <div v-else class="grid-table-wrap">
          <table class="grid-table">
            <thead>
              <tr>
                <th v-for="c in columns" :key="c.field" :class="{ num: ['number', 'amount', 'pct'].includes(c.type ?? '') }">
                  {{ c.label }}
                </th>
                <th v-if="hasActive">Status</th>
                <th class="actions-col">{{ canEdit ? 'Edit' : 'View' }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in filteredRows" :key="rowKey(row)">
                <td
                  v-for="c in columns"
                  :key="c.field"
                  :dir="c.rtl ? 'rtl' : undefined"
                  :class="{ 'code-cell': c.type === 'code', num: ['number', 'amount', 'pct'].includes(c.type ?? '') }"
                >
                  {{ formatCell(row[c.field], c.type) }}
                </td>
                <td v-if="hasActive">
                  <span class="status-pill" :class="row.isActive ? 'status-pill--active' : 'status-pill--inactive'">
                    {{ row.isActive ? 'Active' : 'Inactive' }}
                  </span>
                </td>
                <td class="actions-col">
                  <button type="button" class="edit-button" :title="canEdit ? 'Edit' : 'View'" @click="openEdit(row)">
                    <i :class="canEdit ? 'pi pi-pencil' : 'pi pi-eye'"></i>
                  </button>
                </td>
              </tr>
              <tr v-if="filteredRows.length === 0">
                <td :colspan="columns.length + (hasActive ? 2 : 1)" class="empty-row">No records found.</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <p v-if="apexPages" class="apex-ref">Replaces APEX App {{ apexApp }} pages {{ apexPages }}</p>
    </div>

    <div v-if="dialogVisible" class="modal-overlay" @click.self="closeDialog">
      <section class="edit-modal" role="dialog" aria-modal="true">
        <header class="edit-modal__header">
          <div>
            <h3>{{ editingRow ? (canEdit ? `Edit ${entityLabel}` : entityLabel) : `Add ${entityLabel}` }}</h3>
            <p>{{ editingRow ? `Version ${editingRow.rowVersion}` : `Create a new ${entityLabel.toLowerCase()}.` }}</p>
          </div>
          <button type="button" class="close-button" :disabled="saving" @click="closeDialog">
            <i class="pi pi-times"></i>
          </button>
        </header>


        <div class="edit-modal__body">
          <fieldset class="form-grid" :disabled="!canEdit || saving">
            <div v-for="field in fields" :key="field.name" class="field" :class="{ 'field--wide': field.wide || field.type === 'textarea' }">
              <label v-if="field.type !== 'bool'" :for="`f-${field.name}`">
                {{ field.label }}<span v-if="field.required"> *</span>
              </label>

              <select
                v-if="field.type === 'select'"
                :id="`f-${field.name}`"
                v-model="form[field.name]"
                :disabled="field.lockedOnEdit && !!editingRow"
              >
                <option value="">- Select -</option>
                <option v-if="isMissingOption(field)" :value="form[field.name]">{{ form[field.name] }} (inactive)</option>
                <option v-for="o in field.options ?? []" :key="o.value" :value="o.value">{{ o.label }}</option>
              </select>

              <textarea
                v-else-if="field.type === 'textarea'"
                :id="`f-${field.name}`"
                v-model="form[field.name] as string"
                :maxlength="field.maxLength"
                :dir="field.rtl ? 'rtl' : undefined"
                rows="3"
              ></textarea>

              <label v-else-if="field.type === 'bool'" class="check">
                <input :id="`f-${field.name}`" v-model="form[field.name]" type="checkbox" />
                <span>{{ field.label }}</span>
              </label>

              <input
                v-else
                :id="`f-${field.name}`"
                v-model="form[field.name]"
                :type="field.type === 'date' ? 'date' : ['number', 'amount', 'pct'].includes(field.type) ? 'number' : 'text'"
                :step="field.type === 'amount' ? '0.01' : field.type === 'pct' ? '0.000001' : undefined"
                :min="field.min"
                :max="field.max"
                :maxlength="field.maxLength"
                :disabled="field.lockedOnEdit && !!editingRow"
                :dir="field.rtl ? 'rtl' : undefined"
                :class="{ upper: field.uppercase }"
              />

              <small v-if="fieldErrors[field.name]" class="field-error">{{ fieldErrors[field.name] }}</small>
            </div>

            <div v-if="hasActive" class="field">
              <label class="check">
                <input v-model="form.isActive" type="checkbox" />
                <span>Active</span>
              </label>
            </div>
          </fieldset>
        </div>

        <footer class="edit-modal__footer">
          <button
            v-if="canEdit && editingRow"
            type="button"
            class="delete-button"
            :disabled="saving"
            @click="remove"
          >
            <i class="pi pi-trash"></i>
            <span>{{ confirmDelete ? 'Click again to delete' : 'Delete' }}</span>
          </button>
          <span class="spacer"></span>
          <button type="button" class="cancel-button" :disabled="saving" @click="closeDialog">
            {{ canEdit ? 'Cancel' : 'Close' }}
          </button>
          <button v-if="canEdit" type="button" class="save-button" :class="{ 'save-button--create': !editingRow }" :disabled="saving" @click="save">
            <i :class="saving ? 'pi pi-spin pi-spinner' : 'pi pi-check'"></i>
            <span>{{ saving ? 'Saving...' : editingRow ? 'Save Changes' : 'Create' }}</span>
          </button>
        </footer>
      </section>
    </div>
  </div>
</template>

<style scoped>
.crud-page { padding: 24px 30px 36px; font-family: Inter, "Segoe UI", Arial, sans-serif; }
.crud-page--embedded { padding: 0; }
.crud-card { background: #fff; border: 1px solid var(--cap-border-soft); border-radius: 16px; padding: 20px 28px 22px; box-shadow: 0 8px 24px rgba(13, 42, 46, .06); }
.crud-toolbar { display: flex; justify-content: space-between; align-items: center; gap: 20px; margin-bottom: 18px; }
.crud-header { display: flex; align-items: center; gap: 15px; }
.crud-header-icon { width: 54px; height: 54px; flex: 0 0 54px; display: grid; place-items: center; border-radius: 14px; background: var(--cap-warning-bg); color: var(--cap-warning); font-size: 22px; }
.crud-header h2 { margin: 0; color: var(--cap-heading); font-size: 20px; font-weight: 600; }
.crud-header p { margin: 5px 0 0; color: var(--cap-muted); font-size: 13px; }
.crud-actions { display: flex; gap: 8px; align-items: center; }
.add-button, .secondary-button { height: 40px; display: inline-flex; align-items: center; gap: 8px; padding: 0 15px; border-radius: 7px; font: inherit; font-size: 14px; font-weight: 500; cursor: pointer; }
.add-button { border: 0; background: var(--cap-create); color: #fff; font-weight: 600; }
.add-button:hover { background: var(--cap-create-hover); }
.secondary-button { border: 1px solid var(--cap-border); background: #fff; color: var(--cap-ink-soft); }
.secondary-button:hover { background: var(--cap-bg); }
.error-message { margin-bottom: 16px; padding: 10px 14px; border: 1px solid var(--cap-danger-border); border-radius: 8px; background: var(--cap-danger-bg); color: var(--cap-danger); font-size: 12px; }
.grid-filters { display: flex; flex-wrap: wrap; gap: 12px; align-items: center; margin-bottom: 14px; }
.grid-search { flex: 1 1 280px; height: 42px; display: flex; align-items: center; gap: 9px; padding: 0 13px; border: 1px solid var(--cap-border); border-radius: 8px; background: #fff; }
.grid-search i { color: var(--cap-muted); font-size: 12px; }
.grid-search input { width: 100%; border: 0; outline: 0; background: transparent; color: var(--cap-ink); font: inherit; font-size: 12px; }
.grid-select, :slotted(.grid-select) { height: 42px; min-width: 180px; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 8px; background: #fff; color: var(--cap-ink); font: inherit; font-size: 12px; }
.grid-meta { color: var(--cap-muted); font-size: 11px; white-space: nowrap; }
.grid-panel { overflow: hidden; border: 1px solid var(--cap-border-soft); border-radius: 12px; background: #fff; }
.grid-panel__title { min-height: 42px; display: flex; align-items: center; gap: 9px; padding: 0 16px; border-bottom: 2px solid var(--cap-orange-border); background: var(--cap-orange-tint); color: var(--cap-orange-dark); font-size: 13px; font-weight: 500; }
.grid-table-wrap { width: 100%; overflow-x: auto; }
.grid-table { width: 100%; border-collapse: collapse; }
.grid-table th, .grid-table td { padding: 11px 14px; border-bottom: 1px solid var(--cap-border-soft); text-align: left; vertical-align: middle; color: var(--cap-ink-soft); font-size: 12px; white-space: nowrap; }
.grid-table th { background: var(--cap-bg); color: var(--cap-ink); font-weight: 500; }
.grid-table th.num, .grid-table td.num { text-align: right; }
.grid-table tbody tr:last-child td { border-bottom: 0; }
.grid-table tbody tr:hover { background: var(--cap-bg); }
.code-cell { color: var(--cap-heading) !important; font-weight: 600; }
.actions-col { width: 70px; text-align: center !important; }
.status-pill { min-width: 62px; display: inline-flex; justify-content: center; padding: 4px 9px; border-radius: 6px; font-size: 10px; font-weight: 500; }
.status-pill--active { background: var(--cap-success-bg); border: 1px solid var(--cap-success-border); color: var(--cap-success); }
.status-pill--inactive { background: var(--cap-danger-bg); border: 1px solid var(--cap-danger-border); color: var(--cap-danger); }
.edit-button { width: 36px; height: 32px; display: inline-grid; place-items: center; border: 1px solid var(--cap-primary-tint-2); border-radius: 7px; background: var(--cap-primary-tint); color: var(--cap-primary); cursor: pointer; }
.edit-button:hover { background: var(--cap-primary-tint-2); }
.empty-row { height: 100px; text-align: center !important; color: var(--cap-muted) !important; }
.grid-loading { min-height: 120px; display: flex; align-items: center; justify-content: center; gap: 9px; color: var(--cap-muted); font-size: 12px; }
.apex-ref { margin: 10px 2px 0; color: var(--cap-faint); font-size: 10px; }
.modal-overlay { position: fixed; inset: 0; z-index: 5000; display: flex; align-items: center; justify-content: center; padding: 18px; background: rgba(6, 16, 31, .46); }
.edit-modal { width: min(760px, calc(100vw - 36px)); max-height: calc(100vh - 36px); display: flex; flex-direction: column; overflow: hidden; border: 1px solid var(--cap-border-soft); border-radius: 16px; background: #fff; box-shadow: 0 24px 70px rgba(0, 0, 0, .24); }
.edit-modal__header { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; padding: 17px 20px 14px; border-bottom: 1px solid var(--cap-border-soft); }
.edit-modal__header h3 { margin: 0; color: var(--cap-heading); font-size: 18px; font-weight: 600; }
.edit-modal__header p { margin: 4px 0 0; color: var(--cap-muted); font-size: 11px; }
.close-button { width: 38px; height: 38px; flex: 0 0 38px; display: grid; place-items: center; border: 1px solid var(--cap-border); border-radius: 50%; background: #fff; color: var(--cap-muted); cursor: pointer; }
.modal-error { margin: 14px 20px 0; }
.edit-modal__body { flex: 1 1 auto; min-height: 0; overflow-y: auto; padding: 18px 20px; }
.form-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 14px 18px; margin: 0; padding: 0; border: 0; }
.field { min-width: 0; }
.field--wide { grid-column: 1 / -1; }
.field > label { display: block; margin-bottom: 6px; color: var(--cap-ink-soft); font-size: 11px; font-weight: 500; }
.field input:not([type="checkbox"]), .field select, .field textarea { width: 100%; box-sizing: border-box; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 8px; outline: none; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 13px; }
.field input:not([type="checkbox"]), .field select { height: 42px; }
.field textarea { padding: 10px 12px; resize: vertical; }
.field input:focus, .field select:focus, .field textarea:focus { border-color: var(--cap-faint); box-shadow: 0 0 0 2px rgba(15, 118, 110, .08); }
.field input:disabled, .field select:disabled, .field textarea:disabled { background: var(--cap-bg); color: var(--cap-muted); }
.field .upper { text-transform: uppercase; }
.check { display: flex !important; align-items: center; gap: 8px; height: 42px; margin: 0 !important; color: var(--cap-ink-soft) !important; font-size: 13px !important; cursor: pointer; }
.check input { width: 16px; height: 16px; accent-color: var(--cap-primary); }
.field-error { display: block; margin-top: 5px; color: var(--cap-danger); font-size: 11px; }
.edit-modal__footer { display: flex; align-items: center; gap: 9px; padding: 13px 20px 16px; border-top: 1px solid var(--cap-border-soft); }
.spacer { flex: 1; }
.cancel-button, .save-button, .delete-button { height: 40px; display: inline-flex; align-items: center; gap: 7px; padding: 0 15px; border-radius: 7px; font: inherit; font-size: 13px; font-weight: 500; cursor: pointer; }
.cancel-button { border: 1px solid var(--cap-border); background: #fff; color: var(--cap-muted); }
.save-button { border: 1px solid var(--cap-primary); background: var(--cap-primary); color: #fff; }
.save-button--create { border-color: var(--cap-create); background: var(--cap-create); }
.save-button--create:hover { background: var(--cap-create-hover) !important; }
.save-button:hover { background: var(--cap-primary-hover); }
.delete-button { border: 1px solid var(--cap-danger-border); background: var(--cap-danger-bg); color: var(--cap-danger); }
.delete-button:hover { background: var(--cap-danger-bg); }
button:disabled { opacity: .55; cursor: default; }
@media (max-width: 760px) {
  .crud-page { padding: 16px; }
  .crud-card { padding: 18px; }
  .crud-toolbar { flex-direction: column; align-items: stretch; }
  .form-grid { grid-template-columns: 1fr; }
}
</style>
