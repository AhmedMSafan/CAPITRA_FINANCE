<script setup lang="ts">
  import { computed, ref } from 'vue'
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

  function openViewAll(section: SetupSection) {
    if (!section.viewAllRoute) return
    router.push(section.viewAllRoute)
  }
</script>

<template>
  <div class="setup-home">
    <div class="setup-shell">
      <div class="setup-top">
        <div class="setup-header">
          <div class="setup-header-icon">
            <i class="pi pi-cog"></i>
          </div>

          <div class="setup-header-copy">
            <h1>Administration</h1>
            <p>Maintain Capitra application and product configuration.</p>
          </div>
        </div>

        <div class="setup-search">
          <i class="pi pi-search"></i>
          <input v-model="searchText"
                 type="text"
                 placeholder="Search setup modules..." />
        </div>
      </div>

      <section class="section-grid">
        <article v-for="section in filteredSections"
                 :key="section.title"
                 class="section-card">
          <div class="section-card-head">
            <div class="section-icon" :class="`tone-${section.tone}`">
              <i :class="section.icon"></i>
            </div>

            <div class="section-copy">
              <h2>{{ section.title }}</h2>
              <p>{{ section.description }}</p>
            </div>
          </div>

          <div class="section-links">
            <button v-for="item in section.items"
                    :key="item.title"
                    type="button"
                    class="setup-link"
                    :class="{ disabled: !item.route }"
                    @click="openSetup(item)">
              <div class="link-left">
                <i :class="item.icon"></i>

                <div class="link-copy">
                  <strong>{{ item.title }}</strong>
                  <span>{{ item.description }}</span>
                </div>
              </div>

              <i v-if="item.route"
                 class="pi pi-chevron-right link-arrow"></i>
            </button>

            <button v-if="section.viewAllRoute"
                    type="button"
                    class="view-all-link"
                    @click="openViewAll(section)">
              <span>View All Lookups</span>
              <i class="pi pi-arrow-right"></i>
            </button>
          </div>
        </article>
      </section>

      <div v-if="filteredSections.length === 0" class="empty-state">
        <i class="pi pi-search"></i>
        <strong>No setup functions found</strong>
        <span>Try a different search term.</span>
      </div>
    </div>
  </div>
</template>

