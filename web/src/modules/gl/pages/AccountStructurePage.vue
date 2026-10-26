<script setup lang="ts">
// Account structure & segments (APEX App 120 page 40): the structure, its segments and the values each segment accepts.
import { computed, reactive, ref, shallowRef } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showSuccess } from '@/shared/feedback/feedback'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { formatDate, todayInCairo } from '@/shared/format/datetime'
import GlDialog from '../components/GlDialog.vue'
import GlPageHeader from '../components/GlPageHeader.vue'
import { blank, emptyErrors, numberOrNull, toFormErrors, type FormErrors } from '../components/formErrors'
import {
  accountStructureApi,
  codeLabel,
  type AccountStructurePage,
  type CoaSegment,
  type CoaStructure,
  type SegmentValue
} from '../api/glSetup.api'

const canEdit = hasPermission('GL.SETUP.EDIT')

const page = shallowRef<AccountStructurePage | null>(null)
const values = shallowRef<SegmentValue[]>([])
const selectedSegmentId = ref<number | null>(null)
const search = ref('')
const loadError = ref('')
useFeedback({ errors: [loadError] })

const structure = computed(() => page.value?.structures.find(s => s.coaId === page.value?.coaId) ?? null)
const segments = computed(() => page.value?.segments ?? [])
const segment = computed(() => segments.value.find(s => s.coaSegmentId === selectedSegmentId.value) ?? null)

const ROLES: [keyof CoaStructure, string][] = [
  ['accountSegmentNum', 'Account'],
  ['balancingSegmentNum', 'Balancing'],
  ['branchSegmentNum', 'Branch'],
  ['costCenterSegmentNum', 'Cost centre'],
  ['businessLineSegmentNum', 'Business line'],
  ['intercompanySegmentNum', 'Intercompany']
]

function rolesOf(num: number): string[] {
  const s = structure.value
  return s ? ROLES.filter(([key]) => s[key] === num).map(([, label]) => label) : []
}

async function load(coaId?: number | null) {
  try {
    const data = await accountStructureApi.get(coaId)
    page.value = data
    if (!data.segments.some(s => s.coaSegmentId === selectedSegmentId.value))
      selectedSegmentId.value = data.segments[0]?.coaSegmentId ?? null
    await loadValues()
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'The account structures could not be loaded.'
  }
}

async function loadValues() {
  values.value = selectedSegmentId.value ? await accountStructureApi.values(selectedSegmentId.value) : []
}

function selectSegment(id: number) {
  selectedSegmentId.value = id
  search.value = ''
  loadValues().catch(e => (loadError.value = e instanceof Error ? e.message : 'The values could not be loaded.'))
}

/** Values in tree order (children under their parent) with their depth. */
const orderedValues = computed(() => {
  const byParent = new Map<number | null, SegmentValue[]>()
  const ids = new Set(values.value.map(v => v.segmentValueId))
  for (const v of values.value) {
    const key = v.parentValueId && ids.has(v.parentValueId) ? v.parentValueId : null
    byParent.set(key, [...(byParent.get(key) ?? []), v])
  }
  const out: { value: SegmentValue; depth: number }[] = []
  const walk = (parent: number | null, depth: number) => {
    for (const v of byParent.get(parent) ?? []) {
      out.push({ value: v, depth })
      if (depth < 20) walk(v.segmentValueId, depth + 1)
    }
  }
  walk(null, 0)
  return out
})

const shownValues = computed(() => {
  const q = search.value.trim().toUpperCase()
  if (!q) return orderedValues.value
  return orderedValues.value.filter(({ value: v }) => `${v.valueCode} ${v.valueNameEn} ${v.valueNameAr ?? ''}`.toUpperCase().includes(q))
})

function onStructureChange(event: Event) {
  selectedSegmentId.value = null
  load(Number((event.target as HTMLSelectElement).value))
}

// ---------------------------------------------------------------- dialogs
type DialogKind = 'structure' | 'segment' | 'value' | 'default'
const dialog = ref<DialogKind | null>(null)
const editing = ref(false)
const saving = ref(false)
const errors = ref<FormErrors>(emptyErrors())
const form = reactive<Record<string, unknown>>({})
let editingId = 0
let editingVersion = 0

function open(kind: DialogKind, values: Record<string, unknown>, isEdit: boolean, id = 0, version = 0) {
  Object.keys(form).forEach(k => delete form[k])
  Object.assign(form, values)
  dialog.value = kind
  editing.value = isEdit
  editingId = id
  editingVersion = version
  errors.value = emptyErrors()
}

