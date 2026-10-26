using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.ChartOfAccounts;

/// <summary>
/// The chart of accounts tree (APEX App 120 pages 2/3, process GL_SAVE_ACCOUNT).
/// An account code is always a value of the structure's account segment: saving an account saves that value too,
/// with the same name, parent, dates and flags, so the two never drift apart.
/// </summary>
public sealed class GlChartOfAccountsService(ICapitraDbContext db, IClock clock) : IGlChartOfAccountsService
{
    public async Task<ChartOfAccountsDto> GetAsync(long? coaId, CancellationToken cancellationToken)
    {
        var structures = await db.Set<GlCoaStructure>().AsNoTracking().OrderBy(x => x.CoaCode)
            .Select(x => new CoaOptionDto(x.CoaId, x.CoaCode, x.CoaNameEn, x.DelimiterChar, x.SegmentCount, x.AccountSegmentNum))
            .ToListAsync(cancellationToken);

        var withAccounts = await db.Set<GlAccount>().Select(x => x.CoaId).Distinct().ToListAsync(cancellationToken);

        var selected = structures.FirstOrDefault(x => x.CoaId == coaId)
            ?? structures.FirstOrDefault(x => withAccounts.Contains(x.CoaId))
            ?? structures.FirstOrDefault();

        if (selected is null)
            return new ChartOfAccountsDto(null, [], [], [], TypeDefaults([]), Lists());

        var segments = await db.Set<GlCoaSegment>().AsNoTracking()
            .Where(x => x.CoaId == selected.CoaId).OrderBy(x => x.SegmentNum)
            .Select(x => new AccountSegmentInfoDto(x.SegmentNum, x.SegmentCode, x.SegmentNameEn, x.SegmentType, x.MinLength, x.MaxLength, x.ValueFormat))
            .ToListAsync(cancellationToken);

        var accounts = await (
            from a in db.Set<GlAccount>().AsNoTracking()
            where a.CoaId == selected.CoaId
            join v in db.Set<GlCoaSegmentValue>() on a.AccountSegmentValueId equals v.SegmentValueId into values
            from v in values.DefaultIfEmpty()
            orderby a.SortOrder, a.AccountCode
            select ToDto(a, v.ValueCode)
        ).ToListAsync(cancellationToken);

        return new ChartOfAccountsDto(selected.CoaId, structures, segments, accounts, TypeDefaults(accounts), Lists());
    }

    public async Task<GlAccountDto> CreateAsync(SaveGlAccountRequest request, CancellationToken cancellationToken)
    {
        if (request.CoaId is not { } coaId || !await db.Set<GlCoaStructure>().AnyAsync(x => x.CoaId == coaId, cancellationToken))
            throw BusinessValidationException.For("coaId", "Choose the account structure first.");

        var entity = new GlAccount { CoaId = coaId };
        return await SaveAsync(entity, request, isNew: true, cancellationToken);
    }

    public async Task<GlAccountDto> UpdateAsync(long accountId, SaveGlAccountRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Set<GlAccount>().FirstOrDefaultAsync(x => x.AccountId == accountId, cancellationToken)
            ?? throw new NotFoundException("Account not found.");

        if (request.RowVersion != entity.RowVersion)
            throw new ConflictException("This account was changed by someone else. Reload the page and try again.");

        if (GlRules.Code(request.AccountCode) is { } code && code != entity.AccountCode)
            throw BusinessValidationException.For("accountCode", "The account code cannot be changed: it is the value of the account segment.");

        return await SaveAsync(entity, request, isNew: false, cancellationToken);
    }

