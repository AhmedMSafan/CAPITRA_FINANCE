using System.Linq.Expressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Domain.Modules.Mtg;

namespace Capitra.Application.Modules.Mtg.MasterData;

public sealed record ProjectUnitDto(
    long ProjectUnitId,
    long ProjectId,
    string ProjectName,
    string UnitCode,
    string? UnitTypeCode,
    string? UnitTypeName,
    string? BuildingNo,
    string? FloorNo,
    string? UnitNo,
    decimal? AreaSqm,
    decimal? ContractAmount,
    decimal? MarketValueAmount,
    string? LicenseStatus,
    bool IsActive,
    int RowVersion);

public sealed record SaveProjectUnitRequest(
    long ProjectId,
    string UnitCode,
    string? UnitTypeCode,
    string? BuildingNo,
    string? FloorNo,
    string? UnitNo,
    decimal? AreaSqm,
    decimal? ContractAmount,
    decimal? MarketValueAmount,
    string? LicenseStatus,
    bool IsActive,
    int? RowVersion) : ISaveRequest;

public interface IMtgProjectUnitService : ICrudService<ProjectUnitDto, SaveProjectUnitRequest>
{
    Task<IReadOnlyList<ProjectUnitDto>> ListAsync(long? projectId, CancellationToken cancellationToken);
}

/// <summary>Project units, APEX App 105 pages 107 (list) and 108 (form).</summary>
public sealed class MtgProjectUnitService(ICapitraDbContext db)
    : CrudService<MtgProjectUnit, ProjectUnitDto, SaveProjectUnitRequest>(db), IMtgProjectUnitService
{
    private const decimal MaxAmount = 9_999_999_999_999_999m;

    protected override string Title => "Unit";

    public Task<IReadOnlyList<ProjectUnitDto>> ListAsync(long? projectId, CancellationToken cancellationToken) =>
        ListAsync(projectId is { } id ? x => x.ProjectId == id : null, cancellationToken);

    protected override long IdOf(MtgProjectUnit entity) => entity.ProjectUnitId;

    protected override Expression<Func<MtgProjectUnit, bool>> HasId(long id) => x => x.ProjectUnitId == id;

    protected override IQueryable<ProjectUnitDto> Project(IQueryable<MtgProjectUnit> rows) =>
        from u in rows
        join p in Db.Set<MtgProject>() on u.ProjectId equals p.ProjectId
        join t in Db.Set<MtgLkpUnitType>() on u.UnitTypeCode equals t.Code into types
        from t in types.DefaultIfEmpty()
        orderby p.ProjectNameEn ?? p.ProjectNameAr, u.UnitCode
        select new ProjectUnitDto(
            u.ProjectUnitId, u.ProjectId, p.ProjectNameEn ?? p.ProjectNameAr, u.UnitCode, u.UnitTypeCode,
            t == null ? null : t.NameEn, u.BuildingNo, u.FloorNo, u.UnitNo, u.AreaSqm, u.ContractAmount,
            u.MarketValueAmount, u.LicenseStatus, u.IsActive, u.RowVersion);

    protected override async Task ValidateAsync(
        SaveProjectUnitRequest r, MtgProjectUnit? existing, ValidationErrors errors, CancellationToken cancellationToken)
    {
        var code = errors.Text(nameof(r.UnitCode), "Unit Code", r.UnitCode, 50, required: true);
        errors.Text(nameof(r.BuildingNo), "Building No.", r.BuildingNo, 50);
        errors.Text(nameof(r.FloorNo), "Floor No.", r.FloorNo, 50);
        errors.Text(nameof(r.UnitNo), "Unit No.", r.UnitNo, 50);
        errors.Text(nameof(r.LicenseStatus), "License Status", r.LicenseStatus, 50);
        errors.Range(nameof(r.AreaSqm), "Area (sqm)", r.AreaSqm, 0, MaxAmount);
        errors.Range(nameof(r.ContractAmount), "Contract Value", r.ContractAmount, 0, MaxAmount);
        errors.Range(nameof(r.MarketValueAmount), "Current Market Value", r.MarketValueAmount, 0, MaxAmount);

        if (code is not null && await IsTakenAsync(x => x.ProjectId == r.ProjectId && x.UnitCode == code, existing, cancellationToken))
            errors.Add(nameof(r.UnitCode), $"Unit code '{code}' is already used in this project.");

        if (!await IsSelectableAsync(
                Db.Set<MtgProject>().Where(x => x.ProjectId == r.ProjectId && x.IsActive),
                existing?.ProjectId == r.ProjectId, cancellationToken))
            errors.Add(nameof(r.ProjectId), "Select an active project.");

        var type = MasterDataRules.Code(r.UnitTypeCode);
        if (type is not null && !await IsSelectableAsync(
                Db.Set<MtgLkpUnitType>().Where(x => x.Code == type && x.IsActive), existing?.UnitTypeCode == type, cancellationToken))
            errors.Add(nameof(r.UnitTypeCode), $"Unit type '{type}' is not an active unit type.");
    }

    protected override void Apply(MtgProjectUnit e, SaveProjectUnitRequest r)
    {
        e.ProjectId = r.ProjectId;
        e.UnitCode = r.UnitCode.Trim();
        e.UnitTypeCode = MasterDataRules.Code(r.UnitTypeCode);
        e.BuildingNo = MasterDataRules.Clean(r.BuildingNo);
        e.FloorNo = MasterDataRules.Clean(r.FloorNo);
        e.UnitNo = MasterDataRules.Clean(r.UnitNo);
        e.AreaSqm = r.AreaSqm;
        e.ContractAmount = r.ContractAmount;
        e.MarketValueAmount = r.MarketValueAmount;
        e.LicenseStatus = MasterDataRules.Clean(r.LicenseStatus);
        e.IsActive = r.IsActive;
    }
}
