using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl.Setup;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Fin;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexJson;

namespace Capitra.Application.Modules.Gl.Pages;

/// <summary>App 120 page 40 "Account Structure and Segments": the six on-demand processes, ported from PL/SQL.</summary>
public abstract class P40Process : IApexProcess
{
    public int App => 120;

    public int Page => 40;

    public abstract string Name { get; }

    public virtual string Permission => Permissions.Gl.SetupEdit;

    public abstract Task<object> RunAsync(ApexRequest request, CancellationToken ct);

    /// <summary>Every save process answers {ok:false, message} instead of failing, so the page shows the message.</summary>
    protected static async Task<object> Guard(ICapitraDbContext db, Func<Task<object>> body)
    {
        try
        {
            return await body();
        }
        catch (Exception ex) when (ex is ApexProcessException or Common.Exceptions.ConflictException or FormatException)
        {
            db.DiscardChanges();
            return new { ok = false, message = ex.Message };
        }
    }
}

/// <summary>SEG_LOAD: structures, the chosen structure's segments, ledgers, combination count and pick lists.</summary>
public sealed class P40SegLoad(ICapitraDbContext db) : P40Process
{
    public override string Name => "SEG_LOAD";

    public override string Permission => Permissions.Gl.SetupView;

    public override async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        long? coa = Text(request.X01) is { } x ? long.Parse(x, System.Globalization.CultureInfo.InvariantCulture) : null;
        var structures = db.Set<GlCoaStructure>().AsNoTracking();
        coa ??= await structures.Where(s => s.IsActive).MinAsync(s => (long?)s.CoaId, ct);
        coa ??= await structures.MinAsync(s => (long?)s.CoaId, ct);

        var structureRows = await structures.OrderBy(s => s.CoaCode).ToListAsync(ct);
        var segments = await db.Set<GlCoaSegment>().AsNoTracking().Where(s => s.CoaId == coa).OrderBy(s => s.SegmentNum)
            .Select(s => new { s, n = db.Set<GlCoaSegmentValue>().Count(v => v.CoaSegmentId == s.CoaSegmentId) })
            .ToListAsync(ct);

        return new
        {
            ok = true,
            coaId = coa,
            structures = structureRows.Select(r => new
            {
                id = r.CoaId, code = r.CoaCode, nameEn = r.CoaNameEn, nameAr = r.CoaNameAr, description = r.Description,
                delimiter = r.DelimiterChar, segCount = r.SegmentCount, balancing = r.BalancingSegmentNum, account = r.AccountSegmentNum,
                branch = r.BranchSegmentNum, costCenter = r.CostCenterSegmentNum, businessLine = r.BusinessLineSegmentNum,
                intercompany = r.IntercompanySegmentNum, dynamic = Yn(r.IsDynamicCombinationsAllowed),
                approval = Yn(r.IsCombinationApprovalRequired), active = Yn(r.IsActive),
                start = Ymd(r.EffectiveStartDate), end = Ymd(r.EffectiveEndDate),
            }),
            segments = segments.Select(x => new
            {
                id = x.s.CoaSegmentId, num = x.s.SegmentNum, code = x.s.SegmentCode, nameEn = x.s.SegmentNameEn, nameAr = x.s.SegmentNameAr,
                type = x.s.SegmentType, minLen = x.s.MinLength, maxLen = x.s.MaxLength, format = x.s.ValueFormat, pad = x.s.PadChar,
                upper = Yn(x.s.IsUppercase), required = Yn(x.s.IsRequired), summary = Yn(x.s.IsSummaryValuesAllowed),
                enabled = Yn(x.s.IsEnabled), seq = x.s.DisplaySeq, description = x.s.Description, values = x.n,
                defReady = true, defValue = x.s.DefaultValueCode, defMode = x.s.DefaultMode,
            }),
            ledgers = await db.Set<GlLedger>().AsNoTracking().Where(l => l.CoaId == coa).OrderBy(l => l.LedgerCode).Select(l => l.LedgerCode).ToListAsync(ct),
            combinations = await db.Set<GlAccountCombination>().CountAsync(c => c.CoaId == coa, ct),
            types = await db.Set<GlCoaSegment>().Select(s => s.SegmentType).Distinct().OrderBy(s => s).ToListAsync(ct),
            formats = await db.Set<GlCoaSegment>().Select(s => s.ValueFormat).Distinct().OrderBy(s => s).ToListAsync(ct),
            legalEntities = await db.Set<FinLegalEntity>().AsNoTracking().OrderBy(l => l.LegalEntityCode)
                .Select(l => new { code = l.LegalEntityCode, name = l.LegalEntityNameEn }).ToListAsync(ct),
            businessLines = await db.Set<FinBusinessLine>().AsNoTracking().OrderBy(b => b.SortOrder).ThenBy(b => b.BusinessLineCode)
                .Select(b => new { code = b.BusinessLineCode, name = b.BusinessLineName }).ToListAsync(ct),
        };
    }
}

