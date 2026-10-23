import { createRouter, createWebHistory } from 'vue-router'
import AppLayout from '@/shared/layout/AppLayout.vue'
import { isAuthenticated } from '@/shared/auth/session'

// Every page lives under AppLayout and needs a signed-in user, except the login page.
// Module routes are added here as each module is built (src/modules/{module}).
const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/modules/sec/pages/LoginPage.vue'),
      meta: { public: true, moduleName: 'CAPITRA', functionName: 'Login' }
    },
    {
      path: '/',
      component: AppLayout,
      children: [
        {
          path: '',
          name: 'home',
          component: () => import('@/modules/home/pages/HomePage.vue'),
          meta: { moduleName: 'HOME', functionName: 'My workspace' }
        },
        // Portfolio acquisition (APEX App 105): portfolio cases.
        {
          path: 'mortgage/portfolios',
          name: 'mtg-portfolios',
          component: () => import('@/modules/mtg/pages/PortfolioCasesPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Cases' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId',
          name: 'mtg-portfolio',
          component: () => import('@/modules/mtg/pages/PortfolioWorkspacePage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio' }
        },
        {
          path: 'mortgage/portfolios/:portfolioId/payments',
          name: 'mtg-portfolio-payments',
          component: () => import('@/modules/mtg/pages/PaymentHistoryPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Payment History' }
        },
        // Portfolio acquisition (APEX App 105): master and reference data.
        {
          path: 'mortgage',
          name: 'mtg-master-data',
          component: () => import('@/modules/mtg/pages/MasterDataPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Portfolio Acquisition' }
        },
        {
          path: 'mortgage/reference-data',
          name: 'adm-reference-data',
          component: () => import('@/modules/adm/pages/ReferenceDataPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Reference Data' }
        },
        {
          path: 'mortgage/reference-data/:lookupName',
          name: 'adm-lookup',
          component: () => import('@/modules/adm/pages/LookupPage.vue'),
          props: true,
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Reference Data' }
        },
        {
          path: 'mortgage/developers',
          name: 'mtg-developers',
          component: () => import('@/modules/mtg/pages/DevelopersPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Developers' }
        },
        {
          path: 'mortgage/developer-signatories',
          name: 'mtg-developer-signatories',
          component: () => import('@/modules/mtg/pages/DeveloperSignatoriesPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Developer Signatories' }
        },
        {
          path: 'mortgage/projects',
          name: 'mtg-projects',
          component: () => import('@/modules/mtg/pages/ProjectsPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Projects' }
        },
        {
          path: 'mortgage/project-units',
          name: 'mtg-project-units',
          component: () => import('@/modules/mtg/pages/ProjectUnitsPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Project Units' }
        },
        {
          path: 'mortgage/investors',
          name: 'mtg-investors',
          component: () => import('@/modules/mtg/pages/InvestorsPage.vue'),
          meta: { moduleName: 'PORTFOLIO ACQUISITION', functionName: 'Investors' }
        }
      ]
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/'
    }
  ]
})

router.beforeEach(to => {
  const signedIn = isAuthenticated()

  if (!to.meta.public && !signedIn) return { name: 'login', query: { redirect: to.fullPath } }
  if (to.name === 'login' && signedIn) return { name: 'home' }

  return true
})

export default router
