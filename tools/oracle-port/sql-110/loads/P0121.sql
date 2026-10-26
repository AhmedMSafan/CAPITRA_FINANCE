-- Page 121, Before Header "Load Loan on Entry".
select l.loan_account_no P121_LOAN_ACCOUNT_NO, l.outstanding_principal P121_OUTSTANDING, l.next_due_date P121_NEXT_DUE
  from (select 1 one from dual) d left join mff_loans l on l.loan_id=:P121_LOAN_ID
