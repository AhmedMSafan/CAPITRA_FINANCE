using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Crm;
using Capitra.Application.Modules.Gl;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Chq;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Fct;

public sealed record FctClaimLineDto(
    long ClaimLineId, int LineNo, string ChargeCode, string? ChargeName, decimal ChargeAmount, decimal TaxAmount, decimal TotalAmount, decimal PaidAmount,
    decimal OutstandingAmount);

public sealed record FctClaimPaymentDto(
    long ClaimPaymentId, string RepaymentNumber, string SourceType, string? Reference, DateOnly PaymentDate, decimal PaymentAmount, string? GlTranSetId);

/// <summary>A claim as its lists show it.</summary>
public sealed record FctClaimRowDto(
    long ClaimId, string ClaimNumber, long ClientId, string? ClientName, long DisbursementId, string DisbursementNumber, DateOnly ClaimDate,
    string? CurrencyCode, decimal TotalAmount, decimal PaidAmount, decimal OutstandingAmount, string StatusCode);

public sealed record FctClaimDto(FctClaimRowDto Claim, IReadOnlyList<FctClaimLineDto> Lines, IReadOnlyList<FctClaimPaymentDto> Payments);

/// <summary>The Claim section of the Invoice Upload page: the request's claim status and its claim, if any.</summary>
public sealed record FctDisbursementClaimDto(string ClaimStatusCode, decimal ChargesTotal, bool IsCompleted, FctClaimDto? Claim);

/// <summary>A payment source of the customer with balance left (FCT_REPAYMENT_SOURCES of an active repayment).</summary>
public sealed record FctClaimSourceDto(
    long RepaymentSourceId, string RepaymentNumber, string SourceType, string? Reference, DateOnly ValueDate, decimal SourceAmount, decimal UsedAmount,
    decimal AvailableAmount);

public sealed record FctClaimPayDto(long RepaymentSourceId, decimal? Amount);

public sealed record FctClaimPaidDto(FctClaimDto Claim, string Message);

