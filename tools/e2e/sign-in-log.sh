#!/usr/bin/env bash
# End-to-end check of the sign-in log against the real API and a real PostgreSQL database. Before this runs, develop made the
# database with company OLDCO (old.admin, old.u1 activated; old.u1 made 2 inquiries before sessions were kept).
set -euo pipefail
API=${API:-http://localhost:5080}
UA='Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36'
pass=0; fail=0
ok()   { echo "  ✅ $1"; pass=$((pass+1)); }
bad()  { echo "  ❌ $1"; fail=$((fail+1)); }
check(){ if [ "$2" = "$3" ]; then ok "$1"; else bad "$1 (got [$2], want [$3])"; fi; }
call() { local m=$1 p=$2 t=$3 d=${4:-}; local out; out=$(mktemp)
  local code; code=$(curl -s -A "$UA" -o "$out" -w '%{http_code}' -X "$m" "$API$p" -H 'Content-Type: application/json' ${t:+-H "Authorization: Bearer $t"} ${d:+--data "$d"})
  printf '%s\t%s' "$code" "$(tr '\n' ' ' < "$out")"; rm -f "$out"; }
status(){ cut -f1 <<<"$1"; }
body()  { cut -f2- <<<"$1"; }
login() { call POST /api/sec/auth/login "" "{\"userName\":\"$1\",\"password\":\"$2\"}"; }
sql()   { docker exec "$PG" psql -U postgres -d capitra -tAc "$1"; }
claim() { cut -d. -f2 <<<"$1" | tr '_-' '/+' | python3 -c 'import sys,base64,json; s=sys.stdin.read().strip(); s+="="*(-len(s)%4); print(json.loads(base64.b64decode(s)).get(sys.argv[1],""))' "$2"; }
inquire() { call POST /api/scr/inquiries "$1" "{\"subjectTypeCode\":\"PERSON\",\"subjectName\":\"Session Person $2\",\"identityTypeCode\":\"NATIONAL_ID\",\"identityNo\":\"2900101123457$2\",\"purposeCode\":\"ONBOARDING\"}"; }

echo "1. Inquiries made before the upgrade are kept, without a session"
check "2 old inquiries, no session" "$(sql "select count(*) from \"SCR_INQUIRIES\" where \"SESSION_ID\" is null")" 2

echo "2. Refused sign-ins are recorded, with why"
r=$(login old.u1 'Wrong#Pass1'); check "wrong password refused" "$(status "$r")" 401
r=$(login nobody.here 'Whatever#1'); check "unknown user refused" "$(status "$r")" 401
check "recorded: BAD_PASSWORD for old.u1" "$(sql "select \"RESULT_CODE\" from \"SEC_SIGN_INS\" where \"USER_NAME\"='old.u1' order by 1 limit 1")" BAD_PASSWORD
check "recorded: UNKNOWN_USER, no account" "$(sql "select \"RESULT_CODE\" || ' ' || coalesce(\"USER_ID\",'-') from \"SEC_SIGN_INS\" where \"USER_NAME\"='nobody.here'")" "UNKNOWN_USER -"
check "with the address (local: 127.0.0.1 or ::1) and the browser" "$(sql "select (\"IP_ADDRESS\" in ('127.0.0.1','::1')) || ' ' || (\"USER_AGENT\" like '%Chrome/131%') from \"SEC_SIGN_INS\" where \"USER_NAME\"='nobody.here'")" "true true"
check "no password stored anywhere" "$(sql "select count(*) from \"SEC_SIGN_INS\" where \"FAILURE_TEXT\" like '%Wrong#Pass1%' or \"USER_NAME\" like '%Pass1%'")" 0

