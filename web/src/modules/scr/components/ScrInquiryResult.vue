<template>
  <!-- One inquiry's result: the verdict, then each Final List entry it matched and how. -->
  <section class="fx-card fc-step-card">
    <header class="fc-sec-head">
      <span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-file-text-o"></span></span>
      <div>
        <h2 dir="ltr">{{ inquiry.inquiryRef }}</h2>
        <p>{{ formatDateTime(inquiry.screenedAt) }} · {{ inquiry.requestedBy }} · {{ inquiry.companyCode }} · {{ tr('Final List') }} V{{ inquiry.registryVersionNo }}</p>
      </div>
      <RouterLink :to="{ name: 'scr-certificate', params: { inquiryId: inquiry.inquiryId } }" class="fx-btn fx-btn--sm"><span class="fa fa-certificate" aria-hidden="true"></span>{{ tr('Certificate') }}</RouterLink>
      <button type="button" class="fx-btn fx-btn--sm" @click="print"><span class="fa fa-print" aria-hidden="true"></span>{{ tr('Print') }}</button>
    </header>

    <div class="fc-facts">
      <div class="fc-fact"><span :class="['fa', inquiry.subjectTypeCode === 'PERSON' ? 'fa-user-o' : 'fa-building-o']" aria-hidden="true"></span>
        <div><span>{{ tr(LABELS[inquiry.subjectTypeCode] ?? inquiry.subjectTypeCode) }}</span><b>{{ inquiry.subjectName }}</b></div></div>
      <div class="fc-fact"><span class="fa fa-id-card-o" aria-hidden="true"></span>
        <div><span>{{ tr(LABELS[inquiry.identityTypeCode] ?? inquiry.identityTypeCode) }}</span><b dir="ltr">{{ inquiry.identityNo }}</b>
          <small v-if="inquiry.nationalityCode">{{ tr('Nationality') }} {{ inquiry.nationalityCode }}</small></div></div>
      <div class="fc-fact"><span class="fa fa-bullseye" aria-hidden="true"></span>
        <div><span>{{ tr('Purpose') }}</span><b>{{ inquiry.purposeText || tr(LABELS[inquiry.purposeCode] ?? inquiry.purposeCode) }}</b></div></div>
      <div class="fc-fact"><span class="fa fa-hashtag" aria-hidden="true"></span>
        <div><span>{{ tr('Case reference') }}</span><b dir="ltr">{{ inquiry.caseReference || '-' }}</b></div></div>
    </div>

    <div class="scr-verdict" :class="`scr-verdict--${inquiry.resultCode}`">
      <span :class="['fa', ICON[inquiry.resultCode] ?? 'fa-info-circle']" aria-hidden="true"></span>
      <div>
        <b>{{ tr(LABELS[inquiry.resultCode] ?? inquiry.resultCode) }}</b>
        <small>{{ tr(ADVICE[inquiry.resultCode] ?? '') }}</small>
      </div>
    </div>

    <div v-for="(h, i) in inquiry.hits" :key="i" class="scr-hit" :class="{ 'scr-hit--listed': h.entryStatusCode !== 'LIFTED' }">
      <div class="scr-hit-head">
        <b>{{ h.nameAr }}</b><span v-if="h.nameEn" class="fc-muted">{{ h.nameEn }}</span>
        <span class="fx-chip" :class="CHIP[h.entryStatusCode]">{{ tr(LABELS[h.entryStatusCode] ?? h.entryStatusCode) }}</span>
        <span class="fx-chip fx-chip--info fx-chip--plain">{{ tr(LABELS[h.matchBasisCode] ?? h.matchBasisCode) }}</span>
        <span class="fx-chip fx-chip--plain">{{ tr('Name match') }} {{ h.nameScorePct }}%</span>
      </div>
      <div class="fc-facts">
        <div class="fc-fact"><span class="fa fa-id-card-o" aria-hidden="true"></span><div><span>{{ tr(LABELS[h.identityTypeCode] ?? h.identityTypeCode) }}</span><b dir="ltr">{{ h.identityNo }}</b><small>{{ h.nationalityCode }}</small></div></div>
        <div class="fc-fact"><span class="fa fa-gavel" aria-hidden="true"></span><div><span>{{ tr('Decision') }}</span><b dir="ltr">{{ h.decisionRef }}</b><small dir="ltr">{{ h.officialNo }}</small></div></div>
        <div class="fc-fact"><span class="fa fa-calendar" aria-hidden="true"></span><div><span>{{ tr('Effective date') }}</span><b>{{ formatDate(h.listedFrom) }}</b>
          <small v-if="h.liftedOn">{{ tr('Lifted') }} {{ formatDate(h.liftedOn) }}</small></div></div>
        <div class="fc-fact"><span class="fa fa-balance-scale" aria-hidden="true"></span><div><span>{{ tr('Scope') }}</span><b>{{ h.scopeText }}</b></div></div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { formatDate, formatDateTime } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import { CHIP, LABELS, type ScrInquiry } from '../api/scr.api'

defineProps<{ inquiry: ScrInquiry }>()
const ICON: Record<string, string> = { NO_MATCH: 'fa-check-circle', POSSIBLE_MATCH: 'fa-exclamation-triangle', MATCH: 'fa-ban', PREVIOUSLY_LISTED: 'fa-history' }
const ADVICE: Record<string, string> = {
  NO_MATCH: 'Not on the Federation Final List.',
  POSSIBLE_MATCH: 'The name matches a listed entry but the identity does not: review before going on.',
  MATCH: "Listed on the Federation Final List: apply the decision's scope.",
  PREVIOUSLY_LISTED: 'Only lifted entries match: no measure is in force.'
}
const print = () => window.print()
</script>
