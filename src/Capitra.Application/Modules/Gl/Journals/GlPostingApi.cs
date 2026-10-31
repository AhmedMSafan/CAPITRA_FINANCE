using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl.Setup;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Journals;

/// <summary>
/// C# port of FINOVA gl_posting_api_pkg (R2.0.6), the journal API pages 50 and 51 call: save a manual journal,
/// submit, approve, reject, post, cancel and reverse. Each step runs inside a savepoint, so a failed step
/// leaves the journal exactly as it was, like the PL/SQL "rollback to savepoint".
/// </summary>
public sealed partial class GlPostingApi(
    ICapitraDbContext db, GlJournalProcessing processing, GlPostingEngine engine, GlCoaCombination combinations, ICurrentUser currentUser, IClock clock)
{
    /// <summary>c_require_different_approver: the user who submitted a journal cannot approve it.</summary>
    public const bool RequireDifferentApprover = true;

    public string CurrentUserName => currentUser.UserName ?? "SYSTEM";

    public static string RequireDifferentApproverYn => RequireDifferentApprover ? "Y" : "N";

    [GeneratedRegex("^ORA-[0-9]+: ")]
    private static partial Regex OraPrefix();

    /// <summary>clean_message: the first line, without ORA-nnnnn: prefixes.</summary>
    public static string CleanMessage(string? message)
    {
        var l = (message ?? string.Empty).Split('\n')[0];
        while (OraPrefix().IsMatch(l))
            l = OraPrefix().Replace(l, string.Empty, 1);
        return l;
    }

    private static string? Up(string? value) => value?.Trim().ToUpperInvariant();

    private static string Lower(string? value) => (value ?? string.Empty).Replace('_', ' ').ToLowerInvariant();

    private static string InitCap(string value) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.Replace('_', ' ').ToLowerInvariant());

    /// <summary>Runs a step inside a savepoint; an error rolls the step back and is raised again.</summary>
    private async Task InSavepointAsync(string name, Func<Task> step, CancellationToken ct)
    {
        var tx = db.CurrentTransaction ?? throw new InvalidOperationException("The journal API needs an open transaction.");
        await db.SaveChangesAsync(ct);
        await tx.CreateSavepointAsync(name, ct);
        try
        {
            await step();
            await tx.ReleaseSavepointAsync(name, ct);
        }
        catch
        {
            await tx.RollbackToSavepointAsync(name, ct);
            db.DiscardChanges();
            throw;
        }
    }

    private static bool Handled(Exception ex) => ex is ApexProcessException or ConflictException or FormatException or OverflowException;

    // ------------------------------------------------------------------------------------------------ reference data
    private sealed record Ledger(long LedgerId, string LedgerCode, string Currency, string CoaCode, string Delimiter, string AllowManual,
        string ManualAppr = "Y", string AllowSub = "Y", bool AutoPostSub = false);

    private async Task<Ledger> GetLedgerAsync(string? ledgerCode, long? ledgerId, CancellationToken ct)
    {
        var code = Up(ledgerCode);
        var r = await (from l in db.Set<GlLedger>()
                       join c in db.Set<GlCoaStructure>() on l.CoaId equals c.CoaId
                       join o in db.Set<GlLedgerOption>() on l.LedgerId equals o.LedgerId into os
                       from o in os.DefaultIfEmpty()
                       where (ledgerId != null && l.LedgerId == ledgerId)
                             || (ledgerId == null && code != null && l.LedgerCode == code)
                             || (ledgerId == null && code == null && l.IsPrimaryLedger)
                       orderby l.LedgerId
                       select new
                       {
                           l.LedgerId, l.LedgerCode, l.LedgerCurrencyCode, c.CoaCode, c.DelimiterChar, AllowManual = o.AllowManualJournals,
                           ManualAppr = o.RequireManualJournalAppr, AllowSub = o.AllowSubledgerJournals, AutoPostSub = (bool?)o.IsAutoPostSubledger,
                       })
            .FirstOrDefaultAsync(ct);
        if (r is null)
        {
            throw new ApexProcessException(ledgerId is not null ? $"Ledger {ledgerId} does not exist or has no chart of accounts."
                : ledgerCode is not null ? $"Ledger {Up(ledgerCode)} does not exist or has no chart of accounts."
                : "No primary ledger is defined.");
        }

        return new Ledger(r.LedgerId, r.LedgerCode, r.LedgerCurrencyCode, r.CoaCode, r.DelimiterChar ?? "-", r.AllowManual?.Trim() ?? "Y",
            r.ManualAppr?.Trim() ?? "Y", r.AllowSub?.Trim() ?? "Y", r.AutoPostSub ?? false);
    }

    private async Task<GlJournalSource> GetSourceAsync(string code, CancellationToken ct)
    {
        var c = Up(code);
        var s = await db.Set<GlJournalSource>().AsNoTracking().FirstOrDefaultAsync(x => x.SourceCode == c, ct)
            ?? throw new ApexProcessException($"Journal source {c} does not exist.");
        if (!s.IsActive)
            throw new ApexProcessException($"Journal source {s.SourceCode} is not active.");
        return s;
    }

    private async Task<GlJournalCategory> GetCategoryAsync(string? code, CancellationToken ct)
    {
        if (code is null)
            throw new ApexProcessException("The journal category is required.");
        var c = Up(code);
        var cat = await db.Set<GlJournalCategory>().AsNoTracking().FirstOrDefaultAsync(x => x.CategoryCode == c, ct)
            ?? throw new ApexProcessException($"Journal category {c} does not exist.");
        if (!cat.IsActive)
            throw new ApexProcessException($"Journal category {cat.CategoryCode} is not active.");
        return cat;
    }

    private Task<string?> SourceCodeOfAsync(long sourceId, CancellationToken ct) =>
        db.Set<GlJournalSource>().Where(s => s.SourceId == sourceId).Select(s => s.SourceCode).FirstOrDefaultAsync(ct);

    private Task<GlJournalHeader?> HeaderAsync(long id, CancellationToken ct) =>
        db.Set<GlJournalHeader>().AsNoTracking().FirstOrDefaultAsync(h => h.JournalHeaderId == id, ct);

    // ------------------------------------------------------------------------------------------------ periods
    /// <summary>ledger_period_for_date: the regular period of the ledger that covers the date (an adjustment period last).</summary>
    private async Task<long> LedgerPeriodForDateAsync(long ledgerId, DateOnly date, CancellationToken ct) =>
        await (from lp in db.Set<GlLedgerPeriod>()
               join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
               where lp.LedgerId == ledgerId && p.StartDate <= date && p.EndDate >= date
               orderby p.IsAdjustmentPeriod, p.StartDate, lp.LedgerPeriodId
               select (long?)lp.LedgerPeriodId).FirstOrDefaultAsync(ct)
        ?? throw new ApexProcessException("No accounting period of this ledger covers " + date.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture)
                                          + ". Create the period on the Accounting periods page first.");

    /// <summary>check_period: the period must be open (or future entry) and take manual or sub-ledger journals.</summary>
    private async Task CheckPeriodAsync(long ledgerPeriodId, bool manual, CancellationToken ct)
    {
        var r = await (from lp in db.Set<GlLedgerPeriod>()
                       join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
                       where lp.LedgerPeriodId == ledgerPeriodId
                       select new { lp.PeriodStatus, lp.PostingMode, lp.IsManualJournalAllowed, lp.IsSubledgerAllowed, p.PeriodCode }).FirstOrDefaultAsync(ct)
            ?? throw new ApexProcessException($"Ledger period {ledgerPeriodId} does not exist.");
        if (r.PeriodStatus is not ("OPEN" or "FUTURE_ENTRY"))
            throw new ApexProcessException($"Period {r.PeriodCode} is {InitCap(r.PeriodStatus)}. Journals can only be entered and posted in an open period (Accounting periods page).");
        if (r.PostingMode is not ("NORMAL" or "FUTURE_ONLY" or "ADJUSTMENT_ONLY"))
            throw new ApexProcessException($"Period {r.PeriodCode} does not allow posting (posting mode {r.PostingMode}).");
        if (manual && !r.IsManualJournalAllowed)
            throw new ApexProcessException($"Manual journals are not allowed in period {r.PeriodCode}.");
        if (!manual && !r.IsSubledgerAllowed)
            throw new ApexProcessException($"Sub-ledger journals are not allowed in period {r.PeriodCode}.");
    }

    // ------------------------------------------------------------------------------------------------ lines
    private sealed class Line
    {
        public string? Company { get; set; }

        public string? Branch { get; set; }

        public string? CostCenter { get; set; }

        public string? Account { get; set; }

        public string? BusinessLine { get; set; }

        public string? Future1 { get; set; }

        public string? Future2 { get; set; }

        public long CombinationId { get; set; }

        public string? Concat { get; set; }

        public decimal Dr { get; set; }

        public decimal Cr { get; set; }

        public string? Description { get; set; }

        public string? ReferenceNo { get; set; }

        public string? SourceLineId { get; set; }

        public string? SourceLineNo { get; set; }
    }

    /// <summary>jv: a member as trimmed text (numbers as written); missing, null and '' are NULL.</summary>
    private static string? Jv(JsonElement o, string key)
    {
        if (o.ValueKind != JsonValueKind.Object || !o.TryGetProperty(key, out var el))
            return null;
        var text = el.ValueKind switch
        {
            JsonValueKind.Null or JsonValueKind.Undefined => null,
            JsonValueKind.String => el.GetString(),
            _ => el.GetRawText(),
        };
        return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
    }

    private static decimal? ToNum(string? value, string what)
    {
        if (value is null)
            return null;
        return decimal.TryParse(value, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var n)
            ? n : throw new ApexProcessException($"{what} is not a valid number: {value}");
    }

    private static DateOnly? ToDate(string? value, string what)
    {
        if (value is null)
            return null;
        return DateOnly.TryParseExact(value.Length > 10 ? value[..10] : value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d : throw new ApexProcessException($"{what} must be a date in the format YYYY-MM-DD: {value}");
    }

    private static string? Cut(string? value, int length) => value is null || value.Length <= length ? value : value[..length];

    private static Line ParseLine(JsonElement o, string delimiter)
    {
        if (o.ValueKind != JsonValueKind.Object)
            throw new ApexProcessException("The line is not a JSON object.");
        var l = new Line();
        var comb = Jv(o, "combination");
        if (comb is not null)
        {
            var parts = (comb + delimiter).Split(delimiter).SkipLast(1).Select(Up).ToList();
            if (parts.Count != 7)
                throw new ApexProcessException($"Combination {comb} must have 7 values separated by \"{delimiter}\" (company, branch, cost centre, account, business line, future 1, future 2).");
            (l.Company, l.Branch, l.CostCenter, l.Account, l.BusinessLine, l.Future1, l.Future2) =
                (Nz(parts[0]), Nz(parts[1]), Nz(parts[2]), Nz(parts[3]), Nz(parts[4]), Nz(parts[5]), Nz(parts[6]));
        }
        else
        {
            l.Company = Up(Jv(o, "company"));
            l.Branch = Up(Jv(o, "branch"));
            l.CostCenter = Up(Jv(o, "cost_center"));
            l.Account = Up(Jv(o, "account"));
            l.BusinessLine = Up(Jv(o, "business_line"));
            l.Future1 = Up(Jv(o, "future1"));
            l.Future2 = Up(Jv(o, "future2"));
        }

        l.Dr = ToNum(Jv(o, "dr"), "Debit") ?? 0;
        l.Cr = ToNum(Jv(o, "cr"), "Credit") ?? 0;
        l.Description = Cut(Jv(o, "description"), 4000);
        l.ReferenceNo = Cut(Jv(o, "reference_no"), 120);
        l.SourceLineId = Cut(Jv(o, "source_line_id"), 120);
        l.SourceLineNo = Cut(Jv(o, "source_line_no"), 120);
        if (l.Dr < 0 || l.Cr < 0)
            throw new ApexProcessException("Amounts cannot be negative.");
        if (l.Dr > 0 && l.Cr > 0)
            throw new ApexProcessException("A line has either a debit or a credit amount, not both.");
        if (l.Dr == 0 && l.Cr == 0)
            throw new ApexProcessException("The line has no amount.");
        return l;
    }

    private static string? Nz(string? value) => string.IsNullOrEmpty(value) ? null : value;

    /// <summary>apply_defaults: segments the caller left empty take the journal entry default set on page 40.</summary>
    private async Task ApplyDefaultsAsync(Line l, string coaCode, CancellationToken ct)
    {
        if (l.Company is not null && l.Branch is not null && l.CostCenter is not null && l.Account is not null
            && l.BusinessLine is not null && l.Future1 is not null && l.Future2 is not null)
            return;

        var defaults = await (from s in db.Set<GlCoaSegment>()
                              join st in db.Set<GlCoaStructure>() on s.CoaId equals st.CoaId
                              where st.CoaCode == coaCode && s.DefaultValueCode != null
                              select new { s.SegmentCode, s.DefaultValueCode }).ToListAsync(ct);
        foreach (var d in defaults)
        {
            var value = Up(d.DefaultValueCode);
            switch (Up(d.SegmentCode))
            {
                case "COMPANY": l.Company ??= value; break;
                case "BRANCH": l.Branch ??= value; break;
                case "COST_CENTER": l.CostCenter ??= value; break;
                case "NATURAL" or "ACCOUNT": l.Account ??= value; break;
                case "BUSINESS_LINE": l.BusinessLine ??= value; break;
                case "FUTURE1": l.Future1 ??= value; break;
                case "FUTURE2": l.Future2 ??= value; break;
            }
        }
    }

    /// <summary>resolve_line: fills defaults, finds or creates the account combination and checks it can take the posting.</summary>
    private async Task ResolveLineAsync(Line l, string coaCode, string sourceCode, CancellationToken ct)
    {
        await ApplyDefaultsAsync(l, coaCode, ct);
        l.Future1 ??= "000";
        l.Future2 ??= "000";
        var missing = new List<string>();
        if (l.Company is null) missing.Add("company");
        if (l.Branch is null) missing.Add("branch");
        if (l.CostCenter is null) missing.Add("cost centre");
        if (l.Account is null) missing.Add("account");
        if (l.BusinessLine is null) missing.Add("business line");
        if (missing.Count > 0)
            throw new ApexProcessException($"Missing {string.Join(", ", missing)} (no value sent and no journal entry default is set on page 40).");

        var r = await combinations.GetOrCreateAsync(coaCode,
            new GlCoaCombination.Codes(l.Company, l.Branch, l.CostCenter, l.Account, l.BusinessLine, l.Future1, l.Future2),
            sourceCode == "MANUAL", true, ct);
        l.Concat = r.Concat;
        if (r.Id is null)
            throw new ApexProcessException(r.Concat + ": the account combination could not be created.");
        if (r.Status != "VALID")
            throw new ApexProcessException(r.Concat + " is " + r.Status + (r.Message is not null ? ": " + r.Message : string.Empty));
        try
        {
            await combinations.AssertPostingAllowedAsync(r.Id.Value, sourceCode, ct);
        }
        catch (ApexProcessException ex)
        {
            throw new ApexProcessException(r.Concat + ": " + CleanMessage(ex.Message));
        }

        l.CombinationId = r.Id.Value;
    }

    private async Task AddLinesAsync(long journalHeaderId, List<Line> lines, string currency, decimal rate, CancellationToken ct)
    {
        for (var i = 0; i < lines.Count; i++)
        {
            var l = lines[i];
            await processing.AddJournalLineAsync(journalHeaderId, new GlJournalProcessing.LineInput(
                i + 1, l.CombinationId, currency, l.Dr, l.Cr, rate, l.Description, l.ReferenceNo, l.SourceLineId, l.SourceLineNo), ct);
        }
    }

    /// <summary>replace_lines: deletes the draft's lines; a line other records point to is switched off and renumbered instead.</summary>
    private async Task ReplaceLinesAsync(long journalHeaderId, CancellationToken ct)
    {
        var ids = await db.Set<GlJournalLine>().Where(l => l.JournalHeaderId == journalHeaderId && l.IsActive).Select(l => l.JournalLineId).ToListAsync(ct);
        foreach (var id in ids)
        {
            try
            {
                await InSavepointAsync("gl_api_line", async () =>
                {
                    db.Remove(await db.Set<GlJournalLine>().SingleAsync(l => l.JournalLineId == id, ct));
                    await db.SaveChangesAsync(ct);
                }, ct);
            }
            catch (ConflictException)
            {
                var line = await db.Set<GlJournalLine>().SingleAsync(l => l.JournalLineId == id, ct);
                line.IsActive = false;
                line.LineNo = (int)(1000000 + id);
                await db.SaveChangesAsync(ct);
            }
        }
    }

    // ------------------------------------------------------------------------------------------------ workflow
    private async Task SubmitInternalAsync(long id, string? notes, CancellationToken ct)
    {
        var h = await HeaderAsync(id, ct) ?? throw new ApexProcessException($"Journal {id} does not exist.");
        if (h.PostingStatus is "POSTED" or "POSTING" || h.JournalStatus is "POSTED" or "REVERSED" or "CANCELLED")
            throw new ApexProcessException($"Journal {h.JournalNo} is {(h.JournalStatus == "CANCELLED" ? "cancelled" : h.PostingStatus.ToLowerInvariant())} and cannot be submitted.");
        if (h.ApprovalStatus is not ("NOT_SUBMITTED" or "REJECTED" or "RETURNED"))
            throw new ApexProcessException($"Journal {h.JournalNo} is already {Lower(h.ApprovalStatus)}.");

        var lines = db.Set<GlJournalLine>().Where(l => l.JournalHeaderId == id && l.IsActive);
        var n = await lines.CountAsync(ct);
        var dr = await lines.SumAsync(l => l.AccountedDr, ct);
        var cr = await lines.SumAsync(l => l.AccountedCr, ct);
        if (n == 0)
            throw new ApexProcessException($"Journal {h.JournalNo} has no lines.");
        if (Math.Round(dr, 5) != Math.Round(cr, 5) || dr == 0)
            throw new ApexProcessException($"Journal {h.JournalNo} is not balanced: debit {Money(dr)}, credit {Money(cr)}.");

        var source = await SourceCodeOfAsync(h.SourceId, ct);
        await CheckPeriodAsync(h.LedgerPeriodId, source == "MANUAL", ct);
        foreach (var r in await (from l in lines
                                 join c in db.Set<GlAccountCombination>() on l.AccountCombinationId equals c.AccountCombinationId
                                 orderby l.LineNo
                                 select new { l.LineNo, l.AccountCombinationId, c.ConcatenatedAccount }).ToListAsync(ct))
        {
            try
            {
                await combinations.AssertPostingAllowedAsync(r.AccountCombinationId, source, ct);
            }
            catch (ApexProcessException ex)
            {
                throw new ApexProcessException($"Line {r.LineNo} ({r.ConcatenatedAccount}): {CleanMessage(ex.Message)}");
            }
        }

        await processing.SubmitJournalAsync(id, notes, ct);

        h = (await HeaderAsync(id, ct))!;
        if (h.ApprovalStatus != "SUBMITTED")
        {
            var messages = await OpenValidationMessagesAsync(id, 100, ct);
            var msg = Cut(string.Join("; ", messages), 1500);
            throw new ApexProcessException($"Journal {h.JournalNo} could not be submitted"
                + (string.IsNullOrEmpty(msg) ? $" (status {h.JournalStatus})." : ": " + msg));
        }
    }

    /// <summary>The unresolved error results of the journal checks, oldest first.</summary>
    public async Task<List<string>> OpenValidationMessagesAsync(long id, int max, CancellationToken ct) =>
        (await db.Set<GlJournalValidationResult>().Where(r => r.JournalHeaderId == id && r.ResolvedAt == null)
            .OrderBy(r => r.ValidationResultId).Select(r => new { r.Severity, r.ResultStatus, r.ResultMessage }).ToListAsync(ct))
        .Where(r => r.Severity.ToUpperInvariant().StartsWith("ERR", StringComparison.Ordinal) || r.ResultStatus.ToUpperInvariant() is "FAILED" or "FAIL" or "ERROR")
        .Take(max).Select(r => r.ResultMessage).ToList();

    private static string Money(decimal value) => value.ToString("#,##0.00", CultureInfo.InvariantCulture);

    public Task SubmitJournalAsync(long id, string? notes, CancellationToken ct) =>
        InSavepointAsync("gl_api_submit", () => SubmitInternalAsync(id, notes, ct), ct);

    public Task ApproveJournalAsync(long id, string? notes, CancellationToken ct) => InSavepointAsync("gl_api_approve", async () =>
    {
        var h = await HeaderAsync(id, ct) ?? throw new ApexProcessException($"Journal {id} does not exist.");
        if (h.ApprovalStatus != "SUBMITTED")
            throw new ApexProcessException($"Only a submitted journal can be approved. Journal {h.JournalNo} is {Lower(h.JournalStatus)} / approval {Lower(h.ApprovalStatus)}.");
        var submitter = h.SubmittedBy ?? h.CreatedBy;
        if (RequireDifferentApprover && string.Equals(submitter, CurrentUserName, StringComparison.OrdinalIgnoreCase))
            throw new ApexProcessException($"You submitted journal {h.JournalNo}, so it must be approved by another user.");
        await processing.ApproveJournalAsync(id, notes, ct);
        h = (await HeaderAsync(id, ct))!;
        if (h.ApprovalStatus != "APPROVED")
            throw new ApexProcessException($"Journal {h.JournalNo} could not be approved (approval status {h.ApprovalStatus}).");
    }, ct);

    public Task RejectJournalAsync(long id, string? reason, CancellationToken ct) => InSavepointAsync("gl_api_reject", async () =>
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ApexProcessException("Enter the reason for rejecting the journal.");
        var h = await HeaderAsync(id, ct) ?? throw new ApexProcessException($"Journal {id} does not exist.");
        if (h.ApprovalStatus is not ("SUBMITTED" or "IN_REVIEW"))
            throw new ApexProcessException($"Only a submitted journal can be rejected. Journal {h.JournalNo} approval is {Lower(h.ApprovalStatus)}.");
        await processing.RejectJournalAsync(id, reason, ct);
    }, ct);

    private async Task PostInternalAsync(long id, CancellationToken ct)
    {
        var h = await HeaderAsync(id, ct) ?? throw new ApexProcessException($"Journal {id} does not exist.");
        if (h.PostingStatus == "POSTED")
            throw new ApexProcessException($"Journal {h.JournalNo} is already posted.");
        if (h.JournalStatus is "CANCELLED" or "REVERSED")
            throw new ApexProcessException($"Journal {h.JournalNo} is {h.JournalStatus.ToLowerInvariant()}.");
        if (h.ApprovalStatus is not ("APPROVED" or "NOT_REQUIRED"))
            throw new ApexProcessException($"Journal {h.JournalNo} must be approved before it is posted (approval: {Lower(h.ApprovalStatus)}).");
        await CheckPeriodAsync(h.LedgerPeriodId, await SourceCodeOfAsync(h.SourceId, ct) == "MANUAL", ct);

        await engine.PostJournalAsync(id, ct);

        // the engine returns without an error when its pre-post checks fail: read what really happened
        var status = await db.Set<GlJournalHeader>().Where(x => x.JournalHeaderId == id).Select(x => x.PostingStatus).SingleAsync(ct);
        if (status != "POSTED")
        {
            var run = await db.Set<GlPostingValidationResult>().Where(r => r.JournalHeaderId == id).MaxAsync(r => r.PostingRunId, ct);
            var messages = await db.Set<GlPostingValidationResult>()
                .Where(r => r.JournalHeaderId == id && r.PostingRunId == run && r.ResultStatus == "FAILED" && r.SeverityCode == "ERROR")
                .Select(r => r.ResultMessage).Distinct().ToListAsync(ct);
            var msg = Cut(string.Join("; ", messages), 1500);
            throw new ApexProcessException($"Journal {h.JournalNo} was not posted" + (string.IsNullOrEmpty(msg) ? $" (status {status})." : ": " + msg));
        }
    }

    /// <summary>post_journal: on failure everything is undone and the reason is kept in the journal history (POST_FAILED).</summary>
    public async Task PostJournalAsync(long id, CancellationToken ct)
    {
        try
        {
            await InSavepointAsync("gl_api_post", () => PostInternalAsync(id, ct), ct);
        }
        catch (Exception ex) when (Handled(ex))
        {
            var msg = CleanMessage(ex.Message);
            var h = await HeaderAsync(id, ct);
            if (h is not null)
            {
                processing.AddActionHistory(h.BatchId, h.JournalHeaderId, "POST_FAILED", h.PostingStatus, h.PostingStatus, reason: Cut(msg, 4000));
                await db.SaveChangesAsync(ct);
            }

            throw new ApexProcessException(msg);
        }
    }

    public Task CancelJournalAsync(long id, string? reason, CancellationToken ct) => InSavepointAsync("gl_api_cancel", async () =>
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ApexProcessException("Enter the reason for cancelling the journal.");
        var h = await HeaderAsync(id, ct) ?? throw new ApexProcessException($"Journal {id} does not exist.");
        if (h.PostingStatus is "POSTED" or "POSTING" || h.JournalStatus is "POSTED" or "REVERSED")
            throw new ApexProcessException($"Journal {h.JournalNo} is posted. A posted journal is corrected with a reversal, not cancelled.");
        if (h.JournalStatus == "CANCELLED")
            throw new ApexProcessException($"Journal {h.JournalNo} is already cancelled.");
        await processing.CancelJournalAsync(id, reason, ct);
    }, ct);

    /// <summary>reverse_journal: a REV-&lt;journal no&gt; journal on the reversal date with debit and credit switched; returns its id.</summary>
    public async Task<long> ReverseJournalAsync(long id, DateOnly? reversalDate, string? reason, CancellationToken ct)
    {
        long newId = 0;
        await InSavepointAsync("gl_api_reverse", async () =>
        {
            var date = reversalDate ?? GlTime.Today(clock);
            if (string.IsNullOrWhiteSpace(reason))
                throw new ApexProcessException("Enter the reason for the reversal.");
            var h = await HeaderAsync(id, ct) ?? throw new ApexProcessException($"Journal {id} does not exist.");
            if (h.PostingStatus != "POSTED")
                throw new ApexProcessException($"Only a posted journal can be reversed. Journal {h.JournalNo} is {Lower(h.PostingStatus)}; if it is not needed, cancel it instead.");
            if (h.IsReversal)
                throw new ApexProcessException($"Journal {h.JournalNo} is itself a reversal and cannot be reversed again.");
            var other = await db.Set<GlJournalHeader>().AsNoTracking().Where(x => x.ReversedHeaderId == id && x.JournalStatus != "CANCELLED")
                .OrderBy(x => x.JournalHeaderId).FirstOrDefaultAsync(ct);
            if (other is not null)
                throw new ApexProcessException($"Journal {h.JournalNo} already has the reversal {other.JournalNo} ("
                    + Lower(other.PostingStatus == "POSTED" ? "POSTED" : other.ApprovalStatus) + "). Open it from the journal page.");
            var source = await db.Set<GlJournalSource>().AsNoTracking().SingleAsync(s => s.SourceId == h.SourceId, ct);
            if (!source.IsReversalAllowed)
                throw new ApexProcessException($"Source {source.SourceCode} does not allow reversals.");
            var ledgerFlag = await db.Set<GlLedgerOption>().Where(o => o.LedgerId == h.LedgerId).MaxAsync(o => o.AllowReversalJournals, ct);
            if ((ledgerFlag?.Trim() ?? "Y") != "Y")
                throw new ApexProcessException("This ledger does not allow reversal journals.");
            var lp = await LedgerPeriodForDateAsync(h.LedgerId, date, ct);
            await CheckPeriodAsync(lp, source.SourceCode == "MANUAL", ct);
            if (!await db.Set<GlLedgerPeriod>().Where(x => x.LedgerPeriodId == lp).Select(x => x.IsReversalAllowed).SingleAsync(ct))
                throw new ApexProcessException("Reversals are not allowed in the period of " + date.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture) + ".");

            // the reversal number is REV-<journal no>; a cancelled earlier reversal keeps its record under a changed number
            var no = "REV-" + (h.JournalNo.Length > 45 ? h.JournalNo[..45] : h.JournalNo);
            foreach (var old in await db.Set<GlJournalHeader>().Where(x => x.JournalNo == no && x.JournalStatus == "CANCELLED").ToListAsync(ct))
                old.JournalNo = (old.JournalNo.Length > 200 ? old.JournalNo[..200] : old.JournalNo) + "-X" + old.JournalHeaderId.ToString(CultureInfo.InvariantCulture);
            await db.SaveChangesAsync(ct);

            newId = await processing.CreateReversalJournalAsync(id, lp, date, Cut(reason.Trim(), 4000), ct);
            await processing.RecalcJournalAsync(newId, ct);
        }, ct);
        return newId;
    }

    // ------------------------------------------------------------------------------------------------ page 50
    /// <summary>
    /// save_manual_journal: creates or updates a manual journal from the page 50 JSON and, with action SUBMIT, submits it.
    /// Line errors come back together, one per line, and nothing is saved.
    /// </summary>
    public async Task<Dictionary<string, object?>> SaveManualJournalAsync(string? payload, CancellationToken ct)
    {
        var tx = db.CurrentTransaction ?? throw new InvalidOperationException("The journal API needs an open transaction.");
        await tx.CreateSavepointAsync("gl_api_save", ct);
        var res = new Dictionary<string, object?>();
        try
        {
            JsonElement o;
            try
            {
                o = JsonDocument.Parse(payload ?? string.Empty).RootElement.Clone();
            }
            catch (JsonException)
            {
                throw new ApexProcessException("The journal data is not valid JSON.");
            }

            var idValue = ToNum(Jv(o, "id"), "Journal id");
            long? id = idValue is null ? null : (long)idValue.Value;
            var action = Up(Jv(o, "action")) ?? "SAVE";
            var src = await GetSourceAsync("MANUAL", ct);
            if (!src.IsManualAllowed)
                throw new ApexProcessException("The MANUAL journal source does not allow manual journals.");

            GlJournalHeader? h = null;
            Ledger ledger;
            if (id is not null)
            {
                h = await HeaderAsync(id.Value, ct) ?? throw new ApexProcessException($"Journal {id} does not exist.");
                if (h.SourceId != src.SourceId)
                    throw new ApexProcessException($"Journal {h.JournalNo} comes from source {await SourceCodeOfAsync(h.SourceId, ct)} and cannot be edited here.");
                if (h.IsReversal)
                    throw new ApexProcessException($"Journal {h.JournalNo} is a reversal: its lines are the original lines with debit and credit switched and cannot be changed.");
                if (h.PostingStatus is not ("NOT_POSTED" or "POSTING_ERROR") || h.JournalStatus is "POSTED" or "REVERSED" or "CANCELLED"
                    || h.ApprovalStatus is not ("NOT_SUBMITTED" or "REJECTED" or "RETURNED"))
                    throw new ApexProcessException($"Journal {h.JournalNo} is {Lower(h.JournalStatus)} / approval {Lower(h.ApprovalStatus)} and can no longer be changed.");
                ledger = await GetLedgerAsync(null, h.LedgerId, ct);
            }
            else
            {
                var ledgerId = ToNum(Jv(o, "ledger_id"), "Ledger");
                ledger = await GetLedgerAsync(Jv(o, "ledger_code"), ledgerId is null ? null : (long)ledgerId.Value, ct);
            }

            if (ledger.AllowManual != "Y")
                throw new ApexProcessException($"Ledger {ledger.LedgerCode} does not allow manual journals.");

            var cat = await GetCategoryAsync(Jv(o, "category"), ct);
            if (!cat.IsManualAllowed)
                throw new ApexProcessException($"Category {cat.CategoryCode} cannot be used for manual journals.");
            var date = ToDate(Jv(o, "accounting_date"), "Accounting date") ?? throw new ApexProcessException("The accounting date is required.");
            var journalDate = ToDate(Jv(o, "journal_date"), "Journal date") ?? date;
            var name = Cut(Jv(o, "name"), 240) ?? throw new ApexProcessException("The journal name is required.");
            var description = Cut(Jv(o, "description"), 4000);
            var reference = Cut(Jv(o, "reference_no"), 120);
            var lp = await LedgerPeriodForDateAsync(ledger.LedgerId, date, ct);
            await CheckPeriodAsync(lp, true, ct);

            var lines = new List<Line>();
            var errors = new List<object>();
            if (o.TryGetProperty("lines", out var arr) && arr.ValueKind == JsonValueKind.Array)
            {
                var i = 0;
                foreach (var el in arr.EnumerateArray())
                {
                    i++;
                    try
                    {
                        Line? line = null;
                        await InSavepointAsync("gl_api_line", async () =>
                        {
                            line = ParseLine(el, ledger.Delimiter);
                            await ResolveLineAsync(line, ledger.CoaCode, "MANUAL", ct);
                        }, ct);
                        lines.Add(line!);
                    }
                    catch (Exception ex) when (Handled(ex))
                    {
                        errors.Add(new { line = i, message = CleanMessage(ex.Message) });
                    }
                }
            }

            if (errors.Count > 0)
            {
                await tx.RollbackToSavepointAsync("gl_api_save", ct);
                db.DiscardChanges();
                res["ok"] = false;
                res["message"] = "The journal was not saved. Correct the lines marked in red.";
                res["errors"] = errors;
                return res;
            }

            if (h is null)
            {
                var batchId = await processing.CreateBatchAsync(ledger.LedgerId, lp, "MANUAL", cat.CategoryCode, name, date, description, null, ct);
                id = await processing.CreateJournalAsync(batchId, name, journalDate, date, ledger.Currency, 1, description, reference, false, ct);
            }
            else
            {
                var others = await db.Set<GlJournalHeader>().CountAsync(x => x.BatchId == h.BatchId && x.JournalHeaderId != h.JournalHeaderId, ct);
                if ((lp != h.LedgerPeriodId || cat.CategoryId != h.CategoryId) && others > 0)
                    throw new ApexProcessException($"Journal {h.JournalNo} shares its batch with other journals, so its period and category cannot change.");
                if (others == 0)
                {
                    var batch = await db.Set<GlJournalBatch>().SingleAsync(b => b.BatchId == h.BatchId, ct);
                    batch.LedgerPeriodId = lp;
                    batch.CategoryId = cat.CategoryId;
                    batch.BatchName = name;
                    batch.BatchDate = date;
                    batch.Description = description;
                }

                var header = await db.Set<GlJournalHeader>().SingleAsync(x => x.JournalHeaderId == h.JournalHeaderId, ct);
                header.JournalName = name;
                header.Description = description;
                header.ReferenceNo = reference;
                header.JournalDate = journalDate;
                header.AccountingDate = date;
                header.LedgerPeriodId = lp;
                header.CategoryId = cat.CategoryId;
                await db.SaveChangesAsync(ct);
                await ReplaceLinesAsync(h.JournalHeaderId, ct);
            }

            await AddLinesAsync(id!.Value, lines, ledger.Currency, 1, ct);
            await processing.RecalcJournalAsync(id.Value, ct);

            res["ok"] = true;
            res["id"] = id;
            res["journalNo"] = await db.Set<GlJournalHeader>().Where(x => x.JournalHeaderId == id).Select(x => x.JournalNo).SingleAsync(ct);
            await tx.ReleaseSavepointAsync("gl_api_save", ct);

            if (action == "SUBMIT")
            {
                try
                {
                    // rolls itself back on failure: the draft stays saved
                    await SubmitJournalAsync(id.Value, Jv(o, "notes"), ct);
                    res["submitted"] = true;
                }
                catch (Exception ex) when (Handled(ex))
                {
                    res["submitted"] = false;
                    res["submitError"] = CleanMessage(ex.Message);
                }
            }

            return res;
        }
        catch (Exception ex) when (Handled(ex))
        {
            await tx.RollbackToSavepointAsync("gl_api_save", ct);
            db.DiscardChanges();
            return new Dictionary<string, object?> { ["ok"] = false, ["message"] = CleanMessage(ex.Message) };
        }
    }
}
