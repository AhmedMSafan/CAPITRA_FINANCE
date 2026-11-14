using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Settings;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Fct;

/// <summary>Calculator inputs; percentages as entered on the screen (90 means 90%).</summary>
public sealed record FctCalculationRequest(
    decimal? RequestedLimit, decimal? ExpectedVolume, decimal? AverageTenorDays, decimal? NumDebtors, decimal? EstTransactionsPerYear,
    string? RiskBand, decimal? BaseAdvanceRatio, decimal? DilutionRate, decimal? RiskBufferOverride, decimal? LargestConcentration,
    decimal? CostOfFundsRate, decimal? OperatingSpread, decimal? SetupFeeFixed, decimal? SetupFeePct, decimal? CommissionPct,
    decimal? HandlingFee, decimal? DebtorFee, decimal? DebtorMinimum);

public sealed record FctCalculationResult(
    decimal AdvanceBasePct, decimal AdvanceDilutionPct, decimal AdvanceRiskBufferPct, decimal AdvancePct,
    decimal DiscountCostOfFundsPct, decimal DiscountRiskMarginPct, decimal DiscountOperatingSpreadPct, decimal DiscountTenorAdjustmentPct,
    decimal DiscountConcentrationPct, decimal DiscountRatePct, decimal SpreadOverLcrPct,
    decimal SetupFeeTotal, decimal CommissionAnnual, decimal HandlingAnnual, decimal DebtorFeesTotal, decimal InterestIncome, decimal TotalAnnualIncome);

/// <summary>"Use In Offer": the calculator inputs and results kept on the new application.</summary>
public sealed record FctCreateOfferRequest(
    long ClientId, string? Notes, string? FactoringType, string? RecourseType, decimal RequestedLimit, decimal AverageTenorDays,
    decimal CostOfFundsRate, decimal OperatingSpread, string? RiskBand,
    decimal AdvancePct, decimal AdvanceDilutionPct, decimal DiscountRatePct, decimal SpreadOverLcrPct);

public sealed record FctCreateOfferResult(long OfferId, string OfferRef, string StatusCode, bool ExistingDraft, string Message);

public sealed record FctOfferStatusRequest(string? StatusCode);

/// <summary>"Start Agreement Workflow": Track is NORMAL (FACTORING_LIMIT_APPROVAL, the default) or FAST (FACTORING_FAST_TRACK).</summary>
public sealed record FctStartWorkflowRequest(string? Track);

public sealed record FctStartWorkflowResult(
    long AgreementId, string AgreementNumber, long OfferId, string OfferRef, string OfferStatusCode, string Track, string WorkflowCode, string Message);

/// <summary>
/// The two approval tracks a factoring agreement can run (Ahmed, 3 Nov): the user picks one for every deal when starting
/// the workflow. The track is the agreement's workflow instance's definition, so it is never stored twice.
/// </summary>
public static class FctTracks
{
    public const string Normal = "NORMAL";
    public const string Fast = "FAST";
    public const string NormalWorkflowCode = "FACTORING_LIMIT_APPROVAL";
    public const string FastWorkflowCode = "FACTORING_FAST_TRACK";

    public static string WorkflowCode(string track) => track == Fast ? FastWorkflowCode : NormalWorkflowCode;

    /// <summary>NORMAL or FAST from a workflow code; null when the code is not a factoring track (or no workflow started).</summary>
    public static string? FromWorkflowCode(string? workflowCode) => workflowCode switch
    {
        FastWorkflowCode => Fast,
        NormalWorkflowCode => Normal,
        _ => null,
    };

    /// <summary>NORMAL when empty; anything other than NORMAL or FAST is refused.</summary>
    public static string Parse(string? track)
    {
        var value = string.IsNullOrWhiteSpace(track) ? Normal : track.Trim().ToUpperInvariant();
        return value is Normal or Fast ? value : throw new BusinessRuleException("Choose the Normal track or the Fast track.");
    }
}

/// <summary>
/// Factoring applications (FCT_OFFERS), from FINOVA_FINANCEING: FactoringPricingService (the calculator), FactoringOfferService
/// (create from the calculator, change the status) and FctAgreementService.GenerateFromOfferAsync (the agreement and its workflow).
/// </summary>
public sealed class FctApplicationService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock, WfeRuntime runtime, IModuleSettings moduleSettings)
{
    /// <summary>The statuses the workspace can give an application; APPLICATION is set when the workflow starts.</summary>
    public static readonly string[] Statuses = ["DRAFT", "ACCEPTED", "REJECTED", "SUBMITTED"];

