using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Gl;

/// <summary>
/// GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES: copy of FINOVA GL_ACCOUNTING_ACCOUNT_LOOKUP_LINES (APEX), all columns kept.
/// </summary>
public class GlAccountingAccountLookupLine : AuditableEntity
{
    public long LookupLineId { get; set; }

    public long AccountLookupId { get; set; }

    public int PriorityNo { get; set; } = 100;

    public long? LegalEntityId { get; set; }

    public long? BranchId { get; set; }

    public long? CostCenterId { get; set; }

    public string? BusinessLineCode { get; set; }

    public string? ProductCode { get; set; }

    public string? TransactionTypeCode { get; set; }

    public string? CurrencyCode { get; set; }

    public string? CustomerTypeCode { get; set; }

    public string? PortfolioCode { get; set; }

    public string? FundingSourceCode { get; set; }

    public DateOnly EffectiveFromDate { get; set; }

    public DateOnly? EffectiveToDate { get; set; }

    public long? ResultAccountCombinationId { get; set; }

    public long? ResultAccountId { get; set; }

    public long? ResultCompanyValueId { get; set; }

    public long? ResultBranchValueId { get; set; }

    public long? ResultCostCenterValueId { get; set; }

    public long? ResultBusinessLineValueId { get; set; }

    public long? ResultFuture1ValueId { get; set; }

    public long? ResultFuture2ValueId { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}
