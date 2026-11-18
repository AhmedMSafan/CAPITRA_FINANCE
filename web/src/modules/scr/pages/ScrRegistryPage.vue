<template>
  <!-- The Federation Final List: every entry of every published listing, as its later decisions left it. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Final List') }}</b>
        </nav>
        <h1>{{ tr('Federation Final List') }} <small v-if="registry" class="scr-muted">V{{ registry.versionNo }}</small></h1>
      </div>
    </div>
    <section class="fx-card">
      <div class="scr-filter">
        <input v-model="search" class="fx-search" :placeholder="tr('Search by name, identity number or decision')">
        <select v-model="status" class="fx-select">
          <option value="">{{ tr('All statuses') }}</option>
          <option v-for="s in ['ACTIVE', 'SUSPENDED', 'LIFTED']" :key="s" :value="s">{{ tr(LABELS[s] ?? s) }}</option>
        </select>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr><th>{{ tr('Name') }}</th><th>{{ tr('Identity number') }}</th><th>{{ tr('Nationality') }}</th><th>{{ tr('Decision') }}</th><th>{{ tr('Scope') }}</th><th>{{ tr('Effective date') }}</th><th>{{ tr('Status') }}</th></tr>
          </thead>
          <tbody>
            <tr v-for="e in rows" :key="`${e.partyId}-${e.decisionId}`" class="scr-row" @click="router.push({ name: 'scr-decision', params: { decisionId: e.decisionId } })">
              <td><b>{{ e.nameAr }}</b><br><small class="scr-muted">{{ e.nameEn }}</small><span v-for="a in e.aliases" :key="a" class="scr-tag">{{ a }}</span></td>
              <td dir="ltr">{{ e.identityNo }} <small class="scr-muted">{{ tr(LABELS[e.identityTypeCode] ?? e.identityTypeCode) }}</small></td>
              <td>{{ e.nationalityCode }}</td>
              <td dir="ltr">{{ e.decisionRef }} · {{ e.officialNo }}</td>
              <td>{{ e.scopeText }}</td>
              <td>{{ formatDate(e.listedFrom) }}<template v-if="e.liftedOn"> → {{ formatDate(e.liftedOn) }}</template></td>
              <td><span class="fx-chip" :class="CHIP[e.statusCode]">{{ tr(LABELS[e.statusCode] ?? e.statusCode) }}</span></td>
            </tr>
          </tbody>
        </table>
        <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
        <div v-else-if="!rows.length" class="fx-empty">{{ tr('No entries.') }}</div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { CHIP, LABELS, errorText, scrApi, type ScrRegistry } from '../api/scr.api'

const router = useRouter()
const registry = ref<ScrRegistry | null>(null)
const loading = ref(false)
const search = ref('')
const status = ref('')
const rows = computed(() => {
  const q = search.value.trim().toLowerCase()
  return (registry.value?.entries ?? []).filter((e) => (!status.value || e.statusCode === status.value)
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
