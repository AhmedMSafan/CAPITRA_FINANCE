// Workflow step "Portfolio Coverage Review" (api/mtg/portfolios/{id}/coverage-review): APEX App 105 page 212.
import { http } from '@/shared/api/http'

export type CoverageInvestor = {
  investorId: number
  investorCode: string
  investorName: string
  contractCount: number
  contractAmount: number
  paidAmount: number
  outstandingAmount: number
  paidPct: number | null
  meetsCoverage: boolean | null
  dealDecisionCode: 'SELECTED' | 'EXCLUDED' | 'MIXED' | null
}

export type CoverageReview = {
  portfolioId: number
  portfolioNo: string
  projectName: string
  projectCode: string
  portfolioStatusCode: string
  portfolioStatusName: string
  rejectionReason: string | null
  isOpen: boolean
  requiredCoveragePct: number | null
  investors: CoverageInvestor[]
}

const base = (portfolioId: number) => `/api/mtg/portfolios/${portfolioId}/coverage-review`

export const coverageReviewApi = {
  get: (portfolioId: number) => http.get<CoverageReview>(base(portfolioId)),
  applyRule: (portfolioId: number) =>
    http.post<{ changedInvestorCount: number; review: CoverageReview }>(`${base(portfolioId)}/apply-rule`, {}),
  setDecision: (portfolioId: number, investorId: number, dealDecisionCode: 'SELECTED' | 'EXCLUDED') =>
    http.put<CoverageReview>(`${base(portfolioId)}/investors/${investorId}/decision`, { dealDecisionCode }),
  decide: (portfolioId: number, action: 'ACCEPT' | 'REJECT', comment: string) =>
    http.post<CoverageReview>(`${base(portfolioId)}/decision`, { action, comment })
}
