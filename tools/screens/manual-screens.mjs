// One-off: the screenshots of the Screening user manual, with sample data (mocked API), English and Arabic.
import { chromium } from 'playwright'
import fs from 'node:fs'

const BASE = 'http://localhost:5173'
const OUT = process.argv[2] ?? 'screens'
fs.mkdirSync(OUT, { recursive: true })

// ---------- sample data: National Finance Co (NFC) and the Final List ----------
const lic = (maxUsers, from, to, quota) => ({ licenceId: 1, maxUsers, validFrom: from, validTo: to, yearlyInquiryQuota: quota, notes: null, createdBy: 'admin', createdAt: '2026-01-01T08:00:00Z' })
const COMPANIES = [
  { memberCompanyId: 1, companyCode: 'NFC', nameEn: 'National Finance Co', nameAr: 'الشركة الوطنية للتمويل', licenceNo: 'FRA-1120', contactEmail: 'compliance@nfc.example', statusCode: 'ACTIVE', notes: null, userCount: 4, inquiryCount: 4812, createdBy: 'admin',
    licence: lic(4, '2026-01-01', '2026-12-31', 5000), seats: { adminUserName: 'nfc.admin', adminEmail: 'mona@nfc.example', adminStatusCode: 'ACTIVE', usersInUse: 3 }, licenceStateCode: 'ACTIVE', pricePlanId: 1, pricePlanCode: 'STANDARD', billingCycleCode: 'MONTHLY' },
  { memberCompanyId: 2, companyCode: 'DELTA', nameEn: 'Delta Leasing', nameAr: 'دلتا للتأجير التمويلي', licenceNo: 'FRA-2045', contactEmail: 'kyc@delta.example', statusCode: 'ACTIVE', notes: null, userCount: 6, inquiryCount: 1410, createdBy: 'admin',
    licence: lic(6, '2026-04-01', '2027-03-31', 10000), seats: { adminUserName: 'delta.admin', adminEmail: 'ali@delta.example', adminStatusCode: 'ACTIVE', usersInUse: 5 }, licenceStateCode: 'ACTIVE', pricePlanId: 2, pricePlanCode: 'PREMIUM', billingCycleCode: 'QUARTERLY' },
  { memberCompanyId: 3, companyCode: 'MISR', nameEn: 'Misr Microfinance', nameAr: 'مصر للتمويل متناهي الصغر', licenceNo: null, contactEmail: 'ops@misr-mf.example', statusCode: 'SUSPENDED', notes: null, userCount: 2, inquiryCount: 96, createdBy: 'admin',
    licence: lic(2, '2025-12-01', '2026-11-30', null), seats: { adminUserName: 'misr.admin', adminEmail: 'hany@misr-mf.example', adminStatusCode: 'ACTIVE', usersInUse: 1 }, licenceStateCode: 'ACTIVE', pricePlanId: 1, pricePlanCode: 'STANDARD', billingCycleCode: 'YEARLY' }
]
const USERS = [
  { userId: 'u1', userName: 'nfc.admin', email: 'mona@nfc.example', fullName: 'Mona Adel', isActive: true, roleCode: 'MEMBER_COMPANY_ADMIN', statusCode: 'ACTIVE', lastLoginAt: '2026-11-22T07:02:00Z', twoFactorEnabled: true },
  { userId: 'u2', userName: 'nfc.kyc1', email: 'omar@nfc.example', fullName: 'Omar Fathy', isActive: true, roleCode: 'MEMBER_COMPANY_USER', statusCode: 'ACTIVE', lastLoginAt: '2026-11-22T07:10:00Z', twoFactorEnabled: true },
  { userId: 'u3', userName: 'nfc.kyc2', email: 'salma@nfc.example', fullName: 'Salma Nabil', isActive: true, roleCode: 'MEMBER_COMPANY_USER', statusCode: 'ACTIVE', lastLoginAt: '2026-11-21T12:30:00Z', twoFactorEnabled: true },
  { userId: 'u4', userName: 'nfc.kyc3', email: 'karim@nfc.example', fullName: 'Karim Samir', isActive: true, roleCode: 'MEMBER_COMPANY_USER', statusCode: 'PENDING_ACTIVATION', lastLoginAt: null, twoFactorEnabled: false }
]
const party = (id, type, ar, en, idType, idNo, nat, birth, aliases = []) => ({ partyId: id, partyTypeCode: type, nameAr: ar, nameEn: en, identityTypeCode: idType, identityNo: idNo, nationalityCode: nat, birthDate: birth, notes: null, aliases, createdBy: 'hoda.entry' })
const P = [
  party(1, 'PERSON', 'محمود حسن علي', 'Mahmoud Hassan Ali', 'NATIONAL_ID', '28506151234567', 'EG', '1985-06-15', ['Mahmoud Hasan Aly']),
  party(2, 'PERSON', 'سامي فؤاد عبد الله', 'Samy Fouad Abdallah', 'PASSPORT', 'A12345678', 'EG', '1979-02-03'),
  party(3, 'COMPANY', 'شركة النجم للتجارة', 'Al Nagm Trading', 'COMMERCIAL_REGISTER', '145236', 'EG', null),
  party(4, 'PERSON', 'خالد يوسف منصور', 'Khaled Youssef Mansour', 'NATIONAL_ID', '27011020101234', 'EG', '1970-11-02'),
  party(5, 'PERSON', 'رانيا عادل', 'Rania Adel', 'NATIONAL_ID', '29203151299876', 'EG', '1992-03-15')
]
const done = (key, code, name, action, by, at) => ({ key, stepCode: code, stepName: name, action, doneBy: by, doneAt: at })
const dec = (id, ref, no, type, status, scope, eff, parties, extra = {}) => ({ decisionId: id, decisionRef: ref, officialNo: no, decisionTypeCode: type, parentDecisionId: null, parentDecisionRef: null, letterId: 3, letterNo: 'LTR-2026-000003',
  scopeText: scope, effectiveDate: eff, statusCode: status, returnReason: null, workflowInstanceId: null, enteredBy: 'hoda.entry', reviewedBy: null, reviewedAt: null, approvedBy: null, publishedAt: null, registryVersionNo: null,
  parties, createdAt: eff + 'T09:00:00Z', workflowStepName: null, completedSteps: [], workflowStepHolder: null, ...extra })
