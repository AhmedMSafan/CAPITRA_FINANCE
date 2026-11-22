using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Scr;
using Capitra.Domain.Modules.Scr;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Infrastructure.Identity;

/// <summary>A sign-in attempt; for a successful one, its session's state and how many inquiries were made in it.</summary>
public sealed record SignInDto(
    long SignInId, string? SessionId, string UserName, string? FullName, long? MemberCompanyId, string? CompanyCode, string? CompanyNameAr,
    DateTime AttemptedAt, string ResultCode, string? FailureText, string? IpAddress, string? Device, string? UserAgent, DateTime? ExpiresAt,
    DateTime? SignedOutAt, string? SessionStateCode, int InquiryCount, DateTime? LastActivityAt, string? SecondStepCode = null);

/// <summary>An inquiry made in a session: when it was received, when its result was sent back, and the result.</summary>
public sealed record SessionInquiryDto(
    long InquiryId, string InquiryRef, DateTime? ReceivedAt, DateTime? RespondedAt, int? DurationMs, string SubjectTypeCode, string SubjectName,
    string IdentityTypeCode, string IdentityNo, string PurposeCode, string ResultCode, int HitCount, int RegistryVersionNo);

public sealed record SessionDto(SignInDto SignIn, IReadOnlyList<SessionInquiryDto> Inquiries);

/// <param name="MemberCompanyId">A company; 0 = the Federation's own users; empty = everyone.</param>
public sealed record SignInQuery(DateOnly? FromDate, DateOnly? ToDate, long? MemberCompanyId, string? ResultCode, string? Search, int? Take);

public sealed record SignInSummary(int Attempts, int Successful, int Refused, int Users, int Inquiries);

public sealed record SignInPage(SignInSummary Summary, IReadOnlyList<SignInDto> Rows);

/// <summary>
/// The sign-in log: every attempt with its result, and per session what was done in it (screening inquiries: received, answered,
/// result). Read-only.
/// </summary>
public sealed class SignInLog(CapitraDbContext db, IClock clock)
{
    /// <summary>The results that refused a sign-in (AWAITING_CODE, CODE_TIMEOUT and TWO_FACTOR_RESET are neither successes nor refusals).</summary>
    public static readonly string[] Refusals = ["UNKNOWN_USER", "INACTIVE", "LOCKED", "BAD_PASSWORD", "COMPANY_BLOCKED", "BAD_CODE", "IP_BLOCKED"];

    public async Task<SignInPage> ListAsync(SignInQuery q, CancellationToken ct)
    {
        var today = ScrBilling.CairoDate(clock.UtcNow);
        var from = ScrBilling.CairoStartUtc(q.FromDate ?? today.AddDays(-6));
        var to = ScrBilling.CairoStartUtc((q.ToDate ?? today).AddDays(1));
        var rows = db.Set<SecSignIn>().AsNoTracking().Where(s => s.AttemptedAt >= from && s.AttemptedAt < to);
        if (q.MemberCompanyId is 0)
            rows = rows.Where(s => s.MemberCompanyId == null);
        else if (q.MemberCompanyId is { } c)
            rows = rows.Where(s => s.MemberCompanyId == c);
        if (!string.IsNullOrWhiteSpace(q.ResultCode))
            rows = q.ResultCode == "REFUSED" ? rows.Where(s => Refusals.Contains(s.ResultCode)) : rows.Where(s => s.ResultCode == q.ResultCode);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var like = $"%{q.Search.Trim()}%";
            rows = rows.Where(s => EF.Functions.ILike(s.UserName, like) || (s.IpAddress != null && EF.Functions.ILike(s.IpAddress, like)));
        }

