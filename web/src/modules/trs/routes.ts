// Treasury (redesigned from FINOVA app 210, agreed with Ahmed 5 Nov 2026): its Home card opens Bank Limits, each bank's facilities as a
// tree of tranches and sublimits with the bank's rules.
import type { RouteRecordRaw } from 'vue-router'

const meta = (functionName: string) => ({ moduleName: 'TREASURY', functionName })

export const trsRoutes: RouteRecordRaw[] = [
  { path: 'trs', redirect: { name: 'trs-bank-limits' } },
  { path: 'trs/bank-limits', name: 'trs-bank-limits', component: () => import('./pages/TrsBankLimitsPage.vue'), meta: meta('Bank Limits') },
  // the Treasury Disburse step of DISBURSEMENT_REQUEST_WF opens here (BPMN route trs-disburse, disbursementId = ENTITY_ID)
  {
    path: 'trs/disbursements/:disbursementId(\\d+)/funding', name: 'trs-disburse', component: () => import('./pages/TrsDisbursePage.vue'),
    meta: meta('Treasury Disburse')
  }
]
