<template>
  <!-- Administration in the app 118 design: hero with live counts, a search bar and one card per setup area. -->
  <div class="wx ad wx-page">
    <section class="wx-hero">
      <div class="wr-hero">
        <div>
          <span class="wx-eyebrow"><i></i>Capitra setup</span>
          <h1>Administration</h1>
          <p>Users, roles and access, reference data and the configuration every Capitra module runs on. Open an area to maintain it.</p>
        </div>
        <div class="wr-cta">
          <router-link class="wx-btn wx-btn--ghost" to="/admin/roles"><i class="pi pi-shield"></i><span>Roles</span></router-link>
          <router-link class="wx-btn wx-btn--ghost" to="/admin/reference-data"><i class="pi pi-list"></i><span>Reference data</span></router-link>
          <router-link class="wx-btn wx-btn--hot" to="/admin/users"><i class="pi pi-users"></i><span>Users</span></router-link>
        </div>
      </div>
      <div class="wr-kpis">
        <div class="wr-kpi"><b>{{ show(userCount) }}</b><span>Users &middot; <small>{{ show(activeUsers) }} active</small></span></div>
        <div class="wr-kpi"><b>{{ show(roleCount) }}</b><span>Roles</span></div>
        <div class="wr-kpi"><b>{{ show(lookupCount) }}</b><span>Reference data lists</span></div>
        <div class="wr-kpi"><b>{{ liveCount }} <i>/ {{ allItems.length }}</i></b><span>Setup functions available</span></div>
      </div>
    </section>

    <div class="wr-bar" role="search">
      <label class="wr-search">
        <i class="pi pi-search"></i>
        <input v-model="searchText" type="search" placeholder="Search setup areas and functions" aria-label="Search setup" />
      </label>
    </div>

    <section class="ad-grid">
      <article v-for="section in filteredSections" :key="section.title" class="ad-card">
        <div class="ad-head">
          <div class="ad-ico"><i :class="section.icon"></i></div>
          <div>
            <h3>{{ section.title }}</h3>
            <p>{{ section.description }}</p>
          </div>
        </div>
        <div class="ad-links">
          <button v-for="item in section.items" :key="item.title" type="button" class="ad-link" :class="{ 'is-off': !item.route }"
                  :disabled="!item.route" @click="openSetup(item)">
            <i :class="item.icon" class="ad-li"></i>
            <span class="ad-lt"><b>{{ item.title }}</b><small>{{ item.description }}</small></span>
            <i v-if="item.route" class="pi pi-arrow-right ad-go"></i>
            <span v-else class="wx-tag">Planned</span>
          </button>
        </div>
        <div v-if="section.viewAllRoute" class="ad-foot">
          <button type="button" class="ad-all" @click="openViewAll(section)"><span>All reference data</span><i class="pi pi-arrow-right"></i></button>
        </div>
      </article>
    </section>

    <div v-if="filteredSections.length === 0" class="ad-empty">
      <b>No setup functions found</b>
      Try a different search term.
    </div>
  </div>
</template>

