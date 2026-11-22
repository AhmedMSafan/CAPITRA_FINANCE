import { currentLang } from '@/shared/layout/language'

/**
 * Opens the Screening user manual (served from public/manuals/screening) in a new tab, in the app's language, at a section:
 * 1 about, 3 sign-in, 4 letters, 5 decisions, 6 Final List, 7 companies, 8 portal, 9 billing, 10 End of Day,
 * 11 Communication Center, 12 sign-ins and audit, 13 API.
 */
export function openScreeningManual(section?: number) {
  const lang = currentLang() === 'AR' ? 'ar' : 'en'
  const hash = section === undefined ? lang : `${lang}-${section}`
  window.open(`${import.meta.env.BASE_URL}manuals/screening/screening-manual.html#${hash}`, '_blank', 'noopener')
}
