// Communication Center: the mail servers every e-mail goes through, and the log of what was sent.
import { ApiError, http } from '@/shared/api/http'
import { tr } from '@/shared/layout/language'

export type CmcMailServer = {
  mailServerId: number; serverName: string; hostName: string; portNo: number; isSslEnabled: boolean; userName: string | null
  /** A password is stored (it is never sent back). */ hasPassword: boolean
  /** The stored password can no longer be decrypted: enter it again. */ passwordUnreadable: boolean
  fromAddress: string; fromName: string; isActive: boolean
  lastTestAt: string | null; lastTestStatusCode: string | null; lastTestError: string | null
  createdBy: string; createdAt: string; updatedBy: string | null; updatedAt: string | null
}
export type CmcMailServerRequest = {
  serverName: string; hostName: string; portNo: number | null; isSslEnabled: boolean; userName: string | null
  /** Empty keeps the stored password. */ password: string | null; clearPassword: boolean; fromAddress: string; fromName: string
}
export type CmcTestResult = { sent: boolean; statusCode: string; error: string | null }
/** SOURCE: COMMUNICATION_CENTER (an active server here), APPLICATION_SETTINGS (the Email settings) or NONE. */
export type CmcMailStatus = { sourceCode: string; serverName: string | null; hostName: string | null; fromAddress: string | null; lastTestAt: string | null; lastTestStatusCode: string | null }
export type CmcEmailLog = {
  emailLogId: number; toAddress: string; kindCode: string; subject: string; statusCode: string; errorText: string | null
  attemptedAt: string; serverName: string | null; userName: string | null; createdBy: string
}
export type CmcEmailLogQuery = { search?: string; statusCode?: string; kindCode?: string; fromDate?: string; toDate?: string; take?: number }

const API = '/api/cmc'

export const cmcApi = {
  servers: () => http.get<CmcMailServer[]>(`${API}/mail-servers`),
  status: () => http.get<CmcMailStatus>(`${API}/mail-servers/status`),
  createServer: (r: CmcMailServerRequest) => http.post<CmcMailServer>(`${API}/mail-servers`, r),
  updateServer: (id: number, r: CmcMailServerRequest) => http.put<CmcMailServer>(`${API}/mail-servers/${id}`, r),
  testServer: (id: number, toAddress: string) => http.post<CmcTestResult>(`${API}/mail-servers/${id}/test`, { toAddress }),
  activate: (id: number) => http.post<CmcMailServer>(`${API}/mail-servers/${id}/activate`),
  deactivate: (id: number) => http.post<CmcMailServer>(`${API}/mail-servers/${id}/deactivate`),
  emailLog: (q: CmcEmailLogQuery) => {
    const p = new URLSearchParams(Object.entries(q).filter(([, v]) => v !== undefined && v !== '' && v !== null).map(([k, v]) => [k, String(v)]))
    return http.get<CmcEmailLog[]>(`${API}/email-log?${p}`)
  }
}

export const LABELS: Record<string, string> = {
  SENT: 'Sent', FAILED: 'Failed', NOT_CONFIGURED: 'No mail server', NO_ADDRESS: 'No e-mail address', LINK_COPIED: 'Link copied',
  ACTIVATION: 'Account activation', PASSWORD_RESET: 'Password reset', TEST: 'Test e-mail',
  COMMUNICATION_CENTER: 'Communication Center', APPLICATION_SETTINGS: 'Application settings', NONE: 'None'
}
export const STATUS_CHIP: Record<string, string> = {
  SENT: 'fx-chip--ok', FAILED: 'fx-chip--bad', NOT_CONFIGURED: 'fx-chip--warn', NO_ADDRESS: 'fx-chip--warn', LINK_COPIED: 'fx-chip--teal'
}

export function errorText(err: unknown): string {
  return tr(err instanceof Error ? err.message : String(err))
}

export function fieldErrors(err: unknown): Record<string, string> {
  const out: Record<string, string> = {}
  const problem = err instanceof ApiError ? err.problem : null
  for (const [k, v] of Object.entries(problem?.errors ?? {})) out[k.charAt(0).toLowerCase() + k.slice(1)] = tr(v[0] ?? '')
  return out
}
