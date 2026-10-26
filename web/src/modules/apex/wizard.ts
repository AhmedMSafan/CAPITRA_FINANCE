import type { InjectionKey } from 'vue'
// Pages APEX built with its wizards: interactive reports and forms on one table.
// Their definitions are generated from the APEX export (tools/oracle-port/apex_wizard.py).

/** Page items a link sets: {P602_MEMO_ID: '#MEMO_ID#', P602_PORTFOLIO_ID: '&P601_PORTFOLIO_ID.'} (a row column or a page item). */
export type WizardSet = Record<string, string>

export type WizardTarget = { page: number | null; set?: WizardSet }

export type WizardColumn = {
  name: string
  heading: string
  type: 'STRING' | 'NUMBER' | 'DATE' | string
  /** A column link (f?p=&APP_ID.:205:...:P205_PORTFOLIO_ACCOUNT_ID:#PORTFOLIO_ACCOUNT_ID#). */
  link?: WizardTarget
  /** The APEX format mask (990D00, DD-MON-YYYY). */
  format?: string
}

export type WizardItem = {
  name: string
  /** The column of the form's table; null for an item that is not a column (a file to upload). */
  column?: string | null
  type: string
  label: string | null
  required: boolean
  kind?: 'Text' | 'Number' | 'Flag' | 'Date' | 'DateTime' | 'Audit' | string
  /** A select list whose values come from the page's LOVS process. */
  lov?: boolean
  default?: string
  /** Page Action on Selection: Submit Page. */
  submit?: boolean
  /** Start New Line: No. */
  sameLine?: boolean
}

/** A server-side condition of a button: Item is NULL / Item is NOT NULL. */
export type WizardCondition = { type: 'ITEM_IS_NULL' | 'ITEM_IS_NOT_NULL' | string; item: string }

export type WizardButton = {
  name: string
  label: string
  action?: string
  position?: string
  page: number | null
  set?: WizardSet
  cond?: WizardCondition
}

/** Whether a button's condition holds for the page items. */
export function shown(button: { cond?: WizardCondition }, items: Record<string, string>): boolean {
  if (!button.cond) return true
  const value = items[button.cond.item] ?? ''
  if (button.cond.type === 'ITEM_IS_NULL') return value === ''
  if (button.cond.type === 'ITEM_IS_NOT_NULL') return value !== ''
  return true
}

export type WizardRegion = {
  key: string
  title: string
  /** ir and form: wizard regions; items: a region of page items (report filters); plsql: dynamic content (REGION_<key>); html: static content. */
  kind: 'ir' | 'form' | 'items' | 'plsql' | 'html'
  /** null for a report on a SQL query or a view. */
  table: string | null
  pk: string | null
  parentItem?: string
  columns?: WizardColumn[]
  /** The row link: the form page and its key item, or the items the detail link sets. */
  link?: { page: number; item?: string; set?: WizardSet }
  create?: { label: string; page: number | null; set?: WizardSet }
  /** Every button right of the search bar, when there are more than Create. */
  toolbar?: WizardButton[]
  items?: WizardItem[]
  pkItem?: string
  buttons?: WizardButton[]
  html?: string
  hideHeader?: boolean
  /** The region's template options (t-Region--noPadding t-Region--scrollBody). */
  cls?: string
}

export type WizardPage = {
  page: number
  title: string
  modal: boolean
  regions: WizardRegion[]
  /** The page's inline CSS. */
  css?: string
  /** A Before Header process sets page items (PAGE_LOAD). */
  load?: boolean
  /** The page's JavaScript: Function and Global Variable Declaration, and Execute when Page Loads. */
  js?: string
  onload?: string
  /** Its After Submit processes run in the server process SUBMIT (REQUEST is the button pressed). */
  submit?: boolean
}

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
  return appPageUrl(120, page, item && value != null ? { [item]: String(value) } : {})
}

/** f?p=APP:PAGE:::::ITEMS:VALUES as a Capitra URL: App 120 at its named routes, App 105 at /mortgage/p<page>. */
export function appPageUrl(app: number, page: number, items: Record<string, string> = {}): string {
  const base = app === 120 ? (apexRoutes[page] ?? `/apex/120/${page}`) : page === 1 ? '/mortgage' : `/mortgage/p${page}`
  const query = Object.entries(items)
    .filter(([, v]) => v != null && v !== '')
    .map(([k, v]) => `${k.toLowerCase()}=${encodeURIComponent(v)}`)
    .join('&')
  return query ? `${base}?${query}` : base
}

/** The values a link sets: #COLUMN# from the row, &ITEM. from the page items. */
export function resolveSet(set: WizardSet | undefined, row: Record<string, unknown>, items: Record<string, string>): Record<string, string> {
  const out: Record<string, string> = {}
  for (const [name, value] of Object.entries(set ?? {}))
    out[name] = value
      .replace(/#(\w+)#/g, (_, c: string) => (row[c] == null ? '' : String(row[c])))
      .replace(/&(\w+)\./g, (_, i: string) => items[i.toUpperCase()] ?? '')
  return out
}

const MONTHS = ['JAN', 'FEB', 'MAR', 'APR', 'MAY', 'JUN', 'JUL', 'AUG', 'SEP', 'OCT', 'NOV', 'DEC']

/** A value as APEX shows it with a format mask: 990D00, FML999G999G990D00, DD-MON-YYYY. */
export function formatValue(value: unknown, type: string, mask?: string): string {
  if (value == null || value === '') return '-'
  if (typeof value === 'boolean') return value ? 'Y' : 'N'
  if (mask && /D|G|9|0/.test(mask) && !/[YMD]{2}/i.test(mask) && typeof value === 'number') {
    const decimals = mask.includes('D') ? mask.length - mask.indexOf('D') - 1 : 0
    return value.toLocaleString('en-US', { minimumFractionDigits: decimals, maximumFractionDigits: decimals, useGrouping: mask.includes('G') })
  }
  if (typeof value === 'string' && /^\d{4}-\d{2}-\d{2}/.test(value) && mask) {
    const [y, m, d] = value.slice(0, 10).split('-')
    if (/DD-MON-YYYY/i.test(mask)) return `${d}-${MONTHS[Number(m) - 1]}-${y}`
  }
  if (type === 'NUMBER' && typeof value === 'number') return value.toLocaleString('en-US', { maximumFractionDigits: 10 })
  return String(value)
}

const ESCAPES: Record<string, string> = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }

/** Substitution strings in a region's HTML: &P407_CHEQUE_TOTAL. is the item's value, escaped as APEX does. */
export function substitute(html: string, items: Record<string, string>): string {
  return html.replace(/&([A-Z][A-Z0-9_]*)\./g, (whole, name: string) =>
    name in items ? (items[name] ?? '').replace(/[&<>"']/g, (c) => ESCAPES[c]!) : /^P\d+_/.test(name) ? '' : whole
  )
}

/** The success message of a page submit, shown by the page when it opens again. */
export const flash = { message: '' }

/** The values of a page's items as the user changes them (provided by the page to its regions of items). */
export const pageValuesKey: InjectionKey<Record<string, string>> = Symbol('apex-page-values')
