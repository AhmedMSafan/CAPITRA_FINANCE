-- Page 135, Before Header "Load Loan Event Loan on Entry".
select l.loan_account_no P135_LOAN_ACCOUNT_NO, p.full_name_ar P135_APPLICANT
  from mff_loans l join mff_applications a on a.application_id=l.application_id join mff_persons p on p.person_id=a.primary_person_id
 where l.loan_id=:P135_LOAN_ID
