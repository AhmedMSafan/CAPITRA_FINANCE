-- Page 30, Before Header "Load Inquiry Metrics".
select count(*) P30_TOTAL, sum(case when response_date is null then 1 else 0 end) P30_WAIT,
       sum(case when upper(nvl(result_code,'-')) in ('ACCEPTED','PASS','OK') then 1 else 0 end) P30_ACCEPT,
       sum(case when upper(nvl(result_code,'-')) in ('REJECTED','FAIL','DECLINED') then 1 else 0 end) P30_REJECT
  from mff_vw_credit_inquiry_queue
