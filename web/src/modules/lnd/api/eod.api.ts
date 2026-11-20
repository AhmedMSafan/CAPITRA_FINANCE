import { http } from '@/shared/api/http'

export type EodRun = {
  eodRunId: number; runDate: string; statusCode: 'RUNNING' | 'DONE' | 'FAILED'; loansCount: number; creditAppliedAmount: number
  interestAccruedAmount: number; interestSuspendedAmount: number; lateFeeAmount: number; nonAccrualCount: number; curedCount: number
  writeOffProposedCount: number; glTranSetIds: string | null; errorMessage: string | null; startedAt: string; finishedAt: string | null; createdBy: string
}

export type EodLoan = {
  loanId: number; loanNumber: string; clientName: string; productCode: string; daysLate: number; principalOutstanding: number
  interestSuspenseAmount: number; lateFeeSuspenseAmount: number; isNonAccrual: boolean; isWriteOffProposed: boolean
}

export type EodPage = {
  today: string; lastDoneDate: string | null; nextDate: string | null; canRun: boolean; blocked: string | null
  runs: EodRun[]; nonAccrual: EodLoan[]; writeOffProposed: EodLoan[]
}

const API = '/api/lnd/eod'

export const eodApi = {
  get: () => http.get<EodPage>(API),
  run: (runDate: string | null) => http.post<EodPage>(`${API}/run`, { runDate })
}
