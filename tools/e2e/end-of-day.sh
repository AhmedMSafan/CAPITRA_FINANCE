#!/usr/bin/env bash
# End-to-end check of the End of Day application against the real API and a real PostgreSQL database. Before the upgrade, develop made
# the database with company OLDCO, two Retail Lending end of day runs done (PREV_END-3, PREV_END-2: the last days of last month) and a
# role "EOD_OPERATOR" holding LND.EOD.RUN.
set -euo pipefail
API=${API:-http://localhost:5080}
pass=0; fail=0
ok()   { echo "  ✅ $1"; pass=$((pass+1)); }
bad()  { echo "  ❌ $1"; fail=$((fail+1)); }
check(){ if [ "$2" = "$3" ]; then ok "$1"; else bad "$1 (got [$2], want [$3])"; fi; }
call() { local m=$1 p=$2 t=$3 d=${4:-}; local out; out=$(mktemp)
  local code; code=$(curl -s -o "$out" -w '%{http_code}' -X "$m" "$API$p" -H 'Content-Type: application/json' ${t:+-H "Authorization: Bearer $t"} ${d:+--data "$d"})
  printf '%s\t%s' "$code" "$(tr '\n' ' ' < "$out")"; rm -f "$out"; }
status(){ cut -f1 <<<"$1"; }
body()  { cut -f2- <<<"$1"; }
login() { call POST /api/sec/auth/login "" "{\"userName\":\"$1\",\"password\":\"$2\"}" | cut -f2- | jq -r .token; }
sql()   { docker exec "$PG" psql -U postgres -d capitra -tAc "$1"; }
day()   { python3 -c "import datetime as d,sys; print(d.date.fromisoformat(sys.argv[1]) + d.timedelta(days=int(sys.argv[2])))" "$1" "$2"; }
TODAY=$(TZ=Africa/Cairo date +%F)
PREV_END=$(day "$(TZ=Africa/Cairo date +%Y-%m-01)" -1)
D1=$(day "$PREV_END" -1)     # the first day the End of Day runs
dayjob(){ sql "select j.\"STATUS_CODE\" || ' ' || j.\"ATTEMPTS\" from \"EOD_DAY_JOBS\" j join \"EOD_DAYS\" d using (\"DAY_ID\") where d.\"BUSINESS_DATE\"='$1' and j.\"JOB_CODE\"='$2'"; }

ADMIN=$(login admin 'Admin#Seed2026')
OLD=$(call GET /api/scr/companies "$ADMIN" | cut -f2- | jq -c '.[] | select(.companyCode == "OLDCO")'); CID=$(jq -r .memberCompanyId <<<"$OLD")

echo "1. The upgrade: the jobs, Retail Lending's history and the roles"
r=$(call GET /api/eod "$ADMIN"); P=$(body "$r")
check "page opens" "$(status "$r")" 200
check "four jobs, in order" "$(jq -r '[.jobs[].jobCode] | join(",")' <<<"$P")" "LND_DAILY,SCR_LICENCE_EXPIRY,SCR_INVOICES,SCR_HOUSEKEEPING"
check "lending's two days are End of Day days, done" "$(jq -r '[.days[] | .statusCode] | join(",")' <<<"$P")" "DONE,DONE"
check "each with its lending job" "$(jq -r '[.days[].jobs[] | .jobCode + " " + .statusCode] | unique | join(",")' <<<"$P")" "LND_DAILY DONE"
check "last day done" "$(jq -r .lastDoneDate <<<"$P")" "$(day "$PREV_END" -2)"
check "next day follows it" "$(jq -r '.nextDate + " " + (.isNextDayOpen|tostring) + " " + (.canRun|tostring)' <<<"$P")" "$D1 false true"
check "lending runs kept" "$(sql "select count(*) from \"LND_EOD_RUNS\"")" 2
check "the lending role can run the End of Day" "$(sql "select string_agg(c.\"CLAIM_VALUE\", ',' order by c.\"CLAIM_VALUE\") from \"SEC_ROLE_CLAIMS\" c join \"SEC_ROLES\" r on r.\"ROLE_ID\"=c.\"ROLE_ID\" where r.\"ROLE_NAME\"='EOD_OPERATOR'")" "EOD.JOBS.EDIT,EOD.RUN,EOD.VIEW,LND.EOD.RUN"
r=$(call POST /api/lnd/eod/run "$ADMIN" '{}'); check "lending no longer runs by itself" "$(status "$r")" 404

echo "2. Job settings"
r=$(call PUT /api/eod/jobs/SCR_INVOICES "$ADMIN" '{"isEnabled":true,"sequenceNo":30,"parameters":{"ISSUE_ZERO_INVOICES":"Y"}}')
check "a setting is saved" "$(body "$r" | jq -r '.parameters[0].value')" Y
r=$(call PUT /api/eod/jobs/SCR_INVOICES "$ADMIN" '{"isEnabled":true,"sequenceNo":0}'); check "order must be 1 to 9999" "$(status "$r")" 400
r=$(call PUT /api/eod/jobs/NOPE "$ADMIN" '{"isEnabled":true,"sequenceNo":5}'); check "unknown job" "$(status "$r")" 404

