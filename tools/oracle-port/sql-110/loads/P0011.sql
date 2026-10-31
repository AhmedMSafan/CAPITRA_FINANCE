-- Page 11, Before Header "Load Staging Metrics": the rows of the batch (all batches when none is chosen).
select count(*) P11_TOTAL, sum(case when validation_status='IMPORTED' then 1 else 0 end) P11_IMPORTED,
       sum(case when validation_status='ERROR' then 1 else 0 end) P11_ERRORS,
       sum(case when nvl(validation_status,'NEW') not in ('IMPORTED','ERROR') then 1 else 0 end) P11_PENDING
  from mff_import_staging where (:P11_BATCH_ID is null or batch_id=:P11_BATCH_ID)
