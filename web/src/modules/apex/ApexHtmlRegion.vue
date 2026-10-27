<script setup lang="ts">
// A static content region (its HTML) or a PL/SQL dynamic content region: the server process REGION_<key>
// writes the same HTML the PL/SQL printed, for the page items.
import { computed, nextTick, ref, watch } from 'vue'
import { http } from '@/shared/api/http'
import { runInlineScripts, type createApex } from './apexRuntime'
import { resolveSet, shown, substitute, type WizardButton, type WizardRegion } from './wizard'

const props = defineProps<{ app: number; page: number; region: WizardRegion; items: Record<string, string>; apex?: ReturnType<typeof createApex> }>()
// ready: the region's HTML is on the page, with the function that runs its scripts (the page runs them once every
// region's HTML is there, in the regions' order, as APEX rendered the whole page first); open: a button that redirects
const emit = defineEmits<{ ready: [runScripts: () => void]; open: [page: number | null, items: Record<string, string>]; submit: [request: string] }>()

const body = ref<HTMLElement | null>(null)
const buttons = computed(() => (props.region.buttons ?? []).filter((b) => shown(b, props.items)))
// Close, Previous, Delete and Below Region buttons at the start, the others (Next, Create, Change) at the end
const START = ['CLOSE', 'PREVIOUS', 'DELETE', 'BELOW_BOX']
const startButtons = computed(() => buttons.value.filter((b) => START.includes(b.position ?? '')))
const endButtons = computed(() => buttons.value.filter((b) => !START.includes(b.position ?? '')))
function buttonClass(b: WizardButton) {
  return b.position === 'DELETE' ? 't-Button--danger' : b.action === 'SUBMIT' ? 't-Button--hot' : ''
}

function press(button: WizardButton) {
  if (button.action === 'SUBMIT') emit('submit', button.name)
  else emit('open', button.page, resolveSet(button.set, {}, props.items))
}

const dynamic = ref('')
const html = computed(() => (props.region.kind === 'plsql' ? dynamic.value : substitute(props.region.html ?? '', props.items)))

async function load() {
  if (props.region.kind !== 'plsql') return done()
  try {
    const res = await http.post<{ ok: boolean; html?: string }>(`/api/apex/${props.app}/${props.page}/REGION_${props.region.key}`, {
      items: props.items
    })
    dynamic.value = res.ok === false ? '' : (res.html ?? '')
  } catch {
    dynamic.value = ''
  }
  await done()
}

// the <script> blocks the PL/SQL printed run once the whole page is there, as in APEX
async function done() {
  await nextTick()
  emit('ready', () => {
    if (body.value && props.apex) runInlineScripts(body.value, props.apex)
  })
}

watch(() => JSON.stringify(props.items), load, { immediate: true })
</script>

<template>
  <div v-if="html || buttons.length" :id="region.staticId" class="t-Region" :class="[region.cls, { 't-Region--plain': region.hideHeader }]">
    <div v-if="!region.hideHeader" class="t-Region-header"><h2 class="t-Region-title">{{ region.title }}</h2></div>
    <!-- eslint-disable-next-line vue/no-v-html -- the region's own HTML, built on the server from escaped values as APEX did -->
    <div v-show="html" ref="body" class="t-Region-body" v-html="html"></div>
    <div v-if="buttons.length" class="t-Region-buttons">
      <div class="t-Region-buttons-left">
        <button v-for="b in startButtons" :id="b.staticId" :key="b.name" type="button" class="t-Button" :class="buttonClass(b)" @click="press(b)">
          {{ b.label }}
        </button>
      </div>
      <div class="t-Region-buttons-right">
        <button v-for="b in endButtons" :id="b.staticId" :key="b.name" type="button" class="t-Button" :class="buttonClass(b)" @click="press(b)">
          {{ b.label }}
        </button>
      </div>
    </div>
  </div>
</template>
