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

    public static readonly IReadOnlyList<ProductActionDefinition> All =
    [
        new(ChequeBatchSubmit, "CHEQUE_WALLET", "Cheque batch submission", "إرسال دفعة الشيكات إلى المحفظة",
            "Cheques Wallet, Cheque Batch page: Submit to wallet. The batch's cheques enter the client wallet.", "CHW",
            ["CHEQUE_AMOUNT"], "3100", "FACTORING"),
    ];

    public static ProductActionDefinition? Find(string? actionCode) => All.FirstOrDefault(x => x.ActionCode == actionCode);
}
