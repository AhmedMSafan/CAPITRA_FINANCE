with f as (
  select p.PORTFOLIO_ID, p.PORTFOLIO_NO,
         case when dc.DOCUMENT_READY_FLAG='Y' then 'Y' else 'N' end DOCUMENT_READY_FLAG,
         case when exists (select 1 from FNV_DUE_DILIGENCE dd where dd.PORTFOLIO_ID=p.PORTFOLIO_ID and dd.EXCEPTION_FLAG='Y') then 'N' else 'Y' end DD_READY_FLAG,
         case when exists (select 1 from FNV_VALUATION v where v.PORTFOLIO_ID=p.PORTFOLIO_ID) then 'Y' else 'N' end VALUATION_READY_FLAG,
         case when ls.LEGAL_READY_FLAG='Y' then 'Y' else 'N' end LEGAL_READY_FLAG,
         case when exists (select 1 from FNV_FINANCIAL_STUDY fs where fs.PORTFOLIO_ID=p.PORTFOLIO_ID) then 'Y' else 'N' end FINANCE_READY_FLAG,
         case when cc.COVERAGE_OK_FLAG='Y' then 'Y' else 'N' end CHEQUE_READY_FLAG,
         case when gs.GUARANTEE_READY_FLAG='Y' then 'Y' else 'N' end GUARANTEE_READY_FLAG,
         case when exists (select 1 from FNV_V_PAYMENT_REGULARITY pr where pr.PORTFOLIO_ID=p.PORTFOLIO_ID and (pr.PAYMENT_COUNT < 12 or pr.PROBLEM_COUNT > 0)) then 'N' else 'Y' end PAYMENT_HISTORY_READY_FLAG
    from FNV_PURSHASED_PORTFOLIO p
    left join FNV_V_DOCUMENT_COMPLETENESS dc on dc.PORTFOLIO_ID=p.PORTFOLIO_ID
    left join FNV_V_LEGAL_STATUS ls on ls.PORTFOLIO_ID=p.PORTFOLIO_ID
    left join FNV_V_CHEQUE_COVERAGE cc on cc.PORTFOLIO_ID=p.PORTFOLIO_ID
    left join FNV_V_GUARANTEE_STATUS gs on gs.PORTFOLIO_ID=p.PORTFOLIO_ID
)
select f.*,
       case when DOCUMENT_READY_FLAG='Y' and DD_READY_FLAG='Y' and VALUATION_READY_FLAG='Y' and LEGAL_READY_FLAG='Y'
             and FINANCE_READY_FLAG='Y' and CHEQUE_READY_FLAG='Y' and GUARANTEE_READY_FLAG='Y' and PAYMENT_HISTORY_READY_FLAG='Y'
            then 'Y' else 'N' end READY_FOR_SUBMISSION_FLAG
from f
