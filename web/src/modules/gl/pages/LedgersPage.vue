<script setup lang="ts">
// Ledgers: the books of each legal entity, with their currency, calendar, account structure and control accounts.
// APEX App 120 set its ledgers up by script (GL_CORE_SETUP_PKG.UPSERT_LEDGER); this screen does the same job.
import { computed, reactive, ref, shallowRef } from 'vue'
import { RouterLink } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { showSuccess } from '@/shared/feedback/feedback'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { formatDate, todayInCairo } from '@/shared/format/datetime'
import GlDialog from '../components/GlDialog.vue'
import GlPageHeader from '../components/GlPageHeader.vue'
import { blank, emptyErrors, numberOrNull, toFormErrors, type FormErrors } from '../components/formErrors'
import { codeLabel } from '../api/glSetup.api'
import { ledgersApi, type GlLedger, type LedgersPage } from '../api/glPeriods.api'

const canEdit = hasPermission('GL.SETUP.EDIT')

const page = shallowRef<LedgersPage | null>(null)
const loadError = ref('')
useFeedback({ errors: [loadError] })

async function load() {
  try {
    page.value = await ledgersApi.get()
  } catch (e) {
    loadError.value = e instanceof Error ? e.message : 'The ledgers could not be loaded.'
  }
}

const CONTROL_ACCOUNTS: [string, string][] = [
  ['retainedEarningsAcctCode', 'Retained earnings'],
  ['suspenseAcctCode', 'Suspense'],
  ['roundingDiffAcctCode', 'Rounding difference'],
  ['openingBalanceAcctCode', 'Opening balance'],
  ['fxGainAcctCode', 'FX gain'],
  ['fxLossAcctCode', 'FX loss']
]

function name(list: { code: string; name: string }[] | undefined, code: string | null) {
  return list?.find(x => x.code === code)?.name ?? code ?? ''
}

// ---------------------------------------------------------------- dialog
const dialogOpen = ref(false)
const editing = ref(false)
const saving = ref(false)
const errors = ref<FormErrors>(emptyErrors())
const form = reactive<Record<string, unknown>>({})
let editingId = 0
let editingVersion = 0

function openLedger(l: GlLedger | null) {
  Object.keys(form).forEach(k => delete form[k])
  const p = page.value
  if (l) {
    Object.assign(form, {
      ...l, ledgerNameAr: l.ledgerNameAr ?? '', coaId: l.coaId ?? '', endDate: l.endDate ?? '', notes: l.notes ?? '',
      ...Object.fromEntries(CONTROL_ACCOUNTS.map(([key]) => [key, l[key as keyof GlLedger] ?? '']))
    })
  } else {
    Object.assign(form, {
      legalEntityCode: p?.legalEntities[0]?.code ?? '', ledgerCode: '', ledgerNameEn: '', ledgerNameAr: '', ledgerType: 'PRIMARY',
      ledgerCurrencyCode: p?.currencies.find(c => c.code === 'EGP')?.code ?? p?.currencies[0]?.code ?? '',
      calendarId: p?.calendars[0]?.id ?? '', coaId: p?.structures[0]?.id ?? '', isPrimary: !p?.ledgers.length, ledgerStatus: 'ACTIVE',
      startDate: todayInCairo(), endDate: '', notes: '',
      ...Object.fromEntries(CONTROL_ACCOUNTS.map(([key]) => [key, '']))
    })
  }
  dialogOpen.value = true
  editing.value = !!l
  editingId = l?.ledgerId ?? 0
  editingVersion = l?.rowVersion ?? 0
  errors.value = emptyErrors()
}

/** Posting accounts of the structure chosen in the form. */
const accountOptions = computed(() => {
  const coaId = numberOrNull(form.coaId)
  return coaId ? page.value?.accounts.filter(a => a.coaId === coaId) ?? [] : []
})

function onStructureChange() {
  const codes = new Set(accountOptions.value.map(a => a.accountCode))
  for (const [key] of CONTROL_ACCOUNTS) if (form[key] && !codes.has(String(form[key]))) form[key] = ''
}

const editingLedger = computed(() => (editing.value ? page.value?.ledgers.find(l => l.ledgerId === editingId) ?? null : null))

async function save() {
  saving.value = true
  errors.value = emptyErrors()
  const names = Object.keys(form)
  try {
    const body = {
      ...form,
      ledgerCode: String(form.ledgerCode ?? '').trim().toUpperCase(), ledgerNameAr: blank(form.ledgerNameAr),
      calendarId: numberOrNull(form.calendarId), coaId: numberOrNull(form.coaId),
      startDate: blank(form.startDate), endDate: blank(form.endDate), notes: blank(form.notes),
      ...Object.fromEntries(CONTROL_ACCOUNTS.map(([key]) => [key, blank(form[key])])),
      rowVersion: editingVersion
    }
    const saved = editing.value ? await ledgersApi.update(editingId, body) : await ledgersApi.create(body)
    dialogOpen.value = false
    showSuccess(`Ledger ${saved.ledgerCode} saved.`)
    await load()
  } catch (e) {
    errors.value = toFormErrors(e, names)
  } finally {
    saving.value = false
  }
}

