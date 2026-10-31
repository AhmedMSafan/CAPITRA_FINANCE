// CRM clients (api/crm/clients), from FINOVA_FINANCEING api/clients and its LOVs.
import { http } from '@/shared/api/http'

/** CRM_CLIENTS, as the CRM home lists them */
export type CrmClientRow = {
  clientId: number
  clientNumber: string
  clientName: string
  clientType: string
  phoneNumber: string | null
  email: string | null
  isActive: boolean
  /** the newest of the client's applications, agreements and disbursement requests; null when it has none */
  lastActivityAt: string | null
  lastActivityType: 'Application' | 'Agreement' | 'Disbursement' | null
  lastActivityRef: string | null
}

/** The fields of the client dialog */
export type CrmClientForm = {
  clientId: number | null
  clientNumber: string
  clientName: string
  clientNameAr: string
  shortName: string | null
  clientType: string
  clientRoleCode: string | null
  legalStructure: string
  registrationNumber: string | null
  taxNumber: string | null
  email: string | null
  phoneNumber: string | null
  mobileNumber: string | null
  website: string | null
  countryId: number
  baseCurrencyId: number
  statusId: number
  isActive: boolean
  isSupplier: boolean
  isRelatedParty: boolean
}

export type CrmOption = { value: string; label: string }
export type CrmClientOptions = { countries: CrmOption[]; currencies: CrmOption[]; clientRoles: CrmOption[] }

const API = '/api/crm/clients'

export const crmApi = {
  clients: () => http.get<CrmClientRow[]>(API),
  options: () => http.get<CrmClientOptions>(`${API}/options`),
  client: (clientId: number) => http.get<CrmClientForm>(`${API}/${clientId}`),
  create: (form: CrmClientForm) => http.post<CrmClientForm>(API, form),
  update: (clientId: number, form: CrmClientForm) => http.put<CrmClientForm>(`${API}/${clientId}`, form)
}