const DECISIONS = [
  dec(34, 'DEC-000034', 'CRT-551/2026', 'LISTING', 'DRAFT', 'Ban on dealing: final court judgment', '2026-11-21', [P[4]], { letterId: 4, letterNo: 'LTR-2026-000004' }),
  dec(33, 'DEC-000033', 'FRZ-118/2026', 'LISTING', 'IN_REVIEW', 'Freeze of all assets and accounts', '2026-11-20', [P[3]], { workflowInstanceId: 91, workflowStepName: 'Review', workflowStepHolder: 'tarek.review' }),
  dec(32, 'DEC-000032', 'FRZ-112/2026', 'SUSPENSION', 'AWAITING_APPROVAL', 'Freeze suspended pending appeal', '2026-11-18', [P[1]], { parentDecisionId: 12, parentDecisionRef: 'DEC-000012', workflowInstanceId: 90, workflowStepName: 'Approval', workflowStepHolder: 'nadia.approve', reviewedBy: 'tarek.review', reviewedAt: '2026-11-19T10:00:00Z',
    completedSteps: [done('1', 'SCR_REVIEW', 'Review', 'ACCEPT', 'tarek.review', '2026-11-19T10:00:00Z')] }),
  dec(31, 'DEC-000031', 'FRZ-106/2026', 'LISTING', 'PUBLISHED', 'Freeze of all assets and accounts', '2026-03-02', [P[0], P[2]], { workflowInstanceId: 70, reviewedBy: 'tarek.review', approvedBy: 'nadia.approve', publishedAt: '2026-03-03T12:00:00Z', registryVersionNo: 14 }),
  dec(30, 'DEC-000030', 'CRT-402/2026', 'LIFTING', 'PUBLISHED', 'Ban lifted by the Court of Appeal', '2026-02-10', [party(6, 'PERSON', 'أيمن سعيد', 'Ayman Saeed', 'NATIONAL_ID', '28802021212121', 'EG', '1988-02-02')], { workflowInstanceId: 66, reviewedBy: 'tarek.review', approvedBy: 'nadia.approve', publishedAt: '2026-02-11T12:00:00Z', registryVersionNo: 13 }),
  dec(29, 'DEC-000029', 'FRZ-090/2026', 'LISTING', 'RETURNED', 'Freeze of accounts', '2026-01-25', [P[1]], { returnReason: 'The passport number does not match the letter. Please check page 2.' })
]
const STEPS = [
  { key: 'e', round: 1, stepCode: 'DATA_ENTRY', stepName: 'Data entry', status: 'DONE', action: 'SUBMIT', doneBy: 'hoda.entry', doneAt: '2026-11-18T09:20:00Z', comment: null, stepInstanceId: 1, documents: [{ documentId: 1, stepCode: 'DATA_ENTRY', stepName: 'Data entry', documentTitle: 'Court letter', fileName: 'FRZ-112-2026.pdf', mimeType: 'application/pdf', fileSizeBytes: 482113, createdBy: 'hoda.entry', createdAt: '2026-11-18T09:15:00Z', canRemove: false }] },
  { key: 'r', round: 1, stepCode: 'SCR_REVIEW', stepName: 'Review', status: 'DONE', action: 'ACCEPT', doneBy: 'tarek.review', doneAt: '2026-11-19T10:00:00Z', comment: 'Checked against the letter and the passport copy.', stepInstanceId: 2, documents: [] },
  { key: 'a', round: 1, stepCode: 'SCR_APPROVAL', stepName: 'Approval', status: 'OPEN', action: null, doneBy: null, doneAt: null, comment: null, stepInstanceId: 3, documents: [] }
]
const TASK = { inboxId: 5, taskId: 5, instanceId: 90, stepInstanceId: 3, status: 'IN_PROGRESS', assignmentType: 'ROLE', assignedRole: 'SCR_APPROVER', claimedBy: 'nadia.approve', isClaimedByMe: true, subject: 'DEC-000032 Suspension', allowedActions: 'ACCEPT,REJECT',
  createdOn: '2026-11-19T10:00:00Z', claimedOn: '2026-11-19T11:00:00Z', workflowCode: 'SCR_DECISION_APPROVAL', workflowName: 'Screening decision approval', stepCode: 'SCR_APPROVAL', stepName: 'Approval', moduleCode: 'SCR', entityType: 'SCR_DECISION', entityId: 32, entityRef: 'DEC-000032' }
const LETTERS = [
  { letterId: 4, letterNo: 'LTR-2026-000004', senderReference: 'CAI-EC/551', senderName: 'Cairo Economic Court', channelCode: 'COURIER', letterDate: '2026-11-19', receivedAt: '2026-11-21T08:30:00Z', statusCode: 'RECEIVED', notes: null, decisionCount: 1, createdBy: 'hoda.entry',
    files: [{ letterFileId: 7, fileName: 'CRT-551-2026.pdf', mimeType: 'application/pdf', fileSizeBytes: 612034, sha256Hash: '3b9f1c27aa04e5d6b8c9d0e1f2a3b4c5d6e7f8091a2b3c4d5e6f708192a3b4c5' }] },
  { letterId: 3, letterNo: 'LTR-2026-000003', senderReference: 'PP/FRZ/112', senderName: 'Public Prosecution', channelCode: 'EMAIL', letterDate: '2026-11-16', receivedAt: '2026-11-17T10:05:00Z', statusCode: 'INDEXED', notes: null, decisionCount: 3, createdBy: 'hoda.entry',
    files: [{ letterFileId: 5, fileName: 'FRZ-112-2026.pdf', mimeType: 'application/pdf', fileSizeBytes: 482113, sha256Hash: 'a1b2c3d4e5f60718293a4b5c6d7e8f9001122334455667788990aabbccddeeff' }, { letterFileId: 6, fileName: 'annex-1.png', mimeType: 'image/png', fileSizeBytes: 210554, sha256Hash: 'ff00ee11dd22cc33bb44aa5599668877665544332211009988776655443322aa' }] },
  { letterId: 2, letterNo: 'LTR-2026-000002', senderReference: 'MLU-2026-090', senderName: 'Money Laundering Unit', channelCode: 'HAND', letterDate: '2026-01-22', receivedAt: '2026-01-24T09:00:00Z', statusCode: 'CLOSED', notes: null, decisionCount: 1, createdBy: 'hoda.entry', files: [] }
]
const entry = (p, d, status, from, lifted = null) => ({ partyId: p.partyId, decisionId: d.id, decisionRef: d.ref, officialNo: d.no, partyTypeCode: p.partyTypeCode, nameAr: p.nameAr, nameEn: p.nameEn, aliases: p.aliases, identityTypeCode: p.identityTypeCode, identityNo: p.identityNo,
  nationalityCode: p.nationalityCode, birthDate: p.birthDate, scopeText: d.scope, listedFrom: from, liftedOn: lifted, statusCode: status, letterId: 3, lastDecisionRef: d.last ?? d.ref, enteredBy: 'hoda.entry' })
