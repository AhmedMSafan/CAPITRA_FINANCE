// Documents (Document Designer) and data sources (K2's SmartForms and SmartObjects, kept simple): api/wfe/forms and api/wfe/data-sources.
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
  width: 'full' | 'half' | 'left' | 'right' | null
}

export type FormLayout = { columns: 1 | 2; controls: FormControl[] }

/** A document: its own fields and layout. An older form (made before documents) uses one workflow's data fields. */
export type WorkflowForm = {
  formId: number
  formCode: string
  formName: string
  workflowDefinitionId: number | null
  workflowCode: string | null
  workflowName: string | null
  description: string | null
  isActive: boolean
  controlCount: number
  usedAt: string[]
  fields: DataField[]
  layout: FormLayout
  rowVersion: number
  isDocument: boolean
}

export type SaveWorkflowForm = {
  formCode: string
  formName: string
  workflowDefinitionId: number | null
  description: string | null
  isActive: boolean
  layout: FormLayout
  rowVersion: number
  fields: DocumentField[]
}

/** A document field as the Document Designer saves it. */
export type DocumentField = {
  name: string
  label: string | null
  type: string
  required: boolean
  defaultValue: string | null
  choices: string[]
  description: string | null
  /** Filled in by the system, read-only: CURRENT_USER, CURRENT_USER_NAME, CURRENT_DEPARTMENT, CURRENT_EMPLOYEE_NO or TODAY. */
  auto?: string | null
}

/** The automatic values a document field can take, and the field types each fits. */
export const autoValues = [
  { value: 'CURRENT_USER', label: 'Current user (@UserName)', types: ['User', 'String'] },
  { value: 'CURRENT_USER_NAME', label: 'Full name (@FullName)', types: ['String'] },
  { value: 'CURRENT_DEPARTMENT', label: 'Department (@Department)', types: ['String'] },
  { value: 'CURRENT_EMPLOYEE_NO', label: 'Employee no. (@EmployeeNo)', types: ['String'] },
  { value: 'TODAY', label: "Today's date (@Today)", types: ['Date', 'String'] }
]

export type WorkflowType = 'SYSTEM' | 'USER'
export type WorkflowTypeItem = { workflowDefinitionId: number; workflowCode: string; workflowName: string; type: WorkflowType; documentCode: string | null }

export const fieldTypes = [
  { value: 'String', label: 'Text' },
  { value: 'Number', label: 'Whole number' },
  { value: 'Decimal', label: 'Amount' },
  { value: 'Date', label: 'Date' },
  { value: 'Boolean', label: 'Yes / No' },
  { value: 'Choice', label: 'Choice list' },
  { value: 'User', label: 'User' }
]

export type DataField = {
  name: string
  label: string | null
  type: string
  required: boolean
  defaultValue: string | null
  choices: string[]
  description: string | null
  auto?: string | null
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
  /** a user-defined workflow's document as the requester sent it: read-only for approvers */
  isRequest?: boolean
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
export type MyRequest = {
  instanceId: number
  workflowCode: string
  workflowName: string
  businessReference: string | null
  startedAt: string
  status: 'IN_PROGRESS' | 'APPROVED' | 'REJECTED' | 'COMPLETED' | 'FAULTED'
  currentStep: string | null
  lastAction: string | null
  lastActionBy: string | null
  lastActionAt: string | null
  comment: string | null
}
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
  workflows: () => http.get<WorkflowTypeItem[]>(`${FORMS}/workflows`),
  ofWorkflow: (definitionId: number) => http.get<WorkflowForm[]>(`${FORMS}/workflow/${definitionId}`),
  fields: (definitionId: number) => http.get<DataField[]>(`${FORMS}/workflow/${definitionId}/fields`),
  save: (id: number | null, body: SaveWorkflowForm) => (id ? http.put<WorkflowForm>(`${FORMS}/${id}`, body) : http.post<WorkflowForm>(FORMS, body)),
  remove: (id: number) => http.delete<void>(`${FORMS}/${id}`),
  startList: () => http.get<StartFormItem[]>(`${FORMS}/start`),
  myRequests: () => http.get<MyRequest[]>(`${FORMS}/start/mine`),
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
