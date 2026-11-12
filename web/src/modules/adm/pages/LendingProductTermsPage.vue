<template>
  <!-- Administration > Product Setup > Lending Product Terms. The lending products are the Product Catalog products of business line
       Microfinance or Consumer Finance. Their terms are versions: a draft is prepared, sent for approval and approved by another user;
       an approved version is used from its start date and never changes. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'adm-home' }">{{ tr('Administration') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ tr('Lending Product Terms') }}</b>
        </nav>
        <h1>{{ tr('Lending Product Terms') }}</h1>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'adm-products' }" class="fx-btn"><span class="fa fa-cubes" aria-hidden="true"></span>{{ tr('Product Catalog') }}</RouterLink>
        <RouterLink :to="{ name: 'adm-home' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Administration') }}</RouterLink>
      </div>
    </div>

    <div v-if="loading && !data" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading products...') }}</div>

    <section v-else-if="data && !data.products.length" class="fx-card lt-none">
      <span class="fa fa-cube" aria-hidden="true"></span>
      <b>{{ tr('No lending products yet.') }}</b>
      <p>{{ tr('Create a product in the Product Catalog with business line Microfinance or Consumer Finance; its terms are then set here.') }}</p>
      <RouterLink :to="{ name: 'adm-products' }" class="fx-btn fx-btn--teal"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Open Product Catalog') }}</RouterLink>
    </section>

    <div v-else-if="data" class="lt-layout">
      <aside class="fx-card lt-products">
        <button v-for="p in data.products" :key="p.productId" type="button" class="lt-product" :class="{ 'lt-product--on': p.productId === productId }" @click="selectProduct(p.productId)">
          <b>{{ p.productNameEn }}</b>
          <small>{{ p.productCode }} · {{ tr(LINES[p.businessLineCode] ?? p.businessLineCode) }}</small>
          <span class="fx-chip" :class="p.versionInForce ? 'fx-chip--ok' : 'fx-chip--warn'">
            {{ p.versionInForce ? `${tr('Version')} ${p.versionInForce} ${tr('in force')}` : tr('No terms in force') }}
          </span>
        </button>
      </aside>

      <section v-if="product" class="fx-card lt-main">
        <header class="lt-main-head">
          <div>
            <h2>{{ product.productNameEn }}</h2>
            <small class="c3-muted">{{ product.productCode }} · {{ tr(LINES[product.businessLineCode] ?? '') }} · {{ tr('Type') }} {{ product.productTypeCode }}<template v-if="!product.isActive"> · {{ tr('Inactive') }}</template></small>
          </div>
          <button v-if="data.canEdit && !openVersion" type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="createDraft">
            <span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New version') }}
          </button>
        </header>

        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead>
              <tr><th>{{ tr('Version') }}</th><th>{{ tr('From') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Amount') }}</th><th>{{ tr('Term') }}</th><th>{{ tr('Rate') }}</th><th>{{ tr('Late fee') }}</th></tr>
            </thead>
            <tbody>
              <tr v-for="v in product.versions" :key="v.productVersionId" class="lt-row" :class="{ 'lt-row--on': v.productVersionId === versionId }" @click="selectVersion(v)">
                <td><b>{{ v.versionNo }}</b></td>
                <td>{{ formatDate(v.effectiveFromDate) }}</td>
                <td>
                  <span class="fx-chip" :class="STATUS_CHIP[v.statusCode]">{{ tr(STATUS[v.statusCode]) }}</span>
                  <span v-if="v.isInForce" class="fx-chip fx-chip--teal lt-gap">{{ tr('In force') }}</span>
                </td>
                <td>{{ money(v.minAmount) }} – {{ money(v.maxAmount) }}</td>
                <td>{{ v.minTermMonths }}–{{ v.maxTermMonths }} {{ tr('months') }}</td>
                <td>{{ v.annualRatePct }}% · {{ tr(label(data.options.interestMethods, v.interestMethodCode).split(' (')[0] ?? '') }}</td>
                <td>{{ lateFeeText(v) }}</td>
              </tr>
            </tbody>
          </table>
          <div v-if="!product.versions.length" class="fx-empty"><span class="fa fa-file-o" aria-hidden="true"></span>{{ tr('No versions yet. Start with New version.') }}</div>
        </div>

        <article v-if="version" class="lt-version">
          <header class="lt-version-head">
            <h3>{{ tr('Version') }} {{ version.versionNo }} <span class="fx-chip" :class="STATUS_CHIP[version.statusCode]">{{ tr(STATUS[version.statusCode]) }}</span></h3>
            <small class="c3-muted">
              {{ tr('Prepared by') }} {{ version.createdBy }}
              <template v-if="version.requestedBy"> · {{ tr('sent by') }} {{ version.requestedBy }} {{ formatDateTime(version.requestedAt) }}</template>
              <template v-if="version.decidedBy"> · {{ tr(version.statusCode === 'APPROVED' ? 'approved by' : 'rejected by') }} {{ version.decidedBy }} {{ formatDateTime(version.decidedAt) }}</template>
              <template v-if="version.decisionNotes"> · {{ version.decisionNotes }}</template>
            </small>
          </header>

          <fieldset class="lt-form" :disabled="!editable || busy">
            <div class="lt-group">
              <h4>{{ tr('Amount and term') }}</h4>
              <div class="c3-grid c3-grid--4">
                <label class="fc-field" :class="bad('effectiveFromDate')"><span>{{ tr('In force from') }}<b> *</b></span><input v-model="form.effectiveFromDate" type="date" class="fx-search" :min="data.today"></label>
                <label class="fc-field" :class="bad('minAmount')"><span>{{ tr('Minimum amount') }} ({{ tr('EGP') }})<b> *</b></span><input v-model.number="form.minAmount" type="number" min="0" step="any" class="fx-search"></label>
                <label class="fc-field" :class="bad('maxAmount')"><span>{{ tr('Maximum amount') }} ({{ tr('EGP') }})<b> *</b></span><input v-model.number="form.maxAmount" type="number" min="0" step="any" class="fx-search"></label>
                <label class="fc-field" :class="bad('repaymentFrequencyCode')"><span>{{ tr('Repayment') }}<b> *</b></span>
                  <select v-model="form.repaymentFrequencyCode" class="fx-select"><option v-for="o in data.options.frequencies" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select>
                </label>
                <label class="fc-field" :class="bad('minTermMonths')"><span>{{ tr('Shortest term') }} ({{ tr('months') }})<b> *</b></span><input v-model.number="form.minTermMonths" type="number" min="1" class="fx-search"></label>
                <label class="fc-field" :class="bad('maxTermMonths')"><span>{{ tr('Longest term') }} ({{ tr('months') }})<b> *</b></span><input v-model.number="form.maxTermMonths" type="number" min="1" class="fx-search"></label>
                <label class="fc-field lt-check"><input v-model="form.isInsuranceRequired" type="checkbox"><span>{{ tr('Life and disability insurance required') }}</span></label>
              </div>
            </div>

            <div class="lt-group">
              <h4>{{ tr('Interest') }}</h4>
              <div class="c3-grid c3-grid--4">
                <label class="fc-field" :class="bad('interestMethodCode')"><span>{{ tr('Interest method') }}<b> *</b></span>
                  <select v-model="form.interestMethodCode" class="fx-select"><option v-for="o in data.options.interestMethods" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select>
                </label>
                <label class="fc-field" :class="bad('annualRatePct')"><span>{{ tr('Yearly rate') }} (%)<b> *</b></span><input v-model.number="form.annualRatePct" type="number" min="0" max="100" step="any" class="fx-search"></label>
                <label class="fc-field" :class="bad('dayCountCode')"><span>{{ tr('Day count') }}<b> *</b></span>
                  <select v-model="form.dayCountCode" class="fx-select"><option v-for="o in data.options.dayCounts" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select>
                </label>
              </div>
            </div>

            <div class="lt-group">
              <h4>{{ tr('Late fee') }}</h4>
              <p class="c3-muted">{{ tr('No fee while the instalment is paid in full within the grace days. Once they pass, the fee is charged from day 1. There is no cap.') }}</p>
              <div class="c3-grid c3-grid--4">
                <label class="fc-field" :class="bad('lateFeeTypeCode')"><span>{{ tr('Late fee type') }}<b> *</b></span>
                  <select v-model="form.lateFeeTypeCode" class="fx-select"><option v-for="o in data.options.lateFeeTypes" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select>
                </label>
                <label class="fc-field" :class="bad('lateFeeGraceDays')"><span>{{ tr('Grace days') }}<b> *</b></span><input v-model.number="form.lateFeeGraceDays" type="number" min="0" max="90" class="fx-search"></label>
                <label v-if="form.lateFeeTypeCode === 'FLAT'" class="fc-field" :class="bad('lateFeeFlatAmount')"><span>{{ tr('Fee per late instalment') }} ({{ tr('EGP') }})<b> *</b></span><input v-model.number="form.lateFeeFlatAmount" type="number" min="0" step="any" class="fx-search"></label>
                <template v-if="form.lateFeeTypeCode === 'PCT_ONCE' || form.lateFeeTypeCode === 'PCT_DAILY'">
                  <label class="fc-field" :class="bad('lateFeeRatePct')"><span>{{ tr(form.lateFeeTypeCode === 'PCT_DAILY' ? 'Rate per day' : 'Rate') }} (%)<b> *</b></span><input v-model.number="form.lateFeeRatePct" type="number" min="0" max="100" step="any" class="fx-search"></label>
                  <label class="fc-field" :class="bad('lateFeeBaseCode')"><span>{{ tr('Calculated on') }}<b> *</b></span>
                    <select v-model="form.lateFeeBaseCode" class="fx-select"><option v-for="o in data.options.lateFeeBases" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select>
                  </label>
                </template>
              </div>
            </div>

            <div class="lt-group">
              <h4>{{ tr('Paying early') }}</h4>
              <p class="c3-muted">{{ tr('A part prepayment or an early settlement pays the interest of the current period to the day, never later interest. The fee is a percentage of the principal paid early.') }}</p>
              <div class="c3-grid c3-grid--4">
                <label class="fc-field lt-check"><input v-model="form.isPrepaymentAllowed" type="checkbox"><span>{{ tr('The customer may pay early') }}</span></label>
                <template v-if="form.isPrepaymentAllowed">
                  <label class="fc-field" :class="bad('prepaymentRecalcCode')"><span>{{ tr('After a part prepayment') }}<b> *</b></span>
                    <select v-model="form.prepaymentRecalcCode" class="fx-select"><option v-for="o in data.options.prepaymentRecalcs" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select>
                  </label>
                  <label class="fc-field" :class="bad('prepaymentFeePct')"><span>{{ tr('Part prepayment fee') }} (%)</span><input v-model.number="form.prepaymentFeePct" type="number" min="0" max="100" step="any" class="fx-search"></label>
                  <label class="fc-field" :class="bad('earlySettlementFeePct')"><span>{{ tr('Early settlement fee') }} (%)</span><input v-model.number="form.earlySettlementFeePct" type="number" min="0" max="100" step="any" class="fx-search"></label>
                </template>
              </div>
            </div>

            <div class="lt-group">
              <h4>{{ tr('Payment order') }}</h4>
              <p class="c3-muted">{{ tr('A payment clears the oldest instalment first, in this order within it.') }}</p>
              <ol class="lt-order" :class="bad('paymentOrder')">
                <li v-for="(part, i) in form.paymentOrder" :key="part">
                  <span>{{ tr(label(data.options.paymentParts, part)) }}</span>
                  <span v-if="editable" class="lt-move">
                    <button type="button" class="fx-btn fx-btn--sm" :disabled="i === 0" :aria-label="tr('Move up')" @click="move(i, -1)"><span class="fa fa-arrow-up" aria-hidden="true"></span></button>
                    <button type="button" class="fx-btn fx-btn--sm" :disabled="i === form.paymentOrder.length - 1" :aria-label="tr('Move down')" @click="move(i, 1)"><span class="fa fa-arrow-down" aria-hidden="true"></span></button>
                  </span>
                </li>
              </ol>
            </div>

            <label class="fc-field" :class="bad('changeReason')"><span>{{ tr('What this version changes and why') }}<b> *</b></span><input v-model="form.changeReason" class="fx-search" maxlength="1000"></label>
          </fieldset>

          <div v-if="version.statusCode === 'PENDING' && version.approveBlockedReason" class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr(version.approveBlockedReason) }}</div>

          <footer class="lt-actions">
            <template v-if="editable">
              <button type="button" class="fx-btn" :disabled="busy" @click="discard"><span class="fa fa-trash" aria-hidden="true"></span>{{ tr('Discard draft') }}</button>
              <button type="button" class="fx-btn" :disabled="busy" @click="save(false)"><span class="fa fa-save" aria-hidden="true"></span>{{ tr('Save draft') }}</button>
              <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save(true)"><span class="fa fa-paper-plane" aria-hidden="true"></span>{{ tr('Save and send for approval') }}</button>
            </template>
            <template v-else-if="version.statusCode === 'PENDING'">
              <button v-if="mine && data.canEdit" type="button" class="fx-btn" :disabled="busy" @click="takeBack"><span class="fa fa-undo" aria-hidden="true"></span>{{ tr('Take back to draft') }}</button>
              <template v-if="version.canApprove">
                <button type="button" class="fx-btn" :disabled="busy" @click="openDecision('reject')"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Reject') }}</button>
                <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="openDecision('approve')"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Approve') }}</button>
              </template>
            </template>
          </footer>
        </article>
      </section>
    </div>

    <Crm360Dialog v-model:visible="deciding" :title="tr(decision === 'approve' ? 'Approve Version' : 'Reject Version')" :subtitle="product && version ? `${product.productNameEn} · ${tr('Version')} ${version.versionNo}` : ''"
                  :icon="decision === 'approve' ? 'fa-check-circle' : 'fa-times-circle'" :busy="busy" :width="520">
      <p v-if="decision === 'approve'" class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Approval checks the terms against the lending parameters in force on the start date.') }}</p>
      <label class="fc-field" :class="bad('notes')"><span>{{ tr(decision === 'approve' ? 'Notes' : 'Reason') }}<b v-if="decision === 'reject'"> *</b></span><input v-model="notes" class="fx-search" maxlength="1000"></label>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="deciding = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn" :class="decision === 'approve' ? 'fx-btn--teal' : 'fx-btn--primary'" :disabled="busy" @click="decide">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : decision === 'approve' ? 'fa-check' : 'fa-times'" aria-hidden="true"></span>{{ tr(decision === 'approve' ? 'Approve' : 'Reject') }}
        </button>
      </template>
    </Crm360Dialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { ApiError } from '@/shared/api/http'
