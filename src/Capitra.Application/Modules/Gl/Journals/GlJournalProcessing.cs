using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Journals;

/// <summary>
/// C# port of FINOVA gl_journal_processing_pkg: batches, journals and lines, their totals and checks,
/// and the submit, approve, reject, cancel and reversal steps, each written to the journal history.
/// </summary>
public sealed class GlJournalProcessing(ICapitraDbContext db, ICurrentUser currentUser, IClock clock)
{
    public string User => currentUser.UserName ?? "SYSTEM";

    private static string? N(string? value) => value?.Trim().ToUpperInvariant();

    public async Task<long> GetSourceIdAsync(string sourceCode, CancellationToken ct)
    {
        var code = N(sourceCode);
        return await db.Set<GlJournalSource>().Where(s => s.SourceCode == code && s.IsActive).Select(s => (long?)s.SourceId).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("Invalid or inactive journal source: " + sourceCode);
    }

    public async Task<long?> GetCategoryIdAsync(string? categoryCode, CancellationToken ct)
    {
        var code = N(categoryCode);
        if (code is null)
            return null;
        return await db.Set<GlJournalCategory>().Where(c => c.CategoryCode == code && c.IsActive).Select(c => (long?)c.CategoryId).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException("Invalid or inactive journal category: " + categoryCode);
    }

    /// <summary>The next batch or journal number from the ledger's numbering rule, e.g. MJ-2026-000123.</summary>
    public async Task<string> GetNextNumberAsync(long ledgerId, string sourceCode, string? categoryCode, string journalType, DateOnly? accountingDate, CancellationToken ct)
    {
        var sourceId = await GetSourceIdAsync(sourceCode, ct);
        var categoryId = await GetCategoryIdAsync(categoryCode, ct);
        var type = N(journalType);
        var date = accountingDate ?? GlTime.Today(clock);

        var rules = db.Set<GlJournalNumberingRule>().Where(r => r.LedgerId == ledgerId && r.JournalType == type && r.IsActive);
        var rule = await rules.FirstOrDefaultAsync(r => r.SourceId == sourceId && r.CategoryId == categoryId, ct)
            ?? await rules.FirstOrDefaultAsync(r => r.SourceId == null && r.CategoryId == null, ct);
        if (rule is null)
            return type + "-" + date.ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "-" + GlTime.Stamp(clock.UtcNow, "HHmmssfff");

        var result = rule.Prefix;
        if (rule.IsIncludeYear)
            result += "-" + date.ToString("yyyy", CultureInfo.InvariantCulture);
        if (rule.IsIncludePeriod)
            result += "-" + date.ToString("MM", CultureInfo.InvariantCulture);
        result += "-" + rule.NextNumber.ToString(CultureInfo.InvariantCulture).PadLeft(rule.NumberLength, '0');
        rule.NextNumber++;
        await db.SaveChangesAsync(ct);
        return result;
    }

    public void AddActionHistory(long? batchId, long? journalHeaderId, string actionCode, string? oldStatus = null, string? newStatus = null,
        string? reason = null, string? notes = null) =>
        db.Add(new GlJournalActionHistory
        {
            BatchId = batchId,
            JournalHeaderId = journalHeaderId,
            ActionCode = N(actionCode)!,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ActionReason = reason,
            ActionNotes = notes,
            ActionBy = User,
            ActionAt = clock.UtcNow,
        });

    private void AddApprovalHistory(long journalHeaderId, int step, string actionCode, string? notes) =>
        db.Add(new GlJournalApprovalHistory
        {
            JournalHeaderId = journalHeaderId,
            ApprovalStepNo = step,
            ActionCode = actionCode,
            ActionNotes = notes,
            ApproverUser = User,
            ActionAt = clock.UtcNow,
        });

