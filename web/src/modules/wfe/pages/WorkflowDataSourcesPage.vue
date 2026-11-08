<template>
  <!-- Data sources (K2 SmartObjects, kept simple): the lists workflow forms offer in their drop-downs. -->
  <div class="fx" id="fxSources">
    <div class="fx-head">
      <div>
        <h1>{{ tr('Data sources') }}</h1>
        <p>{{ tr('The lists a workflow form offers in its drop-downs. Built-in ones read the application; make your own for anything else.') }}</p>
      </div>
      <div class="fx-actions">
        <RouterLink class="fx-btn" :to="{ name: 'workflow-forms' }"><span class="fa fa-file-text-o" aria-hidden="true"></span>{{ tr('Forms') }}</RouterLink>
        <button v-if="canEdit" type="button" class="fx-btn fx-btn--teal" @click="newSource"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New data source') }}</button>
      </div>
    </div>

    <section v-if="message" class="wf-msg" :class="{ 'is-bad': message.bad }" role="status">
      <span class="fa" :class="message.bad ? 'fa-exclamation-circle' : 'fa-check-circle'" aria-hidden="true"></span>
      <span>{{ message.text }}</span>
      <button type="button" class="wf-x" :aria-label="tr('Close')" @click="message = null"><span class="fa fa-times" aria-hidden="true"></span></button>
    </section>

    <div class="ds-grid">
      <div class="fx-card ds-list">
        <button v-for="s in sources" :key="s.sourceCode" type="button" class="ds-row" :class="{ on: current?.sourceCode === s.sourceCode && !isNew }" @click="open(s)">
          <span class="ds-row-name">
            <span class="fa" :class="s.builtIn ? 'fa-lock' : 'fa-list-ul'" aria-hidden="true"></span>{{ s.sourceName }}
            <span v-if="s.builtIn" class="fx-chip fx-chip--teal">{{ tr('Built-in') }}</span>
            <span v-else-if="!s.isActive" class="fx-chip fx-chip--plain">{{ tr('Off') }}</span>
          </span>
          <span class="ds-row-sub">{{ s.sourceCode }}<template v-if="!s.builtIn"> · {{ s.itemCount }} {{ s.itemCount === 1 ? tr('item') : tr('items') }}</template></span>
        </button>
      </div>

      <div v-if="current" class="fx-card fx-card-pad ds-edit">
        <template v-if="current.builtIn">
          <h2>{{ current.sourceName }} <span class="fx-chip fx-chip--teal">{{ tr('Built-in') }}</span></h2>
          <p class="ds-sub">{{ current.description }}</p>
          <p class="ds-sub"><b>{{ current.itemCount }}</b> {{ tr('values today. They change as the application changes, so nothing is kept here.') }}</p>
          <div class="fx-table-wrap ds-table">
            <table class="fx-table">
              <thead><tr><th>{{ tr('Stored value') }}</th><th>{{ tr('Shown as') }}</th></tr></thead>
              <tbody><tr v-for="it in current.items.slice(0, 50)" :key="it.value"><td class="ds-mono">{{ it.value }}</td><td>{{ it.label }}</td></tr></tbody>
            </table>
          </div>
          <p v-if="current.items.length > 50" class="ds-sub">{{ tr('Showing the first 50.') }}</p>
        </template>

        <template v-else>
          <h2>{{ isNew ? tr('New data source') : edit.sourceName }}</h2>
          <div class="ds-form">
            <label class="ds-field"><span>{{ tr('Name') }}</span><input v-model="edit.sourceName" class="fx-search" maxlength="150" :placeholder="tr('e.g. Disbursement purposes')" :disabled="!canEdit"></label>
            <label class="ds-field"><span>{{ tr('Code') }}</span><input v-model="edit.sourceCode" class="fx-search ds-mono" maxlength="50" placeholder="DISB_PURPOSES" :disabled="!canEdit"
                   @input="edit.sourceCode = edit.sourceCode.toUpperCase().replace(/[^A-Z0-9_]/g, '_')"></label>
            <label class="ds-field ds-full"><span>{{ tr('Description (optional)') }}</span><input v-model="edit.description" class="fx-search" maxlength="500" :disabled="!canEdit"></label>
            <label class="ds-check ds-full"><input v-model="edit.isActive" type="checkbox" class="fx-check" :disabled="!canEdit"> <span>{{ tr('On') }}</span><small>{{ tr('Switched off, its drop-downs offer nothing.') }}</small></label>
          </div>

          <div class="ds-items-head">
            <h3>{{ tr('Items') }}</h3>
            <span class="fx-cell-sub">{{ tr('The value is stored in the workflow; people see the label.') }}</span>
          </div>
          <div class="fx-table-wrap">
            <table class="fx-table ds-items">
              <thead><tr><th>{{ tr('Value') }}</th><th>{{ tr('Label') }}</th><th>{{ tr('On') }}</th><th></th></tr></thead>
              <tbody>
                <tr v-for="(it, i) in edit.items" :key="i">
                  <td><input v-model="it.value" class="fx-search ds-mono" maxlength="200" :disabled="!canEdit"></td>
                  <td><input v-model="it.label" class="fx-search" maxlength="300" :placeholder="it.value" :disabled="!canEdit"></td>
                  <td><input v-model="it.isActive" type="checkbox" class="fx-check" :disabled="!canEdit"></td>
                  <td class="fx-r ds-tools">
                    <template v-if="canEdit">
                      <button type="button" class="ds-tool" :disabled="i === 0" :aria-label="tr('Move up')" @click="move(i, -1)"><span class="fa fa-arrow-up" aria-hidden="true"></span></button>
                      <button type="button" class="ds-tool" :disabled="i === edit.items.length - 1" :aria-label="tr('Move down')" @click="move(i, 1)"><span class="fa fa-arrow-down" aria-hidden="true"></span></button>
                      <button type="button" class="ds-tool" :aria-label="tr('Remove')" @click="edit.items.splice(i, 1)"><span class="fa fa-trash-o" aria-hidden="true"></span></button>
                    </template>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <button v-if="canEdit" type="button" class="fx-btn fx-btn--sm ds-add" @click="edit.items.push({ value: '', label: '', isActive: true })">
            <span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Add item') }}
          </button>

          <div v-if="canEdit" class="ds-actions">
            <template v-if="!isNew">
              <template v-if="confirmDelete">
                <span class="ds-confirm">{{ tr('Delete this data source?') }}</span>
                <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="remove">{{ tr('Yes, delete') }}</button>
                <button type="button" class="fx-btn" @click="confirmDelete = false">{{ tr('No') }}</button>
              </template>
              <button v-else type="button" class="fx-btn" @click="confirmDelete = true"><span class="fa fa-trash-o" aria-hidden="true"></span>{{ tr('Delete') }}</button>
            </template>
            <span class="ds-gap"></span>
            <span v-if="problem" class="ds-problem"><span class="fa fa-exclamation-circle" aria-hidden="true"></span> {{ problem }}</span>
            <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !!problem" @click="save"><span class="fa fa-check" aria-hidden="true"></span>{{ isNew ? tr('Create data source') : tr('Save') }}</button>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import { apiErrorText, dataSourceApi, type DataSource, type DataSourceItem } from '../services/formService'

