// One-off: the screenshots of the Finova Factoring user manual, with sample data (mocked API), English and Arabic.
// The manual is for Finova: every "Capitra" on screen (name, logo) is shown as Finova. The application itself is not changed.
import { chromium } from 'playwright'
import fs from 'node:fs'

const BASE = 'http://localhost:5173'
const OUT = process.argv[2] ?? 'screens'
fs.mkdirSync(OUT, { recursive: true })
const MARK = 'data:image/svg+xml;base64,' + fs.readFileSync(new URL('./finova-mark.svg', import.meta.url)).toString('base64')

// ---------- sample data: Delta Food Industries, agreement APP-17 ----------
const CID = 124
const CUSTOMERS = [
  { clientId: 124, clientNumber: 'CL-000124', clientName: 'Delta Food Industries', clientType: 'CORPORATE', phoneNumber: '+20 2 2735 4410', email: 'finance@deltafood.example', isActive: true },
  { clientId: 125, clientNumber: 'CL-000125', clientName: 'Nile Packaging Co', clientType: 'CORPORATE', phoneNumber: '+20 2 3345 1180', email: 'ap@nilepack.example', isActive: true },
  { clientId: 126, clientNumber: 'CL-000126', clientName: 'Cairo Pharma Distribution', clientType: 'SME', phoneNumber: '+20 2 2290 7731', email: 'cfo@cpd.example', isActive: true },
  { clientId: 127, clientNumber: 'CL-000127', clientName: 'Alexandria Steel Works', clientType: 'CORPORATE', phoneNumber: '+20 3 4870 2210', email: 'treasury@alexsteel.example', isActive: true },
  { clientId: 128, clientNumber: 'CL-000128', clientName: 'Sinai Agro Exports', clientType: 'SME', phoneNumber: '+20 69 360 1122', email: 'info@sinaiagro.example', isActive: false },
  { clientId: 129, clientNumber: 'CL-000129', clientName: 'Giza Retail Group', clientType: 'CORPORATE', phoneNumber: '+20 2 3571 9900', email: 'finance@gizaretail.example', isActive: true }
]
const CUSTOMER = { clientId: CID, clientNumber: 'CL-000124', clientName: 'Delta Food Industries', clientNameAr: 'دلتا للصناعات الغذائية', clientType: 'CORPORATE', countryName: 'Egypt', currencyCode: 'EGP',
  relationshipManager: 'Karim Fawzy', phoneNumber: '+20 2 2735 4410', email: 'finance@deltafood.example' }
const APPLICATIONS = [
  { offerId: 33, offerRef: 'OFF-20261120-4417', offerAt: '2026-11-20', requestedLimit: 4000000, currencyCode: 'EGP', factoringType: 'CLASSIC - Buyer', statusCode: 'ACCEPTED', agreementNo: null, workflowStepName: null, track: null },
  { offerId: 32, offerRef: 'OFF-20261118-0952', offerAt: '2026-11-18', requestedLimit: 2500000, currencyCode: 'EGP', factoringType: 'CLASSIC - Supplier', statusCode: 'DRAFT', agreementNo: null, workflowStepName: null, track: null },
  { offerId: 31, offerRef: 'OFF-20261102-7731', offerAt: '2026-11-02', requestedLimit: 10000000, currencyCode: 'EGP', factoringType: 'CLASSIC - Supplier', statusCode: 'APPLICATION', agreementNo: 'APP-17', workflowStepName: null, track: 'NORMAL' },
  { offerId: 28, offerRef: 'OFF-20261015-2210', offerAt: '2026-10-15', requestedLimit: 1500000, currencyCode: 'EGP', factoringType: 'CLASSIC - Supplier', statusCode: 'APPLICATION', agreementNo: 'APP-18', workflowStepName: 'Approval', track: 'FAST' }
]
const AGREEMENTS = [
  { agreementId: 17, agreementNumber: 'APP-17', applicationAt: '2026-11-02', facilityLimit: 10000000, currencyCode: 'EGP', factoringType: 'CLASSIC - Supplier', statusCode: 'DRAFT', workflowStepName: null, track: 'NORMAL' },
  { agreementId: 18, agreementNumber: 'APP-18', applicationAt: '2026-10-15', facilityLimit: 1500000, currencyCode: 'EGP', factoringType: 'CLASSIC - Supplier', statusCode: 'DRAFT', workflowStepName: 'Approval', track: 'FAST' }
]
const LIMITS = [
  { creditLimitId: 31, limitNumber: 'CL-APP-17', limitRef: 'APP-17', limitName: 'Delta Food Industries factoring', limitLevel: 'AGREEMENT', limitType: 'REVOLVING', limitCategory: 'FACTORING', currencyId: 1, currencyCode: 'EGP',
    approvedAmount: 9000000, currentLimit: 9000000, utilizedAmount: 3218400, availableAmount: 5781600, effectiveAt: '2026-11-20', expiryAt: '2027-11-19', isActive: true }
]
const done = (stepCode, stepName, route, at, by, gl) => ({ stepCode, stepName, route, completedAt: at, completedBy: by, glTranSetIds: gl })
const DISBURSEMENTS = [
  { disbursementId: 42, disbursementNumber: 'DISB-2026-42', disbursementDate: '2026-11-23', clientName: 'Delta Food Industries', grossAmount: 1250000, netAmount: null, repaymentStartDate: null, tenorMonth: 3, disbursementRef: null,
    disbursementType: 'CLASSIC', workflowStarted: false, limitId: 31, limitNumber: 'CL-APP-17', currencyCode: 'EGP', workflowStepName: null, glTranSetId: null, completedSteps: [] },
  { disbursementId: 41, disbursementNumber: 'DISB-2026-41', disbursementDate: '2026-11-21', clientName: 'Delta Food Industries', grossAmount: 3786353, netAmount: 3157560, repaymentStartDate: '2026-12-21', tenorMonth: 3, disbursementRef: null,
    disbursementType: 'CLASSIC', workflowStarted: true, limitId: 31, limitNumber: 'CL-APP-17', currencyCode: 'EGP', workflowStepName: 'Treasury Disburse', glTranSetId: 'FIN-FCT-2026-000118',
    completedSteps: [done('INITIATION', 'Initiation', 'crm-disbursement-request', '2026-11-21T09:10:00Z', 'hany.ops', []), done('INVOICE_UPLOAD', 'Invoice Upload', 'crm-disbursement-invoice-entry', '2026-11-22T13:40:00Z', 'hany.ops', ['FIN-FCT-2026-000117', 'FIN-FCT-2026-000118'])] },
  { disbursementId: 37, disbursementNumber: 'DISB-2026-37', disbursementDate: '2026-11-05', clientName: 'Delta Food Industries', grossAmount: 950000, netAmount: 801250, repaymentStartDate: '2026-12-05', tenorMonth: 2, disbursementRef: null,
    disbursementType: 'CLASSIC', workflowStarted: true, limitId: 31, limitNumber: 'CL-APP-17', currencyCode: 'EGP', workflowStepName: null, glTranSetId: 'FIN-FCT-2026-000096',
    completedSteps: [done('INITIATION', 'Initiation', 'crm-disbursement-request', '2026-11-05T08:30:00Z', 'hany.ops', []), done('INVOICE_UPLOAD', 'Invoice Upload', 'crm-disbursement-invoice-entry', '2026-11-05T12:00:00Z', 'hany.ops', ['FIN-FCT-2026-000096']),
      done('TREASURY_DISBURSE', 'Treasury Disburse', 'trs-disburse', '2026-11-06T10:15:00Z', 'nour.trs', ['FIN-TRS-2026-000052'])] }
]
const claimRow = (id, no, disb, dno, date, total, paid, status) => ({ claimId: id, claimNumber: no, clientId: CID, clientName: 'Delta Food Industries', disbursementId: disb, disbursementNumber: dno, claimDate: date, currencyCode: 'EGP',
  totalAmount: total, paidAmount: paid, outstandingAmount: total - paid, statusCode: status })
