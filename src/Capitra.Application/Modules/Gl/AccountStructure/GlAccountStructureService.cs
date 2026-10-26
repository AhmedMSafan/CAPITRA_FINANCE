using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.AccountStructure;

/// <summary>
/// Account structures, their segments and segment values (APEX App 120 page 40 and GL_COA_SETUP_PKG).
/// Codes never change once saved: structures, segments and values are found by them.
/// </summary>
public sealed class GlAccountStructureService(ICapitraDbContext db, IClock clock) : IGlAccountStructureService
{
    private const int MaxSegments = 10;
    private const int MaxValueLength = 30;

    public async Task<AccountStructurePageDto> GetAsync(long? coaId, CancellationToken cancellationToken)
    {
        var structures = await db.Set<GlCoaStructure>().AsNoTracking().OrderBy(x => x.CoaCode).ToListAsync(cancellationToken);

        var selected = structures.FirstOrDefault(x => x.CoaId == coaId)
            ?? structures.FirstOrDefault(x => x.IsActive)
            ?? structures.FirstOrDefault();

        var segments = selected is null
            ? []
            : await (
                from s in db.Set<GlCoaSegment>().AsNoTracking()
                where s.CoaId == selected.CoaId
                orderby s.SegmentNum
                select ToDto(s, db.Set<GlCoaSegmentValue>().Count(v => v.CoaSegmentId == s.CoaSegmentId))
            ).ToListAsync(cancellationToken);

        var legalEntities = await db.Set<AdmLkpLegalEntity>().AsNoTracking()
            .Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code)
            .Select(x => new CodeNameDto(x.Code, x.NameEn)).ToListAsync(cancellationToken);

