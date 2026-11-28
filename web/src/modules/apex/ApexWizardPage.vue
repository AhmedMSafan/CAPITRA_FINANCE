<script setup lang="ts">
// One wizard-built page of a copied app: its interactive reports, its form and its filter items, as APEX rendered them.
// Modal pages open as a dialog over the report that links to them; closing it refreshes that report.
import { computed, onBeforeUnmount, provide, reactive, ref, watch } from 'vue'
import { http } from '@/shared/api/http'
import { useRoute, useRouter } from 'vue-router'
import ApexFormRegion from './ApexFormRegion.vue'
import ApexHtmlRegion from './ApexHtmlRegion.vue'
import ApexIrRegion from './ApexIrRegion.vue'
import ApexItemsRegion from './ApexItemsRegion.vue'
import { createApex, releasePageHandlers, routerUrl, runPageScript } from './apexRuntime'
import { ApiError } from '@/shared/api/http'
import { appPageUrl, flash, pageValuesKey, type WizardButton, type WizardPage } from './wizard'
import 'font-awesome/css/font-awesome.min.css'
import './apexAlerts.css'
import './apexUt.css'

// query: the items a modal page opened in a dialog gets from its link (a page of its own reads them from the URL)
const props = defineProps<{ app: number; pages: WizardPage[]; page: number; query?: Record<string, string> }>()
// reload: the page opens again after a submit, as APEX shows it again; close: a dialog page closes (Close Dialog)
const emit = defineEmits<{ reload: []; close: [message?: string] }>()

const route = useRoute()
const router = useRouter()
const spec = computed(() => props.pages.find((p) => p.page === props.page)!)

// Before Header: the items its process set (PAGE_LOAD)
const loaded = ref<Record<string, string>>({})
const loadDone = ref(false)
const error = ref('')

// page items: the item defaults, then the query (?p601_source_id=3 is P601_SOURCE_ID), as session state
const items = computed<Record<string, string>>(() => {
  const out: Record<string, string> = {}
  for (const region of spec.value.regions)
    if (region.kind === 'items') for (const item of region.items ?? []) if (item.default != null) out[item.name] = item.default
  for (const [k, v] of Object.entries(urlItems.value))
    if ((/^p?\d+_/i.test(k) || k === 'request') && typeof v === 'string') out[k.toUpperCase()] = v
  return { ...out, ...loaded.value }
})
const urlItems = computed<Record<string, unknown>>(() => props.query ?? route.query)

// the values of the page's items as the user changes them in its regions of items; a submit sends them
const values = reactive<Record<string, string>>({})
provide(pageValuesKey, values)
watch(
  items,
  (now) => {
    for (const k of Object.keys(values)) delete values[k]
    for (const region of spec.value.regions)
      if (region.kind === 'items') for (const item of region.items ?? []) values[item.name] = now[item.name] ?? ''
  },
  { immediate: true }
)

// Before Header: the process that sets page items from the items in the URL
watch(
  () => route.fullPath,
  async () => {
    loaded.value = {}
    if (spec.value.load) {
      const query: Record<string, string> = {}
      for (const [k, v] of Object.entries(urlItems.value)) if (typeof v === 'string') query[k.toUpperCase()] = v
      try {
        const res = await http.post<{ ok: boolean; message?: string; items?: Record<string, string> }>(`/api/apex/${props.app}/${props.page}/PAGE_LOAD`, {
          x01: JSON.stringify(query)
        })
        // an error the Before Header process raised shows on the page, as APEX showed it
        if (res.ok === false) error.value = res.message ?? 'The page could not be loaded.'
        loaded.value = res.items ?? {}
      } catch (e) {
        error.value = e instanceof Error ? e.message : 'The page could not be loaded.'
      }
    }
    loadDone.value = true
  },
  { immediate: true }
)

