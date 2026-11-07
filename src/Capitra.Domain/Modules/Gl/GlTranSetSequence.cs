using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_TRAN_SET_SEQUENCES: the counter behind every GL transaction set ID, one row per module and year.
/// An ID reads CAP-{MODULE_CODE}-{YEAR_NO}-{LAST_NO padded to DIGITS}, e.g. CAP-CHW-2026-00000001.
/// </summary>
public class GlTranSetSequence : AuditableEntity
{
    public const string Prefix = "CAP";

    /// <summary>Three letters of the module (sub-app) that posts, e.g. MPA, CHW; they go into the ID.</summary>
    public string ModuleCode { get; set; } = string.Empty;

    public int YearNo { get; set; }

    /// <summary>The main app the module belongs to, e.g. MTG for MPA, FCT for CHW.</summary>
    public string AppCode { get; set; } = string.Empty;

    public string ModuleNameEn { get; set; } = string.Empty;

    public string? ModuleNameAr { get; set; }

    /// <summary>The last number given out this year; the next ID takes LAST_NO + 1.</summary>
    public long LastNo { get; set; }

    /// <summary>How many digits the number is padded to (at least 8).</summary>
    public int Digits { get; set; } = 8;

    public bool IsActive { get; set; } = true;
}
