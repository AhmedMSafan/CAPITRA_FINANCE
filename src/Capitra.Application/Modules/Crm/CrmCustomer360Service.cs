using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Settings;
using Capitra.Application.Modules.Gl;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm;

/// <summary>From appsettings Crm:BankAccountWaitingDays: the default of the module setting CRM.BANK_ACCOUNT_WAITING_DAYS.</summary>
public sealed record CrmSettings(int BankAccountWaitingDays);

public sealed record CrmBadgeDto(string Code, string Label, IReadOnlyList<string> Reasons);

public sealed record CrmProfileDto(
    long ClientId, string ClientNumber, string ClientName, string ClientNameAr, string? ShortName, string ClientType, string LegalStructure,
    string? TaxNumber, string? RegistrationNumber, string? VatNumber, string? NationalId, string? CountryName, string? CurrencyCode,
    string? StatusName, string? Email, string? PhoneNumber, string? MobileNumber, string? Website, string? GroupName, bool IsRelatedParty,
    bool IsActive);

public sealed record CrmComplianceDto(
    string? RiskGrade, string? KycStatus, DateOnly? KycExpiryDate, string? AmlStatus, DateOnly? AmlCheckDate, string? SanctionsStatus,
    bool IsPep, bool IsBlacklisted, string? BlacklistReason, DateOnly? NextReviewDate, string? Notes);

public sealed record CrmComplianceStateDto(
    CrmComplianceDto Profile, string? ChangedBy, DateTime? ChangedAt, string? VerifiedBy, DateTime? VerifiedAt, bool CanVerify, string? VerifyBlockedReason);

public sealed record CrmComplianceHistoryDto(
    long ComplianceHistoryId, string ActionCode, string? KycStatus, DateOnly? KycExpiryDate, string? AmlStatus, string? SanctionsStatus,
    bool IsPep, bool IsBlacklisted, string? RiskGrade, DateOnly? NextReviewDate, string? Notes, string By, DateTime At);

public sealed record CrmRoleDto(long ClientRoleId, string RoleCode, string RoleLabel, string StatusCode, DateOnly? ActiveSinceDate, string? Notes,
    IReadOnlyList<string> ActivationBlockers);

public sealed record CrmBankAccountDto(
    long BankAccountId, string BankName, string? BranchName, string? SwiftCode, string? AccountNumber, string? Iban, string AccountHolderName,
    string? AccountHolderNameAr, long CurrencyId, string CurrencyCode, string AccountTypeCode, bool IsPrimary, string StatusCode,
    string ChangedBy, DateTime ChangedAt, string? VerifiedBy, DateTime? VerifiedAt, string? VerificationMethodCode, string? Notes,
    DateTime PayableFromAt, bool IsPayable);

public sealed record CrmBankAccountHistoryDto(
    long BankAccountHistoryId, long BankAccountId, string ActionCode, string BankName, string? AccountNumber, string? Iban, string CurrencyCode,
    bool IsPrimary, string StatusCode, string? VerificationMethodCode, string? Notes, string By, DateTime At);

public sealed record CrmProgramDto(
    long AgreementId, string AgreementNumber, string ProgramRole, string? FactoringType, string? StatusCode, string? CurrencyCode,
    decimal? ApprovedLimit, decimal? UtilizedAmount, decimal? AvailableAmount);

public sealed record CrmCustomer360Dto(
    CrmProfileDto Profile, CrmBadgeDto ComplianceBadge, CrmBadgeDto BankBadge, CrmBadgeDto ReadyToPay, CrmComplianceStateDto Compliance,
    IReadOnlyList<CrmComplianceHistoryDto> ComplianceHistory, IReadOnlyList<CrmRoleDto> Roles, IReadOnlyList<CrmBankAccountDto> BankAccounts,
    IReadOnlyList<CrmBankAccountHistoryDto> BankAccountHistory, IReadOnlyList<CrmProgramDto> Programs, int BankAccountWaitingDays,
    bool BankAccountMakerChecker, bool ComplianceMakerChecker);

public sealed record CrmBankAccountFormDto(
    string BankName, string? BranchName, string? SwiftCode, string? AccountNumber, string? Iban, string AccountHolderName,
    string? AccountHolderNameAr, long CurrencyId, string AccountTypeCode, bool IsPrimary, string? Notes);

public sealed record CrmVerifyDto(string? VerificationMethodCode, string? Notes);

public sealed record CrmAddRoleDto(string RoleCode, string? Notes);

