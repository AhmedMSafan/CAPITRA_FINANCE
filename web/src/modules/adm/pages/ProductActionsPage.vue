<script setup lang="ts">
// Actions of a product (ADM_PRODUCT_ACTIONS): the product's fixed actions outside the workflow (e.g. Cheque batch submission)
// and the product event rule each one posts. The list of actions comes from the code; only the rule and on/off are set here.
import { computed, onMounted, reactive, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showSuccess } from '@/shared/feedback/feedback'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { formatCell } from '@/shared/crud/format'
import { tr } from '@/shared/layout/language'
import '@/shared/styles/apex-fx-inbox.css'
import { labelOf, productsApi, RULE_AMOUNTS, type Product, type ProductAction, type ProductActionRuleOption } from '../api/products.api'

const props = defineProps<{ productId: number }>()

const product = ref<Product | null>(null)
const actions = ref<ProductAction[]>([])
const rules = ref<ProductActionRuleOption[]>([])
const loading = ref(true)
const savingCode = ref('')
const pageError = ref('')
const form = reactive<Record<string, { ruleId: string; isActive: boolean }>>({})
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

function fill(list: ProductAction[], options: ProductActionRuleOption[]) {
  actions.value = list
  rules.value = options
  for (const a of list) form[a.actionCode] = { ruleId: a.productEventRuleId ? String(a.productEventRuleId) : '', isActive: a.isActive }
}

async function load() {
  loading.value = true
  pageError.value = ''
  try {
    const [all, acts] = await Promise.all([productsApi.list(), productsApi.actions(props.productId)])
    product.value = all.find(p => p.productId === props.productId) ?? null
    fill(acts.actions, acts.rules)
  } catch (error) {
    pageError.value = error instanceof Error ? error.message : 'Unable to load the product actions.'
  } finally {
    loading.value = false
  }
}

async function save(a: ProductAction) {
  const f = form[a.actionCode]
  if (!f) return
  savingCode.value = a.actionCode
  try {
    const res = await productsApi.saveAction(props.productId, a.actionCode, {
      productEventRuleId: f.ruleId ? Number(f.ruleId) : null,
      isActive: f.isActive,
      rowVersion: a.rowVersion
    })
    fill(res.actions, res.rules)
    showSuccess('Action saved.')
  } catch (error) {
    pageError.value = error instanceof Error ? error.message : 'Unable to save the action.'
  } finally {
    savingCode.value = ''
  }
}

const amounts = (a: ProductAction) => a.amountCodes.map(c => labelOf(RULE_AMOUNTS, c)).join(', ')

onMounted(load)
useFeedback({ errors: [pageError] })
</script>

