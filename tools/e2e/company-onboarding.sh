#!/usr/bin/env bash
# End-to-end check of company onboarding (licence, administrator, activation links) against the real API, a real PostgreSQL
# database and a real mail server (Mailpit: SMTP on 1025, its API on 8025). Every e-mail is read and its link opened.
# Before this runs, develop made the database and a company LEGACY with one user (temporary password in /tmp/legacy.txt).
set -euo pipefail
API=${API:-http://localhost:5080}
MP=${MP:-http://localhost:8025}
pass=0; fail=0
ok()   { echo "  ✅ $1"; pass=$((pass+1)); }
bad()  { echo "  ❌ $1"; fail=$((fail+1)); }
check(){ if [ "$2" = "$3" ]; then ok "$1"; else bad "$1 (got [$2], want [$3])"; fi; }
# call METHOD PATH TOKEN [JSON] -> prints "status<TAB>body"
call() { local m=$1 p=$2 t=$3 d=${4:-}; local out; out=$(mktemp)
  local code; code=$(curl -s -o "$out" -w '%{http_code}' -X "$m" "$API$p" -H 'Content-Type: application/json' ${t:+-H "Authorization: Bearer $t"} ${d:+--data "$d"})
  printf '%s\t%s' "$code" "$(cat "$out" | tr '\n' ' ')"; rm -f "$out"; }
status(){ cut -f1 <<<"$1"; }
body()  { cut -f2- <<<"$1"; }
login() { call POST /api/sec/auth/login "" "{\"userName\":\"$1\",\"password\":\"$2\"}"; }
# mails TO -> how many e-mails Mailpit holds for that address
mails() { curl -s "$MP/api/v1/search?query=to:$1" | jq '.messages | length'; }
# link TO -> the link in the newest e-mail to that address (activate or reset-password)
link() { local id; id=$(curl -s "$MP/api/v1/search?query=to:$1" | jq -r '.messages[0].ID')
  curl -s "$MP/api/v1/message/$id" | jq -r .HTML | grep -o 'http[^"<]*/\(activate\|reset-password\)?user=[^"<]*' | head -1 | sed 's/&amp;/\&/g'; }
q() { python3 -c 'import sys,urllib.parse as u; print(u.parse_qs(u.urlsplit(sys.argv[1]).query)[sys.argv[2]][0])' "$1" "$2"; }
# linkjson LINK [PASSWORD] -> the JSON body the link pages send
linkjson() { jq -nc --arg u "$(q "$1" user)" --arg t "$(q "$1" token)" --arg p "${2:-}" '{userId:$u, token:$t} + (if $p == "" then {} else {newPassword:$p} end)'; }

r=$(login admin 'Admin#Seed2026'); check "the Federation administrator signs in" "$(status "$r")" 200
ADMIN=$(body "$r" | jq -r .token)
company() { call GET /api/scr/companies "$ADMIN" | cut -f2- | jq -c --arg c "$1" '.[] | select(.companyCode == $c)'; }

echo "1. A company made before the upgrade keeps working"
L=$(company LEGACY); LID=$(jq -r .memberCompanyId <<<"$L")
check "it got an opening licence" "$(jq -r .licenceStateCode <<<"$L")" ACTIVE
check "for the users it had (1)" "$(jq -r .licence.maxUsers <<<"$L")" 1
check "open-ended" "$(jq -r .licence.validTo <<<"$L")" null
check "it has no administrator yet" "$(jq -r .seats.adminUserName <<<"$L")" null
r=$(login legacy.user "$(cat /tmp/legacy.txt)"); check "its user still signs in" "$(status "$r")" 200
check "and must still replace the temporary password" "$(body "$r" | jq -r .mustChangePassword)" true
r=$(call POST /api/scr/companies/$LID/users "$ADMIN" '{"userName":"legacy.admin","email":"legacy.admin@e2e.test","fullName":"Legacy Admin","isActive":true}')
check "the Federation adds a user to it" "$(status "$r")" 200
check "who becomes its administrator (no seat used)" "$(body "$r" | jq -r .user.roleCode)" MEMBER_COMPANY_ADMIN
check "and is e-mailed" "$(body "$r" | jq -r .email.sent)" true
r=$(call POST /api/scr/companies/$LID/users "$ADMIN" '{"userName":"legacy.two","email":"legacy.two@e2e.test","fullName":"Legacy Two","isActive":true}')
check "the next user needs a seat: refused (licence 1, in use 1)" "$(status "$r")" 400

echo "2. Onboarding: company + licence (admin + 1 user) + administrator, in one step"
r=$(call POST /api/scr/companies "$ADMIN" '{"company":{"companyCode":"ACME","nameEn":"Acme Finance","nameAr":"أكمي للتمويل","statusCode":"ACTIVE"},
  "licence":{"maxUsers":1,"validFrom":"2026-01-01","validTo":null,"yearlyInquiryQuota":null},
  "admin":{"userName":"acme.admin","email":"acme.admin@e2e.test","fullName":"Acme Admin"}}')
check "created" "$(status "$r")" 200
CID=$(body "$r" | jq -r .company.memberCompanyId); AID=$(body "$r" | jq -r .admin.userId)
check "activation e-mail sent" "$(body "$r" | jq -r .email.sent)" true
check "administrator pending activation" "$(body "$r" | jq -r .admin.statusCode)" PENDING_ACTIVATION
check "no password anywhere in the answer" "$(body "$r" | grep -ci password || true)" 0
r=$(call POST /api/scr/companies "$ADMIN" '{"company":{"companyCode":"BAD","nameEn":"Bad","nameAr":"سيئ","statusCode":"ACTIVE"},"licence":{"maxUsers":1,"validFrom":"2026-01-01"},"admin":{"userName":"bad.admin","email":"not-an-email","fullName":"Bad"}}')
check "an administrator without a valid e-mail is refused" "$(status "$r")" 400
check "and nothing was made" "$(company BAD)" ""
r=$(login acme.admin 'Anything#123'); check "a pending account cannot sign in" "$(status "$r")" 401

echo "3. The administrator activates the account from the e-mail"
check "one e-mail arrived" "$(mails acme.admin@e2e.test)" 1
L1=$(link acme.admin@e2e.test)
[[ "$L1" == *"/activate?user="* ]] && ok "it carries an activation link" || bad "no activation link: [$L1]"
r=$(call POST /api/sec/auth/link/check "" "$(linkjson "$L1")"); check "the link is valid" "$(status "$r")" 200
check "for acme.admin, activating" "$(body "$r" | jq -r '.userName + " " + (.activating|tostring)')" "acme.admin true"
r=$(call POST /api/sec/auth/link/check "" "$(jq -nc --arg u "$AID" '{userId:$u, token:"forged"}')"); check "a forged token is refused" "$(status "$r")" 400
r=$(call POST /api/sec/auth/link/set-password "" "$(linkjson "$L1" weak)"); check "a weak password is refused" "$(status "$r")" 400
r=$(call POST /api/sec/auth/link/set-password "" "$(linkjson "$L1" 'Acme#Admin2026')"); check "own password set" "$(status "$r")" 204
r=$(call POST /api/sec/auth/link/set-password "" "$(linkjson "$L1" 'Other#Pass2026')"); check "the link works only once" "$(status "$r")" 400
r=$(login acme.admin 'Acme#Admin2026'); check "the administrator signs in" "$(status "$r")" 200
check "not asked to change it" "$(body "$r" | jq -r .mustChangePassword)" false
CA=$(body "$r" | jq -r .token)
r=$(call GET /api/scr/companies/$LID/users "$CA"); check "but not into another company's users" "$(status "$r")" 403

echo "4. The company administrator adds users, within the licence"
r=$(call POST /api/scr/companies/$CID/users "$CA" '{"userName":"acme.u1","email":"acme.u1@e2e.test","fullName":"Acme One","isActive":true}')
check "first user created" "$(status "$r")" 200
U1=$(body "$r" | jq -r .user.userId)
check "as a company user" "$(body "$r" | jq -r .user.roleCode)" MEMBER_COMPANY_USER
check "e-mailed" "$(body "$r" | jq -r .email.sent)" true
r=$(call POST /api/scr/companies/$CID/users "$CA" '{"userName":"acme.u2","email":"acme.u2@e2e.test","fullName":"Acme Two","isActive":true}')
check "a second user is refused: the pending one holds the seat" "$(status "$r")" 400
r=$(call POST /api/scr/companies/$CID/users "$CA" '{"userName":"acme.u3","email":"acme.u1@e2e.test","fullName":"Acme Three","isActive":true}')
check "an e-mail already used is refused" "$(status "$r")" 400
r=$(call PUT /api/scr/companies/$CID/users/$AID "$CA" '{"userName":"acme.admin","email":"acme.admin@e2e.test","fullName":"Acme Admin","isActive":false}')
check "the administrator cannot be deactivated" "$(status "$r")" 400
r=$(call POST /api/scr/companies/$CID/licences "$ADMIN" '{"maxUsers":0,"validFrom":"2026-01-01"}')
check "the licence cannot drop below the users in use" "$(status "$r")" 400

echo "5. Copy link (when the e-mail does not arrive) voids the e-mailed one"
M1=$(link acme.u1@e2e.test)
r=$(call POST /api/scr/companies/$CID/users/$U1/copy-link "$CA"); check "link made" "$(status "$r")" 200
check "an activation link" "$(body "$r" | jq -r .kind)" ACTIVATION
C1=$(body "$r" | jq -r .link)
r=$(call POST /api/sec/auth/link/check "" "$(linkjson "$M1")"); check "the e-mailed link stopped working" "$(status "$r")" 400
r=$(call POST /api/sec/auth/link/set-password "" "$(linkjson "$C1" 'Acme#One2026')"); check "the copied link activates" "$(status "$r")" 204
r=$(login acme.u1 'Acme#One2026'); check "the user signs in" "$(status "$r")" 200
T1=$(body "$r" | jq -r .token)
r=$(call GET /api/scr/inquiries/mine "$T1"); check "and can screen" "$(status "$r")" 200

echo "6. Forgot password"
r=$(call POST /api/sec/auth/forgot-password "" '{"userNameOrEmail":"nobody.here"}'); A=$(body "$r")
r=$(call POST /api/sec/auth/forgot-password "" '{"userNameOrEmail":"acme.u1@e2e.test"}'); check "answered" "$(status "$r")" 200
check "the same answer whether the account exists or not" "$(body "$r")" "$A"
R1=$(link acme.u1@e2e.test)
[[ "$R1" == *"/reset-password?user="* ]] && ok "a reset link arrived" || bad "no reset link: [$R1]"
r=$(login acme.u1 'Acme#One2026'); check "the current password works until the link is used" "$(status "$r")" 200
r=$(call POST /api/sec/auth/link/set-password "" "$(linkjson "$R1" 'Acme#New2026')"); check "new password set" "$(status "$r")" 204
r=$(login acme.u1 'Acme#One2026'); check "the old password no longer works" "$(status "$r")" 401
r=$(call GET /api/scr/inquiries/mine "$T1"); check "the open session ended" "$(status "$r")" 401
r=$(login acme.u1 'Acme#New2026'); check "the new password works" "$(status "$r")" 200
T1=$(body "$r" | jq -r .token)

echo "7. An expired licence blocks sign-in and ends sessions at once"
r=$(call POST /api/scr/companies/$CID/licences "$ADMIN" '{"maxUsers":1,"validFrom":"2025-01-01","validTo":"2025-12-31"}')
check "licence replaced by an expired one" "$(status "$r")" 200
r=$(call GET /api/scr/inquiries/mine "$T1"); check "the open session ended" "$(status "$r")" 401
r=$(login acme.u1 'Acme#New2026'); check "sign-in refused" "$(status "$r")" 401
check "saying why" "$(body "$r" | jq -r .detail)" "Your company's licence has ended. Contact the Federation to renew it."
r=$(call POST /api/scr/companies/$CID/licences "$ADMIN" '{"maxUsers":2,"validFrom":"2026-01-01","validTo":null}'); check "renewed (admin + 2)" "$(status "$r")" 200
r=$(login acme.u1 'Acme#New2026'); check "signs in again" "$(status "$r")" 200
r=$(call GET "/api/scr/companies/$CID/licences" "$ADMIN"); check "the licence history is kept (3)" "$(body "$r" | jq length)" 3

echo "8. A suspended company is blocked"
C=$(company ACME)
r=$(call PUT /api/scr/companies/$CID "$ADMIN" "$(jq -c '{companyCode,nameEn,nameAr,licenceNo,contactEmail,notes,statusCode:"SUSPENDED"}' <<<"$C")"); check "suspended" "$(status "$r")" 200
r=$(call GET /api/scr/companies/$CID/users "$CA"); check "the administrator's session ended" "$(status "$r")" 401
r=$(login acme.admin 'Acme#Admin2026'); check "sign-in refused" "$(status "$r")" 401
check "saying why" "$(body "$r" | jq -r .detail)" "Your company's access is suspended. Contact the Federation."
r=$(call PUT /api/scr/companies/$CID "$ADMIN" "$(jq -c '{companyCode,nameEn,nameAr,licenceNo,contactEmail,notes,statusCode:"ACTIVE"}' <<<"$C")"); check "reactivated" "$(status "$r")" 200
r=$(login acme.admin 'Acme#Admin2026'); check "the administrator signs in again" "$(status "$r")" 200
CA=$(body "$r" | jq -r .token)

echo "9. Seats follow the users"
r=$(call POST /api/scr/companies/$CID/users "$CA" '{"userName":"acme.u2","email":"acme.u2@e2e.test","fullName":"Acme Two","isActive":true}')
check "with admin + 2, a second user fits" "$(status "$r")" 200
U2=$(body "$r" | jq -r .user.userId)
r=$(call PUT /api/scr/companies/$CID/users/$U1 "$CA" '{"userName":"acme.u1","email":"acme.u1@e2e.test","fullName":"Acme One","isActive":false}')
check "a user deactivated" "$(status "$r")" 200
r=$(login acme.u1 'Acme#New2026'); check "who can no longer sign in" "$(status "$r")" 401
check "ACME shows 1 user in use" "$(company ACME | jq -r .seats.usersInUse)" 1
r=$(call POST /api/scr/companies/$CID/users/$U2/send-link "$CA"); check "invitation re-sent" "$(body "$r" | jq -r .sent)" true
check "u2 now has two e-mails" "$(mails acme.u2@e2e.test)" 2

echo "10. The e-mail log, and no password ever e-mailed"
ids=$(curl -s "$MP/api/v1/messages?limit=200" | jq -r '.messages[].ID')
leak=0; for id in $ids; do curl -s "$MP/api/v1/message/$id" | jq -r .HTML | grep -q -e 'Acme#' -e "$(cat /tmp/legacy.txt)" && leak=1; done
check "no e-mail contains a password" "$leak" 0
n=$(docker exec "$PG" psql -U postgres -d capitra -tAc "select count(*) from \"CMC_EMAIL_LOGS\" where \"STATUS_CODE\"='SENT'")
check "every sent e-mail is in the log ($(echo $ids | wc -w))" "$n" "$(echo $ids | wc -w)"
n=$(docker exec "$PG" psql -U postgres -d capitra -tAc "select count(*) from \"CMC_EMAIL_LOGS\" where \"STATUS_CODE\"='LINK_COPIED'")
check "the copied link is in the log" "$n" 1

echo
echo "Passed: $pass   Failed: $fail"
[ "$fail" -eq 0 ]
