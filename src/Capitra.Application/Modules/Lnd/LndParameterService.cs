using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Application.Modules.Adm.Settings;
using Capitra.Domain.Modules.Lnd;
using Microsoft.EntityFrameworkCore;
using S = Capitra.Application.Modules.Lnd.LndParameterCatalog.Statuses;

namespace Capitra.Application.Modules.Lnd;

public sealed record LndParameterPendingDto(
    long ParameterValueId, string Value, string ValueLabel, DateOnly EffectiveFromDate, string? DecisionReference, string Reason,
    string RequestedBy, DateTime RequestedAt, bool CanApprove, string? ApproveBlockedReason);

public sealed record LndParameterDto(
    string Code, string Group, string Label, string Help, string Type, bool IsLegal, decimal? Min, decimal? Max, string? Unit,
    string? SuggestedValue, string? SuggestedLabel, string? Source,
    string? Value, string? ValueLabel, DateOnly? EffectiveFromDate, string? DecisionReference, string? ApprovedBy, DateTime? ApprovedAt,
    string? NextValueLabel, DateOnly? NextEffectiveFromDate, LndParameterPendingDto? Pending);

public sealed record LndScopeDto(
    string Code, string Title, string Description, string Icon, int PendingCount, IReadOnlyList<string> Problems, IReadOnlyList<LndParameterDto> Parameters);

public sealed record LndParameterHistoryDto(
    long ParameterValueId, string Code, string Label, string ScopeCode, string ValueLabel, DateOnly EffectiveFromDate, string? DecisionReference,
    string StatusCode, string Reason, string RequestedBy, DateTime RequestedAt, string? DecidedBy, DateTime? DecidedAt, string? DecisionNotes);

public sealed record LndParametersPageDto(DateOnly Today, bool CanEdit, bool CanApprove, IReadOnlyList<LndScopeDto> Scopes, IReadOnlyList<LndParameterHistoryDto> History);

public sealed record LndParameterRequestDto(string? Value, DateOnly? EffectiveFromDate, string? DecisionReference, string? Reason);

public sealed record LndParameterDecisionDto(string? Notes);

/// <summary>The value of a Retail Lending parameter on a business day, for the lending engine (decision gates, end of day).</summary>
public interface ILndParameters
{
    /// <summary>The approved value in force, or null when there is none (the caller must then stop: no value means no lending).</summary>
    Task<string?> ValueAsync(string code, string scope, DateOnly day, CancellationToken cancellationToken);

    Task<decimal?> NumberAsync(string code, string scope, DateOnly day, CancellationToken cancellationToken);

    /// <summary>The go-live check of a licence: missing values and values that do not fit together. Empty means it can lend.</summary>
    Task<IReadOnlyList<string>> ProblemsAsync(string scope, DateOnly day, CancellationToken cancellationToken);
}

/// <summary>
/// Retail Lending > Parameters. A new value is a request with an effective date and, for a legal figure, the FRA decision it comes
/// from. It is used only after another user approves it (four eyes, always). Values are never edited or deleted, so every past
/// day's value can be shown. A value cannot start before today: a past day keeps the value that was in force on it.
/// </summary>
public sealed class LndParameterService(ICapitraDbContext db, ICurrentUser currentUser, IClock clock) : ILndParameters
{
    private static readonly TimeZoneInfo Cairo = TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo");

    private string User => currentUser.UserName ?? currentUser.UserId ?? "SYSTEM";

    private bool SameUser(string? by) => by is not null && string.Equals(by, User, StringComparison.OrdinalIgnoreCase);

