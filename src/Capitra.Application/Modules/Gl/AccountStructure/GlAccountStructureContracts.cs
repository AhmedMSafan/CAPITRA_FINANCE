namespace Capitra.Application.Modules.Gl.AccountStructure;

/// <summary>Everything the account structure page shows for one structure (APEX App 120 page 40, process SEG_LOAD).</summary>
public sealed record AccountStructurePageDto(
    long? CoaId,
    IReadOnlyList<CoaStructureDto> Structures,
    IReadOnlyList<CoaSegmentDto> Segments,
    IReadOnlyList<string> Ledgers,
    int Combinations,
    IReadOnlyList<CodeNameDto> LegalEntities,
    IReadOnlyList<CodeNameDto> BusinessLines,
    IReadOnlyList<string> SegmentTypes,
    IReadOnlyList<string> ValueFormats);

public sealed record CodeNameDto(string Code, string Name);

public sealed record CoaStructureDto(
    long CoaId,
    string CoaCode,
    string CoaNameEn,
    string? CoaNameAr,
    string? Description,
    string DelimiterChar,
    int SegmentCount,
    int AccountSegmentNum,
    int? BalancingSegmentNum,
    int? BranchSegmentNum,
    int? CostCenterSegmentNum,
    int? BusinessLineSegmentNum,
    int? IntercompanySegmentNum,
    bool IsDynamicCombinationAllowed,
    bool IsCombinationApprovalRequired,
    bool IsActive,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    int RowVersion);

public sealed record CoaSegmentDto(
    long CoaSegmentId,
    int SegmentNum,
    string SegmentCode,
    string SegmentNameEn,
    string? SegmentNameAr,
    string SegmentType,
    int MinLength,
    int MaxLength,
    string ValueFormat,
    string? PadChar,
    bool IsUppercase,
    bool IsRequired,
    bool IsSummaryValueAllowed,
    bool IsEnabled,
    int DisplaySeq,
    string? Description,
    string? DefaultValueCode,
    string DefaultMode,
    int ValueCount,
    int RowVersion);

public sealed record SegmentValueDto(
    long SegmentValueId,
    string ValueCode,
    string ValueNameEn,
    string? ValueNameAr,
    string? Description,
    long? ParentValueId,
    string? ParentValueCode,
    bool IsSummary,
    bool IsPostingAllowed,
    bool IsEnabled,
    DateOnly EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    int SortOrder,
    string? ReferenceLegalEntityCode,
    string? ReferenceBusinessLineCode,
    int RowVersion);

public sealed record SaveCoaStructureRequest(
    string? CoaCode,
    string? CoaNameEn,
    string? CoaNameAr,
    string? Description,
    string? DelimiterChar,
    int? SegmentCount,
    int? AccountSegmentNum,
    int? BalancingSegmentNum,
    int? BranchSegmentNum,
    int? CostCenterSegmentNum,
    int? BusinessLineSegmentNum,
    int? IntercompanySegmentNum,
    bool IsDynamicCombinationAllowed,
    bool IsCombinationApprovalRequired,
    bool IsActive,
    DateOnly? EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    int? RowVersion);

public sealed record SaveCoaSegmentRequest(
    int? SegmentNum,
    string? SegmentCode,
    string? SegmentNameEn,
    string? SegmentNameAr,
    string? SegmentType,
    int? MinLength,
    int? MaxLength,
    string? ValueFormat,
    string? PadChar,
    bool IsUppercase,
    bool IsRequired,
    bool IsSummaryValueAllowed,
    bool IsEnabled,
    int? DisplaySeq,
    string? Description,
    int? RowVersion);

/// <summary>Value filled in on every new journal line; a blank value removes the default.</summary>
public sealed record SaveSegmentDefaultRequest(string? ValueCode, string? DefaultMode);

public sealed record SaveSegmentValueRequest(
    string? ValueCode,
    string? ValueNameEn,
    string? ValueNameAr,
    string? Description,
    long? ParentValueId,
    bool IsSummary,
    bool IsPostingAllowed,
    bool IsEnabled,
    DateOnly? EffectiveStartDate,
    DateOnly? EffectiveEndDate,
    int? SortOrder,
    string? ReferenceLegalEntityCode,
    string? ReferenceBusinessLineCode,
    int? RowVersion);

public interface IGlAccountStructureService
{
    /// <summary>The chosen structure, or the first active one when none is given.</summary>
    Task<AccountStructurePageDto> GetAsync(long? coaId, CancellationToken cancellationToken);

    Task<CoaStructureDto> CreateStructureAsync(SaveCoaStructureRequest request, CancellationToken cancellationToken);

    Task<CoaStructureDto> UpdateStructureAsync(long coaId, SaveCoaStructureRequest request, CancellationToken cancellationToken);

    Task<CoaSegmentDto> CreateSegmentAsync(long coaId, SaveCoaSegmentRequest request, CancellationToken cancellationToken);

    Task<CoaSegmentDto> UpdateSegmentAsync(long coaSegmentId, SaveCoaSegmentRequest request, CancellationToken cancellationToken);

    Task SaveDefaultAsync(long coaSegmentId, SaveSegmentDefaultRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<SegmentValueDto>> GetValuesAsync(long coaSegmentId, CancellationToken cancellationToken);

    Task<SegmentValueDto> CreateValueAsync(long coaSegmentId, SaveSegmentValueRequest request, CancellationToken cancellationToken);

    Task<SegmentValueDto> UpdateValueAsync(long segmentValueId, SaveSegmentValueRequest request, CancellationToken cancellationToken);
}
