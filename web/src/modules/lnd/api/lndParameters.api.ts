import { http } from '@/shared/api/http'

/** A value waiting for a second user's approval. */
export type LndPendingValue = {
  parameterValueId: number
  value: string
  valueLabel: string
  effectiveFromDate: string
  decisionReference: string | null
  reason: string
  requestedBy: string
  requestedAt: string
  canApprove: boolean
  approveBlockedReason: string | null
}

/** One Retail Lending parameter for one licence: the value in force today, the next one if set, and a request if one waits. */
export type LndParameter = {
  code: string
  group: string
  label: string
  help: string
  type: 'BOOL' | 'INT' | 'DECIMAL' | 'CHOICE'
  isLegal: boolean
  min: number | null
  max: number | null
  unit: string | null
  suggestedValue: string | null
  suggestedLabel: string | null
  source: string | null
  value: string | null
  valueLabel: string | null
  effectiveFromDate: string | null
  decisionReference: string | null
  approvedBy: string | null
  approvedAt: string | null
  nextValueLabel: string | null
  nextEffectiveFromDate: string | null
  pending: LndPendingValue | null
}

export type LndScope = {
  code: 'MF' | 'CF'
  title: string
  description: string
  icon: string
  pendingCount: number
  /** The go-live check: empty means the licence can lend. */
  problems: string[]
  parameters: LndParameter[]
}

export type LndParameterHistory = {
  parameterValueId: number; code: string; label: string; scopeCode: string; valueLabel: string; effectiveFromDate: string
  decisionReference: string | null; statusCode: 'PENDING' | 'APPROVED' | 'REJECTED' | 'WITHDRAWN'; reason: string
  requestedBy: string; requestedAt: string; decidedBy: string | null; decidedAt: string | null; decisionNotes: string | null
}

export type LndParametersPage = { today: string; canEdit: boolean; canApprove: boolean; scopes: LndScope[]; history: LndParameterHistory[] }

export type LndValueRequest = { value: string; effectiveFromDate: string | null; decisionReference: string | null; reason: string }

const API = '/api/lnd/parameters'
const value = (id: number, action: string) => `${API}/values/${id}/${action}`

export const lndParametersApi = {
  get: () => http.get<LndParametersPage>(API),
  request: (code: string, scope: string, body: LndValueRequest) =>
    http.post<LndParametersPage>(`${API}/${encodeURIComponent(code)}/${encodeURIComponent(scope)}/request`, body),
  approve: (id: number, notes: string | null) => http.post<LndParametersPage>(value(id, 'approve'), { notes }),
  reject: (id: number, notes: string) => http.post<LndParametersPage>(value(id, 'reject'), { notes }),
  withdraw: (id: number) => http.post<LndParametersPage>(value(id, 'withdraw'), { notes: null })
}
