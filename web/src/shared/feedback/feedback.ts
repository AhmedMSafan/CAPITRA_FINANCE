// App-wide feedback: one centred pop-up for every success, warning and error message, and one animated
// progress screen for long operations. Pages hand their message and busy refs to useFeedback().
import { reactive } from 'vue'

export type PopupKind = 'success' | 'warning' | 'error'

export type Popup = { id: number; kind: PopupKind; title: string; message: string; onClose?: () => void }

/** A long operation: a title, and the sub-steps the progress screen walks through while it runs. */
export type BusySpec = { title: string; steps?: string[]; kind?: 'excel' | 'calc' | 'save' }

type BusyEntry = BusySpec & { token: number; startedAt: number }

const TITLES: Record<PopupKind, string> = { success: 'Done', warning: 'Please check', error: 'Something went wrong' }

let nextId = 1

export const feedback = reactive({
  popups: [] as Popup[],
  busy: [] as BusyEntry[]
})

export function showPopup(kind: PopupKind, message: string, onClose?: () => void, title?: string): void {
  if (!message) return
  // The same message already showing: do not stack a duplicate.
  if (feedback.popups.some(p => p.kind === kind && p.message === message)) return
  feedback.popups.push({ id: nextId++, kind, title: title ?? TITLES[kind], message, onClose })
}

export const showSuccess = (message: string, onClose?: () => void) => showPopup('success', message, onClose)
export const showWarning = (message: string, onClose?: () => void) => showPopup('warning', message, onClose)
export const showError = (message: string, onClose?: () => void) => showPopup('error', message, onClose)

export function closePopup(id: number): void {
  const i = feedback.popups.findIndex(p => p.id === id)
  if (i < 0) return
  const [popup] = feedback.popups.splice(i, 1)
  popup?.onClose?.()
}

export function startBusy(spec: BusySpec): number {
  const token = nextId++
  feedback.busy.push({ ...spec, token, startedAt: Date.now() })
  return token
}

export function stopBusy(token: number): void {
  const i = feedback.busy.findIndex(b => b.token === token)
  if (i >= 0) feedback.busy.splice(i, 1)
}
