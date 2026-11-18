<template>
  <!-- Incoming letters: registered by intake, with their scans (kept with a SHA-256 so they can be shown unchanged). -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Letters') }}</b>
        </nav>
        <h1>{{ tr('Incoming letters') }}</h1>
      </div>
      <div class="fx-actions">
        <button v-if="canEdit" type="button" class="fx-btn fx-btn--teal" @click="adding = !adding"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New letter') }}</button>
      </div>
    </div>

    <section v-if="adding" class="fx-card">
      <div class="scr-grid scr-grid--3">
        <label class="scr-field"><span>{{ tr('Sender') }} *</span><input v-model="form.senderName"><small v-if="errors.senderName" class="err">{{ errors.senderName }}</small></label>
        <label class="scr-field"><span>{{ tr('Sender reference') }}</span><input v-model="form.senderReference" dir="ltr"></label>
        <label class="scr-field"><span>{{ tr('Channel') }} *</span>
          <select v-model="form.channelCode"><option v-for="c in ['COURIER', 'EMAIL', 'HAND', 'PORTAL']" :key="c" :value="c">{{ tr(LABELS[c] ?? c) }}</option></select>
        </label>
        <label class="scr-field"><span>{{ tr('Letter date') }} *</span><input v-model="form.letterDate" type="date"><small v-if="errors.letterDate" class="err">{{ errors.letterDate }}</small></label>
        <label class="scr-field scr-wide"><span>{{ tr('Notes') }}</span><textarea v-model="form.notes" rows="2"></textarea></label>
      </div>
      <div class="scr-actions">
        <button type="button" class="fx-btn" @click="adding = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="saving" @click="save">{{ tr('Save') }}</button>
      </div>
    </section>

    <section class="fx-card">
      <div class="scr-filter"><input v-model="search" class="fx-search" :placeholder="tr('Search by number, sender or reference')" @keyup.enter="load"></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Letter') }}</th><th>{{ tr('Sender') }}</th><th>{{ tr('Letter date') }}</th><th>{{ tr('Received') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Decisions') }}</th><th>{{ tr('Scans') }}</th></tr></thead>
          <tbody>
            <tr v-for="l in letters" :key="l.letterId">
              <td dir="ltr"><b>{{ l.letterNo }}</b><br><small class="scr-muted">{{ l.senderReference }}</small></td>
              <td>{{ l.senderName }}<br><small class="scr-muted">{{ tr(LABELS[l.channelCode] ?? l.channelCode) }}</small></td>
              <td>{{ formatDate(l.letterDate) }}</td>
              <td>{{ formatDateTime(l.receivedAt) }}</td>
              <td>{{ tr(LABELS[l.statusCode] ?? l.statusCode) }}</td>
              <td>{{ l.decisionCount }}</td>
              <td>
                <button v-for="f in l.files" :key="f.letterFileId" type="button" class="fx-btn fx-btn--sm" :title="`SHA-256 ${f.sha256Hash}`" @click="open(f.letterFileId)">
                  <span class="fa fa-file-o" aria-hidden="true"></span>{{ f.fileName }}
                </button>
                <label v-if="canEdit" class="fx-btn fx-btn--sm"><span class="fa fa-upload" aria-hidden="true"></span>{{ tr('Attach') }}
                  <input type="file" accept="application/pdf,image/png,image/jpeg,image/tiff" hidden @change="attach(l.letterId, $event)">
                </label>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
        <div v-else-if="!letters.length" class="fx-empty">{{ tr('No letters yet.') }}</div>
      </div>
    </section>
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
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { LABELS, errorText, fieldErrors, scrApi, type ScrLetter, type ScrLetterRequest } from '../api/scr.api'

const canEdit = hasPermission('SCR.LETTER.EDIT')
const letters = ref<ScrLetter[]>([])
const loading = ref(false)
const saving = ref(false)
const adding = ref(false)
const search = ref('')
const errors = ref<Record<string, string>>({})
const blank = (): ScrLetterRequest => ({ senderName: '', senderReference: null, channelCode: 'COURIER', letterDate: null, notes: null })
const form = reactive<ScrLetterRequest>(blank())

async function load() {
  loading.value = true
  try {
    letters.value = await scrApi.letters(search.value)
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
}

async function save() {
  saving.value = true
  errors.value = {}
  try {
    const l = await scrApi.createLetter({ ...form })
    Object.assign(form, blank())
    adding.value = false
    showSuccess(`${tr('Letter registered')}: ${l.letterNo}`)
    await load()
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) showError(errorText(err))
  } finally {
    saving.value = false
  }
}

async function attach(letterId: number, e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file) return
  try {
    await scrApi.addLetterFile(letterId, file)
    await load()
  } catch (err) {
    showError(errorText(err))
  }
}

async function open(letterFileId: number) {
  try {
    const url = URL.createObjectURL(await scrApi.letterFile(letterFileId))
    window.open(url, '_blank', 'noopener')
    setTimeout(() => URL.revokeObjectURL(url), 60_000)
  } catch (err) {
    showError(errorText(err))
  }
}

onMounted(load)
</script>
