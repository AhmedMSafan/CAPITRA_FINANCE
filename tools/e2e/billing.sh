#!/usr/bin/env bash
# End-to-end check of screening billing against the real API and a real PostgreSQL database. Before this runs, develop made the
# database with a company OLDCO (administrator old.admin, user old.u1, both activated; old.u1 made 2 inquiries).
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
# Cairo dates
TODAY=$(TZ=Africa/Cairo date +%F)
THIS_MONTH=$(TZ=Africa/Cairo date +%Y-%m-01)
PREV_MONTH=$(python3 -c "import datetime as d,sys; t=d.date.fromisoformat(sys.argv[1]); p=(t.replace(day=1)-d.timedelta(days=1)).replace(day=1); print(p)" "$THIS_MONTH")
QUARTER=$(python3 -c "import datetime as d,sys; t=d.date.fromisoformat(sys.argv[1]); print(t.replace(month=(t.month-1)//3*3+1, day=1))" "$TODAY")
NEXT_MONTH=$(python3 -c "import datetime as d,sys; t=d.date.fromisoformat(sys.argv[1]); print((t.replace(day=28)+d.timedelta(days=4)).replace(day=1))" "$THIS_MONTH")
PY=${PREV_MONTH:0:4}

ADMIN=$(login admin 'Admin#Seed2026')
OLD=$(call GET /api/scr/companies "$ADMIN" | cut -f2- | jq -c '.[] | select(.companyCode == "OLDCO")'); CID=$(jq -r .memberCompanyId <<<"$OLD")
U1=$(login old.u1 'Old#User2026'); CA=$(login old.admin 'Old#Admin2026')

echo "1. The upgrade starts the activity history of the usable accounts"
check "two open periods for OLDCO" "$(sql "select count(*) from \"SCR_USER_ACTIVITY\" where \"MEMBER_COMPANY_ID\"=$CID and \"ACTIVE_TO\" is null")" 2
check "from the day they were made (today)" "$(sql "select distinct \"ACTIVE_FROM\" from \"SCR_USER_ACTIVITY\" where \"MEMBER_COMPANY_ID\"=$CID")" "$TODAY"
check "a pending invitation is not billed" "$(sql "select count(*) from \"SCR_USER_ACTIVITY\" where \"USER_NAME\"='old.pending'")" 0

echo "2. Price plans"
r=$(call POST /api/scr/billing/plans "$ADMIN" '{"planCode":"STANDARD","nameEn":"Standard","nameAr":"قياسية","userMonthlyFee":100.005,"inquiryFee":2.5,"isActive":true}')
check "three decimals refused" "$(status "$r")" 400
r=$(call POST /api/scr/billing/plans "$ADMIN" '{"planCode":"STANDARD","nameEn":"Standard","nameAr":"قياسية","userMonthlyFee":100,"inquiryFee":2.5,"isActive":true}')
check "plan saved" "$(status "$r")" 200
PID=$(body "$r" | jq -r .pricePlanId)
r=$(call POST /api/scr/billing/plans "$ADMIN" '{"planCode":"standard","nameEn":"x","nameAr":"x","userMonthlyFee":1,"inquiryFee":1,"isActive":true}')
check "same code refused" "$(status "$r")" 400
r=$(call GET "/api/scr/billing/companies/$CID/preview" "$ADMIN"); check "no invoice without a plan" "$(status "$r")" 400
put_company() { call PUT /api/scr/companies/$CID "$ADMIN" "$(jq -c --argjson p "$1" --arg c "$2" '{companyCode,nameEn,nameAr,licenceNo,contactEmail,statusCode,notes,pricePlanId:$p,billingCycleCode:$c}' <<<"$OLD")"; }
r=$(put_company "$PID" MONTHLY); check "company gets the plan, monthly" "$(body "$r" | jq -r '.pricePlanCode + " " + .billingCycleCode')" "STANDARD MONTHLY"

echo "3. The current month so far"
r=$(call GET /api/scr/billing/overview "$ADMIN"); O=$(body "$r" | jq -c --arg c OLDCO '.[] | select(.companyCode == $c)')
check "2 user-months, 2 inquiries" "$(jq -r '"\(.currentUserMonths) \(.currentInquiries)"' <<<"$O")" "2 2"
check "2 × 100 + 2 × 2.50 = 205" "$(jq -r .currentAmount <<<"$O")" 205
r=$(call GET "/api/scr/billing/companies/$CID/preview?periodFrom=$THIS_MONTH" "$ADMIN")
check "preview: 205.00, period not complete" "$(body "$r" | jq -r '"\(.totalAmount) \(.periodComplete) \(.statusCode)"')" "205 false PREVIEW"
check "one line per user" "$(body "$r" | jq -r '[.lines[].userName] | join(",")')" "old.admin,old.u1"
r=$(call GET "/api/scr/billing/companies/$CID/preview?periodFrom=${THIS_MONTH%01}15" "$ADMIN"); check "a period must start on its first day" "$(status "$r")" 400
r=$(call GET "/api/scr/billing/companies/$CID/preview?periodFrom=$NEXT_MONTH" "$ADMIN"); check "a future period is refused" "$(status "$r")" 400

