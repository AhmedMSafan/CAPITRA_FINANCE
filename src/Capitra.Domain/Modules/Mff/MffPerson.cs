using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_PERSONS: copy of FINOVA MFF_PERSONS (APEX), all columns kept.
/// </summary>
public class MffPerson : AuditableEntity
{
    public long PersonId { get; set; }

    public string? NationalId { get; set; }

    public string FullNameAr { get; set; } = string.Empty;

    public DateOnly? BirthDate { get; set; }

    public string? GenderCode { get; set; }

    public string? MaritalStatusCode { get; set; }

    public string? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? AddressAr { get; set; }

    public string? EmploymentTypeCode { get; set; }

    public string? EmployerName { get; set; }

    public DateOnly? HireDate { get; set; }

    public string? TaxNo { get; set; }

    public string? CommercialRegNo { get; set; }
}
