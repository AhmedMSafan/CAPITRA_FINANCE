// Workflow step "Portfolio Board Review" (api/mtg/portfolios/{id}/board-review): APEX App 105 page 215.
import { http } from '@/shared/api/http'

export type BoardAction = 'ACCEPT' | 'REJECT'

export type BoardReview = {
  portfolioId: number
  portfolioNo: string
  developerName: string
  projectName: string
  projectCode: string
  currencyCode: string
  requestDate: string
  submittedDate: string | null
  portfolioStatusCode: string
  portfolioStatusName: string
  rejectionReason: string | null
  isOpen: boolean
  terms: {
    coveragePct: number | null
    marginPct: number | null
    pvRate: number | null
    reservedAmountPct: number | null
    lateFeeRate: number | null
    adminExpensePct: number | null
    legalFeePerUnitAmount: number | null
    valuationFeePerUnitAmount: number | null
    termMonths: number | null
  }
  totals: {
    contractCount: number
    investorCount: number
    faceValueAmount: number
    outstandingAmount: number
    paidAmount: number
    selectedContractCount: number
    selectedInvestorCount: number
    selectedUnitCount: number
    selectedFaceValueAmount: number
    selectedOutstandingAmount: number
    selectedPaidAmount: number
    chequeCount: number
    chequeAmount: number
  }
  pricing: {
    pvMethodCode: string
    pvRate: number
    pvBaseDate: string
    chequeCount: number
    chequesFaceAmount: number
    chequesPvAmount: number
    marginPct: number
    marginAmount: number
    adminDeductedAmount: number
    legalDeductedAmount: number
    valuationDeductedAmount: number
    totalChargesAmount: number
    proposedPurchaseAmount: number
    expectedReturnAmount: number
  } | null
  dueDiligence: {
    amlCount: number
    amlFlaggedCount: number
    inquiryCount: number
    inquiryHighRiskCount: number
    legal: {
      isOwnershipVerified: boolean
      isBuildingLicenseVerified: boolean
      isTitleDeedVerified: boolean
      isEncumbranceFree: boolean
      isRegistrationPossible: boolean
      isMortgagePossible: boolean
      legalRecommendation: string | null
      reviewDate: string | null
    } | null
    valuation: {
      valuerName: string
      valuationDate: string
      marketValueAmount: number
      recommendedValueAmount: number | null
      projectCompletionPct: number | null
    } | null
  }
  investors: {
    investorId: number
    investorCode: string
    investorName: string
    contractCount: number
    selectedContractCount: number
    contractAmount: number
    paidAmount: number
    outstandingAmount: number
    chequeAmount: number
    isIncluded: boolean
  }[]
  history: { fromStatusName: string | null; toStatusName: string; comments: string | null; changedBy: string; changedAt: string }[]
  decision: { boardDecisionCode: 'ACCEPTED' | 'REJECTED'; boardDecisionNo: string | null; boardDecisionDate: string | null; approvedDate: string | null } | null
}

export type BoardDecisionRequest = { action: BoardAction; decisionNo: string; decisionDate: string | null; comment: string }

const base = (portfolioId: number) => `/api/mtg/portfolios/${portfolioId}/board-review`

export const boardReviewApi = {
  get: (portfolioId: number) => http.get<BoardReview>(base(portfolioId)),
  decide: (portfolioId: number, request: BoardDecisionRequest) => http.post<BoardReview>(`${base(portfolioId)}/decision`, request)
}
