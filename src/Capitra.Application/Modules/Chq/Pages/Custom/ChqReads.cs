using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Security;
using static Capitra.Application.Modules.Chq.Pages.Custom.ChqAjax;

namespace Capitra.Application.Modules.Chq.Pages.Custom;

/// <summary>Page 130 "Cheques Wallet", CHQ_WALLET: every cheque with its status, location and batch.</summary>
public sealed class P0130Wallet(IApexSqlReader reader) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 130;

    public string Name => "CHQ_WALLET";

    public string Permission => Permissions.Chq.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var res = new Json { { "ok", true }, { "uploadUrl", Url(122) } };
        res.Add("statuses", (await reader.QueryAsync(ChqApexPageSql.CHQ_STATUS_CODES, NoItems, ct)).Select(r => r.Text("STATUS_CODE")).ToList());
        res.Add("locations", await ChqLists.CodeNameAsync(reader, ChqApexPageSql.CHQ_LOCATIONS_ACTIVE, ct));
        res.Add("rows", (await reader.QueryAsync(ChqApexPageSql.CHQ_WALLET, NoItems, ct)).Select(r => new Json
        {
            { "ref", r.Text("CHEQUE_REF") },
            { "no", r.Text("CHEQUE_NO") },
            { "customerId", r.Text("CUSTOMER_ID") },
            { "customer", r.Text("CUSTOMER_NAME_EN") },
            { "drawer", r.Text("DRAWER_NAME_EN") },
            { "bank", r.Text("ISSUER_BANK_CODE") },
            { "bankName", r.Text("ISSUER_BANK_NAME") },
            { "due", Dmy(r["DUE_DATE"]) },
            { "dueIso", Iso(r["DUE_DATE"]) },
            { "amount", r.Number("CHEQUE_AMOUNT") },
            { "currency", r.Text("CURRENCY_CODE") },
            { "status", r.Text("STATUS_CODE") },
            { "statusName", r.Text("STATUS_NAME") },
            { "terminal", Yn(r["IS_TERMINAL"]) },
            { "loc", r.Text("CURRENT_LOCATION_CODE") },
            { "locName", r.Text("LOCATION_NAME") },
            { "custodian", r.Text("CURRENT_CUSTODIAN_USER") },
            { "batchNo", r.Text("BATCH_NO") },
            { "url", Url(131, "P131_CHEQUE_ID", r["CHEQUE_ID"]) },
        }).ToList());
        return res;
    }
}

/// <summary>Page 120 "Cheque Batch Upload", CHQ_BATCHES: the upload batches, newest first (1000 at most).</summary>
public sealed class P0120Batches(IApexSqlReader reader) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 120;

    public string Name => "CHQ_BATCHES";

    public string Permission => Permissions.Chq.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var rows = await reader.QueryAsync(ChqApexPageSql.CHQ_BATCHES, NoItems, ct);
        return new Json
        {
            { "ok", true },
            { "uploadUrl", Url(122) },
            {
                "rows", rows.Take(1000).Select(r => new Json
                {
                    { "id", r.Number("BATCH_ID") },
                    { "no", r.Text("BATCH_NO") },
                    { "status", r.Text("BATCH_STATUS") },
                    { "file", r.Text("FILE_NAME") },
                    { "customerId", r.Text("CUSTOMER_ID") },
                    { "customer", r.Text("CUSTOMER_NAME_EN") },
                    { "customerAr", r.Text("CUSTOMER_NAME_AR") },
                    { "currency", r.Text("CURRENCY_CODE") },
                    { "total", r.Number("TOTAL_ROWS") ?? 0 },
                    { "valid", r.Number("VALID_ROWS") ?? 0 },
                    { "errors", r.Number("ERROR_ROWS") ?? 0 },
                    { "submitted", r.Number("SUBMITTED_ROWS") ?? 0 },
                    { "amount", r.Number("AMOUNT") ?? 0 },
                    { "notes", r.Text("NOTES") },
                    { "createdBy", r.Text("CREATED_BY") },
                    { "created", DmyHm(r["CREATED_DT"]) },
                    { "submittedBy", r.Text("SUBMITTED_BY") },
                    { "submittedAt", DmyHm(r["SUBMITTED_DT"]) },
                    { "url", Url(121, "P121_BATCH_ID", r["BATCH_ID"]) },
                }).ToList()
            },
            { "more", rows.Count > 1000 },
        };
    }
}

