// Customer 360: one chip colour per status code, used on every tab.
const OK = ['CLEAR', 'APPROVED', 'VERIFIED', 'ACTIVE', 'YES']
const BAD = ['REJECTED', 'BLOCKED', 'MATCH', 'EXPIRED', 'SUSPENDED', 'NO', 'MISSING']
const WARN = ['PENDING', 'REVIEW', 'DUE', 'ONBOARDING', 'WAITING']

export function chip(code: string | null | undefined): string {
  const c = (code ?? '').toUpperCase()
  if (OK.includes(c)) return 'fx-chip--ok'
  if (BAD.includes(c)) return 'fx-chip--bad'
  if (WARN.includes(c)) return 'fx-chip--warn'
  return 'fx-chip--teal'
}

/** A label from a CODE_LIKE_THIS: "Code Like This". */
export const label = (code: string | null | undefined) =>
  (code ?? '').toLowerCase().split('_').filter(Boolean).map((w) => w.charAt(0).toUpperCase() + w.slice(1)).join(' ')

/**
 * The customer roles, as CrmCustomer360Service.Roles on the server, with the supplier as its two fixed types (Ahmed, 25 Nov 2026):
 * FACTORING_SUPPLIER and CF_SUPPLIER, kept on the server as the SUPPLIER role of that business line.
 */
export const ROLES = [
  { value: 'CLIENT', label: 'Client' }, { value: 'FACTORING_SUPPLIER', label: 'Factoring Supplier' },
  { value: 'CF_SUPPLIER', label: 'Consumer Finance Supplier' }, { value: 'DEBTOR', label: 'Debtor' },
  { value: 'GUARANTOR', label: 'Guarantor' }, { value: 'REAL_ESTATE_DEVELOPER', label: 'Real Estate Developer' }, { value: 'INVESTOR', label: 'Investor' }
]

/** A role as one of the choices above: a supplier by its business line. */
export const choiceOf = (r: { roleCode: string; businessLineCode: string | null }) =>
  r.roleCode === 'SUPPLIER' ? (r.businessLineCode === 'CONSUMER_FINANCE' ? 'CF_SUPPLIER' : 'FACTORING_SUPPLIER') : r.roleCode
