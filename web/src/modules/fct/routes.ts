// Factoring (FINOVA_FINANCEING factoring module). The route names are FINOVA's, so the workflow steps open these pages
// by the routes stored in the FACTORING_LIMIT_APPROVAL definition.
import type { RouteRecordRaw } from 'vue-router'

export const fctRoutes: RouteRecordRaw[] = [
  {
    path: 'factoring',
    name: 'factoring-home',
    component: () => import('./pages/FactoringHomePage.vue'),
    meta: { moduleName: 'FACTORING', functionName: 'Factoring Module' }
  },
  {
    path: 'factoring/customer/:clientId',
    name: 'FactoringCustomerWorkspace',
    component: () => import('./pages/FactoringCustomerWorkspace.vue'),
    props: true,
    meta: { moduleName: 'FACTORING', functionName: 'Factoring Customer Workspace' }
  },
  {
    path: 'factoring/calculator/:clientId',
    name: 'FactoringCalculator',
    component: () => import('./pages/FactoringCalculatorPage.vue'),
    props: true,
    meta: { moduleName: 'FACTORING', functionName: 'Factoring Calculator' }
  },
  // FINOVA gives this step the same path as Sales Review; it has its own here so the two never shadow each other
  {
    path: 'factoring/agreements/:agreementId/assign-sales',
    name: 'factoring-agreement-assign-sales',
    component: () => import('./pages/FactoringAgreementAssignSalesPage.vue'),
    props: true,
    meta: { moduleName: 'FACTORING', functionName: 'Operation Received' }
  },
  {
    path: 'factoring/agreements/:agreementId/review',
    name: 'factoring-agreement-sales-review',
    component: () => import('./pages/FactoringAgreementSalesReviewPage.vue'),
    props: true,
    meta: { moduleName: 'FACTORING', functionName: 'Sales Review' }
  },
  {
    path: 'factoring/agreements/:agreementId/inquiry-field-report',
    name: 'factoring-inquiry-field-report',
    component: () => import('./pages/FactoringInquiryFieldReportPage.vue'),
    props: true,
    meta: { moduleName: 'FACTORING', functionName: 'Investigation' }
  },
  {
    path: 'factoring/agreements/:agreementId/credit-analysis',
    name: 'factoring-credit-analysis',
    component: () => import('./pages/FactoringCreditAnalysisPage.vue'),
    props: true,
    meta: { moduleName: 'FACTORING', functionName: 'Credit Analysis' }
  },
  {
    path: 'factoring/agreements/:agreementId/risk-review',
    name: 'factoring-risk-review',
    component: () => import('./pages/FactoringRiskReviewPage.vue'),
    props: true,
    meta: { moduleName: 'FACTORING', functionName: 'Risk Review' }
  },
  {
    path: 'factoring/agreements/:agreementId/committee-decision',
    name: 'factoring-committee-decision',
    component: () => import('./pages/FactoringCommitteeDecisionPage.vue'),
    props: true,
    meta: { moduleName: 'FACTORING', functionName: 'Committee Decision' }
  },
  {
    path: 'factoring/agreements/:agreementId/contract-request',
    name: 'factoring-contract-request',
    component: () => import('./pages/FactoringContractRequestPage.vue'),
    props: true,
    meta: { moduleName: 'FACTORING', functionName: 'Customer Approval' }
  },
  {
    path: 'factoring/agreements/:agreementId/contract-generation',
    name: 'factoring-contract-generation',
    component: () => import('./pages/FactoringContractGenerationPage.vue'),
    props: true,
    meta: { moduleName: 'FACTORING', functionName: 'Contract Generation' }
  },
  {
    path: 'factoring/agreements/:agreementId/legal-review',
    name: 'factoring-legal-review',
    component: () => import('./pages/FactoringLegalReviewPage.vue'),
    props: true,
    meta: { moduleName: 'FACTORING', functionName: 'Legal Review' }
  }
]
