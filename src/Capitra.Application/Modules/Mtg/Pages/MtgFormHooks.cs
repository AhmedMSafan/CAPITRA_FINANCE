using System.Globalization;
using System.Text.Json;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl;
using Capitra.Application.Modules.Mtg.Packages;
using Capitra.Domain.Modules.Mtg;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mtg.Pages;

// The page processes of App 105 around its wizard forms: "Set Defaults" (Before Header, for a new row) and the
// After Submit processes that follow Process form. Values are by column, items by page item, as APEX had them.

internal static class MtgHookValues
{
    /// <summary>:ITEM := nvl(:ITEM, value)</summary>
    public static void Nvl(IDictionary<string, string?> values, string column, string? value)
    {
        if (string.IsNullOrEmpty(values.TryGetValue(column, out var v) ? v : null))
            values[column] = value;
    }

    public static string? Get(IReadOnlyDictionary<string, string?> values, string key) =>
        values.TryGetValue(key, out var v) && !string.IsNullOrEmpty(v) ? v : null;

    public static long? Id(string? text) => long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) ? id : null;

    public static string Date(DateOnly d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static string Number(decimal? n) => n?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
}

/// <summary>Page 205 Portfolio Account Details: Recalculate Portfolio after Create, Save and Delete.</summary>
internal sealed class P0205Hooks(MtgPurchasedPortfolioPackage portfolio) : IApexFormHooks
{
    public int App => 105;

    public int Page => 205;

    public async Task AfterDmlAsync(string request, string? id, IReadOnlyDictionary<string, string?> values, CancellationToken ct)
    {
        if (request is "CREATE" or "SAVE" or "DELETE" && MtgHookValues.Id(MtgHookValues.Get(values, "PORTFOLIO_ID")) is { } portfolioId)
            await portfolio.RecalcPortfolioAsync(portfolioId, ct);
    }
}

/// <summary>Page 302 Document Details: Set Document Defaults, then Store Uploaded Document after Create and Save.</summary>
internal sealed class P0302Hooks(ICapitraDbContext db, ICurrentUser user, IClock clock) : IApexFormHooks
{
    public int App => 105;

    public int Page => 302;

    public Task DefaultsAsync(IDictionary<string, string?> values, IReadOnlyDictionary<string, string?> items, CancellationToken ct)
    {
        MtgHookValues.Nvl(values, "ENTITY_TYPE", "PORTFOLIO");
        MtgHookValues.Nvl(values, "VERIFIED_FLAG", "N");
        MtgHookValues.Nvl(values, "DOCUMENT_STATUS", "RECEIVED");
        return Task.CompletedTask;
    }

    public async Task AfterDmlAsync(string request, string? id, IReadOnlyDictionary<string, string?> values, CancellationToken ct)
    {
        if (request is not ("CREATE" or "SAVE") || MtgHookValues.Id(id) is not { } documentId)
            return;
        var document = await db.Set<MtgPurchasedPortfolioDocument>().FirstAsync(d => d.DocumentId == documentId, ct);

        // P302_FILE_UPLOAD: {name, type, data (base64)}
        if (MtgHookValues.Get(values, "P302_FILE_UPLOAD") is { } upload)
        {
            using var file = JsonDocument.Parse(upload);
            var f = file.RootElement;
            document.FileBlob = Convert.FromBase64String(f.GetProperty("data").GetString() ?? string.Empty);
            document.FileName = f.GetProperty("name").GetString();
            document.MimeType = f.GetProperty("type").GetString();
            document.FileUrl = null;
        }

        if (MtgHookValues.Get(values, "VERIFIED_FLAG") == "Y")
        {
            document.VerifiedBy = user.UserName;
            document.VerifiedAt = clock.UtcNow;
        }
    }
}

/// <summary>Page 304 Due Diligence Details: Set DD Defaults.</summary>
internal sealed class P0304Hooks(IClock clock) : IApexFormHooks
{
    public int App => 105;

    public int Page => 304;

    public Task DefaultsAsync(IDictionary<string, string?> values, IReadOnlyDictionary<string, string?> items, CancellationToken ct)
    {
        MtgHookValues.Nvl(values, "EXCEPTION_FLAG", "N");
        MtgHookValues.Nvl(values, "START_DATE", MtgHookValues.Date(GlTime.Today(clock)));
        return Task.CompletedTask;
    }
}

/// <summary>Page 402 Valuation Details: Set Defaults (the portfolio's project, today's valuation date).</summary>
internal sealed class P0402Hooks(ICapitraDbContext db, IClock clock) : IApexFormHooks
{
    public int App => 105;