function openStructure(s: CoaStructure | null) {
  open(
    'structure',
    s
      ? { ...s, effectiveEndDate: s.effectiveEndDate ?? '' }
      : {
          coaCode: '', coaNameEn: '', coaNameAr: '', description: '', delimiterChar: '-', segmentCount: 5,
          accountSegmentNum: '', balancingSegmentNum: '', branchSegmentNum: '', costCenterSegmentNum: '',
          businessLineSegmentNum: '', intercompanySegmentNum: '', isDynamicCombinationAllowed: false,
          isCombinationApprovalRequired: false, isActive: true, effectiveStartDate: todayInCairo(), effectiveEndDate: ''
        },
    !!s, s?.coaId, s?.rowVersion
  )
}

function openSegment(s: CoaSegment | null) {
  if (!s) {
    const used = new Set(segments.value.map(x => x.segmentNum))
    let n = 1
    while (used.has(n)) n++
    open('segment', {
      segmentNum: n, segmentCode: '', segmentNameEn: '', segmentNameAr: '', segmentType: '', minLength: 1, maxLength: 10,
      valueFormat: 'ALPHANUMERIC', padChar: '', isUppercase: true, isRequired: true, isSummaryValueAllowed: false, isEnabled: true,
      displaySeq: n * 10, description: ''
    }, false)
    return
  }
  open('segment', { ...s, padChar: s.padChar ?? '', segmentNameAr: s.segmentNameAr ?? '', description: s.description ?? '' }, true, s.coaSegmentId, s.rowVersion)
}

function openValue(v: SegmentValue | null) {
  if (!v) {
    open('value', {
      valueCode: '', valueNameEn: '', valueNameAr: '', description: '', parentValueId: '', isSummary: false, isPostingAllowed: true,
      isEnabled: true, effectiveStartDate: todayInCairo(), effectiveEndDate: '', sortOrder: (values.value.length + 1) * 10,
      referenceLegalEntityCode: '', referenceBusinessLineCode: ''
    }, false)
    return
  }
  open('value', {
    ...v, valueNameAr: v.valueNameAr ?? '', description: v.description ?? '', parentValueId: v.parentValueId ?? '',
    effectiveEndDate: v.effectiveEndDate ?? '', referenceLegalEntityCode: v.referenceLegalEntityCode ?? '',
    referenceBusinessLineCode: v.referenceBusinessLineCode ?? ''
  }, true, v.segmentValueId, v.rowVersion)
}

function openDefault(pick?: string) {
  const s = segment.value
  if (!s) return
  open('default', { valueCode: pick ?? s.defaultValueCode ?? '', defaultMode: s.defaultMode }, true, s.coaSegmentId)
}

function closeDialog() {
  if (!saving.value) dialog.value = null
}

/** Values a segment value may hang under: any other value that is not below it. */
const parentOptions = computed(() => {
  if (dialog.value !== 'value') return []
  const blocked = new Set<number>()
  if (editing.value) {
    blocked.add(editingId)
    let grew = true
    while (grew) {
      grew = false
      for (const v of values.value)
        if (v.parentValueId && blocked.has(v.parentValueId) && !blocked.has(v.segmentValueId)) {
          blocked.add(v.segmentValueId)
          grew = true
        }
    }
  }
  return orderedValues.value.filter(({ value }) => !blocked.has(value.segmentValueId))
})

const defaultOptions = computed(() => values.value.filter(v => v.isEnabled && v.isPostingAllowed && !v.isSummary))

const segmentCountForForm = computed(() => Math.min(10, Math.max(1, Number(form.segmentCount) || 1)))

function onSummaryChange() {
  if (form.isSummary) form.isPostingAllowed = false
}

const valueRule = computed(() => {
  const s = segment.value
  if (!s) return ''
  const len = s.minLength === s.maxLength ? `${s.minLength}` : `${s.minLength}–${s.maxLength}`
  return `${len} characters, ${codeLabel(s.valueFormat).toLowerCase()}, upper case.`
})

