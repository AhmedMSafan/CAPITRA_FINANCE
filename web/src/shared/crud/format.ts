import type { ColumnType } from './types'
import { formatDate } from '@/shared/format/datetime'

const amountFormat = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const numberFormat = new Intl.NumberFormat('en-US', { maximumFractionDigits: 6 })

/** Text shown in a grid cell. Dates without a time (DateOnly) are shown as they are, with no time-zone shift. */
export function formatCell(value: unknown, type: ColumnType = 'text'): string {
  if (value === null || value === undefined || value === '') return ''

  switch (type) {
    case 'amount':
      return amountFormat.format(Number(value))
    case 'number':
      return numberFormat.format(Number(value))
    case 'pct':
      return `${numberFormat.format(Number(value))}%`
    case 'date':
      return formatDate(new Date(`${String(value)}T00:00:00Z`))
    case 'bool':
      return value ? 'Yes' : 'No'
    default:
      return String(value)
  }
}

export function toCsv(headers: string[], rows: string[][]): string {
  const escape = (v: string) => (/[",\n]/.test(v) ? `"${v.replace(/"/g, '""')}"` : v)
  return [headers, ...rows].map(r => r.map(escape).join(',')).join('\r\n')
}

export function downloadCsv(fileName: string, csv: string): void {
  // The BOM makes Excel read Arabic text correctly.
  const blob = new Blob(['﻿', csv], { type: 'text/csv;charset=utf-8' })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = fileName
  link.click()
  URL.revokeObjectURL(url)
}
