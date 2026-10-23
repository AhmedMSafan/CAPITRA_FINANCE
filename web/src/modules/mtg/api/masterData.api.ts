// Portfolio acquisition master data (api/mtg): developers, signatories, projects, units and investors.
import { http } from '@/shared/api/http'
import type { FieldOption, FormValues } from '@/shared/crud/types'

type Versioned = { isActive: boolean; rowVersion: number }

export type Developer = Versioned & {
  developerId: number
  developerCode: string
  legalNameAr: string
  legalNameEn: string | null
  commercialRegNo: string | null
  taxRegNo: string | null
  incorporationDate: string | null
  addressText: string | null
  phoneNo: string | null
  emailAddress: string | null
  riskLevelCode: string | null
  riskLevelName: string | null
}

export type DeveloperSignatory = Versioned & {
  developerSignatoryId: number
  developerId: number
  developerName: string
  signatoryName: string
  nationalIdNo: string | null
  jobTitle: string | null
  authorizationReference: string | null
  validFromDate: string | null
  validToDate: string | null
}

export type Project = Versioned & {
  projectId: number
  developerId: number
  developerName: string
  projectCode: string
  projectNameAr: string
  projectNameEn: string | null
  locationText: string | null
  landArea: number | null
  titleDeedNo: string | null
  buildingLicenseNo: string | null
  buildingLicenseDate: string | null
  projectCompletionPct: number | null
  projectStatus: string | null
}

export type ProjectUnit = Versioned & {
  projectUnitId: number
  projectId: number
  projectName: string
  unitCode: string
  unitTypeCode: string | null
  unitTypeName: string | null
  buildingNo: string | null
  floorNo: string | null
  unitNo: string | null
  areaSqm: number | null
  contractAmount: number | null
  marketValueAmount: number | null
  licenseStatus: string | null
}

export type Investor = Versioned & {
  investorId: number
  investorCode: string
  fullNameAr: string
  fullNameEn: string | null
  nationalIdNo: string | null
  birthDate: string | null
  phoneNo: string | null
  emailAddress: string | null
  addressText: string | null
  employerName: string | null
  declaredIncomeAmount: number | null
}

/** CRUD calls for one resource; the row version goes with every update and delete. */
function resource<T extends Versioned>(path: string, idOf: (row: T) => number) {
  return {
    list: (query = '') => http.get<T[]>(`${path}${query}`),
    create: (values: FormValues) => http.post<T>(path, values),
    update: (row: T, values: FormValues) => http.put<T>(`${path}/${idOf(row)}`, { ...values, rowVersion: row.rowVersion }),
    remove: (row: T) => http.delete<void>(`${path}/${idOf(row)}?rowVersion=${row.rowVersion}`)
  }
}

export const developersApi = resource<Developer>('/api/mtg/developers', r => r.developerId)
export const developerSignatoriesApi = resource<DeveloperSignatory>('/api/mtg/developer-signatories', r => r.developerSignatoryId)
export const projectsApi = resource<Project>('/api/mtg/projects', r => r.projectId)
export const projectUnitsApi = resource<ProjectUnit>('/api/mtg/project-units', r => r.projectUnitId)
export const investorsApi = resource<Investor>('/api/mtg/investors', r => r.investorId)

export async function getDeveloperOptions(): Promise<FieldOption[]> {
  const rows = await developersApi.list('?activeOnly=true')
  return rows.map(r => ({ value: r.developerId, label: `${r.developerCode} - ${r.legalNameEn || r.legalNameAr}` }))
}

export async function getProjectOptions(): Promise<FieldOption[]> {
  const rows = await projectsApi.list('?activeOnly=true')
  return rows.map(r => ({ value: r.projectId, label: `${r.projectCode} - ${r.projectNameEn || r.projectNameAr}` }))
}
