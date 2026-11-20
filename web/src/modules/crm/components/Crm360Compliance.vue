<template>
  <!-- Customer 360 > Compliance: the profile (KYC, AML, sanctions, PEP, blacklist, review), Edit and Verify (four eyes), and the history. -->
  <div class="c3-tab">
    <section class="c3-panel">
      <header class="c3-panel-head">
        <h3><span class="fa fa-shield" aria-hidden="true"></span>{{ tr('Compliance Profile') }}</h3>
        <span class="fx-chip" :class="tone(badge.code)">{{ tr(badge.label) }}</span>
        <div class="c3-actions">
          <button v-if="canEdit" type="button" class="fx-btn" @click="openEdit"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}</button>
          <button v-if="canVerify" type="button" class="fx-btn fx-btn--teal" :disabled="!state.canVerify" :title="state.verifyBlockedReason ? tr(state.verifyBlockedReason) : ''"
                  @click="verifying = true">
            <span class="fa fa-check-circle" aria-hidden="true"></span>{{ tr('Verify') }}
          </button>
        </div>
      </header>
      <ul v-if="badge.reasons.length" class="c3-reasons"><li v-for="r in badge.reasons" :key="r">{{ tr(r) }}</li></ul>
      <dl class="c3-facts">
        <div><dt>{{ tr('Risk Grade') }}</dt><dd>{{ p.riskGrade || '—' }}</dd></div>
        <div><dt>{{ tr('KYC Status') }}</dt><dd><span class="fx-chip" :class="chip(p.kycStatus)">{{ p.kycStatus || '—' }}</span></dd></div>
        <div><dt>{{ tr('KYC Expiry') }}</dt><dd>{{ formatDate(p.kycExpiryDate) || '—' }}</dd></div>
        <div><dt>{{ tr('AML') }}</dt><dd><span class="fx-chip" :class="chip(p.amlStatus)">{{ p.amlStatus || '—' }}</span></dd></div>
        <div><dt>{{ tr('AML Check Date') }}</dt><dd>{{ formatDate(p.amlCheckDate) || '—' }}</dd></div>
        <div><dt>{{ tr('Sanctions') }}</dt><dd><span class="fx-chip" :class="chip(p.sanctionsStatus)">{{ p.sanctionsStatus || '—' }}</span></dd></div>
        <div><dt>{{ tr('Politically Exposed') }}</dt><dd>{{ tr(p.isPep ? 'Yes' : 'No') }}</dd></div>
        <div><dt>{{ tr('Blacklisted') }}</dt><dd>{{ tr(p.isBlacklisted ? 'Yes' : 'No') }}<template v-if="p.blacklistReason"> · {{ p.blacklistReason }}</template></dd></div>
        <div><dt>{{ tr('Next Review') }}</dt><dd>{{ formatDate(p.nextReviewDate) || '—' }}</dd></div>
        <div><dt>{{ tr('Last Changed') }}</dt><dd>{{ state.changedBy ? `${state.changedBy} · ${formatDateTime(state.changedAt)}` : '—' }}</dd></div>
        <div><dt>{{ tr('Verified') }}</dt><dd>{{ state.verifiedBy ? `${state.verifiedBy} · ${formatDateTime(state.verifiedAt)}` : tr('Not verified') }}</dd></div>
      </dl>
    </section>

    <section class="c3-panel">
      <header class="c3-panel-head"><h3><span class="fa fa-history" aria-hidden="true"></span>{{ tr('History') }}</h3></header>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('When') }}</th><th>{{ tr('By') }}</th><th>{{ tr('Action') }}</th><th>{{ tr('KYC') }}</th><th>{{ tr('KYC Expiry') }}</th><th>{{ tr('AML') }}</th><th>{{ tr('Sanctions') }}</th><th>{{ tr('PEP') }}</th><th>{{ tr('Blacklisted') }}</th><th>{{ tr('Notes') }}</th></tr></thead>
          <tbody>
            <tr v-for="h in history" :key="h.complianceHistoryId">
              <td>{{ formatDateTime(h.at) }}</td><td>{{ h.by }}</td>
              <td><span class="fx-chip" :class="h.actionCode === 'VERIFY' ? 'fx-chip--ok' : 'fx-chip--teal'">{{ tr(h.actionCode === 'VERIFY' ? 'Verified' : 'Changed') }}</span></td>
              <td>{{ h.kycStatus }}</td><td>{{ formatDate(h.kycExpiryDate) }}</td><td>{{ h.amlStatus }}</td><td>{{ h.sanctionsStatus }}</td>
              <td>{{ tr(h.isPep ? 'Yes' : 'No') }}</td><td>{{ tr(h.isBlacklisted ? 'Yes' : 'No') }}</td><td>{{ h.notes || '' }}</td>
            </tr>
          </tbody>
        </table>
        <div v-if="!history.length" class="fx-empty"><span class="fa fa-history" aria-hidden="true"></span>{{ tr('No changes recorded yet.') }}</div>
      </div>
    </section>

    <Crm360Dialog v-model:visible="editing" :title="tr('Edit Compliance Profile')" icon="fa-shield" :busy="busy">
      <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr(makerChecker ? 'Saving a change removes the verification. Another user must verify it again.' : 'Saving a change removes the verification. Verify it again to make it count.') }}</p>
      <div class="c3-grid">
        <label class="fc-field" :class="bad('riskGrade')"><span>{{ tr('Risk Grade') }}</span>
          <select v-model="form.riskGrade" class="fx-select"><option :value="null"></option><option v-for="v in RISK" :key="v" :value="v">{{ v }}</option></select></label>
        <label class="fc-field" :class="bad('kycStatus')"><span>{{ tr('KYC Status') }}<b> *</b></span>
          <select v-model="form.kycStatus" class="fx-select"><option v-for="v in KYC" :key="v" :value="v">{{ v }}</option></select></label>
        <label class="fc-field" :class="bad('kycExpiryDate')"><span>{{ tr('KYC Expiry Date') }}</span><input v-model="form.kycExpiryDate" type="date" class="fx-search"></label>
        <label class="fc-field" :class="bad('amlStatus')"><span>{{ tr('AML Status') }}<b> *</b></span>
          <select v-model="form.amlStatus" class="fx-select"><option v-for="v in AML" :key="v" :value="v">{{ v }}</option></select></label>
        <label class="fc-field"><span>{{ tr('AML Check Date') }}</span><input v-model="form.amlCheckDate" type="date" class="fx-search"></label>
        <label class="fc-field" :class="bad('sanctionsStatus')"><span>{{ tr('Sanctions Screening') }}<b> *</b></span>
          <select v-model="form.sanctionsStatus" class="fx-select"><option v-for="v in SANCTIONS" :key="v" :value="v">{{ v }}</option></select></label>
        <label class="fc-field"><span>{{ tr('Politically Exposed Person') }}</span>
          <select v-model="form.isPep" class="fx-select"><option :value="false">{{ tr('No') }}</option><option :value="true">{{ tr('Yes') }}</option></select></label>
        <label class="fc-field"><span>{{ tr('Blacklisted') }}</span>
          <select v-model="form.isBlacklisted" class="fx-select"><option :value="false">{{ tr('No') }}</option><option :value="true">{{ tr('Yes') }}</option></select></label>
        <label class="fc-field"><span>{{ tr('Next Review Date') }}</span><input v-model="form.nextReviewDate" type="date" class="fx-search"></label>
        <label v-if="form.isBlacklisted" class="fc-field c3-wide" :class="bad('blacklistReason')"><span>{{ tr('Blacklist Reason') }}<b> *</b></span>
          <input v-model="form.blacklistReason" class="fx-search" maxlength="1000"></label>
        <label class="fc-field c3-wide"><span>{{ tr('Notes for the history') }}</span><input v-model="form.notes" class="fx-search" maxlength="1000"></label>
      </div>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="editing = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr('Save') }}</button>
      </template>
    </Crm360Dialog>

    <Crm360Dialog v-model:visible="verifying" :title="tr('Verify Compliance Profile')" icon="fa-check-circle" :busy="busy" :width="520">
      <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Confirm that you checked the KYC documents and the AML and sanctions results.') }}</p>
      <label class="fc-field"><span>{{ tr('Notes') }}</span><input v-model="verifyNotes" class="fx-search" maxlength="1000"></label>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="verifying = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="verify"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check-circle'" aria-hidden="true"></span>{{ tr('Verify') }}</button>
      </template>
    </Crm360Dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { ApiError } from '@/shared/api/http'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { crm360Api, type CrmBadge, type CrmCompliance, type CrmComplianceHistory, type CrmComplianceState } from '../api/crm.api'