    private async Task<GlAccountDto> SaveAsync(GlAccount entity, SaveGlAccountRequest r, bool isNew, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();
        var segment = await AccountSegmentAsync(entity.CoaId, cancellationToken);

        var code = isNew ? GlRules.Code(r.AccountCode) : entity.AccountCode;
        if (code is null)
            errors.Add("accountCode", "Account code is required.");
        else if (isNew)
        {
            GlRules.CheckValueCode(errors, "accountCode", code, segment);
            if (await db.Set<GlAccount>().AnyAsync(x => x.CoaId == entity.CoaId && x.AccountCode == code, cancellationToken))
                errors.Add("accountCode", $"Account {code} already exists.");
        }

        var nameEn = errors.Text("accountNameEn", "Name (English)", r.AccountNameEn, 240, required: true);
        var nameAr = errors.Text("accountNameAr", "Name (Arabic)", r.AccountNameAr, 240);
        var description = errors.Text("description", "Description", r.Description, 1000);
        var type = Pick(errors, "accountType", "Account type", r.AccountType, GlCodes.AccountTypes);
        var normalBalance = Pick(errors, "normalBalance", "Normal balance", r.NormalBalance, GlCodes.NormalBalances);
        var statement = Pick(errors, "financialStatementClass", "Financial statement class", r.FinancialStatementClass, GlCodes.FinancialStatementClasses);
        var cashFlow = Pick(errors, "cashFlowClass", "Cash flow class", r.CashFlowClass, GlCodes.CashFlowClasses);
        GlRules.CheckDates(errors, r.EffectiveStartDate ?? (isNew ? null : entity.EffectiveStartDate), r.EffectiveEndDate);

        GlAccount? parent = null;
        if (r.ParentAccountId is { } parentId)
        {
            parent = await db.Set<GlAccount>().AsNoTracking().FirstOrDefaultAsync(x => x.AccountId == parentId, cancellationToken);
            if (parent is null)
                errors.Add("parentAccountId", "The parent account does not exist.");
            else if (parent.CoaId != entity.CoaId)
                errors.Add("parentAccountId", "The parent account belongs to another account structure.");
            else if (!isNew && await IsSelfOrBelowAsync(entity.AccountId, parentId, cancellationToken))
                errors.Add("parentAccountId", "The selected parent is this account or one below it. This would create a circular hierarchy.");
        }

        errors.ThrowIfAny();

        // A summary account never allows posting.
        var posting = !r.IsSummary && r.IsPostingAllowed;
        var start = r.EffectiveStartDate ?? (isNew ? GlRules.Today(clock) : entity.EffectiveStartDate);

        if (isNew)
        {
            var siblings = db.Set<GlAccount>().Where(x => x.CoaId == entity.CoaId && x.ParentAccountId == r.ParentAccountId);
            entity.SortOrder = (await siblings.MaxAsync(x => (int?)x.SortOrder, cancellationToken) ?? 0) + 10;
            entity.AccountCode = code!;
        }

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);

        // 1. The account segment value, kept in step with the account. An existing value keeps its own references.
        var value = await db.Set<GlCoaSegmentValue>()
            .FirstOrDefaultAsync(x => x.CoaSegmentId == segment.CoaSegmentId && x.ValueCode == entity.AccountCode, cancellationToken);
        if (value is null)
        {
            value = new GlCoaSegmentValue { CoaSegmentId = segment.CoaSegmentId, ValueCode = entity.AccountCode, SortOrder = entity.SortOrder };
            db.Add(value);
        }

        value.ValueNameEn = nameEn!;
        value.ValueNameAr = nameAr;
        value.Description = description;
        value.ParentValueId = parent?.AccountSegmentValueId;
        value.IsSummary = r.IsSummary;
        value.IsPostingAllowed = posting;
        value.IsEnabled = r.IsActive;
        value.EffectiveStartDate = start;
        value.EffectiveEndDate = r.EffectiveEndDate;
        await db.SaveChangesAsync(cancellationToken);

        // 2. Only one account of a structure is the retained earnings account.
        if (r.IsRetainedEarnings)
        {
            var others = await db.Set<GlAccount>()
                .Where(x => x.CoaId == entity.CoaId && x.IsRetainedEarnings && x.AccountCode != entity.AccountCode)
                .ToListAsync(cancellationToken);
            others.ForEach(x => x.IsRetainedEarnings = false);
        }

        // 3. The account, linked to that segment value.
        entity.AccountNameEn = nameEn!;
        entity.AccountNameAr = nameAr;
        entity.Description = description;
        entity.ParentAccountId = r.ParentAccountId;
        entity.AccountSegmentValueId = value.SegmentValueId;
        entity.AccountType = type!;
        entity.NormalBalance = normalBalance!;
        entity.FinancialStatementClass = statement!;
        entity.CashFlowClass = cashFlow!;
        entity.IsSummary = r.IsSummary;
        entity.IsPostingAllowed = posting;
        entity.IsControlAccount = r.IsControlAccount;
        entity.IsReconciliation = r.IsReconciliation;
        entity.IsBranchRequired = r.IsBranchRequired;
        entity.IsCostCenterRequired = r.IsCostCenterRequired;
        entity.IsBusinessLineRequired = r.IsBusinessLineRequired;
        entity.IsManualJournalAllowed = r.IsManualJournalAllowed;
        entity.IsSubledgerPostingAllowed = r.IsSubledgerPostingAllowed;
        entity.IsCurrencyRevaluation = r.IsCurrencyRevaluation;
        entity.IsRetainedEarnings = r.IsRetainedEarnings;
        entity.IsActive = r.IsActive;
        entity.EffectiveStartDate = start;
        entity.EffectiveEndDate = r.EffectiveEndDate;

