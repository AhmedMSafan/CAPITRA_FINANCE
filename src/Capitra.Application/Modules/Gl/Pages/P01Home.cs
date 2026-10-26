using System.Globalization;
using System.Net;
using System.Text;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Pages;

/// <summary>
/// App 120 page 1 "Home": the PL/SQL dynamic content region, ported. It renders the same HTML,
/// with the same counts and the periods of the current fiscal year.
/// </summary>
public sealed class P01Home(ICapitraDbContext db, ICurrentUser currentUser, IClock clock) : IApexProcess
{
    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    public int App => 120;

    public int Page => 1;

    public string Name => "REGION_HOME";

    public string Permission => Permissions.Gl.SetupView;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var accounts = db.Set<GlAccount>().AsNoTracking();
        var journals = db.Set<GlJournalHeader>().AsNoTracking();
        var acc = await accounts.CountAsync(ct);
        var accActive = await accounts.CountAsync(a => a.IsActive, ct);
        var accPosting = await accounts.CountAsync(a => a.IsPostingAllowed && a.IsActive, ct);
        var accSummary = await accounts.CountAsync(a => a.IsSummary, ct);
        var ledgers = await db.Set<GlLedger>().CountAsync(ct);
        var openPeriods = await db.Set<GlLedgerPeriod>().CountAsync(p => p.PeriodStatus == "OPEN", ct);
        var segments = await db.Set<GlCoaSegment>().CountAsync(ct);
        string[] waitingStates = ["SUBMITTED", "IN_REVIEW"];
        string[] readyStates = ["APPROVED", "NOT_REQUIRED"];
        string[] draftStates = ["NOT_SUBMITTED", "REJECTED", "RETURNED"];
        string[] closedStates = ["CANCELLED", "REVERSED"];
        var waiting = await journals.CountAsync(j => waitingStates.Contains(j.ApprovalStatus) && j.JournalStatus != "CANCELLED", ct);
        var ready = await journals.CountAsync(j => readyStates.Contains(j.ApprovalStatus) && j.PostingStatus != "POSTED" && !closedStates.Contains(j.JournalStatus), ct);
        var drafts = await journals.CountAsync(j => draftStates.Contains(j.ApprovalStatus) && j.PostingStatus != "POSTED" && !closedStates.Contains(j.JournalStatus), ct);

