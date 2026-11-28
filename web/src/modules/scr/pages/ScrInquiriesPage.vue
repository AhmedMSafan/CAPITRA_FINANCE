<template>
  <!-- Inquiry audit (federation): every member company's screenings and their results. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Inquiry audit') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-search"></span></span>
          <div>
            <h1>{{ tr('Inquiry audit') }}</h1>
            <p>{{ tr('Every screening by every member company, with its result.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openScreeningManual(12)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
      </div>
    </div>

    <div class="fc-indicators">
      <div class="fx-card fc-indicator fc-indicator--orange"><b>{{ found }}</b><span>{{ tr('Entries found') }}</span><small>{{ tr('At least one Final List entry shown') }}</small></div>
      <div class="fx-card fc-indicator"><b>{{ scoped.length - found }}</b><span>{{ tr('Not found') }}</span><small>{{ tr('Nothing close on the Final List') }}</small></div>
      <div class="fx-card fc-indicator"><b>{{ scoped.length }}</b><span>{{ tr('Inquiries') }}</span><small>{{ tr('Every member company, portal and API') }}</small></div>
    </div>

    <div class="fx-card fx-card-pad fc-filters fc-step-card">
      <label class="fc-field"><span>{{ tr('Search') }}</span>
        <input v-model="search" class="fx-search" :placeholder="tr('Search by inquiry, name, identity or user')">
      </label>
      <ScrUserFilter v-model="who" :me="me" :users="users" />
      <label class="fc-field"><span>{{ tr('Result') }}</span>
        <select v-model="result" class="fx-select" @change="load">
          <option value="">{{ tr('All results') }}</option>
          <option value="FOUND">{{ tr('Entries found') }}</option>
          <option value="NOT_FOUND">{{ tr('Not found') }}</option>
        </select>
      </label>
    </div>

    <div class="fx-card fc-step-card">
      <div class="fc-card-head"><h2>{{ tr('Inquiries') }} ({{ rows.length }})</h2></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Inquiry') }}</th><th>{{ tr('Company') }}</th><th>{{ tr('Name') }}</th><th>{{ tr('Identity number') }}</th>
              <th>{{ tr('Purpose') }}</th><th>{{ tr('Result') }}</th><th>{{ tr('Date') }}</th><th class="fx-r">{{ tr('Actions') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="i in rows" :key="i.inquiryId" :class="{ 'is-overdue': i.hits.length > 0 }">
              <td dir="ltr"><b class="fc-ref">{{ i.inquiryRef }}</b></td>
              <td dir="ltr">{{ i.companyCode }}<span class="fx-cell-sub">{{ i.requestedBy }}</span></td>
              <td>{{ i.subjectName }}<span class="fx-cell-sub">{{ tr(LABELS[i.subjectTypeCode] ?? i.subjectTypeCode) }}</span></td>
              <td dir="ltr">{{ i.identityNo }}</td>
              <td>{{ tr(LABELS[i.purposeCode] ?? i.purposeCode) }}</td>
              <td><span class="fx-chip" :class="i.hits.length ? 'fx-chip--warn' : 'fx-chip--teal'">{{ foundLabel(i) }}</span>
                  <span v-if="i.hits[0]" class="scr-basis">
                    <span :class="`scr-basis--${idLevel(i.hits[0]).mark}`">{{ tr('ID') }} <bdi>{{ idLevel(i.hits[0]).text.split(' · ')[0] }}</bdi></span>
                    <span :class="`scr-basis--${nameLevel(i.hits[0]).mark}`">{{ tr('Name') }} <bdi>{{ nameLevel(i.hits[0]).text.split(' · ')[0] }}</bdi></span>
                  </span></td>
              <td>{{ formatDateTime(i.screenedAt) }}</td>
              <td class="fx-r"><button type="button" class="fx-btn fx-btn--sm" @click="open = i"><span class="fa fa-eye" aria-hidden="true"></span>{{ tr('View') }}</button></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="!rows.length" class="fx-empty"><span class="fa fa-search" aria-hidden="true"></span>{{ inquiries.length ? tr('Nothing here for the chosen user. Choose All users to see everything.') : tr('No inquiries.') }}</div>
    </div>

    <ScrInquiryResult v-if="open" :inquiry="open" />
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, nextTick, onMounted, ref, watch } from 'vue'
import { showError } from '@/shared/feedback/feedback'
import { formatDateTime } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import ScrInquiryResult from '../components/ScrInquiryResult.vue'
import { LABELS, errorText, scrApi, type ScrInquiry } from '../api/scr.api'
import { useUserFilter } from '../utils/userFilter'
import ScrUserFilter from '../components/ScrUserFilter.vue'
import { openScreeningManual } from '@/modules/scr/utils/manual'
import { foundLabel, idLevel, nameLevel } from '../utils/levels'

const inquiries = ref<ScrInquiry[]>([])
const open = ref<ScrInquiry | null>(null)
const search = ref('')
const result = ref('')
/** Requested by me by default (Admin: all users). */
const { who, me, users, scoped } = useUserFilter(inquiries, (i) => i.requestedBy)
const found = computed(() => scoped.value.filter((i) => i.hits.length > 0).length)
const rows = computed(() => {
  const q = search.value.trim().toLowerCase()
  return scoped.value.filter((i) => !q || `${i.inquiryRef} ${i.subjectName} ${i.identityNo} ${i.requestedBy} ${i.companyCode}`.toLowerCase().includes(q))
})

watch(open, async (v) => {
  if (!v) return
  await nextTick()
  window.scrollTo({ top: document.body.scrollHeight, behavior: 'smooth' })
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
