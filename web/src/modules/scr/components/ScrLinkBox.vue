<template>
  <!--
    The fallback when an activation or reset e-mail does not arrive: make the link and copy it, to send another way. Making it
    voids every earlier link; it is recorded in the e-mail log.
  -->
  <div class="scr-linkbox">
    <button v-if="!link" type="button" class="fx-btn fx-btn--teal fx-btn--sm" :disabled="busy" @click="make">
      <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-link']" aria-hidden="true"></span>{{ tr('Make the link to copy') }}
    </button>
    <template v-else>
      <div class="scr-temp">
        <code dir="ltr" class="scr-link-code">{{ link }}</code>
        <button type="button" class="fx-btn fx-btn--teal fx-btn--sm" @click="copy"><span :class="['fa', copied ? 'fa-check' : 'fa-copy']" aria-hidden="true"></span>{{ tr(copied ? 'Copied' : 'Copy') }}</button>
      </div>
      <small class="scr-hint">{{ tr('Send it only to the user it belongs to. It works once, expires in 48 hours, and earlier links stopped working.') }}</small>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { showError } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { errorText, scrApi } from '../api/scr.api'

const props = defineProps<{ companyId: number; userId: string }>()
const link = ref('')
const busy = ref(false)
const copied = ref(false)

async function make() {
  busy.value = true
  try {
    link.value = (await scrApi.copyUserLink(props.companyId, props.userId)).link
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function copy() {
  try {
    await navigator.clipboard.writeText(link.value)
    copied.value = true
  } catch {
    copied.value = false
  }
}
</script>
