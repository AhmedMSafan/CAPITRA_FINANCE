<template>
  <!-- Customer 360 > Bank Accounts: the accounts the customer can be paid into. Any change sets an account back to Pending; another
       user verifies it (how it was confirmed is recorded); it can be paid once the waiting period after its last change has passed. -->
  <div class="c3-tab">
    <section class="c3-panel">
      <header class="c3-panel-head">
        <h3><span class="fa fa-university" aria-hidden="true"></span>{{ tr('Bank Accounts') }}</h3>
        <span class="fx-chip" :class="chip(badge.code)">{{ tr(badge.label) }}</span>
        <div class="c3-actions">
          <button v-if="canEdit" type="button" class="fx-btn fx-btn--teal" @click="openEdit(null)"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Add Bank Account') }}</button>
        </div>
      </header>
      <p class="c3-note"><span class="fa fa-lock" aria-hidden="true"></span>{{ tr(`A new or changed account needs another user's verification, then waits ${waitingDays} day(s) before it can be paid.`) }}</p>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead>
            <tr>
              <th>{{ tr('Bank') }}</th><th>{{ tr('Account No. / IBAN') }}</th><th>{{ tr('Holder') }}</th><th>{{ tr('Currency') }}</th>
              <th>{{ tr('Primary') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Verified') }}</th><th>{{ tr('Payable From') }}</th><th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="a in accounts" :key="a.bankAccountId">
              <td><b>{{ a.bankName }}</b><div class="c3-muted">{{ [a.branchName, a.swiftCode].filter(Boolean).join(' · ') }}</div></td>
              <td>{{ a.accountNumber || '—' }}<div class="c3-muted">{{ a.iban || '' }}</div></td>
              <td>{{ a.accountHolderName }}<div v-if="a.accountHolderNameAr" class="c3-muted" dir="rtl">{{ a.accountHolderNameAr }}</div></td>
              <td><span class="fx-chip fx-chip--brand fx-chip--plain">{{ a.currencyCode }}</span></td>
              <td><span v-if="a.isPrimary" class="fa fa-star c3-star" :title="tr('Primary')" aria-label="Primary"></span></td>
              <td><span class="fx-chip" :class="chip(a.statusCode)">{{ tr(label(a.statusCode)) }}</span></td>
              <td>
                <template v-if="a.verifiedBy">{{ a.verifiedBy }}<div class="c3-muted">{{ formatDateTime(a.verifiedAt) }} · {{ tr(label(a.verificationMethodCode)) }}</div></template>
                <span v-else class="c3-muted">{{ tr('Changed by') }} {{ a.changedBy }}</span>
              </td>
              <td>
                <span v-if="a.statusCode !== 'VERIFIED'" class="c3-muted">—</span>
                <span v-else class="fx-chip" :class="a.isPayable ? 'fx-chip--ok' : 'fx-chip--warn'">{{ a.isPayable ? tr('Payable') : formatDateTime(a.payableFromAt) }}</span>
              </td>
              <td>
                <div class="c3-row-actions">
                  <button v-if="canEdit && a.statusCode !== 'SUSPENDED'" type="button" class="fx-btn fx-btn--sm" :title="tr('Edit')" :aria-label="tr('Edit')" @click="openEdit(a)"><span class="fa fa-pencil" aria-hidden="true"></span></button>
                  <button v-if="canVerify && a.statusCode === 'PENDING'" type="button" class="fx-btn fx-btn--teal fx-btn--sm" :disabled="mine(a)"
                          :title="tr(mine(a) ? 'You made the last change; another user must verify it.' : 'Verify')" @click="openVerify(a)">
                    <span class="fa fa-check-circle" aria-hidden="true"></span>{{ tr('Verify') }}
                  </button>
                  <button v-if="canEdit && a.statusCode !== 'SUSPENDED'" type="button" class="fx-btn fx-btn--sm" :title="tr('Suspend')" :aria-label="tr('Suspend')" @click="openSuspend(a)"><span class="fa fa-ban" aria-hidden="true"></span></button>
                  <button v-if="canEdit && a.statusCode === 'SUSPENDED'" type="button" class="fx-btn fx-btn--sm" @click="reactivate(a)"><span class="fa fa-undo" aria-hidden="true"></span>{{ tr('Reactivate') }}</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="!accounts.length" class="fx-empty"><span class="fa fa-university" aria-hidden="true"></span>{{ tr('No bank account yet.') }}</div>
      </div>
    </section>

    <section class="c3-panel">
      <header class="c3-panel-head"><h3><span class="fa fa-history" aria-hidden="true"></span>{{ tr('History') }}</h3></header>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('When') }}</th><th>{{ tr('By') }}</th><th>{{ tr('Action') }}</th><th>{{ tr('Bank') }}</th><th>{{ tr('Account No. / IBAN') }}</th><th>{{ tr('Currency') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Notes') }}</th></tr></thead>
          <tbody>
            <tr v-for="h in history" :key="h.bankAccountHistoryId">
              <td>{{ formatDateTime(h.at) }}</td><td>{{ h.by }}</td><td>{{ tr(label(h.actionCode)) }}</td><td>{{ h.bankName }}</td>
              <td>{{ h.accountNumber || h.iban }}</td><td>{{ h.currencyCode }}</td>
              <td><span class="fx-chip" :class="chip(h.statusCode)">{{ tr(label(h.statusCode)) }}</span></td><td>{{ h.notes || '' }}</td>
            </tr>
          </tbody>
        </table>
        <div v-if="!history.length" class="fx-empty"><span class="fa fa-history" aria-hidden="true"></span>{{ tr('No changes recorded yet.') }}</div>
      </div>
    </section>

    <Crm360Dialog v-model:visible="editing" :title="tr(editId ? 'Edit Bank Account' : 'Add Bank Account')" icon="fa-university" :busy="busy">
      <p v-if="editId" class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Changing the bank, number, IBAN, holder, currency or type sets the account back to Pending.') }}</p>
      <div class="c3-grid">
        <label class="fc-field" :class="bad('bankName')"><span>{{ tr('Bank Name') }}<b> *</b></span><input v-model="form.bankName" class="fx-search" maxlength="200"></label>
        <label class="fc-field"><span>{{ tr('Branch') }}</span><input v-model="form.branchName" class="fx-search" maxlength="200"></label>
        <label class="fc-field" :class="bad('swiftCode')"><span>{{ tr('SWIFT / BIC') }}</span><input v-model="form.swiftCode" class="fx-search" maxlength="11"></label>
        <label class="fc-field" :class="bad('accountNumber')"><span>{{ tr('Account Number') }}</span><input v-model="form.accountNumber" class="fx-search" maxlength="50"></label>
        <label class="fc-field c3-span2" :class="bad('iban')"><span>{{ tr('IBAN') }}</span><input v-model="form.iban" class="fx-search" maxlength="42"></label>
        <label class="fc-field" :class="bad('accountHolderName')"><span>{{ tr('Account Holder Name') }}<b> *</b></span><input v-model="form.accountHolderName" class="fx-search" maxlength="300"></label>
        <label class="fc-field"><span>{{ tr('Arabic Holder Name') }}</span><input v-model="form.accountHolderNameAr" class="fx-search" dir="rtl" maxlength="300"></label>
        <label class="fc-field" :class="bad('currencyId')"><span>{{ tr('Currency') }}<b> *</b></span>
          <select v-model.number="form.currencyId" class="fx-select"><option v-for="c in currencies" :key="c.value" :value="Number(c.value)">{{ c.label }}</option></select></label>
        <label class="fc-field" :class="bad('accountTypeCode')"><span>{{ tr('Account Type') }}<b> *</b></span>
          <select v-model="form.accountTypeCode" class="fx-select"><option v-for="t in TYPES" :key="t" :value="t">{{ tr(label(t)) }}</option></select></label>
        <label class="fc-field"><span>{{ tr('Primary Account') }}</span>
          <select v-model="form.isPrimary" class="fx-select"><option :value="true">{{ tr('Yes') }}</option><option :value="false">{{ tr('No') }}</option></select></label>
        <label class="fc-field c3-wide"><span>{{ tr('Notes') }}</span><input v-model="form.notes" class="fx-search" maxlength="1000"></label>
      </div>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="editing = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr('Save') }}</button>
      </template>
    </Crm360Dialog>

    <Crm360Dialog v-model:visible="verifying" :title="tr('Verify Bank Account')" :subtitle="current ? `${current.bankName} · ${current.accountNumber || current.iban}` : ''"
                  icon="fa-check-circle" :busy="busy" :width="560">
      <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('Confirm the account with the customer through a channel you already trust, never the contact details that came with the change.') }}</p>
      <div class="c3-grid c3-grid--2">
        <label class="fc-field" :class="bad('verificationMethodCode')"><span>{{ tr('Confirmed by') }}<b> *</b></span>
          <select v-model="method" class="fx-select"><option value="" disabled></option><option v-for="m in METHODS" :key="m" :value="m">{{ tr(label(m)) }}</option></select></label>
        <label class="fc-field"><span>{{ tr('Notes') }}</span><input v-model="notes" class="fx-search" maxlength="1000"></label>
      </div>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="verifying = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="verify"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check-circle'" aria-hidden="true"></span>{{ tr('Verify') }}</button>
      </template>
    </Crm360Dialog>

    <Crm360Dialog v-model:visible="suspending" :title="tr('Suspend Bank Account')" icon="fa-ban" :busy="busy" :width="520">
      <label class="fc-field" :class="bad('notes')"><span>{{ tr('Reason') }}<b> *</b></span><input v-model="notes" class="fx-search" maxlength="1000"></label>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="suspending = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="suspend"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-ban'" aria-hidden="true"></span>{{ tr('Suspend') }}</button>
      </template>
    </Crm360Dialog>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { ApiError } from '@/shared/api/http'
