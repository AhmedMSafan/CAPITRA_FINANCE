<template>
  <div class="designer-page">

    <!-- FINOVA PAGE HEADER -->
    <section class="finova-page-head">
      <div class="page-head-copy">
        <div class="page-eyebrow">
          <span class="eyebrow-dot"></span>
          Capitra Workflow Engine
        </div>
        <h1>Workflow Designer</h1>
        <p>Design workflow steps, routing, outcomes and execution metadata.</p>
      </div>

      <div class="page-head-actions">
        <button class="btn btn-ghost" type="button" @click="openWorkflowManual(3)">
          <span class="pi pi-question-circle"></span>
          Help
        </button>

        <button class="btn btn-ghost" @click="router.push('/workflowengine/registry')">
          <span class="pi pi-arrow-left"></span>
          Workflows
        </button>

        <button class="btn btn-ghost" @click="fit">
          <span class="pi pi-expand"></span>
          Fit
        </button>

        <button v-if="form.dgrmId>0" class="btn btn-ghost" @click="openVersions">
          <span class="pi pi-history"></span>
          Versions
        </button>

        <button v-if="wfType==='SYSTEM'" class="btn btn-ghost" @click="openFields">
          <span class="pi pi-database"></span>
          Values from the module
        </button>

        <button class="btn btn-ghost" @click="showXml=true">
          <span class="pi pi-code"></span>
          BPMN XML
        </button>

        <button class="btn btn-primary" :disabled="saving||!!viewing" :title="viewing?'You are viewing an old version. Go back to the current workflow to save.':''" @click="save">
          <span :class="saving ? 'pi pi-spin pi-spinner' : 'pi pi-save'"></span>
          {{ saving ? 'Saving...' : 'Save Workflow' }}
        </button>
      </div>
    </section>

    <!-- WORKFLOW IDENTITY -->
    <section class="workflow-summary-card">
      <div class="summary-field code-field">
        <label>Workflow Code</label>
        <input v-model="form.workflowCode" placeholder="WORKFLOW_CODE" />
      </div>

      <div class="summary-field name-field">
        <label>Workflow Name</label>
        <input v-model="form.dgrmName" placeholder="Workflow name" />
      </div>

      <div class="summary-field version-field">
        <label>Version</label>
        <input v-if="!versionsKept" v-model="form.dgrmVersion" placeholder="1" />
        <button v-else class="version-chip" type="button" title="Versions are kept automatically. Click to see them." @click="openVersions">
          v{{ form.dgrmVersion }}
        </button>
      </div>

      <div class="summary-field status-field">
        <label>Status</label>
        <select v-model="form.dgrmStatus">
          <option value="draft">Draft</option>
          <option value="released">Released</option>
          <option value="deprecated">Deprecated</option>
        </select>
      </div>

      <label class="active-switch">
        <input v-model="active" type="checkbox" />
        <span class="switch-track"><span class="switch-knob"></span></span>
        <span class="switch-copy">
          <strong>Active</strong>
          <small>Workflow can be started</small>
        </span>
      </label>
    </section>

    <!-- VIEWING AN OLD VERSION -->
    <section v-if="viewing" class="version-banner">
      <span class="pi pi-eye"></span>
      <span>
        You are viewing <b>version {{ viewing.versionNo }}</b>, released {{ fmtDate(viewing.releasedAt) }} by {{ viewing.releasedBy }}.
        {{ viewing.running ? (viewing.running===1 ? '1 running instance still follows it.' : `${viewing.running} running instances still follow it.`) : 'No instance is running on it.' }}
        Changes here are not saved.
      </span>
      <button class="btn btn-primary" @click="backToCurrent">
        <span class="pi pi-arrow-left"></span>
        Back to current workflow
      </button>
    </section>

    <!-- DESIGNER WORKSPACE -->
    <section
      class="designer-shell"
      :class="{ 'properties-collapsed': !rightPanelOpen }"
    >

      <!-- CANVAS -->
      <div class="canvas-panel">
        <div class="canvas-toolbar">
          <div class="canvas-title">
            <span class="canvas-title-icon pi pi-share-alt"></span>
            <div>
              <strong>Process Canvas</strong>
              <small>Drag BPMN elements and connect the business flow.</small>
            </div>
          </div>

          <div class="canvas-toolbar-actions">
            <button
              class="tool-btn tool-btn-wide"
              :class="{ active: leftPanelOpen }"
              :title="leftPanelOpen ? 'Hide BPMN toolbox' : 'Show BPMN toolbox'"
              @click="toggleLeftPanel"
            >
              <span class="pi pi-th-large"></span>
              <span>Tools</span>
            </button>

            <button
              class="tool-btn tool-btn-wide"
              :class="{ active: rightPanelOpen }"
              :title="rightPanelOpen ? 'Hide properties' : 'Show properties'"
              @click="toggleRightPanel"
            >
              <span class="pi pi-sliders-h"></span>
              <span>Properties</span>
            </button>

            <div class="zoom-control" aria-label="Diagram zoom controls">
              <button
                class="zoom-button"
                type="button"
                title="Zoom out"
                @click="zoomOut"
              >
                <span class="pi pi-minus"></span>
              </button>

              <button
                class="zoom-value"
                type="button"
                title="Reset zoom to 100%"
                @click="setZoom(100)"
              >
                {{ zoomPercent }}%
              </button>

              <button
                class="zoom-button"
                type="button"
                title="Zoom in"
                @click="zoomIn"
              >
                <span class="pi pi-plus"></span>
              </button>
            </div>

            <button class="tool-btn" title="Fit diagram" @click="fit">
              <span class="pi pi-expand"></span>
            </button>

            <button class="tool-btn" title="Clear selection" @click="clearSelection">
              <span class="pi pi-times"></span>
            </button>
          </div>
        </div>

        <div
          class="canvas-wrap"
          :class="{ 'toolbox-hidden': !leftPanelOpen }"
        >
          <div class="canvas-scroll-content">
            <div ref="canvas" class="canvas"></div>
          </div>
        </div>
      </div>

      <!-- PROPERTIES -->
      <aside v-show="rightPanelOpen" class="properties-panel">
        <div class="properties-header">
          <div>
            <span class="properties-kicker">
              {{ !selectedId ? 'Workflow' : selectedType === 'bpmn:SequenceFlow' ? 'Transition' : 'Element' }}
            </span>
            <h2>
              {{
                !selectedId
                  ? 'Workflow Properties'
                  : selectedType === 'bpmn:SequenceFlow'
                    ? 'Transition Properties'
                    : 'Element Properties'
              }}
            </h2>
          </div>

          <button
            v-if="selectedId"
            class="panel-close"
            title="Back to workflow properties"
            @click="clearSelection"
          >
            <span class="pi pi-times"></span>
          </button>
        </div>

        <div class="properties-body">

          <!-- WORKFLOW PROPERTIES -->
          <template v-if="!selectedId">
            <section class="property-section">
              <div class="section-title">
                <span class="section-icon pi pi-align-left"></span>
                <div>
                  <strong>Description</strong>
                  <small>Explain the business purpose of this workflow.</small>
                </div>
              </div>

              <textarea
                v-model="form.description"
                class="large-textarea"
                rows="4"
                placeholder="Workflow description..."
              ></textarea>
            </section>

            <section class="property-section">
              <div class="section-title">
                <span class="section-icon pi pi-sitemap"></span>
                <div>
                  <strong>Workflow Type</strong>
                  <small>Who starts it, and where its document is linked.</small>
                </div>
              </div>

              <div class="wf-type" role="radiogroup" aria-label="Workflow type">
                <label :class="{on:wfType==='SYSTEM'}">
                  <input v-model="wfType" type="radio" value="SYSTEM" />
                  <span><b>System-built</b><small>Started by the system (factoring, portfolios). Any step can link its own document.</small></span>
                </label>
                <label :class="{on:wfType==='USER'}">
                  <input v-model="wfType" type="radio" value="USER" />
                  <span><b>User-defined</b><small>Started by a user from Start a request, on one document for the whole workflow.</small></span>
                </label>
              </div>

              <template v-if="wfType==='USER'">
                <label class="field-label">Document</label>
                <select v-model="wfDocument">
                  <option value="">- Choose the document -</option>
                  <option v-if="wfDocument && !documents.some(x=>x.formCode===wfDocument)" :value="wfDocument">{{ wfDocument }} (not found)</option>
                  <option v-for="x in documents" :key="x.formId" :value="x.formCode">{{ x.formName }} ({{ x.formCode }}){{ x.isActive ? '' : ' - off' }}</option>
                </select>
                <small class="field-hint">The requester fills it in to start. Every approval step shows it read-only; approvers approve or reject with a comment.</small>
                <button class="text-button form-manage" type="button" @click="openDocument(wfDocument)">
                  <span class="pi pi-external-link"></span>
                  {{ wfDocument ? 'Open this document' : 'Design a document' }}
                </button>
              </template>
            </section>

            <section v-if="wfType==='USER'" class="property-section">
              <div class="section-title">
                <span class="section-icon pi pi-database"></span>
                <div>
                  <strong>Fields</strong>
                  <small>A user-defined workflow uses its document's fields: in the request, on every step and in rules on paths.</small>
                </div>
              </div>

              <div v-if="!wfDocument" class="fields-empty">Choose the document above to see its fields.</div>
              <div v-else-if="!docFields.length" class="fields-empty">{{ wfDocument }} has no fields yet. Add them in the Document Designer.</div>
              <ul v-else class="field-chips">
                <li v-for="f in docFields" :key="f.name">
                  <b>{{ f.label || f.name }}</b>
                  <span>{{ typeLabel(f.type) }}<template v-if="f.required"> · required</template></span>
                </li>
              </ul>

              <div v-if="docFields.length && ownExtra.length" class="legacy-fields">
                <span class="pi pi-exclamation-triangle"></span>
                <div>
                  <b>This workflow still has its own data fields:</b> {{ ownExtra.join(', ') }}.
                  They are not on the document, so nobody can fill them in. Remove them, then save.
                  <button class="text-button" type="button" @click="removeOwnFields">Remove them</button>
                </div>
              </div>
            </section>

            <section v-else class="property-section">
              <div class="section-title">
                <span class="section-icon pi pi-database"></span>
                <div>
                  <strong>Values from the module</strong>
                  <small>What the module sends when it starts this workflow (client, amount…). Shown on each task and in the Process monitor, and used by rules on paths.</small>
                </div>
              </div>

              <div v-if="!variables.some(x=>x?.name)" class="fields-empty">None yet.</div>
              <ul v-else class="field-chips">
                <li v-for="(v,i) in variables.filter(x=>x?.name)" :key="i">
                  <b>{{ v.label || v.name }}</b>
                  <span>{{ typeLabel(v.type) }}</span>
                </li>
              </ul>

              <button class="btn btn-secondary btn-full" @click="openFields">
                <span class="pi pi-pencil"></span>
                Edit values from the module
              </button>
            </section>

            <div class="info-callout">
              <span class="pi pi-info-circle"></span>
              <span>
                Select a task, gateway, event or arrow to maintain its execution metadata.
                Sequence-flow arrows remain the source of next-step routing.
              </span>
            </div>
          </template>

          <!-- TRANSITION PROPERTIES -->
          <template v-else-if="selectedType === 'bpmn:SequenceFlow'">
            <section class="selected-element-card transition-card">
              <div class="selected-icon">
                <span class="pi pi-arrow-right"></span>
              </div>

              <div class="selected-copy">
                <small>Selected Transition</small>
                <strong>{{ selectedId }}</strong>
              </div>
            </section>

            <div class="flow-info">
              <div>
                <span>From</span>
                <strong>{{ selectedFlowSource }}</strong>
              </div>

              <span class="flow-arrow pi pi-arrow-right"></span>

              <div>
                <span>To</span>
                <strong>{{ selectedFlowTarget }}</strong>
              </div>
            </div>

            <section class="property-section">
              <div class="section-title">
                <span class="section-icon outcome-icon pi pi-directions-alt"></span>
                <div>
                  <strong>Outcome / Action</strong>
                  <small>Controls which transition is used after task completion.</small>
                </div>
              </div>

              <label class="field-label">Configured Outcome</label>
              <select v-model="flowOutcome" @change="syncFlowOutcome">
                <option value="">Default / Unconditional</option>
                <option value="ACCEPT">ACCEPT</option>
                <option value="APPROVE">APPROVE</option>
                <option value="REJECT">REJECT</option>
                <option value="RETURN">RETURN</option>
                <option value="COMPLETE">COMPLETE</option>
              </select>

              <label class="field-label">Custom Outcome</label>
              <div class="inline-action">
                <input
                  v-model="customFlowOutcome"
                  placeholder="e.g. CNTAPPROVE"
                  @keyup.enter="applyCustomFlowOutcome"
                />

                <button class="btn btn-secondary" @click="applyCustomFlowOutcome">
                  Apply
                </button>
              </div>
            </section>

            <section class="property-section">
              <div class="section-title">
                <span class="section-icon outcome-icon pi pi-filter"></span>
                <div>
                  <strong>Conditions (rules on data fields)</strong>
                  <small>Take this path only when the request's data matches, for example Amount &gt; 1,000,000.</small>
                </div>
              </div>

              <div v-if="!ruleFields.length" class="fields-empty">{{ wfType==='USER' ? 'Choose the document and give it fields first.' : 'Add values from the module first.' }}</div>
              <div v-for="(r,i) in flowRules" :key="i" class="rule-row">
                <span class="rule-join">{{ i===0 ? 'If' : 'and' }}</span>
                <select v-model="r.field" class="rule-field" aria-label="Data field" @change="syncFlowRules">
                  <option value="" disabled>Field...</option>
                  <option v-if="r.field && !ruleFields.some(f=>f.name.toUpperCase()===r.field.toUpperCase())" :value="r.field">{{ r.field }} (not found)</option>
                  <option v-for="f in ruleFields" :key="f.name" :value="f.name">{{ f.label||f.name }}</option>
                </select>
                <button class="icon-button" type="button" title="Remove this condition" @click="removeFlowRule(i)"><span class="pi pi-times"></span></button>
                <select v-model="r.op" class="rule-op" aria-label="Test" @change="syncFlowRules">
                  <option v-for="o in ruleOps(r.field)" :key="o.value" :value="o.value">{{ o.label }}</option>
                </select>
                <template v-if="r.op!=='empty'&&r.op!=='notEmpty'">
                  <select v-if="ruleChoices(r.field).length" v-model="r.value" class="rule-value" aria-label="Value" @change="syncFlowRules">
                    <option value="" disabled>Value...</option>
                    <option v-for="c in ruleChoices(r.field)" :key="c" :value="c">{{ c }}</option>
                  </select>
                  <input v-else v-model="r.value" class="rule-value" :type="ruleType(r.field)==='Date'?'date':'text'" :inputmode="isNumberField(r.field)?'decimal':undefined" placeholder="Value" aria-label="Value" @change="syncFlowRules" />
                </template>
              </div>
              <button v-if="ruleFields.length" class="btn btn-secondary btn-full" type="button" @click="addFlowRule">
                <span class="pi pi-plus"></span>
                Add condition
              </button>
              <p class="rule-hint">{{ flowRules.length ? 'All conditions must match. Draw another path with no conditions for all other cases.' : 'No conditions: this path takes every case the other paths do not.' }}</p>
            </section>

            <section class="parallel-card">
              <div class="parallel-icon">
                <span class="pi pi-clone"></span>
              </div>

              <div class="parallel-copy">
                <strong>Parallel Outcome</strong>
                <span>
                  Create another flow from the same source to the same target.
                  Useful for ACCEPT → End and REJECT → End.
                </span>
              </div>

              <button
                class="btn btn-primary btn-full"
                @click="addParallelOutcomeFlow"
              >
                <span class="pi pi-plus"></span>
                Add Parallel Outcome Flow
              </button>

              <button
                class="btn btn-secondary btn-full repair-flow-btn"
                @click="repairSelectedFlow"
              >
                <span class="pi pi-wrench"></span>
                Repair Selected Connection
              </button>
            </section>

            <div class="info-callout">
              <span class="pi pi-info-circle"></span>
              <span>
                The outcome is stored as the BPMN sequence-flow name and matched by
                the workflow runtime when ACCEPT, REJECT, APPROVE, RETURN or another
                action is submitted.
              </span>
            </div>
          </template>

          <!-- ELEMENT PROPERTIES -->
          <template v-else>
            <section class="selected-element-card">
              <div class="selected-icon">
                <span class="pi pi-sliders-h"></span>
              </div>

              <div class="selected-copy">
                <small>Selected BPMN Element</small>
                <strong>{{ selectedId }}</strong>
              </div>
            </section>

            <section class="property-section">
              <div class="section-title">
                <span class="section-icon pi pi-pencil"></span>
                <div>
                  <strong>Step Definition</strong>
                  <small>Business identity and execution type.</small>
                </div>
              </div>

              <label class="field-label">Step Code</label>
              <input v-model="step.stepCode" placeholder="STEP_CODE" />

              <label class="field-label">Step Name</label>
              <input
                v-model="step.stepName"
                placeholder="Step name"
                @change="syncStepName"
              />

              <div class="two-col">
                <div>
                  <label class="field-label">Step Type</label>
                  <select v-model="step.stepType">
                    <option>HumanTask</option>
                    <option>SystemTask</option>
                    <option>Decision</option>
                    <option>Start</option>
                    <option>End</option>
                  </select>
                </div>

                <div>
                  <label class="field-label">Assignment Type</label>
                  <select v-model="step.assignmentType">
                    <option>Role</option>
                    <option>User</option>
                    <option>Variable</option>
                    <option>None</option>
                  </select>
                </div>
              </div>

              <label class="field-label">{{ step.parallelMode && step.assignmentType==='User' ? 'Approvers (user names, separated by commas)' : 'Assignee / Role / Variable' }}</label>
              <input v-model="step.assignee" :placeholder="step.parallelMode && step.assignmentType==='User' ? 'e.g. mona, omar, ahmed' : 'e.g. CREDIT_MANAGER'" />
            </section>

            <section v-if="step.stepType==='HumanTask'" class="property-section parallel-section">
              <div class="section-title">
                <span class="section-icon pi pi-users"></span>
                <div>
                  <strong>Parallel approval</strong>
                  <small>Send the task to several people at the same time, and move on when enough of them approve.</small>
                </div>
              </div>

              <label class="field-label">Who approves</label>
              <select v-model="step.parallelMode">
                <option value="">One person (the usual way)</option>
                <option value="ALL">Everyone must approve</option>
                <option value="ANY">Any one approval is enough</option>
                <option value="COUNT">A number of approvals</option>
              </select>

              <template v-if="step.parallelMode">
                <div v-if="step.parallelMode==='COUNT'">
                  <label class="field-label">Approvals needed</label>
                  <input v-model="step.parallelCount" type="number" min="1" step="1" placeholder="e.g. 2" />
                </div>
                <p v-if="step.assignmentType!=='User'&&step.assignmentType!=='Role'" class="rule-hint warn">Set Assignment Type to User (list the approvers) or Role (every member approves).</p>
                <p v-else class="rule-hint">{{ parallelSummary(step) }}</p>
              </template>
            </section>

            <section v-if="step.stepType==='HumanTask'" class="property-section sla-section">
              <div class="section-title">
                <span class="section-icon pi pi-clock"></span>
                <div>
                  <strong>Deadline (SLA)</strong>
                  <small>Optional. How long the assignee has, a reminder before the due time, and who it goes to when overdue.</small>
                </div>
              </div>

              <div class="two-col">
                <div>
                  <label class="field-label">Time limit</label>
                  <input v-model="step.slaDuration" type="number" min="0" step="any" placeholder="e.g. 8" />
                </div>
                <div>
                  <label class="field-label">Unit</label>
                  <select v-model="step.slaUnit">
                    <option value="HOURS">Hours</option>
                    <option value="DAYS">Days</option>
                    <option value="WORKING_DAYS">Working days</option>
                  </select>
                </div>
              </div>

              <template v-if="String(step.slaDuration).trim()">
                <label class="field-label">Remind the assignee (hours before due)</label>
                <input v-model="step.slaRemindHours" type="number" min="0" step="any" placeholder="e.g. 2" />

                <div class="two-col">
                  <div>
                    <label class="field-label">When overdue</label>
                    <select v-model="step.slaEscalation">
                      <option value="NONE">Alert the assignee only</option>
                      <option value="REASSIGN">Move the task (escalate)</option>
                      <option value="NOTIFY">Tell someone else</option>
                    </select>
                  </div>
                  <div v-if="step.slaEscalation!=='NONE'">
                    <label class="field-label">After (hours)</label>
                    <input v-model="step.slaEscalateHours" type="number" min="0" step="any" placeholder="0" />
                  </div>
                </div>

                <div v-if="step.slaEscalation!=='NONE'" class="two-col">
                  <div>
                    <label class="field-label">To a</label>
                    <select v-model="step.slaEscalateType">
                      <option value="Role">Role</option>
                      <option value="User">User</option>
                    </select>
                  </div>
                  <div>
                    <label class="field-label">{{ step.slaEscalateType==='User'?'User name':'Role' }}</label>
                    <input v-model="step.slaEscalateTo" :placeholder="step.slaEscalateType==='User'?'e.g. mona':'e.g. CREDIT_MANAGER'" />
                  </div>
                </div>
              </template>
              <p class="sla-summary"><span class="pi pi-info-circle"></span> {{ slaSummary(step) }}</p>
            </section>

            <section class="property-section">
              <div class="section-title">
                <span class="section-icon pi pi-book"></span>
                <div>
                  <strong>Product Event Rule</strong>
                  <small>The product rule posted when the step completes: it gives the charges and GL entries.</small>
                </div>
              </div>

              <label class="field-label">Product Event Rule</label>
              <select v-if="productRules.length" v-model="step.ruleCode">
                <option value="">- None -</option>
                <option v-if="step.ruleCode && !productRules.some(x=>x.ruleCode===step.ruleCode)" :value="step.ruleCode">{{ step.ruleCode }}</option>
                <option v-for="x in productRules" :key="x.productEventRuleId" :value="x.ruleCode">{{ x.productCode }} · {{ x.ruleCode }} - {{ x.ruleNameEn }}</option>
              </select>
              <input v-else v-model="step.ruleCode" placeholder="e.g. PP_DISBURSE" />
            </section>

            <section v-if="wfType==='USER'&&(step.stepType==='HumanTask'||step.stepType==='Start')" class="property-section">
              <div class="section-title">
                <span class="section-icon pi pi-file-edit"></span>
                <div>
                  <strong>Document</strong>
                  <small>This is a user-defined workflow: {{ step.stepType==='Start' ? 'the requester starts it on' : 'this step shows, read-only,' }} the workflow's document{{ wfDocument ? ` (${wfDocument})` : '' }}. Change it in Workflow Properties.</small>
                </div>
              </div>
            </section>

            <section v-else-if="step.stepType==='HumanTask'||(step.stepType==='Start'&&step.formCode)" class="property-section">
              <div class="section-title">
                <span class="section-icon pi pi-file-edit"></span>
                <div>
                  <strong>Document</strong>
                  <small v-if="step.stepType==='Start'">An older start form. A system-built workflow is started by its module; make the workflow user-defined to start it from Start a request.</small>
                  <small v-else>Optional. Without a business screen the task is worked on the document; with one it shows read-only.</small>
                </div>
              </div>

              <label class="field-label">Document</label>
              <select v-model="step.formCode">
                <option value="">- None -</option>
                <option v-if="step.formCode && !documents.some(x=>x.formCode===step.formCode)" :value="step.formCode">{{ step.formCode }} (not found)</option>
                <option v-for="x in documents" :key="x.formId" :value="x.formCode">{{ x.formName }} ({{ x.formCode }}){{ x.isActive ? '' : ' - off' }}</option>
              </select>
              <button class="text-button form-manage" type="button" @click="openDocument(step.formCode)">
                <span class="pi pi-external-link"></span>
                {{ step.formCode ? 'Open this document' : 'Design a document' }}
              </button>
            </section>

            <section class="property-section">
              <div class="section-title">
                <span class="section-icon pi pi-external-link"></span>
                <div>
                  <strong>Navigation / Route</strong>
                  <small>Business screen opened when the task runs.</small>
                </div>
              </div>

              <label class="field-label">Route / Page</label>
              <input v-model="step.route" placeholder="e.g. factoring-credit-analysis" />

              <div class="route-title-row">
                <label class="field-label no-margin">Route Parameters</label>

                <button
                  class="text-button"
                  @click="step.routeParameters.push({name:'',value:''})"
                >
                  <span class="pi pi-plus"></span>
                  Add
                </button>
              </div>

              <div
                v-for="(p,i) in step.routeParameters"
                :key="i"
                class="param-row"
              >
                <input v-model="p.name" placeholder="Parameter" />
                <input v-model="p.value" placeholder="Variable / value" />

                <button
                  class="delete-row"
                  title="Remove parameter"
                  @click="step.routeParameters.splice(i,1)"
                >
                  <span class="pi pi-trash"></span>
                </button>
              </div>
            </section>

            <section class="property-section">
              <div class="section-title">
                <span class="section-icon pi pi-check-square"></span>
                <div>
                  <strong>Allowed Actions</strong>
                  <small>Buttons available when this human task is in progress.</small>
                </div>
              </div>

              <div class="action-grid">
                <label
                  v-for="a in actionCatalog"
                  :key="a"
                  class="action-check"
                  :class="{ selected: step.actions.includes(a) }"
                >
                  <input v-model="step.actions" type="checkbox" :value="a" />
                  <span class="action-box">
                    <span class="action-dot"></span>
                    {{ a }}
                  </span>
                </label>
              </div>

              <div class="inline-action custom-outcome">
                <input v-model="customAction" placeholder="Custom action" />
                <button class="btn btn-secondary" @click="addCustomAction">
                  Add
                </button>
              </div>
            </section>
          </template>
        </div>
      </aside>
    </section>

    <!-- XML MODAL -->
    <div v-if="showXml" class="modal">
      <div class="modal-card">
        <div class="modal-head">
          <div>
            <small>BPMN Definition</small>
            <h3>Workflow XML</h3>
          </div>

          <button class="modal-close" @click="showXml=false">
            <span class="pi pi-times"></span>
          </button>
        </div>

        <textarea readonly :value="xmlPreview"></textarea>
      </div>
    </div>

    <!-- DATA FIELDS MODAL -->
    <div v-if="showFields" class="modal" @click.self="showFields=false">
      <div class="modal-card fields-card">
        <div class="modal-head">
          <div>
            <small>Values from the module</small>
            <h3>What the module sends when it starts {{ form.dgrmName || 'this workflow' }}</h3>
          </div>

          <button class="modal-close" @click="showFields=false">
            <span class="pi pi-times"></span>
          </button>
        </div>

        <div class="fields-body">
          <p class="versions-note">
            The module fills these in when it starts the workflow; nobody types them. They are shown in <b>Workflow data</b> on each task
            and in the Process monitor, and rules on paths can read them (for example amount &gt; 5000000). The <b>Name</b> must be the one the module sends.
          </p>
          <div class="module-values">
            <span>Add what the module sends:</span>
            <button v-for="m in moduleValues.filter(m=>!fieldRows.some(f=>f.name.toLowerCase()===m.name.toLowerCase()))" :key="m.name" type="button" class="chip-add" :title="m.from" @click="fieldRows.push({name:m.name,label:m.label,type:m.type,required:false,defaultValue:'',choicesText:''})">
              <span class="pi pi-plus"></span>{{ m.label }}
            </button>
          </div>

          <div class="fields-table-wrap">
            <table class="fields-table">
              <thead>
                <tr><th>Name</th><th>Label</th><th>Type</th><th>Choices</th><th></th></tr>
              </thead>
              <tbody>
                <tr v-for="(f,i) in fieldRows" :key="i" :class="{ bad: fieldErrors[i] }">
                  <td><input v-model="f.name" placeholder="amount" /></td>
                  <td><input v-model="f.label" placeholder="What people see" /></td>
                  <td>
                    <select v-model="f.type">
                      <option v-for="t in fieldTypes" :key="t.value" :value="t.value">{{ t.label }}</option>
                    </select>
                  </td>
                  <td>
                    <input v-if="f.type==='Choice'" v-model="f.choicesText" placeholder="LOW, MEDIUM, HIGH" />
                    <span v-else class="muted">-</span>
                  </td>
                  <td class="c">
                    <button class="delete-row" title="Remove field" @click="fieldRows.splice(i,1)">
                      <span class="pi pi-trash"></span>
                    </button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>

          <ul v-if="fieldProblems.length" class="field-problems">
            <li v-for="p in fieldProblems" :key="p"><span class="pi pi-exclamation-circle"></span> {{ p }}</li>
          </ul>

          <div class="fields-actions">
            <button class="btn btn-secondary" @click="fieldRows.push({name:'',label:'',type:'String',required:false,defaultValue:'',choicesText:''})">
              <span class="pi pi-plus"></span>
              Add another value
            </button>
            <span class="spacer"></span>
            <button class="btn btn-ghost" @click="showFields=false">Cancel</button>
            <button class="btn btn-primary" :disabled="fieldProblems.length>0" @click="applyFields">
              <span class="pi pi-check"></span>
              Apply
            </button>
          </div>
          <p class="fields-foot">Apply keeps the values on the diagram. Click <b>Save Workflow</b> to save them.</p>
        </div>
      </div>
    </div>

    <!-- VERSIONS MODAL -->
    <div v-if="showVersions" class="modal" @click.self="showVersions=false">
      <div class="modal-card versions-card">
        <div class="modal-head">
          <div>
            <small>Released versions</small>
            <h3>{{ form.dgrmName || form.workflowCode }}</h3>
          </div>

          <button class="modal-close" @click="showVersions=false">
            <span class="pi pi-times"></span>
          </button>
        </div>

        <div class="versions-body">
          <p class="versions-note">
            Each time you save a <b>Released</b> workflow with changed steps or routing, a new version is kept.
            New instances start on the latest version. Instances already running stay on the version they started with,
            so your change never moves work in progress. Moving shapes only does not make a new version.
          </p>

          <div v-if="versionsLoading" class="versions-empty"><span class="pi pi-spin pi-spinner"></span> Loading...</div>
          <div v-else-if="!versions.length" class="versions-empty">No version is kept yet. Save the workflow as Released to keep version 1.</div>
          <table v-else class="versions-table">
            <thead>
              <tr><th>Version</th><th>Released</th><th>By</th><th class="num">Running</th><th class="num">All instances</th><th></th></tr>
            </thead>
            <tbody>
              <tr v-for="v in versions" :key="v.definitionVersionId">
                <td><b>v{{ v.versionNo }}</b> <span v-if="v.isCurrent" class="current-tag">Current</span></td>
                <td>{{ fmtDate(v.releasedAt) }}</td>
                <td>{{ v.releasedBy }}</td>
                <td class="num">{{ v.running }}</td>
                <td class="num">{{ v.total }}</td>
                <td class="act">
                  <button class="btn btn-ghost btn-sm" @click="viewVersion(v)">
                    <span class="pi pi-eye"></span> View
                  </button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <div
      v-if="message"
      :class="['toast',messageType==='error'?'toast-error':'toast-success']"
    >
      <span :class="messageType==='error' ? 'pi pi-exclamation-circle' : 'pi pi-check-circle'"></span>
      {{ message }}
    </div>
  </div>
