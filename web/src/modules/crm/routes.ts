// CRM (FINOVA_FINANCEING crm module): the CRM home "Customers".
import type { RouteRecordRaw } from 'vue-router'

export const crmRoutes: RouteRecordRaw[] = [
  {
    path: 'crm',
    name: 'crm-home',
    component: () => import('./pages/CrmHomePage.vue'),
    meta: { moduleName: 'CRM', functionName: 'CRM Module' }
  }
]
