-- Page 29, Before Header "Load Workflow Context".
select x.status_code P29_CURRENT_STATUS, x.owner P29_CURRENT_OWNER
  from (select 1 one from dual) d
  left join (select status_code, nvl(current_owner_user,'—') owner from mff_applications where application_id=:P29_APPLICATION_ID) x on 1=1
