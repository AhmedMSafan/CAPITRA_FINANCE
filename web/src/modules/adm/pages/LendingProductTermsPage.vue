<template>
  <!-- Administration > Product Catalog > a product's Lending Terms (its row action, for products of business line Microfinance or
       Consumer Finance). Ahmed, 27 Nov 2026: one set of terms, no versions and no limits. An admin changes them and sends the change;
       another admin approves it and it applies to new finances from then on (each finance keeps its own copy). Every approved change
       is in the audit trail. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'adm-home' }">{{ tr('Administration') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'adm-products' }">{{ tr('Product Catalog') }}</RouterLink><span aria-hidden="true">›</span>
          <b>{{ product?.productCode ?? tr('Lending Terms') }}</b>
        </nav>
        <h1>{{ tr('Lending Terms') }}<template v-if="product"> · {{ product.productNameEn }}</template></h1>
      </div>
      <div class="fx-actions">
        <RouterLink :to="{ name: 'adm-products' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Product Catalog') }}</RouterLink>
      </div>
    </div>

    <div v-if="loading && !data" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading products...') }}</div>

    <section v-else-if="data && !product" class="fx-card lt-none">
      <span class="fa fa-cube" aria-hidden="true"></span>
      <b>{{ tr('This product has no lending terms.') }}</b>
      <p>{{ tr('Lending terms are set for products of business line Microfinance or Consumer Finance.') }}</p>
      <RouterLink :to="{ name: 'adm-products' }" class="fx-btn fx-btn--teal"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Product Catalog') }}</RouterLink>
    </section>

    <div v-else-if="data" class="lt-layout">
      <section v-if="product" class="fx-card lt-main">
        <header class="lt-main-head">
          <div>
            <h2>{{ product.productNameEn }}</h2>
            <small class="c3-muted">{{ product.productCode }} · {{ tr(LINES[product.businessLineCode] ?? '') }} · {{ tr('Type') }} {{ product.productTypeCode }}<template v-if="!product.isActive"> · {{ tr('Inactive') }}</template></small>
          </div>
          <button v-if="data.canEdit && !openVersion" type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="createDraft">
            <span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Change terms') }}
          </button>
        </header>

        <!-- a change in preparation or waiting for approval: what it changes, and its actions -->
        <div v-if="openVersion" class="lt-ban" :class="{ 'lt-ban--wait': openVersion.statusCode === 'PENDING' }">
          <span class="fx-chip" :class="openVersion.statusCode === 'PENDING' ? 'fx-chip--warn' : 'fx-chip--teal'">
            {{ tr(openVersion.statusCode === 'PENDING' ? 'Change waiting for approval' : 'Change being prepared') }}</span>
          <span class="lt-ban-txt">
            <template v-for="(c, i) in openVersion.changes" :key="c.field"><template v-if="i"> · </template>{{ tr(c.field) }} <b>{{ c.oldValue }} → {{ c.newValue }}</b></template>
            <template v-if="!openVersion.changes.length">{{ tr('Nothing changed yet.') }}</template>
            <template v-if="openVersion.requestedBy"> · {{ tr('asked by') }} {{ openVersion.requestedBy }} {{ formatDateTime(openVersion.requestedAt) }}</template>
          </span>
          <template v-if="openVersion.statusCode === 'PENDING'">
            <button v-if="mine && data.canEdit" type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="takeBack"><span class="fa fa-undo" aria-hidden="true"></span>{{ tr('Take back') }}</button>
            <template v-if="openVersion.canApprove">
              <button type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="openDecision('reject')"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Reject') }}</button>
              <button type="button" class="fx-btn fx-btn--teal fx-btn--sm" :disabled="busy" @click="openDecision('approve')"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Approve') }}</button>
            </template>
          </template>
        </div>
        <p v-if="openVersion?.approveBlockedReason" class="c3-muted lt-gap0">{{ tr(openVersion.approveBlockedReason) }}</p>

        <article v-if="version" class="lt-version">
          <header class="lt-version-head">
            <h3>{{ tr(version.statusCode === 'APPROVED' ? 'Lending terms' : 'The change') }}
              <span v-if="version.isInForce" class="fx-chip fx-chip--ok">{{ tr('In force') }}</span></h3>
            <small v-if="!editable && version.decidedBy" class="c3-muted">{{ tr('Last change approved by') }} {{ version.decidedBy }} {{ formatDateTime(version.decidedAt) }}</small>
          </header>

          <fieldset class="lt-form" :disabled="!editable || busy">
            <div class="lt-group">
              <h4>{{ tr('Repayment') }}</h4>
              <div class="c3-grid c3-grid--4">
                <label class="fc-field" :class="bad('repaymentFrequencyCode')"><span>{{ tr('Repayment') }}<b> *</b></span>
                  <select v-model="form.repaymentFrequencyCode" class="fx-select"><option v-for="o in data.options.frequencies" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select>
                </label>
                <label class="fc-field lt-check"><input v-model="form.isInsuranceRequired" type="checkbox"><span>{{ tr('Life and disability insurance required') }}</span></label>
              </div>
              <p class="c3-muted lt-note">{{ tr('No amount or term limits: the amount and term are set on each finance.') }}</p>
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
                <label v-if="form.lateFeeTypeCode !== 'NONE'" class="fc-field" :class="bad('lateFeeGraceDays')"><span>{{ tr('Grace days') }}<b> *</b></span><input v-model.number="form.lateFeeGraceDays" type="number" min="0" max="90" class="fx-search"></label>
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

            <label v-if="editable" class="fc-field" :class="bad('changeReason')"><span>{{ tr('What this change is and why') }}<b> *</b></span><input v-model="form.changeReason" class="fx-search" maxlength="1000"></label>
          </fieldset>

          <footer v-if="editable" class="lt-actions">
            <button type="button" class="fx-btn" :disabled="busy" @click="discard"><span class="fa fa-trash" aria-hidden="true"></span>{{ tr('Discard change') }}</button>
            <button type="button" class="fx-btn" :disabled="busy" @click="save(false)"><span class="fa fa-save" aria-hidden="true"></span>{{ tr('Save') }}</button>
            <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save(true)"><span class="fa fa-paper-plane" aria-hidden="true"></span>{{ tr('Save and send for approval') }}</button>
          </footer>
        </article>
      </section>

      <!-- every approved change: field, old and new value, who asked and who approved -->
      <section v-if="product" class="fx-card lt-main">
        <h3 class="lt-h3">{{ tr('Audit trail') }}</h3>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead><tr><th>{{ tr('When') }}</th><th>{{ tr('Field') }}</th><th>{{ tr('Old') }} → {{ tr('New') }}</th><th>{{ tr('Why') }}</th><th>{{ tr('Asked by') }}</th><th>{{ tr('Approved by') }}</th></tr></thead>
            <tbody>
              <tr v-for="(a, i) in audit" :key="i">
                <td>{{ formatDateTime(a.at) }}</td><td>{{ tr(a.field) }}</td><td><b>{{ a.oldValue }}</b> → <b>{{ a.newValue }}</b></td>
                <td>{{ a.reason || '—' }}</td><td>{{ a.askedBy || '—' }}</td><td>{{ a.approvedBy || '—' }}</td>
              </tr>
            </tbody>
          </table>
          <div v-if="!audit.length" class="fx-empty"><span class="fa fa-history" aria-hidden="true"></span>{{ tr('No changes yet.') }}</div>
        </div>
      </section>
    </div>      </section>
    </div>

    <Crm360Dialog v-model:visible="deciding" :title="tr(decision === 'approve' ? 'Approve change' : 'Reject change')" :subtitle="product?.productNameEn ?? ''"
                  :icon="decision === 'approve' ? 'fa-check-circle' : 'fa-times-circle'" :busy="busy" :width="520">
      <p v-if="decision === 'approve'" class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Once approved, new finances take these terms; finances already created keep theirs.') }}</p>
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
import { formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { lendingProductsApi, type LendingProductsPage, type LendingVersion, type LendingVersionSave, type Option } from '../api/lendingProducts.api'

const LINES: Record<string, string> = { MICROFINANCE: 'Microfinance', CONSUMER_FINANCE: 'Consumer Finance' }
const me = (getUser()?.userName ?? '').toLowerCase()

const props = defineProps<{ lendingProductId: number }>()
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
const mine = computed(() => !!me && (openVersion.value?.requestedBy ?? '').toLowerCase() === me)
const label = (list: Option[], value: string) => list.find((o) => o.value === value)?.label ?? value
/** The audit trail: each approved change's fields, newest first. */
const audit = computed(() => (product.value?.versions ?? []).filter((v) => v.statusCode === 'APPROVED')
  .flatMap((v) => v.changes.map((c) => ({ ...c, at: v.decidedAt ?? v.requestedAt, reason: v.changeReason, askedBy: v.requestedBy ?? v.createdBy, approvedBy: v.decidedBy }))))

const form = reactive<Omit<LendingVersionSave, 'rowVersion'>>({
  repaymentFrequencyCode: 'MONTHLY',
  interestMethodCode: 'DECLINING', annualRatePct: null, dayCountCode: 'ACT_365', lateFeeGraceDays: null, lateFeeTypeCode: 'NONE', lateFeeRatePct: null,
  lateFeeFlatAmount: null, lateFeeBaseCode: 'OVERDUE_INSTALMENT', paymentOrder: [], isInsuranceRequired: false, isPrepaymentAllowed: true,
  prepaymentRecalcCode: 'REDUCE_INSTALMENT', prepaymentFeePct: 0, earlySettlementFeePct: 0, changeReason: null
})

function fill(v: LendingVersion | null) {
  errors.value = {}
  if (!v) return
  Object.assign(form, {
    repaymentFrequencyCode: v.repaymentFrequencyCode, interestMethodCode: v.interestMethodCode, annualRatePct: v.annualRatePct, dayCountCode: v.dayCountCode,
    lateFeeGraceDays: v.lateFeeGraceDays, lateFeeTypeCode: v.lateFeeTypeCode, lateFeeRatePct: v.lateFeeRatePct, lateFeeFlatAmount: v.lateFeeFlatAmount,
    lateFeeBaseCode: v.lateFeeBaseCode, paymentOrder: [...v.paymentOrder], isInsuranceRequired: v.isInsuranceRequired,
    isPrepaymentAllowed: v.isPrepaymentAllowed, prepaymentRecalcCode: v.prepaymentRecalcCode, prepaymentFeePct: v.prepaymentFeePct,
    earlySettlementFeePct: v.earlySettlementFeePct, changeReason: v.changeReason
  })
}

/** The terms shown when a product opens: the change in preparation, else the terms in force, else the latest. */
function selectProduct(id: number) {
  productId.value = id
  const p = data.value?.products.find((x) => x.productId === id)
  const v = p?.versions.find((x) => x.statusCode === 'DRAFT' || x.statusCode === 'PENDING') ?? p?.versions.find((x) => x.isInForce) ?? p?.versions[0] ?? null
  versionId.value = v?.productVersionId ?? null
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

const createDraft = () => run(() => lendingProductsApi.createDraft(productId.value!), 'Change the terms below, then send them for approval.', reopen)
async function save(send: boolean) {
  const v = version.value!
  const ok = await run(() => lendingProductsApi.save(v.productVersionId, { ...form, rowVersion: v.rowVersion }), 'Change saved.', reopen)
  if (ok && send) await run(() => lendingProductsApi.submit(v.productVersionId), 'Change sent for approval. Another admin must approve it.', reopen)
}
const discard = () => run(() => lendingProductsApi.discard(version.value!.productVersionId), 'Change discarded.', reopen)
const takeBack = () => run(() => lendingProductsApi.takeBack(openVersion.value!.productVersionId), 'Change taken back.', reopen)

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
  const id = openVersion.value!.productVersionId
  return decision.value === 'approve'
    ? run(() => lendingProductsApi.approve(id, notes.value || null), 'Change approved: new finances take these terms.', () => { deciding.value = false; reopen() })
    : run(() => lendingProductsApi.reject(id, notes.value), 'Change rejected.', () => { deciding.value = false; reopen() })
}

async function load() {
  loading.value = true
  try {
    data.value = await lendingProductsApi.get()
    if (data.value.products.some((p) => p.productId === props.lendingProductId)) selectProduct(props.lendingProductId)
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
.lt-layout { display: block; }
.lt-products { display: flex; flex-direction: column; gap: 8px; padding: 12px; }
.lt-product { display: flex; flex-direction: column; align-items: flex-start; gap: 4px; padding: 10px 12px; border: 1px solid var(--fin-line); border-radius: var(--fin-radius-sm);
  background: transparent; text-align: start; cursor: pointer; color: var(--fin-ink); }
.lt-product small { color: var(--fin-muted); }
.lt-product--on { border-color: var(--fin-teal, #0f766e); background: var(--fin-teal-50); }
.lt-main { display: flex; flex-direction: column; gap: 14px; padding: 16px; }
.lt-main-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 12px; }
.lt-main-head h2 { margin: 0 0 2px; font-size: 18px; }
.lt-ban { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; padding: 10px 14px; border: 1px solid var(--fin-line); border-radius: 10px; background: var(--fin-teal-50); }
.lt-ban--wait { border-color: #f1d58a; background: #fff8e6; }
.lt-ban-txt { flex: 1; min-width: 0; font-size: 13px; }
.lt-gap0 { margin: 0; }
.lt-note { margin: 6px 0 0; }
.lt-h3 { margin: 0; font-size: 15px; color: var(--fin-teal-700); }
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
