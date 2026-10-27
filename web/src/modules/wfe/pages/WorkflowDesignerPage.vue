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
        <button class="btn btn-ghost" @click="router.push('/workflowengine/registry')">
          <span class="pi pi-arrow-left"></span>
          Workflows
        </button>

        <button class="btn btn-ghost" @click="fit">
          <span class="pi pi-expand"></span>
          Fit
        </button>

        <button class="btn btn-ghost" @click="showXml=true">
          <span class="pi pi-code"></span>
          BPMN XML
        </button>

        <button class="btn btn-primary" :disabled="saving" @click="save">
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
        <input v-model="form.dgrmVersion" placeholder="1" />
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
                <span class="section-icon pi pi-database"></span>
                <div>
                  <strong>Workflow Variables</strong>
                  <small>Values available to routes and runtime processing.</small>
                </div>
              </div>

              <div class="variable-head">
                <span>Name</span>
                <span>Type</span>
                <span>Req.</span>
                <span></span>
              </div>

              <div
                v-for="(v,i) in variables"
                :key="i"
                class="var-row"
              >
                <input v-model="v.name" placeholder="Variable name" />

                <select v-model="v.type">
                  <option>String</option>
                  <option>Number</option>
                  <option>Decimal</option>
                  <option>Boolean</option>
                  <option>Date</option>
                </select>

                <label class="mini-check">
                  <input v-model="v.required" type="checkbox" />
                </label>

                <button
                  class="delete-row"
                  title="Remove variable"
                  @click="variables.splice(i,1)"
                >
                  <span class="pi pi-trash"></span>
                </button>
              </div>

              <button
                class="btn btn-secondary btn-full"
                @click="variables.push({name:'',type:'String',required:false})"
              >
                <span class="pi pi-plus"></span>
                Add Variable
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

              <label class="field-label">Assignee / Role / Variable</label>
              <input v-model="step.assignee" placeholder="e.g. CREDIT_MANAGER" />
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
import {createWorkflowDefinition,getWorkflowDefinition,updateWorkflowDefinition} from '../services/workflowDefinitionService'

type Var={name:string,type:string,required:boolean}; type Param={name:string,value:string};
type Step={stepCode:string,stepName:string,stepType:string,assignmentType:string,assignee:string,route:string,routeParameters:Param[],actions:string[]}
const route=useRoute(),router=useRouter(),canvas=ref<HTMLElement|null>(null),saving=ref(false),active=ref(true),selectedId=ref(''),selectedType=ref(''),message=ref(''),messageType=ref<'success'|'error'>('success'),showXml=ref(false),xmlPreview=ref(''),customAction=ref(''),flowOutcome=ref(''),customFlowOutcome=ref(''),selectedFlowSource=ref(''),selectedFlowTarget=ref(''),leftPanelOpen=ref(false),rightPanelOpen=ref(false),zoomPercent=ref(100)
const variables=reactive<Var[]>([]),steps=reactive<Record<string,Step>>({}); let modeler:any=null
const form=reactive({dgrmId:0,dgrmName:'',dgrmVersion:'1',dgrmStatus:'draft',dgrmContent:'',dgrmCreatedBy:'SYSTEM',dgrmLastUpdateBy:'SYSTEM',workflowCode:'',description:'',isActive:'Y'})
const actionCatalog=ref(['CLAIM','ACCEPT','APPROVE','REJECT','RETURN','COMPLETE'])
function emptyStep():Step{return{stepCode:'',stepName:'',stepType:'HumanTask',assignmentType:'Role',assignee:'',route:'',routeParameters:[],actions:['CLAIM','COMPLETE']}}
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

  registry.getAll().forEach((e:any)=>{
    const bo=e.businessObject
    if(!bo||(!e.type?.includes('Task')&&!e.type?.includes('Gateway')&&!e.type?.includes('Event')))return

    const s=emptyStep()
    s.stepName=bo.name||''
    s.stepCode=bo.stepCode||''
    s.stepType=bo.stepType||inferType(e.type)
    s.assignmentType=bo.assignmentType||((s.stepType==='HumanTask')?'Role':'None')
    s.assignee=bo.assignee||''
    s.route=bo.route||''

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

  if(process)process.variables=JSON.stringify(variables)

  Object.entries(steps).forEach(([id,s])=>{
    const el=registry.get(id)
    if(!el)return

    modeling.updateProperties(el,{
      name:s.stepName||el.businessObject.name,
      stepCode:s.stepCode,
      stepType:s.stepType,
      assignmentType:s.assignmentType,
      assignee:s.assignee,
      route:s.route,
      routeParameters:JSON.stringify(s.routeParameters),
      actions:s.actions.join(',')
    })
  })
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

  const minX=Math.min(...elements.map((e:any)=>e.x))
  const minY=Math.min(...elements.map((e:any)=>e.y))
  const maxX=Math.max(...elements.map((e:any)=>e.x+e.width))
  const maxY=Math.max(...elements.map((e:any)=>e.y+e.height))

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

function bindSelection(){modeler.on('selection.changed',(ev:any)=>{
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

async function load(){
  let xml=starter
  const id=route.params.id

  if(id&&id!=='new'){
    const w=await getWorkflowDefinition(Number(id))
    Object.assign(form,w)
    active.value=form.isActive==='Y'
    xml=form.dgrmContent||starter
  }

  await modeler.importXML(removeSavedViewbox(xml))
  loadMetadataFromXml()

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

    Object.assign(form,saved)

    msg('Workflow and designer position saved successfully.')

    if(route.params.id==='new'||!route.params.id)
      await router.replace(`/workflowengine/designer/${form.dgrmId}`)
  }
  catch(e){
    msg(
      e instanceof Error
        ? e.message
        : 'Save failed.',
      true
    )
  }
  finally{
    saving.value=false
  }
}
onMounted(async()=>{try{modeler=new BpmnModeler({container:canvas.value??undefined,moddleExtensions:{finova:finovaWorkflowModdle}});bindSelection();await load();if(canvas.value)canvasObserver.observe(canvas.value)}catch(e){msg(e instanceof Error?e.message:'Designer initialization failed.',true)}});onBeforeUnmount(()=>{canvasObserver.disconnect();clearTimeout(refitTimer);modeler?.destroy()})
</script>
<style scoped>
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