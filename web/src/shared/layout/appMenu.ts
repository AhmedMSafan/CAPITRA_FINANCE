// The header Menu: the main pages of every module, built from the module menus (App 105 and App 110 navigation
// menus, the Cheques Wallet pages, the General Ledger routes) plus the shared workspace pages.
import type { RouteLocationRaw } from 'vue-router'
import { navMenu as mtgMenu, type NavItem } from '@/modules/mtg/apex/navMenu'
import { navMenu as mffMenu } from '@/modules/mff/apex/navMenu'

export type MenuLink = { label: string; to: RouteLocationRaw; icon?: string }
export type MenuGroup = { title: string; icon: string; links: MenuLink[]; workspace?: boolean }

/** The page an entry opens: its own, else its first child's. */
function pageOf(item: NavItem): number | null {
  return item.page ?? item.children?.find((c) => c.page !== null)?.page ?? null
}

function fromNav(items: NavItem[], prefix: 'mtg' | 'mff', skip: string[], take: number): MenuLink[] {
  return items
    .filter((i) => !skip.includes(i.label) && pageOf(i) !== null)
    .slice(0, take)
    .map((i) => ({ label: i.label, to: { name: `${prefix}-${pageOf(i)}` } }))
}

export const appMenu: MenuGroup[] = [
  {
    title: 'Portfolio Acquisition',
    icon: 'pi pi-briefcase',
    links: fromNav(mtgMenu, 'mtg', ['Home', 'My Inbox', 'Developer Workspace', 'Reference Data'], 8)
  },
  {
    title: 'Mortgage Finance',
    icon: 'pi pi-building',
    links: fromNav(mffMenu, 'mff', [], 7)
  },
  {
    title: 'Accounting',
    icon: 'pi pi-book',
    links: [
      { label: 'Chart of Accounts', to: '/gl/chart-of-accounts' },
      { label: 'Accounting Periods', to: '/gl/accounting-periods' },
      { label: 'Journals', to: '/gl/journals' },
      { label: 'Journal Entry', to: '/gl/journal-entry' },
      { label: 'Account Balances', to: '/gl/account-balances' },
      { label: 'Accounting Rule Sets', to: '/gl/rule-sets' },
      { label: 'Financial Statements', to: '/gl/financial-statements' }
    ]
  },
  {
    title: 'Cheques Wallet',
    icon: 'pi pi-wallet',
    links: [
      { label: 'Cheques Wallet Home', to: { name: 'chq-1' } },
      { label: 'Cheques Wallet', to: { name: 'chq-130' } },
      { label: 'Cheque Batch Upload', to: { name: 'chq-120' } },
      { label: 'Cheques Movements', to: { name: 'chq-140' } },
      { label: 'Cheques Due Report', to: { name: 'chq-12' } },
      { label: 'Cheque Action Catalog', to: { name: 'chq-150' } }
    ]
  },
  {
    title: 'Workspace',
    icon: 'pi pi-th-large',
    workspace: true,
    links: [
      { label: 'Home', to: { name: 'home' }, icon: 'pi pi-home' },
      { label: 'My Inbox', to: '/workflow/inbox', icon: 'pi pi-inbox' },
      { label: 'Workflow Engine', to: '/workflowengine', icon: 'pi pi-sitemap' },
      { label: 'Workflow Registry', to: '/workflowengine/registry', icon: 'pi pi-list' },
      { label: 'Administration', to: '/admin', icon: 'pi pi-cog' },
      { label: 'Users', to: '/admin/users', icon: 'pi pi-users' },
      { label: 'Roles', to: '/admin/roles', icon: 'pi pi-shield' },
      { label: 'Reference Data', to: '/admin/reference-data', icon: 'pi pi-list' }
    ]
  }
]
