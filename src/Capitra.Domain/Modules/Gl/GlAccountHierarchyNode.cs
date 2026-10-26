using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNT_HIERARCHY_NODES: copy of FINOVA GL_ACCOUNT_HIERARCHY_NODES (APEX), all columns kept.
/// </summary>
public class GlAccountHierarchyNode : AuditableEntity
{
    public long NodeId { get; set; }

    public long HierarchyId { get; set; }

    public long? ParentNodeId { get; set; }

    public string NodeCode { get; set; } = string.Empty;

    public string NodeNameEn { get; set; } = string.Empty;

    public string? NodeNameAr { get; set; }

    /// <summary>One of: GROUP, ACCOUNT, SEGMENT_VALUE, TOTAL.</summary>
    public string NodeType { get; set; } = string.Empty;

    public long? AccountId { get; set; }

    public long? SegmentValueId { get; set; }

    public long DisplaySeq { get; set; } = 10;

    public int SignMultiplier { get; set; } = 1;

    /// <summary>One of: ADD, SUBTRACT.</summary>
    public string RollupOperator { get; set; } = "ADD";

    public bool IsActive { get; set; } = true;
}
