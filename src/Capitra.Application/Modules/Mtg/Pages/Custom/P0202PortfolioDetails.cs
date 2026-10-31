using System.Globalization;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexHtml;

namespace Capitra.Application.Modules.Mtg.Pages.Custom;

#pragma warning disable CA1862 // upper(PORTFOLIO_NO) = upper(:P202_PORTFOLIO_NO) runs in the database

/// <summary>App 105 page 202 "Portfolio Details" (dialog), the PL/SQL region "Dialog Header".</summary>
public sealed class P0202DialogHeader(IApexSqlReader reader) : IApexProcess
{
    public int App => 105;

    public int Page => 202;

    public string Name => "REGION_DIALOG_HEADER";

    public string Permission => Permissions.Mtg.PortfolioView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var id = request.Item("P202_PORTFOLIO_ID");
        string? no = null, status = null;
        if (id is not null)
        {
            try
            {
                var row = await reader.RowAsync(MtgApexPageSql.P202_HEADER, request.Items, ct);
                no = row?.Text("PORTFOLIO_NO");
                status = row?.Text("STATUS");
            }
            catch (Exception e) when (e is not OperationCanceledException)
            {
                // exception when others then null
            }
        }

        var h = new StringBuilder();
        h.Append("<div class=\"fx202-head\"><span class=\"fx202-ic fa " + (id is null ? "fa-briefcase" : "fa-pencil") + "\" aria-hidden=\"true\"></span><div>"
            + "<h2>" + (id is null ? T("New purchased portfolio", "محفظة مشتراة جديدة") : T("Edit portfolio", "تعديل المحفظة") + " " + E(no)) + "</h2>"
            + "<p>" + (id is null
                ? T("Register the portfolio you are buying. You will upload its accounts next.", "سجّل المحفظة التي تشتريها، ثم ارفع حساباتها في الخطوة التالية.")
                : T("Update the portfolio details. Developer and project are locked once accounts exist.", "حدّث بيانات المحفظة. لا يمكن تغيير المطور أو المشروع بعد وجود حسابات.")) + "</p>"
            + (id is null ? "<div class=\"fx202-steps\"><span class=\"on\"><b>1</b>" + T("Portfolio details", "بيانات المحفظة") + "</span><span><b>2</b>" + T("Upload accounts", "رفع الحسابات") + "</span><span><b>3</b>" + T("Review & submit", "المراجعة والإرسال") + "</span></div>" : string.Empty)
            + "</div>" + (status is not null ? "<span class=\"fx202-chip\">" + E(status) + "</span>" : string.Empty) + "</div>");
        h.AppendLine();
        return new { ok = true, html = h.ToString() };
    }
}

/// <summary>App 105 page 202, After Submit: Create Portfolio (CREATE), Update Portfolio (SAVE), Delete Draft Portfolio (DELETE).</summary>
public sealed class P0202SubmitPortfolio(ICapitraDbContext db, MtgPurchasedPortfolioPackage portfolios, ICurrentUser user, IClock clock) : IApexProcess
{
    public int App => 105;

    public int Page => 202;

    public string Name => "SUBMIT";