echo "4. Last month: admin active since earlier, old.u1 made one inquiry then"
sql "update \"SCR_USER_ACTIVITY\" set \"ACTIVE_FROM\" = '$PREV_MONTH'::date - 40 where \"USER_NAME\"='old.admin'" >/dev/null
sql "update \"SCR_INQUIRIES\" set \"SCREENED_AT\" = ('$PREV_MONTH'::date + 9)::timestamp at time zone 'Africa/Cairo' where \"INQUIRY_ID\" = (select min(\"INQUIRY_ID\") from \"SCR_INQUIRIES\" where \"MEMBER_COMPANY_ID\"=$CID)" >/dev/null
r=$(call GET "/api/scr/billing/companies/$CID/preview?periodFrom=$PREV_MONTH" "$ADMIN")
check "1 user-month + 1 inquiry = 102.50" "$(body "$r" | jq -r '"\(.userMonths) \(.inquiryCount) \(.totalAmount)"')" "1 1 102.5"
check "complete period" "$(body "$r" | jq -r .periodComplete)" true
check "old.u1's line: no month, 1 inquiry" "$(body "$r" | jq -r '.lines[] | select(.userName=="old.u1") | "\(.userMonths) \(.inquiryCount) \(.lineAmount)"')" "0 1 2.5"

echo "5. Issue, never twice, prices frozen, cancel and reissue"
r=$(call POST /api/scr/billing/companies/$CID/invoices "$ADMIN" "{\"periodFrom\":\"$PREV_MONTH\",\"notes\":\"first\"}")
check "issued" "$(status "$r")" 200
I1=$(body "$r" | jq -r .invoiceId)
check "numbered INV-$PY-000001" "$(body "$r" | jq -r .invoiceNo)" "INV-$PY-000001"
check "total 102.50" "$(body "$r" | jq -r .totalAmount)" 102.5
r=$(call POST /api/scr/billing/companies/$CID/invoices "$ADMIN" "{\"periodFrom\":\"$PREV_MONTH\"}"); check "the same period cannot be invoiced twice" "$(status "$r")" 400
r=$(call PUT /api/scr/billing/plans/$PID "$ADMIN" '{"planCode":"STANDARD","nameEn":"Standard","nameAr":"قياسية","userMonthlyFee":200,"inquiryFee":2.5,"isActive":true}')
check "plan price raised to 200" "$(body "$r" | jq -r .userMonthlyFee)" 200
r=$(call GET /api/scr/billing/invoices/$I1 "$ADMIN"); check "the issued invoice keeps 100" "$(body "$r" | jq -r '"\(.userMonthlyFee) \(.totalAmount)"')" "100 102.5"
r=$(call POST /api/scr/billing/invoices/$I1/cancel "$ADMIN" '{"reason":""}'); check "cancelling needs a reason" "$(status "$r")" 400
r=$(call POST /api/scr/billing/invoices/$I1/cancel "$ADMIN" '{"reason":"Wrong price"}'); check "cancelled, kept" "$(body "$r" | jq -r '"\(.statusCode) \(.cancelReason)"')" "CANCELLED Wrong price"
r=$(call POST /api/scr/billing/invoices/$I1/cancel "$ADMIN" '{"reason":"again"}'); check "not twice" "$(status "$r")" 400
r=$(call POST /api/scr/billing/companies/$CID/invoices "$ADMIN" "{\"periodFrom\":\"$PREV_MONTH\"}")
check "reissued as INV-$PY-000002 at the new price" "$(body "$r" | jq -r '"\(.invoiceNo) \(.totalAmount)"')" "INV-$PY-000002 202.5"
check "both invoices are kept" "$(sql "select count(*) from \"SCR_INVOICES\" where \"MEMBER_COMPANY_ID\"=$CID")" 2
check "with their lines" "$(sql "select count(*) from \"SCR_INVOICE_LINES\"")" 4

echo "6. Activity follows the accounts"
UID1=$(call GET /api/scr/companies/$CID/users "$ADMIN" | cut -f2- | jq -r '.[] | select(.userName=="old.u1") | .userId')
r=$(call PUT /api/scr/companies/$CID/users/$UID1 "$CA" '{"userName":"old.u1","email":"old.u1@e2e.test","fullName":"Old One","isActive":false}'); check "old.u1 deactivated" "$(status "$r")" 200
check "its period closed today" "$(sql "select \"ACTIVE_TO\" from \"SCR_USER_ACTIVITY\" where \"USER_NAME\"='old.u1'")" "$TODAY"
r=$(call PUT /api/scr/companies/$CID/users/$UID1 "$CA" '{"userName":"old.u1","email":"old.u1@e2e.test","fullName":"Old One","isActive":true}'); check "and reactivated" "$(status "$r")" 200
check "a new period opened" "$(sql "select count(*) from \"SCR_USER_ACTIVITY\" where \"USER_NAME\"='old.u1'")" 2
r=$(call GET "/api/scr/billing/companies/$CID/preview?periodFrom=$THIS_MONTH" "$ADMIN"); check "this month still counts old.u1 once" "$(body "$r" | jq -r .userMonths)" 2

echo "7. Quarterly invoicing, totals, and who may see billing"
r=$(put_company "$PID" QUARTERLY); check "company now invoiced quarterly" "$(body "$r" | jq -r .billingCycleCode)" QUARTERLY
r=$(call GET /api/scr/billing/overview "$ADMIN"); check "its current period is the quarter from $QUARTER" "$(body "$r" | jq -r '.[] | select(.companyCode=="OLDCO") | .currentFrom')" "$QUARTER"
r=$(call GET /api/scr/billing/overview "$ADMIN"); check "OLDCO invoiced total 202.50 (the cancelled one not counted)" "$(body "$r" | jq -r '.[] | select(.companyCode=="OLDCO") | "\(.invoicedTotal) \(.invoiceCount)"')" "202.5 1"
r=$(call GET /api/scr/billing/overview "$(login old.u1 'Old#User2026')"); check "a company user cannot see billing" "$(status "$r")" 403
r=$(call GET /api/scr/billing/invoices "$CA"); check "nor its administrator (for now)" "$(status "$r")" 403

echo
echo "Passed: $pass   Failed: $fail"
[ "$fail" -eq 0 ]
