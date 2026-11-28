<template>
  <!-- APEX App 136 page 7 "New Bank Transfer": Save Transfer runs save_repayment with method BANK_TRANSFER, then the new
       transaction opens on page 6. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-exchange"></span></span>
        <div>
          <h1>{{ tr('New Bank Transfer') }}</h1>
          <p>{{ tr('Bank Transfer Repayment Transaction') }}</p>
        </div>
      </div>
      <div class="col-actions">
        <RouterLink class="fx-btn" :to="{ name: 'collection-repayments' }"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Cancel') }}</RouterLink>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="saving" @click="save">
          <span class="fa" :class="saving ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr('Save Transfer') }}
        </button>
      </div>
    </div>

    <section class="col-sec">
      <h2><span class="fa fa-university" aria-hidden="true"></span>{{ tr('Bank Transfer Repayment Transaction') }}</h2>
      <div class="col-grid">
        <label class="fc-field" :class="{ 'col-bad': bad('clientId') }">
          <span>{{ tr('Customer ID') }} <b>*</b></span>
          <select v-model="form.clientId" class="fx-select">
            <option :value="null">{{ tr('- Select -') }}</option>
            <option v-for="c in customers" :key="c.value" :value="Number(c.value)">{{ c.label }}</option>
          </select>
        </label>
        <label class="fc-field" :class="{ 'col-bad': bad('receiptAmount') }">
          <span>{{ tr('Receipt Amount') }} <b>*</b></span>
          <input v-model.number="form.receiptAmount" class="fx-search" type="number" min="0" step="0.01" inputmode="decimal">
        </label>
        <div class="fc-field">
          <span>{{ tr('Repayment Number') }}</span>
          <div class="col-ro">-</div>
        </div>
        <label class="fc-field" :class="{ 'col-bad': bad('transactionDate') }">
          <span>{{ tr('Transaction Date') }} <b>*</b></span>
          <input v-model="form.transactionDate" class="fx-search" type="date">
        </label>
        <label class="fc-field" :class="{ 'col-bad': bad('valueDate') }">
          <span>{{ tr('Value Date') }} <b>*</b></span>
          <input v-model="form.valueDate" class="fx-search" type="date">
        </label>
        <label class="fc-field">
          <span>{{ tr('Notes') }}</span>
          <input v-model="form.notes" class="fx-search" type="text" maxlength="2000">
        </label>
        <label class="fc-field">
          <span>{{ tr('Bank Statement Reference') }}</span>
          <input v-model="form.bankStatementReference" class="fx-search" type="text" maxlength="100">
        </label>
        <label class="fc-field">
          <span>{{ tr('External Reference') }}</span>
          <input v-model="form.externalReference" class="fx-search" type="text" maxlength="100">
        </label>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import './collection.css'
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { tr } from '@/shared/layout/language'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { todayInCairo } from '@/shared/format/datetime'
import { collectionApi, type BankTransfer, type Option } from '../api/collection.api'
import { fieldErrors, messageOf } from './collection'

const router = useRouter()
const customers = ref<Option[]>([])
const saving = ref(false)
const errors = ref<Record<string, string[]>>({})
const bad = (field: string) => !!errors.value[field]

const today = todayInCairo()
const form = reactive<BankTransfer>({
  clientId: null,
  receiptAmount: null,
  transactionDate: today,
  valueDate: today,
  notes: null,
  bankStatementReference: null,
  externalReference: null
})

// the page's required items, checked before the save as APEX does
function check(): boolean {
  const e: Record<string, string[]> = {}
  if (form.clientId == null) e.clientId = ['Customer ID must have some value.']
  if (form.receiptAmount == null || (form.receiptAmount as unknown) === '') e.receiptAmount = ['Receipt Amount must have some value.']
  if (!form.transactionDate) e.transactionDate = ['Transaction Date must have some value.']
  if (!form.valueDate) e.valueDate = ['Value Date must have some value.']
  errors.value = e
  if (Object.keys(e).length) showError(Object.values(e).flat().map((m) => tr(m)).join('\n'))
  return !Object.keys(e).length
}

async function save() {
  if (!check()) return
  saving.value = true
  try {
    const saved = await collectionApi.saveBankTransfer({ ...form, receiptAmount: Number(form.receiptAmount) })
    showSuccess(saved.message)
    await router.push({ name: 'collection-repayment', params: { repaymentTxnId: saved.repaymentTxnId } })
  } catch (err) {
    errors.value = fieldErrors(err) ?? {}
    showError(messageOf(err))
  } finally {
    saving.value = false
  }
}

onMounted(async () => {
  try {
    customers.value = await collectionApi.customers()
  } catch (err) {
    showError(messageOf(err))
  }
})
</script>
