using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Crm;
using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;
using C = Capitra.Application.Modules.Adm.Settings.ModuleSettingCatalog.Codes;

namespace Capitra.Application.Modules.Adm.Settings;

/// <summary>The module settings in force, as CRM, Treasury and Factoring read them (one read per request).</summary>
public interface IModuleSettings
{
    Task<IReadOnlyDictionary<string, string>> ValuesAsync(CancellationToken cancellationToken);
}

/// <summary>Typed reads of the settings the modules use.</summary>
public static class ModuleSettingsExtensions
{
    public static async Task<bool> BoolAsync(this IModuleSettings s, string code, CancellationToken cancellationToken) =>
        (await s.ValuesAsync(cancellationToken))[code] == "true";

    public static async Task<int> IntAsync(this IModuleSettings s, string code, CancellationToken cancellationToken) =>
        int.Parse((await s.ValuesAsync(cancellationToken))[code], CultureInfo.InvariantCulture);

    public static async Task<decimal> DecimalAsync(this IModuleSettings s, string code, CancellationToken cancellationToken) =>
        decimal.Parse((await s.ValuesAsync(cancellationToken))[code], CultureInfo.InvariantCulture);

    public static async Task<string> TextAsync(this IModuleSettings s, string code, CancellationToken cancellationToken) =>
        (await s.ValuesAsync(cancellationToken))[code];

    /// <summary>The bank account waiting period in days: 0 when there is no waiting period.</summary>
    public static async Task<int> BankAccountWaitingDaysAsync(this IModuleSettings s, CancellationToken cancellationToken) =>
        await s.BoolAsync(C.CrmBankAccountWaiting, cancellationToken) ? await s.IntAsync(C.CrmBankAccountWaitingDays, cancellationToken) : 0;

    /// <summary>The review cycle of a risk grade (MEDIUM when there is none).</summary>
    public static async Task<int> ReviewMonthsAsync(this IModuleSettings s, string? riskGrade, CancellationToken cancellationToken) =>
        await s.IntAsync(riskGrade switch { "LOW" => C.CrmReviewMonthsLow, "HIGH" => C.CrmReviewMonthsHigh, _ => C.CrmReviewMonthsMedium }, cancellationToken);
}

/// <summary>
/// Reads ADM_MODULE_SETTINGS once per request; a setting without a row keeps its default. The bank account waiting days default to
/// appsettings Crm:BankAccountWaitingDays, as before the setting existed.
/// </summary>
public sealed class ModuleSettings(ICapitraDbContext db, CrmSettings crm) : IModuleSettings
{
    private Dictionary<string, string>? values;

    /// <summary>The catalog defaults, with the waiting period from appsettings (0 there means no waiting period).</summary>
    public static IReadOnlyDictionary<string, string> Defaults(CrmSettings crm)
    {
        var all = ModuleSettingCatalog.All.ToDictionary(x => x.Code, x => x.Default);
        all[C.CrmBankAccountWaitingDays] = Math.Clamp(crm.BankAccountWaitingDays, 1, 90).ToString(CultureInfo.InvariantCulture);
        if (crm.BankAccountWaitingDays <= 0) all[C.CrmBankAccountWaiting] = "false";
        return all;
    }

    public async Task<IReadOnlyDictionary<string, string>> ValuesAsync(CancellationToken cancellationToken)
    {
        if (values is not null) return values;
        var all = new Dictionary<string, string>(Defaults(crm));
        foreach (var row in await db.Set<AdmModuleSetting>().AsNoTracking().Select(x => new { x.SettingCode, x.ValueText }).ToListAsync(cancellationToken))
            if (all.ContainsKey(row.SettingCode)) all[row.SettingCode] = row.ValueText;
        return values = all;
    }
}

/// <summary>Fixed values, for tests and callers outside a request.</summary>
public sealed class FixedModuleSettings(IReadOnlyDictionary<string, string>? overrides = null) : IModuleSettings
{
    public Task<IReadOnlyDictionary<string, string>> ValuesAsync(CancellationToken cancellationToken)
    {
        var all = ModuleSettingCatalog.All.ToDictionary(x => x.Code, x => x.Default);
        foreach (var (k, v) in overrides ?? new Dictionary<string, string>()) all[k] = v;
        return Task.FromResult<IReadOnlyDictionary<string, string>>(all);
    }
}
