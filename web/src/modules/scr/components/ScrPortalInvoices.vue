<template>
  <!-- A member company administrator's own invoices: what was billed, paid and is still due. Each opens printable. -->
  <section class="fx-card">
    <div class="fc-card-head"><h2>{{ tr('Our invoices') }} ({{ list.length }})</h2>
      <span class="fc-muted">{{ tr('Balance due') }}: <b dir="ltr">{{ egp(due) }}</b></span></div>
    <div class="fx-table-wrap">
      <table class="fx-table">
        <thead><tr><th>{{ tr('Invoice') }}</th><th>{{ tr('Period') }}</th><th class="fx-r">{{ tr('Total') }}</th><th class="fx-r">{{ tr('Paid') }}</th><th class="fx-r">{{ tr('Balance due') }}</th><th>{{ tr('Status') }}</th><th></th></tr></thead>
        <tbody>
          <tr v-for="i in list" :key="i.invoiceId">
            <td><b dir="ltr">{{ i.invoiceNo }}</b><span class="fx-cell-sub">{{ i.issuedAt ? formatDate(i.issuedAt) : '' }}</span></td>
            <td dir="ltr">{{ i.periodCode }}<span class="fx-cell-sub">{{ formatDate(i.periodFrom) }} – {{ formatDate(i.periodTo) }}</span></td>
            <td class="fx-r" dir="ltr">{{ egp(i.totalAmount) }}</td>
            <td class="fx-r" dir="ltr">{{ egp(i.paidAmount ?? 0) }}</td>
            <td class="fx-r" dir="ltr"><b>{{ egp(i.balanceAmount ?? 0) }}</b></td>
            <td><span class="fx-chip" :class="i.statusCode === 'CANCELLED' ? 'fx-chip--bad' : PAY_CHIP[i.paymentStatusCode ?? 'UNPAID']">
              {{ tr(i.statusCode === 'CANCELLED' ? 'Cancelled' : LABELS[i.paymentStatusCode ?? 'UNPAID'] ?? '') }}</span></td>
            <td class="fx-r"><RouterLink :to="{ name: 'scr-portal-invoice', params: { invoiceId: i.invoiceId } }" class="fx-btn fx-btn--sm">
              <span class="fa fa-file-text-o" aria-hidden="true"></span>{{ tr('Open') }}</RouterLink></td>
          </tr>
        </tbody>
      </table>
    </div>
    <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
    <div v-else-if="!list.length" class="fx-empty"><span class="fa fa-file-text-o" aria-hidden="true"></span>{{ tr('No invoices yet.') }}</div>
  </section>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDate } from '../utils/dates'
import { LABELS, egp, errorText, scrApi, type ScrInvoice } from '../api/scr.api'

const PAY_CHIP: Record<string, string> = { UNPAID: 'fx-chip--warn', PARTLY_PAID: 'fx-chip--warn', PAID: 'fx-chip--ok' }
const list = ref<ScrInvoice[]>([])
const loading = ref(false)
const due = computed(() => list.value.filter((i) => i.statusCode === 'ISSUED').reduce((s, i) => s + (i.balanceAmount ?? 0), 0))

onMounted(async () => {
  loading.value = true
  try {
    list.value = await scrApi.portalInvoices()
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
})
</script>
