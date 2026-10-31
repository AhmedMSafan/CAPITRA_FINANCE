namespace Capitra.Domain.Common;

/// <summary>
/// Shape shared by every lookup table (naming standard, section 5).
/// Business tables reference a lookup by its <see cref="Code"/>.
/// </summary>
public abstract class LookupEntity : AuditableEntity
{
    public const int CodeMaxLength = 30;

    public string Code { get; set; } = string.Empty;

    public string NameAr { get; set; } = string.Empty;

    public string NameEn { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
