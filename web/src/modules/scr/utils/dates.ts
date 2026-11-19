// Dates for display on Screening pages. In Arabic (right to left) mode the browser would
// reorder "01 Oct 2026" into "Oct 2026 01", so each date is wrapped in Unicode left-to-right
// isolate marks. Only for showing on screen: do not use these in exports or input values.
import { formatDate as baseDate, formatDateTime as baseDateTime } from '@/shared/format/datetime'

const LRI = '⁦'
const PDI = '⁩'

function isolate(text: string): string {
  return text ? `${LRI}${text}${PDI}` : text
}

export function formatDate(value: string | Date | null | undefined): string {
  return isolate(baseDate(value))
}

export function formatDateTime(value: string | Date | null | undefined): string {
  return isolate(baseDateTime(value))
}
