<script setup lang="ts">
// Rate Maintenance (ADM_RATE_INDEXES, ADM_RATE_INDEX_VALUES), as Ahmed designed it on 30 Oct 2026: the rate indexes, and for the chosen
// index its rate periods. A period has only a start date and a rate between 0 and 100; it ends the day before the next one starts and
// the latest stays open, so periods never overlap or leave gaps. The period that holds today is highlighted; periods that ended
// before today are history and cannot be changed.
import { computed, nextTick, ref } from 'vue'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FieldOption, FormValues } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { tr } from '@/shared/layout/language'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate, formatDateTime, todayInCairo } from '@/shared/format/datetime'
import { messageOf } from '@/modules/collection/pages/collection'
import { ratesApi, type RateIndex, type RatePeriod } from '../api/rates.api'

const canEdit = hasPermission('ADM.PRODUCT.EDIT')
const currencies = ref<FieldOption[]>([])

// ---- rate indexes
const columns: CrudColumn<RateIndex>[] = [
  { field: 'rateIndexCode', label: 'Code', type: 'code' },
  { field: 'rateIndexName', label: 'Name' },
  { field: 'currencyCode', label: 'Currency', type: 'code' },
  { field: 'currentRatePct', label: 'Rate Today %', type: 'pct' },
  { field: 'periodCount', label: 'Periods', type: 'number' }
]

const fields = computed<CrudField[]>(() => [
  { name: 'rateIndexCode', label: 'Code', type: 'text', required: true, maxLength: 50, uppercase: true },
  { name: 'rateIndexName', label: 'Name', type: 'text', required: true, maxLength: 200 },
  { name: 'currencyId', label: 'Currency', type: 'select', options: currencies.value }
])

const toBody = (v: FormValues, rowVersion: number | null) => ({
  rateIndexCode: String(v.rateIndexCode ?? ''),
  rateIndexName: String(v.rateIndexName ?? ''),
  currencyId: v.currencyId == null || v.currencyId === '' ? null : Number(v.currencyId),
  isActive: Boolean(v.isActive),
  rowVersion
})

const indexPage = ref<{ reload: () => Promise<void> } | null>(null)

const api: CrudApi<RateIndex> = {
  list: async () => {
    if (!currencies.value.length)
      currencies.value = (await ratesApi.currencies()).map(c => ({ value: c.currencyId, label: c.currencyCode }))
    return ratesApi.list()
  },
  create: v => ratesApi.create(toBody(v, null)),
  update: (row, v) => ratesApi.update(row.rateIndexId, toBody(v, row.rowVersion)),
  remove: () => Promise.reject(new Error(tr('A rate index cannot be deleted: set it inactive.')))
}

// ---- periods of the chosen index
const selected = ref<RateIndex | null>(null)
const periods = ref<RatePeriod[]>([])
const periodsEl = ref<HTMLElement | null>(null)
const busy = ref(false)
const editingId = ref<number | null>(null)
const form = ref<{ startDate: string; annualRatePct: number | '' }>({ startDate: todayInCairo(), annualRatePct: '' })
const badRate = ref(false)
const badStart = ref(false)

const current = computed(() => periods.value.find(p => p.isCurrent) ?? null)

async function openPeriods(row: RateIndex) {
  selected.value = row
  cancelEdit()
  try {
    periods.value = await ratesApi.periods(row.rateIndexId)
  } catch (err) {
    showError(messageOf(err))
  }
  await nextTick()
  periodsEl.value?.scrollIntoView({ behavior: 'smooth', block: 'start' })
}

function startEdit(p: RatePeriod) {
  editingId.value = p.rateValueId
  form.value = { startDate: p.startDate, annualRatePct: p.annualRatePct }
  badRate.value = badStart.value = false
}

function cancelEdit() {
  editingId.value = null
  form.value = { startDate: todayInCairo(), annualRatePct: '' }
  badRate.value = badStart.value = false
}

// the rate is between 0 and 100 and the start date is required, checked before the save
function check(): boolean {
  const rate = form.value.annualRatePct
  badStart.value = !form.value.startDate
  badRate.value = rate === '' || Number(rate) < 0 || Number(rate) > 100
  const errors = [badStart.value && 'Start date is required.', badRate.value && 'Rate must be between 0 and 100.'].filter(Boolean) as string[]
  if (errors.length) showError(errors.map(e => tr(e)).join('\n'))
  return !errors.length
}

