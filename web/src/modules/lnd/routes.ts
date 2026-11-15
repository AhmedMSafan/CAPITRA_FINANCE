// Retail Lending (microfinance and consumer finance, new development agreed with Ahmed 4 Nov 2026). Its first page is Parameters:
// the legal figures and policy values the lending engine reads, each with an effective date and a second user's approval.
import type { RouteRecordRaw } from 'vue-router'

const meta = (functionName: string) => ({ moduleName: 'RETAIL LENDING', functionName })

export const lndRoutes: RouteRecordRaw[] = [
  { path: 'lnd', redirect: { name: 'lnd-workspace' } },
  // the Lending Workspace, as the Factoring Workspace: the customers, then one customer's loans and payments
  { path: 'lending', name: 'lnd-workspace', component: () => import('./pages/LendingWorkspacePage.vue'), meta: meta('Lending Workspace') },
  {
    path: 'lending/customer/:clientId(\\d+)', name: 'lnd-customer', component: () => import('./pages/LendingCustomerWorkspace.vue'),
    props: (r) => ({ clientId: Number(r.params.clientId) }), meta: meta('Lending Customer Workspace')
  },
  { path: 'lnd/parameters', name: 'lnd-parameters', component: () => import('./pages/LndParametersPage.vue'), meta: meta('Lending Parameters') },
  { path: 'lnd/eod', name: 'lnd-eod', component: () => import('./pages/LndEodPage.vue'), meta: meta('End of Day') },
  { path: 'lnd/loans', name: 'lnd-loans', component: () => import('./pages/LndLoansPage.vue'), meta: meta('Loans') },
  {
    path: 'lnd/loans/:loanId(\\d+)', name: 'lnd-loan', component: () => import('./pages/LndLoanPage.vue'),
    props: (r) => ({ loanId: Number(r.params.loanId) }), meta: meta('Loan')
  }
]
