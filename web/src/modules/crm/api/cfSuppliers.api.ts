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
  /** From the customer (Customer 360 > Compliance): its compliance answer and risk grade. */
  compliance: { code: string; label: string; reasons: string[] }; customerRiskGrade: string | null
}

export type CfOptions = { goodsCategories: CfOption[]; collectors: CfOption[]; partyTypes: CfOption[]; riskRatings: CfOption[]; documentTypes: CfOption[] }

export type CfSupplierRequest = {
  clientId?: number | null; goodsCategoryCodes: string[]; minDownPaymentPct: number | null; downPaymentCollectorCode: string
  commissionPct: number | null; paymentDays: number | null; quotationValidityDays: number | null; deliveryDays: number | null
  validFromDate: string | null; validToDate: string | null; clientBankAccountId: number | null; notes: string | null
}

export type CfDocument = {
  documentId: number; documentTypeCode: string | null; documentTitle: string; fileName: string; mimeType: string; fileSizeBytes: number
  /** Where it was added: Data entry, a workflow step, or After activation. */
  stepName: string; createdBy: string; createdAt: string; canRemove: boolean
}

export type CfStepView = {
  key: string; round: number; stepCode: string; stepName: string; status: 'DONE' | 'OPEN'; action: string | null; doneBy: string | null
  doneAt: string | null; comment: string | null
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
  submit: (id: number) => http.post<CfSupplier>(`${API}/${id}/submit`),
  steps: (id: number) => http.get<CfStepView[]>(`${API}/${id}/steps`),
  saveSiteVisit: (id: number, inboxId: number, siteVisitDate: string | null) =>
    http.post<CfSupplier>(`${API}/${id}/steps/${inboxId}/site-visit`, { siteVisitDate }),
  saveBankAccount: (id: number, inboxId: number, bankAccountId: number | null) =>
    http.post<CfSupplier>(`${API}/${id}/steps/${inboxId}/bank-account${bankAccountId ? `?bankAccountId=${bankAccountId}` : ''}`),
  completeStep: (id: number, inboxId: number, action: 'ACCEPT' | 'REJECT', comment: string | null) =>
    http.post<CfSupplier>(`${API}/${id}/steps/${inboxId}/complete`, { action, comment }),
  documents: (id: number) => http.get<CfDocument[]>(`${API}/${id}/documents`),
  /** Adds a document to the application, at any time (before submit, at any step, or after activation). */
  addDocument: (id: number, file: File, title: string, documentTypeCode: string | null) => {
    const form = new FormData()
    form.append('file', file)
    form.append('title', title)
    if (documentTypeCode) form.append('documentTypeCode', documentTypeCode)
    return http.upload<CfDocument>(`${API}/${id}/documents`, form)
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