function closeDialog() {
  if (!saving.value) dialogOpen.value = false
}

load()
</script>

<template>
  <div class="cp-page">
    <GlPageHeader
      title="Ledgers"
      description="The books of each legal entity: currency, accounting calendar, account structure and the control accounts journals fall back on."
      icon="pi pi-wallet"
    >
      <button v-if="canEdit" type="button" class="cp-btn cp-btn--create" @click="openLedger(null)">
        <i class="pi pi-plus"></i><span>New ledger</span>
      </button>
    </GlPageHeader>

    <div v-if="!page" class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div>

    <section v-else class="cp-card">
      <div v-if="!page.ledgers.length" class="cp-empty lg-none">
        <b>No ledger yet</b>
        <span>A ledger needs a legal entity (<RouterLink to="/gl/reference-data">Reference data</RouterLink>) and a calendar (<RouterLink to="/gl/accounting-periods">Accounting periods</RouterLink>).</span>
      </div>
      <div v-else class="cp-table-wrap">
        <table class="cp-table">
          <thead>
            <tr>
              <th>Code</th><th>Name</th><th>Legal entity</th><th>Currency</th><th>Calendar</th><th>Structure</th><th>Type</th><th>Periods</th><th>Status</th><th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="l in page.ledgers" :key="l.ledgerId">
              <td class="nowrap"><code class="lg-code">{{ l.ledgerCode }}</code><span v-if="l.isPrimary" class="cp-pill cp-pill--info lg-primary">Primary</span></td>
              <td>{{ l.ledgerNameEn }}<span v-if="l.ledgerNameAr" class="ar">{{ l.ledgerNameAr }}</span></td>
              <td>{{ name(page.legalEntities, l.legalEntityCode) }}</td>
              <td>{{ l.ledgerCurrencyCode }}</td>
              <td><code class="lg-code">{{ l.calendarCode }}</code></td>
              <td><code v-if="l.coaCode" class="lg-code">{{ l.coaCode }}</code><span v-else class="cp-muted">–</span></td>
              <td>{{ codeLabel(l.ledgerType) }}</td>
              <td class="num">{{ l.periodCount }}</td>
              <td><span class="cp-pill" :class="l.ledgerStatus === 'ACTIVE' ? 'cp-pill--ok' : 'cp-pill--bad'">{{ codeLabel(l.ledgerStatus) }}</span></td>
              <td>
                <button type="button" class="cp-btn cp-btn--small" :aria-label="`${canEdit ? 'Edit' : 'View'} ${l.ledgerCode}`" @click="openLedger(l)">
                  <i :class="canEdit ? 'pi pi-pencil' : 'pi pi-eye'"></i>
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
    <p class="apex-ref">APEX App 120 had no ledger page (ledgers were set up by script)</p>

    <GlDialog
      v-if="dialogOpen"
      :title="editing ? `Edit ledger ${form.ledgerCode}` : 'New ledger'"
      :subtitle="editing ? 'The code identifies the ledger and cannot be changed.' : 'One primary ledger per legal entity.'"
      :saving="saving"
      :error="errors.form"
      :readonly="!canEdit"
      save-label="Save ledger"
      wide
      @close="closeDialog"
      @save="save"
    >
      <div class="cp-form__section">Ledger</div>
      <div class="cp-field">
        <label for="l-code">Ledger code *</label>
        <input id="l-code" v-model="form.ledgerCode as string" class="upper" maxlength="30" :disabled="editing" />
        <small v-if="errors.fields.ledgerCode" class="cp-field__error">{{ errors.fields.ledgerCode }}</small>
      </div>
      <div class="cp-field">
        <label for="l-le">Legal entity *</label>
        <select id="l-le" v-model="form.legalEntityCode">
          <option value="">–</option>
          <option v-for="o in page?.legalEntities" :key="o.code" :value="o.code">{{ o.code }} - {{ o.name }}</option>
        </select>
        <small v-if="errors.fields.legalEntityCode" class="cp-field__error">{{ errors.fields.legalEntityCode }}</small>
      </div>
      <div class="cp-field">
        <label for="l-name">Name (English) *</label>
        <input id="l-name" v-model="form.ledgerNameEn as string" maxlength="150" />
        <small v-if="errors.fields.ledgerNameEn" class="cp-field__error">{{ errors.fields.ledgerNameEn }}</small>
      </div>
      <div class="cp-field">
        <label for="l-name-ar">Name (Arabic)</label>
        <input id="l-name-ar" v-model="form.ledgerNameAr as string" maxlength="150" dir="rtl" />
      </div>
      <div class="cp-field">
        <label for="l-type">Ledger type</label>
        <select id="l-type" v-model="form.ledgerType">
          <option v-for="t in page?.ledgerTypes" :key="t" :value="t">{{ codeLabel(t) }}</option>
        </select>
      </div>
      <div class="cp-field">
        <label for="l-status">Status</label>
        <select id="l-status" v-model="form.ledgerStatus">
          <option v-for="t in page?.ledgerStatuses" :key="t" :value="t">{{ codeLabel(t) }}</option>
        </select>
      </div>

      <div class="cp-form__section">Books</div>
      <div class="cp-field">
        <label for="l-cur">Currency *</label>
        <select id="l-cur" v-model="form.ledgerCurrencyCode">
          <option value="">–</option>
          <option v-for="o in page?.currencies" :key="o.code" :value="o.code">{{ o.code }} - {{ o.name }}</option>
        </select>
        <small v-if="errors.fields.ledgerCurrencyCode" class="cp-field__error">{{ errors.fields.ledgerCurrencyCode }}</small>
      </div>
      <div class="cp-field">
        <label for="l-cal">Calendar *</label>
        <select id="l-cal" v-model="form.calendarId" :disabled="!!editingLedger?.periodCount">
          <option value="">–</option>
          <option v-for="o in page?.calendars" :key="o.code" :value="o.id">{{ o.code }} - {{ o.name }}</option>
        </select>
        <small v-if="errors.fields.calendarId" class="cp-field__error">{{ errors.fields.calendarId }}</small>
        <small v-else-if="editingLedger?.periodCount" class="cp-field__hint">The ledger has {{ editingLedger.periodCount }} periods on this calendar.</small>
      </div>
      <div class="cp-field">
        <label for="l-coa">Account structure</label>
        <select id="l-coa" v-model="form.coaId" @change="onStructureChange">
          <option value="">–</option>
          <option v-for="o in page?.structures" :key="o.code" :value="o.id">{{ o.code }} - {{ o.name }}</option>
        </select>
        <small v-if="errors.fields.coaId" class="cp-field__error">{{ errors.fields.coaId }}</small>
      </div>
      <div class="gl-checks lg-check">
        <label class="gl-check"><input v-model="form.isPrimary" type="checkbox" /> Primary ledger of the legal entity</label>
      </div>

      <div class="cp-form__section">Control accounts</div>
      <div v-for="[key, label] in CONTROL_ACCOUNTS" :key="key" class="cp-field">
        <label :for="`l-${key}`">{{ label }}</label>
        <select :id="`l-${key}`" v-model="form[key]" :disabled="!accountOptions.length">
          <option value="">–</option>
          <option v-for="a in accountOptions" :key="a.accountCode" :value="a.accountCode">{{ a.accountCode }} - {{ a.accountNameEn }}</option>
        </select>
        <small v-if="errors.fields[key]" class="cp-field__error">{{ errors.fields[key] }}</small>
      </div>
      <p v-if="!accountOptions.length" class="cp-field--wide lg-hint">Choose an account structure with posting accounts to pick the control accounts.</p>

      <div class="cp-form__section">Validity</div>
      <div class="cp-field">
        <label for="l-start">Start date</label>
        <input id="l-start" v-model="form.startDate as string" type="date" />
      </div>
      <div class="cp-field">
        <label for="l-end">End date</label>
        <input id="l-end" v-model="form.endDate as string" type="date" />
        <small v-if="errors.fields.endDate" class="cp-field__error">{{ errors.fields.endDate }}</small>
      </div>
      <div class="cp-field cp-field--wide">
        <label for="l-notes">Notes</label>
        <textarea id="l-notes" v-model="form.notes as string" rows="2" maxlength="1000"></textarea>
      </div>
      <p v-if="editingLedger" class="cp-field--wide lg-hint">Started {{ formatDate(editingLedger.startDate) }} · {{ editingLedger.periodCount }} ledger periods.</p>
    </GlDialog>
  </div>
</template>

<style scoped>
.lg-none { display: flex; flex-direction: column; gap: 6px; }
.lg-none b { color: var(--cap-heading); }
.lg-none a { color: var(--cap-primary); font-weight: 600; }
.lg-code { font: 600 12px ui-monospace, Menlo, Consolas, monospace; color: var(--cap-primary-dark); }
.lg-primary { margin-left: 6px; }
.nowrap { white-space: nowrap; }
.num { font-variant-numeric: tabular-nums; }
td .ar { display: block; color: var(--cap-muted); font-size: 11px; direction: rtl; text-align: start; }
.lg-check { grid-column: auto; grid-template-columns: 1fr; align-self: end; }
.lg-hint { margin: 0; color: var(--cap-muted); font-size: 12px; }
.apex-ref { margin: 12px 0 0; color: var(--cap-faint); font-size: 11px; text-align: end; }
</style>