    /// <summary>The statuses of an open offer: one that is still being worked on and has no agreement yet.</summary>
    public static readonly string[] OpenStatuses = ["DRAFT", "SUBMITTED", "ACCEPTED"];

    // ---------------------------------------------------------------- calculator (FactoringPricingService)

    /// <summary>The calculator with the minimum advance rate of Module Settings (FCT.MIN_ADVANCE_PCT, 50% by default).</summary>
    public async Task<FctCalculationResult> CalculateAsync(FctCalculationRequest r, CancellationToken cancellationToken) =>
        Calculate(r, await moduleSettings.DecimalAsync(ModuleSettingCatalog.Codes.FctMinAdvancePct, cancellationToken) / 100m);

    /// <summary>FactoringPricingService; minAdvance is a fraction (0.50 is 50%).</summary>
    public static FctCalculationResult Calculate(FctCalculationRequest r, decimal minAdvance)
    {
        var baseAdvance = Fraction(r.BaseAdvanceRatio);
        var dilution = Fraction(r.DilutionRate);
        var bufferOverride = Fraction(r.RiskBufferOverride);
        var concentration = Fraction(r.LargestConcentration);
        var costOfFunds = Fraction(r.CostOfFundsRate);
        var operatingSpread = Fraction(r.OperatingSpread);
        var setupFeePct = Fraction(r.SetupFeePct);
        var commissionPct = Fraction(r.CommissionPct);

        var riskBuffer = bufferOverride ?? RiskBuffer(r.RiskBand, concentration);
        var advance = Math.Max(minAdvance, (baseAdvance ?? 0m) - (dilution ?? 0m) - riskBuffer);

        var riskMargin = RiskMargin(r.RiskBand);
        var tenorAdjustment = Math.Max(0m, ((r.AverageTenorDays ?? 90m) - 90m) / 30m * 0.0025m);
        var concentrationSurcharge = (concentration ?? 0m) > 0.25m ? 0.005m : 0m;
        var discountRate = (costOfFunds ?? 0m) + riskMargin + (operatingSpread ?? 0m) + tenorAdjustment + concentrationSurcharge;
        var spreadOverLcr = discountRate - (costOfFunds ?? 0m);

        var setupTotal = (r.SetupFeeFixed ?? 0m) + (setupFeePct ?? 0m) * (r.RequestedLimit ?? 0m);
        var commissionAnnual = (commissionPct ?? 0m) * (r.ExpectedVolume ?? 0m);
        var handlingAnnual = (r.HandlingFee ?? 0m) * (r.EstTransactionsPerYear ?? 0m);
        var debtorTotal = Math.Max((r.DebtorFee ?? 0m) * (r.NumDebtors ?? 0m), r.DebtorMinimum ?? 0m);
        var interestIncome = advance * (r.ExpectedVolume ?? 0m) * discountRate * ((r.AverageTenorDays ?? 0m) / 365m);
        // business rule confirmed in FINOVA: commission + handling + debtor fees + setup fee
        var totalAnnualIncome = commissionAnnual + handlingAnnual + debtorTotal + setupTotal;

        return new FctCalculationResult(
            Pct(baseAdvance ?? 0m), Pct(-(dilution ?? 0m)), Pct(-riskBuffer), Pct(advance),
            Pct(costOfFunds ?? 0m), Pct(riskMargin), Pct(operatingSpread ?? 0m), Pct(tenorAdjustment), Pct(concentrationSurcharge),
            Pct(discountRate), Pct(spreadOverLcr),
            setupTotal, commissionAnnual, handlingAnnual, debtorTotal, interestIncome, totalAnnualIncome);
    }

    private static decimal? Fraction(decimal? percentage) => percentage / 100m;

    private static decimal Pct(decimal fraction) => fraction * 100m;

    private static decimal RiskBuffer(string? riskBand, decimal? largestConcentration) =>
        Band(riskBand) switch { "HIGH" => 0.07m, "LOW" => 0.02m, _ => 0.04m }
        + Math.Max(0m, ((largestConcentration ?? 0m) - 0.25m) * 0.10m);

    private static decimal RiskMargin(string? riskBand) =>
        Band(riskBand) switch { "HIGH" => 0.035m, "LOW" => 0.0125m, _ => 0.0225m };