/// <summary>SEG_SAVE_DEFAULT: the default value (and SUGGEST or FIXED mode) new journal lines get for a segment.</summary>
public sealed class P40SegSaveDefault(ICapitraDbContext db) : P40Process
{
    public override string Name => "SEG_SAVE_DEFAULT";

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Guard(db, async () =>
    {
        var segId = long.Parse(request.X01 ?? string.Empty, System.Globalization.CultureInfo.InvariantCulture);
        var value = Text(request.X02)?.ToUpperInvariant();
        var mode = Text(request.X03)?.ToUpperInvariant() ?? "SUGGEST";
        var segment = await db.Set<GlCoaSegment>().SingleOrDefaultAsync(s => s.CoaSegmentId == segId, ct)
            ?? throw new ApexProcessException("No data found.");

        if (segment.SegmentType == "ACCOUNT" && value is not null)
            throw new ApexProcessException("The account segment has no default: the account is chosen on each journal line.");
        if (mode is not ("SUGGEST" or "FIXED"))
            throw new ApexProcessException("Unknown default mode " + mode);
        if (value is not null && !await db.Set<GlCoaSegmentValue>().AnyAsync(
                v => v.CoaSegmentId == segId && v.ValueCode == value && v.IsEnabled && v.IsPostingAllowed, ct))
            throw new ApexProcessException("Value " + value + " is not an enabled posting value of this segment.");

        segment.DefaultValueCode = value;
        segment.DefaultMode = value is null ? "SUGGEST" : mode;
        await db.SaveChangesAsync(ct);
        return new { ok = true };
    });
}

/// <summary>SEG_SAVE_SEGMENT: gl_coa_setup_pkg.save_coa_segment with the page's JSON.</summary>
public sealed class P40SegSaveSegment(ICapitraDbContext db, GlCoaSetup setup) : P40Process
{
    public override string Name => "SEG_SAVE_SEGMENT";

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Guard(db, async () =>
    {
        var j = new ApexJson(request.X01);
        var id = await setup.SaveCoaSegmentAsync(new GlCoaSetup.SegmentInput(
            j.Str("coa"), j.Number("num") ?? 0, j.Str("code")?.ToUpperInvariant(), j.Str("nameEn"), j.Str("nameAr"), j.Str("type"),
            j.Number("minLen"), j.Number("maxLen"), j.Str("format"), j.Raw("pad"), j.Str("upper"), j.Str("required"), j.Str("summary"),
            j.Str("enabled"), j.Number("seq"), j.Str("description")), ct);
        return new { ok = true, id };
    });
}

/// <summary>SEG_SAVE_STRUCT: checks the segment numbers, then gl_coa_setup_pkg.save_coa_structure.</summary>
public sealed class P40SegSaveStruct(ICapitraDbContext db, GlCoaSetup setup) : P40Process
{
    public override string Name => "SEG_SAVE_STRUCT";

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Guard(db, async () =>
    {
        var j = new ApexJson(request.X01);
        var count = j.Number("segCount");
        if (count is null or < 1 or > 10)
            throw new ApexProcessException("The number of segments must be between 1 and 10.");
        if (j.Number("account") is null)
            throw new ApexProcessException("The account segment is required.");

        foreach (var (label, key) in new[] { ("Account", "account"), ("Balancing", "balancing"), ("Branch", "branch"),
                     ("Cost centre", "costCenter"), ("Business line", "businessLine"), ("Intercompany", "intercompany") })
        {
            if (j.Number(key) is { } n && (n < 1 || n > count))
                throw new ApexProcessException($"{label} segment must be between 1 and {count}.");
        }

        var id = await setup.SaveCoaStructureAsync(new GlCoaSetup.StructureInput(
            j.Str("code")?.ToUpperInvariant(), j.Str("nameEn"), j.Str("nameAr"), j.Str("description"), j.Str("delimiter") ?? "-",
            count.Value, j.Number("balancing"), j.Number("account")!.Value, j.Number("branch"), j.Number("costCenter"), j.Number("businessLine"),
            j.Number("intercompany"), j.Str("dynamic"), j.Str("approval"), j.Str("active"), j.Date("start"), j.Date("end")), ct);
        return new { ok = true, id };
    });
}

