// Workflow step "Portfolio Valuation" (api/mtg/portfolios/{id}/valuation): APEX App 105 page 401.
import { http } from '@/shared/api/http'

export type Valuation = {
  valuationId: number
  valuationTypeCode: string
  valuationTypeName: string
  projectUnitId: number | null
  unitCode: string | null
  valuerName: string
  valuerRegistrationNo: string | null
  valuationDate: string
  marketValueAmount: number
  recommendedValueAmount: number | null
  projectCompletionPct: number | null
  facilitiesStatus: string | null
  approvedBy: string | null
  approvedDate: string | null
  createdBy: string
  createdAt: string
  rowVersion: number
}

export type Valuations = {
  portfolioId: number
  portfolioNo: string
  developerName: string
  projectName: string
  projectCode: string
  portfolioStatusCode: string
  portfolioStatusName: string
  isOpen: boolean
  faceValueAmount: number
  outstandingAmount: number
  valuations: Valuation[]
  units: { projectUnitId: number; unitCode: string; unitTypeCode: string | null; marketValueAmount: number | null }[]
  valuationTypes: { code: string; name: string }[]
}

export type SaveValuation = {
  valuationTypeCode: string
  projectUnitId: number | null
  valuerName: string
  valuerRegistrationNo: string | null
  valuationDate: string | null
  marketValueAmount: number | null
  recommendedValueAmount: number | null
  projectCompletionPct: number | null
  facilitiesStatus: string | null
  approvedBy: string | null
  approvedDate: string | null
  rowVersion?: number
}

const base = (portfolioId: number) => `/api/mtg/portfolios/${portfolioId}/valuation`

export const valuationApi = {
  get: (portfolioId: number) => http.get<Valuations>(base(portfolioId)),
  create: (portfolioId: number, body: SaveValuation) => http.post<Valuation>(`${base(portfolioId)}/records`, body),
  update: (portfolioId: number, id: number, body: SaveValuation) => http.put<Valuation>(`${base(portfolioId)}/records/${id}`, body),
  remove: (portfolioId: number, row: Valuation) =>
    http.delete<void>(`${base(portfolioId)}/records/${row.valuationId}?rowVersion=${row.rowVersion}`),
  complete: (portfolioId: number, comment: string) => http.post<Valuations>(`${base(portfolioId)}/complete`, { comment })
}
