<template>
  <!--
    One decision: entered and changed while DRAFT or RETURNED, then submitted to the SCR_DECISION_APPROVAL workflow.
    Opened from the inbox (?inboxId=), it is the page of the step: the reviewer accepts or returns, the publisher approves
    and publishes or returns. Reviewer and approver are never the person who entered the decision (checked by the server).
  -->
  <div class="fx fc">
    <div class="fx-head">
      <div>
        <nav class="fc-crumbs" :aria-label="tr('Breadcrumb')">
          <RouterLink :to="{ name: 'scr-home' }">{{ tr('Screening') }}</RouterLink><span aria-hidden="true">›</span>
          <RouterLink :to="{ name: 'scr-decisions' }">{{ tr('Decisions') }}</RouterLink><span aria-hidden="true">›</span>
          <b dir="ltr">{{ decision?.decisionRef ?? tr('New decision') }}</b>
        </nav>
        <h1>
          {{ decision ? `${decision.decisionRef} · ${decision.officialNo}` : tr('New decision') }}
          <span v-if="decision" class="fx-chip" :class="CHIP[decision.statusCode]">{{ tr(LABELS[decision.statusCode] ?? decision.statusCode) }}</span>
        </h1>
      </div>
    </div>

    <section v-if="task" class="fx-card">
      <h3>{{ tr(task.stepName ?? 'Workflow step') }}</h3>
      <p class="scr-muted">{{ task.stepCode === 'SCR_REVIEW'
        ? tr('Check the decision against its letter and parties. Accept sends it for approval; Return sends it back to data entry.')
        : tr('Approving publishes the decision to the Federation Final List as the next version. Return sends it back to data entry.') }}</p>
      <label class="scr-field"><span>{{ tr('Comment') }}</span><textarea v-model="comment" rows="2"></textarea></label>
      <div class="scr-actions">
        <button type="button" class="fx-btn fx-btn--bad" :disabled="busy || !taskOpen" @click="complete('REJECT')">{{ tr('Return to data entry') }}</button>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !taskOpen" @click="complete('ACCEPT')">
          {{ task.stepCode === 'SCR_REVIEW' ? tr('Accept review') : tr('Approve and publish') }}
        </button>
      </div>
      <p v-if="!taskOpen" class="scr-muted">{{ tr('Claim and start the task from the inbox first.') }}</p>
    </section>

    <section v-if="decision?.returnReason && decision.statusCode === 'RETURNED'" class="fx-card">
      <b>{{ tr('Returned') }}:</b> {{ decision.returnReason }}
    </section>

    <section class="fx-card">
      <div class="scr-grid scr-grid--3">
        <label class="scr-field"><span>{{ tr('Type') }} *</span>
          <select v-model="form.decisionTypeCode" :disabled="!editable">
            <option v-for="t in ['LISTING', 'AMENDMENT', 'SUSPENSION', 'LIFTING']" :key="t" :value="t">{{ tr(LABELS[t] ?? t) }}</option>
          </select>
        </label>
        <label class="scr-field"><span>{{ tr('Official number') }} *</span><input v-model="form.officialNo" dir="ltr" :disabled="!editable"><small v-if="errors.officialNo" class="err">{{ errors.officialNo }}</small></label>
        <label class="scr-field"><span>{{ tr('Effective date') }} *</span><input v-model="form.effectiveDate" type="date" :disabled="!editable"><small v-if="errors.effectiveDate" class="err">{{ errors.effectiveDate }}</small></label>
        <label v-if="form.decisionTypeCode !== 'LISTING'" class="scr-field"><span>{{ tr('Listing it changes') }} *</span>
          <select v-model="form.parentDecisionId" :disabled="!editable" @change="parentChanged">
            <option :value="null">—</option>
            <option v-for="l in listings" :key="l.decisionId" :value="l.decisionId">{{ l.decisionRef }} · {{ l.officialNo }}</option>
          </select>
          <small v-if="errors.parentDecisionId" class="err">{{ errors.parentDecisionId }}</small>
        </label>
        <label class="scr-field"><span>{{ tr('Letter') }}</span>
          <select v-model="form.letterId" :disabled="!editable">
            <option :value="null">—</option>
            <option v-for="l in letters" :key="l.letterId" :value="l.letterId">{{ l.letterNo }} · {{ l.senderName }}</option>
          </select>
        </label>
        <label class="scr-field scr-wide"><span>{{ tr('Scope') }}{{ form.decisionTypeCode === 'LIFTING' ? '' : ' *' }}</span>
          <textarea v-model="form.scopeText" rows="2" :disabled="!editable"></textarea><small v-if="errors.scopeText" class="err">{{ errors.scopeText }}</small>
        </label>
      </div>

      <h3>{{ tr('Parties') }} *</h3>
      <small v-if="errors.partyIds" class="err">{{ errors.partyIds }}</small>
      <div v-if="editable" class="scr-filter">
        <select v-model="pick" class="fx-select">
          <option :value="null">{{ tr('Add a party...') }}</option>
          <option v-for="p in pickable" :key="p.partyId" :value="p.partyId">{{ p.nameAr }} · {{ p.identityNo }}</option>
        </select>
        <button type="button" class="fx-btn" :disabled="pick === null" @click="addParty">{{ tr('Add') }}</button>
        <RouterLink :to="{ name: 'scr-parties' }" class="fx-btn">{{ tr('Parties') }}</RouterLink>
      </div>
      <table class="fx-table">
        <thead><tr><th>{{ tr('Name') }}</th><th>{{ tr('Identity number') }}</th><th>{{ tr('Nationality') }}</th><th></th></tr></thead>
        <tbody>
          <tr v-for="p in chosen" :key="p.partyId">
            <td><b>{{ p.nameAr }}</b> <small class="scr-muted">{{ p.nameEn }}</small></td>
            <td dir="ltr">{{ p.identityNo }} <small class="scr-muted">{{ tr(LABELS[p.identityTypeCode] ?? p.identityTypeCode) }}</small></td>
            <td>{{ p.nationalityCode }}</td>
            <td><button v-if="editable" type="button" class="fx-btn fx-btn--sm" @click="form.partyIds = form.partyIds.filter((x) => x !== p.partyId)">{{ tr('Remove') }}</button></td>
          </tr>
        </tbody>
      </table>

      <div v-if="decision" class="scr-grid scr-grid--3" style="margin-top: 14px">
        <div><small class="scr-muted">{{ tr('Entered by') }}</small><br>{{ decision.enteredBy }} · {{ formatDateTime(decision.createdAt) }}</div>
        <div><small class="scr-muted">{{ tr('Reviewed by') }}</small><br>{{ decision.reviewedBy ?? '—' }}<template v-if="decision.reviewedAt"> · {{ formatDateTime(decision.reviewedAt) }}</template></div>
        <div><small class="scr-muted">{{ tr('Approved by') }}</small><br>{{ decision.approvedBy ?? '—' }}<template v-if="decision.publishedAt"> · {{ formatDateTime(decision.publishedAt) }} · V{{ decision.registryVersionNo }}</template></div>
      </div>

      <div v-if="editable" class="scr-actions">
        <button v-if="decision" type="button" class="fx-btn fx-btn--bad" :disabled="busy" @click="cancelDecision">{{ tr('Cancel decision') }}</button>
        <button type="button" class="fx-btn" :disabled="busy" @click="save">{{ tr('Save') }}</button>
        <button v-if="decision" type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="submit">{{ tr('Submit for review') }}</button>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import '@/modules/fct/pages/factoring.css'
