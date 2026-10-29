<template>
  <!-- App 100 (FINOVA launcher) page 2 "Admin Setup Main", region "Admin Setup": the same markup and CSS,
       with the card opening this app's users and roles pages instead of APEX application 103. -->
  <div class="fm">
    <section class="fm-hero">
      <div class="fm-hero-l">
        <span class="fm-eyebrow"><i></i>{{ tr('CAPITRA · Admin Setup') }}</span>
        <h1>{{ tr('Admin Setup') }}</h1>
        <p>{{ tr('Platform administration for all CAPITRA applications. Choose a module to continue.') }}</p>
      </div>
      <div class="fm-kpis">
        <div><b>{{ activeUsers ?? '–' }}</b><span>{{ tr('Active users') }}</span></div>
        <div><b>{{ roles ?? '–' }}</b><span>{{ tr('Roles') }}</span></div>
        <div><b>{{ today }}</b><span>{{ tr('Today') }}</span></div>
      </div>
    </section>

    <!-- the setup areas of FINOVA_FINANCEING's AdminHomePage that have a page in this app -->
    <div class="fm-sec"><h2>{{ tr('Administration modules') }}</h2></div>
    <div class="fm-grid fm-grid--1">
      <article v-for="sec in sections" :key="sec.title" class="fm-card fm-card--t2 ad-sec" :class="{ 'is-off': !live(sec) }" :style="{ '--c': sec.color, '--cl': sec.tint }">
        <div class="fm-top">
          <span class="fm-ico"><i :class="sec.icon"></i></span>
          <span class="fm-pill" :class="live(sec) ? 'fm-pill--live' : 'fm-pill--soon'">{{ tr(live(sec) ? 'Live' : 'Coming soon') }}</span>
        </div>
        <div class="fm-body"><h3>{{ tr(sec.title) }}</h3><p>{{ tr(sec.description) }}</p></div>
        <div class="ad-links">
          <component :is="link(item) ? 'router-link' : 'div'" v-for="item in sec.items" :key="item.title" :to="link(item) ? item.route : undefined" class="ad-link"
                     :class="{ 'is-off': !link(item) }" :aria-disabled="link(item) ? undefined : 'true'">
            <i :class="item.icon" class="ad-li"></i>
            <span class="ad-lc"><b>{{ tr(item.title) }}</b><small>{{ tr(item.description) }}</small></span>
            <i v-if="link(item)" class="pi pi-chevron-right ad-arr"></i>
            <span v-else-if="item.route" class="ad-soon ad-noperm"><i class="pi pi-lock"></i> {{ tr('No permission') }}</span>
            <span v-else class="ad-soon">{{ tr('Coming soon') }}</span>
          </component>
        </div>
        <div v-if="sec.viewAll" class="fm-foot"><router-link :to="sec.viewAll" class="fm-go">{{ tr('View all lookups') }}<span v-html="icon(ARROW, 16)"></span></router-link></div>
      </article>
    </div>
    <router-link class="fm-back" to="/"><span v-html="icon(BACK, 16)"></span>{{ tr('All applications') }}</router-link>
  </div>
</template>

