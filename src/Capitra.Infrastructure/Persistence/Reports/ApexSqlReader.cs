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

    public async Task<IReadOnlyList<IReadOnlyDictionary<string, object?>>> QueryAsync(
        string sql, IReadOnlyDictionary<string, string?> items, CancellationToken cancellationToken)
    {
        var connection = (NpgsqlConnection)db.Database.GetDbConnection();
        var opened = connection.State != ConnectionState.Open;
        if (opened)
            await connection.OpenAsync(cancellationToken);
        try
        {
            await using var command = new NpgsqlCommand(sql, connection);
            if (db.Database.CurrentTransaction?.GetDbTransaction() is NpgsqlTransaction transaction)
                command.Transaction = transaction;

            // Untyped parameters: PostgreSQL types each one from where the query uses it, as APEX binds did.
            foreach (var name in Parameter().Matches(sql).Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal))
            {
                var value = items.TryGetValue(name, out var v) && !string.IsNullOrEmpty(v) ? v : null;
                command.Parameters.Add(new NpgsqlParameter(name, NpgsqlDbType.Unknown) { Value = (object?)value ?? DBNull.Value });
            }

            var rows = new List<IReadOnlyDictionary<string, object?>>();
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
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