/// <summary>Page 121 "Cheque Batch Details", CHQ_BATCH: one batch with its lines and the lists of its form.</summary>
public sealed class P0121Batch(IApexSqlReader reader) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 121;

    public string Name => "CHQ_BATCH";

    public string Permission => Permissions.Chq.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var id = string.IsNullOrEmpty(request.X01) ? request.Item("P121_BATCH_ID") : request.X01;
        var b = await reader.RowAsync(ChqApexPageSql.CHQ_BATCH, Id(Number(id)), ct);
        if (b is null)
            return new Json { { "ok", false }, { "message", "Batch " + id + " does not exist." } };
        return new Json
        {
            { "ok", true },
            { "listUrl", Url(120) },
            { "walletUrl", Url(130) },
            {
                "batch", new Json
                {
                    { "id", b.Number("BATCH_ID") },
                    { "no", b.Text("BATCH_NO") },
                    { "status", b.Text("BATCH_STATUS") },
                    { "file", b.Text("FILE_NAME") },
                    { "customerId", b.Text("CUSTOMER_ID") },
                    { "customer", b.Text("CUSTOMER_NAME_EN") },
                    { "customerAr", b.Text("CUSTOMER_NAME_AR") },
                    { "currency", b.Text("CURRENCY_CODE") },
                    { "location", b.Text("DEFAULT_LOCATION_CODE") },
                    { "source", b.Text("SOURCE_MODULE") },
                    { "notes", b.Text("NOTES") },
                    { "total", b.Number("TOTAL_ROWS") ?? 0 },
                    { "valid", b.Number("VALID_ROWS") ?? 0 },
                    { "errors", b.Number("ERROR_ROWS") ?? 0 },
                    { "submitted", b.Number("SUBMITTED_ROWS") ?? 0 },
                    { "amount", b.Number("AMOUNT") ?? 0 },
                    { "received", Dmy(b["RECEIVED_DT"]) },
                    { "chequeDate", Dmy(b["CHEQUE_DATE"]) },
                    { "created", DmyHm(b["CREATED_DT"]) },
                    { "createdBy", b.Text("CREATED_BY") },
                    { "updated", DmyHm(b["UPDATED_DT"]) },
                    { "updatedBy", b.Text("UPDATED_BY") },
                    { "submittedAt", DmyHm(b["SUBMITTED_DT"]) },
                    { "submittedBy", b.Text("SUBMITTED_BY") },
                    { "glTranSetId", b.Text("GL_TRAN_SET_ID") },
                }
            },
            {
                "lines", (await reader.QueryAsync(ChqApexPageSql.CHQ_BATCH_LINES, Id(b["BATCH_ID"]), ct)).Select(r => new Json
                {
                    { "no", r.Number("LINE_NO") },
                    { "status", r.Text("LINE_STATUS") },
                    { "error", r.Text("ERROR_MESSAGE") },
                    { "clientId", r.Text("CLIENT_ID") },
                    { "rawClient", r.Text("RAW_CLIENT_ID") },
                    { "taxId", r.Text("PARTY_TAX_ID") },
                    { "rawTax", r.Text("RAW_PARTY_TAX_ID") },
                    { "chequeNo", r.Text("CHEQUE_NO") },
                    { "rawChequeNo", r.Text("RAW_CHEQUE_NO") },
                    { "dueDate", Iso(r["DUE_DATE"]) },
                    { "rawDue", r.Text("RAW_DUE_DATE") },
                    { "amount", r.Number("CHEQUE_AMOUNT") },
                    { "rawAmount", r.Text("RAW_CHEQUE_AMOUNT") },
                    { "bank", r.Text("ISSUER_BANK_CODE") },
                    { "rawBank", r.Text("RAW_ISSUER_BANK_CODE") },
                    { "bankName", r.Text("ISSUER_BANK_NAME") },
                    { "chequeRef", r.Text("TARGET_CHEQUE_REF") },
                }).ToList()
            },
            { "currencies", await ChqLists.CodeNameAsync(reader, ChqApexPageSql.CHQ_CURRENCIES, ct) },
            { "locations", await ChqLists.CodeNameAsync(reader, ChqApexPageSql.CHQ_LOCATIONS_ACTIVE, ct) },
        };
    }
}