    private static string Band(string? riskBand) => riskBand?.Trim().ToUpperInvariant() ?? string.Empty;

    // ---------------------------------------------------------------- applications

    /// <summary>
    /// "Use In Offer": a new DRAFT application for the customer. Unless Module Settings lets a client have several open offers
    /// (FCT.MULTIPLE_OPEN_OFFERS), the customer's open draft is loaded instead, and an offer already submitted or accepted stops a new one.
    /// </summary>
    public async Task<FctCreateOfferResult> CreateFromCalculatorAsync(FctCreateOfferRequest r, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(r.FactoringType))
            throw new BusinessRuleException("Factoring type is required.");
        if (string.IsNullOrWhiteSpace(r.RecourseType))
            throw new BusinessRuleException("Recourse type is required.");
        if (r.RequestedLimit <= 0)
            throw new BusinessRuleException("Requested limit must be greater than zero.");
        if (!await db.Set<CrmClient>().AnyAsync(x => x.ClientId == r.ClientId, cancellationToken))
            throw new NotFoundException($"Customer {r.ClientId} was not found.");

        if (!await moduleSettings.BoolAsync(ModuleSettingCatalog.Codes.FctMultipleOpenOffers, cancellationToken))
        {
            var open = await db.Set<FctOffer>().AsNoTracking()
                .Where(x => x.ClientId == r.ClientId && OpenStatuses.Contains(x.StatusCode!) && (x.AgreementNo == null || x.AgreementNo == ""))
                .OrderBy(x => x.StatusCode == "DRAFT" ? 0 : 1).ThenByDescending(x => x.OfferId).FirstOrDefaultAsync(cancellationToken);
            if (open is { StatusCode: "DRAFT" })
                return new FctCreateOfferResult(open.OfferId, open.OfferRef, "DRAFT", true, $"Existing draft offer {open.OfferRef} loaded.");
            if (open is not null)
                throw new BusinessRuleException(
                    $"The customer already has open offer {open.OfferRef} ({open.StatusCode!.ToLowerInvariant()}). A client can have one open offer at a time (Administration > Module Settings).");
        }

