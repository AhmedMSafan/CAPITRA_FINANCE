#!/usr/bin/env bash
# End-to-end check of the Screening API v1 against the real API and a real PostgreSQL database. Before this runs, develop made the database
# with company OLDCO (old.u1 made 2 portal inquiries).
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

ADMIN=$(login admin 'Admin#Seed2026')
CID=$(call GET /api/scr/companies "$ADMIN" | cut -f2- | jq -r '.[] | select(.companyCode == "OLDCO") | .memberCompanyId')

echo "1. The Federation creates an API client: the secret is shown once"
r=$(call POST /api/scr/companies/$CID/api-clients "$ADMIN" '{"clientName":"Core banking onboarding"}')
check "created" "$(status "$r")" 200
CLIENT=$(body "$r" | jq -r .client.clientId); SECRET=$(body "$r" | jq -r .clientSecret)
check "client ID made from the company" "$(grep -c '^scr_oldco_' <<<"$CLIENT")" 1
check "secret shown" "$(grep -c '^scs_' <<<"$SECRET")" 1
check "only a hash is kept" "$(sql "select (\"SECRET_HASH\" like '100000.%') and position('$SECRET' in \"SECRET_HASH\") = 0 from \"SCR_API_CLIENTS\" where \"CLIENT_ID\"='$CLIENT'")" t
r=$(call GET /api/scr/companies/$CID/api-clients "$ADMIN")
check "listed, without the secret" "$(body "$r" | jq -r '.[0] | .statusCode + " " + (has("clientSecret")|tostring) + " " + .secretHint')" "ACTIVE false ${SECRET: -4}"
r=$(call POST /api/scr/companies/$CID/api-clients "$ADMIN" '{"clientName":""}'); check "a name is needed" "$(status "$r")" 400

echo "2. Token"
r=$(call POST /api/scr/v1/token "" "{\"clientId\":\"$CLIENT\",\"clientSecret\":\"wrong\"}")
check "wrong secret refused" "$(status "$r") $(body "$r" | jq -r .error)" "401 invalid_client"
r=$(call POST /api/scr/v1/token "" '{"clientId":"scr_nobody_x","clientSecret":"x"}'); check "unknown client refused, same answer" "$(status "$r") $(body "$r" | jq -r .error)" "401 invalid_client"
r=$(call POST /api/scr/v1/token "" "{\"clientId\":\"$CLIENT\",\"clientSecret\":\"$SECRET\"}")
check "token given" "$(status "$r") $(body "$r" | jq -r '.tokenType + " " + (.expiresIn|tostring)')" "200 Bearer 3600"
TOKEN=$(body "$r" | jq -r .accessToken)
check "last token time kept" "$(sql "select \"LAST_TOKEN_AT\" is not null from \"SCR_API_CLIENTS\" where \"CLIENT_ID\"='$CLIENT'")" t

echo "3. Screening"
r=$(call POST /api/scr/v1/inquiries "$TOKEN" '{"subjectTypeCode":"PERSON","subjectName":"Unknown Person","identityTypeCode":"NATIONAL_ID","identityNo":"29001011234567","purposeCode":"ONBOARDING","caseReference":"CB-77"}')
check "screened" "$(status "$r")" 200
REF=$(body "$r" | jq -r .inquiryRef)
check "the API's answer" "$(body "$r" | jq -r '.result + " " + .channel + " " + .caseReference + " " + (.hits|length|tostring)')" "NO_MATCH API CB-77 0"
check "timings kept" "$(body "$r" | jq -r '(.receivedAt != null) and (.respondedAt != null)')" true
check "kept with its channel and client" "$(sql "select \"CHANNEL_CODE\" || ' ' || \"REQUESTED_BY\" || ' ' || (\"API_CLIENT_ID\" is not null) from \"SCR_INQUIRIES\" where \"INQUIRY_REF\"='$REF'")" "API api:$CLIENT true"
sql "insert into \"SCR_INQUIRIES\" (\"INQUIRY_REF\",\"MEMBER_COMPANY_ID\",\"REQUESTED_BY\",\"SUBJECT_TYPE_CODE\",\"SUBJECT_NAME\",\"IDENTITY_TYPE_CODE\",\"IDENTITY_NO\",\"PURPOSE_CODE\",\"RESULT_CODE\",\"REGISTRY_VERSION_NO\",\"SCREENED_AT\",\"CREATED_BY\",\"CREATED_AT\",\"ROW_VERSION\") values ('SCR-2026-900001',$CID,'old.u1','PERSON','Portal Person','NATIONAL_ID','29001011234568','ONBOARDING','NO_MATCH',0,now() - interval '1 minute','E2E',now(),0)" >/dev/null
check "an inquiry saved without a channel is a portal one" "$(sql "select \"CHANNEL_CODE\" from \"SCR_INQUIRIES\" where \"INQUIRY_REF\"='SCR-2026-900001'")" PORTAL
r=$(call POST /api/scr/v1/inquiries "$TOKEN" '{"subjectTypeCode":"PERSON"}'); check "checked like the portal" "$(status "$r")" 400
r=$(call GET /api/scr/v1/inquiries/$REF "$TOKEN"); check "read by reference" "$(status "$r") $(body "$r" | jq -r .inquiryRef)" "200 $REF"
r=$(call GET /api/scr/v1/inquiries/$(echo "$REF" | tr 'A-Z' 'a-z') "$TOKEN"); check "reference in any case" "$(status "$r")" 200
r=$(call GET /api/scr/v1/inquiries/SCR-1999-000001 "$TOKEN"); check "unknown reference" "$(status "$r") $(body "$r" | jq -r .error)" "404 not_found"
r=$(call GET "/api/scr/v1/inquiries?take=2" "$TOKEN"); check "list: the company's inquiries, newest first" "$(status "$r") $(body "$r" | jq -r '(length|tostring) + " " + .[0].inquiryRef')" "200 2 SCR-2026-900001"
TODAY=$(TZ=Africa/Cairo date +%F)
r=$(call GET "/api/scr/v1/inquiries?from=$TODAY&to=$TODAY" "$TOKEN"); check "list by days: API and portal" "$(body "$r" | jq -r '[.[].channel] | sort | join(",")')" "API,PORTAL"

