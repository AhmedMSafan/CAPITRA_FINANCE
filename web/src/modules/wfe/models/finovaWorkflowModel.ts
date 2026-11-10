const finovaWorkflowModdle = {
  name: 'FINOVA Workflow Engine',
  uri: 'http://finova/workflow',
  prefix: 'finova',
  xml: { tagAlias: 'lowerCase' },
  types: [
    {
      name: 'WorkflowMetadata',
      extends: ['bpmn:Process'],
      properties: [
        { name: 'variables', isAttr: true, type: 'String' },
        { name: 'workflowType', isAttr: true, type: 'String' },
        { name: 'documentCode', isAttr: true, type: 'String' }
      ]
    },
    {
      name: 'StepMetadata',
      extends: [
        'bpmn:Task', 'bpmn:UserTask', 'bpmn:ServiceTask', 'bpmn:ManualTask',
        'bpmn:BusinessRuleTask', 'bpmn:ScriptTask', 'bpmn:SendTask', 'bpmn:ReceiveTask',
        'bpmn:ExclusiveGateway', 'bpmn:InclusiveGateway', 'bpmn:ParallelGateway',
        'bpmn:StartEvent', 'bpmn:EndEvent', 'bpmn:IntermediateCatchEvent', 'bpmn:IntermediateThrowEvent'
      ],
      properties: [
        { name: 'stepCode', isAttr: true, type: 'String' },
        { name: 'ruleCode', isAttr: true, type: 'String' },
        { name: 'formCode', isAttr: true, type: 'String' },
        { name: 'stepType', isAttr: true, type: 'String' },
        { name: 'assignmentType', isAttr: true, type: 'String' },
        { name: 'assignee', isAttr: true, type: 'String' },
        { name: 'route', isAttr: true, type: 'String' },
        { name: 'routeParameters', isAttr: true, type: 'String' },
        { name: 'actions', isAttr: true, type: 'String' },
        { name: 'slaDuration', isAttr: true, type: 'String' },
        { name: 'slaUnit', isAttr: true, type: 'String' },
        { name: 'slaRemindHours', isAttr: true, type: 'String' },
        { name: 'slaEscalateHours', isAttr: true, type: 'String' },
        { name: 'slaEscalation', isAttr: true, type: 'String' },
        { name: 'slaEscalateType', isAttr: true, type: 'String' },
        { name: 'slaEscalateTo', isAttr: true, type: 'String' }
      ]
    },
    {
      name: 'FlowMetadata',
      extends: ['bpmn:SequenceFlow'],
      properties: [
        { name: 'outcome', isAttr: true, type: 'String' }
      ]
    }
  ]
}

export default finovaWorkflowModdle
