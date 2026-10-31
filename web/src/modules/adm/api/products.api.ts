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

export type JournalSourceOption = { sourceId: number; sourceCode: string; sourceNameEn: string }

export type JournalCategoryOption = { categoryId: number; categoryCode: string; categoryNameEn: string; defaultSourceId: number | null }

/** ADM_PRODUCT_EVENT_RULES */
export type ProductEventRule = {
  productEventRuleId: number
  productId: number
  productCode: string
  productNameEn: string
  journalSourceId: number
  journalSourceCode: string
  journalCategoryId: number
  journalCategoryCode: string
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
  amountBaseCode: string | null
  incomeRecognitionCode: string
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
  journalNo: number
  debitCreditCode: string
  amountCode: string
  accountSourceCode: string
  accountId: number | null
  accountCode: string | null
  accountNameEn: string | null
  descriptionTemplate: string | null
  isActive: boolean
  rowVersion: number
}

/** A fixed action of a product (a button outside the workflow) and the rule linked to it (ADM_PRODUCT_ACTIONS). */
export type ProductAction = {
  actionCode: string
  actionNameEn: string
  actionNameAr: string
  description: string
  moduleCode: string
  amountCodes: string[]
  productActionId: number | null
  productEventRuleId: number | null
  ruleCode: string | null
  ruleNameEn: string | null
  isActive: boolean
  rowVersion: number | null
}

export type ProductActionRuleOption = { productEventRuleId: number; ruleCode: string; ruleNameEn: string; isActive: boolean }

export type ProductActions = { actions: ProductAction[]; rules: ProductActionRuleOption[] }

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
  saveMortgagePortfolioPurchasing: (b: SaveMppParameters) => http.put<Product>(`${PRODUCTS}/mortgage-portfolio-purchasing`, b),
  actions: (id: number) => http.get<ProductActions>(`${PRODUCTS}/${id}/actions`),
  saveAction: (id: number, actionCode: string, b: { productEventRuleId: number | null; isActive: boolean; rowVersion: number | null }) =>
    http.put<ProductActions>(`${PRODUCTS}/${id}/actions/${actionCode}`, b)
}

export const productEventRulesApi = {
  list: () => http.get<ProductEventRule[]>(RULES),
  get: (id: number) => http.get<ProductEventRule>(`${RULES}/${id}`),
  journalSources: () => http.get<JournalSourceOption[]>(`${RULES}/journal-sources`),
  journalCategories: () => http.get<JournalCategoryOption[]>(`${RULES}/journal-categories`),
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

/** What a percentage charge is applied to. */
export const AMOUNT_BASES = [
  { value: 'FACE_VALUE', label: 'Face value' },
  { value: 'PRESENT_VALUE', label: 'Present value' },
  { value: 'PV_LESS_MARGIN', label: 'Present value less margin' },
  { value: 'DISBURSEMENT_GROSS_INVOICE_AMOUNT', label: 'Gross Invoice Amount in Disbursement' },
  { value: 'DISBURSEMENT_NET_AMOUNT', label: 'Disbursement Net Amount' },
  { value: 'ELIGIBLE_INVOICE_AMOUNT', label: 'Eligible Invoice Amount' },
  { value: 'ADVANCE_AMOUNT', label: 'Advance Amount' },
  { value: 'INVOICE_COUNT', label: 'Invoice Count in Disbursement' },
  { value: 'CHEQUE_COUNT', label: 'Cheque Count' },
  { value: 'TENOR_DAYS', label: 'Tenor Days' },
  { value: 'APPROVED_LIMIT', label: 'Approved Facility Limit' }
]

export const INCOME_RECOGNITIONS = [
  { value: 'PRORATE', label: 'Pro-rate' },
  { value: 'IMMEDIATE', label: 'Immediate' }
]

export const CHARGE_TIMINGS = [
  { value: 'DUE_SEPARATELY', label: 'Due Separately' },
  { value: 'DEDUCT_FROM_DISBURSEMENT', label: 'Deduct From Disbursement' },
  { value: 'FINANCE_WITH_PRINCIPAL', label: 'Finance With Principal' },
  { value: 'PAID_UPFRONT', label: 'Paid Upfront' }
]

/** Code to label, for the list columns. */
/** Figures of the event a rule's own GL line posts (purchased portfolio disbursement; cheque amount for the cheque batch submission). */
export const RULE_AMOUNTS = [
  { value: 'FACE_VALUE', label: 'Face value of the cheques' },
  { value: 'DISCOUNT', label: 'Discount (funding recovery)' },
  { value: 'MARGIN_DEFERRED', label: 'Margin, pro-rated' },
  { value: 'MARGIN_IMMEDIATE', label: 'Margin, recognised at once' },
  { value: 'PURCHASE_PRICE', label: 'Purchase price (payable to developer)' },
  { value: 'CHEQUE_AMOUNT', label: 'Cheque amount (cheques received in the wallet)' }
]

/** Figures of one charge a component GL line posts. */
export const CHARGE_AMOUNTS = [
  { value: 'CHARGE_AMOUNT', label: 'Charge amount' },
  { value: 'CHARGE_DEFERRED', label: 'Charge, pro-rated' },
  { value: 'CHARGE_IMMEDIATE', label: 'Charge, recognised at once' }
]

export const ACCOUNT_SOURCES = [
  { value: 'ACCOUNT', label: 'Account below' },
  { value: 'BANK_ACCOUNT', label: 'GL account of the bank account' }
]

export const labelOf = (options: { value: string; label: string }[], code: string | null) =>
  options.find(o => o.value === code)?.label ?? code ?? ''
