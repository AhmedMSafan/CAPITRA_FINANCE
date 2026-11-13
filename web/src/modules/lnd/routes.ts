// Retail Lending (microfinance and consumer finance, new development agreed with Ahmed 4 Nov 2026). Its first page is Parameters:
// the legal figures and policy values the lending engine reads, each with an effective date and a second user's approval.
import type { RouteRecordRaw } from 'vue-router'

const meta = (functionName: string) => ({ moduleName: 'RETAIL LENDING', functionName })

export const lndRoutes: RouteRecordRaw[] = [
  { path: 'lnd', redirect: { name: 'lnd-loans' } },
  { path: 'lnd/parameters', name: 'lnd-parameters', component: () => import('./pages/LndParametersPage.vue'), meta: meta('Lending Parameters') },
  { path: 'lnd/loans', name: 'lnd-loans', component: () => import('./pages/LndLoansPage.vue'), meta: meta('Loans') },
  {
    path: 'lnd/loans/:loanId(\\d+)', name: 'lnd-loan', component: () => import('./pages/LndLoanPage.vue'),
    props: (r) => ({ loanId: Number(r.params.loanId) }), meta: meta('Loan')
  }
]
