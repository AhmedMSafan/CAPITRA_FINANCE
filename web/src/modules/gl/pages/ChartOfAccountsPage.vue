<script setup lang="ts">
// Chart of accounts (APEX App 120 pages 2/3): the account tree of a structure, with the details of the chosen account.
import { computed, reactive, ref, shallowRef } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { showSuccess } from '@/shared/feedback/feedback'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import GlDialog from '../components/GlDialog.vue'
import GlPageHeader from '../components/GlPageHeader.vue'
import { blank, emptyErrors, numberOrNull, toFormErrors, type FormErrors } from '../components/formErrors'
import { chartOfAccountsApi, codeLabel, type ChartOfAccounts, type GlAccount } from '../api/glSetup.api'

const canEdit = hasPermission('GL.SETUP.EDIT')

const data = shallowRef<ChartOfAccounts | null>(null)
const selectedId = ref<number | null>(null)
const collapsed = reactive<Record<string, boolean>>({})
const search = ref('')
const filter = ref<'ALL' | 'POSTING' | 'SUMMARY' | 'INACTIVE'>('ALL')
const loadError = ref('')
useFeedback({ errors: [loadError] })

type Group = { key: string; label: string }
const GROUPS: Group[] = [
  { key: 'ASSET', label: 'Assets' },
  { key: 'LIABILITY', label: 'Liabilities' },
  { key: 'EQUITY', label: 'Equity' },
  { key: 'INCOME', label: 'Income' },
  { key: 'EXPENSE', label: 'Expenses' }
]

/** The statement group of an account type: CONTRA_ASSET sits with assets, REVENUE with income. */
function bucket(type: string): string {
  if (type.includes('ASSET')) return 'ASSET'
  if (type.includes('LIAB')) return 'LIABILITY'
  if (type.includes('EQUIT')) return 'EQUITY'
  if (type.includes('REVENUE') || type.includes('INCOME')) return 'INCOME'
  return 'EXPENSE'
}

const FLAGS: [keyof GlAccount, string][] = [
  ['isControlAccount', 'Control account'],
  ['isReconciliation', 'Reconciliation'],
  ['isManualJournalAllowed', 'Manual journals'],
  ['isSubledgerPostingAllowed', 'Sub-ledger posting'],
  ['isBranchRequired', 'Branch required'],
  ['isCostCenterRequired', 'Cost centre required'],
  ['isBusinessLineRequired', 'Business line required'],
  ['isCurrencyRevaluation', 'Currency revaluation'],
  ['isRetainedEarnings', 'Retained earnings']
]

const accounts = computed(() => data.value?.accounts ?? [])
const byId = computed(() => new Map(accounts.value.map(a => [a.accountId, a])))
const structure = computed(() => data.value?.structures.find(s => s.coaId === data.value?.coaId) ?? null)
const accountSegment = computed(() => data.value?.segments.find(s => s.segmentNum === structure.value?.accountSegmentNum) ?? null)
const selected = computed(() => (selectedId.value ? byId.value.get(selectedId.value) ?? null : null))

function children(parentId: number | null): GlAccount[] {
  return accounts.value.filter(a => (a.parentAccountId && byId.value.has(a.parentAccountId) ? a.parentAccountId : null) === parentId)
}

function path(a: GlAccount): GlAccount[] {
  const out: GlAccount[] = []
  let current: GlAccount | undefined = a
  for (let guard = 0; current && guard < 40; guard++) {
    out.unshift(current)
    current = current.parentAccountId ? byId.value.get(current.parentAccountId) : undefined
  }
  return out
}

function matches(a: GlAccount): boolean {
  const q = search.value.trim().toUpperCase()
  if (q && !`${a.accountCode} ${a.accountNameEn} ${a.accountNameAr ?? ''}`.toUpperCase().includes(q)) return false
  if (filter.value === 'POSTING') return a.isPostingAllowed
  if (filter.value === 'SUMMARY') return a.isSummary
  if (filter.value === 'INACTIVE') return !a.isActive
  return true
}

