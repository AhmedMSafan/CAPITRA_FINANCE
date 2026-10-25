<script setup lang="ts">
// Workflow step "Excel Import & Validation" (APEX App 105 page 208): upload the seller workbook, fix what the
// validation finds, then import it into the portfolio.
import { computed, ref, shallowRef, watch } from 'vue'
import { hasPermission } from '@/shared/auth/session'
import { formatAmount, formatShort } from '@/shared/format/money'
import { useFeedback } from '@/shared/feedback/useFeedback'
import StepLayout from '../StepLayout.vue'
import { messageOf } from '../stepUtils'
import { excelImportApi, type ExcelImport, type ImportStatus, type WorkbookResult } from './excelImport.api'

type RowFilter = 'ALL' | 'VALID' | 'ERROR'

const props = defineProps<{ portfolioId: string }>()
const canRun = hasPermission('MTG.STEP.EXCEL_IMPORT')

const data = shallowRef<ExcelImport | null>(null)
const result = shallowRef<WorkbookResult | null>(null)
const file = shallowRef<File | null>(null)
const fileInput = ref<HTMLInputElement | null>(null)
const busy = ref<'' | 'validate' | 'import'>('')
const error = ref('')
const notice = ref('')
const rowFilter = ref<RowFilter>('ALL')
const search = ref('')
const showFormat = ref(false)

const id = computed(() => Number(props.portfolioId))

async function load() {
  error.value = ''
  try {
    data.value = await excelImportApi.get(id.value)
  } catch (e) {
    error.value = messageOf(e, 'Unable to load the portfolio.')
  }
}

watch(id, () => {
  data.value = null
  result.value = null
  file.value = null
  load()
}, { immediate: true })

/** Results of this visit win over the stored status: the browser holds the file that was checked. */
const status = computed<ImportStatus>(() => {
  if (result.value) return result.value.importStatusCode
  // A check made before workbooks were kept cannot be imported, so it counts as not validated.
  if (data.value?.importStatusCode === 'VALIDATED' && !data.value.hasValidatedWorkbook) return 'NOT_VALIDATED'
  return data.value?.importStatusCode ?? 'NOT_VALIDATED'
})
const imported = computed(() => status.value === 'IMPORTED')
/** The server keeps the last validated workbook, so a later visit can import it without choosing the file again. */
const storedReady = computed(() => !file.value && !result.value && !!data.value?.hasValidatedWorkbook)
const canImport = computed(() => canRun && (storedReady.value || (!!file.value && result.value?.importStatusCode === 'VALIDATED')))

const stages = computed(() => {
  const s = status.value
  return [
    { label: 'Upload workbook', state: s === 'NOT_VALIDATED' ? 'now' : 'done' },
    { label: 'Validate & fix', state: s === 'VALIDATED' || s === 'IMPORTED' ? 'done' : s === 'ERROR' ? 'now' : 'todo' },
    { label: 'Import & complete step', state: s === 'IMPORTED' ? 'done' : s === 'VALIDATED' ? 'now' : 'todo' }
  ] as { label: string; state: 'done' | 'now' | 'todo' }[]
})

const rows = computed(() => {
  const q = search.value.trim().toLowerCase()
  return (result.value?.rows ?? []).filter(r => {
    if (rowFilter.value === 'VALID' && !r.isValid) return false
    if (rowFilter.value === 'ERROR' && r.isValid) return false
    return !q || [r.code ?? '', r.name ?? '', r.message, r.sheet].some(v => v.toLowerCase().includes(q))
  })
})

function onFile(event: Event) {
  const input = event.target as HTMLInputElement
  file.value = input.files?.[0] ?? null
  // A new file has not been checked yet.
  result.value = null
  notice.value = ''
  error.value = ''
  // Check the file straight away, so the user does not have to press Validate.
  if (file.value && canRun) validate()
}

