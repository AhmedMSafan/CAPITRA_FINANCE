<template>
  <!--
    Price plans (Screening billing): what a member company pays, in EGP: a fee per user per month (the administrator counts) and a
    fee per inquiry. Invoices copy the prices when issued, so changing a plan changes only later invoices. Plans are never deleted:
    one no longer offered is made inactive.
  -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'scr-billing' }">{{ tr('Billing') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Price plans') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-tags"></span></span>
          <div>
            <h1>{{ tr('Price plans') }}</h1>
            <p>{{ tr('What member companies pay: a fee per user per month and a fee per inquiry, in EGP.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <button v-if="canManage" type="button" class="fx-btn fx-btn--primary" @click="edit(null)"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New price plan') }}</button>
      </div>
    </div>

    <div class="fx-card">
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr><th>{{ tr('Code') }}</th><th>{{ tr('Name') }}</th><th class="fx-r">{{ tr('Fee per user per month') }}</th><th class="fx-r">{{ tr('Fee per inquiry') }}</th>
              <th>{{ tr('Companies') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Changed') }}</th><th class="fx-r">{{ tr('Actions') }}</th></tr>
          </thead>
          <tbody>
            <tr v-for="p in plans" :key="p.pricePlanId">
              <td dir="ltr"><b>{{ p.planCode }}</b></td>
              <td>{{ p.nameAr }}<span class="fx-cell-sub">{{ p.nameEn }}</span></td>
              <td class="fx-r" dir="ltr"><b>{{ egp(p.userMonthlyFee) }}</b></td>
              <td class="fx-r" dir="ltr"><b>{{ egp(p.inquiryFee) }}</b></td>
              <td>{{ p.companyCount }}</td>
              <td><span class="fx-chip" :class="p.isActive ? 'fx-chip--ok' : 'fx-chip--plain'">{{ tr(p.isActive ? 'Active' : 'Inactive') }}</span></td>
              <td>{{ p.updatedBy ?? p.createdBy }}<span class="fx-cell-sub">{{ formatDateTime(p.updatedAt ?? p.createdAt) }}</span></td>
              <td class="fx-r"><button v-if="canManage" type="button" class="fx-btn fx-btn--sm" @click="edit(p)"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}</button></td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loaded && !plans.length" class="fx-empty"><span class="fa fa-tags" aria-hidden="true"></span>{{ tr('No price plan yet. Add one, then choose it on each company.') }}</div>
    </div>

    <ScrDialog v-if="editing" :title="editId ? tr('Edit price plan') : tr('New price plan')" icon="fa-tags" @close="editing = false">
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Code') }} <b>*</b></span><input v-model="form.planCode" class="fx-search" dir="ltr" placeholder="STANDARD">
          <small v-if="errors.planCode" class="scr-err">{{ errors.planCode }}</small></label>
        <label class="fc-field"><span>{{ tr('Name (Arabic)') }} <b>*</b></span><input v-model="form.nameAr" class="fx-search" dir="rtl">
          <small v-if="errors.nameAr" class="scr-err">{{ errors.nameAr }}</small></label>
        <label class="fc-field"><span>{{ tr('Name (English)') }} <b>*</b></span><input v-model="form.nameEn" class="fx-search" dir="ltr">
          <small v-if="errors.nameEn" class="scr-err">{{ errors.nameEn }}</small></label>
        <label class="fc-field"><span>{{ tr('Fee per user per month') }} (EGP) <b>*</b></span>
          <input v-model.number="form.userMonthlyFee" class="fx-search" type="number" min="0" step="0.01" dir="ltr">
          <small class="scr-hint">{{ tr('A user active on any day of a month pays the month; the administrator counts.') }}</small>
          <small v-if="errors.userMonthlyFee" class="scr-err">{{ errors.userMonthlyFee }}</small></label>
        <label class="fc-field"><span>{{ tr('Fee per inquiry') }} (EGP) <b>*</b></span>
          <input v-model.number="form.inquiryFee" class="fx-search" type="number" min="0" step="0.01" dir="ltr">
          <small class="scr-hint">{{ tr('Every inquiry, whatever its result.') }}</small>
          <small v-if="errors.inquiryFee" class="scr-err">{{ errors.inquiryFee }}</small></label>
        <label class="fc-check" style="align-self:end;padding-bottom:8px"><input v-model="form.isActive" type="checkbox"><span>{{ tr('Active') }}</span></label>
        <label class="fc-field fc-full"><span>{{ tr('Notes') }}</span><textarea v-model="form.notes" class="fx-search fc-textarea"></textarea></label>
        <div v-if="editId && current?.companyCount" class="fc-note fc-full"><span class="fa fa-info-circle" aria-hidden="true"></span>
          <span>{{ current.companyCount }} {{ tr('companies use this plan. New prices apply to invoices issued from now on; issued invoices keep their prices.') }}</span></div>
        <div v-if="errors.general" class="fc-note fc-note--warn fc-full"><span class="fa fa-exclamation-circle" aria-hidden="true"></span><span>{{ errors.general }}</span></div>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" @click="editing = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save"><span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-check']" aria-hidden="true"></span>{{ tr('Save') }}</button>
      </template>
    </ScrDialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { onMounted, reactive, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import ScrDialog from '../components/ScrDialog.vue'
import { formatDateTime } from '../utils/dates'
import { egp, errorText, fieldErrors, scrApi, type ScrPricePlan, type ScrPricePlanRequest } from '../api/scr.api'

const canManage = hasPermission('SCR.BILLING.MANAGE')
const plans = ref<ScrPricePlan[]>([])
const loaded = ref(false)
const editing = ref(false)
const editId = ref<number | null>(null)
const current = ref<ScrPricePlan | null>(null)
const busy = ref(false)
const errors = ref<Record<string, string>>({})
const blank = (): ScrPricePlanRequest => ({ planCode: '', nameEn: '', nameAr: '', userMonthlyFee: null, inquiryFee: null, isActive: true, notes: null })
const form = reactive<ScrPricePlanRequest>(blank())

async function load() {
  try {
    plans.value = await scrApi.pricePlans()
    loaded.value = true
  } catch (err) {
    showError(errorText(err))
  }
}

function edit(p: ScrPricePlan | null) {
  errors.value = {}
  editId.value = p?.pricePlanId ?? null
  current.value = p
  Object.assign(form, p ? { planCode: p.planCode, nameEn: p.nameEn, nameAr: p.nameAr, userMonthlyFee: p.userMonthlyFee, inquiryFee: p.inquiryFee, isActive: p.isActive, notes: p.notes } : blank())
  editing.value = true
}

async function save() {
  busy.value = true
  errors.value = {}
  const r = { ...form, userMonthlyFee: form.userMonthlyFee === ('' as unknown) ? null : form.userMonthlyFee, inquiryFee: form.inquiryFee === ('' as unknown) ? null : form.inquiryFee }
  try {
    if (editId.value) await scrApi.updatePricePlan(editId.value, r)
    else await scrApi.createPricePlan(r)
    showSuccess(tr('Price plan saved.'))
    editing.value = false
    await load()
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) errors.value = { general: errorText(err) }
  } finally {
    busy.value = false
  }
}

onMounted(load)
</script>
