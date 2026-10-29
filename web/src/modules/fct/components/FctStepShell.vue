<template>
  <!-- The frame every FINOVA_FINANCEING factoring step page shares: title, Documents, Back to Task, the messages and the
       Application Summary. The step's own forms go in the slot. -->
  <div class="fx fc">
    <div class="fx-head fc-no-print">
      <div class="fc-title">
        <span class="fc-title-ic" aria-hidden="true"><span class="fa" :class="icon"></span></span>
        <div>
          <span class="fc-eyebrow">{{ tr('FACTORING') }}</span>
          <h1>{{ tr(title) }}</h1>
          <p>{{ tr(subtitle) }}</p>
        </div>
      </div>
      <div class="fx-actions">
        <slot name="actions"></slot>
        <FctAgreementDocuments :agreement-id="agreementId" @uploaded="emit('uploaded')" />
        <button type="button" class="fx-btn" @click="backToTask"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Task') }}</button>
      </div>
    </div>

    <div v-if="error" class="fc-alert fc-no-print" role="alert"><span class="fa fa-exclamation-circle" aria-hidden="true"></span>{{ error }}</div>
    <div v-if="success" class="fc-ok fc-no-print" role="status"><span class="fa fa-check-circle" aria-hidden="true"></span>{{ success }}</div>
    <div v-if="loading" class="fx-card fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr(loadingText) }}</div>

    <template v-else-if="summary">
      <section class="fx-card fc-step-card fc-no-print">
        <header class="fc-sec-head">
          <span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-briefcase"></span></span>
          <div><h2>{{ tr('Application Summary') }}</h2><p>{{ tr(summaryNote) }}</p></div>
          <span class="fx-chip fx-chip--teal">{{ summary.statusCode || 'DRAFT' }}</span>
        </header>
        <div class="fc-facts">
          <div class="fc-fact"><span class="fa fa-bookmark-o" aria-hidden="true"></span><div><span>{{ tr('Agreement No.') }}</span><b>{{ summary.agreementNumber || `#${summary.agreementId}` }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-user-o" aria-hidden="true"></span><div><span>{{ tr('Client') }}</span><b>{{ summary.clientName || `Client #${summary.clientId}` }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-money" aria-hidden="true"></span><div><span>{{ tr('Requested Amount') }}</span><b>{{ formatAmount(summary.facilityLimit, summary.currencyCode) }}</b></div></div>
          <div class="fc-fact"><span class="fa fa-calendar" aria-hidden="true"></span><div><span>{{ tr('Submitted') }}</span><b>{{ formatDate(summary.applicationDate) }}</b>
            <small>{{ submittedBy ?? summary.relationshipManager ?? '-' }}</small></div></div>
        </div>
      </section>
      <slot></slot>
    </template>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '../pages/factoring.css'
import { computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { tr } from '@/shared/layout/language'
import FctAgreementDocuments from './FctAgreementDocuments.vue'
import { formatAmount, formatDate } from './stepFormat'

defineProps<{
  agreementId: number
  title: string
  subtitle: string
  icon: string
  summaryNote: string
  loadingText: string
  summary: { agreementId: number; agreementNumber: string; clientId: number; clientName: string | null; facilityLimit: number;
    currencyCode: string | null; applicationDate: string | null; relationshipManager: string | null; statusCode: string | null } | null
  submittedBy?: string | null
  loading: boolean
  error: string
  success?: string
}>()

const emit = defineEmits<{ uploaded: [] }>()

const route = useRoute()
const router = useRouter()
const inboxId = computed(() => Number(route.query.inboxId))

function backToTask() {
  if (Number.isFinite(inboxId.value)) {
    router.push({ name: 'workflow-task', params: { inboxId: inboxId.value } })
    return
  }
  router.back()
}

defineExpose({ backToTask })
</script>