import { getUser } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { lendingProductsApi, type LendingProductsPage, type LendingVersion, type LendingVersionSave, type Option } from '../api/lendingProducts.api'

const LINES: Record<string, string> = { MICROFINANCE: 'Microfinance', CONSUMER_FINANCE: 'Consumer Finance' }
const STATUS = { DRAFT: 'Draft', PENDING: 'Waiting', APPROVED: 'Approved', REJECTED: 'Rejected' } as const
const STATUS_CHIP = { DRAFT: 'fx-chip--teal', PENDING: 'fx-chip--warn', APPROVED: 'fx-chip--ok', REJECTED: 'fx-chip--bad' } as const
const me = (getUser()?.userName ?? '').toLowerCase()
const egp = new Intl.NumberFormat('en-US', { maximumFractionDigits: 2 })

const data = ref<LendingProductsPage | null>(null)
const loading = ref(false)
const busy = ref(false)
const productId = ref<number | null>(null)
const versionId = ref<number | null>(null)
const errors = ref<Record<string, string[]>>({})
const bad = (f: string) => ({ 'c3-bad': !!errors.value[f] })

const product = computed(() => data.value?.products.find((p) => p.productId === productId.value) ?? null)
const version = computed(() => product.value?.versions.find((v) => v.productVersionId === versionId.value) ?? null)
const openVersion = computed(() => product.value?.versions.find((v) => v.statusCode === 'DRAFT' || v.statusCode === 'PENDING') ?? null)
const editable = computed(() => !!data.value?.canEdit && version.value?.statusCode === 'DRAFT')
const mine = computed(() => !!me && (version.value?.requestedBy ?? '').toLowerCase() === me)

