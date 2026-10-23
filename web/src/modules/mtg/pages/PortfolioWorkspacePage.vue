<script setup lang="ts">
// Portfolio overview (APEX App 105 page 203): value, terms, investors, accounts, project and history of one portfolio.
import { computed, ref, shallowRef, watch } from 'vue'
import { useRouter } from 'vue-router'
import CrudPage from '@/shared/crud/CrudPage.vue'
import type { CrudApi, CrudColumn, CrudField, FieldOption, FormValues } from '@/shared/crud/types'
import { hasPermission } from '@/shared/auth/session'
import { formatDate, formatDateTime } from '@/shared/format/datetime'
import { formatAmount, formatPct, formatShort } from '@/shared/format/money'
import { investorsApi, projectUnitsApi } from '../api/masterData.api'
import {
  portfolioAccountsApi,
  portfoliosApi,
  type Portfolio,
  type PortfolioAccount,
  type PortfolioWorkspace
} from '../api/portfolios.api'
import FinancialTermsDialog from '../components/FinancialTermsDialog.vue'
import PortfolioFormDialog from '../components/PortfolioFormDialog.vue'

type Tab = 'inv' | 'acc' | 'prj' | 'hist'

/** Each workflow step has its own page; the overview only links to them. */
const workflowSteps = [
  { label: 'Excel import', route: 'mtg-step-excel-import', icon: 'pi pi-file-excel' },
  { label: 'AML review', route: 'mtg-step-aml-review', icon: 'pi pi-shield' },
  { label: 'Inquiries', route: 'mtg-step-inquiries', icon: 'pi pi-comments' },
  { label: 'Legal review', route: 'mtg-step-legal-review', icon: 'pi pi-briefcase' }
]

const props = defineProps<{ portfolioId: string }>()
const router = useRouter()
const canEdit = hasPermission('MTG.PORTFOLIO.EDIT')

const data = shallowRef<PortfolioWorkspace | null>(null)
const error = ref('')
const tab = ref<Tab>('inv')
const investorSearch = ref('')
const editing = ref(false)
const editingTerms = ref(false)
const investorOptions = shallowRef<FieldOption[]>([])
const unitOptions = shallowRef<FieldOption[]>([])

const id = computed(() => Number(props.portfolioId))
const p = computed(() => data.value?.portfolio ?? null)
const f = computed(() => data.value?.figures ?? null)

async function load() {
  error.value = ''
  try {
    data.value = await portfoliosApi.workspace(id.value)
  } catch (e) {
    error.value = e instanceof Error ? e.message : 'Unable to load the portfolio.'
  }
}

watch(id, async () => {
  data.value = null
  await load()
  await loadOptions()
}, { immediate: true })

/** LOVs of page 205: active investors, and active units of the portfolio's project ("UNIT - type"). */
async function loadOptions() {
  if (!data.value) return
  const [investors, units] = await Promise.all([
    investorsApi.list(),
    projectUnitsApi.list(`?projectId=${data.value.portfolio.projectId}`)
  ])
  investorOptions.value = investors
    .filter(i => i.isActive)
    .map(i => ({ value: i.investorId, label: `${i.investorCode} - ${i.fullNameEn || i.fullNameAr}` }))
  unitOptions.value = units
    .filter(u => u.isActive)
    .map(u => ({ value: u.projectUnitId, label: `${u.unitCode} - ${u.unitTypeName ?? '-'}` }))
}

const investors = computed(() => {
  const q = investorSearch.value.trim().toLowerCase()
  return (data.value?.investors ?? []).filter(i =>
    !q || [i.investorCode, i.fullNameEn ?? '', i.fullNameAr, i.nationalIdNo ?? ''].some(v => v.toLowerCase().includes(q)))
})

function pctOf(part: number, whole: number, digits = 0): string {
  return whole > 0 ? `${(Math.round((part / whole) * 100 * 10 ** digits) / 10 ** digits).toString()}%` : ''
}

function initials(name: string): string {
  const words = name.replace(/[^\p{L} ]/gu, ' ').trim().split(/\s+/)
  return ((words[0]?.[0] ?? '?') + (words.length > 1 ? (words.at(-1)?.[0] ?? '') : (words[0]?.[1] ?? ''))).toUpperCase()
}

