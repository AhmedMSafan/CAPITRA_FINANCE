// Portfolio cases (api/mtg/portfolios, portfolio-accounts, account-payments): APEX App 105 pages 201-207.
import { http } from '@/shared/api/http'
import type { CrudApi, FieldOption, FormValues } from '@/shared/crud/types'

export type Portfolio = {
  portfolioId: number
  portfolioNo: string
  developerId: number
  developerName: string
  projectId: number
  projectName: string
  requestDate: string
  portfolioStatusCode: string
  portfolioStatusName: string
  currencyCode: string
  termMonths: number | null
  comments: string | null
  coveragePct: number | null
  reservedAmountPct: number | null
  pvRate: number | null
  lateFeeRate: number | null
  marginPct: number | null
  adminExpensePct: number | null
  legalFeePerUnitAmount: number | null
  valuationFeePerUnitAmount: number | null
  accountCount: number
  rowVersion: number
}

export type PortfolioCase = {
  portfolioId: number
  portfolioNo: string
  portfolioStatusCode: string
  portfolioStatusName: string
  portfolioStatusNameAr: string | null
  statusDisplayOrder: number
  developerId: number
  developerName: string
  projectName: string
  requestDate: string
  ageDays: number
  currencyCode: string
  faceValueAmount: number
  outstandingAmount: number
  accountCount: number
  arrearsAmount: number
}

export type PortfolioAccount = {
  portfolioAccountId: number
  portfolioId: number
  investorId: number
  investorCode: string
  investorName: string
  projectUnitId: number
  unitCode: string
  bookingDate: string | null
  contractDate: string | null
  contractAmount: number
  paidAmount: number
  outstandingAmount: number
  installmentAmount: number | null
  installmentFrequency: string | null
  lastPaymentDate: string | null
  regularPaymentMonths: number
  arrearsAmount: number
  accountStatus: string | null
  rowVersion: number
}

export type AccountPayment = {
  accountPaymentId: number
  portfolioAccountId: number
  dueDate: string
  dueAmount: number
  paymentDate: string | null
  paidAmount: number
  paymentStatusCode: string
  paymentStatusName: string
  daysLate: number
  chequeNo: string | null
  bankName: string | null
  rowVersion: number
}

export type PortfolioWorkspace = {
  portfolio: Portfolio
  figures: {
    faceValueAmount: number
    outstandingAmount: number
    proposedPurchaseAmount: number
    expectedReturnAmount: number
    paidAmount: number
    arrearsAmount: number
    accountCount: number
    investorCount: number
    unitCount: number
    ageDays: number
  }
  project: {
    projectCode: string
    projectNameEn: string | null
    projectNameAr: string
    developerCode: string
    developerName: string
    locationText: string | null
    landArea: number | null
    titleDeedNo: string | null
    buildingLicenseNo: string | null
    buildingLicenseDate: string | null
    projectCompletionPct: number | null
    projectStatus: string | null
    unitsInProject: number
  }
  investors: {
    investorId: number
    investorCode: string
    fullNameEn: string | null
    fullNameAr: string
    nationalIdNo: string | null
    contractCount: number
    contractAmount: number
    paidAmount: number
    outstandingAmount: number
  }[]
  accounts: PortfolioAccount[]
  units: { portfolioAccountId: number; unitCode: string; unitTypeName: string | null; investorName: string; contractAmount: number }[]
  statusHistory: {
    fromStatusCode: string | null
    fromStatusName: string | null
    toStatusCode: string
    toStatusName: string | null
    changedAt: string
    changedBy: string
    comments: string | null
  }[]
}

export type FinancialTerms = {
  coveragePct: number | null
  marginPct: number | null
  pvRate: number | null
  reservedAmountPct: number | null
  lateFeeRate: number | null
  adminExpensePct: number | null
  legalFeePerUnitAmount: number | null
  valuationFeePerUnitAmount: number | null
}

export const portfoliosApi = {
  cases: (developerId?: number) =>
    http.get<PortfolioCase[]>(`/api/mtg/portfolios${developerId ? `?developerId=${developerId}` : ''}`),
  get: (id: number) => http.get<Portfolio>(`/api/mtg/portfolios/${id}`),
  workspace: (id: number) => http.get<PortfolioWorkspace>(`/api/mtg/portfolios/${id}/workspace`),
  create: (values: FormValues) => http.post<Portfolio>('/api/mtg/portfolios', values),
  update: (row: Portfolio, values: FormValues) =>
    http.put<Portfolio>(`/api/mtg/portfolios/${row.portfolioId}`, { ...values, rowVersion: row.rowVersion }),
  saveFinancialTerms: (row: Portfolio, terms: FinancialTerms) =>
    http.put<Portfolio>(`/api/mtg/portfolios/${row.portfolioId}/financial-terms`, { ...terms, rowVersion: row.rowVersion }),
  remove: (row: Portfolio) => http.delete<void>(`/api/mtg/portfolios/${row.portfolioId}?rowVersion=${row.rowVersion}`)
}

/** Accounts of one portfolio (page 205). The portfolio is fixed by the page, never chosen in the form. */
export function portfolioAccountsApi(portfolioId: number): CrudApi<PortfolioAccount> {
  const path = '/api/mtg/portfolio-accounts'
  return {
    list: () => http.get<PortfolioAccount[]>(`${path}?portfolioId=${portfolioId}`),
    create: values => http.post<PortfolioAccount>(path, { ...values, portfolioId }),
    update: (row, values) =>
      http.put<PortfolioAccount>(`${path}/${row.portfolioAccountId}`, { ...values, portfolioId, rowVersion: row.rowVersion }),
    remove: row => http.delete<void>(`${path}/${row.portfolioAccountId}?rowVersion=${row.rowVersion}`)
  }
}

/** Payments of one account (pages 206/207). */
export function accountPaymentsApi(portfolioAccountId: number | null): CrudApi<AccountPayment> {
  const path = '/api/mtg/account-payments'
  return {
    list: () =>
      portfolioAccountId ? http.get<AccountPayment[]>(`${path}?portfolioAccountId=${portfolioAccountId}`) : Promise.resolve([]),
    create: values => http.post<AccountPayment>(path, { ...values, portfolioAccountId }),
    update: (row, values) =>
      http.put<AccountPayment>(`${path}/${row.accountPaymentId}`, {
        ...values,
        portfolioAccountId: row.portfolioAccountId,
        rowVersion: row.rowVersion
      }),
    remove: row => http.delete<void>(`${path}/${row.accountPaymentId}?rowVersion=${row.rowVersion}`)
  }
}

/** LOV FNV_PORTFOLIO_ACCOUNT_BY_PORTFOLIO: "investor code - name - unit". */
export function accountOptions(accounts: PortfolioAccount[]): FieldOption[] {
  return accounts.map(a => ({ value: a.portfolioAccountId, label: `${a.investorCode} - ${a.investorName} - ${a.unitCode}` }))
}
