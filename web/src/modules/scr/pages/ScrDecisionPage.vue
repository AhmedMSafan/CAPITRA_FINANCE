<template>
  <!--
    One decision: entered and changed while DRAFT or RETURNED. Saved decisions wait in Decisions awaiting approval, where Run starts
    their SCR_DECISION_APPROVAL workflow.
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
        <div class="fc-title">
          <span class="fc-title-ic" aria-hidden="true"><span class="fa fa-gavel"></span></span>
          <div>
            <span v-if="task" class="fc-eyebrow">{{ tr('WORKFLOW TASK') }}</span>
            <h1>
              <span dir="ltr">{{ decision ? decision.decisionRef : tr('New decision') }}</span>
              <span v-if="decision" class="fx-chip" :class="CHIP[decision.statusCode]">{{ tr(LABELS[decision.statusCode] ?? decision.statusCode) }}</span>
            </h1>
            <p>{{ decision ? `${tr(LABELS[decision.decisionTypeCode] ?? decision.decisionTypeCode)} · ${decision.officialNo}` : tr('Enter a decision from a letter; then run its approval workflow from Decisions awaiting approval.') }}</p>
          </div>
        </div>
      </div>
      <div class="fx-actions">
        <button v-if="task && !viewOnly" type="button" class="fx-btn" @click="router.push({ name: 'workflow-inbox' })"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back to Task') }}</button>
        <button v-else-if="viewOnly" type="button" class="fx-btn" @click="router.back()"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Back') }}</button>
        <RouterLink v-else :to="{ name: 'scr-approvals' }" class="fx-btn"><span class="fa fa-arrow-left" aria-hidden="true"></span>{{ tr('Awaiting approval') }}</RouterLink>
      </div>
    </div>

    <!-- where the decision is in its life -->
    <section v-if="decision" class="fx-card">
      <ol class="fc-process">
        <li v-for="(s, i) in steps" :key="s.code" :class="{ 'is-done': stepIndex > i, 'is-current': stepIndex === i }">
          <span class="fc-process-no">{{ stepIndex > i ? '✓' : i + 1 }}</span>
          <div><b>{{ tr(s.title) }}</b><small>{{ s.who }}</small></div>
        </li>
      </ol>
    </section>

    <div v-if="viewOnly" class="fc-note fc-step-card"><span class="fa fa-eye" aria-hidden="true"></span>
      <span>{{ tr('View only: a completed step, with what was decided and its documents. Nothing can be changed here.') }}</span></div>

    <div v-if="decision?.statusCode === 'RETURNED' && decision.returnReason" class="fc-note fc-note--warn fc-step-card">
      <span class="fa fa-undo" aria-hidden="true"></span><span><b>{{ tr('Returned') }}:</b> {{ decision.returnReason }}</span>
    </div>

    <!-- the decision -->
    <section class="fx-card fc-step-card">
      <header class="fc-sec-head">
        <span class="fc-sec-ic fc-sec-ic--orange" aria-hidden="true"><span class="fa fa-file-text-o"></span></span>
        <div>
          <h2>{{ tr('Decision') }}</h2>
          <p>{{ editable ? tr('Fields marked * are required.') : tr('Read only: the decision is in the workflow or closed.') }}</p>
        </div>
      </header>
      <div class="fc-grid fc-grid--3">
        <label class="fc-field"><span>{{ tr('Type') }} <b>*</b></span>
          <select v-model="form.decisionTypeCode" class="fx-select" :disabled="!editable">
            <option v-for="t in ['LISTING', 'AMENDMENT', 'SUSPENSION', 'LIFTING']" :key="t" :value="t">{{ tr(LABELS[t] ?? t) }}</option>
          </select>
        </label>
        <label class="fc-field"><span>{{ tr('Official number') }} <b>*</b></span><input v-model="form.officialNo" class="fx-search" dir="ltr" :disabled="!editable">
          <small v-if="errors.officialNo" class="scr-err">{{ errors.officialNo }}</small></label>
        <label class="fc-field"><span>{{ tr('Effective date') }} <b>*</b></span><input v-model="form.effectiveDate" class="fx-search" type="date" :disabled="!editable">
          <small v-if="errors.effectiveDate" class="scr-err">{{ errors.effectiveDate }}</small></label>
        <label v-if="form.decisionTypeCode !== 'LISTING'" class="fc-field"><span>{{ tr('Listing it changes') }} <b>*</b></span>
          <select v-model="form.parentDecisionId" class="fx-select" :disabled="!editable" @change="parentChanged">
            <option :value="null">—</option>
            <option v-for="l in listings" :key="l.decisionId" :value="l.decisionId">{{ l.decisionRef }} · {{ l.officialNo }}</option>
          </select>
          <small v-if="errors.parentDecisionId" class="scr-err">{{ errors.parentDecisionId }}</small>
        </label>
        <label class="fc-field" :class="{ 'fc-span2': form.decisionTypeCode === 'LISTING' }"><span>{{ tr('Letter') }}</span>
          <select v-model="form.letterId" class="fx-select" :disabled="!editable">
            <option :value="null">—</option>
            <option v-for="l in letters" :key="l.letterId" :value="l.letterId">{{ l.letterNo }} · {{ l.senderName }}</option>
          </select>
        </label>
        <label class="fc-field fc-full"><span>{{ tr('Scope') }} <b v-if="form.decisionTypeCode !== 'LIFTING'">*</b></span>
          <textarea v-model="form.scopeText" class="fx-search fc-textarea" :disabled="!editable"></textarea>
          <small v-if="errors.scopeText" class="scr-err">{{ errors.scopeText }}</small></label>
      </div>
    </section>

    <!-- the parties -->
    <section class="fx-card fc-step-card">
      <header class="fc-sec-head">
        <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-users"></span></span>
        <div>
          <h2>{{ tr('Parties') }} ({{ chosen.length }}) <b v-if="editable" class="scr-err">*</b></h2>
          <p v-if="form.decisionTypeCode !== 'LISTING'">{{ tr("Only the listing's parties can be chosen.") }}</p>
        </div>
      </header>
      <div v-if="editable" class="scr-toolbar">
        <select v-model="pick" class="fx-select fx-search">
          <option :value="null">{{ tr('Add a party...') }}</option>
          <option v-for="p in pickable" :key="p.partyId" :value="p.partyId">{{ p.nameAr }} · {{ p.identityNo }}</option>
        </select>
        <button type="button" class="fx-btn fx-btn--teal" :disabled="pick === null" @click="addParty"><span class="fa fa-plus" aria-hidden="true"></span>{{ tr('Add') }}</button>
        <RouterLink :to="{ name: 'scr-parties' }" class="fx-btn"><span class="fa fa-user-plus" aria-hidden="true"></span>{{ tr('New party') }}</RouterLink>
      </div>
      <small v-if="errors.partyIds" class="scr-err" style="display:block;padding:8px 16px 0">{{ errors.partyIds }}</small>
      <div class="fx-table-wrap">
        <table class="fx-table">
          <thead><tr><th class="fc-no">#</th><th>{{ tr('Name') }}</th><th>{{ tr('Identity number') }}</th><th>{{ tr('Nationality') }}</th><th v-if="editable" class="fx-r"></th></tr></thead>
          <tbody>
            <tr v-for="(p, i) in chosen" :key="p.partyId">
              <td class="fc-no">{{ i + 1 }}</td>
              <td><b>{{ p.nameAr }}</b><span v-if="p.nameEn" class="fx-cell-sub">{{ p.nameEn }}</span></td>
              <td dir="ltr">{{ p.identityNo }}<span class="fx-cell-sub">{{ tr(LABELS[p.identityTypeCode] ?? p.identityTypeCode) }}</span></td>
              <td>{{ p.nationalityCode }}</td>
              <td v-if="editable" class="fx-r">
                <button type="button" class="fx-btn fx-btn--sm fc-btn-reject" @click="form.partyIds = form.partyIds.filter((x) => x !== p.partyId)"><span class="fa fa-times" aria-hidden="true"></span>{{ tr('Remove') }}</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-if="!chosen.length" class="fx-empty"><span class="fa fa-users" aria-hidden="true"></span>{{ tr('No parties yet.') }}</div>
    </section>

    <!-- every step, completed ones view-only, with its documents -->
    <ScrDecisionSteps v-if="decision" :decision-id="decision.decisionId" :data-entry-open="editable" :inbox-id="taskOpen && !viewOnly ? task!.inboxId : null"
                      :task-step-code="task?.stepCode ?? null" :read-only="viewOnly" :focus-step="focusStep" :version="version" />

    <!-- who did what -->
    <section v-if="decision" class="fx-card fc-step-card">
      <header class="fc-sec-head">
        <span class="fc-sec-ic" aria-hidden="true"><span class="fa fa-history"></span></span>
        <div><h2>{{ tr('Approvals') }}</h2><p>{{ tr('Reviewer and approver are never the person who entered the decision.') }}</p></div>
      </header>
      <div class="fc-facts">
        <div class="fc-fact"><span class="fa fa-pencil" aria-hidden="true"></span><div><span>{{ tr('Entered by') }}</span><b>{{ decision.enteredBy }}</b><small>{{ formatDateTime(decision.createdAt) }}</small></div></div>
        <div class="fc-fact"><span class="fa fa-eye" aria-hidden="true"></span><div><span>{{ tr('Reviewed by') }}</span><b>{{ decision.reviewedBy ?? '—' }}</b><small>{{ decision.reviewedAt ? formatDateTime(decision.reviewedAt) : '' }}</small></div></div>
        <div class="fc-fact"><span class="fa fa-check-circle-o" aria-hidden="true"></span><div><span>{{ tr('Approved by') }}</span><b>{{ decision.approvedBy ?? '—' }}</b><small>{{ decision.publishedAt ? formatDateTime(decision.publishedAt) : '' }}</small></div></div>
        <div class="fc-fact"><span class="fa fa-list-alt" aria-hidden="true"></span><div><span>{{ tr('Final List version') }}</span><b>{{ decision.registryVersionNo ? `V${decision.registryVersionNo}` : '—' }}</b></div></div>
      </div>
    </section>

    <!-- data entry actions -->
    <section v-if="editable" class="fx-card fc-step-card">
      <div class="fc-form-actions" style="padding-top:16px">
        <div class="fc-note"><span class="fa fa-info-circle" aria-hidden="true"></span>
          <span>{{ tr('After saving, the decision waits in Decisions awaiting approval until its workflow is run.') }}</span></div>
        <button v-if="decision" type="button" class="fx-btn fc-btn-reject" :disabled="busy" @click="cancelDecision"><span class="fa fa-ban" aria-hidden="true"></span>{{ tr('Cancel decision') }}</button>
        <button type="button" class="fx-btn fx-btn--primary" :disabled="busy" @click="save"><span class="fa fa-floppy-o" aria-hidden="true"></span>{{ tr('Save') }}</button>
      </div>
    </section>

    <!-- the workflow step -->
    <section v-if="task && !viewOnly && task.status !== 'COMPLETED'" class="fx-card fc-complete">
      <label class="fc-field"><span>{{ tr('Comment') }} <small class="scr-hint">{{ tr('(required to return)') }}</small></span>
        <textarea v-model="comment" class="fx-search fc-textarea" :placeholder="task.stepCode === 'SCR_REVIEW'
          ? tr('Check the decision against its letter and parties. Accept sends it for approval; Return sends it back to data entry.')
          : tr('Approving publishes the decision to the Federation Final List as the next version. Return sends it back to data entry.')"></textarea>
        <small v-if="!taskOpen" class="scr-hint">{{ tr('Claim and start the task from the inbox first.') }}</small>
      </label>
      <button type="button" class="fx-btn fc-btn-reject" :disabled="busy || !taskOpen" @click="complete('REJECT')"><span class="fa fa-undo" aria-hidden="true"></span>{{ tr('Return to data entry') }}</button>
      <button type="button" class="fx-btn fx-btn--teal" :disabled="busy || !taskOpen" @click="complete('ACCEPT')">
        <span class="fa fa-check" aria-hidden="true"></span>{{ task.stepCode === 'SCR_REVIEW' ? tr('Accept review') : tr('Approve and publish') }}
      </button>
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
import { formatDateTime } from '../utils/dates'
import { tr } from '@/shared/layout/language'
import { workflowService } from '@/modules/wfe/services/workflowService'
import ScrDecisionSteps from '../components/ScrDecisionSteps.vue'
import type { WorkflowInboxTask } from '@/modules/wfe/models/workflow'
import { CHIP, LABELS, errorText, fieldErrors, scrApi, type ScrDecision, type ScrDecisionRequest, type ScrLetter, type ScrParty } from '../api/scr.api'

