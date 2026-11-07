-- Page 82, Before Header "Load Finance Offer": the latest offer; with none, a DRAFT dated today with the figures of the
-- application, the approved credit decision and the latest calculation run.
select nvl(x.application_no,'Not Found') P82_APP_NO, x.status_code P82_APP_STATUS, x.full_name_ar P82_APPLICANT,
       o.offer_id P82_OFFER_ID, o.offer_no P82_OFFER_NO, case when o.offer_id is null then trunc(sysdate) else o.offer_date end P82_OFFER_DATE,
       o.expiry_date P82_EXPIRY_DATE,
       case when o.offer_id is null then a.fin else o.finance_amount end P82_FINANCE_AMOUNT,
       case when o.offer_id is null then c.approved_term_months else o.term_months end P82_TERM_MONTHS,
       case when o.offer_id is null then c.approved_rate_pct else o.annual_rate_pct end P82_ANNUAL_RATE_PCT,
       case when o.offer_id is null then r.out_initial_payment else o.initial_installment end P82_INITIAL_INSTALLMENT,
       case when o.offer_id is null then a.downpayment_amount else o.downpayment_amount end P82_DOWNPAYMENT_AMOUNT,
       case when o.offer_id is null then a.total_subsidy_amount else o.subsidy_amount end P82_SUBSIDY_AMOUNT,
       case when o.offer_id is null then a.ltv else o.ltv_pct end P82_LTV_PCT,
       case when o.offer_id is null then 'DRAFT' else o.offer_status end P82_OFFER_STATUS,
       o.accepted_date P82_ACCEPTED_DATE, o.notes P82_NOTES
  from (select 1 one from dual) d
  left join (select application_no, status_code, p.full_name_ar from mff_applications a join mff_persons p on p.person_id=a.primary_person_id
              where a.application_id=:P82_APPLICATION_ID) x on 1=1
  left join (select * from mff_finance_offers where application_id=:P82_APPLICATION_ID order by offer_id desc fetch first 1 rows only) o on 1=1
  left join (select nvl(approved_finance_amount,requested_finance_amount) fin, downpayment_amount, total_subsidy_amount,
                    case when property_price>0 then nvl(approved_finance_amount,requested_finance_amount)/property_price*100 end ltv
               from mff_applications where application_id=:P82_APPLICATION_ID) a on 1=1
  left join (select approved_term_months, approved_rate_pct from mff_credit_decisions
              where application_id=:P82_APPLICATION_ID and decision_code in ('APPROVE','CONDITIONAL') order by decision_id desc fetch first 1 rows only) c on 1=1
  left join (select out_initial_payment from mff_calculation_runs where application_id=:P82_APPLICATION_ID order by calc_run_id desc fetch first 1 rows only) r on 1=1