const money = (n: number) => egp.format(n)
const label = (list: Option[], value: string) => list.find((o) => o.value === value)?.label ?? value
function lateFeeText(v: LendingVersion) {
  if (v.lateFeeTypeCode === 'NONE') return tr('None')
  const fee = v.lateFeeTypeCode === 'FLAT' ? `${tr('EGP')} ${money(v.lateFeeFlatAmount ?? 0)}` : `${v.lateFeeRatePct}%${v.lateFeeTypeCode === 'PCT_DAILY' ? ' ' + tr('a day') : ''}`
  return `${fee} · ${v.lateFeeGraceDays} ${tr('grace days')}`
}

const form = reactive<Omit<LendingVersionSave, 'rowVersion'>>({
  effectiveFromDate: null, minAmount: null, maxAmount: null, minTermMonths: null, maxTermMonths: null, repaymentFrequencyCode: 'MONTHLY',
  interestMethodCode: 'DECLINING', annualRatePct: null, dayCountCode: 'ACT_365', lateFeeGraceDays: null, lateFeeTypeCode: 'NONE', lateFeeRatePct: null,
  lateFeeFlatAmount: null, lateFeeBaseCode: 'OVERDUE_INSTALMENT', paymentOrder: [], isInsuranceRequired: false, isPrepaymentAllowed: true,
  prepaymentRecalcCode: 'REDUCE_INSTALMENT', prepaymentFeePct: 0, earlySettlementFeePct: 0, changeReason: null
})

