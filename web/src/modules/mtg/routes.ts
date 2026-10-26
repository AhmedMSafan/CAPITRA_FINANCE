// The pages of App 105 (Mortgage): /mortgage is page 1, /mortgage/p<page> every other page, named mtg-<page>
// (the workflow tasks open their step pages by that name).
import type { RouteRecordRaw } from 'vue-router'
import { appPages } from './apex/pages'

export const mtgRoutes: RouteRecordRaw[] = appPages.map((p) => ({
  path: p.page === 1 ? 'mortgage' : `mortgage/p${p.page}`,
  name: `mtg-${p.page}`,
  component: () => import('./apex/MtgPage.vue'),
  props: { page: p.page },
  meta: { moduleName: 'MORTGAGE', functionName: p.title }
}))
