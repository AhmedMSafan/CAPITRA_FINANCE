<template>
  <!-- FINOVA_FINANCEING ClientFormDialog "Add Client" / "Edit Client": Basic Information, Contact Information and
       Classification, with the same fields, lists and required marks. Errors open a pop-up and mark their fields. -->
  <Teleport to="body">
    <div v-if="visible" class="fx fc fc-modal-back" role="presentation" @click.self="close">
      <section class="fc-modal crm-dlg" role="dialog" aria-modal="true" aria-labelledby="crm-dlg-title">
        <header class="fc-docs-head">
          <span class="fc-sec-ic" aria-hidden="true"><span class="fa" :class="clientId ? 'fa-pencil' : 'fa-user-plus'"></span></span>
          <div>
            <h2 id="crm-dlg-title">{{ tr(clientId ? 'Edit Client' : 'Add Client') }}</h2>
            <small v-if="clientId">{{ form.clientNumber }} · {{ form.clientName }}</small>
          </div>
          <button type="button" class="fx-btn fx-btn--sm" :disabled="saving" :aria-label="tr('Close')" @click="close"><span class="fa fa-times" aria-hidden="true"></span></button>
        </header>

        <div v-if="loading" class="fx-empty"><span class="fa fa-spinner fa-spin" aria-hidden="true"></span>{{ tr('Loading client...') }}</div>
        <template v-else>
          <div class="crm-sec">
            <h3><span class="fa fa-id-card-o" aria-hidden="true"></span>{{ tr('Basic Information') }}</h3>
            <div class="crm-grid">
              <label class="fc-field" :class="bad('clientNumber')">
                <span>{{ tr('Client Number') }}<b v-if="!clientId"> *</b></span>
                <input v-model="form.clientNumber" class="fx-search" type="text" maxlength="20" :disabled="!!clientId" :placeholder="tr('Enter client number')">
              </label>
              <label class="fc-field" :class="bad('clientName')">
                <span>{{ tr('Client Name') }}<b> *</b></span>
                <input v-model="form.clientName" class="fx-search" type="text" maxlength="500" :placeholder="tr('Enter client name')">
              </label>
              <label class="fc-field" :class="bad('clientNameAr')">
                <span>{{ tr('Arabic Client Name') }}<b> *</b></span>
                <input v-model="form.clientNameAr" class="fx-search" type="text" maxlength="500" dir="rtl" :placeholder="tr('Enter Arabic client name')">
              </label>
              <label class="fc-field">
                <span>{{ tr('Short Name') }}</span>
                <input v-model="form.shortName" class="fx-search" type="text" maxlength="100" :placeholder="tr('Enter short name')">
              </label>
              <label class="fc-field" :class="bad('clientType')">
                <span>{{ tr('Client Type') }}<b> *</b></span>
                <select v-model="form.clientType" class="fx-select">
                  <option value="" disabled></option>
                  <option v-for="t in withCurrent(CLIENT_TYPES, form.clientType)" :key="t" :value="t">{{ t }}</option>
                </select>
              </label>
              <label class="fc-field" :class="bad('clientRoleCode')">
                <span>{{ tr('Client Role') }}</span>
                <select v-model="form.clientRoleCode" class="fx-select">
                  <option :value="null">{{ tr('Select client role') }}</option>
                  <option v-for="r in options.clientRoles" :key="r.value" :value="r.value">{{ r.label }}</option>
                </select>
              </label>
              <label class="fc-field" :class="bad('legalStructure')">
                <span>{{ tr('Legal Structure') }}<b> *</b></span>
                <select v-model="form.legalStructure" class="fx-select">
                  <option value="" disabled></option>
                  <option v-for="t in withCurrent(LEGAL_STRUCTURES, form.legalStructure)" :key="t" :value="t">{{ t }}</option>
                </select>
              </label>
              <label class="fc-field">
                <span>{{ tr('Registration Number') }}</span>
                <input v-model="form.registrationNumber" class="fx-search" type="text" maxlength="50" :placeholder="tr('Enter registration number')">
              </label>
              <label class="fc-field">
                <span>{{ tr('Tax Number') }}</span>
                <input v-model="form.taxNumber" class="fx-search" type="text" maxlength="50" :placeholder="tr('Enter tax number')">
              </label>
            </div>
          </div>

          <div class="crm-sec">
            <h3><span class="fa fa-phone" aria-hidden="true"></span>{{ tr('Contact Information') }}</h3>
            <div class="crm-grid">
              <label class="fc-field">
                <span>{{ tr('Email') }}</span>
                <input v-model="form.email" class="fx-search" type="email" maxlength="200" placeholder="client@example.com">
              </label>
              <label class="fc-field">
                <span>{{ tr('Phone Number') }}</span>
                <input v-model="form.phoneNumber" class="fx-search" type="text" maxlength="30" :placeholder="tr('Enter phone number')">
              </label>
              <label class="fc-field">
                <span>{{ tr('Mobile Number') }}</span>
                <input v-model="form.mobileNumber" class="fx-search" type="text" maxlength="30" :placeholder="tr('Enter mobile number')">
              </label>
              <label class="fc-field">
                <span>{{ tr('Website') }}</span>
                <input v-model="form.website" class="fx-search" type="text" maxlength="500" placeholder="https://...">
              </label>
            </div>
          </div>

          <div class="crm-sec">
            <h3><span class="fa fa-tags" aria-hidden="true"></span>{{ tr('Classification') }}</h3>
            <div class="crm-grid">
              <label class="fc-field" :class="bad('countryId')">
                <span>{{ tr('Country ID') }}</span>
                <select v-model.number="form.countryId" class="fx-select">
                  <option :value="0" disabled>{{ tr('Select country') }}</option>
                  <option v-for="c in options.countries" :key="c.value" :value="Number(c.value)">{{ c.label }}</option>
                </select>
              </label>
              <label class="fc-field" :class="bad('baseCurrencyId')">
                <span>{{ tr('Base Currency ID') }}</span>
                <select v-model.number="form.baseCurrencyId" class="fx-select">
                  <option :value="0" disabled>{{ tr('Select currency') }}</option>
                  <option v-for="c in options.currencies" :key="c.value" :value="Number(c.value)">{{ c.label }}</option>
                </select>
              </label>
              <label class="fc-field" :class="bad('statusId')">
                <span>{{ tr('Status ID') }}</span>
                <input v-model.number="form.statusId" class="fx-search" type="number" min="0">
              </label>
              <label class="fc-field">
                <span>{{ tr('Status') }}</span>
                <select v-model="form.isActive" class="fx-select">
                  <option :value="true">{{ tr('Active') }}</option>
                  <option :value="false">{{ tr('Inactive') }}</option>
                </select>
              </label>
              <label class="fc-field">
                <span>{{ tr('Supplier') }}</span>
                <select v-model="form.isSupplier" class="fx-select">
                  <option :value="true">{{ tr('Yes') }}</option>
                  <option :value="false">{{ tr('No') }}</option>
                </select>
              </label>
              <label class="fc-field">
                <span>{{ tr('Related Party') }}</span>
                <select v-model="form.isRelatedParty" class="fx-select">
                  <option :value="true">{{ tr('Yes') }}</option>
                  <option :value="false">{{ tr('No') }}</option>
                </select>
              </label>
            </div>
          </div>
        </template>

        <footer class="crm-foot">
          <button type="button" class="fx-btn" :disabled="saving" @click="close"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
          <button type="button" class="fx-btn fx-btn--teal" :disabled="saving || loading" @click="save">
            <span class="fa" :class="saving ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr(clientId ? 'Update' : 'Save') }}
          </button>
        </footer>
      </section>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { reactive, ref, watch } from 'vue'
