using System.Text.Json;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Chq.Packages;
using Capitra.Application.Security;
using Capitra.Domain.Modules.Chq;
using Microsoft.EntityFrameworkCore;
using static Capitra.Application.Modules.Chq.Pages.Custom.ChqAjax;

namespace Capitra.Application.Modules.Chq.Pages.Custom;

/// <summary>Page 121, CHQ_BATCH_ACT: SAVE (the batch fields, then validate), VALIDATE, SUBMIT or DELETE a batch (x01 action, x02 batch).</summary>
public sealed class P0121BatchAction(ICapitraDbContext db, ChqUploadPackage upload, ICurrentUser user) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 121;

    public string Name => "CHQ_BATCH_ACT";

    public string Permission => Permissions.Chq.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => ChqAjax.RunAsync(db, async () =>
    {
        var act = request.X01?.ToUpperInvariant();
        var id = Long(request.X02);
        var batch = await db.Set<ChqUploadBatch>().FirstOrDefaultAsync(b => b.BatchId == id, ct)
            ?? throw new ApexProcessException("ORA-01403: no data found");
        if (act is "SAVE" or "VALIDATE" or "DELETE" && batch.BatchStatus == "SUBMITTED")
            throw new ApexProcessException("ORA-20160: The batch is submitted and can no longer be changed.");
        int? count = null;
        switch (act)
        {
            case "SAVE":
                // same as the former "Update Batch And Revalidate" button: save the batch fields, then validate_batch
                batch.CurrencyCode = request.X03 is { Length: > 0 } cur ? cur : batch.CurrencyCode;
                batch.DefaultLocationCode = request.X04 is { Length: > 0 } loc ? loc : null;
                batch.SourceModule = request.X05 is { Length: > 0 } src ? src : batch.SourceModule;
                batch.Notes = Substr(request.X06, 2000);
                await db.SaveChangesAsync(ct);
                await upload.ValidateBatchAsync(id, ct);
                break;
            case "VALIDATE":
                await upload.ValidateBatchAsync(id, ct);
                break;
            case "SUBMIT":
                count = await upload.SubmitBatchAsync(id, user.UserName, "Y", new ChqSource(ChqAjax.App, 121, null), ct);
                break;
            case "DELETE":
                if (await db.Set<ChqUploadLine>().AnyAsync(l => l.BatchId == id && (l.LineStatus == "SUBMITTED" || l.TargetChequeId != null), ct))
                    throw new ApexProcessException("ORA-20161: Cheques were already created from this batch, so it cannot be deleted.");
                db.Set<ChqUploadLine>().RemoveRange(await db.Set<ChqUploadLine>().Where(l => l.BatchId == id).ToListAsync(ct));
                db.Set<ChqUploadBatch>().Remove(batch);
                await db.SaveChangesAsync(ct);
                break;
            default:
                throw new ApexProcessException("ORA-20162: Unknown action " + act);
        }

        var res = new Json { { "ok", true }, { "count", count ?? 0 } };
        if (act != "DELETE")
        {
            var b = new Json();
            if (await db.Set<ChqUploadBatch>().AsNoTracking().FirstOrDefaultAsync(x => x.BatchId == id, ct) is { } r)
            {
                b.Add("valid", r.ValidRows ?? 0);
                b.Add("errors", r.ErrorRows ?? 0);
                b.Add("status", r.BatchStatus);
                b.Add("glTranSetId", r.GlTranSetId);
            }

            res.Add("batch", b);
        }

        return res;
    }, ct);
}

/// <summary>Page 122 "Upload Cheques", CHQ_CHECK: every check of the Excel file (x01 file name, clob base64 content); nothing is saved.</summary>
public sealed class P0122Check(ChqUploadPackage upload) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 122;

    public string Name => "CHQ_CHECK";

    public string Permission => Permissions.Chq.Edit;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        try
        {
            return await upload.CheckFileAsync(P0122Create.File(request.Clob01), request.X01, ct);
        }
        catch (Exception e) when (e is not OperationCanceledException)
        {
            // check_file: any error as {ok: false, message}
            return new Json { { "ok", false }, { "message", Message(e) } };
        }
    }
}

