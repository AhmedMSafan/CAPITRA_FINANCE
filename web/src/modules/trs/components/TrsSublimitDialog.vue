<template>
  <!-- Add or Edit Sublimit: a slice of a tranche and the business lines it may fund (none ticked: every business line). -->
  <Crm360Dialog v-model:visible="visible" :title="sublimit ? tr('Edit Sublimit') : tr('Add Sublimit')" :subtitle="tranche?.trancheName ?? ''"
                icon="fa-sitemap" :busy="busy" :width="620">
    <div class="c3-grid trs-2">
      <label class="fc-field"><span>{{ tr('Sublimit Name') }}<b> *</b></span><input v-model="form.sublimitName" class="fx-search" maxlength="100"></label>
      <label class="fc-field"><span>{{ tr('Amount') }}<b> *</b></span><input v-model.number="form.limitAmount" type="number" min="0" step="1000" class="fx-search"></label>
      <label class="fc-field"><span>{{ tr('Pool') }}<b> *</b></span>
        <select v-model="form.poolTypeCode" class="fx-select"><option value="DEDICATED">{{ tr('Dedicated pool') }}</option><option value="SHARED">{{ tr('Shared cap') }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Status') }}<b> *</b></span>
        <select v-model="form.statusCode" class="fx-select"><option value="ACTIVE">{{ tr('Active') }}</option><option value="SUSPENDED">{{ tr('Suspended') }}</option><option value="CLOSED">{{ tr('Closed') }}</option></select></label>
      <fieldset class="trs-lines c3-wide">
        <legend>{{ tr('May fund') }}</legend>
        <label v-for="o in lines" :key="o.value"><input v-model="form.businessLines" type="checkbox" :value="o.value">{{ tr(o.label) }}</label>
        <small>{{ form.businessLines.length ? '' : tr('None ticked: every business line.') }}</small>
      </fieldset>
      <label class="fc-field c3-wide"><span>{{ tr('Notes') }}</span><input v-model="form.notes" class="fx-search" maxlength="1000"></label>
    </div>
    <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Dedicated sublimits of a tranche cannot add up to more than the tranche. A shared cap only limits what its business lines may draw from the tranche.') }}</p>
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
import { trsLimitsApi, type TrsOption, type TrsSublimitForm, type TrsSublimitNode, type TrsTrancheNode } from '../api/trsLimits.api'
import { save } from './trs'

const props = defineProps<{ tranche: TrsTrancheNode | null; sublimit: TrsSublimitNode | null; lines: TrsOption[] }>()
const emit = defineEmits<{ saved: [] }>()
const visible = defineModel<boolean>('visible', { required: true })
const busy = ref(false)
const form = ref<TrsSublimitForm>({ sublimitName: '', limitAmount: 0, poolTypeCode: 'DEDICATED', statusCode: 'ACTIVE', businessLines: [], notes: null })

watch(visible, (v) => {
  if (!v) return
  const s = props.sublimit
  const t = props.tranche
  const free = t ? Math.max(0, t.trancheAmount - t.sublimits.filter((x) => x.poolTypeCode === 'DEDICATED' && x.statusCode !== 'CLOSED').reduce((a, x) => a + x.limitAmount, 0)) : 0
  form.value = s
    ? { sublimitName: s.sublimitName, limitAmount: s.limitAmount, poolTypeCode: s.poolTypeCode, statusCode: s.statusCode, businessLines: [...s.businessLines], notes: s.notes }
    : { sublimitName: '', limitAmount: free, poolTypeCode: 'DEDICATED', statusCode: 'ACTIVE', businessLines: [], notes: null }
})

async function submit() {
  if (!props.tranche) return
  busy.value = true
  await save(() => trsLimitsApi.saveSublimit(props.tranche!.trancheId, props.sublimit?.sublimitId ?? null, { ...form.value, notes: form.value.notes || null }),
    'Sublimit saved.', () => {
      visible.value = false
      emit('saved')
    })
  busy.value = false
}
</script>

<style scoped>
@import '@/modules/crm/components/crm360.css';
.trs-2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
.trs-lines { display: flex; flex-wrap: wrap; gap: 8px 18px; margin: 0; padding: 10px 12px; border: 1px solid var(--fin-line); border-radius: var(--fin-radius-sm); }
.trs-lines legend { padding: 0 6px; font-size: var(--fs-xs); font-weight: 700; color: var(--fin-muted); }
.trs-lines label { display: flex; align-items: center; gap: 6px; font-size: var(--fs-sm); color: var(--fin-ink); }
.trs-lines small { flex-basis: 100%; color: var(--fin-muted); font-size: var(--fs-xs); }
</style>