const canEdit = hasPermission('WFE.DEFINITION.EDIT')
const sources = ref<DataSource[]>([])
const current = ref<DataSource | null>(null)
const isNew = ref(false)
const busy = ref(false)
const confirmDelete = ref(false)
const message = ref<{ text: string; bad: boolean } | null>(null)

const blank = () => ({ id: 0, sourceCode: '', sourceName: '', description: '', isActive: true, items: [] as DataSourceItem[], rowVersion: 0 })
const edit = reactive(blank())

const problem = computed(() => {
  if (!edit.sourceName.trim()) return tr('Give the data source a name.')
  if (!/^[A-Z][A-Z0-9_]{1,49}$/.test(edit.sourceCode)) return tr('Give it a code: capital letters, digits or _.')
  if (edit.items.some((x) => !x.value.trim())) return tr('Every item needs a value.')
  const values = edit.items.map((x) => x.value.trim().toLowerCase())
  if (new Set(values).size !== values.length) return tr('A value is in the list twice.')
  return ''
})

function newSource() {
  Object.assign(edit, blank())
  edit.items = [{ value: '', label: '', isActive: true }]
  current.value = { dataSourceId: null, sourceCode: '', sourceName: '', description: null, isActive: true, builtIn: false, itemCount: 0, items: [], rowVersion: 0 }
  isNew.value = true
  confirmDelete.value = false
}

async function open(s: DataSource) {
  try {
    const full = await dataSourceApi.get(s.sourceCode)
    current.value = full
    isNew.value = false
    confirmDelete.value = false
    if (!full.builtIn)
      Object.assign(edit, { id: full.dataSourceId ?? 0, sourceCode: full.sourceCode, sourceName: full.sourceName, description: full.description ?? '', isActive: full.isActive, items: full.items.map((x) => ({ ...x })), rowVersion: full.rowVersion })
  } catch (e) {
    message.value = { bad: true, text: apiErrorText(e) }
  }
}

function move(i: number, by: number) {
  const it = edit.items.splice(i, 1)[0]
  if (it) edit.items.splice(i + by, 0, it)
}

