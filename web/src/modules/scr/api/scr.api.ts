import { ApiError, http } from '@/shared/api/http'
import { tr } from '@/shared/layout/language'

// Screening (the FINOVA screening app merged into CAPITRA): the Federation Final List, its decisions and the member companies' inquiries.

export type ScrLetterFile = { letterFileId: number; fileName: string; mimeType: string; fileSizeBytes: number; sha256Hash: string }
export type ScrLetter = {
  letterId: number; letterNo: string; senderReference: string | null; senderName: string; channelCode: string; letterDate: string
  receivedAt: string; statusCode: string; notes: string | null; files: ScrLetterFile[]; decisionCount: number
  createdBy?: string | null
}
export type ScrLetterRequest = { senderReference: string | null; senderName: string; channelCode: string; letterDate: string | null; notes: string | null }

export type ScrParty = {
  partyId: number; partyTypeCode: string; nameAr: string; nameEn: string | null; identityTypeCode: string; identityNo: string
  nationalityCode: string; birthDate: string | null; notes: string | null; aliases: string[]
  createdBy?: string | null
}
export type ScrPartyRequest = Omit<ScrParty, 'partyId'>

export type ScrDecision = {
  decisionId: number; decisionRef: string; officialNo: string; decisionTypeCode: string; parentDecisionId: number | null
  parentDecisionRef: string | null; letterId: number | null; letterNo: string | null; scopeText: string; effectiveDate: string
  statusCode: string; returnReason: string | null; workflowInstanceId: number | null; enteredBy: string; reviewedBy: string | null
  reviewedAt: string | null; approvedBy: string | null; publishedAt: string | null; registryVersionNo: number | null
  parties: ScrParty[]; createdAt: string
  workflowStepName?: string | null
  completedSteps?: ScrCompletedStep[] | null
  workflowStepHolder?: string | null
}
export type ScrCompletedStep = { key: string; stepCode: string; stepName: string; action: string | null; doneBy: string | null; doneAt: string | null }
export type ScrDocument = {
  documentId: number; stepCode: string; stepName: string; documentTitle: string; fileName: string; mimeType: string; fileSizeBytes: number
  createdBy: string; createdAt: string; canRemove: boolean
}
export type ScrStepView = {
  key: string; round: number; stepCode: string; stepName: string; status: 'DONE' | 'OPEN'; action: string | null; doneBy: string | null
  doneAt: string | null; comment: string | null; stepInstanceId: number | null; documents: ScrDocument[]
}
export type ScrDecisionRequest = {
  officialNo: string; decisionTypeCode: string; parentDecisionId: number | null; letterId: number | null; scopeText: string
  effectiveDate: string | null; partyIds: number[]
}

export type ScrEntry = {
  partyId: number; decisionId: number; decisionRef: string; officialNo: string; partyTypeCode: string; nameAr: string; nameEn: string | null
  aliases: string[]; identityTypeCode: string; identityNo: string; nationalityCode: string; birthDate: string | null; scopeText: string
  listedFrom: string; liftedOn: string | null; statusCode: string; letterId: number | null; lastDecisionRef: string | null
  enteredBy?: string | null
}
export type ScrRegistry = {
  versionNo: number; publishedAt: string | null; activeCount: number; suspendedCount: number; liftedCount: number; pendingCount: number
  entries: ScrEntry[]
}

export type ScrInquiryRequest = {
  subjectTypeCode: string; subjectName: string; identityTypeCode: string; identityNo: string; birthDate: string | null
  nationalityCode: string | null; purposeCode: string; purposeText: string | null; caseReference: string | null
}
export type ScrInquiryHit = {
  matchBasisCode: string; nameScorePct: number; matchedName: string; entryStatusCode: string; partyTypeCode: string; nameAr: string
  nameEn: string | null; identityTypeCode: string; identityNo: string; nationalityCode: string; decisionRef: string; officialNo: string
  scopeText: string; listedFrom: string; liftedOn: string | null
}
export type ScrInquiry = {
  inquiryId: number; inquiryRef: string; companyCode: string; requestedBy: string; subjectTypeCode: string; subjectName: string
  identityTypeCode: string; identityNo: string; birthDate: string | null; nationalityCode: string | null; purposeCode: string
  purposeText: string | null; caseReference: string | null; resultCode: string; registryVersionNo: number; screenedAt: string
  hits: ScrInquiryHit[]
}

