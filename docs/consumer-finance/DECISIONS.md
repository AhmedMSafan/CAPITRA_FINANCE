# Consumer finance: agreed decisions

The single list of what was agreed with Ahmed for consumer finance (CF_FINANCE). Every preview and every PR is checked against it,
line by line, before it is shown. A change of decision is made here first.

## Standing rules (Ahmed, 27 Nov 2026)

- **One style everywhere:** before designing anything, look for a screen that already does the same thing and use its style.
  Changing set-up data (lending terms, supplier terms, ...) is always: an admin asks for the change → another admin approves →
  it applies to new work only → an audit trail (field, old → new, why, asked by, approved by, when).
- **Merging (Ahmed, 27 Nov 2026):** once a change is built and CI is green, merge it without asking; then report what changed and how to test.
- **Backward compatibility, every time:** say what happens to existing data, history, running finances and old workflow runs
  before building; never rewrite history; new columns are nullable or have defaults.

## Steps (workflow CF_FINANCE)

| # | Step | Role | What happens | Posts (rule) | Status |
|---|------|------|--------------|--------------|--------|
| 1 | Application (New Finance) | SALES_OFFICER | Main data only: customer, supplier, product, item, price, down payment, term. Amount = price − down payment. One indicative instalment line at the product rate. No charges, no scenario, no schedule saved. | — | Built (#131) |
| 2 | Credit approval | CREDIT_OFFICER | Approves every value and each charge's **timing** (upfront / add to schedule) and **amount** (0 waives it; tax follows); rate = product rate + **margin ±**. Changes go to the status history. Finance amount = financed + charges added to schedule. **Approve creates the claim once** (Ahmed, 27 Nov 2026): upfront charges + down payment when we take it; the step's rule CF_CHARGES posts the charge GL lines (Dr 120802 / Cr 120803). Nothing upfront: no claim. | CF_CHARGES | Built (#130, #132, #144) |
| 3 | Schedule and contract | OPERATIONS_OFFICER | **Accept waits until the claim is Paid** (or there is none). Builds the schedule on the finance amount at the finance rate, from today. | — | Built (#130, #144) |
| 4 | Delivery and customer receipt | OPERATIONS_OFFICER | Books the loan (CF_DELIVERY: Dr Loan incl. added charges / Cr Payable 230801 / Cr Unearned 120803 for added charges; down payment held 230802 → 230801). Schedule dates move to the delivery day. Delivery date, delivery note no., serial/IMEI, customer's confirmation; the signed delivery note goes to the finance's Documents. | CF_DELIVERY | Built (#140) |
| 5 | Supplier payment | TREASURY_OFFICER | CF_SUPPLIER_PAYMENT: Dr 230801 / Cr 410802 commission / Cr Bank. Commission is the supplier's agreed % of the amount owed (price less the down payment it took, or the whole price when we took it), overridable with a reason; paid from our bank account with a transfer reference. | CF_SUPPLIER_PAYMENT | Built (#141) |

- **Claim, the factoring standard** (27 Nov 2026): Credit approval only **books** the claim. Statuses Unpaid (OPEN), Partially Paid,
  Paid, Cancelled. The claim is **paid in Collection › Customer Payments**, like a factoring claim: Collection records the customer's
  money first (DR Bank / CR 230104 Customer Unallocated Receipts), then pays the claim from that payment source, fully or in part
  (COLLECTION.EDIT). Each payment posts product action CF_CLAIM_PAYMENT: DR 230104 / CR 120802 charges, CR 230802 down payment;
  DR 120803 / CR 410801; charges first, then the down payment. Kept in LND_CF_CLAIM_PAYMENTS with the repayment and source.
- The claim popup (workspace Claims › Open, finance page › Open claim) is read only; its **Pay** opens Customer Payments for that claim.
- Rejecting a finance cancels an unpaid claim and reverses its GL; a claim with payments stays, and the history says to refund it.
- The Claims tab lists only created claims; a finance still before Credit approval shows on the Finances tab with its step.
- Finances started before 27 Nov 2026 keep the old step **Charges and claim** (OPERATIONS_OFFICER) between Credit and Schedule, where
  their claim is created and paid.
- **Anyone holding a step's role does the step, as the Inbox offers it**, including the person who entered the finance (Ahmed, 28 Nov 2026;
  replaces "the person who entered the finance does none of its steps").
- Each finance keeps a copy of the supplier's terms (down payment collector, commission, payment and delivery days).
- Upfront charge = claim, income when the claim is paid. Added to schedule = part of the principal, no separate charge.

## Setup

- Rule CF_CHARGES: admin fee 1% of the financed amount, income recognition IMMEDIATE, default PAID_UPFRONT, its own component GL lines.
  Named by the Credit approval step (`finova:ruleCode`). A charge's base is chosen on the rule: CF Total Goods Value or CF Financed Amount.
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
- Supplier terms of an active supplier: the same standard (admin asks, another admin approves, audit trail); changes made before
  27 Nov 2026 stay in the trail as "applied before approval was required"; finances keep the supplier terms they copied.
- Applies to Microfinance products too (the same lending terms page).

## Screens

- Words on screens: "Finance" / تمويل, never "Loan".
- Finance page: the step card on top (deal chips, the step's work, one-line comment beside Reject / Accept); below, one compact Finance card of chips (terms, financed item, charges, claim). Nothing is shown before it exists: no instalment, interest or dates before step 4, no schedule table before step 4, no Payments before the finance is booked.
- Lending Customer Workspace: tabs Finances, Claims, Payments, the same tab style as the Factoring Customer Workspace. Finances shows the open step; Paid out and Maturity show "—" until booked. Claims shows each finance's upfront claim (To be created, Waiting for payment, Paid).

- Every workflow step and New Finance have a dark orange **Rules and GL** button (read only): the page's role, the rules its buttons
  post (step rule or product action), their charges and GL lines with this finance's amounts.

## Open items


- Refund of a claim already paid when the finance is rejected: not built yet (the history says what to refund).
