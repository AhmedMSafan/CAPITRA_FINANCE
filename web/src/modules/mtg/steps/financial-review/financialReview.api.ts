// Workflow step "Portfolio Financial Review" (api/mtg/portfolios/{id}/financial-review): APEX App 105 page 214.
import { http } from '@/shared/api/http'

export type PvMethod = 'ANNUAL' | 'MONTHLY' | 'SIMPLE'
export type MarginBase = 'SELECTED_OUTSTANDING' | 'CHEQUES_PV'
export type AdminBase = 'SELECTED_OUTSTANDING' | 'SELECTED_FACE' | 'CHEQUES_PV' | 'PROPOSED_PURCHASE'

export const PV_METHODS: { code: PvMethod; label: string; formula: string }[] = [
  { code: 'ANNUAL', label: 'Annual', formula: 'amount ÷ (1 + rate)^(days ÷ 365)' },
  { code: 'MONTHLY', label: 'Monthly', formula: 'amount ÷ (1 + rate ÷ 12)^months' },
  { code: 'SIMPLE', label: 'Simple', formula: 'amount ÷ (1 + rate × days ÷ 365)' }
]

export const MARGIN_BASES: { code: MarginBase; label: string }[] = [
  { code: 'SELECTED_OUTSTANDING', label: 'Outstanding of kept contracts' },
  { code: 'CHEQUES_PV', label: "Cheques' present value" }
]

export const ADMIN_BASES: { code: AdminBase; label: string }[] = [
  { code: 'SELECTED_OUTSTANDING', label: 'Outstanding of kept contracts' },
  { code: 'SELECTED_FACE', label: 'Face value of kept contracts' },
  { code: 'CHEQUES_PV', label: "Cheques' present value" },
  { code: 'PROPOSED_PURCHASE', label: 'Present value less margin' }
]

export type FinancialInputs = {
  pvMethodCode: PvMethod
  marginBaseCode: MarginBase
  adminBaseCode: AdminBase
  adminFinalAmount: number | null
  isAdminIncluded: boolean
  legalFinalAmount: number | null
  isLegalIncluded: boolean
  valuationFinalAmount: number | null
  isValuationIncluded: boolean
}

export type FinancialCalculation = {
  pvBaseDate: string
  pvMethodCode: PvMethod
  pvRate: number
  chequeCount: number
  chequesFaceAmount: number
  chequesPvAmount: number
  selectedOutstandingAmount: number
  selectedFaceValueAmount: number
  marginBaseCode: MarginBase
  marginPct: number
  marginBaseAmount: number
  marginAmount: number
  adminBaseCode: AdminBase
  adminExpensePct: number
  adminBaseAmount: number
  adminCalcAmount: number
  adminFinalAmount: number
  isAdminIncluded: boolean
  unitCount: number
  legalFeePerUnitAmount: number
  legalCalcAmount: number
  legalFinalAmount: number
  isLegalIncluded: boolean
  valuationFeePerUnitAmount: number
  valuationCalcAmount: number
  valuationFinalAmount: number
  isValuationIncluded: boolean
  totalChargesAmount: number
  proposedPurchaseAmount: number
  expectedReturnAmount: number
  cheques: { chequeId: number; chequeNo: string; investorName: string; dueDate: string; chequeAmount: number; days: number; months: number; pvAmount: number }[]
}

export type FinancialReview = {
  portfolioId: number
  portfolioNo: string
  projectName: string
  projectCode: string
  currencyCode: string
  portfolioStatusCode: string
  portfolioStatusName: string
  rejectionReason: string | null
  isOpen: boolean
  terms: {
    pvRate: number | null
    marginPct: number | null
    adminExpensePct: number | null
    legalFeePerUnitAmount: number | null
    valuationFeePerUnitAmount: number | null
  }
  inputs: FinancialInputs
  calculation: FinancialCalculation | null
  saved: {
    pvBaseDate: string
    chequeCount: number
    chequesPvAmount: number
    totalChargesAmount: number
    proposedPurchaseAmount: number
    expectedReturnAmount: number
    savedBy: string
    savedAt: string
    rowVersion: number
  } | null
}

const base = (portfolioId: number) => `/api/mtg/portfolios/${portfolioId}/financial-review`

export const financialReviewApi = {
  get: (portfolioId: number) => http.get<FinancialReview>(base(portfolioId)),
  calculate: (portfolioId: number, inputs: FinancialInputs) => http.post<FinancialCalculation>(`${base(portfolioId)}/calculate`, inputs),
  save: (portfolioId: number, inputs: FinancialInputs, rowVersion: number | null) =>
    http.put<FinancialReview>(base(portfolioId), { inputs, rowVersion }),
  decide: (portfolioId: number, action: 'ACCEPT' | 'REJECT', comment: string) =>
    http.post<FinancialReview>(`${base(portfolioId)}/decision`, { action, comment })
}