const CLAIMS = [
  claimRow(12, 'CLM-202611-00000012', 41, 'DISB-2026-41', '2026-11-22', 39672, 39672, 'PAID'),
  claimRow(14, 'CLM-202611-00000014', 42, 'DISB-2026-42', '2026-11-23', 15960, 6000, 'PARTIALLY_PAID'),
  claimRow(9, 'CLM-202611-00000009', 37, 'DISB-2026-37', '2026-11-05', 12280, 12280, 'PAID')
]
const CLAIM_LINES = [
  { claimLineId: 1, lineNo: 1, chargeCode: 'SETUP_FEE', chargeName: 'Setup fee', chargeAmount: 5000, taxAmount: 700, totalAmount: 5700, paidAmount: 5700, outstandingAmount: 0 },
  { claimLineId: 2, lineNo: 2, chargeCode: 'COMMISSION', chargeName: 'Factoring commission (0.25% of eligible invoices)', chargeAmount: 6000, taxAmount: 840, totalAmount: 6840, paidAmount: 300, outstandingAmount: 6540 },
  { claimLineId: 3, lineNo: 3, chargeCode: 'HANDLING', chargeName: 'Handling fee (6 invoices)', chargeAmount: 3000, taxAmount: 420, totalAmount: 3420, paidAmount: 0, outstandingAmount: 3420 }
]
const CLAIM = { claim: CLAIMS[1], lines: CLAIM_LINES, payments: [
  { claimPaymentId: 3, repaymentNumber: 'RPT-202611-00000058', sourceType: 'BANK_TRANSFER', reference: 'NBE-TRF-90213', paymentDate: '2026-11-23', paymentAmount: 6000, glTranSetId: 'FIN-COL-2026-000044' }] }
const SOURCES = [
  { repaymentSourceId: 61, repaymentNumber: 'RPT-202611-00000061', sourceType: 'BANK_TRANSFER', reference: 'CIB-TRF-55120', valueDate: '2026-11-23', sourceAmount: 25000, usedAmount: 0, availableAmount: 25000 },
  { repaymentSourceId: 58, repaymentNumber: 'RPT-202611-00000058', sourceType: 'BANK_TRANSFER', reference: 'NBE-TRF-90213', valueDate: '2026-11-23', sourceAmount: 6000, usedAmount: 6000, availableAmount: 0 }
]

// the agreement header the step pages show
const S = { agreementId: 17, agreementNumber: 'APP-17', clientId: CID, clientName: 'Delta Food Industries', currencyId: 1, currencyCode: 'EGP', facilityLimit: 10000000, approvedLimit: null,
  applicationDate: '2026-11-02', relationshipManager: 'Karim Fawzy', statusCode: 'DRAFT', currentStep: null, track: 'NORMAL' }
const SF = { ...S, agreementId: 18, agreementNumber: 'APP-18', facilityLimit: 1500000, applicationDate: '2026-10-15', track: 'FAST' }
const TERMS = { approvedLimit: 9000000, approvedAdvRate: 85, approvedTenorMth: 12, effectiveRatePct: 28.5 }
const DOCS_OLDEST = [
  { documentId: 71, agreementId: 17, docTypeId: 1, documentTitle: 'Commercial register', fileName: 'commercial-register.pdf', fileExtension: 'pdf', fileSizeBytes: 412330, mimeType: 'application/pdf', statusId: 2, creationDate: '2026-11-03T09:12:00Z', createdBy: 'mai.sales' },
  { documentId: 72, agreementId: 17, docTypeId: 2, documentTitle: 'Audited financial statements 2025', fileName: 'fs-2025.pdf', fileExtension: 'pdf', fileSizeBytes: 1830220, mimeType: 'application/pdf', statusId: 2, creationDate: '2026-11-03T09:15:00Z', createdBy: 'mai.sales' },
  { documentId: 73, agreementId: 17, docTypeId: 3, documentTitle: 'Credit approval memo', fileName: 'cam-app-17.pdf', fileExtension: 'pdf', fileSizeBytes: 288104, mimeType: 'application/pdf', statusId: 2, creationDate: '2026-11-10T14:02:00Z', createdBy: 'omar.credit' },
  { documentId: 74, agreementId: 17, docTypeId: 4, documentTitle: 'Factoring contract (draft)', fileName: 'contract-app-17-draft.pdf', fileExtension: 'pdf', fileSizeBytes: 356771, mimeType: 'application/pdf', statusId: 1, creationDate: '2026-11-18T11:30:00Z', createdBy: 'hany.ops' }
]
const DOCS = [...DOCS_OLDEST].reverse()
const DOC_LOOKUPS = { documentTypes: [{ value: 1, label: 'Commercial register', code: 'CR' }, { value: 2, label: 'Financial statements', code: 'FS' }, { value: 3, label: 'Credit memo', code: 'CAM' }, { value: 4, label: 'Contract', code: 'CONTRACT' }, { value: 5, label: 'Signed acceptance', code: 'ACCEPTANCE' }],
  documentStatuses: [{ value: 1, label: 'Draft', code: 'DRAFT' }, { value: 2, label: 'Final', code: 'FINAL' }, { value: 3, label: 'Signed', code: 'SIGNED' }] }