/// <summary>
/// Customer 360 (agreed with Ahmed, 4 Nov 2026): one customer record with its roles, its compliance profile and its bank accounts.
/// Compliance and bank accounts are verified by a second person (four eyes); a role becomes ACTIVE only when its checks pass; a bank
/// account can be paid only when VERIFIED and the waiting period after its last change has passed. Administration > Module Settings
/// decides whether the second person is needed, the waiting period, the KYC warning and the review cycle (7 Nov 2026).
/// </summary>
public sealed class CrmCustomer360Service(ICapitraDbContext db, ICurrentUser currentUser, IClock clock, IModuleSettings moduleSettings)
{
    public static readonly IReadOnlyList<CrmOptionDto> Roles =
    [
        new("CLIENT", "Client"),
        new("DEBTOR", "Debtor"),
        new("SUPPLIER", "Supplier"),
        new("GUARANTOR", "Guarantor"),
        new("REAL_ESTATE_DEVELOPER", "Real Estate Developer"),
        new("INVESTOR", "Investor"),
    ];

    public static readonly string[] KycStatuses = ["PENDING", "APPROVED", "REJECTED"];
    public static readonly string[] AmlStatuses = ["PENDING", "CLEAR", "REVIEW", "BLOCKED"];
    public static readonly string[] SanctionsStatuses = ["PENDING", "CLEAR", "MATCH", "BLOCKED"];
    public static readonly string[] RiskGrades = ["LOW", "MEDIUM", "HIGH"];
    public static readonly string[] AccountTypes = ["CURRENT", "SAVINGS", "OTHER"];
    public static readonly string[] VerificationMethods = ["BANK_LETTER", "CALL_BACK", "BANK_STATEMENT"];

    /// <summary>The module settings this service uses, read once per request (<see cref="LoadSettingsAsync"/>).</summary>
    private sealed record Rules(bool ComplianceMakerChecker, bool BankAccountMakerChecker, int WaitingDays, int ReviewWarningDays);

    private Rules rules = new(true, true, 2, 30);

    private async Task LoadSettingsAsync(CancellationToken cancellationToken) =>
        rules = new(
            await moduleSettings.BoolAsync(ModuleSettingCatalog.Codes.CrmComplianceMakerChecker, cancellationToken),
            await moduleSettings.BoolAsync(ModuleSettingCatalog.Codes.CrmBankAccountMakerChecker, cancellationToken),
            await moduleSettings.BankAccountWaitingDaysAsync(cancellationToken),
            await moduleSettings.IntAsync(ModuleSettingCatalog.Codes.CrmKycExpiryWarningDays, cancellationToken));

    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    // ---------------------------------------------------------------- read

