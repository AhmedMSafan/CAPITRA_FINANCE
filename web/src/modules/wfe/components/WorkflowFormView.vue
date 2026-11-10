<template>
  <!-- A workflow form (K2 SmartForm style): its controls over the workflow's data fields, filled in or shown read-only. -->
  <div class="wff" :class="{ 'wff-two': view.columns === 2 }">
    <template v-for="(c, i) in view.controls" :key="c.field || `${c.kind}-${i}`">
      <h3 v-if="c.kind === 'heading'" class="wff-heading">{{ c.label }}</h3>
      <p v-else-if="c.kind === 'note'" class="wff-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ c.label }}</p>

      <div v-else class="wff-item" :class="{ 'wff-half': c.width === 'half', 'wff-left': c.width === 'left', 'wff-right': c.width === 'right', 'has-error': !!errorOf(c) }">
        <label :for="id(c)" class="wff-label">{{ c.label }}<span v-if="c.required && !c.readOnly" class="wff-req">*</span></label>

        <!-- read-only: the value as people read it -->
        <div v-if="c.readOnly" :id="id(c)" class="wff-value" :class="{ empty: !show(c) }">{{ show(c) || '-' }}</div>

        <div v-else-if="c.control === 'yesno'" :id="id(c)" class="wff-yesno" role="radiogroup" :aria-label="c.label">
          <button type="button" role="radio" :aria-checked="val(c) === 'true'" @click="set(c, val(c) === 'true' ? null : 'true')">Yes</button>
          <button type="button" role="radio" :aria-checked="val(c) === 'false'" @click="set(c, val(c) === 'false' ? null : 'false')">No</button>
        </div>

        <select v-else-if="c.control === 'dropdown' || c.control === 'user'" :id="id(c)" class="wff-input" :value="val(c) ?? ''"
                @change="set(c, ($event.target as HTMLSelectElement).value || null)">
          <option value="">{{ c.control === 'user' ? 'Choose a person' : 'Choose…' }}</option>
          <option v-for="o in c.options" :key="o.value" :value="o.value">{{ o.label }}</option>
        </select>

        <textarea v-else-if="c.control === 'textarea'" :id="id(c)" class="wff-input wff-area" rows="3" :value="val(c) ?? ''"
                  @input="set(c, ($event.target as HTMLTextAreaElement).value)"></textarea>

        <input v-else :id="id(c)" class="wff-input" :class="{ 'wff-num': c.control === 'number' || c.control === 'amount' }"
               :type="c.control === 'date' ? 'date' : 'text'"
               :inputmode="c.control === 'number' ? 'numeric' : c.control === 'amount' ? 'decimal' : undefined"
               :placeholder="c.control === 'amount' ? '0.00' : undefined"
               :value="val(c) ?? ''" @input="set(c, ($event.target as HTMLInputElement).value)">

        <small v-if="errorOf(c)" class="wff-error"><span class="fa fa-exclamation-circle" aria-hidden="true"></span> {{ errorOf(c) }}</small>
        <small v-else-if="c.help" class="wff-help">{{ c.help }}</small>
      </div>
    </template>
    <p v-if="!view.controls.length" class="wff-empty">This form has no controls yet.</p>
  </div>
</template>

<script setup lang="ts">
import type { FormControlView, FormView } from '../services/formService'

const props = defineProps<{
  view: FormView
  modelValue: Record<string, string | null>
  errors?: Record<string, string>
  idPrefix?: string
}>()
const emit = defineEmits<{ 'update:modelValue': [Record<string, string | null>] }>()

function id(c: FormControlView) { return `${props.idPrefix ?? 'wff'}-${c.field}` }
function val(c: FormControlView) { return c.field ? props.modelValue[c.field] ?? null : null }
function set(c: FormControlView, v: string | null) {
  if (!c.field) return
  emit('update:modelValue', { ...props.modelValue, [c.field]: v })
}
function errorOf(c: FormControlView) { return c.field ? props.errors?.[c.field.toLowerCase()] ?? '' : '' }

/** A value as people read it: the option's label, Yes/No, amounts with separators, dates as 08 Nov 2026. */
function show(c: FormControlView) {
  const v = val(c)
  if (v === null || v === '') return ''
  const o = c.options.find((x) => x.value.toLowerCase() === v.toLowerCase())
  if (o) return o.label
  if (c.fieldType === 'Boolean') return v === 'true' ? 'Yes' : v === 'false' ? 'No' : v
  if (c.fieldType === 'Decimal' && Number.isFinite(Number(v))) return Number(v).toLocaleString('en-US', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
  if (c.fieldType === 'Number' && Number.isFinite(Number(v))) return Number(v).toLocaleString('en-US')
  if (c.fieldType === 'Date') {
    const d = new Date(v.length === 10 ? `${v}T00:00:00` : v)
    if (!Number.isNaN(d.getTime())) return d.toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' })
  }
  return v
}
</script>

<style scoped>
.wff { display: grid; grid-template-columns: minmax(0, 1fr); gap: 14px 18px; }
.wff.wff-two { grid-template-columns: repeat(2, minmax(0, 1fr)); }
.wff > * { grid-column: 1 / -1; min-width: 0; }
.wff.wff-two > .wff-half { grid-column: auto; }
.wff.wff-two > .wff-left { grid-column: 1; }
.wff.wff-two > .wff-right { grid-column: 2; }
.wff-heading { margin: 6px 0 -4px; padding-bottom: 6px; border-bottom: 1px solid #d9e6ea; color: #00636D; font-size: 13px; font-weight: 800; letter-spacing: .04em; text-transform: uppercase; }
.wff-note { display: flex; gap: 8px; margin: 0; padding: 9px 12px; border-radius: 8px; background: #f2f7f8; color: #3f5b6b; font-size: 12.5px; }
.wff-note .fa { color: #00636D; margin-top: 2px; }
.wff-item { display: grid; gap: 5px; }
.wff-label { color: #12324a; font-size: 12px; font-weight: 700; }
.wff-req { color: #E87722; margin-left: 2px; }
.wff-input { width: 100%; min-width: 0; box-sizing: border-box; padding: 8px 10px; border: 1px solid #c9d8de; border-radius: 8px; background: #fff; color: #12324a; font: inherit; font-size: 13px; }
.wff-input:focus { outline: 2px solid rgba(0, 99, 109, .25); border-color: #00636D; }
.wff-area { resize: vertical; min-height: 64px; }
.wff-num { text-align: right; font-variant-numeric: tabular-nums; }
.wff-value { padding: 8px 10px; border-radius: 8px; background: #f5f8f9; color: #12324a; font-size: 13px; font-weight: 600; min-height: 18px; }
.wff-value.empty { color: #8aa0ad; font-weight: 400; }
.wff-yesno { display: inline-flex; width: max-content; border: 1px solid #c9d8de; border-radius: 8px; overflow: hidden; }
.wff-yesno button { border: 0; padding: 7px 18px; background: #fff; color: #3f5b6b; font: inherit; font-size: 13px; font-weight: 600; cursor: pointer; }
.wff-yesno button + button { border-left: 1px solid #c9d8de; }
.wff-yesno button[aria-checked=true] { background: #00636D; color: #fff; }
.wff-help { color: #5f7787; font-size: 11.5px; }
.wff-error { color: #b4520b; font-size: 11.5px; font-weight: 600; }
.has-error .wff-input, .has-error .wff-yesno { border-color: #E87722; }
.wff-empty { color: #5f7787; font-size: 13px; margin: 0; }
@media (max-width: 700px) { .wff.wff-two { grid-template-columns: minmax(0, 1fr); } }
</style>
