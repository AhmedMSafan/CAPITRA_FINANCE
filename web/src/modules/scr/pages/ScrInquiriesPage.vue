<template>
  <!-- Inquiry audit (federation): every member company's screenings and their results. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Inquiry audit') }}</b>
        </nav>
        <h1>{{ tr('Inquiry audit') }}</h1>
      </div>
    </div>
    <section class="fx-card">
      <div class="scr-filter">
        <input v-model="search" class="fx-search" :placeholder="tr('Search by inquiry, name, identity or user')">
        <select v-model="result" class="fx-select" @change="load">
          <option value="">{{ tr('All results') }}</option>
          <option v-for="r in ['MATCH', 'POSSIBLE_MATCH', 'PREVIOUSLY_LISTED', 'NO_MATCH']" :key="r" :value="r">{{ tr(LABELS[r] ?? r) }}</option>
        </select>
      </div>
      <table class="fx-table">
        <thead><tr><th>{{ tr('Inquiry') }}</th><th>{{ tr('Company') }}</th><th>{{ tr('Name') }}</th><th>{{ tr('Identity number') }}</th><th>{{ tr('Purpose') }}</th><th>{{ tr('Result') }}</th><th>{{ tr('Date') }}</th></tr></thead>
        <tbody>
          <tr v-for="i in rows" :key="i.inquiryId" class="scr-row" @click="open = i">
            <td dir="ltr">{{ i.inquiryRef }}</td>
            <td dir="ltr">{{ i.companyCode }}<br><small class="scr-muted">{{ i.requestedBy }}</small></td>
            <td>{{ i.subjectName }}</td>
            <td dir="ltr">{{ i.identityNo }}</td>
            <td>{{ tr(LABELS[i.purposeCode] ?? i.purposeCode) }}</td>
            <td><span class="fx-chip" :class="CHIP[i.resultCode]">{{ tr(LABELS[i.resultCode] ?? i.resultCode) }}</span></td>
            <td>{{ formatDateTime(i.screenedAt) }}</td>
          </tr>
        </tbody>
      </table>
      <div v-if="!rows.length" class="fx-empty">{{ tr('No inquiries.') }}</div>
    </section>
    <ScrInquiryResult v-if="open" :inquiry="open" />
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, ref } from 'vue'
import { showError } from '@/shared/feedback/feedback'
import { formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import ScrInquiryResult from '../components/ScrInquiryResult.vue'
import { CHIP, LABELS, errorText, scrApi, type ScrInquiry } from '../api/scr.api'

const inquiries = ref<ScrInquiry[]>([])
const open = ref<ScrInquiry | null>(null)
const search = ref('')
const result = ref('')
const rows = computed(() => {
  const q = search.value.trim().toLowerCase()
  return inquiries.value.filter((i) => !q || `${i.inquiryRef} ${i.subjectName} ${i.identityNo} ${i.requestedBy} ${i.companyCode}`.toLowerCase().includes(q))
})

async function load() {
  try {
    inquiries.value = await scrApi.allInquiries(null, result.value)
  } catch (err) {
    showError(errorText(err))
  }
}

onMounted(load)
</script>
