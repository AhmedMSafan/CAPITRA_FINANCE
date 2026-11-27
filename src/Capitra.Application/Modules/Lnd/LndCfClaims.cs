using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Trs;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Lnd;

/// <summary>A line of a consumer finance claim: a charge (amount and tax) or the down payment we take.</summary>
public sealed record LndCfClaimLineDto(string Code, string Name, decimal Amount, decimal TaxAmount, decimal TotalAmount);

/// <summary>A payment of the claim: how much went to the charges and to the down payment, into which account, and its GL.</summary>
public sealed record LndCfClaimPaymentDto(long ClaimPaymentId, DateOnly PaymentDate, decimal PaymentAmount, decimal ChargesAmount, decimal DownPaymentAmount,
    string? BankAccount, string PaymentMethodCode, string? PaymentReference, string? GlTranSetId, string RecordedBy, DateTime RecordedAt);

/// <summary>A consumer finance's upfront claim as its popup shows it; Accounts are the accounts a payment can come into.</summary>
public sealed record LndCfClaimDetailDto(long LoanId, string LoanNumber, string ClientName, string CurrencyCode, string StatusCode, DateOnly? ClaimDate,
    string? CreatedBy, IReadOnlyList<LndCfClaimLineDto> Lines, decimal ChargesAmount, decimal DownPaymentAmount, decimal TotalAmount, decimal PaidAmount,
    decimal OutstandingAmount, string? GlTranSetId, IReadOnlyList<LndCfClaimPaymentDto> Payments, IReadOnlyList<LndPayingAccountDto> Accounts);

/// <summary>A payment of the claim: the amount (the outstanding when left out), our account, how the customer paid, the reference.</summary>
public sealed record LndCfClaimPayRequest(decimal? Amount, long? BankAccountId, string? PaymentMethodCode, string? PaymentReference);

/// <summary>
/// The upfront claim of a consumer finance (Ahmed, 27 Nov 2026), the same standard as a factoring claim: created once, when Credit approves
/// (on finances that started with the Charges and claim step, at that step), posting the rule's charge GL lines; Unpaid (OPEN), Partially
/// Paid, Paid, or Cancelled when the finance is rejected before anything is paid. Schedule and contract waits until it is paid.
/// </summary>
public sealed class LndCfClaimService(ICapitraDbContext db, LndCfChargesPlanner planner, ProductActionPosting posting, IClock clock, ICurrentUser currentUser)
{
    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    /// <summary>
    /// Creates the claim of a tracked finance from Credit's decision: the upfront charges (their GL lines from the rule the step names) and
    /// the down payment when we take it. Nothing upfront: no claim (NONE). Not saved here.
    /// </summary>
    public async Task CreateAsync(LndLoan loan, string step, CancellationToken ct)
    {
        if (loan.ClaimStatusCode is not null) throw new BusinessRuleException("The claim is already created.");
        var saved = await db.Set<LndLoanCharge>().AsNoTracking().Where(x => x.LoanId == loan.LoanId).ToListAsync(ct);
        var (charges, down) = LndCfFinanceRules.ClaimAmounts(saved, loan.DownPaymentCollectorCode, loan.DownPaymentAmount);
        var today = GlTime.Today(clock);
        loan.ClaimChargesAmount = charges;
        loan.ClaimDownPaymentAmount = down;
        loan.ClaimPaidAmount = 0;
        if (charges + down == 0)
        {
            loan.ClaimStatusCode = LndCfFinanceRules.Claim.None;
            return;
        }
        if (charges > 0)
        {
            var (_, businessLine, module, plan, ruleCode) = await planner.PlanAsync(loan, ct, step);
            if (plan is null) throw new BusinessRuleException($"The finance's product has no active rule {ruleCode} today. Add it in Admin Setup.");
            var lines = ClaimLines(plan, saved);
            var doc = new ProductActionDocument("LND_CF_CLAIM", loan.LoanId.ToString(CultureInfo.InvariantCulture), loan.LoanNumber, today, loan.CurrencyCode,
                $"Finance {loan.LoanNumber}: upfront charges claimed, {charges:#,0.00}");
            try
            {
                loan.ClaimGlTranSetId = await posting.PostLinesAsync(plan, lines, module, "Consumer finance claim", "3100", businessLine, doc, ct);
            }
            catch (ApexProcessException ex)
            {
                throw new BusinessRuleException($"GL posting failed, the claim was not created: {GlPostingApi.CleanMessage(ex.Message)}");
            }
        }
        loan.ClaimStatusCode = LndCfFinanceRules.Claim.Open;
        db.Set<LndLoanEvent>().Add(new LndLoanEvent
        {
            LoanId = loan.LoanId, EventCode = "CF_CLAIM", EventDate = today, EventAmount = charges + down, GlTranSetId = loan.ClaimGlTranSetId,
            SourceDocumentType = "LND_CF_CLAIM", SourceDocumentId = loan.LoanId,
            Notes = $"Claim {charges + down:#,0.00}: upfront charges {charges:#,0.00}, down payment {down:#,0.00}.",
        });
    }