</template>
<script setup lang="ts">
/* eslint-disable @typescript-eslint/no-explicit-any -- bpmn-js and its moddle objects are untyped */
import { computed,nextTick,onBeforeUnmount,onMounted,reactive,ref } from 'vue'
import { useRoute,useRouter } from 'vue-router'
import BpmnModeler from 'bpmn-js/lib/Modeler'
import 'bpmn-js/dist/assets/diagram-js.css'; import 'bpmn-js/dist/assets/bpmn-font/css/bpmn.css'
import finovaWorkflowModdle from '../models/finovaWorkflowModel'
import {createWorkflowDefinition,getWorkflowDefinition,getWorkflowVersions,getWorkflowVersionXml,updateWorkflowDefinition,type WorkflowVersion} from '../services/workflowDefinitionService'
import {productEventRulesApi,type ProductEventRule} from '@/modules/adm/api/products.api'
import {apiErrorText,formApi,type WorkflowForm} from '../services/formService'
import { openWorkflowManual } from '../utils/manual'

type Var={name:string,type:string,required:boolean,label?:string,defaultValue?:string,choices?:string[],description?:string}; type Param={name:string,value:string};
type Step={stepCode:string,stepName:string,ruleCode:string,formCode:string,stepType:string,assignmentType:string,assignee:string,route:string,routeParameters:Param[],actions:string[],slaDuration:string,slaUnit:string,slaRemindHours:string,slaEscalateHours:string,slaEscalation:string,slaEscalateType:string,slaEscalateTo:string,parallelMode:string,parallelCount:string}
const route=useRoute(),router=useRouter(),canvas=ref<HTMLElement|null>(null),saving=ref(false),active=ref(true),selectedId=ref(''),selectedType=ref(''),message=ref(''),messageType=ref<'success'|'error'>('success'),showXml=ref(false),xmlPreview=ref(''),customAction=ref(''),flowOutcome=ref(''),customFlowOutcome=ref(''),selectedFlowSource=ref(''),selectedFlowTarget=ref(''),leftPanelOpen=ref(false),rightPanelOpen=ref(false),zoomPercent=ref(100)
type FlowRule={field:string,op:string,value:string}
const flowRules=ref<FlowRule[]>([])
const variables=reactive<Var[]>([]),steps=reactive<Record<string,Step>>({}); let modeler:any=null
const form=reactive({dgrmId:0,dgrmName:'',dgrmVersion:'1',dgrmStatus:'draft',dgrmContent:'',dgrmCreatedBy:'SYSTEM',dgrmLastUpdateBy:'SYSTEM',workflowCode:'',description:'',isActive:'Y'})
// active product event rules for the Product Event Rule drop-down; without access to them the field stays free text
const productRules=ref<ProductEventRule[]>([])
productEventRulesApi.list().then(x=>productRules.value=x.filter(r=>r.isActive)).catch(()=>{})
// the documents (Document Designer) a step or the whole workflow can link, with this workflow's older forms
const documents=ref<WorkflowForm[]>([])
// the workflow's type (finova:workflowType) and, for a user-defined one, its document (finova:documentCode)
const wfType=ref<'SYSTEM'|'USER'>('SYSTEM'),wfDocument=ref('')
function loadForms(){(form.dgrmId?formApi.ofWorkflow(form.dgrmId):formApi.list().then(x=>x.filter(f=>f.isDocument))).then(x=>documents.value=x).catch(()=>documents.value=[])}
function openDocument(code:string){const f=documents.value.find(x=>x.formCode===code);router.push({name:'workflow-forms',query:f?{form:String(f.formId)}:{new:'1'}})}
const actionCatalog=ref(['CLAIM','ACCEPT','APPROVE','REJECT','RETURN','COMPLETE'])
function emptyStep():Step{return{stepCode:'',stepName:'',ruleCode:'',formCode:'',stepType:'HumanTask',assignmentType:'Role',assignee:'',route:'',routeParameters:[],actions:['CLAIM','COMPLETE'],slaDuration:'',slaUnit:'HOURS',slaRemindHours:'',slaEscalateHours:'',slaEscalation:'NONE',slaEscalateType:'Role',slaEscalateTo:'',parallelMode:'',parallelCount:''}}
// selecting an element creates its step settings (bindSelection), so the computed only reads them
const step=computed<Step>(()=>(selectedId.value?steps[selectedId.value]:undefined)??emptyStep())
const starter=`<?xml version="1.0" encoding="UTF-8"?><bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_FINOVA" targetNamespace="http://finova/workflow"><bpmn:process id="Process_FINOVA" name="FINOVA Workflow Engine" isExecutable="true"><bpmn:startEvent id="StartEvent_1" name="Start"><bpmn:outgoing>Flow_1</bpmn:outgoing></bpmn:startEvent><bpmn:userTask id="Task_1" name="Approval Task"><bpmn:incoming>Flow_1</bpmn:incoming><bpmn:outgoing>Flow_2</bpmn:outgoing></bpmn:userTask><bpmn:endEvent id="EndEvent_1" name="End"><bpmn:incoming>Flow_2</bpmn:incoming></bpmn:endEvent><bpmn:sequenceFlow id="Flow_1" sourceRef="StartEvent_1" targetRef="Task_1"/><bpmn:sequenceFlow id="Flow_2" sourceRef="Task_1" targetRef="EndEvent_1"/></bpmn:process><bpmndi:BPMNDiagram id="BPMNDiagram_1"><bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_FINOVA"><bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1"><dc:Bounds x="180" y="182" width="36" height="36"/></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_1_di" bpmnElement="Task_1"><dc:Bounds x="300" y="160" width="120" height="80"/></bpmndi:BPMNShape><bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1"><dc:Bounds x="520" y="182" width="36" height="36"/></bpmndi:BPMNShape><bpmndi:BPMNEdge id="Flow_1_di" bpmnElement="Flow_1"><di:waypoint x="216" y="200"/><di:waypoint x="300" y="200"/></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_2_di" bpmnElement="Flow_2"><di:waypoint x="420" y="200"/><di:waypoint x="520" y="200"/></bpmndi:BPMNEdge></bpmndi:BPMNPlane></bpmndi:BPMNDiagram></bpmn:definitions>`
function msg(t:string,error=false){message.value=t;messageType.value=error?'error':'success';setTimeout(()=>message.value='',4000)}