const REGISTRY = { versionNo: 14, publishedAt: '2026-11-03T12:00:00Z', activeCount: 3, suspendedCount: 1, liftedCount: 1, pendingCount: 2, entries: [
  entry(P[0], { id: 31, ref: 'DEC-000031', no: 'FRZ-106/2026', scope: 'Freeze of all assets and accounts' }, 'ACTIVE', '2026-03-02'),
  entry(P[2], { id: 31, ref: 'DEC-000031', no: 'FRZ-106/2026', scope: 'Freeze of all assets and accounts' }, 'ACTIVE', '2026-03-02'),
  entry(P[3], { id: 20, ref: 'DEC-000020', no: 'CRT-210/2025', scope: 'Ban on dealing: final court judgment' }, 'ACTIVE', '2025-09-14'),
  entry(P[1], { id: 12, ref: 'DEC-000012', no: 'FRZ-044/2025', scope: 'Freeze of accounts', last: 'DEC-000026' }, 'SUSPENDED', '2025-05-20'),
  entry({ partyId: 6, partyTypeCode: 'PERSON', nameAr: 'أيمن سعيد', nameEn: 'Ayman Saeed', aliases: [], identityTypeCode: 'NATIONAL_ID', identityNo: '28802021212121', nationalityCode: 'EG', birthDate: '1988-02-02' }, { id: 9, ref: 'DEC-000009', no: 'CRT-101/2024', scope: 'Ban on dealing', last: 'DEC-000030' }, 'LIFTED', '2024-06-01', '2026-02-10')
] }
const INQ = { inquiryId: 7, inquiryRef: 'SCR-2026-004812', companyCode: 'NFC', requestedBy: 'nfc.kyc1', subjectTypeCode: 'PERSON', subjectName: 'Mahmoud Hassan Ali', identityTypeCode: 'NATIONAL_ID', identityNo: '28506151234567', birthDate: '1985-06-15', nationalityCode: 'EG', purposeCode: 'ONBOARDING', purposeText: null, caseReference: 'CB-2026-4471', resultCode: 'MATCH', registryVersionNo: 14, screenedAt: '2026-11-22T08:55:12Z',
  hits: [{ matchBasisCode: 'ID_AND_NAME', nameScorePct: 100, matchedName: 'Mahmoud Hassan Ali', entryStatusCode: 'ACTIVE', partyTypeCode: 'PERSON', nameAr: 'محمود حسن علي', nameEn: 'Mahmoud Hassan Ali', identityTypeCode: 'NATIONAL_ID', identityNo: '28506151234567', nationalityCode: 'EG', decisionRef: 'DEC-000031', officialNo: 'FRZ-106/2026', scopeText: 'Freeze of all assets and accounts', listedFrom: '2026-03-02', liftedOn: null }],
  channelCode: 'PORTAL', receivedAt: '2026-11-22T08:55:12Z', respondedAt: '2026-11-22T08:55:12Z', recordHash: '9f2c4e1a7b3d5f60a8c2e4f6071b3d5f7a9c1e3f5b7d9e1f3a5c7e9f1b3d5f7a', memberCompanyId: 1, companyNameEn: 'National Finance Co', companyNameAr: 'الشركة الوطنية للتمويل' }
