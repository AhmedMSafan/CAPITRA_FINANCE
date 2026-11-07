-- Page 131, Before Header "Load Delinquency Case on Entry": the loan and its latest delinquency case (else OPEN, zero overdue).
select l.loan_account_no P131_LOAN_ACCOUNT_NO, l.application_no P131_APPLICATION_NO, l.full_name_ar P131_APPLICANT,
       case when q.delinquency_id is null then l.delinquent_days else q.days_past_due end P131_DPD,
       q.delinquency_id P131_DELINQUENCY_ID, q.stage_code P131_STAGE_CODE,
       case when l.loan_id is not null then nvl(q.status,'OPEN') end P131_STATUS,
       case when l.loan_id is not null then nvl(q.overdue_amount,0) end P131_OVERDUE_AMOUNT,
       case when l.loan_id is not null then nvl(q.overdue_installments,0) end P131_OVERDUE_INSTALLMENTS,
       q.collection_action_ar P131_COLLECTION_ACTION
  from (select 1 one from dual) d
  left join (select l.loan_id, l.loan_account_no, a.application_no, p.full_name_ar, l.delinquent_days from mff_loans l
               join mff_applications a on a.application_id=l.application_id join mff_persons p on p.person_id=a.primary_person_id
              where l.loan_id=:P131_LOAN_ID) l on 1=1
  left join (select * from mff_delinquencies where loan_id=:P131_LOAN_ID order by delinquency_id desc fetch first 1 rows only) q on l.loan_id is not null