/// <summary>
/// Customer claims (Ahmed, 2 Nov 2026). Create Claim on the Invoice Upload step processes the request and claims its charges from the
/// customer (FCT_DISBURSEMENT_REQUEST.CLAIM_STATUS_CODE becomes CREATED_UNPAID). Collection pays the claim from the customer's payment
/// sources, fully or in part: each payment uses up the source's balance, pays the claim's lines in order, and posts under its own GL
/// transaction set of the product the charges' journals with the paid amounts (journal 1 DR customer receivable / CR the charge's
/// income, journal 2 DR customer receipts (liability) / CR customer receivable: net DR liability / CR each charge's income). When the
/// claim is fully paid the request's claim status becomes PAID and its Invoice Upload step can be completed.
/// </summary>
public sealed class FctCustomerClaimService(
    ICapitraDbContext db, ICurrentUser currentUser, IClock clock, CrmInvoiceEntryService entry, CrmDisbursementProcessService process,
    ProductActionPosting posting, AdmProductEventRuleService rules)
{
    public const string TargetClaimLine = "CLAIM_LINE";

    public const string NotCreated = "NOT_CREATED";
    public const string CreatedUnpaid = "CREATED_UNPAID";
    public const string PartiallyPaid = "PARTIALLY_PAID";
    public const string Paid = "PAID";

    private static readonly string[] Live = [FctCustomerClaim.Open, FctCustomerClaim.PartiallyPaid, FctCustomerClaim.Paid];

    /// <summary>Charges that are owed: calculated, not cancelled or reversed, with an amount.</summary>
    internal static bool Owed(FctDisbursementCharge c) => c.TotalAmount > 0 && c.StatusCode is not ("CANCELLED" or "REVERSED");

    internal static decimal Outstanding(FctDisbursementCharge c) => Math.Max(c.TotalAmount - c.CollectedAmount - c.WaivedAmount, 0);

    /// <summary>The request's claim that is not cancelled, if any.</summary>
    internal Task<FctCustomerClaim?> LiveClaimAsync(long disbursementId, CancellationToken cancellationToken) =>
        db.Set<FctCustomerClaim>().Where(x => x.DisbursementId == disbursementId && Live.Contains(x.StatusCode))
            .OrderByDescending(x => x.ClaimId).FirstOrDefaultAsync(cancellationToken);

    // ---------------------------------------------------------------- the Invoice Upload page

    public async Task<FctDisbursementClaimDto> ForDisbursementAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await db.Set<FctDisbursement>().AsNoTracking().FirstOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement request {disbursementId} was not found.");
        var total = (await db.Set<FctDisbursementCharge>().AsNoTracking().Where(x => x.DisbursementId == disbursementId).ToListAsync(cancellationToken))
            .Where(Owed).Sum(x => x.TotalAmount);
        var claim = await LiveClaimAsync(disbursementId, cancellationToken);
        return new FctDisbursementClaimDto(d.ClaimStatusCode, total, d.IsGlPosted, claim is null ? null : await GetAsync(claim.ClaimId, cancellationToken));
    }

    /// <summary>Create Claim: the page is saved and processed, then the owed charges are claimed from the customer, in one transaction.</summary>
    public async Task<FctDisbursementClaimDto> CreateAsync(long disbursementId, CrmInvoiceEntrySaveDto dto, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        if (await LiveClaimAsync(disbursementId, cancellationToken) is { } existing)
            throw new BusinessRuleException($"Claim {existing.ClaimNumber} already exists for this request.");
        var (d, batch) = await entry.SaveCoreAsync(disbursementId, dto, cancellationToken);
        if (d.IsGlPosted) throw new BusinessRuleException("This request is already completed.");
        await process.ProcessCoreAsync(d, batch, cancellationToken);

        var charges = (await db.Set<FctDisbursementCharge>().Where(x => x.DisbursementId == disbursementId).OrderBy(x => x.DisbursementChargeId)
            .ToListAsync(cancellationToken)).Where(Owed).ToList();
        if (charges.Count == 0) throw new BusinessRuleException("This request has no charges to claim. It can be completed as it is.");

        var today = GlTime.Today(clock);
        var claim = new FctCustomerClaim
        {
            ClaimNumber = "PENDING-" + Guid.NewGuid().ToString("N").ToUpperInvariant(),
            ClientId = d.ClientId,
            DisbursementId = d.DisbursementId,
            ProductId = d.ProductId,
            ProductEventRuleId = charges[0].ProductEventRuleId,
            CurrencyId = d.CurrencyId,
            ClaimDate = today,
            TotalAmount = charges.Sum(x => x.TotalAmount),
            StatusCode = FctCustomerClaim.Open,
        };
        db.Add(claim);
        await db.SaveChangesAsync(cancellationToken);
        claim.ClaimNumber = $"CLM-{today:yyyyMM}-{claim.ClaimId:D8}";

        var no = 0;
        foreach (var g in charges.GroupBy(x => x.RuleCode))
            db.Add(new FctCustomerClaimLine
            {
                ClaimId = claim.ClaimId,
                LineNo = ++no,
                ChargeCode = g.Key,
                ChargeName = g.First().RuleName,
                ChargeAmount = g.Sum(x => x.ChargeAmount),
                TaxAmount = g.Sum(x => x.TaxAmount),
                TotalAmount = g.Sum(x => x.TotalAmount),
            });
        d.ClaimStatusCode = CreatedUnpaid;
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return await ForDisbursementAsync(disbursementId, cancellationToken);
    }

    /// <summary>Cancel Claim: only while nothing of it is paid; the request's charges can then change again.</summary>
    public async Task<FctDisbursementClaimDto> CancelAsync(long claimId, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var claim = await db.Set<FctCustomerClaim>().FirstOrDefaultAsync(x => x.ClaimId == claimId, cancellationToken)
            ?? throw new NotFoundException($"Claim {claimId} was not found.");
        if (claim.StatusCode == FctCustomerClaim.Cancelled) throw new BusinessRuleException($"Claim {claim.ClaimNumber} is already cancelled.");
        if (claim.PaidAmount > 0)
            throw new BusinessRuleException($"Claim {claim.ClaimNumber} is paid {claim.PaidAmount:#,##0.00}; a paid claim cannot be cancelled.");
        var d = await db.Set<FctDisbursement>().FirstAsync(x => x.DisbursementId == claim.DisbursementId, cancellationToken);
        if (d.IsGlPosted) throw new BusinessRuleException("The request is already completed.");

        claim.StatusCode = FctCustomerClaim.Cancelled;
        claim.CancelledAt = clock.UtcNow;
        claim.CancelledBy = currentUser.UserName ?? "SYSTEM";
        d.ClaimStatusCode = NotCreated;
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return await ForDisbursementAsync(d.DisbursementId, cancellationToken);
    }

    // ---------------------------------------------------------------- the lists (Factoring Workspace, Customer Payments)

    /// <summary>The claims (of one customer when given), newest first; openOnly keeps the ones still to be paid.</summary>
    public async Task<IReadOnlyList<FctClaimRowDto>> ListAsync(long? clientId, bool openOnly, CancellationToken cancellationToken)
    {
        var q = db.Set<FctCustomerClaim>().AsNoTracking();
        if (clientId is { } id) q = q.Where(x => x.ClientId == id);
        if (openOnly) q = q.Where(x => x.StatusCode == FctCustomerClaim.Open || x.StatusCode == FctCustomerClaim.PartiallyPaid);
        return await Rows(q).ToListAsync(cancellationToken);
    }

    /// <summary>The customers that have claims to pay.</summary>
    public async Task<IReadOnlyList<FctOptionDto>> CustomersWithOpenClaimsAsync(CancellationToken cancellationToken) =>
        (await (from x in db.Set<FctCustomerClaim>().AsNoTracking()
                join c in db.Set<CrmClient>() on x.ClientId equals c.ClientId
                where x.StatusCode == FctCustomerClaim.Open || x.StatusCode == FctCustomerClaim.PartiallyPaid
                select new { c.ClientId, c.ClientName }).Distinct().ToListAsync(cancellationToken))
        .OrderBy(x => x.ClientName)
        .Select(x => new FctOptionDto(x.ClientId.ToString(CultureInfo.InvariantCulture), $"{x.ClientName} -{x.ClientId}")).ToList();

    public async Task<FctClaimDto> GetAsync(long claimId, CancellationToken cancellationToken)
    {
        var row = await Rows(db.Set<FctCustomerClaim>().AsNoTracking().Where(x => x.ClaimId == claimId)).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Claim {claimId} was not found.");
        var lines = await db.Set<FctCustomerClaimLine>().AsNoTracking().Where(x => x.ClaimId == claimId).OrderBy(x => x.LineNo)
            .Select(x => new FctClaimLineDto(x.ClaimLineId, x.LineNo, x.ChargeCode, x.ChargeName, x.ChargeAmount, x.TaxAmount, x.TotalAmount,
                x.PaidAmount, x.TotalAmount - x.PaidAmount))
            .ToListAsync(cancellationToken);
        var payments = await (from p in db.Set<FctCustomerClaimPayment>().AsNoTracking()
                              join s in db.Set<FctRepaymentSource>() on p.RepaymentSourceId equals s.RepaymentSourceId
                              join t in db.Set<FctRepaymentTransaction>() on p.RepaymentTxnId equals t.RepaymentTxnId
                              join c in db.Set<ChqCheque>() on s.ChequeId equals c.ChequeId into cs
                              from c in cs.DefaultIfEmpty()
                              where p.ClaimId == claimId
                              orderby p.ClaimPaymentId
                              select new FctClaimPaymentDto(p.ClaimPaymentId, t.RepaymentNumber, s.SourceType,
                                  c != null ? c.ChequeNo : s.SourceReference ?? t.BankStatementReference ?? t.ExternalReference, p.PaymentDate,
                                  p.PaymentAmount, p.GlTranSetId))
            .ToListAsync(cancellationToken);
        return new FctClaimDto(row, lines, payments);
    }

    /// <summary>The customer's payment sources with balance left, oldest first.</summary>
    public async Task<IReadOnlyList<FctClaimSourceDto>> SourcesAsync(long clientId, CancellationToken cancellationToken) =>
        await (from s in db.Set<FctRepaymentSource>().AsNoTracking()
               join t in db.Set<FctRepaymentTransaction>() on s.RepaymentTxnId equals t.RepaymentTxnId
               join c in db.Set<ChqCheque>() on s.ChequeId equals c.ChequeId into cs
               from c in cs.DefaultIfEmpty()
               where t.ClientId == clientId && s.StatusCode == "ACTIVE" && t.StatusCode != "REVERSED" && t.StatusCode != "CANCELLED"
                     && s.SourceAmount > s.AllocatedAmount
               orderby t.ValueDate, s.RepaymentSourceId
               select new FctClaimSourceDto(s.RepaymentSourceId, t.RepaymentNumber, s.SourceType,
                   c != null ? c.ChequeNo : s.SourceReference ?? t.BankStatementReference ?? t.ExternalReference, t.ValueDate,
                   s.SourceAmount, s.AllocatedAmount, s.SourceAmount - s.AllocatedAmount))
            .ToListAsync(cancellationToken);

    private IQueryable<FctClaimRowDto> Rows(IQueryable<FctCustomerClaim> claims) =>
        from x in claims
        join d in db.Set<FctDisbursement>() on x.DisbursementId equals d.DisbursementId
        join c in db.Set<CrmClient>() on x.ClientId equals c.ClientId into cl
        from c in cl.DefaultIfEmpty()
        join cu in db.Set<CrmLkpCurrency>() on x.CurrencyId equals cu.CurrencyId into cus
        from cu in cus.DefaultIfEmpty()
        orderby x.ClaimId descending
        select new FctClaimRowDto(x.ClaimId, x.ClaimNumber, x.ClientId, c == null ? null : c.ClientName, x.DisbursementId, d.DisbursementNumber,
            x.ClaimDate, cu == null ? null : cu.CurrencyCode, x.TotalAmount, x.PaidAmount, x.TotalAmount - x.PaidAmount, x.StatusCode);

    // ---------------------------------------------------------------- Pay

    /// <summary>
    /// Pay a claim from one payment source: the requested amount (else all it can), at most the source's balance and the claim's outstanding,
    /// paid line by line in order, under one GL transaction set.
    /// </summary>
    public async Task<FctClaimPaidDto> PayAsync(long claimId, FctClaimPayDto dto, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var claim = await db.Set<FctCustomerClaim>().FirstOrDefaultAsync(x => x.ClaimId == claimId, cancellationToken)
            ?? throw new NotFoundException($"Claim {claimId} was not found.");
        if (claim.StatusCode is not (FctCustomerClaim.Open or FctCustomerClaim.PartiallyPaid))
            throw new BusinessRuleException($"Claim {claim.ClaimNumber} is {claim.StatusCode.ToLowerInvariant().Replace('_', ' ')}; it cannot be paid.");
        var source = await db.Set<FctRepaymentSource>().FirstOrDefaultAsync(x => x.RepaymentSourceId == dto.RepaymentSourceId, cancellationToken)
            ?? throw new NotFoundException($"Payment source {dto.RepaymentSourceId} was not found.");
        var txn = await db.Set<FctRepaymentTransaction>().FirstAsync(x => x.RepaymentTxnId == source.RepaymentTxnId, cancellationToken);
        if (txn.ClientId != claim.ClientId) throw new BusinessRuleException("The payment source belongs to another customer.");
        if (source.StatusCode != "ACTIVE" || txn.StatusCode is "REVERSED" or "CANCELLED")
            throw new BusinessRuleException("The payment source is not active.");
        var available = source.SourceAmount - source.AllocatedAmount;
        if (available <= 0) throw new BusinessRuleException("The payment source has no balance left.");
        if (dto.Amount is <= 0) throw new BusinessRuleException("The amount to pay must be more than zero.");
        var outstanding = claim.TotalAmount - claim.PaidAmount;
        var pay = Math.Min(Math.Min(available, outstanding), dto.Amount ?? decimal.MaxValue);
        pay = Math.Round(pay, 2, MidpointRounding.AwayFromZero);
        if (pay <= 0) throw new BusinessRuleException($"Claim {claim.ClaimNumber} has nothing left to pay.");

        var today = GlTime.Today(clock);
        var payment = new FctCustomerClaimPayment
        {
            ClaimId = claim.ClaimId,
            RepaymentTxnId = txn.RepaymentTxnId,
            RepaymentSourceId = source.RepaymentSourceId,
            PaymentDate = today,
            PaymentAmount = pay,
        };
        db.Add(payment);

        // the lines in order, and the request's charges of each line with it
        var lines = await db.Set<FctCustomerClaimLine>().Where(x => x.ClaimId == claimId).OrderBy(x => x.LineNo).ToListAsync(cancellationToken);
        var charges = await db.Set<FctDisbursementCharge>().Where(x => x.DisbursementId == claim.DisbursementId).OrderBy(x => x.DisbursementChargeId)
            .ToListAsync(cancellationToken);
        var paidByCode = new Dictionary<string, decimal>();
        var left = pay;
        foreach (var l in lines)
        {
            if (left <= 0) break;
            var amount = Math.Min(left, l.TotalAmount - l.PaidAmount);
            if (amount <= 0) continue;
            l.PaidAmount += amount;
            left -= amount;
            paidByCode[l.ChargeCode] = amount;
            db.Add(new FctRepaymentAllocation
            {
                RepaymentTxnId = txn.RepaymentTxnId,
                RepaymentSourceId = source.RepaymentSourceId,
                TargetType = TargetClaimLine,
                TargetId = l.ClaimLineId,
                DisbursementId = claim.DisbursementId,
                ComponentCode = "CHARGE",
                AllocationMode = "AMOUNT",
                RequestedValue = amount,
                AllocationAmount = amount,
                AllocationDate = today,
                StatusCode = "ACTIVE",
            });
            var rest = amount;
            foreach (var c in charges.Where(x => x.RuleCode == l.ChargeCode && Owed(x) && Outstanding(x) > 0))
            {
                if (rest <= 0) break;
                var part = Math.Min(rest, Outstanding(c));
                c.CollectedAmount += part;
                c.StatusCode = c.CollectedAmount + c.WaivedAmount >= c.TotalAmount ? "COLLECTED" : "PARTIALLY_COLLECTED";
                rest -= part;
            }
        }

        source.AllocatedAmount += pay;
        txn.AllocatedAmount += pay;
        claim.PaidAmount += pay;
        var d = await db.Set<FctDisbursement>().FirstAsync(x => x.DisbursementId == claim.DisbursementId, cancellationToken);
        if (claim.PaidAmount >= claim.TotalAmount)
        {
            claim.StatusCode = FctCustomerClaim.Paid;
            claim.PaidAt = clock.UtcNow;
            d.ClaimStatusCode = Paid;
        }
        else
        {
            claim.StatusCode = FctCustomerClaim.PartiallyPaid;
            d.ClaimStatusCode = PartiallyPaid;
        }

        await db.SaveChangesAsync(cancellationToken);
        var held = await db.Set<FctRepaymentSource>().Where(x => x.RepaymentTxnId == txn.RepaymentTxnId && x.StatusCode == "ACTIVE")
            .SumAsync(x => (decimal?)x.SourceAmount, cancellationToken) ?? 0;
        txn.StatusCode = txn.AllocatedAmount <= 0 ? "DRAFT" : txn.AllocatedAmount >= held ? "ALLOCATED" : "PARTIALLY_ALLOCATED";

        payment.GlTranSetId = await PostAsync(claim, d, payment, paidByCode, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var message = claim.StatusCode == FctCustomerClaim.Paid
            ? $"Claim {claim.ClaimNumber} is fully paid ({payment.GlTranSetId}). The disbursement request can now be completed."
            : $"{pay:#,##0.00} paid on claim {claim.ClaimNumber} ({payment.GlTranSetId}); {claim.TotalAmount - claim.PaidAmount:#,##0.00} is still to be paid.";
        return new FctClaimPaidDto(await GetAsync(claimId, cancellationToken), message);
    }

    /// <summary>The charges' journals of the rule with the amounts paid now: net DR customer liability / CR each charge's income.</summary>
    private async Task<string> PostAsync(
        FctCustomerClaim claim, FctDisbursement d, FctCustomerClaimPayment payment, Dictionary<string, decimal> paidByCode, CancellationToken cancellationToken)
    {
        var product = await db.Set<AdmProduct>().AsNoTracking().FirstOrDefaultAsync(x => x.ProductId == claim.ProductId, cancellationToken)
            ?? throw new BusinessRuleException("The claim's product was not found.");
        if (string.IsNullOrWhiteSpace(product.GlTranSetCode))
            throw new BusinessRuleException($"Product {product.ProductCode} has no GL Tran Set Code.");
        var date = payment.PaymentDate;
        var plan = claim.ProductEventRuleId is { } ruleId
            ? await rules.PlanOfRuleAsync(ruleId, date, cancellationToken)
            : await process.PlanOfRequestAsync(d, cancellationToken);
        if (plan is null) throw new BusinessRuleException($"The product rule of the charges is not in effect on {date:yyyy-MM-dd}.");

        var missing = paidByCode.Keys.Where(code => !plan.Lines.Any(l => l.ChargeCode == code)).ToList();
        if (missing.Count > 0)
            throw new BusinessRuleException($"Charges {string.Join(", ", missing)} have no GL lines on rule {plan.RuleCode}. Run setup-charges-payment.sql.");

        decimal AmountOf(ProductEventGlLine l) =>
            l.ChargeCode is { } code && l.AmountCode is "CHARGE_AMOUNT" or "CHARGE_IMMEDIATE" && paidByCode.TryGetValue(code, out var a) ? a : 0;
        var lines = ProductEventPosting.Build(plan, AmountOf, null);
        try
        {
            return await posting.PostLinesAsync(plan, lines, product.GlTranSetCode.Trim().ToUpperInvariant(), "Customer claim payment", "3100",
                "FACTORING", new ProductActionDocument("FCT_CLAIM_PAYMENT", payment.ClaimPaymentId.ToString(CultureInfo.InvariantCulture),
                    claim.ClaimNumber, date, null,
                    $"Claim {claim.ClaimNumber} of disbursement request {d.DisbursementNumber}: payment of {payment.PaymentAmount:#,##0.00}"),
                cancellationToken);
        }
        catch (ApexProcessException ex)
        {
            throw new BusinessRuleException($"GL posting failed, nothing was paid: {ex.Message}");
        }
    }
}
