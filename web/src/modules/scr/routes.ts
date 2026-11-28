// Screening: the FINOVA screening app merged into CAPITRA. The federation keeps the Final List (letters, parties, decisions approved
// through the SCR_DECISION_APPROVAL workflow); member companies screen against it from the Screening Portal.
import type { RouteRecordRaw } from 'vue-router'

const meta = (functionName: string) => ({ moduleName: 'SCREENING', functionName })

export const scrRoutes: RouteRecordRaw[] = [
  { path: 'screening', name: 'scr-home', component: () => import('./pages/ScrHomePage.vue'), meta: meta('Screening') },
  { path: 'screening/letters', name: 'scr-letters', component: () => import('./pages/ScrLettersPage.vue'), meta: meta('Letters') },
  { path: 'screening/parties', name: 'scr-parties', component: () => import('./pages/ScrPartiesPage.vue'), meta: meta('Parties') },
  // the former 'Decisions awaiting approval' page: the Decisions list now has the workflow columns and Run
  { path: 'screening/approvals', name: 'scr-approvals', redirect: { name: 'scr-decisions' } },
  { path: 'screening/decisions', name: 'scr-decisions', component: () => import('./pages/ScrDecisionsPage.vue'), meta: meta('Decisions') },
  { path: 'screening/decisions/new', name: 'scr-decision-new', component: () => import('./pages/ScrDecisionPage.vue'), meta: meta('New decision') },
  // also the page of the workflow steps SCR_REVIEW and SCR_APPROVAL (opened from the inbox with ?inboxId=)
  {
    path: 'screening/decisions/:decisionId(\\d+)', name: 'scr-decision', component: () => import('./pages/ScrDecisionPage.vue'),
    props: (r) => ({ decisionId: Number(r.params.decisionId) }), meta: meta('Decision')
  },
  { path: 'screening/registry', name: 'scr-registry', component: () => import('./pages/ScrRegistryPage.vue'), meta: meta('Federation Final List') },
  { path: 'screening/companies', name: 'scr-companies', component: () => import('./pages/ScrCompaniesPage.vue'), meta: meta('Member companies') },
  // every sign-in attempt, and per session the inquiries received and answered
  { path: 'screening/access-log', name: 'scr-access-log', component: () => import('./pages/ScrAccessLogPage.vue'), meta: meta('Sign-ins and activity') },
  // billing: price plans, each company's billing, invoices (printable)
  { path: 'screening/billing', name: 'scr-billing', component: () => import('./pages/ScrBillingPage.vue'), meta: meta('Billing') },
  { path: 'screening/billing/plans', name: 'scr-price-plans', component: () => import('./pages/ScrPricePlansPage.vue'), meta: meta('Price plans') },
  {
    path: 'screening/billing/invoices/:invoiceId(\\d+)', name: 'scr-invoice', component: () => import('./pages/ScrInvoicePage.vue'),
    props: (r) => ({ invoiceId: Number(r.params.invoiceId) }), meta: meta('Invoice')
  },
  {
    path: 'screening/inquiries/:inquiryId(\\d+)/certificate', name: 'scr-certificate', component: () => import('./pages/ScrCertificatePage.vue'),
    props: (r) => ({ inquiryId: Number(r.params.inquiryId) }), meta: { ...meta('Screening certificate'), forCompanies: true }
  },
  { path: 'screening/inquiries', name: 'scr-inquiries', component: () => import('./pages/ScrInquiriesPage.vue'), meta: meta('Inquiry audit') },
  // a member company administrator's own invoice, read and print only
  {
    path: 'screening/portal/invoices/:invoiceId(\\d+)', name: 'scr-portal-invoice', component: () => import('./pages/ScrInvoicePage.vue'),
    props: (r) => ({ invoiceId: Number(r.params.invoiceId), portal: true }), meta: { ...meta('Invoice'), forCompanies: true }
  },
  // member companies: the only page their users can open
  { path: 'screening/portal', name: 'scr-portal', component: () => import('./pages/ScrPortalPage.vue'), meta: meta('Screening Portal') }
]
