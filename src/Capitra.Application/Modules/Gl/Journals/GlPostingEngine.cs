using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Journals;

/// <summary>
/// C# port of FINOVA gl_posting_engine_pkg.post_journal with the packages it uses: gl_posting_ref_pkg (run numbers),
/// gl_posting_validation_pkg (pre-post checks), gl_posting_lock_pkg and gl_posting_log_pkg. Posting a journal adds
/// each line to the period and daily balances of its account combination and records the before and after balance.
/// </summary>
public sealed class GlPostingEngine(ICapitraDbContext db, ICurrentUser currentUser, IClock clock)
{
    private string User => currentUser.UserName ?? "SYSTEM";

    /// <summary>gl_posting_ref_pkg.next_reference: PREFIX-YYYYMMDD-000001, counted per scope, ledger and period.</summary>
    private async Task<string> NextReferenceAsync(string scope, long? ledgerId, long? ledgerPeriodId, string prefix, CancellationToken ct)
    {
        var next = (await db.Set<GlPostingSequenceAudit>()
            .Where(a => a.SequenceScope == scope && a.LedgerId == ledgerId && a.LedgerPeriodId == ledgerPeriodId && a.SourceCode == null)
            .MaxAsync(a => (int?)a.LastSequenceNo, ct) ?? 0) + 1;
        var reference = prefix + "-" + GlTime.Today(clock).ToString("yyyyMMdd", CultureInfo.InvariantCulture) + "-" + next.ToString("D6", CultureInfo.InvariantCulture);
        db.Add(new GlPostingSequenceAudit
        {
            SequenceScope = scope,
            LedgerId = ledgerId,
            LedgerPeriodId = ledgerPeriodId,
            LastSequenceNo = next,
            GeneratedReference = reference,
            GeneratedBy = User,
            GeneratedAt = clock.UtcNow,
        });
        return reference;
    }

    private void StatusEvent(long? runId, long? batchId, long? headerId, string? oldStatus, string newStatus, string eventCode, string message) =>
        db.Add(new GlPostingStatusEvent
        {
            PostingRunId = runId,
            JournalBatchId = batchId,
            JournalHeaderId = headerId,
            OldStatus = oldStatus?.Trim().ToUpperInvariant(),
            NewStatus = newStatus,
            EventCode = eventCode,
            EventMessage = message,
            EventBy = User,
            EventAt = clock.UtcNow,
        });

    private async Task<long> CreatePostingRunAsync(GlJournalHeader h, CancellationToken ct)
    {
        var run = new GlPostingRun
        {
            LedgerId = h.LedgerId,
            LedgerPeriodId = h.LedgerPeriodId,
            PostingRunNo = await NextReferenceAsync("POSTING_RUN", h.LedgerId, h.LedgerPeriodId, "PR", ct),
            PostingRunType = "MANUAL",
            RunStatus = "NEW",
            SourceId = h.SourceId,
            SubmittedBatchId = h.BatchId,
            StartedBy = User,
            StartedAt = clock.UtcNow,
            BalanceUpdateMode = "STANDARD",
            ValidationStatus = "NOT_VALIDATED",
        };
        db.Add(run);
        await db.SaveChangesAsync(ct);
        StatusEvent(run.PostingRunId, h.BatchId, null, null, "NEW", "POSTING_RUN_CREATED", "Posting run was created.");
        return run.PostingRunId;
    }

    private void AddResult(long runId, GlJournalHeader h, long? lineId, string ruleCode, bool passed, string message, Dictionary<string, long> rules) =>
        db.Add(new GlPostingValidationResult
        {
            PostingRunId = runId,
            JournalBatchId = h.BatchId,
            JournalHeaderId = h.JournalHeaderId,
            JournalLineId = lineId,
            PostingValidationRuleId = rules.TryGetValue(ruleCode, out var id) ? id : null,
            ValidationStage = "PRE_POST",
            ResultStatus = passed ? "PASSED" : "FAILED",
            SeverityCode = passed ? "INFO" : "ERROR",
            ResultMessage = message,
            CheckedBy = User,
            CheckedAt = clock.UtcNow,
        });

