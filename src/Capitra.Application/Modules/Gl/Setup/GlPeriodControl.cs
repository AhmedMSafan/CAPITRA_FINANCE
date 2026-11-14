using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Setup;

/// <summary>
/// C# port of FINOVA gl_period_control_pkg: open, set to future entry, close, reopen and permanently close a ledger period,
/// with the close checks and a status history row for every change.
/// </summary>
public sealed class GlPeriodControl(ICapitraDbContext db, GlPeriodSetup setup, ICurrentUser currentUser, IClock clock)
{
    /// <summary>Where an action came from (p_source_app_id, p_source_page_id, p_source_session_id).</summary>
    public sealed record ActionSource(long? AppId, long? PageId, string? SessionId);

    private string User => currentUser.UserName ?? "SYSTEM";

    private async Task<GlLedgerPeriod> LedgerPeriodAsync(long ledgerPeriodId, CancellationToken ct) =>
        await db.Set<GlLedgerPeriod>().SingleOrDefaultAsync(lp => lp.LedgerPeriodId == ledgerPeriodId, ct)
        ?? throw new ApexProcessException("No data found.");

    private async Task<bool> IsAdjustmentAsync(GlLedgerPeriod lp, CancellationToken ct) =>
        await db.Set<GlAccountingPeriod>().Where(p => p.PeriodId == lp.PeriodId).Select(p => p.IsAdjustmentPeriod).SingleAsync(ct);

    private static void Allow(GlLedgerPeriod lp, string status, string mode, bool manual, bool subledger, bool reversal, bool adjustment)
    {
        lp.PeriodStatus = status;
        lp.PostingMode = mode;
        lp.IsManualJournalAllowed = manual;
        lp.IsSubledgerAllowed = subledger;
        lp.IsReversalAllowed = reversal;
        lp.IsAdjustmentAllowed = adjustment;
    }

    private void WriteHistory(long ledgerPeriodId, string? oldStatus, string newStatus, string actionCode, string? reason, ActionSource source) =>
        db.Add(new GlPeriodStatusHistory
        {
            LedgerPeriodId = ledgerPeriodId,
            OldPeriodStatus = oldStatus,
            NewPeriodStatus = newStatus,
            ActionCode = actionCode,
            ActionReason = reason,
            ActionBy = User,
            ActionAt = clock.UtcNow,
            SourceAppId = source.AppId,
            SourcePageId = source.PageId,
            SourceSessionId = source.SessionId,
        });

    public async Task SetFutureEntryAsync(long ledgerPeriodId, string? reason, ActionSource source, CancellationToken ct)
    {
        var lp = await LedgerPeriodAsync(ledgerPeriodId, ct);
        var old = lp.PeriodStatus;
        if (old == "PERMANENTLY_CLOSED")
            throw new ApexProcessException("Permanently closed periods cannot be changed.");

        Allow(lp, "FUTURE_ENTRY", "FUTURE_ONLY", false, false, false, false);
        lp.CloseNotes = reason;
        WriteHistory(ledgerPeriodId, old, "FUTURE_ENTRY", "SET_FUTURE_ENTRY", reason, source);
        await db.SaveChangesAsync(ct);
    }

