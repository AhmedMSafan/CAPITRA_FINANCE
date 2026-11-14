-- Page 28, Before Header "Load Credit Decision Context": the decision date (today unless set) and the approved (else requested) finance.
select nvl(x.application_no,'Not Found') P28_APP_NO, x.status_code P28_APP_STATUS, x.full_name_ar P28_APPLICANT,
       nvl(to_date(:P28_DECISION_DATE,'YYYY-MM-DD'), trunc(sysdate)) P28_DECISION_DATE,
       (select nvl(approved_finance_amount,requested_finance_amount) from mff_applications where application_id=:P28_APPLICATION_ID) P28_APPROVED_FINANCE_AMOUNT
  from (select 1 one from dual) d
  left join (select application_no, status_code, p.full_name_ar from mff_applications a join mff_persons p on p.person_id=a.primary_person_id
              where a.application_id=:P28_APPLICATION_ID) x on 1=1
