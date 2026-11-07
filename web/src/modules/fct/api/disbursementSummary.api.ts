// The invoice summary of a disbursement (Ahmed, 7 Nov 2026): from the disbursement request (CRM.VIEW) or Treasury Disburse (TRS.VIEW).
import { http } from '@/shared/api/http'

export type SummaryInvoice = {
  invoiceId: number
  invoiceNumber: string
  invoiceAt: string | null
  dueAt: string | null
  grossAmount: number
  /** what we fund: the advance amount, else the invoice amount at the advance rate */
  financed: number
  eligible: boolean
  ineligibleReason: string | null
}

export type SummaryParty = {
  clientId: number | null
  name: string
  taxNumber: string | null
  count: number
  grossAmount: number
  financed: number
  invoices: SummaryInvoice[]
}

export type DisbursementSummary = {
  disbursementId: number
  disbursementNumber: string
  clientName: string
  businessLine: string
  isReverse: boolean
  currencyCode: string
  advanceRatePct: number | null
  netAmount: number | null
  receivedCount: number
  receivedGross: number
  notEligibleCount: number
  notEligibleGross: number
  eligibleCount: number
  eligibleGross: number
  eligibleFinanced: number
  parties: SummaryParty[]
  notEligible: SummaryInvoice[]
}

export const disbursementSummaryApi = {
  get: (disbursementId: number, from: 'crm' | 'trs') =>
    http.get<DisbursementSummary>(from === 'trs'
      ? `/api/trs/disbursements/${disbursementId}/funding/summary`
      : `/api/crm/disbursement-requests/${disbursementId}/summary`)
}
