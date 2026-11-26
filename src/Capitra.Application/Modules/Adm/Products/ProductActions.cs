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
    public const string ScreeningInvoiceIssue = "SCR_INVOICE_ISSUE";
    public const string ScreeningInvoicePayment = "SCR_INVOICE_PAYMENT";
    public const string CfNewFinance = "CF_NEW_FINANCE";

    /// <summary>The consumer finance product (goods bought from a supplier on finance).</summary>
    public const string ConsumerFinanceProduct = "CONSUMER_GOODS_FINANCE";

    /// <summary>The screening service product (Federation member companies' screening fees).</summary>
    public const string ScreeningProduct = "SCR_SERVICES";

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
        // screening billing (Ahmed, 22 Nov 2026): the invoice on its accounting date (the period's last day, issued by the End of Day), its
        // cancellation (the same entry reversed), and each payment received
        new(ScreeningInvoiceIssue, ScreeningProduct, "Screening invoice", "فاتورة خدمة الفحص",
            "Screening › Billing: an invoice issued (by the End of Day on the period's last day, or by Issue). INVOICE_AMOUNT is the total the "
            + "company owes; USER_FEE_AMOUNT and INQUIRY_FEE_AMOUNT its two parts. Cancelling the invoice posts the same entry reversed.",
            "SCR", ["INVOICE_AMOUNT", "USER_FEE_AMOUNT", "INQUIRY_FEE_AMOUNT"], "3100", "SCREENING"),
        new(ScreeningInvoicePayment, ScreeningProduct, "Screening invoice payment", "سداد فاتورة خدمة الفحص",
            "Screening › Billing: Record payment. PAYMENT_AMOUNT is the money received into the bank account chosen (the rule's bank-account line) "
            + "against the invoice's receivable.",
            "SCR", ["PAYMENT_AMOUNT"], "3100", "SCREENING"),
        // consumer finance (Ahmed, 26 Nov 2026): New Finance is a screen, not a workflow step, so it reaches its rule through an action; it
        // posts no GL, it only takes the rule's charges (the finance keeps them, and the workflow's delivery step posts them)
        new(CfNewFinance, ConsumerFinanceProduct, "New Finance: customer charges", "تمويل جديد: رسوم العميل",
            "Retail Lending, New Finance: the customer's charges are worked out from the linked rule's charges, each paid upfront or added to "
            + "the finance, before the schedule. Posts no GL: the finance keeps the charges and the Delivery step of its workflow posts them.",
            "LND", [], "3100", "CONSUMER_FINANCE"),
    ];

    public static ProductActionDefinition? Find(string? actionCode) => All.FirstOrDefault(x => x.ActionCode == actionCode);
}