/// <summary>Page 122 "Upload Cheques", CHQ_INIT: the defaults and lists of the upload form.</summary>
public sealed class P0122Init(IApexSqlReader reader) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 122;

    public string Name => "CHQ_INIT";

    public string Permission => Permissions.Chq.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct) => new Json
    {
        { "ok", true },
        { "currency", "EGP" },
        { "location", "COMPANY_VAULT" },
        { "source", "FACTORING" },
        { "batchesUrl", Url(120) },
        { "currencies", await ChqLists.CodeNameAsync(reader, ChqApexPageSql.CHQ_CURRENCIES, ct) },
        { "locations", await ChqLists.CodeNameAsync(reader, ChqApexPageSql.CHQ_LOCATIONS_ACTIVE, ct) },
    };
}

/// <summary>Page 131 "Cheque Details", CHQ_CHEQUE: one cheque, its events and the lists of its form.</summary>
public sealed class P0131Cheque(IApexSqlReader reader) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 131;

    public string Name => "CHQ_CHEQUE";

    public string Permission => Permissions.Chq.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var id = string.IsNullOrEmpty(request.X01) ? request.Item("P131_CHEQUE_ID") : request.X01;
        var c = await reader.RowAsync(ChqApexPageSql.CHQ_CHEQUE, Id(Number(id)), ct);
        if (c is null)
            return new Json { { "ok", false }, { "message", "Cheque " + id + " does not exist." } };

        // nm(): select max(name) of the key (null when the key is null or not found)
        string? Nm(string name, string key) => c.Text(key) is null ? null : c.Text(name);
        var cheque = new Json
        {
            { "ref", c.Text("CHEQUE_REF") }, { "chequeNo", c.Text("CHEQUE_NO") },
            { "chequeDate", Iso(c["CHEQUE_DATE"]) }, { "chequeDateTxt", Dmy(c["CHEQUE_DATE"]) },
            { "due", Dmy(c["DUE_DATE"]) }, { "dueIso", Iso(c["DUE_DATE"]) },
            { "amount", c.Number("CHEQUE_AMOUNT") }, { "currency", c.Text("CURRENCY_CODE") },
            { "legalEntity", c.Text("LEGAL_ENTITY_ID") },
            { "legalEntityName", Nm("LEGAL_ENTITY_NAME", "LEGAL_ENTITY_ID") },
            { "bank", c.Text("ISSUER_BANK_CODE") }, { "bankName", c.Text("ISSUER_BANK_NAME") }, { "bankBranch", c.Text("ISSUER_BANK_BRANCH") }, { "issuerAccount", c.Text("ISSUER_ACCOUNT_NO") },
            { "drawer", c.Text("DRAWER_NAME_EN") }, { "drawerAr", c.Text("DRAWER_NAME_AR") }, { "drawerId", c.Text("DRAWER_NATIONAL_ID") }, { "drawerAccount", c.Text("DRAWER_ACCOUNT_NO") },
            { "customerId", c.Text("CUSTOMER_ID") }, { "customer", c.Text("CUSTOMER_NAME_EN") }, { "customerAr", c.Text("CUSTOMER_NAME_AR") }, { "taxId", c.Text("PARTY_TAX_ID") },
            { "source", c.Text("SOURCE_MODULE") }, { "refType", c.Text("SOURCE_REF_TYPE") }, { "refId", c.Text("SOURCE_REF_ID") },
            // the leasing drawdowns and repayments are not in Capitra: nm() returns the key when its query fails
            { "drawdown", c.Text("DRAWDOWN_ID") }, { "drawdownName", c.Text("DRAWDOWN_ID") },
            { "schedule", c.Text("SCHEDULE_ID") }, { "payment", c.Text("PAYMENT_ID") },
            { "paymentName", c.Text("PAYMENT_ID") },
            { "status", c.Text("STATUS_CODE") }, { "statusName", Nm("STATUS_NAME", "STATUS_CODE") },
            { "terminal", c.Text("STATUS_CODE") is null ? null : Yn(c["IS_TERMINAL"]) },
            { "loc", c.Text("CURRENT_LOCATION_CODE") }, { "locName", Nm("LOCATION_NAME", "CURRENT_LOCATION_CODE") },
            { "custodian", c.Text("CURRENT_CUSTODIAN_USER") },
            { "depositAccount", c.Text("DEPOSIT_BANK_ACCOUNT_ID") },
            { "depositAccountName", Nm("DEPOSIT_ACCOUNT_NAME", "DEPOSIT_BANK_ACCOUNT_ID") },
            { "depositBatch", c.Text("DEPOSIT_BATCH_ID") },
            { "depositBatchRef", Nm("DEPOSIT_BATCH_REF", "DEPOSIT_BATCH_ID") },
            { "received", Dmy(c["RECEIVED_DT"]) }, { "receivedIso", Iso(c["RECEIVED_DT"]) },
            { "deposit", Dmy(c["DEPOSIT_DT"]) }, { "depositIso", Iso(c["DEPOSIT_DT"]) },
            { "collection", Dmy(c["COLLECTION_DT"]) }, { "collectionIso", Iso(c["COLLECTION_DT"]) },
            { "rejection", Dmy(c["REJECTION_DT"]) }, { "rejectionIso", Iso(c["REJECTION_DT"]) },
            { "withdrawal", Dmy(c["WITHDRAWAL_DT"]) }, { "withdrawalIso", Iso(c["WITHDRAWAL_DT"]) },
            { "reason", c.Text("REJECTION_REASON_CODE") }, { "reasonName", Nm("REASON_NAME", "REJECTION_REASON_CODE") },
            { "rejectionNotes", c.Text("REJECTION_NOTES") }, { "replaced", Yn(c["IS_REPLACED"]) }, { "replacementId", c.Text("REPLACEMENT_CHEQUE_ID") },
            { "glPosted", Yn(c["GL_POSTED"]) }, { "glRef", c.Text("GL_JOURNAL_REF") }, { "glDate", Dmy(c["GL_POST_DT"]) },
            { "notes", c.Text("NOTES") },
            { "created", DmyHm(c["CREATED_DT"]) }, { "createdBy", c.Text("CREATED_BY") },
            { "updated", DmyHm(c["UPDATED_DT"]) }, { "updatedBy", c.Text("UPDATED_BY") },
        };
        if (c.Text("BATCH_ID") is { } batch)
        {
            cheque.Add("batchNo", c.Text("BATCH_NO"));
            cheque.Add("batchUrl", Url(121, "P121_BATCH_ID", batch));
        }

        var events = (await reader.QueryAsync(ChqApexPageSql.CHQ_CHEQUE_EVENTS, Id(c["CHEQUE_ID"]), ct)).Select(r =>
        {
            var e = new Json
            {
                { "type", r.Text("EVENT_TYPE") }, { "typeName", r.Text("EVENT_DESC_EN") }, { "date", DmyHm(r["EVENT_DT"]) },
                { "amount", r.Number("EVENT_AMOUNT") }, { "currency", r.Text("CURRENCY_CODE") },
                { "from", r.Text("FROM_LOCATION_CODE") }, { "fromName", r.Text("FROM_NAME") }, { "to", r.Text("TO_LOCATION_CODE") }, { "toName", r.Text("TO_NAME") },
                { "oldStatus", r.Text("OLD_STATUS_CODE") }, { "oldStatusName", r.Text("OLD_NAME") }, { "newStatus", r.Text("NEW_STATUS_CODE") }, { "newStatusName", r.Text("NEW_NAME") },
                { "handover", r.Text("HANDOVER_BY") }, { "receivedBy", r.Text("RECEIVED_BY") }, { "by", r.Text("CREATED_BY") }, { "bankRef", r.Text("BANK_REF") },
                { "reason", r.Text("REASON") }, { "notes", r.Text("NOTES") },
            };
            if (r.Number("MOVEMENT_GROUP_ID") is { } move)
            {
                e.Add("moveId", move);
                e.Add("moveRef", r.Text("MOVEMENT_REF"));
                e.Add("moveUrl", Url(141, "P141_MOVEMENT_GROUP_ID", move));
            }

            return e;
        }).ToList();

        return new Json
        {
            { "ok", true },
            { "listUrl", Url(130) },
            { "cheque", cheque },
            { "events", events },
            {
                "lists", new Json
                {
                    { "currencies", await ChqLists.PairsAsync(reader, ChqApexPageSql.CHQ_LIST_CURRENCIES, ct) },
                    { "entities", await ChqLists.PairsAsync(reader, ChqApexPageSql.CHQ_LIST_ENTITIES, ct) },
                    { "banks", await ChqLists.PairsAsync(reader, ChqApexPageSql.CHQ_LIST_BANKS, ct) },
                    { "statuses", await ChqLists.PairsAsync(reader, ChqApexPageSql.CHQ_STATUSES, ct) },
                    { "locations", await ChqLists.PairsAsync(reader, ChqApexPageSql.CHQ_LOCATIONS, ct) },
                    { "reasons", await ChqLists.PairsAsync(reader, ChqApexPageSql.CHQ_LIST_REASONS, ct) },
                    { "accounts", await ChqLists.PairsAsync(reader, ChqApexPageSql.CHQ_LIST_ACCOUNTS, ct) },
                    // FND_DRAWDOWN, FND_REPAYMENT_SCHEDULE and FND_REPAYMENT_PAYMENT (leasing) are not in Capitra:
                    // empty, as lst() wrote them when its query failed
                    { "drawdowns", new List<object?[]>() },
                    { "schedules", new List<object?[]>() },
                    { "payments", new List<object?[]>() },
                }
            },
        };
    }
}

