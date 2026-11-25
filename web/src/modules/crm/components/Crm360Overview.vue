<template>
  <!-- Customer 360 > Overview (Ahmed, 26 Nov 2026): the customer's roles and where each stands, the work still running, the latest
       activity, and links to the workspaces of the business lines the customer has. -->
  <div class="ov">
    <div v-if="loading && !data" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
    <template v-else-if="data">
      <section class="ov-box">
        <h3>{{ tr('Roles') }}</h3>
        <div v-for="r in data.roles" :key="r.clientRoleId" class="ov-row">
          <span class="ov-dot" :class="`ov-dot--${tone(r.statusCode)}`" aria-hidden="true"></span>
          <b class="ov-grow">{{ tr(r.roleLabel) }}</b>
          <span class="ov-mut">{{ tr(r.detail) }}<template v-if="r.step"> · {{ tr(r.step) }}</template><template v-if="r.since"> {{ formatDate(r.since) }}</template></span>
          <button type="button" class="ov-link" @click="emit('open-tab', r.tab)">{{ tr('Open') }} <span class="fa fa-angle-right" aria-hidden="true"></span></button>
        </div>
        <p v-if="!data.roles.length" class="ov-mut">{{ tr('No roles yet') }}</p>
      </section>

      <section class="ov-box">
        <h3>{{ tr('Open now') }}</h3>
        <div v-for="o in data.open" :key="`${o.kind}-${o.id}`" class="ov-row">
          <span class="ov-tag">{{ tr(o.label) }}</span>
          <b class="ov-grow" dir="ltr">{{ o.reference }}</b>
          <span class="ov-mut">{{ tr(o.detail) }}<template v-if="o.step"> · {{ tr(o.step) }}</template> · {{ tr('since') }} {{ formatDate(o.since) }}</span>
          <button type="button" class="ov-link" @click="go(o.tab)">{{ tr('Open') }} <span class="fa fa-angle-right" aria-hidden="true"></span></button>
        </div>
        <p v-if="!data.open.length" class="ov-mut">{{ tr('Nothing is waiting for this customer.') }}</p>
      </section>

      <section class="ov-box ov-wide">
        <h3>{{ tr('Latest activity') }}</h3>
        <div v-for="(e, i) in shown" :key="i" class="ov-row">
          <span class="ov-dot" aria-hidden="true"></span>
          <span class="ov-grow"><b>{{ tr(e.subject) }}<template v-if="e.reference"> <span dir="auto">{{ e.reference }}</span></template></b>
            {{ tr(e.text) }}<template v-if="e.detail"> {{ tr(e.detail) }}</template><template v-if="e.by"> · <span class="ov-mut">{{ e.by }}</span></template></span>
          <span class="ov-mut ov-when">{{ formatDateTime(e.at) }}</span>
        </div>
        <button v-if="data.activity.length > SHOWN" type="button" class="ov-link ov-more" @click="all = !all">{{ tr(all ? 'Show less' : 'Show more') }}</button>
      </section>

      <div v-if="data.workspaces.length" class="ov-ws ov-wide">
        <span class="ov-mut">{{ tr('Workspaces') }}:</span>
        <button v-for="w in data.workspaces" :key="w.code" type="button" class="ov-wsbtn" @click="go(w.code)">
          <span class="fa" :class="w.code === 'factoring' ? 'fa-file-text-o' : 'fa-handshake-o'" aria-hidden="true"></span>{{ tr(w.label) }}
          <span class="fa fa-angle-right" aria-hidden="true"></span>
        </button>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { showError } from '@/shared/feedback/feedback'
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { crm360Api, type CrmCustomerOverview } from '../api/crm.api'

const props = defineProps<{ clientId: number; version: number }>()
const emit = defineEmits<{ (e: 'open-tab', tab: string): void }>()
const router = useRouter()

const SHOWN = 5
const data = ref<CrmCustomerOverview | null>(null)
const loading = ref(false)
const all = ref(false)
const shown = computed(() => (all.value ? data.value?.activity ?? [] : (data.value?.activity ?? []).slice(0, SHOWN)))

const tone = (code: string) => ({ ACTIVE: 'ok', ONBOARDING: 'warn', SUSPENDED: 'bad' } as Record<string, string>)[code] ?? 'none'

/** factoring opens the Factoring Customer Workspace; any other code is a Customer 360 tab. */
function go(code: string) {
  if (code === 'factoring')
    void router.push({ name: 'FactoringCustomerWorkspace', params: { clientId: props.clientId }, query: { from: 'customer' } })
  else emit('open-tab', code)
}

async function load() {
  loading.value = true
  try {
    data.value = await crm360Api.overview(props.clientId)
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  } finally {
    loading.value = false
  }
}

watch(() => [props.clientId, props.version], load, { immediate: true })
</script>

<style scoped>
.ov { display: grid; grid-template-columns: 1.25fr 1fr; gap: 14px; }
.ov-wide { grid-column: 1 / -1; }
.ov-box { padding: 12px 14px; border: 1px solid var(--fin-line); border-radius: 10px; background: #fff; }
.ov-box h3 { margin: 0 0 6px; font-size: 12px; letter-spacing: .5px; text-transform: uppercase; color: var(--fin-muted); }
.ov-row { display: flex; align-items: center; gap: 10px; padding: 8px 0; border-top: 1px solid #f0f2f4; font-size: 13.5px; }
.ov-box h3 + .ov-row { border-top: 0; }
.ov-grow { flex: 1; min-width: 0; }
.ov-mut { color: var(--fin-muted); font-size: 12px; }
.ov-when { flex: none; white-space: nowrap; }
.ov-dot { flex: none; width: 7px; height: 7px; border-radius: 50%; background: #9aa6b0; }
.ov-dot--ok { background: #2e9e6a; } .ov-dot--warn { background: #d39b2a; } .ov-dot--bad { background: #c0392b; }
.ov-tag { flex: none; padding: 2px 8px; border-radius: 999px; background: #f2f4f6; color: #34495a; font-size: 11px; font-weight: 600; }
.ov-link { flex: none; padding: 0; border: 0; background: none; color: var(--fin-teal-700); font: 600 12.5px/1 inherit; cursor: pointer; white-space: nowrap; }
.ov-link:hover { text-decoration: underline; }
.ov-more { margin-top: 6px; }
.ov-ws { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; }
.ov-wsbtn { display: inline-flex; align-items: center; gap: 8px; height: 34px; padding: 0 14px; border: 1px solid #d5dce2; border-radius: 8px; background: #fff;
  color: #34495a; font: 600 13px/1 inherit; cursor: pointer; }
.ov-wsbtn:hover { border-color: var(--fin-teal-600); background: var(--fin-teal-50); color: var(--fin-teal-700); }
@media (max-width: 900px) { .ov { grid-template-columns: 1fr; } .ov-row { flex-wrap: wrap; } }
</style>
