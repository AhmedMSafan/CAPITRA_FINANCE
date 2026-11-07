-- Page 133, Before Header "Load Restructuring Loan on Entry": the loan's current terms and its arrears to capitalise.
select loan_account_no P133_LOAN_ACCOUNT_NO, nvl(outstanding_principal,principal_amount) P133_OUTSTANDING, term_months P133_OLD_TERM_MONTHS,
       annual_rate_pct P133_OLD_RATE_PCT, initial_installment P133_OLD_INSTALLMENT,
       (select nvl(sum(outstanding_due),0) from mff_vw_installments_due where loan_id=:P133_LOAN_ID) P133_CAPITALIZED_ARREARS
  from mff_loans where loan_id=:P133_LOAN_ID