function inferType(type:string){
  if(type.includes('Gateway'))return'Decision'
  if(type.includes('StartEvent'))return'Start'
  if(type.includes('EndEvent'))return'End'
  if(type.includes('ServiceTask')||type.includes('ScriptTask'))return'SystemTask'
  return'HumanTask'
}

function loadMetadataFromXml(){
  variables.splice(0)
  Object.keys(steps).forEach(k=>delete steps[k])

  const registry=modeler.get('elementRegistry')
  const process=
    registry.filter((e:any)=>e.type==='bpmn:Process')[0]?.businessObject ||
    registry.getAll().find((e:any)=>e.businessObject?.$type==='bpmn:Process')?.businessObject

  if(process?.variables){
    try{variables.push(...JSON.parse(process.variables))}catch{}
  }
  // a workflow drawn before types is user-defined when its Start has a form (it was offered on Start a Workflow), else system-built
  const startForm=registry.getAll().find((e:any)=>e.type==='bpmn:StartEvent')?.businessObject?.formCode||''
  const t=String(process?.workflowType??'').toUpperCase()
  wfType.value=t==='USER'||(!t&&startForm)?'USER':'SYSTEM'
  wfDocument.value=process?.documentCode||(wfType.value==='USER'?startForm:'')

  registry.getAll().forEach((e:any)=>{
    const bo=e.businessObject
    if(!bo||(!e.type?.includes('Task')&&!e.type?.includes('Gateway')&&!e.type?.includes('Event')))return

    const s=emptyStep()
    s.stepName=bo.name||''
    s.stepCode=bo.stepCode||''
    s.ruleCode=bo.ruleCode||''
    s.formCode=bo.formCode||''
    s.stepType=bo.stepType||inferType(e.type)
    s.assignmentType=bo.assignmentType||((s.stepType==='HumanTask')?'Role':'None')
    s.assignee=bo.assignee||''
    s.route=bo.route||''
    s.slaDuration=bo.slaDuration||''
    s.slaUnit=bo.slaUnit||'HOURS'
    s.slaRemindHours=bo.slaRemindHours||''
    s.slaEscalateHours=bo.slaEscalateHours||''
    s.slaEscalation=bo.slaEscalation||'NONE'
    s.slaEscalateType=bo.slaEscalateType||'Role'
    s.slaEscalateTo=bo.slaEscalateTo||''
    s.parallelMode=['ALL','ANY','COUNT'].includes(String(bo.parallelMode||'').toUpperCase())?String(bo.parallelMode).toUpperCase():''
    s.parallelCount=bo.parallelCount||''

    try{
      s.routeParameters=bo.routeParameters?JSON.parse(bo.routeParameters):[]
    }catch{
      s.routeParameters=[]
    }

    s.actions=bo.actions?bo.actions.split(',').filter(Boolean):[]
    steps[e.id]=s
  })
}