const USERS = [
  { id: 'u5', userName: 'mai.sales', email: 'mai@finova.example', fullName: 'Mai Hamdy', employeeNo: 'E-1042' },
  { id: 'u6', userName: 'tamer.sales', email: 'tamer@finova.example', fullName: 'Tamer Nabil', employeeNo: 'E-1051' }
]

// the workflow tasks, one per step page
const task = (inboxId, workflowCode, workflowName, stepCode, stepName, actions, entityRef, entityId, extra = {}) => ({ inboxId, taskId: inboxId, instanceId: 500 + inboxId, stepInstanceId: 900 + inboxId, status: 'IN_PROGRESS',
  assignmentType: 'ROLE', assignedRole: 'FCT', claimedBy: 'admin', claimedByName: 'Mariam Adel', isClaimedByMe: true, subject: `${entityRef} · Delta Food Industries`, allowedActions: actions,
  createdOn: '2026-11-22T08:00:00Z', claimedOn: '2026-11-22T08:30:00Z', workflowCode, workflowName, stepCode, stepName, moduleCode: 'FCT', entityType: 'FCT_AGREEMENT', entityId, entityRef,
  clientId: CID, clientName: 'Delta Food Industries', amount: 10000000, currencyCode: 'EGP', priority: 'NORMAL', ...extra })
const NORMAL = ['FACTORING_LIMIT_APPROVAL', 'Factoring Limit Approval']
const TASKS = {
  101: task(101, ...NORMAL, 'OPERATION_RECEIVED', 'Operation Received', 'ACCEPT,RETURN,REJECT,CLAIM', 'APP-17', 17),
  102: task(102, ...NORMAL, 'SALES_REVIEW', 'Sales Review', 'CLAIM,COMPLETE', 'APP-17', 17),
  103: task(103, ...NORMAL, 'INVESTIGATION', 'Investigation', 'CLAIM,COMPLETE', 'APP-17', 17),
  104: task(104, ...NORMAL, 'CREDIT_ANALYSIS', 'Credit Analysis', 'CLAIM,COMPLETE', 'APP-17', 17),
  105: task(105, ...NORMAL, 'RISK_REVIEW', 'Risk Review', 'CLAIM,ACCEPT,REJECT', 'APP-17', 17),
  106: task(106, ...NORMAL, 'COMMITTEE_APPROVAL', 'Committe Approval', 'CLAIM,ACCEPT,REJECT', 'APP-17', 17),
  107: task(107, ...NORMAL, 'CLIENT_ACCEPTANCE', 'Client Acceptance', 'CLAIM,REJECT,ACCEPT', 'APP-17', 17),
  108: task(108, ...NORMAL, 'CONTRACT_GENERATION', 'Generate Contract', 'CLAIM,COMPLETE', 'APP-17', 17),
  109: task(109, ...NORMAL, 'LEGAL_REVIEW', 'Legal Review and Limit Creation', 'CLAIM,REJECT,ACCEPT', 'APP-17', 17),
  110: task(110, 'FACTORING_FAST_TRACK', 'Factoring Fast Track', 'FAST_TRACK_APPROVAL', 'Approval', 'CLAIM,ACCEPT,REJECT', 'APP-18', 18, { amount: 1500000 }),
  111: task(111, 'DISBURSEMENT_REQUEST_WF', 'Disbursement Request Workflow', 'INVOICE_UPLOAD', 'Invoice Upload', 'CLAIM,START,COMPLETE', 'DISB-2026-41', 41, { entityType: 'FCT_DISBURSEMENT_REQUEST', amount: 3786353, moduleCode: 'CRM' }),
  112: task(112, 'DISBURSEMENT_REQUEST_WF', 'Disbursement Request Workflow', 'TREASURY_DISBURSE', 'Treasury Disburse', 'CLAIM,START,COMPLETE', 'DISB-2026-41', 41, { entityType: 'FCT_DISBURSEMENT_REQUEST', amount: 3157560, moduleCode: 'TRS' })
}
const INBOX = [
  { ...TASKS[104], status: 'IN_PROGRESS' },
  { ...TASKS[110], status: 'READY', claimedBy: null, claimedByName: null, isClaimedByMe: false, createdOn: '2026-11-22T10:05:00Z' },
  { ...TASKS[112], status: 'READY', claimedBy: null, claimedByName: null, isClaimedByMe: false, createdOn: '2026-11-22T13:41:00Z' },
  { ...TASKS[102], inboxId: 120, entityRef: 'APP-19', subject: 'APP-19 · Nile Packaging Co', clientName: 'Nile Packaging Co', amount: 2500000, status: 'READY', claimedBy: null, claimedByName: null, isClaimedByMe: false, createdOn: '2026-11-23T07:50:00Z' }
]