const iq = (id, ref, co, by, name, idNo, result, at, purpose = 'ONBOARDING', channel = 'PORTAL') => ({ ...INQ, inquiryId: id, inquiryRef: ref, companyCode: co, requestedBy: by, subjectName: name, identityNo: idNo, resultCode: result, screenedAt: at, purposeCode: purpose, channelCode: channel, caseReference: null, hits: result === 'NO_MATCH' ? [] : INQ.hits })
const HISTORY = [
  INQ,
  iq(6, 'SCR-2026-004811', 'NFC', 'nfc.kyc1', 'Rana Mostafa', '29507011234561', 'NO_MATCH', '2026-11-22T08:41:00Z'),
  iq(5, 'SCR-2026-004805', 'NFC', 'nfc.kyc1', 'Samy Fouad', 'A99887766', 'POSSIBLE_MATCH', '2026-11-22T08:12:00Z', 'TRANSACTION'),
  iq(4, 'SCR-2026-004790', 'NFC', 'nfc.kyc1', 'Ayman Saeed', '28802021212121', 'PREVIOUSLY_LISTED', '2026-11-21T14:20:00Z', 'PERIODIC_REVIEW'),
  iq(3, 'SCR-2026-004771', 'NFC', 'nfc.kyc1', 'Hossam Kamel', '28001011111111', 'NO_MATCH', '2026-11-21T11:03:00Z')
]
const ALL_INQ = [INQ, iq(9, 'SCR-2026-004810', 'DELTA', 'api:scr_delta_9x2k', 'Nermeen Ali', '29001011555555', 'NO_MATCH', '2026-11-22T08:40:00Z', 'ONBOARDING', 'API'), ...HISTORY.slice(1), iq(8, 'SCR-2026-004760', 'DELTA', 'delta.kyc2', 'Al Nagm Trading', '145236', 'MATCH', '2026-11-21T10:30:00Z', 'TRANSACTION')]
const SEAL = { inquiryRef: INQ.inquiryRef, sealed: true, valid: true, recordHash: INQ.recordHash, prevHash: 'ab', message: 'The seal is intact: the inquiry is as it was made.', checkedAt: '2026-11-22T09:30:00Z' }
const CLIENTS = [
  { apiClientId: 1, memberCompanyId: 1, clientId: 'scr_nfc_7k2m9q4x', clientName: 'Core banking onboarding', secretHint: 'h3k9', secretCreatedAt: '2026-11-01T08:00:00Z', statusCode: 'ACTIVE', revokedAt: null, revokedBy: null, lastTokenAt: '2026-11-22T08:55:00Z', lastIpAddress: '41.33.10.7', inquiryCount: 634, notes: null, createdBy: 'admin', createdAt: '2026-11-01T08:00:00Z' },
  { apiClientId: 2, memberCompanyId: 1, clientId: 'scr_nfc_p8w3r2ta', clientName: 'Old CRM (test)', secretHint: 'q2x7', secretCreatedAt: '2026-10-01T08:00:00Z', statusCode: 'REVOKED', revokedAt: '2026-11-05T10:00:00Z', revokedBy: 'admin', lastTokenAt: '2026-11-04T16:20:00Z', lastIpAddress: '41.33.10.9', inquiryCount: 17, notes: null, createdBy: 'admin', createdAt: '2026-10-01T08:00:00Z' }
]
const PLANS = [
  { pricePlanId: 1, planCode: 'STANDARD', nameEn: 'Standard', nameAr: 'الخطة القياسية', userMonthlyFee: 100, inquiryFee: 2.5, freeInquiries: 50, tiers: [], currencyCode: 'EGP', isActive: true, notes: null, companyCount: 2, createdBy: 'admin', createdAt: '2026-11-20T20:00:00Z', updatedBy: null, updatedAt: null },
  { pricePlanId: 2, planCode: 'PREMIUM', nameEn: 'Premium', nameAr: 'الخطة المميزة', userMonthlyFee: 250, inquiryFee: 2, freeInquiries: 0, tiers: [{ fromInquiry: 1, toInquiry: 1000, unitPrice: 2 }, { fromInquiry: 1001, toInquiry: 5000, unitPrice: 1.5 }, { fromInquiry: 5001, toInquiry: null, unitPrice: 1 }], currencyCode: 'EGP', isActive: true, notes: null, companyCount: 1, createdBy: 'admin', createdAt: '2026-11-20T20:05:00Z', updatedBy: 'admin', updatedAt: '2026-11-21T09:00:00Z' }
]
const OVERVIEW = [
  { memberCompanyId: 1, companyCode: 'NFC', nameEn: 'National Finance Co', nameAr: 'الشركة الوطنية للتمويل', statusCode: 'ACTIVE', pricePlanId: 1, planCode: 'STANDARD', billingCycleCode: 'MONTHLY', currentFrom: '2026-11-01', currentTo: '2026-11-30', currentPeriodCode: '2026-11', activeUsersNow: 4, currentUserMonths: 4, currentInquiries: 538, currentAmount: 1620, currentInvoiced: false, invoiceCount: 10, invoicedTotal: 16420, lastInvoicedTo: '2026-10-31' },
  { memberCompanyId: 2, companyCode: 'DELTA', nameEn: 'Delta Leasing', nameAr: 'دلتا للتأجير التمويلي', statusCode: 'ACTIVE', pricePlanId: 2, planCode: 'PREMIUM', billingCycleCode: 'QUARTERLY', currentFrom: '2026-10-01', currentTo: '2026-12-31', currentPeriodCode: '2026-Q4', activeUsersNow: 6, currentUserMonths: 12, currentInquiries: 1410, currentAmount: 5615, currentInvoiced: false, invoiceCount: 2, invoicedTotal: 9850, lastInvoicedTo: '2026-09-30' },
  { memberCompanyId: 3, companyCode: 'MISR', nameEn: 'Misr Microfinance', nameAr: 'مصر للتمويل متناهي الصغر', statusCode: 'SUSPENDED', pricePlanId: 1, planCode: 'STANDARD', billingCycleCode: 'YEARLY', currentFrom: '2026-01-01', currentTo: '2026-12-31', currentPeriodCode: '2026', activeUsersNow: 2, currentUserMonths: 22, currentInquiries: 96, currentAmount: 2315, currentInvoiced: false, invoiceCount: 0, invoicedTotal: 0, lastInvoicedTo: null }
]
const lines = [
  { lineNo: 1, userName: 'nfc.admin', fullName: 'Mona Adel', monthsText: '2026-10', userMonths: 1, userAmount: 100, inquiryCount: 41 },
  { lineNo: 2, userName: 'nfc.kyc1', fullName: 'Omar Fathy', monthsText: '2026-10', userMonths: 1, userAmount: 100, inquiryCount: 402 },
  { lineNo: 3, userName: 'nfc.kyc2', fullName: 'Salma Nabil', monthsText: '2026-10', userMonths: 1, userAmount: 100, inquiryCount: 174 },
  { lineNo: 4, userName: 'api:scr_nfc_7k2m9q4x', fullName: 'Core banking onboarding', monthsText: null, userMonths: 0, userAmount: 0, inquiryCount: 160 }
]
const INVOICE = { invoiceId: 5, invoiceNo: 'INV-2026-000005', memberCompanyId: 1, companyCode: 'NFC', companyNameEn: 'National Finance Co', companyNameAr: 'الشركة الوطنية للتمويل', periodFrom: '2026-10-01', periodTo: '2026-10-31', periodCode: '2026-10',
  planCode: 'STANDARD', userMonthlyFee: 100, inquiryFee: 2.5, freeInquiries: 50, currencyCode: 'EGP', userMonths: 3, userAmount: 300, inquiryCount: 777, inquiryAmount: 1817.5, totalAmount: 2117.5, statusCode: 'ISSUED', issuedAt: '2026-10-31T21:00:00Z', issuedBy: 'EOD',
  cancelledAt: null, cancelledBy: null, cancelReason: null, notes: null, periodComplete: true, lines,
  bands: [{ bandNo: 1, bandTypeCode: 'FREE', fromInquiry: 1, toInquiry: 50, unitPrice: 0, quantity: 50, amount: 0 }, { bandNo: 2, bandTypeCode: 'FLAT', fromInquiry: 51, toInquiry: null, unitPrice: 2.5, quantity: 727, amount: 1817.5 }],
  accountingDate: '2026-10-31', glTranSetId: 'CAP-SCR-2026-000041', cancelGlTranSetId: null, paidAmount: 1000, balanceAmount: 1117.5, paymentStatusCode: 'PARTLY_PAID',
  payments: [{ paymentId: 1, paymentDate: '2026-11-10', paymentAmount: 1000, bankAccountId: 1, bankAccountText: 'National Bank of Egypt · 1203-445566-01 · EGP', referenceNo: 'NBE-TRF-88412', notes: null, glTranSetId: 'CAP-SCR-2026-000047', createdBy: 'fatma.fin', createdAt: '2026-11-10T11:00:00Z' }] }