    public async Task OpenPeriodAsync(long ledgerPeriodId, string? reason, ActionSource source, CancellationToken ct)
    {
        var lp = await LedgerPeriodAsync(ledgerPeriodId, ct);
        var adj = await IsAdjustmentAsync(lp, ct);
        var old = lp.PeriodStatus;
        if (old == "PERMANENTLY_CLOSED")
            throw new ApexProcessException("Permanently closed periods cannot be opened.");
        if (old == "CLOSED")
            throw new ApexProcessException("Closed periods must be reopened using REOPEN_PERIOD.");

        Allow(lp, "OPEN", adj ? "ADJUSTMENT_ONLY" : "NORMAL", true, !adj, true, adj);
        lp.OpenedBy = User;
        lp.OpenedAt = clock.UtcNow;
        lp.CloseNotes = null;
        await db.SaveChangesAsync(ct);

        await setup.SyncSourceStatusAsync(ledgerPeriodId, ct);
        await setup.SyncCloseChecklistAsync(ledgerPeriodId, ct);
        WriteHistory(ledgerPeriodId, old, "OPEN", "OPEN", reason, source);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>Source statuses with work left plus required checklist tasks not completed or waived.</summary>
    public async Task<int> GetCloseBlockerCountAsync(long ledgerPeriodId, CancellationToken ct)
    {
        string[] done = ["CLOSED", "WAIVED"];
        string[] finished = ["COMPLETED", "WAIVED"];
        var sources = await db.Set<GlPeriodSourceStatus>().CountAsync(s => s.LedgerPeriodId == ledgerPeriodId
            && !done.Contains(s.SourceStatus)
            && (s.UntransferredCount > 0 || s.UnpostedCount > 0 || s.ErrorCount > 0 || s.TransferStatus == "ERROR"), ct);
        var tasks = await db.Set<GlPeriodCloseChecklist>().CountAsync(c => c.LedgerPeriodId == ledgerPeriodId
            && c.IsRequired && !finished.Contains(c.TaskStatus), ct);
        return sources + tasks;
    }

    /// <summary>can_close_period: 'Y' or 'N', and the message that says why.</summary>
    public async Task<(string Can, string Message)> CanClosePeriodAsync(long ledgerPeriodId, CancellationToken ct)
    {
        var status = await db.Set<GlLedgerPeriod>().Where(lp => lp.LedgerPeriodId == ledgerPeriodId).Select(lp => lp.PeriodStatus).SingleOrDefaultAsync(ct);
        if (status is null)
            return ("N", "Ledger period not found.");
        if (status != "OPEN")
            return ("N", $"Only open periods can be closed. Current status is {status}.");

        var blockers = await GetCloseBlockerCountAsync(ledgerPeriodId, ct);
        return blockers > 0 ? ("N", $"Period cannot be closed. Blocking items count: {blockers}.") : ("Y", "Period is ready to close.");
    }

    public async Task ClosePeriodAsync(long ledgerPeriodId, string? reason, ActionSource source, CancellationToken ct)
    {
        var lp = await LedgerPeriodAsync(ledgerPeriodId, ct);
        var old = lp.PeriodStatus;
        var (can, message) = await CanClosePeriodAsync(ledgerPeriodId, ct);
        if (can != "Y")
            throw new ApexProcessException(message);

        Allow(lp, "CLOSED", "NONE", false, false, false, false);
        lp.ClosedBy = User;
        lp.ClosedAt = clock.UtcNow;
        lp.CloseNotes = reason;
        WriteHistory(ledgerPeriodId, old, "CLOSED", "CLOSE", reason, source);
        await db.SaveChangesAsync(ct);
    }

    public async Task ReopenPeriodAsync(long ledgerPeriodId, string? reason, ActionSource source, CancellationToken ct)
    {
        if (reason is null)
            throw new ApexProcessException("Reopen reason is required.");

        var lp = await LedgerPeriodAsync(ledgerPeriodId, ct);
        var adj = await IsAdjustmentAsync(lp, ct);
        var old = lp.PeriodStatus;
        if (old != "CLOSED")
            throw new ApexProcessException($"Only closed periods can be reopened. Current status is {old}.");

        Allow(lp, "OPEN", adj ? "ADJUSTMENT_ONLY" : "NORMAL", true, !adj, true, true);
        lp.CloseNotes = reason;
        WriteHistory(ledgerPeriodId, old, "OPEN", "REOPEN", reason, source);
        await db.SaveChangesAsync(ct);
    }

    public async Task PermanentlyClosePeriodAsync(long ledgerPeriodId, string? reason, ActionSource source, CancellationToken ct)
    {
        if (reason is null)
            throw new ApexProcessException("Permanent close reason is required.");

        var lp = await LedgerPeriodAsync(ledgerPeriodId, ct);
        var old = lp.PeriodStatus;
        if (old != "CLOSED")
            throw new ApexProcessException($"Only closed periods can be permanently closed. Current status is {old}.");

        Allow(lp, "PERMANENTLY_CLOSED", "NONE", false, false, false, false);
        lp.PermanentlyClosedBy = User;
        lp.PermanentlyClosedAt = clock.UtcNow;
        lp.CloseNotes = reason;
        WriteHistory(ledgerPeriodId, old, "PERMANENTLY_CLOSED", "PERMANENT_CLOSE", reason, source);
        await db.SaveChangesAsync(ct);
    }
}
