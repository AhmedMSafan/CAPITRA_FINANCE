using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_VALIDATION_RULES: copy of FINOVA GL_CLOSE_VALIDATION_RULES (APEX), all columns kept.
/// </summary>
public class GlCloseValidationRule : AuditableEntity
{
    public long ValidationRuleId { get; set; }

    public long ValidationSetId { get; set; }

    public string RuleCode { get; set; } = string.Empty;

    public string RuleNameEn { get; set; } = string.Empty;

    public string? RuleNameAr { get; set; }

    public string RuleCategory { get; set; } = string.Empty;

    public string? SourceModule { get; set; }

    /// <summary>One of: SQL_CHECK, STATUS_CHECK, BALANCE_CHECK, SOURCE_CHECK, RECON_CHECK, MANUAL_CHECK, PACKAGE_CHECK.</summary>
    public string RuleType { get; set; } = string.Empty;

    /// <summary>One of: INFO, WARNING, ERROR, BLOCKER.</summary>
    public string Severity { get; set; } = "ERROR";

    public bool IsBlocking { get; set; } = true;

    public string? ValidationSql { get; set; }

    /// <summary>One of: ZERO_ROWS, ONE_ROW, COUNT_ZERO, COUNT_GREATER_ZERO, AMOUNT_ZERO, AMOUNT_WITHIN_TOLERANCE, STATUS_MATCH, MANUAL.</summary>
    public string ExpectedResultType { get; set; } = "ZERO_ROWS";

    public decimal? ThresholdAmount { get; set; }

    public decimal? ThresholdPercent { get; set; }

    public decimal? ToleranceAmount { get; set; }

    public decimal? TolerancePercent { get; set; }

    public int DisplaySeq { get; set; } = 10;

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