function writeMetadataToBpmn(){
  const registry=modeler.get('elementRegistry')
  const modeling=modeler.get('modeling')
  const process=registry.getAll().find((e:any)=>e.businessObject?.$type==='bpmn:Process')?.businessObject

  if(process){
    process.variables=JSON.stringify(variables)
    process.workflowType=wfType.value
    process.documentCode=wfType.value==='USER'&&wfDocument.value?wfDocument.value:undefined
  }

  Object.entries(steps).forEach(([id,s])=>{
    const el=registry.get(id)
    if(!el)return

    modeling.updateProperties(el,{
      name:s.stepName||el.businessObject.name,
      stepCode:s.stepCode,
      ruleCode:s.ruleCode||undefined,
      formCode:s.formCode||undefined,
      stepType:s.stepType,
      assignmentType:s.assignmentType,
      assignee:s.assignee,
      route:s.route,
      routeParameters:JSON.stringify(s.routeParameters),
      actions:s.actions.join(','),
      // the step's time limit (SLA); none of it is written when the step has no time limit
      ...slaAttrs(s),
      // parallel approval: only on a human step, and the count only when a number is needed
      parallelMode:s.stepType==='HumanTask'&&s.parallelMode?s.parallelMode:undefined,
      parallelCount:s.stepType==='HumanTask'&&s.parallelMode==='COUNT'&&String(s.parallelCount).trim()?String(s.parallelCount).trim():undefined
    })
  })
}


function parallelSummary(s:Step){
  const users=s.assignee.split(/[,;]/).map(x=>x.trim()).filter(Boolean)
  const who=s.assignmentType==='Role'?`every member of ${s.assignee||'the role'}`:users.length?`${users.length} people (${users.join(', ')})`:'the approvers listed above'
  const need=s.parallelMode==='ALL'?'all of them must approve':s.parallelMode==='ANY'?'the first approval is enough':`${s.parallelCount||'?'} approvals are needed`
  return `Each of ${who} gets the task; ${need}. A rejection that makes this impossible rejects the step, and the tasks not answered yet are cancelled.`
}

function slaAttrs(s:Step){
  const on=s.stepType==='HumanTask'&&String(s.slaDuration).trim()!==''
  const esc=on&&s.slaEscalation!=='NONE'
  return{
    slaDuration:on?String(s.slaDuration).trim():undefined,
    slaUnit:on?s.slaUnit:undefined,
    slaRemindHours:on&&String(s.slaRemindHours).trim()!==''?String(s.slaRemindHours).trim():undefined,
    slaEscalation:esc?s.slaEscalation:undefined,
    slaEscalateHours:esc&&String(s.slaEscalateHours).trim()!==''?String(s.slaEscalateHours).trim():undefined,
    slaEscalateType:esc?s.slaEscalateType:undefined,
    slaEscalateTo:esc&&s.slaEscalateTo.trim()?s.slaEscalateTo.trim():undefined
  }
}
// a plain sentence of the step's time limit, under the settings
function slaSummary(s:Step){
  const n=String(s.slaDuration).trim()
  if(!n)return'No time limit: the task has no due date.'
  const unit=s.slaUnit==='DAYS'?'day(s)':s.slaUnit==='WORKING_DAYS'?'working day(s), skipping Friday, Saturday and holidays':'hour(s)'
  let t=`Due ${n} ${unit} after the task is created.`
  const r=String(s.slaRemindHours).trim()
  if(r&&Number(r)>0)t+=` Reminder ${r} hour(s) before.`
  if(s.slaEscalation!=='NONE'&&s.slaEscalateTo.trim()){
    const h=String(s.slaEscalateHours).trim()||'0'
    const who=(s.slaEscalateType==='User'?'user ':'role ')+s.slaEscalateTo.trim()
    t+=` ${h==='0'?'When overdue':`${h} hour(s) after it is overdue`}, ${s.slaEscalation==='REASSIGN'?`it moves to ${who}`:`${who} is told`}.`
  }
  return t
}

const MIN_ZOOM_PERCENT=50
const MAX_ZOOM_PERCENT=160
const ZOOM_STEP_PERCENT=10

function clampZoomPercent(value:number){
  return Math.min(
    MAX_ZOOM_PERCENT,
    Math.max(MIN_ZOOM_PERCENT,Math.round(value))
  )
}

function syncZoomPercent(){
  const bpmnCanvas=modeler?.get('canvas')
  if(!bpmnCanvas)return

  try{
    const zoom=bpmnCanvas.zoom()
    if(typeof zoom==='number' && Number.isFinite(zoom)){
      zoomPercent.value=clampZoomPercent(zoom*100)
    }
  }catch{}
}

async function setZoom(percent:number){
  const bpmnCanvas=modeler?.get('canvas')
  if(!bpmnCanvas)return

  const target=clampZoomPercent(percent)
  zoomPercent.value=target

  await nextTick()

  try{
    // Zoom around the visible viewport center, preserving the user's
    // current pan position as naturally as possible.
    bpmnCanvas.zoom(target/100,'auto')
    syncZoomPercent()
  }catch{}
}

async function zoomIn(){
  await setZoom(zoomPercent.value+ZOOM_STEP_PERCENT)
}

async function zoomOut(){
  await setZoom(zoomPercent.value-ZOOM_STEP_PERCENT)
}

async function fit(){
  const bpmnCanvas=modeler?.get('canvas')
  const registry=modeler?.get('elementRegistry')
  if(!bpmnCanvas||!registry)return

  await nextTick()
  await new Promise<void>(resolve =>
    requestAnimationFrame(() =>
      requestAnimationFrame(() => resolve())
    )
  )

  try{bpmnCanvas.resized()}catch{}

  const elements=registry.getAll().filter((e:any)=>
    e &&
    e.type!=='label' &&
    e.type!=='bpmn:Process' &&
    Number.isFinite(e.x) &&
    Number.isFinite(e.y) &&
    Number.isFinite(e.width) &&
    Number.isFinite(e.height)
  )

  if(!elements.length){
    try{bpmnCanvas.zoom('fit-viewport')}catch{}
    return
  }

  // the arrows count too: a route around the steps must stay in view
  const points=registry.getAll().flatMap((e:any)=>Array.isArray(e?.waypoints)?e.waypoints:[])
    .filter((p:any)=>Number.isFinite(p?.x)&&Number.isFinite(p?.y))
  const minX=Math.min(...elements.map((e:any)=>e.x),...points.map((p:any)=>p.x))
  const minY=Math.min(...elements.map((e:any)=>e.y),...points.map((p:any)=>p.y))
  const maxX=Math.max(...elements.map((e:any)=>e.x+e.width),...points.map((p:any)=>p.x))
  const maxY=Math.max(...elements.map((e:any)=>e.y+e.height),...points.map((p:any)=>p.y))

  const boundsWidth=Math.max(maxX-minX,1)
  const boundsHeight=Math.max(maxY-minY,1)

  const canvasNode=canvas.value
  const visibleWidth=Math.max(canvasNode?.clientWidth||0,1)
  const visibleHeight=Math.max(canvasNode?.clientHeight||0,1)

  // Margin in screen pixels around the process.
  const marginPx=70

  const scale=Math.min(
    Math.max((visibleWidth-marginPx*2)/boundsWidth,0.05),
    Math.max((visibleHeight-marginPx*2)/boundsHeight,0.05)
  )

  const viewWidth=visibleWidth/scale
  const viewHeight=visibleHeight/scale

  const centerX=(minX+maxX)/2
  const centerY=(minY+maxY)/2

  try{
    bpmnCanvas.viewbox({
      x:centerX-viewWidth/2,
      y:centerY-viewHeight/2,
      width:viewWidth,
      height:viewHeight
    })
  }catch{
    try{bpmnCanvas.zoom('fit-viewport')}catch{}
  }

  syncZoomPercent()
}
async function toggleLeftPanel(){
  // Toolbox is only an overlay. Never resize, fit or recenter the BPMN canvas.
  // This preserves the exact pan/zoom position selected by the user.
  leftPanelOpen.value=!leftPanelOpen.value
  await nextTick()
}

async function toggleRightPanel(){
  // the canvas narrows or widens; the resize observer below fits the diagram to the new size
  rightPanelOpen.value=!rightPanelOpen.value
  await nextTick()
}

// When the canvas changes size (the properties panel opens or closes, the window or the browser zoom changes)
// the diagram is fitted again, so no step is left outside the visible area.
let canvasSize=''
let refitTimer:ReturnType<typeof setTimeout>|undefined
const canvasObserver=new ResizeObserver(entries=>{
  const r=entries[0]?.contentRect
  if(!r||r.width<10||r.height<10)return
  const size=`${Math.round(r.width)}x${Math.round(r.height)}`
  if(size===canvasSize)return
  const first=!canvasSize
  canvasSize=size
  if(first)return
  clearTimeout(refitTimer)
  refitTimer=setTimeout(()=>{void fit()},120)
})

function clearSelection(){
  selectedId.value=''
  selectedType.value=''
  flowOutcome.value=''
  customFlowOutcome.value=''
  selectedFlowSource.value=''
  selectedFlowTarget.value=''
  modeler?.get('selection').select(null)
}

function addCustomAction(){
  const a=customAction.value.trim().toUpperCase()
  if(a&&!step.value.actions.includes(a))step.value.actions.push(a)
  if(a&&!actionCatalog.value.includes(a))actionCatalog.value.push(a)
  customAction.value=''
}

function syncStepName(){
  const el=modeler?.get('elementRegistry').get(selectedId.value)
  if(el)modeler.get('modeling').updateProperties(el,{name:step.value.stepName})
}


