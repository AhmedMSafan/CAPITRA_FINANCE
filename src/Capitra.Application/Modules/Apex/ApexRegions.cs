using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Apex;

/// <summary>How a column of an APEX table is shown and edited: as text, a number, a Y/N flag, a date or a timestamp.</summary>
public enum ApexColumnKind
{
    Text,
    Number,
    Flag,
    Date,
    DateTime,

    /// <summary>CREATED_BY/AT, UPDATED_BY/AT: shown, set by the audit interceptor, never typed.</summary>
    Audit,
}

/// <summary>One column of an APEX table (its Oracle name) and the entity property it is kept in.</summary>
public sealed record ApexColumn(string Name, string Property, ApexColumnKind Kind);

/// <summary>An APEX table (Oracle name) and the Capitra entity that holds it.</summary>
public sealed record ApexTable(string Name, Type Entity, string Pk, IReadOnlyList<ApexColumn> Columns);

/// <summary>
/// A wizard-built region of a copied APEX page: an interactive report on a table (optionally limited to the rows of
/// a parent item, e.g. SOURCE_ID = :P601_SOURCE_ID) or a form on one row of it.
/// </summary>
public sealed record ApexRegion(
    int App, int Page, string Key, ApexTable Table, string ViewPermission, string? EditPermission = null,
    string? ParentColumn = null, string? ParentItem = null);

/// <summary>A select list of a copied page: LOV on TABLE.DISPLAY returning the Return column.</summary>
public sealed record ApexLov(int App, int Page, string Item, ApexTable Table, string Display, string Return, string Permission);

/// <summary>
/// LOVS: the values of every select list of one page, as {ITEM: [[display, return], ...]}: table lists sorted by
/// display, SQL lists in their query's order. x01 holds the page items a SQL list binds, as {ITEM: value}.
/// </summary>
internal sealed class ApexLovProcess(
    int app, int page, IReadOnlyList<ApexLov> lovs, IReadOnlyList<ApexSqlLov> sqlLovs, ICapitraDbContext db, ApexSqlLovValues sqlValues) : IApexProcess
{
    public int App => app;

    public int Page => page;

    public string Name => "LOVS";

    public string Permission => lovs.Count > 0 ? lovs[0].Permission : sqlLovs[0].Permission;

    private static readonly MethodInfo Generic = typeof(ApexLovProcess).GetMethod(nameof(ValuesAsync), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public async Task<object> RunAsync(ApexRequest request, CancellationToken ct)
    {
        var res = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var lov in lovs)
            res[lov.Item] = await (Task<List<object?[]>>)Generic.MakeGenericMethod(lov.Table.Entity).Invoke(this, [lov, ct])!;
        var items = ApexItems.Read(request.X01);
        foreach (var lov in sqlLovs)
            res[lov.Item] = await sqlValues.ValuesAsync(lov, items, ct);
        return res;
    }

    private async Task<List<object?[]>> ValuesAsync<T>(ApexLov lov, CancellationToken ct)
        where T : class
    {
        PropertyInfo P(string column) => typeof(T).GetProperty(lov.Table.Columns.First(c => c.Name == column).Property)!;
        var display = P(lov.Display);
        var ret = P(lov.Return);
        return (await db.Set<T>().AsNoTracking().ToListAsync(ct))
            .Select(e => new[] { display.GetValue(e), ret.GetValue(e) })
            .Where(v => v[1] is not null)
            .OrderBy(v => v[0]?.ToString(), StringComparer.OrdinalIgnoreCase)
            .Select(v => new object?[] { v[0]?.ToString(), Convert.ToString(v[1], CultureInfo.InvariantCulture) })
            .ToList();
    }
}

/// <summary>
/// The processes behind wizard-built regions, which APEX ran itself: IR_&lt;key&gt; (search, sort and page the report),
/// FORM_LOAD_&lt;key&gt; (Initialize form) and FORM_DML_&lt;key&gt; (Process form: CREATE, SAVE or DELETE).
/// </summary>
internal sealed class ApexRegionProcess(ApexRegion region, string operation, ICapitraDbContext db, IApexFormHooks? hooks) : IApexProcess
{
    public int App => region.App;

