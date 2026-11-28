-- Page 21, Before Header "Load Application 360" ('Not Found' and defaults when there is no application).
select nvl(x.application_no,'Not Found') P21_APP_NO, x.status_name_ar P21_STATUS, x.applicant_name_ar P21_APPLICANT,
       nvl(x.income,'0') P21_INCOME, nvl(x.property,'0') P21_PROPERTY, nvl(x.finance,'0') P21_FINANCE, x.national_id P21_NID,
       x.financier_name_ar P21_FINANCIER, x.program_code P21_PROGRAM, nvl(x.score,'—') P21_SCORE, nvl(x.decision,'—') P21_DECISION, nvl(x.owner,'—') P21_OWNER
  from (select 1 one from dual) d
  left join (select application_no, st.name_ar status_name_ar, applicant_name_ar, to_char(monthly_income_total,'FM999G999G990D00') income,
                    to_char(property_price,'FM999G999G999G990D00') property, to_char(requested_finance_amount,'FM999G999G999G990D00') finance,
                    national_id, financier_name_ar, program_code, nvl(to_char(last_credit_score),'—') score, nvl(last_decision_code,'—') decision,
                    nvl(current_owner_user,'—') owner
               from mff_vw_application_360 v join ref_mff_application_status st on st.code=v.status_code
              where v.application_id=:P21_APPLICATION_ID) x on 1=1
