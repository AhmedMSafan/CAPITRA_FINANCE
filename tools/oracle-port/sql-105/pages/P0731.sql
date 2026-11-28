-- Page 731, region "Report Summary": the KPIs and the top 5 of the report, over the report's own query.
-- name: KPIS
select count(*) K1, sum(contract_value) K2, 100*sum(paid_value)/nullif(sum(contract_value),0) K3, sum(outstanding_value) K4, count(case when arrears_amount > 0 then 1 end) K5, sum(arrears_amount) K6
    from (
      select
          a.portfolio_account_id,
          p.portfolio_no,
          i.investor_code,
          i.full_name_en,
          i.full_name_ar,
          i.national_id,
          u.unit_code,
          u.unit_type_code,
          u.building_no,
          u.floor_no,
          u.unit_no,
          a.booking_date,
          a.contract_date,
          a.contract_value,
          a.paid_value,
          a.outstanding_value,
          round(100 * a.paid_value / nullif(a.contract_value,0), 2) paid_pct,
          round(100 * a.outstanding_value / nullif(a.contract_value,0), 2) outstanding_pct,
          a.installment_amount,
          a.installment_frequency,
          a.last_payment_date,
          a.regular_payment_months,
          a.arrears_amount,
          a.account_status
      from FNV_PURSHASED_PORTFOLIO_ACCOUNT a
      join FNV_PURSHASED_PORTFOLIO p
        on p.portfolio_id = a.portfolio_id
      join FNV_INVESTOR i
        on i.investor_id = a.investor_id
      join FNV_PROJECT_UNIT u
        on u.unit_id = a.unit_id
      where (:P731_PORTFOLIO_ID is null or a.portfolio_id = :P731_PORTFOLIO_ID)
        and (:P731_INVESTOR_CODE is null
             or upper(i.investor_code) like '%' || upper(:P731_INVESTOR_CODE) || '%')
        and (:P731_UNIT_CODE is null
             or upper(u.unit_code) like '%' || upper(:P731_UNIT_CODE) || '%')
        and (:P731_ACCOUNT_STATUS is null or a.account_status = :P731_ACCOUNT_STATUS)
        and (nvl(:P731_ARREARS_ONLY,'N') = 'N' or a.arrears_amount > 0)
    ) q
-- name: TOP
select lbl, val, max(val) over () mx, sum(val) over () tot from (
             select portfolio_no lbl, sum(outstanding_value) val from (
      select
          a.portfolio_account_id,
          p.portfolio_no,
          i.investor_code,
          i.full_name_en,
          i.full_name_ar,
          i.national_id,
          u.unit_code,
          u.unit_type_code,
          u.building_no,
          u.floor_no,
          u.unit_no,
          a.booking_date,
          a.contract_date,
          a.contract_value,
          a.paid_value,
          a.outstanding_value,
          round(100 * a.paid_value / nullif(a.contract_value,0), 2) paid_pct,
          round(100 * a.outstanding_value / nullif(a.contract_value,0), 2) outstanding_pct,
          a.installment_amount,
          a.installment_frequency,
          a.last_payment_date,
          a.regular_payment_months,
          a.arrears_amount,
          a.account_status
      from FNV_PURSHASED_PORTFOLIO_ACCOUNT a
      join FNV_PURSHASED_PORTFOLIO p
        on p.portfolio_id = a.portfolio_id
      join FNV_INVESTOR i
        on i.investor_id = a.investor_id
      join FNV_PROJECT_UNIT u
        on u.unit_id = a.unit_id
      where (:P731_PORTFOLIO_ID is null or a.portfolio_id = :P731_PORTFOLIO_ID)
        and (:P731_INVESTOR_CODE is null
             or upper(i.investor_code) like '%' || upper(:P731_INVESTOR_CODE) || '%')
        and (:P731_UNIT_CODE is null
             or upper(u.unit_code) like '%' || upper(:P731_UNIT_CODE) || '%')
        and (:P731_ACCOUNT_STATUS is null or a.account_status = :P731_ACCOUNT_STATUS)
        and (nvl(:P731_ARREARS_ONLY,'N') = 'N' or a.arrears_amount > 0)
             ) q group by portfolio_no) order by val desc nulls last fetch first 5 rows only
