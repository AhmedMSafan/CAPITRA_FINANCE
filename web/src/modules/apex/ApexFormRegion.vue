<script setup lang="ts">
// An APEX form on one table row, as the form wizard builds it: Initialize form loads the row of the primary key item,
// the Create / Apply Changes / Delete buttons run Process form (automatic row processing), Cancel leaves.
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { http } from '@/shared/api/http'
import { buttonIcon } from './buttonIcon'
import type { WizardButton, WizardItem, WizardRegion } from './wizard'

const props = defineProps<{
  app: number
  page: number
  region: WizardRegion
  pk: string
  /** The page items the link to this page set (P302_PORTFOLIO_ID): sent to Initialize form, and the values of empty items. */
  items?: Record<string, string>
  /** Blank region template (modal pages): no region header. */
  plain?: boolean
}>()
const emit = defineEmits<{ done: [request: string, id: string | null, message: string]; cancel: [button: WizardButton] }>()

const values = reactive<Record<string, string>>({})
const lovs = ref<Record<string, [string | null, string][]>>({})
const errors = reactive<Record<string, string>>({})
const message = ref('')
const busy = ref(false)

const visible = computed(() => (props.region.items ?? []).filter((i) => i.type !== 'NATIVE_HIDDEN'))
const buttons = computed(() =>
  (props.region.buttons ?? []).filter((b) => {
    if (b.name === 'CREATE') return !props.pk
    if (b.name === 'SAVE' || b.name === 'DELETE') return !!props.pk
    return true
  })
)
const left = computed(() => buttons.value.filter((b) => b.position === 'CLOSE' || b.position === 'DELETE'))
const right = computed(() => buttons.value.filter((b) => b.position !== 'CLOSE' && b.position !== 'DELETE'))

async function load() {
  const res = await http.post<{ ok: boolean; values: Record<string, string | null> }>(
    `/api/apex/${props.app}/${props.page}/FORM_LOAD_${props.region.key}`,
    { x01: props.pk, x02: JSON.stringify(props.items ?? {}) }
  )
  for (const item of props.region.items ?? []) {
    const loaded = item.column ? res.values?.[item.column] : null
    values[item.name] = loaded ?? props.items?.[item.name] ?? ''
  }
  for (const k of Object.keys(errors)) delete errors[k]
  await loadLovs()
}

// the select lists, after Initialize form: their queries bind the page items (P205_PORTFOLIO_ID)
async function loadLovs() {
  if (!props.region.items?.some((i) => i.lov)) return
  lovs.value = await http.post(`/api/apex/${props.app}/${props.page}/LOVS`, { x01: JSON.stringify({ ...props.items, ...values }) })
}

function display(item: WizardItem) {
  const v = values[item.name] ?? ''
  return lovs.value[item.name]?.find((o) => o[1] === v)?.[0] ?? v
}

// a file browse item: the file goes with the row as {name, type, data (base64)} for the page's upload process
function pick(item: WizardItem, e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) {
    values[item.name] = ''
    return
  }
  const reader = new FileReader()
  reader.onload = () => {
    values[item.name] = JSON.stringify({ name: file.name, type: file.type, data: String(reader.result).split(',')[1] ?? '' })
  }
  reader.readAsDataURL(file)
}

function label(item: WizardItem) {
  return item.label ?? item.name
}

function validate() {
  for (const k of Object.keys(errors)) delete errors[k]
  for (const item of visible.value) {
    if (item.kind === 'Audit' || item.type === 'NATIVE_DISPLAY_ONLY') continue
    const v = (values[item.name] ?? '').trim()
    if (item.required && !v) errors[item.name] = `${label(item)} must have some value.`
    else if (v && item.type === 'NATIVE_NUMBER_FIELD' && !/^-?\d+(\.\d+)?$/.test(v)) errors[item.name] = `${label(item)} must be numeric.`
  }
  return Object.keys(errors).length === 0
}