/// <summary>
/// Page 122, CHQ_CREATE: the file again through every check, then its batch, lines and validation
/// (x01 file name, x02 currency, x03 location, x04 source module, x05 notes, clob base64 content).
/// </summary>
public sealed class P0122Create(ICapitraDbContext db, ChqUploadPackage upload, ICurrentUser user) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 122;

    public string Name => "CHQ_CREATE";

    public string Permission => Permissions.Chq.Edit;

    /// <summary>apex_web_service.clobbase642blob.</summary>
    internal static byte[]? File(string? base64) =>
        string.IsNullOrEmpty(base64) ? null : Convert.FromBase64String(string.Concat(base64.Where(c => !char.IsWhiteSpace(c))));

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => ChqAjax.RunAsync(db, async () =>
    {
        var (batch, valid, errors) = await upload.ImportFileAsync(File(request.Clob01), request.X01, 1,
            request.X02 is { Length: > 0 } cur ? cur : "EGP",
            request.X04 is { Length: > 0 } src ? src : "FACTORING",
            request.X03 is { Length: > 0 } loc ? loc : "COMPANY_VAULT",
            request.X05 is { Length: > 0 } notes ? notes : null, user.UserName, ct);
        var no = await db.Set<ChqUploadBatch>().Where(b => b.BatchId == batch).Select(b => b.BatchNo).FirstAsync(ct);
        return new Json
        {
            { "ok", true },
            { "batchId", batch },
            { "batchNo", no },
            { "valid", valid },
            { "errors", errors },
            { "url", Url(121, "P121_BATCH_ID", batch) },
        };
    }, ct);
}

/// <summary>Page 131, CHQ_CHEQUE_ACT: SAVE the cheque's fields (x02 cheque, clob its JSON) or DELETE a cheque that never moved.</summary>
public sealed class P0131ChequeAction(ICapitraDbContext db) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 131;

    public string Name => "CHQ_CHEQUE_ACT";

    public string Permission => Permissions.Chq.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => ChqAjax.RunAsync(db, async () =>
    {
        var act = request.X01?.ToUpperInvariant();
        var id = Long(request.X02);
        switch (act)
        {
            case "SAVE":
                using (var doc = JsonDocument.Parse(string.IsNullOrEmpty(request.Clob01) ? "{}" : request.Clob01))
                {
                    var j = doc.RootElement;
                    // apex_json.get_varchar2: the value as text, null when missing, null or empty
                    string? G(string key) => j.ValueKind == JsonValueKind.Object && j.TryGetProperty(key, out var v) && v.ValueKind is not (JsonValueKind.Null or JsonValueKind.Undefined)
                        ? v.ValueKind == JsonValueKind.String ? v.GetString() is { Length: > 0 } s ? s : null : v.GetRawText()
                        : null;
                    static T Req<T>(T? value, string column) where T : struct =>
                        value ?? throw new ApexProcessException($"ORA-01407: cannot update (\"FND_CHEQUE\".\"{column}\") to NULL");
                    static string ReqText(string? value, string column) =>
                        value ?? throw new ApexProcessException($"ORA-01407: cannot update (\"FND_CHEQUE\".\"{column}\") to NULL");

                    var bankCode = G("bank");
                    var bankName = await db.Set<ChqLkpIssuerBank>().Where(b => b.BankCode == bankCode).Select(b => b.BankNameEn).FirstOrDefaultAsync(ct);
                    var c = await db.Set<ChqCheque>().FirstOrDefaultAsync(x => x.ChequeId == id, ct);
                    if (c is null)
                        break;
                    c.ChequeNo = ReqText(Substr(G("chequeNo"), 100), "CHEQUE_NO");
                    c.ChequeAt = Req(Date(G("chequeDate")), "CHEQUE_DATE");
                    c.DueDate = DateOnly.FromDateTime(Req(Date(G("dueIso")), "DUE_DATE"));
                    c.ChequeAmount = Req(Number(G("amount")), "CHEQUE_AMOUNT");
                    c.CurrencyCode = ReqText(Substr(G("currency"), 100), "CURRENCY_CODE");
                    c.LegalEntityId = Req(Long(G("legalEntity")), "LEGAL_ENTITY_ID");
                    c.IssuerBankCode = Substr(G("bank"), 100);
                    c.IssuerBankBranch = Substr(G("bankBranch"), 200);
                    c.IssuerAccountNo = Substr(G("issuerAccount"), 100);
                    c.DrawerNameEn = ReqText(Substr(G("drawer"), 300), "DRAWER_NAME_EN");
                    c.DrawerNameAr = Substr(G("drawerAr"), 300);
                    c.DrawerNationalId = Substr(G("drawerId"), 50);
                    c.DrawerAccountNo = Substr(G("drawerAccount"), 100);
                    c.CustomerId = Substr(G("customerId"), 100);
                    c.PartyTaxId = Substr(G("taxId"), 20);
                    c.CustomerNameEn = Substr(G("customer"), 300);
                    c.CustomerNameAr = Substr(G("customerAr"), 300);
                    c.SourceModule = Substr(G("source"), 30);
                    c.SourceRefType = Substr(G("refType"), 50);
                    c.SourceRefId = Substr(G("refId"), 100);
                    c.DrawdownId = Long(G("drawdown"));
                    c.ScheduleId = Long(G("schedule"));
                    c.PaymentId = Long(G("payment"));
                    c.StatusCode = ReqText(Substr(G("status"), 100), "STATUS_CODE");
                    c.CurrentLocationCode = Substr(G("loc"), 100);
                    c.CurrentCustodianUser = Substr(G("custodian"), 100);
                    c.DepositBankAccountId = Long(G("depositAccount"));
                    c.DepositBatchId = Long(G("depositBatch"));
                    c.ReceivedAt = Req(Date(G("receivedIso")), "RECEIVED_DT");
                    c.DepositAt = Date(G("depositIso"));
                    c.CollectionAt = Date(G("collectionIso"));
                    c.RejectionAt = Date(G("rejectionIso"));
                    c.WithdrawalAt = Date(G("withdrawalIso"));
                    c.RejectionReasonCode = Substr(G("reason"), 100);
                    c.RejectionNotes = Substr(G("rejectionNotes"), 2000);
                    c.IsReplaced = Substr(G("replaced"), 100) == "Y";
                    c.ReplacementChequeId = Long(G("replacementId"));
                    c.Notes = Substr(G("notes"), 2000);
                    c.IssuerBankName = bankName ?? c.IssuerBankName;
                    await db.SaveChangesAsync(ct);
                }

                break;
            case "DELETE":
                var n = await db.Set<ChqEvent>().CountAsync(e => e.ChequeId == id, ct);
                if (n > 0)
                    throw new ApexProcessException($"ORA-20180: This cheque has {n} movement(s) and cannot be deleted.");
                await db.Set<ChqCheque>().Where(x => x.ChequeId == id).ExecuteDeleteAsync(ct);
                break;
            default:
                throw new ApexProcessException("ORA-20181: Unknown action " + act);
        }

        return new Json { { "ok", true } };
    }, ct);
}