async function validate() {
  if (!file.value) {
    error.value = 'Choose the seller workbook (.xlsx) first.'
    return
  }
  busy.value = 'validate'
  error.value = ''
  notice.value = ''
  try {
    result.value = await excelImportApi.validate(id.value, file.value)
    rowFilter.value = result.value.errorRowCount > 0 ? 'ERROR' : 'ALL'
    notice.value = result.value.importStatusCode === 'VALIDATED'
      ? 'Workbook validated successfully. You can now import it.'
      : 'Validation completed with errors. Fix the workbook and upload it again.'
  } catch (e) {
    error.value = messageOf(e, 'Unable to validate the workbook.')
  } finally {
    busy.value = ''
  }
}

async function runImport() {
  busy.value = 'import'
  error.value = ''
  notice.value = ''
  try {
    result.value = await excelImportApi.import(id.value, file.value)
    if (result.value.importStatusCode === 'IMPORTED') {
      notice.value = `Workbook imported: ${result.value.importedRowCount} accounts added. This step is complete.`
      file.value = null
      if (fileInput.value) fileInput.value.value = ''
    } else {
      notice.value = 'The workbook has errors and was not imported.'
      rowFilter.value = 'ERROR'
    }
    await load()
  } catch (e) {
    error.value = messageOf(e, 'Unable to import the workbook.')
  } finally {
    busy.value = ''
  }
}

function pctOf(part: number, whole: number): string {
  return whole > 0 ? `${Math.round((part / whole) * 100)}%` : '0%'
}

// Messages open the app-wide pop-up; validating and importing show the progress screen.
useFeedback({
  errors: [error],
  notices: [notice],
  noticeKind: () => (status.value === 'ERROR' ? 'warning' : 'success'),
  busy,
  busyAs: value => value === 'import'
    ? { title: 'Importing the workbook', kind: 'excel', steps: ['Uploading the workbook', 'Checking every row again', 'Creating investors, units and accounts', 'Completing the step'] }
    : { title: 'Validating the workbook', kind: 'excel', steps: ['Uploading the workbook', 'Reading INVESTORS, UNITS and ACCOUNTS', 'Checking every row against the rules', 'Preparing the results'] }
})
</script>

