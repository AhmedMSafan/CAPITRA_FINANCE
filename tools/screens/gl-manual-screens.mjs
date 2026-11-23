// One-off: screenshots of the Finova General Ledger manual and the Overview manual, English and Arabic, on a real test database
// (sample data from gl-sample-setup.sql and gl-sample-journals.py). Every "Capitra" on screen (name, logo) is shown as Finova.
import { chromium } from 'playwright'
import fs from 'node:fs'

const BASE = 'http://localhost:5173'
const API = 'http://localhost:5080'
const OUT = process.argv[2] ?? 'screens'
fs.mkdirSync(OUT, { recursive: true })
const MARK = 'data:image/svg+xml;base64,' + fs.readFileSync(new URL('./finova-mark.svg', import.meta.url)).toString('base64')

async function login(userName, password) {
  const r = await fetch(API + '/api/sec/auth/login', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify({ userName, password }) })
  return r.json()
}
const ADMIN = await login('admin', 'Admin#Seed2026')
const KARIM = await login('karim.gl', 'Karim#Final2026')
const ids = await (await fetch(API + '/api/apex/120/51/J51_SEARCH', { method: 'POST', headers: { 'Content-Type': 'application/json', Authorization: 'Bearer ' + ADMIN.token }, body: JSON.stringify({ x01: '{}' }) })).json()
console.log('J51', JSON.stringify(ids).slice(0, 1500))
const journals = (ids.rows ?? ids.journals ?? ids.data ?? [])
const find = (name) => journals.find((j) => (j.name ?? j.NAME ?? j.journalName ?? '') === name)
const idOf = (j) => j && (j.id ?? j.ID ?? j.journalHeaderId ?? j.journal_header_id)

const LOGO = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 150 40" role="img" aria-label="Finova"><rect x="1" y="2" width="36" height="36" rx="9" fill="#134E4A" stroke="rgba(255,255,255,.18)"/><path d="M14 30V12h11.5M14 21h8.5" fill="none" stroke="#E0B04A" stroke-width="4.2" stroke-linecap="round" stroke-linejoin="round"/><circle cx="26" cy="28" r="2.6" fill="#ffffff"/><text x="46" y="27" fill="#ffffff" font-family="Inter,'Segoe UI',Arial,sans-serif" font-size="19" font-weight="800" letter-spacing="1.5">FINOVA</text></svg>`

async function finova(page) {
  for (const f of page.frames()) if (f !== page.mainFrame()) await f.evaluate(() => {
    const swap = (s) => s.replace(/CAPITRA/g, 'FINOVA').replace(/Capitra/g, 'Finova').replace(/capitra/g, 'finova')
    const w = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT)
    for (let n = w.nextNode(); n; n = w.nextNode()) if (/capitra/i.test(n.nodeValue)) n.nodeValue = swap(n.nodeValue)
  }).catch(() => null)
  await page.evaluate((mark) => {
    const swap = (s) => s.replace(/CAPITRA/g, 'FINOVA').replace(/Capitra/g, 'Finova').replace(/capitra/g, 'finova').replace(/كابيترا/g, 'فينوفا')
    const w = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT)
    for (let n = w.nextNode(); n; n = w.nextNode()) if (/capitra|كابيترا/i.test(n.nodeValue)) n.nodeValue = swap(n.nodeValue)
    for (const el of document.querySelectorAll('[title],[placeholder],[aria-label],[alt]'))
      for (const a of ['title', 'placeholder', 'aria-label', 'alt']) { const v = el.getAttribute(a); if (v && /capitra|كابيترا/i.test(v)) el.setAttribute(a, swap(v)) }
    for (const el of document.querySelectorAll('input,textarea')) if (/capitra|كابيترا/i.test(el.value)) el.value = swap(el.value)
    for (const img of document.querySelectorAll('img')) if (/capitra/i.test(img.getAttribute('src') || '') || img.classList.contains('fx-logo')) img.src = mark
    document.title = swap(document.title)
  }, MARK)
  await page.waitForTimeout(250)
}

