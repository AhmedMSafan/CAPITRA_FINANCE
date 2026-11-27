using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Fct;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Domain.Modules.Col;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Lnd;

/// <summary>A line of a consumer finance claim: a charge (amount and tax) or the down payment we take.</summary>
public sealed record LndCfClaimLineDto(string Code, string Name, decimal Amount, decimal TaxAmount, decimal TotalAmount);

/// <summary>A payment of the claim: how much went to the charges and to the down payment, into which account, and its GL.</summary>
public sealed record LndCfClaimPaymentDto(long ClaimPaymentId, DateOnly PaymentDate, decimal PaymentAmount, decimal ChargesAmount, decimal DownPaymentAmount,
    string? RepaymentNumber, string PaymentMethodCode, string? PaymentReference, string? GlTranSetId, string RecordedBy, DateTime RecordedAt);

/// <summary>A consumer finance's upfront claim as its popup shows it (read only; it is paid in Collection › Customer Payments).</summary>
public sealed record LndCfClaimDetailDto(long LoanId, string LoanNumber, long ClientId, string ClientName, string CurrencyCode, string StatusCode, DateOnly? ClaimDate,
    string? CreatedBy, IReadOnlyList<LndCfClaimLineDto> Lines, decimal ChargesAmount, decimal DownPaymentAmount, decimal TotalAmount, decimal PaidAmount,
    decimal OutstandingAmount, string? GlTranSetId, IReadOnlyList<LndCfClaimPaymentDto> Payments);

