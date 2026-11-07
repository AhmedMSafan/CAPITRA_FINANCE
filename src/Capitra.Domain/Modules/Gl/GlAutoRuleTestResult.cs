using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_AUTO_RULE_TEST_RESULTS: copy of FINOVA GL_AUTO_RULE_TEST_RESULTS (APEX), all columns kept.
/// </summary>
public class GlAutoRuleTestResult : AuditableEntity
{
    public long TestResultId { get; set; }

    public long TestCaseId { get; set; }

    public int RunNo { get; set; }

    public DateTime RunAt { get; set; }

    public string RunBy { get; set; } = string.Empty;

    /// <summary>One of: PASSED, FAILED, ERROR.</summary>
    public string ResultStatus { get; set; } = string.Empty;

    public decimal? GeneratedTotalDr { get; set; }

    public decimal? GeneratedTotalCr { get; set; }

    public decimal? DifferenceAmount { get; set; }

    public string? ResultMessage { get; set; }
}
