// Customer Disbursement Request (api/crm/disbursement-requests), from APEX App 150722 pages 750 and 751.
import { http } from '@/shared/api/http'
import type { CrmOption } from './crm.api'

/** Page 750 row: FCT_DISBURSEMENTS with the client's name; workflowStarted is IS_ACTIVE */
export type DisbursementRequestRow = {
  disbursementId: number
  disbursementNumber: string
  disbursementDate: string
  clientName: string | null
  grossAmount: number
  netAmount: number | null
  repaymentStartDate: string | null
  tenorMonth: number | null
  disbursementRef: string | null
  disbursementType: string | null
  workflowStarted: boolean
  limitId: number | null
  limitNumber: string | null
  currencyCode: string | null
  /** the open step of the started workflow; null before it starts and after it ends */
  workflowStepName: string | null
}

/** Page 751 form */
export type DisbursementRequest = {
  disbursementId: number
  disbursementNumber: string
  disbursementDate: string
  clientId: number
  clientName: string | null
  limitId: number | null
  currencyId: number
  grossAmount: number
  remarks: string | null
  agreementId: number | null
  workflowStarted: boolean
}

/** The page 751 items the user fills */
export type DisbursementRequestSave = {
  disbursementDate: string | null
  clientId: number | null
  limitId: number | null
  currencyId: number | null
  grossAmount: number | null
  remarks: string | null
}

/** Page 751 "Application Summary" */
export type AgreementSummary = {
  agreementId: number
  agreementNumber: string
  clientName: string
  currencyCode: string
  facilityLimit: number
  applicationAt: string | null
  relationshipManager: string | null
  statusCode: string | null
}

export type DisbursementRequestSaved = { disbursementId: number; disbursementNumber: string; message: string }

const API = '/api/crm/disbursement-requests'

export const disbursementApi = {
  list: (clientId?: number) => http.get<DisbursementRequestRow[]>(clientId == null ? API : `${API}?clientId=${clientId}`),
  get: (id: number) => http.get<DisbursementRequest>(`${API}/${id}`),
  clients: () => http.get<CrmOption[]>(`${API}/clients`),
  limits: (clientId: number) => http.get<CrmOption[]>(`${API}/clients/${clientId}/limits`),
  currencies: () => http.get<CrmOption[]>(`${API}/currencies`),
  limitSummary: (limitId: number) => http.get<AgreementSummary | null>(`${API}/limits/${limitId}/agreement-summary`),
  agreementSummary: (agreementId: number) => http.get<AgreementSummary | null>(`${API}/agreements/${agreementId}/summary`),
  create: (form: DisbursementRequestSave) => http.post<DisbursementRequestSaved>(API, form),
  update: (id: number, form: DisbursementRequestSave) => http.put<DisbursementRequest>(`${API}/${id}`, form),
  remove: (id: number) => http.delete<void>(`${API}/${id}`),
  startWorkflow: (id: number, form: DisbursementRequestSave) => http.post<DisbursementRequestSaved>(`${API}/${id}/start-workflow`, form)
}