// rules on data fields (K2's line rules): stored on the arrow as finova:conditions; all must match for the path to be taken
const docFields=computed(()=>documents.value.find(d=>d.formCode.toUpperCase()===wfDocument.value.toUpperCase())?.fields??[])
const ownExtra=computed(()=>variables.filter(v=>v?.name&&!docFields.value.some(f=>f.name.toUpperCase()===String(v.name).toUpperCase())).map(v=>String(v.label||v.name)))
function removeOwnFields(){variables.splice(0,variables.length);msg('Own data fields removed. Save the workflow to keep the change.')}
// what the modules send when they start their workflows (FctApplicationService, CrmDisbursementRequestService, P0201PortfolioCases)
const moduleValues=[
  {name:'client_name',label:'Client name',type:'String',from:'Factoring application, Disbursement request'},
  {name:'client_number',label:'Client number',type:'String',from:'Factoring application'},
  {name:'client_id',label:'Client id',type:'String',from:'Factoring application, Disbursement request'},
  {name:'amount',label:'Amount',type:'Decimal',from:'Factoring application (facility limit), Disbursement request (gross amount)'},
  {name:'currency_code',label:'Currency',type:'String',from:'Factoring application, Disbursement request'},
  {name:'application_reference',label:'Application reference',type:'String',from:'Factoring application'},
  {name:'entity_id',label:'Record id',type:'Number',from:'All modules'},
  {name:'entity_ref',label:'Reference',type:'String',from:'All modules'},
  {name:'function_code',label:'Function',type:'String',from:'Disbursement request'},
  {name:'entity_type',label:'Record type',type:'String',from:'All modules'},
  {name:'module_code',label:'Module',type:'String',from:'All modules'},
]
const ruleFields=computed(()=>{
  const out:{name:string,label:string,type:string,choices:string[]}[]=[]
  const add=(name:string,label:string|undefined|null,type:string|undefined,choices:string[]|undefined|null)=>{
    const n=String(name??'').trim();if(!n||out.some(x=>x.name.toUpperCase()===n.toUpperCase()))return
    out.push({name:n,label:String(label??'').trim()||n,type:type||'String',choices:Array.isArray(choices)?choices:[]})
  }
  // a user-defined workflow's fields are its document's only
  if(wfType.value==='USER'){docFields.value.forEach(f=>add(f.name,f.label,f.type,f.choices));return out}
  variables.forEach(v=>add(v.name,v.label,v.type,v.choices))
  const codes=new Set([wfDocument.value,...Object.values(steps).map(x=>x.formCode)].filter(Boolean).map(x=>String(x).toUpperCase()))
  documents.value.filter(d=>codes.has(d.formCode.toUpperCase())).forEach(d=>(d.fields||[]).forEach(f=>add(f.name,f.label,f.type,f.choices)))
  return out
})
function ruleField(name:string){return ruleFields.value.find(f=>f.name.toUpperCase()===String(name||'').toUpperCase())}
function ruleType(name:string){return ruleField(name)?.type||'String'}
function isNumberField(name:string){return ['Number','Decimal'].includes(ruleType(name))}
function ruleChoices(name:string){const f=ruleField(name);return f?.type==='Boolean'?['yes','no']:f?.type==='Choice'?f.choices:[]}
function ruleOps(name:string){
  const t=ruleType(name)
  const ops=[{value:'=',label:'is'},{value:'!=',label:'is not'}]
  if(['Number','Decimal','Date'].includes(t))ops.push({value:'>',label:t==='Date'?'is after':'is more than'},{value:'>=',label:t==='Date'?'is on or after':'is at least'},{value:'<',label:t==='Date'?'is before':'is less than'},{value:'<=',label:t==='Date'?'is on or before':'is at most'})
  if(['String','User'].includes(t))ops.push({value:'contains',label:'contains'})
  ops.push({value:'empty',label:'is empty'},{value:'notEmpty',label:'is filled in'})
  return ops
}
function readFlowRules(el:any){
  try{
    const raw=JSON.parse(el?.businessObject?.conditions||'[]')
    flowRules.value=Array.isArray(raw)?raw.map((x:any)=>({field:String(x?.field??''),op:String(x?.op??'='),value:x?.value==null?'':String(x.value)})):[]
  }catch{flowRules.value=[]}
}
function syncFlowRules(){
  if(selectedType.value!=='bpmn:SequenceFlow')return
  const el=modeler?.get('elementRegistry').get(selectedId.value)
  if(!el)return
  // a test that does not fit the field (e.g. "more than" on text) falls back to "is"
  flowRules.value.forEach(r=>{if(!ruleOps(r.field).some(o=>o.value===r.op))r.op='='})
  const keep=flowRules.value.filter(r=>r.field).map(r=>({field:r.field,op:r.op,...(r.op==='empty'||r.op==='notEmpty'?{}:{value:r.value.trim()})}))
  modeler.get('modeling').updateProperties(el,{conditions:keep.length?JSON.stringify(keep):undefined})
  refreshRuleBadges()
}
// the conditions shown on the canvas next to their arrow, so the routing reads at a glance
function ruleBadgeText(json:string){
  try{
    const raw=JSON.parse(json||'[]');if(!Array.isArray(raw)||!raw.length)return ''
    const sym:Record<string,string>={'=':'=','!=':'≠','>':'>','>=':'≥','<':'<','<=':'≤',contains:'contains',empty:'is empty',notEmpty:'is filled in'}
    return raw.map((r:any)=>{
      const f=ruleField(r?.field)?.label||r?.field||'?'
      const v=r?.value==null?'':String(r.value),n=Number(v.replace(/,/g,''))
      const shown=v!==''&&!Number.isNaN(n)&&/^[\d,.]+$/.test(v)?n.toLocaleString('en-US'):v
      return r?.op==='empty'||r?.op==='notEmpty'?`${f} ${sym[r.op]}`:`${f} ${sym[r?.op]??r?.op} ${shown}`
    }).join(' and ')
  }catch{return ''}
}
function refreshRuleBadges(){
  if(!modeler)return
  const overlays=modeler.get('overlays')
  overlays.remove({type:'rule'})
  modeler.get('elementRegistry').filter((e:any)=>e.type==='bpmn:SequenceFlow'&&e.businessObject?.conditions).forEach((e:any)=>{
    const text=ruleBadgeText(e.businessObject.conditions)
    const w=e.waypoints||[];if(!text||w.length<2)return
    // the middle of the longest segment, relative to the arrow's box
    let best=0,mid={x:w[0].x,y:w[0].y}
    for(let i=1;i<w.length;i++){const l=Math.hypot(w[i].x-w[i-1].x,w[i].y-w[i-1].y);if(l>best){best=l;mid={x:(w[i].x+w[i-1].x)/2,y:(w[i].y+w[i-1].y)/2}}}
    const minX=Math.min(...w.map((p:any)=>p.x)),minY=Math.min(...w.map((p:any)=>p.y))
    const badge=document.createElement('div');badge.className='rule-badge';badge.textContent=text
    overlays.add(e,'rule',{position:{left:mid.x-minX,top:mid.y-minY},html:badge})
  })
}
function addFlowRule(){flowRules.value.push({field:ruleFields.value[0]?.name||'',op:'=',value:''});syncFlowRules()}
function removeFlowRule(i:number){flowRules.value.splice(i,1);syncFlowRules()}

function syncFlowOutcome(){
  if(selectedType.value!=='bpmn:SequenceFlow')return

  const el=modeler?.get('elementRegistry').get(selectedId.value)
  if(!el)return

  const value=(flowOutcome.value||'').trim().toUpperCase()

  modeler.get('modeling').updateProperties(
    el,
    {name:value||undefined}
  )

  customFlowOutcome.value=''
}

function applyCustomFlowOutcome(){
  if(selectedType.value!=='bpmn:SequenceFlow')return

  const value=customFlowOutcome.value.trim().toUpperCase()
  if(!value)return

  const el=modeler?.get('elementRegistry').get(selectedId.value)
  if(!el)return

  modeler.get('modeling').updateProperties(
    el,
    {name:value}
  )

  flowOutcome.value=value
  customFlowOutcome.value=''
}

function elementCenter(el:any){
  return {
    x:(el?.x||0)+(el?.width||0)/2,
    y:(el?.y||0)+(el?.height||0)/2
  }
}

function boundaryDock(el:any,toward:{x:number,y:number}){
  const c=elementCenter(el)
  const w=Math.max(Number(el?.width)||0,1)
  const h=Math.max(Number(el?.height)||0,1)
  const dx=toward.x-c.x
  const dy=toward.y-c.y

  if(Math.abs(dx)<0.0001 && Math.abs(dy)<0.0001)
    return c

  const type=String(el?.type||'')

  // BPMN events are circular. Dock on the circle perimeter.
  if(type.includes('Event')){
    const rx=w/2
    const ry=h/2
    const denom=Math.sqrt(
      (dx*dx)/(rx*rx)+(dy*dy)/(ry*ry)
    ) || 1

    return {
      x:c.x+dx/denom,
      y:c.y+dy/denom
    }
  }

  // Tasks/sub-processes are rectangular. Dock on the rectangle border.
  const sx=Math.abs(dx)>0.0001 ? (w/2)/Math.abs(dx) : Number.POSITIVE_INFINITY
  const sy=Math.abs(dy)>0.0001 ? (h/2)/Math.abs(dy) : Number.POSITIVE_INFINITY
  const scale=Math.min(sx,sy)

  return {
    x:c.x+dx*scale,
    y:c.y+dy*scale
  }
}

function routeParallelConnection(connection:any,offset=42,direction=1){
  if(!connection?.source||!connection?.target)return

  const modeling=modeler?.get('modeling')
  if(!modeling)return

  const source=connection.source
  const target=connection.target
  const sourceCenter=elementCenter(source)
  const targetCenter=elementCenter(target)

  const start=boundaryDock(source,targetCenter)
  const end=boundaryDock(target,sourceCenter)

  const dx=Math.abs(targetCenter.x-sourceCenter.x)
  const dy=Math.abs(targetCenter.y-sourceCenter.y)

  let waypoints:any[]

  if(dx>=dy){
    // Mostly horizontal flow.
    const lead=Math.min(55,Math.max(28,dx*0.12))
    const routedY=
      ((start.y+end.y)/2)+(offset*direction)

    waypoints=[
      {x:start.x,y:start.y},
      {x:start.x+Math.sign(targetCenter.x-sourceCenter.x||1)*lead,y:start.y},
      {x:start.x+Math.sign(targetCenter.x-sourceCenter.x||1)*lead,y:routedY},
      {x:end.x-Math.sign(targetCenter.x-sourceCenter.x||1)*lead,y:routedY},
      {x:end.x-Math.sign(targetCenter.x-sourceCenter.x||1)*lead,y:end.y},
      {x:end.x,y:end.y}
    ]
  }else{
    // Mostly vertical flow.
    const lead=Math.min(55,Math.max(28,dy*0.12))
    const routedX=
      ((start.x+end.x)/2)+(offset*direction)

    waypoints=[
      {x:start.x,y:start.y},
      {x:start.x,y:start.y+Math.sign(targetCenter.y-sourceCenter.y||1)*lead},
      {x:routedX,y:start.y+Math.sign(targetCenter.y-sourceCenter.y||1)*lead},
      {x:routedX,y:end.y-Math.sign(targetCenter.y-sourceCenter.y||1)*lead},
      {x:end.x,y:end.y-Math.sign(targetCenter.y-sourceCenter.y||1)*lead},
      {x:end.x,y:end.y}
    ]
  }

  try{modeling.updateWaypoints(connection,waypoints)}catch{}
}

function addParallelOutcomeFlow(){
  if(selectedType.value!=='bpmn:SequenceFlow')return

  const registry=modeler?.get('elementRegistry')
  const modeling=modeler?.get('modeling')
  const selection=modeler?.get('selection')
  const current=registry?.get(selectedId.value)

  if(!current?.source||!current?.target||!modeling)return

  try{
    const created=modeling.connect(
      current.source,
      current.target,
      {type:'bpmn:SequenceFlow'}
    )

    routeParallelConnection(created,42,1)
    selection?.select(created)

    msg('Parallel outcome flow created. Select its Outcome / Action.')
  }catch(e){
    msg(
      e instanceof Error
        ? e.message
        : 'Unable to create parallel sequence flow.',
      true
    )
  }
}

function repairSelectedFlow(){
  if(selectedType.value!=='bpmn:SequenceFlow')return

  const registry=modeler?.get('elementRegistry')
  const current=registry?.get(selectedId.value)

  if(!current?.source||!current?.target)return

  try{
    routeParallelConnection(current,42,1)
    msg('Connection repaired and docked to both BPMN elements.')
  }catch(e){
    msg(
      e instanceof Error
        ? e.message
        : 'Unable to repair the selected connection.',
      true
    )
  }
}

function bindSelection(){modeler.on(['import.done','commandStack.changed'],()=>setTimeout(refreshRuleBadges));modeler.on('selection.changed',(ev:any)=>{
  const e=ev.newSelection?.[0]

  if(!e){
    selectedId.value=''
    selectedType.value=''
    flowOutcome.value=''
    customFlowOutcome.value=''
    selectedFlowSource.value=''
    selectedFlowTarget.value=''
    return
  }

  // a step or an arrow opens its properties over the canvas
  rightPanelOpen.value=true

  if(e.type==='bpmn:SequenceFlow'){
    selectedId.value=e.id
    selectedType.value=e.type

    const outcome=(e.businessObject?.name||'').trim().toUpperCase()
    flowOutcome.value=outcome
    readFlowRules(e)
    customFlowOutcome.value=''

    selectedFlowSource.value=
      e.source?.businessObject?.name ||
      e.source?.id ||
      e.businessObject?.sourceRef?.name ||
      e.businessObject?.sourceRef?.id ||
      '-'

    selectedFlowTarget.value=
      e.target?.businessObject?.name ||
      e.target?.id ||
      e.businessObject?.targetRef?.name ||
      e.businessObject?.targetRef?.id ||
      '-'

    return
  }

  if(!e.type?.includes('Task')&&!e.type?.includes('Gateway')&&!e.type?.includes('Event')){
    selectedId.value=''
    selectedType.value=''
    return
  }

  selectedId.value=e.id
  selectedType.value=e.type
  flowOutcome.value=''
  customFlowOutcome.value=''
  selectedFlowSource.value=''
  selectedFlowTarget.value=''

  if(!steps[e.id]){
    const created=emptyStep()
    created.stepName=e.businessObject?.name||''
    created.stepType=inferType(e.type)
    if(created.stepType!=='HumanTask'){
      created.assignmentType='None'
      created.actions=[]
    }
    steps[e.id]=created
  }
})}

