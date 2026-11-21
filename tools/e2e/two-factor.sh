#!/usr/bin/env bash
# End-to-end check of two-factor sign-in (authenticator app) against the real API and a real PostgreSQL database. The codes are
# computed from the shared key exactly as an authenticator app does (RFC 6238, 30 s, 6 digits). Before this runs, develop made
# the database with company OLDCO (old.admin and old.u1 activated).
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
inquire() { call POST /api/scr/inquiries "$1" "{\"subjectTypeCode\":\"PERSON\",\"subjectName\":\"Twofa Person $2\",\"identityTypeCode\":\"NATIONAL_ID\",\"identityNo\":\"2900101123458$2\",\"purposeCode\":\"ONBOARDING\"}"; }

echo "1. A company account must set up its authenticator at the first sign-in"
r=$(login old.u1 'Old#User2026'); check "password right" "$(status "$r")" 200
check "no token yet: a second step" "$(body "$r" | jq -r '"\(.token // "none") \(.secondStep.kind)"')" "none SETUP"
C=$(body "$r" | jq -r .secondStep.challenge)
check "the sign-in log: password right, code not entered yet" "$(sql "select \"RESULT_CODE\" from \"SEC_SIGN_INS\" where \"USER_NAME\"='old.u1' order by \"SIGN_IN_ID\" desc limit 1")" AWAITING_CODE
r=$(call POST /api/sec/auth/second-step/setup "" "$(step "$C")"); check "set-up: QR code and key" "$(body "$r" | jq -r '(.qrCode | startswith("data:image/png;base64,")) and (.otpAuthUri | startswith("otpauth://totp/CAPITRA"))')" true
KEY=$(body "$r" | jq -r .sharedKey)
r=$(call POST /api/sec/auth/second-step/setup "" "$(step "$C")"); check "asking again gives the same key" "$(body "$r" | jq -r .sharedKey)" "$KEY"
r=$(call POST /api/sec/auth/second-step/enable "" "$(step "$C" 000000)"); check "a wrong first code is refused" "$(status "$r")" 400
r=$(call POST /api/sec/auth/second-step/enable "" "$(step "$C" "$(totp "$KEY")")"); check "the app's code turns it on" "$(status "$r")" 200
T=$(body "$r" | jq -r .login.token); CODES=$(body "$r" | jq -c .recoveryCodes)
check "10 backup codes, shown once" "$(jq length <<<"$CODES")" 10
r=$(inquire "$T" 1); check "signed in: can screen" "$(status "$r")" 200
check "the same sign-in row now says SUCCESS by set-up" "$(sql "select \"RESULT_CODE\" || ' ' || \"SECOND_STEP_CODE\" from \"SEC_SIGN_INS\" where \"USER_NAME\"='old.u1' order by \"SIGN_IN_ID\" desc limit 1")" "SUCCESS SETUP"
r=$(call POST /api/sec/auth/second-step/enable "" "$(step "$C" "$(totp "$KEY")")"); check "a used challenge cannot be used again" "$(status "$r")" 401

echo "2. Every sign-in after: the app's code, or a backup code"
r=$(login old.u1 'Old#User2026'); check "asks for the code" "$(body "$r" | jq -r .secondStep.kind)" CODE
C=$(body "$r" | jq -r .secondStep.challenge)
r=$(call POST /api/sec/auth/second-step/verify "" "$(step "$C" 123456)"); check "a wrong code is refused" "$(status "$r")" 400
check "and recorded" "$(sql "select count(*) from \"SEC_SIGN_INS\" where \"USER_NAME\"='old.u1' and \"RESULT_CODE\"='BAD_CODE'")" 1
r=$(call POST /api/sec/auth/second-step/verify "" "$(step "$C" "$(totp "$KEY")")"); check "the right code signs in" "$(status "$r")" 200
check "with the app" "$(sql "select \"SECOND_STEP_CODE\" from \"SEC_SIGN_INS\" where \"USER_NAME\"='old.u1' and \"RESULT_CODE\"='SUCCESS' order by \"SIGN_IN_ID\" desc limit 1")" AUTHENTICATOR
B=$(jq -r '.[0]' <<<"$CODES")
r=$(login old.u1 'Old#User2026'); C=$(body "$r" | jq -r .secondStep.challenge)
r=$(call POST /api/sec/auth/second-step/verify "" "$(step "$C" "" "$B")"); check "a backup code signs in" "$(status "$r")" 200
check "9 backup codes left" "$(body "$r" | jq -r .recoveryCodesLeft)" 9
r=$(login old.u1 'Old#User2026'); C=$(body "$r" | jq -r .secondStep.challenge)
r=$(call POST /api/sec/auth/second-step/verify "" "$(step "$C" "" "$B")"); check "the same backup code works only once" "$(status "$r")" 400
r=$(call POST /api/sec/auth/second-step/verify "" "$(step "forged-challenge" "$(totp "$KEY")")"); check "a forged challenge is refused" "$(status "$r")" 401
r=$(call POST /api/sec/auth/second-step/verify "" "$(step "$C" "$(totp "$KEY")")"); check "the pending one still works" "$(status "$r")" 200
check "no authenticator key stored in clear in the sign-in log" "$(sql "select count(*) from \"SEC_SIGN_INS\" where \"FAILURE_TEXT\" like '%${KEY:0:8}%'")" 0