const browser = await chromium.launch()
async function shot(lang, name, path, act, opts = {}) {
  const who = opts.user === undefined ? ADMIN : opts.user
  const ctx = await browser.newContext({ viewport: { width: 1440, height: opts.height ?? 900 }, deviceScaleFactor: 1 })
  await ctx.addInitScript(([lang, s]) => {
    if (s) {
      sessionStorage.setItem('capitra.token', s.token)
      sessionStorage.setItem('capitra.expiresAt', s.expiresAt)
      sessionStorage.setItem('capitra.user', JSON.stringify(s.user))
    }
    localStorage.setItem('CAP_LANG', lang)
  }, [lang, who])
  const page = await ctx.newPage()
  page.on('pageerror', (e) => console.log('PAGE ERROR', lang, name, String(e).slice(0, 300)))
  page.on('console', (m) => { if (m.type() === 'error') console.log('CONSOLE', lang, name, m.text().slice(0, 300)) })
  // the API calls go to the test server
  await page.route((u) => u.pathname.endsWith('capitra-logo.svg'), (r) => r.fulfill({ status: 200, contentType: 'image/svg+xml', body: LOGO }))
  await page.route((u) => u.pathname.startsWith('/api/'), async (r) => {
    const u = new URL(r.request().url())
    try { const res = await r.fetch({ url: API + u.pathname + u.search }); await r.fulfill({ response: res }) } catch (e) { console.log('ROUTE', u.pathname, String(e).slice(0, 120)); await r.abort() }
  })
  await page.goto(BASE + path)
  await page.addStyleTag({ content: '#__vue-devtools-container__,[id^="__vue-devtools"],[class*="vue-devtools"]{display:none!important}' })
  await page.waitForTimeout(opts.wait ?? 2500)
  if (act) { try { await act(page); await page.waitForTimeout(1500) } catch (e) { console.log('ACT FAILED', lang, name, String(e).split('\n')[0]) } }
  await finova(page)
  await page.screenshot({ path: `${OUT}/${lang}-${name}.png`, fullPage: !opts.viewportOnly })
  await ctx.close()
  console.log('shot', lang, name)
}

const posted = idOf(find('Salaries, October'))
const waiting = idOf(find('Depreciation, November'))
const reversed = idOf(find('Office equipment, Alexandria'))
const draft = idOf(find('Consumer loans, November'))
console.log('ids', posted, waiting, reversed, draft)

for (const lang of ['EN', 'AR']) {
  // overview
  await shot(lang, 'o01-sign-in', '/login', null, { user: null, viewportOnly: true })
  await shot(lang, 'o02-home', '/')
  await shot(lang, 'o03-menu', '/', async (p) => { await p.locator('header button:has-text("Menu"), header button:has-text("القائمة")').first().click() }, { viewportOnly: true })
  // general ledger
  await shot(lang, 'g01-home', '/gl')
  await shot(lang, 'g02-structure', '/gl/account-structure')
  await shot(lang, 'g03-chart', '/gl/chart-of-accounts')
  await shot(lang, 'g04-chart-account', '/gl/chart-of-accounts', async (p) => { await p.getByText('Factoring receivables', { exact: false }).first().click() })
  await shot(lang, 'g05-periods', '/gl/accounting-periods')
  await shot(lang, 'g07-journals', '/gl/journals')
  await shot(lang, 'g08-journal-new', '/gl/journal-entry')
  if (posted) await shot(lang, 'g09-journal-posted', `/gl/journal-entry?p50_id=${posted}`)
  if (waiting) await shot(lang, 'g10-journal-approve', `/gl/journal-entry?p50_id=${waiting}`, null, { user: KARIM })
  if (reversed) await shot(lang, 'g11-journal-reversed', `/gl/journal-entry?p50_id=${reversed}`)
  if (draft) await shot(lang, 'g12-journal-draft', `/gl/journal-entry?p50_id=${draft}`)
  await shot(lang, 'g13-account-ledger', '/gl/account-ledger', async (p) => {
    await p.locator('input[placeholder*="110101"]').first().fill('110101')
    const from = p.locator('select').nth(0); const opts = await from.locator('option').allTextContents(); const oct = opts.find((o) => o.includes('Oct-2026')); if (oct) await from.selectOption({ label: oct })
    await p.locator('button:has-text("Search"), button:has-text("بحث")').first().click()
  })
  await shot(lang, 'g14-balances', '/gl/account-balances')
  for (const [n, tab] of [['g15-trial-balance', 'Trial balance'], ['g15b-balance-sheet', 'Balance sheet'], ['g15c-income', 'Income statement']])
    await shot(lang, n, '/gl/financial-statements', async (p) => {
      await p.getByText(tab, { exact: true }).first().click(); await p.waitForTimeout(600)
      for (const sel of await p.locator('select').all()) { const o = (await sel.locator('option').allTextContents()).find((t) => t.includes('Oct-2026')); if (o) { await sel.selectOption({ label: o }); break } }
      await p.waitForTimeout(800)
      await p.locator('button:has-text("Run statement")').first().click(); await p.waitForTimeout(2500)
    })
  await shot(lang, 'g16-help', '/gl/help', null, { viewportOnly: true })
  await shot(lang, 'g18-reference', '/gl/reference-data')
}
await browser.close()