    public async Task<long> CreateBatchAsync(long ledgerId, long ledgerPeriodId, string sourceCode, string? categoryCode, string batchName,
        DateOnly? batchDate, string? description, string? sourceBatchRef, CancellationToken ct)
    {
        var sourceId = await GetSourceIdAsync(sourceCode, ct);
        var categoryId = await GetCategoryIdAsync(categoryCode, ct);
        var batch = new GlJournalBatch
        {
            LedgerId = ledgerId,
            LedgerPeriodId = ledgerPeriodId,
            BatchNo = await GetNextNumberAsync(ledgerId, sourceCode, categoryCode, "BATCH", batchDate, ct),
            BatchName = batchName,
            SourceId = sourceId,
            CategoryId = categoryId,
            BatchDate = batchDate ?? GlTime.Today(clock),
            BatchStatus = "DRAFT",
            ApprovalStatus = "NOT_SUBMITTED",
            PostingStatus = "NOT_POSTED",
            SourceBatchRef = sourceBatchRef,
            Description = description,
        };
        db.Add(batch);
        await db.SaveChangesAsync(ct);
        AddActionHistory(batch.BatchId, null, "CREATE_BATCH", newStatus: "DRAFT", notes: description);
        await db.SaveChangesAsync(ct);
        return batch.BatchId;
    }

    /// <summary>The source document of a journal another module sends (p_source_module ... p_source_transaction_date).</summary>
    public sealed record SourceDocument(string? Module, string? DocumentType, string? DocumentId, string? DocumentNo, DateOnly? TransactionDate);

    public async Task<long> CreateJournalAsync(long batchId, string journalName, DateOnly? journalDate, DateOnly? accountingDate, string currencyCode,
        decimal? conversionRate, string? description, string? referenceNo, bool systemGenerated, CancellationToken ct,
        DateOnly? conversionDate = null, SourceDocument? source = null)
    {
        var batch = await db.Set<GlJournalBatch>().SingleAsync(b => b.BatchId == batchId, ct);
        var sourceCode = await db.Set<GlJournalSource>().Where(s => s.SourceId == batch.SourceId).Select(s => s.SourceCode).SingleAsync(ct);
        var categoryCode = batch.CategoryId is null ? null
            : await db.Set<GlJournalCategory>().Where(c => c.CategoryId == batch.CategoryId).Select(c => c.CategoryCode).SingleAsync(ct);
        var today = GlTime.Today(clock);

        var header = new GlJournalHeader
        {
            BatchId = batchId,
            LedgerId = batch.LedgerId,
            LedgerPeriodId = batch.LedgerPeriodId ?? 0,
            JournalNo = await GetNextNumberAsync(batch.LedgerId, sourceCode, categoryCode, "JOURNAL", accountingDate, ct),
            JournalName = journalName,
            SourceId = batch.SourceId,
            CategoryId = batch.CategoryId ?? 0,
            JournalDate = journalDate ?? today,
            AccountingDate = accountingDate ?? today,
            CurrencyCode = N(currencyCode)!,
            ConversionRate = conversionRate ?? 1,
            JournalStatus = "DRAFT",
            ApprovalStatus = "NOT_SUBMITTED",
            PostingStatus = "NOT_POSTED",
            IsBalanced = false,
            IsSystemGenerated = systemGenerated,
            Description = description,
            ReferenceNo = referenceNo,
            ConversionDate = conversionDate,
            SourceModule = N(source?.Module),
            SourceDocumentType = N(source?.DocumentType),
            SourceDocumentId = source?.DocumentId,
            SourceDocumentNo = source?.DocumentNo,
            SourceTransactionDate = source?.TransactionDate,
        };
        db.Add(header);
        await db.SaveChangesAsync(ct);
        AddActionHistory(batchId, header.JournalHeaderId, "CREATE_JOURNAL", newStatus: "DRAFT", notes: description);
        await db.SaveChangesAsync(ct);
        await RecalcBatchAsync(batchId, ct);
        return header.JournalHeaderId;
    }

    public sealed record LineInput(int LineNo, long AccountCombinationId, string CurrencyCode, decimal EnteredDr, decimal EnteredCr, decimal ConversionRate,
        string? Description, string? ReferenceNo, string? SourceLineId, string? SourceLineNo);

