-- Page 109 Investors, region "Investor Directory".
-- name: HEAD
select count(*) TOTAL, count(case when nvl(ACTIVE_FLAG,'Y') = 'Y' then 1 end) ACTIVE, count(case when nvl(ACTIVE_FLAG,'Y') <> 'Y' then 1 end) INACTIVE,
       count(case when EMAIL_ADDRESS is null then 1 end) NO_EMAIL, count(case when PHONE_NO is null then 1 end) NO_PHONE, count(case when NATIONAL_ID is null then 1 end) NO_NID
  from FNV_INVESTOR
-- name: EXPOSURE
select nvl(sum(OUTSTANDING_VALUE),0) OUTSTANDING, count(*) CONTRACTS from FNV_PURSHASED_PORTFOLIO_ACCOUNT
-- name: ARREARS
select count(*) ARREARS_N, nvl(sum(arr),0) ARREARS_AMT
  from (select INVESTOR_ID, sum(ARREARS_AMOUNT) arr from FNV_PURSHASED_PORTFOLIO_ACCOUNT group by INVESTOR_ID having sum(ARREARS_AMOUNT) > 0) x
-- name: TOP10
select nvl(sum(o),0) TOP10
  from (select sum(OUTSTANDING_VALUE) o from FNV_PURSHASED_PORTFOLIO_ACCOUNT group by INVESTOR_ID order by 1 desc nulls last fetch first 10 rows only) x
-- name: ROWS
select i.INVESTOR_ID, i.INVESTOR_CODE, i.FULL_NAME_EN, i.FULL_NAME_AR, i.NATIONAL_ID, i.PHONE_NO, i.EMAIL_ADDRESS, i.EMPLOYER_NAME,
       nvl(i.ACTIVE_FLAG,'Y') ACTIVE_FLAG, nvl(A.PF,0) PF, nvl(A.CT,0) CT, nvl(A.CV,0) CV, nvl(A.PV,0) PV, nvl(A.OV,0) OV, nvl(A.AR,0) ARR
  from FNV_INVESTOR i
  left join (select INVESTOR_ID, count(distinct PORTFOLIO_ID) pf, count(*) ct, sum(CONTRACT_VALUE) cv, sum(PAID_VALUE) pv,
                    sum(OUTSTANDING_VALUE) ov, sum(ARREARS_AMOUNT) ar
               from FNV_PURSHASED_PORTFOLIO_ACCOUNT group by INVESTOR_ID) A on A.INVESTOR_ID = i.INVESTOR_ID
 order by nvl(A.OV,0) desc, i.INVESTOR_CODE
-- name: TOP5
select i.INVESTOR_ID, nvl(i.FULL_NAME_EN,i.FULL_NAME_AR) NM, sum(a.OUTSTANDING_VALUE) OV
  from FNV_PURSHASED_PORTFOLIO_ACCOUNT a join FNV_INVESTOR i on i.INVESTOR_ID = a.INVESTOR_ID
 group by i.INVESTOR_ID, nvl(i.FULL_NAME_EN,i.FULL_NAME_AR)
having sum(a.OUTSTANDING_VALUE) > 0
 order by 3 desc fetch first 5 rows only
