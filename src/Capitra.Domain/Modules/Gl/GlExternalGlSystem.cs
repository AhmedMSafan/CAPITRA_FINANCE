using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_EXTERNAL_GL_SYSTEMS: copy of FINOVA GL_EXTERNAL_GL_SYSTEMS (APEX), all columns kept.
/// </summary>
public class GlExternalGlSystem : AuditableEntity
{
    public long ExternalSystemId { get; set; }

    public string SystemCode { get; set; } = string.Empty;

    public string SystemNameEn { get; set; } = string.Empty;

    public string? SystemNameAr { get; set; }

    /// <summary>One of: EBS_R12, EBS_11I, FUSION_CLOUD, OTHER_ORACLE_GL.</summary>
    public string OracleProductType { get; set; } = string.Empty;

    /// <summary>One of: EBS_DB_LINK, EBS_FILE, FUSION_FBDI, FUSION_REST, MANUAL_FILE.</summary>
    public string ExportMethod { get; set; } = string.Empty;

    public string? EndpointCode { get; set; }

    public string? CredentialAlias { get; set; }

    /// <summary>One of: DETAIL, SUMMARY.</summary>
    public string? DefaultInterfaceMode { get; set; } = "SUMMARY";

    public bool IsDirectDbInsertAllowed { get; set; }

    public bool IsFileExportAllowed { get; set; } = true;

    public bool IsImportConfirmationRequired { get; set; } = true;

    public bool IsPostConfirmationRequired { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }
}
