// The small part of the Oracle APEX JavaScript API that the copied App pages use,
// so their scripts run unchanged: apex.server.process goes to the ported C# processes,
// apex.message shows Universal Theme style alerts, apex.item reads the page items.
import jQuery from 'jquery'
import { http, ApiError } from '@/shared/api/http'

type ProcessData = Record<string, unknown> & { p_clob_01?: string }
type ProcessOptions = {
  dataType?: string
  success?: (data: unknown) => void
  error?: (jqXHR: unknown, status: string, error: string) => void
}
type ApexError = { message: string; type?: string; location?: string | string[]; unsafe?: boolean }

const escapeMap: Record<string, string> = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#x27;', '/': '&#x2F;' }

export function escapeHTML(value: unknown): string {
  return String(value ?? '').replace(/[&<>"'/]/g, (c) => escapeMap[c] ?? c)
}

function alertHost(): HTMLElement {
  let host = document.getElementById('apexAlerts')
  if (!host) {
    host = document.createElement('div')
    host.id = 'apexAlerts'
    host.className = 'apx-alerts'
    document.body.appendChild(host)
  }
  return host
}

function showAlert(kind: 'success' | 'danger', html: string, autoClose: boolean) {
  const host = alertHost()
  const el = document.createElement('div')
  el.className = `apx-alert apx-alert--${kind}`
  el.setAttribute('role', kind === 'success' ? 'status' : 'alert')
  el.innerHTML = `<span class="apx-alert-ico" aria-hidden="true">${kind === 'success' ? '&#10003;' : '!'}</span><div class="apx-alert-body">${html}</div><button type="button" class="apx-alert-x" aria-label="Close">&times;</button>`
  el.querySelector('button')?.addEventListener('click', () => el.remove())
  host.appendChild(el)
  if (autoClose) setTimeout(() => el.remove(), 5000)
}

function confirmDialog(message: string, callback: (ok: boolean) => void) {
  const back = document.createElement('div')
  back.className = 'apx-confirm-back'
  back.innerHTML = `<div class="apx-confirm" role="dialog" aria-modal="true"><p>${escapeHTML(message)}</p><div class="apx-confirm-f"><button type="button" class="apx-btn" data-ok="0">Cancel</button><button type="button" class="apx-btn apx-btn--hot" data-ok="1">OK</button></div></div>`
  back.addEventListener('click', (e) => {
    const ok = (e.target as HTMLElement).dataset.ok
    if (ok === undefined) return
    back.remove()
    callback(ok === '1')
  })
  document.body.appendChild(back)
  back.querySelector<HTMLButtonElement>('[data-ok="1"]')?.focus()
}

/** How a page moves to another URL: through the Vue router, so the app keeps its state. */
export type ApexNavigator = {
  /** Opens another page (apex.navigation.redirect, links). */
  push(url: string): void
  /** Changes the address of the current page without reloading it (history.replaceState). */
  replace(url: string): void
}

/** The part of a URL the router takes: path, query and hash. */
export function routerUrl(url: string): string {
  const u = new URL(url, location.href)
  return u.pathname + u.search + u.hash
}

/** Builds the `apex` object for one page of one app. `items` holds the page items (P50_ID ...). */
export function createApex(app: number, page: number, items: Record<string, string>, nav: ApexNavigator) {
  const $ = jQuery

  const server = {
    process(name: string, data: ProcessData = {}, options: ProcessOptions = {}) {
      const deferred = $.Deferred()
      const body: Record<string, unknown> = {}
      for (let i = 1; i <= 20; i++) {
        const key = `x${String(i).padStart(2, '0')}`
        if (data[key] !== undefined && data[key] !== null) body[key] = String(data[key])
      }
      if (data.p_clob_01 !== undefined) body.clob01 = data.p_clob_01
      // the page's session state goes with every call, as APEX keeps it on the server
      body.items = { ...items }
      http.post<unknown>(`/api/apex/${app}/${page}/${encodeURIComponent(name)}`, body).then(
        (result) => deferred.resolve(result),
        (err: unknown) => {
          const message = err instanceof ApiError ? err.message : 'The request failed.'
          deferred.reject({ status: err instanceof ApiError ? err.status : 0, responseText: message }, 'error', message)
        }
      )
      if (options.success) deferred.done(options.success)
      if (options.error) deferred.fail(options.error)
      return deferred.promise()
    }
  }

  const message = {
    showPageSuccess(text: string) {
      alertHost().querySelectorAll('.apx-alert--success').forEach((el) => el.remove())
      showAlert('success', escapeHTML(text), true)
    },
    showErrors(errors: ApexError | ApexError[]) {
      const list = Array.isArray(errors) ? errors : [errors]
      const html = list.map((e) => (e.unsafe === false ? e.message : escapeHTML(e.message))).join('<br>')
      showAlert('danger', html, false)
    },
    clearErrors() {
      alertHost().querySelectorAll('.apx-alert--danger').forEach((el) => el.remove())
    },
    confirm: confirmDialog,
    alert(text: string, callback?: () => void) {
      showAlert('danger', escapeHTML(text), false)
      callback?.()
    }
  }

  const item = (name: string) => ({
    getValue: () => items[name] ?? '',
    setValue: (value: unknown) => {
      items[name] = value == null ? '' : String(value)
    }
  })

  const navigation = {
    redirect(url: string) {
      nav.push(routerUrl(url))
    }
  }

  // history.replaceState / pushState as the page scripts call them, through the router.
  const history = {
    get length() {
      return window.history.length
    },
    get state() {
      return window.history.state as unknown
    },
    replaceState(_state: unknown, _title: string, url?: string | URL | null) {
      if (url != null) nav.replace(routerUrl(String(url)))
    },
    pushState(_state: unknown, _title: string, url?: string | URL | null) {
      if (url != null) nav.push(routerUrl(String(url)))
    },
    back: () => window.history.back(),
    forward: () => window.history.forward(),
    go: (delta?: number) => window.history.go(delta)
  }

  return {
    jQuery: $,
    server,
    message,
    item,
    navigation,
    history,
    util: { escapeHTML },
    env: { APP_ID: String(app), APP_PAGE_ID: String(page) }
  }
}

// Listeners that page scripts add to document and window, removed again when the page closes.
type Added = { target: EventTarget; type: string; listener: EventListenerOrEventListenerObject; options?: boolean | AddEventListenerOptions }
const added: Added[] = []

function tracked(run: () => void) {
  const targets: EventTarget[] = [document, window]
  const originals = targets.map((t) => t.addEventListener)
  targets.forEach((target, i) => {
    target.addEventListener = function (type: string, listener: EventListenerOrEventListenerObject | null, options?: boolean | AddEventListenerOptions) {
      if (listener) added.push({ target, type, listener, options })
      originals[i]!.call(target, type, listener, options)
    }
  })
  try {
    run()
  } finally {
    targets.forEach((target, i) => (target.addEventListener = originals[i]!))
  }
}

/** Runs a copied page script with `apex`, `$`, `$v`, `$s` and `history` in scope, as APEX provides them. */
export function runPageScript(source: string, apex: ReturnType<typeof createApex>) {
  const $v = (name: string) => apex.item(name).getValue()
  const $s = (name: string, value: unknown) => apex.item(name).setValue(value)
  // The scripts are part of this app (copied from the APEX export), not user input.
  const fn = new Function('apex', '$', 'jQuery', '$v', '$s', 'history', source)
  tracked(() => fn(apex, apex.jQuery, apex.jQuery, $v, $s, apex.history))
}

/** Runs the <script> blocks of server-rendered region HTML (innerHTML does not run them). */
export function runInlineScripts(container: HTMLElement, apex: ReturnType<typeof createApex>) {
  container.querySelectorAll('script').forEach((el) => {
    const source = el.textContent ?? ''
    el.remove()
    runPageScript(source, apex)
  })
}

/** Removes the handlers a page script put on document and window, when the page closes. */
export function releasePageHandlers() {
  jQuery(document).off()
  jQuery(window).off()
  for (const a of added.splice(0)) a.target.removeEventListener(a.type, a.listener, a.options)
  document.getElementById('apexAlerts')?.remove()
}
