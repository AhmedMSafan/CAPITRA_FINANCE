<template>
  <!-- A workflow form (K2 SmartForm style): its controls over the workflow's data fields, filled in or shown read-only. -->
  <div class="wff wff-grid" :class="{ 'wff-two': view.columns === 2 }">
    <!-- each heading starts a section; a section can have a frame and a background colour -->
    <div v-for="(g, gi) in sections" :key="gi" class="wff-sec" :class="sectionClass(g.head)">
      <template v-for="(c, i) in g.items" :key="c.field || `${c.kind}-${gi}-${i}`">
        <h3 v-if="c.kind === 'heading'" class="wff-heading">{{ c.label }}</h3>
        <p v-else-if="c.kind === 'note'" class="wff-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ c.label }}</p>
        <div v-else-if="c.kind === 'separator'" class="wff-sep" role="separator"><span v-if="c.label">{{ c.label }}</span></div>

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
    </div>
    <p v-if="!view.controls.length" class="wff-empty">This form has no controls yet.</p>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import type { FormControlView, FormView } from '../services/formService'

const props = defineProps<{
  view: FormView
  modelValue: Record<string, string | null>
  errors?: Record<string, string>
  idPrefix?: string
}>()
const emit = defineEmits<{ 'update:modelValue': [Record<string, string | null>] }>()

/** The controls split into sections: each heading starts one, and the controls before the first heading are a plain one. */
const sections = computed(() => {
  const out: { head: FormControlView | null; items: FormControlView[] }[] = []
  for (const c of props.view.controls) {
    if (c.kind === 'heading' || !out.length) out.push({ head: c.kind === 'heading' ? c : null, items: [] })
    out[out.length - 1]!.items.push(c)
  }
  return out
})
function sectionClass(h: FormControlView | null) {
  const styled = !!h && (!!h.frame || !!h.background)
  return { 'wff-sec--styled': styled, 'wff-grid': styled, 'wff-two': styled && props.view.columns === 2, 'wff-sec--framed': !!h?.frame, [`wff-bg-${h?.background}`]: !!h?.background }
}

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
.wff-grid { display: grid; grid-template-columns: minmax(0, 1fr); gap: 14px 18px; }
.wff-grid.wff-two { grid-template-columns: repeat(2, minmax(0, 1fr)); }
/* a plain section adds no box: its controls sit in the form's own grid */
.wff-sec { display: contents; }
.wff-grid > *, .wff-sec:not(.wff-sec--styled) > * { grid-column: 1 / -1; min-width: 0; }
.wff-two > .wff-half, .wff-two > .wff-sec:not(.wff-sec--styled) > .wff-half { grid-column: auto; }
.wff-two > .wff-left, .wff-two > .wff-sec:not(.wff-sec--styled) > .wff-left { grid-column: 1; }
.wff-two > .wff-right, .wff-two > .wff-sec:not(.wff-sec--styled) > .wff-right { grid-column: 2; }
.wff-sec.wff-sec--styled { display: grid; grid-column: 1 / -1; padding: 14px 16px 16px; border-radius: 12px; }
.wff-sec--framed { border: 1px solid #c9d8de; box-shadow: 0 1px 2px rgba(18, 50, 74, .04); }
.wff-sec--styled > .wff-heading { margin-top: 0; }
.wff-bg-teal { background: #e3f1f2; }
.wff-bg-orange { background: #fdeee2; }
.wff-bg-grey { background: #f1f4f5; }
.wff-bg-green { background: #e8f5ec; }
.wff-bg-yellow { background: #fdf7dc; }
.wff-bg-sand { background: #f6f0e6; }
.wff-sec--styled:not(.wff-sec--framed) .wff-heading { border-bottom-color: rgba(0, 99, 109, .18); }
.wff-sep { display: flex; align-items: center; gap: 10px; color: #5f7787; font-size: 11.5px; font-weight: 700; letter-spacing: .04em; text-transform: uppercase; }
.wff-sep::before, .wff-sep::after { content: ''; flex: 1; border-top: 1px solid #c9d8de; }
.wff-sep:not(:has(span))::after { display: none; }
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
@media (max-width: 700px) { .wff-grid.wff-two { grid-template-columns: minmax(0, 1fr); } .wff-two > .wff-left, .wff-two > .wff-right, .wff-two > .wff-sec > .wff-left, .wff-two > .wff-sec > .wff-right { grid-column: 1 / -1; } }
</style>
