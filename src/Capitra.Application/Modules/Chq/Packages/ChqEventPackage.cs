using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Chq.Packages;

/// <summary>An event of FND_CHEQUE_EVENT_PKG.register_event / move_cheques: the parameters after the cheque (or cheques).</summary>
public sealed record ChqEventArgs
{
    public required string EventType { get; init; }
    public required string? ToLocationCode { get; init; }
    public DateTime? EventAt { get; init; }
    public DateTime? EffectiveAt { get; init; }
    public long? DepositBatchId { get; init; }
    public decimal? EventAmount { get; init; }
    public string? CurrencyCode { get; init; }
    public string? RejectionReasonCode { get; init; }
    public string? BankRef { get; init; }
    public long? DepositBankId { get; init; }
    public long? DepositBankAccountId { get; init; }
    public string? NewStatusCode { get; init; }
    public string? Notes { get; init; }
    public string? MovementRef { get; init; }
    public string? ToCustodianUser { get; init; }
    public string? HandoverBy { get; init; }
    public string? ReceivedBy { get; init; }
    public string? CreatedBy { get; init; }
    public long? MovementGroupId { get; init; }
    public long? SourceAppId { get; init; }
    public long? SourcePageId { get; init; }
    public string? SourceSessionId { get; init; }
}

/// <summary>
/// FND_CHEQUE_EVENT_PKG: a cheque moves by events. Each event checks the target location and the status transition,
/// is written to CHQ_EVENTS and sets the cheque's status, location, custodian and dates. move_cheques registers the
/// same event for several cheques under one movement (CHQ_EVENT_GROUPS).
/// </summary>
public sealed class ChqEventPackage(ICapitraDbContext db, IApexSqlReader reader, ICurrentUser user, IClock clock)
{
    private static readonly string[] EventTypes =
        ["RECEIVED", "DEPOSITED", "COLLECTED", "REJECTED", "WITHDRAWN", "REPLACEMENT_ISSUED", "SETTLED_TRANSFER", "SETTLED_INTERNAL", "CANCELLED", "REDEPOSITED", "LOCATION_TRANSFER"];

    private DateTime Now => DateTime.SpecifyKind(clock.UtcNow, DateTimeKind.Utc);

    private static string Norm(string? eventType) => (eventType ?? string.Empty).Trim().ToUpperInvariant();

    /// <summary>derive_status: the status an event gives the cheque.</summary>
    public static string? DeriveStatus(string eventType, string? oldStatus) => Norm(eventType) switch
    {
        "RECEIVED" => "RECEIVED",
        "DEPOSITED" or "REDEPOSITED" => "DEPOSITED",
        "COLLECTED" => "COLLECTED",
        "REJECTED" => "REJECTED",
        "WITHDRAWN" => "WITHDRAWN",
        "REPLACEMENT_ISSUED" => "REPLACED",
        "SETTLED_TRANSFER" or "SETTLED_INTERNAL" => "SETTLED",
        "CANCELLED" => "CANCELLED",
        _ => oldStatus,
    };

    /// <summary>assert_location: the target location is required and must be an active location.</summary>
    public async Task AssertLocationAsync(string? locationCode, CancellationToken ct)
    {
        if (locationCode is null)
            throw new ApexProcessException("ORA-20200: Target cheque location is required.");
        if (!await db.Set<ChqLkpLocation>().AnyAsync(l => l.LocationCode == locationCode && l.IsActive, ct))
            throw new ApexProcessException("ORA-20201: Invalid or inactive cheque location: " + locationCode);
    }

    /// <summary>validate_event_data: a rejected cheque needs its rejection reason (the deposit batch stays optional).</summary>
    public static void ValidateEventData(string eventType, string? rejectionReasonCode)
    {
        if (Norm(eventType) == "REJECTED" && rejectionReasonCode is null)
            throw new ApexProcessException("ORA-20203: Rejection reason is required for rejected cheques.");
    }