<template>
  <div v-if="error && !data" class="cp-page"><div class="cp-error">{{ error }}</div></div>
  <div v-else-if="!data" class="cp-page"><div class="cp-empty"><i class="pi pi-spin pi-spinner"></i> Loading...</div></div>

  <StepLayout
    v-else
    :portfolio-id="data.portfolioId"
    :title="imported ? 'Portfolio data' : 'Load portfolio data'"
    :lead="imported
      ? 'The accounts loaded into this portfolio from the seller workbook.'
      : 'Upload the seller workbook, fix anything the validation finds, then import it. Importing completes this workflow step.'"
    :portfolio-no="data.portfolioNo"
    :project-name="data.projectName"
    :project-code="data.projectCode"
    :status-name="data.portfolioStatusName"
    :status-code="data.portfolioStatusCode"
    :stages="stages"
    apex-page="208"
  >

    <!-- upload and check -->
    <section v-if="!imported" class="cp-card">
      <div class="cp-card__head">
        <i class="pi pi-file-excel"></i>
        <h3>{{ status === 'NOT_VALIDATED' ? 'Upload the workbook' : 'Upload a corrected workbook' }}</h3>
        <span class="cp-spacer"></span>
        <button type="button" class="cp-btn cp-btn--small" @click="showFormat = true"><i class="pi pi-table"></i><span>Workbook format</span></button>
      </div>
      <div class="cp-card__body">
        <label class="drop">
          <i class="pi pi-cloud-upload"></i>
          <b>{{ file ? file.name : 'Choose the seller workbook (.xlsx)' }}</b>
          <small>Needs 3 tabs: <code>INVESTORS</code> <code>UNITS</code> <code>ACCOUNTS</code></small>
          <input
            id="workbook"
            ref="fileInput"
            type="file"
            accept=".xlsx,application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
            :disabled="!canRun || !!busy"
            @change="onFile"
          />
        </label>
        <p v-if="data.importStatusCode !== 'NOT_VALIDATED' && !result" class="cp-muted hint">
          Last check: <b>{{ data.fileName }}</b>, {{ data.validRowCount }} valid and {{ data.errorRowCount }} with errors.
          {{ data.hasValidatedWorkbook ? 'Ready: press Import & complete step, or choose another file.' : data.importStatusCode === 'VALIDATED' ? 'Click the cloud and choose the file once more; it is checked straight away.' : 'Choose a corrected file to continue.' }}
        </p>
        <p v-if="!canRun" class="cp-muted hint">You can view this step. Loading the workbook needs the Excel import permission.</p>
        <div class="actions">
          <button type="button" class="cp-btn" :disabled="!canRun || !file || !!busy" @click="validate">
            <i :class="busy === 'validate' ? 'pi pi-spin pi-spinner' : 'pi pi-check-square'"></i><span>Validate</span>
          </button>
          <button type="button" class="cp-btn cp-btn--primary" :disabled="!canImport || !!busy" @click="runImport">
            <i :class="busy === 'import' ? 'pi pi-spin pi-spinner' : 'pi pi-download'"></i><span>Import & complete step</span>
          </button>
        </div>
      </div>
    </section>

    <!-- validation results -->
    <section v-if="result && result.importStatusCode !== 'IMPORTED'" class="cp-card">
      <div class="cp-card__head">
        <i class="pi pi-list"></i><h3>Validation results</h3><span class="cp-spacer"></span>
        <span class="cp-muted">{{ rows.length }} of {{ result.totalRowCount }} rows</span>
      </div>
      <div class="sheets">
        <div v-for="s in result.sheets" :key="s.sheet" class="sheet">
          <div class="sheet__top">
            <b>{{ s.sheet }}</b>
            <span>{{ s.validRowCount }} valid<template v-if="s.errorRowCount"> · <em>{{ s.errorRowCount }} {{ s.errorRowCount === 1 ? 'error' : 'errors' }}</em></template> · {{ s.totalRowCount }}</span>
          </div>
          <div class="bar">
            <i class="bar__ok" :style="{ width: pctOf(s.validRowCount, s.totalRowCount) }"></i>
            <i class="bar__bad" :style="{ width: pctOf(s.errorRowCount, s.totalRowCount) }"></i>
          </div>
        </div>
      </div>
      <div class="tools">
        <label class="search"><i class="pi pi-search"></i><input v-model="search" type="search" placeholder="Search code, name or message" aria-label="Search rows" /></label>
        <div class="seg" role="group" aria-label="Rows">
          <button type="button" :aria-pressed="rowFilter === 'ALL'" @click="rowFilter = 'ALL'">All <em>{{ result.totalRowCount }}</em></button>
          <button type="button" :aria-pressed="rowFilter === 'VALID'" @click="rowFilter = 'VALID'">Valid <em>{{ result.validRowCount }}</em></button>
          <button type="button" :aria-pressed="rowFilter === 'ERROR'" @click="rowFilter = 'ERROR'">Errors <em>{{ result.errorRowCount }}</em></button>
        </div>
      </div>
      <div class="cp-table-wrap">
        <table class="cp-table">
          <thead><tr><th>Sheet</th><th>Row</th><th>Code</th><th>Name</th><th>Status</th><th>Message</th></tr></thead>
          <tbody>
            <tr v-for="r in rows" :key="`${r.sheet}-${r.rowNumber}`">
              <td>{{ r.sheet }}</td>
              <td>{{ r.rowNumber }}</td>
              <td><span class="code">{{ r.code ?? '-' }}</span></td>
              <td>{{ r.name ?? '-' }}</td>
              <td><span class="cp-pill" :class="r.isValid ? 'cp-pill--ok' : 'cp-pill--bad'">{{ r.isValid ? 'Valid' : 'Error' }}</span></td>
              <td class="msg" :class="{ bad: !r.isValid }">{{ r.message }}</td>
            </tr>
            <tr v-if="rows.length === 0"><td colspan="6" class="cp-empty">No rows match.</td></tr>
          </tbody>
        </table>
      </div>
    </section>

    <!-- what is in the portfolio now -->
    <section v-if="imported" class="cp-card">
      <div class="cp-card__head">
        <i class="pi pi-database"></i><h3>Now in the portfolio</h3><span class="cp-spacer"></span>
        <RouterLink class="cp-btn cp-btn--small" :to="{ name: 'mtg-portfolio', params: { portfolioId: data.portfolioId } }">Open portfolio <i class="pi pi-angle-right"></i></RouterLink>
      </div>
      <div class="kpis">
        <div><b>{{ data.figures.accountCount }}</b><small>Accounts</small></div>
        <div><b>{{ data.figures.investorCount }}</b><small>Investors</small></div>
        <div><b>{{ data.figures.unitCount }}</b><small>Units</small></div>
        <div><b :title="formatAmount(data.figures.contractAmount)">{{ formatShort(data.figures.contractAmount) }}</b><small>Contract value</small></div>
        <div><b :title="formatAmount(data.figures.paidAmount)">{{ formatShort(data.figures.paidAmount) }}</b><small>Paid · {{ pctOf(data.figures.paidAmount, data.figures.contractAmount) }}</small></div>
        <div><b :title="formatAmount(data.figures.outstandingAmount)">{{ formatShort(data.figures.outstandingAmount) }}</b><small>Outstanding</small></div>
        <div><b :class="{ bad: data.figures.arrearsAmount > 0 }" :title="formatAmount(data.figures.arrearsAmount)">{{ formatShort(data.figures.arrearsAmount) }}</b><small>Arrears</small></div>
      </div>
      <div class="cp-table-wrap scroll">
        <table class="cp-table">
          <thead><tr><th>Investor</th><th>Unit</th><th>Contract</th><th class="cp-num">Contract value</th><th class="cp-num">Paid</th><th class="cp-num">Outstanding</th><th class="cp-num">Arrears</th></tr></thead>
          <tbody>
            <tr v-for="a in data.accounts" :key="a.portfolioAccountId">
              <td><strong>{{ a.investorName }}</strong><br /><small class="cp-muted">{{ a.investorCode }}</small></td>
              <td>{{ a.unitCode }}</td>
              <td><span class="code">{{ a.sellerContractNo ?? '-' }}</span></td>
              <td class="cp-num">{{ formatAmount(a.contractAmount) }}</td>
              <td class="cp-num">{{ formatAmount(a.paidAmount) }}</td>
              <td class="cp-num">{{ formatAmount(a.outstandingAmount) }}</td>
              <td class="cp-num" :class="{ bad: a.arrearsAmount > 0 }">{{ formatAmount(a.arrearsAmount) }}</td>
            </tr>
            <tr v-if="data.accounts.length === 0"><td colspan="7" class="cp-empty">No accounts found for this portfolio.</td></tr>
          </tbody>
        </table>
      </div>
      <p v-if="data.figures.accountCount > data.accounts.length" class="cp-muted more">
        Showing the first {{ data.accounts.length }} of {{ data.figures.accountCount }}. See all in the portfolio's Accounts tab.
      </p>
    </section>

    <div v-if="showFormat" class="cp-modal-overlay" @click.self="showFormat = false">
      <section class="cp-modal" role="dialog" aria-modal="true" aria-labelledby="fmt-title">
        <header class="cp-modal__header">
          <div><h3 id="fmt-title">Workbook format</h3><p>One tab per sheet, a header row, then one row per record. Dates as YYYY-MM-DD.</p></div>
          <button type="button" class="cp-modal__close" aria-label="Close" @click="showFormat = false"><i class="pi pi-times"></i></button>
        </header>
        <div class="cp-modal__body">
          <div v-for="s in data.format" :key="s.sheet" class="fmt">
            <b>{{ s.sheet }}</b>
            <ol><li v-for="c in s.columns" :key="c"><code>{{ c }}</code></li></ol>
          </div>
          <p class="cp-muted">Every row starts with the portfolio number ({{ data.portfolioNo }}) and the project code ({{ data.projectCode }}).
            Investors and units that already exist are matched by code and kept as they are.</p>
        </div>
      </section>
    </div>
  </StepLayout>
