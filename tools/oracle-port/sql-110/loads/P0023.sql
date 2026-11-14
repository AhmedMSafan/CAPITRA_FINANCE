-- Page 23, Before Header "Load Income Context": the application, its applicant and the applicant's party row.
select nvl(x.application_no,'Not Found') P23_APP_NO, x.status_code P23_APP_STATUS, x.full_name_ar P23_APPLICANT,
       (select min(application_party_id) from mff_application_parties where application_id=:P23_APPLICATION_ID and party_role_code='APPLICANT') P23_APPLICATION_PARTY_ID
  from (select 1 one from dual) d
  left join (select application_no, status_code, p.full_name_ar from mff_applications a join mff_persons p on p.person_id=a.primary_person_id
              where a.application_id=:P23_APPLICATION_ID) x on 1=1
