// The pages of App 122 (Cheques Wallet): /cheque-wallet is page 1, /cheque-wallet/p<page> every other page,
// named chq-<page>.
import type { RouteRecordRaw } from 'vue-router'
import { appPages } from './apex/pages'

export const chqRoutes: RouteRecordRaw[] = appPages.map((p) => ({
  path: p.page === 1 ? 'cheque-wallet' : `cheque-wallet/p${p.page}`,
  name: `chq-${p.page}`,
  component: () => import('./apex/ChqPage.vue'),
  props: { page: p.page },
  meta: { moduleName: 'CHEQUES WALLET', functionName: p.title }
}))