async function save() {
  saving.value = true
  errors.value = emptyErrors()
  const names = Object.keys(form)
  try {
    if (dialog.value === 'structure') {
      const body = {
        ...form,
        coaCode: String(form.coaCode ?? '').trim().toUpperCase(),
        coaNameAr: blank(form.coaNameAr), description: blank(form.description), delimiterChar: blank(form.delimiterChar),
        segmentCount: numberOrNull(form.segmentCount), accountSegmentNum: numberOrNull(form.accountSegmentNum),
        balancingSegmentNum: numberOrNull(form.balancingSegmentNum), branchSegmentNum: numberOrNull(form.branchSegmentNum),
        costCenterSegmentNum: numberOrNull(form.costCenterSegmentNum), businessLineSegmentNum: numberOrNull(form.businessLineSegmentNum),
        intercompanySegmentNum: numberOrNull(form.intercompanySegmentNum),
        effectiveStartDate: blank(form.effectiveStartDate), effectiveEndDate: blank(form.effectiveEndDate), rowVersion: editingVersion
      }
      const saved = editing.value
        ? await accountStructureApi.updateStructure(editingId, body)
        : await accountStructureApi.createStructure(body)
      dialog.value = null
      showSuccess(`Account structure ${saved.coaCode} saved.`)
      await load(saved.coaId)
    } else if (dialog.value === 'segment' && structure.value) {
      const body = {
        ...form,
        segmentCode: String(form.segmentCode ?? '').trim().toUpperCase(),
        segmentNum: numberOrNull(form.segmentNum), minLength: numberOrNull(form.minLength), maxLength: numberOrNull(form.maxLength),
        displaySeq: numberOrNull(form.displaySeq), segmentNameAr: blank(form.segmentNameAr), description: blank(form.description),
        padChar: form.padChar === '' ? null : form.padChar, rowVersion: editingVersion
      }
      const saved = editing.value
        ? await accountStructureApi.updateSegment(editingId, body)
        : await accountStructureApi.createSegment(structure.value.coaId, body)
      dialog.value = null
      selectedSegmentId.value = saved.coaSegmentId
      showSuccess(`Segment ${saved.segmentCode} saved.`)
      await load(structure.value.coaId)
    } else if (dialog.value === 'value' && segment.value) {
      const body = {
        ...form,
        valueCode: String(form.valueCode ?? '').trim().toUpperCase(),
        valueNameAr: blank(form.valueNameAr), description: blank(form.description), parentValueId: numberOrNull(form.parentValueId),
        sortOrder: numberOrNull(form.sortOrder), effectiveStartDate: blank(form.effectiveStartDate), effectiveEndDate: blank(form.effectiveEndDate),
        referenceLegalEntityCode: blank(form.referenceLegalEntityCode), referenceBusinessLineCode: blank(form.referenceBusinessLineCode),
        rowVersion: editingVersion
      }
      const saved = editing.value
        ? await accountStructureApi.updateValue(editingId, body)
        : await accountStructureApi.createValue(segment.value.coaSegmentId, body)
      dialog.value = null
      showSuccess(`Value ${saved.valueCode} saved.`)
      await load(structure.value?.coaId)
    } else if (dialog.value === 'default') {
      const code = (blank(form.valueCode) as string | null) ?? null
      await accountStructureApi.saveDefault(editingId, code, String(form.defaultMode))
      dialog.value = null
      showSuccess(code ? `Journal entry default saved: new journal lines get ${code}.` : 'Journal entry default removed.')
      await load(structure.value?.coaId)
    }
  } catch (e) {
    errors.value = toFormErrors(e, names)
  } finally {
    saving.value = false
  }
}

load()
</script>

