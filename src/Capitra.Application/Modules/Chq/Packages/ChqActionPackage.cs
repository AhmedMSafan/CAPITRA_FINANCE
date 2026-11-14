using System.Globalization;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;

namespace Capitra.Application.Modules.Chq.Packages;

/// <summary>The source of an action: the APEX app, page and session that ran it (written on each event).</summary>
public sealed record ChqSource(long? AppId, long? PageId, string? SessionId);

/// <summary>
/// FND_CHEQUE_ACTION_PKG: the named moves of the action catalog (PACKAGE_PROCEDURE_NAME), each a move_cheques with
/// its event, target location, custodian and movement reference.
/// </summary>
public sealed class ChqActionPackage(ChqEventPackage events, ICurrentUser user, IClock clock)
{
    private string Stamp => DateTime.SpecifyKind(clock.UtcNow, DateTimeKind.Utc).ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);

    private string? By(string? createdBy) => createdBy ?? user.UserName;

    /// <summary>send_to_treasury: Operations hand received cheques to Treasury.</summary>
    public Task<(long GroupId, int Count)> SendToTreasuryAsync(string? chequeIds, string? notes, string? createdBy, ChqSource source, CancellationToken ct) =>
        events.MoveChequesAsync(chequeIds, new ChqEventArgs
        {
            EventType = "LOCATION_TRANSFER",
            ToLocationCode = "TREASURY",
            Notes = notes ?? "Operations sent received cheques to Treasury.",
            MovementRef = "OPS-TO-TRS-" + Stamp,
            ToCustodianUser = "TREASURY_USER",
            HandoverBy = By(createdBy),
            ReceivedBy = "TREASURY_USER",
            CreatedBy = By(createdBy),
            SourceAppId = source.AppId,
            SourcePageId = source.PageId,
            SourceSessionId = source.SessionId,
        }, ct);

    /// <summary>deposit_to_bank: Treasury deposits cheques at the bank for collection.</summary>
    public Task<(long GroupId, int Count)> DepositToBankAsync(string? chequeIds, long? depositBatchId, string? bankRef, long? depositBankId, long? depositBankAccountId,
        string? notes, string? createdBy, ChqSource source, CancellationToken ct) =>
        events.MoveChequesAsync(chequeIds, new ChqEventArgs
        {
            EventType = "DEPOSITED",
            ToLocationCode = "BANK",
            DepositBatchId = depositBatchId,
            BankRef = bankRef,
            DepositBankId = depositBankId,
            DepositBankAccountId = depositBankAccountId,
            Notes = notes ?? "Treasury deposited cheques to bank for collection.",
            MovementRef = bankRef ?? "BANK-DEP-" + Stamp,
            ToCustodianUser = "BANK",
            HandoverBy = By(createdBy),
            ReceivedBy = "BANK",
            CreatedBy = By(createdBy),
            SourceAppId = source.AppId,
            SourcePageId = source.PageId,
            SourceSessionId = source.SessionId,
        }, ct);

    /// <summary>mark_collected: the bank statement shows the cheques collected.</summary>
    public Task<(long GroupId, int Count)> MarkCollectedAsync(string? chequeIds, string? bankRef, string? notes, string? createdBy, ChqSource source, CancellationToken ct) =>
        events.MoveChequesAsync(chequeIds, new ChqEventArgs
        {
            EventType = "COLLECTED",
            ToLocationCode = "BANK",
            BankRef = bankRef,
            Notes = notes ?? "Cheque collected according to bank statement.",
            MovementRef = bankRef ?? "BANK-COLL-" + Stamp,
            ToCustodianUser = "BANK",
            HandoverBy = By(createdBy),
            ReceivedBy = "BANK",
            CreatedBy = By(createdBy),
            SourceAppId = source.AppId,
            SourcePageId = source.PageId,
            SourceSessionId = source.SessionId,
        }, ct);

    /// <summary>mark_rejected: the bank returns the cheques rejected; they go back to Treasury.</summary>
    public Task<(long GroupId, int Count)> MarkRejectedAsync(string? chequeIds, string? rejectionReasonCode, string? bankRef, string? notes, string? createdBy,
        ChqSource source, CancellationToken ct)
    {
        if (rejectionReasonCode is null)
            throw new ApexProcessException("ORA-20301: Rejection reason is required.");
        return events.MoveChequesAsync(chequeIds, new ChqEventArgs
        {
            EventType = "REJECTED",
            ToLocationCode = "TREASURY",
            RejectionReasonCode = rejectionReasonCode,
            BankRef = bankRef,
            Notes = notes ?? "Cheque rejected according to bank statement.",
            MovementRef = bankRef ?? "BANK-REJ-" + Stamp,
            ToCustodianUser = "TREASURY_USER",
            HandoverBy = "BANK",
            ReceivedBy = "TREASURY_USER",
            CreatedBy = By(createdBy),
            SourceAppId = source.AppId,
            SourcePageId = source.PageId,
            SourceSessionId = source.SessionId,
        }, ct);
    }

    /// <summary>send_to_legal: Operations send non-performing or rejected cheques to the Legal Department.</summary>
    public Task<(long GroupId, int Count)> SendToLegalAsync(string? chequeIds, string? notes, string? createdBy, ChqSource source, CancellationToken ct) =>
        events.MoveChequesAsync(chequeIds, new ChqEventArgs
        {
            EventType = "LOCATION_TRANSFER",
            ToLocationCode = "LEGAL",
            Notes = notes ?? "Operations sent NPL/rejected cheques to Legal Department.",
            MovementRef = "OPS-TO-LEGAL-" + Stamp,
            ToCustodianUser = "LEGAL_USER",
            HandoverBy = By(createdBy),
            ReceivedBy = "LEGAL_USER",
            CreatedBy = By(createdBy),
            SourceAppId = source.AppId,
            SourcePageId = source.PageId,
            SourceSessionId = source.SessionId,
        }, ct);
}
