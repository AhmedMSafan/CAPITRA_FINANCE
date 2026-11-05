// Treasury Disburse (api/trs/disbursements/{id}/funding): choose, reserve and execute the source of funds of a disbursement.
import { http } from '@/shared/api/http'

export type TrsDisburseHeader = {
  disbursementId: number
  disbursementNumber: string
  clientId: number
  clientName: string
  agreementNumber: string | null
  businessLine: string
  isReverse: boolean
  currencyCode: string
  netAmount: number
  invoiceCount: number
  maxTenorDays: number | null
  fundingStatus: 'NONE' | 'RESERVED' | 'EXECUTED'
  executedBy: string | null
  executedAt: string | null
}

export type TrsDisburseParty = { clientId: number | null; name: string; role: 'DEBTOR' | 'SELLER'; weight: number; groupName: string | null }

export type TrsDisburseSource = {
  sourceKey: string
  sourceTypeCode: 'OWN_FUNDS' | 'BANK'
  bankId: number | null
  bankName: string | null
  facilityId: number | null
  facilityRef: string | null
  trancheName: string | null
  sublimitId: number | null
  sublimitName: string | null
  available: number | null
  amount: number
  isOverrideRequired: boolean
  ruleIds: number[]
}

export type TrsDisburseRule = {
  ruleId: number
  bankId: number
  bankName: string
  facilityId: number | null
  ruleTypeCode: string
  label: string
  breachActionCode: 'BLOCK' | 'OVERRIDE' | 'WARN'
  limitAbove: number | null
  warnAbove: number | null
  detail: string
}

export type TrsDisburse = {
  header: TrsDisburseHeader
  parties: TrsDisburseParty[]
  sources: TrsDisburseSource[]
  rules: TrsDisburseRule[]
  canEdit: boolean
  overrideReason: string | null
  overrideRequestedBy: string | null
  overrideApprovedBy: string | null
  canApproveOverride: boolean
  approveBlockedReason: string | null
}

export type TrsDisburseForm = { lines: { sublimitId: number | null; amount: number }[]; overrideReason: string | null }

const base = (id: number) => `/api/trs/disbursements/${id}/funding`

export const trsDisburseApi = {
  get: (id: number) => http.get<TrsDisburse>(base(id)),
  save: (id: number, form: TrsDisburseForm) => http.put<TrsDisburse>(base(id), form),
  approveOverride: (id: number) => http.post<TrsDisburse>(`${base(id)}/approve-override`, {}),
  execute: (id: number) => http.post<TrsDisburse>(`${base(id)}/execute`, {})
}