    public DateOnly Today => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(clock.UtcNow, Cairo));

    public async Task<LndParametersPageDto> GetAsync(bool canEdit, bool canApprove, CancellationToken cancellationToken)
    {
        var rows = await db.Set<LndParameterValue>().AsNoTracking().ToListAsync(cancellationToken);
        var today = Today;

        var scopes = LndParameterCatalog.Scopes.Select(s =>
        {
            var parameters = LndParameterCatalog.Parameters.Where(d => d.Scopes.Contains(s.Code)).Select(d =>
            {
                var now = LndParameterCatalog.InForce(rows, d.Code, s.Code, today);
                var next = LndParameterCatalog.Next(rows, d.Code, s.Code, today);
                var p = rows.FirstOrDefault(x => x.ParameterCode == d.Code && x.ScopeCode == s.Code && x.StatusCode == S.Pending);
                var suggested = d.Suggested?.GetValueOrDefault(s.Code);
                return new LndParameterDto(d.Code, d.Group, d.Label, d.Help, ModuleSettingCatalog.TypeCode(d.Type), d.IsLegal, d.Min, d.Max, d.Unit,
                    suggested, suggested is null ? null : LndParameterCatalog.Display(d, suggested), d.Source,
                    now?.ValueText, now is null ? null : LndParameterCatalog.Display(d, now.ValueText), now?.EffectiveFromDate, now?.DecisionReference,
                    now?.DecidedBy, now?.DecidedAt,
                    next is null ? null : LndParameterCatalog.Display(d, next.ValueText), next?.EffectiveFromDate,
                    p is null ? null : new LndParameterPendingDto(p.ParameterValueId, p.ValueText, LndParameterCatalog.Display(d, p.ValueText),
                        p.EffectiveFromDate, p.DecisionReference, p.RequestReason, p.RequestedBy, p.RequestedAt,
                        canApprove && !SameUser(p.RequestedBy), SameUser(p.RequestedBy) ? "You asked for this value; another user must approve it." : null));
            }).ToList();
            return new LndScopeDto(s.Code, s.Title, s.Description, s.Icon, parameters.Count(x => x.Pending is not null),
                LndParameterCatalog.Problems(rows, s.Code, today), parameters);
        }).ToList();

        var history = rows.OrderByDescending(x => x.ParameterValueId).Take(200).Select(h =>
        {
            var d = LndParameterCatalog.Find(h.ParameterCode);
            return new LndParameterHistoryDto(h.ParameterValueId, h.ParameterCode, d?.Label ?? h.ParameterCode, h.ScopeCode,
                d is null ? h.ValueText : LndParameterCatalog.Display(d, h.ValueText), h.EffectiveFromDate, h.DecisionReference, h.StatusCode,
                h.RequestReason, h.RequestedBy, h.RequestedAt, h.DecidedBy, h.DecidedAt, h.DecisionNotes);
        }).ToList();

        return new LndParametersPageDto(today, canEdit, canApprove, scopes, history);
    }

    /// <summary>Asks for a new value. One request per parameter and licence at a time; the requester's own earlier request is replaced.</summary>
    public async Task RequestAsync(string code, string scope, LndParameterRequestDto dto, CancellationToken cancellationToken)
    {
        var d = LndParameterCatalog.Find(code) ?? throw new NotFoundException($"Parameter {code} was not found.");
        var s = LndParameterCatalog.FindScope(scope) ?? throw new NotFoundException($"Licence {scope} was not found.");
        if (!d.Scopes.Contains(s.Code)) throw new BusinessRuleException($"{d.Label} does not apply to {s.Title}.");

        var errors = new Dictionary<string, string[]>();
        var (value, error) = LndParameterCatalog.Normalize(d, dto.Value);
        if (error is not null) errors["value"] = [error];
        var today = Today;
        if (dto.EffectiveFromDate is not { } from) errors["effectiveFromDate"] = ["Enter the date the value starts."];
        else if (from < today) errors["effectiveFromDate"] = ["A value cannot start before today."];
        var reference = Blank(dto.DecisionReference);
        if (d.IsLegal && reference is null) errors["decisionReference"] = ["Enter the FRA decision this value comes from."];
        var reason = Blank(dto.Reason);
        if (reason is null) errors["reason"] = ["Give the reason for the change."];
        if (errors.Count > 0) throw new BusinessValidationException(errors);

        var rows = await db.Set<LndParameterValue>().Where(x => x.ParameterCode == d.Code && x.ScopeCode == s.Code).ToListAsync(cancellationToken);
        var pending = rows.FirstOrDefault(x => x.StatusCode == S.Pending);
        if (pending is not null)
        {
            if (!SameUser(pending.RequestedBy))
                throw new BusinessRuleException($"{pending.RequestedBy} has already asked for a new value. Approve or reject that request first.");
            Decide(pending, S.Withdrawn, "Replaced by a new request.");
        }
        var inForce = LndParameterCatalog.InForce(rows, d.Code, s.Code, dto.EffectiveFromDate!.Value);
        if (inForce is not null && inForce.ValueText == value && inForce.EffectiveFromDate == dto.EffectiveFromDate)
            throw new BusinessRuleException("This value is already approved from this date.");

        db.Set<LndParameterValue>().Add(new LndParameterValue
        {
            ParameterCode = d.Code, ScopeCode = s.Code, ValueText = value!, EffectiveFromDate = dto.EffectiveFromDate.Value,
            DecisionReference = reference, RequestReason = reason!, StatusCode = S.Pending, RequestedBy = User, RequestedAt = clock.UtcNow,
        });
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ApproveAsync(long id, LndParameterDecisionDto dto, CancellationToken cancellationToken)
    {
        var r = await PendingAsync(id, cancellationToken);
        if (SameUser(r.RequestedBy)) throw new BusinessRuleException("You asked for this value. Another user must approve it.");
        if (r.EffectiveFromDate < Today) throw new BusinessRuleException("The value's start date has passed. Reject it and ask again with a new date.");
        Decide(r, S.Approved, Blank(dto.Notes));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RejectAsync(long id, LndParameterDecisionDto dto, CancellationToken cancellationToken)
    {
        var r = await PendingAsync(id, cancellationToken);
        if (SameUser(r.RequestedBy)) throw new BusinessRuleException("You asked for this value. Withdraw it instead.");
        Decide(r, S.Rejected, Blank(dto.Notes) ?? throw BusinessValidationException.For("notes", "Give the reason for the rejection."));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task WithdrawAsync(long id, LndParameterDecisionDto dto, CancellationToken cancellationToken)
    {
        var r = await PendingAsync(id, cancellationToken);
        if (!SameUser(r.RequestedBy)) throw new BusinessRuleException("Only the user who asked for the value can withdraw it.");
        Decide(r, S.Withdrawn, Blank(dto.Notes));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<string?> ValueAsync(string code, string scope, DateOnly day, CancellationToken cancellationToken)
    {
        var rows = await db.Set<LndParameterValue>().AsNoTracking()
            .Where(x => x.ParameterCode == code && x.ScopeCode == scope && x.StatusCode == S.Approved && x.EffectiveFromDate <= day)
            .ToListAsync(cancellationToken);
        return LndParameterCatalog.InForce(rows, code, scope, day)?.ValueText;
    }

    public async Task<decimal?> NumberAsync(string code, string scope, DateOnly day, CancellationToken cancellationToken) =>
        await ValueAsync(code, scope, day, cancellationToken) is { } v ? decimal.Parse(v, NumberStyles.Number, CultureInfo.InvariantCulture) : null;

    public async Task<IReadOnlyList<string>> ProblemsAsync(string scope, DateOnly day, CancellationToken cancellationToken)
    {
        var rows = await db.Set<LndParameterValue>().AsNoTracking()
            .Where(x => x.ScopeCode == scope && x.StatusCode == S.Approved && x.EffectiveFromDate <= day)
            .ToListAsync(cancellationToken);
        return LndParameterCatalog.Problems(rows, scope, day);
    }

    private async Task<LndParameterValue> PendingAsync(long id, CancellationToken cancellationToken)
    {
        var r = await db.Set<LndParameterValue>().SingleOrDefaultAsync(x => x.ParameterValueId == id, cancellationToken)
            ?? throw new NotFoundException($"Parameter value {id} was not found.");
        return r.StatusCode == S.Pending ? r : throw new BusinessRuleException("This value is no longer waiting for approval.");
    }

    private void Decide(LndParameterValue r, string status, string? notes)
    {
        r.StatusCode = status;
        r.DecidedBy = User;
        r.DecidedAt = clock.UtcNow;
        r.DecisionNotes = notes;
    }

    private static string? Blank(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