/// <summary>
/// The upfront claim of a consumer finance (Ahmed, 27 Nov 2026), the same standard as a factoring claim: created once, when Credit approves
/// (on finances that started with the Charges and claim step, at that step), posting the rule's charge GL lines; paid in Collection ›
/// Customer Payments from the customer's payment sources; Unpaid (OPEN), Partially Paid, Paid, or Cancelled when the finance is rejected
/// before anything is paid. Schedule and contract waits until it is paid.
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

    // ---------------------------------------------------------------- Collection › Customer Payments (the factoring standard)

    private static bool IsOpen(string? status) => status is LndCfFinanceRules.Claim.Open or LndCfFinanceRules.Claim.PartiallyPaid;

    /// <summary>The customers with consumer finance claims to pay.</summary>
    public async Task<IReadOnlyList<FctOptionDto>> CustomersWithOpenClaimsAsync(CancellationToken ct) =>
        (await (from l in db.Set<LndLoan>().AsNoTracking()
                join c in db.Set<CrmClient>() on l.ClientId equals c.ClientId
                where l.ClaimStatusCode == LndCfFinanceRules.Claim.Open || l.ClaimStatusCode == LndCfFinanceRules.Claim.PartiallyPaid
                select new { c.ClientId, c.ClientName }).Distinct().ToListAsync(ct))
        .OrderBy(x => x.ClientName)
        .Select(x => new FctOptionDto(x.ClientId.ToString(CultureInfo.InvariantCulture), $"{x.ClientName} -{x.ClientId}")).ToList();

    /// <summary>A customer's open consumer finance claims, in the factoring claim's shape (claim id and number: the finance's).</summary>
    public async Task<IReadOnlyList<FctClaimRowDto>> OpenClaimsAsync(long clientId, CancellationToken ct)
    {
        var loans = await db.Set<LndLoan>().AsNoTracking()
            .Where(x => x.ClientId == clientId && (x.ClaimStatusCode == LndCfFinanceRules.Claim.Open || x.ClaimStatusCode == LndCfFinanceRules.Claim.PartiallyPaid))
            .OrderByDescending(x => x.LoanId).ToListAsync(ct);
        var rows = new List<FctClaimRowDto>();
        foreach (var l in loans) rows.Add(await RowAsync(l, ct));
        return rows;
    }

    private async Task<FctClaimRowDto> RowAsync(LndLoan l, CancellationToken ct)
    {
        var client = await db.Set<CrmClient>().AsNoTracking().Where(x => x.ClientId == l.ClientId).Select(x => x.ClientName).FirstOrDefaultAsync(ct);
        var date = await db.Set<LndLoanEvent>().AsNoTracking().Where(x => x.LoanId == l.LoanId && x.EventCode == "CF_CLAIM")
            .OrderBy(x => x.LoanEventId).Select(x => (DateOnly?)x.EventDate).FirstOrDefaultAsync(ct);
        var total = (l.ClaimChargesAmount ?? 0) + (l.ClaimDownPaymentAmount ?? 0);
        var paid = Paid(l);
        return new FctClaimRowDto(l.LoanId, l.LoanNumber, l.ClientId, client, l.LoanId, l.LoanNumber, date ?? DateOnly.FromDateTime(l.CreatedAt),
            l.CurrencyCode, total, paid, total - paid, l.ClaimStatusCode ?? "");
    }

    /// <summary>A consumer finance claim for Customer Payments: its lines (charges, then the down payment, paid in that order) and payments.</summary>
    public async Task<FctClaimDto> CollectionClaimAsync(long loanId, CancellationToken ct)
    {
        var l = await db.Set<LndLoan>().AsNoTracking().FirstOrDefaultAsync(x => x.LoanId == loanId, ct) ?? throw new NotFoundException($"Finance {loanId} was not found.");
        if (l.ClaimStatusCode is null or LndCfFinanceRules.Claim.None) throw new BusinessRuleException($"Finance {l.LoanNumber} has no claim.");
        var row = await RowAsync(l, ct);
        var charges = await db.Set<LndLoanCharge>().AsNoTracking().Where(x => x.LoanId == loanId && x.TimingCode == LndCfCharges.Upfront && x.ChargeAmount + x.TaxAmount != 0)
            .OrderBy(x => x.LoanChargeId).ToListAsync(ct);
        var left = row.PaidAmount;
        var lines = new List<FctClaimLineDto>();
        var no = 0;
        foreach (var c in charges)
        {
            var total = c.ChargeAmount + c.TaxAmount;
            var paid = Math.Min(left, total);
            left -= paid;
            lines.Add(new FctClaimLineDto(++no, no, c.ChargeCode, c.ChargeName, c.ChargeAmount, c.TaxAmount, total, paid, total - paid));
        }
        if ((l.ClaimDownPaymentAmount ?? 0) is > 0 and var down)
        {
            var paid = Math.Min(left, down);
            lines.Add(new FctClaimLineDto(++no, no, "DOWN_PAYMENT", "Down payment", down, 0, down, paid, down - paid));
        }
        var payments = await (from p in db.Set<LndCfClaimPayment>().AsNoTracking()
                              join s in db.Set<ColRepaymentSource>() on p.RepaymentSourceId equals s.RepaymentSourceId into ss
                              from s in ss.DefaultIfEmpty()
                              join t in db.Set<ColRepaymentTransaction>() on p.RepaymentTxnId equals t.RepaymentTxnId into ts
                              from t in ts.DefaultIfEmpty()
                              where p.LoanId == loanId
                              orderby p.ClaimPaymentId
                              select new FctClaimPaymentDto(p.ClaimPaymentId, t == null ? "-" : t.RepaymentNumber, s == null ? p.PaymentMethodCode : s.SourceType,
                                  p.PaymentReference, p.PaymentDate, p.PaymentAmount, p.GlTranSetId)).ToListAsync(ct);
        return new FctClaimDto(row, lines, payments);
    }

    /// <summary>
    /// Pays a consumer finance claim from one of the customer's payment sources (Collection › Customer Payments), like a factoring claim:
    /// the requested amount (else all it can), at most the source's balance and the claim's outstanding; the charges first, then the down
    /// payment. Posts the product action CF_CLAIM_PAYMENT (DR 230104 Customer Unallocated Receipts / CR 120802 charges, CR 230802 down
    /// payment; DR 120803 / CR 410801 the charges earned). The claim becomes Partially Paid or Paid.
    /// </summary>
    public async Task<FctClaimPaidDto> PayFromSourceAsync(long loanId, FctClaimPayDto dto, CancellationToken ct)
    {
        await using var tx = await db.BeginTransactionAsync(ct);
        var loan = await db.Set<LndLoan>().FirstOrDefaultAsync(x => x.LoanId == loanId, ct) ?? throw new NotFoundException($"Finance {loanId} was not found.");
        if (!IsOpen(loan.ClaimStatusCode)) throw new BusinessRuleException($"The claim of finance {loan.LoanNumber} is not waiting for payment.");
        var source = await db.Set<ColRepaymentSource>().FirstOrDefaultAsync(x => x.RepaymentSourceId == dto.RepaymentSourceId, ct)
            ?? throw new NotFoundException($"Payment source {dto.RepaymentSourceId} was not found.");
        var txn = await db.Set<ColRepaymentTransaction>().FirstAsync(x => x.RepaymentTxnId == source.RepaymentTxnId, ct);
        if (txn.ClientId != loan.ClientId) throw new BusinessRuleException("The payment source belongs to another customer.");
        if (source.StatusCode != "ACTIVE" || txn.StatusCode is "REVERSED" or "CANCELLED") throw new BusinessRuleException("The payment source is not active.");
        var available = source.SourceAmount - source.AllocatedAmount;
        if (available <= 0) throw new BusinessRuleException("The payment source has no balance left.");
        if (dto.Amount is <= 0) throw new BusinessRuleException("The amount to pay must be more than zero.");
        var charges = loan.ClaimChargesAmount ?? 0;
        var down = loan.ClaimDownPaymentAmount ?? 0;
        var paidBefore = Paid(loan);
        var outstanding = charges + down - paidBefore;
        var pay = Math.Min(Math.Min(available, outstanding), dto.Amount ?? decimal.MaxValue);
        if (pay <= 0) throw new BusinessRuleException($"The claim of finance {loan.LoanNumber} has nothing left to pay.");

        // the charges first, then the down payment
        var chargesPart = Math.Min(pay, Math.Max(0m, charges - paidBefore));
        var downPart = pay - chargesPart;
        var today = GlTime.Today(clock);
        var reference = source.SourceReference ?? txn.BankStatementReference ?? txn.ExternalReference;
        var doc = new ProductActionDocument("LND_CF_CLAIM", loan.LoanId.ToString(CultureInfo.InvariantCulture), loan.LoanNumber, today, loan.CurrencyCode,
            $"Finance {loan.LoanNumber}: claim payment {pay:#,0.00} from {txn.RepaymentNumber}");
        Func<string, decimal?> amountOf = code => code switch
        {
            "PAYMENT_AMOUNT" => pay,
            "CHARGES_PAID_AMOUNT" => chargesPart,
            "DOWN_PAYMENT_AMOUNT" => downPart,
            _ => null,
        };
        string gl;
        try
        {
            gl = await posting.PostAsync(ProductActions.CfClaimPayment, doc, amountOf, ct);
        }
        catch (ApexProcessException ex)
        {
            throw new BusinessRuleException($"GL posting failed, nothing was paid: {GlPostingApi.CleanMessage(ex.Message)}");
        }

        db.Set<LndCfClaimPayment>().Add(new LndCfClaimPayment
        {
            LoanId = loan.LoanId, PaymentDate = today, PaymentAmount = pay, ChargesAmount = chargesPart, DownPaymentAmount = downPart,
            PaymentMethodCode = source.SourceType, PaymentReference = reference, GlTranSetId = gl,
            RepaymentTxnId = txn.RepaymentTxnId, RepaymentSourceId = source.RepaymentSourceId,
        });
        source.AllocatedAmount += pay;
        txn.AllocatedAmount += pay;
        loan.ClaimPaidAmount = paidBefore + pay;
        loan.ClaimPaymentMethodCode = source.SourceType;
        loan.ClaimPaymentReference = reference;
        loan.ClaimPaymentGlTranSetId = gl;
        var full = loan.ClaimPaidAmount >= charges + down;
        loan.ClaimStatusCode = full ? LndCfFinanceRules.Claim.Paid : LndCfFinanceRules.Claim.PartiallyPaid;
        if (full)
        {
            loan.ClaimPaidAt = clock.UtcNow;
            loan.ClaimPaidBy = User;
        }
        db.Set<LndLoanEvent>().Add(new LndLoanEvent
        {
            LoanId = loan.LoanId, EventCode = "CF_CLAIM_PAID", EventDate = today, EventAmount = pay, GlTranSetId = gl,
            SourceDocumentType = "LND_CF_CLAIM", SourceDocumentId = loan.LoanId,
            Notes = $"Claim payment {pay:#,0.00} from {txn.RepaymentNumber} (charges {chargesPart:#,0.00}, down payment {downPart:#,0.00}); "
                + (full ? "claim paid." : $"outstanding {charges + down - loan.ClaimPaidAmount:#,0.00}."),
        });
        await db.SaveChangesAsync(ct);
        var held = await db.Set<ColRepaymentSource>().Where(x => x.RepaymentTxnId == txn.RepaymentTxnId && x.StatusCode == "ACTIVE")
            .SumAsync(x => (decimal?)x.SourceAmount, ct) ?? 0;
        txn.StatusCode = txn.AllocatedAmount <= 0 ? "DRAFT" : txn.AllocatedAmount >= held ? "ALLOCATED" : "PARTIALLY_ALLOCATED";
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var message = full
            ? $"The claim of finance {loan.LoanNumber} is fully paid ({gl}). Its Schedule and contract step can now be accepted."
            : $"{pay:#,0.00} paid on the claim of finance {loan.LoanNumber} ({gl}); {charges + down - loan.ClaimPaidAmount:#,0.00} is still to be paid.";
        return new FctClaimPaidDto(await CollectionClaimAsync(loanId, ct), message);
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
        var client = await db.Set<CrmClient>().AsNoTracking().Where(x => x.ClientId == loan.ClientId)
            .Select(x => x.ClientName).FirstOrDefaultAsync(ct);
        var created = await db.Set<LndLoanEvent>().AsNoTracking().Where(x => x.LoanId == loanId && x.EventCode == "CF_CLAIM")
            .OrderBy(x => x.LoanEventId).Select(x => new { x.EventDate, x.CreatedBy }).FirstOrDefaultAsync(ct);
        var charges = await db.Set<LndLoanCharge>().AsNoTracking().Where(x => x.LoanId == loanId && x.TimingCode == LndCfCharges.Upfront && x.ChargeAmount + x.TaxAmount != 0)
            .OrderBy(x => x.LoanChargeId).ToListAsync(ct);
        var lines = charges.Select(c => new LndCfClaimLineDto(c.ChargeCode, c.ChargeName, c.ChargeAmount, c.TaxAmount, c.ChargeAmount + c.TaxAmount)).ToList();
        if ((loan.ClaimDownPaymentAmount ?? 0) > 0)
            lines.Add(new LndCfClaimLineDto("DOWN_PAYMENT", "Down payment", loan.ClaimDownPaymentAmount!.Value, 0, loan.ClaimDownPaymentAmount.Value));
        var payments = await (from p in db.Set<LndCfClaimPayment>().AsNoTracking()
                              join t in db.Set<ColRepaymentTransaction>().AsNoTracking() on p.RepaymentTxnId equals t.RepaymentTxnId into ts
                              from t in ts.DefaultIfEmpty()
                              where p.LoanId == loanId
                              orderby p.ClaimPaymentId
                              select new LndCfClaimPaymentDto(p.ClaimPaymentId, p.PaymentDate, p.PaymentAmount, p.ChargesAmount, p.DownPaymentAmount,
                                  t == null ? null : t.RepaymentNumber, p.PaymentMethodCode, p.PaymentReference, p.GlTranSetId, p.CreatedBy, p.CreatedAt)).ToListAsync(ct);
        var total = (loan.ClaimChargesAmount ?? 0) + (loan.ClaimDownPaymentAmount ?? 0);
        var paid = Paid(loan);
        return new LndCfClaimDetailDto(loan.LoanId, loan.LoanNumber, loan.ClientId, client ?? "", loan.CurrencyCode, loan.ClaimStatusCode!, created?.EventDate,
            created?.CreatedBy, lines, loan.ClaimChargesAmount ?? 0, loan.ClaimDownPaymentAmount ?? 0, total, paid, total - paid, loan.ClaimGlTranSetId, payments);
    }

    /// <summary>What the customer has paid of the claim (claims paid before partial payments: the whole claim).</summary>
    public static decimal Paid(LndLoan l) =>
        l.ClaimPaidAmount ?? (l.ClaimStatusCode == LndCfFinanceRules.Claim.Paid ? (l.ClaimChargesAmount ?? 0) + (l.ClaimDownPaymentAmount ?? 0) : 0);
}
