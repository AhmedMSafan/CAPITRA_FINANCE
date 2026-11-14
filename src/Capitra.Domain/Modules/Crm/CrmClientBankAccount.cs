using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Crm;

/// <summary>
/// CRM_CLIENT_BANK_ACCOUNTS: the accounts a customer can be paid into (Customer 360, agreed with Ahmed 4 Nov 2026). The bank is
/// text: TRS_BANKS holds only the banks where we have accounts. Any change sets the account back to PENDING; a second person verifies
/// it, and it can be paid only once the waiting period after its last change has passed.
/// </summary>
public class CrmClientBankAccount : AuditableEntity
{
    public const string Pending = "PENDING";
    public const string Verified = "VERIFIED";
    public const string Suspended = "SUSPENDED";

    public long BankAccountId { get; set; }

    public long ClientId { get; set; }

    public string BankName { get; set; } = string.Empty;

    public string? BranchName { get; set; }

    public string? SwiftCode { get; set; }

    public string? AccountNumber { get; set; }

    /// <summary>Upper case, no spaces.</summary>
    public string? Iban { get; set; }

    public string AccountHolderName { get; set; } = string.Empty;

    public string? AccountHolderNameAr { get; set; }

    public long CurrencyId { get; set; }

    /// <summary>CURRENT, SAVINGS or OTHER.</summary>
    public string AccountTypeCode { get; set; } = "CURRENT";

    public bool IsPrimary { get; set; }

    public string StatusCode { get; set; } = Pending;

    /// <summary>Who last changed the account details; the verifier must be someone else.</summary>
    public string ChangedBy { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    /// <summary>BANK_LETTER, CALL_BACK or BANK_STATEMENT.</summary>
    public string? VerificationMethodCode { get; set; }

    public string? Notes { get; set; }
}

/// <summary>CRM_CLIENT_BANK_ACCOUNT_HISTORY: a copy of a bank account at each action (SAVE, VERIFY, SUSPEND, REACTIVATE).</summary>
public class CrmClientBankAccountHistory : AuditableEntity
{
    public long BankAccountHistoryId { get; set; }

    public long BankAccountId { get; set; }

    public long ClientId { get; set; }

    public string ActionCode { get; set; } = string.Empty;

    public string BankName { get; set; } = string.Empty;

    public string? BranchName { get; set; }

    public string? SwiftCode { get; set; }

    public string? AccountNumber { get; set; }

    public string? Iban { get; set; }

    public string AccountHolderName { get; set; } = string.Empty;

    public long CurrencyId { get; set; }

    public bool IsPrimary { get; set; }

    public string StatusCode { get; set; } = string.Empty;

    public string? VerificationMethodCode { get; set; }

    public string? Notes { get; set; }
}
