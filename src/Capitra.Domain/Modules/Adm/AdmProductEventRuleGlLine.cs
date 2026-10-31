using Capitra.Domain.Common;

namespace Capitra.Domain.Modules.Adm;

/// <summary>
/// ADM_PRODUCT_EVENT_RULE_GL_LINES: a GL entry line of a product event rule (FINOVA_FINANCEING ADM_PRODUCT_EVENT_ACCOUNTING_LINES).
/// A line without a rule charge belongs to the rule itself; a line with one posts that charge.
/// The account is taken straight from the chart of accounts, in place of FINOVA_FINANCEING's account roles and mappings.
/// </summary>
public class AdmProductEventRuleGlLine : AuditableEntity
{
    public long RuleGlLineId { get; set; }

    public long ProductEventRuleId { get; set; }

    /// <summary>Null for a line of the rule itself (FINOVA_FINANCEING scope RULE); set for a charge's line (scope COMPONENT).</summary>
    public long? RuleChargeId { get; set; }

    public int LineNo { get; set; } = 10;

    /// <summary>The journal of the event's posting the line goes to (1, 2 ...), e.g. booking and payment to the developer.</summary>
    public int JournalNo { get; set; } = 1;

    /// <summary>D or C.</summary>
    public string DebitCreditCode { get; set; } = string.Empty;

    /// <summary>The event figure the line posts, one of <see cref="RuleAmounts"/> or, on a charge's line, <see cref="ChargeAmounts"/>.</summary>
    public string AmountCode { get; set; } = string.Empty;

    /// <summary>ACCOUNT (the account below) or BANK_ACCOUNT (the GL account of the bank account chosen at the event).</summary>
    public string AccountSourceCode { get; set; } = AccountSourceAccount;

    /// <summary>GL_ACCOUNTS.ACCOUNT_ID, a posting account; empty when the account comes from the bank account.</summary>
    public long? AccountId { get; set; }

    public string? DescriptionTemplate { get; set; }

    public bool IsActive { get; set; } = true;

    public const string AccountSourceAccount = "ACCOUNT";

    public const string AccountSourceBankAccount = "BANK_ACCOUNT";

    /// <summary>Figures of a purchased portfolio disbursement (PP_DISBURSE) that a rule's own lines can post.</summary>
    public static readonly string[] RuleAmounts = ["FACE_VALUE", "DISCOUNT", "MARGIN_DEFERRED", "MARGIN_IMMEDIATE", "PURCHASE_PRICE", "CHEQUE_AMOUNT"];

    /// <summary>Figures of one charge: the whole amount, or the part taken to income later (pro-rated) or at once.</summary>
    public static readonly string[] ChargeAmounts = ["CHARGE_AMOUNT", "CHARGE_DEFERRED", "CHARGE_IMMEDIATE"];
}
