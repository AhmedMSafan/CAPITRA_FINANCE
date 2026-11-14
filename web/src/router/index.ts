import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '@/shared/layout/AppLayout.vue'
import { isAuthenticated } from '@/shared/auth/session'
import { mtgRoutes } from '@/modules/mtg/routes'
import { mffRoutes } from '@/modules/mff/routes'
import { chqRoutes } from '@/modules/chq/routes'
import { crmRoutes } from '@/modules/crm/routes'
import { fctRoutes } from '@/modules/fct/routes'
import { collectionRoutes } from '@/modules/collection/routes'
import { trsRoutes } from '@/modules/trs/routes'
import { lndRoutes } from '@/modules/lnd/routes'

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
        // the Mortgage card of Home: the same page with the three mortgage applications
        {
          path: 'mortgage-applications',
          name: 'home-mortgage',
          component: () => import('@/modules/home/pages/HomePage.vue'),
          props: { group: 'mortgage' },
          meta: { moduleName: 'HOME', functionName: 'Mortgage' }
        },
        // the Collection and Repayments card of Home: the same page with App 136's two functions
        {
          path: 'collection-repayments',
          name: 'home-collection',
          component: () => import('@/modules/home/pages/HomePage.vue'),
          props: { group: 'collection' },
          meta: { moduleName: 'HOME', functionName: 'Collection and Repayments' }
        },
        // the Retail Lending card of Home: the same page with the lending cards
        {
          path: 'retail-lending',
          name: 'home-lending',
          component: () => import('@/modules/home/pages/HomePage.vue'),
          props: { group: 'lending' },
          meta: { moduleName: 'HOME', functionName: 'Retail Lending' }
        },
        // the CRM card of Home opens Customers
        { path: 'crm', name: 'home-crm', redirect: { name: 'crm-customers' } },
        // Mortgage (APEX App 105).
        ...mtgRoutes,
        // Mortgage Finance (APEX App 110).
        ...mffRoutes,
        ...chqRoutes,
        // CRM and Factoring (FINOVA_FINANCEING).
        ...crmRoutes,
        ...fctRoutes,
        // Collection and Repayments (APEX App 136).
        ...collectionRoutes,
        // Treasury: bank limits.
        ...trsRoutes,
        // Retail Lending: microfinance and consumer finance.
        ...lndRoutes,
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
          component: () => import('@/modules/gl/apex/wizard/GlWizardPage.vue'),
          props: { page: 20 },
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
          path: 'gl/financial-statements',
          name: 'gl-financial-statements',
          component: () => import('@/modules/gl/apex/P0056FinancialStatements.vue'),
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'Financial Statements' }
        },
        {
          path: 'gl/help',
          name: 'gl-help',
          component: () => import('@/modules/gl/apex/P0090Help.vue'),
          meta: { moduleName: 'GENERAL LEDGER', functionName: 'General Ledger Help' }
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
        // Module settings: each module's rules, one tab per module; a change needs a second user's approval
        {
          path: 'admin/module-settings',
          name: 'adm-module-settings',
          component: () => import('@/modules/adm/pages/ModuleSettingsPage.vue'),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Module Settings' }
        },
        // Product and charge setup (FINOVA_FINANCEING Admin Setup)
        {
          path: 'admin/products',
          name: 'adm-products',
          component: () => import('@/modules/adm/pages/ProductsPage.vue'),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Product Catalog' }
        },
        {
          path: 'admin/products/mortgage-portfolio-purchasing',
          name: 'adm-product-mpp',
          component: () => import('@/modules/adm/pages/MortgagePortfolioPurchasingProductPage.vue'),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Product Parameters' }
        },
        // a Microfinance or Consumer Finance product's versioned terms: its Lending Terms row action in the Product Catalog
        {
          path: 'admin/products/:productId(\\d+)/lending-terms',
          name: 'adm-product-lending-terms',
          component: () => import('@/modules/adm/pages/LendingProductTermsPage.vue'),
          props: route => ({ lendingProductId: Number(route.params.productId) }),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Lending Terms' }
        },
        { path: 'admin/products/lending', redirect: { name: 'adm-products' } },
        {
          path: 'admin/products/:productId(\\d+)/actions',
          name: 'adm-product-actions',
          component: () => import('@/modules/adm/pages/ProductActionsPage.vue'),
          props: route => ({ productId: Number(route.params.productId) }),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Product Actions' }
        },
        {
          path: 'admin/charges',
          name: 'adm-charges',
          component: () => import('@/modules/adm/pages/ChargesPage.vue'),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Charges' }
        },
        {
          path: 'admin/rate-indexes',
          name: 'adm-rate-indexes',
          component: () => import('@/modules/adm/pages/RateIndexesPage.vue'),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Rate Maintenance' }
        },
        {
          path: 'admin/product-event-rules',
          name: 'adm-product-event-rules',
          component: () => import('@/modules/adm/pages/ProductEventRulesPage.vue'),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Product Event Rules' }
        },
        {
          path: 'admin/reference-data',
          name: 'adm-reference-data',
          component: () => import('@/modules/adm/pages/ReferenceDataPage.vue'),
          props: { module: 'ADM' },
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Reference Data' }
        },
        {
          path: 'admin/reference-data/currencies',
          name: 'adm-currencies',
          component: () => import('@/modules/adm/pages/CurrencyPage.vue'),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Reference Data' }
        },
        {
          path: 'admin/reference-data/:lookupName',
          name: 'adm-lookup',
          component: () => import('@/modules/adm/pages/LookupPage.vue'),
          props: true,
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Reference Data' }
        },
        // Workflow engine (C# BPMN, from FINOVA_FINANCEING): it replaces the APEX workflow engine (App 118).
        {
          path: 'workflowengine',
          name: 'workflow-engine',
          component: () => import('@/modules/wfe/pages/WorkflowHomePage.vue'),
          meta: { moduleName: 'WORKFLOW ENGINE', functionName: 'Home' }
        },
        {
          path: 'workflowengine/registry',
          name: 'workflow-registry',
          component: () => import('@/modules/wfe/pages/WorkflowDefinitionsPage.vue'),
          meta: { moduleName: 'WORKFLOW ENGINE', functionName: 'Workflow Definitions' }
        },
        {
          path: 'workflowengine/designer/:id',
          name: 'workflow-engine-designer',
          component: () => import('@/modules/wfe/pages/WorkflowDesignerPage.vue'),
          props: true,
          meta: { moduleName: 'WORKFLOW ENGINE', functionName: 'Workflow Designer' }
        },
        {
          path: 'workflowengine/forms',
          name: 'workflow-forms',
          component: () => import('@/modules/wfe/pages/WorkflowFormsPage.vue'),
          meta: { moduleName: 'WORKFLOW ENGINE', functionName: 'Document Designer' }
        },
        {
          path: 'workflowengine/monitor',
          name: 'workflow-monitor',
          component: () => import('@/modules/wfe/pages/WorkflowMonitorPage.vue'),
          meta: { moduleName: 'WORKFLOW ENGINE', functionName: 'Process monitor' }
        },
        {
          path: 'workflowengine/data-sources',
          name: 'workflow-data-sources',
          component: () => import('@/modules/wfe/pages/WorkflowDataSourcesPage.vue'),
          meta: { moduleName: 'WORKFLOW ENGINE', functionName: 'Data Sources' }
        },
        {
          path: 'admin/users',
          name: 'admin-users',
          component: () => import('@/modules/sec/pages/UserManagementPage.vue'),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'User Management' }
        },
        {
          path: 'admin/roles',
          name: 'admin-roles',
          component: () => import('@/modules/sec/pages/RoleManagementPage.vue'),
          meta: { moduleName: 'ADMINISTRATION', functionName: 'Role Management' }
        },
        {
          path: 'workflow/inbox',
          name: 'workflow-inbox',
          component: () => import('@/modules/wfe/pages/WorkflowInboxPage.vue'),
          meta: { moduleName: 'WORKFLOW', functionName: 'My Workflow Inbox' }
        },
        {
          path: 'workflow/start',
          name: 'workflow-start',
          component: () => import('@/modules/wfe/pages/WorkflowStartPage.vue'),
          meta: { moduleName: 'WORKFLOW', functionName: 'Start a request' }
        },
        {
          path: 'workflow/out-of-office',
          name: 'workflow-out-of-office',
          component: () => import('@/modules/wfe/pages/WorkflowOutOfOfficePage.vue'),
          meta: { moduleName: 'WORKFLOW', functionName: 'Out of Office' }
        },
        {
          path: 'workflow/tasks/:inboxId',
          name: 'workflow-task',
          component: () => import('@/modules/wfe/pages/WorkflowTaskPage.vue'),
          props: true,
          meta: { moduleName: 'WORKFLOW', functionName: 'Workflow Task' }
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
