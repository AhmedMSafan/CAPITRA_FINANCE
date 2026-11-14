using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_PROGRAM_VERSIONS: copy of FINOVA MFF_PROGRAM_VERSIONS (APEX), all columns kept.
/// </summary>
public class MffProgramVersion : AuditableEntity
{
    public long ProgramVersionId { get; set; }

    public long? ProtocolId { get; set; }

    public string ProgramCode { get; set; } = string.Empty;

    public string VersionNo { get; set; } = string.Empty;

    public string ProgramNameAr { get; set; } = string.Empty;

    public DateOnly? EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public string CurrencyCode { get; set; } = "EGP";

    public bool IsOperational { get; set; }

    public string? SourceReference { get; set; }

    public string? Notes { get; set; }
}