async function save() {
  if (problem.value) return
  busy.value = true
  try {
    const saved = await dataSourceApi.save(isNew.value ? null : edit.id, {
      sourceCode: edit.sourceCode, sourceName: edit.sourceName.trim(), description: edit.description || null, isActive: edit.isActive,
      items: edit.items.map((x) => ({ value: x.value.trim(), label: x.label.trim(), isActive: x.isActive })), rowVersion: edit.rowVersion
    })
    message.value = { bad: false, text: `${tr('Saved')} ${saved.sourceName}: ${saved.itemCount} ${tr('items')}.` }
    await load()
    await open(saved)
  } catch (e) {
    message.value = { bad: true, text: apiErrorText(e) }
  } finally {
    busy.value = false
  }
}

async function remove() {
  busy.value = true
  try {
    await dataSourceApi.remove(edit.id)
    message.value = { bad: false, text: `${tr('Deleted')} ${edit.sourceName}.` }
    current.value = null
    await load()
  } catch (e) {
    message.value = { bad: true, text: apiErrorText(e) }
  } finally {
    busy.value = false
    confirmDelete.value = false
  }
}

async function load() {
  try { sources.value = await dataSourceApi.list() } catch (e) { message.value = { bad: true, text: apiErrorText(e) } }
}

onMounted(async () => {
  await load()
  const first = sources.value.find((s) => !s.builtIn) ?? sources.value[0]
  if (first) await open(first)
})
</script>

<style scoped>
.wf-msg { display: flex; align-items: center; gap: 10px; margin: 0 0 14px; padding: 11px 14px; border-radius: 10px; font-size: 13.5px; background: #e4f6ee; border: 1px solid #bfe5d2; color: #0e5a3c; }
.wf-msg.is-bad { background: #fdecea; border-color: #f3c3bc; color: #8a2416; }
.wf-msg > span:nth-child(2) { flex: 1; }
.wf-x { border: 0; background: none; color: inherit; cursor: pointer; }
.ds-grid { display: grid; grid-template-columns: minmax(240px, 300px) minmax(0, 1fr); gap: 16px; align-items: start; }
.ds-row { display: grid; gap: 2px; width: 100%; padding: 11px 16px; border: 0; border-top: 1px solid #e6eef1; background: none; text-align: left; cursor: pointer; font: inherit; }
.ds-row:first-child { border-top: 0; }
.ds-row:hover { background: #f5f9fa; }
.ds-row.on { background: #e7f3f2; box-shadow: inset 3px 0 0 #00636D; }
.ds-row-name { display: flex; align-items: center; gap: 7px; color: #12324a; font-weight: 700; font-size: 13.5px; }
.ds-row-name .fa { color: #00636D; width: 14px; }
.ds-row-sub { color: #5f7787; font-size: 12px; padding-left: 21px; }
.ds-edit { min-width: 0; }
.ds-edit h2 { display: flex; align-items: center; gap: 8px; }
.ds-sub { margin: 0 0 10px; color: #5f7787; font-size: 12.5px; }
.ds-table { max-height: 420px; overflow: auto; }
.ds-mono { font-family: ui-monospace, SFMono-Regular, Menlo, monospace; font-size: 12.5px; }
.ds-form { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); gap: 12px 14px; margin: 12px 0 18px; }
.ds-field { display: grid; gap: 5px; min-width: 0; }
.ds-field > span { color: #12324a; font-size: 12px; font-weight: 700; }
.ds-field .fx-search, .ds-items .fx-search { width: 100%; min-width: 0; max-width: 100%; flex: none; }
.ds-full { grid-column: 1 / -1; }
.ds-check { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #12324a; }
.ds-check small { color: #5f7787; }
.ds-items-head { display: flex; align-items: baseline; gap: 12px; margin-bottom: 8px; }
.ds-items-head h3 { margin: 0; color: #12324a; font-size: 14px; }
.ds-items td { vertical-align: middle; }
.ds-tools { white-space: nowrap; }
.ds-tool { width: 28px; height: 28px; border: 0; border-radius: 6px; background: none; color: #5f7787; cursor: pointer; }
.ds-tool:hover:not(:disabled) { background: #eef4f6; color: #00636D; }
.ds-tool:disabled { opacity: .35; cursor: default; }
.ds-add { margin-top: 10px; }
.ds-actions { display: flex; flex-wrap: wrap; align-items: center; gap: 8px; margin-top: 18px; padding-top: 14px; border-top: 1px solid #e6eef1; }
.ds-gap { flex: 1; }
.ds-problem, .ds-confirm { color: #b4520b; font-size: 12.5px; }
.ds-confirm { font-weight: 700; }
@media (max-width: 900px) { .ds-grid, .ds-form { grid-template-columns: minmax(0, 1fr); } }
</style>