echo "3. A sign-in opens a session; inquiries point back to it"
r=$(login old.u1 'Old#User2026'); check "old.u1 signs in" "$(status "$r")" 200
T=$(body "$r" | jq -r .token); SID=$(claim "$T" sid)
[ ${#SID} -eq 32 ] && ok "the token carries the session id" || bad "no session id in the token [$SID]"
check "the sign-in is recorded with it" "$(sql "select \"RESULT_CODE\" from \"SEC_SIGN_INS\" where \"SESSION_ID\"='$SID'")" SUCCESS
check "and when the session ends" "$(sql "select \"EXPIRES_AT\" is not null from \"SEC_SIGN_INS\" where \"SESSION_ID\"='$SID'")" t
r=$(inquire "$T" 1); check "inquiry 1" "$(status "$r")" 200
r=$(inquire "$T" 2); check "inquiry 2" "$(status "$r")" 200
check "both carry the session" "$(sql "select count(*) from \"SCR_INQUIRIES\" where \"SESSION_ID\"='$SID'")" 2
check "received before answered" "$(sql "select bool_and(\"RECEIVED_AT\" <= \"RESPONDED_AT\") from \"SCR_INQUIRIES\" where \"SESSION_ID\"='$SID'")" t

echo "4. The log, as the Federation sees it"
A=$(login admin 'Admin#Seed2026' | cut -f2- | jq -r .token)
CID=$(call GET /api/scr/companies "$A" | cut -f2- | jq -r '.[] | select(.companyCode=="OLDCO") | .memberCompanyId')
r=$(call GET "/api/scr/access-log?memberCompanyId=$CID" "$A"); check "OLDCO: 2 attempts (one refused, one signed in)" "$(body "$r" | jq -r '"\(.summary.attempts) \(.summary.refused) \(.summary.successful)"')" "2 1 1"
ROW=$(body "$r" | jq -c --arg s "$SID" '.rows[] | select(.sessionId == $s)')
check "the session: open, 2 inquiries, Chrome on Windows" "$(jq -r '"\(.sessionStateCode) \(.inquiryCount) \(.device)"' <<<"$ROW")" "OPEN 2 Chrome · Windows"
check "with the company" "$(jq -r .companyCode <<<"$ROW")" OLDCO
r=$(call GET "/api/scr/access-log?resultCode=REFUSED" "$A"); check "refused only: the 2 refusals" "$(body "$r" | jq -r '[.rows[].resultCode] | sort | join(",")')" "BAD_PASSWORD,UNKNOWN_USER"
r=$(call GET "/api/scr/access-log?memberCompanyId=0" "$A"); check "Federation users only: the admin's sign-in" "$(body "$r" | jq -r '[.rows[] | select(.userName=="admin")] | length')" 1
r=$(call GET "/api/scr/access-log/$(jq -r .signInId <<<"$ROW")" "$A")
check "session activity lists its 2 inquiries" "$(body "$r" | jq -r '[.inquiries[].subjectName] | join(",")')" "Session Person 1,Session Person 2"
check "each with the time it took" "$(body "$r" | jq -r '[.inquiries[] | (.durationMs >= 0)] | all')" true
check "and its result" "$(body "$r" | jq -r '.inquiries[0].resultCode')" NOT_FOUND

echo "5. Signing out ends the session and its token"
r=$(call POST /api/sec/auth/logout "$T"); check "signed out" "$(status "$r")" 204
check "the time is recorded" "$(sql "select \"SIGNED_OUT_AT\" is not null from \"SEC_SIGN_INS\" where \"SESSION_ID\"='$SID'")" t
r=$(inquire "$T" 3); check "the old token is refused at once" "$(status "$r")" 401
r=$(login old.u1 'Old#User2026'); T2=$(body "$r" | jq -r .token)
r=$(inquire "$T2" 4); check "a new sign-in works" "$(status "$r")" 200
check "in its own session" "$(sql "select count(distinct \"SESSION_ID\") from \"SCR_INQUIRIES\" where \"SESSION_ID\" is not null")" 2
r=$(call GET "/api/scr/access-log?memberCompanyId=$CID" "$A"); check "the log shows the first session signed out" "$(body "$r" | jq -r --arg s "$SID" '.rows[] | select(.sessionId == $s) | .sessionStateCode')" SIGNED_OUT

echo "6. Who may see it"
r=$(call GET /api/scr/access-log "$T2"); check "a company user cannot" "$(status "$r")" 403
CA=$(login old.admin 'Old#Admin2026' | cut -f2- | jq -r .token)
r=$(call GET /api/scr/access-log "$CA"); check "nor its administrator (for now)" "$(status "$r")" 403

echo
echo "Passed: $pass   Failed: $fail"
[ "$fail" -eq 0 ]
