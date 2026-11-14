-- Page 22, Before Header "Load Applicant Details": the application and its applicant ('Not Found' when there is none).
select nvl(x.application_no, 'Not Found') P22_APPLICATION_NO, x.fund_file_no P22_FUND_FILE_NO, x.full_name_ar P22_FULL_NAME_AR,
       x.national_id P22_NATIONAL_ID, x.birth_date P22_BIRTH_DATE, x.gender_code P22_GENDER_CODE, x.marital_status_code P22_MARITAL_STATUS_CODE,
       x.mobile_no P22_MOBILE_NO, x.email P22_EMAIL, x.address_ar P22_ADDRESS_AR, x.employment_type_code P22_EMPLOYMENT_TYPE_CODE,
       x.employer_name P22_EMPLOYER_NAME, x.hire_date P22_HIRE_DATE, x.requested_finance_amount P22_REQUESTED_FINANCE_AMOUNT,
       x.property_price P22_PROPERTY_PRICE, x.downpayment_amount P22_DOWNPAYMENT_AMOUNT, x.status_code P22_APP_STATUS,
       x.application_no P22_APP_NO, x.full_name_ar P22_APPLICANT
  from (select 1 one from dual) d
  left join (select a.application_no, a.fund_file_no, p.full_name_ar, p.national_id, p.birth_date, p.gender_code, p.marital_status_code, p.mobile_no,
                    p.email, p.address_ar, p.employment_type_code, p.employer_name, p.hire_date, a.requested_finance_amount, a.property_price,
                    a.downpayment_amount, a.status_code
               from mff_applications a join mff_persons p on p.person_id = a.primary_person_id
              where a.application_id = :P22_APPLICATION_ID) x on 1 = 1
