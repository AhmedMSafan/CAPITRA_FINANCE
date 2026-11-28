<template>
  <!-- A finance's documents (27 Nov 2026), the same list as the supplier documents: any user adds one at any time, noting the step;
       the delivery note the customer signed is added at Delivery and customer receipt. -->
  <section class="fx-card ld">
    <header class="ld-head">
      <h3><span class="fa fa-paperclip" aria-hidden="true"></span>{{ tr('Documents') }} <span class="ld-cnt">{{ docs.length }}</span></h3>
      <div class="ld-add">
        <select v-model="typeCode" class="fx-select" :aria-label="tr('Document type')">
          <option value="">{{ tr('Type (optional)') }}</option>
          <option v-for="t in TYPES" :key="t[0]" :value="t[0]">{{ tr(t[1]) }}</option>
        </select>
        <input v-model="title" class="fx-search" :placeholder="tr('Document title (optional)')" :aria-label="tr('Document title')">
        <label class="fx-btn ld-upload" :class="{ 'is-busy': busy }">
          <span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-upload'" aria-hidden="true"></span>{{ tr('Add document') }}
          <input type="file" accept=".pdf,.png,.jpg,.jpeg,.tif,.tiff,.doc,.docx,.xls,.xlsx,.txt" :disabled="busy" @change="upload($event)">
        </label>
      </div>
    </header>
    <div v-for="d in docs" :key="d.documentId" class="ld-row">
      <span class="fa fa-file-o" aria-hidden="true"></span>
      <span v-if="d.documentTypeCode" class="fx-chip fx-chip--info">{{ tr(typeName(d.documentTypeCode)) }}</span>
      <button type="button" class="ld-link" dir="auto" @click="viewing = d">{{ d.documentTitle }}</button>
      <small class="c3-muted">{{ d.fileName }} · {{ tr(d.stepName) }} · {{ d.createdBy }} · {{ formatDateTime(d.createdAt) }}</small>
      <span class="ld-grow"></span>
      <button type="button" class="fx-btn fx-btn--sm" @click="viewing = d"><span class="fa fa-eye" aria-hidden="true"></span>{{ tr('View') }}</button>
      <button v-if="d.canRemove" type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="remove(d)"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Remove') }}</button>
    </div>
    <small v-if="!docs.length" class="c3-muted">{{ tr('No documents yet.') }}</small>
    <ScrDocViewer v-if="viewing" :title="viewing.documentTitle" :file-name="viewing.fileName" :mime-type="viewing.mimeType"
                  :subtitle="`${tr(viewing.stepName)} · ${viewing.createdBy} · ${formatDateTime(viewing.createdAt)}`"
                  :load="() => loansApi.documentFile(viewing!.documentId)" @close="viewing = null" />
  </section>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import ScrDocViewer from '@/modules/scr/components/ScrDocViewer.vue'
import { loansApi, type LoanDocument } from '../api/loans.api'

const props = defineProps<{ loanId: number; version: number }>()
const emit = defineEmits<{ changed: [] }>()

const TYPES: [string, string][] = [['DELIVERY_NOTE', 'Signed delivery note'], ['CONTRACT', 'Contract'], ['OTHER', 'Other']]
const typeName = (code: string) => TYPES.find((t) => t[0] === code)?.[1] ?? code
const docs = ref<LoanDocument[]>([])
const busy = ref(false)
const title = ref('')
const typeCode = ref('')
const viewing = ref<LoanDocument | null>(null)
const message = (err: unknown) => (err instanceof Error ? err.message : String(err))

async function load() {
  try {
    docs.value = await loansApi.documents(props.loanId)
  } catch (err) {
    showError(message(err))
  }
}

async function upload(e: Event) {
  const input = e.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file) return
  busy.value = true
  try {
    await loansApi.addDocument(props.loanId, file, title.value.trim(), typeCode.value || null)
    title.value = ''
    typeCode.value = ''
    showSuccess(`${tr('Document added')}: ${file.name}`)
    await load()
    emit('changed')
  } catch (err) {
    showError(message(err))
  } finally {
    busy.value = false
  }
}

async function remove(d: LoanDocument) {
  if (!window.confirm(`${tr('Remove this document?')} ${d.documentTitle}`)) return
  busy.value = true
  try {
    await loansApi.removeDocument(d.documentId)
    await load()
  } catch (err) {
    showError(message(err))
  } finally {
    busy.value = false
  }
}

watch(() => [props.loanId, props.version], load, { immediate: true })
</script>

<style scoped>
.ld { padding: 14px 18px; }
.ld-head { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; margin-bottom: 8px; }
.ld-head h3 { display: flex; align-items: center; gap: 8px; margin: 0; font-size: 14px; color: var(--fin-teal-700); flex: 1; }
.ld-cnt { padding: 0 7px; border-radius: 999px; background: #eef1f3; color: #34495a; font-size: 11.5px; }
.ld-add { display: flex; flex-wrap: wrap; align-items: center; gap: 8px; }
.ld-add .fx-select { max-width: 200px; }
.ld-add .fx-search { width: 220px; }
.ld-upload { position: relative; overflow: hidden; cursor: pointer; }
.ld-upload input { position: absolute; inset: 0; opacity: 0; cursor: pointer; }
.ld-row { display: flex; flex-wrap: wrap; align-items: center; gap: 8px; padding: 8px 0; border-top: 1px solid #f0f2f4; font-size: 13px; }
.ld-link { padding: 0; border: 0; background: none; color: var(--fin-teal-700); font: 600 13px/1.3 inherit; cursor: pointer; }
.ld-grow { flex: 1; }
</style>
