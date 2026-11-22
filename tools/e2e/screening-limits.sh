#!/usr/bin/env bash
# End-to-end check of the screening limits: the licence's yearly inquiry quota (portal and API together), the request limits, and the
# network block after repeated failed sign-ins. Before this runs, develop made the database with company OLDCO.
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
screen(){ call POST /api/scr/v1/inquiries "$1" "{\"subjectTypeCode\":\"PERSON\",\"subjectName\":\"Quota Person\",\"identityTypeCode\":\"NATIONAL_ID\",\"identityNo\":\"2900101123456$2\",\"purposeCode\":\"ONBOARDING\"}"; }

ADMIN=$(login admin 'Admin#Seed2026')
CID=$(call GET /api/scr/companies "$ADMIN" | cut -f2- | jq -r '.[] | select(.companyCode == "OLDCO") | .memberCompanyId')
r=$(call POST /api/scr/companies/$CID/api-clients "$ADMIN" '{"clientName":"Limits test"}')
CLIENT=$(body "$r" | jq -r .client.clientId); SECRET=$(body "$r" | jq -r .clientSecret)
TOKEN=$(call POST /api/scr/v1/token "" "{\"clientId\":\"$CLIENT\",\"clientSecret\":\"$SECRET\"}" | cut -f2- | jq -r .accessToken)
USED=$(sql "select count(*) from \"SCR_INQUIRIES\" where \"MEMBER_COMPANY_ID\"=$CID")

echo "1. Yearly quota: the licence year, used and left"
r=$(call GET /api/scr/v1/quota "$TOKEN"); check "no quota yet" "$(body "$r" | jq -r '(.quota|tostring) + " " + (.used|tostring) + " " + (.left|tostring)')" "null $USED null"
FROM=$(sql "select \"VALID_FROM\" from \"SCR_COMPANY_LICENCES\" where \"MEMBER_COMPANY_ID\"=$CID order by \"LICENCE_ID\" desc limit 1")
check "the licence year starts on the licence's start day" "$(body "$r" | jq -r .from)" "$FROM"
sql "update \"SCR_COMPANY_LICENCES\" set \"YEARLY_INQUIRY_QUOTA\"=$((USED + 2)) where \"MEMBER_COMPANY_ID\"=$CID" >/dev/null
r=$(screen "$TOKEN" 1); check "inquiry 1 of 2 left" "$(status "$r")" 200
r=$(call GET /api/scr/v1/quota "$TOKEN"); check "one left" "$(body "$r" | jq -r '.left')" 1
r=$(screen "$TOKEN" 2); check "the last one" "$(status "$r")" 200
r=$(screen "$TOKEN" 3); check "quota used: refused" "$(status "$r") $(body "$r" | jq -r .error)" "403 quota_reached"
check "with the reason" "$(body "$r" | jq -r .message | grep -c 'yearly quota of')" 1
check "nothing kept for the refused one" "$(sql "select count(*) from \"SCR_INQUIRIES\" where \"IDENTITY_NO\"='29001011234563'")" 0
check "the company's portal sees it" "$(sql "select count(*) from \"SCR_INQUIRIES\" where \"MEMBER_COMPANY_ID\"=$CID")" "$((USED + 2))"
sql "update \"SCR_COMPANY_LICENCES\" set \"YEARLY_INQUIRY_QUOTA\"=null where \"MEMBER_COMPANY_ID\"=$CID" >/dev/null
r=$(screen "$TOKEN" 3); check "quota raised: screening again" "$(status "$r")" 200

echo "2. Request limits"
codes=""; for i in $(seq 1 61); do codes="$codes $(status "$(call GET /api/scr/v1/quota "$TOKEN")")"; done
check "60 API requests a minute per client: then 429" "$(( $(tr ' ' '\n' <<<"$codes" | grep -c 200) <= 60 )) $(( $(tr ' ' '\n' <<<"$codes" | grep -c 429) >= 1 ))" "1 1"
r=$(call GET /api/scr/v1/quota "$TOKEN"); check "429 says why" "$(status "$r") $(body "$r" | jq -r .error)" "429 rate_limited"
check "and when to retry" "$(curl -s -o /dev/null -D - "$API/api/scr/v1/quota" -H "Authorization: Bearer $TOKEN" | grep -ci '^retry-after:')" 1
codes=""; for i in $(seq 1 10); do codes="$codes $(status "$(call POST /api/scr/v1/token "" '{"clientId":"x","clientSecret":"y"}')")"; done
check "token requests: 10 a minute per network" "$(( $(tr ' ' '\n' <<<"$codes" | grep -c 429) >= 1 ))" 1

echo "3. Network block after 20 failed sign-ins in 15 minutes"
BEFORE=$(sql "select count(*) from \"SEC_SIGN_INS\" where \"RESULT_CODE\" in ('UNKNOWN_USER','BAD_PASSWORD','BAD_CODE') and \"ATTEMPTED_AT\" > now() - interval '15 minutes'")
for i in $(seq 1 $((20 - BEFORE))); do call POST /api/sec/auth/login "" "{\"userName\":\"nobody$i\",\"password\":\"x\"}" >/dev/null; done
r=$(call POST /api/sec/auth/login "" '{"userName":"admin","password":"Admin#Seed2026"}')
check "even the right password is refused" "$(status "$r")" 401
check "with the reason" "$(body "$r" | jq -r .detail)" "Too many failed sign-ins from your network. Try again in 15 minutes."
check "logged as network blocked" "$(sql "select \"RESULT_CODE\" from \"SEC_SIGN_INS\" order by \"SIGN_IN_ID\" desc limit 1")" IP_BLOCKED
r=$(call GET "/api/scr/access-log?resultCode=REFUSED" "$ADMIN"); check "a refusal in the sign-in log" "$(body "$r" | jq -r '[.. | objects | select(.resultCode? == "IP_BLOCKED")] | length > 0')" true
codes=""; for i in $(seq 1 35); do codes="$codes $(status "$(call POST /api/sec/auth/login "" '{"userName":"x","password":"y"}')")"; done
check "sign-in requests: 30 a minute per network" "$(( $(tr ' ' '\n' <<<"$codes" | grep -c 429) >= 1 ))" 1

echo; echo "Passed: $pass  Failed: $fail"
[ "$fail" -eq 0 ]