// the page's scripts: `apex` for them, run once every region with HTML is on the page
const root = ref<HTMLElement | null>(null)
let apexApi: ReturnType<typeof createApex> | null = null
const apex = computed(() => {
  if (!loadDone.value) return undefined
  apexApi ??= createApex(props.app, props.page, { ...items.value }, {
    push: (url: string) => void router.push(routerUrl(url)),
    replace: (url: string) => void router.replace(routerUrl(url))
  })
  return apexApi
})
const htmlRegions = computed(() => spec.value.regions.filter((r) => r.kind === 'plsql' || r.kind === 'html').length)
const regionScripts = new Map<string, () => void>()
let scriptsRan = false
function regionReady(key: string, run: () => void) {
  regionScripts.set(key, run)
  if (regionScripts.size >= htmlRegions.value) void runScripts()
}
async function runScripts() {
  if (scriptsRan || !apex.value) return
  scriptsRan = true
  const api = apex.value
  for (const region of spec.value.regions) regionScripts.get(region.key)?.()
  // the JavaScript file URLs load first, in their order, then the page's own code (as APEX wrote the page)
  for (const url of spec.value.jsFiles ?? []) runPageScript(await pageFile(url), api)
  if (spec.value.js) runPageScript(spec.value.js, api)
  if (spec.value.onload) runPageScript(spec.value.onload, api)
}
const files = new Map<string, Promise<string>>()
function pageFile(url: string): Promise<string> {
  let text = files.get(url)
  if (!text) {
    text = fetch(url).then((r) => (r.ok ? r.text() : ''))
    files.set(url, text)
  }
  return text
}
watch(loadDone, (d) => {
  if (d && htmlRegions.value === 0) setTimeout(() => void runScripts())
})

// links in the regions' HTML are Capitra routes; a link to a modal page of the app opens it as a dialog
function followLink(event: MouseEvent) {
  const link = (event.target as HTMLElement).closest?.('a[href^="/"]') as HTMLAnchorElement | null
  if (!link || event.defaultPrevented || event.ctrlKey || event.metaKey || event.shiftKey || link.target === '_blank') return
  event.preventDefault()
  go(link.getAttribute('href')!)
}

// a row the region's script follows with window.location.href = its data-href (Investors on page 109): the same,
// before the script's own handler; APEX's get_url of a modal page was a javascript: URL that opened the dialog
function followDataHref(event: MouseEvent | KeyboardEvent) {
  const target = event.target as HTMLElement
  const row = target.closest?.('[data-href^="/"]')
  if (!row || target.closest('a, button, input, select, textarea')) return
  if (event instanceof KeyboardEvent && event.key !== 'Enter' && event.key !== ' ') return
  if (event instanceof MouseEvent && (event.ctrlKey || event.metaKey || event.shiftKey)) return
  event.preventDefault()
  event.stopPropagation()
  go(row.getAttribute('data-href')!)
}

function go(href: string) {
  const to = router.resolve(href)
  const page = props.pages.find((p) => p.modal && appPageUrl(props.app, p.page) === to.path)
  if (page) {
    const set: Record<string, string> = {}
    for (const [k, v] of Object.entries(to.query)) if (typeof v === 'string') set[k.toUpperCase()] = v
    open(page.page, set)
  } else void router.push(href)
}

onBeforeUnmount(() => releasePageHandlers())

const success = ref('')
const reports = ref<InstanceType<typeof ApexIrRegion>[]>([])

// a modal page opened over this one, with the items its link set
const dialog = ref<{ page: WizardPage; pk: string; items: Record<string, string>; form: boolean } | null>(null)

function open(page: number | null, set: Record<string, string> = {}) {
  if (page == null) {
    // Cancel of a dialog page: Close Dialog
    if (props.query) emit('close')
    else router.back()
    return
  }
  const target = props.pages.find((p) => p.page === page)
  if (target?.modal) {
    const form = target.regions.some((r) => r.kind === 'form')
    const pkItem = form ? formOf(target).pkItem : undefined
    dialog.value = { page: target, pk: (pkItem && set[pkItem]) || '', items: set, form }
    return
  }
  void router.push(appPageUrl(props.app, page, set))
}

