// Factoring customers and the Factoring Customer Workspace (api/fct/customers), from FINOVA_FINANCEING.
import { http } from '@/shared/api/http'

/** CRM_CLIENTS, as the Factoring home lists them */
export type FctCustomer = {
  clientId: number
  clientNumber: string
  clientName: string
  clientType: string
  phoneNumber: string | null
  email: string | null
  isActive: boolean
}

/** CRM_CLIENTS, as the workspace header shows it */
export type FctCustomerDetails = {
  clientId: number
  clientNumber: string
  clientName: string
  clientNameAr: string
  clientType: string
  countryName: string | null
  currencyCode: string | null
  relationshipManager: string | null
  phoneNumber: string | null
  email: string | null
}

/** FCT_OFFERS: a factoring application */
export type FctApplication = {
  offerId: number
  offerRef: string
  offerAt: string | null
  requestedLimit: number
  currencyCode: string | null
  factoringType: string
  statusCode: string | null
  agreementNo: string | null
  workflowStepName: string | null
}

/** FCT_AGREEMENTS */
export type FctAgreement = {
  agreementId: number
  agreementNumber: string
  applicationAt: string | null
  facilityLimit: number
  currencyCode: string | null
  factoringType: string
  statusCode: string | null
  workflowStepName: string | null
}

const API = '/api/fct/customers'

export const fctApi = {
  customers: () => http.get<FctCustomer[]>(API),
  customer: (id: number) => http.get<FctCustomerDetails>(`${API}/${id}`),
  applications: (id: number) => http.get<FctApplication[]>(`${API}/${id}/applications`),
  agreements: (id: number) => http.get<FctAgreement[]>(`${API}/${id}/agreements`)
}

/** Calculator inputs; percentages as entered (90 means 90%) */
export type FctCalculationRequest = {
  requestedLimit: number | null
  expectedVolume: number | null
  averageTenorDays: number | null
  numDebtors: number | null
  estTransactionsPerYear: number | null
  riskBand: string
  baseAdvanceRatio: number | null
  dilutionRate: number | null
  riskBufferOverride: number | null
  largestConcentration: number | null
  costOfFundsRate: number | null
  operatingSpread: number | null
  setupFeeFixed: number | null
  setupFeePct: number | null
  commissionPct: number | null
  handlingFee: number | null
  debtorFee: number | null
  debtorMinimum: number | null
}

export type FctCalculationResult = {
  advanceBasePct: number
  advanceDilutionPct: number
  advanceRiskBufferPct: number
  advancePct: number
  discountCostOfFundsPct: number
  discountRiskMarginPct: number
  discountOperatingSpreadPct: number
  discountTenorAdjustmentPct: number
  discountConcentrationPct: number
  discountRatePct: number
  spreadOverLcrPct: number
  setupFeeTotal: number
  commissionAnnual: number
  handlingAnnual: number
  debtorFeesTotal: number
  interestIncome: number
  totalAnnualIncome: number
}

export type FctCreateOfferRequest = {
  clientId: number
  notes: string
  factoringType: string
  recourseType: string
  requestedLimit: number
  averageTenorDays: number
  costOfFundsRate: number
  operatingSpread: number
  riskBand: string
  advancePct: number
  advanceDilutionPct: number
  discountRatePct: number
  spreadOverLcrPct: number
}

export type FctCreateOfferResult = { offerId: number; offerRef: string; statusCode: string; existingDraft: boolean; message: string }

export type FctStartWorkflowResult = { agreementId: number; agreementNumber: string; offerId: number; offerRef: string; offerStatusCode: string; message: string }

export const fctApplicationsApi = {
  calculate: (body: FctCalculationRequest) => http.post<FctCalculationResult>('/api/fct/calculator/calculate', body),
  createFromCalculator: (body: FctCreateOfferRequest) => http.post<FctCreateOfferResult>('/api/fct/applications/from-calculator', body),
  setStatus: (offerId: number, statusCode: string) => http.put<void>(`/api/fct/applications/${offerId}/status`, { statusCode }),
  startWorkflow: (offerId: number) => http.post<FctStartWorkflowResult>(`/api/fct/applications/${offerId}/start-workflow`)
}

/** FINOVA AgreementReviewSummaryDto: the agreement header every factoring workflow step page shows */
export type FctAgreementReviewSummary = {
  agreementId: number
  agreementNumber: string | null
  clientId: number | null
  clientName: string | null
  currencyCode: string | null
  facilityLimit: number | null
  approvedLimit: number | null
  applicationDate: string | null
  effectiveDate: string | null
  statusCode: string | null
}

export type FctUser = { id: string; userName: string | null; email: string | null; fullName: string | null; employeeNo: string | null }

export const fctReviewApi = {
  reviewSummary: (agreementId: number) => http.get<FctAgreementReviewSummary>(`/api/fct/agreements/${agreementId}/review-summary`),
  users: () => http.get<FctUser[]>('/api/fct/users')
}