<template>
  <div class="fx pa">
    <div class="fx-head">
      <div>
        <h1>{{ tr('Product Actions') }}</h1>
        <p>{{ product?.productCode ?? '' }}</p>
      </div>
      <div class="fx-actions">
        <span v-if="product" class="fx-chip" :class="{ 'fx-chip--ok': product.isActive }">{{ tr(product.isActive ? 'Active' : 'Inactive') }}</span>
        <RouterLink to="/admin/product-event-rules" class="fx-btn"><i class="pi pi-sitemap"></i><span>{{ tr('Product Event Rules') }}</span></RouterLink>
        <RouterLink to="/admin/products" class="fx-btn fx-btn--ghost"><i class="pi pi-arrow-left"></i><span>{{ tr('Product Catalog') }}</span></RouterLink>
      </div>
    </div>

    <div v-if="loading" class="fx-card fx-card-pad pa-state"><i class="pi pi-spin pi-spinner"></i> {{ tr('Loading...') }}</div>

    <template v-else-if="product">
      <section class="fx-card fx-card-pad">
        <h2>{{ tr('Product Information') }}</h2>
        <div class="pa-facts">
          <div v-for="[label, value] in facts" :key="label"><span>{{ tr(label) }}</span><b>{{ value }}</b></div>
        </div>
      </section>

      <section class="fx-card fx-card-pad">
        <h2>{{ tr('Actions') }}</h2>
        <p class="pa-intro">
          {{ tr('Each action posts the GL entries of the rule linked to it, in the same transaction as the action: if the GL cannot be posted, nothing is saved. The rule must belong to this product.') }}
        </p>
        <div v-if="actions.length === 0" class="fx-empty">{{ tr('This product has no actions outside the workflow.') }}</div>
        <div v-for="a in actions" :key="a.actionCode" class="pa-row">
          <div class="pa-what">
            <b>{{ a.actionNameEn }}</b>
            <span class="pa-code">{{ a.actionCode }}</span>
            <p>{{ a.description }}</p>
            <small>{{ tr('GL tran set ID') }}: CAP-{{ a.moduleCode }}-YYYY-00000001 · {{ tr('Amounts') }}: {{ amounts(a) }}</small>
          </div>
          <fieldset v-if="form[a.actionCode]" class="pa-link" :disabled="!canEdit || savingCode === a.actionCode">
            <div class="pa-field">
              <label :for="`pa-rule-${a.actionCode}`">{{ tr('Product Event Rule') }} *</label>
              <select :id="`pa-rule-${a.actionCode}`" v-model="form[a.actionCode]!.ruleId">
                <option value="">{{ tr('- Choose the rule -') }}</option>
                <option v-for="r in rules" :key="r.productEventRuleId" :value="String(r.productEventRuleId)">
                  {{ r.ruleCode }} - {{ r.ruleNameEn }}{{ r.isActive ? '' : ' · inactive' }}
                </option>
              </select>
            </div>
            <label class="pa-check"><input v-model="form[a.actionCode]!.isActive" type="checkbox" /> {{ tr('Active') }}</label>
            <span class="fx-chip" :class="{ 'fx-chip--ok': a.isActive && a.productEventRuleId }">
              {{ tr(a.isActive && a.productEventRuleId ? 'Linked' : 'Not linked') }}
            </span>
            <button v-if="canEdit" type="button" class="fx-btn fx-btn--teal" @click="save(a)">
              <i :class="savingCode === a.actionCode ? 'pi pi-spin pi-spinner' : 'pi pi-check'"></i>{{ tr('Save') }}
            </button>
          </fieldset>
        </div>
      </section>
    </template>
  </div>
</template>

<style scoped>
.pa { display: flex; flex-direction: column; gap: 16px; }
.pa-state { display: flex; align-items: center; gap: 10px; color: var(--cap-muted); }
.pa-facts { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 14px 18px; }
.pa-facts span { display: block; color: var(--cap-muted); font-size: 11px; }
.pa-facts b { display: block; margin-top: 3px; color: var(--cap-heading); font-size: 14px; font-weight: 600; }
.pa-intro { margin: 0 0 14px; color: var(--cap-muted); font-size: 12px; }
.pa-row { display: grid; grid-template-columns: minmax(0, 1fr) minmax(0, 1.2fr); gap: 18px; padding: 14px 0; border-top: 1px solid var(--cap-border); }
.pa-what b { color: var(--cap-heading); font-size: 14px; }
.pa-code { margin-left: 8px; padding: 2px 6px; border-radius: 4px; background: var(--cap-bg); color: var(--cap-ink-soft); font-family: monospace; font-size: 11px; }
.pa-what p { margin: 6px 0 4px; color: var(--cap-ink-soft); font-size: 12px; }
.pa-what small { color: var(--cap-muted); font-size: 11px; }
.pa-link { display: flex; align-items: flex-end; gap: 14px; margin: 0; padding: 0; border: 0; }
.pa-field { flex: 1; min-width: 0; }
.pa-field label { display: block; margin-bottom: 6px; color: var(--cap-ink-soft); font-size: 11px; font-weight: 500; }
.pa-field select { width: 100%; height: 42px; box-sizing: border-box; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 8px; font: inherit; font-size: 13px; background: #fff; }
.pa-check { display: flex; align-items: center; gap: 6px; height: 42px; color: var(--cap-ink-soft); font-size: 12px; }
.pa-link .fx-chip { margin-bottom: 11px; }
@media (max-width: 900px) { .pa-facts { grid-template-columns: repeat(2, minmax(0, 1fr)); } .pa-row { grid-template-columns: 1fr; } .pa-link { flex-wrap: wrap; } }
</style>
