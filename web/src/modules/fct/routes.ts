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
  }
]
