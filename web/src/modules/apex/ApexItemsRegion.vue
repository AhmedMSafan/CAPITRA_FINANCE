<script setup lang="ts">
// A region of page items that are not a form's columns: the filters of a report. Submit page (a Go button, or a
// select list whose Page Action on Selection is Submit) keeps the values as the page's items and the reports run again.
import { computed, reactive, ref, watch } from 'vue'
import { http } from '@/shared/api/http'
import { resolveSet, type WizardButton, type WizardItem, type WizardRegion } from './wizard'

const props = defineProps<{ app: number; page: number; region: WizardRegion; items: Record<string, string> }>()
const emit = defineEmits<{ submit: [values: Record<string, string>]; open: [page: number | null, items: Record<string, string>] }>()

const values = reactive<Record<string, string>>({})
const lovs = ref<Record<string, [string | null, string][]>>({})
const visible = computed(() => (props.region.items ?? []).filter((i) => i.type !== 'NATIVE_HIDDEN'))
const inline = computed(() => (props.region.items ?? []).some((i) => i.sameLine))

async function load() {
  for (const item of props.region.items ?? []) values[item.name] = props.items[item.name] ?? ''
  if (props.region.items?.some((i) => i.lov))
    lovs.value = await http.post(`/api/apex/${props.app}/${props.page}/LOVS`, { x01: JSON.stringify(props.items) })
}

function submit() {
  emit('submit', { ...values })
}

function changed(item: WizardItem) {
  if (item.submit) submit()
}

function press(button: WizardButton) {
  if (button.action === 'SUBMIT') submit()
  else emit('open', button.page, resolveSet(button.set, {}, props.items))
}

watch(() => JSON.stringify(props.items), load, { immediate: true })
</script>

<template>
  <div class="t-Region" :class="{ 't-Region--plain': region.hideHeader }">
    <div v-if="!region.hideHeader" class="t-Region-header"><h2 class="t-Region-title">{{ region.title }}</h2></div>
    <div class="t-Region-body">
      <div class="t-Form" :class="{ 't-Form--inline': inline }">
        <div v-for="item in visible" :key="item.name" class="t-Form-fieldContainer">
          <label class="t-Form-label" :for="item.name">{{ item.label ?? item.name }}</label>
          <select v-if="item.lov" :id="item.name" v-model="values[item.name]" class="t-Form-field" @change="changed(item)">
            <option value=""></option>
            <option v-for="o in lovs[item.name] ?? []" :key="o[1]" :value="o[1]">{{ o[0] }}</option>
          </select>
          <select v-else-if="item.type === 'NATIVE_YES_NO'" :id="item.name" v-model="values[item.name]" class="t-Form-field" @change="changed(item)">
            <option value="N">No</option>
            <option value="Y">Yes</option>
          </select>
          <input v-else-if="item.type === 'NATIVE_DATE_PICKER_APEX'" :id="item.name" v-model="values[item.name]" type="date" class="t-Form-field" />
          <input v-else :id="item.name" v-model="values[item.name]" type="text" class="t-Form-field" @keydown.enter.prevent="submit" />
        </div>
        <div v-for="b in region.buttons ?? []" :key="b.name" class="t-Form-fieldContainer t-Form-fieldContainer--button">
          <button type="button" class="t-Button" :class="{ 't-Button--hot': b.action === 'SUBMIT' }" @click="press(b)">{{ b.label }}</button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.t-Form--inline {
  display: flex;
  flex-wrap: wrap;
  gap: 0 16px;
  align-items: flex-end;
}
.t-Form--inline .t-Form-fieldContainer {
  flex: 1 1 180px;
}
.t-Form-fieldContainer--button {
  flex: 0 0 auto !important;
}
</style>
