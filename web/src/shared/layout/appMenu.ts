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
    // the portfolio cases page (201) is the Purchased Portfolio Workspace: it opens first, in place of the dashboard
    links: fromNav(mtgMenu, 'mtg', ['Home', 'Portfolio Dashboard', 'My Inbox', 'Developer Workspace', 'Reference Data'], 8)
      .map((l) => (l.label === 'Portfolio Cases' ? { ...l, label: 'Portfolio Workspace' } : l))
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
    title: 'Treasury',
    icon: 'pi pi-building-columns',
    links: [{ label: 'Bank Limits', to: { name: 'trs-bank-limits' } }]
  },
  {
    title: 'CRM',
    icon: 'pi pi-users',
    links: [
      { label: 'Customers', to: { name: 'crm-customers' } },
      { label: 'Consumer Finance Suppliers', to: { name: 'crm-cf-suppliers' } }
    ]
  },
  {
    title: 'Retail Lending',
    icon: 'pi pi-shopping-cart',
    links: [
      { label: 'Lending Workspace', to: { name: 'lnd-workspace' } },
      { label: 'Finance Calculator', to: { name: 'tls-loan-calculator' } }
    ]
  },
  {
    title: 'Screening',
    icon: 'pi pi-shield',
    links: [
      { label: 'Federation Final List', to: { name: 'scr-registry' } },
      { label: 'Decisions', to: { name: 'scr-decisions' } },
      { label: 'Incoming letters', to: { name: 'scr-letters' } },
      { label: 'Parties', to: { name: 'scr-parties' } },
      { label: 'Member companies', to: { name: 'scr-companies' } },
      { label: 'Inquiry audit', to: { name: 'scr-inquiries' } },
      { label: 'Sign-ins and activity', to: { name: 'scr-access-log' } },
      { label: 'Billing', to: { name: 'scr-billing' } },
      { label: 'Price plans', to: { name: 'scr-price-plans' } },
      { label: 'Screening Portal', to: { name: 'scr-portal' } }
    ]
  },
  {
    title: 'End of Day',
    icon: 'pi pi-moon',
    links: [
      { label: 'Run end of day', to: { name: 'eod-home' } },
      { label: 'End of Day jobs', to: { name: 'eod-jobs' } },
      { label: 'Lending end of day', to: { name: 'lnd-eod' } }
    ]
  },
  {
    title: 'Communication Center',
    icon: 'pi pi-send',
    links: [
      { label: 'Mail servers', to: { name: 'cmc-mail-servers' } },
      { label: 'Sent e-mails', to: { name: 'cmc-email-log' } }
    ]
  },
  {
    title: 'Tools',
    icon: 'pi pi-calculator',
    links: [{ label: 'Finance Calculator', to: { name: 'tls-loan-calculator' } }]
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
      { label: 'Start a request', to: '/workflow/start', icon: 'pi pi-play' },
      { label: 'Out of Office', to: '/workflow/out-of-office', icon: 'pi pi-calendar-times' },
      { label: 'Workflow Engine', to: '/workflowengine', icon: 'pi pi-sitemap' },
      { label: 'Workflow Registry', to: '/workflowengine/registry', icon: 'pi pi-list' },
      { label: 'Document Designer', to: '/workflowengine/forms', icon: 'pi pi-file-edit' },
      { label: 'Process monitor', to: '/workflowengine/monitor', icon: 'pi pi-chart-bar' },
      { label: 'Data Sources', to: '/workflowengine/data-sources', icon: 'pi pi-database' },
      { label: 'Administration', to: '/admin', icon: 'pi pi-cog' },
      { label: 'Users', to: '/admin/users', icon: 'pi pi-users' },
      { label: 'Roles', to: '/admin/roles', icon: 'pi pi-shield' },
      { label: 'Reference Data', to: '/admin/reference-data', icon: 'pi pi-list' },
      { label: 'Lending Parameters', to: { name: 'lnd-parameters' }, icon: 'pi pi-sliders-h' }
    ]
  }
]
