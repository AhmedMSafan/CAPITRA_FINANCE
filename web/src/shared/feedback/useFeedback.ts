// Connects a page's message and busy refs to the app-wide pop-up and progress screen.
import { onUnmounted, watch, type Ref } from 'vue'
import { showPopup, startBusy, stopBusy, type BusySpec, type PopupKind } from './feedback'

type Options = {
  /** Error messages: each new value opens an error pop-up; closing it clears the ref. */
  errors?: Ref<string>[]
  /** Success messages, shown the same way. */
  notices?: Ref<string>[]
  /** Kind of a notice pop-up when not every notice is good news (e.g. "validated with errors"). */
  noticeKind?: () => PopupKind
  /** While the ref is truthy the progress screen shows; the function names what is running. */
  busy?: Ref<unknown>
  busyAs?: (value: unknown) => BusySpec
}

export function useFeedback(options: Options): void {
  const bind = (r: Ref<string>, kind: () => PopupKind) =>
    watch(r, value => {
      if (value) showPopup(kind(), value, () => { if (r.value === value) r.value = '' })
    })

  for (const r of options.errors ?? []) bind(r, () => 'error')
  for (const r of options.notices ?? []) bind(r, options.noticeKind ?? (() => 'success'))

  let token = 0
  const stop = () => { if (token) { stopBusy(token); token = 0 } }
  if (options.busy) {
    watch(options.busy, value => {
      if (value && !token) token = startBusy(options.busyAs?.(value) ?? { title: 'Working…' })
      else if (!value) stop()
    })
  }
  onUnmounted(stop)
}