// ----- accounts tab (page 205 form) -----
const accountsApi = computed<CrudApi<PortfolioAccount>>(() => {
  const api = portfolioAccountsApi(id.value)
  // Every change recalculates the portfolio totals on the server, so the figures above are reloaded too.
  return {
    list: api.list,
    create: async v => { const r = await api.create(v); await load(); return r },
    update: async (row, v) => { const r = await api.update(row, v); await load(); return r },
    remove: async row => { await api.remove(row); await load() }
  }
})

const accountColumns: CrudColumn<PortfolioAccount>[] = [
  { field: 'investorCode', label: 'Investor code', type: 'code' },
  { field: 'investorName', label: 'Investor' },
  { field: 'unitCode', label: 'Unit' },
  { field: 'contractDate', label: 'Contract date', type: 'date' },
  { field: 'contractAmount', label: 'Contract value', type: 'amount' },
  { field: 'paidAmount', label: 'Paid value', type: 'amount' },
  { field: 'outstandingAmount', label: 'Outstanding', type: 'amount' },
  { field: 'installmentAmount', label: 'Installment', type: 'amount' },
  { field: 'regularPaymentMonths', label: 'Regular months', type: 'number' },
  { field: 'arrearsAmount', label: 'Arrears', type: 'amount' },
  { field: 'accountStatus', label: 'Account status' }
]

const accountFields = computed<CrudField[]>(() => [
  { name: 'investorId', label: 'Investor', type: 'select', required: true, options: investorOptions.value },
  { name: 'projectUnitId', label: 'Unit', type: 'select', required: true, options: unitOptions.value },
  { name: 'bookingDate', label: 'Booking Date', type: 'date' },
  { name: 'contractDate', label: 'Contract Date', type: 'date' },
  { name: 'contractAmount', label: 'Contract Value', type: 'amount', required: true, min: 0 },
  { name: 'paidAmount', label: 'Paid Value', type: 'amount', required: true, min: 0 },
  { name: 'outstandingAmount', label: 'Outstanding Value', type: 'amount', required: true, min: 0 },
  { name: 'arrearsAmount', label: 'Arrears Amount', type: 'amount', required: true, min: 0 },
  { name: 'installmentAmount', label: 'Installment Amount', type: 'amount', min: 0 },
  { name: 'installmentFrequency', label: 'Installment Frequency', type: 'text', maxLength: 30 },
  { name: 'regularPaymentMonths', label: 'Regular Payment Months', type: 'number', required: true, min: 0 },
  { name: 'lastPaymentDate', label: 'Last Payment Date', type: 'date' },
  { name: 'accountStatus', label: 'Account Status', type: 'text', maxLength: 30 }
])

function newAccount(): FormValues {
  return { arrearsAmount: 0, regularPaymentMonths: 0 }
}

function onSaved(saved: Portfolio) {
  editing.value = false
  editingTerms.value = false
  if (data.value) data.value = { ...data.value, portfolio: saved }
  load()
}

function onDeleted() {
  editing.value = false
  router.push({ name: 'mtg-portfolios' })
}
</script>

