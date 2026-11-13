import { http } from '@/shared/api/http'

export type LoanOption = { id: number; code: string; name: string }

export type LoanRequest = {
  clientId: number | null
  productId: number | null
  principalAmount: number | null
  termMonths: number | null
  disbursementDate: string | null
  firstDueDate: string | null
  notes: string | null
}

export type ScheduleLine = { installmentNo: number; dueDate: string; openingPrincipal: number; principal: number; interest: number; total: number; closingPrincipal: number }

export type LoanPreview = {
  versionNo: number; repaymentFrequencyCode: string; interestMethodCode: string; annualRatePct: number; installmentCount: number
  installmentAmount: number; totalInterestAmount: number; totalRepayableAmount: number; firstDueDate: string; maturityDate: string; schedule: ScheduleLine[]
}

export type LoanListItem = {
  loanId: number; loanNumber: string; clientId: number; clientName: string; productCode: string; productName: string; licenceCode: string
  statusCode: string; principalAmount: number; termMonths: number; installmentAmount: number; disbursementDate: string; maturityDate: string; createdAt: string
}

export type Installment = {
  installmentNo: number; dueDate: string; openingPrincipal: number; principalDue: number; interestDue: number; chargesDue: number; insuranceDue: number
  lateFeeDue: number; totalDue: number; totalPaid: number; closingPrincipal: number; statusCode: string
}

export type Loan = {
  loanId: number; loanNumber: string; clientId: number; clientName: string; productId: number; productCode: string; productName: string; versionNo: number
  licenceCode: string; statusCode: string; isNonAccrual: boolean; daysLate: number; interestSuspenseAmount: number; lateFeeSuspenseAmount: number
  isWriteOffProposed: boolean; lastEodDate: string | null; currencyCode: string; principalAmount: number; termMonths: number; installmentCount: number
  installmentAmount: number; totalInterestAmount: number; disbursementDate: string; firstDueDate: string; maturityDate: string
  repaymentFrequencyCode: string; interestMethodCode: string; annualRatePct: number; dayCountCode: string; lateFeeGraceDays: number; lateFeeTypeCode: string
  lateFeeRatePct: number | null; lateFeeFlatAmount: number | null; lateFeeBaseCode: string; paymentOrder: string[]; isInsuranceRequired: boolean
  notes: string | null; createdBy: string; createdAt: string; rowVersion: number
  installments: Installment[]
  history: { fromStatusCode: string | null; toStatusCode: string; reason: string | null; by: string; at: string }[]
  events: { eventCode: string; eventDate: string; eventAmount: number; glTranSetId: string | null; notes: string | null; by: string; at: string }[]
}

export type PayingAccount = { accountId: number; bankName: string; accountRef: string; accountNo: string; accountTypeCode: string; glAccountCode: string | null }
export type Charge = { chargeCode: string; chargeName: string; amount: number; incomeRecognitionCode: string }
export type Disbursement = {
  disbursementId: number; statusCode: 'REQUESTED' | 'PAID' | 'REJECTED' | 'WITHDRAWN'; paidFromAccountId: number; paidFromAccount: string | null
  payoutMethodCode: string; payeeName: string; payeeAccount: string | null; principalAmount: number; chargesAmount: number; netPayoutAmount: number
  paidDate: string | null; glTranSetId: string | null; charges: Charge[]; requestedBy: string; requestedAt: string; decidedBy: string | null
  decidedAt: string | null; decisionNotes: string | null; canPay: boolean; payBlockedReason: string | null
}
export type DisbursementPage = {
  loanId: number; loanStatusCode: string; canRequest: boolean; canPay: boolean; accounts: PayingAccount[]
  preview: { ruleCode: string | null; principalAmount: number; charges: Charge[]; chargesAmount: number; netPayoutAmount: number; problem: string | null }
  disbursements: Disbursement[]
}
export type ReceiptSource = { sourceType: 'BANK_DEPOSIT' | 'CHEQUE' | 'CREDIT_BALANCE'; depositAccountId: number | null; chequeId: number | null; amount: number | null; reference: string | null }
export type ReceiptsPage = {
  loanId: number; loanStatusCode: string; canReceive: boolean; today: string; overdueToday: number; creditBalance: number
  depositAccounts: { accountId: number; label: string; hasGlAccount: boolean }[]
  cheques: { chequeId: number; chequeRef: string; chequeNo: string; dueDate: string; chequeAmount: number; depositAccount: string | null }[]
  receipts: { repaymentTxnId: number; repaymentNumber: string; valueDate: string; transactionDate: string; receiptAmount: number; allocatedAmount: number
    statusCode: string; glTranSetId: string | null; sources: string; createdBy: string }[]
  writtenOffAmount: number | null; recoveredAmount: number | null
}
export type ReceiptPreview = { receiptAmount: number; allocatedAmount: number; creditKeptAmount: number; overdueAmount: number; lines: { installmentNo: number; partCode: string; amount: number }[] }
export type PayoutRequest = { paidFromAccountId: number | null; payoutMethodCode: string; payeeName: string; payeeAccount: string | null }