const pinv = (id, no, period, from, to, total, paid, status) => ({ ...INVOICE, invoiceId: id, invoiceNo: no, periodCode: period, periodFrom: from, periodTo: to, totalAmount: total, paidAmount: paid, balanceAmount: total - paid, paymentStatusCode: status })
const INVOICES = [INVOICE, pinv(4, 'INV-2026-000004', '2026-09', '2026-09-01', '2026-09-30', 1530, 1530, 'PAID'), pinv(3, 'INV-2026-000003', '2026-08', '2026-08-01', '2026-08-31', 1612.5, 1612.5, 'PAID')]
const si = (id, user, name, co, at, result, n, fail = null, ip = '41.33.10.7') => ({ signInId: id, sessionId: result === 'SUCCESS' ? 's' + id : null, userName: user, fullName: name, memberCompanyId: co ? 1 : null, companyCode: co, companyNameAr: co ? 'الشركة الوطنية للتمويل' : null, attemptedAt: at, resultCode: result, failureText: fail,
  ipAddress: ip, device: 'Chrome · Windows', userAgent: 'Mozilla/5.0', expiresAt: null, signedOutAt: null, sessionStateCode: result === 'SUCCESS' ? 'OPEN' : null, inquiryCount: n, lastActivityAt: null, secondStepCode: result === 'SUCCESS' && co ? 'AUTHENTICATOR' : null })
const SIGNINS = { summary: { attempts: 7, successful: 4, refused: 3, users: 5, inquiries: 33 }, rows: [
  si(44, 'nfc.kyc1', 'Omar Fathy', 'NFC', '2026-11-22T07:10:00Z', 'SUCCESS', 12),
  si(43, 'nfc.kyc2', 'Salma Nabil', 'NFC', '2026-11-22T06:58:00Z', 'BAD_CODE', 0, 'The code is not right.'),
  si(42, 'nfc.admin', 'Mona Adel', 'NFC', '2026-11-22T07:02:00Z', 'SUCCESS', 9),
  si(41, 'misr.user', 'Hany Lotfy', 'MISR', '2026-11-22T06:40:00Z', 'COMPANY_BLOCKED', 0, 'Your company is suspended. Contact the Federation.', '102.40.8.1'),
  si(40, 'test123', null, null, '2026-11-22T05:02:00Z', 'UNKNOWN_USER', 0, 'Invalid user name or password.', '185.220.101.4'),
  si(39, 'admin', 'System Administrator', null, '2026-11-22T06:30:00Z', 'SUCCESS', 0, null, '10.0.0.5'),
  si(38, 'nfc.kyc1', 'Omar Fathy', 'NFC', '2026-11-21T08:40:00Z', 'SUCCESS', 12)
] }
const SERVERS = [
  { mailServerId: 1, serverName: 'Gmail (testing)', hostName: 'smtp.gmail.com', portNo: 587, isSslEnabled: true, userName: 'capitra.test@gmail.com', hasPassword: true, passwordUnreadable: false, fromAddress: 'capitra.test@gmail.com', fromName: 'CAPITRA', isActive: true, lastTestAt: '2026-11-20T17:40:00Z', lastTestStatusCode: 'SENT', lastTestError: null, createdBy: 'admin', createdAt: '2026-11-20T17:30:00Z', updatedBy: null, updatedAt: null },
  { mailServerId: 2, serverName: 'Federation Exchange', hostName: 'mail.federation.local', portNo: 587, isSslEnabled: true, userName: 'noreply@federation.local', hasPassword: true, passwordUnreadable: false, fromAddress: 'noreply@federation.local', fromName: 'CAPITRA', isActive: false, lastTestAt: '2026-11-20T17:35:00Z', lastTestStatusCode: 'FAILED', lastTestError: '5.7.8 Username and Password not accepted.', createdBy: 'admin', createdAt: '2026-11-20T17:20:00Z', updatedBy: null, updatedAt: null }
]
const LOG = [
  { emailLogId: 9, toAddress: 'compliance@nfc.example', kindCode: 'INVOICE', subject: 'CAPITRA — invoice INV-2026-000005 / فاتورة', statusCode: 'SENT', errorText: null, attemptedAt: '2026-10-31T21:00:05Z', serverName: 'Gmail (testing)', userName: null, createdBy: 'EOD' },
  { emailLogId: 8, toAddress: 'hany@misr-mf.example', kindCode: 'LICENCE_EXPIRY', subject: 'CAPITRA — your licence ends in 7 days / ينتهي ترخيصكم', statusCode: 'SENT', errorText: null, attemptedAt: '2026-11-23T21:00:03Z', serverName: 'Gmail (testing)', userName: 'misr.admin', createdBy: 'EOD' },
  { emailLogId: 7, toAddress: 'karim@nfc.example', kindCode: 'ACTIVATION', subject: 'CAPITRA — activate your account / تفعيل حسابك (National Finance Co)', statusCode: 'SENT', errorText: null, attemptedAt: '2026-11-21T18:02:00Z', serverName: 'Gmail (testing)', userName: 'nfc.kyc3', createdBy: 'nfc.admin' },
  { emailLogId: 6, toAddress: 'omar@nfc.example', kindCode: 'PASSWORD_RESET', subject: 'CAPITRA — reset your password / إعادة تعيين كلمة المرور', statusCode: 'SENT', errorText: null, attemptedAt: '2026-11-20T17:55:00Z', serverName: 'Gmail (testing)', userName: 'nfc.kyc1', createdBy: 'SYSTEM' },
  { emailLogId: 5, toAddress: 'capitra.test@gmail.com', kindCode: 'TEST', subject: 'CAPITRA — test e-mail / رسالة تجريبية', statusCode: 'FAILED', errorText: '5.7.8 Username and Password not accepted.', attemptedAt: '2026-11-20T17:35:00Z', serverName: 'Federation Exchange', userName: null, createdBy: 'admin' }
]
// End of Day
const JOBS = [
  { jobCode: 'LND_DAILY', moduleCode: 'LND', nameEn: 'Retail Lending: daily accrual and arrears', nameAr: 'التمويل الاستهلاكي: الاستحقاق اليومي والمتأخرات', descriptionEn: 'Interest accrues, late fees and non-accrual, GL rule LND_EOD.', descriptionAr: 'الاستحقاق اليومي وغرامات التأخير وعدم الاستحقاق، قاعدة القيد LND_EOD.', sequenceNo: 10, isEnabled: true, isAvailable: true, notes: null, parameters: [] },
  { jobCode: 'SCR_LICENCE_EXPIRY', moduleCode: 'SCR', nameEn: 'Screening: licence expiry notices', nameAr: 'الفحص: تنبيهات انتهاء الترخيص', descriptionEn: 'E-mails each member company\'s administrator and contact e-mail the set number of days before its licence ends, and on its last day.', descriptionAr: 'يرسل بريدًا إلى مسؤول كل شركة عضو وبريد التواصل الخاص بها قبل انتهاء الترخيص بعدد الأيام المحدد، وفي آخر يوم له.', sequenceNo: 20, isEnabled: true, isAvailable: true, notes: null,
    parameters: [{ code: 'REMINDER_DAYS', labelEn: 'Reminder days before the end', labelAr: 'أيام التذكير قبل الانتهاء', helpEn: 'Days before the licence\'s last day, separated by commas, e.g. 30,7,1.', helpAr: 'عدد الأيام قبل آخر يوم في الترخيص، مفصولة بفواصل، مثل 30,7,1.', defaultValue: '30,7,1', value: null }] },
  { jobCode: 'SCR_INVOICES', moduleCode: 'SCR', nameEn: 'Screening: period-end invoices', nameAr: 'الفحص: فواتير نهاية الفترة', descriptionEn: 'On the last day of each member company\'s invoice period, issues and posts the period\'s invoice from its price plan.', descriptionAr: 'في آخر يوم من فترة فوترة كل شركة عضو تُصدر فاتورة الفترة وتُرحّل حسب خطة الأسعار.', sequenceNo: 30, isEnabled: true, isAvailable: true, notes: null,
    parameters: [{ code: 'ISSUE_ZERO_INVOICES', labelEn: 'Issue invoices of zero', labelAr: 'إصدار الفواتير الصفرية', helpEn: 'Y: a period with nothing to charge still gets an invoice of 0.00. N: it is skipped.', helpAr: 'Y: تصدر فاتورة بقيمة 0.00. N: لا تصدر.', defaultValue: 'N', value: null }] },
  { jobCode: 'SCR_HOUSEKEEPING', moduleCode: 'SCR', nameEn: 'Screening: sign-in housekeeping', nameAr: 'الفحص: تنظيم سجل الدخول', descriptionEn: 'Sign-ins left waiting for the authenticator code are closed as \'Code not entered\'; expired activation links are counted.', descriptionAr: 'تُغلق محاولات الدخول التي بقيت بانتظار رمز التطبيق، وتُحصى روابط التفعيل المنتهية.', sequenceNo: 40, isEnabled: true, isAvailable: true, notes: null, parameters: [] }
]
const dj = (code, status, items, summary, error = null, attempts = 1) => { const j = JOBS.find((x) => x.jobCode === code)
  return { jobCode: code, moduleCode: j.moduleCode, nameEn: j.nameEn, nameAr: j.nameAr, sequenceNo: j.sequenceNo, statusCode: status, itemsCount: items, summary, errorMessage: error, attempts, startedAt: '2026-11-21T21:00:00Z', finishedAt: '2026-11-21T21:00:04Z', runBy: 'admin' } }
