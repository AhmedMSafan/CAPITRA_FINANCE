using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>ADM_LKP_GUARANTEE_TYPES: guarantee types (APEX App 105 pages 27/28).</summary>
public class AdmLkpGuaranteeType : LookupEntity
{
    public bool IsMandatory { get; set; }
}
