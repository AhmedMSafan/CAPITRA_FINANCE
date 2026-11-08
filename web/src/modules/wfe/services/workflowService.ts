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

export type WorkflowDataValue = { name: string; label: string; type: string; required: boolean; value: string | null; description: string | null }
export type WorkflowInstanceData = { fields: WorkflowDataValue[]; other: WorkflowDataValue[] }
