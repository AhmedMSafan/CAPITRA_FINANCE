using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm;

/// <summary>App 150722 page 750 "Disbursement Requests": one FCT_DISBURSEMENTS row with the client's name.</summary>
public sealed record CrmDisbursementRequestListDto(
    long DisbursementId, string DisbursementNumber, DateOnly DisbursementDate, string? ClientName, decimal GrossAmount, decimal? NetAmount,
    DateOnly? RepaymentStartDate, int? TenorMonth, string? DisbursementRef, string? DisbursementType, bool WorkflowStarted);

/// <summary>App 150722 page 751 "Disbursement Request Detail".</summary>
public sealed record CrmDisbursementRequestDto(
    long DisbursementId, string DisbursementNumber, DateOnly DisbursementDate, long ClientId, string? ClientName, long? LimitId, long CurrencyId,
    decimal GrossAmount, string? Remarks, long? AgreementId, bool WorkflowStarted);

/// <summary>The page 751 items the user fills.</summary>
public sealed record CrmDisbursementRequestSaveDto(
    DateOnly? DisbursementDate, long? ClientId, long? LimitId, long? CurrencyId, decimal? GrossAmount, string? Remarks);

/// <summary>The "Application Summary" region of page 751: the agreement behind the chosen limit.</summary>
public sealed record CrmAgreementSummaryDto(
    long AgreementId, string AgreementNumber, string ClientName, string CurrencyCode, decimal FacilityLimit, DateTime? ApplicationAt,
    string? RelationshipManager, string? StatusCode);

public sealed record CrmDisbursementRequestSavedDto(long DisbursementId, string DisbursementNumber, string Message);

/// <summary>
/// Customer Disbursement Request (APEX App 150722 pages 750 and 751). Saving a new request numbers it DISB-yyyy-id and adds its
/// invoice batch; Initialize Workflow Cycle starts DISBURSEMENT_REQUEST_WF (FCT_DISBURSEMENT_REQUEST), after which the request is
/// read-only.
/// </summary>
public sealed class CrmDisbursementRequestService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock, WfeRuntime runtime)
{
    public const string WorkflowCode = "DISBURSEMENT_REQUEST_WF";

    /// <summary>Page 750 lists the requests not started (IS_ACTIVE N); this app lists them all, started ones marked.</summary>
    public async Task<IReadOnlyList<CrmDisbursementRequestListDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await (
            from d in db.Set<FctDisbursement>().AsNoTracking()
            join c in db.Set<CrmClient>().AsNoTracking() on d.ClientId equals c.ClientId into cs
            from c in cs.DefaultIfEmpty()
            orderby d.DisbursementId descending
            select new CrmDisbursementRequestListDto(
                d.DisbursementId, d.DisbursementNumber, d.DisbursementDate, c != null ? c.ClientName : null, d.GrossAmount, d.NetAmount,
                d.RepaymentStartDate, d.TenorMonth, d.DisbursementRef, d.DisbursementType, d.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<CrmDisbursementRequestDto> GetAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await db.Set<FctDisbursement>().AsNoTracking().FirstOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement request {disbursementId} was not found.");
        var clientName = await db.Set<CrmClient>().AsNoTracking()
            .Where(x => x.ClientId == d.ClientId).Select(x => x.ClientName).FirstOrDefaultAsync(cancellationToken);
        return new CrmDisbursementRequestDto(
            d.DisbursementId, d.DisbursementNumber, d.DisbursementDate, d.ClientId, clientName, d.LimitId, d.CurrencyId, d.GrossAmount, d.Remarks,
            d.AgreementId, d.IsActive);
    }

    /// <summary>P751_CLIENT_ID: the clients that are not prospects, by name.</summary>
    public async Task<IReadOnlyList<CrmOptionDto>> ClientsAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Set<CrmClient>().AsNoTracking()
            .Where(x => x.ClientType == null || x.ClientType != "PROSPECT")
            .OrderBy(x => x.ClientName)
            .Select(x => new { x.ClientId, x.ClientName })
            .ToListAsync(cancellationToken);
        return rows.Select(x => new CrmOptionDto(x.ClientId.ToString(CultureInfo.InvariantCulture), $"{x.ClientName} -- {x.ClientId}")).ToList();
    }

    /// <summary>P751_LIMIT_ID: the client's active credit limits, shown by available amount, largest first.</summary>
    public async Task<IReadOnlyList<CrmOptionDto>> LimitsAsync(long clientId, CancellationToken cancellationToken)
    {
        var rows = await db.Set<FctCreditLimit>().AsNoTracking()
            .Where(x => x.ClientId == clientId && x.IsActive)
            .OrderByDescending(x => x.AvailableAmount)
            .Select(x => new { x.CreditLimitId, x.AvailableAmount })
            .ToListAsync(cancellationToken);
        return rows.Select(x => new CrmOptionDto(
            x.CreditLimitId.ToString(CultureInfo.InvariantCulture),
            x.AvailableAmount is { } amount ? Math.Round(amount, 0, MidpointRounding.AwayFromZero).ToString("#,##0", CultureInfo.InvariantCulture) : string.Empty))
            .ToList();
    }

