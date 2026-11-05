<template>
  <!-- New or Edit Facility: the bank's approved limit, its pricing and its dates. Tranches and sublimits are added from the tree. -->
  <Crm360Dialog v-model:visible="visible" :title="facility ? tr('Edit Facility') : tr('New Facility')" :subtitle="facility?.facilityRef ?? ''"
                icon="fa-university" :busy="busy" :width="760">
    <div class="c3-grid">
      <label class="fc-field"><span>{{ tr('Bank') }}<b> *</b></span>
        <select v-model.number="form.bankId" class="fx-select"><option v-for="b in banks" :key="b.bankId" :value="b.bankId">{{ b.bankName }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Facility Reference') }}<b> *</b></span><input v-model="form.facilityRef" class="fx-search" maxlength="60"></label>
      <label class="fc-field"><span>{{ tr('Facility Type') }}<b> *</b></span>
        <select v-model="form.facilityTypeCode" class="fx-select"><option v-for="o in types" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Approved Limit') }}<b> *</b></span><input v-model.number="form.approvedLimit" type="number" min="0" step="1000" class="fx-search"></label>
      <label class="fc-field"><span>{{ tr('Currency') }}<b> *</b></span><input v-model="form.currencyCode" class="fx-search" maxlength="3"></label>
      <label class="fc-field"><span>{{ tr('Status') }}<b> *</b></span>
        <select v-model="form.facilityStatus" class="fx-select"><option v-for="s in statuses" :key="s" :value="s">{{ tr(label(s)) }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Rate Type') }}<b> *</b></span>
        <select v-model="form.interestRateType" class="fx-select"><option value="FLOATING">{{ tr('Floating') }}</option><option value="FIXED">{{ tr('Fixed') }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Base Rate') }}</span><input v-model="form.baseRateCode" class="fx-search" maxlength="50" :placeholder="tr('e.g. CBE corridor')"></label>
      <label class="fc-field"><span>{{ tr('Margin (bps)') }}</span><input v-model.number="form.marginBps" type="number" step="1" class="fx-search"></label>
      <label class="fc-field"><span>{{ tr('All-in Rate %') }}</span><input v-model.number="form.allInRatePct" type="number" step="0.01" class="fx-search"></label>
      <label class="fc-field"><span>{{ tr('Start Date') }}<b> *</b></span><input v-model="form.startDate" type="date" class="fx-search"></label>
      <label class="fc-field"><span>{{ tr('Maturity Date') }}<b> *</b></span><input v-model="form.maturityDate" type="date" class="fx-search"></label>
      <label class="trs-check c3-wide"><input v-model="form.isRevolving" type="checkbox">{{ tr('Revolving: what we repay can be drawn again') }}</label>
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
import { trsLimitsApi, type TrsBankNode, type TrsFacilityForm, type TrsFacilityNode, type TrsOption } from '../api/trsLimits.api'
import { label, num, save } from './trs'

const props = defineProps<{ facility: TrsFacilityNode | null; bankId: number | null; banks: TrsBankNode[]; types: TrsOption[] }>()
const emit = defineEmits<{ saved: [] }>()
const visible = defineModel<boolean>('visible', { required: true })
const statuses = ['TERM_SHEET', 'ACTIVE', 'SUSPENDED', 'UNDER_RENEWAL', 'EXPIRED', 'CLOSED']
const busy = ref(false)
const form = ref<TrsFacilityForm>(blank())

function blank(): TrsFacilityForm {
  const today = new Date().toISOString().slice(0, 10)
  return { bankId: props.bankId ?? props.banks[0]?.bankId ?? 0, facilityRef: '', facilityTypeCode: 'RCF', currencyCode: 'EGP', approvedLimit: 0,
    interestRateType: 'FLOATING', baseRateCode: null, marginBps: null, allInRatePct: null, startDate: today, maturityDate: today, isRevolving: true,
    facilityStatus: 'ACTIVE' }
}

watch(visible, (v) => {
  if (!v) return
  const f = props.facility
  form.value = f
    ? { bankId: f.bankId, facilityRef: f.facilityRef, facilityTypeCode: f.facilityTypeCode, currencyCode: f.currencyCode, approvedLimit: f.approvedLimit,
        interestRateType: f.interestRateType, baseRateCode: f.baseRateCode, marginBps: f.marginBps, allInRatePct: f.allInRatePct, startDate: f.startDate,
        maturityDate: f.maturityDate, isRevolving: f.isRevolving, facilityStatus: f.facilityStatus }
    : blank()
})

async function submit() {
  busy.value = true
  const body = { ...form.value, marginBps: num(form.value.marginBps), allInRatePct: num(form.value.allInRatePct), baseRateCode: form.value.baseRateCode || null }
  await save(() => trsLimitsApi.saveFacility(props.facility?.facilityId ?? null, body), 'Facility saved.', () => {
    visible.value = false
    emit('saved')
  })
  busy.value = false
}
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
.trs-check { display: flex; align-items: center; gap: 8px; font-size: var(--fs-sm); color: var(--fin-ink); }
</style>
