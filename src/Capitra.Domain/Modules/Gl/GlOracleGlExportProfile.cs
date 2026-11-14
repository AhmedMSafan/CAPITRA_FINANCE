using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ORACLE_GL_EXPORT_PROFILES: copy of FINOVA GL_ORACLE_GL_EXPORT_PROFILES (APEX), all columns kept.
/// </summary>
public class GlOracleGlExportProfile : AuditableEntity
{
    public long ExportProfileId { get; set; }

    public long ExternalSystemId { get; set; }

    public long LedgerMapId { get; set; }

    public string ProfileCode { get; set; } = string.Empty;

    public string ProfileNameEn { get; set; } = string.Empty;

    public string? ProfileNameAr { get; set; }

    /// <summary>One of: APPROVED_JOURNALS, POSTED_JOURNALS, SOURCE_GENERATED, MANUAL_SELECTED.</summary>
    public string ExportScope { get; set; } = "APPROVED_JOURNALS";

    /// <summary>One of: DETAIL, SUMMARY_BY_ACCOUNT, SUMMARY_BY_SOURCE.</summary>
    public string ExportLevel { get; set; } = "DETAIL";

    public string? DefaultSourceCode { get; set; }

    public string? DefaultCategoryCode { get; set; }

    public bool IsAutoSubmitImport { get; set; }

    public bool IsAutoReconcile { get; set; } = true;

    public bool IsZeroDifferenceRequired { get; set; } = true;

    /// <summary>One of: JOURNAL_HEADER, SOURCE_DOCUMENT, EXPORT_LINE.</summary>
    public string DuplicateCheckLevel { get; set; } = "JOURNAL_HEADER";

    /// <summary>One of: EXPORT_REVERSING_JOURNAL, EXPORT_NEGATIVE_LINES, MANUAL.</summary>
    public string ReversalHandling { get; set; } = "EXPORT_REVERSING_JOURNAL";

    public bool IsActive { get; set; } = true;
}