function fill(v: LendingVersion | null) {
  errors.value = {}
  if (!v) return
  Object.assign(form, {
    effectiveFromDate: v.effectiveFromDate, minAmount: v.minAmount, maxAmount: v.maxAmount, minTermMonths: v.minTermMonths, maxTermMonths: v.maxTermMonths,
    repaymentFrequencyCode: v.repaymentFrequencyCode, interestMethodCode: v.interestMethodCode, annualRatePct: v.annualRatePct, dayCountCode: v.dayCountCode,
    lateFeeGraceDays: v.lateFeeGraceDays, lateFeeTypeCode: v.lateFeeTypeCode, lateFeeRatePct: v.lateFeeRatePct, lateFeeFlatAmount: v.lateFeeFlatAmount,
    lateFeeBaseCode: v.lateFeeBaseCode, paymentOrder: [...v.paymentOrder], isInsuranceRequired: v.isInsuranceRequired,
    isPrepaymentAllowed: v.isPrepaymentAllowed, prepaymentRecalcCode: v.prepaymentRecalcCode, prepaymentFeePct: v.prepaymentFeePct,
    earlySettlementFeePct: v.earlySettlementFeePct, changeReason: v.changeReason
  })
}

/** The version shown when a product opens: the one in preparation, else the one in force, else the latest. */
function selectProduct(id: number) {
  productId.value = id
  const p = data.value?.products.find((x) => x.productId === id)
  const v = p?.versions.find((x) => x.statusCode === 'DRAFT' || x.statusCode === 'PENDING') ?? p?.versions.find((x) => x.isInForce) ?? p?.versions[0] ?? null
  versionId.value = v?.productVersionId ?? null
  fill(v)
}
function selectVersion(v: LendingVersion) {
  versionId.value = v.productVersionId
  fill(v)
}
function move(i: number, by: number) {
  const list = form.paymentOrder
  const j = i + by
  if (j < 0 || j >= list.length) return
  const a = list[i]!
  list[i] = list[j]!
  list[j] = a
}

