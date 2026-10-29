<template>
  <!-- APEX App 136 page 6 "Repayment Transactions": the transaction form with its sources and allocations. Update and the
       editable fields are off once the transaction is ALLOCATED, as the page's DisableEnableFields action does. -->
  <div class="fx fc">
    <div class="fx-head">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-file-text-o"></span></span>
        <div>
          <h1>{{ tr('Repayment Transactions') }}</h1>
          <p>{{ r ? r.repaymentNumber : tr('Loading...') }}</p>
        </div>
      </div>
      <div class="col-actions">
        <RouterLink class="fx-btn" :to="{ name: 'collection-repayments' }"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back') }}</RouterLink>
        <button v-if="r && canEdit && !locked" type="button" class="fx-btn fx-btn--teal" :disabled="saving" @click="save">
          <span class="fa" :class="saving ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr('Update') }}
        </button>
      </div>
    </div>

    <div v-if="r" class="col-stack">
      <section class="col-sec">
        <h2><span class="fa fa-file-text-o" aria-hidden="true"></span>{{ tr('Repayment Transaction Form') }}</h2>
        <div class="col-grid">
          <div class="fc-field"><span>{{ tr('Client ID') }}</span><div class="col-ro">{{ r.clientId }}</div></div>
          <div class="fc-field"><span>{{ tr('Name') }}</span><div class="col-ro">{{ r.clientName || '-' }}</div></div>
          <div class="fc-field"><span>{{ tr('Repayment Number') }}</span><div class="col-ro">{{ r.repaymentNumber }}</div></div>
          <div class="fc-field"><span>{{ tr('Receipt Amount') }}</span><div class="col-ro">{{ formatAmount(r.receiptAmount) }}</div></div>
          <div class="fc-field"><span>{{ tr('Status') }}</span><div class="col-ro"><span class="fx-chip" :class="statusChip(r.statusCode)">{{ r.statusCode }}</span></div></div>
          <div class="fc-field"><span>{{ tr('Repayment Method') }}</span><div class="col-ro">{{ r.repaymentMethod }}</div></div>
          <label class="fc-field" :class="{ 'col-bad': bad('transactionDate') }">
            <span>{{ tr('Transaction Date') }} <b>*</b></span>
            <input v-model="form.transactionDate" class="fx-search" type="date" :disabled="readOnly">
          </label>
          <label class="fc-field" :class="{ 'col-bad': bad('valueDate') }">
            <span>{{ tr('Value Date') }} <b>*</b></span>
            <input v-model="form.valueDate" class="fx-search" type="date" :disabled="readOnly">
          </label>
          <label class="fc-field">
            <span>{{ tr('Notes') }}</span>
            <input v-model="form.notes" class="fx-search" type="text" maxlength="2000" :readonly="readOnly">
          </label>
          <label class="fc-field">
            <span>{{ tr('Bank Statement Reference') }}</span>
            <input v-model="form.bankStatementReference" class="fx-search" type="text" maxlength="100" :readonly="readOnly">
          </label>
          <label class="fc-field">
            <span>{{ tr('External Reference') }}</span>
            <input v-model="form.externalReference" class="fx-search" type="text" maxlength="100" :readonly="readOnly">
          </label>
          <label class="fc-field">
            <span>{{ tr('Reversal Reason') }}</span>
            <input v-model="form.reversalReason" class="fx-search" type="text" maxlength="1000" :readonly="readOnly">
          </label>
        </div>
      </section>

      <section class="fx-card">
        <div class="fc-card-head"><h2>{{ tr('Repayment Sources') }} ({{ r.sources.length }})</h2></div>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead>
              <tr>
                <th>{{ tr('Cheque Id') }}</th>
                <th class="fx-r">{{ tr('Source Amount') }}</th>
                <th class="fx-r">{{ tr('Allocated Amount') }}</th>
                <th class="fx-r">{{ tr('Unallocated') }}</th>
                <th>{{ tr('Status Code') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="s in r.sources" :key="s.repaymentSourceId">
                <td>{{ s.chequeId ?? '' }}</td>
                <td class="fx-r">{{ formatAmount(s.sourceAmount) }}</td>
                <td class="fx-r">{{ formatAmount(s.allocatedAmount) }}</td>
                <td class="fx-r">{{ formatAmount(s.sourceAmount - s.allocatedAmount) }}</td>
                <td><span class="fx-chip" :class="statusChip(s.statusCode)">{{ s.statusCode }}</span></td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="!r.sources.length" class="fx-empty">{{ tr('No data found') }}</div>
      </section>

      <section class="fx-card">
        <div class="fc-card-head"><h2>{{ tr('Repayment Allocation') }} ({{ r.allocations.length }})</h2></div>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead>
              <tr>
                <th>{{ tr('Allocation Id') }}</th>
                <th>{{ tr('Repayment Source Id') }}</th>
                <th>{{ tr('Component Code') }}</th>
                <th>{{ tr('Disbursement Id') }}</th>
                <th class="fx-r">{{ tr('Allocation Amount') }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="a in r.allocations" :key="a.allocationId">
                <td>{{ a.allocationId }}</td>
                <td>{{ a.repaymentSourceId }}</td>
                <td>{{ a.componentCode }}</td>
                <td>{{ a.disbursementId }}</td>
                <td class="fx-r">{{ formatAmount(a.allocationAmount) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="!r.allocations.length" class="fx-empty">{{ tr('No data found') }}</div>
      </section>
    </div>
    <div v-else-if="loading" class="fx-card fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import './collection.css'
import { computed, reactive, ref, watch } from 'vue'
import { tr } from '@/shared/layout/language'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatAmount } from '@/shared/format/money'
import { collectionApi, type RepaymentForm, type RepaymentUpdate } from '../api/collection.api'
import { fieldErrors, messageOf, statusChip } from './collection'

const props = defineProps<{ repaymentTxnId: number }>()

const r = ref<RepaymentForm | null>(null)
const canEdit = hasPermission('COLLECTION.EDIT')
const loading = ref(false)
const saving = ref(false)
const errors = ref<Record<string, string[]>>({})
const bad = (field: string) => !!errors.value[field]

const form = reactive<RepaymentUpdate>({ transactionDate: '', valueDate: '', notes: null, bankStatementReference: null, externalReference: null, reversalReason: null })
const locked = computed(() => r.value?.statusCode === 'ALLOCATED')
const readOnly = computed(() => locked.value || !canEdit)

function fill(data: RepaymentForm) {
  r.value = data
  Object.assign(form, {
    transactionDate: data.transactionDate,
    valueDate: data.valueDate,
    notes: data.notes,
    bankStatementReference: data.bankStatementReference,
    externalReference: data.externalReference,
    reversalReason: data.reversalReason
  })
}

async function load() {
  loading.value = true
  try {
    fill(await collectionApi.repayment(props.repaymentTxnId))
  } catch (err) {
    showError(messageOf(err))
  } finally {
    loading.value = false
  }
}

async function save() {
  const e: Record<string, string[]> = {}
  if (!form.transactionDate) e.transactionDate = ['Transaction Date must have some value.']
  if (!form.valueDate) e.valueDate = ['Value Date must have some value.']
  errors.value = e
  if (Object.keys(e).length) {
    showError(Object.values(e).flat().map((m) => tr(m)).join('\n'))
    return
  }
  saving.value = true
  try {
    fill(await collectionApi.updateRepayment(props.repaymentTxnId, { ...form }))
    showSuccess(tr('Action Processed.'))
  } catch (err) {
    errors.value = fieldErrors(err) ?? {}
    showError(messageOf(err))
  } finally {
    saving.value = false
  }
}

watch(() => props.repaymentTxnId, load, { immediate: true })
</script>
