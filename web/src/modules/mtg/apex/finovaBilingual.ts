// App 105 static file finova-portfolio-bilingual.js (FINOVA_PA): the English / Arabic switch. The language is kept in
// localStorage (FINOVA_PA_LANG); Arabic turns the page right to left and every label with a known Arabic text
// (its data-finova-en / data-finova-ar pair, an "English / عربي" label, or the dictionary) shows it. Pages that open
// later are translated as they are added. stop() returns the document to English, left to right, when the app is left.
import type { Router } from 'vue-router'
import dict from './finovaDict.json'

const D: Record<string, string> = dict

export type Lang = 'EN' | 'AR'

export function getLang(): Lang {
  try {
    return (localStorage.getItem('FINOVA_PA_LANG') || 'EN').toUpperCase() === 'AR' ? 'AR' : 'EN'
  } catch {
    return 'EN'
  }
}

function splitPair(s: string): [string, string] | null {
  if (!s) return null
  const a = s.search(/[؀-ۿ]/)
  if (a < 0) return null
  let sep = s.lastIndexOf(' / ', a)
  let n = 3
  if (sep < 0) {
    sep = s.lastIndexOf('/', a)
    n = 1
  }
  if (sep < 0) return null
  const en = s.substring(0, sep).trim()
  const ar = s.substring(sep + n).trim()
  return en && ar ? [en, ar] : null
}

function resolve(raw: string): [string, string] | null {
  if (!raw) return null
  if (Object.prototype.hasOwnProperty.call(D, raw)) return [raw, D[raw]!]
  return splitPair(raw)
}

/** A label in the language: the dictionary's Arabic, or the label. */
export function tr(s: string): string {
  const p = resolve(s)
  return p ? (getLang() === 'AR' ? p[1] || p[0] : p[0]) : s
}

function translateElement(el: Element, lang: Lang) {
  let en = el.getAttribute('data-finova-en')
  let ar = el.getAttribute('data-finova-ar')
  if (!en) {
    const p = resolve((el.textContent || '').trim())
    if (!p) return
    en = p[0]
    ar = p[1] || p[0]
    el.setAttribute('data-finova-en', en)
    el.setAttribute('data-finova-ar', ar)
  }
  if (!ar) ar = en
  el.textContent = lang === 'AR' ? ar : en
}

function translateAttributes(root: ParentNode, lang: Lang) {
  root.querySelectorAll('[title],[aria-label],[placeholder]').forEach((el) => {
    for (const a of ['title', 'aria-label', 'placeholder']) {
      const k = 'data-finova-' + a + '-en'
      const ka = 'data-finova-' + a + '-ar'
      let en = el.getAttribute(k)
      let ar = el.getAttribute(ka)
      if (!en) {
        const p = resolve((el.getAttribute(a) || '').trim())
        if (!p) continue
        en = p[0]
        ar = p[1] || p[0]
        el.setAttribute(k, en)
        el.setAttribute(ka, ar)
      }
      el.setAttribute(a, lang === 'AR' ? ar || en : en)
    }
  })
  root.querySelectorAll<HTMLInputElement>('input[type=button],input[type=submit],input[type=reset]').forEach((el) => {
    let en = el.getAttribute('data-finova-value-en')
    let ar = el.getAttribute('data-finova-value-ar')
    if (!en) {
      const p = resolve((el.value || '').trim())
      if (!p) return
      en = p[0]
      ar = p[1] || p[0]
      el.setAttribute('data-finova-value-en', en)
      el.setAttribute('data-finova-value-ar', ar)
    }
    el.value = lang === 'AR' ? ar || en : en
  })
}

