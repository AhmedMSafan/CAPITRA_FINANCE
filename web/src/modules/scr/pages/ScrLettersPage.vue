<template>
  <!-- Incoming letters: registered by intake, with their scans (kept with a SHA-256 so they can be shown unchanged). -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Letters') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-envelope-o"></span></span>
          <div>
            <h1>{{ tr('Incoming letters') }}</h1>
            <p>{{ tr('Register letters and attach their scans.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <button v-if="canEdit" type="button" class="fx-btn fx-btn--primary" @click="openNew"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New letter') }}</button>
      </div>
    </div>

    <div class="fx-card fx-card-pad fc-filters">
      <label class="fc-field"><span>{{ tr('Search') }}</span>
        <input v-model="search" class="fx-search" :placeholder="tr('Search by number, sender or reference')" @keyup.enter="load">
      </label>
      <div class="fc-filter-actions">
        <button type="button" class="fx-btn fx-btn--teal" @click="load"><span class="fa fa-search" aria-hidden="true"></span>{{ tr('Search') }}</button>
        <button type="button" class="fx-btn" @click="search = ''; load()"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('Clear') }}</button>
      </div>
    </div>

    <div class="fx-card">
      <div class="fc-card-head"><h2>{{ tr('Letters') }} ({{ letters.length }})</h2></div>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th class="fc-no">#</th><th>{{ tr('Letter') }}</th><th>{{ tr('Sender') }}</th><th>{{ tr('Letter date') }}</th><th>{{ tr('Received') }}</th>
              <th>{{ tr('Status') }}</th><th>{{ tr('Decisions') }}</th><th>{{ tr('Scans') }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(l, i) in letters" :key="l.letterId">
              <td class="fc-no">{{ i + 1 }}</td>
              <td><b class="fc-ref" dir="ltr">{{ l.letterNo }}</b><span v-if="l.senderReference" class="fx-cell-sub" dir="ltr">{{ l.senderReference }}</span></td>
              <td>{{ l.senderName }}<span class="fx-cell-sub">{{ tr(LABELS[l.channelCode] ?? l.channelCode) }}</span></td>
              <td>{{ formatDate(l.letterDate) }}</td>
              <td>{{ formatDateTime(l.receivedAt) }}</td>
              <td><span class="fx-chip" :class="l.statusCode === 'RECEIVED' ? 'fx-chip--warn' : 'fx-chip--ok'">{{ tr(LABELS[l.statusCode] ?? l.statusCode) }}</span></td>
              <td>{{ l.decisionCount }}</td>
              <td>
                <div class="scr-files">
                  <button v-for="f in l.files" :key="f.letterFileId" type="button" class="fx-btn fx-btn--sm" :title="`SHA-256 ${f.sha256Hash}`" @click="view(l, f)">
                    <span :class="['fa', f.mimeType === 'application/pdf' ? 'fa-file-pdf-o' : 'fa-file-image-o']" aria-hidden="true"></span>{{ f.fileName }}
                  </button>
                  <label v-if="canEdit" class="fx-btn fx-btn--sm scr-upload"><span class="fa fa-paperclip" aria-hidden="true"></span>{{ tr('Attach') }}
                    <input type="file" accept="application/pdf,image/png,image/jpeg,image/tiff" @change="attach(l.letterId, $event)">
                  </label>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
      <div v-else-if="!letters.length" class="fx-empty"><span class="fa fa-envelope-o" aria-hidden="true"></span>{{ tr('No letters yet.') }}</div>
    </div>

    <ScrDialog v-if="adding" :title="tr('New letter')" :subtitle="tr('Register an incoming letter; attach its scan from the list afterwards.')" icon="fa-envelope-o" @close="adding = false">
      <div class="fc-grid fc-grid--3">
        <label class="fc-field fc-span2"><span>{{ tr('Sender') }} <b>*</b></span><input v-model="form.senderName" class="fx-search">
          <small v-if="errors.senderName" class="scr-err">{{ errors.senderName }}</small></label>
        <label class="fc-field"><span>{{ tr('Channel') }} <b>*</b></span>
          <select v-model="form.channelCode" class="fx-select"><option v-for="c in ['COURIER', 'EMAIL', 'HAND', 'PORTAL']" :key="c" :value="c">{{ tr(LABELS[c] ?? c) }}</option></select>
        </label>
        <label class="fc-field fc-span2"><span>{{ tr('Sender reference') }}</span><input v-model="form.senderReference" class="fx-search" dir="ltr"></label>
        <label class="fc-field"><span>{{ tr('Letter date') }} <b>*</b></span><input v-model="form.letterDate" class="fx-search" type="date">
          <small v-if="errors.letterDate" class="scr-err">{{ errors.letterDate }}</small></label>
        <label class="fc-field fc-full"><span>{{ tr('Notes') }}</span><textarea v-model="form.notes" class="fx-search fc-textarea"></textarea></label>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" @click="adding = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="saving" @click="save"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('Save') }}</button>
      </template>
    </ScrDialog>
    <ScrDocViewer v-if="viewing" :title="viewing.letterNo" :file-name="viewing.file.fileName" :mime-type="viewing.file.mimeType"
                  :subtitle="`SHA-256 ${viewing.file.sha256Hash.slice(0, 12)}…`" :load="() => scrApi.letterFile(viewing!.file.letterFileId)" @close="viewing = null" />
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
import { formatDate, formatDateTime } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import ScrDialog from '../components/ScrDialog.vue'
import ScrDocViewer from '../components/ScrDocViewer.vue'
import { LABELS, errorText, fieldErrors, scrApi, type ScrLetter, type ScrLetterFile, type ScrLetterRequest } from '../api/scr.api'

const canEdit = hasPermission('SCR.LETTER.EDIT')
const letters = ref<ScrLetter[]>([])
const loading = ref(false)
const saving = ref(false)
const adding = ref(false)
const search = ref('')
const errors = ref<Record<string, string>>({})
const blank = (): ScrLetterRequest => ({ senderName: '', senderReference: null, channelCode: 'COURIER', letterDate: null, notes: null })
const form = reactive<ScrLetterRequest>(blank())

function openNew() {
  Object.assign(form, blank())
  errors.value = {}
  adding.value = true
}

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
    showSuccess(tr('Scan attached.'))
    await load()
  } catch (err) {
    showError(errorText(err))
  }
}

/** The scan shown in the pop-up viewer. */
const viewing = ref<{ letterNo: string; file: ScrLetterFile } | null>(null)
const view = (l: ScrLetter, file: ScrLetterFile) => { viewing.value = { letterNo: l.letterNo, file } }

onMounted(load)
</script>
