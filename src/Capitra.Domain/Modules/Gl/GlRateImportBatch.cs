using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_RATE_IMPORT_BATCHES: copy of FINOVA GL_RATE_IMPORT_BATCHES (APEX), all columns kept.
/// </summary>
public class GlRateImportBatch : AuditableEntity
{
    public long ImportBatchId { get; set; }

    public string BatchCode { get; set; } = string.Empty;

    public string BatchName { get; set; } = string.Empty;

    public long? RateTypeId { get; set; }

    /// <summary>One of: CENTRAL_BANK, BANK, IMPORT, API, SYSTEM.</summary>
    public string SourceCode { get; set; } = "IMPORT";

    public string? FileName { get; set; }

    /// <summary>One of: DRAFT, LOADED, VALIDATED, POSTED, ERROR, CANCELLED.</summary>
    public string ImportStatus { get; set; } = "DRAFT";

    public int TotalLineCount { get; set; }

    public int ValidLineCount { get; set; }

    public int ErrorLineCount { get; set; }

    public string? ImportedBy { get; set; }

    public DateTime? ImportedAt { get; set; }

    public string? PostedBy { get; set; }

    public DateTime? PostedAt { get; set; }

    public string? Notes { get; set; }
}
