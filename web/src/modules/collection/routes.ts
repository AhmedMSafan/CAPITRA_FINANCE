// Collection and Repayments (APEX App 136): Customer Payment Sources (pages 5, 7, 6) and Cheques Collection (page 712).
import type { RouteRecordRaw } from 'vue-router'

const meta = (functionName: string) => ({ moduleName: 'COLLECTION AND REPAYMENTS', functionName })

export const collectionRoutes: RouteRecordRaw[] = [
  { path: 'collection-repayments/payment-sources', name: 'collection-repayments', component: () => import('./pages/RepaymentsPage.vue'), meta: meta('Repayment Transactions') },
  { path: 'collection-repayments/payment-sources/new', name: 'collection-bank-transfer', component: () => import('./pages/BankTransferPage.vue'), meta: meta('New Bank Transfer') },
  { path: 'collection-repayments/payment-sources/:repaymentTxnId(\\d+)', name: 'collection-repayment', component: () => import('./pages/RepaymentPage.vue'), props: (r) => ({ repaymentTxnId: Number(r.params.repaymentTxnId) }), meta: meta('Repayment Transactions') },
  { path: 'collection-repayments/cheques', name: 'collection-cheques', component: () => import('./pages/ChequesCollectionPage.vue'), meta: meta('Cheques Collection') }
]
