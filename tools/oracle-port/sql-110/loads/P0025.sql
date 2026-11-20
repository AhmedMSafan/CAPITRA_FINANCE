-- Page 25, Before Header "Load Document Context".
select nvl(x.application_no,'Not Found') P25_APP_NO, x.status_code P25_APP_STATUS, x.full_name_ar P25_APPLICANT
  from (select 1 one from dual) d
  left join (select application_no, status_code, p.full_name_ar from mff_applications a join mff_persons p on p.person_id=a.primary_person_id
              where a.application_id=:P25_APPLICATION_ID) x on 1=1
