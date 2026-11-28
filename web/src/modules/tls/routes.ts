// Tools: the Loan Calculator of FINOVA_FINANCEING (Ahmed, 15 Nov 2026).
import type { RouteRecordRaw } from 'vue-router'

export const tlsRoutes: RouteRecordRaw[] = [
  {
    path: 'tools/loan-calculator', name: 'tls-loan-calculator', component: () => import('./pages/LoanCalculatorPage.vue'),
    meta: { moduleName: 'TOOLS', functionName: 'Finance Calculator' }
  }
]
