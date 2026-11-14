// My Inbox: tasks of the signed-in user, from api/wfe/inbox.
import { http } from '@/shared/api/http'
import type { CompleteWorkflowTaskRequest, WorkflowInboxTask } from '../models/workflow'

const INBOX_API = '/api/wfe/inbox'

export const workflowService = {
  getMyInbox: () => http.get<WorkflowInboxTask[]>(`${INBOX_API}/my`),
  // every open task of every user (Workflow Engine home)
  getAllOpen: () => http.get<WorkflowInboxTask[]>(`${INBOX_API}/all`),
  getTask: (id: number) => http.get<WorkflowInboxTask>(`${INBOX_API}/${id}`),
  /** The workflow data of the task's instance: the declared fields, then other stored values. */
  getTaskData: (id: number) => http.get<WorkflowInstanceData>(`${INBOX_API}/${id}/data`),
  claim: (id: number) => http.post<WorkflowInboxTask>(`${INBOX_API}/${id}/claim`),
  start: (id: number) => http.post<WorkflowInboxTask>(`${INBOX_API}/${id}/start`),
  complete: (id: number, payload: CompleteWorkflowTaskRequest) => http.post<WorkflowInboxTask>(`${INBOX_API}/${id}/complete`, payload)
}

/** A task-deadline alert on the bell in the page header. */
export type WorkflowAlert = { alertId: number; alertType: 'REMINDER' | 'OVERDUE' | 'ESCALATED'; message: string; inboxId: number | null; instanceId: number | null; alertAt: string; isRead: boolean }

const ALERTS_API = '/api/wfe/alerts'
export const alertService = {
  mine: () => http.get<WorkflowAlert[]>(ALERTS_API),
  read: (id: number) => http.post<void>(`${ALERTS_API}/${id}/read`),
  readAll: () => http.post<void>(`${ALERTS_API}/read-all`)
}

export type WorkflowDataValue = { name: string; label: string; type: string; required: boolean; value: string | null; description: string | null }
export type WorkflowInstanceData = { fields: WorkflowDataValue[]; other: WorkflowDataValue[] }
