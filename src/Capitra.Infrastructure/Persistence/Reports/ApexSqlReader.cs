using System.Data;
using System.Text.RegularExpressions;
using Capitra.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NpgsqlTypes;

namespace Capitra.Infrastructure.Persistence.Reports;

public sealed partial class ApexSqlReader(CapitraDbContext db) : IApexSqlReader
{
    [GeneratedRegex(@"@([A-Z][A-Z0-9_]*)")]
    private static partial Regex Parameter();

    public Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> QueryAsync(
        string sql, IReadOnlyDictionary<string, string?> items, CancellationToken cancellationToken) =>
        RunAsync(sql, sql, items, null, cancellationToken);

    /// <summary>One page sorted and counted by PostgreSQL: three small queries (columns, page, count) instead of reading every row.</summary>
    public async Task<ApexSqlPage> QueryPageAsync(
        string sql, IReadOnlyDictionary<string, string?> items, string? sortColumn, bool descending, int offset, int limit,
        CancellationToken cancellationToken)
    {
        var columns = new List<string>();
        await RunAsync(ApexSqlPaging.Columns(sql), sql, items, columns, cancellationToken);
        var rows = await RunAsync(ApexSqlPaging.Page(sql, columns, sortColumn, descending, offset, limit), sql, items, null, cancellationToken);
        var count = await RunAsync(ApexSqlPaging.Count(sql), sql, items, null, cancellationToken);
        var total = count.Count > 0 && count[0].Values.FirstOrDefault() is { } n ? Convert.ToInt64(n, System.Globalization.CultureInfo.InvariantCulture) : 0;
        return new ApexSqlPage(total, rows);
    }

    /// <summary>Runs <paramref name="command"/> with the parameters <paramref name="sql"/> names; fills <paramref name="columns"/> when given.</summary>
    private async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> RunAsync(
        string command, string sql, IReadOnlyDictionary<string, string?> items, List<string>? columns, CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        var opened = connection.State != ConnectionState.Open;
        if (opened)
            await connection.OpenAsync(cancellationToken);
        try
        {
            await using var cmd = new NpgsqlCommand(command, connection);
            if (db.Database.CurrentTransaction?.GetDbTransaction() is NpgsqlTransaction transaction)
                cmd.Transaction = transaction;

            // Untyped parameters: PostgreSQL types each one from where the query uses it, as APEX binds did.
            foreach (var name in Parameter().Matches(sql).Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal))
            {
                var value = items.TryGetValue(name, out var v) && !string.IsNullOrEmpty(v) ? v : null;
                cmd.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Unknown) { Value = (object?)value ?? DBNull.Value });
            }

            var rows = new List<IReadOnlyDictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
            if (columns is not null)
                for (var i = 0; i < reader.FieldCount; i++)
                    columns.Add(reader.GetName(i));
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new Dictionary<string, object?>(reader.FieldCount, StringComparer.Ordinal);
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    row[reader.GetName(i)] = await reader.IsDBNullAsync(i, cancellationToken)
                        ? null
                        : reader.GetDataTypeName(i) == "date" ? reader.GetFieldValue<DateOnly>(i) : reader.GetValue(i);
                }

                rows.Add(row);
            }

            return rows;
        }
        finally
        {
            if (opened)
                await connection.CloseAsync();
        }
    }
}
