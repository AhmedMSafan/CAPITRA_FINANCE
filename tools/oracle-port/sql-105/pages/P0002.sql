-- Page 2 Purchased Portfolio (home of the line of business), region "Purchased Portfolio".
-- name: CASES
select count(*) CASES, nvl(sum(FACE_VALUE),0) FACE,
       nvl(sum(case when READY_FOR_SUBMISSION_FLAG = 'Y' and PORTFOLIO_STATUS_CODE not in ('PENDING_APPROVAL','APPROVED','REJECTED','CLOSED') then 1 else 0 end),0) READY
  from FNV_V_PORTFOLIO_WORKLIST
 where PORTFOLIO_STATUS_CODE not in ('CLOSED','REJECTED')
-- name: EXPIRING
select count(*) EXPIRING, count(distinct PORTFOLIO_ID) EXP_CASES from FNV_V_MIS_DOCUMENT_EXPIRY where DAYS_TO_EXPIRY between 0 and 30
-- name: PIPE_TOTAL
select nvl(sum(a.CASE_COUNT),0) from FNV_V_MIS_PIPELINE_AGING a
-- name: PIPE
select s.STATUS_NAME_EN NAME, a.CASE_COUNT CNT
  from FNV_V_MIS_PIPELINE_AGING a
  join REF_PORTFOLIO_STATUS s on s.STATUS_CODE = a.PORTFOLIO_STATUS_CODE
 where a.CASE_COUNT > 0
 order by s.DISPLAY_SEQ
-- name: LATEST
select PORTFOLIO_ID, PORTFOLIO_NO, DEVELOPER_NAME_EN, PROJECT_NAME_EN, STATUS_NAME_EN, FACE_VALUE
  from FNV_V_PORTFOLIO_WORKLIST
 order by REQUEST_DATE desc nulls last, PORTFOLIO_NO desc
 fetch first 6 rows only
