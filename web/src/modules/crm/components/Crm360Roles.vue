<template>
  <!-- Customer 360 > Roles and Programs: what this customer is to us (Client, Supplier, Debtor, ...) and the programs it is in.
       A role starts as Onboarding and becomes Active only when its checks pass; the checks still open are listed under the role. -->
  <div class="c3-tab">
    <section class="c3-panel">
      <header class="c3-panel-head">
        <h3><span class="fa fa-tags" aria-hidden="true"></span>{{ tr('Roles') }}</h3>
        <div class="c3-actions">
          <button v-if="canEdit && free.length" type="button" class="fx-btn" @click="openAdd"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Add Role') }}</button>
        </div>
      </header>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Role') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Active Since') }}</th><th>{{ tr('Checks Still Open') }}</th><th>{{ tr('Notes') }}</th><th v-if="canEdit"></th></tr></thead>
          <tbody>
            <tr v-for="r in roles" :key="r.clientRoleId">
              <td><b>{{ tr(r.roleLabel) }}</b></td>
              <td><span class="fx-chip" :class="chip(r.statusCode)">{{ tr(label(r.statusCode)) }}</span></td>
              <td>{{ formatDate(r.activeSinceDate) || '—' }}</td>
              <td>
                <ul v-if="r.activationBlockers.length" class="c3-blockers"><li v-for="b in r.activationBlockers" :key="b">{{ tr(b) }}</li></ul>
                <span v-else-if="r.statusCode !== 'ACTIVE'" class="c3-muted">{{ tr('All checks passed') }}</span>
                <span v-else class="c3-muted">—</span>
              </td>
              <td>{{ r.notes || '' }}</td>
              <td v-if="canEdit">
                <div class="c3-row-actions">
                  <button v-if="r.statusCode !== 'ACTIVE'" type="button" class="fx-btn fx-btn--sm fx-btn--teal" :disabled="busy || r.activationBlockers.length > 0"
                          :title="r.activationBlockers.length ? tr('Close the open checks first.') : ''" @click="activate(r)">
                    <span class="fa fa-play" aria-hidden="true"></span>{{ tr('Activate') }}
                  </button>
                  <button v-if="r.statusCode !== 'SUSPENDED'" type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="openSuspend(r)">
                    <span class="fa fa-pause" aria-hidden="true"></span>{{ tr('Suspend') }}
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="!roles.length" class="fx-empty"><span class="fa fa-tags" aria-hidden="true"></span>{{ tr('No roles yet. Add the role this customer plays.') }}</div>
      </div>
    </section>

    <section class="c3-panel">
      <header class="c3-panel-head"><h3><span class="fa fa-briefcase" aria-hidden="true"></span>{{ tr('Programs') }}</h3></header>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Agreement') }}</th><th>{{ tr('Role in Program') }}</th><th>{{ tr('Factoring Type') }}</th><th>{{ tr('Status') }}</th><th>{{ tr('Currency') }}</th><th class="num">{{ tr('Approved Limit') }}</th><th class="num">{{ tr('Utilized') }}</th><th class="num">{{ tr('Available') }}</th></tr></thead>
          <tbody>
            <tr v-for="p in programs" :key="p.agreementId">
              <td><b>{{ p.agreementNumber }}</b></td><td>{{ tr(label(p.programRole)) }}</td><td>{{ p.factoringType ? tr(label(p.factoringType)) : '—' }}</td>
              <td><span class="fx-chip" :class="chip(p.statusCode)">{{ p.statusCode ? tr(label(p.statusCode)) : '—' }}</span></td>
              <td>{{ p.currencyCode || '—' }}</td>
              <td class="num">{{ formatAmount(p.approvedLimit) }}</td><td class="num">{{ formatAmount(p.utilizedAmount) }}</td><td class="num">{{ formatAmount(p.availableAmount) }}</td>
            </tr>
          </tbody>
        </table>
        <div v-if="!programs.length" class="fx-empty"><span class="fa fa-briefcase" aria-hidden="true"></span>{{ tr('This customer is not in any program yet.') }}</div>
      </div>
    </section>

    <Crm360Dialog v-model:visible="adding" :title="tr('Add Role')" icon="fa-tags" :busy="busy" :width="520">
      <p class="c3-note"><span class="fa fa-info-circle" aria-hidden="true"></span>{{ tr('A new role starts as Onboarding. Activate it when its checks pass.') }}</p>
      <label class="fc-field"><span>{{ tr('Role') }}<b> *</b></span>
        <select v-model="newRole" class="fx-select"><option v-for="o in free" :key="o.value" :value="o.value">{{ tr(o.label) }}</option></select></label>
      <label class="fc-field"><span>{{ tr('Notes') }}</span><input v-model="notes" class="fx-search" maxlength="1000"></label>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="adding = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !newRole" @click="add"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-check'" aria-hidden="true"></span>{{ tr('Add') }}</button>
      </template>
    </Crm360Dialog>

    <Crm360Dialog v-model:visible="suspending" :title="tr('Suspend Role')" :subtitle="current ? tr(current.roleLabel) : ''" icon="fa-pause" :busy="busy" :width="520">
      <label class="fc-field"><span>{{ tr('Reason') }}<b> *</b></span><input v-model="notes" class="fx-search" maxlength="1000"></label>
      <template #footer>
        <button type="button" class="fx-btn" :disabled="busy" @click="suspending = false"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn" :disabled="busy || !notes.trim()" @click="suspend"><span class="fa" :class="busy ? 'fa-spinner fa-spin' : 'fa-pause'" aria-hidden="true"></span>{{ tr('Suspend') }}</button>
      </template>
    </Crm360Dialog>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { ApiError } from '@/shared/api/http'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDate } from '@/shared/format/datetime'
