<template>
  <!-- FINOVA_FINANCEING Sales Review "Documents" button and dialog, shared by every factoring workflow step page:
       pick the document type and status, drop or browse a file, and see the agreement's attached documents. -->
  <button type="button" class="fx-btn" :class="{ 'fx-btn--teal': !label }" @click="open">
    <span class="fa" :class="icon || 'fa-folder-open-o'" aria-hidden="true"></span>{{ tr(label || 'Documents') }}
  </button>

  <Teleport to="body">
    <div v-if="visible" class="fx fc fc-modal-back" role="presentation" @click.self="visible = false">
      <section class="fc-modal fc-docs" role="dialog" aria-modal="true" aria-labelledby="fc-docs-title">
        <header class="fc-docs-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-file-o"></span></span>
          <div>
            <h2 id="fc-docs-title">{{ tr('Documents') }}</h2>
            <small>{{ tr('Upload supporting documents for this agreement') }}</small>
          </div>
          <button type="button" class="fx-btn fx-btn--sm" :aria-label="tr('Close')" @click="visible = false"><span class="fa fa-times" aria-hidden="true"></span></button>
        </header>

        <div v-if="error" class="fc-alert fc-alert--sm" role="alert"><span class="fa fa-exclamation-circle" aria-hidden="true"></span>{{ error }}</div>

        <template v-if="canEdit">
          <div class="fc-docs-fields">
            <label class="fc-field"><span>{{ tr('Document Type') }}</span>
              <select v-model="typeId" class="fx-select">
                <option :value="null" disabled>{{ tr('Select document type') }}</option>
                <option v-for="t in types" :key="t.value" :value="t.value">{{ t.label }}</option>
              </select>
            </label>
            <label class="fc-field"><span>{{ tr('Status') }}</span>
              <select v-model="statusId" class="fx-select">
                <option :value="null" disabled>{{ tr('Select status') }}</option>
                <option v-for="s in statuses" :key="s.value" :value="s.value">{{ s.label }}</option>
              </select>
            </label>
          </div>

          <input ref="fileInput" type="file" hidden accept=".pdf,.doc,.docx,.xls,.xlsx,.png,.jpg,.jpeg" @change="onFileSelected">
          <div class="fc-drop" :class="{ 'is-busy': uploading }" role="button" tabindex="0"
               @click="browse" @keydown.enter="browse" @dragover.prevent @drop.prevent="onDrop">
            <template v-if="uploading"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span><b>{{ tr('Uploading document...') }}</b></template>
            <template v-else>
              <span class="fc-drop-ic" aria-hidden="true"><span class="fa fa-cloud-upload"></span></span>
              <b>{{ tr('Drop files here or click to upload') }}</b>
              <span class="fx-btn fx-btn--sm"><span class="fa fa-folder-open-o" aria-hidden="true"></span>{{ tr('Browse Files') }}</span>
              <small>PDF, DOC, DOCX, XLS, XLSX, PNG, JPG · {{ tr('Max 10 MB') }}</small>
            </template>
          </div>
        </template>

        <div class="fc-docs-list">
          <div class="fc-docs-list-head"><b>{{ tr('Attached Documents') }}</b><em>{{ documents.length }}</em></div>
          <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading documents...') }}</div>
          <div v-else-if="!documents.length" class="fx-empty"><span class="fa fa-file-o" aria-hidden="true"></span>{{ tr('No documents uploaded yet.') }}</div>
          <ul v-else>
            <li v-for="d in documents" :key="d.documentId">
              <span class="fc-docs-file" aria-hidden="true"><span class="fa" :class="fileIcon(d)"></span></span>
              <div>
                <b>{{ d.fileName }}</b>
                <small>{{ typeLabel(d.docTypeId) }} · {{ statusLabel(d.statusId) }}<template v-if="d.fileSizeBytes != null"> · {{ fileSize(d.fileSizeBytes) }}</template></small>
              </div>
              <span class="fc-muted">{{ date(d.creationDate) }}</span>
              <button type="button" class="fx-btn fx-btn--sm" :title="tr('View document')" :aria-label="`${tr('View document')} ${d.fileName}`" @click="view(d)">
                <span class="fa fa-eye" aria-hidden="true"></span>
              </button>
            </li>
          </ul>
        </div>

        <p class="fc-note"><span class="fa fa-info-circle" aria-hidden="true"></span><span>{{ tr('Documents are stored against this factoring agreement.') }}</span></p>
      </section>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { dateLocale, tr } from '@/shared/layout/language'
