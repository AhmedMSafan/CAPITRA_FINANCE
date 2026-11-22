<template>
  <!--
    The steps of a decision, in order: data entry, then each workflow step of each run (a returned decision starts a new round).
    Completed steps are view-only: who did them and when, the outcome, the comment and the documents added there. The open step
    takes any number of documents from whoever works it.
  -->
  <section class="fx-card fc-step-card">
    <header class="fc-sec-head">
      <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-sitemap"></span></span>
      <div>
        <h2>{{ tr('Workflow steps') }}</h2>
        <p>{{ tr('Completed steps are view-only, with what was decided and the documents added at each step.') }}</p>
      </div>
    </header>

    <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
    <ol v-else class="scr-steps">
      <li v-for="s in steps" :id="`step-${s.key}`" :key="s.key" class="scr-stepc"
          :class="{ 'scr-stepc--done': s.status === 'DONE', 'scr-stepc--open': s.status === 'OPEN', 'scr-stepc--focus': s.key === focused, 'scr-stepc--back': s.action === 'REJECT' }">
        <span class="scr-stepc-dot" aria-hidden="true"><span class="fa" :class="s.status === 'OPEN' ? 'fa-circle-o' : s.action === 'REJECT' ? 'fa-undo' : 'fa-check'"></span></span>
        <div class="scr-stepc-body">
          <div class="scr-stepc-head">
            <b>{{ tr(s.stepName) }}</b>
            <span v-if="rounds > 1" class="fx-chip fx-chip--plain">{{ tr('Round') }} {{ s.round }}</span>
            <span v-if="s.status === 'OPEN'" class="fx-chip fx-chip--warn">{{ tr('Open step') }}</span>
            <span v-else class="fx-chip" :class="s.action === 'REJECT' ? 'fx-chip--warn' : 'fx-chip--ok'">{{ tr(ACTIONS[s.action ?? ''] ?? 'Completed') }}</span>
            <span v-if="s.status === 'DONE'" class="fx-chip fx-chip--plain"><span class="fa fa-eye" aria-hidden="true"></span>{{ tr('View only') }}</span>
            <small v-if="s.doneBy" class="scr-stepc-who"><span class="fa fa-user-o" aria-hidden="true"></span> {{ s.doneBy }}<template v-if="s.doneAt"> · {{ formatDateTime(s.doneAt) }}</template></small>
          </div>

          <p v-if="s.comment" class="scr-stepc-comment" dir="auto"><span class="fa fa-comment-o" aria-hidden="true"></span> {{ s.comment }}</p>

          <div class="scr-docs">
            <div v-for="doc in s.documents" :key="doc.documentId" class="scr-doc">
              <span class="fa" :class="icon(doc.mimeType)" aria-hidden="true"></span>
              <div class="scr-doc-txt">
                <button type="button" class="scr-doc-link" dir="auto" :title="tr('Open')" @click="openDoc(doc)">{{ doc.documentTitle }}</button>
                <small>{{ doc.fileName }} · {{ size(doc.fileSizeBytes) }} · {{ doc.createdBy }} · {{ formatDateTime(doc.createdAt) }}</small>
              </div>
              <button type="button" class="fx-btn fx-btn--sm" @click="openDoc(doc)"><span class="fa fa-eye" aria-hidden="true"></span>{{ tr('View') }}</button>
              <button v-if="doc.canRemove && !readOnly && s.status === 'OPEN'" type="button" class="fx-btn fx-btn--sm fc-btn-reject" :disabled="busy" @click="remove(doc)">
                <span class="fa fa-times" aria-hidden="true"></span>{{ tr('Remove') }}
              </button>
            </div>
            <small v-if="!s.documents.length" class="scr-hint">{{ tr('No documents at this step.') }}</small>
          </div>

          <!-- the open step: add documents -->
          <div v-if="canAdd(s)" class="scr-doc-add">
            <input v-model="title" class="fx-search" :placeholder="tr('Document title (optional)')" :aria-label="tr('Document title')">
            <label class="fx-btn fx-btn--teal scr-upload" :class="{ 'is-busy': busy }">
              <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-paperclip'" aria-hidden="true"></span>{{ tr('Add document') }}
              <input type="file" accept=".pdf,.png,.jpg,.jpeg,.tif,.tiff,.doc,.docx,.xls,.xlsx,.txt" :disabled="busy" @change="upload($event)">
            </label>
          </div>
        </div>
      </li>
    </ol>
    <ScrDocViewer v-if="viewing" :title="viewing.documentTitle" :file-name="viewing.fileName" :mime-type="viewing.mimeType"
                  :subtitle="`${tr(viewing.stepName || stepName(viewing.stepCode))} · ${viewing.createdBy} · ${formatDateTime(viewing.createdAt)}`"
                  :load="() => scrApi.documentFile(viewing!.documentId)" @close="viewing = null" />
  </section>