    public int Page => 402;

    public async Task DefaultsAsync(IDictionary<string, string?> values, IReadOnlyDictionary<string, string?> items, CancellationToken ct)
    {
        if (MtgHookValues.Id(MtgHookValues.Get(items, "P402_PORTFOLIO_ID")) is not { } portfolioId)
            return;
        var projectId = await db.Set<MtgPurchasedPortfolio>().Where(p => p.PortfolioId == portfolioId).Select(p => (long?)p.ProjectId).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        values["PROJECT_ID"] = projectId.ToString(CultureInfo.InvariantCulture);
        MtgHookValues.Nvl(values, "VALUATION_DATE", MtgHookValues.Date(GlTime.Today(clock)));
    }
}

/// <summary>Page 404 Legal Review Details: Set Defaults (checks N, reviewer the user, review date today).</summary>
internal sealed class P0404Hooks(ICurrentUser user, IClock clock) : IApexFormHooks
{
    public int App => 105;

    public int Page => 404;

    public Task DefaultsAsync(IDictionary<string, string?> values, IReadOnlyDictionary<string, string?> items, CancellationToken ct)
    {
        foreach (var flag in new[] { "OWNERSHIP_VERIFIED_FLAG", "BUILDING_LICENSE_VERIFIED_FLAG", "REGISTRATION_POSSIBLE_FLAG", "MORTGAGE_POSSIBLE_FLAG", "ENCUMBRANCE_FREE_FLAG", "TITLE_DEED_VERIFIED_FLAG" })
            MtgHookValues.Nvl(values, flag, "N");
        MtgHookValues.Nvl(values, "REVIEWED_BY", user.UserName);
        MtgHookValues.Nvl(values, "REVIEW_DATE", MtgHookValues.Date(GlTime.Today(clock)));
        return Task.CompletedTask;
    }
}

/// <summary>Page 406 Financial Study Details: Set Defaults from the portfolio, and Post Process (recalc_financial_study).</summary>
internal sealed class P0406Hooks(ICapitraDbContext db, ICurrentUser user, IClock clock, MtgFinancePackage finance) : IApexFormHooks
{
    public int App => 105;

    public int Page => 406;

    public async Task DefaultsAsync(IDictionary<string, string?> values, IReadOnlyDictionary<string, string?> items, CancellationToken ct)
    {
        if (MtgHookValues.Id(MtgHookValues.Get(items, "P406_PORTFOLIO_ID")) is not { } portfolioId)
            return;
        var p = await db.Set<MtgPurchasedPortfolio>().AsNoTracking().FirstOrDefaultAsync(x => x.PortfolioId == portfolioId, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        values["PORTFOLIO_FACE_VALUE"] = MtgHookValues.Number(p.FaceValue);
        values["OUTSTANDING_VALUE"] = MtgHookValues.Number(p.OutstandingValue);
        values["PROPOSED_PURCHASE_VALUE"] = MtgHookValues.Number(p.ProposedPurchaseValue);
        values["TERM_MONTHS"] = p.TermMonths?.ToString(CultureInfo.InvariantCulture);
        values["EXPECTED_RETURN_AMOUNT"] = MtgHookValues.Number(p.ExpectedReturnAmount);
        values["ADMIN_EXPENSE_AMOUNT"] = MtgHookValues.Number(p.AdminExpenseAmount);
        values["NET_RETURN_AMOUNT"] = MtgHookValues.Number(p.ExpectedReturnAmount - p.AdminExpenseAmount);
        values["CHEQUE_TOTAL_VALUE"] = MtgHookValues.Number(await finance.ChequeTotalAsync(portfolioId, ct));
        values["CHEQUE_COVERAGE_PCT"] = MtgHookValues.Number(await finance.ChequeCoveragePctAsync(portfolioId, ct));
        MtgHookValues.Nvl(values, "PREPARED_BY", user.UserName);
        MtgHookValues.Nvl(values, "PREPARED_DATE", MtgHookValues.Date(GlTime.Today(clock)));
    }

    public async Task AfterDmlAsync(string request, string? id, IReadOnlyDictionary<string, string?> values, CancellationToken ct)
    {
        if (request is "CREATE" or "SAVE" && MtgHookValues.Id(MtgHookValues.Get(values, "PORTFOLIO_ID")) is { } portfolioId)
            await finance.RecalcFinancialStudyAsync(portfolioId, ct);
    }
}

/// <summary>Page 408 Cheque Details: Set Defaults, and Post Process (the financial study again, when there is one).</summary>
internal sealed class P0408Hooks(ICapitraDbContext db, MtgFinancePackage finance) : IApexFormHooks
{
    public int App => 105;

