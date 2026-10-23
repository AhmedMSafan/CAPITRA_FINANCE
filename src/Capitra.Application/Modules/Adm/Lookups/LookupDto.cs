namespace Capitra.Application.Modules.Adm.Lookups;

public sealed record LookupDto(
    string Code,
    string NameAr,
    string NameEn,
    string? Description,
    int DisplayOrder,
    bool IsActive,
    int RowVersion);
