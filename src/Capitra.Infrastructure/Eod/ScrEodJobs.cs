using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Eod;
using Capitra.Domain.Modules.Scr;
using Capitra.Infrastructure.Communication;
using Capitra.Infrastructure.Identity;
using Capitra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using static Capitra.Infrastructure.Communication.SystemEmails;

namespace Capitra.Infrastructure.Eod;

/// <summary>
/// End of Day job SCR_LICENCE_EXPIRY: e-mails each member company's administrator (and the company's contact e-mail) some days before its
/// licence ends, and on its last day. Sign-in and inquiries stop by themselves the day after (CompanyAccess); nothing is changed here.
/// </summary>
public sealed class ScrLicenceExpiryEodJob(CapitraDbContext db, SystemEmails emails) : IEodJob
{
    public const string ReminderDays = "REMINDER_DAYS";
    public const string Kind = "LICENCE_NOTICE";

    public string JobCode => "SCR_LICENCE_EXPIRY";
    public string ModuleCode => "SCR";
    public string NameEn => "Screening: licence expiry notices";
    public string NameAr => "الفحص: تنبيهات انتهاء الترخيص";
    public string DescriptionEn => "E-mails each member company's administrator and contact e-mail the set number of days before its licence ends, and on its last "
        + "day. From the day after, the company's users can no longer sign in or make inquiries until the licence is renewed.";
    public string DescriptionAr => "يرسل بريدًا إلى مسؤول كل شركة عضو وبريد التواصل الخاص بها قبل انتهاء الترخيص بعدد الأيام المحدد، وفي آخر يوم له. "
        + "ومن اليوم التالي لا يستطيع مستخدمو الشركة تسجيل الدخول أو إجراء الاستعلامات حتى يُجدَّد الترخيص.";

    public IReadOnlyList<EodParameterDefinition> Parameters =>
    [
        new(ReminderDays, "Reminder days before the end", "أيام التذكير قبل الانتهاء", "Days before the licence's last day to send a reminder, separated by commas, e.g. 30,7,1.",
            "عدد الأيام قبل آخر يوم في الترخيص لإرسال التذكير، مفصولة بفواصل، مثل 30,7,1.", "30,7,1"),
    ];

    public async Task<EodJobOutcome> RunAsync(EodJobContext context, CancellationToken cancellationToken)
    {
        var day = context.BusinessDate;
        var reminders = context.Numbers(ReminderDays);
        var companies = await db.Set<ScrMemberCompany>().AsNoTracking().Where(c => c.StatusCode == "ACTIVE").OrderBy(c => c.CompanyCode).ToListAsync(cancellationToken);
        var licences = (await db.Set<ScrCompanyLicence>().AsNoTracking().ToListAsync(cancellationToken))
            .GroupBy(l => l.MemberCompanyId).ToDictionary(g => g.Key, g => g.MaxBy(l => l.LicenceId)!);
        var admins = (await (from u in db.Users.AsNoTracking()
                             join ur in db.UserRoles on u.Id equals ur.UserId
                             join r in db.Roles on ur.RoleId equals r.Id
                             where u.MemberCompanyId != null && u.IsActive && u.PasswordHash != null && r.NormalizedName == "MEMBER_COMPANY_ADMIN"
                             select new { CompanyId = u.MemberCompanyId!.Value, u.Id, u.Email, u.FullName }).ToListAsync(cancellationToken))
            .ToLookup(x => x.CompanyId);

        int companiesNoticed = 0, sent = 0, notSent = 0;
        foreach (var c in companies)
        {
            if (!licences.TryGetValue(c.MemberCompanyId, out var l) || l.ValidTo is not { } end || day < l.ValidFrom) continue;
            var left = end.DayNumber - day.DayNumber;
            if (left < 0 || (left > 0 && !reminders.Contains(left))) continue;
            companiesNoticed++;
            var subject = left == 0
                ? $"CAPITRA — {c.CompanyCode}: your licence ends today ({end:yyyy-MM-dd}) / ينتهي ترخيصكم اليوم"
                : $"CAPITRA — {c.CompanyCode}: your licence ends on {end:yyyy-MM-dd} / ينتهي ترخيصكم في {end:yyyy-MM-dd}";
            var en = left == 0
                ? $"<p>The CAPITRA licence of <b>{E(c.NameEn)}</b> ends <b>today, {end:yyyy-MM-dd}</b>. From tomorrow the company's users cannot sign in or make inquiries.</p><p>Contact the Federation to renew it.</p>"
                : $"<p>The CAPITRA licence of <b>{E(c.NameEn)}</b> ends on <b>{end:yyyy-MM-dd}</b>, in {left} day(s). After that day the company's users cannot sign in or make inquiries.</p><p>Contact the Federation to renew it in time.</p>";
            var ar = left == 0
                ? $"<p>ينتهي ترخيص كابيترا لشركة <b>{E(c.NameAr)}</b> <b>اليوم {end:yyyy-MM-dd}</b>. ابتداءً من الغد لن يستطيع مستخدمو الشركة تسجيل الدخول أو إجراء الاستعلامات.</p><p>تواصلوا مع الاتحاد لتجديده.</p>"
                : $"<p>ينتهي ترخيص كابيترا لشركة <b>{E(c.NameAr)}</b> في <b>{end:yyyy-MM-dd}</b>، بعد {left} يوم. بعد هذا اليوم لن يستطيع مستخدمو الشركة تسجيل الدخول أو إجراء الاستعلامات.</p><p>تواصلوا مع الاتحاد لتجديده في الوقت المناسب.</p>";
            var to = new List<(string? Email, string? UserId)>();
            foreach (var a in admins[c.MemberCompanyId]) to.Add((a.Email, a.Id));
            to.Add((c.ContactEmail, null));
            to = [.. to.Where(x => !string.IsNullOrWhiteSpace(x.Email)).DistinctBy(x => x.Email!.Trim().ToUpperInvariant())];
            if (to.Count == 0) notSent++;
            foreach (var (address, userId) in to)
            {
                var status = await emails.SendAsync(address, userId, Kind, subject, en, ar, cancellationToken);
                if (status is "SENT" or "ALREADY_SENT") sent++;
                else notSent++;
            }
        }

        var days = string.Join(", ", reminders.Where(x => x > 0).OrderByDescending(x => x).Append(0));
        var summary = $"Licences ending in {days} days: {companiesNoticed} company(ies); {sent} e-mail(s) sent";
        if (notSent > 0) summary += $"; {notSent} not sent (no address, or the mail server failed: see Communication Center › Sent e-mails)";
        return new EodJobOutcome(sent, summary + ".");
    }
}

