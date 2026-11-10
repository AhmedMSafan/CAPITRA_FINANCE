<template>
  <!-- Document Designer (K2 SmartForms, kept simple): a document has its own fields and layout; workflows link it in the designer. -->
  <div class="fx" id="fxForms">
    <div class="fx-head">
      <div>
        <h1>{{ tr('Document Designer') }}</h1>
        <p>{{ tr('Design a document once: its fields, which are required, and how it looks. In the workflow designer, a user-defined workflow links it to the whole workflow, and a system-built workflow links it to a step.') }}</p>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openWorkflowManual(6)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
        <RouterLink class="fx-btn" :to="{ name: 'workflow-data-sources' }"><span class="fa fa-database" aria-hidden="true"></span>{{ tr('Data Sources') }}</RouterLink>
        <button v-if="canEdit" type="button" class="fx-btn fx-btn--teal" @click="newForm"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New document') }}</button>
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
        <div class="fm-list-head"><h2>{{ tr('Documents') }}</h2><span class="fx-cell-sub">{{ forms.length }}</span></div>
        <button v-for="f in forms" :key="f.formId" type="button" class="fm-row" :class="{ on: edit.id === f.formId }" @click="open(f)">
          <span class="fm-row-name">{{ f.formName }}<span v-if="!f.isActive" class="fx-chip fx-chip--plain">{{ tr('Off') }}</span></span>
          <span class="fm-row-sub">{{ f.formCode }}<template v-if="!f.isDocument"> · {{ tr('older form of') }} {{ f.workflowName }}</template></span>
          <span class="fm-row-sub">
            {{ f.controlCount }} {{ f.controlCount === 1 ? tr('field') : tr('fields') }} ·
            {{ f.usedAt.length ? `${tr('used by')} ${f.usedAt.join(', ')}` : tr('not linked yet') }}
          </span>
        </button>
        <div v-if="!forms.length" class="fx-empty"><span class="fa fa-file-text-o" aria-hidden="true"></span>{{ tr('No documents yet.') }}</div>
      </div>

      <!-- the editor -->
      <div v-if="editing" class="fm-editor">
        <div class="fx-card fx-card-pad">
          <div class="fm-settings">
            <label class="fm-field fm-wide">
              <span>{{ tr('Document name') }}</span>
              <input v-model="edit.formName" class="fx-search" maxlength="150" :placeholder="tr('e.g. Vacation request')" :disabled="!canEdit">
            </label>
            <label class="fm-field">
              <span>{{ tr('Code') }}</span>
              <input v-model="edit.formCode" class="fx-search fm-code" maxlength="50" placeholder="VACATION_REQUEST" :disabled="!canEdit"
                     @input="edit.formCode = edit.formCode.toUpperCase().replace(/[^A-Z0-9_]/g, '_')">
            </label>
            <div class="fm-field fm-wide">
              <span>{{ tr('Used by') }}</span>
              <div class="fm-used">
                <span v-for="u in usedAt" :key="u" class="fx-chip">{{ u }}</span>
                <span v-if="!usedAt.length" class="fx-cell-sub">{{ tr('No workflow links it yet.') }}</span>
              </div>
            </div>
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
              <span>{{ tr('On') }}</span><small>{{ tr('Switched off, it is not offered, and steps that link it work as if they had none.') }}</small>
            </label>
          </div>
          <p v-if="legacyOf" class="fm-hint fm-legacy">
            <span class="fa fa-info-circle" aria-hidden="true"></span>
            {{ tr('This form was made before documents and uses the data fields of') }} <b>{{ legacyOf }}</b>.
            {{ tr('Its fields are copied below. Saving makes it a document of its own, and the workflow keeps using it.') }}
          </p>
        </div>

        <!-- the document's own fields -->
        <div class="fx-card fx-card-pad">
          <div class="fm-pane-head">
            <h2>{{ tr('Fields') }}</h2>
            <span class="fx-cell-sub">{{ tr('The values the document holds. A required field must be filled in before the document is sent.') }}</span>
          </div>
          <div class="fm-table-wrap">
            <table class="fm-fields">
              <thead>
                <tr>
                  <th>{{ tr('Label') }}</th><th>{{ tr('Name') }}</th><th>{{ tr('Type') }}</th><th>{{ tr('Choices') }}</th>
                  <th>{{ tr('Default or automatic value') }}</th><th class="fm-c">{{ tr('Required') }}</th><th></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(f, i) in edit.fields" :key="i">
                  <td><input v-model="f.label" class="fx-search" maxlength="100" :placeholder="tr('e.g. From date')" :disabled="!canEdit" @blur="nameFromLabel(f, i)"></td>
                  <td><input v-model="f.name" class="fx-search fm-code" maxlength="50" placeholder="fromDate" :disabled="!canEdit" @change="renameField(i)"></td>
                  <td>
                    <select v-model="f.type" class="fx-select" :disabled="!canEdit" @change="typeChanged(f)">
                      <option v-for="t in fieldTypes" :key="t.value" :value="t.value">{{ tr(t.label) }}</option>
                    </select>
                  </td>
                  <td>
                    <input v-if="f.type === 'Choice'" :value="f.choices.join(', ')" class="fx-search" :placeholder="tr('ANNUAL, SICK, UNPAID')" :disabled="!canEdit"
                           @change="f.choices = ($event.target as HTMLInputElement).value.split(',').map((x) => x.trim()).filter(Boolean)">
                    <span v-else class="fx-cell-sub">-</span>
                  </td>
                  <td>
                    <div class="fm-auto">
                      <select v-model="f.auto" class="fx-select" :disabled="!canEdit" :aria-label="tr('Default or automatic value')" @change="f.auto && (f.defaultValue = null)">
                        <option :value="null">{{ tr('Typed value') }}</option>
                        <option v-for="a in autoValues.filter((x) => x.types.includes(f.type))" :key="a.value" :value="a.value">{{ tr(a.label) }}</option>
                      </select>
                      <input v-if="!f.auto" v-model="f.defaultValue" class="fx-search" :placeholder="tr('optional, or @UserName, @Today')" :disabled="!canEdit">
                      <span v-else class="fm-auto-tag"><span class="pi pi-lock" aria-hidden="true"></span> {{ tr('Read-only, filled in for the user') }}</span>
                    </div>
                  </td>
                  <td class="fm-c"><input v-model="f.required" type="checkbox" class="fx-check" :disabled="!canEdit" :aria-label="tr('Required')"></td>
                  <td><button v-if="canEdit" type="button" class="fm-tool" :aria-label="tr('Remove')" @click="removeField(i)"><span class="fa fa-trash-o" aria-hidden="true"></span></button></td>
                </tr>
              </tbody>
            </table>
          </div>
          <div v-if="!edit.fields.length" class="fm-hint">{{ tr('No fields yet. Add the first one.') }}</div>
          <button v-if="canEdit" type="button" class="fx-btn fx-btn--sm fm-add-field" @click="addDocField"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Add field') }}</button>
        </div>

        <div class="fm-panes">
          <!-- controls -->
          <div class="fx-card fx-card-pad">
            <div class="fm-pane-head">
              <h2>{{ tr('Layout') }}</h2>
              <span class="fx-cell-sub">{{ tr('Drag the boxes to arrange the document') }}</span>
            </div>

            <p v-if="!fields.length" class="fm-hint">{{ tr('Add the document\'s fields first: each one becomes a control here.') }}</p>

            <!-- the canvas: drag a control up, down, or onto the left or right half of another; the arrows do the same -->
            <div class="fm-canvas" :class="{ two: edit.columns === 2, dragging: dragFrom >= 0 }" @dragover.prevent>
              <div v-for="(c, i) in edit.controls" :key="i" class="fm-card"
                   :class="['w-' + cardWidth(c), { sel: openIndex === i, ghost: dragFrom === i, ['drop-' + dropSide]: dropAt === i && dragFrom !== i }]"
                   :draggable="canEdit" @dragstart="dragStart(i, $event)" @dragend="dragEnd"
                   @dragover.prevent="dragOver(i, $event)" @drop.prevent="dropOn(i)" @click="openIndex = openIndex === i ? -1 : i">
                <span class="fm-card-grip fa fa-arrows" aria-hidden="true"></span>
                <span class="fm-card-icon fa" :class="iconOf(c)" aria-hidden="true"></span>
                <span class="fm-card-text"><b>{{ titleOf(c) }}</b><small>{{ subOf(c) }}</small></span>
                <span v-if="c.kind === 'field'" class="fm-card-mock" aria-hidden="true"></span>
              </div>
              <div v-if="canEdit && edit.controls.length" class="fm-card-end" :class="{ on: dropAt === edit.controls.length }"
                   @dragover.prevent="dropAt = edit.controls.length; dropSide = 'below'" @drop.prevent="dropOn(edit.controls.length)">
                {{ tr('Drop here to put it last') }}
              </div>
            </div>
            <p v-if="canEdit && edit.controls.length" class="fm-hint fm-canvas-hint">
              <span class="fa fa-hand-pointer-o" aria-hidden="true"></span>
              {{ tr('Drag a box above or below another to move it; onto the left or right half of another to sit beside it. Click a box for its settings.') }}
            </p>

            <!-- the selected control: move it with the arrows, change its settings -->
            <div v-for="c in selectedControl" :key="edit.controls.indexOf(c)" class="fm-control open">
              <div class="fm-control-head">
                <span class="fm-control-icon fa" :class="iconOf(c)" aria-hidden="true"></span>
                <div class="fm-control-title"><b>{{ titleOf(c) }}</b><small>{{ subOf(c) }}</small></div>
                <span v-if="canEdit" class="fm-control-tools">
                  <button type="button" class="fm-tool" :disabled="openIndex === 0" :aria-label="tr('Move up')" :title="tr('Move up')" @click="move(openIndex, -1)"><span class="fa fa-arrow-up" aria-hidden="true"></span></button>
                  <button type="button" class="fm-tool" :disabled="openIndex === edit.controls.length - 1" :aria-label="tr('Move down')" :title="tr('Move down')" @click="move(openIndex, 1)"><span class="fa fa-arrow-down" aria-hidden="true"></span></button>
                  <template v-if="c.kind === 'field'">
                    <button type="button" class="fm-tool" :aria-pressed="c.width === 'left'" :aria-label="tr('Left half')" :title="tr('Left half')" @click="place(c, 'left')"><span class="fa fa-arrow-left" aria-hidden="true"></span></button>
                    <button type="button" class="fm-tool" :aria-pressed="c.width === 'right'" :aria-label="tr('Right half')" :title="tr('Right half')" @click="place(c, 'right')"><span class="fa fa-arrow-right" aria-hidden="true"></span></button>
                    <button type="button" class="fm-tool" :aria-pressed="(c.width ?? 'full') === 'full'" :aria-label="tr('Whole row')" :title="tr('Whole row')" @click="place(c, 'full')"><span class="fa fa-arrows-h" aria-hidden="true"></span></button>
                  </template>
                  <button type="button" class="fm-tool" :aria-label="tr('Remove')" :title="tr('Remove')" @click="removeControl(openIndex)"><span class="fa fa-trash-o" aria-hidden="true"></span></button>
                </span>
              </div>
                <div class="fm-control-body">
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
                      <option value="left">{{ tr('Left half') }}</option>
                      <option value="right">{{ tr('Right half') }}</option>
                    </select>
                  </label>
                  <label class="fm-field fm-full">
                    <span>{{ tr('Help text (optional)') }}</span>
                    <input v-model="c.help" class="fx-search" :placeholder="fieldOf(c)?.description ?? ''" :disabled="!canEdit">
                  </label>
                  <div class="fm-flags fm-full">
                    <label class="fm-check"><input v-model="c.required" type="checkbox" class="fx-check" :disabled="!canEdit || c.readOnly || fieldOf(c)?.required"> <span>{{ tr('Required') }}</span>
                      <small v-if="fieldOf(c)?.required">{{ tr('(the field is required)') }}</small></label>
                    <label class="fm-check"><input v-model="c.readOnly" type="checkbox" class="fx-check" :disabled="!canEdit" @change="c.readOnly && (c.required = false)"> <span>{{ tr('Read-only') }}</span>
                      <small>{{ tr('shown, not changed') }}</small></label>
                  </div>
                </template>
                <label v-else class="fm-field fm-full">
                  <span>{{ c.kind === 'heading' ? tr('Heading text') : tr('Note text') }}</span>
                  <input v-model="c.label" class="fx-search" :disabled="!canEdit">
                </label>
              </div>
            </div>

            <div v-if="canEdit" class="fm-add">
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
              <div class="fm-sheet-title">{{ edit.formName || tr('Untitled document') }}</div>
              <WorkflowFormView :view="preview" v-model="previewValues" id-prefix="pv" />
            </div>
          </div>
        </div>

        <div v-if="canEdit" class="fm-actions">
          <template v-if="edit.id">
            <template v-if="confirmDelete">
              <span class="fm-confirm">{{ tr('Delete this document?') }}</span>
              <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="remove">{{ tr('Yes, delete') }}</button>
              <button type="button" class="fx-btn" @click="confirmDelete = false">{{ tr('No') }}</button>
            </template>
            <button v-else type="button" class="fx-btn" @click="confirmDelete = true"><span class="fa fa-trash-o" aria-hidden="true"></span>{{ tr('Delete') }}</button>
          </template>
          <span class="fm-gap"></span>
          <span v-if="problem" class="fm-problem"><span class="fa fa-exclamation-circle" aria-hidden="true"></span> {{ problem }}</span>
          <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !!problem" @click="save">
            <span class="fa fa-check" aria-hidden="true"></span>{{ edit.id ? tr('Save document') : tr('Create document') }}
          </button>
        </div>
      </div>

      <div v-else class="fx-card fx-card-pad fm-blank">
        <span class="fa fa-file-text-o" aria-hidden="true"></span>
        <p>{{ forms.length ? tr('Choose a document to change it, or make a new one.') : tr('Make the first document: add its fields, then lay them out.') }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import { openWorkflowManual } from '../utils/manual'
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import WorkflowFormView from '../components/WorkflowFormView.vue'
import { autoValues,
  apiErrorText, controlName, controlsFor, dataSourceApi, defaultControl, fieldTypes, formApi,
  type DataField, type DataSource, type DocumentField, type FormControl, type FormControlView, type FormView, type Option, type WorkflowForm
} from '../services/formService'

const route = useRoute()
const canEdit = hasPermission('WFE.DEFINITION.EDIT')

const forms = ref<WorkflowForm[]>([])
const sources = ref<DataSource[]>([])
const options = ref<Record<string, Option[]>>({})
const editing = ref(false)
const openIndex = ref(-1)
const busy = ref(false)
const confirmDelete = ref(false)
const message = ref<{ text: string; bad: boolean } | null>(null)
const previewValues = ref<Record<string, string | null>>({})

const blank = () => ({ id: 0, formCode: '', formName: '', description: '', isActive: true, columns: 1 as 1 | 2, controls: [] as FormControl[], fields: [] as DocumentField[], rowVersion: 0 })
const edit = reactive(blank())
const usedAt = ref<string[]>([])
const legacyOf = ref('')

/** The document's fields as the layout and preview use them (with their caption). */
const fields = computed<DataField[]>(() => edit.fields.filter((f) => f.name.trim()).map((f) => ({ ...f, name: f.name.trim(), caption: f.label?.trim() || f.name.trim() })))

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
  if (!f) return tr('Not a field of this document any more')
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
  if (!edit.formName.trim()) return tr('Give the document a name.')
  if (!/^[A-Z][A-Z0-9_]{1,49}$/.test(edit.formCode)) return tr('Give the document a code: capital letters, digits or _.')
  if (!edit.fields.length) return tr('Add the document\'s fields.')
  const unnamed = edit.fields.findIndex((f) => !/^[A-Za-z][A-Za-z0-9_]{0,49}$/.test(f.name.trim()))
  if (unnamed >= 0) return `${tr('Field')} ${unnamed + 1}: ${tr('give it a name of letters, digits or _.')}`
  const names = edit.fields.map((f) => f.name.trim().toLowerCase())
  const twice = names.find((n, i) => names.indexOf(n) !== i)
  if (twice) return `${twice}: ${tr('the field is listed twice.')}`
  const noChoices = edit.fields.find((f) => f.type === 'Choice' && !f.choices.length)
  if (noChoices) return `${noChoices.label || noChoices.name}: ${tr('list its choices.')}`
  const bad = edit.controls.find((c) => c.kind === 'field' && c.control === 'dropdown' && !c.dataSource && !fieldOf(c)?.choices.length)
  if (bad) return `${titleOf(bad)}: ${tr('choose the data source of the drop-down.')}`
  const lost = edit.controls.find((c) => c.kind === 'field' && !fieldOf(c))
  if (lost) return `${lost.field}: ${tr('not a field of this document any more. Remove it.')}`
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
    return [{ kind: 'field', field: f.name, label: c.label || f.caption, control, fieldType: f.type, required: !c.readOnly && !f.auto && (c.required || f.required), readOnly: c.readOnly || !!f.auto, help: c.help || f.description || (f.auto ? tr('Filled in automatically') : null), width: c.width ?? 'full', value: null, options: opts }]
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

function newForm() {
  Object.assign(edit, blank())
  usedAt.value = []
  legacyOf.value = ''
  fieldNames.value = []
  editing.value = true
  openIndex.value = -1
  confirmDelete.value = false
  previewValues.value = {}
}

function open(f: WorkflowForm) {
  Object.assign(edit, {
    id: f.formId, formCode: f.formCode, formName: f.formName, description: f.description ?? '',
    isActive: f.isActive, columns: f.layout.columns === 2 ? 2 : 1, controls: f.layout.controls.map((c) => ({ ...c, width: c.width ?? 'full' })),
    fields: f.fields.map((x) => ({ name: x.name, label: x.label, type: x.type, required: x.required, defaultValue: x.defaultValue, choices: [...x.choices], description: x.description, auto: x.auto ?? null })),
    rowVersion: f.rowVersion
  })
  usedAt.value = f.usedAt
  legacyOf.value = f.isDocument ? '' : f.workflowName ?? ''
  fieldNames.value = edit.fields.map((x) => x.name.trim())
  editing.value = true
  openIndex.value = -1
  confirmDelete.value = false
  previewValues.value = {}
}

function addField(f: DataField) {
  edit.controls.push({ kind: 'field', field: f.name, label: null, control: defaultControl(f.type), dataSource: null, required: f.required, readOnly: false, help: null, width: 'full' })
}
/** A new field goes on the layout too, at the end. */
function addDocField() {
  edit.fields.push({ name: '', label: '', type: 'String', required: false, defaultValue: null, choices: [], description: null })
}
function nameFromLabel(f: DocumentField, i: number) {
  if (f.name.trim() || !f.label?.trim()) return
  const words = f.label.trim().replace(/[^A-Za-z0-9 ]/g, ' ').split(/\s+/).filter(Boolean)
  const name = words.map((w, k) => (k === 0 ? w.toLowerCase() : w.charAt(0).toUpperCase() + w.slice(1).toLowerCase())).join('')
  f.name = /^[A-Za-z]/.test(name) ? name.slice(0, 50) : `f${name}`.slice(0, 50)
  renameField(i)
}
// the layout follows the fields: a named field gets its control, a renamed one keeps it
const fieldNames = ref<string[]>([])
function renameField(i: number) {
  const f = edit.fields[i]
  if (!f) return
  const before = fieldNames.value[i] ?? ''
  const now = f.name.trim()
  const c = before ? edit.controls.find((x) => x.kind === 'field' && x.field?.toLowerCase() === before.toLowerCase()) : undefined
  if (c) c.field = now
  else if (now && !edit.controls.some((x) => x.kind === 'field' && x.field?.toLowerCase() === now.toLowerCase()))
    edit.controls.push({ kind: 'field', field: now, label: null, control: defaultControl(f.type), dataSource: null, required: false, readOnly: false, help: null, width: 'full' })
  fieldNames.value = edit.fields.map((x) => x.name.trim())
}
function typeChanged(f: DocumentField) {
  if (f.type !== 'Choice') f.choices = []
  // an automatic value that does not fit the new type is dropped
  if (f.auto && !autoValues.find((a) => a.value === f.auto)?.types.includes(f.type)) f.auto = null
  const c = edit.controls.find((x) => x.kind === 'field' && x.field?.toLowerCase() === f.name.trim().toLowerCase())
  if (c && !controlsFor(f.type).includes(c.control ?? 'text')) { c.control = defaultControl(f.type); c.dataSource = null }
}
function removeField(i: number) {
  const name = edit.fields[i]?.name.trim().toLowerCase()
  edit.fields.splice(i, 1)
  if (name) edit.controls = edit.controls.filter((c) => !(c.kind === 'field' && c.field?.toLowerCase() === name))
  fieldNames.value = edit.fields.map((x) => x.name.trim())
  openIndex.value = -1
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
// ---------- the layout canvas ----------
const selectedControl = computed(() => { const c = openIndex.value >= 0 ? edit.controls[openIndex.value] : undefined; return c ? [c] : [] })
const dragFrom = ref(-1)
const dropAt = ref(-1)
const dropSide = ref<'above' | 'below' | 'left' | 'right'>('below')
function cardWidth(c: FormControl) {
  if (edit.columns !== 2 || c.kind !== 'field') return 'full'
  return c.width === 'left' || c.width === 'right' || c.width === 'half' ? c.width : 'full'
}
function dragStart(i: number, e: DragEvent) {
  dragFrom.value = i
  e.dataTransfer?.setData('text/plain', String(i))
  if (e.dataTransfer) e.dataTransfer.effectAllowed = 'move'
}
function dragEnd() {
  dragFrom.value = -1
  dropAt.value = -1
}
/** Where the pointer is on the box: its top or bottom quarter puts the control above or below; its left or right half beside it. */
function dragOver(i: number, e: DragEvent) {
  if (dragFrom.value < 0) return
  const r = (e.currentTarget as HTMLElement).getBoundingClientRect()
  const y = (e.clientY - r.top) / r.height
  const x = (e.clientX - r.left) / r.width
  const target = edit.controls[i]
  const dragged = edit.controls[dragFrom.value]
  const side = dragged?.kind === 'field' && target?.kind === 'field' && y > 0.25 && y < 0.75
  dropAt.value = i
  dropSide.value = side ? (x < 0.5 ? 'left' : 'right') : y < 0.5 ? 'above' : 'below'
}
function dropOn(i: number) {
  const from = dragFrom.value
  const side = dropSide.value
  dragEnd()
  if (from < 0 || from === i) return
  const target = i < edit.controls.length ? edit.controls[i] : undefined
  const [c] = edit.controls.splice(from, 1)
  if (!c) return
  if (!target) {
    edit.controls.push(c)
  } else {
    const at = edit.controls.indexOf(target)
    if (side === 'left' || side === 'right') {
      // side by side: a two-column form, the dragged control on the chosen half and the other on the opposite one
      edit.columns = 2
      c.width = side
      target.width = side === 'left' ? 'right' : 'left'
    } else if (c.width === 'left' || c.width === 'right') {
      c.width = 'full'
    }
    edit.controls.splice(side === 'above' || side === 'left' ? at : at + 1, 0, c)
  }
  openIndex.value = edit.controls.indexOf(c)
}
/** The arrow buttons: left or right half (the form becomes two columns) or the whole row. */
function place(c: FormControl, width: 'left' | 'right' | 'full') {
  if (width !== 'full') edit.columns = 2
  c.width = width
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
      formCode: edit.formCode, formName: edit.formName.trim(), workflowDefinitionId: null, description: edit.description || null,
      isActive: edit.isActive, layout: { columns: edit.columns, controls: edit.controls }, rowVersion: edit.rowVersion,
      fields: edit.fields.map((f) => ({ ...f, name: f.name.trim(), label: f.label?.trim() || null, defaultValue: f.defaultValue?.trim() || null }))
    })
    message.value = { bad: false, text: `${tr('Saved')} ${saved.formName}. ${saved.usedAt.length ? `${tr('Used by')} ${saved.usedAt.join(', ')}.` : tr('Link it to a workflow in the workflow designer.')}` }
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
  dataSourceApi.list().then((x) => (sources.value = x.filter((s) => s.isActive))).catch(() => (sources.value = []))
  await load()
  const id = Number(route.query.form)
  const f = forms.value.find((x) => x.formId === id)
  if (f) open(f)
  else if (route.query.new && canEdit) newForm()
})
</script>