const DAYS = [
  { dayId: 4, businessDate: '2026-11-21', statusCode: 'OPEN', startedAt: '2026-11-21T21:00:00Z', finishedAt: null, runBy: 'admin', jobs: [
    dj('LND_DAILY', 'DONE', 42, '42 finances: interest accrued 18,240.55.'),
    dj('SCR_LICENCE_EXPIRY', 'FAILED', 0, null, 'No mail server is in use: set one up in Communication Center > Mail servers.', 1),
    dj('SCR_INVOICES', 'DONE', 0, '0 invoice(s) issued; 0 already invoiced; 0 with nothing to charge.'),
    dj('SCR_HOUSEKEEPING', 'DONE', 1, '1 sign-in(s) left waiting for the code closed; 0 company account(s) not activated and their link expired.') ] },
  { dayId: 3, businessDate: '2026-10-31', statusCode: 'DONE', startedAt: '2026-10-31T21:00:00Z', finishedAt: '2026-10-31T21:00:09Z', runBy: 'admin', jobs: [
    dj('LND_DAILY', 'DONE', 42, '42 finances: interest accrued 18,231.10. GL: CAP-LND-2026-000118.'),
    dj('SCR_LICENCE_EXPIRY', 'DONE', 0, '0 compan(ies) with a licence ending in 30/7/1/0 day(s): 0 e-mail(s) sent.'),
    dj('SCR_INVOICES', 'DONE', 1, '1 invoice(s) issued (NFC INV-2026-000005); 0 already invoiced; 0 with nothing to charge.'),
    dj('SCR_HOUSEKEEPING', 'DONE', 0, '0 sign-in(s) left waiting for the code closed.') ] }
]
const EOD = { today: '2026-11-22', lastDoneDate: '2026-11-20', nextDate: '2026-11-21', isNextDayOpen: true, canRun: true, blocked: null, days: DAYS, jobs: JOBS }
const QR = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAMYAAADGAQAAAACh4MLwAAABoElEQVR4Xu1YSW7DMAwcRQZyVH6Qpyg/K/oz+Sl5QAH5WIDGFENmubSXXsJDCENw4suA5Cx2IX6v/fDHA+D9BACnzqqzo9FaNLJbiu50UnjMr3q9VFKAU2BDKRc0HifqXBcAW9GZA5tqK4a1lI/9dKfA67GpQ1FFx1rt/tfrsQkBOdBYZyeH+tZoKWb64Gm3gJSIp2DUFMjKcftJJtAQcHZJh0iqe803hO71+6ZlI4e5ylXSmlqnmxzY5AvCMzjVMTRRIwW22eVQTgRzapCjziSeJTzHa3FLRTlh9yuJvoUX7GeGskmO20iATRoSwcOHi+pnjhwSukFDD3GzdlO4HFzwplEDdTGBR7gMXHBfkPCKrcHZypFF3xxYDDSo6vCSzLSHkyr6tiHn8kqwbwe0z/1MlxEx9PuEZSvYLkk0JEqa5tMMC0vQt2d+i4TpW5dp39zoFZPUNEUmS6IhN91QOpICz4i+OfTtjs1fGeI9Okm2fCTItc51Pw9OJaUcPFXooGePvfhAgeVrtSVDRnp+D1GwjK1zMUmgIeX9jetfLvMDum5C2yveIqYAAAAASUVORK5CYII='
const MY_COMPANY = { memberCompanyId: 1, companyCode: 'NFC', nameEn: 'National Finance Co', nameAr: 'الشركة الوطنية للتمويل', maxUsers: 4, quota: { quota: 5000, used: 4812, left: 188, from: '2026-01-01', to: '2026-12-31' } }

