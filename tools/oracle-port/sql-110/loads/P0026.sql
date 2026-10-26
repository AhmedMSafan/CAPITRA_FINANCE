-- Page 26, Before Header "Load Credit Inquiry Context": the request date is today unless it is set.
select nvl(x.application_no,'Not Found') P26_APP_NO, x.status_code P26_APP_STATUS, x.full_name_ar P26_APPLICANT, nvl(to_date(:P26_REQUEST_DATE,'YYYY-MM-DD'), trunc(sysdate)) P26_REQUEST_DATE
  from (select 1 one from dual) d
  left join (select application_no, status_code, p.full_name_ar from mff_applications a join mff_persons p on p.person_id=a.primary_person_id
              where a.application_id=:P26_APPLICATION_ID) x on 1=1
