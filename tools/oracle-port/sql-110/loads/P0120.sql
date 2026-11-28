-- Page 120, Before Header "Load Collection Metrics".
select count(*) P120_TOTAL, sum(case when days_past_due>0 then 1 else 0 end) P120_OVER,
       nvl(to_char(sum(outstanding_due),'FM999G999G999G990D00'),'0') P120_AMT, sum(case when days_past_due>=30 then 1 else 0 end) P120_30
  from mff_vw_installments_due
