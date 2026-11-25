// CRM > Consumer Finance suppliers (Ahmed, 25 Nov 2026): a supplier (merchant) application, its agreement terms, owners and
// signatories, documents and the steps of its CF_SUPPLIER_APPROVAL workflow.
import { http } from '@/shared/api/http'

export type CfOption = { code: string; name: string }
export type CfBankAccount = { bankAccountId: number; bankName: string; accountNumber: string | null; iban: string | null; statusCode: string }
export type CfClient = { clientId: number; clientNumber: string; clientName: string }

export type CfParty = {
  partyId: number; partyTypeCode: string; fullName: string; nationalIdNo: string | null; nationality: string | null; ownershipPct: number | null
  screeningResultCode: string | null; screenedBy: string | null; screenedAt: string | null
}

export type CfSupplierListItem = {
  applicationId: number; applicationRef: string; clientId: number; clientNumber: string; clientName: string; statusCode: string
  riskRatingCode: string | null; openStepCode: string | null; openStepName: string | null; enteredBy: string; createdAt: string; activatedAt: string | null
}

export type CfSupplier = {
  applicationId: number; applicationRef: string; clientId: number; clientNumber: string; clientName: string; registrationNumber: string | null
  taxNumber: string | null; clientRoleId: number; roleStatusCode: string; statusCode: string; workflowInstanceId: number | null; enteredBy: string
  returnReason: string | null; goodsCategoryCodes: string[]; minDownPaymentPct: number; downPaymentCollectorCode: string; commissionPct: number
  paymentDays: number; quotationValidityDays: number; deliveryDays: number; validFromDate: string; validToDate: string | null
  clientBankAccountId: number | null; riskRatingCode: string | null; siteVisitDate: string | null; activatedAt: string | null; notes: string | null
  parties: CfParty[]; bankAccounts: CfBankAccount[]; openStepCode: string | null; openStepName: string | null; canEdit: boolean
}

export type CfOptions = { goodsCategories: CfOption[]; collectors: CfOption[]; partyTypes: CfOption[]; riskRatings: CfOption[] }

export type CfSupplierRequest = {
  clientId?: number | null; goodsCategoryCodes: string[]; minDownPaymentPct: number | null; downPaymentCollectorCode: string
  commissionPct: number | null; paymentDays: number | null; quotationValidityDays: number | null; deliveryDays: number | null
  validFromDate: string | null; validToDate: string | null; clientBankAccountId: number | null; notes: string | null
}

export type CfPartyRequest = { partyTypeCode: string; fullName: string; nationalIdNo: string | null; nationality: string | null; ownershipPct: number | null }

export type CfDocument = {
  documentId: number; stepCode: string; stepName: string; documentTitle: string; fileName: string; mimeType: string; fileSizeBytes: number
  createdBy: string; createdAt: string; canRemove: boolean
}

export type CfStepView = {
  key: string; round: number; stepCode: string; stepName: string; status: 'DONE' | 'OPEN'; action: string | null; doneBy: string | null
  doneAt: string | null; comment: string | null; documents: CfDocument[]
}

const API = '/api/crm/cf-suppliers'

export const cfSuppliersApi = {
  list: (status?: string) => http.get<CfSupplierListItem[]>(status ? `${API}?status=${encodeURIComponent(status)}` : API),
  options: () => http.get<CfOptions>(`${API}/options`),
  clients: (search: string) => http.get<CfClient[]>(`${API}/clients?search=${encodeURIComponent(search)}`),
  get: (id: number) => http.get<CfSupplier>(`${API}/${id}`),
  byClient: (clientId: number) => http.get<{ applicationId: number | null }>(`${API}/by-client/${clientId}`),
  create: (r: CfSupplierRequest) => http.post<CfSupplier>(API, r),
  update: (id: number, r: CfSupplierRequest) => http.put<CfSupplier>(`${API}/${id}`, r),
  addParty: (id: number, r: CfPartyRequest) => http.post<CfSupplier>(`${API}/${id}/parties`, r),
  removeParty: (id: number, partyId: number) => http.post<CfSupplier>(`${API}/${id}/parties/${partyId}/remove`),
  submit: (id: number) => http.post<CfSupplier>(`${API}/${id}/submit`),
  withdraw: (id: number) => http.post<CfSupplier>(`${API}/${id}/withdraw`),
  steps: (id: number) => http.get<CfStepView[]>(`${API}/${id}/steps`),
  saveCompliance: (id: number, inboxId: number, riskRatingCode: string | null, screening: { partyId: number; resultCode: string | null }[]) =>
    http.post<CfSupplier>(`${API}/${id}/steps/${inboxId}/compliance`, { riskRatingCode, screening }),
  saveSiteVisit: (id: number, inboxId: number, siteVisitDate: string | null) =>
    http.post<CfSupplier>(`${API}/${id}/steps/${inboxId}/site-visit`, { siteVisitDate }),
  saveBankAccount: (id: number, inboxId: number, bankAccountId: number | null) =>
    http.post<CfSupplier>(`${API}/${id}/steps/${inboxId}/bank-account${bankAccountId ? `?bankAccountId=${bankAccountId}` : ''}`),
  completeStep: (id: number, inboxId: number, action: 'ACCEPT' | 'REJECT', comment: string | null) =>
    http.post<CfSupplier>(`${API}/${id}/steps/${inboxId}/complete`, { action, comment }),
  /** Adds a file at data entry (inboxId null) or at the workflow step of the user's task. */
  addDocument: (id: number, inboxId: number | null, file: File, title: string) => {
    const form = new FormData()
    form.append('file', file)
    form.append('title', title)
    return http.upload<CfDocument>(inboxId ? `${API}/${id}/steps/${inboxId}/documents` : `${API}/${id}/documents`, form)
  },
  removeDocument: (documentId: number) => http.post<void>(`${API}/documents/${documentId}/remove`),
  documentFile: (documentId: number) => http.blob(`${API}/documents/${documentId}`)
}

export const CF_STATUS: Record<string, string> = {
  DRAFT: 'Draft', IN_WORKFLOW: 'In approval', RETURNED: 'Returned', ACTIVE: 'Active', WITHDRAWN: 'Withdrawn'
}
export const CF_CHIP: Record<string, string> = {
  DRAFT: 'fx-chip--plain', IN_WORKFLOW: 'fx-chip--info', RETURNED: 'fx-chip--warn', ACTIVE: 'fx-chip--ok', WITHDRAWN: 'fx-chip--plain'
}
