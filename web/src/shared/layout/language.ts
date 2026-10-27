// The language of the whole app. One switch in the header sets it for every module (the APEX copies keep their
// own keys: FINOVA_PA_LANG for Portfolio Acquisition, FINOVA_MFF_LANG for Mortgage Finance) and opens the page again.
export type Lang = 'EN' | 'AR'

const KEYS = ['CAP_LANG', 'FINOVA_PA_LANG', 'FINOVA_MFF_LANG']

export function currentLang(): Lang {
  try {
    return (localStorage.getItem('CAP_LANG') || localStorage.getItem('FINOVA_PA_LANG') || 'EN').toUpperCase() === 'AR' ? 'AR' : 'EN'
  } catch {
    return 'EN'
  }
}

export function switchLanguage(lang: Lang) {
  try {
    for (const k of KEYS) localStorage.setItem(k, lang)
  } catch {
    /* private window: the page still reloads in English */
  }
  location.reload()
}
