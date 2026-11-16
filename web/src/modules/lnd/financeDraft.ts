// New Finance <-> Finance Calculator (Ahmed, 16 Nov 2026): what the user typed on New Finance is kept while they work in the calculator,
// and the calculator can hand back its schedule's terms without saving a scenario. Kept in this browser tab only (sessionStorage).
import type { LoanScenarioWorkspaceRequest } from '@/modules/tls/api/loanScenarios.api'

export const FINANCE_DRAFT_KEY = 'capitra.newFinance.draft'
export const FINANCE_CALCULATION_KEY = 'capitra.newFinance.calculation'
// the schedule the calculator showed, so New Finance shows it at once on return
export const FINANCE_CALCULATION_RESULT_KEY = 'capitra.newFinance.calculationResult'

export type FinanceCalculation = LoanScenarioWorkspaceRequest

export function keep(key: string, value: unknown) {
  try {
    sessionStorage.setItem(key, JSON.stringify(value))
  } catch {
    // storage unavailable: the page still works, the values are only not kept
  }
}

/** Reads a kept value once and forgets it. */
export function take<T>(key: string): T | null {
  try {
    const text = sessionStorage.getItem(key)
    sessionStorage.removeItem(key)
    return text ? (JSON.parse(text) as T) : null
  } catch {
    return null
  }
}