    public int Page => region.Page;

    public string Name => operation + "_" + region.Key;

    public string Permission => operation == "FORM_DML" ? region.EditPermission! : region.ViewPermission;


    /// <summary>The processes of a region: a report and a form load always, the form DML only where the region may change data.</summary>
    public static IEnumerable<IApexProcess> For(ApexRegion region, ICapitraDbContext db, IApexFormHooks? hooks)
    {
        yield return new ApexRegionProcess(region, "IR", db, hooks);
        yield return new ApexRegionProcess(region, "FORM_LOAD", db, hooks);
        if (region.EditPermission is not null)
            yield return new ApexRegionProcess(region, "FORM_DML", db, hooks);
    }

    private static readonly MethodInfo Generic = typeof(ApexRegionProcess).GetMethod(nameof(RunTypedAsync), BindingFlags.NonPublic | BindingFlags.Instance)!;

    public Task<object> RunAsync(ApexRequest request, CancellationToken ct) =>
        (Task<object>)Generic.MakeGenericMethod(region.Table.Entity).Invoke(this, [request, ct])!;

    private async Task<object> RunTypedAsync<T>(ApexRequest request, CancellationToken ct)
        where T : class, new()
    {
        try
        {
            return operation switch
            {
                "IR" => await ReportAsync<T>(request, ct),
                "FORM_LOAD" => await LoadAsync<T>(request, ct),
                _ => await DmlAsync<T>(request, ct),
            };
        }
        catch (Exception ex) when (ex is ApexProcessException or ConflictException or FormatException or OverflowException)
        {
            db.DiscardChanges();
            return new { ok = false, message = ex.Message };
        }
    }

    private static PropertyInfo Prop<T>(string name) =>
        typeof(T).GetProperty(name) ?? throw new InvalidOperationException($"{typeof(T).Name} has no property {name}.");

    private ApexColumn Column(string name) =>
        region.Table.Columns.FirstOrDefault(c => c.Name == name) ?? throw new ApexProcessException("Unknown column " + name);

    /// <summary>e => e.Property == value, so the parent filter runs in the database.</summary>
    private IQueryable<T> WhereEquals<T>(IQueryable<T> query, string property, string? value)
    {
        var p = Prop<T>(property);
        var e = Expression.Parameter(typeof(T), "e");
        var body = Expression.Equal(Expression.Property(e, p), Expression.Constant(Parse(p.PropertyType, value), p.PropertyType));
        return query.Where(Expression.Lambda<Func<T, bool>>(body, e));
    }

    private object? Parse(Type type, string? value)
    {
        var t = Nullable.GetUnderlyingType(type) ?? type;
        if (string.IsNullOrWhiteSpace(value))
            return t == typeof(string) || Nullable.GetUnderlyingType(type) is not null ? null : t == typeof(bool) ? false : Activator.CreateInstance(t);
        var v = value.Trim();
        if (t == typeof(string))
            return value;
        if (t == typeof(bool))
            return v.Equals("Y", StringComparison.OrdinalIgnoreCase);
        if (t == typeof(DateOnly))
            return DateOnly.ParseExact(v[..Math.Min(10, v.Length)], "yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (t == typeof(DateTime))
            return ApexTime.ToStored(region.App, DateTime.Parse(v, CultureInfo.InvariantCulture));
        return Convert.ChangeType(decimal.Parse(v, NumberStyles.Number, CultureInfo.InvariantCulture), t, CultureInfo.InvariantCulture);
    }

    /// <summary>A value as a form item holds it: flags Y/N, dates yyyy-MM-dd, numbers in invariant format.</summary>
    private string? Edit(object? value) => value switch
    {
        null => null,
        bool b => b ? "Y" : "N",
        DateOnly d => d.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        DateTime t => ApexTime.Stamp(region.App, t, "yyyy-MM-dd HH:mm"),
        IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString(),
    };

