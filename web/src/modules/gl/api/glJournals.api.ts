// Journals (APEX App 120 pages 50 and 51), the account ledger (page 52) and account balances (page 20).
import { http } from '@/shared/api/http'
import { formatAmount } from '@/shared/format/money'

export type Option = { code: string; name: string }

export type JournalLedger = { ledgerId: number; ledgerCode: string; ledgerNameEn: string; currencyCode: string; coaId: number | null; isPrimary: boolean }

export type JournalLedgerPeriod = {
  ledgerPeriodId: number
  ledgerId: number
  periodCode: string
  startDate: string
  endDate: string
  isAdjustmentPeriod: boolean
  periodStatus: string
  isManualJournalAllowed: boolean
}

export type JournalSegmentValue = { code: string; name: string; requiredSegments: number[] | null }

export type JournalSegment = {
  segmentNum: number
  segmentCode: string
  segmentNameEn: string
  isAccount: boolean
  isRequired: boolean
  defaultValueCode: string | null
  defaultMode: string
  values: JournalSegmentValue[]
}

export type JournalStructure = { coaId: number; coaCode: string; delimiter: string; segments: JournalSegment[] }

export type JournalEntrySetup = {
  today: string
  me: string | null
  defaultLedgerId: number | null
  defaultCategory: string
  ledgers: JournalLedger[]
  periods: JournalLedgerPeriod[]
  categories: Option[]
  structures: JournalStructure[]
}

export type JournalLink = { journalHeaderId: number; journalNo: string; journalStatus: string }

export type JournalAllowed = {
  edit: boolean
  submit: boolean
  approve: boolean
  reject: boolean
  post: boolean
  cancel: boolean
  reverse: boolean
  approveBlockedReason: string | null
}

export type JournalLine = {
  lineNo: number
  concatenatedAccount: string
  accountCode: string
  accountNameEn: string
  segments: Record<string, string>
  dr: number
  cr: number
  description: string | null
}

export type JournalImpact = {
  lineNo: number
  concatenatedAccount: string
  accountNameEn: string
  periodCode: string
  dr: number
  cr: number
  balanceBefore: number
  balanceAfter: number
}

export type JournalHistory = { actionCode: string; oldStatus: string | null; newStatus: string; reason: string | null; by: string; at: string }

export type Journal = {
  journalHeaderId: number
  journalNo: string
  batchNo: string
  tranSetId: string | null
  ledgerId: number
  ledgerCode: string
  coaId: number
  periodCode: string
  sourceCode: string
  categoryCode: string
  journalName: string
  description: string | null
  referenceNo: string | null
  journalDate: string
  accountingDate: string
  currencyCode: string
  journalStatus: string
  totalDr: number
  totalCr: number
  isReversal: boolean
  reversalOf: JournalLink | null
  reversedBy: JournalLink | null
  sourceDocumentType: string | null
  sourceDocumentNo: string | null
  createdBy: string
  createdAt: string
  submittedBy: string | null
  submittedAt: string | null
  approvedBy: string | null
  approvedAt: string | null
  postedBy: string | null
  postedAt: string | null
  cancelledBy: string | null
  cancelledAt: string | null
  cancelReason: string | null
  rejectReason: string | null
  lastPostError: string | null
  allowed: JournalAllowed
  lines: JournalLine[]
  impacts: JournalImpact[]
  history: JournalHistory[]
  rowVersion: number
}

export type SaveJournalLine = { segments: Record<number, string | null>; dr: number | null; cr: number | null; description: string | null }

export type SaveJournal = {
  ledgerId: number | null
  categoryCode: string | null
  accountingDate: string | null
  journalDate: string | null
  journalName: string | null
  description: string | null
  referenceNo: string | null
  lines: SaveJournalLine[]
  submit: boolean
  rowVersion: number | null
}

export type JournalAction = 'submit' | 'approve' | 'reject' | 'post' | 'cancel' | 'reverse'
export type BulkAction = Exclude<JournalAction, 'reverse'>

export type JournalRow = {
  journalHeaderId: number
  journalNo: string
  journalName: string
  description: string | null
  batchNo: string
  tranSetId: string | null
  ledgerCode: string
  sourceCode: string
  categoryCode: string
  accountingDate: string
  periodCode: string
  currencyCode: string
  totalDr: number
  lineCount: number
  stage: string
  isReversal: boolean
  createdBy: string
  submittedBy: string | null
  postError: string | null
  rowVersion: number
}

export type FilterSegment = { segmentNum: number; segmentCode: string; segmentNameEn: string; isAccount: boolean; values: Option[] }

export type JournalSearchResult = {
  rows: JournalRow[]
  more: boolean
  counts: Record<string, number>
  lists: { ledgers: JournalLedger[]; sources: Option[]; categories: Option[]; periods: string[]; segments: FilterSegment[] }
}

export type JournalFilters = {
  stage: string
  q: string
  tran: string
  ledgerId: string
  source: string
  category: string
  period: string
  from: string
  to: string
  minAmount: string
  maxAmount: string
  createdBy: string
  segments: Record<number, string>
}

