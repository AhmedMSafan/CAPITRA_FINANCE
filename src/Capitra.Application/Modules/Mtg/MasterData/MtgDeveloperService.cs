using System.Linq.Expressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Mtg;

namespace Capitra.Application.Modules.Mtg.MasterData;

public sealed record DeveloperDto(
    long DeveloperId,
    string DeveloperCode,
    string LegalNameAr,
    string? LegalNameEn,
    string? CommercialRegNo,
    string? TaxRegNo,
    DateOnly? IncorporationDate,
    string? AddressText,
    string? PhoneNo,
    string? EmailAddress,
    string? RiskLevelCode,
    string? RiskLevelName,
    bool IsActive,
    int RowVersion);

public sealed record SaveDeveloperRequest(
    string DeveloperCode,
    string LegalNameAr,
    string? LegalNameEn,
    string? CommercialRegNo,
    string? TaxRegNo,
    DateOnly? IncorporationDate,
    string? AddressText,
    string? PhoneNo,
    string? EmailAddress,
    string? RiskLevelCode,
    bool IsActive,
    int? RowVersion) : ISaveRequest;

public interface IMtgDeveloperService : ICrudService<DeveloperDto, SaveDeveloperRequest>
{
    Task<IReadOnlyList<DeveloperDto>> ListAsync(bool activeOnly, CancellationToken cancellationToken);
}

/// <summary>Real estate developers, APEX App 105 pages 101 (list) and 102 (form).</summary>
public sealed class MtgDeveloperService(ICapitraDbContext db)
    : CrudService<MtgDeveloper, DeveloperDto, SaveDeveloperRequest>(db), IMtgDeveloperService
{
    protected override string Title => "Developer";

    public Task<IReadOnlyList<DeveloperDto>> ListAsync(bool activeOnly, CancellationToken cancellationToken) =>
        ListAsync(activeOnly ? x => x.IsActive : null, cancellationToken);

    protected override long IdOf(MtgDeveloper entity) => entity.DeveloperId;

    protected override Expression<Func<MtgDeveloper, bool>> HasId(long id) => x => x.DeveloperId == id;

    protected override IQueryable<DeveloperDto> Project(IQueryable<MtgDeveloper> rows) =>
        from d in rows
        join r in Db.Set<AdmLkpRiskLevel>() on d.RiskLevelCode equals r.Code into risk
        from r in risk.DefaultIfEmpty()
        orderby d.LegalNameEn ?? d.LegalNameAr
        select new DeveloperDto(
            d.DeveloperId, d.DeveloperCode, d.LegalNameAr, d.LegalNameEn, d.CommercialRegNo, d.TaxRegNo,
            d.IncorporationDate, d.AddressText, d.PhoneNo, d.EmailAddress, d.RiskLevelCode,
            r == null ? null : r.NameEn, d.IsActive, d.RowVersion);

    protected override async Task ValidateAsync(
        SaveDeveloperRequest r, MtgDeveloper? existing, ValidationErrors errors, CancellationToken cancellationToken)
    {
        var code = errors.Text(nameof(r.DeveloperCode), "Developer Code", r.DeveloperCode, 30, required: true);
        errors.Text(nameof(r.LegalNameAr), "Legal Name AR", r.LegalNameAr, 300, required: true);
        errors.Text(nameof(r.LegalNameEn), "Legal Name EN", r.LegalNameEn, 300);
        errors.Text(nameof(r.CommercialRegNo), "Commercial Reg. No.", r.CommercialRegNo, 100);
        errors.Text(nameof(r.TaxRegNo), "Tax Reg. No.", r.TaxRegNo, 100);
        errors.Text(nameof(r.AddressText), "Address", r.AddressText, 1000);
        errors.Text(nameof(r.PhoneNo), "Phone No.", r.PhoneNo, 100);
        errors.Email(nameof(r.EmailAddress), "Email Address", r.EmailAddress, 320);

        if (code is not null && await IsTakenAsync(x => x.DeveloperCode == code, existing, cancellationToken))
            errors.Add(nameof(r.DeveloperCode), $"Developer code '{code}' is already used.");

        var risk = MasterDataRules.Code(r.RiskLevelCode);
        if (risk is not null && !await IsSelectableAsync(
                Db.Set<AdmLkpRiskLevel>().Where(x => x.Code == risk && x.IsActive), existing?.RiskLevelCode == risk, cancellationToken))
            errors.Add(nameof(r.RiskLevelCode), $"Risk level '{risk}' is not an active risk level.");
    }

    protected override void Apply(MtgDeveloper e, SaveDeveloperRequest r)
    {
        e.DeveloperCode = r.DeveloperCode.Trim();
        e.LegalNameAr = r.LegalNameAr.Trim();
        e.LegalNameEn = MasterDataRules.Clean(r.LegalNameEn);
        e.CommercialRegNo = MasterDataRules.Clean(r.CommercialRegNo);
        e.TaxRegNo = MasterDataRules.Clean(r.TaxRegNo);
        e.IncorporationDate = r.IncorporationDate;
        e.AddressText = MasterDataRules.Clean(r.AddressText);
        e.PhoneNo = MasterDataRules.Clean(r.PhoneNo);
        e.EmailAddress = MasterDataRules.Clean(r.EmailAddress);
        e.RiskLevelCode = MasterDataRules.Code(r.RiskLevelCode);
        e.IsActive = r.IsActive;
    }
}