function api(url, method) {
  const p = new URL(url).pathname
  if (p === '/api/sec/auth/second-step/setup') return { qrCode: QR, sharedKey: 'JBSW Y3DP EHPK 3PXP JBSW Y3DP EHPK 3PXP', otpAuthUri: 'otpauth://totp/CAPITRA:x?secret=JBSWY3DPEHPK3PXP' }
  if (p === '/api/sec/auth/second-step/enable') return { login: { token: 'x', expiresAt: new Date(Date.now() + 3600e3).toISOString(), user: { userId: '2', userName: 'nfc.kyc1', fullName: 'Omar Fathy', roles: [], permissions: [] } }, recoveryCodes: ['k2m4p-7qx9t', 'b8n3r-2wz6y', 'h5c9d-4fj7l', 't6v2x-9mq3s', 'p4r8w-1kz5n', 'd7g2h-6bx4q', 'm3s9t-8cv1r', 'x5j7k-2nd8w', 'q9w4e-5ty6u', 'z1a3s-7df2g'], recoveryCodesLeft: 10 }
  if (p === '/api/scr/registry') return REGISTRY
  if (p === '/api/scr/letters') return LETTERS
  if (p === '/api/scr/parties') return P
  if (p === '/api/scr/decisions') return new URL(url).searchParams.get('status') === 'PUBLISHED' ? DECISIONS.filter((d) => d.statusCode === 'PUBLISHED') : DECISIONS
  const dm = p.match(/^\/api\/scr\/decisions\/(\d+)$/); if (dm) return DECISIONS.find((d) => d.decisionId === Number(dm[1])) ?? DECISIONS[0]
  if (/^\/api\/scr\/decisions\/\d+\/steps$/.test(p)) return STEPS
  if (p === '/api/wfe/inbox/5') return TASK
  if (p === '/api/scr/companies') return COMPANIES
  if (/^\/api\/scr\/companies\/\d+\/users$/.test(p)) return USERS
  if (/^\/api\/scr\/companies\/\d+\/licences$/.test(p)) return [COMPANIES[0].licence]
  if (/^\/api\/scr\/companies\/\d+\/api-clients$/.test(p)) return CLIENTS
  if (p === '/api/scr/inquiries' && method === 'POST') return INQ
  if (p === '/api/scr/inquiries') return ALL_INQ
  if (p === '/api/scr/inquiries/mine') return HISTORY
  if (p === '/api/scr/inquiries/my-company') return MY_COMPANY
  if (p === '/api/scr/inquiries/7') return INQ
  if (p === '/api/scr/inquiries/7/seal') return SEAL
  if (p === '/api/scr/portal/invoices') return INVOICES
  if (p === '/api/scr/portal/invoices/5') return INVOICE
  if (p === '/api/scr/portal/sign-ins') return { ...SIGNINS, rows: SIGNINS.rows.filter((r) => r.companyCode === 'NFC') }
  if (p === '/api/scr/portal/sign-ins/44') return { signIn: SIGNINS.rows[0], inquiries: HISTORY.slice(0, 3).map((h) => ({ inquiryId: h.inquiryId, inquiryRef: h.inquiryRef, receivedAt: h.screenedAt, respondedAt: h.screenedAt, durationMs: 64, subjectTypeCode: 'PERSON', subjectName: h.subjectName, identityTypeCode: 'NATIONAL_ID', identityNo: h.identityNo, purposeCode: h.purposeCode, resultCode: h.resultCode, hitCount: h.hits.length, registryVersionNo: 14 })) }
  if (p === '/api/scr/access-log') return SIGNINS
  if (p === '/api/scr/access-log/44') return { signIn: SIGNINS.rows[0], inquiries: [] }
  if (p === '/api/scr/billing/plans') return PLANS
  if (p === '/api/scr/billing/overview') return OVERVIEW
  if (p === '/api/scr/billing/invoices') return INVOICES
  if (/^\/api\/scr\/billing\/invoices\/\d+$/.test(p)) return INVOICE
  if (p === '/api/scr/billing/bank-accounts') return [{ bankAccountId: 1, text: 'National Bank of Egypt · 1203-445566-01 · EGP', glAccountCode: '110201' }]
  if (/^\/api\/scr\/billing\/companies\/\d+\/preview$/.test(p)) return { ...INVOICE, invoiceId: 0, invoiceNo: null, statusCode: 'PREVIEW', periodFrom: '2026-11-01', periodTo: '2026-11-30', periodCode: '2026-11', periodComplete: false, payments: [], paidAmount: 0, balanceAmount: 0, glTranSetId: null }
  if (p === '/api/eod') return EOD
  if (p === '/api/eod/jobs') return JOBS
  if (p === '/api/cmc/mail-servers/status') return { sourceCode: 'COMMUNICATION_CENTER', serverName: 'Gmail (testing)', hostName: 'smtp.gmail.com', fromAddress: 'capitra.test@gmail.com', lastTestAt: '2026-11-20T17:40:00Z', lastTestStatusCode: 'SENT' }
  if (p === '/api/cmc/mail-servers') return SERVERS
  if (p === '/api/cmc/email-log') return LOG
  if (p.includes('/count')) return 2
  return []
}

const FED = { userId: '1', userName: 'admin', fullName: 'System Administrator', roles: ['ADMIN'], permissions: ['*'] }
const APPROVER = { userId: '9', userName: 'nadia.approve', fullName: 'Nadia Samir', roles: ['SCR_APPROVER'], permissions: ['SCR.DECISION.VIEW', 'SCR.DECISION.APPROVE'] }
const CO_USER = { userId: '2', userName: 'nfc.kyc1', fullName: 'Omar Fathy', roles: ['MEMBER_COMPANY_USER'], permissions: ['SCR.INQUIRY.CREATE'] }
const CO_ADMIN = { userId: '3', userName: 'nfc.admin', fullName: 'Mona Adel', roles: ['MEMBER_COMPANY_ADMIN'], permissions: ['SCR.INQUIRY.CREATE', 'SCR.COMPANY.USERS'] }

