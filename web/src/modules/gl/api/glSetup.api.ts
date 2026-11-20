// General ledger setup (APEX App 120): account structures, segments, segment values and the chart of accounts.
import { http } from '@/shared/api/http'

export type CodeName = { code: string; name: string }

export type CoaStructure = {
  coaId: number
  coaCode: string
  coaNameEn: string
  coaNameAr: string | null
  description: string | null
  delimiterChar: string
  segmentCount: number
  accountSegmentNum: number
  balancingSegmentNum: number | null
  branchSegmentNum: number | null
  costCenterSegmentNum: number | null
  businessLineSegmentNum: number | null
  intercompanySegmentNum: number | null
  isDynamicCombinationAllowed: boolean
  isCombinationApprovalRequired: boolean
  isActive: boolean
  effectiveStartDate: string
  effectiveEndDate: string | null
  rowVersion: number
}

export type CoaSegment = {
  coaSegmentId: number
  segmentNum: number
  segmentCode: string
  segmentNameEn: string
  segmentNameAr: string | null
  segmentType: string
  minLength: number
  maxLength: number
  valueFormat: string
  padChar: string | null
  isUppercase: boolean
  isRequired: boolean
  isSummaryValueAllowed: boolean
  isEnabled: boolean
  displaySeq: number
  description: string | null
  defaultValueCode: string | null
  defaultMode: 'SUGGEST' | 'FIXED'
  valueCount: number
  rowVersion: number
}

export type SegmentValue = {
  segmentValueId: number
  valueCode: string
  valueNameEn: string
  valueNameAr: string | null
  description: string | null
  parentValueId: number | null
  parentValueCode: string | null
  isSummary: boolean
  isPostingAllowed: boolean
  isEnabled: boolean
  effectiveStartDate: string
  effectiveEndDate: string | null
  sortOrder: number
  referenceLegalEntityCode: string | null
  referenceBusinessLineCode: string | null
  rowVersion: number
}

export type AccountStructurePage = {
  coaId: number | null
  structures: CoaStructure[]
  segments: CoaSegment[]
  ledgers: string[]
  combinations: number
  legalEntities: CodeName[]
  businessLines: CodeName[]
  segmentTypes: string[]
  valueFormats: string[]
}

export type GlAccount = {
  accountId: number
  accountCode: string
  accountNameEn: string
  accountNameAr: string | null
  description: string | null
  parentAccountId: number | null
  accountType: string
  normalBalance: string
  financialStatementClass: string
  cashFlowClass: string
  isSummary: boolean
  isPostingAllowed: boolean
  isControlAccount: boolean
  isReconciliation: boolean
  isBranchRequired: boolean
  isCostCenterRequired: boolean
  isBusinessLineRequired: boolean
  isManualJournalAllowed: boolean
  isSubledgerPostingAllowed: boolean
  isCurrencyRevaluation: boolean
  isRetainedEarnings: boolean
  isActive: boolean
  effectiveStartDate: string
  effectiveEndDate: string | null
  sortOrder: number
  segmentValueCode: string | null
  createdBy: string
  createdAt: string
  updatedBy: string | null
  updatedAt: string | null
  rowVersion: number
}

export type ChartOfAccounts = {
  coaId: number | null
  structures: { coaId: number; coaCode: string; coaNameEn: string; delimiterChar: string; segmentCount: number; accountSegmentNum: number }[]
  segments: { segmentNum: number; segmentCode: string; segmentNameEn: string; segmentType: string; minLength: number; maxLength: number; valueFormat: string }[]
  accounts: GlAccount[]
  typeDefaults: { accountType: string; normalBalance: string; financialStatementClass: string; cashFlowClass: string }[]
  lists: { accountTypes: string[]; normalBalances: string[]; financialStatementClasses: string[]; cashFlowClasses: string[] }
}

type Body = Record<string, unknown>
const S = '/api/gl/account-structures'
const C = '/api/gl/chart-of-accounts'

export const accountStructureApi = {
  get: (coaId?: number | null) => http.get<AccountStructurePage>(coaId ? `${S}?coaId=${coaId}` : S),
  createStructure: (body: Body) => http.post<CoaStructure>(S, body),
  updateStructure: (coaId: number, body: Body) => http.put<CoaStructure>(`${S}/${coaId}`, body),
  createSegment: (coaId: number, body: Body) => http.post<CoaSegment>(`${S}/${coaId}/segments`, body),
  updateSegment: (id: number, body: Body) => http.put<CoaSegment>(`${S}/segments/${id}`, body),
  saveDefault: (id: number, valueCode: string | null, defaultMode: string) =>
    http.put<void>(`${S}/segments/${id}/default`, { valueCode, defaultMode }),
  values: (id: number) => http.get<SegmentValue[]>(`${S}/segments/${id}/values`),
  createValue: (segmentId: number, body: Body) => http.post<SegmentValue>(`${S}/segments/${segmentId}/values`, body),
  updateValue: (id: number, body: Body) => http.put<SegmentValue>(`${S}/segment-values/${id}`, body)
}

export const chartOfAccountsApi = {
  get: (coaId?: number | null) => http.get<ChartOfAccounts>(coaId ? `${C}?coaId=${coaId}` : C),
  create: (body: Body) => http.post<GlAccount>(`${C}/accounts`, body),
  update: (id: number, body: Body) => http.put<GlAccount>(`${C}/accounts/${id}`, body)
}

/** ASSET_TYPE → "Asset type". */
export function codeLabel(code: string | null | undefined): string {
  if (!code) return ''
  const text = code.replace(/_/g, ' ').toLowerCase()
  return text.charAt(0).toUpperCase() + text.slice(1)
}
