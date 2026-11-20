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

export function logout(): void {
  clearSession()
}
