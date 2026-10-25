<script setup lang="ts">
// Portfolio details (APEX App 105 page 202): register a portfolio, change it, or delete it while it is a draft.
import { statusPillStyle } from '@/shared/format/statusTone'
import { computed, reactive, ref, shallowRef, watch } from 'vue'
import { showSuccess, showWarning } from '@/shared/feedback/feedback'
import { useFeedback } from '@/shared/feedback/useFeedback'
import { todayInCairo } from '@/shared/format/datetime'
import { splitApiError } from '@/shared/crud/errors'
import type { FieldOption } from '@/shared/crud/types'
import { getLookupOptions } from '@/modules/adm/api/lookups.api'
import { getDeveloperOptions, projectsApi } from '../api/masterData.api'
import { portfoliosApi, type Portfolio } from '../api/portfolios.api'

const props = defineProps<{
  /** The portfolio to edit, or null to create one. */
  portfolio: Portfolio | null
  canEdit: boolean
}>()

const emit = defineEmits<{
  close: []
  saved: [portfolio: Portfolio]
  deleted: []
}>()

type Form = {
  portfolioNo: string
  developerId: number | ''
  projectId: number | ''
  requestDate: string
  currencyCode: string
  termMonths: number | ''
  coveragePct: number | ''
  reservedAmountPct: number | ''
  pvRate: number | ''
  lateFeeRate: number | ''
  marginPct: number | ''
  adminExpensePct: number | ''
  legalFeePerUnitAmount: number | ''
  valuationFeePerUnitAmount: number | ''
  comments: string
}

const FIELDS = [
  'portfolioNo', 'developerId', 'projectId', 'requestDate', 'currencyCode', 'termMonths', 'coveragePct', 'reservedAmountPct',
  'pvRate', 'lateFeeRate', 'marginPct', 'adminExpensePct', 'legalFeePerUnitAmount', 'valuationFeePerUnitAmount', 'comments'
]

const form = reactive<Form>(blank())
const fieldErrors = ref<Record<string, string>>({})
const formError = ref('')
const saving = ref(false)
const confirmDelete = ref(false)

const developers = shallowRef<FieldOption[]>([])
const projects = shallowRef<FieldOption[]>([])
const currencies = shallowRef<FieldOption[]>([])

const isNew = computed(() => props.portfolio === null)
const hasAccounts = computed(() => (props.portfolio?.accountCount ?? 0) > 0)
const isDraft = computed(() => props.portfolio?.portfolioStatusCode === 'DRAFT')


/** Defaults of a new portfolio, as page 202 fills them. */
function blank(): Form {
  return {
    portfolioNo: '', developerId: '', projectId: '', requestDate: todayInCairo(), currencyCode: 'EGP', termMonths: '',
    coveragePct: 20, reservedAmountPct: 5, pvRate: 13, lateFeeRate: 15, marginPct: 3, adminExpensePct: 1,
    legalFeePerUnitAmount: 300, valuationFeePerUnitAmount: 500, comments: ''
  }
}

function fromPortfolio(p: Portfolio): Form {
  const n = (v: number | null) => (v === null ? '' : v)
  return {
    portfolioNo: p.portfolioNo, developerId: p.developerId, projectId: p.projectId, requestDate: p.requestDate,
    currencyCode: p.currencyCode, termMonths: n(p.termMonths), coveragePct: n(p.coveragePct), reservedAmountPct: n(p.reservedAmountPct),
    pvRate: n(p.pvRate), lateFeeRate: n(p.lateFeeRate), marginPct: n(p.marginPct), adminExpensePct: n(p.adminExpensePct),
    legalFeePerUnitAmount: n(p.legalFeePerUnitAmount), valuationFeePerUnitAmount: n(p.valuationFeePerUnitAmount),
    comments: p.comments ?? ''
  }
}

