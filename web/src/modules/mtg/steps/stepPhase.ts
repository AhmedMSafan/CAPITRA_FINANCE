// Where a portfolio stands against one workflow step, so a step page can show its stages and counter honestly:
// a step the portfolio has not reached yet shows nothing done, and a step it has passed shows everything done.

/** Status each step leaves behind when it is accepted, in workflow order. */
const ORDER = [
  'DRAFT',
  'DATA_LOADING_COMPLETED',
  'AML_ACCEPTED',
  'INQUIRY_ACCEPTED',
  'LEGAL_REVIEW_ACCEPTED',
  'VALUATION_COMPLETED',
  'COVERAGE_REVIEW_ACCEPTED',
  'CHEQUE_COVERAGE_ACCEPTED',
  'FINANCIAL_REVIEW_ACCEPTED',
  'BOARD_REVIEW_ACCEPTED'
]

export type StepPhase = 'waiting' | 'open' | 'done'
export type Stage = { label: string; state: 'done' | 'now' | 'todo' }

/** Position of a status in the workflow. A rejected step sits where it was opened (it is open again). */
function rank(status: string): number {
  const i = ORDER.indexOf(status)
  if (i >= 0) return i
  if (status.endsWith('_REJECTED')) {
    const accepted = ORDER.indexOf(status.replace(/_REJECTED$/, '_ACCEPTED'))
    if (accepted > 0) return accepted - 1
  }
  return ORDER.length // later statuses (disbursement onwards) are past every purchasing step
}

/** waiting: an earlier step is not finished; open: this step is the current one; done: the portfolio has passed it. */
export function stepPhase(acceptedStatus: string, currentStatus: string): StepPhase {
  const current = rank(currentStatus)
  const target = ORDER.indexOf(acceptedStatus)
  if (current >= target) return 'done'
  return current === target - 1 ? 'open' : 'waiting'
}

/** Stages for StepLayout: all todo while waiting, all done once passed, otherwise the first unfinished one is "now". */
export function buildStages(phase: StepPhase, items: { label: string; done: boolean }[]): Stage[] {
  if (phase === 'done') return items.map(i => ({ label: i.label, state: 'done' }))
  if (phase === 'waiting') return items.map(i => ({ label: i.label, state: 'todo' }))
  let current = false
  return items.map(i => {
    if (i.done) return { label: i.label, state: 'done' }
    if (!current) {
      current = true
      return { label: i.label, state: 'now' }
    }
    return { label: i.label, state: 'todo' }
  })
}
