// Pages APEX built with its wizards: interactive reports and forms on one table.
// Their definitions are generated from the APEX export (tools/oracle-port/apex_wizard.py).

export type WizardColumn = { name: string; heading: string; type: 'STRING' | 'NUMBER' | 'DATE' | string }

export type WizardItem = {
  name: string
  column: string
  type: string
  label: string | null
  required: boolean
  kind: 'Text' | 'Number' | 'Flag' | 'Date' | 'DateTime' | 'Audit' | string
  /** A select list whose values come from the page's LOVS process. */
  lov?: boolean
}

export type WizardButton = { name: string; label: string; action: string; position: string; page: number | null }

export type WizardRegion = {
  key: string
  title: string
  kind: 'ir' | 'form'
  /** null for a report on a view, whose process is written by hand. */
  table: string | null
  pk: string
  parentItem?: string
  columns?: WizardColumn[]
  link?: { page: number; item: string }
  create?: { label: string; page: number }
  items?: WizardItem[]
  pkItem?: string
  buttons?: WizardButton[]
}

export type WizardPage = { page: number; title: string; modal: boolean; regions: WizardRegion[] }

/** Where each page of App 120 lives in the Capitra web app (the server's ApexPages map). */
export const apexRoutes: Record<number, string> = {
  1: '/gl',
  2: '/gl/chart-of-accounts',
  3: '/gl/account-details',
  20: '/gl/account-balances',
  30: '/gl/accounting-periods',
  40: '/gl/account-structure',
  50: '/gl/journal-entry',
  51: '/gl/journals',
  52: '/gl/account-ledger',
  56: '/gl/financial-statements',
  90: '/gl/help',
  600: '/gl/event-sources',
  601: '/gl/event-source',
  605: '/gl/accounting-events',
  606: '/gl/accounting-event',
  610: '/gl/source-values',
  611: '/gl/source-value',
  615: '/gl/rule-sets',
  616: '/gl/rule-set',
  617: '/gl/rule-lines',
  620: '/gl/generated-entries',
  621: '/gl/generated-entry',
  622: '/gl/generated-entry-lines',
  623: '/gl/generated-entry-line',
  718: '/gl/rule-line',
  730: '/gl/interest-servicing'
}

/** f?p=120:PAGE:::::ITEM:VALUE as a Capitra URL. */
export function pageUrl(page: number, item?: string, value?: unknown): string {
  const base = apexRoutes[page] ?? `/apex/120/${page}`
  return item && value != null && value !== '' ? `${base}?${item.toLowerCase()}=${encodeURIComponent(String(value))}` : base
}