/// <summary>Page 150 "Cheque Action Catalog", CHQ_ACTIONS: the actions with their rules.</summary>
public sealed class P0150Actions(IApexSqlReader reader) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 150;

    public string Name => "CHQ_ACTIONS";

    public string Permission => Permissions.Chq.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct) => new Json
    {
        { "ok", true },
        { "newUrl", Url(5) },
        {
            "rows", (await reader.QueryAsync(ChqApexPageSql.CHQ_ACTIONS, NoItems, ct)).Select(r => new Json
            {
                { "code", r.Text("ACTION_CODE") },
                { "name", r.Text("ACTION_NAME_EN") },
                { "nameAr", r.Text("ACTION_NAME_AR") },
                { "event", r.Text("EVENT_TYPE") },
                { "eventName", r.Text("EVENT_DESC_EN") },
                { "to", r.Text("TARGET_LOCATION_CODE") },
                { "fromStatus", r.Text("ALLOWED_FROM_STATUS") },
                { "fromLoc", r.Text("ALLOWED_FROM_LOCATION") },
                { "reqDep", Yn(r["REQUIRE_DEPOSIT_BATCH"]) },
                { "reqBank", Yn(r["REQUIRE_BANK_REF"]) },
                { "reqReason", Yn(r["REQUIRE_REJECTION_REASON"]) },
                { "reqNotes", Yn(r["REQUIRE_NOTES"]) },
                { "proc", r.Text("PACKAGE_PROCEDURE_NAME") },
                { "seq", r.Number("DISPLAY_SEQ") },
                { "active", Yn(r["ACTIVE_FLAG"]) },
                { "createdBy", r.Text("CREATED_BY") },
                { "created", Dmy(r["CREATED_DT"]) },
                { "updatedBy", r.Text("UPDATED_BY") },
                { "updated", Dmy(r["UPDATED_DT"]) },
                { "url", Url(5, "P5_ACTION_CODE", r.Text("ACTION_CODE")) },
            }).ToList()
        },
        { "locations", await ChqLists.PairsAsync(reader, ChqApexPageSql.CHQ_LOCATIONS_ANY, ct) },
        { "statuses", await ChqLists.PairsAsync(reader, ChqApexPageSql.CHQ_STATUSES_ANY, ct) },
    };
}

