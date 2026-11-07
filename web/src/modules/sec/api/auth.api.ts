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

export function logout(): void {
  clearSession()
}