export type ScrLicence = {
  licenceId: number; maxUsers: number; validFrom: string; validTo: string | null; yearlyInquiryQuota: number | null; notes: string | null
  createdBy: string; createdAt: string
}
export type ScrLicenceRequest = { maxUsers: number | null; validFrom: string | null; validTo: string | null; yearlyInquiryQuota: number | null; notes: string | null }
/** The company's administrator and the seats its other users hold (pending activation included). */
export type ScrCompanySeats = { adminUserName: string | null; adminEmail: string | null; adminStatusCode: string | null; usersInUse: number }
export type ScrCompany = {
  memberCompanyId: number; companyCode: string; nameEn: string; nameAr: string; licenceNo: string | null; contactEmail: string | null
  statusCode: string; notes: string | null; userCount: number; inquiryCount: number; createdBy?: string | null
  licence?: ScrLicence | null; seats?: ScrCompanySeats | null
  /** ACTIVE, NOT_STARTED, EXPIRED or NONE. */
  licenceStateCode?: string | null
  /** Billing: the price plan (empty = not billed yet) and how often the company is invoiced (MONTHLY, QUARTERLY, YEARLY). */
  pricePlanId?: number | null; pricePlanCode?: string | null; billingCycleCode?: string
}
export type ScrCompanyRequest = Pick<ScrCompany, 'companyCode' | 'nameEn' | 'nameAr' | 'licenceNo' | 'contactEmail' | 'statusCode' | 'notes' | 'pricePlanId' | 'billingCycleCode'>

// billing (EGP): a fee per user per month and a fee per inquiry; invoices per calendar period, never deleted
/** Inquiries fromInquiry to toInquiry of a period (null = and above) at unitPrice each. */
export type ScrTier = { fromInquiry: number; toInquiry: number | null; unitPrice: number }
export type ScrPricePlan = {
  pricePlanId: number; planCode: string; nameEn: string; nameAr: string; userMonthlyFee: number; inquiryFee: number
  /** Free inquiries per invoice period; tiers empty = the flat inquiryFee after them (graduated otherwise). */
  freeInquiries: number; tiers: ScrTier[]; currencyCode: string
  isActive: boolean; notes: string | null; companyCount: number; createdBy: string; createdAt: string; updatedBy: string | null; updatedAt: string | null
}
export type ScrPricePlanRequest = {
  planCode: string; nameEn: string; nameAr: string; userMonthlyFee: number | null; inquiryFee: number | null; isActive: boolean; notes: string | null
  freeInquiries: number | null; tiers: ScrTier[]
}
export type ScrInvoiceLine = { lineNo: number; userName: string; fullName: string | null; monthsText: string | null; userMonths: number; userAmount: number; inquiryCount: number }
/** The period's inquiries priced in one band: FREE, TIER or FLAT. */
export type ScrInvoiceBand = { bandNo: number; bandTypeCode: string; fromInquiry: number; toInquiry: number | null; unitPrice: number; quantity: number; amount: number }
/** STATUS: PREVIEW (not saved), ISSUED or CANCELLED. periodComplete is false while the period has not ended. */
export type ScrInvoice = {
  invoiceId: number; invoiceNo: string | null; memberCompanyId: number; companyCode: string; companyNameEn: string; companyNameAr: string
  periodFrom: string; periodTo: string; periodCode: string; planCode: string; userMonthlyFee: number; inquiryFee: number; freeInquiries: number; currencyCode: string
  userMonths: number; userAmount: number; inquiryCount: number; inquiryAmount: number; totalAmount: number; statusCode: string
  issuedAt: string | null; issuedBy: string | null; cancelledAt: string | null; cancelledBy: string | null; cancelReason: string | null; notes: string | null
  periodComplete: boolean; lines: ScrInvoiceLine[]; bands: ScrInvoiceBand[]
}
export type ScrBillingCompany = {
  memberCompanyId: number; companyCode: string; nameEn: string; nameAr: string; statusCode: string; pricePlanId: number | null; planCode: string | null
  billingCycleCode: string; currentFrom: string; currentTo: string; currentPeriodCode: string; activeUsersNow: number; currentUserMonths: number
  currentInquiries: number; currentAmount: number | null; currentInvoiced: boolean; invoiceCount: number; invoicedTotal: number; lastInvoicedTo: string | null
}

