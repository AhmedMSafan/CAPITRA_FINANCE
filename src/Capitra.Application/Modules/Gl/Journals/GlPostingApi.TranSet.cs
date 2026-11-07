using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Capitra.Application.Modules.Apex;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Gl.Journals;

/// <summary>post_tran_set: the API other modules call to send the journals of one transaction set.</summary>
public sealed partial class GlPostingApi
{
    private static JsonObject LineError(int? journal, int? line, string message)
    {
        var o = new JsonObject();
        if (journal is not null)
            o["journal"] = journal;
        if (line is not null)
            o["line"] = line;
        o["message"] = message;
        return o;
    }

    /// <summary>journal_json: the journal's numbers, statuses, next step and totals.</summary>
    private async Task<JsonObject> JournalJsonAsync(long id, CancellationToken ct)
    {
        var o = new JsonObject();
        var h = await HeaderAsync(id, ct);
        if (h is null)
            return o;
        var lines = db.Set<GlJournalLine>().Where(l => l.JournalHeaderId == id && l.IsActive);
        o["journal_header_id"] = h.JournalHeaderId;
        o["journal_no"] = h.JournalNo;
        o["journal_name"] = h.JournalName;
        o["accounting_date"] = h.AccountingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        o["source_document_id"] = h.SourceDocumentId;
        o["journal_status"] = h.JournalStatus;
        o["approval_status"] = h.ApprovalStatus;
        o["posting_status"] = h.PostingStatus;
        o["next_step"] = h.PostingStatus == "POSTED" ? "POSTED"
            : h.JournalStatus == "CANCELLED" ? "CANCELLED"
            : h.ApprovalStatus is "SUBMITTED" or "IN_REVIEW" ? "WAITING_APPROVAL"
            : h.ApprovalStatus is "APPROVED" or "NOT_REQUIRED" ? "READY_TO_POST"
            : "DRAFT";
        o["line_count"] = await lines.CountAsync(ct);
        o["total_dr"] = await lines.SumAsync(l => l.AccountedDr, ct);
        o["total_cr"] = await lines.SumAsync(l => l.AccountedCr, ct);
        return o;
    }

