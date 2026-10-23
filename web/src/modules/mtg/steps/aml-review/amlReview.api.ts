// Workflow step "AML Review" (api/mtg/portfolios/{id}/aml-review): APEX App 105 page 209.
import { http } from '@/shared/api/http'

export type DealDecision = 'SELECTED' | 'EXCLUDED'

export type AmlAccount = {
  portfolioAccountId: number
  investorId: number
  investorCode: string
  investorName: string
  unitCode: string | null
  sellerContractNo: string | null
  contractAmount: number
  paidAmount: number
  outstandingAmount: number
  dealDecisionCode: DealDecision | null
  amlStatusCode: string
}

export type AmlResult = {
  amlResultId: number
  subjectTypeCode: 'DEVELOPER' | 'INVESTOR'
  subjectId: number
  subjectName: string
  amlTypeCode: string
  screeningDate: string
  referenceNo: string | null
  amlResultCode: string
  riskLevelCode: string
  matchScorePct: number | null
  resultDetails: string | null
  createdBy: string
  createdAt: string
}

export type AmlReview = {
  portfolioId: number
  portfolioNo: string
  projectName: string
  projectCode: string
  portfolioStatusCode: string
  portfolioStatusName: string
  rejectionReason: string | null
  isOpen: boolean
  developer: { subjectId: number; code: string; name: string; amlStatusCode: string }
  accounts: AmlAccount[]
  results: AmlResult[]
  options: { amlTypes: string[]; results: string[]; riskLevels: string[] }
}

export type AddAmlResult = {
  subjectTypeCode: 'DEVELOPER' | 'INVESTOR'
  subjectId: number
  amlTypeCode: string
  screeningDate: string
  referenceNo: string | null
  amlResultCode: string
  riskLevelCode: string
  matchScorePct: number | null
  resultDetails: string | null
}

const base = (portfolioId: number) => `/api/mtg/portfolios/${portfolioId}/aml-review`

export const amlReviewApi = {
  get: (portfolioId: number) => http.get<AmlReview>(base(portfolioId)),
  addResult: (portfolioId: number, body: AddAmlResult) => http.post<AmlResult>(`${base(portfolioId)}/results`, body),
  saveDecisions: (portfolioId: number, items: { portfolioAccountId: number; dealDecisionCode: DealDecision }[]) =>
    http.put<void>(`${base(portfolioId)}/deal-decisions`, { items }),
  decide: (portfolioId: number, action: 'ACCEPT' | 'REJECT', comment: string) =>
    http.post<AmlReview>(`${base(portfolioId)}/decision`, { action, comment })
}

/** Results that need a second look before the contract stays in the deal. */
export function isFlagged(code: string): boolean {
  return code === 'POTENTIAL_MATCH' || code === 'CONFIRMED_MATCH' || code === 'REVIEW_REQUIRED'
}
