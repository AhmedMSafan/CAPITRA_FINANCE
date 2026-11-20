// Communication Center: the outgoing mail servers (one in use for every e-mail of the system) and the log of every e-mail sent.
import type { RouteRecordRaw } from 'vue-router'

const meta = (functionName: string) => ({ moduleName: 'COMMUNICATION CENTER', functionName })

export const cmcRoutes: RouteRecordRaw[] = [
  { path: 'communication', name: 'cmc-home', component: () => import('./pages/CmcHomePage.vue'), meta: meta('Communication Center') },
  { path: 'communication/mail-servers', name: 'cmc-mail-servers', component: () => import('./pages/CmcMailServersPage.vue'), meta: meta('Mail servers') },
  { path: 'communication/email-log', name: 'cmc-email-log', component: () => import('./pages/CmcEmailLogPage.vue'), meta: meta('Sent e-mails') }
]
