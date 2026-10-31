// CRM (FINOVA_FINANCEING crm module): its Home card opens Customers; Process opens the Customer Workspace, which leads to the
// Factoring Customer Workspace. Customer Disbursement Request (APEX App 150722 pages 750 and 751) with its Invoice Upload step
// (App 148 page 705) is opened from the Factoring Customer Workspace.
import type { RouteRecordRaw } from 'vue-router'

const meta = (functionName: string) => ({ moduleName: 'CRM', functionName })

export const crmRoutes: RouteRecordRaw[] = [
  { path: 'crm/customers', name: 'crm-customers', component: () => import('./pages/CrmHomePage.vue'), meta: meta('Customer Workspace') },
  {
    path: 'crm/customers/:clientId(\\d+)', name: 'crm-customer-workspace', component: () => import('./pages/CustomerWorkspacePage.vue'),
    props: (r) => ({ clientId: Number(r.params.clientId) }), meta: meta('Customer Workspace')
  },
  { path: 'crm/disbursement-requests', name: 'crm-disbursement-requests', component: () => import('./pages/DisbursementRequestsPage.vue'), meta: meta('Disbursement Requests') },
  { path: 'crm/disbursement-requests/new', name: 'crm-disbursement-request-new', component: () => import('./pages/DisbursementRequestPage.vue'), meta: meta('Disbursement Request Detail') },
  {
    path: 'crm/disbursement-requests/:disbursementId(\\d+)', name: 'crm-disbursement-request', component: () => import('./pages/DisbursementRequestPage.vue'),
    props: (r) => ({ disbursementId: Number(r.params.disbursementId) }), meta: meta('Disbursement Request Detail')
  },
  {
    path: 'crm/disbursement-requests/:disbursementId(\\d+)/invoices', name: 'crm-disbursement-invoice-entry', component: () => import('./pages/InvoiceEntryPage.vue'),
    props: (r) => ({ disbursementId: Number(r.params.disbursementId) }), meta: meta('Invoice Entry & Verification')
  }
]
