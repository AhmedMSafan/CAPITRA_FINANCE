-- Page 1, Before Header "Load Dashboard Metrics": the application counts by stage and today's date.
select count(*) P1_TOTAL,
       sum(case when status_code='IMPORTED' then 1 else 0 end) P1_IMPORTED,
       sum(case when status_code in ('DOCUMENTS_PENDING','CREDIT_INQUIRY','CREDIT_ACCEPTED','UNDER_DOC_REVIEW','DOC_REVIEWED','REVIEW_APPROVED','CREDIT_STUDY') then 1 else 0 end) P1_REVIEW,
       sum(case when status_code in ('CREDIT_APPROVED','CONDITIONAL_APPROVAL','OFFERED','SUBSIDY_PENDING','CONTRACT_PENDING') then 1 else 0 end) P1_APPROVED,
       sum(case when status_code in ('ACTIVE','DISBURSED') then 1 else 0 end) P1_ACTIVE,
       sum(case when status_code in ('DELINQUENT','LEGAL_ACTION') then 1 else 0 end) P1_DELINQ,
       sum(case when status_code in ('IMPORTED','DOCUMENTS_PENDING') then 1 else 0 end) P1_PIPE_INTAKE,
       sum(case when status_code in ('CREDIT_INQUIRY','CREDIT_ACCEPTED') then 1 else 0 end) P1_PIPE_CREDIT,
       sum(case when status_code in ('UNDER_DOC_REVIEW','DOC_REVIEWED','REVIEW_APPROVED','CREDIT_STUDY') then 1 else 0 end) P1_PIPE_STUDY,
       sum(case when status_code in ('CREDIT_APPROVED','CONDITIONAL_APPROVAL','OFFERED','SUBSIDY_PENDING','CONTRACT_PENDING') then 1 else 0 end) P1_PIPE_APPROVAL,
       sum(case when status_code in ('DISBURSED','ACTIVE','DELINQUENT','LEGAL_ACTION','EARLY_SETTLEMENT') then 1 else 0 end) P1_PIPE_LIVE,
       to_char(sysdate,'DD Mon YYYY') P1_TODAY
  from mff_applications
