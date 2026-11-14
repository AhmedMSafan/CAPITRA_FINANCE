// Shared by the factoring workflow step pages.
import { dateLocale, tr } from '@/shared/layout/language'
import { DISPLAY_TIME_ZONE } from '@/shared/format/datetime'
import { fctDocumentsApi } from '../api/fct.api'

export function formatAmount(value: number | null | undefined, currency?: string | null) {
  if (value == null) return '-'
  const amount = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(value)
  return currency ? `${currency} ${amount}` : amount
}

export const formatRate = (value: number | null | undefined) => (value == null ? '-' : `${value}%`)

export const formatMonths = (value: number | null | undefined) => (value == null ? '-' : `${value} ${tr('months')}`)

export const formatDate = (v?: string | null) =>
  v ? new Intl.DateTimeFormat(dateLocale(), { year: 'numeric', month: 'short', day: '2-digit', timeZone: DISPLAY_TIME_ZONE }).format(new Date(v)) : '-'

/** The yyyy-MM-dd a date input shows, from a DateOnly or a UTC-midnight DateTime */
export const dayInput = (v?: string | null) => (v ? v.slice(0, 10) : '')

/** A date input's value for a DateOnly column */
export const toDay = (v: string) => v || null

/** A date input's value for a DateTime (UTC) column */
export const toUtcDay = (v: string) => (v ? `${v}T00:00:00Z` : null)

export const errorText = (e: unknown, fallback: string) => (e instanceof Error ? e.message : tr(fallback))

const escapeHtml = (v: string) => v.replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c] as string)

/**
 * FINOVA printed the page (window.print) with everything but the letter hidden; here the letter is printed on its own,
 * so the application's menus never reach the paper.
 */
export function printLetter(title: string, subtitle: string, rows: [string, string][], sections: [string, string][] = []) {
  const win = window.open('', '_blank', 'width=900,height=700')
  if (!win) return
  const dl = rows.map(([k, v]) => `<div><dt>${escapeHtml(k)}</dt><dd>${escapeHtml(v)}</dd></div>`).join('')
  const body = sections.map(([h, p]) => `<h2>${escapeHtml(h)}</h2><p>${escapeHtml(p)}</p>`).join('')
  win.document.write(`<!doctype html><html><head><meta charset="utf-8"><title>${escapeHtml(title)}</title><style>
    body{font-family:Arial,sans-serif;margin:32px;color:#111}h1{margin:0 0 4px;font-size:22px}.sub{color:#555;margin:0 0 20px}
    dl{display:grid;grid-template-columns:repeat(2,1fr);gap:14px 28px;margin:0 0 24px}dt{font-size:11px;color:#666;text-transform:uppercase}
    dd{margin:2px 0 0;font-weight:700}h2{font-size:15px;margin:18px 0 6px}p{white-space:pre-wrap;margin:0}</style></head>
    <body><h1>${escapeHtml(title)}</h1><p class="sub">${escapeHtml(subtitle)}</p><dl>${dl}</dl>${body}</body></html>`)
  win.document.close()
  win.focus()
  win.print()
}

/** A number input's v-model.number value: an emptied input gives '' and is sent as null */
export const num = (v: unknown) => (typeof v === 'number' && Number.isFinite(v) ? v : null)

/** Opens an agreement document in a new tab; the tab is opened before the download so the browser does not block it */
export async function viewDocument(documentId: number) {
  const tab = window.open('', '_blank')
  try {
    const url = URL.createObjectURL(await fctDocumentsApi.content(documentId))
    if (tab) tab.location.href = url
    else window.open(url, '_blank', 'noopener')
  } catch (e) {
    tab?.close()
    throw e
  }
}
