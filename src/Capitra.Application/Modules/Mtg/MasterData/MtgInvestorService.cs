using System.Linq.Expressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Domain.Modules.Mtg;

namespace Capitra.Application.Modules.Mtg.MasterData;

public sealed record InvestorDto(
    long InvestorId,
    string InvestorCode,
    string FullNameAr,
    string? FullNameEn,
    string? NationalIdNo,
    DateOnly? BirthDate,
    string? PhoneNo,
    string? EmailAddress,
    string? AddressText,
    string? EmployerName,
    decimal? DeclaredIncomeAmount,
    bool IsActive,
    int RowVersion);

public sealed record SaveInvestorRequest(
    string InvestorCode,
    string FullNameAr,
    string? FullNameEn,
    string? NationalIdNo,
    DateOnly? BirthDate,
    string? PhoneNo,
    string? EmailAddress,
    string? AddressText,
    string? EmployerName,
    decimal? DeclaredIncomeAmount,
    bool IsActive,
    int? RowVersion) : ISaveRequest;

public interface IMtgInvestorService : ICrudService<InvestorDto, SaveInvestorRequest>
{
    Task<IReadOnlyList<InvestorDto>> ListAsync(CancellationToken cancellationToken);
}

/// <summary>Investors, APEX App 105 pages 109 (list) and 110 (form).</summary>
public sealed class MtgInvestorService(ICapitraDbContext db, IClock clock)
    : CrudService<MtgInvestor, InvestorDto, SaveInvestorRequest>(db), IMtgInvestorService
{
    protected override string Title => "Investor";

    public Task<IReadOnlyList<InvestorDto>> ListAsync(CancellationToken cancellationToken) =>
        ListAsync(null, cancellationToken);

    protected override long IdOf(MtgInvestor entity) => entity.InvestorId;

    protected override Expression<Func<MtgInvestor, bool>> HasId(long id) => x => x.InvestorId == id;

    protected override IQueryable<InvestorDto> Project(IQueryable<MtgInvestor> rows) =>
        from i in rows
        orderby i.FullNameEn ?? i.FullNameAr
        select new InvestorDto(
            i.InvestorId, i.InvestorCode, i.FullNameAr, i.FullNameEn, i.NationalIdNo, i.BirthDate, i.PhoneNo,
            i.EmailAddress, i.AddressText, i.EmployerName, i.DeclaredIncomeAmount, i.IsActive, i.RowVersion);

    protected override async Task ValidateAsync(
        SaveInvestorRequest r, MtgInvestor? existing, ValidationErrors errors, CancellationToken cancellationToken)
    {
        var code = errors.Text(nameof(r.InvestorCode), "Investor Code", r.InvestorCode, 30, required: true);
        errors.Text(nameof(r.FullNameAr), "Full Name AR", r.FullNameAr, 300, required: true);
        errors.Text(nameof(r.FullNameEn), "Full Name EN", r.FullNameEn, 300);
        errors.Text(nameof(r.NationalIdNo), "National ID", r.NationalIdNo, 50);
        errors.Text(nameof(r.PhoneNo), "Phone No.", r.PhoneNo, 100);
        errors.Email(nameof(r.EmailAddress), "Email Address", r.EmailAddress, 320);
        errors.Text(nameof(r.AddressText), "Address", r.AddressText, 1000);
        errors.Text(nameof(r.EmployerName), "Employer Name", r.EmployerName, 300);
        errors.Range(nameof(r.DeclaredIncomeAmount), "Declared Income", r.DeclaredIncomeAmount, 0, 9_999_999_999_999_999m);

        if (r.BirthDate is { } birth && birth > DateOnly.FromDateTime(clock.UtcNow))
            errors.Add(nameof(r.BirthDate), "Date of Birth cannot be in the future.");

        if (code is not null && await IsTakenAsync(x => x.InvestorCode == code, existing, cancellationToken))
            errors.Add(nameof(r.InvestorCode), $"Investor code '{code}' is already used.");
    }

    protected override void Apply(MtgInvestor e, SaveInvestorRequest r)
    {
        e.InvestorCode = r.InvestorCode.Trim();
        e.FullNameAr = r.FullNameAr.Trim();
        e.FullNameEn = MasterDataRules.Clean(r.FullNameEn);
        e.NationalIdNo = MasterDataRules.Clean(r.NationalIdNo);
        e.BirthDate = r.BirthDate;
        e.PhoneNo = MasterDataRules.Clean(r.PhoneNo);
        e.EmailAddress = MasterDataRules.Clean(r.EmailAddress);
        e.AddressText = MasterDataRules.Clean(r.AddressText);
        e.EmployerName = MasterDataRules.Clean(r.EmployerName);
        e.DeclaredIncomeAmount = r.DeclaredIncomeAmount;
        e.IsActive = r.IsActive;
    }
}