// the step pages
const AGREEMENT_FORM = { agreementId: 17, agreementNumber: 'APP-17', agreementRef: 'OFF-20261102-7731', applicationDate: '2026-11-02', clientId: CID, factoringType: 'CLASSIC - Supplier', recourseType: 'RECOURSE', currencyId: 1,
  facilityLimit: 10000000, advanceRatePct: 85, tenorMonths: 12, notes: 'Supplier factoring on the invoices of three large retail debtors.', statusCode: 'DRAFT', createdBy: 'mai.sales',
  clients: [{ value: CID, label: 'Delta Food Industries' }], currencies: [{ value: 1, label: 'EGP' }, { value: 2, label: 'USD' }] }
const INQUIRY = { summary: S, inquiry: { investigationReportDate: '2026-11-06', creditBureauScore: 712, investigationNotes: 'Site visit done at the 10th of Ramadan factory. Operations as stated; no legal cases found.' },
  aml: { amlScreeningReportDate: '2026-11-06', amlRiskBand: 'LOW', amlScreeningResult: 'No match on the sanctions and local lists.', amlScreeningNotes: 'Shareholders and directors screened.' },
  legal: { legalOpenionReportDate: '2026-11-07', legalOpenionNotes: 'Company documents valid. Signatories confirmed from the commercial register.' },
  debtors: [
    { agrmtDebtorId: 1, agreementId: 17, debtorId: 201, debtorName: 'Giza Retail Group', visitDate: '2026-11-05', operationalStatus: 'ACTIVE', paymentHistory: 'Pays on time, 60-90 days', assessment: 'GOOD' },
    { agrmtDebtorId: 2, agreementId: 17, debtorId: 202, debtorName: 'Metro Markets', visitDate: '2026-11-05', operationalStatus: 'ACTIVE', paymentHistory: 'Pays on time', assessment: 'GOOD' },
    { agrmtDebtorId: 3, agreementId: 17, debtorId: 203, debtorName: 'Kheir Zaman', visitDate: '2026-11-06', operationalStatus: 'ACTIVE', paymentHistory: 'Some delays up to 15 days', assessment: 'FAIR' }] }
const CREDIT = { summary: S, financialAnalysis: { fsAssessmentId: 1, fsAssessmentDate: '2026-11-10', fsAssessmentResult: 'STRONG', fsNotes: 'Revenue up 18% in 2025; current ratio 1.4; leverage acceptable.' },
  riskRating: { rrAssessmentId: 1, rrAssessmentDate: '2026-11-10', rrAssessmentResult: 'B+', rrNotes: 'Debtor concentration on Giza Retail Group 38%.' },
  creditMemo: { creditMemoDate: '2026-11-11', recommendedLimt: 9000000, recommendedAdvRate: 85, recommendedTenorMth: 12, creditNotes: 'Recommend EGP 9 million, 85% advance, 12 months. Concentration on one debtor up to 40%.' },
  referenceDocuments: DOCS_OLDEST.slice(0, 3).map((d) => ({ sourceType: 'AGREEMENT', documentId: d.documentId, documentTitle: d.documentTitle, fileName: d.fileName, mimeType: d.mimeType, creationDate: d.creationDate, createdBy: d.createdBy })) }
const RISK = { summary: S, indicators: { creditBureauScore: 712, recourseType: 'RECOURSE', tenorMonths: 12, rrAssessmentResult: 'B+' },
  riskReview: { riskMemoDate: '2026-11-13', riskNotes: 'Agree with the credit recommendation. Keep the concentration cap at 40% per debtor.', riskDecision: null } }
const COMMITTEE = { summary: S, decision: { approvalDate: '2026-11-15', approvedLimit: 9000000, approvedAdvRate: 85, approvedTenorMth: 12 } }
const COMMITTEE_FAST = { summary: SF, decision: { approvalDate: '2026-10-16', approvedLimit: 1500000, approvedAdvRate: 80, approvedTenorMth: 6 } }
const CONTRACT_REQ = { summary: S, approvedTerms: TERMS, notesToOperations: 'Client signed the committee terms on 17 Nov. Please prepare the contract.' }
const CONTRACT = { summary: S, approvedTerms: TERMS, contract: { agreementNumber: 'APP-17', effectiveDate: '2026-11-20', tenorMonths: 12, contractStartDate: '2026-11-20', contractEndDate: '2027-11-19',
  constractSignoffDate: '2026-11-19', additionalTermsConditions: 'Notification of assignment to each debtor before the first disbursement.' } }
const LEGAL = { summary: S, legalReview: { legalReviewDate: '2026-11-19', legalOpinionNotes: 'Contract reviewed against the committee terms. No amendments needed.' } }

