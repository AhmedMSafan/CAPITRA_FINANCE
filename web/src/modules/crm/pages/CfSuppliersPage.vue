<template>
  <!-- CRM > Consumer Finance Suppliers (Ahmed, 25 Nov 2026): every supplier application and where it is in its approval workflow. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink to="/">{{ tr('Home') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Consumer Finance Suppliers') }}</b>
        </nav>
        <h1>{{ tr('Consumer Finance Suppliers') }}</h1>
        <p class="c3-muted">{{ tr('Merchants that sell goods on consumer finance. A supplier is active only after its approval workflow.') }}</p>
      </div>
      <div class="fx-actions">
        <RouterLink v-if="canEdit" :to="{ name: 'crm-cf-supplier-new' }" class="fx-btn fx-btn--teal"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New supplier application') }}</RouterLink>
      </div>
    </div>

    <section class="fx-card">
      <div class="scr-toolbar">
        <input v-model="search" class="fx-search" :placeholder="tr('Search by reference, customer number or name')">
        <select v-model="status" class="fx-select" @change="load">
          <option value="">{{ tr('All statuses') }}</option>
          <option v-for="(label, code) in CF_STATUS" :key="code" :value="code">{{ tr(label) }}</option>
        </select>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Application') }}</th><th>{{ tr('Supplier') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Waiting at') }}</th><th>{{ tr('Risk rating') }}</th><th>{{ tr('Entered by') }}</th><th>{{ tr('Created') }}</th></tr></thead>
          <tbody>
            <tr v-for="a in shown" :key="a.applicationId" class="cf-row" @click="router.push({ name: 'crm-cf-supplier', params: { applicationId: a.applicationId } })">
              <td><b dir="ltr">{{ a.applicationRef }}</b></td>
              <td><b>{{ a.clientName }}</b><span class="fx-cell-sub">{{ a.clientNumber }}</span></td>
              <td><span class="fx-chip" :class="CF_CHIP[a.statusCode]">{{ tr(CF_STATUS[a.statusCode] ?? a.statusCode) }}</span></td>
              <td>{{ a.openStepName ? tr(a.openStepName) : '—' }}</td>
              <td>{{ a.riskRatingCode ? tr(RISK[a.riskRatingCode] ?? a.riskRatingCode) : '—' }}</td>
              <td>{{ a.enteredBy }}</td>
              <td>{{ formatDate(a.createdAt) }}</td>
            </tr>
          </tbody>
        </table>
        <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
        <div v-else-if="!shown.length" class="fx-empty"><span class="fa fa-handshake-o" aria-hidden="true"></span>{{ tr('No supplier applications yet.') }}</div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '@/modules/scr/components/scr.css'
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { showError } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { errorText } from '@/modules/scr/api/scr.api'
import { CF_CHIP, CF_STATUS, cfSuppliersApi, type CfSupplierListItem } from '../api/cfSuppliers.api'

const router = useRouter()
const canEdit = hasPermission('CRM.CF_SUPPLIER.EDIT')
const RISK: Record<string, string> = { LOW: 'Low', MEDIUM: 'Medium', HIGH: 'High' }
const rows = ref<CfSupplierListItem[]>([])
const loading = ref(false)
const status = ref('')
const search = ref('')

const shown = computed(() => {
  const s = search.value.trim().toUpperCase()
  return s ? rows.value.filter((a) => `${a.applicationRef} ${a.clientNumber} ${a.clientName}`.toUpperCase().includes(s)) : rows.value
})

async function load() {
  loading.value = true
  try {
    rows.value = await cfSuppliersApi.list(status.value || undefined)
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>

<style scoped>
.cf-row { cursor: pointer; }
</style>
