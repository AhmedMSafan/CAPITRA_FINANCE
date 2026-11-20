-- Page 201, Before Header "Load Portfolio Case Summary": the developer (or all) and the case counts.
select case when :P201_DEVELOPER_ID is null then 'All Developers'
            else nvl((select nvl(LEGAL_NAME_EN,LEGAL_NAME_AR) from FNV_REALESTATE_DEVELOPER where DEVELOPER_ID = :P201_DEVELOPER_ID), 'Developer Not Found') end P201_DEVELOPER_NAME,
       case when :P201_DEVELOPER_ID is null then 'ALL'
            else nvl((select DEVELOPER_CODE from FNV_REALESTATE_DEVELOPER where DEVELOPER_ID = :P201_DEVELOPER_ID), '-') end P201_DEVELOPER_CODE,
       case when :P201_DEVELOPER_ID is null then 'All projects'
            else nvl((select listagg(nvl(PROJECT_NAME_EN,PROJECT_NAME_AR)||' ('||PROJECT_CODE||')', ', ') within group(order by PROJECT_CODE)
                        from FNV_PROJECT where DEVELOPER_ID = :P201_DEVELOPER_ID), 'No projects') end P201_PROJECTS,
       (select count(*) from FNV_V_PORTFOLIO_WORKLIST where (:P201_DEVELOPER_ID is null or DEVELOPER_ID = :P201_DEVELOPER_ID)) P201_TOTAL_CASES,
       (select nvl(sum(case when upper(nvl(STATUS_NAME_EN,'-')) = 'DUE DILIGENCE' then 1 else 0 end),0) from FNV_V_PORTFOLIO_WORKLIST where (:P201_DEVELOPER_ID is null or DEVELOPER_ID = :P201_DEVELOPER_ID)) P201_DUE_DILIGENCE,
       (select nvl(sum(case when upper(nvl(STATUS_NAME_EN,'-')) = 'PENDING APPROVAL' then 1 else 0 end),0) from FNV_V_PORTFOLIO_WORKLIST where (:P201_DEVELOPER_ID is null or DEVELOPER_ID = :P201_DEVELOPER_ID)) P201_PENDING_APPROVAL,
       (select nvl(sum(case when upper(nvl(READY_FOR_SUBMISSION_FLAG,'N')) = 'Y' then 1 else 0 end),0) from FNV_V_PORTFOLIO_WORKLIST where (:P201_DEVELOPER_ID is null or DEVELOPER_ID = :P201_DEVELOPER_ID)) P201_READY_COUNT
  from dual