<style scoped>
/* the page buttons stay on the right of the title on every workflow page, however long the description */
#fxForms .fx-head { flex-wrap: nowrap; }
#fxForms .fx-head > div:first-child { flex: 1 1 auto; min-width: 0; }
#fxForms .fx-head .fx-actions { flex: none; }
@media (max-width: 900px) { #fxForms .fx-head { flex-wrap: wrap; } }

/* the layout canvas */
.fm-canvas { display: grid; grid-template-columns: minmax(0, 1fr); gap: 8px 10px; padding: 12px; border: 1px dashed #c9dadf; border-radius: 12px; background: repeating-linear-gradient(0deg, #f7fbfb 0 23px, #eef5f6 23px 24px); min-height: 80px; }
.fm-canvas.two { grid-template-columns: repeat(2, minmax(0, 1fr)); }
.fm-canvas > * { grid-column: 1 / -1; min-width: 0; }
.fm-canvas.two > .w-half { grid-column: auto; }
.fm-canvas.two > .w-left { grid-column: 1; }
.fm-canvas.two > .w-right { grid-column: 2; }
.fm-card { position: relative; display: grid; grid-template-columns: auto auto minmax(0, 1fr); grid-template-areas: 'grip icon text' 'grip mock mock'; align-items: center; gap: 4px 8px; padding: 9px 10px; border: 1px solid #d9e6ea; border-radius: 10px; background: #fff; cursor: grab; user-select: none; transition: box-shadow .12s, border-color .12s; }
.fm-card:hover { border-color: #9cc5c9; }
.fm-card.sel { border-color: #00636D; box-shadow: 0 0 0 3px rgba(0, 99, 109, .12); }
.fm-card.ghost { opacity: .4; }
.fm-card-grip { grid-area: grip; align-self: stretch; display: grid; place-items: center; color: #9db3bc; font-size: 12px; }
.fm-card-icon { grid-area: icon; width: 24px; height: 24px; display: grid; place-items: center; border-radius: 6px; background: #e7f3f2; color: #00636D; font-size: 12px; }
.fm-card-text { grid-area: text; display: grid; min-width: 0; }
.fm-card-text b { font-size: 12.5px; color: #12324a; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.fm-card-text small { font-size: 11px; color: #5f7787; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.fm-card-mock { grid-area: mock; height: 22px; border: 1px solid #dbe7eb; border-radius: 6px; background: #fbfdfd; }
.fm-card::after { content: ''; position: absolute; pointer-events: none; border-radius: 3px; background: #E87722; opacity: 0; }
.fm-card.drop-above::after { opacity: 1; left: 0; right: 0; top: -6px; height: 4px; }
.fm-card.drop-below::after { opacity: 1; left: 0; right: 0; bottom: -6px; height: 4px; }
.fm-card.drop-left::after { opacity: 1; top: 0; bottom: 0; left: -6px; width: 4px; }
.fm-card.drop-right::after { opacity: 1; top: 0; bottom: 0; right: -6px; width: 4px; }
.fm-card-end { padding: 8px; border: 1px dashed transparent; border-radius: 8px; color: #8aa2ad; font-size: 11.5px; text-align: center; }
.fm-canvas.dragging .fm-card-end { border-color: #c9dadf; }
.fm-card-end.on { border-color: #E87722; color: #B4570E; background: #FFF7F0; }
.fm-canvas-hint { margin: 8px 0 12px; }
.fm-tool[aria-pressed=true] { background: #e7f3f2; color: #00636D; }

.fm-auto { display: grid; gap: 4px; min-width: 190px; }
.fm-auto-tag { font-size: 11.5px; color: #00636D; }

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
.fm-used { display: flex; flex-wrap: wrap; gap: 6px; min-height: 36px; align-items: center; }
.fm-legacy { margin: 12px 0 0; }
.fm-table-wrap { overflow-x: auto; }
.fm-fields { width: 100%; border-collapse: collapse; font-size: 13px; }
.fm-fields th { text-align: left; color: #5f7787; font-size: 11.5px; font-weight: 700; text-transform: uppercase; letter-spacing: .03em; padding: 0 6px 6px; white-space: nowrap; }
.fm-fields td { padding: 4px 6px; border-top: 1px solid #edf2f4; vertical-align: middle; }
.fm-fields td .fx-search, .fm-fields td .fx-select { width: 100%; min-width: 110px; max-width: 100%; }
.fm-fields .fm-c { text-align: center; width: 70px; }
.fm-add-field { margin-top: 12px; }
.fm-blank { display: grid; justify-items: center; gap: 6px; padding: 48px 18px; color: #5f7787; text-align: center; }
.fm-blank .fa { font-size: 28px; color: #9fbfc4; }
@media (max-width: 1200px) { .fm-panes { grid-template-columns: minmax(0, 1fr); } .fm-preview { position: static; } }
@media (max-width: 900px) { .fm-grid { grid-template-columns: minmax(0, 1fr); } .fm-settings { grid-template-columns: minmax(0, 1fr); } .fm-control-body { grid-template-columns: minmax(0, 1fr); padding-left: 12px; } }
</style>
