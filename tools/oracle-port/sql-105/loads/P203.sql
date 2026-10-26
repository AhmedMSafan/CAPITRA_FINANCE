-- Page 203, Before Header "Load Portfolio Workspace".
select PORTFOLIO_NO P203_PORTFOLIO_NO, nvl(DEVELOPER_NAME_EN,'-') P203_DEVELOPER_NAME, nvl(PROJECT_NAME_EN,'-') P203_PROJECT_NAME,
       STATUS_NAME_EN P203_STATUS_NAME, FACE_VALUE P203_FACE_VALUE, OUTSTANDING_VALUE P203_OUTSTANDING_VALUE,
       PROPOSED_PURCHASE_VALUE P203_PURCHASE_VALUE, ACCOUNT_COUNT P203_ACCOUNT_COUNT, AGE_DAYS P203_AGE_DAYS,
       case when DOCUMENT_READY_FLAG='Y' then 'Ready' else 'Not Ready' end P203_DOC_READY,
       case when DD_READY_FLAG='Y' then 'Ready' else 'Not Ready' end P203_DD_READY,
       case when PAYMENT_HISTORY_READY_FLAG='Y' then 'Ready' else 'Not Ready' end P203_PAY_READY,
       case when VALUATION_READY_FLAG='Y' then 'Ready' else 'Not Ready' end P203_VAL_READY,
       case when LEGAL_READY_FLAG='Y' then 'Ready' else 'Not Ready' end P203_LEGAL_READY,
       case when FINANCE_READY_FLAG='Y' then 'Ready' else 'Not Ready' end P203_FIN_READY,
       case when CHEQUE_READY_FLAG='Y' then 'Ready' else 'Not Ready' end P203_CHQ_READY,
       case when GUARANTEE_READY_FLAG='Y' then 'Ready' else 'Not Ready' end P203_GUA_READY,
       case when READY_FOR_SUBMISSION_FLAG='Y' then 'READY' else 'NOT READY' end P203_OVERALL_READY,
       (select count(*) from FNV_V_PORTFOLIO_EXCEPTIONS e where e.PORTFOLIO_ID = :P203_PORTFOLIO_ID) P203_EXCEPTION_COUNT
  from FNV_V_PORTFOLIO_WORKLIST where PORTFOLIO_ID = :P203_PORTFOLIO_ID
