-- Page 81, Before Header "Load Subsidy": the application and its latest subsidy approval (else not dedicated, PENDING).
select nvl(x.application_no,'Not Found') P81_APP_NO, x.status_code P81_APP_STATUS, x.full_name_ar P81_APPLICANT,
       y.subsidy_approval_id P81_SUBSIDY_APPROVAL_ID, y.support_type_code P81_SUPPORT_TYPE_CODE, y.approval_no P81_APPROVAL_NO,
       y.approval_date P81_APPROVAL_DATE, y.expiry_date P81_EXPIRY_DATE, y.installment_support_npv P81_INSTALLMENT_SUPPORT_NPV,
       y.downpayment_support P81_DOWNPAYMENT_SUPPORT, y.total_support P81_TOTAL_SUPPORT, nvl(y.dedicated_account_flag,'N') P81_DEDICATED_ACCOUNT_FLAG,
       nvl(y.approval_status,'PENDING') P81_APPROVAL_STATUS, y.fund_notes P81_FUND_NOTES
  from (select 1 one from dual) d
  left join (select application_no, status_code, p.full_name_ar from mff_applications a join mff_persons p on p.person_id=a.primary_person_id
              where a.application_id=:P81_APPLICATION_ID) x on 1=1
  left join (select * from mff_subsidy_approvals where application_id=:P81_APPLICATION_ID order by subsidy_approval_id desc fetch first 1 rows only) y on 1=1