// the disbursement request
const AGR_SUMMARY = { agreementId: 17, agreementNumber: 'APP-17', clientName: 'Delta Food Industries', currencyCode: 'EGP', facilityLimit: 9000000, applicationAt: '2026-11-02', relationshipManager: 'Karim Fawzy', statusCode: 'DRAFT' }
const REQUEST = { disbursementId: 41, disbursementNumber: 'DISB-2026-41', disbursementDate: '2026-11-21', clientId: CID, clientName: 'Delta Food Industries', limitId: 31, currencyId: 1, grossAmount: 3786353, remarks: 'November invoices to Giza Retail and Metro Markets.', agreementId: 17, workflowStarted: true, glTranSetId: 'FIN-FCT-2026-000118' }
const inv = (id, no, date, due, gross, status, reason = null) => ({ invoiceId: id, invoiceNumber: no, clientInvoiceDate: date, dueDate: due, grossAmount: gross, advanceRatePct: 85, advanceAmount: status === 'ELIGIBLE' ? Math.round(gross * 0.85 * 100) / 100 : 0, statusCode: status, ineligibleReason: reason })
const INVOICES = [
  inv(901, 'GR-2026-11-0412', '2026-11-03', '2027-01-02', 1120000, 'ELIGIBLE'), inv(902, 'GR-2026-11-0418', '2026-11-05', '2027-01-04', 845500, 'ELIGIBLE'),
  inv(903, 'MM-77812', '2026-11-06', '2027-01-20', 990000, 'ELIGIBLE'), inv(904, 'MM-77840', '2026-11-09', '2027-02-07', 380853, 'ELIGIBLE'),
  inv(905, 'KZ-5521', '2026-11-10', '2027-03-10', 250000, 'NOT_ELIGIBLE', 'Tenor over 90 days'), inv(906, 'GR-2026-10-0377', '2026-10-01', '2026-11-30', 200000, 'NOT_ELIGIBLE', 'Due in less than 15 days')
]
const chq = (id, no, due, amount) => ({ chequeId: id, chequeNo: no, chequeDate: due, dueDate: due, issuerBankName: 'National Bank of Egypt', drawerNameEn: 'Delta Food Industries', depositBankAccountId: null, chequeAmount: amount, currencyCode: 'EGP', sourceModule: 'FCT', receivedDate: '2026-11-21', depositDate: null, statusCode: 'DRAFT' })
const SCHEDULE = [
  { installmentNo: 1, dueDate: '2026-12-21', openingPrincipal: 2843400, principalDue: 0, interestDue: 67520.11, totalInstallment: 67520.11, closingPrincipal: 2843400, interestRate: 28.5, dayCount: 30, statusCode: 'DUE' },
  { installmentNo: 2, dueDate: '2027-01-21', openingPrincipal: 2843400, principalDue: 0, interestDue: 69770.78, totalInstallment: 69770.78, closingPrincipal: 2843400, interestRate: 28.5, dayCount: 31, statusCode: 'DUE' },
  { installmentNo: 3, dueDate: '2027-02-21', openingPrincipal: 2843400, principalDue: 2843400, interestDue: 69770.78, totalInstallment: 2913170.78, closingPrincipal: 0, interestRate: 28.5, dayCount: 31, statusCode: 'DUE' }
]
const CHEQUES = SCHEDULE.map((r, i) => chq(800 + i, `00${4471 + i}`, r.dueDate, r.totalInstallment))
const IE_SUMMARY = { limitNumber: 'CL-APP-17', clientName: 'Delta Food Industries', approvedLimit: 9000000, availableAmount: 5781600, currencyCode: 'EGP', totalInvoices: 6, approvedInvoiced: 3336353, advanceRate: 85, utilizationAmount: 3218400, utilizationPercent: 35.76 }
const INVOICE_ENTRY = { disbursementId: 41,
  terms: { disbursementNumber: 'DISB-2026-41', disbursementDate: '2026-11-21', clientId: CID, limitId: 31, currencyId: 1, productId: 5, grossAmount: 3336353, advanceRatePct: 85, tenorMonth: 3, indexRate: 24, spreadRate: 4.5,
    baseDayCount: 'ACT/365', repaymentStartDate: '2026-11-21', principalRepaymentScheduleType: 'NORMAL_SCHEDULE', principalRepaymentMethod: 'AT_MATURITY', principalDistributionMethod: null, interestRepaymentMethod: 'EVERY_N_MONTHS',
    paymentMethod: 'CHEQUE', remarks: 'November invoices.', principalFrequencyMonths: 3, interestFrequencyMonths: 1, interestStartDate: '2026-11-21', principalFirstDueCode: null, interestFirstDueCode: 'NEXT_MONTH' },
  batch: { batchId: 61, batchNumber: 'BTCH-41-1', batchName: 'November invoices', clientId: CID, batchDate: '2026-11-21', batchType: 'BULK', currencyId: 1, totalNetAmount: 3786353, remarks: null },
  summary: IE_SUMMARY, invoices: INVOICES, cheques: CHEQUES,
  options: { clients: [{ value: String(CID), label: 'Delta Food Industries' }], limits: [{ value: '31', label: 'CL-APP-17 · available 5,781,600.00' }], currencies: [{ value: '1', label: 'EGP' }],
    products: [{ value: '5', label: 'Factoring - Supplier (classic)' }, { value: '6', label: 'Reverse factoring' }], reverseProductIds: [6] },
  customSchedule: [] }
const charge = (id, code, name, method, base, rate, amount, tax, timing) => ({ disbursementChargeId: id, ruleCode: code, ruleName: name, calculationMethod: method, baseCode: base ? 'ELIGIBLE_GROSS' : null, baseAmount: base, ratePercent: rate,
  chargeAmount: amount, taxAmount: tax, totalAmount: amount + tax, chargeTimingCode: timing, dueDate: '2026-11-21', statusCode: 'CALCULATED' })
const RESULT = { statusCode: 'PROCESSED', approvedInvoiceAmount: 3336353, approvedAdvanceAmount: 2835900.05, feeDeducted: 34800, taxDeducted: 4872, netAmount: 2796228.05,
  charges: [charge(1, 'SETUP_FEE', 'Setup fee', 'FIXED', null, null, 25000, 3500, 'UPFRONT'), charge(2, 'COMMISSION', 'Factoring commission', 'PERCENT', 3336353, 0.25, 8340.88, 1167.72, 'UPFRONT'),
    charge(3, 'HANDLING', 'Handling fee', 'PER_INVOICE', null, null, 1459.12, 204.28, 'UPFRONT')],
  schedule: SCHEDULE, glEntries: [], isPreview: false, scheduleNote: null }
const D_CLAIM = { claimStatusCode: 'PAID', chargesTotal: 39672, isCompleted: false, claim: { claim: CLAIMS[0], lines: CLAIM_LINES.map((l) => ({ ...l, paidAmount: l.totalAmount, outstandingAmount: 0 })),
  payments: [{ claimPaymentId: 1, repaymentNumber: 'RPT-202611-00000049', sourceType: 'BANK_TRANSFER', reference: 'CIB-TRF-54980', paymentDate: '2026-11-22', paymentAmount: 39672, glTranSetId: 'FIN-COL-2026-000041' }] } }