/** Options for a saved value that is no longer active, so it does not show as blank. */
function withCurrent(options: FieldOption[], value: string | number | '', label: string | undefined): FieldOption[] {
  if (value === '' || options.some(o => o.value === value)) return options
  return [...options, { value, label: `${label ?? value} (inactive)` }]
}

async function loadProjects(developerId: number | '') {
  if (developerId === '') {
    projects.value = []
    return
  }
  const rows = await projectsApi.list(`?developerId=${developerId}&activeOnly=true`)
  projects.value = withCurrent(
    rows.map(r => ({ value: r.projectId, label: `${r.projectCode} - ${r.projectNameEn || r.projectNameAr}` })),
    props.portfolio?.developerId === developerId ? props.portfolio.projectId : '',
    props.portfolio?.projectName
  )
}

watch(
  () => props.portfolio,
  async p => {
    Object.assign(form, p ? fromPortfolio(p) : blank())
    fieldErrors.value = {}
    formError.value = ''
    confirmDelete.value = false
    try {
      const [devs, curs] = await Promise.all([getDeveloperOptions(), getLookupOptions('currency')])
      developers.value = withCurrent(devs, p?.developerId ?? '', p?.developerName)
      currencies.value = withCurrent(
        curs.map(c => ({ value: c.value, label: `${c.value} - ${c.label}` })),
        p?.currencyCode ?? '',
        p?.currencyCode
      )
      await loadProjects(form.developerId)
    } catch (error) {
      formError.value = splitApiError(error, [], 'Unable to load the lists.').form
    }
  },
  { immediate: true }
)

async function onDeveloperChange() {
  form.projectId = ''
  await loadProjects(form.developerId)
}

function body() {
  const num = (v: number | '') => (v === '' ? null : Number(v))
  return {
    portfolioNo: form.portfolioNo.trim(),
    developerId: form.developerId === '' ? 0 : form.developerId,
    projectId: form.projectId === '' ? 0 : form.projectId,
    requestDate: form.requestDate || null,
    currencyCode: form.currencyCode || null,
    termMonths: num(form.termMonths),
    coveragePct: num(form.coveragePct),
    reservedAmountPct: num(form.reservedAmountPct),
    pvRate: num(form.pvRate),
    lateFeeRate: num(form.lateFeeRate),
    marginPct: num(form.marginPct),
    adminExpensePct: num(form.adminExpensePct),
    legalFeePerUnitAmount: num(form.legalFeePerUnitAmount),
    valuationFeePerUnitAmount: num(form.valuationFeePerUnitAmount),
    comments: form.comments.trim() || null
  }
}

function checkRequired(): boolean {
  const errors: Record<string, string> = {}
  if (!form.portfolioNo.trim()) errors.portfolioNo = 'Portfolio No. is required.'
  if (form.developerId === '') errors.developerId = 'Developer is required.'
  if (form.projectId === '') errors.projectId = 'Project is required.'
  if (!form.requestDate) errors.requestDate = 'Request Date is required.'
  if (!form.currencyCode) errors.currencyCode = 'Currency is required.'
  fieldErrors.value = errors
  return Object.keys(errors).length === 0
}

async function save() {
  formError.value = ''
  if (!checkRequired()) {
    showWarning('Fill in the required fields marked in red.')
    return
  }
  saving.value = true
  try {
    const saved = props.portfolio ? await portfoliosApi.update(props.portfolio, body()) : await portfoliosApi.create(body())
    showSuccess(props.portfolio ? `Portfolio ${saved.portfolioNo} saved.` : `Portfolio ${saved.portfolioNo} created. Start with the Excel import step.`)
    emit('saved', saved)
  } catch (error) {
    const split = splitApiError(error, FIELDS, 'Unable to save the portfolio.')
    fieldErrors.value = split.fields
    formError.value = split.form
  } finally {
    saving.value = false
  }
}

