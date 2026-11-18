

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

-- Identity columns continue after the rows above (by table and column, so the sequences' own names do not matter).
DO $$
DECLARE
    r record;
    m bigint;
BEGIN
    FOR r IN
        SELECT c.table_name, c.column_name
        FROM information_schema.columns c
        WHERE c.table_schema = 'public' AND c.is_identity = 'YES'
    LOOP
        EXECUTE format('SELECT max(%I) FROM %I', r.column_name, r.table_name) INTO m;
        IF m IS NOT NULL THEN
            PERFORM setval(pg_get_serial_sequence(format('%I', r.table_name), r.column_name), m);
        END IF;
    END LOOP;
END $$;
