import { http } from '@/shared/api/http'
import { withMessages } from './adminUserService'
import type { AdminRole } from '../models/AdminRole'

const API_BASE = '/api/sec/roles'

export const getAdminRoles = () => http.get<AdminRole[]>(API_BASE)

/** Every permission code a role can be granted. */
export const getPermissionCatalog = () => http.get<string[]>(`${API_BASE}/permissions`)

export async function createAdminRole(roleName: string): Promise<void> {
  await withMessages(http.post(API_BASE, { roleName }))
}

export const setRolePermissions = (roleId: string, permissions: string[]) =>
  withMessages(http.put<AdminRole>(`${API_BASE}/${encodeURIComponent(roleId)}/permissions`, { permissions }))
