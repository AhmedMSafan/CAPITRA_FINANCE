namespace Capitra.Application.Modules.Adm.Lookups;

/// <summary>Create or update a lookup value. <see cref="RowVersion"/> is required on update.</summary>
public sealed record SaveLookupRequest(
    string Code,
    string NameAr,
    string NameEn,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    int? RowVersion);