echo "3. Screening set-up: plan, monthly invoices, a licence ending in 7 days, a sign-in left waiting"
r=$(call POST /api/scr/billing/plans "$ADMIN" '{"planCode":"EODPLAN","nameEn":"EOD","nameAr":"خطة","userMonthlyFee":100,"inquiryFee":2.5,"isActive":true}'); PID=$(body "$r" | jq -r .pricePlanId)
r=$(call PUT /api/scr/companies/$CID "$ADMIN" "$(jq -c --argjson p "$PID" '{companyCode,nameEn,nameAr,licenceNo,statusCode,notes,pricePlanId:$p,billingCycleCode:"MONTHLY",contactEmail:"contact@oldco.test"}' <<<"$OLD")")
check "company billed monthly" "$(status "$r")" 200
sql "update \"SCR_COMPANY_LICENCES\" set \"VALID_TO\"='$(day "$D1" 7)' where \"LICENCE_ID\"=(select max(\"LICENCE_ID\") from \"SCR_COMPANY_LICENCES\" where \"MEMBER_COMPANY_ID\"=$CID)" >/dev/null
sql "insert into \"SEC_SIGN_INS\" (\"USER_NAME\",\"ATTEMPTED_AT\",\"RESULT_CODE\",\"CREATED_BY\",\"CREATED_AT\",\"ROW_VERSION\") values ('old.u1', now() - interval '2 hours', 'AWAITING_CODE', 'E2E', now(), 0)" >/dev/null
# lending is held up on the first day: its run is marked running
sql "insert into \"LND_EOD_RUNS\" (\"RUN_DATE\",\"STATUS_CODE\",\"LOANS_COUNT\",\"CREDIT_APPLIED_AMOUNT\",\"INTEREST_ACCRUED_AMOUNT\",\"INTEREST_SUSPENDED_AMOUNT\",\"LATE_FEE_AMOUNT\",\"NON_ACCRUAL_COUNT\",\"CURED_COUNT\",\"WRITE_OFF_PROPOSED_COUNT\",\"STARTED_AT\",\"CREATED_BY\",\"CREATED_AT\",\"ROW_VERSION\") values ('$D1','RUNNING',0,0,0,0,0,0,0,0,now(),'E2E',now(),0)" >/dev/null

echo "4. A job fails: the others run, the day stays open"
r=$(call POST /api/eod/run "$ADMIN" "{\"toDate\":\"$PREV_END\"}")
check "run answers" "$(status "$r")" 200
check "no day done, the first open" "$(body "$r" | jq -r '(.result.daysDone|tostring) + " " + .result.openDate')" "0 $D1"
check "lending failed" "$(dayjob "$D1" LND_DAILY)" "FAILED 1"
check "with the reason" "$(sql "select \"ERROR_MESSAGE\" like '%is running%' from \"EOD_DAY_JOBS\" j join \"EOD_DAYS\" d using (\"DAY_ID\") where d.\"BUSINESS_DATE\"='$D1' and j.\"JOB_CODE\"='LND_DAILY'")" t
check "licence notices still ran" "$(dayjob "$D1" SCR_LICENCE_EXPIRY)" "DONE 1"
check "invoices still ran" "$(dayjob "$D1" SCR_INVOICES)" "DONE 1"
check "housekeeping still ran" "$(dayjob "$D1" SCR_HOUSEKEEPING)" "DONE 1"
check "day open" "$(sql "select \"STATUS_CODE\" from \"EOD_DAYS\" where \"BUSINESS_DATE\"='$D1'")" OPEN
check "the next day did not run" "$(sql "select count(*) from \"EOD_DAYS\" where \"BUSINESS_DATE\"='$PREV_END'")" 0
check "licence notice to admin and contact (no mail server: logged)" "$(sql "select count(*) || ' ' || min(\"STATUS_CODE\") from \"CMC_EMAIL_LOGS\" where \"KIND_CODE\"='LICENCE_NOTICE'")" "2 NOT_CONFIGURED"
check "the summary says so" "$(sql "select \"SUMMARY\" like '%not sent%' from \"EOD_DAY_JOBS\" j join \"EOD_DAYS\" d using (\"DAY_ID\") where d.\"BUSINESS_DATE\"='$D1' and j.\"JOB_CODE\"='SCR_LICENCE_EXPIRY'")" t
check "the waiting sign-in is closed" "$(sql "select \"RESULT_CODE\" from \"SEC_SIGN_INS\" where \"CREATED_BY\"='E2E'")" CODE_TIMEOUT
r=$(call GET /api/eod "$ADMIN"); check "page shows the open day as next" "$(body "$r" | jq -r '.nextDate + " " + (.isNextDayOpen|tostring)')" "$D1 true"
r=$(call POST /api/eod/run "$ADMIN" "{\"toDate\":\"$PREV_END\"}")
check "still held: only lending runs again" "$(dayjob "$D1" LND_DAILY) / $(dayjob "$D1" SCR_INVOICES)" "FAILED 2 / DONE 1"

