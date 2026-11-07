using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

/// <summary>
/// App 105 page 721 "Portfolio Snapshots", After Submit "Capture Snapshot" (CAPTURE):
/// FNV_MIS_SNAPSHOT_PKG.capture_snapshot(today), a merge of today's portfolio and account snapshots.
/// </summary>
public sealed class P0721CaptureSnapshot(IApexSqlReader reader, ICapitraDbContext db, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 721;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        if (request.Item("REQUEST") != "CAPTURE")
            return new { ok = true };
        var asOf = DateOnly.FromDateTime(MtgAjax.CairoNow(clock));
        var binds = new Dictionary<string, string?> { ["AS_OF_DATE"] = asOf.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) };

        var portfolios = await db.Set<MtgPurchasedPortfolioSnapshot>().Where(x => x.AsOfDate == asOf).ToDictionaryAsync(x => x.PortfolioId, ct);
        foreach (var s in await reader.QueryAsync(MtgApexPageSql.P721_PORTFOLIOS, binds, ct))
        {
            var id = (long)s.Number("PORTFOLIO_ID")!.Value;
            if (!portfolios.TryGetValue(id, out var t))
            {
                t = new MtgPurchasedPortfolioSnapshot { AsOfDate = asOf, PortfolioId = id };
                db.Set<MtgPurchasedPortfolioSnapshot>().Add(t);
            }

            t.OutstandingValue = s.Number("OUTSTANDING_VALUE") ?? 0;
            t.PurchaseValue = s.Number("PURCHASE_VALUE") ?? 0;
            t.ChequeCoverage = s.Number("CHEQUE_COVERAGE");
            t.DocumentCompletionPct = s.Number("DOCUMENT_COMPLETION_PCT");
            t.OpenExceptionCount = (int)(s.Number("OPEN_EXCEPTION_COUNT") ?? 0);
            t.CurrentStatus = s.Text("CURRENT_STATUS");
        }

        var accounts = await db.Set<MtgAccountSnapshot>().Where(x => x.AsOfDate == asOf).ToDictionaryAsync(x => x.PortfolioAccountId, ct);
        foreach (var s in await reader.QueryAsync(MtgApexPageSql.P721_ACCOUNTS, binds, ct))
        {
            var id = (long)s.Number("PORTFOLIO_ACCOUNT_ID")!.Value;
            if (!accounts.TryGetValue(id, out var t))
            {
                t = new MtgAccountSnapshot { AsOfDate = asOf, PortfolioAccountId = id };
                db.Set<MtgAccountSnapshot>().Add(t);
            }

            t.OutstandingValue = s.Number("OUTSTANDING_VALUE") ?? 0;
            t.ArrearsAmount = s.Number("ARREARS_AMOUNT") ?? 0;
            t.NextDueDate = s.Date("NEXT_DUE_DATE");
            t.PaymentStatus = s.Text("PAYMENT_STATUS");
        }

        await db.SaveChangesAsync(ct);
        return new { ok = true };
    }
}
