import { http } from '@/shared/api/http'

// Tools > Loan Calculator (the FINOVA_FINANCEING calculator): the same request and result as FINOVA, dates as yyyy-MM-dd.
export type LoanScenarioListItem = { scenarioId: number; scenarioName: string }

export type LoanScenarioWorkspaceRate = { effectiveDate: string; interestRatePct: number }

export type LoanScenarioWorkspaceRequest = {
  scenarioId: number | null
  scenarioName: string | null
  rateIndexId: number | null
  principalAmount: number
  contractDate: string
  tenorValue: number
  tenorUnit: string
  interestRatePct: number
  dayCountMethod: string
  scheduleMode: string
  principalMethod: string
  interestPaymentMethod: string
  principalPaymentStartDate: string
  interestPaymentStartDate: string
  principalFrequencyValue: number | null
  principalFrequencyUnit: string | null
  principalDateRule: string | null
  interestFrequencyValue: number | null
  interestFrequencyUnit: string | null
  interestDateRule: string | null
  combinedFrequencyValue: number | null
  combinedFrequencyUnit: string | null
  combinedDateRule: string | null
  rates: LoanScenarioWorkspaceRate[]
}

export type LoanScenarioRate = { effectiveDate: string; interestRatePct: number; sequenceNo: number }

export type LoanScheduleLine = {
  installmentNo: number
  accrualStartDate: string
  accrualEndDate: string
  paymentDate: string
  accrualDays: number
  openingPrincipal: number
  principalAmount: number
  interestAmount: number
  totalPaymentAmount: number
  closingPrincipal: number
  interestRatePct: number
  dayCountFraction: number
}

export type LoanScenarioResult = {
  scenarioId: number
  scenarioName: string
  rateIndexId: number | null
  principalAmount: number
  contractDate: string
  maturityDate: string
  tenorValue: number
  tenorUnit: string
  scheduleMode: string
  principalMethod: string
  interestPaymentMethod: string
  dayCountMethod: string
  principalPaymentStartDate: string
  interestPaymentStartDate: string
  principalFrequencyValue: number | null
  principalFrequencyUnit: string | null
  principalDateRule: string | null
  interestFrequencyValue: number | null
  interestFrequencyUnit: string | null
  interestDateRule: string | null
  combinedFrequencyValue: number | null
  combinedFrequencyUnit: string | null
  combinedDateRule: string | null
  rates: LoanScenarioRate[]
  schedule: LoanScheduleLine[]
}

/** A rate index of Administration > Rate Maintenance with its rate valid on the date asked (null when none is). */
export type RateOption = { rateIndexId: number; rateIndexCode: string; rateIndexName: string; ratePct: number | null }

const API = '/api/tls/loan-scenarios'

export const loanScenariosApi = {
  list: () => http.get<LoanScenarioListItem[]>(API),
  get: (id: number) => http.get<LoanScenarioResult>(`${API}/${id}`),
  rates: (date: string) => http.get<RateOption[]>(`${API}/rates?date=${encodeURIComponent(date)}`),
  preview: (body: LoanScenarioWorkspaceRequest) => http.post<LoanScenarioResult>(`${API}/preview`, body),
  save: (body: LoanScenarioWorkspaceRequest) => http.post<LoanScenarioResult>(`${API}/save`, body)
}