    // ------------------------------------------------------------------------------------------------ interactive report
    /// <summary>x01: {search, sort, dir, first, rows, items}. Returns the matching rows of one page and how many match.</summary>
    private async Task<object> ReportAsync<T>(ApexRequest request, CancellationToken ct)
        where T : class
    {
        using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(request.X01) ? "{}" : request.X01);
        var o = doc.RootElement;
        var query = db.Set<T>().AsNoTracking();
        if (region.ParentColumn is not null)
        {
            var parent = o.TryGetProperty("items", out var items) && items.TryGetProperty(region.ParentItem!, out var pv) ? pv.ToString() : null;
            query = WhereEquals(query, Column(region.ParentColumn).Property, parent);
        }

        var props = region.Table.Columns.Select(c => (c.Name, Prop: Prop<T>(c.Property))).ToList();
        var pk = Prop<T>(Column(region.Table.Pk).Property);
        var rows = (await query.ToListAsync(ct)).Select(e => new ApexReport.Row(pk.GetValue(e), props.ToDictionary(p => p.Name, p => p.Prop.GetValue(e)))).ToList();
        return ApexReport.Page(rows, o, region.App);
    }

    // ------------------------------------------------------------------------------------------------ form
    private async Task<T?> FindAsync<T>(string? id, bool tracking, CancellationToken ct)
        where T : class
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;
        var query = WhereEquals(db.Set<T>(), Column(region.Table.Pk).Property, id);
        return await (tracking ? query : query.AsNoTracking()).FirstOrDefaultAsync(ct);
    }

    /// <summary>
    /// Initialize form: x01 is the primary key; no key (or a deleted row) gives empty items, filled by the page's
    /// "Set Defaults" (x02: the page items, as {ITEM: value}).
    /// </summary>
    private async Task<object> LoadAsync<T>(ApexRequest request, CancellationToken ct)
        where T : class
    {
        var row = await FindAsync<T>(request.X01, false, ct);
        var values = region.Table.Columns.ToDictionary(c => c.Name, c => row is null ? null : Edit(Prop<T>(c.Property).GetValue(row)));
        if (row is null && hooks is not null)
            await hooks.DefaultsAsync(values, ApexItems.Read(request.X02), ct);
        return new { ok = true, found = row is not null, values };
    }

    /// <summary>Process form: x01 CREATE, SAVE or DELETE; x02 the primary key; clob01 the items as {COLUMN: value}.</summary>
    private async Task<object> DmlAsync<T>(ApexRequest request, CancellationToken ct)
        where T : class, new()
    {
        var action = request.X01?.ToUpperInvariant() ?? string.Empty;
        var pkProp = Prop<T>(Column(region.Table.Pk).Property);

        // Process form and the page's processes after it are one transaction, as in APEX
        await using var tx = hooks is null ? null : await db.BeginTransactionAsync(ct);
        if (action == "DELETE")
        {
            var gone = await FindAsync<T>(request.X02, true, ct) ?? throw new ApexProcessException("Current version of data in database has changed since user initiated update process.");
            var before = region.Table.Columns.ToDictionary(c => c.Name, c => Edit(Prop<T>(c.Property).GetValue(gone)), StringComparer.OrdinalIgnoreCase);
            db.Remove(gone);
            await db.SaveChangesAsync(ct);
            if (hooks is not null)
            {
                await hooks.AfterDmlAsync(action, request.X02, before, ct);
                await db.SaveChangesAsync(ct);
                await tx!.CommitAsync(ct);
            }

            return new { ok = true, message = "Action Processed." };
        }

        T row;
        if (action == "CREATE")
        {
            row = new T();
            db.Add(row);
        }
        else if (action == "SAVE")
        {
            row = await FindAsync<T>(request.X02, true, ct) ?? throw new ApexProcessException("Current version of data in database has changed since user initiated update process.");
        }
        else
        {
            throw new ApexProcessException("Unknown request " + request.X01);
        }

        var sent = ApexItems.Read(request.Clob01);
        if (hooks is not null)
            await hooks.BeforeDmlAsync(action, sent, ct);
        foreach (var c in region.Table.Columns.Where(c => c.Kind != ApexColumnKind.Audit && c.Name != region.Table.Pk))
        {
            if (!sent.TryGetValue(c.Name, out var text))
                continue;
            var p = Prop<T>(c.Property);
            p.SetValue(row, Parse(p.PropertyType, text));
        }

        await db.SaveChangesAsync(ct);
        var id = Edit(pkProp.GetValue(row));
        if (hooks is not null)
        {
            var saved = region.Table.Columns.ToDictionary(c => c.Name, c => Edit(Prop<T>(c.Property).GetValue(row)), StringComparer.OrdinalIgnoreCase);
            foreach (var (k, v) in sent.Where(s => !saved.ContainsKey(s.Key)))
                saved[k] = v;                                   // items that are no column (e.g. an upload)
            await hooks.AfterDmlAsync(action, id, saved, ct);
            await db.SaveChangesAsync(ct);
            await tx!.CommitAsync(ct);
        }

        return new { ok = true, id, message = "Action Processed." };
    }
}

