// Reference data (api/adm/lookups): one API for every code table, described by its definition.
import { http } from '@/shared/api/http'
import type { FieldOption } from '@/shared/crud/types'

export type LookupFieldType = 'TEXT' | 'NUMBER' | 'BOOLEAN' | 'LOOKUP'

export type LookupFieldDefinition = {
  name: string
  label: string
  type: LookupFieldType
  required: boolean
  maxLength: number | null
  lookupName: string | null
  min: number | null
  max: number | null
}

export type LookupDefinition = {
  name: string
  module: string
  title: string
  apexPages: string
  codeMaxLength: number
  nameMaxLength: number
  showDisplayOrder: boolean
  displayOrderLabel: string
  displayOrderRequired: boolean
  showDescription: boolean
  fields: LookupFieldDefinition[]
}

export type LookupValue = {
  code: string
  nameAr: string
  nameEn: string
  description: string | null
  displayOrder: number
  isActive: boolean
  rowVersion: number
  extras: Record<string, unknown>
}

export type SaveLookupValue = {
  code: string
  nameAr: string
  nameEn: string
  description: string | null
  displayOrder: number
  isActive: boolean
  rowVersion: number | null
  extras: Record<string, unknown>
}

let definitions: Promise<LookupDefinition[]> | null = null

/** The definitions do not change while the app runs, so they are loaded once. */
export function getLookupDefinitions(): Promise<LookupDefinition[]> {
  definitions ??= http.get<LookupDefinition[]>('/api/adm/lookups').catch(error => {
    definitions = null
    throw error
  })
  return definitions
}

export function getLookupValues(name: string, includeInactive = false): Promise<LookupValue[]> {
  return http.get<LookupValue[]>(`/api/adm/lookups/${encodeURIComponent(name)}?includeInactive=${includeInactive}`)
}

/** Active values for a dropdown, as APEX LOVs show them. */
export async function getLookupOptions(name: string): Promise<FieldOption[]> {
  const values = await getLookupValues(name)
  return values.map(v => ({ value: v.code, label: v.nameEn }))
}

export function createLookupValue(name: string, value: SaveLookupValue): Promise<LookupValue> {
  return http.post<LookupValue>(`/api/adm/lookups/${encodeURIComponent(name)}`, value)
}

export function updateLookupValue(name: string, code: string, value: SaveLookupValue): Promise<LookupValue> {
  return http.put<LookupValue>(`/api/adm/lookups/${encodeURIComponent(name)}/${encodeURIComponent(code)}`, value)
}

export function deleteLookupValue(name: string, code: string, rowVersion: number): Promise<void> {
  return http.delete<void>(
    `/api/adm/lookups/${encodeURIComponent(name)}/${encodeURIComponent(code)}?rowVersion=${rowVersion}`
  )
}