/** Amounts in EGP, two decimals, Western digits in both languages. */
/** The plan's inquiry schedule priced for a number of inquiries (graduated): the same rule as the invoices. */
export function priceInquiries(free: number, flatFee: number, tiers: ScrTier[], inquiries: number) {
  const schedule: { type: string; from: number; to: number | null; price: number }[] = []
  if (free > 0) schedule.push({ type: 'FREE', from: 1, to: free, price: 0 })
  if (!tiers.length) schedule.push({ type: 'FLAT', from: free + 1, to: null, price: flatFee })
  else tiers.forEach((t) => schedule.push({ type: 'TIER', from: t.fromInquiry, to: t.toInquiry, price: t.unitPrice }))
  const bands = schedule.map((b) => {
    const quantity = Math.max(0, Math.min(inquiries, b.to ?? Infinity) - b.from + 1)
    return { ...b, quantity, amount: Math.round(quantity * b.price * 100 + Number.EPSILON) / 100 }
  }).filter((b) => b.quantity > 0)
  return { bands, total: bands.reduce((s, b) => s + b.amount, 0) }
}

export const egp = (v: number | null | undefined) => v == null ? '-' : `${v.toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} EGP`
export type ScrCompanyAdminRequest = { userName: string | null; email: string | null; fullName: string | null }
/** STATUS: PENDING_ACTIVATION (link not used yet; holds a seat), ACTIVE or INACTIVE. No password is ever shown. */
export type ScrCompanyUser = {
  userId: string; userName: string; email: string | null; fullName: string | null; isActive: boolean; roleCode: string; lastLoginAt: string | null
  statusCode: string
}
/** A new company user is always a company user; the administrator comes with the company. */
export type ScrCompanyUserRequest = { userName: string; email: string | null; fullName: string | null; isActive: boolean }
/** What happened to the activation or reset e-mail: SENT, FAILED, NOT_CONFIGURED (no mail server set up) or NO_ADDRESS. */
export type ScrEmailOutcome = { sent: boolean; statusCode: string; error: string | null }
export type ScrMyCompany = { memberCompanyId: number; companyCode: string; nameEn: string; nameAr: string; maxUsers: number | null }

const API = '/api/scr'