export type BulkResult = { succeeded: number; failed: number; results: { journalHeaderId: number; journalNo: string; ok: boolean; message: string | null }[] }

export type AccountLedgerRow = {
  accountingDate: string
  journalHeaderId: number
  journalNo: string
  journalName: string
  tranSetId: string | null
  concatenatedAccount: string
  description: string | null
  dr: number
  cr: number
  balanceBefore: number
  balanceAfter: number
}

export type AccountLedgerResult = {
  ledgerId: number | null
  fromPeriod: string | null
  toPeriod: string | null
  searched: boolean
  broughtForward: number
  totalDr: number
  totalCr: number
  closing: number
  rows: AccountLedgerRow[]
  more: boolean
  ledgers: JournalLedger[]
  periods: string[]
  segments: FilterSegment[]
}

export type AccountBalanceRow = {
  accountCombinationId: number
  concatenatedAccount: string
  accountCode: string
  accountNameEn: string
  accountType: string
  financialStatementClass: string
  currencyCode: string
  opening: number
  periodDr: number
  periodCr: number
  closing: number
  lastJournalNo: string | null
  lastPostedAt: string | null
}

export type AccountBalancesResult = {
  ledgerId: number | null
  period: string | null
  rows: AccountBalanceRow[]
  totalOpening: number
  totalDr: number
  totalCr: number
  totalClosing: number
  ledgers: JournalLedger[]
  periods: string[]
}

const J = '/api/gl/journals'

/** Segment filters travel as "3:1101;1:01". */
export function segmentQuery(segments: Record<number, string>): string {
  return Object.entries(segments)
    .filter(([, v]) => v && v.trim())
    .map(([k, v]) => `${k}:${v.trim().toUpperCase()}`)
    .join(';')
}

function query(params: Record<string, string | number | null | undefined>): string {
  const q = new URLSearchParams()
  for (const [k, v] of Object.entries(params)) if (v !== null && v !== undefined && String(v).trim() !== '') q.set(k, String(v).trim())
  const s = q.toString()
  return s ? `?${s}` : ''
}

export const journalsApi = {
  setup: () => http.get<JournalEntrySetup>(`${J}/setup`),
  get: (id: number) => http.get<Journal>(`${J}/${id}`),
  create: (body: SaveJournal) => http.post<{ journal: Journal; submitError: string | null }>(J, body),
  update: (id: number, body: SaveJournal) => http.put<{ journal: Journal; submitError: string | null }>(`${J}/${id}`, body),
  act: (id: number, action: JournalAction, body: { reason?: string | null; reversalDate?: string | null; rowVersion?: number }) =>
    http.post<{ journal: Journal; message: string; created: JournalLink | null }>(`${J}/${id}/${action}`, body),
  bulk: (action: BulkAction, journalIds: number[], reason: string | null) => http.post<BulkResult>(`${J}/bulk/${action}`, { journalIds, reason }),
  search: (f: JournalFilters) =>
    http.get<JournalSearchResult>(J + query({
      stage: f.stage, q: f.q, tran: f.tran, ledgerId: f.ledgerId, source: f.source, category: f.category, period: f.period,
      from: f.from, to: f.to, minAmount: f.minAmount, maxAmount: f.maxAmount, createdBy: f.createdBy, segments: segmentQuery(f.segments)
    })),
  accountLedger: (p: { ledgerId?: number | null; from?: string; to?: string; q?: string; segments: Record<number, string> }) =>
    http.get<AccountLedgerResult>('/api/gl/account-ledger' + query({ ledgerId: p.ledgerId, from: p.from, to: p.to, q: p.q, segments: segmentQuery(p.segments) })),
  balances: (p: { ledgerId?: number | null; period?: string; q?: string }) =>
    http.get<AccountBalancesResult>('/api/gl/account-balances' + query({ ledgerId: p.ledgerId, period: p.period, q: p.q }))
}

/** Stage names as the journals page shows them. */
export const STAGE_LABELS: Record<string, string> = {
  DRAFT: 'Draft',
  SUBMITTED: 'Waiting approval',
  APPROVED: 'Ready to post',
  POSTED: 'Posted',
  REVERSED: 'Reversed',
  REJECTED: 'Rejected',
  CANCELLED: 'Cancelled'
}

/** Pill colour of a journal status: posted green, waiting blue, rejected red, others neutral. */
export function journalStatusClass(status: string): string {
  switch (status) {
    case 'POSTED': return 'cp-pill--ok'
    case 'SUBMITTED':
    case 'APPROVED': return 'cp-pill--info'
    case 'REJECTED': return 'cp-pill--bad'
    case 'REVERSED': return 'cp-pill--warn'
    default: return ''
  }
}

/** A net balance as accountants read it: 1,200.00 Dr, 250,000.00 Cr, 0.00. */
export function drCr(value: number): string {
  if (!value) return '0.00'
  return `${formatAmount(Math.abs(value))} ${value > 0 ? 'Dr' : 'Cr'}`
}
