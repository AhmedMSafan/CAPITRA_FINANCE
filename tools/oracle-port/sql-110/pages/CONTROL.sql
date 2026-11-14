-- MFF_CONTROL_PKG: the readiness row of an application (MFF_VW_APPLICATION_READINESS).
-- name: READINESS
select readiness_pct, status_code, open_tasks, overdue_tasks, applicant_ready, income_ready, property_ready, document_count, document_pending_count,
       document_rejected_count, missing_required_document_count, credit_inquiry_ready, credit_study_ready, credit_decision_ready,
       calculation_ready, subsidy_ready, offer_ready, contract_ready, loan_ready
  from mff_vw_application_readiness
 where application_id = :APPLICATION_ID
