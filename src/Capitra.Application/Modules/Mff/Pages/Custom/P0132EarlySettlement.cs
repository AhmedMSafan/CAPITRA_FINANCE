using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Pages.Custom;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mff;
using static Capitra.Application.Modules.Mff.Pages.Custom.MffForm;

namespace Capitra.Application.Modules.Mff.Pages.Custom;

/// <summary>App 110 page 132, After Submit "Calculate Subsidy Refund" (CALC_REFUND), "Load Loan" (LOAD_SETTLEMENT) and "Save Request" (SAVE_SETTLEMENT).</summary>
public sealed class P0132EarlySettlement(ICapitraDbContext db, IApexSqlReader reader) : IApexProcess
{
    public int App => 110;

    public int Page => 132;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mff.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var req = request.Item("REQUEST");
        if (req is not ("CALC_REFUND" or "LOAD_SETTLEMENT" or "SAVE_SETTLEMENT"))
            return Task.FromResult<object>(new { ok = true });
        var loanId = Long(request, "P132_LOAN_ID");
        var settled = Number(request, "P132_PRINCIPAL_SETTLED");
        return MffForm.RunAsync(db, async () =>
        {
            switch (req)
            {
                case "LOAD_SETTLEMENT":
                    return (string.Empty, loanId is null ? null : await LoadAsync(reader, 132, request, ct));

                case "CALC_REFUND":
                {
                    if (loanId is null || (settled ?? 0) <= 0)
                        throw new ApexProcessException("ORA-20074: Select a loan and enter the principal amount to settle.");
                    var principal = Number(request, "P132_PRINCIPAL_AMOUNT");
                    decimal? pct = principal is null or 0 ? null : Math.Round(100 * settled!.Value / principal.Value, 4, MidpointRounding.AwayFromZero);
                    var items = new Dictionary<string, string?> { ["P132_FINANCE_PCT_SETTLED"] = S(pct) };
                    var requested = Date(request, "P132_REQUEST_DATE");
                    var disbursed = Date(request, "P132_DISBURSEMENT_DATE");
                    decimal? months = requested is null || disbursed is null ? null : MtgAjax.MonthsBetween(requested.Value, disbursed.Value);
                    if (months <= 36)
                    {
                        decimal refund = pct > 50 ? 100 : 50;
                        items["P132_SUBSIDY_REFUND_PCT"] = S(refund);
                        items["P132_SUBSIDY_REFUND_AMOUNT"] = S(Math.Round((Number(request, "P132_TOTAL_SUBSIDY") ?? 0) * refund / 100, 2, MidpointRounding.AwayFromZero));
                        return ("Subsidy refund calculated for the first three years.", items);
                    }

                    items["P132_SUBSIDY_REFUND_PCT"] = null;
                    items["P132_SUBSIDY_REFUND_AMOUNT"] = null;
                    return ("After year 3 the Fund refund schedule must be applied. Enter the approved percentage manually.", items);
                }

                default:
                {
                    var date = Date(request, "P132_REQUEST_DATE");
                    var type = Text(request, "P132_SETTLEMENT_TYPE");
                    if (loanId is null || date is null || type is null || (settled ?? 0) <= 0)
                        throw new ApexProcessException("ORA-20075: Loan, request date, settlement type and principal are required.");
                    db.Add(new MffEarlySettlement
                    {
                        LoanId = loanId.Value,
                        RequestDate = date.Value,
                        SettlementType = type,
                        PrincipalSettled = settled!.Value,
                        FinancePctSettled = Number(request, "P132_FINANCE_PCT_SETTLED"),
                        SubsidyRefundPct = Number(request, "P132_SUBSIDY_REFUND_PCT"),
                        SubsidyRefundAmount = Number(request, "P132_SUBSIDY_REFUND_AMOUNT"),
                        EarlySettlementStatus = "REQUESTED",
                        NotesAr = Text(request, "P132_NOTES"),
                    });
                    await db.SaveChangesAsync(ct);
                    return ("Early-settlement request saved.", null);
                }
            }
        }, ct);
    }

    private static string? S(decimal? d) => d?.ToString(CultureInfo.InvariantCulture);
}