type FinovaViewbox={
  x:number,
  y:number,
  width:number,
  height:number
}

const VIEWBOX_REGEX=/<!--\s*FINOVA_VIEWBOX:(\{.*?\})\s*-->/s

function removeSavedViewbox(xml:string){
  return xml.replace(VIEWBOX_REGEX,'')
}

function currentViewbox():FinovaViewbox|null{
  const bpmnCanvas=modeler?.get('canvas')
  if(!bpmnCanvas)return null

  try{
    const v=bpmnCanvas.viewbox()

    if(
      Number.isFinite(v?.x) &&
      Number.isFinite(v?.y) &&
      Number.isFinite(v?.width) &&
      Number.isFinite(v?.height)
    ){
      return{
        x:Number(v.x),
        y:Number(v.y),
        width:Number(v.width),
        height:Number(v.height)
      }
    }
  }catch{}

  return null
}

function writeSavedViewbox(xml:string,viewbox:FinovaViewbox|null){
  const clean=removeSavedViewbox(xml)
  if(!viewbox)return clean

  const marker=`<!-- FINOVA_VIEWBOX:${JSON.stringify(viewbox)} -->`

  // Keep it inside the BPMN document but outside the process definition,
  // immediately before the closing definitions element.
  return clean.replace(
    '</bpmn:definitions>',
    `${marker}\n</bpmn:definitions>`
  )
}

async function refreshXmlPreview(){
  writeMetadataToBpmn()
  const r=await modeler.saveXML({format:true})
  xmlPreview.value=writeSavedViewbox(
    r.xml||'',
    currentViewbox()
  )
}

// released versions (K2-style): new instances use the latest, running ones keep theirs
const showVersions=ref(false),versions=ref<WorkflowVersion[]>([]),versionsLoading=ref(false),versionsKept=ref(false)
const viewing=ref<WorkflowVersion|null>(null)
let currentXml=''
function fmtDate(x:string){return new Date(x).toLocaleString(undefined,{day:'2-digit',month:'short',year:'numeric',hour:'2-digit',minute:'2-digit'})}
async function loadVersions(){
  if(form.dgrmId<=0){versions.value=[];versionsKept.value=false;return}
  versionsLoading.value=true
  try{versions.value=await getWorkflowVersions(form.dgrmId);versionsKept.value=versions.value.length>0}
  catch{versions.value=[]}
  finally{versionsLoading.value=false}
}
async function openVersions(){showVersions.value=true;await loadVersions()}
async function viewVersion(v:WorkflowVersion){
  try{
    const xml=await getWorkflowVersionXml(form.dgrmId,v.definitionVersionId)
    // keep the workflow as it is on screen, unsaved edits included, to come back to it
    if(!viewing.value){writeMetadataToBpmn();currentXml=(await modeler.saveXML({format:true})).xml||''}
    viewing.value=v
    showVersions.value=false
    await modeler.importXML(removeSavedViewbox(xml))
    loadMetadataFromXml()
    await fit()
    await refreshXmlPreview()
  }catch(e){msg(e instanceof Error?e.message:'The version could not be opened.',true)}
}
async function backToCurrent(){
  viewing.value=null
  await modeler.importXML(currentXml)
  loadMetadataFromXml()
  await fit()
  await refreshXmlPreview()
}

// data fields (K2-style): declared on the process as finova:variables; older entries have only name, type and required
const fieldTypes=[{value:'String',label:'Text'},{value:'Number',label:'Whole number'},{value:'Decimal',label:'Amount'},{value:'Boolean',label:'Yes / No'},{value:'Date',label:'Date'},{value:'Choice',label:'Choice'},{value:'User',label:'User'}]
function typeLabel(t:string){return fieldTypes.find(x=>x.value===t)?.label??t}
type FieldRow={name:string,label:string,type:string,required:boolean,defaultValue:string,choicesText:string,description?:string}
const showFields=ref(false),fieldRows=ref<FieldRow[]>([])
function openFields(){
  fieldRows.value=variables.filter(v=>String(v?.name??'').trim()).map(v=>({name:String(v.name??''),label:String(v.label??''),type:fieldTypes.some(t=>t.value===v.type)?v.type:'String',required:!!v.required,defaultValue:String(v.defaultValue??''),choicesText:(Array.isArray(v.choices)?v.choices:[]).join(', '),description:v.description}))
  showFields.value=true
}
function choicesOf(f:FieldRow){return f.choicesText.split(',').map(x=>x.trim()).filter(Boolean)}
function fieldProblem(f:FieldRow,i:number):string{
  const n=f.name.trim(),d=f.defaultValue.trim(),c=f.label.trim()||n||`Row ${i+1}`
  if(!n)return `Row ${i+1}: give the field a name.`
  if(!/^[A-Za-z][A-Za-z0-9_]*$/.test(n))return `"${n}": a name uses letters, digits and _ only, starting with a letter.`
  if(fieldRows.value.findIndex(x=>x.name.trim().toLowerCase()===n.toLowerCase())!==i)return `"${n}" is used twice.`
  if(f.type==='Choice'&&!choicesOf(f).length)return `"${c}": list its choices, separated by commas.`
  if(d&&f.type==='Number'&&!/^-?[\d,]+$/.test(d))return `"${c}": the default must be a whole number.`
  if(d&&f.type==='Decimal'&&!/^-?[\d,]*\.?\d+$/.test(d))return `"${c}": the default must be an amount.`
  if(d&&f.type==='Choice'&&!choicesOf(f).some(x=>x.toLowerCase()===d.toLowerCase()))return `"${c}": the default must be one of the choices.`
  return ''
}
const fieldErrors=computed(()=>fieldRows.value.map((f,i)=>fieldProblem(f,i)))
const fieldProblems=computed(()=>fieldErrors.value.filter(Boolean))
function applyFields(){
  if(fieldProblems.value.length)return
  variables.splice(0,variables.length,...fieldRows.value.map(f=>{
    // values from the module are never required: the module sends them
    const v:Var={name:f.name.trim(),type:f.type,required:false}
    if(f.label.trim())v.label=f.label.trim()
    if(f.type==='Choice')v.choices=choicesOf(f)
    if(f.description)v.description=f.description
    return v
  }))
  showFields.value=false
  msg('Values applied. Save the workflow to keep them.')
}

async function load(){
  let xml=starter
  const id=route.params.id

  if(id&&id!=='new'){
    const w=await getWorkflowDefinition(Number(id))
    Object.assign(form,w)
    active.value=form.isActive==='Y'
    xml=form.dgrmContent||starter
  }
  loadForms()

  await modeler.importXML(removeSavedViewbox(xml))
  loadMetadataFromXml()
  await loadVersions()

  // First-time / old workflow:
  // center the entire diagram horizontally and vertically.
  //
  // Previously saved workflow:
  // restore exactly the pan/zoom position the user saved.
  // the diagram opens fitted to the full canvas (the saved position was taken with the old side panel open)
  await fit()

  await refreshXmlPreview()
}
async function save(){
  if(saving.value)return

  if(!form.workflowCode.trim()||!form.dgrmName.trim()){
    msg('Workflow code and name are required.',true)
    return
  }

  saving.value=true

  try{
    // Save BPMN nodes, arrows AND the user's current canvas pan/zoom.
    writeMetadataToBpmn()

    const r=await modeler.saveXML({format:true})
    const viewbox=currentViewbox()

    form.dgrmContent=writeSavedViewbox(
      r.xml||'',
      viewbox
    )

    xmlPreview.value=form.dgrmContent
    form.isActive=active.value?'Y':'N'

    const saved=
      form.dgrmId>0
        ? await updateWorkflowDefinition(form.dgrmId,form)
        : await createWorkflowDefinition(form)

    const before=form.dgrmVersion
    Object.assign(form,saved)
    await loadVersions()

    msg(versionsKept.value&&saved.dgrmVersion!==before
      ? `Saved as version ${saved.dgrmVersion}. New instances use it; running instances stay on their version.`
      : 'Workflow and designer position saved successfully.')

    if(route.params.id==='new'||!route.params.id)
      await router.replace(`/workflowengine/designer/${form.dgrmId}`)
  }
  catch(e){
    msg(apiErrorText(e,'Save failed.'),true)
  }
  finally{
    saving.value=false
  }
}
onMounted(async()=>{try{modeler=new BpmnModeler({container:canvas.value??undefined,moddleExtensions:{finova:finovaWorkflowModdle},textRenderer:{defaultStyle:{fontFamily:'Inter, Arial, sans-serif',fontSize:15,fontWeight:600,lineHeight:1.2},externalStyle:{fontSize:13,lineHeight:1.2}}});bindSelection();await load();if(canvas.value)canvasObserver.observe(canvas.value)}catch(e){msg(e instanceof Error?e.message:'Designer initialization failed.',true)}});onBeforeUnmount(()=>{canvasObserver.disconnect();clearTimeout(refitTimer);modeler?.destroy()})
</script>
<style scoped>
.sla-summary{margin:10px 0 0;padding:8px 10px;border-radius:6px;background:#e8f3f4;color:#00636D;font-size:12px;line-height:1.45}
.sla-summary .pi{font-size:12px;margin-right:4px}

.designer-page,
.designer-page * {
  box-sizing: border-box;
}

.designer-page {
  height: calc(100vh - 10px);
  min-height: 680px;
  display: flex;
  flex-direction: column;
  gap: 9px;
  padding: 10px 14px 14px;
  overflow: hidden;
  background:
    radial-gradient(circle at 84% 0%, rgba(41, 130, 174, .08), transparent 24%),
    #f4f7f9;
  color: #153653;
  font-family: Inter, "Segoe UI", Arial, sans-serif;
}

/* HEADER */
.finova-page-head {
  min-height: 48px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 14px;
  flex: 0 0 auto;
}

.page-eyebrow {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-bottom: 1px;
  color: #71879a;
  font-size: 9px;
  font-weight: 800;
  letter-spacing: .08em;
  text-transform: uppercase;
}

.eyebrow-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #f47a20;
}

.page-head-copy h1 {
  margin: 0;
  color: #0a3c6d;
  font-size: 22px;
  line-height: 1.05;
  font-weight: 850;
  letter-spacing: -.02em;
}

.page-head-copy p {
  margin: 3px 0 0;
  color: #6c8192;
  font-size: 11.5px;
}

.page-head-actions {
  display: flex;
  align-items: center;
  gap: 8px;
}

.btn {
  min-height: 36px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 0 12px;
  border-radius: 8px;
  border: 1px solid transparent;
  font: inherit;
  font-size: 12.5px;
  font-weight: 750;
  cursor: pointer;
  transition: .16s ease;
}

.btn:hover {
  transform: translateY(-1px);
}

.btn:disabled {
  opacity: .55;
  cursor: not-allowed;
  transform: none;
}

.btn-ghost {
  color: #244968;
  border-color: #cfdae2;
  background: #fff;
}

.btn-secondary {
  color: #087b76;
  border-color: #bcdedb;
  background: #f7fcfb;
}

