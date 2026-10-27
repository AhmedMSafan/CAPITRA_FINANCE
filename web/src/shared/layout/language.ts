// The language of the whole app. One switch in the header sets it for every module (the APEX copies keep their
// own keys: FINOVA_PA_LANG for Portfolio Acquisition, FINOVA_MFF_LANG for Mortgage Finance). The switch happens in
// place: the open page is drawn again in the new language, without reloading the browser.
import { ref } from 'vue'
import { AR, AR_STATUS } from './arabic'

export type Lang = 'EN' | 'AR'

const KEYS = ['CAP_LANG', 'FINOVA_PA_LANG', 'FINOVA_MFF_LANG']

function stored(): Lang {
  try {
    return (localStorage.getItem('CAP_LANG') || localStorage.getItem('FINOVA_PA_LANG') || 'EN').toUpperCase() === 'AR' ? 'AR' : 'EN'
  } catch {
    return 'EN'
  }
}

/** The language now; the app layout draws the open page again when it changes. */
export const language = ref<Lang>(stored())

export function currentLang(): Lang {
  return language.value
}

export function switchLanguage(lang: Lang) {
  try {
    for (const k of KEYS) localStorage.setItem(k, lang)
  } catch {
    /* private window: the language is not kept */
  }
  const rtl = lang === 'AR'
  document.documentElement.dir = rtl ? 'rtl' : 'ltr'
  document.documentElement.lang = rtl ? 'ar' : 'en'
  document.documentElement.classList.toggle('fnv-ar', rtl)
  if (!rtl) document.body.classList.remove('mff-rtl', 'finova-rtl')
  language.value = lang
}

/** The Arabic of an English text of the copied pages when the language is Arabic ("Open X" too, as finova-home-lang.js does). */
export function tr(en: string): string {
  if (language.value !== 'AR') return en
  const k = en.replace(/\s+/g, ' ').trim()
  if (Object.hasOwn(AR, k)) return AR[k]!
  const m = /^Open (.+)$/.exec(k)
  if (m && Object.hasOwn(AR, m[1]!)) return 'فتح ' + AR[m[1]!]
  return en
}

/** A status code in words: "In Progress", or its Arabic. */
export function trStatus(code?: string | null): string {
  const c = (code || '').toUpperCase()
  if (language.value === 'AR' && AR_STATUS[c]) return AR_STATUS[c]
  return c.toLowerCase().replaceAll('_', ' ').replace(/\b\w/g, (x) => x.toUpperCase())
}

/** Dates in the page language (Arabic with Latin digits, as the APEX pages show them). */
export const dateLocale = () => (language.value === 'AR' ? 'ar-EG-u-nu-latn' : 'en-GB')

// the page opens in the kept language
if (typeof document !== 'undefined' && language.value === 'AR') {
  document.documentElement.dir = 'rtl'
  document.documentElement.lang = 'ar'
  document.documentElement.classList.add('fnv-ar')
}
