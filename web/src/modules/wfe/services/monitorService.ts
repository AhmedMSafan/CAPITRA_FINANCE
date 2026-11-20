// The process monitor, from api/wfe/monitor: requests and where they wait, one request's process view, time per step and person.
import { http } from '@/shared/api/http'

const API = '/api/wfe/monitor'

export type MonitorRow = {
  instanceId: number
  workflowDefinitionId: number
  workflowName: string
  businessReference: string | null
  status: string
  currentStep: string | null
  waitingFor: string | null
  startedAt: string
  startedBy: string
  completedAt: string | null
  waitingHours: number | null
  overdue: boolean
  totalHours: number
}

export type MonitorTask = { assignedTo: string | null; status: string; claimedBy: string | null; outcome: string | null; dueAt: string | null; completedAt: string | null }
export type MonitorStep = {
  elementId: string
  stepName: string | null
  elementType: string
  status: string
  outcome: string | null
  enteredAt: string
  completedAt: string | null
  completedBy: string | null
  hours: number
  tasks: MonitorTask[]
}
export type MonitorEvent = { at: string; eventType: string; elementId: string | null; action: string | null; by: string | null; details: string | null }
export type MonitorInstance = {
  summary: MonitorRow
  bpmnXml: string
  steps: MonitorStep[]
  history: MonitorEvent[]
  data: { name: string; label: string; type: string; required: boolean; value: string | null }[]
}

export type StepTime = {
  workflowDefinitionId: number
  workflowName: string
  elementId: string
  stepName: string
  completed: number
  averageHours: number
  medianHours: number
  longestHours: number
  waiting: number
  overdue: number
  late: number
}
export type UserTime = { userName: string; completed: number; averageHours: number; longestHours: number; late: number; open: number }
export type TimeReport = { from: string; to: string; steps: StepTime[]; people: UserTime[] }

function query(p: Record<string, string | number | null | undefined>) {
  const q = new URLSearchParams()
  for (const [k, v] of Object.entries(p)) if (v !== null && v !== undefined && v !== '') q.set(k, String(v))
  const s = q.toString()
  return s ? `?${s}` : ''
}

export const monitorApi = {
  list: (status: string, workflowId?: number | null, search?: string) =>
    http.get<MonitorRow[]>(`${API}/instances${query({ status, workflowId, search })}`),
  instance: (id: number) => http.get<MonitorInstance>(`${API}/instances/${id}`),
  times: (from?: string, to?: string, workflowId?: number | null) =>
    http.get<TimeReport>(`${API}/times${query({ from, to, workflowId })}`),
}

/** Hours as people read them: 45 min, 3.5 h, 2 d 4 h. */
export function duration(h: number | null | undefined) {
  if (h === null || h === undefined) return '-'
  if (h < 1) return `${Math.max(1, Math.round(h * 60))} min`
  if (h < 24) return `${Math.round(h * 10) / 10} h`
  const d = Math.floor(h / 24), r = Math.round(h - d * 24)
  return r ? `${d} d ${r} h` : `${d} d`
}