/** While searching or filtering: the matching accounts plus their ancestors, so the tree keeps its shape. */
const visible = computed<Set<number> | null>(() => {
  if (!search.value.trim() && filter.value === 'ALL') return null
  const set = new Set<number>()
  for (const a of accounts.value) if (matches(a)) path(a).forEach(p => set.add(p.accountId))
  return set
})

type Row = { account: GlAccount; depth: number; hasChildren: boolean; open: boolean; hit: boolean }

function rowsOf(group: string): Row[] {
  const out: Row[] = []
  const vs = visible.value
  const walk = (list: GlAccount[], depth: number) => {
    for (const a of list) {
      if (vs && !vs.has(a.accountId)) continue
      const kids = children(a.accountId).filter(k => !vs || vs.has(k.accountId))
      const open = vs ? true : !collapsed[a.accountId]
      out.push({ account: a, depth, hasChildren: kids.length > 0, open, hit: !!vs && matches(a) })
      if (open && depth < 30) walk(kids, depth + 1)
    }
  }
  walk(children(null).filter(a => bucket(a.accountType) === group), 0)
  return out
}

const groups = computed(() =>
  GROUPS.map(g => ({ ...g, rows: rowsOf(g.key), total: accounts.value.filter(a => bucket(a.accountType) === g.key).length }))
)

async function load(coaId?: number | null) {
  try {
    data.value = await chartOfAccountsApi.get(coaId)
    if (selectedId.value && !byId.value.has(selectedId.value)) selectedId.value = null
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'The chart of accounts could not be loaded.'
  }
}

function select(id: number) {
  selectedId.value = id
  const a = byId.value.get(id)
  if (a) path(a).slice(0, -1).forEach(p => (collapsed[p.accountId] = false))
}

function toggle(id: number) {
  collapsed[id] = !collapsed[id]
}

function onStructureChange(event: Event) {
  selectedId.value = null
  load(Number((event.target as HTMLSelectElement).value))
}

// ---------------------------------------------------------------- account form
const dialogOpen = ref(false)
const editing = ref(false)
const saving = ref(false)
const errors = ref<FormErrors>(emptyErrors())
const form = reactive<Record<string, unknown>>({})
let editingId = 0
let editingVersion = 0

function typeDefault(type: string) {
  return data.value?.typeDefaults.find(d => d.accountType === type)
}

function openForm(account: GlAccount | null, parent: GlAccount | null = null) {
  Object.keys(form).forEach(k => delete form[k])
  if (account) {
    Object.assign(form, {
      ...account,
      accountNameAr: account.accountNameAr ?? '',
      description: account.description ?? '',
      parentAccountId: account.parentAccountId ?? '',
      effectiveEndDate: account.effectiveEndDate ?? ''
    })
    editingId = account.accountId
    editingVersion = account.rowVersion
  } else {
    const type = parent?.accountType ?? 'ASSET'
    const d = typeDefault(type)
    Object.assign(form, {
      accountCode: '', accountNameEn: '', accountNameAr: '', description: '',
      parentAccountId: parent?.accountId ?? '',
      accountType: type,
      normalBalance: parent?.normalBalance ?? d?.normalBalance ?? '',
      financialStatementClass: parent?.financialStatementClass ?? d?.financialStatementClass ?? '',
      cashFlowClass: parent?.cashFlowClass ?? d?.cashFlowClass ?? 'NONE',
      isSummary: false, isPostingAllowed: true, isActive: true, effectiveStartDate: '', effectiveEndDate: ''
    })
    for (const [key] of FLAGS) form[key] = parent ? parent[key] : key === 'isManualJournalAllowed' || key === 'isSubledgerPostingAllowed'
    form.isRetainedEarnings = false
  }
  editing.value = !!account
  errors.value = emptyErrors()
  dialogOpen.value = true
}

/** A new top-level account of a group: the group decides its type. */
function openInGroup(group: string) {
  openForm(null)
  form.accountType = group === 'INCOME' ? 'REVENUE' : group
  onTypeChange()
}

function onTypeChange() {
  const d = typeDefault(String(form.accountType))
  if (!d) return
  form.normalBalance = d.normalBalance
  form.financialStatementClass = d.financialStatementClass
  form.cashFlowClass = d.cashFlowClass
}

