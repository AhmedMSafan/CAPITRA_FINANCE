// Workflow definitions for the BPMN designer, from api/wfe/definitions.
// The designer pages keep the field names of the FINOVA_FINANCEING engine (dgrm*), so they are mapped here.
import { http } from '@/shared/api/http'

const API_URL = '/api/wfe/definitions'

type WfeDefinitionDto = {
  workflowDefinitionId: number
  workflowCode: string
  workflowName: string
  versionNo: string
  definitionStatus: string
  bpmnXml: string
  description: string | null
  isActive: boolean
  createdBy: string
  createdAt: string
  updatedBy: string | null
  updatedAt: string | null
  rowVersion: number
}

export type WorkflowDefinition = {
  dgrmId: number
  dgrmName: string
  dgrmVersion: string
  dgrmStatus: string
  dgrmContent: string
  dgrmCreatedBy: string
  dgrmCreatedOn: string
  dgrmLastUpdateBy: string | null
  dgrmLastUpdateOn: string | null
  workflowCode: string
  description: string | null
  isActive: 'Y' | 'N'
  rowVersion: number
}

function fromApi(x: WfeDefinitionDto): WorkflowDefinition {
  return {
    dgrmId: x.workflowDefinitionId,
    dgrmName: x.workflowName,
    dgrmVersion: x.versionNo,
    dgrmStatus: x.definitionStatus,
    dgrmContent: x.bpmnXml,
    dgrmCreatedBy: x.createdBy,
    dgrmCreatedOn: x.createdAt,
    dgrmLastUpdateBy: x.updatedBy,
    dgrmLastUpdateOn: x.updatedAt,
    workflowCode: x.workflowCode,
    description: x.description,
    isActive: x.isActive ? 'Y' : 'N',
    rowVersion: x.rowVersion
  }
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function toApi(model: any) {
  return {
    workflowCode: model.workflowCode,
    workflowName: model.dgrmName,
    versionNo: model.dgrmVersion,
    definitionStatus: model.dgrmStatus,
    bpmnXml: model.dgrmContent,
    description: model.description,
    isActive: model.isActive === 'Y',
    rowVersion: model.rowVersion ?? 0
  }
}

export async function getWorkflowDefinitions(search = '') {
  const rows = await http.get<WfeDefinitionDto[]>(search ? `${API_URL}?search=${encodeURIComponent(search)}` : API_URL)
  return rows.map(fromApi)
}

export async function getWorkflowDefinition(id: number) {
  return fromApi(await http.get<WfeDefinitionDto>(`${API_URL}/${id}`))
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export async function createWorkflowDefinition(model: any) {
  return fromApi(await http.post<WfeDefinitionDto>(API_URL, toApi(model)))
}

// eslint-disable-next-line @typescript-eslint/no-explicit-any
export async function updateWorkflowDefinition(id: number, model: any) {
  return fromApi(await http.put<WfeDefinitionDto>(`${API_URL}/${id}`, toApi(model)))
}

export async function deleteWorkflowDefinition(id: number) {
  await http.delete<void>(`${API_URL}/${id}`)
}

export type RunningStep = { elementId: string; stepName: string | null; stepStatus: string; outcome: string | null; enteredAt: string; completedAt: string | null; completedBy: string | null }
export type RunningInstance = {
  instanceId: number
  workflowDefinitionId: number
  workflowName: string
  versionNo: string
  businessReference: string | null
  instanceStatus: string
  currentElementId: string | null
  startedAt: string
  startedBy: string
  lastActivity: string
  steps: RunningStep[]
}

/** Instances still running, most recently active first, with their step history. */
export function getRunningInstances() {
  return http.get<RunningInstance[]>(`${API_URL}/running`)
}
