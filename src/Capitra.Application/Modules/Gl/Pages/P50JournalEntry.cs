using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Apex.ApexJson;

namespace Capitra.Application.Modules.Gl.Pages;

/// <summary>App 120 pages 50, 51 and 52 (journals): what the on-demand processes share.</summary>
public abstract class GlJournalPageProcess : IApexProcess
{
    public int App => 120;

    public abstract int Page { get; }

    public abstract string Name { get; }

    public abstract string Permission { get; }

    public abstract Task<object> RunAsync(ApexRequest request, CancellationToken ct);

    /// <summary>The APEX request ran in one transaction that was committed at the end, also after an error the block caught.</summary>
    protected static async Task<object> InTransaction(ICapitraDbContext db, Func<Task<object>> body, CancellationToken ct)
    {
        await using var tx = await db.BeginTransactionAsync(ct);
        var result = await body();
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return result;
    }

    protected static long? Id(string? value) =>
        Text(value) is { } v ? (long)decimal.Parse(v, CultureInfo.InvariantCulture) : null;

    /// <summary>to_char(timestamp, 'DD-Mon-YYYY HH24:MI').</summary>
    protected static string? Ts(DateTime? utc) => GlTime.Stamp(utc, "dd-MMM-yyyy HH:mm");

    protected static string JournalUrl(long id) => ApexPages.Url(120, 50, "P50_ID", id.ToString(CultureInfo.InvariantCulture));

    protected static bool Handled(Exception ex) =>
        ex is ApexProcessException or Common.Exceptions.ConflictException or FormatException or OverflowException or InvalidOperationException;

    /// <summary>The fixed segment order of GL_COA_COMBINATION_PKG.</summary>
    private static readonly string[] SegmentOrder = ["COMPANY", "BRANCH", "COST_CENTER", "NATURAL", "BUSINESS_LINE", "FUTURE1", "FUTURE2"];

    /// <summary>apex_json.get_varchar2('segments.X') etc.: a scalar of the search JSON as text, missing as null.</summary>
    protected static string? Get(System.Text.Json.JsonElement root, params string[] path)
    {
        var e = root;
        foreach (var p in path)
        {
            if (e.ValueKind != System.Text.Json.JsonValueKind.Object || !e.TryGetProperty(p, out e))
                return null;
        }

        return e.ValueKind switch
        {
            System.Text.Json.JsonValueKind.String => e.GetString() is { Length: > 0 } s ? s : null,
            System.Text.Json.JsonValueKind.Number => e.GetRawText(),
            System.Text.Json.JsonValueKind.True => "true",
            System.Text.Json.JsonValueKind.False => "false",
            _ => null,
        };
    }

    /// <summary>The segment values chosen on the page as one regular expression on the account combination
    /// (account = starts with, the others exact); null when none is chosen.</summary>
    protected static string? SegmentRegex(System.Text.Json.JsonElement root, string delim)
    {
        var rx = new System.Text.StringBuilder("^");
        var any = false;
        for (var i = 0; i < SegmentOrder.Length; i++)
        {
            var part = Code(Get(root, "segments", SegmentOrder[i]));
            any |= part is not null;
            if (i > 0)
                rx.Append('\\').Append(delim);
            rx.Append(part is null ? "[^" + delim + "]*" : SegmentOrder[i] == "NATURAL" ? part + "[^" + delim + "]*" : part);
        }

        return any ? rx.Append('$').ToString() : null;
    }