import Crm360Dialog from './Crm360Dialog.vue'
import { chip } from './crm360'

const props = defineProps<{ clientId: number; state: CrmComplianceState; badge: CrmBadge; history: CrmComplianceHistory[]; makerChecker: boolean; canEdit: boolean; canVerify: boolean }>()
const emit = defineEmits<{ changed: [] }>()

const KYC = ['PENDING', 'APPROVED', 'REJECTED']
const AML = ['PENDING', 'CLEAR', 'REVIEW', 'BLOCKED']
const SANCTIONS = ['PENDING', 'CLEAR', 'MATCH', 'BLOCKED']
const RISK = ['LOW', 'MEDIUM', 'HIGH']

const p = computed(() => props.state.profile)
const tone = (code: string) => ({ CLEAR: 'fx-chip--ok', DUE: 'fx-chip--warn', PENDING: 'fx-chip--warn', EXPIRED: 'fx-chip--bad', BLOCKED: 'fx-chip--bad' } as Record<string, string>)[code] ?? 'fx-chip--teal'

const editing = ref(false)
const verifying = ref(false)
const busy = ref(false)
const verifyNotes = ref('')
const errors = ref<Record<string, string[]>>({})
const form = reactive<CrmCompliance>({ ...p.value })
const bad = (f: string) => ({ 'c3-bad': !!errors.value[f] })

function openEdit() {
  Object.assign(form, { ...p.value, notes: null, kycStatus: p.value.kycStatus || 'PENDING', amlStatus: p.value.amlStatus || 'PENDING', sanctionsStatus: p.value.sanctionsStatus || 'PENDING' })
  errors.value = {}
  editing.value = true
}

async function run(action: () => Promise<void>, done: string, close: () => void) {
  busy.value = true
  try {
    await action()
    close()
    showSuccess(tr(done))
    emit('changed')
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) {
      errors.value = err.problem.errors
      showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    } else showError(err instanceof Error ? err.message : String(err))
  } finally {
    busy.value = false
  }
}

const blank = (v: string | null) => (v ? v : null)
const save = () => run(() => crm360Api.saveCompliance(props.clientId, {
  ...form, kycExpiryDate: blank(form.kycExpiryDate), amlCheckDate: blank(form.amlCheckDate), nextReviewDate: blank(form.nextReviewDate)
}), props.makerChecker ? 'Compliance profile saved. Another user must verify it.' : 'Compliance profile saved. Verify it to make it count.', () => (editing.value = false))
const verify = () => run(() => crm360Api.verifyCompliance(props.clientId, verifyNotes.value || null), 'Compliance profile verified.', () => {
  verifying.value = false
  verifyNotes.value = ''
})
</script>

<style scoped>
@import './crm360.css';
</style>
