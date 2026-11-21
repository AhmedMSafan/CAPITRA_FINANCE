// One-off: screenshots of screening billing with sample data (mocked API), English and Arabic.
import { chromium } from 'playwright'
import fs from 'node:fs'

const BASE = 'http://localhost:5173'
const OUT = process.argv[2] ?? 'screens'
fs.mkdirSync(OUT, { recursive: true })

const PLANS = [
  { pricePlanId: 1, planCode: 'STANDARD', nameEn: 'Standard', nameAr: 'الخطة القياسية', userMonthlyFee: 100, inquiryFee: 2.5, currencyCode: 'EGP', isActive: true, notes: null, companyCount: 2, createdBy: 'admin', createdAt: '2026-11-20T20:00:00Z', updatedBy: null, updatedAt: null },
  { pricePlanId: 2, planCode: 'PREMIUM', nameEn: 'Premium', nameAr: 'الخطة المميزة', userMonthlyFee: 250, inquiryFee: 1.75, currencyCode: 'EGP', isActive: true, notes: null, companyCount: 1, createdBy: 'admin', createdAt: '2026-11-20T20:05:00Z', updatedBy: 'admin', updatedAt: '2026-11-21T09:00:00Z' },
  { pricePlanId: 3, planCode: 'PILOT-2025', nameEn: 'Pilot 2025', nameAr: 'الخطة التجريبية 2025', userMonthlyFee: 0, inquiryFee: 0, currencyCode: 'EGP', isActive: false, notes: null, companyCount: 0, createdBy: 'admin', createdAt: '2025-01-10T09:00:00Z', updatedBy: null, updatedAt: null }
]
const OVERVIEW = [
  { memberCompanyId: 1, companyCode: 'NFC', nameEn: 'National Finance Co', nameAr: 'الشركة الوطنية للتمويل', statusCode: 'ACTIVE', pricePlanId: 1, planCode: 'STANDARD', billingCycleCode: 'MONTHLY', currentFrom: '2026-11-01', currentTo: '2026-11-30', currentPeriodCode: '2026-11', activeUsersNow: 3, currentUserMonths: 3, currentInquiries: 138, currentAmount: 645, currentInvoiced: false, invoiceCount: 2, invoicedTotal: 1192.5, lastInvoicedTo: '2026-10-31' },
  { memberCompanyId: 2, companyCode: 'DELTA', nameEn: 'Delta Leasing', nameAr: 'دلتا للتأجير التمويلي', statusCode: 'ACTIVE', pricePlanId: 2, planCode: 'PREMIUM', billingCycleCode: 'QUARTERLY', currentFrom: '2026-10-01', currentTo: '2026-12-31', currentPeriodCode: '2026-Q4', activeUsersNow: 5, currentUserMonths: 14, currentInquiries: 410, currentAmount: 4217.5, currentInvoiced: false, invoiceCount: 1, invoicedTotal: 4500, lastInvoicedTo: '2026-09-30' },
  { memberCompanyId: 3, companyCode: 'LEGACY', nameEn: 'Old Credit Co', nameAr: 'شركة الائتمان القديمة', statusCode: 'ACTIVE', pricePlanId: null, planCode: null, billingCycleCode: 'MONTHLY', currentFrom: '2026-11-01', currentTo: '2026-11-30', currentPeriodCode: '2026-11', activeUsersNow: 1, currentUserMonths: 1, currentInquiries: 12, currentAmount: null, currentInvoiced: false, invoiceCount: 0, invoicedTotal: 0, lastInvoicedTo: null }
]
const lines = [
  { lineNo: 1, userName: 'nfc.admin', fullName: 'Mona Adel', monthsText: '2026-10', userMonths: 1, userAmount: 100, inquiryCount: 12, inquiryAmount: 30, lineAmount: 130 },
  { lineNo: 2, userName: 'nfc.kyc1', fullName: 'Omar Fathy', monthsText: '2026-10', userMonths: 1, userAmount: 100, inquiryCount: 151, inquiryAmount: 377.5, lineAmount: 477.5 },
  { lineNo: 3, userName: 'nfc.kyc2', fullName: 'Salma Nabil', monthsText: '2026-10', userMonths: 1, userAmount: 100, inquiryCount: 34, inquiryAmount: 85, lineAmount: 185 }
]
const inv = (id, no, period, from, to, status, total, extra = {}) => ({ invoiceId: id, invoiceNo: no, memberCompanyId: 1, companyCode: 'NFC', companyNameEn: 'National Finance Co', companyNameAr: 'الشركة الوطنية للتمويل',
  periodFrom: from, periodTo: to, periodCode: period, planCode: 'STANDARD', userMonthlyFee: 100, inquiryFee: 2.5, currencyCode: 'EGP', userMonths: 3, userAmount: 300, inquiryCount: 197, inquiryAmount: 492.5,
  totalAmount: total, statusCode: status, issuedAt: '2026-11-01T08:00:00Z', issuedBy: 'admin', cancelledAt: null, cancelledBy: null, cancelReason: null, notes: null, periodComplete: true, lines, ...extra })
