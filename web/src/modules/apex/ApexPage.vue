<script setup lang="ts">
// Hosts one page copied from an APEX application: its region HTML, its page CSS and its page
// scripts run as they were written, against the apex runtime in apexRuntime.ts.
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { http } from '@/shared/api/http'
import './apexAlerts.css'
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
