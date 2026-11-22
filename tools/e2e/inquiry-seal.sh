#!/usr/bin/env bash
# End-to-end check of inquiry seals: each new inquiry is sealed and chained, the database refuses changes and deletions, and the seal
# checks find a change made behind the application's back. Before this runs, develop made the database with company OLDCO.
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
sqlerr(){ docker exec "$PG" psql -U postgres -d capitra -tAc "$1" 2>&1 | grep -o 'ERROR:.*' | head -1; }
screen(){ call POST /api/scr/v1/inquiries "$TOKEN" "{\"subjectTypeCode\":\"PERSON\",\"subjectName\":\"Seal Person $1\",\"identityTypeCode\":\"NATIONAL_ID\",\"identityNo\":\"2900101123457$1\",\"purposeCode\":\"ONBOARDING\"}"; }

ADMIN=$(login admin 'Admin#Seed2026')
CID=$(call GET /api/scr/companies "$ADMIN" | cut -f2- | jq -r '.[] | select(.companyCode == "OLDCO") | .memberCompanyId')
r=$(call POST /api/scr/companies/$CID/api-clients "$ADMIN" '{"clientName":"Seal test"}')
TOKEN=$(call POST /api/scr/v1/token "" "$(body "$r" | jq -c '{clientId: .client.clientId, clientSecret: .clientSecret}')" | cut -f2- | jq -r .accessToken)
OLD=$(sql "select count(*) from \"SCR_INQUIRIES\"")

echo "1. New inquiries are sealed and chained"
r=$(screen 1); A=$(body "$r"); AREF=$(jq -r .inquiryRef <<<"$A"); AHASH=$(jq -r .recordHash <<<"$A")
check "the API answer carries the seal" "$(grep -cE '^[0-9a-f]{64}$' <<<"$AHASH")" 1
check "the first sealed one has nothing before it" "$(sql "select coalesce(\"PREV_HASH\", 'none') from \"SCR_INQUIRIES\" where \"INQUIRY_REF\"='$AREF'")" none
r=$(screen 2); BREF=$(body "$r" | jq -r .inquiryRef)
check "the next one is chained to it" "$(sql "select \"PREV_HASH\" from \"SCR_INQUIRIES\" where \"INQUIRY_REF\"='$BREF'")" "$AHASH"
check "inquiries made before have no seal" "$(sql "select count(*) from \"SCR_INQUIRIES\" where \"RECORD_HASH\" is null")" "$OLD"
AID=$(sql "select \"INQUIRY_ID\" from \"SCR_INQUIRIES\" where \"INQUIRY_REF\"='$AREF'")
r=$(call GET /api/scr/inquiries/$AID/seal "$ADMIN"); check "the seal checks out" "$(body "$r" | jq -r '(.sealed|tostring) + " " + (.valid|tostring)')" "true true"
r=$(call GET /api/scr/inquiries/seal-check "$ADMIN")
check "the whole chain checks out" "$(body "$r" | jq -r '(.checked|tostring) + " " + (.broken|tostring) + " " + (.unsealedBeforeStart|tostring) + " " + .firstSealedRef + " " + .lastSealedRef')" "2 0 $OLD $AREF $BREF"

echo "2. The database refuses changes and deletions"
check "a sealed inquiry cannot be changed" "$(sqlerr "update \"SCR_INQUIRIES\" set \"RESULT_CODE\"='MATCH' where \"INQUIRY_REF\"='$AREF'" | grep -c 'is sealed and cannot be changed')" 1
check "no inquiry can be deleted" "$(sqlerr "delete from \"SCR_INQUIRIES\" where \"INQUIRY_REF\"='$AREF'" | grep -c 'cannot be deleted')" 1
if [ "$OLD" -gt 0 ]; then
  check "not even an old one" "$(sqlerr "delete from \"SCR_INQUIRIES\" where \"RECORD_HASH\" is null" | grep -c 'cannot be deleted')" 1
fi
check "still as made" "$(sql "select \"RESULT_CODE\" from \"SCR_INQUIRIES\" where \"INQUIRY_REF\"='$AREF'")" NO_MATCH

echo "3. A change made behind the application's back shows"
sql "alter table \"SCR_INQUIRIES\" disable trigger \"TRG_SCR_INQUIRIES_GUARD\"; update \"SCR_INQUIRIES\" set \"RESULT_CODE\"='MATCH' where \"INQUIRY_REF\"='$AREF'; alter table \"SCR_INQUIRIES\" enable trigger \"TRG_SCR_INQUIRIES_GUARD\"" >/dev/null
r=$(call GET /api/scr/inquiries/$AID/seal "$ADMIN"); check "its seal no longer matches" "$(body "$r" | jq -r '(.valid|tostring) + " " + .message')" "false The seal does not match: the inquiry or the chain was changed."
r=$(call GET /api/scr/inquiries/seal-check "$ADMIN"); check "the chain check names it" "$(body "$r" | jq -r '(.broken|tostring) + " " + (.brokenRefs|join(","))')" "1 $AREF"
sql "insert into \"SCR_INQUIRIES\" (\"INQUIRY_REF\",\"MEMBER_COMPANY_ID\",\"REQUESTED_BY\",\"SUBJECT_TYPE_CODE\",\"SUBJECT_NAME\",\"IDENTITY_TYPE_CODE\",\"IDENTITY_NO\",\"PURPOSE_CODE\",\"RESULT_CODE\",\"REGISTRY_VERSION_NO\",\"SCREENED_AT\",\"CREATED_BY\",\"CREATED_AT\",\"ROW_VERSION\") values ('SCR-2026-900002',$CID,'x','PERSON','Slipped In','NATIONAL_ID','29001011234599','ONBOARDING','NO_MATCH',0,now(),'E2E',now(),0)" >/dev/null
r=$(call GET /api/scr/inquiries/seal-check "$ADMIN"); check "a row slipped in without a seal shows" "$(body "$r" | jq -r '(.unsealedAfterStart|tostring) + " " + (.unsealedRefs|join(","))')" "1 SCR-2026-900002"
r=$(screen 3); CREF=$(body "$r" | jq -r .inquiryRef)
CID3=$(sql "select \"INQUIRY_ID\" from \"SCR_INQUIRIES\" where \"INQUIRY_REF\"='$CREF'")
r=$(call GET /api/scr/inquiries/$CID3/seal "$ADMIN"); check "new inquiries chain on and check out" "$(body "$r" | jq -r '.valid')" true

echo; echo "Passed: $pass  Failed: $fail"
[ "$fail" -eq 0 ]
