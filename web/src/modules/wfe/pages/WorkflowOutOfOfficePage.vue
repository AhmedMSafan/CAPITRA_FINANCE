<template>
  <!-- Out of office (K2-style): the dates a user is away and who works their tasks meanwhile, for all workflows or one. -->
  <div class="fx" id="fxOoo">
    <div class="fx-head">
      <div>
        <h1>{{ tr('Out of office') }}</h1>
        <p>{{ tr('While you are away, new tasks for you go to the person you choose, and they also see the tasks already waiting for you.') }}</p>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn" @click="openWorkflowManual(9)"><span class="fa fa-question-circle" aria-hidden="true"></span>{{ tr('Help') }}</button>
        <RouterLink class="fx-btn" :to="{ name: 'workflow-inbox' }"><span class="fa fa-inbox" aria-hidden="true"></span>{{ tr('My Inbox') }}</RouterLink>
      </div>
    </div>

    <!-- where the user stands today -->
    <section class="oo-status" :class="awayNow ? 'is-away' : 'is-in'" role="status">
      <span class="fa" :class="awayNow ? 'fa-plane' : 'fa-check-circle'" aria-hidden="true"></span>
      <span v-if="awayNow">
        {{ tr('You are out of office until') }} <b>{{ day(awayNow.toDate) }}</b>.
        {{ tr('Your tasks') }} ({{ awayNow.workflowName || tr('all workflows') }}) {{ tr('go to') }} <b>{{ awayNow.delegateFullName || awayNow.delegateTo }}</b>.
      </span>
      <span v-else>{{ tr('You are in the office. Your tasks come to you.') }}</span>
    </section>

    <section v-if="message" class="oo-msg" :class="{ 'is-bad': message.bad }" role="status">
      <span class="fa" :class="message.bad ? 'fa-exclamation-circle' : 'fa-check-circle'" aria-hidden="true"></span>
      <span>{{ message.text }}</span>
      <button type="button" class="oo-x" :aria-label="tr('Close')" @click="message = null"><span class="fa fa-times" aria-hidden="true"></span></button>
    </section>

    <div class="oo-grid">
      <!-- the form -->
      <div class="fx-card fx-card-pad">
        <h2>{{ form.id ? tr('Change out of office') : tr('Set out of office') }}</h2>
        <p class="oo-sub">{{ tr('One entry for all workflows, and others for single workflows if a different person should cover them.') }}</p>

        <div class="oo-form">
          <label v-if="admin" class="oo-field">
            <span>{{ tr('Who is away') }}</span>
            <select v-model="form.userName" class="fx-select">
              <option v-for="u in users" :key="u.userName" :value="u.userName">{{ label(u) }}</option>
            </select>
          </label>

          <label class="oo-field">
            <span>{{ tr('Who works the tasks') }}</span>
            <select v-model="form.delegateTo" class="fx-select">
              <option value="">{{ tr('Choose a person') }}</option>
              <option v-for="u in users.filter(x => x.userName !== form.userName)" :key="u.userName" :value="u.userName">{{ label(u) }}</option>
            </select>
          </label>

          <label class="oo-field">
            <span>{{ tr('Workflows') }}</span>
            <select v-model="form.workflowCode" class="fx-select">
              <option value="">{{ tr('All workflows') }}</option>
              <option v-for="w in workflows" :key="w.workflowCode" :value="w.workflowCode">{{ w.workflowName }}</option>
            </select>
          </label>

          <div class="oo-dates">
            <label class="oo-field">
              <span>{{ tr('First day away') }}</span>
              <input v-model="form.fromDate" type="date" class="fx-search">
            </label>
            <label class="oo-field">
              <span>{{ tr('Last day away') }}</span>
              <input v-model="form.toDate" type="date" class="fx-search" :min="form.fromDate">
            </label>
          </div>

          <label class="oo-field">
            <span>{{ tr('Note (optional)') }}</span>
            <input v-model="form.note" type="text" class="fx-search" maxlength="500" :placeholder="tr('e.g. Annual leave')">
          </label>

          <label class="oo-check">
            <input v-model="form.isActive" type="checkbox" class="fx-check">
            <span>{{ tr('On') }}</span>
            <small>{{ tr('Switch off to keep the entry without using it.') }}</small>
          </label>

          <p v-if="formProblem" class="oo-problem"><span class="fa fa-exclamation-circle" aria-hidden="true"></span> {{ formProblem }}</p>

          <div class="oo-actions">
            <button v-if="form.id" type="button" class="fx-btn" @click="reset">{{ tr('Cancel') }}</button>
            <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !!formProblem" @click="save">
              <span class="fa fa-check" aria-hidden="true"></span>{{ form.id ? tr('Save changes') : tr('Save out of office') }}
            </button>
          </div>
        </div>
      </div>

      <!-- the entries -->
      <div class="fx-card">
        <div class="oo-list-head">
          <h2>{{ showAll ? tr('Everyone') : tr('My entries') }}</h2>
          <div v-if="admin" class="fx-seg" role="group">
            <button type="button" :aria-pressed="!showAll" @click="showAll = false">{{ tr('Mine') }}</button>
            <button type="button" :aria-pressed="showAll" @click="showAll = true">{{ tr('Everyone') }}</button>
          </div>
        </div>
        <div class="fx-table-wrap">
          <table class="fx-table">
            <thead>
              <tr><th>{{ tr('Away') }}</th><th>{{ tr('Covered by') }}</th><th>{{ tr('Workflows') }}</th><th>{{ tr('Dates') }}</th><th>{{ tr('State') }}</th><th></th></tr>
            </thead>
            <tbody>
              <tr v-for="e in entries" :key="e.outOfOfficeId">
                <td>{{ e.userFullName || e.userName }}<span v-if="e.note" class="fx-cell-sub">{{ e.note }}</span></td>
                <td>{{ e.delegateFullName || e.delegateTo }}</td>
                <td>{{ e.workflowName || tr('All workflows') }}</td>
                <td>{{ day(e.fromDate) }} – {{ day(e.toDate) }}</td>
                <td><span class="fx-chip" :class="chip(e.state)">{{ stateLabel(e.state) }}</span></td>
                <td class="fx-r oo-row-actions">
                  <template v-if="canEdit(e)">
                    <template v-if="removing === e.outOfOfficeId">
                      <span class="oo-confirm">{{ tr('Remove?') }}</span>
                      <button type="button" class="fx-btn fx-btn--sm fx-btn--primary" :disabled="busy" @click="remove(e)">{{ tr('Yes') }}</button>
                      <button type="button" class="fx-btn fx-btn--sm" @click="removing = null">{{ tr('No') }}</button>
                    </template>
                    <template v-else>
                      <button type="button" class="fx-btn fx-btn--sm" @click="edit(e)">{{ tr('Change') }}</button>
                      <button type="button" class="fx-btn fx-btn--sm" @click="removing = e.outOfOfficeId">{{ tr('Remove') }}</button>
                    </template>
                  </template>
                  <span v-else class="fx-cell-sub">{{ tr('You cover this') }}</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
        <div v-if="!entries.length" class="fx-empty"><span class="fa fa-calendar-o" aria-hidden="true"></span>{{ tr('No out of office entries yet.') }}</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import { openWorkflowManual } from '../utils/manual'
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { tr } from '@/shared/layout/language'
import { getUser, hasPermission } from '@/shared/auth/session'
import { apiErrorText } from '../services/formService'
import { outOfOfficeApi, type OutOfOffice, type UserOption, type WorkflowOption } from '../services/outOfOfficeService'

