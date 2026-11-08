// Workflow forms and data sources (K2's SmartForms and SmartObjects, kept simple): api/wfe/forms and api/wfe/data-sources.
import { ApiError, http } from '@/shared/api/http'

export type FormControlKind = 'field' | 'heading' | 'note'
export type FormControlType = 'text' | 'textarea' | 'number' | 'amount' | 'date' | 'yesno' | 'dropdown' | 'user'

export type FormControl = {
  kind: FormControlKind
  field: string | null
  label: string | null
  control: FormControlType | null
  dataSource: string | null
  required: boolean
  readOnly: boolean
  help: string | null
  width: 'full' | 'half' | null
}

export type FormLayout = { columns: 1 | 2; controls: FormControl[] }

export type WorkflowForm = {
  formId: number
  formCode: string
  formName: string
  workflowDefinitionId: number
  workflowCode: string
  workflowName: string
  description: string | null
  isActive: boolean
  controlCount: number
  usedAt: string[]
  layout: FormLayout
  rowVersion: number
}

export type SaveWorkflowForm = {
  formCode: string
  formName: string
  workflowDefinitionId: number
  description: string | null
  isActive: boolean
  layout: FormLayout
  rowVersion: number
}

export type DataField = {
  name: string
  label: string | null
  type: string
  required: boolean
  defaultValue: string | null
  choices: string[]
  description: string | null
  caption: string
}

export type Option = { value: string; label: string }

/** A form ready to show: each control with its value and options. */
export type FormView = {
  formCode: string
  formName: string
  description: string | null
  columns: number
  editable: boolean
  controls: FormControlView[]
}

export type FormControlView = {
  kind: FormControlKind
  field: string | null
  label: string
  control: string
  fieldType: string | null
  required: boolean
  readOnly: boolean
  help: string | null
  width: string
  value: string | null
  options: Option[]
}

export type DataSourceItem = { value: string; label: string; isActive: boolean }

export type DataSource = {
  dataSourceId: number | null
  sourceCode: string
  sourceName: string
  description: string | null
  isActive: boolean
  builtIn: boolean
  itemCount: number
  items: DataSourceItem[]
  rowVersion: number
}

export type StartFormItem = { workflowCode: string; workflowName: string; description: string | null; formCode: string; formName: string }
export type Started = { instanceId: number; nextStep: string | null; nextAssignee: string | null; inboxId: number | null }

/** The controls each data field type can use; the first is the default. */
export function controlsFor(type: string): FormControlType[] {
  switch (type) {
    case 'Number': return ['number', 'dropdown']
    case 'Decimal': return ['amount']
    case 'Boolean': return ['yesno']
    case 'Date': return ['date']
    case 'Choice': return ['dropdown']
    case 'User': return ['user']
    default: return ['text', 'textarea', 'dropdown']
  }
}

export function defaultControl(type: string): FormControlType {
  return controlsFor(type)[0] ?? 'text'
}

export function controlName(t: string) {
  return controlNames[t] ?? t
}

const controlNames: Record<string, string> = {
  text: 'Text box', textarea: 'Text area', number: 'Number', amount: 'Amount', date: 'Date picker', yesno: 'Yes / No', dropdown: 'Drop-down', user: 'User picker'
}

/** An API error as one line: validation messages joined, else the problem's text. */
export function apiErrorText(e: unknown, fallback = 'Something went wrong.') {
  if (e instanceof ApiError && e.problem?.errors) return Object.values(e.problem.errors).flat().join(' ')
  return e instanceof Error ? e.message : fallback
}

/** Validation messages per field, for showing them beside the form's controls. */
export function apiFieldErrors(e: unknown): Record<string, string> {
  if (!(e instanceof ApiError) || !e.problem?.errors) return {}
  return Object.fromEntries(Object.entries(e.problem.errors).map(([k, v]) => [k.toLowerCase(), v.join(' ')]))
}

const FORMS = '/api/wfe/forms'
const SOURCES = '/api/wfe/data-sources'

export const formApi = {
  list: () => http.get<WorkflowForm[]>(FORMS),
  get: (id: number) => http.get<WorkflowForm>(`${FORMS}/${id}`),
  ofWorkflow: (definitionId: number) => http.get<WorkflowForm[]>(`${FORMS}/workflow/${definitionId}`),
  fields: (definitionId: number) => http.get<DataField[]>(`${FORMS}/workflow/${definitionId}/fields`),
  save: (id: number | null, body: SaveWorkflowForm) => (id ? http.put<WorkflowForm>(`${FORMS}/${id}`, body) : http.post<WorkflowForm>(FORMS, body)),
  remove: (id: number) => http.delete<void>(`${FORMS}/${id}`),
  startList: () => http.get<StartFormItem[]>(`${FORMS}/start`),
  startForm: (workflowCode: string) => http.get<FormView>(`${FORMS}/start/${encodeURIComponent(workflowCode)}`),
  start: (workflowCode: string, businessReference: string | null, values: Record<string, string | null>) =>
    http.post<Started>(`${FORMS}/start/${encodeURIComponent(workflowCode)}`, { businessReference, values }),
  taskForm: (inboxId: number) => http.get<{ form: FormView | null }>(`/api/wfe/inbox/${inboxId}/form`)
}

export const dataSourceApi = {
  list: () => http.get<DataSource[]>(SOURCES),
  get: (code: string) => http.get<DataSource>(`${SOURCES}/${encodeURIComponent(code)}`),
  options: (code: string) => http.get<Option[]>(`${SOURCES}/${encodeURIComponent(code)}/options`),
  save: (id: number | null, body: Omit<DataSource, 'dataSourceId' | 'builtIn' | 'itemCount'>) =>
    (id ? http.put<DataSource>(`${SOURCES}/${id}`, body) : http.post<DataSource>(SOURCES, body)),
  remove: (id: number) => http.delete<void>(`${SOURCES}/${id}`)
}
