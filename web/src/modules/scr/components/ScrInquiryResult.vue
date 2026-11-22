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
        <b>{{ tr(VERDICT[inquiry.resultCode] ?? inquiry.resultCode) }}</b>
        <small>{{ tr(ADVICE[inquiry.resultCode] ?? '') }}</small>
      </div>
    </div>

    <div v-for="(h, i) in inquiry.hits" :key="i" class="scr-hit" :class="{ 'scr-hit--listed': h.entryStatusCode !== 'LIFTED' }">
      <div class="scr-hit-head">
        <span class="fc-muted">{{ tr('Final List entry') }}</span>
        <b>{{ h.nameAr }}</b><span v-if="h.nameEn" class="fc-muted">{{ h.nameEn }}</span>
        <span class="fx-chip" :class="CHIP[h.entryStatusCode]">{{ tr(LABELS[h.entryStatusCode] ?? h.entryStatusCode) }}</span>
        <span class="scr-hit-decision fc-muted"><span dir="ltr">{{ h.decisionRef }}</span> (<span dir="ltr">{{ h.officialNo }}</span>) · {{ formatDate(h.listedFrom) }}
          <template v-if="h.liftedOn"> · {{ tr('Lifted') }} {{ formatDate(h.liftedOn) }}</template></span>
      </div>
      <div class="fx-table-wrap">
        <table class="fx-table scr-checks">
          <thead><tr><th>{{ tr('Check') }}</th><th>{{ tr('You entered') }}</th><th>{{ tr('On the Final List') }}</th><th>{{ tr('Result') }}</th></tr></thead>
          <tbody>
            <tr v-for="c in checks(h)" :key="c.label">
              <td><b>{{ tr(c.label) }}</b></td>
              <td><bdi>{{ c.mine }}</bdi></td>
              <td><bdi>{{ c.listed }}</bdi></td>
              <td><span class="scr-mark" :class="`scr-mark--${c.mark}`"><span :class="['fa', MARK[c.mark]]" aria-hidden="true"></span>{{ tr(c.verdict) }}</span>
                <span v-if="c.extra" class="fc-muted"> ({{ c.extra }})</span></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="h.matchBasisCode === 'ID_ONLY'" class="fc-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
        <span>{{ tr('Listed because the identity number is the same. The name is different: check that the identity number was typed right.') }}</span></div>
      <div class="scr-hit-scope"><span class="fa fa-balance-scale" aria-hidden="true"></span><span>{{ tr('Scope') }}:</span> <b>{{ h.scopeText }}</b></div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { formatDate, formatDateTime } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import { CHIP, LABELS, VERDICT, type ScrInquiry, type ScrInquiryHit } from '../api/scr.api'

const props = defineProps<{ inquiry: ScrInquiry }>()
const ICON: Record<string, string> = { NO_MATCH: 'fa-check-circle', POSSIBLE_MATCH: 'fa-exclamation-triangle', MATCH: 'fa-ban', PREVIOUSLY_LISTED: 'fa-history' }
const ADVICE: Record<string, string> = {
  NO_MATCH: 'Nobody on the Federation Final List has this identity number or a similar name.',
  POSSIBLE_MATCH: "The name is close to a listed entry but the identity number is different. Check the customer's documents before deciding.",
  MATCH: "This identity number is on the Federation Final List. Apply the scope of the decision below.",
  PREVIOUSLY_LISTED: 'Only lifted entries match: no measure applies today.'
}
const MARK: Record<string, string> = { yes: 'fa-check', no: 'fa-times', none: 'fa-minus' }

interface Check { label: string; mine: string; listed: string; mark: 'yes' | 'no' | 'none'; verdict: string; extra?: string }

/** Each point checked for one Final List entry: what was entered, what the list holds, and whether they agree. */
function checks(h: ScrInquiryHit): Check[] {
  const q = props.inquiry
  const idSame = h.matchBasisCode !== 'NAME_ONLY'
  const nameSame = h.matchBasisCode !== 'ID_ONLY'
  const score = Math.round(h.nameScorePct)
  const out: Check[] = [
    { label: LABELS[q.identityTypeCode] ?? 'Identity number', mine: q.identityNo, listed: h.identityNo ?? '-', mark: idSame ? 'yes' : 'no', verdict: idSame ? 'Same' : 'Different' },
    { label: 'Name', mine: q.subjectName, listed: [h.nameAr, h.nameEn].filter(Boolean).join(' · '), mark: nameSame ? 'yes' : 'no',
      verdict: !nameSame ? 'Different' : score >= 100 ? 'Same' : 'Similar', extra: `${score}% ${tr('alike')}` }
  ]
  if (q.subjectTypeCode === 'PERSON') {
    out.push(q.birthDate
      ? { label: 'Birth date', mine: formatDate(q.birthDate), listed: '-', mark: h.matchBasisCode === 'NAME_ONLY' ? 'yes' : 'none',
          verdict: h.matchBasisCode === 'NAME_ONLY' ? 'No conflict' : 'Not compared' }
      : { label: 'Birth date', mine: tr('Not given'), listed: '-', mark: 'none', verdict: 'Not given' })
  }
  out.push(!q.nationalityCode || !h.nationalityCode
    ? { label: 'Nationality', mine: q.nationalityCode || tr('Not given'), listed: h.nationalityCode || '-', mark: 'none', verdict: 'Not given' }
    : { label: 'Nationality', mine: q.nationalityCode, listed: h.nationalityCode, mark: q.nationalityCode === h.nationalityCode ? 'yes' : 'no',
        verdict: q.nationalityCode === h.nationalityCode ? 'Same' : 'Different' })
  return out
}
const print = () => window.print()
</script>
