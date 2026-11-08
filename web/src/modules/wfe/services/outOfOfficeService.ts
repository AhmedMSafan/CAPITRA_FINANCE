// Out of office (K2-style): api/wfe/out-of-office.
import { http } from '@/shared/api/http'

const API = '/api/wfe/out-of-office'

/** AWAY = today is within the dates, PLANNED = later, ENDED = past, OFF = switched off. */
export type OutOfOffice = {
  outOfOfficeId: number
  userName: string
  userFullName: string | null
  delegateTo: string
  delegateFullName: string | null
  workflowCode: string | null
  workflowName: string | null
  fromDate: string
  toDate: string
  isActive: boolean
  note: string | null
  state: 'AWAY' | 'PLANNED' | 'ENDED' | 'OFF'
  rowVersion: number
}

export type SaveOutOfOffice = {
  userName: string | null
  delegateTo: string
  workflowCode: string | null
  fromDate: string
  toDate: string
  isActive: boolean
  note: string | null
  rowVersion: number
}

export type CoverInfo = { userName: string; fullName: string | null; workflowCode: string | null; toDate: string }
export type UserOption = { userName: string; fullName: string | null }
export type WorkflowOption = { workflowCode: string; workflowName: string }

export const outOfOfficeApi = {
  mine: () => http.get<OutOfOffice[]>(API),
  all: () => http.get<OutOfOffice[]>(`${API}/all`),
  covering: () => http.get<CoverInfo[]>(`${API}/covering`),
  users: () => http.get<UserOption[]>(`${API}/users`),
  workflows: () => http.get<WorkflowOption[]>(`${API}/workflows`),
  save: (id: number | null, body: SaveOutOfOffice, admin = false) =>
    id ? http.put<OutOfOffice>(`${API}${admin ? '/admin' : ''}/${id}`, body) : http.post<OutOfOffice>(`${API}${admin ? '/admin' : ''}`, body),
  remove: (id: number, admin = false) => http.delete<void>(`${API}${admin ? '/admin' : ''}/${id}`)
}
