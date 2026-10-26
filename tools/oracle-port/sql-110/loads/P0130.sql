-- Page 130, Before Header "Load Delinquency Metrics".
select count(*) P130_TOTAL, sum(case when days_past_due>=90 then 1 else 0 end) P130_90, sum(case when stage_code='LEGAL' then 1 else 0 end) P130_LEGAL,
       nvl(to_char(sum(overdue_amount),'FM999G999G999G990D00'),'0') P130_AMT
  from mff_vw_delinquency