export const scrApi = {
  pricePlans: () => http.get<ScrPricePlan[]>(`${API}/billing/plans`),
  createPricePlan: (r: ScrPricePlanRequest) => http.post<ScrPricePlan>(`${API}/billing/plans`, r),
  updatePricePlan: (id: number, r: ScrPricePlanRequest) => http.put<ScrPricePlan>(`${API}/billing/plans/${id}`, r),
  billingOverview: () => http.get<ScrBillingCompany[]>(`${API}/billing/overview`),
  invoices: (q: { memberCompanyId?: number; statusCode?: string; year?: number } = {}) => {
    const p = new URLSearchParams(Object.entries(q).filter(([, v]) => v !== undefined && v !== '' && v !== null).map(([k, v]) => [k, String(v)]))
    return http.get<ScrInvoice[]>(`${API}/billing/invoices?${p}`)
  },
  invoice: (id: number) => http.get<ScrInvoice>(`${API}/billing/invoices/${id}`),
  previewInvoice: (companyId: number, periodFrom: string) => http.get<ScrInvoice>(`${API}/billing/companies/${companyId}/preview?periodFrom=${periodFrom}`),
  issueInvoice: (companyId: number, periodFrom: string, notes: string | null) => http.post<ScrInvoice>(`${API}/billing/companies/${companyId}/invoices`, { periodFrom, notes }),
  cancelInvoice: (id: number, reason: string) => http.post<ScrInvoice>(`${API}/billing/invoices/${id}/cancel`, { reason }),
  letters: (search = '') => http.get<ScrLetter[]>(`${API}/letters?search=${encodeURIComponent(search)}`),
  createLetter: (r: ScrLetterRequest) => http.post<ScrLetter>(`${API}/letters`, r),
  addLetterFile: (letterId: number, file: File) => {
    const form = new FormData()
    form.append('file', file)
    return http.upload<ScrLetter>(`${API}/letters/${letterId}/files`, form)
  },
  letterFile: (letterFileId: number) => http.blob(`${API}/letters/files/${letterFileId}`),

  parties: (search = '') => http.get<ScrParty[]>(`${API}/parties?search=${encodeURIComponent(search)}`),
  saveParty: (partyId: number | null, r: ScrPartyRequest) =>
    partyId ? http.put<ScrParty>(`${API}/parties/${partyId}`, r) : http.post<ScrParty>(`${API}/parties`, r),

  decisions: (status = '') => http.get<ScrDecision[]>(`${API}/decisions?status=${encodeURIComponent(status)}`),
  decision: (id: number) => http.get<ScrDecision>(`${API}/decisions/${id}`),
  saveDecision: (id: number | null, r: ScrDecisionRequest) =>
    id ? http.put<ScrDecision>(`${API}/decisions/${id}`, r) : http.post<ScrDecision>(`${API}/decisions`, r),
  submit: (id: number) => http.post<ScrDecision>(`${API}/decisions/${id}/submit`),
  /** Starts the decision's approval workflow (Run in Decisions awaiting approval). */
  run: (id: number) => http.post<ScrDecision>(`${API}/decisions/${id}/run`),
  cancel: (id: number) => http.post<ScrDecision>(`${API}/decisions/${id}/cancel`),

  /** Every step of a decision with its documents; completed steps are view-only. */
  steps: (id: number) => http.get<ScrStepView[]>(`${API}/decisions/${id}/steps`),
  /** Completes the user's task on the decision with its comment (one transaction with the step's comment). */
  completeStep: (id: number, inboxId: number, action: 'ACCEPT' | 'REJECT', comment: string | null) =>
    http.post<ScrDecision>(`${API}/decisions/${id}/steps/${inboxId}/complete`, { action, comment }),
  /** Adds a file at data entry (inboxId null) or at the workflow step of the user's task. */
  addDocument: (id: number, inboxId: number | null, file: File, title: string) => {
    const form = new FormData()
    form.append('file', file)
    form.append('title', title)
    return http.upload<ScrDocument>(inboxId ? `${API}/decisions/${id}/steps/${inboxId}/documents` : `${API}/decisions/${id}/documents`, form)
  },
  removeDocument: (documentId: number) => http.post<void>(`${API}/decisions/documents/${documentId}/remove`),
  documentFile: (documentId: number) => http.blob(`${API}/decisions/documents/${documentId}`),

  registry: () => http.get<ScrRegistry>(`${API}/registry`),

  myCompany: () => http.get<ScrMyCompany>(`${API}/inquiries/my-company`),
  screen: (r: ScrInquiryRequest) => http.post<ScrInquiry>(`${API}/inquiries`, r),
  myInquiries: () => http.get<ScrInquiry[]>(`${API}/inquiries/mine`),
  allInquiries: (companyId: number | null, result: string) =>
    http.get<ScrInquiry[]>(`${API}/inquiries?result=${encodeURIComponent(result)}${companyId ? `&companyId=${companyId}` : ''}`),
  inquiry: (id: number) => http.get<ScrInquiry>(`${API}/inquiries/${id}`),

  companies: () => http.get<ScrCompany[]>(`${API}/companies`),
  /** A new company in one step: the company, its licence and its administrator (who gets an activation e-mail). */
  onboardCompany: (company: ScrCompanyRequest, licence: ScrLicenceRequest, admin: ScrCompanyAdminRequest) =>
    http.post<{ company: ScrCompany; admin: ScrCompanyUser; email: ScrEmailOutcome }>(`${API}/companies`, { company, licence, admin }),
  updateCompany: (id: number, r: ScrCompanyRequest) => http.put<ScrCompany>(`${API}/companies/${id}`, r),
  licences: (companyId: number) => http.get<ScrLicence[]>(`${API}/companies/${companyId}/licences`),
  addLicence: (companyId: number, r: ScrLicenceRequest) => http.post<ScrLicence>(`${API}/companies/${companyId}/licences`, r),
  companyUsers: (companyId: number) => http.get<ScrCompanyUser[]>(`${API}/companies/${companyId}/users`),
  createCompanyUser: (companyId: number, r: ScrCompanyUserRequest) =>
    http.post<{ user: ScrCompanyUser; email: ScrEmailOutcome }>(`${API}/companies/${companyId}/users`, r),
  updateCompanyUser: (companyId: number, userId: string, r: ScrCompanyUserRequest) =>
    http.put<ScrCompanyUser>(`${API}/companies/${companyId}/users/${userId}`, r),
  /** E-mails a new activation link (pending account) or a password-reset link. */
  sendUserLink: (companyId: number, userId: string) => http.post<ScrEmailOutcome>(`${API}/companies/${companyId}/users/${userId}/send-link`),
  /** The link itself, to pass on another way when the e-mail does not arrive; earlier links stop working. */
  copyUserLink: (companyId: number, userId: string) =>
    http.post<{ link: string; kind: string }>(`${API}/companies/${companyId}/users/${userId}/copy-link`)
}