<template>
  <div class="cp-page">
    <nav class="crumbs" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'mtg-portfolios' }"><i class="pi pi-arrow-left"></i> Portfolio cases</RouterLink>
      <span v-if="p">/ {{ p.portfolioNo }}</span>
    </nav>

    <div v-if="error" class="cp-error">{{ error }}</div>
    <div v-else-if="!data || !p || !f" class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div>

    <template v-else>
      <header class="cp-card hero">
        <div>
          <div class="hero__eyebrow">Purchased portfolio</div>
          <h1>{{ p.portfolioNo }}</h1>
          <div class="hero__sub">
            <span><i class="pi pi-building"></i>{{ p.developerName }}</span>
            <span><i class="pi pi-map-marker"></i>{{ p.projectName }}</span>
            <span><i class="pi pi-calendar"></i>Requested {{ formatDate(p.requestDate + 'T00:00:00Z') }}</span>
            <span><i class="pi pi-wallet"></i>{{ p.currencyCode }}</span>
          </div>
        </div>
        <span class="cp-spacer"></span>
        <div class="hero__status">
          <small>Status</small>
          <span class="cp-pill cp-pill--info">{{ p.portfolioStatusName }}</span>
        </div>
        <div class="hero__actions">
          <RouterLink class="cp-btn" :to="{ name: 'mtg-portfolio-payments', params: { portfolioId: p.portfolioId } }">
            <i class="pi pi-history"></i><span>Payment history</span>
          </RouterLink>
          <button type="button" class="cp-btn" @click="editing = true">
            <i :class="canEdit ? 'pi pi-pencil' : 'pi pi-eye'"></i><span>{{ canEdit ? 'Edit portfolio' : 'Portfolio details' }}</span>
          </button>
        </div>
      </header>

      <section class="cp-card gap">
        <div class="cp-figures">
          <div class="cp-figure" :title="formatAmount(f.faceValueAmount)">
            <div class="cp-figure__label">Face value</div>
            <div class="cp-figure__value">{{ formatShort(f.faceValueAmount) }}</div>
            <div class="cp-figure__sub">{{ f.accountCount }} contracts</div>
          </div>
          <div class="cp-figure" :title="formatAmount(f.outstandingAmount)">
            <div class="cp-figure__label">Outstanding</div>
            <div class="cp-figure__value">{{ formatShort(f.outstandingAmount) }}</div>
            <div class="cp-figure__sub">{{ pctOf(f.outstandingAmount, f.faceValueAmount) }} {{ f.faceValueAmount > 0 ? 'of face value' : '' }}</div>
          </div>
          <div class="cp-figure" :title="formatAmount(f.proposedPurchaseAmount)">
            <div class="cp-figure__label">Proposed purchase</div>
            <div class="cp-figure__value">{{ formatShort(f.proposedPurchaseAmount) }}</div>
            <div class="cp-figure__sub">
              {{ f.outstandingAmount > 0 && f.proposedPurchaseAmount > 0 ? `${pctOf(f.proposedPurchaseAmount, f.outstandingAmount, 1)} of outstanding` : '' }}
            </div>
          </div>
          <div class="cp-figure" :title="formatAmount(f.expectedReturnAmount)">
            <div class="cp-figure__label">Expected return</div>
            <div class="cp-figure__value">{{ formatShort(f.expectedReturnAmount) }}</div>
            <div class="cp-figure__sub" :class="{ bad: f.arrearsAmount > 0 }">
              {{ f.arrearsAmount > 0 ? `${formatShort(f.arrearsAmount)} in arrears` : '' }}
            </div>
          </div>
        </div>
      </section>

      <div class="grid2 gap">
        <section class="cp-card">
          <div class="cp-card__head">
            <i class="pi pi-percentage"></i><h3>Pricing &amp; terms</h3><span class="cp-spacer"></span>
            <button v-if="canEdit" type="button" class="cp-btn cp-btn--small" @click="editingTerms = true">
              <i class="pi pi-pencil"></i><span>Edit rates &amp; fees</span>
            </button>
          </div>
          <dl class="cp-kv cp-card__body">
            <dt>Currency</dt><dd>{{ p.currencyCode }}</dd>
            <dt>Late fee rate</dt><dd>{{ formatPct(p.lateFeeRate) }}</dd>
            <dt>PV rate</dt><dd>{{ formatPct(p.pvRate) }}</dd>
            <dt>Reserved amount</dt><dd>{{ formatPct(p.reservedAmountPct) }}</dd>
            <dt>Term</dt><dd>{{ p.termMonths === null ? '–' : `${p.termMonths} months` }}</dd>
            <dt>Coverage</dt><dd>{{ formatPct(p.coveragePct) }}</dd>
            <dt>Margin</dt><dd>{{ formatPct(p.marginPct) }}</dd>
            <dt>Admin expense</dt><dd>{{ formatPct(p.adminExpensePct) }}</dd>
            <dt>Legal fee / unit</dt><dd>{{ p.legalFeePerUnitAmount === null ? '–' : `${formatAmount(p.legalFeePerUnitAmount)} ${p.currencyCode}` }}</dd>
            <dt>Valuation fee / unit</dt><dd>{{ p.valuationFeePerUnitAmount === null ? '–' : `${formatAmount(p.valuationFeePerUnitAmount)} ${p.currencyCode}` }}</dd>
          </dl>
        </section>
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-briefcase"></i><h3>Case</h3></div>
          <dl class="cp-kv cp-card__body">
            <dt>Case age</dt><dd>{{ f.ageDays }} days</dd>
            <dt>Contracts</dt><dd>{{ f.accountCount }}</dd>
            <dt>Investors</dt><dd>{{ f.investorCount }}</dd>
            <dt>Units</dt><dd>{{ f.unitCount }}</dd>
            <dt>Paid to date</dt><dd>{{ formatAmount(f.paidAmount) }}</dd>
            <dt>Arrears</dt><dd :class="{ bad: f.arrearsAmount > 0 }">{{ formatAmount(f.arrearsAmount) }}</dd>
          </dl>
          <div v-if="p.comments" class="note"><b>Comments</b>{{ p.comments }}</div>
        </section>
      </div>

      <nav class="cp-card steps gap" aria-label="Workflow steps">
        <span class="steps__label">Workflow steps</span>
        <RouterLink v-for="s in workflowSteps" :key="s.route" class="cp-btn cp-btn--small" :to="{ name: s.route, params: { portfolioId: p.portfolioId } }">
          <i :class="s.icon"></i><span>{{ s.label }}</span>
        </RouterLink>
      </nav>

      <div class="tabs gap" role="tablist">
        <button type="button" role="tab" :aria-selected="tab === 'inv'" @click="tab = 'inv'"><i class="pi pi-users"></i>Investors<em>{{ f.investorCount }}</em></button>
        <button type="button" role="tab" :aria-selected="tab === 'acc'" @click="tab = 'acc'"><i class="pi pi-file"></i>Accounts<em>{{ f.accountCount }}</em></button>
        <button type="button" role="tab" :aria-selected="tab === 'prj'" @click="tab = 'prj'"><i class="pi pi-building"></i>Project &amp; units<em>{{ f.unitCount }}</em></button>
        <button type="button" role="tab" :aria-selected="tab === 'hist'" @click="tab = 'hist'"><i class="pi pi-history"></i>Status history<em>{{ data.statusHistory.length }}</em></button>
      </div>

      <section v-if="tab === 'inv'" class="cp-card" role="tabpanel">
        <div class="cp-card__head">
          <label class="search"><i class="pi pi-search"></i><input v-model="investorSearch" type="search" placeholder="Search investor, code or national ID" /></label>
        </div>
        <div class="cp-table-wrap">
          <table class="cp-table">
            <thead><tr><th>Investor</th><th>Contracts</th><th class="cp-num">Contract value</th><th class="cp-num">Outstanding</th></tr></thead>
            <tbody>
              <tr v-for="i in investors" :key="i.investorId">
                <td>
                  <div class="who">
                    <span class="avatar">{{ initials(i.fullNameEn || i.investorCode) }}</span>
                    <div>
                      <strong>{{ i.fullNameEn || i.fullNameAr }}</strong>
                      <small v-if="i.fullNameEn" dir="rtl">{{ i.fullNameAr }}</small>
                      <span class="code">{{ i.investorCode }}</span>
                    </div>
                  </div>
                </td>
                <td>{{ i.contractCount }}</td>
                <td class="cp-num"><b :title="formatAmount(i.contractAmount)">{{ formatShort(i.contractAmount) }}</b><small class="cp-muted"> {{ pctOf(i.paidAmount, i.contractAmount) }} paid</small></td>
                <td class="cp-num"><b :title="formatAmount(i.outstandingAmount)">{{ formatShort(i.outstandingAmount) }}</b></td>
              </tr>
              <tr v-if="investors.length === 0"><td colspan="4" class="cp-empty">{{ data.investors.length ? 'No investors match.' : 'No investors in this portfolio yet. Add the accounts first.' }}</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <div v-else-if="tab === 'acc'" role="tabpanel">
        <CrudPage
          title="Accounts"
          description="Each investor's contract for a unit. Saving an account recalculates the portfolio's face value and outstanding."
          icon="pi pi-file"
          apex-pages="203/205"
          entity-label="Account"
          :columns="accountColumns"
          :fields="accountFields"
          :api="accountsApi"
          :row-key="row => row.portfolioAccountId"
          :new-values="newAccount"
          :to-values="row => ({ ...row })"
          :can-edit="canEdit"
          :has-active="false"
          embedded
        />
      </div>

      <div v-else-if="tab === 'prj'" class="grid2" role="tabpanel">
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-building"></i><h3>Project</h3></div>
          <dl class="cp-kv cp-card__body">
            <dt>Project</dt><dd>{{ data.project.projectNameEn || data.project.projectNameAr }}</dd>
            <dt v-if="data.project.projectNameEn">Arabic name</dt><dd v-if="data.project.projectNameEn" dir="rtl">{{ data.project.projectNameAr }}</dd>
            <dt>Project code</dt><dd>{{ data.project.projectCode }}</dd>
            <dt>Developer</dt><dd>{{ data.project.developerName }}</dd>
            <dt>Developer code</dt><dd>{{ data.project.developerCode }}</dd>
            <dt>Units in project</dt><dd>{{ data.project.unitsInProject }}</dd>
            <dt>Units in this portfolio</dt><dd>{{ f.unitCount }}</dd>
            <template v-if="data.project.locationText"><dt>Location</dt><dd>{{ data.project.locationText }}</dd></template>
            <template v-if="data.project.landArea !== null"><dt>Land area</dt><dd>{{ formatAmount(data.project.landArea) }}</dd></template>
            <template v-if="data.project.titleDeedNo"><dt>Title deed no.</dt><dd>{{ data.project.titleDeedNo }}</dd></template>
            <template v-if="data.project.buildingLicenseNo"><dt>Building license no.</dt><dd>{{ data.project.buildingLicenseNo }}</dd></template>
            <template v-if="data.project.buildingLicenseDate"><dt>Building license date</dt><dd>{{ formatDate(data.project.buildingLicenseDate + 'T00:00:00Z') }}</dd></template>
            <template v-if="data.project.projectCompletionPct !== null"><dt>Completion</dt><dd>{{ formatPct(data.project.projectCompletionPct) }}</dd></template>
            <template v-if="data.project.projectStatus"><dt>Project status</dt><dd>{{ data.project.projectStatus }}</dd></template>
          </dl>
        </section>
        <section class="cp-card">
          <div class="cp-card__head"><i class="pi pi-th-large"></i><h3>Units in this portfolio</h3><span class="cp-spacer"></span><span class="cp-muted">{{ data.units.length }}</span></div>
          <div class="cp-table-wrap">
            <table class="cp-table">
              <thead><tr><th>Unit</th><th>Type</th><th>Investor</th><th class="cp-num">Contract value</th></tr></thead>
              <tbody>
                <tr v-for="u in data.units" :key="u.portfolioAccountId">
                  <td><span class="code">{{ u.unitCode }}</span></td>
                  <td>{{ u.unitTypeName ?? '–' }}</td>
                  <td>{{ u.investorName }}</td>
                  <td class="cp-num"><b :title="formatAmount(u.contractAmount)">{{ formatShort(u.contractAmount) }}</b></td>
                </tr>
                <tr v-if="data.units.length === 0"><td colspan="4" class="cp-empty">No units linked yet.</td></tr>
              </tbody>
            </table>
          </div>
        </section>
      </div>

      <section v-else class="cp-card" role="tabpanel">
        <ul class="hist">
          <li v-for="(h, i) in data.statusHistory" :key="i">
            <i></i>
            <strong>{{ h.toStatusName ?? h.toStatusCode }}</strong>
            <span v-if="h.fromStatusCode" class="cp-muted"> from {{ h.fromStatusName ?? h.fromStatusCode }}</span>
            <small>{{ formatDateTime(h.changedAt) }} · {{ h.changedBy }}<template v-if="h.comments"> · {{ h.comments }}</template></small>
          </li>
          <li v-if="data.statusHistory.length === 0" class="cp-muted">No status changes yet.</li>
        </ul>
      </section>

      <p class="apex-ref">Replaces APEX App 105 pages 203 and 205</p>

      <PortfolioFormDialog v-if="editing" :portfolio="p" :can-edit="canEdit" @close="editing = false" @saved="onSaved" @deleted="onDeleted" />
      <FinancialTermsDialog v-if="editingTerms" :portfolio="p" @close="editingTerms = false" @saved="onSaved" />
    </template>
  </div>
