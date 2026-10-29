<template>
  <!-- APEX App 136 page 5 "Repayment Transactions": the Customer Repayment Transactions grid; the row link opens page 6,
       New Bank Transfer opens page 7. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-money"></span></span>
        <div>
          <h1>{{ tr('Repayment Transactions') }}</h1>
          <p>{{ tr('Customer Repayment Transactions') }}</p>
        </div>
      </div>
      <RouterLink v-if="canEdit" class="fx-btn fx-btn--teal" :to="{ name: 'collection-bank-transfer' }">
        <span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New Bank Transfer') }}
      </RouterLink>
    </div>

    <div class="fx-card">
      <div class="fc-card-head">
        <h2>{{ tr('Customer Repayment Transactions') }} ({{ filtered.length }})</h2>
        <input v-model="query" class="fx-search col-search" type="search" :placeholder="tr('Search...')" :aria-label="tr('Search')" @input="page = 1">
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th class="col-link"><span class="fx-sr">{{ tr('Open') }}</span></th>
              <th>{{ tr('Repayment Number') }}</th>
              <th>{{ tr('Client Name') }}</th>
              <th>{{ tr('Repayment Method') }}</th>
              <th>{{ tr('Transaction Date') }}</th>
              <th>{{ tr('Value Date') }}</th>
              <th class="fx-r">{{ tr('Receipt Amount') }}</th>
              <th class="fx-r">{{ tr('Allocated Amount') }}</th>
              <th>{{ tr('Status Code') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in paged" :key="r.repaymentTxnId">
              <td class="col-link">
                <RouterLink class="fx-btn fx-btn--sm" :to="{ name: 'collection-repayment', params: { repaymentTxnId: r.repaymentTxnId } }" :title="tr('Edit')" :aria-label="tr('Edit') + ' ' + r.repaymentNumber">
                  <span class="fa fa-pencil" aria-hidden="true"></span>
                </RouterLink>
              </td>
              <td>{{ r.repaymentNumber }}</td>
              <td>{{ r.clientName || '-' }}</td>
              <td>{{ r.repaymentMethod }}</td>
              <td>{{ formatDate(r.transactionDate) }}</td>
              <td>{{ formatDate(r.valueDate) }}</td>
              <td class="fx-r">{{ formatAmount(r.receiptAmount) }}</td>
              <td class="fx-r">{{ formatAmount(r.allocatedAmount) }}</td>
              <td><span class="fx-chip" :class="statusChip(r.statusCode)">{{ r.statusCode }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
      <div v-else-if="!paged.length" class="fx-empty"><span class="fa fa-money" aria-hidden="true"></span>{{ tr('No data found') }}</div>
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
import { hasPermission } from '@/shared/auth/session'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import { collectionApi, type RepaymentRow } from '../api/collection.api'
import { statusChip } from './collection'

const PAGE_SIZE = 15

const rows = ref<RepaymentRow[]>([])
const canEdit = hasPermission('COLLECTION.EDIT')
const loading = ref(false)
const error = ref('')
useFeedback({ errors: [error] })

// the grid's search box: any column containing the text
const query = ref('')
const page = ref(1)
const filtered = computed(() => {
  const q = query.value.trim().toLowerCase()
  if (!q) return rows.value
  return rows.value.filter((r) => [r.repaymentNumber, r.clientName, r.repaymentMethod, r.statusCode, r.transactionDate, r.valueDate,
    String(r.receiptAmount), String(r.allocatedAmount)].some((v) => (v ?? '').toLowerCase().includes(q)))
})
const pageCount = computed(() => Math.max(1, Math.ceil(filtered.value.length / PAGE_SIZE)))
const pages = computed(() => Array.from({ length: pageCount.value }, (_, i) => i + 1))
const paged = computed(() => filtered.value.slice((page.value - 1) * PAGE_SIZE, page.value * PAGE_SIZE))
const first = computed(() => (filtered.value.length ? (page.value - 1) * PAGE_SIZE + 1 : 0))
const last = computed(() => Math.min(page.value * PAGE_SIZE, filtered.value.length))

onMounted(async () => {
  loading.value = true
  try {
    rows.value = await collectionApi.repayments()
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
