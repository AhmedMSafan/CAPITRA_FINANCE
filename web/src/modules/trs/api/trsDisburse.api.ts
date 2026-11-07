// Treasury Disburse (api/trs/disbursements/{id}/funding): choose, reserve and execute the source of funds of a disbursement.
import { http } from '@/shared/api/http'

export type TrsDisburseHeader = {
  disbursementId: number
  disbursementNumber: string
  clientId: number
  clientName: string
  agreementNumber: string | null
  businessLine: string
  isReverse: boolean
  currencyCode: string
  netAmount: number
  invoiceCount: number
  maxTenorDays: number | null
  fundingStatus: 'NONE' | 'RESERVED' | 'EXECUTED'
  executedBy: string | null
  executedAt: string | null
}

export type TrsDisburseParty = {
  clientId: number | null
  name: string
  role: 'DEBTOR' | 'SELLER'
  weight: number
  groupName: string | null
  taxNumber: string | null
}

/** One party's popup: its invoices, its bank accounts and, for the seller (the supplier in reverse factoring), the amount it receives. */
export type TrsDisbursePartyDetail = {
  clientId: number | null
  name: string
  role: 'DEBTOR' | 'SELLER'
  taxNumber: string | null
  clientCode: string | null
  currencyCode: string
  weight: number
  invoices: {
    invoiceId: number
    invoiceNumber: string
    clientInvoiceNumber: string | null
    invoiceAt: string | null
    dueAt: string | null
    grossAmount: number
    amount: number
    statusCode: string | null
    ineligibleReason: string | null
  }[]
  invoiceTotal: number
  eligibleTotal: number
  amountToReceive: number | null
  bankAccounts: {
    bankAccountId: number
    bankName: string
    branchName: string | null
    accountNumber: string | null
    iban: string | null
    accountHolderName: string
    currencyCode: string
    isPrimary: boolean
    statusCode: string
    isPayable: boolean
    payableFromAt: string
  }[]
  /** The account the money goes to: verified, waiting period passed, in the disbursement currency. */
  payTo: string | null
}

export type TrsDisburseSource = {
  sourceKey: string
  sourceTypeCode: 'OWN_FUNDS' | 'BANK'
  bankId: number | null
  bankName: string | null
  facilityId: number | null
  facilityRef: string | null
  trancheName: string | null
  sublimitId: number | null
  sublimitName: string | null
  available: number | null
  amount: number
  isOverrideRequired: boolean
  ruleIds: number[]
  /** Why this sublimit may fund the disbursement. */
  fit: string | null
  /** Why it cannot (status, business line, nothing left, or a bank rule that blocks it); null when it can. */
  unavailable: string | null
}

export type TrsDisburseRule = {
  ruleId: number
  bankId: number
  bankName: string
  facilityId: number | null
  ruleTypeCode: string
  label: string
  breachActionCode: 'BLOCK' | 'OVERRIDE' | 'WARN'
  limitAbove: number | null
  warnAbove: number | null
  detail: string
}

/** How a saved line is paid: from our account at that bank, or the main operating account for own funds. */
export type TrsDisbursePayment = {
  sourceKey: string
  sourceTypeCode: 'OWN_FUNDS' | 'BANK'
  bankName: string | null
  amount: number
  accountId: number | null
  accountRef: string | null
  accountNo: string | null
  accountBankName: string | null
  glAccountCode: string | null
  glTranSetId: string | null
  problem: string | null
}

/** Reverse factoring: one transfer to a supplier. PLANNED before Execute, SENT by Execute, CONFIRMED when its box is ticked. */
export type TrsSupplierTransfer = {
  transferId: number
  supplierClientId: number | null
  supplierName: string
  taxNumber: string | null
  supplierBankName: string | null
  supplierIban: string | null
  supplierAccountNo: string | null
  amount: number
  currencyCode: string
  paidFromAccountRef: string | null
  paidFromBankName: string | null
  statusCode: 'PLANNED' | 'SENT' | 'CONFIRMED'
  sentAt: string | null
  sentBy: string | null
  bankReference: string | null
  confirmedAt: string | null
  confirmedBy: string | null
  glTranSetId: string | null
  problem: string | null
}

export type TrsDisburse = {
  header: TrsDisburseHeader
  parties: TrsDisburseParty[]
  sources: TrsDisburseSource[]
  rules: TrsDisburseRule[]
  canEdit: boolean
  overrideReason: string | null
  overrideRequestedBy: string | null
  overrideApprovedBy: string | null
  canApproveOverride: boolean
  approveBlockedReason: string | null
  payments: TrsDisbursePayment[]
  paymentStatus: string | null
  executionDate: string | null
  /** Module setting TRS.SUGGEST_ORDER: what Suggest fills first. */
  suggestOrder: 'BANK_FIRST' | 'OWN_FIRST'
  /** Reverse factoring only: the transfers to the suppliers. */
  transfers: TrsSupplierTransfer[] | null
}

export type TrsDisburseForm = { lines: { sublimitId: number | null; amount: number }[]; overrideReason: string | null }

const base = (id: number) => `/api/trs/disbursements/${id}/funding`

export const trsDisburseApi = {
  get: (id: number) => http.get<TrsDisburse>(base(id)),
  save: (id: number, form: TrsDisburseForm) => http.put<TrsDisburse>(base(id), form),
  approveOverride: (id: number) => http.post<TrsDisburse>(`${base(id)}/approve-override`, {}),
  execute: (id: number) => http.post<TrsDisburse>(`${base(id)}/execute`, {}),
  confirmTransfers: (id: number, transferIds: number[], bankReference: string | null) =>
    http.post<TrsDisburse>(`${base(id)}/transfers/confirm`, { transferIds, bankReference }),
  party: (id: number, p: { clientId: number | null; taxNumber: string | null }) =>
    http.get<TrsDisbursePartyDetail>(
      `${base(id)}/party?${p.clientId !== null ? `clientId=${p.clientId}` : `taxNumber=${encodeURIComponent(p.taxNumber ?? '')}`}`)
}
