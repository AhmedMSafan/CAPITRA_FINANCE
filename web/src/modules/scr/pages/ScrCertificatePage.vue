<template>
  <!-- A screening certificate: what was screened, when, against which Final List version, the result, and the inquiry's seal (checked
       when the page opens). Printed or saved as PDF from the print dialog. -->
  <div class="fx fc scr-invoice-page">
    <div class="fx-head scr-no-print">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b dir="ltr">{{ inquiry?.inquiryRef }}</b>
        </nav>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="router.back()"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" @click="print"><span class="fa fa-print" aria-hidden="true"></span>{{ tr('Print / PDF') }}</button>
      </div>
    </div>

    <article v-if="inquiry" class="fx-card scr-inv-doc scr-cert">
      <header class="scr-inv-head">
        <div>
          <div class="scr-inv-brand">CAPITRA</div>
          <small>{{ tr('Screening service') }}</small>
        </div>
        <div class="scr-inv-title">
          <h1>{{ tr('Screening certificate') }}</h1>
          <b dir="ltr">{{ inquiry.inquiryRef }}</b>
        </div>
      </header>

      <p class="scr-cert-lead">{{ tr('This certifies that the subject below was screened against the Federation Final List, with the result shown.') }}</p>

      <section class="scr-inv-meta">
        <div><span>{{ tr('Member company') }}</span><b>{{ inquiry.companyNameAr ?? inquiry.companyCode }}</b><small>{{ inquiry.companyNameEn }} · <bdi dir="ltr">{{ inquiry.companyCode }}</bdi></small></div>
        <div><span>{{ tr('Screened') }}</span><b>{{ formatDateTime(inquiry.screenedAt) }}</b><small>{{ tr(inquiry.channelCode === 'API' ? 'Through the API' : 'Screening Portal') }} · <bdi dir="ltr">{{ inquiry.requestedBy }}</bdi></small></div>
        <div><span>{{ tr('Final List version') }}</span><b>V{{ inquiry.registryVersionNo }}</b><small>{{ tr('the list in force when screened') }}</small></div>
        <div><span>{{ tr('Purpose') }}</span><b>{{ inquiry.purposeText || tr(LABELS[inquiry.purposeCode] ?? inquiry.purposeCode) }}</b><small v-if="inquiry.caseReference">{{ tr('Case reference') }} <bdi dir="ltr">{{ inquiry.caseReference }}</bdi></small></div>
      </section>

      <section class="scr-cert-subject">
        <h2>{{ tr('Subject') }}</h2>
        <dl>
          <div><dt>{{ tr('Type') }}</dt><dd>{{ tr(LABELS[inquiry.subjectTypeCode] ?? inquiry.subjectTypeCode) }}</dd></div>
          <div><dt>{{ tr('Full name') }}</dt><dd>{{ inquiry.subjectName }}</dd></div>
          <div><dt>{{ tr(LABELS[inquiry.identityTypeCode] ?? inquiry.identityTypeCode) }}</dt><dd dir="ltr">{{ inquiry.identityNo }}</dd></div>
          <div v-if="inquiry.birthDate"><dt>{{ tr('Birth date') }}</dt><dd>{{ formatDate(inquiry.birthDate) }}</dd></div>
          <div v-if="inquiry.nameMatchMinPct"><dt>{{ tr('Name match at least') }}</dt><dd dir="ltr">{{ inquiry.nameMatchMinPct }}%</dd></div>
          <div v-if="inquiry.idMatchMinDigits"><dt>{{ tr('Identity characters that must match') }}</dt><dd dir="ltr">{{ inquiry.idMatchMinDigits }} / 14</dd></div>
          <div v-if="inquiry.nationalityCode"><dt>{{ tr('Nationality') }}</dt><dd>{{ inquiry.nationalityCode }}</dd></div>
        </dl>
      </section>

      <div class="scr-verdict" :class="inquiry.hits.length ? 'scr-verdict--FOUND' : 'scr-verdict--NOT_FOUND'">
        <span class="fa fa-shield" aria-hidden="true"></span>
        <div><b>{{ foundText(inquiry) }}</b>
          <small>{{ inquiry.hits.length ? tr('The level of match of each point is shown below. The decision is the member company\'s.') : tr('No Final List entry reaches the identity and name levels chosen.') }}</small></div>
      </div>

      <div v-if="inquiry.hits.length" class="fx-table-wrap scr-inv-pay-table">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Listed name') }}</th><th>{{ tr('Identity') }}</th><th>{{ tr('ID') }}</th><th>{{ tr('Name') }}</th>
            <th v-if="inquiry.subjectTypeCode === 'PERSON'">{{ tr('Birth date') }}</th><th>{{ tr('Nationality') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Decision') }}</th></tr></thead>
          <tbody>
            <tr v-for="(h, n) in inquiry.hits" :key="n">
              <td><b>{{ h.nameAr }}</b><span v-if="h.nameEn" class="fx-cell-sub">{{ h.nameEn }}</span></td>
              <td dir="ltr">{{ h.identityNo }}</td>
              <td><bdi>{{ idLevel(h).text }}</bdi></td>
              <td><bdi>{{ nameLevel(h).text }}</bdi></td>
              <td v-if="inquiry.subjectTypeCode === 'PERSON'">{{ birthLevel(inquiry, h).text }}</td>
              <td>{{ nationalityLevel(inquiry, h).text }}</td>
              <td><span class="fx-chip" :class="CHIP[h.entryStatusCode]">{{ tr(LABELS[h.entryStatusCode] ?? h.entryStatusCode) }}</span></td>
              <td dir="ltr">{{ h.officialNo }}<span class="fx-cell-sub">{{ h.decisionRef }}</span></td>
            </tr>
          </tbody>
        </table>
      </div>

      <section class="scr-cert-seal" :class="seal ? (seal.valid ? 'scr-cert-seal--ok' : seal.sealed ? 'scr-cert-seal--bad' : '') : ''">
        <span :class="['fa', seal?.valid ? 'fa-lock' : seal?.sealed ? 'fa-exclamation-triangle' : 'fa-unlock']" aria-hidden="true"></span>
        <div>
          <b>{{ seal ? tr(seal.message) : tr('Checking the seal...') }}</b>
          <small v-if="inquiry.recordHash">{{ tr('Seal (SHA-256)') }}: <code dir="ltr">{{ inquiry.recordHash }}</code></small>
          <small v-if="seal">{{ tr('Checked') }} {{ formatDateTime(seal.checkedAt) }}</small>
        </div>
      </section>

      <p class="scr-cert-foot">{{ tr('The inquiry is kept by the Federation and cannot be changed or deleted. Its seal is chained to the inquiries before it, so any change would show.') }}</p>
    </article>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDate, formatDateTime } from '../utils/dates'
import { CHIP, LABELS, errorText, scrApi, type ScrInquiry, type ScrInquirySeal } from '../api/scr.api'
import { birthLevel, foundText, idLevel, nameLevel, nationalityLevel } from '../utils/levels'

const props = defineProps<{ inquiryId: number }>()
const router = useRouter()
const inquiry = ref<ScrInquiry | null>(null)
const seal = ref<ScrInquirySeal | null>(null)

function print() {
  window.print()
}

onMounted(async () => {
  try {
    inquiry.value = await scrApi.inquiry(props.inquiryId)
    seal.value = await scrApi.inquirySeal(props.inquiryId)
  } catch (err) {
    showError(errorText(err))
  }
})
</script>
