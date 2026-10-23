// Workflow step "Portfolio Purchase Agreement" (api/mtg/portfolios/{id}/purchase-agreement): APEX App 105 page 216.
import { http } from '@/shared/api/http'

export type PurchaseAgreement = {
  portfolioId: number
  portfolioNo: string
  developerNameEn: string
  developerNameAr: string
  projectNameEn: string
  projectNameAr: string
  projectCode: string
  currencyCode: string
  portfolioStatusCode: string
  portfolioStatusName: string
  isBoardApproved: boolean
  proposedPurchaseAmount: number
  pvRate: number | null
  reservedAmountPct: number | null
  lateFeeRate: number | null
  boardDecisionNo: string | null
  boardDecisionDate: string | null
  contractCount: number
  unitCount: number
  outstandingAmount: number
  chequeCount: number
  chequeAmount: number
  adminDeductedAmount: number | null
  legalDeductedAmount: number | null
  valuationDeductedAmount: number | null
  contracts: { investorNameEn: string; investorNameAr: string; sellerContractNo: string | null; unitCode: string | null; outstandingAmount: number }[]
  cheques: { investorName: string; chequeNo: string; bankName: string | null; dueDate: string; chequeAmount: number }[]
}

export const purchaseAgreementApi = {
  get: (portfolioId: number) => http.get<PurchaseAgreement>(`/api/mtg/portfolios/${portfolioId}/purchase-agreement`)
}
