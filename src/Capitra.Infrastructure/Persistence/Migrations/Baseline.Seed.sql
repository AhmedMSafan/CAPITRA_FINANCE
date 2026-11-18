

INSERT INTO public."ADM_LKP_BUSINESS_LINES" ("CODE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE") VALUES ('MICROFINANCE', 'SYSTEM', '2026-11-18 17:35:31.231576+00', NULL, NULL, 1, 'التمويل متناهي الصغر', 'Microfinance', NULL, 10, true);
INSERT INTO public."ADM_LKP_BUSINESS_LINES" ("CODE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE") VALUES ('CONSUMER_FINANCE', 'SYSTEM', '2026-11-18 17:35:31.231576+00', NULL, NULL, 1, 'التمويل الاستهلاكي', 'Consumer Finance', NULL, 11, true);

INSERT INTO public."ADM_LKP_DOCUMENT_STATUS_CODES" ("STATUS_ID", "STATUS_CODE_VALUE", "STATUS_NAME", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES (1, 'RECEIVED', 'Received', 'Document received', true, 'SYSTEM', '2026-11-18 17:35:20.843279+00', NULL, NULL, 0);
INSERT INTO public."ADM_LKP_DOCUMENT_STATUS_CODES" ("STATUS_ID", "STATUS_CODE_VALUE", "STATUS_NAME", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES (2, 'VERIFIED', 'Verified', 'Document verified', true, 'SYSTEM', '2026-11-18 17:35:20.843279+00', NULL, NULL, 0);
INSERT INTO public."ADM_LKP_DOCUMENT_STATUS_CODES" ("STATUS_ID", "STATUS_CODE_VALUE", "STATUS_NAME", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES (3, 'REJECTED', 'Rejected', 'Document rejected', true, 'SYSTEM', '2026-11-18 17:35:20.843279+00', NULL, NULL, 0);

INSERT INTO public."ADM_LKP_DOCUMENT_TYPES" ("DOCUMENT_TYPE_CODE", "DOCUMENT_CATEGORY", "ALLOWED_EXTENSIONS", "MAX_FILE_SIZE_MB", "IS_MANDATORY", "IS_EXPIRY_REQUIRED", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION", "DOCUMENT_TYPE_NAME", "DESCRIPTION", "IS_ACTIVE", "DOCUMENT_TYPE_ID", "DOCUMENT_TYPE_NAME_AR", "GROUP_CODE", "SORT_ORDER") VALUES ('AGREEMENT', NULL, NULL, NULL, false, false, 'SYSTEM', '2026-11-18 17:35:20.843279+00', NULL, NULL, 0, 'Agreement', NULL, true, 1, NULL, 'FACTORING', 1);
INSERT INTO public."ADM_LKP_DOCUMENT_TYPES" ("DOCUMENT_TYPE_CODE", "DOCUMENT_CATEGORY", "ALLOWED_EXTENSIONS", "MAX_FILE_SIZE_MB", "IS_MANDATORY", "IS_EXPIRY_REQUIRED", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION", "DOCUMENT_TYPE_NAME", "DESCRIPTION", "IS_ACTIVE", "DOCUMENT_TYPE_ID", "DOCUMENT_TYPE_NAME_AR", "GROUP_CODE", "SORT_ORDER") VALUES ('SUPPORTING_DOCUMENT', NULL, NULL, NULL, false, false, 'SYSTEM', '2026-11-18 17:35:20.843279+00', NULL, NULL, 0, 'Supporting Document', NULL, true, 2, NULL, 'FACTORING', 2);
INSERT INTO public."ADM_LKP_DOCUMENT_TYPES" ("DOCUMENT_TYPE_CODE", "DOCUMENT_CATEGORY", "ALLOWED_EXTENSIONS", "MAX_FILE_SIZE_MB", "IS_MANDATORY", "IS_EXPIRY_REQUIRED", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION", "DOCUMENT_TYPE_NAME", "DESCRIPTION", "IS_ACTIVE", "DOCUMENT_TYPE_ID", "DOCUMENT_TYPE_NAME_AR", "GROUP_CODE", "SORT_ORDER") VALUES ('FINANCIAL_STATEMENT', NULL, NULL, NULL, false, false, 'SYSTEM', '2026-11-18 17:35:20.843279+00', NULL, NULL, 0, 'Financial Statement', NULL, true, 3, NULL, 'FACTORING', 3);
INSERT INTO public."ADM_LKP_DOCUMENT_TYPES" ("DOCUMENT_TYPE_CODE", "DOCUMENT_CATEGORY", "ALLOWED_EXTENSIONS", "MAX_FILE_SIZE_MB", "IS_MANDATORY", "IS_EXPIRY_REQUIRED", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION", "DOCUMENT_TYPE_NAME", "DESCRIPTION", "IS_ACTIVE", "DOCUMENT_TYPE_ID", "DOCUMENT_TYPE_NAME_AR", "GROUP_CODE", "SORT_ORDER") VALUES ('OTHER', NULL, NULL, NULL, false, false, 'SYSTEM', '2026-11-18 17:35:20.843279+00', NULL, NULL, 0, 'Other', NULL, true, 4, NULL, 'FACTORING', 4);

INSERT INTO public."GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "MODULE_NAME_AR", "LAST_NO", "DIGITS", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('MPA', 2026, 'MTG', 'Mortgage Portfolio Acquisition', NULL, 0, 8, true, 'SYSTEM', '2026-11-18 17:35:22.128109+00', NULL, NULL, 1);
INSERT INTO public."GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "MODULE_NAME_AR", "LAST_NO", "DIGITS", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('FCT', 2026, 'FCT', 'Factoring', NULL, 0, 8, true, 'SYSTEM', '2026-11-18 17:35:22.128109+00', NULL, NULL, 1);
INSERT INTO public."GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "MODULE_NAME_AR", "LAST_NO", "DIGITS", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('CHW', 2026, 'FCT', 'Cheques Wallet', NULL, 0, 8, true, 'SYSTEM', '2026-11-18 17:35:22.128109+00', NULL, NULL, 1);
INSERT INTO public."GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "MODULE_NAME_AR", "LAST_NO", "DIGITS", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('COL', 2026, 'COL', 'Collection and Repayments', NULL, 0, 8, true, 'SYSTEM', '2026-11-18 17:35:22.128109+00', NULL, NULL, 1);
INSERT INTO public."GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "MODULE_NAME_AR", "LAST_NO", "DIGITS", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('GLM', 2026, 'GL', 'Manual GL journals', NULL, 0, 8, true, 'SYSTEM', '2026-11-18 17:35:22.128109+00', NULL, NULL, 1);
INSERT INTO public."GL_TRAN_SET_SEQUENCES" ("MODULE_CODE", "YEAR_NO", "APP_CODE", "MODULE_NAME_EN", "MODULE_NAME_AR", "LAST_NO", "DIGITS", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('LND', 2026, 'LND', 'Retail Lending', 'التمويل للأفراد', 0, 8, true, 'SYSTEM', '2026-11-18 17:35:31.807725+00', NULL, NULL, 1);

INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('ELECTRONICS', 'الأجهزة الإلكترونية والكهربائية', 'Electronics & electrical devices', 'TV, laptop, tablet, camera, gaming', 10, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);
INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('MOBILE', 'الهواتف المحمولة', 'Mobile phones', 'Phones and smart watches', 20, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);
INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('HOME_APPLIANCES', 'الأجهزة المنزلية', 'Household appliances', 'Fridge, washer, air conditioner, cooker, water heater', 30, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);
INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('FURNITURE', 'الأثاث والمفروشات', 'Furniture & home fittings', 'Bedrooms, living rooms, kitchens, carpets', 40, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);
INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('HOME_FINISHING', 'التشطيبات', 'Home finishing', 'Ceramics, sanitary ware, paint, doors, aluminium', 50, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);
INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('VEHICLES', 'السيارات ووسائل النقل', 'Cars & vehicles', 'New or used cars, motorcycles', 60, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);
INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('FASHION', 'الملابس والأحذية والحقائب والساعات', 'Clothing, shoes, bags & watches', NULL, 70, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);
INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('EDUCATION', 'الخدمات التعليمية', 'Education services', 'School, university and course fees', 80, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);
INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('MEDICAL', 'الخدمات الطبية', 'Medical services', 'Surgery, dental, optical, treatment', 90, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);
INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('TRAVEL', 'السفر والسياحة', 'Travel & tourism', 'Trips, Hajj and Umrah', 100, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);
INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('CLUBS', 'عضويات الأندية', 'Clubs & sports memberships', NULL, 110, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);
INSERT INTO public."LND_LKP_GOODS_CATEGORIES" ("CODE", "NAME_AR", "NAME_EN", "DESCRIPTION", "DISPLAY_ORDER", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES ('OTHER', 'سلع معمرة أخرى', 'Other durable goods', 'Anything not listed, with a description', 120, true, 'SYSTEM', '2026-11-18 17:35:33.351127+00', NULL, NULL, 1);

INSERT INTO public."WFE_WORKFLOW_DEFINITIONS" ("WORKFLOW_DEFINITION_ID", "WORKFLOW_CODE", "WORKFLOW_NAME", "VERSION_NO", "DEFINITION_STATUS", "BPMN_XML", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES (1, 'PURCHASED_PORTFOLIO_WF', 'Purchased Portfolio Workflow', '1', 'released', '<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_PURCHASED_PORTFOLIO_WF" targetNamespace="http://finova/workflow">
  <bpmn:process id="Process_PURCHASED_PORTFOLIO_WF" name="Purchased Portfolio Workflow" isExecutable="true" finova:variables=''["ENTITY_ID", "ENTITY_REF", "ENTITY_TYPE", "MODULE_CODE"]''>
    <bpmn:startEvent id="StartEvent_1" name="Start"><bpmn:outgoing>Flow_Start</bpmn:outgoing></bpmn:startEvent>
    <bpmn:userTask id="Task_LOAD_DATA" name="Loading Excel Data" finova:stepCode="LOAD_DATA" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-208" finova:routeParameters=''[{"name": "P208_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_Start</bpmn:incoming><bpmn:outgoing>Flow_LOAD_DATA_NEXT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_AML_REVIEW" name="AML Review" finova:stepCode="AML_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CREDIT_OFFICER" finova:route="mtg-209" finova:routeParameters=''[{"name": "P209_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_LOAD_DATA_NEXT</bpmn:incoming><bpmn:outgoing>Flow_AML_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_AML_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_PORTFOLIO_INQUIRY" name="Portfolio Inquiry" finova:stepCode="PORTFOLIO_INQUIRY" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="INVESTIGATION_OFFICER" finova:route="mtg-210" finova:routeParameters=''[{"name": "P210_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_AML_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_PORTFOLIO_INQUIRY_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_PORTFOLIO_INQUIRY_REJECT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_LEGAL_REVIEW" name="Legal Review" finova:stepCode="LEGAL_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="LEGAL_OFFICER" finova:route="mtg-211" finova:routeParameters=''[{"name": "P211_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_PORTFOLIO_INQUIRY_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_LEGAL_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_LEGAL_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_VALUATION_REVIEW" name="Portfolio Valuation" finova:stepCode="VALUATION_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-401" finova:routeParameters=''[{"name": "P401_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_LEGAL_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_VALUATION_REVIEW_NEXT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_COVERAGE_REVIEW" name="Coverage Review" finova:stepCode="COVERAGE_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-212" finova:routeParameters=''[{"name": "P212_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_VALUATION_REVIEW_NEXT</bpmn:incoming><bpmn:outgoing>Flow_COVERAGE_REVIEW_NEXT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_CHEQUE_COVERAGE_REVIEW" name="Cheque Coverage Review" finova:stepCode="CHEQUE_COVERAGE_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="mtg-213" finova:routeParameters=''[{"name": "P213_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_COVERAGE_REVIEW_NEXT</bpmn:incoming><bpmn:outgoing>Flow_CHEQUE_COVERAGE_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_CHEQUE_COVERAGE_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_FINANCIAL_REVIEW" name="Financial Review" finova:stepCode="FINANCIAL_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CREDIT_OFFICER" finova:route="mtg-214" finova:routeParameters=''[{"name": "P214_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_CHEQUE_COVERAGE_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_FINANCIAL_REVIEW_NEXT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_BOARD_REVIEW" name="Board Review" finova:stepCode="BOARD_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-215" finova:routeParameters=''[{"name": "P215_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_FINANCIAL_REVIEW_NEXT</bpmn:incoming><bpmn:outgoing>Flow_BOARD_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_BOARD_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_DISBURSEMENT" name="Disbursement" finova:stepCode="DISBURSEMENT" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="mtg-217" finova:routeParameters=''[{"name": "P217_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_BOARD_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_DISBURSEMENT_NEXT</bpmn:outgoing></bpmn:userTask>
    <bpmn:endEvent id="EndEvent_1" name="End"><bpmn:incoming>Flow_AML_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_PORTFOLIO_INQUIRY_REJECT</bpmn:incoming><bpmn:incoming>Flow_LEGAL_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_CHEQUE_COVERAGE_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_BOARD_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_DISBURSEMENT_NEXT</bpmn:incoming></bpmn:endEvent>
    <bpmn:sequenceFlow id="Flow_Start" sourceRef="StartEvent_1" targetRef="Task_LOAD_DATA" />
    <bpmn:sequenceFlow id="Flow_LOAD_DATA_NEXT" sourceRef="Task_LOAD_DATA" targetRef="Task_AML_REVIEW" />
    <bpmn:sequenceFlow id="Flow_AML_REVIEW_APPROVE" sourceRef="Task_AML_REVIEW" targetRef="Task_PORTFOLIO_INQUIRY" name="APPROVE" finova:outcome="APPROVE" />
    <bpmn:sequenceFlow id="Flow_AML_REVIEW_REJECT" sourceRef="Task_AML_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_PORTFOLIO_INQUIRY_APPROVE" sourceRef="Task_PORTFOLIO_INQUIRY" targetRef="Task_LEGAL_REVIEW" name="APPROVE" finova:outcome="APPROVE" />
    <bpmn:sequenceFlow id="Flow_PORTFOLIO_INQUIRY_REJECT" sourceRef="Task_PORTFOLIO_INQUIRY" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_LEGAL_REVIEW_APPROVE" sourceRef="Task_LEGAL_REVIEW" targetRef="Task_VALUATION_REVIEW" name="APPROVE" finova:outcome="APPROVE" />
    <bpmn:sequenceFlow id="Flow_LEGAL_REVIEW_REJECT" sourceRef="Task_LEGAL_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_VALUATION_REVIEW_NEXT" sourceRef="Task_VALUATION_REVIEW" targetRef="Task_COVERAGE_REVIEW" />
    <bpmn:sequenceFlow id="Flow_COVERAGE_REVIEW_NEXT" sourceRef="Task_COVERAGE_REVIEW" targetRef="Task_CHEQUE_COVERAGE_REVIEW" />
    <bpmn:sequenceFlow id="Flow_CHEQUE_COVERAGE_REVIEW_APPROVE" sourceRef="Task_CHEQUE_COVERAGE_REVIEW" targetRef="Task_FINANCIAL_REVIEW" name="APPROVE" finova:outcome="APPROVE" />
    <bpmn:sequenceFlow id="Flow_CHEQUE_COVERAGE_REVIEW_REJECT" sourceRef="Task_CHEQUE_COVERAGE_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_FINANCIAL_REVIEW_NEXT" sourceRef="Task_FINANCIAL_REVIEW" targetRef="Task_BOARD_REVIEW" />
    <bpmn:sequenceFlow id="Flow_BOARD_REVIEW_APPROVE" sourceRef="Task_BOARD_REVIEW" targetRef="Task_DISBURSEMENT" name="APPROVE" finova:outcome="APPROVE" />
    <bpmn:sequenceFlow id="Flow_BOARD_REVIEW_REJECT" sourceRef="Task_BOARD_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_DISBURSEMENT_NEXT" sourceRef="Task_DISBURSEMENT" targetRef="EndEvent_1" />
  </bpmn:process>
  <bpmndi:BPMNDiagram id="BPMNDiagram_1"><bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_PURCHASED_PORTFOLIO_WF"><bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1"><dc:Bounds x="100" y="202" width="36" height="36" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_LOAD_DATA_di" bpmnElement="Task_LOAD_DATA"><dc:Bounds x="180" y="180" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_AML_REVIEW_di" bpmnElement="Task_AML_REVIEW"><dc:Bounds x="380" y="180" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_PORTFOLIO_INQUIRY_di" bpmnElement="Task_PORTFOLIO_INQUIRY"><dc:Bounds x="580" y="180" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_LEGAL_REVIEW_di" bpmnElement="Task_LEGAL_REVIEW"><dc:Bounds x="780" y="180" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_VALUATION_REVIEW_di" bpmnElement="Task_VALUATION_REVIEW"><dc:Bounds x="980" y="180" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_COVERAGE_REVIEW_di" bpmnElement="Task_COVERAGE_REVIEW"><dc:Bounds x="180" y="400" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_CHEQUE_COVERAGE_REVIEW_di" bpmnElement="Task_CHEQUE_COVERAGE_REVIEW"><dc:Bounds x="380" y="400" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_FINANCIAL_REVIEW_di" bpmnElement="Task_FINANCIAL_REVIEW"><dc:Bounds x="580" y="400" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_BOARD_REVIEW_di" bpmnElement="Task_BOARD_REVIEW"><dc:Bounds x="780" y="400" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_DISBURSEMENT_di" bpmnElement="Task_DISBURSEMENT"><dc:Bounds x="980" y="400" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1"><dc:Bounds x="1180" y="422" width="36" height="36" /></bpmndi:BPMNShape><bpmndi:BPMNEdge id="Flow_Start_di" bpmnElement="Flow_Start"><di:waypoint x="136" y="220" /><di:waypoint x="180" y="220" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_LOAD_DATA_NEXT_di" bpmnElement="Flow_LOAD_DATA_NEXT"><di:waypoint x="310" y="220" /><di:waypoint x="380" y="220" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_AML_REVIEW_APPROVE_di" bpmnElement="Flow_AML_REVIEW_APPROVE"><di:waypoint x="510" y="220" /><di:waypoint x="580" y="220" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_PORTFOLIO_INQUIRY_APPROVE_di" bpmnElement="Flow_PORTFOLIO_INQUIRY_APPROVE"><di:waypoint x="710" y="220" /><di:waypoint x="780" y="220" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_LEGAL_REVIEW_APPROVE_di" bpmnElement="Flow_LEGAL_REVIEW_APPROVE"><di:waypoint x="910" y="220" /><di:waypoint x="980" y="220" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_VALUATION_REVIEW_NEXT_di" bpmnElement="Flow_VALUATION_REVIEW_NEXT"><di:waypoint x="1045" y="260" /><di:waypoint x="1045" y="330" /><di:waypoint x="245" y="330" /><di:waypoint x="245" y="400" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_COVERAGE_REVIEW_NEXT_di" bpmnElement="Flow_COVERAGE_REVIEW_NEXT"><di:waypoint x="310" y="440" /><di:waypoint x="380" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_CHEQUE_COVERAGE_REVIEW_APPROVE_di" bpmnElement="Flow_CHEQUE_COVERAGE_REVIEW_APPROVE"><di:waypoint x="510" y="440" /><di:waypoint x="580" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_FINANCIAL_REVIEW_NEXT_di" bpmnElement="Flow_FINANCIAL_REVIEW_NEXT"><di:waypoint x="710" y="440" /><di:waypoint x="780" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_BOARD_REVIEW_APPROVE_di" bpmnElement="Flow_BOARD_REVIEW_APPROVE"><di:waypoint x="910" y="440" /><di:waypoint x="980" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_DISBURSEMENT_NEXT_di" bpmnElement="Flow_DISBURSEMENT_NEXT"><di:waypoint x="1110" y="440" /><di:waypoint x="1180" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_AML_REVIEW_REJECT_di" bpmnElement="Flow_AML_REVIEW_REJECT"><di:waypoint x="445" y="180" /><di:waypoint x="445" y="130" /><di:waypoint x="1290" y="130" /><di:waypoint x="1290" y="440" /><di:waypoint x="1216" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_PORTFOLIO_INQUIRY_REJECT_di" bpmnElement="Flow_PORTFOLIO_INQUIRY_REJECT"><di:waypoint x="645" y="180" /><di:waypoint x="645" y="130" /><di:waypoint x="1290" y="130" /><di:waypoint x="1290" y="440" /><di:waypoint x="1216" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_LEGAL_REVIEW_REJECT_di" bpmnElement="Flow_LEGAL_REVIEW_REJECT"><di:waypoint x="845" y="180" /><di:waypoint x="845" y="130" /><di:waypoint x="1290" y="130" /><di:waypoint x="1290" y="440" /><di:waypoint x="1216" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_CHEQUE_COVERAGE_REVIEW_REJECT_di" bpmnElement="Flow_CHEQUE_COVERAGE_REVIEW_REJECT"><di:waypoint x="445" y="480" /><di:waypoint x="445" y="540" /><di:waypoint x="1198" y="540" /><di:waypoint x="1198" y="458" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_BOARD_REVIEW_REJECT_di" bpmnElement="Flow_BOARD_REVIEW_REJECT"><di:waypoint x="845" y="480" /><di:waypoint x="845" y="540" /><di:waypoint x="1198" y="540" /><di:waypoint x="1198" y="458" /></bpmndi:BPMNEdge></bpmndi:BPMNPlane></bpmndi:BPMNDiagram>
</bpmn:definitions>', 'Mortgage portfolio purchase: data load, AML, inquiry, legal, valuation, coverage, cheque coverage, financial and board review, disbursement.', true, 'SYSTEM', '2026-11-18 17:35:16.542062+00', NULL, NULL, 0);
INSERT INTO public."WFE_WORKFLOW_DEFINITIONS" ("WORKFLOW_DEFINITION_ID", "WORKFLOW_CODE", "WORKFLOW_NAME", "VERSION_NO", "DEFINITION_STATUS", "BPMN_XML", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES (2, 'FACTORING_LIMIT_APPROVAL', 'Factoring Limit Approval', '1', 'released', '<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_FINOVA" targetNamespace="http://finova/workflow">
  <bpmn:process id="Process_FINOVA" name="FINOVA Workflow Engine" isExecutable="true" finova:variables="[{&#34;name&#34;:&#34;entity_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_name&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_number&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_type&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true}]">
    <bpmn:startEvent id="StartEvent_1" name="Start" finova:stepCode="" finova:stepType="Start" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
      <bpmn:outgoing>Flow_1</bpmn:outgoing>
    </bpmn:startEvent>
    <bpmn:userTask id="Task_1" name="Operation Received" finova:stepCode="OPERATION_RECEIVED" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="factoring-agreement-assign-sales" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="ACCEPT,APPROVE,REJECT,RETURN,COMPLETE,CLAIM">
      <bpmn:incoming>Flow_1</bpmn:incoming>
      <bpmn:outgoing>Flow_2</bpmn:outgoing>
    </bpmn:userTask>
    <bpmn:endEvent id="EndEvent_1" name="End" finova:stepCode="" finova:stepType="End" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
      <bpmn:incoming>Flow_1n70sc6</bpmn:incoming>
      <bpmn:incoming>Flow_16r22h2</bpmn:incoming>
      <bpmn:incoming>Flow_0dfqa7w</bpmn:incoming>
      <bpmn:incoming>Flow_1bcnv9y</bpmn:incoming>
    </bpmn:endEvent>
    <bpmn:sequenceFlow id="Flow_1" name="" sourceRef="StartEvent_1" targetRef="Task_1" finova:outcome="" />
    <bpmn:sequenceFlow id="Flow_2" name="" sourceRef="Task_1" targetRef="Activity_0cefpuo" finova:outcome="" />
    <bpmn:task id="Activity_0cefpuo" name="Sales Review" finova:stepCode="SALES_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="factoring-agreement-sales-review" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE">
      <bpmn:incoming>Flow_2</bpmn:incoming>
      <bpmn:outgoing>Flow_1gknbdy</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_1gknbdy" name="" sourceRef="Activity_0cefpuo" targetRef="Activity_0wa22in" finova:outcome="" />
    <bpmn:task id="Activity_0wa22in" name="Investigation" finova:stepCode="INVESTIGATION" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="INVESTIGATION_OFFICER" finova:route="factoring-inquiry-field-report" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE,ACCEPT">
      <bpmn:incoming>Flow_1gknbdy</bpmn:incoming>
      <bpmn:outgoing>Flow_01poi2g</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_1n70sc6" name="REJECT" sourceRef="Activity_0btcy1n" targetRef="EndEvent_1" finova:outcome="REJECT" />
    <bpmn:task id="Activity_0punm0q" name="Credit Analysis" finova:stepCode="CREDIT_ANALYSIS" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CREDIT_APPROVER" finova:route="factoring-credit-analysis" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE">
      <bpmn:incoming>Flow_01poi2g</bpmn:incoming>
      <bpmn:outgoing>Flow_0vi5ko2</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_01poi2g" name="" sourceRef="Activity_0wa22in" targetRef="Activity_0punm0q" finova:outcome="" />
    <bpmn:task id="Activity_0btcy1n" name="Risk Review" finova:stepCode="RISK_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="RISK_OFFICER" finova:route="factoring-risk-review" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT,COMPLETE">
      <bpmn:incoming>Flow_0vi5ko2</bpmn:incoming>
      <bpmn:outgoing>Flow_1n70sc6</bpmn:outgoing>
      <bpmn:outgoing>Flow_0z38dkw</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_0vi5ko2" name="" sourceRef="Activity_0punm0q" targetRef="Activity_0btcy1n" finova:outcome="" />
    <bpmn:task id="Activity_0pogexl" name="Committe Approval" finova:stepCode="COMMITTEE_APPROVAL" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="COMMITTEE_SECRETARY" finova:route="factoring-committee-decision" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT,COMPLETE">
      <bpmn:incoming>Flow_0z38dkw</bpmn:incoming>
      <bpmn:incoming>Flow_0db6oid</bpmn:incoming>
      <bpmn:outgoing>Flow_0qv4imo</bpmn:outgoing>
      <bpmn:outgoing>Flow_1bcnv9y</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_0z38dkw" name="ACCEPT" sourceRef="Activity_0btcy1n" targetRef="Activity_0pogexl" finova:outcome="ACCEPT" />
    <bpmn:task id="Activity_10lu0rl" name="Client Acceptance" finova:stepCode="CLIENT_ACCEPTANCE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="factoring-contract-request" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,REJECT,ACCEPT">
      <bpmn:incoming>Flow_0qv4imo</bpmn:incoming>
      <bpmn:outgoing>Flow_0db6oid</bpmn:outgoing>
      <bpmn:outgoing>Flow_0igy339</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_0qv4imo" name="ACCEPT" sourceRef="Activity_0pogexl" targetRef="Activity_10lu0rl" finova:outcome="ACCEPT" />
    <bpmn:sequenceFlow id="Flow_0db6oid" name="REJECT" sourceRef="Activity_10lu0rl" targetRef="Activity_0pogexl" finova:outcome="REJECT" />
    <bpmn:task id="Activity_1ciisf3" name="Generate Contract" finova:stepCode="CONTRACT_GENERATION" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="factoring-contract-generation" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE,APPROVE">
      <bpmn:incoming>Flow_0igy339</bpmn:incoming>
      <bpmn:outgoing>Flow_0qybf5i</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_0igy339" name="ACCEPT" sourceRef="Activity_10lu0rl" targetRef="Activity_1ciisf3" finova:outcome="ACCEPT" />
    <bpmn:task id="Activity_0nb9j68" name="Legal Review and Limit Creation" finova:stepCode="LEGAL_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="LEGAL_OFFICER" finova:route="factoring-legal-review" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,REJECT,ACCEPT">
      <bpmn:incoming>Flow_0qybf5i</bpmn:incoming>
      <bpmn:outgoing>Flow_16r22h2</bpmn:outgoing>
      <bpmn:outgoing>Flow_0dfqa7w</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_0qybf5i" name="" sourceRef="Activity_1ciisf3" targetRef="Activity_0nb9j68" finova:outcome="" />
    <bpmn:sequenceFlow id="Flow_16r22h2" name="REJECT" sourceRef="Activity_0nb9j68" targetRef="EndEvent_1" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_0dfqa7w" name="ACCEPT" sourceRef="Activity_0nb9j68" targetRef="EndEvent_1" />
    <bpmn:sequenceFlow id="Flow_1bcnv9y" name="REJECT" sourceRef="Activity_0pogexl" targetRef="EndEvent_1" finova:outcome="REJECT" />
  </bpmn:process>
  <bpmndi:BPMNDiagram id="BPMNDiagram_1">
    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_FINOVA">
      <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1">
        <dc:Bounds x="1446" y="242" width="36" height="36" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1452" y="278" width="24" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_1_di" bpmnElement="Task_1">
        <dc:Bounds x="1544" y="220" width="120" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1">
        <dc:Bounds x="2066" y="466" width="36" height="36" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="2112" y="477" width="20" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0cefpuo_di" bpmnElement="Activity_0cefpuo">
        <dc:Bounds x="1734" y="220" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0wa22in_di" bpmnElement="Activity_0wa22in">
        <dc:Bounds x="1914" y="220" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0punm0q_di" bpmnElement="Activity_0punm0q">
        <dc:Bounds x="2060" y="220" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0btcy1n_di" bpmnElement="Activity_0btcy1n">
        <dc:Bounds x="2044" y="343" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0pogexl_di" bpmnElement="Activity_0pogexl">
        <dc:Bounds x="1870" y="343" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_10lu0rl_di" bpmnElement="Activity_10lu0rl">
        <dc:Bounds x="1704" y="343" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_1ciisf3_di" bpmnElement="Activity_1ciisf3">
        <dc:Bounds x="1544" y="343" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0nb9j68_di" bpmnElement="Activity_0nb9j68">
        <dc:Bounds x="1544" y="540" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNEdge id="Flow_1_di" bpmnElement="Flow_1">
        <di:waypoint x="1482" y="260" />
        <di:waypoint x="1544" y="260" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_2_di" bpmnElement="Flow_2">
        <di:waypoint x="1664" y="260" />
        <di:waypoint x="1734" y="260" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_1gknbdy_di" bpmnElement="Flow_1gknbdy">
        <di:waypoint x="1834" y="260" />
        <di:waypoint x="1914" y="260" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_1n70sc6_di" bpmnElement="Flow_1n70sc6">
        <di:waypoint x="2084" y="423" />
        <di:waypoint x="2084" y="466" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="2078" y="440" width="43" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_01poi2g_di" bpmnElement="Flow_01poi2g">
        <di:waypoint x="2014" y="260" />
        <di:waypoint x="2060" y="260" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0vi5ko2_di" bpmnElement="Flow_0vi5ko2">
        <di:waypoint x="2160" y="260" />
        <di:waypoint x="2214" y="260" />
        <di:waypoint x="2214" y="383" />
        <di:waypoint x="2144" y="383" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0z38dkw_di" bpmnElement="Flow_0z38dkw">
        <di:waypoint x="2044" y="383" />
        <di:waypoint x="1970" y="383" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1985" y="365" width="45" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0qv4imo_di" bpmnElement="Flow_0qv4imo">
        <di:waypoint x="1870" y="383" />
        <di:waypoint x="1837" y="383" />
        <di:waypoint x="1837" y="363" />
        <di:waypoint x="1804" y="363" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0db6oid_di" bpmnElement="Flow_0db6oid">
        <di:waypoint x="1804" y="403" />
        <di:waypoint x="1837" y="403" />
        <di:waypoint x="1837" y="380" />
        <di:waypoint x="1870" y="380" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1831" y="465" width="43" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0igy339_di" bpmnElement="Flow_0igy339">
        <di:waypoint x="1704" y="383" />
        <di:waypoint x="1644" y="383" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1653" y="365" width="45" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0qybf5i_di" bpmnElement="Flow_0qybf5i">
        <di:waypoint x="1594" y="423" />
        <di:waypoint x="1594" y="540" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_16r22h2_di" bpmnElement="Flow_16r22h2">
        <di:waypoint x="1644" y="560" />
        <di:waypoint x="2084" y="560" />
        <di:waypoint x="2084" y="502" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1868" y="537" width="43" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0dfqa7w_di" bpmnElement="Flow_0dfqa7w">
        <di:waypoint x="1644" y="580" />
        <di:waypoint x="1699" y="580" />
        <di:waypoint x="1699" y="620" />
        <di:waypoint x="2094" y="620" />
        <di:waypoint x="2094" y="499" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1875" y="613" width="45" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_1bcnv9y_di" bpmnElement="Flow_1bcnv9y">
        <di:waypoint x="1920" y="423" />
        <di:waypoint x="1920" y="484" />
        <di:waypoint x="2066" y="484" />
      </bpmndi:BPMNEdge>
    </bpmndi:BPMNPlane>
  </bpmndi:BPMNDiagram>
<!-- FINOVA_VIEWBOX:{"x":1138.0359955005624,"y":189.458942632171,"width":1096.7379077615299,"height":511.81102362204723} -->
</bpmn:definitions>', 'Factoring application: operation received, sales review, investigation, credit analysis, risk review, committee approval, client acceptance, contract generation, legal review and limit creation.', true, 'SYSTEM', '2026-11-18 17:35:20.563727+00', NULL, NULL, 0);
INSERT INTO public."WFE_WORKFLOW_DEFINITIONS" ("WORKFLOW_DEFINITION_ID", "WORKFLOW_CODE", "WORKFLOW_NAME", "VERSION_NO", "DEFINITION_STATUS", "BPMN_XML", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES (4, 'FACTORING_FAST_TRACK', 'Factoring Fast Track', '1', 'released', '<?xml version="1.0" encoding="UTF-8"?>
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
</bpmn:definitions>', 'Factoring application, fast track: approval by one approver, then contract and limit creation.', true, 'SYSTEM', '2026-11-18 17:35:26.170524+00', NULL, NULL, 0);
INSERT INTO public."WFE_WORKFLOW_DEFINITIONS" ("WORKFLOW_DEFINITION_ID", "WORKFLOW_CODE", "WORKFLOW_NAME", "VERSION_NO", "DEFINITION_STATUS", "BPMN_XML", "DESCRIPTION", "IS_ACTIVE", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES (3, 'DISBURSEMENT_REQUEST_WF', 'Disbursement Request Workflow', '1', 'released', '<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_DISBURSEMENT_REQUEST_WF" targetNamespace="http://finova/workflow">
  <bpmn:process id="Process_DISBURSEMENT_REQUEST_WF" name="Disbursement Request Workflow" isExecutable="true" finova:variables=''["ENTITY_ID", "ENTITY_REF", "ENTITY_TYPE", "MODULE_CODE"]''>
    <bpmn:startEvent id="StartEvent_1" name="Start"><bpmn:outgoing>Flow_Start</bpmn:outgoing></bpmn:startEvent>
    <bpmn:userTask id="Task_INITIATION" name="Initiation" finova:stepCode="INITIATION" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="crm-disbursement-request" finova:routeParameters=''[{"name": "disbursementId", "value": "ENTITY_ID"}]'' finova:actions="COMPLETE"><bpmn:incoming>Flow_Start</bpmn:incoming><bpmn:outgoing>Flow_INIT_TO_INVOICE_UPLOAD</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_INVOICE_UPLOAD" name="Invoice Upload" finova:stepCode="INVOICE_UPLOAD" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="crm-disbursement-invoice-entry" finova:routeParameters=''[{"name": "disbursementId", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_INIT_TO_INVOICE_UPLOAD</bpmn:incoming><bpmn:outgoing>Flow_FCT_OPERATIONS_TO_TREASUARY</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_TREASURY_DISBURSE" name="Treasury Disburse" finova:stepCode="TREASURY_DISBURSE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="trs-disburse" finova:routeParameters=''[{"name": "disbursementId", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_FCT_OPERATIONS_TO_TREASUARY</bpmn:incoming><bpmn:outgoing>Flow_TREASURY_TO_COMPLETE</bpmn:outgoing></bpmn:userTask>
    <bpmn:endEvent id="EndEvent_1" name="Workflow Completed"><bpmn:incoming>Flow_TREASURY_TO_COMPLETE</bpmn:incoming></bpmn:endEvent>
    <bpmn:sequenceFlow id="Flow_Start" sourceRef="StartEvent_1" targetRef="Task_INITIATION" />
    <bpmn:sequenceFlow id="Flow_INIT_TO_INVOICE_UPLOAD" sourceRef="Task_INITIATION" targetRef="Task_INVOICE_UPLOAD" name="Initiation → Invoice Upload" />
    <bpmn:sequenceFlow id="Flow_FCT_OPERATIONS_TO_TREASUARY" sourceRef="Task_INVOICE_UPLOAD" targetRef="Task_TREASURY_DISBURSE" name="SALES TO TREASUARY" />
    <bpmn:sequenceFlow id="Flow_TREASURY_TO_COMPLETE" sourceRef="Task_TREASURY_DISBURSE" targetRef="EndEvent_1" name="TREASURY TO COMPLETE" />
  </bpmn:process>
  <bpmndi:BPMNDiagram id="BPMNDiagram_1">
    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_DISBURSEMENT_REQUEST_WF">
      <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1"><dc:Bounds x="120" y="182" width="36" height="36" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_INITIATION_di" bpmnElement="Task_INITIATION"><dc:Bounds x="200" y="160" width="130" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_INVOICE_UPLOAD_di" bpmnElement="Task_INVOICE_UPLOAD"><dc:Bounds x="380" y="160" width="130" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_TREASURY_DISBURSE_di" bpmnElement="Task_TREASURY_DISBURSE"><dc:Bounds x="560" y="160" width="130" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1"><dc:Bounds x="740" y="182" width="36" height="36" /></bpmndi:BPMNShape>
      <bpmndi:BPMNEdge id="Flow_Start_di" bpmnElement="Flow_Start"><di:waypoint x="156" y="200" /><di:waypoint x="200" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_INIT_TO_INVOICE_UPLOAD_di" bpmnElement="Flow_INIT_TO_INVOICE_UPLOAD"><di:waypoint x="330" y="200" /><di:waypoint x="380" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_FCT_OPERATIONS_TO_TREASUARY_di" bpmnElement="Flow_FCT_OPERATIONS_TO_TREASUARY"><di:waypoint x="510" y="200" /><di:waypoint x="560" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_TREASURY_TO_COMPLETE_di" bpmnElement="Flow_TREASURY_TO_COMPLETE"><di:waypoint x="690" y="200" /><di:waypoint x="740" y="200" /></bpmndi:BPMNEdge>
    </bpmndi:BPMNPlane>
  </bpmndi:BPMNDiagram>
</bpmn:definitions>', 'Customer disbursement request: initiation, invoice upload and treasury disbursement.', true, 'SYSTEM', '2026-11-18 17:35:23.187862+00', NULL, NULL, 0);

INSERT INTO public."WFE_DEFINITION_VERSIONS" ("DEFINITION_VERSION_ID", "WORKFLOW_DEFINITION_ID", "VERSION_NO", "BPMN_XML", "RELEASED_AT", "RELEASED_BY", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES (1, 1, 1, '<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_PURCHASED_PORTFOLIO_WF" targetNamespace="http://finova/workflow">
  <bpmn:process id="Process_PURCHASED_PORTFOLIO_WF" name="Purchased Portfolio Workflow" isExecutable="true" finova:variables=''["ENTITY_ID", "ENTITY_REF", "ENTITY_TYPE", "MODULE_CODE"]''>
    <bpmn:startEvent id="StartEvent_1" name="Start"><bpmn:outgoing>Flow_Start</bpmn:outgoing></bpmn:startEvent>
    <bpmn:userTask id="Task_LOAD_DATA" name="Loading Excel Data" finova:stepCode="LOAD_DATA" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-208" finova:routeParameters=''[{"name": "P208_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_Start</bpmn:incoming><bpmn:outgoing>Flow_LOAD_DATA_NEXT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_AML_REVIEW" name="AML Review" finova:stepCode="AML_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CREDIT_OFFICER" finova:route="mtg-209" finova:routeParameters=''[{"name": "P209_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_LOAD_DATA_NEXT</bpmn:incoming><bpmn:outgoing>Flow_AML_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_AML_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_PORTFOLIO_INQUIRY" name="Portfolio Inquiry" finova:stepCode="PORTFOLIO_INQUIRY" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="INVESTIGATION_OFFICER" finova:route="mtg-210" finova:routeParameters=''[{"name": "P210_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_AML_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_PORTFOLIO_INQUIRY_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_PORTFOLIO_INQUIRY_REJECT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_LEGAL_REVIEW" name="Legal Review" finova:stepCode="LEGAL_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="LEGAL_OFFICER" finova:route="mtg-211" finova:routeParameters=''[{"name": "P211_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_PORTFOLIO_INQUIRY_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_LEGAL_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_LEGAL_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_VALUATION_REVIEW" name="Portfolio Valuation" finova:stepCode="VALUATION_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-401" finova:routeParameters=''[{"name": "P401_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_LEGAL_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_VALUATION_REVIEW_NEXT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_COVERAGE_REVIEW" name="Coverage Review" finova:stepCode="COVERAGE_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-212" finova:routeParameters=''[{"name": "P212_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_VALUATION_REVIEW_NEXT</bpmn:incoming><bpmn:outgoing>Flow_COVERAGE_REVIEW_NEXT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_CHEQUE_COVERAGE_REVIEW" name="Cheque Coverage Review" finova:stepCode="CHEQUE_COVERAGE_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="mtg-213" finova:routeParameters=''[{"name": "P213_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_COVERAGE_REVIEW_NEXT</bpmn:incoming><bpmn:outgoing>Flow_CHEQUE_COVERAGE_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_CHEQUE_COVERAGE_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_FINANCIAL_REVIEW" name="Financial Review" finova:stepCode="FINANCIAL_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CREDIT_OFFICER" finova:route="mtg-214" finova:routeParameters=''[{"name": "P214_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_CHEQUE_COVERAGE_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_FINANCIAL_REVIEW_NEXT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_BOARD_REVIEW" name="Board Review" finova:stepCode="BOARD_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="mtg-215" finova:routeParameters=''[{"name": "P215_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,APPROVE,REJECT"><bpmn:incoming>Flow_FINANCIAL_REVIEW_NEXT</bpmn:incoming><bpmn:outgoing>Flow_BOARD_REVIEW_APPROVE</bpmn:outgoing><bpmn:outgoing>Flow_BOARD_REVIEW_REJECT</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_DISBURSEMENT" name="Disbursement" finova:stepCode="DISBURSEMENT" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="mtg-217" finova:routeParameters=''[{"name": "P217_PORTFOLIO_ID", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_BOARD_REVIEW_APPROVE</bpmn:incoming><bpmn:outgoing>Flow_DISBURSEMENT_NEXT</bpmn:outgoing></bpmn:userTask>
    <bpmn:endEvent id="EndEvent_1" name="End"><bpmn:incoming>Flow_AML_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_PORTFOLIO_INQUIRY_REJECT</bpmn:incoming><bpmn:incoming>Flow_LEGAL_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_CHEQUE_COVERAGE_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_BOARD_REVIEW_REJECT</bpmn:incoming><bpmn:incoming>Flow_DISBURSEMENT_NEXT</bpmn:incoming></bpmn:endEvent>
    <bpmn:sequenceFlow id="Flow_Start" sourceRef="StartEvent_1" targetRef="Task_LOAD_DATA" />
    <bpmn:sequenceFlow id="Flow_LOAD_DATA_NEXT" sourceRef="Task_LOAD_DATA" targetRef="Task_AML_REVIEW" />
    <bpmn:sequenceFlow id="Flow_AML_REVIEW_APPROVE" sourceRef="Task_AML_REVIEW" targetRef="Task_PORTFOLIO_INQUIRY" name="APPROVE" finova:outcome="APPROVE" />
    <bpmn:sequenceFlow id="Flow_AML_REVIEW_REJECT" sourceRef="Task_AML_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_PORTFOLIO_INQUIRY_APPROVE" sourceRef="Task_PORTFOLIO_INQUIRY" targetRef="Task_LEGAL_REVIEW" name="APPROVE" finova:outcome="APPROVE" />
    <bpmn:sequenceFlow id="Flow_PORTFOLIO_INQUIRY_REJECT" sourceRef="Task_PORTFOLIO_INQUIRY" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_LEGAL_REVIEW_APPROVE" sourceRef="Task_LEGAL_REVIEW" targetRef="Task_VALUATION_REVIEW" name="APPROVE" finova:outcome="APPROVE" />
    <bpmn:sequenceFlow id="Flow_LEGAL_REVIEW_REJECT" sourceRef="Task_LEGAL_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_VALUATION_REVIEW_NEXT" sourceRef="Task_VALUATION_REVIEW" targetRef="Task_COVERAGE_REVIEW" />
    <bpmn:sequenceFlow id="Flow_COVERAGE_REVIEW_NEXT" sourceRef="Task_COVERAGE_REVIEW" targetRef="Task_CHEQUE_COVERAGE_REVIEW" />
    <bpmn:sequenceFlow id="Flow_CHEQUE_COVERAGE_REVIEW_APPROVE" sourceRef="Task_CHEQUE_COVERAGE_REVIEW" targetRef="Task_FINANCIAL_REVIEW" name="APPROVE" finova:outcome="APPROVE" />
    <bpmn:sequenceFlow id="Flow_CHEQUE_COVERAGE_REVIEW_REJECT" sourceRef="Task_CHEQUE_COVERAGE_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_FINANCIAL_REVIEW_NEXT" sourceRef="Task_FINANCIAL_REVIEW" targetRef="Task_BOARD_REVIEW" />
    <bpmn:sequenceFlow id="Flow_BOARD_REVIEW_APPROVE" sourceRef="Task_BOARD_REVIEW" targetRef="Task_DISBURSEMENT" name="APPROVE" finova:outcome="APPROVE" />
    <bpmn:sequenceFlow id="Flow_BOARD_REVIEW_REJECT" sourceRef="Task_BOARD_REVIEW" targetRef="EndEvent_1" name="REJECT" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_DISBURSEMENT_NEXT" sourceRef="Task_DISBURSEMENT" targetRef="EndEvent_1" />
  </bpmn:process>
  <bpmndi:BPMNDiagram id="BPMNDiagram_1"><bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_PURCHASED_PORTFOLIO_WF"><bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1"><dc:Bounds x="100" y="202" width="36" height="36" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_LOAD_DATA_di" bpmnElement="Task_LOAD_DATA"><dc:Bounds x="180" y="180" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_AML_REVIEW_di" bpmnElement="Task_AML_REVIEW"><dc:Bounds x="380" y="180" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_PORTFOLIO_INQUIRY_di" bpmnElement="Task_PORTFOLIO_INQUIRY"><dc:Bounds x="580" y="180" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_LEGAL_REVIEW_di" bpmnElement="Task_LEGAL_REVIEW"><dc:Bounds x="780" y="180" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_VALUATION_REVIEW_di" bpmnElement="Task_VALUATION_REVIEW"><dc:Bounds x="980" y="180" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_COVERAGE_REVIEW_di" bpmnElement="Task_COVERAGE_REVIEW"><dc:Bounds x="180" y="400" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_CHEQUE_COVERAGE_REVIEW_di" bpmnElement="Task_CHEQUE_COVERAGE_REVIEW"><dc:Bounds x="380" y="400" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_FINANCIAL_REVIEW_di" bpmnElement="Task_FINANCIAL_REVIEW"><dc:Bounds x="580" y="400" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_BOARD_REVIEW_di" bpmnElement="Task_BOARD_REVIEW"><dc:Bounds x="780" y="400" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="Task_DISBURSEMENT_di" bpmnElement="Task_DISBURSEMENT"><dc:Bounds x="980" y="400" width="130" height="80" /></bpmndi:BPMNShape><bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1"><dc:Bounds x="1180" y="422" width="36" height="36" /></bpmndi:BPMNShape><bpmndi:BPMNEdge id="Flow_Start_di" bpmnElement="Flow_Start"><di:waypoint x="136" y="220" /><di:waypoint x="180" y="220" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_LOAD_DATA_NEXT_di" bpmnElement="Flow_LOAD_DATA_NEXT"><di:waypoint x="310" y="220" /><di:waypoint x="380" y="220" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_AML_REVIEW_APPROVE_di" bpmnElement="Flow_AML_REVIEW_APPROVE"><di:waypoint x="510" y="220" /><di:waypoint x="580" y="220" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_PORTFOLIO_INQUIRY_APPROVE_di" bpmnElement="Flow_PORTFOLIO_INQUIRY_APPROVE"><di:waypoint x="710" y="220" /><di:waypoint x="780" y="220" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_LEGAL_REVIEW_APPROVE_di" bpmnElement="Flow_LEGAL_REVIEW_APPROVE"><di:waypoint x="910" y="220" /><di:waypoint x="980" y="220" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_VALUATION_REVIEW_NEXT_di" bpmnElement="Flow_VALUATION_REVIEW_NEXT"><di:waypoint x="1045" y="260" /><di:waypoint x="1045" y="330" /><di:waypoint x="245" y="330" /><di:waypoint x="245" y="400" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_COVERAGE_REVIEW_NEXT_di" bpmnElement="Flow_COVERAGE_REVIEW_NEXT"><di:waypoint x="310" y="440" /><di:waypoint x="380" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_CHEQUE_COVERAGE_REVIEW_APPROVE_di" bpmnElement="Flow_CHEQUE_COVERAGE_REVIEW_APPROVE"><di:waypoint x="510" y="440" /><di:waypoint x="580" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_FINANCIAL_REVIEW_NEXT_di" bpmnElement="Flow_FINANCIAL_REVIEW_NEXT"><di:waypoint x="710" y="440" /><di:waypoint x="780" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_BOARD_REVIEW_APPROVE_di" bpmnElement="Flow_BOARD_REVIEW_APPROVE"><di:waypoint x="910" y="440" /><di:waypoint x="980" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_DISBURSEMENT_NEXT_di" bpmnElement="Flow_DISBURSEMENT_NEXT"><di:waypoint x="1110" y="440" /><di:waypoint x="1180" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_AML_REVIEW_REJECT_di" bpmnElement="Flow_AML_REVIEW_REJECT"><di:waypoint x="445" y="180" /><di:waypoint x="445" y="130" /><di:waypoint x="1290" y="130" /><di:waypoint x="1290" y="440" /><di:waypoint x="1216" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_PORTFOLIO_INQUIRY_REJECT_di" bpmnElement="Flow_PORTFOLIO_INQUIRY_REJECT"><di:waypoint x="645" y="180" /><di:waypoint x="645" y="130" /><di:waypoint x="1290" y="130" /><di:waypoint x="1290" y="440" /><di:waypoint x="1216" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_LEGAL_REVIEW_REJECT_di" bpmnElement="Flow_LEGAL_REVIEW_REJECT"><di:waypoint x="845" y="180" /><di:waypoint x="845" y="130" /><di:waypoint x="1290" y="130" /><di:waypoint x="1290" y="440" /><di:waypoint x="1216" y="440" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_CHEQUE_COVERAGE_REVIEW_REJECT_di" bpmnElement="Flow_CHEQUE_COVERAGE_REVIEW_REJECT"><di:waypoint x="445" y="480" /><di:waypoint x="445" y="540" /><di:waypoint x="1198" y="540" /><di:waypoint x="1198" y="458" /></bpmndi:BPMNEdge><bpmndi:BPMNEdge id="Flow_BOARD_REVIEW_REJECT_di" bpmnElement="Flow_BOARD_REVIEW_REJECT"><di:waypoint x="845" y="480" /><di:waypoint x="845" y="540" /><di:waypoint x="1198" y="540" /><di:waypoint x="1198" y="458" /></bpmndi:BPMNEdge></bpmndi:BPMNPlane></bpmndi:BPMNDiagram>
</bpmn:definitions>', '2026-11-18 17:35:16.542062+00', 'SYSTEM', 'SYSTEM', '2026-11-18 17:35:28.373157+00', NULL, NULL, 1);
INSERT INTO public."WFE_DEFINITION_VERSIONS" ("DEFINITION_VERSION_ID", "WORKFLOW_DEFINITION_ID", "VERSION_NO", "BPMN_XML", "RELEASED_AT", "RELEASED_BY", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES (2, 2, 1, '<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_FINOVA" targetNamespace="http://finova/workflow">
  <bpmn:process id="Process_FINOVA" name="FINOVA Workflow Engine" isExecutable="true" finova:variables="[{&#34;name&#34;:&#34;entity_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_name&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_number&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;client_id&#34;,&#34;type&#34;:&#34;Number&#34;,&#34;required&#34;:true},{&#34;name&#34;:&#34;entity_type&#34;,&#34;type&#34;:&#34;String&#34;,&#34;required&#34;:true}]">
    <bpmn:startEvent id="StartEvent_1" name="Start" finova:stepCode="" finova:stepType="Start" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
      <bpmn:outgoing>Flow_1</bpmn:outgoing>
    </bpmn:startEvent>
    <bpmn:userTask id="Task_1" name="Operation Received" finova:stepCode="OPERATION_RECEIVED" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="factoring-agreement-assign-sales" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="ACCEPT,APPROVE,REJECT,RETURN,COMPLETE,CLAIM">
      <bpmn:incoming>Flow_1</bpmn:incoming>
      <bpmn:outgoing>Flow_2</bpmn:outgoing>
    </bpmn:userTask>
    <bpmn:endEvent id="EndEvent_1" name="End" finova:stepCode="" finova:stepType="End" finova:assignmentType="None" finova:assignee="" finova:route="" finova:routeParameters="[]" finova:actions="">
      <bpmn:incoming>Flow_1n70sc6</bpmn:incoming>
      <bpmn:incoming>Flow_16r22h2</bpmn:incoming>
      <bpmn:incoming>Flow_0dfqa7w</bpmn:incoming>
      <bpmn:incoming>Flow_1bcnv9y</bpmn:incoming>
    </bpmn:endEvent>
    <bpmn:sequenceFlow id="Flow_1" name="" sourceRef="StartEvent_1" targetRef="Task_1" finova:outcome="" />
    <bpmn:sequenceFlow id="Flow_2" name="" sourceRef="Task_1" targetRef="Activity_0cefpuo" finova:outcome="" />
    <bpmn:task id="Activity_0cefpuo" name="Sales Review" finova:stepCode="SALES_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="factoring-agreement-sales-review" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE">
      <bpmn:incoming>Flow_2</bpmn:incoming>
      <bpmn:outgoing>Flow_1gknbdy</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_1gknbdy" name="" sourceRef="Activity_0cefpuo" targetRef="Activity_0wa22in" finova:outcome="" />
    <bpmn:task id="Activity_0wa22in" name="Investigation" finova:stepCode="INVESTIGATION" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="INVESTIGATION_OFFICER" finova:route="factoring-inquiry-field-report" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE,ACCEPT">
      <bpmn:incoming>Flow_1gknbdy</bpmn:incoming>
      <bpmn:outgoing>Flow_01poi2g</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_1n70sc6" name="REJECT" sourceRef="Activity_0btcy1n" targetRef="EndEvent_1" finova:outcome="REJECT" />
    <bpmn:task id="Activity_0punm0q" name="Credit Analysis" finova:stepCode="CREDIT_ANALYSIS" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="CREDIT_APPROVER" finova:route="factoring-credit-analysis" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE">
      <bpmn:incoming>Flow_01poi2g</bpmn:incoming>
      <bpmn:outgoing>Flow_0vi5ko2</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_01poi2g" name="" sourceRef="Activity_0wa22in" targetRef="Activity_0punm0q" finova:outcome="" />
    <bpmn:task id="Activity_0btcy1n" name="Risk Review" finova:stepCode="RISK_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="RISK_OFFICER" finova:route="factoring-risk-review" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT,COMPLETE">
      <bpmn:incoming>Flow_0vi5ko2</bpmn:incoming>
      <bpmn:outgoing>Flow_1n70sc6</bpmn:outgoing>
      <bpmn:outgoing>Flow_0z38dkw</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_0vi5ko2" name="" sourceRef="Activity_0punm0q" targetRef="Activity_0btcy1n" finova:outcome="" />
    <bpmn:task id="Activity_0pogexl" name="Committe Approval" finova:stepCode="COMMITTEE_APPROVAL" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="COMMITTEE_SECRETARY" finova:route="factoring-committee-decision" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,ACCEPT,REJECT,COMPLETE">
      <bpmn:incoming>Flow_0z38dkw</bpmn:incoming>
      <bpmn:incoming>Flow_0db6oid</bpmn:incoming>
      <bpmn:outgoing>Flow_0qv4imo</bpmn:outgoing>
      <bpmn:outgoing>Flow_1bcnv9y</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_0z38dkw" name="ACCEPT" sourceRef="Activity_0btcy1n" targetRef="Activity_0pogexl" finova:outcome="ACCEPT" />
    <bpmn:task id="Activity_10lu0rl" name="Client Acceptance" finova:stepCode="CLIENT_ACCEPTANCE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="SALES_OFFICER" finova:route="factoring-contract-request" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,REJECT,ACCEPT">
      <bpmn:incoming>Flow_0qv4imo</bpmn:incoming>
      <bpmn:outgoing>Flow_0db6oid</bpmn:outgoing>
      <bpmn:outgoing>Flow_0igy339</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_0qv4imo" name="ACCEPT" sourceRef="Activity_0pogexl" targetRef="Activity_10lu0rl" finova:outcome="ACCEPT" />
    <bpmn:sequenceFlow id="Flow_0db6oid" name="REJECT" sourceRef="Activity_10lu0rl" targetRef="Activity_0pogexl" finova:outcome="REJECT" />
    <bpmn:task id="Activity_1ciisf3" name="Generate Contract" finova:stepCode="CONTRACT_GENERATION" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="factoring-contract-generation" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,COMPLETE,APPROVE">
      <bpmn:incoming>Flow_0igy339</bpmn:incoming>
      <bpmn:outgoing>Flow_0qybf5i</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_0igy339" name="ACCEPT" sourceRef="Activity_10lu0rl" targetRef="Activity_1ciisf3" finova:outcome="ACCEPT" />
    <bpmn:task id="Activity_0nb9j68" name="Legal Review and Limit Creation" finova:stepCode="LEGAL_REVIEW" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="LEGAL_OFFICER" finova:route="factoring-legal-review" finova:routeParameters="[{&#34;name&#34;:&#34;agreementId&#34;,&#34;value&#34;:&#34;entity_id&#34;}]" finova:actions="CLAIM,REJECT,ACCEPT">
      <bpmn:incoming>Flow_0qybf5i</bpmn:incoming>
      <bpmn:outgoing>Flow_16r22h2</bpmn:outgoing>
      <bpmn:outgoing>Flow_0dfqa7w</bpmn:outgoing>
    </bpmn:task>
    <bpmn:sequenceFlow id="Flow_0qybf5i" name="" sourceRef="Activity_1ciisf3" targetRef="Activity_0nb9j68" finova:outcome="" />
    <bpmn:sequenceFlow id="Flow_16r22h2" name="REJECT" sourceRef="Activity_0nb9j68" targetRef="EndEvent_1" finova:outcome="REJECT" />
    <bpmn:sequenceFlow id="Flow_0dfqa7w" name="ACCEPT" sourceRef="Activity_0nb9j68" targetRef="EndEvent_1" />
    <bpmn:sequenceFlow id="Flow_1bcnv9y" name="REJECT" sourceRef="Activity_0pogexl" targetRef="EndEvent_1" finova:outcome="REJECT" />
  </bpmn:process>
  <bpmndi:BPMNDiagram id="BPMNDiagram_1">
    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_FINOVA">
      <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1">
        <dc:Bounds x="1446" y="242" width="36" height="36" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1452" y="278" width="24" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_1_di" bpmnElement="Task_1">
        <dc:Bounds x="1544" y="220" width="120" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1">
        <dc:Bounds x="2066" y="466" width="36" height="36" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="2112" y="477" width="20" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0cefpuo_di" bpmnElement="Activity_0cefpuo">
        <dc:Bounds x="1734" y="220" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0wa22in_di" bpmnElement="Activity_0wa22in">
        <dc:Bounds x="1914" y="220" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0punm0q_di" bpmnElement="Activity_0punm0q">
        <dc:Bounds x="2060" y="220" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0btcy1n_di" bpmnElement="Activity_0btcy1n">
        <dc:Bounds x="2044" y="343" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0pogexl_di" bpmnElement="Activity_0pogexl">
        <dc:Bounds x="1870" y="343" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_10lu0rl_di" bpmnElement="Activity_10lu0rl">
        <dc:Bounds x="1704" y="343" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_1ciisf3_di" bpmnElement="Activity_1ciisf3">
        <dc:Bounds x="1544" y="343" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Activity_0nb9j68_di" bpmnElement="Activity_0nb9j68">
        <dc:Bounds x="1544" y="540" width="100" height="80" />
        <bpmndi:BPMNLabel />
      </bpmndi:BPMNShape>
      <bpmndi:BPMNEdge id="Flow_1_di" bpmnElement="Flow_1">
        <di:waypoint x="1482" y="260" />
        <di:waypoint x="1544" y="260" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_2_di" bpmnElement="Flow_2">
        <di:waypoint x="1664" y="260" />
        <di:waypoint x="1734" y="260" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_1gknbdy_di" bpmnElement="Flow_1gknbdy">
        <di:waypoint x="1834" y="260" />
        <di:waypoint x="1914" y="260" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_1n70sc6_di" bpmnElement="Flow_1n70sc6">
        <di:waypoint x="2084" y="423" />
        <di:waypoint x="2084" y="466" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="2078" y="440" width="43" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_01poi2g_di" bpmnElement="Flow_01poi2g">
        <di:waypoint x="2014" y="260" />
        <di:waypoint x="2060" y="260" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0vi5ko2_di" bpmnElement="Flow_0vi5ko2">
        <di:waypoint x="2160" y="260" />
        <di:waypoint x="2214" y="260" />
        <di:waypoint x="2214" y="383" />
        <di:waypoint x="2144" y="383" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0z38dkw_di" bpmnElement="Flow_0z38dkw">
        <di:waypoint x="2044" y="383" />
        <di:waypoint x="1970" y="383" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1985" y="365" width="45" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0qv4imo_di" bpmnElement="Flow_0qv4imo">
        <di:waypoint x="1870" y="383" />
        <di:waypoint x="1837" y="383" />
        <di:waypoint x="1837" y="363" />
        <di:waypoint x="1804" y="363" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0db6oid_di" bpmnElement="Flow_0db6oid">
        <di:waypoint x="1804" y="403" />
        <di:waypoint x="1837" y="403" />
        <di:waypoint x="1837" y="380" />
        <di:waypoint x="1870" y="380" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1831" y="465" width="43" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0igy339_di" bpmnElement="Flow_0igy339">
        <di:waypoint x="1704" y="383" />
        <di:waypoint x="1644" y="383" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1653" y="365" width="45" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0qybf5i_di" bpmnElement="Flow_0qybf5i">
        <di:waypoint x="1594" y="423" />
        <di:waypoint x="1594" y="540" />
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_16r22h2_di" bpmnElement="Flow_16r22h2">
        <di:waypoint x="1644" y="560" />
        <di:waypoint x="2084" y="560" />
        <di:waypoint x="2084" y="502" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1868" y="537" width="43" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_0dfqa7w_di" bpmnElement="Flow_0dfqa7w">
        <di:waypoint x="1644" y="580" />
        <di:waypoint x="1699" y="580" />
        <di:waypoint x="1699" y="620" />
        <di:waypoint x="2094" y="620" />
        <di:waypoint x="2094" y="499" />
        <bpmndi:BPMNLabel>
          <dc:Bounds x="1875" y="613" width="45" height="14" />
        </bpmndi:BPMNLabel>
      </bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_1bcnv9y_di" bpmnElement="Flow_1bcnv9y">
        <di:waypoint x="1920" y="423" />
        <di:waypoint x="1920" y="484" />
        <di:waypoint x="2066" y="484" />
      </bpmndi:BPMNEdge>
    </bpmndi:BPMNPlane>
  </bpmndi:BPMNDiagram>
<!-- FINOVA_VIEWBOX:{"x":1138.0359955005624,"y":189.458942632171,"width":1096.7379077615299,"height":511.81102362204723} -->
</bpmn:definitions>', '2026-11-18 17:35:20.563727+00', 'SYSTEM', 'SYSTEM', '2026-11-18 17:35:28.373157+00', NULL, NULL, 1);
INSERT INTO public."WFE_DEFINITION_VERSIONS" ("DEFINITION_VERSION_ID", "WORKFLOW_DEFINITION_ID", "VERSION_NO", "BPMN_XML", "RELEASED_AT", "RELEASED_BY", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES (3, 4, 1, '<?xml version="1.0" encoding="UTF-8"?>
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
</bpmn:definitions>', '2026-11-18 17:35:26.170524+00', 'SYSTEM', 'SYSTEM', '2026-11-18 17:35:28.373157+00', NULL, NULL, 1);
INSERT INTO public."WFE_DEFINITION_VERSIONS" ("DEFINITION_VERSION_ID", "WORKFLOW_DEFINITION_ID", "VERSION_NO", "BPMN_XML", "RELEASED_AT", "RELEASED_BY", "CREATED_BY", "CREATED_AT", "UPDATED_BY", "UPDATED_AT", "ROW_VERSION") VALUES (4, 3, 1, '<?xml version="1.0" encoding="UTF-8"?>
<bpmn:definitions xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:bpmn="http://www.omg.org/spec/BPMN/20100524/MODEL" xmlns:bpmndi="http://www.omg.org/spec/BPMN/20100524/DI" xmlns:dc="http://www.omg.org/spec/DD/20100524/DC" xmlns:di="http://www.omg.org/spec/DD/20100524/DI" xmlns:finova="http://finova/workflow" id="Definitions_DISBURSEMENT_REQUEST_WF" targetNamespace="http://finova/workflow">
  <bpmn:process id="Process_DISBURSEMENT_REQUEST_WF" name="Disbursement Request Workflow" isExecutable="true" finova:variables=''["ENTITY_ID", "ENTITY_REF", "ENTITY_TYPE", "MODULE_CODE"]''>
    <bpmn:startEvent id="StartEvent_1" name="Start"><bpmn:outgoing>Flow_Start</bpmn:outgoing></bpmn:startEvent>
    <bpmn:userTask id="Task_INITIATION" name="Initiation" finova:stepCode="INITIATION" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="crm-disbursement-request" finova:routeParameters=''[{"name": "disbursementId", "value": "ENTITY_ID"}]'' finova:actions="COMPLETE"><bpmn:incoming>Flow_Start</bpmn:incoming><bpmn:outgoing>Flow_INIT_TO_INVOICE_UPLOAD</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_INVOICE_UPLOAD" name="Invoice Upload" finova:stepCode="INVOICE_UPLOAD" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="OPERATIONS_OFFICER" finova:route="crm-disbursement-invoice-entry" finova:routeParameters=''[{"name": "disbursementId", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_INIT_TO_INVOICE_UPLOAD</bpmn:incoming><bpmn:outgoing>Flow_FCT_OPERATIONS_TO_TREASUARY</bpmn:outgoing></bpmn:userTask>
    <bpmn:userTask id="Task_TREASURY_DISBURSE" name="Treasury Disburse" finova:stepCode="TREASURY_DISBURSE" finova:stepType="HumanTask" finova:assignmentType="Role" finova:assignee="TREASURY_OFFICER" finova:route="trs-disburse" finova:routeParameters=''[{"name": "disbursementId", "value": "ENTITY_ID"}]'' finova:actions="CLAIM,START,COMPLETE"><bpmn:incoming>Flow_FCT_OPERATIONS_TO_TREASUARY</bpmn:incoming><bpmn:outgoing>Flow_TREASURY_TO_COMPLETE</bpmn:outgoing></bpmn:userTask>
    <bpmn:endEvent id="EndEvent_1" name="Workflow Completed"><bpmn:incoming>Flow_TREASURY_TO_COMPLETE</bpmn:incoming></bpmn:endEvent>
    <bpmn:sequenceFlow id="Flow_Start" sourceRef="StartEvent_1" targetRef="Task_INITIATION" />
    <bpmn:sequenceFlow id="Flow_INIT_TO_INVOICE_UPLOAD" sourceRef="Task_INITIATION" targetRef="Task_INVOICE_UPLOAD" name="Initiation → Invoice Upload" />
    <bpmn:sequenceFlow id="Flow_FCT_OPERATIONS_TO_TREASUARY" sourceRef="Task_INVOICE_UPLOAD" targetRef="Task_TREASURY_DISBURSE" name="SALES TO TREASUARY" />
    <bpmn:sequenceFlow id="Flow_TREASURY_TO_COMPLETE" sourceRef="Task_TREASURY_DISBURSE" targetRef="EndEvent_1" name="TREASURY TO COMPLETE" />
  </bpmn:process>
  <bpmndi:BPMNDiagram id="BPMNDiagram_1">
    <bpmndi:BPMNPlane id="BPMNPlane_1" bpmnElement="Process_DISBURSEMENT_REQUEST_WF">
      <bpmndi:BPMNShape id="StartEvent_1_di" bpmnElement="StartEvent_1"><dc:Bounds x="120" y="182" width="36" height="36" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_INITIATION_di" bpmnElement="Task_INITIATION"><dc:Bounds x="200" y="160" width="130" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_INVOICE_UPLOAD_di" bpmnElement="Task_INVOICE_UPLOAD"><dc:Bounds x="380" y="160" width="130" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="Task_TREASURY_DISBURSE_di" bpmnElement="Task_TREASURY_DISBURSE"><dc:Bounds x="560" y="160" width="130" height="80" /></bpmndi:BPMNShape>
      <bpmndi:BPMNShape id="EndEvent_1_di" bpmnElement="EndEvent_1"><dc:Bounds x="740" y="182" width="36" height="36" /></bpmndi:BPMNShape>
      <bpmndi:BPMNEdge id="Flow_Start_di" bpmnElement="Flow_Start"><di:waypoint x="156" y="200" /><di:waypoint x="200" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_INIT_TO_INVOICE_UPLOAD_di" bpmnElement="Flow_INIT_TO_INVOICE_UPLOAD"><di:waypoint x="330" y="200" /><di:waypoint x="380" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_FCT_OPERATIONS_TO_TREASUARY_di" bpmnElement="Flow_FCT_OPERATIONS_TO_TREASUARY"><di:waypoint x="510" y="200" /><di:waypoint x="560" y="200" /></bpmndi:BPMNEdge>
      <bpmndi:BPMNEdge id="Flow_TREASURY_TO_COMPLETE_di" bpmnElement="Flow_TREASURY_TO_COMPLETE"><di:waypoint x="690" y="200" /><di:waypoint x="740" y="200" /></bpmndi:BPMNEdge>
    </bpmndi:BPMNPlane>
  </bpmndi:BPMNDiagram>
</bpmn:definitions>', '2026-11-18 17:35:23.187862+00', 'SYSTEM', 'SYSTEM', '2026-11-18 17:35:28.373157+00', NULL, NULL, 1);

SELECT pg_catalog.setval('public."ADM_CHARGES_CHARGE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."ADM_LKP_DOCUMENT_STATUS_CODES_STATUS_ID_seq"', 3, true);

SELECT pg_catalog.setval('public."ADM_LKP_DOCUMENT_TYPES_DOCUMENT_TYPE_ID_seq"', 4, true);

SELECT pg_catalog.setval('public."ADM_MODULE_SETTINGS_SETTING_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."ADM_MODULE_SETTING_HISTORY_HISTORY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."ADM_PRODUCTS_PRODUCT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."ADM_PRODUCT_ACTIONS_PRODUCT_ACTION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."ADM_PRODUCT_EVENT_RULES_PRODUCT_EVENT_RULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."ADM_PRODUCT_EVENT_RULE_CHARGES_RULE_CHARGE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."ADM_PRODUCT_EVENT_RULE_GL_LINES_RULE_GL_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."ADM_RATE_INDEXES_RATE_INDEX_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."ADM_RATE_INDEX_VALUES_RATE_VALUE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CHQ_BANK_CHARGES_CHARGE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CHQ_BANK_STATEMENTS_STATEMENT_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CHQ_CHEQUES_CHEQUE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CHQ_CHEQUE_STATUS_HIST_CHEQUE_STATUS_HISTORY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CHQ_DEPOSIT_BATCHES_DEPOSIT_BATCH_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CHQ_EVENTS_EVENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CHQ_EVENT_GROUPS_MOVEMENT_GROUP_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CHQ_REPLACEMENTS_REPLACEMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CHQ_UPLOAD_BATCHES_BATCH_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CHQ_UPLOAD_LINES_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CRM_CLIENTS_CLIENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CRM_CLIENT_BANK_ACCOUNTS_BANK_ACCOUNT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CRM_CLIENT_BANK_ACCOUNT_HISTORY_BANK_ACCOUNT_HISTORY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CRM_CLIENT_COMPLIANCE_HISTORY_COMPLIANCE_HISTORY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CRM_CLIENT_ROLES_CLIENT_ROLE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CRM_LKP_COUNTRIES_COUNTRY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CRM_LKP_CURRENCIES_CURRENCY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."CRM_LKP_STATUS_CODES_STATUS_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_AGREEMENTS_AGREEMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_AGREEMENT_DEBTORS_AGRMT_DEBTOR_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_AGREEMENT_DOCUMENTS_DOCUMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_COLLECTION_BATCHES_COLLECTION_BATCH_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_COLLECTION_BATCH_LINES_COLLECTION_BATCH_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_CREDIT_LIMITS_CREDIT_LIMIT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_CUSTOMER_CLAIMS_CLAIM_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_CUSTOMER_CLAIM_LINES_CLAIM_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_CUSTOMER_CLAIM_PAYMENTS_CLAIM_PAYMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_DISBURSEMENTS_DISBURSEMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_DISBURSEMENT_CHARGES_DISBURSEMENT_CHARGE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_DISBURSEMENT_CHEQUES_DISBURSEMENT_CHEQUE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_DISBURSEMENT_CUSTOM_SCHEDULES_CUSTOM_SCHEDULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_INVOICES_INVOICE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_INVOICE_BATCHES_BATCH_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_OFFERS_OFFER_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_REPAYMENTS_SCHEDULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_REPAYMENT_ALLOCATIONS_ALLOCATION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_REPAYMENT_REQUESTS_REPAYMENT_REQUEST_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_REPAYMENT_SOURCES_REPAYMENT_SOURCE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FCT_REPAYMENT_TRANSACTIONS_REPAYMENT_TXN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FIN_BRANCHES_BRANCH_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FIN_BUSINESS_LINES_BUSINESS_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FIN_COST_CENTERS_COST_CENTER_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."FIN_LEGAL_ENTITIES_LEGAL_ENTITY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNTING_ACCOUNT_LOOKUPS_ACCOUNT_LOOKUP_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES_LOOKUP_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNTING_EVENTS_ACCOUNTING_EVENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNTING_EVENT_TYPES_EVENT_TYPE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNTING_EVENT_VALUES_EVENT_VALUE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNTING_PERIODS_PERIOD_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNTS_ACCOUNT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNT_BALANCES_BALANCE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNT_COMBINATIONS_ACCOUNT_COMBINATION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNT_COMBINATION_RULES_RULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNT_COMBINATION_RULE_LINES_RULE_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNT_COMBINATION_SEGMENTS_COMBINATION_SEGMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNT_HIERARCHIES_HIERARCHY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ACCOUNT_HIERARCHY_NODES_NODE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_AUTO_ACCOUNTING_CONDITIONS_CONDITION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_AUTO_ACCOUNTING_GENERATED_LINES_GENERATED_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_AUTO_ACCOUNTING_GENERATIONS_GENERATION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_AUTO_ACCOUNTING_RULE_LINES_RULE_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_AUTO_ACCOUNTING_RULE_SETS_RULE_SET_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_AUTO_RULE_APPROVAL_HIST_APPROVAL_HISTORY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_AUTO_RULE_AUDITS_RULE_AUDIT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_AUTO_RULE_LINE_SEGMENTS_RULE_LINE_SEGMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_AUTO_RULE_TEST_CASES_TEST_CASE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_AUTO_RULE_TEST_RESULTS_TEST_RESULT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_BALANCE_REBUILD_LINES_BALANCE_REBUILD_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_BALANCE_REBUILD_RUNS_BALANCE_REBUILD_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CALENDARS_CALENDAR_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_ACCOUNT_GROUPS_ACCOUNT_GROUP_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_ACCOUNT_GROUP_LINES_ACCOUNT_GROUP_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_ACCRUAL_LINES_ACCRUAL_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_ACCRUAL_RULES_ACCRUAL_RULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_ACCRUAL_RULE_LINES_ACCRUAL_RULE_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_ACCRUAL_RUNS_ACCRUAL_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_ACTION_HIST_CLOSE_ACTION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_ATTACHMENTS_CLOSE_ATTACHMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_COMMENTS_CLOSE_COMMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_EXCEPTIONS_CLOSE_EXCEPTION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_PERIOD_LOCKS_PERIOD_LOCK_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_PROFILES_CLOSE_PROFILE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_PROFILE_MODULES_PROFILE_MODULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_PROFILE_STEPS_CLOSE_STEP_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_RECURRING_RUNS_CLOSE_RECURRING_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_REPORTS_CLOSE_REPORT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_RUNS_CLOSE_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_RUN_MODULE_STATUSES_RUN_MODULE_STATUS_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_RUN_TASKS_CLOSE_RUN_TASK_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_VALIDATION_RESULTS_VALIDATION_RESULT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_VALIDATION_RULES_VALIDATION_RULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CLOSE_VALIDATION_SETS_VALIDATION_SET_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_COA_SEGMENTS_COA_SEGMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_COA_SEGMENT_VALUES_SEGMENT_VALUE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_COA_STRUCTURES_COA_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CURRENCY_EXPOSURE_LINES_EXPOSURE_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_CURRENCY_EXPOSURE_SNAPSHOTS_EXPOSURE_SNAPSHOT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_DAILY_BALANCES_DAILY_BALANCE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_DAILY_RATES_DAILY_RATE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_EXTERNAL_GL_SYSTEMS_EXTERNAL_SYSTEM_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_FISCAL_YEARS_FISCAL_YEAR_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_FX_RATE_USAGE_LOGS_RATE_USAGE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_FX_REVALUATION_LINES_REVALUATION_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_FX_REVALUATION_RULES_REVALUATION_RULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_FX_REVALUATION_RUNS_REVALUATION_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_ACTION_HIST_ACTION_HISTORY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_APPROVAL_HIST_APPROVAL_HISTORY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_ATTACHMENTS_ATTACHMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_BATCHES_BATCH_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_CATEGORIES_CATEGORY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_COMMENTS_COMMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_HEADERS_JOURNAL_HEADER_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_INTERFACE_BATCHES_INTERFACE_BATCH_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_INTERFACE_LINES_INTERFACE_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_LINES_JOURNAL_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_NUMBERING_RULES_NUMBERING_RULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_SOURCES_SOURCE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_VALIDATION_RESULTS_VALIDATION_RESULT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_JOURNAL_VALIDATION_RULES_VALIDATION_RULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_LEDGERS_LEDGER_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_LEDGER_BRANCHES_LEDGER_BRANCH_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_LEDGER_OPTIONS_LEDGER_OPTION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_LEDGER_PERIODS_LEDGER_PERIOD_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_OPENING_BALANCE_LINES_OPENING_BALANCE_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_OPENING_BALANCE_RUNS_OPENING_BALANCE_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_ACCOUNT_MAPS_ACCOUNT_MAP_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_CATEGORY_MAPS_CATEGORY_MAP_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_EXPORT_AUDITS_AUDIT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_EXPORT_BATCHES_EXPORT_BATCH_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_EXPORT_HEADERS_EXPORT_HEADER_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_EXPORT_LINES_EXPORT_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_EXPORT_PROFILES_EXPORT_PROFILE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_IMPORT_REQUESTS_IMPORT_REQUEST_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_IMPORT_RESULTS_IMPORT_RESULT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_INTERFACE_ROWS_INTERFACE_ROW_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_LEDGER_MAPS_LEDGER_MAP_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_RECONCILIATIONS_RECONCILIATION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_SOURCE_FEEDBACKS_FEEDBACK_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_SOURCE_MAPS_SOURCE_MAP_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_ORACLE_GL_VALIDATION_RESULTS_VALIDATION_RESULT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_PERIOD_ACCESS_EXCEPTIONS_PERIOD_EXCEPTION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_PERIOD_CLOSE_CHECKLISTS_CLOSE_CHECKLIST_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_PERIOD_CLOSE_TASKS_CLOSE_TASK_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_PERIOD_CONTROL_RULES_PERIOD_RULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_PERIOD_CONTROL_RULE_LINES_PERIOD_RULE_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_PERIOD_POSTING_WINDOWS_POSTING_WINDOW_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_PERIOD_SOURCE_STATUSES_SOURCE_STATUS_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_PERIOD_STATUS_HIST_PERIOD_HISTORY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_BALANCE_IMPACTS_BALANCE_IMPACT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_EXCEPTIONS_POSTING_EXCEPTION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_LOCKS_POSTING_LOCK_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_PROFILES_POSTING_PROFILE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_PROFILE_RULES_PROFILE_RULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_QUEUES_QUEUE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_RECONCILIATIONS_POSTING_RECON_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_ROLLBACK_LINES_ROLLBACK_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_ROLLBACK_RUNS_ROLLBACK_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_RUNS_POSTING_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_RUN_LINES_POSTING_RUN_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_SCHEDULES_POSTING_SCHEDULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_SEQUENCE_AUDITS_POSTING_SEQUENCE_AUDIT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_SOURCE_FEEDBACKS_SOURCE_FEEDBACK_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_STATUS_EVENTS_POSTING_STATUS_EVENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_VALIDATION_RESULTS_POSTING_VALIDATION_RESULT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_VALIDATION_RULES_POSTING_VALIDATION_RULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_POSTING_VALIDATION_SETS_VALIDATION_SET_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_RATE_IMPORT_BATCHES_IMPORT_BATCH_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_RATE_IMPORT_LINES_IMPORT_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_RATE_TYPES_RATE_TYPE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_RECURRING_JOURNAL_LINES_RECURRING_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_RECURRING_JOURNAL_RUNS_RECURRING_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_RECURRING_JOURNAL_TEMPLATES_RECURRING_TEMPLATE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_RETAINED_EARNINGS_TRANSFERS_RETAINED_TRANSFER_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_SETUP_AUDITS_AUDIT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_SOURCE_POSTING_LINKS_SOURCE_POSTING_LINK_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_SOURCE_VALUE_DEFINITIONS_SOURCE_VALUE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_YEAR_END_CLOSE_LINES_YEAR_END_CLOSE_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."GL_YEAR_END_CLOSE_RUNS_YEAR_END_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."LND_ALLOCATIONS_ALLOCATION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."LND_DISBURSEMENTS_DISBURSEMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."LND_DISBURSEMENT_CHARGES_DISBURSEMENT_CHARGE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."LND_EOD_RUNS_EOD_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."LND_INSTALLMENTS_INSTALLMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."LND_LOANS_LOAN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."LND_LOAN_EVENTS_LOAN_EVENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."LND_LOAN_STATUS_HIST_LOAN_STATUS_HIST_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."LND_PARAMETER_VALUES_PARAMETER_VALUE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."LND_PRODUCT_VERSIONS_PRODUCT_VERSION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."LND_WRITE_OFFS_WRITE_OFF_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_ACCOUNTING_ENTRIES_ENTRY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_APPEALS_APPEAL_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_APPLICATIONS_APPLICATION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_APPLICATION_DOCUMENTS_DOCUMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_APPLICATION_PARTIES_APPLICATION_PARTY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_APPLICATION_PROPERTIES_APP_PROPERTY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_APPLICATION_STATUS_HIST_STATUS_HIST_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_APPRAISALS_APPRAISAL_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_AUDIT_LOGS_AUDIT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_CALCULATION_RUNS_CALC_RUN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_CALCULATION_SCHEDULES_CALC_SCHEDULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_CONTRACTS_CONTRACT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_CREDIT_DECISIONS_DECISION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_CREDIT_INQUIRIES_INQUIRY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_CREDIT_STUDIES_CREDIT_STUDY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_DELINQUENCIES_DELINQUENCY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_EARLY_SETTLEMENTS_SETTLEMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_FINANCE_OFFERS_OFFER_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_GL_ACCOUNT_MAPS_MAP_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_HOUSING_ANNOUNCEMENTS_ANNOUNCEMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_HOUSING_PROJECTS_PROJECT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_IMPORT_BATCHES_BATCH_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_IMPORT_STAGINGS_STAGING_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_INCOME_SOURCES_INCOME_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_INSTALLMENTS_INSTALLMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_INSURANCE_POLICIES_POLICY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_INTEGRATION_LOGS_INTEGRATION_LOG_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_LEGAL_ACTIONS_LEGAL_ACTION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_LEGAL_REVIEWS_LEGAL_REVIEW_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_LOANS_LOAN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_LOAN_EVENTS_LOAN_EVENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_LOAN_MONTHLY_SNAPSHOTS_SNAPSHOT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_LOAN_RESTRUCTURINGS_RESTRUCTURING_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_ORGANIZATIONS_ORG_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_PAYMENTS_PAYMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_PERSONS_PERSON_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_PROGRAM_DOC_REQUIREMENTS_REQUIREMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_PROGRAM_PARAMS_PARAM_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_PROGRAM_PARAM_HIST_PARAM_HIST_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_PROGRAM_VERSIONS_PROGRAM_VERSION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_PROPERTIES_PROPERTY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_PROTOCOLS_PROTOCOL_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_PROTOCOL_PARTIES_PROTOCOL_PARTY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_REVIEW_CHECKS_CHECK_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_SUBSIDY_ACCOUNTS_SUBSIDY_ACCOUNT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_SUBSIDY_APPROVALS_SUBSIDY_APPROVAL_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_SUBSIDY_MOVEMENTS_MOVEMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MFF_WORK_ASSIGNMENTS_ASSIGNMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_ACCOUNT_PAYMENT_ACCOUNT_PAYMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_ACCOUNT_SNAPSHOTS_SNAPSHOT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_AML_RESULT_AML_RESULT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_DEVELOPER_DEVELOPER_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_DEVELOPER_SIGNATORY_DEVELOPER_SIGNATORY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_DUE_DILIGENCES_DD_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_FINANCIAL_STUDIES_FIN_STUDY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_GUARANTEES_GUARANTEE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_INQUIRY_RESULT_INQUIRY_RESULT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_INSURANCES_INSURANCE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_INVESTOR_INVESTOR_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_LEGAL_REVIEW_LEGAL_REVIEW_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PORTFOLIO_ACCOUNT_PORTFOLIO_ACCOUNT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PORTFOLIO_CHEQUE_CHEQUE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PORTFOLIO_FINANCIAL_REVIEW_FINANCIAL_REVIEW_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PORTFOLIO_IMPORT_PORTFOLIO_IMPORT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PORTFOLIO_PORTFOLIO_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PORTFOLIO_STATUS_HISTORY_PORTFOLIO_STATUS_HISTORY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PROJECT_PROJECT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PROJECT_UNIT_PROJECT_UNIT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PURCHASED_PORTFOLIO_APPROVALS_APPROVAL_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PURCHASED_PORTFOLIO_DISBURSEMENTS_DISBURSEMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PURCHASED_PORTFOLIO_DISBURSEMENT_DISBURSEMENT_CHARGE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PURCHASED_PORTFOLIO_DOCUMENTS_DOCUMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PURCHASED_PORTFOLIO_INCOME_SCHEDULES_INCOME_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PURCHASED_PORTFOLIO_MEMOS_MEMO_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PURCHASED_PORTFOLIO_REPAYMENT_SCHEDULE_SCHEDULE_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PURCHASED_PORTFOLIO_SETTLEMENTS_SETTLEMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_PURCHASED_PORTFOLIO_SNAPSHOTS_SNAPSHOT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_SECURITY_DEPOSITS_DEPOSIT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."MTG_VALUATION_VALUATION_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."SEC_ROLE_CLAIM_ROLE_CLAIM_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."SEC_USER_CLAIM_USER_CLAIM_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TLS_LOAN_SCENARIOS_SCENARIO_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TLS_LOAN_SCENARIO_LINES_SCHEDULE_LINE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TLS_LOAN_SCENARIO_RATES_SCENARIO_RATE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_BANKS_BANK_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_BANK_ACCOUNTS_ACCOUNT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_BANK_RULES_RULE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_CALENDARS_CALENDAR_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_CREDIT_FACILITIES_FACILITY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_DISBURSEMENT_FUNDINGS_FUNDING_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_FACILITY_CALENDAR_DAYS_CALENDAR_DAY_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_FACILITY_REPAYMENT_PLANS_REPAYMENT_PLAN_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_FACILITY_RUNOFF_OPTIONS_RUNOFF_OPTIONS_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_FACILITY_SUBLIMITS_SUBLIMIT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_FACILITY_TRANCHES_TRANCHE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_SUBLIMIT_MOVEMENTS_MOVEMENT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_SUBLIMIT_SCOPES_SCOPE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."TRS_SUPPLIER_TRANSFERS_TRANSFER_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."WFE_ALERTS_ALERT_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."WFE_DATA_SOURCES_DATA_SOURCE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."WFE_DATA_SOURCE_ITEMS_DATA_SOURCE_ITEM_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."WFE_DEFINITION_VERSIONS_DEFINITION_VERSION_ID_seq"', 4, true);

SELECT pg_catalog.setval('public."WFE_FORMS_FORM_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."WFE_INBOX_INBOX_ITEM_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."WFE_INSTANCE_HIST_INSTANCE_HIST_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."WFE_INSTANCE_VARIABLES_INSTANCE_VARIABLE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."WFE_OUT_OF_OFFICE_ENTRIES_OUT_OF_OFFICE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."WFE_STEP_INSTANCES_STEP_INSTANCE_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."WFE_TASKS_TASK_ID_seq"', 1, false);

SELECT pg_catalog.setval('public."WFE_WORKFLOW_DEFINITIONS_WORKFLOW_DEFINITION_ID_seq"', 4, true);

SELECT pg_catalog.setval('public."WFE_WORKFLOW_INSTANCES_WORKFLOW_INSTANCE_ID_seq"', 1, false);