/// <summary>
/// End of Day job SCR_HOUSEKEEPING: closes sign-ins left waiting for the authenticator code (CODE_TIMEOUT, shown as "Code not entered"),
/// and reports company accounts whose activation link expired before they chose a password, so the link can be sent again.
/// </summary>
public sealed class ScrHousekeepingEodJob(CapitraDbContext db, IClock clock, IOptions<EmailOptions> email) : IEodJob
{
    public string JobCode => "SCR_HOUSEKEEPING";
    public string ModuleCode => "SCR";
    public string NameEn => "Screening: sign-in housekeeping";
    public string NameAr => "الفحص: تنظيم سجل الدخول";
    public string DescriptionEn => "Sign-ins left waiting for the authenticator code are closed as 'Code not entered'; company accounts whose activation link "
        + "expired before they chose a password are counted, so the link can be sent again from Company users.";
    public string DescriptionAr => "تُغلق محاولات الدخول التي بقيت بانتظار رمز التطبيق بنتيجة 'لم يُدخل الرمز'، وتُحصى حسابات الشركات التي انتهت صلاحية رابط تفعيلها "
        + "قبل اختيار كلمة المرور، ليُعاد إرسال الرابط من شاشة مستخدمي الشركة.";
    public IReadOnlyList<EodParameterDefinition> Parameters => [];

    public async Task<EodJobOutcome> RunAsync(EodJobContext context, CancellationToken cancellationToken)
    {
        var now = clock.UtcNow;
        var cutoff = now.AddHours(-1);
        var waiting = await db.Set<SecSignIn>().Where(s => s.ResultCode == "AWAITING_CODE" && s.AttemptedAt < cutoff).ToListAsync(cancellationToken);
        foreach (var s in waiting)
        {
            s.ResultCode = "CODE_TIMEOUT";
            s.FailureText ??= "The authenticator code was not entered in time.";
        }
        await db.SaveChangesAsync(cancellationToken);

        var linkCutoff = now.AddHours(-Math.Max(1, email.Value.LinkHours));
        var expired = await db.Users.AsNoTracking()
            .CountAsync(u => u.MemberCompanyId != null && u.IsActive && u.PasswordHash == null && (u.UpdatedAt ?? u.CreatedAt) < linkCutoff, cancellationToken);
        var summary = $"{waiting.Count} sign-in(s) left waiting for the code closed; {expired} company account(s) not activated and their link expired"
            + (expired > 0 ? " (send the link again from Member companies › Users)." : ".");
        return new EodJobOutcome(waiting.Count, summary);
    }
}
