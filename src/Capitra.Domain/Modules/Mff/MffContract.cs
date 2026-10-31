using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Mff;

/// <summary>
/// MFF_CONTRACTS: copy of FINOVA MFF_CONTRACTS (APEX), all columns kept.
/// </summary>
public class MffContract : AuditableEntity
{
    public long ContractId { get; set; }

    public long ApplicationId { get; set; }

    public string ContractNo { get; set; } = string.Empty;

    public string ContractType { get; set; } = "TRIPARTITE";

    public DateOnly? ContractDate { get; set; }

    public long BuyerPersonId { get; set; }

    public long? SellerPersonId { get; set; }

    public long FinancierId { get; set; }

    public long PropertyId { get; set; }

    public decimal? FinanceAmount { get; set; }

    public decimal? PropertyPrice { get; set; }

    public bool IsSigned { get; set; }

    public long? DocumentId { get; set; }

    public string ContractStatus { get; set; } = "DRAFT";
}
