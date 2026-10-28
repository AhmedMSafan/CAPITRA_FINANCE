// Product and charge setup of Admin Setup (api/adm/products, charges, product-event-rules), from FINOVA_FINANCEING.
import { http } from '@/shared/api/http'

/** ADM_CHARGES */
export type Charge = {
  chargeId: number
  chargeCode: string
  chargeNameEn: string
  chargeNameAr: string | null
  chargeTypeCode: string
  financialNatureCode: string
  isRecurring: boolean
  calculationTypeCode: string
  defaultRate: number | null
  defaultAmount: number | null
  currencyCode: string | null
  description: string | null
  effectiveFromDate: string | null
  effectiveToDate: string | null
  isActive: boolean
  rowVersion: number
}

/** ADM_PRODUCTS */
export type Product = {
  productId: number
  productCode: string
  productNameEn: string
  productNameAr: string | null
  businessLineCode: string
  productTypeCode: string
  factoringModeCode: string | null
  isRecourse: boolean | null
  defaultCurrencyCode: string | null
  effectiveFromDate: string
  effectiveToDate: string | null
  isActive: boolean
  mppPvRate: number
  mppLateFeeRate: number
  mppReservedAmountPct: number
  mppInvestorPaymentPct: number
  rowVersion: number
}

export type SaveMppParameters = {
  mppPvRate: number | null
  mppLateFeeRate: number | null
  mppReservedAmountPct: number | null
  mppInvestorPaymentPct: number | null
  rowVersion: number
}

export type WorkflowOption = { workflowCode: string; workflowName: string; steps: { stepCode: string; stepName: string | null }[] }

export type EventTypeOption = { eventTypeId: number; eventTypeCode: string; eventTypeNameEn: string }

/** ADM_PRODUCT_EVENT_RULES */
export type ProductEventRule = {
  productEventRuleId: number
  productId: number
  productCode: string
  productNameEn: string
  eventTypeId: number
  eventTypeCode: string
  eventTypeNameEn: string
  workflowCode: string | null
  stepCode: string | null
  ruleCode: string
  ruleNameEn: string
  ruleNameAr: string | null
  priorityNo: number
  effectiveFromDate: string
  effectiveToDate: string | null
  isActive: boolean
  chargeCount: number
  glLineCount: number
  rowVersion: number
}

/** ADM_PRODUCT_EVENT_RULE_CHARGES */
export type RuleCharge = {
  ruleChargeId: number
  productEventRuleId: number
  chargeId: number
  chargeCode: string
  chargeNameEn: string
  sequenceNo: number
  calculationMethodCode: string
  fixedAmount: number | null
  chargeRate: number | null
  perUnitAmount: number | null
  spreadRate: number
  dayCountBasis: number | null
  minimumAmount: number | null
  maximumAmount: number | null
  roundingScale: number
  chargeTimingCode: string
  dueDays: number
  taxRate: number
  effectiveFromDate: string
  effectiveToDate: string | null
  isActive: boolean
  rowVersion: number
}

/** ADM_PRODUCT_EVENT_RULE_GL_LINES; ruleChargeId null = a line of the rule itself */
export type RuleGlLine = {
  ruleGlLineId: number
  productEventRuleId: number
  ruleChargeId: number | null
  chargeCode: string | null
  lineNo: number
  debitCreditCode: string
  accountId: number
  accountCode: string
  accountNameEn: string
  descriptionTemplate: string | null
  isActive: boolean
  rowVersion: number
}

export type GlAccountOption = { accountId: number; accountCode: string; accountNameEn: string }

export type Body = Record<string, unknown>

const CHARGES = '/api/adm/charges'
const PRODUCTS = '/api/adm/products'
const RULES = '/api/adm/product-event-rules'

export const chargesApi = {
  list: () => http.get<Charge[]>(CHARGES),
  create: (b: Body) => http.post<Charge>(CHARGES, b),
  update: (id: number, b: Body) => http.put<Charge>(`${CHARGES}/${id}`, b),
  remove: (id: number, rowVersion: number) => http.delete<void>(`${CHARGES}/${id}?rowVersion=${rowVersion}`)
}

