// CRM (FINOVA_FINANCEING crm module): its Home card opens Customers; Workspaces opens the Customer Workspace, which leads to the
// Factoring Customer Workspace. There a limit makes a Customer Disbursement Request (APEX App 150722 page 751) and the Disbursements
// tab lists them (page 750); its Invoice Upload step is App 148 page 705.
import type { RouteRecordRaw } from 'vue-router'

const meta = (functionName: string) => ({ moduleName: 'CRM', functionName })

export const crmRoutes: RouteRecordRaw[] = [
  { path: 'crm/customers', name: 'crm-customers', component: () => import('./pages/CrmHomePage.vue'), meta: meta('Customer Workspace') },
  {
    path: 'crm/customers/:clientId(\\d+)', name: 'crm-customer-workspace', component: () => import('./pages/CustomerWorkspacePage.vue'),
    props: (r) => ({ clientId: Number(r.params.clientId) }), meta: meta('Customer Workspace')
  },
  // a new request is made from a limit of the Factoring Customer Workspace, which passes the customer and the limit
  {
    path: 'crm/customers/:clientId(\\d+)/limits/:limitId(\\d+)/disbursement-request', name: 'crm-disbursement-request-new',
    component: () => import('./pages/DisbursementRequestPage.vue'),
    props: (r) => ({ clientId: Number(r.params.clientId), limitId: Number(r.params.limitId) }), meta: meta('Disbursement Request Detail')
  },
  {
    path: 'crm/disbursement-requests/:disbursementId(\\d+)', name: 'crm-disbursement-request', component: () => import('./pages/DisbursementRequestPage.vue'),
    props: (r) => ({ disbursementId: Number(r.params.disbursementId) }), meta: meta('Disbursement Request Detail')
  },
  {
    path: 'crm/disbursement-requests/:disbursementId(\\d+)/invoices', name: 'crm-disbursement-invoice-entry', component: () => import('./pages/InvoiceEntryPage.vue'),
    props: (r) => ({ disbursementId: Number(r.params.disbursementId) }), meta: meta('Invoice Entry & Verification')
  }
]