.btn-primary {
  color: #fff;
  border-color: #ec7317;
  background: linear-gradient(135deg, #f68a28, #ec6b0a);
  box-shadow: 0 5px 13px rgba(229, 101, 8, .18);
}

.btn-full {
  width: 100%;
}

/* WORKFLOW SUMMARY */
.workflow-summary-card {
  flex: 0 0 auto;
  display: grid;
  grid-template-columns: 1.05fr 1.5fr .48fr .72fr .78fr;
  gap: 10px;
  align-items: end;
  padding: 9px 12px;
  border: 1px solid #d9e3e8;
  border-radius: 13px;
  background: #fff;
  box-shadow: 0 5px 18px rgba(21, 56, 79, .035);
}

.summary-field label,
.field-label {
  display: block;
  margin: 0 0 6px;
  color: #49657b;
  font-size: 11.5px;
  font-weight: 750;
}

.summary-field input,
.summary-field select,
.properties-panel input:not([type=checkbox]),
.properties-panel select,
.properties-panel textarea {
  width: 100%;
  border: 1px solid #cbd9e1;
  border-radius: 8px;
  background: #fff;
  color: #173951;
  font: inherit;
  font-size: 13px;
  outline: none;
  transition: .15s ease;
}

.summary-field input,
.summary-field select,
.properties-panel input:not([type=checkbox]),
.properties-panel select {
  height: 36px;
  padding: 0 10px;
}

.properties-panel textarea {
  padding: 10px 11px;
  resize: vertical;
}

.summary-field input:focus,
.summary-field select:focus,
.properties-panel input:not([type=checkbox]):focus,
.properties-panel select:focus,
.properties-panel textarea:focus {
  border-color: #4ca9a4;
  box-shadow: 0 0 0 3px rgba(76, 169, 164, .10);
}

.active-switch {
  min-height: 36px;
  display: flex;
  align-items: center;
  gap: 9px;
  cursor: pointer;
}

.active-switch > input {
  position: absolute;
  opacity: 0;
  pointer-events: none;
}

.switch-track {
  width: 42px;
  height: 23px;
  position: relative;
  flex: 0 0 42px;
  border-radius: 999px;
  background: #cbd7dc;
  transition: .18s ease;
}

.switch-knob {
  width: 17px;
  height: 17px;
  position: absolute;
  left: 3px;
  top: 3px;
  border-radius: 50%;
  background: #fff;
  box-shadow: 0 2px 5px rgba(0,0,0,.17);
  transition: .18s ease;
}

.active-switch > input:checked + .switch-track {
  background: #13856f;
}

.active-switch > input:checked + .switch-track .switch-knob {
  transform: translateX(19px);
}

.switch-copy strong,
.switch-copy small {
  display: block;
}

.switch-copy strong {
  color: #294a60;
  font-size: 12px;
}

.switch-copy small {
  margin-top: 2px;
  color: #83939d;
  font-size: 9px;
}

/* WORKSPACE */
.designer-shell {
  flex: 1 1 auto;
  min-height: 0;
  position: relative;
  display: grid;
  grid-template-columns: minmax(0, 1fr) 390px;
  transition: grid-template-columns .22s ease;
  border: 1px solid #d7e2e8;
  border-radius: 14px;
  overflow: hidden;
  background: #fff;
  box-shadow: 0 8px 26px rgba(16, 51, 75, .05);
}

.designer-shell.properties-collapsed {
  grid-template-columns: minmax(0, 1fr) 0;
}

.designer-shell.properties-collapsed .canvas-panel {
  border-right: 0;
}

.canvas-panel {
  min-width: 0;
  min-height: 0;
  display: flex;
  flex-direction: column;
  border-right: 1px solid #dce5ea;
}

.canvas-toolbar {
  min-height: 50px;
  flex: 0 0 50px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 0 14px 0 16px;
  border-bottom: 1px solid #e1e8ec;
  background: linear-gradient(180deg, #ffffff, #fbfcfd);
}

.canvas-title {
  display: flex;
  align-items: center;
  gap: 10px;
}

.canvas-title-icon {
  width: 34px;
  height: 34px;
  display: grid;
  place-items: center;
  border-radius: 9px;
  color: #0a7a76;
  background: #e5f4f2;
}

.canvas-title strong,
.canvas-title small {
  display: block;
}

.canvas-title strong {
  color: #153b59;
  font-size: 13px;
}

.canvas-title small {
  margin-top: 2px;
  color: #82929c;
  font-size: 10px;
}

.canvas-toolbar-actions {
  display: flex;
  gap: 6px;
}

.tool-btn {
  width: 34px;
  height: 34px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  border: 1px solid #d6e0e5;
  border-radius: 8px;
  background: #fff;
  color: #355b75;
  cursor: pointer;
}

.tool-btn-wide {
  width: auto;
  min-width: 82px;
  padding: 0 10px;
  font-size: 11px;
  font-weight: 750;
}

.tool-btn.active {
  border-color: #9bcfc9;
  background: #edf8f6;
  color: #087a71;
}

.zoom-control {
  height: 34px;
  display: inline-flex;
  align-items: stretch;
  overflow: hidden;
  border: 1px solid #d6e0e5;
  border-radius: 8px;
  background: #fff;
}

.zoom-button,
.zoom-value {
  border: 0;
  background: #fff;
  color: #355b75;
  font: inherit;
  cursor: pointer;
}

.zoom-button {
  width: 32px;
  display: grid;
  place-items: center;
}

.zoom-value {
  min-width: 52px;
  padding: 0 8px;
  border-left: 1px solid #e1e8ec;
  border-right: 1px solid #e1e8ec;
  font-size: 11px;
  font-weight: 800;
}

.zoom-button:hover,
.zoom-value:hover {
  background: #f3f8fa;
}


.canvas-wrap {
  flex: 1 1 auto;
  min-height: 0;
  position: relative;
  overflow-x: auto;
  overflow-y: auto;
  background-color: #fff;
  background-image:
    radial-gradient(circle, #dce8ed 1px, transparent 1px);
  background-size: 18px 18px;
  scrollbar-gutter: stable;
}

.canvas-scroll-content {
  width: 100%;
  min-width: 0;
  height: 100%;
  position: relative;
}

.canvas {
  width: 100%;
  height: 100%;
}

/* Native BPMN toolbox is an optional sliding overlay.
   The designer initially opens it collapsed so it cannot cover the process. */
.canvas-wrap.toolbox-hidden :deep(.djs-palette) {
  display: none !important;
}

/* Toolbox is a pure overlay. Showing/hiding it must never change
   canvas dimensions, viewbox, zoom or diagram position. */
.canvas :deep(.djs-palette) {
  position: absolute !important;
}


/* BPMN.js visual alignment with FINOVA */
.canvas :deep(.djs-container) {
  background: transparent !important;
}

.canvas :deep(.djs-palette) {
  top: 12px !important;
  left: 14px !important;
  z-index: 20 !important;
  transform: scale(.90);
  transform-origin: top left;
  border: 1px solid #d9e3e8 !important;
  border-radius: 10px !important;
  overflow: hidden;
  background: rgba(255,255,255,.98) !important;
  box-shadow: 0 8px 22px rgba(16, 54, 77, .08) !important;
}

.canvas :deep(.djs-palette .entry) {
  color: #234d6a !important;
}

.canvas :deep(.djs-palette .entry:hover) {
  background: #f1f7f8 !important;
  color: #f27618 !important;
}

.canvas :deep(.djs-element.selected .djs-outline) {
  stroke: #f27b1d !important;
  stroke-width: 2px !important;
}

.canvas :deep(.djs-element.hover .djs-outline) {
  stroke: #4d9eb5 !important;
}

.canvas :deep(.djs-connection.selected .djs-outline) {
  stroke: #f27b1d !important;
}

.canvas :deep(.djs-label) {
  fill: #243f53 !important;
  font-family: Inter, "Segoe UI", Arial, sans-serif !important;
  font-size: 12px !important;
  font-weight: 600 !important;
}

.canvas :deep(.djs-shape .djs-visual > rect) {
  stroke: #2b6488 !important;
  stroke-width: 1.7px !important;
  fill: #f4f9fd !important;
  filter: drop-shadow(0 2px 2px rgba(30, 77, 105, .05));
}

.canvas :deep(.djs-shape.selected .djs-visual > rect) {
  stroke: #f27618 !important;
  stroke-width: 2px !important;
  fill: #fff2e8 !important;
}

.canvas :deep(.djs-shape.hover .djs-visual > rect) {
  stroke: #2387a8 !important;
  fill: #eef7fc !important;
}

.canvas :deep(.djs-connection .djs-visual > path) {
  stroke: #314a5c !important;
  stroke-width: 1.7px !important;
}

.canvas :deep(.djs-connection.selected .djs-visual > path) {
  stroke: #f27618 !important;
  stroke-width: 2.2px !important;
}

.canvas :deep(.djs-shape .djs-visual > circle) {
  fill: #ffffff !important;
  stroke: #254d68 !important;
}

.canvas :deep(.djs-shape.selected .djs-visual > circle) {
  stroke: #f27618 !important;
}

.canvas :deep(.djs-connection .djs-label) {
  paint-order: stroke;
  stroke: #ffffff;
  stroke-width: 4px;
  stroke-linejoin: round;
}

/* PROPERTIES */
.properties-panel {
  min-width: 0;
  min-height: 0;
  display: flex;
  flex-direction: column;
  background: #fbfcfd;
}

.properties-header {
  min-height: 68px;
  flex: 0 0 68px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 0 18px;
  border-bottom: 1px solid #e0e7eb;
  background: #fff;
}

.properties-kicker {
  display: block;
  margin-bottom: 2px;
  color: #f2781a;
  font-size: 9px;
  font-weight: 850;
  letter-spacing: .10em;
  text-transform: uppercase;
}

.properties-header h2 {
  margin: 0;
  color: #173c59;
  font-size: 17px;
  line-height: 1.15;
}

.panel-close {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  border: 1px solid #d5e0e5;
  border-radius: 8px;
  background: #fff;
  color: #526c7c;
  cursor: pointer;
}

.properties-body {
  flex: 1 1 auto;
  min-height: 0;
  overflow-y: auto;
  padding: 16px;
}

.property-section,
.parallel-card,
.selected-element-card {
  margin-bottom: 13px;
  padding: 14px;
  border: 1px solid #dce6ea;
  border-radius: 11px;
  background: #fff;
}

.section-title {
  display: flex;
  align-items: center;
  gap: 9px;
  margin-bottom: 13px;
}

.section-icon {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  flex: 0 0 32px;
  border-radius: 8px;
  color: #087d78;
  background: #e7f4f3;
}

.section-title strong,
.section-title small {
  display: block;
}

.section-title strong {
  color: #26485f;
  font-size: 12.5px;
}

.section-title small {
  margin-top: 2px;
  color: #82929c;
  font-size: 9.5px;
  line-height: 1.35;
}

.large-textarea {
  min-height: 92px;
}

.variable-head,
.var-row {
  display: grid;
  grid-template-columns: 1fr 90px 40px 34px;
  gap: 6px;
  align-items: center;
}

.variable-head {
  padding: 0 2px 5px;
  color: #80919b;
  font-size: 9px;
  font-weight: 750;
  text-transform: uppercase;
}

.var-row {
  margin-bottom: 7px;
}

.mini-check {
  display: grid;
  place-items: center;
}

.mini-check input {
  width: 16px;
  height: 16px;
  accent-color: #0b8779;
}

.delete-row {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  border: 1px solid #efd5d6;
  border-radius: 7px;
  background: #fff8f8;
  color: #bf4249;
  cursor: pointer;
}

.info-callout {
  display: flex;
  align-items: flex-start;
  gap: 9px;
  padding: 11px 12px;
  border: 1px solid #d6e7e8;
  border-radius: 9px;
  background: #f2f8f8;
  color: #58747b;
  font-size: 10.5px;
  line-height: 1.5;
}

.info-callout > .pi {
  margin-top: 2px;
  color: #0b817b;
}

/* selected */
.selected-element-card {
  display: flex;
  align-items: center;
  gap: 11px;
  background: linear-gradient(135deg, #fffaf5, #fff);
  border-color: #f1d6bf;
}

.transition-card {
  background: linear-gradient(135deg, #f6fbfc, #fff);
  border-color: #d7e7ea;
}

.selected-icon {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  flex: 0 0 40px;
  border-radius: 10px;
  color: #f17416;
  background: #fff0e4;
  font-size: 17px;
}

.transition-card .selected-icon {
  color: #097b78;
  background: #e8f5f4;
}

.selected-copy small,
.selected-copy strong {
  display: block;
}

.selected-copy small {
  color: #8a9aa4;
  font-size: 9px;
  font-weight: 750;
  text-transform: uppercase;
}

.selected-copy strong {
  margin-top: 3px;
  color: #27485e;
  font-size: 12px;
  word-break: break-all;
}

.flow-info {
  display: grid;
  grid-template-columns: minmax(0,1fr) 24px minmax(0,1fr);
  gap: 8px;
  align-items: center;
  margin-bottom: 13px;
}

.flow-info > div {
  min-width: 0;
  padding: 10px;
  border: 1px solid #dce5e9;
  border-radius: 9px;
  background: #fff;
}

.flow-info span:not(.flow-arrow) {
  display: block;
  margin-bottom: 3px;
  color: #84949d;
  font-size: 9px;
  font-weight: 750;
  text-transform: uppercase;
}

.flow-info strong {
  display: block;
  overflow: hidden;
  color: #29495f;
  font-size: 11px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.flow-arrow {
  color: #f27618;
  text-align: center;
}

.outcome-icon {
  color: #f27618;
  background: #fff0e5;
}

.inline-action {
  display: grid;
  grid-template-columns: minmax(0,1fr) auto;
  gap: 7px;
}

.parallel-card {
  background: linear-gradient(135deg, #f7fbfb, #fff);
}

.parallel-icon {
  width: 36px;
  height: 36px;
  display: grid;
  place-items: center;
  margin-bottom: 9px;
  border-radius: 9px;
  background: #e7f4f3;
  color: #0b8078;
}

.parallel-copy strong,
.parallel-copy span {
  display: block;
}

.parallel-copy strong {
  color: #294b61;
  font-size: 12.5px;
}

.repair-flow-btn {
  margin-top: 8px;
}

.parallel-copy span {
  margin: 4px 0 12px;
  color: #7b8e99;
  font-size: 10.5px;
  line-height: 1.45;
}

/* form layout */
.field-label {
  margin-top: 10px;
}

.field-label.no-margin {
  margin: 0;
}

.two-col {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}

.route-title-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin: 12px 0 6px;
}

.form-manage { margin-top: 8px; }
.wf-type { display: grid; gap: 8px; margin-bottom: 12px; }
.wf-type label { display: flex; gap: 10px; align-items: flex-start; padding: 10px 12px; border: 1px solid #d9e6ea; border-radius: 10px; cursor: pointer; background: #fff; }
.wf-type label.on { border-color: #00636D; background: #f0f8f8; box-shadow: 0 0 0 3px rgba(0, 99, 109, .08); }
.wf-type input[type="radio"] { flex: none; width: 16px; height: 16px; min-width: 0; margin: 2px 0 0; padding: 0; accent-color: #00636D; box-shadow: none; }
.wf-type span { display: grid; gap: 2px; }
.wf-type b { color: #12324a; font-size: 13px; }
.wf-type small { color: #5f7787; font-size: 11.5px; line-height: 1.35; }
.field-hint { display: block; margin-top: 6px; color: #5f7787; font-size: 11.5px; line-height: 1.4; }

.text-button {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  border: 0;
  background: transparent;
  color: #07817a;
  font: inherit;
  font-size: 10.5px;
  font-weight: 750;
  cursor: pointer;
}

.param-row {
  display: grid;
  grid-template-columns: 1fr 1fr 34px;
  gap: 6px;
  margin-bottom: 7px;
}

.action-grid {
  display: grid;
  grid-template-columns: repeat(2,minmax(0,1fr));
  gap: 7px;
}

.action-check {
  position: relative;
  cursor: pointer;
}

.action-check input {
  position: absolute;
  opacity: 0;
  pointer-events: none;
}

.action-box {
  min-height: 36px;
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 0 9px;
  border: 1px solid #d8e2e7;
  border-radius: 8px;
  background: #fff;
  color: #5b7383;
  font-size: 10.5px;
  font-weight: 750;
  transition: .15s ease;
}

.action-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #bdc9cf;
}

.action-check.selected .action-box {
  border-color: #9ccfc8;
  background: #f0f8f6;
  color: #147064;
}

.action-check.selected .action-dot {
  background: #148a76;
}

.custom-outcome {
  margin-top: 9px;
}

/* MODAL */
.fields-empty { padding: 6px 2px 10px; color: #80919b; font-size: 12px; }
.rule-row { display: grid; grid-template-columns: 30px minmax(0,1fr) minmax(0,1fr) 30px; gap: 6px; align-items: center; margin-bottom: 10px; padding-bottom: 10px; border-bottom: 1px dashed #e3e9ec; }
.rule-row .rule-field { grid-column: 2 / 4; }
.rule-row .rule-op { grid-column: 2; }
.rule-row .rule-value { grid-column: 3; }
.rule-row .icon-button { grid-column: 4; grid-row: 1; }
.rule-row select, .rule-row input { min-width: 0; width: 100%; }
.rule-join { font-size: 12px; font-weight: 700; color: #00636D; }
.rule-row .icon-button { width: 30px; height: 30px; border: 1px solid #e3e9ec; border-radius: 6px; background: #fff; color: #80919b; cursor: pointer; }
.rule-row .icon-button:hover { color: #E87722; border-color: #E87722; }
:deep(.rule-badge) { white-space: nowrap; padding: 2px 8px; border-radius: 999px; background: #fff4ec; border: 1px solid #E87722; color: #a24f0f; font: 600 11px/1.4 Inter, Arial, sans-serif; transform: translate(-50%, -50%); pointer-events: none; }
.rule-hint.warn { color: #a24f0f; }
.rule-hint { margin: 8px 2px 0; font-size: 12px; color: #6b7c86; line-height: 1.45; }
.legacy-fields { display: flex; gap: 10px; margin: 4px 0 8px; padding: 10px 12px; border: 1px solid #f6d7bd; border-left: 3px solid #E87722; border-radius: 8px; background: #fff8f2; font-size: 12px; color: #5b3a1d; }
.legacy-fields .pi { color: #E87722; margin-top: 2px; }
.legacy-fields .text-button { display: block; margin-top: 6px; color: #00636D; font-weight: 600; }
.module-values { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; margin: 0 0 12px; font-size: 12.5px; color: #5f7787; }
.chip-add { display: inline-flex; align-items: center; gap: 5px; padding: 5px 10px; border: 1px solid #b9d9dc; border-radius: 999px; background: #eef7f7; color: #00636D; font: inherit; font-weight: 600; cursor: pointer; }
.chip-add:hover { background: #dff0f1; }
.chip-add .pi { font-size: 10px; }
.field-chips { list-style: none; margin: 0 0 10px; padding: 0; display: grid; gap: 6px; }
.field-chips li { display: flex; justify-content: space-between; gap: 8px; padding: 7px 9px; border: 1px solid #e1e8ec; border-radius: 8px; font-size: 12px; }
.field-chips b { color: #173b57; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.field-chips span { color: #49657b; white-space: nowrap; }
.modal-card.fields-card { height: auto; max-height: min(720px, 88vh); width: min(1100px, 94vw); }
.fields-body { padding: 14px 17px 16px; overflow: auto; }
.fields-table-wrap { overflow-x: auto; }
.fields-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.fields-table th { text-align: left; padding: 7px 6px; background: #f3f7f9; color: #49657b; font-size: 11.5px; font-weight: 750; }
.fields-table td { padding: 6px; border-top: 1px solid #e1e8ec; }
.fields-table .c { text-align: center; }
.fields-table input:not([type=checkbox]), .fields-table select { width: 100%; height: 34px; padding: 0 8px; border: 1px solid #cbd9e1; border-radius: 7px; font: inherit; color: #173b57; background: #fff; }
.fields-table input[type=checkbox] { width: 16px; height: 16px; accent-color: #00636D; }
.fields-table tr.bad td { background: #fff7f0; }
.fields-table .muted { color: #a3b1ba; }
.field-problems { margin: 10px 0 0; padding: 0; list-style: none; color: #b5530d; font-size: 12.5px; display: grid; gap: 4px; }
.fields-actions { display: flex; align-items: center; gap: 8px; margin-top: 12px; }
.fields-actions .spacer { flex: 1; }
.fields-foot { margin: 8px 0 0; color: #80919b; font-size: 12px; text-align: right; }

.version-chip {
  width: 100%;
  height: 36px;
  border: 1px solid #b9d9d6;
  border-radius: 8px;
  background: #eef7f6;
  color: #00636D;
  font-weight: 800;
  font-size: 13px;
  cursor: pointer;
}

.version-chip:hover { background: #e2f1ef; }

.version-banner {
  flex: 0 0 auto;
  display: flex;
  align-items: center;
  gap: 10px;
  margin: 0 0 8px;
  padding: 9px 12px;
  border: 1px solid #f5c79f;
  border-radius: 10px;
  background: #fff4ea;
  color: #7a3d0a;
  font-size: 13px;
}

.version-banner > .pi { color: #E87722; font-size: 16px; }
.version-banner > span:nth-child(2) { flex: 1; }

.modal-card.versions-card { height: auto; max-height: min(640px, 84vh); width: min(860px, 92vw); }
.versions-body { padding: 14px 17px 18px; overflow: auto; }
.versions-note { margin: 0 0 12px; color: #49657b; font-size: 13px; line-height: 1.5; }
.versions-empty { padding: 18px; color: #49657b; text-align: center; }
.versions-table { width: 100%; border-collapse: collapse; font-size: 13px; }
.versions-table th { text-align: left; padding: 8px 10px; background: #f3f7f9; color: #49657b; font-size: 11.5px; font-weight: 750; }
.versions-table td { padding: 8px 10px; border-top: 1px solid #e1e8ec; color: #173b57; }
.versions-table .num { text-align: right; }
.versions-table .act { text-align: right; }
.current-tag { margin-left: 6px; padding: 2px 7px; border-radius: 999px; background: #00636D; color: #fff; font-size: 10.5px; font-weight: 800; }
.btn-sm { height: 30px; padding: 0 10px; font-size: 12px; }

.modal {
  position: fixed;
  inset: 0;
  z-index: 9998;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 28px;
  background: rgba(8, 24, 38, .48);
  backdrop-filter: blur(2px);
}

.modal-card {
  width: min(1100px, 92vw);
  height: min(760px, 84vh);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border: 1px solid #d9e3e8;
  border-radius: 14px;
  background: #fff;
  box-shadow: 0 26px 70px rgba(0, 28, 48, .25);
}

.modal-head {
  min-height: 64px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 17px;
  border-bottom: 1px solid #e1e8ec;
}

.modal-head small {
  display: block;
  color: #f27719;
  font-size: 9px;
  font-weight: 800;
  text-transform: uppercase;
}

.modal-head h3 {
  margin: 2px 0 0;
  color: #173b57;
  font-size: 17px;
}

.modal-close {
  width: 34px;
  height: 34px;
  display: grid;
  place-items: center;
  border: 1px solid #d6e0e5;
  border-radius: 8px;
  background: #fff;
  color: #536d7c;
  cursor: pointer;
}

.modal-card textarea {
  flex: 1 1 auto;
  width: 100%;
  min-height: 0;
  padding: 15px;
  border: 0;
  outline: 0;
  resize: none;
  color: #263f51;
  background: #f8fafb;
  font-family: Consolas, "Courier New", monospace;
  font-size: 11px;
  line-height: 1.5;
  white-space: pre;
}

/* TOAST */
.toast {
  position: fixed;
  right: 24px;
  bottom: 24px;
  z-index: 9999;
  display: flex;
  align-items: center;
  gap: 9px;
  max-width: 420px;
  padding: 12px 16px;
  border-radius: 9px;
  color: #fff;
  font-size: 12px;
  font-weight: 700;
  box-shadow: 0 10px 28px rgba(0,0,0,.16);
}

.toast-success {
  background: #17715b;
}

.toast-error {
  background: #a13d43;
}

/* scroll */
.properties-body::-webkit-scrollbar {
  width: 8px;
}
.properties-body::-webkit-scrollbar-track {
  background: transparent;
}
.properties-body::-webkit-scrollbar-thumb {
  border-radius: 8px;
  background: #c9d5da;
}


.canvas-wrap::-webkit-scrollbar {
  width: 10px;
  height: 10px;
}
.canvas-wrap::-webkit-scrollbar-track {
  background: #f2f6f8;
}
.canvas-wrap::-webkit-scrollbar-thumb {
  border: 2px solid #f2f6f8;
  border-radius: 10px;
  background: #bccbd3;
}

/* bpmn.io branding hidden only */
.canvas :deep(.bjs-powered-by) {
  display: none !important;
}

/* responsive */
@media (max-width: 1180px) {
  .designer-shell {
    grid-template-columns: minmax(0,1fr) 350px;
  }

  .workflow-summary-card {
    grid-template-columns: 1fr 1.3fr .45fr .65fr;
  }

  .active-switch {
    grid-column: 1 / -1;
  }
}

@media (max-width: 900px) {
  .designer-page {
    height: auto;
    min-height: 100vh;
    overflow: visible;
  }

  .finova-page-head {
    align-items: flex-start;
    flex-direction: column;
  }

  .page-head-actions {
    flex-wrap: wrap;
  }

  .workflow-summary-card {
    grid-template-columns: 1fr 1fr;
  }

  .designer-shell {
    min-height: 900px;
    grid-template-columns: 1fr;
  }

  .canvas-panel {
    min-height: 560px;
    border-right: 0;
    border-bottom: 1px solid #dce5ea;
  }

  .properties-panel {
    min-height: 540px;
  }
}

/* the canvas always takes the full width; Properties slides over its right edge */
.designer-shell,
.designer-shell.properties-collapsed {
  grid-template-columns: minmax(0, 1fr);
}

.designer-shell .properties-panel {
  position: absolute;
  top: 51px; /* below the canvas toolbar, which stays usable */
  bottom: 0;
  inset-inline-end: 0;
  z-index: 20;
  width: min(390px, 92%);
  border-inline-start: 1px solid #d7e2e8;
  box-shadow: -18px 0 40px -18px rgba(16, 51, 75, .35);
  animation: props-in .2s ease-out;
}

/* steps: a soft fill, teal when selected, a light tint on hover */
.designer-shell :deep(.djs-element.djs-shape .djs-visual > rect) {
  fill: #fff !important;
  stroke: #9FB6C0 !important;
  stroke-width: 1.6px !important;
}

.designer-shell :deep(.djs-element.djs-shape.hover .djs-visual > rect) {
  fill: #F2F8F9 !important;
  stroke: #2E9AA3 !important;
}

.designer-shell :deep(.djs-element.djs-shape.selected .djs-visual > rect) {
  fill: #E6F3F4 !important;
  stroke: #00636D !important;
  stroke-width: 3px !important;
}

.designer-shell :deep(.djs-element.djs-shape.selected .djs-visual > text) {
  fill: #014B55 !important;
}

.designer-shell :deep(.djs-element.selected .djs-outline),
.designer-shell :deep(.djs-element.hover .djs-outline) {
  visibility: hidden;
}

/* no dark focus frame around the canvas after a click */
.designer-shell :deep(.djs-container:focus),
.designer-shell :deep(.djs-container:focus-visible),
.designer-shell :deep(svg:focus) {
  outline: none;
}

@keyframes props-in {
  from { transform: translateX(24px); opacity: 0; }
  to { transform: none; opacity: 1; }
}
</style>