<template>
  <div class="cp-page">
    <GlPageHeader
      title="Account structure & segments"
      description="The chart of accounts structure, its segments (company, branch, cost centre, account, business line ...) and the values each segment accepts. Account combinations are built from these values."
      icon="pi pi-sitemap"
    >
      <select v-if="page?.structures.length" :value="page.coaId ?? ''" aria-label="Account structure" @change="onStructureChange">
        <option v-for="s in page.structures" :key="s.coaId" :value="s.coaId">{{ s.coaCode }} - {{ s.coaNameEn }}</option>
      </select>
      <button v-if="canEdit && structure" type="button" class="cp-btn" @click="openStructure(structure)">
        <i class="pi pi-pencil"></i><span>Edit structure</span>
      </button>
      <button v-if="canEdit" type="button" class="cp-btn cp-btn--create" @click="openStructure(null)">
        <i class="pi pi-plus"></i><span>New structure</span>
      </button>
    </GlPageHeader>

    <div v-if="!page" class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div>

    <div v-else-if="!structure" class="cp-card cp-empty as-none">
      <b>No account structure yet</b>
      <span>Create one with <b>New structure</b>, then add its segments and their values.</span>
    </div>

    <template v-else>
      <section class="cp-card as-info">
        <div><small>Structure</small><b>{{ structure.coaNameEn }}<span v-if="structure.coaNameAr" class="ar"> · {{ structure.coaNameAr }}</span></b></div>
        <div><small>Segments</small><b>{{ segments.length }} of {{ structure.segmentCount }}</b></div>
        <div><small>Delimiter</small><b>{{ structure.delimiterChar }}</b></div>
        <div><small>Dynamic combinations</small><b>{{ structure.isDynamicCombinationAllowed ? 'Allowed' : 'No' }}</b></div>
        <div><small>Effective</small><b>{{ formatDate(structure.effectiveStartDate) }}{{ structure.effectiveEndDate ? ` – ${formatDate(structure.effectiveEndDate)}` : ' onwards' }}</b></div>
        <div>
          <small>Status</small>
          <b><span class="cp-pill" :class="structure.isActive ? 'cp-pill--ok' : 'cp-pill--bad'">{{ structure.isActive ? 'Active' : 'Inactive' }}</span></b>
        </div>
        <div v-if="segments.length" class="as-format">
          <small>Account format</small>
          <div class="as-format__chips">
            <template v-for="(s, i) in segments" :key="s.coaSegmentId">
              <i v-if="i > 0">{{ structure.delimiterChar }}</i>
              <span :class="{ acc: s.segmentNum === structure.accountSegmentNum }" :title="s.segmentNameEn">{{ s.segmentCode }}</span>
            </template>
          </div>
        </div>
      </section>

      <h3 class="as-sec">Segments</h3>
      <div class="as-segs">
        <button
          v-for="s in segments"
          :key="s.coaSegmentId"
          type="button"
          class="as-seg"
          :class="{ on: s.coaSegmentId === selectedSegmentId }"
          @click="selectSegment(s.coaSegmentId)"
        >
          <span class="as-seg__top">
            <span class="as-seg__no">{{ s.segmentNum }}</span>
            <span class="cp-pill" :class="s.isEnabled ? 'cp-pill--ok' : 'cp-pill--bad'">{{ s.isEnabled ? 'Enabled' : 'Disabled' }}</span>
          </span>
          <b>{{ s.segmentNameEn }}</b>
          <small><code>{{ s.segmentCode }}</code> · {{ codeLabel(s.segmentType) }}</small>
          <small>
            Length {{ s.minLength }}{{ s.maxLength !== s.minLength ? `–${s.maxLength}` : '' }} · {{ s.valueCount }} values{{ s.isRequired ? ' · required' : '' }}
          </small>
          <span v-if="s.defaultValueCode" class="as-seg__def">
            Journal default <code>{{ s.defaultValueCode }}</code>{{ s.defaultMode === 'FIXED' ? ' · always' : '' }}
          </span>
          <span v-if="rolesOf(s.segmentNum).length" class="as-seg__roles">
            <span v-for="r in rolesOf(s.segmentNum)" :key="r">{{ r }}</span>
          </span>
        </button>
        <button v-if="canEdit && segments.length < structure.segmentCount" type="button" class="as-seg as-seg--add" @click="openSegment(null)">
          <i class="pi pi-plus"></i><span>Add segment</span>
        </button>
      </div>

      <h3 class="as-sec">Segment values</h3>
      <section class="cp-card">
        <div v-if="!segment" class="cp-empty">Add a segment to start defining values.</div>
        <template v-else>
          <div class="cp-card__head as-vhead">
            <h3>{{ segment.segmentNameEn }} <small>{{ values.length }} values · segment {{ segment.segmentNum }}</small></h3>
            <span class="cp-spacer"></span>
            <input v-model="search" type="search" class="as-search" placeholder="Search values" aria-label="Search values" />
            <button
              v-if="canEdit && segment.segmentType !== 'ACCOUNT'"
              type="button"
              class="cp-btn cp-btn--small"
              title="Value filled in automatically on every journal line"
              @click="openDefault()"
            >
              <i class="pi pi-star"></i><span>Journal entry default</span>
            </button>
            <button v-if="canEdit" type="button" class="cp-btn cp-btn--small" @click="openSegment(segment)">
              <i class="pi pi-pencil"></i><span>Edit segment</span>
            </button>
            <button v-if="canEdit" type="button" class="cp-btn cp-btn--small cp-btn--create" @click="openValue(null)">
              <i class="pi pi-plus"></i><span>Add value</span>
            </button>
          </div>
          <p v-if="segment.segmentType === 'ACCOUNT'" class="as-note">
            <i class="pi pi-info-circle"></i> Values of the account segment are created for you when you add accounts on the Chart of accounts page.
          </p>
          <div v-if="shownValues.length === 0" class="cp-empty">
            {{ values.length ? 'No value matches the search.' : 'No values yet. Add the first value of this segment.' }}
          </div>
          <div v-else class="cp-table-wrap">
            <table class="cp-table">
              <thead>
                <tr>
                  <th>Code</th><th>Name</th><th>Parent</th><th>Summary</th><th>Posting</th><th>Effective</th><th>Status</th><th></th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="{ value: v, depth } in shownValues" :key="v.segmentValueId">
                  <td>
                    <span class="as-indent" :style="{ width: `${search ? 0 : depth * 18}px` }"></span>
                    <code class="as-code">{{ v.valueCode }}</code>
                    <span v-if="segment.defaultValueCode === v.valueCode" class="cp-pill cp-pill--ok as-defpill">Default</span>
                  </td>
                  <td class="as-name">{{ v.valueNameEn }}<span v-if="v.valueNameAr" class="ar">{{ v.valueNameAr }}</span></td>
                  <td><code v-if="v.parentValueCode" class="as-code">{{ v.parentValueCode }}</code><span v-else class="cp-muted">–</span></td>
                  <td :class="v.isSummary ? 'yes' : 'no'">{{ v.isSummary ? 'Yes' : 'No' }}</td>
                  <td :class="v.isPostingAllowed ? 'yes' : 'no'">{{ v.isPostingAllowed ? 'Yes' : 'No' }}</td>
                  <td>{{ formatDate(v.effectiveStartDate) }}{{ v.effectiveEndDate ? ` – ${formatDate(v.effectiveEndDate)}` : '' }}</td>
                  <td><span class="cp-pill" :class="v.isEnabled ? 'cp-pill--ok' : 'cp-pill--bad'">{{ v.isEnabled ? 'Enabled' : 'Disabled' }}</span></td>
                  <td><div class="as-actions">
                    <button
                      v-if="canEdit && segment.segmentType !== 'ACCOUNT' && v.isEnabled && v.isPostingAllowed && !v.isSummary"
                      type="button"
                      class="cp-btn cp-btn--small"
                      :class="{ 'as-star-on': segment.defaultValueCode === v.valueCode }"
                      :title="segment.defaultValueCode === v.valueCode ? 'Journal entry default (click to change)' : 'Make this the journal entry default'"
                      :aria-label="`Journal entry default ${v.valueCode}`"
                      @click="openDefault(v.valueCode)"
                    >
                      <i class="pi pi-star"></i>
                    </button>
                    <button type="button" class="cp-btn cp-btn--small" :aria-label="`${canEdit ? 'Edit' : 'View'} ${v.valueCode}`" @click="openValue(v)">
                      <i :class="canEdit ? 'pi pi-pencil' : 'pi pi-eye'"></i>
                    </button>
                  </div></td>
                </tr>
              </tbody>
            </table>
          </div>
        </template>
      </section>
      <p class="apex-ref">Replaces APEX App 120 page 40</p>
    </template>

    <!-- Structure -->
    <GlDialog
      v-if="dialog === 'structure'"
      :title="editing ? 'Edit account structure' : 'New account structure'"
      :subtitle="editing ? 'The code identifies the structure and cannot be changed.' : 'Name the structure and say which segment plays each role.'"
      :saving="saving"
      :error="errors.form"
      save-label="Save structure"
      @close="closeDialog"
      @save="save"
    >
      <div class="cp-field">
        <label for="s-code">Structure code *</label>
        <input id="s-code" v-model="form.coaCode as string" class="upper" maxlength="30" :disabled="editing" />
        <small v-if="errors.fields.coaCode" class="cp-field__error">{{ errors.fields.coaCode }}</small>
      </div>
      <div class="cp-field">
        <label for="s-name">Name (English) *</label>
        <input id="s-name" v-model="form.coaNameEn as string" maxlength="240" />
        <small v-if="errors.fields.coaNameEn" class="cp-field__error">{{ errors.fields.coaNameEn }}</small>
      </div>
      <div class="cp-field">
        <label for="s-name-ar">Name (Arabic)</label>
        <input id="s-name-ar" v-model="form.coaNameAr as string" maxlength="240" dir="rtl" />
      </div>
      <div class="cp-field">
        <label for="s-desc">Description</label>
        <input id="s-desc" v-model="form.description as string" maxlength="1000" />
      </div>
      <div class="cp-field">
        <label for="s-delim">Delimiter</label>
        <input id="s-delim" v-model="form.delimiterChar as string" maxlength="3" />
      </div>
      <div class="cp-field">
        <label for="s-count">Number of segments *</label>
        <input id="s-count" v-model="form.segmentCount" type="number" min="1" max="10" />
        <small v-if="errors.fields.segmentCount" class="cp-field__error">{{ errors.fields.segmentCount }}</small>
        <small v-else class="cp-field__hint">1 to 10. The role lists below follow this number.</small>
      </div>
      <div class="cp-form__section">Segment roles</div>
      <div v-for="[key, label] in ROLES" :key="key" class="cp-field">
        <label :for="`s-${key}`">{{ label }} segment{{ key === 'accountSegmentNum' ? ' *' : '' }}</label>
        <select :id="`s-${key}`" v-model="form[key]">
          <option value="">–</option>
          <option v-for="n in segmentCountForForm" :key="n" :value="n">
            {{ n }}{{ segments.find(x => x.segmentNum === n) ? ` · ${segments.find(x => x.segmentNum === n)!.segmentNameEn}` : '' }}
          </option>
        </select>
        <small v-if="errors.fields[key]" class="cp-field__error">{{ errors.fields[key] }}</small>
      </div>
      <div class="cp-form__section">Validity</div>
      <div class="cp-field">
        <label for="s-start">Effective from</label>
        <input id="s-start" v-model="form.effectiveStartDate as string" type="date" />
      </div>
      <div class="cp-field">
        <label for="s-end">Effective to</label>
        <input id="s-end" v-model="form.effectiveEndDate as string" type="date" />
        <small v-if="errors.fields.effectiveEndDate" class="cp-field__error">{{ errors.fields.effectiveEndDate }}</small>
      </div>
      <div class="gl-checks">
        <label class="gl-check"><input v-model="form.isDynamicCombinationAllowed" type="checkbox" /> Allow dynamic combinations</label>
        <label class="gl-check"><input v-model="form.isCombinationApprovalRequired" type="checkbox" /> Combinations need approval</label>
        <label class="gl-check"><input v-model="form.isActive" type="checkbox" /> Active</label>
      </div>
    </GlDialog>

    <!-- Segment -->
    <GlDialog
      v-if="dialog === 'segment'"
      :title="editing ? `Edit segment ${form.segmentCode}` : `Add segment to ${structure?.coaCode}`"
      :subtitle="editing ? 'Number and code identify the segment and cannot be changed.' : 'Values of this segment must follow the length and format set here.'"
      :saving="saving"
      :error="errors.form"
      save-label="Save segment"
      @close="closeDialog"
      @save="save"
    >
      <div class="cp-field">
        <label for="g-num">Segment number *</label>
        <input id="g-num" v-model="form.segmentNum" type="number" min="1" :max="structure?.segmentCount" :disabled="editing" />
        <small v-if="errors.fields.segmentNum" class="cp-field__error">{{ errors.fields.segmentNum }}</small>
      </div>
      <div class="cp-field">
        <label for="g-code">Segment code *</label>
        <input id="g-code" v-model="form.segmentCode as string" class="upper" maxlength="30" :disabled="editing" />
        <small v-if="errors.fields.segmentCode" class="cp-field__error">{{ errors.fields.segmentCode }}</small>
      </div>
      <div class="cp-field">
        <label for="g-name">Name (English) *</label>
        <input id="g-name" v-model="form.segmentNameEn as string" maxlength="150" />
        <small v-if="errors.fields.segmentNameEn" class="cp-field__error">{{ errors.fields.segmentNameEn }}</small>
      </div>
      <div class="cp-field">
        <label for="g-name-ar">Name (Arabic)</label>
        <input id="g-name-ar" v-model="form.segmentNameAr as string" maxlength="150" dir="rtl" />
      </div>
      <div class="cp-field">
        <label for="g-type">Segment type *</label>
        <select id="g-type" v-model="form.segmentType">
          <option value="">– Select –</option>
          <option v-for="t in page?.segmentTypes ?? []" :key="t" :value="t">{{ codeLabel(t) }}</option>
        </select>
        <small v-if="errors.fields.segmentType" class="cp-field__error">{{ errors.fields.segmentType }}</small>
      </div>
      <div class="cp-field">
        <label for="g-fmt">Value format *</label>
        <select id="g-fmt" v-model="form.valueFormat">
          <option v-for="f in page?.valueFormats ?? []" :key="f" :value="f">{{ codeLabel(f) }}</option>
        </select>
      </div>
      <div class="cp-field">
        <label for="g-min">Min length *</label>
        <input id="g-min" v-model="form.minLength" type="number" min="1" max="30" />
      </div>
      <div class="cp-field">
        <label for="g-max">Max length *</label>
        <input id="g-max" v-model="form.maxLength" type="number" min="1" max="30" />
        <small v-if="errors.fields.maxLength" class="cp-field__error">{{ errors.fields.maxLength }}</small>
        <small v-else class="cp-field__hint">Up to 30.</small>
      </div>
      <div class="cp-field">
        <label for="g-pad">Pad character</label>
        <input id="g-pad" v-model="form.padChar as string" maxlength="1" />
      </div>
      <div class="cp-field">
        <label for="g-seq">Display sequence</label>
        <input id="g-seq" v-model="form.displaySeq" type="number" min="0" />
      </div>
      <div class="cp-field cp-field--wide">
        <label for="g-desc">Description</label>
        <input id="g-desc" v-model="form.description as string" maxlength="1000" />
      </div>
      <div class="gl-checks">
        <label class="gl-check"><input v-model="form.isUppercase" type="checkbox" /> Upper-case values</label>
        <label class="gl-check"><input v-model="form.isRequired" type="checkbox" /> Required</label>
        <label class="gl-check"><input v-model="form.isSummaryValueAllowed" type="checkbox" /> Allow summary values</label>
        <label class="gl-check"><input v-model="form.isEnabled" type="checkbox" /> Enabled</label>
      </div>
    </GlDialog>

    <!-- Segment value -->
    <GlDialog
      v-if="dialog === 'value'"
      :title="editing ? `Edit value ${form.valueCode}` : `Add value to ${segment?.segmentNameEn}`"
      :subtitle="editing ? 'The code identifies the value and cannot be changed.' : `Value code: ${valueRule}`"
      :saving="saving"
      :error="errors.form"
      :readonly="!canEdit"
      save-label="Save value"
      @close="closeDialog"
      @save="save"
    >
      <div class="cp-field">
        <label for="v-code">Value code *</label>
        <input id="v-code" v-model="form.valueCode as string" class="upper" :maxlength="segment?.maxLength ?? 30" :disabled="editing" />
        <small v-if="errors.fields.valueCode" class="cp-field__error">{{ errors.fields.valueCode }}</small>
      </div>
      <div class="cp-field">
        <label for="v-sort">Sort order</label>
        <input id="v-sort" v-model="form.sortOrder" type="number" min="0" />
      </div>
      <div class="cp-field">
        <label for="v-name">Name (English) *</label>
        <input id="v-name" v-model="form.valueNameEn as string" maxlength="240" />
        <small v-if="errors.fields.valueNameEn" class="cp-field__error">{{ errors.fields.valueNameEn }}</small>
      </div>
      <div class="cp-field">
        <label for="v-name-ar">Name (Arabic)</label>
        <input id="v-name-ar" v-model="form.valueNameAr as string" maxlength="240" dir="rtl" />
      </div>
      <div class="cp-field cp-field--wide">
        <label for="v-parent">Parent value</label>
        <select id="v-parent" v-model="form.parentValueId">
          <option value="">No parent</option>
          <option v-for="{ value: p, depth } in parentOptions" :key="p.segmentValueId" :value="p.segmentValueId">
            {{ '  '.repeat(depth) }}{{ p.valueCode }} - {{ p.valueNameEn }}
          </option>
        </select>
        <small v-if="errors.fields.parentValueId" class="cp-field__error">{{ errors.fields.parentValueId }}</small>
      </div>
      <div class="cp-field">
        <label for="v-start">Effective from *</label>
        <input id="v-start" v-model="form.effectiveStartDate as string" type="date" />
        <small v-if="errors.fields.effectiveStartDate" class="cp-field__error">{{ errors.fields.effectiveStartDate }}</small>
      </div>
      <div class="cp-field">
        <label for="v-end">Effective to</label>
        <input id="v-end" v-model="form.effectiveEndDate as string" type="date" />
        <small v-if="errors.fields.effectiveEndDate" class="cp-field__error">{{ errors.fields.effectiveEndDate }}</small>
      </div>
      <div class="cp-field">
        <label for="v-le">Legal entity</label>
        <select id="v-le" v-model="form.referenceLegalEntityCode">
          <option value="">–</option>
          <option v-for="l in page?.legalEntities ?? []" :key="l.code" :value="l.code">{{ l.code }} - {{ l.name }}</option>
        </select>
      </div>
      <div class="cp-field">
        <label for="v-bl">Business line</label>
        <select id="v-bl" v-model="form.referenceBusinessLineCode">
          <option value="">–</option>
          <option v-for="l in page?.businessLines ?? []" :key="l.code" :value="l.code">{{ l.code }} - {{ l.name }}</option>
        </select>
      </div>
      <div class="cp-field cp-field--wide">
        <label for="v-desc">Description</label>
        <input id="v-desc" v-model="form.description as string" maxlength="1000" />
      </div>
      <div class="gl-checks">
        <label class="gl-check"><input v-model="form.isSummary" type="checkbox" @change="onSummaryChange" /> Summary (parent) value</label>
        <label class="gl-check" :style="{ opacity: form.isSummary ? 0.55 : 1 }">
          <input v-model="form.isPostingAllowed" type="checkbox" :disabled="!!form.isSummary" /> Posting allowed
        </label>
        <label class="gl-check"><input v-model="form.isEnabled" type="checkbox" /> Enabled</label>
      </div>
      <small v-if="errors.fields.isPostingAllowed" class="cp-field__error">{{ errors.fields.isPostingAllowed }}</small>
    </GlDialog>

    <!-- Journal entry default -->
    <GlDialog
      v-if="dialog === 'default'"
      :title="`Journal entry default – ${segment?.segmentNameEn}`"
      subtitle="Filled in on every new journal line, so users do not have to type it."
      :saving="saving"
      :error="errors.form"
      save-label="Save default"
      @close="closeDialog"
      @save="save"
    >
      <div class="cp-field cp-field--wide">
        <label for="d-val">Default value</label>
        <select id="d-val" v-model="form.valueCode">
          <option value="">No default</option>
          <option v-for="v in defaultOptions" :key="v.valueCode" :value="v.valueCode">{{ v.valueCode }} - {{ v.valueNameEn }}</option>
        </select>
        <small class="cp-field__hint">Only enabled values that allow posting are listed.</small>
      </div>
      <div class="cp-field cp-field--wide">
        <label>How it is used</label>
        <label class="gl-check"><input v-model="form.defaultMode" type="radio" value="SUGGEST" /> Suggest it: filled in, the user can change it on a line</label>
        <label class="gl-check"><input v-model="form.defaultMode" type="radio" value="FIXED" /> Always use it: the field is hidden in journal entry</label>
      </div>
    </GlDialog>
  </div>
