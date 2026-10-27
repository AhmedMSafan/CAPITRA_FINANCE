// Shared API client. Every call carries the JWT; a 401 ends the session and returns to the login page.
import { ref } from 'vue'
import router from '@/router'
import { clearSession, getToken } from '@/shared/auth/session'

const API_BASE_URL = (import.meta.env.VITE_API_BASE_URL ?? 'https://localhost:7159').replace(/\/$/, '')

/** RFC 7807 problem returned by the API (validation errors come in `errors`). */
export type ProblemDetails = {
  title?: string
  detail?: string
  status?: number
  errors?: Record<string, string[]>
}

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    public readonly problem: ProblemDetails | null
  ) {
    super(
      status === 403
        ? 'You do not have permission for this. Ask an administrator for it; if your roles were just changed, sign out and sign in again.'
        : (problem?.detail ?? problem?.title ?? `Request failed (${status}).`)
    )
    this.name = 'ApiError'
  }
}

/** True while the API cannot be reached at all (not started, or another address): the layout shows a banner. */
export const serverDown = ref(false)
export const apiBaseUrl = API_BASE_URL

async function request<T>(method: string, path: string, body?: unknown): Promise<T> {
  const headers: Record<string, string> = { Accept: 'application/json' }
  const token = getToken()
  if (token) headers.Authorization = `Bearer ${token}`
  // A FormData body (file upload) sets its own multipart Content-Type.
  const isForm = body instanceof FormData
  if (body !== undefined && !isForm) headers['Content-Type'] = 'application/json'

  let response: Response
  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : isForm ? body : JSON.stringify(body)
    })
  } catch (e) {
    serverDown.value = true
    throw e
  }
  serverDown.value = false

  if (response.status === 401 && token) {
    clearSession()
    await router.push({ name: 'login', query: { redirect: router.currentRoute.value.fullPath } })
  }

  const text = await response.text()
  const data = text ? JSON.parse(text) : null

  if (!response.ok) throw new ApiError(response.status, data as ProblemDetails | null)

  return data as T
}

export const http = {
  get: <T>(path: string) => request<T>('GET', path),
  post: <T>(path: string, body?: unknown) => request<T>('POST', path, body),
  upload: <T>(path: string, form: FormData) => request<T>('POST', path, form),
  put: <T>(path: string, body?: unknown) => request<T>('PUT', path, body),
  delete: <T>(path: string) => request<T>('DELETE', path)
}
