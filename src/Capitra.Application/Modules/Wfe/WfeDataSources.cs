using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Capitra.Application.Common.Exceptions;
using Capitra.Domain.Modules.Adm;
using Capitra.Domain.Modules.Crm;
using Capitra.Domain.Modules.Wfe;
using Microsoft.EntityFrameworkCore;

namespace Capitra.Application.Modules.Wfe;

public sealed record WfeOptionDto(string Value, string Label);

/// <param name="BuiltIn">Read from the application's own tables; cannot be changed here.</param>
public sealed record WfeDataSourceDto(
    long? DataSourceId, string SourceCode, string SourceName, string? Description, bool IsActive, bool BuiltIn, int ItemCount,
    IReadOnlyList<WfeDataSourceItemDto> Items, int RowVersion);

public sealed record WfeDataSourceItemDto(string Value, string Label, bool IsActive);

public sealed record SaveWfeDataSourceRequest(
    string SourceCode, string SourceName, string? Description, bool IsActive, IReadOnlyList<WfeDataSourceItemDto>? Items, int RowVersion);

/// <summary>
/// Data sources (K2's SmartObjects, kept simple): the lists a workflow form offers in its drop-downs. Three are built in and read
/// the application (active users, clients, currencies); the others are lists the workflow administrator keeps here.
/// </summary>
public sealed partial class WfeDataSources(ICapitraDbContext db, IUserDirectory directory)
{
    public const string Users = "USERS";
    public const string Clients = "CLIENTS";
    public const string Currencies = "CURRENCIES";

    private const int MaxOptions = 2000;

    private static readonly (string Code, string Name, string Description)[] BuiltIns =
    [
        (Users, "Users", "Active users of the application: the user name is stored, the full name is shown."),
        (Clients, "Clients", "Active CRM clients: the client id is stored, the number and name are shown."),
        (Currencies, "Currencies", "Active currencies: the code is stored, the code and name are shown."),
    ];

    [GeneratedRegex("^[A-Z][A-Z0-9_]{1,49}$")]
    private static partial Regex CodePattern();

    public static bool IsBuiltIn(string code) => BuiltIns.Any(b => string.Equals(b.Code, code, StringComparison.OrdinalIgnoreCase));

    /// <summary>Every data source, built-in first, without their items.</summary>
    public async Task<IReadOnlyList<WfeDataSourceDto>> ListAsync(CancellationToken ct)
    {
        var own = await db.Set<WfeDataSource>().AsNoTracking().Include(x => x.Items).OrderBy(x => x.SourceName).ToListAsync(ct);
        return BuiltIns.Select(b => new WfeDataSourceDto(null, b.Code, b.Name, b.Description, true, true, 0, [], 0))
            .Concat(own.Select(x => Map(x, false)))
            .ToList();
    }

    public async Task<WfeDataSourceDto> GetAsync(string code, CancellationToken ct)
    {
        code = code.Trim().ToUpperInvariant();
        if (IsBuiltIn(code))
        {
            var b = BuiltIns.First(x => x.Code == code);
            var items = await OptionsAsync(code, ct);
            return new WfeDataSourceDto(null, b.Code, b.Name, b.Description, true, true, items.Count,
                items.Select(x => new WfeDataSourceItemDto(x.Value, x.Label, true)).ToList(), 0);
        }

        var row = await db.Set<WfeDataSource>().AsNoTracking().Include(x => x.Items).FirstOrDefaultAsync(x => x.SourceCode == code, ct)
            ?? throw new NotFoundException($"Data source {code} not found.");
        return Map(row, true);
    }

