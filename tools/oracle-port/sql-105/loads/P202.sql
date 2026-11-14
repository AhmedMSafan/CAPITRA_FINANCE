-- Page 202, Before Header "Load Portfolio Details Explicitly": the portfolio, or the defaults of a new one
-- (Request Date's default sysdate, Currency's default EGP).
select P.PORTFOLIO_NO P202_PORTFOLIO_NO, P.DEVELOPER_ID P202_DEVELOPER_ID, P.PROJECT_ID P202_PROJECT_ID,
       to_char(nvl(P.REQUEST_DATE, case when :P202_PORTFOLIO_ID is null then sysdate end),'YYYY-MM-DD') P202_REQUEST_DATE,
       P.TERM_MONTHS P202_TERM_MONTHS,
       case when :P202_PORTFOLIO_ID is null then 'EGP' else P.CURRENCY_CODE end P202_CURRENCY_CODE,
       P.COMMENTS P202_COMMENTS,
       case when :P202_PORTFOLIO_ID is null then 20 else P.COVERAGE_PCT end P202_COVERAGE_PCT,
       case when :P202_PORTFOLIO_ID is null then 5 else P.RESERVED_AMOUNT_PERCENT end P202_RESERVED_AMOUNT_PERCENT,
       case when :P202_PORTFOLIO_ID is null then 13 else P.PV_RATE end P202_PV_RATE,
       case when :P202_PORTFOLIO_ID is null then 15 else P.LATE_FEE_RATE end P202_LATE_FEE_RATE,
       case when :P202_PORTFOLIO_ID is null then 3 else P.MARGIN_PERCENTAGE end P202_MARGIN_PERCENTAGE,
       case when :P202_PORTFOLIO_ID is null then 1 else P.ADMIN_EXPENSE_PERCENT end P202_ADMIN_EXPENSE_PERCENT,
       case when :P202_PORTFOLIO_ID is null then 300 else P.LEGAL_FEE_PER_UNIT end P202_LEGAL_FEE_PER_UNIT,
       case when :P202_PORTFOLIO_ID is null then 500 else P.VALUATION_FEE_PER_UNIT end P202_VALUATION_FEE_PER_UNIT
  from (select 1 X from dual) d
  left join FNV_PURSHASED_PORTFOLIO P on P.PORTFOLIO_ID = :P202_PORTFOLIO_ID
