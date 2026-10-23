using System.Linq.Expressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Domain.Modules.Mtg;

namespace Capitra.Application.Modules.Mtg.MasterData;

public sealed record ProjectDto(
    long ProjectId,
    long DeveloperId,
    string DeveloperName,
    string ProjectCode,
    string ProjectNameAr,
    string? ProjectNameEn,
    string? LocationText,
    decimal? LandArea,
    string? TitleDeedNo,
    string? BuildingLicenseNo,
    DateOnly? BuildingLicenseDate,
    decimal? ProjectCompletionPct,
    string? ProjectStatus,
    bool IsActive,
    int RowVersion);

public sealed record SaveProjectRequest(
    long DeveloperId,
    string ProjectCode,
    string ProjectNameAr,
    string? ProjectNameEn,
    string? LocationText,
    decimal? LandArea,
    string? TitleDeedNo,
    string? BuildingLicenseNo,
    DateOnly? BuildingLicenseDate,
    decimal? ProjectCompletionPct,
    string? ProjectStatus,
    bool IsActive,
    int? RowVersion) : ISaveRequest;

public interface IMtgProjectService : ICrudService<ProjectDto, SaveProjectRequest>
{
    Task<IReadOnlyList<ProjectDto>> ListAsync(long? developerId, bool activeOnly, CancellationToken cancellationToken);
}

/// <summary>Projects, APEX App 105 pages 105 (list) and 106 (form).</summary>
public sealed class MtgProjectService(ICapitraDbContext db)
    : CrudService<MtgProject, ProjectDto, SaveProjectRequest>(db), IMtgProjectService
{
    protected override string Title => "Project";

    public Task<IReadOnlyList<ProjectDto>> ListAsync(long? developerId, bool activeOnly, CancellationToken cancellationToken) =>
        ListAsync(x => (developerId == null || x.DeveloperId == developerId) && (!activeOnly || x.IsActive), cancellationToken);

    protected override long IdOf(MtgProject entity) => entity.ProjectId;

    protected override Expression<Func<MtgProject, bool>> HasId(long id) => x => x.ProjectId == id;

    protected override IQueryable<ProjectDto> Project(IQueryable<MtgProject> rows) =>
        from p in rows
        join d in Db.Set<MtgDeveloper>() on p.DeveloperId equals d.DeveloperId
        orderby p.ProjectNameEn ?? p.ProjectNameAr
        select new ProjectDto(
            p.ProjectId, p.DeveloperId, d.LegalNameEn ?? d.LegalNameAr, p.ProjectCode, p.ProjectNameAr, p.ProjectNameEn,
            p.LocationText, p.LandArea, p.TitleDeedNo, p.BuildingLicenseNo, p.BuildingLicenseDate,
            p.ProjectCompletionPct, p.ProjectStatus, p.IsActive, p.RowVersion);

    protected override async Task ValidateAsync(
        SaveProjectRequest r, MtgProject? existing, ValidationErrors errors, CancellationToken cancellationToken)
    {
        var code = errors.Text(nameof(r.ProjectCode), "Project Code", r.ProjectCode, 30, required: true);
        errors.Text(nameof(r.ProjectNameAr), "Project Name AR", r.ProjectNameAr, 300, required: true);
        errors.Text(nameof(r.ProjectNameEn), "Project Name EN", r.ProjectNameEn, 300);
        errors.Text(nameof(r.LocationText), "Location", r.LocationText, 1000);
        errors.Text(nameof(r.TitleDeedNo), "Title Deed No.", r.TitleDeedNo, 100);
        errors.Text(nameof(r.BuildingLicenseNo), "Building License No.", r.BuildingLicenseNo, 100);
        errors.Text(nameof(r.ProjectStatus), "Project Status", r.ProjectStatus, 50);
        errors.Range(nameof(r.LandArea), "Land Area", r.LandArea, 0, 9_999_999_999_999_999m);
        errors.Range(nameof(r.ProjectCompletionPct), "Completion %", r.ProjectCompletionPct, 0, 100);

        if (code is not null && await IsTakenAsync(x => x.ProjectCode == code, existing, cancellationToken))
            errors.Add(nameof(r.ProjectCode), $"Project code '{code}' is already used.");

        if (!await IsSelectableAsync(
                Db.Set<MtgDeveloper>().Where(x => x.DeveloperId == r.DeveloperId && x.IsActive),
                existing?.DeveloperId == r.DeveloperId, cancellationToken))
            errors.Add(nameof(r.DeveloperId), "Select an active developer.");
    }

    protected override void Apply(MtgProject e, SaveProjectRequest r)
    {
        e.DeveloperId = r.DeveloperId;
        e.ProjectCode = r.ProjectCode.Trim();
        e.ProjectNameAr = r.ProjectNameAr.Trim();
        e.ProjectNameEn = MasterDataRules.Clean(r.ProjectNameEn);
        e.LocationText = MasterDataRules.Clean(r.LocationText);
        e.LandArea = r.LandArea;
        e.TitleDeedNo = MasterDataRules.Clean(r.TitleDeedNo);
        e.BuildingLicenseNo = MasterDataRules.Clean(r.BuildingLicenseNo);
        e.BuildingLicenseDate = r.BuildingLicenseDate;
        e.ProjectCompletionPct = r.ProjectCompletionPct;
        e.ProjectStatus = MasterDataRules.Clean(r.ProjectStatus);
        e.IsActive = r.IsActive;
    }
}
