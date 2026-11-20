// Accounting periods (APEX App 120 page 30) and ledgers: calendars, fiscal years, periods and each ledger's period status.
import { http } from '@/shared/api/http'

export type GlCalendar = {
  calendarId: number
  calendarCode: string
  calendarNameEn: string
  calendarNameAr: string | null
  calendarType: string
  periodFrequency: string
  isAdjustmentPeriodUsed: boolean
  isActive: boolean
  rowVersion: number
}

export type GlFiscalYear = {
  fiscalYearId: number
  fiscalYear: number
  yearName: string | null
  startDate: string
  endDate: string
  yearStatus: string
  periodCount: number
  rowVersion: number
}

export type PeriodLedgerStatus = { ledgerPeriodId: number; ledgerCode: string; periodStatus: string; rowVersion: number }

export type GlAccountingPeriod = {
  periodId: number
  fiscalYearId: number
  periodNo: number
  periodCode: string
  periodNameEn: string
  periodNameAr: string | null
  startDate: string
  endDate: string
  quarterNo: number | null
  isAdjustmentPeriod: boolean
  defaultPeriodStatus: string
  ledgers: PeriodLedgerStatus[]
  rowVersion: number
}

export type AccountingPeriodsPage = {
  calendarId: number | null
  today: string
  calendars: GlCalendar[]
  ledgers: { ledgerId: number; ledgerCode: string; ledgerNameEn: string }[]
  years: GlFiscalYear[]
  periods: GlAccountingPeriod[]
  lists: { calendarTypes: string[]; periodFrequencies: string[]; yearStatuses: string[]; defaultPeriodStatuses: string[] }
}

export type NextLedgerPeriod = { ledgerPeriodId: number; periodCode: string; periodStatus: string; rowVersion: number }

export type LedgerPeriodInfo = {
  ledgerPeriodId: number
  ledgerCode: string
  periodCode: string
  periodStatus: string
  postingMode: string
  isManualJournalAllowed: boolean
  isSubledgerAllowed: boolean
  isReversalAllowed: boolean
  isAdjustmentAllowed: boolean
  openedBy: string | null
  openedAt: string | null
  closedBy: string | null
  closedAt: string | null
  permanentlyClosedBy: string | null
  permanentlyClosedAt: string | null
  closeNotes: string | null
  canClose: boolean | null
  canCloseMessage: string | null
  next: NextLedgerPeriod | null
  history: { oldPeriodStatus: string | null; newPeriodStatus: string; actionCode: string; actionReason: string | null; actionBy: string; actionAt: string }[]
  rowVersion: number
}

export type LedgerPeriodAction = 'OPEN' | 'SET_FUTURE' | 'CLOSE' | 'REOPEN' | 'PERMANENT'

export type GlLedger = {
  ledgerId: number
  legalEntityCode: string
  ledgerCode: string
  ledgerNameEn: string
  ledgerNameAr: string | null
  ledgerType: string
  ledgerCurrencyCode: string
  calendarId: number
  calendarCode: string
  coaId: number | null
  coaCode: string | null
  isPrimary: boolean
  ledgerStatus: string
  retainedEarningsAcctCode: string | null
  suspenseAcctCode: string | null
  roundingDiffAcctCode: string | null
  fxGainAcctCode: string | null
  fxLossAcctCode: string | null
  openingBalanceAcctCode: string | null
  startDate: string
  endDate: string | null
  notes: string | null
  periodCount: number
  rowVersion: number
}

export type LedgerOption = { id: number | null; code: string; name: string }

export type LedgersPage = {
  ledgers: GlLedger[]
  legalEntities: LedgerOption[]
  currencies: LedgerOption[]
  calendars: LedgerOption[]
  structures: LedgerOption[]
  accounts: { coaId: number; accountCode: string; accountNameEn: string; accountType: string }[]
  ledgerTypes: string[]
  ledgerStatuses: string[]
}

type Body = Record<string, unknown>
const P = '/api/gl/accounting-periods'
const L = '/api/gl/ledgers'

export const periodsApi = {
  get: (calendarId?: number | null) => http.get<AccountingPeriodsPage>(calendarId ? `${P}?calendarId=${calendarId}` : P),
  createCalendar: (body: Body) => http.post<GlCalendar>(`${P}/calendars`, body),
  updateCalendar: (id: number, body: Body) => http.put<GlCalendar>(`${P}/calendars/${id}`, body),
  generateYear: (calendarId: number, body: Body) =>
    http.post<{ fiscalYearId: number; message: string }>(`${P}/calendars/${calendarId}/fiscal-years`, body),
  updateYear: (id: number, body: Body) => http.put<GlFiscalYear>(`${P}/fiscal-years/${id}`, body),
  createPeriod: (yearId: number, body: Body) => http.post<GlAccountingPeriod>(`${P}/fiscal-years/${yearId}/periods`, body),
  updatePeriod: (id: number, body: Body) => http.put<GlAccountingPeriod>(`${P}/periods/${id}`, body),
  syncLedgerPeriods: (yearId: number) => http.post<{ created: number; message: string }>(`${P}/fiscal-years/${yearId}/ledger-periods`, {}),
  ledgerPeriod: (id: number) => http.get<LedgerPeriodInfo>(`${P}/ledger-periods/${id}`),
  action: (id: number, action: LedgerPeriodAction, reason: string | null, rowVersion: number) =>
    http.post<{ periodStatus: string; next: NextLedgerPeriod | null }>(`${P}/ledger-periods/${id}/actions`, { action, reason, rowVersion })
}

export const ledgersApi = {
  get: () => http.get<LedgersPage>(L),
  create: (body: Body) => http.post<GlLedger>(L, body),
  update: (id: number, body: Body) => http.put<GlLedger>(`${L}/${id}`, body)
}

/** Pill colour of a period status: open green, future blue, closed amber, permanently closed red. */
export function periodStatusClass(status: string): string {
  switch (status) {
    case 'OPEN': return 'cp-pill--ok'
    case 'FUTURE_ENTRY': return 'cp-pill--info'
    case 'CLOSED': return 'cp-pill--warn'
    case 'PERMANENTLY_CLOSED': return 'cp-pill--bad'
    default: return ''
  }
}
