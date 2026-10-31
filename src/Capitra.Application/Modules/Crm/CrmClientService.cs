using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Crm;

/// <summary>
/// CRM_CLIENTS, as the CRM home lists them, with the client's latest transaction: the newest of its factoring applications
/// (FCT_OFFERS), agreements and disbursement requests by creation time (Ahmed, 31 Oct 2026); null when it has none.
/// </summary>
public sealed record CrmClientListDto(
    long ClientId, string ClientNumber, string ClientName, string ClientType, string? PhoneNumber, string? Email, bool IsActive,
    DateTime? LastActivityAt, string? LastActivityType, string? LastActivityRef);

/// <summary>The fields of FINOVA's client dialog (Basic Information, Contact Information, Classification).</summary>
public sealed record CrmClientFormDto(
    long? ClientId, string ClientNumber, string ClientName, string ClientNameAr, string? ShortName, string ClientType, string? ClientRoleCode,
    string LegalStructure, string? RegistrationNumber, string? TaxNumber, string? Email, string? PhoneNumber, string? MobileNumber,
    string? Website, long CountryId, long BaseCurrencyId, long StatusId, bool IsActive, bool IsSupplier, bool IsRelatedParty);

public sealed record CrmOptionDto(string Value, string Label);

/// <summary>The dialog's lists: countries and currencies (CRM_LKP_*) and the client roles.</summary>
public sealed record CrmClientOptionsDto(IReadOnlyList<CrmOptionDto> Countries, IReadOnlyList<CrmOptionDto> Currencies, IReadOnlyList<CrmOptionDto> ClientRoles);

/// <summary>
/// The CRM clients, from FINOVA_FINANCEING ClientsController / ClientService and its LOVs: list, read, create and update
/// CRM_CLIENTS. Only the dialog's fields are written; the other columns keep their values.
/// </summary>
public sealed class CrmClientService(ICapitraDbContext db)
{
    /// <summary>The client roles FINOVA's code knows (no roles table in the database): the developer and investor lists use them.</summary>
    public static readonly IReadOnlyList<CrmOptionDto> ClientRoles =
    [
        new("REAL_ESTATE_DEVELOPER", "Real Estate Developer"),
        new("INVESTOR", "Investor"),
    ];

    public async Task<IReadOnlyList<CrmClientListDto>> ListAsync(CancellationToken cancellationToken)
    {
        var clients = await db.Set<CrmClient>().AsNoTracking().OrderBy(x => x.ClientId).ToListAsync(cancellationToken);
        var latest = await LatestActivityAsync(cancellationToken);
        return [.. clients.Select(x =>
        {
            var a = latest.GetValueOrDefault(x.ClientId);
            return new CrmClientListDto(x.ClientId, x.ClientNumber, x.ClientName, x.ClientType, x.PhoneNumber, x.Email, x.IsActive,
                a?.At, a?.Type, a?.Ref);
        })];
    }

    private sealed record Activity(long ClientId, string Type, string Ref, DateTime At);

    /// <summary>Per client, its newest application, agreement or disbursement request.</summary>
    private async Task<Dictionary<long, Activity>> LatestActivityAsync(CancellationToken cancellationToken)
    {
        var offers = await db.Set<FctOffer>().AsNoTracking()
            .Select(x => new Activity(x.ClientId, "Application", x.OfferRef, x.CreatedAt)).ToListAsync(cancellationToken);
        var agreements = await db.Set<FctAgreement>().AsNoTracking()
            .Select(x => new Activity(x.ClientId, "Agreement", x.AgreementNumber, x.CreatedAt)).ToListAsync(cancellationToken);
        var disbursements = await db.Set<FctDisbursement>().AsNoTracking()
            .Select(x => new Activity(x.ClientId, "Disbursement", x.DisbursementNumber, x.CreatedAt)).ToListAsync(cancellationToken);
        return offers.Concat(agreements).Concat(disbursements)
            .GroupBy(x => x.ClientId)
            .ToDictionary(g => g.Key, g => g.MaxBy(x => x.At)!);
    }

    public async Task<CrmClientFormDto> GetAsync(long clientId, CancellationToken cancellationToken) =>
        await db.Set<CrmClient>().AsNoTracking()
            .Where(x => x.ClientId == clientId)
            .Select(x => new CrmClientFormDto(x.ClientId, x.ClientNumber, x.ClientName, x.ClientNameAr, x.ShortName, x.ClientType, x.ClientRoleCode,
                x.LegalStructure, x.RegistrationNumber, x.TaxNumber, x.Email, x.PhoneNumber, x.MobileNumber, x.Website,
                x.CountryId, x.BaseCurrencyId, x.StatusId, x.IsActive, x.IsSupplier, x.IsRelatedParty))
            .SingleOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException($"Client with ID {clientId} was not found.");