    /// <summary>The active values of a data source, as a drop-down offers them; empty when it does not exist or is off.</summary>
    public async Task<IReadOnlyList<WfeOptionDto>> OptionsAsync(string code, CancellationToken ct)
    {
        switch (code.Trim().ToUpperInvariant())
        {
            case Users:
                return (await directory.ActiveUsersAsync(ct)).OrderBy(x => x.FullName ?? x.UserName)
                    .Select(x => new WfeOptionDto(x.UserName, string.IsNullOrWhiteSpace(x.FullName) ? x.UserName : $"{x.FullName} ({x.UserName})"))
                    .ToList();
            case Clients:
                return await db.Set<CrmClient>().AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.ClientName).Take(MaxOptions)
                    .Select(x => new WfeOptionDto(x.ClientId.ToString(), x.ClientNumber + " · " + x.ClientName)).ToListAsync(ct);
            case Currencies:
                return await db.Set<AdmLkpCurrency>().AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.CurrencyCode)
                    .Select(x => new WfeOptionDto(x.CurrencyCode, x.CurrencyCode + " · " + x.CurrencyNameEn)).ToListAsync(ct);
            default:
                var upper = code.Trim().ToUpperInvariant();
                return await db.Set<WfeDataSourceItem>().AsNoTracking()
                    .Where(x => x.IsActive && db.Set<WfeDataSource>().Any(s => s.DataSourceId == x.DataSourceId && s.IsActive && s.SourceCode == upper))
                    .OrderBy(x => x.SortOrder).ThenBy(x => x.ItemLabel)
                    .Select(x => new WfeOptionDto(x.ItemValue, x.ItemLabel)).ToListAsync(ct);
        }
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken ct)
    {
        var upper = code.Trim().ToUpperInvariant();
        return IsBuiltIn(upper) || await db.Set<WfeDataSource>().AnyAsync(x => x.SourceCode == upper, ct);
    }

    public async Task<WfeDataSourceDto> SaveAsync(long? id, SaveWfeDataSourceRequest r, CancellationToken ct)
    {
        var code = (r.SourceCode ?? string.Empty).Trim().ToUpperInvariant();
        var name = (r.SourceName ?? string.Empty).Trim();
        var errors = new Dictionary<string, string[]>();
        if (!CodePattern().IsMatch(code))
            errors["sourceCode"] = ["Use 2 to 50 capital letters, digits or _, starting with a letter."];
        else if (IsBuiltIn(code))
            errors["sourceCode"] = [$"{code} is a built-in data source. Choose another code."];
        if (name.Length == 0)
            errors["sourceName"] = ["Give the data source a name."];

        var items = (r.Items ?? []).Select(x => new WfeDataSourceItemDto((x.Value ?? string.Empty).Trim(), (x.Label ?? string.Empty).Trim(), x.IsActive)).ToList();
        if (items.Any(x => x.Value.Length == 0))
            errors["items"] = ["Every item needs a value."];
        else if (items.GroupBy(x => x.Value, StringComparer.OrdinalIgnoreCase).FirstOrDefault(g => g.Count() > 1) is { } dup)
            errors["items"] = [$"The value \"{dup.Key}\" is in the list twice."];
        if (errors.Count > 0)
            throw new BusinessValidationException(errors);

        if (await db.Set<WfeDataSource>().AnyAsync(x => x.SourceCode == code && x.DataSourceId != (id ?? 0), ct))
            throw new ConflictException($"A data source with code {code} already exists.");

        WfeDataSource row;
        if (id is null)
        {
            row = new WfeDataSource();
            db.Add(row);
        }
        else
        {
            row = await db.Set<WfeDataSource>().Include(x => x.Items).FirstOrDefaultAsync(x => x.DataSourceId == id, ct)
                ?? throw new NotFoundException("Data source not found.");
            if (row.RowVersion != r.RowVersion)
                throw new ConflictException("The data source was changed by someone else. Reload it and try again.");
            if (!string.Equals(row.SourceCode, code, StringComparison.Ordinal) && await UsedByAsync(row.SourceCode, ct) is { } form)
                throw new BusinessRuleException($"Form {form} uses {row.SourceCode}, so its code cannot change.");
        }

        row.SourceCode = code;
        row.SourceName = name;
        row.Description = string.IsNullOrWhiteSpace(r.Description) ? null : r.Description.Trim();
        row.IsActive = r.IsActive;

        // items keep their rows when their value stays, so the list can be reordered and relabelled
        var byValue = row.Items.ToDictionary(x => x.ItemValue, StringComparer.OrdinalIgnoreCase);
        var keep = new HashSet<string>(items.Select(x => x.Value), StringComparer.OrdinalIgnoreCase);
        foreach (var gone in row.Items.Where(x => !keep.Contains(x.ItemValue)).ToList())
        {
            row.Items.Remove(gone);
            db.Remove(gone);
        }

        for (var i = 0; i < items.Count; i++)
        {
            var it = items[i];
            if (!byValue.TryGetValue(it.Value, out var item))
            {
                item = new WfeDataSourceItem();
                row.Items.Add(item);
            }

            item.ItemValue = it.Value;
            item.ItemLabel = it.Label.Length == 0 ? it.Value : it.Label;
            item.SortOrder = (i + 1) * 10;
            item.IsActive = it.IsActive;
        }

        await db.SaveChangesAsync(ct);
        return Map(row, true);
    }

    public async Task DeleteAsync(long id, CancellationToken ct)
    {
        var row = await db.Set<WfeDataSource>().FirstOrDefaultAsync(x => x.DataSourceId == id, ct)
            ?? throw new NotFoundException("Data source not found.");
        if (await UsedByAsync(row.SourceCode, ct) is { } form)
            throw new BusinessRuleException($"Form {form} uses {row.SourceCode}. Remove it from the form first, or switch the data source off.");
        db.Remove(row);
        await db.SaveChangesAsync(ct);
    }

    /// <summary>The code of a form whose layout uses this data source, or null.</summary>
    private async Task<string?> UsedByAsync(string code, CancellationToken ct)
    {
        var forms = await db.Set<WfeForm>().AsNoTracking().Select(x => new { x.FormCode, x.LayoutJson }).ToListAsync(ct);
        return forms.FirstOrDefault(f => WfeForms.ParseLayout(f.LayoutJson).Controls
            .Any(c => string.Equals(c.DataSource, code, StringComparison.OrdinalIgnoreCase)))?.FormCode;
    }

    private static WfeDataSourceDto Map(WfeDataSource x, bool withItems) => new(
        x.DataSourceId, x.SourceCode, x.SourceName, x.Description, x.IsActive, false, x.Items.Count,
        withItems ? x.Items.OrderBy(i => i.SortOrder).Select(i => new WfeDataSourceItemDto(i.ItemValue, i.ItemLabel, i.IsActive)).ToList() : [],
        x.RowVersion);
}