        var list = await rows.OrderByDescending(s => s.AttemptedAt).ThenByDescending(s => s.SignInId).Take(Math.Clamp(q.Take ?? 1000, 1, 5000)).ToListAsync(ct);
        var dtos = await ToDtosAsync(list, ct);
        var summary = new SignInSummary(
            dtos.Count, dtos.Count(d => d.ResultCode == "SUCCESS"), dtos.Count(d => Refusals.Contains(d.ResultCode)),
            dtos.Where(d => d.ResultCode == "SUCCESS").Select(d => d.UserName.ToUpperInvariant()).Distinct().Count(), dtos.Sum(d => d.InquiryCount));
        return new SignInPage(summary, dtos);
    }

    public async Task<SessionDto> SessionAsync(long signInId, CancellationToken ct)
    {
        var s = await db.Set<SecSignIn>().AsNoTracking().FirstOrDefaultAsync(x => x.SignInId == signInId, ct)
            ?? throw new NotFoundException("The sign-in was not found.");
        var dto = (await ToDtosAsync([s], ct))[0];
        if (s.SessionId is null)
            return new SessionDto(dto, []);
        var inquiries = await db.Set<ScrInquiry>().AsNoTracking().Where(i => i.SessionId == s.SessionId).OrderBy(i => i.InquiryId)
            .Select(i => new
            {
                i.InquiryId, i.InquiryRef, i.ReceivedAt, i.RespondedAt, i.SubjectTypeCode, i.SubjectName, i.IdentityTypeCode, i.IdentityNo,
                i.PurposeCode, i.ResultCode, Hits = i.Hits.Count, i.RegistryVersionNo,
            }).ToListAsync(ct);
        return new SessionDto(dto, [.. inquiries.Select(i => new SessionInquiryDto(
            i.InquiryId, i.InquiryRef, i.ReceivedAt, i.RespondedAt,
            i.ReceivedAt is { } a && i.RespondedAt is { } b ? (int)Math.Round((b - a).TotalMilliseconds) : null,
            i.SubjectTypeCode, i.SubjectName, i.IdentityTypeCode, i.IdentityNo, i.PurposeCode, i.ResultCode, i.Hits, i.RegistryVersionNo))]);
    }

    private async Task<List<SignInDto>> ToDtosAsync(List<SecSignIn> list, CancellationToken ct)
    {
        var companyIds = list.Where(s => s.MemberCompanyId != null).Select(s => s.MemberCompanyId!.Value).Distinct().ToList();
        var companies = await db.Set<ScrMemberCompany>().AsNoTracking().Where(c => companyIds.Contains(c.MemberCompanyId))
            .ToDictionaryAsync(c => c.MemberCompanyId, ct);
        var userIds = list.Where(s => s.UserId != null).Select(s => s.UserId!).Distinct().ToList();
        var names = await db.Users.AsNoTracking().Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.FullName, ct);
        var sessions = list.Where(s => s.SessionId != null).Select(s => s.SessionId!).ToList();
        var activity = await db.Set<ScrInquiry>().AsNoTracking().Where(i => i.SessionId != null && sessions.Contains(i.SessionId))
            .GroupBy(i => i.SessionId!).Select(g => new { g.Key, Count = g.Count(), Last = g.Max(i => i.ScreenedAt) })
            .ToDictionaryAsync(x => x.Key, ct);
        var now = clock.UtcNow;
        return [.. list.Select(s =>
        {
            var company = s.MemberCompanyId is { } c ? companies.GetValueOrDefault(c) : null;
            var act = s.SessionId is { } sid ? activity.GetValueOrDefault(sid) : null;
            var state = s.SessionId is null ? null : s.SignedOutAt is not null ? "SIGNED_OUT" : s.ExpiresAt is { } e && e <= now ? "EXPIRED" : "OPEN";
            return new SignInDto(
                s.SignInId, s.SessionId, s.UserName, s.UserId is { } u ? names.GetValueOrDefault(u) : null, s.MemberCompanyId, company?.CompanyCode,
                company?.NameAr, s.AttemptedAt, s.ResultCode, s.FailureText, s.IpAddress, Device(s.UserAgent), s.UserAgent, s.ExpiresAt, s.SignedOutAt,
                state, act?.Count ?? 0, act?.Last, s.SecondStepCode);
        })];
    }

    /// <summary>"Chrome · Windows" from the browser's user agent; the full text stays in the log.</summary>
    public static string? Device(string? agent)
    {
        if (string.IsNullOrWhiteSpace(agent))
            return null;
        static bool Has(string a, string v) => a.Contains(v, StringComparison.OrdinalIgnoreCase);
        var browser = Has(agent, "Edg/") ? "Edge" : Has(agent, "OPR/") ? "Opera" : Has(agent, "Firefox/") ? "Firefox"
            : Has(agent, "Chrome/") ? "Chrome" : Has(agent, "Safari/") ? "Safari" : Has(agent, "curl/") ? "curl"
            : Has(agent, "PostmanRuntime") ? "Postman" : null;
        var os = Has(agent, "Windows") ? "Windows" : Has(agent, "Android") ? "Android" : Has(agent, "iPhone") || Has(agent, "iPad") ? "iOS"
            : Has(agent, "Mac OS") ? "macOS" : Has(agent, "Linux") ? "Linux" : null;
        return browser is null && os is null ? (agent.Length > 40 ? agent[..40] : agent) : string.Join(" · ", new[] { browser, os }.Where(x => x is not null));
    }
}
