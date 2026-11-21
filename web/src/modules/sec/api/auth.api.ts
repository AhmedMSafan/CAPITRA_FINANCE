import { http } from '@/shared/api/http'
import { clearSession, saveSession, saveUser, type LoginResponse, type SessionUser } from '@/shared/auth/session'

export async function login(userName: string, password: string): Promise<SessionUser> {
  const response = await http.post<LoginResponse>('/api/sec/auth/login', { userName, password })
  saveSession(response)
  return response.user
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
