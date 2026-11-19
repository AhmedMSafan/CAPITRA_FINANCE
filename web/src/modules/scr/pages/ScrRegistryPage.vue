<template>
  <!-- The Federation Final List: every entry of every published listing, as its later decisions left it. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Final List') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-list-alt"></span></span>
          <div>
            <h1>{{ tr('Federation Final List') }} <span v-if="registry" class="fx-chip fx-chip--teal fx-chip--plain">V{{ registry.versionNo }}</span></h1>
            <p>{{ registry?.publishedAt ? `${tr('Last published')} ${formatDateTime(registry.publishedAt)}` : tr('Every listed person and company, active, suspended and lifted.') }}</p>
          </div>
        </div>
      </div>
    </div>

    <div class="fx-card fx-card-pad fc-filters">
      <label class="fc-field"><span>{{ tr('Search') }}</span>
        <input v-model="search" class="fx-search" :placeholder="tr('Search by name, identity number or decision')">
      </label>
      <label class="fc-field"><span>{{ tr('Type') }}</span>
        <select v-model="type" class="fx-select">
          <option value="">{{ tr('All') }}</option>
          <option v-for="t in ['PERSON', 'COMPANY']" :key="t" :value="t">{{ tr(LABELS[t] ?? t) }}</option>
        </select>
      </label>
      <label class="fc-field"><span>{{ tr('Status') }}</span>
        <select v-model="status" class="fx-select">
          <option value="">{{ tr('All statuses') }}</option>
          <option v-for="s in ['ACTIVE', 'SUSPENDED', 'LIFTED']" :key="s" :value="s">{{ tr(LABELS[s] ?? s) }}</option>
        </select>
      </label>
    </div>

    <div class="fx-card">
      <div class="fc-card-head">
        <h2>{{ tr('Entries') }} ({{ rows.length }})</h2>
        <span v-if="registry" class="fc-muted">{{ registry.activeCount }} {{ tr('active') }} · {{ registry.suspendedCount }} {{ tr('suspended') }} · {{ registry.liftedCount }} {{ tr('lifted') }}</span>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th class="fc-no">#</th><th>{{ tr('Name') }}</th><th>{{ tr('Identity number') }}</th><th>{{ tr('Nationality') }}</th>
              <th>{{ tr('Decision') }}</th><th>{{ tr('Scope') }}</th><th>{{ tr('Effective date') }}</th><th>{{ tr('Status') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(e, i) in rows" :key="`${e.partyId}-${e.decisionId}`">
              <td class="fc-no">{{ i + 1 }}</td>
              <td>
                <b>{{ e.nameAr }}</b><span class="fx-cell-sub">{{ e.nameEn }} · {{ tr(LABELS[e.partyTypeCode] ?? e.partyTypeCode) }}</span>
                <span v-for="a in e.aliases" :key="a" class="scr-tag">{{ a }}</span>
              </td>
              <td dir="ltr">{{ e.identityNo }}<span class="fx-cell-sub">{{ tr(LABELS[e.identityTypeCode] ?? e.identityTypeCode) }}</span></td>
              <td>{{ e.nationalityCode }}</td>
              <td><RouterLink :to="{ name: 'scr-decision', params: { decisionId: e.decisionId } }" dir="ltr">{{ e.decisionRef }}</RouterLink><span class="fx-cell-sub" dir="ltr">{{ e.officialNo }}</span></td>
              <td>{{ e.scopeText }}</td>
              <td>{{ formatDate(e.listedFrom) }}<span v-if="e.liftedOn" class="fx-cell-sub">{{ tr('Lifted') }} {{ formatDate(e.liftedOn) }}</span></td>
              <td><span class="fx-chip" :class="CHIP[e.statusCode]">{{ tr(LABELS[e.statusCode] ?? e.statusCode) }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
      <div v-else-if="!rows.length" class="fx-empty"><span class="fa fa-list-alt" aria-hidden="true"></span>{{ tr('No entries.') }}</div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, ref } from 'vue'
import { showError } from '@/shared/feedback/feedback'
import { formatDate, formatDateTime } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import { CHIP, LABELS, errorText, scrApi, type ScrRegistry } from '../api/scr.api'

const registry = ref<ScrRegistry | null>(null)
const loading = ref(false)
const search = ref('')
const type = ref('')
const status = ref('')
const rows = computed(() => {
  const q = search.value.trim().toLowerCase()
  return (registry.value?.entries ?? []).filter((e) => (!status.value || e.statusCode === status.value) && (!type.value || e.partyTypeCode === type.value)
    && (!q || `${e.nameAr} ${e.nameEn ?? ''} ${e.aliases.join(' ')} ${e.identityNo} ${e.decisionRef} ${e.officialNo}`.toLowerCase().includes(q)))
})

onMounted(async () => {
  loading.value = true
  try {
    registry.value = await scrApi.registry()
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
})
</script>
