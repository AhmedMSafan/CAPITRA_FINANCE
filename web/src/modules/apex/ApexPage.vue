<script setup lang="ts">
// Hosts one page copied from an APEX application: its region HTML, its page CSS and its page
// scripts run as they were written, against the apex runtime in apexRuntime.ts.
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { http } from '@/shared/api/http'
import { createApex, releasePageHandlers, routerUrl, runInlineScripts, runPageScript } from './apexRuntime'

const props = defineProps<{
  app: number
  page: number
  html: string
  css: string
  scripts: string[]
  items?: Record<string, string>
}>()

const host = ref<HTMLElement | null>(null)
const route = useRoute()
const router = useRouter()
let style: HTMLStyleElement | null = null
// The address the page itself set with history.replaceState: the router follows it without reloading the page.
let ownUrl: string | null = null
let mountedPath = ''

function pageUrl(r: { path: string; query: unknown } = route) {
  return r.path + JSON.stringify(r.query)
}

// APEX loads the page again when it is opened with other items (?p50_id=...) or opened again from its own link.
function open(url: string) {
  if (routerUrl(url).split('#')[0] === route.fullPath.split('#')[0]) {
    void mount()
    return
  }
  void router.push(url)
}

const nav = {
  push: open,
  replace(url: string) {
    ownUrl = pageUrl(router.resolve(url))
    void router.replace(url)
  }
}

// Links inside the copied HTML point at Capitra routes (the server maps APEX page numbers to them).
function followLink(event: MouseEvent) {
  const link = (event.target as HTMLElement).closest?.('a[href^="/"]') as HTMLAnchorElement | null
  if (!link || event.defaultPrevented || event.ctrlKey || event.metaKey || event.shiftKey || link.target === '_blank') return
  event.preventDefault()
  open(link.getAttribute('href')!)
}

// Page items arrive in the query in lower case, e.g. ?p50_id=12 is P50_ID.
function queryItems(): Record<string, string> {
  const items: Record<string, string> = {}
  for (const [key, value] of Object.entries(route.query)) {
    if (/^p\d+_/i.test(key) && typeof value === 'string') items[key.toUpperCase()] = value
  }
  return items
}

function unmount() {
  releasePageHandlers()
  host.value?.removeEventListener('click', followLink)
}

async function mount() {
  unmount()
  ownUrl = pageUrl()
  mountedPath = route.path
  const el = host.value!
  el.innerHTML = props.html
  el.addEventListener('click', followLink)
  const apex = createApex(props.app, props.page, { ...queryItems(), ...props.items }, nav)

  // PL/SQL dynamic content regions: <div data-apex-region="PROCESS"> is filled with the HTML the ported process renders.
  for (const slot of Array.from(el.querySelectorAll<HTMLElement>('[data-apex-region]'))) {
    const result = await http.post<{ html: string }>(`/api/apex/${props.app}/${props.page}/${slot.dataset.apexRegion}`, {})
    slot.outerHTML = result.html
  }
  runInlineScripts(el, apex)
  for (const script of props.scripts) runPageScript(script, apex)
}

onMounted(() => {
  style = document.createElement('style')
  style.dataset.apexPage = `${props.app}-${props.page}`
  style.textContent = props.css
  document.head.appendChild(style)
  void mount()
})

watch(
  () => pageUrl(),
  (url) => {
    // another page: this one is closing
    if (route.path !== mountedPath) return
    if (url !== ownUrl) void mount()
  }
)

onBeforeUnmount(() => {
  unmount()
  style?.remove()
})
</script>

<template>
  <div ref="host" class="t-Body-contentInner apx-page"></div>
</template>

<style>
/* Universal Theme page message and confirm dialog, as APEX shows them. */
.apx-alerts {
  position: fixed;
  top: 76px;
  right: 16px;
  z-index: 2000;
  display: flex;
  flex-direction: column;
  gap: 8px;
  width: min(420px, calc(100vw - 32px));
}
.apx-alert {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 14px 14px 14px 16px;
  border-radius: 4px;
  background: #fff;
  color: #262626;
  font: 14px/1.45 'Segoe UI', Arial, sans-serif;
  box-shadow: 0 4px 16px rgb(0 0 0 / 18%);
}
.apx-alert--success {
  border-left: 4px solid #3c763d;
}
.apx-alert--danger {
  border-left: 4px solid #c9302c;
}
.apx-alert-ico {
  flex: none;
  display: grid;
  place-items: center;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  color: #fff;
  font-weight: 700;
}
.apx-alert--success .apx-alert-ico {
  background: #3c763d;
}
.apx-alert--danger .apx-alert-ico {
  background: #c9302c;
}
.apx-alert-body {
  flex: 1;
  padding-top: 2px;
}
.apx-alert-x {
  border: 0;
  background: none;
  font-size: 20px;
  line-height: 1;
  cursor: pointer;
  color: #666;
}
.apx-confirm-back {
  position: fixed;
  inset: 0;
  z-index: 2100;
  display: grid;
  place-items: center;
  background: rgb(0 0 0 / 40%);
}
.apx-confirm {
  width: min(440px, calc(100vw - 32px));
  padding: 20px;
  border-radius: 4px;
  background: #fff;
  font: 14px/1.5 'Segoe UI', Arial, sans-serif;
}
.apx-confirm-f {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 16px;
}
.apx-btn {
  padding: 8px 14px;
  border: 1px solid #ccc;
  border-radius: 4px;
  background: #f8f8f8;
  cursor: pointer;
}
.apx-btn--hot {
  border-color: #056ac8;
  background: #056ac8;
  color: #fff;
}
</style>
