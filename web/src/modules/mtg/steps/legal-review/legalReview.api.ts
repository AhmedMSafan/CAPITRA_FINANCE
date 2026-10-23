// Workflow step "Portfolio Legal Review" (api/mtg/portfolios/{id}/legal-review): APEX App 105 page 211.
import { http } from '@/shared/api/http'

export type LegalCheck =
  | 'isOwnershipVerified'
  | 'isBuildingLicenseVerified'
  | 'isRegistrationPossible'
  | 'isMortgagePossible'
  | 'isEncumbranceFree'
  | 'isTitleDeedVerified'

/** The six checks, in the order the legal team works through them. */
export const LEGAL_CHECKS: { key: LegalCheck; label: string; hint: string; icon: string }[] = [
  { key: 'isOwnershipVerified', label: 'Ownership verified', hint: 'The developer holds clear title to the land and units.', icon: 'pi pi-home' },
  { key: 'isBuildingLicenseVerified', label: 'Building license verified', hint: 'Valid building permits cover the sold units.', icon: 'pi pi-building' },
  { key: 'isRegistrationPossible', label: 'Registration possible', hint: "Units can be registered in the buyers' names.", icon: 'pi pi-pen-to-square' },
  { key: 'isMortgagePossible', label: 'Mortgage possible', hint: 'A mortgage can be registered over the units.', icon: 'pi pi-building-columns' },
  { key: 'isEncumbranceFree', label: 'Free of encumbrances', hint: 'No liens, seizures or third-party claims.', icon: 'pi pi-lock-open' },
  { key: 'isTitleDeedVerified', label: 'Title deed verified', hint: 'The title deed was checked against the registry.', icon: 'pi pi-file' }
]

export type LegalReviewForm = Record<LegalCheck, boolean> & {
  legalFindings: string | null
  legalRecommendation: string | null
  reviewedBy: string | null
  reviewDate: string | null
  approvedBy: string | null
  approvedDate: string | null
}

export type SavedLegalReview = LegalReviewForm & {
  isComplete: boolean
  savedBy: string
  savedAt: string
  rowVersion: number
}

export type LegalReviewAccount = {
  portfolioAccountId: number
  investorCode: string
  investorName: string
  unitCode: string | null
  sellerContractNo: string | null
  contractAmount: number
  paidAmount: number
  outstandingAmount: number
  dealDecisionCode: 'SELECTED' | 'EXCLUDED' | null
}

export type LegalReview = {
  portfolioId: number
  portfolioNo: string
  developerName: string
  projectName: string
  projectCode: string
  portfolioStatusCode: string
  portfolioStatusName: string
  isOpen: boolean
  review: SavedLegalReview | null
  accounts: LegalReviewAccount[]
}

const base = (portfolioId: number) => `/api/mtg/portfolios/${portfolioId}/legal-review`

export const legalReviewApi = {
  get: (portfolioId: number) => http.get<LegalReview>(base(portfolioId)),
  save: (portfolioId: number, body: LegalReviewForm & { rowVersion: number | null }) => http.put<LegalReview>(base(portfolioId), body),
  setDecision: (portfolioId: number, portfolioAccountId: number, dealDecisionCode: 'SELECTED' | 'EXCLUDED') =>
    http.put<void>(`${base(portfolioId)}/deal-decisions`, { items: [{ portfolioAccountId, dealDecisionCode }] }),
  decide: (portfolioId: number, action: 'ACCEPT' | 'REJECT', comment: string) =>
    http.post<LegalReview>(`${base(portfolioId)}/decision`, { action, comment })
}
