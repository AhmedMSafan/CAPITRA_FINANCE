-- Page 140, Before Header "Load MIS Metrics": open loans, outstanding, subsidy, delinquent loans and their rate.
select l.loans P140_LOANS, l.outstanding P140_OUTSTANDING,
       (select to_char(nvl(sum(total_subsidy_amount),0),'FM999G999G999G990D00') from mff_applications) P140_SUBSIDY,
       q.dq P140_DQ, (select count(*) from mff_vw_fund_monthly_report) P140_FUND_EVENTS,
       case when l.loans=0 then '0.00' else to_char(100*q.dq/l.loans,'FM990D00') end P140_DQ_RATE
  from (select count(*) loans, to_char(nvl(sum(outstanding_principal),0),'FM999G999G999G990D00') outstanding from mff_loans where loan_status<>'CLOSED') l,
       (select count(*) dq from mff_vw_delinquency) q