async function run(work: () => Promise<RatePeriod[]>, message: string) {
  if (!selected.value) return
  busy.value = true
  try {
    periods.value = await work()
    cancelEdit()
    await indexPage.value?.reload()
    showSuccess(tr(message))
  } catch (err) {
    showError(messageOf(err))
  } finally {
    busy.value = false
  }
}

function save() {
  if (!check() || !selected.value) return
  const id = selected.value.rateIndexId
  const body = { startDate: form.value.startDate, annualRatePct: Number(form.value.annualRatePct), rowVersion: null as number | null }
  const editing = periods.value.find(p => p.rateValueId === editingId.value)
  if (editing)
    void run(() => ratesApi.updatePeriod(id, editing.rateValueId, { ...body, rowVersion: editing.rowVersion }), 'Rate period saved.')
  else
    void run(() => ratesApi.addPeriod(id, body), 'Rate period added.')
}

function remove(p: RatePeriod) {
  if (!selected.value || !window.confirm(tr('Delete this rate period? The period before it will run on until the next one starts.'))) return
  const id = selected.value.rateIndexId
  void run(() => ratesApi.removePeriod(id, p.rateValueId, p.rowVersion), 'Rate period deleted.')
}

const rate = (n: number) => `${Number(n).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 6 })}%`
</script>

<template>
  <div class="rx">
    <CrudPage
      ref="indexPage"
      title="Rate Maintenance"
      description="Maintain the rate indexes and the rate of each period."
      icon="pi pi-percentage"
      apex-pages="Capitra"
      entity-label="Rate Index"
      :columns="columns"
      :fields="fields"
      :api="api"
      :row-key="row => row.rateIndexId"
      :new-values="() => ({ isActive: true })"
      :to-values="row => ({ ...row })"
      :can-edit="canEdit"
    >
      <template #actions>
        <RouterLink to="/admin" class="fx-btn fx-btn--ghost"><i class="pi pi-arrow-left"></i><span>{{ tr('Admin Setup') }}</span></RouterLink>
      </template>
      <template #row-actions="{ row }">
        <button type="button" class="fx-btn fx-btn--sm rx-open" :class="{ 'is-on': selected?.rateIndexId === row.rateIndexId }" @click="openPeriods(row)">
          <i class="pi pi-calendar"></i>{{ tr('Periods') }}
        </button>
      </template>
    </CrudPage>

    <section v-if="selected" ref="periodsEl" class="fx rx-periods">
      <div class="fx-card fx-card-pad">
        <div class="rx-head">
          <div class="rx-title">
            <span class="rx-icon"><i class="pi pi-calendar"></i></span>
            <div>
              <div class="rx-kicker">{{ tr('RATE PERIODS') }}</div>
              <h2>{{ selected.rateIndexCode }} — {{ selected.rateIndexName }}</h2>
            </div>
          </div>
          <div v-if="current" class="rx-today">
            <small>{{ tr('Rate today') }}</small>
            <b>{{ rate(current.annualRatePct) }}</b>
            <span>{{ tr('since') }} {{ formatDate(current.startDate) }}</span>
          </div>
          <button type="button" class="rx-close" :title="tr('Close')" @click="selected = null"><i class="pi pi-times"></i></button>
        </div>

        <p class="rx-hint">
          <i class="pi pi-info-circle"></i>
          {{ tr('A period runs from its start date until the day before the next period starts. The latest period has no end date.') }}
        </p>

        <div v-if="canEdit" class="rx-form">
          <label class="rx-field" :class="{ 'is-bad': badStart }">
            <span>{{ tr('Start Date') }} *</span>
            <input v-model="form.startDate" type="date" class="fx-search">
          </label>
          <label class="rx-field" :class="{ 'is-bad': badRate }">
            <span>{{ tr('Rate %') }} *</span>
            <input v-model.number="form.annualRatePct" type="number" min="0" max="100" step="0.01" inputmode="decimal" class="fx-search" placeholder="0 - 100">
          </label>
          <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save">
            <i class="pi" :class="editingId ? 'pi-check' : 'pi-plus'"></i>{{ tr(editingId ? 'Save Period' : 'Add Period') }}
          </button>
          <button v-if="editingId" type="button" class="fx-btn" :disabled="busy" @click="cancelEdit">{{ tr('Cancel') }}</button>
        </div>

        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead>
              <tr>
                <th>{{ tr('Start Date') }}</th>
                <th>{{ tr('End Date') }}</th>
                <th class="fx-r">{{ tr('Rate %') }}</th>
                <th>{{ tr('Status') }}</th>
                <th>{{ tr('Last Changed') }}</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="p in periods" :key="p.rateValueId" :class="{ 'rx-current': p.isCurrent, 'rx-past': p.isPast, 'rx-editing': p.rateValueId === editingId }">
                <td><b>{{ formatDate(p.startDate) }}</b></td>
                <td>{{ p.endDate ? formatDate(p.endDate) : tr('Open') }}</td>
                <td class="fx-r rx-rate">{{ rate(p.annualRatePct) }}</td>
                <td>
                  <span v-if="p.isCurrent" class="fx-chip rx-chip-now"><i class="pi pi-star-fill"></i>{{ tr('Current') }}</span>
                  <span v-else-if="p.isPast" class="fx-chip">{{ tr('Past') }}</span>
                  <span v-else class="fx-chip rx-chip-next">{{ tr('Upcoming') }}</span>
                </td>
                <td class="rx-who">{{ p.changedBy }}<small>{{ formatDateTime(p.changedAt) }}</small></td>
                <td class="fx-r">
                  <template v-if="canEdit && !p.isPast">
                    <button type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="startEdit(p)">{{ tr('Edit') }}</button>
                    <button type="button" class="fx-btn fx-btn--sm rx-del" :disabled="busy" :title="tr('Delete')" @click="remove(p)"><i class="pi pi-trash"></i></button>
                  </template>
                  <i v-else-if="p.isPast" class="pi pi-lock rx-lock" :title="tr('Past periods cannot be changed')"></i>
                </td>
              </tr>
            </tbody>
          </table>
          <div v-if="!periods.length" class="fx-empty">{{ tr('No rate periods yet. Add the first one above.') }}</div>
        </div>
      </div>
    </section>
  </div>