/// <summary>Page 140 "Cheques Movements", CHQ_MOVES: the movements (event groups), newest first (1000 at most).</summary>
public sealed class P0140Moves(IApexSqlReader reader) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 140;

    public string Name => "CHQ_MOVES";

    public string Permission => Permissions.Chq.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var rows = await reader.QueryAsync(ChqApexPageSql.CHQ_MOVES, NoItems, ct);
        return new Json
        {
            { "ok", true },
            { "walletUrl", Url(130) },
            {
                "rows", rows.Take(1000).Select(r => new Json
                {
                    { "id", r.Number("MOVEMENT_GROUP_ID") },
                    { "type", r.Text("EVENT_TYPE") },
                    { "typeName", r.Text("EVENT_DESC_EN") },
                    { "date", Dmy(r["EVENT_DT"]) },
                    { "effective", Dmy(r["EFFECTIVE_DT"]) },
                    { "to", r.Text("TO_LOCATION_CODE") },
                    { "toName", r.Text("LOCATION_NAME") },
                    { "ref", r.Text("MOVEMENT_REF") },
                    { "notes", r.Text("NOTES") },
                    { "depositRef", r.Text("BATCH_REF") },
                    { "cheques", r.Number("CHEQUES") },
                    { "amount", r.Number("AMOUNT") ?? 0 },
                    { "currency", r.Text("CURRENCY") },
                    { "createdBy", r.Text("CREATED_BY") },
                    { "created", DmyHm(r["CREATED_DT"]) },
                    { "url", Url(141, "P141_MOVEMENT_GROUP_ID", r["MOVEMENT_GROUP_ID"]) },
                }).ToList()
            },
            { "more", rows.Count > 1000 },
        };
    }
}

