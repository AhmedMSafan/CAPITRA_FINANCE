<template>
  <!-- The Screening API's clients of a member company: its own systems (core banking, onboarding...) that screen through the API with a
       client ID and secret. The secret is shown once, when it is made. -->
  <ScrDialog :title="tr('Screening API clients')" :subtitle="companyName" icon="fa-plug" @close="emit('close')">
    <div class="scr-api">
      <p class="scr-api-intro">{{ tr('A client lets the company\'s own system screen through the Screening API. Its inquiries are kept, billed and audited like the portal\'s.') }}</p>

      <div v-if="secret" class="scr-api-secret" role="alert">
        <b><span class="fa fa-key" aria-hidden="true"></span>{{ tr('Copy the secret now: it is not shown again.') }}</b>
        <div class="scr-api-pair"><span>{{ tr('Client ID') }}</span><code dir="ltr">{{ secret.client.clientId }}</code>
          <button type="button" class="fx-btn fx-btn--sm" @click="copy(secret.client.clientId)"><span class="fa fa-copy" aria-hidden="true"></span>{{ tr('Copy') }}</button></div>
        <div class="scr-api-pair"><span>{{ tr('Client secret') }}</span><code dir="ltr">{{ secret.clientSecret }}</code>
          <button type="button" class="fx-btn fx-btn--sm" @click="copy(secret.clientSecret)"><span class="fa fa-copy" aria-hidden="true"></span>{{ tr('Copy') }}</button></div>
        <small>{{ tr('Give both to the company\'s IT team over a safe channel. If the secret is lost, give the client a new secret.') }}</small>
      </div>

      <div class="scr-api-new">
        <label class="fc-field"><span>{{ tr('New client: what is it for?') }}</span>
          <input v-model="name" class="fx-search" :placeholder="tr('e.g. Core banking onboarding')" maxlength="150"></label>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !name.trim()" @click="create">
          <span :class="['fa', busy ? 'fa-spinner fa-spin' : 'fa-plus']" aria-hidden="true"></span>{{ tr('Create client') }}</button>
      </div>

      <div class="fx-table-wrap scr-api-table">
        <table class="fx-table">
          <thead><tr><th>{{ tr('Client') }}</th><th>{{ tr('Client ID') }}</th><th>{{ tr('Secret') }}</th><th>{{ tr('Last token') }}</th><th>{{ tr('Inquiries') }}</th><th>{{ tr('Status') }}</th><th></th></tr></thead>
          <tbody>
            <tr v-for="c in list" :key="c.apiClientId">
              <td><b>{{ c.clientName }}</b><span class="fx-cell-sub">{{ tr('Created') }} {{ formatDate(c.createdAt) }} · <bdi dir="ltr">{{ c.createdBy }}</bdi></span></td>
              <td><code dir="ltr">{{ c.clientId }}</code></td>
              <td dir="ltr">…{{ c.secretHint }}<span class="fx-cell-sub">{{ formatDate(c.secretCreatedAt) }}</span></td>
              <td>{{ c.lastTokenAt ? formatDateTime(c.lastTokenAt) : tr('Never') }}<span v-if="c.lastIpAddress" class="fx-cell-sub" dir="ltr">{{ c.lastIpAddress }}</span></td>
              <td>{{ c.inquiryCount }}</td>
              <td><span class="fx-chip" :class="c.statusCode === 'ACTIVE' ? 'fx-chip--ok' : 'fx-chip--bad'">{{ tr(c.statusCode === 'ACTIVE' ? 'Active' : 'Revoked') }}</span>
                <span v-if="c.revokedAt" class="fx-cell-sub">{{ formatDate(c.revokedAt) }} · <bdi dir="ltr">{{ c.revokedBy }}</bdi></span></td>
              <td class="fx-r">
                <div v-if="c.statusCode === 'ACTIVE'" class="fc-row-actions">
                  <button type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="confirm = { client: c, action: 'secret' }"><span class="fa fa-refresh" aria-hidden="true"></span>{{ tr('New secret') }}</button>
                  <button type="button" class="fx-btn fx-btn--sm" :disabled="busy" @click="confirm = { client: c, action: 'revoke' }"><span class="fa fa-ban" aria-hidden="true"></span>{{ tr('Revoke') }}</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
        <div v-if="!list.length" class="fx-empty"><span class="fa fa-plug" aria-hidden="true"></span>{{ tr('No API clients yet.') }}</div>
      </div>

      <div v-if="confirm" class="scr-api-confirm" role="alertdialog">
        <p v-if="confirm.action === 'revoke'">{{ tr('Revoke this client? Its system can no longer screen, at once. It is kept with its inquiries.') }} <b>{{ confirm.client.clientName }}</b></p>
        <p v-else>{{ tr('Give this client a new secret? The old secret stops working at once.') }} <b>{{ confirm.client.clientName }}</b></p>
        <div class="fc-row-actions">
          <button type="button" class="fx-btn" @click="confirm = null">{{ tr('Cancel') }}</button>
          <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="act">{{ tr(confirm.action === 'revoke' ? 'Revoke' : 'New secret') }}</button>
        </div>
      </div>
    </div>
    <template #actions>
      <button type="button" class="fx-btn" @click="emit('close')">{{ tr('Close') }}</button>
    </template>
  </ScrDialog>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { tr } from '@/shared/layout/language'
import { formatDate, formatDateTime } from '../utils/dates'
import { errorText, scrApi, type ScrApiClient, type ScrApiClientSecret } from '../api/scr.api'
import ScrDialog from './ScrDialog.vue'

const props = defineProps<{ companyId: number; companyName: string }>()
const emit = defineEmits<{ close: [] }>()

const list = ref<ScrApiClient[]>([])
const name = ref('')
const busy = ref(false)
const secret = ref<ScrApiClientSecret | null>(null)
const confirm = ref<{ client: ScrApiClient; action: 'revoke' | 'secret' } | null>(null)

async function load() {
  try {
    list.value = await scrApi.apiClients(props.companyId)
  } catch (err) {
    showError(errorText(err))
  }
}

async function create() {
  busy.value = true
  try {
    secret.value = await scrApi.createApiClient(props.companyId, { clientName: name.value.trim(), notes: null })
    name.value = ''
    await load()
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function act() {
  if (!confirm.value) return
  busy.value = true
  const { client, action } = confirm.value
  try {
    if (action === 'revoke') {
      await scrApi.revokeApiClient(props.companyId, client.apiClientId)
      secret.value = null
      showSuccess(tr('The client is revoked.'))
    } else {
      secret.value = await scrApi.newApiClientSecret(props.companyId, client.apiClientId)
    }
    confirm.value = null
    await load()
  } catch (err) {
    showError(errorText(err))
  } finally {
    busy.value = false
  }
}

async function copy(text: string) {
  try {
    await navigator.clipboard.writeText(text)
    showSuccess(tr('Copied.'))
  } catch {
    showError(tr('Copy did not work: select the text and copy it.'))
  }
}

onMounted(load)
</script>