    public async Task<CrmCustomer360Dto> GetAsync(long clientId, CancellationToken cancellationToken)
    {
        await LoadSettingsAsync(cancellationToken);
        var c = await db.Set<CrmClient>().AsNoTracking().SingleOrDefaultAsync(x => x.ClientId == clientId, cancellationToken)
            ?? throw new NotFoundException($"Customer {clientId} was not found.");
        var currencies = await db.Set<CrmLkpCurrency>().AsNoTracking().ToDictionaryAsync(x => x.CurrencyId, x => x.CurrencyCode, cancellationToken);
        var country = await db.Set<CrmLkpCountry>().AsNoTracking().Where(x => x.CountryId == c.CountryId).Select(x => x.CountryName)
            .FirstOrDefaultAsync(cancellationToken);
        var status = await db.Set<CrmLkpStatusCode>().AsNoTracking().Where(x => x.StatusId == c.StatusId).Select(x => x.StatusName)
            .FirstOrDefaultAsync(cancellationToken);

        var accounts = await db.Set<CrmClientBankAccount>().AsNoTracking().Where(x => x.ClientId == clientId)
            .OrderByDescending(x => x.IsPrimary).ThenBy(x => x.BankAccountId).ToListAsync(cancellationToken);
        var roles = await db.Set<CrmClientRole>().AsNoTracking().Where(x => x.ClientId == clientId).OrderBy(x => x.ClientRoleId)
            .ToListAsync(cancellationToken);
        var complianceHistory = await db.Set<CrmClientComplianceHistory>().AsNoTracking().Where(x => x.ClientId == clientId)
            .OrderByDescending(x => x.ComplianceHistoryId).Take(50).ToListAsync(cancellationToken);
        var bankHistory = await db.Set<CrmClientBankAccountHistory>().AsNoTracking().Where(x => x.ClientId == clientId)
            .OrderByDescending(x => x.BankAccountHistoryId).Take(100).ToListAsync(cancellationToken);
        var programs = await ProgramsAsync(clientId, currencies, cancellationToken);

        var now = clock.UtcNow;
        var compliance = ComplianceBadge(c);
        var bank = BankBadge(accounts, now);
        var ready = ReadyToPay(c, compliance, accounts, now);

        var (canVerify, blocked) = c.ComplianceVerifiedAt is not null
            ? (false, "The compliance profile is already verified.")
            : rules.ComplianceMakerChecker && SameUser(c.ComplianceChangedBy) ? (false, "You made the last change; another user must verify it.") : (true, (string?)null);

        return new CrmCustomer360Dto(
            new CrmProfileDto(c.ClientId, c.ClientNumber, c.ClientName, c.ClientNameAr, c.ShortName, c.ClientType, c.LegalStructure, c.TaxNumber,
                c.RegistrationNumber, c.VatNumber, c.NationalId, country, currencies.GetValueOrDefault(c.BaseCurrencyId), status, c.Email,
                c.PhoneNumber, c.MobileNumber, c.Website, c.GroupName, c.IsRelatedParty, c.IsActive),
            compliance, bank, ready,
            new CrmComplianceStateDto(Profile(c), c.ComplianceChangedBy, c.ComplianceChangedAt, c.ComplianceVerifiedBy, c.ComplianceVerifiedAt,
                canVerify, blocked),
            [.. complianceHistory.Select(h => new CrmComplianceHistoryDto(h.ComplianceHistoryId, h.ActionCode, h.KycStatus, h.KycExpiryDate,
                h.AmlStatus, h.SanctionsStatus, h.IsPep, h.IsBlacklisted, h.RiskGrade, h.NextReviewDate, h.Notes, h.CreatedBy, h.CreatedAt))],
            [.. roles.Select(r => new CrmRoleDto(r.ClientRoleId, r.RoleCode, RoleLabel(r.RoleCode), r.StatusCode, r.ActiveSinceDate, r.Notes,
                r.StatusCode == CrmClientRole.Active ? [] : RoleBlockers(r.RoleCode, c, compliance)))],
            [.. accounts.Select(a => ToDto(a, currencies, now))],
            [.. bankHistory.Select(h => new CrmBankAccountHistoryDto(h.BankAccountHistoryId, h.BankAccountId, h.ActionCode, h.BankName,
                h.AccountNumber, h.Iban, currencies.GetValueOrDefault(h.CurrencyId) ?? "", h.IsPrimary, h.StatusCode, h.VerificationMethodCode,
                h.Notes, h.CreatedBy, h.CreatedAt))],
            programs, rules.WaitingDays, rules.BankAccountMakerChecker, rules.ComplianceMakerChecker);
    }

    /// <summary>The factoring agreements (programs) where the customer is the client, with the agreement-level limit.</summary>
    private async Task<IReadOnlyList<CrmProgramDto>> ProgramsAsync(long clientId, Dictionary<long, string> currencies, CancellationToken cancellationToken)
    {
        var agreements = await db.Set<FctAgreement>().AsNoTracking().Where(x => x.ClientId == clientId).OrderByDescending(x => x.AgreementId)
            .ToListAsync(cancellationToken);
        var ids = agreements.Select(x => x.AgreementId).ToList();
        var limits = await db.Set<FctCreditLimit>().AsNoTracking().Where(x => x.AgreementId != null && ids.Contains(x.AgreementId.Value) && x.IsActive)
            .ToListAsync(cancellationToken);
        return [.. agreements.Select(a =>
        {
            var l = limits.Where(x => x.AgreementId == a.AgreementId).OrderByDescending(x => x.CreditLimitId).FirstOrDefault();
            return new CrmProgramDto(a.AgreementId, a.AgreementNumber, "Client", a.FactoringType, a.StatusCode,
                currencies.GetValueOrDefault(a.CurrencyId), l?.ApprovedAmount ?? a.ApprovedLimit, l?.UtilizedAmount ?? a.UtilizedAmount,
                l?.AvailableAmount ?? a.AvailableAmount);
        })];
    }

    // ---------------------------------------------------------------- badges

    private DateOnly Today => GlTime.Today(clock);

