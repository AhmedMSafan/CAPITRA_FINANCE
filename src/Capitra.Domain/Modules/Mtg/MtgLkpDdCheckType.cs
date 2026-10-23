using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mtg;

/// <summary>MTG_LKP_DD_CHECK_TYPE: due diligence check types (APEX App 105 pages 19/20).</summary>
public class MtgLkpDdCheckType : LookupEntity
{
    public bool IsMandatory { get; set; }
}