async function submit(button: WizardButton) {
  message.value = ''
  if (button.name === 'CANCEL') {
    emit('cancel', button)
    return
  }
  if (button.name === 'DELETE' && !window.confirm('Would you like to perform this delete action?')) return
  if (button.name !== 'DELETE' && !validate()) return
  busy.value = true
  try {
    const row: Record<string, string> = {}
    for (const item of props.region.items ?? []) row[item.column ?? item.name] = values[item.name] ?? ''
    const res = await http.post<{ ok: boolean; id?: string; message: string }>(
      `/api/apex/${props.app}/${props.page}/FORM_DML_${props.region.key}`,
      { x01: button.name, x02: props.pk, clob01: JSON.stringify(row) }
    )
    if (!res.ok) {
      message.value = res.message
      return
    }
    emit('done', button.name, res.id ?? null, res.message)
  } catch (e) {
    message.value = e instanceof Error ? e.message : 'The request failed.'
  } finally {
    busy.value = false
  }
}

onMounted(load)
watch(() => props.pk, load)
</script>

<template>
  <div :id="region.staticId" class="t-Region" :class="[region.cls, { 't-Region--plain': plain }]">
    <div v-if="!plain" class="t-Region-header"><h2 class="t-Region-title">{{ region.title }}</h2></div>
    <div class="t-Region-body">
      <p v-if="message" class="a-IRR-error" role="alert">{{ message }}</p>
      <div class="t-Form">
        <div
          v-for="item in visible"
          :key="item.name"
          class="t-Form-fieldContainer"
          :class="{ 't-Form-fieldContainer--stretch': item.type === 'NATIVE_TEXTAREA' }"
        >
          <label class="t-Form-label" :for="item.name">
            {{ label(item) }}<span v-if="item.required && item.kind !== 'Audit'" class="is-required"> *</span>
          </label>
          <span v-if="item.type === 'NATIVE_DISPLAY_ONLY'" :id="item.name" class="t-Form-field t-Form-field--display">{{ display(item) }}</span>
          <input v-else-if="item.type === 'NATIVE_FILE'" :id="item.name" type="file" class="t-Form-field" @change="pick(item, $event)" />
          <select v-else-if="item.type === 'NATIVE_YES_NO'" :id="item.name" v-model="values[item.name]" class="t-Form-field">
            <option value="Y">Yes</option>
            <option value="N">No</option>
          </select>
          <select v-else-if="item.lov || item.type === 'NATIVE_SELECT_LIST'" :id="item.name" v-model="values[item.name]" class="t-Form-field" :class="{ 'is-error': errors[item.name] }">
            <option value=""></option>
            <option v-for="o in lovs[item.name] ?? []" :key="o[1]" :value="o[1]">{{ o[0] }}</option>
          </select>
          <textarea v-else-if="item.type === 'NATIVE_TEXTAREA'" :id="item.name" v-model="values[item.name]" class="t-Form-field" :class="{ 'is-error': errors[item.name] }"></textarea>
          <input
            v-else-if="item.type === 'NATIVE_DATE_PICKER_APEX' && item.kind === 'Date'"
            :id="item.name"
            v-model="values[item.name]"
            type="date"
            class="t-Form-field"
            :class="{ 'is-error': errors[item.name] }"
          />
          <input
            v-else
            :id="item.name"
            v-model="values[item.name]"
            type="text"
            class="t-Form-field"
            :class="{ 'is-error': errors[item.name], 't-Form-field--number': item.type === 'NATIVE_NUMBER_FIELD' }"
            :readonly="item.kind === 'Audit'"
            :inputmode="item.type === 'NATIVE_NUMBER_FIELD' ? 'decimal' : undefined"
          />
          <div v-if="errors[item.name]" class="t-Form-error">{{ errors[item.name] }}</div>
        </div>
      </div>
    </div>
  </div>
  <div class="t-ButtonRegion">
    <div class="t-ButtonRegion-col">
      <button v-for="b in left" :key="b.name" type="button" class="t-Button" :class="{ 't-Button--danger': b.name === 'DELETE' }" :disabled="busy" @click="submit(b)">
        <i v-if="buttonIcon(b.name)" :class="buttonIcon(b.name)" aria-hidden="true"></i>{{ b.label }}
      </button>
    </div>
    <div class="t-ButtonRegion-col">
      <button v-for="b in right" :key="b.name" type="button" class="t-Button t-Button--hot" :disabled="busy" @click="submit(b)"><i v-if="buttonIcon(b.name)" :class="buttonIcon(b.name)" aria-hidden="true"></i>{{ b.label }}</button>
    </div>
  </div>
</template>