</template>

<style scoped>
.rx-periods { margin-top: 0; }
.rx-open.is-on, .rx-open.is-on i { background: #00636D; color: #fff !important; border-color: #00636D; }
.rx-head { display: flex; align-items: center; gap: 18px; margin-bottom: 12px; }
.rx-title { display: flex; align-items: center; gap: 12px; flex: 1; }
.rx-icon { width: 40px; height: 40px; border-radius: 10px; background: #E6F3F4; color: #00636D; display: flex; align-items: center; justify-content: center; font-size: 18px; }
.rx-kicker { font-size: 11px; font-weight: 700; letter-spacing: .08em; color: #E87722; }
.rx-title h2 { margin: 2px 0 0; font-size: 18px; }
.rx-today { display: flex; align-items: baseline; gap: 8px; padding: 8px 16px; border-radius: 10px; color: #fff; background: linear-gradient(135deg, #00636D 0%, #014b55 100%); }
.rx-today small { font-size: 10px; text-transform: uppercase; letter-spacing: .06em; color: rgba(255, 255, 255, .7); }
.rx-today b { font-size: 20px; color: #ffb27a; }
.rx-today span { font-size: 12px; color: rgba(255, 255, 255, .8); }
.rx-close { border: 0; background: transparent; cursor: pointer; color: #6b7c7b; font-size: 16px; }
.rx-hint { display: flex; align-items: center; gap: 8px; margin: 0 0 14px; font-size: 13px; color: #6b7c7b; }
.rx-form { display: flex; align-items: flex-end; gap: 12px; flex-wrap: wrap; margin-bottom: 16px; padding: 14px; border: 1px dashed var(--fin-line); border-radius: 10px; }
.rx-field { display: flex; flex-direction: column; gap: 5px; font-size: 12px; font-weight: 600; color: #4b5b5a; }
.rx-field input { min-width: 180px; height: 40px; flex: none; }
.rx-field.is-bad input { border-color: #c0392b; }
.rx-rate { font-weight: 700; }
.rx-current td { background: #FFF4EB; }
.rx-current td:first-child { box-shadow: inset 4px 0 0 #E87722; }
.rx-past td { color: #8a9998; }
.rx-editing td { background: #E6F3F4; }
.rx-chip-now { background: #E87722; color: #fff; }
.rx-chip-now i { margin-inline-end: 4px; font-size: 10px; }
.rx-chip-next { background: #E6F3F4; color: #00636D; }
.rx-who small { display: block; font-size: 11px; color: #8a9998; }
.rx-del { margin-inline-start: 6px; }
.rx-lock { color: #9bb0af; }
@media (max-width: 760px) {
  .rx-head { flex-wrap: wrap; }
  .rx-field, .rx-field input { width: 100%; }
}
</style>
