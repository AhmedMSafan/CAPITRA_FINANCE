using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Capitra.Application.Abstractions;
using Capitra.Application.Modules.Apex;
using Capitra.Application.Modules.Gl.Journals;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Gl;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Adm.Products;

/// <summary>The source document of a product action's posting, as the GL journals show it.</summary>
public sealed record ProductActionDocument(string DocumentType, string DocumentId, string DocumentNo, DateOnly Date, string? CurrencyCode, string Description);

/// <summary>
/// Posts the GL entries of a product action (see <see cref="ProductActions"/>): the rule linked to the action in Admin Setup,
/// its GL lines filled with the action's figures, under the next CAP-{module} transaction set ID. It runs in the caller's
/// transaction and does not commit, so the action's own changes and its GL are saved together or not at all.
/// </summary>
public sealed class ProductActionPosting(ICapitraDbContext db, AdmProductEventRuleService rules, GlTranSetIds tranSetIds, GlPostingApi gl)
{
    private const string Company = "01";
    private const string Branch = "HQ";
    private const string Future = "000";

    /// <summary>Returns the transaction set ID posted; any problem raises an error, so the caller's transaction rolls back.</summary>
    public async Task<string> PostAsync(string actionCode, ProductActionDocument doc, Func<string, decimal?> amountOf, CancellationToken ct)
    {
        var def = ProductActions.Find(actionCode) ?? throw new InvalidOperationException($"Unknown product action {actionCode}.");
        var link = await (from a in db.Set<AdmProductAction>().AsNoTracking()
                          join p in db.Set<AdmProduct>() on a.ProductId equals p.ProductId
                          where a.ActionCode == def.ActionCode
                          select new { a.ProductEventRuleId, a.IsActive, p.ProductCode }).FirstOrDefaultAsync(ct);
        if (link?.ProductEventRuleId is not { } ruleId || !link.IsActive)
            throw Fail(-20983, $"No product rule is linked to the action \"{def.ActionNameEn}\". Link one in Admin Setup > Product Catalog > {def.ProductCode} > Actions.");

        var plan = await rules.PlanOfRuleAsync(ruleId, doc.Date, ct)
            ?? throw Fail(-20984, $"The rule linked to \"{def.ActionNameEn}\" on product {link.ProductCode} is inactive or not in effect on {D(doc.Date)}.");
        if (plan.Charges.Count > 0)
            throw Fail(-20985, $"Rule {plan.RuleCode} has charge components; the action \"{def.ActionNameEn}\" posts only the rule's own GL entries.");
        var unknown = plan.Lines.Select(x => x.AmountCode).Where(x => !def.AmountCodes.Contains(x)).Distinct().ToList();
        if (unknown.Count > 0)
            throw Fail(-20986, $"Rule {plan.RuleCode} posts {string.Join(", ", unknown)}, which the action \"{def.ActionNameEn}\" does not provide (it provides {string.Join(", ", def.AmountCodes)}).");

        var lines = ProductEventPosting.Build(plan, l => amountOf(l.AmountCode) ?? 0, null);
        if (lines.Count == 0)
            throw Fail(-20987, $"Rule {plan.RuleCode} gives no GL entries for this {doc.DocumentType.ToLowerInvariant().Replace('_', ' ')}.");

        var ev = await (from t in db.Set<GlAccountingEventType>().AsNoTracking()
                        join s in db.Set<GlJournalSource>() on t.SourceId equals s.SourceId
                        join c in db.Set<GlJournalCategory>() on t.DefaultCategoryId equals c.CategoryId into cs
                        from c in cs.DefaultIfEmpty()
                        where t.EventTypeCode == plan.EventCode
                        select new { s.SourceCode, CategoryCode = c == null ? null : c.CategoryCode }).FirstOrDefaultAsync(ct);
        if (ev?.CategoryCode is null)
            throw Fail(-20988, $"Business event {plan.EventCode} needs a journal source and a default journal category.");

        var ts = await tranSetIds.NextAsync(def.ModuleCode, doc.Date, ct);
        var res = await gl.PostTranSetAsync(Payload(def, doc, ts, ev.SourceCode, ev.CategoryCode, plan.RuleCode, lines), ct);
        var r = JsonDocument.Parse(res).RootElement;
        if (Str(r, "status") != "OK")
            throw Fail(-20989, "GL refused the entries: " + Str(r, "message") + " " + FirstError(r));
        return ts;
    }

    private static string Payload(ProductActionDefinition def, ProductActionDocument doc, string ts, string source, string category, string ruleCode,
        IReadOnlyList<ProductEventPostingLine> lines)
    {
        var o = new JsonObject
        {
            ["tran_set_id"] = ts,
            ["source"] = source,
            ["category"] = category,
            ["accounting_date"] = D(doc.Date),
        };
        if (!string.IsNullOrWhiteSpace(doc.CurrencyCode))
            o["currency"] = doc.CurrencyCode.Trim();
        o["batch_name"] = def.ActionNameEn + " - " + doc.DocumentNo;
        o["description"] = doc.Description;
        var js = new JsonArray();
        foreach (var j in lines.GroupBy(x => x.JournalNo).OrderBy(x => x.Key))
        {
            var ls = new JsonArray();
            foreach (var l in j)
            {
                ls.Add(new JsonObject
                {
                    ["company"] = Company,
                    ["branch"] = Branch,
                    ["cost_center"] = def.CostCenter,
                    ["account"] = l.Account,
                    ["business_line"] = def.BusinessLine,
                    ["future1"] = Future,
                    ["future2"] = Future,
                    ["dr"] = l.Dr,
                    ["cr"] = l.Cr,
                    ["description"] = l.Description,
                    ["reference_no"] = ts,
                });
            }

            js.Add(new JsonObject
            {
                ["name"] = doc.DocumentNo + " - " + def.ActionNameEn + (j.Key > 1 ? " " + j.Key.ToString(CultureInfo.InvariantCulture) : string.Empty),
                ["description"] = doc.Description + " (rule " + ruleCode + ")",
                ["reference_no"] = ts,
                ["source_document_type"] = doc.DocumentType,
                ["source_document_id"] = doc.DocumentId,
                ["source_document_no"] = doc.DocumentNo,
                ["source_transaction_date"] = D(doc.Date),
                ["lines"] = ls,
            });
        }

        o["journals"] = js;
        return o.ToJsonString();
    }

    private static string D(DateOnly d) => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    private static ApexProcessException Fail(int code, string message) => new("ORA" + code.ToString(CultureInfo.InvariantCulture) + ": " + message);

    private static string? Str(JsonElement r, string name) =>
        r.ValueKind == JsonValueKind.Object && r.TryGetProperty(name, out var v) && v.ValueKind != JsonValueKind.Null
            ? v.ValueKind == JsonValueKind.String ? v.GetString() : v.GetRawText()
            : null;

    private static string? FirstError(JsonElement r) =>
        r.TryGetProperty("errors", out var a) && a.ValueKind == JsonValueKind.Array && a.GetArrayLength() > 0 ? Str(a[0], "message") : null;
}