const SELECTOR = [
  '.t-Form-label', '.t-Region-title', '.t-Button-label',
  '.a-IRR-headerLink', '.a-GV-headerLabel', '.t-Report-report th',
  '.a-TreeView-label', '.t-LinksList-label', '.a-Menu-label',
  '.t-Tabs-link', '.t-Breadcrumb-label', '.t-Breadcrumb-item a',
  '.a-CardView-title', '.a-CardView-subTitle', '.a-CardView-body',
  '.t-Alert-title', '.t-Alert-body', '.a-IRR-noDataMsg', '.a-GV-noDataMsg',
  '.apex-page-item-error', '.t-Form-error', '.t-Body-title h1',
  '.t-BreadcrumbRegion-titleText', '.t-Region-body p', '.t-Region-body h1',
  '.t-Region-body h2', '.t-Region-body h3', '.t-Region-body h4',
  '.fin-kpi-label', '.fin-stage-label', '.fin-card-header span', '.fin-card-link',
  '.fin-d-title h1', '.fin-d-title p', '.fin-d-right', '.fin-login-title',
  '.fin-login-subtitle', '.a-IRR-toolbar label', '.a-GV-toolbar label', '[data-finova-ar]'
].join(',')

function translatePage(lang: Lang, root: ParentNode) {
  const all = root instanceof Element && root.matches(SELECTOR) ? [root, ...root.querySelectorAll(SELECTOR)] : [...root.querySelectorAll(SELECTOR)]
  all.forEach((el) => {
    // containers of bilingual leaf spans are translated through their children
    if (!el.hasAttribute('data-finova-ar') && el.querySelector('[data-finova-ar]')) return
    translateElement(el, lang)
  })
  translateAttributes(root, lang)
  const p = resolve((document.title || '').trim())
  if (p) document.title = lang === 'AR' ? p[1] || p[0] : p[0]
}

/** Y/N flag columns in reports become small badges. */
export function decorateFlags(root: ParentNode = document) {
  root.querySelectorAll('td[headers$="_FLAG"]').forEach((td) => {
    if (td.querySelector('.fx-yn')) return
    const t = (td.textContent || '').trim().toUpperCase()
    if (t !== 'Y' && t !== 'N') return
    const ar = getLang() === 'AR'
    td.innerHTML = '<span class="fx-yn fx-yn--' + t.toLowerCase() + '">' + (t === 'Y' ? (ar ? 'نعم' : 'Yes') : ar ? 'لا' : 'No') + '</span>'
  })
}

function setDirection(lang: Lang) {
  const rtl = lang === 'AR'
  document.documentElement.lang = rtl ? 'ar' : 'en'
  document.documentElement.dir = rtl ? 'rtl' : 'ltr'
  // u-RTL is the Universal Theme's own right-to-left switch
  document.documentElement.classList.toggle('u-RTL', rtl)
  document.body.classList.toggle('finova-rtl', rtl)
}

export function applyLanguage(lang: Lang = getLang()) {
  try {
    localStorage.setItem('FINOVA_PA_LANG', lang)
  } catch {
    // the language is not kept
  }
  setDirection(lang)
  translatePage(lang, document)
}

/** FINOVA_PA.setLanguage: keep the language and open the page again. */
export function setLanguage(lang: Lang) {
  applyLanguage(lang)
  location.reload()
}

/** init(): the language now and for every node added later; the returned function stops it. */
export function startBilingual(): () => void {
  applyLanguage(getLang())
  decorateFlags(document)
  const mo = new MutationObserver((ms) => {
    const lang = getLang()
    ms.forEach((m) =>
      m.addedNodes.forEach((n) => {
        if (n.nodeType === 1) {
          translatePage(lang, n as Element)
          decorateFlags(n as Element)
        }
      })
    )
  })
  mo.observe(document.body, { childList: true, subtree: true })
  return () => {
    mo.disconnect()
    setDirection('EN')
  }
}

let stopBilingual: (() => void) | null = null

/** The switch runs while a page of the app is open: it starts with the first one and stops when another app opens. */
export function useBilingual(router: Router) {
  // already running: a page drawn again (after a language switch) takes the language now
  if (stopBilingual) {
    applyLanguage(getLang())
    return
  }
  stopBilingual = startBilingual()
  const remove = router.afterEach((to) => {
    if (String(to.name ?? '').startsWith('mtg-')) return
    stopBilingual?.()
    stopBilingual = null
    remove()
  })
}
