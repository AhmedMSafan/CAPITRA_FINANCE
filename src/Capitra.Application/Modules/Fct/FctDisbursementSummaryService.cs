using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Trs;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Fct;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Fct;

/// <summary>One invoice of the summary. Financed is what we fund of it: its advance amount, else the invoice amount × the advance rate.</summary>
public sealed record FctSummaryInvoiceDto(long InvoiceId, string InvoiceNumber, DateTime? InvoiceAt, DateTime? DueAt, decimal GrossAmount, decimal Financed,
    bool Eligible, string? IneligibleReason);

/// <summary>The eligible bills of one counterparty (the supplier in reverse factoring, the debtor in factoring) and their totals.</summary>
public sealed record FctSummaryPartyDto(long? ClientId, string Name, string? TaxNumber, int Count, decimal GrossAmount, decimal Financed,
    IReadOnlyList<FctSummaryInvoiceDto> Invoices);

/// <summary>
/// The invoice summary of a disbursement (Ahmed, 7 Nov 2026), shown on the disbursement request and on Treasury Disburse: every invoice
/// received, those that are not eligible, the eligible bills by counterparty, and the value we fund after the advance rate.
/// </summary>
public sealed record FctDisbursementSummaryDto(
    long DisbursementId, string DisbursementNumber, string ClientName, string BusinessLine, bool IsReverse, string CurrencyCode, decimal? AdvanceRatePct,
    decimal? NetAmount, int ReceivedCount, decimal ReceivedGross, int NotEligibleCount, decimal NotEligibleGross, int EligibleCount, decimal EligibleGross,
    decimal EligibleFinanced, IReadOnlyList<FctSummaryPartyDto> Parties, IReadOnlyList<FctSummaryInvoiceDto> NotEligible);

public sealed class FctDisbursementSummaryService(ICapitraDbContext db)
{
    public static bool IsNotEligible(string? statusCode) => string.Equals(statusCode?.Trim(), "NOT_ELIGIBLE", StringComparison.OrdinalIgnoreCase);

    /// <summary>What we fund of an invoice: its advance (or funded) amount, else the invoice amount at the advance rate, else the invoice amount.</summary>
    public static decimal Financed(decimal gross, decimal? advanceAmount, decimal? fundedAmount, decimal? advanceRatePct) =>
        advanceAmount ?? fundedAmount ?? (advanceRatePct is decimal r ? Math.Round(gross * r / 100, 2) : gross);

    public async Task<FctDisbursementSummaryDto> GetAsync(long disbursementId, CancellationToken cancellationToken)
    {
        var d = await db.Set<FctDisbursement>().AsNoTracking().SingleOrDefaultAsync(x => x.DisbursementId == disbursementId, cancellationToken)
            ?? throw new NotFoundException($"Disbursement {disbursementId} was not found.");
        var clientName = await db.Set<CrmClient>().AsNoTracking().Where(x => x.ClientId == d.ClientId).Select(x => x.ClientName)
            .FirstOrDefaultAsync(cancellationToken) ?? $"Client {d.ClientId}";
        var currency = await db.Set<CrmLkpCurrency>().AsNoTracking().Where(x => x.CurrencyId == d.CurrencyId).Select(x => x.CurrencyCode)
            .FirstOrDefaultAsync(cancellationToken) ?? "EGP";
        var profile = (await TrsFundingProfiles.LoadAsync(db, [disbursementId], cancellationToken)).GetValueOrDefault(disbursementId);
        var businessLine = profile?.BusinessLine ?? "FACTORING";

        var batchIds = db.Set<FctInvoiceBatch>().Where(x => x.DisbursementId == disbursementId).Select(x => x.BatchId);
        var rows = await db.Set<FctInvoice>().AsNoTracking()
            .Where(x => x.DisbursementId == disbursementId || (x.BatchId != null && batchIds.Contains(x.BatchId.Value)))
            .OrderBy(x => x.DueAt).ThenBy(x => x.InvoiceId)
            .Select(x => new { x.InvoiceId, x.InvoiceNumber, x.ClientInvoiceNumber, x.ClientInvoiceAt, x.DueAt, x.GrossAmount, x.AdvanceAmount,
                x.FundedAmount, x.AdvanceRatePct, x.StatusCode, x.IneligibleReason, x.DebtorId })
            .ToListAsync(cancellationToken);
        var invoices = rows.Select(x => (Tax: x.DebtorId?.ToString(CultureInfo.InvariantCulture), Dto: new FctSummaryInvoiceDto(
                x.InvoiceId, x.ClientInvoiceNumber ?? x.InvoiceNumber, x.ClientInvoiceAt, x.DueAt, x.GrossAmount,
                Financed(x.GrossAmount, x.AdvanceAmount, x.FundedAmount, x.AdvanceRatePct ?? d.AdvanceRatePct),
                !IsNotEligible(x.StatusCode), x.IneligibleReason)))
            .ToList();

        // the counterparty of each tax number: the customer that has it, else the tax number itself
        var taxes = invoices.Where(x => x.Tax is not null).Select(x => x.Tax!).Distinct().ToList();
        var customers = (await db.Set<CrmClient>().AsNoTracking().Where(x => x.TaxNumber != null && taxes.Contains(x.TaxNumber))
                .Select(x => new { x.ClientId, x.ClientName, x.TaxNumber }).ToListAsync(cancellationToken))
            .GroupBy(x => x.TaxNumber!).ToDictionary(g => g.Key, g => g.OrderBy(x => x.ClientId).First());
        var eligible = invoices.Where(x => x.Dto.Eligible).ToList();
        var parties = eligible.GroupBy(x => x.Tax ?? string.Empty)
            .Select(g =>
            {
                var cp = g.Key.Length > 0 ? customers.GetValueOrDefault(g.Key) : null;
                var name = cp?.ClientName ?? (g.Key.Length > 0 ? $"Tax no. {g.Key}" : "No tax number");
                return new FctSummaryPartyDto(cp?.ClientId, name, g.Key.Length > 0 ? g.Key : null, g.Count(), g.Sum(x => x.Dto.GrossAmount),
                    g.Sum(x => x.Dto.Financed), [.. g.Select(x => x.Dto)]);
            })
            .OrderByDescending(x => x.Financed).ToList();
        var notEligible = invoices.Where(x => !x.Dto.Eligible).Select(x => x.Dto).ToList();

        return new FctDisbursementSummaryDto(d.DisbursementId, d.DisbursementNumber, clientName, businessLine, businessLine == "REVERSE_FACTORING",
            currency, d.AdvanceRatePct, d.NetAmount, invoices.Count, invoices.Sum(x => x.Dto.GrossAmount), notEligible.Count,
            notEligible.Sum(x => x.GrossAmount), eligible.Count, eligible.Sum(x => x.Dto.GrossAmount), eligible.Sum(x => x.Dto.Financed), parties, notEligible);
    }
}
