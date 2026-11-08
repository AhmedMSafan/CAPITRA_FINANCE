<template>
  <!-- Start a workflow from its start form (K2-style): the workflows whose start event has a form, then that form. -->
  <div class="fx" id="fxStart">
    <div class="fx-head">
      <div>
        <h1>{{ tr('Start a workflow') }}</h1>
        <p>{{ tr('Choose a workflow, fill in its form and start it. The first task goes to whoever the workflow names.') }}</p>
      </div>
      <div class="fx-actions">
        <RouterLink class="fx-btn" :to="{ name: 'workflow-inbox' }"><span class="fa fa-inbox" aria-hidden="true"></span>{{ tr('My Inbox') }}</RouterLink>
      </div>
    </div>

    <section v-if="message" class="wf-msg" :class="{ 'is-bad': message.bad }" role="status">
      <span class="fa" :class="message.bad ? 'fa-exclamation-circle' : 'fa-check-circle'" aria-hidden="true"></span>
      <span>{{ message.text }}</span>
      <RouterLink v-if="started?.inboxId" class="fx-btn fx-btn--sm" :to="{ name: 'workflow-task', params: { inboxId: started.inboxId } }">{{ tr('Open the task') }}</RouterLink>
      <button type="button" class="wf-x" :aria-label="tr('Close')" @click="message = null"><span class="fa fa-times" aria-hidden="true"></span></button>
    </section>

    <div class="st-grid">
      <div class="fx-card st-list">
        <button v-for="w in items" :key="w.workflowCode" type="button" class="st-row" :class="{ on: chosen?.workflowCode === w.workflowCode }" @click="choose(w)">
          <span class="st-icon fa fa-play-circle-o" aria-hidden="true"></span>
          <span class="st-text">
            <b>{{ w.workflowName }}</b>
            <small>{{ w.description || w.workflowCode }}</small>
          </span>
        </button>
        <div v-if="!items.length && !loading" class="fx-empty"><span class="fa fa-play-circle-o" aria-hidden="true"></span>{{ tr('No workflow has a start form yet. In the designer, give the Start event a form.') }}</div>
      </div>

      <div v-if="view" class="fx-card fx-card-pad st-form">
        <h2>{{ view.formName }}</h2>
        <p class="st-sub">{{ view.description || chosen?.workflowName }}</p>

        <label class="st-ref">
          <span>{{ tr('Business reference (optional)') }}</span>
          <input v-model="reference" class="fx-search" maxlength="100" :placeholder="tr('e.g. a contract or request number')">
        </label>

        <WorkflowFormView :view="view" v-model="values" :errors="errors" id-prefix="start" />

        <div class="st-actions">
          <button type="button" class="fx-btn" :disabled="busy" @click="reset">{{ tr('Clear') }}</button>
          <button type="button" class="fx-btn fx-btn--teal" :disabled="busy" @click="start">
            <span class="fa fa-play" aria-hidden="true"></span>{{ tr('Start workflow') }}
          </button>
        </div>
      </div>
      <div v-else class="fx-card fx-card-pad st-blank">
        <span class="fa fa-hand-o-left" aria-hidden="true"></span>
        <p>{{ tr('Choose a workflow to see its form.') }}</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import 'font-awesome/css/font-awesome.min.css'
import '@/shared/styles/apex-fx-inbox.css'
import { onMounted, ref } from 'vue'
import { tr } from '@/shared/layout/language'
import WorkflowFormView from '../components/WorkflowFormView.vue'
import { apiErrorText, apiFieldErrors, formApi, type FormView, type StartFormItem, type Started } from '../services/formService'

const items = ref<StartFormItem[]>([])
const chosen = ref<StartFormItem | null>(null)
const view = ref<FormView | null>(null)
const values = ref<Record<string, string | null>>({})
const errors = ref<Record<string, string>>({})
const reference = ref('')
const busy = ref(false)
const loading = ref(true)
const started = ref<Started | null>(null)
const message = ref<{ text: string; bad: boolean } | null>(null)

