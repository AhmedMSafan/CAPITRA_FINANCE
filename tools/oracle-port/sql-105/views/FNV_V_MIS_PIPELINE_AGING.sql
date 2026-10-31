select "PORTFOLIO_STATUS_CODE", count(*) "CASE_COUNT", sum("PROPOSED_PURCHASE_VALUE") "PIPELINE_VALUE", round(avg(current_date - ("REQUEST_DATE")::date), 1) "AVG_DAYS", max(current_date - ("REQUEST_DATE")::date) "OLDEST_DAYS"
from "MTG_PURCHASED_PORTFOLIOS"
where "PORTFOLIO_STATUS_CODE" not in ('CLOSED')
group by "PORTFOLIO_STATUS_CODE"
