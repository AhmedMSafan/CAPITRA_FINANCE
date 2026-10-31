-- Page 132, Before Header "Load Early Settlement Loan on Entry".
select l.loan_account_no P132_LOAN_ACCOUNT_NO, nvl(l.outstanding_principal,l.principal_amount) P132_OUTSTANDING, l.principal_amount P132_PRINCIPAL_AMOUNT,
       l.disbursement_date P132_DISBURSEMENT_DATE, nvl(a.total_subsidy_amount,0) P132_TOTAL_SUBSIDY
  from mff_loans l join mff_applications a on a.application_id=l.application_id where l.loan_id=:P132_LOAN_ID
