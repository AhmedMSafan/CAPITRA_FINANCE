using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_IMPORT_BATCHES: copy of FINOVA MFF_IMPORT_BATCHES (APEX), all columns kept.
/// </summary>
public class MffImportBatch : AuditableEntity
{
    public long BatchId { get; set; }

    public string SourceChannelCode { get; set; } = string.Empty;

    public string FileName { get; set; } = string.Empty;

    public DateTime ReceivedAt { get; set; }

    public long? ProgramVersionId { get; set; }

    public long? FinancierId { get; set; }

    public long? FundOrgId { get; set; }

    public int RowCount { get; set; }

    public int ValidCount { get; set; }

    public int ErrorCount { get; set; }

    public string BatchStatus { get; set; } = "NEW";

    public string? FileChecksum { get; set; }

    public string? Notes { get; set; }
}