/// <summary>What an interactive report does with its rows: search, sort and one page of them.</summary>
public static class ApexReport
{
    /// <summary>One report row: its primary key (the default order) and its values by column.</summary>
    public sealed record Row(object? Pk, Dictionary<string, object?> Values);

    /// <summary>A value as the report shows it: flags Y/N, dates dd-Mon-yyyy, timestamps as the app shows them (ApexTime).</summary>
    public static object? Show(object? value, int app = 0) => value switch
    {
        null => null,
        bool b => b ? "Y" : "N",
        DateOnly d => d.ToString("dd-MMM-yyyy", CultureInfo.InvariantCulture),
        DateTime t => ApexTime.Stamp(app, t, "dd-MMM-yyyy HH:mm"),
        _ => value,
    };

    /// <summary>
    /// The report request {search, column, sort, dir, first, rows} applied to the rows:
    /// returns {ok, total, rows} with the values as the report shows them.
    /// </summary>
    public static object Page(List<Row> rows, JsonElement o, int app = 0)
    {
        string? Str(string key) => o.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;
        int Int(string key, int fallback) => o.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.Number ? v.GetInt32() : fallback;

        // the report search: every column, or the one chosen with the magnifier
        var search = Str("search")?.Trim();
        var column = Str("column");
        if (!string.IsNullOrEmpty(search))
        {
            bool Hit(object? v) => Show(v, app)?.ToString()?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;
            rows = rows.Where(r => string.IsNullOrEmpty(column) ? r.Values.Values.Any(Hit) : r.Values.TryGetValue(column, out var v) && Hit(v)).ToList();
        }

        var sort = Str("sort");
        if (sort is not null && rows.Count > 0 && rows[0].Values.ContainsKey(sort))
        {
            rows = (Str("dir") == "desc"
                ? rows.OrderByDescending(r => r.Values[sort], ValueComparer.Instance)
                : rows.OrderBy(r => r.Values[sort], ValueComparer.Instance)).ToList();
        }
        else
        {
            rows = rows.OrderBy(r => r.Pk, ValueComparer.Instance).ToList();
        }

        var first = Math.Max(1, Int("first", 1));
        var count = Math.Clamp(Int("rows", 15), 1, 1000);
        return new
        {
            ok = true,
            total = rows.Count,
            rows = rows.Skip(first - 1).Take(count).Select(r => r.Values.ToDictionary(v => v.Key, v => Show(v.Value, app))).ToList(),
        };
    }

    /// <summary>Nulls last, then numbers, dates and text in their natural order (as the database sorted).</summary>
    private sealed class ValueComparer : IComparer<object?>
    {
        public static readonly ValueComparer Instance = new();

        public int Compare(object? x, object? y) => (x, y) switch
        {
            (null, null) => 0,
            (null, _) => 1,
            (_, null) => -1,
            (string a, string b) => string.Compare(a, b, StringComparison.OrdinalIgnoreCase),
            (IComparable a, _) when x.GetType() == y.GetType() => a.CompareTo(y),
            _ => string.Compare(x.ToString(), y.ToString(), StringComparison.OrdinalIgnoreCase),
        };
    }
}