    internal CrmBadgeDto ComplianceBadge(CrmClient c)
    {
        var today = Today;
        var blocked = new List<string>();
        if (c.IsBlacklisted) blocked.Add("Blacklisted" + (string.IsNullOrWhiteSpace(c.BlacklistReason) ? "." : $": {c.BlacklistReason}"));
        if (c.KycStatus == "REJECTED") blocked.Add("KYC rejected.");
        if (c.AmlStatus == "BLOCKED") blocked.Add("AML blocked.");
        if (c.SanctionsStatus is "MATCH" or "BLOCKED") blocked.Add($"Sanctions {c.SanctionsStatus!.ToLowerInvariant()}.");
        if (blocked.Count > 0) return new("BLOCKED", "Blocked", blocked);

        if (c.KycExpiryDate is { } exp && exp < today) return new("EXPIRED", "Expired", [$"KYC expired on {exp:dd-MMM-yyyy}."]);

        var pending = new List<string>();
        if (c.KycStatus != "APPROVED") pending.Add("KYC is not approved.");
        if (c.KycExpiryDate is null) pending.Add("KYC expiry date is missing.");
        if (c.AmlStatus != "CLEAR") pending.Add("AML is not clear.");
        if (c.SanctionsStatus != "CLEAR") pending.Add("Sanctions screening is not clear.");
        if (c.ComplianceVerifiedAt is null) pending.Add("The compliance profile is not verified.");
        if (pending.Count > 0) return new("PENDING", "Pending", pending);

        var due = new List<string>();
        if (c.NextReviewDate is { } review && review <= today) due.Add($"Review was due on {review:dd-MMM-yyyy}.");
        if (c.KycExpiryDate is { } e && e <= today.AddDays(rules.ReviewWarningDays)) due.Add($"KYC expires on {e:dd-MMM-yyyy}.");
        return due.Count > 0 ? new("DUE", "Due for review", due) : new("CLEAR", "Clear", []);
    }

    private static bool ComplianceOk(CrmBadgeDto b) => b.Code is "CLEAR" or "DUE";

    private DateTime PayableFrom(CrmClientBankAccount a) => a.ChangedAt.AddDays(rules.WaitingDays);

    private bool Payable(CrmClientBankAccount a, DateTime now) => a.StatusCode == CrmClientBankAccount.Verified && PayableFrom(a) <= now;

    private CrmBadgeDto BankBadge(List<CrmClientBankAccount> accounts, DateTime now)
    {
        var verified = accounts.Where(x => x.StatusCode == CrmClientBankAccount.Verified).ToList();
        if (verified.Count == 0)
            return new("MISSING", "Missing", [accounts.Count == 0 ? "No bank account." : "No verified bank account."]);
        if (!verified.Any(x => Payable(x, now)))
            return new("WAITING", "Waiting", [$"Payable from {GlTime.Stamp(verified.Min(PayableFrom), "dd-MMM-yyyy HH:mm")}."]);
        return new("VERIFIED", "Verified", []);
    }

    private CrmBadgeDto ReadyToPay(CrmClient c, CrmBadgeDto compliance, List<CrmClientBankAccount> accounts, DateTime now)
    {
        var reasons = new List<string>();
        if (!c.IsActive) reasons.Add("The customer is inactive.");
        if (!ComplianceOk(compliance)) reasons.Add($"Compliance is {compliance.Label.ToLowerInvariant()}.");
        if (!accounts.Any(x => x.IsPrimary && Payable(x, now)))
            reasons.Add(accounts.Any(x => x.IsPrimary && x.StatusCode == CrmClientBankAccount.Verified)
                ? "The primary bank account is still in its waiting period."
                : "No verified primary bank account.");
        return reasons.Count == 0 ? new("YES", "Ready to pay", []) : new("NO", "Not ready to pay", reasons);
    }

    /// <summary>
    /// What still stops a role from becoming ACTIVE (onboarding depth by role, agreed with Ahmed 4 Nov 2026). No role needs a bank
    /// account: only the party we pay needs one, and that is checked when it is paid (Ready to Pay, Treasury Disburse).
    /// </summary>
    private static List<string> RoleBlockers(string role, CrmClient c, CrmBadgeDto compliance)
    {
        var r = new List<string>();
        if (!c.IsActive) r.Add("The customer is inactive.");
        if (c.IsBlacklisted) r.Add("The customer is blacklisted.");
        switch (role)
        {
            case "CLIENT":
            case "SUPPLIER":
                if (!ComplianceOk(compliance)) r.Add($"Compliance must be clear (now {compliance.Label.ToLowerInvariant()}).");
                break;
            case "DEBTOR":
                if (string.IsNullOrWhiteSpace(c.TaxNumber) && string.IsNullOrWhiteSpace(c.RegistrationNumber))
                    r.Add("A tax number or commercial register number is required.");
                if (c.SanctionsStatus != "CLEAR") r.Add("Sanctions screening must be clear.");
                break;
        }
        return r;
    }

