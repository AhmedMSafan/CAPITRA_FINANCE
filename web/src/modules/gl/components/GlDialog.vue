<script setup lang="ts">
// Modal form used by the GL setup pages: header, error line, a two-column form and Cancel / Save.
defineProps<{
  title: string
  subtitle?: string
  saving?: boolean
  error?: string
  saveLabel?: string
  readonly?: boolean
  wide?: boolean
  /** A dialog that only shows information and its own buttons: Close, no Save. */
  noSave?: boolean
}>()

const emit = defineEmits<{ close: []; save: [] }>()
</script>

<template>
  <div class="cp-modal-overlay" @click.self="!saving && emit('close')" @keydown.esc="!saving && emit('close')">
    <section class="cp-modal" :class="{ 'gl-dialog--wide': wide }" role="dialog" aria-modal="true">
      <header class="cp-modal__header">
        <div>
          <h3>{{ title }}</h3>
          <p v-if="subtitle">{{ subtitle }}</p>
        </div>
        <button type="button" class="cp-modal__close" :disabled="saving" aria-label="Close" @click="emit('close')">
          <i class="pi pi-times"></i>
        </button>
      </header>
      <div class="cp-modal__body">
        <div v-if="error" class="cp-error" role="alert">{{ error }}</div>
        <fieldset class="cp-form" :disabled="saving || readonly">
          <slot></slot>
        </fieldset>
      </div>
      <footer class="cp-modal__footer">
        <slot name="footer-start"></slot>
        <span class="cp-spacer"></span>
        <button type="button" class="cp-btn" :disabled="saving" @click="emit('close')">{{ readonly || noSave ? 'Close' : 'Cancel' }}</button>
        <button v-if="!readonly && !noSave" type="button" class="cp-btn cp-btn--primary" :disabled="saving" @click="emit('save')">
          <i :class="saving ? 'pi pi-spin pi-spinner' : 'pi pi-check'"></i>
          <span>{{ saving ? 'Saving...' : saveLabel ?? 'Save' }}</span>
        </button>
      </footer>
    </section>
  </div>
</template>

<style scoped>
.gl-dialog--wide { width: min(900px, calc(100vw - 36px)); }
:deep(.cp-form__section:first-child) { margin-top: 0; padding-top: 0; border-top: 0; }
:deep(.gl-check) { display: flex; align-items: center; gap: 8px; min-height: 42px; color: var(--cap-ink-soft); font-size: 13px; cursor: pointer; }
:deep(.gl-check input) { width: 16px; height: 16px; accent-color: var(--cap-primary); }
:deep(.gl-checks) { grid-column: 1 / -1; display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 2px 18px; }
:deep(.cp-field input.upper) { text-transform: uppercase; }
:deep(.cp-field [dir='rtl']) { text-align: right; }
@media (max-width: 760px) { :deep(.gl-checks) { grid-template-columns: 1fr; } }
</style>
