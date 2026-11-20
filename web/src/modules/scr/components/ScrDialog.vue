<template>
  <!-- A form dialog in the CAPITRA modal style: icon, title and subtitle, the form, then the actions. -->
  <!-- closes only by its own buttons or Esc: a click (or a text selection ending) on the backdrop must never lose a typed form -->
  <div class="fc-modal-back" role="presentation">
    <section class="fc-modal scr-modal fc" role="dialog" aria-modal="true" :aria-label="title">
      <header class="fc-sec-head">
        <span class="fc-sec-ic" :class="{ 'fc-sec-ic--orange': orange }" aria-hidden="true"><span :class="['fa', icon]"></span></span>
        <div>
          <h2>{{ title }}</h2>
          <p v-if="subtitle">{{ subtitle }}</p>
        </div>
        <button type="button" class="fx-btn fx-btn--ghost fx-btn--sm" :aria-label="tr('Close')" @click="emit('close')"><span class="fa fa-times" aria-hidden="true"></span></button>
      </header>
      <div class="scr-modal-body">
        <slot />
      </div>
      <div class="fc-form-actions">
        <slot name="actions" />
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted } from 'vue'
import { tr } from '@/shared/layout/language'

defineProps<{ title: string; subtitle?: string; icon: string; orange?: boolean }>()
const emit = defineEmits<{ close: [] }>()
// Esc closes, except while typing in a field: there Esc belongs to the field (e.g. the browser's autofill suggestions)
const onKey = (e: KeyboardEvent) => {
  if (e.key !== 'Escape' || e.defaultPrevented) return
  const t = e.target as HTMLElement | null
  if (t && (t.closest('input, textarea, select, [contenteditable="true"]'))) return
  emit('close')
}
onMounted(() => window.addEventListener('keydown', onKey))
onBeforeUnmount(() => window.removeEventListener('keydown', onKey))
</script>