</template>

<style scoped>
.notice { padding: 10px 14px; border: 1px solid var(--cap-success-border); border-radius: 8px; background: var(--cap-success-bg); color: var(--cap-success); font-size: 13px; }
.notice--bad { border-color: var(--cap-warning-border); background: var(--cap-warning-bg); color: var(--cap-warning); }
.drop { position: relative; display: flex; flex-direction: column; align-items: center; gap: 6px; padding: 28px 16px; border: 2px dashed var(--cap-border); border-radius: 12px; background: var(--cap-bg); color: var(--cap-ink-soft); text-align: center; cursor: pointer; }
.drop:hover { border-color: var(--cap-faint); }
.drop > i { margin-bottom: 4px; color: var(--cap-orange); font-size: 56px; }
.drop b { color: var(--cap-heading); font-size: 14px; }
.drop small { color: var(--cap-muted); font-size: 12px; }
.drop code { margin: 0 2px; padding: 1px 6px; border-radius: 4px; background: var(--cap-primary-tint); color: var(--cap-heading); font-size: 11px; }
.drop input { position: absolute; inset: 0; opacity: 0; cursor: pointer; }
.drop input:disabled { cursor: default; }
.hint { margin: 10px 0 0; font-size: 12px; }
.actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 14px; }
.sheets { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 14px; padding: 16px 20px 6px; }
.sheet__top { display: flex; justify-content: space-between; gap: 8px; color: var(--cap-muted); font-size: 12px; }
.sheet__top b { color: var(--cap-heading); }
.sheet__top em { color: var(--cap-danger); font-style: normal; font-weight: 600; }
.bar { display: flex; height: 6px; margin-top: 6px; overflow: hidden; border-radius: 999px; background: var(--cap-border-soft); }
.bar__ok { background: var(--cap-success-strong); }
.bar__bad { background: var(--cap-danger); }
.tools { display: flex; flex-wrap: wrap; align-items: center; gap: 10px; padding: 12px 20px; }
.search { flex: 1; max-width: 420px; height: 38px; display: flex; align-items: center; gap: 9px; padding: 0 12px; border: 1px solid var(--cap-border); border-radius: 8px; }
.search i { color: var(--cap-muted); font-size: 12px; }
.search input { width: 100%; border: 0; outline: 0; background: transparent; font: inherit; font-size: 12px; }
.seg { display: inline-flex; border: 1px solid var(--cap-border); border-radius: 8px; overflow: hidden; }
.seg button { height: 36px; padding: 0 12px; border: 0; background: #fff; color: var(--cap-ink-soft); font: inherit; font-size: 12px; cursor: pointer; }
.seg button + button { border-inline-start: 1px solid var(--cap-border); }
.seg button em { margin-inline-start: 4px; color: var(--cap-muted); font-style: normal; }
.seg button[aria-pressed="true"] { background: var(--cap-selected); color: #fff; }
.seg button[aria-pressed="true"] em { color: var(--cap-primary-tint-2); }
.code { color: var(--cap-heading); font-weight: 600; font-size: 11px; }
.msg { max-width: 520px; white-space: normal; }
.bad { color: var(--cap-danger) !important; }
.kpis { display: grid; grid-template-columns: repeat(7, minmax(0, 1fr)); padding: 14px 20px; border-bottom: 1px solid var(--cap-border-soft); }
.kpis b { display: block; color: var(--cap-heading); font-size: 20px; font-weight: 600; }
.kpis small { color: var(--cap-muted); font-size: 11px; }
.scroll { max-height: 520px; overflow: auto; }
.more { margin: 0; padding: 12px 20px; font-size: 12px; }
.fmt { margin-bottom: 14px; }
.fmt b { color: var(--cap-heading); }
.fmt ol { display: flex; flex-wrap: wrap; gap: 6px; margin: 8px 0 0; padding: 0; list-style: none; }
.fmt code { padding: 2px 7px; border-radius: 5px; background: var(--cap-bg); color: var(--cap-ink-soft); font-size: 11px; }
@media (max-width: 900px) {
  .sheets { grid-template-columns: 1fr; }
  .kpis { grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 10px; }
}
</style>
