// One-off: prints a manual's HTML to two A4 PDFs, English and Arabic (each section starts a new page).
// node print-manual.mjs <manual.html> <out-prefix>   e.g. ../docs/manuals/screening/screening-manual.html ../docs/manuals/screening/CAPITRA-Screening-Manual
import { chromium } from 'playwright'
import path from 'node:path'

const [html, prefix] = process.argv.slice(2)
const browser = await chromium.launch()
for (const lang of ['en', 'ar']) {
  const page = await browser.newPage({ viewport: { width: 1200, height: 900 } })
  await page.goto('file://' + path.resolve(html) + '#' + lang, { waitUntil: 'networkidle' })
  await page.evaluate(async () => {
    document.querySelectorAll('img[loading=lazy]').forEach((i) => i.removeAttribute('loading'))
    await Promise.all([...document.images].map((i) => (i.complete ? null : new Promise((r) => { i.onload = i.onerror = r }))))
    // every face of every font, all weights and character ranges, before printing
    await Promise.all([...document.fonts].map((f) => f.load().catch(() => null)))
    await document.fonts.ready
  })
  console.log(lang, await page.evaluate(() => [...document.fonts].filter((f) => f.status !== 'loaded').length + ' faces not loaded of ' + document.fonts.size))
  await page.waitForTimeout(1000)
  await page.emulateMedia({ media: 'print' })
  await page.pdf({ path: `${prefix}-${lang.toUpperCase()}.pdf`, format: 'A4', printBackground: true, margin: { top: '16mm', bottom: '16mm', left: '14mm', right: '14mm' } })
  console.log('printed', lang)
  await page.close()
}
await browser.close()
