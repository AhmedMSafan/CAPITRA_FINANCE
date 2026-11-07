using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_YEAR_END_CLOSE_LINES: copy of FINOVA GL_YEAR_END_CLOSE_LINES (APEX), all columns kept.
/// </summary>
public class GlYearEndCloseLine : AuditableEntity
{
    public long YearEndCloseLineId { get; set; }

    public long YearEndRunId { get; set; }

    public long? AccountGroupId { get; set; }

    public long AccountCombinationId { get; set; }

    public string? AccountType { get; set; }

    /// <summary>One of: CARRY_FORWARD, CLOSE_TO_RETAINED_EARNINGS, ZERO_BALANCE, VALIDATE_ZERO_ONLY.</summary>
    public string ClosingBehavior { get; set; } = string.Empty;

    public decimal BeginningBalanceDr { get; set; }

    public decimal BeginningBalanceCr { get; set; }

    public decimal PeriodDebit { get; set; }

    public decimal PeriodCredit { get; set; }

    public decimal EndingBalanceDr { get; set; }

    public decimal EndingBalanceCr { get; set; }

    public bool IsCloseToRetained { get; set; }

    public bool IsCarryForward { get; set; }

    public long? ClosingJournalLineId { get; set; }

    public long? OpeningJournalLineId { get; set; }

    /// <summary>One of: CALCULATED, VALIDATED, GENERATED, POSTED, ERROR, CANCELLED.</summary>
    public string LineStatus { get; set; } = "CALCULATED";

    public string? ErrorMessage { get; set; }
}