import { formatAmount } from '@/shared/format/money'
import { tr } from '@/shared/layout/language'
import { crm360Api, type CrmProgram, type CrmRole } from '../api/crm.api'
import Crm360Dialog from './Crm360Dialog.vue'
import { chip, label, ROLES } from './crm360'

const props = defineProps<{ clientId: number; roles: CrmRole[]; programs: CrmProgram[]; canEdit: boolean }>()
const emit = defineEmits<{ changed: [] }>()

const free = computed(() => ROLES.filter((o) => !props.roles.some((r) => r.roleCode === o.value)))

const adding = ref(false)
const suspending = ref(false)
const busy = ref(false)
const newRole = ref('')
const notes = ref('')
const current = ref<CrmRole | null>(null)

function openAdd() {
  newRole.value = free.value[0]?.value ?? ''
  notes.value = ''
  adding.value = true
}

function openSuspend(r: CrmRole) {
  current.value = r
  notes.value = ''
  suspending.value = true
}

async function run(action: () => Promise<void>, done: string, close: () => void = () => {}) {
  busy.value = true
  try {
    await action()
    close()
    showSuccess(tr(done))
    emit('changed')
  } catch (err) {
    if (err instanceof ApiError && err.problem?.errors) showError(Object.values(err.problem.errors).flat().map((m) => tr(m)).join('\n'))
    else showError(err instanceof Error ? err.message : String(err))
  } finally {
    busy.value = false
  }
}

const add = () => run(() => crm360Api.addRole(props.clientId, newRole.value, notes.value || null), 'Role added.', () => (adding.value = false))
const activate = (r: CrmRole) => run(() => crm360Api.activateRole(props.clientId, r.clientRoleId), 'Role activated.')
const suspend = () => run(() => crm360Api.suspendRole(props.clientId, current.value!.clientRoleId, notes.value.trim()), 'Role suspended.',
  () => (suspending.value = false))
</script>

<style scoped>
@import './crm360.css';
.c3-blockers { margin: 0; padding-inline-start: 18px; color: var(--fin-warn); font-size: var(--fs-xs, 12px); }
.num { text-align: end; white-space: nowrap; }
</style>