    /// <summary>regexp_replace(upper(trim(p)), '[^A-Z0-9_]', '').</summary>
    private static string? Code(string? value) =>
        Text(value) is { } v ? Text(new string(v.ToUpperInvariant().Where(c => c is (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_').ToArray())) : null;

    /// <summary>The segments of a chart of accounts (with their journal entry default) and up to 400 values each, posting values first.</summary>
    protected static async Task<(object Segments, object Values)> SegmentListsAsync(ICapitraDbContext db, long? coaId, CancellationToken ct)
    {
        var segments = await db.Set<GlCoaSegment>().AsNoTracking().Where(g => g.CoaId == coaId).OrderBy(g => g.SegmentNum).ToListAsync(ct);
        var values = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var g in segments)
        {
            values[g.SegmentCode] = (await db.Set<GlCoaSegmentValue>().Where(v => v.CoaSegmentId == g.CoaSegmentId)
                    .OrderBy(v => v.IsPostingAllowed ? 0 : 1).ThenBy(v => v.ValueCode).Take(400)
                    .Select(v => new { v.ValueCode, v.ValueNameEn, v.IsPostingAllowed }).ToListAsync(ct))
                .Select(v => new[] { v.ValueCode, v.ValueNameEn, Yn(v.IsPostingAllowed) }).ToList();
        }

        return (segments.Select(g =>
        {
            var seg = new Dictionary<string, object?> { ["num"] = g.SegmentNum, ["code"] = g.SegmentCode, ["name"] = g.SegmentNameEn };
            if (g.DefaultValueCode is not null)
            {
                seg["def"] = g.DefaultValueCode;
                seg["defMode"] = g.DefaultMode;
            }

            return seg;
        }).ToList(), values);
    }
}

/// <summary>J50_LOAD: everything page 50 needs to enter a journal, and the journal itself when x01 holds its id.</summary>
public sealed class P50Load(ICapitraDbContext db, GlPostingApi api, IClock clock) : GlJournalPageProcess
{
    public override int Page => 50;

    public override string Name => "J50_LOAD";

    public override string Permission => Permissions.Gl.JournalView;

    public override async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var id = Id(request.X01);
        GlJournalHeader? h = null;
        if (id is not null)
        {
            h = await db.Set<GlJournalHeader>().AsNoTracking().FirstOrDefaultAsync(x => x.JournalHeaderId == id, ct);
            if (h is null)
                return new { ok = false, message = $"Journal {id} does not exist." };
        }

        var today = GlTime.Today(clock);
        var me = api.CurrentUserName;
        var res = new Dictionary<string, object?>
        {
            ["ok"] = true,
            ["me"] = me,
            ["today"] = Ymd(today),
            ["fourEyes"] = GlPostingApi.RequireDifferentApproverYn,
            ["listUrl"] = ApexPages.Url(120, 51),
            ["newUrl"] = ApexPages.Url(120, 50),
        };
        if (id is not null)
            res["selfUrl"] = JournalUrl(id.Value);

        // ledgers that accept manual journals (and the journal's own ledger)
        res["defaultLedger"] = await db.Set<GlLedger>().OrderBy(l => l.IsPrimaryLedger ? 0 : 1).ThenBy(l => l.LedgerId)
            .Select(l => (long?)l.LedgerId).FirstOrDefaultAsync(ct);
        var led = h?.LedgerId;
        res["ledgers"] = await (from l in db.Set<GlLedger>()
                                join c in db.Set<GlCoaStructure>() on l.CoaId equals c.CoaId
                                join o in db.Set<GlLedgerOption>() on l.LedgerId equals o.LedgerId into os
                                from o in os.DefaultIfEmpty()
                                where (o.AllowManualJournals == null ? "Y" : o.AllowManualJournals.Trim()) == "Y" || l.LedgerId == led
                                orderby l.IsPrimaryLedger ? 0 : 1, l.LedgerCode
                                select new { id = l.LedgerId, code = l.LedgerCode, name = l.LedgerNameEn, currency = l.LedgerCurrencyCode, coa = c.CoaCode })
            .ToListAsync(ct);

        // periods of those ledgers (the page resolves the accounting date to a period as you type)
        var since = today.AddMonths(-36);
        var until = today.AddMonths(24);
        var ownPeriod = h?.LedgerPeriodId;
        res["periods"] = (await (from lp in db.Set<GlLedgerPeriod>()
                                 join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
                                 where (p.EndDate >= since && p.StartDate <= until) || lp.LedgerPeriodId == ownPeriod
                                 orderby p.StartDate
                                 select new { lp, p }).ToListAsync(ct))
            .Select(r => new
            {
                id = r.lp.LedgerPeriodId,
                ledgerId = r.lp.LedgerId,
                code = r.p.PeriodCode,
                name = r.p.PeriodNameEn,
                start = Ymd(r.p.StartDate),
                end = Ymd(r.p.EndDate),
                status = r.lp.PeriodStatus,
                mode = r.lp.PostingMode,
                manual = Yn(r.lp.IsManualJournalAllowed),
                adj = Yn(r.p.IsAdjustmentPeriod),
            }).ToList();

        // categories allowed for manual journals
        res["defaultCategory"] = "STANDARD";
        res["categories"] = await db.Set<GlJournalCategory>().Where(c => c.IsActive && c.IsManualAllowed).OrderBy(c => c.CategoryNameEn)
            .Select(c => new { code = c.CategoryCode, name = c.CategoryNameEn }).ToListAsync(ct);

        res["coas"] = await CoasAsync(today, ct);

        if (h is not null)
            res["journal"] = await JournalAsync(h, me, ct);
        return res;
    }

    /// <summary>The segments of each chart of accounts in use and the values each one accepts for posting (accounts: only those that allow manual journals).</summary>
    private async Task<Dictionary<string, object>> CoasAsync(DateOnly today, CancellationToken ct)
    {
        var coas = new Dictionary<string, object>(StringComparer.Ordinal);
        var structures = await db.Set<GlCoaStructure>().Where(s => db.Set<GlLedger>().Any(l => l.CoaId == s.CoaId))
            .Select(s => new { s.CoaId, s.CoaCode, s.DelimiterChar }).ToListAsync(ct);
        var max = new DateOnly(4712, 12, 31);
        foreach (var c in structures)
        {
            var segments = await db.Set<GlCoaSegment>().Where(s => s.CoaId == c.CoaId && s.IsEnabled).OrderBy(s => s.SegmentNum).ToListAsync(ct);
            var values = new Dictionary<string, object>(StringComparer.Ordinal);
            foreach (var s in segments)
            {
                if (s.SegmentCode == "NATURAL")
                {
                    values[s.SegmentCode] = (await (from a in db.Set<GlAccount>()
                                                    join v in db.Set<GlCoaSegmentValue>() on a.AccountCode equals v.ValueCode
                                                    where v.CoaSegmentId == s.CoaSegmentId && a.CoaId == c.CoaId && a.IsActive && a.IsPostingAllowed
                                                          && !a.IsSummary && a.IsManualJournalAllowed && v.IsEnabled && v.IsPostingAllowed
                                                          && v.EffectiveStartDate <= today && (v.EffectiveEndDate ?? max) >= today
                                                    orderby a.AccountCode
                                                    select new { a.AccountCode, a.AccountNameEn, a.IsBranchRequired, a.IsCostCenterRequired, a.IsBusinessLineRequired })
                            .ToListAsync(ct))
                        .Select(a => new[]
                        {
                            a.AccountCode, a.AccountNameEn,
                            Text((a.IsBranchRequired ? "B" : string.Empty) + (a.IsCostCenterRequired ? "C" : string.Empty) + (a.IsBusinessLineRequired ? "L" : string.Empty)),
                        }).ToList();
                }
                else
                {
                    values[s.SegmentCode] = (await db.Set<GlCoaSegmentValue>()
                            .Where(v => v.CoaSegmentId == s.CoaSegmentId && v.IsEnabled && v.IsPostingAllowed
                                        && v.EffectiveStartDate <= today && (v.EffectiveEndDate ?? max) >= today)
                            .OrderBy(v => v.SortOrder).ThenBy(v => v.ValueCode).Select(v => new { v.ValueCode, v.ValueNameEn }).ToListAsync(ct))
                        .Select(v => new[] { v.ValueCode, v.ValueNameEn }).ToList();
                }
            }

            coas[c.CoaCode] = new Dictionary<string, object?>
            {
                ["delimiter"] = c.DelimiterChar ?? "-",
                ["segments"] = segments.Select(s =>
                {
                    var seg = new Dictionary<string, object?>
                    {
                        ["num"] = s.SegmentNum,
                        ["code"] = s.SegmentCode,
                        ["name"] = s.SegmentNameEn,
                        ["type"] = s.SegmentType,
                        ["minLen"] = s.MinLength,
                        ["maxLen"] = s.MaxLength,
                    };
                    if (s.DefaultValueCode is not null && s.SegmentCode != "NATURAL")
                    {
                        seg["def"] = s.DefaultValueCode;
                        seg["defMode"] = s.DefaultMode;
                    }

                    return seg;
                }).ToList(),
                ["values"] = values,
            };
        }

        return coas;
    }

    private async Task<Dictionary<string, object?>> JournalAsync(GlJournalHeader h, string me, CancellationToken ct)
    {
        var id = h.JournalHeaderId;
        var src = await db.Set<GlJournalSource>().Where(s => s.SourceId == h.SourceId).Select(s => s.SourceCode).SingleAsync(ct);
        var cat = await db.Set<GlJournalCategory>().Where(c => c.CategoryId == h.CategoryId).Select(c => c.CategoryCode).FirstOrDefaultAsync(ct);
        var batch = await db.Set<GlJournalBatch>().Where(b => b.BatchId == h.BatchId).Select(b => new { b.BatchNo, b.SourceBatchRef }).SingleAsync(ct);
        var per = await (from lp in db.Set<GlLedgerPeriod>()
                         join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
                         where lp.LedgerPeriodId == h.LedgerPeriodId
                         select p.PeriodCode).FirstOrDefaultAsync(ct);
        var active = db.Set<GlJournalLine>().Where(l => l.JournalHeaderId == id && l.IsActive);
        var lineCount = await active.CountAsync(ct);
        var totalDr = await active.SumAsync(l => (decimal?)l.AccountedDr, ct) ?? 0;
        var edit = src == "MANUAL" && h.PostingStatus is "NOT_POSTED" or "POSTING_ERROR"
                   && h.JournalStatus is not ("POSTED" or "REVERSED" or "CANCELLED")
                   && h.ApprovalStatus is "NOT_SUBMITTED" or "REJECTED" or "RETURNED"
                   && !h.IsReversal;
        var submitter = h.SubmittedBy ?? h.CreatedBy;
        var sameUser = string.Equals(submitter, me, StringComparison.OrdinalIgnoreCase);
        var fourEyes = GlPostingApi.RequireDifferentApprover;

        var j = new Dictionary<string, object?>
        {
            ["id"] = id,
            ["journalNo"] = h.JournalNo,
            ["name"] = h.JournalName,
            ["description"] = h.Description,
            ["referenceNo"] = h.ReferenceNo,
            ["ledgerId"] = h.LedgerId,
            ["accountingDate"] = Ymd(h.AccountingDate),
            ["journalDate"] = Ymd(h.JournalDate),
            ["currency"] = h.CurrencyCode,
            ["categoryCode"] = cat,
            ["sourceCode"] = src,
            ["manual"] = src == "MANUAL",
            ["batchNo"] = batch.BatchNo,
            ["tranSetId"] = batch.SourceBatchRef,
            ["periodCode"] = per,
            ["journalStatus"] = h.JournalStatus,
            ["approvalStatus"] = h.ApprovalStatus,
            ["postingStatus"] = h.PostingStatus,
            ["lineCount"] = lineCount,
            ["totalDr"] = totalDr,
            ["createdBy"] = h.CreatedBy,
            ["createdAt"] = Ts(h.CreatedAt),
            ["submittedBy"] = h.SubmittedBy,
            ["submittedAt"] = Ts(h.SubmittedAt),
            ["approvedBy"] = h.ApprovedBy,
            ["approvedAt"] = Ts(h.ApprovedAt),
            ["postedBy"] = h.PostedBy,
            ["postedAt"] = Ts(h.PostedAt),
            ["sourceDocType"] = h.SourceDocumentType,
            ["sourceDocId"] = h.SourceDocumentId,
            ["sourceDocNo"] = h.SourceDocumentNo,
            ["canEdit"] = edit,
            ["canReject"] = h.ApprovalStatus is "SUBMITTED" or "IN_REVIEW",
            ["canApprove"] = h.ApprovalStatus == "SUBMITTED" && (!fourEyes || !sameUser),
        };
        if (h.ApprovalStatus == "SUBMITTED" && fourEyes && sameUser)
            j["approveBlocked"] = "Y";
        j["canPost"] = h.ApprovalStatus is "APPROVED" or "NOT_REQUIRED" && h.PostingStatus is "READY_TO_POST" or "POSTING_ERROR" or "NOT_POSTED"
                       && h.JournalStatus is not ("CANCELLED" or "REVERSED" or "POSTED");
        j["canCancel"] = h.PostingStatus is not ("POSTED" or "POSTING") && h.JournalStatus is not ("CANCELLED" or "REVERSED" or "POSTED");

        // reversal: link to the original (on a reversal) or to the reversal (on a posted journal)
        if (h.IsReversal && h.ReversedHeaderId is not null)
        {
            var orig = await db.Set<GlJournalHeader>().Where(x => x.JournalHeaderId == h.ReversedHeaderId)
                .Select(x => new { x.JournalHeaderId, x.JournalNo }).FirstOrDefaultAsync(ct);
            if (orig is not null)
                j["reversalOf"] = new { id = orig.JournalHeaderId, no = orig.JournalNo, url = JournalUrl(orig.JournalHeaderId) };

            // a reversal's lines cannot be edited, so it is submitted with its own button
            j["canSubmit"] = h.ApprovalStatus is "NOT_SUBMITTED" or "REJECTED" or "RETURNED" && h.PostingStatus is "NOT_POSTED" or "POSTING_ERROR"
                             && h.JournalStatus is not ("CANCELLED" or "REVERSED" or "POSTED");
        }

        var rev = await db.Set<GlJournalHeader>().Where(x => x.ReversedHeaderId == id && x.JournalStatus != "CANCELLED").OrderBy(x => x.JournalHeaderId)
            .Select(x => new { x.JournalHeaderId, x.JournalNo, x.JournalStatus, x.ApprovalStatus, x.PostingStatus }).FirstOrDefaultAsync(ct);
        if (rev is not null)
        {
            j["reversedBy"] = new
            {
                id = rev.JournalHeaderId,
                no = rev.JournalNo,
                journalStatus = rev.JournalStatus,
                approvalStatus = rev.ApprovalStatus,
                postingStatus = rev.PostingStatus,
                url = JournalUrl(rev.JournalHeaderId),
            };
        }

        j["canReverse"] = rev is null && h.PostingStatus == "POSTED" && !h.IsReversal;

        if (h.PostingStatus != "POSTED" && h.JournalStatus is not ("CANCELLED" or "REVERSED"))
        {
            var err = await db.Set<GlJournalActionHistory>()
                .Where(a => a.JournalHeaderId == id && a.ActionCode == "POST_FAILED" && (h.ApprovedAt == null || a.ActionAt >= h.ApprovedAt))
                .OrderByDescending(a => a.ActionAt).Select(a => new { a.ActionReason }).FirstOrDefaultAsync(ct);
            if (err is not null)
                j["lastPostError"] = err.ActionReason;
        }

        if (h.ApprovalStatus is "REJECTED" or "RETURNED")
        {
            var rej = await db.Set<GlJournalApprovalHistory>().Where(a => a.JournalHeaderId == id && a.ActionCode.StartsWith("REJECT"))
                .OrderByDescending(a => a.ActionAt).Select(a => new { a.ActionNotes }).FirstOrDefaultAsync(ct);
            if (rej is not null)
                j["rejectReason"] = rej.ActionNotes;
        }

        var lines = await (from l in db.Set<GlJournalLine>()
                           join c in db.Set<GlAccountCombination>() on l.AccountCombinationId equals c.AccountCombinationId
                           where l.JournalHeaderId == id && l.IsActive
                           orderby l.LineNo
                           select new { l.AccountCombinationId, l.EnteredDr, l.EnteredCr, l.LineDescription, c.ConcatenatedAccount }).ToListAsync(ct);
        var comboIds = lines.Select(l => l.AccountCombinationId).Distinct().ToList();
        var segs = (await (from cs in db.Set<GlAccountCombinationSegment>()
                           join g in db.Set<GlCoaSegment>() on cs.CoaSegmentId equals g.CoaSegmentId
                           where comboIds.Contains(cs.AccountCombinationId)
                           select new { cs.AccountCombinationId, g.SegmentCode, cs.SegmentValueCode }).ToListAsync(ct))
            .ToLookup(s => s.AccountCombinationId);
        j["lines"] = lines.Select(l => new
        {
            dr = l.EnteredDr,
            cr = l.EnteredCr,
            description = l.LineDescription,
            concat = l.ConcatenatedAccount,
            seg = segs[l.AccountCombinationId].ToDictionary(s => s.SegmentCode, s => s.SegmentValueCode, StringComparer.Ordinal),
        }).ToList();

        // posting result: what each line did to the balance of its account combination (GL_POSTING_BALANCE_IMPACTS)
        j["ledgerUrl"] = ApexPages.Url(120, 52);
        j["impacts"] = (await (from i in db.Set<GlPostingBalanceImpact>()
                               join l in db.Set<GlJournalLine>() on i.JournalLineId equals l.JournalLineId
                               join c in db.Set<GlAccountCombination>() on i.AccountCombinationId equals c.AccountCombinationId
                               join a in db.Set<GlAccount>() on c.AccountId equals a.AccountId into accs
                               from a in accs.DefaultIfEmpty()
                               join b in db.Set<GlAccountBalance>()
                                   on new { i.LedgerId, i.LedgerPeriodId, i.AccountCombinationId, i.CurrencyCode }
                                   equals new { b.LedgerId, b.LedgerPeriodId, b.AccountCombinationId, b.CurrencyCode } into bals
                               from b in bals.DefaultIfEmpty()
                               join lp in db.Set<GlLedgerPeriod>() on i.LedgerPeriodId equals lp.LedgerPeriodId
                               join p in db.Set<GlAccountingPeriod>() on lp.PeriodId equals p.PeriodId
                               where i.JournalHeaderId == id && i.ImpactStatus == "APPLIED"
                               orderby l.LineNo
                               select new
                               {
                                   l.LineNo,
                                   c.ConcatenatedAccount,
                                   AccountCode = (string?)a.AccountCode,
                                   AccountNameEn = (string?)a.AccountNameEn,
                                   i.AccountedDr,
                                   i.AccountedCr,
                                   Before = (i.BalanceBeforeDr ?? 0) - (i.BalanceBeforeCr ?? 0),
                                   After = (i.BalanceAfterDr ?? 0) - (i.BalanceAfterCr ?? 0),
                                   Opening = (b == null ? 0 : b.OpeningDr) - (b == null ? 0 : b.OpeningCr),
                                   p.PeriodCode,
                               }).ToListAsync(ct))
            .Select(r => new
            {
                line = r.LineNo,
                concat = r.ConcatenatedAccount,
                account = r.AccountCode,
                accountName = r.AccountNameEn,
                dr = r.AccountedDr,
                cr = r.AccountedCr,
                before = r.Opening + r.Before,
                after = r.Opening + r.After,
                period = r.PeriodCode,
            }).ToList();

        // open problems: journal checks that failed, and the last posting attempt that failed
        var messages = new List<string?>();
        if (h.JournalStatus == "ERROR" || h.PostingStatus == "POSTING_ERROR")
        {
            var run = await db.Set<GlPostingValidationResult>().Where(r => r.JournalHeaderId == id).MaxAsync(r => r.PostingRunId, ct);
            messages.AddRange(await db.Set<GlPostingValidationResult>()
                .Where(r => r.JournalHeaderId == id && r.ResultStatus == "FAILED" && r.SeverityCode == "ERROR" && r.PostingRunId == run)
                .Select(r => r.ResultMessage).Distinct().ToListAsync(ct));
            var runError = await db.Set<GlPostingRunLine>().Where(r => r.JournalHeaderId == id && r.ErrorMessage != null)
                .OrderByDescending(r => r.PostingRunLineId).Select(r => r.ErrorMessage).FirstOrDefaultAsync(ct);
            if (runError is not null)
                messages.Add(runError);
        }

        if (edit)
            messages.AddRange(await api.OpenValidationMessagesAsync(id, 10, ct));
        j["messages"] = messages;

        // history: approval steps and journal actions, newest first
        var approvals = await db.Set<GlJournalApprovalHistory>().Where(a => a.JournalHeaderId == id)
            .Select(a => new { act = a.ActionCode, st = (string?)null, who = a.ApproverUser, at = a.ActionAt, notes = a.ActionNotes }).ToListAsync(ct);
        var actions = await db.Set<GlJournalActionHistory>().Where(a => a.JournalHeaderId == id && a.JournalLineId == null)
            .Select(a => new { act = a.ActionCode, st = a.NewStatus, who = (string?)a.ActionBy, at = a.ActionAt, notes = a.ActionReason ?? a.ActionNotes }).ToListAsync(ct);
        j["history"] = approvals.Concat(actions).OrderByDescending(r => r.at).Take(40).Select(r => new
        {
            action = r.act,
            status = r.st,
            by = r.who,
            at = Ts(r.at),
            notes = r.notes is { Length: > 400 } n ? n[..400] : (string?)r.notes,
        }).ToList();
        return j;
    }
}

/// <summary>J50_SAVE: the page's JSON (clob01) to save_manual_journal, and its answer back.</summary>
public sealed class P50Save(ICapitraDbContext db, GlPostingApi api) : GlJournalPageProcess
{
    public override int Page => 50;

    public override string Name => "J50_SAVE";

    public override string Permission => Permissions.Gl.JournalEdit;

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        InTransaction(db, async () => await api.SaveManualJournalAsync(request.Clob01, ct), ct);
}

/// <summary>J50_ACTION: submit, approve, reject, post, cancel or reverse one journal (x01 action, x02 id, x03 notes, x04 reversal date).</summary>
public sealed class P50Action(ICapitraDbContext db, GlPostingApi api) : GlJournalPageProcess
{
    public override int Page => 50;

    public override string Name => "J50_ACTION";

    public override string Permission => Permissions.Gl.JournalEdit;

    public override Task<object> RunAsync(ApexRequest request, CancellationToken ct) => InTransaction(db, async () =>
    {
        try
        {
            var act = request.X01?.ToUpperInvariant();
            var id = Id(request.X02) ?? 0;
            var txt = request.X03;
            long? newId = null;
            switch (act)
            {
                case "SUBMIT": await api.SubmitJournalAsync(id, txt, ct); break;
                case "APPROVE": await api.ApproveJournalAsync(id, txt, ct); break;
                case "REJECT": await api.RejectJournalAsync(id, txt, ct); break;
                case "POST": await api.PostJournalAsync(id, ct); break;
                case "CANCEL": await api.CancelJournalAsync(id, txt, ct); break;
                case "REVERSE":
                    var date = Text(request.X04) is { } d ? DateOnly.ParseExact(d, "yyyy-MM-dd", CultureInfo.InvariantCulture) : (DateOnly?)null;
                    newId = await api.ReverseJournalAsync(id, date, txt, ct);
                    break;
                default: throw new ApexProcessException("Unknown action " + act);
            }

            if (newId is null)
                return new { ok = true };
            var no = await db.Set<GlJournalHeader>().Where(x => x.JournalHeaderId == newId).Select(x => x.JournalNo).SingleAsync(ct);
            return (object)new { ok = true, newId, newNo = no };
        }
        catch (Exception ex) when (Handled(ex))
        {
            return new { ok = false, message = GlPostingApi.CleanMessage(ex.Message) };
        }
    }, ct);
}