async function run(action: () => Promise<LendingProductsPage>, done: string, after?: () => void) {
  busy.value = true
  try {
    data.value = await action()
    after?.()
    showSuccess(tr(done))
    return true
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) {
      errors.value = err.problem.errors
      showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    } else showError(err instanceof Error ? err.message : String(err))
    return false
  } finally {
    busy.value = false
  }
}
/** After a change the same product stays open, on the version in preparation if there is one. */
function reopen() {
  if (productId.value !== null) selectProduct(productId.value)
}

const createDraft = () => run(() => lendingProductsApi.createDraft(productId.value!), 'Draft created from the version in force.', reopen)
async function save(send: boolean) {
  const v = version.value!
  const ok = await run(() => lendingProductsApi.save(v.productVersionId, { ...form, rowVersion: v.rowVersion }), 'Draft saved.', reopen)
  if (ok && send) await run(() => lendingProductsApi.submit(v.productVersionId), 'Version sent for approval. Another user must approve it.', reopen)
}
const discard = () => run(() => lendingProductsApi.discard(version.value!.productVersionId), 'Draft discarded.', reopen)
const takeBack = () => run(() => lendingProductsApi.takeBack(version.value!.productVersionId), 'Version taken back to draft.', reopen)

const deciding = ref(false)
const decision = ref<'approve' | 'reject'>('approve')
const notes = ref('')
function openDecision(d: 'approve' | 'reject') {
  decision.value = d
  notes.value = ''
  errors.value = {}
  deciding.value = true
}
const decide = () => {
  const id = version.value!.productVersionId
  return decision.value === 'approve'
    ? run(() => lendingProductsApi.approve(id, notes.value || null), 'Version approved.', () => { deciding.value = false; reopen() })
    : run(() => lendingProductsApi.reject(id, notes.value), 'Version rejected.', () => { deciding.value = false; reopen() })
}

