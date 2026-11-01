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
  options: { clients: CrmOption[]; limits: CrmOption[]; currencies: CrmOption[]; products: CrmOption[]; reverseProductIds: number[] }
}

export type InvoiceEntrySave = {
  terms: DisbursementTerms
  batch: InvoiceBatch
  invoices?: InvoiceRow[]
  deletedInvoiceIds?: number[]
  cheques?: ChequeRow[]
  deletedChequeIds?: number[]
  chargeSourceIds?: number[]
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

/** A payment source of the customer with balance left */
export type ChargeSource = {
  repaymentSourceId: number
  repaymentNumber: string
  sourceType: string
  reference: string | null
  valueDate: string
  sourceAmount: number
  usedAmount: number
  availableAmount: number
}

/** One payment of a charge from a source */
export type ChargePaymentRow = {
  allocationId: number
  ruleCode: string
  repaymentNumber: string
  sourceType: string
  reference: string | null
  allocationDate: string
  amount: number
}

/** The Charges Payment section */
export type ChargePayment = {
  totalCharges: number
  paidAmount: number
  outstandingAmount: number
  statusCode: 'NO_CHARGES' | 'NOT_PAID' | 'PARTIALLY_PAID' | 'PAID'
  glTranSetId: string | null
  glPostingDate: string | null
  sources: ChargeSource[]
  payments: ChargePaymentRow[]
  chosenSourceIds: number[]
}

const paymentUrl = (id: number) => `/api/crm/disbursement-requests/${id}/invoice-entry/charge-payment`

export const chargePaymentApi = {
  get: (id: number) => http.get<ChargePayment>(paymentUrl(id)),
  pay: (id: number, sourceId: number) => http.post<ChargePayment>(`${paymentUrl(id)}/sources/${sourceId}`, {}),
  reverse: (id: number, allocationId: number) => http.post<ChargePayment>(`${paymentUrl(id)}/payments/${allocationId}/reverse`, {})
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
