<template>
  <!-- A form dialog in the CAPITRA modal style: icon, title and subtitle, the form, then the actions. -->
  <div class="fc-modal-back" role="presentation" @click.self="emit('close')">
    <section class="fc-modal scr-modal fc" role="dialog" aria-modal="true" :aria-label="title">
      <header class="fc-sec-head">
        <span class="fc-sec-ic" :class="{ 'fc-sec-ic--orange': orange }" aria-hidden="true"><span :class="['fa', icon]"></span></span>
        <div>
          <h2>{{ title }}</h2>
          <p v-if="subtitle">{{ subtitle }}</p>
        </div>
        <button type="button" class="fx-btn fx-btn--ghost fx-btn--sm" :aria-label="tr('Close')" @click="emit('close')"><span class="fa fa-times" aria-hidden="true"></span></button>
      </header>
      <slot />
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
const onKey = (e: KeyboardEvent) => { if (e.key === 'Escape') emit('close') }
onMounted(() => window.addEventListener('keydown', onKey))
onBeforeUnmount(() => window.removeEventListener('keydown', onKey))
</script>
