const fullFormat = new Intl.NumberFormat('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
const shortFormat = new Intl.NumberFormat('en-US', { maximumFractionDigits: 1 })

/** 2,500,000.00 */
export function formatAmount(value: number | null | undefined): string {
  return value === null || value === undefined ? '–' : fullFormat.format(value)
}

/** 2.5M, 750K: the compact figures APEX shows on cards (page 201/203 money()). */
export function formatShort(value: number | null | undefined): string {
  if (value === null || value === undefined) return '–'
  const abs = Math.abs(value)
  if (abs >= 1e9) return `${shortFormat.format(value / 1e9)}B`
  if (abs >= 1e6) return `${shortFormat.format(value / 1e6)}M`
  if (abs >= 1e3) return `${shortFormat.format(value / 1e3)}K`
  return shortFormat.format(value)
}

/** 13.5% */
export function formatPct(value: number | null | undefined): string {
  return value === null || value === undefined ? '–' : `${new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 }).format(value)}%`
}
