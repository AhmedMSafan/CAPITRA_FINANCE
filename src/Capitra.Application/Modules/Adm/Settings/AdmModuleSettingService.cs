using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Crm;
using Capitra.Domain.Modules.Adm;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Adm.Settings;

public sealed record ModuleSettingDto(
    string ModuleCode, string Code, string Group, string Label, string Help, string Type, string Value, string ValueLabel, string DefaultValue,
    bool IsDefault, decimal? Min, decimal? Max, string? Unit, IReadOnlyList<CrmOptionDto> Choices, string? ShownWhen,
    string? PendingValue, string? PendingLabel, string? PendingReason, string? RequestedBy, DateTime? RequestedAt,
    string? ChangedBy, DateTime? ChangedAt, string? ApprovedBy, DateTime? ApprovedAt, bool CanApprove, string? ApproveBlockedReason);

public sealed record ModuleSettingHistoryDto(long HistoryId, string Code, string Label, string ActionCode, string? OldValue, string? NewValue, string? Notes, string By, DateTime At);

public sealed record ModuleSettingsModuleDto(string Code, string Title, string Description, string Icon, int PendingCount, IReadOnlyList<ModuleSettingDto> Settings);

public sealed record ModuleSettingsPageDto(IReadOnlyList<ModuleSettingsModuleDto> Modules, IReadOnlyList<ModuleSettingHistoryDto> History);

public sealed record ModuleSettingRequestDto(string? Value, string? Reason);

public sealed record ModuleSettingDecisionDto(string? Notes);

/// <summary>
/// Administration > Module Settings (Ahmed, 7 Nov 2026). Changing a setting is a request: it takes effect only when another user
/// approves it (always, whatever the module's own maker-checker settings say). The requester can withdraw it; another user can reject it.
/// </summary>
public sealed class AdmModuleSettingService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock, CrmSettings crm)
{
    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    private bool SameUser(string? by) => by is not null && string.Equals(by, User, StringComparison.OrdinalIgnoreCase);

    public async Task<ModuleSettingsPageDto> GetAsync(CancellationToken cancellationToken)
    {
        var rows = await db.Set<AdmModuleSetting>().AsNoTracking().ToDictionaryAsync(x => x.SettingCode, cancellationToken);
        var defaults = ModuleSettings.Defaults(crm);
        var settings = ModuleSettingCatalog.All.Select(d =>
        {
            var r = rows.GetValueOrDefault(d.Code);
            var def = defaults[d.Code];
            var value = r?.ValueText ?? def;
            var (canApprove, blocked) = r?.PendingValue is null ? (false, (string?)null)
                : SameUser(r.RequestedBy) ? (false, "You asked for this change; another user must approve it.") : (true, null);
            return new ModuleSettingDto(d.ModuleCode, d.Code, d.Group, d.Label, d.Help, ModuleSettingCatalog.TypeCode(d.Type), value,
                ModuleSettingCatalog.Display(d, value), def, value == def, d.Min, d.Max, d.Unit, d.Choices ?? [], d.ShownWhen,
                r?.PendingValue, r?.PendingValue is null ? null : ModuleSettingCatalog.Display(d, r.PendingValue), r?.PendingReason, r?.RequestedBy,
                r?.RequestedAt, r?.ChangedBy, r?.ChangedAt, r?.ApprovedBy, r?.ApprovedAt, canApprove, blocked);
        }).ToList();

        var history = await db.Set<AdmModuleSettingHistory>().AsNoTracking().OrderByDescending(x => x.HistoryId).Take(100).ToListAsync(cancellationToken);
        return new ModuleSettingsPageDto(
            [.. ModuleSettingCatalog.Modules.Select(m =>
            {
                var own = settings.Where(x => x.ModuleCode == m.Code).ToList();
                return new ModuleSettingsModuleDto(m.Code, m.Title, m.Description, m.Icon, own.Count(x => x.PendingValue is not null), own);
            })],
            [.. history.Select(h =>
            {
                var d = ModuleSettingCatalog.Find(h.SettingCode);
                return new ModuleSettingHistoryDto(h.HistoryId, h.SettingCode, d?.Label ?? h.SettingCode, h.ActionCode,
                    d is null ? h.OldValue : ModuleSettingCatalog.Display(d, h.OldValue), d is null ? h.NewValue : ModuleSettingCatalog.Display(d, h.NewValue),
                    h.Notes, h.CreatedBy, h.CreatedAt);
            })]);
    }

