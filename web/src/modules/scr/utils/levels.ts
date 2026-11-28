import { tr } from '@/shared/layout/language'
import { formatDate } from './dates'
import type { ScrInquiry, ScrInquiryHit } from '../api/scr.api'

/** How one point of a Final List entry compares with what was entered: a mark (for the icon and colour) and the words. */
export type Level = { mark: 'yes' | 'near' | 'no' | 'none'; text: string }

/** Entries shown: "2 Final List entries found" / "No Final List entry found". No verdict: the member company decides. */
export function foundText(i: Pick<ScrInquiry, 'hits'>): string {
  const n = i.hits.length
  return n === 0 ? tr('No Final List entry found') : n === 1 ? tr('1 Final List entry found') : `${n} ${tr('Final List entries found')}`
}

/** Short label for lists. */
export function foundLabel(i: Pick<ScrInquiry, 'hits'>): string {
  return i.hits.length ? `${i.hits.length} ${tr('found')}` : tr('Not found')
}

const pct = (v: number) => `${Math.round(v)}%`

export function nameLevel(h: ScrInquiryHit): Level {
  const s = h.nameScorePct
  if (s >= 100) return { mark: 'yes', text: `${pct(s)} · ${tr('Exact')}` }
  if (s >= 90) return { mark: 'yes', text: `${pct(s)} · ${tr('High')}` }
  if (s >= 75) return { mark: 'near', text: `${pct(s)} · ${tr('Medium')}` }
  if (s > 0) return { mark: 'no', text: `${pct(s)} · ${tr('Low')}` }
  return { mark: 'no', text: `0% · ${tr('Different')}` }
}

export function idLevel(h: ScrInquiryHit): Level {
  // inquiries made before 23 Nov 2026 only say whether the identity number was the same
  const code = h.idMatchCode ?? (h.matchBasisCode === 'NAME_ONLY' ? 'DIFFERENT' : 'EXACT')
  const p = h.idScorePct ?? (code === 'EXACT' ? 100 : null)
  const head = p === null ? '' : `${pct(p)} · `
  if (code === 'EXACT') return { mark: 'yes', text: `${head}${tr('Exact')}` }
  if (code === 'CLOSE' || code === 'PARTIAL') {
    const d = h.idDigitsDifferent ?? 1
    return { mark: 'near', text: `${head}${d === 1 ? tr('1 character different') : `${d} ${tr('characters different')}`}` }
  }
  return { mark: 'no', text: `${head}${tr('Different')}` }
}

function sameLevel(code: string | null | undefined): Level {
  switch (code) {
    case 'SAME': return { mark: 'yes', text: tr('Same') }
    case 'DIFFERENT': return { mark: 'no', text: tr('Different') }
    case 'NOT_ON_LIST': return { mark: 'none', text: tr('Not on the list') }
    case 'NOT_GIVEN': return { mark: 'none', text: tr('Not given') }
    default: return { mark: 'none', text: '-' }
  }
}

export function birthLevel(i: ScrInquiry, h: ScrInquiryHit): Level {
  if (h.birthMatchCode) return sameLevel(h.birthMatchCode)
  return i.birthDate ? sameLevel(null) : sameLevel('NOT_GIVEN')
}

export function nationalityLevel(i: ScrInquiry, h: ScrInquiryHit): Level {
  if (h.nationalityMatchCode) return sameLevel(h.nationalityMatchCode)
  if (!i.nationalityCode) return sameLevel('NOT_GIVEN')
  if (!h.nationalityCode) return sameLevel('NOT_ON_LIST')
  return sameLevel(i.nationalityCode.toUpperCase() === h.nationalityCode.toUpperCase() ? 'SAME' : 'DIFFERENT')
}

export type CheckRow = { label: string; mine: string; listed: string; level: Level }

/** Every point of one entry: what was entered, what the Final List holds, and the level. */
export function checkRows(i: ScrInquiry, h: ScrInquiryHit, idLabel: string): CheckRow[] {
  const rows: CheckRow[] = [
    { label: idLabel, mine: i.identityNo, listed: h.identityNo || '-', level: idLevel(h) },
    { label: tr('Name'), mine: i.subjectName, listed: [h.nameAr, h.nameEn].filter(Boolean).join(' / '), level: nameLevel(h) }
  ]
  if (i.subjectTypeCode === 'PERSON')
    rows.push({ label: tr('Birth date'), mine: i.birthDate ? formatDate(i.birthDate) : tr('Not given'), listed: h.birthDate ? formatDate(h.birthDate) : '-', level: birthLevel(i, h) })
  rows.push({ label: tr('Nationality'), mine: i.nationalityCode || tr('Not given'), listed: h.nationalityCode || '-', level: nationalityLevel(i, h) })
  return rows
}

export const MARK_ICON: Record<Level['mark'], string> = { yes: 'fa-check', near: 'fa-adjust', no: 'fa-times', none: 'fa-minus' }
