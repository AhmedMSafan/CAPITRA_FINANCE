with req as (select count(*) "REQUIRED_COUNT"
from "ADM_LKP_GUARANTEE_TYPES"
where "IS_MANDATORY" and "IS_ACTIVE"), got as (select "PORTFOLIO_ID", count(distinct "GUARANTEE_TYPE_CODE") "VERIFIED_REQUIRED_COUNT"
from "MTG_GUARANTEES" g
where "IS_VERIFIED" and exists (select 1
from "ADM_LKP_GUARANTEE_TYPES" r
where r."GUARANTEE_TYPE_CODE" = g."GUARANTEE_TYPE_CODE" and r."IS_MANDATORY" and r."IS_ACTIVE")
group by "PORTFOLIO_ID") select p."PORTFOLIO_ID", p."PORTFOLIO_NO", r."REQUIRED_COUNT", coalesce(g."VERIFIED_REQUIRED_COUNT", 0) "VERIFIED_REQUIRED_COUNT", greatest(r."REQUIRED_COUNT" - coalesce(g."VERIFIED_REQUIRED_COUNT", 0), 0) "MISSING_REQUIRED_COUNT", case when coalesce(g."VERIFIED_REQUIRED_COUNT", 0) >= r."REQUIRED_COUNT" then 'Y' else 'N' end "GUARANTEE_READY_FLAG"
from "MTG_PURCHASED_PORTFOLIOS" p
cross join req r
left join got g on g."PORTFOLIO_ID" = p."PORTFOLIO_ID"