        // The period that covers today (trunc(sysdate), Cairo time), and how many ledgers have it open.
        var today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, Cairo));
        var current = await db.Set<GlAccountingPeriod>().AsNoTracking()
            .Where(p => p.StartDate <= today && p.EndDate >= today && !p.IsAdjustmentPeriod)
            .OrderBy(p => p.CalendarId).FirstOrDefaultAsync(ct);
        int curLedgers = 0, curOpen = 0;
        List<GlAccountingPeriod> yearPeriods = [];
        if (current is not null)
        {
            var states = await db.Set<GlLedgerPeriod>().Where(p => p.PeriodId == current.PeriodId).Select(p => p.PeriodStatus).ToListAsync(ct);
            curLedgers = states.Count;
            curOpen = states.Count(s => s == "OPEN");
            yearPeriods = await db.Set<GlAccountingPeriod>().AsNoTracking().Where(p => p.FiscalYearId == current.FiscalYearId)
                .OrderBy(p => p.PeriodNo).ThenBy(p => p.StartDate).ToListAsync(ct);
        }

        var user = InitCap((currentUser.UserName ?? string.Empty).ToLowerInvariant().Replace('_', ' '));

        string CurOpen() => curLedgers > 0 ? $" &middot; <small>open in {curOpen} of {curLedgers} ledgers</small>" : string.Empty;

        string PeriodsStrip()
        {
            if (yearPeriods.Count == 0)
                return string.Empty;
            var s = new StringBuilder("<div class=\"wx-sec\"><h2>Periods of the current fiscal year</h2></div><div class=\"ah-strip\">\n");
            foreach (var p in yearPeriods)
            {
                var css = (p.PeriodId == current!.PeriodId ? " now" : string.Empty) + (p.IsAdjustmentPeriod ? " adj" : string.Empty);
                s.Append(CultureInfo.InvariantCulture, $"<div class=\"ah-p{css}\"><code>{E(p.PeriodCode)}</code><small>{Day(p.StartDate)} &ndash; {Day(p.EndDate)}</small>{Pill(p.PeriodStatus)}</div>\n");
            }

            return s.Append("</div>").ToString();
        }

        var h = new StringBuilder();
        h.AppendLine($$"""
        <div class="wx wh">
        """);
        h.AppendLine($$"""
        <section class="wx-hero wh-hero"><div>
        """);
        h.AppendLine($$"""
        <span class="wx-eyebrow"><i></i>FINOVA General Ledger</span>
        """);
        h.AppendLine($$"""
        <h1><span id="whGreet">Welcome back</span>, <em>{{E(user)}}</em></h1>
        """);
        h.AppendLine($$"""
        <p>Maintain the chart of accounts, control accounting periods and follow every entry the FINOVA modules post to the ledger.</p>
        """);
        h.AppendLine($$"""
        <div class="wh-cta"><a class="wx-btn wx-btn--hot" href="{{U(2)}}"><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5z"/><path d="M4 19.5A2.5 2.5 0 0 0 6.5 22H20v-5"/><path d="M9 7h7M9 11h5"/></svg><span>Chart of accounts</span></a><a class="wx-btn wx-btn--ghost" href="{{U(30)}}"><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="4" width="18" height="17" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/></svg><span>Accounting periods</span></a><button type="button" class="wx-btn wx-btn--ghost" data-flow><svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="6" cy="19" r="3"/><circle cx="18" cy="5" r="3"/><path d="M9 19h6a3 3 0 0 0 3-3V8"/></svg><span>How journals work</span></button></div>
        """);
        h.AppendLine($$"""
        </div><div class="wh-kpis" role="list">
        """);
        h.AppendLine($$"""
        <div class="wh-kpi" role="listitem"><b>{{Fmt(acc)}}</b><span>GL accounts &middot; <small>{{Fmt(accActive)}} active</small></span></div>
        """);
        h.AppendLine($$"""
        <div class="wh-kpi" role="listitem"><b>{{Fmt(accPosting)}}</b><span>Posting accounts</span></div>
        """);
        h.AppendLine($$"""
        <div class="wh-kpi" role="listitem"><b>{{(current is null ? "&ndash;" : E(current.PeriodCode))}}</b><span>Current period{{CurOpen()}}</span></div>
        """);
        h.AppendLine($$"""
        <div class="wh-kpi" role="listitem"><b>{{Fmt(openPeriods)}}</b><span>Open ledger periods &middot; <small>{{Fmt(ledgers)}} ledgers</small></span></div>
        """);
        h.AppendLine($$"""
        </div></section>
        """);
        h.AppendLine($$"""
        <div class="wx-sec"><h2>General ledger setup</h2></div><div class="wh-grid wh-grid--2">
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card wh-feat" href="{{U(2)}}" style="--c:#00636D;--cl:#E6F3F4"><div class="wh-body">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20V3H6.5A2.5 2.5 0 0 0 4 5.5z"/><path d="M4 19.5A2.5 2.5 0 0 0 6.5 22H20v-5"/><path d="M9 7h7M9 11h5"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Chart of accounts</h3><p>The GL account hierarchy: assets, liabilities, equity, income and expenses, with posting and summary accounts.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-stats"><div><b>{{Fmt(acc)}}</b><span>Accounts</span></div><div><b>{{Fmt(accActive)}}</b><span>Active</span></div><div><b>{{Fmt(accPosting)}}</b><span>Posting</span></div><div><b>{{Fmt(accSummary)}}</b><span>Summary</span></div></div></div>
        """);
        h.AppendLine($$"""
        <div class="wh-mini ah-tree"><svg viewBox="0 0 300 130" aria-hidden="true"><g fill="none" stroke="#9FBFC5" stroke-width="1.4"><path d="M28 30 V112"/><path d="M28 56 H48"/><path d="M28 84 H48"/><path d="M28 112 H48"/><path d="M66 70 V98"/><path d="M66 98 H84"/></g><rect x="12" y="12" width="120" height="26" rx="7" fill="#fff" stroke="#B9CBD1"/><rect x="20" y="19" width="12" height="12" rx="3" fill="#E87722"/><rect x="38" y="21" width="60" height="7" rx="3.5" fill="#CFE3E6"/><rect x="48" y="44" width="110" height="24" rx="7" fill="#fff" stroke="#B9CBD1"/><rect x="56" y="53" width="56" height="6" rx="3" fill="#CFE3E6"/><rect x="120" y="53" width="28" height="6" rx="3" fill="#E6F3F4"/><rect x="48" y="72" width="110" height="24" rx="7" fill="#E6F3F4" stroke="#00636D"/><rect x="56" y="81" width="48" height="6" rx="3" fill="#00636D" opacity=".55"/><rect x="120" y="81" width="28" height="6" rx="3" fill="#fff"/><rect x="84" y="88" width="0" height="0"/><rect x="84" y="100" width="0" height="0"/><rect x="48" y="100" width="110" height="24" rx="7" fill="#fff" stroke="#B9CBD1"/><rect x="56" y="109" width="62" height="6" rx="3" fill="#CDEBDD"/><rect x="186" y="26" width="102" height="92" rx="10" fill="#fff" stroke="#B9CBD1"/><rect x="196" y="38" width="50" height="7" rx="3.5" fill="#0B2533" opacity=".75"/><rect x="196" y="54" width="82" height="6" rx="3" fill="#E6F3F4"/><rect x="196" y="66" width="70" height="6" rx="3" fill="#E6F3F4"/><rect x="196" y="78" width="76" height="6" rx="3" fill="#E6F3F4"/><rect x="196" y="96" width="38" height="14" rx="7" fill="#E7F6EF"/><rect x="240" y="96" width="38" height="14" rx="7" fill="#FFF1E6"/></svg></div></a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card wh-new" href="{{U(30)}}" style="--c:#E87722;--cl:#FFF1E6">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="4" width="18" height="17" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Accounting periods</h3><p>Calendars, fiscal years and monthly periods, generated and synchronised to every ledger.</p></div>
        """);
        h.AppendLine($$"""
        <div class="ah-cur"><small>Current period</small><b>{{(current is null ? "No period covers today" : E(current.PeriodNameEn))}}</b>{{(current is null ? "" : Pill(current.PeriodStatus))}}</div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        </div>
        """);
        h.AppendLine($$"""
        <div class="wx-sec"><h2>Journals &amp; ledger</h2><button type="button" class="gf-open" data-flow style="font-size:13px;margin-left:4px">How a journal moves</button></div><div class="wh-grid">
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(50)}}" style="--c:#E87722;--cl:#FFF1E6">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M13 2 3 14h9l-1 8 10-12h-9l1-8z"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Journal entry</h3><p>Create a journal, or open a draft to finish it and submit it.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><b>{{Fmt(drafts)}}</b><span>drafts not submitted yet</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(51)}}#SUBMITTED" style="--c:#00636D;--cl:#E6F3F4">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="11" cy="11" r="7"/><path d="m20 20-3.5-3.5"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Inbox: waiting approval</h3><p>For approvers: the journals to open and approve.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><b>{{Fmt(waiting)}}</b><span>waiting for approval</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(51)}}#APPROVED" style="--c:#14734F;--cl:#E7F6EF">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Inbox: ready to post</h3><p>Approved journals to open and post to the ledger.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><b>{{Fmt(ready)}}</b><span>ready to post</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(52)}}" style="--c:#00636D;--cl:#E6F3F4">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 3v18M5 7h14M5 7l-3 7a4 4 0 0 0 6 0zM19 7l-3 7a4 4 0 0 0 6 0z"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Account ledger</h3><p>Search posted movements by TRAN_SET_ID or account, with balances before and after.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>By account or TRAN_SET_ID</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        </div>
        """);
        h.AppendLine($$"""
        {{PeriodsStrip()}}
        """);
        h.AppendLine($$"""
        <div class="wx-sec"><h2>Reports &amp; sub-ledger interface</h2></div><div class="wh-grid">
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(40)}}" style="--c:#5B5BD6;--cl:#EEEEFC">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="4" width="5" height="16" rx="1"/><rect x="10" y="4" width="5" height="16" rx="1"/><rect x="17" y="4" width="4" height="16" rx="1"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Account segments</h3><p>Account structure, segments and their values.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><b>{{Fmt(segments)}}</b><span>segments</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(56)}}" style="--c:#14734F;--cl:#E7F6EF">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M3 3v18h18"/><path d="M7 15l4-4 3 3 5-6"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Financial statements</h3><p>Trial balance, balance sheet, income statement and cash flow for any combination of segments.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>By company, branch, cost centre or business line</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(20)}}" style="--c:#5B5BD6;--cl:#EEEEFC">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M12 3v18M5 7h14M5 7l-3 7a4 4 0 0 0 6 0zM19 7l-3 7a4 4 0 0 0 6 0z"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Accounts balance</h3><p>Balances by GL account.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>Balance summary</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(620)}}" style="--c:#00636D;--cl:#E6F3F4">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M8 6h13M8 12h13M8 18h13M3 6h.01M3 12h.01M3 18h.01"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Generated GL entries</h3><p>Entries generated from sub-ledger events.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>Auto generated</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(730)}}" style="--c:#14734F;--cl:#E7F6EF">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="12" cy="12" r="10"/><path d="M12 6v6l4 2"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Interest servicing</h3><p>End of day and end of month control.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>EOD / EOM</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(600)}}" style="--c:#E87722;--cl:#FFF1E6">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M21 8 12 3 3 8l9 5 9-5z"/><path d="M3 8v8l9 5 9-5V8"/><path d="M12 13v8"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Sources &amp; events</h3><p>Accounting event catalogue of every sub-ledger.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>Event catalogue</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(605)}}" style="--c:#E87722;--cl:#FFF1E6">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M13 2 3 14h9l-1 8 10-12h-9l1-8z"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Accounting events</h3><p>Events raised by the FINOVA modules.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>Events</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(610)}}" style="--c:#5B5BD6;--cl:#EEEEFC">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><rect x="3" y="4" width="18" height="16" rx="2"/><path d="M3 10h18M9 10v10"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Source values</h3><p>Values an accounting rule can read.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>Definitions</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        <a class="wx-card wh-card" href="{{U(615)}}" style="--c:#14734F;--cl:#E7F6EF">
        """);
        h.AppendLine($$"""
        <div class="wh-top"><span class="wh-ico"><svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><circle cx="6" cy="19" r="3"/><circle cx="18" cy="5" r="3"/><path d="M9 19h6a3 3 0 0 0 3-3V8"/></svg></span><span class="wh-go"><svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true"><path d="M5 12h14M13 5l7 7-7 7"/></svg></span></div>
        """);
        h.AppendLine($$"""
        <div><h3>Accounting entries designer</h3><p>Rule sets that turn events into entries.</p></div>
        """);
        h.AppendLine($$"""
        <div class="wh-foot"><span>Rule sets</span></div>
        """);
        h.AppendLine($$"""
        </a>
        """);
        h.AppendLine($$"""
        </div>
        """);
        h.AppendLine($$"""
        </div>
        """);
        h.AppendLine($$"""
        <div class="gf-mb" id="gfMb"><div class="gf-m" role="dialog" aria-modal="true" aria-labelledby="gfH"><div class="gf-h"><div><h3 id="gfH">How a journal moves</h3><p>From the first line to the account balances, in four steps.</p></div><button type="button" class="gf-x" id="gfX" aria-label="Close">&times;</button></div><div class="gf-b"><svg viewBox="0 100 1000 370" class="gf-svg" role="img" aria-labelledby="gfTitle gfDesc"><title id="gfTitle">How a journal moves</title><desc id="gfDesc">Draft, submitted, approved by another user, posted. Rejected journals go back to draft; a journal can be cancelled before it is posted.</desc><defs><marker id="gfA" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse"><path d="M0 0L10 5L0 10z" fill="#7A929C"/></marker><marker id="gfAo" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse"><path d="M0 0L10 5L0 10z" fill="#E87722"/></marker><marker id="gfAr" viewBox="0 0 10 10" refX="9" refY="5" markerWidth="7" markerHeight="7" orient="auto-start-reverse"><path d="M0 0L10 5L0 10z" fill="#B42318"/></marker></defs><rect x="10" y="118" width="980" height="150" rx="18" fill="#F7FAFB"/><g transform="translate(20,134)"><rect width="190" height="118" rx="16" fill="#fff" stroke="#DFE8EC" stroke-width="1.5"/><rect width="190" height="6" rx="3" fill="#00636D"/><circle cx="26" cy="34" r="14" fill="#00636D"/><text x="26" y="39" text-anchor="middle" class="gf-n">1</text><text x="48" y="39" class="gf-t">Draft</text><rect x="16" y="56" width="39" height="20" rx="10" fill="#E6F3F4"/><text x="26" y="70" class="gf-who" fill="#00636D">You</text><text x="16" y="94" class="gf-s">Enter the lines,</text><text x="16" y="109" class="gf-s">Save draft any time</text></g><g transform="translate(270,134)"><rect width="190" height="118" rx="16" fill="#fff" stroke="#DFE8EC" stroke-width="1.5"/><rect width="190" height="6" rx="3" fill="#00636D"/><circle cx="26" cy="34" r="14" fill="#00636D"/><text x="26" y="39" text-anchor="middle" class="gf-n">2</text><text x="48" y="39" class="gf-t">Submitted</text><rect x="16" y="56" width="39" height="20" rx="10" fill="#E6F3F4"/><text x="26" y="70" class="gf-who" fill="#00636D">You</text><text x="16" y="94" class="gf-s">Debit = Credit,</text><text x="16" y="109" class="gf-s">lines are locked</text></g><g transform="translate(520,134)"><rect width="190" height="118" rx="16" fill="#fff" stroke="#DFE8EC" stroke-width="1.5"/><rect width="190" height="6" rx="3" fill="#E87722"/><circle cx="26" cy="34" r="14" fill="#E87722"/><text x="26" y="39" text-anchor="middle" class="gf-n">3</text><text x="48" y="39" class="gf-t">Approved</text><rect x="16" y="56" width="99" height="20" rx="10" fill="#FFF1E6"/><text x="26" y="70" class="gf-who" fill="#E87722">Another user</text><text x="16" y="94" class="gf-s">Checks the journal</text><text x="16" y="109" class="gf-s">and approves it</text></g><g transform="translate(790,134)"><rect width="190" height="118" rx="16" fill="#fff" stroke="#DFE8EC" stroke-width="1.5"/><rect width="190" height="6" rx="3" fill="#14734F"/><circle cx="26" cy="34" r="14" fill="#14734F"/><text x="26" y="39" text-anchor="middle" class="gf-n">4</text><text x="48" y="39" class="gf-t">Posted</text><rect x="16" y="56" width="138" height="20" rx="10" fill="#E7F6EF"/><text x="26" y="70" class="gf-who" fill="#14734F">Approver or anyone</text><text x="16" y="94" class="gf-s">Post to ledger: the</text><text x="16" y="109" class="gf-s">balances are updated</text></g><path d="M212 193 H262" stroke="#7A929C" stroke-width="2.5" marker-end="url(#gfA)"/><path d="M462 193 H512" stroke="#7A929C" stroke-width="2.5" marker-end="url(#gfA)"/><path d="M712 193 H782" stroke="#7A929C" stroke-width="2.5" marker-end="url(#gfA)"/><text x="237" y="183" text-anchor="middle" class="gf-a">Submit</text><text x="487" y="183" text-anchor="middle" class="gf-a">Approve</text><text x="747" y="170" text-anchor="middle" class="gf-a">Post to</text><text x="747" y="184" text-anchor="middle" class="gf-a">ledger</text><path d="M365 254 C 340 330, 150 330, 115 256" fill="none" stroke="#B42318" stroke-width="2" marker-end="url(#gfAr)"/><rect x="168" y="300" width="150" height="24" rx="12" fill="#FDECEC"/><text x="243" y="316" text-anchor="middle" class="gf-l" fill="#B42318">Reject, with a reason</text><path d="M650 254 C 650 320, 760 320, 790 250" fill="none" stroke="#B42318" stroke-width="2" stroke-dasharray="4 5" marker-end="url(#gfAr)"/><rect x="626" y="300" width="186" height="24" rx="12" fill="#FDECEC"/><text x="719" y="316" text-anchor="middle" class="gf-l" fill="#B42318">Posting failed? Fix, post again</text><g transform="translate(20,370)"><rect width="300" height="74" rx="14" fill="#fff" stroke="#DFE8EC" stroke-dasharray="5 5"/><circle cx="26" cy="37" r="13" fill="#EEF2F4"/><path d="M20 31l12 12M32 31L20 43" stroke="#6B7F88" stroke-width="2.2" stroke-linecap="round"/><text x="50" y="33" class="gf-t" style="font-size:15px">Cancelled</text><text x="50" y="54" class="gf-s">Any time before posting, with a reason</text></g><g transform="translate(680,370)"><rect width="300" height="74" rx="14" fill="#E7F6EF"/><text x="18" y="33" class="gf-t" style="font-size:14.5px" fill="#14734F">A posted journal cannot change</text><text x="18" y="54" class="gf-s">To correct it, enter a reversing journal</text></g><text x="350" y="400" class="gf-s" style="font-weight:700;fill:#0B2533">Two people see every journal</text><text x="350" y="420" class="gf-s">The user who submits a journal cannot approve it:</text><text x="350" y="438" class="gf-s">a different user signs in and approves it.</text></svg></div><div class="gf-f"><div><b>Where to start</b>Menu &rarr; Journals &rarr; Journal entry, or the New journal button.</div><div><b>Where each step is done</b>Inside the journal, with the button of its step. Approvers find their journals in Inbox: waiting approval, posters in Inbox: ready to post.</div><div><b>Journals from other modules</b>Mortgage, leasing, treasury ... arrive already submitted and follow steps 3 and 4.</div></div></div></div>
        """);
        h.AppendLine($$"""
        <script> (function(){   function open(e){if(e)e.preventDefault();var m=document.getElementById('gfMb');if(!m)return;m.classList.add('open');var x=document.getElementById('gfX');if(x)x.focus();}   function close(){var m=document.getElementById('gfMb');if(m)m.classList.remove('open');}   document.addEventListener('click',function(e){var t=e.target.closest?e.target.closest('[data-flow]'):null;if(t){open(e);return;}     if(e.target.id==='gfX'||e.target.id==='gfMb')close();});   document.addEventListener('keydown',function(e){if(e.key==='Escape')close();}); })(); </script>
        """);
        h.AppendLine($$"""
        <script>(function(){var h=new Date().getHours(),g=h<12?'Good morning':(h<18?'Good afternoon':'Good evening'),el=document.getElementById('whGreet');if(el)el.textContent=g;})();</script>
        """);

        return new { html = h.ToString() };
    }

    private static string U(int page) => ApexPages.Url(120, page);

    private static string E(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private static string Fmt(int? value) => value is null ? "&ndash;" : value.Value.ToString("#,##0", CultureInfo.InvariantCulture);

    private static string Day(DateOnly date) => date.ToString("dd MMM", CultureInfo.InvariantCulture);

    private static string InitCap(string value) => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value.ToLowerInvariant());

    /// <summary>The status pill: green when open, grey when closed or empty, amber otherwise.</summary>
    private static string Pill(string? status)
    {
        var css = status switch
        {
            "OPEN" => "wx-pill--ok",
            null => "wx-pill--off",
            _ when status.Contains("CLOSE", StringComparison.Ordinal) => "wx-pill--off",
            _ => "wx-pill--warn",
        };
        var label = status is null ? "&ndash;" : E(InitCap(status.Replace('_', ' ')));
        return $"<span class=\"wx-pill {css}\">{label}</span>";
    }
}
