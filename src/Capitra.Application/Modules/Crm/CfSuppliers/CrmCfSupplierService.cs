using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Scr;
using Capitra.Application.Modules.Wfe;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Lnd;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm.CfSuppliers;

public sealed record CrmCfOptionDto(string Code, string Name);

public sealed record CrmCfBankAccountOptionDto(long BankAccountId, string BankName, string? AccountNumber, string? Iban, string StatusCode);

public sealed record CrmCfClientOptionDto(long ClientId, string ClientNumber, string ClientName);

public sealed record CrmCfPartyDto(
    long PartyId, string PartyTypeCode, string FullName, string? NationalIdNo, string? Nationality, decimal? OwnershipPct,
    string? ScreeningResultCode, string? ScreenedBy, DateTime? ScreenedAt);

public sealed record CrmCfSupplierListItemDto(
    long ApplicationId, string ApplicationRef, long ClientId, string ClientNumber, string ClientName, string StatusCode, string? RiskRatingCode,
    string? OpenStepCode, string? OpenStepName, string EnteredBy, DateTime CreatedAt, DateTime? ActivatedAt);

/// <summary>
/// A supplier application with what the page needs: OpenStepCode is the workflow step waiting now (null when none); CanEdit: the
/// terms can be changed (draft or returned). Parties, Compliance and CustomerRiskGrade come from the customer (Customer 360 >
/// Compliance, one place for KYC); RiskRatingCode is the grade the compliance step accepted.
/// </summary>
public sealed record CrmCfSupplierDto(
    long ApplicationId, string ApplicationRef, long ClientId, string ClientNumber, string ClientName, string? RegistrationNumber, string? TaxNumber,
    long ClientRoleId, string RoleStatusCode, string StatusCode, long? WorkflowInstanceId, string EnteredBy, string? ReturnReason,
    IReadOnlyList<string> GoodsCategoryCodes, decimal MinDownPaymentPct, string DownPaymentCollectorCode, decimal CommissionPct, int PaymentDays,
    int QuotationValidityDays, int DeliveryDays, DateOnly ValidFromDate, DateOnly? ValidToDate, long? ClientBankAccountId,
    string? RiskRatingCode, DateOnly? SiteVisitDate, DateTime? ActivatedAt, string? Notes,
    IReadOnlyList<CrmCfPartyDto> Parties, IReadOnlyList<CrmCfBankAccountOptionDto> BankAccounts,
    string? OpenStepCode, string? OpenStepName, bool CanEdit, CrmBadgeDto Compliance, string? CustomerRiskGrade,
    IReadOnlyList<CrmCfTermChangeDto> TermChanges, CrmCfChangeRequestDto? PendingChange = null);

/// <summary>
/// One field changed on an active supplier's terms (the audit trail): By asked for it, ApprovedBy approved it (null for the changes
/// applied directly before 27 Nov 2026), At is when it applied.
/// </summary>
public sealed record CrmCfTermChangeDto(string FieldName, string? OldValue, string? NewValue, string Reason, string By, DateTime At, string? ApprovedBy = null);

/// <summary>One field a change asks for: as it is and as it would be.</summary>
public sealed record CrmCfFieldChangeDto(string FieldName, string? OldValue, string? NewValue);

/// <summary>
/// A change to an active supplier's terms waiting for approval (Ahmed, 27 Nov 2026): what it changes, why, who asked; IsMine: the
/// signed-in user asked (may take it back, may not approve); CanDecide: another user who may approve or reject it.
/// </summary>
public sealed record CrmCfChangeRequestDto(long ChangeRequestId, IReadOnlyList<CrmCfFieldChangeDto> Changes, string Reason, string RequestedBy,
    DateTime RequestedAt, bool IsMine, bool CanDecide);

public sealed record CrmCfDecisionRequest(string? Notes);

/// <summary>New terms for an active supplier, with the reason for the change.</summary>
public sealed record CrmCfAmendRequest(CrmCfSupplierRequest Terms, string? Reason);

public sealed record CrmCfSupplierOptionsDto(
    IReadOnlyList<CrmCfOptionDto> GoodsCategories, IReadOnlyList<CrmCfOptionDto> Collectors, IReadOnlyList<CrmCfOptionDto> PartyTypes,
    IReadOnlyList<CrmCfOptionDto> RiskRatings, IReadOnlyList<CrmCfOptionDto> DocumentTypes);

/// <summary>The agreement terms Sales enters; ClientId only when the application is created.</summary>
public sealed record CrmCfSupplierRequest(
    long? ClientId, IReadOnlyList<string>? GoodsCategoryCodes, decimal? MinDownPaymentPct, string? DownPaymentCollectorCode, decimal? CommissionPct,
    int? PaymentDays, int? QuotationValidityDays, int? DeliveryDays, DateOnly? ValidFromDate, DateOnly? ValidToDate, long? ClientBankAccountId,
    string? Notes);

public sealed record CrmCfSiteVisitRequest(DateOnly? SiteVisitDate);

public sealed record CrmCfCompleteStepRequest(string? Action, string? Comment);

/// <summary>A document of the application; StepName: where it was added (data entry, a workflow step, or after activation).</summary>
public sealed record CrmCfDocumentDto(
    long DocumentId, string? DocumentTypeCode, string DocumentTitle, string FileName, string MimeType, long FileSizeBytes, string StepName,
    string CreatedBy, DateTime CreatedAt, bool CanRemove);

/// <summary>One step of an application: data entry, then each workflow step of each run (Round 2... after a return), with its decision.</summary>
public sealed record CrmCfStepViewDto(
    string Key, int Round, string StepCode, string StepName, string Status, string? Action, string? DoneBy, DateTime? DoneAt, string? Comment);

