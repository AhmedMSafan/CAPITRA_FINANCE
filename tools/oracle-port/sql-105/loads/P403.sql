-- Page 403, Before Header "Load Legal Review": the latest review of the portfolio, or 'Not Reviewed'.
select L.LEGAL_REVIEW_ID P403_LEGAL_REVIEW_ID,
       case when L.LEGAL_REVIEW_ID is null then 'Not Reviewed' when L.OWNERSHIP_VERIFIED_FLAG = 'Y' then 'Yes' else 'No' end P403_OWNERSHIP,
       case when L.LEGAL_REVIEW_ID is null then 'Not Reviewed' when L.BUILDING_LICENSE_VERIFIED_FLAG = 'Y' then 'Yes' else 'No' end P403_BUILDING_LICENSE,
       case when L.LEGAL_REVIEW_ID is null then 'Not Reviewed' when L.REGISTRATION_POSSIBLE_FLAG = 'Y' then 'Yes' else 'No' end P403_REGISTRATION,
       case when L.LEGAL_REVIEW_ID is null then 'Not Reviewed' when L.MORTGAGE_POSSIBLE_FLAG = 'Y' then 'Yes' else 'No' end P403_MORTGAGE,
       case when L.LEGAL_REVIEW_ID is null then 'Not Reviewed' when L.ENCUMBRANCE_FREE_FLAG = 'Y' then 'Yes' else 'No' end P403_ENCUMBRANCE,
       case when L.LEGAL_REVIEW_ID is null then 'Not Reviewed' when L.TITLE_DEED_VERIFIED_FLAG = 'Y' then 'Yes' else 'No' end P403_TITLE_DEED,
       dbms_lob.substr(L.LEGAL_FINDINGS,3500,1) P403_FINDINGS,
       dbms_lob.substr(L.LEGAL_RECOMMENDATION,3500,1) P403_RECOMMENDATION,
       L.REVIEWED_BY P403_REVIEWED_BY,
       to_char(L.REVIEW_DATE,'DD-MON-YYYY') P403_REVIEW_DATE
  from (select 1 X from dual) d
  left join FNV_LEGAL_REVIEW L on L.LEGAL_REVIEW_ID = (select max(LEGAL_REVIEW_ID) from FNV_LEGAL_REVIEW where PORTFOLIO_ID = :P403_PORTFOLIO_ID)
