import { http } from '@/shared/api/http'
import { clearSession, saveSession, saveUser, type LoginResponse, type SessionUser } from '@/shared/auth/session'

/** The password was right but the account signs in with an authenticator app: CODE (enter it) or SETUP (set it up first). */
export type SecondStep = { kind: 'CODE' | 'SETUP'; challenge: string; userName: string; fullName: string | null; expiresInSeconds: number }
export type AuthenticatorSetup = { qrCode: string; sharedKey: string; otpAuthUri: string }
export type SecondStepResult = { login: LoginResponse; recoveryCodes: string[] | null; recoveryCodesLeft: number }

const SECOND_STEP_KEY = 'capitra.secondStep'

/** Signs in; when the account needs its authenticator code, returns the second step instead (kept for the code page only). */
export async function login(userName: string, password: string): Promise<{ user?: SessionUser; secondStep?: SecondStep }> {
  const response = await http.post<LoginResponse | { secondStep: SecondStep }>('/api/sec/auth/login', { userName, password })
  if ('secondStep' in response) {
    sessionStorage.setItem(SECOND_STEP_KEY, JSON.stringify(response.secondStep))
    return { secondStep: response.secondStep }
  }
  saveSession(response)
  return { user: response.user }
}

/** The pending second step of this browser tab, if any. */
export function pendingSecondStep(): SecondStep | null {
  try {
    const raw = sessionStorage.getItem(SECOND_STEP_KEY)
    return raw ? (JSON.parse(raw) as SecondStep) : null
  } catch {
    return null
  }
}

export function clearSecondStep(): void {
  sessionStorage.removeItem(SECOND_STEP_KEY)
}

export const authenticatorSetup = (challenge: string) => http.post<AuthenticatorSetup>('/api/sec/auth/second-step/setup', { challenge })

/** Confirms the authenticator with a first code: signs in and returns the backup codes (shown once). */
export async function enableAuthenticator(challenge: string, code: string): Promise<SecondStepResult> {
  const r = await http.post<SecondStepResult>('/api/sec/auth/second-step/enable', { challenge, code })
  clearSecondStep()
  saveSession(r.login)
  return r
}

/** The authenticator's code, or one backup code: signs in. */
export async function verifySecondStep(challenge: string, code: string | null, recoveryCode: string | null): Promise<SecondStepResult> {
  const r = await http.post<SecondStepResult>('/api/sec/auth/second-step/verify', { challenge, code, recoveryCode })
  clearSecondStep()
  saveSession(r.login)
  return r
}

/** Reloads the profile, roles and permissions from the server. */
export async function refreshCurrentUser(): Promise<SessionUser> {
  const user = await http.get<SessionUser>('/api/sec/auth/me')
  saveUser(user)
  return user
}

/** Replaces the signed-in user's password; the reply's new token replaces the old one (which stops working). */
export async function changePassword(currentPassword: string, newPassword: string): Promise<SessionUser> {
  const response = await http.post<LoginResponse>('/api/sec/auth/change-password', { currentPassword, newPassword })
  saveSession(response)
  return response.user
}

/** Signs out: the server marks the session ended in the sign-in log (best effort), then the token is dropped. */
export async function logout(): Promise<void> {
  try {
    await http.post<void>('/api/sec/auth/logout')
  } catch {
    // an expired or refused token: the session is already over for the server
  }
  clearSession()
}

/** Who an activation or password-reset link belongs to; fails when it was used, replaced or expired. */
export type AccountLinkInfo = { userName: string; fullName: string | null; activating: boolean }
export const checkAccountLink = (userId: string, token: string) => http.post<AccountLinkInfo>('/api/sec/auth/link/check', { userId, token })

/** Sets the user's own password from the link (it then stops working); the user signs in afterwards. */
export const setPasswordFromLink = (userId: string, token: string, newPassword: string) =>
  http.post<void>('/api/sec/auth/link/set-password', { userId, token, newPassword })

/** E-mails a reset link if the account exists; the answer is the same either way. */
export const forgotPassword = (userNameOrEmail: string) => http.post<{ message: string }>('/api/sec/auth/forgot-password', { userNameOrEmail })
