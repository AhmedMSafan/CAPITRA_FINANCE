-- Page 730, region "Report Summary": the KPIs and the top 5 of the report, over the report's own query.
-- name: KPIS
select count(*) K1, sum(face_value) K2, sum(proposed_purchase_value) K3, 100*(sum(face_value)-sum(proposed_purchase_value))/nullif(sum(face_value),0) K4, sum(account_count) K5
    from (
      select
          p.portfolio_id,
          p.portfolio_no,
          d.developer_code,
          d.legal_name_en developer_name_en,
          d.legal_name_ar developer_name_ar,
          pr.project_code,
          pr.project_name_en,
          pr.project_name_ar,
          p.request_date,
          p.portfolio_status_code,
          ps.status_name_en,
          ps.status_name_ar,
          p.currency_code,
          p.face_value,
          p.outstanding_value,
          p.proposed_purchase_value,
      
          (p.face_value - p.proposed_purchase_value) purchase_discount_amount,
      
          round(
              100 * (p.face_value - p.proposed_purchase_value)
              / nullif(p.face_value, 0),
              2
          ) purchase_discount_pct,
      
          p.expected_return_amount,
          p.term_months,
          p.current_approval_step,
          p.submitted_date,
          p.approved_date,
      
          count(distinct a.portfolio_account_id) account_count,
      
          s.settlement_status_code,
          s.accounting_status_code,
          s.settlement_date
      
      from FNV_PURSHASED_PORTFOLIO p
      
      join FNV_REALESTATE_DEVELOPER d
          on d.developer_id = p.developer_id
      
      join FNV_PROJECT pr
          on pr.project_id = p.project_id
      
      left join REF_PORTFOLIO_STATUS ps
          on ps.status_code = p.portfolio_status_code
      
      left join FNV_PURSHASED_PORTFOLIO_ACCOUNT a
          on a.portfolio_id = p.portfolio_id
      
      left join FNV_PURSHASED_PORTFOLIO_SETTLEMENT s
          on s.portfolio_id = p.portfolio_id
      
      where
          (:P730_DEVELOPER_ID is null
           or p.developer_id = :P730_DEVELOPER_ID)
      
      and
          (:P730_PROJECT_ID is null
           or p.project_id = :P730_PROJECT_ID)
      
      and
          (:P730_STATUS_CODE is null
           or p.portfolio_status_code = :P730_STATUS_CODE)
      
      and
          (:P730_DATE_FROM is null
           or p.request_date >= cast(:P730_DATE_FROM as date))
      
      and
          (:P730_DATE_TO is null
           or p.request_date < cast(:P730_DATE_TO as date) + 1)
      
      and
          (:P730_CURRENCY_CODE is null
           or p.currency_code = :P730_CURRENCY_CODE)
      
      group by
          p.portfolio_id,
          p.portfolio_no,
          d.developer_code,
          d.legal_name_en,
          d.legal_name_ar,
          pr.project_code,
          pr.project_name_en,
          pr.project_name_ar,
          p.request_date,
          p.portfolio_status_code,
          ps.status_name_en,
          ps.status_name_ar,
          p.currency_code,
          p.face_value,
          p.outstanding_value,
          p.proposed_purchase_value,
          p.expected_return_amount,
          p.term_months,
          p.current_approval_step,
          p.submitted_date,
          p.approved_date,
          s.settlement_status_code,
          s.accounting_status_code,
          s.settlement_date
    ) q
-- name: TOP
select lbl, val, max(val) over () mx, sum(val) over () tot from (
             select nvl(status_name_en, portfolio_status_code) lbl, count(*) val from (
      select
          p.portfolio_id,
          p.portfolio_no,
          d.developer_code,
          d.legal_name_en developer_name_en,
          d.legal_name_ar developer_name_ar,
          pr.project_code,
          pr.project_name_en,
          pr.project_name_ar,
          p.request_date,
          p.portfolio_status_code,
          ps.status_name_en,
          ps.status_name_ar,
          p.currency_code,
          p.face_value,
          p.outstanding_value,
          p.proposed_purchase_value,
      
          (p.face_value - p.proposed_purchase_value) purchase_discount_amount,
      
          round(
              100 * (p.face_value - p.proposed_purchase_value)
              / nullif(p.face_value, 0),
              2
          ) purchase_discount_pct,
      
          p.expected_return_amount,
          p.term_months,
          p.current_approval_step,
          p.submitted_date,
          p.approved_date,
      
          count(distinct a.portfolio_account_id) account_count,
      
          s.settlement_status_code,
          s.accounting_status_code,
          s.settlement_date
      
      from FNV_PURSHASED_PORTFOLIO p
      
      join FNV_REALESTATE_DEVELOPER d
          on d.developer_id = p.developer_id
      
      join FNV_PROJECT pr
          on pr.project_id = p.project_id
      
      left join REF_PORTFOLIO_STATUS ps
          on ps.status_code = p.portfolio_status_code
      
      left join FNV_PURSHASED_PORTFOLIO_ACCOUNT a
          on a.portfolio_id = p.portfolio_id
      
      left join FNV_PURSHASED_PORTFOLIO_SETTLEMENT s
          on s.portfolio_id = p.portfolio_id
      
      where
          (:P730_DEVELOPER_ID is null
           or p.developer_id = :P730_DEVELOPER_ID)
      
      and
          (:P730_PROJECT_ID is null
           or p.project_id = :P730_PROJECT_ID)
      
      and
          (:P730_STATUS_CODE is null
           or p.portfolio_status_code = :P730_STATUS_CODE)
      
      and
          (:P730_DATE_FROM is null
           or p.request_date >= cast(:P730_DATE_FROM as date))
      
      and
          (:P730_DATE_TO is null
           or p.request_date < cast(:P730_DATE_TO as date) + 1)
      
      and
          (:P730_CURRENCY_CODE is null
           or p.currency_code = :P730_CURRENCY_CODE)
      
      group by
          p.portfolio_id,
          p.portfolio_no,
          d.developer_code,
          d.legal_name_en,
          d.legal_name_ar,
          pr.project_code,
          pr.project_name_en,
          pr.project_name_ar,
          p.request_date,
          p.portfolio_status_code,
          ps.status_name_en,
          ps.status_name_ar,
          p.currency_code,
          p.face_value,
          p.outstanding_value,
          p.proposed_purchase_value,
          p.expected_return_amount,
          p.term_months,
          p.current_approval_step,
          p.submitted_date,
          p.approved_date,
          s.settlement_status_code,
          s.accounting_status_code,
          s.settlement_date
             ) q group by nvl(status_name_en, portfolio_status_code)) order by val desc nulls last fetch first 5 rows only