/// <summary>Page 140, CHQ_MV_LIST: the cheques that can still move, the actions, and the banks and accounts to deposit to.</summary>
public sealed class P0140MoveList(IApexSqlReader reader) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 140;

    public string Name => "CHQ_MV_LIST";

    public string Permission => Permissions.Chq.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var res = new Json
        {
            { "ok", true },
            {
                "rows", (await reader.QueryAsync(ChqApexPageSql.CHQ_MV_CHEQUES, NoItems, ct)).Select(r => new Json
                {
                    { "id", r.Number("CHEQUE_ID") },
                    { "no", r.Text("CHEQUE_NO") },
                    { "ref", r.Text("CHEQUE_REF") },
                    { "clientId", r.Text("CUSTOMER_ID") },
                    { "client", r.Text("CUSTOMER_NAME_EN") },
                    { "bank", r.Text("ISSUER_BANK_NAME") },
                    { "bankCode", r.Text("ISSUER_BANK_CODE") },
                    { "amount", r.Number("CHEQUE_AMOUNT") },
                    { "currency", r.Text("CURRENCY_CODE") },
                    { "loc", r.Text("CURRENT_LOCATION_CODE") },
                    { "locName", r.Text("LOCATION_NAME") },
                    { "heldBy", r.Text("CURRENT_CUSTODIAN_USER") },
                    { "since", Dmy(r["LAST_EVENT_DT"]) },
                    { "status", r.Text("STATUS_CODE") },
                    { "statusName", r.Text("STATUS_NAME") },
                    { "due", Dmy(r["DUE_DATE"]) },
                    { "dueIso", Iso(r["DUE_DATE"]) },
                    { "url", Url(131, "P131_CHEQUE_ID", r["CHEQUE_ID"]) },
                }).ToList()
            },
            {
                "actions", (await reader.QueryAsync(ChqApexPageSql.CHQ_MV_ACTIONS, NoItems, ct)).Select(a => new Json
                {
                    { "code", a.Text("ACTION_CODE") },
                    { "name", a.Text("ACTION_NAME_EN") },
                    { "event", a.Text("EVENT_TYPE") },
                    { "eventName", a.Text("EVENT_DESC_EN") },
                    { "to", a.Text("TARGET_LOCATION_CODE") },
                    { "toName", a.Text("LOCATION_NAME") },
                    { "fromStatus", a.Text("ALLOWED_FROM_STATUS") },
                    { "fromLoc", a.Text("ALLOWED_FROM_LOCATION") },
                    { "active", Yn(a["ACTIVE_FLAG"]) },
                }).ToList()
            },
            { "banks", await ChqLists.PairsAsync(reader, ChqApexPageSql.CHQ_MV_BANKS, ct) },
            { "accounts", await ChqLists.PairsAsync(reader, ChqApexPageSql.CHQ_MV_ACCOUNTS, ct, 3) },
        };
        return res;
    }
}

/// <summary>Page 141 "Cheque Movement Details", CHQ_MOVE: one movement with its cheques.</summary>
public sealed class P0141Move(IApexSqlReader reader) : IApexProcess
{
    public int App => ChqAjax.App;

    public int Page => 141;

