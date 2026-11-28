-- Page 405, Before Header "Load Financial Study": the latest study of the portfolio, or 'Not Created' and dashes.
select F.FIN_STUDY_ID P405_FIN_STUDY_ID,
       case when F.FIN_STUDY_ID is null then 'Not Created' else 'Available' end P405_STUDY_STATUS,
       case when F.FIN_STUDY_ID is null then '-' else to_char(F.PORTFOLIO_FACE_VALUE,'FM999G999G999G990D00') end P405_FACE_VALUE,
       case when F.FIN_STUDY_ID is null then '-' else to_char(F.OUTSTANDING_VALUE,'FM999G999G999G990D00') end P405_OUTSTANDING,
       case when F.FIN_STUDY_ID is null then '-' else to_char(F.PROPOSED_PURCHASE_VALUE,'FM999G999G999G990D00') end P405_PURCHASE_VALUE,
       case when F.FIN_STUDY_ID is null then '-' else to_char(F.EXPECTED_RETURN_AMOUNT,'FM999G999G999G990D00') end P405_EXPECTED_RETURN,
       case when F.FIN_STUDY_ID is null then '-' else to_char(F.NET_RETURN_AMOUNT,'FM999G999G999G990D00') end P405_NET_RETURN,
       case when F.FIN_STUDY_ID is null then '-' else to_char(F.CHEQUE_TOTAL_VALUE,'FM999G999G999G990D00') end P405_CHEQUE_TOTAL,
       case when F.FIN_STUDY_ID is null or F.CHEQUE_COVERAGE_PCT is null then '-' else to_char(F.CHEQUE_COVERAGE_PCT,'FM999G990D00')||'%' end P405_COVERAGE_PCT,
       case when F.FIN_STUDY_ID is null then '-' else to_char(F.DEPOSIT_AMOUNT,'FM999G999G999G990D00') end P405_DEPOSIT,
       dbms_lob.substr(F.FINANCIAL_COMMENTS,3500,1) P405_COMMENTS
  from (select 1 X from dual) d
  left join FNV_FINANCIAL_STUDY F on F.FIN_STUDY_ID = (select max(FIN_STUDY_ID) from FNV_FINANCIAL_STUDY where PORTFOLIO_ID = :P405_PORTFOLIO_ID)