    /// <summary>The rule's charge GL lines for the upfront charges; every upfront charge must have them.</summary>
    private static IReadOnlyList<ProductEventPostingLine> ClaimLines(ProductEventPlan plan, IEnumerable<LndLoanCharge> saved)
    {
        var upfront = saved.Where(x => x.TimingCode == LndCfCharges.Upfront && x.ChargeAmount + x.TaxAmount != 0)
            .GroupBy(x => x.ChargeCode).ToDictionary(x => x.Key, x => x.Sum(c => c.ChargeAmount + c.TaxAmount));
        var missing = upfront.Keys.Where(code => !plan.Lines.Any(l => l.ChargeCode == code && l.AmountCode is "CHARGE_AMOUNT" or "CHARGE_IMMEDIATE")).ToList();
        if (missing.Count > 0)
            throw new BusinessRuleException($"Rule {plan.RuleCode} has no GL lines for {string.Join(", ", missing)}. Add them in Admin Setup > the rule > Component GL Entries.");
        return ProductEventPosting.Build(plan,
            l => l.ChargeCode is { } code && l.AmountCode is "CHARGE_AMOUNT" or "CHARGE_IMMEDIATE" && upfront.TryGetValue(code, out var a) ? a : 0, null);
    }

    /// <summary>
    /// Records a payment of the claim (all or part of what is outstanding): it pays the charges first (earned), then the down payment
    /// (held for the supplier), posting the product action CF_CLAIM_PAYMENT for those amounts. The claim becomes Partially Paid or Paid.
    /// </summary>
    public async Task PayAsync(long loanId, LndCfClaimPayRequest r, CancellationToken ct)
    {
        await using var tx = await db.BeginTransactionAsync(ct);
        var loan = await db.Set<LndLoan>().FirstOrDefaultAsync(x => x.LoanId == loanId, ct) ?? throw new NotFoundException($"Finance {loanId} was not found.");
        await PayLoanAsync(loan, r, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    /// <summary>The same on a tracked finance, inside the caller's transaction; not saved here.</summary>
    public async Task PayLoanAsync(LndLoan loan, LndCfClaimPayRequest r, CancellationToken ct)
    {
        if (loan.ClaimStatusCode is not (LndCfFinanceRules.Claim.Open or LndCfFinanceRules.Claim.PartiallyPaid))
            throw new BusinessRuleException(loan.ClaimStatusCode is null ? "The claim is not created yet." : "The claim is not waiting for payment.");
        var charges = loan.ClaimChargesAmount ?? 0;
        var down = loan.ClaimDownPaymentAmount ?? 0;
        var paid = loan.ClaimPaidAmount ?? 0;
        var outstanding = charges + down - paid;

        var e = new Dictionary<string, string[]>();
        var amount = Money.Round(r.Amount ?? outstanding, 2);
        if (amount <= 0 || amount > outstanding) e["amount"] = [$"The amount must be more than 0 and at most the outstanding {outstanding:#,0.00}."];
        var method = (r.PaymentMethodCode ?? "").Trim().ToUpperInvariant();
        var reference = string.IsNullOrWhiteSpace(r.PaymentReference) ? null : r.PaymentReference.Trim();
        var account = r.BankAccountId is { } accountId ? await db.Set<TrsBankAccount>().AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == accountId, ct) : null;
        if (account is null || !account.IsActive || account.CurrencyCode != loan.CurrencyCode) e["bankAccountId"] = ["Choose one of our active accounts in the finance's currency."];
        else if (string.IsNullOrWhiteSpace(account.ErpAccountCode)) e["bankAccountId"] = [$"Bank account {account.AccountRef} has no GL account code. Set it in Treasury."];
        if (!LndCfFinanceRules.PaymentMethods.Contains(method)) e["paymentMethodCode"] = ["Choose how the customer paid."];
        else if (method != "CASH" && reference is null) e["paymentReference"] = ["Enter the payment reference."];
        if (reference is { Length: > 100 }) e["paymentReference"] = ["The reference is longer than 100 characters."];
        if (e.Count > 0) throw new BusinessValidationException(e);

        // the charges first, then the down payment
        var chargesPart = Math.Min(amount, Math.Max(0, charges - paid));
        var downPart = amount - chargesPart;
        var today = GlTime.Today(clock);
        var doc = new ProductActionDocument("LND_CF_CLAIM", loan.LoanId.ToString(CultureInfo.InvariantCulture), loan.LoanNumber, today, loan.CurrencyCode,
            $"Finance {loan.LoanNumber}: claim payment {amount:#,0.00} ({method.ToLowerInvariant().Replace('_', ' ')}{(reference is null ? "" : " " + reference)})");
        Func<string, decimal?> amountOf = code => code switch
        {
            "PAYMENT_AMOUNT" => amount,
            "CHARGES_PAID_AMOUNT" => chargesPart,
            "DOWN_PAYMENT_AMOUNT" => downPart,
            _ => null,
        };
        string gl;
        try
        {
            gl = await posting.PostAsync(ProductActions.CfClaimPayment, doc, [(account!.ErpAccountCode!.Trim(), amountOf)], ct);
        }
        catch (ApexProcessException ex)
        {
            throw new BusinessRuleException($"GL posting failed, the payment was not recorded: {GlPostingApi.CleanMessage(ex.Message)}");
        }
        db.Set<LndCfClaimPayment>().Add(new LndCfClaimPayment
        {
            LoanId = loan.LoanId, PaymentDate = today, PaymentAmount = amount, ChargesAmount = chargesPart, DownPaymentAmount = downPart,
            BankAccountId = account!.AccountId, PaymentMethodCode = method, PaymentReference = reference, GlTranSetId = gl,
        });
        loan.ClaimPaidAmount = paid + amount;
        loan.ClaimPaymentMethodCode = method;
        loan.ClaimPaymentReference = reference;
        loan.ClaimPaymentGlTranSetId = gl;
        var full = loan.ClaimPaidAmount == charges + down;
        loan.ClaimStatusCode = full ? LndCfFinanceRules.Claim.Paid : LndCfFinanceRules.Claim.PartiallyPaid;
        if (full)
        {
            loan.ClaimPaidAt = clock.UtcNow;
            loan.ClaimPaidBy = User;
        }
        db.Set<LndLoanEvent>().Add(new LndLoanEvent
        {
            LoanId = loan.LoanId, EventCode = "CF_CLAIM_PAID", EventDate = today, EventAmount = amount, GlTranSetId = gl,
            SourceDocumentType = "LND_CF_CLAIM", SourceDocumentId = loan.LoanId,
            Notes = $"Claim payment {amount:#,0.00} into {account!.AccountRef} by {method.ToLowerInvariant().Replace('_', ' ')}{(reference is null ? "" : ", " + reference)}"
                + $" (charges {chargesPart:#,0.00}, down payment {downPart:#,0.00}); {(full ? "claim paid" : $"outstanding {charges + down - loan.ClaimPaidAmount:#,0.00}")}.",
        });
    }

    /// <summary>
    /// The finance is rejected: an unpaid claim is cancelled and its charge GL reversed. A claim with payments stays as it is; the status
    /// history says what the customer paid, to be refunded. Not saved here.
    /// </summary>
    public async Task<string?> CancelAsync(LndLoan loan, CancellationToken ct)
    {
        if (loan.ClaimStatusCode is not (LndCfFinanceRules.Claim.Open or LndCfFinanceRules.Claim.PartiallyPaid or LndCfFinanceRules.Claim.Paid)) return null;
        if ((loan.ClaimPaidAmount ?? (loan.ClaimStatusCode == LndCfFinanceRules.Claim.Paid ? (loan.ClaimChargesAmount ?? 0) + (loan.ClaimDownPaymentAmount ?? 0) : 0)) is > 0 and var paid)
            return $"The customer paid {paid:#,0.00} of the claim; refund it.";
        var today = GlTime.Today(clock);
        if (loan.ClaimGlTranSetId is not null && (loan.ClaimChargesAmount ?? 0) > 0)
        {
            var (_, businessLine, module, plan, ruleCode) = await planner.PlanAsync(loan, ct);
            if (plan is null) throw new BusinessRuleException($"The finance's product has no active rule {ruleCode} today; the claim cannot be cancelled.");
            var saved = await db.Set<LndLoanCharge>().AsNoTracking().Where(x => x.LoanId == loan.LoanId).ToListAsync(ct);
            // the claim's entries, debit and credit swapped
            var lines = ClaimLines(plan, saved).Select(l => l with { Dr = l.Cr, Cr = l.Dr }).ToList();
            var doc = new ProductActionDocument("LND_CF_CLAIM", loan.LoanId.ToString(CultureInfo.InvariantCulture), loan.LoanNumber, today, loan.CurrencyCode,
                $"Finance {loan.LoanNumber}: claim cancelled (reverses {loan.ClaimGlTranSetId})");
            try
            {
                var gl = await posting.PostLinesAsync(plan, lines, module, "Consumer finance claim cancelled", "3100", businessLine, doc, ct);
                db.Set<LndLoanEvent>().Add(new LndLoanEvent
                {
                    LoanId = loan.LoanId, EventCode = "CF_CLAIM_CANCELLED", EventDate = today, EventAmount = loan.ClaimChargesAmount ?? 0, GlTranSetId = gl,
                    SourceDocumentType = "LND_CF_CLAIM", SourceDocumentId = loan.LoanId, Notes = $"Claim cancelled; GL {loan.ClaimGlTranSetId} reversed.",
                });
            }
            catch (ApexProcessException ex)
            {
                throw new BusinessRuleException($"GL posting failed, the claim was not cancelled: {GlPostingApi.CleanMessage(ex.Message)}");
            }
        }
        loan.ClaimStatusCode = LndCfFinanceRules.Claim.Cancelled;
        return "The unpaid claim was cancelled.";
    }

    /// <summary>The claim of a finance for its popup; null when the finance has no claim (none created, or nothing upfront).</summary>
    public async Task<LndCfClaimDetailDto?> GetAsync(long loanId, CancellationToken ct)
    {
        var loan = await db.Set<LndLoan>().AsNoTracking().FirstOrDefaultAsync(x => x.LoanId == loanId, ct) ?? throw new NotFoundException($"Finance {loanId} was not found.");
        if (loan.ClaimStatusCode is null or LndCfFinanceRules.Claim.None) return null;
        var client = await db.Set<Capitra.Domain.Modules.Crm.CrmClient>().AsNoTracking().Where(x => x.ClientId == loan.ClientId)
            .Select(x => x.ClientName).FirstOrDefaultAsync(ct);
        var created = await db.Set<LndLoanEvent>().AsNoTracking().Where(x => x.LoanId == loanId && x.EventCode == "CF_CLAIM")
            .OrderBy(x => x.LoanEventId).Select(x => new { x.EventDate, x.CreatedBy }).FirstOrDefaultAsync(ct);
        var charges = await db.Set<LndLoanCharge>().AsNoTracking().Where(x => x.LoanId == loanId && x.TimingCode == LndCfCharges.Upfront && x.ChargeAmount + x.TaxAmount != 0)
            .OrderBy(x => x.LoanChargeId).ToListAsync(ct);
        var lines = charges.Select(c => new LndCfClaimLineDto(c.ChargeCode, c.ChargeName, c.ChargeAmount, c.TaxAmount, c.ChargeAmount + c.TaxAmount)).ToList();
        if ((loan.ClaimDownPaymentAmount ?? 0) > 0)
            lines.Add(new LndCfClaimLineDto("DOWN_PAYMENT", "Down payment", loan.ClaimDownPaymentAmount!.Value, 0, loan.ClaimDownPaymentAmount.Value));
        var payments = await (from p in db.Set<LndCfClaimPayment>().AsNoTracking()
                              join a in db.Set<TrsBankAccount>().AsNoTracking() on p.BankAccountId equals a.AccountId into acc
                              from a in acc.DefaultIfEmpty()
                              where p.LoanId == loanId
                              orderby p.ClaimPaymentId
                              select new LndCfClaimPaymentDto(p.ClaimPaymentId, p.PaymentDate, p.PaymentAmount, p.ChargesAmount, p.DownPaymentAmount,
                                  a == null ? null : a.AccountRef, p.PaymentMethodCode, p.PaymentReference, p.GlTranSetId, p.CreatedBy, p.CreatedAt)).ToListAsync(ct);
        var total = (loan.ClaimChargesAmount ?? 0) + (loan.ClaimDownPaymentAmount ?? 0);
        var paid = Paid(loan);
        var open = loan.ClaimStatusCode is LndCfFinanceRules.Claim.Open or LndCfFinanceRules.Claim.PartiallyPaid;
        IReadOnlyList<LndPayingAccountDto> accounts = open ? await LndCfFinanceService.AccountsAsync(db, loan.CurrencyCode, ct) : [];
        return new LndCfClaimDetailDto(loan.LoanId, loan.LoanNumber, client ?? "", loan.CurrencyCode, loan.ClaimStatusCode!, created?.EventDate, created?.CreatedBy,
            lines, loan.ClaimChargesAmount ?? 0, loan.ClaimDownPaymentAmount ?? 0, total, paid, total - paid, loan.ClaimGlTranSetId, payments, accounts);
    }

    /// <summary>What the customer has paid of the claim (claims paid before partial payments: the whole claim).</summary>
    public static decimal Paid(LndLoan l) =>
        l.ClaimPaidAmount ?? (l.ClaimStatusCode == LndCfFinanceRules.Claim.Paid ? (l.ClaimChargesAmount ?? 0) + (l.ClaimDownPaymentAmount ?? 0) : 0);
}
