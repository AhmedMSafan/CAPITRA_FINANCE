using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Products;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Fct;
using Capitra.Domain.Modules.Gl;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm;

/// <summary>A calculated charge of the request (FCT_DISBURSEMENT_CHARGES).</summary>
public sealed record CrmDisbursementChargeDto(
    long DisbursementChargeId, string RuleCode, string RuleName, string CalculationMethod, string? BaseCode, decimal? BaseAmount, decimal? RatePercent,
    decimal ChargeAmount, decimal TaxAmount, decimal TotalAmount, string ChargeTimingCode, DateOnly? DueDate, string StatusCode);

/// <summary>A row of the request's current repayment schedule (FCT_REPAYMENTS).</summary>
public sealed record CrmRepaymentRowDto(
    int InstallmentNo, DateOnly DueDate, decimal? OpeningPrincipal, decimal? PrincipalDue, decimal? InterestDue, decimal? TotalInstallment,
    decimal? ClosingPrincipal, decimal? InterestRate, int? DayCount, string StatusCode);

/// <summary>A GL line the request's charges post (journal 1 the charge, journal 2 its payment), from the product event rule.</summary>
public sealed record CrmGlEntryDto(
    int JournalNo, int LineNo, string ChargeCode, string DebitCreditCode, string? AccountCode, string? AccountName, decimal Amount, string? Description);

/// <summary>The processed figures of the request with its charges and current schedule; IsPreview when nothing is saved yet.</summary>
public sealed record CrmDisbursementResultDto(
    string? StatusCode, decimal ApprovedInvoiceAmount, decimal ApprovedAdvanceAmount, decimal? FeeDeducted, decimal? TaxDeducted, decimal? NetAmount,
    IReadOnlyList<CrmDisbursementChargeDto> Charges, IReadOnlyList<CrmRepaymentRowDto> Schedule, IReadOnlyList<CrmGlEntryDto> GlEntries,
    bool IsPreview = false);

public sealed record CrmDisbursementProcessedDto(CrmInvoiceEntryDto Page, CrmDisbursementResultDto Result, string Message);

