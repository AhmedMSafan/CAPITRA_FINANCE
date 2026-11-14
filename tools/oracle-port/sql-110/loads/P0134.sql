-- Page 134, Before Header "Load Legal Loan on Entry".
select l.loan_account_no P134_LOAN_ACCOUNT_NO, l.application_id P134_APPLICATION_ID, p.full_name_ar P134_APPLICANT
  from mff_loans l join mff_applications a on a.application_id=l.application_id join mff_persons p on p.person_id=a.primary_person_id
 where l.loan_id=:P134_LOAN_ID