<style scoped>
  .setup-home {
    min-height: calc(100vh - 80px);
    padding: 24px 30px 38px;
    background: #f5faf6;
    font-family: Inter, "Segoe UI", Arial, sans-serif;
  }

  .setup-shell {
    width: 100%;
    max-width: 1540px;
    margin: 0 auto;
  }

  .setup-top {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 24px;
    margin-bottom: 28px;
  }

  /* Header */

  .setup-header {
    display: flex;
    align-items: center;
    gap: 16px;
  }

  .setup-header-icon {
    width: 62px;
    height: 62px;
    flex: 0 0 62px;
    display: grid;
    place-items: center;
    border-radius: 15px;
    background: #fff0df;
    color: #bb741f;
  }

    .setup-header-icon i {
      font-size: 27px;
    }

  .setup-header-copy h1 {
    margin: 0;
    color: #244b3a;
    font-size: 28px;
    line-height: 1.1;
    font-weight: 800;
  }

  .setup-header-copy p {
    margin: 6px 0 0;
    color: #718579;
    font-size: 14px;
    line-height: 1.35;
  }

  /* Search */

  .setup-search {
    width: 320px;
    height: 46px;
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 0 14px;
    border: 1px solid #dce5e8;
    border-radius: 12px;
    background: #ffffff;
    box-shadow: 0 4px 14px rgba(31, 61, 74, .035);
  }

    .setup-search i {
      color: #183b5b;
      font-size: 17px;
    }

    .setup-search input {
      width: 100%;
      border: 0;
      outline: 0;
      background: transparent;
      color: #183550;
      font: inherit;
      font-size: 13px;
    }

      .setup-search input::placeholder {
        color: #93a1ae;
      }

  /* Main section cards */

  .section-grid {
    display: grid;
    grid-template-columns: repeat(3, minmax(0, 1fr));
    gap: 20px;
  }

  .section-card {
    min-height: 292px;
    padding: 18px 20px 16px;
    border: 1px solid #dde6e8;
    border-radius: 12px;
    background: #ffffff;
    box-shadow: 0 5px 16px rgba(32, 65, 78, .045);
  }

  .section-card-head {
    display: flex;
    align-items: center;
    gap: 15px;
    margin-bottom: 16px;
  }

  .section-icon {
    width: 58px;
    height: 58px;
    flex: 0 0 58px;
    display: grid;
    place-items: center;
    border-radius: 12px;
  }

    .section-icon i {
      font-size: 27px;
    }

  .section-copy {
    min-width: 0;
  }

    .section-copy h2 {
      margin: 0;
      color: #082451;
      font-size: 17px;
      line-height: 1.2;
      font-weight: 800;
    }

    .section-copy p {
      margin: 5px 0 0;
      color: #6b8098;
      font-size: 12px;
      line-height: 1.4;
    }

  /* Tone colors - same soft pastel approach as approved design */

  .tone-blue {
    background: #e7f2ff;
    color: #1563c7;
  }

  .tone-green {
    background: #e2f5ea;
    color: #128451;
  }

  .tone-orange {
    background: #fff0dc;
    color: #c87918;
  }

  .tone-purple {
    background: #eee7ff;
    color: #6d43bd;
  }

  .tone-teal {
    background: #dff3f0;
    color: #087a73;
  }

  .tone-red {
    background: #fde8e8;
    color: #b54242;
  }

  /* Setup links */

  .section-links {
    max-height: 200px;
    overflow-y: auto;
    overflow-x: hidden;
    border-top: 1px solid #e8edef;
    padding-right: 3px;
    scrollbar-width: thin;
    scrollbar-color: #c7d5dd transparent;
  }

    .section-links::-webkit-scrollbar {
      width: 6px;
    }

    .section-links::-webkit-scrollbar-track {
      background: transparent;
    }

    .section-links::-webkit-scrollbar-thumb {
      background: #c7d5dd;
      border-radius: 999px;
    }

      .section-links::-webkit-scrollbar-thumb:hover {
        background: #aebfc9;
      }

  .setup-link {
    width: 100%;
    min-height: 61px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 14px;
    padding: 8px 4px;
    border: 0;
    border-bottom: 1px solid #e8edef;
    background: transparent;
    text-align: left;
    font: inherit;
    color: inherit;
    cursor: pointer;
  }

    .setup-link:last-child {
      border-bottom: 0;
    }

    .setup-link:not(.disabled):hover {
      background: #fafcfb;
    }

  .link-left {
    min-width: 0;
    display: flex;
    align-items: center;
    gap: 12px;
  }

    .link-left > i {
      width: 22px;
      flex: 0 0 22px;
      text-align: center;
      color: #173d68;
      font-size: 17px;
    }

  .link-copy {
    min-width: 0;
    display: flex;
    flex-direction: column;
  }

    .link-copy strong {
      color: #102b52;
      font-size: 13px;
      line-height: 1.25;
      font-weight: 750;
    }

    .link-copy span {
      margin-top: 3px;
      color: #788b9e;
      font-size: 11px;
      line-height: 1.3;
    }

  .link-arrow {
    flex: 0 0 auto;
    color: #173d68;
    font-size: 13px;
  }

  .setup-link.disabled {
    cursor: default;
    opacity: .76;
  }

  .view-all-link {
    width: 100%;
    min-height: 43px;
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 9px 4px 0;
    border: 0;
    background: transparent;
    color: #1563c7;
    font: inherit;
    font-size: 12px;
    font-weight: 800;
    cursor: pointer;
  }

    .view-all-link:hover {
      color: #0f4f9f;
    }

    .view-all-link i {
      font-size: 12px;
    }

  /* Empty state */

  .empty-state {
    min-height: 220px;
    display: grid;
    place-items: center;
    align-content: center;
    gap: 8px;
    color: #718579;
  }

    .empty-state i {
      font-size: 28px;
    }

    .empty-state strong {
      color: #244b3a;
      font-size: 15px;
    }

    .empty-state span {
      font-size: 12px;
    }

  /* Responsive */

  @media (max-width: 1180px) {
    .section-grid {
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }
  }

  @media (max-width: 780px) {
    .setup-top {
      align-items: flex-start;
      flex-direction: column;
    }

    .setup-search {
      width: 100%;
      box-sizing: border-box;
    }

    .section-grid {
      grid-template-columns: 1fr;
    }
  }

  @media (max-width: 600px) {
    .setup-home {
      padding: 20px 16px 30px;
    }

    .setup-header-icon {
      width: 54px;
      height: 54px;
      flex-basis: 54px;
    }

      .setup-header-icon i {
        font-size: 23px;
      }

    .setup-header-copy h1 {
      font-size: 24px;
    }

    .section-card {
      padding: 16px;
    }
  }
</style>
