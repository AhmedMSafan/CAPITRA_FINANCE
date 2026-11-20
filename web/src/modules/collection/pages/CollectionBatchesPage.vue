<template>
  <!-- Collection Batches: one row per Apply Bank Results run (COL_COLLECTION_BATCHES), with its counts, amounts and the one
       GL transaction set of the batch. The row link opens the batch's cheques. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-list"></span></span>
        <div>
          <h1>{{ tr('Collection Batches') }}</h1>
          <p>{{ tr('Each Apply Bank Results run, its cheques and its GL Tran Set ID.') }}</p>
        </div>
      </div>
      <RouterLink class="fx-btn" :to="{ name: 'collection-cheques' }"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Cheques Collection') }}</RouterLink>
    </div>

    <div class="fx-card">
      <div class="fc-card-head">
        <h2>{{ tr('Collection Batches') }} ({{ filtered.length }})</h2>
        <input v-model="query" class="fx-search col-search" type="search" :placeholder="tr('Search...')" :aria-label="tr('Search')" @input="page = 1">
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th class="col-link"><span class="fx-sr">{{ tr('Open') }}</span></th>
              <th>{{ tr('Batch No') }}</th>
              <th>{{ tr('Batch Date') }}</th>
              <th class="fx-r">{{ tr('Collected') }}</th>
              <th class="fx-r">{{ tr('Collected Amount') }}</th>
              <th class="fx-r">{{ tr('Bounced') }}</th>
              <th class="fx-r">{{ tr('Bounced Amount') }}</th>
              <th class="fx-r">{{ tr('Rejected') }}</th>
              <th class="fx-r">{{ tr('Rejected Amount') }}</th>
              <th class="fx-r">{{ tr('Repayments') }}</th>
              <th>{{ tr('GL Tran Set ID') }}</th>
              <th>{{ tr('Created By') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="b in paged" :key="b.collectionBatchId">
              <td class="col-link">
                <RouterLink class="fx-btn fx-btn--sm" :to="{ name: 'collection-batch', params: { collectionBatchId: b.collectionBatchId } }" :title="tr('View')" :aria-label="tr('View') + ' ' + b.batchNo">
                  <span class="fa fa-eye" aria-hidden="true"></span>
                </RouterLink>
              </td>
              <td>{{ b.batchNo }}</td>
              <td>{{ formatDate(b.batchDate) }}</td>
              <td class="fx-r">{{ b.collectedCount }}</td>
              <td class="fx-r">{{ formatAmount(b.collectedAmount) }}</td>
              <td class="fx-r">{{ b.bouncedCount }}</td>
              <td class="fx-r">{{ formatAmount(b.bouncedAmount) }}</td>
              <td class="fx-r">{{ b.rejectedCount }}</td>
              <td class="fx-r">{{ formatAmount(b.rejectedAmount) }}</td>
              <td class="fx-r">{{ b.repaymentsCount }}</td>
              <td>{{ b.glTranSetId || '-' }}</td>
              <td>{{ b.createdBy || '-' }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
      <div v-else-if="!paged.length" class="fx-empty"><span class="fa fa-list" aria-hidden="true"></span>{{ tr('No data found') }}</div>
      <div class="fc-foot">
        <span class="fc-muted">{{ tr('Showing') }} {{ first }} {{ tr('to') }} {{ last }} {{ tr('of') }} {{ filtered.length }} {{ tr('entries') }}</span>
        <div class="fc-pager">
          <button type="button" class="fx-btn fx-btn--sm" :disabled="page === 1" :aria-label="tr('Previous')" @click="page--"><span class="fa fa-chevron-left" aria-hidden="true"></span></button>
          <button v-for="p in pages" :key="p" type="button" class="fx-btn fx-btn--sm" :class="{ 'fx-btn--teal': p === page }" @click="page = p">{{ p }}</button>
          <button type="button" class="fx-btn fx-btn--sm" :disabled="page === pageCount" :aria-label="tr('Next')" @click="page++"><span class="fa fa-chevron-right" aria-hidden="true"></span></button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, ref } from 'vue'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { tr } from '@/shared/layout/language'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import { collectionApi, type CollectionBatch } from '../api/collection.api'

const PAGE_SIZE = 15

const rows = ref<CollectionBatch[]>([])
const loading = ref(false)
const error = ref('')
useFeedback({ errors: [error] })

const query = ref('')
const page = ref(1)
const filtered = computed(() => {
  const q = query.value.trim().toLowerCase()
  if (!q) return rows.value
  return rows.value.filter((b) => [b.batchNo, b.batchDate, b.glTranSetId, b.createdBy, String(b.collectedAmount)]
    .some((v) => (v ?? '').toLowerCase().includes(q)))
})
const pageCount = computed(() => Math.max(1, Math.ceil(filtered.value.length / PAGE_SIZE)))
const pages = computed(() => Array.from({ length: pageCount.value }, (_, i) => i + 1))
const paged = computed(() => filtered.value.slice((page.value - 1) * PAGE_SIZE, page.value * PAGE_SIZE))
const first = computed(() => (filtered.value.length ? (page.value - 1) * PAGE_SIZE + 1 : 0))
const last = computed(() => Math.min(page.value * PAGE_SIZE, filtered.value.length))

onMounted(async () => {
  loading.value = true
  try {
    rows.value = await collectionApi.batches()
  } catch (e) {
    error.value = e instanceof Error ? e.message : String(e)
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.col-search { width: min(260px, 100%); }
.col-link { width: 44px; }
.fx-sr { position: absolute; width: 1px; height: 1px; overflow: hidden; clip: rect(0 0 0 0); }
</style>
