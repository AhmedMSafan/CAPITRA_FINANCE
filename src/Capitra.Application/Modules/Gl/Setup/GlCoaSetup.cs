using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Setup;

/// <summary>
/// C# port of FINOVA gl_coa_setup_pkg (save_coa_structure, save_coa_segment, save_segment_value)
/// and gl_core_audit_pkg.log_setup_change. Each save updates the row with the same code or inserts it.
/// </summary>
public sealed class GlCoaSetup(ICapitraDbContext db, IClock clock)
{
    public sealed record StructureInput(
        string? CoaCode, string? CoaNameEn, string? CoaNameAr, string? Description, string? DelimiterChar,
        int SegmentCount, int? BalancingSegmentNum, int AccountSegmentNum, int? BranchSegmentNum, int? CostCenterSegmentNum,
        int? BusinessLineSegmentNum, int? IntercompanySegmentNum, string? AllowDynamicCombinations,
        string? RequireCombinationApproval, string? ActiveFlag, DateOnly? EffectiveStartDate, DateOnly? EffectiveEndDate);

    public sealed record SegmentInput(
        string? CoaCode, int SegmentNum, string? SegmentCode, string? SegmentNameEn, string? SegmentNameAr, string? SegmentType,
        int? MinLength, int? MaxLength, string? ValueFormat, string? PadChar, string? UppercaseFlag, string? RequiredFlag,
        string? AllowSummaryValues, string? EnabledFlag, int? DisplaySeq, string? Description);

    public sealed record SegmentValueInput(
        string? CoaCode, string? SegmentCode, string? ValueCode, string? ValueNameEn, string? ValueNameAr, string? Description,
        string? ParentValueCode, string? SummaryFlag, string? PostingAllowedFlag, string? EnabledFlag,
        DateOnly? EffectiveStartDate, DateOnly? EffectiveEndDate, long? SortOrder, string? RefLegalEntityCode,
        string? RefBranchCode, string? RefCostCenterCode, string? RefBusinessLineCode);

    public static string? NormalizeCode(string? code) => code?.Trim().ToUpperInvariant();

    /// <summary>UPPER(NVL(flag, default)) = 'Y'.</summary>
    private static bool Flag(string? value, bool whenNull) =>
        value is null ? whenNull : string.Equals(value.Trim(), "Y", StringComparison.OrdinalIgnoreCase);

    private DateOnly Today => DateOnly.FromDateTime(clock.UtcNow);

    public async Task<long> SaveCoaStructureAsync(StructureInput p, CancellationToken ct)
    {
        var code = NormalizeCode(p.CoaCode) ?? string.Empty;
        var row = await db.Set<GlCoaStructure>().SingleOrDefaultAsync(x => x.CoaCode == code, ct);
        var created = row is null;
        if (row is null)
        {
            row = new GlCoaStructure { CoaCode = code };
            db.Add(row);
        }

        row.CoaNameEn = p.CoaNameEn ?? string.Empty;
        row.CoaNameAr = p.CoaNameAr;
        row.Description = p.Description;
        row.DelimiterChar = p.DelimiterChar ?? "-";
        row.SegmentCount = p.SegmentCount;
        row.BalancingSegmentNum = p.BalancingSegmentNum;
        row.AccountSegmentNum = p.AccountSegmentNum;
        row.BranchSegmentNum = p.BranchSegmentNum;
        row.CostCenterSegmentNum = p.CostCenterSegmentNum;
        row.BusinessLineSegmentNum = p.BusinessLineSegmentNum;
        row.IntercompanySegmentNum = p.IntercompanySegmentNum;
        row.IsDynamicCombinationsAllowed = Flag(p.AllowDynamicCombinations, true);
        row.IsCombinationApprovalRequired = Flag(p.RequireCombinationApproval, false);
        row.IsActive = Flag(p.ActiveFlag, true);
        row.EffectiveStartDate = p.EffectiveStartDate ?? Today;
        row.EffectiveEndDate = p.EffectiveEndDate;

        LogChange("GL_COA_STRUCTURES", code, created ? "CREATE" : "UPDATE", created ? "COA structure created." : "COA structure updated.");
        await db.SaveChangesAsync(ct);
        return row.CoaId;
    }