    public async Task<long> AddJournalLineAsync(long journalHeaderId, LineInput p, CancellationToken ct)
    {
        if (p.EnteredDr < 0 || p.EnteredCr < 0 || (p.EnteredDr > 0 && p.EnteredCr > 0))
            throw new ApexProcessException("Journal line must have either debit or credit amount, not both, and amounts cannot be negative.");

        var header = await db.Set<GlJournalHeader>().SingleAsync(h => h.JournalHeaderId == journalHeaderId, ct);
        if (header.PostingStatus is "POSTED" or "POSTING")
            throw new ApexProcessException("Cannot add lines to a posted or posting journal.");

        var line = new GlJournalLine
        {
            JournalHeaderId = journalHeaderId,
            LedgerId = header.LedgerId,
            LedgerPeriodId = header.LedgerPeriodId,
            LineNo = p.LineNo,
            AccountCombinationId = p.AccountCombinationId,
            CurrencyCode = N(p.CurrencyCode)!,
            EnteredDr = p.EnteredDr,
            EnteredCr = p.EnteredCr,
            AccountedDr = Math.Round(p.EnteredDr * p.ConversionRate, 5),
            AccountedCr = Math.Round(p.EnteredCr * p.ConversionRate, 5),
            ConversionRate = p.ConversionRate,
            LineDescription = p.Description,
            ReferenceNo = p.ReferenceNo,
            SourceLineId = p.SourceLineId,
            SourceLineNo = p.SourceLineNo,
        };
        db.Add(line);
        await db.SaveChangesAsync(ct);
        await RecalcJournalAsync(journalHeaderId, ct);
        return line.JournalLineId;
    }

    private async Task<(decimal Dr, decimal Cr, int Count)> TotalsAsync(long journalHeaderId, CancellationToken ct)
    {
        var lines = db.Set<GlJournalLine>().Where(l => l.JournalHeaderId == journalHeaderId && l.IsActive);
        return (await lines.SumAsync(l => l.AccountedDr, ct), await lines.SumAsync(l => l.AccountedCr, ct), await lines.CountAsync(ct));
    }

    private static bool Balanced(decimal dr, decimal cr) => Math.Round(dr, 5) == Math.Round(cr, 5) && dr > 0;

    public async Task RecalcJournalAsync(long journalHeaderId, CancellationToken ct)
    {
        var (dr, cr, _) = await TotalsAsync(journalHeaderId, ct);
        var header = await db.Set<GlJournalHeader>().SingleAsync(h => h.JournalHeaderId == journalHeaderId, ct);
        header.IsBalanced = Balanced(dr, cr);
        if (header.PostingStatus != "POSTED")
            header.JournalStatus = Balanced(dr, cr) ? "VALIDATED" : "DRAFT";
        await db.SaveChangesAsync(ct);
        await RecalcBatchAsync(header.BatchId, ct);
    }

    public async Task RecalcBatchAsync(long batchId, CancellationToken ct)
    {
        var batch = await db.Set<GlJournalBatch>().SingleAsync(b => b.BatchId == batchId, ct);
        var headers = db.Set<GlJournalHeader>().Where(h => h.BatchId == batchId);
        var lines = from h in headers
                    join l in db.Set<GlJournalLine>() on h.JournalHeaderId equals l.JournalHeaderId
                    where l.IsActive
                    select l;
        batch.ActualTotalDr = await lines.SumAsync(l => l.AccountedDr, ct);
        batch.ActualTotalCr = await lines.SumAsync(l => l.AccountedCr, ct);
        batch.JournalCount = await headers.CountAsync(ct);
        if (batch.PostingStatus != "POSTED")
        {
            batch.BatchStatus = await headers.AnyAsync(h => h.JournalStatus == "ERROR", ct) ? "ERROR"
                : await headers.AnyAsync(h => h.JournalStatus == "DRAFT", ct) ? "DRAFT"
                : "VALIDATED";
        }

        await db.SaveChangesAsync(ct);
    }

    private async Task AddValidationResultAsync(GlJournalHeader header, string ruleCode, string severity, string message, CancellationToken ct)
    {
        var ruleId = await db.Set<GlJournalValidationRule>().Where(r => r.RuleCode == ruleCode && r.IsActive)
            .Select(r => (long?)r.ValidationRuleId).FirstOrDefaultAsync(ct);
        db.Add(new GlJournalValidationResult
        {
            ValidationRuleId = ruleId,
            BatchId = header.BatchId,
            JournalHeaderId = header.JournalHeaderId,
            Severity = severity,
            ResultStatus = "OPEN",
            ResultMessage = message,
        });
    }

