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
  // the collection's GL transaction set (CAP-COL-...), none before posting
  glTranSetId?: string | null
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
  glTranSetId?: string | null
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

/** The result of Apply Bank Results: the collection batch it made and its GL transaction set */
export type CollectionApplied = { message: string; collectionBatchId: number; batchNo: string; glTranSetId: string | null }

/** A collection batch: one run of Apply Bank Results (FCT_COLLECTION_BATCHES) */
export type CollectionBatch = {
  collectionBatchId: number
  batchNo: string
  batchDate: string
  collectedCount: number
  collectedAmount: number
  bouncedCount: number
  bouncedAmount: number
  rejectedCount: number
  rejectedAmount: number
  repaymentsCount: number
  glTranSetId: string | null
  createdBy: string | null
  createdAt: string | null
}

/** A cheque of a collection batch (FCT_COLLECTION_BATCH_LINES) */
export type CollectionBatchLine = {
  lineNo: number
  chequeId: number
  chequeRef: string | null
  chequeNo: string
  dueDate: string
  customerId: string | null
  clientName: string | null
  bankName: string | null
  accountNo: string | null
  glAccount: string | null
  chequeAmount: number
  currencyCode: string | null
  resultCode: string
  statusDate: string
  bankStatementReference: string | null
  reasonCode: string | null
  repaymentTxnId: number | null
  repaymentNumber: string | null
}

export type CollectionBatchView = { batch: CollectionBatch; lines: CollectionBatchLine[] }

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
  applyResults: (rows: ChequeResult[]) => http.post<CollectionApplied>(`${base}/due-cheques/results`, rows),
  batches: () => http.get<CollectionBatch[]>(`${base}/batches`),
  batch: (id: number) => http.get<CollectionBatchView>(`${base}/batches/${id}`)
}