        if (isNew)
            db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToDto(entity, value.ValueCode);
    }

    /// <summary>The structure's account segment, which decides what an account code may look like.</summary>
    private async Task<GlCoaSegment> AccountSegmentAsync(long coaId, CancellationToken cancellationToken)
    {
        var structure = await db.Set<GlCoaStructure>().AsNoTracking().FirstAsync(x => x.CoaId == coaId, cancellationToken);
        var segment = await db.Set<GlCoaSegment>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.CoaId == coaId && x.SegmentNum == structure.AccountSegmentNum, cancellationToken)
            ?? throw BusinessValidationException.For("accountCode",
                "The account structure has no account segment. Define it on the Account structure page first.");

        if (segment.SegmentType != GlCodes.SegmentTypeAccount)
            throw BusinessValidationException.For("accountCode",
                $"The structure's account segment is {segment.SegmentCode} (type {segment.SegmentType}), not an ACCOUNT segment. Fix it on the Account structure page first.");

        return segment;
    }

    private static string? Pick(ValidationErrors errors, string field, string label, string? value, string[] allowed)
    {
        var code = GlRules.Code(value);
        if (code is null)
            errors.Add(field, $"{label} is required.");
        else if (!allowed.Contains(code))
            errors.Add(field, $"Unknown {label.ToLowerInvariant()} {code}.");
        return code;
    }

    private async Task<bool> IsSelfOrBelowAsync(long accountId, long candidateId, CancellationToken cancellationToken)
    {
        long? current = candidateId;
        for (var guard = 0; current is { } id && guard < 100; guard++)
        {
            if (id == accountId)
                return true;
            current = await db.Set<GlAccount>().Where(x => x.AccountId == id).Select(x => x.ParentAccountId).FirstOrDefaultAsync(cancellationToken);
        }
        return false;
    }

    /// <summary>For each type, the classification most accounts of that type already use; the usual one when there are none.</summary>
    private static List<AccountTypeDefaultDto> TypeDefaults(IReadOnlyList<GlAccountDto> accounts) =>
        GlCodes.AccountTypes.Select(type =>
        {
            var usual = accounts.Where(a => a.AccountType == type)
                .GroupBy(a => (a.NormalBalance, a.FinancialStatementClass, a.CashFlowClass))
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault();

            if (usual.NormalBalance is not null)
                return new AccountTypeDefaultDto(type, usual.NormalBalance, usual.FinancialStatementClass, usual.CashFlowClass);

            var (normalBalance, statement) = GlCodes.DefaultsFor(type);
            return new AccountTypeDefaultDto(type, normalBalance, statement, GlCodes.CashFlowNone);
        }).ToList();

    private static AccountListsDto Lists() =>
        new(GlCodes.AccountTypes, GlCodes.NormalBalances, GlCodes.FinancialStatementClasses, GlCodes.CashFlowClasses);

    private static GlAccountDto ToDto(GlAccount a, string? segmentValueCode) => new(
        a.AccountId, a.AccountCode, a.AccountNameEn, a.AccountNameAr, a.Description, a.ParentAccountId, a.AccountType,
        a.NormalBalance, a.FinancialStatementClass, a.CashFlowClass, a.IsSummary, a.IsPostingAllowed, a.IsControlAccount,
        a.IsReconciliation, a.IsBranchRequired, a.IsCostCenterRequired, a.IsBusinessLineRequired, a.IsManualJournalAllowed,
        a.IsSubledgerPostingAllowed, a.IsCurrencyRevaluation, a.IsRetainedEarnings, a.IsActive, a.EffectiveStartDate,
        a.EffectiveEndDate, a.SortOrder, segmentValueCode, a.CreatedBy, a.CreatedAt, a.UpdatedBy, a.UpdatedAt, a.RowVersion);
}
