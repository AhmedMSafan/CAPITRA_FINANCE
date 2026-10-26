<script setup lang="ts">
import { computed, ref } from 'vue'
import { getUser } from '@/shared/auth/session'
import { formatDate, formatWeekday } from '@/shared/format/datetime'

type BusinessModule = {
  code: string
  title: string
  description: string
  route: string
  tone: 'crm' | 'factoring' | 'leasing' | 'mortgage' | 'admin'
  icon: string
  enabled: boolean
}

const searchText = ref('')
const user = getUser()

const displayName = user?.fullName?.trim() || user?.userName?.trim() || 'CAPITRA User'
const firstName = displayName.split(/\s+/)[0] ?? 'User'

// The APEX applications being rebuilt. A module is enabled when its first screens ship.
const modules: BusinessModule[] = [
  {
    code: 'MORTGAGE',
    title: 'Portfolio Acquisition',
    description: 'Mortgage portfolio purchase: data load, AML, inquiry, legal, valuation, coverage, financial and board review, disbursement.',
    route: '/mortgage',
    tone: 'mortgage',
    icon: 'pi pi-home',
    enabled: true
  },
  {
    code: 'MORTGAGE_FINANCE',
    title: 'Mortgage Finance',
    description: 'Fund-supported mortgage finance: Fund file intake, credit, subsidy, offer, contract, disbursement, servicing and collections.',
    route: '/mortgage-finance',
    tone: 'mortgage',
    icon: 'pi pi-building',
    enabled: true
  },
  {
    code: 'WORKFLOW',
    title: 'Workflow',
    description: 'Approval inbox, step assignment, committees and SLA tracking for every business process.',
    route: '/workflow/inbox',
    tone: 'factoring',
    icon: 'pi pi-sitemap',
    enabled: true
  },
  {
    code: 'ACCOUNTING',
    title: 'Accounting',
    description: 'Chart of accounts, accounting periods, posting rules and general ledger transactions.',
    route: '/gl',
    tone: 'crm',
    icon: 'pi pi-book',
    enabled: true
  },
  {
    code: 'CHEQUES',
    title: 'Cheques Wallet',
    description: 'Cheque batches, cheque movements and collection status.',
    route: '/cheque-wallet',
    tone: 'leasing',
    icon: 'pi pi-wallet',
    enabled: false
  },
  {
    code: 'ADMIN',
    title: 'Administration',
    description: 'Users, roles, workflow definitions and the global reference data shared by every application.',
    route: '/admin',
    tone: 'admin',
    icon: 'pi pi-cog',
    enabled: true
  }
]

const now = new Date()
const todayCard = { day: formatWeekday(now), date: formatDate(now) }

const filteredModules = computed(() => {
  const q = searchText.value.trim().toLowerCase()
  if (!q) return modules

  return modules.filter(m =>
    m.code.toLowerCase().includes(q) ||
    m.title.toLowerCase().includes(q) ||
    m.description.toLowerCase().includes(q)
  )
})
</script>

<template>
  <div class="capitra-home-page">
    <aside class="home-sidebar">
      <nav class="home-nav">
        <RouterLink to="/" class="home-nav-item">
          <i class="pi pi-home"></i><span>Home</span>
        </RouterLink>

        <template v-for="module in modules" :key="module.code">
          <RouterLink v-if="module.enabled" :to="module.route" class="home-nav-item">
            <i :class="module.icon"></i><span>{{ module.title }}</span>
          </RouterLink>
          <div v-else class="home-nav-item is-disabled">
            <i :class="module.icon"></i><span>{{ module.title }}</span>
          </div>
        </template>
      </nav>
    </aside>

    <main class="home-main">
      <!-- PERSONAL WORKSPACE HEADER -->
      <section class="workspace-header">
        <div class="workspace-brand">
          <strong class="workspace-capitra">CAPITRA</strong>
          <span class="workspace-slogan">
            Integrated Financing Solutions for a Smarter Tomorrow
          </span>
        </div>

        <div class="workspace-user">
          <span class="workspace-welcome">Welcome {{ firstName }}</span>
          <span class="workspace-subtitle">Your CAPITRA workspace</span>
        </div>

        <label class="workspace-search">
          <i class="pi pi-search"></i>
          <input
            v-model="searchText"
            type="text"
            placeholder="Search modules..."
          />
          <span class="shortcut-key">Ctrl + K</span>
        </label>

        <div class="workspace-smart-actions">
          <div class="smart-item smart-item--today">
            <span class="smart-icon">
              <i class="pi pi-calendar"></i>
            </span>
            <span class="smart-copy">
              <small>{{ todayCard.day }}</small>
              <strong>{{ todayCard.date }}</strong>
            </span>
          </div>

        </div>
      </section>

      <!-- PRIMARY BUSINESS MODULES -->
      <section class="business-section">
        <div class="section-heading">
          <span class="section-marker section-marker--orange"></span>
          <div>
            <h2>Business Modules</h2>
            <p>Open a CAPITRA business area</p>
          </div>
        </div>

        <div class="modules-grid">
          <template v-for="module in filteredModules" :key="module.code">
            <RouterLink
              v-if="module.enabled"
              :to="module.route"
              class="business-card"
              :class="`business-card--${module.tone}`"
            >
              <div class="business-card-head">
                <div class="business-icon"><i :class="module.icon"></i></div>
                <span class="module-code">{{ module.code }}</span>
              </div>

              <div class="business-card-body">
                <h3>{{ module.title }}</h3>
                <p>{{ module.description }}</p>
              </div>

              <div class="business-card-action">
                <span>Open Module</span>
                <i class="pi pi-arrow-right"></i>
              </div>
            </RouterLink>

            <div
              v-else
              class="business-card is-disabled"
              :class="`business-card--${module.tone}`"
            >
              <div class="business-card-head">
                <div class="business-icon"><i :class="module.icon"></i></div>
                <span class="module-code">{{ module.code }}</span>
              </div>

              <div class="business-card-body">
                <h3>{{ module.title }}</h3>
                <p>{{ module.description }}</p>
              </div>

              <div class="business-card-action">
                <span>Coming Soon</span>
                <i class="pi pi-lock"></i>
              </div>
            </div>
          </template>
        </div>
      </section>
    </main>
  </div>
