using System.Linq.Expressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Domain.Modules.Mtg;

namespace Capitra.Application.Modules.Mtg.MasterData;

public sealed record DeveloperSignatoryDto(
    long DeveloperSignatoryId,
    long DeveloperId,
    string DeveloperName,
    string SignatoryName,
    string? NationalIdNo,
    string? JobTitle,
    string? AuthorizationReference,
    DateOnly? ValidFromDate,
    DateOnly? ValidToDate,
    bool IsActive,
    int RowVersion);

public sealed record SaveDeveloperSignatoryRequest(
    long DeveloperId,
    string SignatoryName,
    string? NationalIdNo,
    string? JobTitle,
    string? AuthorizationReference,
    DateOnly? ValidFromDate,
    DateOnly? ValidToDate,
    bool IsActive,
    int? RowVersion) : ISaveRequest;

public interface IMtgDeveloperSignatoryService : ICrudService<DeveloperSignatoryDto, SaveDeveloperSignatoryRequest>
{
    Task<IReadOnlyList<DeveloperSignatoryDto>> ListAsync(long? developerId, CancellationToken cancellationToken);
}

/// <summary>Developer signatories, APEX App 105 pages 103 (list) and 104 (form).</summary>
public sealed class MtgDeveloperSignatoryService(ICapitraDbContext db)
    : CrudService<MtgDeveloperSignatory, DeveloperSignatoryDto, SaveDeveloperSignatoryRequest>(db), IMtgDeveloperSignatoryService
{
    protected override string Title => "Signatory";

    public Task<IReadOnlyList<DeveloperSignatoryDto>> ListAsync(long? developerId, CancellationToken cancellationToken) =>
        ListAsync(developerId is { } id ? x => x.DeveloperId == id : null, cancellationToken);

    protected override long IdOf(MtgDeveloperSignatory entity) => entity.DeveloperSignatoryId;

    protected override Expression<Func<MtgDeveloperSignatory, bool>> HasId(long id) => x => x.DeveloperSignatoryId == id;

    protected override IQueryable<DeveloperSignatoryDto> Project(IQueryable<MtgDeveloperSignatory> rows) =>
        from s in rows
        join d in Db.Set<MtgDeveloper>() on s.DeveloperId equals d.DeveloperId
        orderby d.LegalNameEn ?? d.LegalNameAr, s.SignatoryName
        select new DeveloperSignatoryDto(
            s.DeveloperSignatoryId, s.DeveloperId, d.LegalNameEn ?? d.LegalNameAr, s.SignatoryName, s.NationalIdNo,
            s.JobTitle, s.AuthorizationReference, s.ValidFromDate, s.ValidToDate, s.IsActive, s.RowVersion);

    protected override async Task ValidateAsync(
        SaveDeveloperSignatoryRequest r, MtgDeveloperSignatory? existing, ValidationErrors errors, CancellationToken cancellationToken)
    {
        errors.Text(nameof(r.SignatoryName), "Signatory Name", r.SignatoryName, 300, required: true);
        errors.Text(nameof(r.NationalIdNo), "National ID", r.NationalIdNo, 50);
        errors.Text(nameof(r.JobTitle), "Job Title", r.JobTitle, 200);
        errors.Text(nameof(r.AuthorizationReference), "Authorization Reference", r.AuthorizationReference, 300);

        if (r.ValidFromDate is { } from && r.ValidToDate is { } to && to < from)
            errors.Add(nameof(r.ValidToDate), "Valid To cannot be before Valid From.");

        if (!await IsSelectableAsync(
                Db.Set<MtgDeveloper>().Where(x => x.DeveloperId == r.DeveloperId && x.IsActive),
                existing?.DeveloperId == r.DeveloperId, cancellationToken))
            errors.Add(nameof(r.DeveloperId), "Select an active developer.");
    }

    protected override void Apply(MtgDeveloperSignatory e, SaveDeveloperSignatoryRequest r)
    {
        e.DeveloperId = r.DeveloperId;
        e.SignatoryName = r.SignatoryName.Trim();
        e.NationalIdNo = MasterDataRules.Clean(r.NationalIdNo);
        e.JobTitle = MasterDataRules.Clean(r.JobTitle);
        e.AuthorizationReference = MasterDataRules.Clean(r.AuthorizationReference);
        e.ValidFromDate = r.ValidFromDate;
        e.ValidToDate = r.ValidToDate;
        e.IsActive = r.IsActive;
    }
}
