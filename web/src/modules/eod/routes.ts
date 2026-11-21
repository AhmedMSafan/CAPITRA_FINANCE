// End of Day: the night batch of every application (Retail Lending, Screening ...), run by an administrator, and its jobs.
import type { RouteRecordRaw } from 'vue-router'

const meta = (functionName: string) => ({ moduleName: 'END OF DAY', functionName })

export const eodRoutes: RouteRecordRaw[] = [
  { path: 'eod', name: 'eod-home', component: () => import('./pages/EodPage.vue'), meta: meta('End of Day') },
  { path: 'eod/jobs', name: 'eod-jobs', component: () => import('./pages/EodJobsPage.vue'), meta: meta('Jobs') }
]