    public int Page => 408;

    public Task DefaultsAsync(IDictionary<string, string?> values, IReadOnlyDictionary<string, string?> items, CancellationToken ct)
    {
        MtgHookValues.Nvl(values, "ENDORSED_TO_COMPANY_FLAG", "N");
        MtgHookValues.Nvl(values, "CHEQUE_STATUS_CODE", "PENDING");
        return Task.CompletedTask;
    }

    public async Task AfterDmlAsync(string request, string? id, IReadOnlyDictionary<string, string?> values, CancellationToken ct)
    {
        if (request is not ("CREATE" or "SAVE" or "DELETE") || MtgHookValues.Id(MtgHookValues.Get(values, "PORTFOLIO_ID")) is not { } portfolioId)
            return;
        if (await db.Set<MtgFinancialStudy>().AnyAsync(f => f.PortfolioId == portfolioId, ct))
            await finance.RecalcFinancialStudyAsync(portfolioId, ct);
    }
}

/// <summary>Page 502 Guarantee Details: Set Defaults, and Post Process (who verified it and when).</summary>
internal sealed class P0502Hooks(ICapitraDbContext db, ICurrentUser user, IClock clock) : IApexFormHooks
{
    public int App => 105;

    public int Page => 502;

    public Task DefaultsAsync(IDictionary<string, string?> values, IReadOnlyDictionary<string, string?> items, CancellationToken ct)
    {
        MtgHookValues.Nvl(values, "VERIFIED_FLAG", "N");
        return Task.CompletedTask;
    }

    public async Task AfterDmlAsync(string request, string? id, IReadOnlyDictionary<string, string?> values, CancellationToken ct)
    {
        if (request is not ("CREATE" or "SAVE") || MtgHookValues.Id(id) is not { } guaranteeId)
            return;
        var g = await db.Set<MtgGuarantee>().FirstAsync(x => x.GuaranteeId == guaranteeId, ct);
        if (MtgHookValues.Get(values, "VERIFIED_FLAG") == "Y")
        {
            g.VerifiedBy ??= user.UserName;
            g.VerifiedAt ??= clock.UtcNow;
        }
        else
        {
            g.VerifiedBy = null;
            g.VerifiedAt = null;
        }
    }
}

/// <summary>Page 504 Insurance Details: Set Defaults.</summary>
internal sealed class P0504Hooks : IApexFormHooks
{
    public int App => 105;

    public int Page => 504;

    public Task DefaultsAsync(IDictionary<string, string?> values, IReadOnlyDictionary<string, string?> items, CancellationToken ct)
    {
        MtgHookValues.Nvl(values, "ASSIGNED_TO_COMPANY_FLAG", "N");
        return Task.CompletedTask;
    }
}

/// <summary>Page 506 Security Deposit Details: Set Defaults.</summary>
internal sealed class P0506Hooks : IApexFormHooks
{
    public int App => 105;

    public int Page => 506;

    public Task DefaultsAsync(IDictionary<string, string?> values, IReadOnlyDictionary<string, string?> items, CancellationToken ct)
    {
        MtgHookValues.Nvl(values, "BLOCKED_FLAG", "Y");
        return Task.CompletedTask;
    }
}

/// <summary>Page 602 Memo Details: Set Defaults (the next version of the portfolio's memo, a draft prepared today by the user).</summary>
internal sealed class P0602Hooks(ICapitraDbContext db, ICurrentUser user, IClock clock) : IApexFormHooks
{
    public int App => 105;

    public int Page => 602;

    public async Task DefaultsAsync(IDictionary<string, string?> values, IReadOnlyDictionary<string, string?> items, CancellationToken ct)
    {
        var portfolioId = MtgHookValues.Id(MtgHookValues.Get(items, "P602_PORTFOLIO_ID"));
        var last = await db.Set<MtgPurchasedPortfolioMemo>().Where(m => m.PortfolioId == portfolioId).MaxAsync(m => (int?)m.VersionNo, ct) ?? 0;
        values["VERSION_NO"] = (last + 1).ToString(CultureInfo.InvariantCulture);
        var today = MtgHookValues.Date(GlTime.Today(clock));
        MtgHookValues.Nvl(values, "MEMO_DATE", today);
        MtgHookValues.Nvl(values, "MEMO_STATUS", "DRAFT");
        MtgHookValues.Nvl(values, "PREPARED_BY", user.UserName);
        MtgHookValues.Nvl(values, "PREPARED_DATE", today);
    }
}
