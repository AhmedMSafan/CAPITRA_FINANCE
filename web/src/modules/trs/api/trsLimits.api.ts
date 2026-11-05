// Treasury Bank Limits (api/trs/limits): each bank's facilities as a tree of tranches and sublimits with live usage, and the bank rules.
import { http } from '@/shared/api/http'

export type TrsOption = { value: string; label: string }

export type TrsSublimitNode = {
  sublimitId: number
  trancheId: number
  sublimitName: string
  limitAmount: number
  poolTypeCode: string
  statusCode: string
  notes: string | null
  businessLines: string[]
  outstanding: number
  reserved: number
  available: number
}

export type TrsTrancheNode = {
  trancheId: number
  facilityId: number
  trancheName: string
  trancheAmount: number
  statusCode: string
  releasedDate: string | null
  maturityDate: string | null
  notes: string | null
  outstanding: number
  reserved: number
  available: number
  sublimits: TrsSublimitNode[]
}

export type TrsFacilityNode = {
  facilityId: number
  bankId: number
  facilityRef: string
  facilityTypeCode: string
  currencyCode: string
  approvedLimit: number
  interestRateType: string
  baseRateCode: string | null
  marginBps: number | null
  allInRatePct: number | null
  startDate: string
  maturityDate: string
  facilityStatus: string
  isRevolving: boolean
  outstanding: number
  reserved: number
  available: number
  notReleased: number
  tranches: TrsTrancheNode[]
}

export type TrsRuleUsage = { subjectName: string; exposure: number; pct: number | null; level: 'OK' | 'WARN' | 'BREACH' }

export type TrsBankRule = {
  ruleId: number
  bankId: number
  facilityId: number | null
  facilityRef: string | null
  ruleTypeCode: string
  subjectTypeCode: string
  subjectClientId: number | null
  subjectValue: string | null
  subjectLabel: string
  limitPct: number | null
  limitAmount: number | null
  warningPct: number | null
  maxTenorDays: number | null
  basisCode: string
  breachActionCode: string
  statusCode: string
  changedBy: string
  changedAt: string
  verifiedBy: string | null
  verifiedAt: string | null
  notes: string | null
  canVerify: boolean
  verifyBlockedReason: string | null
  highest: TrsRuleUsage | null
}

export type TrsBankNode = {
  bankId: number
  bankCode: string
  bankName: string
  approvedLimit: number
  outstanding: number
  reserved: number
  available: number
  facilities: TrsFacilityNode[]
  rules: TrsBankRule[]
}

export type TrsLimits = { banks: TrsBankNode[] }

export type TrsLimitsOptions = {
  businessLines: TrsOption[]
  facilityTypes: TrsOption[]
  ruleTypes: TrsOption[]
  subjectTypes: TrsOption[]
  bases: TrsOption[]
  breachActions: TrsOption[]
  subjects: { clients: { clientId: number; clientNumber: string; clientName: string }[]; groups: string[] }
}

export type TrsFacilityForm = {
  bankId: number
  facilityRef: string
  facilityTypeCode: string
  currencyCode: string
  approvedLimit: number
  interestRateType: string
  baseRateCode: string | null
  marginBps: number | null
  allInRatePct: number | null
  startDate: string
  maturityDate: string
  isRevolving: boolean
  facilityStatus: string
}

export type TrsTrancheForm = {
  trancheName: string
  trancheAmount: number
  statusCode: string
  releasedDate: string | null
  maturityDate: string | null
  notes: string | null
}

export type TrsSublimitForm = {
  sublimitName: string
  limitAmount: number
  poolTypeCode: string
  statusCode: string
  businessLines: string[]
  notes: string | null
}

export type TrsBankRuleForm = {
  bankId: number
  facilityId: number | null
  ruleTypeCode: string
  subjectTypeCode: string
  subjectClientId: number | null
  subjectValue: string | null
  limitPct: number | null
  limitAmount: number | null
  warningPct: number | null
  maxTenorDays: number | null
  basisCode: string
  breachActionCode: string
  notes: string | null
}

const base = '/api/trs/limits'

export const trsLimitsApi = {
  get: () => http.get<TrsLimits>(base),
  options: () => http.get<TrsLimitsOptions>(`${base}/options`),
  saveFacility: (id: number | null, body: TrsFacilityForm) =>
    id ? http.put<number>(`${base}/facilities/${id}`, body) : http.post<number>(`${base}/facilities`, body),
  saveTranche: (facilityId: number, id: number | null, body: TrsTrancheForm) =>
    id ? http.put<number>(`${base}/facilities/${facilityId}/tranches/${id}`, body) : http.post<number>(`${base}/facilities/${facilityId}/tranches`, body),
  saveSublimit: (trancheId: number, id: number | null, body: TrsSublimitForm) =>
    id ? http.put<number>(`${base}/tranches/${trancheId}/sublimits/${id}`, body) : http.post<number>(`${base}/tranches/${trancheId}/sublimits`, body),
  saveRule: (id: number | null, body: TrsBankRuleForm) => (id ? http.put<number>(`${base}/rules/${id}`, body) : http.post<number>(`${base}/rules`, body)),
  verifyRule: (id: number) => http.post<void>(`${base}/rules/${id}/verify`, {}),
  deactivateRule: (id: number, notes: string) => http.post<void>(`${base}/rules/${id}/deactivate`, { notes })
}