<script setup lang="ts">
import { language, tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import { onMounted, ref } from 'vue'
import { getAdminUsers } from '@/modules/sec/services/adminUserService'
import { getAdminRoles } from '@/modules/sec/services/adminRoleService'

const ARROW = '<path d="M5 12h14M13 5l7 7-7 7"/>'
const BACK = '<path d="M19 12H5M11 19l-7-7 7-7"/>'
// the APEX page's svg() function
const icon = (paths: string, size = 24) =>
  `<svg width="${size}" height="${size}" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.9" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${paths}</svg>`

const ar = language.value === 'AR'

type SetupItem = { title: string; description: string; icon: string; route?: string; perm?: string }
type SetupSection = { title: string; description: string; icon: string; color: string; tint: string; items: SetupItem[]; viewAll?: string }

// FINOVA_FINANCEING AdminHomePage: its sections and items in its order. An item opens this app's page when it has one;
// the rest are marked Coming soon.
const sections: SetupSection[] = [
  {
    title: 'Product Setup', description: 'Configure products, events and product processing rules.', icon: 'pi pi-box', color: '#1D5BBF', tint: '#E8F1FF',
    items: [
      { title: 'Product Catalog', description: 'Maintain product catalog and product definitions.', icon: 'pi pi-box', route: '/admin/products', perm: 'ADM.PRODUCT.VIEW' },
      { title: 'Product Event Rules', description: 'Define product events and processing rules.', icon: 'pi pi-sitemap', route: '/admin/product-event-rules', perm: 'ADM.PRODUCT.VIEW' },
      { title: 'Business Events', description: 'Maintain business events used by product event rules.', icon: 'pi pi-bolt', route: '/gl/accounting-events', perm: 'GL.SETUP.VIEW' },
      { title: 'Product Parameters', description: 'Maintain product-level configuration and defaults.', icon: 'pi pi-sliders-h', route: '/admin/products/mortgage-portfolio-purchasing', perm: 'ADM.PRODUCT.VIEW' }
    ]
  },
  {
    title: 'Charge Setup', description: 'Maintain charge definitions, types and pricing configuration.', icon: 'pi pi-wallet', color: '#14734F', tint: '#E7F6EF',
    items: [
      { title: 'Charge Components', description: 'Maintain reusable charge components.', icon: 'pi pi-wallet', route: '/admin/charges', perm: 'ADM.PRODUCT.VIEW' },
      { title: 'Charge Component Types', description: 'Maintain charge component classifications.', icon: 'pi pi-tags', route: '/admin/reference-data/charge-type' },
      { title: 'Charge Parameters', description: 'Configure charge defaults and calculation settings.', icon: 'pi pi-percentage' }
    ]
  },
  {
    title: 'Accounting Setup', description: 'Maintain GL, posting and accounting period configuration.', icon: 'pi pi-book', color: '#B4570E', tint: '#FFF1E6',
    items: [
      { title: 'GL Account Setup', description: 'Maintain chart of accounts and account mappings.', icon: 'pi pi-book', route: '/gl/chart-of-accounts', perm: 'GL.SETUP.VIEW' },
      { title: 'Accounting Periods', description: 'Define and manage accounting periods.', icon: 'pi pi-calendar', route: '/gl/accounting-periods', perm: 'GL.SETUP.VIEW' },
      { title: 'Posting Configuration', description: 'Maintain posting rules and accounting settings.', icon: 'pi pi-cog', route: '/gl/rule-sets', perm: 'GL.SETUP.VIEW' }
    ]
  },
  {
    title: 'Customer Setup', description: 'Maintain customer classifications and configuration.', icon: 'pi pi-users', color: '#5B5BD6', tint: '#EEEEFC',
    items: [
      { title: 'Customer Types', description: 'Maintain customer types and classifications.', icon: 'pi pi-user' },
      { title: 'Customer Segments', description: 'Manage customer segments and criteria.', icon: 'pi pi-users' },
      { title: 'Customer Parameters', description: 'Configure customer-level defaults and rules.', icon: 'pi pi-sliders-h' }
    ]
  },
  {
    title: 'Operations Setup', description: 'Configure operational settings and workflow parameters.', icon: 'pi pi-cog', color: '#00636D', tint: '#E6F3F4',
    items: [
      { title: 'Locations', description: 'Maintain branches, locations and operational units.', icon: 'pi pi-map-marker' },
      { title: 'Transaction Rules', description: 'Configure transaction processing rules.', icon: 'pi pi-arrow-right-arrow-left' },
      { title: 'Operational Parameters', description: 'Maintain operational parameters and defaults.', icon: 'pi pi-sliders-h' }
    ]
  },
  {
    title: 'Reference Data / Lookups', description: 'Maintain common reference data used throughout CAPITRA.', icon: 'pi pi-list', color: '#0E7490', tint: '#E0F2F7',
    items: [
      { title: 'Currencies', description: 'Maintain supported currencies and currency information.', icon: 'pi pi-money-bill', route: '/admin/reference-data/currencies' },
      { title: 'Countries', description: 'Maintain country codes and country information.', icon: 'pi pi-globe', route: '/admin/reference-data/country' },
      { title: 'Client Roles', description: 'Maintain customer business roles such as Real Estate Developer and Investor.', icon: 'pi pi-id-card', route: '/admin/reference-data/client-role' }
    ],
    viewAll: '/admin/reference-data'
  },
  {
    title: 'System Setup', description: 'Manage system configuration and application parameters.', icon: 'pi pi-desktop', color: '#014B55', tint: '#E6F0F1',
    items: [
      { title: 'User Management', description: 'Maintain system users and access configuration.', icon: 'pi pi-users', route: '/admin/users', perm: 'SEC.USER.ADMIN' },
      { title: 'Role Management', description: 'Configure roles and application permissions.', icon: 'pi pi-shield', route: '/admin/roles', perm: 'SEC.USER.ADMIN' },
      { title: 'System Parameters', description: 'Maintain application parameters.', icon: 'pi pi-cog' }
    ]
  }
]
const live = (sec: SetupSection) => sec.items.some((i) => i.route)
// the item opens only for a user with its permission (the API checks it again); otherwise it says so
const allowed = (item: SetupItem) => !item.perm || hasPermission(item.perm)
const link = (item: SetupItem) => !!item.route && allowed(item)
const today = new Date().toLocaleDateString(ar ? 'ar-EG-u-nu-latn' : undefined, { day: 'numeric', month: 'short' })
const activeUsers = ref<number | null>(null)
const roles = ref<number | null>(null)

onMounted(() => {
  getAdminUsers().then((u) => { activeUsers.value = u.filter((x) => x.isActive).length }).catch(() => {})
  getAdminRoles().then((r) => { roles.value = r.length }).catch(() => {})
})
</script>

<style scoped>
/* FINOVA's setup links, in the launcher card style */
.ad-sec .fm-ico i { font-size: 20px; }
.ad-links { display: flex; flex-direction: column; gap: 8px; }
.ad-link { display: flex; align-items: center; gap: 12px; padding: 11px 12px; border-radius: 12px; border: 1px solid var(--fm-line); background: #fff; color: var(--fm-ink) !important; transition: border-color .15s, background .15s; }
.ad-link:hover { border-color: color-mix(in srgb, var(--c) 45%, #DFE8EC); background: var(--fm-soft); }
.ad-li { width: 34px; height: 34px; border-radius: 10px; display: grid; place-items: center; flex: 0 0 auto; color: var(--c); background: var(--cl); font-size: 15px; }
.ad-lc { flex: 1; min-width: 0; }
.ad-lc b { display: block; font-size: 14px; font-weight: 700; }
.ad-lc small { display: block; margin-top: 2px; font-size: 12.5px; color: var(--fm-muted); line-height: 1.4; }
.ad-arr { color: var(--fm-muted); font-size: 12px; }
.ad-link.is-off { background: #FBFCFD; cursor: default; }
.ad-link.is-off:hover { border-color: var(--fm-line); background: #FBFCFD; }
.ad-link.is-off .ad-li, .ad-link.is-off .ad-lc { filter: grayscale(.55); opacity: .8; }
.ad-soon { flex: 0 0 auto; font-size: 11.5px; font-weight: 700; color: #9A4F08; background: #FFF4E5; border-radius: 99px; padding: 3px 9px; white-space: nowrap; }
/* a link the user has no permission for */
.ad-noperm { color: #C8321F; background: #FDECEA; }
.ad-noperm i { font-size: 10px; }
html[dir=rtl] .ad-arr { transform: scaleX(-1); }
</style>
