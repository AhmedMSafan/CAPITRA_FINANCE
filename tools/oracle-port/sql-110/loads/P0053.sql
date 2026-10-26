-- Page 53, Before Header "Load Readiness": the readiness row (the message is set by the C# process MFF_CONTROL_PKG.readiness_message).
select nvl(x.readiness_pct,0) P53_READINESS_PCT, x.status_code P53_STATUS, nvl(x.open_tasks,0) P53_OPEN_TASKS, nvl(x.overdue_tasks,0) P53_OVERDUE_TASKS
  from (select 1 one from dual) d
  left join (select readiness_pct, status_code, open_tasks, overdue_tasks from mff_vw_application_readiness where application_id=:P53_APPLICATION_ID) x on 1=1
