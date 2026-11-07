-- Page 603 Submission & Readiness, Before Header "Load Readiness".
-- name: LOAD
select w.PORTFOLIO_NO P603_PORTFOLIO_NO, w.STATUS_NAME_EN P603_STATUS,
       case w.DOCUMENT_READY_FLAG when 'Y' then 'Ready' else 'Not Ready' end P603_DOCUMENT_READY,
       case w.DD_READY_FLAG when 'Y' then 'Ready' else 'Not Ready' end P603_DD_READY,
       case w.PAYMENT_HISTORY_READY_FLAG when 'Y' then 'Ready' else 'Not Ready' end P603_PAYMENT_READY,
       case w.VALUATION_READY_FLAG when 'Y' then 'Ready' else 'Not Ready' end P603_VALUATION_READY,
       case w.LEGAL_READY_FLAG when 'Y' then 'Ready' else 'Not Ready' end P603_LEGAL_READY,
       case w.FINANCE_READY_FLAG when 'Y' then 'Ready' else 'Not Ready' end P603_FINANCE_READY,
       case w.CHEQUE_READY_FLAG when 'Y' then 'Ready' else 'Not Ready' end P603_CHEQUE_READY,
       case w.GUARANTEE_READY_FLAG when 'Y' then 'Ready' else 'Not Ready' end P603_GUARANTEE_READY,
       case w.READY_FOR_SUBMISSION_FLAG when 'Y' then 'Ready' else 'Not Ready' end P603_OVERALL_READY
  from FNV_V_PORTFOLIO_WORKLIST w where w.PORTFOLIO_ID = :P603_PORTFOLIO_ID
