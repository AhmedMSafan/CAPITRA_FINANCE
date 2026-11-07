<template>
  <!-- Add or Edit Bank Rule: a condition the bank puts on its money, checked at every drawdown. A saved rule waits for a second user to
       verify it (four eyes) before it is checked. -->
  <Crm360Dialog v-model:visible="visible" :title="rule ? tr('Edit Bank Rule') : tr('Add Bank Rule')" :subtitle="bank?.bankName ?? ''"
                icon="fa-balance-scale" :busy="busy" :width="720">
    <!-- what the rule will do, in one plain sentence, so the user checks the meaning rather than the fields -->
    <p class="trs-says"><span class="fa fa-balance-scale" aria-hidden="true"></span><span>{{ sentence }}</span></p>
    <div class="c3-grid">
      <label class="fc-field"><span>{{ tr('Rule') }}<b> *</b></span>
        <select v-model="form.ruleTypeCode" class="fx-select"><option v-for="o in ruleTypes" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Applies To Facility') }}</span>
        <select v-model="form.facilityId" class="fx-select"><option :value="null">{{ tr('Every facility of the bank') }}</option>
          <option v-for="f in bank?.facilities ?? []" :key="f.facilityId" :value="f.facilityId">{{ f.facilityRef }}</option></select></label>

      <template v-if="form.ruleTypeCode !== 'MAX_TENOR'">
        <label class="fc-field"><span>{{ tr('Rule On') }}<b> *</b></span>
          <select v-model="form.subjectTypeCode" class="fx-select"><option v-for="o in subjects" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select></label>
        <label v-if="members" class="fc-field"><span>{{ tr(subjectName) }}<b> *</b></span>
          <select v-model="form.subjectValue" class="fx-select">
            <option v-for="o in members" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select></label>
        <label v-else-if="form.subjectTypeCode === 'DEBTOR_GROUP'" class="fc-field"><span>{{ tr('Debtor Group') }}<b v-if="form.ruleTypeCode === 'EXCLUDE'"> *</b></span>
          <select v-model="form.subjectValue" class="fx-select">
            <option v-if="form.ruleTypeCode === 'CONCENTRATION'" :value="null">{{ tr('Each debtor group') }}</option>
            <option v-for="g in options.subjects.groups" :key="g" :value="g">{{ g }}</option></select></label>
        <label v-else class="fc-field"><span>{{ form.subjectTypeCode === 'SELLER' ? tr('Seller') : tr('Debtor') }}<b v-if="form.ruleTypeCode === 'EXCLUDE'"> *</b></span>
          <select v-model="form.subjectClientId" class="fx-select">
            <option v-if="form.ruleTypeCode === 'CONCENTRATION'" :value="null">{{ form.subjectTypeCode === 'SELLER' ? tr('Each seller') : tr('Each debtor') }}</option>
            <option v-for="c in options.subjects.clients" :key="c.clientId" :value="c.clientId">{{ c.clientName }} ({{ c.clientNumber }})</option></select></label>
      </template>
      <label v-else class="fc-field"><span>{{ tr('Maximum Tenor (days)') }}<b> *</b></span><input v-model.number="form.maxTenorDays" type="number" min="1" class="fx-search"></label>

      <template v-if="form.ruleTypeCode === 'CONCENTRATION'">
        <label class="fc-field"><span>{{ tr('Maximum %') }}</span><input v-model.number="form.limitPct" type="number" min="0" max="100" step="0.1" class="fx-search"></label>
        <label class="fc-field"><span>{{ tr('Percent Of') }}<b> *</b></span>
          <select v-model="form.basisCode" class="fx-select"><option v-for="o in options.bases" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select></label>
        <label class="fc-field"><span>{{ tr('Or Maximum Amount') }}</span><input v-model.number="form.limitAmount" type="number" min="0" step="1000" class="fx-search"></label>
        <label class="fc-field"><span>{{ tr('Warn At %') }}</span><input v-model.number="form.warningPct" type="number" min="0" max="100" step="0.1" class="fx-search"></label>
      </template>
      <label class="fc-field"><span>{{ tr('If Broken') }}<b> *</b></span>
        <select v-model="form.breachActionCode" class="fx-select"><option v-for="o in options.breachActions" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Notes') }}</span><input v-model="form.notes" class="fx-search" maxlength="1000" :placeholder="tr('e.g. term sheet clause 7.2')"></label>
    </div>
    <p v-if="form.ruleTypeCode === 'CONCENTRATION'" class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Funded pool: what we have drawn from this bank. Facility limit: the approved limit. The rule is checked only after another user verifies it.') }}</p>
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
import { formatAmount } from '@/shared/format/money'