    /// <summary>gl_posting_validation_pkg.validate_journal (PRE_POST); returns whether any check failed.</summary>
    private async Task<bool> ValidateAsync(long runId, GlJournalHeader h, CancellationToken ct)
    {
        var rules = (await db.Set<GlPostingValidationRule>().Where(r => r.IsActive).Select(r => new { r.RuleCode, r.PostingValidationRuleId }).ToListAsync(ct))
            .GroupBy(r => r.RuleCode).ToDictionary(g => g.Key, g => g.Max(r => r.PostingValidationRuleId));
        var lines = await db.Set<GlJournalLine>().Where(l => l.JournalHeaderId == h.JournalHeaderId && l.IsActive).ToListAsync(ct);
        decimal dr = lines.Sum(l => l.AccountedDr), cr = lines.Sum(l => l.AccountedCr);
        var failed = false;
        void Check(string rule, bool ok, string pass, string fail, long? lineId = null)
        {
            AddResult(runId, h, lineId, rule, ok, ok ? pass : fail, rules);
            failed |= !ok;
        }

        Check("LINE_EXISTS", lines.Count > 0, "Journal contains active lines.", "Journal has no active lines.");
        Check("BALANCED_JOURNAL", Math.Abs(dr - cr) <= 0.01m && dr + cr > 0, "Journal debit and credit totals are balanced.",
            "Journal is not balanced. Dr=" + dr.ToString(CultureInfo.InvariantCulture) + ", Cr=" + cr.ToString(CultureInfo.InvariantCulture));
        var period = await db.Set<GlLedgerPeriod>().Where(p => p.LedgerPeriodId == h.LedgerPeriodId).Select(p => new { p.PeriodStatus, p.PostingMode }).FirstOrDefaultAsync(ct);
        if (period is null)
            Check("OPEN_PERIOD", false, string.Empty, "Ledger period record was not found.");
        else
            Check("OPEN_PERIOD", period.PeriodStatus is "OPEN" or "FUTURE_ENTRY" && period.PostingMode is "NORMAL" or "FUTURE_ONLY" or "ADJUSTMENT_ONLY",
                "Ledger period is available for posting.", "Ledger period is not available for posting.");
        Check("APPROVED_JOURNAL", h.ApprovalStatus is "APPROVED" or "NOT_REQUIRED" || h.JournalStatus == "APPROVED",
            "Journal is approved or approval is not required.", "Journal is not approved for posting.");
        Check("NOT_ALREADY_POSTED", h.PostingStatus != "POSTED", "Journal is not already posted.", "Journal is already posted.");
        Check("DUPLICATE_POSTING", !await db.Set<GlPostingBalanceImpact>().AnyAsync(i => i.JournalHeaderId == h.JournalHeaderId && i.ImpactStatus == "APPLIED", ct),
            "No existing applied posting impact was found.", "Applied posting impacts already exist for this journal.");
        foreach (var line in lines)
            Check("VALID_ACCOUNT_COMBINATION", line.AccountCombinationId != 0, "Account combination is populated.", "Account combination is missing.", line.JournalLineId);
        await db.SaveChangesAsync(ct);
        return failed;
    }

    /// <summary>gl_posting_lock_pkg.acquire_lock: one ACTIVE lock per type and key.</summary>
    private async Task<GlPostingLock> AcquireLockAsync(string type, string key, GlJournalHeader h, long runId, int expireMinutes, CancellationToken ct)
    {
        var now = clock.UtcNow;
        if (await db.Set<GlPostingLock>().AnyAsync(l => l.LockType == type && l.LockKey == key && l.LockStatus == "ACTIVE" && (l.ExpiresAt == null || l.ExpiresAt > now), ct))
            throw new ApexProcessException($"Posting lock already exists for {type}: {key}");

        string[] finished = ["RELEASED", "EXPIRED", "FORCE_RELEASED"];
        foreach (var old in await db.Set<GlPostingLock>().Where(l => l.LockType == type && l.LockKey == key && finished.Contains(l.LockStatus)).ToListAsync(ct))
            db.Remove(old);

        var row = new GlPostingLock
        {
            LockType = type,
            LockKey = key,
            LedgerId = h.LedgerId,
            LedgerPeriodId = h.LedgerPeriodId,
            PostingRunId = runId,
            LockStatus = "ACTIVE",
            LockedBy = User,
            LockedAt = now,
            ExpiresAt = now.AddMinutes(expireMinutes),
        };
        db.Add(row);
        await db.SaveChangesAsync(ct);
        return row;
    }

