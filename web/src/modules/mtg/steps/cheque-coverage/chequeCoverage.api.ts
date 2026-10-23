// Workflow step "Portfolio Cheque Coverage Review" (api/mtg/portfolios/{id}/cheque-coverage): APEX App 105 page 213.
import { http } from '@/shared/api/http'

export type ChequeInvestor = {
  investorId: number
  investorCode: string
  investorName: string
  contractCount: number
  outstandingAmount: number
  chequeCount: number
  chequeAmount: number
  isCovered: boolean
}

export type ChequeAccount = {
  portfolioAccountId: number
  investorId: number
  sellerContractNo: string | null
  unitCode: string | null
  outstandingAmount: number
}

export type Cheque = {
  chequeId: number
  portfolioAccountId: number
  investorId: number
  sellerContractNo: string | null
  unitCode: string | null
  chequeNo: string
  bankName: string | null
  chequeDate: string | null
  dueDate: string
  chequeAmount: number
  isEndorsedToCompany: boolean
  chequeStatusCode: string
  chequeStatusName: string
  clearingDate: string | null
  returnReason: string | null
  rowVersion: number
}

export type ChequeCoverage = {
  portfolioId: number
  portfolioNo: string
  projectName: string
  projectCode: string
  currencyCode: string
  portfolioStatusCode: string
  portfolioStatusName: string
  rejectionReason: string | null
  isOpen: boolean
  investors: ChequeInvestor[]
  accounts: ChequeAccount[]
  cheques: Cheque[]
  chequeStatuses: { code: string; name: string }[]
}

export type SaveCheque = {
  portfolioAccountId: number
  chequeNo: string
  bankName: string | null
  chequeDate: string | null
  dueDate: string | null
  chequeAmount: number | null
  isEndorsedToCompany: boolean
  chequeStatusCode: string | null
  clearingDate: string | null
  returnReason: string | null
  rowVersion?: number
}

const base = (portfolioId: number) => `/api/mtg/portfolios/${portfolioId}/cheque-coverage`

export const chequeCoverageApi = {
  get: (portfolioId: number) => http.get<ChequeCoverage>(base(portfolioId)),
  create: (portfolioId: number, body: SaveCheque) => http.post<Cheque>(`${base(portfolioId)}/cheques`, body),
  update: (portfolioId: number, id: number, body: SaveCheque) => http.put<Cheque>(`${base(portfolioId)}/cheques/${id}`, body),
  remove: (portfolioId: number, row: Cheque) => http.delete<void>(`${base(portfolioId)}/cheques/${row.chequeId}?rowVersion=${row.rowVersion}`),
  decide: (portfolioId: number, action: 'ACCEPT' | 'REJECT', comment: string) =>
    http.post<ChequeCoverage>(`${base(portfolioId)}/decision`, { action, comment })
}