export const productsApi = {
  list: () => http.get<Product[]>(PRODUCTS),
  create: (b: Body) => http.post<Product>(PRODUCTS, b),
  update: (id: number, b: Body) => http.put<Product>(`${PRODUCTS}/${id}`, b),
  remove: (id: number, rowVersion: number) => http.delete<void>(`${PRODUCTS}/${id}?rowVersion=${rowVersion}`),
  /** null while the product is not in the catalog. */
  getMortgagePortfolioPurchasing: () => http.get<Product | null>(`${PRODUCTS}/mortgage-portfolio-purchasing`),
  saveMortgagePortfolioPurchasing: (b: SaveMppParameters) => http.put<Product>(`${PRODUCTS}/mortgage-portfolio-purchasing`, b)
}

export const productEventRulesApi = {
  list: () => http.get<ProductEventRule[]>(RULES),
  get: (id: number) => http.get<ProductEventRule>(`${RULES}/${id}`),
  eventTypes: () => http.get<EventTypeOption[]>(`${RULES}/event-types`),
  workflowSteps: () => http.get<WorkflowOption[]>(`${RULES}/workflow-steps`),
  create: (b: Body) => http.post<ProductEventRule>(RULES, b),
  update: (id: number, b: Body) => http.put<ProductEventRule>(`${RULES}/${id}`, b),
  remove: (id: number, rowVersion: number) => http.delete<void>(`${RULES}/${id}?rowVersion=${rowVersion}`),
  charges: (ruleId: number) => http.get<RuleCharge[]>(`${RULES}/${ruleId}/charges`),
  createCharge: (ruleId: number, b: Body) => http.post<RuleCharge>(`${RULES}/${ruleId}/charges`, b),
  updateCharge: (ruleId: number, id: number, b: Body) => http.put<RuleCharge>(`${RULES}/${ruleId}/charges/${id}`, b),
  removeCharge: (ruleId: number, id: number, rowVersion: number) =>
    http.delete<void>(`${RULES}/${ruleId}/charges/${id}?rowVersion=${rowVersion}`),
  glAccounts: () => http.get<GlAccountOption[]>(`${RULES}/gl-accounts`),
  glLines: (ruleId: number) => http.get<RuleGlLine[]>(`${RULES}/${ruleId}/gl-lines`),
  createGlLine: (ruleId: number, b: Body) => http.post<RuleGlLine>(`${RULES}/${ruleId}/gl-lines`, b),
  updateGlLine: (ruleId: number, id: number, b: Body) => http.put<RuleGlLine>(`${RULES}/${ruleId}/gl-lines/${id}`, b),
  removeGlLine: (ruleId: number, id: number, rowVersion: number) =>
    http.delete<void>(`${RULES}/${ruleId}/gl-lines/${id}?rowVersion=${rowVersion}`)
}

export const CALCULATION_TYPES = [
  { value: 'FIXED', label: 'Fixed' },
  { value: 'PERCENTAGE', label: 'Percentage' },
  { value: 'FORMULA', label: 'Formula' }
]

export const FINANCIAL_NATURES = [
  { value: 'REVENUE', label: 'Revenue' },
  { value: 'EXPENSE', label: 'Expense' },
  { value: 'OTHER', label: 'Other' }
]

export const CALCULATION_METHODS = [
  { value: 'FIXED', label: 'Fixed' },
  { value: 'PERCENTAGE', label: 'Percentage' },
  { value: 'PER_UNIT', label: 'Per Unit' },
  { value: 'TIERED', label: 'Tiered' },
  { value: 'INDEX_PLUS_SPREAD', label: 'Index + Spread' },
  { value: 'DAILY_INTEREST', label: 'Daily Interest' }
]

export const CHARGE_TIMINGS = [
  { value: 'DUE_SEPARATELY', label: 'Due Separately' },
  { value: 'DEDUCT_FROM_DISBURSEMENT', label: 'Deduct From Disbursement' },
  { value: 'FINANCE_WITH_PRINCIPAL', label: 'Finance With Principal' },
  { value: 'PAID_UPFRONT', label: 'Paid Upfront' }
]

/** Code to label, for the list columns. */
export const labelOf = (options: { value: string; label: string }[], code: string | null) =>
  options.find(o => o.value === code)?.label ?? code ?? ''