    public async Task<long> SaveCoaSegmentAsync(SegmentInput p, CancellationToken ct)
    {
        var coaCode = NormalizeCode(p.CoaCode);
        var coa = await db.Set<GlCoaStructure>().SingleOrDefaultAsync(x => x.CoaCode == coaCode, ct)
            ?? throw new ApexProcessException("COA does not exist: " + p.CoaCode);
        var code = NormalizeCode(p.SegmentCode) ?? string.Empty;

        var row = await db.Set<GlCoaSegment>().SingleOrDefaultAsync(x => x.CoaId == coa.CoaId && x.SegmentCode == code, ct);
        var created = row is null;
        if (row is null)
        {
            row = new GlCoaSegment { CoaId = coa.CoaId, SegmentCode = code };
            db.Add(row);
        }

        row.SegmentNum = p.SegmentNum;
        row.SegmentNameEn = p.SegmentNameEn ?? string.Empty;
        row.SegmentNameAr = p.SegmentNameAr;
        row.SegmentType = NormalizeCode(p.SegmentType) ?? string.Empty;
        row.MinLength = p.MinLength ?? 1;
        row.MaxLength = p.MaxLength ?? 30;
        row.ValueFormat = NormalizeCode(p.ValueFormat ?? "ALPHANUMERIC")!;
        row.PadChar = p.PadChar;
        row.IsUppercase = Flag(p.UppercaseFlag, true);
        row.IsRequired = Flag(p.RequiredFlag, true);
        row.IsSummaryValuesAllowed = Flag(p.AllowSummaryValues, true);
        row.IsEnabled = Flag(p.EnabledFlag, true);
        row.DisplaySeq = p.DisplaySeq ?? 10;
        row.Description = p.Description;

        LogChange("GL_COA_SEGMENTS", p.CoaCode + ":" + code, created ? "CREATE" : "UPDATE", created ? "COA segment created." : "COA segment updated.");
        await db.SaveChangesAsync(ct);
        return row.CoaSegmentId;
    }

    public async Task<long> SaveSegmentValueAsync(SegmentValueInput p, CancellationToken ct)
    {
        var coaCode = NormalizeCode(p.CoaCode);
        var segCode = NormalizeCode(p.SegmentCode);
        var segment = await (from s in db.Set<GlCoaSegment>()
                             join c in db.Set<GlCoaStructure>() on s.CoaId equals c.CoaId
                             where c.CoaCode == coaCode && s.SegmentCode == segCode
                             select s).SingleOrDefaultAsync(ct)
            ?? throw new ApexProcessException("COA segment does not exist: " + p.CoaCode + " / " + p.SegmentCode);

        long? parentId = null;
        if (p.ParentValueCode is not null)
        {
            var parentCode = NormalizeCode(p.ParentValueCode);
            parentId = await db.Set<GlCoaSegmentValue>()
                .Where(v => v.CoaSegmentId == segment.CoaSegmentId && v.ValueCode == parentCode)
                .Select(v => (long?)v.SegmentValueId).SingleOrDefaultAsync(ct)
                ?? throw new ApexProcessException("Parent segment value does not exist: " + p.ParentValueCode);
        }

        var leCode = NormalizeCode(p.RefLegalEntityCode);
        long? legalEntityId = leCode is null ? null : await db.Set<FinLegalEntity>()
            .Where(x => x.LegalEntityCode == leCode).Select(x => (long?)x.LegalEntityId).SingleOrDefaultAsync(ct);
        var branchId = await BranchIdAsync(leCode, NormalizeCode(p.RefBranchCode), ct);
        var costCenterId = await CostCenterIdAsync(leCode, NormalizeCode(p.RefCostCenterCode), ct);

        string? businessLine = null;
        if (p.RefBusinessLineCode is not null)
        {
            businessLine = NormalizeCode(p.RefBusinessLineCode);
            if (!await db.Set<FinBusinessLine>().AnyAsync(x => x.BusinessLineCode == businessLine, ct))
                throw new ApexProcessException("Business line does not exist: " + p.RefBusinessLineCode);
        }

        var code = NormalizeCode(p.ValueCode) ?? string.Empty;
        var row = await db.Set<GlCoaSegmentValue>().SingleOrDefaultAsync(x => x.CoaSegmentId == segment.CoaSegmentId && x.ValueCode == code, ct);
        var created = row is null;
        if (row is null)
        {
            row = new GlCoaSegmentValue { CoaSegmentId = segment.CoaSegmentId, ValueCode = code };
            db.Add(row);
        }

        row.ValueNameEn = p.ValueNameEn ?? string.Empty;
        row.ValueNameAr = p.ValueNameAr;
        row.Description = p.Description;
        row.ParentValueId = parentId;
        row.IsSummary = Flag(p.SummaryFlag, false);
        row.IsPostingAllowed = Flag(p.PostingAllowedFlag, true);
        row.IsEnabled = Flag(p.EnabledFlag, true);
        row.EffectiveStartDate = p.EffectiveStartDate ?? Today;
        row.EffectiveEndDate = p.EffectiveEndDate;
        row.SortOrder = p.SortOrder ?? 10;
        row.ReferenceLegalEntityId = legalEntityId;
        row.ReferenceBranchId = branchId;
        row.ReferenceCostCenterId = costCenterId;
        row.ReferenceBusinessLineCode = businessLine;

        LogChange("GL_COA_SEGMENT_VALUES", p.CoaCode + ":" + p.SegmentCode + ":" + code, created ? "CREATE" : "UPDATE",
            created ? "COA segment value created." : "COA segment value updated.");
        await db.SaveChangesAsync(ct);
        return row.SegmentValueId;
    }