async function remove() {
  if (!props.portfolio) return
  if (!confirmDelete.value) {
    confirmDelete.value = true
    return
  }
  saving.value = true
  formError.value = ''
  try {
    await portfoliosApi.remove(props.portfolio)
    showSuccess('Portfolio deleted.')
    emit('deleted')
  } catch (error) {
    confirmDelete.value = false
    formError.value = splitApiError(error, [], 'Unable to delete the portfolio.').form
  } finally {
    saving.value = false
  }
}

function close() {
  if (!saving.value) emit('close')
}

useFeedback({ errors: [formError] })
</script>

<template>
  <div class="cp-modal-overlay" @click.self="close">
    <section class="cp-modal" role="dialog" aria-modal="true" aria-labelledby="pf-title">
      <header class="cp-modal__header">
        <div>
          <h3 id="pf-title">{{ isNew ? 'New purchased portfolio' : `Edit portfolio ${portfolio?.portfolioNo}` }}</h3>
          <p v-if="isNew">Register the portfolio you are buying. You will add its accounts next.</p>
          <p v-else>Update the portfolio details. Developer and project are locked once accounts exist.</p>
        </div>
        <span v-if="portfolio" class="cp-pill" :style="statusPillStyle(portfolio.portfolioStatusCode)">{{ portfolio.portfolioStatusName }}</span>
        <button type="button" class="cp-modal__close" :disabled="saving" aria-label="Close" @click="close">
          <i class="pi pi-times"></i>
        </button>
      </header>

      <div class="cp-modal__body">

        <fieldset class="cp-form" :disabled="!canEdit || saving">
          <div class="cp-field">
            <label for="pf-no">Portfolio No. *</label>
            <input id="pf-no" v-model="form.portfolioNo" maxlength="30" />
            <small v-if="fieldErrors.portfolioNo" class="cp-field__error">{{ fieldErrors.portfolioNo }}</small>
          </div>
          <div class="cp-field">
            <label for="pf-date">Request Date *</label>
            <input id="pf-date" v-model="form.requestDate" type="date" />
            <small v-if="fieldErrors.requestDate" class="cp-field__error">{{ fieldErrors.requestDate }}</small>
          </div>
          <div class="cp-field">
            <label for="pf-dev">Developer *</label>
            <select id="pf-dev" v-model="form.developerId" :disabled="hasAccounts" @change="onDeveloperChange">
              <option value="">- Select -</option>
              <option v-for="o in developers" :key="o.value" :value="o.value">{{ o.label }}</option>
            </select>
            <small v-if="fieldErrors.developerId" class="cp-field__error">{{ fieldErrors.developerId }}</small>
          </div>
          <div class="cp-field">
            <label for="pf-proj">Project *</label>
            <select id="pf-proj" v-model="form.projectId" :disabled="hasAccounts || form.developerId === ''">
              <option value="">- Select -</option>
              <option v-for="o in projects" :key="o.value" :value="o.value">{{ o.label }}</option>
            </select>
            <small v-if="fieldErrors.projectId" class="cp-field__error">{{ fieldErrors.projectId }}</small>
            <small v-else-if="hasAccounts" class="cp-field__hint">Locked: this portfolio has accounts.</small>
          </div>
          <div class="cp-field">
            <label for="pf-cur">Currency *</label>
            <select id="pf-cur" v-model="form.currencyCode">
              <option value="">- Select -</option>
              <option v-for="o in currencies" :key="o.value" :value="o.value">{{ o.label }}</option>
            </select>
            <small v-if="fieldErrors.currencyCode" class="cp-field__error">{{ fieldErrors.currencyCode }}</small>
          </div>
          <div class="cp-field">
            <label for="pf-term">Term (months)</label>
            <input id="pf-term" v-model="form.termMonths" type="number" min="0" step="1" />
            <small v-if="fieldErrors.termMonths" class="cp-field__error">{{ fieldErrors.termMonths }}</small>
          </div>

          <div class="cp-form__section">Financial figures</div>
          <div class="cp-field">
            <label for="pf-cov">Coverage Percentage</label>
            <input id="pf-cov" v-model="form.coveragePct" type="number" min="0" max="100" step="0.01" />
            <small v-if="fieldErrors.coveragePct" class="cp-field__error">{{ fieldErrors.coveragePct }}</small>
          </div>
          <div class="cp-field">
            <label for="pf-res">Reserved Amount Percentage</label>
            <input id="pf-res" v-model="form.reservedAmountPct" type="number" min="0" max="100" step="0.01" />
            <small v-if="fieldErrors.reservedAmountPct" class="cp-field__error">{{ fieldErrors.reservedAmountPct }}</small>
          </div>
          <div class="cp-field">
            <label for="pf-pv">Present Value Calculation Rate</label>
            <input id="pf-pv" v-model="form.pvRate" type="number" min="0" step="0.01" />
            <small v-if="fieldErrors.pvRate" class="cp-field__error">{{ fieldErrors.pvRate }}</small>
          </div>
          <div class="cp-field">
            <label for="pf-late">Late Fees Rate</label>
            <input id="pf-late" v-model="form.lateFeeRate" type="number" min="0" step="0.01" />
            <small v-if="fieldErrors.lateFeeRate" class="cp-field__error">{{ fieldErrors.lateFeeRate }}</small>
          </div>
          <div class="cp-field">
            <label for="pf-mar">Margin Percentage</label>
            <input id="pf-mar" v-model="form.marginPct" type="number" min="0" step="0.01" />
            <small v-if="fieldErrors.marginPct" class="cp-field__error">{{ fieldErrors.marginPct }}</small>
          </div>
          <div class="cp-field">
            <label for="pf-adm">Admin Expense %</label>
            <input id="pf-adm" v-model="form.adminExpensePct" type="number" min="0" step="0.01" />
            <small v-if="fieldErrors.adminExpensePct" class="cp-field__error">{{ fieldErrors.adminExpensePct }}</small>
          </div>
          <div class="cp-field">
            <label for="pf-leg">Legal Fees per Unit</label>
            <input id="pf-leg" v-model="form.legalFeePerUnitAmount" type="number" min="0" step="0.01" />
            <small v-if="fieldErrors.legalFeePerUnitAmount" class="cp-field__error">{{ fieldErrors.legalFeePerUnitAmount }}</small>
          </div>
          <div class="cp-field">
            <label for="pf-val">Valuation Expense per Unit</label>
            <input id="pf-val" v-model="form.valuationFeePerUnitAmount" type="number" min="0" step="0.01" />
            <small v-if="fieldErrors.valuationFeePerUnitAmount" class="cp-field__error">{{ fieldErrors.valuationFeePerUnitAmount }}</small>
          </div>

          <div class="cp-field cp-field--wide">
            <label for="pf-com">Comments</label>
            <textarea id="pf-com" v-model="form.comments" rows="3" maxlength="4000"></textarea>
            <small v-if="fieldErrors.comments" class="cp-field__error">{{ fieldErrors.comments }}</small>
          </div>
        </fieldset>
      </div>

      <footer class="cp-modal__footer">
        <button v-if="canEdit && portfolio && isDraft" type="button" class="cp-btn cp-btn--danger" :disabled="saving" @click="remove">
          <i class="pi pi-trash"></i>
          <span>{{ confirmDelete ? 'Click again to delete' : 'Delete draft' }}</span>
        </button>
        <span class="cp-spacer"></span>
        <button type="button" class="cp-btn" :disabled="saving" @click="close">{{ canEdit ? 'Cancel' : 'Close' }}</button>
        <button v-if="canEdit" type="button" class="cp-btn" :class="isNew ? 'cp-btn--create' : 'cp-btn--primary'" :disabled="saving" @click="save">
          <i :class="saving ? 'pi pi-spin pi-spinner' : 'pi pi-check'"></i>
          <span>{{ saving ? 'Saving...' : isNew ? 'Create portfolio' : 'Save changes' }}</span>
        </button>
      </footer>
    </section>
  </div>
</template>
