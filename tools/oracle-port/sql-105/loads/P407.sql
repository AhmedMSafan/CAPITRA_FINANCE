-- Page 407, Before Header "Load Cheque Coverage": the select into the page items, and the values of its no_data_found handler.
select case when v.PORTFOLIO_ID is null then '-' else to_char(v.REQUIRED_COVERAGE_AMOUNT,'FM999G999G999G990D00') end P407_REQUIRED_COVERAGE,
       case when v.PORTFOLIO_ID is null then '0.00' else to_char(v.CHEQUE_TOTAL_AMOUNT,'FM999G999G999G990D00') end P407_CHEQUE_TOTAL,
       case when v.PORTFOLIO_ID is null or v.CHEQUE_COVERAGE_PCT is null then '-' else to_char(v.CHEQUE_COVERAGE_PCT,'FM999G990D00')||'%' end P407_COVERAGE_PCT,
       case when v.PORTFOLIO_ID is null then 'Not Available' when v.COVERAGE_OK_FLAG = 'Y' then 'Covered' else 'Insufficient' end P407_COVERAGE_STATUS
  from (select 1 X from dual) d
  left join FNV_V_CHEQUE_COVERAGE v on v.PORTFOLIO_ID = :P407_PORTFOLIO_ID