</template>

<script setup lang="ts">
import { computed, nextTick, ref, watch } from 'vue'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDateTime } from '../utils/dates'
import ScrDocViewer from './ScrDocViewer.vue'
import { errorText, scrApi, type ScrDocument, type ScrStepView } from '../api/scr.api'

const props = defineProps<{
  decisionId: number
  /** The signed-in user may add documents at data entry. */
  dataEntryOpen: boolean
  /** The user's task on this decision (the step it is at), when opened from the inbox. */
  inboxId: number | null
  taskStepCode: string | null
  /** Opened view-only (?view=1): nothing can be added or removed. */
  readOnly: boolean
  /** Step code to open on (?step=): its last completion is highlighted. */
  focusStep: string | null
  /** Changes when the decision changes, to read the steps again. */
  version: number
}>()

const ACTIONS: Record<string, string> = { ACCEPT: 'Accepted', REJECT: 'Returned', RUN: 'Workflow started' }

const steps = ref<ScrStepView[]>([])
const loading = ref(false)
const busy = ref(false)
const title = ref('')
const rounds = computed(() => Math.max(1, ...steps.value.map((s) => s.round)))
const focused = computed(() => {
  if (!props.focusStep) return null
  const done = steps.value.filter((s) => s.stepCode === props.focusStep && s.status === 'DONE')
  return done.length ? done[done.length - 1]!.key : null
})

function canAdd(s: ScrStepView): boolean {
  if (props.readOnly || s.status !== 'OPEN') return false
  if (s.stepCode === 'DATA_ENTRY') return props.dataEntryOpen && !props.inboxId
  return !!props.inboxId && s.stepCode === props.taskStepCode
}

const icon = (mime: string) => (mime === 'application/pdf' ? 'fa-file-pdf-o' : mime.startsWith('image/') ? 'fa-file-image-o'
  : mime.includes('word') ? 'fa-file-word-o' : mime.includes('sheet') || mime.includes('excel') ? 'fa-file-excel-o' : 'fa-file-o')
const size = (b: number) => (b >= 1048576 ? `${(b / 1048576).toFixed(1)} MB` : `${Math.max(1, Math.round(b / 1024))} KB`)

async function load() {
  loading.value = true
  try {
    steps.value = await scrApi.steps(props.decisionId)
    if (focused.value) {
      await nextTick()
      document.getElementById(`step-${focused.value}`)?.scrollIntoView({ behavior: 'smooth', block: 'center' })
    }
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
    await scrApi.addDocument(props.decisionId, props.inboxId, file, title.value.trim())
    title.value = ''
    showSuccess(`${tr('Document added')}: ${file.name}`)
    await load()
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function remove(doc: ScrDocument) {
  if (!window.confirm(`${tr('Remove this document?')} ${doc.documentTitle}`)) return
  busy.value = true
  try {
    await scrApi.removeDocument(doc.documentId)
    await load()
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

/** The document shown in the pop-up viewer (same page, with zoom). */
const viewing = ref<ScrDocument | null>(null)
const openDoc = (doc: ScrDocument) => { viewing.value = doc }
const stepName = (code: string) => steps.value.find((s) => s.stepCode === code)?.stepName ?? code

watch(() => [props.decisionId, props.version], load, { immediate: true })
</script>