const sInv = (id, no, at, due, gross, ok, reason = null) => ({ invoiceId: id, invoiceNumber: no, invoiceAt: at, dueAt: due, grossAmount: gross, financed: ok ? Math.round(gross * 85) / 100 : 0, eligible: ok, ineligibleReason: reason })
const D_SUMMARY = { disbursementId: 41, disbursementNumber: 'DISB-2026-41', clientName: 'Delta Food Industries', businessLine: 'FACTORING', isReverse: false, currencyCode: 'EGP', advanceRatePct: 85, netAmount: 2796228.05,
  receivedCount: 6, receivedGross: 3786353, notEligibleCount: 2, notEligibleGross: 450000, eligibleCount: 4, eligibleGross: 3336353, eligibleFinanced: 2835900.05,
  parties: [
    { clientId: 129, name: 'Giza Retail Group', taxNumber: '204-118-553', count: 2, grossAmount: 1965500, financed: 1670675, invoices: [sInv(901, 'GR-2026-11-0412', '2026-11-03', '2027-01-02', 1120000, true), sInv(902, 'GR-2026-11-0418', '2026-11-05', '2027-01-04', 845500, true)] },
    { clientId: null, name: 'Metro Markets', taxNumber: '310-442-901', count: 2, grossAmount: 1370853, financed: 1165225.05, invoices: [sInv(903, 'MM-77812', '2026-11-06', '2027-01-20', 990000, true), sInv(904, 'MM-77840', '2026-11-09', '2027-02-07', 380853, true)] }],
  notEligible: [sInv(905, 'KZ-5521', '2026-11-10', '2027-03-10', 250000, false, 'Tenor over 90 days'), sInv(906, 'GR-2026-10-0377', '2026-10-01', '2026-11-30', 200000, false, 'Due in less than 15 days')] }

// Treasury Disburse
const src = (key, bank, fac, tranche, sub, subId, avail, amount, fit, unavailable = null, rules = []) => ({ sourceKey: key, sourceTypeCode: 'BANK', bankId: bank[0], bankName: bank[1], facilityId: fac[0], facilityRef: fac[1], trancheName: tranche,
  sublimitId: subId, sublimitName: sub, available: avail, amount, isOverrideRequired: false, ruleIds: rules, fit, unavailable })
const NBE = [1, 'National Bank of Egypt'], CIB = [2, 'Commercial International Bank'], QNB = [3, 'QNB Alahli']
const TRS = {
  header: { disbursementId: 41, disbursementNumber: 'DISB-2026-41', clientId: CID, clientName: 'Delta Food Industries', agreementNumber: 'APP-17', businessLine: 'FACTORING', isReverse: false, currencyCode: 'EGP',
    netAmount: 2796228.05, invoiceCount: 4, maxTenorDays: 90, fundingStatus: 'RESERVED', executedBy: null, executedAt: null },
  parties: [{ clientId: 129, name: 'Giza Retail Group', role: 'DEBTOR', weight: 0.589, groupName: null, taxNumber: '204-118-553' }, { clientId: null, name: 'Metro Markets', role: 'DEBTOR', weight: 0.411, groupName: null, taxNumber: '310-442-901' },
    { clientId: CID, name: 'Delta Food Industries', role: 'SELLER', weight: 1, groupName: null, taxNumber: '118-220-447' }],
  sources: [
    src('B-11', NBE, [11, 'NBE-FAC-2026-01'], 'Tranche A', 'Factoring sublimit', 111, 12500000, 2000000, 'Factoring, tenor up to 120 days', null, [1]),
    src('B-12', NBE, [11, 'NBE-FAC-2026-01'], 'Tranche A', 'Short-term trade', 112, 3200000, 0, 'Any business line, tenor up to 90 days'),
    src('B-21', CIB, [21, 'CIB-RF-2025-07'], 'Main tranche', 'Receivables finance', 211, 4100000, 0, null, 'Debtor concentration: Giza Retail Group would reach 46% (limit 40%)', [2]),
    src('B-31', QNB, [31, 'QNB-WC-2026-02'], 'Tranche 1', 'Working capital', 311, 0, 0, null, 'Nothing left on this sublimit'),
    { sourceKey: 'OWN', sourceTypeCode: 'OWN_FUNDS', bankId: null, bankName: null, facilityId: null, facilityRef: null, trancheName: null, sublimitId: null, sublimitName: null, available: null, amount: 796228.05, isOverrideRequired: false, ruleIds: [], fit: null, unavailable: null }],
  rules: [
    { ruleId: 1, bankId: 1, bankName: 'National Bank of Egypt', facilityId: 11, ruleTypeCode: 'MAX_TENOR', label: 'Longest invoice tenor', breachActionCode: 'BLOCK', limitAbove: 5000000, warnAbove: 4000000, detail: 'Invoices up to 120 days: the longest here is 90 days' },
    { ruleId: 2, bankId: 2, bankName: 'Commercial International Bank', facilityId: 21, ruleTypeCode: 'DEBTOR_CONCENTRATION', label: 'Debtor concentration', breachActionCode: 'BLOCK', limitAbove: 0, warnAbove: null, detail: 'Giza Retail Group would reach 46% of the facility, above the 40% allowed' }],
  canEdit: true, overrideReason: null, overrideRequestedBy: null, overrideApprovedBy: null, canApproveOverride: false, approveBlockedReason: null,
  payments: [
    { sourceKey: 'B-11', sourceTypeCode: 'BANK', bankName: 'National Bank of Egypt', amount: 2000000, accountId: 3, accountRef: 'NBE-CUR-01', accountNo: '1203-445566-01', accountBankName: 'National Bank of Egypt', glAccountCode: '110201', glTranSetId: null, problem: null },
    { sourceKey: 'OWN', sourceTypeCode: 'OWN_FUNDS', bankName: null, amount: 796228.05, accountId: 1, accountRef: 'MAIN-OPS', accountNo: '1203-000100-01', accountBankName: 'National Bank of Egypt', glAccountCode: '110101', glTranSetId: null, problem: null }],
  paymentStatus: 'RESERVED', executionDate: '2026-11-23', suggestOrder: 'BANK_FIRST', transfers: null,
  gl: { status: 'PREVIEW', tranSetId: null, problem: null, postsWhen: 'Execute', lines: [
    { journalNo: 1, account: '210301', accountName: 'Bank borrowing - NBE factoring sublimit', dr: 0, cr: 2000000, description: 'Drawdown NBE-FAC-2026-01' },
    { journalNo: 1, account: '110201', accountName: 'NBE current account', dr: 2000000, cr: 0, description: 'Drawdown NBE-FAC-2026-01' },
    { journalNo: 2, account: '130110', accountName: 'Factoring receivables - Delta Food Industries', dr: 2796228.05, cr: 0, description: 'Payment DISB-2026-41' },
    { journalNo: 2, account: '110201', accountName: 'NBE current account', dr: 0, cr: 2000000, description: 'Payment DISB-2026-41' },
    { journalNo: 2, account: '110101', accountName: 'Main operating account', dr: 0, cr: 796228.05, description: 'Payment DISB-2026-41' }] } }

