using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_AUTO_RULE_TEST_CASES: copy of FINOVA GL_AUTO_RULE_TEST_CASES (APEX), all columns kept.
/// </summary>
public class GlAutoRuleTestCase : AuditableEntity
{
    public long TestCaseId { get; set; }

    public long RuleSetId { get; set; }

    public string TestCaseCode { get; set; } = string.Empty;

    public string TestCaseNameEn { get; set; } = string.Empty;

    public string? SampleEventPayload { get; set; }

    public decimal? ExpectedTotalDr { get; set; }

    public decimal? ExpectedTotalCr { get; set; }

    /// <summary>One of: DRAFT, READY, PASSED, FAILED, DISABLED.</summary>
    public string TestStatus { get; set; } = "DRAFT";

    public DateTime? LastRunAt { get; set; }

    public string? LastRunBy { get; set; }

    public string? LastResultStatus { get; set; }

    public string? Description { get; set; }
}
