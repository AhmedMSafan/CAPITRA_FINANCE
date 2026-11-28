-- Oracle: cast(timestamp as date) keeps the time, so days in status has fractions; PostgreSQL ::date would drop them.
with h as (select sh.*, lead(sh."CHANGE_AT") over (partition by sh."PORTFOLIO_ID"
order by sh."CHANGE_AT", sh."STATUS_HIST_ID") "NEXT_CHANGE_DATE"
from "MTG_PURCHASED_PORTFOLIO_STATUS_HIST" sh) select h."STATUS_HIST_ID", p."PORTFOLIO_NO", h."FROM_STATUS", fs."STATUS_NAME_EN" "FROM_STATUS_NAME_EN", fs."STATUS_NAME_AR" "FROM_STATUS_NAME_AR", h."TO_STATUS", ts."STATUS_NAME_EN" "TO_STATUS_NAME_EN", ts."STATUS_NAME_AR" "TO_STATUS_NAME_AR", h."CHANGE_AT" AS "CHANGE_DATE", h."NEXT_CHANGE_DATE", round((extract(epoch from coalesce(h."NEXT_CHANGE_DATE", now()) - h."CHANGE_AT") / 86400)::numeric, 2) "DAYS_IN_STATUS", h."CHANGED_BY", h."COMMENTS"
from h
join "MTG_PURCHASED_PORTFOLIOS" p on p."PORTFOLIO_ID" = h."PORTFOLIO_ID"
left join "MTG_LKP_PORTFOLIO_STATUSES" fs on fs."STATUS_CODE" = h."FROM_STATUS"
left join "MTG_LKP_PORTFOLIO_STATUSES" ts on ts."STATUS_CODE" = h."TO_STATUS" where (@P748_PORTFOLIO_ID::bigint is null or h."PORTFOLIO_ID" = @P748_PORTFOLIO_ID::bigint) and (@P748_FROM_STATUS::text is null or h."FROM_STATUS" = @P748_FROM_STATUS::text) and (@P748_TO_STATUS::text is null or h."TO_STATUS" = @P748_TO_STATUS::text) and (@P748_DATE_FROM::text is null or h."CHANGE_AT" >= cast(to_date(@P748_DATE_FROM::text, 'DD-MM-YYYY') as timestamp)) and (@P748_DATE_TO::text is null or h."CHANGE_AT" < cast(to_date(@P748_DATE_TO::text, 'DD-MM-YYYY') + 1 as timestamp)) and (@P748_CHANGED_BY::text is null or upper(h."CHANGED_BY") like concat('%', upper(@P748_CHANGED_BY::text), '%'))
order by p."PORTFOLIO_NO", h."CHANGE_AT"
