<template>
  <!-- Customer 360 > Compliance > Owners and signatories (Ahmed, 26 Nov 2026): one place for the people behind the customer, used by
       every business line. Each person is screened (sanctions, blacklists, PEP); any change needs the profile verified again. -->
  <section class="c3-panel">
    <header class="c3-panel-head">
      <h3><span class="fa fa-users" aria-hidden="true"></span>{{ tr('Owners and signatories') }} ({{ parties.length }})</h3>
      <span class="c3-muted">{{ tr('At least one owner and one authorised signatory. Screen each person against sanctions, blacklist and PEP lists.') }}</span>
    </header>
    <div v-if="canEdit" class="pt-add">
      <select v-model="form.partyTypeCode" class="fx-select" :aria-label="tr('Type')">
        <option v-for="t in TYPES" :key="t.code" :value="t.code">{{ tr(t.name) }}</option>
      </select>
      <input v-model="form.fullName" class="fx-search" :placeholder="tr('Full name')" maxlength="300">
      <input v-model="form.nationalIdNo" class="fx-search" :placeholder="tr('National ID / passport')" maxlength="50" dir="ltr">
      <input v-model="form.nationality" class="fx-search" :placeholder="tr('Nationality')" maxlength="100">
      <input v-if="form.partyTypeCode === 'OWNER'" v-model.number="form.ownershipPct" class="fx-search pt-share" type="number" min="0" max="100" step="0.01" :placeholder="tr('Share %')">
      <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !form.fullName.trim()" @click="add"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Add') }}</button>
    </div>
    <div class="fx-table-wrap">
      <table class="fx-table">
        <thead><tr><th>{{ tr('Type') }}</th><th>{{ tr('Name') }}</th><th>{{ tr('National ID / passport') }}</th><th>{{ tr('Nationality') }}</th><th class="pt-num">{{ tr('Share %') }}</th><th>{{ tr('Screening') }}</th><th v-if="canEdit"></th></tr></thead>
        <tbody>
          <tr v-for="p in parties" :key="p.partyId">
            <td>{{ tr(typeName(p.partyTypeCode)) }}</td>
            <td><b>{{ p.fullName }}</b></td>
            <td dir="ltr">{{ p.nationalIdNo || '—' }}</td>
            <td>{{ p.nationality || '—' }}</td>
            <td class="pt-num">{{ p.ownershipPct ?? '' }}</td>
            <td>
              <select v-if="canEdit" :value="p.screeningResultCode" class="fx-select fx-select--sm" :class="`pt-scr--${(p.screeningResultCode || 'none').toLowerCase()}`"
                      :disabled="busy" :aria-label="`${tr('Screening')}: ${p.fullName}`" @change="screen(p, ($event.target as HTMLSelectElement).value || null)">
                <option :value="null">{{ tr('Not screened') }}</option><option value="CLEAR">{{ tr('Clear') }}</option><option value="PEP">{{ tr('PEP') }}</option><option value="MATCH">{{ tr('Sanctions match') }}</option>
              </select>
              <span v-else-if="p.screeningResultCode" class="fx-chip" :class="SCREEN_CHIP[p.screeningResultCode]">{{ tr(SCREEN_NAME[p.screeningResultCode] ?? p.screeningResultCode) }}</span>
              <span v-else class="c3-muted">{{ tr('Not screened') }}</span>
              <small v-if="p.screenedBy" class="fx-cell-sub">{{ p.screenedBy }} · {{ formatDateTime(p.screenedAt) }}</small>
            </td>
            <td v-if="canEdit" class="fx-r">
              <button type="button" class="fx-btn fx-btn--sm fc-btn-reject" :disabled="busy" @click="remove(p)"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Remove') }}</button>
            </td>
          </tr>
        </tbody>
      </table>
      <div v-if="!parties.length" class="fx-empty"><span class="fa fa-users" aria-hidden="true"></span>{{ tr('No owners or signatories yet.') }}</div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { ApiError } from '@/shared/api/http'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { crm360Api, type CrmParty, type CrmPartyForm } from '../api/crm.api'

const props = defineProps<{ clientId: number; parties: CrmParty[]; canEdit: boolean }>()
const emit = defineEmits<{ changed: [] }>()

const TYPES = [{ code: 'OWNER', name: 'Owner' }, { code: 'MANAGER', name: 'Manager' }, { code: 'SIGNATORY', name: 'Authorised signatory' }]
const typeName = (code: string) => TYPES.find((t) => t.code === code)?.name ?? code
// one screening against sanctions, blacklist and PEP lists: Clear, PEP (enhanced due diligence) or a sanctions match (blocks the customer)
const SCREEN_NAME: Record<string, string> = { CLEAR: 'Clear', PEP: 'PEP', MATCH: 'Sanctions match' }
const SCREEN_CHIP: Record<string, string> = { CLEAR: 'fx-chip--ok', PEP: 'fx-chip--warn', MATCH: 'fx-chip--bad' }

const busy = ref(false)
const form = reactive<CrmPartyForm>({ partyTypeCode: 'OWNER', fullName: '', nationalIdNo: null, nationality: null, ownershipPct: null })

async function run(action: () => Promise<void>, done: string) {
  busy.value = true
  try {
    await action()
    showSuccess(tr(done))
    emit('changed')
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    else showError(err instanceof Error ? err.message : String(err))
  } finally {
    busy.value = false
  }
}

const add = () => run(async () => {
  await crm360Api.addParty(props.clientId, { ...form, nationalIdNo: form.nationalIdNo || null, nationality: form.nationality || null,
    ownershipPct: form.partyTypeCode === 'OWNER' ? form.ownershipPct : null })
  Object.assign(form, { fullName: '', nationalIdNo: null, nationality: null, ownershipPct: null })
}, 'Added. The compliance profile needs verifying again.')

const remove = (p: CrmParty) => {
  if (!window.confirm(`${tr('Remove')} ${p.fullName}?`)) return
  void run(() => crm360Api.removeParty(props.clientId, p.partyId), 'Removed. The compliance profile needs verifying again.')
}

const screen = (p: CrmParty, result: string | null) =>
  run(() => crm360Api.screenParty(props.clientId, p.partyId, result), 'Screening saved. The compliance profile needs verifying again.')
</script>

<style scoped>
@import './crm360.css';
.pt-add { display: flex; flex-wrap: wrap; gap: 8px; margin-bottom: 10px; }
.pt-add .fx-search { flex: 1; min-width: 150px; }
.pt-add .fx-select { max-width: 200px; }
.pt-share { max-width: 110px; }
.pt-num { text-align: end; }
.pt-scr--clear { border-color: #2e9e6a; }
.pt-scr--match { border-color: #c0392b; }
.pt-scr--pep { border-color: #d39b2a; background: #fffaf0; }
</style>
