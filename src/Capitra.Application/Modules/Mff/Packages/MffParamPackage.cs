using Capitra.Application.Abstractions;
using Capitra.Domain.Modules.Mff;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Mff.Packages;

/// <summary>MFF_PARAM_PKG: a program parameter on a date, the financier's own value before the program's.</summary>
public sealed class MffParamPackage(ICapitraDbContext db)
{
    /// <summary>GET_NUMBER: the parameter's number, or the default when there is no active row on that date.</summary>
    public async Task<decimal?> GetNumberAsync(long programVersionId, string paramCode, long? financierId, DateOnly asOf, decimal? defaultValue, CancellationToken ct)
    {
        var row = await Current(programVersionId, paramCode, financierId, asOf).Select(p => new { p.ValueNumber }).FirstOrDefaultAsync(ct);
        return row is null ? defaultValue : row.ValueNumber;
    }

    /// <summary>GET_TEXT: the parameter's text, or the default when there is none.</summary>
    public async Task<string?> GetTextAsync(long programVersionId, string paramCode, long? financierId, DateOnly asOf, string? defaultValue, CancellationToken ct)
    {
        var row = await Current(programVersionId, paramCode, financierId, asOf).Select(p => new { p.ValueText }).FirstOrDefaultAsync(ct);
        return row?.ValueText ?? defaultValue;
    }

    private IQueryable<MffProgramParam> Current(long programVersionId, string paramCode, long? financierId, DateOnly asOf)
    {
        var code = paramCode.Trim().ToUpperInvariant();
        return db.Set<MffProgramParam>().AsNoTracking()
            .Where(p => p.ProgramVersionId == programVersionId && p.ParamCode == code && p.IsActive
                && ((financierId != null && p.FinancierId == financierId) || p.FinancierId == null)
                && (p.EffectiveFromDate ?? new DateOnly(1900, 1, 1)) <= asOf
                && (p.EffectiveToDate ?? new DateOnly(2999, 12, 31)) >= asOf)
            .OrderBy(p => financierId != null && p.FinancierId == financierId ? 0 : 1)
            .ThenByDescending(p => p.EffectiveFromDate ?? new DateOnly(1900, 1, 1))
            .ThenByDescending(p => p.ParamId);
    }
}