<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import { getAdminUsers } from '@/modules/sec/services/adminUserService'
  import { getAdminRoles } from '@/modules/sec/services/adminRoleService'
  import { getLookupDefinitions } from '../api/lookups.api'
  import { useRouter } from 'vue-router'

  const router = useRouter()
  const searchText = ref('')

  type SetupItem = {
    title: string
    description: string
    icon: string
    route?: string
  }

  type SetupSection = {
    title: string
    description: string
    icon: string
    tone: 'blue' | 'green' | 'orange' | 'purple' | 'teal' | 'red'
    items: SetupItem[]
    viewAllRoute?: string
  }

  const sections: SetupSection[] = [
    {
      title: 'Product Setup',
      description: 'Configure products, events and product processing rules.',
      icon: 'pi pi-box',
      tone: 'blue',
      items: [
        {
          title: 'Product Catalog',
          description: 'Maintain product catalog and product definitions.',
          icon: 'pi pi-box'
        },
        {
          title: 'Product Event Rules',
          description: 'Define product events and processing rules.',
          icon: 'pi pi-sitemap'
        },
        {
          title: 'Business Events',
          description: 'Maintain business events used by product event rules.',
          icon: 'pi pi-bolt'
        },
        {
          title: 'Product Parameters',
          description: 'Maintain product-level configuration and defaults.',
          icon: 'pi pi-sliders-h'
        }
      ]
    },
    {
      title: 'Charge Setup',
      description: 'Maintain charge definitions, types and pricing configuration.',
      icon: 'pi pi-wallet',
      tone: 'green',
      items: [
        {
          title: 'Charge Components',
          description: 'Maintain reusable Capitra charge components.',
          icon: 'pi pi-wallet'
        },
        {
          title: 'Charge Component Types',
          description: 'Maintain charge component classifications.',
          icon: 'pi pi-tags'
        },
        {
          title: 'Charge Parameters',
          description: 'Configure charge defaults and calculation settings.',
          icon: 'pi pi-percentage'
        }
      ]
    },
    {
      title: 'Accounting Setup',
      description: 'Maintain GL, posting and accounting period configuration.',
      icon: 'pi pi-book',
      tone: 'orange',
      items: [
        {
          title: 'GL Account Setup',
          description: 'Maintain chart of accounts and account mappings.',
          icon: 'pi pi-book',
          route: '/gl/chart-of-accounts'
        },
        {
          title: 'Accounting Periods',
          description: 'Define and manage accounting periods.',
          icon: 'pi pi-calendar',
          route: '/gl/accounting-periods'
        },
        {
          title: 'Posting Configuration',
          description: 'Maintain posting rules and accounting settings.',
          icon: 'pi pi-cog',
          route: '/gl/rule-sets'
        }
      ]
    },
    {
      title: 'Customer Setup',
      description: 'Maintain customer classifications and configuration.',
      icon: 'pi pi-users',
      tone: 'purple',
      items: [
        {
          title: 'Customer Types',
          description: 'Maintain customer types and classifications.',
          icon: 'pi pi-user'
        },
        {
          title: 'Customer Segments',
          description: 'Manage customer segments and criteria.',
          icon: 'pi pi-users'
        },
        {
          title: 'Customer Parameters',
          description: 'Configure customer-level defaults and rules.',
          icon: 'pi pi-sliders-h'
        }
      ]
    },
    {
      title: 'Operations Setup',
      description: 'Configure operational settings and workflow parameters.',
      icon: 'pi pi-cog',
      tone: 'teal',
      items: [
        {
          title: 'Locations',
          description: 'Maintain branches, locations and operational units.',
          icon: 'pi pi-map-marker'
        },
        {
          title: 'Transaction Rules',
          description: 'Configure transaction processing rules.',
          icon: 'pi pi-arrow-right-arrow-left'
        },
        {
          title: 'Operational Parameters',
          description: 'Maintain operational parameters and defaults.',
          icon: 'pi pi-sliders-h'
        }
      ]
    },
    {
      title: 'Reference Data / Lookups',
      description: 'Maintain common reference data used throughout Capitra.',
      icon: 'pi pi-list',
      tone: 'blue',
      items: [
        
        {
          title: 'Currencies',
          description: 'Maintain supported currencies and currency information.',
          icon: 'pi pi-money-bill',
          route: '/admin/reference-data/currency'
        },
        {
          title: 'Countries',
          description: 'Maintain country codes and country information.',
          icon: 'pi pi-globe',
          route: '/admin/reference-data/country'
        }
      ],
      viewAllRoute: '/admin/reference-data'
    },
    {
      title: 'System Setup',
      description: 'Manage system configuration and application parameters.',
      icon: 'pi pi-desktop',
      tone: 'red',
      items: [
        {
          title: 'User Management',
          description: 'Maintain system users and access configuration.',
          icon: 'pi pi-users',
          route: '/admin/users'
        },
        {
          title: 'Role Management',
          description: 'Configure roles and application permissions.',
          icon: 'pi pi-shield',
          route: '/admin/roles'
        },
        {
          title: 'System Parameters',
          description: 'Maintain Capitra application parameters.',
          icon: 'pi pi-cog'
        }
      ]
    }
  ]

  const filteredSections = computed(() => {
    const q = searchText.value.trim().toLowerCase()

    if (!q) return sections

    return sections
      .map(section => {
        const sectionMatches =
          section.title.toLowerCase().includes(q) ||
          section.description.toLowerCase().includes(q)

        const matchingItems = section.items.filter(item =>
          item.title.toLowerCase().includes(q) ||
          item.description.toLowerCase().includes(q)
        )

        if (sectionMatches) return section
        if (matchingItems.length) return { ...section, items: matchingItems }
        return null
      })
      .filter((section): section is SetupSection => section !== null)
  })

  function openSetup(item: SetupItem) {
    if (!item.route) return
    router.push(item.route)
  }

  // live counts for the hero; a count that cannot be read shows a dash
  const userCount = ref<number | null>(null)
  const activeUsers = ref<number | null>(null)
  const roleCount = ref<number | null>(null)
  const lookupCount = ref<number | null>(null)

  const allItems = computed(() => sections.flatMap(s => s.items))
  const liveCount = computed(() => allItems.value.filter(i => i.route).length)

  const show = (n: number | null) => (n === null ? '–' : String(n))

  onMounted(() => {
    getAdminUsers().then(u => { userCount.value = u.length; activeUsers.value = u.filter(x => x.isActive).length }).catch(() => {})
    getAdminRoles().then(r => { roleCount.value = r.length }).catch(() => {})
    getLookupDefinitions().then(l => { lookupCount.value = l.length }).catch(() => {})
  })

  function openViewAll(section: SetupSection) {
    if (!section.viewAllRoute) return
    router.push(section.viewAllRoute)
  }
