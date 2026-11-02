// Collection and Repayments (APEX App 136): Customer Payment Sources (pages 5, 7, 6), Cheques Collection (page 712), Collection Batches,
// and Customer Payments (claims of disbursement charges paid from payment sources).
import type { RouteRecordRaw } from 'vue-router'

const meta = (functionName: string) => ({ moduleName: 'COLLECTION AND REPAYMENTS', functionName })

export const collectionRoutes: RouteRecordRaw[] = [
  { path: 'collection-repayments/payment-sources', name: 'collection-repayments', component: () => import('./pages/RepaymentsPage.vue'), meta: meta('Repayment Transactions') },
  { path: 'collection-repayments/payment-sources/new', name: 'collection-bank-transfer', component: () => import('./pages/BankTransferPage.vue'), meta: meta('New Bank Transfer') },
  { path: 'collection-repayments/payment-sources/:repaymentTxnId(\\d+)', name: 'collection-repayment', component: () => import('./pages/RepaymentPage.vue'), props: (r) => ({ repaymentTxnId: Number(r.params.repaymentTxnId) }), meta: meta('Repayment Transactions') },
  { path: 'collection-repayments/cheques', name: 'collection-cheques', component: () => import('./pages/ChequesCollectionPage.vue'), meta: meta('Cheques Collection') },
  { path: 'collection-repayments/batches', name: 'collection-batches', component: () => import('./pages/CollectionBatchesPage.vue'), meta: meta('Collection Batches') },
  { path: 'collection-repayments/batches/:collectionBatchId(\\d+)', name: 'collection-batch', component: () => import('./pages/CollectionBatchPage.vue'), props: (r) => ({ collectionBatchId: Number(r.params.collectionBatchId) }), meta: meta('Collection Batches') },
  { path: 'collection-repayments/customer-payments', name: 'collection-customer-payments', component: () => import('./pages/CustomerPaymentsPage.vue'), meta: meta('Customer Payments') }
]