import { ApiError } from '@/shared/api/http'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { crmApi, type CrmClientForm, type CrmClientOptions } from '../api/crm.api'

// clientId null = Add Client
const props = defineProps<{ clientId: number | null }>()
const visible = defineModel<boolean>('visible', { required: true })
const emit = defineEmits<{ saved: [client: CrmClientForm] }>()

// FINOVA's fixed lists
const CLIENT_TYPES = ['CORPORATE', 'SME', 'INDIVIDUAL', 'GOVERNMENT']
const LEGAL_STRUCTURES = ['LLC', 'JSC', 'PARTNERSHIP', 'SOLE']

const empty = (): CrmClientForm => ({
  clientId: null, clientNumber: '', clientName: '', clientNameAr: '', shortName: null, clientType: '', clientRoleCode: null,
  legalStructure: '', registrationNumber: null, taxNumber: null, email: null, phoneNumber: null, mobileNumber: null, website: null,
  countryId: 0, baseCurrencyId: 0, statusId: 0, isActive: true, isSupplier: false, isRelatedParty: false
})

const form = reactive<CrmClientForm>(empty())
const options = ref<CrmClientOptions>({ countries: [], currencies: [], clientRoles: [] })
const errors = ref<Record<string, string[]>>({})
const loading = ref(false)
const saving = ref(false)

