-- Page 27, Before Header "Load Credit Study Context": the study date (today unless set), the verified income, the proposed
-- finance and LTV from the application, and the disposable income.
select nvl(x.application_no,'Not Found') P27_APP_NO, x.status_code P27_APP_STATUS, x.full_name_ar P27_APPLICANT,
       nvl(to_date(:P27_STUDY_DATE,'YYYY-MM-DD'), trunc(sysdate)) P27_STUDY_DATE,
       y.monthly_income_total P27_VERIFIED_INCOME_AMOUNT, y.requested_finance_amount P27_PROPOSED_FINANCE_AMOUNT, y.ltv P27_LTV_PCT,
       (nvl(:P27_MONTHLY_OBLIGATIONS_AMOUNT,0)+0) P27_MONTHLY_OBLIGATIONS_AMOUNT,
       nvl(y.monthly_income_total,0) - (nvl(:P27_MONTHLY_OBLIGATIONS_AMOUNT,0)+0) P27_DISPOSABLE_INCOME_AMOUNT
  from (select 1 one from dual) d
  left join (select application_no, status_code, p.full_name_ar from mff_applications a join mff_persons p on p.person_id=a.primary_person_id
              where a.application_id=:P27_APPLICATION_ID) x on 1=1
  left join (select monthly_income_total, requested_finance_amount, case when property_price>0 then requested_finance_amount/property_price*100 end ltv
               from mff_applications where application_id=:P27_APPLICATION_ID) y on 1=1