    private async Task ReleaseLockAsync(GlPostingLock row, string reason, CancellationToken ct)
    {
        foreach (var old in await db.Set<GlPostingLock>().Where(l => l.LockType == row.LockType && l.LockKey == row.LockKey && l.LockStatus == "RELEASED"
                     && l.PostingLockId != row.PostingLockId).ToListAsync(ct))
            db.Remove(old);
        if (row.LockStatus == "ACTIVE")
        {
            row.LockStatus = "RELEASED";
            row.ReleasedBy = User;
            row.ReleasedAt = clock.UtcNow;
            row.ReleaseReason = reason;
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>apply_balance: adds one line to the period balance and the daily balance, and records the impact.</summary>
    private async Task ApplyBalanceAsync(long runId, long runLineId, GlJournalHeader h, GlJournalLine line, CancellationToken ct)
    {
        var currency = line.CurrencyCode.Trim().ToUpperInvariant();
        var now = clock.UtcNow;
        var balance = await db.Set<GlAccountBalance>().FirstOrDefaultAsync(b => b.LedgerId == line.LedgerId && b.LedgerPeriodId == line.LedgerPeriodId
            && b.AccountCombinationId == line.AccountCombinationId && b.CurrencyCode == currency, ct);
        decimal beforeDr = balance?.PeriodDr ?? 0, beforeCr = balance?.PeriodCr ?? 0;
        if (balance is null)
        {
            db.Add(new GlAccountBalance
            {
                LedgerId = line.LedgerId,
                LedgerPeriodId = line.LedgerPeriodId,
                AccountCombinationId = line.AccountCombinationId,
                CurrencyCode = currency,
                PeriodDr = line.AccountedDr,
                PeriodCr = line.AccountedCr,
                ClosingDr = line.AccountedDr,
                ClosingCr = line.AccountedCr,
                LastPostedHeaderId = h.JournalHeaderId,
                LastPostedAt = now,
            });
        }
        else
        {
            balance.ClosingDr = balance.OpeningDr + balance.PeriodDr + line.AccountedDr;
            balance.ClosingCr = balance.OpeningCr + balance.PeriodCr + line.AccountedCr;
            balance.PeriodDr += line.AccountedDr;
            balance.PeriodCr += line.AccountedCr;
            balance.LastPostedHeaderId = h.JournalHeaderId;
            balance.LastPostedAt = now;
        }

        var day = h.AccountingDate;
        var daily = await db.Set<GlDailyBalance>().FirstOrDefaultAsync(d => d.LedgerId == line.LedgerId && d.BalanceDate == day
            && d.AccountCombinationId == line.AccountCombinationId && d.CurrencyCode == currency, ct);
        if (daily is null)
        {
            db.Add(new GlDailyBalance
            {
                LedgerId = line.LedgerId,
                LedgerPeriodId = line.LedgerPeriodId,
                BalanceDate = day,
                AccountCombinationId = line.AccountCombinationId,
                CurrencyCode = currency,
                DailyDr = line.AccountedDr,
                DailyCr = line.AccountedCr,
                ClosingDr = line.AccountedDr,
                ClosingCr = line.AccountedCr,
            });
        }
        else
        {
            daily.DailyDr += line.AccountedDr;
            daily.DailyCr += line.AccountedCr;
            daily.ClosingDr += line.AccountedDr;
            daily.ClosingCr += line.AccountedCr;
        }

        db.Add(new GlPostingBalanceImpact
        {
            PostingRunId = runId,
            PostingRunLineId = runLineId,
            JournalBatchId = h.BatchId,
            JournalHeaderId = h.JournalHeaderId,
            JournalLineId = line.JournalLineId,
            LedgerId = line.LedgerId,
            LedgerPeriodId = line.LedgerPeriodId,
            AccountingDate = day,
            AccountCombinationId = line.AccountCombinationId,
            CurrencyCode = currency,
            EnteredDr = line.EnteredDr,
            EnteredCr = line.EnteredCr,
            AccountedDr = line.AccountedDr,
            AccountedCr = line.AccountedCr,
            BalanceBeforeDr = beforeDr,
            BalanceBeforeCr = beforeCr,
            BalanceAfterDr = beforeDr + line.AccountedDr,
            BalanceAfterCr = beforeCr + line.AccountedCr,
            ImpactStatus = "APPLIED",
        });
        await db.SaveChangesAsync(ct);
    }

    /// <summary>update_source_feedback: tells the sub-ledger document behind the journal how its posting went.</summary>
    private async Task SourceFeedbackAsync(long runId, GlJournalHeader h, string status, string? error, CancellationToken ct)
    {
        if (h.SourceModule is null || h.SourceDocumentId is null)
            return;

        var now = clock.UtcNow;
        var rows = await db.Set<GlPostingSourceFeedback>().Where(f => f.JournalHeaderId == h.JournalHeaderId).ToListAsync(ct);
        if (rows.Count == 0)
        {
            rows.Add(new GlPostingSourceFeedback
            {
                JournalHeaderId = h.JournalHeaderId,
                SourceModule = h.SourceModule.Trim().ToUpperInvariant(),
                SourceDocumentType = h.SourceDocumentType?.Trim().ToUpperInvariant(),
                SourceDocumentId = h.SourceDocumentId,
                SourceDocumentNo = h.SourceDocumentNo,
            });
            db.Add(rows[0]);
        }

        foreach (var f in rows)
        {
            f.PostingRunId = runId;
            f.PostingStatus = status;
            f.GlJournalNo = h.JournalNo;
            if (status == "POSTED")
                f.PostedAt = now;
            f.FeedbackStatus = error is null ? "PENDING" : "ERROR";
            f.LastErrorMessage = error;
        }

        await db.SaveChangesAsync(ct);
    }

    /// <summary>
    /// post_journal: checks the journal, then applies every active line to the balances and marks the journal and
    /// batch POSTED. When a check fails it marks the journal POSTING_ERROR and returns without raising, as FINOVA did.
    /// </summary>
    public async Task PostJournalAsync(long journalHeaderId, CancellationToken ct)
    {
        var h = await db.Set<GlJournalHeader>().SingleAsync(x => x.JournalHeaderId == journalHeaderId, ct);
        var oldStatus = h.PostingStatus;
        if (oldStatus == "POSTED")
            throw new ApexProcessException("Journal is already posted.");

        var runId = await CreatePostingRunAsync(h, ct);
        var run = await db.Set<GlPostingRun>().SingleAsync(r => r.PostingRunId == runId, ct);
        run.RunStatus = "RUNNING";
        run.StartedBy = User;
        run.ValidationStatus = "VALIDATING";

        if (await ValidateAsync(runId, h, ct))
        {
            run.RunStatus = "FAILED";
            run.ValidationStatus = "FAILED";
            run.CompletedAt = clock.UtcNow;
            run.ErrorMessage = "Posting validation failed.";
            h.PostingStatus = "POSTING_ERROR";
            h.JournalStatus = "ERROR";
            await db.SaveChangesAsync(ct);
            await SourceFeedbackAsync(runId, h, "ERROR", "Posting validation failed.", ct);
            return;
        }

        run.ValidationStatus = "PASSED";
        var postingLock = await AcquireLockAsync("JOURNAL", journalHeaderId.ToString(CultureInfo.InvariantCulture), h, runId, 120, ct);
        h.PostingStatus = "POSTING";
        h.JournalStatus = "APPROVED";
        var runLine = new GlPostingRunLine { PostingRunId = runId, JournalHeaderId = journalHeaderId, PostingStatus = "PENDING" };
        db.Add(runLine);
        await db.SaveChangesAsync(ct);

        int count = 0;
        decimal dr = 0, cr = 0;
        foreach (var line in await db.Set<GlJournalLine>().AsNoTracking().Where(l => l.JournalHeaderId == journalHeaderId && l.IsActive).OrderBy(l => l.LineNo).ToListAsync(ct))
        {
            await ApplyBalanceAsync(runId, runLine.PostingRunLineId, h, line, ct);
            count++;
            dr += line.AccountedDr;
            cr += line.AccountedCr;
        }

        var now = clock.UtcNow;
        runLine.PostingStatus = "POSTED";
        runLine.ProcessedAt = now;
        h.JournalStatus = "POSTED";
        h.PostingStatus = "POSTED";
        h.PostedBy = User;
        h.PostedAt = now;
        run.TotalHeaders += 1;
        run.PostedHeaders += 1;
        run.TotalLines += count;
        run.TotalAccountedDr += dr;
        run.TotalAccountedCr += cr;
        await db.SaveChangesAsync(ct);

        var batch = await db.Set<GlJournalBatch>().SingleAsync(b => b.BatchId == h.BatchId, ct);
        var allPosted = !await db.Set<GlJournalHeader>().AnyAsync(x => x.BatchId == h.BatchId && x.PostingStatus != "POSTED", ct);
        batch.PostingStatus = allPosted ? "POSTED" : "PARTIALLY_POSTED";
        if (allPosted)
        {
            batch.PostedBy = User;
            batch.PostedAt = now;
        }

        await db.SaveChangesAsync(ct);
        await SourceFeedbackAsync(runId, h, "POSTED", null, ct);
        StatusEvent(runId, h.BatchId, journalHeaderId, oldStatus, "POSTED", "JOURNAL_POSTED", "Journal posted and balances updated.");
        await ReleaseLockAsync(postingLock, "Journal posting completed successfully.", ct);

        if (!await db.Set<GlPostingRunLine>().AnyAsync(l => l.PostingRunId == runId && (l.PostingStatus == "PENDING" || l.PostingStatus == "ERROR"), ct))
        {
            run.RunStatus = run.ErrorHeaders == 0 ? "COMPLETED" : "COMPLETED_WITH_ERRORS";
            run.CompletedAt = clock.UtcNow;
        }

        await db.SaveChangesAsync(ct);
    }
}
