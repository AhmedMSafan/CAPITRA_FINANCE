// Workflow step "Portfolio Inquiries" (api/mtg/portfolios/{id}/inquiries): APEX App 105 page 210.
import { http } from '@/shared/api/http'

export type SubjectType = 'PORTFOLIO' | 'DEVELOPER' | 'PROJECT' | 'INVESTOR' | 'SIGNATORY' | 'GUARANTEE'

export const SUBJECT_TYPES: { code: SubjectType; label: string }[] = [
  { code: 'PORTFOLIO', label: 'Portfolio' },
  { code: 'DEVELOPER', label: 'Developer' },
  { code: 'PROJECT', label: 'Project' },
  { code: 'INVESTOR', label: 'Investor' },
  { code: 'SIGNATORY', label: 'Signatory' },
  { code: 'GUARANTEE', label: 'Guarantee' }
]

export type Inquiry = {
  inquiryResultId: number
  subjectTypeCode: SubjectType
  subjectId: number
  subjectName: string
  inquiryTypeCode: string
  inquiryTypeName: string
  inquiryDate: string | null
  referenceNo: string | null
  resultText: string | null
  riskLevelCode: string | null
  resultDetails: string | null
  createdBy: string
  createdAt: string
  rowVersion: number
}

export type InquiryAccount = {
  portfolioAccountId: number
  investorId: number
  investorCode: string
  investorName: string
  unitCode: string | null
  sellerContractNo: string | null
  contractAmount: number
  outstandingAmount: number
  dealDecisionCode: 'SELECTED' | 'EXCLUDED' | null
  inquiryCount: number
  latestRiskLevelCode: string | null
}

export type Inquiries = {
  portfolioId: number
  portfolioNo: string
  projectName: string
  projectCode: string
  portfolioStatusCode: string
  portfolioStatusName: string
  isOpen: boolean
  subjects: { subjectTypeCode: SubjectType; subjectId: number; name: string }[]
  inquiries: Inquiry[]
  accounts: InquiryAccount[]
  inquiryTypes: { code: string; name: string }[]
  riskLevels: string[]
}

export type SaveInquiry = {
  subjectTypeCode: SubjectType
  subjectId: number | null
  inquiryTypeCode: string
  inquiryDate: string | null
  referenceNo: string | null
  resultText: string | null
  riskLevelCode: string | null
  resultDetails: string | null
  rowVersion?: number
}

const base = (portfolioId: number) => `/api/mtg/portfolios/${portfolioId}/inquiries`

export const inquiriesApi = {
  get: (portfolioId: number) => http.get<Inquiries>(base(portfolioId)),
  create: (portfolioId: number, body: SaveInquiry) => http.post<Inquiry>(`${base(portfolioId)}/records`, body),
  update: (portfolioId: number, id: number, body: SaveInquiry) => http.put<Inquiry>(`${base(portfolioId)}/records/${id}`, body),
  remove: (portfolioId: number, row: Inquiry) =>
    http.delete<void>(`${base(portfolioId)}/records/${row.inquiryResultId}?rowVersion=${row.rowVersion}`),
  saveDecisions: (portfolioId: number, ids: number[], dealDecisionCode: 'SELECTED' | 'EXCLUDED') =>
    http.put<void>(`${base(portfolioId)}/deal-decisions`, { items: ids.map(id => ({ portfolioAccountId: id, dealDecisionCode })) }),
  decide: (portfolioId: number, action: 'APPROVE' | 'REJECT', comment: string) =>
    http.post<Inquiries>(`${base(portfolioId)}/decision`, { action, comment })
}