/// <summary>
/// Page 705 "Process Disbursement Request": FNV_DISBURSEMENT_PROCESS_PKG.process_disbursement. The invoices take the request's
/// advance rate, every invoice must be ELIGIBLE or NOT_ELIGIBLE (with a reason) and one at least ELIGIBLE; then the charges
/// (calculate_charges) and the repayment schedule (generate_repayment_schedule), and the request becomes PROCESSED. The charges
/// come from the Capitra product event rule that the workflow's Invoice Upload step names (finova:ruleCode) on the request's
/// product. The charges are claimed from the customer with Create Claim (FctCustomerClaimService) and paid in Collection.
/// </summary>
public sealed class CrmDisbursementProcessService(
    ICapitraDbContext db, ICurrentUser currentUser, IClock clock, CrmInvoiceEntryService entry, AdmProductEventRuleService rules)
{
    public const string WorkflowCode = "DISBURSEMENT_REQUEST_WF";
    public const string InvoiceUploadStep = "INVOICE_UPLOAD";

    public async Task<CrmDisbursementResultDto> ResultAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await db.Set<FctDisbursement>().AsNoTracking().FirstOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement request {disbursementId} was not found.");
        var charges = await db.Set<FctDisbursementCharge>().AsNoTracking().Where(x => x.DisbursementId == disbursementId)
            .OrderBy(x => x.DisbursementChargeId)
            .Select(x => new CrmDisbursementChargeDto(x.DisbursementChargeId, x.RuleCode, x.RuleName, x.CalculationMethod, x.BaseCode, x.BaseAmount,
                x.RatePercent, x.ChargeAmount, x.TaxAmount, x.TotalAmount, x.ChargeTimingCode, x.DueDate, x.StatusCode))
            .ToListAsync(cancellationToken);
        var schedule = await db.Set<FctRepayment>().AsNoTracking().Where(x => x.DisbursementId == disbursementId && x.IsCurrentVersion)
            .OrderBy(x => x.InstallmentNo)
            .Select(x => new CrmRepaymentRowDto(x.InstallmentNo, x.DueDate, x.OpeningPrincipal, x.PrincipalDue, x.InterestDue, x.TotalInstallment,
                x.ClosingPrincipal, x.InterestRate, x.DayCount, x.StatusCode))
            .ToListAsync(cancellationToken);
        return new CrmDisbursementResultDto(d.StatusCode, d.ApprovedInvoiceAmount, d.ApprovedAdvanceAmount, d.FeeDeducted, d.TaxDeducted, d.NetAmount,
            charges, schedule, await GlEntriesAsync(d, charges, cancellationToken));
    }

    /// <summary>
    /// The charges as soon as the product is chosen (Ahmed, 1 Nov 2026): the page values are saved and the charges calculated as Process
    /// would, inside a transaction that is rolled back, so nothing is kept. Charges already paid or posted are shown as they are.
    /// </summary>
    /// While files are chosen and not saved yet, their rows are part of the preview.
    public async Task<CrmDisbursementResultDto> PreviewAsync(long disbursementId, CrmInvoiceEntrySaveDto dto, CancellationToken cancellationToken,
        CrmUploadFile? invoiceFile = null, CrmUploadFile? chequeFile = null)
    {
        if (await db.Set<FctDisbursementCharge>().AnyAsync(x => x.DisbursementId == disbursementId && x.StatusCode != "CALCULATED", cancellationToken)
            || await LiveClaimAsync(disbursementId, cancellationToken) is not null)
            return await ResultAsync(disbursementId, cancellationToken);
        if (dto.Terms.ProductId is null) return new CrmDisbursementResultDto(null, 0, 0, null, null, null, [], [], [], true);

        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var (d, batch) = await entry.SaveWithFilesCoreAsync(disbursementId, dto, invoiceFile, chequeFile, cancellationToken);
        if (batch is not null) await entry.SyncInvoiceAdvancesAsync(d, batch.BatchId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var invoices = await EligibleInvoicesAsync(d.DisbursementId, cancellationToken);
        var chequeCount = await ChequeCountAsync(d, batch, cancellationToken);
        var custom = await entry.CustomScheduleAsync(d.DisbursementId, cancellationToken);
        var financed = await CalculateChargesAsync(d, invoices, chequeCount, clock.UtcNow, CrmRepaymentSchedule.Maturity(d, custom), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        // the repayment schedule too, when the invoices and terms allow it (Complete builds it the same way)
        if (invoices.Count > 0)
        {
            try
            {
                await GenerateScheduleAsync(d, financed, invoices, custom, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (BusinessRuleException) { }
        }

        var result = await ResultAsync(disbursementId, cancellationToken);
        await tx.RollbackAsync(cancellationToken);
        return result with { StatusCode = null, IsPreview = true };
    }

    /// <summary>
    /// The charges only, kept (Create Claim, Ahmed 2 Nov 2026): calculated from the values as they stand, as the preview does, without
    /// Process's checks of the invoices and cheques; those run at Complete, which refuses when they would change the claimed charges.
    /// </summary>
    internal async Task CalculateChargesOnlyAsync(FctDisbursement d, FctInvoiceBatch? batch, CancellationToken cancellationToken)
    {
        if (batch is not null) await entry.SyncInvoiceAdvancesAsync(d, batch.BatchId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        var invoices = await EligibleInvoicesAsync(d.DisbursementId, cancellationToken);
        var custom = await entry.CustomScheduleAsync(d.DisbursementId, cancellationToken);
        await CalculateChargesAsync(d, invoices, await ChequeCountAsync(d, batch, cancellationToken), clock.UtcNow,
            CrmRepaymentSchedule.Maturity(d, custom), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<int> ChequeCountAsync(FctDisbursement d, FctInvoiceBatch? batch, CancellationToken cancellationToken) =>
        batch is null ? 0 : await entry.Cheques(batch.BatchId, d.DisbursementId)
            .CountAsync(x => x.StatusCode != "CANCELLED" && x.StatusCode != "REPLACED", cancellationToken);

    /// <summary>The GL lines of the charges: the rule's lines of each charge with its amount (none when the rule has no GL set up).</summary>
    private async Task<IReadOnlyList<CrmGlEntryDto>> GlEntriesAsync(
        FctDisbursement d, List<CrmDisbursementChargeDto> charges, CancellationToken cancellationToken)
    {
        if (charges.Count == 0 || d.ProductId is null) return [];
        ProductEventPlan plan;
        try { plan = await PlanAsync(d, cancellationToken); }
        catch (BusinessRuleException) { return []; }

        var amounts = charges.GroupBy(x => x.RuleCode).ToDictionary(x => x.Key, x => x.Sum(c => c.ChargeAmount));
        var lines = plan.Lines.Where(x => x.ChargeCode is not null && amounts.ContainsKey(x.ChargeCode)
                && x.AmountCode is "CHARGE_AMOUNT" or "CHARGE_IMMEDIATE").ToList();
        var codes = lines.Select(x => x.AccountCode).OfType<string>().Distinct().ToList();
        var names = await db.Set<GlAccount>().AsNoTracking().Where(x => codes.Contains(x.AccountCode))
            .Select(x => new { x.AccountCode, x.AccountNameEn }).ToListAsync(cancellationToken);
        var nameOf = names.GroupBy(x => x.AccountCode).ToDictionary(x => x.Key, x => x.First().AccountNameEn);
        var order = charges.Select((c, i) => (c.RuleCode, i)).GroupBy(x => x.RuleCode).ToDictionary(x => x.Key, x => x.First().i);
        return [.. lines.OrderBy(x => x.JournalNo).ThenBy(x => order[x.ChargeCode!]).ThenBy(x => x.LineNo)
            .Select(x => new CrmGlEntryDto(x.JournalNo, x.LineNo, x.ChargeCode!, x.DebitCreditCode, x.AccountCode,
                x.AccountCode is null ? null : nameOf.GetValueOrDefault(x.AccountCode), amounts[x.ChargeCode!], x.Description))];
    }

    /// <summary>Process Disbursement Request: the page is saved first, then processed, in one transaction.</summary>
    public async Task<CrmDisbursementProcessedDto> ProcessAsync(long disbursementId, CrmInvoiceEntrySaveDto dto, CancellationToken cancellationToken)
    {
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var (d, batch) = await entry.SaveCoreAsync(disbursementId, dto, cancellationToken);
        await ProcessCoreAsync(d, batch, cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return new CrmDisbursementProcessedDto(await entry.GetAsync(disbursementId, cancellationToken), await ResultAsync(disbursementId, cancellationToken),
            "Disbursement processed. Charges and repayment schedule were calculated.");
    }

    /// <summary>
    /// process_disbursement inside the caller's transaction (Process, and Complete of the Invoice Upload step): the invoices take the
    /// advance rate and are checked, then the charges and the repayment schedule; the request becomes PROCESSED.
    /// </summary>
    internal async Task ProcessCoreAsync(FctDisbursement d, FctInvoiceBatch? batch, CancellationToken cancellationToken)
    {
        var user = currentUser.UserName ?? "SYSTEM";
        var now = clock.UtcNow;
        if (batch is null) throw new BusinessRuleException("Upload the invoices before completing: the request has no invoice batch yet.");

        await entry.SyncInvoiceAdvancesAsync(d, batch.BatchId, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var statuses = await db.Set<FctInvoice>().AsNoTracking().Where(x => x.BatchId == batch.BatchId)
            .Select(x => new { x.StatusCode, x.IneligibleReason }).ToListAsync(cancellationToken);
        var codes = statuses.Select(x => (Code: CrmRepaymentSchedule.Normalize(x.StatusCode), x.IneligibleReason)).ToList();
        if (codes.Count == 0) throw new BusinessRuleException("Upload at least one invoice before processing.");
        if (codes.Any(x => x.Code is not ("ELIGIBLE" or "NOT_ELIGIBLE")))
            throw new BusinessRuleException("Every invoice must be marked ELIGIBLE or NOT_ELIGIBLE.");
        if (codes.Any(x => x.Code == "NOT_ELIGIBLE" && string.IsNullOrWhiteSpace(x.IneligibleReason)))
            throw new BusinessRuleException("An ineligible reason is required for every NOT_ELIGIBLE invoice.");
        if (!codes.Any(x => x.Code == "ELIGIBLE")) throw new BusinessRuleException("At least one invoice must be ELIGIBLE.");

        var invoices = await EligibleInvoicesAsync(d.DisbursementId, cancellationToken);
        // the schedule comes from the terms (Ahmed, 2 Nov 2026); the customer's cheques must then match it, which Complete checks
        var custom = await entry.CustomScheduleAsync(d.DisbursementId, cancellationToken);
        var financed = await CalculateChargesAsync(d, invoices, await ChequeCountAsync(d, batch, cancellationToken), now,
            CrmRepaymentSchedule.Maturity(d, custom), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await GenerateScheduleAsync(d, financed, invoices, custom, cancellationToken);

        d.StatusCode = "PROCESSED";
        d.ProcessedBy = user;
        d.ProcessingAt = now;
        batch.ValidatedBy = user;
        batch.ValidationAt = now;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>The request's ELIGIBLE invoices: due date and advance amount (the gross amount is read with them).</summary>
    private async Task<List<(CrmScheduleSource Source, decimal Gross)>> EligibleInvoicesAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var rows = await (from i in db.Set<FctInvoice>().AsNoTracking()
                          join b in db.Set<FctInvoiceBatch>() on i.BatchId equals b.BatchId
                          where b.DisbursementId == disbursementId
                          select new { i.StatusCode, i.DueAt, i.ClientInvoiceAt, i.AdvanceAmount, i.GrossAmount }).ToListAsync(cancellationToken);
        return [.. rows.Where(x => CrmRepaymentSchedule.Normalize(x.StatusCode) == "ELIGIBLE")
            .Select(x => (new CrmScheduleSource(DateOnly.FromDateTime((x.DueAt ?? x.ClientInvoiceAt)!.Value), x.AdvanceAmount ?? 0), x.GrossAmount))];
    }

    /// <summary>calculate_charges; returns the FINANCE_WITH_PRINCIPAL total the schedule adds to the principal.</summary>
    private async Task<decimal> CalculateChargesAsync(
        FctDisbursement d, List<(CrmScheduleSource Source, decimal Gross)> invoices, int chequeCount, DateTime now, DateOnly? maturityDate,
        CancellationToken cancellationToken)
    {
        if (d.ProductId is null) throw new BusinessRuleException("PRODUCT_ID is required on the disbursement request.");
        var creditLimit = await db.Set<FctCreditLimit>().AsNoTracking().Where(x => x.CreditLimitId == d.LimitId)
            .Select(x => (decimal?)x.ApprovedAmount).FirstOrDefaultAsync(cancellationToken)
            ?? throw new BusinessRuleException("Choose the Available Limit of the disbursement request.");
        var plan = await PlanAsync(d, cancellationToken);

        var invoiceCount = invoices.Count;
        var approvedInvoice = invoices.Sum(x => x.Gross);
        var approvedAdvance = invoices.Sum(x => x.Source.Amount);
        var maturity = maturityDate ?? CrmRepaymentSchedule.AddMonths(d.DisbursementDate, d.TenorMonth ?? 0);
        var tenorDays = Math.Max(maturity.DayNumber - d.DisbursementDate.DayNumber, 0);
        var net = approvedAdvance - (d.InterestDeducted ?? 0) - (d.OtherDeductions ?? 0);

        // once the charges are claimed from the customer they are kept: a new calculation must give the claim's amounts
        var claim = await LiveClaimAsync(d.DisbursementId, cancellationToken);
        if (claim is null && await db.Set<FctDisbursementCharge>().AnyAsync(x => x.DisbursementId == d.DisbursementId
                && x.StatusCode != "CALCULATED" && x.StatusCode != "CANCELLED" && x.StatusCode != "REVERSED", cancellationToken))
            throw new BusinessRuleException("Charges already collected, waived or otherwise actioned cannot be recalculated.");
        var fresh = new List<FctDisbursementCharge>();

        decimal? BaseOf(string code) => code switch
        {
            "DISBURSEMENT_GROSS_INVOICE_AMOUNT" => d.GrossAmount,
            "DISBURSEMENT_NET_AMOUNT" => net,
            "ELIGIBLE_INVOICE_AMOUNT" => approvedInvoice,
            "ADVANCE_AMOUNT" => approvedAdvance,
            "INVOICE_COUNT" => invoiceCount,
            "CHEQUE_COUNT" => chequeCount,
            "TENOR_DAYS" => tenorDays,
            "APPROVED_LIMIT" => creditLimit,
            _ => throw new BusinessRuleException($"Amount base {code} cannot be used for a factoring disbursement."),
        };

        var extras = await db.Set<AdmProductEventRuleCharge>().AsNoTracking().Where(x => x.ProductEventRuleId == plan.ProductEventRuleId)
            .ToDictionaryAsync(x => x.RuleChargeId, cancellationToken);
        decimal deductedCharge = 0, deductedTax = 0, financed = 0;
        foreach (var c in plan.Charges)
        {
            var baseAmount = c.CalculationMethodCode == "FIXED" && c.AmountBaseCode is null ? 0 : c.AmountBaseCode is null ? (decimal?)null : BaseOf(c.AmountBaseCode);
            var units = c.AmountBaseCode switch
            {
                "INVOICE_COUNT" => invoiceCount,
                "CHEQUE_COUNT" => chequeCount,
                "TENOR_DAYS" => tenorDays,
                _ => baseAmount ?? 0,
            };
            var priced = c.CalculationMethodCode == "PER_UNIT"
                ? ProductEventPosting.Calculate(c with { PerUnitAmount = c.PerUnitAmount * units }, BaseOf, 1)
                : ProductEventPosting.Calculate(c, BaseOf, 0);
            if (priced.Problem is not null)
                throw new BusinessRuleException($"Charge {c.ChargeCode} of rule {plan.RuleCode}: {priced.Problem}.");

            var scale = Math.Clamp(c.RoundingScale, 0, 6);
            var tax = Math.Round(priced.Amount * c.TaxRate / 100, scale, MidpointRounding.AwayFromZero);
            var x = extras[c.RuleChargeId];
            fresh.Add(new FctDisbursementCharge
            {
                DisbursementId = d.DisbursementId,
                ProductEventRuleId = plan.ProductEventRuleId,
                ProductId = d.ProductId.Value,
                RuleChargeId = c.RuleChargeId,
                RuleCode = c.ChargeCode,
                RuleName = c.ChargeNameEn,
                CalculationMethod = c.CalculationMethodCode,
                BaseCode = c.AmountBaseCode,
                BaseAmount = baseAmount,
                Units = units,
                FixedAmount = c.FixedAmount,
                RatePercent = c.ChargeRate,
                PerUnitAmount = c.PerUnitAmount,
                SpreadPercent = x.SpreadRate,
                DayCountBasis = x.DayCountBasis,
                ChargeAmount = priced.Amount,
                TaxRatePercent = c.TaxRate,
                TaxAmount = tax,
                TotalAmount = priced.Amount + tax,
                CurrencyId = d.CurrencyId,
                ChargeTimingCode = c.ChargeTimingCode,
                IncomeRecognitionCode = c.IncomeRecognitionCode,
                DueDate = d.DisbursementDate.AddDays(x.DueDays),
                StatusCode = "CALCULATED",
                CalculationAt = now,
            });
            if (c.ChargeTimingCode == "DEDUCT_FROM_DISBURSEMENT")
            {
                deductedCharge += priced.Amount;
                deductedTax += tax;
            }
            else if (c.ChargeTimingCode == "FINANCE_WITH_PRINCIPAL")
                financed += priced.Amount + tax;
        }

        if (claim is null)
        {
            db.Set<FctDisbursementCharge>().RemoveRange(
                await db.Set<FctDisbursementCharge>().Where(x => x.DisbursementId == d.DisbursementId).ToListAsync(cancellationToken));
            db.Set<FctDisbursementCharge>().AddRange(fresh);
        }
        else
        {
            var claimed = await db.Set<FctCustomerClaimLine>().AsNoTracking().Where(x => x.ClaimId == claim.ClaimId)
                .ToDictionaryAsync(x => x.ChargeCode, x => x.TotalAmount, cancellationToken);
            var now2 = fresh.Where(x => x.TotalAmount > 0).GroupBy(x => x.RuleCode).ToDictionary(x => x.Key, x => x.Sum(c => c.TotalAmount));
            if (claimed.Count != now2.Count || claimed.Any(x => now2.GetValueOrDefault(x.Key) != x.Value))
                throw new BusinessRuleException(
                    $"The charges are claimed from the customer (claim {claim.ClaimNumber}) and these changes would change them. Cancel the claim first.");
        }

        d.ApprovedInvoiceAmount = approvedInvoice;
        d.ApprovedAdvanceAmount = approvedAdvance;
        d.FeeDeducted = Math.Round(deductedCharge, 3, MidpointRounding.AwayFromZero);
        d.TaxDeducted = Math.Round(deductedTax, 3, MidpointRounding.AwayFromZero);
        d.NetAmount = Math.Round(approvedAdvance - deductedCharge - deductedTax - (d.InterestDeducted ?? 0) - (d.OtherDeductions ?? 0), 3,
            MidpointRounding.AwayFromZero);
        return financed;
    }

    /// <summary>The request's claim of its charges that is not cancelled, if any.</summary>
    private Task<FctCustomerClaim?> LiveClaimAsync(long disbursementId, CancellationToken cancellationToken) =>
        db.Set<FctCustomerClaim>().AsNoTracking().Where(x => x.DisbursementId == disbursementId && x.StatusCode != FctCustomerClaim.Cancelled)
            .OrderByDescending(x => x.ClaimId).FirstOrDefaultAsync(cancellationToken);

    /// <summary>The product event rule the Invoice Upload step names, on the request's product and date.</summary>
    internal Task<ProductEventPlan> PlanOfRequestAsync(FctDisbursement d, CancellationToken cancellationToken) => PlanAsync(d, cancellationToken);

    /// <summary>The product event rule the Invoice Upload step names, on the request's product and date.</summary>
    private async Task<ProductEventPlan> PlanAsync(FctDisbursement d, CancellationToken cancellationToken)
    {
        var reference = $"DISB-{d.DisbursementId}";
        var xml = await (from i in db.Set<WfeWorkflowInstance>().AsNoTracking()
                         join w in db.Set<WfeWorkflowDefinition>() on i.WorkflowDefinitionId equals w.WorkflowDefinitionId
                         where w.WorkflowCode == WorkflowCode && i.BusinessReference == reference
                         orderby i.WorkflowInstanceId descending
                         select w.BpmnXml).FirstOrDefaultAsync(cancellationToken)
            ?? await db.Set<WfeWorkflowDefinition>().AsNoTracking().Where(x => x.WorkflowCode == WorkflowCode).Select(x => x.BpmnXml)
                .FirstOrDefaultAsync(cancellationToken);
        var ruleCode = xml is null ? null : BpmnModel.StepByCode(xml, InvoiceUploadStep)?.RuleCode?.Trim();
        if (string.IsNullOrEmpty(ruleCode))
            throw new BusinessRuleException(
                "The Invoice Upload step of DISBURSEMENT_REQUEST_WF names no product event rule: set its rule code in the workflow designer.");
        var productCode = await db.Set<AdmProduct>().AsNoTracking().Where(x => x.ProductId == d.ProductId).Select(x => x.ProductCode)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new BusinessRuleException($"Product {d.ProductId} was not found in the Product Catalog.");
        return await rules.PlanByCodeAsync(productCode, ruleCode, d.DisbursementDate, cancellationToken)
            ?? throw new BusinessRuleException($"No active product event rule {ruleCode} exists for product {productCode} on {d.DisbursementDate:yyyy-MM-dd}.");
    }

    /// <summary>generate_repayment_schedule: the earlier versions stop being current and the new one is added.</summary>
    private async Task GenerateScheduleAsync(
        FctDisbursement d, decimal financed, List<(CrmScheduleSource Source, decimal Gross)> invoices, IReadOnlyList<CrmScheduleSource> custom,
        CancellationToken cancellationToken)
    {
        var current = await db.Set<FctRepayment>().Where(x => x.DisbursementId == d.DisbursementId).ToListAsync(cancellationToken);
        var version = (current.Max(x => x.ScheduleVersion) ?? 0) + 1;
        foreach (var r in current.Where(x => x.IsCurrentVersion))
            r.IsCurrentVersion = false;
        foreach (var row in CrmRepaymentSchedule.Build(d, financed, [.. invoices.Select(x => x.Source)], custom, version))
            db.Add(row);
    }
}