function onSummaryChange() {
  if (form.isSummary) form.isPostingAllowed = false
}

/** Parents the account may hang under: every account that is not this one or below it. */
const parentOptions = computed(() => {
  const sorted = [...accounts.value].sort((x, y) => x.accountCode.localeCompare(y.accountCode, undefined, { numeric: true }))
  if (!editing.value) return sorted
  return sorted.filter(p => path(p).every(q => q.accountId !== editingId))
})

const parentHint = computed(() => {
  const p = byId.value.get(Number(form.parentAccountId))
  return p && !p.isSummary ? 'This parent is a posting account; parents are usually summary accounts.' : ''
})

/** Live check of the code against the account segment, as on APEX page 2. */
const codeRule = computed(() => {
  const s = accountSegment.value
  if (!s) return { ok: false, text: 'The structure has no account segment yet. Add it on the Account structure page.' }
  if (s.segmentType !== 'ACCOUNT') return { ok: false, text: `Segment ${s.segmentCode} is the structure's account segment but its type is ${codeLabel(s.segmentType)}, not Account.` }
  const len = s.minLength === s.maxLength ? `${s.minLength}` : `${s.minLength}–${s.maxLength}`
  const rule = `Segment ${s.segmentNum} · ${s.segmentNameEn}: ${len} characters, ${codeLabel(s.valueFormat).toLowerCase()}, upper case.`
  const code = String(form.accountCode ?? '').trim().toUpperCase()
  if (editing.value || !code) return { ok: null, text: rule }
  if (code.length < s.minLength || code.length > s.maxLength) return { ok: false, text: `Must be ${len} characters (${code.length} now). ${rule}` }
  if (s.valueFormat === 'NUMERIC' && !/^[0-9]+$/.test(code)) return { ok: false, text: `Digits only. ${rule}` }
  if (s.valueFormat === 'ALPHANUMERIC' && !/^[A-Z0-9]+$/.test(code)) return { ok: false, text: `Letters and digits only. ${rule}` }
  if (accounts.value.some(a => a.accountCode === code)) return { ok: false, text: `Account ${code} already exists.` }
  return { ok: true, text: `Valid · ${rule}` }
})

async function save() {
  saving.value = true
  errors.value = emptyErrors()
  const body = {
    ...form,
    coaId: data.value?.coaId,
    accountCode: String(form.accountCode ?? '').trim().toUpperCase(),
    accountNameAr: blank(form.accountNameAr),
    description: blank(form.description),
    parentAccountId: numberOrNull(form.parentAccountId),
    effectiveStartDate: blank(form.effectiveStartDate),
    effectiveEndDate: blank(form.effectiveEndDate),
    rowVersion: editingVersion
  }
  try {
    const saved = editing.value ? await chartOfAccountsApi.update(editingId, body) : await chartOfAccountsApi.create(body)
    dialogOpen.value = false
    showSuccess(editing.value ? `Account ${saved.accountCode} saved.` : `Account ${saved.accountCode} created and linked to the account segment.`)
    await load(data.value?.coaId)
    select(saved.accountId)
  } catch (e) {
    errors.value = toFormErrors(e, Object.keys(form))
  } finally {
    saving.value = false
  }
}

load()
</script>

