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

/** FCT_CREDIT_LIMITS: a customer's credit limit */
export type FctCreditLimit = {
  creditLimitId: number
  limitNumber: string
  limitRef: string | null
  limitName: string
  limitLevel: string
  limitType: string
  limitCategory: string | null
  currencyId: number
  currencyCode: string | null
  approvedAmount: number
  currentLimit: number
  utilizedAmount: number
  availableAmount: number | null
  effectiveAt: string
  expiryAt: string
  isActive: boolean
}

const API = '/api/fct/customers'

export const fctApi = {
  customers: () => http.get<FctCustomer[]>(API),
  customer: (id: number) => http.get<FctCustomerDetails>(`${API}/${id}`),
  applications: (id: number) => http.get<FctApplication[]>(`${API}/${id}/applications`),
  agreements: (id: number) => http.get<FctAgreement[]>(`${API}/${id}/agreements`),
  creditLimits: (id: number) => http.get<FctCreditLimit[]>(`${API}/${id}/credit-limits`)
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

/** FCT_AGREEMENT_DOCUMENTS row, as FINOVA's document list returns it */
export type FctAgreementDocument = {
  documentId: number
  agreementId: number
  docTypeId: number
  documentTitle: string
  fileName: string
  fileExtension: string | null
  fileSizeBytes: number | null
  mimeType: string | null
  statusId: number
  creationDate: string
  createdBy: string
}

export type FctLovItem = { value: number; label: string; code: string }

export const fctDocumentsApi = {
  lookups: () => http.get<{ documentTypes: FctLovItem[]; documentStatuses: FctLovItem[] }>('/api/fct/document-lookups'),
  list: (agreementId: number) => http.get<FctAgreementDocument[]>(`/api/fct/agreements/${agreementId}/documents`),
  upload: (agreementId: number, file: File, docTypeId: number, statusId: number) => {
    const form = new FormData()
    form.append('file', file)
    form.append('docTypeId', String(docTypeId))
    form.append('statusId', String(statusId))
    return http.upload<FctAgreementDocument>(`/api/fct/agreements/${agreementId}/documents`, form)
  },
  content: (documentId: number) => http.blob(`/api/fct/documents/${documentId}/content`)
}

// ---------------------------------------------------------------- the FACTORING_LIMIT_APPROVAL step pages (FINOVA Factoring*Service)

/** The agreement header every step page shows */
export type FctStepSummary = {
  agreementId: number
  agreementNumber: string
  clientId: number
  clientName: string | null
  currencyId: number
  currencyCode: string | null
  facilityLimit: number
  approvedLimit: number | null
  applicationDate: string | null
  relationshipManager: string | null
  statusCode: string | null
  currentStep: string | null
}

export type FctLovOption = { value: number; label: string }

/** Sales Review: the application header form */
export type FctAgreementForm = {
  agreementId: number
  agreementNumber: string
  agreementRef: string | null
  applicationDate: string | null
  clientId: number
  factoringType: string
  recourseType: string
  currencyId: number
  facilityLimit: number
  advanceRatePct: number
  tenorMonths: number | null
  notes: string | null
  statusCode: string | null
  createdBy: string
  clients: FctLovOption[]
  currencies: FctLovOption[]
}

export type FctAgreementUpdate = Pick<FctAgreementForm,
  'applicationDate' | 'clientId' | 'factoringType' | 'recourseType' | 'currencyId' | 'facilityLimit' | 'advanceRatePct' | 'tenorMonths' | 'notes'>

export type FctInquiryDetails = { investigationReportDate: string | null; creditBureauScore: number | null; investigationNotes: string | null }
export type FctAmlDetails = { amlScreeningReportDate: string | null; amlRiskBand: string | null; amlScreeningResult: string | null; amlScreeningNotes: string | null }
export type FctLegalDetails = { legalOpenionReportDate: string | null; legalOpenionNotes: string | null }
export type FctDebtorInquiry = {
  agrmtDebtorId: number
  agreementId: number
  debtorId: number | null
  debtorName: string | null
  visitDate: string | null
  operationalStatus: string | null
  paymentHistory: string | null
  assessment: string | null
}
export type FctInquiryFieldReport = { summary: FctStepSummary; inquiry: FctInquiryDetails; aml: FctAmlDetails; legal: FctLegalDetails; debtors: FctDebtorInquiry[] }

export type FctFinancialAnalysis = { fsAssessmentId: number | null; fsAssessmentDate: string | null; fsAssessmentResult: string | null; fsNotes: string | null }
export type FctRiskRating = { rrAssessmentId: number | null; rrAssessmentDate: string | null; rrAssessmentResult: string | null; rrNotes: string | null }
export type FctCreditMemo = {
  creditMemoDate: string | null
  recommendedLimt: number | null
  recommendedAdvRate: number | null
  recommendedTenorMth: number | null
  creditNotes: string | null
}
export type FctReferenceDocument = {
  sourceType: string
  documentId: number
  documentTitle: string | null
  fileName: string | null
  mimeType: string | null
  creationDate: string
  createdBy: string
}
export type FctCreditAnalysis = {
  summary: FctStepSummary
  financialAnalysis: FctFinancialAnalysis
  riskRating: FctRiskRating
  creditMemo: FctCreditMemo
  referenceDocuments: FctReferenceDocument[]
}

export type FctRiskReview = {
  summary: FctStepSummary
  indicators: { creditBureauScore: number | null; recourseType: string | null; tenorMonths: number | null; rrAssessmentResult: string | null }
  riskReview: { riskMemoDate: string | null; riskNotes: string | null; riskDecision: string | null }
}

export type FctCommitteeDecisionTerms = { approvalDate: string | null; approvedLimit: number | null; approvedAdvRate: number | null; approvedTenorMth: number | null }
export type FctCommitteeDecision = { summary: FctStepSummary; decision: FctCommitteeDecisionTerms }

export type FctApprovedTerms = { approvedLimit: number | null; approvedAdvRate: number | null; approvedTenorMth: number | null; effectiveRatePct: number | null }
export type FctContractRequest = { summary: FctStepSummary; approvedTerms: FctApprovedTerms; notesToOperations: string | null }

export type FctContract = {
  agreementNumber: string | null
  effectiveDate: string | null
  tenorMonths: number | null
  contractStartDate: string | null
  contractEndDate: string | null
  constractSignoffDate: string | null
  additionalTermsConditions: string | null
}
export type FctContractGeneration = { summary: FctStepSummary; contract: FctContract; approvedTerms: FctApprovedTerms }

export type FctLegalReviewDetails = { legalReviewDate: string | null; legalOpinionNotes: string | null }
export type FctLegalReview = { summary: FctStepSummary; legalReview: FctLegalReviewDetails }

export const fctStepsApi = {
  agreement: (id: number) => http.get<FctAgreementForm>(`/api/fct/agreements/${id}`),
  updateAgreement: (id: number, body: FctAgreementUpdate) => http.put<FctAgreementForm>(`/api/fct/agreements/${id}`, body),
  inquiry: (id: number) => http.get<FctInquiryFieldReport>(`/api/fct/inquiry-field-report/${id}`),
  saveInquiry: (id: number, body: { inquiry: FctInquiryDetails; aml: FctAmlDetails; legal: FctLegalDetails }) =>
    http.put<FctInquiryFieldReport>(`/api/fct/inquiry-field-report/${id}`, body),
  creditAnalysis: (id: number) => http.get<FctCreditAnalysis>(`/api/fct/credit-analysis/${id}`),
  saveFinancialAnalysis: (id: number, body: FctFinancialAnalysis) =>
    http.put<FctFinancialAnalysis>(`/api/fct/credit-analysis/${id}/financial-analysis`, body),
  saveRiskRating: (id: number, body: FctRiskRating) => http.put<FctRiskRating>(`/api/fct/credit-analysis/${id}/risk-rating`, body),
  saveCreditMemo: (id: number, body: FctCreditMemo) => http.put<FctCreditMemo>(`/api/fct/credit-analysis/${id}/credit-memo`, body),
  saveCreditAnalysisProgress: (id: number, body: { financialAnalysis: FctFinancialAnalysis; riskRating: FctRiskRating; creditMemo: FctCreditMemo }) =>
    http.put<FctCreditAnalysis>(`/api/fct/credit-analysis/${id}/save-progress`, body),
  riskReview: (id: number) => http.get<FctRiskReview>(`/api/fct/risk-review/${id}`),
  saveRiskReview: (id: number, body: { riskMemoDate: string | null; riskNotes: string | null }) =>
    http.put<FctRiskReview>(`/api/fct/risk-review/${id}`, body),
  committeeDecision: (id: number) => http.get<FctCommitteeDecision>(`/api/fct/committee-decision/${id}`),
  saveCommitteeDecision: (id: number, body: FctCommitteeDecisionTerms) => http.put<FctCommitteeDecision>(`/api/fct/committee-decision/${id}`, body),
  contractRequest: (id: number) => http.get<FctContractRequest>(`/api/fct/contract-request/${id}`),
  saveContractRequest: (id: number, body: { notesToOperations: string | null }) => http.put<FctContractRequest>(`/api/fct/contract-request/${id}`, body),
  contractGeneration: (id: number) => http.get<FctContractGeneration>(`/api/fct/contract-generation/${id}`),
  saveContractGeneration: (id: number, body: FctContract) => http.put<FctContractGeneration>(`/api/fct/contract-generation/${id}`, body),
  legalReview: (id: number) => http.get<FctLegalReview>(`/api/fct/legal-review/${id}`),
  saveLegalReview: (id: number, body: FctLegalReviewDetails) => http.put<FctLegalReview>(`/api/fct/legal-review/${id}`, body)
}