// Submit page from a region of items: the values become the page's items and the reports run with them
async function submitItems(changed: Record<string, string>, request: string) {
  const all = { ...items.value, ...values, ...changed }
  if (spec.value.submit && !(await submitPage(request, all))) return
  void router.replace(appPageUrl(props.app, props.page, Object.fromEntries(Object.entries(all).filter(([k]) => !(k in loaded.value) && k !== 'REQUEST'))))
}

// Submit page: the page's After Submit processes for the button pressed (REQUEST), then the page again
async function submitPage(request: string, sent: Record<string, string> = { ...items.value, ...values }) {
  error.value = ''
  try {
    const res = await http.post<{ ok?: boolean; message?: string; items?: Record<string, string | null> }>(`/api/apex/${props.app}/${props.page}/SUBMIT`, {
      items: { ...sent, REQUEST: request }
    })
    if (res.ok === false) {
      error.value = res.message ?? 'The request failed.'
      return false
    }
    // a modal page closes after its processes (Close Dialog); the page under it opens again
    if (spec.value.modal) {
      if (props.query) emit('close', res.message ?? '')
      else {
        flash.message = res.message ?? ''
        router.back()
      }
      return false
    }
    flash.message = res.message ?? ''
    // the items the processes set (:P208_BATCH_STATUS := ...) stay in the session for the page shown next, as in APEX
    if (res.items && Object.keys(res.items).length) {
      const kept = Object.fromEntries(Object.entries(sent).filter(([k]) => !(k in loaded.value) && k !== 'REQUEST'))
      for (const [k, v] of Object.entries(res.items)) kept[k] = v ?? ''
      const url = appPageUrl(props.app, props.page, Object.fromEntries(Object.entries(kept).filter(([, v]) => v !== '')))
      if (router.resolve(url).fullPath === router.currentRoute.value.fullPath) emit('reload')
      else void router.replace(url)
      return false
    }
    emit('reload')
    return true
  } catch (e) {
    error.value = e instanceof ApiError || e instanceof Error ? e.message : 'The request failed.'
    return false
  }
}

function formOf(page: WizardPage) {
  return page.regions.find((r) => r.kind === 'form')!
}

// Dialog Closed: refresh the reports; after the processes of a dialog page (a message) this page opens again
function closed(message?: string) {
  const form = dialog.value?.form ?? true
  dialog.value = null
  if (!form && message !== undefined) {
    flash.message = message
    emit('reload')
    return
  }
  if (message) success.value = message
  for (const r of reports.value) void r.refresh()
  // Dialog Closed of the page's scripts (apex.navigation.dialog fires it on close)
  apexApi?.jQuery(document.body).trigger('apexafterclosedialog')
}

// After Process form on a normal page APEX shows the page again: with the new row after Create, empty after Delete.
function done(request: string, id: string | null, message: string) {
  success.value = message
  const form = formOf(spec.value)
  if (request === 'CREATE' && id && form.pkItem) void router.replace(appPageUrl(props.app, props.page, { [form.pkItem]: id }))
  else if (request === 'DELETE') void router.replace(appPageUrl(props.app, props.page))
  else for (const r of reports.value) void r.refresh()
}

function cancel(button: WizardButton) {
  if (button.page) void router.push(appPageUrl(props.app, button.page))
  else router.back()
}

const pk = computed(() => {
  const form = spec.value.regions.find((r) => r.kind === 'form')
  return form?.pkItem ? (items.value[form.pkItem] ?? '') : ''
})

watch(success, (m) => {
  if (m) setTimeout(() => (success.value = ''), 5000)
})

// the message of the submit that opened the page again
if (flash.message) {
  success.value = flash.message
  flash.message = ''
}
</script>

