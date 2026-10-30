// The Invoice Upload step of DISBURSEMENT_REQUEST_WF (api/crm/disbursement-requests/{id}/invoice-entry), from APEX App 148 page 705.
import { http } from '@/shared/api/http'
import type { CrmOption } from './crm.api'

/** "Disbursement Request Detail" region */
export type DisbursementTerms = {
  disbursementNumber: string
  disbursementDate: string | null
  clientId: number | null
  limitId: number | null
  currencyId: number | null
  productId: number | null
  grossAmount: number | null
  advanceRatePct: number | null
  tenorMonth: number | null
  indexRate: number | null
  spreadRate: number | null
  baseDayCount: string | null
  repaymentStartDate: string | null
  principalRepaymentScheduleType: string | null
  principalRepaymentMethod: string | null
  principalDistributionMethod: string | null
  interestRepaymentMethod: string | null
  paymentMethod: string | null
  remarks: string | null
}

/** "Invoice Batch Details" region */
export type InvoiceBatch = {
  batchId: number | null
  batchNumber: string | null
  batchName: string | null
  clientId: number | null
  batchDate: string | null
  batchType: string | null
  currencyId: number | null
  totalNetAmount: number | null
  remarks: string | null
}

/** "Invoice Entry" grid row */
export type InvoiceRow = {
  invoiceId: number
  invoiceNumber: string
  clientInvoiceDate: string | null
  dueDate: string | null
  grossAmount: number
  advanceRatePct: number | null
  advanceAmount: number | null
  statusCode: string | null
  ineligibleReason: string | null
}

/** "Cheques Entry" grid row */
export type ChequeRow = {
  chequeId: number
  chequeNo: string
  chequeDate: string
  dueDate: string
  issuerBankName: string
  drawerNameEn: string
  depositBankAccountId: number | null
  chequeAmount: number
  currencyCode: string
  sourceModule: string | null
  receivedDate: string
  depositDate: string | null
  statusCode: string
}

/** "Summary" region */
export type InvoiceSummary = {
  limitNumber: string
  clientName: string
  approvedLimit: number
  availableAmount: number
  currencyCode: string
  totalInvoices: number
  approvedInvoiced: number
  advanceRate: number
  utilizationAmount: number
  utilizationPercent: number
}

export type InvoiceEntry = {
  disbursementId: number
  terms: DisbursementTerms
  batch: InvoiceBatch
  summary: InvoiceSummary
  invoices: InvoiceRow[]
  cheques: ChequeRow[]
  options: { clients: CrmOption[]; limits: CrmOption[]; currencies: CrmOption[]; products: CrmOption[] }
}

export type InvoiceEntrySave = {
  terms: DisbursementTerms
  batch: InvoiceBatch
  invoices?: InvoiceRow[]
  deletedInvoiceIds?: number[]
  cheques?: ChequeRow[]
  deletedChequeIds?: number[]
}

const API = '/api/crm/disbursement-requests'

export const invoiceEntryApi = {
  get: (id: number) => http.get<InvoiceEntry>(`${API}/${id}/invoice-entry`),
  save: (id: number, body: InvoiceEntrySave) => http.put<InvoiceEntry>(`${API}/${id}/invoice-entry`, body),
  upload: (id: number, body: InvoiceEntrySave, invoiceFile: File | null, chequeFile: File | null) => {
    const form = new FormData()
    form.append('payload', JSON.stringify(body))
    if (invoiceFile) form.append('invoiceFile', invoiceFile)
    if (chequeFile) form.append('chequeFile', chequeFile)
    return http.upload<{ page: InvoiceEntry; message: string }>(`${API}/${id}/invoice-entry/upload`, form)
  },
  summary: (id: number, clientId: number | null, limitId: number | null) => {
    const q = new URLSearchParams()
    if (clientId != null) q.set('clientId', String(clientId))
    if (limitId != null) q.set('limitId', String(limitId))
    return http.get<InvoiceSummary>(`${API}/${id}/invoice-entry/summary?${q}`)
  },
  limits: (clientId: number) => http.get<CrmOption[]>(`${API}/clients/${clientId}/invoice-entry-limits`),
  advanceRate: (limitId: number) => http.get<number | null>(`${API}/limits/${limitId}/advance-rate`)
}