const bad = (field: string) => ({ 'crm-bad': !!errors.value[field] })
// a value saved before (e.g. PROSPECT, HOLDING) stays selectable even when it is not in FINOVA's list
const withCurrent = (list: string[], value: string) => (value && !list.includes(value) ? [...list, value] : list)

// FINOVA's checks before calling the API
function check(): Record<string, string[]> {
  const e: Record<string, string[]> = {}
  if (!props.clientId && !form.clientNumber.trim()) e.clientNumber = ['Client Number is required.']
  if (!form.clientName.trim()) e.clientName = ['Client Name is required.']
  if (!form.clientNameAr.trim()) e.clientNameAr = ['Arabic Client Name is required.']
  if (!form.clientType.trim()) e.clientType = ['Client Type is required.']
  if (!form.legalStructure.trim()) e.legalStructure = ['Legal Structure is required.']
  return e
}

function report(e: Record<string, string[]>) {
  errors.value = e
  showError(Object.values(e).flat().map((m) => tr(m)).join('\n'))
}

async function save() {
  const e = check()
  if (Object.keys(e).length) return report(e)
  errors.value = {}
  saving.value = true
  try {
    const saved = props.clientId ? await crmApi.update(props.clientId, { ...form }) : await crmApi.create({ ...form })
    visible.value = false
    showSuccess(tr(props.clientId ? 'Client updated.' : 'Client created.'))
    emit('saved', saved)
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) report(err.problem.errors)
    else showError(err instanceof Error ? err.message : String(err))
  } finally {
    saving.value = false
  }
}

function close() {
  if (!saving.value) visible.value = false
}

watch(visible, async (open) => {
  if (!open) return
  Object.assign(form, empty())
  errors.value = {}
  loading.value = true
  try {
    const [opts, client] = await Promise.all([crmApi.options(), props.clientId ? crmApi.client(props.clientId) : Promise.resolve(null)])
    options.value = opts
    if (client) Object.assign(form, client)
  } catch (err) {
    showError(err instanceof Error ? err.message : String(err))
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.crm-dlg { width: min(900px, calc(100vw - 32px)); max-height: calc(100vh - 48px); overflow: auto; display: flex; flex-direction: column; gap: 16px; }
.crm-sec { border: 1px solid var(--fin-line); border-radius: var(--fin-radius); padding: 14px 16px 16px; }
.crm-sec h3 { display: flex; align-items: center; gap: 8px; margin: 0 0 12px; font-size: var(--fs-md, 14px); color: var(--fin-teal-700); }
.crm-sec h3 .fa { color: var(--fin-orange); }
.crm-grid { display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 12px 14px; }
.crm-grid .fc-field { flex: none; }
.crm-bad .fx-search, .crm-bad .fx-select { border-color: var(--fin-bad); }
.crm-foot { display: flex; justify-content: flex-end; gap: 8px; margin: 0; }
@media (max-width: 760px) { .crm-grid { grid-template-columns: 1fr; } }
</style>