function initialValues(v: FormView) {
  return Object.fromEntries(v.controls.filter((c) => c.field).map((c) => [c.field as string, c.value]))
}

async function choose(w: StartFormItem) {
  chosen.value = w
  errors.value = {}
  try {
    view.value = await formApi.startForm(w.workflowCode)
    values.value = initialValues(view.value)
  } catch (e) {
    view.value = null
    message.value = { bad: true, text: apiErrorText(e) }
  }
}

function reset() {
  if (view.value) values.value = initialValues(view.value)
  reference.value = ''
  errors.value = {}
}

async function start() {
  if (!chosen.value) return
  busy.value = true
  errors.value = {}
  started.value = null
  try {
    const r = await formApi.start(chosen.value.workflowCode, reference.value.trim() || null, values.value)
    started.value = r
    message.value = {
      bad: false,
      text: `${tr('Started')} ${chosen.value.workflowName} (#${r.instanceId}).` +
        (r.nextStep ? ` ${tr('The next task,')} ${r.nextStep}, ${tr('went to')} ${r.nextAssignee ?? tr('the workflow\'s users')}.` : ` ${tr('It has no task waiting.')}`)
    }
    reset()
  } catch (e) {
    errors.value = apiFieldErrors(e)
    message.value = { bad: true, text: apiErrorText(e) }
  } finally {
    busy.value = false
  }
}

onMounted(async () => {
  try {
    items.value = await formApi.startList()
    const only = items.value.length === 1 ? items.value[0] : undefined
    if (only) await choose(only)
  } catch (e) {
    message.value = { bad: true, text: apiErrorText(e) }
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
.wf-msg { display: flex; align-items: center; gap: 10px; margin: 0 0 14px; padding: 11px 14px; border-radius: 10px; font-size: 13.5px; background: #e4f6ee; border: 1px solid #bfe5d2; color: #0e5a3c; }
.wf-msg.is-bad { background: #fdecea; border-color: #f3c3bc; color: #8a2416; }
.wf-msg > span:nth-child(2) { flex: 1; }
.wf-x { border: 0; background: none; color: inherit; cursor: pointer; }
.st-grid { display: grid; grid-template-columns: minmax(240px, 320px) minmax(0, 720px); gap: 16px; align-items: start; }
.st-row { display: flex; align-items: center; gap: 12px; width: 100%; padding: 12px 16px; border: 0; border-top: 1px solid #e6eef1; background: none; text-align: left; cursor: pointer; font: inherit; }
.st-row:first-child { border-top: 0; }
.st-row:hover { background: #f5f9fa; }
.st-row.on { background: #e7f3f2; box-shadow: inset 3px 0 0 #00636D; }
.st-icon { font-size: 22px; color: #00636D; }
.st-row.on .st-icon { color: #E87722; }
.st-text { display: grid; gap: 2px; min-width: 0; }
.st-text b { color: #12324a; font-size: 13.5px; }
.st-text small { color: #5f7787; font-size: 12px; }
.st-form { min-width: 0; }
.st-sub { margin: 0 0 16px; color: #5f7787; font-size: 12.5px; }
.st-ref { display: grid; gap: 5px; margin-bottom: 16px; padding-bottom: 16px; border-bottom: 1px solid #e6eef1; }
.st-ref > span { color: #12324a; font-size: 12px; font-weight: 700; }
.st-ref .fx-search { width: 100%; min-width: 0; max-width: 100%; flex: none; }
.st-actions { display: flex; justify-content: flex-end; gap: 8px; margin-top: 18px; }
.st-blank { display: grid; justify-items: center; gap: 6px; padding: 48px 18px; color: #5f7787; text-align: center; }
.st-blank .fa { font-size: 26px; color: #9fbfc4; }
@media (max-width: 900px) { .st-grid { grid-template-columns: minmax(0, 1fr); } }
</style>