/// <summary>
/// Page 140, CHQ_MV_RUN: one action of the catalog on one cheque (x01 cheque, x02 action, x03 bank, x04 bank account),
/// after its rule is checked; only SEND_TO_TREASURY and DEPOSIT_TO_BANK run from this screen.
/// </summary>
public sealed class P0140MoveRun(ICapitraDbContext db, ChqEventPackage events, ChqActionPackage actions, ICurrentUser user) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 140;

    public string Name => "CHQ_MV_RUN";

    public string Permission => Permissions.Chq.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => ChqAjax.RunAsync(db, async () =>
    {
        var cheque = request.X01 is { Length: > 0 } x1 ? x1 : null;
        var action = request.X02 is { Length: > 0 } x2 ? x2 : null;
        var bank = request.X03 is { Length: > 0 } x3 ? x3 : null;
        var account = request.X04 is { Length: > 0 } x4 ? x4 : null;
        if (cheque is null)
            throw new ApexProcessException("ORA-20005: Please choose the cheque.");
        if (action is null)
            throw new ApexProcessException("ORA-20006: Please choose the action.");
        // validations of page 4 (Bank / Bank account required for deposit)
        if (action == "DEPOSIT_TO_BANK" && bank is null)
            throw new ApexProcessException("ORA-20007: Please select a bank.");
        if (action == "DEPOSIT_TO_BANK" && account is null)
            throw new ApexProcessException("ORA-20008: Please select a bank account.");

        var chequeId = Long(cheque);
        var c = await db.Set<ChqCheque>().AsNoTracking().Where(x => x.ChequeId == chequeId)
                    .Select(x => new { x.StatusCode, x.CurrentLocationCode }).FirstOrDefaultAsync(ct)
                ?? throw new ApexProcessException("ORA-20002: Specified cheque ID was not found.");

        var targets = await db.Set<ChqActionCatalog>().AsNoTracking()
            .Where(k => k.ActionCode == action && k.IsActive && k.TargetLocationCode != null)
            .Select(k => new { k.TargetLocationCode, k.EventType }).Distinct().ToListAsync(ct);
        var target = targets.Count switch
        {
            0 => throw new ApexProcessException("ORA-20001: No target location is configured for action " + action),
            1 => targets[0],
            _ => throw new ApexProcessException("ORA-20002: More than one target location is configured for action " + action),
        };

        var procedure = await events.ValidateTransitionPathAsync(action, target.TargetLocationCode, c.StatusCode, c.CurrentLocationCode, target.EventType, ct);
        var source = new ChqSource(ChqAjax.App, 140, null);
        (long GroupId, int Count) moved = procedure switch
        {
            "SEND_TO_TREASURY" => await actions.SendToTreasuryAsync(cheque, null, user.UserName, source, ct),
            "DEPOSIT_TO_BANK" => await actions.DepositToBankAsync(cheque, null, null, Long(bank), Long(account), null, user.UserName, source, ct),
            _ => throw new ApexProcessException($"ORA-20004: The action {action} passed the rules, but its procedure ({procedure ?? "none"}) cannot be executed from this screen. Only SEND_TO_TREASURY and DEPOSIT_TO_BANK can."),
        };

        var now = await db.Set<ChqCheque>().AsNoTracking().Where(x => x.ChequeId == chequeId)
            .Select(x => new { x.StatusCode, x.CurrentLocationCode, x.CurrentCustodianUser }).FirstAsync(ct);
        var statusName = await db.Set<ChqLkpStatus>().Where(s => s.StatusCode == now.StatusCode).Select(s => s.NameEn).FirstOrDefaultAsync(ct);
        var locationName = await db.Set<ChqLkpLocation>().Where(l => l.LocationCode == now.CurrentLocationCode).Select(l => l.LocationName).FirstOrDefaultAsync(ct);
        return new Json
        {
            { "ok", true },
            { "status", now.StatusCode },
            { "statusName", statusName ?? now.StatusCode },
            { "loc", now.CurrentLocationCode },
            { "locName", locationName ?? now.CurrentLocationCode },
            { "heldBy", now.CurrentCustodianUser },
            { "count", moved.Count },
            { "groupId", moved.GroupId },
            { "moveUrl", Url(141, "P141_MOVEMENT_GROUP_ID", moved.GroupId) },
        };
    }, ct);
}

