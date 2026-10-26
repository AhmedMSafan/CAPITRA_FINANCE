<script setup lang="ts">
// One wizard-built page of App 120: its interactive reports and its form, as APEX rendered them.
// Modal pages open as a dialog over the report that links to them; closing it refreshes that report.
import { computed, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { http } from '@/shared/api/http'
import ApexFormRegion from './ApexFormRegion.vue'
import ApexIrRegion from './ApexIrRegion.vue'
import { pageUrl, type WizardButton, type WizardPage } from './wizard'
import './apexAlerts.css'
import './apexUt.css'

const props = defineProps<{ app: number; pages: WizardPage[]; page: number }>()

const route = useRoute()
const router = useRouter()
const spec = computed(() => props.pages.find((p) => p.page === props.page)!)

// page items from the query: ?p601_source_id=3 is P601_SOURCE_ID
const items = computed<Record<string, string>>(() => {
  const out: Record<string, string> = {}
  for (const [k, v] of Object.entries(route.query)) if (/^p\d+_/i.test(k) && typeof v === 'string') out[k.toUpperCase()] = v
  return out
})

const lovs = reactive<Record<number, Record<string, [string | null, string][]>>>({})
async function loadLovs(page: number) {
  const form = props.pages.find((p) => p.page === page)?.regions.find((r) => r.kind === 'form')
  if (lovs[page] || !form?.items?.some((i) => i.lov)) return
  lovs[page] = await http.post(`/api/apex/${props.app}/${page}/LOVS`, {})
}
watch(() => props.page, (p) => void loadLovs(p), { immediate: true })

const success = ref('')
const reports = ref<InstanceType<typeof ApexIrRegion>[]>([])

// a modal page opened over this one
const dialog = ref<{ page: WizardPage; pk: string } | null>(null)

async function open(page: number, item?: string, value?: unknown) {
  const target = props.pages.find((p) => p.page === page)
  if (target?.modal) {
    await loadLovs(page)
    dialog.value = { page: target, pk: value == null ? '' : String(value) }
    return
  }
  void router.push(pageUrl(page, item, value))
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
  if (request === 'CREATE' && id) void router.replace(pageUrl(props.page, form.pkItem, id))
  else if (request === 'DELETE') void router.replace(pageUrl(props.page))
  else for (const r of reports.value) void r.refresh()
}

function cancel(button: WizardButton) {
  if (button.page) void router.push(pageUrl(button.page))
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
    <div v-if="success" class="apx-alerts">
      <div class="apx-alert apx-alert--success" role="status">
        <span class="apx-alert-ico" aria-hidden="true">&#10003;</span>
        <div class="apx-alert-body">{{ success }}</div>
        <button type="button" class="apx-alert-x" aria-label="Close" @click="success = ''">&times;</button>
      </div>
    </div>

    <template v-for="region in spec.regions" :key="region.key">
      <ApexIrRegion
        v-if="region.kind === 'ir'"
        ref="reports"
        :app="app"
        :page="page"
        :region="region"
        :items="items"
        @edit="open"
        @create="(p) => open(p)"
      />
      <ApexFormRegion
        v-else
        :app="app"
        :page="page"
        :region="region"
        :pk="pk"
        :lovs="lovs[page] ?? {}"
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
            :lovs="lovs[dialog.page.page] ?? {}"
            plain
            @done="(_r, _id, m) => closed(m)"
            @cancel="closed()"
          />
        </div>
      </div>
    </div>
  </div>
</template>