<template>
  <div class="cp-page">
    <GlPageHeader
      title="Chart of accounts"
      description="The account hierarchy of the account structure. Every account code is a value of the structure's account segment, so it always fits the segment rules."
      icon="pi pi-book"
    >
      <select v-if="data?.structures.length" :value="data.coaId ?? ''" aria-label="Account structure" @change="onStructureChange">
        <option v-for="s in data.structures" :key="s.coaId" :value="s.coaId">{{ s.coaCode }} - {{ s.coaNameEn }}</option>
      </select>
      <button v-if="canEdit && structure" type="button" class="cp-btn cp-btn--create" @click="openForm(null)">
        <i class="pi pi-plus"></i><span>New account</span>
      </button>
    </GlPageHeader>

    <div v-if="!data" class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div>

    <div v-else-if="!structure" class="cp-card cp-empty">
      No account structure yet. Create one on the
      <RouterLink :to="{ name: 'gl-account-structure' }">Account structure</RouterLink> page first.
    </div>

    <div v-else class="ca">
      <!-- Tree -->
      <section class="cp-card ca-tree">
        <div class="ca-tools">
          <div class="ca-search">
            <i class="pi pi-search"></i>
            <input v-model="search" type="search" placeholder="Search code or name" aria-label="Search accounts" />
          </div>
          <select v-model="filter" aria-label="Show">
            <option value="ALL">All accounts</option>
            <option value="POSTING">Posting accounts</option>
            <option value="SUMMARY">Summary accounts</option>
            <option value="INACTIVE">Inactive</option>
          </select>
        </div>
        <div class="ca-groups">
          <div v-for="g in groups" :key="g.key" class="ca-group" :class="`ca-group--${g.key.toLowerCase()}`">
            <div class="ca-group__head">
              <span>{{ g.label }}</span>
              <small>{{ g.total }}</small>
              <button v-if="canEdit" type="button" class="ca-add" :title="`New ${g.label.toLowerCase()} account`" @click="openInGroup(g.key)">
                <i class="pi pi-plus"></i>
              </button>
            </div>
            <div v-if="g.rows.length === 0" class="ca-none">{{ g.total ? 'No match.' : 'No accounts.' }}</div>
            <button
              v-for="r in g.rows"
              :key="r.account.accountId"
              type="button"
              class="ca-row"
              :class="{ on: r.account.accountId === selectedId, off: !r.account.isActive, hit: r.hit }"
              :style="{ paddingInlineStart: `${10 + r.depth * 18}px` }"
              @click="select(r.account.accountId)"
            >
              <i
                class="ca-caret pi"
                :class="r.hasChildren ? (r.open ? 'pi-chevron-down' : 'pi-chevron-right') : 'pi-circle-fill ca-dot'"
                @click.stop="r.hasChildren && toggle(r.account.accountId)"
              ></i>
              <code>{{ r.account.accountCode }}</code>
              <span class="ca-row__name">{{ r.account.accountNameEn }}</span>
              <span v-if="r.account.isSummary" class="ca-tag">Summary</span>
              <span v-if="!r.account.isActive" class="ca-tag ca-tag--off">Inactive</span>
            </button>
          </div>
        </div>
      </section>

      <!-- Details -->
      <section class="cp-card ca-det">
        <div v-if="!selected" class="cp-empty ca-pick">
          <i class="pi pi-sitemap"></i>
          <b>Select an account</b>
          <span>Use the tree or the search to open an account.</span>
        </div>
        <template v-else>
          <div class="ca-det__head">
            <nav class="ca-crumb">
              <span>{{ GROUPS.find(g => g.key === bucket(selected!.accountType))?.label }}</span>
              <template v-for="p in path(selected).slice(0, -1)" :key="p.accountId">
                › <a href="#" @click.prevent="select(p.accountId)">{{ p.accountCode }} {{ p.accountNameEn }}</a>
              </template>
            </nav>
            <div class="ca-title">
              <div>
                <h2><code>{{ selected.accountCode }}</code> {{ selected.accountNameEn }}</h2>
                <span v-if="selected.accountNameAr" class="ca-ar">{{ selected.accountNameAr }}</span>
                <div class="ca-pills">
                  <span class="cp-pill" :class="selected.isActive ? 'cp-pill--ok' : 'cp-pill--bad'">{{ selected.isActive ? 'Active' : 'Inactive' }}</span>
                  <span class="cp-pill cp-pill--info">{{ selected.isSummary ? 'Summary' : 'Posting' }} account</span>
                  <span class="cp-pill">Normal balance: {{ selected.normalBalance }}</span>
                  <span v-if="selected.isControlAccount" class="cp-pill cp-pill--warn">Control</span>
                </div>
              </div>
              <div v-if="canEdit" class="ca-acts">
                <button type="button" class="cp-btn" @click="openForm(null, selected)"><i class="pi pi-plus"></i><span>Add child</span></button>
                <button type="button" class="cp-btn cp-btn--primary" @click="openForm(selected)"><i class="pi pi-pencil"></i><span>Edit</span></button>
              </div>
            </div>
          </div>

          <div class="ca-stats">
            <div><small>Level</small><b>{{ path(selected).length }}</b></div>
            <div><small>Child accounts</small><b>{{ children(selected.accountId).length }}</b></div>
            <div :class="{ warn: !selected.segmentValueCode }">
              <small>Account segment value</small>
              <b>{{ selected.segmentValueCode ?? 'Not linked' }}</b>
              <small v-if="accountSegment">Segment {{ accountSegment.segmentNum }} · {{ accountSegment.segmentNameEn }}</small>
            </div>
          </div>

          <h4 class="ca-sec">Classification</h4>
          <div class="ca-grid">
            <div><small>Account type</small><b>{{ codeLabel(selected.accountType) }}</b></div>
            <div><small>Normal balance</small><b>{{ selected.normalBalance === 'DR' ? 'Debit' : 'Credit' }}</b></div>
            <div><small>Financial statement</small><b>{{ codeLabel(selected.financialStatementClass) }}</b></div>
            <div><small>Cash flow class</small><b>{{ codeLabel(selected.cashFlowClass) }}</b></div>
            <div>
              <small>Parent account</small>
              <b>{{ selected.parentAccountId && byId.get(selected.parentAccountId) ? `${byId.get(selected.parentAccountId)!.accountCode} - ${byId.get(selected.parentAccountId)!.accountNameEn}` : 'Top level' }}</b>
            </div>
            <div>
              <small>Effective</small>
              <b>{{ formatDate(selected.effectiveStartDate) }}{{ selected.effectiveEndDate ? ` – ${formatDate(selected.effectiveEndDate)}` : ' onwards' }}</b>
            </div>
            <div v-if="selected.description" class="ca-grid__wide"><small>Description</small><b>{{ selected.description }}</b></div>
          </div>

          <h4 class="ca-sec">Controls</h4>
          <div class="ca-flags">
            <div v-for="[key, label] in FLAGS" :key="key" class="ca-flag" :class="{ on: selected[key] }">
              <i :class="selected[key] ? 'pi pi-check' : 'pi pi-minus'"></i>{{ label }}
            </div>
          </div>

          <template v-if="children(selected.accountId).length">
            <h4 class="ca-sec">Child accounts</h4>
            <div class="ca-kids">
              <a v-for="k in children(selected.accountId)" :key="k.accountId" href="#" @click.prevent="select(k.accountId)">
                <code>{{ k.accountCode }}</code> {{ k.accountNameEn }}
              </a>
            </div>
          </template>

          <h4 class="ca-sec">Audit</h4>
          <div class="ca-grid">
            <div><small>Created</small><b>{{ selected.createdBy }}</b><small>{{ formatDateTime(selected.createdAt) }}</small></div>
            <div><small>Last updated</small><b>{{ selected.updatedBy ?? '–' }}</b><small>{{ selected.updatedAt ? formatDateTime(selected.updatedAt) : '' }}</small></div>
          </div>
        </template>
      </section>
    </div>
    <p v-if="structure" class="apex-ref">Replaces APEX App 120 pages 2/3. Balances by segment are added with journals and posting.</p>

    <GlDialog
      v-if="dialogOpen"
      :title="editing ? `Edit account ${form.accountCode}` : 'New GL account'"
      :subtitle="editing ? 'The account code cannot be changed: it is the value of the account segment.' : 'The account is saved together with its value in the account segment.'"
      :saving="saving"
      :error="errors.form"
      :save-label="editing ? 'Save account' : 'Create account'"
      wide
      @close="dialogOpen = false"
      @save="save"
    >
      <div class="cp-form__section">Account</div>
      <div class="cp-field">
        <label for="a-code">Natural account code *</label>
        <input id="a-code" v-model="form.accountCode as string" class="upper" maxlength="30" autocomplete="off" :disabled="editing" />
        <small v-if="errors.fields.accountCode" class="cp-field__error">{{ errors.fields.accountCode }}</small>
        <small v-else class="ca-rule" :class="{ ok: codeRule.ok === true, bad: codeRule.ok === false }">{{ codeRule.text }}</small>
      </div>
      <div class="cp-field">
        <label for="a-parent">Parent account</label>
        <select id="a-parent" v-model="form.parentAccountId">
          <option value="">Top level (no parent)</option>
          <option v-for="p in parentOptions" :key="p.accountId" :value="p.accountId">
            {{ p.accountCode }} - {{ p.accountNameEn }}{{ p.isSummary ? '' : ' (posting)' }}
          </option>
        </select>
        <small v-if="errors.fields.parentAccountId" class="cp-field__error">{{ errors.fields.parentAccountId }}</small>
        <small v-else-if="parentHint" class="cp-field__hint">{{ parentHint }}</small>
      </div>
      <div v-if="structure && data" class="cp-field cp-field--wide">
        <label>Account combination</label>
        <div class="ca-comb">
          <template v-for="(s, i) in data.segments" :key="s.segmentNum">
            <i v-if="i > 0">{{ structure.delimiterChar }}</i>
            <span v-if="s.segmentNum === structure.accountSegmentNum" class="me">{{ String(form.accountCode || '').toUpperCase() || '••••' }}</span>
            <span v-else :title="`${s.segmentNameEn}: chosen on each journal line`">{{ s.segmentNameEn }}</span>
          </template>
        </div>
        <small class="cp-field__hint">Only the account code is entered here. The other segments are chosen on each journal line when you post.</small>
      </div>
      <div class="cp-field">
        <label for="a-name">Name (English) *</label>
        <input id="a-name" v-model="form.accountNameEn as string" maxlength="240" />
        <small v-if="errors.fields.accountNameEn" class="cp-field__error">{{ errors.fields.accountNameEn }}</small>
      </div>
      <div class="cp-field">
        <label for="a-name-ar">Name (Arabic)</label>
        <input id="a-name-ar" v-model="form.accountNameAr as string" maxlength="240" dir="rtl" />
      </div>
      <div class="cp-field cp-field--wide">
        <label for="a-desc">Description</label>
        <input id="a-desc" v-model="form.description as string" maxlength="1000" />
      </div>

      <div class="cp-form__section">Classification</div>
      <div class="cp-field">
        <label for="a-type">Account type *</label>
        <select id="a-type" v-model="form.accountType" @change="onTypeChange">
          <option v-for="t in data?.lists.accountTypes ?? []" :key="t" :value="t">{{ codeLabel(t) }}</option>
        </select>
        <small v-if="errors.fields.accountType" class="cp-field__error">{{ errors.fields.accountType }}</small>
      </div>
      <div class="cp-field">
        <label for="a-nb">Normal balance *</label>
        <select id="a-nb" v-model="form.normalBalance">
          <option value="">– Select –</option>
          <option value="DR">Debit (DR)</option>
          <option value="CR">Credit (CR)</option>
        </select>
        <small v-if="errors.fields.normalBalance" class="cp-field__error">{{ errors.fields.normalBalance }}</small>
      </div>
      <div class="cp-field">
        <label for="a-fs">Financial statement *</label>
        <select id="a-fs" v-model="form.financialStatementClass">
          <option value="">– Select –</option>
          <option v-for="t in data?.lists.financialStatementClasses ?? []" :key="t" :value="t">{{ codeLabel(t) }}</option>
        </select>
        <small v-if="errors.fields.financialStatementClass" class="cp-field__error">{{ errors.fields.financialStatementClass }}</small>
      </div>
      <div class="cp-field">
        <label for="a-cf">Cash flow class *</label>
        <select id="a-cf" v-model="form.cashFlowClass">
          <option v-for="t in data?.lists.cashFlowClasses ?? []" :key="t" :value="t">{{ codeLabel(t) }}</option>
        </select>
      </div>
      <div class="cp-field">
        <label for="a-start">Effective from</label>
        <input id="a-start" v-model="form.effectiveStartDate as string" type="date" />
        <small class="cp-field__hint">Blank means today.</small>
      </div>
      <div class="cp-field">
        <label for="a-end">Effective to</label>
        <input id="a-end" v-model="form.effectiveEndDate as string" type="date" />
        <small v-if="errors.fields.effectiveEndDate" class="cp-field__error">{{ errors.fields.effectiveEndDate }}</small>
      </div>

      <div class="cp-form__section">Controls</div>
      <div class="gl-checks">
        <label class="gl-check"><input v-model="form.isSummary" type="checkbox" @change="onSummaryChange" /> Summary (parent) account</label>
        <label class="gl-check" :style="{ opacity: form.isSummary ? 0.55 : 1 }">
          <input v-model="form.isPostingAllowed" type="checkbox" :disabled="!!form.isSummary" /> Posting allowed
        </label>
        <label class="gl-check"><input v-model="form.isActive" type="checkbox" /> Active</label>
        <label v-for="[key, label] in FLAGS" :key="key" class="gl-check"><input v-model="form[key]" type="checkbox" /> {{ label }}</label>
      </div>
    </GlDialog>
  </div>
