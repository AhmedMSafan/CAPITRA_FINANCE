<script setup lang="ts">
// Mortgage Portfolio Purchasing product (ADM_PRODUCTS row MTG_PORTFOLIO_PURCHASING): FINOVA_FINANCEING
// MortgagePortfolioPurchasingProductPage. The product's details are read only here; its four rates are edited.
import { computed, onMounted, reactive, ref } from 'vue'
import { ApiError } from '@/shared/api/http'
import { hasPermission } from '@/shared/auth/session'
import { showSuccess } from '@/shared/feedback/feedback'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { formatCell } from '@/shared/crud/format'
import { tr } from '@/shared/layout/language'
import '@/shared/styles/apex-fx-inbox.css'
import { productsApi, type Product } from '../api/products.api'

type RateField = 'mppPvRate' | 'mppLateFeeRate' | 'mppReservedAmountPct' | 'mppInvestorPaymentPct'

const RATES: { name: RateField; label: string }[] = [
  { name: 'mppPvRate', label: 'PV Rate %' },
  { name: 'mppLateFeeRate', label: 'Late Fee Rate %' },
  { name: 'mppReservedAmountPct', label: 'Reserved Amount %' },
  { name: 'mppInvestorPaymentPct', label: 'Investor Payment %' }
]

const product = ref<Product | null>(null)
const loading = ref(true)
const saving = ref(false)
const pageError = ref('')
const form = reactive<Record<RateField, string>>({ mppPvRate: '', mppLateFeeRate: '', mppReservedAmountPct: '', mppInvestorPaymentPct: '' })
const fieldErrors = reactive<Partial<Record<RateField, string>>>({})
const canEdit = hasPermission('ADM.PRODUCT.EDIT')

const facts = computed(() => {
  const p = product.value
  if (!p) return []
  return [
    ['Product Code', p.productCode],
    ['Product Name', p.productNameEn],
    ['Business Line', p.businessLineCode],
    ['Product Type', p.productTypeCode],
    ['Default Currency', p.defaultCurrencyCode ?? '-'],
    ['Effective From', formatCell(p.effectiveFromDate, 'date')],
    ['Effective To', formatCell(p.effectiveToDate, 'date') || '-']
  ] as [string, string][]
})

function fill(p: Product) {
  product.value = p
  for (const r of RATES) form[r.name] = String(p[r.name])
}

async function load() {
  loading.value = true
  pageError.value = ''
  try {
    const p = await productsApi.getMortgagePortfolioPurchasing()
    if (p) fill(p)
    else product.value = null
  } catch (error) {
    pageError.value = error instanceof Error ? error.message : 'Unable to load the product.'
  } finally {
    loading.value = false
  }
}

async function save() {
  if (!product.value) return
  Object.keys(fieldErrors).forEach(k => delete fieldErrors[k as RateField])
  for (const r of RATES) {
    const v = form[r.name]
    if (v === '' || v === null) fieldErrors[r.name] = `${r.label} is required.`
    else if (Number(v) < 0 || Number(v) > 100) fieldErrors[r.name] = `${r.label} must be between 0 and 100.`
  }
  if (Object.keys(fieldErrors).length) return

  saving.value = true
  try {
    fill(await productsApi.saveMortgagePortfolioPurchasing({
      mppPvRate: Number(form.mppPvRate),
      mppLateFeeRate: Number(form.mppLateFeeRate),
      mppReservedAmountPct: Number(form.mppReservedAmountPct),
      mppInvestorPaymentPct: Number(form.mppInvestorPaymentPct),
      rowVersion: product.value.rowVersion
    }))
    showSuccess('Product parameters saved.')
  } catch (error) {
    if (error instanceof ApiError && error.problem?.errors) {
      for (const [key, messages] of Object.entries(error.problem.errors)) {
        const name = RATES.find(r => r.name.toLowerCase() === key.toLowerCase())?.name
        if (name) fieldErrors[name] = messages.join(' ')
        else pageError.value = messages.join(' ')
      }
    } else pageError.value = error instanceof Error ? error.message : 'Unable to save the product parameters.'
  } finally {
    saving.value = false
  }
}