const me = getUser()?.userName ?? ''
const admin = hasPermission('WFE.DEFINITION.EDIT')

const users = ref<UserOption[]>([])
const workflows = ref<WorkflowOption[]>([])
const mine = ref<OutOfOffice[]>([])
const everyone = ref<OutOfOffice[]>([])
const showAll = ref(false)
const busy = ref(false)
const removing = ref<number | null>(null)
const message = ref<{ text: string; bad: boolean } | null>(null)

function today(offset = 0) {
  const d = new Date()
  d.setDate(d.getDate() + offset)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

const form = reactive({ id: 0, userName: me, delegateTo: '', workflowCode: '', fromDate: today(), toDate: today(7), note: '', isActive: true, rowVersion: 0 })

const entries = computed(() => (showAll.value ? everyone.value : mine.value))
const awayNow = computed(() => mine.value.find((e) => e.userName.toLowerCase() === me.toLowerCase() && e.state === 'AWAY' && !e.workflowCode)
  ?? mine.value.find((e) => e.userName.toLowerCase() === me.toLowerCase() && e.state === 'AWAY'))

const formProblem = computed(() => {
  if (!form.delegateTo) return tr('Choose who works the tasks while you are away.')
  if (!form.fromDate || !form.toDate) return tr('Enter the first and last day away.')
  if (form.toDate < form.fromDate) return tr('The last day is before the first day.')
  return ''
})

function label(u: UserOption) { return u.fullName ? `${u.fullName} (${u.userName})` : u.userName }
function day(d: string) { return new Date(d + 'T00:00:00').toLocaleDateString('en-GB', { day: '2-digit', month: 'short', year: 'numeric' }) }
function chip(s: string) { return s === 'AWAY' ? 'fx-chip--brand' : s === 'PLANNED' ? 'fx-chip--teal' : 'fx-chip--plain' }
function stateLabel(s: string) { return s === 'AWAY' ? tr('Away now') : s === 'PLANNED' ? tr('Planned') : s === 'ENDED' ? tr('Ended') : tr('Off') }
function canEdit(e: OutOfOffice) { return admin || e.userName.toLowerCase() === me.toLowerCase() }

function reset() {
  Object.assign(form, { id: 0, userName: me, delegateTo: '', workflowCode: '', fromDate: today(), toDate: today(7), note: '', isActive: true, rowVersion: 0 })
}

function edit(e: OutOfOffice) {
  Object.assign(form, { id: e.outOfOfficeId, userName: e.userName, delegateTo: e.delegateTo, workflowCode: e.workflowCode ?? '', fromDate: e.fromDate, toDate: e.toDate, note: e.note ?? '', isActive: e.isActive, rowVersion: e.rowVersion })
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

function errorText(e: unknown) {
  // validation errors come back per field; show them as one line
  return apiErrorText(e, tr('Something went wrong.'))
}

async function save() {
  if (formProblem.value) return
  busy.value = true
  try {
    const saved = await outOfOfficeApi.save(form.id || null, {
      userName: form.userName, delegateTo: form.delegateTo, workflowCode: form.workflowCode || null, fromDate: form.fromDate, toDate: form.toDate,
      isActive: form.isActive, note: form.note || null, rowVersion: form.rowVersion
    }, admin && !canOwn(form.userName))
    message.value = { bad: false, text: `${tr('Saved.')} ${saved.userFullName || saved.userName}: ${day(saved.fromDate)} – ${day(saved.toDate)}, ${tr('covered by')} ${saved.delegateFullName || saved.delegateTo}.` }
    reset()
    await load()
  } catch (e) {
    message.value = { bad: true, text: errorText(e) }
  } finally {
    busy.value = false
  }
}

function canOwn(user: string) { return user.toLowerCase() === me.toLowerCase() }

async function remove(e: OutOfOffice) {
  busy.value = true
  try {
    await outOfOfficeApi.remove(e.outOfOfficeId, admin && !canOwn(e.userName))
    removing.value = null
    message.value = { bad: false, text: tr('Removed.') }
    if (form.id === e.outOfOfficeId) reset()
    await load()
  } catch (err) {
    message.value = { bad: true, text: errorText(err) }
  } finally {
    busy.value = false
  }
}

async function load() {
  try {
    mine.value = await outOfOfficeApi.mine()
    if (admin) everyone.value = await outOfOfficeApi.all()
  } catch (e) {
    message.value = { bad: true, text: errorText(e) }
  }
}

watch(showAll, () => { removing.value = null })

onMounted(async () => {
  outOfOfficeApi.users().then((x) => (users.value = x)).catch(() => (users.value = []))
  outOfOfficeApi.workflows().then((x) => (workflows.value = x)).catch(() => (workflows.value = []))
  await load()
})
</script>

<style scoped>
.oo-status, .oo-msg { display: flex; align-items: center; gap: 10px; margin: 0 0 14px; padding: 11px 14px; border-radius: 10px; font-size: 13.5px; }
.oo-status.is-in { background: #e7f3f2; border: 1px solid #cfe7e6; color: #014b55; }
.oo-status.is-in .fa { color: #00636D; }
.oo-status.is-away { background: #fff4ea; border: 1px solid #f5c79f; color: #7a3d0a; }
.oo-status.is-away .fa { color: #E87722; }
.oo-msg { background: #e4f6ee; border: 1px solid #bfe5d2; color: #0e5a3c; }
.oo-msg.is-bad { background: #fdecea; border-color: #f3c3bc; color: #8a2416; }
.oo-msg > span:nth-child(2) { flex: 1; }
.oo-x { border: 0; background: none; color: inherit; cursor: pointer; }
.oo-grid { display: grid; grid-template-columns: minmax(300px, 400px) minmax(0, 1fr); gap: 16px; align-items: start; }
.oo-sub { margin: 0 0 14px; color: #5f7787; font-size: 12.5px; }
.oo-form { display: grid; gap: 12px; }
.oo-field { display: grid; gap: 5px; }
.oo-field > span { color: #12324a; font-size: 12px; font-weight: 700; }
.oo-field .fx-select, .oo-field .fx-search { width: 100%; min-width: 0; max-width: 100%; flex: none; }
.oo-dates { display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }
.oo-check { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #12324a; }
.oo-check small { color: #5f7787; }
.oo-problem { margin: 0; color: #b4520b; font-size: 12.5px; }
.oo-actions { display: flex; justify-content: flex-end; gap: 8px; }
.oo-list-head { display: flex; align-items: center; justify-content: space-between; gap: 10px; padding: 14px 18px 10px; }
.oo-list-head h2 { margin: 0; }
.oo-row-actions { white-space: nowrap; }
.oo-row-actions .fx-btn + .fx-btn { margin-left: 6px; }
.oo-confirm { margin-right: 6px; font-size: 12px; font-weight: 700; color: #b4520b; }
@media (max-width: 900px) { .oo-grid { grid-template-columns: 1fr; } }
</style>
