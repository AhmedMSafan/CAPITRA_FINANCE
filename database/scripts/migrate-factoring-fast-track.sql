-- =====================================================================
-- Capitra: Factoring fast track (EF migration 20261103203000_SeedFactoringFastTrack).
--   New workflow FACTORING_FAST_TRACK, released: Approval (role FAST_TRACK_APPROVER, Committee Decision page)
--   -> Contract and Limit Creation (role LEGAL_OFFICER, Legal Review page; ACCEPT creates the credit limit).
--   REJECT on either step ends the workflow. The user picks Normal or Fast track when starting each agreement.
-- Run once on CAPITRA_DB; it skips what is already applied.
-- =====================================================================
START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "SYS_EF_MIGRATION_HISTORY" WHERE "MigrationId" = '20261103203000_SeedFactoringFastTrack') THEN
    INSERT INTO "WFE_WORKFLOW_DEFINITIONS"
        ("WORKFLOW_CODE", "WORKFLOW_NAME", "VERSION_NO", "DEFINITION_STATUS", "BPMN_XML", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "ROW_VERSION")
    VALUES ('FACTORING_FAST_TRACK', 'Factoring Fast Track', '1', 'released', '<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_FINOVA" targetNamespace="http://finova/workflow">
  <bpmn:process id="Process_FINOVA" name="FINOVA Workflow Engine" isExecutable="true" finova:variables="[{&#34;name&#34;:&#34;entity_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_name&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_number&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_type&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true}]">
    <bpmn:startEvent id="StartEvent_1" name="Start" finova:stepCode="" finova:stepType="Start" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
      <bpmn:outgoing>Flow_1</bpmn:outgoing>
    </bpmn:startEvent>
    <bpmn:userTask id="Task_Approval" name="Approval" finova:stepCode="FAST_TRACK_APPROVAL" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="FAST_TRACK_APPROVER" finova:route="factoring-committee-decision" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
      <bpmn:incoming>Flow_1</bpmn:incoming>
      <bpmn:outgoing>Flow_Approve</bpmn:outgoing>
      <bpmn:outgoing>Flow_ApprovalReject</bpmn:outgoing>
    </bpmn:userTask>
    <bpmn:userTask id="Task_ContractLimit" name="Contract and Limit Creation" finova:stepCode="LEGAL_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="LEGAL_OFFICER" finova:route="factoring-legal-review" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT">
      <bpmn:incoming>Flow_Approve</bpmn:incoming>
      <bpmn:outgoing>Flow_LimitCreated</bpmn:outgoing>
      <bpmn:outgoing>Flow_ContractReject</bpmn:outgoing>
    </bpmn:userTask>
    <bpmn:endEvent id="EndEvent_1" name="End" finova:stepCode="" finova:stepType="End" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
      <bpmn:incoming>Flow_LimitCreated</bpmn:incoming>
      <bpmn:incoming>Flow_ApprovalReject</bpmn:incoming>
      <bpmn:incoming>Flow_ContractReject</bpmn:incoming>
    </bpmn:endEvent>
    <bpmn:sequenceFlow id="Flow_1" name="" sourceRef="StartEvent_1" targetRef="Task_Approval" finova:outcome="" />
    <bpmn:sequenceFlow id="Flow_Approve" name="ACCEPT" sourceRef="Task_Approval" targetRef="Task_ContractLimit" finova:outcome="ACCEPT" />
    <bpmn:sequenceFlow id="Flow_ApprovalReject" name="REJECT" sourceRef="Task_Approval" targetRef="EndEvent_1" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_LimitCreated" name="ACCEPT" sourceRef="Task_ContractLimit" targetRef="EndEvent_1" finova:outcome="ACCEPT" />
    <bpmn:sequenceFlow id="Flow_ContractReject" name="REJECT" sourceRef="Task_ContractLimit" targetRef="EndEvent_1" finova:outcome="REJECT" />
  </bpmn:process>
  <bpmndi:BPMNDiagram id="BPMNDiagram_1">
    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_FINOVA">
      <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1">
        <dc:Bounds x="152" y="182" width="36" height="36" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="158" y="218" width="24" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_Approval_di" bpmnElement="Task_Approval">
        <dc:Bounds x="250" y="160" width="140" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_ContractLimit_di" bpmnElement="Task_ContractLimit">
        <dc:Bounds x="470" y="160" width="140" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1">
        <dc:Bounds x="692" y="182" width="36" height="36" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="700" y="218" width="20" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNShape>
      <bpmndi:BPMNEdge id="Flow_1_di" bpmnElement="Flow_1">
        <di:waypoint x="188" y="200" />
        <di:waypoint x="250" y="200" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_Approve_di" bpmnElement="Flow_Approve">
        <di:waypoint x="390" y="200" />
        <di:waypoint x="470" y="200" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="408" y="182" width="45" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_LimitCreated_di" bpmnElement="Flow_LimitCreated">
        <di:waypoint x="610" y="200" />
        <di:waypoint x="692" y="200" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="628" y="182" width="45" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_ApprovalReject_di" bpmnElement="Flow_ApprovalReject">
        <di:waypoint x="320" y="240" />
        <di:waypoint x="320" y="300" />
        <di:waypoint x="710" y="300" />
        <di:waypoint x="710" y="218" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="330" y="282" width="43" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_ContractReject_di" bpmnElement="Flow_ContractReject">
        <di:waypoint x="540" y="240" />
        <di:waypoint x="540" y="270" />
        <di:waypoint x="700" y="270" />
        <di:waypoint x="700" y="216" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="550" y="252" width="43" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
    </bpmndi:BPMNPlane>
  </bpmndi:BPMNDiagram>
</bpmn:definitions>',
            'Factoring application, fast track: approval by one approver, then contract and limit creation.',
            TRUE, 'SYSTEM', now(), 0)
    ON CONFLICT ("WORKFLOW_CODE") DO NOTHING;

    INSERT INTO "SYS_EF_MIGRATION_HISTORY" ("MigrationId", "ProductVersion")
    VALUES ('20261103203000_SeedFactoringFastTrack', '10.0.13');
    END IF;
END $EF$;
COMMIT;
