// The language of the whole app. One switch in the header sets it for every module (the APEX copies keep their
// own keys: FINOVA_PA_LANG for Portfolio Acquisition, FINOVA_MFF_LANG for Mortgage Finance). The switch happens in
// place: the open page is drawn again in the new language, without reloading the browser.
import { ref } from 'vue'

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
  if (!rtl) document.body.classList.remove('mff-rtl', 'finova-rtl')
  language.value = lang
}
