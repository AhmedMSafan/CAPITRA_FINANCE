// Factoring customers and the Factoring Customer Workspace (api/fct/customers), from FINOVA_FINANCEING.
import { http } from '@/shared/api/http'

/** CRM_CLIENTS, as the Factoring home lists them */
export type FctCustomer = {
  clientId: number
  clientNumber: string
  clientName: string
  clientType: string
  phoneNumber: string | null
  email: string | null
  isActive: boolean
}

/** CRM_CLIENTS, as the workspace header shows it */
export type FctCustomerDetails = {
  clientId: number
  clientNumber: string
  clientName: string
  clientNameAr: string
  clientType: string
  countryName: string | null
  currencyCode: string | null
  relationshipManager: string | null
  phoneNumber: string | null
  email: string | null
}

/** FCT_OFFERS: a factoring application */
export type FctApplication = {
  offerId: number
  offerRef: string
  offerAt: string | null
  requestedLimit: number
  currencyCode: string | null
  factoringType: string
  statusCode: string | null
  agreementNo: string | null
  workflowStepName: string | null
}

/** FCT_AGREEMENTS */
export type FctAgreement = {
  agreementId: number
  agreementNumber: string
  applicationAt: string | null
  facilityLimit: number
  currencyCode: string | null
  factoringType: string
  statusCode: string | null
  workflowStepName: string | null
}

const API = '/api/fct/customers'

export const fctApi = {
  customers: () => http.get<FctCustomer[]>(API),
  customer: (id: number) => http.get<FctCustomerDetails>(`${API}/${id}`),
  applications: (id: number) => http.get<FctApplication[]>(`${API}/${id}/applications`),
  agreements: (id: number) => http.get<FctAgreement[]>(`${API}/${id}/agreements`)
}
