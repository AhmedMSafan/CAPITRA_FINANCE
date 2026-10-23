// Workflow step "Excel Import & Validation" (api/mtg/portfolios/{id}/excel-import): APEX App 105 page 208.
import { http } from '@/shared/api/http'

export type ImportStatus = 'NOT_VALIDATED' | 'VALIDATED' | 'ERROR' | 'IMPORTED'

export type WorkbookSheet = { sheet: string; columns: string[] }

export type ExcelImport = {
  portfolioId: number
  portfolioNo: string
  projectCode: string
  projectName: string
  portfolioStatusCode: string
  portfolioStatusName: string
  importStatusCode: ImportStatus
  fileName: string | null
  totalRowCount: number
  validRowCount: number
  errorRowCount: number
  importedRowCount: number
  figures: {
    accountCount: number
    investorCount: number
    unitCount: number
    contractAmount: number
    paidAmount: number
    outstandingAmount: number
    arrearsAmount: number
  }
  accounts: {
    portfolioAccountId: number
    investorCode: string
    investorName: string
    unitCode: string
    sellerContractNo: string | null
    contractAmount: number
    paidAmount: number
    outstandingAmount: number
    arrearsAmount: number
  }[]
  format: WorkbookSheet[]
}

export type WorkbookRowResult = {
  sheet: string
  rowNumber: number
  entity: string
  code: string | null
  name: string | null
  isValid: boolean
  message: string
}

export type WorkbookResult = {
  importStatusCode: ImportStatus
  fileName: string
  totalRowCount: number
  validRowCount: number
  errorRowCount: number
  importedRowCount: number
  sheets: { sheet: string; totalRowCount: number; validRowCount: number; errorRowCount: number }[]
  rows: WorkbookRowResult[]
}

function form(file: File): FormData {
  const data = new FormData()
  data.append('file', file)
  return data
}

export const excelImportApi = {
  get: (portfolioId: number) => http.get<ExcelImport>(`/api/mtg/portfolios/${portfolioId}/excel-import`),
  validate: (portfolioId: number, file: File) =>
    http.upload<WorkbookResult>(`/api/mtg/portfolios/${portfolioId}/excel-import/validate`, form(file)),
  import: (portfolioId: number, file: File) =>
    http.upload<WorkbookResult>(`/api/mtg/portfolios/${portfolioId}/excel-import/import`, form(file))
}
