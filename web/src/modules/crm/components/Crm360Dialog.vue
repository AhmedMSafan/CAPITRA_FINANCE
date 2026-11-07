<template>
  <!-- A small dialog shell for Customer 360 (compliance, bank account, verify, role): header with icon and title, body slot,
       footer slot. Closing is refused while the parent is busy. -->
  <Teleport to="body">
    <div v-if="visible" class="fx fc fc-modal-back" role="presentation" @click.self="close">
      <section class="fc-modal c3-dlg" :style="{ width: `min(${width}px, calc(100vw - 32px))` }" role="dialog" aria-modal="true" :aria-labelledby="id">
        <header class="fc-docs-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa" :class="icon"></span></span>
          <div>
            <h2 :id="id">{{ title }}</h2>
            <small v-if="subtitle">{{ subtitle }}</small>
          </div>
          <button type="button" class="fx-btn fx-btn--sm" :disabled="busy" :aria-label="tr('Close')" @click="close"><span class="fa fa-times" aria-hidden="true"></span></button>
        </header>
        <slot />
        <footer class="c3-foot"><slot name="footer" /></footer>
      </section>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { tr } from '@/shared/layout/language'

const props = withDefaults(defineProps<{ title: string; subtitle?: string; icon?: string; busy?: boolean; width?: number }>(),
  { subtitle: '', icon: 'fa-pencil', busy: false, width: 760 })
const visible = defineModel<boolean>('visible', { required: true })
const id = `c3-dlg-${Math.random().toString(36).slice(2, 8)}`

function close() {
  if (!props.busy) visible.value = false
}
</script>

<style scoped>
.c3-dlg { max-height: calc(100vh - 48px); overflow: auto; display: flex; flex-direction: column; gap: 16px; }
.c3-foot { display: flex; justify-content: flex-end; gap: 8px; margin: 0; }
</style>