    /// <summary>
    /// post_tran_set: checks every journal and line of the payload first (all errors come back together and nothing
    /// is created), then makes one batch for the TRAN_SET_ID and one journal per entry, and submits, approves or posts
    /// them as the source and ledger ask. Returns the JSON text {tran_set_id, status OK|ERROR, ...}. It does not commit.
    /// </summary>
    public async Task<string> PostTranSetAsync(string? payload, CancellationToken ct)
    {
        var tx = db.CurrentTransaction ?? throw new InvalidOperationException("The journal API needs an open transaction.");
        await db.SaveChangesAsync(ct);
        await tx.CreateSavepointAsync("gl_api_tran_set", ct);
        var res = new JsonObject();
        var errs = new JsonArray();
        try
        {
            JsonElement o;
            try
            {
                o = JsonDocument.Parse(payload ?? string.Empty).RootElement.Clone();
            }
            catch (JsonException)
            {
                throw new ApexProcessException("ORA-20950: The payload is not valid JSON.");
            }

            var tran = Cut(Jv(o, "tran_set_id"), 120);
            res["tran_set_id"] = tran;
            if (tran is null)
                throw new ApexProcessException("ORA-20950: tran_set_id is required.");
            var sourceCode = Jv(o, "source") ?? throw new ApexProcessException("ORA-20950: The journal source is required.");
            var src = await GetSourceAsync(sourceCode, ct);
            var manual = src.SourceCode == "MANUAL";
            var cat = await GetCategoryAsync(Jv(o, "category"), ct);
            var led = await GetLedgerAsync(Jv(o, "ledger"), null, ct);
            if (manual && led.AllowManual != "Y")
                throw new ApexProcessException($"ORA-20950: Ledger {led.LedgerCode} does not allow manual journals.");
            if (!manual && led.AllowSub != "Y")
                throw new ApexProcessException($"ORA-20950: Ledger {led.LedgerCode} does not accept journals from other modules.");
            var date = ToDate(Jv(o, "accounting_date"), "accounting_date") ?? throw new ApexProcessException("ORA-20950: accounting_date is required.");
            var cur = Up(Jv(o, "currency")) ?? led.Currency;
            var rate = ToNum(Jv(o, "conversion_rate"), "conversion_rate") ?? 1;
            if (rate <= 0)
                throw new ApexProcessException("ORA-20950: conversion_rate must be greater than zero.");
            if (cur == led.Currency && rate != 1)
                throw new ApexProcessException($"ORA-20950: conversion_rate must be 1 for the ledger currency {led.Currency}.");
            var action = Up(Jv(o, "action")) ?? "AUTO";
            if (action is not ("AUTO" or "SUBMIT" or "DRAFT"))
                throw new ApexProcessException("ORA-20950: action must be AUTO, SUBMIT or DRAFT.");

            var received = await db.Set<GlJournalBatch>().Where(b => b.SourceBatchRef == tran && b.SourceId == src.SourceId && b.BatchStatus != "CANCELLED")
                .Select(b => b.BatchNo).FirstOrDefaultAsync(ct);
            if (received is not null)
                throw new ApexProcessException($"ORA-20950: tran_set_id {tran} was already received from {src.SourceCode} (batch {received}). Use get_tran_set_status to read its status.");

            var lp = await LedgerPeriodForDateAsync(led.LedgerId, date, ct);
            await CheckPeriodAsync(lp, manual, ct);

            var journals = new List<JsonElement>();
            if (o.TryGetProperty("journals", out var js) && js.ValueKind == JsonValueKind.Array)
            {
                journals.AddRange(js.EnumerateArray());
            }
            else if (o.TryGetProperty("lines", out var ls) && ls.ValueKind == JsonValueKind.Array)
            {
                // simple form: the TRAN_SET_ID's lines; GL makes one journal per "group" (one journal when no group is given)
                var groups = new List<(string Key, JsonArray Lines)>();
                foreach (var line in ls.EnumerateArray())
                {
                    var k = (line.ValueKind == JsonValueKind.Object ? Jv(line, "group") : null) ?? "#";
                    var g = groups.FindIndex(x => x.Key == k);
                    if (g < 0)
                    {
                        groups.Add((k, []));
                        g = groups.Count - 1;
                    }

                    groups[g].Lines.Add(JsonNode.Parse(line.GetRawText()));
                }

                foreach (var (key, lines) in groups)
                {
                    var jo = new JsonObject();
                    if (key != "#")
                        jo["group"] = key;
                    jo["lines"] = lines;
                    journals.Add(JsonDocument.Parse(jo.ToJsonString()).RootElement.Clone());
                }
            }
            else
            {
                throw new ApexProcessException("ORA-20950: Send the entries as \"lines\" (or as \"journals\", each with its \"lines\").");
            }

            if (journals.Count == 0)
                throw new ApexProcessException("ORA-20950: journals is empty.");

            // 1. check every journal and line first, and collect all the errors
            var all = new List<List<Line>>();
            for (var j = 0; j < journals.Count; j++)
            {
                var lines = new List<Line>();
                decimal dr = 0, cr = 0;
                var jo = journals[j];
                if (jo.ValueKind != JsonValueKind.Object)
                {
                    errs.Add(LineError(j + 1, null, "The journal is not a JSON object."));
                }
                else
                {
                    if (Jv(jo, "accounting_date") is not null)
                    {
                        try
                        {
                            if (await LedgerPeriodForDateAsync(led.LedgerId, ToDate(Jv(jo, "accounting_date"), "accounting_date")!.Value, ct) != lp)
                                throw new ApexProcessException("ORA-20950: The journal accounting_date is in another period than the transaction set.");
                        }
                        catch (Exception ex) when (Handled(ex))
                        {
                            errs.Add(LineError(j + 1, null, CleanMessage(ex.Message)));
                        }
                    }

                    if (!jo.TryGetProperty("lines", out var larr) || larr.ValueKind != JsonValueKind.Array || larr.GetArrayLength() < 2)
                    {
                        errs.Add(LineError(j + 1, null, "A journal needs at least two lines."));
                    }
                    else
                    {
                        var i = 0;
                        foreach (var el in larr.EnumerateArray())
                        {
                            i++;
                            try
                            {
                                Line? line = null;
                                await InSavepointAsync("gl_api_line", async () =>
                                {
                                    line = ParseLine(el, led.Delimiter);
                                    await ResolveLineAsync(line, led.CoaCode, src.SourceCode, ct);
                                }, ct);
                                lines.Add(line!);
                                dr += Math.Round(line!.Dr * rate, 5, MidpointRounding.AwayFromZero);
                                cr += Math.Round(line.Cr * rate, 5, MidpointRounding.AwayFromZero);
                            }
                            catch (Exception ex) when (Handled(ex))
                            {
                                errs.Add(LineError(j + 1, i, CleanMessage(ex.Message)));
                            }
                        }

                        if (lines.Count == larr.GetArrayLength() && Math.Round(dr, 5, MidpointRounding.AwayFromZero) != Math.Round(cr, 5, MidpointRounding.AwayFromZero))
                            errs.Add(LineError(j + 1, null, $"The journal is not balanced: debit {OraNumber(dr)}, credit {OraNumber(cr)}."));
                    }
                }

                all.Add(lines);
            }

            if (errs.Count > 0)
            {
                await tx.RollbackToSavepointAsync("gl_api_tran_set", ct);
                db.DiscardChanges();
                res["status"] = "ERROR";
                res["message"] = $"Nothing was created. {errs.Count} error(s) found.";
                res["errors"] = errs;
                return res.ToJsonString();
            }

            // 2. one batch for the transaction set, one journal per group
            var batchId = await processing.CreateBatchAsync(led.LedgerId, lp, src.SourceCode, cat.CategoryCode,
                Cut(Jv(o, "batch_name") ?? src.SourceCode + " " + tran, 240)!, date, Cut(Jv(o, "description"), 4000), tran, ct);
            var ids = new List<long>();
            for (var j = 0; j < journals.Count; j++)
            {
                var jo = journals[j];
                var jdate = ToDate(Jv(jo, "accounting_date"), "accounting_date") ?? date;
                var name = Jv(jo, "name") ?? (Jv(o, "description") ?? src.SourceCode + " " + tran)
                    + (journals.Count > 1 ? " #" + (Jv(jo, "group") ?? (j + 1).ToString(CultureInfo.InvariantCulture)) : string.Empty);
                var id = await processing.CreateJournalAsync(batchId, Cut(name, 240)!, jdate, jdate, cur, rate,
                    Cut(Jv(jo, "description") ?? Jv(o, "description"), 4000), Cut(Jv(jo, "reference_no"), 120), !manual, ct,
                    conversionDate: cur != led.Currency ? jdate : null,
                    source: new GlJournalProcessing.SourceDocument(Cut(src.SourceCode, 30), Cut(Jv(jo, "source_document_type"), 60),
                        Cut(Jv(jo, "source_document_id") ?? tran, 120), Cut(Jv(jo, "source_document_no"), 120),
                        ToDate(Jv(jo, "source_transaction_date"), "source_transaction_date")));
                await AddLinesAsync(id, all[j], cur, rate, ct);
                ids.Add(id);
            }

            // 3. the next steps the configuration asks for
            var needsApproval = src.IsApprovalRequired || (manual && led.ManualAppr == "Y");
            var autoPost = src.IsAutoPostAllowed || (!manual && led.AutoPostSub);
            if (action != "DRAFT")
            {
                foreach (var id in ids)
                {
                    if (needsApproval || action == "SUBMIT")
                    {
                        await SubmitInternalAsync(id, "Submitted by the GL posting API for " + tran, ct);
                    }
                    else
                    {
                        await processing.ApproveJournalAsync(id, "Approval not required for source " + src.SourceCode, ct);
                        if (autoPost)
                            await PostInternalAsync(id, ct);
                    }
                }
            }

            var outJ = new JsonArray();
            foreach (var id in ids)
                outJ.Add(await JournalJsonAsync(id, ct));
            res["status"] = "OK";
            res["batch_id"] = batchId;
            res["batch_no"] = await db.Set<GlJournalBatch>().Where(b => b.BatchId == batchId).Select(b => b.BatchNo).SingleAsync(ct);
            res["ledger"] = led.LedgerCode;
            res["period_id"] = lp;
            res["approval_required"] = needsApproval;
            res["journals"] = outJ;
            await tx.ReleaseSavepointAsync("gl_api_tran_set", ct);
            return res.ToJsonString();
        }
        catch (Exception ex) when (Handled(ex))
        {
            var msg = CleanMessage(ex.Message);
            await tx.RollbackToSavepointAsync("gl_api_tran_set", ct);
            db.DiscardChanges();
            res["status"] = "ERROR";
            res["message"] = msg;
            if (errs.Count == 0)
                errs.Add(LineError(null, null, msg));
            res["errors"] = errs;
            return res.ToJsonString();
        }
    }

    /// <summary>to_char(n): a number as Oracle writes it by default.</summary>
    private static string OraNumber(decimal n)
    {
        var s = (n / 1.0000000000000000000000000000m).ToString(CultureInfo.InvariantCulture);
        return s.StartsWith("0.", StringComparison.Ordinal) ? s[1..] : s.StartsWith("-0.", StringComparison.Ordinal) ? "-" + s[2..] : s;
    }
}