const props = defineProps<{ bank: TrsBankNode | null; rule: TrsBankRule | null; options: TrsLimitsOptions; facilityId: number | null }>()
const emit = defineEmits<{ saved: [] }>()
const visible = defineModel<boolean>('visible', { required: true })
const busy = ref(false)
const form = ref<TrsBankRuleForm>(blank())
const subjects = computed(() => props.options.subjectTypes.filter((x) => x.value !== 'INVOICE'))
const subjectName = computed(() => props.options.subjectTypes.find((x) => x.value === form.value.subjectTypeCode)?.label ?? '')
/** The list to pick from for a business line, product or client segment rule; null for a debtor, seller or group. */
const members = computed(() => {
  const s = props.options.subjects
  switch (form.value.subjectTypeCode) {
    case 'BUSINESS_LINE': return props.options.businessLines
    case 'PRODUCT': return s.products
    case 'CLIENT_SEGMENT': return s.segments
    default: return null
  }
})
const ruleTypes = computed(() => props.options.ruleTypes.map((o) => ({ value: o.value, label: RULE_LABELS[o.value] ?? o.label })))
const RULE_LABELS: Record<string, string> = { EXCLUDE: 'Exclude', CONCENTRATION: 'Maximum share', MAX_TENOR: 'Maximum invoice tenor' }
const optionLabel = (list: { value: string; label: string }[], v: string | null) => list.find((x) => x.value === v)?.label ?? ''
/** The rule in one sentence, e.g. "NBE funds at most 70% of its funded pool for Business line: Leasing. If broken: Block." */
const sentence = computed(() => {
  const f = form.value
  const bank = props.bank?.bankName ?? tr('The bank')
  const facility = f.facilityId ? ` (${props.bank?.facilities.find((x) => x.facilityId === f.facilityId)?.facilityRef ?? ''})` : ''
  const broken = `${tr('If broken')}: ${tr(optionLabel(props.options.breachActions, f.breachActionCode))}.`
  if (f.ruleTypeCode === 'MAX_TENOR') return `${bank}${facility} ${tr('funds no invoice longer than')} ${f.maxTenorDays || '…'} ${tr('days')}. ${broken}`
  const kind = tr(subjectName.value)
  const who = members.value ? tr(optionLabel(members.value, f.subjectValue)) || '…'
    : f.subjectTypeCode === 'DEBTOR_GROUP' ? f.subjectValue ?? tr('each one')
      : props.options.subjects.clients.find((c) => c.clientId === f.subjectClientId)?.clientName ?? (f.ruleTypeCode === 'EXCLUDE' ? '…' : tr('each one'))
  if (f.ruleTypeCode === 'EXCLUDE') return `${bank}${facility} ${tr('does not fund')} ${kind}: ${who}. ${broken}`
  const caps = [f.limitPct ? `${f.limitPct}% ${tr('of its')} ${tr(optionLabel(props.options.bases, f.basisCode)).toLowerCase()}` : '',
    f.limitAmount ? formatAmount(Number(f.limitAmount)) : ''].filter(Boolean).join(` ${tr('or')} `) || '…'
  const warn = f.warningPct ? ` ${tr('Warn from')} ${f.warningPct}%.` : ''
  return `${bank}${facility} ${tr('funds at most')} ${caps} ${tr('for')} ${kind}: ${who}.${warn} ${broken}`
})
watch(() => [form.value.subjectTypeCode, form.value.ruleTypeCode], () => {
  const m = members.value
  if (m && !m.some((x) => x.value === form.value.subjectValue)) form.value.subjectValue = m[0]?.value ?? null
})

function blank(): TrsBankRuleForm {
  return { bankId: props.bank?.bankId ?? 0, facilityId: props.facilityId, ruleTypeCode: 'CONCENTRATION', subjectTypeCode: 'DEBTOR', subjectClientId: null,
    subjectValue: null, limitPct: 15, limitAmount: null, warningPct: null, maxTenorDays: null, basisCode: 'FUNDED_POOL', breachActionCode: 'BLOCK', notes: null }
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
.trs-says { display: flex; gap: 10px; align-items: baseline; margin: 0 0 14px; padding: 12px 14px; border-radius: 8px; background: var(--fin-teal-50);
  color: var(--fin-ink); font-weight: 600; line-height: 1.45; }
.trs-says .fa { color: var(--fin-teal-600); }
</style>