const browser = await chromium.launch()
async function shot(lang, name, path, act, opts = {}) {
  const ctx = await browser.newContext({ viewport: { width: opts.width ?? 1440, height: 900 }, deviceScaleFactor: 1 })
  await ctx.addInitScript(([lang, user, kind]) => {
    if (kind) sessionStorage.setItem('capitra.secondStep', JSON.stringify({ kind, challenge: 'x', userName: 'nfc.kyc1', fullName: 'Omar Fathy', expiresInSeconds: 300 }))
    else if (user) {
      sessionStorage.setItem('capitra.token', 'x')
      sessionStorage.setItem('capitra.expiresAt', new Date(Date.now() + 3600e3).toISOString())
      sessionStorage.setItem('capitra.user', JSON.stringify(user))
    }
    localStorage.setItem('CAP_LANG', lang)
  }, [lang, opts.user === undefined ? FED : opts.user, opts.secondStep ?? null])
  const page = await ctx.newPage()
  page.on('pageerror', (e) => console.log('PAGE ERROR', lang, name, String(e).slice(0, 300)))
  page.on('console', (m) => { if (m.type() === 'error') console.log('CONSOLE', lang, name, m.text().slice(0, 300)) })
  await page.route((u) => u.pathname.startsWith('/api/'), (r) => r.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(api(r.request().url(), r.request().method())) }))
  await page.goto(BASE + path)
  await page.addStyleTag({ content: '#__vue-devtools-container__,[id^="__vue-devtools"],[class*="vue-devtools"]{display:none!important}' })
  await page.waitForTimeout(1500)
  if (act) { try { await act(page); await page.waitForTimeout(900) } catch (e) { console.log('ACT FAILED', lang, name, String(e).split('\n')[0]) } }
  await page.screenshot({ path: `${OUT}/${lang}-${name}.png`, fullPage: !opts.viewportOnly })
  await ctx.close()
  console.log('shot', lang, name)
}

for (const lang of ['EN', 'AR']) {
  const t = (en, ar) => (lang === 'AR' ? ar : en)
  const btn = (en, ar, n = 0) => async (p) => { await p.getByRole('button', { name: t(en, ar) }).nth(n).click({ timeout: 5000 }) }
  // Federation
  await shot(lang, 'm01-home', '/screening')
  await shot(lang, 'm02-letters', '/screening/letters')
  await shot(lang, 'm03-new-letter', '/screening/letters', btn('New letter', 'خطاب جديد'))
  await shot(lang, 'm04-decisions', '/screening/decisions')
  await shot(lang, 'm05-decision-draft', '/screening/decisions/34')
  await shot(lang, 'm06-decision-approve', '/screening/decisions/32?inboxId=5', null, { user: APPROVER })
  await shot(lang, 'm07-registry', '/screening/registry')
  await shot(lang, 'm08-companies', '/screening/companies')
  await shot(lang, 'm09-new-company', '/screening/companies', btn('New company', 'شركة جديدة'))
  await shot(lang, 'm10-licence', '/screening/companies', async (p) => { await p.locator('button:has(.fa-id-card-o)').first().click() })
  await shot(lang, 'm11-users', '/screening/companies', btn('Users', 'المستخدمون'))
  await shot(lang, 'm12-api-clients', '/screening/companies', async (p) => { await p.locator('button:has(.fa-plug)').first().click() })
  // sign-in and the portal
  await shot(lang, 'm13-2fa-setup', '/sign-in/code', null, { secondStep: 'SETUP', user: null })
  await shot(lang, 'm14-backup-codes', '/sign-in/code', async (p) => { await p.locator('input.cp-code').fill('123456'); await p.locator('button[type=submit]').click() }, { secondStep: 'SETUP', user: null })
  await shot(lang, 'm15-2fa-code', '/sign-in/code', null, { secondStep: 'CODE', user: null })
  await shot(lang, 'm16-backup-code', '/sign-in/code', btn('No phone? Use a backup code', 'لا يوجد هاتف؟ استخدم رمزًا احتياطيًا'), { secondStep: 'CODE', user: null })
  await shot(lang, 'm17-portal-form', '/screening/portal', async (p) => {
    await p.locator('input.fx-search').nth(0).fill('Mahmoud Hassan Ali'); await p.locator('input.fx-search').nth(1).fill('28506151234567')
    await p.locator('input.fx-search').nth(2).fill('EG'); await p.locator('input[type=date]').fill('1985-06-15'); await p.locator('input.fx-search').last().fill('CB-2026-4471')
  }, { user: CO_USER })
  await shot(lang, 'm18-portal-result', '/screening/portal', async (p) => {
    await p.locator('input.fx-search').nth(0).fill('Mahmoud Hassan Ali'); await p.locator('input.fx-search').nth(1).fill('28506151234567')
    await p.locator('button.fx-btn--primary').last().click()
  }, { user: CO_USER })
  await shot(lang, 'm19-my-inquiries', '/screening/portal', async (p) => { await p.locator('.fc-tab:has(.fa-history)').click() }, { user: CO_USER })
  await shot(lang, 'm20-certificate', '/screening/inquiries/7/certificate', null, { user: CO_USER })
  await shot(lang, 'm21-portal-users', '/screening/portal', async (p) => { await p.locator('.fc-tab:has(.fa-users)').click() }, { user: CO_ADMIN })
  await shot(lang, 'm22-portal-invoices', '/screening/portal?tab=invoices', async (p) => { await p.locator('.fc-tab:has(.fa-file-text-o)').click() }, { user: CO_ADMIN })
  await shot(lang, 'm23-portal-signins', '/screening/portal', async (p) => { await p.locator('.fc-tab:has(.fa-sign-in)').click(); await p.waitForTimeout(800); await p.locator('tr.scr-portal-click').first().click() }, { user: CO_ADMIN })
  // billing
  await shot(lang, 'm24-billing', '/screening/billing')
  await shot(lang, 'm25-invoice', '/screening/billing/invoices/5')
  await shot(lang, 'm26-payment', '/screening/billing/invoices/5', btn('Record payment', 'تسجيل سداد'))
  await shot(lang, 'm27-price-plans', '/screening/billing/plans')
  await shot(lang, 'm28-plan-tiers', '/screening/billing/plans', btn('Edit', 'تعديل', 1))
  // End of Day, e-mail, audit
  await shot(lang, 'm29-eod', '/eod', async (p) => { await p.locator('tr.eod-day').first().click() })
  await shot(lang, 'm30-eod-run', '/eod', async (p) => { await p.locator('.fx-actions .fx-btn--teal').click(); await p.locator('input[value=upto]').check() })
  await shot(lang, 'm31-eod-jobs', '/eod/jobs')
  await shot(lang, 'm32-mail-servers', '/communication/mail-servers')
  await shot(lang, 'm33-email-log', '/communication/email-log')
  await shot(lang, 'm34-access-log', '/screening/access-log')
  await shot(lang, 'm35-inquiries', '/screening/inquiries')
}
await browser.close()
