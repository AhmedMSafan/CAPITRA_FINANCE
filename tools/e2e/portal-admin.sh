#!/usr/bin/env bash
# End-to-end check of the Screening Portal's pages for a company administrator: the company's own invoices (no GL references) and its
# users' sign-ins, never another company's. Before this runs, develop made the database with company OLDCO (old.admin, old.u1).
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
login() { call POST /api/sec/auth/login "" "{\"userName\":\"$1\",\"password\":\"$2\"}"; }
sql()   { docker exec "$PG" psql -U postgres -d capitra -tAc "$1"; }
totp()  { python3 -c '
import base64,hmac,struct,time,hashlib,sys
k=sys.argv[1].replace(" ","").upper(); k=base64.b32decode(k+"="*((8-len(k)%8)%8))
h=hmac.new(k,struct.pack(">Q",int(time.time())//30),hashlib.sha1).digest(); o=h[-1]&15
print("%06d"%((struct.unpack(">I",h[o:o+4])[0]&0x7fffffff)%1000000))' "$1"; }
step()  { jq -nc --arg c "$1" --arg code "${2:-}" --arg r "${3:-}" '{challenge:$c} + (if $code=="" then {} else {code:$code} end) + (if $r=="" then {} else {recoveryCode:$r} end)'; }
# a company account's token: set up the authenticator on first sign-in
company_token() {
  local r C K
  r=$(login "$1" "$2"); C=$(body "$r" | jq -r .secondStep.challenge)
  r=$(call POST /api/sec/auth/second-step/setup "" "$(step "$C")"); K=$(body "$r" | jq -r .sharedKey)
  call POST /api/sec/auth/second-step/enable "" "$(step "$C" "$(totp "$K")")" | cut -f2- | jq -r .login.token
}

ADMIN=$(login admin 'Admin#Seed2026' | cut -f2- | jq -r .token)
CID=$(call GET /api/scr/companies "$ADMIN" | cut -f2- | jq -r '.[] | select(.companyCode == "OLDCO") | .memberCompanyId')
OLDJSON=$(call GET /api/scr/companies "$ADMIN" | cut -f2- | jq -c '.[] | select(.companyCode == "OLDCO")')
# a free plan, so an invoice can be issued without GL; and a second company whose invoice OLDCO must not see
r=$(call POST /api/scr/billing/plans "$ADMIN" '{"planCode":"FREE","nameEn":"Free","nameAr":"مجاني","userMonthlyFee":0,"inquiryFee":0,"isActive":true}'); PID=$(body "$r" | jq -r .pricePlanId)
call PUT /api/scr/companies/$CID "$ADMIN" "$(jq -c --argjson p "$PID" '{companyCode,nameEn,nameAr,licenceNo,contactEmail,statusCode,notes,pricePlanId:$p,billingCycleCode:"MONTHLY"}' <<<"$OLDJSON")" >/dev/null
MONTH=$(TZ=Africa/Cairo date +%Y-%m-01)
r=$(call POST /api/scr/billing/companies/$CID/invoices "$ADMIN" "{\"periodFrom\":\"$MONTH\"}"); IID=$(body "$r" | jq -r .invoiceId)
check "an invoice for OLDCO" "$(status "$r")" 200
OTHER=$(sql "select min(\"INVOICE_ID\") from \"SCR_INVOICES\" where \"MEMBER_COMPANY_ID\"<>$CID")

echo "1. The company administrator"
CA=$(company_token old.admin 'Old#Admin2026')
check "signed in with the app code" "$(grep -c '\.' <<<"$CA")" 1
r=$(call GET /api/scr/portal/invoices "$CA"); check "sees the company's invoices" "$(status "$r") $(body "$r" | jq -r '[.[].memberCompanyId] | unique | join(",")')" "200 $CID"
r=$(call GET /api/scr/portal/invoices/$IID "$CA"); check "opens one, without GL references" "$(status "$r") $(body "$r" | jq -r '(.glTranSetId|tostring) + " " + (.cancelGlTranSetId|tostring)')" "200 null null"
if [ -n "$OTHER" ]; then r=$(call GET /api/scr/portal/invoices/$OTHER "$CA"); check "not another company's" "$(status "$r")" 404; fi
r=$(call GET /api/scr/billing/invoices/$IID "$CA"); check "not the Federation's billing" "$(status "$r")" 403
r=$(call GET "/api/scr/portal/sign-ins" "$CA")
check "sees the company's sign-ins" "$(status "$r") $(body "$r" | jq -r '[.rows[].memberCompanyId] | unique | join(",")')" "200 $CID"
check "including its own" "$(body "$r" | jq -r '[.rows[] | select(.userName == "old.admin" and .resultCode == "SUCCESS")] | length > 0')" true
r=$(call GET "/api/scr/portal/sign-ins?memberCompanyId=999" "$CA"); check "asking for another company still shows its own" "$(body "$r" | jq -r '[.rows[].memberCompanyId] | unique | join(",")')" "$CID"
SID=$(body "$r" | jq -r '.rows[0].signInId')
r=$(call GET /api/scr/portal/sign-ins/$SID "$CA"); check "a session's inquiries" "$(status "$r")" 200
FOREIGN=$(sql "select min(\"SIGN_IN_ID\") from \"SEC_SIGN_INS\" where \"MEMBER_COMPANY_ID\" is null")
r=$(call GET /api/scr/portal/sign-ins/$FOREIGN "$CA"); check "not another's session" "$(status "$r")" 404
r=$(call GET /api/scr/access-log "$CA"); check "not the Federation's sign-in log" "$(status "$r")" 403

echo "2. A company user (not the administrator)"
CU=$(company_token old.u1 'Old#User2026')
r=$(call GET /api/scr/portal/invoices "$CU"); check "no invoices" "$(status "$r")" 403
r=$(call GET /api/scr/portal/sign-ins "$CU"); check "no sign-ins" "$(status "$r")" 403

echo; echo "Passed: $pass  Failed: $fail"
[ "$fail" -eq 0 ]
