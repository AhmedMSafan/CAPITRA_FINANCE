-- Page 10, Before Header "Load Batch Metrics".
select count(*) P10_TOTAL, sum(case when batch_status='COMPLETED' then 1 else 0 end) P10_DONE,
       sum(case when batch_status='PROCESSING' then 1 else 0 end) P10_PROC, sum(case when nvl(error_count,0)>0 then 1 else 0 end) P10_ERR
  from mff_import_batches
