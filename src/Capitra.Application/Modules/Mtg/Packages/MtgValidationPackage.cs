using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Mtg.Pages;

namespace Capitra.Application.Modules.Mtg.Packages;

/// <summary>FNV_VALIDATION_PKG: the checks a portfolio must pass before it is submitted for approval.</summary>
public sealed class MtgValidationPackage(IApexSqlReader reader)
{
    public async Task<bool> DocumentsCompleteAsync(long portfolioId, CancellationToken ct) =>
        await FlagAsync(MtgApexPageSql.VALIDATION_DOCUMENT_READY, portfolioId, ct);

    public async Task<bool> PaymentHistoryValidAsync(long portfolioId, CancellationToken ct) =>
        await CountAsync(MtgApexPageSql.VALIDATION_PAYMENT_PROBLEMS, portfolioId, ct) == 0;

    public async Task<bool> ValuationCompleteAsync(long portfolioId, CancellationToken ct) =>
        await CountAsync(MtgApexPageSql.VALIDATION_VALUATIONS, portfolioId, ct) > 0;

    public async Task<bool> LegalClearanceCompleteAsync(long portfolioId, CancellationToken ct) =>
        await FlagAsync(MtgApexPageSql.VALIDATION_LEGAL_READY, portfolioId, ct);

    public async Task<bool> ChequeCoverageValidAsync(long portfolioId, CancellationToken ct) =>
        await FlagAsync(MtgApexPageSql.VALIDATION_COVERAGE_OK, portfolioId, ct);

    public async Task<bool> GuaranteesCompleteAsync(long portfolioId, CancellationToken ct) =>
        await FlagAsync(MtgApexPageSql.VALIDATION_GUARANTEE_READY, portfolioId, ct);

    public async Task<bool> FinanceStudyCompleteAsync(long portfolioId, CancellationToken ct) =>
        await CountAsync(MtgApexPageSql.VALIDATION_FINANCIAL_STUDIES, portfolioId, ct) > 0;

    /// <summary>is_ready_for_submission</summary>
    public async Task<bool> IsReadyForSubmissionAsync(long portfolioId, CancellationToken ct) =>
        await ReadinessMessageAsync(portfolioId, ct) == "READY";

    /// <summary>readiness_message: what is missing, separated by "; ", or READY.</summary>
    public async Task<string> ReadinessMessageAsync(long portfolioId, CancellationToken ct)
    {
        var msg = new List<string>();
        if (!await DocumentsCompleteAsync(portfolioId, ct))
            msg.Add("Mandatory documents are incomplete");
        if (!await PaymentHistoryValidAsync(portfolioId, ct))
            msg.Add("12-month payment history is not valid");
        if (!await ValuationCompleteAsync(portfolioId, ct))
            msg.Add("Valuation is incomplete");
        if (!await LegalClearanceCompleteAsync(portfolioId, ct))
            msg.Add("Legal clearance is incomplete");
        if (!await ChequeCoverageValidAsync(portfolioId, ct))
            msg.Add("Cheque coverage is insufficient");
        if (!await GuaranteesCompleteAsync(portfolioId, ct))
            msg.Add("Mandatory guarantees are incomplete");
        if (!await FinanceStudyCompleteAsync(portfolioId, ct))
            msg.Add("Financial study is incomplete");
        return msg.Count == 0 ? "READY" : string.Join("; ", msg);
    }

    // flag_value: the flag of the portfolio's row, N when there is none
    private async Task<bool> FlagAsync(string sql, long portfolioId, CancellationToken ct) =>
        ApexSqlLovValues.Text(await reader.ValueAsync(sql, Items(portfolioId), ct)) == "Y";

    private async Task<decimal> CountAsync(string sql, long portfolioId, CancellationToken ct) =>
        await reader.NumberAsync(sql, Items(portfolioId), ct) ?? 0;

    private static Dictionary<string, string?> Items(long portfolioId) =>
        new(StringComparer.OrdinalIgnoreCase) { ["PORTFOLIO_ID"] = portfolioId.ToString(System.Globalization.CultureInfo.InvariantCulture) };
}