</script>

<style scoped>
.wr-kpi small { color: #7FE0C9; font-weight: 600; font-size: 12.5px; }
.wr-kpi b i { font-style: normal; font-size: 16px; font-weight: 600; color: rgba(255, 255, 255, .55); }
.ad-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(420px, 1fr)); gap: 16px; }
.ad-card {
  display: flex; flex-direction: column; border-radius: 16px; background: #fff; border: 1px solid var(--wx-line); overflow: hidden;
  box-shadow: 0 1px 2px rgba(11, 37, 51, .04); transition: box-shadow .18s, border-color .18s;
}
.ad-card:hover { border-color: #B9CBD1; box-shadow: 0 18px 36px -18px rgba(11, 37, 51, .25); }
.ad-head { display: flex; gap: 14px; align-items: flex-start; padding: 20px 20px 14px; }
.ad-ico { flex: 0 0 44px; width: 44px; height: 44px; border-radius: 12px; display: grid; place-items: center; color: var(--wx-teal); background: var(--wx-teal-l); font-size: 18px; }
.ad-ico .pi { font-size: 18px; }
.ad-head h3 { margin: 2px 0 0; font-size: 16px; font-weight: 700; letter-spacing: -.01em; color: var(--wx-ink); }
.ad-head p { margin: 4px 0 0; font-size: 13px; line-height: 1.5; color: var(--wx-muted); }
.ad-links { display: flex; flex-direction: column; gap: 6px; padding: 0 14px 14px; }
.ad-link {
  display: flex; align-items: center; gap: 12px; width: 100%; padding: 10px 12px; border-radius: 12px; border: 1px solid var(--wx-line); background: #fff;
  text-align: start; font: inherit; color: var(--wx-ink); cursor: pointer; transition: border-color .15s, box-shadow .15s, background .15s;
}
.ad-link:hover:not(.is-off) { border-color: #B9CBD1; box-shadow: 0 8px 20px -14px rgba(11, 37, 51, .35); }
.ad-link:hover:not(.is-off) .ad-go { background: var(--wx-teal-d); border-color: var(--wx-teal-d); color: #fff; }
.ad-li { flex: 0 0 auto; width: 30px; height: 30px; border-radius: 9px; display: grid; place-items: center; background: var(--wx-soft); color: var(--wx-ink2); font-size: 13px; }
.ad-lt { flex: 1; min-width: 0; }
.ad-lt b { display: block; font-size: 13.5px; font-weight: 700; }
.ad-lt small { display: block; margin-top: 2px; font-size: 12px; color: var(--wx-muted); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.ad-go {
  flex: 0 0 auto; width: 28px; height: 28px; border-radius: 50%; display: grid; place-items: center; font-size: 11px; color: var(--wx-muted);
  background: var(--wx-soft); border: 1px solid var(--wx-line); transition: all .15s;
}
.ad-link.is-off { cursor: default; background: #FCFDFD; }
.ad-link.is-off .ad-lt b { color: var(--wx-ink2); }
.ad-foot { margin-top: auto; padding: 12px 20px; background: #FAFCFC; border-top: 1px solid var(--wx-line); }
.ad-all { display: inline-flex; align-items: center; gap: 8px; border: 0; background: none; padding: 0; font: 600 13px Inter, Arial, sans-serif; color: var(--wx-teal); cursor: pointer; }
.ad-empty { padding: 40px 20px; text-align: center; border-radius: 16px; border: 1px dashed var(--wx-line); background: #fff; color: var(--wx-muted); }
.ad-empty b { display: block; font-size: 16px; color: var(--wx-ink); margin-bottom: 4px; }
@media (max-width: 640px) { .ad-grid { grid-template-columns: 1fr; } }
</style>
