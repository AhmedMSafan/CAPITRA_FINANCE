select a."APPLICATION_ID", a."APPLICATION_NO", a."STATUS_CODE", a."CURRENT_OWNER_USER", case when a."PRIMARY_PERSON_ID" is not null then 'Y' else 'N' end "APPLICANT_READY", case when exists (select 1
from "MFF_APPLICATION_PARTIES" ap
join "MFF_INCOME_SOURCES" i on i."APPLICATION_PARTY_ID" = ap."APPLICATION_PARTY_ID"
where ap."APPLICATION_ID" = a."APPLICATION_ID" and coalesce(i."IS_VERIFIED", false)) then 'Y' else 'N' end "INCOME_READY", case when exists (select 1
from "MFF_APPLICATION_PROPERTYS" x
where x."APPLICATION_ID" = a."APPLICATION_ID" and x."IS_SELECTED") then 'Y' else 'N' end "PROPERTY_READY", (select count(*)
from "MFF_APPLICATION_DOCUMENTS" d
where d."APPLICATION_ID" = a."APPLICATION_ID") "DOCUMENT_COUNT", (select count(*)
from "MFF_APPLICATION_DOCUMENTS" d
where d."APPLICATION_ID" = a."APPLICATION_ID" and coalesce(d."VERIFICATION_STATUS", 'PENDING') = 'PENDING') "DOCUMENT_PENDING_COUNT", (select count(*)
from "MFF_APPLICATION_DOCUMENTS" d
where d."APPLICATION_ID" = a."APPLICATION_ID" and d."VERIFICATION_STATUS" = 'REJECTED') "DOCUMENT_REJECTED_COUNT", (select count(*)
from "MFF_VW_MISSING_DOCUMENTS" md
where "md"."APPLICATION_ID" = a."APPLICATION_ID") "MISSING_REQUIRED_DOCUMENT_COUNT", case when exists (select 1
from "MFF_CREDIT_INQUIRIES" q
where q."APPLICATION_ID" = a."APPLICATION_ID" and q."RESULT_CODE" = 'PASS') then 'Y' else 'N' end "CREDIT_INQUIRY_READY", case when exists (select 1
from "MFF_CREDIT_STUDIES" s
where s."APPLICATION_ID" = a."APPLICATION_ID") then 'Y' else 'N' end "CREDIT_STUDY_READY", case when exists (select 1
from "MFF_CREDIT_DECISIONS" c
where c."APPLICATION_ID" = a."APPLICATION_ID" and c."DECISION_CODE" in ('APPROVE', 'CONDITIONAL')) then 'Y' else 'N' end "CREDIT_DECISION_READY", case when exists (select 1
from "MFF_CALCULATION_RUNS" c
where c."APPLICATION_ID" = a."APPLICATION_ID" and coalesce(c."CALC_STATUS", 'COMPLETE') = 'COMPLETE') then 'Y' else 'N' end "CALCULATION_READY", case when exists (select 1
from "MFF_SUBSIDY_APPROVALS" s
where s."APPLICATION_ID" = a."APPLICATION_ID" and s."APPROVAL_STATUS" = 'APPROVED') then 'Y' else 'N' end "SUBSIDY_READY", case when exists (select 1
from "MFF_FINANCE_OFFERS" o
where o."APPLICATION_ID" = a."APPLICATION_ID" and o."OFFER_STATUS" in ('ISSUED', 'ACCEPTED')) then 'Y' else 'N' end "OFFER_READY", case when exists (select 1
from "MFF_CONTRACTS" c
where c."APPLICATION_ID" = a."APPLICATION_ID" and (c."IS_SIGNED" or c."CONTRACT_STATUS" = 'SIGNED')) then 'Y' else 'N' end "CONTRACT_READY", case when exists (select 1
from "MFF_LOANS" l
where l."APPLICATION_ID" = a."APPLICATION_ID") then 'Y' else 'N' end "LOAN_READY", (select count(*)
from "MFF_WORK_ASSIGNMENTS" w
where w."APPLICATION_ID" = a."APPLICATION_ID" and coalesce(w."ASSIGNMENT_STATUS", 'OPEN') <> 'COMPLETED') "OPEN_TASKS", (select count(*)
from "MFF_WORK_ASSIGNMENTS" w
where w."APPLICATION_ID" = a."APPLICATION_ID" and coalesce(w."ASSIGNMENT_STATUS", 'OPEN') <> 'COMPLETED' and w."DUE_DATE" < current_date) "OVERDUE_TASKS", round(100 * (case when a."PRIMARY_PERSON_ID" is not null then 1 else 0 end + case when exists (select 1
from "MFF_APPLICATION_PARTIES" ap
join "MFF_INCOME_SOURCES" i on i."APPLICATION_PARTY_ID" = ap."APPLICATION_PARTY_ID"
where ap."APPLICATION_ID" = a."APPLICATION_ID" and coalesce(i."IS_VERIFIED", false)) then 1 else 0 end + case when exists (select 1
from "MFF_APPLICATION_PROPERTYS" x
where x."APPLICATION_ID" = a."APPLICATION_ID" and x."IS_SELECTED") then 1 else 0 end + case when exists (select 1
from "MFF_APPLICATION_DOCUMENTS" d
where d."APPLICATION_ID" = a."APPLICATION_ID") and not exists (select 1
from "MFF_VW_MISSING_DOCUMENTS" md
where "md"."APPLICATION_ID" = a."APPLICATION_ID") and not exists (select 1
from "MFF_APPLICATION_DOCUMENTS" d
where d."APPLICATION_ID" = a."APPLICATION_ID" and d."VERIFICATION_STATUS" = 'REJECTED') then 1 else 0 end + case when exists (select 1
from "MFF_CREDIT_INQUIRIES" q
where q."APPLICATION_ID" = a."APPLICATION_ID" and q."RESULT_CODE" = 'PASS') then 1 else 0 end + case when exists (select 1
from "MFF_CREDIT_STUDIES" s
where s."APPLICATION_ID" = a."APPLICATION_ID") then 1 else 0 end + case when exists (select 1
from "MFF_CREDIT_DECISIONS" c
where c."APPLICATION_ID" = a."APPLICATION_ID" and c."DECISION_CODE" in ('APPROVE', 'CONDITIONAL')) then 1 else 0 end + case when exists (select 1
from "MFF_CALCULATION_RUNS" c
where c."APPLICATION_ID" = a."APPLICATION_ID" and coalesce(c."CALC_STATUS", 'COMPLETE') = 'COMPLETE') then 1 else 0 end + case when exists (select 1
from "MFF_SUBSIDY_APPROVALS" s
where s."APPLICATION_ID" = a."APPLICATION_ID" and s."APPROVAL_STATUS" = 'APPROVED') then 1 else 0 end + case when exists (select 1
from "MFF_FINANCE_OFFERS" o
where o."APPLICATION_ID" = a."APPLICATION_ID" and o."OFFER_STATUS" in ('ISSUED', 'ACCEPTED')) then 1 else 0 end + case when exists (select 1
from "MFF_CONTRACTS" c
where c."APPLICATION_ID" = a."APPLICATION_ID" and (c."IS_SIGNED" or c."CONTRACT_STATUS" = 'SIGNED')) then 1 else 0 end + case when exists (select 1
from "MFF_LOANS" l
where l."APPLICATION_ID" = a."APPLICATION_ID") then 1 else 0 end) / (12)::numeric, 0) "READINESS_PCT"
from "MFF_APPLICATIONS" a
