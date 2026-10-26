// App 110's UI runtime: mff-bilingual.js as the global page's region "MFF UI Runtime" (and "MFF Iteration 4 UI")
// redefines it. The language is kept in localStorage (FINOVA_MFF_LANG); Arabic turns the page right to left and each
// label shows its Arabic ("English / عربي" labels, else the dictionary). layoutForm lays a page's form regions out on
// a 12-column grid. Both run again for the nodes a page adds later (apexafterrefresh in APEX).
import type { Router } from 'vue-router'
import ui from './mffUi.json'

const D: Record<string, string> = ui.dict
const LAYOUTS = ui.layouts as unknown as Record<string, [string, [string, number][]][]>

export type Lang = 'EN' | 'AR'

export function getLang(): Lang {
  try {
    return (localStorage.getItem('FINOVA_MFF_LANG') || 'EN').toUpperCase() === 'AR' ? 'AR' : 'EN'
  } catch {
    return 'EN'
  }
}

function rawText(el: Element) {
  return (el.getAttribute('data-mff-raw') || el.textContent || '').trim()
}

function pair(raw: string): [string, string] {
  const m = raw.match(/^(.*)\s+\/\s+([^/]*[؀-ۿ].*)$/)
  if (m) return [m[1]!.trim(), m[2]!.trim()]
  return [raw, D[raw] || '']
}

const SELECTOR = '.t-Form-label,.t-Region-title,.t-Button-label,.a-IRR-headerLink,.a-TreeView-label,.mff-list-title h1,.mff-list-title p,.mff-list-tag,.mff-calc-box label'

function translate(root: ParentNode, rtl: boolean) {
  const all = root instanceof Element && root.matches(SELECTOR) ? [root, ...root.querySelectorAll(SELECTOR)] : [...root.querySelectorAll(SELECTOR)]
  for (const el of all) {
    if (!el.getAttribute('data-mff-raw')) el.setAttribute('data-mff-raw', (el.textContent || '').trim())
    const p = pair(rawText(el))
    const text = rtl && p[1] ? p[1] : p[0]
    if (el.textContent !== text) el.textContent = text
  }
}

function setDirection(rtl: boolean) {
  document.documentElement.dir = rtl ? 'rtl' : 'ltr'
  document.documentElement.lang = rtl ? 'ar' : 'en'
  document.body.classList.toggle('mff-rtl', rtl)
}

/** FINOVA_MFF.applyLanguage */
export function applyLanguage(lang: Lang = getLang(), root: ParentNode = document) {
  try {
    localStorage.setItem('FINOVA_MFF_LANG', lang)
  } catch {
    // the language is not kept
  }
  setDirection(lang === 'AR')
  translate(root, lang === 'AR')
}

/** FINOVA_MFF.setLanguage (the runtime's: no reload) */
export function setLanguage(lang: Lang) {
  applyLanguage(lang)
}

/** FINOVA_MFF.layoutForm: the region's items, in this order, on a 12-column grid with their spans. */
export function layoutForm(regionId: string, cfg: [string, number][]) {
  const r = document.getElementById(regionId)
  const body = r?.querySelector('.t-Region-body')
  if (!body) return
  let g = body.querySelector<HTMLElement>('.mff-runtime-grid')
  if (!g) {
    g = document.createElement('div')
    g.className = 'mff-runtime-grid'
    body.insertBefore(g, body.firstChild)
  }
  const cells = cfg.map(([item, span]) => [document.getElementById(item + '_CONTAINER'), span] as const).filter((x): x is readonly [HTMLElement, number] => !!x[0])
  // moved only when out of order, so the observer below sees no change the second time
  if (cells.some(([c], i) => g!.children[i] !== c)) for (const [c] of cells) g.appendChild(c)
  for (const [c, span] of cells) {
    c.classList.add('mff-runtime-field')
    c.style.gridColumn = 'span ' + span
    if (span >= 12) c.classList.add('mff-runtime-full')
  }
}

/** FINOVA_MFF.layoutCurrentPage */
export function layoutPage(page: number) {
  for (const [regionId, cfg] of LAYOUTS[String(page)] ?? []) layoutForm(regionId, cfg)
}

let stop: (() => void) | null = null

/** init(), and again for every node a page adds; runs while a page of the app is open. */
export function useMffUi(router: Router, page: () => number) {
  if (stop) return
  let busy = false
  const run = (root: ParentNode) => {
    if (busy) return
    busy = true
    try {
      layoutPage(page())
      applyLanguage(getLang(), root)
    } finally {
      busy = false
    }
  }
  run(document)
  const mo = new MutationObserver((ms) => {
    if (busy) return
    for (const m of ms) for (const n of m.addedNodes) if (n.nodeType === 1 && !(n as Element).classList.contains('mff-runtime-grid')) return run(document)
  })
  mo.observe(document.body, { childList: true, subtree: true })
  const remove = router.afterEach((to) => {
    if (String(to.name ?? '').startsWith('mff-')) return
    stop?.()
  })
  stop = () => {
    mo.disconnect()
    remove()
    setDirection(false)
    stop = null
  }
}