echo "5. The cause is fixed: run again, only the job not done runs, then the next day"
sql "update \"LND_EOD_RUNS\" set \"STATUS_CODE\"='FAILED' where \"RUN_DATE\"='$D1'" >/dev/null
r=$(call POST /api/eod/run "$ADMIN" "{\"toDate\":\"$PREV_END\"}")
check "two days done" "$(body "$r" | jq -r '(.result.daysDone|tostring) + " " + (.result.openDate|tostring)')" "2 null"
check "lending done on its 3rd attempt" "$(dayjob "$D1" LND_DAILY)" "DONE 3"
check "the jobs already done did not run again" "$(dayjob "$D1" SCR_LICENCE_EXPIRY) / $(dayjob "$D1" SCR_INVOICES)" "DONE 1 / DONE 1"
check "lending's own run done" "$(sql "select \"STATUS_CODE\" from \"LND_EOD_RUNS\" where \"RUN_DATE\"='$D1'")" DONE
check "both days done" "$(sql "select string_agg(\"STATUS_CODE\", ',' order by \"BUSINESS_DATE\") from \"EOD_DAYS\" where \"BUSINESS_DATE\">='$D1'")" "DONE,DONE"
check "month end: last month's invoice issued" "$(sql "select count(*) || ' ' || max(\"TOTAL_AMOUNT\") || ' ' || max(\"PERIOD_FROM\")::text from \"SCR_INVOICES\" where \"MEMBER_COMPANY_ID\"=$CID and \"PERIOD_TO\"='$PREV_END'")" "1 0.00 $(TZ=Africa/Cairo date -d "$PREV_END" +%Y-%m-01)"
check "issued by the End of Day" "$(sql "select \"NOTES\" from \"SCR_INVOICES\" where \"PERIOD_TO\"='$PREV_END'")" "Issued by end of day $PREV_END."
check "invoice job counted it" "$(sql "select \"ITEMS_COUNT\" from \"EOD_DAY_JOBS\" j join \"EOD_DAYS\" d using (\"DAY_ID\") where d.\"BUSINESS_DATE\"='$PREV_END' and j.\"JOB_CODE\"='SCR_INVOICES'")" 1
check "licence notice not sent twice for the same day" "$(sql "select count(*) from \"CMC_EMAIL_LOGS\" where \"KIND_CODE\"='LICENCE_NOTICE'")" 2

echo "6. A job switched off is skipped; days run up to today"
r=$(call PUT /api/eod/jobs/SCR_HOUSEKEEPING "$ADMIN" '{"isEnabled":false,"sequenceNo":40}'); check "switched off" "$(body "$r" | jq -r .isEnabled)" false
r=$(call POST /api/eod/run "$ADMIN" "{\"toDate\":\"$(day "$TODAY" 1)\"}"); check "never ahead of today" "$(status "$r")" 400
r=$(call POST /api/eod/run "$ADMIN" '{}')
check "run up to today" "$(body "$r" | jq -r '.result.openDate|tostring')" null
check "every day up to today done" "$(sql "select count(*) from \"EOD_DAYS\" where \"STATUS_CODE\"='DONE' and \"BUSINESS_DATE\" between '$D1' and '$TODAY'")" "$(python3 -c "import datetime as d,sys; print((d.date.fromisoformat(sys.argv[2])-d.date.fromisoformat(sys.argv[1])).days+1)" "$D1" "$TODAY")"
check "housekeeping skipped today" "$(dayjob "$TODAY" SCR_HOUSEKEEPING)" "SKIPPED 0"
check "lending ran each day" "$(sql "select count(*) from \"LND_EOD_RUNS\" where \"STATUS_CODE\"='DONE'")" "$(python3 -c "import datetime as d,sys; print((d.date.fromisoformat(sys.argv[2])-d.date.fromisoformat(sys.argv[1])).days+3)" "$D1" "$TODAY")"
r=$(call POST /api/eod/run "$ADMIN" '{}'); check "done up to today: nothing to run" "$(status "$r")" 400
r=$(call GET /api/eod "$ADMIN"); check "page: done up to today" "$(body "$r" | jq -r '(.nextDate|tostring) + " " + (.canRun|tostring)')" "null false"
r=$(call GET /api/lnd/eod "$ADMIN"); check "lending detail still opens" "$(status "$r") $(body "$r" | jq -r '.canRun')" "200 false"
r=$(call GET /api/eod "" ); check "signed out: refused" "$(status "$r")" 401

echo; echo "Passed: $pass  Failed: $fail"
[ "$fail" -eq 0 ]
