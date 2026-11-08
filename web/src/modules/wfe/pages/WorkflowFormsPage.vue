<template>
  <!-- Workflow forms (K2 SmartForms, kept simple): lay out a form over a workflow's data fields, then use it on a step or at the start. -->
  <div class="fx" id="fxForms">
    <div class="fx-head">
      <div>
        <h1>{{ tr('Workflow forms') }}</h1>
        <p>{{ tr('Lay out a form over a workflow\'s data fields. In the designer, a step or the start of the workflow then uses it.') }}</p>
      </div>
      <div class="fx-actions">
        <RouterLink class="fx-btn" :to="{ name: 'workflow-data-sources' }"><span class="fa fa-database" aria-hidden="true"></span>{{ tr('Data Sources') }}</RouterLink>
        <button v-if="canEdit" type="button" class="fx-btn fx-btn--teal" @click="newForm"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New form') }}</button>
      </div>
    </div>

    <section v-if="message" class="wf-msg" :class="{ 'is-bad': message.bad }" role="status">
      <span class="fa" :class="message.bad ? 'fa-exclamation-circle' : 'fa-check-circle'" aria-hidden="true"></span>
      <span>{{ message.text }}</span>
      <button type="button" class="wf-x" :aria-label="tr('Close')" @click="message = null"><span class="fa fa-times" aria-hidden="true"></span></button>
    </section>

    <div class="fm-grid">
      <!-- the forms -->
      <div class="fx-card fm-list">
        <div class="fm-list-head"><h2>{{ tr('Forms') }}</h2><span class="fx-cell-sub">{{ forms.length }}</span></div>
        <button v-for="f in forms" :key="f.formId" type="button" class="fm-row" :class="{ on: edit.id === f.formId }" @click="open(f)">
          <span class="fm-row-name">{{ f.formName }}<span v-if="!f.isActive" class="fx-chip fx-chip--plain">{{ tr('Off') }}</span></span>
          <span class="fm-row-sub">{{ f.formCode }} · {{ f.workflowName }}</span>
          <span class="fm-row-sub">
            {{ f.controlCount }} {{ f.controlCount === 1 ? tr('field') : tr('fields') }} ·
            {{ f.usedAt.length ? `${tr('used at')} ${f.usedAt.join(', ')}` : tr('not used yet') }}
          </span>
        </button>
        <div v-if="!forms.length" class="fx-empty"><span class="fa fa-file-text-o" aria-hidden="true"></span>{{ tr('No forms yet.') }}</div>
      </div>

      <!-- the editor -->
      <div v-if="editing" class="fm-editor">
        <div class="fx-card fx-card-pad">
          <div class="fm-settings">
            <label class="fm-field fm-wide">
              <span>{{ tr('Form name') }}</span>
              <input v-model="edit.formName" class="fx-search" maxlength="150" :placeholder="tr('e.g. Disbursement request')" :disabled="!canEdit">
            </label>
            <label class="fm-field">
              <span>{{ tr('Code') }}</span>
              <input v-model="edit.formCode" class="fx-search fm-code" maxlength="50" placeholder="DISB_REQUEST_FORM" :disabled="!canEdit"
                     @input="edit.formCode = edit.formCode.toUpperCase().replace(/[^A-Z0-9_]/g, '_')">
            </label>
            <label class="fm-field fm-wide">
              <span>{{ tr('Workflow') }}</span>
              <select v-model.number="edit.workflowDefinitionId" class="fx-select" :disabled="!canEdit">
                <option :value="0">{{ tr('Choose a workflow') }}</option>
                <option v-for="w in workflows" :key="w.dgrmId" :value="w.dgrmId">{{ w.dgrmName }} ({{ w.workflowCode }})</option>
              </select>
            </label>
            <div class="fm-field">
              <span>{{ tr('Columns') }}</span>
              <div class="fx-seg" role="group">
                <button type="button" :aria-pressed="edit.columns === 1" :disabled="!canEdit" @click="edit.columns = 1">1</button>
                <button type="button" :aria-pressed="edit.columns === 2" :disabled="!canEdit" @click="edit.columns = 2">2</button>
              </div>
            </div>
            <label class="fm-field fm-full">
              <span>{{ tr('Description (optional)') }}</span>
              <input v-model="edit.description" class="fx-search" maxlength="500" :disabled="!canEdit">
            </label>
            <label class="fm-check">
              <input v-model="edit.isActive" type="checkbox" class="fx-check" :disabled="!canEdit">
              <span>{{ tr('On') }}</span><small>{{ tr('Switched off, steps that use it work as if they had no form.') }}</small>
            </label>
          </div>
        </div>

        <div class="fm-panes">
          <!-- controls -->
          <div class="fx-card fx-card-pad">
            <div class="fm-pane-head">
              <h2>{{ tr('Controls') }}</h2>
              <span class="fx-cell-sub">{{ tr('Top to bottom, as on the form') }}</span>
            </div>

            <p v-if="!edit.workflowDefinitionId" class="fm-hint">{{ tr('Choose the workflow first: its data fields become the form\'s controls.') }}</p>
            <p v-else-if="!fields.length" class="fm-hint">{{ tr('This workflow has no data fields yet. Add them in the designer with Data Fields.') }}</p>

            <ol class="fm-controls">
              <li v-for="(c, i) in edit.controls" :key="i" class="fm-control" :class="{ open: openIndex === i }">
                <div class="fm-control-head">
                  <span class="fm-control-icon fa" :class="iconOf(c)" aria-hidden="true"></span>
                  <button type="button" class="fm-control-title" @click="openIndex = openIndex === i ? -1 : i">
                    <b>{{ titleOf(c) }}</b>
                    <small>{{ subOf(c) }}</small>
                  </button>
                  <span v-if="canEdit" class="fm-control-tools">
                    <button type="button" class="fm-tool" :disabled="i === 0" :aria-label="tr('Move up')" @click="move(i, -1)"><span class="fa fa-arrow-up" aria-hidden="true"></span></button>
                    <button type="button" class="fm-tool" :disabled="i === edit.controls.length - 1" :aria-label="tr('Move down')" @click="move(i, 1)"><span class="fa fa-arrow-down" aria-hidden="true"></span></button>
                    <button type="button" class="fm-tool" :aria-label="tr('Remove')" @click="removeControl(i)"><span class="fa fa-trash-o" aria-hidden="true"></span></button>
                  </span>
                </div>

                <div v-if="openIndex === i" class="fm-control-body">
                  <template v-if="c.kind === 'field'">
                    <label class="fm-field">
                      <span>{{ tr('Label on the form') }}</span>
                      <input v-model="c.label" class="fx-search" :placeholder="fieldOf(c)?.caption" :disabled="!canEdit">
                    </label>
                    <label class="fm-field">
                      <span>{{ tr('Control') }}</span>
                      <select v-model="c.control" class="fx-select" :disabled="!canEdit">
                        <option v-for="t in controlsFor(fieldOf(c)?.type ?? 'String')" :key="t" :value="t">{{ tr(controlName(t)) }}</option>
                      </select>
                    </label>
                    <label v-if="c.control === 'dropdown'" class="fm-field">
                      <span>{{ tr('Data source') }}</span>
                      <select v-model="c.dataSource" class="fx-select" :disabled="!canEdit">
                        <option :value="null">{{ fieldOf(c)?.choices.length ? tr('The field\'s own choices') : tr('Choose a data source') }}</option>
                        <option v-for="s in sources" :key="s.sourceCode" :value="s.sourceCode">{{ s.sourceName }}{{ s.builtIn ? ` (${tr('built-in')})` : '' }}</option>
                      </select>
                    </label>
                    <label v-if="edit.columns === 2" class="fm-field">
                      <span>{{ tr('Width') }}</span>
                      <select v-model="c.width" class="fx-select" :disabled="!canEdit">
                        <option value="full">{{ tr('Whole row') }}</option>
                        <option value="half">{{ tr('Half row') }}</option>
                      </select>
                    </label>
                    <label class="fm-field fm-full">
                      <span>{{ tr('Help text (optional)') }}</span>
                      <input v-model="c.help" class="fx-search" :placeholder="fieldOf(c)?.description ?? ''" :disabled="!canEdit">
                    </label>
                    <div class="fm-flags fm-full">
                      <label class="fm-check"><input v-model="c.required" type="checkbox" class="fx-check" :disabled="!canEdit || c.readOnly || fieldOf(c)?.required"> <span>{{ tr('Required') }}</span>
                        <small v-if="fieldOf(c)?.required">{{ tr('(the data field is required)') }}</small></label>
                      <label class="fm-check"><input v-model="c.readOnly" type="checkbox" class="fx-check" :disabled="!canEdit" @change="c.readOnly && (c.required = false)"> <span>{{ tr('Read-only') }}</span>
                        <small>{{ tr('shown, not changed') }}</small></label>
                    </div>
                  </template>
                  <label v-else class="fm-field fm-full">
                    <span>{{ c.kind === 'heading' ? tr('Heading text') : tr('Note text') }}</span>
                    <input v-model="c.label" class="fx-search" :disabled="!canEdit">
                  </label>
                </div>
              </li>
            </ol>

            <div v-if="canEdit && edit.workflowDefinitionId" class="fm-add">
              <span class="fm-add-label">{{ tr('Add') }}</span>
              <button v-for="f in unused" :key="f.name" type="button" class="fx-btn fx-btn--sm" @click="addField(f)">
                <span class="fa fa-plus" aria-hidden="true"></span>{{ f.caption }}
              </button>
              <button v-if="unused.length > 1" type="button" class="fx-btn fx-btn--sm" @click="unused.forEach(addField)">{{ tr('All fields') }}</button>
              <button type="button" class="fx-btn fx-btn--sm" @click="addText('heading')"><span class="fa fa-header" aria-hidden="true"></span>{{ tr('Heading') }}</button>
              <button type="button" class="fx-btn fx-btn--sm" @click="addText('note')"><span class="fa fa-info" aria-hidden="true"></span>{{ tr('Note') }}</button>
            </div>
          </div>

          <!-- live preview -->
          <div class="fx-card fx-card-pad fm-preview">
            <div class="fm-pane-head">
              <h2>{{ tr('Preview') }}</h2>
              <span class="fx-cell-sub">{{ tr('As people see it on the task') }}</span>
            </div>
            <div class="fm-sheet">
              <div class="fm-sheet-title">{{ edit.formName || tr('Untitled form') }}</div>
              <WorkflowFormView :view="preview" v-model="previewValues" id-prefix="pv" />
            </div>
          </div>
        </div>

        <div v-if="canEdit" class="fm-actions">
          <template v-if="edit.id">
            <template v-if="confirmDelete">
              <span class="fm-confirm">{{ tr('Delete this form?') }}</span>
              <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="remove">{{ tr('Yes, delete') }}</button>
              <button type="button" class="fx-btn" @click="confirmDelete = false">{{ tr('No') }}</button>
            </template>
            <button v-else type="button" class="fx-btn" @click="confirmDelete = true"><span class="fa fa-trash-o" aria-hidden="true"></span>{{ tr('Delete') }}</button>
          </template>
          <span class="fm-gap"></span>
          <span v-if="problem" class="fm-problem"><span class="fa fa-exclamation-circle" aria-hidden="true"></span> {{ problem }}</span>
          <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !!problem" @click="save">
            <span class="fa fa-check" aria-hidden="true"></span>{{ edit.id ? tr('Save form') : tr('Create form') }}
          </button>
        </div>
      </div>

      <div v-else class="fx-card fx-card-pad fm-blank">
        <span class="fa fa-file-text-o" aria-hidden="true"></span>
        <p>{{ forms.length ? tr('Choose a form to change it, or make a new one.') : tr('Make the first form: choose its workflow, then add its data fields.') }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import WorkflowFormView from '../components/WorkflowFormView.vue'
import { getWorkflowDefinitions, type WorkflowDefinition } from '../services/workflowDefinitionService'
import {
  apiErrorText, controlName, controlsFor, dataSourceApi, defaultControl, formApi,
  type DataField, type DataSource, type FormControl, type FormControlView, type FormView, type Option, type WorkflowForm
} from '../services/formService'

const route = useRoute()
const canEdit = hasPermission('WFE.DEFINITION.EDIT')

const forms = ref<WorkflowForm[]>([])
const workflows = ref<WorkflowDefinition[]>([])
const sources = ref<DataSource[]>([])
const fields = ref<DataField[]>([])
const options = ref<Record<string, Option[]>>({})
const editing = ref(false)
const openIndex = ref(-1)
const busy = ref(false)
const confirmDelete = ref(false)
const message = ref<{ text: string; bad: boolean } | null>(null)
const previewValues = ref<Record<string, string | null>>({})

const blank = () => ({ id: 0, formCode: '', formName: '', workflowDefinitionId: 0, description: '', isActive: true, columns: 1 as 1 | 2, controls: [] as FormControl[], rowVersion: 0 })
const edit = reactive(blank())

const unused = computed(() => fields.value.filter((f) => !edit.controls.some((c) => c.kind === 'field' && c.field?.toLowerCase() === f.name.toLowerCase())))

function fieldOf(c: FormControl) { return fields.value.find((f) => f.name.toLowerCase() === (c.field ?? '').toLowerCase()) }
function titleOf(c: FormControl) {
  if (c.kind !== 'field') return c.label || (c.kind === 'heading' ? tr('Heading') : tr('Note'))
  return c.label || fieldOf(c)?.caption || c.field || ''
}
function subOf(c: FormControl) {
  if (c.kind === 'heading') return tr('Heading')
  if (c.kind === 'note') return tr('Note')
  const f = fieldOf(c)
  if (!f) return tr('Not a data field of this workflow any more')
  const bits = [tr(controlName(c.control ?? defaultControl(f.type)))]
  if (c.control === 'dropdown') bits.push(c.dataSource ? sources.value.find((s) => s.sourceCode === c.dataSource)?.sourceName ?? c.dataSource : tr('own choices'))
  if (c.readOnly) bits.push(tr('read-only'))
  else if (c.required || f.required) bits.push(tr('required'))
  return bits.join(' · ')
}
function iconOf(c: FormControl) {
  if (c.kind === 'heading') return 'fa-header'
  if (c.kind === 'note') return 'fa-info-circle'
  return ({ text: 'fa-font', textarea: 'fa-align-left', number: 'fa-hashtag', amount: 'fa-money', date: 'fa-calendar', yesno: 'fa-toggle-on', dropdown: 'fa-caret-square-o-down', user: 'fa-user' } as Record<string, string>)[c.control ?? 'text'] ?? 'fa-font'
}

/** Problems that stop saving, in plain words; the server checks the same and more. */
const problem = computed(() => {
  if (!edit.formName.trim()) return tr('Give the form a name.')
  if (!/^[A-Z][A-Z0-9_]{1,49}$/.test(edit.formCode)) return tr('Give the form a code: capital letters, digits or _.')
  if (!edit.workflowDefinitionId) return tr('Choose the workflow.')
  const bad = edit.controls.find((c) => c.kind === 'field' && c.control === 'dropdown' && !c.dataSource && !fieldOf(c)?.choices.length)
  if (bad) return `${titleOf(bad)}: ${tr('choose the data source of the drop-down.')}`
  const lost = edit.controls.find((c) => c.kind === 'field' && !fieldOf(c))
  if (lost) return `${lost.field}: ${tr('not a data field of this workflow any more. Remove it.')}`
  return ''
})

/** The preview the task page would show, built here so it changes as the form is laid out. */
const preview = computed<FormView>(() => ({
  formCode: edit.formCode, formName: edit.formName, description: edit.description, columns: edit.columns, editable: true,
  controls: edit.controls.flatMap((c): FormControlView[] => {
    if (c.kind !== 'field') return [{ kind: c.kind, field: null, label: c.label ?? '', control: c.kind, fieldType: null, required: false, readOnly: true, help: null, width: 'full', value: null, options: [] }]
    const f = fieldOf(c)
    if (!f) return []
    const control = c.control ?? defaultControl(f.type)
    const source = control === 'user' ? 'USERS' : control === 'dropdown' ? c.dataSource : null
    const opts = source ? options.value[source] ?? [] : control === 'dropdown' ? f.choices.map((x) => ({ value: x, label: x })) : []
    return [{ kind: 'field', field: f.name, label: c.label || f.caption, control, fieldType: f.type, required: !c.readOnly && (c.required || f.required), readOnly: c.readOnly, help: c.help || f.description, width: c.width ?? 'full', value: null, options: opts }]
  })
}))

// drop-down options for the preview, loaded once per data source
watch(() => preview.value.controls.map((c) => (c.control === 'user' ? 'USERS' : c.control === 'dropdown' ? edit.controls.find((x) => x.field === c.field)?.dataSource : null)).filter(Boolean) as string[],
  (codes) => {
    for (const code of codes)
      if (!options.value[code]) {
        options.value = { ...options.value, [code]: [] }
        dataSourceApi.options(code).then((o) => (options.value = { ...options.value, [code]: o })).catch(() => undefined)
      }
  }, { immediate: true })

watch(() => edit.workflowDefinitionId, async (id) => {
  fields.value = []
  if (!id) return
  try { fields.value = await formApi.fields(id) } catch (e) { message.value = { bad: true, text: apiErrorText(e) } }
})

function newForm() {
  Object.assign(edit, blank())
  editing.value = true
  openIndex.value = -1
  confirmDelete.value = false
  previewValues.value = {}
}

function open(f: WorkflowForm) {
  Object.assign(edit, {
    id: f.formId, formCode: f.formCode, formName: f.formName, workflowDefinitionId: f.workflowDefinitionId, description: f.description ?? '',
    isActive: f.isActive, columns: f.layout.columns === 2 ? 2 : 1, controls: f.layout.controls.map((c) => ({ ...c, width: c.width ?? 'full' })), rowVersion: f.rowVersion
  })
  editing.value = true
  openIndex.value = -1
  confirmDelete.value = false
  previewValues.value = {}
}

function addField(f: DataField) {
  edit.controls.push({ kind: 'field', field: f.name, label: null, control: defaultControl(f.type), dataSource: null, required: f.required, readOnly: false, help: null, width: 'full' })
}
function addText(kind: 'heading' | 'note') {
  edit.controls.push({ kind, field: null, label: '', control: null, dataSource: null, required: false, readOnly: false, help: null, width: 'full' })
  openIndex.value = edit.controls.length - 1
}
function move(i: number, by: number) {
  const c = edit.controls.splice(i, 1)[0]
  if (c) edit.controls.splice(i + by, 0, c)
  if (openIndex.value === i) openIndex.value = i + by
}
function removeControl(i: number) {
  edit.controls.splice(i, 1)
  openIndex.value = -1
}

async function save() {
  if (problem.value) return
  busy.value = true
  try {
    const saved = await formApi.save(edit.id || null, {
      formCode: edit.formCode, formName: edit.formName.trim(), workflowDefinitionId: edit.workflowDefinitionId, description: edit.description || null,
      isActive: edit.isActive, layout: { columns: edit.columns, controls: edit.controls }, rowVersion: edit.rowVersion
    })
    message.value = { bad: false, text: `${tr('Saved')} ${saved.formName}. ${saved.usedAt.length ? `${tr('Used at')} ${saved.usedAt.join(', ')}.` : tr('Open the workflow in the designer to use it on a step or at the start.')}` }
    await load()
    open(saved)
  } catch (e) {
    message.value = { bad: true, text: apiErrorText(e) }
  } finally {
    busy.value = false
  }
}

async function remove() {
  busy.value = true
  try {
    await formApi.remove(edit.id)
    message.value = { bad: false, text: `${tr('Deleted')} ${edit.formName}.` }
    editing.value = false
    Object.assign(edit, blank())
    await load()
  } catch (e) {
    message.value = { bad: true, text: apiErrorText(e) }
  } finally {
    busy.value = false
    confirmDelete.value = false
  }
}

async function load() {
  try { forms.value = await formApi.list() } catch (e) { message.value = { bad: true, text: apiErrorText(e) } }
}

onMounted(async () => {
  getWorkflowDefinitions().then((x) => (workflows.value = x)).catch(() => (workflows.value = []))
  dataSourceApi.list().then((x) => (sources.value = x.filter((s) => s.isActive))).catch(() => (sources.value = []))
  await load()
  const id = Number(route.query.form)
  const wf = Number(route.query.workflow)
  const f = forms.value.find((x) => x.formId === id)
  if (f) open(f)
  else if (wf && canEdit) { newForm(); edit.workflowDefinitionId = wf }
})
</script>

<style scoped>
.wf-msg { display: flex; align-items: center; gap: 10px; margin: 0 0 14px; padding: 11px 14px; border-radius: 10px; font-size: 13.5px; background: #e4f6ee; border: 1px solid #bfe5d2; color: #0e5a3c; }
.wf-msg.is-bad { background: #fdecea; border-color: #f3c3bc; color: #8a2416; }
.wf-msg > span:nth-child(2) { flex: 1; }
.wf-x { border: 0; background: none; color: inherit; cursor: pointer; }
.fm-grid { display: grid; grid-template-columns: minmax(240px, 300px) minmax(0, 1fr); gap: 16px; align-items: start; }
.fm-list { padding-bottom: 6px; }
.fm-list-head { display: flex; align-items: baseline; justify-content: space-between; padding: 14px 16px 8px; }
.fm-list-head h2 { margin: 0; }
.fm-row { display: grid; gap: 2px; width: 100%; padding: 10px 16px; border: 0; border-top: 1px solid #e6eef1; background: none; text-align: left; cursor: pointer; font: inherit; }
.fm-row:hover { background: #f5f9fa; }
.fm-row.on { background: #e7f3f2; box-shadow: inset 3px 0 0 #00636D; }
.fm-row-name { display: flex; align-items: center; gap: 6px; color: #12324a; font-weight: 700; font-size: 13.5px; }
.fm-row-sub { color: #5f7787; font-size: 12px; }
.fm-editor { display: grid; gap: 16px; min-width: 0; }
.fm-settings { display: grid; grid-template-columns: minmax(0, 2fr) minmax(0, 1.2fr) minmax(0, 2fr) auto; gap: 12px 14px; align-items: end; }
.fm-field { display: grid; gap: 5px; min-width: 0; }
.fm-field > span { color: #12324a; font-size: 12px; font-weight: 700; }
.fm-field .fx-select, .fm-field .fx-search { width: 100%; min-width: 0; max-width: 100%; flex: none; }
.fm-full { grid-column: 1 / -1; }
.fm-code { font-family: ui-monospace, SFMono-Regular, Menlo, monospace; letter-spacing: .02em; }
.fm-check { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #12324a; grid-column: 1 / -1; }
.fm-check small { color: #5f7787; }
.fm-panes { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); gap: 16px; align-items: start; }
.fm-pane-head { display: flex; align-items: baseline; justify-content: space-between; gap: 10px; margin-bottom: 12px; }
.fm-pane-head h2 { margin: 0; }
.fm-hint { margin: 0 0 12px; padding: 10px 12px; border-radius: 8px; background: #fff4ea; color: #7a3d0a; font-size: 12.5px; }
.fm-controls { list-style: none; margin: 0; padding: 0; display: grid; gap: 8px; }
.fm-control { border: 1px solid #d9e6ea; border-radius: 10px; background: #fff; }
.fm-control.open { border-color: #00636D; box-shadow: 0 0 0 3px rgba(0, 99, 109, .08); }
.fm-control-head { display: flex; align-items: center; gap: 10px; padding: 8px 10px; }
.fm-control-icon { width: 28px; height: 28px; display: grid; place-items: center; border-radius: 7px; background: #e7f3f2; color: #00636D; flex: none; }
.fm-control-title { flex: 1; min-width: 0; display: grid; gap: 1px; border: 0; background: none; padding: 0; text-align: left; cursor: pointer; font: inherit; }
.fm-control-title b { color: #12324a; font-size: 13px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.fm-control-title small { color: #5f7787; font-size: 11.5px; }
.fm-control-tools { display: flex; gap: 2px; }
.fm-tool { width: 28px; height: 28px; border: 0; border-radius: 6px; background: none; color: #5f7787; cursor: pointer; }
.fm-tool:hover:not(:disabled) { background: #eef4f6; color: #00636D; }
.fm-tool:disabled { opacity: .35; cursor: default; }
.fm-control-body { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1fr); gap: 10px 12px; padding: 4px 12px 12px 48px; }
.fm-flags { display: flex; flex-wrap: wrap; gap: 6px 18px; }
.fm-flags .fm-check { grid-column: auto; }
.fm-add { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; margin-top: 14px; padding-top: 12px; border-top: 1px dashed #d9e6ea; }
.fm-add-label { color: #5f7787; font-size: 12px; font-weight: 700; margin-right: 4px; }
.fm-preview { position: sticky; top: 12px; }
.fm-sheet { padding: 18px; border: 1px solid #d9e6ea; border-radius: 10px; background: #fbfdfd; }
.fm-sheet-title { margin-bottom: 14px; color: #12324a; font-size: 15px; font-weight: 800; }
.fm-actions { display: flex; flex-wrap: wrap; align-items: center; gap: 8px; }
.fm-gap { flex: 1; }
.fm-problem { color: #b4520b; font-size: 12.5px; }
.fm-confirm { color: #b4520b; font-size: 12.5px; font-weight: 700; }
.fm-blank { display: grid; justify-items: center; gap: 6px; padding: 48px 18px; color: #5f7787; text-align: center; }
.fm-blank .fa { font-size: 28px; color: #9fbfc4; }
@media (max-width: 1200px) { .fm-panes { grid-template-columns: minmax(0, 1fr); } .fm-preview { position: static; } }
@media (max-width: 900px) { .fm-grid { grid-template-columns: minmax(0, 1fr); } .fm-settings { grid-template-columns: minmax(0, 1fr); } .fm-control-body { grid-template-columns: minmax(0, 1fr); padding-left: 12px; } }
</style>