import '../components/scr.css'
import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { hasPermission } from '@/shared/auth/session'
import { showError, showSuccess } from '@/shared/feedback/feedback'
import { formatDateTime } from '@/shared/format/datetime'
import { tr } from '@/shared/layout/language'
import { workflowService } from '@/modules/wfe/services/workflowService'
import type { WorkflowInboxTask } from '@/modules/wfe/models/workflow'
import { CHIP, LABELS, errorText, fieldErrors, scrApi, type ScrDecision, type ScrDecisionRequest, type ScrLetter, type ScrParty } from '../api/scr.api'

const props = defineProps<{ decisionId?: number }>()
const route = useRoute()
const router = useRouter()
const canEdit = hasPermission('SCR.DECISION.EDIT')

const decision = ref<ScrDecision | null>(null)
const letters = ref<ScrLetter[]>([])
const parties = ref<ScrParty[]>([])
const listings = ref<ScrDecision[]>([])
const task = ref<WorkflowInboxTask | null>(null)
const comment = ref('')
const pick = ref<number | null>(null)
const busy = ref(false)
const errors = ref<Record<string, string>>({})
const form = reactive<ScrDecisionRequest>({
  officialNo: '', decisionTypeCode: 'LISTING', parentDecisionId: null, letterId: null, scopeText: '', effectiveDate: null, partyIds: []
})