    public string Name => "CHQ_MOVE";

    public string Permission => Permissions.Chq.View;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var id = string.IsNullOrEmpty(request.X01) ? request.Item("P141_MOVEMENT_GROUP_ID") : request.X01;
        var g = await reader.RowAsync(ChqApexPageSql.CHQ_MOVE, Id(Number(id)), ct);
        if (g is null)
            return new Json { { "ok", false }, { "message", "Movement " + id + " does not exist." } };
        decimal amount = 0;
        string? currency = null;
        var lines = (await reader.QueryAsync(ChqApexPageSql.CHQ_MOVE_LINES, Id(g["MOVEMENT_GROUP_ID"]), ct)).Select(r =>
        {
            amount += r.Number("AMOUNT") ?? 0;
            currency ??= r.Text("CURRENCY");
            return new Json
            {
                { "ref", r.Text("CHEQUE_REF") },
                { "no", r.Text("CHEQUE_NO") },
                { "customerId", r.Text("CUSTOMER_ID") },
                { "customer", r.Text("CUSTOMER_NAME_EN") },
                { "bank", r.Text("ISSUER_BANK_CODE") },
                { "due", Dmy(r["DUE_DATE"]) },
                { "amount", r.Number("AMOUNT") ?? 0 },
                { "currency", r.Text("CURRENCY") },
                { "from", r.Text("FROM_LOCATION_CODE") }, { "fromName", r.Text("FROM_NAME") },
                { "to", r.Text("TO_LOCATION_CODE") }, { "toName", r.Text("TO_NAME") },
                { "oldStatus", r.Text("OLD_STATUS_CODE") }, { "oldStatusName", r.Text("OLD_NAME") },
                { "newStatus", r.Text("NEW_STATUS_CODE") }, { "newStatusName", r.Text("NEW_NAME") },
                { "handover", r.Text("HANDOVER_BY") },
                { "receivedBy", r.Text("RECEIVED_BY") },
                { "receivedAt", DmyHm(r["RECEIVED_DT"]) },
                { "bankRef", r.Text("BANK_REF") },
                { "reason", r.Text("REASON") },
                { "url", Url(131, "P131_CHEQUE_ID", r["CHEQUE_ID"]) },
            };
        }).ToList();
        return new Json
        {
            { "ok", true },
            { "listUrl", Url(140) + "#HIST" },
            {
                "group", new Json
                {
                    { "id", g.Number("MOVEMENT_GROUP_ID") },
                    { "type", g.Text("EVENT_TYPE") },
                    { "typeName", g.Text("TYPE_NAME") },
                    { "to", g.Text("TO_LOCATION_CODE") },
                    { "toName", g.Text("LOCATION_NAME") },
                    { "ref", g.Text("MOVEMENT_REF") },
                    { "notes", g.Text("NOTES") },
                    { "date", DmyHm(g["EVENT_DT"]) },
                    { "effective", Dmy(g["EFFECTIVE_DT"]) },
                    { "effectiveIso", Iso(g["EFFECTIVE_DT"]) },
                    { "depositId", g.Number("DEPOSIT_BATCH_ID") },
                    { "depositRef", g.Text("DEPOSIT_REF") },
                    { "createdBy", g.Text("CREATED_BY") },
                    { "created", DmyHm(g["CREATED_DT"]) },
                }
            },
            { "lines", lines },
            { "amount", amount },
            { "currency", currency },
        };
    }
}

/// <summary>The lists the processes write: [{code, name}] and [[a, b(, c)]].</summary>
internal static class ChqLists
{
    public static async Task<List<Json>> CodeNameAsync(IApexSqlReader reader, string sql, CancellationToken ct) =>
        (await reader.QueryAsync(sql, NoItems, ct)).Select(r =>
        {
            var v = r.Values.ToArray();
            return new Json { { "code", ApexValue.Text(v[0]) }, { "name", ApexValue.Text(v[1]) } };
        }).ToList();

    public static async Task<List<object?[]>> PairsAsync(IApexSqlReader reader, string sql, CancellationToken ct, int columns = 2) =>
        (await reader.QueryAsync(sql, NoItems, ct)).Select(r => r.Values.Take(columns).Select(ApexValue.Text).Cast<object?>().ToArray()).ToList();
}
