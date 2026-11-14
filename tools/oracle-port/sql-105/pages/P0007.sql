-- Page 7 Developer Workspace, region "Developer Workspace".
-- name: PIPE
select DEVELOPER_ID,
       count(case when nvl(PORTFOLIO_STATUS_CODE,'~') not in ('CLOSED','REJECTED') then 1 end) OPEN_CNT,
       sum(case when nvl(PORTFOLIO_STATUS_CODE,'~') not in ('CLOSED','REJECTED') then FACE_VALUE end) FACE,
       count(case when READY_FOR_SUBMISSION_FLAG = 'Y' and nvl(PORTFOLIO_STATUS_CODE,'~') not in ('PENDING_APPROVAL','APPROVED','REJECTED','CLOSED') then 1 end) READY,
       count(case when PORTFOLIO_STATUS_CODE = 'PENDING_APPROVAL' then 1 end) PEND,
       count(case when PORTFOLIO_STATUS_CODE = 'APPROVED' then 1 end) APPR,
       max(REQUEST_DATE) LAST_REQUEST
  from FNV_V_PORTFOLIO_WORKLIST group by DEVELOPER_ID
-- name: COUNTS
select (select count(*) from FNV_REALESTATE_DEVELOPER) DEVS,
       (select count(*) from FNV_PROJECT) PROJECTS,
       (select count(*) from FNV_PURSHASED_PORTFOLIO) PF
  from dual
-- name: DEVELOPERS
select d.DEVELOPER_ID, d.DEVELOPER_CODE, d.LEGAL_NAME_EN, d.LEGAL_NAME_AR,
       (select count(*) from FNV_PROJECT p where p.DEVELOPER_ID = d.DEVELOPER_ID) PROJECT_COUNT,
       (select count(*) from FNV_PURSHASED_PORTFOLIO pf where pf.DEVELOPER_ID = d.DEVELOPER_ID) PORTFOLIO_COUNT
  from FNV_REALESTATE_DEVELOPER d
 order by nvl(d.LEGAL_NAME_EN,d.LEGAL_NAME_AR), d.DEVELOPER_CODE
