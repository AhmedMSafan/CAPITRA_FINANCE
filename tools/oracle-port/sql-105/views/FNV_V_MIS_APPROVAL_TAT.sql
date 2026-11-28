select a."STEP_CODE", s."STEP_NAME_AR", s."STEP_NAME_EN", count(*) "TASK_COUNT", round(avg(case when a."DECISION_DATE" is not null and a."ASSIGNED_DATE" is not null then a."DECISION_DATE" - a."ASSIGNED_DATE" end), 2) "AVG_TAT_DAYS"
from "MTG_PURCHASED_PORTFOLIO_APPROVALS" a
join "MTG_LKP_APPROVAL_STEPS" s on s."STEP_CODE" = a."STEP_CODE"
group by a."STEP_CODE", s."STEP_NAME_AR", s."STEP_NAME_EN"
