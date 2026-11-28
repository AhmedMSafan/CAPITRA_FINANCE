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
  // the repayment terms (Ahmed, 2 Nov 2026): every N months from the start dates (the principal's is repaymentStartDate)
  principalFrequencyMonths: number | null
  interestFrequencyMonths: number | null
  interestStartDate: string | null
  principalFirstDueCode: string | null
  interestFirstDueCode: string | null
}

/** A principal payment of a custom (irregular) schedule */
export type CustomScheduleRow = { dueDate: string | null; principalAmount: number | null }

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
  options: { clients: CrmOption[]; limits: CrmOption[]; currencies: CrmOption[]; products: CrmOption[]; reverseProductIds: number[] }
  customSchedule: CustomScheduleRow[]
}

export type InvoiceEntrySave = {
  terms: DisbursementTerms
  batch: InvoiceBatch
  invoices?: InvoiceRow[]
  deletedInvoiceIds?: number[]
  cheques?: ChequeRow[]
  deletedChequeIds?: number[]
  customSchedule?: CustomScheduleRow[]
  // the rows shown are a file's preview: Save Draft gives their edits to the rows it loads from the same file
  invoicesPreviewed?: boolean
  chequesPreviewed?: boolean
}

const filesForm = (body: InvoiceEntrySave, invoiceFile: File | null, chequeFile: File | null) => {
  const form = new FormData()
  form.append('payload', JSON.stringify(body))
  if (invoiceFile) form.append('invoiceFile', invoiceFile)
  if (chequeFile) form.append('chequeFile', chequeFile)
  return form
}

const API = '/api/crm/disbursement-requests'

export const invoiceEntryApi = {
  get: (id: number) => http.get<InvoiceEntry>(`${API}/${id}/invoice-entry`),
  save: (id: number, body: InvoiceEntrySave) => http.put<InvoiceEntry>(`${API}/${id}/invoice-entry`, body),
  // preview: the files' rows are shown and nothing is saved; otherwise Save Draft loads them
  upload: (id: number, body: InvoiceEntrySave, invoiceFile: File | null, chequeFile: File | null, preview = false) =>
    http.upload<{ page: InvoiceEntry; message: string }>(`${API}/${id}/invoice-entry/upload${preview ? '?preview=true' : ''}`,
      filesForm(body, invoiceFile, chequeFile)),
  summary: (id: number, clientId: number | null, limitId: number | null) => {
    const q = new URLSearchParams()
    if (clientId != null) q.set('clientId', String(clientId))
    if (limitId != null) q.set('limitId', String(limitId))
    return http.get<InvoiceSummary>(`${API}/${id}/invoice-entry/summary?${q}`)
  },
  limits: (clientId: number) => http.get<CrmOption[]>(`${API}/clients/${clientId}/invoice-entry-limits`),
  advanceRate: (limitId: number) => http.get<number | null>(`${API}/limits/${limitId}/advance-rate`)
}

/** A calculated charge (FCT_DISBURSEMENT_CHARGES) */
export type DisbursementCharge = {
  disbursementChargeId: number
  ruleCode: string
  ruleName: string
  calculationMethod: string
  baseCode: string | null
  baseAmount: number | null
  ratePercent: number | null
  chargeAmount: number
  taxAmount: number
  totalAmount: number
  chargeTimingCode: string
  dueDate: string | null
  statusCode: string
}

/** A row of the current repayment schedule (FCT_REPAYMENTS) */
export type RepaymentRow = {
  installmentNo: number
  dueDate: string
  openingPrincipal: number | null
  principalDue: number | null
  interestDue: number | null
  totalInstallment: number | null
  closingPrincipal: number | null
  interestRate: number | null
  dayCount: number | null
  statusCode: string
}

export type DisbursementResult = {
  statusCode: string | null
  approvedInvoiceAmount: number
  approvedAdvanceAmount: number
  feeDeducted: number | null
  taxDeducted: number | null
  netAmount: number | null
  charges: DisbursementCharge[]
  schedule: RepaymentRow[]
  glEntries: GlEntry[]
  isPreview: boolean
  // why the preview has no repayment schedule
  scheduleNote?: string | null
}

/** A GL line the charges post: journal 1 the charge, journal 2 its payment */
export type GlEntry = {
  journalNo: number
  lineNo: number
  chargeCode: string
  debitCreditCode: string
  accountCode: string | null
  accountName: string | null
  amount: number
  description: string | null
}

/** A line of a customer claim: one charge */
export type ClaimLine = {
  claimLineId: number
  lineNo: number
  chargeCode: string
  chargeName: string | null
  chargeAmount: number
  taxAmount: number
  totalAmount: number
  paidAmount: number
  outstandingAmount: number
}

/** One payment of a claim, under its own GL tran set */
export type ClaimPayment = {
  claimPaymentId: number
  repaymentNumber: string
  sourceType: string
  reference: string | null
  paymentDate: string
  paymentAmount: number
  glTranSetId: string | null
}

export type ClaimStatus = 'OPEN' | 'PARTIALLY_PAID' | 'PAID' | 'CANCELLED'

/** A customer claim (FCT_CUSTOMER_CLAIMS) as the lists show it */
export type ClaimRow = {
  claimId: number
  claimNumber: string
  clientId: number
  clientName: string | null
  disbursementId: number
  disbursementNumber: string
  claimDate: string
  currencyCode: string | null
  totalAmount: number
  paidAmount: number
  outstandingAmount: number
  statusCode: ClaimStatus
}

export type Claim = { claim: ClaimRow; lines: ClaimLine[]; payments: ClaimPayment[] }

/** FCT_DISBURSEMENTS.CLAIM_STATUS_CODE */
export type DisbursementClaimStatus = 'NOT_CREATED' | 'CREATED_UNPAID' | 'PARTIALLY_PAID' | 'PAID'

/** The Claim section of the Invoice Upload page */
export type DisbursementClaim = { claimStatusCode: DisbursementClaimStatus; chargesTotal: number; isCompleted: boolean; claim: Claim | null }

const claimUrl = (id: number) => `/api/crm/disbursement-requests/${id}/invoice-entry/claim`

export const disbursementClaimApi = {
  get: (id: number) => http.get<DisbursementClaim>(claimUrl(id)),
  // Create Claim: the page is saved, its charges calculated and claimed from the customer
  create: (id: number, body: InvoiceEntrySave) => http.post<DisbursementClaim>(claimUrl(id), body),
  cancel: (id: number, claimId: number) => http.post<DisbursementClaim>(`${claimUrl(id)}/${claimId}/cancel`, {})
}

export const processApi = {
  process: (id: number, body: InvoiceEntrySave) =>
    http.post<{ page: InvoiceEntry; result: DisbursementResult; message: string }>(`/api/crm/disbursement-requests/${id}/invoice-entry/process`, body),
  result: (id: number) => http.get<DisbursementResult>(`/api/crm/disbursement-requests/${id}/invoice-entry/result`),
  // the charges of the page values as they stand, calculated and not kept
  preview: (id: number, body: InvoiceEntrySave) =>
    http.post<DisbursementResult>(`/api/crm/disbursement-requests/${id}/invoice-entry/charges-preview`, body),
  // the same while files are chosen and not saved yet
  previewFiles: (id: number, body: InvoiceEntrySave, invoiceFile: File | null, chequeFile: File | null) =>
    http.upload<DisbursementResult>(`/api/crm/disbursement-requests/${id}/invoice-entry/charges-preview-files`, filesForm(body, invoiceFile, chequeFile))
}
