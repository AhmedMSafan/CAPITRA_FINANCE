<template>
  <!-- End of Day › Jobs: every job of the night, in the order it runs: switch it on or off, set its order and its settings. -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'eod-home' }">{{ tr('End of Day') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Jobs') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-sliders"></span></span>
          <div>
            <h1>{{ tr('End of Day jobs') }}</h1>
            <p>{{ tr('What runs each night, in order. A job switched off is skipped; its days are still done.') }}</p>
          </div>
        </div>
      </div>
    </div>

    <div v-if="loading && !forms.length" class="fx-card fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
    <div class="eod-job-cards">
      <section v-for="f in forms" :key="f.job.jobCode" class="fx-card eod-job-card" :class="{ 'eod-job-card--off': !f.isEnabled }">
        <div class="eod-job-head">
          <span class="eod-job-no" aria-hidden="true">{{ f.job.sequenceNo }}</span>
          <div>
            <h2>{{ language === 'AR' ? f.job.nameAr : f.job.nameEn }}</h2>
            <p>{{ language === 'AR' ? f.job.descriptionAr : f.job.descriptionEn }}</p>
          </div>
          <span class="fx-chip" :class="!f.job.isAvailable ? 'fx-chip--bad' : f.job.isEnabled ? 'fx-chip--ok' : 'fx-chip--plain'">
            {{ tr(!f.job.isAvailable ? 'Not in this version' : f.job.isEnabled ? 'On' : 'Off') }}</span>
        </div>
        <div class="eod-job-fields">
          <label class="eod-switch"><input v-model="f.isEnabled" type="checkbox" :disabled="!canEdit"> {{ tr('Runs each night') }}</label>
          <label class="fc-field"><span>{{ tr('Order') }}</span>
            <input v-model.number="f.sequenceNo" class="fx-search" type="number" min="1" max="9999" :disabled="!canEdit">
            <small>{{ tr('Jobs run from the lowest number up.') }}</small></label>
          <label v-for="p in f.job.parameters" :key="p.code" class="fc-field"><span>{{ language === 'AR' ? p.labelAr : p.labelEn }}</span>
            <input v-model="f.parameters[p.code]" class="fx-search" dir="ltr" :placeholder="p.defaultValue" :disabled="!canEdit">
            <small>{{ language === 'AR' ? p.helpAr : p.helpEn }} {{ tr('Default') }}: <span class="eod-default" dir="ltr">{{ p.defaultValue }}</span></small></label>
          <label class="fc-field"><span>{{ tr('Notes') }}</span><input v-model="f.notes" class="fx-search" :disabled="!canEdit"></label>
        </div>
        <div v-if="canEdit" class="eod-job-actions">
          <button type="button" class="fx-btn fx-btn--teal" :disabled="f.busy" @click="save(f)">
            <span :class="['fa', f.busy ? 'fa-spinner fa-spin' : 'fa-check']" aria-hidden="true"></span>{{ tr('Save') }}</button>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '@/modules/scr/components/scr.css'
import './eod.css'
import { onMounted, ref } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { language, tr } from '@/shared/layout/language'
import { eodApi, errorText, type EodJob } from '../api/eod.api'

type Form = { job: EodJob; isEnabled: boolean; sequenceNo: number; notes: string; parameters: Record<string, string>; busy: boolean }

const canEdit = hasPermission('EOD.JOBS.EDIT')
const forms = ref<Form[]>([])
const loading = ref(false)

const toForm = (job: EodJob): Form => ({
  job, isEnabled: job.isEnabled, sequenceNo: job.sequenceNo, notes: job.notes ?? '', busy: false,
  parameters: Object.fromEntries(job.parameters.map((p) => [p.code, p.value ?? '']))
})

async function save(f: Form) {
  f.busy = true
  try {
    await eodApi.saveJob(f.job.jobCode, { isEnabled: f.isEnabled, sequenceNo: f.sequenceNo, notes: f.notes || null, parameters: f.parameters })
    showSuccess(tr('Saved.'))
    await load()
  } catch (err) {
    showError(errorText(err))
  } finally {
    f.busy = false
  }
}

async function load() {
  loading.value = true
  try {
    forms.value = (await eodApi.jobs()).map(toForm)
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
}

onMounted(load)
</script>