/// <summary>Page 141, CHQ_MOVE_ACT: SAVE a movement's reference, effective date and notes, or DELETE a movement without cheques.</summary>
public sealed class P0141MoveAction(ICapitraDbContext db) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 141;

    public string Name => "CHQ_MOVE_ACT";

    public string Permission => Permissions.Chq.Edit;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) => ChqAjax.RunAsync(db, async () =>
    {
        var act = request.X01?.ToUpperInvariant();
        var id = Long(request.X02);
        switch (act)
        {
            case "SAVE":
                if (await db.Set<ChqEventGroup>().FirstOrDefaultAsync(g => g.MovementGroupId == id, ct) is { } g)
                {
                    g.MovementRef = Substr(request.X03, 200);
                    g.EffectiveAt = Date(request.X04);
                    g.Notes = Substr(request.X05, 2000);
                    await db.SaveChangesAsync(ct);
                }

                break;
            case "DELETE":
                var n = await db.Set<ChqEvent>().CountAsync(e => e.MovementGroupId == id, ct);
                if (n > 0)
                    throw new ApexProcessException($"ORA-20170: This movement has {n} cheque(s) and cannot be deleted.");
                await db.Set<ChqEventGroup>().Where(g => g.MovementGroupId == id).ExecuteDeleteAsync(ct);
                break;
            default:
                throw new ApexProcessException("ORA-20171: Unknown action " + act);
        }

        return new Json { { "ok", true } };
    }, ct);
}
