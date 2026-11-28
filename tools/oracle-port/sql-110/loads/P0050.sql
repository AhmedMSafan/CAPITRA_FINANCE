-- Page 50, Before Header "Load Work Metrics": the signed-in user's open work.
select count(*) P50_TOTAL, count(distinct application_id) P50_MINE, sum(case when due_date<trunc(sysdate) then 1 else 0 end) P50_OVERDUE,
       sum(case when due_date=trunc(sysdate) then 1 else 0 end) P50_TODAY
  from mff_vw_work_queue where upper(assigned_to)=upper(:APP_USER)
