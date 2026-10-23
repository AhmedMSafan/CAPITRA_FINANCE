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