</template>

<style scoped>
.steps { display: flex; flex-wrap: wrap; align-items: center; gap: 8px; padding: 12px 20px; }
.steps__label { margin-inline-end: 6px; color: #718579; font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: .04em; }
.crumbs { display: flex; gap: 8px; margin-bottom: 14px; color: #718579; font-size: 12px; }
.crumbs a { color: #0a3573; text-decoration: none; }
.crumbs a:hover { text-decoration: underline; }
.gap { margin-top: 16px; }
.hero { display: flex; flex-wrap: wrap; align-items: center; gap: 18px; padding: 22px 28px; }
.hero__eyebrow { color: #c47b2c; font-size: 11px; font-weight: 600; text-transform: uppercase; letter-spacing: .08em; }
.hero h1 { margin: 4px 0 8px; color: #0a3573; font-size: 26px; font-weight: 700; }
.hero__sub { display: flex; flex-wrap: wrap; gap: 16px; color: #556b7a; font-size: 13px; }
.hero__sub i { margin-inline-end: 6px; color: #8a9da6; font-size: 12px; }
.hero__status { display: flex; flex-direction: column; align-items: flex-start; gap: 6px; }
.hero__status small { color: #718579; font-size: 11px; text-transform: uppercase; letter-spacing: .04em; }
.hero__actions { display: flex; gap: 8px; }
.grid2 { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
.bad { color: #a53d3d !important; }
.note { margin: 0 20px 16px; padding: 10px 12px; border-radius: 10px; background: #f4f7fb; color: #29465a; font-size: 12px; white-space: pre-wrap; }
.note b { display: block; margin-bottom: 4px; color: #0a3573; }
.tabs { display: flex; flex-wrap: wrap; gap: 6px; margin-bottom: 12px; }
.tabs button { display: inline-flex; align-items: center; gap: 8px; height: 40px; padding: 0 16px; border: 1px solid #d5e1e8; border-radius: 10px; background: #fff; color: #405b4d; font: inherit; font-size: 13px; cursor: pointer; }
.tabs button em { min-width: 22px; padding: 1px 7px; border-radius: 999px; background: #eef2f6; color: #44586a; font-size: 11px; font-style: normal; font-weight: 600; }
.tabs button[aria-selected="true"] { border-color: #0a3573; background: #0a3573; color: #fff; }
.tabs button[aria-selected="true"] em { background: rgba(255, 255, 255, .2); color: #fff; }
.search { flex: 1; max-width: 420px; height: 38px; display: flex; align-items: center; gap: 9px; padding: 0 12px; border: 1px solid #d5e1e8; border-radius: 8px; }
.search i { color: #6f8990; font-size: 12px; }
.search input { width: 100%; border: 0; outline: 0; background: transparent; font: inherit; font-size: 12px; }
.who { display: flex; align-items: center; gap: 10px; }
.who strong { display: block; color: #183550; font-weight: 600; }
.who small { display: block; color: #718579; }
.avatar { width: 34px; height: 34px; flex: 0 0 34px; display: grid; place-items: center; border-radius: 50%; background: #0a3573; color: #fff; font-size: 12px; font-weight: 600; }
.code { color: #0a3573; font-weight: 600; font-size: 11px; }
.hist { margin: 0; padding: 18px 22px; list-style: none; }
.hist li { position: relative; padding: 0 0 16px 22px; font-size: 13px; }
.hist li > i { position: absolute; inset-inline-start: 0; top: 4px; width: 10px; height: 10px; border-radius: 50%; background: #15945b; }
.hist li small { display: block; margin-top: 3px; color: #718579; font-size: 11px; }
.apex-ref { margin: 14px 2px 0; color: #9aa9b4; font-size: 10px; }
@media (max-width: 900px) { .grid2 { grid-template-columns: 1fr; } }
</style>
