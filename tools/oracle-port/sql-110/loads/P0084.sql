-- Page 84, Before Header "Load Disbursement": today's disbursement date and a first due date one month ahead (unless set),
-- the signed contract, the issued offer's terms, the maturity date and the loan when it exists.
select nvl(x.application_no,'Not Found') P84_APP_NO, x.status_code P84_APP_STATUS, x.full_name_ar P84_APPLICANT,
       nvl(to_date(:P84_DISBURSEMENT_DATE,'YYYY-MM-DD'), trunc(sysdate)) P84_DISBURSEMENT_DATE,
       nvl(to_date(:P84_FIRST_DUE_DATE,'YYYY-MM-DD'), add_months(trunc(sysdate),1)) P84_FIRST_DUE_DATE,
       c.contract_id P84_CONTRACT_ID, c.finance_amount P84_PRINCIPAL_AMOUNT,
       o.annual_rate_pct P84_ANNUAL_RATE_PCT, o.term_months P84_TERM_MONTHS, o.initial_installment P84_INITIAL_INSTALLMENT,
       nvl(to_date(:P84_MATURITY_DATE,'YYYY-MM-DD'), add_months(nvl(to_date(:P84_DISBURSEMENT_DATE,'YYYY-MM-DD'), trunc(sysdate)), o.term_months)) P84_MATURITY_DATE,
       l.loan_id P84_LOAN_ID, l.loan_account_no P84_LOAN_ACCOUNT_NO
  from (select 1 one from dual) d
  left join (select application_no, status_code, p.full_name_ar from mff_applications a join mff_persons p on p.person_id=a.primary_person_id
              where a.application_id=:P84_APPLICATION_ID) x on 1=1
  left join (select contract_id, finance_amount from mff_contracts where application_id=:P84_APPLICATION_ID and (signed_flag='Y' or contract_status='SIGNED')
              order by contract_id desc fetch first 1 rows only) c on 1=1
  left join (select annual_rate_pct, term_months, initial_installment from mff_finance_offers
              where application_id=:P84_APPLICATION_ID and offer_status in ('ISSUED','ACCEPTED') order by offer_id desc fetch first 1 rows only) o on 1=1
  left join (select loan_id, loan_account_no from mff_loans where application_id=:P84_APPLICATION_ID order by loan_id fetch first 1 rows only) l on 1=1
