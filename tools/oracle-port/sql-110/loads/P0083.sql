-- Page 83, Before Header "Load Contract": the buyer, financier, figures and selected property of the application, and the
-- latest contract (else a DRAFT dated today, not signed).
select nvl(x.application_no,'Not Found') P83_APP_NO, x.status_code P83_APP_STATUS, x.full_name_ar P83_APPLICANT,
       a.primary_person_id P83_BUYER_PERSON_ID, a.financier_id P83_FINANCIER_ID,
       (select min(property_id) from mff_application_property where application_id=:P83_APPLICATION_ID and selected_flag='Y') P83_PROPERTY_ID,
       c.contract_id P83_CONTRACT_ID, c.contract_no P83_CONTRACT_NO, case when c.contract_id is null then trunc(sysdate) else c.contract_date end P83_CONTRACT_DATE,
       c.seller_person_id P83_SELLER_PERSON_ID,
       case when c.contract_id is null then a.fin else c.finance_amount end P83_FINANCE_AMOUNT,
       case when c.contract_id is null then a.property_price else c.property_price end P83_PROPERTY_PRICE,
       case when c.contract_id is null then 'N' else c.signed_flag end P83_SIGNED_FLAG,
       case when c.contract_id is null then 'DRAFT' else c.contract_status end P83_CONTRACT_STATUS
  from (select 1 one from dual) d
  left join (select application_no, status_code, p.full_name_ar from mff_applications a join mff_persons p on p.person_id=a.primary_person_id
              where a.application_id=:P83_APPLICATION_ID) x on 1=1
  left join (select primary_person_id, financier_id, nvl(approved_finance_amount,requested_finance_amount) fin, property_price
               from mff_applications where application_id=:P83_APPLICATION_ID) a on 1=1
  left join (select * from mff_contracts where application_id=:P83_APPLICATION_ID order by contract_id desc fetch first 1 rows only) c on 1=1
