import { http } from '@/shared/api/http'
import { splitApiError } from '@/shared/crud/errors'
import type { AdminUser } from '../models/AdminUser'
import type { CreateAdminUserRequest } from '../models/CreateAdminUserRequest'
import type { UpdateAdminUserRequest } from '../models/UpdateAdminUserRequest'

const API_BASE = '/api/sec/users'

/** Shows every validation message (for example the password rules) instead of only the problem title. */
export async function withMessages<T>(call: Promise<T>): Promise<T> {
  try {
    return await call
  } catch (error) {
    const messages = splitApiError(error, [], 'Request failed.').form
    throw new Error(messages)
  }
}

export const getAdminUsers = () => http.get<AdminUser[]>(API_BASE)

export const getAdminUser = (userId: string) => http.get<AdminUser>(`${API_BASE}/${encodeURIComponent(userId)}`)

export async function createAdminUser(request: CreateAdminUserRequest): Promise<void> {
  await withMessages(http.post(API_BASE, request))
}

export async function updateAdminUser(userId: string, request: UpdateAdminUserRequest): Promise<void> {
  await withMessages(http.put(`${API_BASE}/${encodeURIComponent(userId)}`, request))
}

export async function setAdminUserRoles(userId: string, roles: string[]): Promise<void> {
  await withMessages(http.put(`${API_BASE}/${encodeURIComponent(userId)}/roles`, { roles }))
}

export async function resetAdminUserPassword(userId: string, newPassword: string): Promise<void> {
  await withMessages(http.post(`${API_BASE}/${encodeURIComponent(userId)}/reset-password`, { newPassword }))
}
