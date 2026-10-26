-- Page 20, Before Header "Load Application Metrics".
select count(*) P20_TOTAL,
       sum(case when status_code in ('DOCUMENTS_PENDING','CREDIT_INQUIRY','CREDIT_ACCEPTED','UNDER_DOC_REVIEW','DOC_REVIEWED','REVIEW_APPROVED','CREDIT_STUDY') then 1 else 0 end) P20_REVIEW,
       sum(case when status_code in ('CREDIT_APPROVED','CONDITIONAL_APPROVAL','OFFERED','SUBSIDY_PENDING','CONTRACT_PENDING') then 1 else 0 end) P20_APPROVED,
       sum(case when status_code in ('DISBURSED','ACTIVE') then 1 else 0 end) P20_LIVE
  from mff_applications
