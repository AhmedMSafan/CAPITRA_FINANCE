namespace Capitra.Application.Modules.Adm.Products;

/// <summary>
/// An action of a product that posts GL outside the workflow, e.g. a button. The code knows where each action happens,
/// so the list is fixed here; Admin Setup only links each action to the product event rule it posts (ADM_PRODUCT_ACTIONS).
/// </summary>
/// <param name="ModuleCode">The module whose GL transaction set ID the posting takes (CAP-{ModuleCode}-...).</param>
/// <param name="AmountCodes">The figures the action passes to the rule's GL lines.</param>
public sealed record ProductActionDefinition(
    string ActionCode, string ProductCode, string ActionNameEn, string ActionNameAr, string Description, string ModuleCode,
    IReadOnlyList<string> AmountCodes, string CostCenter, string BusinessLine);

public static class ProductActions
{
    public const string ChequeBatchSubmit = "CHW_BATCH_SUBMIT";
    public const string ChequeCollection = "CHQ_COLLECTION";
    public const string TreasuryDisburse = "TRS_DISBURSE_EXECUTE";
    public const string ReverseDrawdown = "TRS_RF_DRAWDOWN";
    public const string ReverseTransferConfirm = "TRS_RF_TRANSFER_CONFIRM";

    public static readonly IReadOnlyList<ProductActionDefinition> All =
    [
        new(ChequeBatchSubmit, "CHEQUE_WALLET", "Cheque batch submission", "إرسال دفعة الشيكات إلى المحفظة",
            "Cheques Wallet, Cheque Batch page: Submit to wallet. The batch's cheques enter the client wallet.", "CHW",
            ["CHEQUE_AMOUNT"], "3100", "FACTORING"),
        new(ChequeCollection, "CHEQUE_WALLET", "Cheque collection", "تحصيل الشيكات",
            "Collection and Repayments, Cheques Collection page: Apply Bank Results. The collected cheques' money is in the deposit bank account.",
            "COL", ["CHEQUE_AMOUNT"], "3100", "FACTORING"),
        new(TreasuryDisburse, "TREASURY_FUNDING", "Treasury disbursement", "صرف الخزينة",
            "Treasury, Treasury Disburse page: Execute. BANK_DRAWDOWN is each bank line (the bank puts it in our account at that bank); "
            + "PAYMENT_AMOUNT is each line, own funds included, paid to the client from that account (the rule's bank-account lines).",
            "TRS", ["BANK_DRAWDOWN", "PAYMENT_AMOUNT"], "3100", "FACTORING"),
        // reverse factoring has its own rules and accounts (Ahmed, 7 Nov 2026): the drawdown at Execute, each supplier's transfer when the bank confirms it
        new(ReverseDrawdown, "TREASURY_FUNDING", "Reverse factoring drawdown", "سحب التخصيم العكسي",
            "Treasury, Treasury Disburse page: Execute of a reverse factoring disbursement. BANK_DRAWDOWN is each bank line: the bank puts it in our "
            + "account at that bank (the rule's bank-account lines). Own funds post nothing here.",
            "TRS", ["BANK_DRAWDOWN"], "3100", "REVERSE_FACTORING"),
        new(ReverseTransferConfirm, "TREASURY_FUNDING", "Reverse factoring supplier transfer", "تحويل مورد التخصيم العكسي",
            "Treasury, Treasury Disburse page: Confirm transfer. PAYMENT_AMOUNT is the supplier's transfer, paid from our account (the rule's "
            + "bank-account lines); the buyer owes it at maturity.",
            "TRS", ["PAYMENT_AMOUNT"], "3100", "REVERSE_FACTORING"),
    ];

    public static ProductActionDefinition? Find(string? actionCode) => All.FirstOrDefault(x => x.ActionCode == actionCode);
}
