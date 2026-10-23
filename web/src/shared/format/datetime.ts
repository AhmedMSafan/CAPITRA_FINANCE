// Naming standard section 11: the API stores and sends UTC; the UI always shows Cairo time.
export const DISPLAY_TIME_ZONE = 'Africa/Cairo'

const dateFormat = new Intl.DateTimeFormat('en-GB', {
  timeZone: DISPLAY_TIME_ZONE,
  day: '2-digit',
  month: 'short',
  year: 'numeric'
})

const dateTimeFormat = new Intl.DateTimeFormat('en-GB', {
  timeZone: DISPLAY_TIME_ZONE,
  day: '2-digit',
  month: 'short',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit'
})

const weekdayFormat = new Intl.DateTimeFormat('en-US', { timeZone: DISPLAY_TIME_ZONE, weekday: 'long' })

function toDate(value: string | Date): Date {
  return typeof value === 'string' ? new Date(value) : value
}

export function formatDate(value: string | Date | null | undefined): string {
  return value ? dateFormat.format(toDate(value)) : ''
}

export function formatDateTime(value: string | Date | null | undefined): string {
  return value ? dateTimeFormat.format(toDate(value)) : ''
}

export function formatWeekday(value: string | Date): string {
  return weekdayFormat.format(toDate(value))
}

/** Today's date in Cairo as YYYY-MM-DD, the value a date input expects. */
export function todayInCairo(): string {
  return new Intl.DateTimeFormat('en-CA', { timeZone: DISPLAY_TIME_ZONE }).format(new Date())
}