    /// <summary>
    /// validate_transition: the event must be known and allowed from the cheque's status. As in the PL/SQL, a null
    /// status passes the "not in" and "&lt;&gt;" rules (a comparison with null is not true).
    /// </summary>
    public static void ValidateTransition(long chequeId, string? oldStatus, string eventType)
    {
        var e = Norm(eventType);
        var old = oldStatus?.Trim().ToUpperInvariant();
        if (!EventTypes.Contains(e))
            throw new ApexProcessException("ORA-20204: Invalid cheque event type: " + eventType);
        if (old is "SETTLED" or "CANCELLED" or "REPLACED" && e != "LOCATION_TRANSFER")
            throw new ApexProcessException($"ORA-20205: Cheque {chequeId} is already final with status {oldStatus}. Only location transfer is allowed.");
        bool NotIn(params string[] values) => old is not null && !values.Contains(old);
        switch (e)
        {
            case "RECEIVED" when NotIn("NEW", "CREATED"):
                throw new ApexProcessException($"ORA-20206: Cheque {chequeId} cannot be received from status {oldStatus}");
            case "DEPOSITED" when NotIn("RECEIVED", "WITHDRAWN"):
                throw new ApexProcessException($"ORA-20207: Cheque {chequeId} cannot be deposited from status {oldStatus}");
            case "REDEPOSITED" when NotIn("REJECTED", "WITHDRAWN"):
                throw new ApexProcessException($"ORA-20208: Cheque {chequeId} cannot be redeposited from status {oldStatus}");
            case "COLLECTED" or "REJECTED" or "WITHDRAWN" when NotIn("DEPOSITED"):
                throw new ApexProcessException($"ORA-20209: Cheque {chequeId} cannot execute event {e} from status {oldStatus}");
            case "REPLACEMENT_ISSUED" when NotIn("REJECTED"):
                throw new ApexProcessException("ORA-20210: Replacement can only be issued for a rejected cheque.");
            case "SETTLED_TRANSFER" or "SETTLED_INTERNAL" when NotIn("COLLECTED"):
                throw new ApexProcessException("ORA-20211: Settlement can only be done after cheque collection.");
        }
    }

    /// <summary>
    /// validate_transition_path: the one active action rule of the catalog for the action, event, target location,
    /// status and from location (one of the rule's comma-separated locations); returns its package procedure name.
    /// </summary>
    public async Task<string?> ValidateTransitionPathAsync(string? actionCode, string? targetLocationCode, string? fromStatus, string? fromLocation,
        string? eventType, CancellationToken ct)
    {
        static string? Up(string? v) => v?.Trim().ToUpperInvariant();
        var rules = await db.Set<ChqActionCatalog>().AsNoTracking().Where(c => c.ActionCode == actionCode && c.IsActive).ToListAsync(ct);
        var from = "," + (fromLocation?.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant() ?? string.Empty) + ",";
        var match = rules.Where(c => Up(c.EventType) is { } t && t == eventType
                                     && Up(c.TargetLocationCode) is { } to && to == Up(targetLocationCode)
                                     && Up(c.AllowedFromStatus) is { } st && st == Up(fromStatus)
                                     && ("," + (c.AllowedFromLocation?.Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant() ?? string.Empty) + ",")
                                         .Contains(from, StringComparison.Ordinal))
            .ToList();
        return match.Count switch
        {
            0 => throw new ApexProcessException($"ORA-20001: Action {actionCode} is not allowed for Event - {eventType}, Status={fromStatus}, From Location={fromLocation}, Target Location={targetLocationCode}"),
            1 => match[0].PackageProcedureName,
            _ => throw new ApexProcessException($"ORA-20002: More than one action rule matched action {actionCode}. Please check FND_CHEQUE_ACTION_CATALOG."),
        };
    }

