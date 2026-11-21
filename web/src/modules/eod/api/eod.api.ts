// End of Day: the night batch of every application (api/eod).
import { http } from '@/shared/api/http'
import { tr } from '@/shared/layout/language'

export type EodParameter = { code: string; labelEn: string; labelAr: string; helpEn: string; helpAr: string; defaultValue: string; value: string | null }

export type EodJob = {
  jobCode: string; moduleCode: string; nameEn: string; nameAr: string; descriptionEn: string; descriptionAr: string; sequenceNo: number
  isEnabled: boolean; isAvailable: boolean; notes: string | null; parameters: EodParameter[]
}

export type EodDayJob = {
  jobCode: string; moduleCode: string; nameEn: string; nameAr: string; sequenceNo: number; statusCode: string; itemsCount: number
  summary: string | null; errorMessage: string | null; attempts: number; startedAt: string | null; finishedAt: string | null; runBy: string | null
}

export type EodDay = { dayId: number; businessDate: string; statusCode: string; startedAt: string; finishedAt: string | null; runBy: string; jobs: EodDayJob[] }

export type EodPage = {
  today: string; lastDoneDate: string | null; nextDate: string | null; isNextDayOpen: boolean; canRun: boolean; blocked: string | null
  days: EodDay[]; jobs: EodJob[]
}

export type EodRunResult = { daysDone: number; openDate: string | null; message: string }

export type EodJobRequest = { isEnabled: boolean; sequenceNo: number; notes: string | null; parameters: Record<string, string | null> }

export const STATUS: Record<string, string> = {
  OPEN: 'Open', DONE: 'Done', PENDING: 'Waiting', RUNNING: 'Running', FAILED: 'Failed', SKIPPED: 'Skipped'
}
export const STATUS_CHIP: Record<string, string> = {
  OPEN: 'fx-chip--warn', DONE: 'fx-chip--ok', PENDING: 'fx-chip--plain', RUNNING: 'fx-chip--warn', FAILED: 'fx-chip--bad', SKIPPED: 'fx-chip--plain'
}
export const MODULES: Record<string, string> = { LND: 'Retail Lending', SCR: 'Screening' }

const API = '/api/eod'

export const eodApi = {
  page: () => http.get<EodPage>(API),
  run: (toDate: string | null) => http.post<{ result: EodRunResult; page: EodPage }>(`${API}/run`, { toDate }),
  jobs: () => http.get<EodJob[]>(`${API}/jobs`),
  saveJob: (jobCode: string, body: EodJobRequest) => http.put<EodJob>(`${API}/jobs/${encodeURIComponent(jobCode)}`, body)
}

export function errorText(err: unknown): string {
  return tr(err instanceof Error ? err.message : String(err))
}