async function load() {
  loading.value = true
  try {
    data.value = await lendingProductsApi.get()
    const first = data.value.products[0]
    if (first) selectProduct(first.productId)
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  } finally {
    loading.value = false
  }
}
onMounted(load)
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
.lt-layout { display: grid; grid-template-columns: 300px minmax(0, 1fr); gap: 16px; align-items: start; }
.lt-products { display: flex; flex-direction: column; gap: 8px; padding: 12px; }
.lt-product { display: flex; flex-direction: column; align-items: flex-start; gap: 4px; padding: 10px 12px; border: 1px solid var(--fin-line); border-radius: var(--fin-radius-sm);
  background: transparent; text-align: start; cursor: pointer; color: var(--fin-ink); }
.lt-product small { color: var(--fin-muted); }
.lt-product--on { border-color: var(--fin-teal, #0f766e); background: var(--fin-teal-50); }
.lt-main { display: flex; flex-direction: column; gap: 14px; padding: 16px; }
.lt-main-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 12px; }
.lt-main-head h2 { margin: 0 0 2px; font-size: 18px; }
.lt-row { cursor: pointer; }
.lt-row--on td { background: var(--fin-teal-50); }
.lt-gap { margin-inline-start: 6px; }
.lt-version { display: flex; flex-direction: column; gap: 12px; border-top: 1px solid var(--fin-line); padding-top: 14px; }
.lt-version-head h3 { margin: 0 0 2px; font-size: 16px; display: flex; gap: 8px; align-items: center; }
.lt-form { border: 0; padding: 0; margin: 0; display: flex; flex-direction: column; gap: 14px; min-width: 0; }
.lt-group h4 { margin: 0 0 6px; font-size: 14px; color: var(--fin-ink); }
.lt-group .c3-muted { margin: 0 0 8px; }
.c3-grid--4 { grid-template-columns: repeat(4, minmax(0, 1fr)); }
.lt-check { flex-direction: row; align-items: center; gap: 8px; padding-top: 22px; }
.lt-order { margin: 0; padding-inline-start: 22px; display: flex; flex-direction: column; gap: 6px; max-width: 420px; }
.lt-order li { display: list-item; }
.lt-order li > span:first-child { display: inline-block; min-width: 180px; }
.lt-move { display: inline-flex; gap: 4px; margin-inline-start: 8px; }
.lt-actions { display: flex; justify-content: flex-end; gap: 8px; flex-wrap: wrap; }
.lt-none { display: flex; flex-direction: column; align-items: center; gap: 8px; padding: 32px; text-align: center; }
.lt-none > .fa { font-size: 28px; color: var(--fin-muted); }
@media (max-width: 900px) {
  .lt-layout { grid-template-columns: 1fr; }
  .c3-grid--4 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
</style>
