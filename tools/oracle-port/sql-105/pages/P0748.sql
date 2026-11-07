-- Page 748, region "Report Summary": the KPIs and the top 5 of the report, over the report's own query.
-- name: KPIS
select count(*) K1, count(distinct portfolio_no) K2, avg(days_in_status) K3, count(case when upper(to_status) like '%REJECT%' then 1 end) K4, count(distinct changed_by) K5
    from (
      with h as (
          select
              sh.*,
              lead(sh.change_date) over (
                  partition by sh.portfolio_id
                  order by sh.change_date, sh.status_hist_id
              ) next_change_date
          from FNV_PURSHASED_PORTFOLIO_STATUS_HIST sh
      )
      select
          h.status_hist_id,
          p.portfolio_no,
          h.from_status,
          fs.status_name_en from_status_name_en,
          fs.status_name_ar from_status_name_ar,
          h.to_status,
          ts.status_name_en to_status_name_en,
          ts.status_name_ar to_status_name_ar,
          h.change_date,
          h.next_change_date,
          round(
              cast(nvl(h.next_change_date, systimestamp) as date)
              - cast(h.change_date as date),
              2
          ) days_in_status,
          h.changed_by,
          h.comments
      from h
      join FNV_PURSHASED_PORTFOLIO p
          on p.portfolio_id = h.portfolio_id
      left join REF_PORTFOLIO_STATUS fs
          on fs.status_code = h.from_status
      left join REF_PORTFOLIO_STATUS ts
          on ts.status_code = h.to_status
      where
          (:P748_PORTFOLIO_ID is null
           or h.portfolio_id = :P748_PORTFOLIO_ID)
      
          and (:P748_FROM_STATUS is null
               or h.from_status = :P748_FROM_STATUS)
      
          and (:P748_TO_STATUS is null
               or h.to_status = :P748_TO_STATUS)
      
          and (:P748_DATE_FROM is null
               or h.change_date >= cast(
                      to_date(:P748_DATE_FROM, 'DD-MM-YYYY')
                      as timestamp
                  ))
      
          and (:P748_DATE_TO is null
               or h.change_date < cast(
                      to_date(:P748_DATE_TO, 'DD-MM-YYYY') + 1
                      as timestamp
                  ))
      
          and (:P748_CHANGED_BY is null
               or upper(h.changed_by) like
                  '%' || upper(:P748_CHANGED_BY) || '%')
    ) q
-- name: TOP
select lbl, val, max(val) over () mx, sum(val) over () tot from (
             select nvl(to_status_name_en, to_status) lbl, count(*) val from (
      with h as (
          select
              sh.*,
              lead(sh.change_date) over (
                  partition by sh.portfolio_id
                  order by sh.change_date, sh.status_hist_id
              ) next_change_date
          from FNV_PURSHASED_PORTFOLIO_STATUS_HIST sh
      )
      select
          h.status_hist_id,
          p.portfolio_no,
          h.from_status,
          fs.status_name_en from_status_name_en,
          fs.status_name_ar from_status_name_ar,
          h.to_status,
          ts.status_name_en to_status_name_en,
          ts.status_name_ar to_status_name_ar,
          h.change_date,
          h.next_change_date,
          round(
              cast(nvl(h.next_change_date, systimestamp) as date)
              - cast(h.change_date as date),
              2
          ) days_in_status,
          h.changed_by,
          h.comments
      from h
      join FNV_PURSHASED_PORTFOLIO p
          on p.portfolio_id = h.portfolio_id
      left join REF_PORTFOLIO_STATUS fs
          on fs.status_code = h.from_status
      left join REF_PORTFOLIO_STATUS ts
          on ts.status_code = h.to_status
      where
          (:P748_PORTFOLIO_ID is null
           or h.portfolio_id = :P748_PORTFOLIO_ID)
      
          and (:P748_FROM_STATUS is null
               or h.from_status = :P748_FROM_STATUS)
      
          and (:P748_TO_STATUS is null
               or h.to_status = :P748_TO_STATUS)
      
          and (:P748_DATE_FROM is null
               or h.change_date >= cast(
                      to_date(:P748_DATE_FROM, 'DD-MM-YYYY')
                      as timestamp
                  ))
      
          and (:P748_DATE_TO is null
               or h.change_date < cast(
                      to_date(:P748_DATE_TO, 'DD-MM-YYYY') + 1
                      as timestamp
                  ))
      
          and (:P748_CHANGED_BY is null
               or upper(h.changed_by) like
                  '%' || upper(:P748_CHANGED_BY) || '%')
             ) q group by nvl(to_status_name_en, to_status)) order by val desc nulls last fetch first 5 rows only
