<template>
  <!--
    Billing (Screening): per member company, its price plan, how much its current period comes to so far, and what it has been
    invoiced; then the invoices. A new invoice is previewed (users × months, inquiries) before it is issued; issued invoices never
    change, a wrong one is cancelled with a reason and issued again.
  -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Billing') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-money"></span></span>
          <div>
            <h1>{{ tr('Billing') }}</h1>
            <p>{{ tr('What each member company owes for its users and its inquiries, and its invoices. Amounts in EGP.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'scr-price-plans' }" class="fx-btn"><span class="fa fa-tags" aria-hidden="true"></span>{{ tr('Price plans') }}</RouterLink>
      </div>
    </div>

    <section class="scr-band" :aria-label="tr('Billing totals')">
      <div class="scr-band-head"><span class="scr-band-pulse" aria-hidden="true"></span><b>{{ tr('Billing totals') }}</b></div>
      <div class="scr-band-stats">
        <div class="scr-stat"><span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-file-text-o"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Invoiced') }}</em><b class="scr-billing-amount" dir="ltr">{{ egp(totalInvoiced) }}</b><small>{{ invoices.filter((i) => i.statusCode === 'ISSUED').length }} {{ tr('invoices') }}</small></span></div>
        <div class="scr-stat"><span class="scr-stat-ic scr-stat-ic--orange" aria-hidden="true"><span class="fa fa-hourglass-half"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Current periods so far') }}</em><b class="scr-billing-amount" dir="ltr">{{ egp(totalCurrent) }}</b><small>{{ tr('not invoiced yet') }}</small></span></div>
        <div class="scr-stat"><span class="scr-stat-ic" aria-hidden="true"><span class="fa fa-building-o"></span></span>
          <span class="scr-stat-copy"><em>{{ tr('Companies billed') }}</em><b>{{ companies.filter((c) => c.pricePlanId).length }} / {{ companies.length }}</b><small>{{ tr('with a price plan') }}</small></span></div>
      </div>
    </section>

    <div class="fx-card">
      <div class="fc-card-head"><h2>{{ tr('Member companies') }} ({{ companies.length }})</h2></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr><th>{{ tr('Company') }}</th><th>{{ tr('Price plan') }}</th><th>{{ tr('Invoicing') }}</th><th>{{ tr('Current period') }}</th>
              <th class="fx-r">{{ tr('So far') }}</th><th class="fx-r">{{ tr('Invoiced total') }}</th><th class="fx-r">{{ tr('Actions') }}</th></tr>
          </thead>
          <tbody>
            <tr v-for="c in companies" :key="c.memberCompanyId">
              <td><b dir="ltr">{{ c.companyCode }}</b><span class="fx-cell-sub">{{ c.nameAr }}</span></td>
              <td><template v-if="c.planCode"><b dir="ltr">{{ c.planCode }}</b><span class="fx-cell-sub">{{ planText(c.pricePlanId) }}</span></template>
                <span v-else class="fx-chip fx-chip--warn"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>{{ tr('No price plan') }}</span></td>
              <td>{{ tr(LABELS[c.billingCycleCode] ?? c.billingCycleCode) }}</td>
              <td><b dir="ltr">{{ c.currentPeriodCode }}</b><span class="fx-cell-sub">{{ c.currentUserMonths }} {{ tr('user-months') }} · {{ c.currentInquiries }} {{ tr('inquiries') }}</span></td>
              <td class="fx-r" dir="ltr"><b>{{ c.currentInvoiced ? tr('Invoiced') : egp(c.currentAmount) }}</b></td>
              <td class="fx-r" dir="ltr">{{ egp(c.invoicedTotal) }}<span class="fx-cell-sub">{{ c.invoiceCount }} {{ tr('invoices') }}</span></td>
              <td class="fx-r">
                <div class="fc-row-actions">
                  <button type="button" class="fx-btn fx-btn--sm" @click="filter.memberCompanyId = c.memberCompanyId; loadInvoices()"><span class="fa fa-list" aria-hidden="true"></span>{{ tr('Invoices') }}</button>
                  <button v-if="canManage" type="button" class="fx-btn fx-btn--sm fx-btn--teal" :disabled="!c.pricePlanId" :title="c.pricePlanId ? '' : tr('Choose the company\'s price plan first (Edit company › Billing).')" @click="startInvoice(c)">
                    <span class="fa fa-file-text-o" aria-hidden="true"></span>{{ tr('New invoice') }}</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loaded && !companies.length" class="fx-empty"><span class="fa fa-building-o" aria-hidden="true"></span>{{ tr('No member companies yet.') }}</div>
    </div>

    <div class="fx-card">
      <div class="fc-card-head"><h2>{{ tr('Invoices') }} ({{ invoices.length }})</h2>
        <div class="scr-billing-filters">
          <select v-model="filter.memberCompanyId" class="fx-select" @change="loadInvoices"><option :value="undefined">{{ tr('All companies') }}</option>
            <option v-for="c in companies" :key="c.memberCompanyId" :value="c.memberCompanyId">{{ c.companyCode }}</option></select>
          <select v-model="filter.statusCode" class="fx-select" @change="loadInvoices"><option value="">{{ tr('All') }}</option>
            <option value="ISSUED">{{ tr('Issued') }}</option><option value="CANCELLED">{{ tr('Cancelled') }}</option></select>
        </div>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr><th>{{ tr('Invoice') }}</th><th>{{ tr('Company') }}</th><th>{{ tr('Period') }}</th><th class="fx-r">{{ tr('Users') }}</th><th class="fx-r">{{ tr('Inquiries') }}</th>
              <th class="fx-r">{{ tr('Total') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Issued') }}</th><th class="fx-r">{{ tr('Actions') }}</th></tr>
          </thead>
          <tbody>
            <tr v-for="i in invoices" :key="i.invoiceId" :class="{ 'scr-row-cancelled': i.statusCode === 'CANCELLED' }">
              <td dir="ltr"><b>{{ i.invoiceNo }}</b></td>
              <td><b dir="ltr">{{ i.companyCode }}</b><span class="fx-cell-sub">{{ i.companyNameAr }}</span></td>
              <td><b dir="ltr">{{ i.periodCode }}</b><span class="fx-cell-sub" dir="ltr">{{ formatDate(i.periodFrom) }} – {{ formatDate(i.periodTo) }}</span></td>
              <td class="fx-r" dir="ltr">{{ i.userMonths }} × {{ egp(i.userMonthlyFee) }}</td>
              <td class="fx-r" dir="ltr">{{ i.inquiryCount }} × {{ egp(i.inquiryFee) }}</td>
              <td class="fx-r" dir="ltr"><b>{{ egp(i.totalAmount) }}</b></td>
              <td><span class="fx-chip" :class="i.statusCode === 'ISSUED' ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(LABELS[i.statusCode] ?? i.statusCode) }}</span></td>
              <td>{{ i.issuedBy }}<span class="fx-cell-sub">{{ i.issuedAt ? formatDateTime(i.issuedAt) : '' }}</span></td>
              <td class="fx-r"><RouterLink :to="{ name: 'scr-invoice', params: { invoiceId: i.invoiceId } }" class="fx-btn fx-btn--sm"><span class="fa fa-eye" aria-hidden="true"></span>{{ tr('View') }}</RouterLink></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loaded && !invoices.length" class="fx-empty"><span class="fa fa-file-text-o" aria-hidden="true"></span>{{ tr('No invoices yet.') }}</div>
    </div>

    <!-- new invoice: choose the period, see what it comes to, then issue -->
    <ScrDialog v-if="invoiceFor" :title="tr('New invoice')" :subtitle="`${invoiceFor.companyCode} · ${invoiceFor.nameAr}`" icon="fa-file-text-o" @close="invoiceFor = null">
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Period') }} ({{ tr(LABELS[invoiceFor.billingCycleCode] ?? '') }})</span>
          <select v-model="periodFrom" class="fx-select" @change="preview"><option v-for="p in periods" :key="p.from" :value="p.from">{{ p.code }}</option></select></label>
        <label class="fc-field fc-span-2"><span>{{ tr('Notes on the invoice') }}</span><input v-model="notes" class="fx-search"></label>
      </div>
      <div v-if="previewed" class="scr-invoice-preview">
        <div v-if="!previewed.periodComplete" class="fc-note fc-note--warn"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>
          <span>{{ tr('This period has not ended: users and inquiries after now will not be on this invoice. To bill them, cancel it after the period ends and issue it again.') }}</span></div>
        <ScrInvoiceLines :invoice="previewed" />
      </div>
      <div v-else-if="previewError" class="fc-note fc-note--warn"><span class="fa fa-exclamation-circle" aria-hidden="true"></span><span>{{ previewError }}</span></div>
      <template #actions>
        <button type="button" class="fx-btn" @click="invoiceFor = null">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="busy || !previewed" @click="issue">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-check']" aria-hidden="true"></span>{{ tr('Issue the invoice') }}</button>
      </template>
    </ScrDialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import ScrDialog from '../components/ScrDialog.vue'
import ScrInvoiceLines from '../components/ScrInvoiceLines.vue'
import { formatDate, formatDateTime } from '../utils/dates'
import { LABELS, egp, errorText, scrApi, type ScrBillingCompany, type ScrInvoice, type ScrPricePlan } from '../api/scr.api'

const router = useRouter()
const canManage = hasPermission('SCR.BILLING.MANAGE')
const companies = ref<ScrBillingCompany[]>([])
const invoices = ref<ScrInvoice[]>([])
const plans = ref<ScrPricePlan[]>([])
const loaded = ref(false)
const busy = ref(false)
const filter = reactive<{ memberCompanyId?: number; statusCode: string }>({ memberCompanyId: undefined, statusCode: '' })

const totalInvoiced = computed(() => invoices.value.filter((i) => i.statusCode === 'ISSUED').reduce((s, i) => s + i.totalAmount, 0))
const totalCurrent = computed(() => companies.value.filter((c) => !c.currentInvoiced).reduce((s, c) => s + (c.currentAmount ?? 0), 0))

const invoiceFor = ref<ScrBillingCompany | null>(null)
const periodFrom = ref('')
const notes = ref('')
const previewed = ref<ScrInvoice | null>(null)
const previewError = ref('')

function planText(id: number | null) {
  const p = plans.value.find((x) => x.pricePlanId === id)
  return p ? `${egp(p.userMonthlyFee)} / ${tr('user')} / ${tr('month')} · ${egp(p.inquiryFee)} / ${tr('inquiry')}` : ''
}

const iso = (d: Date) => `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
/** The cycle's calendar periods, the current one first: 12 months, 8 quarters or 3 years. */
const periods = computed(() => {
  const c = invoiceFor.value
  if (!c) return []
  const step = c.billingCycleCode === 'YEARLY' ? 12 : c.billingCycleCode === 'QUARTERLY' ? 3 : 1
  const count = c.billingCycleCode === 'YEARLY' ? 3 : c.billingCycleCode === 'QUARTERLY' ? 8 : 12
  const [y, m] = c.currentFrom.split('-').map(Number)
  return Array.from({ length: count }, (_, k) => {
    const d = new Date(y!, m! - 1 - k * step, 1)
    const code = step === 12 ? `${d.getFullYear()}` : step === 3 ? `${d.getFullYear()}-Q${Math.floor(d.getMonth() / 3) + 1}` : `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}`
    return { from: iso(d), code }
  })
})

async function load() {
  try {
    ;[companies.value, plans.value] = await Promise.all([scrApi.billingOverview(), scrApi.pricePlans()])
    await loadInvoices()
    loaded.value = true
  } catch (err) {
    showError(errorText(err))
  }
}

async function loadInvoices() {
  try {
    invoices.value = await scrApi.invoices({ memberCompanyId: filter.memberCompanyId, statusCode: filter.statusCode })
  } catch (err) {
    showError(errorText(err))
  }
}

async function startInvoice(c: ScrBillingCompany) {
  invoiceFor.value = c
  notes.value = ''
  // the last ended period by default: the one usually invoiced
  periodFrom.value = periods.value[1]?.from ?? periods.value[0]!.from
  await preview()
}

async function preview() {
  if (!invoiceFor.value) return
  previewed.value = null
  previewError.value = ''
  try {
    previewed.value = await scrApi.previewInvoice(invoiceFor.value.memberCompanyId, periodFrom.value)
  } catch (err) {
    previewError.value = errorText(err)
  }
}

async function issue() {
  if (!invoiceFor.value) return
  busy.value = true
  try {
    const i = await scrApi.issueInvoice(invoiceFor.value.memberCompanyId, periodFrom.value, notes.value.trim() || null)
    showSuccess(`${tr('Invoice issued')}: ${i.invoiceNo}`)
    invoiceFor.value = null
    await router.push({ name: 'scr-invoice', params: { invoiceId: i.invoiceId } })
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

onMounted(load)
</script>