    /// <summary>Asks for a new value. It replaces a pending request of the same user; another user's pending request must be decided first.</summary>
    public async Task RequestAsync(string code, ModuleSettingRequestDto dto, CancellationToken cancellationToken)
    {
        var d = Definition(code);
        var (value, error) = ModuleSettingCatalog.Normalize(d, dto.Value);
        if (error is not null) throw BusinessValidationException.For("value", error);
        var reason = string.IsNullOrWhiteSpace(dto.Reason) ? throw BusinessValidationException.For("reason", "Give the reason for the change.") : dto.Reason.Trim();

        var r = await RowAsync(d, cancellationToken);
        if (r.PendingValue is not null && !SameUser(r.RequestedBy))
            throw new BusinessRuleException($"{r.RequestedBy} has already asked to change this setting. Approve or reject that request first.");
        if (value == r.ValueText)
        {
            if (r.PendingValue is null) throw new BusinessRuleException("The setting already has this value.");
            await CancelAsync(code, new ModuleSettingDecisionDto("Asked again for the value in force."), cancellationToken);
            return;
        }
        r.PendingValue = value;
        r.PendingReason = reason;
        r.RequestedBy = User;
        r.RequestedAt = clock.UtcNow;
        AddHistory(d.Code, "REQUEST", r.ValueText, value, reason);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveAsync(string code, ModuleSettingDecisionDto dto, CancellationToken cancellationToken)
    {
        var d = Definition(code);
        var r = await PendingAsync(d, cancellationToken);
        if (SameUser(r.RequestedBy)) throw new BusinessRuleException("You asked for this change. Another user must approve it.");
        var old = r.ValueText;
        r.ValueText = r.PendingValue!;
        r.ChangedBy = r.RequestedBy;
        r.ChangedAt = r.RequestedAt;
        r.ApprovedBy = User;
        r.ApprovedAt = clock.UtcNow;
        Clear(r);
        AddHistory(d.Code, "APPROVE", old, r.ValueText, Blank(dto.Notes));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(string code, ModuleSettingDecisionDto dto, CancellationToken cancellationToken)
    {
        var d = Definition(code);
        var r = await PendingAsync(d, cancellationToken);
        if (SameUser(r.RequestedBy)) throw new BusinessRuleException("You asked for this change. Withdraw it instead.");
        var notes = Blank(dto.Notes) ?? throw BusinessValidationException.For("notes", "Give the reason for the rejection.");
        AddHistory(d.Code, "REJECT", r.ValueText, r.PendingValue, notes);
        Clear(r);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(string code, ModuleSettingDecisionDto dto, CancellationToken cancellationToken)
    {
        var d = Definition(code);
        var r = await PendingAsync(d, cancellationToken);
        if (!SameUser(r.RequestedBy)) throw new BusinessRuleException("Only the user who asked for the change can withdraw it.");
        AddHistory(d.Code, "CANCEL", r.ValueText, r.PendingValue, Blank(dto.Notes));
        Clear(r);
        await db.SaveChangesAsync(cancellationToken);
    }

    private static ModuleSettingDefinition Definition(string code) =>
        ModuleSettingCatalog.Find(code?.Trim().ToUpperInvariant() ?? "") ?? throw new NotFoundException($"Setting {code} was not found.");

    /// <summary>The setting's row; a setting still on its default gets one holding the default.</summary>
    private async Task<AdmModuleSetting> RowAsync(ModuleSettingDefinition d, CancellationToken cancellationToken)
    {
        var r = await db.Set<AdmModuleSetting>().SingleOrDefaultAsync(x => x.SettingCode == d.Code, cancellationToken);
        if (r is not null) return r;
        r = new AdmModuleSetting { ModuleCode = d.ModuleCode, SettingCode = d.Code, ValueText = ModuleSettings.Defaults(crm)[d.Code] };
        db.Set<AdmModuleSetting>().Add(r);
        return r;
    }

    private async Task<AdmModuleSetting> PendingAsync(ModuleSettingDefinition d, CancellationToken cancellationToken)
    {
        var r = await db.Set<AdmModuleSetting>().SingleOrDefaultAsync(x => x.SettingCode == d.Code, cancellationToken);
        return r?.PendingValue is null ? throw new BusinessRuleException("No change of this setting is waiting for approval.") : r;
    }

    private static void Clear(AdmModuleSetting r)
    {
        r.PendingValue = null;
        r.PendingReason = null;
        r.RequestedBy = null;
        r.RequestedAt = null;
    }

    private void AddHistory(string code, string action, string? oldValue, string? newValue, string? notes) =>
        db.Set<AdmModuleSettingHistory>().Add(new AdmModuleSettingHistory { SettingCode = code, ActionCode = action, OldValue = oldValue, NewValue = newValue, Notes = notes });

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
