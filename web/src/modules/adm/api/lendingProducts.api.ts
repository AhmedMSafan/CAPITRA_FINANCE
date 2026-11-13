import { http } from '@/shared/api/http'

export type Option = { value: string; label: string }

/** One version of a lending product's terms (LND_PRODUCT_VERSIONS). */
export type LendingVersion = {
  productVersionId: number
  versionNo: number
  effectiveFromDate: string
  statusCode: 'DRAFT' | 'PENDING' | 'APPROVED' | 'REJECTED'
  isInForce: boolean
  minAmount: number
  maxAmount: number
  minTermMonths: number
  maxTermMonths: number
  repaymentFrequencyCode: string
  interestMethodCode: string
  annualRatePct: number
  dayCountCode: string
  lateFeeGraceDays: number
  lateFeeTypeCode: string
  lateFeeRatePct: number | null
  lateFeeFlatAmount: number | null
  lateFeeBaseCode: string
  paymentOrder: string[]
  isInsuranceRequired: boolean
  changeReason: string | null
  createdBy: string
  requestedBy: string | null
  requestedAt: string | null
  decidedBy: string | null
  decidedAt: string | null
  decisionNotes: string | null
  canApprove: boolean
  approveBlockedReason: string | null
  rowVersion: number
}

export type LendingProduct = {
  productId: number
  productCode: string
  productNameEn: string
  productNameAr: string | null
  businessLineCode: string
  licenceCode: 'MF' | 'CF'
  productTypeCode: string
  isActive: boolean
  versionInForce: number | null
  versions: LendingVersion[]
}

export type LendingProductsPage = {
  today: string
  canEdit: boolean
  canApprove: boolean
  products: LendingProduct[]
  options: {
    frequencies: Option[]; interestMethods: Option[]; dayCounts: Option[]; lateFeeTypes: Option[]; lateFeeBases: Option[]; paymentParts: Option[]
  }
}

export type LendingVersionSave = {
  effectiveFromDate: string | null
  minAmount: number | null
  maxAmount: number | null
  minTermMonths: number | null
  maxTermMonths: number | null
  repaymentFrequencyCode: string
  interestMethodCode: string
  annualRatePct: number | null
  dayCountCode: string
  lateFeeGraceDays: number | null
  lateFeeTypeCode: string
  lateFeeRatePct: number | null
  lateFeeFlatAmount: number | null
  lateFeeBaseCode: string
  paymentOrder: string[]
  isInsuranceRequired: boolean
  changeReason: string | null
  rowVersion: number
}

const API = '/api/adm/lending-products'
const v = (id: number, action = '') => `${API}/versions/${id}${action ? '/' + action : ''}`

export const lendingProductsApi = {
  get: () => http.get<LendingProductsPage>(API),
  createDraft: (productId: number) => http.post<LendingProductsPage>(`${API}/${productId}/versions`, {}),
  save: (id: number, body: LendingVersionSave) => http.put<LendingProductsPage>(v(id), body),
  submit: (id: number) => http.post<LendingProductsPage>(v(id, 'submit'), {}),
  takeBack: (id: number) => http.post<LendingProductsPage>(v(id, 'take-back'), {}),
  discard: (id: number) => http.delete<LendingProductsPage>(v(id)),
  approve: (id: number, notes: string | null) => http.post<LendingProductsPage>(v(id, 'approve'), { notes }),
  reject: (id: number, notes: string) => http.post<LendingProductsPage>(v(id, 'reject'), { notes })
}
