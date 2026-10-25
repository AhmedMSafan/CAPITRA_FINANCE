<script setup lang="ts">
// Payment history (APEX App 105 pages 206/207): the instalments of one account of a portfolio.
import { computed, ref, shallowRef, watch } from 'vue'
import { useFeedback } from '@/shared/feedback/useFeedback'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudColumn, CrudField, FieldOption, FormValues } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { getLookupOptions } from '@/modules/adm/api/lookups.api'
import {
  accountOptions,
  accountPaymentsApi,
  portfolioAccountsApi,
  portfoliosApi,
  type AccountPayment,
  type Portfolio
} from '../api/portfolios.api'

const props = defineProps<{ portfolioId: string }>()
const canEdit = hasPermission('MTG.PORTFOLIO.EDIT')

const portfolio = shallowRef<Portfolio | null>(null)
const accounts = shallowRef<FieldOption[]>([])
const statusOptions = shallowRef<FieldOption[]>([])
const selectedAccount = ref<number | null>(null)
const error = ref('')

const id = computed(() => Number(props.portfolioId))

watch(id, async () => {
  error.value = ''
  portfolio.value = null
  try {
    const [p, list, statuses] = await Promise.all([
      portfoliosApi.get(id.value),
      portfolioAccountsApi(id.value).list(),
      getLookupOptions('payment-status')
    ])
    portfolio.value = p
    accounts.value = accountOptions(list)
    statusOptions.value = statuses
    // APEX page 206 opens on the first account of the portfolio.
    selectedAccount.value = list[0]?.portfolioAccountId ?? null
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Unable to load the portfolio.'
  }
}, { immediate: true })

const paymentsApi = computed(() => accountPaymentsApi(selectedAccount.value))

const columns: CrudColumn<AccountPayment>[] = [
  { field: 'dueDate', label: 'Due date', type: 'date' },
  { field: 'dueAmount', label: 'Due amount', type: 'amount' },
  { field: 'paymentDate', label: 'Payment date', type: 'date' },
  { field: 'paidAmount', label: 'Paid amount', type: 'amount' },
  { field: 'paymentStatusName', label: 'Status' },
  { field: 'daysLate', label: 'Days late', type: 'number' },
  { field: 'chequeNo', label: 'Cheque no.' },
  { field: 'bankName', label: 'Bank' }
]

const fields = computed<CrudField[]>(() => [
  { name: 'dueDate', label: 'Due Date', type: 'date', required: true },
  { name: 'dueAmount', label: 'Due Amount', type: 'amount', required: true, min: 0 },
  { name: 'paymentDate', label: 'Payment Date', type: 'date' },
  { name: 'paidAmount', label: 'Paid Amount', type: 'amount', required: true, min: 0 },
  { name: 'paymentStatusCode', label: 'Payment Status', type: 'select', required: true, options: statusOptions.value },
  { name: 'daysLate', label: 'Days Late', type: 'number', required: true, min: 0 },
  { name: 'chequeNo', label: 'Cheque No.', type: 'text', maxLength: 100 },
  { name: 'bankName', label: 'Bank Name', type: 'text', maxLength: 300 }
])

function newPayment(): FormValues {
  return { paidAmount: 0, daysLate: 0 }
}

// Errors open the app-wide pop-up.
useFeedback({ errors: [error] })
</script>

<template>
  <div class="cp-page">
    <nav class="crumbs" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'mtg-portfolios' }"><i class="pi pi-arrow-left"></i> Portfolio cases</RouterLink>
      <template v-if="portfolio">
        <span>/</span>
        <RouterLink :to="{ name: 'mtg-portfolio', params: { portfolioId } }">{{ portfolio.portfolioNo }}</RouterLink>
        <span>/ Payment history</span>
      </template>
    </nav>

    <div v-if="error && !portfolio" class="cp-empty"><i class="pi pi-exclamation-circle"></i> This page could not be loaded.</div>
    <div v-else-if="!portfolio" class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div>

    <template v-else>
      <section class="cp-card picker">
        <div class="cp-field">
          <label for="account">Account</label>
          <select id="account" v-model="selectedAccount" :disabled="accounts.length === 0">
            <option v-if="accounts.length === 0" :value="null">No accounts in this portfolio</option>
            <option v-for="a in accounts" :key="a.value" :value="a.value">{{ a.label }}</option>
          </select>
        </div>
      </section>

      <div v-if="!selectedAccount" class="cp-empty">Add an account to the portfolio before recording payments.</div>
      <CrudPage
        v-else
        title="Payment history"
        description="Due and paid instalments of the selected account."
        icon="pi pi-calendar"
        apex-pages="206/207"
        entity-label="Payment"
        :columns="columns"
        :fields="fields"
        :api="paymentsApi"
        :row-key="row => row.accountPaymentId"
        :new-values="newPayment"
        :to-values="row => ({ ...row })"
        :can-edit="canEdit"
        :has-active="false"
        embedded
      />
    </template>
  </div>
</template>

<style scoped>
.crumbs { display: flex; gap: 8px; margin-bottom: 14px; color: var(--cap-muted); font-size: 12px; }
.crumbs a { color: var(--cap-primary-dark); text-decoration: none; }
.crumbs a:hover { text-decoration: underline; }
.picker { padding: 16px 20px; margin-bottom: 16px; max-width: 640px; }
</style>