const API = '/api/lnd/loans'

export const loansApi = {
  list: () => http.get<LoanListItem[]>(API),
  get: (id: number) => http.get<Loan>(`${API}/${id}`),
  products: () => http.get<LoanOption[]>(`${API}/products`),
  clients: (q: string) => http.get<LoanOption[]>(`${API}/clients?q=${encodeURIComponent(q)}`),
  preview: (body: LoanRequest) => http.post<LoanPreview>(`${API}/preview`, body),
  create: (body: LoanRequest) => http.post<Loan>(API, body),
  cancel: (id: number, reason: string, rowVersion: number) => http.post<Loan>(`${API}/${id}/cancel`, { reason, rowVersion }),
  payouts: (id: number) => http.get<DisbursementPage>(`${API}/${id}/disbursements`),
  requestPayout: (id: number, body: PayoutRequest) => http.post<DisbursementPage>(`${API}/${id}/disbursements`, body),
  pay: (id: number, d: number, notes: string | null) => http.post<DisbursementPage>(`${API}/${id}/disbursements/${d}/pay`, { notes }),
  rejectPayout: (id: number, d: number, notes: string) => http.post<DisbursementPage>(`${API}/${id}/disbursements/${d}/reject`, { notes }),
  withdrawPayout: (id: number, d: number) => http.post<DisbursementPage>(`${API}/${id}/disbursements/${d}/withdraw`, {}),
  receipts: (id: number) => http.get<ReceiptsPage>(`${API}/${id}/receipts`),
  receiptPreview: (id: number, valueDate: string, amount: number) =>
    http.get<ReceiptPreview>(`${API}/${id}/receipts/preview?valueDate=${encodeURIComponent(valueDate)}&amount=${amount}`),
  receive: (id: number, body: { valueDate: string | null; sources: ReceiptSource[]; notes: string | null }) => http.post<ReceiptsPage>(`${API}/${id}/receipts`, body),
  reversalPreview: (id: number, txnId: number) => http.get<ReversalPreview>(`${API}/${id}/receipts/${txnId}/reversal`),
  reverse: (id: number, txnId: number, reason: string) => http.post<ReceiptsPage>(`${API}/${id}/receipts/${txnId}/reverse`, { reason })
}

export type ReversalPreview = {
  repaymentTxnId: number; repaymentNumber: string; kind: 'RECEIPT' | 'RECOVERY'; receiptAmount: number; allocatedAmount: number
  blocked: string | null; reopensLoan: boolean
  replayed: { repaymentTxnId: number; repaymentNumber: string; valueDate: string; receiptAmount: number; allocatedBefore: number; allocatedAfter: number
    creditKeptBefore: number; creditKeptAfter: number }[]
}