const editable = computed(() => canEdit && !task.value && (!decision.value || ['DRAFT', 'RETURNED'].includes(decision.value.statusCode)))
const taskOpen = computed(() => task.value?.status === 'IN_PROGRESS' || task.value?.status === 'CLAIMED')
const allParties = computed(() => {
  const byId = new Map<number, ScrParty>()
  for (const p of [...parties.value, ...(decision.value?.parties ?? [])]) byId.set(p.partyId, p)
  return byId
})
const chosen = computed(() => form.partyIds.map((id) => allParties.value.get(id)).filter((p): p is ScrParty => !!p))
/** For a change to a listing, only that listing's parties. */
const pickable = computed(() => {
  const parent = listings.value.find((l) => l.decisionId === form.parentDecisionId)
  const pool = form.decisionTypeCode !== 'LISTING' && parent ? parent.parties : parties.value
  return pool.filter((p) => !form.partyIds.includes(p.partyId))
})

function fill(d: ScrDecision) {
  decision.value = d
  Object.assign(form, {
    officialNo: d.officialNo, decisionTypeCode: d.decisionTypeCode, parentDecisionId: d.parentDecisionId, letterId: d.letterId,
    scopeText: d.scopeText, effectiveDate: d.effectiveDate, partyIds: d.parties.map((p) => p.partyId)
  })
}

function parentChanged() {
  const parent = listings.value.find((l) => l.decisionId === form.parentDecisionId)
  if (!parent) return
  const ids = parent.parties.map((p) => p.partyId)
  form.partyIds = form.partyIds.filter((id) => ids.includes(id))
  if (!form.partyIds.length) form.partyIds = ids
  if (!form.scopeText) form.scopeText = parent.scopeText
}

function addParty() {
  if (pick.value !== null && !form.partyIds.includes(pick.value)) form.partyIds = [...form.partyIds, pick.value]
  pick.value = null
}

async function run(work: () => Promise<void>) {
  busy.value = true
  errors.value = {}
  try {
    await work()
  } catch (err) {
    errors.value = fieldErrors(err)
    if (!Object.keys(errors.value).length) showError(errorText(err))
  } finally {
    busy.value = false
  }
}

const save = () => run(async () => {
  const d = await scrApi.saveDecision(decision.value?.decisionId ?? null, { ...form })
  showSuccess(`${tr('Decision saved')}: ${d.decisionRef}`)
  if (!decision.value) await router.replace({ name: 'scr-decision', params: { decisionId: d.decisionId } })
  fill(d)
})

const submit = () => run(async () => {
  const saved = await scrApi.saveDecision(decision.value!.decisionId, { ...form })
  fill(await scrApi.submit(saved.decisionId))
  showSuccess(tr('Sent for review. The reviewers have it in their inbox.'))
})

const cancelDecision = () => run(async () => {
  if (!window.confirm(tr('Cancel this decision? It will not be published.'))) return
  fill(await scrApi.cancel(decision.value!.decisionId))
})

const complete = (action: 'ACCEPT' | 'REJECT') => run(async () => {
  await workflowService.complete(task.value!.inboxId, { action, comment: comment.value || null })
  showSuccess(action === 'REJECT' ? tr('Returned to data entry.') : task.value!.stepCode === 'SCR_REVIEW' ? tr('Sent for approval.') : tr('Published to the Final List.'))
  await router.push({ name: 'workflow-inbox' })
})

onMounted(async () => {
  try {
    const inboxId = Number(route.query.inboxId)
    const jobs: Promise<unknown>[] = [
      scrApi.letters().then((v) => { letters.value = v }),
      scrApi.parties().then((v) => { parties.value = v }),
      scrApi.decisions('PUBLISHED').then((v) => { listings.value = v.filter((d) => d.decisionTypeCode === 'LISTING') })
    ]
    if (props.decisionId) jobs.push(scrApi.decision(props.decisionId).then(fill))
    if (Number.isFinite(inboxId) && inboxId > 0) jobs.push(workflowService.getTask(inboxId).then((t) => { task.value = t }))
    await Promise.all(jobs)
  } catch (err) {
    showError(errorText(err))
  }
})
</script>
