-- Page 732, region "Report Summary": the KPIs and the top 5 of the report, over the report's own query.
-- name: KPIS
select count(*) K1, sum(total_outstanding_value) K2, max(concentration_pct) K3, sum(total_arrears_amount) K4, count(case when total_arrears_amount > 0 then 1 end) K5
    from (
      select
          i.investor_id,
          i.investor_code,
          i.full_name_en,
          i.full_name_ar,
          i.national_id,
          i.declared_income,
          count(distinct a.portfolio_id) portfolio_count,
          count(a.portfolio_account_id) contract_count,
          count(distinct a.unit_id) unit_count,
          sum(a.contract_value) total_contract_value,
          sum(a.paid_value) total_paid_value,
          sum(a.outstanding_value) total_outstanding_value,
          sum(a.arrears_amount) total_arrears_amount,
          round(100 * sum(a.outstanding_value)
                / nullif(sum(sum(a.outstanding_value)) over (),0), 2) concentration_pct
      from FNV_INVESTOR i
      join FNV_PURSHASED_PORTFOLIO_ACCOUNT a
        on a.investor_id = i.investor_id
      join FNV_PURSHASED_PORTFOLIO p
        on p.portfolio_id = a.portfolio_id
      where (:P732_DEVELOPER_ID is null or p.developer_id = :P732_DEVELOPER_ID)
        and (:P732_PROJECT_ID is null or p.project_id = :P732_PROJECT_ID)
        and (:P732_PORTFOLIO_ID is null or p.portfolio_id = :P732_PORTFOLIO_ID)
        and (:P732_INVESTOR_CODE is null
             or upper(i.investor_code) like '%' || upper(:P732_INVESTOR_CODE) || '%')
        and (:P732_ACTIVE_FLAG is null or i.active_flag = :P732_ACTIVE_FLAG)
      group by
          i.investor_id, i.investor_code,
          i.full_name_en, i.full_name_ar,
          i.national_id, i.declared_income
    ) q
-- name: TOP
select lbl, val, max(val) over () mx, sum(val) over () tot from (
             select nvl(full_name_en, investor_code) lbl, sum(total_outstanding_value) val from (
      select
          i.investor_id,
          i.investor_code,
          i.full_name_en,
          i.full_name_ar,
          i.national_id,
          i.declared_income,
          count(distinct a.portfolio_id) portfolio_count,
          count(a.portfolio_account_id) contract_count,
          count(distinct a.unit_id) unit_count,
          sum(a.contract_value) total_contract_value,
          sum(a.paid_value) total_paid_value,
          sum(a.outstanding_value) total_outstanding_value,
          sum(a.arrears_amount) total_arrears_amount,
          round(100 * sum(a.outstanding_value)
                / nullif(sum(sum(a.outstanding_value)) over (),0), 2) concentration_pct
      from FNV_INVESTOR i
      join FNV_PURSHASED_PORTFOLIO_ACCOUNT a
        on a.investor_id = i.investor_id
      join FNV_PURSHASED_PORTFOLIO p
        on p.portfolio_id = a.portfolio_id
      where (:P732_DEVELOPER_ID is null or p.developer_id = :P732_DEVELOPER_ID)
        and (:P732_PROJECT_ID is null or p.project_id = :P732_PROJECT_ID)
        and (:P732_PORTFOLIO_ID is null or p.portfolio_id = :P732_PORTFOLIO_ID)
        and (:P732_INVESTOR_CODE is null
             or upper(i.investor_code) like '%' || upper(:P732_INVESTOR_CODE) || '%')
        and (:P732_ACTIVE_FLAG is null or i.active_flag = :P732_ACTIVE_FLAG)
      group by
          i.investor_id, i.investor_code,
          i.full_name_en, i.full_name_ar,
          i.national_id, i.declared_income
             ) q group by nvl(full_name_en, investor_code)) order by val desc nulls last fetch first 5 rows only
