using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_CLOSE_REPORTS: copy of FINOVA GL_CLOSE_REPORTS (APEX), all columns kept.
/// </summary>
public class GlCloseReport : AuditableEntity
{
    public long CloseReportId { get; set; }

    public long CloseRunId { get; set; }

    public string ReportCode { get; set; } = string.Empty;

    public string ReportNameEn { get; set; } = string.Empty;

    public string? ReportNameAr { get; set; }

    /// <summary>One of: CHECKLIST, VALIDATION, SOURCE_STATUS, ACCRUAL, RECURRING, MONTH_END, YEAR_END, BOARD_PACK, AUDIT.</summary>
    public string ReportType { get; set; } = string.Empty;

    /// <summary>One of: DRAFT, GENERATED, APPROVED, PUBLISHED, ARCHIVED, ERROR.</summary>
    public string ReportStatus { get; set; } = "DRAFT";

    public string? GeneratedFileName { get; set; }

    public string? GeneratedFileMimeType { get; set; }

    public byte[]? GeneratedFileBlob { get; set; }

    public string? GeneratedFileClob { get; set; }

    public string? GeneratedBy { get; set; }

    public DateTime? GeneratedAt { get; set; }

    public string? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? ReportParametersJson { get; set; }
}