public sealed record CrmCfDocumentFile(string FileName, string MimeType, string Sha256Hash, byte[] Content);

/// <summary>
/// Consumer Finance suppliers (Ahmed, 25 Nov 2026): Sales enters the supplier's application (agreement terms, owners and
/// signatories) and submits it to CF_SUPPLIER_APPROVAL. The steps' own inputs (risk rating and screening at compliance, visit date at
/// the site visit) are entered by whoever holds that step's task, and anyone who can see the application adds its documents at any time; the step's checks run when it is accepted
/// (<see cref="CrmCfSupplierWorkflowHandler"/>). One open application per customer.
/// </summary>
public sealed class CrmCfSupplierService(
    ICapitraDbContext db, ICurrentUser currentUser, IClock clock, IScrUserProfiles profiles, WfeRuntime runtime, WfeInbox inbox, CrmCustomer360Service customers)
{
    public const string DataEntryStep = "DATA_ENTRY";
    public const string DataEntryName = "Data entry";
    public const long MaxFileBytes = 20 * 1024 * 1024;
    private const string Done = "DONE";
    private const string Open = "OPEN";

    private static readonly HashSet<string> Mimes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf", "image/png", "image/jpeg", "image/tiff",
        "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "text/plain",
    };

    private string Me => currentUser.UserName ?? throw new BusinessRuleException("No signed-in user.");

    // ---------------------------------------------------------------- lists and options

    public async Task<IReadOnlyList<CrmCfSupplierListItemDto>> ListAsync(string? status, CancellationToken ct)
    {
        var q = from a in db.Set<CrmCfSupplierApplication>().AsNoTracking()
                join c in db.Set<CrmClient>().AsNoTracking() on a.ClientId equals c.ClientId
                select new { a, c.ClientNumber, c.ClientName };
        if (!string.IsNullOrWhiteSpace(status))
        {
            var s = status.Trim().ToUpperInvariant();
            q = q.Where(x => x.a.StatusCode == s);
        }
        var rows = await q.OrderByDescending(x => x.a.ApplicationId).Take(500).ToListAsync(ct);
        var open = await OpenStepsAsync([.. rows.Where(x => x.a.WorkflowInstanceId != null).Select(x => x.a.WorkflowInstanceId!.Value)], ct);
        return [.. rows.Select(x =>
        {
            var step = x.a.StatusCode == CrmCfSupplierStatus.InWorkflow && x.a.WorkflowInstanceId is { } i ? open.GetValueOrDefault(i) : null;
            return new CrmCfSupplierListItemDto(x.a.ApplicationId, x.a.ApplicationRef, x.a.ClientId, x.ClientNumber, x.ClientName, x.a.StatusCode,
                x.a.RiskRatingCode, step, step is null ? null : CrmCfSupplierWorkflowHandler.StepName(step), x.a.EnteredBy, x.a.CreatedAt, x.a.ActivatedAt);
        })];
    }

    public async Task<CrmCfSupplierOptionsDto> OptionsAsync(CancellationToken ct)
    {
        var goods = await db.Set<LndLkpGoodsCategory>().AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code)
            .Select(x => new CrmCfOptionDto(x.Code, x.NameEn)).ToListAsync(ct);
        return new CrmCfSupplierOptionsDto(goods,
            [new("SUPPLIER", "The supplier"), new("COMPANY", "Our company")],
            [new("OWNER", "Owner"), new("MANAGER", "Manager"), new("SIGNATORY", "Authorised signatory")],
            [new("LOW", "Low"), new("MEDIUM", "Medium"), new("HIGH", "High")],
            [new("COMMERCIAL_REGISTER", "Commercial register"), new("TAX_CARD", "Tax card"), new("ACTIVITY_LICENCE", "Activity licence"),
                new("BANK_LETTER", "Bank letter"), new("ID_DOCUMENT", "Owner / signatory ID"), new(CrmCfSupplierRules.SiteVisitReport, "Site visit report"),
                new(CrmCfSupplierRules.SignedAgreement, "Signed agreement"), new("OTHER", "Other")]);
    }

    /// <summary>Customers by number, name or register number, for a new application.</summary>
    public async Task<IReadOnlyList<CrmCfClientOptionDto>> ClientsAsync(string? search, CancellationToken ct)
    {
        var term = (search ?? "").Trim();
        if (term.Length < 2) return [];
        var s = term.ToUpperInvariant();
#pragma warning disable CA1862 // EF Core translates ToUpper to SQL upper(), so the search ignores case in the database.
        return await db.Set<CrmClient>().AsNoTracking()
            .Where(x => x.IsActive && (x.ClientName.ToUpper().Contains(s) || x.ClientNameAr.Contains(term) || x.ClientNumber.ToUpper().Contains(s)
                || (x.RegistrationNumber != null && x.RegistrationNumber.Contains(term))))
            .OrderBy(x => x.ClientName).Take(20).Select(x => new CrmCfClientOptionDto(x.ClientId, x.ClientNumber, x.ClientName)).ToListAsync(ct);
#pragma warning restore CA1862
    }

    // ---------------------------------------------------------------- one application

    /// <summary>The customer's supplier application (the latest that is not withdrawn), for its Customer 360 tab; null when none.</summary>
    public async Task<long?> ApplicationOfClientAsync(long clientId, CancellationToken ct) =>
        await db.Set<CrmCfSupplierApplication>().AsNoTracking().Where(x => x.ClientId == clientId && x.StatusCode != CrmCfSupplierStatus.Withdrawn)
            .OrderByDescending(x => x.ApplicationId).Select(x => (long?)x.ApplicationId).FirstOrDefaultAsync(ct);

    public async Task<CrmCfSupplierDto> GetAsync(long applicationId, CancellationToken ct)
    {
        var a = await db.Set<CrmCfSupplierApplication>().AsNoTracking().FirstOrDefaultAsync(x => x.ApplicationId == applicationId, ct)
            ?? throw new NotFoundException($"Supplier application {applicationId} was not found.");
        var c = await db.Set<CrmClient>().AsNoTracking().FirstAsync(x => x.ClientId == a.ClientId, ct);
        var roleStatus = await db.Set<CrmClientRole>().AsNoTracking().Where(x => x.ClientRoleId == a.ClientRoleId).Select(x => x.StatusCode).FirstAsync(ct);
        var parties = await db.Set<CrmClientParty>().AsNoTracking().Where(x => x.ClientId == a.ClientId).OrderBy(x => x.PartyId)
            .Select(x => new CrmCfPartyDto(x.PartyId, x.PartyTypeCode, x.FullName, x.NationalIdNo, x.Nationality, x.OwnershipPct, x.ScreeningResultCode,
                x.ScreenedBy, x.ScreenedAt)).ToListAsync(ct);
        var banks = await db.Set<CrmClientBankAccount>().AsNoTracking().Where(x => x.ClientId == a.ClientId).OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.BankAccountId).Select(x => new CrmCfBankAccountOptionDto(x.BankAccountId, x.BankName, x.AccountNumber, x.Iban, x.StatusCode))
            .ToListAsync(ct);
        string? step = null;
        if (a.StatusCode == CrmCfSupplierStatus.InWorkflow && a.WorkflowInstanceId is { } instanceId)
            step = (await OpenStepsAsync([instanceId], ct)).GetValueOrDefault(instanceId);
        return new CrmCfSupplierDto(a.ApplicationId, a.ApplicationRef, a.ClientId, c.ClientNumber, c.ClientName, c.RegistrationNumber, c.TaxNumber,
            a.ClientRoleId, roleStatus, a.StatusCode, a.WorkflowInstanceId, a.EnteredBy, a.ReturnReason, CrmCfSupplierRules.Codes(a.GoodsCategoryCodes),
            a.MinDownPaymentPct, a.DownPaymentCollectorCode, a.CommissionPct, a.PaymentDays, a.QuotationValidityDays, a.DeliveryDays, a.ValidFromDate,
            a.ValidToDate, a.ClientBankAccountId, a.RiskRatingCode, a.SiteVisitDate, a.ActivatedAt, a.Notes, parties, banks,
            step, step is null ? null : CrmCfSupplierWorkflowHandler.StepName(step), EntryOpen(a),
            await customers.ComplianceBadgeAsync(a.ClientId, ct), c.RiskGrade,
            await (from x in db.Set<CrmCfSupplierTermChange>().AsNoTracking()
                   join q in db.Set<CrmCfSupplierChangeRequest>().AsNoTracking() on x.ChangeRequestId equals (long?)q.ChangeRequestId into qs
                   from q in qs.DefaultIfEmpty()
                   where x.ApplicationId == applicationId
                   orderby x.TermChangeId descending
                   select new CrmCfTermChangeDto(x.FieldName, x.OldValue, x.NewValue, x.Reason, q != null ? q.RequestedBy : x.CreatedBy,
                       x.ApprovedAt ?? x.CreatedAt, x.ApprovedBy)).ToListAsync(ct),
            await PendingAsync(applicationId, ct));
    }

    private async Task<CrmCfChangeRequestDto?> PendingAsync(long applicationId, CancellationToken ct)
    {
        var q = await db.Set<CrmCfSupplierChangeRequest>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.ApplicationId == applicationId && x.StatusCode == ChangeStatus.Pending, ct);
        if (q is null) return null;
        var mine = string.Equals(q.RequestedBy, currentUser.UserName, StringComparison.OrdinalIgnoreCase);
        return new CrmCfChangeRequestDto(q.ChangeRequestId, JsonSerializer.Deserialize<List<CrmCfFieldChangeDto>>(q.ChangesJson) ?? [], q.Reason,
            q.RequestedBy, q.RequestedAt, mine, !mine);
    }

    public static class ChangeStatus
    {
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Rejected = "REJECTED";
        public const string TakenBack = "TAKEN_BACK";
    }

    /// <summary>
    /// Asks for a change to an active supplier's terms (Ahmed, 27 Nov 2026, the same standard as the lending terms; CRM.CF_SUPPLIER.AMEND):
    /// the same checks as data entry, the bank account verified, a reason. Nothing changes until another admin approves it; one change
    /// waits at a time. Finances already created keep their terms.
    /// </summary>
    public async Task<CrmCfSupplierDto> AmendAsync(long applicationId, CrmCfAmendRequest r, CancellationToken ct)
    {
        var reason = ScrValidation.Clean(r.Reason);
        if (reason is null) throw BusinessValidationException.For("reason", "Write why the terms are changed.");
        if (reason.Length > 1000) throw BusinessValidationException.For("reason", "The reason is longer than 1000 characters.");
        if (await db.Set<CrmCfSupplierChangeRequest>().AnyAsync(x => x.ApplicationId == applicationId && x.StatusCode == ChangeStatus.Pending, ct))
            throw new BusinessRuleException("A change of this supplier's terms is already waiting for approval. Approve, reject or take it back first.");
        // worked out on a copy: nothing changes until the change is approved
        var a = await ApplicationAsync(applicationId, ct, tracking: false);
        var changes = await WorkChangeAsync(a, r.Terms, ct);
        db.Add(new CrmCfSupplierChangeRequest
        {
            ApplicationId = a.ApplicationId, TermsJson = JsonSerializer.Serialize(r.Terms), ChangesJson = JsonSerializer.Serialize(changes), Reason = reason,
            StatusCode = ChangeStatus.Pending, RequestedBy = Me, RequestedAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        return await GetAsync(applicationId, ct);
    }

    /// <summary>Approves a change asked for by another user: the terms apply now, one audit row per field changed.</summary>
    public async Task<CrmCfSupplierDto> ApproveChangeAsync(long changeRequestId, CrmCfDecisionRequest r, CancellationToken ct)
    {
        var q = await PendingRequestAsync(changeRequestId, ct);
        if (string.Equals(q.RequestedBy, Me, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("You asked for this change. Another admin must approve it.");
        var a = await ApplicationAsync(q.ApplicationId, ct);
        var terms = JsonSerializer.Deserialize<CrmCfSupplierRequest>(q.TermsJson) ?? throw new BusinessRuleException("The change cannot be read.");
        var changes = await WorkChangeAsync(a, terms, ct); // applied to the supplier; checked again on today's data
        var now = clock.UtcNow;
        foreach (var c in changes)
            db.Add(new CrmCfSupplierTermChange
            {
                ApplicationId = a.ApplicationId, FieldName = c.FieldName, OldValue = c.OldValue, NewValue = c.NewValue, Reason = q.Reason,
                ChangeRequestId = q.ChangeRequestId, ApprovedBy = Me, ApprovedAt = now,
            });
        Decide(q, ChangeStatus.Approved, ScrValidation.Clean(r.Notes));
        await db.SaveChangesAsync(ct);
        return await GetAsync(a.ApplicationId, ct);
    }

    /// <summary>Rejects a change asked for by another user, with the reason; the terms stay as they are.</summary>
    public async Task<CrmCfSupplierDto> RejectChangeAsync(long changeRequestId, CrmCfDecisionRequest r, CancellationToken ct)
    {
        var q = await PendingRequestAsync(changeRequestId, ct);
        if (string.Equals(q.RequestedBy, Me, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("You asked for this change. Take it back instead, or another admin rejects it.");
        var notes = ScrValidation.Clean(r.Notes) ?? throw BusinessValidationException.For("notes", "Write why the change is rejected.");
        Decide(q, ChangeStatus.Rejected, notes);
        await db.SaveChangesAsync(ct);
        return await GetAsync(q.ApplicationId, ct);
    }

    /// <summary>The one who asked for a change takes it back before it is decided.</summary>
    public async Task<CrmCfSupplierDto> TakeBackChangeAsync(long changeRequestId, CancellationToken ct)
    {
        var q = await PendingRequestAsync(changeRequestId, ct);
        if (!string.Equals(q.RequestedBy, Me, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException("Only the admin who asked for the change can take it back.");
        Decide(q, ChangeStatus.TakenBack, null);
        await db.SaveChangesAsync(ct);
        return await GetAsync(q.ApplicationId, ct);
    }

    private async Task<CrmCfSupplierChangeRequest> PendingRequestAsync(long id, CancellationToken ct)
    {
        var q = await db.Set<CrmCfSupplierChangeRequest>().FirstOrDefaultAsync(x => x.ChangeRequestId == id, ct)
            ?? throw new NotFoundException($"Change {id} was not found.");
        if (q.StatusCode != ChangeStatus.Pending) throw new BusinessRuleException("This change is no longer waiting for approval.");
        return q;
    }

    private void Decide(CrmCfSupplierChangeRequest q, string status, string? notes)
    {
        q.StatusCode = status;
        q.DecidedBy = Me;
        q.DecidedAt = clock.UtcNow;
        q.DecisionNotes = notes is { Length: > 1000 } ? notes[..1000] : notes;
    }

    /// <summary>
    /// Applies new terms to a supplier (a tracked one is changed; an untracked copy only shows what would change), with the data entry
    /// checks and a verified bank account. The supplier must be active and something must change.
    /// </summary>
    private async Task<List<CrmCfFieldChangeDto>> WorkChangeAsync(CrmCfSupplierApplication a, CrmCfSupplierRequest terms, CancellationToken ct)
    {
        if (a.StatusCode != CrmCfSupplierStatus.Active)
            throw new BusinessRuleException($"Only an active supplier's terms are changed this way; {a.ApplicationRef} is {a.StatusCode.ToLowerInvariant().Replace('_', ' ')}.");
        var before = Terms(a);
        Apply(a, terms);
        await CheckTermsAsync(a, ct);
        if (a.ClientBankAccountId is { } bankId
            && !await db.Set<CrmClientBankAccount>().AnyAsync(x => x.BankAccountId == bankId && x.StatusCode == "VERIFIED", ct))
            throw BusinessValidationException.For("clientBankAccountId", "Choose a verified bank account.");
        if (a.ClientBankAccountId is null) throw BusinessValidationException.For("clientBankAccountId", "An active supplier needs the bank account it is paid to.");
        var after = Terms(a);
        var changes = before.Where(x => after[x.Key] != x.Value).Select(x => new CrmCfFieldChangeDto(x.Key, x.Value, after[x.Key])).ToList();
        if (changes.Count == 0) throw new BusinessRuleException("Nothing was changed.");
        return changes;
    }

    /// <summary>The terms as text, by the name the page shows them under, to compare and keep in the change history.</summary>
    private static Dictionary<string, string?> Terms(CrmCfSupplierApplication a) => new()
    {
        ["Goods"] = a.GoodsCategoryCodes,
        ["Minimum down payment %"] = a.MinDownPaymentPct.ToString("0.######", CultureInfo.InvariantCulture),
        ["Down payment taken by"] = a.DownPaymentCollectorCode,
        ["Commission %"] = a.CommissionPct.ToString("0.######", CultureInfo.InvariantCulture),
        ["Payment days"] = a.PaymentDays.ToString(CultureInfo.InvariantCulture),
        ["Quotation validity days"] = a.QuotationValidityDays.ToString(CultureInfo.InvariantCulture),
        ["Delivery days"] = a.DeliveryDays.ToString(CultureInfo.InvariantCulture),
        ["Valid from"] = a.ValidFromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        ["Valid to"] = a.ValidToDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        ["Bank account"] = a.ClientBankAccountId?.ToString(CultureInfo.InvariantCulture),
        ["Notes"] = a.Notes,
    };

    /// <summary>
    /// Starts a supplier application for a customer: its SUPPLIER role for CONSUMER_FINANCE is added (Onboarding) when it has none.
    /// A customer has one application at a time; an active supplier is changed by a later agreement version, not a new application.
    /// </summary>
    public async Task<CrmCfSupplierDto> CreateAsync(CrmCfSupplierRequest r, CancellationToken ct)
    {
        if (r.ClientId is not { } clientId) throw BusinessValidationException.For("clientId", "Choose the supplier (customer).");
        var client = await db.Set<CrmClient>().AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == clientId, ct)
            ?? throw BusinessValidationException.For("clientId", "The customer was not found.");
        if (!client.IsActive) throw BusinessValidationException.For("clientId", "The customer is inactive.");
        if (client.IsBlacklisted) throw BusinessValidationException.For("clientId", "The customer is blacklisted.");
        var existing = await db.Set<CrmCfSupplierApplication>().AsNoTracking()
            .Where(x => x.ClientId == clientId && x.StatusCode != CrmCfSupplierStatus.Withdrawn).Select(x => x.ApplicationRef).FirstOrDefaultAsync(ct);
        if (existing is not null) throw new BusinessRuleException($"The customer already has the supplier application {existing}.");

        var profile = await profiles.ByUserNameAsync(Me, ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        var role = await db.Set<CrmClientRole>().FirstOrDefaultAsync(
            x => x.ClientId == clientId && x.RoleCode == CrmCustomer360Service.Supplier && x.BusinessLineCode == CrmCustomer360Service.ConsumerFinance, ct);
        if (role is null)
        {
            role = new CrmClientRole
            {
                ClientId = clientId, RoleCode = CrmCustomer360Service.Supplier, BusinessLineCode = CrmCustomer360Service.ConsumerFinance,
                StatusCode = CrmClientRole.Onboarding,
            };
            db.Add(role);
            await db.SaveChangesAsync(ct);
        }

        var a = new CrmCfSupplierApplication
        {
            ApplicationRef = "CFS-NEW-" + Guid.NewGuid().ToString("N")[..12], ClientId = clientId, ClientRoleId = role.ClientRoleId, EnteredBy = Me,
            EnteredByPerson = ScrDuties.PersonKey(profile?.EmployeeNo, Me), ValidFromDate = GlTime.Today(clock), PaymentDays = 7,
            QuotationValidityDays = 7, DeliveryDays = 5,
        };
        Apply(a, r);
        await CheckTermsAsync(a, ct);
        db.Add(a);
        await db.SaveChangesAsync(ct);
        a.ApplicationRef = "CFS-" + a.ApplicationId.ToString("D6", CultureInfo.InvariantCulture);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await GetAsync(a.ApplicationId, ct);
    }

    public async Task<CrmCfSupplierDto> UpdateAsync(long applicationId, CrmCfSupplierRequest r, CancellationToken ct)
    {
        var a = await EditableAsync(applicationId, ct);
        Apply(a, r);
        await CheckTermsAsync(a, ct);
        await db.SaveChangesAsync(ct);
        return await GetAsync(applicationId, ct);
    }

    /// <summary>Submits the application to CF_SUPPLIER_APPROVAL (a returned one starts a new run).</summary>
    public async Task<CrmCfSupplierDto> SubmitAsync(long applicationId, CancellationToken ct)
    {
        var a = await EditableAsync(applicationId, ct);
        await CheckTermsAsync(a, ct);
        var parties = await db.Set<CrmClientParty>().AsNoTracking().Where(x => x.ClientId == a.ClientId).ToListAsync(ct);
        var problems = CrmCfSupplierRules.CheckSubmit(parties);
        if (problems.Count > 0) throw new BusinessRuleException(string.Join(" ", problems));

        await using var tx = await db.BeginTransactionAsync(ct);
        var variables = new Dictionary<string, string?>
        {
            ["entity_id"] = a.ApplicationId.ToString(CultureInfo.InvariantCulture),
            ["entity_ref"] = a.ApplicationRef,
            ["entity_type"] = "CRM_CF_SUPPLIER",
            ["module_code"] = "CRM",
        };
        var instance = await runtime.StartAsync(CrmCfSupplierRules.WorkflowCode, a.ApplicationRef, variables, Me, ct);
        a.WorkflowInstanceId = instance.WorkflowInstanceId;
        a.StatusCode = CrmCfSupplierStatus.InWorkflow;
        a.ReturnReason = null;
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await GetAsync(applicationId, ct);
    }

    // ---------------------------------------------------------------- the steps' own inputs

    /// <summary>Site visit: the date of the visit, by whoever holds the site visit task (the report is attached at the step).</summary>
    public async Task<CrmCfSupplierDto> SaveSiteVisitAsync(long applicationId, long inboxId, CrmCfSiteVisitRequest r, CancellationToken ct)
    {
        var a = await StepApplicationAsync(applicationId, inboxId, CrmCfSupplierRules.SiteVisit, ct);
        if (r.SiteVisitDate is { } d && d > GlTime.Today(clock)) throw BusinessValidationException.For("siteVisitDate", "The visit date cannot be in the future.");
        a.SiteVisitDate = r.SiteVisitDate;
        await db.SaveChangesAsync(ct);
        return await GetAsync(applicationId, ct);
    }

    /// <summary>The contract step may still choose the bank account to pay (it must be verified to accept the step).</summary>
    public async Task<CrmCfSupplierDto> SaveBankAccountAsync(long applicationId, long inboxId, long? bankAccountId, CancellationToken ct)
    {
        var a = await StepApplicationAsync(applicationId, inboxId, CrmCfSupplierRules.Contract, ct);
        await CheckBankAsync(a.ClientId, bankAccountId, ct);
        a.ClientBankAccountId = bankAccountId;
        await db.SaveChangesAsync(ct);
        return await GetAsync(applicationId, ct);
    }

    // ---------------------------------------------------------------- completing a step

    /// <summary>
    /// Completes the signed-in user's task (ACCEPT, or REJECT to return it to Sales) and keeps the step's comment. One transaction:
    /// when the step's checks fail, nothing is kept and the task stays open.
    /// </summary>
    public async Task<CrmCfSupplierDto> CompleteAsync(long applicationId, long inboxId, CrmCfCompleteStepRequest r, CancellationToken ct)
    {
        var action = (r.Action ?? "").Trim().ToUpperInvariant();
        if (action is not ("ACCEPT" or "REJECT")) throw BusinessValidationException.For("action", "Accept or return the application.");
        var comment = ScrValidation.Clean(r.Comment);
        if (action == "REJECT" && comment is null) throw BusinessValidationException.For("comment", "Write why the application is returned.");
        if (comment is { Length: > 2000 }) throw BusinessValidationException.For("comment", "The comment is longer than 2000 characters.");

        var a = await ApplicationAsync(applicationId, ct, tracking: false);
        var step = await OpenStepAsync(a, inboxId, ct);
        await using var tx = await db.BeginTransactionAsync(ct);
        db.Add(new CrmCfSupplierStepNote
        {
            ApplicationId = a.ApplicationId, WorkflowInstanceId = step.WorkflowInstanceId, StepInstanceId = step.StepInstanceId,
            StepCode = step.StepCode ?? "", StepName = step.StepName ?? step.StepCode ?? "", ActionCode = action, CommentText = comment, DoneBy = Me, DoneAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(ct);
        await inbox.CompleteAsync(inboxId, new CompleteInboxItemRequest(action, comment), ct);
        if (action == "REJECT")
        {
            var back = await db.Set<CrmCfSupplierApplication>().FirstAsync(x => x.ApplicationId == applicationId, ct);
            back.ReturnReason = Cut($"{back.ReturnReason} {comment}".Trim(), 1000);
            await db.SaveChangesAsync(ct);
        }
        await tx.CommitAsync(ct);
        return await GetAsync(applicationId, ct);
    }

    // ---------------------------------------------------------------- documents

    /// <summary>The application's documents, newest first.</summary>
    public async Task<IReadOnlyList<CrmCfDocumentDto>> DocumentsAsync(long applicationId, CancellationToken ct)
    {
        var a = await ApplicationAsync(applicationId, ct, tracking: false);
        var docs = await db.Set<CrmCfSupplierDocument>().AsNoTracking().Where(x => x.ApplicationId == applicationId && x.IsActive)
            .OrderByDescending(x => x.DocumentId)
            .Select(x => new { x.DocumentId, x.DocumentTypeCode, x.DocumentTitle, x.FileName, x.MimeType, x.FileSizeBytes, x.StepName, x.CreatedBy, x.CreatedAt })
            .ToListAsync(ct);
        return [.. docs.Select(x => new CrmCfDocumentDto(x.DocumentId, x.DocumentTypeCode, x.DocumentTitle, x.FileName, x.MimeType, x.FileSizeBytes, x.StepName,
            x.CreatedBy, x.CreatedAt, CanRemove(x.CreatedBy, a)))];
    }

    /// <summary>
    /// Adds a document to the application, by any user who can see it, at any time (Ahmed, 25 Nov 2026): it is kept with the
    /// application, noting where it was added (data entry, the workflow step waiting now, or after activation).
    /// </summary>
    public async Task<CrmCfDocumentDto> AddDocumentAsync(long applicationId, string? typeCode, string? title, string fileName, string mimeType, byte[] content, CancellationToken ct)
    {
        var a = await ApplicationAsync(applicationId, ct, tracking: false);
        if (a.StatusCode == CrmCfSupplierStatus.Withdrawn) throw new BusinessRuleException($"Supplier application {a.ApplicationRef} is withdrawn.");
        var type = ScrValidation.Clean(typeCode)?.ToUpperInvariant();
        if (type is not null && !CrmCfSupplierRules.DocumentTypes.Contains(type)) throw BusinessValidationException.For("documentTypeCode", "Choose the document type.");
        if (content.Length == 0 || content.Length > MaxFileBytes) throw BusinessValidationException.For("file", "The file is empty or larger than 20 MB.");
        var mime = mimeType.Trim().ToLowerInvariant();
        if (!Mimes.Contains(mime)) throw BusinessValidationException.For("file", "Attach a PDF, image, Word, Excel or text file.");

        var (stepCode, stepName) = (DataEntryStep, DataEntryName);
        if (a.StatusCode == CrmCfSupplierStatus.Active) (stepCode, stepName) = ("ACTIVE", "After activation");
        else if (a.StatusCode == CrmCfSupplierStatus.InWorkflow && a.WorkflowInstanceId is { } run
            && (await OpenStepsAsync([run], ct)).GetValueOrDefault(run) is { } open)
            (stepCode, stepName) = (open, CrmCfSupplierWorkflowHandler.StepName(open));

        var name = Path.GetFileName(fileName.Trim());
        var doc = new CrmCfSupplierDocument
        {
            ApplicationId = a.ApplicationId, WorkflowInstanceId = a.WorkflowInstanceId, StepCode = stepCode, StepName = stepName, DocumentTypeCode = type,
            DocumentTitle = Cut(ScrValidation.Clean(title) ?? Path.GetFileNameWithoutExtension(name), 300)!, FileName = Cut(name, 260)!,
            MimeType = mime, FileSizeBytes = content.Length, Sha256Hash = Convert.ToHexStringLower(SHA256.HashData(content)), FileBlob = content,
        };
        db.Add(doc);
        await db.SaveChangesAsync(ct);
        return new CrmCfDocumentDto(doc.DocumentId, doc.DocumentTypeCode, doc.DocumentTitle, doc.FileName, doc.MimeType, doc.FileSizeBytes, doc.StepName,
            Me, doc.CreatedAt, CanRemove(Me, a));
    }

    /// <summary>Removes a document (kept, marked inactive): only by the user who added it, and not once the supplier is active.</summary>
    public async Task RemoveDocumentAsync(long documentId, CancellationToken ct)
    {
        var doc = await db.Set<CrmCfSupplierDocument>().FirstOrDefaultAsync(x => x.DocumentId == documentId && x.IsActive, ct)
            ?? throw new NotFoundException("The document was not found.");
        var a = await ApplicationAsync(doc.ApplicationId, ct, tracking: false);
        if (!CanRemove(doc.CreatedBy, a))
            throw new BusinessRuleException("Only the user who added a document can remove it, and not once the supplier is active.");
        doc.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    public async Task<CrmCfDocumentFile> DocumentFileAsync(long documentId, CancellationToken ct)
    {
        var f = await db.Set<CrmCfSupplierDocument>().AsNoTracking().Where(x => x.DocumentId == documentId && x.IsActive)
            .Select(x => new CrmCfDocumentFile(x.FileName, x.MimeType, x.Sha256Hash, x.FileBlob)).FirstOrDefaultAsync(ct);
        return f ?? throw new NotFoundException("The document was not found.");
    }

    // ---------------------------------------------------------------- the steps

    /// <summary>Every step of the application, in order, with its decision: data entry, then each workflow run's steps.</summary>
    public async Task<IReadOnlyList<CrmCfStepViewDto>> StepsAsync(long applicationId, CancellationToken ct)
    {
        var a = await ApplicationAsync(applicationId, ct, tracking: false);
        var runs = await (
            from i in db.Set<WfeWorkflowInstance>().AsNoTracking()
            join w in db.Set<WfeWorkflowDefinition>().AsNoTracking() on i.WorkflowDefinitionId equals w.WorkflowDefinitionId
            where w.WorkflowCode == CrmCfSupplierRules.WorkflowCode && i.BusinessReference == a.ApplicationRef
            orderby i.WorkflowInstanceId
            select new { i.WorkflowInstanceId, i.StartedAt, i.StartedBy }).ToListAsync(ct);
        var runIds = runs.Select(x => x.WorkflowInstanceId).ToList();
        var steps = await db.Set<WfeStepInstance>().AsNoTracking()
            .Where(s => runIds.Contains(s.WorkflowInstanceId) && s.ElementType != "startEvent" && s.ElementType != "endEvent" && s.StepCode != null && s.StepCode != "")
            .OrderBy(s => s.StepInstanceId)
            .Select(s => new { s.WorkflowInstanceId, s.StepInstanceId, s.StepCode, s.StepName, s.StepStatus, s.CompletedAt, s.CompletedBy, s.Outcome })
            .ToListAsync(ct);
        var notes = await db.Set<CrmCfSupplierStepNote>().AsNoTracking().Where(n => n.ApplicationId == applicationId).ToDictionaryAsync(n => n.StepInstanceId, ct);
        var list = new List<CrmCfStepViewDto>();
        var round = 0;
        foreach (var run in runs)
        {
            round++;
            list.Add(new CrmCfStepViewDto($"{DataEntryStep}-{round}", round, DataEntryStep, DataEntryName, Done, "SUBMIT", run.StartedBy, run.StartedAt, null));
            foreach (var s in steps.Where(s => s.WorkflowInstanceId == run.WorkflowInstanceId))
            {
                var done = s.StepStatus == WfeRuntime.Completed;
                var note = notes.GetValueOrDefault(s.StepInstanceId);
                list.Add(new CrmCfStepViewDto($"{s.StepCode}-{round}-{s.StepInstanceId}", round, s.StepCode!, s.StepName ?? s.StepCode!, done ? Done : Open,
                    note?.ActionCode ?? (done ? s.Outcome : null), done ? note?.DoneBy ?? s.CompletedBy : null, done ? note?.DoneAt ?? s.CompletedAt : null,
                    note?.CommentText));
            }
        }
        if (EntryOpen(a))
            list.Add(new CrmCfStepViewDto($"{DataEntryStep}-{round + 1}", round + 1, DataEntryStep, DataEntryName, Open, null, a.EnteredBy, null, null));
        return list;
    }

    // ---------------------------------------------------------------- helpers

    private static bool EntryOpen(CrmCfSupplierApplication a) => a.StatusCode is CrmCfSupplierStatus.Draft or CrmCfSupplierStatus.Returned;

    private static void Apply(CrmCfSupplierApplication a, CrmCfSupplierRequest r)
    {
        if (r.GoodsCategoryCodes is { } codes) a.GoodsCategoryCodes = string.Join(",", CrmCfSupplierRules.Codes(string.Join(",", codes)));
        if (r.MinDownPaymentPct is { } dp) a.MinDownPaymentPct = dp;
        if (ScrValidation.Clean(r.DownPaymentCollectorCode) is { } who) a.DownPaymentCollectorCode = who.ToUpperInvariant();
        if (r.CommissionPct is { } commission) a.CommissionPct = commission;
        if (r.PaymentDays is { } pay) a.PaymentDays = pay;
        if (r.QuotationValidityDays is { } quote) a.QuotationValidityDays = quote;
        if (r.DeliveryDays is { } delivery) a.DeliveryDays = delivery;
        if (r.ValidFromDate is { } from) a.ValidFromDate = from;
        a.ValidToDate = r.ValidToDate;
        a.ClientBankAccountId = r.ClientBankAccountId;
        a.Notes = Cut(ScrValidation.Clean(r.Notes), 2000);
    }

    private async Task CheckTermsAsync(CrmCfSupplierApplication a, CancellationToken ct)
    {
        var known = await db.Set<LndLkpGoodsCategory>().AsNoTracking().Where(x => x.IsActive).Select(x => x.Code).ToListAsync(ct);
        var e = CrmCfSupplierRules.CheckTerms(a, known);
        if (e.Count > 0) throw new BusinessValidationException(e);
        await CheckBankAsync(a.ClientId, a.ClientBankAccountId, ct);
    }

    private async Task CheckBankAsync(long clientId, long? bankAccountId, CancellationToken ct)
    {
        if (bankAccountId is { } id && !await db.Set<CrmClientBankAccount>().AnyAsync(x => x.BankAccountId == id && x.ClientId == clientId, ct))
            throw BusinessValidationException.For("clientBankAccountId", "The bank account is not one of the supplier's.");
    }

    private async Task<CrmCfSupplierApplication> ApplicationAsync(long id, CancellationToken ct, bool tracking = true)
    {
        var q = tracking ? db.Set<CrmCfSupplierApplication>() : db.Set<CrmCfSupplierApplication>().AsNoTracking();
        return await q.FirstOrDefaultAsync(x => x.ApplicationId == id, ct) ?? throw new NotFoundException($"Supplier application {id} was not found.");
    }

    private async Task<CrmCfSupplierApplication> EditableAsync(long id, CancellationToken ct)
    {
        var a = await ApplicationAsync(id, ct);
        if (!EntryOpen(a))
            throw new BusinessRuleException($"Supplier application {a.ApplicationRef} is {a.StatusCode.ToLowerInvariant().Replace('_', ' ')}: it can be changed only as a draft or when returned.");
        return a;
    }

    /// <summary>The application, when the signed-in user holds its open task at this step.</summary>
    private async Task<CrmCfSupplierApplication> StepApplicationAsync(long applicationId, long inboxId, string stepCode, CancellationToken ct)
    {
        var a = await ApplicationAsync(applicationId, ct);
        var step = await OpenStepAsync(a, inboxId, ct);
        if (!string.Equals(step.StepCode, stepCode, StringComparison.OrdinalIgnoreCase))
            throw new BusinessRuleException($"This is entered at the {CrmCfSupplierWorkflowHandler.StepName(stepCode).ToLowerInvariant()} step.");
        return a;
    }

    /// <summary>The open workflow step behind the signed-in user's task on this application.</summary>
    private async Task<WfeStepInstance> OpenStepAsync(CrmCfSupplierApplication a, long inboxId, CancellationToken ct)
    {
        var item = await inbox.GetAsync(inboxId, ct); // throws when the task is not the user's
        if (a.WorkflowInstanceId is null || item.InstanceId != a.WorkflowInstanceId)
            throw new BusinessRuleException($"This task is not on supplier application {a.ApplicationRef}.");
        if (item.Status == WfeRuntime.Completed)
            throw new BusinessRuleException("This step is already completed; it is view-only.");
        return await db.Set<WfeStepInstance>().AsNoTracking().FirstAsync(x => x.StepInstanceId == item.StepInstanceId, ct);
    }

    /// <summary>Per workflow run, the step code waiting now (the latest open human step).</summary>
    private async Task<Dictionary<long, string>> OpenStepsAsync(List<long> instanceIds, CancellationToken ct)
    {
        if (instanceIds.Count == 0) return new Dictionary<long, string>();
        var open = await db.Set<WfeStepInstance>().AsNoTracking()
            .Where(s => instanceIds.Contains(s.WorkflowInstanceId) && s.StepStatus != WfeRuntime.Completed && s.StepStatus != WfeRuntime.Cancelled
                && s.StepCode != null && s.StepCode != "")
            .OrderBy(s => s.StepInstanceId).Select(s => new { s.WorkflowInstanceId, s.StepCode }).ToListAsync(ct);
        var result = new Dictionary<long, string>();
        foreach (var s in open) result[s.WorkflowInstanceId] = s.StepCode!;
        return result;
    }

    private bool CanRemove(string createdBy, CrmCfSupplierApplication a) =>
        string.Equals(createdBy, currentUser.UserName, StringComparison.OrdinalIgnoreCase) && a.StatusCode != CrmCfSupplierStatus.Active;

    private static string? Cut(string? s, int max) => s is null ? null : s.Length <= max ? s : s[..max];
}
