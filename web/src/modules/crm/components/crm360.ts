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
