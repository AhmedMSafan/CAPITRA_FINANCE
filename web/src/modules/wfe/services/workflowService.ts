// My Inbox: tasks of the signed-in user, from api/wfe/inbox.
import { http } from '@/shared/api/http'
import type { CompleteWorkflowTaskRequest, WorkflowInboxTask } from '../models/workflow'

const INBOX_API = '/api/wfe/inbox'

export const workflowService = {
  getMyInbox: () => http.get<WorkflowInboxTask[]>(`${INBOX_API}/my`),
  // every open task of every user (Workflow Engine home)
  getAllOpen: () => http.get<WorkflowInboxTask[]>(`${INBOX_API}/all`),
  getTask: (id: number) => http.get<WorkflowInboxTask>(`${INBOX_API}/${id}`),
  claim: (id: number) => http.post<WorkflowInboxTask>(`${INBOX_API}/${id}/claim`),
  start: (id: number) => http.post<WorkflowInboxTask>(`${INBOX_API}/${id}/start`),
  complete: (id: number, payload: CompleteWorkflowTaskRequest) => http.post<WorkflowInboxTask>(`${INBOX_API}/${id}/complete`, payload)
}
