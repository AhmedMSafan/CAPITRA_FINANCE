<template>
  <!--
    The documents of a Consumer Finance supplier application: one list, kept with the supplier (Ahmed, 25 Nov 2026).
    Any user can add a document at any time: before submit, at any workflow step, or after the supplier is active. Each notes where
    it was added. The site visit needs a "Site visit report" and contract signing a "Signed agreement" among them.
  -->
  <section class="fx-card fc-step-card">
    <header class="fc-sec-head">
      <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-paperclip"></span></span>
      <div>
        <h2>{{ tr('Documents') }}</h2>
        <p>{{ tr('Add any document at any time; it stays with the supplier.') }}</p>
      </div>
    </header>

    <div v-if="canAdd" class="scr-doc-add cf-doc-add">
      <select v-model="typeCode" class="fx-select" :aria-label="tr('Document type')">
        <option value="">{{ tr('Type (optional)') }}</option>
        <option v-for="t in types" :key="t.code" :value="t.code">{{ tr(t.name) }}</option>
      </select>
      <input v-model="title" class="fx-search" :placeholder="tr('Document title (optional)')" :aria-label="tr('Document title')">
      <label class="fx-btn fx-btn--teal scr-upload" :class="{ 'is-busy': busy }">
        <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-upload'" aria-hidden="true"></span>{{ tr('Add document') }}
        <input type="file" accept=".pdf,.png,.jpg,.jpeg,.tif,.tiff,.doc,.docx,.xls,.xlsx,.txt" :disabled="busy" @change="upload($event)">
      </label>
    </div>

    <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
    <div v-else class="scr-docs cf-docs">
      <div v-for="doc in docs" :key="doc.documentId" class="scr-doc">
        <span class="fa" :class="icon(doc.mimeType)" aria-hidden="true"></span>
        <div class="scr-doc-txt">
          <span class="cf-doc-line">
            <span v-if="doc.documentTypeCode" class="fx-chip fx-chip--info">{{ tr(typeName(doc.documentTypeCode)) }}</span>
            <button type="button" class="scr-doc-link" dir="auto" :title="tr('Open')" @click="viewing = doc">{{ doc.documentTitle }}</button>
          </span>
          <small>{{ doc.fileName }} · {{ size(doc.fileSizeBytes) }} · {{ tr(doc.stepName) }} · {{ doc.createdBy }} · {{ formatDateTime(doc.createdAt) }}</small>
        </div>
        <button type="button" class="fx-btn fx-btn--sm" @click="viewing = doc"><span class="fa fa-eye" aria-hidden="true"></span>{{ tr('View') }}</button>
        <button v-if="doc.canRemove && !readOnly" type="button" class="fx-btn fx-btn--sm fc-btn-reject" :disabled="busy" @click="remove(doc)">
          <span class="fa fa-times" aria-hidden="true"></span>{{ tr('Remove') }}
        </button>
      </div>
      <small v-if="!docs.length" class="scr-hint">{{ tr('No documents yet.') }}</small>
    </div>

    <ScrDocViewer v-if="viewing" :title="viewing.documentTitle" :file-name="viewing.fileName" :mime-type="viewing.mimeType"
                  :subtitle="`${tr(viewing.stepName)} · ${viewing.createdBy} · ${formatDateTime(viewing.createdAt)}`"
                  :load="() => cfSuppliersApi.documentFile(viewing!.documentId)" @close="viewing = null" />
  </section>
</template>

<script setup lang="ts">
import '@/modules/scr/components/scr.css'
import { computed, ref, watch } from 'vue'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDateTime } from '@/modules/scr/utils/dates'
import ScrDocViewer from '@/modules/scr/components/ScrDocViewer.vue'
import { errorText } from '@/modules/scr/api/scr.api'
import { cfSuppliersApi, type CfDocument, type CfOption } from '../api/cfSuppliers.api'

const props = defineProps<{
  applicationId: number
  statusCode: string
  types: CfOption[]
  /** Opened view-only: nothing can be added or removed. */
  readOnly: boolean
  /** Changes when the application changes, to read the list again. */
  version: number
}>()
const emit = defineEmits<{ (e: 'changed'): void }>()

const docs = ref<CfDocument[]>([])
const loading = ref(false)
const busy = ref(false)
const title = ref('')
const typeCode = ref('')
const viewing = ref<CfDocument | null>(null)
const canAdd = computed(() => !props.readOnly && props.statusCode !== 'WITHDRAWN')

const typeName = (code: string) => props.types.find((t) => t.code === code)?.name ?? code
const icon = (mime: string) => (mime === 'application/pdf' ? 'fa-file-pdf-o' : mime.startsWith('image/') ? 'fa-file-image-o'
  : mime.includes('word') ? 'fa-file-word-o' : mime.includes('sheet') || mime.includes('excel') ? 'fa-file-excel-o' : 'fa-file-o')
const size = (b: number) => (b >= 1048576 ? `${(b / 1048576).toFixed(1)} MB` : `${Math.max(1, Math.round(b / 1024))} KB`)

async function load() {
  loading.value = true
  try {
    docs.value = await cfSuppliersApi.documents(props.applicationId)
  } catch (err) {
    showError(errorText(err))
  } finally {
    loading.value = false
  }
}

async function upload(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file) return
  busy.value = true
  try {
    await cfSuppliersApi.addDocument(props.applicationId, file, title.value.trim(), typeCode.value || null)
    title.value = ''
    typeCode.value = ''
    showSuccess(`${tr('Document added')}: ${file.name}`)
    await load()
    emit('changed')
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function remove(doc: CfDocument) {
  if (!window.confirm(`${tr('Remove this document?')} ${doc.documentTitle}`)) return
  busy.value = true
  try {
    await cfSuppliersApi.removeDocument(doc.documentId)
    await load()
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

watch(() => [props.applicationId, props.version], load, { immediate: true })
</script>

<style scoped>
.cf-doc-add { flex-wrap: wrap; margin: 0 16px; }
.cf-doc-add .fx-select { max-width: 220px; }
.cf-doc-add .fx-search { flex: 1; min-width: 180px; }
.cf-docs { margin: 10px 16px 16px; }
.cf-doc-line { display: inline-flex; align-items: center; gap: 6px; flex-wrap: wrap; }
</style>
