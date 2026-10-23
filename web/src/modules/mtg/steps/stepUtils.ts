// Helpers shared by the workflow step pages.
import { ApiError } from '@/shared/api/http'

/** "POTENTIAL_MATCH" -> "Potential match"; AML and PEP stay upper case. */
export function niceCode(code: string): string {
  const text = code.toLowerCase().replace(/_/g, ' ')
  return (text.charAt(0).toUpperCase() + text.slice(1)).replace(/\baml\b/gi, 'AML').replace(/\bpep\b/gi, 'PEP')
}

/** The API's message for a failed call: field errors joined, else the problem detail. */
export function messageOf(error: unknown, fallback: string): string {
  if (error instanceof ApiError) {
    const fieldErrors = Object.values(error.problem?.errors ?? {}).flat()
    if (fieldErrors.length) return fieldErrors.join(' ')
  }
  return error instanceof Error ? error.message : fallback
}
