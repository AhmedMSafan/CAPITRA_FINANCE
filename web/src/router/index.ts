import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '@/shared/layout/AppLayout.vue'
import { isAuthenticated } from '@/shared/auth/session'

// Every page lives under AppLayout and needs a signed-in user, except the login page.
// Module routes are added here as each module is built (src/modules/{module}).
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/modules/sec/pages/LoginPage.vue'),
      meta: { public: true, moduleName: 'CAPITRA', functionName: 'Login' }
    },
    {
      path: '/',
      component: AppLayout,
      children: [
        {
          path: '',
          name: 'home',
          component: () => import('@/modules/home/pages/HomePage.vue'),
          meta: { moduleName: 'HOME', functionName: 'My workspace' }
        },
        // Portfolio acquisition (APEX App 105): portfolio cases.
        {
          path: 'mortgage/portfolios',
          name: 'mtg-portfolios',
          component: () => import('@/modules/mtg/pages/PortfolioCasesPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Cases' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId',
          name: 'mtg-portfolio',
          component: () => import('@/modules/mtg/pages/PortfolioWorkspacePage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId/payments',
          name: 'mtg-portfolio-payments',
          component: () => import('@/modules/mtg/pages/PaymentHistoryPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Payment History' }
        },
        // Portfolio workflow steps: one page per step.
        {
          path: 'mortgage/portfolios/:portfolioId/excel-import',
          name: 'mtg-step-excel-import',
          component: () => import('@/modules/mtg/steps/excel-import/ExcelImportPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Excel Import & Validation' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId/aml-review',
          name: 'mtg-step-aml-review',
          component: () => import('@/modules/mtg/steps/aml-review/AmlReviewPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'AML Review' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId/inquiries',
          name: 'mtg-step-inquiries',
          component: () => import('@/modules/mtg/steps/inquiries/InquiriesPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Inquiries' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId/legal-review',
          name: 'mtg-step-legal-review',
          component: () => import('@/modules/mtg/steps/legal-review/LegalReviewPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Legal Review' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId/valuation',
          name: 'mtg-step-valuation',
          component: () => import('@/modules/mtg/steps/valuation/ValuationPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Valuation' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId/coverage-review',
          name: 'mtg-step-coverage-review',
          component: () => import('@/modules/mtg/steps/coverage-review/CoverageReviewPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Coverage Review' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId/cheque-coverage',
          name: 'mtg-step-cheque-coverage',
          component: () => import('@/modules/mtg/steps/cheque-coverage/ChequeCoveragePage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Cheque Coverage Review' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId/financial-review',
          name: 'mtg-step-financial-review',
          component: () => import('@/modules/mtg/steps/financial-review/FinancialReviewPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Financial Review' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId/board-review',
          name: 'mtg-step-board-review',
          component: () => import('@/modules/mtg/steps/board-review/BoardReviewPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Board Review' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId/purchase-agreement',
          name: 'mtg-step-purchase-agreement',
          component: () => import('@/modules/mtg/steps/purchase-agreement/PurchaseAgreementPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Purchase Agreement' }
        },
        // Portfolio acquisition (APEX App 105): master and reference data.
        {
          path: 'mortgage',
          name: 'mtg-master-data',
          component: () => import('@/modules/mtg/pages/MasterDataPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Acquisition' }
        },
        {
          path: 'mortgage/reference-data',
          name: 'mtg-reference-data',
          component: () => import('@/modules/adm/pages/ReferenceDataPage.vue'),
          props: { module: 'MTG' },
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Reference Data' }
        },
        {
          path: 'mortgage/reference-data/:lookupName',
          name: 'mtg-lookup',
          component: () => import('@/modules/adm/pages/LookupPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Reference Data' }
        },
        // General ledger (APEX App 120).
        {
          path: 'gl',
          name: 'gl-home',
          component: () => import('@/modules/gl/apex/P0001Home.vue'),
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'General Ledger' }
        },
        {
          path: 'gl/account-structure',
          name: 'gl-account-structure',
          component: () => import('@/modules/gl/apex/P0040AccountStructure.vue'),
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Account Structure & Segments' }
        },
        {
          path: 'gl/chart-of-accounts',
          name: 'gl-chart-of-accounts',
          component: () => import('@/modules/gl/apex/P0002ChartOfAccounts.vue'),
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Chart of Accounts' }
        },
        {
          path: 'gl/accounting-periods',
          name: 'gl-accounting-periods',
          component: () => import('@/modules/gl/apex/P0030AccountingPeriods.vue'),
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Accounting Periods' }
        },
        {
          path: 'gl/ledgers',
          name: 'gl-ledgers',
          component: () => import('@/modules/gl/pages/LedgersPage.vue'),
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Ledgers' }
        },
        {
          path: 'gl/journals',
          name: 'gl-journals',
          component: () => import('@/modules/gl/apex/P0051Journals.vue'),
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Journals' }
        },
        {
          path: 'gl/journal-entry',
          name: 'gl-journal-entry',
          component: () => import('@/modules/gl/apex/P0050JournalEntry.vue'),
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Journal Entry' }
        },
        {
          path: 'gl/account-ledger',
          name: 'gl-account-ledger',
          component: () => import('@/modules/gl/apex/P0052AccountLedger.vue'),
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Account Ledger' }
        },
        {
          path: 'gl/account-balances',
          name: 'gl-account-balances',
          component: () => import('@/modules/gl/pages/AccountBalancesPage.vue'),
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Account Balances' }
        },
        {
          path: 'gl/account-details',
          name: 'gl-p3',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 3 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Account Details' }
        },
        {
          path: 'gl/event-sources',
          name: 'gl-p600',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 600 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Accounting Event Catalogue' }
        },
        {
          path: 'gl/event-source',
          name: 'gl-p601',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 601 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Accounting Event Details' }
        },
        {
          path: 'gl/accounting-events',
          name: 'gl-p605',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 605 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Accounting Events' }
        },
        {
          path: 'gl/accounting-event',
          name: 'gl-p606',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 606 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Accounting Event Details' }
        },
        {
          path: 'gl/source-values',
          name: 'gl-p610',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 610 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Source value Definitions' }
        },
        {
          path: 'gl/source-value',
          name: 'gl-p611',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 611 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Source value Details' }
        },
        {
          path: 'gl/rule-sets',
          name: 'gl-p615',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 615 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Accounting Rule Sets' }
        },
        {
          path: 'gl/rule-set',
          name: 'gl-p616',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 616 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Accounting Rule Details' }
        },
        {
          path: 'gl/rule-lines',
          name: 'gl-p617',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 617 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Accounting Rule Lines' }
        },
        {
          path: 'gl/generated-entries',
          name: 'gl-p620',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 620 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Auto Generated GL Entries' }
        },
        {
          path: 'gl/generated-entry',
          name: 'gl-p621',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 621 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Generated GL Entries Details' }
        },
        {
          path: 'gl/generated-entry-lines',
          name: 'gl-p622',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 622 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Auto Generated GL Entry Lines' }
        },
        {
          path: 'gl/generated-entry-line',
          name: 'gl-p623',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 623 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'GL Entry Line Details' }
        },
        {
          path: 'gl/rule-line',
          name: 'gl-p718',
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 718 },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Accounting Rule Line Details' }
        },
        {
          path: 'gl/reference-data',
          name: 'gl-reference-data',
          component: () => import('@/modules/adm/pages/ReferenceDataPage.vue'),
          props: { module: 'GL' },
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Reference Data' }
        },
        {
          path: 'gl/reference-data/:lookupName',
          name: 'gl-lookup',
          component: () => import('@/modules/adm/pages/LookupPage.vue'),
          props: true,
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Reference Data' }
        },
        // Administration: global settings shared by every application.
        {
          path: 'admin',
          name: 'adm-home',
          component: () => import('@/modules/adm/pages/AdministrationPage.vue'),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Administration' }
        },
        {
          path: 'admin/reference-data',
          name: 'adm-reference-data',
          component: () => import('@/modules/adm/pages/ReferenceDataPage.vue'),
          props: { module: 'ADM' },
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Reference Data' }
        },
        {
          path: 'admin/reference-data/:lookupName',
          name: 'adm-lookup',
          component: () => import('@/modules/adm/pages/LookupPage.vue'),
          props: true,
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Reference Data' }
        },
        {
          path: 'mortgage/developers',
          name: 'mtg-developers',
          component: () => import('@/modules/mtg/pages/DevelopersPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Developers' }
        },
        {
          path: 'mortgage/developer-signatories',
          name: 'mtg-developer-signatories',
          component: () => import('@/modules/mtg/pages/DeveloperSignatoriesPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Developer Signatories' }
        },
        {
          path: 'mortgage/projects',
          name: 'mtg-projects',
          component: () => import('@/modules/mtg/pages/ProjectsPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Projects' }
        },
        {
          path: 'mortgage/project-units',
          name: 'mtg-project-units',
          component: () => import('@/modules/mtg/pages/ProjectUnitsPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Project Units' }
        },
        {
          path: 'mortgage/investors',
          name: 'mtg-investors',
          component: () => import('@/modules/mtg/pages/InvestorsPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Investors' }
        }
      ]
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/'
    }
  ]
})

router.beforeEach(to => {
  const signedIn = isAuthenticated()

  if (!to.meta.public && !signedIn) return { name: 'login', query: { redirect: to.fullPath } }
  if (to.name === 'login' && signedIn) return { name: 'home' }

  return true
})

export default router