</template>

<style scoped>
.as-none { display: flex; flex-direction: column; gap: 6px; }
.as-none b { color: var(--cap-heading); }
.as-info { display: flex; flex-wrap: wrap; gap: 14px 34px; padding: 16px 22px; margin-bottom: 18px; }
.as-info small, .as-vhead small { display: block; color: var(--cap-muted); font-size: 11px; font-weight: 500; text-transform: uppercase; letter-spacing: .04em; }
.as-info b { display: block; margin-top: 4px; color: var(--cap-heading); font-size: 14px; font-weight: 600; }
.as-info .ar { color: var(--cap-muted); font-weight: 500; }
.as-format { flex-basis: 100%; }
.as-format__chips { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; margin-top: 6px; }
.as-format__chips span { padding: 4px 10px; border: 1px solid var(--cap-border); border-radius: 6px; background: var(--cap-bg); font: 600 12px ui-monospace, Menlo, Consolas, monospace; color: var(--cap-ink); }
.as-format__chips span.acc { border-color: var(--cap-primary); background: var(--cap-primary-tint); color: var(--cap-primary-dark); }
.as-format__chips i { color: var(--cap-muted); font-style: normal; font-weight: 700; }
.as-sec { margin: 6px 0 10px; color: var(--cap-heading); font-size: 15px; font-weight: 600; }
.as-segs { display: grid; grid-template-columns: repeat(auto-fill, minmax(210px, 1fr)); gap: 12px; margin-bottom: 20px; }
.as-seg { display: flex; flex-direction: column; align-items: flex-start; gap: 4px; padding: 14px 16px; border: 1px solid var(--cap-border-soft); border-radius: 14px; background: #fff; text-align: start; font: inherit; cursor: pointer; box-shadow: 0 4px 14px rgba(13, 42, 46, .04); }
.as-seg:hover { border-color: var(--cap-primary-tint-2); }
.as-seg.on { border-color: var(--cap-primary); box-shadow: 0 0 0 2px var(--cap-primary-tint-2); }
.as-seg__top { display: flex; width: 100%; justify-content: space-between; align-items: center; margin-bottom: 4px; }
.as-seg__no { width: 26px; height: 26px; display: grid; place-items: center; border-radius: 50%; background: var(--cap-primary); color: #fff; font-size: 12px; font-weight: 700; }
.as-seg b { color: var(--cap-heading); font-size: 14px; }
.as-seg small { color: var(--cap-muted); font-size: 12px; }
.as-seg code, .as-code { font: 600 12px ui-monospace, Menlo, Consolas, monospace; color: var(--cap-primary-dark); }
.as-seg__def { margin-top: 2px; color: var(--cap-ink-soft); font-size: 11px; }
.as-seg__roles { display: flex; flex-wrap: wrap; gap: 4px; margin-top: 4px; }
.as-seg__roles span { padding: 2px 8px; border-radius: 999px; background: var(--cap-orange-tint); color: var(--cap-orange-dark); font-size: 11px; font-weight: 600; }
.as-seg--add { align-items: center; justify-content: center; flex-direction: row; gap: 8px; border-style: dashed; color: var(--cap-primary); font-weight: 600; min-height: 120px; }
.as-vhead { flex-wrap: wrap; }
.as-vhead h3 small { display: inline; margin-left: 8px; text-transform: none; letter-spacing: 0; }
.as-search { height: 32px; width: 200px; padding: 0 10px; border: 1px solid var(--cap-border); border-radius: 7px; font: inherit; font-size: 12px; }
.as-note { margin: 0; padding: 10px 20px; border-bottom: 1px solid var(--cap-border-soft); background: var(--cap-bg); color: var(--cap-ink-soft); font-size: 12px; }
.as-indent { display: inline-block; }
.as-name .ar { display: block; color: var(--cap-muted); font-size: 11px; direction: rtl; text-align: start; }
.as-defpill { margin-left: 6px; }
td.yes { color: var(--cap-success); font-weight: 600; }
td.no { color: var(--cap-muted); }
.as-actions { display: flex; gap: 6px; justify-content: flex-end; }
.as-star-on { border-color: var(--cap-orange-border); background: var(--cap-orange-tint); color: var(--cap-orange-dark); }
.apex-ref { margin: 12px 0 0; color: var(--cap-faint); font-size: 11px; text-align: end; }
</style>
