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
  }
]
