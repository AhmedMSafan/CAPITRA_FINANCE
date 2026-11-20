<script setup lang="ts">
// A region of page items that are not a form's columns: the filters of a report, or the fields of a page whose
// processes are hand-written (a dialog of page items). Submit page (a Go button, or a select list whose Page Action
// on Selection is Submit) sends the values of every region of the page; the page's own processes run with them.
import { computed, inject, reactive, ref, watch } from 'vue'
import { http } from '@/shared/api/http'
import { buttonIcon } from './buttonIcon'
import { pageValuesKey, resolveSet, shown, type WizardButton, type WizardItem, type WizardRegion } from './wizard'

const props = defineProps<{ app: number; page: number; region: WizardRegion; items: Record<string, string> }>()
const emit = defineEmits<{ submit: [values: Record<string, string>, request: string]; open: [page: number | null, items: Record<string, string>] }>()

// the page's items as the user is changing them, shared by all the regions of items of the page
const values = inject(pageValuesKey, () => reactive<Record<string, string>>({}), true)
const lovs = ref<Record<string, [string | null, string][]>>({})
const visible = computed(() => (props.region.items ?? []).filter((i) => i.type !== 'NATIVE_HIDDEN'))
// the grid's rows: an item starts a row unless it is on the same line as the one before (Start New Row: No)
const rows = computed(() => {
  const out: WizardItem[][] = []
  for (const item of visible.value) {
    if (item.sameLine && out.length) out[out.length - 1]!.push(item)
    else out.push([item])
  }
  return out.length ? out : [[]]
})
const buttons = computed(() => (props.region.buttons ?? []).filter((x) => shown(x, props.items)))

async function loadLovs() {
  if (props.region.items?.some((i) => i.lov))
    lovs.value = await http.post(`/api/apex/${props.app}/${props.page}/LOVS`, { x01: JSON.stringify({ ...props.items, ...values }) })
}

function submit(request = '') {
  emit('submit', { ...values }, request)
}

// a list of values can depend on another item (projects of the developer): the lists are read again
function changed(item: WizardItem) {
  if (item.submit) submit(item.name)
  else if (item.lov) void loadLovs()
}

// File Browse: the file goes to the user's APEX session now; the item's value is its name there (APEX_APPLICATION_TEMP_FILES)
async function upload(item: WizardItem, event: Event) {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (!file) {
    values[item.name] = ''
    return
  }
  const form = new FormData()
  form.append('file', file)
  const res = await http.upload<{ name: string }>(`/api/apex/${props.app}/${props.page}/files`, form)
  values[item.name] = res.name
}

function press(button: WizardButton) {
  if (button.action === 'SUBMIT') submit(button.name)
  else emit('open', button.page, resolveSet(button.set, {}, props.items))
}

watch(() => JSON.stringify(props.items), loadLovs, { immediate: true })
</script>

<template>
  <div :id="region.staticId" class="t-Region" :class="[region.cls, { 't-Region--plain': region.hideHeader }]">
    <div v-if="!region.hideHeader" class="t-Region-header"><h2 class="t-Region-title">{{ region.title }}</h2></div>
    <div class="t-Region-body">
      <div class="t-Form">
        <div v-for="(row, n) in rows" :key="n" class="apx-row">
          <div v-for="item in row" :id="`${item.name}_CONTAINER`" :key="item.name" class="t-Form-fieldContainer" :class="{ 'is-required': item.required }">
            <label class="t-Form-label" :for="item.name">{{ item.label ?? item.name }}</label>
            <select v-if="item.lov" :id="item.name" v-model="values[item.name]" class="t-Form-field" @change="changed(item)">
              <option value=""></option>
              <option v-for="o in lovs[item.name] ?? []" :key="o[1]" :value="o[1]">{{ o[0] }}</option>
            </select>
            <select v-else-if="item.type === 'NATIVE_YES_NO'" :id="item.name" v-model="values[item.name]" class="t-Form-field" @change="changed(item)">
              <option value="N">No</option>
              <option value="Y">Yes</option>
            </select>
            <input v-else-if="item.type === 'NATIVE_FILE'" :id="item.name" type="file" class="t-Form-field" @change="upload(item, $event)" />
            <textarea v-else-if="item.type === 'NATIVE_TEXTAREA'" :id="item.name" v-model="values[item.name]" class="t-Form-field" rows="4"></textarea>
            <input v-else-if="item.type === 'NATIVE_DATE_PICKER_APEX'" :id="item.name" v-model="values[item.name]" type="date" class="t-Form-field" />
            <input
              v-else-if="item.type === 'NATIVE_NUMBER_FIELD'"
              :id="item.name"
              v-model="values[item.name]"
              type="text"
              inputmode="decimal"
              class="t-Form-field u-textEnd"
              @keydown.enter.prevent="submit()"
            />
            <input v-else :id="item.name" v-model="values[item.name]" type="text" class="t-Form-field" @keydown.enter.prevent="submit()" />
          </div>
          <template v-if="n === rows.length - 1">
            <div v-for="b in buttons" :key="b.name" class="t-Form-fieldContainer t-Form-fieldContainer--button">
              <button :id="b.staticId" type="button" class="t-Button" :class="{ 't-Button--hot': b.action === 'SUBMIT' }" @click="press(b)"><i v-if="buttonIcon(b.name, b.action)" :class="buttonIcon(b.name, b.action)" aria-hidden="true"></i>{{ b.label }}</button>
            </div>
          </template>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.apx-row {
  display: flex;
  flex-wrap: wrap;
  gap: 0 16px;
  align-items: flex-end;
}
.apx-row .t-Form-fieldContainer {
  flex: 1 1 180px;
}
.t-Form-fieldContainer--button {
  flex: 0 0 auto !important;
}
.is-required .t-Form-label::after {
  content: ' *';
  color: #c43325;
}
.u-textEnd {
  text-align: end;
}
</style>
