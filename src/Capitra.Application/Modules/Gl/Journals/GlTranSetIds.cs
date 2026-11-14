using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Journals;

/// <summary>
/// The one place a module takes its next GL transaction set ID from: CAP-{module}-{year}-{number}, e.g. CAP-CHW-2026-00000001,
/// counted per module and year in GL_TRAN_SET_SEQUENCES (the number starts again at 1 each year).
/// </summary>
public sealed class GlTranSetIds(ICapitraDbContext db)
{
    /// <summary>
    /// The next ID of the module for the year of <paramref name="date"/>. It must run in the transaction of the posting:
    /// the counter row stays locked until that transaction ends, so two postings never get the same number, and a
    /// posting that rolls back gives its number back.
    /// </summary>
    public async Task<string> NextAsync(string moduleCode, DateOnly date, CancellationToken ct)
    {
        if (db.CurrentTransaction is null)
            throw new InvalidOperationException("A GL transaction set ID must be taken inside the posting's transaction.");
        var module = (moduleCode ?? string.Empty).Trim().ToUpperInvariant();
        var year = date.Year;

        // the module's row for the year; a new year starts from the module's latest row
        if (!await db.Set<GlTranSetSequence>().AnyAsync(x => x.ModuleCode == module && x.YearNo == year, ct))
        {
            var last = await db.Set<GlTranSetSequence>().AsNoTracking().Where(x => x.ModuleCode == module)
                .OrderByDescending(x => x.YearNo).FirstOrDefaultAsync(ct)
                ?? throw new ApexProcessException($"ORA-20980: Module {module} has no GL transaction set sequence. Add it to GL_TRAN_SET_SEQUENCES.");
            db.Set<GlTranSetSequence>().Add(new GlTranSetSequence
            {
                ModuleCode = module,
                YearNo = year,
                AppCode = last.AppCode,
                ModuleNameEn = last.ModuleNameEn,
                ModuleNameAr = last.ModuleNameAr,
                Digits = last.Digits,
                IsActive = last.IsActive,
                LastNo = 0,
            });
            await db.SaveChangesAsync(ct);
        }

        // the update locks the row until the transaction ends
        await db.ExecuteSqlAsync(
            $"""UPDATE "GL_TRAN_SET_SEQUENCES" SET "LAST_NO" = "LAST_NO" + 1, "ROW_VERSION" = "ROW_VERSION" + 1 WHERE "MODULE_CODE" = {module} AND "YEAR_NO" = {year}""", ct);
        var seq = await db.Set<GlTranSetSequence>().AsNoTracking().FirstAsync(x => x.ModuleCode == module && x.YearNo == year, ct);
        if (!seq.IsActive)
            throw new ApexProcessException($"ORA-20981: The GL transaction set sequence of module {module} is not active.");

        return Format(module, year, seq.LastNo, seq.Digits);
    }

    public static string Format(string moduleCode, int year, long number, int digits) =>
        GlTranSetSequence.Prefix + "-" + moduleCode + "-" + year.ToString(CultureInfo.InvariantCulture) + "-"
        + number.ToString(CultureInfo.InvariantCulture).PadLeft(Math.Max(digits, 8), '0');
}