        var now = clock.UtcNow;
        var offer = new FctOffer
        {
            OfferRef = $"OFF-{now:yyyyMMdd}-{Random.Shared.Next(1, 10000):0000}",
            ClientId = r.ClientId,
            FactoringType = r.FactoringType.Trim(),
            RecourseType = r.RecourseType.Trim(),
            // FINOVA_FINANCEING creates every application in currency 1
            CurrencyId = 1,
            RequestedLimit = r.RequestedLimit,
            AdvanceRatePct = r.AdvancePct,
            MaxAdvanceRatePct = 90,
            MinAdvanceRatePct = 70,
            ReserveRatePct = 90 - r.AdvancePct,
            RateTypeId = null,
            BaseRatePct = r.CostOfFundsRate,
            SpreadPct = r.OperatingSpread,
            EffectiveRatePct = r.DiscountRatePct,
            MarginPct = r.SpreadOverLcrPct,
            AvgTenorDays = (int)Math.Round(r.AverageTenorDays),
            MaxDebtorConcentrationPct = 25,
            DilutionReservePct = r.AdvanceDilutionPct,
            RiskBand = r.RiskBand,
            RelationshipManager = currentUser.UserName,
            Notes = r.Notes,
            IsActive = true,
            OfferAt = now,
            StatusCode = "DRAFT",
        };
        db.Add(offer);
        await db.SaveChangesAsync(cancellationToken);
        return new FctCreateOfferResult(offer.OfferId, offer.OfferRef, "DRAFT", false, $"Offer {offer.OfferRef} created successfully.");
    }

    /// <summary>The workspace's "Save Status". An application that has an agreement keeps its status.</summary>
    public async Task SetStatusAsync(long offerId, FctOfferStatusRequest r, CancellationToken cancellationToken)
    {
        var status = r.StatusCode?.Trim().ToUpperInvariant() ?? string.Empty;
        if (!Statuses.Contains(status))
            throw new BusinessRuleException($"Invalid status '{r.StatusCode}'.");

        var offer = await db.Set<FctOffer>().FirstOrDefaultAsync(x => x.OfferId == offerId, cancellationToken)
            ?? throw new NotFoundException($"Factoring offer {offerId} was not found.");
        if (!string.IsNullOrEmpty(offer.AgreementNo))
            throw new BusinessRuleException($"Application {offer.OfferRef} already has agreement {offer.AgreementNo}.");

        offer.StatusCode = status;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// "Start Agreement Workflow": creates the agreement (APP-{id}) from an ACCEPTED application and starts
    /// the chosen track's workflow for it (FACTORING_LIMIT_APPROVAL or FACTORING_FAST_TRACK), all or nothing; the application
    /// becomes APPLICATION.
    /// </summary>
    public async Task<FctStartWorkflowResult> StartWorkflowAsync(long offerId, FctStartWorkflowRequest request, CancellationToken cancellationToken)
    {
        var track = FctTracks.Parse(request.Track);
        var workflowCode = FctTracks.WorkflowCode(track);

        await using var tx = await db.BeginTransactionAsync(cancellationToken);

        var offer = await db.Set<FctOffer>().FirstOrDefaultAsync(x => x.OfferId == offerId, cancellationToken)
            ?? throw new NotFoundException($"Factoring offer {offerId} was not found.");
        if (!string.IsNullOrEmpty(offer.AgreementNo))
            throw new BusinessRuleException($"Application {offer.OfferRef} already has agreement {offer.AgreementNo}.");
        if (!string.Equals(offer.StatusCode, "ACCEPTED", StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Save ACCEPTED status first.");

        var client = await db.Set<CrmClient>().AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == offer.ClientId, cancellationToken)
            ?? throw new NotFoundException($"Client {offer.ClientId} was not found.");

        var user = currentUser.UserName ?? "SYSTEM";
        var now = clock.UtcNow;
        var agreement = new FctAgreement
        {
            AgreementNumber = $"TEMP-{Guid.NewGuid():N}"[..30],
            AgreementRef = offer.OfferRef,
            ClientId = offer.ClientId,
            AgreementType = "LIMIT REQUEST",
            FactoringType = offer.FactoringType,
            RecourseType = offer.RecourseType,
            CurrencyId = 1,
            FacilityLimit = offer.RequestedLimit,
            AdvanceRatePct = offer.AdvanceRatePct,
            ReserveRatePct = offer.ReserveRatePct,
            BaseRatePct = offer.BaseRatePct,
            SpreadPct = offer.SpreadPct,
            MarginPct = offer.MarginPct,
            ApplicationAt = now,
            TenorMonths = offer.MaxLimitTenorMth is { } months ? (int)Math.Round(months) : null,
            MaxDebtorConcentrationPct = offer.MaxDebtorConcentrationPct,
            StatusCode = "DRAFT",
            RelationshipManager = offer.RelationshipManager,
            IsActive = true,
            InstanceId = 0,
            TaskId = 0,
        };
        db.Add(agreement);
        await db.SaveChangesAsync(cancellationToken);

        agreement.AgreementNumber = $"APP-{agreement.AgreementId}";
        offer.StatusCode = "APPLICATION";
        offer.AgreementNo = agreement.AgreementNumber;

        var currencyCode = await db.Set<CrmLkpCurrency>().AsNoTracking()
            .Where(x => x.CurrencyId == agreement.CurrencyId).Select(x => x.CurrencyCode).FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Currency {agreement.CurrencyId} was not found.");

        var reference = $"AGREEMENT-{agreement.AgreementId}";
        var variables = new Dictionary<string, string?>
        {
            ["entity_id"] = agreement.AgreementId.ToString(CultureInfo.InvariantCulture),
            ["entity_type"] = "FACTORY-AGREEMENT",
            ["entity_ref"] = agreement.AgreementNumber,
            ["client_id"] = agreement.ClientId.ToString(CultureInfo.InvariantCulture),
            ["client_number"] = client.ClientNumber,
            ["client_name"] = client.ClientName,
            ["amount"] = agreement.FacilityLimit.ToString("0.00", CultureInfo.InvariantCulture),
            ["currency_code"] = currencyCode.Trim().ToUpperInvariant(),
            ["application_reference"] = reference,
            ["module_code"] = "FACTORING",
        };
        var instance = await runtime.StartAsync(workflowCode, reference, variables, user, cancellationToken);
        agreement.InstanceId = instance.WorkflowInstanceId;
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        return new FctStartWorkflowResult(agreement.AgreementId, agreement.AgreementNumber, offer.OfferId, offer.OfferRef, offer.StatusCode, track, workflowCode,
            $"Factoring application {agreement.AgreementNumber} generated successfully and workflow {workflowCode} started successfully.");
    }
}