</template>

<style scoped>
.ca { display: grid; grid-template-columns: minmax(320px, 420px) 1fr; gap: 16px; align-items: start; }
.ca-tree { padding: 14px 0 10px; max-height: calc(100vh - 220px); display: flex; flex-direction: column; }
.ca-tools { display: flex; gap: 8px; padding: 0 14px 12px; border-bottom: 1px solid var(--cap-border-soft); }
.ca-search { position: relative; flex: 1; }
.ca-search i { position: absolute; left: 10px; top: 50%; transform: translateY(-50%); color: var(--cap-muted); font-size: 12px; }
.ca-search input { width: 100%; height: 36px; padding: 0 10px 0 30px; border: 1px solid var(--cap-border); border-radius: 7px; font: inherit; font-size: 12px; }
.ca-tools select { height: 36px; padding: 0 8px; border: 1px solid var(--cap-border); border-radius: 7px; font: inherit; font-size: 12px; background: #fff; }
.ca-groups { overflow-y: auto; padding: 6px 8px; }
.ca-group { margin-bottom: 8px; }
.ca-group__head { display: flex; align-items: center; gap: 8px; padding: 8px 8px 6px; color: var(--cap-heading); font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: .05em; border-inline-start: 3px solid var(--g); }
.ca-group__head small { color: var(--cap-muted); font-weight: 500; }
.ca-group--asset { --g: var(--cap-orange); }
.ca-group--liability { --g: var(--cap-primary); }
.ca-group--equity { --g: var(--cap-progress); }
.ca-group--income { --g: var(--cap-success); }
.ca-group--expense { --g: var(--cap-danger); }
.ca-add { margin-left: auto; width: 24px; height: 24px; display: grid; place-items: center; border: 1px solid var(--cap-border); border-radius: 6px; background: #fff; color: var(--cap-primary); cursor: pointer; font-size: 10px; }
.ca-none { padding: 4px 14px 8px; color: var(--cap-faint); font-size: 12px; }
.ca-row { width: 100%; display: flex; align-items: center; gap: 8px; min-height: 32px; padding: 4px 10px; border: 0; border-radius: 7px; background: none; text-align: start; font: inherit; font-size: 13px; color: var(--cap-ink); cursor: pointer; }
.ca-row:hover { background: var(--cap-bg); }
.ca-row.on { background: var(--cap-primary-tint); box-shadow: inset 3px 0 0 var(--cap-primary); }
.ca-row.off { color: var(--cap-muted); }
.ca-row.hit .ca-row__name { font-weight: 600; }
.ca-row code { font: 600 12px ui-monospace, Menlo, Consolas, monospace; color: var(--cap-primary-dark); }
.ca-row__name { flex: 1; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.ca-caret { width: 14px; font-size: 10px; color: var(--cap-muted); }
.ca-dot { font-size: 5px; color: var(--cap-faint); }
.ca-tag { padding: 1px 7px; border-radius: 999px; background: var(--cap-bg); color: var(--cap-ink-soft); font-size: 10px; font-weight: 600; }
.ca-tag--off { background: var(--cap-danger-bg); color: var(--cap-danger); }
.ca-det { padding: 0 0 18px; min-height: 360px; }
.ca-pick { display: flex; flex-direction: column; align-items: center; gap: 6px; padding: 80px 20px; }
.ca-pick i { font-size: 26px; color: var(--cap-faint); }
.ca-pick b { color: var(--cap-heading); }
.ca-det__head { padding: 16px 22px 14px; border-bottom: 1px solid var(--cap-border-soft); }
.ca-crumb { color: var(--cap-muted); font-size: 12px; }
.ca-crumb a { color: var(--cap-primary-dark); text-decoration: none; }
.ca-title { display: flex; justify-content: space-between; align-items: flex-start; gap: 14px; margin-top: 6px; }
.ca-title h2 { margin: 0; color: var(--cap-heading); font-size: 19px; font-weight: 600; }
.ca-title h2 code { margin-right: 6px; font: 700 17px ui-monospace, Menlo, Consolas, monospace; color: var(--cap-primary-dark); }
.ca-ar { display: block; margin-top: 2px; color: var(--cap-muted); font-size: 13px; direction: rtl; text-align: start; }
.ca-pills { display: flex; flex-wrap: wrap; gap: 6px; margin-top: 8px; }
.ca-acts { display: flex; gap: 8px; }
.ca-stats { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 12px; padding: 16px 22px 4px; }
.ca-stats > div { padding: 12px 14px; border: 1px solid var(--cap-border-soft); border-radius: 10px; }
.ca-stats > div.warn { border-color: var(--cap-warning-border); background: var(--cap-warning-bg); }
.ca-stats small, .ca-grid small { display: block; color: var(--cap-muted); font-size: 11px; }
.ca-stats b { display: block; margin: 3px 0; color: var(--cap-heading); font-size: 18px; font-weight: 600; }
.ca-sec { margin: 18px 22px 8px; color: var(--cap-heading); font-size: 12px; font-weight: 600; text-transform: uppercase; letter-spacing: .05em; }
.ca-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px 20px; padding: 0 22px; }
.ca-grid b { display: block; margin-top: 2px; color: var(--cap-ink); font-size: 13px; font-weight: 500; }
.ca-grid__wide { grid-column: 1 / -1; }
.ca-flags { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 6px; padding: 0 22px; }
.ca-flag { display: flex; align-items: center; gap: 8px; padding: 7px 10px; border-radius: 8px; background: var(--cap-bg); color: var(--cap-muted); font-size: 12px; }
.ca-flag i { font-size: 10px; }
.ca-flag.on { background: var(--cap-success-bg); color: var(--cap-success); font-weight: 600; }
.ca-kids { display: flex; flex-wrap: wrap; gap: 6px; padding: 0 22px; }
.ca-kids a { padding: 5px 10px; border: 1px solid var(--cap-border-soft); border-radius: 7px; color: var(--cap-ink); font-size: 12px; text-decoration: none; }
.ca-kids a:hover { border-color: var(--cap-primary); }
.ca-kids code { font: 600 11px ui-monospace, Menlo, Consolas, monospace; color: var(--cap-primary-dark); }
.ca-rule { display: block; margin-top: 5px; color: var(--cap-muted); font-size: 11px; }
.ca-rule.ok { color: var(--cap-success); }
.ca-rule.bad { color: var(--cap-danger); }
.ca-comb { display: flex; flex-wrap: wrap; align-items: center; gap: 5px; }
.ca-comb span { padding: 4px 9px; border: 1px dashed var(--cap-border); border-radius: 6px; color: var(--cap-muted); font-size: 12px; }
.ca-comb span.me { border: 1px solid var(--cap-primary); background: var(--cap-primary-tint); color: var(--cap-primary-dark); font: 700 12px ui-monospace, Menlo, Consolas, monospace; }
.ca-comb i { color: var(--cap-muted); font-style: normal; font-weight: 700; }
.apex-ref { margin: 12px 0 0; color: var(--cap-faint); font-size: 11px; text-align: end; }
@media (max-width: 1000px) { .ca { grid-template-columns: 1fr; } .ca-tree { max-height: none; } .ca-flags { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
</style>
