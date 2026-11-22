<template>
  <!-- One inquiry's result: the verdict, then each Final List entry it matched and how. -->
  <section class="fx-card fc-step-card">
    <header class="fc-sec-head">
      <span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-file-text-o"></span></span>
      <div>
        <h2 dir="ltr">{{ inquiry.inquiryRef }}</h2>
        <p>{{ formatDateTime(inquiry.screenedAt) }} · {{ inquiry.requestedBy }} · {{ inquiry.companyCode }} · {{ tr('Final List') }} V{{ inquiry.registryVersionNo }}<template v-if="inquiry.nameMatchMinPct"> · {{ tr('ID') }} <bdi>{{ inquiry.idMatchMinDigits ?? 14 }}/14</bdi> · {{ tr('Name') }} <bdi>≥ {{ inquiry.nameMatchMinPct }}%</bdi></template></p>
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

    <div class="scr-verdict" :class="inquiry.hits.length ? 'scr-verdict--FOUND' : 'scr-verdict--NOT_FOUND'">
      <span :class="['fa', inquiry.hits.length ? 'fa-list-ul' : 'fa-check-circle']" aria-hidden="true"></span>
      <div>
        <b>{{ foundText(inquiry) }}</b>
        <small>{{ inquiry.hits.length
          ? tr("Compare each point below with the customer's documents. The decision is your company's.")
          : tr('No Final List entry reaches the identity and name levels chosen.') }}</small>
      </div>
    </div>

    <div v-for="(h, i) in inquiry.hits" :key="i" class="scr-hit">
      <div class="scr-hit-head">
        <span class="fc-muted">{{ tr('Final List entry') }} {{ i + 1 }}</span>
        <b>{{ h.nameAr }}</b><span v-if="h.nameEn" class="fc-muted">{{ h.nameEn }}</span>
        <span class="fx-chip" :class="CHIP_STATUS[h.entryStatusCode]">{{ tr(LABELS[h.entryStatusCode] ?? h.entryStatusCode) }}</span>
        <span class="scr-hit-decision fc-muted"><span dir="ltr">{{ h.decisionRef }}</span> (<span dir="ltr">{{ h.officialNo }}</span>) · {{ formatDate(h.listedFrom) }}
          <template v-if="h.liftedOn"> · {{ tr('Lifted') }} {{ formatDate(h.liftedOn) }}</template></span>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table scr-checks">
          <thead><tr><th>{{ tr('Point') }}</th><th>{{ tr('You entered') }}</th><th>{{ tr('On the Final List') }}</th><th>{{ tr('Level of match') }}</th></tr></thead>
          <tbody>
            <tr v-for="c in checkRows(inquiry, h, tr(LABELS[inquiry.identityTypeCode] ?? 'Identity number'))" :key="c.label">
              <td><b>{{ c.label }}</b></td>
              <td><bdi>{{ c.mine }}</bdi></td>
              <td><bdi>{{ c.listed }}</bdi></td>
              <td><span class="scr-mark" :class="`scr-mark--${c.level.mark}`"><span :class="['fa', MARK_ICON[c.level.mark]]" aria-hidden="true"></span><bdi>{{ c.level.text }}</bdi></span></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="scr-hit-scope"><span class="fa fa-balance-scale" aria-hidden="true"></span><span>{{ tr('Scope') }}:</span> <b>{{ h.scopeText }}</b></div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { formatDate, formatDateTime } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import { LABELS, type ScrInquiry } from '../api/scr.api'
import { MARK_ICON, checkRows, foundText } from '../utils/levels'

defineProps<{ inquiry: ScrInquiry }>()
/** The entry's status on the list, a fact: active and suspended entries in force, lifted ones history. */
const CHIP_STATUS: Record<string, string> = { ACTIVE: 'fx-chip--bad', SUSPENDED: 'fx-chip--warn', LIFTED: 'fx-chip--teal' }
const print = () => window.print()
</script>
