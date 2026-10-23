// Signed-in session: the JWT and the user profile returned by api/sec/auth.
// Kept in sessionStorage so closing the browser tab signs the user out.

const TOKEN_KEY = 'capitra.token'
const USER_KEY = 'capitra.user'
const EXPIRY_KEY = 'capitra.expiresAt'

export type SessionUser = {
  userId: string
  userName: string
  fullName?: string | null
  employeeNo?: string | null
  department?: string | null
  roles: string[]
  permissions: string[]
}

export type LoginResponse = {
  token: string
  expiresAt: string
  user: SessionUser
}

export function saveSession(response: LoginResponse): void {
  sessionStorage.setItem(TOKEN_KEY, response.token)
  sessionStorage.setItem(EXPIRY_KEY, response.expiresAt)
  saveUser(response.user)
}

export function saveUser(user: SessionUser): void {
  sessionStorage.setItem(USER_KEY, JSON.stringify(user))
}

export function clearSession(): void {
  sessionStorage.removeItem(TOKEN_KEY)
  sessionStorage.removeItem(USER_KEY)
  sessionStorage.removeItem(EXPIRY_KEY)
}

export function getToken(): string | null {
  return isAuthenticated() ? sessionStorage.getItem(TOKEN_KEY) : null
}

export function getUser(): SessionUser | null {
  const raw = sessionStorage.getItem(USER_KEY)
  if (!raw) return null

  try {
    return JSON.parse(raw) as SessionUser
  } catch {
    clearSession()
    return null
  }
}

export function isAuthenticated(): boolean {
  if (!sessionStorage.getItem(TOKEN_KEY)) return false

  const expiresAt = new Date(sessionStorage.getItem(EXPIRY_KEY) ?? '').getTime()
  if (Number.isNaN(expiresAt) || expiresAt <= Date.now()) {
    clearSession()
    return false
  }

  return true
}

/** UI hint only: the API checks every permission again on each request. */
export function hasPermission(permission: string): boolean {
  const permissions = getUser()?.permissions ?? []
  return permissions.includes('*') || permissions.some(p => p.toUpperCase() === permission.toUpperCase())
}
