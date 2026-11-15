// The Lending Workspace (api/lnd/customers): the customers, and one customer's loans and payments.
import { http } from '@/shared/api/http'

export type LwCustomer = {
  clientId: number; clientNumber: string; clientName: string; clientType: string; phoneNumber: string | null; email: string | null; isActive: boolean
  activeLoans: number; outstandingPrincipal: number
}

export type LwLoan = {
  loanId: number; loanNumber: string; productCode: string; productName: string; licenceCode: string; statusCode: string; principalAmount: number
  principalOutstanding: number; overdueAmount: number; daysLate: number; isNonAccrual: boolean; nextDueDate: string | null; nextDueAmount: number
  disbursementDate: string; maturityDate: string
}

export type LwPayment = {
  repaymentTxnId: number; repaymentNumber: string; loanNumber: string; kindCode: string; valueDate: string; method: string; receiptAmount: number
  allocatedAmount: number; statusCode: string
}

export type LwCustomerWorkspace = {
  clientId: number; clientNumber: string; clientName: string; clientNameAr: string; clientType: string; nationalId: string | null; phoneNumber: string | null
  mobileNumber: string | null; email: string | null; relationshipManager: string | null; today: string
  summary: { activeLoans: number; outstandingPrincipal: number; overdueAmount: number; maxDaysLate: number; creditBalance: number }
  loans: LwLoan[]; payments: LwPayment[]
}

const API = '/api/lnd/customers'

export const lendingWsApi = {
  customers: () => http.get<LwCustomer[]>(API),
  customer: (clientId: number) => http.get<LwCustomerWorkspace>(`${API}/${clientId}`)
}
