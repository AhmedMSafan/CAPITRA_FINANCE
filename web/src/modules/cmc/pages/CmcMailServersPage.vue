<template>
  <!--
    Mail servers (Communication Center). Add a server, send a test e-mail through it, then use it: from then on every e-mail of
    CAPITRA (account activation, password reset) goes through it. Only one is in use. The password is stored encrypted and never
    shown again; leaving it empty when editing keeps it.
  -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'cmc-home' }">{{ tr('Communication Center') }}</RouterLink><span aria-hidden="true">›</span><b>{{ tr('Mail servers') }}</b>
        </nav>
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-server"></span></span>
          <div>
            <h1>{{ tr('Mail servers') }}</h1>
            <p>{{ tr('Every e-mail of CAPITRA goes through the server in use. Test a server before using it.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <button type="button" class="fx-btn fx-btn--primary" @click="edit(null)"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('New mail server') }}</button>
      </div>
    </div>

    <div v-if="status && status.sourceCode !== 'COMMUNICATION_CENTER'" class="fc-note fc-note--warn cmc-top-note">
      <span class="fa fa-info-circle" aria-hidden="true"></span>
      <span v-if="status.sourceCode === 'APPLICATION_SETTINGS'">{{ tr('No server here is in use, so e-mails go through the server in the application settings') }} (<bdi dir="ltr">{{ status.hostName }}</bdi>). {{ tr('Use a server here to manage it from this page.') }}</span>
      <span v-else>{{ tr('No mail server is in use: e-mails are not sent, and administrators must copy activation links by hand. Add a server, test it, then use it.') }}</span>
    </div>

    <div class="cmc-servers">
      <article v-for="s in servers" :key="s.mailServerId" class="fx-card cmc-server" :class="{ 'cmc-server--active': s.isActive }">
        <header class="cmc-server-head">
          <span class="cmc-server-ic" aria-hidden="true"><span class="fa fa-server"></span></span>
          <div>
            <h2>{{ s.serverName }}</h2>
            <small dir="ltr">{{ s.hostName }}:{{ s.portNo }}</small>
          </div>
          <span v-if="s.isActive" class="fx-chip fx-chip--ok"><span class="fa fa-check" aria-hidden="true"></span>{{ tr('In use') }}</span>
          <span v-else class="fx-chip fx-chip--plain">{{ tr('Not in use') }}</span>
        </header>
        <dl class="cmc-facts">
          <div><dt>{{ tr('Sender') }}</dt><dd><bdi>{{ s.fromName }}</bdi> &lt;<bdi dir="ltr">{{ s.fromAddress }}</bdi>&gt;</dd></div>
          <div><dt>{{ tr('Secure connection') }}</dt><dd>{{ tr(s.isSslEnabled ? 'Yes (STARTTLS)' : 'No') }}</dd></div>
          <div><dt>{{ tr('Sign-in') }}</dt><dd>
            <template v-if="s.userName"><bdi dir="ltr">{{ s.userName }}</bdi> · <span :class="s.passwordUnreadable ? 'cmc-bad' : ''">{{ tr(s.passwordUnreadable ? 'Password must be entered again' : s.hasPassword ? 'Password stored (encrypted)' : 'No password') }}</span></template>
            <template v-else>{{ tr('None') }}</template></dd></div>
          <div><dt>{{ tr('Last test') }}</dt><dd>
            <template v-if="s.lastTestAt"><span class="fx-chip" :class="STATUS_CHIP[s.lastTestStatusCode ?? '']">{{ tr(LABELS[s.lastTestStatusCode ?? ''] ?? '') }}</span> {{ formatDateTime(s.lastTestAt) }}</template>
            <template v-else>{{ tr('Not tested yet') }}</template></dd></div>
        </dl>
        <p v-if="s.lastTestStatusCode === 'FAILED' && s.lastTestError" class="cmc-error" dir="ltr">{{ s.lastTestError }}</p>
        <footer class="fc-row-actions cmc-server-actions">
          <button type="button" class="fx-btn fx-btn--sm" @click="edit(s)"><span class="fa fa-pencil" aria-hidden="true"></span>{{ tr('Edit') }}</button>
          <button type="button" class="fx-btn fx-btn--sm fx-btn--teal" @click="startTest(s)"><span class="fa fa-paper-plane" aria-hidden="true"></span>{{ tr('Send a test e-mail') }}</button>
          <button v-if="!s.isActive" type="button" class="fx-btn fx-btn--sm fx-btn--primary" :disabled="busy || s.lastTestStatusCode !== 'SENT'"
                  :title="s.lastTestStatusCode !== 'SENT' ? tr('Send a test e-mail that arrives first.') : ''" @click="use(s)">
            <span class="fa fa-check-circle" aria-hidden="true"></span>{{ tr('Use this server') }}</button>
          <button v-else type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="stop(s)"><span class="fa fa-pause" aria-hidden="true"></span>{{ tr('Stop using') }}</button>
        </footer>
      </article>
    </div>
    <div v-if="loaded && !servers.length" class="fx-card fx-empty"><span class="fa fa-server" aria-hidden="true"></span>{{ tr('No mail server yet. Add one to send e-mails.') }}</div>

    <!-- add or change a server -->
    <ScrDialog v-if="editing" :title="editId ? tr('Edit mail server') : tr('New mail server')" icon="fa-server" @close="editing = false">
      <div class="fc-grid fc-grid--2">
        <div class="fc-full cmc-presets">
          <span>{{ tr('Fill in for') }}</span>
          <button v-for="p in PRESETS" :key="p.name" type="button" class="fx-btn fx-btn--sm" @click="preset(p)">{{ p.name }}</button>
        </div>
        <label class="fc-field"><span>{{ tr('Name') }} <b>*</b></span><input v-model="form.serverName" class="fx-search" :placeholder="tr('e.g. Gmail (testing)')">
          <small v-if="errors.serverName" class="scr-err">{{ errors.serverName }}</small></label>
        <label class="fc-field"><span>{{ tr('Host') }} <b>*</b></span><input v-model="form.hostName" class="fx-search" dir="ltr" placeholder="smtp.gmail.com">
          <small v-if="errors.hostName" class="scr-err">{{ errors.hostName }}</small></label>
        <label class="fc-field"><span>{{ tr('Port') }} <b>*</b></span><input v-model.number="form.portNo" class="fx-search" type="number" min="1" max="65535" dir="ltr">
          <small class="scr-hint">{{ tr('587 for Gmail and Office 365') }}</small>
          <small v-if="errors.portNo" class="scr-err">{{ errors.portNo }}</small></label>
        <label class="fc-check" style="align-self:end;padding-bottom:8px"><input v-model="form.isSslEnabled" type="checkbox"><span>{{ tr('Secure connection (STARTTLS)') }}</span></label>
        <label class="fc-field"><span>{{ tr('Sign-in user name') }}</span><input v-model="form.userName" class="fx-search" dir="ltr" autocomplete="off" :placeholder="tr('Empty = no sign-in')"></label>
        <label class="fc-field"><span>{{ tr('Password') }}<template v-if="form.userName"> <b>*</b></template></span>
          <input v-model="form.password" class="fx-search" type="password" dir="ltr" autocomplete="new-password" :disabled="!form.userName"
                 :placeholder="editing && current?.hasPassword && !form.clearPassword ? tr('Stored: leave empty to keep it') : ''">
          <small class="scr-hint">{{ tr('For Gmail: an app password (Google account › Security › 2-Step Verification › App passwords), not your Gmail password.') }}</small>
          <small v-if="errors.password" class="scr-err">{{ errors.password }}</small></label>
        <label class="fc-field"><span>{{ tr('Sender e-mail') }} <b>*</b></span><input v-model="form.fromAddress" class="fx-search" type="email" dir="ltr">
          <small class="scr-hint">{{ tr('Gmail sends only from the signed-in address.') }}</small>
          <small v-if="errors.fromAddress" class="scr-err">{{ errors.fromAddress }}</small></label>
        <label class="fc-field"><span>{{ tr('Sender name') }}</span><input v-model="form.fromName" class="fx-search" placeholder="CAPITRA"></label>
        <div class="fc-note fc-full"><span class="fa fa-lock" aria-hidden="true"></span>
          <span>{{ tr('The password is stored encrypted and is never shown again. A changed server must be tested again before it is used.') }}</span></div>
        <div v-if="errors.general" class="fc-note fc-note--warn fc-full"><span class="fa fa-exclamation-circle" aria-hidden="true"></span><span>{{ errors.general }}</span></div>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" @click="editing = false">{{ tr('Cancel') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="save">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-check']" aria-hidden="true"></span>{{ tr('Save') }}
        </button>
      </template>
    </ScrDialog>

    <!-- test e-mail -->
    <ScrDialog v-if="testing" :title="tr('Send a test e-mail')" :subtitle="testing.serverName" icon="fa-paper-plane" @close="closeTest">
      <label class="fc-field"><span>{{ tr('Send to') }}</span><input v-model="testTo" class="fx-search" type="email" dir="ltr"></label>
      <div v-if="testResult" class="fc-note cmc-test-note" :class="testResult.sent ? '' : 'fc-note--warn'">
        <span :class="['fa', testResult.sent ? 'fa-check-circle' : 'fa-exclamation-triangle']" aria-hidden="true"></span>
        <span v-if="testResult.sent">{{ tr('The mail server accepted the e-mail. Check that it arrived (and the spam folder), then use this server.') }}</span>
        <span v-else><b>{{ tr('The e-mail could not be sent.') }}</b> <bdi dir="ltr">{{ testResult.error }}</bdi></span>
      </div>
      <template #actions>
        <button type="button" class="fx-btn" @click="closeTest">{{ tr('Close') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="busy || !testTo.trim()" @click="sendTest">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-paper-plane']" aria-hidden="true"></span>{{ tr(testResult ? 'Send again' : 'Send') }}
        </button>
      </template>
    </ScrDialog>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '@/modules/scr/components/scr.css'
import './cmc.css'
import { onMounted, reactive, ref } from 'vue'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDateTime } from '@/modules/scr/utils/dates'
import ScrDialog from '@/modules/scr/components/ScrDialog.vue'
import { LABELS, STATUS_CHIP, cmcApi, errorText, fieldErrors, type CmcMailServer, type CmcMailServerRequest, type CmcMailStatus, type CmcTestResult } from '../api/cmc.api'

const PRESETS = [
  { name: 'Gmail', hostName: 'smtp.gmail.com', portNo: 587, isSslEnabled: true },
  { name: 'Office 365', hostName: 'smtp.office365.com', portNo: 587, isSslEnabled: true }
]

const servers = ref<CmcMailServer[]>([])
const status = ref<CmcMailStatus | null>(null)
const loaded = ref(false)
const busy = ref(false)

const editing = ref(false)
const editId = ref<number | null>(null)
const current = ref<CmcMailServer | null>(null)
const errors = ref<Record<string, string>>({})
const blank = (): CmcMailServerRequest => ({ serverName: '', hostName: '', portNo: 587, isSslEnabled: true, userName: null, password: null, clearPassword: false, fromAddress: '', fromName: 'CAPITRA' })
const form = reactive<CmcMailServerRequest>(blank())

const testing = ref<CmcMailServer | null>(null)
const testTo = ref('')
const testResult = ref<CmcTestResult | null>(null)

async function load() {
  try {
    ;[servers.value, status.value] = await Promise.all([cmcApi.servers(), cmcApi.status()])
    loaded.value = true
  } catch (err) {
    showError(errorText(err))
  }
}

function edit(s: CmcMailServer | null) {
  errors.value = {}
  editId.value = s?.mailServerId ?? null
  current.value = s
  Object.assign(form, s
    ? { serverName: s.serverName, hostName: s.hostName, portNo: s.portNo, isSslEnabled: s.isSslEnabled, userName: s.userName, password: null, clearPassword: false, fromAddress: s.fromAddress, fromName: s.fromName }
    : blank())
  editing.value = true
}

function preset(p: (typeof PRESETS)[number]) {
  form.hostName = p.hostName
  form.portNo = p.portNo
  form.isSslEnabled = p.isSslEnabled
  if (!form.serverName) form.serverName = p.name
}

async function save() {
  busy.value = true
  errors.value = {}
  const userName = form.userName?.trim() || null
  const request = { ...form, userName, password: form.password || null, clearPassword: !userName }
  try {
    if (editId.value) await cmcApi.updateServer(editId.value, request)
    else await cmcApi.createServer(request)
    showSuccess(tr('Mail server saved. Send a test e-mail through it.'))
    editing.value = false
    await load()
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) errors.value = { general: errorText(err) }
  } finally {
    busy.value = false
  }
}

function startTest(s: CmcMailServer) {
  testing.value = s
  testResult.value = null
  testTo.value = testTo.value || s.fromAddress
}

async function sendTest() {
  if (!testing.value) return
  busy.value = true
  try {
    testResult.value = await cmcApi.testServer(testing.value.mailServerId, testTo.value.trim())
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function closeTest() {
  testing.value = null
  await load()
}

async function use(s: CmcMailServer) {
  busy.value = true
  try {
    await cmcApi.activate(s.mailServerId)
    showSuccess(tr('Every e-mail now goes through this server.'))
    await load()
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function stop(s: CmcMailServer) {
  busy.value = true
  try {
    await cmcApi.deactivate(s.mailServerId)
    showSuccess(tr('The server is no longer used.'))
    await load()
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

onMounted(load)
</script>