    private static string RoleLabel(string code) => Roles.FirstOrDefault(x => x.Value == code)?.Label ?? code;

    private bool SameUser(string? by) => by is not null && string.Equals(by, User, StringComparison.OrdinalIgnoreCase);

    private CrmBankAccountDto ToDto(CrmClientBankAccount a, Dictionary<long, string> currencies, DateTime now) =>
        new(a.BankAccountId, a.BankName, a.BranchName, a.SwiftCode, a.AccountNumber, a.Iban, a.AccountHolderName, a.AccountHolderNameAr,
            a.CurrencyId, currencies.GetValueOrDefault(a.CurrencyId) ?? "", a.AccountTypeCode, a.IsPrimary, a.StatusCode, a.ChangedBy, a.ChangedAt,
            a.VerifiedBy, a.VerifiedAt, a.VerificationMethodCode, a.Notes, PayableFrom(a), Payable(a, now));

    private static CrmComplianceDto Profile(CrmClient c) =>
        new(c.RiskGrade, c.KycStatus, c.KycExpiryDate, c.AmlStatus, c.AmlCheckDate, c.SanctionsStatus, c.IsPep, c.IsBlacklisted, c.BlacklistReason,
            c.NextReviewDate, null);

    // ---------------------------------------------------------------- compliance

