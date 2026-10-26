using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>GL_ACCOUNT_COMBINATION_SEGMENTS: the value of one segment in a combination, for search by segment.</summary>
public class GlAccountCombinationSegment : AuditableEntity
{
    public long CombinationSegmentId { get; set; }

    public long AccountCombinationId { get; set; }

    public long CoaSegmentId { get; set; }

    public int SegmentNum { get; set; }

    public long SegmentValueId { get; set; }

    public string SegmentValueCode { get; set; } = string.Empty;
}