    /// <summary>register_event: one event of one cheque; returns the event id.</summary>
    public async Task<long> RegisterEventAsync(long? chequeId, ChqEventArgs a, CancellationToken ct)
    {
        if (chequeId is null)
            throw new ApexProcessException("ORA-20212: Cheque ID is required.");
        var eventType = Norm(a.EventType);
        var eventAt = a.EventAt ?? Now;
        var createdBy = a.CreatedBy ?? user.UserName;

        await AssertLocationAsync(a.ToLocationCode, ct);
        ValidateEventData(eventType, a.RejectionReasonCode);

        var c = await db.Set<ChqCheque>().FirstOrDefaultAsync(x => x.ChequeId == chequeId, ct)
            ?? throw new ApexProcessException("ORA-20213: Cheque not found: " + chequeId.Value.ToString(CultureInfo.InvariantCulture));
        var oldStatus = c.StatusCode;
        var newStatus = a.NewStatusCode ?? DeriveStatus(eventType, oldStatus);

        ValidateTransition(chequeId.Value, oldStatus, eventType);

        var eventId = await NextIdAsync("CHQ_EVENTS", "EVENT_ID", ct);
        db.Set<ChqEvent>().Add(new ChqEvent
        {
            EventId = eventId,
            ChequeId = chequeId.Value,
            DepositBatchId = a.DepositBatchId,
            EventType = eventType,
            EventAt = eventAt,
            EffectiveAt = a.EffectiveAt,
            EventAmount = a.EventAmount ?? c.ChequeAmount,
            CurrencyCode = a.CurrencyCode ?? c.CurrencyCode,
            RejectionReasonCode = a.RejectionReasonCode,
            BankRef = a.BankRef,
            NewStatusCode = newStatus,
            OldStatusCode = oldStatus,
            IsGlPosted = false,
            Notes = a.Notes,
            CreatedBy = createdBy ?? string.Empty,
            MovementGroupId = a.MovementGroupId,
            FromLocationCode = c.CurrentLocationCode,
            ToLocationCode = a.ToLocationCode,
            FromCustodianUser = c.CurrentCustodianUser,
            ToCustodianUser = a.ToCustodianUser,
            HandoverBy = a.HandoverBy,
            ReceivedBy = a.ReceivedBy,
            ReceivedAt = a.ReceivedBy is not null ? Now : null,
            MovementRef = a.MovementRef,
            SourceAppId = a.SourceAppId,
            SourcePageId = a.SourcePageId,
            SourceSessionId = a.SourceSessionId,
        });

        c.StatusCode = newStatus ?? c.StatusCode;
        c.CurrentLocationCode = a.ToLocationCode;
        c.CurrentCustodianUser = a.ToCustodianUser;
        c.LastEventId = eventId;
        c.LastEventAt = eventAt;
        c.DepositBankId = a.DepositBankId;
        c.DepositBankAccountId = a.DepositBankAccountId;
        if (eventType == "RECEIVED")
            c.ReceivedAt = eventAt;
        if (eventType is "DEPOSITED" or "REDEPOSITED")
        {
            c.DepositAt = eventAt;
            c.DepositBatchId = a.DepositBatchId;
        }

        if (eventType == "COLLECTED")
            c.CollectionAt = eventAt;
        if (eventType == "REJECTED")
        {
            c.RejectionAt = eventAt;
            c.RejectionReasonCode = a.RejectionReasonCode;
            c.RejectionNotes = a.Notes;
        }

        if (eventType == "WITHDRAWN")
            c.WithdrawalAt = eventAt;
        await db.SaveChangesAsync(ct);
        return eventId;
    }

    /// <summary>move_cheques: the same event for the cheques (ids separated by , ; or :) under one new movement.</summary>
    public async Task<(long GroupId, int Count)> MoveChequesAsync(string? chequeIds, ChqEventArgs a, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(chequeIds))
            throw new ApexProcessException("ORA-20214: No cheques selected.");
        var createdBy = a.CreatedBy ?? user.UserName;
        var eventType = Norm(a.EventType);
        var groupId = await NextIdAsync("CHQ_EVENT_GROUPS", "MOVEMENT_GROUP_ID", ct);
        var eventAt = a.EventAt ?? Now;
        db.Set<ChqEventGroup>().Add(new ChqEventGroup
        {
            MovementGroupId = groupId,
            EventType = eventType,
            EventAt = eventAt,
            EffectiveAt = a.EffectiveAt,
            DepositBatchId = a.DepositBatchId,
            ToLocationCode = a.ToLocationCode,
            MovementRef = a.MovementRef,
            Notes = a.Notes,
        });
        await db.SaveChangesAsync(ct);

        var count = 0;
        foreach (var id in chequeIds.Replace(',', ':').Replace(';', ':').Split(':', StringSplitOptions.RemoveEmptyEntries))
        {
            if (!decimal.TryParse(id.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var n))
                throw new ApexProcessException("ORA-01722: invalid number");
            await RegisterEventAsync((long)n, a with { EventType = eventType, EventAt = eventAt, CreatedBy = createdBy, MovementGroupId = groupId }, ct);
            count++;
        }

        return (groupId, count);
    }

    /// <summary>&lt;table&gt;_SEQ.NEXTVAL: the next value of the table's identity, taken before the row is written (refs are built from it).</summary>
    public async Task<long> NextIdAsync(string table, string column, CancellationToken ct) =>
        (long)(await reader.NumberAsync($"select nextval(pg_get_serial_sequence('\"{table}\"', '{column}'))", null, ct))!.Value;
}