</template>

<style scoped>
.capitra-home-page,
.capitra-home-page * {
  box-sizing: border-box;
}

.capitra-home-page {
  height: calc(100vh - 78px);
  display: grid;
  grid-template-columns: 218px minmax(0, 1fr);
  overflow: hidden;
  background: var(--cap-bg);
  color: var(--cap-navy);
  font-family: Inter, "Segoe UI", Arial, sans-serif;
}

/* SIDEBAR */
.home-sidebar {
  min-height: 0;
  display: flex;
  flex-direction: column;
  padding: 18px 14px;
  background: #fff;
  border-right: 1px solid var(--cap-border);
  overflow-y: auto;
}

.home-nav {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.home-nav-item,
.support-btn {
  height: 48px;
  display: flex;
  align-items: center;
  gap: 14px;
  padding: 0 15px;
  border: 0;
  border-radius: 11px;
  background: transparent;
  color: var(--cap-navy);
  text-decoration: none;
  font: inherit;
  font-size: 13.5px;
  font-weight: 700;
}

.home-nav-item i,
.support-btn i {
  width: 22px;
  text-align: center;
  font-size: 18px;
}

.home-nav-item:hover,
.support-btn:hover {
  background: var(--cap-bg);
}

.home-nav-item.router-link-exact-active {
  background: linear-gradient(90deg, var(--cap-warning-bg), var(--cap-primary-tint));
  box-shadow: inset 3px 0 0 var(--cap-accent);
}

.home-nav-item.router-link-exact-active i {
  color: var(--cap-accent-dark);
}

.home-nav-item.is-disabled {
  opacity: .55;
}

.support-btn {
  margin-top: auto;
  cursor: pointer;
}

/* MAIN */
.home-main {
  min-width: 0;
  min-height: 0;
  overflow-y: auto;
  padding: 12px 18px 22px;
}

/* MAIN CONTENT - SIMPLE FLAT HIERARCHY */
.home-main {
  min-width: 0;
  padding: 20px 22px 28px;
  background: var(--cap-bg);
}

/* PERSONAL WORKSPACE HEADER */
.workspace-header {
  display: grid;
  grid-template-columns: minmax(0, 1.1fr) auto;
  grid-template-areas:
    "brand user"
    "search smart";
  gap: 16px 28px;
  align-items: center;
  padding: 22px 24px;
  border: 1px solid var(--cap-border-soft);
  border-radius: 16px;
  background: #ffffff;
  box-shadow: 0 6px 16px rgba(20, 33, 58, .045);
}

.workspace-brand {
  grid-area: brand;
  min-width: 0;
  display: flex;
  align-items: baseline;
  gap: 14px;
}

.workspace-capitra {
  flex: 0 0 auto;
  color: var(--cap-heading);
  font-size: 34px;
  line-height: 1;
  font-weight: 900;
  letter-spacing: .05em;
}

.workspace-slogan {
  min-width: 0;
  color: var(--cap-muted);
  font-size: 14px;
  font-weight: 600;
}

.workspace-user {
  grid-area: user;
  justify-self: end;
  text-align: right;
}

.workspace-welcome {
  display: block;
  color: var(--cap-heading);
  font-size: 14px;
  font-weight: 850;
}

.workspace-subtitle {
  display: block;
  margin-top: 2px;
  color: var(--cap-faint);
  font-size: 9.5px;
}

.workspace-search {
  grid-area: search;
  width: min(520px, 100%);
  min-width: 0;
  height: 44px;
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 0 11px 0 14px;
  border: 1px solid var(--cap-border);
  border-radius: 11px;
  background: var(--cap-bg);
}

.workspace-search > i {
  color: var(--cap-primary);
  font-size: 15px;
}

.workspace-search input {
  min-width: 0;
  flex: 1;
  border: 0;
  outline: 0;
  background: transparent;
  color: var(--cap-heading);
  font: inherit;
  font-size: 12.5px;
}

.shortcut-key {
  padding: 4px 8px;
  border: 1px solid var(--cap-border);
  border-radius: 6px;
  background: #fff;
  color: var(--cap-muted);
  font-size: 9.5px;
  font-weight: 700;
}

.workspace-smart-actions {
  grid-area: smart;
  display: grid;
  grid-template-columns: repeat(2, minmax(185px, 1fr));
  gap: 12px;
}

.smart-item {
  min-height: 58px;
  display: grid;
  grid-template-columns: 44px minmax(0, 1fr) 18px;
  align-items: center;
  gap: 11px;
  padding: 9px 12px;
  border: 1px solid var(--cap-border);
  border-radius: 10px;
  background: #ffffff;
  box-shadow: 0 6px 18px rgba(17, 60, 102, 0.05);
  color: inherit;
  text-decoration: none;
}

.smart-item--today {
  grid-template-columns: 44px minmax(0, 1fr);
  background: linear-gradient(180deg, var(--cap-primary-tint) 0%, var(--cap-primary-tint) 100%);
}

.smart-item--inbox {
  background: linear-gradient(180deg, var(--cap-bg) 0%, var(--cap-primary-tint) 100%);
}

.smart-icon {
  width: 42px;
  height: 42px;
  display: grid;
  place-items: center;
  border-radius: 10px;
  background: var(--cap-primary-tint);
  color: var(--cap-primary);
  font-size: 19px;
  box-shadow: inset 0 0 0 1px rgba(31, 95, 149, 0.08);
}

.smart-item--inbox .smart-icon {
  background: #d9f2eb;
  color: var(--cap-primary);
  box-shadow: inset 0 0 0 1px rgba(10, 124, 111, 0.08);
}

.smart-copy {
  min-width: 0;
}

.smart-copy small {
  display: block;
  color: var(--cap-faint);
  font-size: 10px;
  line-height: 1.2;
}

.smart-copy strong {
  display: block;
  margin-top: 2px;
  color: var(--cap-ink);
  font-size: 13px;
  line-height: 1.2;
}

.smart-arrow {
  justify-self: end;
  color: var(--cap-muted);
  font-size: 14px;
}

/* SECTION HEADINGS */
.quick-access-section {
  margin-top: 26px;
}

.business-section {
  margin-top: 30px;
}

.section-heading {
  display: flex;
  align-items: center;
  gap: 9px;
  margin: 0 2px 9px;
}

.section-heading > div {
  min-width: 0;
}

.section-heading h2 {
  margin: 0;
  color: var(--cap-ink);
  font-size: 14px;
  font-weight: 850;
}

.section-heading p {
  margin: 2px 0 0;
  color: var(--cap-muted);
  font-size: 10px;
}

.section-marker {
  width: 4px;
  height: 18px;
  border-radius: 999px;
  background: var(--cap-primary);
}

.section-marker--orange {
  background: var(--cap-accent);
}

/* QUICK ACCESS: thin navigation strip, not competing cards */
.quick-access-bar {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  overflow: hidden;
  border: 1px solid var(--cap-border-soft);
  border-radius: 12px;
  background: #fff;
  box-shadow: 0 4px 12px rgba(20, 33, 58, .035);
}

.quick-link {
  min-width: 0;
  min-height: 60px;
  display: grid;
  grid-template-columns: 34px minmax(0, 1fr) 18px;
  align-items: center;
  gap: 9px;
  padding: 10px 14px;
  color: inherit;
  text-decoration: none;
  border-right: 1px solid var(--cap-border-soft);
  transition: background .14s ease;
}

.quick-link:last-child {
  border-right: 0;
}

.quick-link:hover {
  background: var(--cap-bg);
}

.quick-link-icon {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  border-radius: 8px;
  font-size: 15px;
}

.quick-link-text {
  min-width: 0;
}

.quick-link-text strong {
  display: block;
  overflow: hidden;
  color: var(--cap-ink);
  font-size: 11.5px;
  font-weight: 800;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.quick-link-text small {
  display: block;
  margin-top: 1px;
  overflow: hidden;
  color: var(--cap-faint);
  font-size: 9px;
  white-space: nowrap;
  text-overflow: ellipsis;
}

.quick-link-arrow {
  justify-self: end;
  color: var(--cap-faint);
  font-size: 11px;
}

.quick-link--admin .quick-link-icon { color:var(--cap-success-strong); background:#e3f7eb; }
.quick-link--tools .quick-link-icon { color:var(--cap-accent-dark); background:var(--cap-warning-bg); }
.quick-link--platform .quick-link-icon { color:var(--cap-primary); background:var(--cap-primary-tint); }
.quick-link--workflow .quick-link-icon { color:#6b55b7; background:#eee9fb; }

/* BUSINESS MODULES: visually dominant primary cards */
.modules-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
}

.business-card {
  min-height: 238px;
  display: flex;
  flex-direction: column;
  padding: 18px;
  border: 1px solid var(--cap-border);
  border-radius: 14px;
  background: #fff;
  color: inherit;
  text-decoration: none;
  box-shadow: 0 5px 15px rgba(20, 33, 58, .045);
  transition: transform .16s ease, box-shadow .16s ease, border-color .16s ease;
}

.business-card:not(.is-disabled):hover {
  transform: translateY(-3px);
  border-color: var(--cap-border);
  box-shadow: 0 10px 22px rgba(23,62,80,.08);
}

.business-card-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.business-icon {
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
  border-radius: 11px;
}

.business-icon i {
  font-size: 20px !important;
}

.module-code {
  font-size: 9.5px;
  font-weight: 900;
  letter-spacing: .045em;
}

.business-card-body {
  flex: 1;
  padding-top: 18px;
}

.business-card-body h3 {
  margin: 0;
  color: var(--cap-ink);
  font-size: 20px;
  line-height: 1.15;
  font-weight: 850;
}

.business-card-body p {
  margin: 9px 0 0;
  color: var(--cap-muted);
  font-size: 11.5px;
  line-height: 1.55;
}

.business-card-action {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: 15px;
  border-top: 1px solid var(--cap-border-soft);
  font-size: 11px;
  font-weight: 800;
}

.business-card--crm {
  border-top: 3px solid #12a85b;
}
.business-card--crm .business-icon {
  color:#0b9b51;
  background:#e3f7eb;
}
.business-card--crm .module-code,
.business-card--crm .business-card-action {
  color:#0b9b51;
}

.business-card--factoring {
  border-top: 3px solid #ff790d;
}
.business-card--factoring .business-icon {
  color:#f26f08;
  background:var(--cap-warning-bg);
}
.business-card--factoring .module-code,
.business-card--factoring .business-card-action {
  color:#f26f08;
}

.business-card--leasing {
  border-top: 3px solid #48a5ee;
}
.business-card--leasing .business-icon {
  color:#378fd4;
  background:var(--cap-primary-tint);
}
.business-card--leasing .module-code,
.business-card--leasing .business-card-action {
  color:#378fd4;
}

.business-card--admin {
  border-top: 3px solid var(--cap-muted);
}
.business-card--admin .business-icon {
  color:var(--cap-muted);
  background:var(--cap-bg);
}
.business-card--admin .module-code,
.business-card--admin .business-card-action {
  color:var(--cap-muted);
}

.business-card--mortgage {
  border-top: 3px solid var(--cap-primary);
}
.business-card--mortgage .business-icon {
  color:var(--cap-primary);
  background:var(--cap-primary-tint);
}
.business-card--mortgage .module-code,
.business-card--mortgage .business-card-action {
  color:var(--cap-primary);
}

.business-card.is-disabled {
  opacity: .68;
  cursor: default;
}
@media (max-width: 1240px) {
  .capitra-home-page {
    grid-template-columns: 185px minmax(0,1fr);
  }

  .modules-grid {
    grid-template-columns: repeat(2, minmax(0,1fr));
  }

  .glance-grid {
    grid-template-columns: repeat(2, minmax(0,1fr));
  }

  .quote-card {
    grid-column: 1 / -1;
  }
}

@media (max-width: 900px) {
  .capitra-home-page {
    height: auto;
    min-height: 100vh;
    grid-template-columns: 1fr;
  }

  .home-sidebar {
    display: none;
  }

  .home-main {
    overflow: visible;
  }

  .hero-copy {
    width: 100%;
    padding: 26px;
  }

  .hero-art {
    opacity: .30;
    inset: 0;
  }

  .quick-grid,
  .modules-grid {
    grid-template-columns: 1fr;
  }
}
</style>