    public sealed record AccountInput(
        string? CoaCode, string? AccountCode, string? AccountNameEn, string? AccountNameAr, string? Description,
        string? ParentAccountCode, string? AccountSegmentValueCode, string? AccountType, string? NormalBalance,
        string? FinancialStatementClass, string? CashFlowClass, string? SummaryFlag, string? PostingAllowedFlag,
        string? ControlAccountFlag, string? ReconciliationFlag, string? RequireBranchFlag, string? RequireCostCenterFlag,
        string? RequireBusinessLineFlag, string? ManualJournalAllowedFlag, string? SubledgerPostingAllowedFlag,
        string? CurrencyRevaluationFlag, string? RetainedEarningsFlag, string? ActiveFlag, DateOnly? EffectiveStartDate,
        DateOnly? EffectiveEndDate, long? SortOrder);

    /// <summary>save_account: the account is linked to the value of the NATURAL segment with the same code.</summary>
    public async Task<long> SaveAccountAsync(AccountInput p, CancellationToken ct)
    {
        var coaCode = NormalizeCode(p.CoaCode);
        var coa = await db.Set<GlCoaStructure>().SingleOrDefaultAsync(x => x.CoaCode == coaCode, ct)
            ?? throw new ApexProcessException("COA does not exist: " + p.CoaCode);
        var code = NormalizeCode(p.AccountCode) ?? string.Empty;

        long? parentId = null;
        if (p.ParentAccountCode is not null)
        {
            var parentCode = NormalizeCode(p.ParentAccountCode);
            parentId = await db.Set<GlAccount>().Where(a => a.CoaId == coa.CoaId && a.AccountCode == parentCode)
                .Select(a => (long?)a.AccountId).SingleOrDefaultAsync(ct)
                ?? throw new ApexProcessException("Parent natural account does not exist: " + p.ParentAccountCode);
        }

        var natural = NormalizeCode(p.AccountSegmentValueCode) ?? code;
        var segmentValueId = await (from v in db.Set<GlCoaSegmentValue>()
                                    join s in db.Set<GlCoaSegment>() on v.CoaSegmentId equals s.CoaSegmentId
                                    where s.CoaId == coa.CoaId && s.SegmentCode == "NATURAL" && v.ValueCode == natural
                                    select (long?)v.SegmentValueId).SingleOrDefaultAsync(ct);

        var retained = Flag(p.RetainedEarningsFlag, false);
        if (retained)
        {
            foreach (var other in await db.Set<GlAccount>()
                         .Where(a => a.CoaId == coa.CoaId && a.AccountCode != code && a.IsRetainedEarnings).ToListAsync(ct))
                other.IsRetainedEarnings = false;
        }

        var row = await db.Set<GlAccount>().SingleOrDefaultAsync(a => a.CoaId == coa.CoaId && a.AccountCode == code, ct);
        var created = row is null;
        if (row is null)
        {
            row = new GlAccount { CoaId = coa.CoaId, AccountCode = code };
            db.Add(row);
        }

        row.AccountNameEn = p.AccountNameEn ?? string.Empty;
        row.AccountNameAr = p.AccountNameAr;
        row.Description = p.Description;
        row.ParentAccountId = parentId;
        row.AccountSegmentValueId = segmentValueId;
        row.AccountType = NormalizeCode(p.AccountType) ?? string.Empty;
        row.NormalBalance = NormalizeCode(p.NormalBalance) ?? string.Empty;
        row.FinancialStatementClass = NormalizeCode(p.FinancialStatementClass) ?? string.Empty;
        row.CashFlowClass = NormalizeCode(p.CashFlowClass ?? "NONE")!;
        row.IsSummary = Flag(p.SummaryFlag, false);
        row.IsPostingAllowed = Flag(p.PostingAllowedFlag, true);
        row.IsControlAccount = Flag(p.ControlAccountFlag, false);
        row.IsReconciliation = Flag(p.ReconciliationFlag, false);
        row.IsBranchRequired = Flag(p.RequireBranchFlag, false);
        row.IsCostCenterRequired = Flag(p.RequireCostCenterFlag, false);
        row.IsBusinessLineRequired = Flag(p.RequireBusinessLineFlag, false);
        row.IsManualJournalAllowed = Flag(p.ManualJournalAllowedFlag, true);
        row.IsSubledgerPostingAllowed = Flag(p.SubledgerPostingAllowedFlag, true);
        row.IsCurrencyRevaluation = Flag(p.CurrencyRevaluationFlag, false);
        row.IsRetainedEarnings = retained;
        row.IsActive = Flag(p.ActiveFlag, true);
        row.EffectiveStartDate = p.EffectiveStartDate ?? Today;
        row.EffectiveEndDate = p.EffectiveEndDate;
        row.SortOrder = p.SortOrder ?? 10;

        LogChange("GL_ACCOUNTS", p.CoaCode + ":" + code, created ? "CREATE" : "UPDATE", created ? "Natural account created." : "Natural account updated.");
        await db.SaveChangesAsync(ct);
        return row.AccountId;
    }