import { getUser } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { crm360Api, type CrmBadge, type CrmBankAccount, type CrmBankAccountForm, type CrmBankAccountHistory, type CrmOption } from '../api/crm.api'
import Crm360Dialog from './Crm360Dialog.vue'
import { chip, label } from './crm360'

const props = defineProps<{
  clientId: number; accounts: CrmBankAccount[]; history: CrmBankAccountHistory[]; badge: CrmBadge; currencies: CrmOption[]
  defaultCurrencyId: number; holderName: string; waitingDays: number; canEdit: boolean; canVerify: boolean
}>()
const emit = defineEmits<{ changed: [] }>()

const TYPES = ['CURRENT', 'SAVINGS', 'OTHER']
const METHODS = ['CALL_BACK', 'BANK_LETTER', 'BANK_STATEMENT']

const editing = ref(false)
const verifying = ref(false)
const suspending = ref(false)
const busy = ref(false)
const editId = ref<number | null>(null)
const current = ref<CrmBankAccount | null>(null)
const method = ref('')
const notes = ref('')
const errors = ref<Record<string, string[]>>({})
const bad = (f: string) => ({ 'c3-bad': !!errors.value[f] })
const me = getUser()?.userName ?? ''
const mine = (a: CrmBankAccount) => !!me && a.changedBy.toLowerCase() === me.toLowerCase()

