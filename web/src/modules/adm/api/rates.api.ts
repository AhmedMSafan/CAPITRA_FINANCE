import { http } from '@/shared/api/http'

/** ADM_RATE_INDEXES with the rate in force today. */
export type RateIndex = {
  rateIndexId: number
  rateIndexCode: string
  rateIndexName: string
  currencyId: number | null
  currencyCode: string | null
  isActive: boolean
  currentRatePct: number | null
  periodCount: number
  rowVersion: number
}

export type SaveRateIndex = {
  rateIndexCode: string
  rateIndexName: string
  currencyId: number | null
  isActive: boolean
  rowVersion: number | null
}

/** A rate period: it ends the day before the next one starts; the latest has no end. */
export type RatePeriod = {
  rateValueId: number
  startDate: string
  endDate: string | null
  annualRatePct: number
  isCurrent: boolean
  isPast: boolean
  changedBy: string
  changedAt: string
  rowVersion: number
}

export type SaveRatePeriod = { startDate: string | null; annualRatePct: number | null; rowVersion: number | null }

const API = '/api/adm/rate-indexes'

export const ratesApi = {
  list: () => http.get<RateIndex[]>(API),
  currencies: () => http.get<{ currencyId: number; currencyCode: string }[]>(`${API}/currencies`),
  create: (body: SaveRateIndex) => http.post<RateIndex>(API, body),
  update: (id: number, body: SaveRateIndex) => http.put<RateIndex>(`${API}/${id}`, body),
  periods: (id: number) => http.get<RatePeriod[]>(`${API}/${id}/periods`),
  addPeriod: (id: number, body: SaveRatePeriod) => http.post<RatePeriod[]>(`${API}/${id}/periods`, body),
  updatePeriod: (id: number, valueId: number, body: SaveRatePeriod) => http.put<RatePeriod[]>(`${API}/${id}/periods/${valueId}`, body),
  removePeriod: (id: number, valueId: number, rowVersion: number) =>
    http.delete<RatePeriod[]>(`${API}/${id}/periods/${valueId}?rowVersion=${rowVersion}`)
}