    /// <summary>validate_journal: closes the old open results, checks lines, balance and combinations, and sets VALIDATED or ERROR.</summary>
    public async Task ValidateJournalAsync(long journalHeaderId, CancellationToken ct)
    {
        foreach (var open in await db.Set<GlJournalValidationResult>().Where(r => r.JournalHeaderId == journalHeaderId && r.ResultStatus == "OPEN").ToListAsync(ct))
        {
            open.ResultStatus = "RESOLVED";
            open.ResolvedBy = User;
            open.ResolvedAt = clock.UtcNow;
        }

        var header = await db.Set<GlJournalHeader>().SingleAsync(h => h.JournalHeaderId == journalHeaderId, ct);
        var (dr, cr, count) = await TotalsAsync(journalHeaderId, ct);
        var errors = 0;
        if (count == 0)
        {
            await AddValidationResultAsync(header, "JOURNAL_LINES_REQUIRED", "ERROR", "Journal has no active lines.", ct);
            errors++;
        }

        if (Math.Round(dr, 5) != Math.Round(cr, 5) || dr == 0)
        {
            await AddValidationResultAsync(header, "BALANCED_JOURNAL", "ERROR", "Journal is not balanced or has zero total.", ct);
            errors++;
        }

        var invalid = await (from l in db.Set<GlJournalLine>()
                             join c in db.Set<GlAccountCombination>() on l.AccountCombinationId equals c.AccountCombinationId
                             where l.JournalHeaderId == journalHeaderId && l.IsActive
                                   && (!c.IsEnabled || !c.IsPostingAllowed || c.ValidationStatus != "VALID")
                             select l).CountAsync(ct);
        if (invalid > 0)
        {
            await AddValidationResultAsync(header, "VALID_ACCOUNT_COMBINATION", "ERROR", "One or more journal lines use invalid or disabled account combinations.", ct);
            errors++;
        }

        var status = errors == 0 ? "VALIDATED" : "ERROR";
        header.JournalStatus = status;
        header.IsBalanced = Balanced(dr, cr);
        AddActionHistory(null, journalHeaderId, "VALIDATE_JOURNAL", newStatus: status);
        await db.SaveChangesAsync(ct);
    }

    public async Task SubmitJournalAsync(long journalHeaderId, string? notes, CancellationToken ct)
    {
        await ValidateJournalAsync(journalHeaderId, ct);
        var header = await db.Set<GlJournalHeader>().SingleAsync(h => h.JournalHeaderId == journalHeaderId, ct);
        var old = header.JournalStatus;
        if (old != "VALIDATED")
            throw new ApexProcessException("Only validated journals can be submitted.");

        header.JournalStatus = "SUBMITTED";
        header.ApprovalStatus = "SUBMITTED";
        header.SubmittedBy = User;
        header.SubmittedAt = clock.UtcNow;
        AddApprovalHistory(journalHeaderId, 1, "SUBMIT", notes);
        AddActionHistory(null, journalHeaderId, "SUBMIT_JOURNAL", old, "SUBMITTED", notes: notes);
        await db.SaveChangesAsync(ct);
    }

    public async Task ApproveJournalAsync(long journalHeaderId, string? notes, CancellationToken ct)
    {
        var header = await db.Set<GlJournalHeader>().SingleAsync(h => h.JournalHeaderId == journalHeaderId, ct);
        var old = header.JournalStatus;
        if (old is not ("SUBMITTED" or "VALIDATED" or "APPROVED"))
            throw new ApexProcessException("Journal is not ready for approval. Current status: " + old);

        header.JournalStatus = "APPROVED";
        header.ApprovalStatus = "APPROVED";
        header.PostingStatus = "READY_TO_POST";
        header.ApprovedBy = User;
        header.ApprovedAt = clock.UtcNow;
        AddApprovalHistory(journalHeaderId, 2, "APPROVE", notes);
        AddActionHistory(null, journalHeaderId, "APPROVE_JOURNAL", old, "APPROVED", notes: notes);
        await db.SaveChangesAsync(ct);
    }

    public async Task RejectJournalAsync(long journalHeaderId, string? reason, CancellationToken ct)
    {
        var header = await db.Set<GlJournalHeader>().SingleAsync(h => h.JournalHeaderId == journalHeaderId, ct);
        var old = header.JournalStatus;
        header.JournalStatus = "DRAFT";
        header.ApprovalStatus = "REJECTED";
        header.PostingStatus = "NOT_POSTED";
        AddApprovalHistory(journalHeaderId, 2, "REJECT", reason);
        AddActionHistory(null, journalHeaderId, "REJECT_JOURNAL", old, "DRAFT", reason: reason);
        await db.SaveChangesAsync(ct);
    }