function api(url, method) {
  const u = new URL(url), p = u.pathname
  if (p === '/api/fct/customers') return CUSTOMERS
  if (/^\/api\/fct\/customers\/\d+$/.test(p)) return CUSTOMER
  if (/^\/api\/fct\/customers\/\d+\/applications$/.test(p)) return APPLICATIONS
  if (/^\/api\/fct\/customers\/\d+\/agreements$/.test(p)) return AGREEMENTS
  if (/^\/api\/fct\/customers\/\d+\/credit-limits$/.test(p)) return LIMITS
  if (/^\/api\/fct\/customers\/\d+\/claims$/.test(p)) return CLAIMS
  if (p === '/api/fct/calculator/calculate') return { advanceBasePct: 90, advanceDilutionPct: -2, advanceRiskBufferPct: -2, advancePct: 86, discountCostOfFundsPct: 24, discountRiskMarginPct: 1.25, discountOperatingSpreadPct: 3,
    discountTenorAdjustmentPct: 0, discountConcentrationPct: 0, discountRatePct: 28.25, spreadOverLcrPct: 4.25, setupFeeTotal: 35000, commissionAnnual: 90000, handlingAnnual: 36000, debtorFeesTotal: 24000, interestIncome: 2156597.26, totalAnnualIncome: 185000 }
  if (/^\/api\/fct\/agreements\/\d+\/review-summary$/.test(p)) return { agreementId: 17, agreementNumber: 'APP-17', clientId: CID, clientName: 'Delta Food Industries', currencyCode: 'EGP', facilityLimit: 10000000, approvedLimit: null, applicationDate: '2026-11-02', effectiveDate: null, statusCode: 'DRAFT' }
  if (p === '/api/fct/users') return USERS
  if (p === '/api/fct/document-lookups') return DOC_LOOKUPS
  if (/^\/api\/fct\/agreements\/\d+\/documents$/.test(p)) return DOCS
  if (/^\/api\/fct\/agreements\/\d+$/.test(p)) return AGREEMENT_FORM
  if (/^\/api\/fct\/inquiry-field-report\/\d+$/.test(p)) return INQUIRY
  if (/^\/api\/fct\/credit-analysis\/\d+$/.test(p)) return CREDIT
  if (/^\/api\/fct\/risk-review\/\d+$/.test(p)) return RISK
  if (p === '/api/fct/committee-decision/18') return COMMITTEE_FAST
  if (/^\/api\/fct\/committee-decision\/\d+$/.test(p)) return COMMITTEE
  if (/^\/api\/fct\/contract-request\/\d+$/.test(p)) return CONTRACT_REQ
  if (/^\/api\/fct\/contract-generation\/\d+$/.test(p)) return CONTRACT
  if (/^\/api\/fct\/legal-review\/\d+$/.test(p)) return LEGAL
  // workflow
  if (p === '/api/wfe/inbox/my' || p === '/api/wfe/inbox/all') return INBOX
  const tm = p.match(/^\/api\/wfe\/inbox\/(\d+)$/); if (tm) return TASKS[Number(tm[1])] ?? TASKS[104]
  // disbursement requests
  if (p === '/api/crm/disbursement-requests') return DISBURSEMENTS
  if (/^\/api\/crm\/disbursement-requests\/clients\/\d+\/limits$/.test(p)) return [{ value: '31', label: 'CL-APP-17 · available 5,781,600.00' }]
  if (p === '/api/crm/disbursement-requests/clients') return [{ value: String(CID), label: 'Delta Food Industries' }]
  if (p === '/api/crm/disbursement-requests/currencies') return [{ value: '1', label: 'EGP' }]
  if (/\/limits\/\d+\/agreement-summary$/.test(p) || /\/agreements\/\d+\/summary$/.test(p)) return AGR_SUMMARY
  if (/\/limits\/\d+\/advance-rate$/.test(p)) return 85
  if (/\/clients\/\d+\/invoice-entry-limits$/.test(p)) return INVOICE_ENTRY.options.limits
  if (/^\/api\/crm\/disbursement-requests\/\d+$/.test(p)) return REQUEST
  if (/\/invoice-entry\/summary$/.test(p)) return IE_SUMMARY
  if (/\/invoice-entry\/result$/.test(p) || /\/charges-preview/.test(p)) return RESULT
  if (/\/invoice-entry\/claim$/.test(p)) return D_CLAIM
  if (/\/invoice-entry$/.test(p)) return INVOICE_ENTRY
  if (/\/disbursement-requests\/\d+\/summary$/.test(p) || /\/funding\/summary$/.test(p)) return D_SUMMARY
  if (/\/steps\/[A-Z_]+\/gl-tran-sets$/.test(p)) return []
  if (/^\/api\/trs\/disbursements\/\d+\/funding$/.test(p)) return TRS
  // collection
  if (p === '/api/collection/claims/customers' || /\/claims\/customers$/.test(p)) return [{ value: String(CID), label: 'Delta Food Industries' }]
  if (/\/claims\/sources\/\d+$/.test(p)) return SOURCES
  if (/\/claims\/\d+$/.test(p)) return CLAIM
  if (/\/claims$/.test(p)) return CLAIMS.filter((c) => c.statusCode !== 'PAID')
  if (p === '/api/sec/auth/me') return USER
  if (p.includes('/count')) return 3
  return []
}

