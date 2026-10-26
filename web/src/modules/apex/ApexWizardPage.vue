<script setup lang="ts">
// One wizard-built page of a copied app: its interactive reports, its form and its filter items, as APEX rendered them.
// Modal pages open as a dialog over the report that links to them; closing it refreshes that report.
import { computed, onBeforeUnmount, ref, watch } from 'vue'
import { http } from '@/shared/api/http'
import { useRoute, useRouter } from 'vue-router'
import ApexFormRegion from './ApexFormRegion.vue'
import ApexHtmlRegion from './ApexHtmlRegion.vue'
import ApexIrRegion from './ApexIrRegion.vue'
import ApexItemsRegion from './ApexItemsRegion.vue'
import { createApex, releasePageHandlers, routerUrl, runPageScript } from './apexRuntime'
import { ApiError } from '@/shared/api/http'
import { appPageUrl, flash, type WizardButton, type WizardPage } from './wizard'
import 'font-awesome/css/font-awesome.min.css'
import './apexAlerts.css'
import './apexUt.css'

const props = defineProps<{ app: number; pages: WizardPage[]; page: number }>()
// reload: the page opens again after a submit, as APEX shows it again
const emit = defineEmits<{ reload: [] }>()

const route = useRoute()
const router = useRouter()
const spec = computed(() => props.pages.find((p) => p.page === props.page)!)

// page items: the item defaults, then the query (?p601_source_id=3 is P601_SOURCE_ID), as session state
const items = computed<Record<string, string>>(() => {
  const out: Record<string, string> = {}
  for (const region of spec.value.regions)
    if (region.kind === 'items') for (const item of region.items ?? []) if (item.default != null) out[item.name] = item.default
  for (const [k, v] of Object.entries(route.query))
    if ((/^p?\d+_/i.test(k) || k === 'request') && typeof v === 'string') out[k.toUpperCase()] = v
  return { ...out, ...loaded.value }
})

// Before Header: the process that sets page items from the items in the URL
const loaded = ref<Record<string, string>>({})
const loadDone = ref(false)
const error = ref('')
watch(
  () => route.fullPath,
  async () => {
    loaded.value = {}
    if (spec.value.load) {
      const query: Record<string, string> = {}
      for (const [k, v] of Object.entries(route.query)) if (typeof v === 'string') query[k.toUpperCase()] = v
      try {
        const res = await http.post<{ ok: boolean; items?: Record<string, string> }>(`/api/apex/${props.app}/${props.page}/PAGE_LOAD`, {
          x01: JSON.stringify(query)
        })
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
let readyCount = 0
let scriptsRan = false
function regionReady() {
  readyCount++
  if (readyCount >= htmlRegions.value) runScripts()
}
function runScripts() {
  if (scriptsRan || !apex.value) return
  scriptsRan = true
  if (spec.value.js) runPageScript(spec.value.js, apex.value)
  if (spec.value.onload) runPageScript(spec.value.onload, apex.value)
}
watch(loadDone, (d) => {
  if (d && htmlRegions.value === 0) setTimeout(runScripts)
})

// links in the regions' HTML are Capitra routes
function followLink(event: MouseEvent) {
  const link = (event.target as HTMLElement).closest?.('a[href^="/"]') as HTMLAnchorElement | null
  if (!link || event.defaultPrevented || event.ctrlKey || event.metaKey || event.shiftKey || link.target === '_blank') return
  event.preventDefault()
  void router.push(link.getAttribute('href')!)
}

onBeforeUnmount(() => releasePageHandlers())

const success = ref('')
const reports = ref<InstanceType<typeof ApexIrRegion>[]>([])

// a modal page opened over this one, with the items its link set
const dialog = ref<{ page: WizardPage; pk: string; items: Record<string, string> } | null>(null)

function open(page: number | null, set: Record<string, string> = {}) {
  if (page == null) {
    router.back()
    return
  }
  const target = props.pages.find((p) => p.page === page)
  if (target?.modal && target.regions.some((r) => r.kind === 'form')) {
    const pkItem = formOf(target).pkItem
    dialog.value = { page: target, pk: (pkItem && set[pkItem]) || '', items: set }
    return
  }
  void router.push(appPageUrl(props.app, page, set))
}

// Submit page from a region of items: the values become the page's items and the reports run with them
async function submitItems(values: Record<string, string>, request: string) {
  const all = { ...items.value, ...values }
  if (spec.value.submit && !(await submitPage(request, all))) return
  void router.replace(appPageUrl(props.app, props.page, Object.fromEntries(Object.entries(all).filter(([k]) => !(k in loaded.value) && k !== 'REQUEST'))))
}

// Submit page: the page's After Submit processes for the button pressed (REQUEST), then the page again
async function submitPage(request: string, values: Record<string, string> = items.value) {
  error.value = ''
  try {
    const res = await http.post<{ ok?: boolean; message?: string }>(`/api/apex/${props.app}/${props.page}/SUBMIT`, {
      items: { ...values, REQUEST: request }
    })
    if (res.ok === false) {
      error.value = res.message ?? 'The request failed.'
      return false
    }
    flash.message = res.message ?? ''
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

// Dialog Closed: refresh the reports
function closed(message?: string) {
  dialog.value = null
  if (message) success.value = message
  for (const r of reports.value) void r.refresh()
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
  <div ref="root" class="ut-page t-Body-contentInner" @click="followLink">
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
      <ApexIrRegion v-if="region.kind === 'ir'" ref="reports" :app="app" :page="page" :region="region" :items="items" @open="open" />
      <ApexItemsRegion v-else-if="region.kind === 'items'" :app="app" :page="page" :region="region" :items="items" @submit="submitItems" @open="open" />
      <ApexHtmlRegion
        v-else-if="region.kind === 'plsql' || region.kind === 'html'"
        :app="app"
        :page="page"
        :region="region"
        :items="items"
        :apex="apex"
        @ready="regionReady"
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
          <ApexFormRegion
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
