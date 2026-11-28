<template>
  <!-- Add or Edit Tranche: the part of the facility the bank released (Active) or will release on request (Draft). -->
  <Crm360Dialog v-model:visible="visible" :title="tranche ? tr('Edit Tranche') : tr('Add Tranche')" :subtitle="facility?.facilityRef ?? ''"
                icon="fa-th-large" :busy="busy" :width="620">
    <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
      {{ tr('Only an Active tranche can be drawn. The tranches of a facility cannot add up to more than its limit of') }} {{ money(facility?.approvedLimit) }}.</p>
    <div class="c3-grid trs-2">
      <label class="fc-field"><span>{{ tr('Tranche Name') }}<b> *</b></span><input v-model="form.trancheName" class="fx-search" maxlength="100"></label>
      <label class="fc-field"><span>{{ tr('Amount') }}<b> *</b></span><input v-model.number="form.trancheAmount" type="number" min="0" step="1000" class="fx-search"></label>
      <label class="fc-field"><span>{{ tr('Status') }}<b> *</b></span>
        <select v-model="form.statusCode" class="fx-select"><option value="DRAFT">{{ tr('Draft (not released)') }}</option><option value="ACTIVE">{{ tr('Active (released)') }}</option><option value="CLOSED">{{ tr('Closed') }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Released On') }}<b v-if="form.statusCode === 'ACTIVE'"> *</b></span><input v-model="form.releasedDate" type="date" class="fx-search"></label>
      <label class="fc-field"><span>{{ tr('Matures On') }}</span><input v-model="form.maturityDate" type="date" class="fx-search"></label>
      <label class="fc-field"><span>{{ tr('Notes') }}</span><input v-model="form.notes" class="fx-search" maxlength="1000"></label>
    </div>
    <template #footer>
      <button type="button" class="fx-btn" :disabled="busy" @click="visible = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
      <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="submit"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr('Save') }}</button>
    </template>
  </Crm360Dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { trsLimitsApi, type TrsFacilityNode, type TrsTrancheForm, type TrsTrancheNode } from '../api/trsLimits.api'
import { money, save } from './trs'

const props = defineProps<{ facility: TrsFacilityNode | null; tranche: TrsTrancheNode | null }>()
const emit = defineEmits<{ saved: [] }>()
const visible = defineModel<boolean>('visible', { required: true })
const busy = ref(false)
const form = ref<TrsTrancheForm>({ trancheName: '', trancheAmount: 0, statusCode: 'DRAFT', releasedDate: null, maturityDate: null, notes: null })

watch(visible, (v) => {
  if (!v) return
  const t = props.tranche
  const f = props.facility
  const free = f ? Math.max(0, f.approvedLimit - f.tranches.filter((x) => x.statusCode !== 'CLOSED').reduce((a, x) => a + x.trancheAmount, 0)) : 0
  form.value = t
    ? { trancheName: t.trancheName, trancheAmount: t.trancheAmount, statusCode: t.statusCode, releasedDate: t.releasedDate, maturityDate: t.maturityDate, notes: t.notes }
    : { trancheName: `Tranche ${(f?.tranches.length ?? 0) + 1}`, trancheAmount: free, statusCode: 'DRAFT', releasedDate: null, maturityDate: f?.maturityDate ?? null, notes: null }
})

async function submit() {
  if (!props.facility) return
  busy.value = true
  const body = { ...form.value, releasedDate: form.value.releasedDate || null, maturityDate: form.value.maturityDate || null, notes: form.value.notes || null }
  await save(() => trsLimitsApi.saveTranche(props.facility!.facilityId, props.tranche?.trancheId ?? null, body), 'Tranche saved.', () => {
    visible.value = false
    emit('saved')
  })
  busy.value = false
}
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
.trs-2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
</style>
