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
        <button v-if="h.partyId" type="button" class="fx-btn fx-btn--sm scr-hit-docs-btn" :aria-expanded="!!open[h.partyId]" @click="toggle(h.partyId)">
          <span class="fa fa-file-image-o" aria-hidden="true"></span>{{ tr('Decision documents') }}
          <span :class="['fa', open[h.partyId] ? 'fa-chevron-up' : 'fa-chevron-down']" aria-hidden="true"></span></button>
      </div>
      <div v-if="h.partyId && open[h.partyId]" class="scr-hit-docs">
        <div v-if="!docs[h.partyId]" class="fc-muted"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span> {{ tr('Loading...') }}</div>
        <div v-else-if="!docs[h.partyId]!.length" class="fc-muted">{{ tr('No decision documents.') }}</div>
        <div v-for="d in docs[h.partyId] ?? []" v-else :key="d.decisionId" class="scr-hit-doc">
          <span class="fx-chip">{{ tr(LABELS[d.decisionTypeCode] ?? d.decisionTypeCode) }}</span>
          <span><b dir="ltr">{{ d.decisionRef }}</b> (<span dir="ltr">{{ d.officialNo }}</span>) · {{ formatDate(d.effectiveDate) }}
            <template v-if="d.letterNo"> · {{ tr('Letter') }} <span dir="ltr">{{ d.letterNo }}</span></template></span>
          <div class="scr-files">
            <button v-for="f in d.files" :key="f.letterFileId" type="button" class="fx-btn fx-btn--sm scr-file" @click="viewing = { partyId: h.partyId, decision: d, file: f }">
              <span :class="['fa', f.mimeType === 'application/pdf' ? 'fa-file-pdf-o' : 'fa-file-image-o']" aria-hidden="true"></span><span dir="auto">{{ f.fileName }}</span>
              <span class="scr-file-view"><span class="fa fa-eye" aria-hidden="true"></span>{{ tr('View') }}</span>
            </button>
            <span v-if="!d.files.length" class="fc-muted">{{ tr('No scan attached') }}</span>
          </div>
        </div>
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
    <ScrDocViewer v-if="viewing" :title="`${viewing.decision.decisionRef} (${viewing.decision.officialNo})`" :file-name="viewing.file.fileName"
                  :mime-type="viewing.file.mimeType" :subtitle="`${tr(LABELS[viewing.decision.decisionTypeCode] ?? viewing.decision.decisionTypeCode)} · ${formatDate(viewing.decision.effectiveDate)}`"
                  :load="() => scrApi.hitFile(inquiry.inquiryId, viewing!.partyId, viewing!.file.letterFileId)" @close="viewing = null" />
  </section>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { formatDate, formatDateTime } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import { showError } from '@/shared/feedback/feedback'
import ScrDocViewer from './ScrDocViewer.vue'
import { LABELS, errorText, scrApi, type ScrHitDecision, type ScrInquiry } from '../api/scr.api'
import { MARK_ICON, checkRows, foundText } from '../utils/levels'

const props = defineProps<{ inquiry: ScrInquiry }>()

/** The decisions behind each entry (its listing and any later decision on it), read when the user opens them. */
const open = reactive<Record<number, boolean>>({})
const docs = reactive<Record<number, ScrHitDecision[] | undefined>>({})
const viewing = ref<{ partyId: number; decision: ScrHitDecision; file: ScrHitDecision['files'][number] } | null>(null)
watch(() => props.inquiry.inquiryId, () => {
  for (const k of Object.keys(open)) delete open[Number(k)]
  for (const k of Object.keys(docs)) delete docs[Number(k)]
  viewing.value = null
})
async function toggle(partyId: number) {
  open[partyId] = !open[partyId]
  if (!open[partyId] || docs[partyId]) return
  try {
    docs[partyId] = await scrApi.hitDecisions(props.inquiry.inquiryId, partyId)
  } catch (err) {
    open[partyId] = false
    showError(errorText(err))
  }
}
/** The entry's status on the list, a fact: active and suspended entries in force, lifted ones history. */
const CHIP_STATUS: Record<string, string> = { ACTIVE: 'fx-chip--bad', SUSPENDED: 'fx-chip--warn', LIFTED: 'fx-chip--teal' }
const print = () => window.print()
</script>
