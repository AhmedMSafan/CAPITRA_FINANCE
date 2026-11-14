using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Trs;

/// <summary>
/// TRS_BANK_ACCOUNTS: copy of FINOVA TRS_BANK_ACCOUNTS (APEX), all columns kept.
/// </summary>
public class TrsBankAccount : AuditableEntity
{
    public long AccountId { get; set; }

    public long CompanyId { get; set; }

    public long BankId { get; set; }

    public long? ParentAccountId { get; set; }

    public string AccountRef { get; set; } = string.Empty;

    public string AccountNo { get; set; } = string.Empty;

    public string? Iban { get; set; }

    public string CurrencyCode { get; set; } = "EGP";

    public string AccountTypeCode { get; set; } = string.Empty;

    public string? PurposeEn { get; set; }

    public string? PurposeAr { get; set; }

    public DateOnly? OpeningDate { get; set; }

    public DateOnly? ClosingDate { get; set; }

    public string? ErpAccountCode { get; set; }

    public string? AuthorizedSignatoriesJson { get; set; }

    public decimal CurrentBalance { get; set; }

    public bool IsActive { get; set; } = true;
}