    /// <summary>P751_CURRENCY_ID: LOV LKP_CURRENCIES.CURRENCY_CODE.</summary>
    public async Task<IReadOnlyList<CrmOptionDto>> CurrenciesAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Set<CrmLkpCurrency>().AsNoTracking()
            .OrderBy(x => x.CurrencyCode)
            .Select(x => new { x.CurrencyId, x.CurrencyCode })
            .ToListAsync(cancellationToken);
        return rows.Select(x => new CrmOptionDto(x.CurrencyId.ToString(CultureInfo.InvariantCulture), x.CurrencyCode)).ToList();
    }

    /// <summary>The FetchDetails action: the limit's agreement, shown in the Application Summary; null when there is none.</summary>
    public async Task<CrmAgreementSummaryDto?> AgreementSummaryAsync(long limitId, CancellationToken cancellationToken)
    {
        var agreementId = await db.Set<FctCreditLimit>().AsNoTracking()
            .Where(x => x.CreditLimitId == limitId).Select(x => x.AgreementId).FirstOrDefaultAsync(cancellationToken);
        return agreementId is { } id ? await SummaryAsync(id, cancellationToken) : null;
    }

    public async Task<CrmAgreementSummaryDto?> SummaryAsync(long agreementId, CancellationToken cancellationToken)
    {
        return await (
            from a in db.Set<FctAgreement>().AsNoTracking()
            join c in db.Set<CrmClient>().AsNoTracking() on a.ClientId equals c.ClientId
            join cu in db.Set<CrmLkpCurrency>().AsNoTracking() on a.CurrencyId equals cu.CurrencyId
            where a.AgreementId == agreementId
            select new CrmAgreementSummaryDto(
                a.AgreementId, a.AgreementNumber, c.ClientName, cu.CurrencyCode, a.FacilityLimit, a.ApplicationAt, a.RelationshipManager, a.StatusCode))
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>CREATE: the form insert with the page's hidden defaults, then GENERATE_DISBURSEMENT_NUMBER and Create Invoice Batch.</summary>
    public async Task<CrmDisbursementRequestSavedDto> CreateAsync(CrmDisbursementRequestSaveDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var today = GlTime.Today(clock);
        var d = new FctDisbursement
        {
            DisbursementNumber = "TMP-" + clock.UtcNow.ToString("HHmmss", CultureInfo.InvariantCulture),
            DisbursementType = "ADVANCE",
            PaymentMethod = "CHEQUE",
            PaymentStatus = "PENDING",
            StatusId = 1,
            FundingRequestId = 1,
            NetAmount = 0,
            IsActive = false,
        };
        await ApplyAsync(d, dto, cancellationToken);
        db.Add(d);
        await db.SaveChangesAsync(cancellationToken);

        d.DisbursementNumber = Number(d.DisbursementId, today);
        await AddInvoiceBatchAsync(d, today, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return new CrmDisbursementRequestSavedDto(d.DisbursementId, d.DisbursementNumber, "Action Processed.");
    }

    /// <summary>SAVE (Apply Changes): the form update, then GENERATE_DISBURSEMENT_NUMBER.</summary>
    public async Task<CrmDisbursementRequestDto> UpdateAsync(long disbursementId, CrmDisbursementRequestSaveDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var d = await EditableAsync(disbursementId, cancellationToken);
        await ApplyAsync(d, dto, cancellationToken);
        d.DisbursementNumber = Number(d.DisbursementId, GlTime.Today(clock));
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(disbursementId, cancellationToken);
    }

    /// <summary>DELETE: the request with its invoice batches (FK_IBATCH_AGRMT would refuse the request alone).</summary>
    public async Task DeleteAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await EditableAsync(disbursementId, cancellationToken);
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        var batches = await db.Set<FctInvoiceBatch>().Where(x => x.DisbursementId == disbursementId).ToListAsync(cancellationToken);
        db.Set<FctInvoiceBatch>().RemoveRange(batches);
        db.Set<FctDisbursement>().Remove(d);
        await db.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// START (Initialize Workflow Cycle): UPDATE_STATUS (IS_ACTIVE Y), the form update, GENERATE_DISBURSEMENT_NUMBER, then START_WF,
    /// which is FCT_DISBURSEMENT_REQUEST / WFE_OPERATIONS_PKG.SUBMIT_DISBURSEMENT_REQUEST.
    /// </summary>
    public async Task<CrmDisbursementRequestSavedDto> StartWorkflowAsync(long disbursementId, CrmDisbursementRequestSaveDto dto, CancellationToken cancellationToken)
    {
        Validate(dto);
        var d = await EditableAsync(disbursementId, cancellationToken);
        await using var tx = await db.BeginTransactionAsync(cancellationToken);
        d.IsActive = true;
        await ApplyAsync(d, dto, cancellationToken);
        d.DisbursementNumber = Number(d.DisbursementId, GlTime.Today(clock));
        await db.SaveChangesAsync(cancellationToken);

        var clientName = await db.Set<CrmClient>().AsNoTracking()
            .Where(x => x.ClientId == d.ClientId).Select(x => x.ClientName).FirstOrDefaultAsync(cancellationToken);
        var id = d.DisbursementId.ToString(CultureInfo.InvariantCulture);
        var reference = $"DISB-{id}";
        var variables = new Dictionary<string, string?>
        {
            ["entity_id"] = id,
            ["entity_type"] = "DISBURSEMENT_REQUEST",
            ["entity_ref"] = reference,
            ["function_code"] = "NEW_DISBURSEMENT_REQUEST",
            ["client_id"] = d.ClientId.ToString(CultureInfo.InvariantCulture),
            ["client_name"] = clientName,
            ["amount"] = d.GrossAmount.ToString("0.00", CultureInfo.InvariantCulture),
            ["currency_code"] = "EGP",
            ["module_code"] = "FACTORING",
        };
        await runtime.StartAsync(WorkflowCode, reference, variables, currentUser.UserName ?? "SYSTEM", cancellationToken);
        await tx.CommitAsync(cancellationToken);
        return new CrmDisbursementRequestSavedDto(d.DisbursementId, d.DisbursementNumber,
            $"Disbursement request {d.DisbursementNumber} saved and workflow {WorkflowCode} started successfully.");
    }

    /// <summary>The required items of page 751.</summary>
    private static void Validate(CrmDisbursementRequestSaveDto dto)
    {
        var errors = new Dictionary<string, string[]>();
        if (dto.DisbursementDate is null) errors["disbursementDate"] = ["Disbursement Date must have some value."];
        if (dto.ClientId is null) errors["clientId"] = ["Client Id must have some value."];
        if (dto.CurrencyId is null) errors["currencyId"] = ["Currency Id must have some value."];
        if (dto.GrossAmount is null) errors["grossAmount"] = ["Invoices Gross Amount must have some value."];
        if (errors.Count > 0) throw new BusinessValidationException(errors);
    }

    private async Task<FctDisbursement> EditableAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await db.Set<FctDisbursement>().FirstOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement request {disbursementId} was not found.");
        if (d.IsActive)
            throw new BusinessRuleException($"The workflow of disbursement request {d.DisbursementNumber} has started; it can no longer be changed.");
        return d;
    }

    /// <summary>The form items, with the agreement values FetchDetails takes from a newly chosen limit.</summary>
    private async Task ApplyAsync(FctDisbursement d, CrmDisbursementRequestSaveDto dto, CancellationToken cancellationToken)
    {
        if (dto.LimitId is { } limitId && limitId != d.LimitId)
        {
            var agreementId = await db.Set<FctCreditLimit>().AsNoTracking()
                .Where(x => x.CreditLimitId == limitId).Select(x => (long?)x.AgreementId).FirstOrDefaultAsync(cancellationToken)
                ?? throw BusinessValidationException.For("limitId", "The selected limit was not found.");
            var agreement = await db.Set<FctAgreement>().AsNoTracking()
                .Where(x => x.AgreementId == agreementId)
                .Select(x => new { x.ApprovedAdvRate, x.SpreadPct, x.RecommendedTenorMth })
                .FirstOrDefaultAsync(cancellationToken);
            d.AgreementId = agreementId;
            d.AdvanceRatePct = agreement?.ApprovedAdvRate;
            d.SpreadRate = agreement?.SpreadPct;
            d.TenorMonth = agreement?.RecommendedTenorMth;
        }

        d.DisbursementDate = dto.DisbursementDate!.Value;
        d.ClientId = dto.ClientId!.Value;
        d.LimitId = dto.LimitId;
        d.CurrencyId = dto.CurrencyId!.Value;
        d.GrossAmount = dto.GrossAmount!.Value;
        d.Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? null : dto.Remarks.Trim();
    }

    /// <summary>GENERATE_DISBURSEMENT_NUMBER: DISB-yyyy-id, the id padded to three digits.</summary>
    private static string Number(long id, DateOnly today) =>
        $"DISB-{today.Year.ToString(CultureInfo.InvariantCulture)}-{id.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0')}";

    /// <summary>Create Invoice Batch: the client's next batch, BTCH-id-n, holding the request's gross amount.</summary>
    private async Task AddInvoiceBatchAsync(FctDisbursement d, DateOnly today, CancellationToken cancellationToken)
    {
        var n = await db.Set<FctInvoiceBatch>().CountAsync(x => x.ClientId == d.ClientId, cancellationToken) + 1;
        var clientName = await db.Set<CrmClient>().AsNoTracking()
            .Where(x => x.ClientId == d.ClientId).Select(x => x.ClientName).FirstOrDefaultAsync(cancellationToken)
            ?? throw BusinessValidationException.For("clientId", $"Client {d.ClientId} was not found.");
        db.Add(new FctInvoiceBatch
        {
            BatchNumber = $"BTCH-{d.DisbursementId}-{n}",
            BatchRef = $"REF-{n}",
            BatchName = $"{clientName} - Batch-{n}",
            ClientId = d.ClientId,
            BatchDate = today,
            BatchType = "BULK",
            CurrencyId = 1,
            TotalGrossAmount = d.GrossAmount,
            StatusId = 1,
            IsActive = true,
            DisbursementId = d.DisbursementId,
        });
    }
}
