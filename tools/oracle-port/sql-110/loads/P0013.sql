-- Page 13, Before Header "Load Batch on Entry" ('Not Found' and zeros when there is no batch). Its second process,
-- "Load Workflow Readiness", sets page 21 items only and shows nothing on this page.
select x.file_name P13_FILE_NAME, nvl(x.batch_status,'Not Found') P13_BATCH_STATUS, nvl(x.row_count,0) P13_ROWS,
       nvl(x.valid_count,0) P13_IMPORTED, nvl(x.error_count,0) P13_ERRORS
  from (select 1 one from dual) d
  left join (select file_name, batch_status, row_count, valid_count, error_count from mff_import_batches where batch_id=:P13_BATCH_ID) x on 1=1