    public async Task SaveComplianceAsync(long clientId, CrmComplianceDto dto, CancellationToken cancellationToken)
    {
        await LoadSettingsAsync(cancellationToken);
        var c = await ClientAsync(clientId, cancellationToken);
        var errors = new Dictionary<string, string[]>();
        void In(string field, string? value, string[] list, string label)
        {
            if (value is null || !list.Contains(value)) errors[field] = [$"Choose the {label}."];
        }
        In("kycStatus", dto.KycStatus, KycStatuses, "KYC status");
        In("amlStatus", dto.AmlStatus, AmlStatuses, "AML status");
        In("sanctionsStatus", dto.SanctionsStatus, SanctionsStatuses, "sanctions status");
        if (dto.RiskGrade is not null && !RiskGrades.Contains(dto.RiskGrade)) errors["riskGrade"] = ["Choose the risk grade."];
        if (dto.KycStatus == "APPROVED" && dto.KycExpiryDate is null) errors["kycExpiryDate"] = ["An approved KYC needs its expiry date."];
        if (dto.IsBlacklisted && string.IsNullOrWhiteSpace(dto.BlacklistReason)) errors["blacklistReason"] = ["Give the blacklist reason."];
        if (errors.Count > 0) throw new BusinessValidationException(errors);

        var reason = dto.IsBlacklisted ? dto.BlacklistReason!.Trim() : null;
        var changed = c.RiskGrade != dto.RiskGrade || c.KycStatus != dto.KycStatus || c.KycExpiryDate != dto.KycExpiryDate
            || c.AmlStatus != dto.AmlStatus || c.AmlCheckDate != dto.AmlCheckDate || c.SanctionsStatus != dto.SanctionsStatus
            || c.IsPep != dto.IsPep || c.IsBlacklisted != dto.IsBlacklisted || c.BlacklistReason != reason || c.NextReviewDate != dto.NextReviewDate;
        if (!changed) return;

        c.RiskGrade = dto.RiskGrade;
        c.KycStatus = dto.KycStatus;
        c.KycExpiryDate = dto.KycExpiryDate;
        c.AmlStatus = dto.AmlStatus;
        c.AmlCheckDate = dto.AmlCheckDate;
        c.SanctionsStatus = dto.SanctionsStatus;
        c.IsPep = dto.IsPep;
        c.IsBlacklisted = dto.IsBlacklisted;
        c.BlacklistReason = reason;
        c.NextReviewDate = dto.NextReviewDate;
        c.ComplianceChangedBy = User;
        c.ComplianceChangedAt = clock.UtcNow;
        c.ComplianceVerifiedBy = null;
        c.ComplianceVerifiedAt = null;
        AddHistory(c, "SAVE", dto.Notes);
        await SuspendRolesIfBlockedAsync(c, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task VerifyComplianceAsync(long clientId, CrmVerifyDto dto, CancellationToken cancellationToken)
    {
        await LoadSettingsAsync(cancellationToken);
        var c = await ClientAsync(clientId, cancellationToken);
        if (c.ComplianceVerifiedAt is not null) throw new BusinessRuleException("The compliance profile is already verified.");
        if (rules.ComplianceMakerChecker && SameUser(c.ComplianceChangedBy))
            throw new BusinessRuleException("You made the last change to this compliance profile. Another user must verify it.");
        c.ComplianceVerifiedBy = User;
        c.ComplianceVerifiedAt = clock.UtcNow;
        c.LastReviewDate = Today;
        // no next review date yet: the review cycle of the customer's risk grade (Module Settings)
        c.NextReviewDate ??= Today.AddMonths(await moduleSettings.ReviewMonthsAsync(c.RiskGrade, cancellationToken));
        AddHistory(c, "VERIFY", dto.Notes);
        await db.SaveChangesAsync(cancellationToken);
    }

    private void AddHistory(CrmClient c, string action, string? notes) =>
        db.Set<CrmClientComplianceHistory>().Add(new CrmClientComplianceHistory
        {
            ClientId = c.ClientId, ActionCode = action, RiskGrade = c.RiskGrade, KycStatus = c.KycStatus, KycExpiryDate = c.KycExpiryDate,
            AmlStatus = c.AmlStatus, AmlCheckDate = c.AmlCheckDate, SanctionsStatus = c.SanctionsStatus, IsPep = c.IsPep,
            IsBlacklisted = c.IsBlacklisted, BlacklistReason = c.BlacklistReason, NextReviewDate = c.NextReviewDate, Notes = Blank(notes),
        });

    /// <summary>A blocked customer cannot keep active roles: they are suspended with the reason.</summary>
    private async Task SuspendRolesIfBlockedAsync(CrmClient c, CancellationToken cancellationToken)
    {
        var badge = ComplianceBadge(c);
        if (badge.Code != "BLOCKED") return;
        foreach (var r in await db.Set<CrmClientRole>().Where(x => x.ClientId == c.ClientId && x.StatusCode == CrmClientRole.Active).ToListAsync(cancellationToken))
        {
            r.StatusCode = CrmClientRole.Suspended;
            r.Notes = $"Suspended: {string.Join(" ", badge.Reasons)}";
        }
    }

    // ---------------------------------------------------------------- roles

    public async Task AddRoleAsync(long clientId, CrmAddRoleDto dto, CancellationToken cancellationToken)
    {
        await ClientAsync(clientId, cancellationToken);
        var code = dto.RoleCode?.Trim().ToUpperInvariant() ?? "";
        if (Roles.All(x => x.Value != code)) throw BusinessValidationException.For("roleCode", "Choose the role.");
        if (await db.Set<CrmClientRole>().AnyAsync(x => x.ClientId == clientId && x.RoleCode == code, cancellationToken))
            throw new BusinessRuleException($"The customer already has the role {RoleLabel(code)}.");
        db.Set<CrmClientRole>().Add(new CrmClientRole { ClientId = clientId, RoleCode = code, StatusCode = CrmClientRole.Onboarding, Notes = Blank(dto.Notes) });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ActivateRoleAsync(long clientId, long roleId, CancellationToken cancellationToken)
    {
        await LoadSettingsAsync(cancellationToken);
        var role = await RoleAsync(clientId, roleId, cancellationToken);
        if (role.StatusCode == CrmClientRole.Active) return;
        var c = await ClientAsync(clientId, cancellationToken);
        var blockers = RoleBlockers(role.RoleCode, c, ComplianceBadge(c));
        if (blockers.Count > 0)
            throw new BusinessRuleException($"The {RoleLabel(role.RoleCode)} role cannot be activated: {string.Join(" ", blockers)}");
        role.StatusCode = CrmClientRole.Active;
        role.ActiveSinceDate ??= Today;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SuspendRoleAsync(long clientId, long roleId, CrmVerifyDto dto, CancellationToken cancellationToken)
    {
        var role = await RoleAsync(clientId, roleId, cancellationToken);
        if (string.IsNullOrWhiteSpace(dto.Notes)) throw BusinessValidationException.For("notes", "Give the reason for the suspension.");
        role.StatusCode = CrmClientRole.Suspended;
        role.Notes = dto.Notes.Trim();
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<CrmClientRole> RoleAsync(long clientId, long roleId, CancellationToken cancellationToken) =>
        await db.Set<CrmClientRole>().SingleOrDefaultAsync(x => x.ClientRoleId == roleId && x.ClientId == clientId, cancellationToken)
        ?? throw new NotFoundException($"Role {roleId} was not found for customer {clientId}.");

    // ---------------------------------------------------------------- bank accounts

    public async Task<long> SaveBankAccountAsync(long clientId, long? bankAccountId, CrmBankAccountFormDto dto, CancellationToken cancellationToken)
    {
        await ClientAsync(clientId, cancellationToken);
        var iban = Blank(dto.Iban)?.Replace(" ", "").ToUpperInvariant();
        var number = Blank(dto.AccountNumber)?.Replace(" ", "");
        var swift = Blank(dto.SwiftCode)?.Replace(" ", "").ToUpperInvariant();
        var type = dto.AccountTypeCode?.Trim().ToUpperInvariant() ?? "";

        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(dto.BankName)) errors["bankName"] = ["Bank Name is required."];
        if (string.IsNullOrWhiteSpace(dto.AccountHolderName)) errors["accountHolderName"] = ["Account Holder Name is required."];
        if (number is null && iban is null) errors["accountNumber"] = ["Enter the account number or the IBAN."];
        if (iban is not null && (iban.Length is < 15 or > 34 || !iban.All(char.IsLetterOrDigit) || !char.IsLetter(iban[0]) || !char.IsLetter(iban[1])))
            errors["iban"] = ["The IBAN must be 15 to 34 letters and digits, starting with the country code."];
        if (swift is not null && (swift.Length is not (8 or 11) || !swift.All(char.IsLetterOrDigit)))
            errors["swiftCode"] = ["The SWIFT / BIC code must be 8 or 11 letters and digits."];
        if (!AccountTypes.Contains(type)) errors["accountTypeCode"] = ["Choose the account type."];
        if (!await db.Set<CrmLkpCurrency>().AnyAsync(x => x.CurrencyId == dto.CurrencyId, cancellationToken)) errors["currencyId"] = ["Choose the currency."];
        if (errors.Count > 0) throw new BusinessValidationException(errors);

        var duplicate = await db.Set<CrmClientBankAccount>().AnyAsync(x => x.ClientId == clientId && x.BankAccountId != (bankAccountId ?? 0)
            && x.StatusCode != CrmClientBankAccount.Suspended && ((iban != null && x.Iban == iban) || (number != null && x.AccountNumber == number)), cancellationToken);
        if (duplicate) throw new BusinessRuleException("The customer already has this account number or IBAN.");

        var now = clock.UtcNow;
        CrmClientBankAccount a;
        if (bankAccountId is { } id)
        {
            a = await AccountAsync(clientId, id, cancellationToken);
            if (a.StatusCode == CrmClientBankAccount.Suspended) throw new BusinessRuleException("Reactivate the account before changing it.");
        }
        else
        {
            a = new CrmClientBankAccount { ClientId = clientId, StatusCode = CrmClientBankAccount.Pending, ChangedBy = User, ChangedAt = now };
            db.Set<CrmClientBankAccount>().Add(a);
        }

        var bankName = dto.BankName.Trim();
        var holder = dto.AccountHolderName.Trim();
        var detailsChanged = bankAccountId is null || a.BankName != bankName || a.BranchName != Blank(dto.BranchName) || a.SwiftCode != swift
            || a.AccountNumber != number || a.Iban != iban || a.AccountHolderName != holder || a.AccountHolderNameAr != Blank(dto.AccountHolderNameAr)
            || a.CurrencyId != dto.CurrencyId || a.AccountTypeCode != type;

        a.BankName = bankName;
        a.BranchName = Blank(dto.BranchName);
        a.SwiftCode = swift;
        a.AccountNumber = number;
        a.Iban = iban;
        a.AccountHolderName = holder;
        a.AccountHolderNameAr = Blank(dto.AccountHolderNameAr);
        a.CurrencyId = dto.CurrencyId;
        a.AccountTypeCode = type;
        a.IsPrimary = dto.IsPrimary;
        a.Notes = Blank(dto.Notes);
        if (detailsChanged)
        {
            // any change of the details needs a new verification and restarts the waiting period
            a.StatusCode = CrmClientBankAccount.Pending;
            a.ChangedBy = User;
            a.ChangedAt = now;
            a.VerifiedBy = null;
            a.VerifiedAt = null;
            a.VerificationMethodCode = null;
        }

        if (a.IsPrimary)
        {
            // one primary account per currency
            foreach (var other in await db.Set<CrmClientBankAccount>()
                         .Where(x => x.ClientId == clientId && x.CurrencyId == a.CurrencyId && x.IsPrimary && x.BankAccountId != (bankAccountId ?? 0))
                         .ToListAsync(cancellationToken))
                other.IsPrimary = false;
        }

        await db.SaveChangesAsync(cancellationToken);
        AddHistory(a, "SAVE", dto.Notes);
        await db.SaveChangesAsync(cancellationToken);
        return a.BankAccountId;
    }

    public async Task VerifyBankAccountAsync(long clientId, long bankAccountId, CrmVerifyDto dto, CancellationToken cancellationToken)
    {
        var a = await AccountAsync(clientId, bankAccountId, cancellationToken);
        if (a.StatusCode != CrmClientBankAccount.Pending) throw new BusinessRuleException("Only a pending bank account can be verified.");
        await LoadSettingsAsync(cancellationToken);
        if (rules.BankAccountMakerChecker && SameUser(a.ChangedBy))
            throw new BusinessRuleException("You made the last change to this bank account. Another user must verify it.");
        var method = dto.VerificationMethodCode?.Trim().ToUpperInvariant();
        if (method is null || !VerificationMethods.Contains(method))
            throw BusinessValidationException.For("verificationMethodCode", "Choose how the account was confirmed.");
        a.StatusCode = CrmClientBankAccount.Verified;
        a.VerifiedBy = User;
        a.VerifiedAt = clock.UtcNow;
        a.VerificationMethodCode = method;
        AddHistory(a, "VERIFY", dto.Notes);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task SuspendBankAccountAsync(long clientId, long bankAccountId, CrmVerifyDto dto, CancellationToken cancellationToken)
    {
        var a = await AccountAsync(clientId, bankAccountId, cancellationToken);
        if (a.StatusCode == CrmClientBankAccount.Suspended) return;
        if (string.IsNullOrWhiteSpace(dto.Notes)) throw BusinessValidationException.For("notes", "Give the reason for the suspension.");
        a.StatusCode = CrmClientBankAccount.Suspended;
        a.IsPrimary = false;
        AddHistory(a, "SUSPEND", dto.Notes);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>A suspended account comes back PENDING: it needs a new verification and a new waiting period.</summary>
    public async Task ReactivateBankAccountAsync(long clientId, long bankAccountId, CrmVerifyDto dto, CancellationToken cancellationToken)
    {
        var a = await AccountAsync(clientId, bankAccountId, cancellationToken);
        if (a.StatusCode != CrmClientBankAccount.Suspended) return;
        a.StatusCode = CrmClientBankAccount.Pending;
        a.ChangedBy = User;
        a.ChangedAt = clock.UtcNow;
        a.VerifiedBy = null;
        a.VerifiedAt = null;
        a.VerificationMethodCode = null;
        AddHistory(a, "REACTIVATE", dto.Notes);
        await db.SaveChangesAsync(cancellationToken);
    }

    private void AddHistory(CrmClientBankAccount a, string action, string? notes) =>
        db.Set<CrmClientBankAccountHistory>().Add(new CrmClientBankAccountHistory
        {
            BankAccountId = a.BankAccountId, ClientId = a.ClientId, ActionCode = action, BankName = a.BankName, BranchName = a.BranchName,
            SwiftCode = a.SwiftCode, AccountNumber = a.AccountNumber, Iban = a.Iban, AccountHolderName = a.AccountHolderName,
            CurrencyId = a.CurrencyId, IsPrimary = a.IsPrimary, StatusCode = a.StatusCode, VerificationMethodCode = a.VerificationMethodCode,
            Notes = Blank(notes),
        });

    private async Task<CrmClientBankAccount> AccountAsync(long clientId, long bankAccountId, CancellationToken cancellationToken) =>
        await db.Set<CrmClientBankAccount>().SingleOrDefaultAsync(x => x.BankAccountId == bankAccountId && x.ClientId == clientId, cancellationToken)
        ?? throw new NotFoundException($"Bank account {bankAccountId} was not found for customer {clientId}.");

    private async Task<CrmClient> ClientAsync(long clientId, CancellationToken cancellationToken) =>
        await db.Set<CrmClient>().SingleOrDefaultAsync(x => x.ClientId == clientId, cancellationToken)
        ?? throw new NotFoundException($"Customer {clientId} was not found.");

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