    private async Task<long?> BranchIdAsync(string? leCode, string? branchCode, CancellationToken ct)
    {
        if (branchCode is null)
            return null;

        return await (from b in db.Set<FinBranch>()
                      join le in db.Set<FinLegalEntity>() on b.LegalEntityId equals le.LegalEntityId into les
                      from le in les.DefaultIfEmpty()
                      where b.BranchCode == branchCode && (leCode == null || le.LegalEntityCode == leCode)
                      select (long?)b.BranchId).FirstOrDefaultAsync(ct);
    }

    private async Task<long?> CostCenterIdAsync(string? leCode, string? costCenterCode, CancellationToken ct)
    {
        if (costCenterCode is null)
            return null;

        return await (from cc in db.Set<FinCostCenter>()
                      join le in db.Set<FinLegalEntity>() on cc.LegalEntityId equals le.LegalEntityId into les
                      from le in les.DefaultIfEmpty()
                      where cc.CostCenterCode == costCenterCode && (leCode == null || le.LegalEntityCode == leCode)
                      select (long?)cc.CostCenterId).FirstOrDefaultAsync(ct);
    }

    /// <summary>gl_core_audit_pkg.log_setup_change: one GL_SETUP_AUDITS row per saved setup record.</summary>
    private void LogChange(string entityName, string entityKey, string actionCode, string summary) =>
        db.Add(new GlSetupAudit
        {
            EntityName = entityName.Trim().ToUpperInvariant(),
            EntityKey = entityKey.Trim(),
            ActionCode = actionCode,
            ChangeSummary = summary,
            SourceAppId = 120,
        });
}