        var businessLines = await db.Set<AdmLkpBusinessLine>().AsNoTracking()
            .Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code)
            .Select(x => new CodeNameDto(x.Code, x.NameEn)).ToListAsync(cancellationToken);

        // Ledgers and account combinations come with the journals step; until then a structure has none.
        return new AccountStructurePageDto(
            selected?.CoaId,
            structures.Select(ToDto).ToList(),
            segments,
            [],
            0,
            legalEntities,
            businessLines,
            GlCodes.SegmentTypes,
            GlCodes.ValueFormats);
    }

    public async Task<CoaStructureDto> CreateStructureAsync(SaveCoaStructureRequest request, CancellationToken cancellationToken)
    {
        var entity = new GlCoaStructure();
        await ApplyAsync(entity, request, isNew: true, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    public async Task<CoaStructureDto> UpdateStructureAsync(long coaId, SaveCoaStructureRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Set<GlCoaStructure>().FirstOrDefaultAsync(x => x.CoaId == coaId, cancellationToken)
            ?? throw new NotFoundException("Account structure not found.");

        if (request.RowVersion != entity.RowVersion)
            throw new ConflictException("This account structure was changed by someone else. Reload the page and try again.");

        await ApplyAsync(entity, request, isNew: false, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(entity);
    }

    private async Task ApplyAsync(GlCoaStructure entity, SaveCoaStructureRequest r, bool isNew, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();

        if (isNew)
        {
            var code = errors.Text("coaCode", "Structure code", GlRules.Code(r.CoaCode), 30, required: true);
            if (code is not null && await db.Set<GlCoaStructure>().AnyAsync(x => x.CoaCode == code, cancellationToken))
                errors.Add("coaCode", $"Account structure {code} already exists.");
            entity.CoaCode = code ?? string.Empty;
        }

        var nameEn = errors.Text("coaNameEn", "Name (English)", r.CoaNameEn, 240, required: true);
        var nameAr = errors.Text("coaNameAr", "Name (Arabic)", r.CoaNameAr, 240);
        var description = errors.Text("description", "Description", r.Description, 1000);
        var delimiter = errors.Text("delimiterChar", "Delimiter", r.DelimiterChar, 3) ?? "-";

        var count = r.SegmentCount;
        if (count is null || count < 1 || count > MaxSegments)
            errors.Add("segmentCount", $"The number of segments must be between 1 and {MaxSegments}.");

        if (r.AccountSegmentNum is null)
            errors.Add("accountSegmentNum", "The account segment is required.");

        if (count is { } n and >= 1 and <= MaxSegments)
        {
            CheckRole(errors, "accountSegmentNum", "Account", r.AccountSegmentNum, n);
            CheckRole(errors, "balancingSegmentNum", "Balancing", r.BalancingSegmentNum, n);
            CheckRole(errors, "branchSegmentNum", "Branch", r.BranchSegmentNum, n);
            CheckRole(errors, "costCenterSegmentNum", "Cost centre", r.CostCenterSegmentNum, n);
            CheckRole(errors, "businessLineSegmentNum", "Business line", r.BusinessLineSegmentNum, n);
            CheckRole(errors, "intercompanySegmentNum", "Intercompany", r.IntercompanySegmentNum, n);

            if (!isNew)
            {
                var highest = await db.Set<GlCoaSegment>().Where(x => x.CoaId == entity.CoaId)
                    .MaxAsync(x => (int?)x.SegmentNum, cancellationToken);
                if (highest > n)
                    errors.Add("segmentCount", $"Segment {highest} already exists, so the structure needs at least {highest} segments.");
            }
        }

        GlRules.CheckDates(errors, r.EffectiveStartDate, r.EffectiveEndDate);
        errors.ThrowIfAny();

        entity.CoaNameEn = nameEn!;
        entity.CoaNameAr = nameAr;
        entity.Description = description;
        entity.DelimiterChar = delimiter;
        entity.SegmentCount = count!.Value;
        entity.AccountSegmentNum = r.AccountSegmentNum!.Value;
        entity.BalancingSegmentNum = r.BalancingSegmentNum;
        entity.BranchSegmentNum = r.BranchSegmentNum;
        entity.CostCenterSegmentNum = r.CostCenterSegmentNum;
        entity.BusinessLineSegmentNum = r.BusinessLineSegmentNum;
        entity.IntercompanySegmentNum = r.IntercompanySegmentNum;
        entity.IsDynamicCombinationAllowed = r.IsDynamicCombinationAllowed;
        entity.IsCombinationApprovalRequired = r.IsCombinationApprovalRequired;
        entity.IsActive = r.IsActive;
        entity.EffectiveStartDate = r.EffectiveStartDate ?? GlRules.Today(clock);
        entity.EffectiveEndDate = r.EffectiveEndDate;
    }

    private static void CheckRole(ValidationErrors errors, string field, string label, int? value, int count)
    {
        if (value is { } v && (v < 1 || v > count))
            errors.Add(field, $"{label} segment must be between 1 and {count}.");
    }

    public async Task<CoaSegmentDto> CreateSegmentAsync(long coaId, SaveCoaSegmentRequest request, CancellationToken cancellationToken)
    {
        var structure = await db.Set<GlCoaStructure>().AsNoTracking().FirstOrDefaultAsync(x => x.CoaId == coaId, cancellationToken)
            ?? throw new NotFoundException("Account structure not found.");

        var entity = new GlCoaSegment { CoaId = coaId };
        await ApplyAsync(entity, structure, request, isNew: true, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return ToDto(entity, 0);
    }

    public async Task<CoaSegmentDto> UpdateSegmentAsync(long coaSegmentId, SaveCoaSegmentRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Set<GlCoaSegment>().FirstOrDefaultAsync(x => x.CoaSegmentId == coaSegmentId, cancellationToken)
            ?? throw new NotFoundException("Segment not found.");

        if (request.RowVersion != entity.RowVersion)
            throw new ConflictException("This segment was changed by someone else. Reload the page and try again.");

        var structure = await db.Set<GlCoaStructure>().AsNoTracking().FirstAsync(x => x.CoaId == entity.CoaId, cancellationToken);
        await ApplyAsync(entity, structure, request, isNew: false, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var values = await db.Set<GlCoaSegmentValue>().CountAsync(x => x.CoaSegmentId == coaSegmentId, cancellationToken);
        return ToDto(entity, values);
    }

    private async Task ApplyAsync(GlCoaSegment entity, GlCoaStructure structure, SaveCoaSegmentRequest r, bool isNew, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();

        // Number and code identify the segment and cannot be changed.
        if (isNew)
        {
            var code = errors.Text("segmentCode", "Segment code", GlRules.Code(r.SegmentCode), 30, required: true);

            if (r.SegmentNum is not { } num)
                errors.Add("segmentNum", "Segment number is required.");
            else if (num < 1 || num > structure.SegmentCount)
                errors.Add("segmentNum", $"Segment number must be between 1 and {structure.SegmentCount}, the number of segments of {structure.CoaCode}.");
            else if (await db.Set<GlCoaSegment>().AnyAsync(x => x.CoaId == structure.CoaId && x.SegmentNum == num, cancellationToken))
                errors.Add("segmentNum", $"Segment {num} already exists in {structure.CoaCode}.");

            if (code is not null && await db.Set<GlCoaSegment>().AnyAsync(x => x.CoaId == structure.CoaId && x.SegmentCode == code, cancellationToken))
                errors.Add("segmentCode", $"Segment {code} already exists in {structure.CoaCode}.");

            entity.SegmentNum = r.SegmentNum ?? 0;
            entity.SegmentCode = code ?? string.Empty;
        }

        var nameEn = errors.Text("segmentNameEn", "Name (English)", r.SegmentNameEn, 150, required: true);
        var nameAr = errors.Text("segmentNameAr", "Name (Arabic)", r.SegmentNameAr, 150);
        var description = errors.Text("description", "Description", r.Description, 1000);

        var type = GlRules.Code(r.SegmentType);
        if (type is null)
            errors.Add("segmentType", "Segment type is required.");
        else if (!GlCodes.SegmentTypes.Contains(type))
            errors.Add("segmentType", $"Unknown segment type {type}.");

        var format = GlRules.Code(r.ValueFormat) ?? GlCodes.FormatAlphanumeric;
        if (!GlCodes.ValueFormats.Contains(format))
            errors.Add("valueFormat", $"Unknown value format {format}.");

        var min = r.MinLength ?? 1;
        var max = r.MaxLength ?? MaxValueLength;
        if (min < 1 || max < min || max > MaxValueLength)
            errors.Add("maxLength", $"Lengths must satisfy 1 ≤ min length ≤ max length ≤ {MaxValueLength}.");

        var pad = string.IsNullOrEmpty(r.PadChar) ? null : r.PadChar;
        if (pad?.Length > 1)
            errors.Add("padChar", "Pad character is one character.");

        // An account segment never has a journal default: the account is chosen on each line.
        if (type == GlCodes.SegmentTypeAccount && entity.DefaultValueCode is not null)
            errors.Add("segmentType", "Remove the journal entry default before making this the account segment.");

        errors.ThrowIfAny();

        entity.SegmentNameEn = nameEn!;
        entity.SegmentNameAr = nameAr;
        entity.SegmentType = type!;
        entity.MinLength = min;
        entity.MaxLength = max;
        entity.ValueFormat = format;
        entity.PadChar = pad;
        entity.IsUppercase = r.IsUppercase;
        entity.IsRequired = r.IsRequired;
        entity.IsSummaryValueAllowed = r.IsSummaryValueAllowed;
        entity.IsEnabled = r.IsEnabled;
        entity.DisplaySeq = r.DisplaySeq ?? entity.SegmentNum * 10;
        entity.Description = description;
    }

    public async Task SaveDefaultAsync(long coaSegmentId, SaveSegmentDefaultRequest request, CancellationToken cancellationToken)
    {
        var segment = await db.Set<GlCoaSegment>().FirstOrDefaultAsync(x => x.CoaSegmentId == coaSegmentId, cancellationToken)
            ?? throw new NotFoundException("Segment not found.");

        var value = GlRules.Code(request.ValueCode);
        var mode = GlRules.Code(request.DefaultMode) ?? GlCodes.DefaultModeSuggest;

        if (segment.SegmentType == GlCodes.SegmentTypeAccount && value is not null)
            throw BusinessValidationException.For("valueCode", "The account segment has no default: the account is chosen on each journal line.");

        if (mode is not (GlCodes.DefaultModeSuggest or GlCodes.DefaultModeFixed))
            throw BusinessValidationException.For("defaultMode", $"Unknown default mode {mode}.");

        if (value is not null && !await db.Set<GlCoaSegmentValue>().AnyAsync(
                x => x.CoaSegmentId == coaSegmentId && x.ValueCode == value && x.IsEnabled && x.IsPostingAllowed && !x.IsSummary,
                cancellationToken))
            throw BusinessValidationException.For("valueCode", $"Value {value} is not an enabled posting value of this segment.");

        segment.DefaultValueCode = value;
        segment.DefaultMode = value is null ? GlCodes.DefaultModeSuggest : mode;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SegmentValueDto>> GetValuesAsync(long coaSegmentId, CancellationToken cancellationToken)
    {
        if (!await db.Set<GlCoaSegment>().AnyAsync(x => x.CoaSegmentId == coaSegmentId, cancellationToken))
            throw new NotFoundException("Segment not found.");

        return await (
            from v in db.Set<GlCoaSegmentValue>().AsNoTracking()
            where v.CoaSegmentId == coaSegmentId
            join p in db.Set<GlCoaSegmentValue>() on v.ParentValueId equals p.SegmentValueId into parents
            from p in parents.DefaultIfEmpty()
            orderby v.SortOrder, v.ValueCode
            select new SegmentValueDto(
                v.SegmentValueId, v.ValueCode, v.ValueNameEn, v.ValueNameAr, v.Description, v.ParentValueId, p.ValueCode,
                v.IsSummary, v.IsPostingAllowed, v.IsEnabled, v.EffectiveStartDate, v.EffectiveEndDate, v.SortOrder,
                v.ReferenceLegalEntityCode, v.ReferenceBusinessLineCode, v.RowVersion)
        ).ToListAsync(cancellationToken);
    }

    public async Task<SegmentValueDto> CreateValueAsync(long coaSegmentId, SaveSegmentValueRequest request, CancellationToken cancellationToken)
    {
        var segment = await db.Set<GlCoaSegment>().AsNoTracking().FirstOrDefaultAsync(x => x.CoaSegmentId == coaSegmentId, cancellationToken)
            ?? throw new NotFoundException("Segment not found.");

        var entity = new GlCoaSegmentValue { CoaSegmentId = coaSegmentId };
        await ApplyAsync(entity, segment, request, isNew: true, cancellationToken);
        db.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        return await ValueDtoAsync(entity, cancellationToken);
    }

    public async Task<SegmentValueDto> UpdateValueAsync(long segmentValueId, SaveSegmentValueRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Set<GlCoaSegmentValue>().FirstOrDefaultAsync(x => x.SegmentValueId == segmentValueId, cancellationToken)
            ?? throw new NotFoundException("Segment value not found.");

        if (request.RowVersion != entity.RowVersion)
            throw new ConflictException("This value was changed by someone else. Reload the page and try again.");

        var segment = await db.Set<GlCoaSegment>().AsNoTracking().FirstAsync(x => x.CoaSegmentId == entity.CoaSegmentId, cancellationToken);
        await ApplyAsync(entity, segment, request, isNew: false, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return await ValueDtoAsync(entity, cancellationToken);
    }

    private async Task ApplyAsync(GlCoaSegmentValue entity, GlCoaSegment segment, SaveSegmentValueRequest r, bool isNew, CancellationToken cancellationToken)
    {
        var errors = new ValidationErrors();

        // The code identifies the value and cannot be changed.
        if (isNew)
        {
            var code = errors.Text("valueCode", "Value code", GlRules.Code(r.ValueCode), MaxValueLength, required: true);
            if (code is not null)
            {
                GlRules.CheckValueCode(errors, "valueCode", code, segment);
                if (await db.Set<GlCoaSegmentValue>().AnyAsync(x => x.CoaSegmentId == segment.CoaSegmentId && x.ValueCode == code, cancellationToken))
                    errors.Add("valueCode", $"Value {code} already exists in segment {segment.SegmentCode}.");
            }
            entity.ValueCode = code ?? string.Empty;
        }

        var nameEn = errors.Text("valueNameEn", "Name (English)", r.ValueNameEn, 240, required: true);
        var nameAr = errors.Text("valueNameAr", "Name (Arabic)", r.ValueNameAr, 240);
        var description = errors.Text("description", "Description", r.Description, 1000);

        if (r.IsSummary && r.IsPostingAllowed)
            errors.Add("isPostingAllowed", "A summary value cannot allow posting.");

        if (r.EffectiveStartDate is null)
            errors.Add("effectiveStartDate", "Effective from is required.");
        GlRules.CheckDates(errors, r.EffectiveStartDate, r.EffectiveEndDate);

        if (r.ParentValueId is { } parentId)
        {
            var parentSegment = await db.Set<GlCoaSegmentValue>().Where(x => x.SegmentValueId == parentId)
                .Select(x => (long?)x.CoaSegmentId).FirstOrDefaultAsync(cancellationToken);
            if (parentSegment != segment.CoaSegmentId)
                errors.Add("parentValueId", "The parent value must belong to the same segment.");
            else if (!isNew && await IsSelfOrBelowAsync(entity.SegmentValueId, parentId, cancellationToken))
                errors.Add("parentValueId", "The selected parent is this value or one below it. This would create a circular hierarchy.");
        }

        var le = GlRules.Code(r.ReferenceLegalEntityCode);
        if (le is not null && !await db.Set<AdmLkpLegalEntity>().AnyAsync(x => x.Code == le, cancellationToken))
            errors.Add("referenceLegalEntityCode", $"Legal entity {le} does not exist.");

        var bl = GlRules.Code(r.ReferenceBusinessLineCode);
        if (bl is not null && !await db.Set<AdmLkpBusinessLine>().AnyAsync(x => x.Code == bl, cancellationToken))
            errors.Add("referenceBusinessLineCode", $"Business line {bl} does not exist.");

        errors.ThrowIfAny();

        entity.ValueNameEn = nameEn!;
        entity.ValueNameAr = nameAr;
        entity.Description = description;
        entity.ParentValueId = r.ParentValueId;
        entity.IsSummary = r.IsSummary;
        entity.IsPostingAllowed = r.IsPostingAllowed;
        entity.IsEnabled = r.IsEnabled;
        entity.EffectiveStartDate = r.EffectiveStartDate!.Value;
        entity.EffectiveEndDate = r.EffectiveEndDate;
        entity.SortOrder = r.SortOrder ?? 10;
        entity.ReferenceLegalEntityCode = le;
        entity.ReferenceBusinessLineCode = bl;
    }

    /// <summary>True when <paramref name="candidateId"/> is the value itself or sits below it in the hierarchy.</summary>
    private async Task<bool> IsSelfOrBelowAsync(long valueId, long candidateId, CancellationToken cancellationToken)
    {
        long? current = candidateId;
        for (var guard = 0; current is { } id && guard < 100; guard++)
        {
            if (id == valueId)
                return true;
            current = await db.Set<GlCoaSegmentValue>().Where(x => x.SegmentValueId == id).Select(x => x.ParentValueId).FirstOrDefaultAsync(cancellationToken);
        }
        return false;
    }

    private async Task<SegmentValueDto> ValueDtoAsync(GlCoaSegmentValue v, CancellationToken cancellationToken)
    {
        var parentCode = v.ParentValueId is { } parentId
            ? await db.Set<GlCoaSegmentValue>().Where(x => x.SegmentValueId == parentId).Select(x => x.ValueCode).FirstOrDefaultAsync(cancellationToken)
            : null;

        return new SegmentValueDto(
            v.SegmentValueId, v.ValueCode, v.ValueNameEn, v.ValueNameAr, v.Description, v.ParentValueId, parentCode,
            v.IsSummary, v.IsPostingAllowed, v.IsEnabled, v.EffectiveStartDate, v.EffectiveEndDate, v.SortOrder,
            v.ReferenceLegalEntityCode, v.ReferenceBusinessLineCode, v.RowVersion);
    }

    private static CoaStructureDto ToDto(GlCoaStructure x) => new(
        x.CoaId, x.CoaCode, x.CoaNameEn, x.CoaNameAr, x.Description, x.DelimiterChar, x.SegmentCount,
        x.AccountSegmentNum, x.BalancingSegmentNum, x.BranchSegmentNum, x.CostCenterSegmentNum, x.BusinessLineSegmentNum,
        x.IntercompanySegmentNum, x.IsDynamicCombinationAllowed, x.IsCombinationApprovalRequired, x.IsActive,
        x.EffectiveStartDate, x.EffectiveEndDate, x.RowVersion);

    private static CoaSegmentDto ToDto(GlCoaSegment x, int valueCount) => new(
        x.CoaSegmentId, x.SegmentNum, x.SegmentCode, x.SegmentNameEn, x.SegmentNameAr, x.SegmentType, x.MinLength, x.MaxLength,
        x.ValueFormat, x.PadChar, x.IsUppercase, x.IsRequired, x.IsSummaryValueAllowed, x.IsEnabled, x.DisplaySeq, x.Description,
        x.DefaultValueCode, x.DefaultMode, valueCount, x.RowVersion);
}
