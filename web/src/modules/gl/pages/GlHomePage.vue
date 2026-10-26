<script setup lang="ts">
// General ledger hub (APEX App 120 page 1): setup, journals and balances.
import HubPage from '@/shared/hub/HubPage.vue'
import type { HubTile } from '@/shared/hub/types'

const tiles: HubTile[] = [
  { title: 'Account Structure & Segments', description: 'The account structure, its segments (company, branch, account ...) and the values each segment accepts.', icon: 'pi pi-sitemap', to: '/gl/account-structure', apexPages: '40' },
  { title: 'Chart of Accounts', description: 'The natural account tree: types, normal balance, statements and posting controls.', icon: 'pi pi-book', to: '/gl/chart-of-accounts', apexPages: '2/3' },
  { title: 'Accounting Periods', description: 'Calendars, fiscal years and periods; open and close each period per ledger.', icon: 'pi pi-calendar', to: '/gl/accounting-periods', apexPages: '30' },
  { title: 'Ledgers', description: 'The books of each legal entity: currency, calendar, account structure and control accounts.', icon: 'pi pi-wallet', to: '/gl/ledgers', apexPages: '' },
  { title: 'Journals', description: 'Find journals, approve, post or cancel several at once, and open any of them.', icon: 'pi pi-list', to: '/gl/journals', apexPages: '51' },
  { title: 'Journal Entry', description: 'Enter a manual journal line by line; another user approves it before it posts.', icon: 'pi pi-file-edit', to: '/gl/journals/new', apexPages: '50' },
  { title: 'Account Ledger', description: 'Every posted movement of an account with its running balance.', icon: 'pi pi-chart-line', to: '/gl/account-ledger', apexPages: '52' },
  { title: 'Account Balances', description: 'Opening, period movement and closing balance of each account in a period.', icon: 'pi pi-table', to: '/gl/account-balances', apexPages: '20' },
  { title: 'Reference Data', description: 'Legal entities and business lines used by segment values and ledgers.', icon: 'pi pi-database', to: '/gl/reference-data', apexPages: '' }
]
</script>

<template>
  <HubPage
    title="General Ledger"
    description="Chart of accounts, ledgers and periods; journals, posting, the account ledger and balances."
    icon="pi pi-book"
    :tiles="tiles"
    apex-page="1"
  />
</template>
