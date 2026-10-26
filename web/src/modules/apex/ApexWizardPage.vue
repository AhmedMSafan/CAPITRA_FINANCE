<script setup lang="ts">
// One wizard-built page of a copied app: its interactive reports, its form and its filter items, as APEX rendered them.
// Modal pages open as a dialog over the report that links to them; closing it refreshes that report.
import { computed, ref, watch } from 'vue'
import { http } from '@/shared/api/http'
import { useRoute, useRouter } from 'vue-router'
import ApexFormRegion from './ApexFormRegion.vue'
import ApexHtmlRegion from './ApexHtmlRegion.vue'
import ApexIrRegion from './ApexIrRegion.vue'
import ApexItemsRegion from './ApexItemsRegion.vue'
import { appPageUrl, type WizardButton, type WizardPage } from './wizard'
import './apexAlerts.css'
import './apexUt.css'

const props = defineProps<{ app: number; pages: WizardPage[]; page: number }>()

const route = useRoute()
const router = useRouter()
const spec = computed(() => props.pages.find((p) => p.page === props.page)!)

// page items: the item defaults, then the query (?p601_source_id=3 is P601_SOURCE_ID), as session state
const items = computed<Record<string, string>>(() => {
  const out: Record<string, string> = {}
  for (const region of spec.value.regions)
    if (region.kind === 'items') for (const item of region.items ?? []) if (item.default != null) out[item.name] = item.default
  for (const [k, v] of Object.entries(route.query)) if (/^p?\d+_/i.test(k) && typeof v === 'string') out[k.toUpperCase()] = v
  return { ...out, ...loaded.value }
})

// Before Header: the process that sets page items from the items in the URL
const loaded = ref<Record<string, string>>({})
watch(
  () => route.fullPath,
  async () => {
    loaded.value = {}
    if (!spec.value.load) return
    const query: Record<string, string> = {}
    for (const [k, v] of Object.entries(route.query)) if (typeof v === 'string') query[k.toUpperCase()] = v
    const res = await http.post<{ ok: boolean; items?: Record<string, string> }>(`/api/apex/${props.app}/${props.page}/PAGE_LOAD`, {
      x01: JSON.stringify(query)
    })
    loaded.value = res.items ?? {}
  },
  { immediate: true }
)

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
  if (target?.modal) {
    const pkItem = formOf(target).pkItem
    dialog.value = { page: target, pk: (pkItem && set[pkItem]) || '', items: set }
    return
  }
  void router.push(appPageUrl(props.app, page, set))
}

// Submit page from a region of items: the values become the page's items and the reports run with them
function submitItems(values: Record<string, string>) {
  void router.replace(appPageUrl(props.app, props.page, { ...items.value, ...values }))
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
</script>

<template>
  <div class="ut-page">
    <component :is="'style'" v-if="spec.css">{{ spec.css }}</component>
    <div v-if="success" class="apx-alerts">
      <div class="apx-alert apx-alert--success" role="status">
        <span class="apx-alert-ico" aria-hidden="true">&#10003;</span>
        <div class="apx-alert-body">{{ success }}</div>
        <button type="button" class="apx-alert-x" aria-label="Close" @click="success = ''">&times;</button>
      </div>
    </div>

    <template v-for="region in spec.regions" :key="region.key">
      <ApexIrRegion v-if="region.kind === 'ir'" ref="reports" :app="app" :page="page" :region="region" :items="items" @open="open" />
      <ApexItemsRegion v-else-if="region.kind === 'items'" :app="app" :page="page" :region="region" :items="items" @submit="submitItems" @open="open" />
      <ApexHtmlRegion v-else-if="region.kind === 'plsql' || region.kind === 'html'" :app="app" :page="page" :region="region" :items="items" />
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
