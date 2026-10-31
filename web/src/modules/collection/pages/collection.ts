// Shared bits of the Collection and Repayments pages.
import { ApiError } from '@/shared/api/http'

/** Chip colour of a repayment, source or cheque status. */
export function statusChip(status: string | null | undefined): string {
  switch (status) {
    case 'ALLOCATED':
    case 'COLLECTED':
    case 'ACTIVE':
      return 'fx-chip--ok'
    case 'PARTIALLY_ALLOCATED':
    case 'DEPOSITED':
    case 'PRESENTED':
    case 'DUE':
      return 'fx-chip--teal'
    case 'DRAFT':
    case 'RECEIVED':
      return 'fx-chip--brand'
    case 'BOUNCED':
    case 'REJECTED':
    case 'REVERSED':
    case 'CANCELLED':
      return 'fx-chip--bad'
    default:
      return 'fx-chip--plain'
  }
}

/** The field errors of a failed save (a validation problem), if it was one. */
export function fieldErrors(err: unknown): Record<string, string[]> | null {
  return err instanceof ApiError && err.problem?.errors ? err.problem.errors : null
}

/** The text of a failed request: its field errors one per line, or the error's own message. */
export function messageOf(err: unknown): string {
  const errors = fieldErrors(err)
  if (errors) return Object.values(errors).flat().join('\n')
  return err instanceof Error ? err.message : String(err)
}