/** Labels of the codes, in English (tr() gives the Arabic). */
export const LABELS: Record<string, string> = {
  FREE: 'Free', TIER: 'Tier', FLAT: 'Each inquiry',
  MONTHLY: 'Monthly', QUARTERLY: 'Quarterly', YEARLY: 'Yearly', ISSUED: 'Issued', PREVIEW: 'Preview',
  PERSON: 'Person', COMPANY: 'Company',
  NATIONAL_ID: 'National ID', PASSPORT: 'Passport', COMMERCIAL_REGISTER: 'Commercial register', OTHER: 'Other',
  COURIER: 'Courier', EMAIL: 'Email', HAND: 'By hand', PORTAL: 'Portal',
  RECEIVED: 'Received', INDEXED: 'Indexed', CLOSED: 'Closed',
  LISTING: 'Listing', AMENDMENT: 'Amendment', SUSPENSION: 'Suspension', LIFTING: 'Lifting',
  DRAFT: 'Draft', IN_REVIEW: 'In review', AWAITING_APPROVAL: 'Awaiting approval', RETURNED: 'Returned', PUBLISHED: 'Published', CANCELLED: 'Cancelled',
  ACTIVE: 'Active', SUSPENDED: 'Suspended', LIFTED: 'Lifted',
  NO_MATCH: 'No match', POSSIBLE_MATCH: 'Possible match', MATCH: 'Match', PREVIOUSLY_LISTED: 'Previously listed',
  ID_AND_NAME: 'Identity and name', ID_ONLY: 'Identity only (name differs)', NAME_ONLY: 'Name only (identity differs)',
  ONBOARDING: 'Customer onboarding', TRANSACTION: 'Transaction', PERIODIC_REVIEW: 'Periodic review',
  MEMBER_COMPANY_USER: 'Company user', MEMBER_COMPANY_ADMIN: 'Company administrator',
  PENDING_ACTIVATION: 'Pending activation', INACTIVE: 'Inactive', NOT_STARTED: 'Not started yet', EXPIRED: 'Expired', NONE: 'No licence'
}

export const CHIP: Record<string, string> = {
  DRAFT: 'fx-chip--teal', IN_REVIEW: 'fx-chip--warn', AWAITING_APPROVAL: 'fx-chip--warn', RETURNED: 'fx-chip--bad', PUBLISHED: 'fx-chip--ok',
  CANCELLED: 'fx-chip--bad', ACTIVE: 'fx-chip--bad', SUSPENDED: 'fx-chip--warn', LIFTED: 'fx-chip--teal',
  NO_MATCH: 'fx-chip--ok', POSSIBLE_MATCH: 'fx-chip--warn', MATCH: 'fx-chip--bad', PREVIOUSLY_LISTED: 'fx-chip--teal'
}

export function errorText(err: unknown): string {
  return tr(err instanceof Error ? err.message : String(err))
}

/** The validation errors of a failed save, by field (camelCase), first message each. */
export function fieldErrors(err: unknown): Record<string, string> {
  const out: Record<string, string> = {}
  const problem = err instanceof ApiError ? err.problem : null
  for (const [k, v] of Object.entries(problem?.errors ?? {})) out[k.charAt(0).toLowerCase() + k.slice(1)] = tr(v[0] ?? '')
  return out
}
