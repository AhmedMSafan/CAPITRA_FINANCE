using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

// EF Core translates ToUpper to SQL upper(); string.Equals with a comparison type is not translatable.
#pragma warning disable CA1862

namespace Capitra.Application.Modules.Gl.Journals;

public interface IGlJournalQueryService
{
    Task<JournalSearchResultDto> SearchAsync(JournalSearchRequest request, CancellationToken cancellationToken);

    Task<AccountLedgerResultDto> AccountLedgerAsync(AccountLedgerRequest request, CancellationToken cancellationToken);

    Task<AccountBalancesResultDto> BalancesAsync(AccountBalancesRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Journal search (APEX App 120 page 51, J51_SEARCH), the account ledger (page 52, J52_SEARCH)
/// and account balances (page 20, GL_ACCOUNT_BALANCE_SUMMARY_V).
/// </summary>
public sealed class GlJournalQueryService(ICapitraDbContext db, IClock clock, ICurrentUser currentUser) : IGlJournalQueryService
{
    public const string StageMine = "MINE";

    private const int MaxJournalRows = 500;
    private const int MaxLedgerRows = 2000;

    public async Task<JournalSearchResultDto> SearchAsync(JournalSearchRequest request, CancellationToken cancellationToken)
    {
        var me = (currentUser.UserName ?? string.Empty).ToUpperInvariant();
        var ledgers = await GlJournalService.LedgersAsync(db, cancellationToken);

        var journals =
            from h in db.Set<GlJournalHeader>().AsNoTracking()
            join b in db.Set<GlJournalBatch>() on h.BatchId equals b.BatchId
            join s in db.Set<GlJournalSource>() on h.SourceId equals s.SourceId
            join c in db.Set<GlJournalCategory>() on h.CategoryId equals c.CategoryId
            join l in db.Set<GlLedger>() on h.LedgerId equals l.LedgerId
            join lp in db.Set<GlLedgerPeriod>() on h.LedgerPeriodId equals lp.LedgerPeriodId
            join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
            select new { h, b.BatchNo, b.SourceBatchRef, s.SourceCode, c.CategoryCode, l.LedgerCode, p.PeriodCode };

        var stage = GlRules.Code(request.Stage);
        if (stage == StageMine)
            journals = journals.Where(x => x.h.JournalStatus == GlCodes.JournalSubmitted && (x.h.SubmittedBy ?? x.h.CreatedBy).ToUpper() != me);
        else if (stage is not null)
            journals = journals.Where(x => x.h.JournalStatus == stage);

        if (GlRules.Code(request.Q) is { } q)
            journals = journals.Where(x =>
                x.h.JournalNo.ToUpper().Contains(q) || x.h.JournalName.ToUpper().Contains(q) || x.BatchNo.ToUpper().Contains(q)
                || (x.SourceBatchRef != null && x.SourceBatchRef.ToUpper().Contains(q))
                || (x.h.ReferenceNo != null && x.h.ReferenceNo.ToUpper().Contains(q))
                || (x.h.SourceDocumentNo != null && x.h.SourceDocumentNo.ToUpper().Contains(q))
                || (x.h.Description != null && x.h.Description.ToUpper().Contains(q)));
        if (GlRules.Code(request.Tran) is { } tran)
            journals = journals.Where(x => x.SourceBatchRef != null && x.SourceBatchRef.ToUpper().StartsWith(tran));
        if (request.LedgerId is { } ledgerId)
            journals = journals.Where(x => x.h.LedgerId == ledgerId);
        if (GlRules.Code(request.Source) is { } source)
            journals = journals.Where(x => x.SourceCode == source);
        if (GlRules.Code(request.Category) is { } category)
            journals = journals.Where(x => x.CategoryCode == category);
        if (GlRules.Code(request.Period) is { } period)
            journals = journals.Where(x => x.PeriodCode == period);
        if (request.From is { } from)
            journals = journals.Where(x => x.h.AccountingDate >= from);
        if (request.To is { } to)
            journals = journals.Where(x => x.h.AccountingDate <= to);
        if (request.MinAmount is { } min)
            journals = journals.Where(x => x.h.TotalDr >= min);
        if (request.MaxAmount is { } max)
            journals = journals.Where(x => x.h.TotalDr <= max);
        if (GlRules.Code(request.CreatedBy) is { } by)
            journals = journals.Where(x => x.h.CreatedBy.ToUpper().Contains(by));

        var primaryCoa = ledgers.FirstOrDefault()?.CoaId;
        var segments = primaryCoa is null ? [] : await FilterSegmentsAsync(primaryCoa.Value, cancellationToken);
        if (await CombinationsAsync(primaryCoa, request.Segments, cancellationToken) is { } combinations)
            journals = journals.Where(x => db.Set<GlJournalLine>()
                .Any(l => l.JournalHeaderId == x.h.JournalHeaderId && combinations.Contains(l.AccountCombinationId)));

        var found = await journals
            .OrderByDescending(x => x.h.AccountingDate).ThenByDescending(x => x.h.JournalHeaderId)
            .Take(MaxJournalRows + 1)
            .Select(x => new
            {
                x.h.JournalHeaderId, x.h.JournalNo, x.h.JournalName, x.h.Description, x.BatchNo, x.SourceBatchRef, x.LedgerCode,
                x.SourceCode, x.CategoryCode, x.h.AccountingDate, x.PeriodCode, x.h.CurrencyCode, x.h.TotalDr,
                LineCount = db.Set<GlJournalLine>().Count(l => l.JournalHeaderId == x.h.JournalHeaderId),
                x.h.JournalStatus, x.h.IsReversal, x.h.CreatedBy, x.h.SubmittedBy, x.h.LastPostError, x.h.RowVersion,
            })
            .ToListAsync(cancellationToken);

        var rows = found.Take(MaxJournalRows).Select(x => new JournalRowDto(
            x.JournalHeaderId, x.JournalNo, x.JournalName, Cut(x.Description, 200), x.BatchNo, x.SourceBatchRef, x.LedgerCode,
            x.SourceCode, x.CategoryCode, x.AccountingDate, x.PeriodCode, x.CurrencyCode, x.TotalDr, x.LineCount,
            x.JournalStatus, x.IsReversal, x.CreatedBy, x.SubmittedBy,
            x.JournalStatus == GlCodes.JournalApproved ? Cut(x.LastPostError, 300) : null, x.RowVersion)).ToList();

        // Counts per status of all journals, and how many wait for someone other than their submitter.
        var counts = await db.Set<GlJournalHeader>().AsNoTracking()
            .GroupBy(x => x.JournalStatus)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);
        counts[StageMine] = await db.Set<GlJournalHeader>().AsNoTracking()
            .CountAsync(x => x.JournalStatus == GlCodes.JournalSubmitted && (x.SubmittedBy ?? x.CreatedBy).ToUpper() != me, cancellationToken);

        var sources = await db.Set<GlJournalSource>().AsNoTracking().OrderBy(x => x.SourceCode)
            .Select(x => new JournalOptionDto(x.SourceCode, x.SourceNameEn)).ToListAsync(cancellationToken);
        var categories = await db.Set<GlJournalCategory>().AsNoTracking().OrderBy(x => x.CategoryCode)
            .Select(x => new JournalOptionDto(x.CategoryCode, x.CategoryNameEn)).ToListAsync(cancellationToken);

        return new JournalSearchResultDto(rows, found.Count > MaxJournalRows, counts,
            new JournalSearchListsDto(ledgers, sources, categories, await PeriodCodesAsync(null, cancellationToken), segments));
    }

    public async Task<AccountLedgerResultDto> AccountLedgerAsync(AccountLedgerRequest request, CancellationToken cancellationToken)
    {
        var ledgers = await GlJournalService.LedgersAsync(db, cancellationToken);
        var ledger = ledgers.FirstOrDefault(x => x.LedgerId == request.LedgerId) ?? ledgers.FirstOrDefault();
        if (ledger is null)
            return new AccountLedgerResultDto(null, null, null, false, 0, 0, 0, 0, [], false, [], [], []);

        var periods = await LedgerPeriodsAsync(ledger.LedgerId, cancellationToken);
        var current = CurrentPeriod(periods);
        var fromCode = GlRules.Code(request.FromPeriod) ?? current?.PeriodCode;
        var toCode = GlRules.Code(request.ToPeriod) ?? current?.PeriodCode;
        var segments = await FilterSegmentsAsync(ledger.CoaId!.Value, cancellationToken);
        var codes = periods.Select(x => x.PeriodCode).Reverse().ToList();

        var fromPeriod = periods.FirstOrDefault(x => x.PeriodCode == fromCode);
        var toPeriod = periods.FirstOrDefault(x => x.PeriodCode == toCode);
        var combinations = await CombinationsAsync(ledger.CoaId, request.Segments, cancellationToken);
        var q = GlRules.Code(request.Q);

        if (fromPeriod is null || toPeriod is null || (combinations is null && q is null))
            return new AccountLedgerResultDto(ledger.LedgerId, fromCode, toCode, false, 0, 0, 0, 0, [], false, ledgers, codes, segments);

        var start = fromPeriod.StartDate < toPeriod.StartDate ? fromPeriod.StartDate : toPeriod.StartDate;
        var end = fromPeriod.EndDate > toPeriod.EndDate ? fromPeriod.EndDate : toPeriod.EndDate;

        var impacts =
            from i in db.Set<GlPostingBalanceImpact>().AsNoTracking()
            join lp in db.Set<GlLedgerPeriod>() on i.LedgerPeriodId equals lp.LedgerPeriodId
            join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
            where i.LedgerId == ledger.LedgerId
            select new { i, p.StartDate, p.EndDate };
        if (combinations is not null)
            impacts = impacts.Where(x => combinations.Contains(x.i.AccountCombinationId));

        var broughtForward = await impacts.Where(x => x.StartDate < start)
            .SumAsync(x => (decimal?)(x.i.AccountedDr - x.i.AccountedCr), cancellationToken) ?? 0m;

        var movements = await (
            from x in impacts
            where x.StartDate >= start && x.EndDate <= end
            join h in db.Set<GlJournalHeader>() on x.i.JournalHeaderId equals h.JournalHeaderId
            join b in db.Set<GlJournalBatch>() on h.BatchId equals b.BatchId
            join l in db.Set<GlJournalLine>() on x.i.JournalLineId equals l.JournalLineId
            join c in db.Set<GlAccountCombination>() on x.i.AccountCombinationId equals c.AccountCombinationId
            orderby x.i.AccountingDate, x.i.CreatedAt, x.i.BalanceImpactId
            select new
            {
                x.i.AccountingDate, h.JournalHeaderId, h.JournalNo, h.JournalName, b.SourceBatchRef, c.ConcatenatedAccount,
                l.LineDescription, x.i.AccountedDr, x.i.AccountedCr,
            }
        ).ToListAsync(cancellationToken);

        // Totals and the running balance cover every movement of the accounts; the text search only picks the rows shown.
        var rows = new List<AccountLedgerRowDto>();
        var balance = broughtForward;
        decimal totalDr = 0, totalCr = 0;
        var more = false;
        foreach (var m in movements)
        {
            var before = balance;
            balance += m.AccountedDr - m.AccountedCr;
            totalDr += m.AccountedDr;
            totalCr += m.AccountedCr;

            if (q is not null && !Has(m.JournalNo, q) && !Has(m.JournalName, q) && !Has(m.SourceBatchRef, q))
                continue;
            if (rows.Count == MaxLedgerRows)
            {
                more = true;
                continue;
            }
            rows.Add(new AccountLedgerRowDto(m.AccountingDate, m.JournalHeaderId, m.JournalNo, m.JournalName, m.SourceBatchRef,
                m.ConcatenatedAccount, m.LineDescription, m.AccountedDr, m.AccountedCr, before, balance));
        }

        return new AccountLedgerResultDto(ledger.LedgerId, fromPeriod.PeriodCode, toPeriod.PeriodCode, true,
            broughtForward, totalDr, totalCr, balance, rows, more, ledgers, codes, segments);
    }

    public async Task<AccountBalancesResultDto> BalancesAsync(AccountBalancesRequest request, CancellationToken cancellationToken)
    {
        var ledgers = await GlJournalService.LedgersAsync(db, cancellationToken);
        var ledger = ledgers.FirstOrDefault(x => x.LedgerId == request.LedgerId) ?? ledgers.FirstOrDefault();
        if (ledger is null)
            return new AccountBalancesResultDto(null, null, [], 0, 0, 0, 0, [], []);

        var periods = await LedgerPeriodsAsync(ledger.LedgerId, cancellationToken);
        var codes = periods.Select(x => x.PeriodCode).Reverse().ToList();
        var period = periods.FirstOrDefault(x => x.PeriodCode == GlRules.Code(request.Period)) ?? CurrentPeriod(periods);
        if (period is null)
            return new AccountBalancesResultDto(ledger.LedgerId, null, [], 0, 0, 0, 0, ledgers, codes);

        var balances = await (
            from b in db.Set<GlAccountBalance>().AsNoTracking()
            join lp in db.Set<GlLedgerPeriod>() on b.LedgerPeriodId equals lp.LedgerPeriodId
            join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
            join c in db.Set<GlAccountCombination>() on b.AccountCombinationId equals c.AccountCombinationId
            join a in db.Set<GlAccount>() on c.AccountId equals a.AccountId
            where b.LedgerId == ledger.LedgerId && p.StartDate <= period.StartDate
            select new
            {
                b.AccountCombinationId, c.ConcatenatedAccount, a.AccountCode, a.AccountNameEn, a.AccountType, a.FinancialStatementClass,
                b.CurrencyCode, b.PeriodDr, b.PeriodCr, b.LastPostedAt, IsThis = lp.LedgerPeriodId == period.LedgerPeriodId,
                Earlier = p.StartDate < period.StartDate,
                LastJournalNo = db.Set<GlJournalHeader>().Where(h => h.JournalHeaderId == b.LastPostedHeaderId).Select(h => h.JournalNo).FirstOrDefault(),
            }
        ).ToListAsync(cancellationToken);

        var q = GlRules.Code(request.Q);
        var rows = balances
            .GroupBy(x => new { x.AccountCombinationId, x.CurrencyCode })
            .Select(g =>
            {
                var first = g.First();
                var opening = g.Where(x => x.Earlier).Sum(x => x.PeriodDr - x.PeriodCr);
                var current = g.FirstOrDefault(x => x.IsThis);
                var dr = current?.PeriodDr ?? 0m;
                var cr = current?.PeriodCr ?? 0m;
                return new AccountBalanceRowDto(first.AccountCombinationId, first.ConcatenatedAccount, first.AccountCode, first.AccountNameEn,
                    first.AccountType, first.FinancialStatementClass, first.CurrencyCode, opening, dr, cr, opening + dr - cr,
                    current?.LastJournalNo, current?.LastPostedAt);
            })
            .Where(x => x.Opening != 0 || x.PeriodDr != 0 || x.PeriodCr != 0)
            .Where(x => q is null || Has(x.ConcatenatedAccount, q) || Has(x.AccountNameEn, q))
            .OrderBy(x => x.ConcatenatedAccount).ThenBy(x => x.CurrencyCode)
            .ToList();

        return new AccountBalancesResultDto(ledger.LedgerId, period.PeriodCode, rows,
            rows.Sum(x => x.Opening), rows.Sum(x => x.PeriodDr), rows.Sum(x => x.PeriodCr), rows.Sum(x => x.Closing), ledgers, codes);
    }

    // ---------------------------------------------------------------- helpers

    private sealed record LedgerPeriodRow(long LedgerPeriodId, string PeriodCode, DateOnly StartDate, DateOnly EndDate, bool IsAdjustmentPeriod);

    private Task<List<LedgerPeriodRow>> LedgerPeriodsAsync(long ledgerId, CancellationToken cancellationToken) =>
        (from lp in db.Set<GlLedgerPeriod>().AsNoTracking()
         join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
         where lp.LedgerId == ledgerId
         orderby p.StartDate, p.IsAdjustmentPeriod
         select new LedgerPeriodRow(lp.LedgerPeriodId, p.PeriodCode, p.StartDate, p.EndDate, p.IsAdjustmentPeriod))
        .ToListAsync(cancellationToken);

    /// <summary>The regular period of today, else the last one that has started.</summary>
    private LedgerPeriodRow? CurrentPeriod(List<LedgerPeriodRow> periods)
    {
        var today = GlRules.Today(clock);
        return periods.FirstOrDefault(x => !x.IsAdjustmentPeriod && x.StartDate <= today && x.EndDate >= today)
            ?? periods.LastOrDefault(x => !x.IsAdjustmentPeriod && x.StartDate <= today)
            ?? periods.FirstOrDefault();
    }

    private async Task<List<string>> PeriodCodesAsync(long? ledgerId, CancellationToken cancellationToken) =>
        (await (
            from lp in db.Set<GlLedgerPeriod>().AsNoTracking()
            join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
            where ledgerId == null || lp.LedgerId == ledgerId
            select new { p.PeriodCode, p.StartDate, p.IsAdjustmentPeriod }
        ).Distinct().ToListAsync(cancellationToken))
        .OrderByDescending(x => x.StartDate).ThenByDescending(x => x.IsAdjustmentPeriod)
        .Select(x => x.PeriodCode).Distinct().ToList();

    /// <summary>The enabled segments of a structure with their posting values, for the filters.</summary>
    private async Task<List<JournalFilterSegmentDto>> FilterSegmentsAsync(long coaId, CancellationToken cancellationToken)
    {
        var structure = await db.Set<GlCoaStructure>().AsNoTracking().FirstAsync(x => x.CoaId == coaId, cancellationToken);
        var segments = await db.Set<GlCoaSegment>().AsNoTracking()
            .Where(x => x.CoaId == coaId && x.IsEnabled).OrderBy(x => x.SegmentNum).ToListAsync(cancellationToken);

        var result = new List<JournalFilterSegmentDto>();
        foreach (var s in segments)
        {
            var isAccount = s.SegmentNum == structure.AccountSegmentNum;
            var values = isAccount
                ? await db.Set<GlAccount>().AsNoTracking().Where(x => x.CoaId == coaId).OrderBy(x => x.AccountCode)
                    .Select(x => new JournalOptionDto(x.AccountCode, x.AccountNameEn)).Take(400).ToListAsync(cancellationToken)
                : await db.Set<GlCoaSegmentValue>().AsNoTracking().Where(x => x.CoaSegmentId == s.CoaSegmentId).OrderBy(x => x.ValueCode)
                    .Select(x => new JournalOptionDto(x.ValueCode, x.ValueNameEn)).Take(400).ToListAsync(cancellationToken);
            result.Add(new JournalFilterSegmentDto(s.SegmentNum, s.SegmentCode, s.SegmentNameEn, isAccount, values));
        }

        return result;
    }

    /// <summary>
    /// The combinations whose segment values match the filter, or null when no segment is filtered.
    /// The account segment matches by prefix (1101 finds 110101 and 110102); the others match exactly.
    /// </summary>
    private async Task<IQueryable<long>?> CombinationsAsync(long? coaId, IReadOnlyDictionary<int, string?>? filter, CancellationToken cancellationToken)
    {
        var wanted = (filter ?? new Dictionary<int, string?>())
            .Select(x => (Num: x.Key, Code: GlRules.Code(x.Value)))
            .Where(x => x.Code is not null).ToList();
        if (wanted.Count == 0 || coaId is null)
            return null;

        var accountNum = await db.Set<GlCoaStructure>().Where(x => x.CoaId == coaId).Select(x => x.AccountSegmentNum).FirstAsync(cancellationToken);
        var combinations = db.Set<GlAccountCombination>().AsNoTracking().Where(x => x.CoaId == coaId);
        foreach (var (num, code) in wanted)
        {
            var value = code!;
            combinations = num == accountNum
                ? combinations.Where(c => db.Set<GlAccountCombinationSegment>().Any(s =>
                    s.AccountCombinationId == c.AccountCombinationId && s.SegmentNum == num && s.SegmentValueCode.StartsWith(value)))
                : combinations.Where(c => db.Set<GlAccountCombinationSegment>().Any(s =>
                    s.AccountCombinationId == c.AccountCombinationId && s.SegmentNum == num && s.SegmentValueCode == value));
        }

        return combinations.Select(x => x.AccountCombinationId);
    }

    private static bool Has(string? text, string q) => text is not null && text.Contains(q, StringComparison.OrdinalIgnoreCase);

    private static string? Cut(string? text, int length) => text is null || text.Length <= length ? text : text[..length];
}
