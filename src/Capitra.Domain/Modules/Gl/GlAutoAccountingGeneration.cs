using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_AUTO_ACCOUNTING_GENERATIONS: copy of FINOVA GL_AUTO_ACCOUNTING_GENERATIONS (APEX), all columns kept.
/// </summary>
public class GlAutoAccountingGeneration : AuditableEntity
{
    public long GenerationId { get; set; }

    public string GenerationNo { get; set; } = string.Empty;

    public long AccountingEventId { get; set; }

    public long RuleSetId { get; set; }

    public long LedgerId { get; set; }

    /// <summary>One of: GENERATED, VALIDATED, READY_FOR_REVIEW, APPROVED_FOR_POSTING, INTERFACED, POSTED, REVERSED, ERROR, CANCELLED.</summary>
    public string GenerationStatus { get; set; } = "GENERATED";

    public DateTime GeneratedAt { get; set; }

    public string GeneratedBy { get; set; } = string.Empty;

    public decimal TotalEnteredDr { get; set; }

    public decimal TotalEnteredCr { get; set; }

    public decimal TotalAccountedDr { get; set; }

    public decimal TotalAccountedCr { get; set; }

    public long? InterfaceBatchId { get; set; }

    public long? JournalHeaderId { get; set; }

    public string? ErrorMessage { get; set; }
}
