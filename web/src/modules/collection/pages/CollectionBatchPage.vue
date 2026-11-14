<template>
  <!-- View Collection Batch: the batch header with its GL Tran Set ID and the cheques it applied (COL_COLLECTION_BATCH_LINES),
       as View Batch does for the cheque wallet. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-file-text-o"></span></span>
        <div>
          <h1>{{ tr('Collection Batch') }}</h1>
          <p>{{ v ? v.batch.batchNo : tr('Loading...') }}</p>
        </div>
      </div>
      <RouterLink class="fx-btn" :to="{ name: 'collection-batches' }"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back') }}</RouterLink>
    </div>

    <div v-if="v" class="col-stack">
      <section class="col-sec">
        <h2><span class="fa fa-file-text-o" aria-hidden="true"></span>{{ tr('Collection Batch') }}</h2>
        <div class="col-grid">
          <div class="fc-field"><span>{{ tr('Batch No') }}</span><div class="col-ro">{{ v.batch.batchNo }}</div></div>
          <div class="fc-field"><span>{{ tr('Batch Date') }}</span><div class="col-ro">{{ formatDate(v.batch.batchDate) }}</div></div>
          <div class="fc-field"><span>{{ tr('GL Tran Set ID') }}</span><div class="col-ro">{{ v.batch.glTranSetId || tr('Not posted') }}</div></div>
          <div class="fc-field"><span>{{ tr('Collected') }}</span><div class="col-ro">{{ v.batch.collectedCount }} / {{ formatAmount(v.batch.collectedAmount) }}</div></div>
          <div class="fc-field"><span>{{ tr('Bounced') }}</span><div class="col-ro">{{ v.batch.bouncedCount }} / {{ formatAmount(v.batch.bouncedAmount) }}</div></div>
          <div class="fc-field"><span>{{ tr('Rejected') }}</span><div class="col-ro">{{ v.batch.rejectedCount }} / {{ formatAmount(v.batch.rejectedAmount) }}</div></div>
          <div class="fc-field"><span>{{ tr('Repayments') }}</span><div class="col-ro">{{ v.batch.repaymentsCount }}</div></div>
          <div class="fc-field"><span>{{ tr('Created By') }}</span><div class="col-ro">{{ v.batch.createdBy || '-' }}</div></div>
        </div>
      </section>

      <section class="fx-card">
        <div class="fc-card-head"><h2>{{ tr('Cheques') }} ({{ v.lines.length }})</h2></div>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead>
              <tr>
                <th>#</th>
                <th>{{ tr('Cheque Ref') }}</th>
                <th>{{ tr('Cheque No') }}</th>
                <th>{{ tr('Due Date') }}</th>
                <th>{{ tr('Client Name') }}</th>
                <th>{{ tr('Deposited Bank Account') }}</th>
                <th>{{ tr('GL Account') }}</th>
                <th class="fx-r">{{ tr('Cheque Amount') }}</th>
                <th>{{ tr('Bank Result') }}</th>
                <th>{{ tr('Status Date') }}</th>
                <th>{{ tr('Bank Statement Reference') }}</th>
                <th>{{ tr('Reason Code') }}</th>
                <th>{{ tr('Repayment Number') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="l in v.lines" :key="l.lineNo">
                <td>{{ l.lineNo }}</td>
                <td>{{ l.chequeRef || '-' }}</td>
                <td>{{ l.chequeNo }}</td>
                <td>{{ formatDate(l.dueDate) }}</td>
                <td>{{ l.clientName || l.customerId || '-' }}</td>
                <td>{{ [l.bankName, l.accountNo].filter(Boolean).join(' - ') || '-' }}</td>
                <td>{{ l.glAccount || '-' }}</td>
                <td class="fx-r">{{ formatAmount(l.chequeAmount) }} {{ l.currencyCode || '' }}</td>
                <td><span class="fx-chip" :class="statusChip(l.resultCode)">{{ l.resultCode }}</span></td>
                <td>{{ formatDate(l.statusDate) }}</td>
                <td>{{ l.bankStatementReference || '-' }}</td>
                <td>{{ l.reasonCode || '-' }}</td>
                <td>
                  <RouterLink v-if="l.repaymentTxnId" :to="{ name: 'collection-repayment', params: { repaymentTxnId: l.repaymentTxnId } }">{{ l.repaymentNumber }}</RouterLink>
                  <span v-else>-</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="!v.lines.length" class="fx-empty"><span class="fa fa-list" aria-hidden="true"></span>{{ tr('No data found') }}</div>
      </section>
    </div>
    <div v-else-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import './collection.css'
import { ref, watch } from 'vue'
import { tr } from '@/shared/layout/language'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import { collectionApi, type CollectionBatchView } from '../api/collection.api'
import { messageOf, statusChip } from './collection'

const props = defineProps<{ collectionBatchId: number }>()

const v = ref<CollectionBatchView | null>(null)
const loading = ref(false)

async function load() {
  loading.value = true
  try {
    v.value = await collectionApi.batch(props.collectionBatchId)
  } catch (err) {
    showError(messageOf(err))
  } finally {
    loading.value = false
  }
}

watch(() => props.collectionBatchId, load, { immediate: true })
</script>
