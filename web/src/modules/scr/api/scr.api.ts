import { ApiError, http } from '@/shared/api/http'

// Screening (the FINOVA screening app merged into CAPITRA): the Federation Final List, its decisions and the member companies' inquiries.

export type ScrLetterFile = { letterFileId: number; fileName: string; mimeType: string; fileSizeBytes: number; sha256Hash: string }
export type ScrLetter = {
  letterId: number; letterNo: string; senderReference: string | null; senderName: string; channelCode: string; letterDate: string
  receivedAt: string; statusCode: string; notes: string | null; files: ScrLetterFile[]; decisionCount: number
}
export type ScrLetterRequest = { senderReference: string | null; senderName: string; channelCode: string; letterDate: string | null; notes: string | null }

export type ScrParty = {
  partyId: number; partyTypeCode: string; nameAr: string; nameEn: string | null; identityTypeCode: string; identityNo: string
  nationalityCode: string; birthDate: string | null; notes: string | null; aliases: string[]
}
export type ScrPartyRequest = Omit<ScrParty, 'partyId'>

export type ScrDecision = {
  decisionId: number; decisionRef: string; officialNo: string; decisionTypeCode: string; parentDecisionId: number | null
  parentDecisionRef: string | null; letterId: number | null; letterNo: string | null; scopeText: string; effectiveDate: string
  statusCode: string; returnReason: string | null; workflowInstanceId: number | null; enteredBy: string; reviewedBy: string | null
  reviewedAt: string | null; approvedBy: string | null; publishedAt: string | null; registryVersionNo: number | null
  parties: ScrParty[]; createdAt: string
}
export type ScrDecisionRequest = {
  officialNo: string; decisionTypeCode: string; parentDecisionId: number | null; letterId: number | null; scopeText: string
  effectiveDate: string | null; partyIds: number[]
}

export type ScrEntry = {
  partyId: number; decisionId: number; decisionRef: string; officialNo: string; partyTypeCode: string; nameAr: string; nameEn: string | null
  aliases: string[]; identityTypeCode: string; identityNo: string; nationalityCode: string; birthDate: string | null; scopeText: string
  listedFrom: string; liftedOn: string | null; statusCode: string; letterId: number | null; lastDecisionRef: string | null
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

export type ScrCompany = {
  memberCompanyId: number; companyCode: string; nameEn: string; nameAr: string; licenceNo: string | null; contactEmail: string | null
  statusCode: string; notes: string | null; userCount: number; inquiryCount: number
}
export type ScrCompanyRequest = Omit<ScrCompany, 'memberCompanyId' | 'userCount' | 'inquiryCount'>
export type ScrCompanyUser = {
  userId: string; userName: string; email: string | null; fullName: string | null; isActive: boolean; roleCode: string; lastLoginAt: string | null
}
export type ScrCompanyUserRequest = {
  userName: string; password: string | null; email: string | null; fullName: string | null; roleCode: string; isActive: boolean
}
export type ScrMyCompany = { memberCompanyId: number; companyCode: string; nameEn: string; nameAr: string }

const API = '/api/scr'

export const scrApi = {
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
  cancel: (id: number) => http.post<ScrDecision>(`${API}/decisions/${id}/cancel`),

  registry: () => http.get<ScrRegistry>(`${API}/registry`),

  myCompany: () => http.get<ScrMyCompany>(`${API}/inquiries/my-company`),
  screen: (r: ScrInquiryRequest) => http.post<ScrInquiry>(`${API}/inquiries`, r),
  myInquiries: () => http.get<ScrInquiry[]>(`${API}/inquiries/mine`),
  allInquiries: (companyId: number | null, result: string) =>
    http.get<ScrInquiry[]>(`${API}/inquiries?result=${encodeURIComponent(result)}${companyId ? `&companyId=${companyId}` : ''}`),
  inquiry: (id: number) => http.get<ScrInquiry>(`${API}/inquiries/${id}`),

  companies: () => http.get<ScrCompany[]>(`${API}/companies`),
  saveCompany: (id: number | null, r: ScrCompanyRequest) =>
    id ? http.put<ScrCompany>(`${API}/companies/${id}`, r) : http.post<ScrCompany>(`${API}/companies`, r),
  companyUsers: (companyId: number) => http.get<ScrCompanyUser[]>(`${API}/companies/${companyId}/users`),
  saveCompanyUser: (companyId: number, userId: string | null, r: ScrCompanyUserRequest) =>
    userId ? http.put<ScrCompanyUser>(`${API}/companies/${companyId}/users/${userId}`, r) : http.post<ScrCompanyUser>(`${API}/companies/${companyId}/users`, r),
  resetCompanyUserPassword: (companyId: number, userId: string, newPassword: string) =>
    http.post<ScrCompanyUser>(`${API}/companies/${companyId}/users/${userId}/reset-password`, { newPassword })
}

/** Labels of the codes, in English (tr() gives the Arabic). */
export const LABELS: Record<string, string> = {
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
  SCR_COMPANY_USER: 'Company user', SCR_COMPANY_ADMIN: 'Company administrator'
}

export const CHIP: Record<string, string> = {
  DRAFT: 'fx-chip--teal', IN_REVIEW: 'fx-chip--warn', AWAITING_APPROVAL: 'fx-chip--warn', RETURNED: 'fx-chip--bad', PUBLISHED: 'fx-chip--ok',
  CANCELLED: 'fx-chip--bad', ACTIVE: 'fx-chip--bad', SUSPENDED: 'fx-chip--warn', LIFTED: 'fx-chip--teal',
  NO_MATCH: 'fx-chip--ok', POSSIBLE_MATCH: 'fx-chip--warn', MATCH: 'fx-chip--bad', PREVIOUSLY_LISTED: 'fx-chip--teal'
}

export function errorText(err: unknown): string {
  return err instanceof Error ? err.message : String(err)
}

/** The validation errors of a failed save, by field (camelCase), first message each. */
export function fieldErrors(err: unknown): Record<string, string> {
  const out: Record<string, string> = {}
  const problem = err instanceof ApiError ? err.problem : null
  for (const [k, v] of Object.entries(problem?.errors ?? {})) out[k.charAt(0).toLowerCase() + k.slice(1)] = v[0] ?? ''
  return out
}