const INVOICES = [
  inv(3, 'INV-2026-000003', '2026-10', '2026-10-01', '2026-10-31', 'ISSUED', 792.5),
  inv(2, 'INV-2026-000002', '2026-09', '2026-09-01', '2026-09-30', 'ISSUED', 400),
  inv(1, 'INV-2026-000001', '2026-09', '2026-09-01', '2026-09-30', 'CANCELLED', 380, { cancelledAt: '2026-10-02T10:00:00Z', cancelledBy: 'admin', cancelReason: 'Wrong price plan' })
]

const ROWS = [
  { signInId: 9, sessionId: 'a'.repeat(32), userName: 'nfc.kyc1', fullName: 'Omar Fathy', memberCompanyId: 1, companyCode: 'NFC', companyNameAr: 'الشركة الوطنية للتمويل', attemptedAt: '2026-11-21T09:12:00Z', resultCode: 'SUCCESS', failureText: null, ipAddress: '41.33.12.7', device: 'Chrome · Windows', userAgent: 'Mozilla/5.0', expiresAt: '2026-11-21T10:12:00Z', signedOutAt: null, sessionStateCode: 'OPEN', inquiryCount: 14, lastActivityAt: '2026-11-21T09:58:00Z' },
  { signInId: 8, sessionId: null, userName: 'nfc.kyc1', fullName: 'Omar Fathy', memberCompanyId: 1, companyCode: 'NFC', companyNameAr: 'الشركة الوطنية للتمويل', attemptedAt: '2026-11-21T09:11:00Z', resultCode: 'BAD_PASSWORD', failureText: 'Invalid user name or password.', ipAddress: '41.33.12.7', device: 'Chrome · Windows', userAgent: 'Mozilla/5.0', expiresAt: null, signedOutAt: null, sessionStateCode: null, inquiryCount: 0, lastActivityAt: null },
  { signInId: 7, sessionId: 'b'.repeat(32), userName: 'delta.admin', fullName: 'Ali Hassan', memberCompanyId: 2, companyCode: 'DELTA', companyNameAr: 'دلتا للتأجير التمويلي', attemptedAt: '2026-11-21T08:40:00Z', resultCode: 'SUCCESS', failureText: null, ipAddress: '156.204.3.90', device: 'Edge · Windows', userAgent: 'Mozilla/5.0', expiresAt: '2026-11-21T09:40:00Z', signedOutAt: '2026-11-21T09:05:00Z', sessionStateCode: 'SIGNED_OUT', inquiryCount: 3, lastActivityAt: '2026-11-21T09:01:00Z' },
  { signInId: 6, sessionId: null, userName: 'legacy.user', fullName: 'Legacy User', memberCompanyId: 3, companyCode: 'LEGACY', companyNameAr: 'شركة الائتمان القديمة', attemptedAt: '2026-11-21T08:30:00Z', resultCode: 'COMPANY_BLOCKED', failureText: "Your company's licence has ended. Contact the Federation to renew it.", ipAddress: '102.40.8.1', device: 'Safari · iOS', userAgent: 'Mozilla/5.0', expiresAt: null, signedOutAt: null, sessionStateCode: null, inquiryCount: 0, lastActivityAt: null },
  { signInId: 5, sessionId: null, userName: 'test123', fullName: null, memberCompanyId: null, companyCode: null, companyNameAr: null, attemptedAt: '2026-11-21T07:02:00Z', resultCode: 'UNKNOWN_USER', failureText: 'Invalid user name or password.', ipAddress: '185.220.101.4', device: 'curl', userAgent: 'curl/8.5.0', expiresAt: null, signedOutAt: null, sessionStateCode: null, inquiryCount: 0, lastActivityAt: null },
  { signInId: 4, sessionId: 'c'.repeat(32), userName: 'admin', fullName: 'System Administrator', memberCompanyId: null, companyCode: null, companyNameAr: null, attemptedAt: '2026-11-21T06:50:00Z', resultCode: 'SUCCESS', failureText: null, ipAddress: '10.0.0.5', device: 'Chrome · Windows', userAgent: 'Mozilla/5.0', expiresAt: '2026-11-21T07:50:00Z', signedOutAt: null, sessionStateCode: 'EXPIRED', inquiryCount: 0, lastActivityAt: null }
]
const INQ = [1, 2, 3, 4].map((n) => ({ inquiryId: n, inquiryRef: `SCR-2026-00010${n}`, receivedAt: `2026-11-21T09:${10 + n * 5}:0${n}.120Z`, respondedAt: `2026-11-21T09:${10 + n * 5}:0${n}.${200 + n * 17}Z`, durationMs: 80 + n * 17,
  subjectTypeCode: 'PERSON', subjectName: ['Mohamed Ali Hassan', 'Sara Mahmoud', 'Karim Adel', 'Nour El-Din Farouk'][n - 1], identityTypeCode: 'NATIONAL_ID', identityNo: `2900101123456${n}`, purposeCode: 'ONBOARDING', resultCode: n === 3 ? 'POSSIBLE_MATCH' : 'NO_MATCH', hitCount: n === 3 ? 1 : 0, registryVersionNo: 12 }))
