// Small helpers shared by the Treasury pages: compact money, bar widths, chip classes, and the save wrapper that shows pop-ups.
import { ApiError } from '@/shared/api/http'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { currentLang, tr } from '@/shared/layout/language'

/** 12.5M, 750K, 900: the compact figures of the limits tree. */
export function money(n: number | null | undefined): string {
  if (n === null || n === undefined) return '—'
  const a = Math.abs(n)
  if (a >= 1e9) return `${(n / 1e9).toLocaleString('en-US', { maximumFractionDigits: 2 })}B`
  if (a >= 1e6) return `${(n / 1e6).toLocaleString('en-US', { maximumFractionDigits: 1 })}M`
  if (a >= 1e3) return `${(n / 1e3).toLocaleString('en-US', { maximumFractionDigits: 0 })}K`
  return n.toLocaleString('en-US', { maximumFractionDigits: 0 })
}

/** A share of a whole as a bar width (0 to 100). */
export const pct = (part: number, whole: number) => (whole > 0 ? Math.min(100, Math.max(0, (part / whole) * 100)) : 0)

export function statusChip(code: string): string {
  if (code === 'ACTIVE') return 'fx-chip--ok'
  if (code === 'DRAFT' || code === 'TERM_SHEET' || code === 'PENDING') return 'fx-chip--warn'
  if (code === 'CLOSED' || code === 'INACTIVE' || code === 'EXPIRED') return 'fx-chip--plain'
  return 'fx-chip--bad'
}

export const label = (code: string | null | undefined) =>
  (code ?? '').toLowerCase().split('_').filter(Boolean).map((w) => w.charAt(0).toUpperCase() + w.slice(1)).join(' ')

/** Runs a save: on success a pop-up and onDone; on failure a pop-up with the server's messages. Returns true when it worked. */
export async function save(action: () => Promise<unknown>, done: string, onDone: () => void = () => {}): Promise<boolean> {
  try {
    await action()
    onDone()
    showSuccess(tr(done))
    return true
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    else showError(err instanceof Error ? err.message : String(err))
    return false
  }
}

/** A number input's value, or null when empty. */
export const num = (v: string | number | null | undefined) => (v === '' || v === null || v === undefined || Number.isNaN(Number(v)) ? null : Number(v))

/** Opens the Treasury user manual (served from public/manuals) in a new tab, in the app's language, at a section (e.g. 5 or 6). */
export function openManual(section?: number) {
  const lang = currentLang() === 'AR' ? 'ar' : 'en'
  const hash = section === undefined ? lang : `${lang}-${section}`
  window.open(`${import.meta.env.BASE_URL}manuals/treasury/treasury-manual.html#${hash}`, '_blank', 'noopener')
}
