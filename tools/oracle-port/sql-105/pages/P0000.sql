-- Page 0 (Global Page), region "Navigation Badges": the open exceptions count of window.FINOVA_NAV.
-- name: EXCEPTIONS
select count(*) EXCEPTIONS from FNV_V_PORTFOLIO_EXCEPTIONS

-- Application process FNV_FIND_PORTFOLIO (On Demand): the header search, 8 portfolios matching :Q (upper, at least 2 characters).
-- name: FIND_PORTFOLIO
select PORTFOLIO_ID, PORTFOLIO_NO, DEVELOPER_NAME_EN, PROJECT_NAME_EN, STATUS_NAME_EN
  from FNV_V_PORTFOLIO_WORKLIST
 where instr(upper(PORTFOLIO_NO), :Q) > 0
    or instr(upper(DEVELOPER_NAME_EN), :Q) > 0
    or instr(upper(PROJECT_NAME_EN), :Q) > 0
 order by case when upper(PORTFOLIO_NO) like :Q||'%' then 0 else 1 end, REQUEST_DATE desc nulls last
 fetch first 8 rows only
