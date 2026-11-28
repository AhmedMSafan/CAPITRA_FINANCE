// The pages of App 110 (Mortgage Finance): /mortgage-finance is page 1, /mortgage-finance/p<page> every other page,
// named mff-<page>.
import type { RouteRecordRaw } from 'vue-router'
import { appPages } from './apex/pages'

export const mffRoutes: RouteRecordRaw[] = appPages.map((p) => ({
  path: p.page === 1 ? 'mortgage-finance' : `mortgage-finance/p${p.page}`,
  name: `mff-${p.page}`,
  component: () => import('./apex/MffPage.vue'),
  props: { page: p.page },
  meta: { moduleName: 'MORTGAGE FINANCE', functionName: p.title }
}))