echo "3. Federation accounts sign in as before"
r=$(login admin 'Admin#Seed2026'); check "token straight away" "$(body "$r" | jq -r '.token != null and .secondStep == null')" true
A=$(body "$r" | jq -r .token)

echo "4. Wrong codes lock the account like wrong passwords"
r=$(login old.admin 'Old#Admin2026'); C=$(body "$r" | jq -r .secondStep.challenge)
r=$(call POST /api/sec/auth/second-step/setup "" "$(step "$C")"); AKEY=$(body "$r" | jq -r .sharedKey)
r=$(call POST /api/sec/auth/second-step/enable "" "$(step "$C" "$(totp "$AKEY")")"); CA=$(body "$r" | jq -r .login.token)
check "the company administrator set it up too" "$(status "$r")" 200
r=$(login old.admin 'Old#Admin2026'); C=$(body "$r" | jq -r .secondStep.challenge)
for n in 1 2 3 4 5; do call POST /api/sec/auth/second-step/verify "" "$(step "$C" 111111)" >/dev/null; done
r=$(call POST /api/sec/auth/second-step/verify "" "$(step "$C" "$(totp "$AKEY")")"); check "after 5 wrong codes even the right one is refused" "$(status "$r")" 401
check "saying the account is locked" "$(body "$r" | jq -r .detail)" "The account is locked. Try again later."
sql "update \"SEC_USERS\" set \"LOCKOUT_END_AT\" = null, \"ACCESS_FAILED_COUNT\" = 0 where \"USER_NAME\"='old.admin'" >/dev/null

echo "5. A lost phone: the administrator resets two-factor"
CID=$(call GET /api/scr/companies "$A" | cut -f2- | jq -r '.[] | select(.companyCode=="OLDCO") | .memberCompanyId')
U=$(call GET /api/scr/companies/$CID/users "$A" | cut -f2- | jq -c '[.[] | {key:.userName, value:.}] | from_entries')
check "the users list shows two-factor on" "$(jq -r '."old.u1".twoFactorEnabled' <<<"$U")" true
r=$(login old.u1 'Old#User2026'); C=$(body "$r" | jq -r .secondStep.challenge)
r=$(call POST /api/sec/auth/second-step/verify "" "$(step "$C" "$(totp "$KEY")")"); T=$(body "$r" | jq -r .login.token)
r=$(call POST /api/scr/companies/$CID/users/$(jq -r '."old.u1".userId' <<<"$U")/reset-two-factor "$CA"); check "the company administrator resets old.u1" "$(status "$r")" 200
check "two-factor off" "$(body "$r" | jq -r .twoFactorEnabled)" false
r=$(inquire "$T" 2); check "old.u1's open session ended" "$(status "$r")" 401
r=$(login old.u1 'Old#User2026'); check "next sign-in: set up again" "$(body "$r" | jq -r .secondStep.kind)" SETUP
C=$(body "$r" | jq -r .secondStep.challenge)
r=$(call POST /api/sec/auth/second-step/setup "" "$(step "$C")"); check "with a new key" "$([ "$(body "$r" | jq -r .sharedKey)" != "$KEY" ] && echo new || echo same)" new
r=$(call POST /api/scr/companies/$CID/users/$(jq -r '."old.admin".userId' <<<"$U")/reset-two-factor "$CA"); check "the administrator cannot reset its own" "$(status "$r")" 400
r=$(call POST /api/scr/companies/$CID/users/$(jq -r '."old.admin".userId' <<<"$U")/reset-two-factor "$A"); check "the Federation can" "$(status "$r")" 200
check "both resets are in the sign-in log" "$(sql "select count(*) from \"SEC_SIGN_INS\" where \"RESULT_CODE\"='TWO_FACTOR_RESET'")" 2
r=$(call GET "/api/scr/access-log?memberCompanyId=$CID&resultCode=REFUSED" "$A"); check "the log counts wrong codes as refusals" "$(body "$r" | jq -r '[.rows[] | select(.resultCode=="BAD_CODE")] | length >= 7')" true

echo
echo "Passed: $pass   Failed: $fail"
[ "$fail" -eq 0 ]