onMounted(load)
useFeedback({ errors: [pageError] })
</script>

<template>
  <div class="fx mpp">
    <div class="fx-head">
      <div>
        <h1>{{ tr('Mortgage Portfolio Purchasing') }}</h1>
        <p>MTG_PORTFOLIO_PURCHASING</p>
      </div>
      <div class="fx-actions">
        <span v-if="product" class="fx-chip" :class="{ 'fx-chip--ok': product.isActive }">{{ tr(product.isActive ? 'Active' : 'Inactive') }}</span>
        <RouterLink to="/admin/products" class="fx-btn fx-btn--ghost"><i class="pi pi-arrow-left"></i><span>{{ tr('Product Catalog') }}</span></RouterLink>
      </div>
    </div>

    <div v-if="loading" class="fx-card fx-card-pad mpp-state"><i class="pi pi-spin pi-spinner"></i> {{ tr('Loading...') }}</div>

    <div v-else-if="!product" class="fx-card fx-card-pad mpp-state">
      <i class="pi pi-info-circle"></i>
      {{ tr('Product MTG_PORTFOLIO_PURCHASING is not in the product catalog yet. Add it there first.') }}
      <RouterLink to="/admin/products" class="fx-btn fx-btn--sm">{{ tr('Product Catalog') }}</RouterLink>
    </div>

    <template v-else>
      <section class="fx-card fx-card-pad">
        <h2>{{ tr('Product Information') }}</h2>
        <div class="mpp-facts">
          <div v-for="[label, value] in facts" :key="label"><span>{{ tr(label) }}</span><b>{{ value }}</b></div>
        </div>
      </section>

      <section class="fx-card fx-card-pad">
        <h2>{{ tr('Portfolio Purchasing Parameters') }}</h2>
        <fieldset class="mpp-grid" :disabled="!canEdit || saving">
          <div v-for="r in RATES" :key="r.name" class="mpp-field">
            <label :for="`mpp-${r.name}`">{{ tr(r.label) }} *</label>
            <input :id="`mpp-${r.name}`" v-model="form[r.name]" type="number" step="0.000001" min="0" max="100" />
            <small v-if="fieldErrors[r.name]" class="mpp-error">{{ fieldErrors[r.name] }}</small>
          </div>
        </fieldset>
        <div v-if="canEdit" class="mpp-foot">
          <button type="button" class="fx-btn fx-btn--teal" :disabled="saving" @click="save">
            <i :class="saving ? 'pi pi-spin pi-spinner' : 'pi pi-check'"></i>{{ tr('Save Parameters') }}
          </button>
        </div>
      </section>
    </template>
  </div>
</template>

<style scoped>
.mpp { display: flex; flex-direction: column; gap: 16px; }
.mpp-state { display: flex; align-items: center; gap: 10px; color: var(--cap-muted); }
.mpp-facts { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 14px 18px; }
.mpp-facts span { display: block; color: var(--cap-muted); font-size: 11px; }
.mpp-facts b { display: block; margin-top: 3px; color: var(--cap-heading); font-size: 14px; font-weight: 600; }
.mpp-grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 14px 18px; margin: 0; padding: 0; border: 0; }
.mpp-field label { display: block; margin-bottom: 6px; color: var(--cap-ink-soft); font-size: 11px; font-weight: 500; }
.mpp-field input { width: 100%; height: 42px; box-sizing: border-box; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 8px; font: inherit; font-size: 13px; }
.mpp-field input:disabled { background: var(--cap-bg); color: var(--cap-muted); }
.mpp-error { display: block; margin-top: 5px; color: var(--cap-danger); font-size: 11px; }
.mpp-foot { display: flex; justify-content: flex-end; margin-top: 16px; }
@media (max-width: 900px) { .mpp-facts, .mpp-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
