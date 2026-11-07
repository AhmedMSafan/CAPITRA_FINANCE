with req as (select count(*) "REQUIRED_COUNT"
from "ADM_LKP_DOCUMENT_TYPES"
where "IS_MANDATORY" and "IS_ACTIVE"), doc as (select "PORTFOLIO_ID", count(distinct "DOCUMENT_TYPE_CODE") "RECEIVED_REQUIRED_COUNT"
from "MTG_PURCHASED_PORTFOLIO_DOCUMENTS" d
where exists (select 1
from "ADM_LKP_DOCUMENT_TYPES" r
where r."DOCUMENT_TYPE_CODE" = d."DOCUMENT_TYPE_CODE" and r."IS_MANDATORY" and r."IS_ACTIVE")
group by "PORTFOLIO_ID") select p."PORTFOLIO_ID", p."PORTFOLIO_NO", r."REQUIRED_COUNT", coalesce(d."RECEIVED_REQUIRED_COUNT", 0) "RECEIVED_REQUIRED_COUNT", greatest(r."REQUIRED_COUNT" - coalesce(d."RECEIVED_REQUIRED_COUNT", 0), 0) "MISSING_REQUIRED_COUNT", case when r."REQUIRED_COUNT" = 0 then 100 else round(coalesce(d."RECEIVED_REQUIRED_COUNT", 0) / (r."REQUIRED_COUNT")::numeric * 100, 2) end "DOCUMENT_COMPLETION_PCT", case when coalesce(d."RECEIVED_REQUIRED_COUNT", 0) >= r."REQUIRED_COUNT" then 'Y' else 'N' end "DOCUMENT_READY_FLAG"
from "MTG_PURCHASED_PORTFOLIOS" p
cross join req r
left join doc d on d."PORTFOLIO_ID" = p."PORTFOLIO_ID"
