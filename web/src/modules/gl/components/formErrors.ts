// Turns an API error into a message for the top of a form and messages beside its fields.
import { ApiError } from '@/shared/api/http'

export type FormErrors = { form: string; fields: Record<string, string> }

export function emptyErrors(): FormErrors {
  return { form: '', fields: {} }
}

export function toFormErrors(error: unknown, fieldNames: string[]): FormErrors {
  const result = emptyErrors()
  if (error instanceof ApiError && error.problem?.errors) {
    for (const [key, messages] of Object.entries(error.problem.errors)) {
      const name = fieldNames.find(n => n.toLowerCase() === key.toLowerCase())
      if (name) result.fields[name] = messages.join(' ')
      else result.form = [result.form, ...messages].filter(Boolean).join(' ')
    }
    if (!result.form && Object.keys(result.fields).length > 0) result.form = 'Correct the fields marked in red.'
    if (!result.form) result.form = error.message
    return result
  }
  result.form = error instanceof Error ? error.message : 'The request failed.'
  return result
}

/** Blank text becomes null so the API treats it as "not given". */
export function blank(value: unknown): unknown {
  return typeof value === 'string' && value.trim() === '' ? null : value
}

export function numberOrNull(value: unknown): number | null {
  if (value === '' || value === null || value === undefined) return null
  const n = Number(value)
  return Number.isFinite(n) ? n : null
}
