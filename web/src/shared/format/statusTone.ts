/**
 * One colour per portfolio status, used on every page (agreed with Ahmed, 25 Oct 2026):
 * Draft gold, steps in progress violet, board approval (the end) green, rejected red, cancelled grey.
 */
const FINAL = ['BOARD_REVIEW_ACCEPTED']

export function statusTone(code: string | null | undefined): string {
  const c = (code ?? '').toUpperCase()
  if (c.includes('REJECT')) return 'var(--cap-danger)'
  if (c.includes('CANCEL')) return 'var(--cap-muted)'
  if (c.includes('DRAFT')) return 'var(--cap-orange)'
  if (FINAL.includes(c)) return 'var(--cap-success)'
  return 'var(--cap-progress)'
}

/** Inline style for a status pill: the tone on a light tint of itself. */
export function statusPillStyle(code: string | null | undefined): Record<string, string> {
  const tone = statusTone(code)
  return { color: tone, background: `color-mix(in srgb, ${tone} 12%, #fff)` }
}
