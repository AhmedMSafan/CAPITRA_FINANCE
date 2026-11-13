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
  licenceCode: string; statusCode: string; isNonAccrual: boolean; currencyCode: string; principalAmount: number; termMonths: number; installmentCount: number
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
  withdrawPayout: (id: number, d: number) => http.post<DisbursementPage>(`${API}/${id}/disbursements/${d}/withdraw`, {})
}
