<template>
  <!-- One inquiry's result: the verdict, then each Final List entry it matched and how. -->
  <section class="fx-card">
    <div class="fx-head">
      <h3 dir="ltr">{{ inquiry.inquiryRef }}</h3>
      <small class="scr-muted">{{ formatDateTime(inquiry.screenedAt) }} · {{ inquiry.requestedBy }} · {{ inquiry.companyCode }} · {{ tr('Final List') }} V{{ inquiry.registryVersionNo }}</small>
    </div>
    <div>
      <b>{{ inquiry.subjectName }}</b> · {{ tr(LABELS[inquiry.subjectTypeCode] ?? inquiry.subjectTypeCode) }} ·
      <span dir="ltr">{{ inquiry.identityNo }}</span> ({{ tr(LABELS[inquiry.identityTypeCode] ?? inquiry.identityTypeCode) }})
      <template v-if="inquiry.caseReference"> · {{ tr('Case reference') }} {{ inquiry.caseReference }}</template>
    </div>
    <div class="scr-result" :class="`scr-result--${inquiry.resultCode}`">
      {{ tr(LABELS[inquiry.resultCode] ?? inquiry.resultCode) }}
      <small v-if="inquiry.resultCode === 'POSSIBLE_MATCH'"> · {{ tr('The name matches a listed entry but the identity does not: review before going on.') }}</small>
      <small v-else-if="inquiry.resultCode === 'MATCH'"> · {{ tr('Listed on the Federation Final List: apply the decision\'s scope.') }}</small>
      <small v-else-if="inquiry.resultCode === 'PREVIOUSLY_LISTED'"> · {{ tr('Only lifted entries match: no measure is in force.') }}</small>
    </div>
    <div v-for="(h, i) in inquiry.hits" :key="i" class="scr-hit" :class="{ 'scr-hit--listed': h.entryStatusCode !== 'LIFTED' }">
      <div>
        <b>{{ h.nameAr }}</b> <small class="scr-muted">{{ h.nameEn }}</small>
        <span class="fx-chip" :class="CHIP[h.entryStatusCode]">{{ tr(LABELS[h.entryStatusCode] ?? h.entryStatusCode) }}</span>
        <span class="scr-tag">{{ tr(LABELS[h.matchBasisCode] ?? h.matchBasisCode) }} · {{ h.nameScorePct }}%</span>
      </div>
      <dl>
        <div><dt>{{ tr('Identity number') }}</dt><dd dir="ltr">{{ h.identityNo }}</dd></div>
        <div><dt>{{ tr('Nationality') }}</dt><dd>{{ h.nationalityCode }}</dd></div>
        <div><dt>{{ tr('Decision') }}</dt><dd dir="ltr">{{ h.decisionRef }} · {{ h.officialNo }}</dd></div>
        <div class="scr-wide"><dt>{{ tr('Scope') }}</dt><dd>{{ h.scopeText }}</dd></div>
        <div><dt>{{ tr('Effective date') }}</dt><dd>{{ formatDate(h.listedFrom) }}<template v-if="h.liftedOn"> → {{ formatDate(h.liftedOn) }}</template></dd></div>
      </dl>
    </div>
  </section>
</template>

<script setup lang="ts">
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { CHIP, LABELS, type ScrInquiry } from '../api/scr.api'

defineProps<{ inquiry: ScrInquiry }>()
</script>
