-- Page 1 Mortgage Applications, region "Mortgage Applications": the counts it prints.
-- name: CASES
select count(*) from FNV_V_PORTFOLIO_WORKLIST where nvl(PORTFOLIO_STATUS_CODE,'~') not in ('CLOSED','REJECTED')
-- name: FACE
select sum(FACE_VALUE) from FNV_V_PORTFOLIO_WORKLIST where nvl(PORTFOLIO_STATUS_CODE,'~') not in ('CLOSED','REJECTED')
-- name: READY
select count(*) from FNV_V_PORTFOLIO_WORKLIST where READY_FOR_SUBMISSION_FLAG='Y' and nvl(PORTFOLIO_STATUS_CODE,'~') not in ('PENDING_APPROVAL','APPROVED','REJECTED','CLOSED')
-- name: APPROVAL
select count(*) from FNV_V_PORTFOLIO_WORKLIST where PORTFOLIO_STATUS_CODE='PENDING_APPROVAL'
