import { http } from '@/shared/api/http'

/** ADM_LKP_CURRENCIES, the list APEX App 105 pages 37/38 maintain. */
export type Currency = {
  currencyCode: string
  currencyNameEn: string
  currencyNameAr: string
  decimalPlaces: number
  isActive: boolean
  rowVersion: number
}

export type SaveCurrency = Omit<Currency, 'rowVersion'> & { rowVersion: number | null }

const API = '/api/adm/currencies'

export const getCurrencies = () => http.get<Currency[]>(API)
export const createCurrency = (c: SaveCurrency) => http.post<Currency>(API, c)
export const updateCurrency = (code: string, c: SaveCurrency) => http.put<Currency>(`${API}/${encodeURIComponent(code)}`, c)
export const deleteCurrency = (code: string, rowVersion: number) =>
  http.delete<void>(`${API}/${encodeURIComponent(code)}?rowVersion=${rowVersion}`)
