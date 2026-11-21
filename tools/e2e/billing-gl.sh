#!/usr/bin/env bash
# End-to-end check of screening billing's GL on a database without a chart of accounts (a new test database): the migration adds the
# product but no GL rules, so an invoice with an amount is refused with the reason and nothing is saved; an invoice of 0.00 needs no GL.
# Before this runs, develop made the database with company OLDCO (old.u1 made 2 inquiries).
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
THIS_MONTH=$(TZ=Africa/Cairo date +%Y-%m-01)
TODAY=$(TZ=Africa/Cairo date +%F)

ADMIN=$(login admin 'Admin#Seed2026')
OLD=$(call GET /api/scr/companies "$ADMIN" | cut -f2- | jq -c '.[] | select(.companyCode == "OLDCO")'); CID=$(jq -r .memberCompanyId <<<"$OLD")

echo "1. The migration: the product, its business line and GL counter; no GL rules without a chart of accounts"
check "product SCR_SERVICES" "$(sql "select \"BUSINESS_LINE_CODE\" || ' ' || \"PRODUCT_TYPE_CODE\" || ' ' || \"GL_TRAN_SET_CODE\" from \"ADM_PRODUCTS\" where \"PRODUCT_CODE\"='SCR_SERVICES'")" "SCREENING SERVICE SCR"
check "business line SCREENING" "$(sql "select \"NAME_EN\" from \"ADM_LKP_BUSINESS_LINES\" where \"CODE\"='SCREENING'")" Screening
check "GL counter SCR" "$(sql "select count(*) from \"GL_TRAN_SET_SEQUENCES\" where \"MODULE_CODE\"='SCR'")" 1
check "no rules (no chart to copy from)" "$(sql "select count(*) from \"ADM_PRODUCT_EVENT_RULES\" r join \"ADM_PRODUCTS\" p using (\"PRODUCT_ID\") where p.\"PRODUCT_CODE\"='SCR_SERVICES'")" 0
r=$(call GET /api/adm/products "$ADMIN"); check "product in the catalog" "$(body "$r" | jq -r '[.[] | select(.productCode=="SCR_SERVICES")] | length')" 1
r=$(call GET "/api/adm/products/$(sql "select \"PRODUCT_ID\" from \"ADM_PRODUCTS\" where \"PRODUCT_CODE\"='SCR_SERVICES'")/actions" "$ADMIN")
check "its two actions, to link in Admin Setup" "$(body "$r" | jq -r '[.actions[].actionCode] | sort | join(",")')" "SCR_INVOICE_ISSUE,SCR_INVOICE_PAYMENT"

echo "2. An invoice with an amount needs the GL rule: refused, nothing saved"
r=$(call POST /api/scr/billing/plans "$ADMIN" '{"planCode":"GLPLAN","nameEn":"GL","nameAr":"خطة","userMonthlyFee":100,"inquiryFee":2.5,"isActive":true}'); PID=$(body "$r" | jq -r .pricePlanId)
r=$(call PUT /api/scr/companies/$CID "$ADMIN" "$(jq -c --argjson p "$PID" '{companyCode,nameEn,nameAr,licenceNo,contactEmail,statusCode,notes,pricePlanId:$p,billingCycleCode:"MONTHLY"}' <<<"$OLD")")
check "company on the plan" "$(status "$r")" 200
r=$(call POST /api/scr/billing/companies/$CID/invoices "$ADMIN" "{\"periodFrom\":\"$THIS_MONTH\"}")
check "refused" "$(status "$r")" 400
check "with the reason" "$(body "$r" | jq -r .detail | grep -c 'GL posting failed, nothing was saved: .*No product rule is linked to the action "Screening invoice"')" 1
check "no invoice saved" "$(sql "select count(*) from \"SCR_INVOICES\"")" 0

echo "3. An invoice of 0.00 needs no GL; payments and cancelling"
r=$(call PUT /api/scr/billing/plans/$PID "$ADMIN" '{"planCode":"GLPLAN","nameEn":"GL","nameAr":"خطة","userMonthlyFee":0,"inquiryFee":0,"isActive":true}')
r=$(call POST /api/scr/billing/companies/$CID/invoices "$ADMIN" "{\"periodFrom\":\"$THIS_MONTH\"}"); IID=$(body "$r" | jq -r .invoiceId)
check "issued" "$(status "$r")" 200
check "no GL entry, dated today (period not ended)" "$(body "$r" | jq -r '(.glTranSetId|tostring) + " " + .accountingDate + " " + .paymentStatusCode + " " + (.balanceAmount|tostring)')" "null $TODAY UNPAID 0.00"
r=$(call GET /api/scr/billing/bank-accounts "$ADMIN"); check "bank accounts list" "$(status "$r")" 200
r=$(call POST /api/scr/billing/invoices/$IID/payments "$ADMIN" "{\"paymentDate\":\"$TODAY\",\"paymentAmount\":10,\"bankAccountId\":999}")
check "nothing to pay, bad bank account" "$(status "$r") $(body "$r" | jq -r '[.errors | keys[]] | sort | join(",")')" "400 bankAccountId,paymentAmount"
r=$(call POST /api/scr/billing/invoices/$IID/payments "$ADMIN" '{"paymentDate":"2099-01-01"}')
check "date and amount checked" "$(body "$r" | jq -r '[.errors | keys[]] | sort | join(",")')" "bankAccountId,paymentAmount,paymentDate"
r=$(call POST /api/scr/billing/invoices/$IID/cancel "$ADMIN" '{"reason":"test"}')
check "cancelled, nothing to reverse" "$(body "$r" | jq -r '.statusCode + " " + (.cancelGlTranSetId|tostring)')" "CANCELLED null"
r=$(call POST /api/scr/billing/invoices/$IID/payments "$ADMIN" "{\"paymentDate\":\"$TODAY\",\"paymentAmount\":1,\"bankAccountId\":1}")
check "no payment on a cancelled invoice" "$(status "$r")" 400

echo; echo "Passed: $pass  Failed: $fail"
[ "$fail" -eq 0 ]