function api(url) {
  const p = new URL(url).pathname
  if (p === '/api/scr/access-log') return { summary: { attempts: 6, successful: 3, refused: 3, users: 3, inquiries: 17 }, rows: ROWS }
  if (/\/api\/scr\/access-log\/\d+$/.test(p)) return { signIn: ROWS[0], inquiries: INQ }
  if (p === '/api/scr/companies') return [{ memberCompanyId: 1, companyCode: 'NFC', nameAr: 'الشركة الوطنية للتمويل' }, { memberCompanyId: 2, companyCode: 'DELTA', nameAr: 'دلتا' }]
  if (p.includes('/count')) return 2
  return []
}

const browser = await chromium.launch()
async function shot(lang, name, path, act) {
  const ctx = await browser.newContext({ viewport: { width: 1440, height: 900 }, deviceScaleFactor: 1 })
  await ctx.addInitScript((lang) => {
    sessionStorage.setItem('capitra.token', 'x')
    sessionStorage.setItem('capitra.expiresAt', new Date(Date.now() + 3600e3).toISOString())
    sessionStorage.setItem('capitra.user', JSON.stringify({ userId: '1', userName: 'admin', fullName: 'System Administrator', roles: ['ADMIN'], permissions: ['*'] }))
    localStorage.setItem('CAP_LANG', lang)
  }, lang)
  const page = await ctx.newPage()
  page.on('pageerror', (e) => console.log('PAGE ERROR', lang, name, String(e).slice(0, 300)))
  page.on('console', (m) => { if (m.type() === 'error') console.log('CONSOLE', lang, name, m.text().slice(0, 300)) })
  await page.route((u) => u.pathname.startsWith('/api/'), (r) => r.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(api(r.request().url())) }))
  await page.goto(BASE + path)
  await page.waitForTimeout(1500)
  if (act) { try { await act(page); await page.waitForTimeout(800) } catch (e) { console.log('ACT FAILED', lang, name, String(e).split('\n')[0]) } }
  await page.screenshot({ path: `${OUT}/${lang}-${name}.png`, fullPage: true })
  await ctx.close()
  console.log('shot', lang, name)
}
for (const lang of ['EN', 'AR']) {
  const t = (en, ar) => (lang === 'AR' ? ar : en)
  await shot(lang, 'l1-access-log', '/screening/access-log')
  await shot(lang, 'l2-session', '/screening/access-log', async (p) => { await p.getByRole('button', { name: t('Activity', 'النشاط') }).first().click({ timeout: 5000 }) })
  await shot(lang, 'l3-home', '/screening')
}
await browser.close()
