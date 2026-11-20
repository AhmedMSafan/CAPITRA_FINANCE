<template>
  <!-- One invoice, ready to print (or save as PDF from the print dialog). A cancelled invoice is kept, marked, with its reason. -->
  <div class="fx fc scr-invoice-page">
    <div class="fx-head scr-no-print">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'scr-billing' }">{{ tr('Billing') }}</RouterLink><span aria-hidden="true">›</span><b dir="ltr">{{ invoice?.invoiceNo }}</b>
        </nav>
      </div>
      <div class="fx-actions">
        <button v-if="invoice?.statusCode === 'ISSUED' && canManage" type="button" class="fx-btn" @click="cancelling = true"><span class="fa fa-ban" aria-hidden="true"></span>{{ tr('Cancel invoice') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" @click="print"><span class="fa fa-print" aria-hidden="true"></span>{{ tr('Print / PDF') }}</button>
      </div>
    </div>

    <article v-if="invoice" class="fx-card scr-inv-doc">
      <div v-if="invoice.statusCode === 'CANCELLED'" class="scr-inv-stamp">{{ tr('Cancelled') }}</div>
      <header class="scr-inv-head">
        <div>
          <div class="scr-inv-brand">CAPITRA</div>
          <small>{{ tr('Screening service') }}</small>
        </div>
        <div class="scr-inv-title">
          <h1>{{ tr('Invoice') }}</h1>
          <b dir="ltr">{{ invoice.invoiceNo }}</b>
        </div>
      </header>
      <section class="scr-inv-meta">
        <div><span>{{ tr('Bill to') }}</span><b>{{ invoice.companyNameAr }}</b><small>{{ invoice.companyNameEn }} · <bdi dir="ltr">{{ invoice.companyCode }}</bdi></small></div>
        <div><span>{{ tr('Period') }}</span><b dir="ltr">{{ invoice.periodCode }}</b><small>{{ formatDate(invoice.periodFrom) }} – {{ formatDate(invoice.periodTo) }}</small></div>
        <div><span>{{ tr('Price plan') }}</span><b dir="ltr">{{ invoice.planCode }}</b><small>{{ tr('Currency') }}: {{ invoice.currencyCode }}</small></div>
        <div><span>{{ tr('Issued') }}</span><b>{{ invoice.issuedAt ? formatDateTime(invoice.issuedAt) : '-' }}</b><small>{{ invoice.issuedBy }}</small></div>
      </section>
      <div v-if="invoice.statusCode === 'CANCELLED'" class="fc-note fc-note--warn"><span class="fa fa-ban" aria-hidden="true"></span>
        <span>{{ tr('Cancelled by') }} {{ invoice.cancelledBy }} · {{ invoice.cancelledAt ? formatDateTime(invoice.cancelledAt) : '' }}: {{ invoice.cancelReason }}</span></div>
      <ScrInvoiceLines :invoice="invoice" />
      <p v-if="invoice.notes" class="scr-inv-notes"><b>{{ tr('Notes') }}:</b> {{ invoice.notes }}</p>
      <footer class="scr-inv-foot">{{ tr('User fee: per user per month, a user active on any day of a month pays the month. Inquiry fee: per inquiry, whatever its result.') }}</footer>
    </article>

    <ScrDialog v-if="cancelling" :title="tr('Cancel invoice')" :subtitle="invoice?.invoiceNo ?? ''" icon="fa-ban" @close="cancelling = false">
      <p class="scr-confirm">{{ tr('The invoice is kept, marked cancelled with the reason, and its period can be invoiced again.') }}</p>
      <label class="fc-field"><span>{{ tr('Reason') }} <b>*</b></span><textarea v-model="reason" class="fx-search fc-textarea"></textarea></label>
      <template #actions>
        <button type="button" class="fx-btn" @click="cancelling = false">{{ tr('Close') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="busy || !reason.trim()" @click="cancel">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-ban']" aria-hidden="true"></span>{{ tr('Cancel invoice') }}</button>
      </template>
    </ScrDialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { onMounted, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import ScrDialog from '../components/ScrDialog.vue'
import ScrInvoiceLines from '../components/ScrInvoiceLines.vue'
import { formatDate, formatDateTime } from '../utils/dates'
import { errorText, scrApi, type ScrInvoice } from '../api/scr.api'

const props = defineProps<{ invoiceId: number }>()
const canManage = hasPermission('SCR.BILLING.MANAGE')
const invoice = ref<ScrInvoice | null>(null)
const cancelling = ref(false)
const reason = ref('')
const busy = ref(false)

async function load() {
  try {
    invoice.value = await scrApi.invoice(props.invoiceId)
  } catch (err) {
    showError(errorText(err))
  }
}

function print() {
  window.print()
}

async function cancel() {
  busy.value = true
  try {
    invoice.value = await scrApi.cancelInvoice(props.invoiceId, reason.value.trim())
    cancelling.value = false
    showSuccess(tr('The invoice is cancelled.'))
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

onMounted(load)
</script>
