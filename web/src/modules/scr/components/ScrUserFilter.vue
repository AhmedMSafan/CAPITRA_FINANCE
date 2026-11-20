<template>
  <!-- The user filter of a Screening list: Entered by me, Waiting for me (workflow lists), All users, or one user. -->
  <label class="fc-field"><span>{{ tr('User') }}</span>
    <select :value="modelValue" class="fx-select" @change="emit('update:modelValue', ($event.target as HTMLSelectElement).value)">
      <option :value="MINE">{{ tr('Entered by me') }} ({{ me }})</option>
      <option v-if="withWaiting" :value="WAITING">{{ tr('Waiting for me') }} ({{ waitingCount }})</option>
      <option value="">{{ tr('All users') }}</option>
      <option v-for="u in users" :key="u" :value="u">{{ u }}</option>
    </select>
  </label>
</template>

<script setup lang="ts">
import { tr } from '@/shared/layout/language'
import { MINE, WAITING } from '../utils/userFilter'

defineProps<{ modelValue: string; me: string; users: string[]; withWaiting?: boolean; waitingCount?: number }>()
const emit = defineEmits<{ 'update:modelValue': [value: string] }>()
</script>
