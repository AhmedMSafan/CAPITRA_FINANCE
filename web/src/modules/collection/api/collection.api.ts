// Collection and Repayments (api/collection), from APEX App 136: repayment transactions (pages 5, 6, 7) and cheques collection (page 712).
import { http } from '@/shared/api/http'

export type Option = { value: string; label: string }

/** Page 5 row: FNV_REPAYMENT_TRANSACTIONS with the client's name */
export type RepaymentRow = {
  repaymentTxnId: number
  repaymentNumber: string
  clientId: number
  clientName: string | null
  repaymentMethod: string
  transactionDate: string
  valueDate: string
  currencyCode: string | null
  receiptAmount: number
  allocatedAmount: number
  statusCode: string
}

export type RepaymentSource = {
  repaymentSourceId: number
  sourceType: string
  chequeId: number | null
  sourceAmount: number
  allocatedAmount: number
  statusCode: string
}

export type RepaymentAllocation = {
  allocationId: number
  repaymentSourceId: number
  componentCode: string
  disbursementId: number
  allocationAmount: number
}

/** Page 6 form */
export type RepaymentForm = {
  repaymentTxnId: number
  repaymentNumber: string
  clientId: number
  clientName: string | null
  repaymentMethod: string
  transactionDate: string
  valueDate: string
  receiptAmount: number
  statusCode: string
  bankStatementReference: string | null
  externalReference: string | null
  reversalReason: string | null
  notes: string | null
  sources: RepaymentSource[]
  allocations: RepaymentAllocation[]
}

export type RepaymentUpdate = Pick<RepaymentForm, 'transactionDate' | 'valueDate' | 'notes' | 'bankStatementReference' | 'externalReference' | 'reversalReason'>

/** Page 7 New Bank Transfer */
export type BankTransfer = {
  clientId: number | null
  receiptAmount: number | null
  transactionDate: string | null
  valueDate: string | null
  notes: string | null
  bankStatementReference: string | null
  externalReference: string | null
}

export type RepaymentSaved = { repaymentTxnId: number; repaymentNumber: string; message: string }

/** Page 712 grid row (FNV_DUE_CHEQUES_V) */
export type DueCheque = {
  chequeId: number
  chequeRef: string
  chequeNo: string
  dueDate: string
  issuerBankName: string
  drawerNameEn: string
  customerId: string | null
  clientName: string
  bankNameEn: string
  chequeAmount: number
  currencyCode: string
  statusCode: string
  rejectionReasonCode: string | null
}

export type ChequeResult = {
  chequeId: number
  customerId: string
  chequeAmount: number
  newStatusCode: string
  statusDate: string | null
  bankStatementReference: string | null
  reasonCode: string | null
  notes: string | null
}

const base = '/api/collection'

export const collectionApi = {
  repayments: () => http.get<RepaymentRow[]>(`${base}/repayments`),
  repayment: (id: number) => http.get<RepaymentForm>(`${base}/repayments/${id}`),
  updateRepayment: (id: number, dto: RepaymentUpdate) => http.put<RepaymentForm>(`${base}/repayments/${id}`, dto),
  customers: () => http.get<Option[]>(`${base}/customers`),
  saveBankTransfer: (dto: BankTransfer) => http.post<RepaymentSaved>(`${base}/bank-transfers`, dto),
  depositAccounts: () => http.get<Option[]>(`${base}/deposit-accounts`),
  dueCheques: (filter: { depositBankAccountId: string; dueFrom: string; dueTo: string }) => {
    const q = new URLSearchParams()
    if (filter.depositBankAccountId) q.set('depositBankAccountId', filter.depositBankAccountId)
    if (filter.dueFrom) q.set('dueFrom', filter.dueFrom)
    if (filter.dueTo) q.set('dueTo', filter.dueTo)
    return http.get<DueCheque[]>(`${base}/due-cheques?${q}`)
  },
  applyResults: (rows: ChequeResult[]) => http.post<{ message: string }>(`${base}/due-cheques/results`, rows)
}