const props = defineProps<{ decisionId?: number }>()
const route = useRoute()
const router = useRouter()
const canEdit = hasPermission('SCR.DECISION.EDIT')
/** Opened from a completed step in a list (?view=1&step=CODE): everything view-only, that step highlighted. */
const viewOnly = computed(() => route.query.view === '1')
const focusStep = computed(() => (typeof route.query.step === 'string' ? route.query.step : null))
const version = ref(0)

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

const steps = computed(() => [
  { code: 'DRAFT', title: 'Data entry', who: decision.value?.enteredBy ?? '' },
  { code: 'IN_REVIEW', title: 'Review decision', who: decision.value?.reviewedBy ?? '' },
  { code: 'AWAITING_APPROVAL', title: 'Approve and publish', who: decision.value?.approvedBy ?? '' },
  { code: 'PUBLISHED', title: 'Published', who: decision.value?.registryVersionNo ? `V${decision.value.registryVersionNo}` : '' }
])
const stepIndex = computed(() => {
  const s = decision.value?.statusCode ?? 'DRAFT'
  if (s === 'PUBLISHED') return 4
  if (s === 'RETURNED' || s === 'CANCELLED') return 0
  return Math.max(0, steps.value.findIndex((x) => x.code === s))
})
const editable = computed(() => canEdit && !task.value && !viewOnly.value && (!decision.value || ['DRAFT', 'RETURNED'].includes(decision.value.statusCode)))
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
  version.value++
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
  showSuccess(`${tr('Decision saved')}: ${d.decisionRef}. ${tr('It is waiting in Decisions awaiting approval.')}`)
  await router.push({ name: 'scr-approvals' })
})

const cancelDecision = () => run(async () => {
  if (!window.confirm(tr('Cancel this decision? It will not be published.'))) return
  fill(await scrApi.cancel(decision.value!.decisionId))
})

const complete = (action: 'ACCEPT' | 'REJECT') => run(async () => {
  if (action === 'REJECT' && !comment.value.trim()) {
    errors.value = { comment: tr('Write why the decision is returned.') }
    showError(tr('Write why the decision is returned.'))
    return
  }
  await scrApi.completeStep(decision.value!.decisionId, task.value!.inboxId, action, comment.value.trim() || null)
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