import { DISPLAY_TIME_ZONE } from '@/shared/format/datetime'
import { hasPermission } from '@/shared/auth/session'
import { fctDocumentsApi, type FctAgreementDocument, type FctLovItem } from '../api/fct.api'
import { viewDocument } from './stepFormat'

// label / icon: the same dialog behind a step's own button (Upload Contract, Upload Signed Acceptance)
const props = defineProps<{ agreementId: number; label?: string; icon?: string }>()
const emit = defineEmits<{ uploaded: [] }>()

const MAX_BYTES = 10 * 1024 * 1024
const canEdit = hasPermission('FCT.EDIT')

const visible = ref(false)
const documents = ref<FctAgreementDocument[]>([])
const types = ref<FctLovItem[]>([])
const statuses = ref<FctLovItem[]>([])
const typeId = ref<number | null>(null)
const statusId = ref<number | null>(null)
const loading = ref(false)
const uploading = ref(false)
const error = ref('')
const fileInput = ref<HTMLInputElement | null>(null)

const errorText = (e: unknown, fallback: string) => (e instanceof Error ? e.message : tr(fallback))

async function loadDocuments() {
  loading.value = true
  try {
    documents.value = await fctDocumentsApi.list(props.agreementId)
  } catch (e) {
    error.value = errorText(e, 'Unable to load agreement documents.')
  } finally {
    loading.value = false
  }
}

async function loadLookups() {
  try {
    const lookups = await fctDocumentsApi.lookups()
    types.value = lookups.documentTypes
    statuses.value = lookups.documentStatuses
    // as in FINOVA, the first type and the first status are preselected
    typeId.value ??= types.value[0]?.value ?? null
    statusId.value ??= statuses.value[0]?.value ?? null
  } catch {
    error.value = tr('Unable to load document types/statuses.')
  }
}

async function open() {
  visible.value = true
  error.value = ''
  await Promise.all([loadDocuments(), loadLookups()])
}

function browse() {
  if (!uploading.value) fileInput.value?.click()
}

async function onFileSelected(event: Event) {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  if (file) await upload(file)
  input.value = ''
}

async function onDrop(event: DragEvent) {
  const file = event.dataTransfer?.files?.[0]
  if (file && !uploading.value) await upload(file)
}

async function upload(file: File) {
  if (typeId.value == null || statusId.value == null) {
    error.value = tr('Please select document type and status.')
    return
  }
  if (file.size > MAX_BYTES) {
    error.value = tr('The selected file exceeds the 10 MB limit.')
    return
  }
  uploading.value = true
  error.value = ''
  try {
    await fctDocumentsApi.upload(props.agreementId, file, typeId.value, statusId.value)
    await loadDocuments()
    emit('uploaded')
  } catch (e) {
    error.value = errorText(e, 'Unable to upload document.')
  } finally {
    uploading.value = false
  }
}

async function view(d: FctAgreementDocument) {
  try {
    await viewDocument(d.documentId)
  } catch (e) {
    error.value = errorText(e, 'Unable to open document.')
  }
}

const typeLabel = (id: number) => types.value.find((x) => x.value === id)?.label || `Type ${id}`
const statusLabel = (id: number) => statuses.value.find((x) => x.value === id)?.label || `Status ${id}`

function fileIcon(d: FctAgreementDocument) {
  const mime = (d.mimeType || '').toLowerCase()
  const ext = (d.fileExtension || '').toLowerCase()
  if (mime.includes('pdf') || ext === 'pdf') return 'fa-file-pdf-o'
  if (mime.includes('image') || ['png', 'jpg', 'jpeg'].includes(ext)) return 'fa-file-image-o'
  if (ext === 'xls' || ext === 'xlsx') return 'fa-file-excel-o'
  return 'fa-file-o'
}

function fileSize(value: number) {
  if (!Number.isFinite(value) || value <= 0) return '0 KB'
  if (value < 1024) return `${value} B`
  if (value < 1024 * 1024) return `${(value / 1024).toFixed(1)} KB`
  return `${(value / (1024 * 1024)).toFixed(1)} MB`
}

const date = (v: string) =>
  new Intl.DateTimeFormat(dateLocale(), { year: 'numeric', month: 'short', day: '2-digit', timeZone: DISPLAY_TIME_ZONE }).format(new Date(v))
</script>