<template>
  <div ref="root" class="ut-page t-Body-contentInner" @click.capture="followDataHref" @keydown.capture="followDataHref" @click="followLink">
    <link v-for="url in spec.cssFiles ?? []" :key="url" rel="stylesheet" :href="url" />
    <component :is="'style'" v-if="spec.css">{{ spec.css }}</component>
    <div v-if="error" class="apx-alerts">
      <div class="apx-alert apx-alert--danger" role="alert">
        <span class="apx-alert-ico" aria-hidden="true">!</span>
        <div class="apx-alert-body">{{ error }}</div>
        <button type="button" class="apx-alert-x" aria-label="Close" @click="error = ''">&times;</button>
      </div>
    </div>
    <div v-if="success" class="apx-alerts">
      <div class="apx-alert apx-alert--success" role="status">
        <span class="apx-alert-ico" aria-hidden="true">&#10003;</span>
        <div class="apx-alert-body">{{ success }}</div>
        <button type="button" class="apx-alert-x" aria-label="Close" @click="success = ''">&times;</button>
      </div>
    </div>

    <template v-for="region in loadDone ? spec.regions : []" :key="region.key">
      <!-- Breadcrumb (Breadcrumb Bar position): the page's entry and its parents -->
      <div v-if="region.kind === 'breadcrumb'" class="t-BreadcrumbRegion" :class="region.cls">
        <div class="t-BreadcrumbRegion-body">
          <div class="t-BreadcrumbRegion-breadcrumb">
            <ul class="t-Breadcrumb">
              <li v-for="(c, n) in region.crumbs" :key="n" class="t-Breadcrumb-item" :class="{ 'is-active': n === region.crumbs!.length - 1 }">
                <h1 v-if="n === region.crumbs!.length - 1" class="t-Breadcrumb-label">{{ c.label }}</h1>
                <a v-else class="t-Breadcrumb-label" href="#" @click.prevent="open(c.page)">{{ c.label }}</a>
              </li>
            </ul>
          </div>
        </div>
      </div>
      <ApexIrRegion v-else-if="region.kind === 'ir'" ref="reports" :app="app" :page="page" :region="region" :items="items" @open="open" @submit="submitPage" />
      <ApexItemsRegion
        v-else-if="region.kind === 'items'"
        :app="app"
        :page="page"
        :region="region"
        :items="items"
        @submit="submitItems"
        @open="open"
      />
      <ApexHtmlRegion
        v-else-if="region.kind === 'plsql' || region.kind === 'html'"
        :app="app"
        :page="page"
        :region="region"
        :items="items"
        :apex="apex"
        @ready="(run: () => void) => regionReady(region.key, run)"
        @open="open"
        @submit="submitPage"
      />
      <ApexFormRegion
        v-else
        :app="app"
        :page="page"
        :region="region"
        :pk="pk"
        :items="items"
        :plain="spec.modal"
        @done="done"
        @cancel="cancel"
      />
    </template>

    <div v-if="dialog" class="ui-dialog-back" @keydown.esc="closed()">
      <div class="ui-dialog" role="dialog" aria-modal="true" :aria-label="dialog.page.title">
        <div class="ui-dialog-titlebar">
          <h2 class="ui-dialog-title">{{ dialog.page.title }}</h2>
          <button type="button" class="ui-dialog-close" aria-label="Close" @click="closed()">&times;</button>
        </div>
        <div class="ui-dialog-content">
          <ApexWizardPage v-if="!dialog.form" :app="app" :pages="pages" :page="dialog.page.page" :query="dialog.items" @close="closed" />
          <ApexFormRegion
            v-else
            :app="app"
            :page="dialog.page.page"
            :region="formOf(dialog.page)"
            :pk="dialog.pk"
            :items="dialog.items"
            plain
            @done="(_r, _id, m) => closed(m)"
            @cancel="closed()"
          />
        </div>
      </div>
    </div>
  </div>
</template>