/// <summary>SEG_SAVE_VALUE: gl_coa_setup_pkg.save_segment_value, keeping the branch and cost centre links an edited value had.</summary>
public sealed class P40SegSaveValue(ICapitraDbContext db, GlCoaSetup setup) : P40Process
{
    public override string Name => "SEG_SAVE_VALUE";

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => Guard(db, async () =>
    {
        var j = new ApexJson(request.X01);
        var oldId = j.Key("id");
        long? branch = null, costCenter = null;
        if (oldId is not null)
        {
            var old = await db.Set<GlCoaSegmentValue>().AsNoTracking().SingleAsync(v => v.SegmentValueId == oldId, ct);
            (branch, costCenter) = (old.ReferenceBranchId, old.ReferenceCostCenterId);
        }

        var id = await setup.SaveSegmentValueAsync(new GlCoaSetup.SegmentValueInput(
            j.Str("coa"), j.Str("segment"), j.Str("code")?.ToUpperInvariant(), j.Str("nameEn"), j.Str("nameAr"), j.Str("description"),
            j.Str("parent"), j.Str("summary"), j.Str("posting"), j.Str("enabled"), j.Date("start"), j.Date("end"), j.Key("sort"),
            j.Str("le"), null, null, j.Str("bl")), ct);

        if (oldId is not null && (branch is not null || costCenter is not null))
        {
            var row = await db.Set<GlCoaSegmentValue>().SingleAsync(v => v.SegmentValueId == id, ct);
            row.ReferenceBranchId ??= branch;
            row.ReferenceCostCenterId ??= costCenter;
            await db.SaveChangesAsync(ct);
        }

        return new { ok = true, id };
    });
}

/// <summary>SEG_VALUES: the values of one segment, with parent code and references.</summary>
public sealed class P40SegValues(ICapitraDbContext db) : P40Process
{
    public override string Name => "SEG_VALUES";

    public override string Permission => Permissions.Gl.SetupView;

    public override async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var seg = long.Parse(request.X01 ?? string.Empty, System.Globalization.CultureInfo.InvariantCulture);
        var rows = await (from v in db.Set<GlCoaSegmentValue>().AsNoTracking()
                          join p in db.Set<GlCoaSegmentValue>() on v.ParentValueId equals p.SegmentValueId into ps
                          from p in ps.DefaultIfEmpty()
                          join le in db.Set<FinLegalEntity>() on v.ReferenceLegalEntityId equals le.LegalEntityId into les
                          from le in les.DefaultIfEmpty()
                          where v.CoaSegmentId == seg
                          orderby v.SortOrder, v.ValueCode
                          select new { v, parentCode = p.ValueCode, leCode = le.LegalEntityCode }).ToListAsync(ct);

        return new
        {
            ok = true,
            values = rows.Select(r => new
            {
                id = r.v.SegmentValueId, code = r.v.ValueCode, nameEn = r.v.ValueNameEn, nameAr = r.v.ValueNameAr,
                description = r.v.Description, parentId = r.v.ParentValueId, parentCode = r.parentCode,
                summary = Yn(r.v.IsSummary), posting = Yn(r.v.IsPostingAllowed), enabled = Yn(r.v.IsEnabled),
                start = Ymd(r.v.EffectiveStartDate), end = Ymd(r.v.EffectiveEndDate), sort = r.v.SortOrder,
                leCode = r.leCode, blCode = r.v.ReferenceBusinessLineCode,
            }),
        };
    }
}