echo "4. Tokens stay in their lane"
r=$(call GET /api/scr/v1/inquiries "$ADMIN"); check "a user token does not open the API" "$(status "$r")" 401
r=$(call GET /api/scr/inquiries/mine "$TOKEN"); check "an API token does not open CAPITRA" "$(status "$r")" 401
r=$(call GET /api/scr/v1/inquiries ""); check "no token" "$(status "$r")" 401

echo "5. Billing counts API inquiries (no user fee)"
r=$(call POST /api/scr/billing/plans "$ADMIN" '{"planCode":"APIPLAN","nameEn":"API","nameAr":"خطة","userMonthlyFee":100,"inquiryFee":2.5,"isActive":true}'); PID=$(body "$r" | jq -r .pricePlanId)
OLD=$(call GET /api/scr/companies "$ADMIN" | cut -f2- | jq -c '.[] | select(.companyCode == "OLDCO")')
r=$(call PUT /api/scr/companies/$CID "$ADMIN" "$(jq -c --argjson p "$PID" '{companyCode,nameEn,nameAr,licenceNo,contactEmail,statusCode,notes,pricePlanId:$p,billingCycleCode:"MONTHLY"}' <<<"$OLD")")
r=$(call GET "/api/scr/billing/companies/$CID/preview?periodFrom=$(TZ=Africa/Cairo date +%Y-%m-01)" "$ADMIN")
check "the client's line: 1 inquiry, no user months" "$(body "$r" | jq -r --arg u "api:$CLIENT" '[.lines[] | select(.userName == $u) | ((.inquiryCount|tostring) + " " + (.userMonths|tostring))] | join(",")')" "1 0"

echo "6. New secret, revoke, blocked company"
r=$(call POST /api/scr/companies/$CID/api-clients/$(sql "select \"API_CLIENT_ID\" from \"SCR_API_CLIENTS\" where \"CLIENT_ID\"='$CLIENT'")/secret "$ADMIN" '{}')
NEW=$(body "$r" | jq -r .clientSecret); check "new secret" "$(grep -c '^scs_' <<<"$NEW")" 1
r=$(call POST /api/scr/v1/token "" "{\"clientId\":\"$CLIENT\",\"clientSecret\":\"$SECRET\"}"); check "the old secret stops" "$(status "$r")" 401
r=$(call POST /api/scr/v1/token "" "{\"clientId\":\"$CLIENT\",\"clientSecret\":\"$NEW\"}"); check "the new one works" "$(status "$r")" 200
sql "update \"SCR_COMPANY_LICENCES\" set \"VALID_TO\"=current_date - 2 where \"MEMBER_COMPANY_ID\"=$CID" >/dev/null
r=$(call POST /api/scr/v1/token "" "{\"clientId\":\"$CLIENT\",\"clientSecret\":\"$NEW\"}"); check "licence ended: no token" "$(status "$r") $(body "$r" | jq -r .error)" "403 company_blocked"
sleep 31
r=$(call GET /api/scr/v1/inquiries "$TOKEN"); check "and the token in hand stops" "$(status "$r")" 401
sql "update \"SCR_COMPANY_LICENCES\" set \"VALID_TO\"=null where \"MEMBER_COMPANY_ID\"=$CID" >/dev/null
sleep 31
r=$(call GET /api/scr/v1/inquiries "$TOKEN"); check "licence back: the token works again" "$(status "$r")" 200
AID=$(sql "select \"API_CLIENT_ID\" from \"SCR_API_CLIENTS\" where \"CLIENT_ID\"='$CLIENT'")
r=$(call POST /api/scr/companies/$CID/api-clients/$AID/revoke "$ADMIN" '{}'); check "revoked" "$(body "$r" | jq -r .statusCode)" REVOKED
r=$(call GET /api/scr/v1/inquiries "$TOKEN"); check "its token stops at once" "$(status "$r")" 401
r=$(call POST /api/scr/v1/token "" "{\"clientId\":\"$CLIENT\",\"clientSecret\":\"$NEW\"}"); check "no new token" "$(status "$r")" 401
r=$(call POST /api/scr/companies/$CID/api-clients/$AID/revoke "$ADMIN" '{}'); check "revoked once" "$(status "$r")" 400
r=$(call POST /api/scr/companies/999/api-clients/$AID/secret "$ADMIN" '{}'); check "another company's client" "$(status "$r")" 404
check "kept with its inquiry" "$(sql "select count(*) from \"SCR_INQUIRIES\" where \"API_CLIENT_ID\"=$AID")" 1

echo; echo "Passed: $pass  Failed: $fail"
[ "$fail" -eq 0 ]