    public async Task CancelJournalAsync(long journalHeaderId, string? reason, CancellationToken ct)
    {
        var header = await db.Set<GlJournalHeader>().SingleAsync(h => h.JournalHeaderId == journalHeaderId, ct);
        var old = header.JournalStatus;
        if (old == "POSTED")
            throw new ApexProcessException("Posted journals cannot be cancelled. Create a reversal journal instead.");

        header.JournalStatus = "CANCELLED";
        header.PostingStatus = "NOT_POSTED";
        header.CancelledBy = User;
        header.CancelledAt = clock.UtcNow;
        header.CancelReason = reason;
        AddActionHistory(null, journalHeaderId, "CANCEL_JOURNAL", old, "CANCELLED", reason: reason);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>create_reversal_journal: a new batch and journal with the original lines, debit and credit switched.</summary>
    public async Task<long> CreateReversalJournalAsync(long originalHeaderId, long reversalPeriodId, DateOnly? reversalDate, string? reason, CancellationToken ct)
    {
        var orig = await db.Set<GlJournalHeader>().AsNoTracking().SingleAsync(h => h.JournalHeaderId == originalHeaderId, ct);
        if (orig.PostingStatus != "POSTED")
            throw new ApexProcessException("Only posted journals can be reversed.");

        var date = reversalDate ?? GlTime.Today(clock);
        var batch = new GlJournalBatch
        {
            LedgerId = orig.LedgerId,
            LedgerPeriodId = reversalPeriodId,
            BatchNo = "REV-" + Left(orig.JournalNo, 40) + "-" + GlTime.Stamp(clock.UtcNow, "HHmmssfff"),
            BatchName = "Reversal of " + orig.JournalNo,
            SourceId = orig.SourceId,
            CategoryId = orig.CategoryId,
            BatchDate = date,
            BatchStatus = "DRAFT",
            ApprovalStatus = "NOT_SUBMITTED",
            PostingStatus = "NOT_POSTED",
            Description = reason,
        };
        db.Add(batch);
        await db.SaveChangesAsync(ct);

        var header = new GlJournalHeader
        {
            BatchId = batch.BatchId,
            LedgerId = orig.LedgerId,
            LedgerPeriodId = reversalPeriodId,
            JournalNo = "REV-" + Left(orig.JournalNo, 45),
            JournalName = "Reversal of " + orig.JournalName,
            SourceId = orig.SourceId,
            CategoryId = orig.CategoryId,
            JournalDate = date,
            AccountingDate = date,
            CurrencyCode = orig.CurrencyCode,
            RateTypeId = orig.RateTypeId,
            ConversionDate = orig.ConversionDate,
            ConversionRate = orig.ConversionRate,
            JournalStatus = "DRAFT",
            ApprovalStatus = "NOT_SUBMITTED",
            PostingStatus = "NOT_POSTED",
            IsBalanced = false,
            IsSystemGenerated = true,
            IsReversal = true,
            ReversalMethod = "SWITCH_DR_CR",
            ReversedHeaderId = originalHeaderId,
            OriginalHeaderId = originalHeaderId,
            SourceModule = orig.SourceModule,
            SourceDocumentType = orig.SourceDocumentType,
            SourceDocumentId = orig.SourceDocumentId,
            SourceDocumentNo = orig.SourceDocumentNo,
            Description = reason,
            ReferenceNo = orig.ReferenceNo,
        };
        db.Add(header);
        await db.SaveChangesAsync(ct);

        foreach (var l in await db.Set<GlJournalLine>().AsNoTracking().Where(l => l.JournalHeaderId == originalHeaderId && l.IsActive).OrderBy(l => l.LineNo).ToListAsync(ct))
        {
            await AddJournalLineAsync(header.JournalHeaderId, new LineInput(l.LineNo, l.AccountCombinationId, l.CurrencyCode, l.EnteredCr, l.EnteredDr,
                l.ConversionRate, "Reversal: " + l.LineDescription, l.ReferenceNo, l.SourceLineId, l.SourceLineNo), ct);
        }

        await ValidateJournalAsync(header.JournalHeaderId, ct);
        AddActionHistory(batch.BatchId, header.JournalHeaderId, "CREATE_REVERSAL", newStatus: "VALIDATED", reason: reason);
        await db.SaveChangesAsync(ct);
        return header.JournalHeaderId;
    }

    private static string Left(string value, int length) => value.Length <= length ? value : value[..length];
}