    public string Permission => Permissions.Mtg.PortfolioEdit;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        await using var tx = await db.BeginTransactionAsync(ct);
        try
        {
            switch (request.Item("REQUEST"))
            {
                case "CREATE":
                    await CreateAsync(request, ct);
                    break;
                case "SAVE":
                    await UpdateAsync(request, ct);
                    break;
                case "DELETE":
                    await DeleteAsync(request, ct);
                    break;
            }

            await tx.CommitAsync(ct);
            return new { ok = true };
        }
        catch (ApexProcessException e)
        {
            await tx.RollbackAsync(ct);
            return new { ok = false, message = e.Message };
        }
    }

    private async Task CreateAsync(ApexRequest r, CancellationToken ct)
    {
        var developerId = Long(r, "P202_DEVELOPER_ID");
        var projectId = Long(r, "P202_PROJECT_ID");
        await CheckAsync(r, developerId, projectId, ct);
        var no = r.Item("P202_PORTFOLIO_NO")!.Trim();
        if (await db.Set<MtgPurchasedPortfolio>().AnyAsync(p => p.PortfolioNo.ToUpper() == no.ToUpper(), ct))
            throw new ApexProcessException("Portfolio No. already exists");

        var p = await portfolios.CreatePortfolioAsync(developerId!.Value, projectId!.Value, user.UserName ?? string.Empty, ct);
        p.PortfolioNo = no;
        p.RequestDate = Date(r, "P202_REQUEST_DATE") ?? DateOnly.FromDateTime(clock.UtcNow);
        p.ProposedPurchaseValue = 0;
        p.ExpectedReturnAmount = 0;
        p.AdminExpenseAmount = 0;
        p.CurrencyCode = r.Item("P202_CURRENCY_CODE") ?? "EGP";
        Figures(r, p);
        await db.SaveChangesAsync(ct);
    }

    private async Task UpdateAsync(ApexRequest r, CancellationToken ct)
    {
        var id = Long(r, "P202_PORTFOLIO_ID") ?? throw new ApexProcessException("ORA-01403: no data found");
        var developerId = Long(r, "P202_DEVELOPER_ID");
        var projectId = Long(r, "P202_PROJECT_ID");
        await CheckAsync(r, developerId, projectId, ct);
        var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == id, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        var no = r.Item("P202_PORTFOLIO_NO")!.Trim();
        if (await db.Set<MtgPurchasedPortfolio>().AnyAsync(x => x.PortfolioNo.ToUpper() == no.ToUpper() && x.PortfolioId != id, ct))
            throw new ApexProcessException("Portfolio No. already exists");
        var accounts = await db.Set<MtgPurchasedPortfolioAccount>().CountAsync(a => a.PortfolioId == id, ct);
        if (accounts > 0 && (p.DeveloperId != developerId || p.ProjectId != projectId))
            throw new ApexProcessException("Developer/Project cannot be changed after portfolio accounts exist");

        p.PortfolioNo = no;
        p.DeveloperId = developerId!.Value;
        p.ProjectId = projectId!.Value;
        p.RequestDate = Date(r, "P202_REQUEST_DATE") ?? p.RequestDate;
        p.CurrencyCode = r.Item("P202_CURRENCY_CODE") ?? p.CurrencyCode;
        Figures(r, p);
        await db.SaveChangesAsync(ct);
    }

    private async Task DeleteAsync(ApexRequest r, CancellationToken ct)
    {
        var id = Long(r, "P202_PORTFOLIO_ID") ?? throw new ApexProcessException("ORA-01403: no data found");
        var p = await db.Set<MtgPurchasedPortfolio>().FirstOrDefaultAsync(x => x.PortfolioId == id, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        if (p.PortfolioStatusCode != "DRAFT")
            throw new ApexProcessException("Only draft portfolios can be deleted");
        if (await db.Set<MtgPurchasedPortfolioAccount>().AnyAsync(a => a.PortfolioId == id, ct))
            throw new ApexProcessException("Remove portfolio accounts before deleting the portfolio");

        db.Set<MtgPurchasedPortfolioApproval>().RemoveRange(db.Set<MtgPurchasedPortfolioApproval>().Where(x => x.PortfolioId == id));
        db.Set<MtgPurchasedPortfolioStatusHist>().RemoveRange(db.Set<MtgPurchasedPortfolioStatusHist>().Where(x => x.PortfolioId == id));
        db.Set<MtgPurchasedPortfolio>().Remove(p);
        await db.SaveChangesAsync(ct);
    }

    // the checks Create and Update share
    private async Task CheckAsync(ApexRequest r, long? developerId, long? projectId, CancellationToken ct)
    {
        var projectDeveloper = await db.Set<MtgProject>().Where(x => x.ProjectId == projectId).Select(x => (long?)x.DeveloperId).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        if (projectDeveloper != developerId)
            throw new ApexProcessException("Selected project does not belong to the selected developer");
        if (string.IsNullOrWhiteSpace(r.Item("P202_PORTFOLIO_NO")))
            throw new ApexProcessException("Portfolio No. is required");
        if (Dec(r, "P202_COVERAGE_PCT") is < 0 or > 100)
            throw new ApexProcessException("Coverage Percentage must be between 0 and 100.");
        if (Dec(r, "P202_RESERVED_AMOUNT_PERCENT") is < 0 or > 100)
            throw new ApexProcessException("Reserved Amount Percentage must be between 0 and 100.");
        if (Dec(r, "P202_PV_RATE") < 0 || Dec(r, "P202_LATE_FEE_RATE") < 0)
            throw new ApexProcessException("Rates cannot be negative.");
        if (Dec(r, "P202_MARGIN_PERCENTAGE") < 0 || Dec(r, "P202_ADMIN_EXPENSE_PERCENT") < 0 || Dec(r, "P202_LEGAL_FEE_PER_UNIT") < 0 || Dec(r, "P202_VALUATION_FEE_PER_UNIT") < 0)
            throw new ApexProcessException("Margin, admin expense and fees cannot be negative.");
    }

    private static void Figures(ApexRequest r, MtgPurchasedPortfolio p)
    {
        p.TermMonths = (int?)Dec(r, "P202_TERM_MONTHS");
        p.AdminExpensePercent = Dec(r, "P202_ADMIN_EXPENSE_PERCENT");
        p.LegalFeePerUnit = Dec(r, "P202_LEGAL_FEE_PER_UNIT");
        p.ValuationFeePerUnit = Dec(r, "P202_VALUATION_FEE_PER_UNIT");
        p.Comments = r.Item("P202_COMMENTS");
        p.CoveragePct = Dec(r, "P202_COVERAGE_PCT");
        p.ReservedAmountPercent = Dec(r, "P202_RESERVED_AMOUNT_PERCENT") ?? 0;
        p.PvRate = Dec(r, "P202_PV_RATE") ?? 0;
        p.LateFeeRate = Dec(r, "P202_LATE_FEE_RATE") ?? 0;
        p.MarginPercentage = Dec(r, "P202_MARGIN_PERCENTAGE");
    }

    // to_number(:ITEM): null when empty, ORA-01722 when it is not a number
    private static decimal? Dec(ApexRequest r, string item) =>
        r.Item(item) is not { } v ? null
        : decimal.TryParse(v, NumberStyles.Number, CultureInfo.InvariantCulture, out var d) ? d
        : throw new ApexProcessException("ORA-01722: invalid number");

    private static long? Long(ApexRequest r, string item) => (long?)Dec(r, item);

    private static DateOnly? Date(ApexRequest r, string item) =>
        r.Item(item) is { } v && DateOnly.TryParse(v, CultureInfo.InvariantCulture, out var d) ? d : null;
}
