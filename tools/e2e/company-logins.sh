#!/usr/bin/env bash
# End-to-end check of company logins, step 1, against the real API and a real PostgreSQL database.
set -euo pipefail
API=${API:-http://localhost:5080}
pass=0; fail=0
ok()   { echo "  ✅ $1"; pass=$((pass+1)); }
bad()  { echo "  ❌ $1"; fail=$((fail+1)); }
check(){ if [ "$2" = "$3" ]; then ok "$1"; else bad "$1 (got [$2], want [$3])"; fi; }
# call METHOD PATH TOKEN [JSON] -> prints "status<TAB>body"
call() { local m=$1 p=$2 t=$3 d=${4:-}; local out; out=$(mktemp)
  local code; code=$(curl -s -o "$out" -w '%{http_code}' -X "$m" "$API$p" -H 'Content-Type: application/json' ${t:+-H "Authorization: Bearer $t"} ${d:+--data "$d"})
  printf '%s\t%s' "$code" "$(cat "$out")"; rm -f "$out"; }
status(){ cut -f1 <<<"$1"; }
body()  { cut -f2- <<<"$1"; }
login() { call POST /api/sec/auth/login "" "{\"userName\":\"$1\",\"password\":\"$2\"}"; }

echo "1. The administrator who existed before the upgrade is not asked to change password"
r=$(login admin 'Admin#Seed2026'); check "admin signs in" "$(status "$r")" 200
check "admin: no forced change" "$(body "$r" | jq -r .mustChangePassword)" false
ADMIN=$(body "$r" | jq -r .token)

echo "2. The Federation creates a member company and a company user (no password typed)"
r=$(call POST /api/scr/companies "$ADMIN" '{"companyCode":"ACME","nameEn":"Acme Finance","nameAr":"أكمي للتمويل","statusCode":"ACTIVE"}')
check "company created" "$(status "$r")" 200
CID=$(body "$r" | jq -r .memberCompanyId)
r=$(call POST /api/scr/companies/$CID/users "$ADMIN" '{"userName":"acme.user","fullName":"Acme User","roleCode":"MEMBER_COMPANY_USER","isActive":true}')
check "company user created" "$(status "$r")" 200
TEMP=$(body "$r" | jq -r .temporaryPassword); UID_=$(body "$r" | jq -r .userId)
[ ${#TEMP} -eq 14 ] && ok "a 14-character temporary password is returned once" || bad "temporary password missing: [$TEMP]"
check "flagged: must change password" "$(body "$r" | jq -r .mustChangePassword)" true
r=$(call GET /api/scr/companies/$CID/users "$ADMIN")
check "the user list never shows the password" "$(body "$r" | jq -r '.[0].temporaryPassword')" null

echo "3. First sign-in with the temporary password: only changing it is allowed"
r=$(login acme.user "$TEMP"); check "signs in with the temporary password" "$(status "$r")" 200
check "told to change password" "$(body "$r" | jq -r .mustChangePassword)" true
TT=$(body "$r" | jq -r .token)
r=$(call GET /api/scr/inquiries/mine "$TT"); check "screening refused" "$(status "$r")" 403
check "refusal says why" "$(body "$r" | jq -r .code)" PASSWORD_CHANGE_REQUIRED
r=$(call POST /api/scr/inquiries "$TT" '{"subjectTypeCode":"PERSON","subjectName":"Test","identityTypeCode":"NATIONAL_ID","identityNo":"29001011234567","purposeCode":"ONBOARDING"}')
check "an inquiry cannot be sent" "$(status "$r")" 403

echo "4. Change password: the rules are enforced"
r=$(call POST /api/sec/auth/change-password "$TT" "{\"currentPassword\":\"Wrong#Pass123\",\"newPassword\":\"Acme#Mine2026\"}")
check "wrong current password refused" "$(status "$r")" 400
r=$(call POST /api/sec/auth/change-password "$TT" "{\"currentPassword\":\"$TEMP\",\"newPassword\":\"$TEMP\"}")
check "same password refused" "$(status "$r")" 400
r=$(call POST /api/sec/auth/change-password "$TT" "{\"currentPassword\":\"$TEMP\",\"newPassword\":\"weak\"}")
check "weak password refused" "$(status "$r")" 400
r=$(call POST /api/sec/auth/change-password "$TT" "{\"currentPassword\":\"$TEMP\",\"newPassword\":\"Acme#Mine2026\"}")
check "own password accepted" "$(status "$r")" 200
check "no longer forced" "$(body "$r" | jq -r .mustChangePassword)" false
NT=$(body "$r" | jq -r .token)

echo "5. After the change"
r=$(call GET /api/scr/inquiries/mine "$TT"); check "the old (temporary) session no longer works" "$(status "$r")" 401
r=$(call GET /api/scr/inquiries/mine "$NT"); check "the new session can screen" "$(status "$r")" 200
r=$(login acme.user "$TEMP"); check "the temporary password no longer signs in" "$(status "$r")" 401
r=$(login acme.user 'Acme#Mine2026'); check "the user's own password signs in" "$(status "$r")" 200
check "and is not forced to change" "$(body "$r" | jq -r .mustChangePassword)" false

echo "6. The administrator resets the password"
r=$(call POST /api/scr/companies/$CID/users/$UID_/reset-password "$ADMIN"); check "reset done" "$(status "$r")" 200
TEMP2=$(body "$r" | jq -r .temporaryPassword)
[ ${#TEMP2} -eq 14 ] && [ "$TEMP2" != "$TEMP" ] && ok "a new temporary password is returned" || bad "no new temporary password"
r=$(call GET /api/scr/inquiries/mine "$NT"); check "the user's open session is ended" "$(status "$r")" 401
r=$(login acme.user 'Acme#Mine2026'); check "the old own password no longer signs in" "$(status "$r")" 401
r=$(login acme.user "$TEMP2"); check "the new temporary password signs in" "$(status "$r")" 200
check "and must be changed again" "$(body "$r" | jq -r .mustChangePassword)" true

echo "7. Lock-out still protects the account"
for i in 1 2 3 4 5; do login acme.user 'Nope#Wrong123' >/dev/null; done
r=$(login acme.user "$TEMP2"); check "locked after 5 wrong passwords" "$(status "$r")" 401
check "says it is locked" "$(body "$r" | jq -r .detail | grep -c locked)" 1

echo "8. Deactivating the user ends their session at once"
r=$(call POST /api/scr/companies/$CID/users/$UID_/reset-password "$ADMIN"); T3=$(body "$r" | jq -r .temporaryPassword)
r=$(login acme.user "$T3"); TT3=$(body "$r" | jq -r .token)
r=$(call POST /api/sec/auth/change-password "$TT3" "{\"currentPassword\":\"$T3\",\"newPassword\":\"Acme#Again2026\"}"); S3=$(body "$r" | jq -r .token)
r=$(call GET /api/scr/inquiries/mine "$S3"); check "active user screens" "$(status "$r")" 200
r=$(call PUT /api/scr/companies/$CID/users/$UID_ "$ADMIN" '{"userName":"acme.user","fullName":"Acme User","roleCode":"MEMBER_COMPANY_USER","isActive":false}')
check "user deactivated" "$(status "$r")" 200
r=$(call GET /api/scr/inquiries/mine "$S3"); check "their session ends at once" "$(status "$r")" 401
r=$(login acme.user 'Acme#Again2026'); check "and they cannot sign in" "$(status "$r")" 401

echo; echo "Passed $pass, failed $fail"; [ $fail -eq 0 ]
