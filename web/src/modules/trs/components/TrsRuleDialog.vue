<template>
  <!-- Add or Edit Bank Rule: a condition the bank puts on its money, checked at every drawdown. A saved rule waits for a second user to
       verify it (four eyes) before it is checked. -->
  <Crm360Dialog v-model:visible="visible" :title="rule ? tr('Edit Bank Rule') : tr('Add Bank Rule')" :subtitle="bank?.bankName ?? ''"
                icon="fa-balance-scale" :busy="busy" :width="720">
    <div class="c3-grid">
      <label class="fc-field"><span>{{ tr('Rule') }}<b> *</b></span>
        <select v-model="form.ruleTypeCode" class="fx-select"><option v-for="o in options.ruleTypes" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Applies To Facility') }}</span>
        <select v-model="form.facilityId" class="fx-select"><option :value="null">{{ tr('Every facility of the bank') }}</option>
          <option v-for="f in bank?.facilities ?? []" :key="f.facilityId" :value="f.facilityId">{{ f.facilityRef }}</option></select></label>
      <label v-if="form.ruleTypeCode !== 'MAX_TENOR'" class="fc-field"><span>{{ tr('Subject') }}<b> *</b></span>
        <select v-model="form.subjectTypeCode" class="fx-select"><option v-for="o in subjects" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select></label>
      <label v-else class="fc-field"><span>{{ tr('Maximum Tenor (days)') }}<b> *</b></span><input v-model.number="form.maxTenorDays" type="number" min="1" class="fx-search"></label>

      <template v-if="form.ruleTypeCode !== 'MAX_TENOR'">
        <label v-if="form.subjectTypeCode === 'DEBTOR_GROUP'" class="fc-field c3-wide"><span>{{ tr('Debtor Group') }}<b v-if="form.ruleTypeCode === 'EXCLUDE'"> *</b></span>
          <select v-model="form.subjectValue" class="fx-select">
            <option v-if="form.ruleTypeCode === 'CONCENTRATION'" :value="null">{{ tr('Each debtor group') }}</option>
            <option v-for="g in options.subjects.groups" :key="g" :value="g">{{ g }}</option></select></label>
        <label v-else class="fc-field c3-wide"><span>{{ form.subjectTypeCode === 'SELLER' ? tr('Seller') : tr('Debtor') }}<b v-if="form.ruleTypeCode === 'EXCLUDE'"> *</b></span>
          <select v-model="form.subjectClientId" class="fx-select">
            <option v-if="form.ruleTypeCode === 'CONCENTRATION'" :value="null">{{ form.subjectTypeCode === 'SELLER' ? tr('Each seller') : tr('Each debtor') }}</option>
            <option v-for="c in options.subjects.clients" :key="c.clientId" :value="c.clientId">{{ c.clientName }} ({{ c.clientNumber }})</option></select></label>
      </template>

      <template v-if="form.ruleTypeCode === 'CONCENTRATION'">
        <label class="fc-field"><span>{{ tr('Limit %') }}</span><input v-model.number="form.limitPct" type="number" min="0" max="100" step="0.1" class="fx-search"></label>
        <label class="fc-field"><span>{{ tr('Or Limit Amount') }}</span><input v-model.number="form.limitAmount" type="number" min="0" step="1000" class="fx-search"></label>
        <label class="fc-field"><span>{{ tr('Warn At %') }}</span><input v-model.number="form.warningPct" type="number" min="0" max="100" step="0.1" class="fx-search"></label>
        <label class="fc-field"><span>{{ tr('Percent Of') }}<b> *</b></span>
          <select v-model="form.basisCode" class="fx-select"><option v-for="o in options.bases" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select></label>
      </template>
      <label class="fc-field"><span>{{ tr('On Breach') }}<b> *</b></span>
        <select v-model="form.breachActionCode" class="fx-select"><option v-for="o in options.breachActions" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select></label>
      <label class="fc-field c3-wide"><span>{{ tr('Notes') }}</span><input v-model="form.notes" class="fx-search" maxlength="1000" :placeholder="tr('e.g. term sheet clause 7.2')"></label>
    </div>
    <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Funded pool: what we have drawn from this bank. Facility limit: the approved limit. The rule is checked only after another user verifies it.') }}</p>
    <template #footer>
      <button type="button" class="fx-btn" :disabled="busy" @click="visible = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
      <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="submit"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr('Save') }}</button>
    </template>
  </Crm360Dialog>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { tr } from '@/shared/layout/language'
import Crm360Dialog from '@/modules/crm/components/Crm360Dialog.vue'
import { trsLimitsApi, type TrsBankNode, type TrsBankRule, type TrsBankRuleForm, type TrsLimitsOptions } from '../api/trsLimits.api'
import { num, save } from './trs'

const props = defineProps<{ bank: TrsBankNode | null; rule: TrsBankRule | null; options: TrsLimitsOptions; facilityId: number | null }>()
const emit = defineEmits<{ saved: [] }>()
const visible = defineModel<boolean>('visible', { required: true })
const busy = ref(false)
const form = ref<TrsBankRuleForm>(blank())
const subjects = computed(() => props.options.subjectTypes.filter((x) => x.value !== 'INVOICE'))

function blank(): TrsBankRuleForm {
  return { bankId: props.bank?.bankId ?? 0, facilityId: props.facilityId, ruleTypeCode: 'CONCENTRATION', subjectTypeCode: 'DEBTOR', subjectClientId: null,
    subjectValue: null, limitPct: 15, limitAmount: null, warningPct: 12, maxTenorDays: null, basisCode: 'FUNDED_POOL', breachActionCode: 'BLOCK', notes: null }
}

watch(visible, (v) => {
  if (!v) return
  const r = props.rule
  form.value = r
    ? { bankId: r.bankId, facilityId: r.facilityId, ruleTypeCode: r.ruleTypeCode, subjectTypeCode: r.subjectTypeCode === 'INVOICE' ? 'DEBTOR' : r.subjectTypeCode,
        subjectClientId: r.subjectClientId, subjectValue: r.subjectValue, limitPct: r.limitPct, limitAmount: r.limitAmount, warningPct: r.warningPct,
        maxTenorDays: r.maxTenorDays, basisCode: r.basisCode, breachActionCode: r.breachActionCode, notes: r.notes }
    : blank()
})

async function submit() {
  busy.value = true
  const f = form.value
  const body = { ...f, limitPct: num(f.limitPct), limitAmount: num(f.limitAmount), warningPct: num(f.warningPct), maxTenorDays: num(f.maxTenorDays), notes: f.notes || null }
  await save(() => trsLimitsApi.saveRule(props.rule?.ruleId ?? null, body), 'Rule saved. Another user must verify it.', () => {
    visible.value = false
    emit('saved')
  })
  busy.value = false
}
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
</style>
