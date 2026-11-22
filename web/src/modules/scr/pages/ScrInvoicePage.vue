<template>
  <!-- One invoice, ready to print (or save as PDF from the print dialog). A cancelled invoice is kept, marked, with its reason. -->
  <div class="fx fc scr-invoice-page">
    <div class="fx-head scr-no-print">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <template v-if="portal"><RouterLink :to="{ name: 'scr-portal', query: { tab: 'invoices' } }">{{ tr('Screening Portal') }}</RouterLink><span aria-hidden="true">›</span></template>
          <template v-else><RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'scr-billing' }">{{ tr('Billing') }}</RouterLink><span aria-hidden="true">›</span></template><b dir="ltr">{{ invoice?.invoiceNo }}</b>
        </nav>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openScreeningManual(9)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
        <button v-if="invoice?.statusCode === 'ISSUED' && canManage && !(invoice.paidAmount ?? 0)" type="button" class="fx-btn" @click="cancelling = true"><span class="fa fa-ban" aria-hidden="true"></span>{{ tr('Cancel invoice') }}</button>
        <button v-if="invoice?.statusCode === 'ISSUED' && canManage && (invoice.balanceAmount ?? 0) > 0" type="button" class="fx-btn fx-btn--teal" @click="startPayment">
          <span class="fa fa-money" aria-hidden="true"></span>{{ tr('Record payment') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" @click="print"><span class="fa fa-print" aria-hidden="true"></span>{{ tr('Print / PDF') }}</button>
      </div>
    </div>

    <article v-if="invoice" class="fx-card scr-inv-doc">
      <header class="scr-inv-head">
        <div>
          <div class="scr-inv-brand">CAPITRA</div>
          <small>{{ tr('Screening service') }}</small>
        </div>
        <div class="scr-inv-title">
          <h1>{{ tr('Invoice') }}<span v-if="invoice.statusCode === 'CANCELLED'" class="scr-inv-stamp">{{ tr('Cancelled') }}</span></h1>
          <b dir="ltr">{{ invoice.invoiceNo }}</b>
        </div>
      </header>
      <section class="scr-inv-meta">
        <div><span>{{ tr('Bill to') }}</span><b>{{ invoice.companyNameAr }}</b><small>{{ invoice.companyNameEn }} · <bdi dir="ltr">{{ invoice.companyCode }}</bdi></small></div>
        <div><span>{{ tr('Period') }}</span><b dir="ltr">{{ invoice.periodCode }}</b><small dir="ltr">{{ formatDate(invoice.periodFrom) }} – {{ formatDate(invoice.periodTo) }}</small></div>
        <div><span>{{ tr('Price plan') }}</span><b dir="ltr">{{ invoice.planCode }}</b><small>{{ tr('Currency') }}: {{ invoice.currencyCode }}</small></div>
        <div><span>{{ tr('Issued') }}</span><b>{{ invoice.issuedAt ? formatDateTime(invoice.issuedAt) : '-' }}</b><small>{{ invoice.issuedBy }}</small></div>
      </section>
      <div v-if="invoice.statusCode === 'CANCELLED'" class="fc-note fc-note--warn"><span class="fa fa-ban" aria-hidden="true"></span>
        <span>{{ tr('Cancelled by') }} {{ invoice.cancelledBy }} · {{ invoice.cancelledAt ? formatDateTime(invoice.cancelledAt) : '' }}: {{ invoice.cancelReason }}</span></div>
      <ScrInvoiceLines :invoice="invoice" />
      <p v-if="invoice.notes" class="scr-inv-notes"><b>{{ tr('Notes') }}:</b> {{ invoice.notes }}</p>
      <section class="scr-inv-pay">
        <div class="scr-inv-pay-sum">
          <div><span>{{ tr('Total') }}</span><b dir="ltr">{{ egp(invoice.totalAmount) }}</b></div>
          <div><span>{{ tr('Paid') }}</span><b dir="ltr">{{ egp(invoice.paidAmount ?? 0) }}</b></div>
          <div><span>{{ tr('Balance due') }}</span><b dir="ltr" :class="{ 'scr-inv-due': (invoice.balanceAmount ?? 0) > 0 }">{{ egp(invoice.balanceAmount ?? 0) }}</b></div>
          <div v-if="invoice.statusCode === 'ISSUED'"><span>{{ tr('Payment') }}</span>
            <b><span class="fx-chip" :class="PAY_CHIP[invoice.paymentStatusCode ?? 'UNPAID']">{{ tr(LABELS[invoice.paymentStatusCode ?? 'UNPAID'] ?? '') }}</span></b></div>
        </div>
        <div v-if="invoice.payments?.length" class="fx-table-wrap scr-inv-pay-table">
          <table class="fx-table">
            <thead><tr><th>{{ tr('Paid on') }}</th><th class="fx-r">{{ tr('Amount') }}</th><th>{{ tr('Into') }}</th><th>{{ tr('Reference') }}</th><template v-if="!portal"><th class="scr-no-print">{{ tr('GL') }}</th><th class="scr-no-print">{{ tr('By') }}</th></template></tr></thead>
            <tbody>
              <tr v-for="p in invoice.payments" :key="p.paymentId">
                <td>{{ formatDate(p.paymentDate) }}</td>
                <td class="fx-r" dir="ltr"><b>{{ egp(p.paymentAmount) }}</b></td>
                <td dir="ltr">{{ p.bankAccountText }}</td>
                <td dir="ltr">{{ p.referenceNo ?? '-' }}<span v-if="p.notes" class="fx-cell-sub" dir="auto">{{ p.notes }}</span></td>
                <template v-if="!portal"><td class="scr-no-print" dir="ltr"><small>{{ p.glTranSetId ?? '-' }}</small></td>
                <td class="scr-no-print" dir="ltr">{{ p.createdBy }}<span class="fx-cell-sub">{{ formatDateTime(p.createdAt) }}</span></td></template>
              </tr>
            </tbody>
          </table>
        </div>
        <p v-if="!portal" class="scr-inv-gl scr-no-print"><span class="fa fa-book" aria-hidden="true"></span>
          <template v-if="invoice.glTranSetId">{{ tr('GL entry') }} <b dir="ltr">{{ invoice.glTranSetId }}</b> {{ tr('dated') }} {{ formatDate(invoice.accountingDate ?? null) }}<template v-if="invoice.cancelGlTranSetId"> · {{ tr('reversed by') }} <b dir="ltr">{{ invoice.cancelGlTranSetId }}</b></template></template>
          <template v-else>{{ tr('No GL entry: the invoice comes to 0.00.') }}</template>
        </p>
      </section>
      <footer class="scr-inv-foot">{{ tr('User fee: per user per month, a user active on any day of a month pays the month. Inquiries: every inquiry, whatever its result, priced band by band from the start of the period.') }}</footer>
    </article>

    <ScrDialog v-if="paying && invoice" :title="tr('Record payment')" :subtitle="`${invoice.invoiceNo} · ${tr('Balance due')} ${egp(invoice.balanceAmount ?? 0)}`" icon="fa-money" @close="paying = false">
      <div class="fc-grid">
        <label class="fc-field"><span>{{ tr('Paid on') }} <b>*</b></span><input v-model="pay.paymentDate" class="fx-search" type="date" :max="today"></label>
        <label class="fc-field"><span>{{ tr('Amount (EGP)') }} <b>*</b></span><input v-model.number="pay.paymentAmount" class="fx-search" type="number" min="0.01" step="0.01" dir="ltr"></label>
        <label class="fc-field fc-span-2"><span>{{ tr('Into our bank account') }} <b>*</b></span>
          <select v-model="pay.bankAccountId" class="fx-select"><option :value="null">{{ tr('Choose...') }}</option>
            <option v-for="b in banks" :key="b.bankAccountId" :value="b.bankAccountId">{{ b.text }} (GL {{ b.glAccountCode }})</option></select></label>
        <label class="fc-field"><span>{{ tr('Bank reference') }}</span><input v-model="pay.referenceNo" class="fx-search" dir="ltr"></label>
        <label class="fc-field"><span>{{ tr('Notes') }}</span><input v-model="pay.notes" class="fx-search"></label>
      </div>
      <p class="scr-confirm">{{ tr('Posts DR the bank account / CR screening fees receivable on the payment date. A part payment leaves the rest due.') }}</p>
      <template #actions>
        <button type="button" class="fx-btn" @click="paying = false">{{ tr('Close') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !pay.paymentDate || !pay.paymentAmount || !pay.bankAccountId" @click="recordPayment">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-check']" aria-hidden="true"></span>{{ tr('Record payment') }}</button>
      </template>
    </ScrDialog>

    <ScrDialog v-if="cancelling" :title="tr('Cancel invoice')" :subtitle="invoice?.invoiceNo ?? ''" icon="fa-ban" @close="cancelling = false">
      <p class="scr-confirm">{{ tr('The invoice is kept, marked cancelled with the reason, and its period can be invoiced again. Its GL entry is reversed today.') }}</p>
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
import { onMounted, reactive, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import ScrDialog from '../components/ScrDialog.vue'
import ScrInvoiceLines from '../components/ScrInvoiceLines.vue'
import { formatDate, formatDateTime } from '../utils/dates'
import { LABELS, egp, errorText, scrApi, type ScrBankAccount, type ScrInvoice } from '../api/scr.api'
import { openScreeningManual } from '@/modules/scr/utils/manual'

/** portal: a member company administrator viewing the company's own invoice (read and print only). */
const props = defineProps<{ invoiceId: number; portal?: boolean }>()
const canManage = !props.portal && hasPermission('SCR.BILLING.MANAGE')
const invoice = ref<ScrInvoice | null>(null)
const cancelling = ref(false)
const reason = ref('')
const busy = ref(false)
const paying = ref(false)
const banks = ref<ScrBankAccount[]>([])
const today = new Intl.DateTimeFormat('en-CA', { timeZone: 'Africa/Cairo' }).format(new Date())
const pay = reactive({ paymentDate: today, paymentAmount: 0, bankAccountId: null as number | null, referenceNo: '', notes: '' })
const PAY_CHIP: Record<string, string> = { UNPAID: 'fx-chip--warn', PARTLY_PAID: 'fx-chip--warn', PAID: 'fx-chip--ok' }

async function startPayment() {
  Object.assign(pay, { paymentDate: today, paymentAmount: invoice.value?.balanceAmount ?? 0, bankAccountId: null, referenceNo: '', notes: '' })
  paying.value = true
  try {
    banks.value = await scrApi.bankAccounts()
    if (banks.value.length === 1) pay.bankAccountId = banks.value[0]!.bankAccountId
  } catch (err) {
    showError(errorText(err))
  }
}

async function recordPayment() {
  busy.value = true
  try {
    invoice.value = await scrApi.recordPayment(props.invoiceId, {
      paymentDate: pay.paymentDate, paymentAmount: pay.paymentAmount, bankAccountId: pay.bankAccountId,
      referenceNo: pay.referenceNo.trim() || null, notes: pay.notes.trim() || null
    })
    paying.value = false
    showSuccess(tr('Payment recorded.'))
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function load() {
  try {
    invoice.value = props.portal ? await scrApi.portalInvoice(props.invoiceId) : await scrApi.invoice(props.invoiceId)
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