const empty = (): CrmBankAccountForm => ({
  bankName: '', branchName: null, swiftCode: null, accountNumber: null, iban: null, accountHolderName: props.holderName, accountHolderNameAr: null,
  currencyId: props.defaultCurrencyId, accountTypeCode: 'CURRENT', isPrimary: !props.accounts.some((x) => x.isPrimary), notes: null
})
const form = reactive<CrmBankAccountForm>(empty())

function openEdit(a: CrmBankAccount | null) {
  errors.value = {}
  editId.value = a?.bankAccountId ?? null
  Object.assign(form, a ? {
    bankName: a.bankName, branchName: a.branchName, swiftCode: a.swiftCode, accountNumber: a.accountNumber, iban: a.iban,
    accountHolderName: a.accountHolderName, accountHolderNameAr: a.accountHolderNameAr, currencyId: a.currencyId, accountTypeCode: a.accountTypeCode,
    isPrimary: a.isPrimary, notes: a.notes
  } : empty())
  editing.value = true
}
function openVerify(a: CrmBankAccount) {
  current.value = a
  method.value = ''
  notes.value = ''
  errors.value = {}
  verifying.value = true
}
function openSuspend(a: CrmBankAccount) {
  current.value = a
  notes.value = ''
  errors.value = {}
  suspending.value = true
}

async function run(action: () => Promise<unknown>, done: string, close: () => void) {
  busy.value = true
  try {
    await action()
    close()
    showSuccess(tr(done))
    emit('changed')
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) {
      errors.value = err.problem.errors
      showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    } else showError(err instanceof Error ? err.message : String(err))
  } finally {
    busy.value = false
  }
}

const save = () => run(() => (editId.value ? crm360Api.updateBankAccount(props.clientId, editId.value, { ...form }) : crm360Api.addBankAccount(props.clientId, { ...form })),
  editId.value ? 'Bank account saved.' : 'Bank account added. Another user must verify it.', () => (editing.value = false))
const verify = () => run(() => crm360Api.verifyBankAccount(props.clientId, current.value!.bankAccountId, method.value, notes.value || null),
  'Bank account verified.', () => (verifying.value = false))
const suspend = () => run(() => crm360Api.suspendBankAccount(props.clientId, current.value!.bankAccountId, notes.value), 'Bank account suspended.', () => (suspending.value = false))
const reactivate = (a: CrmBankAccount) => run(() => crm360Api.reactivateBankAccount(props.clientId, a.bankAccountId), 'Bank account reactivated. It must be verified again.', () => {})
</script>

<style scoped>
@import './crm360.css';
.c3-star { color: var(--fin-orange); }
.c3-span2 { grid-column: span 2; }
.c3-grid--2 { grid-template-columns: repeat(2, minmax(0, 1fr)); }
</style>
