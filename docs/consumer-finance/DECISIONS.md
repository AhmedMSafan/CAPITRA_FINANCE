# Consumer finance: agreed decisions

The single list of what was agreed with Ahmed for consumer finance (CF_FINANCE). Every preview and every PR is checked against it,
line by line, before it is shown. A change of decision is made here first.

## Steps (workflow CF_FINANCE)

| # | Step | Role | What happens | Posts (rule) | Status |
|---|------|------|--------------|--------------|--------|
| 1 | Application (New Finance) | SALES_OFFICER | Main data only: customer, supplier, product, item, price, down payment, term. Amount = price − down payment. One indicative instalment line at the product rate. No charges, no scenario, no schedule saved. | — | Built (#131) |
| 2 | Credit approval | CREDIT_OFFICER | Approves amount and term. Shows the charges of rule CF_CHARGES; Credit can change each one's **timing** (upfront / add to schedule) and **amount** (0 waives it; tax follows). Rate: the product rate stays fixed, Credit adds a **margin ±**. Changes are written to the status history. Finance amount = financed + charges added to schedule. | — | Built (#130, #132) |
| 3 | Charges and claim | OPERATIONS_OFFICER | Executes Credit's decision, no override. **Create claim**: upfront charges + down payment when we take it; posts CF_CHARGES component GL lines (Dr 120802 / Cr 120803). **Record payment**: product action CF_CLAIM_PAYMENT (Dr Bank / Cr 120802, Cr 230802 down payment; Dr 120803 / Cr 410801). Accept needs PAID, or nothing upfront. Charges added to the schedule post nothing here; they are principal. | CF_CHARGES, CF_CLAIM_PAYMENT | Built (#130) |
| 4 | Schedule and contract | OPERATIONS_OFFICER | Accept builds the schedule on the finance amount at the finance rate, from today. | — | Built (#130) |
| 5 | Delivery and customer receipt | OPERATIONS_OFFICER | Books the loan (CF_DELIVERY: Dr Loan incl. added charges / Cr Payable 230801 / Cr Unearned 120803 for added charges; down payment held 230802 → 230801). Schedule dates move to the delivery day. | CF_DELIVERY | **Not built** |
| 6 | Supplier payment | TREASURY_OFFICER | CF_SUPPLIER_PAYMENT: Dr 230801 / Cr 410802 commission / Cr Bank. Commission is the supplier's agreed %, overridable with a reason. | CF_SUPPLIER_PAYMENT | **Not built** |

- The person who entered the finance does none of its steps.
- Each finance keeps a copy of the supplier's terms (down payment collector, commission, payment and delivery days).
- Upfront charge = claim, income when the claim is paid. Added to schedule = part of the principal, no separate charge.

## Setup

- Rule CF_CHARGES: admin fee 1% of the financed amount, income recognition IMMEDIATE, default PAID_UPFRONT, its own component GL lines.
- A workflow step links its rule by `finova:ruleCode`; a button outside the workflow links through a Product Action (Admin Setup › Product Actions, which lists both, with "Used on").
- No rule CF_CLAIM, no product action CF_NEW_FINANCE.

## Product terms and each finance's copy (Ahmed, 27 Nov 2026)

- A product has **one set of lending terms**: no versions, no amount limits, no term limits.
- The terms are changed by an admin; **another user with the admin role approves** the change. Every change is kept in an audit trail
  on the product page: field, old → new, asked by, approved by, when. An approved change applies to new finances only.
- A finance **copies all the terms when it is created** (step 1). From then on, at any stage of the workflow, product changes do not touch it.
- **Credit approval (step 2) can change every financial value of that finance**: down payment (so the amount), term, rate (directly or as
  a margin on the product rate), repayment frequency, interest method, day count, late fee (type, rate or amount, grace days),
  payment order, insurance required, prepayment allowed, and each charge (timing and amount). Each changed value shows the product's
  value beside it and goes to the history (old → new). Sales cannot change them; after Credit approval they are locked.
- The item price is the supplier's price and is not changed by Credit.
- Charges and GL stay in Product Event Rules and Product Actions.
- Grace days are hidden and saved as 0 when there is no late fee.
- Applies to Microfinance products too (the same lending terms page).

## Screens

- Words on screens: "Finance" / تمويل, never "Loan".
- Finance page: the step card on top (deal chips, the step's work, one-line comment beside Reject / Accept); below, one compact Finance card of chips (terms, financed item, charges, claim). Nothing is shown before it exists: no instalment, interest or dates before step 4, no schedule table before step 4, no Payments before the finance is booked.
- Lending Customer Workspace: tabs Finances, Claims, Payments, the same tab style as the Factoring Customer Workspace. Finances shows the open step; Paid out and Maturity show "—" until booked. Claims shows each finance's upfront claim (To be created, Waiting for payment, Paid).

## Open items

- Step 5 Delivery and customer receipt.
- Step 6 Supplier payment.
