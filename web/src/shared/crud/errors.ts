import { ApiError } from '@/shared/api/http'

/**
 * Splits an API error into messages per form field (matched case-insensitively on the field name)
 * and one message for the whole form.
 */
export function splitApiError(
  error: unknown,
  fieldNames: string[],
  fallback: string
): { fields: Record<string, string>; form: string } {
  const fields: Record<string, string> = {}
  const formMessages: string[] = []

  if (error instanceof ApiError && error.problem?.errors) {
    for (const [key, messages] of Object.entries(error.problem.errors)) {
      const name = fieldNames.find(n => n.toLowerCase() === key.toLowerCase())
      if (name) fields[name] = messages.join(' ')
      else formMessages.push(...messages)
    }
    if (formMessages.length === 0 && Object.keys(fields).length === 0) formMessages.push(error.message)
  } else {
    formMessages.push(error instanceof Error ? error.message : fallback)
  }

  return { fields, form: formMessages.join(' ') }
}