    // FINOVA LovService: countries by name, currencies by code
    public async Task<CrmClientOptionsDto> OptionsAsync(CancellationToken cancellationToken)
    {
        var countries = await db.Set<CrmLkpCountry>().AsNoTracking().OrderBy(x => x.CountryName)
            .Select(x => new CrmOptionDto(x.CountryId.ToString(), x.CountryName)).ToListAsync(cancellationToken);
        var currencies = await db.Set<CrmLkpCurrency>().AsNoTracking().OrderBy(x => x.CurrencyCode)
            .Select(x => new CrmOptionDto(x.CurrencyId.ToString(), x.CurrencyCode)).ToListAsync(cancellationToken);
        return new CrmClientOptionsDto(countries, currencies, ClientRoles);
    }

    public async Task<CrmClientFormDto> CreateAsync(CrmClientFormDto dto, CancellationToken cancellationToken)
    {
        var number = dto.ClientNumber?.Trim() ?? string.Empty;
        await ValidateAsync(dto, requireNumber: true, cancellationToken);
        if (await db.Set<CrmClient>().AnyAsync(x => x.ClientNumber == number, cancellationToken))
            throw BusinessValidationException.For("clientNumber", $"Client Number {number} already exists.");

        // FINOVA's defaults: active, not blacklisted, not a related party, not a supplier unless chosen
        var client = new CrmClient { ClientNumber = number };
        Apply(client, dto);
        db.Set<CrmClient>().Add(client);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(client.ClientId, cancellationToken);
    }

    public async Task<CrmClientFormDto> UpdateAsync(long clientId, CrmClientFormDto dto, CancellationToken cancellationToken)
    {
        var client = await db.Set<CrmClient>().SingleOrDefaultAsync(x => x.ClientId == clientId, cancellationToken)
            ?? throw new NotFoundException($"Client with ID {clientId} was not found.");
        await ValidateAsync(dto, requireNumber: false, cancellationToken);

        // the client number cannot be changed once the client exists (disabled in FINOVA's edit dialog)
        Apply(client, dto);
        await db.SaveChangesAsync(cancellationToken);
        return await GetAsync(clientId, cancellationToken);
    }

    private static void Apply(CrmClient client, CrmClientFormDto dto)
    {
        client.ClientName = dto.ClientName.Trim();
        client.ClientNameAr = dto.ClientNameAr.Trim();
        client.ShortName = Blank(dto.ShortName);
        client.ClientType = dto.ClientType.Trim();
        // FINOVA stores the role code in upper case
        client.ClientRoleCode = Blank(dto.ClientRoleCode)?.ToUpperInvariant();
        client.LegalStructure = dto.LegalStructure.Trim();
        client.RegistrationNumber = Blank(dto.RegistrationNumber);
        client.TaxNumber = Blank(dto.TaxNumber);
        client.Email = Blank(dto.Email);
        client.PhoneNumber = Blank(dto.PhoneNumber);
        client.MobileNumber = Blank(dto.MobileNumber);
        client.Website = Blank(dto.Website);
        client.CountryId = dto.CountryId;
        client.BaseCurrencyId = dto.BaseCurrencyId;
        client.StatusId = dto.StatusId;
        client.IsActive = dto.IsActive;
        client.IsSupplier = dto.IsSupplier;
        client.IsRelatedParty = dto.IsRelatedParty;
    }

    // FINOVA's required fields, plus the lists the columns reference (FINOVA left those to the database)
    private async Task ValidateAsync(CrmClientFormDto dto, bool requireNumber, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (requireNumber && string.IsNullOrWhiteSpace(dto.ClientNumber)) errors["clientNumber"] = ["Client Number is required."];
        if (string.IsNullOrWhiteSpace(dto.ClientName)) errors["clientName"] = ["Client Name is required."];
        if (string.IsNullOrWhiteSpace(dto.ClientNameAr)) errors["clientNameAr"] = ["Arabic Client Name is required."];
        if (string.IsNullOrWhiteSpace(dto.ClientType)) errors["clientType"] = ["Client Type is required."];
        if (string.IsNullOrWhiteSpace(dto.LegalStructure)) errors["legalStructure"] = ["Legal Structure is required."];
        if (!await db.Set<CrmLkpCountry>().AnyAsync(x => x.CountryId == dto.CountryId, cancellationToken)) errors["countryId"] = ["Country is required."];
        if (!await db.Set<CrmLkpCurrency>().AnyAsync(x => x.CurrencyId == dto.BaseCurrencyId, cancellationToken)) errors["baseCurrencyId"] = ["Base Currency is required."];
        if (!await db.Set<CrmLkpStatusCode>().AnyAsync(x => x.StatusId == dto.StatusId, cancellationToken)) errors["statusId"] = [$"Status ID {dto.StatusId} does not exist."];
        var role = Blank(dto.ClientRoleCode)?.ToUpperInvariant();
        if (role is not null && ClientRoles.All(x => x.Value != role)) errors["clientRoleCode"] = [$"Client Role {role} does not exist."];
        if (errors.Count > 0) throw new BusinessValidationException(errors);
    }

    private static string? Blank(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
