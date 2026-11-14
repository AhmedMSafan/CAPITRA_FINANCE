-- Page 711, region "Report Summary": the KPIs and the top 5 of the report, over the report's own query.
-- name: KPIS
select count(*) K1, sum(portfolio_count) K2, sum(investor_count) K3, sum(face_value) K4, sum(outstanding_exposure) K5
    from (
      select DEVELOPER_ID,DEVELOPER_CODE,
             nvl(LEGAL_NAME_EN,'-') DEVELOPER,
             PORTFOLIO_COUNT,PROJECT_COUNT,INVESTOR_COUNT,
             FACE_VALUE,PURCHASE_VALUE,OUTSTANDING_EXPOSURE
        from FNV_V_MIS_DEVELOPER_EXPOSURE
    ) q
-- name: TOP
select lbl, val, max(val) over () mx, sum(val) over () tot from (
             select developer lbl, sum(outstanding_exposure) val from (
      select DEVELOPER_ID,DEVELOPER_CODE,
             nvl(LEGAL_NAME_EN,'-') DEVELOPER,
             PORTFOLIO_COUNT,PROJECT_COUNT,INVESTOR_COUNT,
             FACE_VALUE,PURCHASE_VALUE,OUTSTANDING_EXPOSURE
        from FNV_V_MIS_DEVELOPER_EXPOSURE
             ) q group by developer) order by val desc nulls last fetch first 5 rows only
