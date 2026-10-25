<script setup lang="ts">
// "Financial rates & fees" dialog of the portfolio overview (APEX App 105 page 203, process FIN_TERMS_SAVE).
import { reactive, ref } from 'vue'
import { showSuccess } from '@/shared/feedback/feedback'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { splitApiError } from '@/shared/crud/errors'
import { portfoliosApi, type FinancialTerms, type Portfolio } from '../api/portfolios.api'

const props = defineProps<{ portfolio: Portfolio }>()
const emit = defineEmits<{ close: []; saved: [portfolio: Portfolio] }>()

type Key = keyof FinancialTerms

const FIELDS: { key: Key; label: string; unit: 'pct' | 'currency'; max?: number }[] = [
  { key: 'coveragePct', label: 'Coverage percentage', unit: 'pct', max: 100 },
  { key: 'marginPct', label: 'Margin percentage', unit: 'pct' },
  { key: 'pvRate', label: 'Present value calculation rate', unit: 'pct' },
  { key: 'reservedAmountPct', label: 'Reserved amount percentage', unit: 'pct', max: 100 },
  { key: 'lateFeeRate', label: 'Late fees rate', unit: 'pct' },
  { key: 'adminExpensePct', label: 'Admin expense percentage', unit: 'pct' },
  { key: 'legalFeePerUnitAmount', label: 'Legal fees per unit', unit: 'currency' },
  { key: 'valuationFeePerUnitAmount', label: 'Valuation expense per unit', unit: 'currency' }
]

const form = reactive<Record<Key, number | ''>>(
  Object.fromEntries(FIELDS.map(f => [f.key, props.portfolio[f.key] ?? ''])) as Record<Key, number | ''>
)
const fieldErrors = ref<Record<string, string>>({})
const formError = ref('')
const saving = ref(false)

async function save() {
  saving.value = true
  formError.value = ''
  fieldErrors.value = {}
  try {
    const terms = Object.fromEntries(FIELDS.map(f => [f.key, form[f.key] === '' ? null : Number(form[f.key])])) as FinancialTerms
    const saved = await portfoliosApi.saveFinancialTerms(props.portfolio, terms)
    showSuccess('Rates and fees saved.')
    emit('saved', saved)
  } catch (error) {
    const split = splitApiError(error, FIELDS.map(f => f.key), 'Not saved.')
    fieldErrors.value = split.fields
    formError.value = split.form
  } finally {
    saving.value = false
  }
}

function close() {
  if (!saving.value) emit('close')
}

useFeedback({ errors: [formError] })
</script>

<template>
  <div class="cp-modal-overlay" @click.self="close">
    <section class="cp-modal terms" role="dialog" aria-modal="true" aria-labelledby="terms-title">
      <header class="cp-modal__header">
        <div>
          <h3 id="terms-title"><i class="pi pi-percentage"></i> Financial rates &amp; fees</h3>
          <p>{{ portfolio.portfolioNo }}</p>
        </div>
        <button type="button" class="cp-modal__close" :disabled="saving" aria-label="Close" @click="close">
          <i class="pi pi-times"></i>
        </button>
      </header>
      <div class="cp-modal__body">
        <fieldset class="cp-form" :disabled="saving">
          <div v-for="f in FIELDS" :key="f.key" class="cp-field">
            <label :for="`ft-${f.key}`">{{ f.label }}</label>
            <div class="terms__in">
              <input :id="`ft-${f.key}`" v-model="form[f.key]" type="number" min="0" :max="f.max" step="0.01" inputmode="decimal" />
              <span>{{ f.unit === 'pct' ? '%' : portfolio.currencyCode }}</span>
            </div>
            <small v-if="fieldErrors[f.key]" class="cp-field__error">{{ fieldErrors[f.key] }}</small>
          </div>
        </fieldset>
        <p class="terms__note">
          Changes apply from now on. A proposed purchase value that was already calculated changes only when the Financial
          Review is saved again.
        </p>
      </div>
      <footer class="cp-modal__footer">
        <span class="cp-spacer"></span>
        <button type="button" class="cp-btn" :disabled="saving" @click="close">Cancel</button>
        <button type="button" class="cp-btn cp-btn--primary" :disabled="saving" @click="save">
          <i :class="saving ? 'pi pi-spin pi-spinner' : 'pi pi-check'"></i><span>Save</span>
        </button>
      </footer>
    </section>
  </div>
</template>

<style scoped>
.terms { width: min(640px, calc(100vw - 36px)); }
.terms__in { position: relative; }
.terms__in input { padding-inline-end: 58px; }
.terms__in span { position: absolute; inset-inline-end: 6px; top: 50%; transform: translateY(-50%); min-width: 30px; height: 28px; padding: 0 6px; display: grid; place-items: center; border-radius: 6px; background: var(--cap-primary-tint); color: var(--cap-primary-dark); font-size: 12px; font-weight: 700; }
.terms__note { margin: 16px 0 0; padding: 10px 12px; border: 1px solid var(--cap-warning-border); border-radius: 10px; background: var(--cap-warning-bg); color: var(--cap-warning); font-size: 12px; }
</style>
