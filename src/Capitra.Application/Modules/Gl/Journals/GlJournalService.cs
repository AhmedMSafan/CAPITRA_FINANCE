using Capitra.Application.Abstractions;
using Capitra.Application.Common.Crud;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Journals;

public interface IGlJournalService
{
    Task<JournalEntrySetupDto> GetSetupAsync(CancellationToken cancellationToken);

    Task<JournalDto> GetAsync(long journalHeaderId, CancellationToken cancellationToken);

    Task<SaveJournalResultDto> CreateAsync(SaveJournalRequest request, CancellationToken cancellationToken);

    Task<SaveJournalResultDto> UpdateAsync(long journalHeaderId, SaveJournalRequest request, CancellationToken cancellationToken);

    Task<JournalActionResultDto> ActAsync(long journalHeaderId, string action, JournalActionRequest request, CancellationToken cancellationToken);

    Task<BulkJournalResultDto> ActManyAsync(string action, BulkJournalActionRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Manual journals from entry to posting: save, submit, approve (by another user), reject, post, cancel and reverse
/// (APEX App 120 pages 50 and 51, GL_POSTING_API_PKG and the posting engine's balance update).
/// </summary>
public sealed class GlJournalService(ICapitraDbContext db, IClock clock, ICurrentUser currentUser) : IGlJournalService
{
    public const string Submit = "SUBMIT";
    public const string Approve = "APPROVE";
    public const string Reject = "REJECT";
    public const string Post = "POST";
    public const string Cancel = "CANCEL";
    public const string Reverse = "REVERSE";

    /// <summary>Actions the journals page runs on several journals at once.</summary>
    public static readonly string[] BulkActions = [Submit, Approve, Reject, Post, Cancel];

    private const int MaxLines = 500;
    private const int ReasonMaxLength = 1000;
    private const decimal MaxAmount = 9_999_999_999_999_999.99m;

    private string Me => currentUser.UserName ?? "system";

    // ---------------------------------------------------------------- page data

    public async Task<JournalEntrySetupDto> GetSetupAsync(CancellationToken cancellationToken)
    {
        var today = GlRules.Today(clock);
        var ledgers = await LedgersAsync(db, cancellationToken);

        var ledgerIds = ledgers.Select(x => x.LedgerId).ToList();
        var periods = await (
            from lp in db.Set<GlLedgerPeriod>().AsNoTracking()
            join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
            where ledgerIds.Contains(lp.LedgerId)
            orderby p.StartDate, p.IsAdjustmentPeriod
            select new JournalLedgerPeriodDto(lp.LedgerPeriodId, lp.LedgerId, p.PeriodCode, p.StartDate, p.EndDate,
                p.IsAdjustmentPeriod, lp.PeriodStatus, lp.IsManualJournalAllowed)
        ).ToListAsync(cancellationToken);

        var categories = await db.Set<GlJournalCategory>().AsNoTracking()
            .Where(x => x.IsActive && x.IsManualAllowed).OrderBy(x => x.CategoryCode)
            .Select(x => new JournalOptionDto(x.CategoryCode, x.CategoryNameEn))
            .ToListAsync(cancellationToken);

        var structures = new List<JournalStructureDto>();
        foreach (var coaId in ledgers.Where(x => x.CoaId is not null).Select(x => x.CoaId!.Value).Distinct())
            structures.Add(await StructureAsync(coaId, today, cancellationToken));

        return new JournalEntrySetupDto(
            today,
            currentUser.UserName,
            ledgers.FirstOrDefault(x => x.IsPrimary)?.LedgerId ?? ledgers.FirstOrDefault()?.LedgerId,
            GlCodes.CategoryStandard,
            ledgers,
            periods,
            categories,
            structures);
    }

    /// <summary>Active ledgers that have an account structure, primary first.</summary>
    internal static Task<List<JournalLedgerDto>> LedgersAsync(ICapitraDbContext db, CancellationToken cancellationToken) =>
        db.Set<GlLedger>().AsNoTracking()
            .Where(x => x.LedgerStatus == GlCodes.LedgerActive && x.CoaId != null)
            .OrderByDescending(x => x.IsPrimary).ThenBy(x => x.LedgerCode)
            .Select(x => new JournalLedgerDto(x.LedgerId, x.LedgerCode, x.LedgerNameEn, x.LedgerCurrencyCode, x.CoaId, x.IsPrimary))
            .ToListAsync(cancellationToken);

    /// <summary>The enabled segments and the values a manual journal line may use on <paramref name="today"/>.</summary>
    private async Task<JournalStructureDto> StructureAsync(long coaId, DateOnly today, CancellationToken cancellationToken)
    {
        var structure = await db.Set<GlCoaStructure>().AsNoTracking().FirstAsync(x => x.CoaId == coaId, cancellationToken);
        var segments = await db.Set<GlCoaSegment>().AsNoTracking()
            .Where(x => x.CoaId == coaId && x.IsEnabled).OrderBy(x => x.SegmentNum).ToListAsync(cancellationToken);

        var result = new List<JournalSegmentDto>();
        foreach (var segment in segments)
        {
            var isAccount = segment.SegmentNum == structure.AccountSegmentNum;
            List<JournalSegmentValueDto> values;
            if (isAccount)
            {
                var accounts = await db.Set<GlAccount>().AsNoTracking()
                    .Where(x => x.CoaId == coaId && x.IsActive && !x.IsSummary && x.IsPostingAllowed && x.IsManualJournalAllowed
                        && x.EffectiveStartDate <= today && (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                    .OrderBy(x => x.AccountCode)
                    .ToListAsync(cancellationToken);
                values = accounts.Select(a => new JournalSegmentValueDto(a.AccountCode, a.AccountNameEn, Needs(structure, a))).ToList();
            }
            else
            {
                values = await db.Set<GlCoaSegmentValue>().AsNoTracking()
                    .Where(x => x.CoaSegmentId == segment.CoaSegmentId && x.IsEnabled && !x.IsSummary && x.IsPostingAllowed
                        && x.EffectiveStartDate <= today && (x.EffectiveEndDate == null || x.EffectiveEndDate >= today))
                    .OrderBy(x => x.SortOrder).ThenBy(x => x.ValueCode)
                    .Select(x => new JournalSegmentValueDto(x.ValueCode, x.ValueNameEn, null))
                    .ToListAsync(cancellationToken);
            }

            result.Add(new JournalSegmentDto(segment.SegmentNum, segment.SegmentCode, segment.SegmentNameEn, isAccount,
                segment.IsRequired || isAccount, isAccount ? null : segment.DefaultValueCode, segment.DefaultMode, values));
        }

        return new JournalStructureDto(structure.CoaId, structure.CoaCode, structure.DelimiterChar, result);
    }

    private static List<int>? Needs(GlCoaStructure structure, GlAccount account)
    {
        var needs = new List<int>();
        if (account.IsBranchRequired && structure.BranchSegmentNum is { } b) needs.Add(b);
        if (account.IsCostCenterRequired && structure.CostCenterSegmentNum is { } c) needs.Add(c);
        if (account.IsBusinessLineRequired && structure.BusinessLineSegmentNum is { } l) needs.Add(l);
        return needs.Count == 0 ? null : needs;
    }

    public async Task<JournalDto> GetAsync(long journalHeaderId, CancellationToken cancellationToken)
    {
        var h = await db.Set<GlJournalHeader>().AsNoTracking().FirstOrDefaultAsync(x => x.JournalHeaderId == journalHeaderId, cancellationToken)
            ?? throw new NotFoundException("Journal not found.");

        var batch = await db.Set<GlJournalBatch>().AsNoTracking().FirstAsync(x => x.BatchId == h.BatchId, cancellationToken);
        var ledger = await db.Set<GlLedger>().AsNoTracking().FirstAsync(x => x.LedgerId == h.LedgerId, cancellationToken);
        var source = await db.Set<GlJournalSource>().AsNoTracking().FirstAsync(x => x.SourceId == h.SourceId, cancellationToken);
        var category = await db.Set<GlJournalCategory>().AsNoTracking().FirstAsync(x => x.CategoryId == h.CategoryId, cancellationToken);
        var period = await PeriodOfAsync(h.LedgerPeriodId, cancellationToken);

        var lines = await (
            from l in db.Set<GlJournalLine>().AsNoTracking()
            join c in db.Set<GlAccountCombination>() on l.AccountCombinationId equals c.AccountCombinationId
            join a in db.Set<GlAccount>() on c.AccountId equals a.AccountId
            where l.JournalHeaderId == h.JournalHeaderId
            orderby l.LineNo
            select new { l.LineNo, l.AccountCombinationId, c.ConcatenatedAccount, a.AccountCode, a.AccountNameEn, l.EnteredDr, l.EnteredCr, l.LineDescription }
        ).ToListAsync(cancellationToken);

        var combinationIds = lines.Select(x => x.AccountCombinationId).Distinct().ToList();
        var segments = (await db.Set<GlAccountCombinationSegment>().AsNoTracking()
                .Where(x => combinationIds.Contains(x.AccountCombinationId)).ToListAsync(cancellationToken))
            .ToLookup(x => x.AccountCombinationId);

        var lineDtos = lines.Select(l => new JournalLineDto(l.LineNo, l.ConcatenatedAccount, l.AccountCode, l.AccountNameEn,
            segments[l.AccountCombinationId].ToDictionary(s => s.SegmentNum, s => s.SegmentValueCode),
            l.EnteredDr, l.EnteredCr, l.LineDescription)).ToList();

        var history = await db.Set<GlJournalHeaderHistory>().AsNoTracking()
            .Where(x => x.JournalHeaderId == h.JournalHeaderId)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.JournalHistoryId)
            .Select(x => new JournalHistoryDto(x.ActionCode, x.OldStatus, x.NewStatus, x.ActionReason, x.CreatedBy, x.CreatedAt))
            .ToListAsync(cancellationToken);

        JournalLinkDto? reversalOf = null;
        if (h.ReversedHeaderId is { } originalId)
            reversalOf = await db.Set<GlJournalHeader>().AsNoTracking().Where(x => x.JournalHeaderId == originalId)
                .Select(x => new JournalLinkDto(x.JournalHeaderId, x.JournalNo, x.JournalStatus)).FirstOrDefaultAsync(cancellationToken);
        var reversedBy = await ActiveReversalAsync(h.JournalHeaderId, cancellationToken);

        var impacts = h.JournalStatus is GlCodes.JournalPosted or GlCodes.JournalReversed
            ? await ImpactsAsync(h, cancellationToken)
            : [];

        return new JournalDto(
            h.JournalHeaderId, h.JournalNo, batch.BatchNo, batch.SourceBatchRef, h.LedgerId, ledger.LedgerCode, ledger.CoaId ?? 0,
            period.PeriodCode, source.SourceCode, category.CategoryCode, h.JournalName, h.Description, h.ReferenceNo,
            h.JournalDate, h.AccountingDate, h.CurrencyCode, h.JournalStatus, h.TotalDr, h.TotalCr,
            h.IsReversal, reversalOf, reversedBy, h.SourceDocumentType, h.SourceDocumentNo,
            h.CreatedBy, h.CreatedAt, h.SubmittedBy, h.SubmittedAt, h.ApprovedBy, h.ApprovedAt, h.PostedBy, h.PostedAt,
            h.CancelledBy, h.CancelledAt, h.CancelReason,
            h.JournalStatus == GlCodes.JournalRejected ? h.RejectReason : null,
            h.JournalStatus == GlCodes.JournalApproved ? h.LastPostError : null,
            Allowed(h, source, lineDtos.Count, reversedBy),
            lineDtos, impacts, history, h.RowVersion);
    }

    /// <summary>What the journal's state allows (APEX J50_LOAD canEdit, canSubmit ...).</summary>
    private JournalAllowedDto Allowed(GlJournalHeader h, GlJournalSource source, int lineCount, JournalLinkDto? reversedBy)
    {
        var open = h.JournalStatus is GlCodes.JournalDraft or GlCodes.JournalRejected;
        var submitted = h.JournalStatus == GlCodes.JournalSubmitted;
        var ownSubmission = submitted && SameUser(h.SubmittedBy ?? h.CreatedBy, Me);

        return new JournalAllowedDto(
            Edit: open && source.SourceCode == GlCodes.SourceManual && !h.IsReversal,
            Submit: open && lineCount > 0,
            Approve: submitted && !ownSubmission,
            Reject: submitted,
            Post: h.JournalStatus == GlCodes.JournalApproved,
            Cancel: h.JournalStatus is GlCodes.JournalDraft or GlCodes.JournalRejected or GlCodes.JournalSubmitted or GlCodes.JournalApproved,
            Reverse: h.JournalStatus == GlCodes.JournalPosted && !h.IsReversal && reversedBy is null && source.IsReversalAllowed,
            ApproveBlockedReason: ownSubmission ? "You submitted this journal, so another user must approve it." : null);
    }

    private static bool SameUser(string? a, string? b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

    private Task<JournalLinkDto?> ActiveReversalAsync(long journalHeaderId, CancellationToken cancellationToken) =>
        db.Set<GlJournalHeader>().AsNoTracking()
            .Where(x => x.ReversedHeaderId == journalHeaderId && x.JournalStatus != GlCodes.JournalCancelled)
            .OrderBy(x => x.JournalHeaderId)
            .Select(x => new JournalLinkDto(x.JournalHeaderId, x.JournalNo, x.JournalStatus))
            .FirstOrDefaultAsync(cancellationToken);

    /// <summary>The balance of each line's account before and after posting, earlier periods included.</summary>
    private async Task<List<JournalImpactDto>> ImpactsAsync(GlJournalHeader h, CancellationToken cancellationToken)
    {
        var rows = await (
            from i in db.Set<GlPostingBalanceImpact>().AsNoTracking()
            join l in db.Set<GlJournalLine>() on i.JournalLineId equals l.JournalLineId
            join c in db.Set<GlAccountCombination>() on i.AccountCombinationId equals c.AccountCombinationId
            join a in db.Set<GlAccount>() on c.AccountId equals a.AccountId
            join lp in db.Set<GlLedgerPeriod>() on i.LedgerPeriodId equals lp.LedgerPeriodId
            join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
            where i.JournalHeaderId == h.JournalHeaderId
            orderby l.LineNo
            select new
            {
                l.LineNo, c.ConcatenatedAccount, a.AccountNameEn, p.PeriodCode, p.StartDate,
                i.AccountedDr, i.AccountedCr, i.BalanceBefore, i.BalanceAfter, i.AccountCombinationId, i.CurrencyCode,
                Earlier = (
                    from b in db.Set<GlAccountBalance>()
                    join blp in db.Set<GlLedgerPeriod>() on b.LedgerPeriodId equals blp.LedgerPeriodId
                    join bp in db.Set<GlAccountingPeriod>() on blp.PeriodId equals bp.PeriodId
                    where b.LedgerId == i.LedgerId && b.AccountCombinationId == i.AccountCombinationId
                        && b.CurrencyCode == i.CurrencyCode && bp.StartDate < p.StartDate
                    select (decimal?)(b.PeriodDr - b.PeriodCr)
                ).Sum() ?? 0m,
            }
        ).ToListAsync(cancellationToken);

        return rows.Select(r => new JournalImpactDto(r.LineNo, r.ConcatenatedAccount, r.AccountNameEn, r.PeriodCode,
            r.AccountedDr, r.AccountedCr, r.Earlier + r.BalanceBefore, r.Earlier + r.BalanceAfter)).ToList();
    }

    // ---------------------------------------------------------------- save (APEX save_manual_journal)

    public Task<SaveJournalResultDto> CreateAsync(SaveJournalRequest request, CancellationToken cancellationToken) =>
        SaveAsync(null, request, cancellationToken);

    public Task<SaveJournalResultDto> UpdateAsync(long journalHeaderId, SaveJournalRequest request, CancellationToken cancellationToken) =>
        SaveAsync(journalHeaderId, request, cancellationToken);

    private async Task<SaveJournalResultDto> SaveAsync(long? journalHeaderId, SaveJournalRequest request, CancellationToken cancellationToken)
    {
        var source = await db.Set<GlJournalSource>().AsNoTracking().FirstOrDefaultAsync(x => x.SourceCode == GlCodes.SourceManual, cancellationToken)
            ?? throw BusinessValidationException.For("source", "The MANUAL journal source is missing.");
        if (!source.IsActive || !source.IsManualAllowed)
            throw BusinessValidationException.For("source", "The MANUAL journal source does not allow manual journals.");

        GlJournalHeader? h = null;
        if (journalHeaderId is { } id)
        {
            h = await db.Set<GlJournalHeader>().FirstOrDefaultAsync(x => x.JournalHeaderId == id, cancellationToken)
                ?? throw new NotFoundException("Journal not found.");
            if (request.RowVersion != h.RowVersion)
                throw new ConflictException($"Journal {h.JournalNo} was changed by someone else. Reload it and try again.");
            if (h.SourceId != source.SourceId)
                throw Invalid($"Journal {h.JournalNo} comes from another module and cannot be edited here.");
            if (h.IsReversal)
                throw Invalid($"Journal {h.JournalNo} is a reversal: its lines are the original lines with debit and credit switched and cannot be changed.");
            if (h.JournalStatus is not (GlCodes.JournalDraft or GlCodes.JournalRejected))
                throw Invalid($"Journal {h.JournalNo} is {Label(h.JournalStatus)} and can no longer be changed.");
        }

        var errors = new ValidationErrors();
        var name = errors.Text("journalName", "Journal name", request.JournalName, 240, required: true);
        var description = errors.Text("description", "Description", request.Description, 1000);
        var reference = errors.Text("referenceNo", "Reference", request.ReferenceNo, 120);
        if (request.AccountingDate is null)
            errors.Add("accountingDate", "Accounting date is required.");

        var ledgerId = h?.LedgerId ?? request.LedgerId;
        var ledger = ledgerId is null ? null : await db.Set<GlLedger>().AsNoTracking().FirstOrDefaultAsync(x => x.LedgerId == ledgerId, cancellationToken);
        if (ledger is null)
            errors.Add("ledgerId", "Choose the ledger.");
        else if (ledger.LedgerStatus != GlCodes.LedgerActive)
            errors.Add("ledgerId", $"Ledger {ledger.LedgerCode} is inactive.");
        else if (ledger.CoaId is null)
            errors.Add("ledgerId", $"Ledger {ledger.LedgerCode} has no account structure.");

        var categoryCode = GlRules.Code(request.CategoryCode) ?? GlCodes.CategoryStandard;
        var category = await db.Set<GlJournalCategory>().AsNoTracking().FirstOrDefaultAsync(x => x.CategoryCode == categoryCode, cancellationToken);
        if (category is null || !category.IsActive)
            errors.Add("categoryCode", $"Category {categoryCode} does not exist.");
        else if (!category.IsManualAllowed)
            errors.Add("categoryCode", $"Category {categoryCode} cannot be used for manual journals.");

        var lines = request.Lines ?? [];
        if (lines.Count > MaxLines)
            errors.Add("lines", $"A journal can have up to {MaxLines} lines.");
        errors.ThrowIfAny();

        var date = request.AccountingDate!.Value;
        var ledgerPeriod = await LedgerPeriodForDateAsync(ledger!, date, "accountingDate", cancellationToken);
        await CheckPeriodAsync(ledgerPeriod, manual: true, reversal: false, "accountingDate", cancellationToken);

        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        try
        {
            var resolver = await GlCombinationResolver.CreateAsync(db, ledger!.CoaId!.Value, cancellationToken);
            var newLines = new List<GlJournalLine>();
            for (var i = 0; i < lines.Count; i++)
            {
                var field = $"lines[{i}]";
                var line = lines[i];
                var dr = line.Dr ?? 0m;
                var cr = line.Cr ?? 0m;
                var lineDescription = errors.Text(field, "Line description", line.Description, 1000);

                if (dr < 0 || cr < 0)
                    errors.Add(field, "Amounts cannot be negative.");
                else if (dr > 0 && cr > 0)
                    errors.Add(field, "A line has either a debit or a credit amount, not both.");
                else if (dr == 0 && cr == 0)
                    errors.Add(field, "The line has no amount.");
                else if (dr > MaxAmount || cr > MaxAmount || decimal.Round(dr, 2) != dr || decimal.Round(cr, 2) != cr)
                    errors.Add(field, "Amounts have at most two decimals.");

                // Values and accounts must be in effect today, as in APEX (GL_COA_COMBINATION_PKG checks SYSDATE).
                var (combination, error) = await resolver.ResolveAsync(line.Segments, GlRules.Today(clock), manual: true, cancellationToken);
                if (error is not null)
                    errors.Add(field, error);

                if (combination is not null)
                    newLines.Add(new GlJournalLine
                    {
                        LineNo = i + 1,
                        AccountCombinationId = combination.AccountCombinationId,
                        CurrencyCode = ledger.LedgerCurrencyCode,
                        EnteredDr = dr,
                        EnteredCr = cr,
                        AccountedDr = dr,
                        AccountedCr = cr,
                        LineDescription = lineDescription,
                    });
            }

            if (errors.HasErrors)
                errors.Add("lines", "The journal was not saved. Correct the lines marked in red.");
            errors.ThrowIfAny();

            var journalDate = request.JournalDate ?? date;
            if (h is null)
            {
                var batch = new GlJournalBatch
                {
                    LedgerId = ledger.LedgerId,
                    BatchNo = await NextNumberAsync("BT", date.Year, isBatch: true, cancellationToken),
                    BatchName = name!,
                    SourceId = source.SourceId,
                    CategoryId = category!.CategoryId,
                    BatchDate = date,
                    Description = description,
                };
                db.Add(batch);
                await db.SaveChangesAsync(cancellationToken);

                h = new GlJournalHeader
                {
                    BatchId = batch.BatchId,
                    LedgerId = ledger.LedgerId,
                    JournalNo = await NextNumberAsync("JV", date.Year, isBatch: false, cancellationToken),
                    SourceId = source.SourceId,
                    CurrencyCode = ledger.LedgerCurrencyCode,
                    JournalStatus = GlCodes.JournalDraft,
                };
                db.Add(h);
            }
            else
            {
                var batch = await db.Set<GlJournalBatch>().FirstAsync(x => x.BatchId == h.BatchId, cancellationToken);
                batch.BatchName = name!;
                batch.CategoryId = category!.CategoryId;
                batch.BatchDate = date;
                batch.Description = description;

                var old = await db.Set<GlJournalLine>().Where(x => x.JournalHeaderId == h.JournalHeaderId).ToListAsync(cancellationToken);
                foreach (var line in old)
                    db.Remove(line);
            }

            h.LedgerPeriodId = ledgerPeriod.LedgerPeriodId;
            h.JournalName = name!;
            h.CategoryId = category.CategoryId;
            h.JournalDate = journalDate;
            h.AccountingDate = date;
            h.Description = description;
            h.ReferenceNo = reference;
            h.TotalDr = newLines.Sum(x => x.AccountedDr);
            h.TotalCr = newLines.Sum(x => x.AccountedCr);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var line in newLines)
            {
                line.JournalHeaderId = h.JournalHeaderId;
                db.Add(line);
            }
            AddHistory(h, journalHeaderId is null ? "CREATE" : "UPDATE", journalHeaderId is null ? null : h.JournalStatus, h.JournalStatus, null);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            db.DiscardChanges();
            throw;
        }

        string? submitError = null;
        if (request.Submit)
        {
            try
            {
                var fresh = await db.Set<GlJournalHeader>().FirstAsync(x => x.JournalHeaderId == h.JournalHeaderId, cancellationToken);
                await RunAsync(fresh, Submit, null, null, cancellationToken);
            }
            catch (BusinessValidationException ex)
            {
                db.DiscardChanges();
                submitError = string.Join(" ", ex.Errors.Values.SelectMany(x => x));
            }
        }

        return new SaveJournalResultDto(await GetAsync(h.JournalHeaderId, cancellationToken), submitError);
    }

    // ---------------------------------------------------------------- actions

    public async Task<JournalActionResultDto> ActAsync(long journalHeaderId, string action, JournalActionRequest request, CancellationToken cancellationToken)
    {
        var h = await db.Set<GlJournalHeader>().FirstOrDefaultAsync(x => x.JournalHeaderId == journalHeaderId, cancellationToken)
            ?? throw new NotFoundException("Journal not found.");
        if (request.RowVersion is { } rowVersion && rowVersion != h.RowVersion)
            throw new ConflictException($"Journal {h.JournalNo} was changed by someone else. Reload it and try again.");

        var (message, created) = await RunAsync(h, action, request.Reason, request.ReversalDate, cancellationToken);
        return new JournalActionResultDto(await GetAsync(journalHeaderId, cancellationToken), message, created);
    }

    public async Task<BulkJournalResultDto> ActManyAsync(string action, BulkJournalActionRequest request, CancellationToken cancellationToken)
    {
        if (!BulkActions.Contains(action))
            throw Invalid($"{action} cannot be run on several journals.");
        var ids = (request.JournalIds ?? []).Distinct().ToList();
        if (ids.Count == 0)
            throw BusinessValidationException.For("journalIds", "Select at least one journal.");
        if (ids.Count > 200)
            throw BusinessValidationException.For("journalIds", "Select up to 200 journals at a time.");

        var results = new List<BulkJournalItemDto>();
        foreach (var id in ids)
        {
            var h = await db.Set<GlJournalHeader>().FirstOrDefaultAsync(x => x.JournalHeaderId == id, cancellationToken);
            if (h is null)
            {
                results.Add(new BulkJournalItemDto(id, id.ToString(), false, "Journal not found."));
                continue;
            }

            var no = h.JournalNo;
            try
            {
                var (message, _) = await RunAsync(h, action, request.Reason, null, cancellationToken);
                results.Add(new BulkJournalItemDto(id, no, true, message));
            }
            catch (Exception ex) when (ex is BusinessValidationException or ConflictException)
            {
                db.DiscardChanges();
                var message = ex is BusinessValidationException v ? string.Join(" ", v.Errors.Values.SelectMany(x => x)) : ex.Message;
                results.Add(new BulkJournalItemDto(id, no, false, message));
            }
        }

        return new BulkJournalResultDto(results.Count(x => x.Ok), results.Count(x => !x.Ok), results);
    }

    /// <summary>Runs one action on a tracked journal and saves it. A failed post is recorded on the journal before the error is thrown.</summary>
    private async Task<(string Message, JournalLinkDto? Created)> RunAsync(
        GlJournalHeader h, string action, string? reasonText, DateOnly? reversalDate, CancellationToken cancellationToken)
    {
        var reason = string.IsNullOrWhiteSpace(reasonText) ? null : reasonText.Trim();
        if (reason?.Length > ReasonMaxLength)
            throw BusinessValidationException.For("reason", $"The reason is limited to {ReasonMaxLength} characters.");

        var source = await db.Set<GlJournalSource>().AsNoTracking().FirstAsync(x => x.SourceId == h.SourceId, cancellationToken);
        var old = h.JournalStatus;
        var now = clock.UtcNow;

        switch (action)
        {
            case Submit:
                if (old is not (GlCodes.JournalDraft or GlCodes.JournalRejected))
                    throw Invalid($"Journal {h.JournalNo} is {Label(old)} and cannot be submitted.");
                await CheckReadyAsync(h, source, cancellationToken);
                h.JournalStatus = source.IsApprovalRequired ? GlCodes.JournalSubmitted : GlCodes.JournalApproved;
                h.SubmittedBy = Me;
                h.SubmittedAt = now;
                h.RejectReason = null;
                return await FinishAsync(h, "SUBMIT", old, reason,
                    source.IsApprovalRequired ? $"Journal {h.JournalNo} was submitted for approval." : $"Journal {h.JournalNo} is ready to post.", cancellationToken);

            case Approve:
                if (old != GlCodes.JournalSubmitted)
                    throw Invalid($"Only a submitted journal can be approved. Journal {h.JournalNo} is {Label(old)}.");
                if (SameUser(h.SubmittedBy ?? h.CreatedBy, Me))
                    throw Invalid($"You submitted journal {h.JournalNo}, so it must be approved by another user.");
                h.JournalStatus = GlCodes.JournalApproved;
                h.ApprovedBy = Me;
                h.ApprovedAt = now;
                h.LastPostError = null;
                return await FinishAsync(h, "APPROVE", old, reason, $"Journal {h.JournalNo} was approved.", cancellationToken);

            case Reject:
                if (reason is null)
                    throw BusinessValidationException.For("reason", "Enter the reason for rejecting the journal.");
                if (old != GlCodes.JournalSubmitted)
                    throw Invalid($"Only a submitted journal can be rejected. Journal {h.JournalNo} is {Label(old)}.");
                h.JournalStatus = GlCodes.JournalRejected;
                h.RejectReason = reason;
                return await FinishAsync(h, "REJECT", old, reason, $"Journal {h.JournalNo} was sent back to its owner.", cancellationToken);

            case Cancel:
                if (reason is null)
                    throw BusinessValidationException.For("reason", "Enter the reason for cancelling the journal.");
                if (old is GlCodes.JournalPosted or GlCodes.JournalReversed)
                    throw Invalid($"Journal {h.JournalNo} is posted and cannot be cancelled. Reverse it instead.");
                if (old == GlCodes.JournalCancelled)
                    throw Invalid($"Journal {h.JournalNo} is already cancelled.");
                h.JournalStatus = GlCodes.JournalCancelled;
                h.CancelledBy = Me;
                h.CancelledAt = now;
                h.CancelReason = reason;
                return await FinishAsync(h, "CANCEL", old, reason, $"Journal {h.JournalNo} was cancelled.", cancellationToken);

            case Post:
                return await PostAsync(h, source, cancellationToken);

            case Reverse:
                return await ReverseAsync(h, source, reversalDate, reason, cancellationToken);

            default:
                throw Invalid($"Unknown action {action}.");
        }
    }

    private async Task<(string, JournalLinkDto?)> FinishAsync(
        GlJournalHeader h, string actionCode, string old, string? reason, string message, CancellationToken cancellationToken)
    {
        AddHistory(h, actionCode, old, h.JournalStatus, reason);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException($"Journal {h.JournalNo} was changed by someone else. Reload it and try again.");
        }
        return (message, null);
    }

    /// <summary>Lines, balance, period and accounts are fit to submit or post (APEX submit_journal / post_journal checks).</summary>
    private async Task<List<GlJournalLine>> CheckReadyAsync(GlJournalHeader h, GlJournalSource source, CancellationToken cancellationToken)
    {
        var lines = await db.Set<GlJournalLine>().AsNoTracking()
            .Where(x => x.JournalHeaderId == h.JournalHeaderId).OrderBy(x => x.LineNo).ToListAsync(cancellationToken);
        if (lines.Count == 0)
            throw Invalid($"Journal {h.JournalNo} has no lines.");

        var dr = lines.Sum(x => x.AccountedDr);
        var cr = lines.Sum(x => x.AccountedCr);
        if (dr == 0)
            throw Invalid($"Journal {h.JournalNo} has no amounts.");
        if (dr != cr)
            throw Invalid($"Journal {h.JournalNo} is not balanced: debits {dr:N2}, credits {cr:N2}, difference {Math.Abs(dr - cr):N2}.");

        var ledgerPeriod = await db.Set<GlLedgerPeriod>().AsNoTracking().FirstAsync(x => x.LedgerPeriodId == h.LedgerPeriodId, cancellationToken);
        await CheckPeriodAsync(ledgerPeriod, source.SourceCode == GlCodes.SourceManual, h.IsReversal, "action", cancellationToken);

        var manual = source.SourceCode == GlCodes.SourceManual;
        var combinationIds = lines.Select(x => x.AccountCombinationId).Distinct().ToList();
        var accounts = await (
            from c in db.Set<GlAccountCombination>().AsNoTracking()
            join a in db.Set<GlAccount>() on c.AccountId equals a.AccountId
            where combinationIds.Contains(c.AccountCombinationId)
            select new { c.ConcatenatedAccount, c.IsEnabled, a.IsActive, a.IsSummary, a.IsPostingAllowed, a.IsManualJournalAllowed, a.IsSubledgerPostingAllowed }
        ).ToListAsync(cancellationToken);

        foreach (var a in accounts)
        {
            if (!a.IsEnabled)
                throw Invalid($"{a.ConcatenatedAccount}: the account combination is disabled.");
            if (!a.IsActive || a.IsSummary || !a.IsPostingAllowed)
                throw Invalid($"{a.ConcatenatedAccount}: the account is not active or does not allow posting.");
            // A reversal undoes posted lines, so it is not stopped by the manual-journal flag of the account.
            if (manual && !h.IsReversal && !a.IsManualJournalAllowed)
                throw Invalid($"{a.ConcatenatedAccount}: manual journals are not allowed for this account.");
            if (!manual && !a.IsSubledgerPostingAllowed)
                throw Invalid($"{a.ConcatenatedAccount}: sub-ledger posting is not allowed for this account.");
        }

        return lines;
    }

    /// <summary>
    /// Adds each line to its period balance and records the before and after (APEX posting engine apply_balance).
    /// When a check fails, the reason is kept on the journal (POST_FAILED) and the journal stays approved.
    /// </summary>
    private async Task<(string, JournalLinkDto?)> PostAsync(GlJournalHeader h, GlJournalSource source, CancellationToken cancellationToken)
    {
        var old = h.JournalStatus;
        if (old is GlCodes.JournalPosted or GlCodes.JournalReversed)
            throw Invalid($"Journal {h.JournalNo} is already posted.");
        if (old != GlCodes.JournalApproved)
            throw Invalid($"Only an approved journal can be posted. Journal {h.JournalNo} is {Label(old)}.");

        List<GlJournalLine> lines;
        try
        {
            lines = await CheckReadyAsync(h, source, cancellationToken);
        }
        catch (BusinessValidationException ex)
        {
            var reason = string.Join(" ", ex.Errors.Values.SelectMany(x => x));
            h.LastPostError = reason.Length > 2000 ? reason[..2000] : reason;
            AddHistory(h, "POST_FAILED", old, old, h.LastPostError);
            await db.SaveChangesAsync(cancellationToken);
            throw;
        }

        var now = clock.UtcNow;
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        try
        {
            foreach (var line in lines)
            {
                var balance = await db.Set<GlAccountBalance>().FirstOrDefaultAsync(x =>
                    x.LedgerId == h.LedgerId && x.LedgerPeriodId == h.LedgerPeriodId &&
                    x.AccountCombinationId == line.AccountCombinationId && x.CurrencyCode == line.CurrencyCode, cancellationToken);
                if (balance is null)
                {
                    balance = new GlAccountBalance
                    {
                        LedgerId = h.LedgerId,
                        LedgerPeriodId = h.LedgerPeriodId,
                        AccountCombinationId = line.AccountCombinationId,
                        CurrencyCode = line.CurrencyCode,
                    };
                    db.Add(balance);
                }

                var before = balance.PeriodDr - balance.PeriodCr;
                balance.PeriodDr += line.AccountedDr;
                balance.PeriodCr += line.AccountedCr;
                balance.LastPostedHeaderId = h.JournalHeaderId;
                balance.LastPostedAt = now;

                db.Add(new GlPostingBalanceImpact
                {
                    JournalHeaderId = h.JournalHeaderId,
                    JournalLineId = line.JournalLineId,
                    LedgerId = h.LedgerId,
                    LedgerPeriodId = h.LedgerPeriodId,
                    AccountingDate = h.AccountingDate,
                    AccountCombinationId = line.AccountCombinationId,
                    CurrencyCode = line.CurrencyCode,
                    AccountedDr = line.AccountedDr,
                    AccountedCr = line.AccountedCr,
                    BalanceBefore = before,
                    BalanceAfter = balance.PeriodDr - balance.PeriodCr,
                });
                // Saved line by line so two lines on one combination find the row the first one created.
                await db.SaveChangesAsync(cancellationToken);
            }

            h.JournalStatus = GlCodes.JournalPosted;
            h.PostedBy = Me;
            h.PostedAt = now;
            h.LastPostError = null;
            AddHistory(h, "POST", old, h.JournalStatus, null);

            if (h.IsReversal && h.ReversedHeaderId is { } originalId)
            {
                var original = await db.Set<GlJournalHeader>().FirstAsync(x => x.JournalHeaderId == originalId, cancellationToken);
                AddHistory(original, "REVERSED", original.JournalStatus, GlCodes.JournalReversed, $"Reversed by {h.JournalNo}.");
                original.JournalStatus = GlCodes.JournalReversed;
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            db.DiscardChanges();
            throw new ConflictException($"Journal {h.JournalNo} or one of its account balances was changed at the same time. Try posting again.");
        }
        catch
        {
            db.DiscardChanges();
            throw;
        }

        return ($"Journal {h.JournalNo} was posted.", null);
    }

    /// <summary>Creates a draft REV-&lt;no&gt; journal with debit and credit switched (APEX reverse_journal).</summary>
    private async Task<(string, JournalLinkDto?)> ReverseAsync(
        GlJournalHeader h, GlJournalSource source, DateOnly? reversalDate, string? reason, CancellationToken cancellationToken)
    {
        if (reason is null)
            throw BusinessValidationException.For("reason", "Enter the reason for the reversal.");
        if (h.JournalStatus != GlCodes.JournalPosted)
            throw Invalid(h.JournalStatus == GlCodes.JournalReversed
                ? $"Journal {h.JournalNo} is already reversed."
                : $"Only a posted journal can be reversed. Journal {h.JournalNo} is {Label(h.JournalStatus)}; if it is not needed, cancel it instead.");
        if (h.IsReversal)
            throw Invalid($"Journal {h.JournalNo} is itself a reversal and cannot be reversed again.");
        if (await ActiveReversalAsync(h.JournalHeaderId, cancellationToken) is { } existing)
            throw Invalid($"Journal {h.JournalNo} already has the reversal {existing.JournalNo} ({Label(existing.JournalStatus)}). Open it from the journal page.");
        if (!source.IsReversalAllowed)
            throw Invalid($"Source {source.SourceCode} does not allow reversals.");

        var date = reversalDate ?? GlRules.Today(clock);
        var ledger = await db.Set<GlLedger>().AsNoTracking().FirstAsync(x => x.LedgerId == h.LedgerId, cancellationToken);
        var ledgerPeriod = await LedgerPeriodForDateAsync(ledger, date, "reversalDate", cancellationToken);
        await CheckPeriodAsync(ledgerPeriod, source.SourceCode == GlCodes.SourceManual, reversal: true, "reversalDate", cancellationToken);

        var originalBatch = await db.Set<GlJournalBatch>().AsNoTracking().FirstAsync(x => x.BatchId == h.BatchId, cancellationToken);
        var lines = await db.Set<GlJournalLine>().AsNoTracking()
            .Where(x => x.JournalHeaderId == h.JournalHeaderId).OrderBy(x => x.LineNo).ToListAsync(cancellationToken);

        GlJournalHeader reversal;
        await using var transaction = await db.BeginTransactionAsync(cancellationToken);
        try
        {
            // A cancelled earlier reversal keeps its record under a changed number.
            var number = "REV-" + h.JournalNo;
            var cancelled = await db.Set<GlJournalHeader>().FirstOrDefaultAsync(x => x.JournalNo == number && x.JournalStatus == GlCodes.JournalCancelled, cancellationToken);
            if (cancelled is not null)
            {
                cancelled.JournalNo = $"{number}-X{cancelled.JournalHeaderId}";
                await db.SaveChangesAsync(cancellationToken);
            }

            var batch = new GlJournalBatch
            {
                LedgerId = h.LedgerId,
                BatchNo = await NextNumberAsync("BT", date.Year, isBatch: true, cancellationToken),
                BatchName = Truncate("Reversal of " + h.JournalNo, 240),
                SourceId = h.SourceId,
                CategoryId = h.CategoryId,
                BatchDate = date,
                SourceBatchRef = originalBatch.SourceBatchRef,
                Description = reason,
            };
            db.Add(batch);
            await db.SaveChangesAsync(cancellationToken);

            reversal = new GlJournalHeader
            {
                BatchId = batch.BatchId,
                LedgerId = h.LedgerId,
                LedgerPeriodId = ledgerPeriod.LedgerPeriodId,
                JournalNo = number,
                JournalName = Truncate("Reversal of " + h.JournalName, 240),
                SourceId = h.SourceId,
                CategoryId = h.CategoryId,
                JournalDate = date,
                AccountingDate = date,
                CurrencyCode = h.CurrencyCode,
                JournalStatus = GlCodes.JournalDraft,
                TotalDr = h.TotalCr,
                TotalCr = h.TotalDr,
                IsReversal = true,
                ReversedHeaderId = h.JournalHeaderId,
                Description = reason,
                ReferenceNo = h.ReferenceNo,
                SourceDocumentType = h.SourceDocumentType,
                SourceDocumentNo = h.SourceDocumentNo,
            };
            db.Add(reversal);
            await db.SaveChangesAsync(cancellationToken);

            foreach (var line in lines)
                db.Add(new GlJournalLine
                {
                    JournalHeaderId = reversal.JournalHeaderId,
                    LineNo = line.LineNo,
                    AccountCombinationId = line.AccountCombinationId,
                    CurrencyCode = line.CurrencyCode,
                    EnteredDr = line.EnteredCr,
                    EnteredCr = line.EnteredDr,
                    AccountedDr = line.AccountedCr,
                    AccountedCr = line.AccountedDr,
                    LineDescription = line.LineDescription,
                    ReferenceNo = line.ReferenceNo,
                });

            AddHistory(reversal, "CREATE", null, reversal.JournalStatus, $"Reversal of {h.JournalNo}: {reason}");
            AddHistory(h, "REVERSE", h.JournalStatus, h.JournalStatus, $"Reversal {number} created: {reason}");
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            db.DiscardChanges();
            throw;
        }

        return ($"Reversal {reversal.JournalNo} was created as a draft. Submit it for approval to post it.",
            new JournalLinkDto(reversal.JournalHeaderId, reversal.JournalNo, reversal.JournalStatus));
    }

    // ---------------------------------------------------------------- helpers

    /// <summary>The ledger period of a date: the regular period before an adjustment period (APEX ledger_period_for_date).</summary>
    private async Task<GlLedgerPeriod> LedgerPeriodForDateAsync(GlLedger ledger, DateOnly date, string field, CancellationToken cancellationToken) =>
        await (
            from lp in db.Set<GlLedgerPeriod>().AsNoTracking()
            join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
            where lp.LedgerId == ledger.LedgerId && p.StartDate <= date && p.EndDate >= date
            orderby p.IsAdjustmentPeriod, p.StartDate
            select lp
        ).FirstOrDefaultAsync(cancellationToken)
        ?? throw BusinessValidationException.For(field,
            $"No period of ledger {ledger.LedgerCode} covers {date:dd MMM yyyy}. Generate the year on the accounting periods page.");

    /// <summary>The period takes this journal (APEX check_period).</summary>
    private async Task CheckPeriodAsync(GlLedgerPeriod lp, bool manual, bool reversal, string field, CancellationToken cancellationToken)
    {
        var code = (await PeriodOfAsync(lp.LedgerPeriodId, cancellationToken)).PeriodCode;
        if (lp.PeriodStatus is not (GlCodes.PeriodOpen or GlCodes.PeriodFutureEntry))
            throw BusinessValidationException.For(field,
                $"Period {code} {(lp.PeriodStatus == GlCodes.PeriodNeverOpened ? "has never been opened" : "is " + Label(lp.PeriodStatus))}. Journals can only be entered and posted in an open period (accounting periods page).");
        if (lp.PostingMode is not (GlCodes.PostingNormal or GlCodes.PostingFutureOnly or GlCodes.PostingAdjustmentOnly))
            throw BusinessValidationException.For(field, $"Period {code} does not allow posting.");
        if (manual && !lp.IsManualJournalAllowed)
            throw BusinessValidationException.For(field, $"Manual journals are not allowed in period {code}.");
        if (!manual && !lp.IsSubledgerAllowed)
            throw BusinessValidationException.For(field, $"Sub-ledger journals are not allowed in period {code}.");
        if (reversal && !lp.IsReversalAllowed)
            throw BusinessValidationException.For(field, $"Reversals are not allowed in period {code}.");
    }

    private async Task<GlAccountingPeriod> PeriodOfAsync(long ledgerPeriodId, CancellationToken cancellationToken) =>
        await (
            from lp in db.Set<GlLedgerPeriod>().AsNoTracking()
            join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
            where lp.LedgerPeriodId == ledgerPeriodId
            select p
        ).FirstAsync(cancellationToken);

    /// <summary>JV-2026-000001 or BT-2026-000001: the next number of the prefix and year.</summary>
    private async Task<string> NextNumberAsync(string prefix, int year, bool isBatch, CancellationToken cancellationToken)
    {
        var start = $"{prefix}-{year}-";
        var last = isBatch
            ? await db.Set<GlJournalBatch>().Where(x => x.BatchNo.StartsWith(start)).MaxAsync(x => (string?)x.BatchNo, cancellationToken)
            : await db.Set<GlJournalHeader>().Where(x => x.JournalNo.StartsWith(start)).MaxAsync(x => (string?)x.JournalNo, cancellationToken);
        var next = last is not null && int.TryParse(last[start.Length..], out var n) ? n + 1 : 1;
        return $"{start}{next:D6}";
    }

    private void AddHistory(GlJournalHeader h, string actionCode, string? oldStatus, string newStatus, string? reason) =>
        db.Add(new GlJournalHeaderHistory
        {
            JournalHeaderId = h.JournalHeaderId,
            ActionCode = actionCode,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ActionReason = reason is null ? null : Truncate(reason, 2000),
        });

    private static string Truncate(string value, int length) => value.Length > length ? value[..length] : value;

    internal static string Label(string status) => status.Replace('_', ' ').ToLowerInvariant();

    private static BusinessValidationException Invalid(string message) => BusinessValidationException.For("action", message);
}
