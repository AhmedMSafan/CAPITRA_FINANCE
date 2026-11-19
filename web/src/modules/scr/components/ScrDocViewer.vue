<template>
  <!--
    A stored document shown in a pop-up on the same page (not a new browser tab), with zoom: 50% to 200% and Fit width.
    PDFs use the browser's PDF view at the chosen zoom, images and text files are scaled here; Word and Excel files cannot be
    shown in a browser, so they offer Download.
  -->
  <div class="fc-modal-back scr-viewer-back" role="presentation" @click.self="emit('close')">
    <section class="fc-modal fc scr-viewer" role="dialog" aria-modal="true" :aria-label="title">
      <header class="scr-viewer-head">
        <span class="fc-sec-ic" aria-hidden="true"><span :class="['fa', icon]"></span></span>
        <div class="scr-viewer-title">
          <h2 dir="auto">{{ title }}</h2>
          <small dir="auto">{{ fileName }}<template v-if="subtitle"> · {{ subtitle }}</template></small>
        </div>

        <div v-if="kind !== 'other'" class="scr-zoom" role="group" :aria-label="tr('Zoom')">
          <button type="button" class="fx-btn fx-btn--sm" :disabled="zoomIndex <= 0" :title="tr('Zoom out')" @click="step(-1)"><span class="fa fa-search-minus" aria-hidden="true"></span></button>
          <select v-model="zoom" class="fx-select scr-zoom-select" :aria-label="tr('Zoom')">
            <option value="fit">{{ tr('Fit width') }}</option>
            <option v-for="z in ZOOMS" :key="z" :value="String(z)">{{ z }}%</option>
          </select>
          <button type="button" class="fx-btn fx-btn--sm" :disabled="zoomIndex >= ZOOMS.length - 1" :title="tr('Zoom in')" @click="step(1)"><span class="fa fa-search-plus" aria-hidden="true"></span></button>
        </div>

        <button type="button" class="fx-btn fx-btn--sm" :disabled="!url" @click="download"><span class="fa fa-download" aria-hidden="true"></span>{{ tr('Download') }}</button>
        <button type="button" class="fx-btn fx-btn--ghost fx-btn--sm" :aria-label="tr('Close')" @click="emit('close')"><span class="fa fa-times" aria-hidden="true"></span></button>
      </header>

      <div class="scr-viewer-body" :class="`scr-viewer-body--${kind}`">
        <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading...') }}</div>
        <div v-else-if="failed" class="fx-empty"><span class="fa fa-exclamation-triangle" aria-hidden="true"></span>{{ failed }}</div>
        <template v-else-if="url">
          <!-- the key reloads the PDF view when the zoom changes -->
          <iframe v-if="kind === 'pdf'" :key="pdfSrc" :src="pdfSrc" class="scr-viewer-pdf" :title="title"></iframe>
          <div v-else-if="kind === 'image'" class="scr-viewer-scroll">
            <img :src="url" :alt="title" :style="imageStyle" @load="onImage">
          </div>
          <div v-else-if="kind === 'text'" class="scr-viewer-scroll">
            <pre class="scr-viewer-text" dir="auto" :style="{ fontSize: `${13 * scale}px` }">{{ text }}</pre>
          </div>
          <div v-else class="fx-empty scr-viewer-other">
            <span class="fa fa-file-o" aria-hidden="true"></span>
            <p>{{ tr('This kind of file cannot be shown in the browser. Download it to open it.') }}</p>
            <button type="button" class="fx-btn fx-btn--primary" @click="download"><span class="fa fa-download" aria-hidden="true"></span>{{ tr('Download') }}</button>
          </div>
        </template>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import { errorText } from '../api/scr.api'

const props = defineProps<{
  title: string
  fileName: string
  mimeType: string
  subtitle?: string
  /** Reads the stored file. */
  load: () => Promise<Blob>
}>()
const emit = defineEmits<{ close: [] }>()

const ZOOMS = [50, 75, 100, 125, 150, 200]
const zoom = ref<string>('fit')
const url = ref<string | null>(null)
const text = ref('')
const loading = ref(true)
const failed = ref<string | null>(null)
const natural = ref(0)

const mime = computed(() => props.mimeType.toLowerCase())
const kind = computed(() => (mime.value === 'application/pdf' ? 'pdf' : mime.value.startsWith('image/') && mime.value !== 'image/tiff' ? 'image'
  : mime.value.startsWith('text/') ? 'text' : 'other'))
const icon = computed(() => ({ pdf: 'fa-file-pdf-o', image: 'fa-file-image-o', text: 'fa-file-text-o', other: 'fa-file-o' })[kind.value])
const scale = computed(() => (zoom.value === 'fit' ? 1 : Number(zoom.value) / 100))
const zoomIndex = computed(() => (zoom.value === 'fit' ? ZOOMS.indexOf(100) : ZOOMS.indexOf(Number(zoom.value))))
const pdfSrc = computed(() => (url.value ? `${url.value}#${zoom.value === 'fit' ? 'view=FitH' : `zoom=${zoom.value}`}` : ''))
const imageStyle = computed(() => (zoom.value === 'fit' || !natural.value
  ? { maxWidth: '100%', height: 'auto' }
  : { width: `${Math.round(natural.value * scale.value)}px`, maxWidth: 'none', height: 'auto' }))

function step(by: number) {
  const next = Math.min(ZOOMS.length - 1, Math.max(0, zoomIndex.value + by))
  zoom.value = String(ZOOMS[next])
}

function onImage(e: Event) {
  natural.value = (e.target as HTMLImageElement).naturalWidth
}

function download() {
  if (!url.value) return
  const a = document.createElement('a')
  a.href = url.value
  a.download = props.fileName
  a.click()
}

const onKey = (e: KeyboardEvent) => {
  if (e.key === 'Escape') emit('close')
  else if ((e.ctrlKey || e.metaKey) && (e.key === '+' || e.key === '=')) { e.preventDefault(); step(1) }
  else if ((e.ctrlKey || e.metaKey) && e.key === '-') { e.preventDefault(); step(-1) }
}

onMounted(async () => {
  window.addEventListener('keydown', onKey)
  try {
    const blob = await props.load()
    // the stored type decides how it is shown; a blob without one would not open as a PDF
    const typed = blob.type ? blob : new Blob([blob], { type: props.mimeType })
    url.value = URL.createObjectURL(typed)
    if (kind.value === 'text') text.value = await typed.text()
  } catch (err) {
    failed.value = errorText(err)
  } finally {
    loading.value = false
  }
})

onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKey)
  if (url.value) URL.revokeObjectURL(url.value)
})
</script>