const USER = { userId: '1', userName: 'admin', fullName: 'Mariam Adel', roles: ['ADMIN'], permissions: ['*'] }

// Finova in place of Capitra: the name in every text, title and label, and the logo
async function finova(page) {
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
const missing = new Set()
async function shot(lang, name, path, act, opts = {}) {
  const ctx = await browser.newContext({ viewport: { width: opts.width ?? 1440, height: 900 }, deviceScaleFactor: 1 })
  await ctx.addInitScript(([lang, user]) => {
    sessionStorage.setItem('capitra.token', 'x')
    sessionStorage.setItem('capitra.expiresAt', new Date(Date.now() + 3600e3).toISOString())
    sessionStorage.setItem('capitra.user', JSON.stringify(user))
    localStorage.setItem('CAP_LANG', lang)
  }, [lang, USER])
  const page = await ctx.newPage()
  page.on('pageerror', (e) => console.log('PAGE ERROR', lang, name, String(e).slice(0, 300)))
  page.on('console', (m) => { if (m.type() === 'error') console.log('CONSOLE', lang, name, m.text().slice(0, 300)) })
  await page.route((u) => u.pathname.startsWith('/api/'), (r) => {
    const body = api(r.request().url(), r.request().method())
    if (Array.isArray(body) && body.length === 0) missing.add(r.request().method() + ' ' + new URL(r.request().url()).pathname)
    return r.fulfill({ status: 200, contentType: 'application/json', body: JSON.stringify(body) })
  })
  await page.goto(BASE + path)
  await page.addStyleTag({ content: '#__vue-devtools-container__,[id^="__vue-devtools"],[class*="vue-devtools"]{display:none!important}' })
  await page.waitForTimeout(1800)
  if (act) { try { await act(page); await page.waitForTimeout(1000) } catch (e) { console.log('ACT FAILED', lang, name, String(e).split('\n')[0]) } }
  await finova(page)
  await page.screenshot({ path: `${OUT}/${lang}-${name}.png`, fullPage: !opts.viewportOnly })
  await ctx.close()
  console.log('shot', lang, name)
}

const tab = (n) => async (p) => { await p.locator('.fc-tab').nth(n).click() }
for (const lang of ['EN', 'AR']) {
  await shot(lang, 'f01-home', '/factoring')
  await shot(lang, 'f02-workspace', `/factoring/customer/${CID}`)
  await shot(lang, 'f03-calculator', `/factoring/calculator/${CID}`, async (p) => {
    const nums = p.locator('.fc-calc-main input[type=number]')
    const v = ['10000000', '36000000', '90', '3', '360', null, '2', '24', '3', '90', null, '10000', '0.25', '0.25', '100', '8000', '0']
    for (let i = 0; i < v.length; i++) if (v[i] !== null) await nums.nth(i).fill(v[i])
    await p.locator('.fc-actions .fx-btn--teal').click()
  })
  await shot(lang, 'f04-track', `/factoring/customer/${CID}`, async (p) => { await p.locator('td .fx-btn--primary:not([disabled])').first().click() }, { viewportOnly: true })
  await shot(lang, 'f05-inbox', '/workflow/inbox')
  await shot(lang, 'f06-operation-received', '/factoring/agreements/17/assign-sales?inboxId=101')
  await shot(lang, 'f07-sales-review', '/factoring/agreements/17/review?inboxId=102')
  await shot(lang, 'f08-investigation', '/factoring/agreements/17/inquiry-field-report?inboxId=103')
  await shot(lang, 'f09-credit-analysis', '/factoring/agreements/17/credit-analysis?inboxId=104')
  await shot(lang, 'f10-risk-review', '/factoring/agreements/17/risk-review?inboxId=105')
  await shot(lang, 'f11-committee', '/factoring/agreements/17/committee-decision?inboxId=106')
  await shot(lang, 'f12-client-acceptance', '/factoring/agreements/17/contract-request?inboxId=107')
  await shot(lang, 'f13-contract', '/factoring/agreements/17/contract-generation?inboxId=108')
  await shot(lang, 'f14-legal-review', '/factoring/agreements/17/legal-review?inboxId=109')
  await shot(lang, 'f15-documents', '/factoring/agreements/17/credit-analysis?inboxId=104', async (p) => { await p.locator('.fx-btn:has(.fa-folder-open-o)').first().click() }, { viewportOnly: true })
  await shot(lang, 'f16-accept', '/factoring/agreements/17/legal-review?inboxId=109', async (p) => { await p.locator('.fx-btn--teal:has(.fa-check-circle)').first().click() }, { viewportOnly: true })
  await shot(lang, 'f17-fast-approval', '/factoring/agreements/18/committee-decision?inboxId=110')
  await shot(lang, 'f18-agreements', `/factoring/customer/${CID}`, tab(1))
  await shot(lang, 'f19-limits', `/factoring/customer/${CID}`, tab(2))
  await shot(lang, 'f20-new-request', `/crm/customers/${CID}/limits/31/disbursement-request`)
  await shot(lang, 'f21-disbursements', `/factoring/customer/${CID}?tab=disbursements`)
  await shot(lang, 'f22-invoice-entry', '/crm/disbursement-requests/41/invoices?inboxId=111')
  await shot(lang, 'f23-invoice-summary', '/crm/disbursement-requests/41/invoices?inboxId=111', async (p) => { await p.locator('.fx-btn:has(.fa-list-alt)').first().click() }, { viewportOnly: true })
  await shot(lang, 'f24-treasury', '/trs/disbursements/41/funding?inboxId=112')
  await shot(lang, 'f25-claims', `/factoring/customer/${CID}?tab=claims`)
  await shot(lang, 'f26-customer-payments', `/collection-repayments/customer-payments?clientId=${CID}&claimId=14`)
}
await browser.close()
console.log('EMPTY RESPONSES:\n' + [...missing].sort().join('\n'))
